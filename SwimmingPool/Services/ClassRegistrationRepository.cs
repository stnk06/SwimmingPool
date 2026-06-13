using SwimmingPool.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class ClassRegistrationRepository
    {
        public List<Client> GetRegisteredClientsForClass(int classId)
        {
            var clients = new List<Client>();
            string query = @"
                SELECT c.ClientId, c.FullName, c.DateOfBirth, c.PassportNumber, c.PhoneNumber, c.Address, c.ClientToken
                FROM dbo.Clients c
                INNER JOIN dbo.ClassRegistrations cr ON c.ClientId = cr.ClientId
                WHERE cr.ClassId = @ClassId
                ORDER BY cr.RegistrationDate ASC";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", classId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            DateOfBirth = reader.GetDateTime(2),
                            PassportNumber = reader.GetString(3),
                            PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ClientToken = reader.GetGuid(6)
                        });
                    }
                }
            }
            return clients;
        }

        public void RegisterClientToClass(int classId, int clientId)
        {
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM dbo.ClassRegistrations WHERE ClassId = @ClassId AND ClientId = @ClientId)
                BEGIN
                    INSERT INTO dbo.ClassRegistrations (ClassId, ClientId, RegistrationDate, AttendanceStatus) 
                    VALUES (@ClassId, @ClientId, GETDATE(), N'Записан')
                END";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", classId);
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UnregisterClientFromClass(int classId, int clientId)
        {
            string query = "DELETE FROM dbo.ClassRegistrations WHERE ClassId = @ClassId AND ClientId = @ClientId";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", classId);
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}