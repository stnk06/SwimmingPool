using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SwimmingPool.Models;

namespace SwimmingPool.Services
{
    public class SickLeaveRepository : ISickLeaveRepository
    {
        private readonly string _connectionString;

        public SickLeaveRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddSickLeave(SickLeave sickLeave)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        // 1. Сохраняем информацию о больничном
                        string insertSickLeaveQuery = @"
                            INSERT INTO SickLeaves (ClientId, StartDate, EndDate, Notes, IsProcessed)
                            VALUES (@ClientId, @StartDate, @EndDate, @Notes, 1);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand cmd = new SqlCommand(insertSickLeaveQuery, connection, transaction))
                        {
                            cmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = sickLeave.ClientId;
                            cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value = sickLeave.StartDate;
                            cmd.Parameters.Add("@EndDate", SqlDbType.Date).Value = sickLeave.EndDate;
                            cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(sickLeave.Notes) ? DBNull.Value : (object)sickLeave.Notes;
                            sickLeave.Id = (int)cmd.ExecuteScalar();
                        }

                        int daysToExtend = (int)(sickLeave.EndDate.Date - sickLeave.StartDate.Date).TotalDays + 1;

                        // 2. Продлеваем абонемент. 
                        // ТОНКОСТЬ ИСПРАВЛЕНА: Проверяем, что абонемент был активен на момент НАЧАЛА болезни, а не на сегодня.
                        string updateMembershipQuery = @"
                            UPDATE Memberships 
                            SET ExpiryDate = DATEADD(day, @Days, ExpiryDate) 
                            WHERE ClientId = @ClientId 
                              AND ExpiryDate >= @SickStartDate;";

                        using (SqlCommand cmd = new SqlCommand(updateMembershipQuery, connection, transaction))
                        {
                            cmd.Parameters.Add("@Days", SqlDbType.Int).Value = daysToExtend;
                            cmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = sickLeave.ClientId;
                            cmd.Parameters.Add("@SickStartDate", SqlDbType.Date).Value = sickLeave.StartDate;
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<SickLeave> GetSickLeavesByClient(int clientId)
        {
            var result = new List<SickLeave>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, ClientId, StartDate, EndDate, Notes, IsProcessed FROM SickLeaves WHERE ClientId = @ClientId ORDER BY StartDate DESC";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = clientId;
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new SickLeave
                            {
                                Id = reader.GetInt32(0),
                                ClientId = reader.GetInt32(1),
                                StartDate = reader.GetDateTime(2),
                                EndDate = reader.GetDateTime(3),
                                Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                                IsProcessed = reader.GetBoolean(5)
                            });
                        }
                    }
                }
            }
            return result;
        }
    }
}