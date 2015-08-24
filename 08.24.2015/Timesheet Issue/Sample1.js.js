
/// <reference path="~/Scripts/_references.js" />

var tsViewModel = null;
var test;
ko.validation.configure({
    decorateElement: true
});

(function (ts) {
    var tsheetVm = function (model) {
        var self = this;
        self1 = this;
        newModel = model;
        self.TimesheetEntry = ko.observable(new Timesheet.TimesheetEntryViewModel(model));
        self.TimesheetAssociateDetails = new Timesheet.TimesheetAssociateModel(model.TimesheetAssociate);
        self.TimesheetRecieptInfo = new Object(model.ExpenseReceiptDetails);
        self.Suspended = model.Suspended;

        self.ReadOnly = (model.Status == "Invoiced"
            || model.Status == "Approved"
            || model.Status == "Processed"
            || model.Status == "Paid"
            || model.Status == "Billed to client"
            || model.Status == "Suspended"
            || model.Status == "Checked"); // changed from "Complete"
        
        if (model.CurrentUserId == model.TimesheetApproverAssociateId && model.AssociateId != model.TimesheetApproverAssociateId) {
            self.ReadOnly = true;
        }

        self.DetermineReadOnly = function () {
            if (self.Suspended || self.ReadOnly) {                
                $("#editTimesheet *").attr("disabled", "disabled");
                return true;
            }

            return false;
        };
    };

    ts.TimesheetViewModel = tsheetVm;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var tsEntryVM = function (model) {
        var self = this;

        self.Timesheet = ko.observable(new Timesheet.TimesheetModel(model));

        self.dirtyItems = ko.computed(function() {
            return ko.utils.arrayFilter(self.Timesheet().Entries(), function(item) {
                return item.IsDirtyFlag().isDirty();
            });
        }, self);

    
        self.isDirty = ko.computed(function() {
            return self.dirtyItems().length > 0;
        }, self);

        self.ParentDataSource = null;

        self.ParentDataSourcePage = null;

        self.IsDirty = ko.observable();

        self.Prefix = ko.observable("");

        self.FirstName = ko.observable("");

        self.From = ko.observable();

        self.To = ko.observable();

        self.test = ko.computed(function () {
            return self.Timesheet().AccomodationLimit() + 1;
        });

        self.TotalHours = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Hour();
            });

            return total;
        });

        self.IsAgency = model.TimesheetAssociate.IsAgencyOrUmbrella;

        self.TotalWorking = ko.computed(function () {
            var totalDays = 0;
            var totalWorking = 0;
            var rateWeekTotal = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                var todayTotal = 0;
                var today =0
                if (item.AttendenceId() != 3) {
                    switch (item.AttendenceId()) {
                        case 1:
                        case 6:
                        case 4:
                            //totalDays += 1;
                            today = 1;
                            todayTotal = today * item.AssociateRate();
                            break;
                        case 14:
                            //totalDays = totalDays - 1;
                            today = -1;
                            todayTotal = today * item.AssociateRate();
                            break;
                        case 2:
                        case 5:
                        case 7:
                            //totalDays += 0.5;
                            today = 0.5;
                            todayTotal = today * item.AssociateRate();
                            break;
                        case 15:
                            //totalDays = totalDays - 0.5;
                            today = -0.5;
                            todayTotal = today * item.AssociateRate();
                            break;

                        default:
                            break;
                    }
                } else if (item.AttendenceId() == 3) {

                    if (item.ActualHour() > 0 || item.Minute() > 0) {
                        var today = (+item.ActualHour() + (+Math.floor(+item.ActualMinute() * 100 / 60) * .01));
                        var day = today / +model.WorkingHours;
                        todayTotal = day * item.AssociateRate();
                        //totalDays += +day;
                    }
                }
                totalWorking += todayTotal;

            });

            rateWeekTotal = totalWorking;

            totalWorking = (totalDays) + (+self.Timesheet().IncentiveDays() * +self.Timesheet().IncentiveRate);

            totalWorking += rateWeekTotal;

            return totalWorking.toFixed(2);
        });

        self.TotalWorkingHours = ko.computed(function () {
            var totalHour = 0;
            var totalMinute = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return totalHour += +item.ActualHour();
            });

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return totalMinute += +item.ActualMinute();
            });

            //var TotalWorkingHours = (+totalHour + +Math.floor(totalMinute / 60) + totalMinute % 60 / 100);

            //return TotalWorkingHours.toFixed(2);
            var TotalWorkingHours = eval(totalHour+Math.floor(totalMinute / 60)) + " : " + (totalMinute % 60);

            //var TotalWorkingHours = totalHour + (totalMinute / 60);
            return TotalWorkingHours;
        });

        self.TotalMintues = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Minute();
            });

            return total;
        });

        self.TotalOverTime = ko.computed(function () {
            var total = 0;
            var amount = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                var fraction = +item.OverTimeComputed();
                var hour = Math.floor(+fraction);
                var min = (fraction % 1) * 100 / 60;
                var itemTotal = hour + min;
                total += +itemTotal;
            });

            amount = +total * +self.Timesheet().OverTimePayAway;
            return amount.toFixed(2);
        });

        self.TotalOverProduction = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.OverProduction();
            });

            var result = total * self.Timesheet().OverProduction;

            return result.toFixed(2);
        });

        self.TotalOneOfPayment = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.OneOfPayment();
            });

            return total.toFixed(2);
        });

        self.TotalAccomodation = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Accomodation();
            });

            return total.toFixed(2);
        });

        self.TotalMealAllowance = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.MealAllowance();
            });

            return total.toFixed(2);
        });

        self.TotalTravel = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Travel();
            });

            return total.toFixed(2);
        });

        self.TotalNoOfMiles = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.NoOfMiles();
            });
            return total.toFixed(0);
        });

        self.TotalMileage = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Mileage();
            });

            return total.toFixed(2);
        });

        self.TotalParking = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Parking();
            });

            return total.toFixed(2);
        });

        self.TotalOther = ko.computed(function () {
            var total = 0;

            ko.utils.arrayForEach(self.Timesheet().Entries(), function (item) {
                return total += +item.Other();
            });

            return total.toFixed(2);
        });

        self.EnableAll = ko.observable();

        // all defaults to false
        self.CanApprove = ko.observable(false);

        self.Key = ko.observable("");

        self.Key.subscribe(function (val) {
            if(val == "all-timesheet-approval") {              
                self.CanApprove(false);
                self.CanReject(false);
                self.CanComment(false);
            }          
        });
        
        self.CanReject = ko.observable(false);

        self.AssociateCanReject = ko.observable(false);

        self.CanComment = ko.observable(false);

        self.CanPending = ko.observable(false);

        self.CanComplete = ko.observable(false);

        self.CanReSubmit = ko.observable(false);

        self.CanSubmit = ko.observable(false);

        self.CanSave = ko.observable(false);

        self.ApproveEnabled = ko.observable(false);

        self.RejectEnabled = ko.observable(false);

        self.ReSubmitEnabled = ko.observable(false);

        self.SubmitEnabled = ko.observable(false);

        self.SaveEnabled = ko.observable(false);

        //alert(model.AssociateId + ":" +model.CurrentUserId + ":" + model.TimesheetApproverAssociateId + ":" +model.Status+":"+self.Timesheet().TimeSheetTypeId);

        // associate view
        if (model.AssociateId == model.CurrentUserId) {

            self.CanPending(false);
            self.CanComplete(false);

            if (model.Status == "Approved") {
                // defaults apply
            }

            if (model.Status == "Pending") {
                // defaults apply
            }

            if (model.Status == "Checked") { // changed from "Complete"
                // defaults apply
            }

            if (model.Status == "Submitted") {
                self.AssociateCanReject(true);
                self.CanComment(true);
            }

            if (model.Status == "Submitted" && model.TimesheetApproverAssociateId != model.AssociateId && self.Timesheet().TimeSheetTypeId == 3) {
                self.AssociateCanReject(false);
                self.CanComment(false);

            }
            if (model.Status == "Blank") {
                self.CanSubmit(true);
                self.CanSave(true);

                self.SubmitEnabled(true);
                self.SaveEnabled(true);
            }

            if (model.Status == "Blank" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSubmit(false);
                self.CanSave(false);
            }

            if (model.Status == "Updated") {
                self.CanSubmit(true);
                self.CanSave(true);

                self.SubmitEnabled(true);
                self.SaveEnabled(true);
            }

            if (model.Status == "Updated" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSave(false);
            }

            if (model.Status == "Rejected") {
                self.CanSave(true);
                self.CanReSubmit(true);

                self.ReSubmitEnabled(true);
                self.SaveEnabled(true);
            }

            if (model.Status == "Rejected" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSave(false);
                self.CanReSubmit(true);

                self.ReSubmitEnabled(true);
                self.SaveEnabled(false);
            }
            self.IsComment = ko.computed(function() {
                //if (model.Status == "Submitted") {
                //    return true;
                //}
                //return false;
                return true;
            });

        }
        // approver
        if (model.TimesheetApproverAssociateId == model.CurrentUserId) {

            self.CanPending(false);
            self.CanComplete(false);

            if (model.Status == "Approved") {
                self.CanApprove(true);
                self.CanReject(true);
                self.CanComment(true);
            }

            if (model.Status == "Pending") {
                // defaults apply
            }

            if (model.Status == "Checked") { // changed from "Complete"
                self.CanApprove(false);
                self.CanReject(false);
            }

            if (model.Status == "Submitted") {

                //if (self.Key() == "all-timesheet-approval")
                //{
                //    self.CanApprove(false);
                //    self.CanReject(false);
                //    self.CanComment(false);
                //}
                //else
                //{
                self.CanApprove(true);
                self.CanReject(true);
                self.CanComment(true);
                //}
                

                self.ApproveEnabled(true);
                self.RejectEnabled(true);
            }

            if (model.Status == "Blank") {
                // defaults apply
            }

               if (model.Status == "Blank" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSubmit(false);
                self.CanSave(false);
               }

            if (model.Status == "Updated") {
                // defaults apply
            }

            if (model.Status == "Updated" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSave(false);
            }

            if (model.Status == "Rejected") {
                // defaults apply
            }

            if (model.Status == "Rejected" && self.Timesheet().TimeSheetTypeId == 3) {
                self.CanSave(false);
                self.CanReSubmit(true);

                self.ReSubmitEnabled(true);
                self.SaveEnabled(false);
            }


        self.IsComment = ko.computed(function() {
           
            //if(model.Status == "Rejected" || model.Status == "Submitted") {
            //    return true;
            //}

            //return false;
            return true;
        });
        }

        self.IsNotPending = ko.computed(function () {
            if (model.Status != "Pending") {
                return true;
            }

            return false;
        });

        self.IsApproved = ko.computed(function () {
            if (model.Status == "Approved" || model.Status == "Pending") {
                return true;
            }

            return false;
        });

        //if (model.Status == "Submitted") {
        //    self.CanApprove(true);
        //    self.CanReject(true);
        //    self.CanComment(true);
        //    self.ApproveEnabled(true);
        //    self.RejectEnabled(true);
        //}

        self.IsRejected = ko.computed(function () {
            if (model.Status == "Rejected") {
                return true;
            }

            return false;
        });

        self.EnableHistory = ko.computed(function () {
            return true;
            //if (model.Status == "Submitted" || model.Status == "Rejected") {
            //    return true;
            //} else {
            //    for (var i in model.History) {

            //        if (model.History[i].Status == "Submitted") {
            //            return true;
            //        }
            //    }
            //}

            //return false;
        });

        self.IsSubmitted = ko.computed(function () {
            if (model.Status == "Submitted" && self.Timesheet().AssociateId != self.Timesheet().CurrentUserId) {
                return true;
            }

            return false;
        });

        self.DisableHourMin = function () {
            $('.ts-hour-min').attr("disabled", "disabled");
        };

        self.SaveOnExpenseLoad = function () {
            var model = ko.toJSON(self.Timesheet());
            var model3 = JSON.parse(model);
            var timesheetId = self.Timesheet().TimesheetId;
            $.ajax({
                type: 'POST',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                url: '/TimeSheet/SaveTimesheetEntriesWithoutStatus',
                cache: false,
                data: model,
                success: function (data, success) {

                },
                error: function (data, error) {
                    //console.log(data);
                    //console.log(error);
                }
            });
        };
    
        self.Save = function () {            
            if (!self.Timesheet().IsTSValid()) {               
                self.Timesheet().showErrors();
                return;
            }
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;

            var model = ko.toJSON(self.Timesheet());
            var model3 = JSON.parse(model);

            var timesheetId = self.Timesheet().TimesheetId;
            /*if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Save requires a note");
            } else {*/


                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Notification",
                    buttons: [{
                        //IE Temp COMMENTED CODE
                        //   class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/SaveTimesheetEntries',
                                cache: false,
                                data: model,
                                success: function (data, success) {

                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                    //console.log(data);
                                    //console.log(error);
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    }, {
                        //IE Temp COMMENTED CODE
                        //  class: "btn btn-default",
                        text: "No",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }]
                });
            //}
            $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to Save?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.Submit = function () {
            if (!self.Timesheet().IsTSValid()) {
                self.Timesheet().showErrors();

                return;
            }
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;

            var model = ko.toJSON(self.Timesheet());
            var model3 = JSON.parse(model);

            var timesheetId = self.Timesheet().TimesheetId;
            /*if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Submit requires a note");
            } else {*/

                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    dialogClass: '',
                    title: "Notification",
                    buttons: [{
                        // class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                cache: false,
                                url: '/TimeSheet/SubmitTimesheet',
                                data: model,
                                success: function (data, success) {
                                    // update the grid
                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    }, {
                        // class: "btn btn-default",
                        text: "No",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }]
                });
            //}
                $dialog.html('<div class="alert" style="width:200px"> <p> <strong>Are you sure you wish to Submit?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.ReSubmit = function () {
            if (!self.Timesheet().IsTSValid()) {
                self.Timesheet().showErrors();
                return;
            }
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            var model = ko.toJSON(self.Timesheet());

            var timesheetId = self.Timesheet().TimesheetId;
           
            /*if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Re-Submit requires a note");
            } else {*/

                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Notification",
                    buttons: [{
                        //IE Temp COMMENTED CODE
                        //  class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/ReSubmitTimesheet',
                                cache: false,
                                data: model,
                                //            dataType : "application/json",
                                success: function (data, success) {
                                    // update the grid
                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    }, {
                        //IE Temp COMMENTED CODE
                        //  class: "btn btn-default",
                        text: "No",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }]
                });
            //}
                $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to Re-Submit?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.Back = function (check) {
            if (check && self.isDirty()) {
                Confirm("Unsaved changes will be lost, do you wish to continue?", "Notification", "OK", "Cancel", goBack);
            } else {
                goBack();
            }
        };

        function goBack() {

            $('#' + self.Prefix() + 'timesheetDetail').hide();
            $('#' + self.Prefix() + 'adjustTimesheetDetail').hide();

            $('#invoiced-timesheet-adjustTimesheetDetail').hide();

            if (self.Key().indexOf("other") != -1) {
                $('.approver-lists').hide();
                $('.other-approver-lists').fadeIn("fast");
            } else {
                $('.' + self.Prefix() + 'lists').fadeIn("fast");
            }
        }

        self.Approve = function () {
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            var model = ko.toJSON(self.Timesheet());
            var model3 = JSON.parse(model);
            var timesheetId = self.Timesheet().TimesheetId;
            /*if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Approve requires a note");
            } else {*/


                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Notification",
                    buttons: [{
                        //IE Temp COMMENTED CODE
                        //   class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/ApproveTimesheet?roleId=' + $('#roleId').val(),
                                cache: false,
                                data: model,
                                success: function (data, success) {
                                    // update the grid
                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    }, {
                        //IE Temp COMMENTED CODE
                        //  class: "btn btn-default",
                        text: "No",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }]
                });
            //}
                $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to Approve?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.Reject = function () {
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            var model = ko.toJSON(self.Timesheet());

            if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Rejection requires a note including a reason");
            } else {
                var $dialog = $("<div/ >").dialog({
                    visible: false,
                    modal: true,
                    title: "Notification",
                    buttons: [
                    {
                        //class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/RejectTimesheet',
                                cache: false,
                                data: model,
                                //            dataType : "application/json",
                                success: function (data, success) {
                                    $("#rejectCommentLabel").html("");
                                    // update the grid
                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    },
                         {
                             //IE Temp COMMENTED CODE
                           //  class: "btn btn-default",
                             text: "No",
                             click: function () {
                                 $(this).dialog('destroy').remove();
                             }
                         }
                    ]
                });

                $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to Reject?<strong></p></div>');
                $dialog.dialog("open");
            }
        };

        self.RejectToUpdate = ko.computed(function () {
            if (model.Status == "Submitted" && self.Timesheet().AssociateId == self.Timesheet().CurrentUserId) {
                return true;
            }
            return false;
        });

        self.AssociateReject = function () {
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            var model = ko.toJSON(self.Timesheet());

            if (comment == "") {
                $("#commentText").addClass("highlight"); //.attr('style', ' border-color:: #f2632f');
                $("#rejectCommentLabel").html("Rejection requires a note including a reason");
            } else {
                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Notification",
                    buttons: [
                    {
                        //IE Temp COMMENTED CODE
                      //  class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/AssociateRejectTimesheet',
                                cache: false,
                                data: model,
                                success: function (data, success) {
                                    $("#rejectCommentLabel").html("");
                                    // update the grid
                                    self.ParentDataSource.read();
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    },
                    {
                        //IE Temp COMMENTED CODE
                      //  class: "btn btn-default",
                        text: "No",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }
                    ]
                });

                $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to Reject?<strong></p></div>');
                $dialog.dialog("open");
            }
        };

        self.Pending = function () {
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            var model = ko.toJSON(self.Timesheet());
            var model3 = JSON.parse(model);
            var timesheetId = self.Timesheet().TimesheetId;
            if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Pending requires a note including reason");
            } else {


                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Mark TimeSheet As Pending",
                    buttons: [
                    {
                        //class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/MarkAsPending',
                                cache: false,
                                data: JSON.stringify(model),
                                success: function (data, success) {
                                    $("#rejectCommentLabel").html("");

                                    // refresh the grid
                                    self.ParentDataSource.read();

                                    // go back
                                    self.Back(false);
                                },
                                error: function (data, error) {
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    },
                         {
                             //IE Temp COMMENTED CODE
                             //  class: "btn btn-default",
                             text: "No",
                             click: function () {
                                 $(this).dialog('destroy').remove();
                             }
                         }
                    ]
                });
            }
            $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to mark this timesheet as pending?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.Complete = function () {
            var comment = $("#commentText").val();
            self.Timesheet().Comment = comment;
            /*if (comment == "") {
                $("#commentText").addClass("highlight");
                $("#rejectCommentLabel").html("Submit requires a note");
            } else {*/
                var $dialog = $("<div/ >").dialog({
                    autoOpen: false,
                    modal: true,
                    title: "Mark TimeSheet As Completed",
                    buttons: [
                    {
                        // class: "btn btn-default",
                        text: "Yes",
                        click: function () {
                            $.ajax({
                                type: 'POST',
                                dataType: 'json',
                                contentType: 'application/json; charset=utf-8',
                                url: '/TimeSheet/MarkAsCompleted',
                                cache: false,
                                data: JSON.stringify(model),
                                success: function (data, success) {
                                    if (data) {
                                        $("#rejectCommentLabel").html("");
                                        // update the grid
                                        self.ParentDataSource.read();
                                        self.Back(false);
                                    }
                                    else {
                                        alert("Unable to complete timesheet as all receipts have not yet been accepted.");
                                    }
                                },
                                error: function (data, error) {
                                    alert(error);
                                }
                            });

                            $(this).dialog('destroy').remove();
                        }
                    },
                         {
                             //IE Temp COMMENTED CODE
                             //  class: "btn btn-default",
                             text: "No",
                             click: function () {
                                 $(this).dialog('destroy').remove();
                             }
                         }
                    ]
                });
            //}
                $dialog.html('<div class="alert" style="width:200px"><p> <strong>Are you sure you wish to mark this timesheet as completed?<strong></p></div>');
            $dialog.dialog("open");
        };

        self.Suspended = model.Suspended;

        if (self.Suspended)
        {
            self.CanSubmit(false);
            self.CanSave(false);
            self.CanReSubmit(false);
        }
          
        self.NoExpenseAllowed = ko.computed(function () {
            if ((CheckPositive(model.TravelLimit)
                || CheckPositive(model.MealAllowanceLimit)
                || CheckPositive(model.AccomodationLimit)
                || CheckPositive(model.MileageLimit)
                || CheckPositive(model.ParkingLimit)
                || CheckPositive(model.OtherLimit))
                && self.Timesheet().AssociateId == self.Timesheet().CurrentUserId) { return true; }
            return false;
        });

        self.CanUploadReceipt = ko.computed(function () {
            return (model.Status != "Processed"
                    && model.Status != "Paid"
                    && model.Status != "Billed to client"
                    && model.Status != "Suspended"
                    && model.Status != "Checked"); // changed from "Complete"
        });        

        function CheckPositive(val) {
            if (typeof (val) == "undefined" || typeof (val) == "object" || val == null || +val <= 0) {
                return false;
            }
            return true;
        };
    };

    ts.TimesheetEntryViewModel = tsEntryVM;
}(window.Timesheet = window.Timesheet || {}));

