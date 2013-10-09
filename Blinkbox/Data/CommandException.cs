// -----------------------------------------------------------------------
// <copyright file="CommandException.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    using System;

    /// <summary>
    /// Exception when an error occurs in an sccc command.
    /// </summary>
    /// <typeparam name="T">a type of SccCommand</typeparam>
    public class CommandException<T> : Exception where T : SccCommand
    {
        /// <summary>
        /// Initializes a new instance of the CommandException class.
        /// </summary>
        /// <param name="command">The command.</param>
        public CommandException(T command)
            : base(GetMessage(command))
        {
            this.Command = command;
        }

        /// <summary>
        /// Initializes a new instance of the CommandException class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="e">The e.</param>
        public CommandException(T command, Exception e)
            : base(GetMessage(command), e)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        public T Command { get; private set; }

        /// <summary>
        /// Gets an appropriate message for the type of command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>a message for the exception.</returns>
        private static string GetMessage(T command)
        {
            return typeof(T) + " failed: " + Environment.NewLine + command.Error;
        }
    }
}
