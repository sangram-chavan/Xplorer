using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity;
using Xplorer.Core.Repository;

namespace Xplorer.Core
{

    public class FileManagerUtil
    {
        static FileManagerUtil()
        {
            var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            Container = new UnityContainer();
            section.Containers["root"].Configure(Container);
        }

        static readonly IUnityContainer Container = null;

        public static IFileSystemRepository BaseRepository
        {
            get { return Container.Resolve<IFileSystemRepository>(); }
        }

        public static IFileSystemRepository InternalFileSystem
        {
            get { return Container.Resolve<IFileSystemRepository>("Base"); }
        }
        
    }
}