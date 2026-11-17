using System.ComponentModel.DataAnnotations;

namespace Williams_AP1_P2.Models;

public class Ventas
{
    [Key]
    public int VentaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [StringLength(500, ErrorMessage = "No puede superar los 500 caracteres")]
    public string Observacion { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();
}