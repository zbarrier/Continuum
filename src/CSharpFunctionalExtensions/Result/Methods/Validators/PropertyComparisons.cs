#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
	/// <summary>
	///     Ensure value is equal to the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> Equal<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> EqualityComparer<T>.Default.Equals(value, otherValue)
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is not equal to the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> NotEqual<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> !EqualityComparer<T>.Default.Equals(value, otherValue)
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is greater than the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> GreaterThan<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) > 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is greater than or equal to the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> GreaterThanOrEqual<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) >= 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is less than the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> LessThan<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) < 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is less than or equal to the value of another property.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns both properties.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> LessThanOrEqual<T>(T value, T otherValue, PropertyPath path, string member, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) <= 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, path, member, path, otherMember);

	/// <summary>
	///     Ensure value is equal to the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> Equal<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> EqualityComparer<T>.Default.Equals(value, otherValue)
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.Equal, ValidatorErrorStrings.Equal, path, member, otherPath, otherMember);

	/// <summary>
	///     Ensure value is not equal to the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> NotEqual<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> !EqualityComparer<T>.Default.Equals(value, otherValue)
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.NotEqual, ValidatorErrorStrings.NotEqual, path, member, otherPath, otherMember);

	/// <summary>
	///     Ensure value is greater than the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> GreaterThan<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) > 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.GreaterThan, ValidatorErrorStrings.GreaterThan, path, member, otherPath, otherMember);

	/// <summary>
	///     Ensure value is greater than or equal to the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> GreaterThanOrEqual<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) >= 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.GreaterThanOrEqual, ValidatorErrorStrings.GreaterThanOrEqual, path, member, otherPath, otherMember);

	/// <summary>
	///     Ensure value is less than the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> LessThan<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) < 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.LessThan, ValidatorErrorStrings.LessThan, path, member, otherPath, otherMember);

	/// <summary>
	///     Ensure value is less than or equal to the value of a property on another object.
	/// </summary>
	/// <param name="value">The value being checked.</param>
	/// <param name="otherValue">The value of the other property.</param>
	/// <param name="path">The path to the object that owns the property being checked.</param>
	/// <param name="member">The name of the property being checked.</param>
	/// <param name="otherPath">The path to the object that owns the other property.</param>
	/// <param name="otherMember">The name of the other property.</param>
	public static Result<T> LessThanOrEqual<T>(T value, T otherValue, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
		=> Comparer<T>.Default.Compare(value, otherValue) <= 0
			? Result.Success(value)
			: PropertyComparisonFailure<T>(ValidationErrorCodes.LessThanOrEqual, ValidatorErrorStrings.LessThanOrEqual, path, member, otherPath, otherMember);

	static Result<T> PropertyComparisonFailure<T>(string code, string message, PropertyPath path, string member, PropertyPath otherPath, string otherMember)
	{
		var propertyName = path.Format(member);
		return Result.Failure<T>(new ValidationError(propertyName, code, message, propertyName, otherPath.Format(otherMember)));
	}
}
