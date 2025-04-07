namespace Vinov.Models
{
    public class Header
    {
        public string StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ObjectID { get; set; }
    }

    public class District
    {
        public string DistrictName { get; set; }
        public string NVIDistrictCode { get; set; }
        public string BankDistrictCode { get; set; }
        public string DistrictCode { get; set; }
    }

    public class Data
    {
        public IList<District> District { get; set; }
        public string CaptchaImage { get; set; }
        public string CaptchaId { get; set; }
        public Schoollist[] SchoolList { get; set; }
        public Result Result { get; set; }
        public int StatusCode { get; set; }
    }


    public class CaptchaDogrulaData
    {
        public string CaptchaImage { get; set; }
        public string CaptchaId { get; set; }
        public int StatusCode { get; set; }
        public bool Result { get; set; }
    }


    public class HataCevap
    {
        public Header Header { get; set; }
    }

    public class IlceCevap
    {
        public Header Header { get; set; }
        public Data Data { get; set; }
    }


    public class TokenCevap
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }


    public class CaptchaCevap
    {
        public Header Header { get; set; }
        public Data Data { get; set; }
    }

    public class CaptchaDogrulaCevap
    {
        public Header Header { get; set; }
        public CaptchaDogrulaData Data { get; set; }
    }

    public class SchoolCevap
    {
        public Header Header { get; set; }
        public Data Data { get; set; }
    }

    public class Schoollist
    {
        public string WorkPlaceSignName { get; set; }
        public string WorkPlaceId { get; set; }
    }


    public class BasvuruCevap
    {
        public Header Header { get; set; }
        public Data Data { get; set; }
    }

    public class Result
    {
        public int RejectReasonCode { get; set; }
        public bool IsSucceeded { get; set; }
        public string RejectReasonDescription { get; set; }
        public int StatusCode { get; set; }
    }

}
