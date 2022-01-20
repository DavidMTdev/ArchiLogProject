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
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, Params param)
        {
            if (param.HasOrder())
            {
                string champ = param.Asc;
                // var property = typeof(TModel).GetProperty(champ, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                // return query.OrderBy(x => x.GetType().GetProperty(champ, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));

                // création lambda (And/Or apres expressions pour condition)
                var parameter = Expression.Parameter(typeof(TModel), "x");
                var property = Expression.Property(parameter, champ);
                var convert = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<TModel, object>>(convert, parameter);

                // utilisation
                return query.OrderBy(lambda);

            }
            return (IOrderedQueryable<TModel>)query;
        }
    }
}