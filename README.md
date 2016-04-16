# WindowControls

Project to evaluate the possibilites of dynamic window controls with .Net and JS

Development steps in the develop branch, final version to present in master.


# Major steps i have changed in the code:

Renamed namespace to WindowControls for distinction from other projects.

Moved hard coded values out of the program into Settings file for easier usability.

Established a thread safe dictionary that holds the client connections so the server can broadcast

Designed the control panel after the given conditions and registered event listeners for the elements

Implemented message parsings for changes in the state of the different control panel items.

For having a valid font list available, i used a 2 way approach. When the server (C#) gets a request
for delivering a list of fonts from one of the window elements, he retrieves these from the system
and sends the fonts to the scripting part. (JS). Here every font is again tested against browser support.

For being persistent against page reloads the editing changes are currently saved in a dictionary
from the server in a central place and retrieved from the clients after reloading their pages

