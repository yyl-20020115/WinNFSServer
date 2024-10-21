namespace WinNFSServer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            statusStrip = new StatusStrip();
            menuStrip = new MenuStrip();
            文件FToolStripMenuItem = new ToolStripMenuItem();
            打开OToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            退出XToolStripMenuItem = new ToolStripMenuItem();
            帮助HToolStripMenuItem = new ToolStripMenuItem();
            关于AToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip
            // 
            resources.ApplyResources(statusStrip, "statusStrip");
            statusStrip.Name = "statusStrip";
            // 
            // menuStrip
            // 
            resources.ApplyResources(menuStrip, "menuStrip");
            menuStrip.Items.AddRange(new ToolStripItem[] { 文件FToolStripMenuItem, 帮助HToolStripMenuItem, helpToolStripMenuItem });
            menuStrip.Name = "menuStrip";
            // 
            // 文件FToolStripMenuItem
            // 
            resources.ApplyResources(文件FToolStripMenuItem, "文件FToolStripMenuItem");
            文件FToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 打开OToolStripMenuItem, toolStripMenuItem1, 退出XToolStripMenuItem });
            文件FToolStripMenuItem.Name = "文件FToolStripMenuItem";
            // 
            // 打开OToolStripMenuItem
            // 
            resources.ApplyResources(打开OToolStripMenuItem, "打开OToolStripMenuItem");
            打开OToolStripMenuItem.Name = "打开OToolStripMenuItem";
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // 退出XToolStripMenuItem
            // 
            resources.ApplyResources(退出XToolStripMenuItem, "退出XToolStripMenuItem");
            退出XToolStripMenuItem.Name = "退出XToolStripMenuItem";
            退出XToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // 帮助HToolStripMenuItem
            // 
            resources.ApplyResources(帮助HToolStripMenuItem, "帮助HToolStripMenuItem");
            帮助HToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 关于AToolStripMenuItem, exitToolStripMenuItem });
            帮助HToolStripMenuItem.Name = "帮助HToolStripMenuItem";
            // 
            // 关于AToolStripMenuItem
            // 
            resources.ApplyResources(关于AToolStripMenuItem, "关于AToolStripMenuItem");
            关于AToolStripMenuItem.Name = "关于AToolStripMenuItem";
            关于AToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            resources.ApplyResources(exitToolStripMenuItem, "exitToolStripMenuItem");
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            // 
            // helpToolStripMenuItem
            // 
            resources.ApplyResources(helpToolStripMenuItem, "helpToolStripMenuItem");
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            // 
            // aboutToolStripMenuItem
            // 
            resources.ApplyResources(aboutToolStripMenuItem, "aboutToolStripMenuItem");
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip;
        private MenuStrip menuStrip;
        private ToolStripMenuItem 文件FToolStripMenuItem;
        private ToolStripMenuItem 打开OToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem 退出XToolStripMenuItem;
        private ToolStripMenuItem 帮助HToolStripMenuItem;
        private ToolStripMenuItem 关于AToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
    }
}
