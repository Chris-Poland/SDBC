using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;


namespace StudioDisplayBrightnessController
{
    public partial class Form_Main : Form
    {
        private bool programStart = true;

        private bool form_Main_DeactivateEvent = false;



        enum MonitorConnectionState
        {
            CONNECTED,
            DISCONNECTED
        }
        private MonitorConnectionState monitorConnectionState = MonitorConnectionState.DISCONNECTED;

        private readonly Color colorGreen = Color.FromArgb(255, 0, 192, 0);
        private readonly Color colorRed = Color.FromArgb(255, 192, 0, 0);

        private Form_About form_About;
        private Form_Settings form_Settings;
        private Form_Closing form_Closing;





        public Form_Main()
        {
            InitializeComponent();
            InitUserInterface();
            InitThreadWorker();
            ThreadWorker.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThreadWorker.Stop();
        }










        private void InitUserInterface()
        {
            trackBar1.Value = Properties.Settings.Default.AutomaticBrightnessLevel;
            label5.Text = Properties.Settings.Default.AutomaticBrightnessLevel.ToString();
        }

        private void InitThreadWorker()
        {
            ThreadWorker.userMinMonitorBrightness = Properties.Settings.Default.MinMonitorBrightness;
            ThreadWorker.userMaxMonitorBrightness = Properties.Settings.Default.MaxMonitorBrightness;
            ThreadWorker.userAutomaticBrightnessLevelFactor = Properties.Settings.Default.AutomaticBrightnessLevelFactor;
            ThreadWorker.userAmbientLightGammaCorrectionFactor = Properties.Settings.Default.AmbientLightGammaCorrectionFactor;
        }










        private void timerProgramStart_Tick(object sender, EventArgs e)
        {
            programStart = false;
            timerProgramStart.Stop();
        }










        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (programStart == true)
            {
                Visible = false;
                timerUpdateInterface.Enabled = false;
            }
            SetWindowPosition();
        }

        private void SetWindowPosition()
        {
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int xLocation = screenWidth - this.Width - 6;
            int yLocation = screenHeight - this.Height - 6;
            Location = new Point(xLocation, yLocation);
        }















        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!form_Main_DeactivateEvent)
                {
                    if (Visible == false)
                    {
                        this.Show();
                        this.Activate();
                        ShowUI();
                    }
                }
            }
        }

        private void Form_Main_Deactivate(object sender, EventArgs e)
        {
            form_Main_DeactivateEvent = true;
            timerHideWindow.Enabled = true;

            HideUI();
            this.Hide();
        }

        private void timerHideWindow_Tick(object sender, EventArgs e)
        {
            form_Main_DeactivateEvent = false;
            timerHideWindow.Enabled = false;
        }

        private void ShowUI()
        {
            Visible = true;
            timerUpdateInterface.Enabled = true;
        }

        private void HideUI()
        {
            Visible = false;
            timerUpdateInterface.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Visible = false;
            timerUpdateInterface.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.buymeacoffee.com/krzysztof.sk");
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            // przeskalowanie liniowe zakresu wejściowego [0 : 1000] na zakres [0.1 : 10]
            double intermediateValue = 2d * (trackBar1.Value / 1000d) - 1d;  // przeskalowanie pierwotnego zakresu [0 : 1000] do zakresu [-1 : 1]
            float gamma = (float)Math.Pow(10, intermediateValue); // obliczenie wartosci gamma w zakresie wyjściowym [0.1 : 10]
            Properties.Settings.Default.AutomaticBrightnessLevel = trackBar1.Value;
            Properties.Settings.Default.AutomaticBrightnessLevelFactor = gamma;
            Properties.Settings.Default.Save();

            ThreadWorker.userAutomaticBrightnessLevelFactor = Properties.Settings.Default.AutomaticBrightnessLevelFactor;
            label5.Text = Properties.Settings.Default.AutomaticBrightnessLevel.ToString();
        }



        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form_About = new Form_About();
            DialogResult dialogResult = form_About.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                form_About.Close();
            }
            else
            {
                form_About.Close();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form_Settings = new Form_Settings();
            DialogResult dialogResult = form_Settings.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                form_Settings.Close();
            }
            else
            {
                form_Settings.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThreadWorker.Stop();

            form_Closing = new Form_Closing();
            DialogResult dialogResult = form_Closing.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                form_Closing.Close();
            }
            else
            {
                form_Closing.Close();
            }
            Close();
        }







        private void timerUpdateInterface_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = ThreadWorker.GetMonitorBrightnessNormalized();
            label2.Text = (progressBar1.Value / 10).ToString();

            progressBar2.Value = ThreadWorker.GetMonitorAmbientLightWithGammaCorrection();
            label4.Text = (progressBar2.Value / 1000).ToString();
        }



        private void timerUpdateMonitorConnectionState_Tick(object sender, EventArgs e)
        {
            if (ThreadWorker.IsMonitorOpened())
            {
                if (monitorConnectionState != MonitorConnectionState.CONNECTED)
                {
                    monitorConnectionState = MonitorConnectionState.CONNECTED;
                    panel1.BackColor = colorGreen;
                    notifyIcon1.Icon = Properties.Resources.monitor_v3b_512px_color;
                }
            }
            else
            {
                if (monitorConnectionState != MonitorConnectionState.DISCONNECTED)
                {
                    monitorConnectionState = MonitorConnectionState.DISCONNECTED;
                    panel1.BackColor = colorRed;
                    notifyIcon1.Icon = Properties.Resources.monitor_v3b_512px_bw;
                }
            }
        }

    }
}
