using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class ClientRepository
    {
        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            string query = "SELECT ClientId, FullName, DateOfBirth, PassportNumber, PhoneNumber, Address, ClientToken FROM dbo.Clients";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
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

        public int AddClient(Client client)
        {
            string query = "INSERT INTO dbo.Clients (FullName, DateOfBirth, PassportNumber, PhoneNumber, Address, ClientToken) " +
                           "VALUES (@FullName, @DateOfBirth, @PassportNumber, @PhoneNumber, @Address, @ClientToken); " +
                           "SELECT SCOPE_IDENTITY();";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FullName", client.FullName);
                command.Parameters.AddWithValue("@DateOfBirth", client.DateOfBirth);
                command.Parameters.AddWithValue("@PassportNumber", client.PassportNumber);
                command.Parameters.AddWithValue("@PhoneNumber", (object)client.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (object)client.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@ClientToken", client.ClientToken);

                connection.Open();
                var newClientId = Convert.ToInt32(command.ExecuteScalar());
                return newClientId;
            }
        }

        public void UpdateClient(Client client)
        {
            string query = "UPDATE dbo.Clients SET FullName = @FullName, DateOfBirth = @DateOfBirth, " +
                           "PassportNumber = @PassportNumber, PhoneNumber = @PhoneNumber, Address = @Address " +
                           "WHERE ClientId = @ClientId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FullName", client.FullName);
                command.Parameters.AddWithValue("@DateOfBirth", client.DateOfBirth);
                command.Parameters.AddWithValue("@PassportNumber", client.PassportNumber);
                command.Parameters.AddWithValue("@PhoneNumber", (object)client.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (object)client.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@ClientId", client.ClientId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteClient(int clientId)
        {
            string query = "DELETE FROM dbo.Clients WHERE ClientId = @ClientId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}