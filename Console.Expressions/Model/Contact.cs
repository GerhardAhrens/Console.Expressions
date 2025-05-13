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
        private int age = 0;

        public Contact(string name, DateTime birthday)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Birthday = birthday;
            this.Age = this.GetAge(birthday);
        }

        public Contact(string name, DateTime birthday, decimal betrag)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Birthday = birthday;
            this.Age = this.GetAge(birthday);
            this.Betrag = betrag;
        }

        [PrimaryKey]
        [TableColumn(SQLiteDataType.Guid)]
        public Guid Id { get; set; }

        [TableColumn(SQLiteDataType.Text, 50)]
        public string Name { get; }

        [TableColumn(SQLiteDataType.DateTime)]
        public DateTime Birthday { get; }

        [TableColumn(SQLiteDataType.Integer)]
        public int Age
        {
            get
            {
                return this.GetAge(this.Birthday);
            }
            private set 
            {
                this.age = value;
            }
        }

        [TableColumn(SQLiteDataType.Decimal,8,3)]
        public decimal Betrag { get; }


        [TableColumn(SQLiteDataType.Boolean)]
        public bool IsActive { get; }

        private int GetAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

            return (a - b) / 10000;
        }
    }
}
