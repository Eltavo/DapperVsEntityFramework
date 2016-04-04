namespace MicroOrmDemo.Console.Entidades
{
    class DetalleOrden : Orden
    {
        public int IdProducto { get; set; }

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        public double Descuento { get; set; }
    }
}
