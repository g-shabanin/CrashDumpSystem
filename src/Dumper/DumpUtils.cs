﻿namespace Dumper.Dumps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Minidump support tools.
    /// </summary>
    public static class DumpUtils
    {
        /// <summary>
        /// Folder for saved minidumps.
        /// </summary>
        public const string DumpDirectory = "Minidump";

        [DllImport("dbghelp.dll")]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, int processId, IntPtr hFile, int dumpType,
            IntPtr exceptionParam, IntPtr userStreamParam, IntPtr callStackParam);

        /// <summary>
        /// Write minidump to file.
        /// </summary>
        /// <param name="minidumpType">Minidump flag(s).</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool WriteDump(
            MinidumpType minidumpType = MinidumpType.MiniDumpWithFullMemory |
            MinidumpType.MiniDumpWithHandleData |
            MinidumpType.MiniDumpWithUnloadedModules |
            MinidumpType.MiniDumpWithFullMemoryInfo |
            MinidumpType.MiniDumpWithThreadInfo)
        {
            try
            {
                if (!Directory.Exists(DumpDirectory))
                {
                    Directory.CreateDirectory(DumpDirectory);
                }

                var currentProcess = Process.GetCurrentProcess();
                var fileName = GetNewDumpFileName(currentProcess.ProcessName);
                var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filePath = Path.Combine(currentDir, DumpDirectory, fileName);
                var handler = currentProcess.Handle;
                var processId = currentProcess.Id;

                using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
                {
                    return MiniDumpWriteDump(
                        handler,
                        processId,
                        fileStream.SafeFileHandle.DangerousGetHandle(),
                        (int)minidumpType,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetNewDumpFileName(string processName)
        {
            return string.Format("{0}_{1}_{2}.dmp", processName,
                DateTime.Now.ToString("yyyy-dd-mm_HH-mm-ss"),
                Path.GetRandomFileName().Replace(".", ""));
        }
    }
}