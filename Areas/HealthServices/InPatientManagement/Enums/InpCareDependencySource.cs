namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

/// <summary>
/// Sumber pencatatan High/Low Dependency, ditentukan server dari peran pencatat (GUARD-DNH-02),
/// tidak pernah dikirim klien.
/// </summary>
public enum InpCareDependencySource
{
    /// <summary>Catatan awal petugas admisi selagi episode <c>Draft</c>.</summary>
    AdmissionNote = 1,

    /// <summary>Penilaian perawat penanggung jawab aktif.</summary>
    NurseAssessment = 2,

    /// <summary>Keputusan DPJP aktif.</summary>
    DoctorDecision = 3
}
