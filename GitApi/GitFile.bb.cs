// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitFile.bb.cs" company="blinkbox">
//   Blinkbox implementation of GitFile
// </copyright>
// <summary>
//   Defines the GitFile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Note: namespace does not match the location, to match the original implementation. 
namespace GitScc
{
    using System;

    /// <summary>
    /// Blinkbox implementation of GitFile
    /// The original implementation of this class is located in GitFileStatus.cs
    /// </summary>
    public partial class GitFile
    {
        /// <summary>
        /// Parses a line of output from "git diff --name-status" into a gitFile instance. 
        /// </summary>
        /// <param name="diffLine">A line of output from "git diff --name-status".</param>
        /// <returns>A GitFile instance</returns>
        public static GitFile FromDiff(string diffLine)
        {
            var parts = diffLine.Split("\t".ToCharArray());
            var filename = parts[1].Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries)[0]; // if a rename has occurred the filename is "original -> new"
            var status = parts[0].Substring(0, 1); // If a merge has occurred, the status is "XY", otherwise its just "X"

            return new GitFile { FileName = filename, Status = ParseGitStatus(status) };
        }

        /// <summary>
        /// Parses the git status flag.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>the equivalent GitFileStatus</returns>
        public static GitFileStatus ParseGitStatus(string status)
        {
            switch (status)
            {
                case "M":
                    return GitFileStatus.Modified;

                case "A":
                    return GitFileStatus.Added;

                case "D":
                    return GitFileStatus.Deleted;

                case "R":
                    return GitFileStatus.Renamed;

                case "C":
                    return GitFileStatus.Copied;

                case "U":
                    return GitFileStatus.Modified;

                case "?":
                    return GitFileStatus.NotControlled;

                case "!":
                    return GitFileStatus.Ignored;

                default: 
                    return GitFileStatus.Ignored;
            }
        }
    }
}
