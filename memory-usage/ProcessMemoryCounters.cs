namespace memory_usage
{
   using System.Runtime.InteropServices;

   [StructLayout(LayoutKind.Sequential, Size = 72)]
   struct ProcessMemoryCounters
   {
      public uint cb;
      public uint PageFaultCount;
      public ulong PeakWorkingSetSize;
      public ulong WorkingSetSize;
      public ulong QuotaPeakPagedPoolUsage;
      public ulong QuotaPagedPoolUsage;
      public ulong QuotaPeakNonPagedPoolUsage;
      public ulong QuotaNonPagedPoolUsage;
      public ulong PagefileUsage;
      public ulong PeakPagefileUsage;
   }
}
