using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class ActivityTypeRepository
    {
        public List<ActivityType> GetAllActivityTypes()
        {
            var types = new List<ActivityType>();
            string query = "SELECT ActivityTypeId, TypeName, Description FROM dbo.ActivityTypes";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(new ActivityType
                        {
                            ActivityTypeId = reader.GetInt32(0),
                            TypeName = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return types;
        }

        public void AddActivityType(ActivityType activityType)
        {
            string query = "INSERT INTO dbo.ActivityTypes (TypeName, Description) VALUES (@TypeName, @Description)";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", activityType.TypeName);
                command.Parameters.AddWithValue("@Description", (object)activityType.Description ?? DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateActivityType(ActivityType activityType)
        {
            string query = "UPDATE dbo.ActivityTypes SET TypeName = @TypeName, Description = @Description WHERE ActivityTypeId = @ActivityTypeId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", activityType.TypeName);
                command.Parameters.AddWithValue("@Description", (object)activityType.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@ActivityTypeId", activityType.ActivityTypeId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteActivityType(int activityTypeId)
        {
            string query = "DELETE FROM dbo.ActivityTypes WHERE ActivityTypeId = @ActivityTypeId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ActivityTypeId", activityTypeId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
