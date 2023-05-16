using Gtk;

public partial class ConnectForm : Gtk.Dialog
{
    public string Host { get; private set; }
    public string DbName { get; private set; }
    public string UserName { get; private set; }
    public string Password { get; private set; }
    private readonly Builder builder = new();
    Dialog win;
    public ConnectForm() : base()
    {
        builder.AddFromFile("./forms/Forms.glade");

        win = (Dialog)builder.GetObject("connectForm");

        var connectToDB = (Button)builder.GetObject("connectToDB");
        var cancel = (Button)builder.GetObject("cancel");

        connectToDB.Clicked += on_connectToDB_clicked;
        cancel.Clicked += on_cancel_clicked;

        win.Show();
    }

    protected void on_connectToDB_clicked(object sender, EventArgs e)
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

    protected void on_cancel_clicked(object sender, EventArgs e)
    {
        this.Respond(ResponseType.Cancel);
        win.Hide();
        Application.Quit();
    }
}
