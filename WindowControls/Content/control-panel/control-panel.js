(function () {
    "use strict";

    function onLoad() {
        var port = Utils.getWebSocketPort();
        var webSocket = new WebSocket("ws://127.0.0.1:" + port);
        
        // Registering editing actions
        var undo = document.getElementById("undoButton");
        undo.addEventListener('click', function () { alert("Undo not implemented yet."); })

        var redo = document.getElementById("redoButton");
        redo.addEventListener('click', function () { alert("Redo not implemented yet."); })

        var load = document.getElementById("loadButton");
        load.addEventListener('click', function () { alert("Load not implemented yet."); })

        var save = document.getElementById("saveButton");
        save.addEventListener('click', function () { alert("Save not implemented yet."); })

        var textfield = document.getElementById("textInput");
        textfield.addEventListener('keyup', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("textInput").value,
                    action: "headlineText",
                    name: "control-panel"
                }));
            }
        })

        var boldBox = document.getElementById("boldCheckBox");
        boldBox.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("boldCheckBox").checked,
                    action: "boldToogle",
                    name: "control-panel"
                }));
            }
        })

        var italicBox = document.getElementById("italicCheckBox");
        italicBox.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("italicCheckBox").checked,
                    action: "italicToogle",
                    name: "control-panel"
                }));
            }
        })

        var underlineBox = document.getElementById("underlineCheckBox");
        underlineBox.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("underlineCheckBox").checked,
                    action: "underlineToogle",
                    name: "control-panel"
                }));
            }
        })

        var spaceRangeBox = document.getElementById("spaceRange");
        spaceRangeBox.addEventListener('input', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("spaceRange").value + "px",
                    action: "changeSpacing",
                    name: "control-panel"
                }));
            }
        })

        var sizeRangeBox = document.getElementById("sizeRange");
        sizeRangeBox.addEventListener('input', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("sizeRange").value + "em",
                    action: "changeSize",
                    name: "control-panel"
                }));
            }
        })

        webSocket.onmessage = function (event) {
            var message = JSON.parse(event.data);
            var messageType = message.type;

            if (messageType === "error") {
                throw new Utils.RequestError(message);
            }
            if (messageType === "changeVariable") {
                // At the moment the control panel does not receive any variable changes.
                return;
            }
            else if (messageType === "serverInit") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "clientInit",
                        name: "control-panel"
                    }));
                }
            }
            else {
                throw new Utils.RequestError("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
