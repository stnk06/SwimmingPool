using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace SwimmingPool.Services
{
    public class AdvancedReportsRepository
    {
        public List<TrainerPerformanceDTO> GetTrainerPerformance()
        {
            var result = new List<TrainerPerformanceDTO>();
            string query = @"
                SELECT 
                    t.FullName,
                    t.Specialization,
                    COUNT(DISTINCT c.ClassId) as TotalClasses,
                    COUNT(cr.RegistrationId) as TotalStudents,
                    ISNULL(CAST(SUM(DATEDIFF(MINUTE, c.StartTime, c.EndTime)) / 60.0 AS FLOAT), 0) as TotalHours
                FROM dbo.Trainers t
                LEFT JOIN dbo.Classes c ON t.TrainerId = c.TrainerId
                LEFT JOIN dbo.ClassRegistrations cr ON c.ClassId = cr.ClassId
                GROUP BY t.TrainerId, t.FullName, t.Specialization
                ORDER BY TotalStudents DESC";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double hours = 0;
                        try { hours = Convert.ToDouble(reader["TotalHours"]); } catch { }

                        result.Add(new TrainerPerformanceDTO
                        {
                            TrainerName = reader.GetString(0),
                            Specialization = reader.IsDBNull(1) ? "Не указана" : reader.GetString(1),
                            TotalClasses = reader.GetInt32(2),
                            TotalStudents = reader.GetInt32(3),
                            TotalHours = Math.Round(hours, 1),
                            AvgAttendance = reader.GetInt32(2) > 0 ? Math.Round((double)reader.GetInt32(3) / reader.GetInt32(2), 1) : 0
                        });
                    }
                }
            }
            return result;
        }

        public List<MonthlyRevenueDTO> GetMonthlyRevenueStats()
        {
            var result = new List<MonthlyRevenueDTO>();
            string query = @"
                SELECT 
                    FORMAT(m.PurchaseDate, 'yyyy-MM') as MonthKey,
                    ISNULL(SUM(mt.Price), 0) as Revenue,
                    COUNT(m.MembershipId) as Count
                FROM dbo.Memberships m
                JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId
                WHERE m.PurchaseDate >= DATEADD(year, -1, GETDATE())
                GROUP BY FORMAT(m.PurchaseDate, 'yyyy-MM')
                ORDER BY MonthKey";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string monthKey = reader.GetString(0);
                        decimal revenue = reader.GetDecimal(1);
                        int count = reader.GetInt32(2);

                        DateTime date;
                        if (!DateTime.TryParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        {
                            date = DateTime.MinValue;
                        }

                        result.Add(new MonthlyRevenueDTO
                        {
                            MonthYear = date.ToString("MMMM yyyy", new CultureInfo("ru-RU")),
                            TotalRevenue = revenue,
                            MembershipsSold = count,
                            AvgCheck = count > 0 ? Math.Round(revenue / count, 2) : 0
                        });
                    }
                }
            }
            return result;
        }

        public List<PoolOccupancyDTO> GetPoolOccupancy()
        {
            var result = new List<PoolOccupancyDTO>();
            string query = @"
                SELECT 
                    p.PoolName,
                    COUNT(DISTINCT c.ClassId) as ClassCount,
                    COUNT(cr.RegistrationId) as VisitorCount,
                    ISNULL(SUM(c.MaxParticipants), 0) as TotalCapacity
                FROM dbo.Pools p
                LEFT JOIN dbo.Classes c ON p.PoolId = c.PoolId
                LEFT JOIN dbo.ClassRegistrations cr ON c.ClassId = cr.ClassId
                GROUP BY p.PoolId, p.PoolName";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int visitors = reader.GetInt32(2);
                        int capacity = reader.GetInt32(3);

                        result.Add(new PoolOccupancyDTO
                        {
                            PoolName = reader.GetString(0),
                            TotalClasses = reader.GetInt32(1),
                            TotalVisitors = visitors,
                            OccupancyRate = capacity > 0 ? Math.Round(((double)visitors / capacity) * 100, 1) : 0
                        });
                    }
                }
            }
            return result;
        }

        public List<AttendanceTrendDTO> GetAttendanceTrends()
        {
            var result = new List<AttendanceTrendDTO>();
            string query = @"
                SET LANGUAGE Russian;
                SELECT 
                    DATENAME(dw, c.StartTime) as DayName,
                    DATEPART(dw, c.StartTime) as DayNum,
                    COUNT(cr.RegistrationId) as Visits
                FROM dbo.Classes c
                JOIN dbo.ClassRegistrations cr ON c.ClassId = cr.ClassId
                GROUP BY DATENAME(dw, c.StartTime), DATEPART(dw, c.StartTime)
                ORDER BY CASE 
                    WHEN DATEPART(dw, c.StartTime) = 1 THEN 7 
                    ELSE DATEPART(dw, c.StartTime) - 1 
                END";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new AttendanceTrendDTO
                        {
                            DayOfWeek = reader.GetString(0),
                            TotalVisits = reader.GetInt32(2),
                            BusiestTime = "Вечер"
                        });
                    }
                }
            }
            return result;
        }
    }
}