/// <reference path="~/Scripts/_references.js" />

var addressLookupMain = false;
var tabInd = false;
var tabApprovals = false;
var tabTMSInv = false;
var tabTMS = false;
var tabAbsence = false;
var tabDocs = false;
var tabLibrary = false;

function InitialiseTabs(selectedTab) {
    var tabStrip = $("#tabs").kendoTabStrip().data("kendoTabStrip");
    var tabName = $.trim($(tabStrip.items()[selectedTab]).text().toLowerCase());

    // if (!Readonly) {
    if (tabName === "timesheets"
        || tabName === "individual"
        || tabName === "approvals"
        || tabName === "timesheets & invoices") {
        //$(".submit-button").attr('disabled', 'disabled');
        $(".submit-button").hide();
    } else {
        //$(".submit-button").removeAttr('disabled');
        $(".submit-button").show();
        //}
    }

    if (tabName === "timesheets & invoices") {
        if (tabTMSInv == false) {
            var id = $('input[type=hidden][name=Id]').val();
            InitialiseAssociateTimesheetGrid();
            InitialiseAssociateRetainerGrid();
            InitialiseInvoicedAssociateTimesheetGrid();
            InitialiseAssociateInvoiceGrid();
            tabTMSInv = true;
        }
        else {
            ReclickTimesheetInv();
            ReclickInv();

        }
    }

    if (tabName === "timesheets & invoices") {
        if (tabLibrary == false) {
            InitialiseAssociateTimesheetGrid();

            tabLibrary = true;
        }
        else {

        }
    }

    else if (tabName === "approvals") {
        if (tabApprovals == false) {

            var id = $('input[type=hidden][name=Id]').val();
            InitialiseApproverTimesheetGrid();
            InitialiseOtherApproverTimesheetGrid();
            InitialiseApproverRetainerGrid();
            InitialiseAwaitingApprovalTimesheetGrid();
            populateAbsenceApprovals();
            tabApprovals = true;
        }
        else {

            ReclickApprovals();
        }
    }

    else if (tabName === "activity") {
        // Initialise communications tab on demand.
        // Tab can be loaded either through explicit select or implicitly 
        // by passing in the URL, e.g. Associate/Details/380#tabs-5 

        if (!commsHistoryVM) {
            commsHistoryVM = new CommunicationsHistoryVM();
        }

        commsHistoryVM.GetPagingDataIfNotLoaded();
        $("#activityDocumentsGrid").kendoGrid({
            dataSource: {
                type: "json",
                transport: {
                    read: {
                        url: "/Associate/GetActivityUploadedDocuments",
                        data: {
                            associateId: $("#Id").val(),

                            uploadedDocuments: ""
                        },
                        dataType: "json",
                        type: "POST"
                    }
                },
                schema: {
                    model: {
                        fields: {
                            DocumentId: { type: "string" },
                            Title: { type: "string" },
                            CreatedDate: { type: "date" },
                            Size: { type: "int" }

                        }
                    }
                },
                pageSize: 2,
                serverPaging: false,
                serverFiltering: false,
                serverSorting: false,
                autoSync: false
            },
            scrollable: false,
            filterable: false,
            sortable: true,
            pageable: true,
            columns: [
            { field: "Title", title: "Title", width: "100px" },
            { field: "Size", title: "Size(MB)", width: "50px" },
            { title: "Actions", width: "50px", template: kendo.template($("#activityemaildocument-delete-template").html().replace("varDocFileId", "'#= DocumentId #'")) }

            ]
        });


    }
    else if (tabName === "absence") {

        populateCurrentAbsences(tabAbsence);
        populateOlderAbsences(tabAbsence);
        tabAbsence = true;

    }
    else if (tabName === "individual") {

        $("#invidiual_form").toggle();
        individualVM.GetAssignedRolesData();
        individualVM.GetPastRolesData();
        individualVM.GetAssociateProspectsData();
        $(".submit-button").attr('disabled', 'disabled');
        tabInd = true;

    }

    if (tabName === "documents") {
        if (tabDocs == false) {
            populateAssociateAssessments(0);
            initialiseDocumentsGrid();
            tabDocs = true;
        }
        else {
            populateAssociateAssessments(1);
        }
    }
}

function GetBusinessTypes() {
    return new kendo.data.DataSource({
        type: "json",
        transport: {
            read: {
                url: "/Associate/GetBusinessTypes",
                dataType: "json",
                type: "POST"
            }
        },
        schema: {
            model: {
                fields: {
                    ID: { type: "number" },
                    Type: { type: "string" },
                }
            }
        }
    });
}


$(document).ready(function () {

    $("#tabs").kendoTabStrip({
        animation: {
            open: {
                effects: "fadeIn"
            }
        },
        select: function (element) {
            $("#SelectedTab").val($(element.item).index());
            InitialiseTabs($(element.item).index());
        }
    });

    $(".BusinessTypeChangeIcon").click(
        function () {
            GetBusinessTypeChangeFields(detailsSettings.AssociateId);
        });


    kendo.culture().calendar.firstDay = 1;

    $(".datepicker").kendoDatePicker({
        culture: "en-GB",
        format: "dd/MM/yyyy"
    });

    var tabToSelect = $("#SelectedTab").val();

    if (tabToSelect) {
        $("#tabs").kendoTabStrip("select", tabToSelect);
        InitialiseTabs(tabToSelect);
    }
    else {
        $("#tabs").kendoTabStrip("select", 0);
    }

    SetEditHistoryLinkText($("#EditHistoryLink"));

    $("input:radio[name='VersionA']").change(function () {
        EnableDisableCompareButton();
    });

    $("input:radio[name='VersionB']").change(function () {
        EnableDisableCompareButton();
    });

    EnableDisableCompareButton();

    SetUpNationalities();

    SetUpCounties();

    SetUpCountries();

    SetUpGlobalEmailDisabledReasonFields();

    initBusinessAreas();

    $("input[name=HadPreviousName], #PrevForeName, #PrevSurname").change(function () {
        var forenameReqd;
        var surnameReqd;

        if ($("input[name=HadPreviousName]:checked").val() === "True") {
            forenameReqd = $.trim($("#PrevSurname").val()) === "" ? "True" : "False";
            surnameReqd = $.trim($("#PrevForeName").val()) === "" ? "True" : "False";
        } else {
            forenameReqd = surnameReqd = "False";
        }

        $("#PrevForeNameRequired").val(forenameReqd).trigger("change");
        $("#PrevSurnameRequired").val(surnameReqd).trigger("change");
    });

    $("#AssociateImageHolder").hover(
        function () {
            if (!Readonly) {
                $("#AssociateImageChanger").show();
            }
        },
        function () {
            $("#AssociateImageChanger").hide();
        }
    );

    $("#AddressLookupDialog").kendoWindow({
        visible: false,
        height: 400,
        width: 550,
        resizable: false,
        position: "center",
        modal: true,
        open: function (event, ui) {
            $('<div class="AddressLookupDialog">If the address isn\'t in the list you can <a id="manualAddressEntry">enter it manually</a></div>').insertBefore("#AddressLookupDialog");
            $("#manualAddressEntry").click(function () {
                ManualAddressEntry();
            });
        },
        close: function (event, ui) {
            $(".AddressLookupDialog").remove();
            if (addressLookupMain) {
                MainAddressLookup();
            }
        }
    });

    $("#AddressLookupDialog").on("click", "li", function () { SelectAddress(this); });

    $("body").on("click", ".addressLookupMain", function () {
        addressLookupMain = true;
        AddressLookup();
    });

    $(document.body).on("keypress", "#postCodeLookup", function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            var evt = event.target;
            var target = evt ? evt : window.event.srcElement;
            var postCode = $.trim($(target).val());
            var btn = $(target).nextAll(".submit-button:first");
            addressLookupMain = true;
            AddressLookup(postCode, btn);
            return false;
        }
    });

    $("body").on("click", ".addressLookup", function () { AddressLookup(); });

    $(document.body).on("keypress", ".postCodeLookup:not(#postCodeLookup)", function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            var evt = event.target;
            var target = evt ? evt : window.event.srcElement;
            var postCode = $.trim($(target).val());
            var btn = $(target).nextAll(".submit-button:first");
            AddressLookup(postCode, btn)
            return false;
        }
    });

    $("#AddressData").change(function () {
        var jsonString = $(this).val();

        if (jsonString == "") {
            viewModel.AddressDataRender("");
            viewModel.AddressDataRender.valueHasMutated();
            return;
        }

        var jsonObj = JSON.parse(jsonString);

        var ad = ko.mapping.fromJS(jsonObj);

        viewModel.AddressDataRender(ad);
    });

    // Start: Set field-required styles from unobtrusive validation attributes   
    $("#mainForm *[data-val-required]").each(function () {
        var label = $("label[for=" + this.id + "]");
        if (label.length === 0) {
            //console.log(this.id);
        } else {
            $(label).addClass("field-required-for-save");
        }
    });

    $('#mainForm *[data-val-mvcvtkrequiredif]').each(function () {
        var $this = $(this);
        var $label = $("label[for=" + this.id + "]");
        if ($label.length === 0) {
            //console.log(this.id);
        } else {
            var dependentProperty = ($this).data("val-mvcvtkrequiredif-dependentproperty");
            var targetValue = ($this).data("val-mvcvtkrequiredif-targetvalue");

            if (dependentProperty == "ActionNameButtonSource") {
                if (targetValue.indexOf("save details") == -1) {
                    $label.addClass("field-required-for-submit");
                } else {
                    $label.addClass("field-required-for-save");
                }
            } else {
                setUpDependentFieldRequiredClasses($label, dependentProperty, targetValue, "field-required-for-save");
            }
        }
    });

    function setUpDependentFieldRequiredClasses($label, dependentProperty, targetValue, className) {
        var nameSelector = "[name='" + dependentProperty + "']";
        setFieldRequiredClassFromDependentProperty($label, nameSelector, targetValue, className);

        //e.g. Passport Number is only mandatory if HasVisa is selected but remains visible
        //so class needs to be re evaluated on change
        $(nameSelector).change(function () {
            setFieldRequiredClassFromDependentProperty($label, nameSelector, targetValue, className);
        });
    }

    function setFieldRequiredClassFromDependentProperty($label, nameSelector, targetValue, className) {
        targetValue = (targetValue == null ? '' : targetValue).toString();

        // get the actual value of the target control
        var control = $(nameSelector);
        var controltype = control.attr('type');
        var actualvalue;

        if (controltype === 'checkbox') {
            actualvalue = control.is(':checked') ? "True" : "False";
        } else if (controltype === 'radio') {
            actualvalue = $(nameSelector + ":checked").val();
        } else {
            actualvalue = control.val();
        }

        var targetvalueArray = targetValue.split("|");

        if ($.inArray(actualvalue, targetvalueArray) > -1) {
            $label.addClass(className);
        } else {
            $label.removeClass(className);
        }
    }

    $('#mainForm *[data-val-mvcvtkrequiredifpartone]').each(function () {
        var $this = $(this);
        var $label = $("label[for=" + this.id + "]");
        if ($label.length === 0) {
            //console.log(this.id);
        } else {
            var className;

            if (($this).data("val-mvcvtkrequiredifpartone-targetvalue").indexOf("save details") == -1) {
                className = "field-required-for-submit";
            } else {
                className = "field-required-for-save";
            }

            var dependentProperty = ($this).data("val-mvcvtkrequiredifparttwo-dependentproperty");
            var targetValue = ($this).data("val-mvcvtkrequiredifparttwo-targetvalue");

            setUpDependentFieldRequiredClasses($label, dependentProperty, targetValue, className);
        }
    });

    NationalityChanged();

    $("#AddressData").trigger("change");

    CountryChanged();

    $("#tabs").css("display", "");

    var sendEmail = getParameterByName("sendEmail");

    if (sendEmail != null && Boolean(sendEmail.toLowerCase()) == true) {
        var filter = getParameterByName("filter");

        SendEmail(detailsSettings.AssociateId, 0, false, null, filter, ApproveEmailSent, "");
    }

});

