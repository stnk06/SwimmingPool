
using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace SwimmingPool.Services
{
    public class MembershipTypeRepository
    {
        public List<MembershipType> GetAllMembershipTypes()
        {
            var types = new List<MembershipType>();
            string query = "SELECT MembershipTypeId, TypeName, Description, Price, DurationInDays FROM dbo.MembershipTypes";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(new MembershipType
                        {
                            MembershipTypeId = reader.GetInt32(0),
                            TypeName = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            DurationInDays = reader.GetInt32(4)
                        });
                    }
                }
            }
            return types;
        }

        public void AddMembershipType(MembershipType membershipType)
        {
            string query = "INSERT INTO dbo.MembershipTypes (TypeName, Description, Price, DurationInDays) VALUES (@TypeName, @Description, @Price, @DurationInDays)";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", membershipType.TypeName);
                command.Parameters.AddWithValue("@Description", (object)membershipType.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Price", membershipType.Price);
                command.Parameters.AddWithValue("@DurationInDays", membershipType.DurationInDays);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateMembershipType(MembershipType membershipType)
        {
            string query = "UPDATE dbo.MembershipTypes SET TypeName = @TypeName, Description = @Description, Price = @Price, DurationInDays = @DurationInDays WHERE MembershipTypeId = @MembershipTypeId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", membershipType.TypeName);
                command.Parameters.AddWithValue("@Description", (object)membershipType.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Price", membershipType.Price);
                command.Parameters.AddWithValue("@DurationInDays", membershipType.DurationInDays);
                command.Parameters.AddWithValue("@MembershipTypeId", membershipType.MembershipTypeId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteMembershipType(int membershipTypeId)
        {
            string query = "DELETE FROM dbo.MembershipTypes WHERE MembershipTypeId = @MembershipTypeId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MembershipTypeId", membershipTypeId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
