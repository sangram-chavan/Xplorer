using System;

namespace Xplorer.Core.Repository
{
    public class FileSystemEntry : IFileSystemEntry
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public byte[] Data { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public DateTime CreationTime { get; set; }
        public System.IO.FileAttributes Attributes { get; set; }
        public string Extension { get; set; }
        public bool IsZipped { get; set; }
        public bool IsEncrypted { get; set; }
        public long OriginalSize { get; set; }
        public System.IO.FileAccess Permissions { get; set; }
        public string FullName { get; set; }

        public object Clone()
        {
            return new FileSystemEntry
                       {
                    Attributes = Attributes,
                    CreationTime = CreationTime,
                    Data = Data,
                    Extension = Extension,
                    FullName = FullName,
                    IsDirectory = IsDirectory,
                    IsEncrypted = IsEncrypted,
                    IsZipped = IsZipped,
                    LastModifiedTime = LastModifiedTime,
                    Name = Name,
                    OriginalSize = OriginalSize,
                    Permissions = Permissions
                };
        }
    }
}