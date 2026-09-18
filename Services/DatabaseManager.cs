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

            var createSettingsCmd = connection.CreateCommand();
            createSettingsCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS AppSettings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )";
            createSettingsCmd.ExecuteNonQuery();
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

        public double Get24HourUptimePercentage(int deviceId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cutoff = DateTime.UtcNow.AddHours(-24);
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT State, Timestamp 
                FROM NetworkEvents 
                WHERE DeviceId = $id AND Timestamp >= $cutoff 
                ORDER BY Timestamp ASC";
            cmd.Parameters.AddWithValue("$id", deviceId);
            cmd.Parameters.AddWithValue("$cutoff", cutoff);

            using var reader = cmd.ExecuteReader();
            var events = new List<(string State, DateTime Timestamp)>();
            while (reader.Read())
            {
                events.Add((reader.GetString(0), reader.GetDateTime(1)));
            }

            if (events.Count == 0) return 100.0; // Default 100% if no outage logs exist

            double offlineMinutes = 0;
            DateTime windowStart = cutoff;
            DateTime windowEnd = DateTime.UtcNow;

            for (int i = 0; i < events.Count; i++)
            {
                var current = events[i];
                if (current.State == "Offline" || current.State == "Degraded")
                {
                    DateTime start = current.Timestamp < windowStart ? windowStart : current.Timestamp;
                    DateTime end = (i + 1 < events.Count) ? events[i + 1].Timestamp : windowEnd;
                    offlineMinutes += (end - start).TotalMinutes;
                }
            }

            double totalMinutes = 1440.0; // 24 Hours
            double uptimePercent = ((totalMinutes - offlineMinutes) / totalMinutes) * 100.0;
            return Math.Max(0.0, Math.Min(100.0, uptimePercent));
        }

        public List<NetworkEventRecord> GetAllEventsForExport()
        {
            var records = new List<NetworkEventRecord>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT e.Id, d.Name, d.IpAddress, e.State, e.Timestamp, e.AverageLatency
                FROM NetworkEvents e
                LEFT JOIN Devices d ON e.DeviceId = d.Id
                ORDER BY e.Timestamp DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                records.Add(new NetworkEventRecord
                {
                    Id = reader.GetInt32(0),
                    DeviceName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                    IpAddress = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                    State = reader.GetString(3),
                    Timestamp = reader.GetDateTime(4),
                    AverageLatency = reader.GetDouble(5)
                });
            }

            return records;
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Value FROM AppSettings WHERE Key = $key";
            cmd.Parameters.AddWithValue("$key", key);

            var result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : defaultValue;
        }

        public void SaveSetting(string key, string value)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO AppSettings (Key, Value) VALUES ($key, $val)
                ON CONFLICT(Key) DO UPDATE SET Value = $val";
            cmd.Parameters.AddWithValue("$key", key);
            cmd.Parameters.AddWithValue("$val", value ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

    }

    public class NetworkEventRecord
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string State { get; set; }
        public DateTime Timestamp { get; set; }
        public double AverageLatency { get; set; }
    }
}
