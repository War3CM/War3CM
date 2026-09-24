using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Phanmemwar3.Core
{
    // A map's canonical path identifies it even when two maps share a filename.
    public sealed class MapEntry
    {
        public string Path { get; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public MapEntry(string path) => Path = System.IO.Path.GetFullPath(path);
        public override string ToString() => Name + "  (" + Path + ")";
    }

    public sealed class SaveSlot
    {
        public string Path { get; }
        public SaveSlot(string path) => Path = path;
        public override string ToString() => System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    public sealed class MapSaveManager
    {
        private readonly string _savesRoot;
        public MapSaveManager(string appDir) => _savesRoot = System.IO.Path.Combine(appDir, "Saves");

        public IEnumerable<MapEntry> Scan(string gameDir)
        {
            string maps = System.IO.Path.Combine(gameDir, "Maps");
            if (!Directory.Exists(maps)) return Enumerable.Empty<MapEntry>();
            var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            string launchFolder = System.IO.Path.Combine(maps, "WPM") + System.IO.Path.DirectorySeparatorChar;
            return Directory.EnumerateFiles(maps, "*", options)
                .Where(p => IsMap(p) && !p.StartsWith(launchFolder, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .Select(p => new MapEntry(p)).ToList();
        }

        public static bool IsMap(string path) =>
            string.Equals(System.IO.Path.GetExtension(path), ".w3x", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(System.IO.Path.GetExtension(path), ".w3m", StringComparison.OrdinalIgnoreCase);

        public string MapDirectory(MapEntry map)
        {
            string clean = new string(map.Name.Select(c => System.IO.Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray()).Trim().TrimEnd('.');
            if (clean.Length == 0) clean = "Map";
            if (clean.Length > 60) clean = clean.Substring(0, 60);
            string canonical = map.Path.ToUpperInvariant();
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).Substring(0, 12);
            return System.IO.Path.Combine(_savesRoot, clean + "_" + hash);
        }

        public List<SaveSlot> Slots(MapEntry map)
        {
            string dir = MapDirectory(map);
            if (!Directory.Exists(dir)) return new List<SaveSlot>();
            return Directory.EnumerateFiles(dir, "*.ini", SearchOption.TopDirectoryOnly)
                .OrderBy(p => p, StringComparer.CurrentCultureIgnoreCase).Select(p => new SaveSlot(p)).ToList();
        }

        private static string CheckSlotName(string name)
        {
            name = name.Trim();
            string stem = name.Split('.')[0].ToUpperInvariant();
            if (name.Length == 0 || name.Length > 80 || name == "." || name == ".." ||
                name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 || name.EndsWith('.') ||
                new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                    "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }.Contains(stem))
                throw new ArgumentException("Invalid or reserved slot name.");
            return name;
        }

        private string ValidatedSlotDirectory(MapEntry map, SaveSlot slot)
        {
            string dir = MapDirectory(map);
            if (!File.Exists(slot.Path) ||
                !string.Equals(System.IO.Path.GetDirectoryName(slot.Path), dir, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Save slot does not belong to this map.");
            return dir;
        }

        public SaveSlot Create(MapEntry map, string name, string? source = null)
        {
            name = CheckSlotName(name);
            string dir = MapDirectory(map);
            Directory.CreateDirectory(dir);
            string target = System.IO.Path.Combine(dir, name + ".ini");
            if (Directory.EnumerateFiles(dir, "*.ini").Any(p =>
                System.IO.Path.GetFileName(p).Equals(System.IO.Path.GetFileName(target), StringComparison.OrdinalIgnoreCase)))
                throw new IOException("Slot already exists.");
            if (source == null) { using var _ = new FileStream(target, FileMode.CreateNew); }
            else File.Copy(source, target, false);
            return new SaveSlot(target);
        }

        public SaveSlot Backup(MapEntry map, SaveSlot slot)
        {
            ValidatedSlotDirectory(map, slot);
            string stem = slot.ToString();
            if (stem.Length > 50) stem = stem.Substring(0, 50);
            return Create(map, stem + " - " + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"), slot.Path);
        }

        // Soft delete: the actual save remains under this map's _Trash directory.
        public void Delete(MapEntry map, SaveSlot slot)
        {
            string dir = ValidatedSlotDirectory(map, slot);
            string trash = System.IO.Path.Combine(dir, "_Trash");
            Directory.CreateDirectory(trash);
            string id = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N");
            string target = System.IO.Path.Combine(trash, id + ".ini");
            string metadata = target + ".name";
            using (var writer = new StreamWriter(new FileStream(metadata, FileMode.CreateNew, FileAccess.Write)))
                writer.Write(System.IO.Path.GetFileName(slot.Path));
            try { File.Move(slot.Path, target); }
            catch
            {
                File.Delete(metadata);
                throw;
            }
        }

        public SaveSlot RestoreLatestDeleted(MapEntry map)
        {
            string dir = MapDirectory(map);
            string trash = System.IO.Path.Combine(dir, "_Trash");
            if (!Directory.Exists(trash)) throw new FileNotFoundException("No deleted save slots for this map.");
            foreach (string source in Directory.EnumerateFiles(trash, "*.ini").OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase))
            {
                string metadata = source + ".name";
                if (!File.Exists(metadata)) continue;
                string original = System.IO.Path.GetFileName(File.ReadAllText(metadata).Trim());
                string baseName = CheckSlotName(System.IO.Path.GetFileNameWithoutExtension(original));
                if (!original.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) continue;
                string name = baseName;
                for (int i = 1; Directory.EnumerateFiles(dir, "*.ini").Any(p =>
                    System.IO.Path.GetFileName(p).Equals(name + ".ini", StringComparison.OrdinalIgnoreCase)); i++)
                    name = baseName.Length > 65 ? baseName.Substring(0, 65) + " (" + i + ")" : baseName + " (" + i + ")";
                string target = System.IO.Path.Combine(dir, name + ".ini");
                File.Move(source, target);
                try { File.Delete(metadata); } catch { /* Restored slot is already safe. */ }
                return new SaveSlot(target);
            }
            throw new FileNotFoundException("No recoverable deleted save slots for this map.");
        }

        public bool HasDeleted(MapEntry map)
        {
            string trash = System.IO.Path.Combine(MapDirectory(map), "_Trash");
            try { return Directory.Exists(trash) && Directory.EnumerateFiles(trash, "*.ini").Any(); }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        public SaveSession Prepare(string gameDir, MapEntry map, SaveSlot slot)
        {
            if (!File.Exists(map.Path) || !IsMap(map.Path)) throw new FileNotFoundException("Map unavailable", map.Path);
            ValidatedSlotDirectory(map, slot);
            string root = System.IO.Path.Combine(gameDir, "dz_w3_plugin.ini");
            string archive = System.IO.Path.Combine(_savesRoot, "_PreviousRoot");
            Directory.CreateDirectory(archive);
            // Never delete the previous root save: a dated copy is kept even after the session ends.
            string? previous = null;
            if (File.Exists(root))
            {
                previous = System.IO.Path.Combine(archive,
                    DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N")[..8] + ".ini");
                File.Copy(root, previous, false);
            }
            try { File.Copy(slot.Path, root, true); }
            catch
            {
                if (previous != null) File.Copy(previous, root, true);
                throw;
            }
            return new SaveSession(root, slot.Path, previous);
        }
    }

    public sealed class SaveSession
    {
        private readonly string _root;
        private readonly string _slot;
        private readonly string? _previous;
        internal SaveSession(string root, string slot, string? previous)
        { _root = root; _slot = slot; _previous = previous; }
        public void Rollback()
        {
            if (_previous != null) File.Copy(_previous, _root, true);
            else if (File.Exists(_root)) File.Delete(_root);
        }
        public void Sync()
        {
            if (!File.Exists(_root)) throw new FileNotFoundException("Game save missing", _root);
            // Keep the old slot intact until the new bytes are safely staged.
            string staged = _slot + ".pending";
            File.Copy(_root, staged, true);
            try
            {
                if (File.Exists(_slot))
                {
                    string history = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_slot)!, "_History");
                    Directory.CreateDirectory(history);
                    string snapshot = System.IO.Path.Combine(history, System.IO.Path.GetFileNameWithoutExtension(_slot) + "_" +
                        DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N")[..8] + ".ini");
                    File.Replace(staged, _slot, snapshot, true);
                }
                else File.Move(staged, _slot);
                // Restore the root save that existed before this map was played.
                // If restoration fails, leave the new slot intact and the archived root available.
                Rollback();
            }
            finally { if (File.Exists(staged)) File.Delete(staged); }
        }
    }
}
