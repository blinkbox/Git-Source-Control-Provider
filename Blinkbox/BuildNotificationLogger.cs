// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildNotificationLogger.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Diagnostics;

    using Microsoft.Build.Framework;

    /// <summary>
    /// Sends build messages to the VS output window 
    /// </summary>
    public class BuildNotificationLogger : ILogger
    {
        /// <summary>
        /// Inserts a new line and a tab. 
        /// </summary>
        private static readonly string newLineIndent = Environment.NewLine + "\t";

        /// <summary>
        /// Gets or sets Verbosity.
        /// </summary>
        public LoggerVerbosity Verbosity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Parameters.
        /// </summary>
        public string Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Initialises the logger.
        /// </summary>
        /// <param name="buildEventSource">
        /// The event source.
        /// </param>
        public void Initialize(IEventSource buildEventSource)
        {
            buildEventSource.TargetFinished += (sender, args) => this.HandleMessage(args.Message, MessageImportance.Low);
            buildEventSource.MessageRaised += (sender, args) => this.HandleMessage(args.Message, args.Importance);
            buildEventSource.ErrorRaised += (sender, args) => this.HandleError(args);
            buildEventSource.WarningRaised += (sender, args) => this.HandleWarning(args);
            buildEventSource.BuildStarted += (sender, args) => this.HandleMessage(args.Message, MessageImportance.High);
            buildEventSource.BuildFinished += (sender, args) => this.HandleMessage(args.Message, MessageImportance.High);
        }

        /// <summary>
        /// Handle a raised message. 
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="importance">
        /// The importance.
        /// </param>
        public void HandleMessage(string message, MessageImportance importance = MessageImportance.Normal)
        {
            if (this.OutputMessage(importance))
            {
                NotificationWriter.Write(message);
            }
        }

        /// <summary>
        /// Outputs an error.
        /// </summary>
        /// <param name="error">
        /// The error.
        /// </param>
        public void HandleError(BuildErrorEventArgs error)
        {
            string template = Environment.NewLine + "Error: \"{0}\"" + newLineIndent + "in file {1} line {2}" + newLineIndent + "in project {3}" + Environment.NewLine;
            string message = string.Format(template, error.Message, error.File, error.LineNumber, error.ProjectFile);
            NotificationWriter.Write(message);
        }

        /// <summary>
        /// Outputs a warning.
        /// </summary>
        /// <param name="warning">
        /// The warning.
        /// </param>
        public void HandleWarning(BuildWarningEventArgs warning)
        {
            string template = "Warning: \"{0}\"" + newLineIndent + "in file {1} line {2}" + newLineIndent + "in project {3}";
            string message = string.Format(template, warning.Message, warning.File, warning.LineNumber, warning.ProjectFile);
            NotificationWriter.Write(message);
        }

        /// <summary>
        /// Cleans up.
        /// </summary>
        public void Shutdown()
        {
        }

        /// <summary>
        /// Decides whether a message shuld be output depending on the verbosity of the logger.
        /// </summary>
        /// <param name="importance">
        /// The importance.
        /// </param>
        /// <returns>
        /// true if the message should be output. 
        /// </returns>
        private bool OutputMessage(MessageImportance importance)
        {
            switch (importance)
            {
                case MessageImportance.High:
                    return this.Verbosity > LoggerVerbosity.Quiet;

                case MessageImportance.Normal:
                    return this.Verbosity >= LoggerVerbosity.Normal;

                case MessageImportance.Low:
                    return this.Verbosity > LoggerVerbosity.Normal;

                default:
                    return false;
            }
        }
    }
}