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

namespace TreeSpeak_AndOnly
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        //.NET parameters
        DataTable active_CSV_file = new DataTable();


        //Xamarin parameters
        private static int CHOOSE_FILE_REQUESTCODE = 8777;
        private static int PICKFILE_RESULT_CODE = 8778;

        FrameLayout frame_home;
        FrameLayout frame_dashboard;
        FrameLayout frame_log;

        TextView textview_status;

        ListView log_listview;
        string[] items;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            frame_home = FindViewById<FrameLayout>(Resource.Id.frame_home);
            frame_dashboard = FindViewById<FrameLayout>(Resource.Id.frame_dashboard);
            frame_log = FindViewById<FrameLayout>(Resource.Id.frame_log);

            Button upload_btn = (Button)FindViewById(Resource.Id.dashboard_uploadBtn);

            textview_status = FindViewById<TextView>(Resource.Id.dashboard_file_status);

            upload_btn.Click += Upload_btn_Click;


            items = new string[] {"Test 1", "Test 2", "Test 3" };
            log_listview = FindViewById<ListView>(Resource.Id.log_listview);
            log_listview.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, items);

            log_listview.ItemClick += Log_listview_ItemClick;

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
        }

        private void Log_listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var t = items[e.Position];
            Toast.MakeText(this, t, ToastLength.Long).Show(); ;
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public bool OnNavigationItemSelected(IMenuItem item)
        {
            Fragment fragment = null;

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

        public virtual void SetOnClickListner(View.IOnClickListener l)
        {
            
        }

        public void SetUpdateStatusTest(string msg)
        {
            textview_status.Text = msg;
        }
    }
}

