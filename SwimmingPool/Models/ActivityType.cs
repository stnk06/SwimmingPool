using System;
using System.ComponentModel;

namespace SwimmingPool.Models
{
    public class ActivityType : IDataErrorInfo
    {
        public int ActivityTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                if (columnName == "TypeName" && string.IsNullOrWhiteSpace(TypeName))
                {
                    error = "Название типа не может быть пустым.";
                }
                return error;
            }
        }
        public string Error => null;
    }
}

