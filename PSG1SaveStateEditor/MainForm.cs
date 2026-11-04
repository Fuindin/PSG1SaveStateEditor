using System.Drawing.Drawing2D;
using PSG1SaveStateEditor.Game;

namespace PSG1SaveStateEditor;

public sealed partial class MainForm : Form
{
    private readonly Psg1Layout _layout = Psg1Layout.Load();
    private Psg1Save? _save;
    private readonly List<Psg1Character> _chars = new();
    private readonly Dictionary<string, NumericUpDown> _fieldEditors = new();
    private bool _dirty;
    private bool _loading;      // suppress write-back while populating controls
    private bool _syncingId;    // suppress combo <-> id feedback loops
    private string? _pendingOpen;

    private Psg1Character? Current =>
        _lstChars.SelectedIndex is int i && i >= 0 && i < _chars.Count ? _chars[i] : null;

    public MainForm() : this(null) { }

    public MainForm(string? openPath)
    {
        InitializeComponent();   // visual layout (MainForm.Designer.cs)
        ApplyTheme();            // colours, fonts, owner-draw
        WireEvents();
        BuildFieldEditors();     // stat rows from the offset config
        PopulateItemPicker();
        UpdateState();
        _pendingOpen = openPath;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UpdateListMetrics();
        UpdateTabSize();

        if (_pendingOpen is not null)
        {
            string path = _pendingOpen;
            _pendingOpen = null;
            LoadPath(path);
        }
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        UpdateListMetrics();
        UpdateTabSize();
    }

    /// <summary>Size the owner-drawn tabs to fit the widest caption at the current DPI.</summary>
    private void UpdateTabSize()
    {
        int w = 0, h = 0;
        foreach (TabPage p in _tabs.TabPages)
        {
            Size sz = TextRenderer.MeasureText(p.Text, _tabs.Font);
            w = Math.Max(w, sz.Width);
            h = Math.Max(h, sz.Height);
        }

        _tabs.ItemSize = new Size(w + 36, h + 16);
    }

    /// <summary>Row heights for the owner-drawn lists, derived from font height (DPI-safe).</summary>
    private void UpdateListMetrics()
    {
        if (!_lstChars.IsHandleCreated)
        {
            return;
        }

        using var g = _lstChars.CreateGraphics();
        using var small = Theme.Body(9f, FontStyle.Bold);
        float name = _lstChars.Font.GetHeight(g), line = small.GetHeight(g);
        _lstChars.ItemHeight = Math.Min(255, (int)Math.Ceiling(name + line * 2 + 34));
        _itemList.ItemHeight = Math.Min(255, (int)Math.Ceiling(_itemList.Font.GetHeight(g)) + 12);
        _spellList.ItemHeight = _itemList.ItemHeight;
        _lstChars.Invalidate();
    }

    // ---- Theme & wiring ----------------------------------------------------

