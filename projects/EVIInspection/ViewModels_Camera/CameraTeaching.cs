using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EVIInspection.Camera;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Controls;
using NAutoSuite.UI.Controls.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows;
using System.Windows.Media.Imaging;


namespace EVIInspection.Camera
{
    public partial class CameraTeaching : ObservableObject
    {
        private readonly Mat _roiMat;
        [ObservableProperty] private double _thresholdValue = 127;
        [ObservableProperty] private int _blurValue = 5;
        [ObservableProperty] private WriteableBitmap _previewImage;

        public CameraTeaching(Mat roiMat)
        {
            _roiMat = roiMat.Clone();
            UpdatePreview();
        }

        partial void OnThresholdValueChanged(double value) => UpdatePreview();
        partial void OnBlurValueChanged(int value) => UpdatePreview();

        private void UpdatePreview()
        {
            if (_roiMat == null || _roiMat.Empty()) return;
            try
            {
                int kSize = (BlurValue % 2 == 0) ? BlurValue + 1 : BlurValue;
                if (kSize < 1) kSize = 1;

                using var gray = _roiMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                using var blurred = gray.GaussianBlur(new OpenCvSharp.Size(kSize, kSize), 0);
                using var thresh = blurred.Threshold(ThresholdValue, 255, ThresholdTypes.Binary);
                
                PreviewImage = thresh.ToWriteableBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            
        }
    }
}
