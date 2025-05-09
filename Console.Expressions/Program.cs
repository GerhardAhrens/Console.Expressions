//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.05.2025 14:27:39</date>
//
// <summary>
// Konsolen Applikation mit Menü
// </summary>
//-----------------------------------------------------------------------

namespace Console.Expressions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using static Console.Expressions.Program;

    public class Program
    {
        private static void Main(string[] args)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("1. Insert");
                Console.WriteLine("2. Select");
                Console.WriteLine("3. Select Where");
                Console.WriteLine("X. Beenden");

                Console.WriteLine("Wählen Sie einen Menüpunkt oder 'x' für beenden");
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.X)
                {
                    Environment.Exit(0);
                }
                else
                {
                    if (key == ConsoleKey.D1)
                    {
                        MenuPoint1();
                    }
                    else if (key == ConsoleKey.D2)
                    {
                        MenuPoint2();
                    }
                    else if (key == ConsoleKey.D3)
                    {
                        MenuPoint3();
                    }
                }
            }
            while (true);
        }

        private static void MenuPoint1()
        {
            Console.Clear();

            Contact contact = new Contact("Gerhard", 64);

            ContactRepository<Contact> cr = new ContactRepository<Contact>(contact);
            string result = cr.Insert().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 1, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            ContactRepository<Contact> cr = new ContactRepository<Contact>(null);
            string result = cr.Select().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint3()
        {
            Console.Clear();

            ContactRepository<Contact> cr = new ContactRepository<Contact>(null);
            string result = cr.Select().Where(x => x.Age, "=", 64).AndWhere(w => w.Name, "=", "Gerhard").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        public interface IRepository<TEntity>
        {
            IRepository<TEntity> Insert();

            IRepository<TEntity> Select();

            IRepository<TEntity> Where(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value);
            IRepository<TEntity> AndWhere(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value);

            string ToSql();
        }

        public class ContactRepository<TEntity> : IRepository<TEntity>
        {
            private StringBuilder sb = new StringBuilder();

            public ContactRepository(TEntity entity)
            {
                this.Entity = entity;
            }

            public TEntity Entity { get; private set; }

            public IRepository<TEntity> Insert()
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

            public IRepository<TEntity> Select()
            {
                PropertyInfo[] propInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                sb.Append($"SELECT").Append(' ');
                sb.Append(string.Join(", ", propInfos.Select(s => s.Name)));
                sb.Append(' ').Append('\n').Append("FROM").Append(' ');
                sb.Append($"{typeof(TEntity).Name}");

                return this;
            }

            public IRepository<TEntity> Where(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value)
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

            public IRepository<TEntity> AndWhere(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value)
            {
                string propertyName = ExpressionPropertyName.For<TEntity>(expressions);

                if (sb.ToString().Contains("WHERE",StringComparison.OrdinalIgnoreCase) == true)
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
        }

        public class Contact
        {
            public Contact(string name, int age)
            {
                this.Id = Guid.NewGuid();
                this.Name = name;
                this.Age = age;
            }

            public Guid Id { get; set; }
            public string Name { get; }
            public int Age { get; }
        }
    }

    public class ExpressionPropertyName
    {
        public static string For<TProperty>(Expression<Func<TProperty, object>> expression)
        {
            return GetMemberName(expression);
        }

        public static string For(Expression<Func<string>> expression)
        {
            return GetMemberName(expression);
        }

        private static string GetMemberName(LambdaExpression expression)
        {
            var currentExpression = expression.Body;

            while (true)
            {
                switch (currentExpression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return ((ParameterExpression)currentExpression).Name;
                    case ExpressionType.MemberAccess:
                        return ((MemberExpression)currentExpression).Member.Name;
                    case ExpressionType.Call:
                        return ((MethodCallExpression)currentExpression).Method.Name;
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        currentExpression = ((UnaryExpression)currentExpression).Operand;
                        break;
                    case ExpressionType.Invoke:
                        currentExpression = ((InvocationExpression)currentExpression).Expression;
                        break;
                    case ExpressionType.ArrayLength:
                        return "Length";
                    default:
                        throw new Exception("not a proper member selector");
                }
            }
        }

        public static string NameOfProperty<T>(Expression<Func<T, object>> expression)
        {
            // Validate argument.
            _ = expression ?? throw new ArgumentNullException(nameof(expression));
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new ArgumentException("The expression must be a member access expression", nameof(expression));
            }

            // Now that we know we have a member expression in the body
            // we can cast it, get a reference to the MemberInfo
            // and return the property name.
            var memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }
    }
}
