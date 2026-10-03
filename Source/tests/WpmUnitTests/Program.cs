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
            Test_MapSaveManager_MapLevelAndRank1();
            Test_PluginManager();
            Test_SaveValueMuter();
            Test_PlayerNameRenamingAndRegistrySync();
            Test_DataCleaner();
            Test_CompactUILayoutAndLocalization();
            Test_GuideLocalization();

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
                string iniContent = "[Settings]\nWar3Path=C:\\TestWar3\nLanguage=EN\nUserName=HeroPlayer\n";
                File.WriteAllText(Path.Combine(tempDir, "settings.ini"), iniContent);

                string langContent = "[EN]\nhello=Hello World\n[RU]\nhello=Привет мир\n[DE]\nhello=Hallo Welt\n[KO]\nhello=안녕하세요\n[ES]\nhello=Hola Mundo\n[UK]\nhello=Привіт світ\n[FR]\nhello=Bonjour le monde\n[PL]\nhello=Witaj świecie\n[PT]\nhello=Olá Mundo\n[CN]\nhello=Nihao\n";
                File.WriteAllText(Path.Combine(tempDir, "lang.ini"), langContent);

                var cfg = new ConfigManager(tempDir);
                Assert(cfg.GetSetting("War3Path") == "C:\\TestWar3", "Reads War3Path from settings.ini");
                Assert(cfg.GetSetting("UserName") == "HeroPlayer", "Reads UserName from settings.ini");
                Assert(cfg.GetSetting("NonExistent", "DefaultVal") == "DefaultVal", "Returns defaultValue for missing key");

                Assert(cfg.GetText("hello", "EN") == "Hello World", "Localizes text with [EN]");
                Assert(cfg.GetText("hello", "RU") == "Привет мир", "Localizes text with [RU]");
                Assert(cfg.GetText("hello", "DE") == "Hallo Welt", "Localizes text with [DE]");
                Assert(cfg.GetText("hello", "KO") == "안녕하세요", "Localizes text with [KO]");
                Assert(cfg.GetText("hello", "CN") == "Nihao", "Localizes text with [CN]");
                Assert(cfg.GetText("missing_key", "EN") == "missing_key", "Returns key if missing in lang");
                Assert(cfg.GetText("hello", "NONEXISTENT") == "Hello World", "Falls back to EN when lang is missing");

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

                // Test caching / reuse of identical staged map
                string restaged = MapLaunchStager.Stage(gameDir, mapSource);
                Assert(restaged == staged, "Re-staging the same map reuses the existing cached staged file");

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

                // Test external save import (Load Save feature)
                string externalVipSave = Path.Combine(tempDir, "VIP_Shared_Save.ini");
                File.WriteAllText(externalVipSave, "[VIP]\nSupreme=1\n[DzAPI]\nDzAPI_Map_GetMapLevel=99\nDzAPI_Map_GetMapLevelRank=1\nSSV-0-TOKEN=VIP123\n");
                var importedSlot = mgr.Create(map, "Slot_VIP_Imported", externalVipSave);
                Assert(File.Exists(importedSlot.Path), "Imported external VIP save slot created on disk");
                Assert(File.ReadAllText(importedSlot.Path).Contains("Supreme=1"), "Imported slot preserves custom sections");
                Assert(MapSaveManager.ReadMapLevel(importedSlot.Path) == 99, "Imported slot reads Map Level 99");
                Assert(MapSaveManager.ReadMapLevelRank(importedSlot.Path) == true, "Imported slot reads Rank 1 as true");

                // Check History
                string historyDir = Path.Combine(mgr.MapDirectory(map), "_History");
                Assert(Directory.Exists(historyDir) && Directory.EnumerateFiles(historyDir).Any(), "_History archive created for pre-sync snapshot");
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        static void Test_MapSaveManager_MapLevelAndRank1()
        {
            Console.WriteLine("\n[3b] Testing MapSaveManager (Map Level & Rank 1 Logic)");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Level_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string emptyIni = Path.Combine(tempDir, "EmptySlot.ini");

                // 1. Initial read on non-existent file
                Assert(MapSaveManager.ReadMapLevel(emptyIni, 100) == 100, "ReadMapLevel returns default level 100 on missing file");
                Assert(MapSaveManager.ReadMapLevel(emptyIni, 50) == 50, "ReadMapLevel returns custom default level on missing file");
                Assert(MapSaveManager.ReadMapLevelRank(emptyIni) == false, "ReadMapLevelRank returns false on missing file");

                // 2. Write to empty/new file -> auto creates [DzAPI] and syncs DzAPI + KKAPI keys
                bool writeOk = MapSaveManager.WriteMapLevel(emptyIni, 88);
                Assert(writeOk, "WriteMapLevel returns true on new file creation");
                Assert(File.Exists(emptyIni), "WriteMapLevel creates new INI file on disk");
                string emptyContent = File.ReadAllText(emptyIni);
                Assert(emptyContent.Contains("[DzAPI]"), "Created INI contains [DzAPI] section header");
                Assert(emptyContent.Contains("DzAPI_Map_GetMapLevel=88"), "Contains DzAPI_Map_GetMapLevel=88");
                Assert(emptyContent.Contains("MLS-MsGetPlayerMapLevel-0=88"), "Contains MLS-MsGetPlayerMapLevel-0=88");
                Assert(emptyContent.Contains("MLS-MsGetPlayerMapLevel-1=88"), "Contains MLS-MsGetPlayerMapLevel-1=88");
                Assert(emptyContent.Contains("MLS-MsGetPlayerMapLevel-2=88"), "Contains MLS-MsGetPlayerMapLevel-2=88");
                Assert(emptyContent.Contains("MLS-MsGetPlayerMapLevel-3=88"), "Contains MLS-MsGetPlayerMapLevel-3=88");
                Assert(MapSaveManager.ReadMapLevel(emptyIni) == 88, "ReadMapLevel reads back 88 correctly");

                // 3. Data preservation: Overwrite existing file with other keys and comments
                string populatedIni = Path.Combine(tempDir, "ExistingData.ini");
                string originalData = "[Profile]\n" +
                                     "PlayerName=Arthas\n" +
                                     "Gold=99999\n" +
                                     "\n" +
                                     "[DzAPI]\n" +
                                     "# Custom game timestamp\n" +
                                     "SSV-0-DAYTIME=12345\n" +
                                     "SSV-0-TOKEN=abcdef123456\n" +
                                     "CUSTOM_HERO=Paladin\n";
                File.WriteAllText(populatedIni, originalData);

                bool updateOk = MapSaveManager.WriteMapLevel(populatedIni, 75);
                Assert(updateOk, "WriteMapLevel succeeds on existing populated file");
                string updatedContent = File.ReadAllText(populatedIni);
                Assert(updatedContent.Contains("PlayerName=Arthas"), "Preserves unrelated section [Profile] keys");
                Assert(updatedContent.Contains("Gold=99999"), "Preserves Gold=99999");
                Assert(updatedContent.Contains("SSV-0-DAYTIME=12345"), "Preserves SSV-0-DAYTIME=12345 in [DzAPI]");
                Assert(updatedContent.Contains("SSV-0-TOKEN=abcdef123456"), "Preserves SSV-0-TOKEN in [DzAPI]");
                Assert(updatedContent.Contains("CUSTOM_HERO=Paladin"), "Preserves CUSTOM_HERO in [DzAPI]");
                Assert(updatedContent.Contains("DzAPI_Map_GetMapLevel=75"), "Appends/updates DzAPI_Map_GetMapLevel=75");
                Assert(updatedContent.Contains("MLS-MsGetPlayerMapLevel-0=75"), "Appends/updates KKAPI key 0");
                Assert(updatedContent.Contains("MLS-MsGetPlayerMapLevel-3=75"), "Appends/updates KKAPI key 3");
                Assert(MapSaveManager.ReadMapLevel(populatedIni) == 75, "ReadMapLevel returns 75 from populated file");

                // 4. Rank 1 Toggle (1 vs 0)
                Assert(MapSaveManager.ReadMapLevelRank(populatedIni) == false, "Rank 1 is false when key absent");
                bool rankOn = MapSaveManager.WriteMapLevelRank(populatedIni, true);
                Assert(rankOn, "WriteMapLevelRank(true) succeeds");
                Assert(MapSaveManager.ReadMapLevelRank(populatedIni) == true, "ReadMapLevelRank returns true after toggle on");
                string rankOnContent = File.ReadAllText(populatedIni);
                Assert(rankOnContent.Contains("DzAPI_Map_GetMapLevelRank=1"), "File contains DzAPI_Map_GetMapLevelRank=1");
                Assert(rankOnContent.Contains("SSV-0-DAYTIME=12345"), "SSV-0-DAYTIME preserved after Rank 1 toggle on");
                Assert(rankOnContent.Contains("DzAPI_Map_GetMapLevel=75"), "Map Level 75 preserved after Rank 1 toggle on");

                bool rankOff = MapSaveManager.WriteMapLevelRank(populatedIni, false);
                Assert(rankOff, "WriteMapLevelRank(false) succeeds");
                Assert(MapSaveManager.ReadMapLevelRank(populatedIni) == false, "ReadMapLevelRank returns false after toggle off");
                string rankOffContent = File.ReadAllText(populatedIni);
                Assert(rankOffContent.Contains("DzAPI_Map_GetMapLevelRank=0"), "File contains DzAPI_Map_GetMapLevelRank=0");
                Assert(rankOffContent.Contains("SSV-0-DAYTIME=12345"), "SSV-0-DAYTIME preserved after Rank 1 toggle off");

                // 5. Clamping and validation boundary checks (1-100)
                string clampIni = Path.Combine(tempDir, "ClampTest.ini");
                MapSaveManager.WriteMapLevel(clampIni, 0);
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 1, "Level 0 clamped to 1");

                MapSaveManager.WriteMapLevel(clampIni, -99);
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 1, "Negative level -99 clamped to 1");

                MapSaveManager.WriteMapLevel(clampIni, 999);
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 100, "Level 999 clamped to 100");

                MapSaveManager.WriteMapLevel(clampIni, 100);
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 100, "Level 100 preserved exactly");

                MapSaveManager.WriteMapLevel(clampIni, 1);
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 1, "Level 1 preserved exactly");

                // 6. Manual corrupted value in file is clamped / handled safely on read
                File.WriteAllText(clampIni, "[DzAPI]\nDzAPI_Map_GetMapLevel=350\n");
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 100, "Out-of-range file value 350 clamped to 100 on read");

                File.WriteAllText(clampIni, "[DzAPI]\nDzAPI_Map_GetMapLevel=-10\n");
                Assert(MapSaveManager.ReadMapLevel(clampIni) == 1, "Out-of-range file value -10 clamped to 1 on read");

                File.WriteAllText(clampIni, "[DzAPI]\nDzAPI_Map_GetMapLevel=not_a_number\n");
                Assert(MapSaveManager.ReadMapLevel(clampIni, 100) == 100, "Non-numeric level falls back to default level");
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

        static void Test_PlayerNameRenamingAndRegistrySync()
        {
            Console.WriteLine("\n[6] Testing Player Name Renaming & Registry Sync");
            string? prevName = RegistryHelper.GetPlayerName();

            try
            {
                // Test default player name War3CM
                bool setDefOk = RegistryHelper.SetPlayerName("War3CM");
                Assert(setDefOk, "RegistryHelper sets default player name 'War3CM' successfully");
                Assert(RegistryHelper.GetPlayerName() == "War3CM", "RegistryHelper reads back 'War3CM' correctly");

                // Test safe length boundary strictly limited to 10 chars
                RegistryHelper.SetPlayerName("SuperLongPlayerNameExceeding10Chars");
                string? clamped = RegistryHelper.GetPlayerName();
                Assert(clamped != null && clamped.Length <= 10, "RegistryHelper limits player name length safely to <= 10 chars");
                Assert(clamped == "SuperLongP", "RegistryHelper clamps to exact first 10 characters");

                // Test name with spaces within 10 chars
                RegistryHelper.SetPlayerName("W3 RPG Map");
                Assert(RegistryHelper.GetPlayerName() == "W3 RPG Map", "RegistryHelper preserves player name with spaces within 10 chars");

                // Test memory patcher bounds check
                bool invalidPidCheck = War3Launcher.PatchPlayerNameInMemory(-1, "SafeTest");
                Assert(!invalidPidCheck, "War3Launcher.PatchPlayerNameInMemory safely rejects non-positive PID");
            }
            finally
            {
                RegistryHelper.SetPlayerName(string.IsNullOrEmpty(prevName) ? "War3CM" : prevName);
            }
        }

        static void Test_DataCleaner()
        {
            Console.WriteLine("\n[6b] Testing DataCleaner (Scan & Safe Execution)");
            string tempDir = Path.Combine(Path.GetTempPath(), "WpmTest_Cleaner_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string appDir = Path.Combine(tempDir, "App");
                string war3Dir = Path.Combine(tempDir, "War3");
                Directory.CreateDirectory(appDir);
                Directory.CreateDirectory(war3Dir);

                // 1. Map Cache
                string wpmDir = Path.Combine(war3Dir, "Maps", "WPM");
                Directory.CreateDirectory(wpmDir);
                string stagedMap = Path.Combine(wpmDir, "staged_001.w3x");
                File.WriteAllText(stagedMap, "STAGED_MAP_DATA");

                // 2. Saves Trash, History, PreviousRoot, and active slot
                string savesDir = Path.Combine(appDir, "Saves", "MyMap_abc123");
                string trashDir = Path.Combine(savesDir, "_Trash");
                string histDir = Path.Combine(savesDir, "_History");
                string prevRootDir = Path.Combine(appDir, "Saves", "_PreviousRoot");
                Directory.CreateDirectory(trashDir);
                Directory.CreateDirectory(histDir);
                Directory.CreateDirectory(prevRootDir);

                string trashFile = Path.Combine(trashDir, "deleted_slot.ini");
                File.WriteAllText(trashFile, "[Trash]");
                string histFile = Path.Combine(histDir, "history_snap.ini");
                File.WriteAllText(histFile, "[History]");
                string prevRootFile = Path.Combine(prevRootDir, "prev.ini");
                File.WriteAllText(prevRootFile, "[PrevRoot]");

                // Active slot (must NEVER be deleted)
                string activeSlot = Path.Combine(savesDir, "Knight_Active.ini");
                File.WriteAllText(activeSlot, "[ActiveSave]\nLevel=99");

                // Old backup slot (matches timestamp pattern)
                string backupSlot = Path.Combine(savesDir, "Knight - 20261002-120000-123.ini");
                File.WriteAllText(backupSlot, "[BackupSave]\nLevel=50");

                // 3. Game Temp & Logs
                string gameIni = Path.Combine(war3Dir, "dz_w3_plugin.ini");
                File.WriteAllText(gameIni, "[SessionTemp]");
                string errorsDir = Path.Combine(war3Dir, "Errors");
                Directory.CreateDirectory(errorsDir);
                File.WriteAllText(Path.Combine(errorsDir, "crash.txt"), "Crash log");

                // 4. Plugin Backups
                string pluginBackupsDir = Path.Combine(appDir, "Profiles", "_PluginBackups");
                Directory.CreateDirectory(pluginBackupsDir);
                File.WriteAllText(Path.Combine(pluginBackupsDir, "old_plugin.dll"), "DLL_BYTES");

                // Perform Scan
                var categories = DataCleaner.Scan(appDir, war3Dir);
                Assert(categories.Count == 5, "DataCleaner scans 5 categories");

                var catMap = categories.First(c => c.Category == CleanCategory.MapCache);
                Assert(catMap.FileCount == 1, "MapCache detects 1 staged map");

                var catTrash = categories.First(c => c.Category == CleanCategory.SaveTrashAndHistory);
                Assert(catTrash.FileCount == 3, "SaveTrashAndHistory detects 3 files");

                var catGame = categories.First(c => c.Category == CleanCategory.GameLogsAndTemp);
                Assert(catGame.FileCount == 2, "GameLogsAndTemp detects 2 files (dz_w3_plugin.ini and crash log)");

                var catPlugin = categories.First(c => c.Category == CleanCategory.PluginBackups);
                Assert(catPlugin.FileCount == 1, "PluginBackups detects 1 backup file");

                var catBackupSlots = categories.First(c => c.Category == CleanCategory.OldBackupSlots);
                Assert(catBackupSlots.FileCount == 1, "OldBackupSlots detects 1 timestamped backup slot");

                // Perform Clean
                var report = DataCleaner.ExecuteClean(categories);
                Assert(report.DeletedFiles == 8, "ExecuteClean deleted all 8 detected temp files");
                Assert(report.FreedBytes > 0, "ExecuteClean reported non-zero freed bytes");
                Assert(report.Errors.Count == 0, "ExecuteClean finished with 0 errors");

                // Safety check: active slot must remain intact!
                Assert(File.Exists(activeSlot), "Active save slot 'Knight_Active.ini' remains intact and was NOT deleted");
                Assert(File.ReadAllText(activeSlot).Contains("Level=99"), "Active save content is 100% preserved");

                // Staged map, trash, and logs must be deleted
                Assert(!File.Exists(stagedMap), "Staged map in Maps/WPM was deleted");
                Assert(!File.Exists(trashFile), "Trash save was deleted");
                Assert(!File.Exists(backupSlot), "Old timestamped backup slot was deleted");
                Assert(!File.Exists(gameIni), "dz_w3_plugin.ini was deleted");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        static void Test_CompactUILayoutAndLocalization()
        {
            Console.WriteLine("\n[7] Testing Compact UI & Localization Integrity");

            // 1. Check pathHintShort in all 10 supported languages
            var cfg = new ConfigManager(AppDomain.CurrentDomain.BaseDirectory);
            string[] supportedLangs = new[] { "EN", "RU", "DE", "KO", "ES", "UK", "FR", "PL", "PT", "CN" };
            foreach (var lang in supportedLangs)
            {
                string hint = cfg.GetText("pathHintShort", lang);
                Assert(!string.IsNullOrEmpty(hint) && hint != "pathHintShort", $"[{lang}] contains pathHintShort");
            }

            // 2. Check UI keys existence across all 10 languages
            string[] keys = new[]
            {
                "appName", "browse", "selectWar3Folder", "pathHintShort", "mapSaveTitle",
                "selectMap", "newSlot", "backupSlot", "deleteSlot", "restoreSlot",
                "loadSave", "tipLoadSave", "loadSaveTitle", "loadSlotPrompt", "loadSlotTitle", "loadSlotDone", "selectMapFirst",
                "pluginOptions", "selectPlugin", "btnConfig", "scanPlugins", "gameOptions",
                "openGL", "fullScreen", "borderless", "windowed", "instance", "btnSettings",
                "btnCheckUpdate", "btnGuide", "btnInGameOptions", "btnDonate", "muteShort", "launchHint", "btnCloseGame", "btnEnterWar3", "btnRunGame", "statusLabel",
                "ready", "serverUnknown", "serverVersion", "openFolderHint", "settingsTitle",
                "userName", "languageLabel", "btnCancel", "btnSaveSettings", "restoreRegistry",
                "guideHint", "guideTitle", "btnOpenDrive", "btnCopyLink", "linkCopied", "btnClose",
                "tipDiscord", "tipYouTube", "tipEnterWar3", "tipRunGame",
                "setMapLevel", "tipSetLevel", "btnRank1", "tipRank1", "invalidLevel", "setLevelSuccess", "rank1Enabled", "rank1Disabled",
                "cleanData", "tipCleanData", "cleanTitle", "cleanSubtitle",
                "cleanCatMapCache", "cleanCatMapCacheDesc", "cleanCatTrashHistory", "cleanCatTrashHistoryDesc",
                "cleanCatGameTemp", "cleanCatGameTempDesc", "cleanCatPluginBackups", "cleanCatPluginBackupsDesc",
                "cleanCatBackupSlots", "cleanCatBackupSlotsDesc",
                "cleanSelectAll", "cleanDeselectAll", "btnCleanNow", "cleanConfirm", "cleanSuccess", "cleanNoSelection", "cleanZeroFiles"
            };

            bool allPresent = true;
            foreach (var lang in supportedLangs)
            {
                foreach (var k in keys)
                {
                    if (!cfg.HasLanguageKey(lang, k))
                    {
                        Console.WriteLine($"    Missing key '{k}' in [{lang}]");
                        allPresent = false;
                    }
                }
            }
            Assert(allPresent, $"All {keys.Length} core UI keys are translated directly in all {supportedLangs.Length} languages");
            Assert(!cfg.HasLanguageKey("VN", "appName"), "VN section is completely removed from lang.ini");

            // 3. Test Form Instantiation & Compact Dimensions in STA thread
            Exception? staEx = null;
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    using var mainForm = new Phanmemwar3.Forms.MainForm();
                    Assert(mainForm.ClientSize.Width == 820 && mainForm.ClientSize.Height == 648, "MainForm fixed compact ClientSize is 820x648");
                    Assert(mainForm.MinimumSize.Width == 820 && mainForm.MinimumSize.Height == 648, "MainForm MinimumSize is 820x648");
                    Assert(mainForm.MaximumSize.Width == 820 && mainForm.MaximumSize.Height == 648, "MainForm MaximumSize is 820x648");
                    Assert(mainForm.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None, "MainForm FormBorderStyle is None");

                    // Check btnOpenFolder does not get overwritten by ellipsis
                    System.Windows.Forms.Control? FindBtn(System.Windows.Forms.Control parent, string text)
                    {
                        foreach (System.Windows.Forms.Control c in parent.Controls)
                        {
                            if (c.Text == text) return c;
                            var found = FindBtn(c, text);
                            if (found != null) return found;
                        }
                        return null;
                    }

                    var arrowBtn = FindBtn(mainForm, "↗");
                    Assert(arrowBtn != null, "btnOpenFolder text is '↗'");
                    Assert(arrowBtn?.Tag == null, "btnOpenFolder.Tag is null (protects against '...' ellipsis overwrite)");

                    System.Windows.Forms.Control? FindControl(System.Windows.Forms.Control parent, Func<System.Windows.Forms.Control, bool> predicate)
                    {
                        foreach (System.Windows.Forms.Control c in parent.Controls)
                        {
                            if (predicate(c)) return c;
                            var found = FindControl(c, predicate);
                            if (found != null) return found;
                        }
                        return null;
                    }

                    // Check btnCheckUpdate exists, is styled, and has valid bounds
                    var updateBtn = FindControl(mainForm, c => c.Tag as string == "btnCheckUpdate");
                    Assert(updateBtn != null, "btnCheckUpdate exists in header actions");
                    Assert(!string.IsNullOrEmpty(updateBtn?.Text), "btnCheckUpdate has localized text");
                    Assert(updateBtn?.Height >= 28, "btnCheckUpdate height is >= 28px (not vertically clipped)");

                    // Check lblServerInfo in footer play card has ample height to prevent cut-off
                    var serverLbl = FindControl(mainForm, c => c is System.Windows.Forms.Label l && (l.Text.Contains("KKWE") || l.Text.Contains("Check for updates") || l.Text.Contains("Sẵn sàng")));
                    Assert(serverLbl != null, "lblServerInfo exists in play card");
                    Assert(serverLbl?.Height >= 20, "lblServerInfo height is >= 20px (prevents half-line text cut-off)");

                    // Check lblHeaderSubtitle is visible with ample height
                    var subLbl = FindControl(mainForm, c => c is System.Windows.Forms.Label l && l.Tag as string == "appDescription");
                    Assert(subLbl != null, "lblHeaderSubtitle exists in hero card");
                    Assert(!string.IsNullOrEmpty(subLbl?.Text), "lblHeaderSubtitle has non-empty text");
                    Assert(subLbl?.Height >= 18, "lblHeaderSubtitle height is >= 18px (not hidden or clipped)");

                    // Check 1x instance combobox is removed and graphic dropdown has full width
                    var instanceCbo = FindControl(mainForm, c => c is System.Windows.Forms.ComboBox cb && cb.Items.Count > 0 && cb.Items[0]?.ToString() == "1x");
                    Assert(instanceCbo == null, "cboInstances ('1x') is removed from UI");
                    var graphicCbo = FindControl(mainForm, c => c is System.Windows.Forms.ComboBox cb && cb.Items.Contains("DirectX"));
                    Assert(graphicCbo != null && graphicCbo.Width >= 300, "cboGraphic has full width >= 300px without squishing");

                    // Check cboLanguage contains all 10 languages, excludes VN, and DropDownWidth is >= 140px
                    var langCbo = FindControl(mainForm, c => c is System.Windows.Forms.ComboBox cb && cb.Items.Contains("EN") && cb.Items.Contains("RU")) as System.Windows.Forms.ComboBox;
                    Assert(langCbo != null && langCbo.Width >= 88, "cboLanguage width is >= 88px (fully visible language code)");
                    Assert(langCbo != null && langCbo.DropDownWidth >= 140, "cboLanguage DropDownWidth is >= 140px for readable language names");
                    Assert(langCbo != null && !langCbo.Items.Contains("VN"), "cboLanguage does NOT contain VN");
                    foreach (var l in supportedLangs)
                    {
                        Assert(langCbo != null && langCbo.Items.Contains(l), $"cboLanguage contains [{l}]");
                    }
                    var actionTipField = typeof(Phanmemwar3.Forms.MainForm).GetField("_actionTip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var actionTip = actionTipField?.GetValue(mainForm) as System.Windows.Forms.ToolTip;
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(langCbo!)), "cboLanguage has informative tooltip");

                    // Check btnInGameOptions exists in optionBody and has ample width
                    var inGameBtn = FindControl(mainForm, c => c.Tag as string == "btnInGameOptions");
                    Assert(inGameBtn != null && inGameBtn.Width >= 300, "btnInGameOptions exists in renderer card with width >= 300px");

                    // Check btnDonate exists in optionBody, has ample width, has PayPal link and tooltip
                    var donateBtn = FindControl(mainForm, c => c.Tag as string == "btnDonate") as Phanmemwar3.Forms.ModernButton;
                    Assert(donateBtn != null && donateBtn.Width >= 300, "btnDonate exists in renderer card with width >= 300px");
                    Assert(donateBtn != null && donateBtn.Text.Contains("paypal.me/Mr007007"), "btnDonate displays paypal.me link in text");
                    Assert(donateBtn != null && !string.IsNullOrEmpty(actionTip?.GetToolTip(donateBtn)), "btnDonate has informative tooltip");

                    // Check btnGuide exists in header actions
                    var guideBtn = FindControl(mainForm, c => c.Tag as string == "btnGuide");
                    Assert(guideBtn != null, "btnGuide exists in header actions");
                    Assert(!string.IsNullOrEmpty(guideBtn?.Text), "btnGuide has localized text");
                    Assert(guideBtn?.Height >= 28, "btnGuide height is >= 28px");
                    Assert(guideBtn?.Width >= 140, "btnGuide width is >= 140px (prevents ellipsis on 📖 Hướng dẫn)");

                    // Check btnDiscord and btnYouTube exist in header actions
                    var discordBtn = FindControl(mainForm, c => c.Tag as string == "btnDiscord") as Phanmemwar3.Forms.ModernButton;
                    Assert(discordBtn != null && discordBtn.IconPainter != null, "btnDiscord exists in header with vector logo painter");
                    var ytBtn = FindControl(mainForm, c => c.Tag as string == "btnYouTube") as Phanmemwar3.Forms.ModernButton;
                    Assert(ytBtn != null && ytBtn.IconPainter != null, "btnYouTube exists in header with vector logo painter");

                    // Check AppIcons and Form icons
                    var appIcon = Phanmemwar3.Forms.AppIcons.GetAppIcon();
                    Assert(appIcon != null, "AppIcons.GetAppIcon() loads embedded/disk application icon");
                    Assert(mainForm.Icon != null, "MainForm has valid non-null window/taskbar Icon");

                    // Check duplicate mute checkbox is removed from MainForm
                    var duplicateMuteChk = FindControl(mainForm, c => c is System.Windows.Forms.CheckBox && (c.Tag as string == "muteShort" || c.Tag as string == "muteSaveValue"));
                    Assert(duplicateMuteChk == null, "Duplicate mute checkbox is removed from MainForm play card");

                    // Check launchHint label exists in MainForm
                    var launchHintLbl = FindControl(mainForm, c => c.Tag as string == "launchHint");
                    Assert(launchHintLbl != null && !string.IsNullOrEmpty(launchHintLbl.Text), "lblLaunchHint exists in play card with localized text");

                    // Check btnEnterWar3 and btnRunGame in play card
                    var enterWar3Btn = FindControl(mainForm, c => c.Tag as string == "btnEnterWar3") as Phanmemwar3.Forms.ModernButton;
                    Assert(enterWar3Btn != null, "btnEnterWar3 exists in play card");
                    Assert(enterWar3Btn != null && enterWar3Btn.Text == "Play War3", "btnEnterWar3 displays exact button text 'Play War3'");

                    var runGameBtn = FindControl(mainForm, c => c.Tag as string == "btnRunGame") as Phanmemwar3.Forms.ModernButton;
                    Assert(runGameBtn != null, "btnRunGame exists in play card");
                    Assert(runGameBtn != null && runGameBtn.Text == "Play Map", "btnRunGame displays exact button text 'Play Map'");

                    // Verify localized text in all 10 languages
                    foreach (var lang in supportedLangs)
                    {
                        Assert(cfg.GetText("btnEnterWar3", lang) == "Play War3", $"btnEnterWar3 is 'Play War3' in [{lang}]");
                        Assert(cfg.GetText("btnRunGame", lang) == "Play Map", $"btnRunGame is 'Play Map' in [{lang}]");
                        Assert(!string.IsNullOrEmpty(cfg.GetText("tipEnterWar3", lang)), $"tipEnterWar3 tooltip is defined in [{lang}]");
                        Assert(!string.IsNullOrEmpty(cfg.GetText("tipRunGame", lang)), $"tipRunGame tooltip is defined in [{lang}]");
                    }

                    // Check Map Level and Rank 1 row 3 controls
                    var setLevelBtn = FindControl(mainForm, c => c.Tag as string == "setMapLevel") as Phanmemwar3.Forms.ModernButton;
                    Assert(setLevelBtn != null, "btnSetLevel exists in slot actions");
                    Assert(!string.IsNullOrEmpty(setLevelBtn?.Text), "btnSetLevel has localized text");

                    var rank1Btn = FindControl(mainForm, c => c.Tag as string == "btnRank1") as Phanmemwar3.Forms.ModernButton;
                    Assert(rank1Btn != null, "btnRank1 exists in slot actions");
                    Assert(rank1Btn?.Text.Contains("Rank 1") == true, "btnRank1 displays Rank 1 text");

                    var mapLevelTxt = FindControl(mainForm, c => c is Phanmemwar3.Forms.ModernTextBox mt && mt.InnerTextBox.MaxLength == 3) as Phanmemwar3.Forms.ModernTextBox;
                    Assert(mapLevelTxt != null, "txtMapLevel exists with MaxLength=3");
                    Assert(mapLevelTxt?.InnerTextBox.TextAlign == System.Windows.Forms.HorizontalAlignment.Center, "txtMapLevel is center-aligned");

                    var slotGrid = setLevelBtn?.Parent?.Parent as System.Windows.Forms.TableLayoutPanel;
                    Assert(slotGrid != null && slotGrid.RowCount == 3, "slotActions grid has RowCount == 3");

                    var capsule = FindControl(mainForm, c => c is Phanmemwar3.Forms.ModernInputCapsule) as Phanmemwar3.Forms.ModernInputCapsule;
                    Assert(capsule != null, "capsuleLevel exists as ModernInputCapsule");
                    Assert(capsule?.TextBox == mapLevelTxt, "capsuleLevel hosts txtMapLevel");
                    Assert(capsule?.ActionButton == setLevelBtn, "capsuleLevel hosts btnSetLevel");

                    var playGrid = runGameBtn?.Parent as System.Windows.Forms.TableLayoutPanel;
                    Assert(playGrid != null && playGrid.ColumnStyles.Count >= 4 && playGrid.ColumnStyles[3].Width >= 165, "playRow allocates at least 165px for btnRunGame");

                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(setLevelBtn!)), "btnSetLevel has informative tooltip");
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(rank1Btn!)), "btnRank1 has informative tooltip");
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(mapLevelTxt!)), "txtMapLevel has informative tooltip");
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(capsule!)), "capsuleLevel has informative tooltip");

                    Assert(!setLevelBtn!.Enabled, "btnSetLevel disabled when no slot is selected");
                    Assert(!rank1Btn!.Enabled, "btnRank1 disabled when no slot is selected");
                    Assert(!mapLevelTxt!.Enabled, "txtMapLevel disabled when no slot is selected");
                    Assert(!capsule!.Enabled, "capsuleLevel disabled when no slot is selected");

                    var loadSaveBtn = FindControl(mainForm, c => c.Tag as string == "loadSave") as Phanmemwar3.Forms.ModernButton;
                    Assert(loadSaveBtn != null, "btnLoadSave exists in slot actions");
                    Assert(!string.IsNullOrEmpty(loadSaveBtn?.Text), "btnLoadSave has localized text");
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(loadSaveBtn!)), "btnLoadSave has informative tooltip");
                    var newSlotBtn = FindControl(mainForm, c => c.Tag as string == "newSlot") as Phanmemwar3.Forms.ModernButton;
                    Assert(loadSaveBtn!.Enabled == newSlotBtn!.Enabled, "btnLoadSave enablement matches btnNewSlot according to map selection");

                    var cleanDataBtn = FindControl(mainForm, c => c.Tag as string == "cleanData") as Phanmemwar3.Forms.ModernButton;
                    Assert(cleanDataBtn != null, "btnCleanData exists in slot actions");
                    Assert(!string.IsNullOrEmpty(cleanDataBtn?.Text), "btnCleanData has localized text");
                    Assert(!string.IsNullOrEmpty(actionTip?.GetToolTip(cleanDataBtn!)), "btnCleanData has informative tooltip");
                    Assert(cleanDataBtn!.Enabled, "btnCleanData is enabled when session is active");

                    // Check slotActions grid layout
                    var slotTable = loadSaveBtn!.Parent as TableLayoutPanel;
                    Assert(slotTable != null, "slotActions grid exists");
                    Assert(slotTable!.GetRow(loadSaveBtn) == 0 && slotTable.GetColumn(loadSaveBtn) == 1, "btnLoadSave is at Row 0, Col 1 (replacing Backup)");
                    Assert(slotTable.GetRow(cleanDataBtn!) == 1 && slotTable.GetColumn(cleanDataBtn!) == 1, "btnCleanData is at Row 1, Col 1 (replacing Load Save)");

                    using var cleanDataForm = new Phanmemwar3.Forms.CleanDataForm(cfg, AppDomain.CurrentDomain.BaseDirectory);
                    Assert(cleanDataForm.Icon != null, "CleanDataForm has valid window Icon");
                    Assert(cleanDataForm.ClientSize.Width == 540 && cleanDataForm.ClientSize.Height == 520, "CleanDataForm fixed compact ClientSize is 540x520");
                    Assert(cleanDataForm.MinimumSize.Width == 540 && cleanDataForm.MinimumSize.Height == 520, "CleanDataForm MinimumSize is 540x520");
                    Assert(cleanDataForm.MaximumSize.Width == 540 && cleanDataForm.MaximumSize.Height == 520, "CleanDataForm MaximumSize is 540x520");

                    using var guideForm = new Phanmemwar3.Forms.GuideForm(cfg);
                    Assert(guideForm.Icon != null, "GuideForm has valid window Icon");
                    Assert(guideForm.ClientSize.Width == 720 && guideForm.ClientSize.Height == 680, "GuideForm fixed compact ClientSize is 720x680");
                    Assert(guideForm.MinimumSize.Width == 720 && guideForm.MinimumSize.Height == 680, "GuideForm MinimumSize is 720x680");
                    Assert(guideForm.MaximumSize.Width == 720 && guideForm.MaximumSize.Height == 680, "GuideForm MaximumSize is 720x680");

                    cfg.SetSetting("Language", "RU");
                    using var guideFormRu = new Phanmemwar3.Forms.GuideForm(cfg);
                    Assert(guideFormRu.Text.Contains("Руководство"), "GuideForm title is translated in [RU]");

                    cfg.SetSetting("Language", "KO");
                    using var guideFormKo = new Phanmemwar3.Forms.GuideForm(cfg);
                    Assert(guideFormKo.Text.Contains("가이드"), "GuideForm title is translated in [KO]");

                    cfg.SetSetting("Language", "EN");

                    using var settingsForm = new Phanmemwar3.Forms.SettingsForm(cfg);
                    var settingsMuteChk = FindControl(settingsForm, c => c.Tag as string == "muteSaveValue");
                    Assert(settingsMuteChk != null, "MuteSaveValue checkbox exists cleanly inside SettingsForm");
                    var userTxt = FindControl(settingsForm, c => c is Phanmemwar3.Forms.ModernTextBox) as Phanmemwar3.Forms.ModernTextBox;
                    Assert(userTxt != null && userTxt.MaxLength == 10, "SettingsForm txtUserName limits input to max 10 characters");
                    Assert(settingsForm.ClientSize.Width == 510 && settingsForm.ClientSize.Height == 480, "SettingsForm fixed compact ClientSize is 510x480");
                    Assert(settingsForm.MinimumSize.Width == 510 && settingsForm.MinimumSize.Height == 480, "SettingsForm MinimumSize is 510x480");
                    Assert(settingsForm.MaximumSize.Width == 510 && settingsForm.MaximumSize.Height == 480, "SettingsForm MaximumSize is 510x480");
                }
                catch (Exception ex)
                {
                    staEx = ex;
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (staEx != null)
            {
                Console.WriteLine($"  [FAIL] Form STA initialization threw: {staEx}");
                failed++;
            }
            else
            {
                Assert(true, "Forms instantiate and validate layout bounds cleanly on STA thread");
            }
        }

        static void Test_GuideLocalization()
        {
            Console.WriteLine("\n[8] Testing Multi-language Guide (GuideLocalization)");
            string[] supportedLangs = { "EN", "RU", "DE", "KO", "ES", "UK", "FR", "PL", "PT", "CN" };

            foreach (var lang in supportedLangs)
            {
                var content = Phanmemwar3.Forms.GuideLocalization.GetContent(lang);
                Assert(content != null, $"GuideContent is not null for [{lang}]");
                Assert(!string.IsNullOrWhiteSpace(content!.BannerTitle), $"[{lang}] BannerTitle is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.BannerSubtitle), $"[{lang}] BannerSubtitle is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step1Title), $"[{lang}] Step1Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step1Desc), $"[{lang}] Step1Desc is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.BtnCardDrive), $"[{lang}] BtnCardDrive is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.BtnCardCopy), $"[{lang}] BtnCardCopy is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.CommTitle), $"[{lang}] CommTitle is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.CommDesc), $"[{lang}] CommDesc is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.BtnDiscord), $"[{lang}] BtnDiscord is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.BtnYouTube), $"[{lang}] BtnYouTube is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.WarnTitle), $"[{lang}] WarnTitle is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.WarnBody), $"[{lang}] WarnBody is non-empty");

                Assert(!string.IsNullOrWhiteSpace(content.Step2.Title), $"[{lang}] Step2.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step2.Body), $"[{lang}] Step2.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step3.Title), $"[{lang}] Step3.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step3.Body), $"[{lang}] Step3.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step4.Title), $"[{lang}] Step4.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step4.Body), $"[{lang}] Step4.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step5.Title), $"[{lang}] Step5.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step5.Body), $"[{lang}] Step5.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step6.Title), $"[{lang}] Step6.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step6.Body), $"[{lang}] Step6.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step7.Title), $"[{lang}] Step7.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step7.Body), $"[{lang}] Step7.Body is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step8.Title), $"[{lang}] Step8.Title is non-empty");
                Assert(!string.IsNullOrWhiteSpace(content.Step8.Body), $"[{lang}] Step8.Body is non-empty");
            }

            // Language-specific keyword checks
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("RU").BannerTitle.Contains("Руководство"), "[RU] contains Russian 'Руководство'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("DE").BannerTitle.Contains("Anleitung"), "[DE] contains German 'Anleitung'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("KO").BannerTitle.Contains("가이드"), "[KO] contains Korean '가이드'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("ES").BannerTitle.Contains("Guía"), "[ES] contains Spanish 'Guía'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("UK").BannerTitle.Contains("Посібник"), "[UK] contains Ukrainian 'Посібник'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("FR").BannerTitle.Contains("Guide"), "[FR] contains French 'Guide'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("PL").BannerTitle.Contains("Instrukcja"), "[PL] contains Polish 'Instrukcja'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("PT").BannerTitle.Contains("Guia"), "[PT] contains Portuguese 'Guia'");
            Assert(Phanmemwar3.Forms.GuideLocalization.GetContent("CN").BannerTitle.Contains("指南"), "[CN] contains Chinese '指南'");

            // Fallback for unknown language returns EN
            var fallback = Phanmemwar3.Forms.GuideLocalization.GetContent("UNKNOWN");
            Assert(fallback.BannerTitle == Phanmemwar3.Forms.GuideLocalization.GetContent("EN").BannerTitle, "Unknown language safely falls back to [EN]");
        }
    }
}
