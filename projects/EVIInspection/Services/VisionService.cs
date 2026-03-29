using EVIInspection.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace EVIInspection.Services
{
    public interface IVisionService
    {
        // Trả về kết quả xử lý ảnh (X, Y, Angle, Score,...)
        VisionResult ExecuteInspection(Mat sceneMat, Mat templateMat, double threshold, int blur, double templateAngle);
    }
    public class VisionService : IVisionService
    {
        public VisionResult ExecuteInspection(Mat sceneMat, Mat templateMat, double threshold, int blur, double templateAngle)
        {
            var result = new VisionResult { IsSuccess = false };

            if (sceneMat == null || sceneMat.Empty() || templateMat == null || templateMat.Empty())
                return result;

            try
            {
                // 1. Lấy Contour mẫu (Template)
                using var tGray = templateMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                using var tBinary = ApplyThreshold(tGray, threshold, blur);
                var tContours = tBinary.FindContoursAsArray(RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                if (tContours.Length == 0) return result;
                var bestTemplateContour = tContours.OrderByDescending(c => Cv2.ContourArea(c)).First();

                // 2. Xử lý ảnh hiện tại (Scene)
                Mat outputMat = sceneMat.Clone();
                using var sGray = sceneMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                using var sBinary = ApplyThreshold(sGray, threshold, blur);
                var sContours = sBinary.FindContoursAsArray(RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                double minMatchScore = double.MaxValue;
                OpenCvSharp.Point[]? bestMatchContour = null;

                // 3. Tìm đối tượng khớp nhất (Logic MatchShapes từ file gốc)
                foreach (var contour in sContours)
                {
                    double area = Cv2.ContourArea(contour);
                    var rect = Cv2.BoundingRect(contour);
                    double aspectRatio = (double)rect.Width / rect.Height;
                    if (area < 20000 || area > 50000) continue; // Bỏ qua nhiễu quá nhỏ
                    if (aspectRatio < 0.4 || aspectRatio > 2) continue;

                    // So sánh hình dạng
                    double score = Cv2.MatchShapes(bestTemplateContour, contour, ShapeMatchModes.I1, 0);

                    if (score < minMatchScore)
                    {
                        minMatchScore = score;
                        bestMatchContour = contour;
                    }
                }

                // 4. Tính toán kết quả nếu Score nằm trong ngưỡng cho phép (ví dụ < 0.2)
                if (bestMatchContour != null && minMatchScore < 0.5)
                {
                    var moments = Cv2.Moments(bestMatchContour);
                    double centerX = moments.M10 / moments.M00;
                    double centerY = moments.M01 / moments.M00;

                    // Tính góc xoay bằng FitEllipse (Yêu cầu ít nhất 5 điểm)
                    double finalAngle = 0;
                    if (bestMatchContour.Length >= 5)
                    {
                        var rect = Cv2.FitEllipse(bestMatchContour);
                        // Tính độ lệch góc so với lúc Teaching
                        finalAngle = rect.Angle - templateAngle;
                    }

                    // Vẽ minh họa kết quả
                    outputMat.DrawContours(new[] { bestMatchContour }, -1, Scalar.Yellow, 2);
                    outputMat.Circle(new OpenCvSharp.Point(centerX, centerY), 5, Scalar.Green, -1);
                    outputMat.PutText($"{minMatchScore:F3}", new OpenCvSharp.Point(centerX + 10, centerY),
                                     HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);

                    result.IsSuccess = true;
                    result.X = Math.Round(centerX, 2);
                    result.Y = Math.Round(centerY, 2);
                    result.Angle = Math.Round(finalAngle, 2);
                    result.Score = minMatchScore;
                    result.ProcessedMat = outputMat;
                }
                else
                {
                    outputMat.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return result;
        }

        private Mat ApplyThreshold(Mat gray, double thresh, int blur)
        {
            // Đảm bảo kSize luôn lẻ cho GaussianBlur
            int kSize = (blur % 2 == 0) ? blur + 1 : blur;
            if (kSize < 1) kSize = 1;

            using var blurred = gray.GaussianBlur(new OpenCvSharp.Size(kSize, kSize), 0);
            return blurred.Threshold(thresh, 255, ThresholdTypes.Binary);
        }
    }
}
