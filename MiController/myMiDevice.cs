using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HidLibrary;
using ODIF;

namespace MiController
{
	public class MyMiDevice : InputDevice
	{
		internal HidDevice Device;
		internal Midevice DeviceWrapper;


		private Thread InputPoolingThread { get; }
		private Thread OutputThread { get; }

		private byte[] Vibration { get; } = { 0x20, 0x00, 0x00 };
		private bool StopThread { get; set; }
		private bool AccelerometerEnabled { get; set; }

		public MyMiDevice(HidDevice deviceInstance)
		{
			this.Device = deviceInstance;
			Device.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped);
			this.DeviceWrapper = new Midevice();
			DeviceWrapper.ControlAccelerometer.PropertyChanged +=
					(sender, args) => AccelerometerEnabled = 0 != DeviceWrapper.ControlAccelerometer.Value;
			byte[] serialNumber;
			byte[] product;
			Device.ReadSerialNumber(out serialNumber);
			Device.ReadProduct(out product);
			base.DeviceName = Regex.Replace(Encoding.UTF8.GetString(product), "[^-a-zA-Z0-9 ]", String.Empty)
												+ " (" + Regex.Replace(Encoding.UTF8.GetString(serialNumber), "[^-a-zA-Z0-9 ]", String.Empty) + ")";
			AddChannels();
			InputPoolingThread = new Thread(InputListenerThread);
			InputPoolingThread.Start();
			OutputThread = new Thread(OutputVibration);
			OutputThread.Start();
			ToggleAccelerometer();
		}

		private void AddChannels()
		{
			AddInputChannels();

			AddOutputChannels();
		}

		private void AddOutputChannels()
		{
			Channels.Add(DeviceWrapper.SmallRumble);
			Channels.Add(DeviceWrapper.BigRumble);
			Channels.Add(DeviceWrapper.ControlAccelerometer);
		}

		private void AddInputChannels()
		{
			Channels.Add(DeviceWrapper.LSx);
			Channels.Add(DeviceWrapper.LSy);
			Channels.Add(DeviceWrapper.RSx);
			Channels.Add(DeviceWrapper.RSy);

			Channels.Add(DeviceWrapper.LS);
			Channels.Add(DeviceWrapper.RS);

			Channels.Add(DeviceWrapper.L1);
			Channels.Add(DeviceWrapper.R1);
			Channels.Add(DeviceWrapper.L2);
			Channels.Add(DeviceWrapper.R2);
			Channels.Add(DeviceWrapper.L2Digital);
			Channels.Add(DeviceWrapper.R2Digital);

			Channels.Add(DeviceWrapper.DUp);
			Channels.Add(DeviceWrapper.DDown);
			Channels.Add(DeviceWrapper.DLeft);
			Channels.Add(DeviceWrapper.DRight);

			Channels.Add(DeviceWrapper.A);
			Channels.Add(DeviceWrapper.B);
			Channels.Add(DeviceWrapper.X);
			Channels.Add(DeviceWrapper.Y);

			Channels.Add(DeviceWrapper.Start);
			Channels.Add(DeviceWrapper.Back);
			Channels.Add(DeviceWrapper.Mi);
			Channels.Add(DeviceWrapper.BatteryLevel);
			Channels.Add(DeviceWrapper.GX);
			Channels.Add(DeviceWrapper.GY);
			Channels.Add(DeviceWrapper.GZ);
		}


		protected override void Dispose(bool disposing)
		{
			StopThread = true;
			InputPoolingThread?.Abort();
			OutputThread?.Abort();
			Device.CloseDevice();
			Device.Dispose();
			base.Dispose(disposing);
		}

