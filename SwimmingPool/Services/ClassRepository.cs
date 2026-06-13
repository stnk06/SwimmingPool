using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SwimmingPool.Models;

namespace SwimmingPool.Services
{
    public class ClassRepository
    {
        public List<Class> GetAllClasses()
        {
            var classes = new List<Class>();
            string query = @"
                SELECT c.ClassId, c.ActivityTypeId, c.TrainerId, c.PoolId, c.StartTime, c.EndTime, c.MaxParticipants,
                       at.TypeName, t.FullName AS TrainerName, p.PoolName
                FROM Classes c
                INNER JOIN ActivityTypes at ON c.ActivityTypeId = at.ActivityTypeId
                INNER JOIN Trainers t ON c.TrainerId = t.TrainerId
                INNER JOIN Pools p ON c.PoolId = p.PoolId";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        classes.Add(new Class
                        {
                            ClassId = reader.GetInt32(0),
                            ActivityTypeId = reader.GetInt32(1),
                            TrainerId = reader.GetInt32(2),
                            PoolId = reader.GetInt32(3),
                            StartTime = reader.GetDateTime(4),
                            EndTime = reader.GetDateTime(5),
                            MaxParticipants = reader.IsDBNull(6) ? 10 : reader.GetInt32(6),
                            ActivityTypeName = reader.GetString(7),
                            TrainerName = reader.GetString(8),
                            PoolName = reader.GetString(9)
                        });
                    }
                }
            }
            return classes;
        }

        public void AddClass(Class cls)
        {
            string query = @"
                INSERT INTO Classes (ActivityTypeId, TrainerId, PoolId, StartTime, EndTime, MaxParticipants)
                VALUES (@ActivityTypeId, @TrainerId, @PoolId, @StartTime, @EndTime, @MaxParticipants)";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActivityTypeId", cls.ActivityTypeId);
                command.Parameters.AddWithValue("@TrainerId", cls.TrainerId);
                command.Parameters.AddWithValue("@PoolId", cls.PoolId);
                command.Parameters.AddWithValue("@StartTime", cls.StartTime);
                command.Parameters.AddWithValue("@EndTime", cls.EndTime);
                command.Parameters.AddWithValue("@MaxParticipants", cls.MaxParticipants);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateClass(Class cls)
        {
            string query = @"
                UPDATE Classes
                SET ActivityTypeId = @ActivityTypeId,
                    TrainerId = @TrainerId,
                    PoolId = @PoolId,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    MaxParticipants = @MaxParticipants
                WHERE ClassId = @ClassId";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", cls.ClassId);
                command.Parameters.AddWithValue("@ActivityTypeId", cls.ActivityTypeId);
                command.Parameters.AddWithValue("@TrainerId", cls.TrainerId);
                command.Parameters.AddWithValue("@PoolId", cls.PoolId);
                command.Parameters.AddWithValue("@StartTime", cls.StartTime);
                command.Parameters.AddWithValue("@EndTime", cls.EndTime);
                command.Parameters.AddWithValue("@MaxParticipants", cls.MaxParticipants);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteClass(int classId)
        {
            string query = "DELETE FROM Classes WHERE ClassId = @ClassId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", classId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}