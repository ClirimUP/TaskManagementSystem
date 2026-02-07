using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagementSystem.Domain.Users;
using TaskManagementSystem.Features.Auth.Login;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Tests.Auth.Builders;

public class LoginHandlerBuilder : IDisposable
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new(MockBehavior.Strict);
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new(MockBehavior.Strict);
    private readonly AppDbContext _db;

    public LoginHandlerBuilder()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    public LoginHandlerBuilder WithPasswordVerify(bool returns)
    {
        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(returns);
        return this;
    }

    public LoginHandlerBuilder WithGenerateToken(string token)
    {
        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);
        return this;
    }

    public LoginHandlerBuilder WithExistingUser(User user)
    {
        _db.Users.Add(user);
        _db.SaveChanges();
        return this;
    }

    public LoginHandlerBuilder VerifyVerifyCalled(int times = 1)
    {
        _passwordHasherMock.Verify(
            x => x.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(times));
        return this;
    }

    public LoginHandlerBuilder VerifyVerifyNotCalled()
    {
        _passwordHasherMock.Verify(
            x => x.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        return this;
    }

    public LoginHandlerBuilder VerifyGenerateTokenCalled(int times = 1)
    {
        _jwtTokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Exactly(times));
        return this;
    }

    public LoginHandlerBuilder VerifyGenerateTokenNotCalled()
    {
        _jwtTokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
        return this;
    }

    public void VerifyNoOtherCalls()
    {
        _passwordHasherMock.VerifyNoOtherCalls();
        _jwtTokenServiceMock.VerifyNoOtherCalls();
    }

    public LoginHandler Build() => new(_db, _passwordHasherMock.Object, _jwtTokenServiceMock.Object);

    public void Dispose() => _db.Dispose();
}
