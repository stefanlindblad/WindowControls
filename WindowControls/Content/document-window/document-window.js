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
            else if (messageType === "serverInit") {
                webSocket.send(JSON.stringify({
                    type: "clientInit",
                    name: "document-window"
                }));
            }
            else {
                throw new Error("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
