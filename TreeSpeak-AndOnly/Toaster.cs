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
    public static class Toaster
    {
        static Context context = Application.Context;

        // Create toast with string message
        public static void MakeToast(string message, ToastLength toast_length)
        {
            Toast.MakeText(context, message, toast_length);
        }

        // Add more methods to handle messages in different date types.
    }
}