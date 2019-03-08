using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLogTask
{
    /// <summary>
    /// 操作数据库
    /// </summary>
    public class DbSqlHelp
    {
        private string connString = string.Empty;

        /// <summary>
        /// 数据库连接串
        /// </summary>
        public string ConnectionString
        {
            set { connString = value; }
        }

        /// <summary>
        /// 构造
        /// </summary>
        public DbSqlHelp()
        {
            this.connString = System.Configuration.ConfigurationManager.AppSettings["DefalutConnectionStr"].ToString();
        }

        #region  === ExecuteNonQuery ===

        /// <summary>
        /// 执行命令并返回影响的记录数.
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">T-SQL语句或存储过程名</param>
        /// <param name="commandParameters">SqlParamters 集合</param>
        /// <returns>受影响的记录条数</returns>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                cmd.CommandTimeout = 60 * 15;
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                conn.Close();
                return val;
            }
        }
        /// <summary>
        /// 事务执行sql语句集并返回是否成功
        /// </summary>
        /// <param name="listSql">sql语句集</param>
        /// <returns>成功true 失败false</returns>
        public bool ExecuteNonQuery(List<string> listSql)
        {
            SqlTransaction ts = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                ts = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = ts;
                try
                {
                    foreach (string sql in listSql)
                    {
                        cmd.CommandText = sql;
                        if (cmd.CommandText.Trim() == "")
                            continue;
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Dispose();
                    ts.Commit();
                    conn.Close();
                    return true;
                }
                catch
                {
                    ts.Rollback();
                    conn.Close();
                    return false;
                }
            }

        }

        /// <summary>
        /// 事务执行sql语句集并返回是否成功，出错不会回退。
        /// </summary>
        /// <param name="listSql">sql语句集</param>
        /// <returns>成功true 失败false</returns>
        public bool ExecuteNonRollBack(List<string> listSql)
        {
            SqlTransaction ts = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                ts = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = ts;
                try
                {
                    foreach (string sql in listSql)
                    {
                        cmd.CommandText = sql;
                        if (cmd.CommandText.Trim() == "")
                            continue;
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Dispose();
                    ts.Commit();
                    conn.Close();
                    return true;
                }
                catch
                {
                    //ts.Rollback();
                    conn.Close();
                    return false;
                }
            }

        }
        #endregion

        #region === ExecuteReader ===

        /// <summary>
        /// 执行sql命令返回DataReader
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">T-SQL语句或存储过程名</param>
        /// <param name="commandParameters">SqlParamters 集合</param>
        /// <returns>SqlDataReader结果集</returns>
        public SqlDataReader ExecuteReader(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connString);
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw new Exception("DBHelper ExecuteReader Exception.", ex);
            }
        }

        #endregion

        #region === ExecuteDataSet ===

        /// <summary>
        /// 执行Sql语句返回DataSet
        /// </summary>
        /// <param name="cmdText">T-SQL语句</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(string cmdText)
        {
            DataSet ds = new DataSet();

            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            SqlDataAdapter oAdapter = new SqlDataAdapter();

            try
            {
                cmd.CommandText = cmdText;
                oAdapter.SelectCommand = cmd;
                oAdapter.Fill(ds);
            }
            catch (Exception ex)
            {
                ds.Clear();
                throw new Exception("DBHelper ExecuteDataSet Exception.", ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return ds;
        }
        /// <summary>
        /// 执行Sql命令返回DataSet
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">T-SQL语句或存储过程名</param>
        /// <param name="commandParameters">SqlParamters 集合</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connString);
            SqlDataAdapter oAdapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                oAdapter.SelectCommand = cmd;
                oAdapter.Fill(ds);
            }
            catch (Exception ex)
            {
                ds.Clear();
                throw new Exception("DBHelper ExecuteDataSet Exception.", ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return ds;
        }


        #endregion

        #region === ExecuteScalar ===

        /// <summary>
        /// 返回查询结果中的第一行的第一列
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">T-SQL语句或存储过程名</param>
        /// <param name="commandParameters">SqlParamters 集合</param>
        /// <returns>查询结果中的第一行的第一列数据</returns>
        public object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    object val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return val;
                }
                catch (Exception ex)
                {
                    throw new Exception("DBHelper ExecuteScalar Exception.", ex);
                }
            }
        }

        #endregion

        public SqlParameter GetParameter(string name, object value)
        {
            if (value == null)
                value = System.DBNull.Value;
            return new SqlParameter(name, value);
        }

        /// <summary>
        /// 准备一条命令信息
        /// </summary>
        /// <param name="cmd">SqlCommand 对象</param>
        /// <param name="conn">SqlConnection 对象</param>
        /// <param name="trans">SqlTransaction 对象</param>
        /// <param name="cmdType">SqlCommand 类型</param>
        /// <param name="cmdText">命令内容</param>
        /// <param name="cmdParms">SqlParameters 数组</param>
        private void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
    }
}
