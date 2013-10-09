// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxOptionsControl.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// <summary>
//   Summary description for SccProviderOptionsControl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Generates the UI for the Blinkbox Options Panel.
    /// </summary>
    public class BlinkboxOptionsControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Text template for the CommitGuidPropertyNameLabel
        /// </summary>
        private const string CommitGuidPropertyNameLabelText = "Name of the property in {projectName} which accepts the recent commit id";

        /// <summary>
        /// Text template for the CommitCommentPropertyNameLabel
        /// </summary>
        private const string CommitCommentPropertyNameLabelText = "Name of the property in {projectName} which acccepts the recent commit comment";

        /// <summary>
        /// Text template for the UrlToLaunchPropertyNameLabel
        /// </summary>
        private const string UrlToLaunchPropertyNameLabelText = "Name of the property in {projectName} which returns the url to launch after deployment";

        /// <summary>
        /// UI components.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// UI Component
        /// </summary>
        private OpenFileDialog openFileDialog;

        /// <summary>
        /// UI Component
        /// </summary>
        private Label gitTfsLabel;

        /// <summary>
        /// UI Component
        /// </summary>
        private TextBox gitTfsTextBox;

        /// <summary>
        /// UI Component
        /// </summary>
        private Button gitTfsBrowseButton;

        /// <summary>
        /// UI Component
        /// </summary>
        private CheckBox launchInVSCheckbox;

        private TextBox postCommitDeployProjectNameTextbox;
        private Label postCommitDeployProjectNameLabel;
        private TextBox urlToLaunchPropertyNameTextBox;
        private Label urlToLaunchPropertyNameLabel;
        private TextBox commitCommentPropertyNameTextBox;
        private Label commitCommentPropertyNameLabel;
        private TextBox commitGuidPropertyNameTextBox;
        private Label commitGuidPropertyNameLabel;

        /// <summary>
        /// The parent page
        /// </summary>
        private BlinkboxOptionsPage parentPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlinkboxOptionsControl"/> class.
        /// </summary>
        public BlinkboxOptionsControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }


        #region Component Designer generated code
       
        #endregion

        /// <summary>
        /// Sets OptionsPage.
        /// </summary>
        public BlinkboxOptionsPage OptionsPage
        {
            set
            {
                this.parentPage = value;
            }
        }

        /// <summary>
        /// Saves options.
        /// </summary>
        internal void Save()
        {
            BlinkboxSccOptions.Current.GitTfsPath = this.gitTfsTextBox.Text;
            BlinkboxSccOptions.Current.LaunchDeployedUrlsInVS = this.launchInVSCheckbox.Checked;
            BlinkboxSccOptions.Current.CommitCommentPropertyName = this.commitCommentPropertyNameTextBox.Text;
            BlinkboxSccOptions.Current.CommitGuidPropertyName = this.commitGuidPropertyNameTextBox.Text;
            BlinkboxSccOptions.Current.UrlToLaunchPropertyName = this.urlToLaunchPropertyNameTextBox.Text;
            BlinkboxSccOptions.Current.PostCommitDeployProjectName = this.postCommitDeployProjectNameTextbox.Text;

            BlinkboxSccOptions.Current.Save();

            var sccProviderService = (SccProviderService)GetService(typeof(SccProviderService));
            if (sccProviderService != null)
            {
                sccProviderService.Refresh();
            }
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }

                GC.SuppressFinalize(this);
            }

            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.gitTfsLabel = new System.Windows.Forms.Label();
            this.gitTfsTextBox = new System.Windows.Forms.TextBox();
            this.gitTfsBrowseButton = new System.Windows.Forms.Button();
            this.launchInVSCheckbox = new System.Windows.Forms.CheckBox();
            this.postCommitDeployProjectNameTextbox = new System.Windows.Forms.TextBox();
            this.postCommitDeployProjectNameLabel = new System.Windows.Forms.Label();
            this.urlToLaunchPropertyNameTextBox = new System.Windows.Forms.TextBox();
            this.urlToLaunchPropertyNameLabel = new System.Windows.Forms.Label();
            this.commitCommentPropertyNameTextBox = new System.Windows.Forms.TextBox();
            this.commitCommentPropertyNameLabel = new System.Windows.Forms.Label();
            this.commitGuidPropertyNameTextBox = new System.Windows.Forms.TextBox();
            this.commitGuidPropertyNameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // gitTfsLabel
            // 
            this.gitTfsLabel.AutoSize = true;
            this.gitTfsLabel.Location = new System.Drawing.Point(3, 10);
            this.gitTfsLabel.Name = "gitTfsLabel";
            this.gitTfsLabel.Size = new System.Drawing.Size(80, 13);
            this.gitTfsLabel.TabIndex = 11;
            this.gitTfsLabel.Text = "Path to Git TFS";
            // 
            // gitTfsTextBox
            // 
            this.gitTfsTextBox.Location = new System.Drawing.Point(6, 26);
            this.gitTfsTextBox.Name = "gitTfsTextBox";
            this.gitTfsTextBox.Size = new System.Drawing.Size(283, 20);
            this.gitTfsTextBox.TabIndex = 12;
            // 
            // gitTfsBrowseButton
            // 
            this.gitTfsBrowseButton.Location = new System.Drawing.Point(295, 23);
            this.gitTfsBrowseButton.Name = "gitTfsBrowseButton";
            this.gitTfsBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.gitTfsBrowseButton.TabIndex = 17;
            this.gitTfsBrowseButton.Text = "Browse ...";
            this.gitTfsBrowseButton.UseVisualStyleBackColor = true;
            this.gitTfsBrowseButton.Click += new System.EventHandler(this.GitTfsPathBrowse_Click);
            // 
            // launchInVSCheckbox
            // 
            this.launchInVSCheckbox.AutoSize = true;
            this.launchInVSCheckbox.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.launchInVSCheckbox.Location = new System.Drawing.Point(6, 68);
            this.launchInVSCheckbox.Name = "launchInVSCheckbox";
            this.launchInVSCheckbox.Size = new System.Drawing.Size(227, 17);
            this.launchInVSCheckbox.TabIndex = 25;
            this.launchInVSCheckbox.Text = "Launch deployed websites in Visual Studio";
            this.launchInVSCheckbox.UseVisualStyleBackColor = true;
            // 
            // postCommitDeployProjectNameTextbox
            // 
            this.postCommitDeployProjectNameTextbox.Location = new System.Drawing.Point(6, 121);
            this.postCommitDeployProjectNameTextbox.Name = "postCommitDeployProjectNameTextbox";
            this.postCommitDeployProjectNameTextbox.Size = new System.Drawing.Size(283, 20);
            this.postCommitDeployProjectNameTextbox.TabIndex = 27;
            this.postCommitDeployProjectNameTextbox.TextChanged += new System.EventHandler(this.PostCommitDeployProjectNameTextbox_TextChanged);
            // 
            // postCommitDeployProjectNameLabel
            // 
            this.postCommitDeployProjectNameLabel.AutoSize = true;
            this.postCommitDeployProjectNameLabel.Location = new System.Drawing.Point(3, 105);
            this.postCommitDeployProjectNameLabel.Name = "postCommitDeployProjectNameLabel";
            this.postCommitDeployProjectNameLabel.Size = new System.Drawing.Size(428, 13);
            this.postCommitDeployProjectNameLabel.TabIndex = 26;
            this.postCommitDeployProjectNameLabel.Text = "Name of the postCommitDeploy project, called after the commit during Commit and D" +
    "eploy";
            // 
            // urlToLaunchPropertyNameTextBox
            // 
            this.urlToLaunchPropertyNameTextBox.Location = new System.Drawing.Point(6, 160);
            this.urlToLaunchPropertyNameTextBox.Name = "urlToLaunchPropertyNameTextBox";
            this.urlToLaunchPropertyNameTextBox.Size = new System.Drawing.Size(283, 20);
            this.urlToLaunchPropertyNameTextBox.TabIndex = 29;
            // 
            // urlToLaunchPropertyNameLabel
            // 
            this.urlToLaunchPropertyNameLabel.AutoSize = true;
            this.urlToLaunchPropertyNameLabel.Enabled = false;
            this.urlToLaunchPropertyNameLabel.Location = new System.Drawing.Point(3, 144);
            this.urlToLaunchPropertyNameLabel.Name = "urlToLaunchPropertyNameLabel";
            this.urlToLaunchPropertyNameLabel.Size = new System.Drawing.Size(414, 13);
            this.urlToLaunchPropertyNameLabel.TabIndex = 28;
            this.urlToLaunchPropertyNameLabel.Text = UrlToLaunchPropertyNameLabelText;
            this.urlToLaunchPropertyNameLabel.Tag = UrlToLaunchPropertyNameLabelText;
            // 
            // commitCommentPropertyNameTextBox
            // 
            this.commitCommentPropertyNameTextBox.Location = new System.Drawing.Point(6, 238);
            this.commitCommentPropertyNameTextBox.Name = "commitCommentPropertyNameTextBox";
            this.commitCommentPropertyNameTextBox.Size = new System.Drawing.Size(283, 20);
            this.commitCommentPropertyNameTextBox.TabIndex = 31;
            // 
            // commitCommentPropertyNameLabel
            // 
            this.commitCommentPropertyNameLabel.AutoSize = true;
            this.commitCommentPropertyNameLabel.Enabled = false;
            this.commitCommentPropertyNameLabel.Location = new System.Drawing.Point(3, 222);
            this.commitCommentPropertyNameLabel.Name = "commitCommentPropertyNameLabel";
            this.commitCommentPropertyNameLabel.Size = new System.Drawing.Size(399, 13);
            this.commitCommentPropertyNameLabel.TabIndex = 30;
            this.commitCommentPropertyNameLabel.Text = CommitCommentPropertyNameLabelText;
            // 
            // commitGuidPropertyNameTextBox
            // 
            this.commitGuidPropertyNameTextBox.Location = new System.Drawing.Point(6, 199);
            this.commitGuidPropertyNameTextBox.Name = "commitGuidPropertyNameTextBox";
            this.commitGuidPropertyNameTextBox.Size = new System.Drawing.Size(283, 20);
            this.commitGuidPropertyNameTextBox.TabIndex = 33;
            // 
            // commitGuidPropertyNameLabel
            // 
            this.commitGuidPropertyNameLabel.AutoSize = true;
            this.commitGuidPropertyNameLabel.Enabled = false;
            this.commitGuidPropertyNameLabel.Location = new System.Drawing.Point(3, 183);
            this.commitGuidPropertyNameLabel.Name = "commitGuidPropertyNameLabel";
            this.commitGuidPropertyNameLabel.Size = new System.Drawing.Size(358, 13);
            this.commitGuidPropertyNameLabel.TabIndex = 32;
            this.commitGuidPropertyNameLabel.Text = CommitGuidPropertyNameLabelText;
            // 
            // BlinkboxOptionsControl
            // 
            this.AllowDrop = true;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.commitGuidPropertyNameTextBox);
            this.Controls.Add(this.commitGuidPropertyNameLabel);
            this.Controls.Add(this.commitCommentPropertyNameTextBox);
            this.Controls.Add(this.commitCommentPropertyNameLabel);
            this.Controls.Add(this.urlToLaunchPropertyNameTextBox);
            this.Controls.Add(this.urlToLaunchPropertyNameLabel);
            this.Controls.Add(this.postCommitDeployProjectNameTextbox);
            this.Controls.Add(this.postCommitDeployProjectNameLabel);
            this.Controls.Add(this.launchInVSCheckbox);
            this.Controls.Add(this.gitTfsBrowseButton);
            this.Controls.Add(this.gitTfsTextBox);
            this.Controls.Add(this.gitTfsLabel);
            this.Name = "BlinkboxOptionsControl";
            this.Size = new System.Drawing.Size(1559, 604);
            this.Load += new System.EventHandler(this.SccProviderOptionsControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        /// <summary>
        /// Page Load Event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SccProviderOptionsControl_Load(object sender, EventArgs e)
        {
            this.gitTfsTextBox.Text = BlinkboxSccOptions.Current.GitTfsPath;
            this.commitCommentPropertyNameTextBox.Text = BlinkboxSccOptions.Current.CommitCommentPropertyName;
            this.commitGuidPropertyNameTextBox.Text = BlinkboxSccOptions.Current.CommitGuidPropertyName;
            this.urlToLaunchPropertyNameTextBox.Text = BlinkboxSccOptions.Current.UrlToLaunchPropertyName;
            this.postCommitDeployProjectNameTextbox.Text = BlinkboxSccOptions.Current.PostCommitDeployProjectName;
            PostCommitDeployProjectNameTextbox_TextChanged(null, null);

        }

        /// <summary>
        /// Handles the Click event of the gitTfsPathBrowse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GitTfsPathBrowse_Click(object sender, EventArgs e)
        {
            this.OpenFile("git-tfs.exe", this.gitTfsTextBox);
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="shexe">The shexe.</param>
        /// <param name="textBox">The text box.</param>
        private void OpenFile(string shexe, TextBox textBox)
        {
            this.openFileDialog.FileName = shexe;
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = this.openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Update labels with the name of the postCommitDeploy project.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PostCommitDeployProjectNameTextbox_TextChanged(object sender, EventArgs e)
        {
            this.commitCommentPropertyNameLabel.Text = CommitCommentPropertyNameLabelText.Replace("{projectName}", this.postCommitDeployProjectNameTextbox.Text);
            this.commitGuidPropertyNameLabel.Text = CommitGuidPropertyNameLabelText.Replace("{projectName}", this.postCommitDeployProjectNameTextbox.Text);
            this.urlToLaunchPropertyNameLabel.Text = UrlToLaunchPropertyNameLabelText.Replace("{projectName}", this.postCommitDeployProjectNameTextbox.Text);
        }
    }
}
