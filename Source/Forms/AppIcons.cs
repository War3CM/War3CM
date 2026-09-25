using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Phanmemwar3.Forms
{
    public static class AppIcons
    {
        private static Icon? _icon;
        private static bool _attempted;

        public static Icon? GetAppIcon()
        {
            if (_attempted) return _icon;
            _attempted = true;

            try
            {
                var asm = typeof(AppIcons).Assembly;
                string? res = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("WarcraftPlatformManager.ico", StringComparison.OrdinalIgnoreCase));
                if (res != null)
                {
                    using var stream = asm.GetManifestResourceStream(res);
                    if (stream != null)
                    {
                        _icon = new Icon(stream);
                        return _icon;
                    }
                }
            }
            catch { }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidates = new[]
                {
                    Path.Combine(baseDir, "WarcraftPlatformManager.ico"),
                    Path.Combine(baseDir, "Resources", "WarcraftPlatformManager.ico"),
                    Path.Combine(baseDir, "..", "..", "..", "WarcraftPlatformManager.ico")
                };
                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        _icon = new Icon(path);
                        return _icon;
                    }
                }
            }
            catch { }

            return _icon;
        }
    }
}
