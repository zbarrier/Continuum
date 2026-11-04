#nullable enable

using System.Globalization;
using System.Text;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     An immutable, chainable path to a property being validated (e.g. <c>Transfers[0].FromAccountId</c>).
///     Building a path only links segments; the path text is only created by <see cref="Format(string?)"/>,
///     which validators call on failure.
/// </summary>
/// <remarks>
///     This is a class rather than a struct because each segment must reference its parent, and a struct cannot
///     contain a field of its own type (boxing it would allocate anyway). <see cref="Root"/> is a shared instance,
///     so root-level validation allocates nothing; nested paths allocate one small, short-lived object per segment,
///     typically built once per object or collection item and shared across its validators.
/// </remarks>
public sealed class PropertyPath
{
	readonly PropertyPath? _parent;
	readonly string? _name;
	readonly int _index;
	readonly int _depth;

	/// <summary>
	///     The empty path. Formatting a member against it returns the member name unchanged.
	/// </summary>
	public static PropertyPath Root { get; } = new(null, null, 0);

	PropertyPath(PropertyPath? parent, string? name, int index)
	{
		_parent = parent;
		_name = name;
		_index = index;
		_depth = parent is null ? 0 : parent._depth + 1;
	}

	/// <summary>
	///     Returns a new path with a named segment appended (e.g. <c>Transfers</c>).
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
	public PropertyPath Append(string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);
		return new PropertyPath(this, name, 0);
	}

	/// <summary>
	///     Returns a new path with an index segment appended (e.g. <c>[0]</c>).
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
	public PropertyPath Index(int index)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		return new PropertyPath(this, null, index);
	}

	/// <summary>
	///     Formats the path followed by <paramref name="member"/> (e.g. <c>Transfers[0].FromAccountId</c>).
	///     When <paramref name="member"/> is null or empty, only the path is formatted.
	/// </summary>
	public string Format(string? member)
	{
		if (_depth == 0)
		{
			return member ?? string.Empty;
		}

		var segments = new PropertyPath[_depth];
		var current = this;
		for (int i = _depth - 1; i >= 0; i--)
		{
			segments[i] = current;
			current = current._parent!;
		}

		var builder = new StringBuilder();
		foreach (var segment in segments)
		{
			if (segment._name is null)
			{
				builder.Append('[').Append(segment._index.ToString(CultureInfo.InvariantCulture)).Append(']');
			}
			else
			{
				if (builder.Length > 0)
				{
					builder.Append('.');
				}
				builder.Append(segment._name);
			}
		}

		if (!string.IsNullOrEmpty(member))
		{
			builder.Append('.').Append(member);
		}

		return builder.ToString();
	}

	/// <summary>
	///     Formats the path without a trailing member.
	/// </summary>
	public override string ToString() => Format(null);
}