function ApproveEmailSent(e) {
    var url = window.location.href;

    window.location.replace(url.split("?")[0] + "?AutoUnlocked=True");
}

// End: Set field-required styles from unobtrusive validation attributes

function GetTextFromSelectedValueinJsonList(selectedValue, list) {
    var match = $.grep(list, function (a) { return a.Value == selectedValue; });

    if (match.length === 1) {
        return match[0].Text;
    } else {
        return "";
    }
}

function ToggleEdit(element) {
    var $summaryRow = $(element).parent().parent();

    $summaryRow.next('.panelEdit').toggle();

    if ($summaryRow.next('.panelEdit').is(':visible')) {
        $(element).find('img').attr('src', '/Content/images/collapse.gif');
        $summaryRow.css('font-weight', 'bold');
    } else {
        $(element).find('img').attr('src', '/Content/images/expand.png');
        $summaryRow.css('font-weight', 'normal');
    }

    return false; // Stop default behaviour for href="#" which scrolls to top of page

}

function ToggleEditHistory(link) {
    $("#EditHistory").toggle();

    SetEditHistoryLinkText(link);

    return false;
}

function ToggleEditRefCheck(element) {
    var panel = $(element).parent().next('div.panelEditRefCheck');

    if (panel.length === 0) {
        panel = $(element).closest('.referenceRequestsTemplate').next('div.panelEditRefCheck');
    }

    panel.toggle();
    return false; // Stop default behaviour for href="#" which scrolls to top of page
}

function SetEditHistoryLinkText(link) {
    if ($("#EditHistory").is(":visible"))
        $(link).text("Hide full history");
    else
        $(link).text("Show full history");

    return false;
}

function EnableDisableCompareButton() {
    var a = $('input:radio[name=VersionA]:checked').val();
    var b = $('input:radio[name=VersionB]:checked').val();

    $("#Compare").attr("disabled", !(a && b && a != b));
}

function SetUpNationalities() {
    var nationalities = $("#NationalityId");

    for (var n in nationalitiesJson) {
        var nationality = nationalitiesJson[n];
        $('<option />', { value: nationality.Value, text: nationality.Text }).appendTo(nationalities);
    }

    if (detailsSettings.NationalityId !== null) {
        nationalities.val(detailsSettings.NationalityId);
    }
}


function SetUpCountries() {
    var countries = $("#CountryId");

    var insuranceAplicationCountries = $("#InsuranceApplication_CountryID");

    for (var n in countriesJson) {
        var country = countriesJson[n];
        $('<option />', { value: country.Value, text: country.Text }).appendTo(countries);
        $('<option />', { value: country.Value, text: country.Text }).appendTo(insuranceAplicationCountries);
    }
    if (detailsSettings.CountryId !== null) {
        countries.val(detailsSettings.CountryId);
    }

    insuranceAplicationCountries.val(insuranceAplicationCountries.data("selected"));
}

function SetUpCounties() {
    var counties = $("#CountyId");

    for (var n in countiesJson) {
        var county = countiesJson[n];
        $('<option />', { value: county.Value, text: county.Text }).appendTo(counties);
    }
    if (detailsSettings.CountyId !== null) {
        counties.val(detailsSettings.CountyId);
    }
}

function TitleChanged() {
    if ($('#PersonTitleId').val() == 5)
        $('#TitleOtherContainer').show();
    else
        $('#TitleOtherContainer').hide();
}

function CountyChanged() {
}

function NationalityChanged() {
    if ($('#NationalityId').val() === "27")
        $('#EUNumber').attr('disabled', 'disabled');
    else
        $('#EUNumber').removeAttr('disabled');
}


function CountryChanged() {
    switch ($('#CountryId').val()) {
        case "":
            $("#UkAddress").val("Unknown").trigger("change");
            $('.nonUkAddress').hide();
            $('.ukAddress').hide();
            $("#postCodeLookup").removeClass("required");
            break;
        case "1":
            var found = false;

            $("#manualEntry .ukAddress input[type='text']").each(function () {
                if ($(this).val()) {
                    $("#UkAddress").val("True").trigger("change");
                    viewModel.ShowAddressEntry(true);
                    viewModel.ShowAddressDataRender(false);

                    found = true;

                    return false;
                }
            });

            if (!found && !$("#AddressData").val()) {
                $("#UkAddress").val("Lookup").trigger("change");

                if (!$("#postCodeLookup").hasClass("required")) {
                    $("#postCodeLookup").addClass("required");
                }

                if (!$("label[for='AddressData']").hasClass("field-required-for-submit")) {
                    $("label[for='AddressData']").addClass("field-required-for-submit");
                }
            }

            if (!$("#postCodeLookup").is(":visible"))
                $("#postCodeLookup").show();

            $('.nonUkAddress').hide();
            $('.ukAddress').show();
            break;
        default:
            $("label[for='AddressData']").removeClass("field-required-for-submit");
            $("#postCodeLookup").removeClass("required");
            $("#UkAddress").val("False").trigger("change");
            $('.nonUkAddress').show();
            $('.ukAddress').hide();
            $("#postCodeLookup").removeClass("required");
    }
    SetHouseNumberRequired();
    SetHouseNameRequired();
}

function MainAddressLookup() {
    $("#UkAddress").val("Lookup").trigger("change");
    $('.nonUkAddress').hide();
    $('.ukAddress').show();
    SetHouseNumberRequired();
    SetHouseNameRequired();
    addressLookupMain = false;
}

function MainAddressManualEntry() {
    $("#UkAddress").val("True").trigger("change");
    $('.nonUkAddress').hide();
    $('.ukAddress').show();
    SetHouseNumberRequired();
    SetHouseNameRequired();
    addressLookupMain = false;
}

function SetHouseNumberRequired() {
    var countryId = parseInt($('#CountryId').val(), 10);
    var houseName = $.trim($('#HouseName').val());
    var ukAddress = $("#UkAddress").val();

    var reqd = (countryId === 1 && houseName === "" && ukAddress === "True") ? "True" : "False";

    $('#HouseNumberRequired').val(reqd).trigger('change');
}

function SetHouseNameRequired() {
    var countryId = parseInt($('#CountryId').val(), 10);
    var houseNumber = $.trim($('#HouseNumber').val());
    var ukAddress = $("#UkAddress").val();

    var reqd = (countryId === 1 && houseNumber === "" && ukAddress === "True") ? "True" : "False";

    $('#HouseNameRequired').val(reqd).trigger('change');
}

function VisaTypeChanged() {
    if ($('#VisaTypeId').val() === "8")
        $('#OtherVisaType').show();
    else
        $('#OtherVisaType').hide();
}

//function VATRegisteredChanged() {
//    if ($('#VATRegistered').is(':checked')) {
//        $('#VATRegistrationNumber').show();
//        $("#VatRegInfoRequired").val("True").trigger("change");
//        $('#VATDeRegistration').hide();
//        $('#DateVatDeRegisteredRequired').val("False").trigger('change');
//    } else {
//        $('#VATRegistrationNumber').hide();
//        $("#VatRegInfoRequired").val("False").trigger("change");

//        if ($('#HasBeenVatRegistered').val() == 'True') {
//            $('#DateVatDeRegisteredRequired').val("True").trigger('change');
//            $('#VATDeRegistration').show();
//        }
//    }
//}

//function OptOutSelfBillingChanged() {

//    if ($('#OptOutSelfBilling').is(':checked')) {
//        $('#SelfBilling').hide();
//    } else {
//        if ($('#AssociateDocumentWarning').val() == "False") {
//            $('#SelfBilling').show();
//            $("#associateWarningNotifications").show();
//        } else {
//            $('#SelfBilling').hide();
//            $("#associateWarningNotifications").hide();
//        }
//    }
//}

var globalEmailDisabledReasonFields;

function SetUpGlobalEmailDisabledReasonFields() {
    if (!vettingContactDefined) {
        viewModel.GlobalEmailDisabledReason("Cannot send reference until Vetting Contact selected and the page saved.");
        return;
    }

    globalEmailDisabledReasonFields = $("#FirstName, #LastName, #DateOfBirth, #NI");
    globalEmailDisabledReasonFields.each(function () {
        var $this = $(this);
        $this.data("initialVal", $this.val());
        $this.change(checkGlobalEmailDisabledReasonFields);
    });
}

function checkGlobalEmailDisabledReasonFields() {
    for (var i = 0; i < globalEmailDisabledReasonFields.length; i++) {
        var $g = $(globalEmailDisabledReasonFields[i]);
        if ($g.data("initialVal") != $g.val()) {
            viewModel.GlobalEmailDisabledReason($g[0].id + " edited. Save the page to enable the email link");
            return;
        }
    }
}

function ManualAddressEntry() {
    btn.nextAll("input[type='hidden']:first").val("");
    btn.nextAll("input[type='hidden']:first").trigger("change");

    btn.parent().parent().find(".ukAddress input[type='text']").each(function () {
        $(this).attr("value", "");
    });

    btn.parent().parent().find("select.CountyId").children("option:eq(0)").attr("selected", "selected");

    if (addressLookupMain) {
        MainAddressManualEntry();
    }

    var window = $("#AddressLookupDialog").data("kendoWindow");
    window.close();
}

function SelectAddress(li) {
    var xml = $(li).children("input[type='hidden']").val();
    btn.nextAll("input[type='hidden']:first").val(xml);
    btn.nextAll("input[type='hidden']:first").trigger("change");

    btn.parent().parent().find(".nonUkAddress textarea").text("");

    btn.parent().parent().find(".ukAddress input[type='text']:not(.pcl)").each(function () {
        $(this).attr("value", "");
    });

    btn.parent().parent().find("select.CountyId").children("option:eq(0)").attr("selected", "selected");
    var window = $("#AddressLookupDialog").data("kendoWindow");
    window.close();
}

