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

namespace TreeSpeak_V2
{
    public class InterpretationInstance
    {
        // to uniquely identify the stored session
        public string session_id { get; set; }

        // to bind any given session to a particular survey
        public Guid survey_id { get; set; }

        public string survey_name { get; set; }

        // the date and time at which the session began
        public DateTime session_datetime { get; set; }

        public bool session_isOpen { get; set; } = false;

        // user definable string that replaces the session dated title with a name.
        public string session_name { get; set; }

        public bool have_tree_id { get; set; } = false;

        public string tree_id { get; set; }


        public List<NLQ> session_queries = new List<NLQ>();

       

        public void ProcessSpeech(string input, out NLQ current_query)
        {
            switch (GetSessionDirective(input))
            {
                case SessionDirective.ADD_PROPERTY:

                    break;
                case SessionDirective.EDIT_PROPERTY:
                    NLQ start_query;
                    if (!have_tree_id)
                    {
                        // add NLQ to mark the start of a session
                        start_query = new NLQ();
                        start_query.query_type = SurveyHelper.QueryTypes.SESSION_START;
                        session_queries.Add(start_query);

                        SetActiveTree(input.Split(' ')[1]);

                        current_query = session_queries.Last();
                        return;
                    }

                    input = input.ToLower();

                    EditProperty(input);
                    break;

                case SessionDirective.DELETE_RECORD:

                    break;

                case SessionDirective.END_SESSION:
                    input = input.ToLower();

                    NLQ end_query = new NLQ();
                    end_query.query_type = SurveyHelper.QueryTypes.SESSION_END;
                    session_queries.Add(end_query);

                    session_isOpen = false;
                    break;

                case SessionDirective.NULL_Q:
                    current_query = session_queries.Last();
                    return;

                    //case SessionDirective.START_NEW_SESSION:
                    //    // add NLQ to mark the start of a session
                    //    NLQ start_query = new NLQ();
                    //    start_query.query_type = SurveyHelper.QueryTypes.SESSION_START;
                    //    session_queries.Add(start_query);

                    //    SetActiveTree(input);
                    //    break;
            }

            current_query = session_queries.Last();
        }

        private SessionDirective GetSessionDirective(string input)
        {
            //if (!have_tree_id)
            //    return SessionDirective.START_NEW_SESSION;
            //else if (input.ToLower() == "end session" && have_tree_id)
            //    return SessionDirective.END_SESSION;
            //else
            //    return SessionDirective.EDIT_PROPERTY;

            if (input.ToLower().StartsWith("Add"))
            {
                return SessionDirective.ADD_PROPERTY;
            }
            else if (input.ToLower().StartsWith("Edit"))
            {
                return SessionDirective.EDIT_PROPERTY;
            }
            else if (input.ToLower().StartsWith("Delete"))
            {
                return SessionDirective.DELETE_RECORD;
            }
            else if (input.ToLower() == "end session")
            {
                return SessionDirective.END_SESSION;
            }
            else
                return SessionDirective.NULL_Q;

        }

        #region GET TREE ID/SIGNS START OF QUERY SESSION

        public void SetActiveTree(string input)
        {
            NLQ new_query = new NLQ();

            new_query.query_type = SurveyHelper.QueryTypes.GET_TREE;
            // get tree id for new query
            new_query.tree_id = tree_id = GetValidTreeID(input);

            // store whether any input could be parsed as a tree id
            have_tree_id = string.IsNullOrEmpty(tree_id) ? false : true;

            // if no value can be validated as a potential tree id, return
            if (!have_tree_id)
            {
                Toaster.MakeToast($"Input is invalid.\nValue: '{input}'", ToastLength.Long);
                return;
            }
            // if a valid tree id does not return any table results, return
            if (!SQLiteHelper.CheckTreeExists(tree_id))
            {
                Toaster.MakeToast($"Cannot find tree record associated with ID: '{input}'", ToastLength.Long);
                return;
            }

            session_queries.Add(new_query);
        }
        private string GetValidTreeID(string input)
        {
            string rtn_str = null;

            if (!have_tree_id)
            {
                //input = SanitizeInput(input, SanitizeMode.TREE_ID, out object output);


                //if (input.StartsWith("tree id"))
                //    rtn_str = input.Split(" ")[2];

                rtn_str = input.Split(' ')[0];

                return rtn_str;
            }

            return rtn_str;
        }

        #endregion

        #region EDIT PROPERTY

        private void EditProperty(string input)
        {
            NLQ new_query = new NLQ();

            string property_to_edit = GetProperty(input);

            if (string.IsNullOrEmpty(property_to_edit))
            {
                // make toast
                return;
            }

            new_query.tree_id = tree_id;
            new_query.query_type = SurveyHelper.QueryTypes.EDIT_RECORD;
            new_query.property_actioned = property_to_edit;
            new_query.property_val_new = input.Substring(property_to_edit.Length);
            new_query.property_val_old = SQLiteHelper.GetTreeOldValue(tree_id, property_to_edit);

            session_queries.Add(new_query);
        }


        // suppose a method whereby each instance a header is detected
        // this is added to a list. This list is then iterated through
        // to edit all the headers detected with the values proceeding
        // the header up until the next header in the list is reached.
        // i.e. Height [10] | Stem [2] | Work Item 1 [Truncate trees by 1m] | etc.
        // in order to update more than one property 
        private string GetProperty(string input)
        {
            string rtn_string = null;

            //input = SanitizeInput(input, SanitizeMode.PROPERTY, out object output);

            if (SurveyHelper.ActiveSurvey.survey_type == SurveyHelper.TableType.GeneralSurvey)
            {
                for (int i = 0; i < SurveyHelper.general_survey_headers_useable.Count(); i++)
                    if (input.Contains(SurveyHelper.general_survey_headers_useable[i].ToLower()))
                        return SurveyHelper.general_survey_headers[i];
            }
            else
            {
                for (int i = 0; i < SurveyHelper.bs5837_survey_headers_useable.Count(); i++)
                    if (input.Contains(SurveyHelper.general_survey_headers_useable[i].ToLower()))
                        return SurveyHelper.bs5837_survey_headers[i];
            }


            return rtn_string;
        }

        #endregion

        private string SanitizeInput(string input, SanitizeMode mode, out object output)
        {
            string sanitized_string = null;
            
            switch (mode)
            {
                case SanitizeMode.TREE_ID:
                    sanitized_string = input;
                    foreach (string sub_str in input.Split(" "))
                        if (!int.TryParse(sub_str, out int whocares))
                        {
                            output = whocares;
                            if (sub_str != "tree" && sub_str != "id")
                                sanitized_string = sanitized_string.Replace(sub_str, "");
                        }
                    break;
                case SanitizeMode.PROPERTY:
                    sanitized_string = input;
                    foreach (string sub_str in input.Split(" "))
                    {
                        
                    }
                    
                    break;
            }
            output = null;
            return sanitized_string;
        }
    }
    public class NLQ
    {
        public int query_num { get; set; }

        public string query_type { get; set; }

        public string tree_id { get; set; }
        public string property_actioned { get; set; }
        public string property_val_old { get; set; }
        public string property_val_new { get; set; }
    }
    enum SanitizeMode
    {
        TREE_ID,
        PROPERTY,
        END
    }

    enum SessionDirective
    {
        START_NEW_SESSION,
        EDIT_PROPERTY,
        ADD_PROPERTY,
        DELETE_RECORD,
        END_SESSION,
        NULL_Q
    }
}