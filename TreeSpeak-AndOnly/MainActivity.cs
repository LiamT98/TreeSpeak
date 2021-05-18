using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Speech;
using Android.Content;
using Android.Provider;

// Additonal libraries
using Xamarin.Essentials;
using NotVisualBasic.FileIO;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using AlertDialog = Android.App.AlertDialog;
using System.Threading.Tasks;
using System.IO;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace TreeSpeak_V2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        // Control parameters
        FrameLayout frame_home;
        FrameLayout frame_dashboard;
        RelativeLayout frame_log;

        Button upload_btn;
        Button record_btn;

        TextView textview_status;
        TextView textview_record_status;

        EditText edittext_treeid;
        EditText edittext_property;
        EditText edittext_oldval;
        EditText edittext_newval;

        ViewFlipper vf;

        ListView log_listview;
        SessionAdapter session_listview_adapter;

        ListView listview_detailed_log;
        NLQListAdapter NLQ_listview_adapter;
        Button button_execute_all;

        TextView textview_itemdetail_title;
        TextView textview_itemdetail_qtype;
        EditText edittext_itemdetail_prop;
        EditText edittext_itemdetail_oldval;
        EditText edittext_itemdetail_newval;
        Button button_itemdetail_save;

        Button button_export;



        //.NET parameters
        private bool isRecording = false;
        // store the session list index to fetch the correct query list when saving
        private int session_position;
        // Store the query index to use when saving any edits made
        private int query_postion;

        private CurrentPage curr_page = CurrentPage.HOME;

        // Stores all the parameters that make up a session. Most importantly the queries made to the database which allows
        // a user to undo or rectify changes made to the database.
        List<InterpretationInstance> sessions = new List<InterpretationInstance>();

        //Xamarin parameters
        private static int CHOOSE_FILE_REQUESTCODE = 8777;
        private static int PICKFILE_RESULT_CODE = 8778;
        private readonly int VOICE = 10;

        enum CurrentPage
        {
            HOME,
            DETAIL_LIST_VIEW,
            DETAIL_QUERY_CARD
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            

            // Get the control resources from the layout
            frame_home = FindViewById<FrameLayout>(Resource.Id.frame_home);
            frame_dashboard = FindViewById<FrameLayout>(Resource.Id.frame_dashboard);
            frame_log = FindViewById<RelativeLayout>(Resource.Id.frame_log);


            upload_btn = (Button)FindViewById(Resource.Id.dashboard_uploadBtn);
            record_btn = (Button)FindViewById(Resource.Id.dashboard_recordBtn);


            textview_status = FindViewById<TextView>(Resource.Id.dashboard_file_status);
            textview_record_status = FindViewById<TextView>(Resource.Id.dashboard_record_status);


            upload_btn.Click += Upload_btn_Click;
            record_btn.Click += Record_btn_Click;


            edittext_treeid = FindViewById<EditText>(Resource.Id.current_q_treeid_edittext);
            edittext_property = FindViewById<EditText>(Resource.Id.current_q_property_edittext);
            edittext_oldval = FindViewById<EditText>(Resource.Id.current_q_oldval_edittext);
            edittext_newval = FindViewById<EditText>(Resource.Id.current_q_newval_edittext);


            log_listview = FindViewById<ListView>(Resource.Id.log_listview);
            session_listview_adapter = new SessionAdapter(sessions);
            log_listview.Adapter = session_listview_adapter;
            log_listview.ItemClick += Log_listview_ItemClick;


            listview_detailed_log = FindViewById<ListView>(Resource.Id.detailed_listview);
            listview_detailed_log.ItemClick += Listview_detailed_log_ItemClick;
            button_execute_all = FindViewById<Button>(Resource.Id.button_exe_all);
            button_execute_all.Click += Button_execute_all_Click;


            textview_itemdetail_title = FindViewById<TextView>(Resource.Id.query_detail_name);
            textview_itemdetail_qtype = FindViewById<TextView>(Resource.Id.query_detail_querytype_val);
            edittext_itemdetail_prop = FindViewById<EditText>(Resource.Id.query_detail_property_val);
            edittext_itemdetail_oldval = FindViewById<EditText>(Resource.Id.query_detail_oldval_val);
            edittext_itemdetail_newval = FindViewById<EditText>(Resource.Id.query_detail_newval_val);
            button_itemdetail_save = FindViewById<Button>(Resource.Id.query_detail_save_button);
            button_itemdetail_save.Click += Button_itemdetail_save_Click;


            vf = FindViewById<ViewFlipper>(Resource.Id.viewflipper_logview);


            button_export = FindViewById<Button>(Resource.Id.button_export);
            button_export.Click += Button_export_Click;
            
            
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            
        }

        

        private async void Upload_btn_Click(object sender, System.EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a .csv file"
            });

            if (result != null)
            {
                if (SQLiteHelper.CheckTableExistance(result.FileName))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                    alert.SetTitle("Survey already exists");
                    alert.SetMessage("This survey has been uploaded before.\nDo you want to continue?\nNote: This will not overwrite the existing file.");
                    alert.SetPositiveButton("Yes", async (senderAlert, args) =>
                    {
                        // Read contents of CSV file into byte stream
                        var stream = await result.OpenReadAsync();
                        // creates a new database table for the survey but the 
                        SurveyHelper.LoadSurveyFromCSV(result.FileName, stream, out string message);

                        textview_status.Text = message;
                    });
                    alert.SetNegativeButton("No", (senderAlert, args) => 
                    {
                        return;
                    });

                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
                else
                {
                    // Read contents of CSV file into byte stream
                    var stream = await result.OpenReadAsync();

                    SurveyHelper.LoadSurveyFromCSV(result.FileName, stream, out string message);

                    textview_status.Text = message;
                }
            }

        }

        private void Record_btn_Click(object sender, EventArgs e)
        {
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                var alert = new AlertDialog.Builder(record_btn.Context);
                alert.SetTitle("No microphone on the device was detected.");
                alert.SetPositiveButton("OK", (sender2, e2) =>
                {
                    textview_record_status.Text = "No microphone detected!";
                    record_btn.Enabled = false;
                    return;
                });
            }
            else
            {
                record_btn.Click += delegate
                {
                    record_btn.Text = "End recording";
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        // Create an intent and start the activity
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        // put a message on the modal dialog
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.message_speaknow));

                        // if there is more then 1.5s of silence, consider the speech over
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };
            }
        }

        private void Button_export_Click(object sender, EventArgs e)
        {
            if (SurveyHelper.ActiveSurvey.survey_name != null)
                SurveyHelper.ExportTableToCSV();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.TitleFormatted == null)
            {
                switch (curr_page)
                {
                    case CurrentPage.HOME:
                        break;
                    case CurrentPage.DETAIL_LIST_VIEW:
                        vf.ShowPrevious();
                        curr_page = CurrentPage.HOME;
                        SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                        break;
                    case CurrentPage.DETAIL_QUERY_CARD:
                        vf.ShowPrevious();
                        curr_page = CurrentPage.DETAIL_LIST_VIEW;
                        break;
                }
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultCode == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        // get an open session or create a new one
                        InterpretationInstance working_instance = GetActiveSession();

                        working_instance.ProcessSpeech(matches[0], out NLQ current_query);

                        // update the current query information display
                        UpdateCurrentQuery(current_query);
                        session_listview_adapter.NotifyDataSetChanged();

                        // update the list instance
                        int index = sessions.FindIndex(x => x.session_id == working_instance.session_id);
                        sessions[index] = working_instance;
                    }
                    else
                    {
                        textview_record_status.Text = "No speech was recognized.";
                    }

                    record_btn.Text = "Record";
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void Log_listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            PopulateDetailedListView(e.Position);
            session_position = e.Position;

            vf.ShowNext();
            curr_page = CurrentPage.DETAIL_LIST_VIEW;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void Button_execute_all_Click(object sender, EventArgs e)
        {
            InterpretationInstance instance = session_listview_adapter[session_position];

            List<NLQ> queries_to_execute = instance.session_queries.Where(x => 
            x.query_type == SurveyHelper.QueryTypes.EDIT_RECORD ||
            x.query_type == SurveyHelper.QueryTypes.ADD_RECORD ||
            x.query_type == SurveyHelper.QueryTypes.DELETE_RECORD).ToList();

            foreach (NLQ query in queries_to_execute)
                SQLiteHelper.ExecuteNLQ(query);
        }

        private void Listview_detailed_log_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            

            PopulateDetailedItemCard(e.Position);
            query_postion = e.Position;

            vf.ShowNext();
            curr_page = CurrentPage.DETAIL_QUERY_CARD;
        }

        private void Button_itemdetail_save_Click(object sender, EventArgs e)
        {
            sessions[session_position].session_queries[query_postion].property_actioned = edittext_itemdetail_prop.Text;
            sessions[session_position].session_queries[query_postion].property_val_old = edittext_itemdetail_oldval.Text;
            sessions[session_position].session_queries[query_postion].property_val_new = edittext_itemdetail_newval.Text;

            NLQ_listview_adapter.NotifyDataSetChanged();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public bool OnNavigationItemSelected(IMenuItem item)
        {

            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    set_active_frame(frame_home.Id);
                    return true;
                case Resource.Id.navigation_dashboard:
                    set_active_frame(frame_dashboard.Id);
                    return true;
                case Resource.Id.navigation_notifications:
                    set_active_frame(frame_log.Id);
                    return true;
            }
            return false;
        }

        private void set_active_frame(int itemId)
        {
            switch(itemId)
            {
                case Resource.Id.frame_home:
                    frame_home.Visibility = ViewStates.Visible;
                    frame_dashboard.Visibility = ViewStates.Invisible;
                    frame_log.Visibility = ViewStates.Invisible;
                    break;
                case Resource.Id.frame_dashboard:
                    frame_home.Visibility = ViewStates.Invisible;
                    frame_dashboard.Visibility = ViewStates.Visible;
                    frame_log.Visibility = ViewStates.Invisible;
                    break;
                case Resource.Id.frame_log:
                    frame_home.Visibility = ViewStates.Invisible;
                    frame_dashboard.Visibility = ViewStates.Invisible;
                    frame_log.Visibility = ViewStates.Visible;
                    break;
            }
        }

        public void SetUpdateStatusTest(string msg)
        {
            textview_status.Text = msg;
        }

        private InterpretationInstance GetActiveSession()
        {
            foreach (InterpretationInstance session in sessions)
                if (session.session_isOpen)
                    return session; // potentially add checks for multiple open instances

            InterpretationInstance new_instance = new InterpretationInstance()
            {
                session_id = Guid.NewGuid().ToString(),
                survey_id = SurveyHelper.ActiveSurvey.survey_id,
                survey_name = SurveyHelper.ActiveSurvey.survey_name,
                session_datetime = DateTime.Now,
                session_isOpen = true
            };

            sessions.Add(new_instance);
            return new_instance;
        }

        private void UpdateCurrentQuery(NLQ current_query)
        {
            edittext_treeid.Text = current_query.tree_id;
            edittext_property.Text = current_query.property_actioned;
            edittext_oldval.Text = current_query.property_val_old;
            edittext_newval.Text = current_query.property_val_new;
        }


        private void PopulateDetailedListView(int session_index)
        {
            var instance = session_listview_adapter[session_index];

            NLQ_listview_adapter = new NLQListAdapter(instance.session_queries);
            listview_detailed_log.Adapter = NLQ_listview_adapter;
        }

        private void PopulateDetailedItemCard(int item_index)
        {
            var query = NLQ_listview_adapter[item_index];

            textview_itemdetail_title.Text = query.tree_id;
            textview_itemdetail_qtype.Text = query.query_type;

            edittext_itemdetail_prop.Text = query.property_actioned;
            edittext_itemdetail_oldval.Text = query.property_val_old;
            edittext_itemdetail_newval.Text = query.property_val_new;
        }



        public class SessionAdapter : BaseAdapter<InterpretationInstance>
        {
            List<InterpretationInstance> sessions;

            public SessionAdapter(List<InterpretationInstance> sessions)
            {
                this.sessions = sessions;
            }

            public override InterpretationInstance this[int position]
            {
                get { return sessions[position]; }
            }

            public override int Count
            {
                get { return sessions.Count(); }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView;

                if (view == null)
                {
                    view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ListItem, parent, false);

                    var name = view.FindViewById<TextView>(Resource.Id.nameTextView);
                    var date = view.FindViewById<TextView>(Resource.Id.dateTextView);

                    view.Tag = new ViewHolder() { session_name = name, session_date = date };
                }

                var holder = (ViewHolder)view.Tag;

                holder.session_name.Text = sessions[position].survey_name + " - " + sessions[position].tree_id;
                holder.session_date.Text = sessions[position].session_datetime.ToString();

                return view;
            }


            public class ViewHolder : Java.Lang.Object
            {
                public TextView session_name { get; set; }
                public TextView session_date { get; set; }
            }

            
        }

        public class NLQListAdapter : BaseAdapter<NLQ>
        {
            List<NLQ> queries;

            public NLQListAdapter(List<NLQ> queries)
            {
                this.queries = queries;
            }

            public override NLQ this[int position]
            {
                get { return queries[position]; }
            }

            public override int Count
            {
                get { return queries.Count(); }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView;

                if (view == null)
                {
                    view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ListItem, parent, false);

                    var detail = view.FindViewById<TextView>(Resource.Id.nameTextView);
                    var type = view.FindViewById<TextView>(Resource.Id.dateTextView);

                    view.Tag = new ViewHolder() { query_title = detail, query_type = type};
                }

                var holder = (ViewHolder)view.Tag;

                holder.query_title.Text = queries[position].tree_id;
                holder.query_type.Text = queries[position].query_type;

                //holder.query_property.Text = queries[position].property_actioned;
                //holder.query_oldval.Text = queries[position].property_val_old;
                //holder.query_newval.Text = queries[position].property_val_new;


                return view;
            }

            public class ViewHolder : Java.Lang.Object
            {
                public TextView query_title { get; set; }
                public TextView query_type { get; set; }

                //public EditText query_property { get; set; }
                //public EditText query_oldval { get; set; }
                //public EditText query_newval { get; set; }
            }
        }

    }
}


