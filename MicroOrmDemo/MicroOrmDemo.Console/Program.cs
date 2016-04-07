using Dapper;
using MicroOrmDemo.Console.Entidades;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace MicroOrmDemo.Console
{
    class Program
    {
        private const string Cadena = "Initial Catalog=NORTHWND;Data Source=TAVOPC;Integrated Security=SSPI;";

        static void Main(string[] args)
        {
            /////////// Consulta a una tabla

            // Dapper
            ConsultarOrdenesDapper();

            // EF
            ConsultarOrdenesEf();

            // EF
            ConsultarOrdenesEfAsNoTracking();

            System.Console.WriteLine();

            /////////// consulta con dos tablas con relación (Eaguer load)

            //Dapper
            ConsultarOrdenesDetallesDapper();

            // EF
            ConsultarOrdenesDetallesEf();

            // EF
            ConsultarOrdenesDetallesEfAsNoTracking();

            // EF
            ConsultarOrdenesDetallesEfRawSQL();

            System.Console.WriteLine();

            /////////// Inserción simple.

            // Dapper
            InsertarOrdenDapper();

            // EF
            InsertarOrdenEf();

            /////////// Inserción de tablas relacionadas

            // Dapper
            InsertarOrdenDetalleDapper();

            // EF
            InsertarOrdenDetalleEf();

            System.Console.ReadLine();
        }

        #region Métodos Dapper

        private static void ConsultarOrdenesDapper()
        {
            using (IDbConnection db = new SqlConnection(Cadena))
            {
                var consulta = @"SELECT [OrderID] AS Id, [CustomerID] AS IdCliente, [EmployeeID] AS IdEmpledo,
                    [OrderDate] AS Fecha, [RequiredDate] AS FechaRequerida, [ShipCountry] AS Pais, [ShipCity] AS Ciudad
                    FROM [NORTHWND].[dbo].[Orders]";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Se ejecuta la consulta.
                var ordenes = db.Query<Orden>(consulta);

                stopwatch.Stop();
                System.Console.WriteLine("Dapper.Net: {0} registros obtenidos en {1} milisegundos.", ordenes.Count(),
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private static void ConsultarOrdenesDetallesDapper()
        {
            var consulta = @"SELECT a.[OrderID] AS Id
	                    ,a.[CustomerID] AS IdCliente
	                    ,a.[OrderDate] AS Fecha
                          ,b.[ProductID] AS IdProducto
                          ,b.[UnitPrice] AS Precio
                          ,b.[Quantity] AS Cantidad
                          ,b.[Discount] AS Descuento
                      FROM [NORTHWND].[dbo].[Order Details] b
                      INNER JOIN [NORTHWND].[dbo].[Orders] a
                      ON a.OrderID = b.OrderID;";

            using (IDbConnection db = new SqlConnection(Cadena))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Se ejecuta la consulta.
                var ordenesDetalle = db.Query<DetalleOrden>(consulta);

                stopwatch.Stop();
                System.Console.WriteLine("Dapper.Net: {0} registros obtenidos en {1} milisegundos.",
                    ordenesDetalle.Count(), stopwatch.ElapsedMilliseconds);
            }
        }

        private static void InsertarOrdenDapper()
        {
            var consulta =
                $@"INSERT INTO [NORTHWND].[dbo].[Orders] (CustomerID, OrderDate) 
                    VALUES ('VINET', '{DateTime.Now.ToString("yyyy-MM-dd")}');";
            using (IDbConnection db = new SqlConnection(Cadena))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Se ejecuta la instrucción.
                db.Execute(new CommandDefinition(consulta));

                stopwatch.Stop();
                System.Console.WriteLine("Dapper.Net: un registro insertado en {0} milisegundos.",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private static void InsertarOrdenDetalleDapper()
        {
            var consulta =
                $@"INSERT INTO [NORTHWND].[dbo].[Orders] (CustomerID, OrderDate) 
                VALUES ('VINET', '{DateTime.Now.ToString("yyyy-MM-dd")}') select SCOPE_IDENTITY();";
            using (IDbConnection db = new SqlConnection(Cadena))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Se almacena la orden y se obtiene su Id Identity.
                var idOrden = (int)db.Query<decimal>(consulta).First();

                consulta = $@"INSERT INTO [NORTHWND].[dbo].[Order Details] 
                VALUES ({idOrden}, 41, 20, 100, 0);";
                db.Execute(new CommandDefinition(consulta));

                stopwatch.Stop();
                System.Console.WriteLine("Dapper.Net: registros relacionados insertados en {0} milisegundos.",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        #endregion

        #region Métodos EF

        private static void ConsultarOrdenesEf()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ordenes = contexto.Orders.ToList();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework: {0} registros obtenidos en {1} milisegundos.", ordenes.Count,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private static void ConsultarOrdenesEfAsNoTracking()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ordenes = contexto.Orders.AsNoTracking().ToList();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework AsNoTracking: {0} registros obtenidos en {1} milisegundos.", ordenes.Count,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private static void ConsultarOrdenesDetallesEf()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ordenesDetalle = contexto.Orders.Include("Order_Details").ToList();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework: {0} registros obtenidos en {1} milisegundos.",
                    ordenesDetalle.Count, stopwatch.ElapsedMilliseconds);
            }
        }

        private static void ConsultarOrdenesDetallesEfRawSQL()
        {
            var consulta = @"SELECT a.[OrderID] AS Id
	                    ,a.[CustomerID] AS IdCliente
	                    ,a.[OrderDate] AS Fecha
                          ,b.[ProductID] AS IdProducto
                          ,CAST(b.[UnitPrice] AS DECIMAL(18,2)) AS Precio
                          ,CAST(b.[Quantity] AS INT) AS Cantidad
                          ,CAST(b.[Discount] AS FLOAT) AS Descuento
                      FROM [NORTHWND].[dbo].[Order Details] b
                      INNER JOIN [NORTHWND].[dbo].[Orders] a
                      ON a.OrderID = b.OrderID;";

            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ordenesDetalle = contexto.Database.SqlQuery<DetalleOrden>(consulta).ToList();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework RawSQL: {0} registros obtenidos en {1} milisegundos.",
                    ordenesDetalle.Count, stopwatch.ElapsedMilliseconds);
            }
        }

        private static void ConsultarOrdenesDetallesEfAsNoTracking()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var ordenesDetalle = contexto.Orders.Include("Order_Details").AsNoTracking().ToList();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework AsNoTracking: {0} registros obtenidos en {1} milisegundos.",
                    ordenesDetalle.Count, stopwatch.ElapsedMilliseconds);
            }
        }

        private static void InsertarOrdenEf()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var orden = new Order
                {
                    CustomerID = "VINET",
                    OrderDate = DateTime.Now
                };

                contexto.Orders.Add(orden);
                contexto.SaveChanges();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework: un registro insertado en {0} milisegundos.",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private static void InsertarOrdenDetalleEf()
        {
            using (var contexto = new NorthwndModel())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var orden = new Order
                {
                    CustomerID = "VINET",
                    OrderDate = DateTime.Now,
                    Order_Details =
                    {
                        new Order_Detail
                        {
                            ProductID = 41,
                            UnitPrice = 20,
                            Quantity = 100,
                            Discount = 0
                        }
                    }
                };

                contexto.Orders.Add(orden);
                contexto.SaveChanges();

                stopwatch.Stop();
                System.Console.WriteLine("EntityFramework: registros relacionados insertados en {0} milisegundos.",
                    stopwatch.ElapsedMilliseconds);
            }
        }

        #endregion
    }
}
