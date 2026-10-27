using System.ComponentModel;

namespace MyCloudSync.App.Controls;

public class CardPanel : Panel
{
    private Color _borderColor = Theme.Border;

    public CardPanel()
    {
        BackColor = Color.White;
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(_borderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
