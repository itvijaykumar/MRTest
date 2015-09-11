/// <reference path="~/Scripts/_references.js" />

var searching = false;

$(document).ready(function () {
    var checkedIds = {};
    viewModel = new ViewModel();

    // hide the search results
    $("#searchResults").hide();

    // hide the search info banner
    $("#searchInfo").hide();

    // load the saved search
    searchKey = "associate";
    LoadSavedSearch(searchKey);

    // initialise the search fields
    InitialiseSearchFields();

    $(".search-rate-bar").kendoTooltip({
        width: 100,
        position: "top",
        content: "Rate related to current rate paid to an associate on project"
    }).data("kendoTooltip");

    // search grid
    var searchColumns = GetSearchColumns();
    var searchDataSource = GetSearchDataSource(searchKey);

    dataSources[searchKey] = searchDataSource;

    InitialiseResultSort(searchKey, "CVCreatedDate", "desc");

    $("#searchResults").kendoGrid({
        dataSource: searchDataSource,
        autoBind: false,
        height: 875,
        sortable: {
            allowUnsort: false
        },
        pageable: {
            pageSize: 10,
            buttonCount: 1,
            pageSizes: [100]
        },
        columns: searchColumns,
        dataBound: function (e) {
            var data = this.dataSource.data();
            $.each(data, function (i, row) {
                try {
                   // alert('xx')
                    if (selectall == true && checkedIds[row.id]!=false) {
                        $('tr[data-uid="' + row.uid + '"] ').find(".ob-checked")
                            .attr("checked", "checked");
                    }
                    else if (checkedIds[row.id] == true) {
                        $('tr[data-uid="' + row.uid + '"] ').find(".ob-checked")
                            .attr("checked", "checked");
                    }
                }
                catch (e) {
                    // do nothing
                }

            });

            $("#searchResults").data("kendoGrid").tbody.on("change", ".ob-checked", function (e) {

                var row = $(e.target).closest("tr");
                var item = $("#searchResults").data("kendoGrid").dataItem(row);
                item.Checked = $(e.target).is(":checked") ? true : false;

                checkedIds[item.id] = $(e.target).is(":checked") ? true : false;

                var checked = [];
                for (var i in checkedIds) {
                    if (checkedIds[i]) {
                        checked.push(i);
                    }
                }

            });


        }
        // dataBound: onDataBound
    });

    var pageSizeDropDown = $('#searchResults').find("select").data("kendoDropDownList");
    pageSizeDropDown.dataSource.add({ text: "All", value: 1000000 });

    //searchResultGrid.table.on("click", ".ob-checked", selectrow);
    // the search button
    $("#search").click(SearchClick);

    // the reset button
    $("#searchResetButton").click(AssociateSearchResetClick);
    function AssociateSearchResetClick() {
        SearchResetClick();
        var state = false;
        selectall = state;
        var grid = $('#searchResults').data('kendoGrid');
        $.each(grid.dataSource.view(), function (idx, record) {
            if (this['Checked'] != state)
                this.dirty = true;
            this['Checked'] = state;
            
        });
        grid.refresh();
        
        checkedIds = {};
        $("#chkSelectAllTop").prop('checked', state);
    }
    // knockout
    infuser.defaults.templateUrl = "/Templates";
    infuser.defaults.templatePrefix = "";
    infuser.defaults.templateSuffix = ".html?v=" + templateCacheBuster;
    infuser.defaults.ajax.async = false;
    ko.applyBindings(viewModel);

    // perform saved search
    PerformSavedAssociateSearch(searchDataSource);

    // perform querystring search
    PerformFreeTextSearch();

    // configure the export
    ConfigureExport("/Search/ExportAssociateSearch", searchKey);
});


//function selectrow() {
//    var checked = this.checked,
//				row = $(this).closest("tr"),
//				grid = $("#searchResults").data("kendoGrid"),
//				dataItem = grid.dataItem(row);

//    checkedIds[dataItem.id] = checked;
//    var checked = [];
//    for (var i in checkedIds) {
//        if (checkedIds[i]) {
//            checked.push(i);
//        }
//    }

