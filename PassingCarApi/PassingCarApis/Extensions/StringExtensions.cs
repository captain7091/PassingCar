using Dapper;
using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace PassingCarApis.Extensions
{
    public static class StringExtensions
    {
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("PassingCarKeyEncryptDevelop");
        private static readonly ThreadLocal<HMACSHA256> Algorithm = new(() => new HMACSHA256(EncryptionKey));

        public static string EncryptString(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = Algorithm.Value.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }
        public static void AddToLog(this string message, int userId = 0)
        {
            //using SqlConnection connection = AppConfiguration.GetConnection();
            //connection.Open();
            //try
            //{
            //    string query = $@"INSERT INTO [dbo].[Log]
            //            (Message,
            //             UserId,
            //             CreatedAt)
            //            Values 
            //              (@Message,
            //                @UserId,
            //                GETDATE())";
            //    _ = connection.Query(query, new { Message = message, UserId = userId });
            //}
            //catch (Exception ex)
            //{
            //    ex.CatchIt();
            //}
            //finally
            //{
            //    connection.Close();
            //}
        }
    }
}
