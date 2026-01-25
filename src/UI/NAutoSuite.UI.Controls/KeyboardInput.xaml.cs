using System.Windows;
using System.Windows.Controls;
using NAutoSuite.UI.Controls.Dialogs;

namespace NAutoSuite.UI.Controls;

public partial class KeyboardInput : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(KeyboardInput),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PendingTextProperty =
        DependencyProperty.Register(nameof(PendingText), typeof(string), typeof(KeyboardInput),
            new PropertyMetadata(string.Empty, OnPendingTextChanged));

    public static readonly DependencyProperty KeyboardTypeProperty =
        DependencyProperty.Register(nameof(KeyboardType), typeof(KeyboardInputType), typeof(KeyboardInput),
            new PropertyMetadata(KeyboardInputType.Text));

    public static readonly DependencyProperty AutoShowOnFocusProperty =
        DependencyProperty.Register(nameof(AutoShowOnFocus), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowKeyboardButtonProperty =
        DependencyProperty.Register(nameof(ShowKeyboardButton), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AllowDecimalProperty =
        DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AllowNegativeProperty =
        DependencyProperty.Register(nameof(AllowNegative), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CommitOnEnterProperty =
        DependencyProperty.Register(nameof(CommitOnEnter), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true, OnCommitOnEnterChanged));

    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(KeyboardInput),
            new PropertyMetadata(null));

    private bool _isDialogOpen;
    private bool _suppressNextFocus;
    private bool _isSyncing;

    public KeyboardInput()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PendingText
    {
        get => (string)GetValue(PendingTextProperty);
        set => SetValue(PendingTextProperty, value);
    }

    public KeyboardInputType KeyboardType
    {
        get => (KeyboardInputType)GetValue(KeyboardTypeProperty);
        set => SetValue(KeyboardTypeProperty, value);
    }

    public bool AutoShowOnFocus
    {
        get => (bool)GetValue(AutoShowOnFocusProperty);
        set => SetValue(AutoShowOnFocusProperty, value);
    }

    public bool ShowKeyboardButton
    {
        get => (bool)GetValue(ShowKeyboardButtonProperty);
        set => SetValue(ShowKeyboardButtonProperty, value);
    }

    public bool AllowDecimal
    {
        get => (bool)GetValue(AllowDecimalProperty);
        set => SetValue(AllowDecimalProperty, value);
    }

    public bool AllowNegative
    {
        get => (bool)GetValue(AllowNegativeProperty);
        set => SetValue(AllowNegativeProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool CommitOnEnter
    {
        get => (bool)GetValue(CommitOnEnterProperty);
        set => SetValue(CommitOnEnterProperty, value);
    }

    public Style? TextBoxStyle
    {
        get => (Style?)GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control || control._isSyncing)
        {
            return;
        }

        control._isSyncing = true;
        control.PendingText = e.NewValue?.ToString() ?? string.Empty;
        control._isSyncing = false;
    }

    private static void OnPendingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control || control._isSyncing)
        {
            return;
        }

        if (!control.CommitOnEnter)
        {
            control._isSyncing = true;
            control.Text = e.NewValue?.ToString() ?? string.Empty;
            control._isSyncing = false;
        }
    }

    private static void OnCommitOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control)
        {
            return;
        }

        if (!(bool)e.NewValue)
        {
            control._isSyncing = true;
            control.Text = control.PendingText;
            control._isSyncing = false;
        }
        else
        {
            control._isSyncing = true;
            control.PendingText = control.Text;
            control._isSyncing = false;
        }
    }

    private void InputBox_GotKeyboardFocus(object sender, RoutedEventArgs e)
    {
        if (_suppressNextFocus)
        {
            _suppressNextFocus = false;
            return;
        }

        if (!AutoShowOnFocus || KeyboardType != KeyboardInputType.Text || IsReadOnly || _isDialogOpen)
        {
            return;
        }

        OpenKeyboard();
    }

    private void InputBox_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!CommitOnEnter || e.Key != System.Windows.Input.Key.Enter)
        {
            return;
        }

        Text = PendingText;
        e.Handled = true;
    }

    private void KeyboardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDialogOpen)
        {
            return;
        }

        OpenKeyboard();
    }

    private void OpenKeyboard()
    {
        _isDialogOpen = true;

        try
        {
            var owner = Window.GetWindow(this);
            string? result = KeyboardType == KeyboardInputType.Numeric
                ? NumericKeyboardDialog.Show(owner, Text, AllowDecimal, AllowNegative)
                : TextKeyboardDialog.Show(owner, Text);

            if (result != null)
            {
                PendingText = result;

                if (CommitOnEnter)
                {
                    Text = result;
                }
            }
        }
        finally
        {
            _isDialogOpen = false;
            _suppressNextFocus = true;
        }
    }
}

public enum KeyboardInputType
{
    Text,
    Numeric
}
