using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace PickupAPi.Models
{
    public static class SqlHelper
    {
        public static readonly string connectionString =
            ConfigurationManager.AppSettings["DBCS"];

        private const int DefaultCommandTimeoutSeconds = 60;
        private const int DefaultBulkTimeoutSeconds = 120;

        public static int ExecuteNonQuery(ref SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                cmd.Connection = connection;
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                connection.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> NewExecuteNonQueryAsync(SqlCommand cmd, string spName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    cmd.Connection = connection;
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                    await connection.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 0;
            }
        }

        public static int ExecuteNonQueryInTrans(SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                    try
                    {
                        int result = cmd.ExecuteNonQuery();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static object ExecuteScalerInTrans(SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                    try
                    {
                        object result = cmd.ExecuteScalar();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static object ExecuteScaler(ref SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                cmd.Connection = connection;
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                connection.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static object Execute(string paramName, string paramValue, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(spName, connection))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    cmd.Parameters.Add(paramName, SqlDbType.NVarChar, 50).Value = paramValue ?? (object)DBNull.Value;
                }

                var ds = new DataSet();
                adapter.Fill(ds, "CRMTable");
                return ds.Tables["CRMTable"];
            }
        }

        public static DataTable Execute(ref SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var adapter = new SqlDataAdapter())
            {
                cmd.Connection = connection;
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                adapter.SelectCommand = cmd;

                var ds = new DataSet();
                adapter.Fill(ds, "CRMTable");
                return ds.Tables["CRMTable"];
            }
        }

        public static DataSet ExecuteDS(ref SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var adapter = new SqlDataAdapter())
            {
                cmd.Connection = connection;
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                adapter.SelectCommand = cmd;

                var ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public static DataSet ExecuteDSInTransaction(SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                using (var adapter = new SqlDataAdapter())
                {
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                    adapter.SelectCommand = cmd;

                    try
                    {
                        var ds = new DataSet();
                        adapter.Fill(ds);
                        transaction.Commit();
                        return ds;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static DataTable Execute(ref SqlCommand cmd, ref string strErr, string spName)
        {
            try
            {
                return Execute(ref cmd, spName);
            }
            catch (Exception ex)
            {
                strErr = ex.Message;
                throw;
            }
        }

        public static object Execute(string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(spName, connection))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                var ds = new DataSet();
                adapter.Fill(ds, "CRMTable");
                return ds.Tables["CRMTable"];
            }
        }

        public static DataTable ExecuteSQLQuery(string sqlQuery)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sqlQuery, connection))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                var ds = new DataSet();
                adapter.Fill(ds, "CRMTable");
                return ds.Tables["CRMTable"];
            }
        }

        public static void ExecuteBulk(DataTable sourceTable, string destinationTable)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.BulkCopyTimeout = DefaultBulkTimeoutSeconds;
                bulkCopy.DestinationTableName = destinationTable;

                connection.Open();
                bulkCopy.WriteToServer(sourceTable);
            }
        }

        public static bool Truncate(string sourceTable)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("TRUNCATE TABLE " + sourceTable, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return false;
                }
            }
        }

        public static bool ExecuteBulkCopyOnDataTable(DataTable table, string sourceTable)
        {
            try
            {
                using (var bulkCopy = new SqlBulkCopy(connectionString))
                {
                    bulkCopy.BulkCopyTimeout = DefaultBulkTimeoutSeconds;
                    bulkCopy.DestinationTableName = sourceTable;
                    bulkCopy.WriteToServer(table);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public static async Task<DataSet> ExecuteDSAsync(SqlCommand cmd, string spName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var adapter = new SqlDataAdapter())
            {
                cmd.Connection = connection;
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = DefaultCommandTimeoutSeconds;

                adapter.SelectCommand = cmd;

                var ds = new DataSet();
                await connection.OpenAsync();
                await Task.Run(() => adapter.Fill(ds));
                return ds;
            }
        }

        public static async Task<DataSet> ExecuteDSWithRetryAsync(SqlCommand cmd, string spName, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await ExecuteDSAsync(cmd, spName);
                }
                catch (SqlException ex) when ((ex.Number == -2 || ex.Number == 53 || ex.Number == 40) && i < maxRetries - 1)
                {
                    await Task.Delay((i + 1) * 1000);
                }
            }

            throw new Exception($"Failed to execute {spName} after {maxRetries} attempts.");
        }

        public static void ErrorLogging(string msg)
        {
            try
            {
                string path = @"C:\Error\Log.txt";
                string dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(path))
                    File.Create(path).Dispose();

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("===========Start============= " + DateTime.Now);
                    sw.WriteLine("Error Message: " + msg);
                    sw.WriteLine("Stack Trace: " + msg);
                    sw.WriteLine("===========End============= " + DateTime.Now);
                }
            }
            catch
            {
            }
        }

        private static void LogError(Exception ex)
        {
            try
            {
                string logPath = @"H:\logs\log.txt";
                string directory = Path.GetDirectoryName(logPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string logMessage =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                    $"Message: {ex.Message}{Environment.NewLine}" +
                    $"{(ex.InnerException != null ? "Inner Exception: " + ex.InnerException.Message + Environment.NewLine : string.Empty)}" +
                    $"Stack Trace: {ex.StackTrace}{Environment.NewLine}" +
                    "----------------------------------------" + Environment.NewLine;

                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
            }
        }
    }
}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Threading.Tasks;

