using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace EVIInspection.Camera
{
    public class List_Object
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Angle { get; set; }
        public double Score { get; set; } // Giá trị diff từ MatchShapes
        public Point[] Contour {  get; set; }
    }
}
