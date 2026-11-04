using System.Drawing.Drawing2D;

namespace PSG1SaveStateEditor;

/// <summary>
/// Phantasy Star Generation:1 menu palette — near-black translucent windows with glowing
/// cyan frames, a neon-green "STATUS" plate, lime stat labels and gold equipped items.
/// </summary>
internal static class Theme
{
    public static readonly Color Void       = Color.FromArgb(0x07, 0x0C, 0x12); // outer frame
    public static readonly Color Window     = Color.FromArgb(0x10, 0x18, 0x20); // menu windows
    public static readonly Color WindowHi   = Color.FromArgb(0x18, 0x24, 0x30); // cards / tab pages
    public static readonly Color Field      = Color.FromArgb(0x1E, 0x2E, 0x3C); // numeric / combo fields
    public static readonly Color PlateTop   = Color.FromArgb(0x1C, 0x5F, 0x8A); // name-plate gradient
    public static readonly Color PlateBot   = Color.FromArgb(0x0C, 0x2A, 0x42);
    public static readonly Color Frame      = Color.FromArgb(0x3C, 0xCC, 0xF2); // cyan window border
    public static readonly Color FrameDeep  = Color.FromArgb(0x17, 0x78, 0xA0);
    public static readonly Color Neon       = Color.FromArgb(0x3A, 0xE8, 0x3C); // STATUS plate green
    public static readonly Color Lime       = Color.FromArgb(0xBE, 0xF0, 0x32); // LV / ATT. labels
    public static readonly Color Gold       = Color.FromArgb(0xF2, 0xE1, 0x4A); // equipped items / meseta
    public static readonly Color Text       = Color.FromArgb(0xF4, 0xF7, 0xFA);
    public static readonly Color TextMuted  = Color.FromArgb(0x8E, 0xA4, 0xB6);
    public static readonly Color HpRed      = Color.FromArgb(0xE8, 0x34, 0x3C);
    public static readonly Color MpGreen    = Color.FromArgb(0x3E, 0xD4, 0x4C);
    public static readonly Color BarEmpty   = Color.FromArgb(0x9A, 0xA4, 0xAC);
    public static readonly Color PortraitBg = Color.FromArgb(0x7E, 0xC8, 0x50); // green portrait backdrop

    public static readonly string Family = "Segoe UI";

    public static Font Body(float size, FontStyle style = FontStyle.Regular) => new(Family, size, style);

    /// <summary>A rectangle with the game's clipped (chamfered) top-left / bottom-right corners.</summary>
    public static GraphicsPath Chamfer(Rectangle r, int cut)
    {
        var p = new GraphicsPath();
        p.AddLine(r.Left + cut, r.Top, r.Right, r.Top);
        p.AddLine(r.Right, r.Top, r.Right, r.Bottom - cut);
        p.AddLine(r.Right, r.Bottom - cut, r.Right - cut, r.Bottom);
        p.AddLine(r.Right - cut, r.Bottom, r.Left, r.Bottom);
        p.AddLine(r.Left, r.Bottom, r.Left, r.Top + cut);
        p.CloseFigure();
        return p;
    }

    /// <summary>Paints a chamfered menu window: dark fill + glowing cyan frame.</summary>
    public static void PaintWindow(Graphics g, Rectangle r, Color fill, Color frame, int cut = 12)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        r.Width -= 1;
        r.Height -= 1;
        using var path = Chamfer(r, cut);
        using var brush = new SolidBrush(fill);
        g.FillPath(brush, path);
        using var glow = new Pen(Color.FromArgb(70, frame), 5);
        g.DrawPath(glow, path);
        using var pen = new Pen(frame, 2);
        g.DrawPath(pen, path);
    }
}

/// <summary>Themed colors for the menu strip (dark popups, cyan highlight).</summary>
internal sealed class PsMenuColors : ProfessionalColorTable
{
    public override Color MenuStripGradientBegin => Theme.Void;
    public override Color MenuStripGradientEnd => Theme.Void;
    public override Color ToolStripDropDownBackground => Theme.Window;
    public override Color ImageMarginGradientBegin => Theme.Window;
    public override Color ImageMarginGradientMiddle => Theme.Window;
    public override Color ImageMarginGradientEnd => Theme.Window;
    public override Color MenuItemSelected => Theme.FrameDeep;
    public override Color MenuItemSelectedGradientBegin => Theme.FrameDeep;
    public override Color MenuItemSelectedGradientEnd => Theme.FrameDeep;
    public override Color MenuItemBorder => Theme.Frame;
    public override Color MenuItemPressedGradientBegin => Theme.Field;
    public override Color MenuItemPressedGradientEnd => Theme.Window;
    public override Color MenuBorder => Theme.FrameDeep;
    public override Color SeparatorDark => Theme.FrameDeep;
}
