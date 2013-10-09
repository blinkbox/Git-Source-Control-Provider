// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandId.bb.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    /// <summary>
    /// Defines the command ids used for blinkbox commands in the Visual Studio environment.
    /// </summary>
    public static partial class CommandId
    {
        /// <summary>
        /// Identifies the Deploy command.
        /// </summary>
        public const int BlinkboxDeployId = 0x117;

        /// <summary>
        /// Identifies the SubmitTest command.
        /// </summary>
        public const int SubmitTestButtonId = 0x118;

        /// <summary>
        /// Identifies the SubmitTest command.
        /// </summary>
        public const int UnitTestButtonId = 0x119;

        /// <summary>
        /// Identifies the ToolsMenu command.
        /// </summary>
        public const int ToolsMenu = 0x401;

        /// <summary>
        /// Identifies the ToolsMenuGroup command.
        /// </summary>
        public const int ToolsMenuGroup = 0x402;

        /// <summary>
        /// Identifies the GitTfsCheckin command.
        /// </summary>
        public const int GitTfsCheckinButtonId = 0x403;

        /// <summary>
        /// Identifies the GitTfsGetLatest command.
        /// </summary>
        public const int GitTfsGetLatestButtonId = 0x404;

        /// <summary>
        /// Identifies the GitTfs CleanWorkspaces command.
        /// </summary>
        public const int GitTfsCleanWorkspacesButtonId = 0x405;

        /// <summary>
        /// Identifies the GitTfs Review command.
        /// </summary>
        public const int GitTfsReviewButtonId = 0x406;

        /// <summary>
        /// Identifies the GitTfs Cancel Review command.
        /// </summary>
        public const int GitTfsCancelReviewButtonId = 0x407;
    }
}
