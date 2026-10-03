using System;
using System.IO;
using Microsoft.Win32;

namespace Phanmemwar3.Core
{
    public static class RegistryHelper
    {
        private const string HKCU_WAR3 = @"Software\Blizzard Entertainment\Warcraft III";
        private const string HKLM_WAR3_64 = @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Warcraft III";
        private const string HKCU_WAR3_VIDEO = @"Software\Blizzard Entertainment\Warcraft III\Video";

        public const string DefaultOriginalWar3Path = @"D:\Game\Warcraft3 1.27 DZ\Warcraft 3.2";

        public static string? GetWar3InstallPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(HKCU_WAR3);
                if (key != null)
                {
                    var val = key.GetValue("InstallPath") as string;
                    if (!string.IsNullOrEmpty(val) && Directory.Exists(val))
                        return val;
                }
            }
            catch { }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(HKLM_WAR3_64);
                if (key != null)
                {
                    var val = key.GetValue("InstallPath") as string;
                    if (!string.IsNullOrEmpty(val) && Directory.Exists(val))
                        return val;
                }
            }
            catch { }

            return null;
        }

        public static bool RestoreToOriginalPath()
        {
            if (Directory.Exists(DefaultOriginalWar3Path))
            {
                return SetWar3InstallPath(DefaultOriginalWar3Path);
            }
            return false;
        }

        public static bool SetWar3InstallPath(string war3Path)
        {
            if (string.IsNullOrWhiteSpace(war3Path) || !Directory.Exists(war3Path))
                return false;

            string normalized = Path.GetFullPath(war3Path).TrimEnd('\\');
            string war3Exe = Path.Combine(normalized, "war3.exe");

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(HKCU_WAR3, true);
                if (key != null)
                {
                    key.SetValue("InstallPath", normalized, RegistryValueKind.String);
                    key.SetValue("InstallPathX", normalized, RegistryValueKind.String);
                    key.SetValue("war3.exe", war3Exe, RegistryValueKind.String);
                    key.SetValue("Allow Local Files", 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(HKLM_WAR3_64, true);
                if (key != null)
                {
                    key.SetValue("InstallPath", normalized, RegistryValueKind.String);
                    key.SetValue("GamePath", war3Exe, RegistryValueKind.String);
                    key.SetValue("Allow Local Files", 1, RegistryValueKind.DWord);
                }
            }
            catch { }

            SetHighDpiAware(normalized);

            return true;
        }

        public static void SetAllowLocalFiles(bool allow = true)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(HKCU_WAR3, true);
                if (key != null)
                {
                    key.SetValue("Allow Local Files", allow ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        public static void SetVideoResolution(int width, int height)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(HKCU_WAR3_VIDEO, true);
                if (key != null)
                {
                    key.SetValue("reswidth", width, RegistryValueKind.DWord);
                    key.SetValue("resheight", height, RegistryValueKind.DWord);
                    key.SetValue("colordepth", 32, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        public static void SetHighDpiAware(string war3Path)
        {
            if (string.IsNullOrWhiteSpace(war3Path) || !Directory.Exists(war3Path))
                return;

            string normalized = Path.GetFullPath(war3Path).TrimEnd('\\');
            string war3Exe = Path.Combine(normalized, "war3.exe");
            string war3ExeUpper = Path.Combine(normalized, "War3.exe");

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                if (key != null)
                {
                    key.SetValue(war3Exe, "~ HIGHDPIAWARE", RegistryValueKind.String);
                    key.SetValue(war3ExeUpper, "~ HIGHDPIAWARE", RegistryValueKind.String);
                }
            }
            catch { }
        }
        public static bool SetPlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return false;

            string name = playerName.Trim();
            if (name.Length > 10) name = name.Substring(0, 10);

            bool ok = false;
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(HKCU_WAR3, true);
                if (key != null)
                {
                    key.SetValue("Player Name", name, RegistryValueKind.String);
                    ok = true;
                }
            }
            catch { }

            try
            {
                using var strKey = Registry.CurrentUser.CreateSubKey(HKCU_WAR3 + @"\String", true);
                if (strKey != null)
                {
                    strKey.SetValue("userlocal", name, RegistryValueKind.String);
                    strKey.SetValue("userbnet", name, RegistryValueKind.String);
                    ok = true;
                }
            }
            catch { }

            return ok;
        }

        public static string? GetPlayerName()
        {
            try
            {
                using var strKey = Registry.CurrentUser.OpenSubKey(HKCU_WAR3 + @"\String");
                var val = strKey?.GetValue("userlocal") as string;
                if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
            }
            catch { }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(HKCU_WAR3);
                var val = key?.GetValue("Player Name") as string;
                if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
            }
            catch { }

            return null;
        }
    }
}
