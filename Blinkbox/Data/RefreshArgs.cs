// -----------------------------------------------------------------------
// <copyright file="RefreshArgs.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RefreshArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshArgs"/> class.
        /// </summary>
        /// <param name="forceUpdate">if set to <c>true</c> [force update].</param>
        public RefreshArgs(bool forceUpdate = false)
        {
            this.ForceUpdate = forceUpdate;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [force update].
        /// </summary>
        /// <value><c>true</c> if [force update]; otherwise, <c>false</c>.</value>
        public bool ForceUpdate { get; set; }
    }
}
