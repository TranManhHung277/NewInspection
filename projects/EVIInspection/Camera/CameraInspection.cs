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
        [ObservableProperty]
        private double _currentCanvasWidth;
        [ObservableProperty]
        private double _currentCanvasHeight;
        public IRelayCommand OnMouseDownCommand { get; }
        public IRelayCommand OnMouseMoveCommand { get; }
        public IRelayCommand OnMouseUpCommand { get; }


        private System.Windows.Point _startPoint;
        private Mat? _originalMat;
        private Mat? _templateMat;
        private bool _isDrawing = false;
        public ImageViewModel()
        {
            // 2. Khởi tạo trong Constructor
            OnMouseDownCommand = new RelayCommand<object>(OnMouseDown);
            OnMouseMoveCommand = new RelayCommand<object>(OnMouseMove);
            OnMouseUpCommand = new RelayCommand<object>(OnMouseUp);
            // Tương tự cho MouseMove và MouseUp nếu chúng cũng bị lỗi
        }
       
        [RelayCommand]
        private void ActionOpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                _originalMat = Cv2.ImRead(openFileDialog.FileName);
                if (!_originalMat.Empty() && !_originalMat.Empty())
                {
                    // 2. Gán trực tiếp cho Property (viết hoa), bỏ .Source
                    // Sử dụng ToWriteableBitmap hoặc ToBitmapSource từ OpenCvSharp.WpfExtensions
                    this.MyImage = _originalMat.ToWriteableBitmap();
                }
            }
        }
    
        private void OnMouseDown(object? e)
        {
            if (e is System.Windows.Input.MouseEventArgs args)
            {
               // ModernMessageBox.Show("Mouse Down Triggered!");

                // Dùng OriginalSource hoặc Source tùy vào cấu trúc UI
                var canvas = args.Source as System.Windows.Controls.Canvas;
                if (canvas == null) return;

                if (!_isDrawing)
                {
                    // CLICK LẦN 1: Bắt đầu vẽ
                    _startPoint = args.GetPosition(canvas);
                    RoiX = _startPoint.X;
                    RoiY = _startPoint.Y;
                    RoiWidth = 0;
                    RoiHeight = 0;
                    CurrentCanvasWidth = canvas.ActualWidth;
                    CurrentCanvasHeight = canvas.ActualHeight;
                    IsSelecting = true;
                    _isDrawing = true;

                    // Chiếm quyền chuột để MouseMove mượt mà
                  
                }
                else
                {
                    // CLICK LẦN 2: Kết thúc vẽ
                    _isDrawing = false;
                    IsSelecting = false; // Vẫn giữ true để Rectangle không bị ẩn đi ngay lập tức

                    var canvasCapture = System.Windows.Input.Mouse.Captured as Canvas;
                    canvasCapture?.ReleaseMouseCapture();

                    ModernMessageBox.Show("Đã chọn xong vùng ROI. Nhấn TEACHING để xác nhận.");
                }
            }
        }

        private void OnMouseMove(object? e)
        {
            
            if (!IsSelecting || e is not System.Windows.Input.MouseEventArgs args) return;
            //var canvas = args.Source as System.Windows.Controls.Canvas;
            var canvas = args.Source as Canvas;
            if (canvas == null) return;
            var currentPoint = args.GetPosition(canvas);

            // Ràng buộc không cho lấn ra ngoài vùng Canvas (giả định Canvas khớp kích thước ảnh hiển thị)
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            var endX = Math.Clamp(currentPoint.X, 0, canvasWidth);
            var endY = Math.Clamp(currentPoint.Y, 0, canvasHeight);

            RoiX = Math.Min(_startPoint.X, endX);
            RoiY = Math.Min(_startPoint.Y, endY);
            RoiWidth = Math.Abs(_startPoint.X - endX);
            RoiHeight = Math.Abs(_startPoint.Y - endY);
        }

        private void OnMouseUp(object? e)
        {
            //if (e is MouseEventArgs args)
            //{
            //    var canvas = args.Source as Canvas;

            //    // GIẢI PHÓNG CHUỘT: Khi buông tay phải trả lại quyền cho hệ thống
            //    canvas?.ReleaseMouseCapture();
            //}
            //IsSelecting = false;
        }

        // --- Xử lý OpenCV ---
        [RelayCommand]
        private void Teaching()
        {

            if (_originalMat == null || RoiWidth <= 0) return;


            // Tính tỉ lệ giữa Pixel thật và UI để cắt ảnh chính xác
            double ratioX = _originalMat.Width / CurrentCanvasWidth;
            double ratioY = _originalMat.Height / CurrentCanvasHeight;

            OpenCvSharp.Rect rect = new((int)(RoiX * ratioX), (int)(RoiY * ratioY),
                                        (int)(RoiWidth * ratioX), (int)(RoiHeight * ratioY));
            rect.X = Math.Clamp(rect.X, 0, _originalMat.Width - 1);
            rect.Y = Math.Clamp(rect.Y, 0, _originalMat.Height - 1);
            rect.Width = Math.Clamp(rect.Width, 1, _originalMat.Width - rect.X);
            rect.Height = Math.Clamp(rect.Height, 1, _originalMat.Height - rect.Y);
            _templateMat = new Mat(_originalMat, rect).Clone();
            Cv2.ImShow("Template Check", _templateMat);

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
