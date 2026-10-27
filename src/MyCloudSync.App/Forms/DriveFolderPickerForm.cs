using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Forms;

public partial class DriveFolderPickerForm : Form
{
    private const string LoadingText = "Завантаження...";
    private readonly ICloudStorage _cloudStorage;

    public DriveFolderPickerForm(ICloudStorage cloudStorage)
    {
        InitializeComponent();
        _cloudStorage = cloudStorage;
    }

    public CloudFolder? SelectedFolder { get; private set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var root = CreateNode(new CloudFolder("root", "Мій диск"));
        treeFolders.Nodes.Add(root);
        root.Expand();
    }

    private static TreeNode CreateNode(CloudFolder folder)
    {
        var node = new TreeNode(folder.Name) { Tag = folder };
        node.Nodes.Add(LoadingText);
        return node;
    }

    private static bool IsLoaded(TreeNode node)
    {
        return node.Nodes.Count != 1 || node.Nodes[0].Tag != null;
    }

    private async void treeFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        var node = e.Node!;
        if (IsLoaded(node))
        {
            return;
        }

        try
        {
            var parent = (CloudFolder)node.Tag!;
            var folders = await _cloudStorage.ListFoldersAsync(parent.Id);

            node.Nodes.Clear();
            foreach (var folder in folders)
            {
                node.Nodes.Add(CreateNode(folder));
            }
        }
        catch (Exception ex)
        {
            node.Nodes.Clear();
            MessageHelper.ShowError(this, ex);
        }
    }

    private async void btnNewFolder_Click(object sender, EventArgs e)
    {
        var name = txtNewFolder.Text.Trim();
        var parentNode = treeFolders.SelectedNode ?? treeFolders.Nodes[0];
        if (name == "" || parentNode.Tag is not CloudFolder parent)
        {
            MessageHelper.ShowWarning(this, "Виберіть папку і введіть назву нової папки.");
            return;
        }

        try
        {
            btnNewFolder.Enabled = false;
            var created = await _cloudStorage.CreateFolderAsync(parent.Id, name);

            if (IsLoaded(parentNode))
            {
                var node = new TreeNode(created.Name) { Tag = created };
                parentNode.Nodes.Add(node);
                treeFolders.SelectedNode = node;
            }

            parentNode.Expand();
            txtNewFolder.Clear();
        }
        catch (Exception ex)
        {
            MessageHelper.ShowError(this, ex);
        }
        finally
        {
            btnNewFolder.Enabled = true;
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
