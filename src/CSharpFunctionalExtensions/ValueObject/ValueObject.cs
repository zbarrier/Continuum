using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Base class for value objects whose equality is determined by their components rather than by identity.
    /// </summary>
    /// <remarks>
    ///     ORM proxy types (EF Core/Castle and NHibernate) are unwrapped so a proxy equals its underlying type.
    /// </remarks>
    [Serializable]
    public abstract class ValueObject
    {
        private int? _cachedHashCode;

        /// <summary>
        ///     Returns the components that participate in equality and hash code calculation, in a stable order.
        /// </summary>
        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (GetUnproxiedType(this) != GetUnproxiedType(obj))
                return false;

            var valueObject = (ValueObject)obj;

            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     The hash code is computed once and cached; equality components must therefore be immutable.
        /// </remarks>
        public override int GetHashCode()
        {
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = ComputeHashCode(GetEqualityComponents());
            }

            return _cachedHashCode.Value;
        }

        internal static int ComputeHashCode<TComponent>(IEnumerable<TComponent> components)
        {
            var hash = 1;

            foreach (var component in components)
            {
                unchecked
                {
                    hash = hash * 23 + (component?.GetHashCode() ?? 0);
                }
            }

            return hash;
        }

        /// <summary>
        ///     Determines whether two value objects are equal. Two
        /// </summary>
        public static bool operator ==(ValueObject a, ValueObject b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        /// <summary>
        ///     Determines whether two value objects are not equal.
        /// </summary>
        public static bool operator !=(ValueObject a, ValueObject b)
        {
            return !(a == b);
        }

        private static readonly ConcurrentDictionary<Type, Type> UnproxiedTypes = new();

        internal static Type GetUnproxiedType(object obj)
        {
            return UnproxiedTypes.GetOrAdd(obj.GetType(), static type => ResolveUnproxiedType(type));
        }

        private static Type ResolveUnproxiedType(Type type)
        {
            const string EFCoreProxyPrefix = "Castle.Proxies.";
            const string NHibernateProxyPostfix = "Proxy";

            string typeString = type.ToString();

            if (typeString.Contains(EFCoreProxyPrefix) || typeString.EndsWith(NHibernateProxyPostfix))
                return type.BaseType ?? type;

            return type;
        }
    }
}
