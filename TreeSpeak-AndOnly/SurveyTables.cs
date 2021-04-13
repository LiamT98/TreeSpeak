using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TreeSpeak_AndOnly
{
    public class SurveyTables
    {
        public class Table
        {
            public string name { get; set; }
            public List<TableCategory> categories = new List<TableCategory>(); 
        }
        public class TableCategory
        {
            public string category_name { get; set; }
            public List<string> items = new List<string>();
        }

        public List<Table> value_tables = new List<Table>();



        private string table_path = @"android.resource://treespeak-andonly";

        public string GetSurveyType(Table table) => table.name;
        


        public void PopulateTable(SurveyHelper.TableType table_type)
        {

        }
    }
    
}