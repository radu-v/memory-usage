namespace memory_usage
{
   using System.Runtime.InteropServices;

   public static class Win32ApiHelpers
   {
      const int ANYSIZE_ARRAY = 1;

      public const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
      public const uint SE_PRIVILEGE_ENABLED = 0x00000002;

      [StructLayout(LayoutKind.Sequential)]
      public struct LUID
      {
         public uint LowPart;
         public int HighPart;
      }

      public struct TOKEN_PRIVILEGES
      {
         public int PrivilegeCount;
         [MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
         public LUID_AND_ATTRIBUTES[] Privileges;
      }

      [StructLayout(LayoutKind.Sequential, Pack = 4)]
      public struct LUID_AND_ATTRIBUTES
      {
         public LUID Luid;
         public uint Attributes;
      }
   }
}