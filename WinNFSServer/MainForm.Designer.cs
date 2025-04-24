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
            label1 = new Label();
            label2 = new Label();
            textBoxUID = new TextBox();
            textBoxGID = new TextBox();
            textBoxIP = new TextBox();
            label3 = new Label();
            label4 = new Label();
            textBoxLocalDirectory = new TextBox();
            label5 = new Label();
            textBoxExportDirectory = new TextBox();
            textBoxMountPort = new TextBox();
            label6 = new Label();
            checkBoxLog = new CheckBox();
            label7 = new Label();
            textBoxRPC = new TextBox();
            label8 = new Label();
            label9 = new Label();
            textBoxLockPort = new TextBox();
            textBoxStatPort = new TextBox();
            buttonConfirm = new Button();
            buttonCancel = new Button();
            buttonStart = new Button();
            buttonStop = new Button();
            SuspendLayout();
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(24, 24);
            resources.ApplyResources(statusStrip, "statusStrip");
            statusStrip.Name = "statusStrip";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // textBoxUID
            // 
            resources.ApplyResources(textBoxUID, "textBoxUID");
            textBoxUID.Name = "textBoxUID";
            // 
            // textBoxGID
            // 
            resources.ApplyResources(textBoxGID, "textBoxGID");
            textBoxGID.Name = "textBoxGID";
            // 
            // textBoxIP
            // 
            resources.ApplyResources(textBoxIP, "textBoxIP");
            textBoxIP.Name = "textBoxIP";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            // 
            // textBoxLocalDirectory
            // 
            resources.ApplyResources(textBoxLocalDirectory, "textBoxLocalDirectory");
            textBoxLocalDirectory.Name = "textBoxLocalDirectory";
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // textBoxExportDirectory
            // 
            resources.ApplyResources(textBoxExportDirectory, "textBoxExportDirectory");
            textBoxExportDirectory.Name = "textBoxExportDirectory";
            // 
            // textBoxMountPort
            // 
            resources.ApplyResources(textBoxMountPort, "textBoxMountPort");
            textBoxMountPort.Name = "textBoxMountPort";
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            // 
            // checkBoxLog
            // 
            resources.ApplyResources(checkBoxLog, "checkBoxLog");
            checkBoxLog.Name = "checkBoxLog";
            checkBoxLog.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            resources.ApplyResources(label7, "label7");
            label7.Name = "label7";
            // 
            // textBoxRPC
            // 
            resources.ApplyResources(textBoxRPC, "textBoxRPC");
            textBoxRPC.Name = "textBoxRPC";
            // 
            // label8
            // 
            resources.ApplyResources(label8, "label8");
            label8.Name = "label8";
            // 
            // label9
            // 
            resources.ApplyResources(label9, "label9");
            label9.Name = "label9";
            // 
            // textBoxLockPort
            // 
            resources.ApplyResources(textBoxLockPort, "textBoxLockPort");
            textBoxLockPort.Name = "textBoxLockPort";
            // 
            // textBoxStatPort
            // 
            resources.ApplyResources(textBoxStatPort, "textBoxStatPort");
            textBoxStatPort.Name = "textBoxStatPort";
            // 
            // buttonConfirm
            // 
            resources.ApplyResources(buttonConfirm, "buttonConfirm");
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.UseVisualStyleBackColor = true;
            buttonConfirm.Click += ButtonConfirm_Click;
            // 
            // buttonCancel
            // 
            resources.ApplyResources(buttonCancel, "buttonCancel");
            buttonCancel.Name = "buttonCancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // buttonStart
            // 
            resources.ApplyResources(buttonStart, "buttonStart");
            buttonStart.Name = "buttonStart";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += ButtonStart_Click;
            // 
            // buttonStop
            // 
            resources.ApplyResources(buttonStop, "buttonStop");
            buttonStop.Name = "buttonStop";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += ButtonStop_Click;
            // 
            // MainForm
            // 
            AcceptButton = buttonConfirm;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Controls.Add(buttonCancel);
            Controls.Add(buttonConfirm);
            Controls.Add(textBoxStatPort);
            Controls.Add(textBoxLockPort);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(textBoxRPC);
            Controls.Add(label7);
            Controls.Add(checkBoxLog);
            Controls.Add(label6);
            Controls.Add(textBoxMountPort);
            Controls.Add(textBoxExportDirectory);
            Controls.Add(label5);
            Controls.Add(textBoxLocalDirectory);
            Controls.Add(label4);
            Controls.Add(textBoxIP);
            Controls.Add(label3);
            Controls.Add(textBoxGID);
            Controls.Add(textBoxUID);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(statusStrip);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainForm";
            TopMost = true;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip;
        private Label label1;
        private Label label2;
        private TextBox textBoxUID;
        private TextBox textBoxGID;
        private TextBox textBoxIP;
        private Label label3;
        private Label label4;
        private TextBox textBoxLocalDirectory;
        private Label label5;
        private TextBox textBoxExportDirectory;
        private TextBox textBoxMountPort;
        private Label label6;
        private CheckBox checkBoxLog;
        private Label label7;
        private TextBox textBoxRPC;
        private Label label8;
        private Label label9;
        private TextBox textBoxLockPort;
        private TextBox textBoxStatPort;
        private Button buttonConfirm;
        private Button buttonCancel;
        private Button buttonStart;
        private Button buttonStop;
    }
}
