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

            Uri serverUri = new Uri($"ws://{Form1.IP_SERVIDOR}:{Form1.PUERTO}/ws/sala");

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
            byte[] buffer = new byte[2048];
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
                        string mensajeJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        bool soltarTunel = ProcesarMensaje(mensajeJson);

                        // Si el juego ya inició, rompemos este ciclo para que Categorias use el WebSocket
                        if (soltarTunel) break;
                    }
                }
            }
            catch (Exception) { }
        }

        private bool ProcesarMensaje(string json)
        {
            dynamic datos = JsonConvert.DeserializeObject(json);
            string accion = datos.accion;

            if (accion == "actualizar_sala")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lstJugadores.Items.Clear();
                    foreach (var jugador in datos.jugadores)
                    {
                        lstJugadores.Items.Add(jugador.ToString());
                    }
                    lblEstado.Text = $"Esperando jugadores ({datos.jugadores.Count}/2)...";
                });
                return false;
            }
            else if (accion == "iniciar_juego")
            {
                this.Invoke((MethodInvoker)delegate
                {
                    // 1. Guardamos el túnel en Form1 para no perderlo
                    formPrincipal.wsCliente = this.wsCliente;
                    formPrincipal.cancelToken = this.cancelToken;

                    MessageBox.Show(datos.mensaje.ToString());

                    Categorias ventana = new Categorias(formPrincipal);
                    ventana.Show();
                    this.Close();
                });
                return true; // Le avisa al While que se detenga
            }
            return false;
        }

        // Modifica tu evento FormClosing para que no corte el internet al pasar de ventana:
        private async void SalaEspera_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Solo cerramos internet si el jugador le dio a la X roja
            if (formPrincipal.wsCliente == null && wsCliente != null && wsCliente.State == WebSocketState.Open)
            {
                cancelToken.Cancel();
                await wsCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "Saliendo", CancellationToken.None);
            }
        }


       


    }
}