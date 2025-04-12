using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Options;
using PasswordManager.Abstractions.Services;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref=""/>
/// </summary>
public class UserSettingsApiControllerTests
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
            .Setup(m => m.Create(It.IsAny<byte[]>(), It.IsAny<int>()))
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
            .Setup(m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default))
            .Callback((Action<UserOptions> act, CancellationToken _) =>
            {
                act(options);
            });

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
            m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default),
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
    public async Task ChangeKeySettings_InvalidSaltFormat_ShouldReturnBadRequest()
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "1",
            Iterations = 1,
            Salt = "not hex"
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
            Salt = "FF"
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
            Salt = "not hex",
            NewMasterPassword = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    [TestCase(null)]
    [TestCase("password")]
    public async Task ChangeKeySettings_ChangesAreNotNeeded_ShouldOnlyReturnOk(string newMasterPassword)
    {
        // arrange
        var request = new ChangeKeySettingsRequest
        {
            MasterPassword = "password",
            NewMasterPassword = newMasterPassword
        };
        var controller = CreateController();

        // act
        var res = await controller.ChangeKeySettingsAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Never);
        _generatorFactoryMock.Verify(m => m.Create(It.IsAny<byte[]>(), It.IsAny<int>()), Times.Never);
        _userOptionsMock.Verify(
            m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default),
            Times.Never);
        _keyServiceMock.Verify(
            m => m.ChangeKeySettingsAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), default),
            Times.Never);
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
            Salt = string.Empty,
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
        _generatorFactoryMock.Verify(m => m.Create(It.IsAny<byte[]>(), It.IsAny<int>()), Times.Exactly(2));
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Never);
        _userOptionsMock.Verify(
            m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default),
            Times.Never);
        _keyServiceMock.Verify(
            m => m.ChangeKeySettingsAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), default),
            Times.Never);
    }

    [Test]
    [TestCaseSource(nameof(ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings_TestCaseSource))]
    public async Task ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings(
        string salt, string newSalt,
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
            .Setup(m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default))
            .Callback((Action<UserOptions> act, CancellationToken _) =>
            {
                act(options);
            });
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
        _generatorFactoryMock.Verify(
            m => m.Create(request.SaltBytes, request.Iterations.Value),
            Times.Between(1, 2, Moq.Range.Inclusive));
        _generatorFactoryMock.Verify(
            m => m.Create(options.SaltBytes, options.Iterations),
            Times.Between(1, 2, Moq.Range.Inclusive));
        _userOptionsMock.Verify(
            m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default),
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
        var salt = RandomNumberGenerator.GetHexString(32);
        var options = new UserOptions() { Salt = salt };
        _userOptionsMock
            .Setup(m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default))
            .Callback((Action<UserOptions> act, CancellationToken _) =>
            {
                act(options);
            });

        var controller = CreateController();

        // act
        var res = await controller.DeleteStorageAsync(default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _userOptionsMock.Verify(
            m => m.UpdateAsync(It.IsAny<Action<UserOptions>>(), default),
            Times.Once);
        _keyServiceMock.Verify(m => m.ClearKeyDataAsync(default), Times.Once);
        _cookieHelperMock.Verify(m => m.SignOutAsync(default), Times.Once);
        Assert.That(options.Salt, Is.Not.EqualTo(salt));
    }

    private UserSettingsApiController CreateController()
    {
        return new UserSettingsApiController(_userOptionsMock.Object, _cookieHelperMock.Object, _generatorFactoryMock.Object,
            _keyServiceMock.Object);
    }

    private static IEnumerable<TestCaseData> ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings_TestCaseSource()
    {
        const string testName = nameof(ChangeKeySettings_ChangesAreNeeded_ShouldChangeSettings);

        var salt = RandomNumberGenerator.GetHexString(32);
        var newSalt = RandomNumberGenerator.GetHexString(32);
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
