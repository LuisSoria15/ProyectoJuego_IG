using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoJuego
{
    internal class APIrequest
    {

    }


    
public class OpcionJuego
    {
        public int id { get; set; }
        public int pregunta_id { get; set; }
        public string formato { get; set; }
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

    public class CategoriaAPI
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string IMAGEN { get; set; }
    }
    // --- CLASES PARA LA API ---
    public class PeticionRegistro
    {
        public string username { get; set; }
    }

    public class RespuestaRegistro
    {
        public bool existe { get; set; }
        public int id_usuario { get; set; }
        public string mensaje { get; set; }

    }
    public class UsuarioPuntaje
    {
        public int id_usuario { get; set; }
        public int puntaje { get; set; }
        public int id_categoria { get; set; }
    }

    public class EnvioPuntaje
    {
        public string estatus { get; set; }
        public string mensaje { get; set; }

    }

}
