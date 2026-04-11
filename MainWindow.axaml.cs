using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Threading;

namespace AvaloniaApplication1
{
    public partial class MainWindow : Window
    {
        private string DbPath => Path.Combine(AppContext.BaseDirectory, "Data", "Database1b.db");
        private string FlipperInputPath = @"C:\FlipperData\FlipperInput.txt";

        private InputScript _inputScript;

        private DispatcherTimer _refreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            _inputScript = new InputScript(DbPath, FlipperInputPath);
            LoadAllEvents();
            SetupAutoRefresh(); // <-- start the timer
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            _refreshTimer.Tick += (s, e) =>
            {
                _inputScript.Process();  // <-- THIS IS REQUIRED
                LoadAllEvents();
            };

            _refreshTimer.Start();
        }

        private void LoadAllEvents()
        {
            var grid = this.FindControl<DataGrid>("EventsGrid");

            if (!File.Exists(DbPath))
            {
                var placeholder = new List<EventModel>
                {
                    new EventModel { Details = $"Database not found:\n{DbPath}" }
                };
                grid.ItemsSource = placeholder;
                return;
            }

            try
            {
                var events = new List<EventModel>();

                using var connection = new SqliteConnection($"Data Source={DbPath}");
                connection.Open();

                string query = @"
                    SELECT 
                        e.EventsID,
                        e.TimeStamp,
                        e.Source,
                        e.Location,
                        et.EventName,
                        et.EventResult,
                        e.DeviceID,
                        e.Details,
                        e.EmpID
                    FROM Events e
                    LEFT JOIN EventType et 
                        ON e.EventTypeID = et.EventTypeID
                    ORDER BY e.TimeStamp DESC;
                 ";

                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    events.Add(new EventModel
                    {
                        EventID = reader.GetInt32(0),
                        TimeStamp = reader.GetString(1),
                        Source = reader.GetString(2),
                        Location = reader.GetString(3),
                        EventName = reader.GetString(4),      // NEW
                        EventResult = reader.GetString(5),    // NEW
                        DeviceID = reader.GetString(6),
                        Details = reader.GetString(7),
                        EmpID = reader.GetInt32(8)
                    });
                }

                grid.ItemsSource = events;
            }
            catch (Exception ex)
            {
                var errorList = new List<EventModel>
                {
                    new EventModel { Details = $"Error loading database:\n{ex.Message}" }
                };
                grid.ItemsSource = errorList;
            }
        }

        private void GoToUserControl1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Content = new UserControl1(this);
        }

        private void GoToUserControl2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Content = new UserControl2(this);
        }

        public void ShowMainView()
        {
            InitializeComponent();
            LoadAllEvents();
        }
    }
}
