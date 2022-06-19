﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Backend.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        ///// <summary>
        ///// 应用程序启动入口方法(Main)
        ///// </summary>
        ///// <param name="args"></param>
        //public static void Main(string[] args)
        //{
        //    //CreateWebHostBuilder(args).Build().Run();
        //    var host = CreateWebHostBuilder(args).Build();
        //    host.Run();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseKestrel(c => c.AddServerHeader = false)
        //        .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>();
    }
}
