using System.Collections.Generic;

namespace Stingray.WindowControls.Undo
{
    /// <summary>
    /// The StateHolder manages the Undo/Redo Stack of the Application
    /// He is layouted as a singleton since only one undo/redo stack is necessary
    /// </summary>
    internal class StateManager
    {
        private Stack<State> _editStack;
        private Stack<State> _redoStack;
        private static StateManager _manager;

        /// <summary>
        /// Creates a StateHolder Object and initializes the undo+redo Stacks
        /// </summary>
        private StateManager()
        {
            _editStack = new Stack<State>();
            _redoStack = new Stack<State>();
        }

        public static StateManager GetManager()
        {
            if (_manager != null)
                return _manager;
            else
                return _manager = new StateManager();
        }

        public bool hasUndo()
        {
            return _editStack.Count != 0;
        }

        public bool hasRedo()
        {
            return _redoStack.Count != 0;
        }

        /// <summary>
        /// GetLastActions returns the last change of a given action in the stack
        /// </summary>
        /// <param name="searchedAction">The Action to look for in the stack</param>
        public string GetLastAction(string searchedAction)
        {
            foreach(State s in _editStack)
            {
                if (s.action == searchedAction)
                {
                    return s.newValue;
                }
            }
            return "";
        }

        public State[] GetDoneActions()
        {
            return _editStack.ToArray();
        }

        /// <summary>
        /// DoAction adds a new action to the undo stack and creates the Stack Object for it
        /// </summary>
        /// <param name="action">The performed action</param>
        /// <param name="oldValue">The value before the action</param>
        /// <param name="newValue">The value after the action</param>
        public void DoAction(string action, string oldValue, string newValue)
        {
            _editStack.Push(new State(action, oldValue, newValue));
            _redoStack.Clear();
        }

        /// <summary>
        /// Removes the last action from the undo stack, adds it to the redo stack and returns it.
        /// </summary>
        public State UndoAction()
        {
            if(_editStack.Count > 0)
            {
                State s = _editStack.Pop();
                _redoStack.Push(s);
                return s;
            }
            return null;
        }

        /// <summary>
        /// Removes the last action from the redo stack, adds it to the undo stack and returns it.
        /// </summary>
        public State RedoAction()
        {
            if (_redoStack.Count > 0)
            {
                State s = _redoStack.Pop();
                _editStack.Push(s);
                return s;
            }
            return null;
        }
    }
}
