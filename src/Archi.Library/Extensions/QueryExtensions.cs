using Archi.Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public static IQueryable<TModel> Pagination<TModel>(this IQueryable<TModel> query, Params param, String URL,String QueryString, Microsoft.AspNetCore.Http.HttpResponse response)
        {
            if (param.HasRange())
            {
                string Range = param.Range;
                string[] RangeSplit = Range.Split('-');
                int RangeValue = int.Parse(RangeSplit[1]) - int.Parse(RangeSplit[0]) +1;
                int SkipValue = int.Parse(RangeSplit[0]);
                int MaxRange = query.Count();

                // Rel
                string first = (SkipValue != 0) 
                    ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], "0", (RangeValue-1).ToString(), "first") + ", "
                    : "";
                string next = (int.Parse(RangeSplit[1]) != MaxRange) 
                    ? URL + getRel(QueryString,RangeSplit[0],RangeSplit[1],(int.Parse(RangeSplit[1])+1).ToString(),(MaxRange < (int.Parse(RangeSplit[1]) + RangeValue)) 
                    ? MaxRange.ToString() 
                    : (int.Parse(RangeSplit[1]) + RangeValue).ToString(),"next") + ", "
                    : "";
                string last = (int.Parse(RangeSplit[1]) != MaxRange) 
                    ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], (MaxRange-RangeValue+1).ToString(), MaxRange.ToString(), "last") 
                    : "" ;
                string prev = (SkipValue != 0) 
                    ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], (SkipValue - RangeValue) != 0 
                    ? (SkipValue - RangeValue - 1).ToString() 
                    : "0", (SkipValue - 1).ToString(), "prev") + (next != "" ? ", " : "") 
                    : "";
                response.Headers.Add("Content-Range", $"{Range}/{MaxRange}");
                response.Headers.Add("Accept-Range", $"Product 50");
                response.Headers.Add("Link", $"{first}{prev}{next}{last}");
                return query.Skip(SkipValue).Take(RangeValue);
            }
            return (IQueryable<TModel>)query;
        }

        public static string getRel(string QueryString, string initStart, string initEnd, string ReplaceStart, string ReplaceEnd, string rel)
        {
            string page = QueryString.Replace(initStart + "-", ReplaceStart + "-");
            page = page.Replace("-" + initEnd, "-" + ReplaceEnd);
            page += $"; rel=\"{rel}\"";
            return page;
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

        public static IQueryable<TModel> Filter<TModel>(this IQueryable<TModel> query, Params param, IQueryCollection requestQuery)
        {
            var result = query;
            var opts = new Dictionary<string, StringValues>();
            var parameter = Expression.Parameter(typeof(TModel), "x");

            foreach (KeyValuePair<string, StringValues> search in requestQuery)
            {
                if (typeof(Params).GetProperty(search.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null)
                {
                    opts.Add(search.Key, search.Value.ToString().Split(','));
                }
            }

            var likeExpressions = opts.Select(d => d.Value.Select(f => LikeExpression<TModel>(parameter, d.Key, f)));
            Expression expOr;

            foreach (var likeExpression in likeExpressions)
            {
                var exp = likeExpression.ToArray();
                if (exp.Length > 1)
                {
                    expOr = Expression.Or(exp[0], exp[1]);
                    expOr = exp.Skip(2).Aggregate(expOr, (current, expression) => Expression.Or(current, expression));
                }
                else
                {
                    expOr = exp[0];
                }

                var lambda = Expression.Lambda<Func<TModel, bool>>(expOr, parameter);
                result = result.Where(lambda);
            }

            return result;
        }

        public static IQueryable<TModel> QuerySearch<TModel>(this IQueryable<TModel> query, Params param, IQueryCollection searchFields)
        {
            var opts = new Dictionary<string, StringValues>();

            foreach (KeyValuePair<string, StringValues> search in searchFields)
            {
                if (typeof(Params).GetProperty(search.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null)
                {
                    opts.Add(search.Key, search.Value);
                } 
            }

            var result = query;
            var parameter = Expression.Parameter(typeof(TModel), "x");
            var test = opts.Select(f => LikeExpression<TModel>(parameter, f.Key, f.Value));

            foreach (var exp in test)
            {
                var lambda = Expression.Lambda<Func<TModel, bool>>(exp, parameter);
                result = result.Where(lambda);
            }

            return result;
            
        }

        public static Expression LikeExpression<TModel>(ParameterExpression param, string property, string value)
        {
            //var param = Expression.Parameter(typeof(TModel), "x");
            var propertyInfo = typeof(TModel).GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var member = Expression.Property(param, propertyInfo);
            
            //var convert = Expression.Convert(member, propertyInfo.PropertyType);

            var tt = propertyInfo.PropertyType;
            var tttt = propertyInfo.PropertyType.Name;


            var startWith = value.StartsWith("*");
            var endsWith = value.EndsWith("*");
            var searchValue = value.Replace("*", "");

            Expression constant;
            Expression convert;

            if (propertyInfo.PropertyType == typeof(int))
            {
                int num = int.Parse(searchValue);
                constant = Expression.Constant(num);
                convert = Expression.Convert(member, typeof(int));
            }
            else if (propertyInfo.PropertyType == typeof(DateTime?))
            {
                DateTime datetime = DateTime.Parse(searchValue);
                constant = Expression.Constant(searchValue);
                convert = Expression.Convert(member, typeof(string));
            }
            else
            {
                constant = Expression.Constant(searchValue);
                convert = Expression.Convert(member, typeof(string));
            }

            //var constant = Expression.Constant(searchValue);

            Expression exp;

            if (endsWith && startWith)
            {
                exp = Expression.Call(convert, "Contains", null, constant);
            }
            else if (startWith)
            {
                exp = Expression.Call(convert, "EndsWith", null, constant);
            }
            else if (endsWith)
            {
                exp = Expression.Call(convert, "StartsWith", null, constant);

            }
            /*else if (propertyInfo.PropertyType == typeof(DateTime?))
            {
                exp = Expression.Call(convert, "CompareTo", null, constant);
            }*/
            else
            {
                exp = Expression.Equal(convert, constant);
            }

            //return Expression.Lambda<Func<TModel, bool>>(exp, param);
            return exp;
        }
    }
}