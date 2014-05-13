using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;
using System.IO;
using Xplorer.Core.Utilities;
using Directory = Xplorer.Core.IO.Directory;
using DirectoryInfo = Xplorer.Core.IO.DirectoryInfo;
using File = Xplorer.Core.IO.File;
using FileInfo = Xplorer.Core.IO.FileInfo;

namespace Xplorer.Core.Repository
{
    public class DbFileSystemRepository : IFileSystemRepository
    {
        
        private readonly string _root = string.Empty;


        public string Root
        {
            get { return _root; }
        }

        public void Dispose()
        {
        }

        public DbFileSystemRepository()
        {
            this._root = PathHelper.Combine("D:\\", AppConfig.FileSystemRootPath);
        }


        private static IFileSystemEntry GetFileSystemEntry(Data.File retVal)
        {
            IFileSystemEntry info = null;
            if (retVal != null)
            {
                info = new FileSystemEntry
                            {
                                CreationTime = retVal.CreationTime ?? DateTime.UtcNow,
                                Data = retVal.Data != null ? retVal.Data.ToArray() : null,
                                Extension = retVal.Extension,
                                FullName = retVal.FullName,
                                IsDirectory = retVal.IsDirectory ?? false
                            };
                if (!retVal.Attributes.HasValue)
                {
                    if (!retVal.IsDirectory.HasValue)
                        info.Attributes = FileAttributes.Normal | FileAttributes.Directory;
                    else
                        info.Attributes = FileAttributes.Normal;
                }
                else
                    info.Attributes = (FileAttributes)(retVal.Attributes.Value);


                info.IsEncrypted = retVal.IsEncrypted ?? false;
                info.IsZipped = retVal.IsZipped ?? false;
                info.LastModifiedTime = retVal.LastModifiedTime ?? DateTime.UtcNow;
                info.Name = retVal.Name;
                info.OriginalSize = retVal.OriginalSize ?? 0;
                info.Permissions = (FileAccess)retVal.Permissions;
            }
            return info;
        }

        public IFileSystemEntry GetObject(string path)
        {
            Data.File retVal = null;
            Data.FileDbContext.Using(context =>
            {
                string parentDir = PathHelper.GetDirectoryName(path);
                string name = PathHelper.GetFileName(path);
                retVal = context.spFMGetObject(parentDir, name, null).SingleOrDefault();
            });
            return GetFileSystemEntry(retVal);
        }

        private static bool DirectoryExistsInternal(string path, out Data.File dir)
        {
            Data.File tempDir = null;
            Data.FileDbContext.Using(context =>
            {
                string parentDir = PathHelper.GetDirectoryName(path);
                string fileName = PathHelper.GetFileName(path);
                tempDir = context.spFMGetObject(parentDir, fileName, true).SingleOrDefault();
            });
            dir = tempDir;
            return dir != null;
        }

        public bool DirectoryExists(string path)
        {
            path = PathHelper.GetFullPath(path);
            Data.File dir;
            return DirectoryExistsInternal(path, out dir);
        }

        private static bool CreateDirectoryInternal(string path, out Data.File parent)
        {
            if (DirectoryExistsInternal(path, out parent))
                return true;
            string parentDirName = PathHelper.GetDirectoryName(path);
            string dirName = PathHelper.GetFileName(path);
            var now = DateTime.Now.ToUniversalTime();

            if (DirectoryExistsInternal(parentDirName, out parent) || CreateDirectoryInternal(parentDirName, out parent))
            {
                Data.File parentTemp = parent;
                if (parent != null)
                {
                    Data.FileDbContext.Using(context =>
                    {
                        parentTemp = context.spFMDirectoryCreate(dirName, null, now,
                                                                 (int)
                                                                 (FileAttributes.
                                                                      Directory |
                                                                  FileAttributes.Normal),
                                                                 0,
                                                                 parentTemp.Id,
                                                                 (int)
                                                                 FileAccess.ReadWrite,
                                                                 path).SingleOrDefault();
                    });
                    parent = parentTemp;
                }
            }

            return parent != null;
        }

        public bool CreateDirectory(string path)
        {
            path = PathHelper.GetFullPath(path);
            bool bRetVal;
            using (var scope = new TransactionScope())
            {
                Data.File tempDir;
                bRetVal = CreateDirectoryInternal(path, out tempDir);
                if (bRetVal)
                    scope.Complete();
            }
            return bRetVal;
        }

