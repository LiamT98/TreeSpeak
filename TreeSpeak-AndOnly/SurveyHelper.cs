using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using NotVisualBasic.FileIO;
using Debug = System.Diagnostics.Debug;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace TreeSpeak_V2
{
    public static class SurveyHelper
    {
        public static string[] general_survey_headers = { "Easting", "Northing", "GMSurveyV111", "Tree ID", "Tag No", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height (m)", "Stem Dia (mm)", "Spread Radius (m)", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf/Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey (months)", "QTRA Base Score 1/", "Time", "Date", "Comment" };
        public static string[] bs5837_survey_headers = { "Easting", "Northing", "BS5837V111", "Tree ID", "Tag Number", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Maturity", "Likely Bat Habitat", "Measurements Estimated", "Height (m)", "Height and direction of first significant branch (m)", "Number of Stems", "Stem 1 (mm) Enter average diameter for trees with more than 5 stems", "Stem 2 (mm)", "Stem 3 (mm)", "Stem 4 (mm)", "Stem 5 (mm)", "Spread - N (m)", "Spread - E (m)", "Spread - S (m)", "Spread - W (m)", "CH - N (m)", "CH - E (m)", "CH - S (m)", "CH - W (m)", "Crown", "Stem", "Basal Area", "Category", "Life Expectancy", "Subcategories", "Phys Condition", "Build Stage", "Category 1", "Action 1", "Time 1", "Category 2", "Action 2", "Time 2", "Category 3", "Action 3", "Time 3", "Category 4", "Action 4", "Time 4", "Priority", "Next Inspection (months)", "Time", "Date", "Comment" };


        public static string[] general_survey_headers_useable = { "Easting", "Northing", "GMSurveyV111", "Tree ID", "Tag Number", "TPO Number", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height", "Stem Diameter", "Spread Radius", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf and Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey", "QTRA Base Score", "Time", "Date", "Comment" };
        public static string[] bs5837_survey_headers_useable = { "Easting", "Northing", "BS5837V111", "Tree ID", "Tag Number", "TPO Number", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Maturity", "Likely Bat Habitat", "Measurements Estimated", "Height", "Height and direction of first significant branch", "Number of Stems", "Stem 1", "Stem 2", "Stem 3", "Stem 4", "Stem 5", "Spread North", "Spread East", "Spread South", "Spread West", "CH North", "CH East", "CH South", "CH West", "Crown", "Stem", "Basal Area", "Category", "Life Expectancy", "Subcategories", "Physical Condition", "Build Stage", "Category 1", "Action 1", "Time 1", "Category 2", "Action 2", "Time 2", "Category 3", "Action 3", "Time 3", "Category 4", "Action 4", "Time 4", "Priority", "Next Inspection", "Time", "Date", "Comment" };
        public enum TableType
        {
            BS5837,
            GeneralSurvey
        }

        public static class QueryTypes
        {
            public const string SESSION_START = "SESSION START";
            public const string SESSION_END = "SESSION END";
            public const string EDIT_RECORD = "EDIT RECORD";
            public const string ADD_RECORD = "ADD RECORD";
            public const string DELETE_RECORD = "DELETE RECORD";
            public const string GET_TREE = "SELECT TREE";
        }

        public static class ActiveSurvey
        {
            // Survey meta data
            public static Guid survey_id { get; set; }

            public static string survey_name { get; set; }

            public static DateTime survey_lastmodified { get; set; }

            public static TableType survey_type { get; set; }

            // If active, populate DataTable
            public static DataTable survey_datatable { get; set; }
        }
        



        



        // survey type possible values tables
        public static List<SurveyTables.Table> values_tables = new List<SurveyTables.Table>();


        

        public static void LoadSurveyFromCSV(string fileName, Stream fileData, out string message)
        {
            ClearActiveSurvey();

            DataTable temp_datatable = new DataTable();

            try
            {
                using (CsvTextFieldParser csvReader = new CsvTextFieldParser(fileData))
                {
                    csvReader.SetDelimiter(',');
                    csvReader.HasFieldsEnclosedInQuotes = true;

                    string[] colfields = csvReader.ReadFields();
                    foreach (string column in colfields)
                    {
                        DataColumn dColumn = new DataColumn(column);
                        dColumn.AllowDBNull = true;
                        temp_datatable.Columns.Add(dColumn);
                    }

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();

                        //Make empty values NULL
                        for (int i = 0; i < fieldData.Length; i++)
                            if (fieldData[i] == "")
                                fieldData[i] = null;

                        temp_datatable.Rows.Add(fieldData);
                    }
                }

                ActiveSurvey.survey_id = Guid.NewGuid();
                ActiveSurvey.survey_name = GetCopyFileName(fileName);
                ActiveSurvey.survey_type = temp_datatable.Columns[2].ColumnName == "BS5837V111" ? TableType.BS5837 : TableType.GeneralSurvey;
                ActiveSurvey.survey_datatable = temp_datatable; // warning, clone instead of assign?
                
                SQLiteHelper.CreateSurveyTable(ActiveSurvey.survey_name, ActiveSurvey.survey_type);
                
                SurveyTables tables = new SurveyTables();
                tables.PopulateTable(ActiveSurvey.survey_type);

                message = "Survey successfully loaded";
            }
            catch (Exception ex)
            {
                Context context = Application.Context;
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();

                message = "Cannot import survey";
            }
        }

        public static string GetCopyFileName(string file_name)
        {
            string copy_name = file_name;

            bool needs_copy;

            int highest_iteration = 0;

            List<string> copies = SQLiteHelper.GetTableCopies(file_name);

            // the survey has only been uploaded once before
            if (copies.Count() == 1)
                return copy_name += "-|-" + 1;

            if (copies.Count() > 1)
            {
                foreach (string str in copies)
                {
                    if (str.Split("-|-").Count() > 1)
                    {
                        if (int.Parse(str.Split("-|-")[1]) > highest_iteration)
                        {
                            highest_iteration = int.Parse(str.Split("-|-")[1]);
                        }
                    }
                }
            }
            else
                return copy_name;

            highest_iteration++;
            copy_name += "-|-" + highest_iteration;

            return copy_name;
        }

        public static void ClearActiveSurvey()
        {
            ActiveSurvey.survey_name = null;
            ActiveSurvey.survey_lastmodified = DateTime.Now;
            ActiveSurvey.survey_datatable = null;
        }

        public static async void ExportTableToCSV()
        {
            

            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                {
                    AlertDialog.Builder dialog = new AlertDialog.Builder(Application.Context);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Storage Permissions Required");
                    alert.SetMessage("To export a survey you must first grant storage permissions.");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        CrossPermissions.Current.OpenAppSettings();
                    });
                    alert.SetButton2("Maybe later", (c, ev) =>
                    {
                        Toast.MakeText(Application.Context, "Permission denied for now.", ToastLength.Long);
                    });
                }

                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
            }
            if (status == PermissionStatus.Unknown)
            {
                Toast.MakeText(Application.Context, "Permission denied, cannot export survey.", ToastLength.Long);
                return;
            }



            bool available = false;

            string state = Android.OS.Environment.GetExternalStorageState(Android.OS.Environment.ExternalStorageDirectory);
            if (state == Android.OS.Environment.MediaMounted)
                Toast.MakeText(Application.Context, "Your external storage media is unavailable.", ToastLength.Long);


            //string csvFileName = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath, SurveyHelper.ActiveSurvey.survey_name) + ".csv";

            var appDirectory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TreeSpeak");
            Directory.CreateDirectory(appDirectory);

            string csvFileName = Path.Combine(appDirectory, SurveyHelper.ActiveSurvey.survey_name) + ".csv";


            if (!File.Exists(csvFileName))
            {
                FileStream fs = new FileStream(csvFileName, FileMode.CreateNew);
                fs.Close();

                string csvHeaders;
                if (ActiveSurvey.survey_type == TableType.GeneralSurvey)
                    csvHeaders = string.Join(',', general_survey_headers);
                else
                    csvHeaders = string.Join(',', bs5837_survey_headers);

                using (FileStream fsWHT = new FileStream(csvFileName, FileMode.Append, FileAccess.Write))
                using (StreamWriter swT = new StreamWriter(fsWHT))
                {
                    swT.WriteLine(csvHeaders);
                }
            }

            SQLiteHelper.ExportSurvey(csvFileName);
        }
    }
}