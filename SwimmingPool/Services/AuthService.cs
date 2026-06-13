using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SwimmingPool.Services
{
    public class AuthService
    {
        private const string TokenFileName = "session.token";

        public User Login(string username, string password, bool rememberMe)
        {
            string hash = HashPassword(password);
            User user = null;

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand("SELECT UserId, Role FROM dbo.Users WHERE Username = @Username AND PasswordHash = @Hash", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Hash", hash);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = username,
                            Role = reader.GetString(1)
                        };
                    }
                }
            }

            if (user != null && rememberMe)
            {
                string token = Guid.NewGuid().ToString();
                SaveTokenToDb(user.UserId, token);
                File.WriteAllText(TokenFileName, token);
            }

            return user;
        }

        public User TryAutoLogin()
        {
            if (!File.Exists(TokenFileName)) return null;

            string token = File.ReadAllText(TokenFileName);
            User user = null;

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand("SELECT UserId, Username, Role FROM dbo.Users WHERE AuthToken = @Token AND TokenExpiry > GETDATE()", connection))
            {
                command.Parameters.AddWithValue("@Token", token);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetString(2)
                        };
                    }
                }
            }

            return user;
        }

        public void Logout()
        {
            if (File.Exists(TokenFileName))
            {
                File.Delete(TokenFileName);
            }
        }

        private void SaveTokenToDb(int userId, string token)
        {
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand("UPDATE dbo.Users SET AuthToken = @Token, TokenExpiry = @Expiry WHERE UserId = @UserId", connection))
            {
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@Expiry", DateTime.Now.AddDays(7));
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand("SELECT UserId, Username, Role FROM dbo.Users", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Role = reader.GetString(2)
                        });
                    }
                }
            }
            return users;
        }

        public void AddUser(string username, string password, string role)
        {
            string hash = HashPassword(password);
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand("INSERT INTO dbo.Users (Username, PasswordHash, Role) VALUES (@Username, @Hash, @Role)", connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Hash", hash);
                command.Parameters.AddWithValue("@Role", role);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateUser(int userId, string username, string role, string newPassword)
        {
            using (var connection = DatabaseService.GetConnection())
            {
                connection.Open();
                string query = "UPDATE dbo.Users SET Username = @Username, Role = @Role";

                // Если пароль предоставлен, обновляем и его
                if (!string.IsNullOrEmpty(newPassword))
                {
                    query += ", PasswordHash = @Hash";
                }

                query += " WHERE UserId = @UserId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@UserId", userId);

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        command.Parameters.AddWithValue("@Hash", HashPassword(newPassword));
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(int userId, int currentUserId)
        {
            if (userId == currentUserId)
                throw new InvalidOperationException("Нельзя удалить самого себя.");

            int adminCount = 0;
            using (var connection = DatabaseService.GetConnection())
            {
                connection.Open();
                using (var cmdCount = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Role = 'Admin'", connection))
                {
                    adminCount = (int)cmdCount.ExecuteScalar();
                }

                string roleToDelete = "";
                using (var cmdRole = new SqlCommand("SELECT Role FROM dbo.Users WHERE UserId = @UserId", connection))
                {
                    cmdRole.Parameters.AddWithValue("@UserId", userId);
                    roleToDelete = (string)cmdRole.ExecuteScalar();
                }

                if (roleToDelete == "Admin" && adminCount <= 1)
                    throw new InvalidOperationException("Нельзя удалить последнего администратора.");

                using (var command = new SqlCommand("DELETE FROM dbo.Users WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}