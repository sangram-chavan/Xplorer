using System;

namespace Xplorer.Core.Repository
{
    public interface IFileSystemEntry : ICloneable
    {
        string Name { get; set; }

        bool IsDirectory { get; set; }

        byte[] Data { get; set; }

        DateTime LastModifiedTime { get; set; }

        DateTime CreationTime { get; set; }

        System.IO.FileAttributes Attributes { get; set; }

        string Extension { get; set; }

        bool IsZipped { get; set; }

        bool IsEncrypted { get; set; }

        long OriginalSize { get; set; }

        System.IO.FileAccess Permissions { get; set; }

        string FullName { get; set; }
    }
}
