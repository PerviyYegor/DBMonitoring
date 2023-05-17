using Gtk;

//Клас, що описує форму, що аутентифікує користувача при його підключенні до застосунку
public partial class LoginForm : Gtk.Dialog
{
    private readonly Builder builder = new();
    readonly Window win;

    public string login { get; private set; } //Логін працівника
    public string password { get; private set; }//Пароль працівника

    //Конструктор класу форми LoginForm
    public LoginForm() : base()
    {
        builder.AddFromFile("./forms/Forms.glade");

        win = (Window)builder.GetObject("loginForm");
        InitTriggers();


        win.Show();
    }

    //Ініціювання тригерів натискань на кнопки
    private void InitTriggers()
    {
        var loginB = (Button)builder.GetObject("loginUserButton");
        var cancelB = (Button)builder.GetObject("cancelLogin");

        loginB.Clicked += OnLoginClicked;
        cancelB.Clicked += OnCancelClicked;
    }

    //Тригер натискання на кнопку аутентифікації працівника
    private void OnLoginClicked(object sender, System.EventArgs args)
    {
        var loginField = (Entry)builder.GetObject("loginText");
        var passwordField = (Entry)builder.GetObject("passwordText");

        login = loginField.Text;
        password = passwordField.Text;

        win.Hide();
        Application.Quit();
    }

    //Тригер натискання на кнопку відміни аутентифікації
    private void OnCancelClicked(object sender, System.EventArgs args)
    {
        win.Hide();
        Application.Quit();
    }

}