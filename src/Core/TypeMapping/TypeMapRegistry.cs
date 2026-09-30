namespace Continuum.TypeMapping;

/// <summary>
/// A type decorated with a type map attribute, recorded at startup by source-generated module initializers.
/// </summary>
/// <param name="Type">The mapped CLR type.</param>
/// <param name="TypeIdentifier">The stable identifier the type is persisted under.</param>
/// <param name="Kind">The single type map kind declared by the attribute.</param>
public readonly record struct TypeMapRegistration(Type Type, string TypeIdentifier, TypeMapKinds Kind);

/// <summary>
/// Process-wide registry of attribute-decorated types. Populated at module load by code emitted by the
/// Continuum.Core source generator, which replaces reflection-based assembly scanning and keeps type mapping
/// compatible with trimming and Native AOT.
/// </summary>
public static class TypeMapRegistry
{
    private static readonly object _lock = new();
    private static readonly Dictionary<Type, TypeMapRegistration> _registrations = new();

    /// <summary>
    /// Registers a type. Registering the same type again with the same identifier and kind is a no-op, which allows
    /// several assemblies to emit registrations for a shared referenced assembly.
    /// </summary>
    /// <exception cref="ArgumentException">The type was already registered with a different identifier or kind.</exception>
    public static void Register(Type type, string typeIdentifier, TypeMapKinds kind)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeIdentifier);

        var registration = new TypeMapRegistration(type, typeIdentifier, kind);
        lock (_lock)
        {
            if (_registrations.TryGetValue(type, out var existing))
            {
                if (existing != registration)
                {
                    throw new ArgumentException($"Type '{type.FullName}' has already been registered as '{existing.TypeIdentifier}' ({existing.Kind}).", nameof(type));
                }
                return;
            }
            _registrations.Add(type, registration);
        }
    }

    /// <summary>
    /// Returns a snapshot of all registrations whose kind is included in <paramref name="typeMapKinds"/>.
    /// </summary>
    public static IReadOnlyList<TypeMapRegistration> GetRegistrations(TypeMapKinds typeMapKinds)
    {
        lock (_lock)
        {
            var result = new List<TypeMapRegistration>(_registrations.Count);
            foreach (var registration in _registrations.Values)
            {
                if ((registration.Kind & typeMapKinds) != 0)
                {
                    result.Add(registration);
                }
            }
            return result;
        }
    }
}
