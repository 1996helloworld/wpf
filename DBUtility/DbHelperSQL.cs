using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;

//using Common;

namespace DBUtility //可以修改成实际项目的命名空间名称 
{
    /**/
    /// <summary> 
    /// Copyright (C) 2004-2008 LiTianPing  
    /// 数据访问基础类(基于SQLServer) 
    /// 用户可以修改满足自己项目的需要。 
    /// </summary> 
    public class DbHelperSQL
    {
        //数据库连接字符串(web.config来配置) 
        //protected static string ConnectionString1 = "server=.\;database=HighnetV2Power;uid=sa;pwd=guest0.;";
        protected static string ConnectionString1 = ConfigurationManager.AppSettings["main_db"];

        public DbHelperSQL()
        {

        }

        public static int GetStringByteLength(string ls_temp)
        {
            return System.Text.Encoding.Default.GetByteCount(ls_temp);
        }

        public static string GetSubstringByLength(string str, int len)
        {//截取字符串指定字节数的内容，并返回实际截取的字节数
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
            int n = 0;  //  表示当前的字节数
            int i = 0;  //  要截取的字节数
            for (; i < bytes.GetLength(0) && n < len; i++)
            {
                if (i % 2 == 0)//  偶数位置，如0、2、4等，为UCS2编码中两个字节的第一个字节
                {
                    n++;      //  在UCS2第一个字节时n加1
                }
                else
                {
                    if (bytes[i] > 0)//  当UCS2编码的第二个字节大于0时，该UCS2字符为汉字，一个汉字算两个字节
                    {
                        n++;
                    }
                }
            }
            //  如果i为奇数时，处理成偶数
            if (i % 2 == 1)
            {
                if (bytes[i] > 0) //  该UCS2字符是汉字时，去掉这个截一半的汉字
                    i = i - 1;
                else
                    i = i + 1;//  该UCS2字符是字母或数字，则保留该字符
            }
            len = i;
            return System.Text.Encoding.Unicode.GetString(bytes, 0, i);
        }

