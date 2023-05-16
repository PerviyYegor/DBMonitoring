using System.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System;
using System.Xml.Linq;
using System.Linq;
using Gtk;

public partial class MainForm : Gtk.Window
{
    public string[] ColumnNames { get; private set; }

    private readonly Builder builder = new();
    readonly Window win;
    public MainForm() : base(Gtk.WindowType.Toplevel)
    {
        builder.AddFromFile("./forms/Forms.glade");

        win = (Window)builder.GetObject("MainWindow");
        var tableNameBox = (ComboBoxText)builder.GetObject("tableNameText");
        InitTriggers();

        if (Program.connection.GetTableNames().Length != 0)
        {
            foreach (var t in Program.connection.GetTableNames())
                tableNameBox.AppendText(t);

            tableNameBox.Active = 0;

            win.Show();
        }
        else Console.WriteLine("Your database is empty and there is no tables");
    }

    private void InitTriggers()
    {
        var tableNameBox = (ComboBoxText)builder.GetObject("tableNameText");
        var table = (TreeView)builder.GetObject("tableText");
        var reloadB = (Button)builder.GetObject("reloadButton");
        var insertB = (Button)builder.GetObject("insertButton");
        var exitB = (Button)builder.GetObject("exitButton");

        tableNameBox.Changed += tableNameText_changed_cb;
        table.RowActivated += OnRowActivated;
        table.RowActivated += tableNameText_changed_cb;
        reloadB.Clicked += tableNameText_changed_cb;
        insertB.Clicked += OnInsertClicked;
        insertB.Clicked += tableNameText_changed_cb;
        exitB.Clicked += OnExitClicked;
    }

    private void OnInsertClicked(object sender, System.EventArgs args)
    {
        var tableNameBox = (ComboBoxText)builder.GetObject("tableNameText");
        Application.Init();
        var mf = new ShowRowForm(Program.connection.GetColumnNames(tableNameBox.ActiveText));
        Application.Run();
        if (mf.Values is null) return;
        if (Program.connection.InsertRow(tableNameBox.ActiveText, Program.connection.GetColumnNames(tableNameBox.ActiveText), mf.Values))
            _ = new MessageBox("Insert successful!");
        else _ = new MessageBox("Insert unsuccessful :("); ;
    }

    private void OnExitClicked(object sender, System.EventArgs args)
    {
        Application.Quit();
    }

    private void OnRowActivated(object sender, RowActivatedArgs args)
    {
        var tableNameBox = (ComboBoxText)builder.GetObject("tableNameText");
        var table = (TreeView)builder.GetObject("tableText");

        TreePath path = args.Path;
        int index = path.Indices[0];

        ITreeModel model = ((TreeView)sender).Model;

        switch (AskUser("What do you want do with row?", "Modify", "Delete"))
        {
            case ("Modify"):
                Application.Init();
                string prKeyColumn = Program.connection.GetPrimaryKeyColName(tableNameBox.ActiveText);
                if (model.GetIter(out TreeIter iter1, path))
                {
                    var mf = new ShowRowForm(Program.connection.GetColumnNames(tableNameBox.ActiveText),
                Program.connection.GetRow(tableNameBox.ActiveText, prKeyColumn, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyColumn))));
                    Application.Run();
                    if (mf.Values == null) return;

                    if (Program.connection.UpdateRow(tableNameBox.ActiveText, prKeyColumn, (string)model.GetValue(iter1, GetColumnIndex(table, prKeyColumn)),
                Program.connection.GetColumnNames(tableNameBox.ActiveText), mf.Values))
                        _ = new MessageBox("Update successful!");
                    else _ = new MessageBox("Update unsuccessful :(");
                }
                break;
            case ("Delete"):
                string prKey = Program.connection.GetPrimaryKeyColName(tableNameBox.ActiveText);
                if (model.GetIter(out TreeIter iter2, path))
                {
                    if (Program.connection.DeleteRow(tableNameBox.ActiveText, prKey, (string)model.GetValue(iter2, GetColumnIndex(table, prKey))))
                        _ = new MessageBox("Delete successful!");
                    else _ = new MessageBox("Delete unsuccessful :(");
                }
                break;
            case (null):
                return;
        }
    }
    protected void tableNameText_changed_cb(object sender, EventArgs e)
    {
        var tableNameBox = (ComboBoxText)builder.GetObject("tableNameText");
        var table = (TreeView)builder.GetObject("tableText");

        foreach (var column in table.Columns)
        {
            table.RemoveColumn(column);
        }

        var columnsTittles = Program.connection.GetColumnNames(tableNameBox.ActiveText);
        var columnsType = new List<Type>();


        for (int i = 0; i < columnsTittles.Length; i++)
        {
            table.AppendColumn(new TreeViewColumn(columnsTittles[i], new CellRendererText(), "text", i));
            columnsType.Add(typeof(string));
        }
        var ts = new TreeStore(columnsType.ToArray());

        var rows = Program.connection.GetRowsOfTable(tableNameBox.ActiveText);
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
