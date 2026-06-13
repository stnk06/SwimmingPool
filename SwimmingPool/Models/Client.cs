using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SwimmingPool.Models
{
    public class Client : IDataErrorInfo
    {
        public int ClientId { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-20);
        public string PassportNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Guid ClientToken { get; set; } = Guid.NewGuid();

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "FullName":
                        if (string.IsNullOrWhiteSpace(FullName))
                            error = "ФИО не может быть пустым.";
                        break;
                    case "PassportNumber":
                        if (string.IsNullOrWhiteSpace(PassportNumber))
                            error = "Номер паспорта не может быть пустым.";
                        break;
                    case "PhoneNumber":
                        if (!string.IsNullOrEmpty(PhoneNumber) && !Regex.IsMatch(PhoneNumber, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$"))
                            error = "Некорректный формат телефона.";
                        break;
                    case "DateOfBirth":
                        if (DateOfBirth > DateTime.Now)
                            error = "Дата рождения не может быть в будущем.";
                        break;
                }
                return error;
            }
        }
        public string Error => null;
    }
}