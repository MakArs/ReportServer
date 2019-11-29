using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Nancy;
using Nancy.Responses;

namespace ReportService.Nancy
{
    public static class NancyExtensions
    {
        public static void AddEmbeddedDirectory<T>(this IList<Func<NancyContext, string, Response>> conventions,
            string requestPath, string embeddedPath)
            => conventions.Add(EmbeddedContentConventionBuilder.AddEmbeddedDirectory<T>(requestPath, embeddedPath));
    }

    public static class EmbeddedContentConventionBuilder
    {
        public static Func<NancyContext, string, Response> AddEmbeddedDirectory<T>(string requestPath,
            string embedDirectory)
        {
            return (ctx, root) =>
            {
                var path = ctx.Request.Url.Path;
                if (!path.StartsWith(requestPath))
                    return null;
                var filename = Path.GetFileName(ctx.Request.Url.Path);
                if (string.IsNullOrEmpty(filename))
                    return HttpStatusCode.NotFound;

                var manifestEmbeddedProvider =
                    new ManifestEmbeddedFileProvider(typeof(Program).Assembly);

                var contents = manifestEmbeddedProvider.GetDirectoryContents(embedDirectory);
                var fileInfo = contents.FirstOrDefault(finf => finf.Name == filename);
                if (fileInfo == null)
                    return HttpStatusCode.NotFound;
                var stream = fileInfo.CreateReadStream();

                if (stream == null)
                    return HttpStatusCode.NotFound;

                return new StreamResponse(() => stream, MimeTypes.GetMimeType(filename));
            };
        }
    }
}
