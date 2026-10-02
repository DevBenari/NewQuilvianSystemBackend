using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Hasil uji kepekaan <b>satu antibiotik</b> terhadap <b>satu isolat</b>
    /// (<c>LAB-DC-037</c>, BR-23).
    ///
    /// <b>Dua metode uji tinggal pada tabel yang sama, dan kolom yang berlaku berbeda</b>
    /// (<c>LAB-DEC-124</c>):
    /// <list type="bullet">
    /// <item>Difusi cakram memakai <see cref="DiscContentUgSnapshot"/>, rentang breakpoint,
    /// dan <see cref="ZoneDiameterMm"/>.</item>
    /// <item>Dilusi memakai <see cref="Concentration"/> beserta
    /// <see cref="ConcentrationUnitId"/>.</item>
    /// </list>
    /// Itu sebabnya hampir seluruh kolom di bawah <b>nullable</b> — bukan karena longgar,
    /// melainkan karena separuhnya memang tidak berlaku pada metode yang lain.
    /// </summary>
    public class LabIsolateSusceptibility : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Isolat yang diuji. Satu baris kepekaan melekat pada <b>tepat satu</b> isolat dan
        /// tidak dapat berpindah (<c>INV-27</c>).
        /// </summary>
        [Required]
        public Guid LabMicrobiologyIsolateId { get; set; }

        /// <summary>
        /// Antibiotik yang dikenal. Pengetikan bebas ditolak (<c>INV-30</c>).
        /// </summary>
        [Required]
        public Guid LabAntibioticId { get; set; }

        /// <summary>
        /// Nama antibiotik sebagaimana berlaku saat baris dicatat (<c>AC-110</c>).
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string AntibioticNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Nilai MIC pada metode <b>dilusi</b>. Kosong pada metode difusi cakram.
        /// </summary>
        public decimal? Concentration { get; set; }

        /// <summary>
        /// Satuan bagi <see cref="Concentration"/> — <c>ug/mL</c>, <c>mg/L</c>
        /// (<c>LAB-DEC-115</c>).
        ///
        /// <b>Wajib bila kadarnya terisi</b> (<c>VAL-112</c>). Alasannya sama persis dengan
        /// <c>LAB-DEC-041</c> pada volume specimen: angka <c>1,25</c> yang tersimpan sendirian
        /// nol dapat dibaca ulang siapa pun, dan pola resistensi tidak dapat dihitung dari
        /// nilai bersatuan berbeda yang tidak tercatat.
        /// </summary>
        public Guid? ConcentrationUnitId { get; set; }

        /// <summary>
        /// Kandungan cakram dalam mikrogram pada metode <b>difusi</b> — Ampicillin 10,
        /// Cefoperazone 75, Fosfomycin 200 (<c>LAB-DEC-122</c>, bukti <c>LAB-EVD-006</c>).
        ///
        /// <b>Ini BUKAN nilai hasil.</b> Ia sifat cakram yang dipakai, disalin dari
        /// <c>LabAntibiotic</c> saat baris dibuat.
        /// </summary>
        public int? DiscContentUgSnapshot { get; set; }

        /// <summary>
        /// Batas bawah rentang breakpoint yang <b>berlaku saat hasil diisi</b>
        /// (<c>LAB-DEC-122</c>).
        ///
        /// <b>Snapshot, dan itu yang menjaga arti hasil lama.</b> Breakpoint berubah ketika
        /// versi CLSI berganti; tanpa snapshot, hasil tahun lalu akan dibaca ulang dengan
        /// rentang tahun ini dan cetak ulangnya menghasilkan lembar yang berbeda.
        /// </summary>
        public int? BreakpointLowerMmSnapshot { get; set; }

        /// <summary>Batas atas rentang breakpoint yang berlaku saat hasil diisi.</summary>
        public int? BreakpointUpperMmSnapshot { get; set; }

        /// <summary>
        /// Lebar zona hambat dalam milimeter pada metode <b>difusi</b>.
        ///
        /// <b>Nilai <c>0</c> adalah pengukuran yang SAH</b>, bukan kekosongan
        /// (<c>LAB-DEC-128</c>): ia berarti nol zona hambat terbentuk, dan itu temuan terkuat
        /// bahwa kuman kebal — sebelas dari 22 baris pada <c>LAB-EVD-006</c> bernilai <c>0</c>
        /// dan seluruhnya <c>R</c>. Ruas <b>kosong</b> berarti belum diukur, dan keduanya
        /// disimpan berbeda (<c>VAL-116</c>).
        /// </summary>
        public int? ZoneDiameterMm { get; set; }

        /// <summary>
        /// Interpretasi yang <b>dihitung sistem</b> dari zona terhadap rentang breakpoint
        /// (<c>LAB-DEC-123</c>). Kosong ketika breakpoint belum tersedia atau zona belum
        /// diukur.
        ///
        /// <b>Disimpan, berbeda dari penanda kritis yang sengaja TIDAK disimpan.</b>
        /// Penanda kritis adalah kesimpulan atas aturan yang boleh berubah — pertanyaan masa
        /// kini. Kolom ini adalah <b>fakta apa yang sistem katakan pada saat analis memutuskan
        /// menimpanya</b> — pertanyaan masa lalu, dan pertanyaan masa lalu selalu disimpan.
        /// Tanpanya, <i>"analis menimpa dari apa"</i> kehilangan jawabannya begitu breakpoint
        /// diperbarui.
        /// </summary>
        public LabSusceptibilityResult? ComputedResult { get; set; }

        /// <summary>
        /// Interpretasi yang <b>berlaku</b> dan yang dicetak. Sama dengan
        /// <see cref="ComputedResult"/> kecuali analis menimpanya.
        /// </summary>
        [Required]
        public LabSusceptibilityResult Result { get; set; }

        /// <summary><c>ComputedResult</c> berbeda dari <see cref="Result"/>.</summary>
        public bool IsResultOverridden { get; set; }

        /// <summary>
        /// Alasan menimpa hitungan sistem. <b>Wajib bila ditimpa</b> (<c>VAL-113</c>).
        ///
        /// Penimpaan tetap dibuka sebab <b>resistensi intrinsik</b> menuntut penilaian di luar
        /// rumus — kuman yang secara alami kebal walaupun zonanya lebar. Menutupnya berarti
        /// melaporkan <c>Sensitive</c> pada kuman yang pasti tidak merespons.
        /// </summary>
        [MaxLength(500)]
        public string? ResultOverrideReason { get; set; }

        /// <summary>Catatan analis atas baris ini.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public LabMicrobiologyIsolate? LabMicrobiologyIsolate { get; set; }

        public LabAntibiotic? LabAntibiotic { get; set; }

        public MstMeasurement? ConcentrationUnit { get; set; }
    }
}
