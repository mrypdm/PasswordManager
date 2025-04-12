using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Options;
using PasswordManager.Abstractions.Services;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="LogonApiController"/>
/// </summary>
public class LogonApiControllerTests
{
    private readonly Mock<IKeyService> _keyServiceMock = new();
    private readonly Mock<IKeyGenerator> _keyGeneratorMock = new();
    private readonly Mock<ICookieAuthorizationHelper> _cookieHelperMock = new();
    private readonly Mock<IWritableOptions<UserOptions>> _userOptionsMock = new();
    private readonly Mock<IOptions<ConnectionOptions>> _connectionOptionsMock = new();

    [SetUp]
    public void SetUp()
    {
        _keyServiceMock.Reset();
        _keyGeneratorMock.Reset();
        _cookieHelperMock.Reset();
        _userOptionsMock.Reset();
        _connectionOptionsMock.Reset();
    }

    [Test]
    public async Task SignIn_Empty_ShouldReturnBadRequest()
    {
        // arrange
        var request = new LoginRequest() { MasterPassword = "" };
        var controller = CreateController();

        // act
        var res = await controller.SignInAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task SignIn_CommonWay_ShouldInitKeyAndSignInAndReturnOk()
    {
        // arrange
        var key = Array.Empty<byte>();
        var request = new LoginRequest() { MasterPassword = "password" };
        var userOptions = new UserOptions() { SessionTimeout = TimeSpan.FromSeconds(1) };
        var connectionOptions = new ConnectionOptions() { IsProxyUsed = false };
        var controller = CreateController();

        _keyGeneratorMock
            .Setup(m => m.Generate(request.MasterPassword))
            .Returns(key);
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(userOptions);
        _connectionOptionsMock
            .Setup(m => m.Value)
            .Returns(connectionOptions);

        // act
        var res = await controller.SignInAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _keyServiceMock.Verify(
            m => m.InitKeyAsync(key, userOptions.SessionTimeout, default),
            Times.Once);
        _cookieHelperMock.Verify(m => m.SignInAsync(It.IsAny<HttpContext>(), connectionOptions), Times.Once);
    }

    [Test]
    public async Task SignIn_InvalidKey_ShouldReturnUnauthroized()
    {
        // arrange
        var request = new LoginRequest() { MasterPassword = "password" };
        var userOptions = new UserOptions() { SessionTimeout = TimeSpan.FromSeconds(1) };
        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(userOptions);
        _keyServiceMock
            .Setup(m => m.InitKeyAsync(It.IsAny<byte[]>(), userOptions.SessionTimeout, default))
            .ThrowsAsync(new KeyValidationException());

        var controller = CreateController();

        // act
        var res = await controller.SignInAsync(request, default);

        // assert
        Assert.That(res, Is.TypeOf<UnauthorizedObjectResult>());
        _keyServiceMock.Verify(
            m => m.InitKeyAsync(It.IsAny<byte[]>(), userOptions.SessionTimeout, default),
            Times.Once);
        _cookieHelperMock.Verify(
            m => m.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<ConnectionOptions>()),
            Times.Never);
    }

    [Test]
    public async Task SignIn_StorageIsBlocked_ShouldReturnForbidden()
    {
        // arrange
        var request = new LoginRequest() { MasterPassword = "password" };
        var userOptions = new UserOptions() { SessionTimeout = TimeSpan.FromSeconds(1) };
        var controller = CreateController();

        _userOptionsMock
            .Setup(m => m.Value)
            .Returns(userOptions);

        _keyServiceMock
            .Setup(m => m.InitKeyAsync(It.IsAny<byte[]>(), userOptions.SessionTimeout, default))
            .ThrowsAsync(new StorageBlockedException());

        // act
        var res = await controller.SignInAsync(request, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(res, Is.TypeOf<ObjectResult>());
            Assert.That((res as ObjectResult).StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        });
        _keyServiceMock.Verify(
            m => m.InitKeyAsync(It.IsAny<byte[]>(), userOptions.SessionTimeout, default),
            Times.Once);
        _cookieHelperMock.Verify(
            m => m.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<ConnectionOptions>()),
            Times.Never);
    }

    [Test]
    public async Task SignOut_ShouldClearKeyAndSignOut()
    {
        // arrange
        var controller = CreateController();

        // act
        var res = await controller.SignOutAsync(default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _keyServiceMock.Verify(
            m => m.ClearKeyAsync(default),
            Times.Once);
        _cookieHelperMock.Verify(
            m => m.SignOutAsync(It.IsAny<HttpContext>()),
            Times.Once);
    }

    private LogonApiController CreateController()
    {
        return new LogonApiController(_keyServiceMock.Object, _keyGeneratorMock.Object, _cookieHelperMock.Object,
            _userOptionsMock.Object, _connectionOptionsMock.Object);
    }
}
