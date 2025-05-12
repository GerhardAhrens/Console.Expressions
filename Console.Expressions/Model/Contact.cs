//-----------------------------------------------------------------------
// <copyright file="Contact.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.05.2025 14:27:39</date>
//
// <summary>
// Model Klasse zu Contact
// </summary>
//-----------------------------------------------------------------------

namespace Console.Model
{
    using System;
    using Console.Expressions;

    [DataTable("TAB_Contact")]
    public class Contact
    {
        public Contact(string name, DateTime birthday)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Birthday = birthday;
        }

        [PrimaryKey]
        [TableColumn(SQLiteDataType.Guid)]
        public Guid Id { get; set; }

        [TableColumn(SQLiteDataType.Text, 50)]
        public string Name { get; }

        [TableColumn(SQLiteDataType.Integer)]
        public int Age { get; }

        [TableColumn(SQLiteDataType.DateTime)]
        public DateTime Birthday { get; }
    }
}
