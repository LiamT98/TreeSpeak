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
    public static class SurveyHelper
    {
        public static string[] general_survey_headers = { "Easting", "Northing", "GMSurveyV111", "Tree ID", "Tag No", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height (m)", "Stem Dia (mm)", "Spread Radius (m)", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf/Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey (months)", "QTRA Base Score 1/", "Time", "Date", "Comment" };
        public static string[] bs5837_survey_headers = { "Easting", "Northing", "BS5837V111", "Tree ID", "Tag Number", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Maturity", "Likely Bat Habitat", "Measurements Estimated", "Height (m)", "Height and direction of first significant branch (m)", "Number of Stems", "Stem 1 (mm) Enter average diameter for trees with more than 5 stems", "Stem 2 (mm)", "Stem 3 (mm)", "Stem 4 (mm)", "Stem 5 (mm)", "Spread - N (m)", "Spread - E (m)", "Spread - S (m)", "Spread - W (m)", "CH - N (m)", "CH - E (m)", "CH - S (m)", "CH - W (m)", "Crown", "Stem", "Basal Area", "Category", "Life Expectancy", "Subcategories", "Phys Condition", "Build Stage", "Category 1", "Action 1", "Time 1", "Category 2", "Action 2", "Time 2", "Category 3", "Action 3", "Time 3", "Category 4", "Action 4", "Time 4", "Priority", "Next Inspection (months)", "Time", "Date", "Comment" };
    }
}