    private void ApplyTheme()
    {
        BackColor = Theme.Void;
        ForeColor = Theme.Text;
        Font = Theme.Body(9.75f);

        _menu.Renderer = new ToolStripProfessionalRenderer(new PsMenuColors()) { RoundedEdges = false };
        _menu.BackColor = Theme.Void;
        _menu.ForeColor = Theme.Text;
        _menu.Font = Theme.Body(9.75f);

        _header.BackColor = Theme.Window;
        _lblFile.ForeColor = Theme.Text;
        _lblSerial.ForeColor = Theme.TextMuted;
        _lblFile.BackColor = _lblSerial.BackColor = Color.Transparent;

        _lblTitle.BackColor = Theme.Window;
        _lblTitle.ForeColor = Theme.Text;
        _lblTitle.Font = Theme.Body(15f, FontStyle.Bold | FontStyle.Italic);
        _lblTitle.Paint += PaintTitlePlate;

        _mesetaPanel.BackColor = Theme.Window;
        _lblMeseta.ForeColor = Theme.Gold;
        _lblMeseta.BackColor = Color.Transparent;
        _lblMeseta.Font = Theme.Body(12f, FontStyle.Bold);
        _numMeseta.Maximum = _layout.MesetaMax;
        _numMeseta.Font = Theme.Body(12f, FontStyle.Bold);
        StyleNumeric(_numMeseta);
        StyleButton(_btnSave);

        _lblBanner.Font = Theme.Body(9.5f, FontStyle.Bold);

        _leftPanel.BackColor = Theme.Window;
        StyleHeader(_lblParty);
        _lstChars.BackColor = Theme.Window;
        _lstChars.ForeColor = Theme.Text;
        _lstChars.Font = Theme.Body(12f, FontStyle.Bold);

        _divider.BackColor = Theme.FrameDeep;

        _portraitPanel.BackColor = Theme.Window;
        _picPortrait.BackColor = Theme.PortraitBg;
        _portraitPanel.Paint += (_, e) =>
        {
            var r = Rectangle.Inflate(_picPortrait.Bounds, 6, 6);
            Theme.PaintWindow(e.Graphics, r, Theme.PortraitBg, Theme.Frame, 14);
        };

        _rightPanel.BackColor = Theme.Window;
        _namePlate.BackColor = Theme.Window;
        _namePlate.Paint += PaintNamePlate;
        _namePlate.Resize += (_, _) => _namePlate.Invalidate();

        _tabs.Font = Theme.Body(10f, FontStyle.Bold);
        foreach (TabPage page in _tabs.TabPages)
        {
            page.BackColor = Theme.WindowHi;
        }

        _fieldTable.BackColor = Theme.WindowHi;

        _equipTable.BackColor = Theme.WindowHi;
        _equipTable.AutoSize = true;
        _equipTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        foreach (var l in new[] { _lblEquipHdr, _lblItemsHdr, _lblMagicHdr })
        {
            StyleHeader(l);
            l.BackColor = Theme.WindowHi;
        }

        foreach (var l in new[] { _itemsNote, _magicNote })
        {
            l.BackColor = Theme.WindowHi;
            l.ForeColor = Theme.TextMuted;
            l.Font = Theme.Body(8.5f, FontStyle.Italic);
        }

        foreach (var lb in new[] { _itemList, _spellList })
        {
            lb.BackColor = Theme.Window;
            lb.ForeColor = Theme.Text;
            lb.Font = Theme.Body(11f);
        }

        _itemEdit.BackColor = Theme.WindowHi;
        _cboItem.BackColor = Theme.Field;
        _cboItem.ForeColor = Theme.Text;
        _cboItem.Font = Theme.Body(10.5f);
        _lblId.ForeColor = Theme.TextMuted;
        _lblId.BackColor = Color.Transparent;
        _lblId.Font = Theme.Body(10f);
        _numItemId.Font = Theme.Body(10.5f);
        StyleNumeric(_numItemId);
        foreach (var b in new[] { _btnAdd, _btnReplace, _btnRemove })
        {
            StyleButton(b);
        }
    }

    private static void StyleHeader(Label l)
    {
        l.ForeColor = Theme.Frame;
        l.BackColor = Color.Transparent;
        l.Font = Theme.Body(12f, FontStyle.Bold | FontStyle.Italic);
        l.Padding = new Padding(2, 4, 0, 0);
    }

    private static void StyleNumeric(NumericUpDown n)
    {
        n.BorderStyle = BorderStyle.FixedSingle;
        n.BackColor = Theme.Field;
        n.ForeColor = Theme.Text;
    }

    private static void StyleButton(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = Theme.PlateBot;
        b.ForeColor = Theme.Text;
        b.Font = Theme.Body(10f, FontStyle.Bold);
        b.FlatAppearance.BorderColor = Theme.Frame;
        b.FlatAppearance.BorderSize = 2;
        b.FlatAppearance.MouseOverBackColor = Theme.PlateTop;
        b.FlatAppearance.MouseDownBackColor = Theme.FrameDeep;
        b.Cursor = Cursors.Hand;
    }

