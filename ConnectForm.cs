using Gtk;

// Клас ConnectForm унаслідує клас Gtk.Dialog
public partial class ConnectForm : Gtk.Dialog
{
    // Поля
    public string Host { get; private set; } // Хост
    public string DbName { get; private set; } // Назва бази даних
    public string UserName { get; private set; } // Ім'я користувача
    public string Password { get; private set; } // Пароль

    private readonly Builder builder = new(); // Об'єкт для створення і керування елементами інтерфейсу
    readonly Dialog win; // Вікно

    // Конструктор класу ConnectForm
    public ConnectForm() : base()
    {
        builder.AddFromFile("./forms/Forms.glade"); 

        win = (Dialog)builder.GetObject("connectForm"); 

        var connectToDB = (Button)builder.GetObject("connectToDB");
        var cancel = (Button)builder.GetObject("cancel"); 

        connectToDB.Clicked += On_connectToDB_clicked; 
        cancel.Clicked += On_cancel_clicked;
 
        win.Show(); 
    }

    // Метод для обробки події натискання кнопки підключення до БД
    protected void On_connectToDB_clicked(object sender, EventArgs e)
    {
        var hostText = (Entry)builder.GetObject("hostText");
        var dbNameText = (Entry)builder.GetObject("DBNameText"); 
        var userNameText = (Entry)builder.GetObject("userNameText");
        var passwordText = (Entry)builder.GetObject("usrPassText");

        Host = hostText.Text; 
        DbName = dbNameText.Text; 
        UserName = userNameText.Text; 
        Password = passwordText.Text; 

        this.Respond(ResponseType.Ok); 
        win.Hide(); 
        Application.Quit(); 
    }

    // Метод для обробки події натискання кнопки "Скасувати"
    protected void On_cancel_clicked(object sender, EventArgs e)
    {
        this.Respond(ResponseType.Cancel); 
        win.Hide(); 
        Application.Quit(); 
    }
}
