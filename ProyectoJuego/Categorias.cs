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
            this.Size = new Size(formPrincipal.Width, formPrincipal.Height + 120);
            this.StartPosition = FormStartPosition.CenterScreen;

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

            AcomodarCuadricula();
        }

        private async void Categorias_Load(object sender, EventArgs e)
        {
            await CargarCategoriasDesdeServidor();


        }

        // Esta función atrapa la decisión del servidor
        private async Task EscucharVotacion()
        {
            try
            {
                while (Form1.clienteTCP.Connected)
                {
                    string jsonLlegada = await Form1.lectorTCP.ReadLineAsync();
                    if (jsonLlegada == null) break;

                    dynamic datos = JsonConvert.DeserializeObject(jsonLlegada);

                    if (datos.accion == "resultado_votacion")
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            int categoriaGanadora = datos.categoria_ganadora;
                            MessageBox.Show($"¡Todos votaron! La categoría elegida es la {categoriaGanadora}");

                            // Pasamos a la ventana de Preguntas con la categoría ganadora
                            Preguntas ventana = new Preguntas(categoriaGanadora, formPrincipal);
                            ventana.Show();
                            this.Close();
                        });
                        break; // Dejamos de escuchar
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error en la votación: " + ex.Message); }
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
        private async Task CargarCategoriasDesdeServidor()
        {
            try
            {
                // 1. Pedir categorías a Python por el túnel TCP
                var peticion = new { accion = "obtener_categorias" };
                await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
                await Form1.escritorTCP.FlushAsync();

                // 2. Leer la respuesta
                string jsonRespuesta = await Form1.lectorTCP.ReadLineAsync();
                if (jsonRespuesta == null) return;

                dynamic resultado = JsonConvert.DeserializeObject(jsonRespuesta);

                if (resultado.accion == "respuesta_categorias" && resultado.estatus == "exito")
                {
                    // 3. Modificamos la pantalla usando Invoke
                    this.Invoke((MethodInvoker)delegate
                    {
                        var listaCategorias = resultado.datos;

                        // Aquí asignamos los textos de la BD a tus labels visuales
                        // (Ajusta esto si tus labels tienen otros nombres)
                        if (listaCategorias.Count > 0) nomCateg1.Text = listaCategorias[0].nombre.ToString();
                        if (listaCategorias.Count > 1) nomCateg2.Text = listaCategorias[1].nombre.ToString();
                        if (listaCategorias.Count > 2) nomCateg3.Text = listaCategorias[2].nombre.ToString();
                        if (listaCategorias.Count > 3) nomCateg4.Text = listaCategorias[3].nombre.ToString();
                        if (listaCategorias.Count > 4) nomCateg5.Text = listaCategorias[4].nombre.ToString();
                        if (listaCategorias.Count > 5) nomCateg6.Text = listaCategorias[5].nombre.ToString();
                    });
                }
                else
                {
                    MessageBox.Show("Hubo un problema al cargar las categorías desde la BD.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el servidor TCP: " + ex.Message);
            }
        }


        private void CargarImagenCategoria(string nombreImagen, PictureBox pbDestino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreImagen)) return;

                nombreImagen = nombreImagen.Trim();

                // Armamos la ruta
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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 1 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();
            // Mostramos el mensaje flotante
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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 2 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();
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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 5 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();

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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 3 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();
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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 6 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();
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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 1 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();

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

            // 1. Mandamos el voto por el socket
            var peticion = new { accion = "votar", id_categoria = 7 }; // Cambia el 1 por el ID que le toque a cada PictureBox
            await Form1.escritorTCP.WriteLineAsync(JsonConvert.SerializeObject(peticion));
            await Form1.escritorTCP.FlushAsync();

            // 2. Aquí podrías mostrar un letrerito de "Esperando a los demás..."
            // MostrarMensajeEspera(); // (Si tienes una función que muestre un form morado temporal)

            // 3. Nos ponemos a escuchar quién ganó
            _ = EscucharVotacion();
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
        // ==========================================
        // ACOMODO PERFECTO DE LA CUADRÍCULA
        // ==========================================
        private void AcomodarCuadricula()
        {
            // 1. Calculamos el centro de la pantalla
            int centroX = this.ClientSize.Width / 2;

            // 2. Definimos distancias
            int ancho = picBoxAnimales.Width; // Tomamos el ancho de una imagen como base
            int espacioX = 40; // Espacio de separación horizontal

            // Alturas (Y) de cada fila 
            int fila1Y = 130;
            int fila2Y = 280;
            int fila3Y = 430;

            // --- FILA 1 (3 opciones) ---
            picBoxJuegos.Location = new Point(centroX - (ancho / 2), fila1Y);
            picBoxAnimales.Location = new Point(picBoxJuegos.Left - espacioX - ancho, fila1Y);
            picBoxPaises.Location = new Point(picBoxJuegos.Right + espacioX, fila1Y);

            // --- FILA 2 (2 opciones) ---
            picBoxPeliculas.Location = new Point(centroX - ancho - (espacioX / 2), fila2Y);
            picBoxSeries.Location = new Point(centroX + (espacioX / 2), fila2Y);

            // --- FILA 3 (2 opciones) ---
            picBoxCanciones.Location = new Point(centroX - ancho - (espacioX / 2), fila3Y);
            picBoxMarcas.Location = new Point(centroX + (espacioX / 2), fila3Y);

            // --- CENTRAR LOS TEXTOS PERFECTAMENTE SOBRE SU IMAGEN ---
            CentrarLabel(nomCateg1, picBoxAnimales);
            CentrarLabel(nomCateg2, picBoxJuegos);
            CentrarLabel(nomCateg5, picBoxPaises);

            CentrarLabel(nomCateg3, picBoxPeliculas);
            CentrarLabel(nomCateg6, picBoxSeries);

            CentrarLabel(nomCateg4, picBoxCanciones);
            CentrarLabel(nomCateg7, picBoxMarcas);
        }

        private void CentrarLabel(Label lbl, PictureBox pic)
        {
            lbl.AutoSize = false;
            lbl.Width = pic.Width + 40; // Le damos margen para textos largos
            lbl.Height = 25;
            // Lo ponemos justo al centro y 25 píxeles arriba de la foto
            lbl.Location = new Point(pic.Left - 20, pic.Top - 25);
            lbl.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
