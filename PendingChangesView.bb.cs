// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesView.bb.cs" company="blinkbox">
//   blinkbox implementation of PendingChangesView.
// </copyright>
// <summary>
//   blinkbox implementation of PendingChangesView.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.Windows.Input;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// BB extensions to the PendingChangesView
    /// </summary>
    public partial class PendingChangesView
    {
        /// <summary>
        /// The name of the branch to be used for reviewing
        /// </summary>
        private readonly string comparisonBranch;

        /// <summary>
        /// Initialises the blinkbox extensions.
        /// </summary>
        public void InitialiseBlinkboxExtensions()
        {
            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
        }

        /// <summary>
        /// Writes a message to the diff editor
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteToDiffWindow(string message)
        {
            var action = new Action(() => this.DiffEditor.AppendText(Environment.NewLine + message));
            this.DiffEditor.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Clears the diff editor.
        /// </summary>
        public void ClearDiffEditor()
        {
            var action = new Action(() => this.DiffEditor.Clear());
            this.DiffEditor.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Replaces the double-click functionality with a tortoise-git diff, if available.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!BlinkboxSccOptions.Current.TortoiseAvailable)
            {
                // Compare the file (differs from existing implementation.
                this.GetSelectedFileFullName(fileName =>
                {
                    var sccService = BasicSccProvider.GetServiceEx<SccProviderService>();
                    sccService.CompareFile(fileName, null);
                });
                return;
            }

            // Otherwise, use tortiose git to provide the diff.
            this.GetSelectedFileFullName(fileName =>
            {
                var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();

                // Diff the file in tortoise
                var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsRemoteBranch);
                sccHelper.DiffFileInTortoise(fileName, tfsRevision, BlinkboxSccOptions.WorkingDirectoryRevision);
            });
        }
    }
}