//    Alert(checkedIds.length);
//}

function PerformFreeTextSearch() {
    if (getParameterByName("search").length != 0) {
        // get the search term
        var search = getParameterByName("search");

        // hide the search box
        $(".search-box").hide();

        // set the search term
        $("#FreeText").val(search);

        // perform the search
        SearchClick();
    }
}

function InitialiseSearchFields() {
    // create DropDownList for associate status
    if (typeof statusData != "undefined") {
        $("#ApprovalStatusId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: statusData,
        });
        $("#ApprovalStatusId").data("kendoDropDownList").list.width("auto");
    }

    // business unit
    if (typeof businessUnitData != "undefined") {
        $("#BusinessUnitId").kendoMultiSelect({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: businessUnitData
        });
    }

    // business area
    if (typeof businessAreaData != "undefined") {
        $("#BusinessAreaId").kendoMultiSelect({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: businessAreaData
        });
    }

    // availability dates
    $("#AvailableFrom").kendoDatePicker({
        culture: "en-GB",
        format: "dd/MM/yyyy"
    });
    $("#AvailableTo").kendoDatePicker({
        culture: "en-GB",
        format: "dd/MM/yyyy"
    });

    // availability notice interval
    if (typeof noticeIntervalData != "undefined") {
        $("#NoticeIntervalId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: noticeIntervalData
        });
    }

    // clients
    if (typeof clientData != "undefined") {
        $("#ClientId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: clientData,
            change: onClientChange
        });
    }

    // projects
    if (typeof projectData != "undefined") {
        $("#ProjectId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: projectData,
            enable: false,
            change: onProjectChange
        });
    }

    // requirements
    if (typeof requirementData != "undefined") {
        $("#RequirementId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: requirementData,
            enable: false,
            change: onRequirementChange
        });
    }

    // roles
    if (typeof roleData != "undefined") {
        $("#RoleId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: roleData,
            enable: false
        });
    }

    // visa type
    if (typeof visaData != "undefined") {
        $("#VisaTypeId").kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: visaData
        });
    }

    var visaType = $("#VisaTypeId").data("kendoDropDownList");
    if (typeof visaType != "undefined") {
        visaType.list.width(380);
    }

    // visa expiry start and end
    $("#VisaExpiryStart").kendoDatePicker({
        culture: "en-GB",
        format: "dd/MM/yyyy"
    });
    $("#VisaExpiryEnd").kendoDatePicker({
        culture: "en-GB",
        format: "dd/MM/yyyy"
    });

    // assessment grid
    if ($("#assessmentsGrid").length != 0) {
        var assessmentColumns = AssessmentColumns();
        var assessmentDataSource = GetAssessmentDataSource();
        $("#assessmentsGrid").kendoGrid({
            dataSource: assessmentDataSource,
            height: 280,
            autoBind: false,
            sortable: true,
            pageable: true,
            columns: assessmentColumns,
            editable: "popup"
        });
        var assessmentGrid = $("#assessmentsGrid").data("kendoGrid");

        assessmentGrid.bind("edit", grid_edit);
        assessmentGrid.bind("add", grid_edit);

        // add new assessment
        $("#addAssessment").click(AddAssessment);
    }

    //expand panels    
    $("#panelbar-search").data("kendoPanelBar").expand($("#panel-all"));
    $("#panelbar-search").data("kendoPanelBar").expand($("#panel-personal-info"));
}

function PerformSavedAssociateSearch(searchDataSource) {
    if (getParameterByName("saved") == "associate") {
        try {
            var searchModel = searchModels[searchKey];

            // qualifications
            PopulateSearchFiltersQualifications(searchModel);

            // execute the search
            Search(searchDataSource);
        }
        catch (e) {
            // do nothing if there is an error
            alert(e.message);
        }
    }
}

