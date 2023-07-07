using MySqlConnector;

namespace yabm;

public class Database
{
    public static MySqlConnection GetConnection()
    {
        string host = (Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost");
        string user = (Environment.GetEnvironmentVariable("DB_USER") ?? "root");
        string pass = (Environment.GetEnvironmentVariable("DB_PASS") ?? "root");
        string db = (Environment.GetEnvironmentVariable("DB_NAME") ?? "test");

        var connection = new MySqlConnection($"Server={host};Database={db};User ID={user};Password={pass};ConnectionTimeout=5;");
        
        connection.Open();
        
        return connection;
    }
}