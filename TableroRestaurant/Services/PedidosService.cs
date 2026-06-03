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
                throw new ArgumentException("El número de pedido no puede estar vacío");

            lock (Pedidos)
            {
                if (Pedidos.Any(p => p.Numero == numero))
                    throw new InvalidOperationException("Ya existe un pedido con ese número");

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

                if (pedido == null)
                    throw new KeyNotFoundException("El pedido no existe");

                if (pedido.Estado == "ENTREGADO")
                    throw new InvalidOperationException("No se puede marcar como listo un pedido ya entregado");

                pedido.Estado = "LISTO";
                PedidoModificado?.Invoke(pedido);
            }
        }

        public void EntregarPedido(int id)
        {
            lock (Pedidos)
            {
                var pedido = Pedidos.FirstOrDefault(p => p.Id == id);

                if (pedido == null)
                    throw new KeyNotFoundException("El pedido no existe");

                if (pedido.Estado == "PREPARANDO")
                    throw new InvalidOperationException("No se puede entregar un pedido en preparación");

                if (pedido.Estado != "LISTO")
                    throw new InvalidOperationException("Solo se pueden entregar pedidos listos");

                pedido.Estado = "ENTREGADO";

                Pedidos.Remove(pedido);

                PedidoEliminado?.Invoke(pedido);
            }
        }
    }
}
