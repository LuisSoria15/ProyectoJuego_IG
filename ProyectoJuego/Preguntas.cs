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
    public partial class Preguntas : Form
    {
        public Preguntas()
        {
            InitializeComponent();
        }

        private Point mouseLoc;

        private void Preguntas_Load(object sender, EventArgs e)
        {

        }

        private void Preguntas_MouseMove(object sender, MouseEventArgs e)
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
    }
}
