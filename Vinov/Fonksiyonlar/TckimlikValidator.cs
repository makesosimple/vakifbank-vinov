using System.ComponentModel.DataAnnotations;


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public class TCKimlikNoAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("T.C. Kimlik Numarası No 11 haneli olmalıdır.");
        }

        string tckimlikNo = value.ToString();

        if (!UzunlukGecerliMi(tckimlikNo))
        {
            return new ValidationResult("T.C. Kimlik Numarası 11 haneli olmalıdır.");
        }

        if ( !RakamlarRakamMı(tckimlikNo))
        {
            return new ValidationResult("T.C. Kimlik Numarası sadece rakamlardan oluşmalıdır.");
        }

        if (!GecerliTCKimlikNoMu(tckimlikNo))
        {
            return new ValidationResult("Geçerli bir T.C. Kimlik Numarası giriniz.");
        }

        return ValidationResult.Success;
    }

    private bool UzunlukGecerliMi(string tckimlikNo)
    {
        return tckimlikNo.Length == 11;
    }

    private bool RakamlarRakamMı(string tckimlikNo)
    {
        return long.TryParse(tckimlikNo, out _);
    }

    private bool GecerliTCKimlikNoMu(string tckimlikNo)
    {
        int[] rakamlar = tckimlikNo.Select(c => int.Parse(c.ToString())).ToArray();
        int ciftlerToplami = rakamlar[0] + rakamlar[2] + rakamlar[4] + rakamlar[6] + rakamlar[8];
        int teklerToplami = rakamlar[1] + rakamlar[3] + rakamlar[5] + rakamlar[7];

        int onuncuHane = (ciftlerToplami * 7 - teklerToplami) % 10;
        int onbirinciHane = (ciftlerToplami + teklerToplami + rakamlar[9]) % 10;

        return rakamlar[9] == onuncuHane && rakamlar[10] == onbirinciHane;
    }

}
