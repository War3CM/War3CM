using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;

namespace Phanmemwar3.Core
{
    public class LaunchOptions
    {
        public string War3Path { get; set; } = "";
        public string GraphicType { get; set; } = "OpenGL"; // OpenGL or DirectX
        public string DisplayMode { get; set; } = "Borderless Windowed"; // Borderless Windowed, Window, Fullscreen
        public int Instances { get; set; } = 1;
        public string UserName { get; set; } = "Player";
        public bool LockMouse { get; set; } = true;
        public bool FixRatio { get; set; } = true;
        public bool WideScreen { get; set; } = true;
        public bool FastLoad { get; set; } = true;
        public bool MuteSaveValue { get; set; } = true;
        public bool IsCleanProfile { get; set; } = false;
        public bool IsInstalledProfile { get; set; } = false;
        public string MapPath { get; set; } = "";
        // Temporary short path used only for the current launch; removed after game exit.
        public string StagedMapPath { get; set; } = "";
    }

    public static class War3Launcher
    {
        public static bool Launch(LaunchOptions options, Action<string>? logger = null, Action<Process>? onStarted = null)
        {
            if (string.IsNullOrWhiteSpace(options.War3Path) || !Directory.Exists(options.War3Path))
            {
                logger?.Invoke("Thư mục War3 không hợp lệ!");
                return false;
            }

            // Always ensure Registry is updated: InstallPath, Allow Local Files & Player Name
            RegistryHelper.SetWar3InstallPath(options.War3Path);
            RegistryHelper.SetAllowLocalFiles(true);
            RegistryHelper.SetHighDpiAware(options.War3Path);
            RegistryHelper.SetPlayerName(options.UserName);

            // Mute / Unmute Save Value on disk directly before launch
            // An installed profile belongs to the player. Apply mute in memory after launch
            // without patching the DLLs they already placed in the game directory.
            if (!options.IsInstalledProfile)
                SaveValueMuter.PatchPluginDlls(options.War3Path, options.MuteSaveValue);

            // Fast Load & Memory Boost (LAA 4GB Patch)
            if (options.FastLoad)
            {
                ApplyLaaPatch(options.War3Path, true);
            }

            string weBin = Path.Combine(options.War3Path, "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C", "bin");
            string ydweConfig = Path.Combine(weBin, "YDWEConfig.exe");

            // Write EverConfig.cfg if YDWEConfig directory exists
            if (Directory.Exists(weBin))
            {
                WriteEverConfig(weBin, options);
            }

            string mapArgument = "";
            if (!string.IsNullOrEmpty(options.MapPath))
            {
                options.StagedMapPath = MapLaunchStager.Stage(options.War3Path, options.MapPath);
                mapArgument = MapLaunchStager.RelativeArgument(options.StagedMapPath);
            }

            for (int i = 0; i < options.Instances; i++)
            {
                if (!options.IsCleanProfile)
                {
                    if (!File.Exists(ydweConfig))
                        throw new FileNotFoundException("YDWEConfig.exe is required for this plugin profile.", ydweConfig);
                    var existingProcesses = Process.GetProcessesByName("war3");
                    var existing = existingProcesses.Select(p => p.Id).ToHashSet();
                    foreach (var process in existingProcesses) process.Dispose();
                    logger?.Invoke($"Đang khởi chạy Giả lập Offline qua YDWE Loader (Cửa sổ {i + 1})...");
                    var psi = new ProcessStartInfo
                    {
                        FileName = ydweConfig,
                        Arguments = "-launchwar3" + (mapArgument.Length > 0 ? " -loadfile \"" + mapArgument + "\"" : ""),
                        WorkingDirectory = weBin,
                        UseShellExecute = true
                    };
                    using var loader = Process.Start(psi);
                    if (loader == null) return false;
                    // The loader is short lived. Track the new game process for save sync.
                    Process? game = null;
                    for (int attempt = 0; attempt < 100 && game == null; attempt++)
                    {
                        foreach (var candidate in Process.GetProcessesByName("war3"))
                        {
                            if (!existing.Contains(candidate.Id)) { game = candidate; break; }
                            candidate.Dispose();
                        }
                        if (game == null) Thread.Sleep(200);
                    }
                    if (game == null) return false;
                    PatchPlayerNameInMemory(game.Id, options.UserName);
                    onStarted?.Invoke(game);
                }
                else
                {
                    // Direct launch war3.exe (with version.dll hook if present)
                    string war3Exe = Path.Combine(options.War3Path, "war3.exe");
                    if (!File.Exists(war3Exe))
                    {
                        logger?.Invoke($"Không tìm thấy file {war3Exe}!");
                        return false;
                    }

                    var sbArgs = new StringBuilder();
                    if (options.GraphicType.Equals("OpenGL", StringComparison.OrdinalIgnoreCase))
                    {
                        sbArgs.Append(" -opengl");
                    }

                    if (options.DisplayMode.Contains("Window", StringComparison.OrdinalIgnoreCase))
                    {
                        sbArgs.Append(" -window");
                    }

                    if (options.FastLoad)
                    {
                        sbArgs.Append(" -fastload");
                    }

                    if (mapArgument.Length > 0)
                    {
                        sbArgs.Append(" -loadfile \"" + mapArgument + "\"");
                    }

                    logger?.Invoke($"Đang khởi chạy War3.exe {sbArgs} (Cửa sổ {i + 1})...");
                    var psi = new ProcessStartInfo
                    {
                        FileName = war3Exe,
                        Arguments = sbArgs.ToString().Trim(),
                        WorkingDirectory = options.War3Path,
                        UseShellExecute = true
                    };
                    var started = Process.Start(psi);
                    if (started == null) return false;
                    PatchPlayerNameInMemory(started.Id, options.UserName);
                    onStarted?.Invoke(started);
                }
            }

            return true;
        }

