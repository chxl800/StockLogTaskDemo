using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace StockLogTask
{

    /// <summary>
    /// 入口
    /// </summary>
    class Program
    {
        public  static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<StockLogJob>(s =>
                {
                    s.ConstructUsing(name => new StockLogJob());
                    s.WhenStarted((t) => t.Start());
                    s.WhenStopped((t) => t.Stop());
                });

                x.RunAsLocalSystem();

                //服务的描述
                x.SetDescription("StockLog_Description");
                //服务的显示名称
                x.SetDisplayName("StockLog_DisplayName");
                //服务名称
                x.SetServiceName("StockLog_ServiceName");

            });
        }

        
    }
}
