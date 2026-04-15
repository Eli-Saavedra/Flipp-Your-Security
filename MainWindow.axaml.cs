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
        private string FlipperInputPath = Path.Combine(AppContext.BaseDirectory, "Data", "FlipperInput.txt");
        private InputScript _inputScript;
        private DispatcherTimer _refreshTimer;

        private TextBlock? _detailsPanel;
        // Stable backing list (IMPORTANT FIX)
        private List<EventModel> _eventsCache = new();

        public MainWindow()
        {
            InitializeComponent();

            _inputScript = new InputScript(DbPath, FlipperInputPath);

            var grid = this.FindControl<DataGrid>("EventsGrid");

            // ADD THIS
            _detailsPanel = this.FindControl<TextBlock>("DetailsPanel");

            // Bind ONCE only
            grid.ItemsSource = _eventsCache;

            // ADD THIS
            grid.SelectionChanged += Grid_SelectionChanged;

            LoadAllEvents();
            SetupAutoRefresh();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // =========================
        // TIMER (SAFE)
        // =========================
        private void SetupAutoRefresh()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            _refreshTimer.Tick += (s, e) =>
            {
                _inputScript.Process();

                // IMPORTANT: force UI update safely
                Dispatcher.UIThread.Post(() =>
                {
                    LoadAllEvents();
                });
            };

            _refreshTimer.Start();
        }

        // =========================
        // LOAD DATA (FIXED)
        // =========================
        private void LoadAllEvents()
        {
            var grid = this.FindControl<DataGrid>("EventsGrid");

            if (grid == null)
                return;

            if (!File.Exists(DbPath))
            {
                _eventsCache = new List<EventModel>
                {
                    new EventModel
                    {
                        Details = $"Database not found:\n{DbPath}"
                    }
                };

                ApplyToGrid(grid);
                return;
            }

            var temp = new List<EventModel>();

            try
            {
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
                    temp.Add(new EventModel
                    {
                        EventID = reader.GetInt32(0),
                        TimeStamp = reader.GetString(1),
                        Source = reader.GetString(2),
                        Location = reader.GetString(3),
                        EventName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        EventResult = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        DeviceID = reader.GetString(6),
                        Details = reader.GetString(7),
                        EmpID = reader.GetInt32(8)
                    });
                }

                _eventsCache = temp;

                ApplyToGrid(grid);

                this.Title = $"Loaded {_eventsCache.Count} events";
            }
            catch (Exception ex)
            {
                _eventsCache = new List<EventModel>
                {
                    new EventModel
                    {
                        Details = $"Error loading database:\n{ex.Message}"
                    }
                };

                ApplyToGrid(grid);
            }
        }

        private void Grid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid?.SelectedItem is not EventModel selected)
                return;

            if (_detailsPanel == null)
                return;

            _detailsPanel.Text =
                $"Time: {selected.TimeStamp}\n" +
                $"Source: {selected.Source}\n" +
                $"Location: {selected.Location}\n" +
                $"Event: {selected.EventName}\n" +
                $"Result: {selected.EventResult}\n\n" +
                $"{selected.Details}";
        }

        // =========================
        // FORCE SAFE UI UPDATE
        // =========================
        private void ApplyToGrid(DataGrid grid)
        {
            Dispatcher.UIThread.Post(() =>
            {
                grid.ItemsSource = null;
                grid.ItemsSource = _eventsCache;
            });
        }

        // =========================
        // NAVIGATION
        // =========================
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
