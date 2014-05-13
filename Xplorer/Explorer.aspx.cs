using System;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;
using Xplorer.Core;

namespace Xplorer
{
    public partial class Explorer : System.Web.UI.Page
    {
        public string BaseDirectory
        {
            get
            {
                return PathHelper.MapPath("~/").ReplaceConsecutiveSlashes("\\").EncodeToBase64();
            }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            string createIfNotExist = Request.QueryString["cd"];
            if (string.Compare(createIfNotExist, "Y", true) == 0)
            {
                string path = BaseDirectory;
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
            }
            this.Theme = string.Empty;
        }
    }


}
