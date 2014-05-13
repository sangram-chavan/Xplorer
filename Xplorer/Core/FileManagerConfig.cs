using System.Collections.Generic;
using System.Configuration;

namespace WebSite.Core
{
    public class FileManagerConfig
    {
        public string MaxUploadSize = ConfigurationManager.AppSettings["UPLOAD_MAX_FILESIZE"] ?? "2M";
        public string root = string.Empty;                          // path to root directory
        public string URL = "http://localhost:1828/";      // root directory URL
        public string rootAlias = "Tavisca";                        // display this instead of root directory name
        public List<string> disabled = new List<string>() { };      // list of not allowed commands
        public bool dotFiles = false;                               // display dot files
        public bool dirSize = true;                                 // count total directories sizes
        public int fileMode = 0666;                                 // new files mode
        public int dirMode = 0777;                                  // new folders mode
        public string mimeDetect = "auto";                          // files mimetypes detection method (finfo}, mime_content_type}, linux (file -ib)}, bsd (file -Ib)}, internal (by extensions))
        public List<string> uploadAllow = new List<string>() { "all" };       // mimetypes which allowed to upload
        public List<string> uploadDeny = new List<string>();        // mimetypes which not allowed to upload
        public string uploadOrder = "deny,allow";             // order to proccess uploadAllow and uploadAllow options
        public string imgLib = "auto";                              // image manipulation library (imagick}, mogrify}, gd)
        public bool enableThumbnails = true;
        public string tmbDir = ".tmb";                              // directory name for image thumbnails. Set to "" to avoid thumbnails generation
        public int tmbCleanProb = 1;                                // how frequiently clean thumbnails dir (0 - never}, 200 - every init request)
        public int tmbAtOnce = 5;                                   // number of thumbnails to generate per request
        public int tmbSize = 48;                                    // images thumbnails size (px)
        public bool fileURL = true;                                 // display file URL in "get info"
        public ILogger logger = null;
        public string dateFormat = "dddd, dd MMMM yyyy,  hh:mm tt"; // file modification date format
        public Dictionary<string, bool> defaults = new Dictionary<string, bool> // default permisions
                        {        
                                    {"read", true},
                                    {"write", true},
                                    {"rm", true}
                        };
        public OrderedMap perms = new OrderedMap();                 // individual folders/files permisions     
        public bool debug = false;                                  // send debug to client
        public OrderedMap archiveMimes = new OrderedMap();          // allowed archive"s mimetypes to create. Leave empty for all available types.
        public OrderedMap archivers = new OrderedMap();             // info about archivers to use. See example below. Leave empty for auto detect
        //{"archivers", new OrderedMap{
        //                                {"create", new OrderedMap{{"application/x-gzip", new OrderedMap{{"cmd", "tar"},{"argc","-czf"},{"ext", "tar.gz"}}}}},
        //                                {"extract", new OrderedMap{{"application/x-gzip", new OrderedMap{{"cmd", "tar"},{"argc","-xzf"},{"ext", "tar.gz"}}},
        //                                                          {"application/x-bzip2", new OrderedMap{{"cmd", "tar"},{"argc","-xjf"},{"ext", "tar.bz"}}}}}
        //                            }}

    };
}