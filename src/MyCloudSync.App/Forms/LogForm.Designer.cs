namespace MyCloudSync.App.Forms;

partial class LogForm
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
        gridLog = new DataGridView();
        colTime = new DataGridViewTextBoxColumn();
        colFile = new DataGridViewTextBoxColumn();
        colAction = new DataGridViewTextBoxColumn();
        colResult = new DataGridViewTextBoxColumn();
        cmbFilter = new ComboBox();
        btnRefresh = new Button();
        btnClear = new Button();
        ((System.ComponentModel.ISupportInitialize)gridLog).BeginInit();
        SuspendLayout();
        gridLog.AllowUserToAddRows = false;
        gridLog.AllowUserToDeleteRows = false;
        gridLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLog.BackgroundColor = SystemColors.Window;
        gridLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridLog.Columns.AddRange(new DataGridViewColumn[] { colTime, colFile, colAction, colResult });
        gridLog.ReadOnly = true;
        gridLog.RowHeadersVisible = false;
        gridLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridLog.Location = new Point(20, 20);
        gridLog.Name = "gridLog";
        gridLog.Size = new Size(640, 330);
        colTime.FillWeight = 22F;
        colTime.HeaderText = "Час";
        colTime.Name = "colTime";
        colTime.ReadOnly = true;
        colFile.FillWeight = 40F;
        colFile.HeaderText = "Файл";
        colFile.Name = "colFile";
        colFile.ReadOnly = true;
        colAction.FillWeight = 18F;
        colAction.HeaderText = "Дія";
        colAction.Name = "colAction";
        colAction.ReadOnly = true;
        colResult.FillWeight = 20F;
        colResult.HeaderText = "Результат";
        colResult.Name = "colResult";
        colResult.ReadOnly = true;
        cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilter.Items.AddRange(new object[] { "Усі події", "Лише помилки" });
        cmbFilter.Location = new Point(20, 365);
        cmbFilter.Name = "cmbFilter";
        cmbFilter.Size = new Size(160, 23);
        cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
        btnRefresh.Location = new Point(420, 362);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(100, 30);
        btnRefresh.Text = "Оновити";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;
        btnClear.Location = new Point(530, 362);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(130, 30);
        btnClear.Text = "Очистити журнал";
        btnClear.UseVisualStyleBackColor = true;
        btnClear.Click += btnClear_Click;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(680, 410);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        Text = "Журнал операцій";
        ((System.ComponentModel.ISupportInitialize)gridLog).EndInit();
        Controls.Add(gridLog);
        Controls.Add(cmbFilter);
        Controls.Add(btnRefresh);
        Controls.Add(btnClear);
        ResumeLayout(false);
        PerformLayout();
    }

    private DataGridView gridLog;
    private DataGridViewTextBoxColumn colTime;
    private DataGridViewTextBoxColumn colFile;
    private DataGridViewTextBoxColumn colAction;
    private DataGridViewTextBoxColumn colResult;
    private ComboBox cmbFilter;
    private Button btnRefresh;
    private Button btnClear;
}
