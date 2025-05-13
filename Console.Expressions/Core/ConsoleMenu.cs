//-----------------------------------------------------------------------
// <copyright file="ConsoleMenu.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.05.2025 14:27:39</date>
//
// <summary>
// Klasse zum Erstellen und Auswählen von Menüpunkten
// </summary>
//-----------------------------------------------------------------------

namespace Console.Expressions
{
    using System;
    using System.Linq;

    public class ConsoleMenu
    {
        private static Dictionary<string,string> menuList = new Dictionary<string,string>();
        private static List<char> inputKeys = new List<char>();

        public static void Add(string mpoint, string mtext)
        {
            if (menuList.ContainsKey(mpoint) == false)
            {
                menuList.Add(mpoint, mtext);
            }
        }

        public static void Wait(string text = "")
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorVisible = false;
            if (string.IsNullOrEmpty(text) == true)
            {
                Console.Write('\n');
                Console.WriteLine("Eine Taste drücken, um zum Menü zurück zukehren!");
            }
            else
            {
                Console.Write('\n');
                Console.WriteLine(text);
            }

            Console.ForegroundColor = defaultColor;
            Console.ReadKey();
            Console.CursorVisible = true;
        }

        public static string SelectKey()
        {
            string resultKeys = string.Empty;
            ConsoleColor defaultColor = Console.ForegroundColor;

            Console.Clear();
            foreach (KeyValuePair<string, string> mtext in menuList)
            {
                Console.WriteLine($"{mtext.Key}. {mtext.Value}");
            }

            if (menuList.ContainsKey("X") == false)
            {
                menuList.Add("X", "Beenden");
                Console.WriteLine("X. Beenden");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('\n');
            Console.WriteLine("Wählen Sie einen Menüpunkt oder 'x' für beenden [mit ESC letze Eingabe löschen!]");
            Console.ForegroundColor = defaultColor;

            do
            {
                Console.CursorVisible = false;
                char key = Console.ReadKey(true).KeyChar;
                if (key == '\u001b')
                {
                    inputKeys.Clear();
                    continue;
                }

                if (inputKeys != null && inputKeys.Count == 0)
                {
                    foreach (var _ in menuList.Where(menu => menu.Key.StartsWith(key.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase) == true).Select(menu => new { }))
                    {
                        inputKeys.Add(key);
                        break;
                    }
                }
                else if (inputKeys != null && inputKeys.Count == 1)
                {
                    inputKeys.Add(key);
                }


                string selectedKeys = string.Join("-", inputKeys);

                if (menuList.ContainsKey(selectedKeys.ToUpper().Replace("-",string.Empty)) == true)
                {
                    resultKeys = selectedKeys.ToUpper().Replace("-", string.Empty);
                    inputKeys.Clear();
                    Console.CursorVisible = true;
                    break;
                }

            } while (true);

            return resultKeys;
        }
    }
}
