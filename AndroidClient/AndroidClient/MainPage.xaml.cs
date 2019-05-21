using ClientServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AndroidClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Client client = new Client(new ConnectionArguments("192.168.0.188", 998, '@', Convert.ToByte(';')));
            client.debug = new Action<string, int>((o, a) =>
            {
                
            });
            var msg = client.Communicate(new ClientMessage("repeat", "hello"));
            txt.Text = msg.message;
        }
    }
}
