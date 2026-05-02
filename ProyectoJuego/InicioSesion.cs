using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http; // Para peticiones web
using Newtonsoft.Json;
using System.Drawing; // Para procesar JSON

namespace ProyectoJuego
{
    public partial class InicioSesion : Form
    {
        private Form1 formPrincipal;

        // Estas variables se las pasaremos al Form1

        private Point mouseLoc;

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

            //txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.TextAlign = HorizontalAlignment.Center;

            // --- ESTILO Y TAMAÑO DEL BOTÓN JUGAR ---
            btnJugar.Text = "JUGAR"; 
            btnJugar.Font = FontsManager.GetFipps(12);
            btnJugar.Size = new Size(180, 65);
            btnJugar.Location = new Point((this.ClientSize.Width - btnJugar.Width) / 2, txtUsername.Bottom + 25);

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

            // ¡AQUÍ ESTÁ LA MAGIA! Cambiamos el 65 por 85 para subir el texto y separarlo de la caja
            int yPos = txtUsername.Location.Y - 85;
            Rectangle rect = new Rectangle(0, yPos, this.ClientSize.Width, 60);

            using (Brush bBorde = new SolidBrush(Color.Black))
            using (Brush bTexto = new SolidBrush(Color.White))
            {
                // Sombras/Contorno
                for (int x = -3; x <= 3; x += 3)
                    for (int y = -3; y <= 3; y += 3)
                        if (x != 0 || y != 0) e.Graphics.DrawString(texto, fuente, bBorde, new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height), formato);

                // Texto principal
                e.Graphics.DrawString(texto, fuente, bTexto, rect, formato);
            }

            // --- DIBUJAR EL BORDE REDONDEADO DEL TEXTBOX ---
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
            // Código para arrastrar la ventana sin bordes
            if (e.Button == MouseButtons.Left)
                this.Location = new Point(this.Location.X + (e.X - mouseLoc.X), this.Location.Y + (e.Y - mouseLoc.Y));
        }

        #endregion
        // --------------------------

        #region Base de Datos y API

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
        #endregion
        /*private void btnCerrar_Click(object sender, EventArgs e)
        {
            btnCerrar.Top -= 4;
            formPrincipal.Show();
            this.Close();
        }*/
    }
}