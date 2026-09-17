using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANMonitor.Services
{
    public class DatabaseManager
    {
        private readonly string _connectionString = "Data Source=NetworkMonitor.db";

        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createDevicesCmd = connection.CreateCommand();
            createDevicesCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Devices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IpAddress TEXT NOT NULL,
                    Name TEXT NOT NULL
                )";
            createDevicesCmd.ExecuteNonQuery();

            var createLogsCmd = connection.CreateCommand();
            createLogsCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS NetworkEvents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceId INTEGER,
                    State TEXT,
                    Timestamp DATETIME,
                    AverageLatency REAL
                )";
            createLogsCmd.ExecuteNonQuery();
        }

        public void LogEvent(int deviceId, string state, double averageLatency)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO NetworkEvents (DeviceId, State, Timestamp, AverageLatency) VALUES ($id, $state, $time, $latency)";
            cmd.Parameters.AddWithValue("$id", deviceId);
            cmd.Parameters.AddWithValue("$state", state);
            cmd.Parameters.AddWithValue("$time", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("$latency", averageLatency);
            cmd.ExecuteNonQuery();
        }

        public int AddDevice(string name, string ipAddress)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Devices (Name, IpAddress) VALUES ($name, $ip);
            SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$ip", ipAddress);

            long newId = (long)cmd.ExecuteScalar();
            return (int)newId;
        }


        public void DeleteDevice(int deviceId)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Devices WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", deviceId);
            cmd.ExecuteNonQuery();
        }
    }
}
