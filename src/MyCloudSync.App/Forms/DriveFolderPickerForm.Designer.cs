namespace MyCloudSync.App.Forms;

partial class DriveFolderPickerForm
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
        treeFolders = new TreeView();
        btnOk = new Button();
        btnCancel = new Button();
        txtNewFolder = new TextBox();
        btnNewFolder = new Button();
        SuspendLayout();
        treeFolders.Location = new Point(20, 20);
        treeFolders.Name = "treeFolders";
        treeFolders.Size = new Size(360, 255);
        treeFolders.HideSelection = false;
        treeFolders.BeforeExpand += treeFolders_BeforeExpand;
        txtNewFolder.Location = new Point(20, 290);
        txtNewFolder.Name = "txtNewFolder";
        txtNewFolder.PlaceholderText = "Назва нової папки";
        txtNewFolder.Size = new Size(230, 23);
        btnNewFolder.Location = new Point(260, 288);
        btnNewFolder.Name = "btnNewFolder";
        btnNewFolder.Size = new Size(120, 27);
        btnNewFolder.Text = "Створити папку";
        btnNewFolder.UseVisualStyleBackColor = true;
        btnNewFolder.Click += btnNewFolder_Click;
        btnOk.Location = new Point(170, 335);
        btnOk.Name = "btnOk";
        btnOk.Size = new Size(100, 30);
        btnOk.Text = "Вибрати";
        btnOk.UseVisualStyleBackColor = true;
        btnOk.Click += btnOk_Click;
        btnCancel.Location = new Point(280, 335);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(100, 30);
        btnCancel.Text = "Скасувати";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.DialogResult = DialogResult.Cancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 380);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        CancelButton = btnCancel;
        Text = "Папка Google Drive";
        Controls.Add(treeFolders);
        Controls.Add(txtNewFolder);
        Controls.Add(btnNewFolder);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        ResumeLayout(false);
        PerformLayout();
    }

    private TreeView treeFolders;
    private Button btnOk;
    private Button btnCancel;
    private TextBox txtNewFolder;
    private Button btnNewFolder;
}
