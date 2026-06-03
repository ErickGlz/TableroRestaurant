using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using TableroRestaurant.Models;
using TableroRestaurant.Services;

namespace TableroRestaurant.ViewModels
{
    public class PedidosViewmodel
    {
        public ObservableCollection<PedidoDTO> Pedidos { get; set; } = new();

        private PedidosService service;
        private Dispatcher dispatcher;

        public string Hora { get; set; }

        private DispatcherTimer timer;

        public PedidosViewmodel()
        {
            dispatcher = Application.Current.Dispatcher;

            service = new PedidosService();

            service.PedidoCreado += Cambio;
            service.PedidoModificado += Cambio;
            service.PedidoEliminado += Cambio;

            Refresh();

            new HttpServerService(service).Iniciar();

            Hora = DateTime.Now.ToString("HH:mm:ss");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                Hora = DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();
        }

        private void Cambio(PedidoDTO obj)
        {
            dispatcher.BeginInvoke(() =>
            {
                Refresh();
            });
        }

        private void Refresh()
        {
            Pedidos.Clear();

            foreach (var p in service.Pedidos)
                Pedidos.Add(p);
        }
    }
}
