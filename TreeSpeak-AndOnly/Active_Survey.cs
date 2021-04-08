using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;


namespace TreeSpeak_AndOnly
{
     static class Active_Survey
    {
        // Survey meta data
        public static string survey_name { get; set; }

        public static DateTime survey_lastmodified { get; set; }

        // If active, populate DataTable
        public static DataTable active_survey { get; set; }


        public static void ClearActiveSurvey()
        {
            survey_name = null;
            survey_lastmodified = DateTime.Now;
            active_survey = null;
        }
    }

    
}