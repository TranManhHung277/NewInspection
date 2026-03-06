using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EVIInspection.Camera;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Controls;
using NAutoSuite.UI.Controls.Dialogs;
using OpenCvSharp;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EVIInspection.ViewModels
{
    public partial class TeachingViewModel : ObservableObject
    {
        [ObservableProperty] private double _thresholdValue = 127;
        [ObservableProperty] private int _blurValue = 5;
        [ObservableProperty] private WriteableBitmap _previewImage;

        // Gọi hàm này mỗi khi Slider thay đổi (Sử dụng OnThresholdValueChanged)
        partial void OnThresholdValueChanged(double value) => UpdatePreview();
        partial void OnBlurValueChanged(int value) => UpdatePreview();

        private void UpdatePreview()
        {
            using var gray = _rawTemplate.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var blurred = gray.GaussianBlur(new Size(BlurValue, BlurValue), 0);
            using var thresh = blurred.Threshold(ThresholdValue, 255, ThresholdTypes.Binary);

            PreviewImage = thresh.ToWriteableBitmap();
        }
    }
}
