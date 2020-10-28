using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleforServer
{
    class HandleThermalPictrue
    {
        DataBaseManage DBManage;

      //  public readonly static HandleThermalPictrue Instance = new HandleThermalPictrue();
       
        private static readonly Queue<Pic_info> ListQueue = new Queue<Pic_info>();  //实例化Pic_infod队列

        readonly static object _locker = new object();//object类的锁，它的作用就是防止冲突

        ManualResetEvent waitingKeeper = new ManualResetEvent(false);

        public HandleThermalPictrue()
        {
            DBManage = DataBaseManage.GetInstance();
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <returns> bool 成功返回true；失败返回false</returns>
        public bool ConnectDB()
        {
            if (DBManage.DB_Connect())
            {
                Console.WriteLine("数据库连接!");
                return true;
            }
            else
            {
                Console.WriteLine("数据库连接 失败!");
                return false;
            }
        }


        /// <summary>
        /// 开启工作线程
        /// </summary>
        public void Start()
        {
            Thread th_search = new Thread(ThreadTask_SearchDatabase);
            Thread th_handle = new Thread(ThreadTask_HandlePicture);
            //th_search.IsBackground = true;  //设置未后台线程，应用关闭时，进程关闭时随之关闭
           // th_handle.IsBackground = true;  //false：设置为前台线程(只要线程还在就会阻止进程关闭)
            th_search.Start();
            Console.WriteLine("启动：th_Search");
            th_handle.Start();
            Console.WriteLine("启动：th_Handle");
        }

        /// <summary>
        /// 添加任务进入队列
        /// </summary>
        /// <param name="info"></param>
        public bool AddTaskQueue(Pic_info info) //入列
        {
            if (!ListQueue.Contains(info))
            {
                ListQueue.Enqueue(info);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 搜索数据库查看是否有需要处理的任务，插入任务队列中
        /// </summary>
        private void ThreadTask_SearchDatabase()
        {
            int oldQueueCount;
            while (true)
            {
                List<Pic_info> list_pic_Info = DBManage.DB_CheckForUpdate();// 检查是否有需要处理的图片
                Console.WriteLine("[{0}][定时查询] 搜索到[{1}]条记录", DateTime.Now.ToString(), list_pic_Info.Count);

                if (list_pic_Info.Count > 0)
                {
                    lock (_locker) //锁，用来保护数据读写不会冲突
                    {
                        oldQueueCount = ListQueue.Count();
                        foreach (var pic_Info in list_pic_Info)
                        {
                            if (AddTaskQueue(pic_Info))  //添加任务
                                Console.WriteLine("-----------------------添加任务----ListQueue: +{0}---------当前任务量:{1}---------------", ListQueue.Count() - oldQueueCount, ListQueue.Count());
                        }
                    }
                }
                Thread.Sleep(3000); //用一个线程控制，每3秒查询一次数据库
            }
        }

        /// <summary>
        /// 处理任务队列中的任务
        /// </summary>
        private void ThreadTask_HandlePicture()
        {
            HTTP_Transport HTTP = new HTTP_Transport();
            AtlasSDK atlasSDK = new AtlasSDK();
            while (true)
            {
                if (ListQueue.Count > 0)
                {
                    try
                    {
                        ScanListQueue(HTTP, atlasSDK);  //扫描任务队列
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[{0}]扫描任务队列，开始处理任务时发生异常：\r\n {1}", DateTime.Now.ToString(),ex);
                    }
                }
                else
                {
                    //没有任务，休息3秒钟
                    Thread.Sleep(2000);
                }
            }
        }

        private void ScanListQueue(HTTP_Transport HTTP, AtlasSDK atlasSDK)
        {
            lock (_locker) //锁，用来保护数据读写不会冲突
            {
                try
                {
                    Pic_info pic_info = ListQueue.Dequeue(); //从队列中取出
                    DBManage.DB_UpdataState(pic_info.id);//领取任务后马上跟新任务状态，避免被重复搜索

                    string filename = HTTP.HTTP_GET_PIC(pic_info.download_url); // 通过http下载图片
                    
                    if (null == filename)
                    {
                        Console.WriteLine("[{0}][error]未接收指定图片主体！ 图片ID:{1}", DateTime.Now.ToString(), pic_info.id);
                        return;
                    }

                    if (File.Exists(filename))
                    {
                        Result_pic_info? r_pic_info = atlasSDK.OpenPic(filename, pic_info); //解析红外图片,处理后的图片临时保存在本地
                        if (null != r_pic_info)
                        {
                            if (File.Exists(r_pic_info.Value.result_name))
                                Console.WriteLine("结果图片已生成。准备上传.");

                            HTTP.HTTP_POST_PIC(r_pic_info.Value.result_name, pic_info.upload_url); //上传处理好的红外图

                            pic_info.intAverage = r_pic_info.Value.avg;
                            pic_info.intMaxTemperature = r_pic_info.Value.max;
                            pic_info.intMinTemperature = r_pic_info.Value.min;
                            //上传之前删除刚才下载的图片临时文件
                            //if (File.Exists(filename))
                            //{
                            //    File.Delete(filename);
                            //}

                            DBManage.DB_UpdataRecord(pic_info);
                        }
                    }
                    else
                        Console.WriteLine("---------------------  正在下载图---------");
                    //Thread.Sleep(1000);//模拟其他过程要耗费的时间
                    //取出的queueinfo就可以用了，里面有你要的东西
                    //以下就是处理程序了
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}
