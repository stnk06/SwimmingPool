using System;

namespace SwimmingPool.Models
{
    public class SickLeave
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Notes { get; set; }
        public bool IsProcessed { get; set; }
    }
}