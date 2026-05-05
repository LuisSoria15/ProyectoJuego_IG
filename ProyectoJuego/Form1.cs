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
        private int offsetColor = 0;

        private Size tamanoOriginalBoton;
        private Size tamanoOriginalBoton2;

        //private Color[] paleta = { Color.DarkOrchid, Color.MediumPurple, Color.Plum, Color.MediumPurple };
        private Color[] paletaMario = { Color.Red, Color.DodgerBlue, Color.Yellow, Color.LimeGreen };

        private Point mouseLoc;

        // Variables para la conexión WebSocket
        public System.Net.WebSockets.ClientWebSocket wsCliente { get; set; }
        public System.Threading.CancellationTokenSource cancelToken { get; set; }

        // Variables para almacenar la información del jugador
        public static string IP_SERVIDOR = "10.220.96.135";

        public static string PUERTO = "11000";

        // Variables para almacenar la información del jugador
        public int IdJugadorActual { get; set; }
        public string NombreJugadorActual { get; set; }


        public Form1()
        {
            
            InitializeComponent();
            lblTitulo.Font = FontsManager.GetFipps(24);

            lblTitulo.AutoSize = true;

            lblTitulo.Location = new Point(
                (this.ClientSize.Width - lblTitulo.Width) / 2,
                lblTitulo.Location.Y
            );
            lblTitulo.ForeColor = Color.Transparent;
            lblTitulo.Paint += new PaintEventHandler(lblTitulo_Paint);
            timer1.Interval = 400;

            tamanoOriginalBoton = pictureBox1.Size;
            tamanoOriginalBoton2 = pictureBox2.Size;

            this.Opacity = 0.0;

            Timer timerAparicion = new Timer();
            timerAparicion.Interval = 15; 
            timerAparicion.Tick += TimerAparicion_Tick;
            timerAparicion.Start();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Close();

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
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Top -= 4;

            // Abrimos el formulario de inicio de sesión como "Dialog"
            InicioSesion formLogin = new InicioSesion(this);
            formLogin.Show();
            this.Hide();

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox2.Top -= 4;
            LeaderBoard ventana = new LeaderBoard(this, 1);
            ventana.Show();
            this.Hide();
        }
        private void CentrarLabel(Label lbl)
        {
            lbl.Left = (this.ClientSize.Width - lbl.Width) / 2;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            offsetColor++;
            lblTitulo.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Top += 4;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Top -= 4;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.juego_resplandor1;
            pictureBox1.Cursor = Cursors.Hand;

            int nuevoAncho = (int)(tamanoOriginalBoton.Width * 1.10);
            int nuevoAlto = (int)(tamanoOriginalBoton.Height * 1.10);
            pictureBox1.Size = new Size(nuevoAncho, nuevoAlto);

            pictureBox1.Location = new Point(
                pictureBox1.Location.X - (nuevoAncho - tamanoOriginalBoton.Width) / 2,
                pictureBox1.Location.Y - (nuevoAlto - tamanoOriginalBoton.Height) / 2
            );
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.jugar_normal;

            pictureBox1.Location = new Point(
                pictureBox1.Location.X + (pictureBox1.Width - tamanoOriginalBoton.Width) / 2,
                pictureBox1.Location.Y + (pictureBox1.Height - tamanoOriginalBoton.Height) / 2
            );
            pictureBox1.Size = tamanoOriginalBoton;
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2.Top -= 4;
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox2.Top += 4;
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.leader_board_resplandor1;
            pictureBox2.Cursor = Cursors.Hand;

            int nuevoAncho = (int)(tamanoOriginalBoton2.Width * 1.10);
            int nuevoAlto = (int)(tamanoOriginalBoton2.Height * 1.10);
            pictureBox2.Size = new Size(nuevoAncho, nuevoAlto);

            pictureBox2.Location = new Point(
                pictureBox2.Location.X - (nuevoAncho - tamanoOriginalBoton2.Width) / 2,
                pictureBox2.Location.Y - (nuevoAlto - tamanoOriginalBoton2.Height) / 2
            );
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.leader_board_normal1;

            pictureBox2.Location = new Point(
                pictureBox2.Location.X + (pictureBox2.Width - tamanoOriginalBoton2.Width) / 2,
                pictureBox2.Location.Y + (pictureBox2.Height - tamanoOriginalBoton2.Height) / 2
            );
            pictureBox2.Size = tamanoOriginalBoton2;
        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox3.Top -= 4;
        }

        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox3.Top += 4;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.Image = Properties.Resources.cerrar_resplandor;
            pictureBox3.Cursor = Cursors.Hand;
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            pictureBox3.Image = Properties.Resources.cerrar_normal;
        }

        private void lblTitulo_Paint(object sender, PaintEventArgs e)
        {
            string texto = lblTitulo.Text;
            float x = 0;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            Brush brochaSombra = new SolidBrush(Color.Black);

            for (int i = 0; i < texto.Length; i++)
            {
                int indice = (i + offsetColor) % paletaMario.Length;
                Brush brochaColor = new SolidBrush(paletaMario[indice]);
                string letra = texto[i].ToString();

                e.Graphics.DrawString(letra, lblTitulo.Font, brochaSombra, x + 2, 2);

                e.Graphics.DrawString(letra, lblTitulo.Font, brochaColor, x, 0);

                x += e.Graphics.MeasureString(letra, lblTitulo.Font).Width - 10;
            }
        }

        private void TimerAparicion_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1.0)
            {
                this.Opacity += 0.05;
            }
            else
            {
                Timer timer = (Timer)sender;
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
