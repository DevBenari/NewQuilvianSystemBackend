using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Mengubah isi pengaturan satu disiplin (<c>LAB-API-v1</c> <c>r27</c> bagian 22.7).
    ///
    /// <b>Nol ruas disiplin di sini.</b> Disiplinnya ada pada path, dan barisnya tetap tiga —
    /// mengirimkannya lagi pada badan permintaan hanya membuka kemungkinan keduanya berbeda.
    /// </summary>
    public class LabDisciplineSettingUpdateRequest
    {
        [Required(ErrorMessage = "Label konsultan wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Label konsultan paling panjang 150 karakter.")]
        public string ConsultantLabel { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Nama konsultan paling panjang 200 karakter.")]
        public string? ConsultantName { get; set; }

        [MaxLength(1000, ErrorMessage = "Kalimat baku paling panjang 1000 karakter.")]
        public string? StandingNote { get; set; }

        [MaxLength(20, ErrorMessage = "Awalan nomor cetak paling panjang 20 karakter.")]
        public string? ReportNumberPrefix { get; set; }

        /// <summary>
        /// Pemisah antara tahun dan nomor urut. <b>Teks kosong berarti tanpa pemisah</b>, dan
        /// itu nilai yang sah — Patologi Klinik memang menempelkan tahun langsung pada
        /// nomornya (<c>25039254</c>).
        /// </summary>
        [MaxLength(5, ErrorMessage = "Pemisah nomor cetak paling panjang 5 karakter.")]
        public string? ReportNumberSeparator { get; set; }

        /// <summary>Lebar minimum nomor urut. Empat pada Mikrobiologi dan Patologi Anatomi, enam pada Patologi Klinik.</summary>
        [Range(1, 12, ErrorMessage = "Lebar nomor cetak harus antara 1 dan 12 digit.")]
        public int ReportNumberLength { get; set; } = 4;

        public bool IsActive { get; set; } = true;
    }

    public class LabDisciplineSettingResponse
    {
        public Guid Id { get; set; }

        public int Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        public string ConsultantLabel { get; set; } = string.Empty;

        public string? ConsultantName { get; set; }

        public string? StandingNote { get; set; }

        public string? ReportNumberPrefix { get; set; }

        public string? ReportNumberSeparator { get; set; }

        public int ReportNumberLength { get; set; }

        /// <summary>
        /// Contoh nomor yang akan dihasilkan bentuk ini pada tahun berjalan — misalnya
        /// <c>26-0001</c>. Baca-saja, dan ada supaya kepala instalasi melihat akibat
        /// setelannya <b>sebelum</b> lembar pertama tercetak.
        /// </summary>
        public string ReportNumberExample { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    /// <summary>Pilihan disiplin untuk layar pengaturan.</summary>
    public class LabDisciplineOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        /// <summary>Apakah disiplin ini sudah punya barisnya.</summary>
        public bool IsConfigured { get; set; }
    }
}