function PopulateSearchFilters(sm) {
    // all
    PopulateSearchFiltersAll(sm);

    // personal information
    PopulateSearchFiltersPersonalInformation(sm);

    // availability
    PopulateSearchFiltersAvailability(sm);

    // rate
    PopulateSearchFiltersRate(sm);

    // location
    PopulateSearchFiltersLocation(sm);

    // assessments
    PopulateSearchFiltersAssessments(sm);

    // project
    PopulateSearchFiltersProject(sm);

    // visa
    PopulateSearchFiltersVisa(sm);
}

function PopulateSearchFiltersAll(sm) {
    // free text
    if (sm.FreeText) {
        $("#FreeText").val(sm.FreeText);
    }
}

function PopulateSearchFiltersAssessments(sm) {
    // todo:
}

function PopulateSearchFiltersVisa(sm) {
    // visa type
    if (sm.VisaTypeId) {
        $("#VisaTypeId").val(sm.VisaTypeId);
    }

    // visa expiry start
    if (sm.VisaExpiryStart) {
        $("#VisaExpiryStart").val(sm.VisaExpiryStart);
    }

    // visa expiry start
    if (sm.VisaExpiryEnd) {
        $("#VisaExpiryEnd").val(sm.VisaExpiryEnd);
    }
}

function PopulateSearchFiltersQualifications(sm) {
    // qualifcations
    if (sm.Qualifications) {
        $("#QualificationsPanelQualificationsDiv input:checkbox[name='Qualifications'][value='" + sm.Qualifications.join("'],[value='") + "']").prop("checked", true);

        $("#QualificationsPanelQualificationsDiv input:checkbox[name='Qualifications'][value='" + sm.Qualifications.join("'],[value='") + "']").each(function () {
            setStyleOfSelectedQualificationAncestors($(this));
        });
    }

    // qualifications other
    if (sm.QualificationsOther) {
        // todo:
    }
}

function PopulateSearchFiltersLocation(sm) {
    // postcode
    if (sm.Postcode) {
        $("#Postcode").val(sm.Postcode);
    }

    // miles
    if (sm.Miles) {
        $("#Miles").val(sm.Miles);
    }
}

function PopulateSearchFiltersRate(sm) {
    // rate from
    if (sm.RateFrom) {
        $("#RateFrom").val(sm.RateFrom);
    }

    // rate to
    if (sm.RateTo) {
        $("#RateTo").val(sm.RateTo);
    }
}

function PopulateSearchFiltersAvailability(sm) {
    // available from
    if (sm.AvailableFrom) {
        $("#AvailableFrom").val(sm.AvailableFrom);
    }

    // available to
    if (sm.AvailableTo) {
        $("#AvailableTo").val(sm.AvailableTo);
    }

    // notice period
    if (sm.NoticePeriod) {
        $("#NoticePeriod").val(sm.NoticePeriod);
    }

    // notice period interval
    if (sm.NoticeIntervalId) {
        $("#NoticeIntervalId").val(sm.NoticeIntervalId);
    }
}

function PopulateSearchFiltersPersonalInformation(sm) {
    // first name
    if (sm.FirstName) {
        $("#FirstName").val(sm.FirstName);
    }

    // last name
    if (sm.LastName) {
        $("#LastName").val(sm.LastName);
    }

    // associate id
    if (sm.AssociateId) {
        $("#AssociateId").val(sm.AssociateId);
    }

    // email
    if (sm.EmailAddress) {
        $("#EmailAddress").val(sm.EmailAddress);
    }

    // phone number
    if (sm.PhoneNumber) {
        $("#PhoneNumber").val(sm.PhoneNumber);
    }

    // approval status
    if (sm.ApprovalStatusId) {
        $("#ApprovalStatusId").val(sm.ApprovalStatusId);
    }

    // agency associate
    if (sm.AgencyAssociate) {
        $("#AgencyAssociate").prop('checked', true);
    }

    // associate
    if (sm.FullAssociate) {
        $("#FullAssociate").prop('checked', true);
    }

    // employed associate
    if (sm.EmployedAssociate) {
        $("#EmployedAssociate").prop('checked', true);
    }

    // interim management associate
    if (sm.InterimManagementAssociate) {
        $("#InterimManagementAssociate").prop('checked', true);
    }
}

