using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class StepIndicatorControl : UserControl
{
    private static readonly Brush CompletedBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush CurrentBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush PendingBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42));
    private static readonly Brush PendingBorderBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(StepIndicatorControl),
            new PropertyMetadata(string.Empty, OnTitleChanged));

    public static readonly DependencyProperty CurrentStepProperty =
        DependencyProperty.Register(nameof(CurrentStep), typeof(int), typeof(StepIndicatorControl),
            new PropertyMetadata(0, OnCurrentStepChanged));

    public static readonly DependencyProperty StepNamesProperty =
        DependencyProperty.Register(nameof(StepNames), typeof(string[]), typeof(StepIndicatorControl),
            new PropertyMetadata(null, OnStepNamesChanged));

    public static readonly DependencyProperty StepsProperty =
        DependencyProperty.Register(nameof(Steps), typeof(ObservableCollection<StepItem>), typeof(StepIndicatorControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TitleVisibilityProperty =
        DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(StepIndicatorControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty ProgressTextProperty =
        DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(StepIndicatorControl),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public string[]? StepNames
    {
        get => (string[]?)GetValue(StepNamesProperty);
        set => SetValue(StepNamesProperty, value);
    }

    public ObservableCollection<StepItem>? Steps
    {
        get => (ObservableCollection<StepItem>?)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public Visibility TitleVisibility
    {
        get => (Visibility)GetValue(TitleVisibilityProperty);
        set => SetValue(TitleVisibilityProperty, value);
    }

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        set => SetValue(ProgressTextProperty, value);
    }

    public StepIndicatorControl()
    {
        InitializeComponent();
        Steps = new ObservableCollection<StepItem>();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepIndicatorControl control)
        {
            control.TitleVisibility = string.IsNullOrEmpty(control.Title) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnCurrentStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepIndicatorControl control)
        {
            control.UpdateSteps();
        }
    }

    private static void OnStepNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepIndicatorControl control)
        {
            control.BuildSteps();
        }
    }

    private void BuildSteps()
    {
        Steps?.Clear();

        if (StepNames == null || StepNames.Length == 0)
            return;

        for (int i = 0; i < StepNames.Length; i++)
        {
            Steps?.Add(new StepItem
            {
                Index = i,
                Name = StepNames[i],
                IsFirst = i == 0,
                IsLast = i == StepNames.Length - 1
            });
        }

        UpdateSteps();
    }

    private void UpdateSteps()
    {
        if (Steps == null) return;

        int completedCount = 0;

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];

            if (i < CurrentStep)
            {
                // Completed
                step.State = StepState.Completed;
                step.BackgroundBrush = CompletedBrush;
                step.BorderBrush = CompletedBrush;
                step.ForegroundBrush = Brushes.White;
                step.DisplayText = "\uE73E"; // Checkmark
                step.FontFamily = new FontFamily("Segoe MDL2 Assets");
                step.NameForeground = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0));
                step.NameFontWeight = FontWeights.Normal;
                completedCount++;
            }
            else if (i == CurrentStep)
            {
                // Current
                step.State = StepState.Current;
                step.BackgroundBrush = CurrentBrush;
                step.BorderBrush = CurrentBrush;
                step.ForegroundBrush = Brushes.White;
                step.DisplayText = (i + 1).ToString();
                step.FontFamily = new FontFamily("Segoe UI");
                step.NameForeground = Brushes.White;
                step.NameFontWeight = FontWeights.SemiBold;
            }
            else
            {
                // Pending
                step.State = StepState.Pending;
                step.BackgroundBrush = PendingBrush;
                step.BorderBrush = PendingBorderBrush;
                step.ForegroundBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
                step.DisplayText = (i + 1).ToString();
                step.FontFamily = new FontFamily("Segoe UI");
                step.NameForeground = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
                step.NameFontWeight = FontWeights.Normal;
            }

            // Update connectors
            step.LeftConnectorBrush = i > 0 && i <= CurrentStep ? CompletedBrush : PendingBrush;
            step.RightConnectorBrush = i < CurrentStep ? CompletedBrush : PendingBrush;
            step.LeftConnectorVisibility = step.IsFirst ? Visibility.Hidden : Visibility.Visible;
            step.RightConnectorVisibility = step.IsLast ? Visibility.Hidden : Visibility.Visible;
        }

        ProgressText = $"{completedCount}/{Steps.Count}";
    }

    public class StepItem : INotifyPropertyChanged
    {
        private int _index;
        private string _name = string.Empty;
        private StepState _state;
        private Brush _backgroundBrush = PendingBrush;
        private Brush _borderBrush = PendingBorderBrush;
        private Brush _foregroundBrush = Brushes.Gray;
        private Brush _nameForeground = Brushes.Gray;
        private Brush _leftConnectorBrush = PendingBrush;
        private Brush _rightConnectorBrush = PendingBrush;
        private string _displayText = "1";
        private FontFamily _fontFamily = new("Segoe UI");
        private FontWeight _nameFontWeight = FontWeights.Normal;
        private Visibility _leftConnectorVisibility = Visibility.Visible;
        private Visibility _rightConnectorVisibility = Visibility.Visible;
        private bool _isFirst;
        private bool _isLast;

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public StepState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set { _backgroundBrush = value; OnPropertyChanged(); }
        }

        public Brush BorderBrush
        {
            get => _borderBrush;
            set { _borderBrush = value; OnPropertyChanged(); }
        }

        public Brush ForegroundBrush
        {
            get => _foregroundBrush;
            set { _foregroundBrush = value; OnPropertyChanged(); }
        }

        public Brush NameForeground
        {
            get => _nameForeground;
            set { _nameForeground = value; OnPropertyChanged(); }
        }

        public Brush LeftConnectorBrush
        {
            get => _leftConnectorBrush;
            set { _leftConnectorBrush = value; OnPropertyChanged(); }
        }

        public Brush RightConnectorBrush
        {
            get => _rightConnectorBrush;
            set { _rightConnectorBrush = value; OnPropertyChanged(); }
        }

        public string DisplayText
        {
            get => _displayText;
            set { _displayText = value; OnPropertyChanged(); }
        }

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(); }
        }

        public FontWeight NameFontWeight
        {
            get => _nameFontWeight;
            set { _nameFontWeight = value; OnPropertyChanged(); }
        }

        public Visibility LeftConnectorVisibility
        {
            get => _leftConnectorVisibility;
            set { _leftConnectorVisibility = value; OnPropertyChanged(); }
        }

        public Visibility RightConnectorVisibility
        {
            get => _rightConnectorVisibility;
            set { _rightConnectorVisibility = value; OnPropertyChanged(); }
        }

        public bool IsFirst
        {
            get => _isFirst;
            set { _isFirst = value; OnPropertyChanged(); }
        }

        public bool IsLast
        {
            get => _isLast;
            set { _isLast = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum StepState
    {
        Pending,
        Current,
        Completed
    }
}
