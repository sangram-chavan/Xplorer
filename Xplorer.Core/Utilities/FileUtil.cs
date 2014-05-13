using System.IO;

namespace Xplorer.Core.Utilities
{
    public static class FileUtil
    {
        public static bool CopyAllFiles(string sourceFolder, string destinationFolder)
        {
            try
            {
                if (!Directory.Exists(sourceFolder))
                    return false;
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);
                DirectoryInfo sourceDirectory = new DirectoryInfo(sourceFolder);
                DirectoryInfo targetDirectory = new DirectoryInfo(destinationFolder);
                foreach (var file in sourceDirectory.GetFiles())
                {
                    file.CopyTo(Path.Combine(destinationFolder, file.Name), true);
                }
                foreach (DirectoryInfo diSourceDir in sourceDirectory.GetDirectories())
                {
                    DirectoryInfo nextTargetDir = targetDirectory.CreateSubdirectory(diSourceDir.Name);
                    CopyAllFiles(diSourceDir.FullName, nextTargetDir.FullName);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
