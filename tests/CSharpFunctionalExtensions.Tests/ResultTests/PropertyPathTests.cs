namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests;

public class PropertyPathTests
{
	[Fact]
	public void Format_Root_ReturnsMember()
	{
		Assert.Equal("Amount", PropertyPath.Root.Format("Amount"));
	}

	[Fact]
	public void Format_RootWithoutMember_ReturnsEmpty()
	{
		Assert.Equal(string.Empty, PropertyPath.Root.Format(null));
		Assert.Equal(string.Empty, PropertyPath.Root.ToString());
	}

	[Fact]
	public void Format_SingleSegment_JoinsWithDot()
	{
		Assert.Equal("Transfer.Amount", PropertyPath.Root.Append("Transfer").Format("Amount"));
	}

	[Fact]
	public void Format_IndexedSegment_UsesBrackets()
	{
		var path = PropertyPath.Root.Append("Transfers").Index(3);

		Assert.Equal("Transfers[3].FromAccountId", path.Format("FromAccountId"));
	}

	[Fact]
	public void Format_MultipleLevels_FormatsFullPath()
	{
		var path = PropertyPath.Root.Append("Batch").Append("Transfers").Index(0).Append("From").Index(12);

		Assert.Equal("Batch.Transfers[0].From[12].Value", path.Format("Value"));
	}

	[Fact]
	public void Format_RootIndex_HasNoLeadingDot()
	{
		Assert.Equal("[2].Amount", PropertyPath.Root.Index(2).Format("Amount"));
	}

	[Fact]
	public void Format_WithoutMember_FormatsPathOnly()
	{
		var path = PropertyPath.Root.Append("Transfers").Index(1);

		Assert.Equal("Transfers[1]", path.Format(null));
		Assert.Equal("Transfers[1]", path.Format(string.Empty));
		Assert.Equal("Transfers[1]", path.ToString());
	}

	[Fact]
	public void Append_DoesNotModifyParent()
	{
		var parent = PropertyPath.Root.Append("Transfers");
		_ = parent.Index(0).Append("Amount");

		Assert.Equal("Transfers.Amount", parent.Format("Amount"));
	}

	[Fact]
	public void Append_NullOrEmpty_Throws()
	{
		Assert.ThrowsAny<ArgumentException>(() => PropertyPath.Root.Append(null!));
		Assert.ThrowsAny<ArgumentException>(() => PropertyPath.Root.Append(string.Empty));
	}

	[Fact]
	public void Index_Negative_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => PropertyPath.Root.Index(-1));
	}
}
