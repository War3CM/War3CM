using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Phanmemwar3.Core
{
    public class ConfigManager
    {
        private readonly string _settingsPath;
        private readonly string _langPath;
        private readonly Dictionary<string, Dictionary<string, string>> _settingsData = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _langData = new(StringComparer.OrdinalIgnoreCase);

        public ConfigManager(string appDir)
        {
            _settingsPath = Path.Combine(appDir, "settings.ini");
            _langPath = Path.Combine(appDir, "lang.ini");
            LoadSettings();
            LoadLang();
        }

        public void LoadSettings()
        {
            _settingsData.Clear();
            if (File.Exists(_settingsPath))
            {
                ParseIni(_settingsPath, _settingsData);
            }
        }

        public void LoadLang()
        {
            _langData.Clear();
            if (File.Exists(_langPath))
            {
                ParseIni(_langPath, _langData);
            }
            else
            {
                // Fallback to embedded lang.ini resource when running standalone EXE on a new machine
                try
                {
                    var asm = typeof(ConfigManager).Assembly;
                    string resName = asm.GetManifestResourceNames()
                        .FirstOrDefault(n => n.EndsWith("lang.ini", StringComparison.OrdinalIgnoreCase)) ?? "";
                    if (!string.IsNullOrEmpty(resName))
                    {
                        using var stream = asm.GetManifestResourceStream(resName);
                        if (stream != null)
                        {
                            var lines = new List<string>();
                            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                            {
                                string? l;
                                while ((l = reader.ReadLine()) != null) lines.Add(l);
                            }
                            ParseIniLines(lines, _langData);

                            // Auto-extract lang.ini to disk so user can customize translations if needed
                            try
                            {
                                File.WriteAllLines(_langPath, lines, System.Text.Encoding.UTF8);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            if (_settingsData.TryGetValue("Settings", out var section) && section.TryGetValue(key, out var val))
            {
                return val;
            }
            return defaultValue;
        }

        public void SetSetting(string key, string value)
        {
            if (!_settingsData.ContainsKey("Settings"))
            {
                _settingsData["Settings"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            _settingsData["Settings"][key] = value;
        }

        public void SaveSettings()
        {
            SaveIni(_settingsPath, _settingsData);
        }

        public string GetText(string key, string lang = "VN")
        {
            if (_langData.TryGetValue(lang, out var section) && section.TryGetValue(key, out var text))
            {
                return text;
            }
            // Fallback to VN or EN
            if (_langData.TryGetValue("VN", out var vnSection) && vnSection.TryGetValue(key, out var vnText))
            {
                return vnText;
            }
            return key;
        }

        private static void ParseIni(string filePath, Dictionary<string, Dictionary<string, string>> target)
        {
            if (File.Exists(filePath))
            {
                ParseIniLines(File.ReadAllLines(filePath), target);
            }
        }

        private static void ParseIniLines(IEnumerable<string> lines, Dictionary<string, Dictionary<string, string>> target)
        {
            string currentSection = "";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                    continue;

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    if (!target.ContainsKey(currentSection))
                    {
                        target[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                else if (!string.IsNullOrEmpty(currentSection))
                {
                    int eqIdx = trimmed.IndexOf('=');
                    if (eqIdx > 0)
                    {
                        string k = trimmed.Substring(0, eqIdx).Trim();
                        string v = trimmed.Substring(eqIdx + 1).Trim();
                        target[currentSection][k] = v;
                    }
                }
            }
        }

        private static void SaveIni(string filePath, Dictionary<string, Dictionary<string, string>> source)
        {
            using var sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            foreach (var section in source)
            {
                sw.WriteLine($"[{section.Key}]");
                foreach (var kvp in section.Value)
                {
                    sw.WriteLine($"{kvp.Key} = {kvp.Value}");
                }
                sw.WriteLine();
            }
        }
    }
}
