using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODIF;
using ODIF.Extensions;
//using SlimDX;
//using SlimDX.XInput;
using System.Threading;
using System.Management;
using System.Reflection;
using Mighty.HID;

//using SlimDX.DirectInput;
//using SlimDX.Multimedia;
//using SlimDX.RawInput;
//using Device = SlimDX.RawInput.Device;
//using DeviceFlags = SlimDX.RawInput.DeviceFlags;
//using DeviceType = SlimDX.RawInput.DeviceType;

//using DeviceSubtype = SlimDX.XInput.DeviceSubtype;

namespace _Mi_Controller_Input
{
    [PluginInfo(
        PluginName = "Mi Controller",
        PluginDescription = "Adds support for the Mi Controller",
        PluginID = 999,
        PluginAuthorName = "André Ferreira",
        PluginAuthorEmail = "aadf.pt [at] gmail [dot] com",
        PluginAuthorURL = "https://github.com/aadfPT",
        PluginIconPath = @"pack://application:,,,/Mi Controller;component/Resources/360_Guide.png"
    )]
    public class MiControllerPlugin : InputDevicePlugin
    {
        public MiControllerPlugin()
        {
            Global.HardwareChangeDetected += CheckForControllersEvent;
            CheckForControllers();
        }
        private void CheckForControllersEvent(object sender, EventArrivedEventArgs e)
        {
            CheckForControllers();
        }
        private void CheckForControllers()
        {
            lock (base.Devices)
            {
                var compatibleDevices = HIDBrowse.Browse().Where(d => d.Vid.ToString("X4") == "2717" && d.Pid.ToString("X4") == "3144").ToList();
                foreach (var deviceInstance in compatibleDevices)
                {
                    if (Devices.Any(d => ((myMiDevice)d).DeviceInfo.Path == deviceInstance.Path))
                    {
                        continue;
                    }
                    Devices.Add(new myMiDevice(deviceInstance));
                }
                foreach (var inputDevice in Devices)
                {
                    var deviceReference = (myMiDevice)inputDevice;
                    if (compatibleDevices.All(d => d.Path != deviceReference.DeviceInfo.Path))
                    {
                        Devices.Remove(deviceReference);
                    }
                }
            }
        }

        public class myMiDevice : InputDevice
        {
            internal HIDDev Device;
            internal HIDInfo DeviceInfo;


            private readonly Thread _poolingThread;

            private bool _stopThread;
            internal Midevice DeviceWrapper;

            public myMiDevice(HIDInfo deviceInstance)
            {
                this.DeviceInfo = deviceInstance;
                this.Device = new HIDDev();
                /* connect */
                Device.Open(DeviceInfo, isExclusive: true);
                this.DeviceWrapper = new Midevice();
                base.DeviceName = DeviceInfo.Product + " (" + DeviceInfo.SerialNumber + ")";

                AddChannels();

                this._poolingThread = new Thread(ListenerThread);
                this._poolingThread.Start();
            }

            private void AddChannels()
            {
                AddInputChannels();

                AddOutputChannels();
            }

            private void AddOutputChannels()
            {
                OutputChannels.Add(DeviceWrapper.SmallRumble);
                OutputChannels.Add(DeviceWrapper.BigRumble);
            }

            private void AddInputChannels()
            {
                InputChannels.Add(DeviceWrapper.LSx);
                InputChannels.Add(DeviceWrapper.LSy);
                InputChannels.Add(DeviceWrapper.RSx);
                InputChannels.Add(DeviceWrapper.RSy);

                InputChannels.Add(DeviceWrapper.LS);
                InputChannels.Add(DeviceWrapper.RS);

                InputChannels.Add(DeviceWrapper.L1);
                InputChannels.Add(DeviceWrapper.R1);
                InputChannels.Add(DeviceWrapper.L2);
                InputChannels.Add(DeviceWrapper.R2);
                InputChannels.Add(DeviceWrapper.L2Digital);
                InputChannels.Add(DeviceWrapper.R2Digital);

                InputChannels.Add(DeviceWrapper.DUp);
                InputChannels.Add(DeviceWrapper.DDown);
                InputChannels.Add(DeviceWrapper.DLeft);
                InputChannels.Add(DeviceWrapper.DRight);

                InputChannels.Add(DeviceWrapper.A);
                InputChannels.Add(DeviceWrapper.B);
                InputChannels.Add(DeviceWrapper.X);
                InputChannels.Add(DeviceWrapper.Y);

                InputChannels.Add(DeviceWrapper.Start);
                InputChannels.Add(DeviceWrapper.Back);
                InputChannels.Add(DeviceWrapper.Mi);
            }


