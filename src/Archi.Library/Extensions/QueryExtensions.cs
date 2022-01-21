using Archi.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
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

        public static IQueryable<dynamic> SelectFields<TModel>(this IQueryable<TModel> query, Params param)
        {
            if (param.HasFields())
            {
                string champ = param.Fields;
                string[] fields = champ.Split(",");


                /*var parameter = Expression.Parameter(typeof(TModel), "x");
                //var bindings2 = fields.Select(name => Expression.Property(parameter, name));

                var bindings = fields
                    .Select(name => Expression.Property(parameter, name))
                    .Select(member => Expression.Bind(member.Member, member));
                //var body = Expression.MemberInit(Expression.New(typeof(TModel)), bindings);

                dynamic aa = new ExpandoObject();


                var body = Expression.MemberInit(Expression.New(typeof(TModel)), bindings);

                var selector = Expression.Lambda<Func<TModel, dynamic>>(body, parameter);*/

                var parameter = Expression.Parameter(typeof(TModel), "x");
                var properties = fields
                    .Select(f => typeof(TModel).GetProperty(f, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    .Select(p => new DynamicProperty(p.Name, p.PropertyType))
                    .ToList();
                var resultType = DynamicClassFactory.CreateType(properties, false);
                var bindings = properties.Select(p => Expression.Bind(resultType.GetProperty(p.Name), Expression.Property(parameter, p.Name)));
                var result = Expression.MemberInit(Expression.New(resultType), bindings);
                var selector = Expression.Lambda<Func<TModel, dynamic>>(result, parameter);

                return query.Select(selector);
            }

            return (IQueryable<dynamic>)query;
        }
    }
}