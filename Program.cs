using projekt_.NET_schronisko.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Services;
using projekt_.NET_schronisko.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IAnimalService, AnimalService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDbContext<ShelterContext>(options =>
        {
            //zmieniac w zaleznosci od uzycia
            //zmienic tez connection string
            //to jest do bazy danych w chmurze
            options.UseNpgsql(builder.Configuration.GetConnectionString("schroniskoDB"));
            //to jest do bazy danych lokalnie
            //options.UseSqlServer(builder.Configuration.GetConnectionString("schroniskoDB"));
        });

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddIdentity<Users, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ShelterContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

            options.Events.OnCreatingTicket = context =>
            {
                var email = context.Identity?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (email == null || !email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Logowanie mo¿liwe tylko przez konto gmail.com");
                }

                return Task.CompletedTask;
            };

            options.CallbackPath = "/signin-google";
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

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
                        Console.WriteLine($"B³¹d tworzenia pracownika: {error.Description}");
                    }
                }
            }
        }

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
                    await userManager.AddToRoleAsync(user, "Klient");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"B³¹d tworzenia klienta: {error.Description}");
                    }
                }
            }
        }

        app.Run();
    }
}
