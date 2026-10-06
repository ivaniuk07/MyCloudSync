using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Forms;

public partial class DriveFolderPickerForm : Form
{
    private readonly ICloudStorage _cloudStorage;

    public DriveFolderPickerForm(ICloudStorage cloudStorage)
    {
        InitializeComponent();
        _cloudStorage = cloudStorage;
    }

    public CloudFolder? SelectedFolder { get; private set; }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            var folders = await _cloudStorage.ListFoldersAsync("root");
            foreach (var folder in folders)
            {
                treeFolders.Nodes.Add(new TreeNode(folder.Name) { Tag = folder });
            }
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        SelectedFolder = treeFolders.SelectedNode?.Tag as CloudFolder;
        if (SelectedFolder == null)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
