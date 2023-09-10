using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace StudioDisplayBrightnessController
{
    class ThreadWorker : ThreadA
    {
        private static readonly int THREAD_DELAY = 1000;
        private static readonly Object THREAD_CREATE_LOCK = new Object();
        private static readonly Object THREAD_RUN_LOCK = new Object();

        private static readonly long READ_TIMEOUT = 20000;
        private static readonly long RECONNECT_INTERVAL = 60000;


        public static volatile int userMinMonitorBrightness = 0;
        public static volatile int userMaxMonitorBrightness = 0;
        public static volatile float userAutomaticBrightnessLevelFactor = 1;
        public static volatile float userAmbientLightGammaCorrectionFactor = 1;

        public static bool IsMonitorOpened()
        {
            return studioDisplayController.IsMonitorOpened();
        }
        public static int GetMonitorBrightnessNormalized()
        {
            return studioDisplayController.GetMonitorBrightnessNormalized();
        }
        public static int GetMonitorAmbientLightWithGammaCorrection()
        {
            return studioDisplayController.GetMonitorAmbientLightWithGammaCorrection(userAmbientLightGammaCorrectionFactor);
        }


        private static StudioDisplayController studioDisplayController = new StudioDisplayController();


        private static volatile ThreadWorker instance;
        public static void Start()
        {
            lock (THREAD_CREATE_LOCK)
            {
                if (instance != null)
                {
                    instance.setThreadActiveToFalse();
                }
                instance = new ThreadWorker();
                Thread thread = new Thread(new ThreadStart(instance.ThreadWorkerRun));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        public static void Stop()
        {
            lock (THREAD_CREATE_LOCK)
            {
                if (instance != null)
                {
                    instance.setThreadActiveToFalse();
                }
            }
        }


        private long lastReconnect = 0;


        public ThreadWorker()
        {
        }








        public void ThreadWorkerRun()
        {
            lock (THREAD_RUN_LOCK)
            {
                try
                {
                    Logs.logInfo("ThreadWorker: ThreadWorkerRun: START");
                    while (isThreadActive())
                    {
                        ReconnectToMonitor();
                        SetMonitorBrightness();

                        ThreadDelay();
                    }
                }
                catch (Exception ex)
                {
                    Logs.logError("ThreadWorker: ThreadWorkerRun: Exception: ", ex);
                }
                finally
                {
                    studioDisplayController.CloseMonitor();
                    Logs.logInfo("ThreadWorker: ThreadWorkerRun: STOP");
                }
            }
        }









        private void ReconnectToMonitor()
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if ((timeNow - studioDisplayController.GetLastRead()) > READ_TIMEOUT)
            {
                if ((timeNow - lastReconnect) > RECONNECT_INTERVAL)
                {
                    Logs.logInfo("ThreadWorker: ReconnectToMonitor: BEGIN");
                    lastReconnect = timeNow;
                    studioDisplayController.CloseMonitor();
                    studioDisplayController.OpenMonitor();
                    Logs.logInfo("ThreadWorker: ReconnectToMonitor: END");
                }
            }
        }









        private void SetMonitorBrightness()
        {
            if (!studioDisplayController.IsMonitorOpened())
            {
                return;
            }

            Logs.logInfo("ThreadWorker: SetMonitorBrightness: BEGIN");
            int monitorBrightnessNormalizedNew;
            while (
                isThreadActive() &&
                studioDisplayController.IsMonitorOpened() &&
                ((monitorBrightnessNormalizedNew = CountMonitorBrightnessNormalized()) != studioDisplayController.GetMonitorBrightnessNormalized())
                )
            {
                if (monitorBrightnessNormalizedNew < studioDisplayController.GetMonitorBrightnessNormalized())
                {
                    if (Math.Abs(monitorBrightnessNormalizedNew - studioDisplayController.GetMonitorBrightnessNormalized()) > 18)
                    {
                        studioDisplayController.SetMonitorBrightnessNormalized(studioDisplayController.GetMonitorBrightnessNormalized() - 9);
                    }
                    else
                    {
                        studioDisplayController.SetMonitorBrightnessNormalized(studioDisplayController.GetMonitorBrightnessNormalized() - 1);
                    }
                }
                else if (monitorBrightnessNormalizedNew > studioDisplayController.GetMonitorBrightnessNormalized())
                {
                    if (Math.Abs(monitorBrightnessNormalizedNew - studioDisplayController.GetMonitorBrightnessNormalized()) > 18)
                    {
                        studioDisplayController.SetMonitorBrightnessNormalized(studioDisplayController.GetMonitorBrightnessNormalized() + 9);
                    }
                    else
                    {
                        studioDisplayController.SetMonitorBrightnessNormalized(studioDisplayController.GetMonitorBrightnessNormalized() + 1);
                    }
                }
            }
            Logs.logInfo("ThreadWorker: SetMonitorBrightness: END");
        }


        private int CountMonitorBrightnessNormalized()
        {
            // obliczenie aktualnej wartosci jasnosci dla monitora
            // dzielenie przez 100d jest wykonywane dlatego, ze Ambient Light Sensor jest wyskalowany do miliona a jasnosc monitora do 10 tysiecy
            int monitorAmbientLightWithGammaCorrection = studioDisplayController.GetMonitorAmbientLightWithGammaCorrection(userAmbientLightGammaCorrectionFactor);
            int monitorBrightnessNormalizedNew = (int)((monitorAmbientLightWithGammaCorrection / 100d) * userAutomaticBrightnessLevelFactor);
            if (monitorBrightnessNormalizedNew < userMinMonitorBrightness)
            {
                monitorBrightnessNormalizedNew = userMinMonitorBrightness;
            }
            if (monitorBrightnessNormalizedNew > userMaxMonitorBrightness)
            {
                monitorBrightnessNormalizedNew = userMaxMonitorBrightness;
            }
            return monitorBrightnessNormalizedNew;
        }










        private void ThreadDelay()
        {
            if (isThreadActive())
            {
                Thread.Sleep(THREAD_DELAY);
            }
        }




    }
}
