namespace Dostar.Cli.Tests.Unit;

public class ModuleNameValidationTests
{
    [Theory]
    [InlineData("Billing")]
    [InlineData("UserManagement")]
    [InlineData("OrderProcessing")]
    [InlineData("A")]
    [InlineData("Module123")]
    [InlineData("X1Y2Z3")]
    public void IsPascalCase_ValidNames_ReturnsTrue(string name)
    {
        name.IsPascalCase().ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("billing")]
    [InlineData("my-module")]
    [InlineData("my_module")]
    [InlineData("1Billing")]
    [InlineData("Billing!")]
    [InlineData(" Billing")]
    [InlineData("Billing Module")]
    public void IsPascalCase_InvalidNames_ReturnsFalse(string name)
    {
        name.IsPascalCase().ShouldBeFalse();
    }
}