var btn = null;

function AddressLookup(postCode, button) {
    if (postCode == null) {
        var evt = event.target;
        var target = evt ? evt : window.event.srcElement;
        var postCode = $.trim($(target).prevAll("input.postCodeLookup:first").val());
    }

    if (!postCode) {
        alert("Please enter a postcode.");
        return;
    }

    if (button == null)
        btn = $(target);
    else
        btn = button;

    $.ajax({
        type: 'POST',
        url: '/Vetting/AddressLookup',
        data: {
            postCode: postCode
        },
        success: function (result) {
            if (result != null) {
                viewModel.vettingAddresses(result);

                var window = $("#AddressLookupDialog").data("kendoWindow");
                window.center().open();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            // error message
            alert("Sorry, an error occurred checking the postcode, please check that it is correct");
        }
    });
}

function closeAddressLookupDialog() {
    var window = $("#AddressLookupDialog").data("kendoWindow");
    window.close();
}

//Conditional Validation

//If AvailableDate is visible it should have a value

function setAvailableDateVisibility(isVisible) {
    if (isVisible) {
        $(".available-date").show();
    } else {
        $(".available-date").hide();
    }
}

//If NoticePeriod is visible both NoticePeriod and NoticeIntervalId need a value

function setNoticePeriodVisibility(isVisible) {
    if (isVisible) {
        $(".notice-period").show();
    } else {
        $(".notice-period").hide();
    }
}

//If VisaType is visible both VisaType and VisaExpiry need a value

function setVisaVisibility(isVisible) {
    if (isVisible) {
        $("#VisaType").show();
    } else {
        $("#VisaType").hide();
    }
}

function ensureCustomValidationRulesAdded() {

    if (!$.validator.methods["ValidateBeforeCurrentDate"]) {
        // This validator checks that the date must be earlier than current date.
        // unless its '01/01/2999' which is equivalent to the present
        $.validator.addMethod("ValidateBeforeCurrentDate",
        function (value, element) {
            var dt = $(element).val();
            if (dt != '01/01/2999') {
                return (ConvertStringToDate(dt) <= GetCurrentDate());
            }
        },
        "Date must be earlier than today."
    );
    }

    if (!$.validator.methods["ValidateStartEndDate"]) {

        // Get the start and end dates for an item and makes
        // sure that the end date is greater than the start date.

        // Get the start and end dates for an item and makes
        // sure that the end date is greater than the start date.
        $.validator.addMethod("ValidateStartEndDate",
        function (value, element) {
            var dt = ConvertToLongDateUTC($(element).val());
            if (!dt) {
                return true;
            }

            var startDate, endDate;
            var fieldName = element.name.substr(0, element.name.indexOf("-"));

            startDate = $("input[name^='" + fieldName + "-Start']").val();

            // NOTE: If endDate is hidden (i.e. 'to present' is checked)
            // then it will be undefined.
            endDate = $("input[name^='" + fieldName + "-End']").val();
            if (endDate == null) {
                endDate = "01/01/2999";
            }

            if (endDate == "") {
                return true;
            }

            if (startDate && endDate) {
                return (ConvertToLongDateUTC(endDate) > ConvertToLongDateUTC(startDate));
            } else {
                return true;
            }
        },
        "End date must be greater than Starting date."
    );
    }

    if (!$.validator.methods["ValidateHouseNameOrNumberSupplied"]) {

        $.validator.addMethod("ValidateHouseNameOrNumberSupplied",
        function (value, element) {

            var fieldName = element.name.substr(0, element.name.indexOf("-"));

            var houseName = $("input[name^='" + fieldName + "-HouseName']").val();
            houseName = $.trim(houseName);

            var houseNumber = $("input[name^='" + fieldName + "-HouseNumber']").val();
            houseNumber = $.trim(houseNumber);

            //disable validation of this on Admin site as optional on portal
            return true;

            return !(houseNumber === "" && houseName === "");

        },
        "Either the house name or number is required."
    );
    }

    if (!$.validator.methods["ValidatePeriodCoverage"]) {

        $.validator.addMethod("ValidatePeriodCoverage",
        function (value, element, param) {
            var val = $(element).val();

            if (!val) {
                return true;
            }

            var dt = new Date(ConvertToLongDateUTC(val));
            if (dt) {

                var refEndDate = $("input[name^='" + param + "']").val();
                // NOTE: If refEndDate is hidden (i.e. 'to present' is checked)
                // then it will be undefined.
                if (refEndDate == null) {
                    refEndDate = new Date();
                } else {
                    refEndDate = new Date(ConvertToLongDateUTC(refEndDate));
                }

                if (refEndDate) {
                    return (dt > refEndDate.setFullYear(refEndDate.getFullYear() - 1));
                }
            }
        },
        "The reference must relate to the latest 12 months for the period of activity."
    );
    }
}

function setPrevNameVisibility(isVisible) {
    if (isVisible) {
        $("#PrevNameDiv").show();
    } else {
        $("#PrevNameDiv").hide();
    }
}

function setBusinessTypeVisibility() {
    var associateRegistrationTypeId = $('input:radio[name=AssociateRegistrationTypeId]:checked').val();

    if ((associateRegistrationTypeId === "2")) {
        $("#EmployedSection").show();
        $("#BusinessTypeId").val("");
        $('#UmbrellaCompanyId').val("");
        $(".businessType").hide();
        $("#BusinessTypeRequired").val("False").trigger("change");
    } else {
        $("#EmployedSection").hide();
        $(".businessType").show();
        $("#BusinessTypeRequired").val("True").trigger("change");
    }

    var businessTypeId = $('#BusinessTypeId').val();
    var umbrellaCompanyId = $('#UmbrellaCompanyId').val();
    var vatRegInfoRequired;

    $(".contractingDocumentsUmbrella").toggle(businessTypeId === "3");
    $(".contractingDocumentsLtdCo").toggle(businessTypeId === "1");
    $("#ContractingDocuments").toggle(businessTypeId === "1" || businessTypeId === "3");

    if ((businessTypeId === "3")) {
        $("#ContractingDocumentsCompanyType").text("umbrella");


        $("#UmbrellaList, #UmbrellaCompany").show();
        if (umbrellaCompanyId === "1") {
            $("#UmbrellaOther").show();
        } else {
            $("#UmbrellaOther").hide();
        }
    } else {
        $("#UmbrellaOther, #UmbrellaList, #UmbrellaCompany").hide();
        $('#UmbrellaCompanyId').val("");
    }

    if (businessTypeId === "1") {
        $("#ContractingDocumentsCompanyType").text("limited");
        $("#LtdCompanySection").show();

        vatRegInfoRequired = $("#VATRegistered").is(":checked") ? "True" : "False";
    } else {
        $("#LtdCompanySection").hide();
        vatRegInfoRequired = "False";
    }

    $("#VatRegInfoRequired").val(vatRegInfoRequired).trigger("change");
}

function LockAssociate() {
    $.ajax({
        type: 'Post',
        url: '/Associate/Lock',
        data: { id: $('#Id').val() },
        success: function (data) {
            if (data) {
                autoUnlocked = false;
                Readonly = false;
                $('#LockingUser').val(data);
                lockedMessage = null;
                ToggleLock();
            } else {
                alert("Sorry an error occurred trying to lock this record, please refresh the page and try again.");
            }
        }
    });
}

function UnlockAssociate(askConfirmation) {
    if (askConfirmation && !confirm("This record is locked by someone else. Are you sure you want to force their lock to be released? If they currently have the record open it will prevent them saving changes")) {
        return;
    }

    $.ajax({
        type: 'Post',
        url: '/Associate/Unlock',
        data: { id: $('#Id').val() },
        success: function (data) {
            if (data == "True") {
                Readonly = true;
                $('#LockingUser').val(null);
                lockedMessage = null;
                ToggleLock();
            } else {
                alert("Sorry an error occurred trying to unlock this record, please refresh the page and try again.");
            }
        }
    });
}

function ToggleLock() {
    ToggleReadOnly();
}

function ToggleReadOnly() {
    var $mainForm = $("#mainForm");
    var $ncf = $('#newCommunicationsForm');

    $ncf.find("textarea").attr("disabled", false);
    $ncf.find("select").attr("disabled", false);
    $ncf.find("input,button").attr("disabled", false);
    $("#EnterNewCommunicationsLink").show();

    $mainForm.find("textarea").not('[name^="PWRCheck."]').attr("disabled", false);
    $mainForm.find("select").attr("disabled", false);
    $mainForm.find("input,button").not('[name^="PWRCheck."]').not(".lockButtons").attr("disabled", false);
    $mainForm.find("[id$='ControlBox']").show();
    $mainForm.find("input[name='btnSubmit']").attr("style", "visibility:show;");
    $mainForm.find("#RefType").show();

    $mainForm.find("a.lockable").show();
    $mainForm.find("a.add, a.delete").show();
    $mainForm.find("textarea").not('[name^="PWRCheck."]').attr("disabled", false);
    $mainForm.find("select").attr("disabled", false);
    $mainForm.find("input,button").not('[name^="PWRCheck."]').not(".lockButtons").attr("disabled", false);
    $mainForm.find("[id$='ControlBox']").show();
    $mainForm.find(".scheduledChangeIcon").show();

    if ($(".postCodeLookup").siblings().find("select#CountryId").val() == 1)
        $(".postCodeLookup").show();

    $mainForm.find("input[name='btnSubmit']").attr("style", "visibility:show;");
    $mainForm.find(".deletedTemplateItem input,.deletedTemplateItem select,.deletedTemplateItem textarea").attr("disabled", true);
    $mainForm.find('a[id^="uploadLink"]').show();

    $(".submittedReference").find("input, textarea, select").not(".approved-checkbox").attr("disabled", true);

    $(".kgFooterPanel").find("input, textarea, select, button").attr("disabled", false);
}

$(document).ready(function () {
    // Configure error message placement for the knockout fields.
    DisableBusinessAreas();

    var validatorSettings = $("#mainForm").data("validator").settings;

    var baseErrorPlacement = validatorSettings.errorPlacement;
    validatorSettings.errorPlacement = function (error, element) {
        if (baseErrorPlacement) {
            baseErrorPlacement(error, element);
        }

        if (("_" + element.attr("name")).indexOf("_ko_") != -1) {
            //element.siblings("span").empty();
            //error.appendTo(element.siblings("span"));
            element.parents(".editor-field").find(".field-validation-error").empty();
            error.appendTo(element.parents(".editor-field").find(".field-validation-error"));
        }

    };

    validatorSettings.submitHandler = function (form) {

        var confirmationMessage;

        switch ($('#ActionNameButtonSource').val()) {
            case "Approve Associate":
                confirmationMessage = "Do you really wish to approve this associate?";
                break;
            case "Accept Associate":
                confirmationMessage = "Do you really wish to accept this associate?";
                break;
            case "Archive Associate":
                confirmationMessage = null;
                break;
            case "Save To ITRIS":
                confirmationMessage = "Do you really wish to save this associate to ITRIS?";
                break;
            default:
                confirmationMessage = "Do you really wish to update this record?";
                break;
        }

        if (confirmationMessage && !window.confirm(confirmationMessage)) {
            return;
        }


        viewModel.serializeJson();
        form.submit();
    };

    validatorSettings.ignore = ".dontValidate :input";

    $.validator.addMethod("notFreeEmail", function (value, element) {
        var result = !value.match(/@(gmail|googlemail|hotmail|outlook|yahoo)\./i);
        return result;
    }, "Please supply a business email address.");

    //ToggleLock();

    //Only allow numeric input to inputs of class "numeric"
    $(".numeric").keydown(function (event) {
        // Allow: backspace, delete, tab and escape
        if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 ||
            // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) ||
            // Allow: home, end, left, right
            (event.keyCode >= 35 && event.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        } else {
            // Ensure that it is a number and stop the keypress
            if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {

                event.preventDefault();
            }
        }
    });

    TitleChanged();

    var AvailabilityTypeEnum = {
        PermanentlyEmployed: 0,
        CurrentlyAvailable: 1,
        TemporaryContractWithNotice: 2
    };

    $("input[name=AvailabilityType]").change(function () {
        switch (parseInt($(this).val())) {
            case AvailabilityTypeEnum.PermanentlyEmployed:
                setAvailableDateVisibility(false);
                setNoticePeriodVisibility(true);
                break;
            case AvailabilityTypeEnum.CurrentlyAvailable:
                setAvailableDateVisibility(false);
                setNoticePeriodVisibility(false);
                break;
            case AvailabilityTypeEnum.TemporaryContractWithNotice:
                setAvailableDateVisibility(true);
                setNoticePeriodVisibility(true);
                break;
            default:
                setAvailableDateVisibility(false);
                setNoticePeriodVisibility(false);
                break;
        }
    });

    $("input[name=HasVisa]").change(function () {
        setVisaVisibility($(this).val() == "True");
    });

    $("input[name=HadPreviousName]").change(function () {
        setPrevNameVisibility($(this).val() == "True");
    });

    $("input[name=CountyCourtJudgements]").change(function () {
        $("#CountyCourtJudgementsAdditionalDetailsDiv").toggle($(this).val() == "True");
    });

    $("input[name=UnspentCriminalConvictions]").change(function () {
        $("#UnspentCriminalConvictionsAdditionalDetailsDiv").toggle($(this).val() == "True");
    });

    $("input[name=DeclaredBankrupt]").change(function () {
        $("#DeclaredBankruptAdditionalDetailsDiv").toggle($(this).val() == "True");
    });

    $('input[name=AssociateRegistrationTypeId], #UmbrellaCompanyId, #BusinessTypeId').change(function () {
        setBusinessTypeVisibility();
    });

    setBusinessTypeVisibility();

    $('a[name="AssociateIndexLink"]').each(function () {

        if ($('#fromHoldingBay').val() == "True" || location.href.indexOf("holdingBay") != -1) {
            $(this).attr('href', '/holding-bay');
        } else {
            var link = $(this).attr('href');
            var filterURL = sessionStorage.getItem("SearchFilter");
            $(this).attr('href', link + filterURL);
        }
    });

    ensureCustomValidationRulesAdded();

    ToggleLock();

    if ($('#NationalityId').val() == 27)
        $('input#EUNumber').attr('disabled', 'disabled');


    function invalidFormValidate(form, validator) {
        // Reset text for all tabs
        $("a[href=#tabs-1]")[0].innerText = "Associate";
        $("a[href=#tabs-2]")[0].innerText = "CV";
        if ($("a[href=#tabs-3]")[0]) {
            $("a[href=#tabs-3]")[0].innerText = "History";
        }

        var errors = validator.numberOfInvalids();

        var invalidTabs = [];

        if (errors) {
            $(validator.errorList).each(function () {
                var name = $($(this)[0].element).attr('name');
                if (name) {
                    var tabId = $("[name=" + name + "]").parents('div.ui-tabs-panel').attr('id');

                    if (tabId) {

                        if (!invalidTabs[tabId]) {
                            //Add tabid as an entry to the array
                            invalidTabs.push(tabId);

                            //Add first invalid element found as a property indexed by tabId
                            invalidTabs[tabId] = $(this)[0].element;
                        }

                        if ($("[name=" + name + "]").parents('div.panelEdit')[0]) {
                            $("[name=" + name + "]").parents('div.panelEdit').show();
                            $("[name=" + name + "]").parents('div.panelEdit').prev().find('div.rowView').hide();
                        }
                    }
                }
            });

            // Append "Incomplete" warning to all tabs with validation errors

            for (var j = 0; j < invalidTabs.length; j++) {
                var invalidTabId = invalidTabs[j];
                if ($("a[href=#" + invalidTabId + "]")[0].innerText.indexOf("Incomplete") == -1) {
                    $("a[href=#" + invalidTabId + "]").append(" - <span style='color: red'>Incomplete</span>");
                };
            }

            if (invalidTabs.length > 0) {
                var selectedTabIndex = $("#tabs").kendoTabStrip('option', 'selected');
                var selectedTabId = $('#tabs .ui-tabs-panel:eq(' + selectedTabIndex + ')').attr('id');

                if (!invalidTabs[selectedTabId]) {
                    selectedTabId = invalidTabs[0];
                    $("#tabs").kendoTabStrip("select", selectedTabId);
                }

                var $elementToFocus = $(invalidTabs[selectedTabId]);

                var $hiddenParents = $elementToFocus.parents(":hidden");

                if ($hiddenParents.length > 0) {
                    $hiddenParents.show();
                }

                $(window).scrollTop($elementToFocus.position().top);
                $elementToFocus.focus();
            } else {
                // scroll to top of page
                document.body.scrollTop = document.documentElement.scrollTop = 0;
            }
        }
    }

    // This is the only way to hook into the invalidHandler so that
    // action can be taken for each validation error.
    $("#mainForm").bind("invalid-form.validate", function (form, validator) {
        try {
            invalidFormValidate(form, validator);
        } catch (e) {
            /*An error in this method will prevent the JSON getting serialized otherwise*/
            viewModel.serializeJson();
            throw e;
        }
    });

    $("#archiveReasonOther").keyup(function () {
        $("#archiveReasonOther").removeAttr("disabled", "disabled");
        var reason = $("#archiveReasonOther").val();
        if (reason.length > 0) {
            $("#archiveTheAssociate").removeAttr("disabled", "disabled");
        }
        else {
            $("#archiveTheAssociate").attr("disabled", "disabled");
        }
    });

    $("input[name='Archive']").change(function () {
        $("#archiveTheAssociate").attr("disabled", "disabled");

        if (!$("input[name='Archive']").is(":checked")) {
            return;
        }

        var status = $("input[name='Archive']:checked").val();

        if (status == 20 || status == 10) {
            $("#archiveReasonOther").removeAttr("disabled", "disabled");

            var reason = $("#archiveReasonOther").val();
            if (reason.length > 0) {
                $("#archiveTheAssociate").removeAttr("disabled", "disabled");
            }
        }
        else {
            $("#archiveReasonOther").attr("disabled", "disabled");
            $("#archiveTheAssociate").removeAttr("disabled", "disabled");
        }
    });

    $("#ArchiveAssociate").click(function () {
        $("#archiveTheAssociate").attr("disabled", "disabled");
        $("#archiveReasonOther").attr("disabled", "disabled");

        $("input[name='Archive']").prop('checked', false);

        $("#archiveAssociateDialog").kendoWindow({
            visible: false,
            height: 540,
            width: 420,
            resizable: false,
            modal: true
        });

        var archiveWindow = $("#archiveAssociateDialog").data("kendoWindow");
        archiveWindow.center();
        archiveWindow.open();
    });

    $("#UnArchiveAssociate").click(function () {
        if (!window.confirm("Are you sure you want to un-archive this associate?")) {
            return;
        }

        var id = detailsSettings.AssociateId;

        $.ajax({
            type: 'POST',
            url: '/Associate/UnArchiveAssociate',
            data: {
                id: id
            },
            success: function (result) {
                location.reload();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                // error message
                alert("Sorry, an error occurred");
            }
        });
    });

    $("#FullAssociate").click(function () {
        if (!window.confirm("Are you sure that you want to convert this agency associate to a full associate?")) {
            return;
        }

        try {
            var variables = {
                FIRSTNAME: $("#FirstName").val(),
                ToAddress: $("#Email").val()
            };

            // show the email dialog
            SendAddHocEmail(72, variables, ChangeToFullAssociate);
        }
        catch (e) {
            alert(e.message);
        }
    });

    // By default cluetip will use the text in the 'rel' attribute in a jquery selector and
    // display the html inside the first match.
    $(".clickTip").cluetip({ local: true, showTitle: false });
});

function DisableBusinessAreas() {

    //bussiness type related values editable false   start---
    $('#SageId').attr('readonly', true);
    $('#DateVatDeRegistered').attr('readonly', true);
    $('#VATRegistered').attr('readonly', true);
    $('#RegisteredCompanyAddress').attr('readonly', true);
    $('#RegisteredCompanyName').attr('readonly', true);
    $('#LimitedCompanyNumber').attr('readonly', true);
    $('#RegistedCompanyBankAcctName').attr('readonly', true);
    $('#RegistedCompanyBankAcctSort').attr('readonly', true);
    $('#RegistedCompanyBankAcctNumber').attr('readonly', true);
    $('#OptOutSelfBilling').attr('readonly', true);
    $('#OtherUmbrellaCompanyName').attr('readonly', true);
    $('#OtherUmbrellaContactEmail').attr('readonly', true);
    $('#OtherUmbrellaContactName').attr('readonly', true);
    $('#VATRegistration').attr('readonly', true);
   
    $('#DateVatRegistered').attr('readonly', true);
    $('#BusinessTypeId').on("mousedown", function (e) {
        if (e.button == 0) {
            e.preventDefault();
        }
        e.preventDefault();
    });
    $("input[name='VATRegistered']").on("click", function (e) {
        e.preventDefault();
    });


    $("input[name='OptOutSelfBilling']").on("click", function (e) {
        e.preventDefault();
    });


   

    $('#UmbrellaCompanyId').on("mousedown", function (e) {
        if (e.button == 0) {
            e.preventDefault();
        }
        e.preventDefault();
    });

    //bussiness type related values editable false   end----
}
function ChangeToFullAssociate() {
    var id = detailsSettings.AssociateId;

    $.ajax({
        type: 'POST',
        url: '/Associate/ChangeToFullAssociate',
        data: {
            id: id,
            model: JSON.stringify(model)
        },
        success: function (result) {
            location.reload();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            // error message
            alert("Sorry, an error occurred");
        }
    });
}

function archiveAssociate() {
    var id = detailsSettings.AssociateId;

    var status = $("input[name='Archive']:checked").val();

    var reason = $("#archiveReasonOther").val();

    $.ajax({
        type: 'POST',
        url: '/Associate/ArchiveAssociate',
        data: {
            id: id,
            status: status,
            reason: reason
        },
        success: function (result) {
            location.reload();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            // error message
            alert("Sorry, an error occurred");
        }
    });

    var window = $("#archiveAssociateDialog").data("kendoWindow");
    window.close();
}
var EmailTemplate = {
    AssociateEmploymentReferencePermission: 33
}
var DocumentType = {
    CV: 1,
    AutoConvertedDeclarationForm: 2,
    ReferenceSupportingEvidence: 3,
    DeclarationForm: 4,
    UmbrellaConfirmation: 5,
    CertificateOfIncorporation: 6,
    Passport: 7,
    EuropeanIdCard: 8,
    BirthCertificate: 9,
    NICard: 10,
    DrivingLicense: 13,
    Visa: 14,
    Miscellaneous: 15,
    ProfessionalIndemnity: 17,
    ChangeOfName: 18,
    ReferenceAdminDocument: 19,
    VatCertificate: 20,
    Photo: 21,
    InsuranceQuote: 22,
    DeloitteSecurityAndConfidentiality: 23,
    FscsConfidentiality: 24,
    LbgConfidentiality: 25,
    HuntswoodConfidentiality: 26,
    NagDeclaration: 27,
    ProofOfAddress: 28,
    CreditCheck: 29,
    IdentityCheck: 30,
    BasicDisclosure: 31,
    SanctionsWorldcheck: 32,
    MediaCheck: 33,
    TimesheetReciept: 34,
    Invoice: 35,
    SelfBillingForm: 36,
    PIPLInsurance: 37,
    Assessment: 38,
    EmailAttachment: 39,
    PPIServicesLimited: 40,
    MomentaCustomerServicesLimited: 41,
    MomentaHoldingsPPILimited: 42,
    LbgConfidentialityHuntswood: 43,
    FormattedCV: 44

};

function ApproveDocument(guidId) {
    $.ajax({
        type: 'Post',
        url: '/Associate/ApproveDocument',
        data: { documentId: guidId },
        success: function (data) {
            if (data) {
                alert("This document has been successfully approved");
            } else {
                alert("Sorry a server error occurred while approving this document, please refresh the page and try again.");
            }
        },
        error: function () {
            alert("Sorry an error occurred approving this document, please refresh the page and try again.");
        }
    });
}

$(function () {
    $(".documentApprovedCheckBox").change(function () {
        var $this = $(this);

        var documentType = $this.data("document-type");

        var approvedDate = $this.is(":checked") ? JSON.parse(JSON.stringify(new Date())) : "";

        $("input[name='QualityCheckedDocs[" + documentType + "].ApprovedDate']").val(approvedDate);
    });
});

function toggleQualityCheckedHistory(documentTypeName) {
    $("#" + documentTypeName + "History").toggle();
    var $historyLink = $("#" + documentTypeName + "ToggleQualityCheckedHistory");
    if ($historyLink.text() == "Show older") {
        $historyLink.text("Hide older");
    } else {
        $historyLink.text("Show older");
    }
}

function ConvertToShortDate(dateString) {
    if (dateString) {

        if (dateString == "2999-01-01T00:00:00Z") {
            return "to present";
        }

        // This is to convert any yy into yyyy
        if (dateString.indexOf("-") === 2) {
            if (dateString.substring(0, 1) > 50) {
                dateString = "19" + dateString;
            } else {
                dateString = "20" + dateString;
            }
        }

        var date = new Date(dateString);

        if (isNaN(date.getTime())) {
            //IE8 doesn't cope with ISO dates
            date.setISO8601(dateString);
        }

        var yyyy = date.getFullYear();
        var dd = date.getDate();
        var mm = date.getMonth() + 1; //January is 0
        if (dd < 10) {
            dd = '0' + dd;
        }
        if (mm < 10) {
            mm = '0' + mm;
        }
        return dd + "/" + mm + "/" + yyyy;
    }
    else {
        return dateString;
    }
}

Date.prototype.setISO8601 = function (string) {
    var regexp = "([0-9]{4})(-([0-9]{2})(-([0-9]{2})" +
        "(T([0-9]{2}):([0-9]{2})(:([0-9]{2})(\.([0-9]+))?)?" +
        "(Z|(([-+])([0-9]{2}):([0-9]{2})))?)?)?)?";
    var d = string.match(new RegExp(regexp));

    var offset = 0;
    var date = new Date(d[1], 0, 1);

    if (d[3]) {
        date.setMonth(d[3] - 1);
    }
    if (d[5]) {
        date.setDate(d[5]);
    }
    if (d[7]) {
        date.setHours(d[7]);
    }
    if (d[8]) {
        date.setMinutes(d[8]);
    }
    if (d[10]) {
        date.setSeconds(d[10]);
    }
    if (d[12]) {
        date.setMilliseconds(Number("0." + d[12]) * 1000);
    }
    if (d[14]) {
        offset = (Number(d[16]) * 60) + Number(d[17]);
        offset *= ((d[15] == '-') ? 1 : -1);
    }

    offset -= date.getTimezoneOffset();
    var time = (Number(date) + (offset * 60 * 1000));
    this.setTime(Number(time));
};

function ConvertToFriendlyDateTime(dateString) {
    var date;

    if (dateString instanceof Date) {
        date = dateString;
    } else {
        date = new Date(dateString);
    }

    // dateString in format such as "2012-07-24T10:14:11.1608251Z"
    // Chrome deals with it fine. IE doesn't like that degree of precision.
    // Support for shorter ISO date formats is patchy too in IE. http://msdn.microsoft.com/en-us/library/ff743760(v=vs.94).aspx
    if (isNaN(date))
        date = new Date(Date.parse(dateString.substring(0, 23) + "Z"));

    var h = FormatTimePart(date.getHours());
    var m = FormatTimePart(date.getMinutes());
    return kendo.toString(date, 'dddd, MMMM dd, yyyy ') + h + ':' + m
}

function FormatTimePart(t) {
    t = String(t);
    return t.length == 1 ? '0' + t : t;
}

function ConvertStringToDate(dateString, showAlertOnError) {
    showAlertOnError = (showAlertOnError === undefined) || showAlertOnError;

    if (dateString === "") {
        return "";
    }

    if (!IsValidDate(dateString)) {
        if (showAlertOnError) {
            return "Error: Dates must be entered in the format of dd/mm/yyyy";
        }

        return "";
    }

    var arrDate = dateString.split("/");
    var yyyy = arrDate[2];
    var mm = arrDate[1];
    if (mm.length == 1) {
        mm = '0' + mm;
    }
    var dd = arrDate[0];
    if (dd.length == 1) {
        dd = '0' + dd;
    }

    return new Date(yyyy, mm - 1, dd);
}

function GetCurrentDate() {
    var today = new Date();
    var dd = today.getUTCDate();
    var mm = today.getUTCMonth(); //January is 0
    var yyyy = today.getUTCFullYear();

    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }

    return new Date(yyyy, mm, dd);
}

