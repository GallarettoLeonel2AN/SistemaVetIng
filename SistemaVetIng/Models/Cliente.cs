﻿namespace SistemaVetIng.Models
{
    public class Cliente : Persona
    {

        public string Direccion { get; set; }
        public List<Mascota> Mascotas { get; set; }
        public List<Turno> Turnos { get; set; }
    }
}
