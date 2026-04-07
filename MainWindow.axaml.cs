using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
namespace AvaloniaApplication1
{
    public partial class MainWindow : Window
    {
        private string DbPath => Path.Combine(AppContext.BaseDirectory, "Data", "Database1b.db");
        private string FlipperInputPath => Path.Combine(AppContext.BaseDirectory, "Data", "FlipperInput.txt");
        public MainWindow()
        {
            InitializeComponent();
            LoadAllEvents();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void LoadAllEvents()
        {
            var grid = this.FindControl<DataGrid>("EventsGrid");

            if (!File.Exists(DbPath))
            {
                // Show a single row message if database is missing
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
                    SELECT EventsID, TimeStamp, Source, Location, EventTypeID, Result, DeviceID, Details, EmpID 
                    FROM Events 
                    ORDER BY TimeStamp DESC;";
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
                        EventTypeID = reader.GetInt32(4),
                        Result = reader.GetString(5),
                        DeviceID = reader.GetString(6),
                        Details = reader.GetString(7),
                        EmpID = reader.GetInt32(8)
                    });
                }
                this.Title = $"Loaded {events.Count} events"; // if this shows 0, DB is empty
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