function ConvertToLongDateUTC(dateString) {
    if (dateString === "") {
        return "";
    }

    if (!IsValidDate(dateString)) {
        return "Error: Dates must be entered in the format of dd/mm/yyyy";
    }

    var arrDate = dateString.split("/");
    var yyyy = arrDate[2];
    var mm = arrDate[1];

    if (mm.length == 1) {
        mm = '0' + mm;
    }

    var dd = arrDate[0];
    if (dd.length == 1) {
        dd = '0' + dd;
    }

    if (yyyy && mm && dd) {
        // Date in the formate of "2012-04-11T00:00:00Z"
        return yyyy + "-" + mm + "-" + dd + "T00:00:00Z";
    } else {
        return "Error: Dates must be entered in the format of dd/mm/yyyy";
    }
}


function populateAssociateAssessments(varRefresh) {

    if (varRefresh == 1) {
        if ($('#associateAssessmentsGrid').data("kendoGrid").dataSource.page() != 1) {
            $('#associateAssessmentsGrid').data("kendoGrid").dataSource.page(1);
        }
        $('#associateAssessmentsGrid').data("kendoGrid").dataSource.read();
        return;
    }
    var url = "/Associate/GetAssessments";

    $("#associateAssessmentsGrid").kendoGrid({
        width: 900,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: url,
                    dataType: "json",
                    type: "POST",
                    data: { associateId: $("#Id").val() }
                }
            },
            schema: {
                model: {
                    fields: {
                        AssessmentId: { type: "number" },
                        AssociateId: { type: "number" },
                        AssessmentTypeId: { type: "number" },
                        AssessmentTypeName: { type: "string" },
                        AssessmentTypeDescription: { type: "string" },
                        AssessmentDate: { type: "date" },
                        Pass: { type: "string" },
                        Score: { type: "number" },
                        Scorable: { type: "bool" }
                    }
                }
            },
            pageSize: 5,
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            autoSync: false
        },
        scrollable: false,
        filterable: false,
        sortable: true,
        pageable: true,
        columns: [
            { field: "AssessmentTypeDescription", title: "Assessment Type", width: "120px" },
            { field: "AssessmentDate", title: "Assessment Date", width: "100px", template: "#= kendo.toString(kendo.parseDate(AssessmentDate, 'yyyy-MM-dd'), 'dd/MM/yyyy') #" },
            { field: "Pass", title: "Pass", width: "100px" },
            { field: "Score", title: "Score", width: "80px" },
            {
                field: "HasDetails", title: "Action", width: "50px", command: [
                  { name: "Details", text: " ", click: GetAssessmentTypeHistory },
                  { name: "Add", text: "", click: AddAssessmentInfo }

                ]
            }
        ],
        dataBound: function (e) {
            $("#associateAssessmentsGrid .k-grid-Details").kendoTooltip({
                width: 100,
                position: "top",
                content: "View Assessment Details"
            }).data("kendoTooltip");

            $("#associateAssessmentsGrid .k-grid-Add").kendoTooltip({
                width: 100,
                position: "top",
                content: "Edit Assessment"
            }).data("kendoTooltip");
        }
    });
}

