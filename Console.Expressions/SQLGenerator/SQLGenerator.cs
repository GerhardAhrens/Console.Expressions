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
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

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
                        sb.Append($"{column.ColumnName} DECIMAL({column.Length},{column.AfterComma},").Append('\n');
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
            sb.Clear();

            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.Append($"INSERT INTO {typeof(TEntity).Name}").Append(' ');
            sb.Append('(');
            sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
            sb.Append(')').Append(' ');
            sb.Append('\n').Append("VALUES").Append('\n');
            sb.Append(' ').Append('(');
            sb.Append(string.Join(", ", propInfos.Select(s => $"'{s.GetValue(this.Entity)}'")));
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> Select()
        {
            sb.Clear();

            PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.Append($"SELECT").Append(' ');
            sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
            sb.Append(' ').Append('\n').Append("FROM").Append(' ');
            sb.Append($"{typeof(TEntity).Name}");

            return this;
        }

        public ISQLGenerator<TEntity> Where(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value)
        {
            string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains("WHERE", StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(' ').Append('\n').Append("WHERE").Append(' ');
            }

            sb.Append('(');
            sb.Append(propertyName);
            sb.Append(' ').Append(sqlOperator).Append(' ');
            sb.Append($"'{value}'");
            sb.Append(')');

            return this;
        }

        public ISQLGenerator<TEntity> AndWhere(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value)
        {
            string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

            if (sb.ToString().Contains("WHERE", StringComparison.OrdinalIgnoreCase) == true)
            {
                sb.Append(' ').Append('\n').Append("AND").Append(' ');
                sb.Append('(');
                sb.Append(propertyName);
                sb.Append(' ').Append(sqlOperator).Append(' ');
                sb.Append($"'{value}'");
                sb.Append(')');
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
    }
}