        public IEnumerable<IFileSystemEntry> GetFileInfos(string path, Regex pattern, bool recursive)
        {
            path = PathHelper.GetFullPath(path);
            Data.File directory;
            if (!DirectoryExistsInternal(path, out directory))
                throw new DirectoryNotFoundException("Could not find a part of the path '" + path + "'.");
            var fileEntries = new List<Data.File>();
            if (pattern == null)
                pattern = RegexUtil.MatchAny;
            Data.FileDbContext.Using(context => fileEntries.AddRange(
                recursive
                    ? context.spFMGetFilesRecursive(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()
                    : context.spFMGetFiles(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()));

            return fileEntries.Select(GetFileSystemEntry);
        }

        public IEnumerable<IFileSystemEntry> GetDirectoryInfos(string path, Regex pattern, bool recursive)
        {
            path = PathHelper.GetFullPath(path);
            Data.File directory;
            if (!DirectoryExistsInternal(path, out directory))
                throw new DirectoryNotFoundException("Could not find a part of the path '" + path + "'.");
            var fileEntries = new List<Data.File>();
            if (pattern == null)
                pattern = RegexUtil.MatchAny;

            Data.FileDbContext.Using(context => fileEntries.AddRange(
                recursive
                    ? context.spFMGetDirectoriesRecursive(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()
                    : context.spFMGetDirectories(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()));

            return fileEntries.Select(GetFileSystemEntry);
        }

        public IEnumerable<IFileSystemEntry> GetFileSystemInfos(string path, Regex pattern, bool recursive)
        {
            path = PathHelper.GetFullPath(path);
            Data.File directory;
            if (!DirectoryExistsInternal(path, out directory))
                throw new DirectoryNotFoundException("Could not find a part of the path '" + path + "'.");
            var fileEntries = new List<Data.File>();
            if (pattern == null)
                pattern = RegexUtil.MatchAny;

            Data.FileDbContext.Using(context => fileEntries.AddRange(
                recursive
                    ? context.spFMGetFileSystemEntriesRecursive(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()
                    : context.spFMGetFileSystemEntries(directory.Id).Where(o => pattern.IsMatch(o.Name)).ToList()));

            return fileEntries.Select(GetFileSystemEntry);
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
                if (!Directory.Exists(source))
                    return false;
                if (!Directory.Exists(target))
                    Directory.CreateDirectory(target);

                var lstFiles = new DirectoryInfo(source).GetFileSystemInfos(RegexUtil.MatchIfStartsWithoutDot, true);
                lstFiles = lstFiles.OrderByDescending(o => o.IsDirectory);
                lstFiles.ForEach(o =>
                {
                    var newPath = o.FullName.Replace(source, target);
                    if (o.IsDirectory)
                        Directory.CreateDirectory(newPath);
                    else
                        File.Create(newPath, ((FileInfo)o).Data);
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
            Data.File srcDir;
            if (DirectoryExistsInternal(source, out srcDir))
            {
                Data.File tgtDir;
                string targetDirParent = PathHelper.GetDirectoryName(target);
                if (DirectoryExistsInternal(targetDirParent, out tgtDir) ||
                    CreateDirectoryInternal(targetDirParent, out tgtDir))
                {
                    var dirName = PathHelper.GetFileName(target);
                    DateTime now = DateTime.Now.ToUniversalTime();
                    Data.FileDbContext.Using(context =>
                    {
                        using (var scope = new TransactionScope())
                        {
                            context.spFMDirectoryMove(source, target, targetDirParent,
                                                      dirName, now);
                            scope.Complete();
                        }
                    });
                    return true;
                }
            }
            return false;
        }

        public bool DeleteDirectory(string path)
        {
            path = PathHelper.GetFullPath(path);
            Data.File dir;
            if (DirectoryExistsInternal(path, out dir))
            {
                Data.FileDbContext.Using(context =>
                {
                    using (var scope = new TransactionScope())
                    {
                        context.spFMDelete(dir.Id);
                        scope.Complete();
                    }
                });
                return true;
            }
            return false;
        }

        public DateTime GetLastWriteTime(string path)
        {
            Data.File directory;
            if (!DirectoryExistsInternal(path, out directory))
                throw new DirectoryNotFoundException("Could not find a part of the path '" + path + "'.");
            return directory.LastModifiedTime ?? DateTime.UtcNow;
        }

        private static bool FileExistsInternal(string path, out Data.File file)
        {
            Data.File f = null;
            Data.FileDbContext.Using(context =>
            {
                string parentDir = PathHelper.GetDirectoryName(path);
                string fileName = PathHelper.GetFileName(path);
                f = context.spFMGetObject(parentDir, fileName, false).SingleOrDefault();
            });
            file = f;
            return f != null;
        }

        public bool FileExists(string path)
        {
            path = PathHelper.GetFullPath(path);
            Data.File file;
            return FileExistsInternal(path, out file);
        }

        public void CopyFile(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);
            Data.FileDbContext.Using(context =>
            {
                DateTime now = DateTime.Now.ToUniversalTime();
                string srcDir = PathHelper.GetDirectoryName(source);
                string srcFileName = PathHelper.GetFileName(source);
                string tarDir = PathHelper.GetDirectoryName(target);
                string tarFileName = PathHelper.GetFileName(target);
                string tarGetExtension = PathHelper.GetExtension(target);
                context.spFMFileCopy(srcFileName, srcDir, tarFileName, tarGetExtension,
                                     tarDir, now);
            });
        }

        public void CreateFile(string path, byte[] fileContent)
        {
            path = PathHelper.GetFullPath(path);
            DateTime now = DateTime.Now.ToUniversalTime();
            string parentDir = PathHelper.GetDirectoryName(path);
            string fileName = PathHelper.GetFileName(path);
            string extension = PathHelper.GetExtension(path);
            Data.File temp;
            const short attributes = (short)FileAttributes.Normal;
            const short permissions = (short)FileAccess.ReadWrite;

            if (!DirectoryExists(parentDir))
                CreateDirectory(parentDir);

            if (!FileExistsInternal(path, out temp))
            {
                Data.FileDbContext.Using(context => context.spFMFileUpdate(parentDir, fileName, fileContent, now,
                                                                           attributes, extension, false, false,
                                                                           fileContent.Length, permissions));
            }
        }

        public void DeleteFile(string path)
        {
            path = PathHelper.GetFullPath(path);
            Data.FileDbContext.Using(context =>
            {
                Data.File file;
                if (FileExistsInternal(path, out file))
                {
                    context.spFMDelete(file.Id);
                }
            });
        }

        public void MoveFile(string source, string target)
        {
            source = PathHelper.GetFullPath(source);
            target = PathHelper.GetFullPath(target);
            Data.FileDbContext.Using(context =>
            {
                DateTime now = DateTime.Now.ToUniversalTime();
                string srcDir = PathHelper.GetDirectoryName(source);
                string srcFileName = PathHelper.GetFileName(source);
                string tarDir = PathHelper.GetDirectoryName(target);
                string tarFileName = PathHelper.GetFileName(target);
                string tarGetExtension = PathHelper.GetExtension(target);
                context.spFMFileMove(srcFileName, srcDir, tarFileName, tarGetExtension,
                                     tarDir, now);
            });
        }

        public byte[] ReadFile(string path)
        {
            path = PathHelper.GetFullPath(path);
            string parentDir = PathHelper.GetDirectoryName(path);
            string fileName = PathHelper.GetFileName(path);
            Data.File f;
            var fileContent = new byte[0];
            if (FileExistsInternal(path, out f))
            {
                Data.FileDbContext.Using(context =>
                {
                    Data.File file = context.spFMFileRead(parentDir, fileName).SingleOrDefault();
                    if (file.Data != null)
                    {
                        fileContent = file.Data.ToArray();
                    }
                });
            }
            return fileContent;
        }

        public void WriteFile(string path, byte[] fileContent)
        {
            path = PathHelper.GetFullPath(path);
            DateTime now = DateTime.Now.ToUniversalTime();
            string parentDir = PathHelper.GetDirectoryName(path);
            string fileName = PathHelper.GetFileName(path);
            Data.File f;
            if (FileExistsInternal(path, out f))
            {
                Data.FileDbContext.Using(context => context.spFMFileUpdate(parentDir, fileName, fileContent, now,
                                                                           f.Attributes, f.Extension, false, false,
                                                                           fileContent.Length, f.Permissions));
            }
            else
                CreateFile(path, fileContent);
        }

        /*
                private static void Decompress(SIO.MemoryStream zipped, SIO.MemoryStream output)
                {
                    zipped.Seek(0, SIO.SeekOrigin.Begin);
                    var gzip = new SIO.Compression.GZipStream(zipped,SIO.Compression.CompressionMode.Decompress,true);

                    var bytes = new byte[4096];
                    int n;
                    while ((n = gzip.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        output.Write(bytes, 0, n);
                    }
                    gzip.Close();
                }
        */

        /*
                private static void Compress(SIO.MemoryStream raw, SIO.MemoryStream output)
                {
                    var gzip = new SIO.Compression.GZipStream(output,
                                                                                                 SIO.Compression.
                                                                                                     CompressionMode.Compress,
                                                                                                 true);
                    raw.Seek(0, SIO.SeekOrigin.Begin);
                    var bytes = new byte[4096];
                    int n;
                    while ((n = raw.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        gzip.Write(bytes, 0, n);
                    }
                    gzip.Close();
                }
        */

    }
}