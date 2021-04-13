using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Speech;
using Android.Content;

// Additonal libraries
using Xamarin.Essentials;
using NotVisualBasic.FileIO;
using System;
using System.Data;
using SQLite;
using AlertDialog = Android.App.AlertDialog;

namespace TreeSpeak_V2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
            //.NET parameters
        DataTable active_CSV_file = new DataTable();
        private bool isRecording;

        //Xamarin parameters
        private static int CHOOSE_FILE_REQUESTCODE = 8777;
        private static int PICKFILE_RESULT_CODE = 8778;
        private readonly int VOICE = 10;

        FrameLayout frame_home;
        FrameLayout frame_dashboard;
        FrameLayout frame_log;

        Button upload_btn;
        Button record_btn;

        TextView textview_status;
        TextView textview_record_status;

        ListView log_listview;
        string[] items;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Get the control resources from the layout
            frame_home = FindViewById<FrameLayout>(Resource.Id.frame_home);
            frame_dashboard = FindViewById<FrameLayout>(Resource.Id.frame_dashboard);
            frame_log = FindViewById<FrameLayout>(Resource.Id.frame_log);

            upload_btn = (Button)FindViewById(Resource.Id.dashboard_uploadBtn);
            record_btn = (Button)FindViewById(Resource.Id.dashboard_recordBtn);

            textview_status = FindViewById<TextView>(Resource.Id.dashboard_file_status);
            textview_record_status = FindViewById<TextView>(Resource.Id.dashboard_record_status);

            upload_btn.Click += Upload_btn_Click;
            record_btn.Click += Record_btn_Click;


            items = new string[] {"Test 1", "Test 2", "Test 3" };
            log_listview = FindViewById<ListView>(Resource.Id.log_listview);
            log_listview.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, items);

            log_listview.ItemClick += Log_listview_ItemClick;

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
                // Read contents of CSV file into byte stream
                var stream = await result.OpenReadAsync();

                SurveyLoader.LoadSurvey(result.FileName, stream, out string message);

                textview_status.Text = message;
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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultCode == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];
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
            var t = items[e.Position];
            Toast.MakeText(this, t, ToastLength.Long).Show(); ;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

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
    }
}


