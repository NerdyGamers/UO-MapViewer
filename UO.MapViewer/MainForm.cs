using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace UO.MapViewer
{
    public partial class MainForm : Form
    {
        private MapPanel _mapPanel = null!;
        private ToolStrip _toolbar = null!;
        private StatusStrip _status = null!;
        private ToolStripStatusLabel _coordLabel = null!;
        private ToolStripComboBox _facetCombo = null!;

        public MainForm()
        {
            Text = "UO Map Viewer";
            Size = new Size(1280, 900);
            MinimumSize = new Size(800, 600);
            BackColor = Color.Black;

            BuildUI();
            PromptDataPath();
        }

        private void BuildUI()
        {
            _toolbar = new ToolStrip { Dock = DockStyle.Top };

            _facetCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            _facetCombo.Items.AddRange(new object[] { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" });
            _facetCombo.SelectedIndex = 0;
            _facetCombo.SelectedIndexChanged += (_, _) => SwitchFacet();

            var exportBtn = new ToolStripButton("Export...") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            exportBtn.Click += (_, _) => ShowExportDialog();

            var pathBtn = new ToolStripButton("Set Data Path") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            pathBtn.Click += (_, _) => PromptDataPath();

            _toolbar.Items.Add(new ToolStripLabel("Facet: "));
            _toolbar.Items.Add(_facetCombo);
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(exportBtn);
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(pathBtn);

            _status = new StatusStrip { Dock = DockStyle.Bottom };
            _coordLabel = new ToolStripStatusLabel("X: 0  Y: 0") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _status.Items.Add(_coordLabel);

            _mapPanel = new MapPanel { Dock = DockStyle.Fill };
            _mapPanel.CoordinateChanged += (_, e) => _coordLabel.Text = $"X: {e.X}  Y: {e.Y}  (Drag: MMB/LMB | Zoom: Wheel)";

            Controls.Add(_mapPanel);
            Controls.Add(_toolbar);
            Controls.Add(_status);
        }

        private void PromptDataPath()
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select your Ultima Online data directory (containing map0.mul or map0LegacyMUL.uop)",
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Files.SetMulPath(dlg.SelectedPath);
                _mapPanel.LoadMap(Map.Felucca);
            }
        }

        private void SwitchFacet()
        {
            var facet = _facetCombo.SelectedItem?.ToString() switch
            {
                "Trammel"  => Map.Trammel,
                "Ilshenar" => Map.Ilshenar,
                "Malas"    => Map.Malas,
                "Tokuno"   => Map.Tokuno,
                "TerMur"   => Map.TerMur,
                _          => Map.Felucca
            };
            _mapPanel.LoadMap(facet);
        }

        private void ShowExportDialog()
        {
            if (_mapPanel.CurrentMap == null)
            {
                MessageBox.Show("Load a map first.", "UO Map Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var dlg = new ExportDialog(_mapPanel.CurrentMap!);
            dlg.ShowDialog(this);
        }
    }
}
