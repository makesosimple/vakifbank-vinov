
function postJSON(url, data, successcallback, errorcallback) {
    return jQuery.ajax({
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        },
        'type': 'POST',
        'url': url,
        'data': JSON.stringify(data),
        'dataType': 'json',
        'success': successcallback,
        'error': errorcallback
    });
}


function getResultMessage(result, desc) {
    if (desc) {
        
        switch (result) {
            case "1":
                return '<p class="centerPosition">' + desc + '</p>'; 
            case "2":
                return '<p class="centerPosition">' + desc + '</p>'; 
            case "3":
                return '<p class="centerPosition">' + desc + '</p>'; 
            default:
                return "Bilinmeyen bir hata oluştu.";
        }
    } else {
        switch (result) {
            case "1":
                return `<p class="centerPosition">
                        Başvurunuz olumlu sonuçlanmış olup ön limitiniz onaylanmıştır.<br />
                        VakıfBank mobil, internet bankacılığı menülerinden ya da Şubelerimiz aracılığıyla Vinov hesap tanımınızı gerçekleştirebilirsiniz.
                    </p>`;
            case "2":
                return `<p class="centerPosition">
                        Başvurunuz olumlu sonuçlanmış olup ön limitiniz onaylanmıştır.<br />
                        Bankamız müşterisi olabilir devamında VakıfBank mobil, internet bankacılığı menülerinden ya da Şubelerimiz aracılığıyla Vinov hesap tanımınızı gerçekleştirebilirsiniz.
                    </p>`;
            case "3":
                return `<p class="centerPosition">
                        Başvurunuz olumlu sonuçlanmış olup ön limitiniz onaylanmıştır. Okul kayıt işlemlerinizi gerçekleştirebilirsiniz.
                    </p>`;
            default:
                return "Bilinmeyen bir hata oluştu.";
        }
    }
    
}

$(function () {

    const swalCustomButtons = Swal.mixin({
        customClass: {
            confirmButton: "custom-button !w-20 !py-2",
            cancelButton: "btn btn-danger"
        },
        buttonsStyling: false
    });

    $('.ajaxform').on("submit", "form", function (event) {
        event.preventDefault();
        var form = $(this);
        $(this).find(':button').prop('disabled', true);
        // var btnoldvalue = $(this).find(':submit').html();
        // $(this).find(':submit').html('<div class="loading-spinner"></div>Yükleniyor...');

       // var data = { "Succeded": true, "ApiResponse": { "RejectReasonCode": 5 }, "ErrorMessage": null };
  
        var form = $(this);
        $.ajax({
            url: $(this).attr("action"),
            type: $(this).attr("method"),
            dataType: "JSON",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (data, status) {
                if (data.Succeded == true) {
                    if (form.attr("afterform") != "slient") {
                        //top.location = '/Home/SchoolApplication?Result=' + data.ApiResponse.RejectReasonCode;

                        const rejectCode = data.ApiResponse.RejectReasonCode.toString();
                        const rejectReason = data.ApiResponse.RejectReasonDescription.toString();
                        const resultMessage = getResultMessage(rejectCode, rejectReason);
                        let icon = 'success';

                        if (!["1", "2", "3"].includes(rejectCode)) {
                            icon = 'warning';
                        }

                        swalCustomButtons.fire({
                            html: resultMessage,
                            icon: icon,
                            allowOutsideClick: false,
                            confirmButtonText: 'Tamam'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.href = 'https://www.vakifbank.com.tr/';
                            }
                        });

                    }
                } else {
                    reloadCaptcha();
                    swalCustomButtons.fire({
                        html: data.ErrorMessage, icon: 'warning', allowOutsideClick: false, confirmButtonText: 'Tamam'
                    })
                        .then(function () {

                        });
                }
            },
            error: function (xhr, desc, err) {
                reloadCaptcha();
                swalCustomButtons.fire({ html: err, icon: 'warning', allowOutsideClick: false, confirmButtonText: 'Tamam' })
                    .then(function () {

                    });
            },
            complete: function () {
                form[0].reset();
                //$(":submit").html(btnoldvalue);
                $(":button").removeAttr("disabled");
            }
        });
    });
});

$('.date').mask('00/00/0000');

//Swal.fire({
//    html: `Geçersiz sonuç numarası`, icon: 'success', allowOutsideClick: false, confirmButtonText: 'Tamam'
//}).then(function (result) {
//        if (result.isConfirmed) window.location.href = 'https://www.vakifbank.com.tr/';
//});

function reloadCaptcha() {
    var captchaimg = document.getElementById("captchaimg");
    captchaimg.src = captchaimg.src + "?" + Math.random();
    $("#Captcha").val("");
}

// extend range validator method to treat checkboxes differently
var defaultRangeValidator = $.validator.methods.range;
$.validator.methods.range = function (value, element, param) {
    if (element.type === 'checkbox') {
        // if it's a checkbox return true if it is checked
        return element.checked;
    } else {
        // otherwise run the default validation function
        return defaultRangeValidator.call(this, value, element, param);
    }
};

$.validator.addMethod("dateDDMMYYYY", function (value, element) {
    // Parse the date with Moment.js
    var date = moment(value, "DD/MM/YYYY", true);

    // Check if the date is valid and after the year 1900
    return this.optional(element) || (date.isValid() && date.year() > 1900);
}, "Please enter a valid date in the format dd/mm/yyyy that is after the year 1900.");


$.validator.methods.date = function (value, element) {
    return $.validator.methods.dateDDMMYYYY.call(this, value, element);
};

var settings = {
    validClass: "is-valid",
    errorClass: "is-invalid"
};
$.validator.setDefaults(settings);
$.validator.unobtrusive.options = settings;