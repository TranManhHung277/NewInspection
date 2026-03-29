using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace EVIInspection.Models
{
    public class VisionResult
    {
        public bool IsSuccess { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double Score { get; set; }
        public Mat? ProcessedMat { get; set; } // Ảnh để hiển thị kết quả (đã vẽ khung)
    }
}
