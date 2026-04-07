using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace AvaloniaApplication1;

public partial class UserControl1 : UserControl
{
    
    private TextBox? _sourceBox, _locationBox, _eventTypeIDBox, _resultBox, _deviceIDBox, _detailsBox, _empIDBox;

    private string DbPath => Path.Combine(AppContext.BaseDirectory, "Data", "Database1b.db");

    private readonly MainWindow _mainWindow;

    public UserControl1(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

        CacheControls();
        AttachEventHandlers();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void CacheControls()
    {
        _sourceBox = this.FindControl<TextBox>("SourceBox");
        _locationBox = this.FindControl<TextBox>("LocationBox");
        _eventTypeIDBox = this.FindControl<TextBox>("EventTypeIDBox");
        _resultBox = this.FindControl<TextBox>("ResultBox");
        _deviceIDBox = this.FindControl<TextBox>("DeviceIDBox");
        _detailsBox = this.FindControl<TextBox>("DetailsBox");
        _empIDBox = this.FindControl<TextBox>("EmpIDBox");
    }

    private void AttachEventHandlers()
    {
        if (this.FindControl<Button>("SubmitButton") is Button submit)
            submit.Click += SubmitButton_Click;

        if (this.FindControl<Button>("BackButton") is Button back)
            back.Click += BackButton_Click;
    }

    private void SubmitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!TryParseInputs(out int eventTypeID, out int empID)) return;

        if (!File.Exists(DbPath))
        {
            ShowMessage($"Database file not found:\n{DbPath}");
            return;
        }

        try
        {
            InsertEvent(eventTypeID, empID);
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}");
        }
    }

    private bool TryParseInputs(out int eventTypeID, out int empID)
    {
        if (!int.TryParse(_eventTypeIDBox?.Text, out eventTypeID))
        {
            ShowMessage("EventTypeID must be a number.");
            empID = 0;
            return false;
        }
        if (!int.TryParse(_empIDBox?.Text, out empID))
        {
            ShowMessage("EmpID must be a number.");
            return false;
        }
        return true;
    }

    private void InsertEvent(int eventTypeID, int empID)
    {
        const string query = @"
            INSERT INTO Events (TimeStamp, Source, Location, EventTypeID, Result, DeviceID, Details, EmpID)
            VALUES (@TimeStamp, @Source, @Location, @EventTypeID, @Result, @DeviceID, @Details, @EmpID);";

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@Source", _sourceBox?.Text ?? string.Empty);
        command.Parameters.AddWithValue("@Location", _locationBox?.Text ?? string.Empty);
        command.Parameters.AddWithValue("@EventTypeID", eventTypeID);
        command.Parameters.AddWithValue("@Result", _resultBox?.Text ?? string.Empty);
        command.Parameters.AddWithValue("@DeviceID", _deviceIDBox?.Text ?? string.Empty);
        command.Parameters.AddWithValue("@Details", _detailsBox?.Text ?? string.Empty);
        command.Parameters.AddWithValue("@EmpID", empID);

        bool success = command.ExecuteNonQuery() > 0;
        ShowMessage(success ? "Event successfully added!" : "Failed to add event.");
    }

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _mainWindow.ShowMainView();
    }

    private void ShowMessage(string message)
    {
        var dlg = new Window
        {
            Width = 400,
            Height = 150,
            Title = "Info",
            Content = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(20),
                TextWrapping = TextWrapping.Wrap
            }
        };

        if (this.VisualRoot is Window owner)
            dlg.ShowDialog(owner);
    }
}