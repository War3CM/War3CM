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
            Test_CompactUILayoutAndLocalization();

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

        static void Test_CompactUILayoutAndLocalization()
        {
            Console.WriteLine("\n[6] Testing Compact UI & Localization Integrity");

            // 1. Check pathHintShort in all 3 languages
            var cfg = new ConfigManager(AppDomain.CurrentDomain.BaseDirectory);
            string vnHint = cfg.GetText("pathHintShort", "VN");
            string enHint = cfg.GetText("pathHintShort", "EN");
            string cnHint = cfg.GetText("pathHintShort", "CN");

            Assert(!string.IsNullOrEmpty(vnHint) && vnHint != "pathHintShort", "VN contains pathHintShort");
            Assert(!string.IsNullOrEmpty(enHint) && enHint != "pathHintShort", "EN contains pathHintShort");
            Assert(!string.IsNullOrEmpty(cnHint) && cnHint != "pathHintShort", "CN contains pathHintShort");

            // 2. Check UI keys existence across languages
            string[] keys = new[]
            {
                "appName", "browse", "selectWar3Folder", "pathHintShort", "mapSaveTitle",
                "selectMap", "newSlot", "backupSlot", "deleteSlot", "restoreSlot",
                "pluginOptions", "selectPlugin", "btnConfig", "scanPlugins", "gameOptions",
                "openGL", "fullScreen", "borderless", "windowed", "instance", "btnSettings",
                "btnCheckUpdate", "btnGuide", "btnInGameOptions", "muteShort", "launchHint", "btnCloseGame", "btnRunGame", "statusLabel",
                "ready", "serverUnknown", "serverVersion", "openFolderHint", "settingsTitle",
                "userName", "languageLabel", "btnCancel", "btnSaveSettings", "restoreRegistry",
                "guideHint", "guideTitle", "btnOpenDrive", "btnCopyLink", "linkCopied", "btnClose",
                "tipDiscord", "tipYouTube"
            };

            bool allPresent = true;
            foreach (var lang in new[] { "VN", "EN", "CN" })
            {
                foreach (var k in keys)
                {
                    if (cfg.GetText(k, lang) == k)
                    {
                        Console.WriteLine($"    Missing key '{k}' in [{lang}]");
                        allPresent = false;
                    }
                }
            }
            Assert(allPresent, $"All {keys.Length} core UI keys are translated across VN, EN, CN");

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

                    // Check cboLanguage width is >= 85px so text like EN/VN/CN is never clipped to a vertical bar
                    var langCbo = FindControl(mainForm, c => c is System.Windows.Forms.ComboBox cb && cb.Items.Contains("EN") && cb.Items.Contains("VN"));
                    Assert(langCbo != null && langCbo.Width >= 85, "cboLanguage width is >= 85px (fully visible language code)");

                    // Check btnInGameOptions exists in optionBody and has ample width
                    var inGameBtn = FindControl(mainForm, c => c.Tag as string == "btnInGameOptions");
                    Assert(inGameBtn != null && inGameBtn.Width >= 300, "btnInGameOptions exists in renderer card with width >= 300px");

                    // Check btnGuide exists in header actions
                    var guideBtn = FindControl(mainForm, c => c.Tag as string == "btnGuide");
                    Assert(guideBtn != null, "btnGuide exists in header actions");
                    Assert(!string.IsNullOrEmpty(guideBtn?.Text), "btnGuide has localized text");
                    Assert(guideBtn?.Height >= 28, "btnGuide height is >= 28px");

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

                    using var guideForm = new Phanmemwar3.Forms.GuideForm(cfg);
                    Assert(guideForm.Icon != null, "GuideForm has valid window Icon");
                    Assert(guideForm.ClientSize.Width == 720 && guideForm.ClientSize.Height == 680, "GuideForm fixed compact ClientSize is 720x680");
                    Assert(guideForm.MinimumSize.Width == 720 && guideForm.MinimumSize.Height == 680, "GuideForm MinimumSize is 720x680");
                    Assert(guideForm.MaximumSize.Width == 720 && guideForm.MaximumSize.Height == 680, "GuideForm MaximumSize is 720x680");

                    using var settingsForm = new Phanmemwar3.Forms.SettingsForm(cfg);
                    var settingsMuteChk = FindControl(settingsForm, c => c.Tag as string == "muteSaveValue");
                    Assert(settingsMuteChk != null, "MuteSaveValue checkbox exists cleanly inside SettingsForm");
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
    }
}
