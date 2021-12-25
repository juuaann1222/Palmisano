using System;
using System.Collections.Generic;
using System.Text;

namespace Palmisano
{
    public class EstudianteMateria
    {
        public int EstudianteId { get; set; }
        public int MateriaId { get; set; }
        public int AñoCursado { get; set; }
        public Estudiante Estudiante { get; set; }
        public Materia Materia { get; set; }
        public DateTime FechaInscripcion { get; set; }


        
    }
}
