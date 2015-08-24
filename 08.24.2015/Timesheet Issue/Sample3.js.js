
/// <reference path="~/Scripts/_references.js" />

(function (ts) {
    var TimesheetAssociateModel = function (model) {
        var self = this;

        self.AssociateName = model.AssociateName;
        self.ClientName = model.ClientName;
        self.ProjectName = model.ProjectName;
        self.Role = model.Role;
        self.RoleId = model.RoleId;
        self.WeekStartDate = model.WeekStartDate;
        self.WeekEndDate = model.WeekEndDate;
        self.Rate = model.Rate;
        self.Manager = model.Manager;
        self.IsAgencyOrUmbrella = model.IsAgencyOrUmbrella;
    };

    ts.TimesheetAssociateModel = TimesheetAssociateModel;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var timesheetHistory = function (model) {
        var self = this;
        self.HistoryDate = model.Date;
        self.Associate = model.Associate;
        self.Status = model.Status;
        self.Comment = model.Comment;
        self.Time = model.Time;
    };

    ts.TimesheetHistory = timesheetHistory;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var timesheetDocument = function (model) {
        var self = this;

        self.Title = model.Title;
        self.From = model.From;
        self.To = model.To;
        self.DocumentId = model.DocumentId;
        self.Pass = model.Pass;
    };

    ts.TimesheetDocument = timesheetDocument;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var tsModel = function (model) {
        var self = this;

        self.Status = ko.observable(model.Status);
        self.TimesheetId = model.TimesheetId;
        self.AssociateId = model.AssociateId;
        self.TimeSheetTypeId = model.TimeSheetTypeId;
        self.TimesheetApproverAssociateId = model.TimesheetApproverAssociateId;
        self.TimesheetApproverId = model.TimesheetApproverId;
        self.CurrentUserId = model.CurrentUserId;
        self.AvailableAttendences = model.AvailableAttendences;
        self.AccomodationLimit = ko.observable(model.AccomodationLimit);
        self.MealAllowanceLimit = model.MealAllowanceLimit;
        self.TravelLimit = model.TravelLimit;
        self.NoOfMilesLimit = model.NoOfMilesLimit;
        self.MileageLimit = model.MileageLimit;
        self.ParkingLimit = model.ParkingLimit;
        self.OtherLimit = model.OtherLimit;
        self.OneOfPaymentLimit = model.OneOfPaymentLimit;
        self.IncentiveDayMaxWorked = model.IncentiveDayMaxWorked;
        self.IncentiveDaysCountedAs = model.IncentiveDaysCountedAs;
        self.IncentiveDaysIn7 = model.IncentiveDaysIn7;
        self.OverTimePayAway = model.OverTimePayAway;
        self.OverTimePayRatio = model.OverTimePayRatio;
        self.OneOfPayment = model.OneOfPayment;
        self.OverProduction = model.OverProduction;
        self.WorkingHours = model.WorkingHours;
        self.OneofPaymentDate = model.OneofPaymentDate;
        self.Comment = model.Comment;
        self.AssociateRate = model.AssociateRate;
        self.IncentiveRate = model.IncentiveRate;

        self.ApproveNotAllowed = ko.computed(function () {
            if (self.AssociateId == self.CurrentUserId) { return true; }
            return false;
        });


        self.OvertimeVisibilty = ko.computed(function () {
            if (typeof (self.OverTimePayAway) == "undefined" || typeof (self.OverTimePayAway) == "object" || self.OverTimePayAway == null) {
                return false;
            } else {
                if (+self.OverTimePayAway > 0) {
                    return true;
                }

                return false;
            }
        });

        self.OverProductionVisibilty = ko.computed(function () {
            if (typeof (self.OverProduction) == "undefined" || typeof (self.OverProduction) == "object" || self.OverProduction == null) {
                return false;
            } else {
                if (+self.OverProduction > 0) {
                    return true;
                }
                return false;
            }
        });

        self.AccomodationVisibilty = ko.computed(function () {
            if (typeof (self.AccomodationLimit()) == "undefined" || typeof (self.AccomodationLimit()) == "object" || self.AccomodationLimit() == null) {
                return false;
            } else {
                if (+self.AccomodationLimit() > 0) {
                    return true;
                }

                return false;
            }
        });

        self.MealAllowanceVisibilty = ko.computed(function () {
            if (typeof (self.MealAllowanceLimit) == "undefined" || typeof (self.MealAllowanceLimit) == "object" || self.MealAllowanceLimit == null) {
                return false;
            } else {
                if (+self.MealAllowanceLimit > 0) {
                    return true;
                }

                return false;
            }
        });

        self.TravelVisibilty = ko.computed(function () {
            if (typeof (self.TravelLimit) == "undefined" || typeof (self.TravelLimit) == "object" || self.TravelLimit == null) {
                return false;
            } else {
                if (+self.TravelLimit > 0) {
                    return true;
                }

                return false;
            }
        });

        self.NoOfMilesVisibilty = ko.computed(function () {
            if (typeof (self.NoOfMilesLimit) == "undefined" || typeof (self.NoOfMilesLimit) == "object" || self.NoOfMilesLimit == null) {
                return false;
            } else {
                if (+self.NoOfMilesLimit > 0) {
                    return true;
                }

                return false;
            }
        });

        self.MileageVisibilty = ko.computed(function () {
            if (typeof (self.MileageLimit) == "undefined" || typeof (self.MileageLimit) == "object" || self.MileageLimit == null) {
                return false;
            } else {
                if (+self.MileageLimit > 0) {
                    return true;
                }
                return false;
            }
        });

        self.ParkingVisibilty = ko.computed(function () {
            if (typeof (self.ParkingLimit) == "undefined" || typeof (self.ParkingLimit) == "object" || self.ParkingLimit == null) {
                return false;
            } else {
                if (+self.ParkingLimit > 0) {
                    return true;
                }

                return false;
            }
        });

        self.OtherVisibilty = ko.computed(function () {
            if (typeof (self.OtherLimit) == "undefined" || typeof (self.OtherLimit) == "object" || self.OtherLimit == null) {
                return false;
            } else {
                if (+self.OtherLimit > 0) {
                    return true;
                }

                return false;
            }
        });

        function validateLimitKO(obj) {
            if (typeof (obj()) == "undefined" || typeof (obj()) == "object" || obj() == null) {
                return false;
            } else {
                if (+obj() > 0) {
                    return true;
                }

                return false;
            }
        }

        function validateLimit(obj) {
        }

        self.Documents = ko.observableArray(model.Documents, function (doc) {
            return new Timesheet.TimesheetDocument(doc);
        });

        self.History = ko.observableArray(model.History, function (history) {
            return new Timesheet.TimesheetHistory(history);
        });
        var OFP = false;
        var entries = ko.utils.arrayMap(model.Entries, function (item) {
            if (item.OneOfPayment > 0) {
                OFP = true;
            }
            return new Timesheet.TimesheetEntryModel(model, item);
        });

        self.Entries = ko.observableArray(entries);
        if (model.Status == "Submitted") {
            self.Enable = ko.observable(false);
        } else {
            self.Enable = ko.observable(true);
        }

        self.IncentiveEnable = function () {
            var output = (self.IncentiveDaysCountedAs != null && self.IncentiveDaysCountedAs == 5) || (self.IncentiveDayMaxWorked != null && self.IncentiveDayMaxWorked != 0) || (self.IncentiveDaysIn7 != null && self.IncentiveDaysIn7 != 0);
            return output;
        };
        if (self.TimeSheetTypeId == 3) {
            self.IncentiveDays = ko.observable(model.IncentiveDays).extend({ number: true });
        }
        else {
            self.IncentiveDays = ko.observable(model.IncentiveDays).extend({ min: 0, max: validateIncentiveDay(), number: true });

        }
        function validateIncentiveDay() {

            var maxIncentiveDays = 0;

            if (model.IncentiveDaysCountedAs != null && self.IncentiveDaysCountedAs == 5) {
                maxIncentiveDays++;
            }

            if (model.IncentiveDayMaxWorked != null && self.IncentiveDayMaxWorked != 0) {
                maxIncentiveDays += (model.IncentiveDayMaxWorked - 5);
            }

            if (model.IncentiveDaysIn7 != null && model.IncentiveDaysIn7 != 0) {
                maxIncentiveDays += 2;

            }

            return maxIncentiveDays;
        }

        self.OOPVisibilty = ko.computed(function () {
            if (typeof (self.OneofPaymentDate) == "undefined" || typeof (self.OneofPaymentDate) == "object" || self.OneofPaymentDate == null) {
                return false;
            }
            else {
                var OOPDate = new Date(parseInt(self.OneofPaymentDate.substr(6)));
                var tDate = new Date();
                var fromDay = new Date();
                var toDay = new Date();
                var tday = tDate.getDay();

                OOPDate.setHours(0, 0, 0, 0)
                fromDay.setDate(tday == 0 ? tDate.getDate() - 6 : tDate.getDate() - (7 - (7 - +tday) - 1));
                toDay.setDate(tday == 0 ? tDate.getDate() : tDate.getDate() + (7 - +tday));
                fromDay.setHours(0, 0, 0, 0);
                toDay.setHours(0, 0, 0, 0);

                var sparts = model.TimesheetAssociate.WeekStartDate.split("/");
                var startDate = new Date(Number(sparts[2]), Number(sparts[1]) - 1, Number(sparts[0]));

                var eparts = model.TimesheetAssociate.WeekEndDate.split("/");
                var endDate = new Date(Number(eparts[2]), Number(eparts[1]) - 1, Number(eparts[0]));

                if ((OOPDate >= startDate && OOPDate <= endDate) || OFP) {
                    return true;
                }
                //if (OOPDate >= fromDay && OOPDate <= toDay) {
                //    return true;
                //}
            }

            return false;
        });

        self.ValidationObject = ko.validation.group([self.Entries(), self.IncentiveDays]);

        self.IsTSValid = ko.computed(function () {
            var error = 0;
            var IncDay = ko.utils.unwrapObservable(ko.validation.group([self.IncentiveDays]));
            var m = ko.utils.arrayMap(self.Entries(), function (item) {
                var notValid = ko.utils.unwrapObservable(ko.validation.group(item));
                error += +notValid.length;
                return notValid.length;
            });
            error += IncDay.length;
            return !(error > 0);
        });

        self.showErrors = function () {
            var error = 0;
            var m = ko.utils.arrayMap(self.Entries(), function (item) {
                var notValid = ko.utils.unwrapObservable(ko.validation.group(item));

                item.errors.showAllMessages();
            });
        }
    };

    ts.TimesheetModel = tsModel;
}(window.Timesheet = window.Timesheet || {}));

