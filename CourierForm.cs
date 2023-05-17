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
        LoadView(viewName);
        fillLabels();
        InitTriggers();
        win.Show();
    }


    private void fillLabels()
    {
        var courierInfoLabel = (Label)builder.GetObject("courierInfoLabel");
        var adminInfoLabel = (Label)builder.GetObject("AdminOfCourierInfo");
        var courierRow = Program.connection.GetRow("Couriers", "idEmployee", Program.idEmployeeConnection);
       
        adminInfoLabel.Text = "Info about you:\n"+Program.connection.GetCourierInfo(courierRow[0]);
        courierInfoLabel.Text = "Info about your admin:\n"+Program.connection.GetAdminInfo(courierRow[2]);
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
        string prKeyName = Program.connection.GetPrimaryKeyColName("Orders");

        TreePath path = args.Path;

        ITreeModel model = ((TreeView)sender).Model;


        if (model.GetIter(out TreeIter iter1, path))
            switch (AskUser($"Do you want to take {model.GetValue(iter1, GetColumnIndex(table, prKeyName))}th order?", "No", "Yeah!"))
            {
                case "Yeah!":
                    Application.Init();

                    if (Program.connection.UpdateRow("Orders", prKeyName, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyName)),
                 new String[] { "DeliveryActuality", "idCourier" }, new String[] { "0", Program.connection.GetRow("Couriers", "idEmployee", Program.idEmployeeConnection)[0] }))
                    {
                        switch (AskUser("Order is your! When delivery will be ready just click necessary button below", "Cancel delivery", "Delivery done!"))
                        {
                            case "Cancel delivery":
                                Program.connection.UpdateRow("Orders", prKeyName, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyName)),
                     new String[] { "DeliveryActuality", "idCourier" }, new String[] { "1", "" });
                                break;
                            case "Delivery done!":
                                Program.connection.UpdateRow("Orders", prKeyName, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyName)),
                         "deliveryEnd", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


                                break;
                        }


                        LoadView(viewName);
                    }
                    else _ = new MessageBox("Something went wrong :( \n Contact with your admin");

                    break;

                default:
                    return;
            }
        LoadView(viewName);
    }
    protected void LoadView(string viewName)
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
