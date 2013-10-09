// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeploymentService.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;
    using GitScc.Blinkbox.Powershell;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Performs deployments 
    /// </summary>
    public class DeploymentService : IDisposable
    {
        /// <summary>
        /// The current instance of the <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProviderService;

        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly CancellationTokenSource cancelRunAsync = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentService"/> class.
        /// </summary>
        /// <param name="basicSccProvider">The basic SCC provider.</param>
        public DeploymentService(BasicSccProvider basicSccProvider)
        {
            this.sccProviderService = basicSccProvider.GetService<SccProviderService>();
            this.notificationService = basicSccProvider.GetService<NotificationService>();
        }

        /// <summary>
        /// Deploys using the deploy project specified in settings.
        /// </summary>
        /// <param name="newDeployment">
        /// The new Deployment.
        /// </param>
        public void RunDeploy(Deployment newDeployment)
        {
            Func<Deployment, bool> buildAction;

            if (Path.GetExtension(SolutionSettings.Current.DeployProjectLocation) == ".ps1")
            {
                buildAction = deployment =>
                    {
                        try
                        {
                            // Call a powershell script to run the deployment.
                            var scriptName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.DeployProjectLocation);

                            var powershellArgs = new Dictionary<string, object>
                                {
                                    { "buildProjectPath", this.sccProviderService.GetSolutionFileName() },
                                    { "buildLabel", deployment.BuildLabel },
                                    { "branchName", SolutionSettings.Current.CurrentBranch },
                                    { "release", SolutionSettings.Current.CurrentRelease },
                                };

                            this.RunPowershell<object>(scriptName, powershellArgs);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            NotificationService.DisplayException(ex, "build failed");
                            return false;
                        }
                    };
            }
            else
            {
                buildAction = this.DeployMsBuild;
            }

            Action<Deployment> successFulBuildAction = deployment =>
                {
                    // Save the deployment in userSettings. 
                    var replacements = new Dictionary<string, string>
                        {
                            { "MachineName", Environment.MachineName },
                            { "BuildLabel", deployment.BuildLabel },
                            { "Tags", SolutionUserSettings.Current.TestSwarmTags },
                            { "RunnerMode", SolutionSettings.Current.TestRunnerMode },
                        };
                    deployment.AppUrl = SolutionUserSettings.Current.LocalAppUrlTemplate;
                    deployment.TestRunUrl = SolutionUserSettings.Current.LocalTestUrlTemplate;

                    foreach (var replacement in replacements)
                    {
                        deployment.AppUrl = deployment.AppUrl.Replace("{" + replacement.Key + "}", replacement.Value);
                        deployment.TestRunUrl = deployment.TestRunUrl.Replace("{" + replacement.Key + "}", replacement.Value);
                    }

                    SolutionUserSettings.Current.LastDeployment = deployment;
                    SolutionUserSettings.Current.Save();

                    if (SolutionUserSettings.Current.OpenUrlsOnDeploy)
                    {
                        this.OpenUrls(deployment);
                    }

                    if (SolutionUserSettings.Current.SubmitTestsOnDeploy.GetValueOrDefault())
                    {
                        // Submit tests to testswarm
                        this.SubmitTests();
                    }

                    if (SolutionUserSettings.Current.NotifyOnDeploy)
                    {
                        NotificationService.DisplayMessage("Deployment complete", deployment.BuildLabel + " deployment complete");
                    }
                };

            if (buildAction(newDeployment))
            {
                successFulBuildAction(newDeployment);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Submits a test run to testswarm.
        /// </summary>
        public void SubmitTests()
        {
            Action action = () =>
                {
                    this.notificationService.ClearMessages();

                    var scriptName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.TestSubmissionScript);
                    var featureDirectory = SccHelperService.GetAbsolutePath(SolutionSettings.Current.FeaturePath);

                    var lastDeployment = SolutionUserSettings.Current.LastDeployment;
                    if (lastDeployment == null || string.IsNullOrEmpty(lastDeployment.BuildLabel))
                    {
                        NotificationService.DisplayError("cannot find previous deployment", "cannot find the previous deployment - please re-deploy first");
                        return;
                    }

                    var browserSets = SolutionUserSettings.Current.TestBrowserSets.Split(new[] { ",", ".", "|", ";" }, StringSplitOptions.RemoveEmptyEntries);

                    var args = new Dictionary<string, object>
                        {
                            { "version", lastDeployment.BuildLabel },
                            { "featuresDirectory", featureDirectory },
                            { "featureSubPath", SolutionUserSettings.Current.FeatureSubPath },
                            { "branch", SolutionSettings.Current.CurrentBranch },
                            { "userName", SolutionUserSettings.Current.TestSwarmUsername },
                            { "password", SolutionUserSettings.Current.TestSwarmPassword },
                            { "appUrl", lastDeployment.AppUrl },
                            { "tag", SolutionUserSettings.Current.TestSwarmTags },
                            { "jobName", lastDeployment.Message + " (" + lastDeployment.BuildLabel + ")" },
                            { "browserSets", browserSets },
                            { "runnerMode", SolutionUserSettings.Current.TestRunnerMode }
                        };

                    this.RunPowershell<object>(scriptName, args);
                };

            this.RunAsync(action, "Submit Tests");
        }

        /// <summary>
        /// Runs the provided action asyncronously.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>The task.</returns>
        public Task RunAsync(Action action, string operation)
        {
            var task = new TaskFactory().StartNew(action, this.cancelRunAsync.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    NotificationService.DisplayException(t.Exception.Flatten().InnerException, operation + " failed");
                }
            });

            return task;
        }

        /// <summary>
        /// Deploys using the deploy project specified in settings.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns>true if the deploy was successful.</returns>
        private bool DeployMsBuild(Deployment deployment)
        {
            this.notificationService.AddMessage("Begin build and deploy to " + deployment.BuildLabel);

            // Look for a deploy project
            var buildProjectFileName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.DeployProjectLocation);

            if (!File.Exists(buildProjectFileName))
            {
                NotificationService.DisplayError("Deploy abandoned", "build project not found");
                return false;
            }

            this.notificationService.AddMessage("Deploy project found at " + buildProjectFileName);

            // Initisalise our own project collection which can be cleaned up after the build. This is to prevent caching of the project. 
            using (var projectCollection = new ProjectCollection(Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.ToolsetLocations))
            {
                var commitComment = Regex.Replace(deployment.Message, @"\r|\n|\t", string.Empty);
                commitComment = HttpUtility.UrlEncode(commitComment.Substring(0, commitComment.Length > 80 ? 80 : commitComment.Length));

                // Global properties need to be set before the projects are instantiated. 
                var globalProperties = new Dictionary<string, string>
                    {
                        { BlinkboxSccOptions.Current.CommitGuidPropertyName, deployment.BuildLabel }, 
                        { BlinkboxSccOptions.Current.CommitCommentPropertyName, commitComment }
                    };
                var msbuildProject = new ProjectInstance(buildProjectFileName, globalProperties, "4.0", projectCollection);

                // Build it
                this.notificationService.AddMessage("Building " + Path.GetFileNameWithoutExtension(msbuildProject.FullPath));
                var buildRequest = new BuildRequestData(msbuildProject, new string[] { });

                var buildParams = new BuildParameters(projectCollection);
                buildParams.Loggers = new List<ILogger> { new BuildNotificationLogger(this.notificationService) { Verbosity = LoggerVerbosity.Minimal } };

                var result = BuildManager.DefaultBuildManager.Build(buildParams, buildRequest);

                if (result.OverallResult == BuildResultCode.Failure)
                {
                    string message = result.Exception == null
                        ? "An error occurred during build; please see the pending changes window for details."
                        : result.Exception.Message;

                    NotificationService.DisplayError("Build failed", message);
                    return false;
                }

                var launchUrls = msbuildProject.Items.Where(pii => pii.ItemType == BlinkboxSccOptions.Current.UrlToLaunchPropertyName);

                // msbuild appends v2-dev onto the front. 
                deployment.BuildLabel = "v2-dev-" + deployment.BuildLabel;

                // Launch urls in browser
                this.notificationService.AddMessage("Launch urls...");
                foreach (var launchItem in launchUrls)
                {
                    BasicSccProvider.LaunchBrowser(launchItem.EvaluatedInclude);
                }

                // Clean up project to prevent caching.
                projectCollection.UnloadAllProjects();
                projectCollection.UnregisterAllLoggers();
            }

            return true;
        }

        /// <summary>
        /// Launches the test urls.
        /// </summary>
        /// <param name="deployment">
        /// The deployment.
        /// </param>
        private void OpenUrls(Deployment deployment)
        {
            try
            {
                BasicSccProvider.LaunchBrowser(deployment.AppUrl);
                BasicSccProvider.LaunchBrowser(deployment.TestRunUrl);
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Couldnt launch url");
            }
        }

        /// <summary>
        /// Runs a powershell script.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the return value. Hashtable for a returned object.
        /// </typeparam>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The return value of the script.
        /// </returns>
        private T RunPowershell<T>(string script, IEnumerable<KeyValuePair<string, object>> parameters) where T : class
        {
            try
            {
                var host = new PowershellHost();

                using (var runSpace = RunspaceFactory.CreateRunspace(host))
                using (var powerShell = System.Management.Automation.PowerShell.Create())
                {
                    // Open the runspace.
                    runSpace.Open();
                    powerShell.Runspace = runSpace;

                    // Set the execution policy so that scripts wil run. 
                    powerShell.AddScript("Set-ExecutionPolicy RemoteSigned");
                    powerShell.Invoke();

                    powerShell.AddCommand(script, false);
                    foreach (var parameter in parameters)
                    {
                        powerShell.AddParameter(parameter.Key, parameter.Value);
                    }

                    var outputs = powerShell.Invoke();

                    // the return value is the last output from the script.
                    var result = outputs.LastOrDefault();
                    return result == null ? null : result.ImmediateBaseObject as T;
                }
            }
            catch (Exception ex)
            {
                NotificationService.DisplayException(ex, Path.GetFileNameWithoutExtension(script) + " failed");
            }

            return null;
        }
    }
}