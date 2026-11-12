using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using ChangeType = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.ShiftRevisionChangeType;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Menulis satu baris Riwayat Revisi roster terbit (HRD-DEC-195, HRD-DEC-217, INV-A52-01).
    ///
    /// Hanya menambahkan baris ke konteks; tidak memanggil SaveChanges dan tidak membuka transaksi.
    /// Pemanggil menyimpannya bersama perubahan shift dalam satu transaksi, sehingga perubahan tanpa
    /// riwayat tidak mungkin tersimpan. Riwayat hanya-tambah: tidak ada metode ubah atau hapus.
    /// </summary>
    public class ShiftRevisionRecorder
    {
        public const int ReasonCategoryMaxLength = 50;
        public const int ReasonNoteMaxLength = 1000;

        private static readonly HashSet<string> ChangeTypes = new(StringComparer.Ordinal)
        {
            ChangeType.Replacement, ChangeType.RetroactiveCorrection, ChangeType.AddShift, ChangeType.RemoveShift
        };

        private readonly ApplicationDbContext _context;

        public ShiftRevisionRecorder(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Ganti Petugas: <paramref name="previousWorkforceProfileId"/> adalah petugas tepat sebelum perubahan ini,
        /// bukan petugas pertama pada shift tersebut (35.9.3: Budi → Tono → Andi menulis Tono pada baris kedua).
        /// </summary>
        public HrdShiftAssignmentRevision RecordReplacement(
            TrxShiftAssignment shift,
            Guid rosterPeriodId,
            Guid previousWorkforceProfileId,
            Guid newWorkforceProfileId,
            string reasonCategory,
            string reasonNote,
            Guid? shiftReplacementId,
            Guid changedByUserId,
            DateTime changedAt) =>
            Record(new HrdShiftAssignmentRevision
            {
                ShiftAssignmentId = shift.Id,
                RosterPeriodId = rosterPeriodId,
                ShiftDate = shift.ShiftDate,
                ChangeType = ChangeType.Replacement,
                PreviousWorkforceProfileId = previousWorkforceProfileId,
                NewWorkforceProfileId = newWorkforceProfileId,
                PreviousShiftId = shift.ShiftId,
                NewShiftId = shift.ShiftId,
                PreviousStartAt = shift.ScheduledStartAt,
                PreviousEndAt = shift.ScheduledEndAt,
                NewStartAt = shift.ScheduledStartAt,
                NewEndAt = shift.ScheduledEndAt,
                ReasonCategory = reasonCategory,
                ReasonNote = reasonNote,
                ShiftReplacementId = shiftReplacementId,
                ChangedByUserId = changedByUserId,
                ChangedAt = changedAt,
            });

        public HrdShiftAssignmentRevision Record(HrdShiftAssignmentRevision revision)
        {
            ArgumentNullException.ThrowIfNull(revision);

            if (!ChangeTypes.Contains(revision.ChangeType))
                throw new ArgumentException($"Jenis perubahan '{revision.ChangeType}' tidak dikenal.", nameof(revision));
            if (revision.ShiftAssignmentId == Guid.Empty || revision.RosterPeriodId == Guid.Empty)
                throw new ArgumentException("Riwayat revisi wajib menunjuk shift dan periode roster.", nameof(revision));
            if (revision.ChangedByUserId == Guid.Empty)
                throw new ArgumentException("Riwayat revisi wajib mencatat pelaku.", nameof(revision));

            revision.ReasonCategory = revision.ReasonCategory?.Trim() ?? string.Empty;
            revision.ReasonNote = revision.ReasonNote?.Trim() ?? string.Empty;
            if (revision.ReasonCategory.Length == 0 || revision.ReasonNote.Length == 0)
                throw new ArgumentException("Alasan perubahan wajib diisi.", nameof(revision));
            if (revision.ReasonCategory.Length > ReasonCategoryMaxLength || revision.ReasonNote.Length > ReasonNoteMaxLength)
                throw new ArgumentException("Alasan perubahan melebihi panjang yang diizinkan.", nameof(revision));

            revision.CreateBy = revision.ChangedByUserId;
            revision.CreateDateTime = revision.ChangedAt;

            _context.HrdShiftAssignmentRevisions.Add(revision);
            return revision;
        }
    }
}
