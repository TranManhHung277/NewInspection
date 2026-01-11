using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class IOIndicator : UserControl
{
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(IOIndicator),
            new PropertyMetadata(false, OnIsActiveChanged));

    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(IOIndicator),
            new PropertyMetadata("IO", OnLabelTextChanged));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public IOIndicator()
    {
        InitializeComponent();
        UpdateIndicator();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IOIndicator control)
        {
            control.UpdateIndicator();
        }
    }

    private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IOIndicator control)
        {
            control.Label.Text = e.NewValue?.ToString() ?? "IO";
        }
    }

    private void UpdateIndicator()
    {
        Indicator.Fill = IsActive
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) // Green
            : new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray
    }
}
