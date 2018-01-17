using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArabicTextAnalyzer.Domain.Models
{
    [Table("ScheduleTask")]
    public class ScheduleTask
    {
        [Key]
        public int Id { get; set; }

        public string TaskName { get; set; }
        public int RepeatDays { get; set; }
        public DateTime? NextRunDate { get; set; }
        public string TimeToStart { get; set; }
        public string MethodName { get; set; }

        public bool Active { get; set; }
        public int TaskType { get; set; }


    }
}
