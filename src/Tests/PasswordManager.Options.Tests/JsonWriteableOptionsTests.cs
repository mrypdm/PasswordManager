using System.Threading.Tasks;

namespace PasswordManager.Options.Tests;

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
    public async Task WithoutFile_CreatesNew()
    {
        // arrange
        // act
        var settings = await JsonWriteableOptions.CreateAsync<ExampleOptions>("not-exists", default);

        // assert
        Assert.That(settings.Value.Number, Is.EqualTo(DefaultNumber));
    }

    [Test]
    public async Task BadJson_ReturnsNew()
    {
        // arrange
        using var file = new TempFile();
        file.Write(
            """
            {
              "Number": 444
            """);

        // act
        var settings = await JsonWriteableOptions.CreateAsync<ExampleOptions>(file.FilePath, default);

        // assert
        Assert.That(settings.Value.Number, Is.EqualTo(DefaultNumber));
    }

    [Test]
    public async Task WithFile_ReadsFromFile()
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
        var settings = await JsonWriteableOptions.CreateAsync<ExampleOptions>(file.FilePath, default);

        // assert
        Assert.That(settings.Value.Number, Is.EqualTo(number));
    }

    [Test]
    public async Task Update_ShouldStoreNewValue()
    {
        // arrange
        const int newNumber = 234;
        using var file = new TempFile();

        // act
        var settings = await JsonWriteableOptions.CreateAsync<ExampleOptions>(file.FilePath, default);
        await settings.UpdateAsync(o => o.Number = newNumber, default);

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
