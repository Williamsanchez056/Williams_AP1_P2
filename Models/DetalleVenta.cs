using System.ComponentModel.DataAnnotations;

namespace Williams_AP1_P2.Models;

public class DetalleVenta
{
    [Key]
    public int DetalleId { get; set; }

    public int VentaId { get; set; }

    public int ProductoId { get; set; }

    [Range(1, 999999, ErrorMessage = "La cantidad debe ser mayor que cero")]
    public int Cantidad { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Precio { get; set; }

    public Ventas? Venta { get; set; }

    public Productos? Producto { get; set; }
}