using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Phanmemwar3.Core
{
    public class SaveValueMuter
    {
        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint MEM_COMMIT = 0x1000;

        // Signature for 60.0f duration message in Game.dll
        private static readonly byte[] CallSig = new byte[]
        {
            0xC7, 0x44, 0x24, 0x08, 0x00, 0x00, 0x70, 0x42,
            0xC7, 0x44, 0x24, 0x04, 0x00, 0x00, 0x00, 0x00
        };

        // ret 14h (pop 5 arguments)
        private static readonly byte[] RetPatch = new byte[] { 0xC2, 0x14, 0x00 };

        public static bool PatchPluginDlls(string war3Dir, bool mute)
        {
            if (string.IsNullOrWhiteSpace(war3Dir) || !Directory.Exists(war3Dir))
                return false;

            string[] targets = {
                Path.Combine(war3Dir, "dz_w3_plugin.dll"),
                Path.Combine(war3Dir, "kkapi_local_plugin.dll"),
                Path.Combine(war3Dir, "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C", "plugin", "warcraft3", "kkapi_local_plugin.dll")
            };

            bool anyPatched = false;
            foreach (var target in targets)
            {
                if (!File.Exists(target)) continue;
                try
                {
                    byte[] data = File.ReadAllBytes(target);
                    bool modified = false;

                    if (data.Length == 1853400) // KKWE 2606
                    {
                        byte[] expected = mute ? new byte[] { 0x55, 0x8B, 0xEC } : new byte[] { 0xC2, 0x14, 0x00 };
                        byte[] replacement = mute ? new byte[] { 0xC2, 0x14, 0x00 } : new byte[] { 0x55, 0x8B, 0xEC };
                        if (data.Length > 0x573b0 + 3 && MatchesAt(data, 0x573b0, expected))
                        {
                            Array.Copy(replacement, 0, data, 0x573b0, replacement.Length);
                            modified = true;
                        }
                    }
                    else if (data.Length == 1706944) // KKWE 2485
                    {
                        byte[] expected = mute ? new byte[] { 0x55, 0x8B, 0xEC } : new byte[] { 0xC2, 0x14, 0x00 };
                        byte[] replacement = mute ? new byte[] { 0xC2, 0x14, 0x00 } : new byte[] { 0x55, 0x8B, 0xEC };
                        if (data.Length > 0x52b60 + 3 && MatchesAt(data, 0x52b60, expected))
                        {
                            Array.Copy(replacement, 0, data, 0x52b60, replacement.Length);
                            modified = true;
                        }
                    }
                    else if (data.Length == 573432) // Plugin 270 (Cơ bản)
                    {
                        byte expected = mute ? (byte)0x55 : (byte)0xC3;
                        byte replacement = mute ? (byte)0xC3 : (byte)0x55;
                        if (data.Length > 0x22350 && data[0x22350] == expected)
                        {
                            data[0x22350] = replacement;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllBytes(target, data);
                        anyPatched = true;
                    }
                }
                catch { }
            }

            return anyPatched;
        }

        private static bool MatchesAt(byte[] data, int offset, byte[] pattern)
        {
            if (offset + pattern.Length > data.Length) return false;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (data[offset + i] != pattern[i]) return false;
            }
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        private int _lastPatchedPid = 0;
        private CancellationTokenSource? _cts;

        public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        public void Start(Action<string> logCallback)
        {
            if (IsRunning) return;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var procs = Process.GetProcessesByName("war3");
                        foreach (var proc in procs)
                        {
                            if (proc.Id != _lastPatchedPid && !proc.HasExited)
                            {
                                if (TryPatchProcess(proc.Id))
                                {
                                    _lastPatchedPid = proc.Id;
                                    logCallback?.Invoke($"[OK] Đã tắt thông báo Save Value cho War3 (PID: {proc.Id})");
                                }
                            }
                        }
                    }
                    catch { }

                    await Task.Delay(1500, token);
                }
            }, token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
            _lastPatchedPid = 0;
        }

        public bool TryPatchProcess(int pid)
        {
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
            if (hProcess == IntPtr.Zero) return false;

            try
            {
                IntPtr currentAddr = IntPtr.Zero;
                IntPtr maxAddr = new IntPtr(0x7FFE0000);
                uint mbiSize = (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();

                while (currentAddr.ToInt64() < maxAddr.ToInt64())
                {
                    if (VirtualQueryEx(hProcess, currentAddr, out var mbi, mbiSize) == 0)
                        break;

                    if (mbi.State == MEM_COMMIT &&
                        (mbi.Protect == 0x20 || mbi.Protect == 0x40 || mbi.Protect == 0x04)) // PAGE_EXECUTE_READ, PAGE_EXECUTE_READWRITE, PAGE_READWRITE
                    {
                        int regionSize = (int)Math.Min((long)mbi.RegionSize, 10 * 1024 * 1024); // max 10MB chunk
                        byte[] buffer = new byte[regionSize];
                        if (ReadProcessMemory(hProcess, mbi.BaseAddress, buffer, regionSize, out var bytesRead) && bytesRead.ToInt64() > 0)
                        {
                            int matchOffset = FindBytes(buffer, CallSig);
                            if (matchOffset != -1)
                            {
                                IntPtr patchAddress = IntPtr.Add(mbi.BaseAddress, matchOffset);
                                if (VirtualProtectEx(hProcess, patchAddress, (UIntPtr)RetPatch.Length, PAGE_EXECUTE_READWRITE, out uint oldProtect))
                                {
                                    bool success = WriteProcessMemory(hProcess, patchAddress, RetPatch, RetPatch.Length, out _);
                                    VirtualProtectEx(hProcess, patchAddress, (UIntPtr)RetPatch.Length, oldProtect, out _);
                                    if (success)
                                        return true;
                                }
                            }
                        }
                    }

                    long nextAddr = mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64();
                    if (nextAddr <= currentAddr.ToInt64()) break;
                    currentAddr = new IntPtr(nextAddr);
                }
            }
            finally
            {
                CloseHandle(hProcess);
            }

            return false;
        }

        private static int FindBytes(byte[] haystack, byte[] needle)
        {
            if (haystack.Length < needle.Length) return -1;
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }
    }
}
