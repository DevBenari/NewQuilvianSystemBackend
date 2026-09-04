using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using System.Xml.Linq;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-18` — keterangan pada Swagger.
    ///
    /// KENAPA DIUJI OTOMATIS PADAHAL VERIFIKASINYA MANUAL. Roadmap menyebut verifikasi `BE-18`
    /// berupa pemeriksaan manual halaman Swagger. Masalahnya, pemeriksaan manual hanya berlaku
    /// pada hari ia dilakukan: keterangan dapat terhapus pada perubahan berikutnya tanpa ada
    /// yang menyadarinya.
    ///
    /// Uji ini memeriksa **berkas dokumentasi XML hasil build** — sumber yang dipakai Swagger
    /// untuk menampilkan keterangan. Bila keterangannya hilang, atau berkas dokumentasinya
    /// berhenti dihasilkan, uji ini gagal.
    ///
    /// Yang TIDAK diuji di sini: tampilan halaman Swagger-nya sendiri. Itu tetap perlu dilihat
    /// sekali dengan mata.
    /// </summary>
    public class SwaggerDocumentationTests
    {
        private const string RuasProviderUserId =
            "P:QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs." +
            "UpdatePatientIntegratedProgressNoteRequest.ProviderUserId";

        private const string RuasIsReadOnlyGenerated =
            "P:QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs." +
            "UpdatePatientIntegratedProgressNoteRequest.IsReadOnlyGenerated";

        private const string MetodeUbahCppt =
            "M:QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers." +
            "PatientIntegratedProgressNoteController.UpdateProgressNote";

        /// <summary>
        /// Membaca berkas dokumentasi XML yang dihasilkan build aplikasi.
        /// </summary>
        private static XDocument BacaDokumentasi()
        {
            var assembly = typeof(PatientIntegratedProgressNoteController).Assembly;
            var berkasXml = Path.ChangeExtension(assembly.Location, ".xml");

            Assert.True(
                File.Exists(berkasXml),
                $"Berkas dokumentasi XML tidak ditemukan di {berkasXml}. " +
                "Pastikan GenerateDocumentationFile masih menyala pada QuilvianSystemBackend.csproj — " +
                "tanpa berkas ini, keterangan pada Swagger hilang seluruhnya.");

            return XDocument.Load(berkasXml);
        }

        /// <summary>
        /// Mengambil seluruh teks keterangan sebuah anggota, apa pun bentuk penulisannya.
        /// </summary>
        private static string Keterangan(XDocument dokumentasi, string awalanNama)
        {
            var anggota = dokumentasi
                .Descendants("member")
                .FirstOrDefault(x => (x.Attribute("name")?.Value ?? string.Empty)
                    .StartsWith(awalanNama, StringComparison.Ordinal));

            Assert.NotNull(anggota);

            return anggota!.Value;
        }

        /// <summary>
        /// Acceptance criteria 1: Swagger menyebut bahwa `ProviderUserId` dan
        /// `IsReadOnlyGenerated` diabaikan pada permintaan ubah.
        ///
        /// Ini menutup risiko yang disebut roadmap: mengabaikan kiriman klien tanpa
        /// pemberitahuan adalah praktik buruk. Klien yang tetap mengirim kedua kolom itu tidak
        /// menerima galat, sehingga tanpa keterangan ini satu-satunya cara mengetahuinya adalah
        /// membaca source.
        /// </summary>
        [Fact]
        public void Swagger_MenyebutDuaKolomYangDiabaikanPadaPermintaanUbah()
        {
            var dokumentasi = BacaDokumentasi();

            foreach (var ruas in new[] { RuasProviderUserId, RuasIsReadOnlyGenerated })
            {
                var keterangan = Keterangan(dokumentasi, ruas);

                Assert.Contains("DIABAIKAN", keterangan, StringComparison.OrdinalIgnoreCase);

                // Sekaligus menyatakan permintaannya tidak ditolak — bagian yang paling mudah
                // disalahpahami bila hanya disebut "diabaikan".
                Assert.Contains("tidak ditolak", keterangan, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Acceptance criteria 3: Swagger menyatakan bahwa baru CPPT yang tunduk aturan
        /// keutuhan.
        ///
        /// Tanpa keterangan ini, pemakai API akan mengira seluruh dokumen klinis sudah
        /// terlindungi aturan keutuhan — padahal dua belas dari tiga belas jenis belum.
        /// </summary>
        [Fact]
        public void Swagger_MenyatakanBaruCpptYangTundukAturanKeutuhan()
        {
            var keterangan = Keterangan(BacaDokumentasi(), MetodeUbahCppt);

            Assert.Contains("belum ditegakkan", keterangan, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("dua belas", keterangan, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Endpoint ubah CPPT menyatakan bahwa catatan terkunci menolak perubahan, dan
        /// mengarahkan ke addendum.
        ///
        /// Ini kode status baru pada endpoint yang sudah berjalan, sehingga klien perlu
        /// mengetahuinya sebelum menemuinya sendiri.
        /// </summary>
        [Fact]
        public void Swagger_MenyebutPenolakanPadaCatatanTerkunciBesertaJalanKeluarnya()
        {
            var keterangan = Keterangan(BacaDokumentasi(), MetodeUbahCppt);

            Assert.Contains("terkunci", keterangan, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("addendum", keterangan, StringComparison.OrdinalIgnoreCase);
        }
    }
}
