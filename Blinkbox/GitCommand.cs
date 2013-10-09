// -----------------------------------------------------------------------
// <copyright file="GitCommand.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    /// <summary>
    /// Executes a command in git.
    /// </summary>
    public class GitCommand : SccCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommand"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        public GitCommand(string command)
            : base(GitBash.GitExePath, command)
        {
            // Environment variable required for various git commands such as diff.
            StartInfo.EnvironmentVariables.Add("TERM", "msys");
        }


        /// <summary>
        /// Gets GitBash output - which is often sent to Error.
        /// </summary>
        public new string Error
        {
            get
            {
                // Need to distinguish real errors from the output which often goes to error. 
                return base.Error.StartsWith("fatal:") ? base.Error : null;
            }
        }

        /// <summary>
        /// Gets GitBash output - which is often sent to Error.
        /// </summary>
        public new string Output
        {
            get
            {
                return base.Output + (base.Error.StartsWith("fatal:") ? string.Empty : base.Error);
            }
        }
    }
}
