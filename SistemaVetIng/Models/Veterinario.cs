namespace SistemaVetIng.Models
{
    public class Veterinario : Persona
    {

        public string Matricula { get; set; }
        public string Direccion { get; set; }

        public int VeterinariaId { get; set; }
        public Veterinaria Veterinaria { get; set; }

    }
}
