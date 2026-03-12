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
    public partial class Categorias : Form
    {
        Form1 formPrincipal;
        private Point mouseLoc;

        // 1. Tu cadena de conexión a Clever Cloud
        public string connectionString = "Server=bhuefshpv92bhb0wqb5n-mysql.services.clever-cloud.com;" +
                                        "Port=3306;" +
                                        "Database=bhuefshpv92bhb0wqb5n;" +
                                        "User ID=u7mcmeqwvuwiyurk;" +
                                        "Password=hwlYTA5OEtN6FXWbJowK;";

        public Categorias(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;

            // Llamamos al método al cargar el form
            CargarCategoriasEnBotones();
        }

        // 2. Método para jalar las categorías y ponerlas en los botones
        private void CargarCategoriasEnBotones()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT nombre, imagen FROM categorias ORDER BY id ASC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Suponiendo que tienes botones llamados btnCat1, btnCat2, btnCat3
                        // Usamos un contador para saber a qué botón asignarle el texto
                        int i = 1;
                        while (reader.Read())
                        {
                            string nombreCat = reader["nombre"].ToString();
                            string URLimagen = reader["imagen"].ToString();

                            if (i == 1)
                            {
                                btnAnimales.Text = nombreCat;
                                CargarImagenCategoria(URLimagen, picBoxAnimales); // Suponiendo que tienes uno para cada uno
                            }
                            if (i == 2)
                            {
                                btnVideojuegos.Text = nombreCat;
                                CargarImagenCategoria(URLimagen, picBoxJuegos);
                            }
                            if (i == 3)
                            {
                                btnPaises.Text = nombreCat;
                                CargarImagenCategoria(URLimagen, picBoxPaises);
                            }
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con la nube: " + ex.Message);
                }
            }
        }

        private void CargarImagenCategoria(string urlImagen, PictureBox pbDestino)
        {
            try
            {
                // Esto le dice al PictureBox que descargue la imagen de la URL de la base de datos
                pbDestino.LoadAsync(urlImagen);
                pbDestino.SizeMode = PictureBoxSizeMode.Zoom; // Para que no se deforme
            }
            catch (Exception ex)
            {
                // Si el link está roto o no hay internet, puedes poner una imagen por defecto
                Console.WriteLine("No se pudo cargar la imagen: " + ex.Message);
            }
        }

        // --- Tus métodos de movimiento de ventana y navegación ---

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            formPrincipal.Show();
            this.Close();
        }

        private void Categorias_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLoc = e.Location;
        }

        private void Categorias_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mouseLoc.X;
                int dy = e.Location.Y - mouseLoc.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }

        private void btnRegresar_Click(object sender, EventArgs e)
        {
            formPrincipal.Show();
            this.Close();
        }

        private void Categorias_Load(object sender, EventArgs e)
        {

        }
    }
}
