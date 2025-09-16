using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioDisplayBrightnessController
{
    class Logs
    {
        private static bool LOGGING_INFO_ACTIVE = false;
        private static bool LOGGING_WARNING_ACTIVE = false;
        private static bool LOGGING_ERROR_ACTIVE = true;

        private static String LOG_FILE = "logs.txt";






        public static void logInfo(String message)
        {
            if (!LOGGING_INFO_ACTIVE)
            {
                return;
            }
            try
            {
                String timeStampFull = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String msg = timeStampFull + ": INFO: " + message + Environment.NewLine;
                Console.Write(msg);
                saveStringToFile(msg);
            }
            catch (Exception ex1)
            {
                saveStringToFile("Logs: logInfo: " + ex1.Message);
            }
        }




        public static void logWarning(String message)
        {
            if (!LOGGING_WARNING_ACTIVE)
            {
                return;
            }
            try
            {
                String timeStampFull = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String msg = timeStampFull + ": WARNING: " + message + Environment.NewLine;
                Console.Write(msg);
                saveStringToFile(msg);
            }
            catch (Exception ex1)
            {
                saveStringToFile("Logs: logWarning: " + ex1.Message);
            }
        }




        public static void logError(String message)
        {
            if (!LOGGING_ERROR_ACTIVE)
            {
                return;
            }
            try
            {
                String timeStampFull = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String msg = timeStampFull + ": ERROR: " + message + Environment.NewLine;
                Console.Write(msg);
                saveStringToFile(msg);
            }
            catch (Exception ex1)
            {
                saveStringToFile("Logs: logError: " + ex1.Message);
            }
        }

        public static void logError(String message, Exception ex)
        {
            if (!LOGGING_ERROR_ACTIVE)
            {
                return;
            }
            try
            {
                String timeStampFull = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                String msg =
                    timeStampFull + ": ERROR: " + message + Environment.NewLine +
                    timeStampFull + ": ERROR: " + ex.Message + Environment.NewLine +
                    timeStampFull + ": ERROR: " + ex.StackTrace + Environment.NewLine;
                Console.Write(msg);
                saveStringToFile(msg);
            }
            catch (Exception ex1)
            {
                saveStringToFile("Logs: logError: " + ex1.Message);
            }
        }




        private static Object LOCK1 = new Object();

        private static void saveStringToFile(String message)
        {
            lock (LOCK1)
            {
                try
                {
                    File.AppendAllText(LOG_FILE, message);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Logs: saveStringToFile: exception: " + ex.Message);
                }
            }
        }






    }
}
