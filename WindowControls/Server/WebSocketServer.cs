using System;
using System.Collections.Concurrent;
using Fleck;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stingray.WindowControls.Server
{
    /// <summary>
    /// Example web socket server.
    /// 
    /// Sends a "helloFromServer" message when a client connects. Prints to stdout if it receives a "helloFromView" message.
    /// Demonstrates serialization to and from JSON, plus sending and receiving messages.
    /// </summary>
    internal class WebSocketServer : IDisposable
    {
        [CanBeNull] private IWebSocketServer _server;
        private static ConcurrentDictionary<String, IWebSocketConnection> _connections = new ConcurrentDictionary<string, IWebSocketConnection>(); 

        /// <summary>
        /// Starts the example web socket server listening for connections to the specified port. Polling happens on a background thread.
        /// </summary>
        /// <param name="port">Port that clients can connect to.</param>
        public WebSocketServer(int port)
        {
            _server = new Fleck.WebSocketServer("ws://127.0.0.1:" + port);
            _server.Start( ConfigureConnection );
        }

        /// <summary>
        /// Stops polling for client connections and shuts down the server.
        /// </summary>
        public void Dispose()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
        }

        private static void ConfigureConnection(IWebSocketConnection connection)
        {
            connection.OnOpen = () => ConnectionOnOpen(connection);
            connection.OnMessage = message => ConnectionOnMessage(connection, message);
        }

        private static void ConnectionOnOpen(IWebSocketConnection connection)
        {
            connection.Send(JsonConvert.SerializeObject(new
            {
                type = "serverInit"
            }));
        }

        private static void ConnectionOnMessage(IWebSocketConnection connection, string message)
        {
            try
            {
                var jsonMessage = JObject.Parse(message);
                var messageType = (string)jsonMessage["type"];
                var name = (string)jsonMessage["name"];

                switch (messageType)
                {
                    case "clientInit":
                        _connections.AddOrUpdate(name, connection,
                            (key, oldValue) =>
                            {
                                return oldValue;
                            });
                        Console.WriteLine(name + " registered!", "WebSocketServer");
                        break;

                    case "changeVariable":
                        var variable = (string)jsonMessage["variable"];
                        var action = (string)jsonMessage["action"];
                        WebSocketServer.BroadcastMessage(message, name);
                        //Console.WriteLine(action + ": " + variable, "WebSocketServer");
                        break;

                    default:
                        throw new NotImplementedException("Unsupported message type: " + messageType);
                }
            }
            catch (Exception exception)
            {
                if(connection.IsAvailable)
                    connection.Send(CreateErrorMessage(exception));
            }
        }

        private static void BroadcastMessage(string message, string sender)
        {
            foreach (var con in _connections)
            {
                if (con.Key != sender && con.Value.IsAvailable)
                    con.Value.Send(message);
            }
        }

        [NotNull, Pure]
        private static string CreateErrorMessage(Exception exception)
        {
            return JsonConvert.SerializeObject(new
            {
                type = "error",
                name = exception.GetType().Name,
                message = exception.Message,
                stack = exception.StackTrace
            });
        }
    }
}
