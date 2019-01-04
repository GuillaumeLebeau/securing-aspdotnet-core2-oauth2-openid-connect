using System.Collections.Generic;
using System.Security.Claims;

using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Marvin.IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Main Road 1"),
                        new Claim("role", "FreeUser"),
                        new Claim("subscriptionlevel", "FreeUser"),
                        new Claim("country", "nl")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Big Street 2"),
                        new Claim("role", "PayingUser"),
                        new Claim("subscriptionlevel", "PayingUser"),
                        new Claim("country", "be")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
            yield return new IdentityResources.Address();
            yield return new IdentityResource("roles", "Your role(s)", new[] {"role"});
            yield return new IdentityResource(
                "country",
                "The country you're living in",
                new[] {"country"});

            yield return new IdentityResource(
                "subscriptionlevel",
                "Your subscription level",
                new[] {"subscriptionlevel"});
        }

        // api-related resources (scopes)
        public static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource("imagegalleryapi", "Image Gallery API", new[] {"role"})
            {
                ApiSecrets = {new Secret("apisecret".Sha256())}
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            yield return new Client
            {
                ClientName = "Image Gallery",
                ClientId = "imagegalleryclient",
                AllowedGrantTypes = GrantTypes.Hybrid,
                AccessTokenType = AccessTokenType.Reference,
                //IdentityTokenLifetime = ...
                //AuthorizationCodeLifetime = = ...
                AccessTokenLifetime = 120,
                AllowOfflineAccess = true,
                //AbsoluteRefreshTokenLifetime = ...
                UpdateAccessTokenClaimsOnRefresh = true,
                RedirectUris = new List<string> {"https://localhost:5003/signin-oidc"},
                PostLogoutRedirectUris =
                    new List<string> {"https://localhost:5003/signout-callback-oidc"},
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    "roles",
                    "imagegalleryapi",
                    "country",
                    "subscriptionlevel"
                },
                ClientSecrets = {new Secret("secret".Sha256())}
            };
        }
    }
}
