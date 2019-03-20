using BetConstruct.AGP.VIKI.Models.Schema;
using BetConstruct.AGP.VIKI.VirtualDb;
using Notification;
using Reciver.Extensions;
using Reciver.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Reciver.Data
{
    public class BulkUbdateCommand : DataCommand
    {

        private IEnumerable<ObjectChange> _alldata;

        private string _tableName;

        private TableInfo _tableInfo;

        private UpdateQueryBuilder querybulder;

        public static readonly Dictionary<string, Dictionary<string, SqlDbType>> shcema = QueryBuilderHelper.dbSchema;


        public BulkUbdateCommand(IEnumerable<ObjectChange> objects, string tablename)
        {
            _alldata = objects;
            _tableName = tablename;
            _tableInfo = VirtualDB.Schema.GetTable(_tableName);
        }


        public void Run()
        {

            var _distinctdata = _alldata.SelectMany(e => e.BaseObject.Select(w => w.Key)).Distinct().ToList();

            var AllValidfields = _tableInfo.ColumnInfos.Select(e => e.ColumnName).Intersect(_distinctdata);

            var ValidKeys = _tableInfo.ColumnInfos.Where(q => q.IsKey).Select(e => e.ColumnName).Intersect(_distinctdata).ToList();

            if (ValidKeys.Count != _tableInfo.ColumnInfos.Where(e => e.IsKey).Select(e => e.ColumnName).Distinct().Count())
            {
                Console.WriteLine($"Key missed -Type {_tableName} ");
                throw new Exception();
            }

            querybulder = new UpdateQueryBuilder(_tableName);

            List<SqlParameter> _paramsSql = new List<SqlParameter>();

            int _index = 0;

            foreach (var item in _alldata)
            {
                List<string> stringParamForQuery = new List<string>();

                List<string> stringKeyParam = new List<string>();

                var data = item.ChangedProperties.Intersect(AllValidfields);

                if (data.Count() > 0)
                {
                    foreach (var obj in data)
                    {
                        string param = $"@p{_index++}";

                        stringParamForQuery.Add(param);

                        var dbpar = new SqlParameter(param, shcema[_tableName][obj]);

                        if (item.BaseObject.ContainsKey(obj))
                        {
                            dbpar.Value = QueryBuilderHelper.FormatSQLForDb(_tableName, obj, item.BaseObject[obj]);

                        }
                        else
                        {
                            dbpar.Value = DBNull.Value;
                        }

                        _paramsSql.Add(dbpar);

                    }

                    foreach (var singleKey in ValidKeys)
                    {
                        string param = $"@p{_index++}";
                        var dbpar = new SqlParameter(param, shcema[_tableName][singleKey]);

                        stringKeyParam.Add(param);
                        dbpar.Value = QueryBuilderHelper.FormatSQLForDb(_tableName, singleKey, item.BaseObject[singleKey]);
                        _paramsSql.Add(dbpar);
                    }

                    querybulder.AppendCommand(data.ToList(), stringParamForQuery, ValidKeys, stringKeyParam);

                    if (_paramsSql.Count > UpdateCommandParametersCount)
                    {
                        Execute(_paramsSql);
                        _paramsSql = new List<SqlParameter>();
                        _index = 0;
                        querybulder.Reset();
                        stringParamForQuery = new List<string>();
                        stringKeyParam = new List<string>();

                    }
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
                MessageForError(ex.Message, "First try");

            }

            finally
            {
                Connection.Close();

            }

            return -1;
        }

        void MessageForError(string message, string header)
        {
            Console.WriteLine("Execute exception---" + message + $"{_tableName}");
            string text = ($"Table---> {_tableName} Execute exception {message}{Environment.NewLine} {Environment.NewLine} Command text--{querybulder.GetCommandText()} ");
            EmailSender sender = new EmailSender();
            sender.SendEmailAsync("arman.petrosyan@betconstruct.com", header, text);

        }



    }
}
