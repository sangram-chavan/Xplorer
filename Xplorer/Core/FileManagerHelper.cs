using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using Xplorer.Core;
using Xplorer.Core.Extensions;
using Xplorer.Core.IO;
using Xplorer.Core.Repository;
using SIO = System.IO;
namespace WebSite.Core
{
    public partial class FileManager
    {
        /************************************************************/
        /**                   Other Functions                      **/
        /************************************************************/
        bool empty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        string BaseName(string path)
        {
            return BaseName(path, string.Empty);
        }

        string BaseName(string path, string suffix)
        {
            if (!string.IsNullOrEmpty(suffix) && path.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                return PathHelper.GetFileNameWithoutExtension(path);
            else
                return PathHelper.GetFileName(path);
        }

        string dirname(string path)
        {
            return PathHelper.GetDirectoryName((string)path);
        }

        string substr(string str, int start)
        {
            return substr(str, start, str.Length);
        }

        string substr(string str, int start, int length)
        {
            if (Math.Abs(start) > str.Length) return string.Empty;

            if (start >= 0 && length < 0)
                length = str.Length - start + length;
            else if (start < 0 && length >= 0)
                start = str.Length + start;
            else if (start < 0 & length < 0)
            {
                length = str.Length + start + length;
                start = str.Length + start;
            }
            if (length < 0)
                return string.Empty;
            length = length > (str.Length - start) ? (str.Length - start) : length;

            return str.Substring(start, length);
        }

        string rawurlencode(string target)
        {
            if (target == null) return string.Empty;
            return HttpContext.Current.Server.HtmlEncode(target);
        }

        static Dictionary<string, string> pathinfo(string path)
        {
            Dictionary<string, string> pInfo = null;
            try
            {
                pInfo = new Dictionary<string, string>();
                pInfo["dirname"] = PathHelper.GetDirectoryName(path);
                pInfo["basename"] = PathHelper.GetFileNameWithoutExtension(path);
                pInfo["extension"] = PathHelper.GetExtension(path);
            }
            catch
            { }
            return pInfo;
        }

        bool exit(string message)
        {
            System.Web.HttpContext.Current.Response.Write(message);
            System.Web.HttpContext.Current.Response.End();
            return false;
        }

        string json_encode(object obj)
        {
            return obj.ToJson();
            //return JsonConvert.ExportToString(obj);
        }

        bool exit()
        {
            System.Web.HttpContext.Current.Response.End();
            return false;
        }

        string filetype(string path)
        {
            string result = "";
            if (File.Exists(path))
                result = "file";
            else if (Directory.Exists(path))
                result = "dir";
            else
                result = "unkown";

            return result;
        }


        /************************************************************/
        /**                   Public Members                       **/
        /************************************************************/
        /**
        * constructor
        **/
        public FileManager(FileManagerConfig cfg)
        {
            _config = cfg;
        }


        /**
         * Proccess client request and output json
         *
         * @return void
         **/
        public void run()
        {
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            if (_config == null || empty(_config.root) || !Directory.Exists(_config.root))
            {
                exit(json_encode(new OrderedMap { { "error", "Invalid backend configuration" } }));
            }
            if (!_isAllowed(_config.root, "read"))
                exit(json_encode(new OrderedMap { { "error", "Access Denied" } }));


            if (!string.IsNullOrEmpty(_config.tmbDir))
            {
                _config.tmbDir = PathHelper.Combine(_config.root, _config.tmbDir);
                if (!Directory.Exists(_config.tmbDir))
                {
                    try
                    {
                        //TODO:@mkdir($tmbDir, $this->_options['dirMode']) 
                        Directory.CreateDirectory(_config.tmbDir);
                    }
                    catch
                    {
                        _config.tmbDir = string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(_config.tmbDir))
                {
                    if (new[] { "imagick", "mogrify", "gd" }.Contains<string>(_config.imgLib) == false)
                        _config.imgLib = _getImgLib();
                }
            }

            var cmd = "";
            if (!empty(request.Form["cmd"]))
                cmd = request.Form["cmd"].Trim();
            else if (!empty(request.Form["cmd"]))
                cmd = request.Form["cmd"].Trim();

            if (empty(cmd) && request.HttpMethod == System.Net.WebRequestMethods.Http.Post)
            {
                response.Headers["Content-Type"] = "text/html";
                _result["error"] = "Data exceeds the maximum allowed size";
                exit(json_encode(_result));
            }

            var clearThumnails = false;
            if ("reload".Equals(cmd, StringComparison.InvariantCultureIgnoreCase))
            {
                clearThumnails = true;
                cmd = "open";
            }

            if (empty(cmd) || !_commands.ContainsKey(cmd) || this.GetType().GetMethod(_commands[cmd], System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public) == null)
                exit(json_encode(new OrderedMap { { "error", "Unknown command" } }));

            if (!empty(request.Form["init"]))
            {
                var ts = _utime();
                _result["disabled"] = _config.disabled;

                _result["params"] = new OrderedMap
                {
                    {"dotFiles", _config.dotFiles},
                    {"uplMaxSize",_config.MaxUploadSize},
                    {"archives", new OrderedMap()},
                    {"extract", new OrderedMap()},
                    {"url",_config.fileURL? _config.URL:string.Empty}
                };


                if (_commands["archive"] != null || _commands["extract"] != null)
                {
                    _checkArchivers();
                    if (_commands["archive"] != null)
                        ((OrderedMap)_result["params"])["archives"] = ((OrderedMap)_config.archivers["extract"]).Keys;
                    if (_commands["extract"] != null && _config.archivers.ContainsKey("extract"))
                        ((OrderedMap)_result["params"])["extract"] = ((OrderedMap)_config.archivers["extract"]).Keys;
                }
            }


            // clean thumbnails dir
            if (!empty(_config.tmbDir) && clearThumnails && _config.enableThumbnails)
            {
                try
                {
                    Directory.Delete(_config.tmbDir);
                    Directory.CreateDirectory(_config.tmbDir);
                }
                catch
                {
                    _config.tmbDir = "";
                    _config.enableThumbnails = false;
                }
            }

            if (string.IsNullOrEmpty(cmd))
                _open();
            else
                this.GetType().GetMethod(_commands[cmd], System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(this, null);

            if (_config.debug)
            {
                _result["debug"] = new OrderedMap
                {
                    {"time",_utime()},
                    {"mimeDetect",_config.mimeDetect},
                    {"imgLib",_config.imgLib}
                };
                if (_config.dirSize)
                {
                    ((OrderedMap)_result["debug"])["dirSize"] = true;
                    ((OrderedMap)_result["debug"])["du"] = true;
                }

            }

            response.AddHeader("Content-Type", (cmd.Equals("upload", StringComparison.InvariantCultureIgnoreCase) ? "text/html" : "application/json"));
            response.AddHeader("Connection", "close");
            //TODO : Error => This operation requires IIS integrated pipeline mode.
            //response.Headers["Content-Type"] = (cmd == "upload" ? "text/html" : "application/json");
            //response.Headers["Connection"] = "close";
            response.Write(json_encode(_result));

            if (_config.logger != null && _loggedCommands.Contains(cmd))
                _config.logger.log(cmd, _logContext, _result);
            exit();
        }

        /************************************************************/
        /**                   elFinder commands                    **/
        /************************************************************/

        /**
        * Return current dir content to client or output file content to browser
        *
        * @return void
        **/
        void _open()
        {
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            var _current = request.Form["current"];
            var _target = request.Form["target"];

            string _dir = string.Empty;
            string _file = string.Empty;
            if (!string.IsNullOrEmpty(_current))
            { // read file
                if (empty(_current)
                || empty(_target)
                || empty(_dir = _findDir(_current.Trim()))
                || empty(_file = _find(_target.Trim(), _dir))
                || Directory.Exists(_file)
                )
                {
                    response.Headers["HTTP/1.x"] = "404 Not Found";
                    exit("File not found");
                }
                if (!_isAllowed(_dir, "read") || !_isAllowed(_file, "read"))
                {
                    response.Headers["HTTP/1.x"] = "403 Access Denied";
                    exit("Access denied");
                }

                var mime = this._mimetype(_file);
                var parts = mime.Split("/".ToCharArray());

                var disp = "image".Equals(parts[0], StringComparison.InvariantCultureIgnoreCase) || "text".Equals(parts[0], StringComparison.InvariantCultureIgnoreCase) ? "inline" : "attachments";

                //response.ContentType = "application/octet-stream";
                response.Headers["Content-Type"] = mime;
                response.Headers["Content-Disposition"] = disp + "; filename=" + BaseName(_file);
                response.Headers["Content-Location"] = _file.Replace(_config.root, "");
                response.Headers["Content-Transfer-Encoding"] = "binary";
                response.Headers["Content-Length"] = new FileInfo(_file).OriginalSize + "";
                response.Headers["Connection"] = "close";
                response.WriteFile(_file);
                exit();
            }
            else
            {
                // enter directory
                var path = _config.root;
                if (!empty(_target))
                {
                    var p = _findDir(_target);
                    if (empty(p))
                    {
                        if (empty(request.Form["init"]))
                            _result["error"] = "Invalid parameters";
                    }
                    else if (!_isAllowed(p, "read"))
                    {
                        if (empty(request.Form["init"]))
                            _result["error"] = "Access denied";
                    }
                    else
                        path = p;
                }
                _content(path, !empty(request.Form["init"]));
            }
        }

        bool Native_Rename(string source, string target)
        {
            try
            {
                switch (filetype(source).ToLower())
                {
                    case "file":
                        File.Move(source, target);
                        break;
                    case "dir":
                        Directory.Move(source, target);
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /**
         * Rename file/folder
         *
         * @return void
         **/
        void _rename()
        {
            var _GET = HttpContext.Current.Request.Form;
            var _current = _GET["current"];
            var _target = _GET["target"];
            string name = _GET["name"];
            string _dir = string.Empty;
            if (empty(_current)
            || empty(_target)
            || empty(_dir = _findDir(_current.Trim()))
            || empty(_target = _find(_target.Trim(), _dir))
            )
            {
                _result["error"] = "File not found";
            }
            else if (!_checkName(_GET["name"]))
                _result["error"] = "Invalid Name";
            else if (!_isAllowed(_dir, "write"))
                _result["error"] = "Access denied";
            else if (File.Exists(PathHelper.Combine(_dir, name)))
                _result["error"] = "File or folder with the same name already exists";
            else if (!Native_Rename(_target, PathHelper.Combine(_dir, name)))
                _result["error"] = "Unable to rename file";
            else
            {
                _rmTmb(_target);
                _logContext["from"] = _target;
                _logContext["to"] = PathHelper.Combine(_dir, name);
                _result["select"] = _hash(PathHelper.Combine(_dir, name));
                _content(_dir, Directory.Exists(PathHelper.Combine(_dir, name)));
            }
        }

        bool Native_CreateDir(string dir, int mode)
        {
            try { Directory.CreateDirectory(dir); }
            catch { } return true;
        }

        /**
        * Create new folder
        *
        * @return void
        **/
        void _mkdir()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _dir = string.Empty;
            string name = _GET["name"];
            if (empty(_GET["current"]) || empty(_dir = _findDir(_GET["current"].Trim())))
            {
                _result["error"] = "Invalid parameters";
                return;
            }
            _logContext["dir"] = PathHelper.Combine(_dir, _GET["name"]);
            if (!_isAllowed(_dir, "write"))
                _result["error"] = "Access denied";
            else if (!_checkName(name))
                _result["error"] = "Invalid name";
            else if (File.Exists(PathHelper.Combine(_dir, name)))
                _result["error"] = "File or folder with the same name already exists";
            else if (!Native_CreateDir(PathHelper.Combine(_dir, name), _config.dirMode))
                _result["error"] = "Unable to create folder";
            else
            {
                _logContext["dir"] = PathHelper.Combine(_dir, name);
                _result["select"] = _hash(PathHelper.Combine(_dir, name));
                _content(_dir, true);
            }
        }

        /**
        * Create new empty file
        *
        * @return void
        **/
        void _mkfile()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _dir = string.Empty;
            string _name = _GET["name"];
            if (empty(_GET["current"])
            || empty(_dir = _findDir(_GET["current"].Trim())))
            {
                _result["error"] = "Invalid parameters";
                return;
            }
            var _path = PathHelper.Combine(_dir, _GET["name"]);
            _logContext["file"] = _path;
            if (!_isAllowed(_dir, "write"))
                _result["error"] = "Access denied";
            else if (!_checkName(_GET["name"]))
                _result["error"] = "Invalid name";
            else if (File.Exists(_path))
                _result["error"] = "File or folder with the same name already exists";
            else
            {
                try
                {
                    File.WriteAllText(_path, "");
                    _logContext["file"] = _path;
                    _result["select"] = _hash(_path);
                    _content(_dir, false);
                }
                catch { _result["error"] = "Unable to create file"; }
            }
        }

        /**
        * Remove files/folders
        *
        * @return void
        **/
        void _rm()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _dir;
            string[] targets = _GET["targets[]"].Split(new char[] { ',' });
            if (empty(_GET["current"])
                || empty(_dir = _findDir(_GET["current"].Trim())) || targets.Length == 0)
            {
                _result["error"] = "Invalid parameters";
                return;
            }
            _logContext["targets"] = targets;
            for (int i = 0; i < targets.Length; i++)
            {
                var o = targets[i];
                var f = string.Empty;
                if (!empty(f = _find(o, _dir)))
                {
                    if (!_isAllowed(f, "rm"))
                        _errorData(f, "Access denied");
                    else
                        Native_Delete(f);
                    ((string[])_logContext["targets"])[i] = f;
                }
            }
            if (_result.ContainsKey("errorData"))
                _result["error"] = "Unable to remove file";
            _content(_dir, true);
        }

        /**
        * Copy/move files/folders
        *
        * @return void
        **/
        void _paste()
        {
            var failCount = 0;
            var _GET = HttpContext.Current.Request.Form;
            string _current = string.Empty;
            string _src = string.Empty;
            string _dst = string.Empty;
            string[] _targets = _GET["targets[]"].Split(new char[] { ',' });
            if (empty(_GET["current"])
            || empty(_current = _findDir(_GET["current"].Trim()))
            || empty(_GET["src"])
            || empty(_src = _findDir(_GET["src"].Trim()))
            || empty(_GET["dst"])
            || empty(_dst = _findDir(_GET["dst"].Trim()))
            || _targets.Length == 0
            )
                _result["error"] = "Invalid parameters";
            else
            {
                var _cut = _GET["cut"];
                _logContext["src"] = new OrderedMap();
                _logContext["dest"] = _dst;
                _logContext["cut"] = _cut;


                if (!_isAllowed(_dst, "write") || !_isAllowed(_src, "read"))
                    _result["error"] = "Access denied";
                else
                {
                    foreach (string t in _targets)
                    {
                        string _f = string.Empty;
                        if (empty(_f = _find(t, _src)))
                        {
                            _result["error"] = "File not found";
                            break;
                        }
                        _logContext["src"] = _f;
                        var _targetFile = PathHelper.Combine(_dst, BaseName(_f));
                        if (_targetFile.Equals(_f, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _result["error"] = "Unable to copy into itself";
                            break;
                        }
                        else if (_cut == "1" && !_isAllowed(_f, "rm"))
                        {
                            _result["error"] = "Access denied";
                            break;
                        }
                        else if (File.Exists(_targetFile))
                        {
                            failCount++;
                            _result["error"] = "File or folder with the same name already exists";
                            continue;
                        }


                        if (_cut == "1")
                        {
                            if (!Native_Rename(_f, _targetFile))
                            {
                                failCount++;
                                _result["error"] = "Unable to move files";
                                continue;
                            }
                            else if (!Directory.Exists(_f))
                                _rmTmb(_f);
                        }
                        else if (!_copy(_f, _targetFile))
                        {
                            failCount++;
                            _result["error"] = "Unable to copy files";
                            break;
                        }
                    }
                    if (failCount > 0)
                        _result["error"] = string.Format("Unable to {0} " + failCount + " items", (_cut == "1" ? "move" : "copy"));

                }
            }
            _content(_current, true);
        }

        /**
        * Create file/folder copy with suffix - "copy"
        *
        * @return void
        **/
        void _duplicate()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _current;
            string _target;
            if (empty(_GET["current"])
            || empty(_current = _findDir(_GET["current"].Trim()))
            || empty(_GET["target"])
            || empty(_target = _find(_GET["target"].Trim(), _current)))
                _result["error"] = "Invalid parameters";
            else
            {
                _logContext["target"] = _target;
                if (!_isAllowed(_current, "write") || !_isAllowed(_target, "read"))
                    _result["error"] = "Access denied";
                else
                {
                    var _dup = _uniqueName(_target, "Copy of ");
                    if (!_copy(_target, _dup))
                        _result["error"] = "Unable to create file copy";
                    _result["select"] = new List<string>() { _hash(_dup) };
                    _content(_current, Directory.Exists(_target));
                }
            }
        }

        /**
       * Create images thumbnails 
       *
       * @return void
       **/
        void _thumbnails()
        {
            var _GET = HttpContext.Current.Request.Form;
            var _current = string.Empty;
            if (!empty(_config.tmbDir) && !empty(_GET["current"]) && null != (_current = _findDir(_GET["current"].Trim())))
            {
                _result["current"] = _hash(_current);
                _result["images"] = new OrderedMap();

                int cnt = 0;
                int max = _config.tmbAtOnce;
                foreach (var path in Directory.GetFiles(_current))
                {
                    if (_isAccepted(path))
                    {
                        if (_canCreateTmb(_mimetype(path)))
                        {
                            var tmb = _tmbPath(path);
                            if (!File.Exists(tmb))
                            {
                                if (cnt >= max)
                                    _result["tmb"] = true;
                                else if (_tmb(path, tmb))
                                {
                                    ((OrderedMap)_result["images"])[_hash(path)] = _path2url(tmb);
                                    cnt++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /**
        * Return file content to client
        *
        * @return void
        **/
        void _fread()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _current = string.Empty;
            string _target = string.Empty;
            if (empty(_GET["current"])
            || empty(_current = _findDir(_GET["current"].Trim()))
            || empty(_GET["target"])
            || empty(_target = _find(_GET["target"].Trim(), _current))
        )
                _result["error"] = "Invalid parameters";
            else
            {
                if (!_isAllowed(_target, "read"))
                    _result["error"] = "Access denied";
                else
                    _result["content"] = File.ReadAllText(_target);
            }
        }

        /**
        * Save data into text file. 
        *
        * @return void
        **/
        void _edit()
        {
            var _POST = HttpContext.Current.Request.Form;
            string _current = string.Empty;
            string _target = string.Empty;
            if (empty(_POST["current"])
            || empty(_current = _findDir(_POST["current"].Trim()))
            || empty(_POST["target"])
            || empty(_target = _find(_POST["target"].Trim(), _current))
            || empty(_POST["content"]))
                _result["error"] = "Invalid parameters";
            else
            {
                _logContext["target"] = _target;
                if (!_isAllowed(_target, "write"))
                    _result["error"] = "Access denied";
                try { File.WriteAllText(_target, _POST["content"].Trim()); }
                catch { _result["error"] = "Unable to write to file"; }
            }
            _result["target"] = _info(_target);
            // $this->_result["select"] = array($this->_hash($target));
        }

        /**
        * Send header Connection: close. Required by safari to fix bug http://www.webmasterworld.com/macintosh_webmaster/3300569.htm
        *
        * @return void
        **/
        void _ping()
        {
            HttpContext.Current.Response.AddHeader("Connection", "close");
            exit();
        }

        /************************************************************/
        /**                    "content" methods                   **/
        /************************************************************/

        /**
         * Set current dir info, content and [dirs tree]
         *
         * @param  string  $path  current dir path
         * @param  bool    $tree  set dirs tree?
         * @return void
         **/
        void _content(string path, bool tree = false)
        {
            _cwd(path);
            _cdc(path);
            if (tree)
            {
                _result["tree"] = _tree(_config.root);
            }
        }

        /**
        * Set current dir info
        *
        * @param  string  $path  current dir path
        * @return void
        **/
        void _cwd(string path)
        {
            var rel = empty(_config.rootAlias) ? _config.rootAlias : BaseName(_config.root);
            string name;
            if (_config.root.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                name = rel;
            else
            {
                name = BaseName(path);
                rel = PathHelper.Combine(rel, substr(path, _config.root.Length + 1));
            }

            _result["cwd"] = new OrderedMap{
                {"hash",  _hash(path)},
                {"name", name},
                {"mime","directory"},
                {"rel",rel},
                {"size", 0},
                {"date", Directory.GetLastWriteTime(path).ToString(_config.dateFormat)},
                {"read",true},
                {"write",_isAllowed(path, "write")},
                {"rm", _config.root.Equals(path,StringComparison.InvariantCultureIgnoreCase) ? false : _isAllowed(path, "rm")}
                };
        }

        /**
         * Set current dir content
         *
         * @param  string  $path  current dir path
         * @return void
         **/
        void _cdc(string path)
        {
            _result["cdc"] = new DirectoryInfo(path)
                                    .GetFileSystemInfos(null)
                                    .Where(o => _isAccepted(o.FullName))
                                    .OrderByDescending(o => o.IsDirectory)
                                    .OrderBy(o => o.Name)
                                    .Select(o =>
                                                {
                                                    if (o.FullName.Equals(o.Name + "\\" + o.Name, StringComparison.InvariantCultureIgnoreCase))
                                                        return _info(o.Name);
                                                    return _info(o.FullName);
                                                })
                                    .ToList();
        }

        /**
        * Return file/folder info
        * @param  string  $path  file path
        * @return array
        **/
        OrderedMap _info(string path)
        {
            var type = filetype(path);
            FileSystemInfo stat = null;
            bool isDirectory = false;
            if ("dir".Equals(type, StringComparison.InvariantCultureIgnoreCase))
            {
                stat = new DirectoryInfo(path);
                isDirectory = true;
            }
            else
            {
                stat = new FileInfo(path);
                isDirectory = false;
            }

            string d;
            if (stat.LastWriteTime > _today)
                d = "Today " + stat.LastWriteTime.ToString("hh:mm tt");
            else if (stat.LastWriteTime > _yesterday)
                d = "Today " + stat.LastWriteTime.ToString("hh:mm tt");
            else
                d = stat.LastWriteTime.ToString(_config.dateFormat);

            var info = new OrderedMap
            {
                {"name",HttpContext.Current.Server.HtmlEncode(BaseName(path))},
                {"hash", _hash(path)},
                {"mime",(isDirectory ? "directory" : _mimetype(path))},
                {"date",d}, 
                {"size",(isDirectory ? _dirSize(path) : stat.OriginalSize)},
                {"read", _isAllowed(path, "read")},
                {"write",_isAllowed(path, "write")},
                {"rm",_isAllowed(path, "rm")},
            };


            var lpath = string.Empty;
            string mime = info.Get<string>("mime").ToLower();
            if (mime != "directory")
            {
                if (_config.fileURL && info.Get<bool>("read"))
                    info["url"] = _path2url(path);
                if (mime.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                {
                    FileInfo imageFile = new FileInfo(path);
                    if (imageFile.Exists && imageFile.Data != null && imageFile.Data.Length > 0)
                    {
                        using (var img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(imageFile.Data)))
                        {
                            info["dim"] = img.Size.Width + "x" + img.Size.Height;
                            if (info.Get<bool>("read"))
                            {
                                var allowTmb = _canCreateTmb(mime);
                                info["resize"] = info.ContainsKey("dim") && allowTmb;
                                if (allowTmb)
                                {
                                    var tmb = _tmbPath(path);
                                    if (File.Exists(tmb))
                                        info["tmb"] = _path2url(tmb);
                                    else if (info.Get<bool>("resize"))
                                        _result["tmb"] = true;
                                }
                            }
                        }

                    }
                }
            }
            return info;
        }

        /**
        * Return directory tree (multidimensional array)
        *
        * @param  string  $path  directory path
        * @return array
        **/
        OrderedMap _tree(string path)
        {
            var info = new DirectoryInfo(path);
            if (info.Exists)
            {
                var dir = new OrderedMap
                              {
                                  {"hash",_hash(path)},
                                  {"name", (_config.root.Equals(path,StringComparison.InvariantCultureIgnoreCase) ? _config.rootAlias : info.Name)},
                                  {"read", _isAllowed(path, "read")},
                                  {"write", this._isAllowed(path, "write")},
                                  {"dirs", new List<OrderedMap>()}
                              };

                foreach (var dict in info.GetDirectories(null))
                {
                    if (_isAccepted(dict.FullName))
                    {
                        var p = dict.FullName;
                        ((List<OrderedMap>)dir["dirs"]).Add(_tree(p));
                    }
                }
                return dir;
            }
            return new OrderedMap();
        }


        ///**
        // * upload files
        // *
        // * @return void
        // **/
        void _upload()
        {
            var _POST = HttpContext.Current.Request.Form;
            var _current = _POST["current"];
            var _dir = string.Empty;
            HttpFileCollection myFileCollection = HttpContext.Current.Request.Files;

            if (empty(_current) || empty(_dir = _findDir(_current.Trim())))
                _result["error"] = "Invalid parameters";
            else if (!_isAllowed(_dir, "write"))
                _result["error"] = "access denied";
            else if (myFileCollection.Count == 0)
                _result["error"] = "no file to upload";
            else
            {
                var fileNames = new List<string>();
                _logContext["upload"] = new OrderedMap();
                ((OrderedMap)_logContext["upload"])["name"] = fileNames;

                _result["select"] = new OrderedMap();
                var _total = 0;
                for (int i = 0; i < myFileCollection.Count; i++)
                {
                    HttpPostedFile file = myFileCollection[i];
                    if (string.IsNullOrEmpty(file.FileName))
                        continue;
                    fileNames.Add(file.FileName);
                    _total++;
                    var fname = PathHelper.GetFileName(file.FileName);
                    if (false == _checkName(fname))
                        _errorData(file.FileName, "invalid name");
                    else if (!_isUploadAllow(fname, file.ContentType))
                        _errorData(file.FileName, "not allowed file type");
                    else
                    {
                        var filePath = PathHelper.Combine(_dir, fname);
                        if (!File.Exists(filePath))
                        {

                            byte[] fileData = new byte[file.InputStream.Length];
                            file.InputStream.Read(fileData, 0, fileData.Length);
                            File.Create(filePath, fileData);
                            //@chmod($file, $this->_options["filemode"]);
                            _result["select"] = new List<string>() { _hash(filePath) };

                        }
                        else
                            _errorData(file.FileName, "Unable to save uploaded file, File name already exists");
                    }
                }

                var _errcnt = _result.ContainsKey("errorData") ? ((OrderedMap)_result["errorData"]).Count : 0;

                if (_errcnt == _total)
                    _result["error"] = "unable to upload files";
                else
                {
                    if (_errcnt > 0)
                        _result["error"] = "some files was not uploaded";
                    _content(_dir);
                }
            }

        }


        public static SIO.MemoryStream GetZipEntries(string root, List<string> files)
        {
            SIO.MemoryStream outputStream = new SIO.MemoryStream();
            List<FileInfo> lst = new List<FileInfo>();
            FileSystemInfo temp = null;
            files.ForEach(o =>
            {
                temp = FileSystemInfo.GetEntry(o);
                if (temp.IsDirectory)
                {
                    lst.AddRange(((DirectoryInfo)temp).GetFiles(null, true));
                }
                else
                    lst.Add(temp as FileInfo);
            });

            using (ZipOutputStream zipStream = new ZipOutputStream(outputStream))
            {
                zipStream.UseZip64 = UseZip64.Off;
                lst.ForEach(data =>
                {
                    var entry = new ZipEntry(ZipEntry.CleanName(data.FullName.Replace(root + "\\", string.Empty)));
                    entry.Size = data.OriginalSize;
                    zipStream.PutNextEntry(entry);
                    if (data.Data != null && data.Data.Length > 0)
                        zipStream.Write(data.Data, 0, data.Data.Length);
                    zipStream.CloseEntry();
                });

                zipStream.Finish();
                zipStream.Close();
            }
            return outputStream;
        }

        /////**
        //// * Create archive of selected type
        //// *
        //// * @return void
        //// **/
        private void _archive()
        {
            try
            {
                _checkArchivers();
                var _GET = HttpContext.Current.Request.Form;
                var _type = _GET["type"];

                if (!_config.archivers.ContainsKey("create")
                || empty(_type)
                || !((OrderedMap)_config.archivers["create"]).ContainsKey(_type)
                || !_config.archiveMimes.ContainsKey(_type))
                {
                    _result["error"] = "Invalid parameters";
                    return;
                }

                var _current = _GET["current"].Trim();
                string[] _targets = _GET["targets[]"].Split(new char[] { ',' });
                string _dir = string.Empty;
                if (empty(_current)
                    || empty(_dir = _findDir(_current))
                    || _targets.Length == 0
                    || !_isAllowed(_dir, "write")
                )
                {
                    _result["error"] = "Invalid parameters";
                }
                var files = new List<string>();
                //$argc  = "";
                for (int i = 0; i < _targets.Length; i++)
                {
                    var o = _targets[i];
                    string f;
                    if (empty(f = _find(o, _dir)))
                    {
                        _result["error"] = "File not found";
                        return;
                    }
                    files.Add(f);
                };

                var arc = ((OrderedMap)_config.archivers["create"])[_type] as OrderedMap;
                var archiveName = _uniqueName(PathHelper.Combine(_dir, PathHelper.GetFileNameWithoutExtension(files.Count == 1 ? files[0] : _dir) + "." + arc["ext"]));
                SIO.MemoryStream stream = GetZipEntries(_dir, files) ?? new SIO.MemoryStream();
                File.Create(archiveName, stream.ToArray());
                _content(_dir);
                _result["select"] = new List<string>() { _hash(archiveName) };
            }
            catch (Exception)
            {
                _result["error"] = "Unable to create archive";
            }
        }

        ///**
        // * Extract files from archive
        // *
        // * @return void
        // **/
        private void _extract()
        {
            var _GET = HttpContext.Current.Request.Form;
            string _current = _GET["current"].Trim();
            string _target = _GET["target"].Trim();
            string _file = string.Empty;
            if (empty(_current)
            || empty(_current = _findDir(_current))
            || empty(_target)
            || empty(_file = _find(_target, _current))
            || !_isAllowed(_current, "write")
            )
            {
                _result["error"] = "Invalid parameters";
                return;
            }
            _checkArchivers();
            var mime = _mimetype(_file);

            if (!((OrderedMap)_config.archivers["extract"]).ContainsKey(mime))
            {
                _result["error"] = "Invalid parameters";
                return;
            }

            FileInfo info = new FileInfo(_file);
            if (info.Data != null && info.Data.Length > 0)
            {
                SIO.MemoryStream ms = new SIO.MemoryStream(info.Data);
                ZipInputStream zis = new ZipInputStream(ms);

                ZipEntry theEntry;
                string fullpath = string.Empty;
                _target = PathHelper.Combine(_current, PathHelper.GetFileNameWithoutExtension(info.Name));
                List<string> lstDirNames = new List<string>();
                int iCnt = 0;
                while ((theEntry = zis.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(theEntry.Name)) return;

                    fullpath = PathHelper.GetFullPath(PathHelper.Combine(_target, theEntry.Name));
                    if (theEntry.IsDirectory && !lstDirNames.Contains(fullpath))
                    {
                        if (Directory.CreateDirectory(fullpath))
                            lstDirNames.Add(fullpath);
                        iCnt++;
                    }
                    else
                    {
                        byte[] contentData = new byte[theEntry.Size];
                        zis.Read(contentData, 0, contentData.Length);
                        File.Create(fullpath, contentData);
                        fullpath = PathHelper.GetDirectoryName(fullpath);
                        if (!lstDirNames.Contains(fullpath))
                            lstDirNames.Add(fullpath);
                    }
                }

                //TODO : Remove when File Unzip is moved to File manager
                var cacheRepo = FileManagerUtil.BaseRepository as CacheFileSystem;
                if (cacheRepo != null)
                    cacheRepo.Refresh(_target);

                _content(_current, true);
            }
            else
                _result["error"] = "Unable to extract files from archive";

        }

        /************************************************************/
        /**                      fs methods                        **/
        /************************************************************/

        /**
        * Return name for duplicated file/folder or new archive
        *
        * @param  string  $f       file/folder name
        * @param  string  $suffix  file name suffix
        * @return string
        **/
        string _uniqueName(string f)
        {
            return _uniqueName(f, string.Empty);
        }

        string _uniqueName(string f, string prefix)
        {
            Dictionary<string, string> path = pathinfo(f);
            var _dir = path["dirname"];
            var _name = path["basename"];
            var _ext = path["extension"];

            List<string> lstNames = Directory.GetFileSystemEntries(_dir).Select(PathHelper.GetFileName).ToList();
            int iCnt = 0;
            var tempName = _name + _ext;
            while (iCnt++ <= 10000)
            {
                if (!lstNames.Contains(tempName))
                    break;
                if (tempName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) == false)
                    tempName = prefix + _name + _ext;
                else
                    tempName = prefix + _name + "(" + iCnt + ")" + _ext;
            }
            return PathHelper.Combine(_dir, tempName);
        }

        /**
       * Remove file or folder (recursively)
       *
       * @param  string  $path  fole/folder path
       * @return void
       **/
        bool Native_Delete(string path)
        {

            if (Directory.Exists(path))
            {
                try { Directory.Delete(path); }
                catch { _errorData(path, "Unable to remove folder"); return false; }
            }
            else
            {
                try { File.Delete(path); _rmTmb(path); }
                catch { _errorData(path, "Unable to remove file"); return false; }
            }
            return true;
        }

        bool Native_Copy(string source, string target)
        {
            try { File.Copy(source, target); }
            catch { } return true;
        }

        /**
       * Copy file/folder (recursively)
       *
       * @param  string  $src  file/folder to copy
       * @param  string  $trg  destination name
       * @return bool
       **/
        bool _copy(string _src, string _trg)
        {
            if (!_isAllowed(_src, "read"))
                return _errorData(_src, "Access denied");

            var _dir = dirname(_trg);

            if (!_isAllowed(_dir, "write"))
                return _errorData(_dir, "Access denied");
            if (File.Exists(_trg))
                return _errorData(_src, "File or folder with the same name already exists");

            if (!Directory.Exists(_src))
            {
                if (!Native_Copy(_src, _trg))
                    return _errorData(_src, "Unable to copy files");
            }
            else
            {
                if (!Native_CreateDir(_trg, _config.dirMode))
                    return _errorData(_src, "Unable to copy files");
                foreach (var source in new DirectoryInfo(_src).GetFileSystemInfos(null))
                {

                    var target = PathHelper.Combine(_trg, source.Name);
                    if (Directory.Exists(source.FullName))
                    {
                        if (!_copy(source.FullName, target))
                            return _errorData(source.FullName, "Unable to copy files");
                    }
                    else if (!Native_Copy(source.FullName, target))
                        return _errorData(source.FullName, "Unable to copy files");
                }
            }
            return true;
        }

        /**
        * Check new file name for invalid simbols. Return name if valid
        *
        * @return string  $n  file name
        * @return string
        **/
        bool _checkName(string name)
        {
            foreach (var o in new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' })
                if (name.Contains(o))
                    return false;
            return !name.StartsWith(".", StringComparison.InvariantCultureIgnoreCase);
        }

        /**
        * Find folder by hash in required folder and subfolders
        *
        * @param  string  $hash  folder hash
        * @param  string  $path  folder path to search in
        * @return string
        **/
        string _findDir(string hash)
        {
            return _findDir(hash, string.Empty);
        }
        string _findDir(string hash, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = _config.root;
                if (_hash(path) == hash)
                {
                    return path;
                }
            }

            foreach (var item in Directory.GetDirectories(path))
            {
                var p = item;
                if (_isAccepted(p))
                    if (_hash(p) == hash || string.IsNullOrEmpty(p = _findDir(hash, p)) == false)
                        return p;
            }
            return null;
        }

        /**
         * Find file/folder by hash in required folder
         *
         * @param  string  $hash  file/folder hash
         * @param  string  $path  folder path to search in
         **/
        string _find(string hash, string path)
        {
            foreach (var p in Directory.GetFileSystemEntries(path))
            {
                if (_isAccepted(p))
                {
                    if (_hash(p) == hash)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        /**
        * Count total directory size if this allowed in options
        *
        * @param  string  $path  directory path
        * @return int
        **/
        long _dirSize(string path)
        {
            long size = 0;
            if (!_config.dirSize || _isAllowed(path, "read"))
                return 0;
            if (filetype(path) == "dir")
            {
                foreach (var fi in new DirectoryInfo(path).GetFileSystemInfos(null))
                {
                    if (_isAccepted(fi.FullName))
                    {
                        size += _dirSize(fi.FullName);
                    }
                }
            }
            else
                size = new FileInfo(path).OriginalSize;
            return size;
        }

        /**
        * Return file mimetype
        *
        * @param  string  $path  file path
        * @return string
        **/
        string _mimetype(string path)
        {
            var pinfo = pathinfo(path);
            var ext = pinfo.ContainsKey("extension") && string.IsNullOrWhiteSpace(pinfo["extension"]) == false ? ((string)pinfo["extension"]).ToLower().Substring(1) : string.Empty;
            if (_mimeTypes.ContainsKey(ext))
                return _mimeTypes[ext];
            return "unknown;";
        }


        /************************************************************/
        /**                   image manipulation                   **/
        /************************************************************/

        /**
        * Create image thumbnail
        *
        * @param  string  $img  image file
        * @param  string  $tmb  thumbnail name
        * @return bool
        **/
        bool _tmb(string path, string tmb)
        {
            FileInfo imageFile = new FileInfo(path);
            if (imageFile.Exists && imageFile.Data != null && imageFile.Data.Length > 0)
            {
                using (var img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(imageFile.Data)))
                {
                    int tmbSize = _config.tmbSize;
                    switch (_config.imgLib)
                    {
                        case "imagick":
                            break;
                        case "mogrify":
                            break;
                        case "gd":
                            break;
                        default:
                            if (img.Width > tmbSize || img.Height > tmbSize)
                            {
                                using (var newImg = img.GetThumbnailImage(tmbSize, tmbSize, null, IntPtr.Zero))
                                {
                                    if (File.Exists(tmb))
                                    {
                                        try
                                        {
                                            File.Delete(tmb);
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    var tempStream = new System.IO.MemoryStream();
                                    newImg.Save(tempStream, ImageFormat.Png);
                                    File.Update(tmb, tempStream.ToArray());
                                }
                            }
                            else
                            {
                                var tempStream = new System.IO.MemoryStream();
                                img.Save(tempStream, ImageFormat.Png);
                                File.Update(tmb, tempStream.ToArray());
                            }
                            break;
                    }


                }
                return true;
            }
            return false;
        }

        /**
        * Remove image thumbnail
        *
        * @param  string  $img  image file
        * @return void
        **/
        void _rmTmb(string img)
        {
            try
            {
                string tmb = string.Empty;
                if (!empty(_config.tmbDir) && !empty(tmb = _tmbPath(img)) && File.Exists(tmb))
                    File.Delete(tmb);
            }
            catch (Exception)
            {
            }
        }

        /**
        * Return x/y coord for crop image thumbnail
        *
        * @param  int  $w  image width
        * @param  int  $h  image height	
        * @return array
        **/
        int[] _cropPos(int w, int h)
        {
            int x = 0, y = 0;
            int size = Math.Min(w, h);
            if (w > h)
                x = (int)Math.Ceiling((double)((w - h) / 2));
            else
                y = (int)Math.Ceiling((double)((h - w) / 2));
            return new int[] { x, y, size };
        }

        /**
        * Return true if we can create thumbnail for file with this mimetype
        *
        * @param  string  $mime  file mimetype
        * @return bool
        **/
        bool _canCreateTmb(string mime)
        {
            if (!_config.enableThumbnails) return false;
            if (!empty(_config.tmbDir) && !empty(_config.imgLib) && mime.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                return "image/jpeg".Equals(mime, StringComparison.InvariantCultureIgnoreCase) || "image/png".Equals(mime, StringComparison.InvariantCultureIgnoreCase) || "image/gif".Equals(mime, StringComparison.InvariantCultureIgnoreCase);
            return false;
        }

        /**
       * Return image thumbnail path. For thumbnail return itself 
       *
       * @param  string  $path  image path
       * @return string
       **/
        string _tmbPath(string path)
        {
            var tmb = "";
            if (!empty(_config.tmbDir))
            {
                tmb = dirname(path) != _config.tmbDir
                    ? PathHelper.Combine(_config.tmbDir, _hash(path) + ".png")
                    : path;
            }
            return tmb;
        }

        /**
         * Resize image
         *
         * @param  string  $img  image path
         * @param  int     $w    image width
         * @param  int     $h    image height
         * @return bool
         **/
        private bool _resizeImg(string _img, int w, int h)
        {
            FileInfo imageFile = new FileInfo(_img);
            if (imageFile.Exists && imageFile.Data != null && imageFile.Data.Length > 0)
            {
                using (var tempImage = System.Drawing.Image.FromStream(new System.IO.MemoryStream(imageFile.Data)))
                {
                    var newImage = tempImage.GetThumbnailImage(w, h, null, IntPtr.Zero);
                    var tempStream = new System.IO.MemoryStream();
                    newImage.Save(tempStream, ImageFormat.Png);
                    File.Update(_img, tempStream.ToArray());
                    return true;
                }
            }
            return false;
        }

        /**
        * Resize image
        *
        * @return void
        **/
        private void _resize()
        {
            var _GET = HttpContext.Current.Request.Form;
            var _current = string.Empty;
            var _target = string.Empty;
            int _width, _height;
            if (empty(_GET["current"])
            || empty(_current = _findDir(_GET["current"].Trim()))
            || empty(_GET["target"])
            || empty(_target = _find(_GET["target"].Trim(), _current))
            || empty(_GET["width"]) || 0 >= (_width = ValueTypeParser.Parse<int>(_GET["width"].Trim(), -1))
            || empty(_GET["height"]) || 0 >= (_height = ValueTypeParser.Parse<int>(_GET["height"].Trim(), -1))
            )
                _result["error"] = "Invalid parameters";
            else
            {
                _logContext = new OrderedMap{
                {"target",_target},
                {"width",_width},
                {"height",_height}
                };
                if (!_isAllowed(_target, "write"))
                    _result["error"] = "Access denied";
                else if (!_mimetype(_target).StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                    _result["error"] = "File is not an image";
                else if (!_resizeImg(_target, _width, _height))
                    _result["error"] = "Unable to resize image";
                else
                {
                    _result["select"] = new List<string>() { _hash(_target) };
                    _content(_current);
                }
            }

        }



        /************************************************************/
        /**                       access control                   **/
        /************************************************************/
        /**
         * Return true if file's mimetype is allowed for upload
         *
         * @param  string  $name    file name
         * @param  string  $tmpName uploaded file tmp name
         * @return bool
         **/
        private bool _isUploadAllow(string _name, string _tmpName)
        {
            var _mime = _config.mimeDetect != "auto" ? _tmpName : _mimetype(_name);
            var _allow = false;
            var _deny = false;

            if (_config.uploadAllow.Contains("all"))
                _allow = true;
            else
            {
                foreach (var item in _config.uploadAllow)
                {
                    if (_mime.StartsWith(item, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _allow = true;
                        break;
                    }
                }
            }

            if (_config.uploadDeny.Contains("all"))
                _deny = true;
            else
            {
                foreach (var item in _config.uploadDeny)
                {
                    if (_mime.StartsWith(item, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _deny = true;
                        break;
                    }
                }
            }
            return _config.uploadOrder.StartsWith("allow", StringComparison.InvariantCultureIgnoreCase) ? _allow && _deny : _allow || _deny;
        }

        /**
        * Return true if file name is not . or ..
        * If file name begins with . return value according to $this->_options['dotFiles']
        *
        * @param  string  $file  file name
        * @return bool
        **/
        bool _isAccepted(string file)
        {
            if ("." == file || ".." == file || _config.tmbDir.Equals(file, StringComparison.InvariantCultureIgnoreCase) || (!_config.dotFiles && BaseName(file).StartsWith(".", StringComparison.InvariantCultureIgnoreCase)))
                return false;
            return true;
        }

        /**
        * Return true if requeired action allowed to file/folder
        *
        * @param  string  $path    file/folder path
        * @param  string  $action  action name (read/write/rm)
        * @return void
        **/
        bool _isAllowed(string path, string action)
        {
            return _config.defaults[action];
        }


        /************************************************************/
        /**                          utilites                      **/
        /************************************************************/

        /**
        * Return image manipalation library name
        *
        * @return string
        **/
        string _getImgLib()
        {
            return "auto";
            //if (extension_loaded('imagick')) {
            //    return 'imagick';
            //} elseif (function_exists('exec')) {
            //    exec('mogrify --version', $o, $c);
            //    if ($c == 0) {
            //        return 'mogrify';
            //    }
            //}
            //return function_exists('gd_info') ? 'gd' : '';
        }

        /**
         * Return list of available archivers
         *
         * @return array
         **/
        void _checkArchivers()
        {
            var arcs = new OrderedMap{
                    {"create",new OrderedMap()},
                    {"extract", new OrderedMap()}
                };
            ((OrderedMap)arcs["create"])["application/zip"] = new OrderedMap { { "cmd", "zip" }, { "argc", "" }, { "ext", "zip" } };
            ((OrderedMap)arcs["extract"])["application/zip"] = new OrderedMap { { "cmd", "unzip" }, { "argc", "" }, { "ext", "zip" } };

            _config.archivers = arcs;
            if (_config.archiveMimes.Count == 0)
                _config.archiveMimes = ((OrderedMap)_config.archivers)["create"] as OrderedMap;

        }

        /**
        * Return file path hash
        *
        * @param  string  $path 
        * @return string
        **/
        string _hash(string path)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(path, "md5").ToLower();
        }


        /**
        * Return file URL
        *
        * @param  string  $path 
        * @return string
        **/
        string _path2url(string physicalPath)
        {
            return PathHelper.VirtualPath(physicalPath);
            //var dir = substr(dirname(path), _config.root.Length + 1);
            //var file = rawurlencode(BaseName(path, string.Empty));
            //return _config.URL + (!string.IsNullOrEmpty(dir) ? dir.Replace(Data.Path.DirectorySeparatorChar + "", "/") + "/" : string.Empty) + file;
        }

        /**
        * Paack error message in $this->_result['errorData']
        *
        * @param string  $path  path to file
        * @param string  $msg   error message
        * @return bool always false
        **/
        bool _errorData(string path, string msg)
        {
            //TODO: Check this
            //$path = preg_replace('|^'.preg_quote($this->_options['root']).'|', $this->_fakeRoot, $path);
            if (!_result.ContainsKey("errorData") || (_result.ContainsKey("errorData") && !(_result["errorData"] is OrderedMap)))
                _result["errorData"] = new OrderedMap();
            ((OrderedMap)_result["errorData"])[path] = msg;
            return false;
        }

        int _utime()
        {
            long initialTicks = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
            long dateTicks = DateTime.Now.ToUniversalTime().Ticks;
            int elapsedSeconds = System.Convert.ToInt32((dateTicks - initialTicks) / 10000000);
            return elapsedSeconds;
        }

    }
}
