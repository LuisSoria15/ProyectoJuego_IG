using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Drawing;

namespace ProyectoJuego
{
    public partial class InicioSesion : Form
    {
        private Form1 formPrincipal;
        private Point mouseLoc;

        // Declaramos nuestro nuevo Label para los mensajes
        private Label lblMensaje;

        public InicioSesion(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;

            this.Size = new Size(500, 280);

            //quitar bordes
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            ConfigurarDiseno();
        }

        #region Configuracion Visual y Dibujo

        private void ConfigurarDiseno()
        {
            // --- ESTILO Y TAMAÑO DEL TEXTBOX ---
            txtUsername.Font = FontsManager.GetFipps(12);
            txtUsername.Width = 320;
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Location = new Point((this.ClientSize.Width - txtUsername.Width) / 2, 115);
            txtUsername.TextAlign = HorizontalAlignment.Center;

            // --- CONFIGURACIÓN DEL LABEL DE MENSAJES ---
            lblMensaje = new Label();
            lblMensaje.AutoSize = false;
            lblMensaje.Width = this.ClientSize.Width;
            lblMensaje.Height = 25;
            // Lo ponemos un poco más abajo de la caja de texto
            lblMensaje.Location = new Point(0, txtUsername.Bottom + 15);
            lblMensaje.TextAlign = ContentAlignment.MiddleCenter;
            lblMensaje.BackColor = Color.Transparent; // Para que no tape el fondo morado
            try
            {
                lblMensaje.Font = FontsManager.GetFipps(8); // Letra más pequeña para mensajes
            }
            catch
            {
                lblMensaje.Font = new Font("Arial", 10, FontStyle.Bold);
            }
            lblMensaje.Text = ""; // Inicia vacío
            this.Controls.Add(lblMensaje); // Lo agregamos a la ventana

            // --- ESTILO Y TAMAÑO DEL BOTÓN JUGAR ---
            btnJugar.Text = "JUGAR";
            btnJugar.Font = FontsManager.GetFipps(12);
            btnJugar.Size = new Size(180, 65);
            // Empujamos el botón hacia abajo para hacerle espacio al Label
            btnJugar.Location = new Point((this.ClientSize.Width - btnJugar.Width) / 2, lblMensaje.Bottom + 5);

            btnJugar.FlatStyle = FlatStyle.Flat;
            btnJugar.FlatAppearance.BorderSize = 4;
            btnJugar.FlatAppearance.BorderColor = Color.Black;
            btnJugar.BackColor = Color.Gold;
            btnJugar.Cursor = Cursors.Hand;

            // --- ACOMODAR LA TACHA (X) ---
            pbCerrar.Location = new Point(this.ClientSize.Width - pbCerrar.Width - -10, 5);

            // Eventos de diseño
            this.Paint += InicioSesion_Paint;
            this.MouseDown += InicioSesion_MouseDown;
            this.MouseMove += InicioSesion_MouseMove;

            // Eventos del boton cerrar
            pbCerrar.MouseEnter += pbCerrar_MouseEnter;
            pbCerrar.MouseLeave += pbCerrar_MouseLeave;
            pbCerrar.Click += pbCerrar_Click;
        }

        private void InicioSesion_Paint(object sender, PaintEventArgs e)
        {
            string texto = "NUEVO JUGADOR";
            Font fuente = FontsManager.GetFipps(16);

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            StringFormat formato = new StringFormat { Alignment = StringAlignment.Center };

            int yPos = txtUsername.Location.Y - 85;
            Rectangle rect = new Rectangle(0, yPos, this.ClientSize.Width, 60);

            using (Brush bBorde = new SolidBrush(Color.Black))
            using (Brush bTexto = new SolidBrush(Color.White))
            {
                for (int x = -3; x <= 3; x += 3)
                    for (int y = -3; y <= 3; y += 3)
                        if (x != 0 || y != 0) e.Graphics.DrawString(texto, fuente, bBorde, new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height), formato);

                e.Graphics.DrawString(texto, fuente, bTexto, rect, formato);
            }

