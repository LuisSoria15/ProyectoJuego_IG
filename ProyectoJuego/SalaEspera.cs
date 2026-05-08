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
            this.ControlBox = false; 
            this.Text = "";
            this.BackColor = Color.FromArgb(142, 148, 255); // Color moradito del juego

            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblEstado.BackColor = Color.Transparent;
            lblEstado.AutoSize = false; // Lo desactivamos para centrar el texto bien
            lblEstado.Size = new Size(this.ClientSize.Width, 60);
            lblEstado.Location = new Point(0, 70); // Lo ponemos arribita
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;

            // --- ESTIRAMOS LA CAJA BLANCA ---
            lstJugadores.Size = new Size(400, 200); // 200 de alto da espacio para los 4 jugadores
            // Centramos la caja usando matemáticas
            lstJugadores.Location = new Point((this.ClientSize.Width - lstJugadores.Width) / 2, 140);
            lstJugadores.BorderStyle = BorderStyle.FixedSingle;
            lstJugadores.BackColor = Color.White;

            try
            {
                lblEstado.Font = FontsManager.GetFipps(12);
                lstJugadores.Font = FontsManager.GetFipps(10);
            }
            catch { }
        }

        private async void SalaEspera_LoadAsync(object sender, EventArgs e)
        {
            lblEstado.Text = "Conectando a la sala...";

            // Avisamos al servidor que entramos
            var peticion = new { accion = "entrar_sala", nombre = formPrincipal.NombreJugadorActual };
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            _ = EscucharServidor();
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
            try
            {
                while (Form1.clienteTCP.Connected)
                {
                    // ReadLineAsync detiene el programa aquí hasta que Python nos mande algo
                    string jsonLlegada = await Form1.lectorTCP.ReadLineAsync();
                    if (jsonLlegada == null) break; // Si es null, el servidor se apagó

                    dynamic datos = JsonConvert.DeserializeObject(jsonLlegada);
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
                            // OJO: Aquí le pones 2 o 3 dependiendo de cuántos jugadores configuraron
                            lblEstado.Text = $"Esperando jugadores ({datos.jugadores.Count}/2)...";
                        });
                    }
                    else if (accion == "iniciar_juego")
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show(datos.mensaje.ToString());
                            Categorias ventana = new Categorias(formPrincipal);
                            ventana.Show();
                            this.Close();
                        });
                        break; // Dejamos de escuchar en esta ventana para que escuche la siguiente
                    }
                }
            }
            catch { }
        }

        
    }
}