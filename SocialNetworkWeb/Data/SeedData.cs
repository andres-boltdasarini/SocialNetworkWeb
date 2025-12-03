// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkWeb.Models;
using System;
using System.Threading.Tasks;

namespace SocialNetworkWeb.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminUser = await userManager.FindByEmailAsync("admin@socialnetwork.com");
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@socialnetwork.com",
                    Email = "admin@socialnetwork.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var createUser = await userManager.CreateAsync(user, "Admin123!");
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Создаем тестовых пользователей для проверки дружбы
            await CreateTestUsers(userManager);
        }

        private static async Task CreateTestUsers(UserManager<ApplicationUser> userManager)
        {
            // Тестовый пользователь 1
            var testUser1 = await userManager.FindByEmailAsync("test1@socialnetwork.com");
            if (testUser1 == null)
            {
                var user1 = new ApplicationUser
                {
                    UserName = "test1@socialnetwork.com",
                    Email = "test1@socialnetwork.com",
                    FirstName = "Иван",
                    LastName = "Иванов",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user1, "Test123!");
            }

            // Тестовый пользователь 2
            var testUser2 = await userManager.FindByEmailAsync("test2@socialnetwork.com");
            if (testUser2 == null)
            {
                var user2 = new ApplicationUser
                {
                    UserName = "test2@socialnetwork.com",
                    Email = "test2@socialnetwork.com",
                    FirstName = "Петр",
                    LastName = "Петров",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user2, "Test123!");
            }

            // Тестовый пользователь 3
            var testUser3 = await userManager.FindByEmailAsync("test3@socialnetwork.com");
            if (testUser3 == null)
            {
                var user3 = new ApplicationUser
                {
                    UserName = "test3@socialnetwork.com",
                    Email = "test3@socialnetwork.com",
                    FirstName = "Мария",
                    LastName = "Сидорова",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user3, "Test123!");
            }
        }
    }
}