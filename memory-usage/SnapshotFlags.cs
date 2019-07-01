namespace memory_usage
{
   using System;

   [Flags]
   public enum SnapshotFlags : uint
   {
      HeapList = 0x00000001,
      Process = 0x00000002,
      Thread = 0x00000004,
      Module = 0x00000008
   }
}
