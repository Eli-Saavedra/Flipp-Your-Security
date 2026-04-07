using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;

namespace AvaloniaApplication1;

public partial class UserControl2 : UserControl
{
    private MainWindow _mainWindow;
    private string FlipperInputPath => Path.Combine(AppContext.BaseDirectory, "Data", "FlipperInput.txt");

    public UserControl2(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

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
        _mainWindow.ShowMainView();
    }
}