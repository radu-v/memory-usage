using System;

namespace memory_usage
{
   [Flags]
   public enum ProcessAccessFlags : uint
   {
      QueryInformation = 0x00000400,
      QueryLimitedInformation = 0x00001000
   }
}
