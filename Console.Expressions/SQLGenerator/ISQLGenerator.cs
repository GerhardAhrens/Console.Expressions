//-----------------------------------------------------------------------
// <copyright file="ISQLGenerator.cs" company="Lifeprojects.de">
//     Class: ISQLGenerator
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.05.2025 14:27:39</date>
//
// <summary>
// Interface zum SQLGenerator
// </summary>
//-----------------------------------------------------------------------

namespace Console.Expressions
{
    using System;
    using System.Linq.Expressions;

    public interface ISQLGenerator<TEntity>
    {
        ISQLGenerator<TEntity> CreateTable();
        ISQLGenerator<TEntity> Insert();
        ISQLGenerator<TEntity> Select(SelectOperator selectOperator = SelectOperator.All, int limit = 0);

        ISQLGenerator<TEntity> Where(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value);
        ISQLGenerator<TEntity> AndWhere(Expression<Func<TEntity, object>> expressions, string sqlOperator, object value);

        string ToSql();
    }
}
