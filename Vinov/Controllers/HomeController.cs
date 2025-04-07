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
                //throw new Exception($"Okullar listelenirken bir hata oluştu. <br/> {OkulListele.ErrorMessage}");
                ViewBag.ApiError = "Bilinmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                ViewBag.SchoolList = new List<SelectListItem>();
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
                //throw new Exception($"Bilinmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
                //throw new Exception($"Captcha getirilirken bir hata oluştu. <br/> {CaptchaGetir.ErrorMessage}");
                //ViewBag.ApiError = "Bilinmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                //ViewBag.SchoolList = new List<SelectListItem>();
                /*byte[] onePixelWhiteJpeg = new byte[]
                {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
                    0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60,
                    0x00, 0x60, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
                    0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08,
                    0x07, 0x07, 0x07, 0x09, 0x09, 0x08, 0x0A, 0x0C,
                    0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
                    0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D,
                    0x1A, 0x1C, 0x1C, 0x20, 0x24, 0x2E, 0x27, 0x20,
                    0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
                    0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27,
                    0x39, 0x3D, 0x38, 0x32, 0x3C, 0x2E, 0x33, 0x34,
                    0x32, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01,
                    0x00, 0x01, 0x03, 0x01, 0x11, 0x00, 0x02, 0x11,
                    0x01, 0x03, 0x11, 0x01, 0xFF, 0xC4, 0x00, 0x1F,
                    0x00, 0x00, 0x01, 0x05, 0x01, 0x01, 0x01, 0x01,
                    0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                    0x07, 0x08, 0x09, 0x0A, 0x0B, 0xFF, 0xDA, 0x00,
                    0x0C, 0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11,
                    0x00, 0x3F, 0x00, 0xFF, 0xD9
                };

                //Response.Headers["X-Captcha-Error"] = "true";
                //return File(onePixelWhiteJpeg, "image/jpeg");*/

                Response.StatusCode = 500;
                // return invalid content that breaks the image rendering
                return Content("captcha_error", "text/plain");

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