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

        private List<PreguntaJuego> listaPreguntas = new List<PreguntaJuego>();
        private int indiceActual = 0;
        private int idCategoriaSeleccionada;


        public Preguntas(int idCategoria, Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
            this.idCategoriaSeleccionada = idCategoria;

            btnOpcion1.Click += ValidarRespuesta_Click;
            btnOpcion2.Click += ValidarRespuesta_Click;
            btnOpcion3.Click += ValidarRespuesta_Click;
            btnOpcion4.Click += ValidarRespuesta_Click;

            // Conectamos también los PictureBoxes para cuando toquen imágenes
            picOpcion1.Click += ValidarRespuesta_Click;
            picOpcion2.Click += ValidarRespuesta_Click;
            picOpcion3.Click += ValidarRespuesta_Click;
            picOpcion4.Click += ValidarRespuesta_Click;

            CargarTodasLasPreguntasDesdeBD(); // Carga la lista
            MostrarPreguntaActual();
        }

        private void Preguntas_Load(object sender, EventArgs e)
        {

        }

        private void MostrarPreguntaActual()
        {
            // Verificar si ya terminamos el juego
            if (indiceActual >= listaPreguntas.Count)
            {
                MessageBox.Show("¡Categoría terminada!");
                formPrincipal.Show();
                this.Close();
                return;
            }

            // Obtener la pregunta que toca
            PreguntaJuego preguntaActual = listaPreguntas[indiceActual];
            pregunta.Text = preguntaActual.Enunciado;

            // Apagar paneles
            panelTexto.Visible = false;
            panelImagen.Visible = false;
            panelAudio.Visible = false;

            // Lógica para llenar los controles (similar a la que ya tienes)
            if (preguntaActual.Formato == "texto")
            {
                panelTexto.Visible = true;
                // Asignar textos. Asegúrate de guardar cuál es la correcta en el Tag
                btnOpcion1.Text = preguntaActual.Opciones[0].Contenido;
                btnOpcion1.Tag = preguntaActual.Opciones[0].EsCorrecta;
                btnOpcion2.Text = preguntaActual.Opciones[1].Contenido;
                btnOpcion2.Tag = preguntaActual.Opciones[1].EsCorrecta;
                btnOpcion3.Text = preguntaActual.Opciones[2].Contenido;
                btnOpcion3.Tag = preguntaActual.Opciones[2].EsCorrecta;
                btnOpcion4.Text = preguntaActual.Opciones[3].Contenido;
                btnOpcion4.Tag = preguntaActual.Opciones[3].EsCorrecta;
            }
            else if (preguntaActual.Formato == "imagen")
            {
                panelImagen.Visible = true;
                CargarImagen(preguntaActual.Opciones[0].Contenido, picOpcion1);
                picOpcion1.Tag = preguntaActual.Opciones[0].EsCorrecta;
                CargarImagen(preguntaActual.Opciones[1].Contenido, picOpcion2);
                picOpcion2.Tag = preguntaActual.Opciones[1].EsCorrecta;
                CargarImagen(preguntaActual.Opciones[2].Contenido, picOpcion3);
                picOpcion3.Tag = preguntaActual.Opciones[2].EsCorrecta;
                CargarImagen(preguntaActual.Opciones[3].Contenido, picOpcion4);
                picOpcion4.Tag = preguntaActual.Opciones[3].EsCorrecta;
            }
            // ... igual para audio
            else if(preguntaActual.Formato == "audio")
            {

            }
        }


        private void Preguntas_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            formPrincipal.Show();
            Close();//funcionara diferente :p
        }

        // Solo agregas la palabra 'async' aquí
        private async void CargarImagen(string url, PictureBox picBox)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return;

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    // Cambias a DownloadDataTaskAsync y le pones 'await'
                    byte[] imageData = await webClient.DownloadDataTaskAsync(url);

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageData))
                    {
                        picBox.Image = Image.FromStream(ms);
                        picBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
            }
        }

        private void ValidarRespuesta_Click(object sender, EventArgs e)
        {
            Control controlClickeado = (Control)sender; // Puede ser un Button o PictureBox

            // Verificamos si la respuesta es correcta leyendo el Tag que le asignamos
            if (controlClickeado.Tag != null && (bool)controlClickeado.Tag == true)
            {
                MessageBox.Show("¡Respuesta Correcta!");
                // Aquí puedes sumar puntos a una variable global de puntuación
            }
            else
            {
                MessageBox.Show("Respuesta Incorrecta");
            }

            // Avanzamos a la siguiente pregunta
            indiceActual++;
            MostrarPreguntaActual();
        }


        private void CargarTodasLasPreguntasDesdeBD()
        {
            listaPreguntas.Clear(); // Limpiamos la lista por si se vuelve a llamar

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Usamos un JOIN para traer la pregunta y sus opciones juntas.
                    string query = @"
                SELECT 
                    p.id AS PreguntaId, 
                    p.enunciado, 
                    p.formato,
                    o.contenido, 
                    o.es_correcta
                FROM preguntas p
                INNER JOIN opciones o ON p.id = o.pregunta_id
                WHERE p.categoria_id = @categoriaId
                ORDER BY p.id"; // El orden es vital para agrupar en el while

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Usar parámetros (@categoriaId) previene ataques de inyección SQL. 
                    // Es una práctica de seguridad fundamental.
                    cmd.Parameters.AddWithValue("@categoriaId", this.idCategoriaSeleccionada);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        PreguntaJuego preguntaActual = null;

                        while (reader.Read())
                        {
                            int idPreguntaBD = Convert.ToInt32(reader["PreguntaId"]);

                            // Si preguntaActual es nula o el ID cambió, estamos leyendo una pregunta nueva
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

                            // En cada vuelta del while, leemos una opción y se la agregamos a la pregunta actual
                            OpcionJuego nuevaOpcion = new OpcionJuego
                            {
                                Contenido = reader["contenido"].ToString(),
                                //Convert.ToBoolean maneja bien el tinyint(1) de MySQL
                                EsCorrecta = Convert.ToBoolean(reader["es_correcta"])
                            };

                            preguntaActual.Opciones.Add(nuevaOpcion);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con la base de datos: " + ex.Message);
                }
            }
        }

        private void Preguntas_MouseMove_1(object sender, MouseEventArgs e)
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