function AddNewDocument(e) {
    try {
        var uid = $(e.currentTarget).closest("tr:parent").data("uid");
        var dataRow = $("#associateDocumentsGrid").data("kendoGrid").dataSource.getByUid(uid);
        AddDocument(dataRow, false)

    }


    catch (e) {
        //do nothing, not been called from grid, so e is undefined
        AddDocument(null, false);
    }
}

function AddDocument(dataRow, readonly) {

    var associateId = $("#Id").val();

    var guid = '';
    var documentTypeId = 0;
    if (dataRow != null) {
        guid = dataRow.DocGuid;
        documentTypeId = dataRow.DocumentType;
    }



    var window = $("#documentsWindow").load('/Associate/Documents?associateId=' + associateId, function () {
        if (guid == '') {
            InitialHideUploader();
            $("#documentTypeId").val(0);
            $("#documentId").val(0);
            $("#existingDocumentTypeId").val(0);

            //populate dropdown
            var ds = GetDocumentTypes();
            $("#DocumentType").kendoDropDownList({
                dataSource: ds,
                dataTextField: "Description",
                dataValueField: "DocumentTypeId",
                index: 0,
                dataBound: resizeDropDown,
                change: onChangeDocType
            });

            if ($("#DocumentType").data("kendoDropDownList").text() == "CV") {
                $.ajax({
                    type: 'POST',
                    url: "/Document/GetCVGUID",
                    data: { id: $("#Id").val() },
                    success: function (result) {
                        $("#cvGUID").val(result);
                    }
                });
            }

            $("#DocumentDate").kendoDatePicker({
                culture: "en-GB",
                format: "dd/MM/yyyy",
                max: new Date()
            });

            $("#DocumentDate").val("");

            window.data("kendoWindow").title("Add Document");
            $("#btnAssociateDocumentArchive").hide();
        }
        else {
            window.data("kendoWindow").title("Edit Document");
            $("#secondaryForm").show();
            $("#FullSave").val("true");
            $("#DocumentDate").kendoDatePicker({
                culture: "en-GB",
                format: "dd/MM/yyyy",
                max: new Date()
            });

            var varDocDate = new Date(dataRow.Dated);
            if (dataRow.Dated == null) {
                $("#DocumentDate").val("");
            }
            else {
                $("#DocumentDate").val(varDocDate.getDate() + "/" + (eval(varDocDate.getMonth()) + 1) + "/" + varDocDate.getFullYear());
            }
            $("#uploadePopUpType").val("Edit");
            $("#DocumentType").text(GetDocumentTypeName(dataRow.DocumentType));
            $("#documentId").val(dataRow.DocGuid);
            $("#existingDocumentTypeId").val(documentTypeId);
            ExistingHideUploader(documentTypeId);
            $("#DocumentComments").val(dataRow.EditableComment);
            $("#DocumentQC").prop("checked", dataRow.QualityChecked);
            $("#DocumentIssues").prop("checked", dataRow.Issues);

            if (readonly) {
                $("#DocumentDate").attr('disabled', 'disabled');
                $("#DocumentQC").attr('disabled', 'disabled');
                $("#DocumentIssues").attr('disabled', 'disabled');
                var dropDown = $("#DocumentType").data("kendoDropDownList");
                dropDown.enable(false);
                $("#DocumentComments").attr('disabled', 'disabled');
                $("#DocumentUpload").css('display', 'none');
                $("#documentButtons").css('display', 'none');

            }
            //LoadAssessmentHistoryGrid(associateId, assessmentId);
            //LoadAssessmentDocsGrid(associateId, assessmentId);

        }

    });


    window.kendoWindow({
        width: "750px",
        close: onCloseDocumentWindow,
        actions: ["Close"],
        modal: true,
        visible: false
    });


    window.data("kendoWindow").center().open();
}

