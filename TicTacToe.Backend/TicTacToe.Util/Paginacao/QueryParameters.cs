using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Util.Paginacao
{
    public class QueryParameters
    {
        // Paginação
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize || value < 1) ? MaxPageSize : value;
        }

        // Filtros / Busca
        public string? SearchTerm { get; set; }  // nome ou email

        // Ordenação (opcional)
        public string? OrderBy { get; set; }
    }
}
