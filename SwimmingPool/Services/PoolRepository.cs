using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class PoolRepository
    {
        public List<Pool> GetAllPools()
        {
            var pools = new List<Pool>();
            string query = "SELECT PoolId, PoolName, DepthMeters, WorkingHours FROM dbo.Pools";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pools.Add(new Pool
                        {
                            PoolId = reader.GetInt32(0),
                            PoolName = reader.GetString(1),
                            DepthMeters = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                            WorkingHours = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            return pools;
        }

        public void AddPool(Pool pool)
        {
            string query = "INSERT INTO dbo.Pools (PoolName, DepthMeters, WorkingHours) VALUES (@PoolName, @DepthMeters, @WorkingHours)";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PoolName", (object)pool.PoolName ?? DBNull.Value);
                command.Parameters.AddWithValue("@DepthMeters", pool.DepthMeters);
                command.Parameters.AddWithValue("@WorkingHours", (object)pool.WorkingHours ?? DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdatePool(Pool pool)
        {
            string query = "UPDATE dbo.Pools SET PoolName = @PoolName, DepthMeters = @DepthMeters, WorkingHours = @WorkingHours WHERE PoolId = @PoolId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PoolName", (object)pool.PoolName ?? DBNull.Value);
                command.Parameters.AddWithValue("@DepthMeters", pool.DepthMeters);
                command.Parameters.AddWithValue("@WorkingHours", (object)pool.WorkingHours ?? DBNull.Value);
                command.Parameters.AddWithValue("@PoolId", pool.PoolId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeletePool(int poolId)
        {
            string query = "DELETE FROM dbo.Pools WHERE PoolId = @PoolId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PoolId", poolId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
