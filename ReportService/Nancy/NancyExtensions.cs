using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Domain0.Service.Tokens;
using Domain0.Tokens;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Newtonsoft.Json;

namespace ReportService.Nancy
{
    public static class NancyExtensions
    {
        public static void AddDomain0Auth(this IPipelines pipelines, TokenValidationSettings settings)
        {
            StatelessAuthentication.Enable(
                pipelines,
                BuildAuthConfiguration(settings));
        }

        private const string TokenPrefix = "Bearer ";

        private static StatelessAuthenticationConfiguration BuildAuthConfiguration(TokenValidationSettings settings)
        {
            var configuration = new StatelessAuthenticationConfiguration(
                ctx =>
                {
                    try
                    {
                        var authorization = ctx.Request.Headers.Authorization;

                        if (string.IsNullOrWhiteSpace(authorization))
                            return null;

                        if (!authorization.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
                            return null;

                        var jwtToken = authorization.Remove(0, TokenPrefix.Length);

                        var handler = new JwtSecurityTokenHandler { SetDefaultTimesOnTokenCreation = false };

                        var principal = handler.ValidateToken(
                            jwtToken,
                            settings.BuildTokenValidationParameters(),
                            out _);

                        ParsePermissions(principal, jwtToken);

                        return principal;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

            return configuration;
        }

        private static void ParsePermissions(ClaimsPrincipal principal, string jwtToken)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            identity.AddClaim(new Claim("id_token", jwtToken));
            foreach (var role in principal.FindAll(TokenClaims.CLAIM_PERMISSIONS))
            {
                foreach (var permission in JsonConvert.DeserializeObject<string[]>(role.Value))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, permission));
                }
            }

            var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (subClaim != null)
                identity.AddClaim(new Claim(ClaimTypes.Name, subClaim.Value));
        }
    }


    public static class EmbeddedContentConventionBuilderExtensions
    {
        public static void AddEmbeddedDirectory<T>(this IList<Func<NancyContext, string, Response>> conventions,
            string requestPath, string embeddedPath)
            => conventions.Add(EmbeddedContentConventionBuilder.AddEmbeddedDirectory<T>(requestPath, embeddedPath));
    }

    public static class EmbeddedContentConventionBuilder
    {
        public static Func<NancyContext, string, Response> AddEmbeddedDirectory<T>(string requestPath, string embedDirectory)
        {
            return (ctx, root) =>
            {
                var path = ctx.Request.Url.Path;
                if (!path.StartsWith(requestPath))
                    return null;

                var assembly = Assembly.GetExecutingAssembly();
                var filename = Path.GetFileName(ctx.Request.Url.Path);
                if (string.IsNullOrEmpty(filename))
                    return HttpStatusCode.NotFound;

                var pathParts = string.Concat(embedDirectory, path.Substring(requestPath.Length)).Split('/');
                if (pathParts.Length == 0)
                    return HttpStatusCode.NotFound;

                var embeddedPath = GetEmbeddedPath<T>(pathParts);
                var stream = assembly.GetManifestResourceStream(embeddedPath);
                if (stream == null)
                    return HttpStatusCode.NotFound;

                return new StreamResponse(() => stream, MimeTypes.GetMimeType(filename));
            };
        }

        private static string GetEmbeddedPath<T>(params string[] parts)
        {
            var path = string.Join(".", parts.Take(parts.Length - 1).Select(p => p.Replace("-", "_")));
            if (!string.IsNullOrEmpty(path))
                return $"{typeof(T).Namespace}.{path}.{parts.Last()}";

            return $"{typeof(T).Namespace}.{parts.Last()}";
        }
    }
}

