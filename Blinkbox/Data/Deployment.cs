// -----------------------------------------------------------------------
// <copyright file="Deployment.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    using System;

    /// <summary>
    /// Represents a user deployment. 
    /// </summary>
    [Serializable]
    public class Deployment
    {
        /// <summary>
        /// Gets or sets the build label.
        /// </summary>
        /// <value>The build label.</value>
        public string BuildLabel { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the app URL.
        /// </summary>
        /// <value>The app URL.</value>
        public string AppUrl { get; set; }

        /// <summary>
        /// Gets or sets the test run URL.
        /// </summary>
        /// <value>The test run URL.</value>
        public string TestRunUrl { get; set; }
    }
}
