using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class ReportsRepository
    {
        public int GetTotalClientsCount()
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM dbo.Clients;";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                count = (int)command.ExecuteScalar();
            }
            return count;
        }

        public int GetActiveMembershipsCount()
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM dbo.Memberships WHERE GETDATE() BETWEEN PurchaseDate AND ExpiryDate;";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                count = (int)command.ExecuteScalar();
            }
            return count;
        }

        public List<RevenueReportItem> GetRevenueByPeriod(DateTime startDate, DateTime endDate)
        {
            var report = new List<RevenueReportItem>();
            string query = @"
                SELECT 
                    mt.TypeName,
                    COUNT(m.MembershipId) as SalesCount,
                    SUM(mt.Price) as TotalRevenue
                FROM dbo.Memberships m
                JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId
                WHERE m.PurchaseDate BETWEEN @StartDate AND @EndDate
                GROUP BY mt.TypeName
                ORDER BY TotalRevenue DESC;";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        report.Add(new RevenueReportItem
                        {
                            MembershipType = reader.GetString(0),
                            SalesCount = reader.GetInt32(1),
                            TotalRevenue = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return report;
        }

        public DailyStats GetTodaysStats()
        {
            var stats = new DailyStats();
            string query = @"
                SELECT 
                    COUNT(m.MembershipId) as NewMemberships,
                    ISNULL(SUM(mt.Price), 0) as TotalRevenue
                FROM dbo.Memberships m
                JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId
                WHERE CONVERT(date, m.PurchaseDate) = CONVERT(date, GETDATE());";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.NewMembershipsToday = reader.GetInt32(0);
                        stats.RevenueToday = reader.GetDecimal(1);
                    }
                }
            }
            return stats;
        }
    }
}
