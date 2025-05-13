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
// Konsolen Applikation zum Test des SQLGenerator
// </summary>
// <Link>
// https://www.sqlitetutorial.net/
// </Link>
//-----------------------------------------------------------------------

namespace Console.Expressions
{
    using System;
    using System.ComponentModel;

    using Console.Model;

    public partial class Program
    {
        private static List<ConsoleKey> inputKeys = new List<ConsoleKey>();

        private static void Main(string[] args)
        {

            do
            {
                ConsoleMenu.Add("1", "Create Tabele");
                ConsoleMenu.Add("2", "Insert");
                ConsoleMenu.Add("A1", "Select");
                ConsoleMenu.Add("A2", "Select Where And");
                ConsoleMenu.Add("A3", "Select Where Or");
                ConsoleMenu.Add("A4", "Select Limit, Count");
                ConsoleMenu.Add("A5", "Select Order By");
                ConsoleMenu.Add("A6", "Select Direct SQL");
                ConsoleMenu.Add("B1", "Select Between / Not Between");
                ConsoleMenu.Add("B2", "Select In / Not In");
                ConsoleMenu.Add("B3", "Select Like / Not Like / Glob");
                ConsoleMenu.Add("B4", "Select IsNull / IsNotNull");
                ConsoleMenu.Add("C1", "Select Group By");
                ConsoleMenu.Add("D1", "Update");
                ConsoleMenu.Add("D2", "Delete");
                string selectKey = ConsoleMenu.SelectKey();

                if (selectKey == "X")
                {
                    Environment.Exit(0);
                }
                else if (selectKey == "1")
                {
                    MenuPoint1();
                }
                else if (selectKey == "2")
                {
                    MenuPoint2();
                }
                else if (selectKey == "A1")
                {
                    MenuPointA1();
                }
                else if (selectKey == "A2")
                {
                    MenuPointA2();
                }
                else if (selectKey == "A3")
                {
                    MenuPointA3();
                }
                else if (selectKey == "A4")
                {
                    MenuPointA4();
                }
                else if (selectKey == "A5")
                {
                    MenuPointA5();
                }
                else if (selectKey == "A6")
                {
                    MenuPointA6();
                }
                else if (selectKey == "B1")
                {
                    MenuPointA7();
                }
                else if (selectKey == "B2")
                {
                    MenuPointA8();
                }
                else if (selectKey == "B3")
                {
                    MenuPointA9();
                }
                else if (selectKey == "B4")
                {
                    MenuPointB4();
                }
                else if (selectKey == "C1")
                {
                    MenuPointC1();
                }
                else if (selectKey == "D1")
                {
                    MenuPointD1();
                }
                else if (selectKey == "D2")
                {
                    MenuPointD2();
                }
            }
            while (true);
        }

        private static void MenuPoint1()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.CreateTable().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            Contact contact = new Contact("Gerhard", new DateTime(1960,6,28),22.65M);

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(contact);
            string result = cr.Insert().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA1()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select(x => x.Name, x => x.Age).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select(x => x.Name).Distinct().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA2()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AddWhereAnd(w => w.Name, SQLComparison.Equals, "Gerhard").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.GreaterThan, 60).AddWhereAnd(w => w.IsActive, SQLComparison.IsNull).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.In, "91,64").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.NotIn, "91,64").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AddWhereAnd(w => w.Name, SQLComparison.Equals, "Gerhard").AddWhereAnd(x => x.IsActive,SQLComparison.Equals, true).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Name, SQLComparison.Like, "ger%").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA3()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AddWhereOr(SQLComparison.Equals, 2).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AddWhereOr(SQLComparison.Equals, 2).AddWhereOr(SQLComparison.Equals,5).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA4()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select(SelectOperator.Count).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Take(2).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA5()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string resultOrderBy = cr.Select().OrderBy(x => x.Age).ToSql();
            Console.Write($"Ergebnis:{resultOrderBy}");
            Console.Write('\n');
            Console.Write('\n');

            resultOrderBy = cr.Select().OrderBy(x => x.Age).AndOrderBy(y => y.Name, SQLSorting.Descending).ToSql();

            Console.Write($"Ergebnis:{resultOrderBy}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA6()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string resultSQLDirect = cr.Select(SelectOperator.Direct,"select * from Tabelle").ToSql();

            Console.Write($"Ergebnis:{resultSQLDirect}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA7()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().AddBetween(x => x.Age,70,100).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddNotBetween(x => x.Age, 70, 100).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddBetween(x => x.Betrag, 23.00M, 100.00M).AddWhereAnd(x => x.IsActive,SQLComparison.Equals,true).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select(SelectOperator.Direct, "select Id, Name, date(Birthday) Birthday, Age, Betrag, IsActive from tab_contact").AddBetween(x => x.Birthday, "1910-01-01", "1930-12-31").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA8()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().AddIn(x => x.Age, 64, 65).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddNotIn(x => x.Age, 64, 65).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddIn(x => x.Age, 64, 65).AddWhereAnd(x => x.IsActive, SQLComparison.Equals, true).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointA9()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().AddLike(x => x.Name,"Ger%").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddNotLike(x => x.Name, "Ger%").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddLike(x => x.Name, "Ger%").AddWhereAnd(x => x.IsActive, SQLComparison.Equals, true).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddGlob(x => x.Name, "?er*").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddGlob(x => x.Age, "*[5-9]*").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointB4()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().AddIsNull(x => x.IsActive).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddIsNotNull(x => x.IsActive).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().AddIsNull(x => x.IsActive).AddWhereAnd(x => x.Age, SQLComparison.GreaterThan, 50).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointC1()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select(SelectOperator.Direct, "select Name, Count(Name) from tab_contact").AddGroupBy(g => g.Name).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select(SelectOperator.Direct, "select Name, age, count(id) from tab_contact").AddGroupBy(g => g.Name, g=>g.Age).ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointD1()
        {
            Console.Clear();
            Contact contact = new Contact("Gerhard-Maus", new DateTime(1960, 6, 28), 33.66M);
            contact.Id = new Guid("6599e87d-0d4a-4548-be38-2aef47483118");

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(contact);
            string result = cr.Update().Where(w => w.Id,SQLComparison.Equals, "6599e87d-0d4a-4548-be38-2aef47483118").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Update(x => x.Name).Where(w => w.Id, SQLComparison.Equals, "6599e87d-0d4a-4548-be38-2aef47483118").ToSql();

            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }

        private static void MenuPointD2()
        {
            Console.Clear();
            Contact contact = new Contact("Gerhard-Maus", new DateTime(1960, 6, 28), 33.66M);
            contact.Id = new Guid("6599e87d-0d4a-4548-be38-2aef47483118");
            contact.IsActive = true;

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(contact);
            string result = cr.Delete(x => x.Id).ToSql();
            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Delete(x => x.Age,x => x.IsActive).ToSql();
            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Delete().Where(x => x.Age,SQLComparison.Equals,64).AddWhereAnd(w => w.IsActive,SQLComparison.Equals,true).ToSql();
            Console.Write($"{result}");
            Console.Write('\n');
            Console.Write('\n');

            ConsoleMenu.Wait();
        }
    }
}
