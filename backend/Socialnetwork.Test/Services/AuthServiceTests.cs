using SocialNetwork.Services;
using SocialNetwork.Test.Factories;
using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Test.Services;

public class AuthServiceTests
{
    [Fact]
    public void Login_ShouldFail_WhenUserDoesNotExist()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new AuthService(context);

        // act
        var result = service.Login("unknownUser", "password");

        // assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public void Login_ShouldFail_WhenPasswordIsIncorrect()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();

        context.Users.Add(new User
        {
            Username = "hamse",
            PasswordHash = AuthService.HashPassword("correctpassword")
        });

        context.SaveChanges();

        var service = new AuthService(context);

        // act
        var result = service.Login("hamse", "wrongpassword");

        // assert
        Assert.False(result.Success);
        Assert.Equal("Incorrect password", result.ErrorMessage);
    }

    [Fact]
    public void Login_ShouldSucceed_WhenCredentialsAreCorrect()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();

        context.Users.Add(new User
        {
            Username = "hamse",
            PasswordHash = AuthService.HashPassword("12345")
        });

        context.SaveChanges();

        var service = new AuthService(context);

        // act
        var result = service.Login("hamse", "12345");

        // assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
    }
}
