// -----------------------------------------------------------------------
// <copyright file="GitTfsCommand.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System;

    /// <summary>
    /// Defines a git tfs command
    /// </summary>
    public class GitTfsCommand
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets CommandId.
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        /// Gets or sets the command handler.
        /// </summary>
        public Action Handler { get; set; }
    }
}
