using System;
using System.Collections.Generic;
using System.Text;

namespace TableroRestaurant.Models
{
    public class PedidoDTO
    {
        public int Id { get; set; }

        public string Numero { get; set; } = null!;

        public string Estado { get; set; } = null!;
    }
}
