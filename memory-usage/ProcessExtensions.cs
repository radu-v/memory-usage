using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace memory_usage
{
   public static class ProcessExtensions  
   {
      /// <summary>
      /// Get the child processes for a given process
      /// </summary>
      /// <param name="process"></param>
      /// <returns></returns>
      public static List<Process> GetChildProcesses(this Process process)
      {
         var results = new List<Process>();

         // query the management system objects for any process that has the current
         // process listed as it's parentprocessid
         var queryText = $"select processid from win32_process where parentprocessid = {process.Id}";
         using (var searcher = new ManagementObjectSearcher(queryText))
         {
            foreach (var obj in searcher.Get())
            {
               var data = obj.Properties["processid"].Value;
               if (data != null)
               {
                  // retrieve the process
                  var childId = Convert.ToInt32(data);
                  var childProcess = Process.GetProcessById(childId);

                  // ensure the current process is still live
                  results.Add(childProcess);
               }
            }
         }
         return results;
      }
      /// <summary>
      /// Get the Parent Process ID for a given process
      /// </summary>
      /// <param name="process"></param>
      /// <returns></returns>
      public static int? GetParentId(this Process process)
      {
         // query the management system objects
         var queryText = $"select parentprocessid from win32_process where processid = {process.Id}";
         using (var searcher = new ManagementObjectSearcher(queryText))
         {
            foreach (var obj in searcher.Get())
            {
               var data = obj.Properties["parentprocessid"].Value;
               if (data != null)
                  return Convert.ToInt32(data);
            }
         }
         return null;
      }
   }
}