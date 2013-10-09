// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BBPendingChanges.xaml.cs" company="blinkbox">
//   TODO: add comment
// </copyright>
// <summary>
//   Interaction logic for BBPendingChanges.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using GitScc.Blinkbox.Data;

    /// <summary>
    /// Interaction logic for BBPendingChanges.xaml
    /// </summary>
    public partial class BBPendingChanges
    {
        /// <summary>
        /// reference to the tracker
        /// </summary>
        private GitFileStatusTracker tracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="BBPendingChanges"/> class.
        /// </summary>
        public BBPendingChanges()
        {
            this.InitializeComponent();
            BasicSccProvider.RegisterService(this);
        }

        /// <summary>
        /// Gets the review tab.
        /// </summary>
        /// <value>The review tab.</value>
        public TabItem ReviewTab
        {
            get
            {
                return this.reviewTabItem;
            }
        }

        /// <summary>
        /// Gets the git tab.
        /// </summary>
        /// <value>The git tab.</value>
        public TabItem GitTab
        {
            get
            {
                return this.gitTabItem;
            }
        }

        /// <summary>
        /// Gets the deploy tab.
        /// </summary>
        /// <value>The deploy tab.</value>
        public TabItem DeployTab
        {
            get
            {
                return this.deployTabItem;
            }
        }

        /// <summary>
        /// Commits the working directory.
        /// </summary>
        public void Commit()
        {
            pendingChangesView.Commit();
        }

        /// <summary>
        /// Amends the previous commit.
        /// </summary>
        public void AmendCommit()
        {
            pendingChangesView.AmendCommit();
        }

        /// <summary>
        /// Refreshes the pending changes.
        /// </summary>
        /// <param name="currentTracker">The tracker.</param>
        public void RefreshPendingChanges(GitFileStatusTracker currentTracker)
        {
            this.tracker = currentTracker;
            pendingChangesView.Refresh(currentTracker);
        }

        /// <summary>
        /// Gets the tracker.
        /// </summary>
        /// <returns>the current tracker.</returns>
        public GitFileStatusTracker GetTracker()
        {
            return this.tracker;
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void OpenFile(string fileName)
        {
            pendingChangesView.OpenFile(fileName);
        }

        /// <summary>
        /// Reviews the specified changed files.
        /// </summary>
        /// <param name="changedFiles">The changed files.</param>
        /// <param name="branchName">Name of the branch.</param>
        public void Review(List<GitFile> changedFiles, string branchName = null)
        {
            if (changedFiles.Any())
            {
                ////this.comparisonBranch = branchName;
                reviewTab.DisplayReview(changedFiles);
            }
        }

        /// <summary>
        /// Ends the review.
        /// </summary>
        public void EndReview()
        {
            reviewTab.EndReview();
        }

        /// <summary>
        /// Updates the TFS status.
        /// </summary>
        /// <param name="aheadBehind">The ahead behind.</param>
        public void UpdateTfsStatus(AheadBehind aheadBehind)
        {
            Action action = () =>
            {
                var text = string.Format("{0} ahead, {1} behind TFS", aheadBehind.Ahead, aheadBehind.Behind);
                TfsStatusLabel.Content = text;
                TfsStatusLabel.Foreground = aheadBehind.Behind == 0 ? System.Windows.Media.Brushes.DarkGreen : System.Windows.Media.Brushes.DarkRed;
            };
            this.Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
        }
    }
}
