using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;
using Xplorer.Core.ThreadSafeUtilities;
using Xplorer.Core.Utilities;

namespace Xplorer.Core.Repository
{
    public class CacheFileSystem : IFileSystemRepository
    {
        readonly IFileSystemRepository _baseRepository;

        private static ThreadSafeSortedDictionary<string, IFileSystemEntry> FileCache
        {
            get { return _internalFileCache; }
        }
        private static ThreadSafeSortedDictionary<string, IFileSystemEntry> _internalFileCache;

        public string Root
        {
            get { return _baseRepository.Root; }
        }

        public CacheFileSystem()
        {
            _baseRepository = FileManagerUtil.InternalFileSystem;
            if (FileCache == null)
            {
                _internalFileCache = new ThreadSafeSortedDictionary<string, IFileSystemEntry>(StringComparer.InvariantCultureIgnoreCase);

                if (FileCache != null)
                {
                    FileCache[this._baseRepository.Root] = this._baseRepository.GetObject(this._baseRepository.Root);
                    var lstEntries = _baseRepository.GetFileSystemInfos(_baseRepository.Root, null, true);
                    lstEntries.ForEach(o => FileCache[o.FullName] = o);
                }
            }

        }

        private static readonly object Lock = new object();

        public void Refresh()
        {
            lock (Lock)
            {
                var tempCache = new ThreadSafeSortedDictionary<string, IFileSystemEntry>(StringComparer.InvariantCultureIgnoreCase);
                var repo = FileManagerUtil.InternalFileSystem;
                var lstEntries = repo.GetFileSystemInfos(repo.Root, null, true);
                lstEntries.ForEach(o => tempCache[o.FullName] = o);
                _internalFileCache = tempCache;
            }
        }

        public void Refresh(string dirPath)
        {
            lock (Lock)
            {
                var repo = FileManagerUtil.InternalFileSystem;
                var temp = repo.GetObject(dirPath);
                if (temp != null)
                {
                    FileCache[dirPath] = temp;
                    var lstEntries = repo.GetFileSystemInfos(dirPath, null, true);
                    lstEntries.ForEach(o => FileCache[o.FullName] = o);
                }
            }
        }

        public IFileSystemEntry GetObject(string path)
        {
            if (FileCache.ContainsKey(path))
                return FileCache[path].Clone() as IFileSystemEntry;
            return null;
        }

        public bool DirectoryExists(string path)
        {
            if (FileCache.ContainsKey(path))
                return FileCache[path].IsDirectory;
            return false;
        }

        public bool CreateDirectory(string path)
        {
            var bRetVal = _baseRepository.CreateDirectory(path);
            if (bRetVal)
            {
                //FileCache[path] = baseRepository.GetObject(path);
                while (GetObject(path) == null)
                {
                    FileCache[path] = _baseRepository.GetObject(path);
                    path = PathHelper.GetDirectoryName(path);
                }
            }
            return bRetVal;
        }

        public IEnumerable<IFileSystemEntry> GetFileInfos(string path, Regex pattern, bool recursive)
        {
            var regDir = new Regex(Regex.Escape(path) + @"\\[^\\]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            var lst = new List<IFileSystemEntry>();

            if (recursive)
            {
                var t = FileCache.Keys.Where(o => o.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) && FileCache[o] != null && FileCache[o].IsDirectory == false && pattern.IsMatch(FileCache[o].Name))
                    .Select(o => FileCache[o].Clone() as IFileSystemEntry).ToList();
                lst.AddRange(t);
            }
            else
            {
                var t = FileCache.Keys.Where(o => regDir.IsMatch(o) && FileCache[o] != null && FileCache[o].IsDirectory == false && pattern.IsMatch(FileCache[o].Name))
                    .Select(o => FileCache[o].Clone() as IFileSystemEntry).ToList();

                lst.AddRange(t);
            }
            return lst;
        }

