using PharmacyManagement.CSharp.Data;

namespace PharmacyManagement.CSharp.Forms;

internal sealed class LoginForm : Form
{
    private readonly TextBox _userName = new() { PlaceholderText = "Username" };
    private readonly TextBox _email = new() { PlaceholderText = "Email" };
    private readonly TextBox _password = new() { PlaceholderText = "Password", UseSystemPasswordChar = true };
    private readonly TextBox _quickFill = new() { PlaceholderText = "Type 'a' to autofill legacy creds" };

    internal LoginForm()
    {
        Text = "Pharmacy Management - Login";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 460;
        Height = 320;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "Pharmacy Management",
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 6
        };

        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var loginButton = new Button
        {
            Text = "Login",
            Dock = DockStyle.Fill,
            Height = 36,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        loginButton.Click += (_, _) => TryLogin();
        _quickFill.TextChanged += (_, _) => ApplyQuickFill();

        table.Controls.Add(_userName, 0, 0);
        table.Controls.Add(_email, 0, 1);
        table.Controls.Add(_password, 0, 2);
        table.Controls.Add(_quickFill, 0, 3);
        table.Controls.Add(loginButton, 0, 4);

        Controls.Add(table);
        Controls.Add(title);

        AcceptButton = loginButton;
    }

    private void ApplyQuickFill()
    {
        if (_quickFill.Text.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            _userName.Text = "admin";
            _email.Text = "mail@admin";
            _password.Text = "#access";
        }
    }

    private void TryLogin()
    {
        var valid = _userName.Text == "admin"
            && _email.Text == "mail@admin"
            && _password.Text == "#access";

        if (!valid)
        {
            MessageBox.Show("Wrong credentials", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Database.TryResolveDatabasePath(out _))
        {
            var result = MessageBox.Show(
                "Database file pharm.mdb was not found.\nClick OK to select the database file now.",
                "Database Missing",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
            {
                return;
            }

            using var picker = new OpenFileDialog
            {
                Title = "Select pharm.mdb",
                Filter = "Access Database (*.mdb;*.accdb)|*.mdb;*.accdb|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (picker.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Database.SetSelectedDatabasePath(picker.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set database path.\n{ex.Message}", "Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        Hide();
        using var mainForm = new MainForm();
        mainForm.ShowDialog(this);
        Show();
    }
}