//namespace PickupAPi.Models
//{
//    public class SqlHelper
//    {
//        //private static string strConnectionString = ConfigurationManager.ConnectionStrings["LPTDBCS"].ConnectionString;
//        ////private static string strConnectionString = ConfigurationManager.ConnectionStrings["LPTDBNW"].ConnectionString;
//        ////static string connectionString = EncryptDecrypt.GetDecryptedString(strConnectionString); //ADDED BY Narendra on 6-03-2017
//        public static string connectionString = ConfigurationManager.AppSettings["DBCS"].ToString();
//        // static string connectionString = strConnectionString;
//        //  static string connectionString = EncryptDecrypt.GetDecryptedString("Lm4wW6qDKRViAanv03xdBW0/aMAXe/abISJjPxr+KCux0iEXiG3S0H445kuiErFQarZX3zS4bVqqAvYCvBnUYPfwFVpmzTgZrR0i5g1OnSdLtwJfaJ+zJTPYGSSNuY9JBji8djXQvnBXgkHmbdkU67ffM1FSjnTr"); 
//        //  private static string connectionString = "Data Source=EDEMUMUATVM3;Initial Catalog=LPT_UAT;Persist Security Info=True;User ID=LPTUATUSER;Password=LPTUATUSER";// CASSecurityLib.CASSecurity.DESDecrypt(ConfigurationManager.AppSettings["LPTDBCS"].ToString());

//        static SqlHelper()
//        {
//        }
//        /// <summary>
//        /// Returns numbers of rows affected
//        /// </summary>
//        /// <param name="cmd">SqlCommand Object</param>
//        /// <param name="spName">Stored Procedure Name</param>
//        /// <returns></returns>
//        /// 
//        //public static int ExecuteNonQuery(ref SqlCommand cmd, string spName)
//        //{
//        //    int result = 0;

//        //    // Use the connection and command within a using statement to ensure proper disposal
//        //    using (SqlConnection _cnn = new SqlConnection(connectionString))
//        //    {
//        //        try
//        //        {
//        //            // Open connection
//        //            _cnn.Open();
//        //            cmd.Parameters.Clear();
//        //            // Set up the command
//        //            cmd.Connection = _cnn;
//        //            cmd.CommandText = spName;
//        //            cmd.CommandTimeout = 0; // This means no timeout
//        //            cmd.CommandType = CommandType.StoredProcedure;

//        //            // Execute the command
//        //            result = cmd.ExecuteNonQuery();
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            // Log or handle exception here if necessary
//        //            //throw;  // Preserve the stack trace
//        //            using (var file = new StreamWriter(@"H:\logs\" + "log.txt", true))
//        //            {
//        //                file.WriteLine("Testing");
//        //                file.WriteLine(ex.Message);
//        //                file.WriteLine(ex.StackTrace);
//        //                file.Close();
//        //            }
//        //        }
//        //    }

//        //    return result;
//        //}

//        public async Task<int> NewExecuteNonQueryAsync(SqlCommand cmd,string spName)
//        {
//            try
//            {
//                using (SqlConnection cnn = new SqlConnection(connectionString))
//                {
//                    await cnn.OpenAsync();

//                    using (cmd)
//                    {
//                        cmd.Connection = cnn;
//                        cmd.CommandText = spName;
//                        cmd.CommandTimeout = 0; // Consider setting a reasonable timeout to avoid indefinite hanging.
//                        cmd.CommandType = CommandType.StoredProcedure;

//                        await cmd.ExecuteNonQueryAsync();
//                    }
//                }

