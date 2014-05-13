using Xplorer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public static class Directory
    {
        static IFileSystemRepository BaseRepository
        {
            get { return FileManagerUtil.BaseRepository; }
        }
        
        public static bool Exists(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.DirectoryExists(path);
        }

        public static bool CreateDirectory(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return Exists(path) || BaseRepository.CreateDirectory(path);
        }

        public static IEnumerable<string> GetDirectories(string path)
        {
            return GetDirectories(path, null, false);
        }

        public static IEnumerable<string> GetDirectories(string path,Regex pattern,bool recursive)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.GetDirectories(path, pattern, recursive);
        }

        public static IEnumerable<string> GetFileSystemEntries(string path)
        {
            return GetFileSystemEntries(path, null, false);
        }
        public static IEnumerable<string> GetFileSystemEntries(string path, Regex pattern, bool recursive)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.GetFileSystemEntries(path, pattern, recursive);
        }

        public static IEnumerable<string> GetFiles(string path)
        {
           return GetFiles(path, null, false);
        }
        public static IEnumerable<string> GetFiles(string path, Regex pattern, bool recursive)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.GetFiles(path, pattern, recursive);
        }

        public static bool Delete(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.DeleteDirectory(path);
        }

        public static bool Copy(string source, string target)
        {
            source = source.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            target = target.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.CopyDirectory(source, target);
        }

        public static bool Move(string source, string target)
        {
            source = source.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            target = target.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.MoveDirectory(source, target);
        }

        public static DateTime GetLastWriteTime(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.GetLastWriteTime(path);
        }

    }
}

