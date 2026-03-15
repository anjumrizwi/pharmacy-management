using PharmacyManagement.CSharp.Data;

namespace PharmacyManagement.CSharp.Forms;

internal sealed class MainForm : Form
{
    internal MainForm()
    {
        Text = "Pharmacy Management";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 70,
            Text = "Pharmacy Management Modules",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            BackColor = Color.FromArgb(25, 118, 210),
            ForeColor = Color.White
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 4,
            RowCount = 3
        };

        for (var i = 0; i < grid.ColumnCount; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        for (var i = 0; i < grid.RowCount; i++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        }

        var index = 0;
        foreach (var module in ModuleCatalog.Modules)
        {
            var button = new Button
            {
                Text = module.Title,
                Dock = DockStyle.Fill,
                Margin = new Padding(12),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(236, 239, 241),
                FlatStyle = FlatStyle.Flat
            };

            button.Click += (_, _) =>
            {
                using var form = new CrudForm(module);
                form.ShowDialog(this);
            };

            var col = index % grid.ColumnCount;
            var row = index / grid.ColumnCount;
            grid.Controls.Add(button, col, row);
            index++;
        }

        var aboutButton = new Button
        {
            Text = "About",
            Dock = DockStyle.Bottom,
            Height = 38
        };

        aboutButton.Click += (_, _) => MessageBox.Show(
            "Migrated from VB6 to C# WinForms baseline.\nModules map to the original VB forms.",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        Controls.Add(grid);
        Controls.Add(aboutButton);
        Controls.Add(header);
    }
}
