using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Phanmemwar3.Core
{
    public class AppUpdateInfo
    {
        public string Version { get; set; } = "";
        public string Title { get; set; } = "";
        public string Changelog { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long FileSize { get; set; }
    }

    public static class AppUpdater
    {
        public const string CURRENT_VERSION = "1.0.3";
        public const string GITHUB_REPO = "War3CM/War3CM";
        public const string RELEASES_API_URL = "https://api.github.com/repos/War3CM/War3CM/releases/latest";
        public const string TARGET_EXE_NAME = "WarcraftPlatformManager.exe";

        public static string CurrentVersion
        {
            get
            {
                try
                {
                    var asm = typeof(AppUpdater).Assembly;
                    var ver = asm.GetName().Version;
                    if (ver != null && (ver.Major > 0 || ver.Minor > 0 || ver.Build > 0))
                    {
                        string verStr = $"{ver.Major}.{ver.Minor}.{ver.Build}";
                        if (Version.TryParse(verStr, out var parsed) && Version.TryParse(CURRENT_VERSION, out var currentConst))
                        {
                            return parsed > currentConst ? verStr : CURRENT_VERSION;
                        }
                        return verStr;
                    }
                }
                catch { }
                return CURRENT_VERSION;
            }
        }

        public static string FormatChangelogForDialog(string? rawBody, int maxLines = 5)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return "";

            var lines = rawBody.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var bullets = new System.Collections.Generic.List<string>();

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Skip headers, horizontal rules, markdown tables, badges, keywords footer
                if (line.StartsWith("#") || line.StartsWith("---") || line.StartsWith("===") || 
                    line.StartsWith("|") || line.StartsWith(">") || line.StartsWith("![") || 
                    line.StartsWith("[!") || line.StartsWith("*Keywords", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Check for bullet items
                if (line.StartsWith("- ") || line.StartsWith("* ") || line.StartsWith("• ") || line.StartsWith("+ "))
                {
                    string item = line.Substring(2).Trim();
                    item = item.Replace("**", "");
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        bullets.Add("• " + item);
                    }
                }
                else if (bullets.Count == 0 && !line.Contains('|') && !line.StartsWith("<") && !line.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string item = line.Replace("**", "").Trim();
                    if (!string.IsNullOrWhiteSpace(item) && item.Length < 120)
                    {
                        bullets.Add("• " + item);
                    }
                }

                if (bullets.Count >= maxLines)
                    break;
            }

            if (bullets.Count > 0)
            {
                return string.Join("\n", bullets);
            }

            string clean = rawBody.Replace("\r", " ").Replace("\n", " ").Replace("**", "").Trim();
            if (clean.Length > 200)
            {
                clean = clean.Substring(0, 197) + "...";
            }
            return clean;
        }

        public static bool IsNewerVersion(string? remoteTag, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(remoteTag) || string.IsNullOrWhiteSpace(currentVersion))
                return false;

            string cleanedRemote = remoteTag.Trim().TrimStart('v', 'V');
            string cleanedCurrent = currentVersion.Trim().TrimStart('v', 'V');

            if (Version.TryParse(cleanedRemote, out var vRemote) && Version.TryParse(cleanedCurrent, out var vCurrent))
            {
                return vRemote > vCurrent;
            }

            return false;
        }

        public static AppUpdateInfo? ParseReleaseJson(string json, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("tag_name", out var tagElem))
                    return null;

                string? tagName = tagElem.GetString();
                if (string.IsNullOrEmpty(tagName) || !IsNewerVersion(tagName, currentVersion))
                    return null;

                string version = tagName.Trim().TrimStart('v', 'V');
                string title = root.TryGetProperty("name", out var nameElem) ? (nameElem.GetString() ?? "") : "";
                string body = root.TryGetProperty("body", out var bodyElem) ? (bodyElem.GetString() ?? "") : "";

                if (!root.TryGetProperty("assets", out var assetsElem) || assetsElem.ValueKind != JsonValueKind.Array)
                    return null;

                string? downloadUrl = null;
                long fileSize = 0;

                foreach (var asset in assetsElem.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out var assetNameElem))
                    {
                        string? assetName = assetNameElem.GetString();
                        if (string.Equals(assetName, TARGET_EXE_NAME, StringComparison.OrdinalIgnoreCase))
                        {
                            if (asset.TryGetProperty("browser_download_url", out var urlElem))
                            {
                                downloadUrl = urlElem.GetString();
                            }
                            if (asset.TryGetProperty("size", out var sizeElem))
                            {
                                fileSize = sizeElem.GetInt64();
                            }
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl))
                    return null;

                return new AppUpdateInfo
                {
                    Version = version,
                    Title = string.IsNullOrWhiteSpace(title) ? $"Warcraft Platform Manager v{version}" : title,
                    Changelog = body,
                    DownloadUrl = downloadUrl,
                    FileSize = fileSize
                };
            }
            catch
            {
                return null;
            }
        }

        public static async Task<AppUpdateInfo?> CheckForUpdateAsync(string? currentVersion = null, CancellationToken ct = default)
        {
            string versionToCheck = string.IsNullOrWhiteSpace(currentVersion) ? CurrentVersion : currentVersion;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "War3CM-Launcher");

                var response = await client.GetAsync(RELEASES_API_URL, cts.Token);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync(cts.Token);
                return ParseReleaseJson(json, versionToCheck);
            }
            catch
            {
                return null;
            }
        }

        public static bool ApplyUpdateFiles(string exePath, string newPath, string bakPath)
        {
            try
            {
                if (File.Exists(bakPath))
                {
                    File.Delete(bakPath);
                }

                File.Move(exePath, bakPath);
                File.Move(newPath, exePath);
                return true;
            }
            catch
            {
                // If moving newPath failed after exe was moved to bak, try restoring
                if (!File.Exists(exePath) && File.Exists(bakPath))
                {
                    try { File.Move(bakPath, exePath); } catch { }
                }
                return false;
            }
        }

        public static void CleanupOldBackup(string exePath)
        {
            try
            {
                string bak1 = Path.ChangeExtension(exePath, ".bak");
                string bak2 = exePath + ".bak";
                if (File.Exists(bak1)) File.Delete(bak1);
                if (File.Exists(bak2)) File.Delete(bak2);
            }
            catch
            {
                // Non-critical cleanup
            }
        }

        public static async Task<bool> DownloadAndApplyAsync(
            string downloadUrl,
            string currentExePath,
            IProgress<int>? progress = null,
            CancellationToken ct = default)
        {
            string newPath = Path.ChangeExtension(currentExePath, ".new");
            string bakPath = Path.ChangeExtension(currentExePath, ".bak");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "War3CM-Launcher");
                    using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;

                    using var source = await response.Content.ReadAsStreamAsync(ct);
                    using var destination = new FileStream(newPath, FileMode.Create, FileAccess.Write, FileShare.None);

                    byte[] buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await destination.WriteAsync(buffer, 0, bytesRead, ct);
                        totalRead += bytesRead;

                        if (totalBytes > 0 && progress != null)
                        {
                            int pct = (int)((totalRead * 100) / totalBytes);
                            progress.Report(Math.Min(pct, 100));
                        }
                    }
                }

                // Minimum sanity check: file must be >= 500 KB to be a valid binary
                var fi = new FileInfo(newPath);
                if (fi.Length < 500_000)
                {
                    if (File.Exists(newPath)) File.Delete(newPath);
                    return false;
                }

                return ApplyUpdateFiles(currentExePath, newPath, bakPath);
            }
            catch
            {
                if (File.Exists(newPath))
                {
                    try { File.Delete(newPath); } catch { }
                }
                return false;
            }
        }

        public static void RestartApplication(string exePath)
        {
            try
            {
                string dir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = dir,
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
            catch
            {
                try
                {
                    string dir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c timeout /t 1 /nobreak >nul & start \"\" \"{exePath}\"",
                        WorkingDirectory = dir,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    Environment.Exit(0);
                }
                catch { }
            }
        }
    }
}
