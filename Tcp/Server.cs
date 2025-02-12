using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace Tcp
{
    public class Server
    {
        private ServerConfiguration _serverConfiguration;
        private Socket _listenerSocket;
        private ILogger<Server> _logger;
        private List<EndPointHandler<object>> _endPointHandlers = new List<EndPointHandler<object>>();

        public Server(ServerConfiguration serverConfiguration, ILogger<Server> logger)
        {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void RegisterEndPoint(string endPoint, Func<string, Task<object>> handler)
        {
            _endPointHandlers.Add(new EndPointHandler<object>(endPoint, handler));
        }
        public async Task StartListening()
        {
            try
            {
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);
                _listenerSocket.Bind(ipEnd);
                _listenerSocket.Listen(5);

                _logger.LogInformation("Server is listening on {0}:{1}", _serverConfiguration.IpAddress, _serverConfiguration.Port);

                while (true)
                {
                    Socket clientSocket = await _listenerSocket.AcceptAsync();
                    var clientEndPoint = clientSocket.RemoteEndPoint?.ToString(); // Get the client IP and port
                    _logger.LogInformation($"Client connected. IP: {clientEndPoint}");


                    // Handle client communication in a separate method or thread
                    _ = Task.Run(() => HandleClient(clientSocket));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[20400];
            var response = new TcpResponse();
            try
            {
                _logger.LogInformation($"Processing data from {clientSocket.RemoteEndPoint}");
                int bytesRead = clientSocket.Receive(buffer);
                string receivedData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                //find which endpoint the client is trying to reach by deserializing the received data into a TcpRequest object
                TcpRequest request = JsonSerializer.Deserialize<TcpRequest>(receivedData);
                if (request == null)
                {
                    _logger.LogError("Invalid request.Error during deserialization of receivedData obj");
                    response.Error = "Invalid request.";
                }
                else
                {
                    EndPointHandler<object> handler = _endPointHandlers?.FirstOrDefault(x => x.EndPoint == request.EndPoint);
                    if (handler != null)
                    {
                        _logger.LogInformation($"Handler found for endpoint {request.EndPoint}");
                        var result = await handler.Handler(request.Json);
                        response.Data = JsonSerializer.Serialize(result);
                        _logger.LogInformation($"endpoint {request.EndPoint} was processed.Returning {response?.Data}");
                        response.IsSuccess = true;
                    }
                    else
                        _logger.LogError($"No handler found for endpoint {request.EndPoint}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                response.IsSuccess = false;
                response.Error = ex.Message;
            }
            finally
            {
                clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)));
                clientSocket.Close();
                _logger.LogInformation("Client disconnected.");
            }
        }
    }
}
