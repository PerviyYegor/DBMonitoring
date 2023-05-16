using Gtk;

public partial class CourierForm : Gtk.Window
{
    public string[] ColumnNames { get; private set; }

    private readonly Builder builder = new();
    private readonly string viewName = "CourierView";
    readonly Window win;
    public CourierForm() : base(Gtk.WindowType.Toplevel)
    {
        builder.AddFromFile("./forms/Forms.glade");

        win = (Window)builder.GetObject("CourierWindow");
        loadView("CourierView");

        InitTriggers();
        win.Show();
    }

    private void InitTriggers()
    {
        var table = (TreeView)builder.GetObject("tableTextC");
        var exitB = (Button)builder.GetObject("exitButtonC");

        table.RowActivated += OnRowActivated;

        exitB.Clicked += OnExitClicked;
    }

    private void OnExitClicked(object sender, System.EventArgs args)
    {
        Application.Quit();
    }

    private void OnRowActivated(object sender, RowActivatedArgs args)
    {
        var table = (TreeView)builder.GetObject("tableTextC");
        string prKeyName = Program.connection.GetColumnNames(viewName)[0];

        TreePath path = args.Path;
        int index = path.Indices[0];

        ITreeModel model = ((TreeView)sender).Model;


        if (model.GetIter(out TreeIter iter1, path))
            switch (AskUser($"Do you want to take {model.GetValue(iter1, GetColumnIndex(table, prKeyName))}th order?", "No", "Yeah!"))
            {
                case "Yeah!":
                    Application.Init();

                   if (Program.connection.UpdateRow(viewName, prKeyName, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyName)),
                "DeliveryActuality", "0")) {
                        _ = new MessageBox("Order is your!"); loadView(viewName);}
                    else _ = new MessageBox("Something went wrong :( \n Contact with your admin");

                    break;

                default:
                    return;
            }
    }
    protected void loadView(string viewName)
    {
        var table = (TreeView)builder.GetObject("tableTextC");

        foreach (var column in table.Columns)
        {
            table.RemoveColumn(column);
        }

        var columnsTittles = Program.connection.GetColumnNames(viewName);
        var columnsType = new List<Type>();


        for (int i = 0; i < columnsTittles.Length; i++)
        {
            table.AppendColumn(new TreeViewColumn(columnsTittles[i], new CellRendererText(), "text", i));
            columnsType.Add(typeof(string));
        }
        var ts = new TreeStore(columnsType.ToArray());

        var rows = Program.connection.GetRowsOfTable(viewName);
        foreach (var row in rows)
        {
            ts.AppendValues(row);
        }

        table.Model = ts;
    }
    static string AskUser(string question, string option1, string option2)
    {
        var dialog = new Gtk.MessageDialog(
             null,
             DialogFlags.Modal,
             MessageType.Question,
             ButtonsType.None,
             question
         );

        dialog.AddButton(option1, ResponseType.Yes);
        dialog.AddButton(option2, ResponseType.No);

        ResponseType response = (ResponseType)dialog.Run();
        dialog.Destroy();

        if (response == ResponseType.Yes)
            return option1;
        else if (response == ResponseType.No)
            return option2;
        else return null;
    }
    static private int GetColumnIndex(TreeView table, string tittle)
    {
        for (int i = 0; i < table.Columns.Length; i++)
            if (table.Columns[i].Title == tittle) return i;
        return -1;
    }
}
