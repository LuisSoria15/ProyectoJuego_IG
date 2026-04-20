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
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace ProyectoJuego
{
    public partial class Categorias : Form
    {
        Form1 formPrincipal;
        private Point mouseLoc;



        // 1. Tu cadena de conexión a Clever Cloud
        public string connectionString = "Server=127.0.0.1;" +
                                        "Port=3306;" +
                                        "Database=preguntaslocaldisco;" +
                                        "User ID=root;" +
                                        "Password=Cucusoria1515!;";

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
            CargarCategoriasEnBotones();
        }

        // 2. Método para jalar las categorías y ponerlas en los botones
        private void CargarCategoriasEnBotones()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT nombre, imagen FROM categorias ORDER BY id ASC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                   
                        // Usamos un contador para saber a qué botón asignarle el texto
                        int i = 1;
                        while (reader.Read())
                        {
                            string nombreCat = reader["nombre"].ToString();
                            string URLimagen = reader["imagen"].ToString();

                            if (i == 1)
                            {
                                picBoxAnimales.Text = nombreCat;
                                // etiqueta que muestra el nombre de la categoría
                                try
                                {
                                    nomCateg1.Text = nombreCat.ToUpperInvariant();
                                    nomCateg1.Font = FontsManager.GetFipps(7);
                                   // nomCateg1.ForeColor = Color.White;
                                    nomCateg1.BackColor = Color.Transparent;
                                    nomCateg1.TextAlign = ContentAlignment.MiddleCenter;
                                    nomCateg1.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxAnimales);
                            }
                            if (i == 2)
                            {
                                picBoxJuegos.Text = nombreCat;
                                try
                                {
                                    nomCateg2.Text = nombreCat.ToUpperInvariant();
                                    nomCateg2.Font = FontsManager.GetFipps(7);
                                    //nomCateg2.ForeColor = Color.White;
                                    nomCateg2.BackColor = Color.Transparent;
                                    nomCateg2.TextAlign = ContentAlignment.MiddleCenter;
                                    nomCateg2.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxJuegos);
                            }
                            if (i == 3)
                            {
                                picBoxPeliculas.Text = nombreCat;
                                try
                                {
                                    nomCateg3.Text = nombreCat.ToUpperInvariant();
                                    nomCateg3.Font = FontsManager.GetFipps(7);
                                    //nomCateg3.ForeColor = Color.White;
                                    nomCateg3.BackColor = Color.Transparent;
                                    nomCateg3.TextAlign = ContentAlignment.MiddleCenter;
                                    
                                    nomCateg3.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxPeliculas);
                            }
                            if (i == 4)
                            {
                                picBoxCanciones.Text = nombreCat;
                                try
                                {
                                    nomCateg4.Text = nombreCat.ToUpperInvariant();
                                    nomCateg4.Font = FontsManager.GetFipps(7);
                                   // nomCateg4.ForeColor = Color.White;
                                    nomCateg4.BackColor = Color.Transparent;
                                    nomCateg4.TextAlign = ContentAlignment.MiddleCenter;
                                    
                                    nomCateg4.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxCanciones);
                            }
                            if (i == 5)
                            {
                                picBoxPaises.Text = nombreCat;
                                try
                                {
                                    nomCateg5.Text = nombreCat.ToUpperInvariant();
                                    nomCateg5.Font = FontsManager.GetFipps(7);
                                   // nomCateg5.ForeColor = Color.White;
                                    nomCateg5.BackColor = Color.Transparent;
                                    nomCateg5.TextAlign = ContentAlignment.MiddleCenter;
                                    
                                    nomCateg5.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxPaises);
                            }
                            if (i == 6)
                            {
                                picBoxSeries.Text = nombreCat;
                                try
                                {
                                    nomCateg6.Text = nombreCat.ToUpperInvariant();
                                    nomCateg6.Font = FontsManager.GetFipps(7);
                                    //nomCateg7.ForeColor = Color.White;
                                    nomCateg6.BackColor = Color.Transparent;
                                    nomCateg6.TextAlign = ContentAlignment.MiddleCenter;
                                    
                                    nomCateg6.BringToFront();
                                } catch { }
                                CargarImagenCategoria(URLimagen, picBoxSeries);
                            }
                            if (i == 7)
                            {
                                picBoxMarcas.Text = nombreCat;
                                try
                                {
                                    nomCateg7.Text = nombreCat.ToUpperInvariant();
                                    nomCateg7.Font = FontsManager.GetFipps(7);
                                    //nomCateg7.ForeColor = Color.White;
                                    nomCateg7.BackColor = Color.Transparent;
                                    nomCateg7.TextAlign = ContentAlignment.MiddleCenter;

                                    nomCateg7.BringToFront();
                                }
                                catch { }
                                CargarImagenCategoria(URLimagen, picBoxMarcas);
                            }
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al conectar con la nube: " + ex.Message);
                }
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

        // --- Tus métodos de movimiento de ventana y navegación ---

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

        private void Categorias_Load(object sender, EventArgs e)
        {

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

        private void picBoxAnimales_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(1, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxJuegos_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(2, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxPaises_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(5, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxPeliculas_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(3, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxSeries_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(6, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxCanciones_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(4, formPrincipal);
            formPreguntas.Show();
            this.Close();
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

        private void picBoxMarcas_Click(object sender, EventArgs e)
        {
            Preguntas formPreguntas = new Preguntas(7, formPrincipal);
            formPreguntas.Show();
            this.Close();
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