var resizeDropDown = function (e) {
    var $dropDown = $(e.sender.element),
        dataWidth = $dropDown.data("kendoDropDownList").list.width(),
        listWidth = dataWidth + 20,
        containerWidth = listWidth + 6;

    // Set widths to the new values
    $dropDown.data("kendoDropDownList").list.width(listWidth);
    $dropDown.closest(".k-widget").width(containerWidth);
}

function AddAssessmentInfo(e) {
    try {
        var uid = $(e.currentTarget).closest("tr:parent").data("uid");
        var dataRow = $("#associateAssessmentsGrid").data("kendoGrid").dataSource.getByUid(uid);
        AddAssessment(dataRow, false)

    }


    catch (e) {
        //do nothing, not been called from grid, so e is undefined
        AddAssessment(null, false);
    }
}

function ViewAssessmentInfo(e) {
    try {
        var uid = $(e.currentTarget).closest("tr:parent").data("uid");
        var dataRow = $("#assessmentHistoryGrid").data("kendoGrid").dataSource.getByUid(uid);
        AddAssessment(dataRow, true);



    }


    catch (e) {
        //do nothing, not been called from grid, so e is undefined
    }
}

function AddAssessment(dataRow, readonly) {

    var associateId = $("#Id").val();

    var assessmentTypeId = 0;
    var assessmentId = 0;
    if (dataRow != null) {
        assessmentTypeId = dataRow.AssessmentTypeId;
        assessmentId = dataRow.AssessmentId;
    }
    var window = $("#assessmentsWindow").load('/Associate/Assessments', function () {
        //populate fields



        var data = [
                    { text: "Pass", value: "Pass" },
                    { text: "Borderline", value: "Borderline" },
                    { text: "Fail", value: "Fail" }
        ];
        $("#AssessmentPass").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: data,
            index: 0
        });

        if (assessmentTypeId == 0) {

            $("#existingAssessmentTypeId").val(0);
            $("#assessmentId").val(0);
            //ale
            var ds = GetAssessmentTypes();


            $("#AssessmentType").kendoDropDownList({
                dataSource: ds,
                dataTextField: "Description",
                dataValueField: "AssessmentTypeId",
                index: 1,
                change: onChange
            });

            $("#AssessmentDate").kendoDatePicker({
                culture: "en-GB",
                format: "dd/MM/yyyy",
                max: new Date()
            });
            $("#assessmentHistory").css('display', 'none');
            LoadAssessmentDocsGrid(associateId, assessmentId);
            window.data("kendoWindow").title("Add Assessment");
            $("#btnAssociateAssessmentArchive").hide();
        }
        else {
            window.data("kendoWindow").title("Edit Assessment");
            var varAssDate = new Date(dataRow.AssessmentDate);

            $("#AssessmentType").text(dataRow.AssessmentTypeDescription);
            $("#assessmentId").val(dataRow.AssessmentId);
            $("#existingAssessmentTypeId").val(assessmentTypeId);

            $("#AssessmentScore").val(dataRow.Score);

            $("#AssessmentPass").data("kendoDropDownList").select(function (dataItem) {
                return dataItem.value === dataRow.Pass;
            });
            $("#AssessmentDate").val(varAssDate.getDate() + "/" + (eval(varAssDate.getMonth()) + 1) + "/" + varAssDate.getFullYear());
            $("#AssessmentDate").attr('disabled', 'disabled');
            $("#AssessmentDate").addClass("k-textbox");
            $("#AssessmentComments").val(dataRow.Comment);
            if (readonly) {
                $("#AssessmentScore").attr('disabled', 'disabled');
                var dropDown = $("#AssessmentPass").data("kendoDropDownList");
                dropDown.enable(false);
                $("#AssessmentComments").attr('disabled', 'disabled');
                $("#assessmentDocumentUpload").css('display', 'none');
                $("#assessmentButtons").css('display', 'none');
                window.data("kendoWindow").title("View Assessment");

            }
            LoadAssessmentHistoryGrid(associateId, assessmentId);
            LoadAssessmentDocsGrid(associateId, assessmentId);

        }

    });


    if (!window.data("kendoWindow")) {
        window.kendoWindow({
            width: "850px",
            actions: ["Close"],
            modal: true,
            visible: false
        });
        window.data("kendoWindow").center().open();

    }
    else {
        // reopening window
        window.data("kendoWindow")
        .refresh() // request the URL via AJAX
        .center().open() // open the window;

    }
    event.preventDefault();
}
function onChange() {
    var associateId = $("#Id").val();
    var assessmentTypeId = $("#AssessmentType").data("kendoDropDownList").value();

    if (assessmentTypeId != 11 && assessmentTypeId != 13 && assessmentTypeId != 14) {
        var data = [
                     { text: "Pass", value: "Pass" },
                     { text: "Borderline", value: "Borderline" },
                     { text: "Fail", value: "Fail" }
        ];

        $("#AssessmentPass").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: data,
            index: 0
        });
    }
    else {
        var data1 = [
                        { text: "Pass", value: "Pass" },
                        { text: "Fail", value: "Fail" }
        ];
        $("#AssessmentPass").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: data1,
            index: 0
        });
    }

    LoadAssessmentHistoryGrid(associateId, assessmentId)

};
function onChangeDocType() {
    HideUploader();
    var DocumentTypeId = $("#DocumentType").data("kendoDropDownList").value();
    var DocumentTypeText = $("#DocumentType").data("kendoDropDownList").text();
    var uploaderGenericId = "#docDownloadType_";
    var uploaderViewId = "#viewDocumentLink";
    var uploaderId = uploaderGenericId.concat(DocumentTypeId);
    var downloaderId = uploaderViewId.concat(DocumentTypeText);
    $(uploaderId).show();
    $(downloaderId).hide();

    if (DocumentTypeText == "CV")
    {
        $.ajax({
            type: 'POST',
            url: "/Document/GetCVGUID",
            data: { id: $("#Id").val() },
            success: function (result) {
                $("#cvGUID").val(result);
        }
        });
    }
};

function onChangeBusType() {
    var busType = $("#ScheduledBusinessTypeId").data("kendoDropDownList").value();
    if (busType == 2) {
        $("#ScheduledUmbrellaOther").hide();
        $("#ScheduledUmbrellaList").hide();
        $("#ScheduledLtdCompanySection").hide();
    }
    if (busType == 3) {
        $("#ScheduledUmbrellaList").show();
        $("#ScheduledLtdCompanySection").hide();
    } else if (busType == 1)
    {
        $("#ScheduledUmbrellaList").hide();
        $("#ScheduledLtdCompanySection").show();
    }
}

function onChangeBusDate() {
    $("#HiddenScheduledDate").val($("#BusinessTypeChange .ScheduledDate").val());
}

function InitialHideUploader() {
    $(".docDownloadType_Container:not(:first)").hide();
    $(".docViewers").hide();
}

function ExistingHideUploader(ind) {
    $(".docDownloadType_Container").hide();
    $("#docDownloadType_" + ind).show();
    $(".docViewers").hide();
}

