
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public class DirectoryInfo : FileSystemInfo
    {
        public DirectoryInfo(string path)
            : base(path)
        {
        }
        internal protected DirectoryInfo(IFileSystemEntry item)
            : base(item)
        {
        }
        public IEnumerable<FileSystemInfo> GetFileSystemInfos(Regex pattern)
        {
            return GetFileSystemInfos(pattern,false);
        }

        public IEnumerable<FileSystemInfo> GetFileSystemInfos(Regex pattern,bool recursive)
        {
            var lstItems = new List<FileSystemInfo>();
            if (Exists)
            {
                BaseRepository.GetFileSystemInfos(FullName, pattern, recursive).ToList().ForEach(o =>
                {
                    if (o.IsDirectory)
                        lstItems.Add(new DirectoryInfo(o));
                    else
                        lstItems.Add(new FileInfo(o));
                });

            }
            return lstItems;
        }

        public IEnumerable<DirectoryInfo> GetDirectories(Regex pattern)
        {
            var lstItems = new List<DirectoryInfo>();
            if (Exists)
            {
                BaseRepository.GetDirectoryInfos(FullName, pattern, false).ToList().ForEach(o =>
                {
                    if (o.IsDirectory)
                        lstItems.Add(new DirectoryInfo(o));
                });
            }
            return lstItems;
        }

        public IEnumerable<FileInfo> GetFiles(Regex pattern, bool recursive)
        {
            var lstItems = new List<FileInfo>();
            if (Exists)
            {
                BaseRepository.GetFileInfos(FullName, pattern, recursive).ToList().ForEach(o =>
                {
                    if (!o.IsDirectory)
                        lstItems.Add(new FileInfo(o));
                });

            }
            return lstItems;
        }
    }
}