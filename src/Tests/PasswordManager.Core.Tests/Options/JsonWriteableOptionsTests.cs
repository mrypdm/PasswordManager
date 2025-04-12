using PasswordManager.Core.Options;

namespace PasswordManager.Core.Tests.Options;

/// <summary>
/// Tests for <see cref="JsonWriteableOptions{TOptions}"/>
/// </summary>
public class JsonWriteableOptionsTests
{
    private const int DefaultNumber = 123;

    public class ExampleOptions
    {
        public int Number { get; set; } = DefaultNumber;
    }

    [Test]
    public void WithoutFile_ShouldCreateFile()
    {
        // arrange
        using var file = new TempFile("not-exist");

        // act
        var settings = JsonWriteableOptions.Create<ExampleOptions>(file.FilePath);

        // assert
        var content = file.Read().Replace("\r\n", "\n");
        Assert.Multiple(() =>
        {
            Assert.That(settings.Value.Number, Is.EqualTo(DefaultNumber));
            Assert.That(file.Exists, Is.True);
            Assert.That(content, Is.EqualTo(
                $$"""
                {
                  "Number": {{DefaultNumber}}
                }
                """));
        });
    }

    [Test]
    public void BadJson_ShouldReturnNewValue()
    {
        // arrange
        using var file = new TempFile();
        file.Write(
            """
            {
              "Number": 444
            """);

        // act
        var settings = JsonWriteableOptions.Create<ExampleOptions>(file.FilePath);

        // assert
        Assert.That(settings.Value.Number, Is.EqualTo(DefaultNumber));
    }

    [Test]
    public void WithFile_ShouldReadFromFile()
    {
        // arrange
        const int number = 321;
        using var file = new TempFile();
        file.Write(
            $$"""
            {
              "Number": {{number}}
            }
            """);

        // act
        var settings = JsonWriteableOptions.Create<ExampleOptions>(file.FilePath);

        // assert
        Assert.That(settings.Value.Number, Is.EqualTo(number));
    }

    [Test]
    public void Update_ShouldStoreNewValue()
    {
        // arrange
        const int newNumber = 234;
        using var file = new TempFile();

        // act
        var settings = JsonWriteableOptions.Create<ExampleOptions>(file.FilePath);
        settings.Update(o => o.Number = newNumber);

        // assert
        var content = file.Read().Replace("\r\n", "\n");
        Assert.Multiple(() =>
        {
            Assert.That(settings.Value.Number, Is.EqualTo(newNumber));
            Assert.That(content, Is.EqualTo(
                $$"""
                {
                  "Number": {{newNumber}}
                }
                """));
        });
    }
}
