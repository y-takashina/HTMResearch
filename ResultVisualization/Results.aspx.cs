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
        private const string RootPath = @"C:\Users\ytakashina\Documents\Visual Studio 2015\Projects\HTMResearch\DetectorTests\bin\results";
        public string DataPath;

        protected void Page_Load(object sender, EventArgs e)
        {
            var dirs = new DirectoryInfo(RootPath).GetDirectories();
            dirs.ForEach(dir => dropDownList.Items.Add(new ListItem(dir.ToString())));
            DataPath = RootPath + @"\" + dropDownList.SelectedValue + @"\";
        }

        protected void dropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataPath = RootPath + @"\" + dropDownList.SelectedValue + @"\";
        }
    }
}