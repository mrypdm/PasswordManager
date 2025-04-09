using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.SecureData.Services;
using PasswordManager.Web.Controllers;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Views.Authentication;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="AuthenticationController"/>
/// </summary>
public class AuthenticationControllerTests
{
    private readonly Mock<IMasterKeyService> _keyServiceMock = new();
    private readonly Mock<ICookieAuthorizationHelper> _cookieHelperMock = new();

    [SetUp]
    public void SetUp()
    {
        _keyServiceMock.Reset();
        _cookieHelperMock.Reset();
    }

    [Test]
    public async Task Logout_ShouldClearKeyAndLogout()
    {
        // arrange
        var controller = CreateController();

        // act
        var res = await controller.LogoutAsync(default);

        // assert
        Assert.That(res, Is.TypeOf<RedirectResult>());
        Assert.That((res as RedirectResult).Url, Is.EqualTo("/auth/login"));
        _keyServiceMock.Verify(m => m.ClearMasterKeyAsync(default), Times.Once);
        _cookieHelperMock.Verify(m => m.SignOutAsync(It.IsAny<HttpContext>()), Times.Once);
    }

    [Test]
    public async Task GetView_ShouldCheckMasterKeyDataExistance()
    {
        // arrange
        var returnUrl = "/test";
        var masterKeyDataExist = true;
        _keyServiceMock
            .Setup(m => m.IsMasterKeyDataExistsAsync(default))
            .ReturnsAsync(masterKeyDataExist);

        var controller = CreateController();

        // act
        var res = await controller.GetViewAsync(returnUrl, default);

        // assert
        Assert.That(res, Is.TypeOf<ViewResult>());
        var model = (res as ViewResult).Model as LoginModel;
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model.ReturnUrl, Is.EqualTo(returnUrl));
            Assert.That(model.IsMasterKeyDataExist, Is.EqualTo(masterKeyDataExist));
        });
    }

    private AuthenticationController CreateController()
    {
        return new AuthenticationController(_keyServiceMock.Object, _cookieHelperMock.Object);
    }
}
