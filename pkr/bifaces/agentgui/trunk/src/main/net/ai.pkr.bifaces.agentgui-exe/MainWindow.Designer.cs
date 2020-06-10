/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

namespace ai.pkr.bifaces.agentgui_exe
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageGame = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btn1 = new System.Windows.Forms.RadioButton();
            this.btn0 = new System.Windows.Forms.RadioButton();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblAgentPos = new System.Windows.Forms.Label();
            this.reGameInfo = new System.Windows.Forms.RichTextBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.reUserInput = new ai.pkr.bifaces.agentgui_exe.ActionFiled();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.reGameRecord = new System.Windows.Forms.RichTextBox();
            this.reMessages = new System.Windows.Forms.RichTextBox();
            this.tabPagePlayerInfo = new System.Windows.Forms.TabPage();
            this.rePlayerInfo = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPageGame.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabPagePlayerInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageGame);
            this.tabControl1.Controls.Add(this.tabPagePlayerInfo);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(595, 285);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageGame
            // 
            this.tabPageGame.Controls.Add(this.splitContainer1);
            this.tabPageGame.Location = new System.Drawing.Point(4, 25);
            this.tabPageGame.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageGame.Name = "tabPageGame";
            this.tabPageGame.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageGame.Size = new System.Drawing.Size(587, 256);
            this.tabPageGame.TabIndex = 0;
            this.tabPageGame.Text = "Control";
            this.tabPageGame.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(4, 4);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btn1);
            this.splitContainer1.Panel1.Controls.Add(this.btn0);
            this.splitContainer1.Panel1.Controls.Add(this.btnNext);
            this.splitContainer1.Panel1.Controls.Add(this.lblAgentPos);
            this.splitContainer1.Panel1.Controls.Add(this.reGameInfo);
            this.splitContainer1.Panel1.Controls.Add(this.btnProcess);
            this.splitContainer1.Panel1.Controls.Add(this.reUserInput);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(579, 248);
            this.splitContainer1.SplitterDistance = 100;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // btn1
            // 
            this.btn1.AutoSize = true;
            this.btn1.Location = new System.Drawing.Point(126, 24);
            this.btn1.Margin = new System.Windows.Forms.Padding(4);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(34, 21);
            this.btn1.TabIndex = 1;
            this.btn1.Text = "&1";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn0
            // 
            this.btn0.AutoSize = true;
            this.btn0.Checked = true;
            this.btn0.Location = new System.Drawing.Point(42, 24);
            this.btn0.Margin = new System.Windows.Forms.Padding(4);
            this.btn0.Name = "btn0";
            this.btn0.Size = new System.Drawing.Size(34, 21);
            this.btn0.TabIndex = 0;
            this.btn0.TabStop = true;
            this.btn0.Text = "&0";
            this.btn0.UseVisualStyleBackColor = true;
            this.btn0.Click += new System.EventHandler(this.btn0_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(195, 20);
            this.btnNext.Margin = new System.Windows.Forms.Padding(4);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(71, 28);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "&Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // lblAgentPos
            // 
            this.lblAgentPos.AutoSize = true;
            this.lblAgentPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblAgentPos.Location = new System.Drawing.Point(7, 26);
            this.lblAgentPos.Name = "lblAgentPos";
            this.lblAgentPos.Size = new System.Drawing.Size(18, 17);
            this.lblAgentPos.TabIndex = 3;
            this.lblAgentPos.Text = "A";
            // 
            // reGameInfo
            // 
            this.reGameInfo.Location = new System.Drawing.Point(289, 20);
            this.reGameInfo.Margin = new System.Windows.Forms.Padding(4);
            this.reGameInfo.Name = "reGameInfo";
            this.reGameInfo.ReadOnly = true;
            this.reGameInfo.Size = new System.Drawing.Size(203, 28);
            this.reGameInfo.TabIndex = 3;
            this.reGameInfo.Text = "";
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(392, 61);
            this.btnProcess.Margin = new System.Windows.Forms.Padding(4);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(100, 28);
            this.btnProcess.TabIndex = 6;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // reUserInput
            // 
            this.reUserInput.Location = new System.Drawing.Point(7, 64);
            this.reUserInput.Margin = new System.Windows.Forms.Padding(4);
            this.reUserInput.Multiline = false;
            this.reUserInput.Name = "reUserInput";
            this.reUserInput.Size = new System.Drawing.Size(377, 26);
            this.reUserInput.TabIndex = 5;
            this.reUserInput.Text = "";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.reGameRecord);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.reMessages);
            this.splitContainer2.Size = new System.Drawing.Size(579, 143);
            this.splitContainer2.SplitterDistance = 105;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // reGameRecord
            // 
            this.reGameRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reGameRecord.Location = new System.Drawing.Point(0, 0);
            this.reGameRecord.Margin = new System.Windows.Forms.Padding(4);
            this.reGameRecord.Name = "reGameRecord";
            this.reGameRecord.ReadOnly = true;
            this.reGameRecord.Size = new System.Drawing.Size(579, 105);
            this.reGameRecord.TabIndex = 7;
            this.reGameRecord.Text = "";
            // 
            // reMessages
            // 
            this.reMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reMessages.Location = new System.Drawing.Point(0, 0);
            this.reMessages.Margin = new System.Windows.Forms.Padding(4);
            this.reMessages.Name = "reMessages";
            this.reMessages.ReadOnly = true;
            this.reMessages.Size = new System.Drawing.Size(579, 33);
            this.reMessages.TabIndex = 8;
            this.reMessages.Text = "";
            // 
            // tabPagePlayerInfo
            // 
            this.tabPagePlayerInfo.Controls.Add(this.rePlayerInfo);
            this.tabPagePlayerInfo.Location = new System.Drawing.Point(4, 25);
            this.tabPagePlayerInfo.Margin = new System.Windows.Forms.Padding(4);
            this.tabPagePlayerInfo.Name = "tabPagePlayerInfo";
            this.tabPagePlayerInfo.Padding = new System.Windows.Forms.Padding(4);
            this.tabPagePlayerInfo.Size = new System.Drawing.Size(587, 256);
            this.tabPagePlayerInfo.TabIndex = 1;
            this.tabPagePlayerInfo.Text = "A Info";
            this.tabPagePlayerInfo.UseVisualStyleBackColor = true;
            // 
            // rePlayerInfo
            // 
            this.rePlayerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rePlayerInfo.Location = new System.Drawing.Point(4, 4);
            this.rePlayerInfo.Margin = new System.Windows.Forms.Padding(4);
            this.rePlayerInfo.Name = "rePlayerInfo";
            this.rePlayerInfo.ReadOnly = true;
            this.rePlayerInfo.Size = new System.Drawing.Size(579, 248);
            this.rePlayerInfo.TabIndex = 0;
            this.rePlayerInfo.Text = "";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 285);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainWindow";
            this.Text = "ag";
            this.tabControl1.ResumeLayout(false);
            this.tabPageGame.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.tabPagePlayerInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageGame;
        private System.Windows.Forms.RichTextBox rePlayerInfo;
        private System.Windows.Forms.TabPage tabPagePlayerInfo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox reMessages;
        private System.Windows.Forms.RichTextBox reGameRecord;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox reGameInfo;
        private ActionFiled reUserInput;
        private System.Windows.Forms.RadioButton btn1;
        private System.Windows.Forms.RadioButton btn0;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lblAgentPos;
    }
}

