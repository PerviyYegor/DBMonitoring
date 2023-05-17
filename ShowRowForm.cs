using Gtk;

//Клас форми, що відображає один рядок таблиці
public partial class ShowRowForm : Gtk.Window
{
    public string[] Values { get; private set; }//Поле, що містить дані, що були введені під час дії вікна

    private readonly Builder builder = new();
    readonly Window win;
    
     //Конструктор класу ShowRowForm, що приймає назви колонок та значення одного рядка для цих колонок
    public ShowRowForm(string[] headers, string[] values) : base(Gtk.WindowType.Toplevel)
    {
        Values = values;
        builder.AddFromFile("./forms/Forms.glade");
        win = (Window)builder.GetObject("ShowRowForm");
        var table = (TreeView)builder.GetObject("RowsTree");

        var saveB = (Button)builder.GetObject("SaveButton");
        var discardB = (Button)builder.GetObject("DiscardButton");


        saveB.Clicked += (sender, args) =>
        {
            win.Hide();
            Application.Quit();
        };

        discardB.Clicked += (sender, args) =>
        {
            Values = null;
            win.Hide();
            Application.Quit();
        };

        DisplayTable(headers, values);
        win.Show();
    }
    //Конструктор класу ShowRowForm, що приймає тільки назви колонок
    public ShowRowForm(string[] headers) : base(Gtk.WindowType.Toplevel)
    {
        builder.AddFromFile("./forms/Forms.glade");
        win = (Window)builder.GetObject("ShowRowForm");
        var table = (TreeView)builder.GetObject("RowsTree");

        var saveB = (Button)builder.GetObject("SaveButton");
        var discardB = (Button)builder.GetObject("DiscardButton");
        Values = new string[headers.Length];


        saveB.Clicked += (sender, args) =>
        {
            win.Hide();
            Application.Quit();
        };

        discardB.Clicked += (sender, args) =>
        {
            Values = null;
            win.Hide();
            Application.Quit();
        };

        DisplayTable(headers, new object[headers.Length]);
        win.Show();
    }
    //Відображення даного рядку в таблицю
    private void DisplayTable(string[] headers, object[] values)
    {

        var table = (TreeView)builder.GetObject("RowsTree");
        var ts = new TreeStore(typeof(string), typeof(string));
        foreach (var column in table.Columns)
        {
            table.RemoveColumn(column);
        }

        table.AppendColumn(new TreeViewColumn("Headers", new CellRendererText(), "text", 0));


        var editableCell = new Gtk.CellRendererText();
        editableCell.Editable = true;
        editableCell.Edited += (sender, args) =>
        {
            if(args.NewText==null) return;
            values[Convert.ToInt32(args.Path)] = args.NewText;
            Values[Convert.ToInt32(args.Path)] = args.NewText;
            
            DisplayTable(headers, values);
            win.Show();
        };

        var vals = new TreeViewColumn("Values", editableCell, "text", 1);

        table.AppendColumn(vals);

        for (int i = 0; i < headers.Length; i++)
            ts.AppendValues(new string[] { headers[i], values[i]==null? "" :values[i].ToString() });

        table.Model = ts;
    }
}