        public static int AddLog(string ls_user, string ls_ip, string ls_opt, string hostname)
        {
            ls_opt = ls_opt.Replace("'", "''");
            if (GetStringByteLength(ls_opt) > 1000)
            {
                ls_opt = GetSubstringByLength(ls_opt, 1000);
            }
            string ls_sql = "insert into syslog(operatorid,opt_time,opt_comment,ip,hostname) values('" + ls_user + "',getdate(),'" + ls_opt + "','" + ls_ip + "','" + hostname + "')";
            DbHelperSQL.ExecuteSql(ls_sql);
            return 1;

        }

        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            strsql = GetSelectText(strsql);
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }

        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            strSql = GetSelectText(strSql);
            foreach (SqlParameter sqlp in cmdParms)
            {
                if ((sqlp.Value.ToString()) == "")
                    sqlp.Value = DBNull.Value;
            }
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }





        /**/
        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteSql(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException er)
                    {
                        DbHelperSQL.AddLog("SQL", "", SQLString, "");
                        //Common.Log.WriteLog(er.ToString(), 1);
                        connection.Close();
                        throw;
                        //    throw new Exception(E.Message); 
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }

        /**/
        /// <summary> 
        /// 执行多条SQL语句，实现数据库事务。 
        /// </summary> 
        /// <param name="SQLStringList">多条SQL语句</param>         
        public static bool ExecuteSqlTran(List<string> SQLStringList)
        {
            int tmp = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString1))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            tmp = cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    if (tmp > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    //Common.Log.WriteLog(E.ToString(), 1);
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
        /**/
        /// <summary> 
        /// 执行带一个存储过程参数的的SQL语句。 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteSql(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    DbHelperSQL.AddLog("SQL", "", SQLString, "");
                    //Common.Log.WriteLog(E.ToString(), 1);
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();
                }
            }
        }


        /**/
        /// <summary> 
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例) 
        /// </summary> 
        /// <param name="strSQL">SQL语句</param> 
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    //Common.Log.WriteLog(E.ToString(), 1);
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        /**/
        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public static object GetSingle(string SQLString)
        {
            SQLString = GetSelectText(SQLString);
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        DbHelperSQL.AddLog("SQL", "", SQLString, "");
                        //Common.Log.WriteLog(e.ToString(), 1);
                        //connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }
        /**/
        /// <summary> 
        /// 执行查询语句，返回SqlDataReader 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>SqlDataReader</returns> 
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            strSQL = GetSelectText(strSQL);
            SqlConnection connection = new SqlConnection(ConnectionString1);

            using (SqlCommand cmd = new SqlCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return myReader;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    DbHelperSQL.AddLog("SQL", "", strSQL, "");
                    //Common.Log.WriteLog(e.ToString(), 1);
                    throw new Exception(e.Message);
                }
                finally
                {
                    //connection.Close();
                }
            }
        }


        /**/
        /// <summary> 
        /// 执行查询语句，返回DataSet 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataSet</returns> 
        public static DataSet Query(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                try
                {
                    DataSet ds = new DataSet();
                    connection.Open();
                    using (SqlDataAdapter command = new SqlDataAdapter(SQLString, connection))
                    {
                        command.Fill(ds, "ds");
                        command.Dispose();
                        return ds;
                    }
                }
                catch (System.Exception er)
                {
                    DbHelperSQL.AddLog("SQL", "", SQLString, "");
                    //Common.Log.WriteLog(er.ToString(), 1);
                    throw;
                    //return null; 
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }

            }

        }

        /**/
        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        foreach (SqlParameter sqlp in cmdParms)
                        {
                            if (sqlp.Value == null)
                                sqlp.Value = DBNull.Value;
                            else if ((sqlp.Value.ToString()) == "")
                                sqlp.Value = DBNull.Value;
                        }
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        DbHelperSQL.AddLog("SQL", "", SQLString, "");
                        //Common.Log.WriteLog(E.ToString(), 1);
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }


        /**/
        /// <summary> 
        /// 执行多条SQL语句，实现数据库事务。 
        /// </summary> 
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param> 
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString1))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环 
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch (Exception er)
                    {
                        //Common.Log.WriteLog(er.ToString(), 1);
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
        }


        /**/
        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        foreach (SqlParameter sqlp in cmdParms)
                        {
                            if ((sqlp.Value.ToString()) == "")
                                sqlp.Value = DBNull.Value;
                        }
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        DbHelperSQL.AddLog("SQL", "", SQLString, "");
                        //Common.Log.WriteLog(e.ToString(), 1);
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }

        /**/
        /// <summary> 
        /// 执行查询语句，返回SqlDataReader 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>SqlDataReader</returns> 
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SQLString = GetSelectText(SQLString);
            SqlConnection connection = new SqlConnection(ConnectionString1);

            using (SqlCommand cmd = new SqlCommand())
            {
                try
                {
                    foreach (SqlParameter sqlp in cmdParms)
                    {
                        if ((sqlp.Value.ToString()) == "")
                            sqlp.Value = DBNull.Value;
                    }
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    cmd.Parameters.Clear();
                    return myReader;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    DbHelperSQL.AddLog("SQL", "", SQLString, "");
                    //Common.Log.WriteLog(e.ToString(), 1);
                    throw new Exception(e.Message);
                }

            }
        }

        /**/
        /// <summary> 
        /// 执行查询语句，返回DataSet 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataSet</returns> 
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            SQLString = GetSelectText(SQLString);
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                foreach (SqlParameter sqlp in cmdParms)
                {
                    if ((sqlp.Value.ToString()) == "")
                        sqlp.Value = DBNull.Value;
                }
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        DbHelperSQL.AddLog("SQL", "", SQLString, "");
                        //Common.Log.WriteLog(ex.ToString(), 1);
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                    return ds;
                }
            }
        }


        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            foreach (SqlParameter sqlp in cmdParms)
            {
                if ((sqlp.Value.ToString()) == "")
                    sqlp.Value = DBNull.Value;
            }
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType; 
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }




        /**/
        /// <summary> 
        /// 执行存储过程 
        /// </summary> 
        /// <param name="storedProcName">存储过程名</param> 
        /// <param name="parameters">存储过程参数</param> 
        /// <returns>SqlDataReader</returns> 
        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(ConnectionString1);
            SqlDataReader returnReader;
            connection.Open();
            using (SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters))
            {
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }
        }


        /**/
        /// <summary> 
        /// 执行存储过程 
        /// </summary> 
        /// <param name="storedProcName">存储过程名</param> 
        /// <param name="parameters">存储过程参数</param> 
        /// <param name="tableName">DataSet结果中的表名</param> 
        /// <returns>DataSet</returns> 
        public static DataSet RunProceduredb(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                DataSet dataSet = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter sqlDA = new SqlDataAdapter();
                    sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                    sqlDA.Fill(dataSet, tableName);
                }
                catch (Exception e)
                {
                    //Common.Log.WriteLog(e.ToString(), 1);
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }

                return dataSet;
            }
        }


        /**/
        /// <summary> 
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值) 
        /// </summary> 
        /// <param name="connection">数据库连接</param> 
        /// <param name="storedProcName">存储过程名</param> 
        /// <param name="parameters">存储过程参数</param> 
        /// <returns>SqlCommand</returns> 
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        /**/
        /// <summary> 
        /// 执行存储过程，返回影响的行数         
        /// </summary> 
        /// <param name="storedProcName">存储过程名</param> 
        /// <param name="parameters">存储过程参数</param> 
        /// <param name="rowsAffected">影响的行数</param> 
        /// <returns></returns> 
        public static int RunProcedureexe(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            int result = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                    rowsAffected = command.ExecuteNonQuery();
                    result = (int)command.Parameters["ReturnValue"].Value;
                    return result;
                }
                catch (Exception ex)
                {
                    //Common.Log.WriteLog(ex.ToString(), 1);
                    throw ex;
                    //rowsAffected = 0;
                    //return 0;
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                    connection.Dispose();
                }

            }
        }



        public static int RunExecuteScalar(string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                object o = command.ExecuteScalar();
                connection.Close();
                connection.Dispose();
                return o == null ? 0 : Convert.ToInt32(o);
            }
        }

        /**/
        /// <summary> 
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)     
        /// </summary> 
        /// <param name="storedProcName">存储过程名</param> 
        /// <param name="parameters">存储过程参数</param> 
        /// <returns>SqlCommand 对象实例</returns> 
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        public static int indentityid(string SQLString)
        {
            SQLString = GetSelectText(SQLString);
            using (SqlConnection connection = new SqlConnection(ConnectionString1))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        connection.Open();
                        int rows = Convert.ToInt32(cmd.ExecuteScalar());
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        //Common.Log.WriteLog(ex.ToString(), 1);
                        connection.Close();
                        throw;
                        //    throw new Exception(E.Message); 
                    }
                    finally
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 构造select脚本
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>
        private static string GetSelectText(string strsql)
        {
            if (strsql.ToLower().IndexOf("with(nolock)") > 0)
            {
                return strsql;
            }
            if (strsql.ToLower().IndexOf(" on ") > 0)
            {
                strsql = strsql.Replace(" on ", " with(nolock) on ").Insert(strsql.ToLower().IndexOf(" join ") - 5, " with(nolock)");
            }
            else
            {
                if (strsql.ToLower().IndexOf(" where ") > 0)
                {
                    strsql = strsql.Replace(" where ", " with(nolock) where ");
                }
                else if (strsql.ToLower().IndexOf(" group by ") > 0)
                {
                    strsql = strsql.Replace(" group by ", " with(nolock) group by ");
                }
                else if (strsql.ToLower().IndexOf(" order by ") > 0)
                {
                    strsql = strsql.Replace(" order by ", " with(nolock) order by ");
                }
                else
                {
                    strsql = strsql + " with(nolock)";
                }
            }
            return strsql;
        }
    }
}
