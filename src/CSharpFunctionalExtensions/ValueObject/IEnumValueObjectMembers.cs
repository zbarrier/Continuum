namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Supplies the members of an <see cref="EnumValueObject{TEnumeration}"/> or
    ///     <see cref="EnumValueObject{TEnumeration, TId}"/> without reflection.
    /// </summary>
    /// <typeparam name="TEnumeration">The concrete enumeration type.</typeparam>
    /// <remarks>
    ///     Implemented automatically by the source generator for every <see langword="partial"/> enumeration class.
    ///     The generated implementation returns the type's public static fields of type <typeparamref name="TEnumeration"/>
    ///     in declaration order. Implement it manually only if you need custom member discovery.
    /// </remarks>
    public interface IEnumValueObjectMembers<TEnumeration>
        where TEnumeration : IEnumValueObjectMembers<TEnumeration>
    {
        /// <summary>
        ///     Returns every member of the enumeration.
        /// </summary>
        static abstract TEnumeration[] GetMembers();
    }
}
