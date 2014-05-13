using System.Web.Routing;

namespace Xplorer.Core
{
    public class DbFileSystemRouteHandler : IRouteHandler
    {
        public System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new DbFileHandler();
        }
    }
}
