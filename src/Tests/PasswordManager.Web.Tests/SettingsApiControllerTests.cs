using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Options;
using PasswordManager.Abstractions.Services;
using PasswordManager.Core.Options;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref=""/>
/// </summary>
public class SettingsApiControllerTests
{
    private readonly Mock<IWritableOptions<UserOptions>> _userOptionsMock = new();
    private readonly Mock<ICookieAuthorizationHelper> _cookieHelperMock = new();
    private readonly Mock<IKeyGeneratorFactory> _generatorFactoryMock = new();
    private readonly Mock<IKeyGenerator> _generatorMock = new();
    private readonly Mock<IKeyService> _keyServiceMock = new();

    [SetUp]
    public void SetUp()
    {
        _userOptionsMock.Reset();
        _cookieHelperMock.Reset();
        _generatorFactoryMock.Reset();
        _keyServiceMock.Reset();
        _generatorMock.Reset();

        _generatorFactoryMock
            .Setup(m => m.Create(It.IsAny<IKeyGeneratorOptions>()))
            .Returns(_generatorMock.Object);
    }

    [Test]
    public void GetSettings_ShouldReturnUserSettings()
    {
        // arrange
        var options = new UserOptions();
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var controller = CreateController();

        // act
        var settings = controller.GetSettings();

        // assert
        Assert.That(settings.Value, Is.EqualTo(options));
    }

