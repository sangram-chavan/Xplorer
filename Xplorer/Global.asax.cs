using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Xplorer.Core;
using Xplorer.Core.Extensions;

namespace Xplorer
{
    public class Global : HttpApplication
    {

        private static void RegisterRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = false;

            var routes = RouteTable.Routes;

            if (AppConfig.FileSystemMode == FileSystemMode.DatabaseFileSystem)
            {
                routes.Add(new Route("{*data}", new DbFileSystemRouteHandler()));

                //routes.Add(new Route("dfs/" + AppConfig.FileSystemRootPath.ReplaceConsecutiveSlashes("/") + "/{*data}",
                //            new RouteValueDictionary(),                                                  //Defaults
                //            new RouteValueDictionary(),                                                  //Constraint
                //            new RouteValueDictionary(),                                                  //DataTokens
                //            new DbFileSystemRouteHandler()));
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();

        }
    }

}