    private void WireEvents()
    {
        _mnuOpen.Click += (_, _) => OpenFile();
        _mnuSaveAs.Click += (_, _) => SaveAs();
        _mnuExit.Click += (_, _) => Close();
        _mnuInstructions.Click += (_, _) => ShowInstructions();
        _mnuAbout.Click += (_, _) => MessageBox.Show(this,
            "Phantasy Star Generation 1 — Save State Editor\n" +
            "Created by Fuindin\n\n" +
            "Edits PCSX2 (.p2s) save states for Phantasy Star Generation:1 (SLPM-623.62).\n\n" +
            "Always writes to a new copy; your original is never modified.",
            "About", MessageBoxButtons.OK, MessageBoxIcon.Information);

        _numMeseta.ValueChanged += (_, _) =>
        {
            if (!_loading && _save is not null)
            {
                _save.Meseta = (long)_numMeseta.Value;
                MarkDirty();
            }
        };
        _btnSave.Click += (_, _) => SaveAs();

        _lstChars.DrawItem += DrawCharacterCard;
        _lstChars.SelectedIndexChanged += (_, _) => LoadSelectedCharacter();
        _tabs.DrawItem += DrawTab;
        _itemList.DrawItem += DrawItemRow;
        _itemList.SelectedIndexChanged += (_, _) => UpdateItemButtons();
        _spellList.DrawItem += DrawSpellRow;

        _cboItem.SelectedIndexChanged += (_, _) =>
        {
            if (_syncingId || _cboItem.SelectedItem is not PickerEntry p)
            {
                return;
            }

            _syncingId = true;
            _numItemId.Value = p.Id;
            _syncingId = false;
            UpdateItemButtons();
        };
        _numItemId.ValueChanged += (_, _) =>
        {
            if (_syncingId)
            {
                return;
            }

            _syncingId = true;
            SelectPickerId((int)_numItemId.Value);
            _syncingId = false;
            UpdateItemButtons();
        };

        _btnAdd.Click += (_, _) => EditBag(c => c.AddItem((int)_numItemId.Value), selectLast: true);
        _btnReplace.Click += (_, _) =>
        {
            int idx = _itemList.SelectedIndex;
            EditBag(c => c.ReplaceItem(idx, (int)_numItemId.Value), keepIndex: idx);
        };
        _btnRemove.Click += (_, _) =>
        {
            int idx = _itemList.SelectedIndex;
            EditBag(c => c.RemoveItem(idx), keepIndex: idx);
        };
    }

    // ---- Owner drawing -----------------------------------------------------

