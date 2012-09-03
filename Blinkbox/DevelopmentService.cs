﻿// -----------------------------------------------------------------------
// <copyright file="DevelopmentService.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using GitScc.Blinkbox.Options;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Implementation of common development processes.
    /// </summary>
    public class DevelopmentService : IDisposable
    {
        /// <summary>
        /// Instance of the  <see cref="notificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

        /// <summary>
        /// Instance of the  <see cref="SccHelperService"/>
        /// </summary>
        private readonly SccHelperService sccHelper;

        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProvider;

        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly CancellationTokenSource cancelRunAsync = new CancellationTokenSource();

        /// <summary>
        /// Keeps the time of the last tfs Update. 
        /// </summary>
        private DateTime lastTfsFetch = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentService"/> class.
        /// </summary>
        /// <param name="basicSccProvider">The basic SCC provider.</param>
        /// <param name="sccProvider">The SCC provider.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="sccHelper">The SCC helper.</param>
        public DevelopmentService(BasicSccProvider basicSccProvider, SccProviderService sccProvider, NotificationService notificationService, SccHelperService sccHelper)
        {
            this.sccProvider = sccProvider;
            this.notificationService = notificationService;
            this.sccHelper = sccHelper;
            //// this.sccProvider.OnRefresh += (s, e) => this.RefreshTfsStatus();
            basicSccProvider.OnRefreshButton += (s, e) => this.UpdateTfsStatus();
            basicSccProvider.OnCheckout += (s, e) => this.UpdateTfsStatus();
        }

        /// <summary>
        /// Describes the current use of the plugin.
        /// </summary>
        public enum DevMode
        {
            /// <summary>
            /// normal developing use
            /// </summary>
            Working = 0,

            /// <summary>
            /// Review in progress
            /// </summary>
            Reviewing = 1,

            /// <summary>
            /// Checking into TFS
            /// </summary>
            Checkin = 2
        }

        /// <summary>
        /// Gets or sets the current mode.
        /// </summary>
        /// <value>The current mode.</value>
        public DevMode CurrentMode { get; set; }

        /// <summary>
        /// Runs the provided action asyncronously.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>The task.</returns>
        public Task RunAsync(Action action, string operation)
        {
            var task = new TaskFactory().StartNew(action, this.cancelRunAsync.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        NotificationService.DisplayException(t.Exception, operation + " failed");
                    }
                });

            return task;
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void GetLatest()
        {
            const string OperationName = "Get Latest";

            Action action = () =>
                {
                    this.notificationService.ClearMessages();
                    this.notificationService.NewSection("Start " + OperationName);

                    if (!this.CheckWorkingDirectoryClean())
                    {
                        return;
                    }

                    // Pull down changes into tfs/default remote branch, and tfs_merge branch
                    SccHelperService.RunGitTfs("fetch");

                    // Merge without commit from tfs-merge to current branch. 
                    SccHelperService.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", wait: true);

                    this.CommitIfRequired();
                };

            this.RunAsync(action, OperationName);
        }

        /// <summary>
        /// Compare working directory with TFS
        /// </summary>
        public void Review()
        {
            const string OperationName = "Review";

            Action action = () =>
                {
                    this.notificationService.ClearMessages();
                    this.notificationService.NewSection("Start " + OperationName);

                    var currentBranch = this.sccHelper.GetCurrentBranch();

                    if (!this.CheckWorkingDirectoryClean() || !this.CheckLatestFromTfs(currentBranch))
                    {
                        return;
                    }

                    var diff = SccHelperService.DiffBranches(BlinkboxSccOptions.Current.TfsRemoteBranch, currentBranch);

                    if (diff.Any())
                    {
                        // Switch to reviewing mode
                        this.CurrentMode = DevMode.Reviewing;

                        var pendingChangesView = BasicSccProvider.GetServiceEx<PendingChangesView>();
                        if (pendingChangesView != null)
                        {
                            pendingChangesView.Review(diff.ToList(), BlinkboxSccOptions.Current.TfsRemoteBranch);
                        }

                        // force the commands to update
                        var shell = BasicSccProvider.GetServiceEx<IVsUIShell>();
                        if (shell != null)
                        {
                            shell.UpdateCommandUI(0);
                        }
                    }
                    else
                    {
                        notificationService.AddMessage("No changes found to review");
                    }
                };

            this.RunAsync(action, OperationName);
        }

        /// <summary>
        /// Cancels a review.
        /// </summary>
        public void CancelReview()
        {
            var pendingChanges = BasicSccProvider.GetServiceEx<PendingChangesView>();
            pendingChanges.EndReview();
            this.CurrentMode = DevMode.Working;

            // force the commands to update
            var shell = BasicSccProvider.GetServiceEx<IVsUIShell>();
            if (shell != null)
            {
                shell.UpdateCommandUI(0);
            }

            this.sccProvider.RefreshToolWindows();
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void Checkin()
        {
            const string OperationName = "Check in";

            Action action = () =>
                {
                    this.notificationService.ClearMessages();
                    this.notificationService.NewSection("Start " + OperationName);

                    var currentBranch = this.sccHelper.GetCurrentBranch();

                    if (!this.CheckWorkingDirectoryClean() || !this.CheckLatestFromTfs(currentBranch))
                    {
                        return;
                    }

                    // Checkin from tfs-merge branch
                    SccHelperService.RunGitTfs("checkintool");
                };

            this.RunAsync(action, OperationName).ContinueWith(t => this.CancelReview());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Stop and dispose all running tasks (should be none)
            this.cancelRunAsync.Cancel();
        }

        /// <summary>
        /// Updates the TFS status and displays in the pending changes window.
        /// </summary>
        public void UpdateTfsStatus()
        {
            Action update = () =>
                {
                    this.lastTfsFetch = DateTime.Now;
                    this.FetchFromTfs();
                    var aheadBehind = SccHelperService.BranchAheadOrBehind(this.sccHelper.GetCurrentBranch(), BlinkboxSccOptions.Current.TfsRemoteBranch);
                    var pendingChanges = BasicSccProvider.GetServiceEx<PendingChangesView>();
                    if (pendingChanges != null)
                    {
                        pendingChanges.UpdateTfsStatus(aheadBehind);
                    }
                };

            this.RunAsync(update, "UpdateTfsStatus");
        }

        /// <summary>
        /// Updates the TFS status and displays in the pending changes window.
        /// </summary>
        public void RefreshTfsStatus()
        {
            if (this.sccProvider.IsSolutionGitTfsControlled() && DateTime.Now.Subtract(this.lastTfsFetch).Minutes > 0)
            {
                this.UpdateTfsStatus();
            }
        }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <returns>true if successful</returns>
        private bool CheckWorkingDirectoryClean()
        {
            if (!this.sccHelper.WorkingDirectoryClean())
            {
                NotificationService.DisplayError("Cannot proceed", "There are uncommitted changes in your working directory");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the provided branch is ahead and/or behind tfs.
        /// </summary>
        /// <param name="currentBranch">The current branch.</param>
        /// <returns> true if the current branch has the latest revisions from tfs.</returns>
        private bool CheckLatestFromTfs(string currentBranch)
        {
            this.FetchFromTfs();
            var aheadBehind = SccHelperService.BranchAheadOrBehind(currentBranch, BlinkboxSccOptions.Current.TfsRemoteBranch);
            if (aheadBehind.Behind > 0)
            {
                NotificationService.DisplayError(
                    "Cannot proceed", 
                    "The current branch \""  + currentBranch + "\" is " + aheadBehind.Behind + " commits behind TFS. " + Environment.NewLine + "Please Get Latest and then try again.");

                return false;
            }

            this.notificationService.AddMessage("current branch is " + aheadBehind.Ahead + " commits ahead of TFS");
            return true;
        }

        /// <summary>
        /// Fetches from TFS into the tfs/default remote branch.
        /// </summary>
        private void FetchFromTfs()
        {
            SccHelperService.RunGitTfs("fetch", wait: true);
        }

        /// <summary>
        /// Merges if required.
        /// </summary>
        private void CommitIfRequired()
        {
            if (!this.sccHelper.WorkingDirectoryClean() || this.sccHelper.Tracker.IsInTheMiddleOfMerge)
            {
                this.sccHelper.RunTortoise("commit");
            }
        }
    }
}
