using System;
using System.Configuration;

namespace Xplorer.Core.Data
{
    internal static class Configuration
    {
        public static string DatabaseConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["FileManagerDB"].ConnectionString;
            }
        }

    }

    public class FileDbContext : IDisposable
    {
        public static void Using(Action<FileDBDataContext> action)
        {
            using (var context = new FileDbContext())
            {
                action(context.Current);
            }
        }

        private FileDbContext()
        {
            if (_depth == 0)
            {
                _context = new FileDBDataContext(Configuration.DatabaseConnection);
            }
            this.Current = _context;
            _depth++;
        }

        [ThreadStatic]
        private static int _depth;
        [ThreadStatic]
        private static FileDBDataContext _context;

        private FileDBDataContext Current { get; set; }


        #region IDisposable Members

        public void Dispose()
        {
            _depth--;
            if (_depth == 0)
            {
                if (_context != null)
                    _context.Dispose();
                _context = null;
            }
        }

        #endregion
    }
}
