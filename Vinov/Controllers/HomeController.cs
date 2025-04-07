using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using sd;
using System;
using System.Diagnostics;
using System.Globalization;
using Vinov.Models;

namespace Vinov.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            VakifBankApi vakifBankApi = new();

            ApiSonuc<SchoolCevap> OkulListele = vakifBankApi.ApiIstek<SchoolCevap>(HttpMethod.Post, "/vinov/schoolList", "");

            if (OkulListele.Succeded)
            {
                Schoollist[] schoollist = OkulListele.Result.Data.SchoolList;
                schoollist = schoollist.OrderBy(x => x.WorkPlaceSignName).ToArray();
                ViewBag.SchoolList = schoollist.Select(x => new SelectListItem() { Text = x.WorkPlaceSignName, Value = x.WorkPlaceId }).ToList();
            }
            else
            {
                throw new Exception($"Okullar listelenirken bir hata oluştu. <br/> {OkulListele.ErrorMessage}");
            }

            return View();
        }

        public IActionResult SchoolApplication()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SchoolApplication(SchoolApplicationModel schoolApplication)
        {
            //captcha doğrula
            VakifBankApi vakifBankApi = new();

            string CaptchaBody = @$"
            {{
            ""CaptchaId"": ""{HttpContext.Session.GetString("CaptchaId")}"",
            ""CaptchaText"": ""{schoolApplication.Captcha}""
            }}
            ";

            ApiSonuc<CaptchaDogrulaCevap> CaptchaDogrula = vakifBankApi.ApiIstek<CaptchaDogrulaCevap>(HttpMethod.Post, "/captcha", CaptchaBody);

            if (!CaptchaDogrula.Succeded) return Json(new { Succeded = false, ErrorMessage = CaptchaDogrula.ErrorMessage });

            if (!CaptchaDogrula.Result.Data.Result) return Json(new { Succeded = false, ErrorMessage = "Güvenlik kodu doğrulanamadı, kodu yenileyerek tekrar deneyiniz." });

            // date doğrula
            DateTime parsedDate;
            if(!TryParseDate(schoolApplication.DogumTarihi, out parsedDate)) ModelState.AddModelError("DogumTarihi", "Geçersiz tarih.");

            //model kontrol
            if (!ModelState.IsValid) return VinovFonksiyonlar.InvalidModel(ModelState);


            //başvuru

            string RequestBody = @$"
            {{
            ""IdentityNo"": ""{schoolApplication.Tckn}"",
            ""PhoneNumber"": ""{schoolApplication.Telefon}"",
            ""WorkPlaceID"": {schoolApplication.School},
            ""BirthDate"": ""{schoolApplication.DogumTarihi}""
            }}
            ";


            ApiSonuc<BasvuruCevap> apiSonuc = vakifBankApi.ApiIstek<BasvuruCevap>(HttpMethod.Post, "/vinov/schoolApplication", RequestBody);

            if (apiSonuc.Succeded)
            {
                if (!apiSonuc.Result.Data.Result.IsSucceeded)
                {
                    _logger.Log(LogLevel.Error, apiSonuc.Result.Data.Result.RejectReasonDescription);
                    return Json(new { Succeded = false, ErrorMessage = apiSonuc.Result.Data.Result.RejectReasonDescription });
                }

                if(apiSonuc.Result.Data.Result.StatusCode == 1)
                {
                    return Json(new { Succeded = true, ApiResponse = apiSonuc.Result.Data.Result });
                }
                else
                {
                    _logger.Log(LogLevel.Error, apiSonuc.Result.Data.Result.RejectReasonDescription);
                    return Json(new { Succeded = false, ErrorMessage = apiSonuc.Result.Data.Result.RejectReasonDescription });
                }   
            }
            else
            {
                _logger.Log(LogLevel.Error, apiSonuc.ErrorMessage);
                return Json(new { Succeded = false, ErrorMessage = apiSonuc.ErrorMessage });
            }


            //BasvuruCevap basvuruCevap = VinovFonksiyonlar.BasvuruYap(schoolApplication);

            //if (basvuruCevap.Data.Result.IsSucceeded)
            //{
            //    return Json(new { Succeded = true });
            //}
            //else
            //{
            //    return Json(new { Succeded = false, ErrorMessage = basvuruCevap.Header.StatusDescription });
            //}
        }



        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GenerateCaptcha()
        {
            VakifBankApi vakifBankApi = new();

            ApiSonuc<CaptchaCevap> CaptchaGetir = vakifBankApi.ApiIstek<CaptchaCevap>(HttpMethod.Get, "/captcha", "");

            if (CaptchaGetir.Succeded)
            {
                HttpContext.Session.SetString("CaptchaId", CaptchaGetir.Result.Data.CaptchaId);
                string base64ImageData = CaptchaGetir.Result.Data.CaptchaImage;
                byte[] imageData = Convert.FromBase64String(base64ImageData.Split(',')[1]);
                using MemoryStream ms = new(imageData);
                return File(ms.ToArray(), "image/jpeg");
            }
            else
            {
                throw new Exception($"Captcha getirilirken bir hata oluştu. <br/> {CaptchaGetir.ErrorMessage}");
            }


        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
         
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            _logger.Log(LogLevel.Error, exception.Message);

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Exception = exception
            });
        }

        private bool TryParseDate(string dateString, out DateTime validDate)
        {
            bool isValidDate = DateTime.TryParseExact(
                dateString,
                "dd/MM/yyyy",
                CultureInfo.GetCultureInfo("tr-TR"),
                DateTimeStyles.None,
                out validDate);

            return isValidDate && validDate.Year > 1900;
        }
    }
}