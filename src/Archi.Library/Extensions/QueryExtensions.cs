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
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, Params param, string order)
        {
            if (param.HasOrder())
            {
                if (order == "ascToDesc" || order == "asc")
                {
                    string[] champsAsc = param.Asc.Split(","); // param.Asc.Split(",");
                    string[] champsDesc = order == "ascToDesc" ? param.Desc.Split(",") : new string[] { }; // new string[] { };

                    var finalQuery = query.OrderBy(SortExpression<TModel>(champsAsc[0]));

                    foreach (string champ in champsAsc)
                    {
                        var index = Array.FindIndex(champsAsc, w => w == champ);
                        if (index != 0)
                        {
                            finalQuery = finalQuery.ThenBy(SortExpression<TModel>(champ));
                        }
                    }
                    foreach (string champ in champsDesc)
                    {
                        finalQuery = finalQuery.ThenByDescending(SortExpression<TModel>(champ));
                    }
                    return finalQuery;
                } else if (order == "descToAsc" || order == "desc")
                {
                    string[] champsAsc = order == "descToAsc" ? param.Asc.Split(",") : new string[] { }; // param.Asc.Split(",");
                    string[] champsDesc = param.Desc.Split(","); // new string[] { };

                    var finalQuery = query.OrderByDescending(SortExpression<TModel>(champsDesc[0]));

                    foreach (string champ in champsDesc)
                    {
                        var index = Array.FindIndex(champsDesc, w => w == champ);
                        if (index != 0)
                            finalQuery = finalQuery.ThenByDescending(SortExpression<TModel>(champ));
                    }
                    foreach (string champ in champsAsc)
                    {
                        {
                            finalQuery = finalQuery.ThenBy(SortExpression<TModel>(champ));
                        }
                    }
                    return finalQuery;
                } else
                {

                }
            }
            return (IOrderedQueryable<TModel>)query;
        }

        public static Expression<Func<TModel, object>> SortExpression<TModel>(string champ)
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