    private void PaintTitlePlate(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var r = _lblTitle.ClientRectangle;
        g.Clear(Theme.Window);
        Theme.PaintWindow(g, Rectangle.Inflate(r, -3, -3), Color.Black, Theme.Neon, 10);
        TextRenderer.DrawText(g, _lblTitle.Text, _lblTitle.Font, r, Theme.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    /// <summary>The game's name tab: cyan-framed blue gradient plate with the name, role and level.</summary>
    private void PaintNamePlate(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Theme.Window);
        var c = Current;
        if (c is null)
        {
            return;
        }

        var r = Rectangle.Inflate(_namePlate.ClientRectangle, -3, -6);
        r.Height -= 8;
        using (var path = Theme.Chamfer(r, 16))
        using (var grad = new LinearGradientBrush(r, Theme.PlateTop, Theme.PlateBot, LinearGradientMode.Vertical))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(grad, path);
            using var glow = new Pen(Color.FromArgb(80, Theme.Frame), 6);
            g.DrawPath(glow, path);
            using var pen = new Pen(Theme.Frame, 2);
            g.DrawPath(pen, path);
        }

        using var nameFont = Theme.Body(20f, FontStyle.Bold);
        using var subFont = Theme.Body(10.5f, FontStyle.Italic);
        using var lvFont = Theme.Body(16f, FontStyle.Bold);
        int nameH = (int)Math.Ceiling(nameFont.GetHeight(g));
        var nameRect = new Rectangle(r.X + 22, r.Y + 6, r.Width - 40, nameH);
        TextRenderer.DrawText(g, c.Name, nameFont, nameRect, Theme.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        var subRect = new Rectangle(r.X + 24, nameRect.Bottom, r.Width - 40, r.Bottom - nameRect.Bottom - 4);
        TextRenderer.DrawText(g, c.Role, subFont, subRect, Theme.TextMuted, TextFormatFlags.Left | TextFormatFlags.Top);

        string lv = $"LV {c.Get("level")}";
        TextRenderer.DrawText(g, lv, lvFont, new Rectangle(r.X, r.Y, r.Width - 26, r.Height), Theme.Lime,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private void DrawTab(object? sender, DrawItemEventArgs e)
    {
        var rect = _tabs.GetTabRect(e.Index);
        bool selected = e.Index == _tabs.SelectedIndex;
        e.Graphics.FillRectangle(new SolidBrush(Theme.Window), rect);
        var plate = Rectangle.Inflate(rect, -2, -2);
        using (var grad = new LinearGradientBrush(plate,
                   selected ? Theme.PlateTop : Theme.WindowHi,
                   selected ? Theme.PlateBot : Theme.Window, LinearGradientMode.Vertical))
        {
            e.Graphics.FillRectangle(grad, plate);
        }

        using (var pen = new Pen(selected ? Theme.Frame : Theme.FrameDeep, selected ? 2 : 1))
        {
            e.Graphics.DrawRectangle(pen, plate);
        }

        TextRenderer.DrawText(e.Graphics, _tabs.TabPages[e.Index].Text, _tabs.Font, rect,
            selected ? Theme.Text : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    /// <summary>Party card in the style of the ITEM menu: name, LV, red HP bar, green MP bar.</summary>
    private void DrawCharacterCard(object? sender, DrawItemEventArgs e)
    {
        var g = e.Graphics;
        g.FillRectangle(new SolidBrush(Theme.Window), e.Bounds);
        if (e.Index < 0 || e.Index >= _chars.Count)
        {
            return;
        }

        var c = _chars[e.Index];
        bool selected = (e.State & DrawItemState.Selected) != 0;
        var card = Rectangle.Inflate(e.Bounds, -4, -5);
        Theme.PaintWindow(g, card, selected ? Color.FromArgb(0x16, 0x3E, 0x58) : Theme.WindowHi,
            selected ? Theme.Frame : Theme.FrameDeep, 12);

        using var small = Theme.Body(9f, FontStyle.Bold);
        int nameH = (int)Math.Ceiling(_lstChars.Font.GetHeight(g));
        int lineH = (int)Math.Ceiling(small.GetHeight(g));
        int x = card.X + 14, w = card.Width - 28;

        var nameRect = new Rectangle(x, card.Y + 6, w, nameH);
        TextRenderer.DrawText(g, c.Name, _lstChars.Font, nameRect, Theme.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, $"LV {c.Get("level")}", _lstChars.Font, nameRect, Theme.Lime, TextFormatFlags.Right);

        int y = nameRect.Bottom + 4;
        DrawBar(g, small, "HP", c.Get("hpCur"), c.Get("hpMax"), Theme.HpRed, new Rectangle(x, y, w, lineH + 6));
        DrawBar(g, small, "MP", c.Get("mpCur"), c.Get("mpMax"), Theme.MpGreen, new Rectangle(x, y + lineH + 8, w, lineH + 6));
    }

    private static void DrawBar(Graphics g, Font f, string label, long cur, long max, Color color, Rectangle r)
    {
        int labelW = TextRenderer.MeasureText("MP ", f).Width;
        TextRenderer.DrawText(g, label, f, new Rectangle(r.X, r.Y, labelW, r.Height - 4), Theme.Lime,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        var bar = new Rectangle(r.X + labelW, r.Bottom - 5, r.Width - labelW, 3);
        using (var empty = new SolidBrush(Theme.BarEmpty))
        {
            g.FillRectangle(empty, bar);
        }

        if (max > 0)
        {
            int fw = (int)(bar.Width * Math.Clamp((double)cur / max, 0, 1));
            using var fill = new SolidBrush(color);
            g.FillRectangle(fill, bar.X, bar.Y, fw, bar.Height);
        }

        TextRenderer.DrawText(g, $"{cur} / {max}", f, new Rectangle(r.X, r.Y, r.Width, r.Height - 4), Theme.Text,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private static string Glyph(ItemKind k) => k switch
    {
        ItemKind.Weapon => "⚔",
        ItemKind.Armor => "▲",
        ItemKind.Shield => "◆",
        ItemKind.Crystal => "✦",
        _ => "●",
    };

    private void DrawItemRow(object? sender, DrawItemEventArgs e)
    {
        var g = e.Graphics;
        g.FillRectangle(new SolidBrush(Theme.Window), e.Bounds);
        if (e.Index < 0 || e.Index >= _itemList.Items.Count)
        {
            return;
        }

        if (_itemList.Items[e.Index] is not BagItem it)
        {
            TextRenderer.DrawText(g, _itemList.Items[e.Index]?.ToString() ?? "", _itemList.Font,
                Rectangle.Inflate(e.Bounds, -12, 0), Theme.TextMuted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            return;
        }

        if ((e.State & DrawItemState.Selected) != 0)
        {
            var hi = Rectangle.Inflate(e.Bounds, -3, -2);
            using var fill = new SolidBrush(Color.FromArgb(55, Theme.Frame));
            using var pen = new Pen(Theme.Frame, 1);
            g.FillRectangle(fill, hi);
            g.DrawRectangle(pen, hi);
        }

        Color color = it.Equipped ? Theme.Gold : Theme.Text;
        var r = Rectangle.Inflate(e.Bounds, -12, 0);
        int glyphW = TextRenderer.MeasureText("⚔ ", _itemList.Font).Width;
        TextRenderer.DrawText(g, Glyph(it.Kind), _itemList.Font, new Rectangle(r.X, r.Y, glyphW, r.Height), color,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, it.Name, _itemList.Font, new Rectangle(r.X + glyphW, r.Y, r.Width - glyphW, r.Height), color,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        using var small = Theme.Body(8.5f, FontStyle.Italic);
        string tag = it.Equipped ? $"equipped · {it.Kind}   0x{it.Id:X2}" : $"0x{it.Id:X2}";
        TextRenderer.DrawText(g, tag, small, r, it.Equipped ? Theme.Gold : Theme.TextMuted,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private void DrawSpellRow(object? sender, DrawItemEventArgs e)
    {
        var g = e.Graphics;
        g.FillRectangle(new SolidBrush(Theme.Window), e.Bounds);
        if (e.Index < 0 || e.Index >= _spellList.Items.Count)
        {
            return;
        }

        var r = Rectangle.Inflate(e.Bounds, -12, 0);
        object item = _spellList.Items[e.Index];
        if (item is int id)
        {
            TextRenderer.DrawText(g, "✧ " + Psg1Spells.Name(id), _spellList.Font, r, Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            using var small = Theme.Body(8.5f, FontStyle.Italic);
            TextRenderer.DrawText(g, $"0x{id:X2}", small, r, Theme.TextMuted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }
        else
        {
            TextRenderer.DrawText(g, item?.ToString() ?? "", _spellList.Font, r, Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    // ---- Stat rows (generated from the offset config) ----------------------

    private void BuildFieldEditors()
    {
        _fieldTable.Controls.Clear();
        _fieldEditors.Clear();
        _fieldTable.RowStyles.Clear();
        _fieldTable.RowCount = _layout.Fields.Count;

        int row = 0;
        foreach (var f in _layout.Fields)
        {
            _fieldTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label
            {
                Text = f.Label, AutoSize = true, Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 14, 5), ForeColor = Theme.Lime,
                BackColor = Color.Transparent, Font = Theme.Body(11f, FontStyle.Bold | FontStyle.Italic),
            };
            var desc = new Label
            {
                Text = f.Description, AutoSize = true, Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 14, 5), ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent, Font = Theme.Body(10f),
            };
            var num = new NumericUpDown
            {
                Minimum = f.Min,
                Maximum = f.Max,
                Width = 150,
                ThousandsSeparator = f.Max > 9999,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 14, 5),
                Tag = f.Key,
                Font = Theme.Body(11f, FontStyle.Bold),
            };
            StyleNumeric(num);
            num.ValueChanged += (_, _) =>
            {
                if (_loading || Current is not { } c)
                {
                    return;
                }

                c.Set(f.Key, (long)num.Value);
                MarkDirty();
                _lstChars.Invalidate();      // refresh LV / HP / MP on the card
                _namePlate.Invalidate();
            };
            var hint = new Label
            {
                Text = f.Hint ?? "", AutoSize = true, Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 0, 5), ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent, Font = Theme.Body(8.5f, FontStyle.Italic),
            };

            _fieldEditors[f.Key] = num;
            _fieldTable.Controls.Add(lbl, 0, row);
            _fieldTable.Controls.Add(desc, 1, row);
            _fieldTable.Controls.Add(num, 2, row);
            _fieldTable.Controls.Add(hint, 3, row);
            row++;
        }
    }

    // ---- Equipment & items ---------------------------------------------------

    private sealed record PickerEntry(int Id, string Name)
    {
        public override string ToString() => $"{Name}  (0x{Id:X2})";
    }

    private void PopulateItemPicker()
    {
        _cboItem.Items.Clear();
        foreach (var (id, name) in Psg1Items.Names.OrderBy(kv => kv.Value, StringComparer.OrdinalIgnoreCase))
        {
            _cboItem.Items.Add(new PickerEntry(id, name));
        }

        if (_cboItem.Items.Count > 0)
        {
            _cboItem.SelectedIndex = 0;
        }
    }

    private void SelectPickerId(int id)
    {
        for (int i = 0; i < _cboItem.Items.Count; i++)
        {
            if (_cboItem.Items[i] is PickerEntry p && p.Id == id)
            {
                _cboItem.SelectedIndex = i;
                return;
            }
        }

        _cboItem.SelectedIndex = -1;   // an unnamed id typed directly
    }

    private static readonly (ItemKind kind, string label)[] EquipSlots =
    {
        (ItemKind.Weapon, "Weapon"), (ItemKind.Armor, "Armor"),
        (ItemKind.Shield, "Shield"), (ItemKind.Crystal, "Crystal"),
    };

    private void RenderEquipmentAndItems(Psg1Character c, int selectIndex = -1)
    {
        var items = c.Items();

        _equipTable.SuspendLayout();
        _equipTable.Controls.Clear();
        _equipTable.RowStyles.Clear();
        var equipped = items.Where(i => i.Equipped).ToList();
        var rows = new List<(string label, BagItem? item)>();
        var shown = new HashSet<int>();   // bag indices already placed in a slot row
        foreach (var (kind, label) in EquipSlots)
        {
            BagItem? match = null;
            foreach (var i in equipped)
            {
                if (i.Kind == kind && !shown.Contains(i.Index))
                {
                    match = i;
                    shown.Add(i.Index);
                    break;
                }
            }

            rows.Add((label, match));
        }

        // Anything equipped that didn't fit the four categories still gets a row.
        foreach (var extra in equipped.Where(i => !shown.Contains(i.Index)))
        {
            rows.Add(("Other", extra));
        }

        _equipTable.RowCount = rows.Count;
        int row = 0;
        foreach (var (label, item) in rows)
        {
            _equipTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bool filled = item.HasValue;
            _equipTable.Controls.Add(new Label
            {
                Text = label, AutoSize = true, Anchor = AnchorStyles.Left,
                Margin = new Padding(2, 4, 12, 4), ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent, Font = Theme.Body(10.5f),
            }, 0, row);
            _equipTable.Controls.Add(new Label
            {
                Text = item is { } eq ? $"{Glyph(eq.Kind)}  {eq.Name}" : "— none —",
                AutoSize = true, Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 4, 0, 4), BackColor = Color.Transparent,
                ForeColor = filled ? Theme.Gold : Theme.TextMuted,
                Font = Theme.Body(11f, filled ? FontStyle.Bold : FontStyle.Italic),
            }, 1, row);
            row++;
        }

        _equipTable.ResumeLayout();

        _itemList.BeginUpdate();
        _itemList.Items.Clear();
        foreach (var it in items)
        {
            _itemList.Items.Add(it);
        }

        if (items.Count == 0)
        {
            _itemList.Items.Add("— no items —");
        }

        _itemList.EndUpdate();
        if (selectIndex >= 0 && items.Count > 0)
        {
            _itemList.SelectedIndex = Math.Min(selectIndex, items.Count - 1);
        }

        _itemsNote.Text = $"{items.Count} / {c.ItemCapacity} items.  Gold = equipped (locked).  " +
                          "Add, replace or remove unequipped items; equip them in-game.";
        UpdateItemButtons();
    }

    private void UpdateItemButtons()
    {
        var c = Current;
        bool loaded = _save is not null && c is not null;
        var sel = _itemList.SelectedItem as BagItem?;
        bool canEditSel = loaded && sel is { Equipped: false };

        _cboItem.Enabled = _numItemId.Enabled = loaded;
        _btnAdd.Enabled = loaded && c!.CanAdd;
        _btnReplace.Enabled = canEditSel && sel!.Value.Id != (int)_numItemId.Value;
        _btnRemove.Enabled = canEditSel;
    }

    private void EditBag(Func<Psg1Character, bool> edit, int keepIndex = -1, bool selectLast = false)
    {
        if (Current is not { } c || !edit(c))
        {
            return;
        }

        MarkDirty();
        int sel = selectLast ? c.Items().Count - 1 : keepIndex;
        RenderEquipmentAndItems(c, sel);
    }

    private void RenderSpells(Psg1Character c)
    {
        var spells = c.Spells();
        _spellList.BeginUpdate();
        _spellList.Items.Clear();
        foreach (int id in spells)
        {
            _spellList.Items.Add(id);
        }

        if (spells.Count == 0)
        {
            _spellList.Items.Add($"{c.Name} doesn't use magic.");
        }

        _spellList.EndUpdate();
        _magicNote.Text = $"{spells.Count} spell(s) learned — read-only.  Name spell ids in psg1_spells.json next to the exe.";
    }

    // ---- Help -----------------------------------------------------------------

    private void ShowInstructions()
    {
        MessageBox.Show(this,
            "How to use\r\n\r\n" +
            "1.  In PCSX2, make a save state, then File ▸ Open… and pick the .p2s.\r\n" +
            "2.  Pick a party member on the left.\r\n" +
            "      STATUS — edit level, EXP, HP / MP and attributes.\r\n" +
            "      EQUIP & ITEMS — add, replace or remove unequipped items in that\r\n" +
            "      character's bag (equipped gear is shown in gold and locked).\r\n" +
            "      MAGIC — learned spells (display only).\r\n" +
            "3.  Edit Meseta at the top.\r\n" +
            "4.  File ▸ Save As… — the editor writes a NEW *_edited.p2s file and never\r\n" +
            "    touches your original.\r\n" +
            "5.  Load the edited save state in PCSX2.\r\n\r\n" +
            "Note: ATT., DEF. and DEXT. are base values; the in-game Status screen\r\n" +
            "adds your equipment's bonuses on top.\r\n\r\n" +
            "Tip: make your first edit a small one and confirm it in-game.",
            "How to Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ---- Actions ----------------------------------------------------------------

    private void OpenFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "PCSX2 save state (*.p2s)|*.p2s|All files (*.*)|*.*",
            Title = "Open PCSX2 save state",
        };

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            LoadPath(dlg.FileName);
        }
    }

    private void LoadPath(string path)
    {
        try
        {
            _save = Psg1Save.Open(path, _layout);
            _dirty = false;
            PopulateCharacters();
            UpdateState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not open save state:\n\n{ex.Message}", "Open failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveAs()
    {
        if (_save is null)
        {
            return;
        }

        string suggested = Path.GetFileNameWithoutExtension(_save.SourcePath) + "_edited.p2s";
        using var dlg = new SaveFileDialog
        {
            Filter = "PCSX2 save state (*.p2s)|*.p2s",
            Title = "Save edited copy",
            FileName = suggested,
            InitialDirectory = Path.GetDirectoryName(_save.SourcePath),
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(dlg.FileName), Path.GetFullPath(_save.SourcePath),
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "Please choose a different file name — the editor never overwrites the original.",
                "Choose a new file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _save.SaveAs(dlg.FileName);
            _dirty = false;
            UpdateTitle();
            MessageBox.Show(this, $"Saved:\n{dlg.FileName}\n\nLoad this state in PCSX2 to use it.",
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save:\n\n{ex.Message}", "Save failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ---- Population / state -------------------------------------------------------

    private void PopulateCharacters()
    {
        _lstChars.Items.Clear();
        _chars.Clear();
        if (_save is null)
        {
            return;
        }

        foreach (var c in _save.Characters)
        {
            _chars.Add(c);
            _lstChars.Items.Add(c.Name);
        }

        if (_lstChars.Items.Count > 0)
        {
            _lstChars.SelectedIndex = 0;
        }
    }

    private void LoadSelectedCharacter()
    {
        if (Current is not { } c)
        {
            return;
        }

        _loading = true;
        foreach (var (key, num) in _fieldEditors)
        {
            if (c.TryGet(key, out long v))
            {
                num.Value = Math.Clamp(v, num.Minimum, num.Maximum);
                num.Enabled = true;
            }
            else
            {
                num.Enabled = false;
            }
        }

        _loading = false;
        _namePlate.Invalidate();
        RenderEquipmentAndItems(c);
        RenderSpells(c);
        LoadPortrait(c.Name);
    }

    /// <summary>
    /// Shows the portrait from <c>portraits/&lt;Name&gt;.png</c> next to the executable
    /// (user-swappable). Missing or unreadable files just leave the panel blank.
    /// </summary>
    private void LoadPortrait(string name)
    {
        _picPortrait.Image?.Dispose();
        _picPortrait.Image = null;

        string path = Path.Combine(AppContext.BaseDirectory, "portraits", name + ".png");
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            using var ms = new MemoryStream(File.ReadAllBytes(path));
            using var loaded = Image.FromStream(ms);
            _picPortrait.Image = new Bitmap(loaded);
        }
        catch { /* leave blank on any decode error */ }
    }

    private void UpdateState()
    {
        bool loaded = _save is not null;
        _btnSave.Enabled = loaded;
        _mnuSaveAs.Enabled = loaded;
        _numMeseta.Enabled = loaded;
        UpdateItemButtons();

        if (!loaded)
        {
            _lblFile.Text = "No save state loaded.  File ▸ Open…";
            _lblSerial.Text = "";
            _lblBanner.Visible = false;
            UpdateTitle();
            return;
        }

        _lblFile.Text = _save!.SourcePath;
        _lblSerial.Text = $"Detected game: {_save.DetectedSerial ?? "unknown"}   •   {_chars.Count} party members";

        _loading = true;
        _numMeseta.Value = Math.Clamp(_save.Meseta, _numMeseta.Minimum, _numMeseta.Maximum);
        _loading = false;

        if (!_save.StructureFound)
        {
            _lblBanner.Visible = true;
            _lblBanner.Height = 28;
            _lblBanner.BackColor = Color.FromArgb(60, 20, 20);
            _lblBanner.ForeColor = Color.FromArgb(255, 150, 150);
            _lblBanner.Text = "⚠  Character data not found at the expected offsets — this may be a different game, version, or an unusual save point.";
        }
        else
        {
            _lblBanner.Visible = false;
            _lblBanner.Height = 0;
        }

        UpdateTitle();
    }

    private void MarkDirty()
    {
        if (_dirty)
        {
            return;
        }

        _dirty = true;
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        const string baseTitle = "Phantasy Star Generation 1 — Save Editor";
        Text = _save is null
            ? baseTitle
            : $"{baseTitle} — {Path.GetFileName(_save.SourcePath)}{(_dirty ? " *" : "")}";
    }
}