            protected override void Dispose(bool disposing)
            {
                this._stopThread = true;
                this._poolingThread?.Abort();
                Device.Dispose();
                base.Dispose(disposing);
            }

            public void ListenerThread()
            {
                byte[] currentState = new byte[21];
                while (!this._stopThread && !Global.IsShuttingDown)
                {
                    Thread.Sleep(2);
                    Device.Read(currentState);
                    DeviceWrapper.LSx.Value = Math.Max(-127.0, currentState[5] - 128) / 127;
                    DeviceWrapper.LSy.Value = Math.Max(-127.0, currentState[6] - 128) / 127;
                    DeviceWrapper.RSx.Value = Math.Max(-127.0, currentState[7] - 128) / 127;
                    DeviceWrapper.RSy.Value = Math.Max(-127.0, currentState[8] - 128) / 127;
                    DeviceWrapper.L2.Value = Math.Max(-127.0, currentState[11] - 128) / 127;
                    DeviceWrapper.R2.Value = Math.Max(-127.0, currentState[12] - 128) / 127;
                    DeviceWrapper.A.Value = (currentState[1] & Convert.ToByte(1)) != 0;
                    DeviceWrapper.B.Value = (currentState[1] & Convert.ToByte(2)) != 0;
                    DeviceWrapper.X.Value = (currentState[1] & Convert.ToByte(8)) != 0;
                    DeviceWrapper.Y.Value = (currentState[1] & Convert.ToByte(16)) != 0;
                    DeviceWrapper.Mi.Value = (currentState[20] & Convert.ToByte(1)) != 0;
                    if (DeviceWrapper.Mi.Value)
                    {
                        Thread.Sleep(250);
                    }
                    DeviceWrapper.Back.Value = (currentState[2] & Convert.ToByte(4)) != 0;
                    DeviceWrapper.Start.Value = (currentState[2] & Convert.ToByte(8)) != 0;
                    DeviceWrapper.L1.Value = (currentState[1] & Convert.ToByte(64)) != 0;
                    DeviceWrapper.R1.Value = (currentState[1] & Convert.ToByte(128)) != 0;
                    DeviceWrapper.L2Digital.Value = (currentState[2] & Convert.ToByte(1)) != 0;
                    DeviceWrapper.R2Digital.Value = (currentState[2] & Convert.ToByte(2)) != 0;
                    DeviceWrapper.LS.Value = (currentState[2] & Convert.ToByte(32)) != 0;
                    DeviceWrapper.RS.Value = (currentState[2] & Convert.ToByte(64)) != 0;
                    var dpad = currentState[4];
                    if (dpad == Convert.ToByte(15))
                    {
                        DeviceWrapper.DUp.Value = DeviceWrapper.DLeft.Value = DeviceWrapper.DRight.Value = DeviceWrapper.DDown.Value = false;
                    }
                    else
                    {
                        DeviceWrapper.DUp.Value = currentState[4] == 0 || currentState[4] == Convert.ToByte(1) || currentState[4] == Convert.ToByte(7);
                        DeviceWrapper.DRight.Value = currentState[4] == Convert.ToByte(2) || currentState[4] == Convert.ToByte(1) || currentState[4] == Convert.ToByte(3);
                        DeviceWrapper.DDown.Value = currentState[4] == Convert.ToByte(4) || currentState[4] == Convert.ToByte(3) || currentState[4] == Convert.ToByte(5);
                        DeviceWrapper.DLeft.Value = currentState[4] == Convert.ToByte(6) || currentState[4] == Convert.ToByte(5) || currentState[4] == Convert.ToByte(7);
                    }

                    //var vibe = new Vibration
                    //{
                    //    LeftMotorSpeed = (ushort)DeviceWrapper.SmallRumble.Value,
                    //    RightMotorSpeed = (ushort)DeviceWrapper.BigRumble.Value
                    //};
                }
            }
        }
    }

    internal class Midevice
    {
        public InputChannelTypes.JoyAxis LSx { get; set; }
        public InputChannelTypes.JoyAxis LSy { get; set; }
        public InputChannelTypes.JoyAxis RSx { get; set; }
        public InputChannelTypes.JoyAxis RSy { get; set; }

        public InputChannelTypes.Button LS { get; set; }
        public InputChannelTypes.Button RS { get; set; }

