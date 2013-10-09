// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AheadBehind.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Data
{
    /// <summary>
    /// Contains the number of commits a branch is ahead or behind.
    /// </summary>
    public struct AheadBehind
    {
        /// <summary>
        /// Gets or sets the number of commits ahead.
        /// </summary>
        /// <value>The number of commits ahead.</value>
        public int Ahead { get; set; }

        /// <summary>
        /// Gets or sets the number of commits behind.
        /// </summary>
        /// <value>The number of commits behind.</value>
        public int Behind { get; set; }
    }
}
