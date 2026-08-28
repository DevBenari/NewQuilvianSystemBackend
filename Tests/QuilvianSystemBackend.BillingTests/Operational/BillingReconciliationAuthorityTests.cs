using System.Reflection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Attributes;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Operational
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-007 yang dapat dibuktikan tanpa database.
    ///
    /// Yang dijaga di sini adalah batas kewenangan rekonsiliasi. Rekonsiliasi berwenang
    /// menemukan dan menampilkan masalah, tetapi tidak berwenang memindahkan uang. Selama
    /// tidak ada route maupun anggota finansial pada permukaannya, petugas rekonsiliasi tidak
    /// dapat diam-diam berubah menjadi pemutus uang.
    /// </summary>
    public sealed class BillingReconciliationAuthorityTests
    {
        /// <summary>
        /// Istilah yang menandakan pemindahan uang. Tidak satu pun boleh muncul sebagai anggota
        /// pada model maupun service rekonsiliasi.
        /// </summary>
        private static readonly string[] IstilahFinansialTerlarang =
        {
            "Paid",
            "Payment",
            "Settlement",
            "Tender",
            "Void",
            "Refund",
            "Reversal",
            "WriteOff",
            "Invoice",
            "Discount"
        };

        [Fact]
        public void ModelRekonsiliasi_TidakMemilikiPropertiFinansialApaPun()
        {
            Type[] model = { typeof(BilReconciliationCase), typeof(MstBillingReconciliationPolicy) };

            foreach (var tipe in model)
            {
                var namaProperti = tipe
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => property.Name)
                    .ToList();

                foreach (var istilah in IstilahFinansialTerlarang)
                {
                    Assert.DoesNotContain(
                        namaProperti,
                        nama => nama.Contains(istilah, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        [Fact]
        public void ServiceRekonsiliasi_TidakMemilikiMethodKewenanganFinansial()
        {
            var namaMethod = typeof(BillingReconciliationService)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .ToList();

            foreach (var istilah in IstilahFinansialTerlarang)
            {
                Assert.DoesNotContain(
                    namaMethod,
                    nama => nama.Contains(istilah, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Membaca, memindai, menugaskan, dan menyelesaikan adalah empat kewenangan berbeda.
        /// Menyatukannya membuat hak lihat berubah menjadi hak menutup masalah.
        /// </summary>
        [Theory]
        [InlineData(nameof(BillingReconciliationController.GetCases), "BillingReconciliation", "Read")]
        [InlineData(nameof(BillingReconciliationController.GetCaseById), "BillingReconciliation", "Read")]
        [InlineData(nameof(BillingReconciliationController.GetClosureReadiness), "BillingReconciliation", "Read")]
        [InlineData(nameof(BillingReconciliationController.GetRecoveryReport), "BillingReconciliation", "Read")]
        [InlineData(nameof(BillingReconciliationController.GetProcessingStatus), "BillingReconciliation", "Read")]
        [InlineData(nameof(BillingReconciliationController.Scan), "BillingReconciliation", "Scan")]
        [InlineData(nameof(BillingReconciliationController.Assign), "BillingReconciliation", "Assign")]
        [InlineData(nameof(BillingReconciliationController.Resolve), "BillingReconciliation", "Resolve")]
        public void EndpointRekonsiliasi_MemakaiPermissionYangDitetapkan(
            string namaMethod,
            string controllerName,
            string actionName)
        {
            var method = typeof(BillingReconciliationController)
                .GetMethod(namaMethod, BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

            Assert.NotNull(permission);
            Assert.Equal(new object[] { controllerName, actionName }, permission!.Arguments);
        }

        [Fact]
        public void PermissionMenutupMasalah_BerbedaDenganPermissionMelihatDanMenugaskan()
        {
            var read = PermissionDari(nameof(BillingReconciliationController.GetCases));
            var assign = PermissionDari(nameof(BillingReconciliationController.Assign));
            var resolve = PermissionDari(nameof(BillingReconciliationController.Resolve));
            var scan = PermissionDari(nameof(BillingReconciliationController.Scan));

            Assert.NotEqual(read, resolve);
            Assert.NotEqual(assign, resolve);
            Assert.NotEqual(read, assign);
            Assert.NotEqual(read, scan);
        }

        /// <summary>
        /// Kosakata hasil pemrosesan harus memuat seluruh anggota yang dituntut
        /// RJ-BIL-GATE-DEC-008, termasuk pembedaan tiga jenis kegagalan.
        /// </summary>
        [Theory]
        [InlineData("RejectedValidation")]
        [InlineData("TransientFailure")]
        [InlineData("PermanentFailure")]
        [InlineData("PendingReconciliation")]
        [InlineData("Reconciled")]
        [InlineData("OutcomeUnknown")]
        [InlineData("PartialOutcome")]
        public void KosakataHasilPemrosesan_MemuatAnggotaYangDituntutKeputusan(string nama)
        {
            Assert.Contains(nama, Enum.GetNames<BillingProcessingOutcome>());
        }

        /// <summary>
        /// Anggota peninggalan RJ-BIL-BE-002 tidak boleh dihapus. Baris lama di database sudah
        /// memuat nilainya, dan menghapusnya membuat baris tersebut tidak dapat dibaca kembali
        /// sebagai status mana pun.
        /// </summary>
        [Fact]
        public void AnggotaPeninggalan_TidakDihapusDanNilainyaTidakBergeser()
        {
            Assert.Equal(4, (int)BillingProcessingOutcome.FailedBeforeEffect);
            Assert.Equal(1, (int)BillingProcessingOutcome.Received);
            Assert.Equal(2, (int)BillingProcessingOutcome.InProgress);
            Assert.Equal(3, (int)BillingProcessingOutcome.Succeeded);
            Assert.Equal(5, (int)BillingProcessingOutcome.PartialOutcome);
            Assert.Equal(6, (int)BillingProcessingOutcome.OutcomeUnknown);
        }

        /// <summary>
        /// Enum status tidak boleh memiliki anggota bernilai nol, dengan alasan yang sama
        /// seperti pada RJ-BIL-BE-003: kolom integer bawaan bernilai nol akan menghasilkan
        /// baris yang tidak dapat dibaca kembali sebagai status mana pun.
        /// </summary>
        [Fact]
        public void StatusRekonsiliasi_TidakMemakaiNilaiNol()
        {
            Assert.DoesNotContain(0, Enum.GetValues<BillingProcessingOutcome>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<BillingReconciliationCaseType>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<BillingReconciliationCaseStatus>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<BillingReconciliationPriority>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<BillingReconciliationResolutionType>().Select(x => (int)x));
        }

        /// <summary>
        /// Penyelesaian yang menuntut tindakan finansial harus tersedia sebagai jenis
        /// penyelesaian tersendiri. Tanpa itu, petugas rekonsiliasi tidak punya cara menyatakan
        /// "masalahnya diketahui, tetapi uangnya diputuskan pihak lain", dan akan tergoda
        /// menutupnya seolah tidak berdampak.
        /// </summary>
        [Fact]
        public void JenisPenyelesaian_MenyediakanPenyerahanKeJalurFinansial()
        {
            Assert.Contains(
                nameof(BillingReconciliationResolutionType.ManualFinancialAction),
                Enum.GetNames<BillingReconciliationResolutionType>());
        }

        private static object[] PermissionDari(string namaMethod)
        {
            var method = typeof(BillingReconciliationController)
                .GetMethod(namaMethod, BindingFlags.Public | BindingFlags.Instance);

            return method!.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments;
        }
    }
}
