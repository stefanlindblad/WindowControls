# WindowControls

Project to evaluate the possibilites of dynamic window controls with .Net and JS

Development steps in the develop branch, final version to present in master.


# Major steps i have changed in the code:

Renamed namespace to WindowControls for distinction from other projects.

Moved hard coded values out of the program into Settings file for easier usability.

Established a thread safe dictionary that holds the client connections so the server can broadcast

Designed the control panel after the given conditions and registered event listeners for the elements

Implemented message parsings for changes in the state of the different control panel items.
