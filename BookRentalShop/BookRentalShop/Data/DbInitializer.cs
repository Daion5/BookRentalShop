using BookRentalShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalShop.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminUser = new ApplicationUser
            {
                UserName = "admin@bookrental.com",
                Email = "admin@bookrental.com",
            };

            string adminPassword = "Admin@123";
            var user = await userManager.FindByEmailAsync("admin@bookrental.com");

            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Science Fiction" },
                    new Category { Name = "Fantasy" },
                    new Category { Name = "Programming" },
                    new Category { Name = "Classic" },
                    new Category { Name = "Adventure" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Books
            if (!context.Books.Any())
            {
                context.Books.AddRange(
                    new Book
                    {
                        Title = "Diuna",
                        Author = "Frank Herbert",
                        CategoryId = context.Categories.First(c => c.Name == "Science Fiction").CategoryId,
                        Price = 39,
                        Year = 1965,
                        Description = "Diuna to powieść science fiction...",
                        AvailableCopies = 5
                    },
                    new Book
                    {
                        Title = "Harry Potter i Kamień Filozoficzny",
                        Author = "J.K. Rowling",
                        CategoryId = context.Categories.First(c => c.Name == "Fantasy").CategoryId,
                        Price = 29,
                        Year = 1997,
                        Description = "Pierwsza książka z serii o Harrym Potterze...",
                        AvailableCopies = 3
                    },
                    new Book
                    {
                        Title = "Python. Wprowadzenie",
                        Author = "Mark Lutz",
                        CategoryId = context.Categories.First(c => c.Name == "Programming").CategoryId,
                        Price = 89,
                        Year = 2019,
                        Description = "Podręcznik programowania w Pythonie...",
                        AvailableCopies = 7
                    },
                    new Book
                    {
                        Title = "Mały Książę",
                        Author = "Antoine de Saint-Exupéry",
                        CategoryId = context.Categories.First(c => c.Name == "Classic").CategoryId,
                        Price = 19,
                        Year = 1943,
                        Description = "Klasyczna opowieść o Małym Księciu...",
                        AvailableCopies = 10
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
