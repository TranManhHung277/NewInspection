using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls.Dialogs;

public partial class TextKeyboardDialog : Window
{
    public static string? TitleOkButton { get; set; }
    public static string? TitleCancelButton { get; set; }
    public static string? TitleClearButton { get; set; }
    public static string? TitleBackButton { get; set; }
    public static string? TitleSpaceButton { get; set; }
    public static string? TitleCapsButton { get; set; }

    private string _value;
    private bool _isCaps = true;
    private bool _suppressTextChange;

    public TextKeyboardDialog(string title, string initialValue)
    {
        InitializeComponent();
        ApplyButtonTextOverrides();
        TitleText.Text = title;
        _value = initialValue ?? string.Empty;
        UpdateDisplay();
        UpdateLetterKeys();
        Loaded += OnLoaded;
    }

    public string ResultText => _value;

    public static string? Show(Window? owner, string? initialValue)
    {
        var dialog = new TextKeyboardDialog("Text Input", initialValue ?? string.Empty)
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
        if (key.Length == 1 && char.IsLetter(key[0]))
        {
            _value += _isCaps ? key.ToUpperInvariant() : key.ToLowerInvariant();
        }
        else
        {
            _value += key;
        }

        UpdateDisplay();
    }

    private void SpaceButton_Click(object sender, RoutedEventArgs e)
    {
        _value += " ";
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

    private void CapsButton_Click(object sender, RoutedEventArgs e)
    {
        _isCaps = !_isCaps;
        UpdateLetterKeys();
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
        _suppressTextChange = true;
        DisplayBox.Text = _value;
        DisplayBox.CaretIndex = DisplayBox.Text.Length;
        _suppressTextChange = false;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DisplayBox.Focus();
        DisplayBox.Select(DisplayBox.Text.Length, 0);
    }

    private void DisplayBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChange)
        {
            return;
        }

        _value = DisplayBox.Text;
    }

    private void UpdateLetterKeys()
    {
        foreach (var button in FindLetterButtons(KeyboardPanel))
        {
            if (button.Tag is not string key || key.Length != 1 || !char.IsLetter(key[0]))
            {
                continue;
            }

            button.Content = _isCaps ? key.ToUpperInvariant() : key.ToLowerInvariant();
        }
    }

    private void ApplyButtonTextOverrides()
    {
        OkButton.Content = GetText("OK", TitleOkButton);
        CancelButton.Content = GetText("Cancel", TitleCancelButton);
        ClearButton.Content = GetText("Clear", TitleClearButton);
        BackButton.Content = GetText("Back", TitleBackButton);
        SpaceButton.Content = GetText("Space", TitleSpaceButton);
        CapsButton.Content = GetText("Caps", TitleCapsButton);
    }

    private static string GetText(string defaultText, string? overrideText)
    {
        return string.IsNullOrWhiteSpace(overrideText) ? defaultText : overrideText;
    }

    private static IEnumerable<Button> FindLetterButtons(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Button button)
            {
                yield return button;
            }

            foreach (var nested in FindLetterButtons(child))
            {
                yield return nested;
            }
        }
    }
}
