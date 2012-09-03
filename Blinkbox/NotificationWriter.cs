// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationWriter.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// This class is repsonsible for writing messages into Visual Studio Output Window.
    /// </summary>
    public class NotificationWriter
    {
        /// <summary>
        /// Lock for singleton initialisation.
        /// </summary>
        private static readonly object sync = new object();

        /// <summary>
        /// A queue of the messages. 
        /// </summary>
        private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        /// <summary>
        /// A thread for writing messages to the output window. 
        /// </summary>
        private readonly Thread processingThread;

        /// <summary>
        /// The singleton instance of the NotificationWriter.
        /// </summary>
        private static NotificationWriter instance = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationWriter"/> class. 
        /// </summary>
        public NotificationWriter()
        {
            this.processingThread = new Thread(this.ProcessMessages);
            this.processingThread.Start();
        }

        /// <summary>
        /// Gets a singleton Instance.
        /// </summary>
        private static NotificationWriter Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        instance = instance ?? new NotificationWriter();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Writes a message into our output pane.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public static void Write(string message)
        {
            Instance.messages.Enqueue(message);
        }

        /// <summary>
        /// Writes a new section to the output window.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void NewSection(string name)
        {
            Write(Environment.NewLine + "#### " + name + " ############################################");
        }

        /// <summary>
        /// Clears the output pane. 
        /// </summary>
        public static void Clear()
        {
            PendingChangesView.ClearDiffEditor();
        }

        /// <summary>
        /// Periodically checks for messages and writes them to the output window. 
        /// </summary>
        private void ProcessMessages()
        {
            while (true)
            {
                this.WriteMessageQueue();
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Writes all mesages in the queue to the output window. 
        /// </summary>
        private void WriteMessageQueue()
        {
            while (!this.messages.IsEmpty)
            {
                string message = null;
                this.messages.TryDequeue(out message);
                if (!string.IsNullOrEmpty(message))
                {
                    PendingChangesView.WriteToDiffWindow(message);
                }
            }
        }
    }
}