using System.Data;
using System.Data.OleDb;
using PharmacyManagement.CSharp.Data;
using PharmacyManagement.CSharp.Models;

namespace PharmacyManagement.CSharp.Forms;

internal sealed class CrudForm : Form
{
    private readonly ModuleDefinition _module;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly FlowLayoutPanel _editor = new() { Dock = DockStyle.Top, Height = 180, AutoScroll = true, Padding = new Padding(8) };
    private readonly Dictionary<string, TextBox> _fields = new(StringComparer.OrdinalIgnoreCase);

    private DataTable _table = new();
    private string _idColumn = string.Empty;

    internal CrudForm(ModuleDefinition module)
    {
        _module = module;

        Text = $"{module.Title} - {module.TableName}";
        Width = 1200;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(8)
        };

        var insertButton = new Button { Text = "Insert", Width = 100 };
        var updateButton = new Button { Text = "Update", Width = 100 };
        var deleteButton = new Button { Text = "Delete", Width = 100 };
        var refreshButton = new Button { Text = "Refresh", Width = 100 };

        insertButton.Click += (_, _) => InsertRow();
        updateButton.Click += (_, _) => UpdateRow();
        deleteButton.Click += (_, _) => DeleteRow();
        refreshButton.Click += (_, _) => LoadData();
        _grid.SelectionChanged += (_, _) => PopulateEditorFromSelection();

        toolbar.Controls.Add(insertButton);
        toolbar.Controls.Add(updateButton);
        toolbar.Controls.Add(deleteButton);
        toolbar.Controls.Add(refreshButton);

        Controls.Add(_grid);
        Controls.Add(_editor);
        Controls.Add(toolbar);

        Load += (_, _) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            _table = Database.Query($"SELECT * FROM [{_module.TableName}]");
            _grid.DataSource = _table;
            BuildEditor();
            PopulateEditorFromSelection();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load module '{_module.TableName}'.\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BuildEditor()
    {
        _editor.Controls.Clear();
        _fields.Clear();

        if (_table.Columns.Count == 0)
        {
            return;
        }

        _idColumn = _table.Columns[0].ColumnName;

        foreach (DataColumn column in _table.Columns)
        {
            var label = new Label
            {
                Text = column.ColumnName,
                Width = 140,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 10, 0, 0)
            };

            var textbox = new TextBox
            {
                Name = $"txt{column.ColumnName}",
                Width = 180,
                Margin = new Padding(8)
            };

            _editor.Controls.Add(label);
            _editor.Controls.Add(textbox);
            _fields[column.ColumnName] = textbox;
        }
    }

    private void PopulateEditorFromSelection()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView view)
        {
            return;
        }

        foreach (DataColumn column in _table.Columns)
        {
            if (_fields.TryGetValue(column.ColumnName, out var textbox))
            {
                var value = view.Row[column.ColumnName];
                textbox.Text = value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;
            }
        }
    }

    private void InsertRow()
    {
        if (_table.Columns.Count == 0)
        {
            return;
        }

        var columnNames = _table.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]").ToArray();
        var placeholders = _table.Columns.Cast<DataColumn>().Select(_ => "?").ToArray();
        var parameters = BuildAllColumnParameters();

        var sql = $"INSERT INTO [{_module.TableName}] ({string.Join(",", columnNames)}) VALUES ({string.Join(",", placeholders)})";
        ExecuteWrite(sql, parameters, "Record inserted.");
    }

    private void UpdateRow()
    {
        if (_table.Columns.Count < 2)
        {
            return;
        }

        var updates = _table.Columns.Cast<DataColumn>()
            .Where(c => !c.ColumnName.Equals(_idColumn, StringComparison.OrdinalIgnoreCase))
            .Select(c => $"[{c.ColumnName}] = ?")
            .ToArray();

        var parameters = BuildNonIdParameters();
        parameters.Add(new OleDbParameter("@id", GetFieldText(_idColumn)));

        var sql = $"UPDATE [{_module.TableName}] SET {string.Join(",", updates)} WHERE [{_idColumn}] = ?";
        ExecuteWrite(sql, parameters, "Record updated.");
    }

    private void DeleteRow()
    {
        if (string.IsNullOrWhiteSpace(_idColumn))
        {
            return;
        }

        var idValue = GetFieldText(_idColumn);
        if (string.IsNullOrWhiteSpace(idValue))
        {
            MessageBox.Show($"Enter {_idColumn} to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sql = $"DELETE FROM [{_module.TableName}] WHERE [{_idColumn}] = ?";
        var parameters = new List<OleDbParameter> { new("@id", idValue) };

        ExecuteWrite(sql, parameters, "Record deleted.");
    }

    private List<OleDbParameter> BuildAllColumnParameters()
    {
        return _table.Columns.Cast<DataColumn>()
            .Select(c => new OleDbParameter($"@{c.ColumnName}", GetFieldText(c.ColumnName)))
            .ToList();
    }

    private List<OleDbParameter> BuildNonIdParameters()
    {
        return _table.Columns.Cast<DataColumn>()
            .Where(c => !c.ColumnName.Equals(_idColumn, StringComparison.OrdinalIgnoreCase))
            .Select(c => new OleDbParameter($"@{c.ColumnName}", GetFieldText(c.ColumnName)))
            .ToList();
    }

    private string GetFieldText(string columnName)
    {
        return _fields.TryGetValue(columnName, out var textbox)
            ? textbox.Text.Trim()
            : string.Empty;
    }

    private void ExecuteWrite(string sql, List<OleDbParameter> parameters, string successMessage)
    {
        try
        {
            var affected = Database.Execute(sql, parameters);
            MessageBox.Show($"{successMessage} Rows affected: {affected}.", "Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Operation failed.\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
