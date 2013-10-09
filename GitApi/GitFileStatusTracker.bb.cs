// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitFileStatusTracker.bb.cs" company="blinkbox">
//   Blinkbox implementation of GitFileStatusTracker
// </copyright>
// <summary>
//   Defines the GitFileStatusTracker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Note: namespace does not match the location, to match the original implementation. 
namespace GitScc
{
    using System;
    using System.IO;

    using NGit.Revwalk;
    using NGit.Treewalk;

    /// <summary>
    /// Blinkbox implementation of GitFileStatusTracker
    /// </summary>
    public partial class GitFileStatusTracker
    {
        /// <summary>
        /// the name of the head branch.
        /// </summary>
        public const string HeadBranch = "HEAD";

        /// <summary>
        /// Save the content of a file on the specified branch to the specified temp file. 
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="tempFile">
        /// The temp file.
        /// </param>
        /// <param name="branchName">
        /// The branch name.
        /// </param>
        public void SaveFileFromRepository(string fileName, string tempFile, string branchName)
        {
            if (!this.HasGitRepository || this.head == null)
            {
                return;
            }

            branchName = string.IsNullOrEmpty(branchName) ? HeadBranch : branchName;

            if (GitBash.Exists)
            {
                string fileNameRel = GetRelativeFileNameForGit(fileName);
                GitBash.RunCmd(string.Format("show \"{0}:{1}\" > \"{2}\"", branchName, fileNameRel, tempFile), this.GitWorkingDirectory);
            }
            else
            {
                var data = this.GetFileContent(fileName, branchName);
                using (var binWriter = new BinaryWriter(File.Open(tempFile, System.IO.FileMode.Create)))
                {
                    binWriter.Write(data ?? new byte[] { });
                }
            }
        }

        /// <summary>
        /// Get the content of a file from the specified branch
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="branchName">
        /// The branch name.
        /// </param>
        /// <returns>
        /// The contents of a file as a byte[]
        /// </returns>
        public byte[] GetFileContent(string fileName, string branchName)
        {
            if (!HasGitRepository || string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            branchName = string.IsNullOrEmpty(branchName) ? HeadBranch : branchName;
            fileName = GetRelativeFileNameForGit(fileName);

            try
            {
                var branch = repository.Resolve(branchName);
                var revTree = branch == null ? null : new RevWalk(repository).ParseTree(branch);
                if (revTree != null)
                {
                    var entry = TreeWalk.ForPath(repository, fileName, revTree);
                    if (entry != null && !entry.IsSubtree)
                    {
                        var blob = repository.Open(entry.GetObjectId(0));
                        if (blob != null)
                        {
                            return blob.GetCachedBytes();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Get File Content: {0}\r\n{1}", fileName, ex.ToString());
            }

            return null;
        } 
    }
}
