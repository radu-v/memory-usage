namespace memory_usage
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Text.RegularExpressions;

   static class Program
   {
      static void Main(string[] args)
      {
         try
         {
            AdjustPrivileges();

            var processTree = GetProcesses()
               .ToProcessTree();

            processTree
               .ForEach((p, i) =>
               {
                  Console.WriteLine($"{new string(' ', i)}{p.Id}::{p.ImageName} - {ToMegabytes(p.WorkingSet)} MB");
               });


            if (args.Length > 0)
            {
               Console.WriteLine();
               var procname = args[0];

               var process = processTree.FindProcessByName(Regex.Escape(procname));

               if (process == null)
               {
                  Console.WriteLine($"Process {procname} not found.");
                  return;
               }
               var processUsage = process.TotalWorkingSet();
               Console.WriteLine($"Process {process.ImageName} uses {ToMegabytes(processUsage)} MB");
            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      static void AdjustPrivileges()
      {
         var luid = new Win32ApiHelpers.LUID();
         if (!Win32ApiImports.LookupPrivilegeValue(null, "SeDebugPrivilege", ref luid))
         {
            return;
         }

         var tokenHandle = IntPtr.Zero;
         try
         {
            if (!Win32ApiImports.OpenProcessToken(Win32ApiImports.GetCurrentProcess(), Win32ApiHelpers.TOKEN_ADJUST_PRIVILEGES, out tokenHandle))
            {
               return;
            }

            var tp = new Win32ApiHelpers.TOKEN_PRIVILEGES
            {
               PrivilegeCount = 1,
               Privileges = new Win32ApiHelpers.LUID_AND_ATTRIBUTES[1]
            };

            tp.Privileges[0].Luid = luid;
            tp.Privileges[0].Attributes = Win32ApiHelpers.SE_PRIVILEGE_ENABLED;

            Win32ApiImports.AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
         }
         finally
         {
            if (tokenHandle != IntPtr.Zero)
            {
               Win32ApiImports.CloseHandle(tokenHandle);
            }
         }
      }

      static ProcessEntry ToProcessTree(this IEnumerable<ProcessEntry32> processes)
      {
         if (processes == null) throw new ArgumentNullException(nameof(processes));

         var processList = processes.ToList();

         var processEntryList = processList
            .OrderBy(p => p.th32ParentProcessID)
            .ThenBy(p => p.th32ProcessID)
            .Select(p => new ProcessEntry
            {
               Id = p.th32ProcessID,
               ParentId = p.th32ParentProcessID,
               ImageName = p.szExeFile,
               WorkingSet = GetWorkingSet(p.th32ProcessID),
               ValidParent = processList.IsProcessRunning(p.th32ParentProcessID)
            }).ToList();

         return new ProcessEntry()
            .BuildTree(processEntryList);
      }

      static ulong GetWorkingSet(uint processId)
      {
         var hProcess = Win32ApiImports.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);

         if (!Win32ApiImports.GetProcessMemoryInfo(hProcess, out var counters, (uint)Marshal.SizeOf(typeof(ProcessMemoryCounters))))
            return 0;

         Win32ApiImports.CloseHandle(hProcess);

         return counters.WorkingSetSize;
      }

      static ulong ToMegabytes(ulong? bytes) => bytes / 1048576 ?? 0;

      public static bool IsProcessRunning(this IEnumerable<ProcessEntry32> processes, uint pid) => processes.Any(p => p.th32ProcessID == pid);

      public static IEnumerable<ProcessEntry32> GetProcesses()
      {
         var handleToSnapshot = IntPtr.Zero;
         try
         {
            var procEntry = new ProcessEntry32 { dwSize = (uint)Marshal.SizeOf(typeof(ProcessEntry32)) };
            handleToSnapshot = Win32ApiImports.CreateToolhelp32Snapshot(SnapshotFlags.Process, 0);

            if (Win32ApiImports.Process32First(handleToSnapshot, ref procEntry))
            {
               do yield return procEntry;
               while (Win32ApiImports.Process32Next(handleToSnapshot, ref procEntry));
            }
            else
            {
               throw new ApplicationException($"Failed with win32 error code {Marshal.GetLastWin32Error()}");
            }
         }
         finally
         {
            Win32ApiImports.CloseHandle(handleToSnapshot);
         }
      }
   }
}
