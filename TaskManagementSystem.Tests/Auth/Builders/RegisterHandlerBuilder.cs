using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagementSystem.Domain.Users;
using TaskManagementSystem.Features.Auth.Register;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Tests.Auth.Builders;

public class RegisterHandlerBuilder : IDisposable
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new(MockBehavior.Strict);
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new(MockBehavior.Strict);
    private readonly AppDbContext _db;

    public RegisterHandlerBuilder()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    public RegisterHandlerBuilder WithPasswordHash(string hash)
    {
        _passwordHasherMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns(hash);
        return this;
    }

    public RegisterHandlerBuilder WithGenerateToken(string token)
    {
        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);
        return this;
    }

    public RegisterHandlerBuilder WithExistingUser(User user)
    {
        _db.Users.Add(user);
        _db.SaveChanges();
        return this;
    }

    public RegisterHandlerBuilder VerifyHashCalled(int times = 1)
    {
        _passwordHasherMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Exactly(times));
        return this;
    }

    public RegisterHandlerBuilder VerifyHashNotCalled()
    {
        _passwordHasherMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never);
        return this;
    }

    public RegisterHandlerBuilder VerifyGenerateTokenCalled(int times = 1)
    {
        _jwtTokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Exactly(times));
        return this;
    }

    public RegisterHandlerBuilder VerifyGenerateTokenNotCalled()
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

    public AppDbContext GetDbContext() => _db;

    public RegisterHandler Build() => new(_db, _passwordHasherMock.Object, _jwtTokenServiceMock.Object);

    public void Dispose() => _db.Dispose();
}
