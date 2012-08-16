// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Deploy.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Blinkbox implementation inheriting from GitSourceControlProvider.
//   BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Options;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using MessageBox = System.Windows.MessageBox;
    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    public class Deploy : IDisposable
    {
        /// <summary>
        /// Builds and deploys.
        /// </summary>
        /// <param name="commit">
        /// The commit. Supplied if called after a successful commit, otherwise a new instance is created. 
        /// </param>
        /// <returns>
        /// true if the deploy was successful.
        /// </returns>
        public bool RunDeploy(CommitData commit)
        {
            NotificationWriter.Write("Begin build and deploy to " + commit.Hash);

            try
            {
                // Look for a deploy project
                var buildProjectFileName = BasicSccProvider.GetSolutionDirectory() + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName;
                if (!File.Exists(buildProjectFileName))
                {
                    MessageBox.Show("build project not found", "Deploy abandoned", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                NotificationWriter.Write("Deploy project found at " + buildProjectFileName);

                // Initisalise our own project collection which can be cleaned up after the build. This is to prevent caching of the project. 
                using (var projectCollection = new ProjectCollection(Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.ToolsetLocations))
                {
                    var commitComment = Regex.Replace(commit.Message, @"\r|\n|\t", string.Empty);
                    commitComment = HttpUtility.UrlEncode(commitComment.Substring(0, commitComment.Length > 80 ? 80 : commitComment.Length));

                    // Global properties need to be set before the projects are instantiated. 
                    var globalProperties = new Dictionary<string, string>
                        {
                            { BlinkboxSccOptions.Current.CommitGuidPropertyName, commit.Hash }, 
                            { BlinkboxSccOptions.Current.CommitCommentPropertyName, commitComment }
                        };
                    var msbuildProject = new ProjectInstance(buildProjectFileName, globalProperties, "4.0", projectCollection);

                    // Build it
                    BasicSccProvider.WriteToStatusBar("Building " + Path.GetFileNameWithoutExtension(msbuildProject.FullPath));
                    var buildRequest = new BuildRequestData(msbuildProject, new string[] { });

                    var buildParams = new BuildParameters(projectCollection);
                    buildParams.Loggers = new List<ILogger>() { new BuildNotificationLogger() { Verbosity = LoggerVerbosity.Minimal } };

                    var result = BuildManager.DefaultBuildManager.Build(buildParams, buildRequest);

                    if (result.OverallResult == BuildResultCode.Failure)
                    {
                        string message = result.Exception == null
                            ? "An error occurred during build; please see the pending changes window for details."
                            : result.Exception.Message;
                        MessageBox.Show(message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    // Launch urls in browser
                    var launchUrls = msbuildProject.Items.Where(pii => pii.ItemType == BlinkboxSccOptions.Current.UrlToLaunchPropertyName);
                    foreach (var launchItem in launchUrls)
                    {
                        this.LaunchBrowser(launchItem.EvaluatedInclude);
                    }

                    // Clean up project to prevent caching.
                    projectCollection.UnloadAllProjects();
                    projectCollection.UnregisterAllLoggers();
                }

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Launches the provided url in the Visual Studio browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void LaunchBrowser(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string errorMessage;
                try
                {
                    if (Blinkbox.Options.BlinkboxSccOptions.Current.LaunchDeployedUrlsInVS)
                    {
                        // launch in visual studio browser
                        var browserService = BasicSccProvider.GetServiceEx<SVsWebBrowsingService>() as IVsWebBrowsingService;
                        if (browserService != null)
                        {
                            IVsWindowFrame frame;

                            // passing 0 to the NavigateFlags allows the browser service to reuse open instances of the internal browser.
                            browserService.Navigate(url, 0, out frame);
                        }
                    }
                    else
                    {
                        // Launch in default browser
                        System.Diagnostics.Process.Start(url);
                    }

                    return;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }

                MessageBox.Show("Cannot launch " + url + ": " + errorMessage, "Browser failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            
        }
    }
}