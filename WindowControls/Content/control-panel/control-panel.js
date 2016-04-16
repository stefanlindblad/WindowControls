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
            var checked = "false";
            if(document.getElementById("boldCheckBox").checked)
                checked = "true";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: checked,
                    action: "boldToogle",
                    name: "control-panel"
                }));
            }
        })

        var italicBox = document.getElementById("italicCheckBox");
        italicBox.addEventListener('change', function () {
            var checked = "false";
            if (document.getElementById("italicCheckBox").checked)
                checked = "true";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: checked,
                    action: "italicToogle",
                    name: "control-panel"
                }));
            }
        })

        var underlineBox = document.getElementById("underlineCheckBox");
        underlineBox.addEventListener('change', function () {
            var checked = "false";
            if (document.getElementById("underlineCheckBox").checked)
                checked = "true";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: checked,
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

        var fontSelector = document.getElementById("fontSelect");
        fontSelector.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("fontSelect").options[document.getElementById("fontSelect").selectedIndex].value,
                    action: "changeFont",
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
            else if (messageType === "changeVariable") {
                var action = message.action;
                var variable = message.variable;

                if (action === "headlineText") {
                    if (variable != "")
                    document.getElementById("textInput").value = variable;
                }
                else if (action === "boldToogle") {
                    if (variable === "true")
                        document.getElementById("boldCheckBox").checked = true;
                    else
                        document.getElementById("boldCheckBox").checked = false;
                }
                else if (action === "italicToogle") {
                    if (variable === "true")
                        document.getElementById("italicCheckBox").checked = true;
                    else
                        document.getElementById("italicCheckBox").checked = false;
                }
                else if (action === "underlineToogle") {
                    if (variable === "true")
                        document.getElementById("underlineCheckBox").checked = true;
                    else
                        document.getElementById("underlineCheckBox").checked = false;
                }
                else if (action === "changeSpacing") {
                    var variable = variable.slice(0, -2);
                    document.getElementById("spaceRange").value = variable;
                    document.getElementById("spaceValue").value = variable;
                }
                else if (action === "changeSize") {
                    var variable = variable.slice(0, -2);
                    document.getElementById("sizeRange").value = variable;
                    document.getElementById("sizeValue").value = variable;
                }
                else if (action === "changeFont") {
                    document.getElementById("fontSelect").value = variable;
                }
                else {
                    throw new Utils.RequestError("Unsupported action type: " + action);
                }
                




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
            else if (messageType === "serverAck") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "requestFontList",
                        name: "control-panel"
                    }));
                    webSocket.send(JSON.stringify({
                        type: "requestVariables",
                        name: "control-panel"
                    }));
                }
            }
            else if (messageType === "fontEntry") {
                var font = message.font;
                var d = new Detector();
                if (d.detect(font)) {
                    var selector = document.getElementById("fontSelect");
                    if (!(selector.hasAttribute("initialized"))) {
                        selector.setAttribute("initialized", "true");
                        selector.options.length = 0;
                    }
                    var id = selector.options.length++;
                    selector.options[id] = new Option(font, font);
                }
            }
            else {
                throw new Utils.RequestError("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
