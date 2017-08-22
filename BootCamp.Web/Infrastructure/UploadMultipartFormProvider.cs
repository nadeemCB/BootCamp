using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Web.Infrastructure
{
    public class UploadMultipartFormProvider: MultipartFormDataStreamProvider
    {
        public UploadMultipartFormProvider(string rootPath) : base(rootPath) { }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            var extensions = new[] { "png", "gif", "jpg" };
            var filename = headers.ContentDisposition.FileName.Replace("\"", string.Empty);

            if (filename.IndexOf('.') < 0)
                return Stream.Null;

            var extension = filename.Split('.').Last();

            return extensions.Any(i => i.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                       ? base.GetStream(parent, headers)
                       : Stream.Null;
        }
        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            string oldfileName = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(oldfileName);
            return newFileName;
        }
    }
}
