using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Speech;
using Android.Content;
using Android.Content.Res;

using Xamarin.Essentials;

namespace TreeSpeak_V2
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

        public string GetSurveyType(Table table) => table.name;
        


        public void PopulateTable(SurveyHelper.TableType table_type)
        {
            string table_name = null;

            switch (table_type)
            {
                case SurveyHelper.TableType.BS5837:
                    table_name = "bs5837_table.txt";
                    break;
                case SurveyHelper.TableType.GeneralSurvey:
                    table_name = "general_survey_table.txt";
                    break;
                default:
                    return;
            }

            AssetManager assets = Application.Context.Assets;

            Table temp_table = new Table();
            TableCategory temp_cat = new TableCategory();

            using (StreamReader reader = new StreamReader(assets.Open(table_name)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (isFileStart(line))
                        while (reader.ReadLine() != "[End]") { }
                    else if (isTableStart(line))
                        temp_table.name = reader.ReadLine();
                    else if (isTableEnd(line))
                    {
                        temp_table.categories.Add(temp_cat);
                        //value_tables.Add(temp_table);
                        SurveyHelper.values_tables.Add(temp_table);
                        temp_table = new Table();
                        temp_cat = new TableCategory();
                    }
                    else if (isTableCategory(line))
                    {
                        if (temp_cat.items.Count() > 0)
                        {
                            temp_table.categories.Add(temp_cat);
                            temp_cat = new TableCategory();
                        }
                        temp_cat.category_name = line;
                    }
                    else if (isTableItem(line))
                        temp_cat.items.Add(line);
                    else if (line == "[Topic]")
                        break;
                }
            }


        }

        Boolean isTableStart(string line)
        {
            if (line == "[Table]")
                return true;
            else
                return false;
        }
        Boolean isTableEnd(string line)
        {
            if (line == "[End]")
                return true;
            else
                return false;
        }
        Boolean isTableCategory(string line)
        {
            if (line.Contains("Category="))
                return true;
            else
                return false;
        }
        Boolean isTableItem(string line)
        {
            if (line.Contains("Item="))
                return true;
            else
                return false;
        }
        Boolean isFileStart(string line)
        {
            if (line == "[Survey]")
                return true;
            else
                return false;
        }
    }
    
}