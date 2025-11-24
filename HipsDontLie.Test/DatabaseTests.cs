using HipsDontLie.Database;
using HipsDontLie.Models;
using HipsDontLie.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace HipsDontLie.Test
{
    [TestFixture]
    public class DatabaseTests
    {
        private CustomWebApplicationFactory<Program> _factory = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<Program>();
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task Database_CanConnect()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var canConnect = await db.Database.CanConnectAsync();
            Assert.That(canConnect, Is.True, "Cannot connect to the database");

        }

        [Test]
        public async Task Database_HasAppliedMigrations()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var applied = await db.Database.GetAppliedMigrationsAsync();
            Assert.That(applied.Any(), Is.True, "There are no applied migrations");

        }

        [Test]
        public async Task Database_CanCreateUser()
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var user = new User
            {
                Id = 1,
                Email = "test@tester.dk",
                UserName = "test@tester.dk",
                DisplayName = "Tester",
                EmailConfirmed = true
            };

            var res = await userManager.CreateAsync(user, "Password123!");
            Assert.That(res.Succeeded, Is.True, "Failed to create user");

            var userFromDb = await userManager.FindByIdAsync(user.Id.ToString());

            var actualUser = JsonSerializer.Serialize(user);
            var expectedUser = JsonSerializer.Serialize(userFromDb);
            Assert.That(actualUser, Is.EqualTo(expectedUser),"Actual user is not the same as Expected user");

            await userManager.DeleteAsync(user);
        }

    }
}