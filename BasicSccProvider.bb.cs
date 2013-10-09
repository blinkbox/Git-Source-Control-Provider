// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicSccProvider.bb.cs" company="blinkbox">
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Xml;
    using System.Xml.Linq;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;
    using GitScc.Blinkbox.UI;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell.Interop;

    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    public partial class BasicSccProvider
    {
        /// <summary>
        /// list of commands which are disabled during review
        /// </summary>
        private static readonly List<uint> DisabledCommandsDuringReview = new List<uint>
        {
            CommandId.BlinkboxDeployId,
            CommandId.GitTfsGetLatestButtonId,
            CommandId.GitTfsReviewButtonId,
            CommandId.icmdPendingChangesAmend,
            CommandId.icmdPendingChangesCommit,
            CommandId.icmdPendingChangesRefresh,
            CommandId.icmdPendingChangesCommitToBranch,
            CommandId.SubmitTestButtonId
        };

        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private NotificationService notificationService;

        /// <summary>
        /// List of git tfs commands available
        /// </summary>
        private List<GitTfsCommand> gitTfsCommands;

        /// <summary>
        /// Instance of the  <see cref="SccHelperService"/>
        /// </summary>
        private SccHelperService sccHelperService;

        /// <summary>
        /// Instance of the  <see cref="DevelopmentService"/>
        /// </summary>
        private DevelopmentService developmentService;

        /// <summary>
        /// Instance of the  <see cref="DevelopmentService"/>
        /// </summary>
        private DeploymentService deploymentService;

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <typeparam name="T">The thye of the service to register.</typeparam>
        /// <param name="instance">The instance of the service.</param>
        public static void RegisterService<T>(T instance)
        {
            if (_SccProvider == null)
            {
                throw new Exception("no instance of BasicSccProvider found");
            }

            ((IServiceContainer)_SccProvider).AddService(typeof(T), instance, false);
        }

        /// <summary>
        /// Launches the provided url in the Visual Studio browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void LaunchBrowser(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string errorMessage;
                try
                {
                    if (UserSettings.Current.OpenUrlsInVS.GetValueOrDefault())
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

                NotificationService.DisplayError("Browser failed", "Cannot launch " + url + ": " + errorMessage);
            }
        }

        /// <summary>
        /// Gets the service of type T.
        /// </summary>
        /// <typeparam name="T">The type of a service</typeparam>
        /// <returns>service of type T.</returns>
        public T GetService<T>()
        {
            return (T)base.GetService(typeof(T));
        }

        /// <summary>
        /// Checks for new version.
        /// </summary>
        public void InstallNewVersion()
        {
            try
            {
                var currentVersion = this.GetCurrentVersion();
                var availableVersion = this.GetAvailableVersion();

                if (currentVersion < availableVersion && availableVersion > UserSettings.Current.LastVersionPrompt)
                {
                    var process = new Process { StartInfo = new ProcessStartInfo(Path.Combine(UserSettings.Current.ReleaseLocation, "BBGitSourceControl.vsix")) };

                    // Install the new version
                    process.Start();
                }
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Install failed", "Please install manually");  
            }
        }

        /// <summary>
        /// Initialises the blinkbox extensions to BasicSccProvider.
        /// </summary>
        private void InitialiseBlinkboxExtensions()
        {
            this.notificationService = new NotificationService();
            RegisterService(this.notificationService);

            this.sccHelperService = new SccHelperService(this.sccService);
            RegisterService(this.sccHelperService);

            this.developmentService = new DevelopmentService(this.sccService, this.notificationService, this.sccHelperService);
            RegisterService(this.developmentService);

            this.deploymentService = new DeploymentService(this);

            this.developmentService.RunAsync(
                () =>
                    { 
                        System.Threading.Thread.Sleep(10000);
                        this.CheckForNewVersion();
                    },
                "Check for new version");
        }

        /// <summary>
        /// Registers commands and services used by the extension.
        /// </summary>
        /// <param name="menuService">The menu service.</param>
        private void RegisterComponents(MsVsShell.OleMenuCommandService menuService)
        {
            this.gitTfsCommands = this.DefineGitTfsCommands();

            if (menuService != null)
            {
                // Register Git Tfs commands with menu service
                foreach (var menuOption in this.gitTfsCommands)
                {
                    var currentMenuOption = menuOption;
                    Action handler = () => currentMenuOption.Handler();
                    this.RegisterCommandWithMenuService(menuService, menuOption.CommandId, (sender, args) => handler());
                }
            }
        }

        /// <summary>
        /// Defines the git TFS commands availalble.
        /// </summary>
        /// <returns>A list of commands</returns>
        private List<GitTfsCommand> DefineGitTfsCommands()
        {
            var commands = new List<GitTfsCommand>();

            commands.Add(new GitTfsCommand
            {
                Name = "Get Latest",
                CommandId = CommandId.GitTfsGetLatestButtonId,
                Handler = this.developmentService.GetLatest
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Review",
                CommandId = CommandId.GitTfsReviewButtonId,
                Handler = this.developmentService.Review
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Cancel Review",
                CommandId = CommandId.GitTfsCancelReviewButtonId,
                Handler = this.developmentService.CancelReview
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Check in",
                CommandId = CommandId.GitTfsCheckinButtonId,
                Handler = this.developmentService.Checkin
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Clean Workspace",
                CommandId = CommandId.GitTfsCleanWorkspacesButtonId,
                Handler = () => SccHelperService.RunGitTfs("cleanup-workspaces")
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Submit Tests",
                CommandId = CommandId.SubmitTestButtonId,
                Handler = () => this.deploymentService.SubmitTests()
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Unit Tests",
                CommandId = CommandId.UnitTestButtonId,
                Handler = () => this.developmentService.RunUnitTests()
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Deploy",
                CommandId = CommandId.BlinkboxDeployId,
                Handler = this.DeployCurrentVersion
            });

            return commands;
        }

        /// <summary>
        /// Registers the command with menu service.
        /// </summary>
        /// <param name="menuService">The menu service.</param>
        /// <param name="commandId">The command id.</param>
        /// <param name="handler">The handler for the command.</param>
        /// <param name="commandSetGuid">The command set GUID.</param>
        private void RegisterCommandWithMenuService(MsVsShell.OleMenuCommandService menuService, int commandId, EventHandler handler, Guid? commandSetGuid = null)
        {
            commandSetGuid = commandSetGuid ?? GuidList.guidSccProviderCmdSet;
            var command = new CommandID(commandSetGuid.Value, commandId);
            var menuCommand = new MenuCommand(handler, command);
            menuService.AddCommand(menuCommand);
        }

        /// <summary>
        /// QueryStatus called for each command when the extension initialises.
        /// </summary>
        /// <param name="commandGroupGuid">The Command group Guid.</param>
        /// <param name="commands">The commands array. Passing the array is necessary, because otherwise command flags do not apply to the original instance. </param>
        /// <param name="commandFlags">The command flags.</param>
        /// <param name="commandText">the command text.</param>
        /// <returns>
        /// integer indicating whether the command is supported.
        /// </returns>
        private int QueryBlinkboxCommandStatus(Guid commandGroupGuid, OLECMD[] commands, OLECMDF commandFlags, IntPtr commandText)
        {
            var handled = false;
            var commandId = commands[0].cmdID;

            handled = this.IsSupported(commandId, ref commandFlags, commandText);
            handled = this.SwitchForReview(commandId, ref commandFlags) || handled;

            if (handled)
            {
                commands[0].cmdf = (uint)commandFlags;
                return VSConstants.S_OK;
            }

            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Determines whether the specified command id is supported.
        /// </summary>
        /// <param name="commandId">The command id.</param>
        /// <param name="commandFlags">The command flags.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>true if the command was handled</returns>
        private bool IsSupported(uint commandId, ref OLECMDF commandFlags, IntPtr commandText)
        {
            var enabled = false;

            // Process Blinkbox Commands
            switch (commandId)
            {
                case CommandId.BlinkboxDeployId:
                    // Enabled if git and a deploy project is available
                    enabled = this.sccService.IsSolutionGitControlled && this.DeployProjectAvailable();
                    this.SetCommandFlag(ref commandFlags, enabled);
                    
                    return true;

                case CommandId.GitTfsCheckinButtonId:
                case CommandId.GitTfsGetLatestButtonId:
                case CommandId.GitTfsCleanWorkspacesButtonId:
                case CommandId.GitTfsReviewButtonId:
                case CommandId.GitTfsCancelReviewButtonId:
                case CommandId.SubmitTestButtonId:
                case CommandId.UnitTestButtonId:
                case CommandId.ToolsMenu:
                case CommandId.ToolsMenuGroup:

                    // Enable controls if git-tfs is available. 
                    enabled = this.sccService.IsSolutionGitTfsControlled() && this.sccService.IsSolutionGitControlled;
                    this.SetCommandFlag(ref commandFlags, enabled);

                    var menuOption = this.gitTfsCommands.FirstOrDefault(x => x.CommandId == commandId);
                    if (menuOption != null)
                    {
                        // If its a menu option set the text. 
                        this.SetOleCmdText(commandText, menuOption.Name);
                    }

                    return true;

                default:
                    // Not handled
                    return false;
            }
        }

        /// <summary>
        /// Enable or disable buttons depending on the dev mode..
        /// </summary>
        /// <param name="commandId">The command id.</param>
        /// <param name="commandFlags">The command flags.</param>
        /// <returns></returns>
        private bool SwitchForReview(uint commandId, ref OLECMDF commandFlags)
        {
            var reviewing = this.developmentService.CurrentMode == DevelopmentService.DevMode.Reviewing;

            // Check whether the command needs to be disabled during review
            if (reviewing && DisabledCommandsDuringReview.Contains(commandId))
            {
                // Not enabled
                commandFlags = commandFlags & ~OLECMDF.OLECMDF_ENABLED;
                return true;
            }

            if (commandId == CommandId.GitTfsCancelReviewButtonId || commandId == CommandId.GitTfsCheckinButtonId)
            {
                // Enabled for review if generally enabled, otherwise disabled.
                commandFlags = reviewing
                    ? commandFlags & OLECMDF.OLECMDF_ENABLED
                    : commandFlags & ~OLECMDF.OLECMDF_ENABLED;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enables the specified command flags.
        /// </summary>
        /// <param name="commandFlags">The command flags.</param>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        private void SetCommandFlag(ref OLECMDF commandFlags, bool enable)
        {
            commandFlags |= OLECMDF.OLECMDF_SUPPORTED;
            commandFlags = enable
                ? commandFlags | OLECMDF.OLECMDF_ENABLED 
                : commandFlags & ~OLECMDF.OLECMDF_ENABLED;
        }

        /// <summary>
        /// Checks whether a deploy project is available.
        /// </summary>
        /// <returns>true if the solution has a deploy project.</returns>
        private bool DeployProjectAvailable()
        {
            if (this.sccService.Active && this.sccService.SolutionOpen && SolutionSettings.Current != null && !string.IsNullOrEmpty(SolutionSettings.Current.DeployProjectLocation))
            {
                var deployProjectPath = SccHelperService.GetAbsolutePath(SolutionSettings.Current.DeployProjectLocation);
                return File.Exists(deployProjectPath);
            }

            return false;
        }

        /// <summary>
        /// Handles the Deploy button. 
        /// </summary>
        private void DeployCurrentVersion()
        {
            try
            {
                // Run the following action asynchronously
                Action action = () =>
                    {
                        var deployment = new Deployment 
                        {
                            BuildLabel = sccHelperService.GetHeadRevisionHash(),
                            Message = sccHelperService.GetLastCommitMessage()
                        };
                        this.deploymentService.RunDeploy(deployment);
                    };

                this.notificationService.ClearMessages();
                new System.Threading.Tasks.Task(action).Start();
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Deploy failed");
            }
        }

        /// <summary>
        /// Gets the available version by checking the install directory.
        /// </summary>
        /// <returns>
        /// The current Version.
        /// </returns>
        private Version GetCurrentVersion()
        {
            // http://blogs.msdn.com/b/visualstudio/archive/2010/02/19/how-vsix-extensions-are-discovered-and-loaded-in-vs-2010.aspx
            var installDirectory = new DirectoryInfo(this.UserLocalDataPath + "\\Extensions\\blinkbox\\BB Git Source Control");
            var version = installDirectory.EnumerateDirectories().Select(d => d.Name).OrderByDescending(x => x).FirstOrDefault();
            if (version != null)
            {
                return new Version(version);
            }

            return new Version();
        }

        /// <summary>
        /// Gets the available version.
        /// </summary>
        /// <returns>
        /// The available System.Version.
        /// </returns>
        private Version GetAvailableVersion()
        {
            var version = new Version();
            var releaseManifest = Path.Combine(UserSettings.Current.ReleaseLocation, "extension.vsixmanifest");

            if (File.Exists(releaseManifest))
            {
                var doc = XDocument.Load(releaseManifest);

                if (doc.Root == null)
                {
                    return version;
                }

                var xmlns = doc.Root.Name.Namespace;

                var versionElement = doc.Elements(XName.Get("Vsix", xmlns.NamespaceName))
                    .Elements(XName.Get("Identifier", xmlns.NamespaceName))
                    .Elements(XName.Get("Version", xmlns.NamespaceName)).FirstOrDefault();

                if (versionElement != null)
                {
                    return new Version(versionElement.Value);
                }
            }

            return new Version();
        }

        /// <summary>
        /// Checks for new version.
        /// </summary>
        private void CheckForNewVersion()
        {
            try
            {
                var currentVersion = this.GetCurrentVersion();
                var availableVersion = this.GetAvailableVersion();

                var settingsTab = this.GetService<SettingsTab>();
                if (settingsTab != null)
                {
                    Action action = () =>
                        {
                            settingsTab.CurrentVersionText = "Current Version: " + currentVersion;

                            if (currentVersion < availableVersion)
                            {
                                settingsTab.InstallText = "Install version " + availableVersion;
                                settingsTab.Focus();
                            }

                            settingsTab.RefreshBindings();
                        };

                    settingsTab.Dispatcher.Invoke(action);
                }
            }
            catch (Exception e)
            {
                // No Action   
            }
        }
    }
}