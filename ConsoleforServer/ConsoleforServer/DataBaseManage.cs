﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleforServer
{
    public struct Pic_info
    {
        public int id;
        public string url;
        public int point_start;
        public int point_end;

        public string GetString()
        {
             return id.ToString()+", "+url + ", " + point_start.ToString() + ", " + point_end.ToString();
        }

}
    public class DataBaseManage : IDisposable
    {
        MySqlConnection sqlConn;
        MySqlConnection sqlConn_updata;

        List<Pic_info> List_Pic_info;
        // 定义一个静态变量来保存类的实例
        private volatile static DataBaseManage instance;

        private DataBaseManage()
        {
            List_Pic_info = new List<Pic_info>();
            instance = this;
           // Connect();
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,获取数据库类实例
        /// </summary>
        /// <returns>返回 DataBaseManage 类型</returns>
        public static DataBaseManage GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (instance == null)
            {
                instance = new DataBaseManage();
            }
            return instance;
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <returns>bool 成功返回true;失败返回false</returns>
        public bool DB_Connect()
        {
            //数据库
            string connStr = "Database = text_qxt; Data Source = 81.68.252.160;";
            connStr += "User Id = root; Password = !23456789o; port = 3306";
           
            try
            {
                sqlConn = new MySqlConnection(connStr);
                sqlConn_updata = new MySqlConnection(connStr);
                sqlConn.Open();
                sqlConn_updata.Open();
                Console.Write("[DataBaseManage]Connect success! \r\n" );
                return true;
            }
            catch (Exception e)
            {
                Console.Write("[DataBaseManage]Connect false! \r\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查是否有需要识别更新的图片
        /// </summary>
        /// <returns>需要识别更新的图片ID</returns>
        public List<Pic_info> DB_CheckForUpdate()
        {
            //string sql_Query = "select id,url,point_start,point_end from t_pic_info where state = '1' AND id >" + search_index + " LIMIT 1 ";
            string sql_Query = "select id,url,point_start,point_end from t_pic_info where state = '1'";
            using (MySqlCommand cmd = new MySqlCommand(sql_Query, sqlConn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())//执行ExecuteReader()返回一个MySqlDataReader对象
                {
                    List_Pic_info.Clear();
                    Pic_info Info = new Pic_info();
                    while (reader.Read())//初始索引是-1，执行读取下一行数据，返回值是bool
                    {
                        Info.id = reader.GetInt32("id");
                        Info.url = reader.GetString("url");
                        Info.point_start = reader.GetInt32("point_start");
                        Info.point_end = reader.GetInt32("point_end");
                        List_Pic_info.Add(Info);
                        //Console.WriteLine("[{0}] need update -- ID:{1}", DateTime.Now.ToString(), search_index);
                    }
                }
            }
            return List_Pic_info;
        }

        /// <summary>
        /// 根据图片ID 拿到图片的URL地址，获取图片
        /// <param name="ID">图片在数据库里的ID</param>
        /// </summary>
        /// <returns>Pic_info 图片信息 ID,URL等</returns>
        public Pic_info DB_GetPicInfo(int ID) //加一个问号表示一个可空对象，相当于 Nullable<StructA> 类型
        {
            Pic_info Info = new Pic_info();
            string sql_Query = "select id, url, point_start, point_end from t_pic_info where id = " + ID ;
            using (MySqlCommand cmd = new MySqlCommand(sql_Query, sqlConn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())//执行ExecuteReader()返回一个MySqlDataReader对象
                {
                    // while (reader.Read())//初始索引是-1，执行读取下一行数据，返回值是bool
                    if (reader.Read())
                    {
                        //search_index = reader.GetInt32("id"); //reader.GetString("username") "id"是数据库对应的列名
                        Info.id = reader.GetInt32("id");
                        Info.url = reader.GetString("url");
                        Info.point_start = reader.GetInt32("point_start");
                        Info.point_end = reader.GetInt32("point_end");

                        return Info;
                        //Console.WriteLine("[{0}] need update -- ID:{1}", DateTime.Now.ToString(), search_index);
                    }
                }
            }
            
            return Info;
        }

        /// <summary>
        /// 更新图片数据库状态
        /// <param name="updateID">图片在数据库里的ID</param>
        /// </summary>
        /// <returns></returns>
        public void DB_UpdataRecord(int updateID)
        {
            string sql_Query = "UPDATE t_pic_info SET state = '2' WHERE id = '" + updateID+"' ";
            using (MySqlCommand cmd = new MySqlCommand(sql_Query, sqlConn_updata))
            {
                int result = cmd.ExecuteNonQuery(); //执行语句
                if (result > 0)
                {
                    Console.WriteLine("[{0}] updateID:{1}  state 1 => 2", DateTime.Now.ToString(), updateID);
                }
            }

        }

        //释放
        void IDisposable.Dispose()
        {
            sqlConn.Close();
            sqlConn.Dispose();
        }

    }
}