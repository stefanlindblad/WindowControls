using System;
using System.Diagnostics;
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
    internal class ExampleWebSocketServer : IDisposable
    {
        [CanBeNull] private IWebSocketServer _server;

        /// <summary>
        /// Starts the example web socket server listening for connections to the specified port. Polling happens on a background thread.
        /// </summary>
        /// <param name="port">Port that clients can connect to.</param>
        public ExampleWebSocketServer(int port)
        {
            _server = new WebSocketServer("ws://127.0.0.1:" + port);
            _server.Start(ConfigureConnection);
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
                type = "helloFromServer"
            }));
        }

        private static void ConnectionOnMessage(IWebSocketConnection connection, string message)
        {
            try
            {
                var jsonMessage = JObject.Parse(message);
                var messageType = (string)jsonMessage["type"];

                switch (messageType)
                {
                    case "helloFromView":
                        Debug.WriteLine(jsonMessage["name"] + " says hello!", "ExampleWebSocketServer");
                        break;

                    default:
                        throw new NotImplementedException("Unsupported message type: " + messageType);
                }
            }
            catch (Exception exception)
            {
                connection.Send(CreateErrorMessage(exception));
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
