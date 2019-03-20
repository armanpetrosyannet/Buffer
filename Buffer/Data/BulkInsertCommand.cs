using BetConstruct.AGP.VIKI.Models.Schema;
using BetConstruct.AGP.VIKI.VirtualDb;
using Notification;
using Reciver.Data;
using Reciver.Extensions;
using Reciver.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Reciver.BulkOperation
{
    public class BulkInsertCommand : DataCommand
    {

        private IEnumerable<ObjectChange> _alldata;

        private string _tableName;

        private TableInfo _tableInfo;

        private InsertQueryBuilder querybulder;

        public static readonly Dictionary<string, Dictionary<string, SqlDbType>> shcema = QueryBuilderHelper.dbSchema;


        public BulkInsertCommand(IEnumerable<ObjectChange> objects, string tablename)
        {
            _alldata = objects;
            _tableName = tablename;
            _tableInfo = VirtualDB.Schema.GetTable(_tableName);

        }

        public void Run()
        {

            var _distinctdata = _alldata.SelectMany(e => e.BaseObject.Select(w => w.Key)).Distinct();

            var ValidKeys = _tableInfo.ColumnInfos.Where(q => q.IsKey).Select(e => e.ColumnName).Intersect(_distinctdata).ToList();

            var AllValidfields = _tableInfo.ColumnInfos.Select(e => e.ColumnName).Intersect(_distinctdata);


            if (ValidKeys.Count != _tableInfo.ColumnInfos.Where(e => e.IsKey).Select(e => e.ColumnName).Distinct().Count())
            {
                Console.WriteLine($"Key missed -Type {_tableName} ");

                EmailSender sender = new EmailSender();
                sender.SendEmailAsync("arman.petrosyan@betconstruct.com", "Error--key", $"Key missed -Type {_tableName} ");

            }

            querybulder = new InsertQueryBuilder(AllValidfields, _tableName);

            List<SqlParameter> _paramsSql = new List<SqlParameter>();

            int _index = 0;

            foreach (var item in _alldata)
            {
                List<string> stringparamForQuery = new List<string>();

                foreach (var field in AllValidfields)
                {
                    string param = $"@p{_index++}";

                    stringparamForQuery.Add(param);

                    var dbpar = new SqlParameter(param, shcema[_tableName][field]);
                    if (item.BaseObject.ContainsKey(field))
                    {
                        dbpar.Value = QueryBuilderHelper.FormatSQLForDb(_tableName, field, item.BaseObject[field]);

                    }
                    else
                    {
                        dbpar.Value = DBNull.Value;
                    }

                    _paramsSql.Add(dbpar);

                }

                querybulder.AddQueryParametrs(stringparamForQuery.ToList());

                if (_paramsSql.Count > InsertCommandParametersCount)
                {
                    Execute(_paramsSql);
                    _paramsSql = new List<SqlParameter>();
                    _index = 0;
                    querybulder.Reset();
                }
            }

            if (_paramsSql.Count > 0)
            {
                Execute(_paramsSql);
                _paramsSql = new List<SqlParameter>();
                _index = 0;

            }


        }



        internal int Execute(IEnumerable<SqlParameter> parameters)
        {
            try
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    command.CommandText = querybulder.GetCommandText();
                    command.CommandType = CommandType;
                    command.Connection = Connection;
                    command.CommandTimeout = CommandTimeout;
                    var count = command.ExecuteNonQuery();
                    return count;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Execute exception---" + ex.Message + $"{_tableName}");
                string text = ($"Table---> {_tableName} Execute exception {ex.Message}{Environment.NewLine}-Inner exception {ex.InnerException} {Environment.NewLine} Command text--{querybulder.GetCommandText()} ");
                EmailSender sender = new EmailSender();
                sender.SendEmailAsync("arman.petrosyan@betconstruct.com", "Error", text);
            }

            finally
            {
                Connection.Close();
            }

            return -1;
        }



    }


}
