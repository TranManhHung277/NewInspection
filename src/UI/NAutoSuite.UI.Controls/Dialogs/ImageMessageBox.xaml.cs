using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NAutoSuite.UI.Controls.Dialogs;

/// <summary>
/// Message box with image display for showing error locations, instructions, etc.
/// Image is automatically scaled to fit within fixed dimensions.
/// </summary>
public partial class ImageMessageBox : Window
{
    public enum MessageBoxType
    {
        Information,
        Warning,
        Error,
        Question,
        Success
    }

    public enum MessageBoxButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum ButtonStyle
    {
        Default,
        Primary,
        Success,
        Danger,
        Warning
    }

    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
    private ButtonStyle _primaryButtonStyle = ButtonStyle.Default;

    public static string? TitleOkButton { get; set; }
    public static string? TitleCancelButton { get; set; }
    public static string? TitleYesButton { get; set; }
    public static string? TitleNoButton { get; set; }

    private ImageMessageBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Show message box with image from file path
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="imagePath">Path to the image file</param>
    /// <param name="title">Dialog title</param>
    /// <param name="type">Icon type</param>
    /// <param name="buttons">Which buttons to show</param>
    /// <param name="primaryStyle">Style for the primary button</param>
    /// <param name="buttonTexts">Custom button texts. Keys: "OK", "Cancel", "Yes", "No"</param>
    public static MessageBoxResult Show(
        string message,
        string imagePath,
        string title = "Message",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        ButtonStyle primaryStyle = ButtonStyle.Default,
        Dictionary<string, string>? buttonTexts = null)
    {
        var dialog = new ImageMessageBox();
        dialog._primaryButtonStyle = primaryStyle;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        // Load image from file
        if (File.Exists(imagePath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                dialog.ContentImage.Source = bitmap;
            }
            catch
            {
                dialog.ImageContainer.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            dialog.ImageContainer.Visibility = Visibility.Collapsed;
        }

        SetupDialog(dialog, type, buttons, buttonTexts);
        dialog.ShowDialog();
        return dialog.Result;
    }

    /// <summary>
    /// Show message box with image from ImageSource (BitmapImage, etc.)
    /// </summary>
    /// <param name="buttonTexts">Custom button texts. Keys: "OK", "Cancel", "Yes", "No"</param>
    public static MessageBoxResult Show(
        string message,
        ImageSource imageSource,
        string title = "Message",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        ButtonStyle primaryStyle = ButtonStyle.Default,
        Dictionary<string, string>? buttonTexts = null)
    {
        var dialog = new ImageMessageBox();
        dialog._primaryButtonStyle = primaryStyle;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        if (imageSource != null)
        {
            dialog.ContentImage.Source = imageSource;
        }
        else
        {
            dialog.ImageContainer.Visibility = Visibility.Collapsed;
        }

        SetupDialog(dialog, type, buttons, buttonTexts);
        dialog.ShowDialog();
        return dialog.Result;
    }

    /// <summary>
    /// Show message box with image from byte array (useful for embedded resources)
    /// </summary>
    /// <param name="buttonTexts">Custom button texts. Keys: "OK", "Cancel", "Yes", "No"</param>
    public static MessageBoxResult Show(
        string message,
        byte[] imageData,
        string title = "Message",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        ButtonStyle primaryStyle = ButtonStyle.Default,
        Dictionary<string, string>? buttonTexts = null)
    {
        var dialog = new ImageMessageBox();
        dialog._primaryButtonStyle = primaryStyle;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        if (imageData != null && imageData.Length > 0)
        {
            try
            {
                var bitmap = new BitmapImage();
                using (var ms = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }
                dialog.ContentImage.Source = bitmap;
            }
            catch
            {
                dialog.ImageContainer.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            dialog.ImageContainer.Visibility = Visibility.Collapsed;
        }

        SetupDialog(dialog, type, buttons, buttonTexts);
        dialog.ShowDialog();
        return dialog.Result;
    }

    /// <summary>
    /// Show message box with image from URI (for resources or web images)
    /// </summary>
    /// <param name="buttonTexts">Custom button texts. Keys: "OK", "Cancel", "Yes", "No"</param>
    public static MessageBoxResult Show(
        string message,
        Uri imageUri,
        string title = "Message",
        MessageBoxType type = MessageBoxType.Information,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        ButtonStyle primaryStyle = ButtonStyle.Default,
        Dictionary<string, string>? buttonTexts = null)
    {
        var dialog = new ImageMessageBox();
        dialog._primaryButtonStyle = primaryStyle;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        if (imageUri != null)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                dialog.ContentImage.Source = bitmap;
            }
            catch
            {
                dialog.ImageContainer.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            dialog.ImageContainer.Visibility = Visibility.Collapsed;
        }

        SetupDialog(dialog, type, buttons, buttonTexts);
        dialog.ShowDialog();
        return dialog.Result;
    }

    private static void SetupDialog(ImageMessageBox dialog, MessageBoxType type, MessageBoxButtons buttons, Dictionary<string, string>? buttonTexts = null)
    {
        // Helper function to get button text
        string GetText(string defaultText)
        {
            if (buttonTexts != null && buttonTexts.TryGetValue(defaultText, out var custom) && !string.IsNullOrWhiteSpace(custom))
            {
                return custom;
            }

            string? overrideText = defaultText switch
            {
                "OK" => TitleOkButton,
                "Cancel" => TitleCancelButton,
                "Yes" => TitleYesButton,
                "No" => TitleNoButton,
                _ => null
            };

            return string.IsNullOrWhiteSpace(overrideText) ? defaultText : overrideText;
        }

        // Set icon and color based on type
        switch (type)
        {
            case MessageBoxType.Information:
                dialog.IconBorder.Background = new SolidColorBrush(Color.FromRgb(31, 111, 235));
                dialog.IconPath.Data = Geometry.Parse("M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                break;

            case MessageBoxType.Warning:
                dialog.IconBorder.Background = new SolidColorBrush(Color.FromRgb(219, 154, 4));
                dialog.IconPath.Data = Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z");
                break;

            case MessageBoxType.Error:
                dialog.IconBorder.Background = new SolidColorBrush(Color.FromRgb(218, 54, 51));
                dialog.IconPath.Data = Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z");
                break;

            case MessageBoxType.Question:
                dialog.IconBorder.Background = new SolidColorBrush(Color.FromRgb(88, 166, 255));
                dialog.IconPath.Data = Geometry.Parse("M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z");
                break;

            case MessageBoxType.Success:
                dialog.IconBorder.Background = new SolidColorBrush(Color.FromRgb(35, 134, 54));
                dialog.IconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z");
                break;
        }

        // Add buttons
        switch (buttons)
        {
            case MessageBoxButtons.OK:
                dialog.AddButton(GetText("OK"), MessageBoxResult.OK, true);
                break;

            case MessageBoxButtons.OKCancel:
                dialog.AddButton(GetText("Cancel"), MessageBoxResult.Cancel, false);
                dialog.AddButton(GetText("OK"), MessageBoxResult.OK, true);
                break;

            case MessageBoxButtons.YesNo:
                dialog.AddButton(GetText("No"), MessageBoxResult.No, false);
                dialog.AddButton(GetText("Yes"), MessageBoxResult.Yes, true);
                break;

            case MessageBoxButtons.YesNoCancel:
                dialog.AddButton(GetText("Cancel"), MessageBoxResult.Cancel, false);
                dialog.AddButton(GetText("No"), MessageBoxResult.No, false);
                dialog.AddButton(GetText("Yes"), MessageBoxResult.Yes, true);
                break;
        }
    }

    private void AddButton(string text, MessageBoxResult result, bool isPrimary)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 90,
            Height = 36,
            Margin = new Thickness(10, 0, 0, 0),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        if (isPrimary)
        {
            var (normal, hover, pressed) = GetPrimaryButtonColors();
            button.Style = CreateButtonStyle(normal, hover, pressed);
        }
        else
        {
            button.Style = CreateButtonStyle(
                Color.FromRgb(55, 62, 71),
                Color.FromRgb(68, 76, 86),
                Color.FromRgb(45, 51, 59));
        }

        button.Click += (s, e) =>
        {
            Result = result;
            Close();
        };

        ButtonPanel.Children.Add(button);
    }

    private (Color normal, Color hover, Color pressed) GetPrimaryButtonColors()
    {
        return _primaryButtonStyle switch
        {
            ButtonStyle.Danger => (
                Color.FromRgb(218, 54, 51),
                Color.FromRgb(240, 70, 67),
                Color.FromRgb(182, 35, 36)),

            ButtonStyle.Success => (
                Color.FromRgb(35, 134, 54),
                Color.FromRgb(46, 160, 67),
                Color.FromRgb(28, 110, 44)),

            ButtonStyle.Warning => (
                Color.FromRgb(219, 154, 4),
                Color.FromRgb(240, 175, 25),
                Color.FromRgb(180, 125, 0)),

            _ => (
                Color.FromRgb(31, 111, 235),
                Color.FromRgb(48, 126, 245),
                Color.FromRgb(22, 93, 207))
        };
    }

    private Style CreateButtonStyle(Color normal, Color hover, Color pressed)
    {
        var style = new Style(typeof(Button));

        var template = new ControlTemplate(typeof(Button));
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.Name = "border";
        factory.SetValue(Border.BackgroundProperty, new SolidColorBrush(normal));
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        factory.SetValue(Border.PaddingProperty, new Thickness(16, 0, 16, 0));

        var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
        contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        factory.AppendChild(contentFactory);

        template.VisualTree = factory;

        var hoverTrigger = new Trigger { Property = IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hover), "border"));

        var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
        pressedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(pressed), "border"));

        template.Triggers.Add(hoverTrigger);
        template.Triggers.Add(pressedTrigger);

        style.Setters.Add(new Setter(TemplateProperty, template));
        style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));

        return style;
    }
}
