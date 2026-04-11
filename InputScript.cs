using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace AvaloniaApplication1
{
    public class InputScript
    {
        private readonly string _dbPath;
        private readonly string _filePath;
        private int _lastProcessedLine = 0;

        public InputScript(string dbPath, string filePath)
        {
            _dbPath = dbPath;
            _filePath = filePath;
        }

        public void Process()
        {
            System.Diagnostics.Debug.WriteLine("PROCESS CALLED");

            if (!File.Exists(_filePath))
            {
                System.Diagnostics.Debug.WriteLine($"FILE NOT FOUND: {_filePath}");
                return;
            }

            var lines = File.ReadAllLines(_filePath);
            System.Diagnostics.Debug.WriteLine("LINES: " + lines.Length);

            if (_lastProcessedLine > lines.Length)
                _lastProcessedLine = 0;

            if (_lastProcessedLine >= lines.Length)
                return;

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            for (int i = _lastProcessedLine; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping empty line {i}");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"Processing line {i}: {line}");

                ProcessLine(line, connection);
            }

            _lastProcessedLine = lines.Length;
        }

        private void ProcessLine(string line, SqliteConnection connection)
        {
            string source = "";
            string location = "";
            string evt = "";
            string act = "";

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (part.StartsWith("SOURCE="))
                    source = part.Replace("SOURCE=", "");

                else if (part.StartsWith("LOC="))
                    location = part.Replace("LOC=", "");

                else if (part.StartsWith("EVT="))
                    evt = part.Replace("EVT=", "");

                else if (part.StartsWith("ACT="))
                    act = part.Replace("ACT=", "").Trim();
            }





            if (!int.TryParse(act, out int actValue))
            {
                System.Diagnostics.Debug.WriteLine($"ACT PARSE FAILED: '{act}'");
                return;
            }

            int eventTypeId = GetEventTypeId(evt, connection);
            System.Diagnostics.Debug.WriteLine($"EVENT TYPE ID = {eventTypeId} (EVT = '{evt}')");

            if (eventTypeId == 0)
            {
                System.Diagnostics.Debug.WriteLine($"EVENT TYPE NOT FOUND: {evt}");
                return;
            }

            string result = GetEventResult(eventTypeId, connection);

            System.Diagnostics.Debug.WriteLine($"Mapped -> EventTypeID:{eventTypeId}, Result:{result}");

            const string query = @"
                INSERT INTO Events 
                (TimeStamp, Source, Location, EventTypeID, Result, DeviceID, Details, EmpID)
                VALUES 
                (@TimeStamp, @Source, @Location, @EventTypeID, @Result, @DeviceID, @Details, @EmpID);";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@Source", source);
            command.Parameters.AddWithValue("@Location", location);
            command.Parameters.AddWithValue("@EventTypeID", eventTypeId);
            command.Parameters.AddWithValue("@Result", result);
            command.Parameters.AddWithValue("@DeviceID", actValue);
            command.Parameters.AddWithValue("@Details", "N/A");
            command.Parameters.AddWithValue("@EmpID", actValue);

            int rows = command.ExecuteNonQuery();

            System.Diagnostics.Debug.WriteLine($"INSERT RESULT: {rows} row(s) added");
        }

        private int GetEventTypeId(string evt, SqliteConnection connection)
        {
            string query = "SELECT EventTypeID FROM EventType WHERE EventName = @name LIMIT 1;";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", evt);

            var result = cmd.ExecuteScalar();

            return result != null ? Convert.ToInt32(result) : 0;
        }

        private string GetEventResult(int eventTypeId, SqliteConnection connection)
        {
            string query = "SELECT EventResult FROM EventType WHERE EventTypeID = @id LIMIT 1;";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", eventTypeId);

            var result = cmd.ExecuteScalar();

            return result?.ToString() ?? "Unknown";
        }
    }
}
