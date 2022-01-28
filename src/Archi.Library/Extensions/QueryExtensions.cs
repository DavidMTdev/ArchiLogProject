using Archi.library.Controllers;
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
        public static IOrderedQueryable<TModel> QuerySort<TModel>(this IQueryable<TModel> query, Params param, string order)
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

        public static IQueryable<dynamic> QueryFields<TModel>(this IQueryable<TModel> query, Params param)
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

        public static IQueryable<TModel> QueryPaging<TModel>(this IQueryable<TModel> query, Params param, String URL, String QueryString, HttpResponse response)
        {
            if (param.HasRange())
            {
                string Range = param.Range;

                string[] RangeSplit = Range.Split('-');
                if (int.Parse(RangeSplit[0]) > int.Parse(RangeSplit[1]) || int.Parse(RangeSplit[1]) - int.Parse(RangeSplit[0]) + 1 > 50)
                {
                    throw new ArgumentException();
                }

                int RangeValue = int.Parse(RangeSplit[1]) - int.Parse(RangeSplit[0]) + 1;
                int SkipValue = int.Parse(RangeSplit[0]);
                int MaxRange = query.Count();
                bool testprev = SkipValue != 0 ? true : false;
                bool testnext = (int.Parse(RangeSplit[1]) != MaxRange) ? true : false;

                // Rel
                string first = testprev
                    ? URL + getRel(QueryString, RangeSplit, "0", (RangeValue-1).ToString(), "first") + ", "
                    : "";
                string next = testnext
                    ? URL + getRel(QueryString,RangeSplit,(int.Parse(RangeSplit[1])+1).ToString(),(MaxRange < (int.Parse(RangeSplit[1]) + RangeValue)) 
                    ? MaxRange.ToString() 
                    : (int.Parse(RangeSplit[1]) + RangeValue).ToString(),"next") + ", "
                    : "";
                string last = testnext
                    ? URL + getRel(QueryString, RangeSplit, (MaxRange-RangeValue+1).ToString(), MaxRange.ToString(), "last") 
                    : "" ;
                string prev = testprev
                    ? URL + getRel(QueryString, RangeSplit, (SkipValue - RangeValue) >= 0 
                    ? (SkipValue - RangeValue).ToString() 
                    : "0", (SkipValue - 1).ToString(), "prev") + (next != "" ? ", " : "") 
                    : "";

                response.Headers.Add("Content-Range", $"{Range}/{MaxRange}");
                response.Headers.Add("Accept-Range", $"Product 50");
                response.Headers.Add("Link", $"{first}{prev}{next}{last}");

                return query.Skip(SkipValue).Take(RangeValue);
            }

            return query;
        }

        public static string getRel(string QueryString, string[] Tab, string ReplaceStart, string ReplaceEnd, string rel)
        {
            string page = QueryString.Replace(Tab[0] + "-", ReplaceStart + "-");
            page = page.Replace("-" + Tab[1], "-" + ReplaceEnd);
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

        public static IQueryable<TModel> QueryFilter<TModel>(this IQueryable<TModel> query, Params param, IQueryCollection requestQuery)
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

            foreach (var item in opts)
            {
                Expression exp;
                if (item.Value[0].Contains("["))
                {
                    exp = RangeExpression<TModel>(parameter, item.Key, item.Value);
                }
                else
                {
                    var likeExpressions = item.Value.Select(f => LikeExpression<TModel>(parameter, item.Key, f)).ToArray();

                    if (likeExpressions.Length > 1)
                    {
                        exp = Expression.Or(likeExpressions[0], likeExpressions[1]);
                        exp = likeExpressions.Skip(2).Aggregate(exp, (current, expression) => Expression.Or(current, expression));
                    }
                    else
                    {
                        exp = likeExpressions[0];
                    }
                }

                var lambda = Expression.Lambda<Func<TModel, bool>>(exp, parameter);
                result = result.Where(lambda);
            }

            return result;
        }

        public static IQueryable<TModel> QuerySearch<TModel>(this IQueryable<TModel> query, Params param, IQueryCollection searchFields)
        {
            var result = query;
            var opts = new Dictionary<string, StringValues>();
            var parameter = Expression.Parameter(typeof(TModel), "x");

            foreach (KeyValuePair<string, StringValues> search in searchFields)
            {
                if (typeof(Params).GetProperty(search.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) == null)
                {
                    opts.Add(search.Key, search.Value);
                }
            }

            var likeExpressions = opts.Select(f => LikeExpression<TModel>(parameter, f.Key, f.Value));
            foreach (var exp in likeExpressions)
            {
                var lambda = Expression.Lambda<Func<TModel, bool>>(exp, parameter);
                result = result.Where(lambda);
            }

            return result;
        }

        public static Expression RangeExpression<TModel>(ParameterExpression param, string property, string[] value)
        {
            var propertyInfo = typeof(TModel).GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var member = Expression.Property(param, propertyInfo);

            var lessValue = value[0].Replace("[", "");
            var greaterValue = value[1].Replace("]", "");

            Expression expGreaterThanOrEqual = null;
            Expression expLessThanOrEqual = null;
            Expression convert = Expression.Convert(member, propertyInfo.PropertyType);

            if (!string.IsNullOrWhiteSpace(lessValue))
            {
                Expression lessValueConstant = Expression.Constant(Convert.ChangeType(lessValue, propertyInfo.PropertyType));
                expGreaterThanOrEqual = Expression.GreaterThanOrEqual(convert, lessValueConstant);
            }

            if (!string.IsNullOrWhiteSpace(greaterValue))
            {
                Expression greaterValueConstant = Expression.Constant(Convert.ChangeType(greaterValue, propertyInfo.PropertyType));
                expLessThanOrEqual = Expression.LessThanOrEqual(convert, greaterValueConstant);
            }

            Expression exp;
            if (expLessThanOrEqual == null && expGreaterThanOrEqual != null)
            {
                exp = expGreaterThanOrEqual;
            }
            else if (expLessThanOrEqual != null && expGreaterThanOrEqual == null)
            {
                exp = expLessThanOrEqual;
            }
            else
            {
                exp = Expression.And(expLessThanOrEqual, expGreaterThanOrEqual);
            }

            return exp;
        }

        public static Expression LikeExpression<TModel>(ParameterExpression param, string property, string value)
        {
            var propertyInfo = typeof(TModel).GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var member = Expression.Property(param, propertyInfo);

            var startWith = value.StartsWith("*");
            var endsWith = value.EndsWith("*");

            var searchValue = value.Replace("*", "");

            Expression exp;
            Expression convert = Expression.Convert(member, propertyInfo.PropertyType);
            Expression constant = Expression.Constant(Convert.ChangeType(searchValue, propertyInfo.PropertyType));

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
            else
            {
                exp = Expression.Equal(convert, constant);
            }

            return exp;
        }

        public static dynamic DefineValues(string scheme, string hostValue, string pathValue, string value)
        {
            var Url = scheme + "://" + hostValue + pathValue;
            var QueryString = value;

            var Order = "none";
            if (QueryString.ToLower().Contains("asc") && QueryString.ToLower().Contains("desc"))
            {
                Order = (QueryString.ToLower().IndexOf("asc", 0) < QueryString.ToLower().IndexOf("desc", 0)) ? "ascToDesc" : "descToAsc";
            } else if (QueryString.ToLower().Contains("asc"))
            {
                Order = "asc";
            } else
            {
                Order = "desc";
            }

            return new { Url, QueryString, Order};
        }
    }
}