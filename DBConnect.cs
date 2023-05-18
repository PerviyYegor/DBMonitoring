using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;


//Клас, що містить дані та методи для їх отримання задля підключення до БД
public class ConnectionData
{
    public string Host { get; private set; }//Адреса підключення до БД
    public string DbName { get; private set; }//Ім'я БД
    public string UserName { get; private set; }//Ім'я користувача до якого приєднується підключення до БД
    public string Password { get; private set; }//Пароль користувача до якого приєднується підключення до БД

    //Конструктор класу ConnectionData, якому подаються всі поля для підключення
    public ConnectionData(string server_, string database_, string uid_, string password_)
    {
        Host = server_;
        DbName = database_;
        UserName = uid_;
        Password = password_;
    }

    //Конструктор класу ConnectionData, що бере дані з файлу за даним шляхом
    public ConnectionData(string pathToTxtFile)
    {
        var text = "";
        using (var reader = new StreamReader(pathToTxtFile))
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

//Клас обгортка для взаємодії з базою даних
public class DBConnect
{
    //Поле, що містить посилання на діюче підключення до БД
    private MySqlConnection connection;

    //Поле, що містить дані про діюче підключення до БД
    private readonly ConnectionData CD;

    //Конструктор класу DBConnect, якому подаються всі поля для підключення
    public DBConnect(string server_, string database_, string uid_, string password_)
    {
        CD = new ConnectionData(server_, database_, uid_, password_);
        Initialize();
    }

    //Конструктор класу ConnectionData, що бере дані з файлу за даним шляхом
    public DBConnect(string pathToTxtFile)
    {
        CD = new ConnectionData(pathToTxtFile);
        Initialize();
    }

    //Ініціалізація підключення до бази даних
    private void Initialize()
    {
        string connectionString;
        connectionString = $"Database={CD.DbName};Server={CD.Host};Uid={CD.UserName};Pwd={CD.Password};charset=utf8mb4";

        connection = new MySqlConnection(connectionString);
    }

    //Відкриття підключення до БД
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

    //Закриття підключення до БД
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

    //Отримання імен таблиць, що містяться в БД
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

    //Отримання імен стовпців, що містяться в таблиці
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

    //Отримання типів колонок в таблиці
    public string[] GetColumnTypes(string tableName)
    {
        var columns = new List<string>();

        using (var cmd = new MySqlCommand($"SHOW COLUMNS FROM {tableName}", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
                columns.Add(reader.GetString(1));
            reader.Close();
        }

        return columns.ToArray();
    }

    //Отримання імені колонки, що містить клочове значення
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

    //Отримання всіх рядків, що містяться в таблиці
    public string[][] GetRowsOfTable(string tableName)
    {
        var rows = new List<string[]>();
        var colTypes = GetColumnTypes(tableName);

        using (var cmd = new MySqlCommand($"SELECT * FROM {tableName}", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (!reader.IsDBNull(i))
                        if (colTypes[i] == "datetime") { row[i] = string.Concat(ConvertDateFormat(reader.GetMySqlDateTime(i).ToString()[..10]), reader.GetMySqlDateTime(i).ToString().AsSpan(10, 9)); }
                        else if (colTypes[i] == "date") { row[i] = ConvertDateFormat(reader.GetMySqlDateTime(i).ToString()); }
                        else
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

    //Видалення рядка в таблиці по значенню в конкретному стовпці
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

    //Отримання одного рядку, що міститься в таблиці по значенню в конкретному стовпці
    public string[] GetRow(string tableName, string columnName, string value)
    {
        var row = new List<string>();
        var colTypes = GetColumnTypes(tableName);

        using (var cmd = new MySqlCommand($"SELECT * FROM `{tableName}` WHERE `{columnName}`=@Value;", connection))
        {
            cmd.Parameters.AddWithValue("@Value", value);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (colTypes[i] == "datetime")
                        {
                            row.Add(string.Concat(ConvertDateFormat(reader.GetMySqlDateTime(i).ToString()[..10]), reader.GetMySqlDateTime(i).ToString().AsSpan(10, 9)));
                        }
                        else if (colTypes[i] == "date") { row.Add(ConvertDateFormat(reader.GetMySqlDateTime(i).ToString())); }
                        else
                            row.Add(reader.GetString(i)); // Додаємо значення кожної колонки в рядок
                    }
                }
                reader.Close();
            }
        }

        return row.ToArray();
    }

    //Включення нового рядка
    public bool InsertRow(string tableName, string[] colNames, string[] values)
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

    //Оновлення рядка в таблиці по значенню в конкретному стовпці, де подаються одразу декілька значень, що треба оновити
    public bool UpdateRow(string tableName, string columnName, string value, string[] updateColNames, string[] updateValues)
    {
        try
        {
            string updStr = "";
            for (int i = 0; i < updateColNames.Length; i++)
            {
                var newValue = updateValues[i] == "" ? "@valueNull" : updateValues[i] = $"'{updateValues[i]}'";
                updStr += updateColNames[i] + $"={newValue}, ";
            }
            updStr = updStr[..^2];

            string query = $"UPDATE {tableName} SET {updStr} WHERE {columnName}='{value}'";
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@valueNull", DBNull.Value);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    //Оновлення рядка в таблиці по значенню в конкретному стовпці, де подається одне значення, що треба оновити
    public bool UpdateRow(string tableName, string columnName, string value, string updateColName, string updateValue)
    {
        try
        {
            string query = $"UPDATE {tableName} SET {updateColName}= @value WHERE {columnName}='{value}'";
            var cmd = new MySqlCommand(query, connection);
            if (updateValue == null || updateValue == "")
                cmd.Parameters.AddWithValue("@value", DBNull.Value);
            else cmd.Parameters.AddWithValue("@value", updateValue);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    //Перевірка чи конкретна таблиця є переглядом
    public bool isTableView(string tableName)
    {
        using (var cmd = new MySqlCommand($"SELECT table_type FROM information_schema.tables WHERE table_name = '{tableName}';", connection))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(0) == "VIEW") { reader.Close(); return true; }
                }
                reader.Close();
                return false;
            }
        }
    }

    //Отримання інформації про конкретного кур'єра за його ID кур'єра
    public string GetCourierInfo(string courierID)
    {
        var courierEmpID = Program.connection.GetRow("Couriers", "idCourier", courierID)[1];
        return $"Courier ID:{courierEmpID}\nCourier {GetEmployeeInfo(courierEmpID)}";
    }

    //Отримання інформації про конкретного адміна за його ID адміна
    public string GetAdminInfo(string adminID)
    {
        var adminEmpID = Program.connection.GetRow("Admins", "idAdmin", adminID)[1];
        return $"Admin ID:{adminEmpID}\nAdmin {GetEmployeeInfo(adminEmpID)}";
    }

    //Отримання інформації про конкретного співробітника за його ID співробітника
    public string GetEmployeeInfo(string employeeID)
    {

        var empInfoRow = GetRow("Employees", "idEmployee", employeeID);
        return $"Employee ID:{empInfoRow[0]}\n{empInfoRow[1]} {empInfoRow[2]}\nPhone:{empInfoRow[4]}";
    }
    //Конвертація з типу, що дає mysqlconnector до типу, з яким працює mysql
    private static string ConvertDateFormat(string stringDate)
    {
        DateTime inputDate = DateTime.ParseExact(stringDate, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        return inputDate.ToString("yyyy-MM-dd");
    }
}

