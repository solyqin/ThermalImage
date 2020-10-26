using System;
using System.Collections.Generic;
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
            if (!DBManage.DB_Connect())
            {
                Console.WriteLine("数据库连接 失败!");
                return false;
            }
            else
            {
                Console.WriteLine("数据库连接!");
                return true;
            }
        }


        /// <summary>
        /// 开启工作线程
        /// </summary>
        public void Start()
        {


            Thread th_search = new Thread(ThreadTask_SearchDatabase);
            Thread th_handle = new Thread(ThreadTask_HandlePicture);
            //thread.IsBackground = true;
            th_search.Start();
            Console.WriteLine("启动：th_search");
            th_handle.Start();
            Console.WriteLine("启动：th_search");
        }

        /// <summary>
        /// 添加任务进入队列
        /// </summary>
        /// <param name="info"></param>
        public void AddTaskQueue(Pic_info info) //入列
        {
            if (!ListQueue.Contains(info))
                ListQueue.Enqueue(info);
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
                            AddTaskQueue(pic_Info);  //添加任务
                        }
                        Console.WriteLine("-----------------------添加任务----ListQueue：{0}------------------------", ListQueue.Count() - oldQueueCount);
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
            while (true)
            {
                if (ListQueue.Count > 0)
                {
                    try
                    {
                        ScanListQueue();  //扫描任务队列
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

        private void ScanListQueue()
        {
            lock (_locker) //锁，用来保护数据读写不会冲突
            {
                try
                {
                    //从队列中取出
                    Pic_info pic_info = ListQueue.Dequeue();

                    Console.WriteLine("[{0}][ThreadTask_HandlePicture]记录：{1}", DateTime.Now.ToString(), pic_info.GetString());
                    DBManage.DB_UpdataRecord(pic_info.id);
                    Thread.Sleep(1000);//模拟其他过程要耗费的时间
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
