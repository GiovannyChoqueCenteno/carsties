﻿using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp","Aucion App Full Access"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId="postman",
                ClientName="Postman",
                AllowedScopes = {"openid","profile","auctionApp"},
                RedirectUris={"https://getpostman.com/oauth2/callback"},
                ClientSecrets=new[]{
                    new Secret("NotASecret".Sha256())
                },
                AllowedGrantTypes={GrantType.ResourceOwnerPassword}
            },
            new Client{
                ClientId = "nextApp",
                ClientName= "nextApp",
                ClientSecrets = {
                    new Secret("secret".Sha256())
                },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                RedirectUris = {"https://localhost:3000/api/auth/callback/id-server"},
                AllowOfflineAccess = true,
                AllowedScopes = {"openid","profile","auctionApp"},
                AccessTokenLifetime = 3600 * 24 *30
            }
        };
}
