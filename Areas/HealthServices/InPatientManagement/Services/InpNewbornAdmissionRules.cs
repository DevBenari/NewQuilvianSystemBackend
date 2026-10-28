using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

internal static class InpNewbornAdmissionRules
{
    public static string? ValidateContext(InpatientNewbornAdmissionContext? context, MstPatient patient)
    {
        if (!patient.IsNewborn)
            return context == null ? null : "Konteks admisi bayi hanya boleh dipakai untuk pasien bayi baru lahir.";
        if (context == null)
            return "Konteks admisi bayi baru lahir wajib diisi.";
        if (!patient.MotherPatientId.HasValue || !patient.DeliveryCaseId.HasValue)
            return "Profil pasien bayi dan hubungan pasien ibu harus dilengkapi melalui pendaftaran pasien.";
        if (context.DeliveryCaseId == Guid.Empty || context.DeliveryCaseId != patient.DeliveryCaseId.Value)
            return "Kasus persalinan pada admisi tidak sesuai dengan identitas bayi.";
        if (context.Source != "HospitalBirth")
            return "Admisi bayi baru lahir hanya menerima episode kelahiran di rumah sakit ini.";
        if (context.CareMode is not ("RoomingIn" or "Neonatal"))
            return "Pilih rencana perawatan bayi yang valid.";
        if (string.IsNullOrWhiteSpace(context.CareSource) || context.CareSource.Trim().Length > 200)
            return "Sumber keputusan perawatan bayi wajib diisi, maksimal 200 karakter.";
        if (context.CareMode == "RoomingIn" && context.RequiresIntensiveCare)
            return "Kebutuhan perawatan intensif tidak dapat dipilih bersama rawat gabung.";
        return null;
    }

    public static async Task<string?> ValidateDeliveryCaseAsync(
        ApplicationDbContext db,
        InpatientNewbornAdmissionContext? context,
        MstPatient patient,
        CancellationToken ct)
    {
        if (!patient.IsNewborn || context == null) return null;

        var valid = await db.Set<OprCase>().AsNoTracking().AnyAsync(x =>
            x.Id == context.DeliveryCaseId &&
            x.PatientId == patient.MotherPatientId &&
            x.SurgicalServiceType == OprSurgicalServiceType.Obstetric &&
            (x.Status == OprCaseStatus.InProgress || x.Status == OprCaseStatus.Completed) &&
            !x.IsDelete &&
            !x.IsCancel,
            ct);

        return valid
            ? null
            : "Kasus persalinan ibu tidak ditemukan, bukan layanan obstetri, atau tidak lagi dapat dipakai.";
    }

    public const string NonPediatricReasonPrefix = "Alasan DPJP non-Anak: ";

