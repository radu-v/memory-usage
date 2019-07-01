namespace memory_usage
{
   using System;
   using System.Diagnostics;
   using System.Runtime.InteropServices;

   [DebuggerDisplay("pid = {th32ProcessID}, parent = {th32ParentProcessID}, exe = {szExeFile}")]
   [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
   public struct ProcessEntry32
   {
      const int MAX_PATH = 260;
      internal uint dwSize;
      internal uint cntUsage;
      internal uint th32ProcessID;
      internal IntPtr th32DefaultHeapID;
      internal uint th32ModuleID;
      internal uint cntThreads;
      internal uint th32ParentProcessID;
      internal int pcPriClassBase;
      internal uint dwFlags;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
      internal string szExeFile;

      internal bool bParentValid;
   }
}
