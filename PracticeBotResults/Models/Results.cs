using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PracticeBotResults.Models
{
    public partial class Results
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public string AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public long QuestionId { get; set; }
        public string UserId { get; set; }
        public bool IsCorrect { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}
