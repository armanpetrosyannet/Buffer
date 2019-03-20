using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Reciver.Data
{
    public class InsertQueryBuilder
    {
        private StringBuilder _queryBuilder = new StringBuilder();

        string _query;

        public InsertQueryBuilder(IEnumerable<string> column, string tableName)
        {
            _queryBuilder.AppendFormat("INSERT INTO {0}", tableName);
            string columns = string.Join(", ", column.Select(x => string.Format("[{0}]", x)));
            _queryBuilder.AppendFormat("( {0} )  VALUES ", columns);
            _query = _queryBuilder.ToString();

        }


        public StringBuilder AddQueryParametrs(IEnumerable<string> parametrs)
        {
            string parameters = string.Join(", ", parametrs.ToList());

            _queryBuilder.AppendFormat("( {0} ), ", parameters);

            return _queryBuilder;
        }

        public string GetCommandText()
        {
            _queryBuilder[_queryBuilder.Length - 2] = ' ';
            return _queryBuilder.ToString();
        }


        public void Reset()
        {
            _queryBuilder = new StringBuilder();
            _queryBuilder.Append(_query);

        }
    }
}