//                return 1; // Success
//            }
//            catch (Exception ex)
//            {
//                LogError(ex);
//                return 0; // Failure
//            }
//        }

//        private void LogError(Exception ex)
//        {
//            try
//            {
//                string logPath = @"H:\logs\log.txt";
//                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
//                                    $"Message: {ex.Message}\r\n" +
//                                    (ex.InnerException != null
//                                        ? $"Inner Exception: {ex.InnerException.Message}\r\n"
//                                        : string.Empty) +
//                                    $"Stack Trace: {ex.StackTrace}\r\n" +
//                                    "----------------------------------------\r\n";

//                string directory = Path.GetDirectoryName(logPath);
//                if (!Directory.Exists(directory))
//                {
//                    Directory.CreateDirectory(directory);
//                }

//                File.AppendAllText(logPath, logMessage);
//            }
//            catch
//            {
//                // Handle exceptions during logging (optional)
//            }
//        }


//        public static int ExecuteNonQuery(ref SqlCommand cmd, string spName)
//        {
//            using (var connection = new SqlConnection(connectionString))
//            using (var command = cmd)
//            {
//                command.Connection = connection;
//                command.CommandText = spName;
//                command.CommandTimeout = 0;
//                command.CommandType = CommandType.StoredProcedure;

//                try
//                {
//                    connection.Open();
//                    return command.ExecuteNonQuery();
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//            }
//        }

//        public static int ExecuteNonQueryInTrans(SqlCommand cmd, string spName)
//        {
//            SqlCommand _cmd = null;
//            int _result = 0;

//            SqlConnection _cnn = null;
//            _cmd = new SqlCommand();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }

//            SqlTransaction objTran = _cnn.BeginTransaction();

//            _cmd = cmd;
//            _cmd.Transaction = objTran;
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;
//            try
//            {
//                _result = _cmd.ExecuteNonQuery();
//                objTran.Commit();
//                _cnn.Close();
//            }
//            catch (Exception ex)
//            {
//                objTran.Rollback();
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                GC.Collect();
//            }
//            return _result;


//        }

//        public static object ExecuteScalerInTrans(SqlCommand cmd, string spName)
//        {
//            object _result = "";
//            using (SqlConnection con = new SqlConnection(connectionString))
//            {
//                con.Open();
//                SqlTransaction objTran = con.BeginTransaction();

//                try
//                {
//                    cmd.Connection = con;
//                    cmd.Transaction = objTran;
//                    _result = cmd.ExecuteScalar();
//                    objTran.Commit();
//                    con.Close();
//                }
//                catch (Exception ex)
//                {
//                    objTran.Rollback();
//                    throw;
//                }
//                finally
//                {
//                    cmd.Dispose();
//                    GC.Collect();
//                }
//            }
//            return _result;
//        }

//        public static object ExecuteScaler(ref SqlCommand cmd, string spName)
//        {
//            SqlCommand _cmd = null;
//            object _result = "";
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = new SqlCommand();
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            _cmd = cmd;
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;
//            try
//            {
//                _result = _cmd.ExecuteScalar();
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _result;

//        }

//        public static object Execute(string paramName, string ParamValue, string spName)
//        {
//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = new SqlCommand();
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }

//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;

//            if ((paramName != null))
//            {
//                _cmd.Parameters.Add(paramName, SqlDbType.NVarChar, 50).Value = ParamValue;
//            }

//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds, "CRMTable");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds.Tables["CRMTable"];
//        }

//        public static DataTable Execute(ref SqlCommand cmd, string spName)
//        {
//            using (var connection = new SqlConnection(connectionString))
//            using (var command = cmd)
//            using (var adapter = new SqlDataAdapter(command))
//            {
//                command.Connection = connection;
//                command.CommandText = spName;
//                command.CommandTimeout = 0;
//                command.CommandType = CommandType.StoredProcedure;

//                var ds = new DataSet();
//                try
//                {
//                    adapter.Fill(ds, "CRMTable");
//                    return ds.Tables["CRMTable"];
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//            }
//        }

//        public static DataSet ExecuteDS(ref SqlCommand cmd, string spName)
//        {

//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = cmd;
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            _cmd = cmd;
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;
//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds);
//            }
//            catch (Exception ex)
//            {
//                _cmd.Dispose();
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds;
//        }

//        public static DataSet ExecuteDSInTransaction(SqlCommand cmd, string spName)
//        {

//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = cmd;
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);


