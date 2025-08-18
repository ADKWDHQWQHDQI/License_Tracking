using System;
using Microsoft.Data.SqlClient;
using System.IO;

var connectionString = "Server=IT-32\\SANDEEP;Database=LicenseTrackingDB;Trusted_Connection=True;TrustServerCertificate=True;";
var sqlFilePath = @"c:\Users\sandeepk\Downloads\Project\License_Tracking\SQL_Scripts\Insert_Sample_Data.sql";

Console.WriteLine("Starting database seeding...");

try
{
    var sqlScript = File.ReadAllText(sqlFilePath);

    using var connection = new SqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("Connected to database successfully!");

    using var command = new SqlCommand(sqlScript, connection);
    command.CommandTimeout = 300;

    var result = command.ExecuteNonQuery();
    Console.WriteLine($"SQL script executed. Result: {result}");

    // Verify sample data
    using var verifyCommand = new SqlCommand("SELECT COUNT(*) FROM Deals WHERE DealName LIKE 'SAMPLE_%'", connection);
    var count = (int)verifyCommand.ExecuteScalar();
    Console.WriteLine($"Sample deals created: {count}");

    Console.WriteLine("Database seeding completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}
