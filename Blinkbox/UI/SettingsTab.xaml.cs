// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsTab.xaml.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// <summary>
//   Interaction logic for SettingsTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System.Windows.Controls;

    using GitScc.Blinkbox.Options;

    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsTab"/> class.
        /// </summary>
        public SettingsTab()
        {
            InitializeComponent();

            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProviderService = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProviderService != null)
            {
                sccProviderService.OnSolutionOpen += (s, a) => this.RefreshBindings();
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
        /// Gets the user settings.
        /// </summary>
        /// <value>The user settings.</value>
        public UserSettings userSettings
        {
            get
            {
                return UserSettings.Current;
            }
        }

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <value>The user settings.</value>
        public GitSccOptions sccSettings
        {
            get
            {
                return GitSccOptions.Current;
            }
        }

        /// <summary>
        /// Gets or sets the text for the current version.
        /// </summary>
        public string CurrentVersionText { get; set; }

        /// <summary>
        /// Gets or sets the text for the install link.
        /// </summary>
        public string InstallText { get; set; }

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
            GitSccOptions.Current.SaveConfig();
        }

        /// <summary>
        /// installs the latest version of the app.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void InstallButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var sccService = BasicSccProvider.GetServiceEx<BasicSccProvider>();
            if (sccService != null)
            {
                sccService.InstallNewVersion();
            }
        }
    }
}
