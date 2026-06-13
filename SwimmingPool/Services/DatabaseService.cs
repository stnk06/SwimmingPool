using System.Data.SqlClient;

namespace SwimmingPool.Services
{
    public static class DatabaseService
    {
        private const string connectionString = "Data Source=STNKDESKTOP\\SQLEXPRESS;Initial Catalog=SwimmingManagmentKlimovo;Integrated Security=True";

        public static string ConnectionString => connectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}