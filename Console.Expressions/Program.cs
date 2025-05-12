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
        private static void Main(string[] args)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("1. Create Tabele");
                Console.WriteLine("2. Insert");
                Console.WriteLine("3. Select");
                Console.WriteLine("4. Select Where And");
                Console.WriteLine("5. Select Where Or");
                Console.WriteLine("6. Select Limit, Count");
                Console.WriteLine("7. Select Order by");
                Console.WriteLine("8. Select Direct SQL");
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
                    else if (key == ConsoleKey.D4)
                    {
                        MenuPoint4();
                    }
                    else if (key == ConsoleKey.D5)
                    {
                        MenuPoint5();
                    }
                    else if (key == ConsoleKey.D6)
                    {
                        MenuPoint6();
                    }
                    else if (key == ConsoleKey.D7)
                    {
                        MenuPoint7();
                    }
                    else if (key == ConsoleKey.D8)
                    {
                        MenuPoint8();
                    }
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

            Console.WriteLine("Menüpunkt 1, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            Contact contact = new Contact("Gerhard", new DateTime(1960,6,28));

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(contact);
            string result = cr.Insert().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 1, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint3()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint4()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AndWhere(w => w.Name, SQLComparison.Equals, "Gerhard").ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).AndWhere(w => w.Name, SQLComparison.Equals, "Gerhard").AndWhere(x => x.IsActive,SQLComparison.Equals, true).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint5()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string result = cr.Select().Where(x => x.Age, SQLComparison.Equals, 64).OrWhere(SQLComparison.Equals, 2).ToSql();

            Console.Write(result);
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint6()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string resultCount = cr.Select(SelectOperator.Count).ToSql();

            Console.Write($"Select Count:{resultCount}");
            Console.Write('\n');
            Console.Write('\n');

            string resultLimit = cr.Select(SelectOperator.Limit,2).ToSql();

            Console.Write($"Select Limit:{resultLimit}");
            Console.Write('\n');
            Console.Write('\n');

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint7()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string resultOrderBy = cr.Select().OrderBy(x => x.Age).ToSql();

            resultOrderBy = cr.Select().OrderBy(x => x.Age).AndOrderBy(y => y.Name, SQLSorting.Descending).ToSql();

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }

        private static void MenuPoint8()
        {
            Console.Clear();

            SQLGenerator<Contact> cr = new SQLGenerator<Contact>(null);
            string resultOrderBy = cr.Select(SelectOperator.Direct,"").ToSql();

            Console.WriteLine("Menüpunkt 2, eine Taste drücken für zurück!");
            Console.ReadKey();
        }
    }
}
