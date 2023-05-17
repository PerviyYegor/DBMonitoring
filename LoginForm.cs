using Gtk;

public partial class LoginForm : Gtk.Dialog
{

    private readonly Builder builder = new();
    readonly Window win;

    public string login { get; private set; }
    public string password { get; private set; }

    public LoginForm() : base()
    {
        builder.AddFromFile("./forms/Forms.glade");

        win = (Window)builder.GetObject("loginForm");
        InitTriggers();


        win.Show();
    }

    private void InitTriggers()
    {
        var loginB = (Button)builder.GetObject("loginUserButton");
        var cancelB = (Button)builder.GetObject("cancelLogin");

        loginB.Clicked += OnLoginClicked;
        cancelB.Clicked += OnCancelClicked;
    }

    private void OnLoginClicked(object sender, System.EventArgs args)
    {
        var loginField = (Entry)builder.GetObject("loginText");
        var passwordField = (Entry)builder.GetObject("passwordText");

        login = loginField.Text;
        password = passwordField.Text;

        win.Hide();
        Application.Quit();
    }

    private void OnCancelClicked(object sender, System.EventArgs args)
    {
        win.Hide();
        Application.Quit();
    }

}