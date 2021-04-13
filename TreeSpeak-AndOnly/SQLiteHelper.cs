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

using Mono.Data.Sqlite;

namespace TreeSpeak_V2
{
    public static class SQLiteHelper
    {
        static string db_name = "Single_Instance_Survey_Database";
        static string db_path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), db_name + ".db3");

        static SqliteConnection connection;

        /// <summary>
        /// Create single main database
        /// </summary>
        public static void CreateApplicationDatabase()
        {
            // Create the ONLY database file to store all new and old survey files
            // Surveys will take the form of a new table
            if (!File.Exists(db_path))
                SqliteConnection.CreateFile(db_path);
             
        }


        public static void CreateSurveyTable(string table_name, int survey_type)
        {
            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            if (!File.Exists(db_path))
                CreateApplicationDatabase();

            if (CheckTableExistance(table_name))
                return; // A table of the same name already exists


            string sql_statement = $"CREATE table [{table_name}] (";

            foreach (DataColumn col in Active_Survey.active_survey.Columns)
            {
                sql_statement += "[" + col.ColumnName + "]" + "nvarchar(255)" + ",";
            }

            sql_statement = sql_statement.TrimEnd(',') + ")";

            SqliteCommand command = new SqliteCommand(sql_statement, connection);
            SqliteDataAdapter data_adapter = new SqliteDataAdapter(command);

            command.ExecuteNonQuery();

            connection.Close();

            //using (var adapter = new SqliteDataAdapter($"SELECT * FROM {table_name}", connection))
            //using (var builder = new SqliteCommandBuilder(adapter))
            //{
            //    adapter.InsertCommand = builder.GetInsertCommand();
            //    adapter.Update(Active_Survey.active_survey);
            //}

                switch (survey_type)
                {
                    case 0: //BS5837
                        CreateBS5837Table(table_name);
                        break;
                    case 1: //General Survey
                        CreateGeneralSurveyTable(table_name);
                        break;
                }
        }

        private static void CreateBS5837Table(string table_name)
        {
            
        }

        private static void CreateGeneralSurveyTable(string table_name)
        {

        }

        private static bool CheckTableExistance(string table_name)
        {
            bool table_exists = false;

            using (SqliteCommand get_all_tables = new SqliteCommand(connection))
            {
                get_all_tables.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name = '{table_name}'";

                using (SqliteDataReader reader = get_all_tables.ExecuteReader())
                {
                    while (reader.Read())
                        table_exists = true;
                }
            }

            return table_exists;
        }

        private static string GetAppendedConnString()
        {
            return string.Concat("Data Source=", db_path);
        }
    }
}