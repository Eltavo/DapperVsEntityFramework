using System;

namespace MicroOrmDemo.Console.Entidades
{
    public class Orden
    {
        public int Id { get; set; }

        public string IdCliente { get; set; }

        public int IdEmpledo { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime FechaRequerida { get; set; }

        public string Pais { get; set; }

        public string Ciudad { get; set; }
    }
}
