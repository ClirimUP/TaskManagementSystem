using Bogus;
using TaskManagementSystem.Domain.Users;

namespace TaskManagementSystem.Tests.Helpers;

public static class UserHelper
{
    private static readonly Faker<User> Faker = new Faker<User>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Email, f => f.Internet.Email().ToLowerInvariant())
        .RuleFor(x => x.PasswordHash, f => f.Random.Hash())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent());

    public static User Generate(
        Guid? id = null,
        string? email = null,
        string? passwordHash = null)
    {
        var user = Faker.Generate();

        if (id.HasValue) user.Id = id.Value;
        if (email is not null) user.Email = email;
        if (passwordHash is not null) user.PasswordHash = passwordHash;

        return user;
    }
}
