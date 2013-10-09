// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SccProviderService.bb.cs" company="blinkbox">
//   Blinkbox implementation for the SccProviderService
// </copyright>
// <summary>
//   Blinkbox implementation for the SccProviderService
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    using GitScc.Blinkbox.Data;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Blinkbox implementation for the SccProviderService
    /// </summary>
    public partial class SccProviderService
    {
        /// <summary>
        /// Caches the solution directory
        /// </summary>
        private string solutionDirectory;

        /// <summary>
        /// Occurs when the source control provider refreshes.
        /// </summary>
        public event EventHandler<RefreshArgs> OnRefresh;

        /// <summary>
        /// Occurs when the source control provider refreshes.
        /// </summary>
        public event EventHandler<EventArgs> OnSolutionOpen;

        /// <summary>
        /// Gets a value indicating whether a solution is oen].
        /// </summary>
        /// <value><c>true</c> if [solution open]; otherwise, <c>false</c>.</value>
        public bool SolutionOpen { get; private set; }

        /// <summary>
        /// Returns true if a merge, patch, bisect or rebase operation is in progress.
        /// This are treated as external operations and are not implemented by the plugin. 
        /// </summary>
        /// <returns>true if an external git operation is in progress. </returns>
        public bool OperationInProgress()
        {
            var tracker = this.GetSolutionTracker();
            return tracker != null && (tracker.IsInTheMiddleOfBisect || tracker.IsInTheMiddleOfMerge || tracker.IsInTheMiddleOfPatch || tracker.IsInTheMiddleOfRebase
                   || tracker.IsInTheMiddleOfRebaseI);
        }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <returns>the path to the solution.</returns>
        public string GetSolutionDirectory()
        {
            if (this.SolutionOpen)
            {
                if (this.solutionDirectory == null)
                {
                    var sol = (IVsSolution)this._sccProvider.GetService(typeof(SVsSolution));
                    string solutionDirectoryPath, solutionFile, solutionUserOptions;

                    if (sol.GetSolutionInfo(out solutionDirectoryPath, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
                    {
                        this.solutionDirectory = solutionDirectoryPath;
                    }
                }

                return this.solutionDirectory;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the solution is git TFS controlled.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the solution is git TFS controlled.
        /// </returns>
        public bool IsSolutionGitTfsControlled()
        {
            if (this.Active && this.SolutionOpen)
            {
                var repositoryDirectory = GitFileStatusTracker.GetRepositoryDirectory(this.GetSolutionDirectory());
                if (!string.IsNullOrEmpty(repositoryDirectory))
                {
                    var expectedGitTfsDirectory = repositoryDirectory + "\\.git\\tfs";
                    return Directory.Exists(expectedGitTfsDirectory);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="branchName">Name of the branch.</param>
        public void CompareFile(string fileName, string branchName)
        {
            GitFileStatus status = this.GetFileStatus(fileName);
            if (status == GitFileStatus.Modified || status == GitFileStatus.Staged)
            {
                string tempFile = Path.GetFileName(fileName);
                tempFile = Path.Combine(Path.GetTempPath(), tempFile);
                this.CurrentTracker.SaveFileFromRepository(fileName, tempFile, branchName);
                this._sccProvider.RunDiffCommand(tempFile, fileName);
            }
        }

        /// <summary>
        /// Removes the suffix applied to branches when a merge, bisect, patch or rebase operation is in progress.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>the clean branchnameas used by git.</returns>
        public string CleanBranchName(string branchName)
        {
            if (!string.IsNullOrEmpty(branchName) && this.OperationInProgress())
            {
                // the plugin appends an operation code to the branch name - remove it.
                var parts = branchName.Split(new[] { " | ", "|" }, StringSplitOptions.RemoveEmptyEntries);
                return parts[0];
            }

            return branchName;
        }

        #region IVsSolutionEvents interface functions

        /// <summary>
        /// blinkbox implementation of OnAfterOpenSolution
        /// </summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <param name="fNewSolution">The f new solution.</param>
        /// <returns>status as an int</returns>
        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            this.SolutionOpen = true;

            // automatic switch the scc provider
            if (!this.Active && !GitSccOptions.Current.DisableAutoLoad)
            {
                this.OpenTracker();
                if (this.trackers.Count > 0)
                {
                    var rscp = (IVsRegisterScciProvider)this._sccProvider.GetService(typeof(IVsRegisterScciProvider));
                    rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
                }
            }

            this.Refresh();
            if (this.OnSolutionOpen != null)
            {
                this.OnSolutionOpen(this, new EventArgs());
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// blinkbox implementation of OnAfterCloseSolution
        /// </summary>
        /// <param name="pUnkReserved">The p unk reserved.</param>
        /// <returns>status as an int</returns>
        public int OnAfterCloseSolution([In] Object pUnkReserved)
        {
            this.solutionDirectory = null;
            this.SolutionOpen = false;
            this.CloseTracker();
            return VSConstants.S_OK;
        }

        #endregion
    }
}