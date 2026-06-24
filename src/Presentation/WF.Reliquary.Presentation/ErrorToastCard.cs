using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

/// <summary>
/// A dismissible, stacking error toast card with an "Open log" control.
/// Rendered as a code-only control; hosted in the <see cref="StackPanel"/> overlay in MainWindow.
/// </summary>
internal sealed class ErrorToastCard : Border
{
    private readonly IFileLauncher fileLauncher;
    private readonly string logFilePath;

    public ErrorToastCard(string message, string logFilePath, IFileLauncher fileLauncher)
    {
        this.logFilePath = logFilePath;
        this.fileLauncher = fileLauncher;

        Background = new SolidColorBrush(Color.Parse("#FEE2E2"));
        BorderBrush = new SolidColorBrush(Color.Parse("#DC2626"));
        BorderThickness = new Thickness(0, 0, 0, 3);
        CornerRadius = new CornerRadius(4);
        Padding = new Thickness(12, 10);
        MaxWidth = 360;
        Margin = new Thickness(0, 4, 0, 0);

        Child = BuildContent(message);
    }

    private Grid BuildContent(string message)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 8, 8),
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetRow(messageText, 0);
        Grid.SetColumn(messageText, 0);

        var closeBtn = new Button
        {
            Content = "✕",
            Padding = new Thickness(6, 2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };
        closeBtn.Click += (_, _) => (Parent as Panel)?.Children.Remove(this);
        Grid.SetRow(closeBtn, 0);
        Grid.SetColumn(closeBtn, 1);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };
        Grid.SetRow(actions, 1);
        Grid.SetColumn(actions, 0);
        Grid.SetColumnSpan(actions, 2);

        var openBtn = new Button { Content = "Open log", Padding = new Thickness(8, 4) };
        openBtn.Click += (_, _) =>
        {
            try { fileLauncher.Open(logFilePath); }
            catch { }
        };

        actions.Children.Add(openBtn);

        grid.Children.Add(messageText);
        grid.Children.Add(closeBtn);
        grid.Children.Add(actions);

        return grid;
    }
}
