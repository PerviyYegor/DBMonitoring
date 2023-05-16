﻿
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;


public class SMTH
{
    public string Host { get; private set; }
    public string DbName { get; private set; }
    public string UserName { get; private set; }
    public string Password { get; private set; }
}

public class ConnectionData
{
    public string Host { get; private set; }
    public string DbName { get; private set; }
    public string UserName { get; private set; }
    public string Password { get; private set; }

    public ConnectionData(string server_, string database_, string uid_, string password_)
    {
        Host = server_;
        DbName = database_;
        UserName = uid_;
        Password = password_;
    }

    public ConnectionData(string pathToJsonFile)
    {
        var text = "";
        using (var reader = new StreamReader(pathToJsonFile))
            text = reader.ReadToEnd();

        Regex regex = new Regex(@"(\w+):\s*(\S+)");
        MatchCollection matches = regex.Matches(text);

        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            string value = match.Groups[2].Value;

            switch (key)
            {
                case "Host":
                    Host = value;
                    break;
                case "DbName":
                    DbName = value;
                    break;
                case "UserName":
                    UserName = value;
                    break;
                case "Password":
                    Password = value;
                    break;
                default:
                    break;
            }
        }
    }
}

public class DBConnect
{
    private MySqlConnection connection;
    private readonly ConnectionData CD;

    //Constructor
    public DBConnect(string server_, string database_, string uid_, string password_)
    {
        CD = new ConnectionData(server_, database_, uid_, password_);
        Initialize();
    }

    public DBConnect(string pathToJsonFile)
    {
        CD = new ConnectionData(pathToJsonFile);
        Initialize();
    }

    //Initialize values
    private void Initialize()
    {
        string connectionString;
        connectionString = $"Database={CD.DbName};Server={CD.Host};Uid={CD.UserName};Pwd={CD.Password};charset=utf8mb4";

        connection = new MySqlConnection(connectionString);
    }

    //open connection to database
    public bool OpenConnection()
    {
        try
        {
            connection.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
        return true;
    }

    //Close connection
    public bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public string[] GetTableNames()
    {
        var tables = new List<string>();

        using (var cmd = new MySqlCommand("SHOW TABLES", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                tables.Add(reader.GetString(0));
            reader.Close();
        }

        return tables.ToArray();
    }

    public string[] GetColumnNames(string tableName)
    {
        var columns = new List<string>();

        using (var cmd = new MySqlCommand($"SHOW COLUMNS FROM {tableName}", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                columns.Add(reader.GetString(0));
            reader.Close();
        }

        return columns.ToArray();
    }



    public string GetPrimaryKeyColName(string tableName)
    {
        using (var cmd = new MySqlCommand($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE CONSTRAINT_NAME = 'PRIMARY' AND TABLE_NAME = '{tableName}';", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                return reader.GetString(0);
            reader.Close();
        }
        return null;
    }

    public string[][] GetRowsOfTable(string tableName)
    {
        var rows = new List<string[]>();

        using (var cmd = new MySqlCommand($"SELECT * FROM {tableName}", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (!reader.IsDBNull(i))
                        row[i] = reader.GetString(i);
                    else
                        row[i] = null;
                }
                rows.Add(row);
            }
            reader.Close();
        }

        return rows.ToArray();
    }


    //Delete statement
    public bool DeleteRow(string tableName, string columnName, string value)
    {
        try
        {
            string query = $"DELETE FROM {tableName} WHERE {columnName}=@value";
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string[] GetRow(string tableName, string columnName, string value)
    {
        var rows = new List<string>();

        using (var cmd = new MySqlCommand($"SELECT * FROM `{tableName}` WHERE `{columnName}`=@Value;", connection))
        {
            cmd.Parameters.AddWithValue("@Value", value); // Використовуємо параметри для уникнення SQL ін'єкцій

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        rows.Add(reader.GetString(i)); // Додаємо значення кожної колонки в рядок
                    }
                }
                reader.Close();
            }
        }

        return rows.ToArray();
    }



    //Insert statement
    public bool InsertRow(string tableName, string[] colNames, object[] values)
    {
        try
        {
            var colNameStr = string.Join(", ", colNames);
            var paramStr = string.Join(", ", colNames.Select(col => $"@{col}"));
            string query = $"INSERT INTO {tableName} ({colNameStr}) VALUES({paramStr})";
            var cmd = new MySqlCommand(query, connection);

            for (int i = 0; i < colNames.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@{colNames[i]}", values[i]);
            }

            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool UpdateRow(string tableName, string columnName, string value, string[] updateColNames, object[] updateValues)
    {
        try
        {
            string updStr = "";
            for (int i = 0; i < updateColNames.Length; i++)
                updStr += updateColNames[i] + $"='{updateValues[i]}', ";
            updStr = updStr[..^2];

            string query = $"UPDATE {tableName} SET {updStr} WHERE {columnName}='{value}'";
            var cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    static private string GetRightSqlValsString(object[] values)
    {
        return string.Join(", ", values.Select(val =>
        {
            return $"'{val}'"; // surround string with quotes
        }));
    }
}
