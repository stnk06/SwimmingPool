using System;
using System.ComponentModel;

namespace SwimmingPool.Models
{
    public class Membership : IDataErrorInfo
    {
        public int MembershipId { get; set; }
        public int ClientId { get; set; }
        public int MembershipTypeId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(30);

        public string ClientFullName { get; set; }
        public string MembershipTypeName { get; set; }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "ClientId":
                        if (ClientId <= 0) error = "Необходимо выбрать клиента.";
                        break;
                    case "MembershipTypeId":
                        if (MembershipTypeId <= 0) error = "Необходимо выбрать тип абонемента.";
                        break;
                    case "ExpiryDate":
                        if (ExpiryDate < PurchaseDate) error = "Дата окончания не может быть раньше даты покупки.";
                        break;
                }
                return error;
            }
        }
    }
}

