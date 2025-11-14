using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SocialNetworkWeb.Data;
using SocialNetworkWeb.Models;
using SocialNetworkWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;         // Не требовать цифры
    options.Password.RequireLowercase = false;     // Не требовать строчные
    options.Password.RequireUppercase = false;     // Не требовать заглавные
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 2;           // Минимум 2 символа
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// Seed data class
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
    }
}