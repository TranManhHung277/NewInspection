using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NAutoSuite.UI.Controls;

public enum IconPosition
{
    Left,
    Right,
    Top,
    Bottom
}

public partial class ActionButton : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(ActionButton),
            new PropertyMetadata(string.Empty, OnTextRelatedChanged));

    public static readonly DependencyProperty CheckedTextProperty =
        DependencyProperty.Register(nameof(CheckedText), typeof(string), typeof(ActionButton),
            new PropertyMetadata(string.Empty, OnTextRelatedChanged));

    public static readonly DependencyProperty UseCheckedTextProperty =
        DependencyProperty.Register(nameof(UseCheckedText), typeof(bool), typeof(ActionButton),
            new PropertyMetadata(true, OnTextRelatedChanged));

    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayText), typeof(string), typeof(ActionButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(ActionButton),
            new PropertyMetadata(null, OnIconChanged));

    public static readonly DependencyProperty IconPositionProperty =
        DependencyProperty.Register(nameof(IconPosition), typeof(IconPosition), typeof(ActionButton),
            new PropertyMetadata(IconPosition.Left, OnLayoutChanged));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(ActionButton),
            new PropertyMetadata(20.0));

    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(ActionButton),
            new PropertyMetadata(new Thickness(0, 0, 8, 0)));

    public static readonly DependencyProperty TextMarginProperty =
        DependencyProperty.Register(nameof(TextMargin), typeof(Thickness), typeof(ActionButton),
            new PropertyMetadata(new Thickness(0)));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(nameof(ShowIcon), typeof(Visibility), typeof(ActionButton),
            new PropertyMetadata(Visibility.Visible, OnShowIconChanged));

    public static readonly DependencyProperty ShowTextProperty =
        DependencyProperty.Register(nameof(ShowText), typeof(Visibility), typeof(ActionButton),
            new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty ContentPaddingProperty =
        DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(ActionButton),
            new PropertyMetadata(new Thickness(12, 6, 12, 6)));

    public static readonly DependencyProperty ContentHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(ContentHorizontalAlignment), typeof(HorizontalAlignment), typeof(ActionButton),
            new PropertyMetadata(HorizontalAlignment.Center));

    public static readonly DependencyProperty ContentVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(ContentVerticalAlignment), typeof(VerticalAlignment), typeof(ActionButton),
            new PropertyMetadata(VerticalAlignment.Center));

    public static readonly DependencyProperty NormalBackgroundProperty =
        DependencyProperty.Register(nameof(NormalBackground), typeof(Brush), typeof(ActionButton),
            new PropertyMetadata(Brushes.DimGray, OnVisualChanged));

    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(ActionButton),
            new PropertyMetadata(Brushes.Gray, OnVisualChanged));

    public static readonly DependencyProperty PressedBackgroundProperty =
        DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(ActionButton),
            new PropertyMetadata(Brushes.DarkGray, OnVisualChanged));

    public static readonly DependencyProperty BlinkBackgroundProperty =
        DependencyProperty.Register(nameof(BlinkBackground), typeof(Brush), typeof(ActionButton),
            new PropertyMetadata(Brushes.Orange, OnVisualChanged));

    public static readonly DependencyProperty IsToggleProperty =
        DependencyProperty.Register(nameof(IsToggle), typeof(bool), typeof(ActionButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ActionButton),
            new PropertyMetadata(false, OnCheckedChanged));

    public static readonly DependencyProperty CheckedBackgroundProperty =
        DependencyProperty.Register(nameof(CheckedBackground), typeof(Brush), typeof(ActionButton),
            new PropertyMetadata(Brushes.SteelBlue, OnVisualChanged));

    public static readonly DependencyProperty IsBlinkingProperty =
        DependencyProperty.Register(nameof(IsBlinking), typeof(bool), typeof(ActionButton),
            new PropertyMetadata(false, OnBlinkChanged));

    public static readonly DependencyProperty BlinkIntervalMsProperty =
        DependencyProperty.Register(nameof(BlinkIntervalMs), typeof(int), typeof(ActionButton),
            new PropertyMetadata(500, OnBlinkChanged));

    public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HoldCommandProperty =
        DependencyProperty.Register(nameof(HoldCommand), typeof(ICommand), typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HoldCompletedCommandProperty =
        DependencyProperty.Register(nameof(HoldCompletedCommand), typeof(ICommand), typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HoldDelayMsProperty =
        DependencyProperty.Register(nameof(HoldDelayMs), typeof(int), typeof(ActionButton),
            new PropertyMetadata(600));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ActionButton),
            new PropertyMetadata(new CornerRadius(6)));

    public static readonly DependencyProperty DisabledOpacityProperty =
        DependencyProperty.Register(nameof(DisabledOpacity), typeof(double), typeof(ActionButton),
            new PropertyMetadata(0.5, OnVisualChanged));

    public event RoutedEventHandler? Clicked;
    public event RoutedEventHandler? HoldStarted;
    public event RoutedEventHandler? HoldCompleted;

    private readonly DispatcherTimer _blinkTimer;
    private readonly DispatcherTimer _holdTimer;
    private bool _blinkOn;
    private bool _isPressed;
    private bool _isHolding;

    public ActionButton()
    {
        InitializeComponent();

        _blinkTimer = new DispatcherTimer();
        _blinkTimer.Tick += (_, _) =>
        {
            _blinkOn = !_blinkOn;
            UpdateVisualState();
        };

        _holdTimer = new DispatcherTimer();
        _holdTimer.Tick += (_, _) =>
        {
            _holdTimer.Stop();
            if (_isPressed && !_isHolding)
            {
                _isHolding = true;
                RaiseHoldStarted();
            }
        };

        Loaded += (_, _) => UpdateLayoutForIcon();
        Loaded += (_, _) => UpdateIconVisibility();
        Loaded += (_, _) => UpdateDisplayText();

        MouseEnter += (_, _) => UpdateVisualState();
        MouseLeave += (_, _) =>
        {
            _isPressed = false;
            UpdateVisualState();
        };
        PreviewMouseLeftButtonDown += OnMouseDown;
        PreviewMouseLeftButtonUp += OnMouseUp;
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string CheckedText
    {
        get => (string)GetValue(CheckedTextProperty);
        set => SetValue(CheckedTextProperty, value);
    }

    public bool UseCheckedText
    {
        get => (bool)GetValue(UseCheckedTextProperty);
        set => SetValue(UseCheckedTextProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextPropertyKey, value);
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }

    public Visibility ShowIcon
    {
        get => (Visibility)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public Visibility ShowText
    {
        get => (Visibility)GetValue(ShowTextProperty);
        set => SetValue(ShowTextProperty, value);
    }

    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public HorizontalAlignment ContentHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty);
        set => SetValue(ContentHorizontalAlignmentProperty, value);
    }

    public VerticalAlignment ContentVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty);
        set => SetValue(ContentVerticalAlignmentProperty, value);
    }

    public Brush NormalBackground
    {
        get => (Brush)GetValue(NormalBackgroundProperty);
        set => SetValue(NormalBackgroundProperty, value);
    }

    public Brush HoverBackground
    {
        get => (Brush)GetValue(HoverBackgroundProperty);
        set => SetValue(HoverBackgroundProperty, value);
    }

    public Brush PressedBackground
    {
        get => (Brush)GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }

    public Brush BlinkBackground
    {
        get => (Brush)GetValue(BlinkBackgroundProperty);
        set => SetValue(BlinkBackgroundProperty, value);
    }

    public bool IsToggle
    {
        get => (bool)GetValue(IsToggleProperty);
        set => SetValue(IsToggleProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public Brush CheckedBackground
    {
        get => (Brush)GetValue(CheckedBackgroundProperty);
        set => SetValue(CheckedBackgroundProperty, value);
    }

    public bool IsBlinking
    {
        get => (bool)GetValue(IsBlinkingProperty);
        set => SetValue(IsBlinkingProperty, value);
    }

    public int BlinkIntervalMs
    {
        get => (int)GetValue(BlinkIntervalMsProperty);
        set => SetValue(BlinkIntervalMsProperty, value);
    }

    public ICommand? ClickCommand
    {
        get => (ICommand?)GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public ICommand? HoldCommand
    {
        get => (ICommand?)GetValue(HoldCommandProperty);
        set => SetValue(HoldCommandProperty, value);
    }

    public ICommand? HoldCompletedCommand
    {
        get => (ICommand?)GetValue(HoldCompletedCommandProperty);
        set => SetValue(HoldCompletedCommandProperty, value);
    }

    public int HoldDelayMs
    {
        get => (int)GetValue(HoldDelayMsProperty);
        set => SetValue(HoldDelayMsProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double DisabledOpacity
    {
        get => (double)GetValue(DisabledOpacityProperty);
        set => SetValue(DisabledOpacityProperty, value);
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateIconVisibility();
        }
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateLayoutForIcon();
        }
    }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateVisualState();
        }
    }

    private static void OnBlinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateBlink();
        }
    }

    private static void OnCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateDisplayText();
            control.UpdateVisualState();
        }
    }

    private static void OnTextRelatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateDisplayText();
        }
    }

    private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButton control)
        {
            control.UpdateIconVisibility();
        }
    }

    private void UpdateLayoutForIcon()
    {
        if (IconImage == null)
        {
            return;
        }

        DockPanel.SetDock(IconImage, IconPosition switch
        {
            IconPosition.Top => Dock.Top,
            IconPosition.Bottom => Dock.Bottom,
            IconPosition.Right => Dock.Right,
            _ => Dock.Left
        });
    }

    private void UpdateIconVisibility()
    {
        if (IconImage == null)
        {
            return;
        }

        // Hide the icon placeholder when no icon is provided.
        IconImage.Visibility = Icon == null ? Visibility.Collapsed : ShowIcon;
    }

    private void UpdateDisplayText()
    {
        if (IsToggle && IsChecked && UseCheckedText && !string.IsNullOrWhiteSpace(CheckedText))
        {
            DisplayText = CheckedText;
            return;
        }

        DisplayText = Text;
    }

    private void UpdateBlink()
    {
        if (IsBlinking)
        {
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(100, BlinkIntervalMs));
            _blinkTimer.Start();
        }
        else
        {
            _blinkTimer.Stop();
            _blinkOn = false;
        }

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (!IsEnabled)
        {
            RootBorder.Opacity = DisabledOpacity;
            RootBorder.Background = NormalBackground;
            return;
        }

        RootBorder.Opacity = 1;

        if (IsBlinking && _blinkOn)
        {
            RootBorder.Background = BlinkBackground;
            return;
        }

        if (IsToggle && IsChecked)
        {
            RootBorder.Background = CheckedBackground;
            return;
        }

        if (_isPressed)
        {
            RootBorder.Background = PressedBackground;
            return;
        }

        if (IsMouseOver)
        {
            RootBorder.Background = HoverBackground;
            return;
        }

        RootBorder.Background = NormalBackground;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        _isPressed = true;
        _isHolding = false;
        _holdTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(200, HoldDelayMs));
        _holdTimer.Start();
        UpdateVisualState();
        CaptureMouse();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        _holdTimer.Stop();
        if (_isPressed && !_isHolding)
        {
            RaiseClicked();
        }
        else if (_isHolding)
        {
            RaiseHoldCompleted();
        }

        _isPressed = false;
        _isHolding = false;
        UpdateVisualState();
        ReleaseMouseCapture();
    }

    private void RaiseClicked()
    {
        if (IsToggle)
        {
            IsChecked = !IsChecked;
        }

        Clicked?.Invoke(this, new RoutedEventArgs());
        if (ClickCommand?.CanExecute(null) == true)
        {
            ClickCommand.Execute(null);
        }
    }

    private void RaiseHoldStarted()
    {
        HoldStarted?.Invoke(this, new RoutedEventArgs());
        if (HoldCommand?.CanExecute(null) == true)
        {
            HoldCommand.Execute(null);
        }
    }

    private void RaiseHoldCompleted()
    {
        HoldCompleted?.Invoke(this, new RoutedEventArgs());
        if (HoldCompletedCommand?.CanExecute(null) == true)
        {
            HoldCompletedCommand.Execute(null);
        }
    }
}
