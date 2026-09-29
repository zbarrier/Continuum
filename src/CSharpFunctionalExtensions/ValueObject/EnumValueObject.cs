using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Base class for smart enumerations identified by a struct <typeparamref name="TId"/> and a display name.
    /// </summary>
    /// <typeparam name="TEnumeration">The concrete enumeration type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <remarks>
    ///     Members are the public static fields of <typeparamref name="TEnumeration"/> declared on that type, discovered at
    ///     compile time by a source generator. The derived type must be declared <see langword="partial"/>.
    /// </remarks>
    public abstract class EnumValueObject<TEnumeration, TId> : ComparableValueObject
        where TEnumeration : EnumValueObject<TEnumeration, TId>, IEnumValueObjectMembers<TEnumeration>
        where TId : struct, IComparable
    {
        private int? _cachedHashCode;
        
        private static readonly TEnumeration[] Members = GetEnumerations();
        private static readonly Dictionary<TId, TEnumeration> EnumerationsById = Members.ToDictionary(e => e.Id);
        private static readonly Dictionary<string, TEnumeration> EnumerationsByName = Members.ToDictionary(e => e.Name);
        
        /// <summary>
        ///     Initializes a new enumeration member.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The unique display name.</param>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty, or whitespace.</exception>
        protected EnumValueObject(TId id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The name cannot be null or empty");
            }
            
            Id = id;
            Name = name;
        }

        /// <summary>
        ///     Gets the unique identifier.
        /// </summary>
        public TId Id { get; protected set; }
        
        /// <summary>
        ///     Gets the unique display name.
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        ///     Determines whether the member's <see cref="Id"/> equals <paramref name="b"/>.
        /// </summary>
        public static bool operator ==(EnumValueObject<TEnumeration, TId> a, TId b)
        {
            if (a is null)
            {
                return false;
            }
            
            return a.Id.Equals(b);
        }

        /// <summary>
        ///     Determines whether the member's <see cref="Id"/> does not equal <paramref name="b"/>.
        /// </summary>
        public static bool operator !=(EnumValueObject<TEnumeration, TId> a, TId b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Determines whether <paramref name="a"/> equals the member's <see cref="Id"/>.
        /// </summary>
        public static bool operator ==(TId a, EnumValueObject<TEnumeration, TId> b)
        {
            return b == a;
        }

        /// <summary>
        ///     Determines whether <paramref name="a"/> does not equal the member's <see cref="Id"/>.
        /// </summary>
        public static bool operator !=(TId a, EnumValueObject<TEnumeration, TId> b)
        {
            return !(b == a);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (GetUnproxiedType(this) != GetUnproxiedType(obj))
                return false;

            var enumValueObject = (EnumValueObject<TEnumeration, TId>)obj;

            return EqualityComparer<TId>.Default.Equals(Id, enumValueObject.Id);
        }
        
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = ComputeHashCode(GetEqualityComponents());
            }

            return _cachedHashCode.Value;
        }
        
        /// <summary>
        ///     Finds the member with the given identifier.
        /// </summary>
        /// <param name="id">The identifier to look up.</param>
        /// <returns>The matching member, or an empty <see cref="Maybe{T}"/>.</returns>
        public static Maybe<TEnumeration> FromId(TId id)
        {
            return EnumerationsById.ContainsKey(id)
                ? EnumerationsById[id]
                : null;
        }
        
        /// <summary>
        ///     Finds the member with the given name (case-sensitive).
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The matching member, or an empty <see cref="Maybe{T}"/>.</returns>
        public static Maybe<TEnumeration> FromName(string name)
        {
            return EnumerationsByName.ContainsKey(name)
                ? EnumerationsByName[name]
                : null;
        }

        /// <summary>
        ///     Gets all members of the enumeration.
        /// </summary>
        public static readonly IReadOnlyCollection<TEnumeration> All = Array.AsReadOnly(Members);

        /// <summary>
        ///     Determines whether a member with the given name exists.
        /// </summary>
        /// <param name="possibleName">The name to check.</param>
        public static bool Is(string possibleName) => possibleName is not null && EnumerationsByName.ContainsKey(possibleName);

        /// <summary>
        ///     Determines whether a member with the given identifier exists.
        /// </summary>
        /// <param name="possibleId">The identifier to check.</param>
        public static bool Is(TId possibleId) => EnumerationsById.ContainsKey(possibleId);

        /// <summary>
        ///     Returns <see cref="Name"/>.
        /// </summary>
        public override string ToString() => Name;

        /// <inheritdoc/>
        protected override IEnumerable<IComparable> GetComparableEqualityComponents()
        {
            yield return Id;
        }

        private static TEnumeration[] GetEnumerations() => TEnumeration.GetMembers();
    }

    /// <summary>
    ///     Base class for smart enumerations identified by a string key.
    /// </summary>
    /// <typeparam name="TEnumeration">The concrete enumeration type.</typeparam>
    /// <remarks>
    ///     Members are the public static fields of <typeparamref name="TEnumeration"/> declared on that type, discovered at
    ///     compile time by a source generator. The derived type must be declared <see langword="partial"/>.
    /// </remarks>
    public abstract class EnumValueObject<TEnumeration> : ComparableValueObject
        where TEnumeration : EnumValueObject<TEnumeration>, IEnumValueObjectMembers<TEnumeration>
    {
        private int? _cachedHashCode;
        
        private static readonly TEnumeration[] Members = GetEnumerations();
        private static readonly Dictionary<string, TEnumeration> Enumerations = Members.ToDictionary(e => e.Id);
        
        /// <summary>
        ///     Initializes a new enumeration member.
        /// </summary>
        /// <param name="id">The unique string key.</param>
        /// <exception cref="ArgumentException"><paramref name="id"/> is null, empty, or whitespace.</exception>
        protected EnumValueObject(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The enum key cannot be null or empty");
            }

            Id = id;
        }

        /// <summary>
        ///     Gets all members of the enumeration.
        /// </summary>
        public static readonly IReadOnlyCollection<TEnumeration> All = Array.AsReadOnly(Members);

        /// <summary>
        ///     Gets the unique string key.
        /// </summary>
        public virtual string Id { get; protected set; }
        
        /// <summary>
        ///     Determines whether the member's <see cref="Id"/> equals <paramref name="b"/>. Two <see langword="null"/> operands are equal.
        /// </summary>
        public static bool operator ==(EnumValueObject<TEnumeration> a, string b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Id.Equals(b);
        }

        /// <summary>
        ///     Determines whether the member's <see cref="Id"/> does not equal <paramref name="b"/>.
        /// </summary>
        public static bool operator !=(EnumValueObject<TEnumeration> a, string b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Determines whether <paramref name="a"/> equals the member's <see cref="Id"/>.
        /// </summary>
        public static bool operator ==(string a, EnumValueObject<TEnumeration> b)
        {
            return b == a;
        }

        /// <summary>
        ///     Determines whether <paramref name="a"/> does not equal the member's <see cref="Id"/>.
        /// </summary>
        public static bool operator !=(string a, EnumValueObject<TEnumeration> b)
        {
            return !(b == a);
        }
        
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (GetUnproxiedType(this) != GetUnproxiedType(obj))
                return false;

            var enumValueObject = (EnumValueObject<TEnumeration>)obj;

            return string.Equals(Id, enumValueObject.Id, StringComparison.Ordinal);
        }
        
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = ComputeHashCode(GetEqualityComponents());
            }

            return _cachedHashCode.Value;
        }
        
        /// <summary>
        ///     Finds the member with the given key.
        /// </summary>
        /// <param name="id">The key to look up.</param>
        /// <returns>The matching member, or an empty <see cref="Maybe{T}"/>.</returns>
        public static Maybe<TEnumeration> FromId(string id)
        {
            return Enumerations.ContainsKey(id)
                ? Enumerations[id]
                : null;
        }

        /// <summary>
        ///     Determines whether a member with the given key exists.
        /// </summary>
        /// <param name="possibleId">The key to check.</param>
        public static bool Is(string possibleId) => possibleId is not null && Enumerations.ContainsKey(possibleId);

        /// <summary>
        ///     Returns <see cref="Id"/>.
        /// </summary>
        public override string ToString() => Id;

        /// <inheritdoc/>
        protected override IEnumerable<IComparable> GetComparableEqualityComponents()
        {
            yield return Id;
        }
        
        private static TEnumeration[] GetEnumerations() => TEnumeration.GetMembers();
    }
}
