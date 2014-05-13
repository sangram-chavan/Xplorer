using Xplorer.Core.Extensions;
using System;
using System.Linq;
using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public abstract class FileSystemInfo
    {
        private IFileSystemRepository _baseRepository;
        protected IFileSystemRepository BaseRepository
        {
            get
            {
                return _baseRepository ?? (_baseRepository = FileManagerUtil.BaseRepository);
            }
        }

        public string Name { get; private set; }
        public string FullName { get; private set; }
        public System.IO.FileAttributes Attributes { get; internal set; }
        public System.IO.FileAccess Permissions { get; internal set; }
        public DateTime CreationTime
        {
            get
            {
                return CreationTimeUtc.ToLocalTime();
            }
            internal set
            {
                CreationTimeUtc = value.ToUniversalTime();
            }
        }
        public DateTime CreationTimeUtc { get; internal set; }
        public DateTime LastWriteTime
        {
            get
            {
                return LastWriteTimeUtc.ToLocalTime();
            }
            internal set
            {
                LastWriteTimeUtc = value.ToUniversalTime();
            }
        }
        public DateTime LastWriteTimeUtc { get; internal set; }

        public bool Exists { get; internal set; }
        public long OriginalSize { get; internal set; }
        public bool IsDirectory { get; internal set; }

        internal protected FileSystemInfo(IFileSystemEntry info)
        {
            Exists = false;
            if (info == null) return;
            if (this is DirectoryInfo)
            {
                var item = this as DirectoryInfo;
                if (item != null)
                {
                    item.Attributes = info.Attributes;
                    item.Exists = true;
                    item.CreationTimeUtc = info.CreationTime;
                    item.FullName = info.FullName;
                    item.IsDirectory = true;
                    item.LastWriteTimeUtc = info.LastModifiedTime;
                    item.Name = info.Name;
                    item.OriginalSize = info.OriginalSize;
                    item.Permissions = info.Permissions;
                }
            }
            if (this is FileInfo)
            {
                var item = this as FileInfo;
                if (item != null)
                {
                    item.Attributes = info.Attributes;
                    item.Exists = true;
                    item.CreationTimeUtc = info.CreationTime;
                    item.FullName = info.FullName;
                    item.IsDirectory = false;
                    item.LastWriteTimeUtc = info.LastModifiedTime;
                    item.Name = info.Name;
                    item.OriginalSize = info.OriginalSize;
                    item.Permissions = info.Permissions;
                    item.Extension = info.Extension;
                    item.IsEncrypted = info.IsEncrypted;
                    item.IsZipped = info.IsZipped;
                    item.Data = info.Data == null ? null : info.Data.ToArray();
                }
            }
        }

        protected static IFileSystemRepository BaseRepo
        {
            get
            {
                return FileManagerUtil.BaseRepository;
            }
        }

        public static FileSystemInfo GetEntry(string path)
        {
            path = PathHelper.GetFullPath(path);
            string parentDir = PathHelper.GetDirectoryName(path);
            string name = PathHelper.GetFileName(path);
            if (string.IsNullOrEmpty(parentDir) || string.IsNullOrEmpty(name))
                return null;

            var info = BaseRepo.GetObject(path);
            if (info == null)
                return null;
            if (info.IsDirectory)
                return new DirectoryInfo(info);
            return new FileInfo(info);
        }

        public bool Delete()
        {
            try
            {
                if (this.Exists)
                {
                    if (this.IsDirectory) Directory.Delete(this.FullName);
                    else File.Delete(this.FullName);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected FileSystemInfo(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            Exists = false;
            FullName = path;
            path = PathHelper.GetFullPath(path);
            string parentDir = PathHelper.GetDirectoryName(path);
            string name = PathHelper.GetFileName(path);
            if (string.IsNullOrEmpty(parentDir) || string.IsNullOrEmpty(name))
                return;
            IFileSystemEntry info = BaseRepository.GetObject(path);
            if (info == null) return;
            if (this is DirectoryInfo)
            {
                var item = this as DirectoryInfo;
                if (item != null)
                {
                    item.Attributes = info.Attributes;
                    item.Exists = true;
                    item.CreationTimeUtc = info.CreationTime;
                    item.FullName = info.FullName;
                    item.IsDirectory = true;
                    item.LastWriteTimeUtc = info.LastModifiedTime;
                    item.Name = info.Name;
                    item.OriginalSize = info.OriginalSize;
                    item.Permissions = info.Permissions;
                }
            }
            if (this is FileInfo)
            {
                var item = this as FileInfo;
                if (item != null)
                {
                    item.Attributes = info.Attributes;
                    item.Exists = true;
                    item.CreationTimeUtc = info.CreationTime;
                    item.FullName = info.FullName;
                    item.IsDirectory = true;
                    item.LastWriteTimeUtc = info.LastModifiedTime;
                    item.Name = info.Name;
                    item.OriginalSize = info.OriginalSize;
                    item.Permissions = info.Permissions;
                    item.Extension = info.Extension;
                    item.IsEncrypted = info.IsEncrypted;
                    item.IsZipped = info.IsZipped;
                    item.Data = info.Data == null ? null : info.Data.ToArray();
                }
            }
        }
    }
}