function HideUploader() {
    $(".docDownloadType_Container").hide();
    $(".docViewers").hide();
}
function GetAssessmentTypes() {
    return new kendo.data.DataSource({
        type: "json",
        transport: {
            read: {
                url: "/Associate/GetAssessmentTypes",
                dataType: "json",
                type: "POST"
            }
        },
        schema: {
            model: {
                fields: {
                    AssessmentTypeId: { type: "number" },
                    Name: { type: "string" },
                    Description: { type: "string" },
                    Scorable: { type: "bool" }
                }
            }
        }
    });
}

function GetDocumentTypes() {
    return new kendo.data.DataSource({
        type: "json",
        transport: {
            read: {
                url: "/Associate/GetDocumentTypes",
                dataType: "json",
                type: "POST"
            }
        },
        schema: {
            model: {
                fields: {
                    DocumentTypeId: { type: "number" },
                    Description: { type: "string" },
                }
            }
        }
    });
}

function closeTaskWindow() {
    $("#BusinessTypeChangeWindow").data("kendoWindow").close();
}

function closeDocumentWindow() {
    deleteRedundantDocument();
    $('#associateDocumentsGrid').data('kendoGrid').dataSource.read();
    //$('#associateDocumentsGrid').data('kendoGrid').refresh();
    var uploaderController = $("#DocumentUploader");
    $("#documentsWindow").data("kendoWindow").close();
    $("#documentsWindow").html("");
    $("#mainForm").append(uploaderController);
}

function onCloseDocumentWindow() {
    deleteRedundantDocument();
    $("#uploadePopUpType").val("");
    $('#associateDocumentsGrid').data('kendoGrid').dataSource.read();
    //$('#associateDocumentsGrid').data('kendoGrid').refresh();
    var uploaderController = $("#DocumentUploader");
    $("#documentsWindow").html("");
    $("#mainForm").append(uploaderController);
}

function deleteRedundantDocument() {
    //hiddem field data
    var fileUploaded = $("#lastUploadGuid").prop("isUnSaved");
    var redundantDocId = $("#lastUploadGuid").val();
    if (fileUploaded === true) {
        $.ajax({
            type: 'POST',
            url: "/Document/DeleteDocument",
            data: { id: redundantDocId },
            success: function (result) {
            }
        });
        $("#lastUploadGuid").val("");
        $("#lastUploadGuid").prop("isUnSaved", false);
    }
}
function saveDocument() {
    $.ajax({
        type: 'POST',
        url: "/Document/GetCVGUID",
        data: { id: $("#Id").val() },
        success: function (result) {
    $("#cvGUID").val(result);
    if ($("#FullSave").val() == "true") {
        //Update/Edit
        var documentId = $("#documentId").val();
        var url = "/Associate/CreateDocument";
        var documentTypeId;

        if (documentId == "" || documentId == "0") {
            //create new
            documentTypeId = $("#DocumentType").data("kendoDropDownList").value();
            documentId = $("#lastUploadGuid").val();

            if ($("#DocumentType").data("kendoDropDownList").text() == "CV") {

                documentId = $("#cvGUID").val();
            }
        }
        else {
            //update existing
            documentTypeId = $("#existingDocumentTypeId").val();
        }

        var docComment = "";
        if ($("#DocumentComments").val() != "") {
            docComment = $("#DocumentComments").val();
        }

        //Test Data
        /*var commentData = {      
            CreatedTime: $("#DocumentDate").val(),
            User: 'TestName',
            Comment: docComment
        };
        var comList = [commentData];
        docComment = comList;*/

        var data = {
            DocumentId: documentId,
            AssociateId: $("#Id").val(),
            DocGuid: documentId,
            DocumentType: documentTypeId,
            Dated: $("#DocumentDate").val(),
            CurrentComment: docComment
        };

        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                if (result === true) {
                    $("#lastUploadGuid").val("");
                    $("#lastUploadGuid").prop("isUnSaved", false);
                    closeDocumentWindow();
                    $('#associateDocumentsGrid').data('kendoGrid').dataSource.read();
                }
                else {
                    Alert(result, "Add Document");
                }
            }
        });
    }
    else {
        //create requirement
        var documentTypeId = $("#DocumentType").data("kendoDropDownList").value();
        $.ajax({
            type: 'POST',
            url: "/Associate/CreateAssociateDocumentRequirment",
            data: { associateId: $("#Id").val(), docTypeId: documentTypeId },
            success: function (result) {
                if (result === true) {
                    $("#lastUploadGuid").val("");
                    $("#lastUploadGuid").prop("isUnSaved", false);
                    closeDocumentWindow();
                    $('#associateDocumentsGrid').data('kendoGrid').dataSource.read();
                }
                else {
                    Alert(result, "Add Document");
                }
            }
        });
    }
         
    }
});
}


function closeAssessmentWindow() {
    $("#assessmentsWindow").data("kendoWindow").close();
    $("#assessmentsWindow").html("");
}

function saveAssessment() {

    if ($("#AssessmentDate").val() == "") {
        $("#DateAssessmentError").css('display', 'block');
        return;
    }
    else {
        $("#DateAssessmentError").css('display', 'none');
    }

    var assessmentId = $("#assessmentId").val();
    var url = "/Associate/CreateAssessment";
    var assessmentTypeId;

    if (assessmentId == "" || assessmentId == "0") {
        //create new
        assessmentTypeId = $("#AssessmentType").data("kendoDropDownList").value();
    }
    else {
        //update existing
        assessmentTypeId = $("#existingAssessmentTypeId").val();
    }

    var asscomment = "";
    if ($("#AssessmentComments").val() != "")
        asscomment = $("#AssessmentComments").val();
    if (($("#AssessmentScore").val() == "") && (assessmentTypeId != 11 && assessmentTypeId != 13 && assessmentTypeId != 14)) {
        $("#AssessmentScoreError").css('visibility', 'visible');
        return;
    }
    else {
        $("#AssessmentScoreError").css('visibility', 'hidden');
    }
    var varDocumentIds = "";
    if ($("#hdnNewDocuments").val().length > 0)
        varDocumentIds = $("#hdnNewDocuments").val();
    var data = {
        AssessmentId: assessmentId,
        AssessmentTypeId: assessmentTypeId,
        AssessmentTypeName: "",
        AssessmentTypeDescription: "",
        Pass: $("#AssessmentPass").data("kendoDropDownList").value(),
        Score: $("#AssessmentScore").val(),
        AssessmentDate: $("#AssessmentDate").val(),
        Scorable: false,
        AssociateId: $("#Id").val(),
        Comment: asscomment,
        Documents: varDocumentIds
    };

    $.ajax({
        type: 'POST',
        url: url,
        data: data,
        success: function (result) {
            if (result === true) {
                closeAssessmentWindow();
                populateAssociateAssessments(1);
            }
            else {
                Alert(result, "Add Assessment");
            }
        }
    });
}

function archiveDeleteDocument() {
    if (!confirm("Are you sure you want to delete this document?"))
        return;
    var documentId = $("#documentId").val();


    $.ajax({
        type: 'POST',
        url: "/Document/DeleteDocument",
        data: { id: $("#documentId").val() },
        success: function (result) {
            if (result === true) {
                closeDocumentWindow();
                associateDocumentGridInitialised = false;
                $('#associateDocumentsGrid').data('kendoGrid').dataSource.read();
            }
            else {
                Alert(result, "Delete Document");
            }
        }
    });
}

function deleteAssessment() {
    // $("#DateAssessmentError").css('visibility', 'hidden');
    // $("#AssessmentScoreError").css('visibility', 'hidden');

    if (!confirm("Are you sure you want to delete this assessment?"))
        return;
    var assessmentId = $("#assessmentId").val();


    $.ajax({
        type: 'POST',
        url: "/Assessment/DeleteAssessment",
        data: { assessmentId: $("#assessmentId").val() },
        success: function (result) {
            if (result === true) {
                closeAssessmentWindow();
                populateAssociateAssessments(1);
            }
            else {
                Alert(result, "Delete Assessment");
            }
        }
    });
}
function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}

function IsValidDate(dateString) {
    var dateParts = dateString.split('/');
    if (dateParts.length == 3) {
        try {
            var d = new Date(dateParts[2], dateParts[1], dateParts[0]);
            return !isNaN(d.getTime());
        } catch (ex) {

        }
    }
    return false;
}
function EmployeeReferenceRequest(associateid) {

    SendEmail(associateid, 0, false, EmailTemplate.AssociateEmploymentReferencePermission, "", "", "");
    
}
function initBusinessAreas() {

    var businessAreas = $("#businessAreas").data("kendoMultiSelect");

    if (typeof businessAreas == 'undefined') {

        businessAreas = $('#businessAreas').kendoMultiSelect({
            placeholder: "Select business areas...",
            autoBind: false,
            dataValueField: "Value",
            dataTextField: "Text",
            change: function (e) {
                $("#selectedBusinessAreas").val(e.sender.value());
            }
        }).data("kendoMultiSelect");

    }

    $.ajax({
        type: 'POST',
        url: '/Associate/GetBusinessAreas',
        async: false,
        data: {
            associateId: $("#Id").val()
        },
        success: function (result) {
            if (result) {
                businessAreas.dataSource.data(result.data);
                businessAreas.value(result.value);

            } else {
                Alert("An error occurred attempting to retrieve business area data.");
            }
        },
        error: function () {
            Alert("An error occurred attempting to retrieve business area data.");
        }
    });

}


function DeleteAssessmentDocument(documentId) {


    if (!confirm("Are you sure you want to delete this document?"))
        return;

    $.ajax({
        type: 'POST',
        url: '/Document/DeleteDocument',
        data: {
            id: documentId
        },
        success: function (result) {
            var varNewlyUploadedDocs = $("#hdnNewDocuments").val();
            $("#hdnNewDocuments").val(varNewlyUploadedDocs.replace("," + documentId, "").replace(documentId + ",", ""));
            if (result) {

                if ($('#assessmentDocumentsGrid').data("kendoGrid").dataSource.page() != 1) {
                    $('#assessmentDocumentsGrid').data("kendoGrid").dataSource.page(1);
                }
                $('#assessmentDocumentsGrid').data("kendoGrid").dataSource.read({ uploadedDocuments: $("#hdnNewDocuments").val() });

                return;
            }
            alert("Failed to delete this document.");
        },
        error: function () {
            alert("Failed to  delete this document.");
        }
    });

}

function ReloadAssessmentDocumentsGrid(varFileId) {

    var documentTitle = $.trim($("#AssessementDocumentTitle").val());
    if ($("#hdnNewDocuments").val() == "") {

        $("#hdnNewDocuments").val(varFileId);

    }
    else {
        $("#hdnNewDocuments").val($("#hdnNewDocuments").val() + "," + varFileId);
    }

    if ($('#assessmentDocumentsGrid').data("kendoGrid").dataSource.page() != 1) {
        $('#assessmentDocumentsGrid').data("kendoGrid").dataSource.page(1);
    }
    $('#assessmentDocumentsGrid').data("kendoGrid").dataSource.read({ uploadedDocuments: $("#hdnNewDocuments").val() });
    $("#AssessementDocumentTitle").val("");
}

