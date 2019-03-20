using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Reciver.Extensions
{
    public static class QueryBuilderHelper
    {
        public static readonly Dictionary<string, Dictionary<string, SqlDbType>> dbSchema;
        static QueryBuilderHelper()
        {
            dbSchema = new GetDbSchemaCommand()
                .Result
                .GroupBy(k => k.TableName)
                .ToDictionary(gdc => gdc.Key,
                    gdc => gdc.ToDictionary(
                        v => v.ColumnName,
                        v => v.ColumnType));
        }


        public static object FormatSQL(string tableName, string columnName, object value)
        {
            SqlDbType type = dbSchema[tableName][columnName];
            switch (type)
            {
                case SqlDbType.Date:
                    {
                        return value == null ? "NULL" : DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd");
                    }
                //case SqlDbType.Image:               
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                //case SqlDbType.Structured:
                case SqlDbType.Text:
                case SqlDbType.Timestamp:
                //case SqlDbType.Udt:
                case SqlDbType.UniqueIdentifier:
                //case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                //case SqlDbType.Variant:
                //case SqlDbType.Xml:
                case SqlDbType.Char:
                    {
                        return value == null ? "NULL" : string.Format("{0}", value.ToString()); //TODO: decide later remove or replace ' with something other
                    }
                case SqlDbType.DateTimeOffset:
                    {
                        return value == null ? "NULL" : DateTimeOffset.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:sszzz");
                    }
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.SmallDateTime:
                    {
                        return value == null ? "NULL" : DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:sss");
                    }
                case SqlDbType.Time:
                    {
                        return value == null ? "NULL" : DateTime.Parse(value.ToString()).ToString("HH:mm:sss");
                    }
                case SqlDbType.BigInt:
                case SqlDbType.Binary:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Real:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.TinyInt:
                case SqlDbType.Money:
                    {
                        return value == null ? "NULL" : value.ToString();
                    }
                case SqlDbType.Bit:
                    {

                        if (value == null)
                        {
                            return "NULL";
                        }
                        else if (string.Compare(value.ToString(), "true", true) == 0 || value == "1")
                        {
                            return 1;
                        }
                        else if (string.Compare(value.ToString(), "false", true) == 0 || value == "0")
                        {
                            return 0;
                        }
                        else
                        {
                            throw new Exception("Wrong Boolean " + value + " Value For Field:" + tableName + "." + columnName);
                        }
                    }
                default:
                    throw new Exception("Unsupported data type in FormatSQL");
            }
        }

        public static object FormatSQLForDb(string tableName, string columnName, object value)
        {
            SqlDbType type = dbSchema[tableName][columnName];

            if (value == null)
                return System.DBNull.Value;

            switch (type)
            {
                case SqlDbType.Date:
                    {
                        return DateTime.Parse(DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd"));
                    }
                //case SqlDbType.Image:               
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                //case SqlDbType.Structured:
                case SqlDbType.Text:
                case SqlDbType.Timestamp:
                //case SqlDbType.Udt:
                case SqlDbType.UniqueIdentifier:
                //case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                //case SqlDbType.Variant:
                //case SqlDbType.Xml:
                case SqlDbType.Char:
                    {
                        return value;
                    }
                case SqlDbType.DateTimeOffset:
                    {
                        return DateTime.Parse(DateTimeOffset.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:sszzz"));
                    }
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.SmallDateTime:
                    {
                        return DateTime.Parse(DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:sss"));
                    }
                case SqlDbType.Time:
                    {
                        return DateTime.Parse(DateTime.Parse(value.ToString()).ToString("HH:mm:sss"));
                    }
                case SqlDbType.BigInt:
                case SqlDbType.Binary:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Real:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.TinyInt:
                case SqlDbType.Money:
                    {
                        return value;
                    }
                case SqlDbType.Bit:
                    {

                        if (value == null)
                        {
                            return System.DBNull.Value;
                        }
                        else if (string.Compare(value.ToString(), "true", true) == 0 || value == "1")
                        {
                            return 1;
                        }
                        else if (string.Compare(value.ToString(), "false", true) == 0 || value == "0")
                        {
                            return 0;
                        }
                        else
                        {
                            throw new Exception("Wrong Boolean " + value + " Value For Field:" + tableName + "." + columnName);
                        }
                    }
                default:
                    throw new Exception("Unsupported data type in FormatSQL");
            }
        }
    }
}
