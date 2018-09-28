using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using TouchTracking;
using System;

namespace minicon_app
{
    public static class Params
    {
        static public byte spin;
        static public byte x;
        static public byte y;
        static public byte period;
        static public byte upper;
    }

    public partial class MainPage : ContentPage
    {
        double radius1, radius2, r;
        double x, y;

        double radius3, l;
        double spin;

        public MainPage()
        {
            InitializeComponent();
            sliderZ.Minimum = -150;
            sliderZ.Maximum = 0;
            sliderPeriod.Maximum = 1500;
            sliderPeriod.Minimum = 100;
            BindingContext = new MainPageViewModel();
        }

        private void ImageSizeChanged(object sender, System.EventArgs e)
        {
            radius1 = ((Image)sender).Width / 2;
            radius2 = radius1 / 5;
            x = y = radius1;
            r = radius1 - radius2;
            SetCircle2();
        }

        private void OnTouch1(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released && Params.period != 150)
                x = y = radius1;
            else
            {
                x = args.Location.X;
                y = args.Location.Y;
                spin = l / 2 + radius3;
                SetCircle3();
            }
            SetCircle2();
        }
        private void OnTouch2(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released && Params.period != 150)
                x = y = radius1;
            else
            {
                x += args.Location.X - radius2;
                y += args.Location.Y - radius2;
                spin = l / 2 + radius3;
                SetCircle3();
            }
            SetCircle2();
        }

        void SetCircle2()
        {
            var xs = x - radius1;
            var ys = y - radius1;
            if (xs * xs + ys * ys > r * r)
            {
                var theta = Math.Atan2(ys, xs);
                x = r * Math.Cos(theta) + radius1;
                y = r * Math.Sin(theta) + radius1;
            }
            Params.x = (byte)((x - radius1) / r * 127 + 127);
            Params.y = (byte)((y - radius1) / r * 127 + 127);
            AbsoluteLayout.SetLayoutBounds(circle2,
                new Rectangle(x - radius2, y - radius2, radius2 * 2, radius2 * 2));
        }

        private void ImageSizeChangedSpin(object sender, System.EventArgs e)
        {
            radius3 = ((Image)sender).Height / 2;
            l = ((Image)sender).Width - radius3 * 2;
            spin = l / 2 + radius3;
            SetCircle3();
        }
        private void OnTouchSpin(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released && Params.period != 150)
                spin = l / 2 + radius3;
            else
            {
                spin = args.Location.X;
                x = y = radius1;
                SetCircle2();
            }
            SetCircle3();
        }
        private void OnTouch3(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released && Params.period != 150)
                spin = l / 2 + radius3;
            else
            {
                spin += args.Location.X - radius3;
                x = y = radius1;
                SetCircle2();
            }
            SetCircle3();
        }
        void SetCircle3()
        {
            if (spin < radius3) spin = radius3;
            if (spin > l + radius3) spin = l + radius3;

            Params.spin = (byte)((spin-radius3) / l * 255);

            AbsoluteLayout.SetLayoutBounds(circle3,
                new Rectangle(spin-radius3, 0, radius3 * 2, radius3 * 2));
        }

        private void sliderPeriod_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            x = y = radius1;
            SetCircle2();
            spin = l / 2 + radius3;
            SetCircle3();
        }

        private async void OnTouchCCW(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released)
            {
                Params.upper = 0;
                var a = spinCCW.Rotation;
                await spinCCW.RotateTo(a - 360 - a % 360, 250);
                spinCCW.Rotation = 0;
            }
            else
            {
                Params.upper = 1;
                await spinCCW.RelRotateTo(-20000, 100000);
            }
        }
        private async void OnTouchCW(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released)
            {
                Params.upper = 0;
                var a = spinCW.Rotation;
                await spinCW.RotateTo(a + 360 - a % 360, 250);
                spinCW.Rotation = 0;
            }
            else
            {
                Params.upper = 2;
                await spinCW.RelRotateTo(20000, 100000);
            }
        }

        class MainPageViewModel : INotifyPropertyChanged
        {
            public Command ConnectCommand { get; private set; }

            private string textConnection;
            public string TextConnection
            {
                get { return textConnection; }
                set { textConnection = value; OnPropertyChanged("TextConnection"); }
            }

            private string textRx;
            public string TextRx
            {
                get { return textRx; }
                set { textRx = value; OnPropertyChanged("TextRx"); }
            }

            double zValue = -80;
            public double ZValue
            {
                get { return zValue; }
                set { zValue = value; OnPropertyChanged("ZValue"); OnPropertyChanged("ZValueText"); }
            }
            double period = 300;
            public double Period
            {
                get { return period; }
                set
                {
                    period = value - value % 50;
                    Params.period = (byte)(period / 10);
                    OnPropertyChanged("PeriodText");
                }
            }
            public string ZValueText { get { return "z: " + (int)ZValue + "[mm]"; } }
            public string PeriodText
            {
                get { return "T: " + (Period != 1500 ? Period.ToString() + "[ms]" : "∞"); }
            }


            BLEManager ble;

            public MainPageViewModel()
            {
                ble = BLEManager.Instance;
                ConnectCommand = new Command(async () =>
                {
                    if (!ble.IsConnected)
                    {
                        TextConnection = "デバイスを検索中";
                        await ble.Connect();
                    }
                });
                ble.BLEConnectionEvent += BLEConnectionEvent;
            }

            void BLEConnectionEvent(BLEConnectionState state)
            {
                switch (state)
                {
                    case BLEConnectionState.DeviceDiscovered:
                        TextConnection = "DeviceDiscovered";
                        break;
                    case BLEConnectionState.DeviceConnected:
                        TextConnection = "DeviceConnected";
                        break;
                    case BLEConnectionState.ServiceFound:
                        TextConnection = "ServiceFound";
                        break;
                    case BLEConnectionState.CharacteristicFound:
                        TextConnection = "CharacteristicFound";
                        ble.RxCharacteristic.ValueUpdated += RxCharacteristic_ValueUpdated;
                        Send();
                        break;
                    case BLEConnectionState.BLEStateIsOFF:
                        TextConnection = "BluetoothをONにして下さい";
                        break;
                    case BLEConnectionState.CannotConnect:
                        TextConnection = "接続できませんでした";
                        break;
                    case BLEConnectionState.ScanTimeoutElapsed:
                        TextConnection = "デバイスが見つかりませんでした";
                        break;
                }
            }

            async void Send()
            {
                while (true)
                {
                    if (ble.IsConnected && ble.TxCharacteristic.CanWrite)
                    {
                        //TextConnection = Params.x + " , " + Params.y
                        //    + " , " + ZValue + " , " + Period;
                        //await ble.TxCharacteristic.WriteAsync(new byte[] { Params.x, Params.y });
                        await ble.TxCharacteristic.WriteAsync(new byte[] {
                            Params.x,
                            Params.y ,
                            (byte)(-ZValue),
                            Params.period,
                            Params.spin,
                            Params.upper
                        });
                    }
                }
            }

            private void RxCharacteristic_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
            {
                TextRx += Encoding.ASCII.GetString(e.Characteristic.Value);
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
