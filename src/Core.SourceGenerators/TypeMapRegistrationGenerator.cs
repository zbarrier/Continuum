using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Continuum.Core.SourceGenerators;

/// <summary>
/// Emits a module initializer that registers every class decorated with a Continuum type map attribute
/// with <c>Continuum.TypeMapping.TypeMapRegistry</c>, so type mapping works without reflection scanning.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class TypeMapRegistrationGenerator : IIncrementalGenerator
{
    private const string CoreAssemblyName = "Continuum.Core";
    private const string TypeMappingNamespace = "Continuum.TypeMapping";
    private const string RegistryTypeName = "Continuum.TypeMapping.TypeMapRegistry";

    private static readonly Dictionary<string, int> KnownAttributeKinds = new()
    {
        ["CommandTypeAttribute"] = 1,
        ["DomainEventTypeAttribute"] = 2,
        ["IntegrationEventTypeAttribute"] = 4,
        ["SnapshotTypeAttribute"] = 8,
        ["MetadataTypeAttribute"] = 16,
    };

    private readonly record struct Registration(string TypeName, string Identifier, int Kind);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // ForAttributeWithMetadataName uses the compiler's attribute index, so only classes carrying one of the
        // known attributes reach the semantic transform.
        var local = CollectLocal(context, "CommandTypeAttribute")
            .Combine(CollectLocal(context, "DomainEventTypeAttribute"))
            .Combine(CollectLocal(context, "IntegrationEventTypeAttribute"))
            .Combine(CollectLocal(context, "SnapshotTypeAttribute"))
            .Combine(CollectLocal(context, "MetadataTypeAttribute"))
            .Select(static (x, _) => new EquatableArray<Registration>(
                x.Left.Left.Left.Left.AddRange(x.Left.Left.Left.Right).AddRange(x.Left.Left.Right).AddRange(x.Left.Right).AddRange(x.Right)));

        // Runs on every compilation change, but each metadata reference is scanned only once (see ReferenceCache)
        // and the equatable result stops downstream work when nothing changed.
        var referenced = context.CompilationProvider
            .Select(static (compilation, ct) => GetReferencedRegistrations(compilation, ct));

        var hasRegistry = context.CompilationProvider
            .Select(static (compilation, _) => compilation.GetTypeByMetadataName(RegistryTypeName) is { } registry
                && !SymbolEqualityComparer.Default.Equals(registry.ContainingAssembly, compilation.Assembly));

        var combined = local.Combine(referenced).Combine(hasRegistry);

        context.RegisterSourceOutput(combined, static (spc, input) =>
        {
            var ((localRegistrations, referencedRegistrations), registryAvailable) = input;
            if (!registryAvailable)
            {
                return;
            }

            var all = localRegistrations.Concat(referencedRegistrations)
                .Distinct()
                .OrderBy(r => r.TypeName, System.StringComparer.Ordinal)
                .ToList();
            if (all.Count == 0)
            {
                return;
            }

            spc.AddSource("TypeMapRegistrations.g.cs", SourceText.From(Render(all), Encoding.UTF8));
        });
    }

    private static IncrementalValueProvider<ImmutableArray<Registration>> CollectLocal(IncrementalGeneratorInitializationContext context, string attributeName)
        => context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{TypeMappingNamespace}.{attributeName}",
                static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (ctx, _) => ctx.TargetSymbol is INamedTypeSymbol symbol
                    && TryGetRegistration(symbol, requirePublic: false, out var registration)
                        ? registration
                        : (Registration?)null)
            .Where(static r => r is not null)
            .Select(static (r, _) => r!.Value)
            .Collect();

    // Metadata references are immutable and reused across compilations in the IDE, so their registrations are
    // computed once per reference instead of walking every referenced type on each keystroke.
    private static readonly ConditionalWeakTable<MetadataReference, RegistrationList> ReferenceCache = new();

    private sealed class RegistrationList(ImmutableArray<Registration> items)
    {
        public ImmutableArray<Registration> Items { get; } = items;
    }

    private static EquatableArray<Registration> GetReferencedRegistrations(Compilation compilation, CancellationToken cancellationToken)
    {
        var builder = ImmutableArray.CreateBuilder<Registration>();

        foreach (var reference in compilation.References)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ReferenceCache.TryGetValue(reference, out var cached))
            {
                var items = compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly
                    && assembly.Name != CoreAssemblyName
                    && ReferencesCore(assembly)
                        ? CollectFromAssembly(assembly)
                        : ImmutableArray<Registration>.Empty;
                cached = ReferenceCache.GetValue(reference, _ => new RegistrationList(items));
            }

            builder.AddRange(cached.Items);
        }

        return new EquatableArray<Registration>(builder.ToImmutable());
    }

    private static ImmutableArray<Registration> CollectFromAssembly(IAssemblySymbol assembly)
    {
        var builder = ImmutableArray.CreateBuilder<Registration>();
        CollectFromNamespace(assembly.GlobalNamespace, builder);
        return builder.ToImmutable();
    }

    private static bool ReferencesCore(IAssemblySymbol assembly)
        => assembly.Modules.Any(m => m.ReferencedAssemblies.Any(r => r.Name == CoreAssemblyName));

    private static void CollectFromNamespace(INamespaceSymbol ns, ImmutableArray<Registration>.Builder builder)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                CollectFromNamespace(childNs, builder);
            }
            else if (member is INamedTypeSymbol type)
            {
                CollectFromType(type, builder);
            }
        }
    }

    private static void CollectFromType(INamedTypeSymbol type, ImmutableArray<Registration>.Builder builder)
    {
        if (type.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        if (TryGetRegistration(type, requirePublic: true, out var registration))
        {
            builder.Add(registration);
        }

        foreach (var nested in type.GetTypeMembers())
        {
            CollectFromType(nested, builder);
        }
    }

    private static bool TryGetRegistration(INamedTypeSymbol type, bool requirePublic, out Registration registration)
    {
        registration = default;

        if (type.TypeKind != TypeKind.Class || type.IsGenericType || (type.IsAbstract && type.IsSealed))
        {
            return false;
        }

        if (requirePublic && !IsPubliclyAccessible(type))
        {
            return false;
        }

        foreach (var attribute in type.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null
                || attributeClass.ContainingNamespace?.ToDisplayString() != TypeMappingNamespace
                || !KnownAttributeKinds.TryGetValue(attributeClass.Name, out var kind)
                || attribute.ConstructorArguments.Length != 1
                || attribute.ConstructorArguments[0].Value is not string identifier
                || string.IsNullOrWhiteSpace(identifier))
            {
                continue;
            }

            registration = new Registration(
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                identifier,
                kind);
            return true;
        }

        return false;
    }

    private static bool IsPubliclyAccessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }

        return true;
    }

    private static string Render(List<Registration> registrations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Continuum.TypeMapping.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCode(\"Continuum.Core.SourceGenerators\", \"1.0.0\")]");
        sb.AppendLine("    internal static class TypeMapRegistrations");
        sb.AppendLine("    {");
        sb.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        sb.AppendLine("        internal static void Register()");
        sb.AppendLine("        {");

        foreach (var r in registrations)
        {
            sb.Append("            global::Continuum.TypeMapping.TypeMapRegistry.Register(typeof(")
                .Append(r.TypeName)
                .Append("), ")
                .Append(SymbolDisplay.FormatLiteral(r.Identifier, quote: true))
                .Append(", (global::Continuum.TypeMapping.TypeMapKinds)")
                .Append(r.Kind)
                .AppendLine(");");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