function SearchAssociateBackButtonClick() {
    // clear any checkboxes to prevent bulk operations from accidentally acting on the previous result set
    UnCheckAssociates("#searchResults");

    // hide the search back button
    $("#searchBackButton").hide();

    // hide the search info banner
    $("#searchInfo").hide();

    // hide the search results
    $("#searchResults").hide();

    // show the search criteria
    $(".search-box").show();
}

function grid_edit(e) {
    var fromList = $("[data-bind='value:AssessmentType']").data("kendoDropDownList");
    fromList.list.width(300);
}

function AddAssessment() {
    grid = $("#assessmentsGrid").data("kendoGrid");
    grid.addRow();
}

function AssessmentColumns() {
    var assessmentType = $.map(assessmentTypeData, function (item) {
        return [{
            "value": item.value,
            "text": item.text
        }];
    });

    return [
        { field: "AssessmentType", title: "Assessment", values: assessmentType, editor: AssessmentDropDownEditor },
        { field: "Above", title: "Above" },
        { field: "Below", title: "Below" },
        { field: "Passed", title: "Passed", template: "<input name='Passed' class='ob-checked' type='checkbox' data-bind='checked: Passed' #= Passed ? checked='checked' : '' #/>" },
        { field: "Failed", title: "Failed", template: "<input name='Failed' class='ob-checked' type='checkbox' data-bind='checked: Failed' #= Failed ? checked='checked' : '' #/>" },
        { title: "Actions", width: "80px", command: ["delete", "edit"] }
    ];
}

function AssessmentDropDownEditor(container, options) {
    $('<input required data-text-field="text" data-value-field="value" data-bind="value:' + options.field + '"/>')
        .appendTo(container)
        .kendoDropDownList({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: assessmentTypeData,
            schema: {
                model: {
                    fields: {
                        value: { type: "number" },
                        text: { type: "string" }
                    }
                }
            }
        });
}

function GetAssessments() {

    grid = $("#assessmentsGrid").data("kendoGrid");
    var data = grid.dataSource.data();
    var assessments = new Array;

    $.each(data, function (i, row) {

        var item = {
            AssessmentType: row.AssessmentType,
            Above: row.Above,
            Below: row.Below,
            Passed: row.Passed,
            Failed: row.Failed
        };
        assessments.push(item);
    });

    return assessments;
}

function GetAssessmentDataSource() {
    return new kendo.data.DataSource({
        schema: {
            model: {
                fields: {
                    AssessmentType: { field: "AssessmentType", editable: true, type: "number" },
                    Above: { editable: true, type: "number" },
                    Below: { editable: true, type: "number" },
                    Passed: { editable: true, type: "boolean" },
                    Failed: { editable: true, type: "boolean" }
                }
            }
        },
        pageSize: 7
    });
}

function SearchGetSelectedExperienceType() {
    return "1";
}

function SearchProspectBulkToRole() {
    var associates = new Array();

    invalidStatusDetected = false;

    GetProspectableCheckedAssociates("#searchResults", associates);
    if (invalidStatusDetected) {
        Alert("Associate must be at the 'Pending Approval, Approved, Pending Acceptance, Accepted or On Contract status to qualify for prospecting.");
    }
    else {
        if (associates.length > 0) {
            ProspectBulkToRole(associates);
        }
    }

    invalidStatusDetected = false;

    //UnCheckAssociates("#searchResults");
}

function GetProspectableCheckedAssociates(grid, associates) {
    //var data = $(grid).data("kendoGrid").dataSource.data();
    //var grid = $('#searchResults').data('kendoGrid');
    $.each($('#searchResults').data('kendoGrid').dataSource.view(), function (idx, record) {
        if (this['Checked']) {

            var dataRow = $(grid).data("kendoGrid").dataSource.getByUid(record.uid);

            if (dataRow.Status == AssociateApprovalStatus.ACCEPTED
                || dataRow.Status == AssociateApprovalStatus.ON_CONTRACT
                || dataRow.Status == AssociateApprovalStatus.APPROVED
                || dataRow.Status == AssociateApprovalStatus.PENDING_APPROVAL
                || dataRow.Status == AssociateApprovalStatus.PENDING_ACCEPTANCE) {
                associates.push(dataRow.AssociateId);
            }
            else {
                invalidStatusDetected = true;
            }
        }
    });

}



