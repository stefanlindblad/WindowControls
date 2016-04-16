using System;
using System.Drawing;
using System.Drawing.Text;
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
        private static StateHolder ServerStateHolder = new StateHolder();
        private static ConcurrentDictionary<string, IWebSocketConnection> Connections = new ConcurrentDictionary<string, IWebSocketConnection>();

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
                        // Check if a connection with NAME already exists, add it if not or update the value recordingly
                        Connections.AddOrUpdate(name, connection,
                            (k, v) =>
                            {
                                v = connection;
                                return v;
                            });
                        connection.Send(JsonConvert.SerializeObject(new
                        {
                            type = "serverAck"
                        }));
                        break;

                    case "requestFontList":
                        // Iterate over all fonts installed in the system and send them to the requested client
                        using (InstalledFontCollection ifc = new InstalledFontCollection())
                        {
                            foreach (FontFamily ff in ifc.Families)
                            {
                                WebSocketServer.SendMessage(CreateFontMessage(ff.Name), name);
                            }
                        }
                        break;
                    case "undoChange":
                        State stu = ServerStateHolder.UndoAction();
                        if(stu != null)
                        {
                            WebSocketServer.BroadcastMessage(CreateVariableMessage(stu.action, stu.oldValue, false), "webserver");
                            Console.WriteLine("Undo: " + stu.action + ", Before: " + stu.newValue + ", Now: " + stu.oldValue, "undoChange");
                        }
                        break;
                    case "redoChange":

                        // TODO

                        break;
                    // Register a Action in the undo/redo stack for being able to undo the changes
                    case "registerAction":
                        var newValue = (string)jsonMessage["newValue"];
                        var oldValue = (string)jsonMessage["oldValue"];
                        var action = (string)jsonMessage["action"];
                        ServerStateHolder.DoAction(action, oldValue, newValue);
                        Console.WriteLine("Register: " + action + ", Before: " + oldValue + ", Now: " + newValue, "registerChange");
                        break;
                    // Special Action Register Event for changes that come without an old value -> less performant!
                    case "registerActionWithoutOldValue":
                        var nv = (string)jsonMessage["newValue"];
                        var a = (string)jsonMessage["action"];
                        var ov = ServerStateHolder.GetLastValue(a);
                        ServerStateHolder.DoAction(a, ov, nv);
                        Console.WriteLine("Register: " + a + ", Before: " + ov + ", Now: " + nv, "registerChange");
                        break;

                    case "changeVariable":
                        WebSocketServer.BroadcastMessage(message, name);
                        break;

                    case "requestVariables":
                        State[] actions = ServerStateHolder.GetDoneActions();
                        // Iterate backward through the actions, since we unwind the stack
                        for(int i = actions.Length-1; i >= 0; i--)
                        {
                            State s = actions[i];
                            WebSocketServer.SendMessage(CreateVariableMessage(s.action, s.newValue, false), name);
                        }
                        break;

                    case "debug":
                        var debug = (string)jsonMessage["debug"];
                        Console.WriteLine("Debug Console: " + debug, "Javascript");
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

        /** The SendMessage Method sends a msg to one client
        /*  message = the message to send
        /*  recipient = the recipient of the message
        */
        private static void SendMessage(string message, string recipient)
        {
            bool sent = false;
            foreach (var con in Connections)
            {
                if (con.Key == recipient && con.Value.IsAvailable)
                {
                    con.Value.Send(message);
                    sent = true;
                }
            }
            if (!sent)
                Console.WriteLine("Recipient not found!");
        }

        /** The BroadcastMessage Method broadcasts a msg to all clients
        /*  message = the message to send
        /*  sender = the sender of the message who is excluded from getting it.
        */
        private static void BroadcastMessage(string message, string sender)
        {
            foreach (var con in Connections)
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

        /** The CreateFontMessage returns a serialized Json Message to register a font entry in the JS Frontend
        /*  type = the type to distinquish it from other system messages
        /*  name = the sender of the message
        /*  font = the font to register
        */
        [NotNull, Pure]
        private static string CreateFontMessage(string fontName)
        {
            return JsonConvert.SerializeObject(new
            {
                type = "fontEntry",
                name = "webserver",
                font = fontName
            });
        }

        /** The CreateVariableMessage returns a serialized Json Message for changing variables
        /*  type = the type to distinquish it from other system messages
        /*  action = the action performed, which is used to identify the variable to change
        /*  oldValue = the old value of the variable before the change was performed
        /*  newValue = the new value of the variable
        /*  name = the sender of the message
        /*  register = if the change should be registered at the undo/redo stack, only for major changes, not typing e.g.
        */
        [NotNull, Pure]
        private static string CreateVariableMessage(string action, string value, bool registerForUndo)
        {
            return JsonConvert.SerializeObject(new
            {
                type = "changeVariable",
                action = action,
                variable = value,
                name = "webserver",
                register = registerForUndo
            });
        }
    }
}