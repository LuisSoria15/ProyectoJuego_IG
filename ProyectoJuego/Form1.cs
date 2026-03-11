using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoJuego
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }
        private Point mouseLoc;


        private void button1_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void FormMenu_Load(object sender, EventArgs e)
        {

        }

        private void FormMenu_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouseLoc.X;
                int dy = e.Location.Y - mouseLoc.Y;
                dx += this.Location.X;
                dy += this.Location.Y;
                this.Location = new Point(dx, dy);
            }
        }

        private void FormMenu_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLoc = e.Location;
        }

        private void btnLeaderboard_Click(object sender, EventArgs e)
        {
            LeaderBoard ventana = new LeaderBoard(this);
            ventana.Show();
            this.Hide();
        }

        private void btnJugar_Click(object sender, EventArgs e)
        {
            Categorias ventana = new Categorias(this);
            ventana.Show();
            this.Hide();
        }
    }
}
