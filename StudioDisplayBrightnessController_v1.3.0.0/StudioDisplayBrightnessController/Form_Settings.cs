using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace StudioDisplayBrightnessController
{
    public partial class Form_Settings : Form
    {
        public Form_Settings()
        {
            InitializeComponent();
            InitUserInterface();
        }



        private void InitUserInterface()
        {
            trackBar1.Value = Properties.Settings.Default.MinMonitorBrightness;
            label7.Text = (Properties.Settings.Default.MinMonitorBrightness/10).ToString();

            trackBar2.Value = Properties.Settings.Default.MaxMonitorBrightness;
            label8.Text = (Properties.Settings.Default.MaxMonitorBrightness/10).ToString();

            trackBar3.Value = Properties.Settings.Default.AmbientLightGammaCorrection;
            label9.Text = String.Format("{0:N3}", Properties.Settings.Default.AmbientLightGammaCorrectionFactor);
        }






        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (trackBar1.Value <= trackBar2.Value)
            {
                Properties.Settings.Default.MinMonitorBrightness = trackBar1.Value;
                Properties.Settings.Default.Save();

                ThreadWorker.userMinMonitorBrightness = Properties.Settings.Default.MinMonitorBrightness;
                label7.Text = (Properties.Settings.Default.MinMonitorBrightness / 10).ToString();
            }
            else
            {
                trackBar1.Value = trackBar2.Value;
            }
        }



        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            if (trackBar2.Value >= trackBar1.Value)
            {
                Properties.Settings.Default.MaxMonitorBrightness = trackBar2.Value;
                Properties.Settings.Default.Save();

                ThreadWorker.userMaxMonitorBrightness = Properties.Settings.Default.MaxMonitorBrightness;
                label8.Text = (Properties.Settings.Default.MaxMonitorBrightness / 10).ToString();
            }
            else
            {
                trackBar2.Value = trackBar1.Value;
            }
        }



        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            // przeskalowanie liniowe zakresu wejściowego [0 : 1000] na zakres [0.1 : 10]
            double intermediateValue = 2d * (trackBar3.Value / 1000d) - 1d;  // przeskalowanie pierwotnego zakresu [0 : 1000] do zakresu [-1 : 1]
            float gamma = (float) Math.Pow(10, intermediateValue); // obliczenie wartosci gamma w zakresie wyjściowym [0.1 : 10]
            Properties.Settings.Default.AmbientLightGammaCorrection = trackBar3.Value;
            Properties.Settings.Default.AmbientLightGammaCorrectionFactor = gamma;
            Properties.Settings.Default.Save();

            ThreadWorker.userAmbientLightGammaCorrectionFactor = Properties.Settings.Default.AmbientLightGammaCorrectionFactor;
            label9.Text = String.Format("{0:N3}", Properties.Settings.Default.AmbientLightGammaCorrectionFactor);
        }







        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }



    }
}
