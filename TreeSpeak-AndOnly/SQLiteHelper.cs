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
        static string db_name = "Single_Instance_Survey_Database.sqlite";
        static string db_path = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, db_name);
        //static string db_path = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath, db_name);

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


        public static bool CreateSurveyTable(string table_name, SurveyHelper.TableType survey_type)
        {
            if (!File.Exists(db_path))
                CreateApplicationDatabase();

            if (CheckTableExistance(table_name))
                return false; // A table of the same name already exists

            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            string create_table_statement = $"CREATE table [{table_name}] (";

            string populate_table_statement = $"INSERT INTO [{SurveyHelper.ActiveSurvey.survey_name}](";



            foreach (DataColumn col in SurveyHelper.ActiveSurvey.survey_datatable.Columns)
            {
                //create_table_statement += "[" + col.ColumnName + "]" + "nvarchar(255)" + ",";
                create_table_statement += $"[{col.ColumnName}] nvarchar(255),";

                populate_table_statement += $"[{col.ColumnName}], ";
            }

            create_table_statement = create_table_statement.TrimEnd(',') + ");";

            SqliteCommand command = new SqliteCommand(create_table_statement, connection);

            command.ExecuteNonQuery();




            populate_table_statement = populate_table_statement.TrimEnd(' ', ',');

            populate_table_statement += ") VALUES (";


            //populate_table_statement = populate_table_statement.TrimEnd(' ', ',');

            //populate_table_statement += ");";


            
            //populate_table_statement += ") VALUES (";

            using (var transaction = connection.BeginTransaction())
            using (var populate_command = connection.CreateCommand())
            {

                foreach (DataRow dataRow in SurveyHelper.ActiveSurvey.survey_datatable.Rows)
                {
                    string statement_values = null;

                    foreach (object obj in dataRow.ItemArray)
                    {
                        //populate_table_statement += $"'{obj}', ";
                        statement_values += $"'{obj}', ";
                    }

                    statement_values = statement_values.TrimEnd(' ', ',');
                    statement_values += ");";

                    populate_command.CommandText = populate_table_statement + statement_values;

                    populate_command.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            connection.Close();
            return true;
        }

        public static bool CheckTableExistance(string table_name)
        {
            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            bool table_exists = false;

            using (SqliteCommand get_all_tables = new SqliteCommand(connection))
            {
                get_all_tables.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name = '{table_name}';";

                using (SqliteDataReader reader = get_all_tables.ExecuteReader())
                {
                    while (reader.Read())
                        table_exists = true;
                }
            }

            connection.Close();

            return table_exists;
        }

        public static List<string> GetTableCopies(string table_name)
        {
            List<string> table_names = new List<string>();

            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            bool table_exists = false;

            using (SqliteCommand get_all_tables = new SqliteCommand(connection))
            {
                get_all_tables.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '{table_name}';";

                using (SqliteDataReader reader = get_all_tables.ExecuteReader())
                {
                    while (reader.Read())
                        table_names.Add(reader.GetString(0));
                }
            }

            connection.Close();

            return table_names;

        }

        public static bool CheckTreeExists(string tree_id)
        {
            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            bool rtn_val = false;

            using (SqliteCommand get_tree_id = new SqliteCommand(connection))
            {
                //get_tree_id.CommandText = $"SELECT 1 FROM [{SurveyHelper.survey_name}] WHERE [Tree ID]='{tree_id}';";

                get_tree_id.CommandText = $"SELECT EXISTS(SELECT 1 FROM [{SurveyHelper.ActiveSurvey.survey_name}] WHERE [Tree ID]='{tree_id}')";

                using (SqliteDataReader reader = get_tree_id.ExecuteReader())
                    while (reader.Read())
                        rtn_val = true;
                
            }

            connection.Close();
            return rtn_val;
        }

        public static string GetTreeOldValue(string tree_id, string property_header)
        {
            //CheckTableExistance(SurveyHelper.ActiveSurvey.survey_name);

            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();

            string return_val = null;

            using (SqliteCommand get_old_val = new SqliteCommand(connection))
            {
                get_old_val.CommandText = $"SELECT [{property_header}] FROM [{SurveyHelper.ActiveSurvey.survey_name}] WHERE [Tree ID]='{tree_id}';";

                using (SqliteDataReader reader = get_old_val.ExecuteReader())
                {
                    while (reader.Read())
                        return_val = reader.GetString(0);
                }
            }


            connection.Close();
            return return_val;

        }

        //public static string GetTreeOldValue(string tree_id, string column_header)
        //{
        //    connection = new SqliteConnection(GetAppendedConnString());
        //    connection.Open();

        //    string return_val = null;

        //    using (SqliteCommand get_old_val = new SqliteCommand(connection))
        //    {
        //        get_old_val.CommandText = string.Format("SELECT [{0}] FROM [{1}] WHERE [Tree ID]='{2}';", column_header, SurveyHelper.ActiveSurvey.survey_name, tree_id);

        //        using (SqliteDataReader reader = get_old_val.ExecuteReader())
        //        {
        //            while (reader.Read())
        //                return reader.GetString(0);
        //        }
        //    }

        //    return null;

        //}

        public static bool ExecuteNLQ(NLQ query)
        {
            bool isSuccess = true;

            switch (query.query_type)
            {
                case SurveyHelper.QueryTypes.ADD_RECORD:
                    isSuccess = ExecuteAddRecord(query);
                    break;
                case SurveyHelper.QueryTypes.EDIT_RECORD:
                    isSuccess = ExecuteEditRecord(query);
                    break;
                case SurveyHelper.QueryTypes.DELETE_RECORD:
                    isSuccess = ExecuteDeleteRecord(query);
                    break;
            }

            return isSuccess;
        }

        private static bool ExecuteAddRecord(NLQ query)
        {
            if (!CheckTableExistance(SurveyHelper.ActiveSurvey.survey_name))
                return false;
            if (!CheckTreeExists(query.tree_id))
                return false;

            try
            {

                connection = new SqliteConnection(GetAppendedConnString());
                connection.Open();


                using (SqliteCommand command = new SqliteCommand(connection))
                {
                    /*
                    To implement adding a new record efficiently it might be a good idea to design
                    another Speech Processing segment that's optimised for stating a new TreeID
                    and then subsequently recording a list of parameters that are then added to a list in 
                    the NLQ class.
                    The Add query can then be contained within one NLQ instance where the parameter list is 
                    iterated through when adding paramenters to a prepared INSERT statement.
                    */
                }
                return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Add query for {query.tree_id} has failed.", ToastLength.Long);
                return false;
            }
        }
        private static bool ExecuteEditRecord(NLQ query)
        {
            if (!CheckTableExistance(SurveyHelper.ActiveSurvey.survey_name))
                return false;
            if (!CheckTreeExists(query.tree_id))
                return false;

            try
            {

                connection = new SqliteConnection(GetAppendedConnString());
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(connection))
                {
                    command.CommandText = $"UPDATE [{SurveyHelper.ActiveSurvey.survey_name}] SET [{query.property_actioned}] = :propval WHERE [Tree ID] = :treeid;";
                    command.Parameters.Add("propval", DbType.String).Value = query.property_val_new;
                    command.Parameters.Add("treeid", DbType.String).Value = query.tree_id;

                    command.ExecuteNonQuery();
                }

                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Edit query for {query.tree_id} has failed.", ToastLength.Long);
                return false;
            }
        }
        private static bool ExecuteDeleteRecord(NLQ query)
        {
            if (!CheckTableExistance(SurveyHelper.ActiveSurvey.survey_name))
                return false;
            if (!CheckTreeExists(query.tree_id))
                return false;

            try
            {

                connection = new SqliteConnection(GetAppendedConnString());
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(connection))
                {
                    command.CommandText = $"DELETE FROM [{SurveyHelper.ActiveSurvey.survey_name}] WHERE [Tree ID] = :treeid";
                    command.Parameters.Add("treeid", DbType.String).Value = query.tree_id;

                    command.ExecuteNonQuery();
                }

                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Delete query for {query.tree_id} has failed.", ToastLength.Long);
                return false;
            }
        }


        public static void ExportSurvey(string fileName)
        {
            connection = new SqliteConnection(GetAppendedConnString());
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(connection))
            {
                command.CommandText = $"SELECT * FROM [{SurveyHelper.ActiveSurvey.survey_name}];";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                         string csvLine = reader[0].ToString() + ",";

                        for (int i = 1; i < reader.FieldCount; i++)
                        {
                            csvLine += reader[i].ToString() + ",";
                        }

                        using (FileStream fsWDT = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                        using (StreamWriter swDT = new StreamWriter(fsWDT))
                        {
                            //write csv line to file.
                            swDT.WriteLine(csvLine.ToString());
                        }
                    }
                }
            }
        }


        private static string GetAppendedConnString()
        {
            return string.Concat("Data Source=", db_path);
        }
    }
}