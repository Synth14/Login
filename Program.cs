using Login.Data;
using Login.Models;
using Login.Resources;
using Login.Services.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Login.DevTools;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Login.Helpers;
using Duende.IdentityServer.Services;

namespace Login
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!
                .Replace("{DbUser}", builder.Configuration["ConnectionStrings:DbUser"])
                .Replace("{DbPassword}", builder.Configuration["ConnectionStrings:DbPassword"]);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), b => b.MigrationsAssembly("Login")));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDevelopmentSignKey();

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
             .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();

            // Modification de la configuration de DataProtection
            builder.Services.AddDataProtection()
                  .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
                  .SetApplicationName("Login")
                  .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                  {
                      EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                      ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                  });

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResources));
                });
            builder.Services.Configure<LocalizationOptions>(options =>
            {
                options.ResourcesPath = "Resources";
            });

            builder.Services.ConfigureApplicationCookie(o =>
            {
                o.LogoutPath = "/account/logout";
            });
            builder.Services.AddTransient<IEmailService, EmailService>(); 
            builder.Services.AddSingleton<IEventSink, IdentityServerEventSink>();


            builder.Services.AddAuthentication();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Login", Version = "v1" });
            });
            builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddConfigurationStore(o =>
                o.ConfigureDbContext = ctx => ctx.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), b => b.MigrationsAssembly("Login")))
                .AddOperationalStore(o => o.ConfigureDbContext = ctx => ctx.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), b => b.MigrationsAssembly("Login")))
                .AddAspNetIdentity<ApplicationUser>()
                .AddDeveloperSigningCredential();



            // Modification de la configuration Antiforgery
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".AspNetCore.Antiforgery.NewKey";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax; // Changé à Lax pour la compatibilité
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Path = "/";
            });
            var app = builder.Build();
            await app.SeedDatabase();
            string[] supportedCultures = ["en-EN", "fr-FR"];
            RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[1])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Login v1"));
            }

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
    }
}