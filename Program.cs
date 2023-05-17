using Gtk;
using System.Data;
using MySql.Data.MySqlClient;

class Program
{

    public static DBConnect connection;
    public static string idEmployeeConnection;

    static void Main()
    {
        ConnectWithConfigFile("./credentials/DBConnectData.txt");
        UserEnter();

        /*   ConnectWithConfigFile("./credentials/courier.txt");
        Application.Init();
        _ = new CourierForm();
        Application.Run();
         */

        Application.Init();
        var MF = new MainForm();
        Application.Run();
        

        connection.CloseConnection();
        Environment.Exit(0);
    }

    static bool UserEnter(){
        Application.Init();

        var LF = new LoginForm();
        Application.Run();

        foreach(var st in connection.GetRow("Passwords","login_", LF.login))
        Console.WriteLine(st);

        return true;
    }

    static DBConnect ConnectWithForm()
    {
        Application.Init();

        var cf = new ConnectForm();
        Application.Run();

        connection = new DBConnect(cf.Host, cf.DbName, cf.UserName, cf.Password);
        if (connection.OpenConnection())
        {
            _ = new MessageBox(String.Join(',', cf.Host, cf.DbName, cf.UserName, cf.Password));
            _ = new MessageBox("Connection successful!");
        }
        else
        {
            _ = new MessageBox("Something went wrong. Maybe some data was written wrong!");
            Environment.Exit(1);
        }

        connection.OpenConnection();
        return connection;
    }

    static DBConnect ConnectWithConfigFile(string pathToJsonFile)
    {
        Application.Init();
        _ = new MessageBox($"Trying connect to DB with info from here: {pathToJsonFile}");
        connection = new DBConnect(pathToJsonFile);
        if (connection.OpenConnection())
        {
            _ = new MessageBox("Connection successful!");
        }
        else
        {
            _ = new MessageBox("Something went wrong. Maybe some data was written wrong or file doesn't exist!");
            Environment.Exit(1);
        }
        connection = new DBConnect(pathToJsonFile);
        connection.OpenConnection();
        return connection;
    }

}