function LoadAssessmentTypeHistoryGrid(varAssociateId, varAssessmentTypeId) {

    $("#assessmentHistoryGrid").kendoGrid({
        width: 900,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/Assessment/GetAssessmentTypeHistory",
                    data: {
                        associateId: varAssociateId,
                        assessmentTypeId: varAssessmentTypeId
                    },
                    dataType: "json",
                    type: "POST"
                }
            },
            schema: {
                model: {
                    fields: {
                        AssessmentId: { type: "number" },
                        AssociateId: { type: "number" },
                        AssessmentTypeId: { type: "number" },
                        AssessmentTypeName: { type: "string" },
                        AssessmentTypeDescription: { type: "string" },
                        AssessmentDate: { type: "date" },
                        Pass: { type: "string" },
                        Score: { type: "number" },
                        Scorable: { type: "bool" },
                        Comment: { type: "string" }
                    }
                }
            },
            pageSize: 5,
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            autoSync: false
        },
        scrollable: false,
        filterable: false,
        sortable: true,
        pageable: true,
        columns: [
            { field: "AssessmentDate", title: "Assessment Date", width: "100px", template: "#= kendo.toString(kendo.parseDate(AssessmentDate, 'yyyy-MM-dd'), 'dd/MM/yyyy') #" },
            { field: "Pass", title: "Pass", width: "100px" },
            { field: "Score", title: "Score", width: "80px" },
            {
                title: " ", width: "50px", command: [
                  { name: "Details", text: " ", click: ViewAssessmentInfo }

                ]
            }
        ]
    });


}

function LoadAssessmentHistoryGrid(varAssociateId, varAssessmentId) {

    $("#assessmentHistoryGrid").kendoGrid({
        width: 900,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/Assessment/GetAssessmentHistory",
                    data: {
                        associateId: varAssociateId,
                        assessmentId: varAssessmentId
                    },
                    dataType: "json",
                    type: "POST"
                }
            },
            schema: {
                model: {
                    fields: {
                        AssessmentId: { type: "number" },
                        AssociateId: { type: "number" },
                        AssessmentTypeId: { type: "number" },
                        AssessmentTypeName: { type: "string" },
                        AssessmentTypeDescription: { type: "string" },
                        AssessmentDate: { type: "date" },
                        Pass: { type: "string" },
                        Score: { type: "number" },
                        Scorable: { type: "bool" },
                        Date: { type: "date" },
                        Comment: { type: "string" }
                    }
                }
            },
            pageSize: 5,
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            autoSync: false
        },
        scrollable: false,
        filterable: false,
        sortable: true,
        pageable: true,
        columns: [
            { field: "Date", title: "DateTime", width: "100px", template: "#= kendo.toString(kendo.parseDate(Date, 'yyyy-MM-dd HH:mm:ss'), 'dd/MM/yyyy HH:mm:ss') #" },

            { field: "LoggedInUser", title: "Username", width: "100px" },
            { field: "Action_Comments", title: "Comments", width: "80px", template: GetActionComments }
        ]
    });


}

function GetActionComments(container, options) {

    var comments = "";

    if (container.Comment == "") {
        if (container.changed_Pass) {
            comments = "pass";
        }
        if (container.changed_Score) {
            if (comments != "")
                comments = comments + ",score";
            else
                comments = "score";
        }
        if (container.changed_Comment) {
            if (comments != "")
                comments = comments + ",comment";
            else
                comments = "comment";
        }

        if (comments != "") {
            comments = comments + " has been changed"
            if (container.Document_Changes)
                comments = comments + " & " + container.Document_Changes;
        }
        else {
            if (container.Document_Changes)
                comments = container.Document_Changes;
        }
    }
    else
        comments = container.Comment;

    return comments;
}
function GetAssessmentDocsColumns() {

    if (inAssessmentAdministratorRole == true) {

        return [
            { field: "Title", title: "Title", width: "100px" },
            { title: "", template: "<a href='/Associate/DownloadDocument/#= DocumentId #'>View<a>", width: "40px" },
            { title: "", width: "30px", template: kendo.template($("#assessmentdocument-delete-template").html().replace("varDocFileId", "'#= DocumentId #'")) }
        ]
    }
    else {

        return [
            { field: "Title", title: "Title", width: "100px" },
            { field: "DocumentId", template: "<a href='/Associate/DownloadDocument/#= DocumentId #'>View<a>" }
        ]
    }
}

function LoadAssessmentDocsGrid(varAssociateId, varAssessmentId) {

    var assessmentDocsColumns = GetAssessmentDocsColumns();

    $("#assessmentDocumentsGrid").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/Assessment/GetAssessmentDocuments",
                    data: {
                        associateId: varAssociateId,
                        assessmentId: varAssessmentId,
                        uploadedDocuments: $("#hdnNewDocuments").val()
                    },
                    dataType: "json",
                    type: "POST"
                }
            },
            schema: {
                model: {
                    fields: {
                        AssessmentId: { type: "number" },
                        AssociateId: { type: "number" },
                        AssessmentTypeId: { type: "number" },
                        AssessmentTypeName: { type: "string" },
                        AssessmentTypeDescription: { type: "string" },
                        AssessmentDate: { type: "date" },
                        Pass: { type: "string" },
                        Score: { type: "number" },
                        Scorable: { type: "bool" }
                    }
                }
            },
            pageSize: 2,
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            autoSync: false
        },
        scrollable: false,
        filterable: false,
        sortable: true,
        pageable: true,
        columns: assessmentDocsColumns
    });
}

function GetAssessmentTypeHistory(e) {

    var uid = $(e.currentTarget).closest("tr:parent").data("uid");
    var dataRow = $("#associateAssessmentsGrid").data("kendoGrid").dataSource.getByUid(uid);
    var assessmentTypeId = dataRow.AssessmentTypeId;


    var associateId = $("#Id").val();


    //open window
    var window = $("#assessmentsWindow").load('/Associate/Assessments #assessmentHistory', function () {
        LoadAssessmentTypeHistoryGrid(associateId, assessmentTypeId);

        window.kendoWindow({
            width: "700px",
            actions: ["Close"],
            modal: true
        });

        window.data("kendoWindow").title(dataRow.AssessmentTypeDescription);
        window.data("kendoWindow").center().open();

    });

}

function ToggleAssessmentHistory(e) {
    var $container = $(e).parent().parent();
    var $grid = $container.find('.grid-container');

    $grid.toggle();

    if ($grid.is(':visible')) {
        $(e).find('img').attr('src', '/Content/images/collapse.gif');
    } else {
        $(e).find('img').attr('src', '/Content/images/expand.png');
    }

    return false; // Stop default behaviour for href="#" which scrolls to top of page
}

function GetAssessmentModel() {

    var sm = {
        // free text
        FreeText: $("#FreeText").val().length == 0 ? null : $("#FreeText").val(),

        // personal information
        FirstName: $("#FirstName").val().length == 0 ? null : $("#FirstName").val(),
        LastName: $("#LastName").val().length == 0 ? null : $("#LastName").val(),
        AssociateId: $("#AssociateId").val().length == 0 ? null : $("#AssociateId").val(),
        EmailAddress: $("#EmailAddress").val().length == 0 ? null : $("#EmailAddress").val(),
        PhoneNumber: $("#PhoneNumber").val().length == 0 ? null : $("#PhoneNumber").val(),
        ApprovalStatusId: $("#ApprovalStatusId").val() == "-1" ? null : $("#ApprovalStatusId").val(),
        AgencyAssociate: $("#AgencyAssociate").is(":checked") ? true : null,
        FullAssociate: $("#FullAssociate").is(":checked") ? true : null,
        EmployedAssociate: $("#EmployedAssociate").is(":checked") ? true : null,
        InterimManagementAssociate: $("#InterimManagementAssociate").is(":checked") ? true : null,
        CV: $("#CV").val().length == 0 ? null : $("#CV").val(),
        BusinessUnitId: $("#BusinessUnitId").data("kendoMultiSelect").value(),
        BusinessAreaId: $("#BusinessAreaId").data("kendoMultiSelect").value(),

        // availability
        AvailableFrom: $("#AvailableFrom").val().length == 0 ? null : $("#AvailableFrom").val(),
        AvailableTo: $("#AvailableTo").val().length == 0 ? null : $("#AvailableTo").val(),
        NoticePeriod: $("#NoticePeriod").val().length == 0 ? null : $("#NoticePeriod").val(),
        NoticeIntervalId: $("#NoticeIntervalId").val() == "-1" ? null : $("#NoticeIntervalId").val(),

        // rate
        RateFrom: $("#RateFrom").val().length == 0 || $("#RateFrom").val() == 0 ? null : $("#RateFrom").val(),
        RateTo: $("#RateTo").val().length == 0 || $("#RateTo").val() == 0 ? null : $("#RateTo").val(),

        // location
        Postcode: $("#Postcode").val().length == 0 ? null : $("#Postcode").val(),
        Miles: $("#Miles").val().length == 0 || $("#Miles").val() == 0 ? null : $("#Miles").val(),

        // qualifications
        Qualifications: $("#QualificationsPanelQualificationsDiv :checked").map(function () { return $(this).val(); }).get(),

        // qualifications other
        QualificationsOther: GetQualificationsOther(),

        // assessments
        Assessments: GetAssessments(),

        // project - client
        ClientId: $("#ClientId").val() == "-1" ? null : $("#ClientId").val(),

        // project - project
        ProjectId: $("#ProjectId").val() == "-1" ? null : $("#ProjectId").val(),

        // project - requirement
        RequirementId: $("#RequirementId").val() == "-1" ? null : $("#RequirementId").val(),

        // project - role
        RoleId: $("#RoleId").val() == "-1" ? null : $("#RoleId").val(),

        // visa 
        VisaTypeId: $("#VisaTypeId").val() == "-1" ? null : $("#VisaTypeId").val(),
        VisaExpiryStart: $("#VisaExpiryStart").val().length == 0 ? null : $("#VisaExpiryStart").val(),
        VisaExpiryEnd: $("#VisaExpiryEnd").val().length == 0 ? null : $("#VisaExpiryEnd").val()
    };

    return sm;
}

function ScheduledVATRegisteredChanged() {
    alert($('#ScheduledVATRegistered').is(':checked'));
    if ($('#ScheduledVATRegistered').is(':checked')) {
        $('#ScheduledVATRegistrationNumber').show();
        $('#ScheduledVATDeRegistration').hide();

    } else {
        $('#ScheduledVATRegistrationNumber').hide();
        $('#ScheduledVATDeRegistration').show();

    }
}