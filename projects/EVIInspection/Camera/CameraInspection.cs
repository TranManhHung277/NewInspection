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
        [ObservableProperty]
        private string _resultX = "0";

        [ObservableProperty]
        private string _resultY = "0";

        [ObservableProperty]
        private string _resultR = "0";
        [ObservableProperty]
        private CameraTeaching _teachingVM;
        public IRelayCommand OnMouseDownCommand { get; }
        public IRelayCommand OnMouseMoveCommand { get; }
        public IRelayCommand OnMouseUpCommand { get; }


        private System.Windows.Point _startPoint;
        private Mat? _originalMat;
        private Mat? _templateMat;
        private Mat? _grayMat;
        private bool _isDrawing = false;
        private double finalThresh;
        private int finalBlur;
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

            // Thực hiện logic Process hoặc lưu thông số
            

            ModernMessageBox.Show($"Đã học xong với Thresh: {finalThresh}, Blur: {finalBlur}");
            //Cv2.ImShow("Template Check", _templateMat);

            //ModernMessageBox.Show("Teaching Successful!");  
        }
        [RelayCommand]
        private void Process()
        {
            if (_originalMat == null || _templateMat == null) return;
            using var gray = _originalMat.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var blurred = gray.GaussianBlur(new Size(finalBlur, finalBlur), 0);
            using var thresh = blurred.Threshold(finalThresh, 255, ThresholdTypes.BinaryInv);
            Cv2.ImShow("Debug Thresh", thresh);
            Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            using var debugMat = _originalMat.Clone();
            bool found = false;
            var mainContour = contours
        .OrderByDescending(c => Cv2.ContourArea(c))
        .FirstOrDefault(c => Cv2.ContourArea(c) > 5000);
            if (mainContour==null || Cv2.ContourArea(mainContour) <= 500)
            {
                this.MyImage = _originalMat.ToWriteableBitmap(); // Hiện ảnh gốc nếu không tìm thấy
                ModernMessageBox.Show("Không tìm thấy đối tượng!");
                return;
            }
            else if (mainContour != null)
            {
                // 4. Tính toán Tâm (X, Y) bằng Moments
                var moments = Cv2.Moments(mainContour);
                if (moments.M00 != 0)
                {
                    int centerX = (int)(moments.M10 / moments.M00);
                    int centerY = (int)(moments.M01 / moments.M00);
                    // 5. Tính toán Góc (R) bằng MinAreaRect (Hình chữ nhật bao quanh tối ưu)
                    RotatedRect minRect = Cv2.MinAreaRect(mainContour);
                    float angle = minRect.Angle;
                    Point2f[] vertices = minRect.Points();
                    for (int j = 0; j < 4; j++)
                    {
                        debugMat.Line(new Point((int)vertices[j].X, (int)vertices[j].Y),
                             new Point((int)vertices[(j + 1) % 4].X, (int)vertices[(j + 1) % 4].Y),
                             Scalar.Red, 2);
                    }
                    debugMat.Circle(new Point(centerX, centerY), 5, Scalar.Green, -1);
                    // 6. Cập nhật Binding dữ liệu
                    if (minRect.Size.Width < minRect.Size.Height)
                    {
                        angle -= 90;
                    }
                    ResultX = centerX.ToString();
                    ResultY = centerY.ToString();
                    ResultR = angle.ToString("F1");
                    if (debugMat != null && !debugMat.Empty() && debugMat.Width > 0)
                    {
                        this.MyImage = debugMat.ToWriteableBitmap();
                        
                    }
                }

                else
                {
                    this.MyImage = _originalMat.ToWriteableBitmap(); // Hiện ảnh gốc nếu không tìm thấy
                    ModernMessageBox.Show("Không tìm thấy đối tượng!");
                }
            }
            else
            {
                this.MyImage = _originalMat.ToWriteableBitmap(); // Hiện ảnh gốc nếu không tìm thấy
                ModernMessageBox.Show("Không tìm thấy đối tượng!");
            }
                    
        }
            //this.MyImage = debugMat.ToWriteableBitmap();
            //if (!found) ModernMessageBox.Show("Không tìm thấy vật thể!");

            //if (maxVal > 0.8)
            //{
            //    // 1. Tính toán tâm vật thể (Pixel thực)
            //    int centerX = maxLoc.X + (_templateMat.Width / 2);
            //    int centerY = maxLoc.Y + (_templateMat.Height / 2);

            //    // 2. CẬP NHẬT KẾT QUẢ LÊN TEXTBOX (X, Y, R)
            //    ResultX = centerX.ToString();
            //    ResultY = centerY.ToString();
            //    ResultR = "0"; // Hiện tại chưa tính góc xoay

            //    // 3. VẼ KHUNG ĐỎ (Phải dùng đúng kích thước của ảnh mẫu _templateMat)
            //    using var debugMat = _originalMat.Clone();

            //    // Vẽ hình chữ nhật có kích thước BẰNG HỆT ảnh mẫu đã Teach
            //    OpenCvSharp.Rect resultRect = new OpenCvSharp.Rect(maxLoc, _templateMat.Size());
            //    debugMat.Rectangle(resultRect, Scalar.Red, 3);

            //    // Cập nhật lại ảnh hiển thị
            //    MyImage = debugMat.ToWriteableBitmap();

            //    ModernMessageBox.Show($"Tìm thấy vật thể!\nĐộ khớp: {maxVal:P0}");
            //}
        }

    }
   

