using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Phanmemwar3.Core;

namespace Phanmemwar3.Tests
{
    class Program
    {
        static int passed = 0;
        static int failed = 0;

        static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($"  [PASS] {testName}");
                passed++;
            }
            else
            {
                Console.WriteLine($"  [FAIL] {testName}");
                failed++;
            }
        }

        static void Main()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("RUNNING WARCRAFT PLATFORM MANAGER UNIT & SUBTESTS");
            Console.WriteLine("==================================================");

            Test_ConfigManager();
            Test_MapLaunchStager();
            Test_MapSaveManager_Deep();
            Test_PluginManager();
            Test_SaveValueMuter();

            Console.WriteLine("==================================================");
            Console.WriteLine($"SUBTEST RESULTS: {passed} PASSED, {failed} FAILED");
            Console.WriteLine("==================================================");

            if (failed > 0) Environment.Exit(1);
        }

        static void Test_ConfigManager()
        {
            Console.WriteLine("\n[1] Testing ConfigManager & Multilingual Support");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Cfg_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create custom ini files
                string iniContent = "[Settings]\nWar3Path=C:\\TestWar3\nLanguage=VN\nUserName=HeroPlayer\n";
                File.WriteAllText(Path.Combine(tempDir, "settings.ini"), iniContent);

                string langContent = "[VN]\nhello=Xin chao\n[EN]\nhello=Hello World\n[CN]\nhello=Nihao\n";
                File.WriteAllText(Path.Combine(tempDir, "lang.ini"), langContent);

                var cfg = new ConfigManager(tempDir);
                Assert(cfg.GetSetting("War3Path") == "C:\\TestWar3", "Reads War3Path from settings.ini");
                Assert(cfg.GetSetting("UserName") == "HeroPlayer", "Reads UserName from settings.ini");
                Assert(cfg.GetSetting("NonExistent", "DefaultVal") == "DefaultVal", "Returns defaultValue for missing key");

                Assert(cfg.GetText("hello", "VN") == "Xin chao", "Localizes text with [VN]");
                Assert(cfg.GetText("hello", "EN") == "Hello World", "Localizes text with [EN]");
                Assert(cfg.GetText("hello", "CN") == "Nihao", "Localizes text with [CN]");
                Assert(cfg.GetText("missing_key", "VN") == "missing_key", "Returns key if missing in lang");

                // Test saving settings
                cfg.SetSetting("GraphicType", "DirectX");
                cfg.SaveSettings();
                var cfg2 = new ConfigManager(tempDir);
                Assert(cfg2.GetSetting("GraphicType") == "DirectX", "Persists updated settings to disk");

                // Test standalone fallback (when lang.ini is missing on a new machine)
                string emptyDir = Path.Combine(Path.GetTempPath(), "WpmTest_Empty_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(emptyDir);
                try
                {
                    var cfgEmpty = new ConfigManager(emptyDir);
                    bool hasExtracted = File.Exists(Path.Combine(emptyDir, "lang.ini"));
                    bool hasText = cfgEmpty.GetText("appName", "EN") != "appName";
                    Assert(hasExtracted || hasText, "Standalone fallback extracts/loads lang.ini on a clean machine");
                }
                finally
                {
                    if (Directory.Exists(emptyDir)) Directory.Delete(emptyDir, true);
                }
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        static void Test_MapLaunchStager()
        {
            Console.WriteLine("\n[2] Testing MapLaunchStager");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Stager_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string gameDir = Path.Combine(tempDir, "War3");
                string mapSource = Path.Combine(tempDir, "OriginalMap.w3x");
                File.WriteAllText(mapSource, "MAP_PAYLOAD_12345");

                // Test staging
                string staged = MapLaunchStager.Stage(gameDir, mapSource);
                Assert(File.Exists(staged), "Staged map file is created");
                Assert(File.ReadAllText(staged) == "MAP_PAYLOAD_12345", "Staged map content matches source");
                Assert(staged.EndsWith(".w3x"), "Staged map preserves .w3x extension");

                string arg = MapLaunchStager.RelativeArgument(staged);
                Assert(arg.StartsWith("Maps\\WPM") || arg.StartsWith("Maps/WPM"), "Relative argument starts with Maps/WPM");
                Assert(arg.Length < 54, $"Argument length ({arg.Length}) is safely below the 54-char buffer limit");

                // Test empty map throws IOException
                string emptyMap = Path.Combine(tempDir, "Empty.w3x");
                File.WriteAllText(emptyMap, "");
                bool caughtEmpty = false;
                try { MapLaunchStager.Stage(gameDir, emptyMap); } catch (IOException) { caughtEmpty = true; }
                Assert(caughtEmpty, "Empty map file is rejected with IOException");

                // Test non-map extension throws FileNotFoundException
                string notAMap = Path.Combine(tempDir, "Test.txt");
                File.WriteAllText(notAMap, "text");
                bool caughtNotMap = false;
                try { MapLaunchStager.Stage(gameDir, notAMap); } catch (FileNotFoundException) { caughtNotMap = true; }
                Assert(caughtNotMap, "Non-w3x/w3m file rejected");
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        static void Test_MapSaveManager_Deep()
        {
            Console.WriteLine("\n[3] Testing MapSaveManager (Deep Lifecycle)");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Save_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string gameDir = Path.Combine(tempDir, "War3");
                string mapDir = Path.Combine(gameDir, "Maps", "RPG");
                Directory.CreateDirectory(mapDir);
                string mapPath = Path.Combine(mapDir, "DefendTheTemple.w3x");
                File.WriteAllText(mapPath, "DUMMY_MAP");

                var mgr = new MapSaveManager(tempDir);
                var map = new MapEntry(mapPath);

                // Slot creation
                var slot1 = mgr.Create(map, "Profile_Knight");
                Assert(File.Exists(slot1.Path), "Created save slot on disk");
                File.WriteAllText(slot1.Path, "[Knight]\nLevel=50\nGold=9999\n");

                // Duplicate slot creation rejected
                bool dupRejected = false;
                try { mgr.Create(map, "Profile_Knight"); } catch (IOException) { dupRejected = true; }
                Assert(dupRejected, "Duplicate slot creation rejected");

                // Reserved Windows name rejected
                bool resRejected = false;
                try { mgr.Create(map, "CON"); } catch (ArgumentException) { resRejected = true; }
                Assert(resRejected, "Reserved name 'CON' rejected");

                // Backup
                var backup = mgr.Backup(map, slot1);
                Assert(File.Exists(backup.Path), "Backup slot created");
                Assert(File.ReadAllText(backup.Path) == "[Knight]\nLevel=50\nGold=9999\n", "Backup preserves content exactly");

                // Soft-delete and Trash
                mgr.Delete(map, slot1);
                Assert(!File.Exists(slot1.Path), "Slot deleted from active list");
                Assert(mgr.HasDeleted(map), "Map reports has deleted slots in _Trash");

                // Restore
                var restored = mgr.RestoreLatestDeleted(map);
                Assert(File.Exists(restored.Path), "Deleted slot successfully restored");
                Assert(File.ReadAllText(restored.Path) == "[Knight]\nLevel=50\nGold=9999\n", "Restored slot preserves data");
                Assert(!mgr.HasDeleted(map), "Trash is now empty after restore");

                // Prepare and Sync session
                string rootIni = Path.Combine(gameDir, "dz_w3_plugin.ini");
                File.WriteAllText(rootIni, "[PreviousGame]\nOldData=1\n");

                var session = mgr.Prepare(gameDir, map, restored);
                Assert(File.ReadAllText(rootIni).Contains("Level=50"), "Root INI staged with slot data for game launch");

                // Simulate game modified INI
                File.WriteAllText(rootIni, "[Knight]\nLevel=51\nGold=12000\n");
                session.Sync();

                Assert(File.ReadAllText(restored.Path).Contains("Level=51"), "Slot synced with new game progress");
                Assert(File.ReadAllText(rootIni).Contains("OldData=1"), "Previous root INI restored after session ends");

                // Check History
                string historyDir = Path.Combine(mgr.MapDirectory(map), "_History");
                Assert(Directory.Exists(historyDir) && Directory.EnumerateFiles(historyDir).Any(), "_History archive created for pre-sync snapshot");
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        static void Test_PluginManager()
        {
            Console.WriteLine("\n[4] Testing PluginManager");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Plugins_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var pm = new PluginManager(tempDir);
                var profiles = pm.GetAvailableProfiles();
                Assert(profiles.Any(p => p.Id == "CLEAN" && p.IsClean), "Includes 'CLEAN' profile by default");

                // Simulate game directory with installed dz_w3_plugin.dll
                string fakeWar3 = Path.Combine(tempDir, "FakeWar3");
                Directory.CreateDirectory(fakeWar3);
                File.WriteAllText(Path.Combine(fakeWar3, "dz_w3_plugin.dll"), "DLL_BINARY_DATA");

                var profilesWithWar3 = pm.GetAvailableProfiles(fakeWar3);
                Assert(profilesWithWar3.Any(p => p.Id == "INSTALLED" && p.IsInstalled), "Detects existing 'INSTALLED' game plugin");

                // Clean root plugins
                pm.CleanWar3RootPlugins(fakeWar3);
                Assert(!File.Exists(Path.Combine(fakeWar3, "dz_w3_plugin.dll")), "CleanWar3RootPlugins removes root plugin DLLs");
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        static void Test_SaveValueMuter()
        {
            Console.WriteLine("\n[5] Testing SaveValueMuter (KKWE 2606 Disk Signature Matching)");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Muter_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create a mock plugin DLL for KKWE 2606 (length = 1853400)
                byte[] mockDll = new byte[1853400];
                int offset = 0x573b0;
                // Original prologue: 0x55, 0x8B, 0xEC
                mockDll[offset] = 0x55;
                mockDll[offset + 1] = 0x8B;
                mockDll[offset + 2] = 0xEC;

                string dllPath = Path.Combine(tempDir, "dz_w3_plugin.dll");
                File.WriteAllBytes(dllPath, mockDll);

                // Mute
                bool mutedResult = SaveValueMuter.PatchPluginDlls(tempDir, true);
                Assert(mutedResult, "SaveValueMuter reports successful patch for KKWE 2606");
                byte[] muted = File.ReadAllBytes(dllPath);
                Assert(muted[offset] == 0xC2 && muted[offset + 1] == 0x14 && muted[offset + 2] == 0x00, "Patches prologue to ret 14h on mute");

                // Unmute
                bool unmutedResult = SaveValueMuter.PatchPluginDlls(tempDir, false);
                Assert(unmutedResult, "SaveValueMuter reports successful unpatch for KKWE 2606");
                byte[] unmuted = File.ReadAllBytes(dllPath);
                Assert(unmuted[offset] == 0x55 && unmuted[offset + 1] == 0x8B && unmuted[offset + 2] == 0xEC, "Restores original prologue 0x55, 0x8B, 0xEC on unmute");
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }
}
