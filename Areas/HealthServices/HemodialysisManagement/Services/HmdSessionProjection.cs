using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Proyeksi ringkas satu sesi, dipakai bersama service penjadwalan, pelaksanaan, dan finalisasi
    /// supaya ketiganya mengembalikan bentuk <see cref="HmdSessionResponse"/> yang sama persis.
    /// </summary>
    public static class HmdSessionProjection
    {
        public static IQueryable<HmdSessionResponse> Project(IQueryable<HmdSession> rows) =>
            rows.Select(x => new HmdSessionResponse
            {
                Id = x.Id,
                SessionNumber = x.SessionNumber,
                EpisodeId = x.EpisodeId,
                EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                PatientId = x.Episode != null ? x.Episode.PatientId : Guid.Empty,
                PatientName = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.FullName : null,
                MedicalRecordNumber = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.MedicalRecordNumber : null,
                PrescriptionId = x.PrescriptionId,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                InpEpisodeId = x.InpEpisodeId,
                OrderId = x.OrderId,
                MachineId = x.MachineId,
                MachineCode = x.Machine != null ? x.Machine.MachineCode : null,
                StationId = x.StationId,
                StationCode = x.Station != null ? x.Station.StationCode : null,
                ResponsibleDoctorId = x.ResponsibleDoctorId,
                ResponsibleDoctorName = x.ResponsibleDoctor != null ? x.ResponsibleDoctor.FullName : null,
                ScheduledDate = x.ScheduledDate,
                Shift = x.Shift,
                ScheduledStartAt = x.ScheduledStartAt,
                ScheduledEndAt = x.ScheduledEndAt,
                CheckedInAt = x.CheckedInAt,
                ReadyAt = x.ReadyAt,
                StartedAt = x.StartedAt,
                StartedByUserId = x.StartedByUserId,
                EndedAt = x.EndedAt,
                ActualDurationMinutes = x.ActualDurationMinutes,
                ActualUltrafiltrationMl = x.ActualUltrafiltrationMl,
                SessionStatus = x.SessionStatus,
                HoldReason = x.HoldReason,
                StopReason = x.StopReason,
                Disposition = x.Disposition,
                DocumentedByUserId = x.DocumentedByUserId,
                DocumentedAt = x.DocumentedAt,
                SignedByUserId = x.SignedByUserId,
                SignedAt = x.SignedAt,
                ReturnReason = x.ReturnReason,
                CancelReason = x.CancelReason,
                PatientProcedureId = x.PatientProcedureId,
                BillingHandoffStatus = x.BillingHandoffStatus
            });

        public static T Label<T>(T row) where T : HmdSessionResponse
        {
            row.ShiftName = HmdLabels.Shift(row.Shift);
            row.SessionStatusName = HmdLabels.SessionStatus(row.SessionStatus);
            row.StopReasonName = row.StopReason.HasValue ? HmdLabels.StopReason(row.StopReason.Value) : null;
            row.DispositionName = row.Disposition.HasValue ? HmdLabels.Disposition(row.Disposition.Value) : null;
            row.BillingHandoffStatusName = HmdLabels.BillingHandoff(row.BillingHandoffStatus);
            row.IsLocked = row.SessionStatus == HmdSessionStatus.Finalized;
            return row;
        }

        /// <summary>Status sesi yang masih boleh diubah jadwal, sumber daya, maupun petugasnya.</summary>
        public static readonly HmdSessionStatus[] ReschedulableStatuses =
        [
            HmdSessionStatus.Planned,
            HmdSessionStatus.Scheduled,
            HmdSessionStatus.CheckedIn,
            HmdSessionStatus.PreCheck,
            HmdSessionStatus.Held,
            HmdSessionStatus.Ready
        ];
    }
}
