// -----------------------------------------------------------------------
// <copyright file="UserSettings.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UserSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static UserSettings sccOptions;

        /// <summary>
        /// Gets Current.
        /// </summary>
        public static UserSettings Current
        {
            get
            {
                if (sccOptions == null)
                {
                    string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "blinkboxScc.settings");
                    sccOptions = SettingsBase.LoadFromConfig<UserSettings>(configFileName);
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Gets or sets the preview get latest.
        /// </summary>
        /// <value>The preview get latest.</value>
        public bool? PreviewGetLatest { get; set; }

        /// <summary>
        /// Gets or sets the open urls in VS.
        /// </summary>
        /// <value>The open urls in VS.</value>
        public bool? OpenUrlsInVS { get; set; }

        /// <summary>
        /// Gets or sets the location of git-tfs.
        /// </summary>
        /// <value>the location of git-tfs.</value>
        public string GitTfsLocation { get; set; }

        /// <summary>
        /// Gets or sets the location of the current release manifest file.
        /// </summary>
        /// <value>the location of the current release manifest file.</value>
        public string ReleaseLocation { get; set; }

        /// <summary>
        /// Gets or sets the location of the current release manifest file.
        /// </summary>
        /// <value>the location of the current release manifest file.</value>
        public Version LastVersionPrompt { get; set; }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.PreviewGetLatest = this.PreviewGetLatest ?? false;
            this.OpenUrlsInVS = this.OpenUrlsInVS ?? false;
            this.GitTfsLocation = string.IsNullOrEmpty(this.GitTfsLocation) ? "C:\\Program Files (x86)\\Git-tfs\\git-tfs.exe" : this.GitTfsLocation;
            this.ReleaseLocation = string.IsNullOrEmpty(this.ReleaseLocation) ? "P:\\Software\\git\\BBGitSourceControl\\" : this.ReleaseLocation;
            this.LastVersionPrompt = this.LastVersionPrompt ?? new Version();
        }
    }
}
