using MySql.Data.MySqlClient;//调用MySQL动态库
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleforServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HandleThermalPictrue HandlePro = new HandleThermalPictrue();

            if (HandlePro.ConnectDB()) //连接数据库
            {
                HandlePro.Start(); //开始任务
            }
            else
            {
                Console.WriteLine("数据库连接失败，程序退出！");
                Console.ReadLine();
            }
        }

        //static void Main(string[] args)
        //{


        //    DataBaseManage DBManage = DataBaseManage.GetInstance();
        //    AtlasSDK atlasSDK = new AtlasSDK();
        //    HTTP_Transport HTTP = new HTTP_Transport();
            


        //    if (!DBManage.DB_Connect())
        //    {
        //        Console.WriteLine("数据库连接 失败!");
        //        Console.ReadLine();
        //        return;
        //    }
        //    else 
        //    {
        //        Console.WriteLine("数据库连接!");
        //    }

        //    int need_updateID;
        //    Pic_info pic_Info;
        //    Bitmap pictrue;
        //    //轮询数据库
        //    // while (true)
        //    {
        //        DateTime beforDT = System.DateTime.Now;
        //        DateTime afterDT;
        //        TimeSpan ts;

        //        need_updateID = DBManage.DB_CheckForUpdate();// 检查是否有需要处理的图片
                 
        //        //if (-1 != need_updateID)
        //        //{
        //        //    pic_Info = DBManage.DB_GetPicInfo(need_updateID); //获取需要处理的图片信息

        //        //    if (!pic_Info.IsNull)
        //        //    {
        //        //        pictrue = HTTP.HTTP_GET_PIC(pic_Info.url); // 通过http下载图片

        //        //        if (null == pictrue)
        //        //        {
        //        //            Console.WriteLine("未接收指定url图片主体！");
        //        //            Console.ReadLine();
        //        //            return;
        //        //        }

        //        //        atlasSDK.OpenPic(pictrue); //解析红外图片

        //        //        pictrue.Dispose();
        //        //    }
        //        //}

        //        afterDT = System.DateTime.Now;
        //        ts = afterDT.Subtract(beforDT);
        //        Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);

        //        Console.ReadLine();
        //        //System.Threading.Thread.Sleep(2000); //用一个线程控制，睡2秒后重新开始
        //        //Console.WriteLine("[{0}] 定时查询", DateTime.Now.ToString());
        //    }
        //}
       
    }
}
