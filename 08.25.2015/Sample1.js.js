﻿function GetBusinessTypeChangeFields(AssociateId) {

    // Alert("Hi");
    $.ajax({
        type: 'POST',
        cache: false,
        url: "/Associate/GetBusinessTypeChangeFields",
        data: { id: AssociateId },
        success: function (result) {
            var varMaxDate = new Date();
            $("#BusinessTypeChangeWindow").load('/Associate/BusinessTypeChange #BusinessTypeChange', function () {

                var scheduledDatePicker = $("#BusinessTypeChange .ScheduledDate").kendoDatePicker({
                    culture: "en-GB",
                    format: "dd/MM/yyyy",
                    month: {
                        // template for dates in month view
                        content: '# if (data.date.getDay() != 1) { #' +
                        '<div class="disabledDay">#= data.value #</div>' +
                        '# } else { #' +
                        '#= data.value #' +
                        '# } #'
                    },
                    open: function (e) {
                        $(".disabledDay").parent().removeClass("k-link") //removing this class makes the day unselectable
                        $(".disable dDay").parent().removeAttr("href") //this removes the hyperlink styling
                    }
                }).data("kendoDatePicker");


                var scheduledDatePicker1 = $("#ScheduledDateVatRegistered").kendoDatePicker({
                    culture: "en-GB",
                    format: "dd/MM/yyyy",
                    max: varMaxDate,
                    open: function (e) {
                        $(".disabledDay").parent().removeClass("k-link") //removing this class makes the day unselectable
                        $(".disable dDay").parent().removeAttr("href") //this removes the hyperlink styling
                    }
                }).data("kendoDatePicker");

                var scheduledDatePicker2 = $("#ScheduledVATDeRegistrationDate").kendoDatePicker({
                    culture: "en-GB",
                    format: "dd/MM/yyyy",
                    max: varMaxDate,
                    open: function (e) {
                        $(".disabledDay").parent().removeClass("k-link") //removing this class makes the day unselectable
                        $(".disable dDay").parent().removeAttr("href") //this removes the hyperlink styling
                    }
                }).data("kendoDatePicker");

                var input = $("<input type='text' id='ScheduledBusinessTypeId' style='margin-left:15px;width:344px;' />");
                $("#BusinessTypeFieldContainer").append(input);
                $(input).kendoDropDownList({
                    animation: false,
                    dataSource: businessTypesOptions,
                    dataTextField: "Text",
                    dataValueField: "Value",
                    value: $("#BusinessTypeId").val(),
                    change: function (e) {
                        setScheduledBusinessTypeVisibility();
                    }
                });
                var origUmbrellaCompanyId = $("#UmbrellaCompanyId").val();
                var input1 = $("<input type='text' id='ScheduledUmbrellaCompanyId' />");
                $("#UmbrellaCompanyFieldContainer").append(input1);
                $(input1).kendoDropDownList({
                    animation: false,
                    dataSource: umbrellaCompanyOptions,
                    dataTextField: "Text",
                    dataValueField: "Value",
                    value: origUmbrellaCompanyId,
                    optionLabel: "-- Select --",
                    change: function (e) {

                        setScheduledBusinessTypeVisibility();
                    }
                });

                $("#ScheduledVATRegistered").prop("checked", $("#VATRegistered").prop("checked"));
                $("#ScheduledVATDeRegistrationDate").val($("#VATDeRegistrationDate").val());
                $("#ScheduledVATRegistrationNumber").val($("#VATRegistration").val());
                $("#ScheduledDateVatRegistered").val($("#DateVatRegistered").val());
               
                $("#ScheduledRegisteredCompanyName").val($("#RegisteredCompanyName").val());
                $("#ScheduledRegisteredCompanyAddress").val($("#RegisteredCompanyAddress").val());
                $("#ScheduledLimitedCompanyNumber").val($("#LimitedCompanyNumber").val());
                $("#ScheduledRegistedCompanyBankAcctName").val($("#RegistedCompanyBankAcctName").val());
                $("#ScheduledRegistedCompanyBankAcctSort").val($("#RegistedCompanyBankAcctSort").val());
                $("#ScheduledRegistedCompanyBankAcctNumber").val($("#RegistedCompanyBankAcctNumber").val());
                $("input[name=ScheduledOptOutSelfBilling][value=" + $('input:radio[name=OptOutSelfBilling]:checked').val()  + "]").attr('checked', true);
                $("#ScheduledOtherUmbrellaCompanyName").val($("#OtherUmbrellaCompanyName").val());
                $("#ScheduledOtherUmbrellaContactEmail").val($("#OtherUmbrellaContactEmail").val());
                $("#ScheduledOtherUmbrellaContactName").val($("#OtherUmbrellaContactName").val());

                if (origUmbrellaCompanyId != "") {
                    var dropdownlist = $("#ScheduledUmbrellaCompanyId").data("kendoDropDownList");
                    dropdownlist.select(function (dataItem) {
                        return dataItem.Value === origUmbrellaCompanyId;
                    });

                }


                setScheduledBusinessTypeVisibility();
                ScheduledVATRegisteredChanged();

                var window = $("#BusinessTypeChangeWindow").kendoWindow({
                    title: "BusinessType",
                    modal: true,
                    width: "40%"
                }).data("kendoWindow");

                window.center().open();
                window.title("Business Type Changes");

                var validator = $("#form_BusinessTypeChange").kendoValidator(
                    {
                        messages: {
                            // defines a message for the 'custom' validation rule
                            //  custom: "Please enter valid value for my custom rule",

                            // overrides the built-in message for the required rule
                            //   required: "This field is required.",

                            // overrides the built-in message for the email rule
                            // with a custom function that returns the actual message
                            email: function (input) {
                                return getMessage(input);
                            },

                            LtdLLpName: function (input) {

                                return getMessage(input);
                            },
                            LtdLLPAddress: function (input) {

                                return getMessage(input);
                            },
                            LtdLLPRegNum: function (input) {

                                return getMessage(input);
                            }
                            ,
                            umbcompname: function (input) {

                                return getMessage(input);
                            },
                            umbcompemail: function (input) {

                                return getMessage(input);
                            },
                            umbcompcontname: function (input) {

                                return getMessage(input);
                            }

                        },
                        rules: {


                            vatregnumb: function (input) {
                                if (input.is("[data-vatregnumb-msg]")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    if (varBusinessTypeId == 1) {
                                        var varSchVATReg = ($("#ScheduledVATRegistered").prop("checked") ? 1 : 0);
                                        if (varSchVATReg == 1) {

                                            return ($("#ScheduledVATRegistrationNumber").val() != "")
                                        }
                                        else {

                                            return true;
                                        }
                                    }
                                }

                                return true;
                            },

                            LtdLLpName: function (input) {
                                if (input.is("[data-LLPName-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    if (varBusinessTypeId == 1) {
                                        return ($("#ScheduledRegisteredCompanyName").val() != "")
                                    }
                                    else {
                                        return true;
                                    }
                                }
                                return true;

                            },
                            LtdLLPAddress: function (input) {
                                if (input.is("[data-LLPAddress-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    if (varBusinessTypeId == 1) {
                                        return ($("#ScheduledRegisteredCompanyAddress").val() != "")
                                    }
                                    else { return true; }
                                }
                                return true;
                            },
                            LtdLLPRegNum: function (input) {
                                if (input.is("[data-LLPRegNum-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    if (varBusinessTypeId == 1) {
                                        return ($("#ScheduledLimitedCompanyNumber").val() != "")
                                    }
                                    else { return true; }
                                }
                                return true;
                            },
                            umbcompname: function (input) {
                                if (input.is("[data-umbcompname-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    var varUmbrellaCompnayId = $("#ScheduledUmbrellaCompanyId").val();
                                    if (varBusinessTypeId == 3 && varUmbrellaCompnayId == 1) {
                                        return ($("#ScheduledOtherUmbrellaCompanyName").val() != "")
                                    }
                                    else {
                                        return true;
                                    }
                                }
                                return true;

                            },
                            umbcompemail: function (input) {
                                if (input.is("[data-umbcompemail-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    var varUmbrellaCompnayId = $("#ScheduledUmbrellaCompanyId").val();
                                    if (varBusinessTypeId == 3 && varUmbrellaCompnayId == 1) {
                                        return ($("#ScheduledOtherUmbrellaContactEmail").val() != "")
                                    }
                                    else { return true; }
                                }
                                return true;
                            },
                            umbcompcontname: function (input) {
                                if (input.is("[data-umbcompcontname-msg")) {
                                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                                    var varUmbrellaCompnayId = $("#ScheduledUmbrellaCompanyId").val();
                                    if (varBusinessTypeId == 3 && varUmbrellaCompnayId == 1) {
                                        return ($("#ScheduledOtherUmbrellaContactName").val() != "")
                                    }
                                    else { return true; }
                                }
                                return true;
                            }


                        },



                        messages: {
                            LtdLLpName: "Please Enter Ltd LLP Name",
                            LtdLLPAddress: "Please Enter Ltd LLp Address",
                            LtdLLPRegNum: "Please Enter Ltd LLP Register number",
                            //  vatderegdate:"asd,jhaksjdhkajsdhkjasd"

                        },

                        errorTemplate: "<span>#=message#</span>"

                    }).data("kendoValidator");
                function getMessage(input) {

                    return input.data("message");
                }

                $("#BusinessTypeChange .ScheduledDate").blur(function () {
                    var vatSchDateVal = scheduledDatePicker.value();
                    
                    if (vatSchDateVal == null )
                    {
                        $("#errSchDatestatus").text("Please Choose Scheduled Date").css('color', 'red');
                        return false;
                    }
                                        
                    else if (vatSchDateVal != "") {
                            $("#errSchDatestatus").text("");
                            return true;
                    }
                    else {
                        $("#errSchDatestatus").text("Please Choose Scheduled Date").css('color', 'red');
                        return false;
                    }
                    
                });
                var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                if (varBusinessTypeId == 1) {
                    $("#ScheduledVATRegistrationNumber").keypress(function (e) {
                        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
                            $("#status").html("Digits Only").show().css('color', 'red');
                            return false;
                        }
                    });
                    $("#ScheduledVATRegistrationNumber").blur(function () {
                        var val = $("#ScheduledVATRegistrationNumber").kendoValidator().data("kendoValidator");
                        var val1 = $("#ScheduledVATRegistrationNumber").val();

                        if (val.validate()) {
                            if (val1.length==9) {
                                $("#status").text("");
                                return true;
                            }
                            else {
                                $("#status").text("Please Enter 9 digit Numeric Value").css('color', 'red');
                                return false;
                            }



                        }
                        else {
                            return false;

                        }
                    });
                    $("#ScheduledLimitedCompanyNumber").keypress(function (e) {
                        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
                            $("#LLPStatus").html("Digits Only").show().css('color', 'red');
                            return false;
                        }
                    });
                    $("#ScheduledLimitedCompanyNumber").blur(function () {
                        var val = $("#ScheduledLimitedCompanyNumber").kendoValidator().data("kendoValidator");
                        var val1 = $("#ScheduledLimitedCompanyNumber").val();

                        if (val.validate()) {
                            if (val1.length == 7) {
                                $("#LLPStatus").text("");
                                return true;
                            }
                            else {
                                $("#LLPStatus").text("Please Enter 7 digit Numeric Value").css('color', 'red');
                                return false;
                            }



                        }
                        else {
                            return false;

                        }
                    });
                    $("#ScheduledDateVatRegistered").blur(function () {
                        var vatRegister = $("#ScheduledDateVatRegistered").kendoValidator().data("kendoValidator");
                        var varRegisterVal = $("#ScheduledDateVatRegistered").val();
                        if (vatRegister.validate()) {
                            if (varRegisterVal != "") {
                                $("#dateStatus").text("");
                               
                            }
                            else {
                                $("#dateStatus").text("Please Choose Date Vat Registered").css('color', 'red');
                                return false;
                            }



                        }
                        else {
                            return false;

                        }
                    });
                    $("#ScheduledVATDeRegistrationDate").blur(function () {
                        var vatDRegister = $("#ScheduledVATDeRegistrationDate").kendoValidator().data("kendoValidator");
                        var varDRegisterVal = $("#ScheduledVATDeRegistrationDate").val();
                        if (vatDRegister.validate()) {
                            if (varDRegisterVal != "") {
                                $("#dateDstatus").text("");
                                return true;
                            }
                            else {
                                $("#dateDstatus").text("Please Choose Date Vat Registered").css('color', 'red');
                                return false;
                            }



                        }
                        else {
                            return false;

                        }
                    });
                }




                //bind events
                $("#btnCreateScheduledChange").click(function () {
                    var vatSchDateVal = scheduledDatePicker.value();
                    if (vatSchDateVal == null )
                    {
                        $("#errSchDatestatus").text("Please Choose Scheduled Date").css('color', 'red');
                        return false;
                    }
                                        
                    else{
                        if (vatSchDateVal != "") {
                            $("#errSchDatestatus").text("");
                        }
                        else {
                            $("#errSchDatestatus").text("Please Choose Scheduled Date").css('color', 'red');
                            return false;
                        }
                    }
                    var varBusinessTypeId = $("#ScheduledBusinessTypeId").val();
                    if (varBusinessTypeId == 1) {
                        var varSchVATReg = ($("#ScheduledVATRegistered").prop("checked") ? 1 : 0);
                        if (varSchVATReg == 0) {

                            var vatDRegisterC = $("#ScheduledVATDeRegistrationDate").kendoValidator().data("kendoValidator");
                            var varDRegisterValC = $("#ScheduledVATDeRegistrationDate").val();
                            // alert(val1111);
                            if (vatDRegisterC.validate()) {
                                if (varDRegisterValC != "") {
                                    $("#dateDstatus").text("");
                                }
                                else {
                                    $("#dateDstatus").text("Please Choose Date VatDE Registered").css('color', 'red');
                                    return false;
                                }
                            }
                            else {
                                return false;

                            }
                        }

                        else {
                            var val = $("#ScheduledVATRegistrationNumber").kendoValidator().data("kendoValidator");
                            var val1 = $("#ScheduledVATRegistrationNumber").val();

                            if (val.validate()) {
                                if (val1.length == 9) {
                                    $("#status").text("");
                                }
                                else {
                                    $("#status").text("Please Enter 9 digit Numeric Value").css('color', 'red');
                                    return false;
                                }
                            }
                            else {
                                return false;
                            }

                            var vatRegisterC = $("#ScheduledDateVatRegistered").kendoValidator().data("kendoValidator");
                            var varRegisterValC = $("#ScheduledDateVatRegistered").val();
                            if (vatRegisterC.validate()) {

                                if (varRegisterValC != "") {
                                    $("#dateStatus").text("");
                                }
                                else {
                                    $("#dateStatus").text("Please Choose Date Vat Registered").css('color', 'red');
                                    return false;
                                }
                            }
                            else {
                                return false;

                            }
                        }
                        var val = $("#ScheduledLimitedCompanyNumber").kendoValidator().data("kendoValidator");
                        var val1 = $("#ScheduledLimitedCompanyNumber").val();

                        if (val.validate()) {
                            if (val1!="" && val1.length == 7) {
                                $("#LLPStatus").text("");
                               
                            }
                            else {
                                $("#LLPStatus").text("Please Enter 7 digit Numeric Value").css('color', 'red');
                                return false;
                            }



                        }
                        else {
                            return false;

                        }
                    }
                    else if (varBusinessTypeId == 3) {
                        if ($("#ScheduledUmbrellaCompanyId").val() == "") {
                            $("#umbrellaCompanyIdStatus").text("Please Choose Umbrella Company").css('color', 'red');
                            return false;
                        }


                    }
                    
                    var isValid = validator.validate();
                    if (isValid == false) {
                        return false;
                    }

                    var isValid = true;
                    var scheduledDate = "";
                    var varVATdeRegisteredDate = "";
                    var varVATRegDate = "";
                    try {
                        var dt = scheduledDatePicker.value();
                        scheduledDate = dt.getDate() + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear();
                    }
                    catch (e) {
                        isValid = false;
                    }

                    try {
                        var dt2 = scheduledDatePicker2.value();
                        varVATdeRegisteredDate = (dt2.getMonth() + 1) + "/" + dt2.getDate() + "/" + dt2.getFullYear() + " 00:00:00.000";
                    }

                    catch (e) {
                        // isValid = false;
                    }

                    try {
                        var dt1 = scheduledDatePicker1.value();
                        varVATRegDate = (dt1.getMonth() + 1) + "/" + dt1.getDate() + "/" + dt1.getFullYear() + " 00:00:00.000";
                    }
                    catch (e) {
                        // isValid = false;
                    }
                    var varAssId = AssociateId;
                    var BusinessTypeId = $("#ScheduledBusinessTypeId").val();

                    var VATRegistered = "";

                    var DateVatDeRegistered = "";
                    var varVATRegDate1 = "";
                    var VATRegistrationNumber = "";
                    var RegisteredCompanyName = "";
                    var RegisteredCompanyAddress = "";
                    var LimitedCompanyNumber = "";
                    var RegistedCompanyBankAcctName = "";
                    var RegistedCompanyBankAcctSort = "";
                    var RegistedCompanyBankAcctNumber = "";
                    var OptOutSelfBilling = "";
                    var UmbrellaCompanyId = "";
                    var varUmbrellaCompName = "";
                    var varUmbrellaContactName = "";
                    var varUmbrellaContactEMail = "";

                    if (BusinessTypeId == 1) {


                        var VATRegistered = ($("#ScheduledVATRegistered").prop("checked") ? 1 : 0);

                        if (VATRegistered == 1) {
                            VATRegistrationNumber = $("#ScheduledVATRegistrationNumber").val();
                            varVATRegDate1 = varVATRegDate;
                        }
                        else {
                            DateVatDeRegistered = varVATdeRegisteredDate;
                        }
                        RegisteredCompanyName = $("#ScheduledRegisteredCompanyName").val();
                        RegisteredCompanyAddress = $("#ScheduledRegisteredCompanyAddress").val();
                        LimitedCompanyNumber = $("#ScheduledLimitedCompanyNumber").val();
                        RegistedCompanyBankAcctName = $("#ScheduledRegistedCompanyBankAcctName").val();
                        RegistedCompanyBankAcctSort = $("#ScheduledRegistedCompanyBankAcctSort").val();
                        RegistedCompanyBankAcctNumber = $("#ScheduledRegistedCompanyBankAcctNumber").val();
                        OptOutSelfBilling = $('input:radio[name=ScheduledOptOutSelfBilling]:checked').val();
                    }

                    else if (BusinessTypeId == 3) {
                        var UmbrellaCompanyId = $('#ScheduledUmbrellaCompanyId').val();
                        if ($('#ScheduledUmbrellaCompanyId').val() == "1") {
                            varUmbrellaCompName = $("#ScheduledOtherUmbrellaCompanyName").val();
                            varUmbrellaContactName = $("#ScheduledOtherUmbrellaContactName").val();
                            varUmbrellaContactEMail = $("#ScheduledOtherUmbrellaContactEmail").val();
                        }

                    }

                    var varAssId = AssociateId;


                    var change = [
               { field: 'BusinessTypeId', value: BusinessTypeId, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'VATRegistered', value: VATRegistered, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'DateVatDeRegistered', value: DateVatDeRegistered, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'VATRegistration', value: VATRegistrationNumber, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'DateVatRegistered', value: varVATRegDate1, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'RegisteredCompanyName', value: RegisteredCompanyName, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'RegisteredCompanyAddress', value: RegisteredCompanyAddress, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'LimitedCompanyNumber', value: LimitedCompanyNumber, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'RegistedCompanyBankAcctName', value: RegistedCompanyBankAcctName, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'RegistedCompanyBankAcctSort', value: RegistedCompanyBankAcctSort, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'RegistedCompanyBankAcctNumber', value: RegistedCompanyBankAcctNumber, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'OptOutSelfBilling', value: OptOutSelfBilling, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'UmbrellaCompanyId', value: UmbrellaCompanyId, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'OtherUmbrellaCompanyName', value: varUmbrellaCompName, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'OtherUmbrellaContactEmail', value: varUmbrellaContactEMail, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' },
               { field: 'OtherUmbrellaContactName', value: varUmbrellaContactName, AssId: varAssId, taskId: 'TASKIDPLACEHOLDER' }
                    ];
                   
                    var associateIds = [parseInt(AssociateId)];
                    createTaskForAssScheduledChange(associateIds, scheduledDate, "BusinessTypes", change);
                });

                $("#btnCreateScheduledCancel").click(function () {
                    window.close();
                    $("#ScheduledChangeWindow").html();
                });



            });


        }
    });

}

function createTaskForAssScheduledChange(associateIds, scheduledDate, field, value) {

    $.ajax({
        type: "POST",
        url: "/Scheduler/CreateTask",
        traditional: true,
        data: {
            ids: associateIds,
            scheduledDate: scheduledDate,
            field: field,
            value: JSON.stringify(value)
        },
        error: function (result) {
            return false;
        },
        success: function (result) {
            alert('Associate BusinessType details successfully updated.');
            //hide dialog
            var window = $("#BusinessTypeChangeWindow").data("kendoWindow");
            window.close();
            return false;
        }
    });
}
function setScheduledBusinessTypeVisibility() {
    $("#umbrellaCompanyIdStatus").text("");
    var businessTypeId = $('#ScheduledBusinessTypeId').val();
    var umbrellaCompanyId = $('#ScheduledUmbrellaCompanyId').val();
    var vatRegInfoRequired;

    if ((businessTypeId === "3")) {

        $("#ScheduledUmbrellaList, #ScheduledUmbrellaCompany").show();
        if (umbrellaCompanyId === "1") {
            $("#ScheduledUmbrellaOther").show();
        } else {
            $("#ScheduledUmbrellaOther").hide();
        }
    } else {
        $("#ScheduledUmbrellaOther, #ScheduledUmbrellaList, #ScheduledUmbrellaCompany").hide();
        $('#ScheduledUmbrellaCompanyId').val("");
    }

    if (businessTypeId === "1") {

        $("#ScheduledLtdCompanySection").show();

        vatRegInfoRequired = $("#VATRegistered").is(":checked") ? "True" : "False";
    } else {
        $("#ScheduledLtdCompanySection").hide();
        vatRegInfoRequired = "False";
    }

    $("#ScheduledVatRegInfoRequired").val(vatRegInfoRequired).trigger("change");
}

function ScheduledVATRegisteredChanged() {
    if ($('#ScheduledVATRegistered').is(':checked')) {
        $("#SchVatRegInfoRequired").val("True");
        $('#ScheduledVATRegistration').show();
        $('#ScheduledVATDeRegistration').hide();

    } else {
        $("#SchVatRegInfoRequired").val("False");
        $('#ScheduledVATRegistration').hide();
        $('#ScheduledVATDeRegistration').show();

    }
}


//$(function () {


//    SetBusinessSchValidation();
//});

function SetBusinessSchValidation() {

    $("form_BusinessTypeChange").kendoValidator({
        rules: {

            vatderegdate: function (input) {

                if (input.is("[data-vatderegdate-msg]") && input.val() != "") {

                    var varSchVATReg = ($("#ScheduledVATRegistered").prop("checked") ? 1 : 0);
                    if (varSchVATReg == 0) {
                        return ($("#ScheduledVATDeRegistrationDate").val() == "");
                    }
                    else { return true; }

                }

                return true;
            }
        }, errorTemplate: "<span class='field-validation-error'>#=message#</span>"
    });

}