using System;
using System.ComponentModel;

namespace SwimmingPool.Models
{
    public class Trainer : IDataErrorInfo
    {
        public int TrainerId { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public string ContactInfo { get; set; }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "FullName":
                        if (string.IsNullOrWhiteSpace(FullName)) error = "ФИО тренера не может быть пустым.";
                        break;
                    case "ExperienceYears":
                        if (ExperienceYears.HasValue && ExperienceYears < 0) error = "Опыт не может быть отрицательным.";
                        break;
                }
                return error;
            }
        }
    }
}
