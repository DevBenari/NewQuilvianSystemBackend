namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Jenis pesanan yang belum selesai saat pasien pergi dari IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-035</c>. <c>RadiologyOrder</c> ditambahkan mengikuti <c>IGD-DEC-099</c>.
    ///
    /// Ketika nilai ini ditulis pada 27 Agustus 2026, modul Radiologi memang belum ada,
    /// sehingga pesanannya ditempuh di luar sistem dan dicatat sebagai pesanan
    /// <c>External</c>. Keadaan itu <b>sudah berubah</b>: modul Radiologi rilis 31 Agustus
    /// 2026, dan <c>POST api/v1/health-services/radiology-management/rad-orders</c> tersedia
    /// untuk seluruh modul pemesan tanpa perlakuan khusus.
    ///
    /// Penyambungan IGD ke endpoint itu belum dikerjakan dan menunggu frontend Radiologi
    /// Rilis 1, supaya pesanan tidak menumpuk tanpa ada petugas yang dapat
    /// menindaklanjutinya. Sampai saat itu nilai ini tetap dipakai untuk mencatat pesanan
    /// yang ditempuh di luar sistem.
    ///
    /// Pesanan <c>External</c> yang sudah terlanjur tercatat <b>tidak</b> dipindahkan ke
    /// jalur resmi (<c>RAD-DEC-009</c>): pesanan lama tidak punya study, tidak punya jawaban
    /// gerbang keselamatan, dan tidak punya penilaian mutu citra, sehingga memindahkannya
    /// berarti membuat catatan pemeriksaan untuk kejadian yang tidak pernah tercatat.
    /// </remarks>
    public enum EmergencyOrderKind
    {
        Medication = 1,
        Procedure = 2,
        LaboratoryOrder = 3,
        RadiologyOrder = 4
    }
}
