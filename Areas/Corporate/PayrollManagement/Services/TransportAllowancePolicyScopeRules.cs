using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Status cakupan Grade Gaji dan Struktur Gaji pada kebijakan transport (PAY-BE-FIX-014). Nilai adalah KUNCI BAKU yang dibaca Frontend;
/// jangan diterjemahkan. Status diturunkan saat dibaca, tidak disimpan.
/// </summary>
public static class TransportPolicyScopeStatuses
{
    /// <summary>Grade Gaji dan Struktur Gaji sama-sama ditetapkan: kebijakan hanya untuk struktur itu.</summary>
    public const string Scoped = "Scoped";

    /// <summary>Hanya Grade Gaji yang ditetapkan: kebijakan berlaku untuk seluruh struktur milik grade itu.</summary>
    public const string GradeScoped = "GradeScoped";

    /// <summary>
    /// Keduanya kosong: data lama sebelum cakupan ini ada. Perilaku lama dipertahankan (tidak dimigrasikan dan tidak diubah), tetapi tidak
    /// pernah dianggap bukti bahwa kebijakan ini sah untuk grade atau struktur tertentu.
    /// </summary>
    public const string LegacyUnscoped = "LegacyUnscoped";

    /// <summary>Struktur terisi tanpa grade. Dicegah check constraint dan validasi; hanya muncul bila data dimanipulasi di luar aplikasi.</summary>
    public const string Invalid = "Invalid";
}

/// <summary>Fakta Grade Gaji yang diperlukan untuk memvalidasi cakupan kebijakan transport.</summary>
public sealed record TransportPolicyGradeFacts(Guid Id, bool IsActive, DateTime? EffectiveStartDate, DateTime? EffectiveEndDate);

/// <summary>Fakta Struktur Gaji yang diperlukan untuk memvalidasi cakupan kebijakan transport.</summary>
public sealed record TransportPolicyStructureFacts(
    Guid Id,
    Guid SalaryGradeId,
    bool IsActive,
    DateTime? EffectiveStartDate,
    DateTime? EffectiveEndDate,
    Guid? LegalEntityId,
    Guid? HospitalSiteId);

/// <summary>
/// Satu-satunya definisi aturan cakupan Grade Gaji dan Struktur Gaji untuk kebijakan transport (PAY-BE-FIX-014). Murni: tanpa akses data
/// dan tanpa efek samping, sehingga dapat dipakai bersama oleh CRUD kebijakan sekarang, serta validasi penetapan pegawai dan kalkulasi
/// payroll pada fase berikutnya tanpa menyalin aturan.
///
/// Tiga konsep yang TIDAK boleh disatukan: <c>EmployeeGradeId</c> (grade kepegawaian, cakupan tambahan yang sudah ada),
/// <c>SalaryGradeId</c> (grade penggajian), dan <c>SalaryStructureId</c> (struktur gaji yang milik tepat satu grade gaji).
/// </summary>
public static class TransportAllowancePolicyScopeRules
{
    public static string ScopeStatusOf(Guid? salaryGradeId, Guid? salaryStructureId)
    {
        if (salaryGradeId.HasValue && salaryStructureId.HasValue) return TransportPolicyScopeStatuses.Scoped;
        if (salaryGradeId.HasValue) return TransportPolicyScopeStatuses.GradeScoped;
        if (salaryStructureId.HasValue) return TransportPolicyScopeStatuses.Invalid;
        return TransportPolicyScopeStatuses.LegacyUnscoped;
    }

    /// <summary>
    /// Memvalidasi cakupan kebijakan; mengembalikan pesan yang dapat ditampilkan kepada pengguna, atau <c>null</c> bila sah. Tanpa
    /// grade dan struktur (data lama) selalu sah di sini: aturan ini tidak mengubah arti data lama. Dipakai pada Create, Update, dan
    /// aktivasi ulang, sehingga permintaan yang dimanipulasi lewat API tidak lolos meski dropdown frontend benar.
    /// </summary>
    public static string? ValidateScope(
        Guid? salaryGradeId,
        Guid? salaryStructureId,
        TransportPolicyGradeFacts? grade,
        TransportPolicyStructureFacts? structure,
        Guid? policyLegalEntityId,
        Guid? policyHospitalSiteId,
        DateTime? policyStartDate,
        DateTime? policyEndDate)
    {
        if (!salaryGradeId.HasValue && !salaryStructureId.HasValue) return null;

        if (!salaryGradeId.HasValue)
            return "Pilih Grade Gaji terlebih dahulu sebelum memilih Struktur Gaji.";

        if (grade is null || grade.Id != salaryGradeId.Value || !grade.IsActive)
            return "Grade Gaji tidak ditemukan atau tidak aktif.";

        if (salaryStructureId.HasValue)
        {
            if (structure is null || structure.Id != salaryStructureId.Value || !structure.IsActive)
                return "Struktur Gaji tidak ditemukan atau tidak aktif.";

            if (structure.SalaryGradeId != salaryGradeId.Value)
                return "Struktur Gaji yang dipilih bukan milik Grade Gaji yang dipilih.";

            if (structure.LegalEntityId.HasValue && policyLegalEntityId.HasValue && structure.LegalEntityId.Value != policyLegalEntityId.Value)
                return "Legal entity kebijakan berbeda dengan legal entity Struktur Gaji yang dipilih.";

            if (structure.HospitalSiteId.HasValue && policyHospitalSiteId.HasValue && structure.HospitalSiteId.Value != policyHospitalSiteId.Value)
                return "Fasilitas rumah sakit kebijakan berbeda dengan fasilitas Struktur Gaji yang dipilih.";
        }

        // Masa berlaku kebijakan tidak boleh dipakai untuk membenarkan master yang tidak berlaku pada periode itu.
        var gradeWindow = ValidateWindow("Grade Gaji", grade.EffectiveStartDate, grade.EffectiveEndDate, policyStartDate, policyEndDate);
        if (gradeWindow is not null) return gradeWindow;

        if (structure is not null)
        {
            var structureWindow = ValidateWindow(
                "Struktur Gaji", structure.EffectiveStartDate, structure.EffectiveEndDate, policyStartDate, policyEndDate);
            if (structureWindow is not null) return structureWindow;
        }

        return null;
    }

    private static string? ValidateWindow(
        string label, DateTime? masterStart, DateTime? masterEnd, DateTime? policyStart, DateTime? policyEnd)
    {
        if (masterStart.HasValue)
        {
            if (!policyStart.HasValue)
                return $"Tanggal mulai berlaku kebijakan wajib diisi karena {label} baru berlaku sejak {Format(masterStart.Value)}.";
            if (policyStart.Value.Date < masterStart.Value.Date)
                return $"Tanggal mulai berlaku kebijakan tidak boleh sebelum tanggal mulai {label} ({Format(masterStart.Value)}).";
        }

        if (masterEnd.HasValue)
        {
            if (!policyEnd.HasValue)
                return $"Tanggal akhir berlaku kebijakan wajib diisi karena {label} berakhir pada {Format(masterEnd.Value)}.";
            if (policyEnd.Value.Date > masterEnd.Value.Date)
                return $"Tanggal akhir berlaku kebijakan tidak boleh melewati tanggal akhir {label} ({Format(masterEnd.Value)}).";
        }

        return null;
    }

    private static string Format(DateTime value) => value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
