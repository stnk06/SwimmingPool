using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SwimmingPool.Models
{
    public class Pool : IDataErrorInfo
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; }
        public decimal DepthMeters { get; set; }
        public string WorkingHours { get; set; }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "PoolName":
                        if (string.IsNullOrWhiteSpace(PoolName)) error = "Название бассейна не может быть пустым.";
                        break;
                    case "DepthMeters":
                        if (DepthMeters <= 0) error = "Глубина должна быть положительным числом.";
                        break;
                }
                return error;
            }
        }
    }
}