    /// <summary>
    /// Spesialisasi anak dikenali dari nama spesialisasi dokter ("Anak", "Bedah Anak", "Pediatri").
    /// Master spesialisasi belum punya penanda khusus anak; aturan ini dipusatkan di sini supaya
    /// mudah diganti bila penanda itu ditambahkan (K-05 ISSUE-EPS-040).
    /// </summary>
    public static bool IsPediatricSpecialty(string? specialtyName) =>
        !string.IsNullOrWhiteSpace(specialtyName) &&
        (specialtyName.Contains("anak", StringComparison.OrdinalIgnoreCase) ||
         specialtyName.Contains("pediatri", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// K-05 (25-10-2026): DPJP bayi boleh dokter non-anak hanya dengan alasan tertulis.
    /// Contoh: dr. Gigi tanpa alasan → ditolak; dengan "Sp.A jaga sedang operasi" → diterima.
    /// </summary>
    public static async Task<string?> ValidateNewbornDoctorAsync(
        ApplicationDbContext db,
        MstPatient patient,
        InpatientNewbornAdmissionContext? context,
        Guid doctorId,
        CancellationToken ct)
    {
        if (!patient.IsNewborn || context == null) return null;

        var doctor = await db.Set<MstDoctor>().AsNoTracking()
            .Where(x => x.Id == doctorId)
            .Select(x => new
            {
                x.SpecialistName,
                SpecializationName = x.Specialization != null ? x.Specialization.SpecializationName : null
            })
            .FirstOrDefaultAsync(ct);

        if (doctor == null || IsPediatricSpecialty(doctor.SpecialistName) || IsPediatricSpecialty(doctor.SpecializationName))
            return null;

        return string.IsNullOrWhiteSpace(context.NonPediatricDoctorReason)
            ? "Alasan memilih dokter non-Anak sebagai DPJP bayi wajib diisi."
            : null;
    }

    /// <summary>
    /// Menggabungkan alasan DPJP non-anak ke catatan admisi tanpa menggandakan baris yang sama.
    /// Bila request tidak membawa alasan, baris lama pada catatan sebelumnya dipertahankan.
    /// </summary>
    public static string? MergeNonPediatricReason(string? notes, string? reason, string? previousNotes = null)
    {
        var cleanReason = string.IsNullOrWhiteSpace(reason)
            ? ExtractNonPediatricReason(previousNotes)
            : reason.Trim();
        if (cleanReason == null) return notes;

        var line = NonPediatricReasonPrefix + cleanReason;
        if (notes?.Contains(line, StringComparison.Ordinal) == true) return notes;
        return string.IsNullOrWhiteSpace(notes) ? line : $"{notes.TrimEnd()}\n{line}";
    }

    private static string? ExtractNonPediatricReason(string? notes) =>
        notes?.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(NonPediatricReasonPrefix, StringComparison.Ordinal))
            .Select(line => line[NonPediatricReasonPrefix.Length..].Trim())
            .FirstOrDefault(value => value.Length > 0);

    public static void ApplyContext(InpEpisode episode, InpatientNewbornAdmissionContext? context)
    {
        if (context == null) return;
        episode.DeliveryCaseId = context.DeliveryCaseId;
        episode.NewbornAdmissionSource = context.Source;
        episode.NewbornCareMode = context.CareMode;
        episode.NewbornCareSource = context.CareSource.Trim();
        episode.RequiresNeonatalIntensiveCare = context.RequiresIntensiveCare;
        // Nilai penagihan/deposit lama sengaja tidak ditulis lagi. Admission hanya mengirim
        // layanan ke Billing; invoice dan pemakaian deposit ibu menjadi keputusan Billing/AR.
    }

    public static async Task<Guid?> FindMotherRoomAsync(ApplicationDbContext db, Guid? motherEpisodeId,
        Guid? motherPatientId, CancellationToken ct)
    {
        if (!motherEpisodeId.HasValue || !motherPatientId.HasValue) return null;
        return await db.Set<InpBedPlacement>().AsNoTracking()
            .Where(p => p.EpisodeId == motherEpisodeId && !p.IsDelete && !p.IsSuperseded &&
                p.EndDateTime == null && p.Episode != null && !p.Episode.IsDelete &&
                p.Episode.PatientId == motherPatientId &&
                (p.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                 (p.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending && p.Episode.PhysicallyLeftAt == null)))
            .Select(p => (Guid?)p.RoomId).FirstOrDefaultAsync(ct);
    }

    public static List<(string Code, string Message)> ValidateBed(InpEpisode? episode, MstPatient? patient,
        MstBed bed, MstRoom room, Guid? motherRoomId)
    {
        var failures = new List<(string Code, string Message)>();
        if (patient == null) return failures;
        if (patient.IsNewborn && !bed.IsForNewborn)
            failures.Add(("NEWBORN_BED_REQUIRED", "Pasien bayi hanya dapat memakai tempat yang ditetapkan untuk bayi."));
        if (!patient.IsNewborn && bed.IsForNewborn)
            failures.Add(("NEWBORN_BED_ONLY", "Tempat ini khusus pasien bayi."));
        // Admission planning does not replace later clinical transfer decisions.
        var isAdmission = episode?.EpisodeStatus == InpEpisodeStatus.Draft;
        if (isAdmission && patient.IsNewborn && episode?.NewbornCareMode == "RoomingIn" &&
            (!motherRoomId.HasValue || motherRoomId.Value != room.Id))
            failures.Add(("NEWBORN_ROOMING_IN_MISMATCH", "Rawat gabung memerlukan boks bayi di kamar penempatan ibu yang masih aktif."));
        if (isAdmission && patient.IsNewborn && episode?.RequiresNeonatalIntensiveCare == true &&
            !bed.IsIntensiveCareBed && !room.IsIntensiveCare)
            failures.Add(("NEWBORN_INTENSIVE_REQUIRED", "Bayi membutuhkan tempat neonatal dengan kemampuan intensif."));
        // FIX-EPS-040-03: neonatal terpisah hanya di kamar khusus bayi. Boks di kamar perawatan dewasa
        // adalah boks rawat gabung milik ibu yang menempati kamar itu (issue-031 kartu 2).
        if (isAdmission && patient.IsNewborn && episode?.NewbornCareMode == "Neonatal" && !room.IsForNewborn)
            failures.Add(("NEWBORN_NEONATAL_ROOM_REQUIRED", "Boks rawat gabung hanya untuk bayi dari ibu di kamar ini. Pilih kamar ruang bayi."));
        // FIX-EPS-040-04 (K-02, 25-10-2026): bed intensif tidak dipakai bayi non-intensif di alur admisi.
        if (isAdmission && patient.IsNewborn && episode?.NewbornCareMode == "Neonatal" &&
            episode.RequiresNeonatalIntensiveCare == false && (bed.IsIntensiveCareBed || room.IsIntensiveCare))
            failures.Add(("NEWBORN_INTENSIVE_BED_RESERVED", "Bed intensif hanya untuk bayi yang membutuhkan NICU."));
        return failures;
    }
}
