using Gtk;

//Клас, що ініціалізує програму та викликає потрібні блоки
class Program
{
    public static DBConnect connection; //поле, що містить посилання на поточне підключення до БД
    public static string idEmployeeConnection; //ID аутентифікованого працівника
    public static bool isUserAdmin;//поле, що містить інформацію чи аутентифікований працівник є адміністратором

    //Метод, що ініціалізує програму та викликає потрібні блоки
    static void Main()
    {
        ConnectWithConfigFile("./credentials/DBConnectData.txt");
        if (UserEnter())
            if (isUserAdmin)
            {
                Application.Init();
                var MF = new MainForm();
                Application.Run();
            }
            else
            {
                Application.Init();
                _ = new CourierForm();
                Application.Run();
            }
        else
        {
            _ = new MessageBox("Authentication unsuccessful! Please try again");
            Main();
        }
        connection.CloseConnection();
        Environment.Exit(0);
    }
    //Блок, що здійснює аутентифікацію користувача
    static bool UserEnter()
    {
        Application.Init();

        var LF = new LoginForm();
        Application.Run();

        var userData = connection.GetRow("Passwords", "login_", LF.login);

        if (userData.Length != 0)
        {
            switch (userData[4])
            {
                case "1": isUserAdmin = true; break;
                case "0": isUserAdmin = false; break;
                default: return false;
            }
            idEmployeeConnection = userData[1];
        }
        else
            return false;
        return true;
    }
    //Блок, що здійснює підключення через окрему форму
    static DBConnect ConnectWithForm()
    {
        Application.Init();

        var cf = new ConnectForm();
        Application.Run();
        if (!(cf.Host == "" || cf.DbName == "" || cf.UserName == "" || cf.Password == ""))
            connection = new DBConnect(cf.Host, cf.DbName, cf.UserName, cf.Password);

        if (connection != null && connection.OpenConnection())
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
    //Блок, що здійснює підключення через конфігураційний файл
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