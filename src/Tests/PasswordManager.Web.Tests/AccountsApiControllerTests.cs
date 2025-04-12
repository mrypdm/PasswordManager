using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Services;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="AccountsApiController"/>
/// </summary>
public class AccountsApiControllerTests
{
    private readonly Mock<IAccountService> _serviceMock = new();

    [SetUp]
    public void SetUp()
    {
        _serviceMock.Reset();
    }

    [Test]
    public async Task GetAccountById_ItemNotExist_ShouldReturnNotFound()
    {
        // arrange
        var id = 10;
        _serviceMock
            .Setup(m => m.GetAccountByIdAsync(id, default))
            .ThrowsAsync(new ItemNotExistsException(null));

        var controller = CreateController();

        // act
        var res = await controller.GetAccountByIdAsync(id, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAccountById_CommonWay_ShouldReturnAccount()
    {
        // arrange
        var account = new Account();
        var id = 10;
        _serviceMock
            .Setup(m => m.GetAccountByIdAsync(id, default))
            .ReturnsAsync(account);

        var controller = CreateController();

        // act
        var res = await controller.GetAccountByIdAsync(id, default);

        // assert
        Assert.That(res.Value, Is.EqualTo(account));
        _serviceMock.Verify(m => m.GetAccountByIdAsync(id, default), Times.Once);
    }

    [Test]
    public async Task GetAllHeaders_CommonWay_ShouldReturnHeaders()
    {
        // arrange
        var item0 = new AccountHeader { Id = 0, Name = "name0" };
        var item1 = new AccountHeader { Id = 1, Name = "name1" };
        _serviceMock
            .Setup(m => m.GetAccountHeadersAsync(default))
            .ReturnsAsync([item0, item1]);

        var controller = CreateController();

        // act
        var res = await controller.GetAllHeadersAsync(default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(res.Value[0].Id, Is.EqualTo(item0.Id));
            Assert.That(res.Value[0].Name, Is.EqualTo(item0.Name));
            Assert.That(res.Value[1].Id, Is.EqualTo(item1.Id));
            Assert.That(res.Value[1].Name, Is.EqualTo(item1.Name));
        });
        _serviceMock.Verify(m => m.GetAccountHeadersAsync(default), Times.Once);
    }

    [Test]
    public async Task AddAccount_EmptyName_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.AddAccountAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task AddAccount_EmptyLogin_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.AddAccountAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task AddAccount_EmptyPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = "1",
            Password = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.AddAccountAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task AddAccount_CommonWay_ShouldAddAccount()
    {
        // arrange
        var id = 12;
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = "1",
            Password = "1"
        };

        _serviceMock
            .Setup(m => m.AddAccountAsync(It.Is<Account>(m => CheckAccount(m, default, request)), default))
            .ReturnsAsync(id);

        var controller = CreateController();

        // act
        var res = await controller.AddAccountAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value.Id, Is.EqualTo(id));
        _serviceMock.Verify(
            m => m.AddAccountAsync(It.Is<Account>(m => CheckAccount(m, default, request)), default),
            Times.Once);
    }

    [Test]
    public async Task UpdateAccount_EmptyName_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(0, request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAccount_EmptyLogin_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(0, request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAccount_EmptyPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = "1",
            Password = ""
        };
        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(0, request, default);

        // assert
        Assert.That(res, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateAccount_ItemNotExists_ShouldReturnNotFound()
    {
        // arrange
        var id = 12;
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = "1",
            Password = "1"
        };

        _serviceMock
            .Setup(m => m.UpdateAccountAsync(It.Is<Account>(m => CheckAccount(m, id, request)), default))
            .ThrowsAsync(new ItemNotExistsException(null));

        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(id, request, default);

        // assert
        Assert.That(res, Is.TypeOf<NotFoundObjectResult>());
        _serviceMock.Verify(
            m => m.UpdateAccountAsync(It.Is<Account>(m => CheckAccount(m, id, request)), default),
            Times.Once);
    }

    [Test]
    public async Task UpdateAccount_CommonWay_ShouldUpdateAccount()
    {
        // arrange
        var id = 12;
        var request = new UploadAccountRequest
        {
            Name = "1",
            Login = "1",
            Password = "1"
        };

        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(id, request, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _serviceMock.Verify(
            m => m.UpdateAccountAsync(It.Is<Account>(m => CheckAccount(m, id, request)), default),
            Times.Once);
    }

    [Test]
    public async Task DeleteAccount_CommonWay_ShouldDeleteAccount()
    {
        // arrange
        var id = 12;
        var controller = CreateController();

        // act
        var res = await controller.DeleteAccountAsync(id, default);

        // assert
        Assert.That(res, Is.TypeOf<OkResult>());
        _serviceMock.Verify(
            m => m.DeleteAccountAsync(id, default),
            Times.Once);
    }

    private AccountsApiController CreateController()
    {
        return new AccountsApiController(_serviceMock.Object);
    }

    private static bool CheckAccount(Account actual, int id, UploadAccountRequest expected)
    {
        return actual.Id == id
            && actual.Name == expected.Name
            && actual.Data.Login == expected.Login
            && actual.Data.Password == expected.Password;
    }
}
