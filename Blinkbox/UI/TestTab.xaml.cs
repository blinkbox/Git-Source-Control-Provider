// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTab.xaml.cs" company="blinkbox">
//   TestTab control
// </copyright>
// <summary>
//   Interaction logic for TestTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System.Windows;
    using System.Windows.Controls;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for TestTab.xaml
    /// </summary>
    public partial class TestTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTab"/> class.
        /// </summary>
        public TestTab()
        {
            this.InitializeComponent();

            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) => this.RefreshBindings();
            }

            // PasswordBoxs dont support databinding - do it manually. 
            testSwarmPassword.PasswordChanged += (s, e) => { solutionUserSettings.TestSwarmPassword = ((PasswordBox)s).Password; };
        }

        /// <summary>
        /// Refreshes the data context bindings to update the UI.
        /// </summary>
        public void RefreshBindings()
        {
            // TODO: proper way to do this?
            grid.DataContext = null;
            grid.DataContext = this;
            testSwarmPassword.Password = this.solutionUserSettings.TestSwarmPassword;
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
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SolutionSettings.Current.Save();
            SolutionUserSettings.Current.Save();
        }

        /// <summary>
        /// Handles the Click event of the Hyperlink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            BasicSccProvider.LaunchBrowser(SolutionSettings.Current.TestSwarmUrl + "/user/" + SolutionUserSettings.Current.TestSwarmUsername);
        }

    }
}
