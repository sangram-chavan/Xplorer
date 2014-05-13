using System.Web;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;

namespace WebSite.Core
{
    /// <summary>
    /// Summary description for connector
    /// </summary>
    public class Connector : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            var dirname = context.Request.Form["ud"].DecodeFromBase64();
            FileManagerConfig cfg = null;
            //if (AppConfig.FileSystemMode == FileSystemMode.DatabaseFileSystem)
            //{
            //    cfg = new FileManagerConfig{root = dirname, rootAlias = PathHelper.GetFileNameWithoutExtension(dirname)};
            //}
            //else
            //{
            //    cfg = new FileManagerConfig { root = dirname, rootAlias = PathHelper.GetFileNameWithoutExtension(dirname), enableThumbnails = false, tmbDir = "", fileURL = true };
            //}
            cfg = new FileManagerConfig { root = dirname, rootAlias = PathHelper.GetFileNameWithoutExtension(dirname) };
            new FileManager(cfg).run();

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}