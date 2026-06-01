using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using TableroRestaurant.Models;


namespace TableroRestaurant.Services
{
    public class HttpServerService
    {
        private HttpListener servidor;
        private bool activo;

        private PedidosService pedidoService;

        public event Action<string>? OnLog;

        public HttpServerService(PedidosService pedidoService)
        {
            this.pedidoService = pedidoService;

            servidor = new HttpListener();

            string url = "http://localhost:8080/restaurant/";

            servidor.Prefixes.Add(url);
        }

        public void Iniciar()
        {
            servidor.Start();

            activo = true;

            Thread hiloPrincipal = new(EscucharPeticiones)
            {
                IsBackground = true
            };

            hiloPrincipal.Start();

            OnLog?.Invoke("Servidor iniciado");
        }

        private void EscucharPeticiones()
        {
            while (activo)
            {
                try
                {
                    HttpListenerContext context =
                        servidor.GetContext();

                    Thread hiloPeticion = new(() =>
                        ProcesarPeticion(context))
                    {
                        IsBackground = true
                    };

                    hiloPeticion.Start();
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"Error: {ex.Message}");
                }
            }
        }

        private void ProcesarPeticion(
            HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod == "GET"
                    && request.RawUrl == "/restaurant/")
                {
                    ServirArchivo(
                        response,
                        "index.html",
                        "text/html");
                }

                else if (request.HttpMethod == "GET"
                    && request.RawUrl == "/restaurant/styles.css")
                {
                    ServirArchivo(
                        response,
                        "styles.css",
                        "text/css");
                }

                else if (request.HttpMethod == "GET"
                    && request.RawUrl == "/restaurant/script.js")
                {
                    ServirArchivo(
                        response,
                        "script.js",
                        "application/javascript");
                }

                else if (request.HttpMethod == "GET"
                    && request.RawUrl == "/restaurant/pedidos")
                {
                    string json = JsonSerializer.Serialize(
                        pedidoService.Pedidos);

                    byte[] buffer =
                        Encoding.UTF8.GetBytes(json);

                    response.ContentType =
                        "application/json";

                    response.ContentLength64 =
                        buffer.Length;

                    response.OutputStream.Write(
                        buffer,
                        0,
                        buffer.Length);
                }

                else if (request.HttpMethod == "POST"
                    && request.RawUrl == "/restaurant/agregar")
                {
                    byte[] buffer =
                        new byte[request.ContentLength64];

                    request.InputStream.ReadExactly(
                        buffer,
                        0,
                        buffer.Length);

                    string json =
                        Encoding.UTF8.GetString(buffer);

                    PedidoDTO? pedido =
                        JsonSerializer.Deserialize<PedidoDTO>(
                            json);

                    if (pedido != null)
                    {
                        pedidoService.AgregarPedido(
                            pedido.Numero);

                        response.StatusCode = 200;
                    }
                    else
                    {
                        response.StatusCode = 400;
                    }
                }

                else if (request.HttpMethod == "POST"
                    && request.RawUrl == "/restaurant/listo")
                {
                    byte[] buffer =
                        new byte[request.ContentLength64];

                    request.InputStream.ReadExactly(
                        buffer,
                        0,
                        buffer.Length);

                    string json =
                        Encoding.UTF8.GetString(buffer);

                    PedidoDTO? pedido =
                        JsonSerializer.Deserialize<PedidoDTO>(
                            json);

                    if (pedido != null)
                    {
                        pedidoService.MarcarListo(
                            pedido.Id);

                        response.StatusCode = 200;
                    }
                    else
                    {
                        response.StatusCode = 400;
                    }
                }

                else if (request.HttpMethod == "POST"
                    && request.RawUrl == "/restaurant/entregar")
                {
                    byte[] buffer =
                        new byte[request.ContentLength64];

                    request.InputStream.ReadExactly(
                        buffer,
                        0,
                        buffer.Length);

                    string json =
                        Encoding.UTF8.GetString(buffer);

                    PedidoDTO? pedido =
                        JsonSerializer.Deserialize<PedidoDTO>(
                            json);

                    if (pedido != null)
                    {
                        pedidoService.EntregarPedido(
                            pedido.Id);

                        response.StatusCode = 200;
                    }
                    else
                    {
                        response.StatusCode = 400;
                    }
                }

                else
                {
                    response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Error: {ex.Message}");

                response.StatusCode = 500;
            }
            finally
            {
                response.Close();
            }
        }

        private void ServirArchivo(
            HttpListenerResponse response,
            string nombreArchivo,
            string contentType)
        {
            string ruta =
                Path.Combine("Assets", nombreArchivo);

            if (File.Exists(ruta))
            {
                byte[] buffer =
                    File.ReadAllBytes(ruta);

                response.ContentLength64 =
                    buffer.Length;

                response.ContentType =
                    contentType;

                response.OutputStream.Write(
                    buffer,
                    0,
                    buffer.Length);

                response.StatusCode = 200;
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        public void Detener()
        {
            activo = false;

            servidor.Stop();

            OnLog?.Invoke("Servidor detenido");
        }
    }
}