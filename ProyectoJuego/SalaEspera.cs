using System;
using System.Drawing;
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
            ConfigurarDiseño();
        }

        // ==========================================
        // CONFIGURACIÓN VISUAL
        // ==========================================
        private void ConfigurarDiseño()
        {
            this.ControlBox = false; // Adiós a la tachita para salir
            this.Text = "";
            this.BackColor = Color.FromArgb(142, 148, 255); // Color moradito de tu juego
            this.StartPosition = FormStartPosition.CenterScreen;

            lblEstado.BackColor = Color.Transparent;
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;

            try
            {
                lblEstado.Font = FontsManager.GetFipps(12);
                lstJugadores.Font = FontsManager.GetFipps(10);
            }
            catch { }

            lstJugadores.BorderStyle = BorderStyle.FixedSingle;
            lstJugadores.BackColor = Color.White;
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
                await wsCliente.ConnectAsync(serverUri, cancelToken.Token);
                _ = EscucharServidor();

                string miNombre = formPrincipal.NombreJugadorActual;
                byte[] bytesNombre = Encoding.UTF8.GetBytes(miNombre);
                await wsCliente.SendAsync(new ArraySegment<byte>(bytesNombre), WebSocketMessageType.Text, true, cancelToken.Token);
            }
            catch (Exception ex)
            {
                // REEMPLAZO DEL MESSAGE BOX DE ERROR
                // Mostramos el error en el label, esperamos 3 segundos y regresamos a la pantalla anterior
                lblEstado.Text = "Error de conexión.\nRegresando...";
                await Task.Delay(3000);
                this.Close();
            }
        }

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
                    lblEstado.Text = $"Esperando jugadores ({datos.jugadores.Count}/4)...";
                });
                return false;
            }
            else if (accion == "iniciar_juego")
            {
                // Usamos un Action asíncrono para poder hacer una pausa sin congelar la pantalla
                this.Invoke(new Action(async () =>
                {
                    formPrincipal.wsCliente = this.wsCliente;
                    formPrincipal.cancelToken = this.cancelToken;

                    // REEMPLAZO DEL MESSAGE BOX DE INICIO
                    // Escribimos el mensaje que manda Python ("¡Listos, comienza el Kahoot!") en la pantalla
                    lblEstado.Text = datos.mensaje.ToString();

                    // Hacemos una pausa de 1.5 segundos para que el jugador alcance a leerlo
                    await Task.Delay(1500);

                    // Cambiamos de pantalla fluidamente
                    Categorias ventana = new Categorias(formPrincipal);
                    ventana.Show();
                    this.Close();
                }));
                return true;
            }
            return false;
        }

        private async void SalaEspera_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (formPrincipal.wsCliente == null && wsCliente != null && wsCliente.State == WebSocketState.Open)
            {
                cancelToken.Cancel();
                await wsCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "Saliendo", CancellationToken.None);
            }
        }
    }
}