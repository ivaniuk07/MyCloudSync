using MyCloudSync.App.Controls;

namespace MyCloudSync.App.Pages;

partial class LogPage
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
        var headerStyle = new DataGridViewCellStyle();
        var cellStyle = new DataGridViewCellStyle();
        lblTitle = new Label();
        gridLog = new DataGridView();
        colTime = new DataGridViewTextBoxColumn();
        colFile = new DataGridViewTextBoxColumn();
        colAction = new DataGridViewTextBoxColumn();
        colResult = new DataGridViewTextBoxColumn();
        cmbFilter = new ComboBox();
        btnClear = new Button();
        btnRefresh = new Button();
        SuspendLayout();
        headerStyle.BackColor = Color.FromArgb(232, 239, 252);
        headerStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        headerStyle.ForeColor = Color.FromArgb(20, 38, 61);
        headerStyle.SelectionBackColor = Color.FromArgb(232, 239, 252);
        headerStyle.SelectionForeColor = Color.FromArgb(20, 38, 61);
        headerStyle.Padding = new Padding(8, 0, 0, 0);
        cellStyle.BackColor = Color.White;
        cellStyle.Font = new Font("Segoe UI", 9.5F);
        cellStyle.ForeColor = Color.FromArgb(20, 38, 61);
        cellStyle.SelectionBackColor = Color.FromArgb(232, 239, 252);
        cellStyle.SelectionForeColor = Color.FromArgb(20, 38, 61);
        cellStyle.Padding = new Padding(8, 0, 0, 0);
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(20, 38, 61);
        lblTitle.Location = new Point(32, 24);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Журнал операцій";
        gridLog.AllowUserToAddRows = false;
        gridLog.AllowUserToDeleteRows = false;
        gridLog.AllowUserToResizeRows = false;
        gridLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLog.BackgroundColor = Color.White;
        gridLog.BorderStyle = BorderStyle.None;
        gridLog.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        gridLog.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        gridLog.ColumnHeadersDefaultCellStyle = headerStyle;
        gridLog.ColumnHeadersHeight = 40;
        gridLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        gridLog.DefaultCellStyle = cellStyle;
        gridLog.EnableHeadersVisualStyles = false;
        gridLog.GridColor = Color.FromArgb(213, 220, 230);
        gridLog.Location = new Point(32, 74);
        gridLog.MultiSelect = false;
        gridLog.Name = "gridLog";
        gridLog.ReadOnly = true;
        gridLog.RowHeadersVisible = false;
        gridLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridLog.Size = new Size(644, 340);
        gridLog.Columns.AddRange(new DataGridViewColumn[] { colTime, colFile, colAction, colResult });
        gridLog.RowTemplate.Height = 38;
        colTime.FillWeight = 24F;
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
        colResult.FillWeight = 18F;
        colResult.HeaderText = "Результат";
        colResult.Name = "colResult";
        colResult.ReadOnly = true;
        cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilter.FlatStyle = FlatStyle.Flat;
        cmbFilter.Font = new Font("Segoe UI", 10F);
        cmbFilter.ForeColor = Color.FromArgb(20, 38, 61);
        cmbFilter.Location = new Point(32, 433);
        cmbFilter.Name = "cmbFilter";
        cmbFilter.Size = new Size(190, 25);
        cmbFilter.Items.AddRange(new object[] { "Усі події", "Лише помилки" });
        cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
        btnClear.BackColor = Color.White;
        btnClear.Cursor = Cursors.Hand;
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.Font = new Font("Segoe UI", 10F);
        btnClear.ForeColor = Color.FromArgb(82, 98, 116);
        btnClear.Location = new Point(440, 428);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(110, 36);
        btnClear.Text = "Очистити";
        btnClear.UseVisualStyleBackColor = false;
        btnClear.FlatAppearance.BorderColor = Color.FromArgb(213, 220, 230);
        btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 239, 252);
        btnClear.Click += btnClear_Click;
        btnRefresh.BackColor = Color.White;
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.Font = new Font("Segoe UI", 10F);
        btnRefresh.ForeColor = Color.FromArgb(45, 104, 216);
        btnRefresh.Location = new Point(566, 428);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(110, 36);
        btnRefresh.Text = "Оновити";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(191, 208, 245);
        btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 239, 252);
        btnRefresh.Click += btnRefresh_Click;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);
        Name = "LogPage";
        Size = new Size(708, 512);
        Controls.Add(lblTitle);
        Controls.Add(gridLog);
        Controls.Add(cmbFilter);
        Controls.Add(btnClear);
        Controls.Add(btnRefresh);
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblTitle;
    private DataGridView gridLog;
    private DataGridViewTextBoxColumn colTime;
    private DataGridViewTextBoxColumn colFile;
    private DataGridViewTextBoxColumn colAction;
    private DataGridViewTextBoxColumn colResult;
    private ComboBox cmbFilter;
    private Button btnClear;
    private Button btnRefresh;
}