    [Test]
    public async Task ChangeSessionTimeout_InvalidTimeout_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeSessionTimeoutRequest
        {
            Timeout = TimeSpan.Zero
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeSessionTimeoutAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeSessionTimeout_CommonWay_ShouldUpdateOptionsAndStorage()
    {
        // arrange
        var options = new UserOptions();
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(options);

        _userOptionsMock
            .Setup(m => m.Update(It.IsAny<Action<UserOptions>>()))
            .Callback((Action<UserOptions> act) => act(options));

        var request = new ChangeSessionTimeoutRequest
        {
            Timeout = TimeSpan.FromSeconds(1)
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeSessionTimeoutAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _userOptionsMock.Verify(
            m => m.Update(It.IsAny<Action<UserOptions>>()),
            Times.Once);
        _keyServiceMock.Verify(m => m.ChangeKeyTimeoutAsync(request.Timeout, default), Times.Once);
        Assert.That(options.SessionTimeout, Is.EqualTo(request.Timeout));
    }

    [Test]
    public async Task ChangeKeySettings_InvalidMasterPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeKeySettings_InvalidIterationsCount_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "1",
            Iterations = 0
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeKeySettings_InvalidSaltSize_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "1",
            Iterations = 1,
            Salt = []
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeKeySettings_InvalidNewMasterPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "1",
            Iterations = 1,
            NewMasterPassword = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ChangeKeySettings_KeyNotChanged_ShouldOnlyReturnOk()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "password",
            NewMasterPassword = "new_password"
        };
        var options = new UserOptions()
        {
            Salt = [],
            Iterations = 0
        };

        _generatorMock
            .Setup(m => m.Generate(It.IsAny<string>()))
            .Returns([]);
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _generatorFactoryMock.Verify(m => m.Create(It.IsAny<IKeyGeneratorOptions>()), Times.Exactly(2));
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Never);
        _userOptionsMock.Verify(
            m => m.Update(It.IsAny<Action<UserOptions>>()),
            Times.Never);
        _keyServiceMock.Verify(
            m => m.ChangeKeySettingsAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), default),
            Times.Never);
    }

    [Test]
    [TestCaseSource(nameof(ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings_TestCaseSource))]
    public async Task ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings(
        byte[] salt, byte[] newSalt,
        int iterations, int newIterations,
        string masterPassword, string newMasterPassword)
    {
        // arrange
        var oldKey = RandomNumberGenerator.GetBytes(32);
        var newKey = RandomNumberGenerator.GetBytes(32);
        var options = new UserOptions()
        {
            Salt = salt,
            Iterations = iterations
        };
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(options);
        _userOptionsMock
            .Setup(m => m.Update(It.IsAny<Action<UserOptions>>()))
            .Callback((Action<UserOptions> act) => act(options));
        _generatorMock
            .SetupSequence(m => m.Generate(It.IsAny<string>()))
            .Returns(oldKey)
            .Returns(newKey);

        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = masterPassword,
            NewMasterPassword = newMasterPassword,
            Salt = newSalt,
            Iterations = newIterations
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Once);
        _generatorMock
            .Verify(m => m.Generate(masterPassword), Times.Between(1, 2, Moq.Range.Inclusive));
        _generatorMock
            .Verify(m => m.Generate(newMasterPassword), Times.Between(1, 2, Moq.Range.Inclusive));
        _generatorFactoryMock.Verify(m => m.Create(It.IsAny<IKeyGeneratorOptions>()), Times.Exactly(2));
        _generatorFactoryMock.Verify(m => m.Create(options), Times.Once);
        _userOptionsMock.Verify(
            m => m.Update(It.IsAny<Action<UserOptions>>()),
            Times.Once);
        _keyServiceMock.Verify(m => m.ChangeKeySettingsAsync(oldKey, newKey, default), Times.Once);
        Assert.Multiple(() =>
        {
            Assert.That(options.Salt, Is.EqualTo(newSalt));
            Assert.That(options.Iterations, Is.EqualTo(newIterations));
        });
    }

    [Test]
    public async Task DeleteStorage_CommonWay_ShouldClearKeyDataAndChangeSaltAndLogout()
    {
        // arrange
        var salt = RandomNumberGenerator.GetBytes(32);
        var options = new UserOptions() { Salt = salt };
        _userOptionsMock
            .Setup(m => m.Update(It.IsAny<Action<UserOptions>>()))
            .Callback((Action<UserOptions> act) => act(options));

        var controller = CreateController();

        // act
        var res = await controller.DeleteStorageAsync(default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _userOptionsMock.Verify(
            m => m.Update(It.IsAny<Action<UserOptions>>()),
            Times.Once);
        _keyServiceMock.Verify(m => m.ClearKeyDataAsync(default), Times.Once);
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Once);
        Assert.That(options.Salt, Is.Not.EqualTo(salt));
    }

    private SettingsApiController CreateController()
    {
        return new SettingsApiController(_userOptionsMock.Object, _cookieHelperMock.Object, _generatorFactoryMock.Object,
            _keyServiceMock.Object);
    }

    private static IEnumerable<TestCaseData> ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings_TestCaseSource()
    {
        const string testName = nameof(ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings);

        var salt = RandomNumberGenerator.GetBytes(16);
        var newSalt = RandomNumberGenerator.GetBytes(16);
        var iterations = 1;
        var newIterations = 2;
        var masterPassword = "password";
        var newMasterPassword = "new_password";

        yield return new TestCaseData(salt, salt, iterations, iterations, masterPassword, newMasterPassword)
            .SetName($"{testName}(Master password)");
        yield return new TestCaseData(salt, salt, iterations, newIterations, masterPassword, masterPassword)
            .SetName($"{testName}(Iterations)");
        yield return new TestCaseData(salt, salt, iterations, newIterations, masterPassword, newMasterPassword)
            .SetName($"{testName}(Master password & Iterations)");
        yield return new TestCaseData(salt, newSalt, iterations, iterations, masterPassword, masterPassword)
            .SetName($"{testName}(Salt)");
        yield return new TestCaseData(salt, newSalt, iterations, iterations, masterPassword, newMasterPassword)
            .SetName($"{testName}(Master password & Salt)");
        yield return new TestCaseData(salt, newSalt, iterations, newIterations, masterPassword, masterPassword)
            .SetName($"{testName}(Iterations & Salt)");
        yield return new TestCaseData(salt, newSalt, iterations, newIterations, masterPassword, newMasterPassword)
            .SetName($"{testName}(Master password & Iterations & Salt)");
    }
}
