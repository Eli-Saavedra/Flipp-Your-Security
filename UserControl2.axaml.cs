using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.IO;

namespace AvaloniaApplication1;

public partial class UserControl2 : UserControl
{
    private MainWindow _mainWindow;
    private DispatcherTimer _timer;

    private string FlipperInputPath = @"C:\FlipperData\FlipperInput.txt";

    public UserControl2(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

        LoadFileContents();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        LoadFileContents();
    }

    private void LoadFileContents()
    {
        var textBlock = this.FindControl<TextBlock>("FileContents");

        if (!File.Exists(FlipperInputPath))
        {
            textBlock.Text = $"File not found:\n{FlipperInputPath}";
            return;
        }

        try
        {
            textBlock.Text = File.ReadAllText(FlipperInputPath);
        }
        catch (Exception ex)
        {
            textBlock.Text = $"Error reading file:\n{ex.Message}";
        }
    }

    private void GoBack_Click(object? sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        _mainWindow.ShowMainView();
    }
}

