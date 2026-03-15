using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoJuego
{
    public class OpcionJuego
    {
        public string Contenido { get; set; }
        public bool EsCorrecta { get; set; }
    }

    public class PreguntaJuego
    {
        public int Id { get; set; }
        public string Enunciado { get; set; }
        public string Formato { get; set; }
        public List<OpcionJuego> Opciones { get; set; } = new List<OpcionJuego>();
    }
}
