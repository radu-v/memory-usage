using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace memory_usage
{
   static class ProcessEntryExtensions
   {
      public static void ForEach(this ProcessEntry processEntries, Action<ProcessEntry, int> action, int indentLevel = 0)
      {
         action(processEntries, indentLevel);

         foreach (var entry in processEntries.Children)
         {
            entry.ForEach(action, indentLevel + 1);
         }
      }

      public static ProcessEntry FindProcessByName(this ProcessEntry root, string name)
      {
         var regex = new Regex($"{name}(?:\\..*)?", RegexOptions.IgnoreCase);

         return root.FindProcessByName(regex);
      }

      public static ProcessEntry FindProcessByName(this ProcessEntry root, Regex nameRegex)
      {
         if (root.ImageName != null && nameRegex.IsMatch(root.ImageName)) return root;

         foreach (var proc in root.Children)
         {
            var child = proc.FindProcessByName(nameRegex);
            if (child != null) return child;
         }

         return null;
      }

      public static ulong TotalWorkingSet(this ProcessEntry process)
      {
         return process.WorkingSet
                + process.Children
                   .Aggregate(0UL, (current, child) => current + child.TotalWorkingSet());
      }

      public static ProcessEntry BuildTree(this ProcessEntry root, IList<ProcessEntry> processList)
      {
         if (processList.Count == 0) return root;

         var children = processList.Where(p => p.ParentId == root.Id || !p.ValidParent).ToList();
         root.Children.AddRange(children);
         root.RemoveChildren(processList);

         for (var i = 0; i < children.Count; i++)
         {
            children[i] = children[i].BuildTree(processList);

            if (processList.Count == 0) break;
         }

         return root;
      }

      static void RemoveChildren(this ProcessEntry root, IList<ProcessEntry> processEntries)
      {
         foreach (var entry in root.Children)
         {
            processEntries.Remove(entry);
         }
      }
   }
}