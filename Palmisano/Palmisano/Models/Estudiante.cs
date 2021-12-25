using System;
using System.Collections.Generic;
using System.Text;

namespace Palmisano 
{
    public class Estudiante
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public int Año { get; set; }    
        public Direccion Direccion { get; set; }
        public int? CarreraId { get; set; }
        public Carrera Carrera { get; set; }    
        public List<EstudianteMateria> EstudianteMateria { get; set; }
        public string NombreFoto { get; set; }

        public string Localidad { get; set; }
        public string Calle { get; set; }

    }
}
