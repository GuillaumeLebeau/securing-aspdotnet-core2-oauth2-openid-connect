using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using Flurl.Http;

using IdentityModel.Client;

using ImageGallery.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryApiClient : IImageGalleryApiClient
    {
        private readonly IFlurlClient _flurlClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageGalleryApiClient(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _flurlClient = new FlurlClient(httpClientFactory.CreateClient("api_client"));
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Image>> GetImages()
        {
            return await _flurlClient
                .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .Request("api", "images")
                .GetJsonAsync<List<Image>>()
                .ConfigureAwait(false);
        }

        public async Task<Image> GetImage(Guid id)
        {
            return await _flurlClient
                .WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .Request("api", "images", id)
                .GetJsonAsync<Image>()
                .ConfigureAwait(false);
        }

        public async Task UpdateImage(Guid id, ImageForUpdate imageForUpdate)
        {
            await _flurlClient.WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .Request("api", "images", id)
                .PutJsonAsync(imageForUpdate)
                .ConfigureAwait(false);
        }

        public async Task DeleteImage(Guid id)
        {
            await _flurlClient.WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .Request("api", "images", id)
                .DeleteAsync()
                .ConfigureAwait(false);
        }

        public async Task AddImage(ImageForCreation imageForCreation)
        {
            await _flurlClient.WithOAuthBearerToken(await GetAccessToken().ConfigureAwait(false))
                .Request("api", "images")
                .PostJsonAsync(imageForCreation)
                .ConfigureAwait(false);
        }

        private async Task<string> GetAccessToken()
        {
            string accessToken;

            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get access token
            // accessToken =
            //     await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            // should we renew access & refresh tokens?
            // get expires_at value
            var expiresAt = await currentContext.GetTokenAsync("expires_at").ConfigureAwait(false);

            // compare - make sure to use the exact date formats for comparison (UTC, in this case)
            if (string.IsNullOrEmpty(expiresAt)
                || DateTime.Parse(expiresAt).AddSeconds(-60).ToUniversalTime() < DateTime.UtcNow)
            {
                accessToken = await RenewTokens().ConfigureAwait(false);
            }
            else
            {
                accessToken = await currentContext
                    .GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
                    .ConfigureAwait(false);
            }

            return accessToken;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            var client = _httpClientFactory.CreateClient("idp_client");

            // get the metadata
            var disco = await client.GetDiscoveryDocumentAsync().ConfigureAwait(false);

            if (disco.IsError)
                throw new Exception(disco.Error, disco.Exception);

            // get the saved refresh token
            var currentRefreshToken = await currentContext
                .GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
                .ConfigureAwait(false);

            if (currentRefreshToken == null)
                return null;

            // refresh the tokens
            var tokenResult = await client.RequestRefreshTokenAsync(
                    new RefreshTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = "imagegalleryclient",
                        ClientSecret = "secret",
                        RefreshToken = currentRefreshToken
                    })
                .ConfigureAwait(false);

            if (tokenResult.IsError)
                throw new Exception(tokenResult.Error, tokenResult.Exception);

            // update the tokens & expiration value
            var updatedTokens = new List<AuthenticationToken>
            {
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = tokenResult.IdentityToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResult.AccessToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResult.RefreshToken
                }
            };

            var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
            updatedTokens.Add(
                new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                });

            // get authenticate result, containing the current principal & properties
            var currentAuthenticateResult =
                await currentContext.AuthenticateAsync("Cookies").ConfigureAwait(false);

            // store the updated tokens
            currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

            // sign in
            await currentContext.SignInAsync(
                    "Cookies",
                    currentAuthenticateResult.Principal,
                    currentAuthenticateResult.Properties)
                .ConfigureAwait(false);

            // return the new access token 
            return tokenResult.AccessToken;
        }
    }
}
