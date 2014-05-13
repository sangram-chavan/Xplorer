using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Xplorer.Core.IO;
using Xplorer.Core.Utilities;
using Directory = Xplorer.Core.IO.Directory;
using DirectoryInfo = Xplorer.Core.IO.DirectoryInfo;
using File = Xplorer.Core.IO.File;
using FileInfo = Xplorer.Core.IO.FileInfo;
using FileSystemInfo = Xplorer.Core.IO.FileSystemInfo;

namespace Xplorer.Core
{
    public static class ZipUtil
    {
        public static byte[] ZipFiles(string inputFolderPath)
        {
            try
            {
                var outputStream = new MemoryStream();
                var lst = new List<FileInfo>();
                
                var temp = FileSystemInfo.GetEntry(inputFolderPath);
                if (temp.IsDirectory)
                {
                    if (RegexUtil.MatchIfStartsWithoutDot.IsMatch(temp.Name))
                        lst.AddRange(((DirectoryInfo)temp).GetFiles(null, true));
                }
                else
                    lst.Add(temp as FileInfo);

                using (var zipStream = new ZipOutputStream(outputStream))
                {
                    zipStream.UseZip64 = UseZip64.Off;
                    lst.ForEach(data =>
                    {
                        var entry = new ZipEntry(ZipEntry.CleanName(data.FullName.Replace(inputFolderPath + "\\", string.Empty))) { Size = data.OriginalSize };
                        zipStream.PutNextEntry(entry);
                        if (data.Data != null && data.Data.Length > 0)
                            zipStream.Write(data.Data, 0, data.Data.Length);
                        zipStream.CloseEntry();
                    });

                    zipStream.Finish();
                    zipStream.Close();
                }
                return outputStream.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            UnZipFiles(zipPathAndFile, outputFolder, string.Empty, deleteZipFile);
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder)
        {
            UnZipFiles(zipPathAndFile, outputFolder, string.Empty, false);
        }

        public static void UnZipFiles(string zipPathAndFile, string target, string password, bool deleteZipFile)
        {
            var info = new FileInfo(zipPathAndFile);
            if (info.Data != null && info.Data.Length > 0)
            {
                var ms = new MemoryStream(info.Data);
                var zis = new ZipInputStream(ms);

                ZipEntry theEntry;
                var lstDirNames = new List<string>();
                while ((theEntry = zis.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(theEntry.Name)) return;

                    var fullpath = PathHelper.GetFullPath(PathHelper.Combine(target, theEntry.Name));
                    if (theEntry.IsDirectory && !lstDirNames.Contains(fullpath))
                    {
                        if (Directory.CreateDirectory(fullpath))
                            lstDirNames.Add(fullpath);
                    }
                    else
                    {
                        var contentData = new byte[theEntry.Size];
                        zis.Read(contentData, 0, contentData.Length);
                        File.Create(fullpath, contentData);
                        fullpath = PathHelper.GetDirectoryName(fullpath);
                        if (!lstDirNames.Contains(fullpath))
                            lstDirNames.Add(fullpath);
                    }
                }
                if (deleteZipFile)
                    File.Delete(zipPathAndFile);
            }
        }

        public static void UnZipFiles(byte[] data, string target)
        {
            if (data != null && data.Length > 0)
            {
                var ms = new MemoryStream(data);
                var zis = new ZipInputStream(ms);

                ZipEntry theEntry;
                var lstDirNames = new List<string>();
                while ((theEntry = zis.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(theEntry.Name)) return;

                    var fullpath = PathHelper.GetFullPath(PathHelper.Combine(target, theEntry.Name));
                    if (theEntry.IsDirectory && !lstDirNames.Contains(fullpath))
                    {
                        if (Directory.CreateDirectory(fullpath))
                            lstDirNames.Add(fullpath);
                    }
                    else
                    {
                        var contentData = new byte[theEntry.Size];
                        zis.Read(contentData, 0, contentData.Length);
                        File.Create(fullpath, contentData);
                        fullpath = PathHelper.GetDirectoryName(fullpath);
                        if (!lstDirNames.Contains(fullpath))
                            lstDirNames.Add(fullpath);
                    }
                }
            }
        }
    }
}