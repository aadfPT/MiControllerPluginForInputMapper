using ODIF;

namespace MiController
{
    internal class Midevice
    {
        public JoyAxis LSx { get; set; }
        public JoyAxis LSy { get; set; }
        public JoyAxis RSx { get; set; }
        public JoyAxis RSy { get; set; }

        public Button LS { get; set; }
        public Button RS { get; set; }

        public JoyAxis L2 { get; set; }
        public JoyAxis R2 { get; set; }
        public Button L1 { get; set; }
        public Button R1 { get; set; }
        public Button L2Digital { get; set; }
        public Button R2Digital { get; set; }

        public Button DUp { get; set; }
        public Button DDown { get; set; }
        public Button DLeft { get; set; }
        public Button DRight { get; set; }

        public Button A { get; set; }
        public Button B { get; set; }
        public Button X { get; set; }
        public Button Y { get; set; }

        public Button Start { get; set; }
        public Button Back { get; set; }
        public Button Mi { get; set; }
        public JoyAxis BatteryLevel { get; set; }


        public JoyAxis GX { get; set; }
        public JoyAxis GY { get; set; }
        public JoyAxis GZ { get; set; }



        public RumbleMotor BigRumble { get; set; }
        public RumbleMotor SmallRumble { get; set; }
        public Toggle ControlAccelerometer { get; set; }

        public Midevice()
        {
            LSx = new JoyAxis("Left Stick X", DataFlowDirection.Input);//, Properties.Resources._360_Left_Stick.ToImageSource());
            LSy = new JoyAxis("Left Stick Y", DataFlowDirection.Input);//, Properties.Resources._360_Left_Stick.ToImageSource());
            RSx = new JoyAxis("Right Stick X", DataFlowDirection.Input);//, Properties.Resources._360_Right_Stick.ToImageSource());
            RSy = new JoyAxis("Right Stick Y", DataFlowDirection.Input);//, Properties.Resources._360_Right_Stick.ToImageSource());

            LS = new Button("Left Stick", DataFlowDirection.Input);//, Properties.Resources._360_Left_Stick.ToImageSource());
            RS = new Button("Right Stick", DataFlowDirection.Input);//, Properties.Resources._360_Right_Stick.ToImageSource());

            L2 = new JoyAxis("L2 (Analog)", DataFlowDirection.Input);//, Properties.Resources._360_LT.ToImageSource()) { min_Value = 0 };
            R2 = new JoyAxis("R2 (Analog)", DataFlowDirection.Input);//, Properties.Resources._360_RT.ToImageSource()) { min_Value = 0 };
            L1 = new Button("L1", DataFlowDirection.Input);//, Properties.Resources._360_LB.ToImageSource());
            R1 = new Button("R1", DataFlowDirection.Input);//, Properties.Resources._360_RB.ToImageSource());
            L2Digital = new Button("L2 (Digital)", DataFlowDirection.Input);//, Properties.Resources._360_LB.ToImageSource());
            R2Digital = new Button("R2 (Digital)", DataFlowDirection.Input);//, Properties.Resources._360_RB.ToImageSource());

            DUp = new Button("DPad Up", DataFlowDirection.Input);//, Properties.Resources._360_Dpad_Up.ToImageSource());
            DDown = new Button("DPad Down", DataFlowDirection.Input);//, Properties.Resources._360_Dpad_Down.ToImageSource());
            DLeft = new Button("DPad Left", DataFlowDirection.Input);//, Properties.Resources._360_Dpad_Left.ToImageSource());
            DRight = new Button("DPad Right", DataFlowDirection.Input);//, Properties.Resources._360_Dpad_Right.ToImageSource());

            A = new Button("A", DataFlowDirection.Input);//, Properties.Resources._360_A.ToImageSource());
            B = new Button("B", DataFlowDirection.Input);//, Properties.Resources._360_B.ToImageSource());
            X = new Button("X", DataFlowDirection.Input);//, Properties.Resources._360_X.ToImageSource());
            Y = new Button("Y", DataFlowDirection.Input);//, Properties.Resources._360_Y.ToImageSource());

            Start = new Button("Menu", DataFlowDirection.Input);//, Properties.Resources._360_Start.ToImageSource());
            Back = new Button("Back", DataFlowDirection.Input);//, Properties.Resources._360_Back.ToImageSource());
            Mi = new Button("MI", DataFlowDirection.Input);//, Properties.Resources._360_Guide.ToImageSource());

            BatteryLevel = new JoyAxis("Battery level", DataFlowDirection.Input);
            GX = new JoyAxis("Gravity X", DataFlowDirection.Input);
            GY = new JoyAxis("Gravity Y", DataFlowDirection.Input);
            GZ = new JoyAxis("Gravity Z", DataFlowDirection.Input);

            BigRumble = new RumbleMotor("Big Rumble", DataFlowDirection.Output);
            SmallRumble = new RumbleMotor("Small Rumble", DataFlowDirection.Output);
            ControlAccelerometer = new Toggle("Control Accelerometer", DataFlowDirection.Output);
        }
    }
}