ko.bindingHandlers.descendantBinding = {
    init: function () {
        return { controlsDescendantBindings: true };
    }
};

ko.bindingHandlers.disableAll = {
    update: function (element, valueAccessor, allBindingsAccessor) {
        var aa = allBindingsAccessor();
        var selector = aa.selector;
        var va = valueAccessor();
        var value = ko.utils.unwrapObservable(va);
        if (value) {
            $(selector).children().removeAttr('disabled');
        }
        else {
        }
    }
};

ko.bindingHandlers.DisableElem = {
    init: function (elem, v, a, vm, bc) {
        if (bc.$parent.Enable() && vm.IsDay && ko.utils.unwrapObservable(v()) && bc.$data.EnableAbsense() && vm.DisableOnAttendence()) {

            $(elem).prop('disabled', false);

        } else {
            if (vm.DisableOnAttendence() == false && elem.tagName.toLowerCase() == "select") { }
            else {
                $(elem).prop('disabled', true);
                //$(elem).parent().parent().find("input,button,select").prop("disabled", true);
            }
        }
    },
    update: function (elem, v, a, vm, bc) {
        if (bc.$parent.Enable() && vm.IsDay && ko.utils.unwrapObservable(v()) && bc.$data.EnableAbsense() && vm.DisableOnAttendence()) {
            if (!vm.OverTimeEnabe() && (elem.name == "OvertTimeHour" || elem.name == "OvertTimeMinute")) {
                $(elem).prop('disabled', true);               
            } else {
                $(elem).prop('disabled', false);
            }           
        } else {
            if (vm.DisableOnAttendence() == false && elem.tagName.toLowerCase() == "select") { }
            else if (vm.DisableOnAttendence() == false && vm.OverTimeEnabe() == true && (elem.name == "OvertTimeHour" || elem.name == "OvertTimeMinute" || elem.tagName.toLowerCase() == "select")) {
                    $(elem).prop('disabled', false);                
            }
            else {
                $(elem).prop('disabled', true);
            }            
        }
    }
};

