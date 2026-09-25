using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Phanmemwar3.Core
{
    public class PluginProfile
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string SourcePath { get; set; } = "";
        public bool IsClean { get; set; } = false;
        public bool IsInstalled { get; set; } = false;
        public string Origin { get; set; } = "";
        public string? DetectedVersion { get; set; } = null;

        public override string ToString() => DisplayName;
    }

    public class PluginManager
    {
        private readonly string _appDir;
        private readonly string _profilesDir;
        private readonly string _emulatorBaseDir;
        private string? _lastRootSnapshot;

        public PluginManager(string appDir)
        {
            _appDir = appDir;
            _profilesDir = Path.Combine(appDir, "Profiles");
            _emulatorBaseDir = Path.Combine(_profilesDir, "_Emulator_Base");
            Directory.CreateDirectory(_profilesDir);
        }

        public static string? DetectPluginVersion(string? war3Dir)
        {
            if (string.IsNullOrWhiteSpace(war3Dir) || !Directory.Exists(war3Dir)) return null;
            string weRoot = Path.Combine(war3Dir, "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C");
            string[] candidateDlls = new[]
            {
                Path.Combine(war3Dir, "dz_w3_plugin.dll"),
                Path.Combine(war3Dir, "kkapi_local_plugin.dll"),
                Path.Combine(weRoot, "plugin", "warcraft3", "kkapi_local_plugin.dll"),
                Path.Combine(weRoot, "plugin", "warcraft3", "dz_w3_plugin.dll")
            };

            foreach (var path in candidateDlls)
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var fileInfo = new FileInfo(path);
                    long len = fileInfo.Length;
                    if (len == 1853400) return "KKWE 2.0.12.2606";
                    if (len == 1706944) return "KKWE 2.0.12.2485";
                    if (len == 573432) return "Plugin 270";

                    var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                    if (!string.IsNullOrEmpty(ver.ProductVersion) && ver.ProductVersion != "0.0.0.0")
                        return "KKWE " + ver.ProductVersion;
                    if (!string.IsNullOrEmpty(ver.FileVersion) && ver.FileVersion != "0.0.0.0")
                        return "KKWE " + ver.FileVersion;
                }
                catch { }
            }
            return null;
        }

        public List<PluginProfile> GetAvailableProfiles(string war3Dir = null)
        {
            var list = new List<PluginProfile>
            {
                new PluginProfile
                {
                    Id = "CLEAN",
                    DisplayName = "Warcraft Clean (Nguyên bản Blizzard - Không Plugin)",
                    IsClean = true
                }
            };

            string weRoot = Path.Combine(war3Dir ?? "", "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C");
            if (!string.IsNullOrWhiteSpace(war3Dir) && Directory.Exists(war3Dir) &&
                new[] { "dz_w3_plugin.dll", "kkapi.dll", "kkapi_local_plugin.dll", "version.dll" }
                    .Any(name => File.Exists(Path.Combine(war3Dir, name)) ||
                        File.Exists(Path.Combine(weRoot, "plugin", "warcraft3", name))))
            {
                string? detectedVer = DetectPluginVersion(war3Dir);
                string installedName = detectedVer != null
                    ? $"Installed game plugins ({detectedVer})"
                    : "Installed game plugins";

                list.Add(new PluginProfile
                {
                    Id = "INSTALLED",
                    DisplayName = installedName,
                    IsInstalled = true,
                    Origin = "game",
                    SourcePath = war3Dir,
                    DetectedVersion = detectedVer
                });
            }

            void AddProfiles(string parent, string origin)
            {
                if (!Directory.Exists(parent)) return;
                string[] directories;
                try { directories = Directory.GetDirectories(parent); }
                catch (IOException) { return; }
                catch (UnauthorizedAccessException) { return; }
                foreach (var dir in directories)
                {
                    string name = Path.GetFileName(dir);
                    if (name.StartsWith("_", StringComparison.OrdinalIgnoreCase) ||
                        (origin == "backup" && !File.Exists(Path.Combine(dir, "fingerprint.txt"))) ||
                        new[] { "bin", "plugin", "jass", "share", "scripts", "components", "logs", "maps" }
                            .Contains(name, StringComparer.OrdinalIgnoreCase) || !HasProfileFiles(dir))
                        continue;
                    if (list.Exists(p => string.Equals(p.SourcePath, dir, StringComparison.OrdinalIgnoreCase))) continue;
                    list.Add(new PluginProfile
                    {
                        Id = origin + ":" + name,
                        DisplayName = name,
                        SourcePath = dir,
                        Origin = origin,
                        IsClean = false
                    });
                }
            }

            AddProfiles(_profilesDir, "app");
            AddProfiles(Path.Combine(_profilesDir, "_PluginBackups"), "backup");

            // Also check if War3 directory has 4_we_WorldEdit with existing profiles
            if (!string.IsNullOrEmpty(war3Dir) && Directory.Exists(war3Dir))
            {
                AddProfiles(Path.Combine(war3Dir, "Profiles"), "game");
                AddProfiles(weRoot, "game");
            }

            return list;
        }

        private static bool HasProfileFiles(string dir) =>
            new[] { "dz_w3_plugin.dll", "kkapi_local_plugin.dll", "kkapi.dll", "version.dll" }
                .Any(name => File.Exists(Path.Combine(dir, name)) ||
                    File.Exists(Path.Combine(dir, "plugin", "warcraft3", name))) ||
            Directory.Exists(Path.Combine(dir, "bin")) || Directory.Exists(Path.Combine(dir, "plugin"));

        public bool ApplyProfile(string war3Dir, PluginProfile profile)
        {
            if (string.IsNullOrWhiteSpace(war3Dir) || !Directory.Exists(war3Dir))
                return false;

            if (profile.IsInstalled) return true; // Never alter files selected as already installed.

            // 1. Clean root plugin DLLs and emulator files first
            _lastRootSnapshot = null;
            string? originalRoot = CleanWar3RootPlugins(war3Dir);
            _lastRootSnapshot = originalRoot;
            try
            {
                if (profile.IsClean) return true;
                DeployEmulatorBase(war3Dir);
                DeployProfileDllsToRoot(war3Dir, profile);
                DeployWorldEditEnvironment(war3Dir, profile);
                RegistryHelper.SetWar3InstallPath(war3Dir);
                SaveValueMuter.PatchPluginDlls(war3Dir, true);
                return true;
            }
            catch
            {
                RestoreRootBackup(war3Dir, originalRoot);
                _lastRootSnapshot = null;
                throw;
            }
        }

        public void UndoLastRootSwitch(string war3Dir)
        {
            RestoreRootBackup(war3Dir, _lastRootSnapshot);
            _lastRootSnapshot = null;
        }

        private static readonly string[] CleanupFiles = {
            "dz_w3_plugin.dll", "kkapi_local_plugin.dll", "kkapi.dll", "version.dll",
            "dznep.dll", "GameDll_fix_mapsize_limit.mix", "NEPDaemon.exe"
        };

        private static string Fingerprint(IEnumerable<string> files)
        {
            var digests = new StringBuilder();
            foreach (string file in files.OrderBy(p => Path.GetFileName(p), StringComparer.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(file);
                digests.Append(Path.GetFileName(file)).Append(':')
                    .Append(Convert.ToHexString(SHA256.HashData(stream))).Append('\n');
            }
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(digests.ToString())));
        }

        private static bool MatchesSnapshot(string dir, string fingerprint, int expectedFiles)
        {
            try
            {
                string marker = Path.Combine(dir, "fingerprint.txt");
                if (!File.Exists(marker) || File.ReadAllText(marker) != fingerprint) return false;
                string[] files = Directory.GetFiles(dir).Where(p => p != marker).ToArray();
                return files.Length == expectedFiles && Fingerprint(files) == fingerprint;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        private static void RestoreRootBackup(string war3Dir, string? snapshot)
        {
            foreach (string name in CleanupFiles)
            {
                string target = Path.Combine(war3Dir, name);
                if (File.Exists(target)) File.Delete(target);
            }
            if (snapshot == null) return;
            foreach (string file in Directory.GetFiles(snapshot))
            {
                if (Path.GetFileName(file).Equals("fingerprint.txt", StringComparison.OrdinalIgnoreCase)) continue;
                File.Copy(file, Path.Combine(war3Dir, Path.GetFileName(file)), false);
            }
        }

        public string? CleanWar3RootPlugins(string war3Dir)
        {
            // Plugin switches used to destroy installed DLLs. Preserve them as a selectable
            // profile before changing anything in the game's root directory.
            var present = CleanupFiles.Select(name => Path.Combine(war3Dir, name)).Where(File.Exists).ToArray();
            string? snapshot = null;
            if (present.Length > 0)
            {
                string archive = Path.Combine(_profilesDir, "_PluginBackups");
                Directory.CreateDirectory(archive);
                string fingerprint = Fingerprint(present);
                snapshot = Directory.EnumerateDirectories(archive).FirstOrDefault(dir =>
                    MatchesSnapshot(dir, fingerprint, present.Length));
                if (snapshot == null)
                {
                    snapshot = Path.Combine(archive,
                        DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N")[..8]);
                    Directory.CreateDirectory(snapshot);
                    foreach (string source in present)
                        File.Copy(source, Path.Combine(snapshot, Path.GetFileName(source)), false);
                    File.WriteAllText(Path.Combine(snapshot, "fingerprint.txt"), fingerprint);
                }
            }

            try
            {
                foreach (var f in CleanupFiles)
                {
                    string target = Path.Combine(war3Dir, f);
                    if (File.Exists(target)) File.Delete(target);
                }
            }
            catch
            {
                RestoreRootBackup(war3Dir, snapshot);
                throw;
            }
            return snapshot;
        }

        private void DeployEmulatorBase(string war3Dir)
        {
            if (Directory.Exists(_emulatorBaseDir))
            {
                foreach (var file in Directory.GetFiles(_emulatorBaseDir))
                {
                    // Save profiles own this file; changing plugins must not overwrite it.
                    if (Path.GetFileName(file).Equals("dz_w3_plugin.ini", StringComparison.OrdinalIgnoreCase)) continue;
                    string target = Path.Combine(war3Dir, Path.GetFileName(file));
                    File.Copy(file, target, true);
                }
            }
        }

        private void DeployProfileDllsToRoot(string war3Dir, PluginProfile profile)
        {
            string[] dlls = { "dz_w3_plugin.dll", "kkapi_local_plugin.dll", "kkapi.dll", "version.dll" };

            if (!string.IsNullOrEmpty(profile.SourcePath) && Directory.Exists(profile.SourcePath))
            {
                if (profile.Origin == "backup")
                {
                    foreach (string file in Directory.GetFiles(profile.SourcePath))
                    {
                        if (Path.GetFileName(file).Equals("fingerprint.txt", StringComparison.OrdinalIgnoreCase)) continue;
                        File.Copy(file, Path.Combine(war3Dir, Path.GetFileName(file)), true);
                    }
                    return;
                }
                foreach (var dll in dlls)
                {
                    string p = Path.Combine(profile.SourcePath, dll);
                    if (File.Exists(p))
                    {
                        File.Copy(p, Path.Combine(war3Dir, dll), true);
                    }
                }

                // Check inside plugin/warcraft3
                string w3PluginDir = Path.Combine(profile.SourcePath, "plugin", "warcraft3");
                if (Directory.Exists(w3PluginDir))
                {
                    string kkLocal = Path.Combine(w3PluginDir, "kkapi_local_plugin.dll");
                    if (File.Exists(kkLocal))
                    {
                        File.Copy(kkLocal, Path.Combine(war3Dir, "dz_w3_plugin.dll"), true);
                        File.Copy(kkLocal, Path.Combine(war3Dir, "kkapi_local_plugin.dll"), true);
                    }
                    string kk = Path.Combine(w3PluginDir, "kkapi.dll");
                    if (File.Exists(kk))
                    {
                        File.Copy(kk, Path.Combine(war3Dir, "kkapi.dll"), true);
                    }
                }
            }
        }

        private void DeployWorldEditEnvironment(string war3Dir, PluginProfile profile)
        {
            string weRoot = Path.Combine(war3Dir, "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C");
            Directory.CreateDirectory(weRoot);

            if (!string.IsNullOrEmpty(profile.SourcePath) && Directory.Exists(profile.SourcePath))
            {
                string srcBin = Path.Combine(profile.SourcePath, "bin");
                string dstBin = Path.Combine(weRoot, "bin");
                if (Directory.Exists(srcBin))
                {
                    CopyDirectory(srcBin, dstBin);
                }

                string srcPlugin = Path.Combine(profile.SourcePath, "plugin");
                string dstPlugin = Path.Combine(weRoot, "plugin");
                if (Directory.Exists(srcPlugin))
                {
                    CopyDirectory(srcPlugin, dstPlugin);
                }

                string srcJass = Path.Combine(profile.SourcePath, "jass");
                string dstJass = Path.Combine(weRoot, "jass");
                if (Directory.Exists(srcJass))
                {
                    CopyDirectory(srcJass, dstJass);
                }
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }
    }
}
