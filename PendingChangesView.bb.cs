// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesView.bb.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Additional implementation required by the BB version of Git Source Control.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;

    /// <summary>
    /// BB extensions to the PendingChangesView
    /// </summary>
    public partial class PendingChangesView
    {
        /// <summary>
        /// Static reference to the current instance of the PendingChangesView.
        /// </summary>
        private static PendingChangesView currentInstance;

        /// <summary>
        /// Writes a message to the diff editor
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void WriteToDiffWindow(string message)
        {
            if (currentInstance != null)
            {
                var action = new Action(() => currentInstance.DiffEditor.AppendText(Environment.NewLine + message));
                currentInstance.DiffEditor.Dispatcher.BeginInvoke(action);
            }
        }

        /// <summary>
        /// Clears the diff editor.
        /// </summary>
        public static void ClearDiffEditor()
        {
            if (currentInstance != null)
            {
                var action = new Action(() => currentInstance.DiffEditor.Clear());
                currentInstance.DiffEditor.Dispatcher.BeginInvoke(action);
            }
        }
    }
}
