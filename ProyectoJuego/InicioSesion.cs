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
    public partial class InicioSesion : Form
    {
        private Form1 formPrincipal;
        public string connectionString = "Server=bhuefshpv92bhb0wqb5n-mysql.services.clever-cloud.com;Port=3306;Database=bhuefshpv92bhb0wqb5n;User ID=u7mcmeqwvuwiyurk;Password=hwlYTA5OEtN6FXWbJowK;";

        // Estas variables se las pasaremos al Form1
        public int IdUsuarioRegistrado { get; private set; }
        public string NombreUsuarioRegistrado { get; private set; }

        public InicioSesion(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
        }

        // Evento click de tu botón de Jugar dentro de esta ventanita
        private void btnJugar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario.");
                return;
            } 

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Buscar si el usuario ya existe
                    string queryBuscar = "SELECT id FROM usuarios WHERE username = @user";
                    MySqlCommand cmdBuscar = new MySqlCommand(queryBuscar, conn);
                    cmdBuscar.Parameters.AddWithValue("@user", username);

                    object resultado = cmdBuscar.ExecuteScalar(); // Trae solo la primera columna (id)

                    if (resultado != null)
                    {
                        // ¡El usuario ya existe! Guardamos su ID.
                        //IdUsuarioRegistrado = Convert.ToInt32(resultado);
                        MessageBox.Show($"El usuario \"{username}\" ya existe, ingresa otro nombre!");
                        txtUsername.Text = "";
                    }
                    else
                    {
                        // 2. No existe. Lo insertamos y recuperamos su nuevo ID.
                        // LAST_INSERT_ID() es una función nativa de MySQL súper útil aquí
                        string queryInsertar = "INSERT INTO usuarios (username) VALUES (@user); SELECT LAST_INSERT_ID();";
                        MySqlCommand cmdInsertar = new MySqlCommand(queryInsertar, conn);
                        cmdInsertar.Parameters.AddWithValue("@user", username);

                        // Ejecutamos el insert y recuperamos el ID generado
                        IdUsuarioRegistrado = Convert.ToInt32(cmdInsertar.ExecuteScalar());
                        NombreUsuarioRegistrado = username;
                        MessageBox.Show($"¡Bienvenido a QuizTown, {username}!");
                        // 2. Si el usuario ingresó su nombre y le dio a "Jugar" (DialogResult.OK)
                      
                        // 3. Ahora sí, pasamos a las categorías
                        Categorias ventana = new Categorias(formPrincipal);
                        ventana.Show();
                        this.Close();
                        formPrincipal.Hide();
                    }

                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con la base de datos: " + ex.Message);
                }
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            btnCerrar.Top -= 4;
            formPrincipal.Show();
            this.Close();
        }
    }
}
