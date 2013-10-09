// -----------------------------------------------------------------------
// <copyright file="PowershellHost.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Powershell
{
    using System;
    using System.Management.Automation.Host;

    /// <summary>
    /// Implements a host for running powershell scripts (required when write-host is used in the script). 
    /// </summary>
    public class PowershellHost : PSHost
    {
        /// <summary>
        /// The host id.
        /// </summary>
        private readonly Guid hostId = Guid.NewGuid();

        /// <summary>
        /// The ui.
        /// </summary>
        private readonly PowershellHostUserInterface ui = new PowershellHostUserInterface();

        /// <summary>
        /// Gets the current culture.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentCulture; }
        }

        /// <summary>
        /// Gets the current ui culture.
        /// </summary>
        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture; }
        }

        /// <summary>
        /// Gets the instance id.
        /// </summary>
        public override Guid InstanceId
        {
            get
            {
                return this.hostId;
            }
        }

        /// <summary>
        /// Gets the ui.
        /// </summary>
        public override PSHostUserInterface UI
        {
            get
            {
                return this.ui;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get { return "powershellHost"; }
        }

        /// <summary>
        /// The enter nested prompt.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The exit nested prompt.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The notify begin application.
        /// </summary>
        public override void NotifyBeginApplication()
        {
        }

        /// <summary>
        /// The notify end application.
        /// </summary>
        public override void NotifyEndApplication()
        {
        }

        /// <summary>
        /// The set should exit.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code.
        /// </param>
        public override void SetShouldExit(int exitCode)
        {
        }
    }
}
