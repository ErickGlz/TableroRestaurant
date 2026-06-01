using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TableroRestaurant.Models;

namespace TableroRestaurant.Services
{
    public class PedidosService
    {
        public List<PedidoDTO> Pedidos { get; set; } = new();

        public event Action<PedidoDTO>? PedidoCreado;
        public event Action<PedidoDTO>? PedidoModificado;
        public event Action<PedidoDTO>? PedidoEliminado;

        private int ultimoId = 1;

        public void AgregarPedido(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return;

            lock (Pedidos)
            {
                if (Pedidos.Any(p => p.Numero == numero))
                    return;

                PedidoDTO pedido = new()
                {
                    Id = ultimoId++,
                    Numero = numero,
                    Estado = "PREPARANDO"
                };

                Pedidos.Add(pedido);
                PedidoCreado?.Invoke(pedido);
            }
        }

        public void MarcarListo(int id)
        {
            lock (Pedidos)
            {
                var pedido = Pedidos.FirstOrDefault(p => p.Id == id);
                if (pedido != null)
                {
                    pedido.Estado = "LISTO";
                    PedidoModificado?.Invoke(pedido);
                }
            }
        }

        public void EntregarPedido(int id)
        {
            lock (Pedidos)
            {
                var pedido = Pedidos.FirstOrDefault(p => p.Id == id);

                if (pedido == null)
                    return;

                if (pedido.Estado != "LISTO")
                {
                    return;
                }

                pedido.Estado = "ENTREGADO";

                Pedidos.Remove(pedido);

                PedidoEliminado?.Invoke(pedido);
            }
        }
    }
}
