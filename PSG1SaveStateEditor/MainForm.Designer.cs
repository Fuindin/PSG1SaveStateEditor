namespace PSG1SaveStateEditor;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private MenuStrip _menu;
    private ToolStripMenuItem _mnuFile, _mnuOpen, _mnuSaveAs, _mnuExit, _mnuHelp, _mnuInstructions, _mnuAbout;

    private Panel _header;
    private Label _lblFile, _lblSerial, _lblTitle;
    private Panel _mesetaPanel;
    private Label _lblMeseta;
    private NumericUpDown _numMeseta;
    private Button _btnSave;

    private Label _lblBanner;

    private Panel _leftPanel;
    private Label _lblParty;
    private ListBox _lstChars;

    private Panel _divider;

    private Panel _portraitPanel;
    private PictureBox _picPortrait;

    private Panel _rightPanel;
    private Panel _namePlate;
    private TabControl _tabs;
    private TabPage _tabStatus, _tabItems, _tabMagic;
    private TableLayoutPanel _fieldTable;

    private Label _lblEquipHdr, _lblItemsHdr, _itemsNote;
    private TableLayoutPanel _equipTable;
    private ListBox _itemList;
    private FlowLayoutPanel _itemEdit;
    private ComboBox _cboItem;
    private Label _lblId;
    private NumericUpDown _numItemId;
    private Button _btnAdd, _btnReplace, _btnRemove;

    private Label _lblMagicHdr, _magicNote;
    private ListBox _spellList;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _menu = new MenuStrip();
        _mnuFile = new ToolStripMenuItem();
        _mnuOpen = new ToolStripMenuItem();
        _mnuSaveAs = new ToolStripMenuItem();
        _mnuExit = new ToolStripMenuItem();
        _mnuHelp = new ToolStripMenuItem();
        _mnuInstructions = new ToolStripMenuItem();
        _mnuAbout = new ToolStripMenuItem();
        _header = new Panel();
        _lblTitle = new Label();
        _mesetaPanel = new Panel();
        _lblMeseta = new Label();
        _numMeseta = new NumericUpDown();
        _btnSave = new Button();
        _lblSerial = new Label();
        _lblFile = new Label();
        _lblBanner = new Label();
        _leftPanel = new Panel();
        _lstChars = new ListBox();
        _lblParty = new Label();
        _divider = new Panel();
        _portraitPanel = new Panel();
        _picPortrait = new PictureBox();
        _rightPanel = new Panel();
        _tabs = new TabControl();
        _tabStatus = new TabPage();
        _fieldTable = new TableLayoutPanel();
        _tabItems = new TabPage();
        _itemList = new ListBox();
        _lblItemsHdr = new Label();
        _equipTable = new TableLayoutPanel();
        _lblEquipHdr = new Label();
        _itemEdit = new FlowLayoutPanel();
        _cboItem = new ComboBox();
        _lblId = new Label();
        _numItemId = new NumericUpDown();
        _btnAdd = new Button();
        _btnReplace = new Button();
        _btnRemove = new Button();
        _itemsNote = new Label();
        _tabMagic = new TabPage();
        _spellList = new ListBox();
        _lblMagicHdr = new Label();
        _magicNote = new Label();
        _namePlate = new Panel();
        _menu.SuspendLayout();
        _header.SuspendLayout();
        _mesetaPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numMeseta).BeginInit();
        _leftPanel.SuspendLayout();
        _portraitPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_picPortrait).BeginInit();
        _rightPanel.SuspendLayout();
        _tabs.SuspendLayout();
        _tabStatus.SuspendLayout();
        _tabItems.SuspendLayout();
        _itemEdit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numItemId).BeginInit();
        _tabMagic.SuspendLayout();
        SuspendLayout();
        //
        // _menu
        //
        _menu.ImageScalingSize = new Size(24, 24);
        _menu.Items.AddRange(new ToolStripItem[] { _mnuFile, _mnuHelp });
        _menu.Location = new Point(0, 0);
        _menu.Name = "_menu";
        _menu.Padding = new Padding(9, 3, 0, 3);
        _menu.Size = new Size(1452, 35);
        _menu.TabIndex = 6;
        //
        // _mnuFile
        //
        _mnuFile.DropDownItems.AddRange(new ToolStripItem[] { _mnuOpen, _mnuSaveAs, _mnuExit });
        _mnuFile.Name = "_mnuFile";
        _mnuFile.Size = new Size(54, 29);
        _mnuFile.Text = "File";
        //
        // _mnuOpen
        //
        _mnuOpen.Name = "_mnuOpen";
        _mnuOpen.ShortcutKeys = Keys.Control | Keys.O;
        _mnuOpen.Size = new Size(250, 34);
        _mnuOpen.Text = "Open…";
        //
        // _mnuSaveAs
        //
        _mnuSaveAs.Name = "_mnuSaveAs";
        _mnuSaveAs.ShortcutKeys = Keys.Control | Keys.S;
        _mnuSaveAs.Size = new Size(250, 34);
        _mnuSaveAs.Text = "Save As…";
        //
        // _mnuExit
        //
        _mnuExit.Name = "_mnuExit";
        _mnuExit.Size = new Size(250, 34);
        _mnuExit.Text = "Exit";
        //
        // _mnuHelp
        //
        _mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { _mnuInstructions, _mnuAbout });
        _mnuHelp.Name = "_mnuHelp";
        _mnuHelp.Size = new Size(65, 29);
        _mnuHelp.Text = "Help";
        //
        // _mnuInstructions
        //
        _mnuInstructions.Name = "_mnuInstructions";
        _mnuInstructions.ShortcutKeys = Keys.F1;
        _mnuInstructions.Size = new Size(251, 34);
        _mnuInstructions.Text = "How to Use…";
        //
        // _mnuAbout
        //
        _mnuAbout.Name = "_mnuAbout";
        _mnuAbout.Size = new Size(251, 34);
        _mnuAbout.Text = "About";
        //
        // _header
        //
        _header.Controls.Add(_lblTitle);
        _header.Controls.Add(_mesetaPanel);
        _header.Controls.Add(_lblSerial);
        _header.Controls.Add(_lblFile);
        _header.Dock = DockStyle.Top;
        _header.Location = new Point(0, 35);
        _header.Margin = new Padding(4, 5, 4, 5);
        _header.Name = "_header";
        _header.Padding = new Padding(17, 13, 17, 13);
        _header.Size = new Size(1452, 153);
        _header.TabIndex = 5;
        //
        // _lblTitle
        //
        _lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lblTitle.Location = new Point(1062, 13);
        _lblTitle.Margin = new Padding(4, 0, 4, 0);
        _lblTitle.Name = "_lblTitle";
        _lblTitle.Size = new Size(373, 62);
        _lblTitle.TabIndex = 3;
        _lblTitle.Text = "SAVE EDITOR";
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // _mesetaPanel
        //
        _mesetaPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _mesetaPanel.Controls.Add(_lblMeseta);
        _mesetaPanel.Controls.Add(_numMeseta);
        _mesetaPanel.Controls.Add(_btnSave);
        _mesetaPanel.Location = new Point(20, 87);
        _mesetaPanel.Margin = new Padding(4, 5, 4, 5);
        _mesetaPanel.Name = "_mesetaPanel";
        _mesetaPanel.Size = new Size(1415, 57);
        _mesetaPanel.TabIndex = 0;
        //
        // _lblMeseta
        //
        _lblMeseta.AutoSize = true;
        _lblMeseta.Location = new Point(0, 10);
        _lblMeseta.Margin = new Padding(4, 0, 4, 0);
        _lblMeseta.Name = "_lblMeseta";
        _lblMeseta.Size = new Size(69, 25);
        _lblMeseta.TabIndex = 0;
        _lblMeseta.Text = "MONEY";
        //
        // _numMeseta
        //
        _numMeseta.Location = new Point(123, 3);
        _numMeseta.Margin = new Padding(4, 5, 4, 5);
        _numMeseta.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        _numMeseta.Name = "_numMeseta";
        _numMeseta.Size = new Size(200, 31);
        _numMeseta.TabIndex = 1;
        _numMeseta.ThousandsSeparator = true;
        //
        // _btnSave
        //
        _btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnSave.Location = new Point(1243, 0);
        _btnSave.Margin = new Padding(4, 5, 4, 5);
        _btnSave.Name = "_btnSave";
        _btnSave.Size = new Size(169, 53);
        _btnSave.TabIndex = 2;
        _btnSave.Text = "Save As…";
        //
        // _lblSerial
        //
        _lblSerial.AutoSize = true;
        _lblSerial.Location = new Point(20, 50);
        _lblSerial.Margin = new Padding(4, 0, 4, 0);
        _lblSerial.Name = "_lblSerial";
        _lblSerial.Size = new Size(0, 25);
        _lblSerial.TabIndex = 1;
        //
        // _lblFile
        //
        _lblFile.AutoSize = true;
        _lblFile.Location = new Point(20, 17);
        _lblFile.Margin = new Padding(4, 0, 4, 0);
        _lblFile.Name = "_lblFile";
        _lblFile.Size = new Size(302, 25);
        _lblFile.TabIndex = 2;
        _lblFile.Text = "No save state loaded.  File ▸ Open…";
        //
        // _lblBanner
        //
        _lblBanner.Dock = DockStyle.Top;
        _lblBanner.Location = new Point(0, 188);
        _lblBanner.Margin = new Padding(4, 0, 4, 0);
        _lblBanner.Name = "_lblBanner";
        _lblBanner.Padding = new Padding(17, 0, 17, 0);
        _lblBanner.Size = new Size(1452, 0);
        _lblBanner.TabIndex = 4;
        _lblBanner.TextAlign = ContentAlignment.MiddleLeft;
        _lblBanner.Visible = false;
        //
        // _leftPanel
        //
        _leftPanel.Controls.Add(_lstChars);
        _leftPanel.Controls.Add(_lblParty);
        _leftPanel.Dock = DockStyle.Left;
        _leftPanel.Location = new Point(0, 188);
        _leftPanel.Margin = new Padding(4, 5, 4, 5);
        _leftPanel.Name = "_leftPanel";
        _leftPanel.Padding = new Padding(17, 17, 9, 20);
        _leftPanel.Size = new Size(430, 983);
        _leftPanel.TabIndex = 3;
        //
        // _lstChars
        //
        _lstChars.BorderStyle = BorderStyle.None;
        _lstChars.Dock = DockStyle.Fill;
        _lstChars.DrawMode = DrawMode.OwnerDrawFixed;
        _lstChars.IntegralHeight = false;
        _lstChars.ItemHeight = 120;
        _lstChars.Location = new Point(17, 64);
        _lstChars.Margin = new Padding(4, 5, 4, 5);
        _lstChars.Name = "_lstChars";
        _lstChars.Size = new Size(404, 899);
        _lstChars.TabIndex = 0;
        //
        // _lblParty
        //
        _lblParty.Dock = DockStyle.Top;
        _lblParty.Location = new Point(17, 17);
        _lblParty.Margin = new Padding(4, 0, 4, 0);
        _lblParty.Name = "_lblParty";
        _lblParty.Size = new Size(404, 47);
        _lblParty.TabIndex = 1;
        _lblParty.Text = "PARTY";
        //
        // _divider
        //
        _divider.Dock = DockStyle.Left;
        _divider.Location = new Point(380, 188);
        _divider.Margin = new Padding(4, 5, 4, 5);
        _divider.Name = "_divider";
        _divider.Size = new Size(3, 983);
        _divider.TabIndex = 2;
        //
        // _portraitPanel
        //
        _portraitPanel.Controls.Add(_picPortrait);
        _portraitPanel.Dock = DockStyle.Left;
        _portraitPanel.Location = new Point(383, 188);
        _portraitPanel.Name = "_portraitPanel";
        _portraitPanel.Padding = new Padding(22, 24, 22, 24);
        _portraitPanel.Size = new Size(300, 983);
        _portraitPanel.TabIndex = 1;
        //
        // _picPortrait
        //
        _picPortrait.Dock = DockStyle.Fill;
        _picPortrait.Location = new Point(22, 24);
        _picPortrait.Name = "_picPortrait";
        _picPortrait.Size = new Size(256, 935);
        _picPortrait.SizeMode = PictureBoxSizeMode.Zoom;
        _picPortrait.TabIndex = 0;
        _picPortrait.TabStop = false;
        //
        // _rightPanel
        //
        _rightPanel.Controls.Add(_tabs);
        _rightPanel.Controls.Add(_namePlate);
        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.Location = new Point(683, 188);
        _rightPanel.Margin = new Padding(4, 5, 4, 5);
        _rightPanel.Name = "_rightPanel";
        _rightPanel.Padding = new Padding(23, 20, 23, 20);
        _rightPanel.Size = new Size(769, 983);
        _rightPanel.TabIndex = 0;
        //
        // _tabs
        //
        _tabs.Controls.Add(_tabStatus);
        _tabs.Controls.Add(_tabItems);
        _tabs.Controls.Add(_tabMagic);
        _tabs.Dock = DockStyle.Fill;
        _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        _tabs.ItemSize = new Size(150, 30);
        _tabs.Location = new Point(23, 127);
        _tabs.Margin = new Padding(4, 5, 4, 5);
        _tabs.Name = "_tabs";
        _tabs.Padding = new Point(14, 6);
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(723, 836);
        _tabs.SizeMode = TabSizeMode.Fixed;
        _tabs.TabIndex = 0;
        //
        // _tabStatus
        //
        _tabStatus.AutoScroll = true;
        _tabStatus.Controls.Add(_fieldTable);
        _tabStatus.Location = new Point(4, 34);
        _tabStatus.Margin = new Padding(4, 5, 4, 5);
        _tabStatus.Name = "_tabStatus";
        _tabStatus.Padding = new Padding(23, 27, 23, 27);
        _tabStatus.Size = new Size(715, 798);
        _tabStatus.TabIndex = 0;
        _tabStatus.Text = "STATUS";
        //
        // _fieldTable
        //
        _fieldTable.AutoSize = true;
        _fieldTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fieldTable.ColumnCount = 4;
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _fieldTable.Dock = DockStyle.Top;
        _fieldTable.Location = new Point(23, 27);
        _fieldTable.Margin = new Padding(4, 5, 4, 5);
        _fieldTable.Name = "_fieldTable";
        _fieldTable.Size = new Size(669, 0);
        _fieldTable.TabIndex = 0;
        //
        // _tabItems
        //
        _tabItems.Controls.Add(_itemList);
        _tabItems.Controls.Add(_lblItemsHdr);
        _tabItems.Controls.Add(_equipTable);
        _tabItems.Controls.Add(_lblEquipHdr);
        _tabItems.Controls.Add(_itemEdit);
        _tabItems.Controls.Add(_itemsNote);
        _tabItems.Location = new Point(4, 34);
        _tabItems.Margin = new Padding(4, 5, 4, 5);
        _tabItems.Name = "_tabItems";
        _tabItems.Padding = new Padding(23, 20, 23, 20);
        _tabItems.Size = new Size(715, 798);
        _tabItems.TabIndex = 1;
        _tabItems.Text = "EQUIP & ITEMS";
        //
        // _itemList
        //
        _itemList.BorderStyle = BorderStyle.None;
        _itemList.Dock = DockStyle.Fill;
        _itemList.DrawMode = DrawMode.OwnerDrawFixed;
        _itemList.IntegralHeight = false;
        _itemList.ItemHeight = 34;
        _itemList.Location = new Point(23, 366);
        _itemList.Margin = new Padding(4, 5, 4, 5);
        _itemList.Name = "_itemList";
        _itemList.Size = new Size(669, 300);
        _itemList.TabIndex = 0;
        //
        // _lblItemsHdr
        //
        _lblItemsHdr.Dock = DockStyle.Top;
        _lblItemsHdr.Location = new Point(23, 323);
        _lblItemsHdr.Margin = new Padding(4, 0, 4, 0);
        _lblItemsHdr.Name = "_lblItemsHdr";
        _lblItemsHdr.Size = new Size(669, 43);
        _lblItemsHdr.TabIndex = 1;
        _lblItemsHdr.Text = "ITEMS";
        //
        // _equipTable
        //
        _equipTable.ColumnCount = 2;
        _equipTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 157F));
        _equipTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _equipTable.Dock = DockStyle.Top;
        _equipTable.Location = new Point(23, 63);
        _equipTable.Margin = new Padding(4, 5, 4, 5);
        _equipTable.Name = "_equipTable";
        _equipTable.Size = new Size(669, 260);
        _equipTable.TabIndex = 2;
        //
        // _lblEquipHdr
        //
        _lblEquipHdr.Dock = DockStyle.Top;
        _lblEquipHdr.Location = new Point(23, 20);
        _lblEquipHdr.Margin = new Padding(4, 0, 4, 0);
        _lblEquipHdr.Name = "_lblEquipHdr";
        _lblEquipHdr.Size = new Size(669, 43);
        _lblEquipHdr.TabIndex = 3;
        _lblEquipHdr.Text = "EQUIP";
        //
        // _itemEdit
        //
        _itemEdit.AutoSize = true;
        _itemEdit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _itemEdit.Controls.Add(_cboItem);
        _itemEdit.Controls.Add(_lblId);
        _itemEdit.Controls.Add(_numItemId);
        _itemEdit.Controls.Add(_btnAdd);
        _itemEdit.Controls.Add(_btnReplace);
        _itemEdit.Controls.Add(_btnRemove);
        _itemEdit.Dock = DockStyle.Bottom;
        _itemEdit.Location = new Point(23, 666);
        _itemEdit.Margin = new Padding(4, 5, 4, 5);
        _itemEdit.Name = "_itemEdit";
        _itemEdit.Padding = new Padding(0, 10, 0, 4);
        _itemEdit.Size = new Size(669, 75);
        _itemEdit.TabIndex = 4;
        //
        // _cboItem
        //
        _cboItem.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboItem.FlatStyle = FlatStyle.Flat;
        _cboItem.Margin = new Padding(0, 8, 10, 4);
        _cboItem.Name = "_cboItem";
        _cboItem.Size = new Size(240, 33);
        _cboItem.TabIndex = 0;
        //
        // _lblId
        //
        _lblId.Anchor = AnchorStyles.Left;
        _lblId.AutoSize = true;
        _lblId.Margin = new Padding(0, 8, 4, 4);
        _lblId.Name = "_lblId";
        _lblId.Size = new Size(46, 25);
        _lblId.TabIndex = 1;
        _lblId.Text = "ID 0x";
        //
        // _numItemId
        //
        _numItemId.Hexadecimal = true;
        _numItemId.Margin = new Padding(0, 8, 14, 4);
        _numItemId.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        _numItemId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numItemId.Name = "_numItemId";
        _numItemId.Size = new Size(80, 31);
        _numItemId.TabIndex = 2;
        _numItemId.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // _btnAdd
        //
        _btnAdd.Margin = new Padding(0, 4, 8, 4);
        _btnAdd.Name = "_btnAdd";
        _btnAdd.AutoSize = true;
        _btnAdd.MinimumSize = new Size(110, 0);
        _btnAdd.Padding = new Padding(8, 4, 8, 4);
        _btnAdd.Size = new Size(110, 42);
        _btnAdd.TabIndex = 3;
        _btnAdd.Text = "Add";
        //
        // _btnReplace
        //
        _btnReplace.Margin = new Padding(0, 4, 8, 4);
        _btnReplace.Name = "_btnReplace";
        _btnReplace.AutoSize = true;
        _btnReplace.MinimumSize = new Size(110, 0);
        _btnReplace.Padding = new Padding(8, 4, 8, 4);
        _btnReplace.Size = new Size(110, 42);
        _btnReplace.TabIndex = 4;
        _btnReplace.Text = "Replace";
        //
        // _btnRemove
        //
        _btnRemove.Margin = new Padding(0, 4, 8, 4);
        _btnRemove.Name = "_btnRemove";
        _btnRemove.AutoSize = true;
        _btnRemove.MinimumSize = new Size(110, 0);
        _btnRemove.Padding = new Padding(8, 4, 8, 4);
        _btnRemove.Size = new Size(110, 42);
        _btnRemove.TabIndex = 5;
        _btnRemove.Text = "Remove";
        //
        // _itemsNote
        //
        _itemsNote.Dock = DockStyle.Bottom;
        _itemsNote.Location = new Point(23, 741);
        _itemsNote.Margin = new Padding(4, 0, 4, 0);
        _itemsNote.Name = "_itemsNote";
        _itemsNote.Size = new Size(669, 37);
        _itemsNote.TabIndex = 5;
        //
        // _tabMagic
        //
        _tabMagic.Controls.Add(_spellList);
        _tabMagic.Controls.Add(_lblMagicHdr);
        _tabMagic.Controls.Add(_magicNote);
        _tabMagic.Location = new Point(4, 34);
        _tabMagic.Margin = new Padding(4, 5, 4, 5);
        _tabMagic.Name = "_tabMagic";
        _tabMagic.Padding = new Padding(23, 20, 23, 20);
        _tabMagic.Size = new Size(715, 798);
        _tabMagic.TabIndex = 2;
        _tabMagic.Text = "MAGIC";
        //
        // _spellList
        //
        _spellList.BorderStyle = BorderStyle.None;
        _spellList.Dock = DockStyle.Fill;
        _spellList.DrawMode = DrawMode.OwnerDrawFixed;
        _spellList.IntegralHeight = false;
        _spellList.ItemHeight = 34;
        _spellList.Location = new Point(23, 63);
        _spellList.Margin = new Padding(4, 5, 4, 5);
        _spellList.Name = "_spellList";
        _spellList.SelectionMode = SelectionMode.None;
        _spellList.Size = new Size(669, 678);
        _spellList.TabIndex = 0;
        //
        // _lblMagicHdr
        //
        _lblMagicHdr.Dock = DockStyle.Top;
        _lblMagicHdr.Location = new Point(23, 20);
        _lblMagicHdr.Margin = new Padding(4, 0, 4, 0);
        _lblMagicHdr.Name = "_lblMagicHdr";
        _lblMagicHdr.Size = new Size(669, 43);
        _lblMagicHdr.TabIndex = 1;
        _lblMagicHdr.Text = "MAGIC";
        //
        // _magicNote
        //
        _magicNote.Dock = DockStyle.Bottom;
        _magicNote.Location = new Point(23, 741);
        _magicNote.Margin = new Padding(4, 0, 4, 0);
        _magicNote.Name = "_magicNote";
        _magicNote.Size = new Size(669, 37);
        _magicNote.TabIndex = 2;
        //
        // _namePlate
        //
        _namePlate.Dock = DockStyle.Top;
        _namePlate.Location = new Point(23, 20);
        _namePlate.Margin = new Padding(4, 5, 4, 5);
        _namePlate.Name = "_namePlate";
        _namePlate.Size = new Size(723, 107);
        _namePlate.TabIndex = 1;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1452, 1171);
        Controls.Add(_rightPanel);
        Controls.Add(_portraitPanel);
        Controls.Add(_divider);
        Controls.Add(_leftPanel);
        Controls.Add(_lblBanner);
        Controls.Add(_header);
        Controls.Add(_menu);
        MainMenuStrip = _menu;
        Margin = new Padding(4, 5, 4, 5);
        MinimumSize = new Size(1300, 1000);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Phantasy Star Generation 1 — Save Editor";
        _menu.ResumeLayout(false);
        _menu.PerformLayout();
        _header.ResumeLayout(false);
        _header.PerformLayout();
        _mesetaPanel.ResumeLayout(false);
        _mesetaPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numMeseta).EndInit();
        _leftPanel.ResumeLayout(false);
        _portraitPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_picPortrait).EndInit();
        _rightPanel.ResumeLayout(false);
        _tabs.ResumeLayout(false);
        _tabStatus.ResumeLayout(false);
        _tabStatus.PerformLayout();
        _tabItems.ResumeLayout(false);
        _tabItems.PerformLayout();
        _itemEdit.ResumeLayout(false);
        _itemEdit.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numItemId).EndInit();
        _tabMagic.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
