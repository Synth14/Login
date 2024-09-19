using Duende.IdentityServer.Services;
using Login.Data;
using Login.Helpers;
using Login.Models;
using Login.Models.Settings;
using Login.Resources;
using Login.Services.EmailService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
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
            
            // Configuration setup
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            var requiredEnvVars = new[] { "DB_SERVER", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD" };
            foreach (var envVar in requiredEnvVars)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar)))
                {
                    throw new InvalidOperationException($"Environment variable '{envVar}' is not set.");
                }
                if (envVar == "DB_PORT")
                {
                    Console.WriteLine(envVar.GetType());
                }
            }
            builder.Services.ConfigureAppSettings(builder.Configuration);
            var dbSettings = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseSettings>();
            Console.WriteLine($"Connection string: {dbSettings.ConnectionString}");
            Console.WriteLine($"DB_SERVER: {Environment.GetEnvironmentVariable("DB_SERVER")}");
            Console.WriteLine($"DB_PORT: {Environment.GetEnvironmentVariable("DB_PORT")}");
            Console.WriteLine($"DB_NAME: {Environment.GetEnvironmentVariable("DB_NAME")}");
            Console.WriteLine($"DB_USER: {Environment.GetEnvironmentVariable("DB_USER")}");
            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(7032);
                options.ListenAnyIP(7031, listenOptions => listenOptions.UseHttps());
            });
            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    dbSettings.ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    mySqlOptions =>
                    {
                        mySqlOptions.MigrationsAssembly("Login");
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null
                        );
                    }
                )
            );
            builder.Services.AddHttpContextAccessor();
            //pour le reverseproxy
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDevelopmentSignKey();
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            }
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
            });
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

            // DataProtection configuration
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
                    sp.GetRequiredService<IWebHostEnvironment>(),
                    sp.GetRequiredService<ILogger<EmailService>>(),
                    sp.GetRequiredService<EmailSettings>()
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
                o.ConfigureDbContext = ctx => ctx.UseMySql(dbSettings.ConnectionString, new MySqlServerVersion(new Version(8, 0, 21)), b => b.MigrationsAssembly("Login")))
            .AddOperationalStore(o =>
                o.ConfigureDbContext = ctx => ctx.UseMySql(dbSettings.ConnectionString, new MySqlServerVersion(new Version(8, 0, 21)), b => b.MigrationsAssembly("Login")))
            .AddAspNetIdentity<ApplicationUser>()
            .AddDeveloperSigningCredential(persistKey: true, filename: Path.Combine(Environment.GetEnvironmentVariable("TEMPKEY_DIRECTORY") ?? ".", "tempkey.jwk"));
            
            // Antiforgery configuration
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = ".AspNetCore.Antiforgery.NewKey";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Path = "/";
            });

            var app = builder.Build();
            await app.SeedDatabase();
            app.UseForwardedHeaders();
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