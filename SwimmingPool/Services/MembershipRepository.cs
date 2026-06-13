using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class MembershipRepository
    {
        public List<Membership> GetAllMemberships()
        {
            var memberships = new List<Membership>();
            string query = @"
                SELECT 
                    m.MembershipId, m.ClientId, m.MembershipTypeId, m.PurchaseDate, m.ExpiryDate,
                    c.FullName AS ClientFullName, mt.TypeName AS MembershipTypeName
                FROM dbo.Memberships m
                LEFT JOIN dbo.Clients c ON m.ClientId = c.ClientId
                LEFT JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        memberships.Add(new Membership
                        {
                            MembershipId = reader.GetInt32(0),
                            ClientId = reader.GetInt32(1),
                            MembershipTypeId = reader.GetInt32(2),
                            PurchaseDate = reader.GetDateTime(3),
                            ExpiryDate = reader.GetDateTime(4),
                            ClientFullName = reader.IsDBNull(5) ? "Клиент не найден" : reader.GetString(5),
                            MembershipTypeName = reader.IsDBNull(6) ? "Тип не найден" : reader.GetString(6)
                        });
                    }
                }
            }
            return memberships;
        }

        public void AddMembership(Membership membership)
        {
            string query = "INSERT INTO dbo.Memberships (ClientId, MembershipTypeId, PurchaseDate, ExpiryDate) VALUES (@ClientId, @MembershipTypeId, @PurchaseDate, @ExpiryDate)";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClientId", membership.ClientId);
                command.Parameters.AddWithValue("@MembershipTypeId", membership.MembershipTypeId);
                command.Parameters.AddWithValue("@PurchaseDate", membership.PurchaseDate);
                command.Parameters.AddWithValue("@ExpiryDate", membership.ExpiryDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateMembership(Membership membership)
        {
            string query = "UPDATE dbo.Memberships SET ClientId = @ClientId, MembershipTypeId = @MembershipTypeId, PurchaseDate = @PurchaseDate, ExpiryDate = @ExpiryDate WHERE MembershipId = @MembershipId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClientId", membership.ClientId);
                command.Parameters.AddWithValue("@MembershipTypeId", membership.MembershipTypeId);
                command.Parameters.AddWithValue("@PurchaseDate", membership.PurchaseDate);
                command.Parameters.AddWithValue("@ExpiryDate", membership.ExpiryDate);
                command.Parameters.AddWithValue("@MembershipId", membership.MembershipId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteMembership(int membershipId)
        {
            string query = "DELETE FROM dbo.Memberships WHERE MembershipId = @MembershipId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MembershipId", membershipId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public Membership GetActiveMembershipForClient(int clientId)
        {
            Membership membership = null;
            string query = @"
                SELECT TOP 1
                    m.MembershipId, m.ClientId, m.MembershipTypeId, m.PurchaseDate, m.ExpiryDate,
                    c.FullName AS ClientFullName, mt.TypeName AS MembershipTypeName
                FROM dbo.Memberships m
                JOIN dbo.Clients c ON m.ClientId = c.ClientId
                JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId
                WHERE m.ClientId = @ClientId AND m.ExpiryDate >= GETDATE()
                ORDER BY m.ExpiryDate DESC";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        membership = new Membership
                        {
                            MembershipId = reader.GetInt32(0),
                            ClientId = reader.GetInt32(1),
                            MembershipTypeId = reader.GetInt32(2),
                            PurchaseDate = reader.GetDateTime(3),
                            ExpiryDate = reader.GetDateTime(4),
                            ClientFullName = reader.GetString(5),
                            MembershipTypeName = reader.GetString(6)
                        };
                    }
                }
            }
            return membership;
        }

        public List<Membership> GetExpiringSoonMemberships()
        {
            var memberships = new List<Membership>();
            string query = @"
                SELECT 
                    m.MembershipId, m.ClientId, m.MembershipTypeId, m.PurchaseDate, m.ExpiryDate,
                    c.FullName AS ClientFullName, mt.TypeName AS MembershipTypeName
                FROM dbo.Memberships m
                LEFT JOIN dbo.Clients c ON m.ClientId = c.ClientId
                LEFT JOIN dbo.MembershipTypes mt ON m.MembershipTypeId = mt.MembershipTypeId
                WHERE m.ExpiryDate BETWEEN GETDATE() AND DATEADD(day, 7, GETDATE())
                ORDER BY m.ExpiryDate ASC";

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        memberships.Add(new Membership
                        {
                            MembershipId = reader.GetInt32(0),
                            ClientId = reader.GetInt32(1),
                            MembershipTypeId = reader.GetInt32(2),
                            PurchaseDate = reader.GetDateTime(3),
                            ExpiryDate = reader.GetDateTime(4),
                            ClientFullName = reader.IsDBNull(5) ? "Клиент не найден" : reader.GetString(5),
                            MembershipTypeName = reader.IsDBNull(6) ? "Тип не найден" : reader.GetString(6)
                        });
                    }
                }
            }
            return memberships;
        }
    }
}

