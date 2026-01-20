using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class CylinderControl : UserControl
{
    public enum CylinderState
    {
        Unknown,
        Retracted,
        Extended,
        Moving
    }

    private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush ActiveBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush MovingBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    private static readonly Brush UnknownBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    public static readonly DependencyProperty CylinderNameProperty =
        DependencyProperty.Register(nameof(CylinderName), typeof(string), typeof(CylinderControl),
            new PropertyMetadata("Cylinder"));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(CylinderState), typeof(CylinderControl),
            new PropertyMetadata(CylinderState.Unknown, OnStateChanged));

    public static readonly DependencyProperty IsRetractedProperty =
        DependencyProperty.Register(nameof(IsRetracted), typeof(bool), typeof(CylinderControl),
            new PropertyMetadata(false, OnSensorChanged));

    public static readonly DependencyProperty IsExtendedProperty =
        DependencyProperty.Register(nameof(IsExtended), typeof(bool), typeof(CylinderControl),
            new PropertyMetadata(false, OnSensorChanged));

    public static readonly DependencyProperty ShowControlButtonsProperty =
        DependencyProperty.Register(nameof(ShowControlButtons), typeof(bool), typeof(CylinderControl),
            new PropertyMetadata(true, OnShowControlButtonsChanged));

    public static readonly DependencyProperty ExtendCommandProperty =
        DependencyProperty.Register(nameof(ExtendCommand), typeof(ICommand), typeof(CylinderControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty RetractCommandProperty =
        DependencyProperty.Register(nameof(RetractCommand), typeof(ICommand), typeof(CylinderControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(CylinderControl),
            new PropertyMetadata("UNKNOWN"));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(nameof(StatusBrush), typeof(Brush), typeof(CylinderControl),
            new PropertyMetadata(UnknownBrush));

    public static readonly DependencyProperty RetractedBrushProperty =
        DependencyProperty.Register(nameof(RetractedBrush), typeof(Brush), typeof(CylinderControl),
            new PropertyMetadata(InactiveBrush));

    public static readonly DependencyProperty ExtendedBrushProperty =
        DependencyProperty.Register(nameof(ExtendedBrush), typeof(Brush), typeof(CylinderControl),
            new PropertyMetadata(InactiveBrush));

    public static readonly DependencyProperty PistonWidthProperty =
        DependencyProperty.Register(nameof(PistonWidth), typeof(double), typeof(CylinderControl),
            new PropertyMetadata(20.0));

    public static readonly DependencyProperty ControlButtonsVisibilityProperty =
        DependencyProperty.Register(nameof(ControlButtonsVisibility), typeof(Visibility), typeof(CylinderControl),
            new PropertyMetadata(Visibility.Visible));

    public string CylinderName
    {
        get => (string)GetValue(CylinderNameProperty);
        set => SetValue(CylinderNameProperty, value);
    }

    public CylinderState State
    {
        get => (CylinderState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool IsRetracted
    {
        get => (bool)GetValue(IsRetractedProperty);
        set => SetValue(IsRetractedProperty, value);
    }

    public bool IsExtended
    {
        get => (bool)GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }

    public bool ShowControlButtons
    {
        get => (bool)GetValue(ShowControlButtonsProperty);
        set => SetValue(ShowControlButtonsProperty, value);
    }

    public ICommand? ExtendCommand
    {
        get => (ICommand?)GetValue(ExtendCommandProperty);
        set => SetValue(ExtendCommandProperty, value);
    }

    public ICommand? RetractCommand
    {
        get => (ICommand?)GetValue(RetractCommandProperty);
        set => SetValue(RetractCommandProperty, value);
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

    public Brush RetractedBrush
    {
        get => (Brush)GetValue(RetractedBrushProperty);
        set => SetValue(RetractedBrushProperty, value);
    }

    public Brush ExtendedBrush
    {
        get => (Brush)GetValue(ExtendedBrushProperty);
        set => SetValue(ExtendedBrushProperty, value);
    }

    public double PistonWidth
    {
        get => (double)GetValue(PistonWidthProperty);
        set => SetValue(PistonWidthProperty, value);
    }

    public Visibility ControlButtonsVisibility
    {
        get => (Visibility)GetValue(ControlButtonsVisibilityProperty);
        set => SetValue(ControlButtonsVisibilityProperty, value);
    }

    public CylinderControl()
    {
        InitializeComponent();
        SizeChanged += (s, e) => UpdatePistonWidth();
        UpdateStatus();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CylinderControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnSensorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CylinderControl control)
        {
            control.UpdateFromSensors();
        }
    }

    private static void OnShowControlButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CylinderControl control)
        {
            control.ControlButtonsVisibility = control.ShowControlButtons ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateFromSensors()
    {
        if (IsRetracted && !IsExtended)
        {
            State = CylinderState.Retracted;
        }
        else if (IsExtended && !IsRetracted)
        {
            State = CylinderState.Extended;
        }
        else if (!IsRetracted && !IsExtended)
        {
            State = CylinderState.Moving;
        }
        else
        {
            State = CylinderState.Unknown;
        }
    }

    private void UpdateStatus()
    {
        RetractedBrush = IsRetracted ? ActiveBrush : InactiveBrush;
        ExtendedBrush = IsExtended ? ActiveBrush : InactiveBrush;

        switch (State)
        {
            case CylinderState.Retracted:
                StatusText = "RETRACTED";
                StatusBrush = ActiveBrush;
                break;
            case CylinderState.Extended:
                StatusText = "EXTENDED";
                StatusBrush = ActiveBrush;
                break;
            case CylinderState.Moving:
                StatusText = "MOVING";
                StatusBrush = MovingBrush;
                break;
            default:
                StatusText = "UNKNOWN";
                StatusBrush = UnknownBrush;
                break;
        }

        UpdatePistonWidth();
    }

    private void UpdatePistonWidth()
    {
        var maxWidth = ActualWidth - 120; // Account for margins and status area
        if (maxWidth < 20) maxWidth = 100;

        PistonWidth = State switch
        {
            CylinderState.Extended => maxWidth,
            CylinderState.Retracted => 20,
            CylinderState.Moving => maxWidth / 2,
            _ => 20
        };
    }
}
