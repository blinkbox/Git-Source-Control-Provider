// -----------------------------------------------------------------------
// <copyright file="SccHelperService.cs" company="blinkbox">
// Wrapper for Source control functionality.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Wrapper for Source control functionality.
    /// </summary>
    public class SccHelperService
    {
        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SccHelperService"/> class.
        /// </summary>
        /// <param name="sccProvider">The SCC provider.</param>
        public SccHelperService(SccProviderService sccProvider)
        {
            this.sccProvider = sccProvider;
        }

        /// <summary>
        /// Gets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        private GitFileStatusTracker Tracker
        {
            get
            {
                return this.sccProvider.GetSolutionTracker() ?? this.sccProvider.CurrentTracker;
            }
        }

        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="wait">if set to <c>true</c> [wait].</param>
        /// <param name="silent">if set to <c>true</c> output is not sent to the notification window.</param>
        /// <returns>the output of the git command.</returns>
        public static GitCommand RunGitCommand(string command, bool wait = false, bool silent = false)
        {
            var process = new GitCommand(command) { Silent = silent, WaitUntilFinished = wait };
            process.Start();

            if (!string.IsNullOrEmpty(process.Error))
            {
                throw new CommandException<GitCommand>(process);
            }

            return process;
        }

        /// <summary>
        /// Diffs the two branches.
        /// </summary>
        /// <param name="from">From branch.</param>
        /// <param name="to">To branch.</param>
        /// <returns>A list of GitFiles</returns>
        public static IEnumerable<GitFile> DiffBranches(string from, string to)
        {
            var diffText = SccHelperService.RunGitCommand("diff --name-status " + from + ".." + to).Output;
            var diffList = diffText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var gitFiles = diffList.Select(GitFile.FromDiff);

            return gitFiles;
        }

        /// <summary>
        /// Determines how many commits the from branch is ahead and/or behind the toBranch.
        /// </summary>
        /// <param name="from">From branch.</param>
        /// <param name="to">To branch.</param>
        /// <returns>A tuple(ahead, behind)</returns>
        public static AheadBehind BranchAheadOrBehind(string from, string to)
        {
            var comparison = new AheadBehind();

            // Gets a list of commits which are in one branch but not the other on separate lines. 
            // if the commit is in the left branch but not the right the line start with "<", otherwise ">"
            var status = RunGitCommand("rev-list --left-right " + from + "..." + to, silent: true).Output;

            // Count the number of arrows. 
            var lines = status.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries); 
            foreach (var line in lines)
            {
                if (line.StartsWith(">"))
                {
                    // fromBranch is a commit behind the toBranch
                    comparison.Behind++;
                }
                else if (line.StartsWith("<"))
                {
                    // fromBranch has a commit ahead of the toBranch
                    comparison.Ahead++;
                }
            }

            return comparison;
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="wait">waits for the process to exit before continuing execution.</param>
        /// <param name="silent">if set to <c>true</c> [silent].</param>
        /// <returns>the output from the git tfs command</returns>
        public static string RunGitTfs(string command, bool wait = false, bool silent = false)
        {
            var gitTfsCommand = new SccCommand("cmd.exe", "/k git tfs " + command)
                {
                    WaitUntilFinished = wait,
                    Silent = silent
                };

            gitTfsCommand.Start();

            if (!string.IsNullOrEmpty(gitTfsCommand.Error))
            {
                throw new Exception(gitTfsCommand.Error);
            }

            return gitTfsCommand.Output;
        }

        /// <summary>
        /// Converts a relative path to an absolute path relative to the solution.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>an absolute path relative to the solution. </returns>
        public static string GetAbsolutePath(string path)
        {
            path = path.TrimStart("\\".ToCharArray());
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            path = Path.IsPathRooted(path)
                     ? path
                     : Path.Combine(sccProvider.GetSolutionDirectory(), path);

            return path;
        }

        /// <summary>
        /// Diffs the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="endRevision">To revision.</param>
        /// <param name="startRevision">From revision.</param>
        public void DiffFileInTortoise(string fileName, string endRevision = null, string startRevision = null)
        {
            if (System.IO.File.Exists(GitSccOptions.Current.TortoiseGitPath))
            {
                var command = string.Format("diff /path:\"{0}\"", fileName);

                if (!string.IsNullOrEmpty(endRevision))
                {
                    command += string.Format(" /startrev:{0} /endrev:{1}", string.IsNullOrEmpty(startRevision) ? BlinkboxSccOptions.WorkingDirectoryRevision : startRevision, endRevision);
                }

                this.RunTortoise(command);
            }
        }

        /// <summary>
        /// Diffs a file against the specified revision using git.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="revision">The revision.</param>
        /// <returns>a text diff.</returns>
        public string DiffFileWithGit(string fileName, string revision)
        {
            var diffCommand = SccHelperService.RunGitCommand(string.Format("diff {0} \"{1}\"", revision, fileName), silent: true);
            return diffCommand.Output;
        }

        /// <summary>
        /// Determines whether this branch is merging.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a merge is in progress
        /// </returns>
        public bool IsMerging()
        {
            return this.Tracker.IsInTheMiddleOfMerge;
        }

        /// <summary>
        /// Checks out a branch.
        /// </summary>
        /// <param name="branch">The branch.</param>
        /// <param name="createNew">if set to <c>true</c> creates a new branch.</param>
        public void CheckOutBranch(string branch, bool createNew = false)
        {
            this.Tracker.CheckOutBranch(branch, createNew);
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public string GetWorkingDirectory()
        {
            string directory = null;
            try
            {
                directory = this.Tracker == null ? null : this.Tracker.GitWorkingDirectory;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            // Getting the directory failed (perhaps because the tracker was unavailable). Try a different approach.
            directory = !string.IsNullOrEmpty(directory)
                ? directory
                : GitFileStatusTracker.GetRepositoryDirectory(this.sccProvider.GetSolutionDirectory());

            return directory;
        }

        /// <summary>
        /// Gets the last commit message.
        /// </summary>
        /// <returns>The last commit message</returns>
        public string GetLastCommitMessage()
        {
            return this.Tracker.LastCommitMessage.Replace("\n", string.Empty);
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public bool WorkingDirectoryClean()
        {
            return !this.Tracker.ChangedFiles.Any(); 
        }

        /// <summary>
        /// Parses git status into a list of files.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A list of git files. </returns>
        public IList<GitFile> ParseGitStatus(string status)
        {
            return this.Tracker.ParseGitStatus(status);
        }

        /// <summary>
        /// Gets the current branch clean of any operation status.
        /// </summary>
        /// <returns>The branch</returns>
        public string GetCurrentBranch()
        {
            string branch = null;

            if (this.Tracker != null)
            {
                branch = this.sccProvider.CleanBranchName(this.Tracker.CurrentBranch);
            }

            if (string.IsNullOrEmpty(branch))
            {
                NotificationService.DisplayError(string.Empty, "unable to get current branch, please refresh and try again.");
            }

            return branch;
        }

        /// <summary>
        /// Gets the latest revision for the specified branch (defaults to current branch).
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>the hash of the latest revision.</returns>
        public string GetHeadRevisionHash(string branchName = null)
        {
            branchName = branchName ?? this.GetCurrentBranch();
            if (!string.IsNullOrEmpty(branchName))
            {
                var revision = RunGitCommand("rev-parse " + branchName, wait: true, silent: true).Output;
                return revision.Replace("\n", string.Empty); // Git adds a return to the revision
            }

            return null;
        }

        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void RunTortoise(string command)
        {
            var tortoiseGitPath = GitSccOptions.Current.TortoiseGitPath;
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = tortoiseGitPath;
                process.StartInfo.WorkingDirectory = this.GetWorkingDirectory();
                process.StartInfo.Arguments = "/command:" + command + " /path:\"" + process.StartInfo.WorkingDirectory + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                process.WaitForExit();
            }
        }
    }
}
