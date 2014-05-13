
using Xplorer.Core.Extensions;
using System;
using System.Text;
using Xplorer.Core.Repository;

namespace Xplorer.Core.IO
{
    public static class File
    {
        static IFileSystemRepository BaseRepository
        {
            get { return FileManagerUtil.BaseRepository; }
        }

        public static bool Exists(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.FileExists(path);
        }

        public static void Delete(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            BaseRepository.DeleteFile(path);
        }

        public static void Move(string source, string target)
        {
            source = source.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            target = target.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            BaseRepository.MoveFile(source, target);
        }

        public static void Create(string path, byte[] fileContent)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            BaseRepository.CreateFile(path, fileContent);

        }

        public static void Update(string path, byte[] fileContent)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            BaseRepository.WriteFile(path, fileContent);
        }

        public static byte[] Read(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return BaseRepository.ReadFile(path);
        }

        public static byte[] ReadAllBytes(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            return Read(path);
        }

        public static void WriteAllText(string path, string fileContent)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            byte[] contents = Array.ConvertAll(fileContent.ToCharArray(), Convert.ToByte);
            BaseRepository.WriteFile(path, contents);
        }

        public static string ReadAllText(string path)
        {
            path = path.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            byte[] fileContent = BaseRepository.ReadFile(path);
            if (fileContent == null || fileContent.Length == 0)
                return string.Empty;
            return Encoding.UTF8.GetString(fileContent);
        }

        public static void Copy(string source, string target)
        {
            source = source.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            target = target.ReplaceConsecutiveSlashes("\\").RemoveEndSlashes();
            BaseRepository.CopyFile(source, target);
        }
    }

}

