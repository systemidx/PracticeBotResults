using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracticeBotResults.ViewModels
{
    public class ResultsViewModel
    {
        public string CourseName { get; set; }
        public IList<AssessmentViewModel> Assessments { get; set; }
    }

    public class AssessmentViewModel
    {
        public string AssessmentTitle { get; set; }
        
        public int Correct { get; set; }
        public int QuestionsTotal { get; set; }
        public decimal Score
        {
            get
            {
                return (Convert.ToDecimal(Correct) / Convert.ToDecimal(QuestionsTotal))*100;
            }
        }
    }
}
