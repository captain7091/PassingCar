using Microsoft.Data.SqlClient;

namespace PassingCarApis.Configuration
{
    public class AppConfiguration
    {
        public static string? MyConnectionString
        {
            // LOCAL DATABASE (Commented Out)
            //get => "Server=DESKTOP-CEAQLHA;Initial Catalog=PassingCar;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connection Timeout=30;";
            
            // ALTERNATIVE LOCAL DATABASE (Commented)
            //get => "Server=.\\SQLEXPRESS;Initial Catalog=PassingCar;Persist Security Info=False;User ID=passingcar;Password=admin1passing@car;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";

            // PRODUCTION DATABASE (Currently Active)
            get => "Server=winbd-server2.hostever.com,14335;Initial Catalog=discount_PassingCar;Persist Security Info=False;User ID=TestUser;Password=Test@123;Connection Timeout=10;Command Timeout=15;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
            set { }
        }
        public static bool CanDeleteLogs = true;
        public static bool CatchFullException = false;

        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new(MyConnectionString);

            return conn;
        }
    }
}
