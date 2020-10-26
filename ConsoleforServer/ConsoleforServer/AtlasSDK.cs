using Flir.Atlas.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace ConsoleforServer
{
   
    class AtlasSDK
    {
        DashStyle ds = DashStyle.Solid; //矩形框的边框样式


        private ThermalImageFile th;  // 热成像图片文件对象
         //  public void OpenPic(Point Rect_Location)
        public void OpenPic(Bitmap pictrue)
        {
           // th = new ThermalImageFile("123.jpg");   //打开热成像图片文件  
            Bitmap saveimage = DrawRectangleInPicture(pictrue, new Point (100,100), new Point(200, 200), Color.Red, 1);
            saveimage.Save(@"D:\saveimage.jpg");
        }

        public void AnalysisPic(ThermalImageFile th)
        {
            Console.WriteLine("avg:{0}", th.Statistics.Average.Value);
            Console.WriteLine("max:{0}", th.Statistics.Max.Value);
            Console.WriteLine("min:{0}", th.Statistics.Min.Value);
        }

        /// <summary>
        /// 在图片上画框
        /// </summary>
        /// <param name="bmp">原始图</param>
        /// <param name="p0">起始点</param>
        /// <param name="p1">终止点</param>
        /// <param name="RectColor">矩形框颜色</param>
        /// <param name="LineWidth">矩形框边界</param>
        /// <param name="ds">边界样式</param>
        // /// <param name="HotSpot">最高温度点</param>
        // /// <param name="ColdSpot">最低温度点</param>
        /// <returns>Bitmap 画上矩形和标记出最高、低温度点的图</returns>
        // public static Bitmap DrawRectangleInPicture(Bitmap bmp, Point p0, Point p1, Color RectColor, int LineWidth, DashStyle ds,Point HotSpot,Point ColdSpot)
        public Bitmap DrawRectangleInPicture(Bitmap bmp, Point p0, Point p1, Color RectColor, int LineWidth)
        {
            if (bmp == null) return null;

            Graphics g = Graphics.FromImage(bmp);

            Brush brush = new SolidBrush(RectColor);
            Pen pen = new Pen(brush, LineWidth);
            pen.DashStyle = ds;

            g.DrawRectangle(pen, new Rectangle(p0.X, p0.Y, Math.Abs(p0.X - p1.X), Math.Abs(p0.Y - p1.Y)));

            g.Dispose();

            return bmp;
        }
        public enum Dire
        {
            UP,
            DOWN
        }

        /// <summary>
        /// 填充三角形,用三角形标记出点的位置
        /// </summary>
        /// <param name="g">Graphics类</param>
        /// <param name="point">三角尖尖</param>
        /// <param name="dire">三角形的指向</param>
        /// <returns></returns>
        private void FillTriangle_1(Graphics g, Point point, Dire dire)
        {
            int dev = 8;  //三角形的高
            Point cornerleft;
            Point cornerright;

            switch (dire)
            {
                case Dire.UP:
                    cornerleft = new Point(point.X - dev / 2, point.Y - dev);
                    cornerright = new Point(point.X + dev / 2, point.Y - dev);
                    Point[] pntArr = { point, cornerleft, cornerright };
                    g.FillPolygon(Brushes.Red, pntArr);
                    break;
                case Dire.DOWN:
                    cornerleft = new Point(point.X - dev / 2, point.Y + dev);
                    cornerright = new Point(point.X + dev / 2, point.Y + dev);
                    Point[] pntArr1 = { point, cornerleft, cornerright };
                    g.FillPolygon(Brushes.Blue, pntArr1);
                    break;
                default:
                    break;
            }
        }

        public void GetMaxMinPoint(ThermalImageFile th, Rectangle rect)
        {
            double[] tempertureRect = th.GetValues(rect); //Math.Round(d, 2).ToString()
            Point pt_max = GetMaxPointInRectangle(tempertureRect, rect);//获取高温点
            Point pt_min = GetMinPointInRectangle(tempertureRect, rect);//获取高温点
        }

        /// <summary>
        /// 获取框选区域温度最高点的坐标
        /// </summary>
        /// <param name="arr">目标矩形区域内所有点的温度集合</param>
        /// <param name="rect">目标矩形</param>
        /// <returns>Point 最高温度点的坐标</returns>
        public Point GetMaxPointInRectangle(double[] arr, Rectangle rect)
        {
            int index = Array.IndexOf(arr, arr.Max());
            int x = rect.Location.X + index % rect.Width;
            int y = rect.Location.Y + index / rect.Width;
            //Console.WriteLine(arr.Max().ToString()+" MaxPoint:({0},{1}),  index:{2}, location:({3},{4})", x, y, index, rect.Location.X, rect.Location.Y);
            return new Point(x, y);
        }
        
        /// <summary>
        /// 获取框选区域温度最低点的坐标
        /// </summary>
        /// <param name="arr">目标矩形区域内所有点的温度集合</param>
        /// <param name="rect">目标矩形</param>
        /// <returns>Point 最低温度点的坐标</returns>
        public Point GetMinPointInRectangle(double[] arr, Rectangle rect)
        {
            int index = Array.IndexOf(arr, arr.Min());
            int x = rect.Location.X + index % rect.Width;
            int y = rect.Location.Y + index / rect.Width;
            //Console.WriteLine(arr.Min().ToString() + "  MaxPoint:({0},{1}),  index:{2}, location:({3},{4})", x, y, index, rect.Location.X, rect.Location.Y);
            return new Point(x, y);
        }
    }
}
