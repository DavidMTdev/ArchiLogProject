using Archi.Library.Models;
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
                string[] champsDesc = order == "ascToDesc" || order == "descToAsc" || order == "asc" ? param.Desc.Split(",") : new string[] { }; // new string[] { };
                string[] champsAsc = order == "descToAsc" || order == "ascToDesc" || order == "desc" ? param.Asc.Split(",") : new string[] { }; // param.Asc.Split(",");

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
      
        public static IQueryable<dynamic> SelectFields<TModel>(this IOrderedQueryable<TModel> query, Params param)
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
                string first = (SkipValue != 0) ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], "0", (RangeValue-1).ToString(), "first") + ", ": "";
                string next = (int.Parse(RangeSplit[1]) != MaxRange) ? URL + getRel(QueryString,RangeSplit[0],RangeSplit[1],(int.Parse(RangeSplit[1])+1).ToString(),(MaxRange < (int.Parse(RangeSplit[1]) + RangeValue)) ? MaxRange.ToString() : (int.Parse(RangeSplit[1]) + RangeValue).ToString(),"next") + ", ": "";
                string last = (int.Parse(RangeSplit[1]) != MaxRange) ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], (MaxRange-RangeValue+1).ToString(), MaxRange.ToString(), "last") : "" ;
                string prev = (SkipValue != 0) ? URL + getRel(QueryString, RangeSplit[0], RangeSplit[1], (SkipValue - RangeValue) != 0 ? (SkipValue - RangeValue - 1).ToString() : "0", (SkipValue - 1).ToString(), "prev") + (next != "" ? ", " : "") : "";
                response.Headers.Add("Content-Range", $"{Range}/{MaxRange}");
                response.Headers.Add("Accept-Range", $"Product 50");
                response.Headers.Add("Link", $"{first}{prev}{next}{last}");
                return query.Skip(SkipValue).Take(RangeValue);
            }
            return (IQueryable<TModel>)query;
        }
        
        public static string getRel(string QueryString,string initStart, string initEnd, string ReplaceStart, string ReplaceEnd, string rel)
        {
            string page = QueryString.Replace( initStart + "-", ReplaceStart+"-");
            page = page.Replace("-" + initEnd, "-" + ReplaceEnd);
            page += $"; rel=\"{rel}\"";
            return page;

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