            int radio = 20;
            Rectangle cajaFondo = new Rectangle(txtUsername.Left - 10, txtUsername.Top - 10, txtUsername.Width + 20, txtUsername.Height + 20);

            System.Drawing.Drawing2D.GraphicsPath ruta = new System.Drawing.Drawing2D.GraphicsPath();
            ruta.AddArc(cajaFondo.X, cajaFondo.Y, radio, radio, 180, 90);
            ruta.AddArc(cajaFondo.Right - radio, cajaFondo.Y, radio, radio, 270, 90);
            ruta.AddArc(cajaFondo.Right - radio, cajaFondo.Bottom - radio, radio, radio, 0, 90);
            ruta.AddArc(cajaFondo.X, cajaFondo.Bottom - radio, radio, radio, 90, 90);
            ruta.CloseFigure();

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            e.Graphics.FillPath(Brushes.White, ruta);
            e.Graphics.DrawPath(new Pen(Color.Black, 4), ruta);
        }

        #endregion

        #region Eventos de Ventana y boton cerrar

        private void pbCerrar_MouseEnter(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_resplandor;
            pbCerrar.Cursor = Cursors.Hand;
        }

        private void pbCerrar_MouseLeave(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_normal;
        }

        private void pbCerrar_Click(object sender, EventArgs e)
        {
            formPrincipal.Show();
            this.Close();
        }

        private void InicioSesion_MouseDown(object sender, MouseEventArgs e) => mouseLoc = e.Location;

        private void InicioSesion_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = new Point(this.Location.X + (e.X - mouseLoc.X), this.Location.Y + (e.Y - mouseLoc.Y));
        }

        #endregion

        #region Base de Datos y API

        // Método auxiliar para pintar mensajes fácilmente
        private void MostrarMensaje(string mensaje, Color color)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.ForeColor = color;
        }

        private async void btnJugar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = ""; // Limpiamos cualquier mensaje anterior
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                // REEMPLAZO 1: Validación de campo vacío
                MostrarMensaje("INGRESA UN NOMBRE", Color.Red);
                return;
            }

            btnJugar.Enabled = false;
            MostrarMensaje("CONECTANDO...", Color.White); // Le avisamos al jugador que está cargando

            string urlApi = $"http://{Form1.IP_SERVIDOR}:{Form1.PUERTO}/registro";

            PeticionRegistro solicitud = new PeticionRegistro { username = username };
            string jsonEnvio = JsonConvert.SerializeObject(solicitud);
            StringContent contenido = new StringContent(jsonEnvio, Encoding.UTF8, "application/json");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage respuesta = await client.PostAsync(urlApi, contenido);
                    respuesta.EnsureSuccessStatusCode();

                    string jsonRespuesta = await respuesta.Content.ReadAsStringAsync();
                    RespuestaRegistro resultado = JsonConvert.DeserializeObject<RespuestaRegistro>(jsonRespuesta);

                    if (resultado.existe)
                    {
                        // REEMPLAZO 2: El usuario ya existe
                        MostrarMensaje("EL USUARIO YA EXISTE", Color.Red);
                        txtUsername.Text = "";
                    }
                    else
                    {
                        // REEMPLAZO 3: Registro exitoso
                        MostrarMensaje("¡REGISTRADO!", Color.LimeGreen);

                        formPrincipal.IdJugadorActual = resultado.id_usuario;
                        formPrincipal.NombreJugadorActual = username;

                        // Esperamos 1 segundo para que el jugador alcance a leer "¡REGISTRADO!"
                        await Task.Delay(1000);

                        SalaEspera ventanaSala = new SalaEspera(formPrincipal);
                        ventanaSala.Show();
                        this.Close();
                        formPrincipal.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ""; // Ocultamos el mensaje si sale el MessageBox de error crítico
                MessageBox.Show("Error al conectar con la API: " + ex.Message);
            }
            finally
            {
                btnJugar.Enabled = true;
            }
        }
        #endregion
    }
}