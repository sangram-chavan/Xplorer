using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xplorer.Core.Repository
{
    public interface IFileSystemRepository : IDisposable
    {
        string Root { get;}
        IFileSystemEntry GetObject(string path);
        bool DirectoryExists(string path);
        bool CreateDirectory(string path);
        IEnumerable<IFileSystemEntry> GetFileInfos(string path, Regex pattern, bool recursive);
        IEnumerable<IFileSystemEntry> GetDirectoryInfos(string path, Regex pattern,bool recursive);
        IEnumerable<IFileSystemEntry> GetFileSystemInfos(string path, Regex pattern, bool recursive);
        IEnumerable<string> GetFiles(string path, Regex pattern, bool recursive);
        IEnumerable<string> GetDirectories(string path, Regex pattern, bool recursive);
        IEnumerable<string> GetFileSystemEntries(string path, Regex pattern, bool recursive);
        bool CopyDirectory(string source, string target);
        bool MoveDirectory(string source, string target);
        bool DeleteDirectory(string path);
        DateTime GetLastWriteTime(string path);
        bool FileExists(string path);
        void CopyFile(string source, string target);
        void CreateFile(string path, byte[] fileContent);
        void DeleteFile(string path);
        void MoveFile(string source, string target);
        byte[] ReadFile(string path);
        void WriteFile(string path, byte[] fileContent);
    }
}
