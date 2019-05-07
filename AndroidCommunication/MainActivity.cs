using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Net.Sockets;
using System;
using ClientServer;

namespace AndroidCommunication
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TextView log;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            log = FindViewById<TextView>(Resource.Id.log);
            CommunicateToServer();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void CommunicateToServer()
        {
            
            Client client = new ClientServer.Client(new ConnectionArguments("192.168.0.187", 998));
            client.debug = new Action<string, int>((o, a) =>
            {
                log.SetText(o.ToCharArray(), 0, o.ToCharArray().Length);
            });
            var msg = client.Communicate(new ClientServer.ClientMessage("repeat", "hello"));
            var answer = ("answer:" + msg.message).ToCharArray();
            log.SetText(answer, 0, answer.Length);
        }
    }
}