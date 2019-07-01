namespace memory_usage
{
   using System.Collections.Generic;
   using System.Diagnostics;

   [DebuggerDisplay("Id = {Id}, ParentId = {ParentId}, ImageName = {ImageName}")]
   public class ProcessEntry
   {
      public long Id { get; set; }
      public long ParentId { get; set; }
      public string ImageName { get; set; }
      public ulong WorkingSet { get; set; }

      public List<ProcessEntry> Children { get; } = new List<ProcessEntry>();
      public bool ValidParent { get; internal set; }
   }
}
