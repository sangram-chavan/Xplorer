using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public class FileInfo : FileSystemInfo
    {
        public string Extension { get;  set; }
        public bool IsZipped { get;  set; }
        public bool IsEncrypted { get;  set; }
        public byte[] Data { get;  set; }

        public FileInfo(string path)
            : base(path)
        {
        }
        internal protected FileInfo(IFileSystemEntry item)
            : base(item)
        {
        }
    }
}