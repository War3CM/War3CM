using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Phanmemwar3.Core
{
    // Only the launch copy is renamed. The original map path remains the save profile identity.
    public static class MapLaunchStager
    {
        public static string Stage(string gameDir, string sourceMap, Func<string>? nextName = null)
        {
            if (!MapSaveManager.IsMap(sourceMap) || !File.Exists(sourceMap))
                throw new FileNotFoundException("Map unavailable", sourceMap);
            if (new FileInfo(sourceMap).Length == 0) throw new IOException("Map file is empty.");
            string mapFolder = Path.Combine(gameDir, "Maps", "WPM");
            if (Path.GetFullPath(mapFolder).Length > 225)
                throw new PathTooLongException("The Warcraft III folder is too deep for a safe map path.");
            Directory.CreateDirectory(mapFolder);
            string extension = Path.GetExtension(sourceMap).Equals(".w3m", StringComparison.OrdinalIgnoreCase) ? ".w3m" : ".w3x";

            for (int attempt = 0; attempt < 20; attempt++)
            {
                string token = nextName?.Invoke() ?? (attempt == 0 ? ComputeMapToken(sourceMap) : Guid.NewGuid().ToString("N")[..12]);
                if (!Regex.IsMatch(token, "^[A-Za-z0-9]{1,12}$"))
                    throw new ArgumentException("Invalid short map identifier.");
                string candidate = Path.Combine(mapFolder, token + extension);

                if (nextName == null && File.Exists(candidate))
                {
                    try
                    {
                        var srcInfo = new FileInfo(sourceMap);
                        var dstInfo = new FileInfo(candidate);
                        if (dstInfo.Length == srcInfo.Length && Math.Abs((dstInfo.LastWriteTimeUtc - srcInfo.LastWriteTimeUtc).TotalSeconds) < 2)
                        {
                            return candidate;
                        }
                    }
                    catch { }
                }

                bool reserved = false;
                try
                {
                    using var destination = new FileStream(candidate, FileMode.Create, FileAccess.Write, FileShare.None);
                    reserved = true;
                    using var source = new FileStream(sourceMap, FileMode.Open, FileAccess.Read, FileShare.Read);
                    source.CopyTo(destination);
                    destination.Close();
                    try { File.SetLastWriteTimeUtc(candidate, new FileInfo(sourceMap).LastWriteTimeUtc); } catch { }
                    return candidate;
                }
                catch (IOException) when (!reserved && File.Exists(candidate))
                {
                    // Existing map belongs to someone else: retry, never overwrite it.
                }
                catch
                {
                    if (reserved) { try { File.Delete(candidate); } catch { } }
                    throw;
                }
            }
            throw new IOException("Unable to reserve a unique short map name.");
        }

        private static string ComputeMapToken(string sourceMap)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(sourceMap).ToLowerInvariant()));
            return Convert.ToHexString(hash)[..12].ToLowerInvariant();
        }

        public static string RelativeArgument(string stagedMap) =>
            Path.Combine("Maps", "WPM", Path.GetFileName(stagedMap));
    }
}
