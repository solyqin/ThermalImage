using Flir.Atlas.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace ConsoleforServer
{
    struct Result_pic_info
    {
        public double max;
        public double min;
        public double avg;
        public string result_name;
    }

    class AtlasSDK
    {
        // private ThermalImageFile th;  // 热成像图片文件对象
        
        public Result_pic_info? OpenPic(string filename, Pic_info pic_info)
        {
            if (!ThermalImageFile.IsThermalImage(filename))
            {
                Console.WriteLine("图片不是热成像图片文件，无法打开");
                return null;
            }
            using (ThermalImageFile th = new ThermalImageFile(filename))   //打开热成像图片文件  
            {
                using (Bitmap saveimage = DrawRectangleInPicture(th, pic_info, Color.Red, 1, DashStyle.Solid))
                {
                    saveimage.Save("AAA_" + filename);
                    Console.WriteLine("生成结果图片");
                }

                Result_pic_info result_Pic_Info = new Result_pic_info();
                result_Pic_Info.avg = th.Statistics.Average.Value;
                result_Pic_Info.max = th.Statistics.Max.Value;
                result_Pic_Info.min = th.Statistics.Min.Value;
                result_Pic_Info.result_name = "AAA_" + filename;

                return result_Pic_Info;
            }
        }

        public void  AnalysisPic(ThermalImageFile th)
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
        public Bitmap DrawRectangleInPicture(ThermalImageFile thermal, Pic_info pic_info, Color RectColor, int LineWidth, DashStyle ds)
        {
            if (thermal == null) return null;

            Graphics g = Graphics.FromImage(thermal.Image);
            Brush brush = new SolidBrush(RectColor);
            Pen pen = new Pen(brush, LineWidth);
            pen.DashStyle = ds;  //边界样式
            Rectangle rectangle = new Rectangle(pic_info.point_start.X, pic_info.point_start.Y, Math.Abs(pic_info.point_start.X - pic_info.point_end.X), Math.Abs(pic_info.point_start.Y - pic_info.point_end.Y));

            //画框
            g.DrawRectangle(pen, rectangle);

            //画三角形标记
            DrawMaxMinPoint(thermal, rectangle, g, pic_info);

            g.Dispose();

            return thermal.Image;
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
        /// <summary>
        /// 标记指定矩形区域内的最高和最低温度点
        /// </summary>
        /// <param name="th">红外图片对象</param>
        /// <param name="rect">指定矩形</param>
        public void DrawMaxMinPoint(ThermalImageFile th, Rectangle rect, Graphics g, Pic_info pic_info)
        {
            double[] tempertureRect = th.GetValues(rect); //Math.Round(d, 2).ToString()
            Point pt_max = GetMaxPointInRectangle(tempertureRect, rect);//获取高温点
            Point pt_min = GetMinPointInRectangle(tempertureRect, rect);//获取高温点

            FillTriangle_1(g, pt_max, Dire.UP);
            FillTriangle_1(g, pt_min, Dire.DOWN);
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
