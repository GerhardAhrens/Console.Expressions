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
        ISQLGenerator<TEntity> Update();
        ISQLGenerator<TEntity> Update(params Expression<Func<TEntity, object>>[] expressions);
        ISQLGenerator<TEntity> Delete(params Expression<Func<TEntity, object>>[] expressions);
        ISQLGenerator<TEntity> Select(SelectOperator selectOperator = SelectOperator.All);
        ISQLGenerator<TEntity> Distinct();
        ISQLGenerator<TEntity> Take(int limit);

        ISQLGenerator<TEntity> Where(Expression<Func<TEntity, object>> expressions, SQLComparison sqlOperator, object value);
        ISQLGenerator<TEntity> AddWhereAnd(Expression<Func<TEntity, object>> expressions, SQLComparison sqlOperator, object value = null);
        ISQLGenerator<TEntity> AddWhereOr(SQLComparison sqlOperator, object value);
        ISQLGenerator<TEntity> AddBetween(Expression<Func<TEntity, object>> expressions, object valueLow, object valueHigh);
        ISQLGenerator<TEntity> AddNotBetween(Expression<Func<TEntity, object>> expressions, object valueLow, object valueHigh);
        ISQLGenerator<TEntity> AddIn(Expression<Func<TEntity, object>> expressions, params object[] values);
        ISQLGenerator<TEntity> AddNotIn(Expression<Func<TEntity, object>> expressions, params object[] values);
        ISQLGenerator<TEntity> AddLike(Expression<Func<TEntity, object>> expressions, string value);
        ISQLGenerator<TEntity> AddNotLike(Expression<Func<TEntity, object>> expressions, string value);
        ISQLGenerator<TEntity> AddGlob(Expression<Func<TEntity, object>> expressions, string value);
        ISQLGenerator<TEntity> AddIsNull(Expression<Func<TEntity, object>> expressions);
        ISQLGenerator<TEntity> AddIsNotNull(Expression<Func<TEntity, object>> expressions);

        ISQLGenerator<TEntity> OrderBy(Expression<Func<TEntity, object>> expressions, SQLSorting sorting = SQLSorting.Ascending);
        ISQLGenerator<TEntity> AndOrderBy(Expression<Func<TEntity, object>> expressions, SQLSorting sorting = SQLSorting.Ascending);

        ISQLGenerator<TEntity> AddGroupBy(params Expression<Func<TEntity, object>>[] expressions);
        string ToSql();
    }
}
