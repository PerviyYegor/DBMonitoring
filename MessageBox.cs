using Gtk;

//Клас форми MessageBox, що виконує просте виведення інформації на екран
public partial class MessageBox : Gtk.Dialog
{
    //Конструктор класу форми MessageBox
    public MessageBox(string message) : base()
    {
        var dialog = new MessageDialog(
            this,
            DialogFlags.Modal,
            MessageType.Info,
            ButtonsType.Ok,
            message);

        this.SetDefaultSize(300, -1);
        this.Resizable = false;
        this.BorderWidth = 12;
        dialog.Run();
        dialog.Destroy();
    }
}