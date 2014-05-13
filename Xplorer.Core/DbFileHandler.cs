using System;
using System.Net;
using System.Web;
using Xplorer.Core.IO;

namespace Xplorer.Core
{
    public class DbFileHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string physicalPath = PathHelper.MapPath(HttpUtility.UrlDecode(context.Request.Url.AbsolutePath));
            var fileInfo = new FileInfo(physicalPath);

            context.Response.ContentType = MimeMapping.GetMimeMapping(physicalPath);
            context.Response.AppendHeader("Accept-Ranges", "bytes");
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(1.0));
            context.Response.Cache.SetLastModified(fileInfo.LastWriteTimeUtc);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            if (fileInfo.Exists && fileInfo.Data != null)
            {
                context.Response.OutputStream.Write(fileInfo.Data, 0, fileInfo.Data.Length);
            }
            else
            {
                var exception = new Exception("File Not Found");
                exception.Data.Add("RequestUrl", context.Request.Url.AbsolutePath);

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.StatusDescription = "Not Found";
            }
        }
    }
}
