using System;
using System.Collections.Generic;
using System.Windows;

namespace NAutoSuite.UI.Controls.Dialogs;

public class ErrorInfo
{
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "MEDIUM"; // "HIGH", "MEDIUM", "LOW"
}

public partial class ErrorListDialog : Window
{
    public static string? TitleClearButton { get; set; }
    public static string? TitleCloseButton { get; set; }

    public ErrorListDialog()
    {
        InitializeComponent();
        ApplyButtonTextOverrides();
    }

    /// <summary>
    /// Show error list dialog with multiple errors
    /// </summary>
    public static void ShowErrors(List<ErrorInfo> errors)
    {
        var dialog = new ErrorListDialog();
        dialog.ErrorList.ItemsSource = errors;
        dialog.ErrorCountText.Text = $"{errors.Count} error(s) found";
        dialog.ShowDialog();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorList.ItemsSource = null;
        ErrorCountText.Text = "0 errors found";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ApplyButtonTextOverrides()
    {
        ClearButton.Content = GetText("Clear All", TitleClearButton);
        CloseButton.Content = GetText("Close", TitleCloseButton);
    }

    private static string GetText(string defaultText, string? overrideText)
    {
        return string.IsNullOrWhiteSpace(overrideText) ? defaultText : overrideText;
    }
}
