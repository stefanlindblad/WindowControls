namespace Stingray.WindowControls.Undo
{
    /// <summary>
    /// The State Class assembles one action with the related new and old variable value
    /// </summary>
    internal class State
    {
        /// <summary>
        /// Creates a State Object
        /// </summary>
        /// <param name="action">The performed action</param>
        /// <param name="oldValue">The value before the action</param>
        /// <param name="newValue">The value after the action</param>
        public State(string action, string oldValue, string newValue)
        {
            this.action = action;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public string action { get; set; }
        public string oldValue { get; set; }
        public string newValue { get; set; }
    }
}
