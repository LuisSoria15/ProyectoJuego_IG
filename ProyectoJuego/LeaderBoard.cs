using Newtonsoft.Json;
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
        private Form ventanaEspera;
        private Point mouseLoc;

        private Timer timerEstrellas;
        private List<Estrella> listaEstrellas = new List<Estrella>();
        private Image imagenEstrella;

        private DataGridView dgvLeaderboard;

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

            ConfigurarTabla(); 

            Timer timerAparicion = new Timer();
            timerAparicion.Interval = 15;
            timerAparicion.Tick += TimerAparicion_Tick;
            timerAparicion.Start();
        }

        private void LeaderBoard_Load(object sender, EventArgs e)
        {
            // Apenas entra, muestra el mensaje de espera
            MostrarMensajeEspera();
            _ = EscucharServidor();
        }

        private void MostrarMensajeEspera()
        {
            ventanaEspera = new Form();
            ventanaEspera.FormBorderStyle = FormBorderStyle.None;
            ventanaEspera.StartPosition = FormStartPosition.CenterScreen;
            ventanaEspera.Size = new Size(400, 100);
            ventanaEspera.BackColor = Color.DarkSlateBlue;

            Label lbl = new Label();
            lbl.Text = "Esperando a que el otro jugador termine...";
            lbl.ForeColor = Color.White;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Arial", 12, FontStyle.Bold);

            ventanaEspera.Controls.Add(lbl);
            ventanaEspera.Show(this);
        }

        private async Task EscucharServidor()
        {
            byte[] buffer = new byte[2048];
            try
            {
                while (formPrincipal.wsCliente.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var result = await formPrincipal.wsCliente.ReceiveAsync(new ArraySegment<byte>(buffer), formPrincipal.cancelToken.Token);

                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        await formPrincipal.wsCliente.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", formPrincipal.cancelToken.Token);
                        MessageBox.Show("El servidor cerró la conexión inesperadamente.");
                        break;
                    }
                    else
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        dynamic datos = JsonConvert.DeserializeObject(json);

                        if (datos != null && datos.accion == "mostrar_ganador")
                        {
                            // Armamos el texto ANTES de tocar la interfaz
                            string mensajeGanador;
                            if ((bool)datos.empate)
                            {
                                mensajeGanador = $"¡Es un EMPATE con {datos.puntaje_ganador} puntos!";
                            }
                            else
                            {
                                mensajeGanador = $"¡El ganador es {datos.ganador} con {datos.puntaje_ganador} puntos!";
                            }

                            this.Invoke((MethodInvoker)delegate
                            {
                                if (ventanaEspera != null) ventanaEspera.Close();
                            });

                            // 2. Mostramos el ganador (esto pausa 3 segundos)
                            await AnunciarGanadorTemporal(mensajeGanador);

                            // 3. Llenamos y mostramos la tabla con los resultados <--- NUEVO
                            this.Invoke((MethodInvoker)delegate
                            {
                                dgvLeaderboard.Rows.Clear(); // Limpiamos por si acaso

                                // Recorremos el arreglo "resultados" que mandó Python
                                foreach (var jugador in datos.resultados)
                                {
                                    // Agregamos una fila con el nombre y el puntaje
                                    dgvLeaderboard.Rows.Add(jugador.nombre.ToString(), jugador.puntaje.ToString());
                                }

                                dgvLeaderboard.Visible = true; // Hacemos visible la tabla
                            });

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ¡AQUÍ ESTÁ LA MAGIA! Si algo falla, ahora sí te saldrá una alerta
                MessageBox.Show("Error crítico en la sala de resultados: " + ex.Message);
            }
        }

        private async Task AnunciarGanadorTemporal(string mensaje)
        {
            Form frmAnuncio = null;

            // Creamos y mostramos la ventana en el hilo principal
            this.Invoke((MethodInvoker)delegate {
                frmAnuncio = new Form();
                frmAnuncio.FormBorderStyle = FormBorderStyle.None;
                frmAnuncio.StartPosition = FormStartPosition.CenterScreen;
                frmAnuncio.Size = new Size(600, 150);
                frmAnuncio.BackColor = Color.Gold;

                Label lbl = new Label();
                lbl.Text = mensaje;
                lbl.ForeColor = Color.Black;
                lbl.Dock = DockStyle.Fill;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Arial", 16, FontStyle.Bold);

                frmAnuncio.Controls.Add(lbl);
                frmAnuncio.Show(this);
            });

            // Pausamos 3 segundos afuera del hilo principal (evita que el juego se trabe)
            await Task.Delay(3000);

            // Cerramos la ventana en el hilo principal
            this.Invoke((MethodInvoker)delegate {
                if (frmAnuncio != null) frmAnuncio.Close();
            });
        }

        private void ConfigurarTabla()
        {
            dgvLeaderboard = new DataGridView();
            dgvLeaderboard.Size = new Size(400, 200);

            // Centramos la tabla en la pantalla, un poco más abajo del título
            dgvLeaderboard.Location = new Point((this.ClientSize.Width - 400) / 2, 120);

            // Configuraciones visuales para que parezca de juego y no de Excel
            dgvLeaderboard.BackgroundColor = this.BackColor;
            dgvLeaderboard.BorderStyle = BorderStyle.None;
            dgvLeaderboard.AllowUserToAddRows = false;
            dgvLeaderboard.ReadOnly = true;
            dgvLeaderboard.RowHeadersVisible = false; // Oculta la flechita lateral
            dgvLeaderboard.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLeaderboard.AllowUserToResizeColumns = false;
            dgvLeaderboard.AllowUserToResizeRows = false;

            // Colores y fuentes de las filas
            dgvLeaderboard.DefaultCellStyle.BackColor = Color.DarkSlateBlue;
            dgvLeaderboard.DefaultCellStyle.ForeColor = Color.White;
            dgvLeaderboard.DefaultCellStyle.SelectionBackColor = Color.Gold;
            dgvLeaderboard.DefaultCellStyle.SelectionForeColor = Color.Black;
            // Puedes cambiar "Arial" por FontsManager.GetFipps(10) si quieres la fuente pixelada
            dgvLeaderboard.DefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            dgvLeaderboard.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Colores y fuentes del encabezado
            dgvLeaderboard.EnableHeadersVisualStyles = false;
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.BackColor = Color.Gold;
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 14, FontStyle.Bold);
            dgvLeaderboard.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Agregamos las dos columnas
            dgvLeaderboard.Columns.Add("Jugador", "JUGADOR");
            dgvLeaderboard.Columns.Add("Puntaje", "PUNTAJE");

            dgvLeaderboard.Visible = false; // La mantenemos oculta hasta que termine el anuncio
            this.Controls.Add(dgvLeaderboard);
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
