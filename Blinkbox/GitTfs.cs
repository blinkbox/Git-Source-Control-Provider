// -----------------------------------------------------------------------
// <copyright file="GitTfs.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Git Tfs class.
    /// </summary>
    public class GitTfs
    {
        /// <summary>
        /// Commands which appear in the git tfs menu
        /// </summary>
        private static readonly List<GitTfsCommand> menuOptions = new List<GitTfsCommand> { Commands.Checkin, Commands.GetLatest, Commands.CleanWorkspace };

        /// <summary>
        /// Gets the menu options.
        /// </summary>
        /// <value>The menu options.</value>
        public static List<GitTfsCommand> MenuOptions 
        { 
            get
            {
                return menuOptions;
            } 
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        public static void RunGitTfsCommand(string command, string workingDirectory)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo("cmd.exe", "/k git tfs " + command);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            // Write output to the pending changes window
            NotificationWriter.Clear();
            NotificationWriter.NewSection("Git-Tfs " + command);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (sender, args) => NotificationWriter.Write(args.Data);
            process.ErrorDataReceived += (sender, args) => NotificationWriter.Write(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        /// <summary>
        /// Defines git tfs command syntax
        /// </summary>
        public struct Commands
        {
            /// <summary>
            /// Checkin command syntax.
            /// </summary>
            public static readonly GitTfsCommand Checkin = new GitTfsCommand { Name = "Check in", CommandText = "checkintool", CommandId = Blinkbox.CommandIds.GitTfsCheckinButtonId };

            /// <summary>
            /// Get latest command syntax.
            /// </summary>
            public static readonly GitTfsCommand GetLatest = new GitTfsCommand { Name = "Get Latest", CommandText = "pull", CommandId = Blinkbox.CommandIds.GitTfsGetLatestButtonId };

            /// <summary>
            /// cleanup workspace command syntax.
            /// </summary>
            public static readonly GitTfsCommand CleanWorkspace = new GitTfsCommand { Name = "Clean Workspace", CommandText = "cleanup-workspaces", CommandId = Blinkbox.CommandIds.GitTfsCleanWorkspacesButtonId };
        }
      
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
            /// Gets or sets the command text.
            /// </summary>
            /// <value>The command text.</value>
            public string CommandText { get; set; }

            /// <summary>
            /// Gets or sets CommandId.
            /// </summary>
            public int CommandId { get; set; }
        }
    }
}
