using Archi.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Archi.Library.Extensions
{
    public static class QueryExtensions
    {
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, Params param, bool order)
        {
            if (param.HasOrder())
            {
                if (order == true)
                {
                    string[] champsAsc = param.Asc.Split(",");
                    string[] champsDesc = param.Desc.Split(",");


                    var finalQuery = query.OrderBy(LambdaExpression<TModel>(champsAsc[0]));

                    foreach (string champ in champsAsc)
                    {
                        var index = Array.FindIndex(champsAsc, w => w == champ);
                        if (index != 0)
                        {
                            finalQuery = finalQuery.ThenBy(LambdaExpression<TModel>(champ));
                        }
                    }
                    foreach (string champ in champsDesc)
                    {
                        finalQuery = finalQuery.ThenByDescending(LambdaExpression<TModel>(champ));
                    }
                    return finalQuery;
                } else
                {
                    string[] champsDesc = param.Desc.Split(",");
                    string[] champsAsc = param.Asc.Split(",");

                    var finalQuery = query.OrderByDescending(LambdaExpression<TModel>(champsDesc[0]));

                    foreach (string champ in champsDesc)
                    {
                        var index = Array.FindIndex(champsDesc, w => w == champ);
                        if (index != 0)
                            finalQuery = finalQuery.ThenByDescending(LambdaExpression<TModel>(champ));
                    }
                    foreach (string champ in champsAsc)
                    {
                        {
                            finalQuery = finalQuery.ThenBy(LambdaExpression<TModel>(champ));
                        }
                    }
                    return finalQuery;
                }
            }
            return (IOrderedQueryable<TModel>)query;
        }

        public static Expression<Func<TModel, object>> LambdaExpression<TModel>(string champ)
        {
            // création lambda (And/Or apres expressions pour condition)
            var parameter = Expression.Parameter(typeof(TModel), "x");
            var property = Expression.Property(parameter, champ);
            var convert = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<TModel, object>>(convert, parameter);

            return lambda;
        }
    }
}