using System.Data;
using System.Data.OleDb;
using PharmacyManagement.CSharp.Properties;

namespace PharmacyManagement.CSharp.Data;

internal static class Database
{
    private static string? _selectedPath;
    private static string? _resolvedConnectionString;
    private static string? _resolvedForPath;

    private static string AppDataPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PharmacyManagement.CSharp", "dbpath.txt");

    internal static bool TryResolveDatabasePath(out string path)
    {
        var discoveredFromParents = FindDatabaseInParentDirectories("pharm.mdb", 8);

        var candidates = new List<string?>
        {
            Environment.GetEnvironmentVariable("PHARMACY_DB_PATH"),
            _selectedPath,
            Settings.Default.PHARMACY_DB_PATH,
            LoadPersistedPath(),
            Path.Combine(AppContext.BaseDirectory, "pharm.mdb"),
            Path.Combine(Environment.CurrentDirectory, "pharm.mdb"),
            discoveredFromParents
        };

        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }

        path = string.Empty;
        return false;
    }

    private static string? FindDatabaseInParentDirectories(string fileName, int maxLevels)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        for (var level = 0; current is not null && level <= maxLevels; level++)
        {
            var candidate = Path.Combine(current.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    internal static void SetSelectedDatabasePath(string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Database file not found.", absolutePath);
        }

        _selectedPath = absolutePath;
        PersistPath(absolutePath);
        _resolvedConnectionString = null;
        _resolvedForPath = null;
    }

    private static string DatabasePath =>
        TryResolveDatabasePath(out var resolved)
            ? resolved
            : throw new FileNotFoundException(
                "Database file pharm.mdb was not found. Set PHARMACY_DB_PATH, select the file at login, or place it next to the executable.");

    internal static string ConnectionString => ResolveConnectionString();

    private static string ResolveConnectionString()
    {
        var databasePath = DatabasePath;
        if (!string.IsNullOrWhiteSpace(_resolvedConnectionString)
            && string.Equals(_resolvedForPath, databasePath, StringComparison.OrdinalIgnoreCase))
        {
            return _resolvedConnectionString;
        }

        var extension = Path.GetExtension(databasePath).ToLowerInvariant();
        var providers = extension switch
        {
            ".accdb" => new[] { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" },
            _ => new[] { "Microsoft.Jet.OLEDB.4.0", "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" }
        };

        var providerErrors = new List<string>();

        foreach (var provider in providers)
        {
            var connectionString = $"Provider={provider};Data Source={databasePath};Persist Security Info=False;";

            try
            {
                using var connection = new OleDbConnection(connectionString);
                connection.Open();

                _resolvedConnectionString = connectionString;
                _resolvedForPath = databasePath;
                return connectionString;
            }
            catch (Exception ex)
            {
                providerErrors.Add($"{provider}: {ex.Message}");
            }
        }

        throw new InvalidOperationException(
            "No compatible Access OLE DB provider was found for this database file. " +
            "For .mdb files, run x86 with Jet 4.0. For .accdb files, install Microsoft Access Database Engine (x86). " +
            $"Details: {string.Join(" | ", providerErrors)}");
    }

    private static void PersistPath(string path)
    {
        var folder = Path.GetDirectoryName(AppDataPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(AppDataPath, path);
    }

    private static string? LoadPersistedPath()
    {
        if (!File.Exists(AppDataPath))
        {
            return null;
        }

        var value = File.ReadAllText(AppDataPath).Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    internal static DataTable Query(string sql, IReadOnlyList<OleDbParameter>? parameters = null)
    {
        using var connection = new OleDbConnection(ConnectionString);
        using var command = new OleDbCommand(sql, connection);

        if (parameters is not null)
        {
            command.Parameters.AddRange(parameters.ToArray());
        }

        using var adapter = new OleDbDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    internal static int Execute(string sql, IReadOnlyList<OleDbParameter>? parameters = null)
    {
        using var connection = new OleDbConnection(ConnectionString);
        using var command = new OleDbCommand(sql, connection);

        if (parameters is not null)
        {
            command.Parameters.AddRange(parameters.ToArray());
        }

        connection.Open();
        return command.ExecuteNonQuery();
    }
}
