using System;
using System.Collections.Generic;

namespace Next_V4.Utils
{
    public class JQueryDataTablePagination
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<Order> order { get; set; }
        public string error { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid ClienteId { get; set; }
        public class Column
        {
            public string data { get; set; }
            public string name { get; set; }
            public bool searchable { get; set; }
            public bool orderable { get; set; }
            public Search search { get; set; }
        }

        public class Search
        {
            public string value { get; set; }
            public string regex { get; set; }
        }

        public class Order
        {
            public int column { get; set; }
            public string dir { get; set; }
        }

        public class RetornoDataTablePagination
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public dynamic[] data { get; set; }
            public string error { get; set; }            
        }
    }
}
