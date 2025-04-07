using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vinov
{
    public class SchoolApplicationModel
    {
        public int Il { get; set; }
        public int Ilce { get; set; }
        [Display(Name = "Okul Adı")]
        [Required(ErrorMessage = "Bu alanın doldurulması zorunludur.")]    
        public string? School { get; set; }

        [Display(Name = "TC Kimlik No")]
        [Required(ErrorMessage = "Bu alanın doldurulması zorunludur.")]
        [TCKimlikNo(ErrorMessage = "Geçerli bir TC Kimlik No giriniz.")]
        [RegularExpression("^[1-9]{1}[0-9]{10}$", ErrorMessage = "Geçerli bir TC Kimlik No giriniz.")]
        public string Tckn { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Bu alanın doldurulması zorunludur.")]
        [RegularExpression("^5[0-9]{9}$", ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(10)]
        public string? Telefon { get; set; }
        public string? Captcha { get; set; }

        //[DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Doğum Tarihi")]
        [Required(ErrorMessage = "Bu alanın doldurulması zorunludur.")]
        public string DogumTarihi { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Sözleşmeyi Kabul Ediyorum")]
        [Range(typeof(bool), "false", "true", ErrorMessage = "Lütfen bilgilendirme metnini okuyup, onaylayınız.")]
        public bool Kvkk { get; set; }
    }
}