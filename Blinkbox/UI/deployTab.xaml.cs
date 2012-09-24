﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="deployTab.xaml.cs" company="blinkbox">
//   TODO: add comment
// </copyright>
// <summary>
//   Interaction logic for deployTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for deployTab.xaml
    /// </summary>
    public partial class deployTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="deployTab"/> class.
        /// </summary>
        public deployTab()
        {
            this.InitializeComponent();

            // Allow binding to local properties
            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) => this.RefreshBindings();
            }
        }

        /// <summary>
        /// Gets the solution user settings.
        /// </summary>
        /// <value>The solution user settings.</value>
        public SolutionUserSettings solutionUserSettings
        {
            get
            {
                return SolutionUserSettings.Current;
            }
        }

        /// <summary>
        /// Gets the solution settings.
        /// </summary>
        /// <value>The solution settings.</value>
        public SolutionSettings solutionSettings
        {
            get
            {
                return SolutionSettings.Current;
            }
        }

        /// <summary>
        /// Refreshes the data context bindings to update the UI.
        /// </summary>
        public void RefreshBindings()
        {
            // TODO: proper way to do this?
            grid.DataContext = null;
            grid.DataContext = this;
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SolutionSettings.Current.Save();
            SolutionUserSettings.Current.Save();
            UserSettings.Current.Save();
        }

        /// <summary>
        /// Handles the Click event of the Deploylink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Deploylink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SolutionUserSettings.Current.LastDeployment == null)
            {
                return;
            }

            var url = sender == AppLink ? SolutionUserSettings.Current.LastDeployment.AppUrl : SolutionUserSettings.Current.LastDeployment.TestRunUrl;
            BasicSccProvider.LaunchBrowser(url);
        }
    }
}