//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            SqlTransaction objSqlTran = _cnn.BeginTransaction();
//            _cmd = cmd;
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;
//            _cmd.Transaction = objSqlTran;
//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds);
//                objSqlTran.Commit();
//            }
//            catch (Exception ex)
//            {
//                objSqlTran.Rollback();
//                _cmd.Dispose();
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds;
//        }

//        public static DataTable Execute(ref SqlCommand cmd, ref string strErr, string spName)
//        {
//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = cmd;
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            _cmd = cmd;
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.StoredProcedure;
//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds, "CRMTable");
//            }
//            catch (Exception ex)
//            {
//                _cmd.Dispose();


//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds.Tables["CRMTable"];
//        }

//        public static object Execute(string spName)
//        {
//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = new SqlCommand();
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = spName;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.Text;
//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds, "CRMTable");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds.Tables["CRMTable"];
//        }

//        public static DataTable ExecuteSQLQuery(string sqlQuery)
//        {
//            SqlCommand _cmd = null;
//            SqlDataAdapter _adapter = null;
//            DataSet _ds = null;
//            SqlConnection _cnn = null;
//            _cmd = new SqlCommand();
//            _adapter = new SqlDataAdapter();
//            _ds = new DataSet();
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }
//            _cmd.Connection = _cnn;
//            _cmd.CommandText = sqlQuery;
//            _cmd.CommandTimeout = 0;
//            _cmd.CommandType = CommandType.Text;
//            _adapter.SelectCommand = _cmd;
//            _ds = new DataSet();
//            try
//            {
//                _adapter.Fill(_ds, "CRMTable");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                _cmd.Dispose();
//                _adapter.Dispose();
//                GC.Collect();
//            }

//            return _ds.Tables["CRMTable"];
//        }

//        public static void ExecuteBulk(DataTable sourceTable, string distinationTable)
//        {
//            SqlConnection _cnn = null;
//            _cnn = new SqlConnection(connectionString);
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }

//            try
//            {
//                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_cnn))
//                {
//                    bulkCopy.BulkCopyTimeout = 0;
//                    bulkCopy.DestinationTableName = distinationTable;
//                    bulkCopy.WriteToServer(sourceTable);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                GC.Collect();
//            }
//        }

//        public static bool Truncate(string sourceTable)
//        {
//            SqlConnection _cnn = null;
//            _cnn = new SqlConnection(connectionString);
//            SqlCommand cmd = null;
//            cmd = new SqlCommand();
//            if ((_cnn.State == ConnectionState.Open))
//            {
//                _cnn.Close();
//            }
//            else
//            {
//                _cnn.Open();
//            }

//            cmd.CommandText = "Truncate table " + sourceTable;
//            cmd.CommandType = CommandType.Text;
//            cmd.Connection = _cnn;
//            try
//            {
//                cmd.ExecuteNonQuery();

//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            finally
//            {
//                if ((_cnn.State == ConnectionState.Open))
//                {
//                    _cnn.Close();
//                }
//                _cnn.Dispose();
//                GC.Collect();
//            }
//            return true;
//        }

//        public static bool ExecuteBulkCopy(string sourceFile, string sourceTable)
//        {
//            try
//            {

//                System.Data.OleDb.OleDbConnection MyConnection = null;
//                string query = "provider=Microsoft.Jet.OLEDB.4.0; Data Source='" + sourceFile + "'; Extended Properties='text;HDR=Yes;FMT=Delimited(,)';";
//                MyConnection = new System.Data.OleDb.OleDbConnection(query);
//                System.Data.OleDb.OleDbCommand MyCommand = new System.Data.OleDb.OleDbCommand("SELECT * FROM [{0}]", MyConnection);
//                MyConnection.Open();
//                System.Data.OleDb.OleDbDataReader rdr = MyCommand.ExecuteReader();
//                SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString);
//                bulkCopy.BulkCopyTimeout = 0;
//                bulkCopy.DestinationTableName = sourceTable;
//                bulkCopy.WriteToServer(rdr);
//                rdr.Close();

//                MyConnection.Close();

//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }

//        public static bool ExecuteBulkCopyOnDataTable(DataTable table, string sourceTable)
//        {
//            try
//            {
//                SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString);
//                bulkCopy.BulkCopyTimeout = 0;
//                bulkCopy.DestinationTableName = sourceTable;
//                bulkCopy.WriteToServer(table);


//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            return true;
//        }

