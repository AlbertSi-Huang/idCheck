using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlLibrary
{
    public enum EKeyType
    {
        NOT,
        PRI,    //主键约束
        UNI,    //唯一约束
    }

    public struct TableColumnParam<ColumnEnum> where ColumnEnum : struct
    {
        public string Name { get; private set; }
        public string MySqlType { get; private set; }
        public bool IsNullable { get; private set; }
        public EKeyType KeyType { get; private set; }

        public object DefautValue { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public string[] OldNames { get; private set; }

        public TableColumnParam(ColumnEnum col, string mySqlType, bool isNullable = true, 
            EKeyType keyType = EKeyType.NOT, object defaultValue = null, bool isAutoIncrement = false,
            string[] oldNames = null)
        {
            this.Name = Enum.ToObject(typeof(ColumnEnum), col).ToString();
            this.MySqlType = mySqlType;
            this.IsNullable = isNullable;
            this.KeyType = keyType;
            this.DefautValue = defaultValue;
            this.IsAutoIncrement = isAutoIncrement;
            this.OldNames = oldNames;
        }
    }

    public enum AggregateFunction
    {
        none,   //無，默認取第一個
        count,  //數據個數
        sum,    //總數
        avg,    //平均值
        min,    //最小值
        max,    //最大值
    }

    public enum ConditionOperator
    {
        Equals,             //等于
        NotEquals,          //不等于
        LessThan,           //小于
        LessThanOrEquals,   //小于或等于
        GreaterThan,        //大于
        GreaterOrEquals,    //大于或等于
        Like,               //通配（%、_）
        In,                 //in(x,y,z,...)
        NotIn,              //not in(x,y,z,...)
    }

    public enum ConditionRelation
    {
        and,
        or,
    }

    public class QueryCondition<ColumnEnum> where ColumnEnum : struct
    {
        //public ColumnEnum Column { get; set; }
        public string ColumnName { get; set; }
        public object Value { get; set; }
        public string Operator { get; set; }
        public ConditionOperator _Operator { get; private set; }

        public QueryCondition()
        {
            Init(default(ColumnEnum), null, ConditionOperator.Equals);
        }

        public QueryCondition(ColumnEnum column, object value, ConditionOperator _operator = ConditionOperator.Equals)
        {
            Init(column, value, _operator);
        }

        private void Init(ColumnEnum column, object value, ConditionOperator _operator = ConditionOperator.Equals)
        {
            ColumnName = Enum.ToObject(typeof(ColumnEnum), column).ToString();
            Value = value;
            _Operator = _operator;
            switch (_operator)
            {
                case ConditionOperator.Equals: Operator = "="; break;
                case ConditionOperator.NotEquals: Operator = "!="; break;
                case ConditionOperator.LessThan: Operator = "<"; break;
                case ConditionOperator.LessThanOrEquals: Operator = "<="; break;
                case ConditionOperator.GreaterThan: Operator = ">"; break;
                case ConditionOperator.GreaterOrEquals: Operator = ">="; break;
                case ConditionOperator.Like: Operator = " like "; break;
                case ConditionOperator.In: Operator = " in "; break;
                case ConditionOperator.NotIn: Operator = " not in "; break;
                default: Operator = "="; break;
            }
        }
    }

    public class QueryCondition
    {
        public string ColumnFullName { get; private set; }
        public object Value { get; private set; }
        public string Operator { get; private set; }
        public ConditionOperator _Operator { get; private set; }

        public QueryCondition(string columnFullName, object value, ConditionOperator _operator = ConditionOperator.Equals)
        {
            Init(columnFullName, value, _operator);
        }

        public QueryCondition(string databaseName, string tableName, string columnName, 
            object value, ConditionOperator _operator = ConditionOperator.Equals)
        {
            string columnFullName = databaseName + "." + tableName + "." + columnName;
            Init(columnFullName, value, _operator);
        }

        private void Init(string columnFullName, object value, ConditionOperator _operator)
        {
            ColumnFullName = columnFullName;
            Value = value;
            _Operator = _operator;
            switch (_operator)
            {
                case ConditionOperator.Equals: Operator = "="; break;
                case ConditionOperator.NotEquals: Operator = "!="; break;
                case ConditionOperator.LessThan: Operator = "<"; break;
                case ConditionOperator.LessThanOrEquals: Operator = "<="; break;
                case ConditionOperator.GreaterThan: Operator = ">"; break;
                case ConditionOperator.GreaterOrEquals: Operator = ">="; break;
                case ConditionOperator.Like: Operator = " like "; break;
                case ConditionOperator.In: Operator = " in "; break;
                case ConditionOperator.NotIn: Operator = " not in "; break;
                default: Operator = "="; break;
            }
        }
    }

    public class ColumnName
    {
        public string ColFullName;
        public string ColAsName;

        public ColumnName(string colFullName, string colAsName = null)
        {
            ColFullName = colFullName;
            ColAsName = colAsName;
        }
    }

    public enum JoinType
    {
        InnerJoin,
        LeftJoin,
        RightJoin,
    }

    public class OnCondition
    {
        public string Table1ColumnFullName;
        public string Table2ColumnFullName;

        public OnCondition(string table1ColumnFullName, string table2ColumnFullName)
        {
            Table1ColumnFullName = table1ColumnFullName;
            Table2ColumnFullName = table2ColumnFullName;
        }
    }

    public class Join
    {
        public string JoinSql;
        public string AsName { get; private set; }

        public Join(JoinType joinType, string tableFullName, List<OnCondition> onConditions, 
            ConditionRelation relation = ConditionRelation.and)
        {
            Load(joinType, tableFullName, null, onConditions, relation);
        }

        public Join(JoinType joinType, string tableFullName, string asName, List<OnCondition> onConditions,
            ConditionRelation relation = ConditionRelation.and)
        {
            Load(joinType, tableFullName, asName, onConditions, relation);
        }

        private void Load(JoinType joinType, string tableFullName, string asName, 
            List<OnCondition> onConditions, ConditionRelation relation = ConditionRelation.and)
        {
            AsName = asName;
            JoinSql = string.Empty;
            switch (joinType)
            {
                case JoinType.InnerJoin: JoinSql = " INNER JOIN "; break;
                case JoinType.LeftJoin: JoinSql = " LEFT JOIN"; break;
                case JoinType.RightJoin: JoinSql = " RIGHT JOIN "; break;
                default: return;
            }

            if (onConditions != null && onConditions.Count > 0)
            {
                JoinSql += " " + tableFullName;
                if (!string.IsNullOrEmpty(asName))
                {
                    JoinSql += " as " + asName;
                }
                JoinSql += " on ";
                for (int i = 0; i < onConditions.Count; i++)
                {
                    var oc = onConditions[i];
                    if (i != 0) JoinSql += relation.ToString() + " ";
                    JoinSql += oc.Table1ColumnFullName + "=" + oc.Table2ColumnFullName + " ";
                }
            }
        }
    }

    public class TableHandle<ColumnEnum> where ColumnEnum : struct
    {
        #region 属性、字段
        public string databaseName { get; private set; }
        public string tableName { get; private set; }
        private TableColumnParam<ColumnEnum>[] Columns;
        private List<string> keyColumnNames;
        private HashSet<string> AutoIncColumnNames;

        // 定義一個標識確保線程同步
        private readonly object locker = new object();
        #endregion

        #region 构造、公共方法
        public TableHandle(string tn, string dn, TableColumnParam<ColumnEnum>[] columns)
        {
            tableName = tn;
            databaseName = dn;
            Columns = columns;
            //keyColumnNames = columns.Where(c => 
            //    c.MySqlType.ToLower().Contains("key")).Select(c => c.Name).ToList();
            //AutoIncColumnNames = new HashSet<string>(columns.Where(c => 
            //    c.MySqlType.ToLower().Contains("auto_increment")).Select(c => c.Name));
            keyColumnNames = columns.Where(c =>
                c.KeyType != EKeyType.NOT).Select(c => c.Name).ToList();
            AutoIncColumnNames = new HashSet<string>(columns.Where(c =>
                c.IsAutoIncrement).Select(c => c.Name));
        }

        public string TableFullName()
        {
            return this.databaseName + "." + this.tableName;
        }

        public string ColumnFullName(ColumnEnum column, string tableAsName = null)
        {
            if(string.IsNullOrEmpty(tableAsName))
                return this.databaseName + "." + this.tableName + "." + column.ToString();
            else
                return tableAsName + "." + column.ToString();
        }

        /// <summary>
        /// 獲取一個空表,包含該數據表的所有列（字段）名
        /// </summary>
        /// <returns></returns>
        public DataTable NewDataTable(string name = "")
        {
            DataTable dt = new DataTable(name);
            foreach (var v in Columns)
            {
                dt.Columns.Add(v.Name);
            }

            return dt;
        }

        public void SetValue<T>(DataRow dr, ColumnEnum column, T value)
        {
            try
            {
                Type type = typeof(T);
                object obj;
                if (type.IsEnum)
                    obj = Convert.ToInt32(value);
                else
                    obj = value;
                dr[column.ToString()] = DbHandle.ToDbType(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T GetValue<T>(DataRow dr, ColumnEnum column, T defValue = default(T))
        {
            return GetValue(dr, column.ToString(), defValue);
        }

        public object GetValue(DataRow dr, ColumnEnum column)
        {
            try { return dr[column.ToString()]; }
            catch (Exception ex) { throw ex; }
        }

        public T GetValue<T>(DataRow dr, ColumnName columnName, T defValue = default(T))
        {
            string name = string.IsNullOrEmpty(columnName.ColAsName) ?
                columnName.ColFullName.Split('.').Last() : columnName.ColAsName;
            return GetValue(dr, name, defValue);
        }

        public T GetValue<T>(DataRow dr, string columnName, T defValue = default(T))
        {
            try
            {
                T result;
                object value = dr[columnName];
                if (Convert.IsDBNull(value))
                    result = defValue;
                else
                {
                    Type type = typeof(T);
                    if (type.IsEnum)
                        result = (T)Enum.ToObject(type, value);
                    else if (type.Equals(typeof(string)))
                        result = (T)((object)value.ToString());
                    else
                        result = (T)value;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 初始化表
        public bool CreateTable()
        {
            if (Columns.Count() <= 0)
            {
                return false;
            }

            bool rel = false;
            lock (locker)
            {
                if (IsTableExisted())
                {
                    DataTable dt = GetTableInfo();
                    rel = UpdateColumns(dt);
                }
                else
                {
                    rel = AddTable();
                }

                return rel;
            }
        }

        private bool AddTable()
        { 
            MySqlParameter[] paras =
            {
                new MySqlParameter(),
            };
            //string strSql = @"create table if not exists  " + databaseName + "." + tableName + @"(
            //                            cardNum varchar(32) PRIMARY KEY NOT NULL,
            //                            name varchar(48),sex varchar(4),birthday varchar(16),
            //                            nation varchar(16),address varchar(128),issue varchar(64),dateStart varchar(16),
            //                            dateEnd varchar(16),photo varchar(256))ENGINE=InnoDB COLLATE=utf8mb4_bin";
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("create table if not exists  ");
            sbSql.Append(databaseName).Append(".").Append(tableName).Append("(");
            for (int i = 0; i < Columns.Count(); i++)
            {
                if (i > 0) sbSql.Append(",");
                var col = Columns[i];
                //sbSql.Append(col.Name).Append(" ").Append(col.MySqlType);
                LoadColumnInfo(col, ref sbSql);
            }
            sbSql.Append(")ENGINE=InnoDB COLLATE=utf8mb4_bin");   //utf8mb4_bin:以utf8mb4编码二进制方式存储，区分大小写

            //lock (locker)
            {
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    return false;
                }
            }

            return true;
        }

        private bool IsTableExisted()
        {
            //select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA='databaseName' and TABLE_NAME='tableName';
            MySqlParameter databaseNameParam = new MySqlParameter("@c_" + nameof(databaseName), databaseName);
            MySqlParameter tableNameParam = new MySqlParameter("@c_" + nameof(tableName), tableName);
            MySqlParameter[] paras = new MySqlParameter[] { databaseNameParam, tableNameParam };

            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("select TABLE_NAME from INFORMATION_SCHEMA.TABLES where");
            sbSql.Append(" TABLE_SCHEMA =").Append(databaseNameParam.ParameterName);
            sbSql.Append(" and TABLE_NAME=").Append(tableNameParam.ParameterName);

            DataTable tempTable = null;
            //lock (locker)
            {
                try
                {
                    tempTable = DbManager.Ins.ExcuteDataTable(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    tempTable = new DataTable();
                }

                if (tempTable != null && tempTable.Rows.Count > 0)
                    return true;
            }

            return false;
        }

        private DataTable GetTableInfo()
        {
            //desc databaseName.tableName;
            MySqlParameter[] paras = new MySqlParameter[] { };
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("desc ").Append(databaseName).Append(".").Append(tableName);

            DataTable tempTable = null;
            //lock (locker)
            {
                try
                {
                    tempTable = DbManager.Ins.ExcuteDataTable(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    tempTable = new DataTable();
                }

                return tempTable;
            }
        }

        private bool UpdateColumns(DataTable oldCols)
        {
            var newps = Columns.ToDictionary(c => c.Name);

            //更新原主键所在列
            UpdateOldPrimaryKeyCol(oldCols, newps);

            var newColNames = Columns.Select(c => c.Name);
            var noFindCols = new Dictionary<string, DataRow>();
            foreach(DataRow dr in oldCols.Rows)
            {
                string name = dr["Field"] as string;
                if(!newps.ContainsKey(name))
                {
                    //列名没找到
                    noFindCols.Add(name, dr);
                }
                else
                {
                    //列名已存在，比较参数
                    var col = newps[name];
                    if (IsParamsChanged(dr, col))
                    {
                        bool rel = ChangeColumn(name, col);
                        if (rel == false) return rel;
                    }
                    newps.Remove(name);
                }
            }

            //没找到的列处理
            foreach(var dr in noFindCols)
            {
                string name = dr.Key;
                var newCols = newps.Where(c => (c.Value.OldNames != null) && c.Value.OldNames.Contains(name)).ToList();
                if(newCols == null || newCols.Count() == 0)
                {
                    //列已删除
                    bool rel = RemoveColumn(name);
                    if (rel == false) return rel;
                }
                else
                {
                    //列名已修改
                    bool rel = ChangeColumn(name, newCols[0].Value);
                    if (rel == false) return rel;
                    newps.Remove(name);
                }
            }

            if (newps.Count > 0)
            {
                //添加新列
                string prevColName = null;
                foreach (var colName in newColNames)
                {
                    if (newps.ContainsKey(colName))
                    {
                        var col = newps[colName];
                        bool rel = AddColumn(col, string.IsNullOrEmpty(prevColName), prevColName);
                        if (rel == false) return rel;
                    }
                    prevColName = colName;
                }
            }

            return true;
        }

        private void UpdateOldPrimaryKeyCol(DataTable oldCols, Dictionary<string, TableColumnParam<ColumnEnum>> newps)
        {
            var oldPriKeyCols = oldCols.AsEnumerable().Where(d =>
                string.Equals(d.Field<string>("Key").ToUpper(), EKeyType.PRI.ToString()));
            if (oldPriKeyCols != null && oldPriKeyCols.Count() > 0)
            {
                DataRow dr = oldPriKeyCols.ToList()[0];
                string oldColName = dr["Field"] as string;
                var newCols = newps.Select(d => d.Value).Where(d =>
                    string.Equals(d.Name, oldColName) || (d.OldNames != null && d.OldNames.Contains(oldColName)));
                if (newCols == null || newCols.Count() <= 0)
                {
                    //删除原主键所在列
                    RemoveColumn(oldColName);
                    oldCols.Rows.Remove(dr);
                }
                else
                {
                    var newCol = newCols.ToList()[0];
                    if (newCol.KeyType != EKeyType.PRI)
                    {
                        bool isKeyChanged = false;
                        TableColumnParam<ColumnEnum> newCol2;
                        if (newCol.KeyType == EKeyType.NOT)
                            newCol2 = newCol;
                        else
                        {
                            isKeyChanged = true;
                            ColumnEnum col = (ColumnEnum)Enum.Parse(typeof(ColumnEnum), newCol.Name);
                            newCol2 = new TableColumnParam<ColumnEnum>(
                                col, newCol.MySqlType, newCol.IsNullable, EKeyType.NOT, 
                                newCol.DefautValue, newCol.IsAutoIncrement);
                        }

                        //修改原主键所在列
                        ChangeColumn(oldColName, newCol2);

                        //删除原主键
                        DropKey(" PRIMARY KEY ");
                        dr["Key"] = string.Empty;

                        if (isKeyChanged)
                            ChangeColumn(oldColName, newCol);
                    }
                    else
                    {
                        bool rel = IsParamsChanged(dr, newCol);
                        if(rel)
                        {
                            //其他参数被修改，sql语句中不能再含主键关键字
                            ColumnEnum col = (ColumnEnum)Enum.Parse(typeof(ColumnEnum), newCol.Name);
                            var newCol2 = new TableColumnParam<ColumnEnum>(
                                col, newCol.MySqlType, newCol.IsNullable, EKeyType.NOT, 
                                newCol.DefautValue, newCol.IsAutoIncrement);
                            ChangeColumn(oldColName, newCol2);
                        }
                    }

                    //该列不用再做后续处理
                    oldCols.Rows.Remove(dr);
                    newps.Remove(newCol.Name);
                }
            }
        }

        private bool IsParamsChanged(DataRow dr, TableColumnParam<ColumnEnum> col)
        {
            string type = dr["Type"] as string;
            if (!IsDataTypeEquals(type, col.MySqlType))
            {
                return true;
            }

            string isNullableText = dr["Null"] as string;
            bool isNullable = string.Equals(isNullableText.Trim().ToUpper(), "YES");
            if (isNullable != col.IsNullable)
            {
                return true;
            }

            string key = dr["Key"] as string;
            if(string.IsNullOrEmpty(key.Trim()))
            {
                if (col.KeyType != EKeyType.NOT)
                    return true;
            }
            else if(!string.Equals(key.Trim().ToUpper(), col.KeyType.ToString().ToUpper()))
            {
                //需要删除之前的约束
                string oldKeyInfo = " INDEX " + col.Name;
                if(string.Equals(key.Trim().ToUpper(), EKeyType.PRI.ToString().ToUpper()))
                {
                    oldKeyInfo = " PRIMARY KEY ";
                }
                DropKey(oldKeyInfo);

                if(col.KeyType != EKeyType.NOT)
                    return true;
            }

            object defValue = dr["Default"];
            if (!object.Equals(defValue, DbHandle.ToDbType(col.DefautValue)))
            {
                return true;
            }

            string extra = dr["Extra"] as string;
            bool isAutoIncrement = (!string.IsNullOrEmpty(extra))
                && extra.Trim().ToUpper().Contains("AUTO_INCREMENT");
            if (isAutoIncrement != col.IsAutoIncrement)
            {
                return true;
            }

            return false;
        }

        private bool IsDataTypeEquals(string oldType, string newType)
        {
            string ot = oldType.Trim().ToLower();
            string nt = newType.Trim().ToLower();
            if (string.Equals(ot, nt))  //date,time,datetime,timestemp,char
                return true;

            string[] ots = ot.Split(new char[] { '(', ',', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] nts = nt.Split(new char[] { '(', ',', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ot.Contains("int")) //tinyint,smallint,mediumint,int,bigint
            {
                if (nts.Count() <= 0)
                    return false;
                else if (ots[0] != nts[0])
                    return false;
                else if (!ot.Contains("unsigned"))
                    return true;
                else if (ots[ots.Count() - 1] == ots[ots.Count() - 1])
                    return true;
                else
                    return false;
            }
            if (ot.Contains("year")) //year(4)
            {
                if (ots[0] != nts[0])
                    return false;
                else
                    return true;
            }
            else    //float,double,varchar(n)...
            {
                if (ots.Count() != nts.Count())
                    return false;
                else
                {
                    for (int i = 0; i < ots.Count(); i++)
                    {
                        if (ots[i] != nts[i])
                            return false;
                    }
                    return true;
                }
            }
        }

        private StringBuilder LoadColumnInfo(TableColumnParam<ColumnEnum> col, ref StringBuilder sbSql)
        {
            sbSql.Append(" ").Append(col.Name);
            sbSql.Append(" ").Append(col.MySqlType).Append(" ");

            if (col.IsNullable == false)
                sbSql.Append(" NOT NULL ");

            switch (col.KeyType)
            {
                case EKeyType.PRI: sbSql.Append(" PRIMARY KEY "); break;
                case EKeyType.UNI: sbSql.Append(" UNIQUE KEY "); break;
                default: break;
            }

            if (col.DefautValue != null)
                sbSql.Append(" DEFAULT(").Append(DbHandle.ToDbType(col.DefautValue)).Append(") ");
            if (col.IsAutoIncrement)
                sbSql.Append(" AUTO_INCREMENT ");

            return sbSql;
        }

        private bool AddColumn(TableColumnParam<ColumnEnum> col, bool isFirstCol, string prevColName = null)
        {
            //ALTER TABLE table_name ADD field_name field_type;
            MySqlParameter[] paras = new MySqlParameter[] { };
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("ALTER TABLE ").Append(databaseName).Append(".").Append(tableName);
            sbSql.Append(" ADD ");
            LoadColumnInfo(col, ref sbSql);
            if(isFirstCol)
                sbSql.Append(" FIRST ");
            else if(!string.IsNullOrEmpty(prevColName))
                sbSql.Append(" AFTER ").Append(prevColName);

            //lock (locker)
            {
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    return false;
                }
            }

            return true;
        }

        private bool ChangeColumn(string columnOldName, TableColumnParam<ColumnEnum> columnNewParams)
        {
            //ALTER TABLE dbName.tableName CHANGE columnOldName columnNewName columnNewParams;
            MySqlParameter[] paras = new MySqlParameter[] { };
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("ALTER TABLE ").Append(databaseName).Append(".").Append(tableName);
            sbSql.Append(" CHANGE ").Append(columnOldName);
            LoadColumnInfo(columnNewParams, ref sbSql);

            //lock (locker)
            {
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    return false;
                }
            }

            return true;
        }

        private bool DropKey(string oldKeyInfo)
        {
            //ALTER TABLE dbName.tableName CHANGE columnOldName columnNewName columnNewParams;
            MySqlParameter[] paras = new MySqlParameter[] { };
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("ALTER TABLE ").Append(databaseName).Append(".").Append(tableName);
            sbSql.Append(" DROP ").Append(oldKeyInfo);

            //lock (locker)
            {
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    return false;
                }
            }

            return true;
        }

        private bool RemoveColumn(string colName)
        {
            //ALTER TABLE table_name DROP field_name;
            MySqlParameter[] paras = new MySqlParameter[] { };
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("ALTER TABLE ").Append(databaseName).Append(".").Append(tableName);
            sbSql.Append(" DROP ").Append(colName);

            //lock (locker)
            {
                try
                {
                    DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region 查询条件
        private MySqlParameter[] WhereCondition(ref StringBuilder sbSql, 
            List<QueryCondition<ColumnEnum>> queryConditions, ConditionRelation relation)
        {
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (queryConditions == null || queryConditions.Count() <= 0)
            {
                paras.Add(new MySqlParameter());
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int count = 0;
                for (int i = 0; i < queryConditions.Count; i++)
                {
                    var condition = queryConditions[i];
                    if (condition.Value == null)
                    {
                        if (condition._Operator == ConditionOperator.Equals
                            || condition._Operator == ConditionOperator.In)
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" ").Append(condition.ColumnName).Append(" is null");
                            paras.Add(new MySqlParameter());
                            count++;
                        }
                        else if (condition._Operator == ConditionOperator.NotEquals
                            || condition._Operator == ConditionOperator.NotIn)
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" ").Append(condition.ColumnName).Append(" is not null");
                            paras.Add(new MySqlParameter());
                            count++;
                        }
                    }
                    else
                    {
                        if (condition._Operator == ConditionOperator.In || condition._Operator == ConditionOperator.NotIn)
                        {
                            try
                            {
                                List<object> values;
                                bool isNullable = false;
                                if ((condition.Value is string) || (condition.Value is IEnumerable) == false)
                                {
                                    object value;
                                    Type type = condition.Value.GetType();
                                    if (type.IsEnum)
                                        value = Convert.ToInt32(condition.Value);
                                    else
                                        value = condition.Value;
                                    values = new List<object> { value };
                                }
                                else
                                {
                                    var vs = condition.Value as IEnumerable;
                                    values = new List<object>();
                                    foreach (var v in vs)
                                    {
                                        if (v == null)
                                        {
                                            isNullable = true;
                                            continue;
                                        }

                                        object value;
                                        Type type = v.GetType();
                                        if (type.IsEnum)
                                            value = Convert.ToInt32(v);
                                        else
                                            value = v;
                                        values.Add(value);
                                    }
                                }

                                if (values == null || values.Count() <= 0)
                                {
                                    if (condition._Operator == ConditionOperator.In)
                                    {
                                        if (count > 0) sb.Append(" ").Append(relation.ToString());
                                        sb.Append(" (").Append(condition.ColumnName).Append(" is null");
                                        sb.Append(" and ").Append(condition.ColumnName).Append(" is not null)");
                                        count += 2;
                                    }
                                }
                                else
                                {
                                    if (count > 0) sb.Append(" ").Append(relation.ToString());
                                    sb.Append(" (");
                                    if (isNullable)
                                    {
                                        sb.Append(" ").Append(condition.ColumnName);
                                        if (condition._Operator == ConditionOperator.In)
                                            sb.Append(" is null").Append(" or ");
                                        else if (condition._Operator == ConditionOperator.NotIn)
                                            sb.Append(" is not null").Append(" and ");
                                    }

                                    sb.Append(" ").Append(condition.ColumnName).Append(condition.Operator).Append("(");
                                    int index = 0;
                                    foreach (var value in values)
                                    {
                                        if (index > 0) sb.Append(",");
                                        string parameterName = "@c" + "_" + i.ToString() + "_" + (index++).ToString();
                                        sb.Append(parameterName);
                                        paras.Add(new MySqlParameter(parameterName, value));
                                    }
                                    sb.Append(")");
                                    sb.Append(") ");
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {
                                DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sb, paras, ex);
                            }
                        }
                        else
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" (");
                            string parameterName = "@c" + "_" + i.ToString();
                            sb.Append(" ").Append(condition.ColumnName).Append(condition.Operator).Append("(").Append(parameterName).Append(")");
                            paras.Add(new MySqlParameter(parameterName, condition.Value));

                            if (condition._Operator == ConditionOperator.NotEquals)
                            {
                                sb.Append(" or ").Append(condition.ColumnName).Append(" is null");
                            }
                            sb.Append(") ");
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    sbSql.Append(" where binary ").Append(sb); //binary：二進制方式查找，區分大小寫
                }
            }

            return paras.ToArray();
        }

        private MySqlParameter[] WhereCondition(ref StringBuilder sbSql,
            List<QueryCondition> queryConditions, ConditionRelation relation)
        {
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (queryConditions == null || queryConditions.Count() <= 0)
            {
                paras.Add(new MySqlParameter());
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int count = 0;
                for (int i = 0; i < queryConditions.Count; i++)
                {
                    var condition = queryConditions[i];
                    if (condition.Value == null)
                    {
                        if (condition._Operator == ConditionOperator.Equals
                            || condition._Operator == ConditionOperator.In)
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" ").Append(condition.ColumnFullName).Append(" is null");
                            paras.Add(new MySqlParameter());
                            count++;
                        }
                        else if (condition._Operator == ConditionOperator.NotEquals
                            || condition._Operator == ConditionOperator.NotIn)
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" ").Append(condition.ColumnFullName).Append(" is not null");
                            paras.Add(new MySqlParameter());
                            count++;
                        }
                    }
                    else
                    {
                        if (condition._Operator == ConditionOperator.In || condition._Operator == ConditionOperator.NotIn)
                        {
                            try
                            {
                                List<object> values;
                                bool isNullable = false;
                                if ((condition.Value is string) || (condition.Value is IEnumerable) == false)
                                {
                                    object value;
                                    Type type = condition.Value.GetType();
                                    if (type.IsEnum)
                                        value = Convert.ToInt32(condition.Value);
                                    else
                                        value = condition.Value;
                                    values = new List<object> { value };
                                }
                                else
                                {
                                    var vs = condition.Value as IEnumerable;
                                    values = new List<object>();
                                    foreach (var v in vs)
                                    {
                                        if (v == null)
                                        {
                                            isNullable = true;
                                            continue;
                                        }

                                        object value;
                                        Type type = v.GetType();
                                        if (type.IsEnum)
                                            value = Convert.ToInt32(v);
                                        else
                                            value = v;
                                        values.Add(value);
                                    }
                                }

                                if (values == null || values.Count() <= 0)
                                {
                                    if (condition._Operator == ConditionOperator.In)
                                    {
                                        if (count > 0) sb.Append(" ").Append(relation.ToString());
                                        sb.Append(" (").Append(condition.ColumnFullName).Append(" is null");
                                        sb.Append(" and ").Append(condition.ColumnFullName).Append(" is not null)");
                                        count += 2;
                                    }
                                }
                                else
                                {
                                    if (count > 0) sb.Append(" ").Append(relation.ToString());
                                    sb.Append(" (");
                                    if(isNullable)
                                    {
                                        sb.Append(" ").Append(condition.ColumnFullName);
                                        if (condition._Operator == ConditionOperator.In)
                                            sb.Append(" is null").Append(" or ");
                                        else if (condition._Operator == ConditionOperator.NotIn)
                                            sb.Append(" is not null").Append(" and ");
                                    }

                                    sb.Append(" ").Append(condition.ColumnFullName).Append(condition.Operator).Append("(");
                                    int index = 0;
                                    foreach (var value in values)
                                    {
                                        if (index > 0) sb.Append(",");
                                        string parameterName = "@c" + "_" + i.ToString() + "_" + (index++).ToString();
                                        sb.Append(parameterName);
                                        paras.Add(new MySqlParameter(parameterName, value));
                                    }
                                    sb.Append(")");
                                    sb.Append(") ");
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {
                                DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sb, paras, ex);
                            }
                        }
                        else
                        {
                            if (count > 0) sb.Append(" ").Append(relation.ToString());
                            sb.Append(" (");
                            string parameterName = "@c" + "_" + i.ToString();
                            sb.Append(" ").Append(condition.ColumnFullName).Append(condition.Operator).Append("(").Append(parameterName).Append(")");
                            paras.Add(new MySqlParameter(parameterName, condition.Value));

                            if (condition._Operator == ConditionOperator.NotEquals)
                            {
                                sb.Append(" or ").Append(condition.ColumnFullName).Append(" is null");
                            }
                            sb.Append(") ");
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    sbSql.Append(" where binary ").Append(sb); //binary：二進制方式查找，區分大小寫
                }
            }

            return paras.ToArray();
        }

        private StringBuilder OrderBy(ColumnEnum orderByColumn, bool isDescOrder)
        {
            return OrderBy(orderByColumn.ToString(), isDescOrder);
        }

        private StringBuilder OrderBy(string orderByColumnName, bool isDescOrder)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(orderByColumnName))
            {
                sb.Append(" order by ").Append(orderByColumnName);
                if (isDescOrder) sb.Append(" desc");
            }
            return sb;
        }

        private StringBuilder GroupBy(List<string> groupByColumnNames)
        {
            StringBuilder sb = new StringBuilder();
            if (groupByColumnNames != null)
            {
                string cols = groupByColumnNames.Aggregate((d, s) => s = d + ','+s);
                sb.Append(" group by ").Append(cols);
            }
            return sb;
        }

        private StringBuilder Limit(ulong? limitStart, ulong? limitCount)
        {
            StringBuilder sb = new StringBuilder();
            if (limitCount != null)
            {
                sb.Append(" limit ");
                if (limitStart != null) sb.Append(limitStart.Value).Append(',');
                sb.Append(limitCount.Value);
            }
            return sb;
        }
        #endregion

        #region 多表操作
        private StringBuilder Joins(List<Join> joins)
        {
            StringBuilder sbSql = new StringBuilder();
            if (joins != null)
            {
                foreach (var join in joins)
                {
                    sbSql.Append(" ").Append(join.JoinSql);
                }
            }

            return sbSql;
        }

        /// <summary>
        /// 獲取某些字段的數據
        /// </summary>
        /// <param name="columnNames">字段集合 null或數量為0:查詢全部字段</param>
        /// <param name="joins">連接集合 null或數量為0:無連接</param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumnFullName">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public DataTable GetColumnsData(List<ColumnName> columnNames, List<Join> joins = null,
            List<QueryCondition> queryConditions = null, ConditionRelation relation = ConditionRelation.and,
            string orderByColumnFullName = null, bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null, string tableAsName = "",
            List<string> groupByColumnFullNames = null)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select ");

            if (columnNames == null || columnNames.Count == 0)
            {
                sbSql.Append("*");
            }
            else
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (i > 0) sbSql.Append(",");
                    var col = columnNames[i];
                    sbSql.Append(col.ColFullName);
                    if (!string.IsNullOrEmpty(col.ColAsName))
                    {
                        sbSql.Append(" as ").Append(col.ColAsName);
                    }
                }
            }

            sbSql.Append(" from ").Append(databaseName).Append(".").Append(tableName);
            if (!string.IsNullOrEmpty(tableAsName)) sbSql.Append(" as ").Append(tableAsName);
            sbSql.Append(Joins(joins));

            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(GroupBy(groupByColumnFullNames));
            sbSql.Append(OrderBy(orderByColumnFullName, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            DataTable tempTable = null;
            lock (locker)
            {
                try
                {
                    tempTable = DbManager.Ins.ExcuteDataTable(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    tempTable = new DataTable();
                }
            }
            return tempTable;
        }

        /// <summary>
        /// 獲取值"select [count/sum/avg/min/max](columnName) from databaseName.tableName while ..."
        /// </summary>
        /// <param name="aggrFunc">聚合函數</param>
        /// <param name="columnFullName">要查找字段</param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumnFullName">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public object GetValue(AggregateFunction aggrFunc, string columnFullName, List<Join> joins = null,
            List<QueryCondition> queryConditions = null, ConditionRelation relation = ConditionRelation.and,
            string orderByColumnFullName = null, bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null, string tableAsName = "")
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select ");
            if (aggrFunc != AggregateFunction.none)
            {
                sbSql.Append(aggrFunc.ToString());
            }
            sbSql.Append("(").Append(columnFullName).Append(") ");
            sbSql.Append("from ").Append(databaseName).Append(".").Append(tableName);
            if (!string.IsNullOrEmpty(tableAsName)) sbSql.Append(" as ").Append(tableAsName);
            sbSql.Append(Joins(joins));

            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(OrderBy(orderByColumnFullName, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            object value = null;
            lock (locker)
            {
                try
                {
                    value = DbManager.Ins.ExecuteScalar(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }

            }

            return value;
        }
        #endregion

        #region 单表操作
        /// <summary>
        /// 滿足條件的數據是否存在(对外方法，内部使用会造成死锁)
        /// </summary>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <returns></returns>
        public bool IsDataExist(List<QueryCondition<ColumnEnum>> queryConditions = null, 
            ConditionRelation relation = ConditionRelation.and)
        {
            lock (locker)
            {
                return IsExist(queryConditions, relation);
            }
        }

        private bool IsExist(List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select * from ").Append(databaseName).Append(".").Append(tableName);
            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);

            object i = null;
            try
            {
                i = DbManager.Ins.ExecuteScalar(sbSql.ToString(), paras);
                DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
            }
            catch (Exception ex)
            {
                DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
            }

            if (i == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 獲取值"select [count/sum/avg/min/max](columnName) from databaseName.tableName while ..."
        /// </summary>
        /// <param name="aggrFunc">聚合函數</param>
        /// <param name="column">要查找字段</param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public object GetValue(AggregateFunction aggrFunc, ColumnEnum column,
            List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select ");
            if (aggrFunc != AggregateFunction.none)
            {
                sbSql.Append(aggrFunc.ToString());
            }
            sbSql.Append("(").Append(column.ToString()).Append(") ");
            sbSql.Append("from ").Append(databaseName).Append(".").Append(tableName);
            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(OrderBy(orderByColumn, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            object value = null;
            lock (locker)
            {
                try
                {
                    value = DbManager.Ins.ExecuteScalar(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }

            }

            return value;
        }

        /// <summary>
        /// 獲取一條數據
        /// </summary>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <returns></returns>
        public DataRow GetOneData(List<QueryCondition<ColumnEnum>> queryConditions = null, 
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null)
        {
            DataTable dt = GetData(queryConditions, relation, orderByColumn, isDescOrder, limitStart, 1);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 獲取數據（全部字段）
        /// </summary>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public DataTable GetData(List<QueryCondition<ColumnEnum>> queryConditions = null, 
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select * from ").Append(databaseName).Append(".").Append(tableName);
            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(OrderBy(orderByColumn, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            DataTable tempTable = null;
            lock (locker)
            {
                try
                {
                    tempTable = DbManager.Ins.ExcuteDataTable(sbSql.ToString(), paras);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                    tempTable = new DataTable();
                }
            }
            return tempTable;
        }

        /// <summary>
        /// 獲取某些字段的數據(对外方法，内部使用会造成死锁)
        /// </summary>
        /// <param name="columns">字段集合 null或數量為0:查詢全部字段</param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public DataTable GetColumnsData(List<ColumnEnum> columns = null, 
            List<QueryCondition<ColumnEnum>> queryConditions = null, 
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum),
            bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            lock (locker)
            {
                return GetColsData(columns, queryConditions, relation,
                     orderByColumn, isDescOrder, limitStart, limitCount);
            }
        }

        private DataTable GetColsData(List<ColumnEnum> columns = null,
            List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum),
            bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"select ");

            if (columns == null || columns.Count == 0)
            {
                sbSql.Append("*");
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    if (i > 0) sbSql.Append(",");
                    var col = columns[i];
                    sbSql.Append(col.ToString());
                }
            }

            sbSql.Append(" from ").Append(databaseName).Append(".").Append(tableName);
            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(OrderBy(orderByColumn, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            DataTable tempTable = null;
            try
            {
                tempTable = DbManager.Ins.ExcuteDataTable(sbSql.ToString(), paras);
                DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
            }
            catch (Exception ex)
            {
                DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                tempTable = new DataTable();
            }

            return tempTable;
        }

        /// <summary>
        /// 刪除數據(对外方法，内部使用会造成死锁)
        /// </summary>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public int DeleteData(List<QueryCondition<ColumnEnum>> queryConditions = null, 
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            lock (locker)
            {
                return Delete(queryConditions, relation, 
                    orderByColumn, isDescOrder, limitStart, limitCount);
            }
        }

        private int Delete(List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            if (!IsExist(queryConditions, relation))
            {
                return 0;
            }

            StringBuilder sbSql = new StringBuilder();
            sbSql.Append(@"delete from ").Append(databaseName).Append(".").Append(tableName);
            MySqlParameter[] paras = WhereCondition(ref sbSql, queryConditions, relation);
            sbSql.Append(OrderBy(orderByColumn, isDescOrder));
            sbSql.Append(Limit(limitStart, limitCount));

            int iRet = 0;

            try
            {
                iRet = DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras);
                DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
            }
            catch (Exception ex)
            {
                DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
            }

            return iRet;
        }

        /// <summary>
        /// 修改字段
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public bool ChangeValue(ColumnEnum column, object value, 
            List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            lock (locker)
            {
                if (!IsExist(queryConditions, relation))
                {
                    return false;
                }

                StringBuilder sbSql = new StringBuilder();
                sbSql.Append(@"update ").Append(databaseName).Append(".").Append(tableName);
                sbSql.Append(" set ").Append(column.ToString()).Append("=");

                string parameterName = "@s" + column.ToString();
                sbSql.Append(parameterName);
                List<MySqlParameter> paras = new List<MySqlParameter>();
                paras.Add(new MySqlParameter(parameterName, value));

                paras.AddRange(WhereCondition(ref sbSql, queryConditions, relation));
                sbSql.Append(OrderBy(orderByColumn, isDescOrder));
                sbSql.Append(Limit(limitStart, limitCount));

                int iRet = 0;
                try
                {
                    iRet = DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras.ToArray());
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }
            }

            return true;
        }

        /// <summary>
        /// 修改多個字段
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <param name="queryConditions">查找條件集合 null或數量為0:無條件查找</param>
        /// <param name="relation">條件之間的關系 默認為and關系</param>
        /// <param name="orderByColumn">排序字段 默認為起始列</param>
        /// <param name="isDescOrder">是否為降序排列 false:升序 true:降序</param>
        /// <param name="limitStart">查詢起始位置 null:起始位置為0</param>
        /// <param name="limitCount">查詢個數 null:不限制查詢個數</param>
        /// <returns></returns>
        public bool ChangeValues(DataRow dr,
            List<QueryCondition<ColumnEnum>> queryConditions = null,
            ConditionRelation relation = ConditionRelation.and,
            ColumnEnum orderByColumn = default(ColumnEnum), bool isDescOrder = false,
            ulong? limitStart = null, ulong? limitCount = null)
        {
            lock (locker)
            {
                if (!IsExist(queryConditions, relation))
                {
                    return false;
                }

                StringBuilder sbSql = new StringBuilder();
                sbSql.Append(@"update ").Append(databaseName).Append(".").Append(tableName);
                sbSql.Append(" set ");

                bool isFirst = true;
                List<MySqlParameter> paras = new List<MySqlParameter>();
                for(int c = 0; c < dr.Table.Columns.Count; c++)
                {
                    DataColumn col = dr.Table.Columns[c];
                    string colName = col.ColumnName;
                    if (AutoIncColumnNames.Contains(colName))
                    {
                        continue;
                    }

                    if (isFirst) isFirst = false;
                    else sbSql.Append(", ");

                    string parameterName = "@s" + colName + "_" + c.ToString();
                    sbSql.Append(colName).Append("=").Append(parameterName);
                    paras.Add(new MySqlParameter(parameterName, dr[colName]));
                }

                paras.AddRange(WhereCondition(ref sbSql, queryConditions, relation));
                sbSql.Append(OrderBy(orderByColumn, isDescOrder));
                sbSql.Append(Limit(limitStart, limitCount));

                int iRet = 0;
                try
                {
                    iRet = DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras.ToArray());
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }
            }

            return true;
        }

        /// <summary>
        /// 插入數據，默認覆蓋原來的數據
        /// </summary>
        /// <param name="data">數據源</param>
        /// <param name="isUpdateIfKeyExisted">如果數據存在，是否覆蓋，默認覆蓋</param>
        /// <returns></returns>
        public int InsertData(DataRow data, bool isUpdateIfKeyExisted = true)
        {
            List<QueryCondition<ColumnEnum>> keyQueryConditions = new List<QueryCondition<ColumnEnum>>();
            foreach (string colName in keyColumnNames)
            {
                ColumnEnum column = (ColumnEnum)Enum.Parse(typeof(ColumnEnum), colName);
                keyQueryConditions.Add(new QueryCondition<ColumnEnum>(column, data[colName]));
            }

            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("insert into ").Append(databaseName).Append(".").Append(tableName).Append(@" values(");
            List<MySqlParameter> paras = new List<MySqlParameter>();

            for (int i = 0; i < Columns.Count(); i++)
            {
                if (i > 0) sbSql.Append(",");
                var col = Columns[i];
                sbSql.Append("@c").Append(col.Name);
                paras.Add(new MySqlParameter("@c" + col.Name, data[col.Name]));
            }
            sbSql.Append(")");

            MySqlParameter[] paraArray = paras.ToArray();

            int iRet = 0;
            lock (locker)
            {
                if (keyQueryConditions.Count > 0 && IsExist(keyQueryConditions, ConditionRelation.or))
                {
                    if (isUpdateIfKeyExisted)
                    {
                        Delete(keyQueryConditions, ConditionRelation.or);
                    }
                    else
                    {
                        return 0;
                    }
                }

                try
                {
                    iRet = DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paraArray);
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }
            }

            return iRet;
        }

        /// <summary>
        /// 插入數據，默認覆蓋原來的數據
        /// </summary>
        /// <param name="dataTable">數據源</param>
        /// <param name="isUpdateIfKeyExisted">如果數據存在，是否覆蓋，默認覆蓋</param>
        /// <returns></returns>
        public int InsertData(DataTable dataTable, bool isUpdateIfKeyExisted = true)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return 0;
            }

            lock (locker)
            {
                List<Dictionary<object, int>> NoRepeatDataKeys = new List<Dictionary<object, int>>();
                List<ColumnEnum> keyColumns = new List<ColumnEnum>();
                List<HashSet<object>> keyDatas = new List<HashSet<object>>();
                foreach (string colName in keyColumnNames)
                {
                    ColumnEnum column = (ColumnEnum)Enum.Parse(typeof(ColumnEnum), colName);
                    keyColumns.Add(column);

                    NoRepeatDataKeys.Add(new Dictionary<object, int>());
                    keyDatas.Add(new HashSet<object>());
                }

                //標記dataTable內Key不為空且不重復的數據
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    DataRow dr = dataTable.Rows[row];
                    if(keyColumnNames.Any(k => (dr[k] == null)))
                    {
                        continue;
                    }

                    bool isRepeat = false;
                    do
                    {
                        isRepeat = false;
                        for (int i = 0; i < keyColumnNames.Count; i++)
                        {
                            string colName = keyColumnNames[i];
                            object key = dr[colName];
                            var nrdk = NoRepeatDataKeys[i];
                            if (!nrdk.ContainsKey(key))
                            {
                                nrdk.Add(key, row);
                            }
                            else
                            {
                                if (isUpdateIfKeyExisted)
                                {
                                    int index = nrdk[key];
                                    foreach (var dk in NoRepeatDataKeys)
                                    {
                                        try
                                        {
                                            var kv = dk.First(a => a.Value.Equals(index));
                                            dk.Remove(kv.Key);
                                        }
                                        catch { }
                                    }
                                    isRepeat = true;
                                    break;
                                }
                            }
                        }
                    } while (isRepeat);
                }

                //標記(刪除)數據庫與dataTable重復的數據
                if (keyColumnNames.Count > 0)
                {
                    List<QueryCondition<ColumnEnum>> keyQueryConditions = new List<QueryCondition<ColumnEnum>>();
                    for (int i = 0; i < keyColumnNames.Count; i++)
                    {
                        List<object> values = new List<object>();
                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            if (NoRepeatDataKeys.Count > 0 && (!NoRepeatDataKeys[0].ContainsValue(row)))
                            {
                                continue;   //跳過dataTable中重復數據
                            }

                            DataRow dr = dataTable.Rows[row];
                            values.Add(dr[keyColumnNames[i]]);
                        }
                        keyQueryConditions.Add(new QueryCondition<ColumnEnum>(keyColumns[i], values, ConditionOperator.In));
                    }

                    if (isUpdateIfKeyExisted)
                    {
                        Delete(keyQueryConditions, ConditionRelation.or);
                    }
                    else
                    {
                        DataTable dt = GetColsData(keyColumns, keyQueryConditions, ConditionRelation.or);
                        for (int i = 0; i < keyColumnNames.Count; i++)
                        {
                            for (int r = 0; r < dt.Rows.Count; r++)
                            {
                                object value = dt.Rows[r][keyColumnNames[i]];
                                if (!keyDatas[i].Contains(value))
                                {
                                    keyDatas[i].Add(value);
                                }
                            }
                        }
                    }
                }

                //寫SQL語句
                int insertCount = 0;
                StringBuilder sbSql = new StringBuilder();
                sbSql.Append("insert into ").Append(databaseName).Append(".").Append(tableName).Append(@" values");
                List<MySqlParameter> paras = new List<MySqlParameter>();
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    if(NoRepeatDataKeys.Count > 0 && (!NoRepeatDataKeys[0].ContainsValue(row)))
                    {
                        continue;   //跳過dataTable中重復數據
                    }

                    DataRow dr = dataTable.Rows[row];
                    bool isExisted = false;
                    if (!isUpdateIfKeyExisted)
                    {
                        for (int i = 0; i < keyColumnNames.Count; i++)
                        {
                            if(keyDatas[i].Contains(dr[keyColumnNames[i]]))
                            {
                                isExisted = true;
                                break;
                            }
                        }
                    }

                    if (!isExisted)
                    {
                        if(insertCount > 0) sbSql.Append(",");
                        sbSql.Append("(");
                        for (int i = 0; i < Columns.Count(); i++)
                        {
                            if (i > 0) sbSql.Append(",");
                            var col = Columns[i];
                            string paramName = "@c" + col.Name + "_" + row.ToString();
                            sbSql.Append(paramName);
                            paras.Add(new MySqlParameter(paramName, dr[col.Name]));
                        }
                        sbSql.Append(")");
                        insertCount++;
                    }
                }

                if (insertCount == 0)
                {
                    return 0;
                }

                //插入數據
                int iRet = 0;
                try
                {
                    iRet = DbManager.Ins.ExecuteNonquery(sbSql.ToString(), paras.ToArray());
                    DbHandle.LogOutput(new StackFrame(true), databaseName, tableName, sbSql, paras);
                }
                catch (Exception ex)
                {
                    DbHandle.ExceptionProcess(new StackFrame(true), databaseName, tableName, sbSql, paras, ex);
                }

                return iRet;
            }

        }
        #endregion

        #region 数据缓存
        DataTable DataBuffer = null;
        private readonly object DataBufferLocker = new object();
        public void InsertToDataBuffer(DataRow data)
        {
            lock (DataBufferLocker)
            {
                if (DataBuffer == null)
                {
                    DataBuffer = NewDataTable();
                }

                int colCount = DataBuffer.Columns.Count;
                if (data == null || (colCount != data.Table.Columns.Count))
                {
                    return;
                }

                DataRow dr = DataBuffer.NewRow();
                DataBuffer.Rows.Add(dr);
                for (int i = 0; i < colCount; i++)
                {
                    dr[i] = data[i];
                }

                //if(tableName.Contains("StaffPhotoTable"))
                //{
                //    string id = dr[StaffPhotoColumn.Uid.ToString()].ToString();
                //    string pp = dr[StaffPhotoColumn.facePhoto.ToString()].ToString();
                //    if (string.IsNullOrEmpty(pp))
                //    {
                //        TS_IDManager.LogHandle.AddFromDataRow(new StackFrame(true), data);
                //        TS_IDManager.LogHandle.AddFromDataRow(new StackFrame(true), dr);
                //    }
                //    else
                //    {
                //        string pid = pp.Substring(pp.Length - 24, 18);
                //        if(!string.Equals(id, pid))
                //        {
                //            TS_IDManager.LogHandle.AddFromDataRow(new StackFrame(true), data);
                //            TS_IDManager.LogHandle.AddFromDataRow(new StackFrame(true), dr);
                //        }
                //    }
                //}
            }
        }

        public void CleanDataBuffer()
        {
            lock (DataBufferLocker)
            {
                if (DataBuffer != null)
                {
                    DataBuffer.Rows.Clear();
                }
            }
        }

        public void SaveDataBuffer(bool isUpdateIfKeyExisted = true)
        {
            lock (DataBufferLocker)
            {
                if (DataBuffer != null)
                {
                    InsertData(DataBuffer, isUpdateIfKeyExisted);
                    DataBuffer.Rows.Clear();
                }
            }
        }
        #endregion
    }
}
