using ODIF;

namespace MiController
{
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
            Mi = new InputChannelTypes.Button("MI", "");//, Properties.Resources._360_Guide.ToImageSource());

            BigRumble = new OutputChannelTypes.RumbleMotor("Big Rumble", "");
            SmallRumble = new OutputChannelTypes.RumbleMotor("Small Rumble", "");
        }
    }
}