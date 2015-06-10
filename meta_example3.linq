<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

void Main()
{
    var factory = new ObjectEditorControlFactory();

    var foo = new Foo
    {
        Bar = "abc",
        Baz = new Baz
        {
            Value = true
        }
    };
    
    var fooEditorControl = factory.GetObjectEditorControl(foo);
    fooEditorControl.Height = 200;
    fooEditorControl.Width = 400;
    
    // This button exists so we can see the values of our object.
    var showButton = new Button { Text = "Show" };
    showButton.Click += (sender, args) => foo.Dump();
    
    var flowLayoutPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown };
    flowLayoutPanel.Controls.Add(fooEditorControl);
    flowLayoutPanel.Controls.Add(showButton);
    // End of stuff for showing values of our object
    
    PanelManager.DisplayControl(flowLayoutPanel);
}

public class Foo
{
    public Baz Baz { get; set; }
    public string Bar { get; set; }
    public string Qux { get; set; }
}

public class Baz
{
    public bool Value { get; set; }
}

public class ObjectEditorControlFactory
    {
        public Control GetObjectEditorControl(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return GetObjectEditorControl(obj.GetType(), obj);
        }

        private Control GetObjectEditorControl(Type type, object value)
        {
            var groupBox = new GroupBox() { Text = type.Name };
            var flowLayoutPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill };
            groupBox.Controls.Add(flowLayoutPanel);

            foreach (var property in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
            {
                var p = property;
                var propertyValue = property.GetValue(value);

                if (property.PropertyType == typeof(string))
                {
                    var label = new Label { Text = property.Name };

                    var textBox = new TextBox { Width = 200, Text = (string)propertyValue };
                    textBox.TextChanged += (sender, args) => p.SetValue(value, textBox.Text);

                    flowLayoutPanel.Controls.Add(label);
                    flowLayoutPanel.Controls.Add(textBox);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox { Text = property.Name, Width = 200, Checked = (bool)propertyValue };
                    checkBox.CheckedChanged += (sender, args) => p.SetValue(value, checkBox.Checked);

                    flowLayoutPanel.Controls.Add(checkBox);
                }
                // TODO: include more types
                else
                {
                    flowLayoutPanel.Controls.Add(GetObjectEditorControl(property.PropertyType, propertyValue));
                }
            }

            return groupBox;
        }
    }