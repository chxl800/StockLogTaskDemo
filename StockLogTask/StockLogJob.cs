using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StockLogTask
{
    /// <summary>
    ///  任务-库存操作日志
    /// </summary>
    public class StockLogJob
    {
        private Timer _timer = null;
 
        public StockLogJob()
        {
            _timer = new Timer(10000) { AutoReset = true };
            _timer.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                if (Condition)
                    Job();//实现的作业方法
            };
        }
        public void Start(){ _timer.Start();}
        public void Stop() { _timer.Stop(); }

    
        #region 实现的作业方法
        private static string time = System.Configuration.ConfigurationManager.AppSettings["StockLogTime"].ToString();//设置时间点
        private static bool canExecuteNext = true;
        private static DateTime LastExecuteTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + time);
        public bool Condition
        {
            get
            {
                return canExecuteNext && (LastExecuteTime < DateTime.Now);
            }
        }
        public void Job()
        {
            try
            {
                canExecuteNext = false;
                //具体操作
                Common.PrintLog("库存操作日志跑批开始");
                //Console.WriteLine("库存操作日志跑批开始" + DateTime.Now);

                //具体业务
                string data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ff:ffff");
                //int rows = new DbSqlHelp().ExecuteNonQuery(CommandType.StoredProcedure, "p_StockLog", null);



                //Console.WriteLine("库存操作日志跑批结束rows=" + rows + DateTime.Now);
                Common.PrintLog("库存操作日志跑批成结束rows=" + data);
            }
            catch (Exception ex) {
                Common.PrintLog(ex.ToString());
            }
            finally
            {
                canExecuteNext = true;
                LastExecuteTime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " " + time);
            }
           
        }
        #endregion

    }
}
