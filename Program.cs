using Gtk;
using System.Data;
using MySql.Data.MySqlClient;

class Program
{

    public static DBConnect connection;

    static void Main()
    {
        Application.Init();

        var cf = new ConnectForm();
        Application.Run();

        var DBConnect = new DBConnect(cf.Host, cf.DbName, cf.UserName, cf.Password);
        if (DBConnect.OpenConnection())
        {
            _ = new MessageBox(String.Join(',', cf.Host, cf.DbName, cf.UserName, cf.Password));
            _ = new MessageBox("Connection successful!");
        }
        else
        {
            _ = new MessageBox("Something went wrong. Maybe some data was written wrong!");
            Environment.Exit(1);
        }

        connection = new DBConnect(cf.Host, cf.DbName, cf.UserName, cf.Password);
        connection.OpenConnection();

        Application.Init();
        var mf = new MainForm();
        Application.Run();

        connection.CloseConnection();
        Environment.Exit(0);
    }
}