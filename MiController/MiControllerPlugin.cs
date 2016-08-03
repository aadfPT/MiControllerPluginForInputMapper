using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODIF;
using ODIF.Extensions;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using HidLibrary;

namespace MiController
{
	[PluginInfo(
			PluginName = "Mi Controller",
			PluginDescription = "Adds support for the Mi Controller",
			PluginVersion = "1.0.0.3",
			PluginID = 56,
			PluginAuthorName = "André Ferreira",
			PluginAuthorEmail = "aadf.pt [at] gmail [dot] com",
			PluginAuthorURL = "https://github.com/aadfPT",
			PluginIconPath = @"pack://application:,,,/Mi Controller;component/Resources/MIButton.png"
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
				var compatibleDevices = HidDevices.Enumerate().ToList();
				var regex = new Regex("^.*2717.+3144.*$");
				compatibleDevices =
						compatibleDevices.Where(d => (d.Attributes.VendorId == 0x2717 && d.Attributes.ProductId == 0x3144) || regex.IsMatch(d.DevicePath))
								.ToList();
				foreach (var deviceInstance in compatibleDevices)
				{
					if (Devices.Any(d => ((MyMiDevice)d).Device.DevicePath == deviceInstance.DevicePath))
					{
						continue;
					}
					Devices.Add(new MyMiDevice(deviceInstance));
				}
				foreach (var inputDevice in Devices)
				{
					var deviceReference = (MyMiDevice)inputDevice;
					if (compatibleDevices.All(d => d.DevicePath != deviceReference.Device.DevicePath))
					{
						Devices.Remove(deviceReference);
					}
				}
			}
		}
	}
}