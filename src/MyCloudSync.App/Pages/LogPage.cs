using MyCloudSync.App.Controls;
using MyCloudSync.App.Forms;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.App.Pages;

public partial class LogPage : UserControl
{
    private readonly ISyncLogRepository _logRepository;

    public LogPage(ISyncLogRepository logRepository)
    {
        InitializeComponent();
        _logRepository = logRepository;
        cmbFilter.SelectedIndex = 0;
    }

    public void LoadEntries()
    {
        gridLog.Rows.Clear();
        var onlyErrors = cmbFilter.SelectedIndex == 1;

        foreach (var entry in _logRepository.GetRecent(500))
        {
            if (onlyErrors && entry.Result != LogResult.Error)
            {
                continue;
            }

            var rowIndex = gridLog.Rows.Add(
                entry.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                entry.Message ?? "",
                GetActionText(entry.Action),
                GetResultText(entry.Result));

            var resultCell = gridLog.Rows[rowIndex].Cells[colResult.Index];
            if (entry.Result == LogResult.Error)
            {
                resultCell.Style.ForeColor = Theme.Red;
                resultCell.Style.Font = new Font(gridLog.Font, FontStyle.Bold);
            }
            else if (entry.Result == LogResult.Warning)
            {
                resultCell.Style.ForeColor = Theme.Orange;
            }
        }
    }

    private static string GetActionText(string action)
    {
        return action switch
        {
            "Upload" => "Вивантаження",
            "Download" => "Завантаження",
            "Delete" => "Видалення",
            "Rename" => "Перейменування",
            "Conflict" => "Конфлікт",
            "Skip" => "Пропущено",
            "Error" => "Помилка",
            "Info" => "Інформація",
            _ => action
        };
    }

    private static string GetResultText(LogResult result)
    {
        return result switch
        {
            LogResult.Success => "Успішно",
            LogResult.Warning => "Попередження",
            LogResult.Error => "Помилка",
            _ => ""
        };
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        LoadEntries();
    }

    private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (IsHandleCreated)
        {
            LoadEntries();
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        if (MessageHelper.Confirm(this, "Очистити весь журнал?"))
        {
            _logRepository.Clear();
            LoadEntries();
        }
    }
}
