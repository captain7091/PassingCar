using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;

namespace PassingCarApis.SQL
{
    public static class StartMigration
    {
        /// <summary>
        /// Start migration on startup
        /// </summary>
        public static void Run()
        {
            try
            {
                string query = $@"
                        USE master;
                        CREATE DATABASE PassingCar;";
                if (query != "")
                {
                    using SqlConnection conn = new SqlConnection(AppConfiguration.MyConnectionString);
                    using SqlCommand command = new SqlCommand(query, conn);
                    try
                    {
                        conn.Open();
                        var i = command.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }

                MyMigration myMigration = new();
                IServiceProvider serviceProvider = myMigration.CreateServices();
                using IServiceScope scope = serviceProvider.CreateScope();
                myMigration.UpdateDatabase(scope.ServiceProvider);
            }
            catch (Exception)
            {
            }
        }
    }
}
