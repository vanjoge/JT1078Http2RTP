using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JT1078Http2RTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JTH2R_Web
{
    public class Program
    {
        /// <summary>
        /// 用静态变量存储下所有当前正在传输的客户端 防止被GC回收
        /// </summary>
        public static JTTask task;
        public static void Main(string[] args)
        {
            SQ.Base.ByteHelper.RegisterGBKEncoding();

            task = new JTTask();
            CreateHostBuilder(args).Build().Run();
            task.Dispose();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
