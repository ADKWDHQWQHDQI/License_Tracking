using Microsoft.Data.SqlClient;
using System;
using System.IO;

class DatabaseSeeder
{
    static void Main()
    {
        string connectionString = "Server=IT-32\\SANDEEP;Database=LicenseTrackingDB;Trusted_Connection=True;TrustServerCertificate=True;";
        string sqlFilePath = @"c:\Users\sandeepk\Downloads\Project\License_Tracking\SQL_Scripts\Insert_Sample_Data.sql";

        try
        {
            Console.WriteLine("Reading SQL script...");
            string sqlScript = File.ReadAllText(sqlFilePath);

            Console.WriteLine("Connecting to database...");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected successfully!");

                // Split the script by GO statements and execute each batch
                var batches = sqlScript.Split(new string[] { "\nGO\n", "\ngo\n", "\nGo\n", "\ngO\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                    {
                        using (var command = new SqlCommand(batch, connection))
                        {
                            command.CommandTimeout = 300; // 5 minutes timeout
                            var result = command.ExecuteNonQuery();
                            Console.WriteLine($"Batch executed. Rows affected: {result}");
                        }
                    }
                }

                Console.WriteLine("Sample data insertion completed successfully!");

                // Verify the data was inserted
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Deals WHERE DealName LIKE 'SAMPLE_%'", connection))
                {
                    var count = (int)command.ExecuteScalar();
                    Console.WriteLine($"Sample deals found: {count}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
