using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Xplorer.Core.Extensions;
using System.IO;
using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public static class PathHelper
    {

        [DebuggerStepThrough]
        static void CheckPath(IEnumerable<char> p)
        {
            if (Path.GetInvalidPathChars().Any(item => p.Contains(item)))
                throw new ArgumentException("Illegal characters in path.");
        }

        public static string GetFullPath(string path)
        {
            CheckPath(path);
            return string.Compare(Path.GetPathRoot(path), path, true) == 0 ? path.RemoveEndSlashes() : Path.GetFullPath(path);
        }

        public static string GetDirectoryName(string path)
        {
            CheckPath(path);
            return string.Compare(Path.GetPathRoot(path), path, true) == 0 ? path.RemoveEndSlashes() :
                (Path.GetDirectoryName(path) ?? string.Empty).RemoveEndSlashes();
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            CheckPath(path);
            if (string.Compare(Path.GetPathRoot(path), path, true) == 0)
                return path.RemoveEndSlashes();
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetFileName(string path)
        {
            CheckPath(path);
            if (string.Compare(Path.GetPathRoot(path), path, true) == 0)
                return path.RemoveEndSlashes();
            return Path.GetFileName(path);
        }

        public static string GetExtension(string path)
        {
            CheckPath(path);
            return Path.GetExtension(path);
        }

        public static string Combine(string path1, string path2)
        {
            path1 = path1.ReplaceConsecutiveSlashes("\\");
            path2 = path2.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return Path.Combine(path1, path2);
        }

        static IFileSystemRepository BaseRepository
        {
            get
            {
                return FileManagerUtil.BaseRepository;
            }
        }

        public static string MapPath(string virtualPath)
        {
            
            string retVal = VirtualPathUtility.ToAbsolute(virtualPath).ReplaceConsecutiveSlashes("\\").RemoveStartSlashes();
            return Combine(BaseRepository.Root, retVal).RemoveEndSlashes();
        }

        public static string VirtualPath(string physicalPath)
        {
            physicalPath = physicalPath.Replace(FileManagerUtil.BaseRepository.Root, "~/").ReplaceConsecutiveSlashes("/");
            return VirtualPathUtility.ToAbsolute(physicalPath);
        }
    }
}