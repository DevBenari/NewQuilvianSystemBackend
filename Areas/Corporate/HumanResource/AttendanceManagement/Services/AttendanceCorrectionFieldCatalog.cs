using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    internal sealed class AttendanceCorrectionFieldDefinition
    {
        public string FieldName { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string DataType { get; init; } = string.Empty;
        public string[] AllowedCorrectionTypes { get; init; } = Array.Empty<string>();
    }

    internal static class AttendanceCorrectionFieldCatalog
    {
        private static readonly IReadOnlyDictionary<string, AttendanceCorrectionFieldDefinition>
            Definitions = BuildDefinitions();

        public static IReadOnlyCollection<AttendanceCorrectionFieldDefinition> All =>
            Definitions.Values.OrderBy(x => x.Label).ToList();

        public static bool TryGet(
            string? fieldName,
            out AttendanceCorrectionFieldDefinition definition)
        {
            definition = null!;
            return !string.IsNullOrWhiteSpace(fieldName) &&
                   Definitions.TryGetValue(fieldName.Trim(), out definition!);
        }

        public static List<AttendanceCorrectionFieldOptionResponse> ToOptions() =>
            All.Select(x => new AttendanceCorrectionFieldOptionResponse
            {
                FieldName = x.FieldName,
                Label = x.Label,
                DataType = x.DataType,
                AllowedCorrectionTypes = x.AllowedCorrectionTypes.ToList()
            }).ToList();

        public static bool IsAllowedForCorrectionType(
            AttendanceCorrectionFieldDefinition definition,
            string correctionType)
        {
            return definition.AllowedCorrectionTypes.Any(x =>
                string.Equals(x, correctionType, StringComparison.OrdinalIgnoreCase));
        }

        public static string? GetValue(
            TrxAttendanceDaily daily,
            string fieldName)
        {
            return fieldName switch
            {
                "FirstCheckInAt" => FormatDateTime(daily.FirstCheckInAt),
                "LastCheckOutAt" => FormatDateTime(daily.LastCheckOutAt),
                "ScheduledCheckInAt" => FormatDateTime(daily.ScheduledCheckInAt),
                "ScheduledCheckOutAt" => FormatDateTime(daily.ScheduledCheckOutAt),
                "AttendanceStatus" => daily.AttendanceStatus,
                "BreakMinutes" => daily.BreakMinutes.ToString(CultureInfo.InvariantCulture),
                "ActualWorkMinutes" => daily.ActualWorkMinutes.ToString(CultureInfo.InvariantCulture),
                "PayableWorkMinutes" => daily.PayableWorkMinutes.ToString(CultureInfo.InvariantCulture),
                "LateMinutes" => daily.LateMinutes.ToString(CultureInfo.InvariantCulture),
                "EarlyLeaveMinutes" => daily.EarlyLeaveMinutes.ToString(CultureInfo.InvariantCulture),
                "OvertimeMinutes" => daily.OvertimeMinutes.ToString(CultureInfo.InvariantCulture),
                "IsPresent" => daily.IsPresent.ToString(),
                "IsAbsent" => daily.IsAbsent.ToString(),
                "IsLate" => daily.IsLate.ToString(),
                "IsEarlyLeave" => daily.IsEarlyLeave.ToString(),
                "HasMissingPunch" => daily.HasMissingPunch.ToString(),
                "IsBusinessTrip" => daily.IsBusinessTrip.ToString(),
                "IsRemoteAttendance" => daily.IsRemoteAttendance.ToString(),
                "WorkScheduleId" => daily.WorkScheduleId?.ToString(),
                "ShiftId" => daily.ShiftId?.ToString(),
                _ => null
            };
        }

        public static string? ValidateRequestedValue(
            AttendanceCorrectionFieldDefinition definition,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (definition.DataType is "DateTime" or "Guid")
                {
                    return null;
                }

                return $"Nilai field {definition.Label} wajib diisi.";
            }

            var normalized = value.Trim();

            return definition.DataType switch
            {
                "DateTime" => DateTimeOffset.TryParse(
                    normalized,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                    out _)
                        ? null
                        : $"Nilai field {definition.Label} harus berupa tanggal dan waktu ISO 8601.",
                "Integer" => int.TryParse(
                    normalized,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var number) && number >= 0
                        ? null
                        : $"Nilai field {definition.Label} harus berupa angka 0 atau lebih besar.",
                "Boolean" => bool.TryParse(normalized, out _)
                        ? null
                        : $"Nilai field {definition.Label} harus berupa true atau false.",
                "Guid" => Guid.TryParse(normalized, out _)
                        ? null
                        : $"Nilai field {definition.Label} harus berupa GUID yang valid.",
                "String" when definition.FieldName == "AttendanceStatus" &&
                              !GetAttendanceStatuses().Contains(normalized, StringComparer.OrdinalIgnoreCase)
                        => "Attendance status tidak termasuk nilai yang didukung.",
                _ => null
            };
        }

        public static string GetLabel(string fieldName) =>
            TryGet(fieldName, out var definition)
                ? definition.Label
                : fieldName;

        public static IReadOnlyCollection<string> GetAttendanceStatuses() =>
            new[]
            {
                AttendanceValueConstants.AttendanceStatus.Unprocessed,
                AttendanceValueConstants.AttendanceStatus.Present,
                AttendanceValueConstants.AttendanceStatus.Absent,
                AttendanceValueConstants.AttendanceStatus.Late,
                AttendanceValueConstants.AttendanceStatus.EarlyLeave,
                AttendanceValueConstants.AttendanceStatus.Incomplete,
                AttendanceValueConstants.AttendanceStatus.Holiday,
                AttendanceValueConstants.AttendanceStatus.RestDay,
                AttendanceValueConstants.AttendanceStatus.Leave,
                AttendanceValueConstants.AttendanceStatus.BusinessTrip,
                AttendanceValueConstants.AttendanceStatus.Remote
            };

        private static IReadOnlyDictionary<string, AttendanceCorrectionFieldDefinition>
            BuildDefinitions()
        {
            var attendanceTime = AttendanceValueConstants.CorrectionType.AttendanceTime;
            var missingPunch = AttendanceValueConstants.CorrectionType.MissingPunch;
            var schedule = AttendanceValueConstants.CorrectionType.Schedule;
            var location = AttendanceValueConstants.CorrectionType.Location;
            var status = AttendanceValueConstants.CorrectionType.Status;
            var businessTrip = AttendanceValueConstants.CorrectionType.BusinessTrip;
            var remote = AttendanceValueConstants.CorrectionType.RemoteAttendance;
            var other = AttendanceValueConstants.CorrectionType.Other;

            var definitions = new[]
            {
                Define("FirstCheckInAt", "Waktu check-in", "DateTime", attendanceTime, missingPunch, other),
                Define("LastCheckOutAt", "Waktu check-out", "DateTime", attendanceTime, missingPunch, other),
                Define("ScheduledCheckInAt", "Jadwal masuk", "DateTime", schedule, other),
                Define("ScheduledCheckOutAt", "Jadwal pulang", "DateTime", schedule, other),
                Define("AttendanceStatus", "Status kehadiran", "String", status, businessTrip, remote, other),
                Define("BreakMinutes", "Durasi istirahat", "Integer", attendanceTime, other),
                Define("ActualWorkMinutes", "Durasi kerja aktual", "Integer", attendanceTime, other),
                Define("PayableWorkMinutes", "Durasi dibayar", "Integer", attendanceTime, location, status, other),
                Define("LateMinutes", "Menit keterlambatan", "Integer", attendanceTime, status, other),
                Define("EarlyLeaveMinutes", "Menit pulang awal", "Integer", attendanceTime, status, other),
                Define("OvertimeMinutes", "Menit lembur", "Integer", attendanceTime, other),
                Define("IsPresent", "Hadir", "Boolean", location, status, businessTrip, remote, other),
                Define("IsAbsent", "Tidak hadir", "Boolean", location, status, other),
                Define("IsLate", "Terlambat", "Boolean", attendanceTime, status, other),
                Define("IsEarlyLeave", "Pulang awal", "Boolean", attendanceTime, status, other),
                Define("HasMissingPunch", "Punch belum lengkap", "Boolean", attendanceTime, missingPunch, location, status, other),
                Define("IsBusinessTrip", "Perjalanan dinas", "Boolean", businessTrip, status, other),
                Define("IsRemoteAttendance", "Kehadiran remote", "Boolean", remote, status, other),
                Define("WorkScheduleId", "Work schedule", "Guid", schedule, other),
                Define("ShiftId", "Shift", "Guid", schedule, other)
            };

            return definitions.ToDictionary(
                x => x.FieldName,
                x => x,
                StringComparer.OrdinalIgnoreCase);
        }

        private static AttendanceCorrectionFieldDefinition Define(
            string fieldName,
            string label,
            string dataType,
            params string[] correctionTypes)
        {
            return new AttendanceCorrectionFieldDefinition
            {
                FieldName = fieldName,
                Label = label,
                DataType = dataType,
                AllowedCorrectionTypes = correctionTypes
            };
        }

        private static string? FormatDateTime(DateTime? value) =>
            value?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }
}
