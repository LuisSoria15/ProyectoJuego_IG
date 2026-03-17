using MySql.Data.MySqlClient;
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
        public string connectionString = "Server=bhuefshpv92bhb0wqb5n-mysql.services.clever-cloud.com;" +
                                        "Port=3306;" +
                                        "Database=bhuefshpv92bhb0wqb5n;" +
                                        "User ID=u7mcmeqwvuwiyurk;" +
                                        "Password=hwlYTA5OEtN6FXWbJowK;";
        private Form1 formPrincipal;
        private Point mouseLoc;

        private int puntosActuales = 0;

        private string textoPreguntaActual = "";

        public Preguntas(string nombreCategoria, Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;

            this.Size = formPrincipal.Size;

            lblScore.Font = FontsManager.GetFipps(14); 
            lblScore.AutoSize = false;
            lblScore.Width = 300;
            lblScore.Height = 60;
            lblScore.BackColor = Color.Transparent;
            lblScore.ForeColor = Color.Transparent; 
            lblScore.Location = new Point(20, 20);

            lblScore.Parent = this;
            lblScore.BringToFront();
           
            lblScore.Paint += new PaintEventHandler(lblScore_Paint);

            pregunta.Font = FontsManager.GetFipps(10);
            pregunta.AutoSize = false;

            pregunta.Width = this.ClientSize.Width - 100; 
            pregunta.Height = 100;

            pregunta.Location = new Point(50, 80);

            pregunta.BackColor = Color.Transparent;
            pregunta.ForeColor = Color.Transparent; 

            pregunta.Paint += new PaintEventHandler(pregunta_Paint);

            this.Opacity = 0.0;

            Timer timerAparicion = new Timer();
            timerAparicion.Interval = 15; 
            timerAparicion.Tick += TimerAparicion_Tick;
            timerAparicion.Start();

            cargaPreguntas();
        }
        
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
        private void Preguntas_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLoc = e.Location;
        }

        private void pbCerrar_Click(object sender, EventArgs e)
        {
            formPrincipal.Show();
            Close();
        }

        private void pregunta_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(textoPreguntaActual)) return;

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            Brush brochaColor = new SolidBrush(Color.Black);
            Brush brochaBorde = new SolidBrush(Color.White);

            StringFormat formato = new StringFormat();
            formato.Alignment = StringAlignment.Center;
            formato.LineAlignment = StringAlignment.Center;

            float x = 0;
            float y = 0;
            float w = pregunta.Width;
            float h = pregunta.Height;

            e.Graphics.DrawString(textoPreguntaActual, pregunta.Font, brochaBorde, new RectangleF(x - 2, y, w, h), formato);
            e.Graphics.DrawString(textoPreguntaActual, pregunta.Font, brochaBorde, new RectangleF(x + 2, y, w, h), formato);
            e.Graphics.DrawString(textoPreguntaActual, pregunta.Font, brochaBorde, new RectangleF(x, y - 2, w, h), formato);
            e.Graphics.DrawString(textoPreguntaActual, pregunta.Font, brochaBorde, new RectangleF(x, y + 2, w, h), formato);

            e.Graphics.DrawString(textoPreguntaActual, pregunta.Font, brochaColor, new RectangleF(x, y, w, h), formato);
        }

        private void cargaPreguntas()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Modificamos la consulta para traer también el formato
                    string queryPregunta = "SELECT enunciado, formato FROM preguntas WHERE categoria_id = 1 AND id = 2";
                    MySqlCommand cmdPregunta = new MySqlCommand(queryPregunta, conn);

                    string enunciado = "";
                    string formato = "";

                    // Volvemos a usar ExecuteReader porque ahora traemos 2 columnas
                    using (MySqlDataReader readerPregunta = cmdPregunta.ExecuteReader())
                    {
                        if (readerPregunta.Read())
                        {
                            enunciado = readerPregunta["enunciado"].ToString();
                            formato = readerPregunta["formato"].ToString();
                        }
                    }

                    // Si encontramos la pregunta, procedemos con las opciones
                    if (!string.IsNullOrEmpty(enunciado))
                    {
                        //pregunta.Text = enunciado; //tengo que quitar esto para dibuje la pregunta con el diseno del juego sorry

                        textoPreguntaActual = enunciado;

                        pregunta.Text = "";

                        pregunta.Invalidate();


                        // 2. Ocultamos todos los paneles primero para "limpiar" la pantalla
                        // (Asegúrate de cambiar estos nombres por los nombres reales de tus paneles)
                        panelTexto.Visible = false;
                        panelImagen.Visible = false;
                        panelAudio.Visible = false;

                        string queryOpciones = "SELECT contenido FROM opciones WHERE pregunta_id = 2";
                        MySqlCommand cmdOpciones = new MySqlCommand(queryOpciones, conn);

                        using (MySqlDataReader readerOpciones = cmdOpciones.ExecuteReader())
                        {
                            int i = 1;
                            while (readerOpciones.Read() && i <= 4)
                            {
                                string contenido = readerOpciones["contenido"].ToString();

                                // 3. Dependiendo del formato, mostramos el panel y asignamos los datos
                                if (formato == "texto")
                                {
                                    panelTexto.Visible = true;
                                    if (i == 1) btnOpcion1.Text = contenido;
                                    if (i == 2) btnOpcion2.Text = contenido;
                                    if (i == 3) btnOpcion3.Text = contenido;
                                    if (i == 4) btnOpcion4.Text = contenido;
                                }
                                else if (formato == "imagen")
                                {
                                    panelImagen.Visible = true;
                                    // En Windows Forms, PictureBox tiene la propiedad ImageLocation 
                                    // que carga automáticamente una imagen desde un enlace (URL)
                                    if (i == 1) CargarImagen(contenido, picOpcion1);
                                    if (i == 2) CargarImagen(contenido, picOpcion2);
                                    if (i == 3) CargarImagen(contenido, picOpcion3);
                                    if (i == 4) CargarImagen(contenido, picOpcion4);
                                }
                                else if (formato == "audio")
                                {
                                    panelAudio.Visible = true;
                                    // Para el audio, podrías guardar el enlace en la propiedad "Tag" del botón
                                    // Así, cuando le den clic a "Reproducir", sabes qué enlace usar.
                                    if (i == 1) btnAudio1.Tag = contenido;
                                    if (i == 2) btnAudio2.Tag = contenido;
                                    if (i == 3) btnAudio3.Tag = contenido;
                                    if (i == 4) btnAudio4.Tag = contenido;
                                }
                                i++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la pregunta: " + ex.Message);
                }
            }
            lblScore.Invalidate();
        }

        private void CargarImagen(string url, PictureBox picBox)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return;

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    // Engañamos a Wikipedia diciendo que somos un navegador web
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    // Descargamos la imagen
                    byte[] imageData = webClient.DownloadData(url);

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageData))
                    {
                        picBox.Image = Image.FromStream(ms);
                        picBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
            catch (Exception ex)
            {
                // Esto te dirá el error exacto si vuelve a fallar
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
            }
        }

        private void pbCerrar_MouseUp(object sender, MouseEventArgs e)
        {
            pbCerrar.Top -= 4;
        }

        private void pbCerrar_MouseDown(object sender, MouseEventArgs e)
        {
            pbCerrar.Left += 4;
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

        private void lblScore_Paint(object sender, PaintEventArgs e)
        {
           string texto = "PUNTOS: " + puntosActuales.ToString();

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            Brush brochaBorde = new SolidBrush(Color.Black);
            Brush brochaColor = new SolidBrush(Color.Gold);

            float x = 5;
            float y = 5;

            e.Graphics.DrawString(texto, lblScore.Font, brochaBorde, x - 2, y);
            e.Graphics.DrawString(texto, lblScore.Font, brochaBorde, x + 2, y);
            e.Graphics.DrawString(texto, lblScore.Font, brochaBorde, x, y - 2);
            e.Graphics.DrawString(texto, lblScore.Font, brochaBorde, x, y + 2);

            e.Graphics.DrawString(texto, lblScore.Font, brochaBorde, x + 3, y + 3);

            e.Graphics.DrawString(texto, lblScore.Font, brochaColor, x, y);
        }

        //quien haga lo de validar si la pregunta es correcta, solo mande a llamar esta funcion
        //con los puntos a darle por pregunta bien contestada
        //Ejemplo:
        //          SumarPuntos(100); //suponiendo que se le agrega 100 puntos
        private void SumarPuntos(int cantidad)
        {
            puntosActuales += cantidad;

            lblScore.Invalidate();
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
