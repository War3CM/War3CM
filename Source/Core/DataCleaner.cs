using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Phanmemwar3.Core
{
    public enum CleanCategory
    {
        MapCache,
        SaveTrashAndHistory,
        GameLogsAndTemp,
        PluginBackups,
        OldBackupSlots
    }

    public sealed class CleanCategoryInfo
    {
        public CleanCategory Category { get; set; }
        public string TitleKey { get; set; } = "";
        public string DescKey { get; set; } = "";
        public int FileCount { get; set; }
        public long TotalBytes { get; set; }
        public List<string> TargetFiles { get; } = new List<string>();
        public List<string> TargetDirectories { get; } = new List<string>();

        public string SizeDisplay => FormatSize(TotalBytes);

        public static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }

    public sealed class CleanReport
    {
        public int DeletedFiles { get; set; }
        public int DeletedDirs { get; set; }
        public long FreedBytes { get; set; }
        public List<string> Errors { get; } = new List<string>();

        public string FreedSizeDisplay => CleanCategoryInfo.FormatSize(FreedBytes);
    }

    public static class DataCleaner
    {
        private static readonly Regex BackupSlotRegex = new Regex(@" - \d{8}-\d{6}-\d{3}\.ini$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<CleanCategoryInfo> Scan(string appDir, string? war3Dir)
        {
            var list = new List<CleanCategoryInfo>();

            // 1. Map Cache (Maps\WPM)
            var catMap = new CleanCategoryInfo
            {
                Category = CleanCategory.MapCache,
                TitleKey = "cleanCatMapCache",
                DescKey = "cleanCatMapCacheDesc"
            };
            if (!string.IsNullOrWhiteSpace(war3Dir) && Directory.Exists(war3Dir))
            {
                string wpmFolder = Path.Combine(war3Dir, "Maps", "WPM");
                ScanDirectoryFiles(wpmFolder, catMap, true);
            }
            list.Add(catMap);

            // 2. Save Trash & History
            var catTrash = new CleanCategoryInfo
            {
                Category = CleanCategory.SaveTrashAndHistory,
                TitleKey = "cleanCatTrashHistory",
                DescKey = "cleanCatTrashHistoryDesc"
            };
            string savesRoot = Path.Combine(appDir, "Saves");
            if (Directory.Exists(savesRoot))
            {
                foreach (var mapDir in Directory.GetDirectories(savesRoot))
                {
                    string trashDir = Path.Combine(mapDir, "_Trash");
                    ScanDirectoryFiles(trashDir, catTrash, true);

                    string histDir = Path.Combine(mapDir, "_History");
                    ScanDirectoryFiles(histDir, catTrash, true);
                }

                string prevRoot = Path.Combine(savesRoot, "_PreviousRoot");
                ScanDirectoryFiles(prevRoot, catTrash, true);
            }
            list.Add(catTrash);

            // 3. Game Logs & Temp
            var catGame = new CleanCategoryInfo
            {
                Category = CleanCategory.GameLogsAndTemp,
                TitleKey = "cleanCatGameTemp",
                DescKey = "cleanCatGameTempDesc"
            };
            if (!string.IsNullOrWhiteSpace(war3Dir) && Directory.Exists(war3Dir))
            {
                string pluginIni = Path.Combine(war3Dir, "dz_w3_plugin.ini");
                if (File.Exists(pluginIni))
                {
                    try
                    {
                        var fi = new FileInfo(pluginIni);
                        catGame.TargetFiles.Add(pluginIni);
                        catGame.FileCount++;
                        catGame.TotalBytes += fi.Length;
                    }
                    catch { }
                }

                ScanDirectoryFiles(Path.Combine(war3Dir, "Errors"), catGame, true);
                ScanDirectoryFiles(Path.Combine(war3Dir, "Logs"), catGame, true);
            }
            list.Add(catGame);

            // 4. Plugin Backups
            var catPlugin = new CleanCategoryInfo
            {
                Category = CleanCategory.PluginBackups,
                TitleKey = "cleanCatPluginBackups",
                DescKey = "cleanCatPluginBackupsDesc"
            };
            string pluginBackups = Path.Combine(appDir, "Profiles", "_PluginBackups");
            ScanDirectoryFiles(pluginBackups, catPlugin, true);
            list.Add(catPlugin);

            // 5. Old Backup Slots
            var catBackups = new CleanCategoryInfo
            {
                Category = CleanCategory.OldBackupSlots,
                TitleKey = "cleanCatBackupSlots",
                DescKey = "cleanCatBackupSlotsDesc"
            };
            if (Directory.Exists(savesRoot))
            {
                foreach (var mapDir in Directory.GetDirectories(savesRoot))
                {
                    try
                    {
                        foreach (var file in Directory.EnumerateFiles(mapDir, "*.ini", SearchOption.TopDirectoryOnly))
                        {
                            if (BackupSlotRegex.IsMatch(file))
                            {
                                try
                                {
                                    var fi = new FileInfo(file);
                                    catBackups.TargetFiles.Add(file);
                                    catBackups.FileCount++;
                                    catBackups.TotalBytes += fi.Length;
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
            list.Add(catBackups);

            return list;
        }

        private static void ScanDirectoryFiles(string dir, CleanCategoryInfo cat, bool deleteDirIfEmpty)
        {
            if (!Directory.Exists(dir)) return;
            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        cat.TargetFiles.Add(file);
                        cat.FileCount++;
                        cat.TotalBytes += fi.Length;
                    }
                    catch { }
                }
                if (deleteDirIfEmpty)
                {
                    cat.TargetDirectories.Add(dir);
                }
            }
            catch { }
        }

        public static CleanReport ExecuteClean(IEnumerable<CleanCategoryInfo> categories)
        {
            var report = new CleanReport();

            foreach (var cat in categories)
            {
                // Delete files first
                foreach (var file in cat.TargetFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            long len = 0;
                            try { len = new FileInfo(file).Length; } catch { }
                            File.Delete(file);
                            report.DeletedFiles++;
                            report.FreedBytes += len;
                        }
                    }
                    catch (Exception ex)
                    {
                        report.Errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                // Delete directories
                foreach (var dir in cat.TargetDirectories)
                {
                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true);
                            report.DeletedDirs++;
                        }
                    }
                    catch (Exception ex)
                    {
                        report.Errors.Add($"{Path.GetFileName(dir)}: {ex.Message}");
                    }
                }
            }

            return report;
        }
    }
}
