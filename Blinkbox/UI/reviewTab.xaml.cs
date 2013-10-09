// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReviewTab.xaml.cs" company="blinkbox">
//   
// </copyright>
// <summary>
//   Interaction logic for reviewTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for reviewTab.xaml
    /// </summary>
    public partial class ReviewTab
    {
        /// <summary>
        /// the sorting order of the file list
        /// </summary>
        private const string SortMemberPath = "FileName";

        /// <summary>
        /// Sort direction of the file list
        /// </summary>
        private const ListSortDirection SortDirection = ListSortDirection.Ascending;

        /// <summary>
        /// Instance of the sccProvider. 
        /// </summary>
        private readonly SccProviderService service;

        /// <summary>
        /// instance of the <see cref="SccProviderService"/>
        /// </summary>
        private BBPendingChanges bbPendingChanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewTab"/> class.
        /// </summary>
        public ReviewTab()
        {
            this.InitializeComponent();

            this.service = BasicSccProvider.GetServiceEx<SccProviderService>();
        }

        /// <summary>
        /// Gets the development service.
        /// </summary>
        /// <value>The development service.</value>
        private BBPendingChanges BBPendingChanges
        {
            get
            {
                this.bbPendingChanges = this.bbPendingChanges ?? BasicSccProvider.GetServiceEx<BBPendingChanges>();
                return this.bbPendingChanges;
            }
        }

        /// <summary>
        /// Initialises the blinkbox extensions.
        /// </summary>
        public void InitialiseBlinkboxExtensions()
        {
            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
        }

        /// <summary>
        /// Show a list of files for review.
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        /// <param name="branchName">
        /// The branch Name.
        /// </param>
        public void Review(List<GitFile> changedFiles, string branchName)
        {
            if (changedFiles.Any())
            {
                this.DisplayReview(changedFiles);
            }
        }

        /// <summary>
        /// Cancel the current review and enable the pending changes list. 
        /// </summary>
        public void CancelReview()
        {
        }

        /// <summary>
        /// Writes a message to the diff editor
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteToDiffWindow(string message)
        {
            var action = new Action(() => this.ReviewDiff.AppendText(Environment.NewLine + message));
            this.ReviewDiff.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Clears the diff editor.
        /// </summary>
        public void ClearDiffEditor()
        {
            var action = new Action(() => this.ReviewDiff.Clear());
            this.ReviewDiff.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// List the provided files in the pending changes window for reivew. 
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        public void DisplayReview(List<GitFile> changedFiles)
        {
            Action act = () =>
            {
                if (!GitBash.Exists)
                {
                    ////Settings.Show();
                    return;
                }

                ////Settings.Hide();
                BBPendingChanges.ReviewTab.IsEnabled = true;

                //////if (tracker == null)
                //////{
                //////    service.NoRefresh = true;
                //////    ClearUI();
                //////    service.NoRefresh = false;
                //////    return;
                //////}

                ////service.NoRefresh = true;

                ////var selectedFile = GetSelectedFileName();
                ////var selectedFiles = this.dataGrid1.Items.Cast<GitFile>()
                ////    .Where(i => i.IsSelected)
                ////    .Select(i => i.FileName).ToList();

                this.reviewList.BeginInit();

                try
                {
                    this.reviewList.ItemsSource = changedFiles;

                    var view = CollectionViewSource.GetDefaultView(this.reviewList.ItemsSource);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(SortMemberPath, SortDirection));
                        view.Refresh();
                    }

                    ////this.reviewList.SelectedValue = selectedFile;
                    ////selectedFiles.ForEach(fn =>
                    ////{
                    ////    var item = this.reviewList.Items.Cast<GitFile>().FirstOrDefault(i => i.FileName == fn);
                    ////    if (item != null)
                    ////    {
                    ////        item.IsSelected = true;
                    ////    }
                    ////});

                    ////ShowStatusMessage(string.Empty);
                }
                catch (Exception ex)
                {
                    NotificationService.DisplayException(ex, "Review failed");
                }

                this.reviewList.EndInit();

                BBPendingChanges.ReviewTab.Focus();

                service.NoRefresh = false;
                service.lastTimeRefresh = DateTime.Now;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Ends the review.
        /// </summary>
        public void EndReview()
        {
            Action act = () =>
            {
                BBPendingChanges.ReviewTab.IsEnabled = false;
                BBPendingChanges.GitTab.Focus();
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Review control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        public void Review_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Action act = () =>
            {
                service.NoRefresh = true;
                try
                {
                    this.ReviewDiff.Text = string.Empty;

                    var reviewItem = this.GetSelectedReviewItem();
                    if (reviewItem == null || string.IsNullOrEmpty(reviewItem.FileName))
                    {
                        return;
                    }

                    // Diff the selected file against the tfs version using git
                    var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();
                    var fileNameRel = BBPendingChanges.GetTracker().GetRelativeFileName(reviewItem.FileName);
                    var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsRemoteBranch);
                    var diff = sccHelper.DiffFileWithGit(fileNameRel, tfsRevision);

                    // Show in the diff window
                    this.ReviewDiff.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(".diff");
                    this.ReviewDiff.Text = diff;
                }
                catch (Exception ex)
                {
                    NotificationService.DisplayException(ex, "Diff failed");
                }

                service.NoRefresh = false;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Opens a tortoise diff.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Review_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Otherwise, use tortiose git to provide the diff.
            var reviewItem = this.GetSelectedReviewItem();
            if (reviewItem != null && !string.IsNullOrEmpty(reviewItem.FileName))
            {
                var fileName = System.IO.Path.Combine(BBPendingChanges.GetTracker().GitWorkingDirectory, reviewItem.FileName);

                if (reviewItem.Status == GitFileStatus.Added)
                {
                    BBPendingChanges.OpenFile(fileName);
                    return;
                }

                var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();

                // Diff the file in tortoise
                var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsRemoteBranch);
                sccHelper.DiffFileInTortoise(fileName, tfsRevision, BlinkboxSccOptions.WorkingDirectoryRevision);
            }
        }

        /// <summary>
        /// Opens the file in VS.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Diff_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Open the file in VS
            var reviewItem = this.GetSelectedReviewItem();
           
            if (reviewItem != null && !string.IsNullOrEmpty(reviewItem.FileName))
            {
                var fileName = System.IO.Path.Combine(BBPendingChanges.GetTracker().GitWorkingDirectory, reviewItem.FileName);
                this.OpenFile(fileName);
            }
        }

        /// <summary>
        /// Opens a file in VS.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        private void OpenFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            fileName = fileName.Replace("/", "\\");
            var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
            bool opened = false;
            var projects = (Array)dte.ActiveSolutionProjects;

            foreach (dynamic project in projects)
            {
                foreach (dynamic item in project.ProjectItems)
                {
                    if (string.Compare(item.FileNames[0], fileName, true) == 0)
                    {
                        dynamic wnd = item.Open(EnvDTE.Constants.vsViewKindPrimary);
                        wnd.Activate();
                        opened = true;
                        break;
                    }
                }
                if (opened)
                {
                    break;
                }
            }

            if (!opened)
            {
                dte.ItemOperations.OpenFile(fileName);
            }
        }

        /// <summary>
        /// Gets the selected review item.
        /// </summary>
        /// <returns>the selected item.</returns>
        private GitFile GetSelectedReviewItem()
        {
            if (this.reviewList.SelectedCells.Any())
            {
                var selectedItem = this.reviewList.SelectedCells[0].Item as GitFile;
                if (selectedItem != null)
                {
                    return selectedItem;
                }
            }

            return null;
        }
    }
}
