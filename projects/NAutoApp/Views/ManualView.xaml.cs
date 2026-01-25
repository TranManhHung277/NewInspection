using System.Windows;
using System.Windows.Controls;
using NAutoSuite.UI.Controls.Dialogs;

namespace NAutoApp.Views;

public partial class ManualView : UserControl
{
    public ManualView()
    {
        InitializeComponent();
    }

    private void TestImageDialogButton_Click(object sender, RoutedEventArgs e)
    {
        // Cách 1: Sử dụng embedded resource (pack URI)
        // Assembly name là "NAutoApp.Frontend" (tên file .csproj)
        var testImageUri = new Uri("pack://application:,,,/NAutoApp;component/Assets/error.jpg", UriKind.Absolute);
        ImageMessageBox.TitleOkButton = "Đồng ý";
        ImageMessageBox.Show(
            message: "Phát hiện lỗi sensor tại vị trí Pick.\n\n" +
                     "Hướng dẫn khắc phục:\n" +
                     "1. Kiểm tra kết nối dây sensor\n" +
                     "2. Kiểm tra nguồn 24V\n" +
                     "3. Thay thế sensor nếu cần",
            imageUri: testImageUri,
            title: "Lỗi Sensor Pick Position",
            type: ImageMessageBox.MessageBoxType.Error,
            buttons: ImageMessageBox.MessageBoxButtons.OK,
            primaryStyle: ImageMessageBox.ButtonStyle.Danger);
    }
}

