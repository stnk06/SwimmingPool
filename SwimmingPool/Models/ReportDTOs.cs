using System;

namespace SwimmingPool.Models
{
    public class TrainerPerformanceDTO
    {
        public string TrainerName { get; set; }
        public string Specialization { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public double TotalHours { get; set; }
        public double AvgAttendance { get; set; }
    }

    public class AttendanceTrendDTO
    {
        public string DayOfWeek { get; set; }
        public int TotalVisits { get; set; }
        public string BusiestTime { get; set; }
    }

    public class MonthlyRevenueDTO
    {
        public string MonthYear { get; set; }
        public decimal TotalRevenue { get; set; }
        public int MembershipsSold { get; set; }
        public decimal AvgCheck { get; set; }
    }

    public class PoolOccupancyDTO
    {
        public string PoolName { get; set; }
        public int TotalClasses { get; set; }
        public int TotalVisitors { get; set; }
        public double OccupancyRate { get; set; } 
    }
}