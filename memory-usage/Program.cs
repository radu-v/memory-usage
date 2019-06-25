using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace memory_usage
{
   class Program
   {
      static void Main(string[] args)
      {
         try
         {
            var processes = GetProcesses().ToList();
            var rootProcesses = processes.Where(pe => pe.ParentId == 0);
            var processesExceptRootProcesses = processes.Where(pe => pe.ParentId != 0).ToList();
            var usageSummary = rootProcesses
               .Select(parentProcessEntry => new ProcessEntry
               {
                  Id = parentProcessEntry.Id,
                  ExeFile = parentProcessEntry.ExeFile,
                  WorkingSet = parentProcessEntry.WorkingSet + AggregateChildrenWorkingSet(processesExceptRootProcesses, parentProcessEntry)
               })
               .OrderByDescending(p => p.WorkingSet)
               .ToDictionary(p => p.Id);


            foreach (var (id, processEntry) in usageSummary)
            {
               Console.WriteLine($"{id}::{processEntry.ExeFile} - {ToMegabytes(processEntry.WorkingSet)} MB");
            }

         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      static ulong AggregateChildrenWorkingSet(IList<ProcessEntry> processesExceptRootProcesses, ProcessEntry parentProcessEntry) =>
         processesExceptRootProcesses
            .Where(c => c.ParentId == parentProcessEntry.Id)
            .Aggregate(0UL, (current, childProcess) => current + childProcess.WorkingSet + AggregateChildrenWorkingSet(processesExceptRootProcesses, childProcess));

      static ulong ToMegabytes(ulong bytes) => bytes / 1048576;

      [DllImport("kernel32.dll", SetLastError = true)]
      static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessId);

      [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
      static extern bool Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

      [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
      static extern bool Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, uint processId);

      [DllImport("psapi.dll", SetLastError = true)]
      static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

      [DllImport("kernel32", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      static extern bool CloseHandle([In] IntPtr hObject);

      public static bool HasParentProcess(uint pid)
      {
         var hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, pid);
         if (hProcess == IntPtr.Zero) return false;

         CloseHandle(hProcess);
         return true;
      }

      public static IEnumerable<ProcessEntry> GetProcesses()
      {
         var handleToSnapshot = IntPtr.Zero;
         try
         {
            var procEntry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };
            handleToSnapshot = CreateToolhelp32Snapshot(SnapshotFlags.Process, 0);
            if (Process32First(handleToSnapshot, ref procEntry))
            {
               do
               {
                  var hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, procEntry.th32ProcessID);

                  if (!GetProcessMemoryInfo(hProcess, out var counters, (uint)Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS)))) continue;
                  CloseHandle(hProcess);

                  yield return new ProcessEntry
                  {
                     Id = procEntry.th32ProcessID,
                     ParentId = HasParentProcess(procEntry.th32ParentProcessID) ? procEntry.th32ParentProcessID : 0,
                     ExeFile = procEntry.szExeFile,
                     WorkingSet = counters.WorkingSetSize
                  };
               } while (Process32Next(handleToSnapshot, ref procEntry));
            }
            else
            {
               throw new ApplicationException($"Failed with win32 error code {Marshal.GetLastWin32Error()}");
            }
         }
         finally
         {
            CloseHandle(handleToSnapshot);
         }
      }

      [Flags]
      public enum SnapshotFlags : uint
      {
         HeapList = 0x00000001,
         Process = 0x00000002,
         Thread = 0x00000004,
         Module = 0x00000008
      }

      [Flags]
      public enum ProcessAccessFlags : uint
      {
         QueryInformation = 0x00000400
      }

      [StructLayout(LayoutKind.Sequential, Size = 72)]
      struct PROCESS_MEMORY_COUNTERS
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

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
      public struct PROCESSENTRY32
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
      }

      public class ProcessEntry
      {
         public uint Id { get; set; }
         public uint ParentId { get; set; }
         public string ExeFile { get; set; }
         public ulong WorkingSet { get; set; }
      }
   }
}
