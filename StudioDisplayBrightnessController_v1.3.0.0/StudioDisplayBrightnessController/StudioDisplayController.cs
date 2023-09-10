using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet.Main;
using LibUsbDotNet;
using System.Threading;


namespace StudioDisplayBrightnessController
{
    internal class StudioDisplayController
    {
        private static readonly ushort MIN_MONITOR_BRIGHTNESS = 400;              // [400 - 60 000]
        private static readonly ushort MAX_MONITOR_BRIGHTNESS = 60000;            // [400 - 60 000]
        private static readonly int MIN_MONITOR_BRIGHTNESS_NORMALIZED = 0;        // [0 - 10 000]
        private static readonly int MAX_MONITOR_BRIGHTNESS_NORMALIZED = 10000;    // [0 - 10 000]
        private static readonly int MIN_MONITOR_AMBIENT_LIGHT = 0;                // [0 - 1 000 000]
        private static readonly int MAX_MONITOR_AMBIENT_LIGHT = 1000000;          // [0 - 1 000 000]



        private static readonly int VENDOR_ID = 0x05ac;
        private static readonly int PRODUCT_ID = 0x1114;
        private static readonly byte PRODUCT_CONFIGURATION = 1;
        private static readonly int PRODUCT_INTERFACE = 8;
        private static readonly ReadEndpointID READ_ENDPOINT_ID = ReadEndpointID.Ep09;
        private static readonly EndpointType ENDPOINT_TYPE = EndpointType.Interrupt;
        private static readonly int ENDPOINT_BUFFER_SIZE = 18;



        private volatile UsbDevice usbDevice;
        private volatile UsbEndpointReader usbEndpointReader;
        private UsbSetupPacket usbSetupPacket = new UsbSetupPacket(
                        0x21,  // RequestType
                        0x9,   // Request
                        0x301, // Value
                        7,     // Index
                        7);    // Length

        
        
        private volatile int monitorBrightnessNormalized = 0;
        private volatile int monitorAmbientLight = 0;
        private volatile uint lastRead = 0;







