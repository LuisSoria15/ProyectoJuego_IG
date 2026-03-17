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
    public partial class LeaderBoard : Form
    {
        private Form1 formPrincipal;
        private Point mouseLoc;

        private Timer timerEstrellas;
        private List<Estrella> listaEstrellas = new List<Estrella>();
        private Image imagenEstrella;
        
        private class Estrella
        {
            public PointF Posicion;
            public float Velocidad;
            public int Tamaño;
            public float Balanceo;
        }
        public LeaderBoard(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;

            this.Size = formPrincipal.Size;
            this.BackColor = formPrincipal.BackColor;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;

            lblTituloLeader.BackColor = Color.Transparent;
            pbCerrar.BackColor = Color.Transparent;

            lblTituloLeader.Font = FontsManager.GetFipps(20);
            lblTituloLeader.AutoSize = true;
            lblTituloLeader.ForeColor = Color.Transparent;
            lblTituloLeader.Paint += new PaintEventHandler(lblTituloLeader_Paint);

            lblTituloLeader.Location = new Point(
                (this.ClientSize.Width - lblTituloLeader.Width) / 2,
                30
            );

            imagenEstrella = Properties.Resources.estrellaleaderboard1;
            CrearEstrellasIniciales(40);
            timerEstrellas = new Timer();
            timerEstrellas.Interval = 30;
            timerEstrellas.Tick += TimerEstrellas_Tick;
            timerEstrellas.Start();
            this.DoubleBuffered = true;

            this.Opacity = 0.0;

            Timer timerAparicion = new Timer();
            timerAparicion.Interval = 15;
            timerAparicion.Tick += TimerAparicion_Tick;
            timerAparicion.Start();
        }

        private void CrearEstrellasIniciales(int count)
        {
            Random rnd = new Random();
            int margin = (int)(this.ClientSize.Width * 0.3);

            for (int i = 0; i < count; i++)
            {
                bool isLeft = rnd.Next(2) == 0;
                float x = isLeft ? rnd.Next(10, margin) : rnd.Next(this.ClientSize.Width - margin, this.ClientSize.Width - 10);
                float y = rnd.Next(0, this.ClientSize.Height);

                listaEstrellas.Add(new Estrella
                {
                    Posicion = new PointF(x, y),
                    Velocidad = (float)rnd.NextDouble() * 3 + 1,

                    Tamaño = rnd.Next(20, 45),
                    Balanceo = (float)rnd.NextDouble() * (float)Math.PI * 2
                });
            }
        }

        private void TimerEstrellas_Tick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            foreach (var estrella in listaEstrellas)
            {
                estrella.Posicion.Y -= estrella.Velocidad;
                estrella.Balanceo += 0.1f;
                estrella.Posicion.X += (float)Math.Sin(estrella.Balanceo) * 0.5f;

                if (estrella.Posicion.Y < -estrella.Tamaño)
                {
                    bool isLeft = rnd.Next(2) == 0;

                    int margin = (int)(this.ClientSize.Width * 0.3);
                    float x = isLeft ? rnd.Next(10, margin) : rnd.Next(this.ClientSize.Width - margin, this.ClientSize.Width - 10);

                    estrella.Tamaño = rnd.Next(20, 45);
                    estrella.Posicion = new PointF(x, this.ClientSize.Height + rnd.Next(10, 50));
                }
            }
            this.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void LeaderBoard_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLoc = e.Location;
        }

        private void LeaderBoard_MouseMove(object sender, MouseEventArgs e)
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

        private void LeaderBoard_Paint(object sender, PaintEventArgs e)
        {
            if (imagenEstrella != null)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                foreach (var estrella in listaEstrellas)
                {
                    e.Graphics.DrawImage(imagenEstrella,
                                         estrella.Posicion.X, estrella.Posicion.Y,
                                         estrella.Tamaño, estrella.Tamaño);
                }
            }
        }

        private void lblTituloLeader_Paint(object sender, PaintEventArgs e)
        {
            string texto = lblTituloLeader.Text;
            float x = 0;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            Brush brochaBorde = new SolidBrush(Color.Black);
            Brush brochaColor = new SolidBrush(Color.Gold);

            for (int i = 0; i < texto.Length; i++)
            {
                string letra = texto[i].ToString();

                e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaBorde, x - 2, 0); // Izquierda
                e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaBorde, x + 2, 0); // Derecha
                e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaBorde, x, -2); // Arriba
                e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaBorde, x, +2); // Abajo

                 e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaBorde, x + 4, 4);

                e.Graphics.DrawString(letra, lblTituloLeader.Font, brochaColor, x, 0);

                x += e.Graphics.MeasureString(letra, lblTituloLeader.Font).Width - 10;
            }
        }

        private void pbCerrar_Click(object sender, EventArgs e)
        {
            timerEstrellas.Stop();
            pbCerrar.Top -= 4;
            formPrincipal.Show();
            this.Close();
        }

        private void pbCerrar_MouseUp(object sender, MouseEventArgs e)
        {
            pbCerrar.Top -= 4;
        }

        private void pbCerrar_MouseDown(object sender, MouseEventArgs e)
        {
            pbCerrar.Top += 4;
        }

        private void pbCerrar_MouseEnter(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_resplandor;
            pbCerrar.Cursor = Cursors.Hand;
        }

        private void pbCerrar_MouseLeave(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_normal;
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
