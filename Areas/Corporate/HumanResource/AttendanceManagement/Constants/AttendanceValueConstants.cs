namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants
{
    public static class AttendanceValueConstants
    {
        public static class RawLogEventType
        {
            public const string CheckIn = "CheckIn";
            public const string CheckOut = "CheckOut";
            public const string BreakStart = "BreakStart";
            public const string BreakEnd = "BreakEnd";
            public const string Unknown = "Unknown";
        }

        public static class RawLogSourceType
        {
            public const string Device = "Device";
            public const string Mobile = "Mobile";
            public const string WebLogin = "WebLogin";
            public const string Import = "Import";
            public const string Integration = "Integration";
            public const string Manual = "Manual";
        }

        public static class RawLogProcessingStatus
        {
            public const string Pending = "Pending";
            public const string Matched = "Matched";
            public const string Processed = "Processed";
            public const string Duplicate = "Duplicate";
            public const string Rejected = "Rejected";
            public const string Error = "Error";
        }

        public static class AttendanceStatus
        {
            public const string Unprocessed = "Unprocessed";
            public const string Present = "Present";
            public const string Absent = "Absent";
            public const string Late = "Late";
            public const string EarlyLeave = "EarlyLeave";
            public const string Incomplete = "Incomplete";
            public const string Holiday = "Holiday";
            public const string RestDay = "RestDay";
            public const string Leave = "Leave";
            public const string BusinessTrip = "BusinessTrip";
            public const string Remote = "Remote";
        }

        public static class AttendanceProcessingStatus
        {
            public const string Pending = "Pending";
            public const string Processing = "Processing";
            public const string Processed = "Processed";
            public const string ReprocessRequired = "ReprocessRequired";
            public const string Skipped = "Skipped";
            public const string Error = "Error";
        }

        public static class PayrollInputStatus
        {
            public const string Pending = "Pending";
            public const string Ready = "Ready";
            public const string Processed = "Processed";
            public const string Blocked = "Blocked";
            public const string Excluded = "Excluded";
        }

        public static class ScheduleSource
        {
            public const string PublishedRoster = "PublishedRoster";
            public const string ConfirmedRoster = "ConfirmedRoster";
            public const string CompletedRoster = "CompletedRoster";
            public const string FixedWorkSchedule = "FixedWorkSchedule";
            public const string RemoteAttendance = "RemoteAttendance";
            public const string BusinessTrip = "BusinessTrip";
            public const string ManualOverride = "ManualOverride";
            public const string Unresolved = "Unresolved";
        }

        public static class AttendanceSegmentType
        {
            public const string Work = "Work";
            public const string Break = "Break";
            public const string OnCall = "OnCall";
            public const string Overtime = "Overtime";
            public const string Remote = "Remote";
            public const string BusinessTrip = "BusinessTrip";
        }

        public static class AttendanceSegmentSource
        {
            public const string Processor = "Processor";
            public const string Roster = "Roster";
            public const string ManualCorrection = "ManualCorrection";
            public const string RemoteAttendance = "RemoteAttendance";
            public const string BusinessTrip = "BusinessTrip";
        }

        public static class AttendanceSegmentStatus
        {
            public const string Pending = "Pending";
            public const string Calculated = "Calculated";
            public const string Corrected = "Corrected";
            public const string Invalid = "Invalid";
            public const string Cancelled = "Cancelled";
        }

        public static class AttendanceExceptionType
        {
            public const string Late = "Late";
            public const string EarlyLeave = "EarlyLeave";
            public const string MissingCheckIn = "MissingCheckIn";
            public const string MissingCheckOut = "MissingCheckOut";
            public const string Absent = "Absent";
            public const string OutsideGeofence = "OutsideGeofence";
            public const string DuplicatePunch = "DuplicatePunch";
            public const string ScheduleMismatch = "ScheduleMismatch";
            public const string ScheduleConflict = "ScheduleConflict";
            public const string ExcessiveWorkHours = "ExcessiveWorkHours";
            public const string Unknown = "Unknown";
        }

        public static class AttendanceExceptionStatus
        {
            public const string Open = "Open";
            public const string UnderReview = "UnderReview";
            public const string Corrected = "Corrected";
            public const string Waived = "Waived";
            public const string Rejected = "Rejected";
            public const string Closed = "Closed";
        }

        public static class AttendanceExceptionSeverity
        {
            public const string Info = "Info";
            public const string Warning = "Warning";
            public const string High = "High";
            public const string Critical = "Critical";
        }

        public static class CorrectionType
        {
            public const string AttendanceTime = "AttendanceTime";
            public const string MissingPunch = "MissingPunch";
            public const string Location = "Location";
            public const string Schedule = "Schedule";
            public const string Status = "Status";
            public const string BusinessTrip = "BusinessTrip";
            public const string RemoteAttendance = "RemoteAttendance";
            public const string Other = "Other";
        }

        public static class CorrectionRequestStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string UnderReview = "UnderReview";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string PartiallyApproved = "PartiallyApproved";
            public const string Rejected = "Rejected";
            public const string Applied = "Applied";
            public const string Cancelled = "Cancelled";
        }

        public static class ShiftAssignmentStatus
        {
            public const string Draft = "Draft";
            public const string Validated = "Validated";
            public const string Published = "Published";
            public const string Confirmed = "Confirmed";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Replaced = "Replaced";
        }

        public static class ProcessingRunMode
        {
            public const string Batch = "Batch";
            public const string Reprocess = "Reprocess";
            public const string SingleWorkforce = "SingleWorkforce";
            public const string SingleDate = "SingleDate";
        }

        public static class ProcessingRunStatus
        {
            public const string Pending = "Pending";
            public const string Running = "Running";
            public const string Completed = "Completed";
            public const string CompletedWithErrors = "CompletedWithErrors";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
        }

        public static class ProcessingTriggerSource
        {
            public const string Manual = "Manual";
            public const string Scheduler = "Scheduler";
            public const string Api = "Api";
            public const string System = "System";
        }


        public static class AttendancePeriodStatus
        {
            public const string Open = "Open";
            public const string Closing = "Closing";
            public const string Closed = "Closed";
            public const string Reopened = "Reopened";
            public const string Cancelled = "Cancelled";
        }

        public static class AttendanceSchedulerJobType
        {
            public const string ProcessRange = "ProcessRange";
            public const string ReprocessRange = "ReprocessRange";
        }

        public static class AttendanceSchedulerJobStatus
        {
            public const string Pending = "Pending";
            public const string Running = "Running";
            public const string Completed = "Completed";
            public const string CompletedWithErrors = "CompletedWithErrors";
            public const string RetryScheduled = "RetryScheduled";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
        }
    }
}
