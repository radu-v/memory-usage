namespace memory_usage
{
   using System;
   using System.Runtime.InteropServices;
   using static Win32ApiHelpers;

   class Win32ApiImports
   {
      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessId);

      [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
      public static extern bool Process32First([In]IntPtr hSnapshot, ref ProcessEntry32 lppe);

      [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
      public static extern bool Process32Next([In]IntPtr hSnapshot, ref ProcessEntry32 lppe);

      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, long processId);

      [DllImport("psapi.dll", SetLastError = true)]
      public static extern bool GetProcessMemoryInfo(IntPtr hProcess, out ProcessMemoryCounters counters, uint size);

      [DllImport("kernel32", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool CloseHandle([In] IntPtr hObject);

      [DllImport("kernel32")]
      public static extern int GetLastError();

      [DllImport("advapi32.dll")]
      public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
         ref Win32ApiHelpers.LUID lpLuid);

      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern IntPtr GetCurrentProcess();

      // Use this signature if you do not want the previous state
      [DllImport("advapi32.dll", SetLastError=true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, 
         [MarshalAs(UnmanagedType.Bool)]bool disableAllPrivileges, 
         ref TOKEN_PRIVILEGES newState, uint zero, IntPtr null1, IntPtr null2);

      [DllImport("advapi32.dll", SetLastError=true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool OpenProcessToken(IntPtr processHandle,
         uint desiredAccess, out IntPtr tokenHandle);
   }
}