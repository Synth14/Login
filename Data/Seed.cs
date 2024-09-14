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