		public void InputListenerThread()
		{
			while (!this.StopThread && !Global.IsShuttingDown)
			{
				try
				{
					var currentState = Device.Read().Data;
					if (currentState.Length < 21) continue;
					DeviceWrapper.LSx.Value = Math.Max(-127.0, currentState[5] - 128) / 127;
					DeviceWrapper.LSy.Value = Math.Max(-127.0, currentState[6] - 128) / 127;
					DeviceWrapper.RSx.Value = Math.Max(-127.0, currentState[7] - 128) / 127;
					DeviceWrapper.RSy.Value = Math.Max(-127.0, currentState[8] - 128) / 127;
					DeviceWrapper.L2.Value = Math.Max(-127.0, currentState[11] - 128) / 127;
					DeviceWrapper.R2.Value = Math.Max(-127.0, currentState[12] - 128) / 127;
					DeviceWrapper.A.Value = (currentState[1] & 1) != 0;
					DeviceWrapper.B.Value = (currentState[1] & 2) != 0;
					DeviceWrapper.X.Value = (currentState[1] & 8) != 0;
					DeviceWrapper.Y.Value = (currentState[1] & 16) != 0;
					var miValue = (currentState[20] & 1) != 0;
					DeviceWrapper.Mi.Value = miValue;
					if (miValue)
					{
						Thread.Sleep(250);
					}
					DeviceWrapper.Back.Value = (currentState[2] & 4) != 0;
					DeviceWrapper.Start.Value = (currentState[2] & 8) != 0;
					DeviceWrapper.L1.Value = (currentState[1] & 64) != 0;
					DeviceWrapper.R1.Value = (currentState[1] & 128) != 0;
					DeviceWrapper.L2Digital.Value = (currentState[2] & 1) != 0;
					DeviceWrapper.R2Digital.Value = (currentState[2] & 2) != 0;
					DeviceWrapper.LS.Value = (currentState[2] & 32) != 0;
					DeviceWrapper.RS.Value = (currentState[2] & 64) != 0;
					var dpad = currentState[4];
					if (dpad == 15)
					{
						DeviceWrapper.DUp.Value =
								DeviceWrapper.DLeft.Value =
										DeviceWrapper.DRight.Value = DeviceWrapper.DDown.Value = false;
					}
					else
					{
						DeviceWrapper.DUp.Value = currentState[4] == 0 || currentState[4] == 1 ||
																			currentState[4] == 7;
						DeviceWrapper.DRight.Value = currentState[4] == 2 ||
																				 currentState[4] == 1 ||
																				 currentState[4] == 3;
						DeviceWrapper.DDown.Value = currentState[4] == 4 ||
																				currentState[4] == 3 ||
																				currentState[4] == 5;
						DeviceWrapper.DLeft.Value = currentState[4] == 6 ||
																				currentState[4] == 5 ||
																				currentState[4] == 7;
					}
					DeviceWrapper.BatteryLevel.Value = Convert.ToDouble(currentState[19]) / 255;

					//Accelerometer
					DeviceWrapper.GX.Value = currentState[13];
					DeviceWrapper.GY.Value = currentState[14];
					DeviceWrapper.GZ.Value = currentState[18];
					Debug.WriteLine("DX:" + Convert.ToInt32(currentState[13]) + " " + Convert.ToInt32(currentState[14])
													+ " DY:" + Convert.ToInt32(currentState[15]) + " " +
													Convert.ToInt32(currentState[16])
													+ " DZ:" + Convert.ToInt32(currentState[17]) + " " +
													Convert.ToInt32(currentState[18]));
				}
				catch (Exception)
				{
					//error in reading from controller is ignored
					continue;
				}

			}
		}
		private void OutputVibration()
		{
			var state = new Tuple<byte, byte>(0, 0);
			while (!StopThread && !Global.IsShuttingDown)
			{
				Vibration[1] = Convert.ToByte(Math.Min(1, Math.Abs((double)DeviceWrapper.SmallRumble.Value)) * 0xff);
				Vibration[2] = Convert.ToByte(Math.Min(1, Math.Abs((double)DeviceWrapper.BigRumble.Value)) * 0xff);
				if (state.Item1 != Vibration[1] || state.Item2 != Vibration[2])
				{
					state = new Tuple<byte, byte>(Vibration[1], Vibration[2]);
					try
					{
						Device.WriteFeatureData(Vibration);
					}
					catch (Exception)
					{
						continue;
					}
				}
				Thread.Sleep(100);
			}
		}
		private void ToggleAccelerometer()
		{
			var headerByte = Convert.ToByte(0x31);
			var controlByte = Convert.ToByte(0x01);
			var sensibilityByte = Convert.ToByte(0x08);
			var data = new byte[21];
			data[0] = headerByte;
			data[1] = controlByte;
			data[2] = sensibilityByte;
			try
			{
				Device.WriteFeatureData(data);
			}
			catch (Exception)
			{
				//ignore exceptions
			}
		}

	}
}