function GetSearchColumns() {
    return [
        { field: "Checked", title: " ", width: "30px", template: "<input name='Checked' class='ob-checked' type='checkbox' data-bind='checked: Checked' #= Checked ? checked='checked' : '' #/>" },
        { field: "AssociateId", title: "AssociateId" },
        { field: "FirstName", title: "First Name", template: "<a onclick='CallAssociate(this);'>#=FirstName#</a>" },
//"<a target=\"_blank\" href=\"/Associate/DownloadDocument?DocumentType=" + 1 + "&associateId=#= AssociateId #\">#= FirstName #</a>"},
        { field: "LastName", title: "Last Name" },
        { field: "Email", title: "Email" },
        { field: "StatusName", title: "Status" },
        { field: "Client", title: "Client" },
        { field: "Role", title: "Role" },
        { field: "CVCreatedDate", title: "CV Created Date", template: "<a target=\"_blank\" href=\"/Associate/DownloadDocument?DocumentType=" + 1 + "&associateId=#= AssociateId #\">#= (CVCreatedDate == null ? '' : kendo.toString(kendo.parseDate(CVCreatedDate, 'yyyy-MM-dd'), 'dd/MM/yyyy')) #</a>" },
        { title: "View", width: "80px", template: "<a href='/Associate/Details/#= AssociateId #'>Details<a>" }
    ];
}
function CallAssociate(t)
{
    //Alert(t);
    var associate_uid = $(t).closest("tr:parent").data("uid");
    //Alert(timesheet_uid);
    var datasource = dataSources["associate"];
    var associate = datasource.getByUid(associate_uid);
    var template = kendo.template(
        "<table width=\"300\">" +
        "<tr><td>Name</td><td>#= Name #</td></tr>" +
        "<tr><td>Email</td><td>#= Email #</td></tr>" +
        "<tr #= Mobile == null ? 'style=\"display: none;\"' : '' #><td>Mobile</td><td>#= Mobile #</td></tr>" +
        "<tr #= Home == null ? 'style=\"display: none;\"' : '' #><td>Home</td><td>#= Home #</td></tr>" +
        "<tr #= Work == null ? 'style=\"display: none;\"' : '' #><td>Work</td><td>#= Work #</td></tr>" +
        "</table>");

    var data = {
        Name: associate.FirstName + " " + associate.LastName,
        Email: associate.Email,
        Home: associate.HomePhone,
        Work: associate.WorkPhone,
        Mobile: associate.MobilePhone
    };
    var html = template(data);

    Alert(html, "Call Associate");
}
var checkedIds = {};
var selectall = false;
function CheckAll(ele) {
    var state = $(ele).is(':checked');
    selectall = state;
    var grid = $('#searchResults').data('kendoGrid');
    $.each(grid.dataSource.view(), function (idx, record) {
        if (this['Checked'] != state)
            this.dirty = true;
        this['Checked'] = state;
        checkedIds[record.id] = state;
    });
    grid.refresh();
    $("#chkSelectAllTop").prop('checked', state);
    $("#unchkSelectAllTop").prop('checked', false);
}

function UnCheckAll(ele) {
    
    var state = $(ele).is(':checked');
    if (state == true) {
        selectall = false;
        var grid = $('#searchResults').data('kendoGrid');
        $.each(grid.dataSource.view(), function (idx, record) {
           this.dirty = true;
           this['Checked'] = false;
           checkedIds[record.id] = false;
        });
        grid.refresh();

        $("#chkSelectAllTop").prop('checked', false);
        checked = [];
    }
}

