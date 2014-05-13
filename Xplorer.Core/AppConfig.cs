using System.Configuration;
using Xplorer.Core.Extensions;

namespace Xplorer.Core
{

    public class AppConfig
    {

        public static FileSystemMode FileSystemMode
        {
            get
            {
                var val = ConfigurationManager.AppSettings["FileSystem"] ?? "WindowsFileSystem";
                switch (val.ToLower().Trim())
                {
                    case "databasefilesystem":
                        return FileSystemMode.DatabaseFileSystem;
                    default:
                        return FileSystemMode.WindowsFileSystem;
                }
            }
        }
        public static string FileSystemRootPath
        {
            get
            {
                var path = (ConfigurationManager.AppSettings["FileSystemRootPath"] ?? "Uploads").Trim(new char[] { '/', '\\' });
                return path.ReplaceConsecutiveSlashes("\\");
            }
        }


    }
}
