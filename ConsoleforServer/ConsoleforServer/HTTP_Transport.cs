using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;


namespace ConsoleforServer
{
    class HTTP_Transport
    {
        public string HTTP_GET_PIC(string url)
        {
            try
            {
                // string uri = @"http://a2.att.hudong.com/36/48/19300001357258133412489354717.jpg";
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";                            //请求方法
                request.ProtocolVersion = new Version(1, 1);   //Http/1.1版本
                request.Timeout = 10000;


                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Console.WriteLine("-----------------  响应体  -----------------");

                //Header
                foreach (var item in response.Headers)
                {
                    Console.WriteLine(item.ToString() + ": " + response.GetResponseHeader(item.ToString()) + System.Environment.NewLine);
                }

                //如果主体信息不为空，则接收主体信息内容
                if (response.ContentLength <= 0)
                {
                    Console.WriteLine("主体消息为空！");
                    return null;
                }

                string filename = response.GetResponseHeader("Content-Disposition").Split('"')[1];
                
                //接收响应主体信息
                //using (Stream stream = response.GetResponseStream())
                //{
                //    int totalLength = (int)response.ContentLength;
                //    int numBytesRead = 0;
                //    byte[] bytes = new byte[totalLength + 1024];
                //    //通过一个循环读取流中的数据，读取完毕，跳出循环
                //    while (numBytesRead < totalLength)
                //    {
                //        int num = stream.Read(bytes, numBytesRead, 1024);  //每次希望读取1024字节
                //        if (num == 0)   //说明流中数据读取完毕
                //            break;
                //        numBytesRead += num;
                //    }
                //    //将接收到的主体数据显示到界面
                //    string content = Encoding.UTF8.GetString(bytes);
                //    Console.WriteLine("主体消息：" + "\r\n");
                //    Console.WriteLine(content);
                //}

                //保存图片
                using (Stream stream = response.GetResponseStream())
                {
                    //Bitmap pic = new Bitmap(stream);
                    //pic.Save(filename);

                    //当前时间作为文件名
                    //string fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";

                    using (Stream fsStream = new FileStream(filename, FileMode.Create))
                    {
                        stream.CopyTo(fsStream);
                    }
                }
               
                return filename;
            }
            catch (Exception ee)
            {
                Console.WriteLine("HTTP_GET_PIC 异常:" + ee.ToString());
            }
            return null;
           
        }

        public void  HTTP_POST_PIC(string filename ,string upload_url)
        {
            //using (HttpClient client = new HttpClient())
            //{
            //    var content = new MultipartFormDataContent();
            //    //添加字符串参数，参数名为qq
            //    content.Add(new StringContent("123456"), "qq");

            //    string path = Path.Combine(System.Environment.CurrentDirectory, "111.jpg");
            //    //添加文件参数，参数名为files，文件名为123.png
            //    content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(path)), "file", "111.jpg");

            //    //var requestUri = "http://192.168.1.4/";
            //    var requestUri = "http://192.168.1.4/UpLoadTest/";
            //    var result = client.PostAsync(requestUri, content).Result.Content.ReadAsStringAsync().Result;

            //    Console.WriteLine(result);
            //}
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    // 测试上传url:  http://172.17.0.14/UpLoadFile/
                    //webClient.UploadDataAsync(Uri address, byte[] data);
                    try
                    {
                        //webClient.UploadFileAsync(new Uri("http://150.158.7.244/UpLoadFile/"), filename);
                        webClient.UploadFileAsync(new Uri(upload_url), filename);
                        
                        Console.WriteLine("上传完成");
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(@"UploadFileAsync 异常:\r\n"+ee.ToString());
                    }

                    //webClient.DownloadFile("http://example.com", "路径");
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(@"HTTP_POST_PIC 异常 \r\n", ee.ToString());
            }

        }
    }
}
