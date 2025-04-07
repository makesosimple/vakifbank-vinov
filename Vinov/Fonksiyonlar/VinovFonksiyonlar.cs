using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using sd;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vinov.Models;

namespace Vinov
{
    public class VinovFonksiyonlar
    {
        public static IConfiguration Ayarlar = default!;

        public static JsonResult InvalidModel(ModelStateDictionary ModelState)
        {
            return new JsonResult(new { Succeded = false, ErrorMessage = string.Join(",", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()) });
        }

        public static bool CaptchaDogrula(string CaptchaId, string CaptchaText)
        {
            string RequestBody = @$"
            {{
            ""CaptchaId"": ""{CaptchaId}"",
            ""CaptchaText"": ""{CaptchaText}""
            }}
            ";

            VakifBankApi vakifBankApi = new();
            ApiSonuc<string> ApiCevap = vakifBankApi.ApiIstek<string>(HttpMethod.Post, "/captcha", RequestBody);

            CaptchaDogrulaCevap captchaDogrulaCevap = JsonSerializer.Deserialize<CaptchaDogrulaCevap>(ApiCevap.Result);

            return captchaDogrulaCevap.Data.Result;
        }

        public static string SondakiVirguluSil(string jsonString)
        {
            return Regex.Replace(jsonString, @",\s*([\]}])", "$1");
        }

    }
}
