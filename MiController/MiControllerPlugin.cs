using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODIF;
using ODIF.Extensions;
using SlimDX;
using SlimDX.XInput;
using System.Threading;
using System.Management;
using System.Reflection;
using SlimDX.DirectInput;
//using DeviceSubtype = SlimDX.XInput.DeviceSubtype;

namespace _Mi_Controller_Input
{
    [PluginInfo(
        PluginName = "Mi Controller Input",
        PluginDescription = "Adds support for the Mi Controller",
        PluginID = 999,
        PluginAuthorName = "André Ferreira",
        PluginAuthorEmail = "aadf.pt [at] gmail [dot] com",
        PluginAuthorURL = "https://github.com/aadfPT",
        PluginIconPath = @"pack://application:,,,/360 Controller Input;component/Resources/360_Guide.png"
    )]
    public class MiControllerPlugin : InputDevicePlugin
    {
        private readonly DirectInput _directInput = new DirectInput();

        //private Joystick gamepad;
        //private JoystickState state;

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
                var deviceInstances = new List<DeviceInstance>();
                deviceInstances.AddRange(this._directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly));
                foreach (var deviceInstance in deviceInstances)
                {
                    if (Devices.Any(d => ((myMiDevice)d).Device.InstanceGuid == deviceInstance.InstanceGuid))
                    {
                        continue;
                    }
                    Devices.Add(new myMiDevice(this._directInput, deviceInstance));
                }
                foreach (var inputDevice in Devices)
                {
                    var deviceReference = (myMiDevice)inputDevice;
                    if (deviceInstances.All(d => d.InstanceGuid != deviceReference.Device.InstanceGuid))
                    {
                        Devices.Remove(deviceReference);
                    }
                }
            }
        }

        public class myMiDevice : InputDevice
        {
            internal DeviceInstance Device;

            private readonly Joystick _gamepad;

            //private DirectInput directInput;


            private readonly Thread _poolingThread;

            private bool _stopThread;
            //internal UserIndex ControllerID;
            internal Midevice DeviceWrapper;
            //Controller controller;

            public myMiDevice(DirectInput directInput, DeviceInstance device)
            {
                //base.StatusIcon = Extensions.ToImageSource(Resources.GenericGamepad);
                //this.directInput = directInput;
                this.Device = device;
                this.DeviceWrapper = new Midevice();
                this._gamepad = new Joystick(directInput, device.InstanceGuid);
                base.DeviceName = _gamepad.Information.ProductName;
                this._gamepad.Acquire();
                foreach (var @object in
                    this._gamepad.GetObjects()
                    .Where(@object => (@object.ObjectType & ObjectDeviceType.Axis) != ObjectDeviceType.All))
                {
                    this._gamepad.GetObjectPropertiesById((int)@object.ObjectType).SetRange(-1000, 1000);
                }

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

                OutputChannels.Add(DeviceWrapper.SmallRumble);
                OutputChannels.Add(DeviceWrapper.BigRumble);
                this._poolingThread = new Thread(ListenerThread);
                this._poolingThread.Start();
            }

            protected override void Dispose(bool disposing)
            {
                this._stopThread = true;
                this._poolingThread?.Abort();
                this._gamepad.Unacquire();
                base.Dispose(disposing);
            }

            public void ListenerThread()
            {
                while (!this._stopThread && !Global.IsShuttingDown)
                {
                    Thread.Sleep(2);
                    var currentState = this._gamepad.GetCurrentState();
                    DeviceWrapper.LSx.Value = currentState.X / 1000f;
                    DeviceWrapper.LSy.Value = currentState.Y / 1000f;
                    DeviceWrapper.RSx.Value = currentState.Z / 1000f;
                    DeviceWrapper.RSy.Value = currentState.RotationZ / 1000f;
                    DeviceWrapper.A.Value = currentState.IsPressed(0);
                    DeviceWrapper.B.Value = currentState.IsPressed(1);
                    DeviceWrapper.X.Value = currentState.IsPressed(3);
                    DeviceWrapper.Y.Value = currentState.IsPressed(4);
                    DeviceWrapper.Back.Value = currentState.IsPressed(10);
                    DeviceWrapper.Start.Value = currentState.IsPressed(11);
                    DeviceWrapper.L1.Value = currentState.IsPressed(6);
                    DeviceWrapper.R1.Value = currentState.IsPressed(7);
                    DeviceWrapper.L2Digital.Value = currentState.IsPressed(8);
                    DeviceWrapper.R2Digital.Value = currentState.IsPressed(9);
                    DeviceWrapper.LS.Value = currentState.IsPressed(13);
                    DeviceWrapper.RS.Value = currentState.IsPressed(14);
                    var pov = currentState.GetPointOfViewControllers()[0];
                    DeviceWrapper.DUp.Value = 0 <= pov && pov <= 4500 || 31500 <= pov;
                    DeviceWrapper.DDown.Value = 13500 <= pov && pov <= 22500;
                    DeviceWrapper.DLeft.Value = 22500 <= pov && pov <= 31500;
                    DeviceWrapper.DRight.Value = 4500 <= pov && pov <= 13500;

                    var vibe = new Vibration
                    {
                        LeftMotorSpeed = (ushort)DeviceWrapper.SmallRumble.Value,
                        RightMotorSpeed = (ushort)DeviceWrapper.BigRumble.Value
                    };
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
        public InputChannelTypes.Button Guide { get; set; }

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

            L2 = new InputChannelTypes.JoyAxis("Left Trigger (Analog)", "");//, Properties.Resources._360_LT.ToImageSource()) { min_Value = 0 };
            R2 = new InputChannelTypes.JoyAxis("Right Trigger (Analog)", "");//, Properties.Resources._360_RT.ToImageSource()) { min_Value = 0 };
            L1 = new InputChannelTypes.Button("Left Bumper", "");//, Properties.Resources._360_LB.ToImageSource());
            R1 = new InputChannelTypes.Button("Right Bumper", "");//, Properties.Resources._360_RB.ToImageSource());
            L2Digital = new InputChannelTypes.Button("Left Trigger (Digital)", "");//, Properties.Resources._360_LB.ToImageSource());
            R2Digital = new InputChannelTypes.Button("Right Trigger (Digital)", "");//, Properties.Resources._360_RB.ToImageSource());

            DUp = new InputChannelTypes.Button("DPad Up", "");//, Properties.Resources._360_Dpad_Up.ToImageSource());
            DDown = new InputChannelTypes.Button("DPad Down", "");//, Properties.Resources._360_Dpad_Down.ToImageSource());
            DLeft = new InputChannelTypes.Button("DPad Left", "");//, Properties.Resources._360_Dpad_Left.ToImageSource());
            DRight = new InputChannelTypes.Button("DPad Right", "");//, Properties.Resources._360_Dpad_Right.ToImageSource());

            A = new InputChannelTypes.Button("A", "");//, Properties.Resources._360_A.ToImageSource());
            B = new InputChannelTypes.Button("B", "");//, Properties.Resources._360_B.ToImageSource());
            X = new InputChannelTypes.Button("X", "");//, Properties.Resources._360_X.ToImageSource());
            Y = new InputChannelTypes.Button("Y", "");//, Properties.Resources._360_Y.ToImageSource());

            Start = new InputChannelTypes.Button("Start", "");//, Properties.Resources._360_Start.ToImageSource());
            Back = new InputChannelTypes.Button("Back", "");//, Properties.Resources._360_Back.ToImageSource());
            Guide = new InputChannelTypes.Button("Guide", "");//, Properties.Resources._360_Guide.ToImageSource());

            BigRumble = new OutputChannelTypes.RumbleMotor("Big Rumble", "");
            SmallRumble = new OutputChannelTypes.RumbleMotor("Small Rumble", "");
        }
    }
}