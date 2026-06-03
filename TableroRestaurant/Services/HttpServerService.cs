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
        private HttpListener servidor = new();
        private bool activo;

        private PedidosService pedidoService;

        public event Action<string>? Mensaje;

        public HttpServerService(PedidosService pedidoService)
        {
            this.pedidoService = pedidoService;

            string url = "http://*:8080/restaurant/";
            servidor.Prefixes.Add(url);
        }

        public void Iniciar()
        {
            servidor.Start();
            activo = true;

            new Thread(EscucharPeticiones)
            {
                IsBackground = true
            }.Start();

            Mensaje?.Invoke("Servidor iniciado");
        }

        private void EscucharPeticiones()
        {
            while (activo)
            {
                var context = servidor.GetContext();

                var request = context.Request;
                var response = context.Response;

                try
                {
                    if (request.HttpMethod == "GET" && (request.RawUrl == "/restaurant/" || request.RawUrl == "/restaurant"))
                      {
                        ServirArchivo(response, "index.html", "text/html");
                    }

                    else if (request.HttpMethod == "GET" && request.RawUrl == "/restaurant/styles.css")
                    {
                        ServirArchivo(response, "styles.css", "text/css");
                    }

                    else if (request.HttpMethod == "GET" && request.RawUrl == "/restaurant/script.js")
                    {
                        ServirArchivo(response, "script.js", "application/javascript");
                    }

                    else if (request.HttpMethod == "GET" && request.RawUrl == "/restaurant/pedidos")
                    {
                        string json = JsonSerializer.Serialize(pedidoService.Pedidos);

                        byte[] buffer = Encoding.UTF8.GetBytes(json);

                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;

                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }

                    else if (request.HttpMethod == "POST" && request.RawUrl == "/restaurant/agregar")
                    {
                        byte[] buffer = new byte[request.ContentLength64];
                        request.InputStream.ReadExactly(buffer, 0, buffer.Length);

                        string json = Encoding.UTF8.GetString(buffer);

                        PedidoDTO? pedido = JsonSerializer.Deserialize<PedidoDTO>(json);

                        string mensaje;

                        try
                        {
                            if (pedido == null)
                            {
                                mensaje = "ERROR: datos inválidos";
                            }
                            else
                            {
                                pedidoService.AgregarPedido(pedido.Numero);
                                mensaje = "OK";
                            }
                        }
                        catch (Exception ex)
                        {
                            mensaje = ex.Message;
                        }

                        byte[] resp = Encoding.UTF8.GetBytes(mensaje);

                        response.ContentType = "text/plain";
                        response.ContentLength64 = resp.Length;
                        response.OutputStream.Write(resp, 0, resp.Length);
                    }

                    else if (request.HttpMethod == "POST" && request.RawUrl == "/restaurant/listo")
                    {
                        byte[] buffer = new byte[request.ContentLength64];
                        request.InputStream.ReadExactly(buffer, 0, buffer.Length);

                        string json = Encoding.UTF8.GetString(buffer);

                        PedidoDTO? pedido = JsonSerializer.Deserialize<PedidoDTO>(json);

                        string mensaje;

                        try
                        {
                            if (pedido == null)
                            {
                                mensaje = "ERROR: datos inválidos";
                            }
                            else
                            {
                                pedidoService.MarcarListo(pedido.Id);
                                mensaje = "OK";
                            }
                        }
                        catch (Exception ex)
                        {
                            mensaje = ex.Message;
                        }

                        byte[] resp = Encoding.UTF8.GetBytes(mensaje);

                        response.ContentType = "text/plain";
                        response.ContentLength64 = resp.Length;
                        response.OutputStream.Write(resp, 0, resp.Length);
                    }

                    else if (request.HttpMethod == "POST" && request.RawUrl == "/restaurant/entregar")
                    {
                        byte[] buffer = new byte[request.ContentLength64];
                        request.InputStream.ReadExactly(buffer, 0, buffer.Length);

                        string json = Encoding.UTF8.GetString(buffer);

                        PedidoDTO? pedido = JsonSerializer.Deserialize<PedidoDTO>(json);

                        string mensaje;

                        try
                        {
                            if (pedido == null)
                            {
                                mensaje = "ERROR: datos inválidos";
                            }
                            else
                            {
                                pedidoService.EntregarPedido(pedido.Id);
                                mensaje = "OK";
                            }
                        }
                        catch (Exception ex)
                        {
                            mensaje = ex.Message;
                        }

                        byte[] resp = Encoding.UTF8.GetBytes(mensaje);

                        response.ContentType = "text/plain";
                        response.ContentLength64 = resp.Length;
                        response.OutputStream.Write(resp, 0, resp.Length);
                    }

                    else
                    {
                        response.StatusCode = 404;
                    }
                }
                catch (Exception ex)
                {
                    Mensaje?.Invoke($"Error: {ex.Message}");
                    response.StatusCode = 500;
                }
                finally
                {
                    response.Close();
                }
            }
        }

        private void ServirArchivo(
            HttpListenerResponse response,
            string nombreArchivo,
            string contentType)
        {
            string ruta = Path.Combine("Assets", nombreArchivo);

            if (File.Exists(ruta))
            {
                byte[] buffer = File.ReadAllBytes(ruta);

                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;

                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 404;
            }

            response.Close();
        }

        public void Detener()
        {
            activo = false;
            servidor.Stop();

            Mensaje?.Invoke("Servidor detenido");
        }
    }
}