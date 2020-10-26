using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleforServer
{
    class OtherFunction
    {
        void InsertDBRandomName(MySqlConnection conn)
        {
            char[] allcheckRandom ={'0','1','2','3','4','5','6','7','8','9',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W',
                'X','Y','Z','a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q',
                'r','s','t','u','v','w','x','y','z'};

            for (int i = 0; i < 20; i++)
            {
                string Randomcode = "";
                for (int j = 0; j < 4; j++)
                {
                    int seekSeek = unchecked((int)DateTime.Now.Ticks);
                    Console.WriteLine("[{0}]", seekSeek.ToString());
                    Random seekRand = new Random(seekSeek);

                    Randomcode += allcheckRandom[seekRand.Next(0, 35)];
                }

                // 插入数据
                MySqlCommand mycmd = new MySqlCommand("insert into user(name, state) values('" + Randomcode + "', '0');", conn);
                if (mycmd.ExecuteNonQuery() > 0)
                {
                    Console.WriteLine("success");
                }
            }
        }
    }
}
