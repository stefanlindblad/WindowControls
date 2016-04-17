(function () {
    "use strict";

    function onLoad() {
        var port = Utils.getWebSocketPort();
        var webSocket = new WebSocket("ws://127.0.0.1:" + port);
        
        // Registering the control panel undo button event listener
        var undo = document.getElementById("undoButton");
        undo.disabled = true;
        undo.style.opacity = 0.5;
        undo.addEventListener('click', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "undoChange",
                    name: "control-panel",
                }));
            }
        })
        // Registering the control panel redo button event listener
        var redo = document.getElementById("redoButton");
        redo.disabled = true;
        redo.style.opacity = 0.5;
        redo.addEventListener('click', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "redoChange",
                    name: "control-panel",
                }));
            }
        })
        //save.addEventListener('click', function () { alert("Save not implemented yet."); })
        // Registering the event listener that reacts to tipping in the textfield but doesnt create an undo event
        var textfield = document.getElementById("textInput");
        textfield.addEventListener('keyup', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("textInput").value,
                    action: "headlineText",
                    name: "control-panel",
                    register: false
                }));
            }
        })
        // Registering the event listener that creates an undo event after the user finished changing the textfield
        textfield.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("textInput").value,
                    action: "headlineText",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to (un)checking the Bold Check Box
        var boldBox = document.getElementById("boldCheckBox");
        boldBox.addEventListener('change', function () {
            var bold = "normal";
            if(document.getElementById("boldCheckBox").checked)
                bold = "bold";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: bold,
                    action: "boldToogle",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to (un)checking the Italic Check Box
        var italicBox = document.getElementById("italicCheckBox");
        italicBox.addEventListener('change', function () {
            var italic = "normal";
            if (document.getElementById("italicCheckBox").checked)
                italic = "italic";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: italic,
                    action: "italicToogle",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to (un)checking the Underline Check Box
        var underlineBox = document.getElementById("underlineCheckBox");
        underlineBox.addEventListener('change', function () {
            var underline = "none";
            if (document.getElementById("underlineCheckBox").checked)
                underline = "underline";
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: underline,
                    action: "underlineToogle",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to use the spacing slider but doesnt create an undo event
        var spaceRangeBox = document.getElementById("spaceRange");
        spaceRangeBox.addEventListener('input', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("spaceRange").value + "px",
                    action: "changeSpacing",
                    name: "control-panel",
                    register: false
                }));
            }
        })
        // Registering the event listener that creates an undo event after finishing to change the spacing value
        spaceRangeBox.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("spaceRange").value + "px",
                    action: "changeSpacing",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to use the size slider but doesnt create an undo event
        var sizeRangeBox = document.getElementById("sizeRange");
        sizeRangeBox.addEventListener('input', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("sizeRange").value + "em",
                    action: "changeSize",
                    name: "control-panel",
                    register: false
                }));
            }
        })
        // Registering the event listener that creates an undo event after finishing to change the size value
        sizeRangeBox.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("sizeRange").value + "em",
                    action: "changeSize",
                    name: "control-panel",
                    register: true
                }));
            }
        })
        // Registering the event listener that reacts to changes in the selected font type
        var fontSelector = document.getElementById("fontSelect");
        fontSelector.addEventListener('change', function () {
            if (webSocket.readyState == 1) {
                webSocket.send(JSON.stringify({
                    type: "changeVariable",
                    variable: document.getElementById("fontSelect").options[document.getElementById("fontSelect").selectedIndex].value + "__" + document.getElementById("fontSelect").selectedIndex,
                    action: "changeFont",
                    name: "control-panel",
                    register: true
                }));
            }

        })
        // Reactions from the control panel to incoming event messages, after reload for example
        webSocket.onmessage = function (event) {
            var message = JSON.parse(event.data);
            var messageType = message.type;

            // Error Messages
            if (messageType === "error") {
                throw new Utils.RequestError(message);
            }
            // Message after connect from the server to establish handshake, gets direct response
            else if (messageType === "serverInit") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "clientInit",
                        name: "control-panel"
                    }));
                }
            }
            // Messages after init response, the server acknowledges the connections and font list and variable changes are requested
            else if (messageType === "serverAck") {
                if (webSocket.readyState == 1) {
                    webSocket.send(JSON.stringify({
                        type: "requestFontList",
                        name: "control-panel"
                    }));
                    webSocket.send(JSON.stringify({
                        type: "requestStack",
                        name: "control-panel"
                    }));
                }
            }
            // A message with a new entry for the font selector arrives
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
            // Messages from the server when there is a possible change in the undo/redo stack
            else if (messageType === "undoRedoStatus") {
                var undoButton = document.getElementById("undoButton");
                var redoButton = document.getElementById("redoButton");
                if (message.undo) {
                    undoButton.style.opacity = 1.0;
                    undoButton.disabled = false;
                }
                else {
                    undoButton.style.opacity = 0.5;
                    undoButton.disabled = true;
                }
                if (message.redo) {
                    redoButton.style.opacity = 1.0;
                    redoButton.disabled = false;
                }
                else {
                    redoButton.style.opacity = 0.5;
                    redoButton.disabled = true;
                }
            }
            // Changes to variables are incoming, for example after reloading the panel or undo/redo actions
            else if (messageType === "changeVariable") {
                var action = message.action;
                var variable = message.variable;

                if (action === "headlineText") {
                    document.getElementById("textInput").value = variable;
                }
                else if (action === "boldToogle") {
                    if (variable === "bold")
                        document.getElementById("boldCheckBox").checked = true;
                    else
                        document.getElementById("boldCheckBox").checked = false;
                }
                else if (action === "italicToogle") {
                    if (variable === "italic")
                        document.getElementById("italicCheckBox").checked = true;
                    else
                        document.getElementById("italicCheckBox").checked = false;
                }
                else if (action === "underlineToogle") {
                    if (variable === "underline")
                        document.getElementById("underlineCheckBox").checked = true;
                    else
                        document.getElementById("underlineCheckBox").checked = false;
                }
                else if (action === "changeSpacing") {
                    var variable = variable.slice(0, -2);
                    if (variable === "")
                        variable = "0"
                    document.getElementById("spaceRange").value = variable + "px";
                    document.getElementById("spaceValue").value = variable;
                }
                else if (action === "changeSize") {
                    var variable = variable.slice(0, -2);
                    if (variable === "")
                        variable = "4"
                    document.getElementById("sizeRange").value = variable + "em";
                    document.getElementById("sizeValue").value = variable;
                }
                else if (action === "changeFont") {

                    var splittedVar = variable.split("__");
                    var id = splittedVar[1];
                    if (id === "")
                        document.getElementById("fontSelect").selectedIndex = 0;
                    else
                        document.getElementById("fontSelect").selectedIndex = id
                }
                else {
                    throw new Utils.RequestError("Unsupported action type: " + action);
                }
                return;
            }
            else {
                throw new Utils.RequestError("Unsupported message type: " + messageType);
            }
        };
    }
    window.addEventListener("load", onLoad);
})();
