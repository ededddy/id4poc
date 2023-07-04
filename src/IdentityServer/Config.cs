// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope(name: "ApiOne.read",   displayName: "Read your data.",userClaims:new string[]{"scope.claim" }),
                new ApiScope(name: "ApiOne.write",   displayName: "write your data."),
                new ApiScope(name: "ApiOne.all",   displayName: "management apione"),
            };
        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            { 

                new ApiResource(name:"ApiOne",displayName:"ApiOneResource",userClaims:new string[]{ "role.apione" })
                {
                    Scopes = {"ApiOne.Read"}
                },
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>()
            { new Client()
                {
                    ClientId = "userjs",
                    ClientSecrets = {new Secret("secret".ToSha256())},
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {"http://localhost:5500/postAuth.html" },
                    RequireConsent = false,
                    AllowedCorsOrigins = {"https://localhost:5500", "https://localhost:5500/postAuth.html", "https://localhost:5500/index.html"},
                    PostLogoutRedirectUris = { "http://localhost:5500/logout" },
                    AllowedScopes =  {
                        "ApiOne.read",
                    },
                    RequireClientSecret = false,
                }
            };
        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim(JwtClaimTypes.Name, "sub"),
                        new Claim(JwtClaimTypes.Name, "profile"),
                        new Claim(JwtClaimTypes.Name, "public_profile"),
                        new Claim(JwtClaimTypes.Name, "email"),
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                }
            };
    }
}