using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WorkTodo {
 public static class TaskDates {
  public static bool Matches(TaskItem task, DateTime date) {
   return task.DueDate == date.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
  }
  public static DateTime[] MarkedDates(IEnumerable<TaskItem> tasks) {
   var dates=new HashSet<DateTime>();
   foreach(var task in tasks) {
    DateTime date;
    if(DateTime.TryParseExact(task.DueDate,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date)) dates.Add(date);
   }
   return dates.OrderBy(d=>d).ToArray();
  }
 }
}
