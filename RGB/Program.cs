//#define useMainboard

using RGB.NET.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Asus;
using RGB.NET.Devices.Logitech;
using RGB.NET.Groups;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;

namespace RGB
{
    class Program
    {
        static void Main(string[] args)
        {
            RGBSurface surface = RGBSurface.Instance;

            bool sync = false;
            if (args != null && args.Length > 0 && args[0] == "sync")
            {
                sync = true;
            }


            if (sync)
            {
                surface.LoadDevices(AsusDeviceProvider.Instance, exclusiveAccessIfPossible: false, throwExceptions: true);
                surface.LoadDevices(CorsairDeviceProvider.Instance, exclusiveAccessIfPossible: true, throwExceptions: true);
                surface.LoadDevices(LogitechDeviceProvider.Instance, exclusiveAccessIfPossible: true, throwExceptions: true);
            }
            else
            {
                surface.LoadDevices(AsusDeviceProvider.Instance, exclusiveAccessIfPossible: true, throwExceptions: true);
                surface.LoadDevices(CorsairDeviceProvider.Instance, exclusiveAccessIfPossible: true, throwExceptions: true);
                surface.LoadDevices(LogitechDeviceProvider.Instance, exclusiveAccessIfPossible: true, throwExceptions: true);
            }

            surface.AlignDevices();


            Console.WriteLine(surface.Devices.Count());
            foreach (var dev in surface.Devices)
            {
                Console.WriteLine(dev.DeviceInfo.DeviceName);
            }

            if (sync)
            {
                AsusMainboardRGBDevice mainboard = RGBSurface.Instance.GetDevices<AsusMainboardRGBDevice>().FirstOrDefault();
                if (mainboard == null)
                    throw new ApplicationException("No mainboard to sync with is loaded.");

                mainboard.UpdateMode = DeviceUpdateMode.SyncBack;
                
                var group = new ListLedGroup(surface.Leds).Exclude(mainboard.ToArray());
                group.Brush = new SyncBrush(((IRGBDevice)mainboard)[LedId.Mainboard1]);

            }
            else
            {
                foreach (var dev in surface.Devices)
                {
                    dev.UpdateMode = DeviceUpdateMode.Sync;
                }

                var group = new ListLedGroup(surface.Leds);
                var gradient = new RainbowGradient();
                var brush = new LinearGradientBrush(gradient);
                group.Brush = brush;

                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += (s, e) => { gradient.Move(7); };
                aTimer.Interval = 100;
                aTimer.Start();
            }

            TimerUpdateTrigger timerTrigger = new TimerUpdateTrigger();
            timerTrigger.UpdateFrequency = 0.05;
            surface.RegisterUpdateTrigger(timerTrigger);
            timerTrigger.Start();

            Console.Read();
        }
    }
}
