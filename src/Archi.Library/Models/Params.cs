using System;
using System.Collections.Generic;
using System.Text;

namespace Archi.Library.Models
{
    public class Params
    {
        public string Asc { get; set; }
        public string Desc { get; set; }
        public string Range { get; set; }
        public string Fields { get; set; }

        public bool HasOrder()
        {
            return !string.IsNullOrWhiteSpace(Asc) || !string.IsNullOrWhiteSpace(Desc);
        }

        public bool HasRange()
        {
            return !string.IsNullOrWhiteSpace(Range);
        }

        public bool HasFields()
        {
            return !string.IsNullOrWhiteSpace(Fields);
        }
    }
}