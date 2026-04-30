using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ProyectoJuego
{
    public partial class SalaEspera : Form
    {
        private Form1 formPrincipal;
        private ClientWebSocket wsCliente;
        private CancellationTokenSource cancelToken;

        public SalaEspera(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
        }

        private async void SalaEspera_LoadAsync(object sender, EventArgs e)
        {

            lblEstado.Text = "Conectando al servidor...";
            await ConectarASala();

        }

        private async Task ConectarASala()
        {
            wsCliente = new ClientWebSocket();
            cancelToken = new CancellationTokenSource();

            Uri serverUri = new Uri("ws://10.17.217.135:11000/ws/sala");

            try
            {
                // 1. Abrimos el túnel
                await wsCliente.ConnectAsync(serverUri, cancelToken.Token);

                // 2. ¡EL TRUCO! Primero nos ponemos a escuchar en segundo plano
                _ = EscucharServidor();

                // 3. Y AHORA SÍ, le gritamos nuestro nombre al servidor
                string miNombre = formPrincipal.NombreJugadorActual;
                byte[] bytesNombre = Encoding.UTF8.GetBytes(miNombre);
                await wsCliente.SendAsync(new ArraySegment<byte>(bytesNombre), WebSocketMessageType.Text, true, cancelToken.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando a la sala: " + ex.Message);
                this.Close(); // Si falla, nos salimos
            }
        }

        // Este método vive en segundo plano escuchando todo lo que mande Python
        private async Task EscucharServidor()
        {
            byte[] buffer = new byte[2048]; // Espacio para recibir los mensajes

            try
            {
                while (wsCliente.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await wsCliente.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await wsCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancelToken.Token);
                    }
                    else
                    {
                        // Traducimos los bytes recibidos a texto JSON
                        string mensajeJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcesarMensaje(mensajeJson);
                    }
                }
            }
            catch (Exception)
            {
                // Aquí cae si se corta la conexión de golpe
            }
        }

        private void ProcesarMensaje(string json)
        {
            // Como Python manda un diccionario dinámico, usamos "dynamic" para leerlo súper fácil
            dynamic datos = JsonConvert.DeserializeObject(json);
            string accion = datos.accion;

            // EL SECRETO: Como este método lo llama un proceso de fondo, 
            // C# no nos deja tocar la interfaz gráfica directamente. 
            // Tenemos que usar "this.Invoke" para pedirle permiso a la ventana principal.
            this.Invoke((MethodInvoker)delegate
            {
                if (accion == "actualizar_sala")
                {
                    // Limpiamos la lista y la rellenamos con los nombres nuevos
                    lstJugadores.Items.Clear();
                    foreach (var jugador in datos.jugadores)
                    {
                        lstJugadores.Items.Add(jugador.ToString());
                    }
                    lblEstado.Text = $"Esperando jugadores ({datos.jugadores.Count}/4)...";
                }
                else if (accion == "iniciar_juego")
                {
                    // ¡Llegaron los 4!
                    MessageBox.Show(datos.mensaje.ToString());

                    // Pasamos a las categorías y cerramos esta ventana
                    Categorias ventana = new Categorias(formPrincipal);
                    ventana.Show();
                    this.Close();
                }
            });
        }

        private async void SalaEspera_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si cerramos la ventana a la fuerza, avisamos al servidor para que cierre el túnel limpio
            if (wsCliente != null && wsCliente.State == WebSocketState.Open)
            {
                cancelToken.Cancel();
                await wsCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "Saliendo", CancellationToken.None);
            }
        }

        
    }
}