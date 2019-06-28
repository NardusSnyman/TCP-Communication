using ClientServer;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace CrossPlat
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Client client = new Client(ConnectionArguments.fromLocal(998, 1024));
            client.debug = new Action<string>((o) =>
            {

            });
            var msg = client.Communicate("repeat", "hello");
            txt.Text = msg;
        }
    }
}