        public static bool OpenYDWEConfig(string war3Path)
        {
            string weBin = Path.Combine(war3Path, "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C", "bin");
            string ydweConfig = Path.Combine(weBin, "YDWEConfig.exe");
            if (File.Exists(ydweConfig))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ydweConfig,
                    WorkingDirectory = weBin,
                    UseShellExecute = true
                });
                return true;
            }
            return false;
        }

        public static void CloseAllWar3()
        {
            try
            {
                var procs = Process.GetProcessesByName("war3");
                foreach (var p in procs)
                {
                    try { p.Kill(); } catch { }
                }
            }
            catch { }
        }

        private static void WriteEverConfig(string binDir, LaunchOptions options)
        {
            string cfgPath = Path.Combine(binDir, "EverConfig.cfg");
            bool isWindowed = options.DisplayMode.Contains("Window", StringComparison.OrdinalIgnoreCase);
            bool isFullWindowed = options.DisplayMode.Contains("Borderless", StringComparison.OrdinalIgnoreCase) ||
                                  options.DisplayMode.Equals("Full Windowed", StringComparison.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            sb.AppendLine("[FeatureToggle]");
            sb.AppendLine("EnableManualNewId = 0");
            sb.AppendLine("EnableShowInternalAttributeId = 0");
            sb.AppendLine("EnableTriggerCopyEncodingAutoConversion = 1");
            sb.AppendLine("[Font]");
            sb.AppendLine("FontEnable = 0");
            sb.AppendLine("FontName =  ");
            sb.AppendLine("FontSize = 12");
            sb.AppendLine("[HostTest]");
            sb.AppendLine("Option = 0");
            sb.AppendLine("[MapSave]");
            sb.AppendLine("Option = 0");
            sb.AppendLine("[MapTest]");
            sb.AppendLine("EnableHost = 0");
            sb.AppendLine("EnableMapSlk = 0");
            sb.AppendLine("EnableMlScript = 0");
            sb.AppendLine("LaunchDisableSecurityAccess = 0");
            sb.AppendLine($"LaunchFastLoad = {(options.FastLoad ? "1" : "0")}");
            sb.AppendLine($"LaunchFixedRatioWindowed = {(options.FixRatio ? "1" : "0")}");
            sb.AppendLine($"LaunchFullWindowed = {(isFullWindowed ? "1" : "0")}");
            sb.AppendLine($"LaunchLockingMouse = {(options.LockMouse ? "1" : "0")}");
            sb.AppendLine($"LaunchRenderingEngine = {options.GraphicType}");
            sb.AppendLine($"LaunchWideScreenSupport = {(options.WideScreen ? "1" : "0")}");
            sb.AppendLine($"LaunchWindowed = {(isWindowed ? "1" : "0")}");
            sb.AppendLine("MlScriptFolder =  ");
            sb.AppendLine($"UserName = {options.UserName}");
            sb.AppendLine("VirtualMpq =  ");
            sb.AppendLine("VirtualPath = ");
            sb.AppendLine("[PJass]");
            sb.AppendLine("Option = 0");
            sb.AppendLine("[ScriptCompiler]");
            sb.AppendLine("EnableCJass = 0");
            sb.AppendLine("EnableJassHelper = 1");
            sb.AppendLine("EnableJassHelperDebug = 0");
            sb.AppendLine("EnableJassHelperOptimization = 0");
            sb.AppendLine("EnableJassHelperScriptOnly = 0");
            sb.AppendLine("[ScriptInjection]");
            sb.AppendLine("Option = 0");
            sb.AppendLine("[ThirdPartyPlugin]");
            sb.AppendLine("EnableDarkMode = 0");
            sb.AppendLine("EnableDotNetSupport = 0");
            sb.AppendLine("EnableMCPPlugin = 1");
            sb.AppendLine("EnableMapHelper = 0");
            sb.AppendLine("EnableTesh = 1");
            sb.AppendLine("EnableYDTrigger = 1");
            sb.AppendLine("MCPPort = 19816");
            sb.AppendLine("[War3Patch]");
            sb.AppendLine("Option = 0");

            try
            {
                File.WriteAllText(cfgPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        public static void ApplyLaaPatch(string war3Path, bool enable = true)
        {
            try
            {
                string[] targets = { "war3.exe", "Game.dll" };
                foreach (var t in targets)
                {
                    string p = Path.Combine(war3Path, t);
                    if (!File.Exists(p)) continue;

                    using var fs = new FileStream(p, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    byte[] header = new byte[0x200];
                    fs.Read(header, 0, header.Length);

                    int peOffset = BitConverter.ToInt32(header, 0x3C);
                    if (peOffset <= 0 || peOffset > fs.Length - 0x100) continue;

                    fs.Seek(peOffset + 4 + 18, SeekOrigin.Begin);
                    byte[] charsBytes = new byte[2];
                    fs.Read(charsBytes, 0, 2);
                    ushort chars = BitConverter.ToUInt16(charsBytes, 0);

                    ushort newChars = chars;
                    if (enable) newChars |= 0x0020; // IMAGE_FILE_LARGE_ADDRESS_AWARE
                    else newChars = (ushort)(newChars & ~0x0020);

                    if (newChars != chars)
                    {
                        fs.Seek(peOffset + 4 + 18, SeekOrigin.Begin);
                        fs.Write(BitConverter.GetBytes(newChars), 0, 2);
                    }
                }
            }
            catch { }
        }

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static bool PatchPlayerNameInMemory(int pid, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) || pid <= 0) return false;
            string safeName = newName.Trim();
            if (safeName.Length > 11) safeName = safeName.Substring(0, 11);

            byte[] nameBytes = new byte[12];
            Encoding.ASCII.GetBytes(safeName, 0, safeName.Length, nameBytes, 0);

            try
            {
                using var proc = Process.GetProcessById(pid);
                IntPtr gameBase = IntPtr.Zero;
                try
                {
                    foreach (ProcessModule mod in proc.Modules)
                    {
                        if (string.Equals(mod.ModuleName, "Game.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            gameBase = mod.BaseAddress;
                            break;
                        }
                    }
                }
                catch { }

                if (gameBase == IntPtr.Zero)
                    gameBase = new IntPtr(0x6F000000);

                IntPtr targetAddr = IntPtr.Add(gameBase, 0x00A54A14);
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
                if (hProcess == IntPtr.Zero) return false;

                try
                {
                    if (VirtualProtectEx(hProcess, targetAddr, (UIntPtr)nameBytes.Length, 0x40 /* PAGE_EXECUTE_READWRITE */, out uint oldProtect))
                    {
                        bool written = WriteProcessMemory(hProcess, targetAddr, nameBytes, nameBytes.Length, out _);
                        VirtualProtectEx(hProcess, targetAddr, (UIntPtr)nameBytes.Length, oldProtect, out _);
                        return written;
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
            catch { }
            return false;
        }
    }
}
