using Phanmemwar3.Core;

string root = Path.Combine(Path.GetTempPath(), "WpmSmoke_" + Guid.NewGuid().ToString("N"));
try
{
    string game = Path.Combine(root, "game");
    string a = Path.Combine(game, "Maps", "A", "same.w3x");
    string b = Path.Combine(game, "Maps", "B", "same.w3x");
    string staged = Path.Combine(game, "Maps", "WPM", "temporary.w3x");
    foreach (string file in new[] { a, b, staged })
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, "map-data");
    }
    var manager = new MapSaveManager(root);
    var maps = manager.Scan(game).ToList();
    Require(maps.Count == 2, "scan excludes staged maps");
    var mapA = maps.Single(m => m.Path == Path.GetFullPath(a));
    var mapB = maps.Single(m => m.Path == Path.GetFullPath(b));
    Require(manager.MapDirectory(mapA) != manager.MapDirectory(mapB), "identical filenames keep separate saves");

    string collision = Path.Combine(game, "Maps", "WPM", "AAAAAAAAAAAA.w3x");
    File.WriteAllText(collision, "existing-map-must-survive");
    var names = new Queue<string>(new[] { "AAAAAAAAAAAA", "BBBBBBBBBBBB" });
    string alias = MapLaunchStager.Stage(game, a, () => names.Dequeue());
    Require(Path.GetFileName(alias) == "BBBBBBBBBBBB.w3x", "alias collision retries");
    Require(File.ReadAllText(collision) == "existing-map-must-survive", "existing map never overwritten");
    Require(File.ReadAllText(alias) == "map-data", "launch copy matches original");
    Require(MapLaunchStager.RelativeArgument(alias).Length < 54, "short WC3 loadfile argument");
    File.Delete(alias);

    var slot = manager.Create(mapA, "Hero");
    File.WriteAllText(slot.Path, "level=5");
    var backup = manager.Backup(mapA, slot);
    Require(File.ReadAllText(backup.Path) == "level=5", "backup preserves contents");
    manager.Delete(mapA, slot);
    Require(!File.Exists(slot.Path) && manager.HasDeleted(mapA), "soft delete");
    var replacement = manager.Create(mapA, "Hero");
    var restored = manager.RestoreLatestDeleted(mapA);
    Require(restored.Path != replacement.Path && File.ReadAllText(restored.Path) == "level=5", "restore avoids name collision");
    Require(!manager.HasDeleted(mapA), "trash entry removed after restore");

    string rootIni = Path.Combine(game, "dz_w3_plugin.ini");
    File.WriteAllText(rootIni, "previous-map");
    var session = manager.Prepare(game, mapA, restored);
    Require(File.ReadAllText(rootIni) == "level=5", "selected slot staged in game root");
    File.WriteAllText(rootIni, "level=6");
    session.Sync();
    Require(File.ReadAllText(restored.Path) == "level=6", "game data synchronized");
    Require(File.ReadAllText(rootIni) == "previous-map", "previous root restored");
    Require(Directory.EnumerateFiles(Path.Combine(manager.MapDirectory(mapA), "_History"), "*.ini").Any(), "old slot snapshot kept");

    File.Delete(rootIni);
    var secondSession = manager.Prepare(game, mapB, manager.Create(mapB, "New"));
    File.WriteAllText(rootIni, "new-map-save");
    secondSession.Sync();
    Require(!File.Exists(rootIni), "empty previous root stays empty");
    Console.WriteLine("Map/save smoke checks passed.");
}
finally
{
    if (Directory.Exists(root)) Directory.Delete(root, true);
}

static void Require(bool condition, string description)
{
    if (!condition) throw new Exception("Failed: " + description);
}
