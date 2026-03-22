using NAudio.Wave;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoJuego
{
    public partial class Preguntas : Form
    {
        // Configuración de Conexión
        public string connectionString = "Server=127.0.0.1;" +
                                         "Port=3306;" +
                                         "Database=preguntaslocal;" +
                                         "User ID=root;" +
                                         "Password=123456;";

        private Form1 formPrincipal;
        private Point mouseLoc;
        private int puntosActuales = 0;
        private int indiceActual = 0;
        private int idCategoriaSeleccionada;
        private List<PreguntaJuego> listaPreguntas = new List<PreguntaJuego>();
        private string textoPreguntaActual = "";

        private WaveOutEvent reproductorAudio;
        private MediaFoundationReader lectorAudio;
        private string indicador;

        private Random rng = new Random();

        public Preguntas(int idCategoria, Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
            this.idCategoriaSeleccionada = idCategoria;
            this.Size = formPrincipal.Size;

            ConfigurarDisenoInicial();
            AsignarEventos();

            // Se inicia carga y efectos
            IniciarEfectoAparicion();
            CargarTodasLasPreguntasDesdeBD();
            MostrarPreguntaActual();
        }
        //_____________________________________________________________________________________________________________________________________________________________

        #region Configuración de Interfaz y Diseño

        private void ConfigurarDisenoInicial()
        {
            // Se configura el Label de Score para que se dibuje con un estilo personalizado en el evento Paint
            lblScore.Font = FontsManager.GetFipps(14);
            lblScore.AutoSize = false;
            lblScore.Size = new Size(300, 60);
            lblScore.Location = new Point(20, 20);
            lblScore.BackColor = Color.Transparent;
            lblScore.ForeColor = Color.Transparent; // Se dibuja en el evento Paint
            lblScore.Paint += lblScore_Paint;

            // Se configura el Label de Pregunta para que se dibuje con un estilo personalizado en el evento Paint
            pregunta.Font = FontsManager.GetFipps(10);
            pregunta.AutoSize = false;
            pregunta.Size = new Size(this.ClientSize.Width - 100, 100);
            pregunta.Location = new Point(50, 80);
            pregunta.BackColor = Color.Transparent;
            pregunta.ForeColor = Color.Transparent;
            pregunta.Text = "";
            pregunta.Paint += pregunta_Paint;

            this.Opacity = 0.0;
        }

        private void AsignarEventos()
        {
            // Asignamos el mismo evento de validación a los botones de texto e imagen, y a las imágenes (que también funcionan como opciones)
            btnOpcion1.Click += ValidarRespuesta_Click;
            btnOpcion2.Click += ValidarRespuesta_Click;
            btnOpcion3.Click += ValidarRespuesta_Click;
            btnOpcion4.Click += ValidarRespuesta_Click;


            picOpcion1.Click += ValidarRespuesta_Click;
            picOpcion2.Click += ValidarRespuesta_Click;
            picOpcion3.Click += ValidarRespuesta_Click;
            picOpcion4.Click += ValidarRespuesta_Click;


            picAudio1.Click += EscucharAudio_Click;
            picAudio2.Click += EscucharAudio_Click;
            picAudio3.Click += EscucharAudio_Click;
            picAudio4.Click += EscucharAudio_Click;

            picAudio1.DoubleClick += SeleccionarAudio_DoubleClick;
            picAudio2.DoubleClick += SeleccionarAudio_DoubleClick;
            picAudio3.DoubleClick += SeleccionarAudio_DoubleClick;
            picAudio4.DoubleClick += SeleccionarAudio_DoubleClick;
        }

        private void IniciarEfectoAparicion()
        {
            Timer timerAparicion = new Timer { Interval = 15 };
            timerAparicion.Tick += (s, e) => {
                if (this.Opacity < 1.0) this.Opacity += 0.05;
                else { ((Timer)s).Stop(); ((Timer)s).Dispose(); }
            };
            timerAparicion.Start();
        }

        #endregion
        //_____________________________________________________________________________________________________________________________________________________________
        #region Lógica del Juego y Base de Datos

        private void CargarTodasLasPreguntasDesdeBD()
        {
            listaPreguntas.Clear();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT p.id AS PreguntaId, p.enunciado, p.formato, o.contenido, o.es_correcta
                        FROM preguntas p
                        INNER JOIN opciones o ON p.id = o.pregunta_id
                        WHERE p.categoria_id = @categoriaId
                        ORDER BY p.id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@categoriaId", idCategoriaSeleccionada);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        PreguntaJuego preguntaActual = null;
                        while (reader.Read())
                        {
                            int idPreguntaBD = Convert.ToInt32(reader["PreguntaId"]);

                            if (preguntaActual == null || preguntaActual.Id != idPreguntaBD)
                            {
                                preguntaActual = new PreguntaJuego
                                {
                                    Id = idPreguntaBD,
                                    Enunciado = reader["enunciado"].ToString(),
                                    Formato = reader["formato"].ToString()
                                };
                                listaPreguntas.Add(preguntaActual);
                            }

                            preguntaActual.Opciones.Add(new OpcionJuego
                            {
                                Contenido = reader["contenido"].ToString(),
                                EsCorrecta = Convert.ToBoolean(reader["es_correcta"])
                            });
                        }
                    }
                    if (listaPreguntas.Count > 0)
                    {
                        MezclarPreguntas(listaPreguntas);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de BD: " + ex.Message);
                }
            }
        }

        private void MezclarOpciones(List<OpcionJuego> opciones)
        {
            int n = opciones.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1); 

                OpcionJuego valorTemporal = opciones[k];
                opciones[k] = opciones[n];
                opciones[n] = valorTemporal;
            }
        }

        private void MezclarPreguntas(List<PreguntaJuego> preguntas)
        {
            int n = preguntas.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                PreguntaJuego valorTemporal = preguntas[k];
                preguntas[k] = preguntas[n];
                preguntas[n] = valorTemporal;
            }
        }

        private void MostrarPreguntaActual()
        {
            DetenerAudio();
            if (indiceActual >= listaPreguntas.Count)
            {
                MessageBox.Show($"¡Juego Terminado! Puntos totales: {puntosActuales}");
                CerrarYRegresar();
                return;
            }

            PreguntaJuego pActual = listaPreguntas[indiceActual];
            // Mezclamos las opciones para que no siempre estén en el mismo orden (esto no afecta la lógica de validación porque cada opción tiene su propiedad EsCorrecta)
            MezclarOpciones(pActual.Opciones);

            // Actualizar texto para el dibujo personalizado
            textoPreguntaActual = pActual.Enunciado;
            pregunta.Invalidate();

            // Limpiar paneles
            panelTexto.Visible = panelImagen.Visible = panelAudio.Visible = false;

            if (pActual.Formato == "texto")
            {
                int centroX = (this.ClientSize.Width - panelTexto.Width) / 2;
                panelTexto.Location = new Point(centroX, 250);
                panelTexto.Visible = true;
                ConfigurarBoton(btnOpcion1, pActual.Opciones[0]);
                ConfigurarBoton(btnOpcion2, pActual.Opciones[1]);
                ConfigurarBoton(btnOpcion3, pActual.Opciones[2]);
                ConfigurarBoton(btnOpcion4, pActual.Opciones[3]);
                indicador = "texto";
            }
            else if (pActual.Formato == "imagen")
            {
                int centroX = (this.ClientSize.Width - panelImagen.Width) / 2;
                panelImagen.Location = new Point(centroX, 200);

                panelImagen.Visible = true;
                CargarImagen(pActual.Opciones[0].Contenido, picOpcion1, pActual.Opciones[0].EsCorrecta);
                CargarImagen(pActual.Opciones[1].Contenido, picOpcion2, pActual.Opciones[1].EsCorrecta);
                CargarImagen(pActual.Opciones[2].Contenido, picOpcion3, pActual.Opciones[2].EsCorrecta);
                CargarImagen(pActual.Opciones[3].Contenido, picOpcion4, pActual.Opciones[3].EsCorrecta);
                indicador = "imagen";
            }
            else if(pActual.Formato == "audio")
            {
                int centroX = (this.ClientSize.Width - panelAudio.Width) / 2;
                panelAudio.Location = new Point(centroX, 250);

                panelAudio.Visible = true;
                CargarAudio(pActual.Opciones[0].Contenido, picAudio1, pActual.Opciones[0].EsCorrecta);
                CargarAudio(pActual.Opciones[1].Contenido, picAudio2, pActual.Opciones[1].EsCorrecta);
                CargarAudio(pActual.Opciones[2].Contenido, picAudio3, pActual.Opciones[2].EsCorrecta);
                CargarAudio(pActual.Opciones[3].Contenido, picAudio4, pActual.Opciones[3].EsCorrecta);
                indicador = "audio";

            }
        }



        private void ConfigurarBoton(Button btn, OpcionJuego opcion)
        {
            btn.Text = opcion.Contenido;
            btn.Tag = opcion.EsCorrecta;
        }

        private async void CargarImagen(string url, PictureBox picBox, bool esCorrecta)
        {
            picBox.Tag = esCorrecta;
            picBox.Image = null; // Limpiar imagen previa
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return;
                using (WebClient webClient = new WebClient())
                {
                    //esto es por que algunas imagenes no se cargaban por el user agent, asi que se lo agregue para que se haga pasar por un navegador
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0");
                    byte[] imageData = await webClient.DownloadDataTaskAsync(url);
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        picBox.Image = Image.FromStream(ms);
                        picBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
            catch {  }
        }

        private void DetenerAudio()
        {
            if (reproductorAudio != null)
            {
                reproductorAudio.Stop();
                reproductorAudio.Dispose();
                reproductorAudio = null;
            }
            if (lectorAudio != null)
            {
                lectorAudio.Dispose();
                lectorAudio = null;
            }
        }

        private void ReproducirMP3(string url)
        {
            DetenerAudio(); // Siempre detenemos el anterior antes de poner uno nuevo

            try
            {
                lectorAudio = new MediaFoundationReader(url);
                reproductorAudio = new WaveOutEvent();
                reproductorAudio.Init(lectorAudio);
                reproductorAudio.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reproducir el audio: " + ex.Message);
            }
        }

        // Este método carga la información oculta en el botón
        private void CargarAudio(string url, PictureBox picReproducir, bool esCorrecta)
        {
            // Guardamos la URL y si es correcta separados por un símbolo |
            picReproducir.Tag = url + "|" + esCorrecta.ToString();
        }

        // Este evento es para ESCUCHAR (Asígnalo al evento Click de tus 4 botones de audio)
        private void EscucharAudio_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            if (control.Tag == null) return;

            string[] datos = control.Tag.ToString().Split('|');
            string urlAudio = datos[0];

            ReproducirMP3(urlAudio); // Solo reproduce, no avanza el juego
        }

        // Este evento es para ELEGIR LA RESPUESTA (Asígnalo al evento DoubleClick)
        private void SeleccionarAudio_DoubleClick(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            if (control.Tag == null) return;

            string[] datos = control.Tag.ToString().Split('|');
            bool esCorrecta = bool.Parse(datos[1]);

            DetenerAudio(); // Callamos el audio al elegir

            if (esCorrecta)
            {
                SumarPuntos(100);
                MessageBox.Show("¡Correcto!");
            }
            else
            {
                MessageBox.Show("Incorrecto...");
            }

            indiceActual++;
            MostrarPreguntaActual();
        }


        private void ValidarRespuesta_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            if (control.Tag != null && (bool)control.Tag)
            {
                SumarPuntos(100);
                MessageBox.Show("¡Correcto!");
            }
            else
            {
                MessageBox.Show("Incorrecto...");
            }

            indiceActual++;
            MostrarPreguntaActual();
        }

        private void SumarPuntos(int cantidad)
        {
            puntosActuales += cantidad;
            lblScore.Invalidate(); // Redibuja el score con el nuevo puntaje
        }

        
        #endregion
        //_____________________________________________________________________________________________________________________________________________________________

        #region Eventos de Dibujo (Paint)

        private void pregunta_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(textoPreguntaActual)) return;
            DibujarTextoConBorde(e.Graphics, textoPreguntaActual, pregunta.Font, Color.White, Color.Black, pregunta.ClientRectangle);
        }

        private void lblScore_Paint(object sender, PaintEventArgs e)
        {
            string texto = "PUNTOS: " + puntosActuales;
            DibujarTextoConBorde(e.Graphics, texto, lblScore.Font, Color.Black, Color.Gold, new Rectangle(5, 5, 300, 60), true);
        }

        private void DibujarTextoConBorde(Graphics g, string texto, Font fuente, Color colorBorde, Color colorTexto, Rectangle rect, bool esScore = false)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            StringFormat formato = new StringFormat 
            { 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.Word
            };

            if (esScore) 
            {
                formato.Alignment = StringAlignment.Near;
                formato.LineAlignment = StringAlignment.Near;
            }

            using (Brush bBorde = new SolidBrush(colorBorde))
            using (Brush bTexto = new SolidBrush(colorTexto))
            {
                // Dibujar contorno (Offsets)
                for (int x = -2; x <= 2; x += 2)
                    for (int y = -2; y <= 2; y += 2)
                        if (x != 0 || y != 0) g.DrawString(texto, fuente, bBorde, new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height), formato);

                // Dibujar texto principal
                g.DrawString(texto, fuente, bTexto, rect, formato);
            }
        }

        #endregion
        //_____________________________________________________________________________________________________________________________________________________________

        #region Controles de Ventana (Mover, Cerrar)

        private void pbCerrar_Click(object sender, EventArgs e) => CerrarYRegresar();

        private void CerrarYRegresar()
        {
            DetenerAudio();
            formPrincipal.Show();
            this.Close();
        }

        private void Preguntas_MouseDown(object sender, MouseEventArgs e) => mouseLoc = e.Location;

        private void Preguntas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = new Point(this.Location.X + (e.X - mouseLoc.X), this.Location.Y + (e.Y - mouseLoc.Y));
        }

        // Efectos del botón cerrar (Asegúrate que los nombres coincidan con tus recursos)
        private void pbCerrar_MouseEnter(object sender, EventArgs e) { pbCerrar.Image = Properties.Resources.cerrar_resplandor; Cursor = Cursors.Hand; }
        private void pbCerrar_MouseLeave(object sender, EventArgs e) { pbCerrar.Image = Properties.Resources.cerrar_normal; }

        #endregion

        private void btnOpcion4_Click(object sender, EventArgs e)
        {

        }

        private void picOpcion2_Click(object sender, EventArgs e)
        {

        }
    }
}