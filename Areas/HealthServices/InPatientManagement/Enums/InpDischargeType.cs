namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    // Nilai 4 dan 5 sengaja dikosongkan untuk cara pulang meninggal dan kabur.
    // Keduanya di luar scope revisi ini dan menunggu DEC-INP-007; mengosongkan nomornya
    // sekarang membuat penambahan kelak tidak mengubah angka yang sudah tersimpan.
    public enum InpDischargeType
    {
        Unknown = 0,
        DoctorApproved = 1,
        AgainstMedicalAdvice = 2,
        Referred = 3
    }
}
