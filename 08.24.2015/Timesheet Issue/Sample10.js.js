function PopulateTimeSheetsGrid() {
    $("#TimeSheetsGrid").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/TimeSheet/GetTimeSheetsForInvoicing",
                    dataType: "json",
                    data: {
                        associateId: $("#Id").val()
                    },
                    type: "POST"
                }
            },
            schema: {
                model: {
                    id: "TimehSheetId",
                    fields: {
                        ProjectName: { type: "string" },
                        StartDate: { type: "date" },
                        EndDate: { type: "date" },
                        RoleName: { type: "string" },
                        Status: { type: "string" },
                        ManagerName: { type: "string" },
                        TimeSheetId: { type: "number" },
                        AssociateId: { type: "number" },
                        TimesheetTypeId: { type: "number" }
                    }
                }
            },
            pageSize: 5,
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false
        },
        scrollable: false,
        sortable: true,
        pageable: true,
        dataBound: onTimeSheetsGridDataBound,
        columns: [
            { field: "check_item", title: "<input type='checkbox' id='chkSelectAll' onchange='TimeSheetsGridSelectAll();'/>", template: "<input type='checkbox' id='inpchk' />", width: '50px', sortable: false },
            { field: "ProjectName", title: "Project Name", width: "160px" },
            { field: "StartDate", title: "Start Date", width: "120px", template: "#= kendo.toString(kendo.parseDate(StartDate, 'yyyy-MM-dd'), 'dd/MM/yyyy') #" },
            { field: "EndDate", title: "End Date", width: "120px", template: "#= kendo.toString(kendo.parseDate(EndDate, 'yyyy-MM-dd'), 'dd/MM/yyyy') #" },
            { field: "RoleName", title: "Role Name", width: "120px" },
            { field: "Status", title: "Status", width: "100px" },
            { field: "ManagerName", title: "Manager Name", width: "100px" },
            { field: "Details", title: "Details", width: "100px", command: [{ name: "Details", text: "View", click: TimeSheetsViewTimeSheetDetails}] }
        ]
    });
}

function onTimeSheetsGridDataBound(e) {
    var dataSource = $("#TimeSheetsGrid").data("kendoGrid").dataSource;
    var data = dataSource.data();

    $(data).each(function (index, item) {
        if (item.Status != "Approved") {
            var row = $("#TimeSheetsGrid").data("kendoGrid").table.find('tr[data-uid="' + item.uid + '"]');
            $(row).find("#inpchk").prop("disabled", "disabled");
        }
    });
}

function TimeSheetsViewTimeSheetDetails(e) {
    var uid = $(e.currentTarget).closest("tr:parent").data("uid");
    var dataRow = $("#TimeSheetsGrid").data("kendoGrid").dataSource.getByUid(uid);

    jQuery.ajaxSetup({ cache: false });
    var id = dataRow.TimeSheetId;

    var prefix = "timesheet-invoice-";

    LoadTimesheet(id, dataRow.TimesheetTypeId, prefix);
}

function GenerateInvoiceReport() {
    // get checked timesheets
    var checked = new Array();

    $("#TimeSheetsGrid tr #inpchk").each(function () {
        if ($(this).prop('checked')) {
            var uid = $(this).closest("tr:parent").data("uid");
            var dataRow = $("#TimeSheetsGrid").data("kendoGrid").dataSource.getByUid(uid);

            checked.push(dataRow.TimeSheetId);
        }
    });

    if (checked.length == 0) {
        alert("Please select one or more timesheets to include in the Invoice report");
    } else {
        var data = {
            associateId: $("#Id").val(),
            timesheetIds: checked
        };        

        $.ajax({
            type: 'POST',
            url: '/TimeSheet/CreateInvoice',
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (data, success) {
                var result = JSON.stringify(data);
                if (result != '') {
                    // refresh grids
                    InitialiseAssociateTimesheetGrid();
                    InitialiseAssociateInvoiceGrid();
                    InitialiseAssociateRetainerGrid();
                    InitialiseInvoicedAssociateTimesheetGrid();

                    returnToDashBoard();
                }
                else {
                    alert(result);
                }
            },
            error: function (data, error) {
                alert("There was a problem generating the invoice.");                
            }
        });
    }
}

function TimeSheetsGridSelectAll() {
    var checked = $("#TimeSheetsGrid #chkSelectAll").prop("checked");

    if (checked) {
        $("#TimeSheetsGrid tr #inpchk").prop("checked", true);
    }
    else {
        $("#TimeSheetsGrid tr #inpchk").prop("checked", false);
    }
}
function UpdateNotes(tsentryId, colName, colValue) {

    $.ajax({
        type: 'POST',
        url: '/TimeSheet/UpdateNotes',
        data: {
            TimeSheetEntryId: tsentryId,
            ExpenseColumn: colName,
            ExpenseNotes: colValue
        },
        success: function (result) {

        }
    });
}