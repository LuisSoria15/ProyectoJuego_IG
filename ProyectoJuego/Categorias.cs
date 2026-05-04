using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace ProyectoJuego
{
    public partial class Categorias : Form
    {
        Form1 formPrincipal;
        private Form ventanaEspera;
        private Point mouseLoc;

        public Categorias(Form1 formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;
            this.Size = formPrincipal.Size;

            lblTitulo.Font = FontsManager.GetFipps(20); 
            lblTitulo.AutoSize = false;
            lblTitulo.Width = this.ClientSize.Width;
            lblTitulo.Height = 80;
            lblTitulo.ForeColor = Color.Transparent;
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.Location = new Point(0, 20);

            lblTitulo.Paint += new PaintEventHandler(lblTitulo_Paint);


            this.Opacity = 0.0;

            Timer timerAparicion = new Timer();
            timerAparicion.Interval = 15;
            timerAparicion.Tick += TimerAparicion_Tick;
            timerAparicion.Start();

            // Llamamos al método al cargar el form
            //CargarCategoriasEnBotones();
        }

        private async void Categorias_Load(object sender, EventArgs e)
        {
            await CargarCategoriasEnBotones();
            _ = EscucharServidor();
        }

        private async Task EscucharServidor()
        {
            byte[] buffer = new byte[2048];
            try
            {
                while (formPrincipal.wsCliente.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var result = await formPrincipal.wsCliente.ReceiveAsync(new ArraySegment<byte>(buffer), formPrincipal.cancelToken.Token);

                    // ¡Faltaba esta protección vital!
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await formPrincipal.wsCliente.CloseAsync(WebSocketCloseStatus.NormalClosure, "", formPrincipal.cancelToken.Token);
                        break;
                    }
                    else
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        dynamic datos = JsonConvert.DeserializeObject(json);

                        // Como datos es dinámico, verificamos que no sea nulo
                        if (datos != null && datos.accion == "resultado_votacion")
                        {
                            int catGanadora = datos.categoria_ganadora;

                            this.Invoke((MethodInvoker)delegate
                            {
                                // 1. Cerramos el mensajito de "Esperando..."
                                if (ventanaEspera != null) ventanaEspera.Close();

                                MessageBox.Show($"¡El sistema ha elegido la categoría {catGanadora}!");

                                // 2. Ahora sí, ¡a las preguntas!
                                Preguntas ventana = new Preguntas(catGanadora, formPrincipal);
                                ventana.Show();
                                this.Close();
                            });
                            break; // Dejamos de escuchar
                        }
                    }
                }
            }
            catch { }
        }

        private void MostrarMensajeEspera()
        {
            ventanaEspera = new Form();
            ventanaEspera.FormBorderStyle = FormBorderStyle.None;
            ventanaEspera.StartPosition = FormStartPosition.CenterScreen;
            ventanaEspera.Size = new System.Drawing.Size(400, 100);
            ventanaEspera.BackColor = Color.DarkSlateBlue;

            Label lbl = new Label();
            lbl.Text = "Esperando a que el otro jugador elija...";
            lbl.ForeColor = Color.White;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Arial", 12, FontStyle.Bold);

            ventanaEspera.Controls.Add(lbl);
            ventanaEspera.Show(this);
        }

        // 2. Método para jalar las categorías y ponerlas en los botones
        private async Task CargarCategoriasEnBotones()
        {
            string urlApi = "http://10.36.144.135:11000/categorias";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage respuesta = await client.GetAsync(urlApi);

                    respuesta.EnsureSuccessStatusCode();

                    string jsonString = await respuesta.Content.ReadAsStringAsync();
                    //MessageBox.Show("Esto me mandó Python:\n\n" + jsonString);
                    List<CategoriaAPI> listaCategorias = JsonConvert.DeserializeObject<List<CategoriaAPI>>(jsonString);

                    // 1. Metemos tus controles en el orden exacto de los IDs de la base de datos
                    PictureBox[] misPictureBoxes = { picBoxAnimales, picBoxJuegos, picBoxPeliculas, picBoxCanciones, picBoxPaises, picBoxSeries, picBoxMarcas };
                    Label[] misLabels = { nomCateg1, nomCateg2, nomCateg3, nomCateg4, nomCateg5, nomCateg6, nomCateg7 };

                    // 2. Recorremos la lista usando un simple for
                    // (Usamos Math.Min para que el programa no falle si la base de datos devuelve más o menos de 7 categorías)
                    int limite = Math.Min(listaCategorias.Count, misPictureBoxes.Length);

                    for (int i = 0; i < limite; i++)
                    {
                        var cat = listaCategorias[i]; // Sacamos la categoría actual

                        misPictureBoxes[i].Text = cat.nombre;

                        try
                        {
                            misLabels[i].Text = cat.nombre.ToUpperInvariant();
                            misLabels[i].Font = FontsManager.GetFipps(7);
                            misLabels[i].BackColor = Color.Transparent;
                            misLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                            misLabels[i].BringToFront();
                        }
                        catch { }

                        // Enviamos la imagen y el PictureBox correspondiente
                        CargarImagenCategoria(cat.IMAGEN, misPictureBoxes[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el servidor del juego: " + ex.Message);
            }
        }


        private void CargarImagenCategoria(string nombreImagen, PictureBox pbDestino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreImagen)) return;

                nombreImagen = nombreImagen.Trim();

                //Armamos la ruta
                string rutaAbsoluta = Path.Combine(Application.StartupPath, "Recursos", "Imagenes", nombreImagen);

                // Verificamos si existe físicamente
                if (File.Exists(rutaAbsoluta))
                {
                    using (FileStream fs = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read))
                    {
                        pbDestino.Image = Image.FromStream(fs);
                        pbDestino.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
                else
                {
                    
                    MessageBox.Show("No encuentro la imagen. Busqué exactamente en:\n\n" + rutaAbsoluta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar cargar: " + ex.Message);
            }
        }

        // --- Métodos de movimiento de ventana y navegación ---

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

       
        
        private void lblTitulo_Paint(object sender, PaintEventArgs e)
        {
            string texto = "Categorias";
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            
            Brush brochaBorde = new SolidBrush(Color.Black);
            Brush brochaColor = new SolidBrush(Color.White);

            SizeF tamanoTexto = e.Graphics.MeasureString(texto, lblTitulo.Font);
            float x = (lblTitulo.Width - tamanoTexto.Width) / 2;
            float y = 0;

            e.Graphics.DrawString(texto, lblTitulo.Font, brochaBorde, x - 3, y);
            e.Graphics.DrawString(texto, lblTitulo.Font, brochaBorde, x + 3, y);
            e.Graphics.DrawString(texto, lblTitulo.Font, brochaBorde, x, y - 3);
            e.Graphics.DrawString(texto, lblTitulo.Font, brochaBorde, x, y + 3);

            e.Graphics.DrawString(texto, lblTitulo.Font, brochaBorde, x + 5, y + 5);

            e.Graphics.DrawString(texto, lblTitulo.Font, brochaColor, x, y);
        }

        private void pbCerrar_Click(object sender, EventArgs e)
        {
            pbCerrar.Top -= 4;
            formPrincipal.Show();
            this.Close();
        }

        private void pbCerrar_MouseUp(object sender, MouseEventArgs e)
        {
            pbCerrar.Top -= 4;
        }

        private void pbCerrar_MouseDown(object sender, MouseEventArgs e)
        {
            pbCerrar.Top += 4;
        }

        private void pbCerrar_MouseEnter(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_resplandor;
            pbCerrar.Cursor = Cursors.Hand;
        }

        private void pbCerrar_MouseLeave(object sender, EventArgs e)
        {
            pbCerrar.Image = Properties.Resources.cerrar_normal;
        }

        private void pbRegresar_Click(object sender, EventArgs e)
        {
            pbRegresar.Top -= 4;
            formPrincipal.Show();
            this.Close();
        }

        private void pbRegresar_MouseUp(object sender, MouseEventArgs e)
        {
            pbRegresar.Top -= 4;
        }

        private void pbRegresar_MouseDown(object sender, MouseEventArgs e)
        {
            pbRegresar.Top += 4;
        }

        private void pbRegresar_MouseEnter(object sender, EventArgs e)
        {
            pbRegresar.Image = Properties.Resources.flecha_resplandor;
            pbRegresar.Cursor = Cursors.Hand;
        }

        private void pbRegresar_MouseLeave(object sender, EventArgs e)
        {
            pbRegresar.Image = Properties.Resources.flecha_normal;
        }

        private async void picBoxAnimales_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 1;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }
        
        private void picBoxAnimales_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxAnimales.Top -= 4;
        }
        private void picBoxAnimales_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxAnimales.Top += 4;
        }
        private void picBoxAnimales_MouseEnter(object sender, EventArgs e)
        {
            picBoxAnimales.Width += 10;
            picBoxAnimales.Height += 10;
            picBoxAnimales.Top -= 5;
            picBoxAnimales.Left -= 5;
            picBoxAnimales.Cursor = Cursors.Hand;
        }

        private void picBoxAnimales_MouseLeave(object sender, EventArgs e)
        {
            picBoxAnimales.Width -= 10;
            picBoxAnimales.Height -= 10;
            picBoxAnimales.Top += 5;
            picBoxAnimales.Left += 5;
        }

        private async void picBoxJuegos_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 2;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }
        private void picBoxJuegos_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxJuegos.Top -= 4;
        }

        private void picBoxJuegos_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxJuegos.Top += 4;
        }

        private void picBoxJuegos_MouseEnter(object sender, EventArgs e)
        {
            picBoxJuegos.Width += 10;
            picBoxJuegos.Height += 10;
            picBoxJuegos.Top -= 5;
            picBoxJuegos.Left -= 5;
            picBoxJuegos.Cursor = Cursors.Hand;
        }

        private void picBoxJuegos_MouseLeave(object sender, EventArgs e)
        {
            picBoxJuegos.Width -= 10;
            picBoxJuegos.Height -= 10;
            picBoxJuegos.Top += 5;
            picBoxJuegos.Left += 5;
        }

        private async void picBoxPaises_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 5;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }

        private void picBoxPaises_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxPaises.Top -= 4;
        }

        private void picBoxPaises_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxPaises.Top += 4;
        }

        private void picBoxPaises_MouseEnter(object sender, EventArgs e)
        {
            picBoxPaises.Width += 10;
            picBoxPaises.Height += 10;
            picBoxPaises.Top -= 5;
            picBoxPaises.Left -= 5;
            picBoxPaises.Cursor = Cursors.Hand;
        }

        private void picBoxPaises_MouseLeave(object sender, EventArgs e)
        {
            picBoxPaises.Width -= 10;
            picBoxPaises.Height -= 10;
            picBoxPaises.Top += 5;
            picBoxPaises.Left += 5;
        }

        private async void picBoxPeliculas_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 3;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }

        private void picBoxPeliculas_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxPeliculas.Top -= 4;
        }

        private void picBoxPeliculas_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxPeliculas.Top += 4;
        }

        private void picBoxPeliculas_MouseEnter(object sender, EventArgs e)
        {
            picBoxPeliculas.Width += 10;
            picBoxPeliculas.Height += 10;
            picBoxPeliculas.Top -= 5;
            picBoxPeliculas.Left -= 5;
            picBoxPeliculas.Cursor = Cursors.Hand;
        }

        private void picBoxPeliculas_MouseLeave(object sender, EventArgs e)
        {
            picBoxPeliculas.Width -= 10;
            picBoxPeliculas.Height -= 10;
            picBoxPeliculas.Top += 5;
            picBoxPeliculas.Left += 5;
        }

        private async void picBoxSeries_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 6;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }

        private void picBoxSeries_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxSeries.Top -= 4;
        }

        private void picBoxSeries_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxSeries.Width += 4;
        }

        private void picBoxSeries_MouseEnter(object sender, EventArgs e)
        {
            picBoxSeries.Width += 10;
            picBoxSeries.Height += 10;
            picBoxSeries.Top -= 5;
            picBoxSeries.Left -= 5;
            picBoxSeries.Cursor = Cursors.Hand;
        }

        private void picBoxSeries_MouseLeave(object sender, EventArgs e)
        {
            picBoxSeries.Width -= 10;
            picBoxSeries.Height -= 10;
            picBoxSeries.Top += 5;
            picBoxSeries.Left += 5;
        }

        private async void picBoxCanciones_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 4;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }

        private void picBoxCanciones_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxCanciones.Top -= 4;
        }

        private void picBoxCanciones_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxCanciones.Top += 4;
        }

        private void picBoxCanciones_MouseEnter(object sender, EventArgs e)
        {
            picBoxCanciones.Width += 10;
            picBoxCanciones.Height += 10;
            picBoxCanciones.Top -= 5;
            picBoxCanciones.Left -= 5;
            picBoxCanciones.Cursor = Cursors.Hand;
        }

        private void picBoxCanciones_MouseLeave(object sender, EventArgs e)
        {
            picBoxCanciones.Width -= 10;
            picBoxCanciones.Height -= 10;
            picBoxCanciones.Top += 5;
            picBoxCanciones.Left += 5;
        }

        private async void picBoxMarcas_ClickAsync(object sender, EventArgs e)
        {
            this.Enabled = false;

            // 2. (¡Importante!) Asigna aquí el número real de la categoría a la que le dio clic
            int idCategoriaElegida = 7;

            // 3. Mandamos el voto a Python en JSON
            var voto = new { accion = "votar", id_categoria = idCategoriaElegida };
            string jsonVoto = JsonConvert.SerializeObject(voto);
            byte[] bytesVoto = Encoding.UTF8.GetBytes(jsonVoto);
            await formPrincipal.wsCliente.SendAsync(new ArraySegment<byte>(bytesVoto), System.Net.WebSockets.WebSocketMessageType.Text, true, formPrincipal.cancelToken.Token);

            // 4. Mostramos el mensaje flotante
            MostrarMensajeEspera();
        }

        private void picBoxMarcas_MouseUp(object sender, MouseEventArgs e)
        {
            picBoxMarcas.Top -= 4;
        }

        private void picBoxMarcas_MouseDown(object sender, MouseEventArgs e)
        {
            picBoxMarcas.Top += 4; 
        }

        private void picBoxMarcas_MouseEnter(object sender, EventArgs e)
        {
            picBoxMarcas.Width += 10;
            picBoxMarcas.Height += 10;
            picBoxMarcas.Top -= 5;
            picBoxMarcas.Left -= 5;
            picBoxMarcas.Cursor = Cursors.Hand;
        }

        private void picBoxMarcas_MouseLeave(object sender, EventArgs e)
        {
            picBoxMarcas.Width -= 10;
            picBoxMarcas.Height -= 10;
            picBoxMarcas.Top += 5;
            picBoxMarcas.Left += 5;
        }

        private void TimerAparicion_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1.0)
            {
                this.Opacity += 0.05;
            }
            else
            {
                Timer timer = (Timer)sender;
                timer.Stop();
                timer.Dispose();
            }
        }

    }
}
