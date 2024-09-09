using Duende.IdentityServer.Services;
using Login.Data;
using Login.DevTools;
using Login.Helpers;
using Login.Models;
using Login.Resources;
using Login.Services.EmailService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;

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
            builder.Services.AddHttpContextAccessor();


            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDevelopmentSignKey();
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            }
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
            builder.Services.AddTransient<IEmailService, EmailService>(sp =>
                new EmailService(
                    sp.GetRequiredService<IConfiguration>(),
                    sp.GetRequiredService<IWebHostEnvironment>(),
                      sp.GetRequiredService<ILogger<EmailService>>()
                    )
                );
            builder.Services.AddSingleton<IEventSink, IdentityServerEventSink>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            builder.Services.AddAuthentication();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Login", Version = "v1" });
            });

            builder.Services.AddIdentityServer(options =>
            {
                options.IssuerUri = builder.Configuration["Login:BaseURL"];

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.LogoutUrl = "/Account/Logout";
                options.UserInteraction.LogoutIdParameter = "logoutId";
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
                options.Cookie.SameSite = SameSiteMode.Lax; // Chang� � Lax pour la compatibilit�
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
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Login v1"));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.UseCors("AllowAll");
            app.Run();
        }
    }
}