        public InputChannelTypes.JoyAxis L2 { get; set; }
        public InputChannelTypes.JoyAxis R2 { get; set; }
        public InputChannelTypes.Button L1 { get; set; }
        public InputChannelTypes.Button R1 { get; set; }
        public InputChannelTypes.Button L2Digital { get; set; }
        public InputChannelTypes.Button R2Digital { get; set; }

        public InputChannelTypes.Button DUp { get; set; }
        public InputChannelTypes.Button DDown { get; set; }
        public InputChannelTypes.Button DLeft { get; set; }
        public InputChannelTypes.Button DRight { get; set; }

        public InputChannelTypes.Button A { get; set; }
        public InputChannelTypes.Button B { get; set; }
        public InputChannelTypes.Button X { get; set; }
        public InputChannelTypes.Button Y { get; set; }

        public InputChannelTypes.Button Start { get; set; }
        public InputChannelTypes.Button Back { get; set; }
        public InputChannelTypes.Button Mi { get; set; }

        public OutputChannelTypes.RumbleMotor BigRumble { get; set; }
        public OutputChannelTypes.RumbleMotor SmallRumble { get; set; }

        public Midevice()
        {
            LSx = new InputChannelTypes.JoyAxis("Left Stick X", "");//, Properties.Resources._360_Left_Stick.ToImageSource());
            LSy = new InputChannelTypes.JoyAxis("Left Stick Y", "");//, Properties.Resources._360_Left_Stick.ToImageSource());
            RSx = new InputChannelTypes.JoyAxis("Right Stick X", "");//, Properties.Resources._360_Right_Stick.ToImageSource());
            RSy = new InputChannelTypes.JoyAxis("Right Stick Y", "");//, Properties.Resources._360_Right_Stick.ToImageSource());

            LS = new InputChannelTypes.Button("Left Stick", "");//, Properties.Resources._360_Left_Stick.ToImageSource());
            RS = new InputChannelTypes.Button("Right Stick", "");//, Properties.Resources._360_Right_Stick.ToImageSource());

            L2 = new InputChannelTypes.JoyAxis("L2 (Analog)", "");//, Properties.Resources._360_LT.ToImageSource()) { min_Value = 0 };
            R2 = new InputChannelTypes.JoyAxis("R2 (Analog)", "");//, Properties.Resources._360_RT.ToImageSource()) { min_Value = 0 };
            L1 = new InputChannelTypes.Button("L1", "");//, Properties.Resources._360_LB.ToImageSource());
            R1 = new InputChannelTypes.Button("R1", "");//, Properties.Resources._360_RB.ToImageSource());
            L2Digital = new InputChannelTypes.Button("L2 (Digital)", "");//, Properties.Resources._360_LB.ToImageSource());
            R2Digital = new InputChannelTypes.Button("R2 (Digital)", "");//, Properties.Resources._360_RB.ToImageSource());

            DUp = new InputChannelTypes.Button("DPad Up", "");//, Properties.Resources._360_Dpad_Up.ToImageSource());
            DDown = new InputChannelTypes.Button("DPad Down", "");//, Properties.Resources._360_Dpad_Down.ToImageSource());
            DLeft = new InputChannelTypes.Button("DPad Left", "");//, Properties.Resources._360_Dpad_Left.ToImageSource());
            DRight = new InputChannelTypes.Button("DPad Right", "");//, Properties.Resources._360_Dpad_Right.ToImageSource());

            A = new InputChannelTypes.Button("A", "");//, Properties.Resources._360_A.ToImageSource());
            B = new InputChannelTypes.Button("B", "");//, Properties.Resources._360_B.ToImageSource());
            X = new InputChannelTypes.Button("X", "");//, Properties.Resources._360_X.ToImageSource());
            Y = new InputChannelTypes.Button("Y", "");//, Properties.Resources._360_Y.ToImageSource());

            Start = new InputChannelTypes.Button("Menu", "");//, Properties.Resources._360_Start.ToImageSource());
            Back = new InputChannelTypes.Button("Back", "");//, Properties.Resources._360_Back.ToImageSource());
            Mi = new InputChannelTypes.Button("Mi", "");//, Properties.Resources._360_Guide.ToImageSource());

            BigRumble = new OutputChannelTypes.RumbleMotor("Big Rumble", "");
            SmallRumble = new OutputChannelTypes.RumbleMotor("Small Rumble", "");
        }
    }
}