using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;
using System.IO;
using Xplorer.Core.Utilities;
using Directory = System.IO.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;
using File = System.IO.File;
using FileInfo = System.IO.FileInfo;
using FileSystemInfo = System.IO.FileSystemInfo;

namespace Xplorer.Core.Repository
{
    public class FileSystemRepository : IFileSystemRepository
    {
        private string _root = string.Empty;
        public string Root
        {
            get { return _root; }
        }


        public FileSystemRepository()
        {
            this._root = PathHelper.Combine(HttpContext.Current.Server.MapPath("~//"), AppConfig.FileSystemRootPath);
        }

        private IFileSystemEntry GetFileSystemEntry(FileSystemInfo retVal)
        {
            IFileSystemEntry info = null;
            if (retVal != null)
            {
                info = new FileSystemEntry();
                info.CreationTime = retVal.CreationTimeUtc;
                info.Extension = retVal.Extension;
                info.FullName = retVal.FullName;
                info.IsDirectory = (retVal.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
                info.Attributes = retVal.Attributes;
                info.IsEncrypted = (retVal.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted;
                info.IsZipped = (retVal.Attributes & FileAttributes.Compressed) == FileAttributes.Compressed;
                info.LastModifiedTime = retVal.LastAccessTimeUtc;
                info.Name = retVal.Name;
                if (!info.IsDirectory && retVal is FileInfo)
                {
                    info.Data = File.ReadAllBytes(retVal.FullName);
                    info.OriginalSize = ((FileInfo)retVal).Length;
                }
                info.Permissions = (retVal.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;

            }
            return info;
        }

        public IFileSystemEntry GetObject(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists && (dirInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return GetFileSystemEntry(dirInfo);
            }
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                return GetFileSystemEntry(fileInfo);
            return null;
        }

        public bool DirectoryExists(string path)
        {
            path = PathHelper.GetFullPath(path);
            return Directory.Exists(path);
        }

        public bool CreateDirectory(string path)
        {
            path = PathHelper.GetFullPath(path);
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<IFileSystemEntry> GetFileInfos(string path, Regex pattern, bool recursive)
        {
            SearchOption opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            return new DirectoryInfo(path).GetFiles("*", opt).Where(o => pattern.IsMatch(o.Name)).Select(GetFileSystemEntry);
        }

        public IEnumerable<IFileSystemEntry> GetDirectoryInfos(string path, Regex pattern, bool recursive)
        {
            SearchOption opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            return new DirectoryInfo(path).GetDirectories("*", opt).Where(o => pattern.IsMatch(o.Name)).Select(GetFileSystemEntry);
        }

        public IEnumerable<IFileSystemEntry> GetFileSystemInfos(string path, Regex pattern, bool recursive)
        {
            var lstEntries = new List<IFileSystemEntry>();
            lstEntries.AddRange(GetDirectoryInfos(path, pattern, recursive));
            lstEntries.AddRange(GetFileInfos(path, pattern, recursive));
            return lstEntries;
        }

        public IEnumerable<string> GetFiles(string path, Regex pattern, bool recursive)
        {
            return GetFileInfos(path, pattern, recursive).Select(o => o.FullName);
        }

        public IEnumerable<string> GetDirectories(string path, Regex pattern, bool recursive)
        {
            return GetDirectoryInfos(path, pattern, recursive).Select(o => o.FullName);
        }

        public IEnumerable<string> GetFileSystemEntries(string path, Regex pattern, bool recursive)
        {
            return GetFileSystemInfos(path, pattern, recursive).Select(o => o.FullName);
        }

        public bool CopyDirectory(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);
            try
            {
                if (!IO.Directory.Exists(source))
                    return false;
                if (!IO.Directory.Exists(target))
                    IO.Directory.CreateDirectory(target);

                var lstFiles = new IO.DirectoryInfo(source).GetFileSystemInfos(RegexUtil.MatchIfStartsWithoutDot, true);
                lstFiles = lstFiles.OrderByDescending(o => o.IsDirectory);
                lstFiles.ForEach(o =>
                                     {
                                         var newPath = o.FullName.Replace(source, target);
                                         if (o.IsDirectory)
                                             IO.Directory.CreateDirectory(newPath);
                                         else
                                             IO.File.Create(newPath, ((IO.FileInfo) o).Data);
                                     });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool MoveDirectory(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);
            try
            {
                if (DirectoryExists(source) && !DirectoryExists(target))
                {
                    Directory.Move(source, target);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteDirectory(string path)
        {
            path = PathHelper.GetFullPath(path);
            try
            {
                if (DirectoryExists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public DateTime GetLastWriteTime(string path)
        {
            path = PathHelper.GetFullPath(path);
            return Directory.GetLastAccessTimeUtc(path);
        }

        public bool FileExists(string path)
        {
            path = PathHelper.GetFullPath(path);
            return File.Exists(path);
        }

        public void CopyFile(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);
            if (FileExists(source))
            {
                var tgtparentDir = PathHelper.GetDirectoryName(target);
                if (!DirectoryExists(tgtparentDir))
                    CreateDirectory(tgtparentDir);

                File.Copy(source, target, true);
            }
        }

        public void CreateFile(string path, byte[] fileContent)
        {
            path = PathHelper.GetFullPath(path);

            var tgtparentDir = PathHelper.GetDirectoryName(path);
            if (!DirectoryExists(tgtparentDir))
                CreateDirectory(tgtparentDir);
            if (!FileExists(path))
            {
                using (var stream = File.Create(path))
                {
                    stream.Write(fileContent, 0, fileContent.Length);
                    stream.Close();
                }
                File.SetAttributes(path, FileAttributes.Normal);
            }
        }

        public void DeleteFile(string path)
        {
            path = PathHelper.GetFullPath(path);
            if (FileExists(path))
            {
                File.Delete(path);
            }
        }

        public void MoveFile(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);

            if (FileExists(source) && !FileExists(target))
            {
                var tgtparentDir = PathHelper.GetDirectoryName(target);
                if (!DirectoryExists(tgtparentDir))
                    CreateDirectory(tgtparentDir);
                File.Move(source, target);
            }
        }

        public byte[] ReadFile(string path)
        {
            path = PathHelper.GetFullPath(path);
            if (FileExists(path))
                return File.ReadAllBytes(path);
            return null;
        }

        public void WriteFile(string path, byte[] fileContent)
        {
            path = PathHelper.GetFullPath(path);
            var tgtparentDir = PathHelper.GetDirectoryName(path);
            if (!DirectoryExists(tgtparentDir))
                CreateDirectory(tgtparentDir);
            File.WriteAllBytes(path, fileContent);
        }

        public void Dispose()
        {

        }
    }
}