using System;
using System.ComponentModel;

namespace SwimmingPool.Models
{
    public class MembershipType : IDataErrorInfo
    {
        public int MembershipTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "TypeName":
                        if (string.IsNullOrWhiteSpace(TypeName)) error = "Название не может быть пустым.";
                        break;
                    case "Price":
                        if (Price <= 0) error = "Цена должна быть больше нуля.";
                        break;
                    case "DurationInDays":
                        if (DurationInDays <= 0) error = "Длительность должна быть больше нуля.";
                        break;
                }
                return error;
            }
        }
    }
}

