(function () {
    "use strict";

    var defaultPort = 9696;

    function getWebSocketPort() {
        try {
            var tokens = window.location.search.substr(1).split("=");
            var port = parseInt(tokens[tokens.indexOf("port") + 1], 10);
            return port;
        }
        catch (e) {
            console.warn("Unable to parse port from URL. Using " + defaultPort);
            return defaultPort;
        }
    }

    function RequestError(value) {
        if (typeof(value) === "object" && value.type === "error") {
            this.name = value.name;
            this.message = value.message + "\n\n" + value.stack;
        }
        else {
            this.name = "RequestError";
            this.message = typeof(value) === "string" ? value : "Unknown Error";
        }

        this.stack = (new Error()).stack;
    }

    RequestError.prototype = Object.create(Error.prototype);
    RequestError.prototype.constructor = RequestError;

    window.Utils = {
        getWebSocketPort: getWebSocketPort,
        RequestError: RequestError
    };
})();