        public IEnumerable<IFileSystemEntry> GetDirectoryInfos(string path, Regex pattern, bool recursive)
        {
            var regDir = new Regex(Regex.Escape(path) + @"\\[^\\]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            var lst = new List<IFileSystemEntry>();

            if (recursive)
            {
                var t = FileCache.Keys.Where(o => o.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) && FileCache[o] != null && FileCache[o].IsDirectory && pattern.IsMatch(FileCache[o].Name))
                                     .Select(o => FileCache[o].Clone() as IFileSystemEntry).ToList();
                lst.AddRange(t);
            }
            else
            {
                var t = FileCache.Keys.Where(o => regDir.IsMatch(o) && FileCache[o] != null && FileCache[o].IsDirectory && pattern.IsMatch(FileCache[o].Name))
                        .Select(o => FileCache[o].Clone() as IFileSystemEntry).ToList();
                lst.AddRange(t);
            }
            return lst;
        }

        public IEnumerable<IFileSystemEntry> GetFileSystemInfos(string path, Regex pattern, bool recursive)
        {
            var regDir = new Regex(Regex.Escape(path) + @"\\[^\\]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            var lst = new List<IFileSystemEntry>();

            if (recursive)
            {
                var t = FileCache.Keys.Where(o => o.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) && FileCache[o] != null && pattern.IsMatch(FileCache[o].Name))
                                     .Select(o => FileCache[o].Clone() as IFileSystemEntry).ToList();
                lst.AddRange(t);
            }
            else
            {
                var t = FileCache.Keys.Where(o => regDir.IsMatch(o) && FileCache[o] != null && pattern.IsMatch(FileCache[o].Name)).
                        Select(o => FileCache[o].Clone() as IFileSystemEntry);
                lst.AddRange(t);
            }
            return lst;
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
            var bRetVal = _baseRepository.CopyDirectory(source, target);
            if (bRetVal)
            {
                Refresh(target);
            }
            return bRetVal;
        }

        public bool MoveDirectory(string source, string target)
        {
            var bRetVal = _baseRepository.MoveDirectory(source, target);
            if (bRetVal)
            {
                var lst = FileCache.Keys.Where(o => o.StartsWith(source, StringComparison.InvariantCultureIgnoreCase)).ToList();
                lst.ForEach(o => FileCache.Remove(o));
                Refresh(target);
            }
            return bRetVal;
        }

        public bool DeleteDirectory(string path)
        {
            var bRetVal = _baseRepository.DeleteDirectory(path);
            if (bRetVal)
            {
                var lst = FileCache.Keys.Where(o => o.StartsWith(path + "\\", StringComparison.InvariantCultureIgnoreCase)).ToList();
                lst.ForEach(o => FileCache.Remove(o));
                FileCache.Remove(path);
            }
            return bRetVal;
        }

        public DateTime GetLastWriteTime(string path)
        {
            if (FileCache.ContainsKey(path))
                return FileCache[path].LastModifiedTime;
            return DateTime.UtcNow;
        }

        public bool FileExists(string path)
        {
            if (FileCache.ContainsKey(path))
                return FileCache[path].IsDirectory == false;
            return false;
        }

        public void CopyFile(string source, string target)
        {
            _baseRepository.CopyFile(source, target);
            FileCache[target] = _baseRepository.GetObject(target);
        }

        public void CreateFile(string path, byte[] fileContent)
        {
            _baseRepository.CreateFile(path, fileContent);
            while (GetObject(path) == null)
            {
                FileCache[path] = _baseRepository.GetObject(path);
                path = PathHelper.GetDirectoryName(path);
            }
        }

        public void DeleteFile(string path)
        {
            _baseRepository.DeleteFile(path);
            if (FileCache.ContainsKey(path))
                FileCache.Remove(path);
        }

        public void MoveFile(string source, string target)
        {
            _baseRepository.MoveFile(source, target);
            FileCache.Remove(source);
            FileCache[target] = _baseRepository.GetObject(target);
        }

        public byte[] ReadFile(string path)
        {
            if (FileCache.ContainsKey(path))
                return FileCache[path].Data;
            return null;
        }

        public void WriteFile(string path, byte[] fileContent)
        {
            _baseRepository.WriteFile(path, fileContent);
            FileCache[path] = _baseRepository.GetObject(path);
        }

        public void Dispose()
        {
        }
    }
}