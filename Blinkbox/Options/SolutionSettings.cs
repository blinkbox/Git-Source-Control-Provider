// -----------------------------------------------------------------------
// <copyright file="SolutionSettings.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System.IO;

    /// <summary>
    /// Provides settings specific to a solution.
    /// </summary>
    public class SolutionSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionSettings sccOptions;

        /// <summary>
        /// Gets Current.
        /// </summary>
        public static SolutionSettings Current
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
                        string configFileName = Path.Combine(solutionDirectory, Path.GetFileNameWithoutExtension(sccProvider.GetSolutionFileName()) + "." + Extension);

                        sccOptions = SettingsBase.LoadFromConfig<SolutionSettings>(configFileName);
                    }
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Gets or sets the test swarm URL.
        /// </summary>
        /// <value>The test swarm URL.</value>
        public string TestSwarmUrl { get; set; }

        /// <summary>
        /// Gets or sets the feature path.
        /// </summary>
        /// <value>The feature path.</value>
        public string FeaturePath { get; set; }

        /// <summary>
        /// Gets or sets the test browser sets.
        /// </summary>
        /// <value>The test browser sets.</value>
        public string TestBrowserSets { get; set; }

        /// <summary>
        /// Gets or sets the current branch.
        /// </summary>
        /// <value>The current branch.</value>
        public string CurrentBranch { get; set; }

        /// <summary>
        /// Gets or sets the test runner mode.
        /// </summary>
        /// <value>The test runner mode.</value>
        public string TestRunnerMode { get; set; }

        /// <summary>
        /// Gets or sets the test swarm tags.
        /// </summary>
        /// <value>The test swarm tags.</value>
        public string TestSwarmTags { get; set; }

        /// <summary>
        /// Gets or sets the deploy project location.
        /// </summary>
        /// <value>The deploy project location.</value>
        public string DeployProjectLocation { get; set; }

        /// <summary>
        /// Gets or sets the current release.
        /// </summary>
        /// <value>The current release.</value>
        public string CurrentRelease { get; set; }

        /// <summary>
        /// Gets or sets the test submission script.
        /// </summary>
        /// <value>The test submission script.</value>
        public string TestSubmissionScript { get; set; }

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
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.TestSwarmUrl = string.IsNullOrEmpty(this.TestSwarmUrl) ? "http://battleship/testswarm" : this.TestSwarmUrl;
            this.FeaturePath = string.IsNullOrEmpty(this.FeaturePath) ? "test\\AngularClient.Test.Artefacts\\Features" : this.FeaturePath;
            this.TestBrowserSets = string.IsNullOrEmpty(this.TestBrowserSets) ? "default" : this.TestBrowserSets;
            this.CurrentBranch = string.IsNullOrEmpty(this.CurrentBranch) ? "v0" : this.CurrentBranch;
            this.TestRunnerMode = string.IsNullOrEmpty(this.TestRunnerMode) ? "appfirst" : this.TestRunnerMode;
            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags) ? "devcomplete" : this.TestSwarmTags;
            this.DeployProjectLocation = string.IsNullOrEmpty(this.DeployProjectLocation) ? "deploy\\deployLocal.ps1" : this.DeployProjectLocation;
            this.CurrentRelease = string.IsNullOrEmpty(this.CurrentRelease) ? "v0.0.0" : this.CurrentRelease;
            this.TestSubmissionScript = string.IsNullOrEmpty(this.TestSubmissionScript) ? "deploy\\testSwarm.ps1" : this.TestSubmissionScript;
            this.LocalAppUrlTemplate = string.IsNullOrEmpty(this.LocalAppUrlTemplate)
               ? "http://tv-{MachineName}.bbdev1.com/Client/{BuildLabel}"
               : this.LocalAppUrlTemplate;
            this.LocalTestUrlTemplate = string.IsNullOrEmpty(this.LocalTestUrlTemplate)
                ? (this.LocalAppUrlTemplate + "/Test/Index.html?runnerMode={RunnerMode}&tags={Tags}")
                : this.LocalTestUrlTemplate;
        }
    }
}
