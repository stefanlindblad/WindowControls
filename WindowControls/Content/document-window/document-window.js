(function () {
    "use strict";

    function onLoad() {
        var port = Utils.getWebSocketPort();
        var webSocket = new WebSocket("ws://127.0.0.1:" + port);
        var headline = document.getElementById("headline");
        var style = window.getComputedStyle(headline);
        var oldValue = "";

        webSocket.onmessage = function (event) {
            var message = JSON.parse(event.data);
            var messageType = message.type;

            if (messageType === "error") {
                throw new Utils.RequestError(message);
            }
            else if (messageType === "changeVariable") {
                var action = message.action;
                var variable = message.variable;
                var register = message.register;
                
                if (action === "headlineText") {
                    if (variable === "")
                        variable = "Enter Headline Text...";
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: "",
                            newValue: variable
                        }));
                    }
                    headline.innerHTML = variable;
                }
                else if (action === "boldToogle") {
                    oldValue = style.getPropertyValue('font-weight');
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: oldValue,
                            newValue: variable
                        }));
                    }
                    headline.style.fontWeight = variable;
                }
                else if (action === "italicToogle") {
                    oldValue = style.getPropertyValue('font-style');
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: oldValue,
                            newValue: variable
                        }));
                    }
                    headline.style.fontStyle = variable;
                }
                else if (action === "underlineToogle") {
                    oldValue = style.getPropertyValue('text-decoration');
                    if (oldValue != "underline")
                        oldValue = "none";
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: oldValue,
                            newValue: variable
                        }));
                    }
                    headline.style.textDecoration = variable;
                }
                else if (action === "changeSpacing") {
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: "",
                            newValue: variable
                        }));
                    }
                    if (variable === "")
                        variable = "0px"
                    headline.style.letterSpacing = variable;
                }
                else if (action === "changeSize") {
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: "",
                            newValue: variable
                        }));
                    }
                    if (variable === "")
                        variable = "4em"
                    headline.style.fontSize = variable;
                }
                else if (action === "changeFont") {
                    oldValue = style.getPropertyValue('font-family');
                    if (webSocket.readyState == 1 && register) {
                        webSocket.send(JSON.stringify({
                            type: "registerAction",
                            action: action,
                            name: "document-window",
                            oldValue: "",
                            newValue: variable
                        }));
                    }
                    var splittedVar = variable.split("__");
                    var fontName = splittedVar[0];
                    headline.style.fontFamily = fontName;
                }
                else {
                    throw new Utils.RequestError("Unsupported action type: " + action);
                }
            }
            else if (messageType === "serverInit") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "clientInit",
                        name: "document-window"
                    }));
                }
            }
            else if (messageType === "serverAck") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "requestStack",
                        name: "document-window"
                    }));
                }
            }
            else if (messageType === "fontEntry") {
                throw new Utils.RequestError("Wrong Recipient: " + messageType);
            }
            else {
                throw new Utils.RequestError("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
