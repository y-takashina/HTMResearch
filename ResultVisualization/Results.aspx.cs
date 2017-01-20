using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MoreLinq;

namespace ResultVisualization
{
    public partial class Results : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var dirs = new DirectoryInfo(".\\").GetDirectories();
            dirs.ForEach(dir => dropDownList.Items.Add(new ListItem(dir.ToString())));
        }
    }
}