//        public static bool ExecuteBulkCopy_xlsx(string sourceFile, string sourceTable)
//        {
//            try
//            {
//                System.Data.OleDb.OleDbConnection MyConnection = null;
//                string query = "provider=Microsoft.ACE.OLEDB.12.0; Data Source='" + sourceFile + "'; Extended Properties=\"Excel 12.0 Xml;HDR=Yes\\\"";
//                MyConnection = new System.Data.OleDb.OleDbConnection(query);
//                System.Data.OleDb.OleDbCommand MyCommand = new System.Data.OleDb.OleDbCommand("select * from [Sheet1$]", MyConnection);
//                MyConnection.Open();

//                using (System.Data.OleDb.OleDbDataReader rdr = MyCommand.ExecuteReader())
//                {
//                    SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString);
//                    bulkCopy.BulkCopyTimeout = 0;
//                    bulkCopy.DestinationTableName = sourceTable;
//                    bulkCopy.WriteToServer(rdr);
//                    rdr.Close();
//                }
//                MyConnection.Close();

//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//            finally
//            {
//                GC.Collect();

//            }
//            return true;
//        }

//        public static void ErrorLogging(string msg)
//        {
//            string strPath = @"C:\Error\Log.txt";
//            if (!File.Exists(strPath))
//            {
//                File.Create(strPath).Dispose();
//            }
//            using (StreamWriter sw = File.AppendText(strPath))
//            {
//                sw.WriteLine("=============Error Logging ===========");
//                sw.WriteLine("===========Start============= " + DateTime.Now);
//                sw.WriteLine("Error Message: " + msg);
//                sw.WriteLine("Stack Trace: " + msg);
//                sw.WriteLine("===========End============= " + DateTime.Now);

//            }
//        }

//        public static async Task<DataSet> ExecuteDSAsync(SqlCommand cmd, string spName)
//        {
//            using (var connection = new SqlConnection(connectionString))
//            using (var command = cmd)
//            using (var adapter = new SqlDataAdapter(command))
//            {
//                command.Connection = connection;
//                command.CommandText = spName;
//                command.CommandTimeout = 0;
//                command.CommandType = CommandType.StoredProcedure;

//                var ds = new DataSet();
//                try
//                {
//                    await connection.OpenAsync();
//                    await Task.Run(() => adapter.Fill(ds));
//                    return ds;
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//            }
//        }

//        public static async Task<DataSet> ExecuteDSWithRetryAsync(SqlCommand cmd, string spName, int maxRetries = 3)
//        {
//            for (int i = 0; i < maxRetries; i++)
//            {
//                try
//                {
//                    return await ExecuteDSAsync(cmd, spName);
//                }
//                catch (SqlException ex) when (
//                    (ex.Number == -2 || ex.Number == 53 || ex.Number == 40) && 
//                    i < maxRetries - 1)
//                {
//                    // Wait before retrying, with exponential backoff
//                    await Task.Delay((i + 1) * 1000);
//                    continue;
//                }
//            }
//            throw new Exception($"Failed to execute {spName} after {maxRetries} attempts");
//        }

//        private static int _activeConnections = 0;

//        public static async Task MonitorConnectionPoolAsync()
//        {
//            while (true)
//            {
//                using (var connection = new SqlConnection(connectionString))
//                using (var command = new SqlCommand(@"
//                    SELECT 
//                        DB_NAME(dbid) as DatabaseName,
//                        COUNT(dbid) as NumberOfConnections,
//                        loginame as LoginName
//                    FROM sys.sysprocesses
//                    WHERE dbid > 0
//                    GROUP BY dbid, loginame", connection))
//                {
//                    await connection.OpenAsync();
//                    using (var reader = await command.ExecuteReaderAsync())
//                    {
//                        while (await reader.ReadAsync())
//                        {
//                            // Log connection pool statistics
//                            string logMessage = $"Database: {reader["DatabaseName"]}, " +
//                                              $"Connections: {reader["NumberOfConnections"]}, " +
//                                              $"Login: {reader["LoginName"]}";
//                            await LogAsync(logMessage);
//                        }
//                    }
//                }
//                await Task.Delay(TimeSpan.FromMinutes(1));
//            }
//        }

//        private static async Task LogAsync(string message)
//        {
//            string logPath = @"H:\logs\connectionpool.txt";
//            string directory = Path.GetDirectoryName(logPath);

//            if (!Directory.Exists(directory))
//            {
//                Directory.CreateDirectory(directory);
//            }

//            using (var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read))
//            using (var streamWriter = new StreamWriter(fileStream))
//            {
//                await streamWriter.WriteAsync(
//                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
//            }
//        }

//    }
//}