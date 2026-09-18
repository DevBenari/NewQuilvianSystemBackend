namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums
{
    /// <summary>
    /// Kategori Periode pada daftar pantau — <b>waktu mana</b> yang dibandingkan terhadap
    /// rentang <c>StartDate</c>/<c>EndDate</c> (<c>BR-50</c> butir 1, <c>LAB-API-v1</c>
    /// <c>r18</c>).
    ///
    /// <b>Ketiganya kini berdiri, dan urutan lahirnya pantas dibaca.</b> <c>r18</c> sengaja
    /// memuat <b>dua</b> nilai dan menolak mencantumkan <c>ExaminationDate</c>, karena
    /// <c>LabExamination</c> saat itu nol punya kolom waktu pemeriksaan — mencantumkannya
    /// berarti mendirikan pilihan yang tidak menuju ke mana-mana: layar menawarkannya, petugas
    /// memilihnya, dan daftarnya kosong <b>tanpa satu pun galat</b>.
    ///
    /// <c>r23</c> menambahkannya sesudah <b>dua</b> syarat terpenuhi, bukan satu: kolomnya ada
    /// (<c>r21</c>), <b>dan ada yang mengisinya</b> — jalur tulis beserta layarnya
    /// (<c>r21</c>/<c>r22</c>). Syarat kedua yang menentukan; kolom tanpa penulis adalah persis
    /// kesalahan <c>BE-EXT-04</c>.
    ///
    /// <b>Nilai angkanya dimulai dari 1, bukan 0.</b> Ruas yang memakainya nullable, dan
    /// <c>0</c> sebagai nilai sah akan tidak dapat dibedakan dari "tidak dikirim" pada beberapa
    /// jalur pengikatan.
    /// </summary>
    public enum LabDateCategory
    {
        /// <summary>
        /// Waktu pesanan dibuat — <c>RequestedAt ?? CreateDateTime</c>.
        /// <b>Ini perilaku yang sudah berjalan</b>, dan ia menjadi bawaan ketika ruasnya tidak
        /// dikirim, sehingga pemanggil lama tidak berubah perilakunya.
        /// </summary>
        OrderDate = 1,

        /// <summary>
        /// Waktu bahan diambil dari pasien — <c>LabSpecimen.CollectedAt</c>.
        ///
        /// Satu pesanan dapat memiliki beberapa wadah dengan waktu pengambilan berbeda; pesanan
        /// ikut tersaring bila <b>salah satu</b> wadahnya diambil di dalam rentang — pola yang
        /// sama dengan penyaring status wadah pada DTO yang sama.
        ///
        /// Wadah yang belum dinyatakan waktu pengambilannya <b>tidak pernah cocok</b>; tidak ada
        /// jatuh-tempo ke waktu pembuatan baris.
        /// </summary>
        SamplingDate = 2,

        /// <summary>
        /// Waktu pemeriksaannya <b>dikerjakan</b> — <c>LabExamination.ExaminedAt</c>
        /// (<c>r23</c>).
        ///
        /// Satu pesanan dapat memuat beberapa pemeriksaan yang dikerjakan pada waktu berbeda —
        /// elektrolit pagi, kultur sore. Pesanan ikut tersaring bila <b>salah satu</b>
        /// pemeriksaannya dikerjakan di dalam rentang, aturan yang sama dengan
        /// <see cref="SamplingDate"/>.
        ///
        /// Pemeriksaan yang belum dikerjakan <b>tidak pernah cocok</b>, sebab yang sama:
        /// pertanyaannya "pesanan mana yang dikerjakan dalam rentang ini", dan yang belum
        /// dikerjakan bukan bagian dari jawabannya.
        /// </summary>
        ExaminationDate = 3
    }
}