ko.bindingHandlers.ToggleElem = {
    update: function (elem, v, a, vm, bc) {
        var identifier = $(elem).attr("class");
        if (identifier.indexOf("comment") != -1) {
            if (ko.utils.unwrapObservable(v()) == null) {
                $(elem).hide();
            } else if (ko.utils.unwrapObservable(v()).length > 0) {
                $(elem).show();
            } else {
                $(elem).hide();
            }
        } else {
            if (ko.utils.unwrapObservable(v()) == null) {
                $(elem).show();
            } else if (ko.utils.unwrapObservable(v()).length > 0) {
                $(elem).hide();
            } else {
                $(elem).show();
            }
        }
    }
};

ko.bindingHandlers.jqyValidate = {
    init: function (elem, v) {         //console.log("valid");

    },
    update: function (elem, v, a, vm, bc) {

        //console.log("valid");

    }
};

function onSuc() {
    $(".retainerlinks").hide();
    $("#rCommentArea").hide();

    if (typeof dataSources != "undefined" && dataSources["all-retainer-approval"] != null) {
        dataSources["all-retainer-approval"].read();
    }
    InitialiseAwaitingApprovalTimesheetGrid();
    returnToDashBoard(this);
}

function returnToDashBoard(e) {
    var classAttribute = $(e).closest(".ui-tabs-panel").find("[class$='-lists']").attr("class");
    var prefix = "associate-";

    if (typeof classAttribute != 'undefined' && classAttribute.length > 0) {
        prefix = classAttribute.match(/\w*-lists\b$/)[0];
        prefix = prefix.substring(0, prefix.indexOf("-") + 1);
    }

    $('#' + prefix + 'retainerDetail').hide();
    $('.' + prefix + 'lists').fadeIn("fast");
    $('#' + prefix + 'InvoiceDetail').hide();

    return false;
}

