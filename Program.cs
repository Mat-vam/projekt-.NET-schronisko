using projekt_.NET_schronisko.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Services;
using projekt_.NET_schronisko.Interfaces;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<ICartService, CartService>(); // Rejestracja serwisu koszyka
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddDbContext<ShelterContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("schroniskoDB"));
        });

        builder.Services.AddIdentity<Users, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;    //Account oraz Email powinno byæ na True w ostatniej wersji !!!
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ShelterContext>()
                .AddDefaultTokenProviders();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Tworzenie ról (jeœli nie istniej¹)
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Pracownik", "Klient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // Tworzenie konta Pracownika
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            string email = "pracownik@schronisko.com";
            string password = "Test1234!";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Users
                {
                    UserName = email,
                    Email = email,
                    FullName = email
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Pracownik");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Blad tworzenia pracownika: {error.Description}");
                    }
                }
            }
        }

        // Tworzenie konta Klienta
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            string email = "klient@schronisko.com";
            string password = "Test1234!";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Users
                {
                    UserName = email,
                    Email = email,
                    FullName = email
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Klient"); // Automatyczne przypisanie roli "Klient"
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Blad tworzenia klienta: {error.Description}");
                    }
                }
            }
        }

        app.Run();
    }
}