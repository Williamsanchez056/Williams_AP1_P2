using System.ComponentModel.DataAnnotations;

namespace Williams_AP1_P2.Models;

public class Productos
{
    [Key]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Este campo es obligatorio")]
    [StringLength(100, ErrorMessage = "No puede superar los 100 caracteres")]
    public string Concepto { get; set; } = string.Empty;

    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Precio { get; set; }

    [Range(0, 999999, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; } = 10;
}