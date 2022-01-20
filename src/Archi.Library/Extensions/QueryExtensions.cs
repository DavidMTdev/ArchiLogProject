using Archi.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archi.Library.Extensions
{
    public static class QueryExtensions
    {
        public static void Sort<TModel>(this IQueryable<TModel> query, Params param)
        {
            if (param.HasOrder())
            {
                string champ = param.Asc;
                query = query.OrderBy(x => x.GetType().GetProperty("firstname", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance));
            }
        }
    }
}