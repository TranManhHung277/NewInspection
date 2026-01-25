using System;
using System.Windows;
using System.Windows.Controls;

namespace NAutoSuite.UI.Controls.Dialogs;

public partial class NumericKeyboardDialog : Window
{
    public static string? TitleOkButton { get; set; }
    public static string? TitleCancelButton { get; set; }
    public static string? TitleClearButton { get; set; }
    public static string? TitleBackButton { get; set; }

    private string _value;
    private readonly bool _allowDecimal;
    private readonly bool _allowNegative;

    public NumericKeyboardDialog(string title, string initialValue, bool allowDecimal, bool allowNegative)
    {
        InitializeComponent();
        ApplyButtonTextOverrides();
        TitleText.Text = title;
        _allowDecimal = allowDecimal;
        _allowNegative = allowNegative;
        _value = initialValue ?? string.Empty;
        UpdateDisplay();
    }

    public string ResultText => _value;

    public static string? Show(Window? owner, string? initialValue, bool allowDecimal, bool allowNegative)
    {
        var dialog = new NumericKeyboardDialog("Numeric Input", initialValue ?? string.Empty, allowDecimal, allowNegative)
        {
            Owner = owner
        };

        return dialog.ShowDialog() == true ? dialog.ResultText : null;
    }

    private void KeyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        AppendKey(key);
    }

    private void AppendKey(string key)
    {
        if (key == ".")
        {
            if (!_allowDecimal || _value.Contains('.'))
            {
                return;
            }

            _value = string.IsNullOrEmpty(_value) ? "0." : _value + ".";
            UpdateDisplay();
            return;
        }

        if (key == "-")
        {
            if (!_allowNegative)
            {
                return;
            }

            if (string.IsNullOrEmpty(_value))
            {
                _value = "-";
            }
            else if (_value.StartsWith("-", StringComparison.Ordinal))
            {
                _value = _value[1..];
            }
            else
            {
                _value = "-" + _value;
            }

            UpdateDisplay();
            return;
        }

        _value += key;
        UpdateDisplay();
    }

    private void BackspaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_value))
        {
            return;
        }

        _value = _value[..^1];
        UpdateDisplay();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _value = string.Empty;
        UpdateDisplay();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void UpdateDisplay()
    {
        DisplayBox.Text = _value;
        DisplayBox.CaretIndex = DisplayBox.Text.Length;
    }

    private void ApplyButtonTextOverrides()
    {
        OkButton.Content = GetText("OK", TitleOkButton);
        CancelButton.Content = GetText("Cancel", TitleCancelButton);
        ClearButton.Content = GetText("Clear", TitleClearButton);
        BackButton.Content = GetText("Back", TitleBackButton);
    }

    private static string GetText(string defaultText, string? overrideText)
    {
        return string.IsNullOrWhiteSpace(overrideText) ? defaultText : overrideText;
    }
}