function CheckRow(ele) {

    var state = $(ele).is(':checked');
    var row = $(ele).closest("tr"),
       grid = $('#searchResults').data("kendoGrid"),
       dataItem = grid.dataItem(row);
    checkedIds[dataItem.id] = state;

    if (state == false) {
        $("#chkSelectAllTop").prop('checked', false);
    }
    else {
        var varAll = true;
        $(".k-grid-content tbody tr").each(function () {
            if ($(this).find('.ob-checked').is(':checked') == false) {
                varAll = false;
            }
        });
        if (varAll == true) {
            $("#chkSelectAllTop").prop('checked', true);
        }
        else {
            $("#chkSelectAllTop").prop('checked', false);
        }
    }
}



function GetSearchDataSource(key) {
    return new kendo.data.DataSource({
        type: "json",
        transport: {
            read: {
                url: "/Search/Associates",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "POST"
            },
            parameterMap: function (data, operation) {
                if (operation == "read") {
                    var searchModel = searchModels[key];

                    var sort = resultSort[key];

                    if (typeof (data.sort) != "undefined") {
                        sort.column = data.sort[0].field;
                        sort.direction = data.sort[0].dir;
                    }

                    return kendo.stringify({
                        search: searchModel,
                        pageSize: data.pageSize,
                        page: data.page,
                        sortDirection: sort.direction,
                        sortColumn: sort.column,
                    });
                }
            }
        },
        requestEnd: function (e) {
            if (searching == true) {
                // show the search results
                $("#searchResults").show();

                // show the search info banner
                $("#searchInfo").fadeIn("fast");

                // show the back button
                $("#searchBackButton").show();
            }
        },
        schema: {
            errors: function (response) {
                // check the response for any errors
                if (response.Status && response.Status !== "OK") {
                    return response.Status;
                }

                // no errors
                return false;
            },
            data: "Items",
            total: "TotalServerItems",
            model: {
                id: "AssociateId",
                fields: {
                    Checked: { editable: false, type: "boolean" },
                    AssociateId: { editable: false, type: "number" },
                    FirstName: { editable: false, type: "string" },
                    LastName: { editable: false, type: "string" },
                    Email: { editable: false, type: "string" },
                    Status: { editable: false, type: "number" },
                    StatusName: { editable: false, type: "string" },
                    Client: { editable: false, type: "string" },
                    Role: { editable: false, type: "string" },
                    CVCreatedDate: { editable: false, type: "date" }
                }
            }
        },
        pageSize: 100,
        serverPaging: true,
        serverFiltering: true,
        serverSorting: true,
        error: function (e) {
            if (e.errors !== false) {

                if (typeof (e.errors) == "undefined") {
                    alert("sorry, an error has occured.");
                }
                else {
                    // display the error message
                    alert(e.errors);
                }
            }

            return false;
        }
    });
}

function GetSearchModel() {

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

function GetQualificationsOther() {
    // todo: get the qualifications
    return null;
}

function SendBulkEmail() {
    var associates = new Array();

    GetCheckedAssociates("#searchResults", associates);

    if (associates.length > 0) {
        SendBulkEmailDialog(associates);
    }

    //UnCheckAssociates("#searchResults");
}

function SendBulkSMS() {
    var associates = new Array();

    GetCheckedAssociates("#searchResults", associates);

    if (associates.length > 0) {
        SendBulkSMSDialog(associates);
    }

    UnCheckAssociates("#searchResults");
}

function ViewModel() {
    var self = this;

    // prospect clients
    self.prospectClient = ko.observableArray([]);

    // prospect projects
    self.prospectProject = ko.observableArray([]);

    // prospect requirements
    self.prospectRequirement = ko.observableArray([]);

    // prospect roles
    self.prospectRole = ko.observableArray([]);
};

// replace the GetSelectedExperienceType function with SearchGetSelectedExperienceType
GetSelectedExperienceType = SearchGetSelectedExperienceType;

// replace the ProspectToRole function with ProspectBulkToRole
ProspectToRole = SearchProspectBulkToRole;

var viewModel = null;
var invalidStatusDetected = false;

$(function () {
    if (sessionStorage) {
        sessionStorage.setItem("SearchFilter", "search/associate?saved=associate");
    }
});