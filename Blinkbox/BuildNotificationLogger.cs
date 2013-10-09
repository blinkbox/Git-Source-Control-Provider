// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildNotificationLogger.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;

    using Microsoft.Build.Framework;

    /// <summary>
    /// Sends build messages to the VS output window 
    /// </summary>
    public class BuildNotificationLogger : ILogger
    {
        /// <summary>
        /// Inserts a new line and a tab. 
        /// </summary>
        private static readonly string NewLineIndent = Environment.NewLine + "\t";

        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

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
        /// Initializes a new instance of the <see cref="BuildNotificationLogger"/> class.
        /// </summary>
        /// <param name="notifications">The notifications.</param>
        public BuildNotificationLogger(NotificationService notifications)
        {
            this.notificationService = notifications;
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
                this.notificationService.AddMessage(message);
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
            string template = Environment.NewLine + "Error: \"{0}\"" + NewLineIndent + "in file {1} line {2}" + NewLineIndent + "in project {3}" + Environment.NewLine;
            string message = string.Format(template, error.Message, error.File, error.LineNumber, error.ProjectFile);
            this.notificationService.AddMessage(message);
        }

        /// <summary>
        /// Outputs a warning.
        /// </summary>
        /// <param name="warning">
        /// The warning.
        /// </param>
        public void HandleWarning(BuildWarningEventArgs warning)
        {
            string template = "Warning: \"{0}\"" + NewLineIndent + "in file {1} line {2}" + NewLineIndent + "in project {3}";
            string message = string.Format(template, warning.Message, warning.File, warning.LineNumber, warning.ProjectFile);
            this.notificationService.AddMessage(message);
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