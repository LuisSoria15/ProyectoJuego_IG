using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoJuego
{
    public partial class Preguntas : Form
    {

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

        //Variables para la barra de tiempo
        private Timer timerTiempo;
        private int tiempoMaximo = 150; //equivale a 15 segundos, modificarlo para el tiempo requerido
        private int tiempoActual = 100;
        private PictureBox pbBarraTiempo;
        //
        private Random rng = new Random();
            
        public Preguntas(int idCategoria, Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
            this.idCategoriaSeleccionada = idCategoria;

            this.Size = new Size(formPrincipal.Width, formPrincipal.Height + 120);

            this.StartPosition = FormStartPosition.CenterScreen;

            ConfigurarDisenoInicial();
            ConfigurarControlesDeTextoCustom();
            AsignarEventos();

            // Iniciar carga y efectos
            IniciarEfectoAparicion();
            //MostrarPreguntaActual();
        }

        private async void Preguntas_Load(object sender, EventArgs e)
        {
            await CargarTodasLasPreguntasDesdeAPI();

            // Una vez que ya bajó y revolvió las preguntas, ahora sí mostramos la primera
            MostrarPreguntaActual();
        }
        //_____________________________________________________________________________________________________________________________________________________________

        #region Configuración de Interfaz y Diseño

        private void ConfigurarControlesDeTextoCustom()
        {
            // 1. Asignar el PictureBox como padre del Label
            label1.Parent = opcion1;
            label2.Parent = opcion2;
            label3.Parent = opcion3;
            label4.Parent = opcion4;

            // 2. Asegurar que el fondo del Label sea transparente al PictureBox
            label1.BackColor = Color.Transparent;
            label2.BackColor = Color.Transparent;
            label3.BackColor = Color.Transparent;
            label4.BackColor = Color.Transparent;

   
            // Desactivamos AutoSize para controlar nosotros el tamaño
            label1.AutoSize = false;
            label2.AutoSize = false;
            label3.AutoSize = false;
            label4.AutoSize = false;

            // Hacemos que el Label ocupe todo el espacio del PictureBox parent
            
            label1.Dock = DockStyle.Fill;
            label2.Dock = DockStyle.Fill;
            label3.Dock = DockStyle.Fill;
            label4.Dock = DockStyle.Fill;
            
                    
            // Centramos el texto dentro del Label
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label4.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void ConfigurarDisenoInicial()
        {
            // Configurar Label de Score (Dibujo personalizado)
            lblScore.Font = FontsManager.GetFipps(14);
            lblScore.AutoSize = false;
            lblScore.Size = new Size(300, 60);
            lblScore.Location = new Point(20, 20);
            lblScore.BackColor = Color.Transparent;
            lblScore.ForeColor = Color.Transparent; // Se dibuja en el evento Paint
            lblScore.Paint += lblScore_Paint;

            // Configurar Label de Pregunta (Dibujo personalizado)
            pregunta.Font = FontsManager.GetFipps(10);
            pregunta.AutoSize = false;
            pregunta.Size = new Size(this.ClientSize.Width - 100, 100);
            pregunta.Location = new Point(50, 80);
            pregunta.BackColor = Color.Transparent;
            pregunta.ForeColor = Color.Transparent;
            pregunta.Text = "";
            pregunta.Paint += pregunta_Paint;

            this.Opacity = 0.0;

            //Configuracion de la barra de tiempo
            pbBarraTiempo = new PictureBox();
            pbBarraTiempo.Height = 25;
            pbBarraTiempo.Width = this.ClientSize.Width - 100;
            pbBarraTiempo.Location = new Point(50, this.ClientSize.Height - 60);
            pbBarraTiempo.BackColor = Color.Transparent;
            pbBarraTiempo.Paint += pbBarraTiempo_Paint;
            this.Controls.Add(pbBarraTiempo);

            //para motor de tiempo
            timerTiempo = new Timer();
            timerTiempo.Interval = 100;
            timerTiempo.Tick += TimerTiempo_Tick;
        }

        private void AsignarEventos()
        {
            // Eventos para las opciones de texto (PictureBox y Labels)
            opcion1.Click += ValidarRespuesta_Click;
            opcion2.Click += ValidarRespuesta_Click;
            opcion3.Click += ValidarRespuesta_Click;
            opcion4.Click += ValidarRespuesta_Click;

            label1.Click += ValidarRespuesta_Click;
            label2.Click += ValidarRespuesta_Click;
            label3.Click += ValidarRespuesta_Click;
            label4.Click += ValidarRespuesta_Click;


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

        // Lo renombramos y le ponemos async Task
        private async Task CargarTodasLasPreguntasDesdeAPI()
        {
            listaPreguntas.Clear();

            // Usamos la IP y puerto que acabamos de depurar, y le concatenamos el ID de la categoría
            string urlApi = $"http://10.17.217.135:11000/preguntas/{idCategoriaSeleccionada}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 1. Pedimos los datos al servidor Python
                    HttpResponseMessage respuesta = await client.GetAsync(urlApi);
                    respuesta.EnsureSuccessStatusCode(); // Verifica que no haya error 404 o 500

                    // 2. Leemos el texto JSON
                    string jsonString = await respuesta.Content.ReadAsStringAsync();
                    MessageBox.Show("Esto me mandó Python:\n\n" + jsonString);

                    // Opcional: Descomenta esta línea si quieres ver el JSON en pantalla antes de convertirlo
                    // MessageBox.Show("JSON Recibido:\n" + jsonString);

                    // 3. LA MAGIA: Convertimos el texto anidado directamente a tu lista de objetos
                    listaPreguntas = JsonConvert.DeserializeObject<List<PreguntaJuego>>(jsonString);

                    // 4. Si llegaron preguntas, las revolvemos tal como lo hacías antes
                    if (listaPreguntas != null && listaPreguntas.Count > 0)
                    {
                        MezclarPreguntas(listaPreguntas);
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron preguntas para esta categoría.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar preguntas del servidor: " + ex.Message);
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
            // Mezclamos las opciones para que no siempre estén en el mismo orden 
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

          
                ConfigurarOpcionTexto(label1, opcion1, pActual.Opciones[0]);
                ConfigurarOpcionTexto(label2, opcion2, pActual.Opciones[1]);
                ConfigurarOpcionTexto(label3, opcion3, pActual.Opciones[2]);
                ConfigurarOpcionTexto(label4, opcion4, pActual.Opciones[3]);

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
            //reiniciar y arrancar el temporizador
            tiempoActual = tiempoMaximo;
            timerTiempo.Start();
        }


        private void ConfigurarOpcionTexto(Label lbl, PictureBox pic, OpcionJuego opcion)
        {
            lbl.Text = opcion.Contenido;

            // Asignamos el valor booleano (EsCorrecta) al Tag de ambos controles.
            lbl.Tag = opcion.EsCorrecta;
            pic.Tag = opcion.EsCorrecta;
        }

        private void ConfigurarBoton(Button btn, OpcionJuego opcion)
        {
            btn.Text = opcion.Contenido;
            btn.Tag = opcion.EsCorrecta;
        }

        private void CargarImagen(string nombreArchivo, PictureBox picBox, bool esCorrecta)
        {
            picBox.Tag = esCorrecta;
            picBox.Image = null; // Limpiar imagen previa

            if (string.IsNullOrWhiteSpace(nombreArchivo)) return;

            
            string rutaAbsoluta = Path.Combine(Application.StartupPath, "Recursos", "Imagenes", nombreArchivo);

            try
            {
                
                if (File.Exists(rutaAbsoluta))
                {
                    // Usamos FileStream en lugar de Image.FromFile para que C# no "bloquee" el archivo
                    using (FileStream fs = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read))
                    {
                        picBox.Image = Image.FromStream(fs);
                        picBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
                else
                {
                    
                    Console.WriteLine("¡Falta la imagen!: " + rutaAbsoluta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar imagen local: {ex.Message}");
            }
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

        private void ReproducirMP3(string nombreArchivo)
        {
            DetenerAudio(); 

            if (string.IsNullOrWhiteSpace(nombreArchivo)) return;

            string rutaAbsoluta = Path.Combine(Application.StartupPath, "Recursos", "Audios", nombreArchivo);

            if (!File.Exists(rutaAbsoluta))
            {
                MessageBox.Show("Falta el archivo de audio: " + nombreArchivo);
                return;
            }

            try
            {
                
                lectorAudio = new MediaFoundationReader(rutaAbsoluta);
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

        // Evento para ELEGIR LA RESPUESTA
        private async void SeleccionarAudio_DoubleClick(object sender, EventArgs e)
        {
            timerTiempo.Stop();//Pausa el reloj

            Control control = (Control)sender;

            if (control.Tag == null) return;

            string[] datos = control.Tag.ToString().Split('|');
            bool esCorrecta = bool.Parse(datos[1]);

            DetenerAudio(); // Callamos el audio al elegir

            if (esCorrecta)
            {
                SumarPuntos(100);
                //Se pone pantalla en verde
                await MostrarFeedback(true);
                //MessageBox.Show("¡Correcto!");
            }
            else
            {
                //Se pone pantalla en rojo
                //MessageBox.Show("Incorrecto...");
                await MostrarFeedback(false);
            }

            indiceActual++;
            MostrarPreguntaActual();
        }


        private async void ValidarRespuesta_Click(object sender, EventArgs e)
        {
            timerTiempo.Stop();//Pausa el reloj
            
            Control control = (Control)sender;
            if (control.Tag != null && (bool)control.Tag)
            {
                SumarPuntos(100);
                //Se pone la pantalla verde
                await MostrarFeedback(true);
                //MessageBox.Show("¡Correcto!");
            }
            else
            {
                //Se pone la pantalla roja
                await MostrarFeedback(false);
                //MessageBox.Show("Incorrecto...");
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
        private void TimerTiempo_Tick(object sender, EventArgs e)
        {
            tiempoActual--;
            pbBarraTiempo.Invalidate();

            if(tiempoActual <= 0)
            {
                //Pausar el reloj
                timerTiempo.Stop();

                //Anvanzamos el indice para la siguente pregunta
                indiceActual++;

                //Se carga la siguiente pregunta
                MostrarPreguntaActual();
            }
        }  
        private void pbBarraTiempo_Paint(object sender, PaintEventArgs e)
        {
            float porcentaje = (float)tiempoActual / tiempoMaximo;
            if(porcentaje < 0) 
                porcentaje = 0;
            int anchoRelleno = (int)(pbBarraTiempo.Width * porcentaje);

            //colores de la barra
            Brush brochaRelleno = Brushes.LimeGreen;
            if (porcentaje <= 0.5f)
                brochaRelleno = Brushes.Gold;
            if (porcentaje <= 0.2f)
                brochaRelleno = Brushes.Crimson;

            //hueco que va dejando
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(80, 0, 0, 0)), 0, 0, pbBarraTiempo.Width, pbBarraTiempo.Height);

            //barra de color
            if (anchoRelleno > 0)
                e.Graphics.FillRectangle(brochaRelleno, 0, 0, anchoRelleno,pbBarraTiempo.Height);

            //contorno de la barra
            using (Pen penBorde = new Pen(Color.Black, 4))
                e.Graphics.DrawRectangle(penBorde, 0, 0, pbBarraTiempo.Width, pbBarraTiempo.Height);
        }
        #endregion
        //_____________________________________________________________________________________________________________________________________________________________

        #region Controles de Ventana (Mover, Cerrar)

        private void pbCerrar_Click(object sender, EventArgs e) => CerrarYRegresar();

        private void CerrarYRegresar()
        {
            timerTiempo.Stop();//Pausa el reloj
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


        private void label1_MouseEnter(object sender, EventArgs e)
        {
            opcion1.Width += 10;
            opcion1.Height += 10;
            opcion1.Top -= 5;
            opcion1.Left -= 5;
            opcion1.Cursor = Cursors.Hand;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            opcion1.Width -= 10;
            opcion1.Height -= 10;
            opcion1.Top += 5;
            opcion1.Left += 5;
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            opcion2.Width += 10;
            opcion2.Height += 10;
            opcion2.Top -= 5;
            opcion2.Left -= 5;
            opcion2.Cursor = Cursors.Hand;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            opcion2.Width -= 10;
            opcion2.Height -= 10;
            opcion2.Top += 5;
            opcion2.Left += 5;
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            opcion3.Width += 10;
            opcion3.Height += 10;
            opcion3.Top -= 5;
            opcion3.Left -= 5;
            opcion3.Cursor = Cursors.Hand;
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            opcion3.Width -= 10;
            opcion3.Height -= 10;
            opcion3.Top += 5;
            opcion3.Left += 5;
        }

        private void label4_MouseEnter(object sender, EventArgs e)
        {
            opcion4.Width += 10;
            opcion4.Height += 10;
            opcion4.Top -= 5;
            opcion4.Left -= 5;
            opcion4.Cursor = Cursors.Hand;
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            opcion4.Width -= 10;
            opcion4.Height -= 10;
            opcion4.Top += 5;
            opcion4.Left += 5;
        }

        private async Task MostrarFeedback(bool esCorrecta)
        {
            //Crear pantalla fantasma
            Form filtro = new Form();
            filtro.FormBorderStyle = FormBorderStyle.None;
            filtro.StartPosition = FormStartPosition.Manual;

            //Se posiciona y se hace del tamano
            filtro.Location = this.PointToScreen(Point.Empty);
            filtro.Size = this.ClientSize;

            //Se elige el color:
            //Verde: correcta      Rojo: incorrecto
            filtro.BackColor = esCorrecta ? Color.LimeGreen : Color.Crimson;

            //Se da 50% de transparencia
            filtro.Opacity = 0.5;

            //Se muestra encima de la ventana
            filtro.Show(this);

            //Se pausa medio segundo el juego
            await Task.Delay(500);

            //Se cierra ventana
            filtro.Close();
            filtro.Dispose();
        }

        
    }
}