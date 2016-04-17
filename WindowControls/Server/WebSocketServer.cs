using System;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Concurrent;
using Fleck;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stingray.WindowControls.Undo;

namespace Stingray.WindowControls.Server
{
    /// <summary>
    /// Web socket server.
    /// 
    /// Establishs a three way handshake with clients who connect and handles message communication via JSON
    /// </summary>
    internal class WebSocketServer : IDisposable
    {
        [CanBeNull] private IWebSocketServer _server;
        private static StateManager ServerStateManager = StateManager.GetManager();
        private static ConcurrentDictionary<string, IWebSocketConnection> Connections = new ConcurrentDictionary<string, IWebSocketConnection>();

        /// <summary>
        /// Starts the web socket server listening for connections to the specified port. Polling happens on a background thread.
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

        /// <summary>
        /// ConnectionOnMessage Callback
        /// 
        /// Gets called everytime a client sends a message to the server.
        /// Handles all message communication.
        /// </summary>
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
                    // Request the list of fonts if a panel wants to use system fonts
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
                    // Request the server to undo the last action
                    case "undoChange":
                        WebSocketServer.Undo();
                        break;
                    // Request the server to redo the last action
                    case "redoChange":
                        WebSocketServer.Redo();
                        break;
                    // Register a Action in the undo/redo stack for being able to undo the changes
                    case "registerAction":
                        var newValue = (string)jsonMessage["newValue"];
                        var oldValue = (string)jsonMessage["oldValue"];
                        var action = (string)jsonMessage["action"];
                        if (oldValue == "")
                            oldValue = ServerStateManager.GetLastAction(action);
                        ServerStateManager.DoAction(action, oldValue, newValue);
                        WebSocketServer.BroadcastMessage(CreateUndoRedoMessage(), "webserver");
                        Console.WriteLine("Register: " + action + ", Before: " + oldValue + ", Now: " + newValue, "registerChange");
                        break;
                    // Send a variable change to all clients
                    case "changeVariable":
                        WebSocketServer.BroadcastMessage(message, name);
                        break;
                    // Request the whole stack of variable changes, after reload for example
                    case "requestStack":
                        WebSocketServer.BroadcastMessage(CreateUndoRedoMessage(), "webserver");
                        State[] actions = ServerStateManager.GetDoneActions();
                        // Iterate backward through the actions, since we unwind the stack
                        for(int i = actions.Length-1; i >= 0; i--)
                        {
                            State s = actions[i];
                            WebSocketServer.SendMessage(CreateVariableMessage(s.action, s.newValue, false), name);
                        }
                        break;
                    // DebugPrintMessage for debug purposes
                    case "debugPrint":
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

        /// <summary>
        /// Undos one action that was performed by the user
        /// Public to be accessible for the KeyboardHandler
        /// </summary>
        public static void Undo()
        {
            State stu = ServerStateManager.UndoAction();
            if (stu != null)
            {
                WebSocketServer.BroadcastMessage(CreateVariableMessage(stu.action, stu.oldValue, false), "webserver");
                WebSocketServer.BroadcastMessage(CreateUndoRedoMessage(), "webserver");
                Console.WriteLine("Undo: " + stu.action + ", Before: " + stu.newValue + ", Now: " + stu.oldValue, "undoChange");
            }
        }

        /// <summary>
        /// Redos one action that was undone
        /// Public to be accessible for the KeyboardHandler
        /// </summary>
        public static void Redo()
        {
            State str = ServerStateManager.RedoAction();
            if (str != null)
            {
                WebSocketServer.BroadcastMessage(CreateVariableMessage(str.action, str.newValue, false), "webserver");
                WebSocketServer.BroadcastMessage(CreateUndoRedoMessage(), "webserver");
                Console.WriteLine("Redo: " + str.action + ", Before: " + str.newValue + ", Now: " + str.oldValue, "redoChange");
            }
        }

        /// <summary>
        /// SendMessage Method to send messages to one specific client
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="recipient">the receiver of the message</param>
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

        /// <summary>
        /// BroadcastMessage Method to send messages to all connected clients
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the sender of the message, for msg forwarding</param>
        private static void BroadcastMessage(string message, string sender)
        {
            foreach (var con in Connections)
            {
                if (con.Key != sender && con.Value.IsAvailable)
                    con.Value.Send(message);
            }
        }

        /// <summary>
        /// CreateErrorMessage returns a serialized Json Message to send error information
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
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

        /// <summary>
        /// The CreateFontMessage returns a serialized Json Message to register a font entry in the JS Frontend
        /// </summary>
        /// <param name="font">the font to register</param>
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

        /// <summary>
        /// The CreateUndoRedoMessage creates a message with the status of the undo/redo stacks
        /// </summary>
        private static string CreateUndoRedoMessage()
        {
            bool u = ServerStateManager.hasUndo();
            bool r = ServerStateManager.hasRedo();
            return JsonConvert.SerializeObject(new
            {
                type = "undoRedoStatus",
                name = "webserver",
                undo = u,
                redo = r
            });
        }

        /// <summary>
        /// The CreateVariableMessage returns a serialized Json Message for changing variables
        /// </summary>
        /// <param name="action">the action that identifies the variable</param>
        /// <param name="value">the value of the variable</param>
        /// <param name="registerForUndo">if the change should be registered at the undo/redo stack</param>
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