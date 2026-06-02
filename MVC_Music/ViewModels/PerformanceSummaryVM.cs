using System.ComponentModel.DataAnnotations;

namespace MVC_Music.ViewModels
{
    public class PerformanceSummaryVM
    {
        public int ID { get; set; }

        [Display(Name = "Musician")]
        public string FormalName
        {
            get
            {
                return Last_Name + ", " + First_Name
                    + (string.IsNullOrEmpty(Middle_Name) ? "" :
                        (" " + (char?)Middle_Name[0] + ".").ToUpper());
            }
        }

        public string First_Name { get; set; } = "";

        public string? Middle_Name { get; set; }

        public string Last_Name { get; set; } = "";

        [Display(Name = "Average Fee Paid")]
        [DataType(DataType.Currency)]
        public double? Average_FeePaid { get; set; }

        [Display(Name = "Highest Fee Paid")]
        [DataType(DataType.Currency)]
        public double? Highest_FeePaid { get; set; }

        [Display(Name = "Lowest Fee Paid")]
        [DataType(DataType.Currency)]
        public double? Lowest_FeePaid { get; set; }

        [Display(Name = "Total No. Of Perfromances")]
        public int? Total_Performances { get; set; }

        [Display(Name = "Total Distinct Songs")]
        public int? Total_Songs { get; set; }
    }
}