        public bool OpenMonitor()
        {
            Logs.logInfo("StudioDisplayController: OpenMonitor: BEGIN");
            try
            {
                UsbDeviceFinder usbFinder = new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID);
                usbDevice = UsbDevice.OpenUsbDevice(usbFinder);
                if (usbDevice == null)
                {
                    throw new Exception("Monitor was not found");
                }
                if (usbDevice.IsOpen)
                {
                    IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        wholeUsbDevice.SetConfiguration(PRODUCT_CONFIGURATION);
                        wholeUsbDevice.ClaimInterface(PRODUCT_INTERFACE);
                    }
                    usbEndpointReader = usbDevice.OpenEndpointReader(READ_ENDPOINT_ID, ENDPOINT_BUFFER_SIZE, ENDPOINT_TYPE);
                    usbEndpointReader.DataReceived += (OnReceiveAmbientLightData);
                    usbEndpointReader.DataReceivedEnabled = true;
                    Logs.logInfo("StudioDisplayController: OpenMonitor: Opened");
                    Logs.logInfo("StudioDisplayController: OpenMonitor: END");
                    return true;
                }
                else
                {
                    throw new Exception("Monitor was not opened");
                }
            }
            catch (Exception ex)
            {
                Logs.logWarning("StudioDisplayController: OpenMonitor: Exception: " + ex.Message);
                CloseMonitor();
                Logs.logInfo("StudioDisplayController: OpenMonitor: END");
                return false;
            }
        }


        public void CloseMonitor()
        {
            Logs.logInfo("StudioDisplayController: CloseMonitor: BEGIN");
            try
            {
                if (usbEndpointReader != null)
                {
                    usbEndpointReader.DataReceivedEnabled = false;
                    usbEndpointReader.DataReceived -= (OnReceiveAmbientLightData);
                    usbEndpointReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logs.logError("StudioDisplayController: CloseMonitor: Exception1: " + ex.Message);
            }
            finally
            {
                usbEndpointReader = null;
            }

            try
            {
                if (usbDevice != null)
                {
                    if (usbDevice.IsOpen)
                    {
                        IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            wholeUsbDevice.ReleaseInterface(PRODUCT_INTERFACE);
                        }
                        usbDevice.Close();
                    }
                    UsbDevice.Exit();
                }
            }
            catch (Exception ex)
            {
                Logs.logError("StudioDisplayController: CloseMonitor: Exception2: " + ex.Message);
            }
            finally
            {
                usbDevice = null;
            }
            Logs.logInfo("StudioDisplayController: CloseMonitor: Closed");
            Logs.logInfo("StudioDisplayController: CloseMonitor: END");
        }


        public bool IsMonitorOpened()
        {
            if ((usbDevice != null) && (usbEndpointReader != null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }






        public int GetMonitorBrightnessNormalized()
        {
            return monitorBrightnessNormalized;
        }


        public void SetMonitorBrightnessNormalized(int brightnessNormalized)
        {
            Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: BEGIN");
            if (!IsMonitorOpened())
            {
                Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: Monitor is not opened");
                Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: END");
                return;
            }
            Task<bool> task = Task.Run(() => SetMonitorBrightnessNormalizedTask(brightnessNormalized));
            if (task.Wait(TimeSpan.FromSeconds(5)))
            {
                Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: IN TIME");
                if (!task.Result)
                {
                    CloseMonitor();
                }
            }
            else
            {
                Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: TIMEOUT!!!");
                CloseMonitor();
            }
            Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalized: END");
        }


        private bool SetMonitorBrightnessNormalizedTask(int brightnessNormalized)
        {
            Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalizedTask: BEGIN");
            int brightnessLocalInt = MIN_MONITOR_BRIGHTNESS + ((brightnessNormalized * (MAX_MONITOR_BRIGHTNESS - MIN_MONITOR_BRIGHTNESS)) / MAX_MONITOR_BRIGHTNESS_NORMALIZED);
            if (brightnessLocalInt < MIN_MONITOR_BRIGHTNESS)
            {
                brightnessLocalInt = MIN_MONITOR_BRIGHTNESS;
            }
            else if (brightnessLocalInt > MAX_MONITOR_BRIGHTNESS)
            {
                brightnessLocalInt = MAX_MONITOR_BRIGHTNESS;
            }

            ushort brightnessLocalUshort = (ushort)brightnessLocalInt;
            if (brightnessLocalUshort < MIN_MONITOR_BRIGHTNESS)
            {
                brightnessLocalUshort = MIN_MONITOR_BRIGHTNESS;
            }
            else if (brightnessLocalUshort > MAX_MONITOR_BRIGHTNESS)
            {
                brightnessLocalUshort = MAX_MONITOR_BRIGHTNESS;
            }

            byte[] brightnessLocalBytes = new byte[2];
            brightnessLocalBytes[0] = (byte)(brightnessLocalUshort & 255);
            brightnessLocalBytes[1] = (byte)(brightnessLocalUshort >> 8);

            //byte[] buffer = new byte[7] { 0x01, 0x90, 0x01, 0x00, 0x00, 0x00, 0x00 }; // min
            //byte[] buffer = new byte[7] { 0x01, 0x60, 0xEA, 0x00, 0x00, 0x00, 0x00 }; // max
            byte[] buffer = new byte[7];
            buffer[0] = 0x01;
            buffer[1] = brightnessLocalBytes[0];
            buffer[2] = brightnessLocalBytes[1];
            buffer[3] = 0x00;
            buffer[4] = 0x00;
            buffer[5] = 0x00;
            buffer[6] = 0x00;

            try
            {
                int lengthTransferred = 0;
                bool ec = usbDevice.ControlTransfer(ref usbSetupPacket, buffer, buffer.Length, out lengthTransferred);
                if (ec && (lengthTransferred == buffer.Length))
                {
                    if (brightnessNormalized < MIN_MONITOR_BRIGHTNESS_NORMALIZED)
                    {
                        monitorBrightnessNormalized = MIN_MONITOR_BRIGHTNESS_NORMALIZED;
                    }
                    else if (brightnessNormalized > MAX_MONITOR_BRIGHTNESS_NORMALIZED)
                    {
                        monitorBrightnessNormalized = MAX_MONITOR_BRIGHTNESS_NORMALIZED;
                    }
                    else
                    {
                        monitorBrightnessNormalized = brightnessNormalized;
                    }
                    Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalizedTask: [0-10000]: " + monitorBrightnessNormalized.ToString());
                    Logs.logInfo("StudioDisplayController: SetMonitorBrightnessNormalizedTask: END");
                    return true;
                }
                else
                {
                    Logs.logWarning("StudioDisplayController: SetMonitorBrightnessNormalizedTask: Brightness was not set");
                    Logs.logInfo   ("StudioDisplayController: SetMonitorBrightnessNormalizedTask: END");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.logWarning("StudioDisplayController: SetMonitorBrightnessNormalizedTask: Exception: " + ex.Message);
                Logs.logInfo   ("StudioDisplayController: SetMonitorBrightnessNormalizedTask: END");
                return false;
            }
        }






        public int GetMonitorAmbientLightWithGammaCorrection(float gammaCorrectionFactor)
        {
            float monitorAmbientLightFactor = ((float)monitorAmbientLight / MAX_MONITOR_AMBIENT_LIGHT);
            float monitorAmbientLightFactorWithGammaCorrection = Helpers.GammaCorrection(monitorAmbientLightFactor, gammaCorrectionFactor);
            int monitorAmbientLightWithGammaCorrection = (int)(monitorAmbientLightFactorWithGammaCorrection * MAX_MONITOR_AMBIENT_LIGHT);
            return monitorAmbientLightWithGammaCorrection;
        }


        public long GetLastRead()
        {
            return ((long)lastRead)*1000;
        }


        private void OnReceiveAmbientLightData(object sender, EndpointDataEventArgs e)
        {
            if (e.Buffer.Length == ENDPOINT_BUFFER_SIZE)
            {
                lastRead = (uint) (DateTimeOffset.Now.ToUnixTimeMilliseconds()/1000);

                byte[] tablica = new byte[4];
                tablica[0] = e.Buffer[2];
                tablica[1] = e.Buffer[3];
                tablica[2] = e.Buffer[4];
                tablica[3] = e.Buffer[5];
                int localMonitorAmbientLight = BitConverter.ToInt32(tablica, 0);
                if (localMonitorAmbientLight < MIN_MONITOR_AMBIENT_LIGHT)
                {
                    monitorAmbientLight = MIN_MONITOR_AMBIENT_LIGHT;
                }
                else if (localMonitorAmbientLight > MAX_MONITOR_AMBIENT_LIGHT)
                {
                    monitorAmbientLight = MAX_MONITOR_AMBIENT_LIGHT;
                }
                else
                {
                    monitorAmbientLight = localMonitorAmbientLight;
                }
                Logs.logInfo("StudioDisplayController: OnReceiveAmbientLightData: " + monitorAmbientLight.ToString());
            }
            try
            {
                ((UsbEndpointReader)sender).ReadFlush();
            }
            catch (Exception ex)
            {
                Logs.logError("StudioDisplayController: OnReceiveAmbientLightData: ReadFlush() was not successful", ex);
            }
        }










    }
}
