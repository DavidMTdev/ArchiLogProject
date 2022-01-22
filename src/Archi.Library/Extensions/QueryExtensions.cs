using Archi.Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, Params param, string order)
        {
            if (param.HasOrder())
            {
                string[] champsDesc = order != "asc" ? param.Desc.Split(",") : new string[] { }; // new string[] { };
                string[] champsAsc = order != "desc" ? param.Asc.Split(",") : new string[] { }; // param.Asc.Split(",");

                if (order == "ascToDesc" || order == "asc")
                {
                    //string[] champsAsc = param.Asc.Split(","); // param.Asc.Split(",");

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
                    //string[] champsDesc = param.Desc.Split(","); // new string[] { };

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
                } 
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

        public static Expression<Func<TModel, object>> SortExpression<TModel>(string champ)
        {
            // création lambda (And/Or apres expressions pour condition)
            var parameter = Expression.Parameter(typeof(TModel), "x");
            var property = Expression.Property(parameter, champ);
            var convert = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<TModel, object>>(convert, parameter);

            return lambda;

        }


        public static IQueryable<TModel> QuerySearch<TModel>(this IQueryable<TModel> query, Params param, IQueryCollection searchFields)
        {
            /*IEnumerable<KeyValuePair<string, StringValues>>[] array = new IEnumerable<KeyValuePair<string, StringValues>>[] { };

            foreach(KeyValuePair<string, StringValues> search in searchFields)
            {
                try
                {
                   var test2 = typeof(Params).GetProperty(search.Key);
                }
                catch
                {
                    array.;
                }
            }*/
                
            var result = query;

            var test = searchFields.Select(f => LikeExpression<TModel>(f.Key, f.Value));

            foreach(Expression<Func<TModel, bool>> exp in test)
            {
                result = result.Where(exp);
            }

            return result;
            
        }

        public static Expression<Func<TModel, bool>> LikeExpression<TModel>(string property, string value)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            var propertyInfo = typeof(TModel).GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var member = Expression.Property(param, propertyInfo.Name);

            var startWith = value.StartsWith("*");
            var endsWith = value.EndsWith("*");

            var searchValue = "";
            searchValue = value.Replace("*", "");

            var constant = Expression.Constant(searchValue);
            Expression exp;

            if (endsWith && startWith)
            {
                exp = Expression.Call(member, "Contains", null, constant);
            }
            else if (startWith)
            {
                exp = Expression.Call(member, "EndsWith", null, constant);
            }
            else if (endsWith)
            {
                exp = Expression.Call(member, "StartsWith", null, constant);
            }
            else
            {
                exp = Expression.Equal(member, constant);
            }

            return Expression.Lambda<Func<TModel, bool>>(exp, param);
        }
    }
}