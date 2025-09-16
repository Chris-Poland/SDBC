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
    public partial class Form_Closing : Form
    {
        private static readonly int WIDTH = 450;
        private static readonly int HEIGHT = 100;




        public Form_Closing()
        {
            InitializeComponent();
            SetWindowSize();
            SetWindowPosition();
            timer1.Enabled = true;
        }




        private void SetWindowSize()
        {
            Size = new Size(WIDTH, HEIGHT);
        }

        private void SetWindowPosition()
        {
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int xLocation = screenWidth - this.Width - 12;
            int yLocation = screenHeight - this.Height - 12;
            Location = new Point(xLocation, yLocation);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            DialogResult = DialogResult.OK;
        }




    }
}
