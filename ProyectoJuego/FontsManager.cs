using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;            


namespace ProyectoJuego
{
    internal class FontsManager
    {
        private static PrivateFontCollection pfc = new PrivateFontCollection();

        static FontsManager()
        {
            string fontPath = Path.Combine(Application.StartupPath, "Fonts", "Fipps-Regular.otf");

            if (File.Exists(fontPath))
            {
                pfc.AddFontFile(fontPath);

                if (pfc.Families.Length > 0)
                {
                    Console.WriteLine("Fuente cargada: " + pfc.Families[0].Name);
                }
            }
            else
            {
                MessageBox.Show("¡Archivo no encontrado en: " + fontPath);
            }

        }

        public static Font GetFipps(float size)
        {
            if (pfc.Families.Length > 0)
            {
                return new Font(pfc.Families[0], size);
            }
            return new Font("Arial", size);
        }
    }
}
