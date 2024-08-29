using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using IdentityModel;

namespace Login.Data
{
    public static class Seed
    {
        public static async Task SeedDatabase(this WebApplication app)
        {
            IServiceScope scope = app.Services.CreateScope();

            ApplicationDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await applicationDbContext.Database.MigrateAsync();

            ConfigurationDbContext configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            await configurationDbContext.Database.MigrateAsync();

            PersistedGrantDbContext persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            await persistedGrantDbContext.Database.MigrateAsync();

            #region DefaultIdentityResources
            if (!await configurationDbContext.IdentityResources.AnyAsync(IdRessource => IdRessource.Name == "openid" || IdRessource.Name == "profile" || IdRessource.Name == "email" || IdRessource.Name == "phone" || IdRessource.Name == "address" || IdRessource.Name == "roles")  )
            {
                IdentityResource[] identityResources = new IdentityResource[]
                {
                new() {
                    Name= "openid",
                    DisplayName="openid",
                    Description="Open ID scope",
                    Required=true,
                    UserClaims=new string []
                    {
                        "sub"
                    }
                },
                new() {
                    Name= "profile",
                    DisplayName="profile",
                    Description="Profile scope",
                    UserClaims=new string []
                    {
                        "name",
                        "given_name",
                        "family_name",
                        "middle_name",
                        "nickname",
                        "preferred_username",
                        "profile",
                        "picture",
                        "website",
                        "gender",
                        "birthdate",
                        "zoneinfo",
                        "locale",
                        "updated_at"
                    }
                },
                new() {
                    Name= "email",
                    DisplayName="email",
                    Description="Email scope",
                    UserClaims=new string []
                    {
                        "email",
                        "email_verified"
                    }
                },
                new() {
                    Name= "phone",
                    DisplayName="phone",
                    Description="Phone scope",
                    UserClaims=new string []
                    {
                        "phone_number",
                        "phone_number_verified"
                    }
                },
                new() {
                    Name= "address",
                    DisplayName="address",
                    Description="Address scope",
                    UserClaims=new string []
                    {
                        "address"
                    }
                },
                new() {
                    Name= "roles",
                    DisplayName="roles",
                    Description="Roles scope",
                    UserClaims=new string []
                    {
                        "role"
                    }
                }
                };
                IEnumerable<Duende.IdentityServer.EntityFramework.Entities.IdentityResource> entities = identityResources.Select(ressource => ressource.ToEntity());
                configurationDbContext.IdentityResources.AddRange(entities);
            }
            #endregion

            if (!await configurationDbContext.Clients.AnyAsync(client => client.ClientId == "C95335E5814247ECAC857646BB5676D5"))
            {
                Client testClient = new()
                {
                    AllowedCorsOrigins = { "https://localhost:7141" },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "inventory.delete",
                    "inventory.update",
                    "inventory.read",
                    "inventory.create"

                },
                    ClientId = "C95335E5814247ECAC857646BB5676D5",
                    RequireClientSecret = false,
                    ClientName = "API Swagger test client",
                    RedirectUris = { "https://localhost:7141/swagger/oauth2-redirect.html" },
                };
                configurationDbContext.Clients.Add(testClient.ToEntity());
            }
            if (!await configurationDbContext.Clients.AnyAsync(client => client.ClientId == "A7B29F3D12E654C8F0932167D4E8A0B1"))
            {
                Client client = new()
                {
                    AllowedCorsOrigins = { "https://localhost:8433" },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "inventory.delete",
                    "inventory.update",
                    "inventory.read",
                    "inventory.create"

                },
                    ClientId = "A7B29F3D12E654C8F0932167D4E8A0B1",
                    RequireClientSecret = false,
                    ClientName = "Front - StockTracker.Client",
                    RedirectUris = { "https://localhost:8433/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:8433/signout-callback-oidc" }
                };
                configurationDbContext.Clients.Add(client.ToEntity());
            }
            if (!await configurationDbContext.Clients.AnyAsync(client => client.ClientId == "78D9E2F100D049E8A46477CEFC811C49"))
            {
                Client mvcWebApplication = new()
                {
                    AllowedCorsOrigins = { "https://localhost:5001" },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "read",
                    "write"
                },
                    ClientId = "78D9E2F100D049E8A46477CEFC811C49",
                    ClientName = "MVC Web Application",
                    ClientSecrets = { new Secret("22435308B4C04FD9B4886BC4457F66B445AB50DF52E34F3C904B841F380F7225".ToSha256()) },
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                    RedirectUris = { "https://localhost:5001/signin-oidc" }
                };
                configurationDbContext.Clients.Add(mvcWebApplication.ToEntity());
            }
            #region StocktrackerScopes
            if (!await configurationDbContext.ApiScopes.AnyAsync(scope => scope.Name == "inventory.update"))
            {
                ApiScope booksRead = new()
                {
                    Name = "inventory.update",
                    DisplayName = "inventory.update",
                    Description = "Grant access to updating inventory"
                };
                configurationDbContext.ApiScopes.Add(booksRead.ToEntity());
            }
            if (!await configurationDbContext.ApiScopes.AnyAsync(scope => scope.Name == "inventory.read"))
            {
                ApiScope booksRead = new()
                {
                    Name = "inventory.read",
                    DisplayName = "inventory.read",
                    Description = "Grant access to reading inventory"
                };
                configurationDbContext.ApiScopes.Add(booksRead.ToEntity());
            }
            if (!await configurationDbContext.ApiScopes.AnyAsync(scope => scope.Name == "inventory.delete"))
            {
                ApiScope booksRead = new()
                {
                    Name = "inventory.delete",
                    DisplayName = "inventory.delete",
                    Description = "Grant access to deleting inventory"
                };
                configurationDbContext.ApiScopes.Add(booksRead.ToEntity());
            }
            if (!await configurationDbContext.ApiScopes.AnyAsync(scope => scope.Name == "inventory.create"))
            {
                ApiScope booksRead = new()
                {
                    Name = "inventory.create",
                    DisplayName = "inventory.create",
                    Description = "Grant access to creating inventory item"
                };
                configurationDbContext.ApiScopes.Add(booksRead.ToEntity());
            }
            #endregion

            try
            {
                await configurationDbContext.Database.BeginTransactionAsync();
                await configurationDbContext.SaveChangesAsync();
                await configurationDbContext.Database.CommitTransactionAsync();
            }
            catch
            {
                await configurationDbContext.Database.RollbackTransactionAsync();
            }
        }
    }
}
