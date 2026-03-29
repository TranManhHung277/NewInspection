using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EVIInspection.Views;
using Microsoft.Win32;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Configuration;
using NAutoSuite.UI.Controls.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EVIInspection.Services;
using System.Windows;
using Point = OpenCvSharp.Point;


//ver 23032026
//ver 26032026
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
        [ObservableProperty]
        private string _resultX = "0";

        [ObservableProperty]
        private string _resultY = "0";

        [ObservableProperty]
        private string _resultR = "0";

        [ObservableProperty]
        private string _resultScore = "0";

        [ObservableProperty]
        private CameraTeaching _teachingVM;
        public IRelayCommand OnMouseDownCommand { get; }
        public IRelayCommand OnMouseMoveCommand { get; }
        public IRelayCommand OnMouseUpCommand { get; }
        public ObservableCollection<List_Object> ListResults { get; set; } = new ObservableCollection<List_Object>();

        private readonly IVisionService _visionService;
       


        private System.Windows.Point _startPoint;
        private Mat? _originalMat;
        private Mat? _templateMat;
        private Mat? _grayMat;
        private bool _isDrawing = false;
        private double finalThresh;
        private int finalBlur;
        private float templateAngle, currentAngle, finalAngle;
       
        public ImageViewModel(IVisionService visionService)
        {
            _visionService = visionService ?? throw new ArgumentNullException(nameof(visionService));
            OnMouseDownCommand = new RelayCommand<object>(OnMouseDown);
            OnMouseMoveCommand = new RelayCommand<object>(OnMouseMove);
            OnMouseUpCommand = new RelayCommand<object>(OnMouseUp);

            if (_visionService == null)
            {
                MessageBox.Show("DI thất bại: VisionService truyền vào bị null!");
            }
            else
            {
                // Nếu hiện cái này khi chạy app là DI đã thành công
                System.Diagnostics.Debug.WriteLine("DI thành công!");
            }
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


                    // hiển thị ngay vùng đã chọn
                    if (_originalMat == null || RoiWidth <= 0) return;
                    double ratioX = _originalMat.Width / CurrentCanvasWidth;
                    double ratioY = _originalMat.Height / CurrentCanvasHeight;
                    OpenCvSharp.Rect rect = new((int)(RoiX * ratioX), (int)(RoiY * ratioY),
                                       (int)(RoiWidth * ratioX), (int)(RoiHeight * ratioY));
                    rect.X = Math.Clamp(rect.X, 0, _originalMat.Width - 1);
                    rect.Y = Math.Clamp(rect.Y, 0, _originalMat.Height - 1);
                    rect.Width = Math.Clamp(rect.Width, 1, _originalMat.Width - rect.X);
                    rect.Height = Math.Clamp(rect.Height, 1, _originalMat.Height - rect.Y);
                    _templateMat = new Mat(_originalMat, rect).Clone();

                    // 2. Mở cửa sổ phụ và truyền roiMat sang
                    TeachingVM = new CameraTeaching(_templateMat);

                    var teachingWin = new TeachingWindow { DataContext = TeachingVM };
                    teachingWin.Show(); // Dùng Show() để người dùng vừa chỉnh vừa bấm nút bên Main được


                    //var canvasCapture = System.Windows.Input.Mouse.Captured as Canvas;
                    //canvasCapture?.ReleaseMouseCapture();

                    //ModernMessageBox.Show("Đã chọn xong vùng ROI. Nhấn TEACHING để xác nhận.");
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

            //if (_originalMat == null || RoiWidth <= 0) return;


            //// Tính tỉ lệ giữa Pixel thật và UI để cắt ảnh chính xác
            //double ratioX = _originalMat.Width / CurrentCanvasWidth;
            //double ratioY = _originalMat.Height / CurrentCanvasHeight;

            //OpenCvSharp.Rect rect = new((int)(RoiX * ratioX), (int)(RoiY * ratioY),
            //                            (int)(RoiWidth * ratioX), (int)(RoiHeight * ratioY));
            //rect.X = Math.Clamp(rect.X, 0, _originalMat.Width - 1);
            //rect.Y = Math.Clamp(rect.Y, 0, _originalMat.Height - 1);
            //rect.Width = Math.Clamp(rect.Width, 1, _originalMat.Width - rect.X);
            //rect.Height = Math.Clamp(rect.Height, 1, _originalMat.Height - rect.Y);
            //_templateMat = new Mat(_originalMat, rect).Clone();
            if (TeachingVM == null) return;

            // Lấy thông số "nóng" từ cửa sổ phụ
            finalThresh = TeachingVM.ThresholdValue;
            finalBlur = TeachingVM.BlurValue;

            // 2. Tiền xử lý ảnh mẫu (phải giống hệt lúc Process)
            using Mat gray = new Mat();
            using Mat blurred = new Mat();
            using Mat thresh = new Mat();

            Cv2.ImShow("Debug ROI", _templateMat);
            Cv2.CvtColor(_templateMat, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(finalBlur, finalBlur), 0);
            Cv2.Threshold(blurred, thresh, finalThresh, 255, ThresholdTypes.BinaryInv);

            // 3. Tìm Contour của đối tượng trong vùng ROI
            Cv2.FindContours(thresh, out Point[][] contours, out HierarchyIndex[] hierarchy,
                             RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length > 0)
            {
                // Giả sử đối tượng cần học là contour lớn nhất
                var templateContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                double area = Cv2.ContourArea(templateContour);
                var rect = Cv2.BoundingRect(templateContour);
                double aspectRatio = (double)rect.Width / rect.Height;
                // 4. Lưu lại góc máy (Template Angle)
                // Lưu ý: FitEllipse yêu cầu contour phải có ít nhất 5 điểm
                if (templateContour.Length >= 5 && area > 10)
                {
                    templateAngle = Cv2.FitEllipse(templateContour).Angle;

                    ModernMessageBox.Show($"Học thành công!\nThresh: {finalThresh}\nAngle: {templateAngle:F2}\nArea: {area:F2}\nRatio: {aspectRatio:F2}");
                }
                else
                {
                    ModernMessageBox.Show("Contour quá nhỏ hoặc không đủ điểm để tính góc.");
                }
            }
            else
            {
                ModernMessageBox.Show("Không tìm thấy Contour nào với thông số hiện tại!");
            }


        }
        [RelayCommand]

        private async Task ProcessAsync()
        {
            if (MyImage == null) return;

            if (MyImage is BitmapSource bitmapSource)
            {
                // 1. Chuyển đổi ImageSource (giao diện) sang Mat (OpenCV)
                // Lưu ý: Bạn cần dùng thư viện OpenCvSharp.WpfExtensions
                using var currentMat = bitmapSource.ToMat();
            }
            if (_templateMat == null||_originalMat==null) return;
            if (_visionService == null)
            {
                MessageBox.Show("Vision Service chưa được khởi tạo. Vui lòng kiểm tra cấu hình.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
                
            }
            var result = await Task.Run(() => _visionService.ExecuteInspection(_originalMat,
            _templateMat,
            finalThresh,
            finalBlur,
            templateAngle));

            if (result.IsSuccess)
            {
                ResultX = result.X.ToString();
                ResultY = result.Y.ToString();
                ResultR = result.Angle.ToString();
                ResultScore = result.Score.ToString();

                // Hiển thị ảnh đã vẽ khung/điểm trung tâm lên màn hình
                if (result.ProcessedMat != null)
                {
                    MyImage = result.ProcessedMat.ToWriteableBitmap();
                    result.ProcessedMat.Dispose(); // Giải phóng vùng nhớ ảnh debug
                }
            }
            else
            {
                // Xử lý khi không tìm thấy đối tượng
                ResultX = "N/A";
                ResultY = "N/A";
                ResultR = "N/A";
                ResultScore = "N/A";
                ModernMessageBox.Show("không tìm thấy");
            }
        }

    }

}
   

