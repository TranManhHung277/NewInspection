using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NAutoSuite.Core.Configuration;
using NAutoSuite.UI.Controls.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace EVIInspection.Camera
{

    public partial class ImageViewModel : ObservableObject // 1. Đảm bảo có partial
    {
        [ObservableProperty]
        private ImageSource? _myImage;
        [ObservableProperty] 
        private double _roiX, _roiY, _roiWidth, _roiHeight;
        [ObservableProperty] 
        private bool _isSelecting;

        private System.Windows.Point _startPoint;
        private Mat? _originalMat;
        private Mat? _templateMat;

        [RelayCommand]
        private void ActionOpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                using var mat = Cv2.ImRead(openFileDialog.FileName);
                if (!mat.Empty())
                {
                    // 2. Gán trực tiếp cho Property (viết hoa), bỏ .Source
                    // Sử dụng ToWriteableBitmap hoặc ToBitmapSource từ OpenCvSharp.WpfExtensions
                    this.MyImage = mat.ToWriteableBitmap();
                }
            }
        }
        [RelayCommand]
        private void OnMouseDown(MouseButtonEventArgs e)
        {
            var canvas = e.Source as Canvas;
            _startPoint = e.GetPosition(canvas);
            IsSelecting = true;
            RoiWidth = RoiHeight = 0;
        }
        [RelayCommand]
        private void OnMouseMove(MouseEventArgs e)
        {
            if (!IsSelecting) return;
            var currentPoint = e.GetPosition(e.Source as Canvas);

            // Ràng buộc không cho lấn ra ngoài vùng Canvas (giả định Canvas khớp kích thước ảnh hiển thị)
            double canvasWidth = (e.Source as Canvas).ActualWidth;
            double canvasHeight = (e.Source as Canvas).ActualHeight;

            var endX = Math.Clamp(currentPoint.X, 0, canvasWidth);
            var endY = Math.Clamp(currentPoint.Y, 0, canvasHeight);

            RoiX = Math.Min(_startPoint.X, endX);
            RoiY = Math.Min(_startPoint.Y, endY);
            RoiWidth = Math.Abs(_startPoint.X - endX);
            RoiHeight = Math.Abs(_startPoint.Y - endY);
        }
        [RelayCommand]
        private void OnMouseUp() => IsSelecting = false;

        // --- Xử lý OpenCV ---
        [RelayCommand]
        private void Teaching()
        {
            if (_originalMat == null || RoiWidth <= 0) return;

            // Tính tỉ lệ giữa Pixel thật và UI để cắt ảnh chính xác
            double ratioX = _originalMat.Width / 600.0; // 600 là Width của Grid/Canvas ở XAML
            double ratioY = _originalMat.Height / 450.0;

            OpenCvSharp.Rect rect = new((int)(RoiX * ratioX), (int)(RoiY * ratioY),
                                        (int)(RoiWidth * ratioX), (int)(RoiHeight * ratioY));

            _templateMat = new Mat(_originalMat, rect).Clone();
            ModernMessageBox.Show("Teaching Successful!");
        }
        [RelayCommand]
        private void Process()
        {
            if (_originalMat == null || _templateMat == null) return;

            using var res = new Mat();
            Cv2.MatchTemplate(_originalMat, _templateMat, res, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(res, out _, out double maxVal, out _, out var maxLoc);

            if (maxVal > 0.8)
            {
                // Tọa độ tâm (Pixel thực)
                int centerX = maxLoc.X + (_templateMat.Width / 2);
                int centerY = maxLoc.Y + (_templateMat.Height / 2);

                // Vẽ kết quả lên UI
                using var debugMat = _originalMat.Clone();
                debugMat.Rectangle(new OpenCvSharp.Rect(maxLoc, _templateMat.Size()), Scalar.Red, 3);
                MyImage = debugMat.ToWriteableBitmap();

                ModernMessageBox.Show($"Found! Center: {centerX},{centerY} Score: {maxVal:F2}");
            }
        }
    }
}
