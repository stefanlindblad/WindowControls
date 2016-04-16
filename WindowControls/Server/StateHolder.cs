using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stingray.WindowControls.Server
{
    class State
    {
        public State(string a, string ov, string nv)
        {
            this.action = a;
            this.oldValue = ov;
            this.newValue = nv;
        }

        public string action { get; set; }
        public string oldValue { get; set;  }
        public string newValue { get; set; }
    }

    class StateHolder
    {
        private Stack<State> _editStack;
        private Stack<State> _redoStack;

        public StateHolder()
        {
            _editStack = new Stack<State>();
            _redoStack = new Stack<State>();
        }

        public bool hasUndo()
        {
            return _editStack.Count != 0;
        }

        public bool hasRedo()
        {
            return _redoStack.Count != 0;
        }

        //public void UndoAction(WebSocketServer* server)
        //{
        //
        //} 

        public string GetLastValue(string searchedAction)
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

        public void DoAction(string action, string oldValue, string newValue)
        {
            _editStack.Push(new State(action, oldValue, newValue));
            _redoStack.Clear();
        }

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

        public void RedoAction(string action, string oldValue, string newValue)
        {
            _redoStack.Push(new State(action, oldValue, newValue));
        }
   
    }
}
