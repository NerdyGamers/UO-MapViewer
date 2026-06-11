using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UO.MapExporter;

namespace UO.MapViewer
{
    public class ExportDialog : Form
    {
        private readonly Map _map;
        private TextBox _outPathBox = null!;
        private ComboBox _modeCombo = null!;
        private NumericUpDown _tileSizeNum = null!;
        private CheckBox _staticsCheck = null!;
        private NumericUpDown _scaleNum = null!;
        private ProgressBar _progress = null!;
        private Button _exportBtn = null!;
        private Button _cancelBtn = null!;
        private CancellationTokenSource? _cts;

        public ExportDialog(Map map)
        {
            _map = map;
            Text = $"Export Map — {map}"; 
            Width = 480; Height = 340;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BuildUI();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _outPathBox = new TextBox { Dock = DockStyle.Fill };
            var browseBtn = new Button { Text = "...", Width = 28, Dock = DockStyle.Right };
            browseBtn.Click += (_, _) => BrowseOutput();
            var pathPanel = new Panel { Dock = DockStyle.Fill };
            pathPanel.Controls.Add(_outPathBox);
            pathPanel.Controls.Add(browseBtn);

            _modeCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _modeCombo.Items.AddRange(new object[] { "Deep Zoom (DZI)", "PNG Tile Grid", "Full PNG" });
            _modeCombo.SelectedIndex = 0;

            _tileSizeNum = new NumericUpDown { Minimum = 64, Maximum = 1024, Value = 256, Dock = DockStyle.Fill };
            _staticsCheck = new CheckBox { Text = "Include Statics", Checked = true, Dock = DockStyle.Fill };
            _scaleNum = new NumericUpDown { Minimum = 1, Maximum = 8, Value = 1, Dock = DockStyle.Fill };

            _progress = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Continuous };

            int row = 0;
            void AddRow(string label, Control ctrl)
            {
                layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleRight }, 0, row);
                layout.Controls.Add(ctrl, 1, row);
                row++;
            }

            AddRow("Output Path:", pathPanel);
            AddRow("Mode:", _modeCombo);
            AddRow("Tile Size (px):", _tileSizeNum);
            AddRow("Statics:", _staticsCheck);
            AddRow("Scale (x):", _scaleNum);
            AddRow("Progress:", _progress);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            _cancelBtn = new Button { Text = "Cancel", Width = 80, DialogResult = DialogResult.Cancel };
            _exportBtn = new Button { Text = "Export", Width = 80 };
            _exportBtn.Click += async (_, _) => await RunExport();
            btnPanel.Controls.Add(_cancelBtn);
            btnPanel.Controls.Add(_exportBtn);
            layout.Controls.Add(new Label(), 0, row);
            layout.Controls.Add(btnPanel, 1, row);

            Controls.Add(layout);
        }

        private void BrowseOutput()
        {
            using var dlg = new FolderBrowserDialog { Description = "Select output directory", UseDescriptionForTitle = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                _outPathBox.Text = dlg.SelectedPath;
        }

        private async Task RunExport()
        {
            if (string.IsNullOrWhiteSpace(_outPathBox.Text))
            {
                MessageBox.Show("Choose an output path first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _exportBtn.Enabled = false;
            _cts = new CancellationTokenSource();

            var mode = _modeCombo.SelectedIndex switch
            {
                1 => ExportMode.TileGrid,
                2 => ExportMode.FullPng,
                _ => ExportMode.DeepZoom
            };

            var options = new ExportOptions
            {
                Map         = _map,
                OutputPath  = _outPathBox.Text,
                Mode        = mode,
                TileSize    = (int)_tileSizeNum.Value,
                RenderStatics = _staticsCheck.Checked,
                Scale       = (int)_scaleNum.Value
            };

            var progress = new Progress<int>(p => _progress.Value = Math.Min(p, 100));

            try
            {
                await Task.Run(() => MapExporterCore.Export(options, progress, _cts.Token), _cts.Token);
                MessageBox.Show("Export complete!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Export cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _exportBtn.Enabled = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            base.OnFormClosing(e);
        }
    }
}
