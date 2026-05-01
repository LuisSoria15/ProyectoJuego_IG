using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http; // Para peticiones web
using Newtonsoft.Json; // Para procesar JSON

namespace ProyectoJuego
{
    public partial class InicioSesion : Form
    {
        private Form1 formPrincipal;

        // Estas variables se las pasaremos al Form1
        

        public InicioSesion(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
        }

        
        
        // --------------------------

        // Hacemos el botón async
        private async void btnJugar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario.");
                return;
            }

            // Desactivamos el botón mientras se conecta para evitar doble clic
            btnJugar.Enabled = false;

            // IP del hotspot y la nueva ruta
            string urlApi = "http://10.17.217.135:11000/registro";

            // Empaquetamos la petición
            PeticionRegistro solicitud = new PeticionRegistro { username = username };
            string jsonEnvio = JsonConvert.SerializeObject(solicitud);
            StringContent contenido = new StringContent(jsonEnvio, Encoding.UTF8, "application/json");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Hacemos la consulta al servidor
                    HttpResponseMessage respuesta = await client.PostAsync(urlApi, contenido);
                    respuesta.EnsureSuccessStatusCode();

                    // Leemos y decodificamos el JSON
                    string jsonRespuesta = await respuesta.Content.ReadAsStringAsync();
                    //MessageBox.Show("Esto me mandó Python:\n\n" + jsonRespuesta);
                    RespuestaRegistro resultado = JsonConvert.DeserializeObject<RespuestaRegistro>(jsonRespuesta);

                    if (resultado.existe)
                    {
                        // El servidor detectó que ya está registrado
                        MessageBox.Show($"El usuario \"{username}\" ya existe, ingresa otro nombre!");
                        txtUsername.Text = "";
                    }
                    else
                    {
                        // ¡Registro exitoso! Guardamos las variables
                        formPrincipal.IdJugadorActual = resultado.id_usuario;
                        formPrincipal.NombreJugadorActual = username;

                        MessageBox.Show(resultado.mensaje);

                        // Pasamos a la SALA DE ESPERA
                        SalaEspera ventanaSala = new SalaEspera(formPrincipal);
                        ventanaSala.Show();
                        this.Close();
                        formPrincipal.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la API: " + ex.Message);
            }
            finally
            {
                // Volvemos a activar el botón pase lo que pase
                btnJugar.Enabled = true;
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