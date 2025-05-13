//-----------------------------------------------------------------------
// <copyright file="SQLGenerator.cs" company="Lifeprojects.de">
//     Class: SQLGenerator
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.05.2025 14:27:39</date>
//
// <summary>
// Basisstruktur eines SQLGenerator
// </summary>
//-----------------------------------------------------------------------

namespace Console.Expressions
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    public class SQLGenerator<TEntity> : ISQLGenerator<TEntity>, IDisposable
    {
        private bool classIsDisposed = false;
        private StringBuilder sb = new StringBuilder();

        public SQLGenerator(TEntity entity)
        {
            this.Entity = entity;
        }

        ~SQLGenerator()
        {
            this.Dispose(false);
        }

        public TEntity Entity { get; private set; }

        private string PropertyNameWhere { get; set; }

        private AddBracket AddBracket { get; set; }

        public ISQLGenerator<TEntity> CreateTable()
        {
            string tableName = this.DataTableAttributes();

            sb.Clear();
            sb.AppendFormat("CREATE TABLE IF NOT EXISTS {0} ", tableName).Append('\n').Append('(').Append('\n');

            IEnumerable<TableColumnAttribute> dataColumns = this.CustomerAttributesTableColumn();

            foreach (TableColumnAttribute column in dataColumns)
            {
                PropertyInfo propertyInfo = typeof(TEntity).GetProperty(column.ColumnName);
                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType == typeof(Guid))
                    {
                        sb.Append($"{column.ColumnName} VARCHAR(36),").Append('\n');
                    }
                    else if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (column.Length == 0)
                        {
                            sb.Append($"{column.ColumnName} {SQLiteDataType.Text},").Append('\n');
                        }
                        else
                        {
                            sb.Append($"{column.ColumnName} VARCHAR({column.Length}),").Append('\n');
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        if (column.Length == 0)
                        {
                            sb.Append($"{column.ColumnName} {SQLiteDataType.Integer},").Append('\n');
                        }
                        else
                        {
                            sb.Append($"{column.ColumnName} {SQLiteDataType.Integer}({column.Length}),").Append('\n');
                        }
                    }
                    else if (propertyInfo.PropertyType.Name.Contains("decimal", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        sb.Append($"{column.ColumnName} DECIMAL({column.Length},{column.AfterComma}),").Append('\n');
                    }
                    else if (propertyInfo.PropertyType == typeof(double))
                    {
                        sb.AppendFormat("{0} {1},", column.ColumnName, $"DOUBLE({column.Length},{column.AfterComma})").Append('\n');
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        sb.Append($"{column.ColumnName} {SQLiteDataType.Boolean},").Append('\n');
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        sb.Append($"{column.ColumnName} {SQLiteDataType.DateTime},").Append('\n');
                    }
                    else
                    {
                        sb.Append($"{column.ColumnName} {SQLiteDataType.Text},").Append('\n');
                    }
                }
            }

            sb.Remove(sb.ToString().Length-2, 1);

            List<string> keyColumns = this.CustomerAttributesPK();
            if (keyColumns != null && keyColumns.Count > 0)
            {
                sb.Append(", ");
                sb.Append("PRIMARY KEY").Append(' ').Append('\n').Append('(').Append('\n');
                foreach (string keyColumn in keyColumns)
                {
                    sb.Append($"{keyColumn},");
                }

                sb.Remove(sb.ToString().Length - 1, 1);
                sb.Append('\n').Append(')');
            }

            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> Insert()
        {
            string tableName = this.DataTableAttributes();

            sb.Clear();

            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.Append($"INSERT INTO {tableName}").Append(' ').Append('\n');
            sb.Append('(');
            sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
            sb.Append(')').Append(' ');
            sb.Append('\n').Append("VALUES").Append('\n');
            sb.Append(' ').Append('(');
            sb.Append(string.Join(", ", propInfos.Select(pi => $"{this.InsertHelper(pi)}")));
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> Update()
        {
            string tableName = this.DataTableAttributes();
            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> keyColumns = this.CustomerAttributesPK();

            sb.Clear();
            sb.Append($"UPDATE {tableName}").Append(' ').Append('\n');
            sb.Append($"SET").Append(' ');
            foreach (PropertyInfo item in propInfos)
            {
                if (keyColumns.Contains(item.Name) == false)
                {
                    sb.Append(item.Name);
                    sb.Append(" = ");
                    sb.Append($"{this.InsertHelper(item)}");
                    sb.Append(',');
                }
            }

            sb.Remove(sb.ToString().Length-1, 1);

            return this;
        }

        public ISQLGenerator<TEntity> Update(params Expression<Func<TEntity, object>>[] expressions)
        {
            string tableName = this.DataTableAttributes();
            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> keyColumns = this.CustomerAttributesPK();

            sb.Clear();
            sb.Append($"UPDATE {tableName}").Append(' ').Append('\n');
            sb.Append($"SET").Append(' ');
            foreach (var item in expressions)
            {
                string expName = ExpressionPropertyName.For<TEntity>(item);

                if (keyColumns.Contains(expName) == false)
                {
                    object propertyValue = typeof(TEntity).GetProperty(expName).GetValue(this.Entity);
                    sb.Append(expName);
                    sb.Append(" = ");
                    sb.Append($"{this.ValueAsText(propertyValue)}");
                    sb.Append(',');
                }
            }

            sb.Remove(sb.ToString().Length - 1, 1);

            return this;
        }

        public ISQLGenerator<TEntity> Delete(params Expression<Func<TEntity, object>>[] expressions)
        {
            string tableName = this.DataTableAttributes();

            sb.Clear();
            sb.Append($"UPDATE {tableName}").Append(' ').Append('\n');
            if (sb.ToString().Contains("WHERE", StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append("WHERE").Append(' ');
            }

            sb.Append('(').Append('\n');

            foreach (var item in expressions)
            {
                string expName = ExpressionPropertyName.For<TEntity>(item);
                object propertyValue = typeof(TEntity).GetProperty(expName).GetValue(this.Entity);
                sb.Append(expName);
                sb.Append(" = ");
                sb.Append($"{this.ValueAsText(propertyValue)}");
                sb.Append(' ');
                sb.Append('\n');
                sb.Append("AND").Append(' ');
            }

            sb.Append(')');

            return this;
        }

        private object InsertHelper(PropertyInfo propertyInfo)
        {
            object value = null;

            if (propertyInfo.PropertyType == typeof(string))
            {
                value = $"'{propertyInfo.GetValue(this.Entity).ToString()}'";
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                value = propertyInfo.GetValue(this.Entity).ToString();
            }
            else if (propertyInfo.PropertyType == typeof(long))
            {
                value = propertyInfo.GetValue(this.Entity).ToString();
            }
            else if (propertyInfo.PropertyType == typeof(decimal))
            {
                value = Convert.ToDecimal(propertyInfo.GetValue(this.Entity)).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            else if (propertyInfo.PropertyType == typeof(double))
            {
                value = Convert.ToDouble(propertyInfo.GetValue(this.Entity)).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            else if (propertyInfo.PropertyType == typeof(DateTime))
            {
                value = $"'{Convert.ToDateTime(propertyInfo.GetValue(this.Entity)).ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                if (Convert.ToBoolean(propertyInfo.GetValue(this.Entity)) == true)
                {
                    value = 1.ToString();
                }
                else
                {
                    value = 0.ToString();
                }
            }
            else if (propertyInfo.PropertyType == typeof(Guid))
            {
                value = $"'{propertyInfo.GetValue(this.Entity).ToString()}'";
            }

            return value;
        }

        public ISQLGenerator<TEntity> Select(params Expression<Func<TEntity, object>>[] expressions)
        {
            string tableName = this.DataTableAttributes();

            sb.Clear();
            sb.Append($"SELECT").Append(' ').Append('\n');
            sb.Append(' ').Append(string.Join(", ", expressions.Select(s => ExpressionPropertyName.For<TEntity>(s))));
            sb.Append(' ').Append('\n').Append("FROM").Append(' ');
            sb.Append($"{tableName}");

            return this;
        }

        public ISQLGenerator<TEntity> Select(SelectOperator selectOperator = SelectOperator.All)
        {
            string tableName = this.DataTableAttributes();
            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            sb.Clear();
            sb.Append($"SELECT").Append(' ').Append('\n');
            if (selectOperator == SelectOperator.All)
            {
                sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
                sb.Append(' ').Append('\n').Append("FROM").Append(' ');
                sb.Append($"{tableName}");
            }
            else if (selectOperator == SelectOperator.Count)
            {
                sb.Append("COUNT(*)").Append(' ').Append('\n');
                sb.Append("FROM").Append(' ');
                sb.Append($"{tableName}");
            }
            else
            {
                sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
                sb.Append(' ').Append('\n').Append("FROM").Append(' ');
                sb.Append($"{tableName}");
            }

            return this;
        }

        public ISQLGenerator<TEntity> Select(SelectOperator selectOperator, string sqlText)
        {
            string tableName = this.DataTableAttributes();
            sb.Clear();

            if (selectOperator == SelectOperator.Direct)
            {
                sb.Append(sqlText);
            }

            return this;
        }

        public ISQLGenerator<TEntity> Distinct()
        {
            const string SELECT = "SELECT";
            const string DISTINCT = "DISTINCT";

            if (sb.ToString().Contains(SELECT, StringComparison.OrdinalIgnoreCase) == true && sb.ToString().Contains(DISTINCT, StringComparison.OrdinalIgnoreCase) == false)
            {
                int pos = sb.ToString().LastIndexOf(SELECT) + (SELECT.Length + 1);
                if (pos > 0)
                {
                    sb.Insert(pos, DISTINCT);
                    sb.Append(' ');
                }
            }

            return this;
        }

        public ISQLGenerator<TEntity> Take(int limit)
        {
            const string SELECT = "SELECT";
            const string LIMIT = "LIMIT";

            if (sb.ToString().Contains(SELECT, StringComparison.OrdinalIgnoreCase) == true && sb.ToString().Contains(LIMIT, StringComparison.OrdinalIgnoreCase) == false)
            {
                int pos = sb.ToString().Length;
                if (pos > 0)
                {
                    sb.Append(' ').Append($"{LIMIT} {limit}");
                }
            }

            return this;
        }


        public ISQLGenerator<TEntity> Where(Expression<Func<TEntity, object>> expressions, SQLComparison sqlCompare, object value)
        {
            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains("WHERE", StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append("WHERE").Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere);
            sb.Append(' ').Append(this.WhereOperatorAsText(sqlCompare)).Append(' ');
            if (value != null)
            {
                if (sqlCompare == SQLComparison.In)
                {
                    sb.Replace(this.WhereOperatorAsText(sqlCompare), $"IN({value})");
                }
                else if (sqlCompare == SQLComparison.NotIn)
                {
                    sb.Replace(this.WhereOperatorAsText(sqlCompare), $"NOT IN({value})");
                }
                else if (sqlCompare == SQLComparison.Like)
                {
                    sb.Replace(this.WhereOperatorAsText(sqlCompare), $"LIKE ('{value}')");
                }
                else if (sqlCompare == SQLComparison.NotLike)
                {
                    sb.Replace(this.WhereOperatorAsText(sqlCompare), $"NOT LIKE ('{value}')");
                }
                else
                {
                    sb.Append($"{this.ValueAsText(value)}");
                }
            }

            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddWhereAnd(Expression<Func<TEntity, object>> expressions, SQLComparison sqlCompare, object value = null)
        {
            const string WHERE = "WHERE";
            this.AddBracket = AddBracket.None;
            string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == true)
            {
                int wherePos = sb.ToString().LastIndexOf(WHERE) + (WHERE.Length + 1);
                if (wherePos > 0)
                {
                    if (sb.ToString().LastIndexOf("((") == -1)
                    {
                        if (sb.ToString().LastIndexOf("IN(") > 0 || sb.ToString().LastIndexOf("NOT IN(") > 0)
                        {
                            this.AddBracket = AddBracket.None;
                        }
                        else
                        {
                            sb.Insert(wherePos, '(');
                            this.AddBracket = AddBracket.BracketLeft;
                        }

                    }
                    else if (this.AddBracket == AddBracket.None)
                    {
                        if (sb.ToString().LastIndexOf("))") > 0)
                        {
                            sb.Remove(sb.ToString().LastIndexOf("))") + 1, 1);
                        }
                    }
                }

                sb.Append(' ').Append('\n').Append("AND").Append(' ');
                sb.Append('(');
                sb.Append(propertyName);
                sb.Append(' ').Append(this.WhereOperatorAsText(sqlCompare)).Append(' ');
                if (value != null)
                {
                    if (sqlCompare == SQLComparison.In)
                    {
                        sb.Replace(this.WhereOperatorAsText(sqlCompare), $"IN({value})");
                    }
                    else if (sqlCompare == SQLComparison.NotIn)
                    {
                        sb.Replace(this.WhereOperatorAsText(sqlCompare), $"NOT IN({value})");
                    }
                    else if (sqlCompare == SQLComparison.Like)
                    {
                        sb.Replace(this.WhereOperatorAsText(sqlCompare), $"LIKE ('{value}')");
                    }
                    else if (sqlCompare == SQLComparison.NotLike)
                    {
                        sb.Replace(this.WhereOperatorAsText(sqlCompare), $"NOT LIKE ('{value}')");
                    }
                    else
                    {
                        sb.Append($"{this.ValueAsText(value)}");
                    }
                }

                sb.Append(')');
                if (this.AddBracket == AddBracket.BracketLeft)
                {
                    sb.Append(')');
                    this.AddBracket = AddBracket.None;
                }
                else if (this.AddBracket == AddBracket.None)
                {
                    if (sb.ToString().LastIndexOf("IN(") > 0 || sb.ToString().LastIndexOf("NOT IN(") > 0)
                    {
                    }
                    else
                    {
                        sb.Append(')');
                        this.AddBracket = AddBracket.None;
                    }
                }
            }

            return this;
        }

        public ISQLGenerator<TEntity> AddWhereOr(SQLComparison sqlCompare, object value)
        {
            const string WHERE = "WHERE";

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == true)
            {
                int wherePos = sb.ToString().IndexOf(WHERE) + (WHERE.Length + 1);
                if (wherePos > 0)
                {
                    if (sb.ToString().LastIndexOf("((") == -1)
                    {
                        sb.Insert(wherePos, '(');
                        this.AddBracket = AddBracket.BracketLeft;
                    }
                    else if (this.AddBracket == AddBracket.None)
                    {
                        if (sb.ToString().LastIndexOf("))") > 0)
                        {
                            sb.Remove(sb.ToString().LastIndexOf("))") + 1, 1);
                        }
                    }
                }

                sb.Append(' ').Append('\n').Append("OR").Append(' ');
                sb.Append('(');
                sb.Append(this.PropertyNameWhere);
                sb.Append(' ').Append(this.WhereOperatorAsText(sqlCompare)).Append(' ');
                sb.Append($"{this.ValueAsText(value)}");
                sb.Append(')');

                if (this.AddBracket == AddBracket.BracketLeft)
                {
                    sb.Append(')');
                    this.AddBracket = AddBracket.None;
                }
                else if (this.AddBracket == AddBracket.None)
                {
                    sb.Append(')');
                    this.AddBracket = AddBracket.None;
                }
            }

            return this;
        }

        public ISQLGenerator<TEntity> AddBetween(Expression<Func<TEntity, object>> expressions, object valueLow, object valueHigh)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("BETWEEN").Append(' ');
            sb.Append($"{this.ValueAsText(valueLow)}");
            sb.Append(' ');
            sb.Append("AND");
            sb.Append(' ');
            sb.Append($"{this.ValueAsText(valueHigh)}");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddNotBetween(Expression<Func<TEntity, object>> expressions, object valueLow, object valueHigh)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("NOT BETWEEN").Append(' ');
            sb.Append($"{this.ValueAsText(valueLow)}");
            sb.Append(' ');
            sb.Append("AND");
            sb.Append(' ');
            sb.Append($"{this.ValueAsText(valueHigh)}");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddIn(Expression<Func<TEntity, object>> expressions, params object[] values)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("IN").Append('(').Append(string.Join(", ", values.Select(pi => $"{this.ValueAsText(pi)}")));
            sb.Append(')');
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddNotIn(Expression<Func<TEntity, object>> expressions, params object[] values)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("NOT IN").Append('(').Append(' ');
            sb.Append(string.Join(", ", values.Select(pi => $"{this.ValueAsText(pi)}")));
            sb.Append(')');
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddLike(Expression<Func<TEntity, object>> expressions, string value)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("LIKE").Append(' ').Append($"'{value}'");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddNotLike(Expression<Func<TEntity, object>> expressions, string value)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("NOT LIKE").Append(' ').Append($"'{value}'");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddIsNull(Expression<Func<TEntity, object>> expressions)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append("IS NULL");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddIsNotNull(Expression<Func<TEntity, object>> expressions)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append("IS NOT NULL");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AddGlob(Expression<Func<TEntity, object>> expressions, string value)
        {
            const string WHERE = "WHERE";

            this.PropertyNameWhere = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(WHERE).Append(' ');
            }

            sb.Append('(');
            sb.Append(this.PropertyNameWhere).Append(' ');
            sb.Append(' ').Append("GLOB").Append(' ').Append($"'{value}'");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> OrderBy(Expression<Func<TEntity, object>> expressions, SQLSorting sorting = SQLSorting.Ascending)
        {
            const string FROMTAB = "FROM";
            const string ORDERBY = "ORDER BY";
            string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(FROMTAB, StringComparison.OrdinalIgnoreCase) == true)
            {
                sb.Append(' ').Append('\n').Append(ORDERBY).Append(' ');
                sb.Append('\n').Append(propertyName).Append(' ').Append(this.SortingAsText(sorting));
            }

            return this;
        }

        public ISQLGenerator<TEntity> AndOrderBy(Expression<Func<TEntity, object>> expressions, SQLSorting sorting = SQLSorting.Ascending)
        {
            const string FROMTAB = "FROM";
            const string ORDERBY = "ORDER BY";
            string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains(FROMTAB, StringComparison.OrdinalIgnoreCase) == true && sb.ToString().Contains(ORDERBY, StringComparison.OrdinalIgnoreCase) == true)
            {
                sb.Append(',').Append('\n').Append(propertyName).Append(' ').Append(this.SortingAsText(sorting));
            }

            return this;
        }

        public ISQLGenerator<TEntity> AddGroupBy(params Expression<Func<TEntity, object>>[] expressions)
        {
            const string FROM = "FROM";
            const string WHERE = "WHERE";
            const string GROUPBY = "GROUP BY";

            if (sb.ToString().Contains(FROM, StringComparison.OrdinalIgnoreCase) == true && sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append(GROUPBY).Append(' ');
                sb.Append(' ').Append(string.Join(", ", expressions.Select(s => ExpressionPropertyName.For<TEntity>(s))));
            }

            return this;
        }

        public ISQLGenerator<TEntity> AddHaving(params Expression<Func<TEntity, object>>[] expressions)
        {
            const string FROM = "FROM";
            const string WHERE = "WHERE";
            const string GROUPBY = "GROUP BY";
            const string HAVING = "HAVING";

            if (sb.ToString().Contains(FROM, StringComparison.OrdinalIgnoreCase) == true
                && sb.ToString().Contains(WHERE, StringComparison.OrdinalIgnoreCase) == true 
                && sb.ToString().Contains(GROUPBY, StringComparison.OrdinalIgnoreCase) == true)
            {
                sb.Append(' ').Append('\n').Append(HAVING).Append(' ');
                sb.Append(' ').Append(string.Join(", ", expressions.Select(s => ExpressionPropertyName.For<TEntity>(s))));
            }

            return this;
        }


        public string ToSql()
        {
            string result = sb.ToString();
            sb.Clear();
            return result;
        }

        #region Disposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool classDisposing = false)
        {
            if (this.classIsDisposed == false)
            {
                if (classDisposing == true)
                {
                }
                else
                {
                    sb.Clear();
                    sb = null;
                }
            }

            this.classIsDisposed = true;
        }

        #endregion Disposable

        private string DataTableAttributes()
        {
            string tableName = string.Empty;
            tableName = typeof(TEntity).GetAttributeValue((DataTableAttribute table) => table.TableName);
            if (string.IsNullOrEmpty(tableName) == true)
            {
                tableName = typeof(TEntity).Name;
            }

            return tableName;
        }

        private IEnumerable<TableColumnAttribute> CustomerAttributesTableColumn()
        {
            IEnumerable<TableColumnAttribute>  obj = typeof(TEntity).GetProperties()
                .SelectMany(p => p.GetCustomAttributes())
                .OfType<TableColumnAttribute>()
                .AsParallel();

            return obj;
        }

        private List<string> CustomerAttributesPK()
        {
            List<string> obj = null;
            obj = typeof(TEntity).GetProperties()
                .SelectMany(p => p.GetCustomAttributes())
                .OfType<PrimaryKeyAttribute>()
                .AsParallel()
                .Select(s => s.ColumnName)
                .ToList();

            return obj;
        }

        private DateTime ConvertToDateTime(string str)
        {
            string pattern = @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})\.(\d{3})";
            if (Regex.IsMatch(str, pattern) == true)
            {
                Match match = Regex.Match(str, pattern);
                int year = Convert.ToInt32(match.Groups[1].Value);
                int month = Convert.ToInt32(match.Groups[2].Value);
                int day = Convert.ToInt32(match.Groups[3].Value);
                int hour = Convert.ToInt32(match.Groups[4].Value);
                int minute = Convert.ToInt32(match.Groups[5].Value);
                int second = Convert.ToInt32(match.Groups[6].Value);
                int millisecond = Convert.ToInt32(match.Groups[7].Value);
                return new DateTime(year, month, day, hour, minute, second, millisecond);
            }
            else
            {
                throw new Exception("Unable to parse.");
            }
        }

        private string SortingAsText(SQLSorting sqlSorting)
        {
            string result = string.Empty;

            if (sqlSorting == SQLSorting.Ascending)
            {
                result = "ASC";
            }
            else if (sqlSorting == SQLSorting.Descending)
            {
                result = "DESC";
            }
            else
            {
                result = "ASC";
            }

            return result;
        }

        private string WhereOperatorAsText(SQLComparison sqlCompare)
        {
            string result = string.Empty;

            if (sqlCompare == SQLComparison.None)
            {
                result = string.Empty;
            }
            else if (sqlCompare == SQLComparison.Equals)
            {
                result = " = ";
            }
            else if (sqlCompare == SQLComparison.NotEquals)
            {
                result = " <> ";
            }
            else if (sqlCompare == SQLComparison.GreaterOrEquals)
            {
                result = " >= ";
            }
            else if (sqlCompare == SQLComparison.GreaterThan)
            {
                result = " > ";
            }
            else if (sqlCompare == SQLComparison.LessOrEquals)
            {
                result = " <= ";
            }
            else if (sqlCompare == SQLComparison.GreaterThan)
            {
                result = " < ";
            }
            else if (sqlCompare == SQLComparison.In)
            {
                result = " IN(@) ";
            }
            else if (sqlCompare == SQLComparison.NotIn)
            {
                result = " NOT IN(@) ";
            }
            else if (sqlCompare == SQLComparison.Like)
            {
                result = " LIKE ";
            }
            else if (sqlCompare == SQLComparison.NotLike)
            {
                result = " NOT LIKE ";
            }
            else if (sqlCompare == SQLComparison.IsNull)
            {
                result = " IS NULL ";
            }
            else if (sqlCompare == SQLComparison.IsNotNull)
            {
                result = " IS NOT NULL";
            }

            return result;
        }

        private string ValueAsText(object compareValue)
        {
            string result = string.Empty;

            if (compareValue.GetType() == typeof(string))
            {
                result = $"'{compareValue.ToString()}'";
            }
            else if (compareValue.GetType() == typeof(Guid))
            {
                result = $"'{compareValue.ToString()}'";
            }
            else if (compareValue.GetType() == typeof(bool))
            {
                if ((bool)compareValue == true)
                {
                    result = "1";
                }
                else
                {
                    result = "0";
                }
            }
            else if (compareValue.GetType() == typeof(int))
            {
                result = ((int)compareValue).ToString();
            }
            else if (compareValue.GetType() == typeof(long))
            {
                result = ((long)compareValue).ToString();
            }
            else if (compareValue.GetType() == typeof(double))
            {
                result = ((double)compareValue).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            else if (compareValue.GetType() == typeof(decimal))
            {
                result = ((decimal)compareValue).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            else if (compareValue.GetType() == typeof(DateTime))
            {
                result = $"'{Convert.ToDateTime(compareValue).ToString("yyyy-MM-dd HH:mm:ss")}'";
            }

            return result;
        }
    }

    public enum SelectOperator : int
    {
        [Description("Alle Datensätze")]
        All = 0,
        [Description("Count Anzahl")]
        Count = 1,
        [Description("SQL Anweisung")]
        Direct = 3,
    }
}
