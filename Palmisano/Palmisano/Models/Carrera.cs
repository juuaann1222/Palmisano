﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Palmisano
{
    public class Carrera
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }

        public List<Estudiante> Estudiantes { get; set; }
    }
}
