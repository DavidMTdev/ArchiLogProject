using Archi.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
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
        }
    }
}