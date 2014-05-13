using System;
using System.Collections.Generic;

namespace WebSite.Core
{
    public partial class FileManager
    {
        private readonly FileManagerConfig _config = null;

        private readonly Dictionary<string, string> _commands = new Dictionary<string, string>
                                    {
                                        {"open", "_open"},
                                        {"reload", "_reload"},
                                        {"mkdir", "_mkdir"},
                                        {"mkfile", "_mkfile"},
                                        {"rename", "_rename"},
                                        {"upload", "_upload"},
                                        {"paste", "_paste"},
                                        {"rm", "_rm"},
                                        {"duplicate", "_duplicate"},
                                        {"read", "_fread"},
                                        {"edit", "_edit"},
                                        {"archive", "_archive"},
                                        {"extract", "_extract"},
                                        {"resize", "_resize"},
                                        {"tmb", "_thumbnails"},
                                        {"ping", "_ping"}
                                    };

        private readonly List<string> _loggedCommands = new List<string>() { "mkdir", "mkfile", "rename", "upload", "paste", "rm", "duplicate", "edit", "resize" };

        private OrderedMap _logContext = new OrderedMap();

        private readonly Dictionary<string, string> _mimeTypes = new Dictionary<string, string>
                                                                     {
                                                                         //applications
                                                                         {"ai", "application/postscript"},
                                                                         {"eps", "application/postscript"},
                                                                         {"exe", "application/octet-stream"},
                                                                         {"doc", "application/vnd.ms-word"},
                                                                         {"xls", "application/vnd.ms-excel"},
                                                                         {"ppt", "application/vnd.ms-powerpoint"},
                                                                         {"pps", "application/vnd.ms-powerpoint"},
                                                                         {"pdf", "application/pdf"},
                                                                         {"xml", "application/xml"},
                                                                         {"odt", "application/vnd.oasis.opendocument.text"},
                                                                         {"swf", "application/x-shockwave-flash"},
                                                                         //archives
                                                                         {"gz", "application/x-gzip"},
                                                                         {"tgz", "application/x-gzip"},
                                                                         {"bz", "application/x-bzip2"},
                                                                         {"bz2", "application/x-bzip2"},
                                                                         {"tbz", "application/x-bzip2"},
                                                                         {"zip", "application/zip"},
                                                                         {"rar", "application/x-rar"},
                                                                         {"tar", "application/x-tar"},
                                                                         {"7z", "application/x-7z-compressed"},
                                                                         //texts
                                                                         {"txt", "text/plain"},
                                                                         {"php", "text/x-php"},
                                                                         {"html", "text/html"},
                                                                         {"htm", "text/html"},
                                                                         {"js", "text/javascript"},
                                                                         {"css", "text/css"},
                                                                         {"rtf", "text/rtf"},
                                                                         {"rtfd", "text/rtfd"},
                                                                         {"py", "text/x-python"},
                                                                         {"java", "text/x-java-source"},
                                                                         {"rb", "text/x-ruby"},
                                                                         {"sh", "text/x-shellscript"},
                                                                         {"pl", "text/x-perl"},
                                                                         {"sql", "text/x-sql"},
                                                                         //images
                                                                         {"bmp", "image/x-ms-bmp"},
                                                                         {"jpg", "image/jpeg"},
                                                                         {"jpeg", "image/jpeg"},
                                                                         {"gif", "image/gif"},
                                                                         {"png", "image/png"},
                                                                         {"tif", "image/tiff"},
                                                                         {"tiff", "image/tiff"},
                                                                         {"tga", "image/x-targa"},
                                                                         {"psd", "image/vnd.adobe.photoshop"},
                                                                         //audio
                                                                         {"mp3", "audio/mpeg"},
                                                                         {"mid", "audio/midi"},
                                                                         {"ogg", "audio/ogg"},
                                                                         {"mp4a", "audio/mp4"},
                                                                         {"wav", "audio/wav"},
                                                                         {"wma", "audio/x-ms-wma"},
                                                                         //video
                                                                         {"avi", "video/x-msvideo"},
                                                                         {"dv", "video/x-dv"},
                                                                         {"mp4", "video/mp4"},
                                                                         {"mpeg", "video/mpeg"},
                                                                         {"mpg", "video/mpeg"},
                                                                         {"mov", "video/quicktime"},
                                                                         {"wm", "video/x-ms-wmv"},
                                                                         {"flv", "video/x-flv"},
                                                                         {"mkv", "video/x-matroska"}
                                                                     };

        private OrderedMap _result = new OrderedMap();

        private readonly DateTime _today = DateTime.Now.ToUniversalTime();

        private readonly DateTime _yesterday = DateTime.Now.ToUniversalTime().AddDays(-1);


    }
}