function valedit() {
}

ko.dirtyFlag = function (root, isInitiallyDirty, hashFunction) {

    hashFunction = hashFunction || ko.toJSON;

    var
        self = this,
        _objectToTrack = root,
        _lastCleanState = ko.observable(hashFunction(_objectToTrack)),
        _isInitiallyDirty = ko.observable(isInitiallyDirty),

        result = function () {
            self.forceDirty = function () {
                _isInitiallyDirty(true);
            };

            self.isDirty = ko.computed(function () {
                var lastCleanStateString = _lastCleanState();
                var jsonlastCleanState = $.parseJSON(lastCleanStateString);
                var currentObjectTrack = $.parseJSON(hashFunction(_objectToTrack));
                try {
                    delete currentObjectTrack["AccomodationNotes"];
                    delete jsonlastCleanState["AccomodationNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                try {
                    delete currentObjectTrack["MealAllowanceNotes"];
                    delete jsonlastCleanState["MealAllowanceNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                try {
                    delete currentObjectTrack["TravelNotes"];
                    delete jsonlastCleanState["TravelNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                try {
                    delete currentObjectTrack["NoOfMilesNotes"];
                    delete jsonlastCleanState["NoOfMilesNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                try {
                    delete currentObjectTrack["ParkingNotes"];
                    delete jsonlastCleanState["ParkingNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                try {
                    delete currentObjectTrack["OtherNotes"];
                    delete jsonlastCleanState["OtherNotes"];
                } catch (err) {  //We can also throw from try block and catch it here
                    //nothing to do
                }
                _lastCleanState = ko.observable(hashFunction(jsonlastCleanState));
                return _isInitiallyDirty() || hashFunction(currentObjectTrack) !== _lastCleanState();
            });

            self.reset = function () {
                _lastCleanState(hashFunction(_objectToTrack));
                _isInitiallyDirty(false);
            };

            self.undo = function () {
                var source = JSON.parse(_lastCleanState());
                for (prop in source) {
                    if (_objectToTrack[prop]() && _objectToTrack[prop]() != source[prop]) {
                        _objectToTrack[prop](source[prop]);
                    }
                }
                _isInitiallyDirty(false);
            };
            return self;
        };

    return result;
};

//ko.dirtyFlag = function(root, isInitiallyDirty) {
//    var result = function() {},
//        _initialState = ko.observable(ko.toJSON(root)),
//        _isInitiallyDirty = ko.observable(isInitiallyDirty);

//    result.isDirty = ko.computed(function() {
//        return _isInitiallyDirty() || _initialState() !== ko.toJSON(root);
//    });

//    result.reset = function() {
//        _initialState(ko.toJSON(root));
//        _isInitiallyDirty(false);
//    };

//    return result;
//};
