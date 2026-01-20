using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class ValveControl : UserControl
{
    private static readonly Brush OpenBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush ClosedBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush FlowActiveBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush FlowInactiveBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42));

    public static readonly DependencyProperty ValveNameProperty =
        DependencyProperty.Register(nameof(ValveName), typeof(string), typeof(ValveControl),
            new PropertyMetadata("Valve"));

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ValveControl),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty ShowToggleProperty =
        DependencyProperty.Register(nameof(ShowToggle), typeof(bool), typeof(ValveControl),
            new PropertyMetadata(true, OnShowToggleChanged));

    public static readonly DependencyProperty ToggleCommandProperty =
        DependencyProperty.Register(nameof(ToggleCommand), typeof(ICommand), typeof(ValveControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OpenTextProperty =
        DependencyProperty.Register(nameof(OpenText), typeof(string), typeof(ValveControl),
            new PropertyMetadata("OPEN"));

    public static readonly DependencyProperty ClosedTextProperty =
        DependencyProperty.Register(nameof(ClosedText), typeof(string), typeof(ValveControl),
            new PropertyMetadata("CLOSED"));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ValveControl),
            new PropertyMetadata("CLOSED"));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(nameof(StatusBrush), typeof(Brush), typeof(ValveControl),
            new PropertyMetadata(ClosedBrush));

    public static readonly DependencyProperty ValveBrushProperty =
        DependencyProperty.Register(nameof(ValveBrush), typeof(Brush), typeof(ValveControl),
            new PropertyMetadata(ClosedBrush));

    public static readonly DependencyProperty FlowBrushProperty =
        DependencyProperty.Register(nameof(FlowBrush), typeof(Brush), typeof(ValveControl),
            new PropertyMetadata(FlowInactiveBrush));

    public static readonly DependencyProperty ToggleVisibilityProperty =
        DependencyProperty.Register(nameof(ToggleVisibility), typeof(Visibility), typeof(ValveControl),
            new PropertyMetadata(Visibility.Visible));

    public string ValveName
    {
        get => (string)GetValue(ValveNameProperty);
        set => SetValue(ValveNameProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool ShowToggle
    {
        get => (bool)GetValue(ShowToggleProperty);
        set => SetValue(ShowToggleProperty, value);
    }

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public string OpenText
    {
        get => (string)GetValue(OpenTextProperty);
        set => SetValue(OpenTextProperty, value);
    }

    public string ClosedText
    {
        get => (string)GetValue(ClosedTextProperty);
        set => SetValue(ClosedTextProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    public Brush ValveBrush
    {
        get => (Brush)GetValue(ValveBrushProperty);
        set => SetValue(ValveBrushProperty, value);
    }

    public Brush FlowBrush
    {
        get => (Brush)GetValue(FlowBrushProperty);
        set => SetValue(FlowBrushProperty, value);
    }

    public Visibility ToggleVisibility
    {
        get => (Visibility)GetValue(ToggleVisibilityProperty);
        set => SetValue(ToggleVisibilityProperty, value);
    }

    public ValveControl()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValveControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnShowToggleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValveControl control)
        {
            control.ToggleVisibility = control.ShowToggle ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateStatus()
    {
        if (IsOpen)
        {
            StatusText = OpenText;
            StatusBrush = OpenBrush;
            ValveBrush = OpenBrush;
            FlowBrush = FlowActiveBrush;
        }
        else
        {
            StatusText = ClosedText;
            StatusBrush = ClosedBrush;
            ValveBrush = ClosedBrush;
            FlowBrush = FlowInactiveBrush;
        }
    }
}
