// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolutionUserSettings.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;

    using GitScc.Blinkbox.Data;

    /// <summary>
    /// Provides user settings for a solution.
    /// </summary>
    public class SolutionUserSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionUserSettings sccOptions;

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static SolutionUserSettings Current
        {
            get
            {
                if (sccOptions == null)
                {
                    var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
                    var solutionDirectory = sccProvider.GetSolutionDirectory();
                    var solutionFileName = sccProvider.GetSolutionFileName();
                    if (!string.IsNullOrEmpty(solutionFileName))
                    {
                        // Solution is open. 
                        string configFileName = Path.Combine(solutionDirectory, Path.GetFileNameWithoutExtension(solutionFileName) + "." + Extension + ".user");
                        sccOptions = SettingsBase.LoadFromConfig<SolutionUserSettings>(configFileName);
                    }
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Gets or sets the test swarm project id.
        /// </summary>
        /// <value>The test swarm project id.</value>
        public string TestSwarmProjectId { get; set; }

        /// <summary>
        /// Gets or sets the test swarm auth token.
        /// </summary>
        /// <value>The test swarm auth token.</value>
        public string TestSwarmAuthToken { get; set; }

        /// <summary>
        /// Gets or sets the test swarm URL.
        /// </summary>
        /// <value>The test swarm URL.</value>
        public string TestSwarmUrl { get; set; }

        /// <summary>
        /// Gets or sets the test swarm tags.
        /// </summary>
        /// <value>The test swarm tags.</value>
        public string TestSwarmTags { get; set; }

        /// <summary>
        /// Gets or sets the submit tests on deploy.
        /// </summary>
        /// <value>The submit tests on deploy.</value>
        public bool? SubmitTestsOnDeploy { get; set; }

        /// <summary>
        /// Gets or sets the local app URL template.
        /// </summary>
        /// <value>The local app URL template.</value>
        public string LocalAppUrlTemplate { get; set; }

        /// <summary>
        /// Gets or sets the local test URL template.
        /// </summary>
        /// <value>The local test URL template.</value>
        public string LocalTestUrlTemplate { get; set; }

        /// <summary>
        /// Gets or sets the test browser sets.
        /// </summary>
        /// <value>The test browser sets.</value>
        public string TestBrowserSets { get; set; }

        /// <summary>
        /// Gets or sets the test runner mode.
        /// </summary>
        /// <value>The test runner mode.</value>
        public string TestRunnerMode { get; set; }

        /// <summary>
        /// Gets or sets the last deployment.
        /// </summary>
        /// <value>The last deployment.</value>
        public Deployment LastDeployment { get; set; }

        /// <summary>
        /// Gets or sets the sub-path for feature files.
        /// </summary>
        /// <value>The sub-path for feature files.</value>
        public string FeatureSubPath { get; set; }

        /// <summary>
        /// Gets or sets the OpenUrlsOnDeploy option.
        /// </summary>
        public bool OpenUrlsOnDeploy { get; set; }

        /// <summary>
        /// Gets or sets the notify on deploy setting.
        /// </summary>
        /// <value>The notify on deploy setting.</value>
        public bool NotifyOnDeploy { get; set; }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.TestSwarmProjectId = string.IsNullOrEmpty(this.TestSwarmProjectId)
                ? string.Empty
                : this.TestSwarmProjectId;

            this.TestSwarmAuthToken = string.IsNullOrEmpty(this.TestSwarmAuthToken)
                ? string.Empty
                : this.TestSwarmAuthToken;

            this.SubmitTestsOnDeploy = this.SubmitTestsOnDeploy ?? true;

            // An update to the solution settings should override the user settings. 
            this.TestSwarmUrl = string.IsNullOrEmpty(this.TestSwarmUrl)
                ? SolutionSettings.Current.TestSwarmUrl
                : this.TestSwarmUrl;

            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags)
                ? SolutionSettings.Current.TestSwarmTags
                : this.TestSwarmTags;

            this.TestBrowserSets = string.IsNullOrEmpty(this.TestBrowserSets) 
                ? SolutionSettings.Current.TestBrowserSets
                : this.TestBrowserSets;

            this.TestRunnerMode = string.IsNullOrEmpty(this.TestRunnerMode) 
                ? SolutionSettings.Current.TestRunnerMode
                : this.TestRunnerMode;

            this.LocalAppUrlTemplate = string.IsNullOrEmpty(this.LocalAppUrlTemplate) 
                ? SolutionSettings.Current.LocalAppUrlTemplate
                : this.LocalAppUrlTemplate;

            this.LocalTestUrlTemplate = string.IsNullOrEmpty(this.LocalTestUrlTemplate) 
                ? SolutionSettings.Current.LocalTestUrlTemplate
                : this.LocalTestUrlTemplate;
        }
    }
}
