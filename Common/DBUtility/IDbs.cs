using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Common
{
    public interface IDbs
    {
        bool ColumnExists(string tableName, string columnName);
        bool Exists(string strSql);
        bool TabExists(string TableName);
        bool Exists(string strSql, params SqlParameter[] cmdParms);
        int ExecuteSql(string SQLString);
        int ExecuteSqlByTime(string SQLString, int Times);
        int ExecuteSqlTran(List<String> SQLStringList);
        int ExecuteSql(string SQLString, string content);
        int ExecuteSqlInsertImg(string strSQL, byte[] fs);
        int ExecuteSql(string SQLString, params SqlParameter[] cmdParms);
        int ExecuteSqlTrans(List<DbData> aSQLs);
        int GetMaxID(string FieldName, string TableName);
        object GetSingle(string SQLString);
        object GetSingle(string SQLString, int Times);
        object GetSingle(string SQLString, params SqlParameter[] cmdParms);
        DataSet Query(string SQLString);
        DataSet Query(string SQLString, int Times);
        DataSet Query(string SQLString, params SqlParameter[] cmdParms);
        DataSet ExecuteSqlGet(string SQLString, string content);
        DataSet RunProcedureNew(string storedProcName, params IDataParameter[] parameters);
        DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times);
        int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected);
        int BatchOperateDATA(string SQLString, int index, int total);
        int BatchOperateDLDATA(string SQLString, int index, int total);
        int BatchOperateSelectDLDATA(string SQLString, int index, int total);
        int BatchOperateSelectDATA(string SQLString, int index, int total);
    }

    public class DbData
    {
        public string SQL;
        public SqlParameter[] Params;
    }
}
