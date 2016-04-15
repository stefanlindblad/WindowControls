(function () {
    "use strict";

    function onLoad() {
        var port = Utils.getWebSocketPort();
        var webSocket = new WebSocket("ws://127.0.0.1:" + port);

        webSocket.onmessage = function (event) {
            var message = JSON.parse(event.data);
            var messageType = message.type;

            if (messageType === "error") {
                throw new Utils.RequestError(message);
            }
            else if (messageType === "changeVariable") {
                var action = message.action;
                var variable = message.variable;
                
                if (action === "headlineText") {
                    if (variable === "")
                        variable = "Enter Headline Text...";
                    document.getElementById("headline").innerHTML = variable;
                }
                else if (action === "boldToogle") {
                    if(variable === true)
                        document.getElementById("headline").style.fontWeight = "bold";
                    else
                        document.getElementById("headline").style.fontWeight = "normal";
                }
                else if (action === "italicToogle") {
                    if (variable === true)
                        document.getElementById("headline").style.fontStyle = "italic";
                    else
                        document.getElementById("headline").style.fontStyle = "normal";
                }
                else if (action === "underlineToogle") {
                    if (variable === true)
                        document.getElementById("headline").style.textDecoration = "underline";
                    else
                        document.getElementById("headline").style.textDecoration = "";
                }
                else if (action === "changeSpacing") {
                    document.getElementById("headline").style.letterSpacing = variable;
                }
                else if (action === "changeSize") {
                    document.getElementById("headline").style.fontSize = variable;
                }
                else {
                    throw new Utils.RequestError("Unsupported action type: " + action);
                }
            }
            else if (messageType === "serverInit") {
                webSocket.send(JSON.stringify({
                    type: "clientInit",
                    name: "document-window"
                }));
            }
            else {
                throw new Utils.RequestError("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
