using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public class TrainerRepository
    {
        public List<Trainer> GetAllTrainers()
        {
            var trainers = new List<Trainer>();
            string query = "SELECT TrainerId, FullName, Specialization, ExperienceYears, ContactInfo FROM dbo.Trainers";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trainers.Add(new Trainer
                        {
                            TrainerId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Specialization = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ExperienceYears = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                            ContactInfo = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }
            }
            return trainers;
        }

        public void AddTrainer(Trainer trainer)
        {
            string query = "INSERT INTO dbo.Trainers (FullName, Specialization, ExperienceYears, ContactInfo) VALUES (@FullName, @Specialization, @ExperienceYears, @ContactInfo)";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FullName", trainer.FullName);
                command.Parameters.AddWithValue("@Specialization", (object)trainer.Specialization ?? DBNull.Value);
                command.Parameters.AddWithValue("@ExperienceYears", (object)trainer.ExperienceYears ?? DBNull.Value);
                command.Parameters.AddWithValue("@ContactInfo", (object)trainer.ContactInfo ?? DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateTrainer(Trainer trainer)
        {
            string query = "UPDATE dbo.Trainers SET FullName = @FullName, Specialization = @Specialization, ExperienceYears = @ExperienceYears, ContactInfo = @ContactInfo WHERE TrainerId = @TrainerId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FullName", trainer.FullName);
                command.Parameters.AddWithValue("@Specialization", (object)trainer.Specialization ?? DBNull.Value);
                command.Parameters.AddWithValue("@ExperienceYears", (object)trainer.ExperienceYears ?? DBNull.Value);
                command.Parameters.AddWithValue("@ContactInfo", (object)trainer.ContactInfo ?? DBNull.Value);
                command.Parameters.AddWithValue("@TrainerId", trainer.TrainerId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteTrainer(int trainerId)
        {
            string query = "DELETE FROM dbo.Trainers WHERE TrainerId = @trainerId";
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@trainerId", trainerId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}

