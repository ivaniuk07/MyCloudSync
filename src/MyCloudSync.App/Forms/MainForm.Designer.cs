namespace MyCloudSync.App.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblStatus = new Label();
        lblAccount = new Label();
        btnSignIn = new Button();
        grpFolders = new GroupBox();
        lblLocalCaption = new Label();
        lblLocalPath = new Label();
        lblDriveCaption = new Label();
        lblDriveFolder = new Label();
        progressSync = new ProgressBar();
        btnSyncNow = new Button();
        btnPause = new Button();
        btnSettings = new Button();
        btnLog = new Button();
        grpFolders.SuspendLayout();
        SuspendLayout();
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Стан синхронізації";
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblStatus.Location = new Point(22, 52);
        lblStatus.Name = "lblStatus";
        lblStatus.Text = "Не підключено";
        lblAccount.AutoSize = true;
        lblAccount.Location = new Point(22, 88);
        lblAccount.Name = "lblAccount";
        lblAccount.Text = "Акаунт: не підключено";
        btnSignIn.Location = new Point(330, 82);
        btnSignIn.Name = "btnSignIn";
        btnSignIn.Size = new Size(200, 30);
        btnSignIn.Text = "Підключити Google Drive";
        btnSignIn.UseVisualStyleBackColor = true;
        btnSignIn.Click += btnSignIn_Click;
        grpFolders.Controls.Add(lblLocalCaption);
        grpFolders.Controls.Add(lblLocalPath);
        grpFolders.Controls.Add(lblDriveCaption);
        grpFolders.Controls.Add(lblDriveFolder);
        grpFolders.Location = new Point(20, 125);
        grpFolders.Name = "grpFolders";
        grpFolders.Size = new Size(510, 110);
        grpFolders.TabStop = false;
        grpFolders.Text = "Папки для синхронізації";
        lblLocalCaption.AutoSize = true;
        lblLocalCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLocalCaption.Location = new Point(15, 28);
        lblLocalCaption.Name = "lblLocalCaption";
        lblLocalCaption.Text = "Цей комп'ютер";
        lblLocalPath.AutoSize = false;
        lblLocalPath.Location = new Point(15, 50);
        lblLocalPath.Name = "lblLocalPath";
        lblLocalPath.Size = new Size(230, 40);
        lblLocalPath.Text = "—";
        lblDriveCaption.AutoSize = true;
        lblDriveCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblDriveCaption.Location = new Point(265, 28);
        lblDriveCaption.Name = "lblDriveCaption";
        lblDriveCaption.Text = "Google Drive";
        lblDriveFolder.AutoSize = false;
        lblDriveFolder.Location = new Point(265, 50);
        lblDriveFolder.Name = "lblDriveFolder";
        lblDriveFolder.Size = new Size(230, 40);
        lblDriveFolder.Text = "—";
        progressSync.Location = new Point(20, 250);
        progressSync.Name = "progressSync";
        progressSync.Size = new Size(510, 18);
        btnSyncNow.Location = new Point(20, 285);
        btnSyncNow.Name = "btnSyncNow";
        btnSyncNow.Size = new Size(170, 34);
        btnSyncNow.Text = "Синхронізувати зараз";
        btnSyncNow.UseVisualStyleBackColor = true;
        btnSyncNow.Click += btnSyncNow_Click;
        btnPause.Location = new Point(200, 285);
        btnPause.Name = "btnPause";
        btnPause.Size = new Size(100, 34);
        btnPause.Text = "Пауза";
        btnPause.UseVisualStyleBackColor = true;
        btnPause.Click += btnPause_Click;
        btnSettings.Location = new Point(310, 285);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(120, 34);
        btnSettings.Text = "Налаштування";
        btnSettings.UseVisualStyleBackColor = true;
        btnSettings.Click += btnSettings_Click;
        btnLog.Location = new Point(440, 285);
        btnLog.Name = "btnLog";
        btnLog.Size = new Size(90, 34);
        btnLog.Text = "Журнал";
        btnLog.UseVisualStyleBackColor = true;
        btnLog.Click += btnLog_Click;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(550, 340);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MyCloudSync";
        grpFolders.ResumeLayout(false);
        grpFolders.PerformLayout();
        Controls.Add(lblTitle);
        Controls.Add(lblStatus);
        Controls.Add(lblAccount);
        Controls.Add(btnSignIn);
        Controls.Add(grpFolders);
        Controls.Add(progressSync);
        Controls.Add(btnSyncNow);
        Controls.Add(btnPause);
        Controls.Add(btnSettings);
        Controls.Add(btnLog);
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblTitle;
    private Label lblStatus;
    private Label lblAccount;
    private Button btnSignIn;
    private GroupBox grpFolders;
    private Label lblLocalCaption;
    private Label lblLocalPath;
    private Label lblDriveCaption;
    private Label lblDriveFolder;
    private ProgressBar progressSync;
    private Button btnSyncNow;
    private Button btnPause;
    private Button btnSettings;
    private Button btnLog;
}
