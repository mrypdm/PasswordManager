using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="AccountsApiController"/>
/// </summary>
public class AccountsApiControllerTests
{
    private readonly Mock<ISecureItemsRepository> _repositoryMock = new();

    [SetUp]
    public void SetUp()
    {
        _repositoryMock.Reset();
    }

    [Test]
    public async Task GetAccountById_ItemNotExist_ShouldReturnNotFound()
    {
        // arrange
        var id = 10;
        _repositoryMock
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
        var account = new AccountData();
        var id = 10;
        _repositoryMock
            .Setup(m => m.GetAccountByIdAsync(id, default))
            .ReturnsAsync(account);

        var controller = CreateController();

        // act
        var res = await controller.GetAccountByIdAsync(id, default);

        // assert
        Assert.That(res.Value, Is.EqualTo(account));
        _repositoryMock.Verify(m => m.GetAccountByIdAsync(id, default), Times.Once);
    }

    [Test]
    public async Task GetAllHeaders_CommonWay_ShouldReturnHeaders()
    {
        // arrange
        var item0 = new SecureItemDbModel() { Id = 1, Name = "0" };
        var item1 = new SecureItemDbModel() { Id = 1, Name = "1" };
        _repositoryMock
            .Setup(m => m.GetItemsAsync(default))
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
        _repositoryMock.Verify(m => m.GetItemsAsync(default), Times.Once);
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

        _repositoryMock
            .Setup(m => m.AddAccountAsync(It.Is<AccountData>(m => CheckAccount(m, request)), default))
            .ReturnsAsync(id);

        var controller = CreateController();

        // act
        var res = await controller.AddAccountAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value.Id, Is.EqualTo(id));
        _repositoryMock.Verify(
            m => m.AddAccountAsync(It.Is<AccountData>(m => CheckAccount(m, request)), default),
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

        _repositoryMock
            .Setup(m => m.UpdateAccountAsync(id, It.Is<AccountData>(m => CheckAccount(m, request)), default))
            .ThrowsAsync(new ItemNotExistsException(null));

        var controller = CreateController();

        // act
        var res = await controller.UpdateAccountAsync(id, request, default);

        // assert
        Assert.That(res, Is.TypeOf<NotFoundObjectResult>());
        _repositoryMock.Verify(
            m => m.UpdateAccountAsync(id, It.Is<AccountData>(m => CheckAccount(m, request)), default),
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
        _repositoryMock.Verify(
            m => m.UpdateAccountAsync(
                id,
                It.Is<AccountData>(
                    m => m.Name == request.Name && m.Login == request.Login && m.Password == request.Password),
                default),
            Times.Once);
    }

    private AccountsApiController CreateController()
    {
        return new AccountsApiController(_repositoryMock.Object);
    }

    private static bool CheckAccount(AccountData actual, UploadAccountRequest expected)
    {
        return actual.Name == expected.Name && actual.Login == expected.Login && actual.Password == expected.Password;
    }
}