ko.extenders.OmitZeroNumber = function (t, p) {
    var result = ko.computed({
        read: t,
        write: function (newValue) {
            var current = t(),
                roundingMultiplier = Math.pow(10, p),
                newValueAsNum = isNaN(newValue) ? 0 : parseFloat(+newValue),
                valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;
            if (valueToWrite !== current) {
                t(valueToWrite);
            } else {
                if (newValue !== current) {
                    t.notifySubscribers(valueToWrite);
                }
            }
        }
    }).extend({ notify: 'always' });

    result(t());

    return result;
};

(function (ts) {

    var tsEmodel = function (timesheet, model) {
        var self = this;
        self.TimeSheetEntryId = model.TimeSheetEntryId;
        self.DayId = model.DayId;
        self.Day = model.Day;
        self.Date = ko.observable(model.Date);
        self.IsDay = model.IsDay;
        self.DisableOnAttendence = ko.observable(true);
        self.IsWorkingDay = model.IsWorkingDay;
        self.AttendenceId = ko.observable(model.AttendenceId);
        self.Attendences = ko.observableArray(model.Attendences, function (attendence) {
            return new Timesheet.Attendences(attendence);
        });
        self.Hour = ko.observable(model.Hour).extend({ min: -24, max: 24, number: true });
        self.Minute = ko.observable(model.Minute).extend({ max: 60, number: true });
        self.ShowMe = function ShowMe(val, elem, event) {
            //var notes = elem.AccomodationNotes();
            var notes = val();

            if (notes == null) {
                $("#toolTip").text("");
            }
            else {
                $("#toolTip").text(notes);
                var x = event.pageX + 8;
                var y = event.pageY + 5;
                $('#toolTip').css({ 'top': y, 'left': x }).fadeIn('fast');
            }
        };
        self.HideMe = function HideMe(elem, event) {
            $('#toolTip').fadeOut('fast');
        };
        self.Absence = model.Absence;
        var requriedAttendanceIds = [1, 2, 4, 5, 6, 7];


        self.ActualHour = ko.observable(model.ActualHour).extend({
            min: timesheet.TimeSheetTypeId == 3 ? -24 : 0,
            max: 24,
            number: true, pattern: { message: 'Decimal places are not allowed.', params: '^(0|-?[0-9][0-9]*)$' },
            required: {
                onlyIf: function () {
                    return requriedAttendanceIds.indexOf(self.AttendenceId()) > -1 && model.TimeSheetTypeId == 1;
                }
            }
        });


        self.ActualMinute = ko.observable(model.ActualMinute).extend({
            min: timesheet.TimeSheetTypeId == 3 ? -59 : 0,
            max: 59,
            number: true,
            pattern: { message: 'Decimal places are not allowed.', params: '^(0|-?[0-9][0-9]*)$' } //^[0-9]{1,3}(\\.[0-9]{1,2})?$

        });

        self.ActualOvertimeHourCOM = ko.computed(function () {
            try {
                var value = (model.OverTime == "" || model.OverTime == null ? "" : Math.floor(model.OverTime));
                return (value == 0 ? "" : value);
            }
            catch (e) {
                console.dir(e);
                return "";
            }
        }).extend({ notify: 'always', number: true });

        self.ActualOvertimeMinuteCOM = ko.computed(function () {
            try {
                if (model.OverTime == "" || model.OverTime == null) {
                    return "";
                }
                else {
                    // var value = model.OverTime % 1;
                    //  var overtimeMin = model.OverTime % 1;
                    //  var value = Math.floor(overtimeMin * 60);
                    var index = model.OverTime.toString().indexOf(".");

                    if (index != -1) {
                        var value = model.OverTime.toString().substring(index + 1, model.OverTime.toString().length);
                        //var value = Math.floor((model.OverTime % 1) * 100);
                    }
                    return (value == 0 ? "" : value);
                }

            }
            catch (e) {
                console.dir(e);
                return "";
            }
        });


        self.ActualOvertimeHour = ko.observable(self.ActualOvertimeHourCOM()).extend({
            min: timesheet.TimeSheetTypeId == 3 ? -24 : 0,
            max: 24,
            number: true
        });


        var overtimeComp = self.ActualOvertimeMinuteCOM();

        self.ActualOvertimeMinute = ko.observable(overtimeComp).extend({
            min: timesheet.TimeSheetTypeId == 3 ? -59 : 0,
            max: 59,
            number: true
        });

        self.UpdateOverTime = function (data, event) {
            var hours = parseInt($(event.currentTarget).parent().find(".overtime-hour").val()) || 0;
            var mins = $(event.currentTarget).parent().find(".overtime-minute").val() || 0;

            var valstring = hours.toString() + "." + mins.toString();

            if (mins > 59) {
                // Alert("Minutes must be between 0 and 59.", "Overtime");
                // $(event.currentTarget).parent().find(".overtime-minute").val(59)
                //  mins = 59;
            }
            //var overtime = hours + (mins == 0 ? 0 : (mins / 60.0));
            var overtime = +valstring;
            $(event.currentTarget).parent().find(".actual-overtime").val(overtime);
            self.OverTime = overtime;
            self.OverTimecal(overtime);
            return true;
        };

        self.OverTime = ko.observable(model.OverTime);
        self.OverTimecal = ko.observable(model.OverTime);
        self.IsOvertime = ko.observable(model.IsOvertime);


        self.OvertimeOnNonWD = function () {
            if (!self.IsWorkingDay) {
                if (self.IsOvertime()) {
                    return false;
                }
            }
            return true;
        }
        self.OverTimeEnabe = ko.observable(false);
        self.IsOneOfPayment = ko.computed(function () {
            if (self.Date() == null || timesheet.OneofPaymentDate == null) {
                return false;
            }
            var dateParts = self.Date().split("/");
            var date = new Date(dateParts[2], (dateParts[1] - 1), dateParts[0]);
            var ofpd = new Date(parseInt(timesheet.OneofPaymentDate.substr(6)));
            return (date - ofpd == 0);
            // Date.parse(self.Date) == Date.parse(timesheet.OneofPaymentDate);         
        });
        self.TimesheetId = model.TimesheetId;
        if (timesheet.TimeSheetTypeId == 3) {
            if (model.OverProduction == null) {
                self.OverProduction = ko.observable(model.OverProduction).extend({ number: true, pattern: { message: 'Decimals are not allowed', params: '^(0|-?[0-9]{1,3})$', onlyIf: function () { return timesheet.TimeSheetTypeId != 2; } } });
            } else {
                self.OverProduction = ko.observable(model.OverProduction.toString().split(".")[0]).extend({ number: true, pattern: { message: 'Decimals are not allowed', params: '^(0|-?[0-9]{1,3})$', onlyIf: function () { return timesheet.TimeSheetTypeId != 2; } } });
            }

            self.OneOfPayment = ko.observable(model.OneOfPayment).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.Accomodation = ko.observable(model.Accomodation).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.AccomodationNotes = ko.observable(model.AccomodationNotes).extend({ maxLength: 200 });
            self.AssociateRate = ko.observable(model.AssociateRate);
            self.MealAllowance = ko.observable(model.MealAllowance).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } }); //, pattern: { message: 'Only numbers are allowed.', params: '^(0|[1-9][0-9]*)$' } });
            self.MealAllowanceNotes = ko.observable(model.MealAllowanceNotes).extend({ maxLength: 200 });
            self.Travel = ko.observable(model.Travel).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.TravelNotes = ko.observable(model.TravelNotes).extend({ maxLength: 200 });
            self.NoOfMiles = ko.observable(model.NoOfMiles).extend({ number: true, pattern: { message: 'No decimal places are allowed.', params: '^(0|[1-9][0-9]*)$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.NoOfMilesNotes = ko.observable(model.NoOfMilesNotes).extend({ maxLength: 200 });
            self.Mileage = ko.computed(function () {
                return (self.NoOfMiles() * timesheet.MileageLimit) / 100;
            });
            self.Parking = ko.observable(model.Parking).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.ParkingNotes = ko.observable(model.ParkingNotes).extend({ maxLength: 200 });
            self.Other = ko.observable(model.Other).extend({ number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.OtherNotes = ko.observable(model.OtherNotes).extend({ maxLength: 200 });
        }
        else {
            if (model.OverProduction == null) {
                self.OverProduction = ko.observable(model.OverProduction).extend({ max: 100, number: true, pattern: { message: 'Decimals are not allowed', params: '^(0|-?[0-9]{1,3})$', onlyIf: function () { return timesheet.TimeSheetTypeId != 2; } } });
            } else {
                self.OverProduction = ko.observable(model.OverProduction.toString().split(".")[0]).extend({ max: 100, number: true, pattern: { message: 'Decimals are not allowed', params: '^(0|-?[0-9]{1,3})$', onlyIf: function () { return timesheet.TimeSheetTypeId != 2; } } });
            }

            self.OneOfPayment = ko.observable(model.OneOfPayment).extend({ max: timesheet.OneOfPaymentLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.Accomodation = ko.observable(model.Accomodation).extend({ max: timesheet.AccomodationLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.AccomodationNotes = ko.observable(model.AccomodationNotes).extend({ maxLength: 200 });
            self.AssociateRate = ko.observable(model.AssociateRate);
            self.MealAllowance = ko.observable(model.MealAllowance).extend({ max: timesheet.MealAllowanceLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } }); //, pattern: { message: 'Only numbers are allowed.', params: '^(0|[1-9][0-9]*)$' } });
            self.MealAllowanceNotes = ko.observable(model.MealAllowanceNotes).extend({ maxLength: 200 });
            self.Travel = ko.observable(model.Travel).extend({ max: timesheet.TravelLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.TravelNotes = ko.observable(model.TravelNotes).extend({ maxLength: 200 });
            self.NoOfMiles = ko.observable(model.NoOfMiles).extend({ number: true, pattern: { message: 'No decimal places are allowed.', params: '^(0|[1-9][0-9]*)$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.NoOfMilesNotes = ko.observable(model.NoOfMilesNotes).extend({ maxLength: 200 });
            self.Mileage = ko.computed(function () {
                return (self.NoOfMiles() * timesheet.MileageLimit) / 100;
            });
            self.Parking = ko.observable(model.Parking).extend({ max: timesheet.ParkingLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.ParkingNotes = ko.observable(model.ParkingNotes).extend({ maxLength: 200 });
            self.Other = ko.observable(model.Other).extend({ max: timesheet.OtherLimit, number: true, pattern: { message: 'Only 2 decimal places are allowed.', params: '^[0-9]{1,3}(\\.[0-9]{1,2})?$', onlyIf: function () { return timesheet.TimeSheetTypeId == 1; } } });
            self.OtherNotes = ko.observable(model.OtherNotes).extend({ maxLength: 200 });

        }
        self.OverTimeComputed = ko.computed(function () {
            return self.OverTimecal();
        });
        self.IsModelValid = ko.computed(function () {
            var notValid = ko.utils.unwrapObservable(ko.validation.group(self));
            return (notValid.length > 0);
        });
        self.EnableH = ko.observable();
        self.EnableHour = ko.computed(function (data) {
            var actHour = self.ActualHour();
            var actMin = self.ActualMinute();
            if (self.AttendenceId() != 3) {
                switch (self.AttendenceId()) {
                    case AttendanceOption.FullDay:
                    case AttendanceOption.TravelFullDay:
                        self.Hour(Math.floor(+timesheet.WorkingHours));
                        self.Minute((+timesheet.WorkingHours % 1) * 60);
                        if (self.IsOvertime()) {
                            self.OverTimeEnabe(true);
                        }

                        break;

                    case AttendanceOption.HalfDay:
                    case AttendanceOption.UnpaidHalfDay:
                    case AttendanceOption.TravelHalfDay:
                        var hours = +timesheet.WorkingHours / 2;
                        self.Hour(Math.floor(hours));
                        self.Minute((+hours % 1) * 60);
                        if (self.IsOvertime()) {
                            self.OverTimeEnabe(true);
                        }
                        break;
                    case AttendanceOption.Overtime:
                        self.OverTimeEnabe(true);
                        break;

                    case AttendanceOption.Hourly:
                    case AttendanceOption.NegativeFullDay:
                    case AttendanceOption.NegativeHalfDay:
                    case AttendanceOption.Adjustment:

                        break;

                    default:
                        self.Hour("");
                        self.Minute("");
                        if (actHour == 0 || actHour == null) {
                            self.ActualHour("");
                        }

                        if (actMin == 0 || actMin == null) {
                            self.ActualMinute("");
                        }
                        break;
                }

                self.EnableH(false);

                return true;
            }

            var hour = self.ActualHour();
            var min = self.ActualMinute();

            self.Minute(+min);
            self.Hour(+hour);
            self.EnableH(false);

            return true;
        });
        self.EnableAbsense = ko.computed(function (e) {
            if (self.Absence == 'None' || self.Absence == "undefined" || typeof (self.Absence) == "object" || self.Absence == null || self.Absence == 'AMOnly' || self.Absence == 'PMOnly') {
                return true;
            }

            return false;
        });
        self.AttendenceId.subscribe(function (e, d) {
        //    alert(self.AttendenceId());
            // if (self.AttendenceId() != 3) {
            switch (self.AttendenceId()) {
                case AttendanceOption.FullDay:
                case AttendanceOption.TravelFullDay:
                    self.DisableOnAttendence(true);
                    self.OverTimeEnabe(false);
                    if (self.IsOvertime()) {
                        self.OverTimeEnabe(true);
                    }
                    break;
                case AttendanceOption.CancelFullDay:
                case AttendanceOption.CancelHalfDay:
                    self.DisableOnAttendence(false);
                    self.OverTimeEnabe(false);

                    break;
                case AttendanceOption.HalfDay:
                case AttendanceOption.UnpaidHalfDay:
                case AttendanceOption.TravelHalfDay:
                    self.DisableOnAttendence(true);
                    self.OverTimeEnabe(false);
                    if (self.IsOvertime()) {
                        self.OverTimeEnabe(true);
                    }
                    break;

                case AttendanceOption.Hourly:
                    self.DisableOnAttendence(true);
                    self.OverTimeEnabe(false);
                    break;

                case AttendanceOption.NotSelected:
                case AttendanceOption.SickDay:
                case AttendanceOption.PlannedAbsence:
                case AttendanceOption.UnplannedAbsence:
                    self.DisableOnAttendence(false);
                    self.OverTimeEnabe(false);
                    ClearRow();
                    break;
                case AttendanceOption.Overtime:
                    self.DisableOnAttendence(false);
                    self.OverTimeEnabe(true);
                    break;

                case AttendanceOption.NegativeFullDay:
                case AttendanceOption.NegativeHalfDay:
                case AttendanceOption.Adjustment:
                    break;

                default:
                    self.DisableOnAttendence(true);
                    self.OverTimeEnabe(false);
                    self.ActualHour("");
                    self.ActualMinute("");
                    break;
            }
            // }
        });
        self.AttendenceId.valueHasMutated();
        self.IsDirtyFlag = new ko.dirtyFlag(self);

        /*self.TravelNotes.subscribe(function (e, d) {
            self.TravelNotes =
            SaveEntryNote();
        });

        self.AccomodationNotes.subscribe(function (e, d) {
            SaveEntryNote();
        });
        self.MealAllowanceNotes.subscribe(function (e, d) {
            SaveEntryNote();
        });
        self.ParkingNotes.subscribe(function (e, d) {
            SaveEntryNote();
        });
        self.OtherNotes.subscribe(function (e, d) {
            SaveEntryNote();
        });

        self.MealAllowanceNotes.subscribe(function (e, d) {
            SaveEntryNote();
        });*/


        function SaveEntryNote() {
            $.ajax({
                type: 'POST',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                url: '/TimeSheet/SaveTimesheetEntryNote',
                cache: false,
                data: JSON.stringify({ id: self.TimeSheetEntryId, accomodation: self.AccomodationNotes(), mealAllowance: self.MealAllowanceNotes(), travel: self.TravelNotes(), noOfMiles: self.NoOfMilesNotes(), parkingNotes: self.ParkingNotes(), other: self.OtherNotes() }),
                success: function (data, success) {
                },
                error: function (data, error) {
                    //console.log(data);
                    //console.log(error);
                }
            });
        }

        function ClearRow() {
            self.ActualHour("");
            self.ActualMinute("");
            self.OverTime("");
            self.OverProduction("");
            self.OneOfPayment("");
            self.Accomodation("");
            self.AccomodationNotes("");
            self.MealAllowance("");
            self.MealAllowanceNotes("");
            self.Travel("");
            self.TravelNotes("");
            self.NoOfMiles("");
            self.NoOfMilesNotes("");
            self.Parking("");
            self.ParkingNotes("");
            self.Other("");
            self.OtherNotes("");
        }
    };

    ts.TimesheetEntryModel = tsEmodel;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var attendences = function (attendence) {
        var self = this;

        self.AttendenceOptionId = attendence.AttendenceOptionId;
        self.AttendenceOption = attendence.AttendenceOption;
    }

    ts.Attendences = attendences;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var dbTimesheetModel = function (model) {
        var self = this;

        self.ProjectName = ko.observable(model.ProjectName);
        self.StartDate = ko.observable(model.StartDate);
        self.EndDate = ko.observable(model.EndDate);
        self.Role = ko.observable(model.Role);
        self.Status = ko.observable(model.Status);
        self.TimeSheetId = ko.observable(model.TimeSheetId);
        self.TimesheetTypeId = ko.observable(model.TimesheetTypeId);
        self.StatusLink = ko.computed(function () {
            if (self.Status() == "Approved") { return "View"; }
            else { return "Edit"; }
        });
    };

    ts.dbTimesheetModel = dbTimesheetModel;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var dbAwaitingTimesheetModel = function (model) {
        var self = this;

        self.TimeSheetId = model.TimeSheetId;
        self.ProjectName = ko.observable(model.ProjectName);
        self.StartDate = ko.observable(model.StartDate);
        self.EndDate = ko.observable(model.EndDate);
        self.Associate = ko.observable(model.Associate);
        self.Status = ko.observable(model.Status);
        self.TimesheetTypeId = ko.observable(model.TimesheetTypeId);
    };

    ts.dbAwaitingTimesheetModel = dbAwaitingTimesheetModel;
}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var dbCompanyInvoiceModel = function (model) {
        var self = this;

        self.ProjectName = ko.observable(model.ProjectName);
        self.StartDate = ko.observable(model.StartDate);
        self.EndDate = ko.observable(model.EndDate);
        self.Role = ko.observable(model.Role);
        self.Status = ko.observable(model.Status);
        self.ReportNumber = ko.observable(model.ReportNumber);
    };

    ts.dbCompanyInvoiceModel = dbCompanyInvoiceModel;

}(window.Timesheet = window.Timesheet || {}));

(function (ts) {
    var loggedInUserModel = function (model) {
        var self = this;

        self.LoggedInUser = Name;
    };

    ts.LoggedInUser = loggedInUserModel;

}(window.Timesheet = window.Timesheet || {}));

ko.bindingHandlers.AttedenceDisable = {
    init: function (elem, v, a, vm, bc) {
        if (bc.$parent.Enable() && vm.IsDay && ko.utils.unwrapObservable(v()) && bc.$data.EnableAbsense()) {
            $(elem).prop('disabled', false);
        } else {
            $(elem).prop('disabled', true);
            //$(elem).parent().parent().find("input,button,select").prop("disabled", true);
        }
    },
    update: function (elem, v, a, vm, bc) {
        if (bc.$parent.Enable() && vm.IsDay && ko.utils.unwrapObservable(v()) && bc.$data.EnableAbsense()) {
            $(elem).prop('disabled', false);
        } else {
            $(elem).prop('disabled', true);
            //$(elem).parent().parent().find("input,button,select").prop("disabled", true);
        }
    }
};

ko.validation.rules['validateOvertime'] = {
    validator: function (value, otherValue) {
        var result = value < otherValue;

        return result;
    },
    message: 'OverTime Validation Failed {0}'
};