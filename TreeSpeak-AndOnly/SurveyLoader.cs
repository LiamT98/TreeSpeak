using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using NotVisualBasic.FileIO;

namespace TreeSpeak_AndOnly
{
    static class SurveyLoader
    {
        public static void LoadSurvey(string fileName, Stream fileData, out string message)
        {
            Active_Survey.ClearActiveSurvey();

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

                Active_Survey.survey_name = Path.GetFileNameWithoutExtension(fileName);

                //foreach (DataColumn col in temp_datatable.Columns)
                //    Active_Survey.active_survey.Columns.Add(col);
                //foreach (DataRow row in temp_datatable.Rows)
                //    Active_Survey.active_survey.Rows.Add(row);

                Active_Survey.active_survey = temp_datatable; // warning, clone instead of assign?

                SQLiteHelper.CreateSurveyTable(fileName, 0);

                message = "Survey successfully loaded";
            }
            catch (Exception ex)
            {
                Context context = Application.Context;
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();

                message =  "Cannot import survey";
            }
        }
    }
}