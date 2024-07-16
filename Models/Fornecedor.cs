using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Fornecedor
{
    public int ID { get; set; }

    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O campo CNPJ é obrigatório.")]
    [RegularExpression(@"\d{14}", ErrorMessage = "CNPJ inválido")]
    public string CNPJ { get; set; }

    [Required(ErrorMessage = "O campo Segmento é obrigatório.")]
    public string Segmento { get; set; }

    [Required(ErrorMessage = "O campo CEP é obrigatório.")]
    [RegularExpression(@"\d{8}", ErrorMessage = "CEP inválido")]
    public string CEP { get; set; }

    [Required(ErrorMessage = "O campo Endereço é obrigatório.")]
    public string Endereco { get; set; }
    [NotMapped]
    [DataType(DataType.Upload)]
    public IFormFile? FotoArquivo { get; set; } // Permite que seja nulo usando ?
    public string Foto { get; set; } = string.Empty; // Inicialização para evitar nulo
}
