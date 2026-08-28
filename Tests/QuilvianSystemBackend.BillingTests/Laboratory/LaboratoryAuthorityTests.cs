using System.Reflection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Laboratory
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-003 yang dapat dibuktikan tanpa database.
    ///
    /// Test di sini bekerja pada permukaan kontrak dan permukaan API. Yang dijaga adalah
    /// keberadaan jalurnya: selama Laboratorium tidak memiliki route atau method finansial,
    /// tidak ada izin klinis yang dapat dipakai untuk menyatakan sesuatu lunas atau
    /// dibatalkan secara finansial. Test ini sekaligus menjadi pengaman regresi bila suatu
    /// saat seseorang menambahkannya kembali.
    /// </summary>
    public sealed class LaboratoryAuthorityTests
    {
        /// <summary>
        /// Istilah finansial yang tidak boleh muncul sebagai anggota apa pun pada Laboratorium.
        /// </summary>
        private static readonly string[] IstilahFinansialTerlarang =
        {
            "Paid",
            "Payment",
            "Settlement",
            "PayerApproval",
            "Void",
            "Refund",
            "Reversal",
            "WriteOff",
            "Invoice"
        };

        [Fact]
        public void BillingSourceContract_MenerimaLaboratory()
        {
            Assert.True(BillingSourceContract.IsKnownSourceContext(
                BillingSourceContract.LaboratorySourceContext));

            Assert.True(BillingSourceContract.IsAllowedEffectType(
                BillingSourceContract.LaboratorySourceContext,
                BillingSourceContract.LaboratoryChargeEffectType));
        }

        [Fact]
        public void BillingSourceContract_MasihMenolakRadiology()
        {
            // RJ-BIL-BE-004 belum dikerjakan. Gerbang kontrak harus tetap menutup Radiology
            // agar modul yang belum punya boundary tidak dapat membentuk tagihan.
            Assert.False(BillingSourceContract.IsKnownSourceContext("Radiology"));
            Assert.False(BillingSourceContract.IsAllowedEffectType("Radiology", "RadiologyCharge"));
        }

        [Fact]
        public void BillingSourceContract_MenolakEffectTypeYangTidakCocokDenganLaboratory()
        {
            Assert.False(BillingSourceContract.IsAllowedEffectType(
                BillingSourceContract.LaboratorySourceContext,
                BillingSourceContract.PrescriptionChargeEffectType));

            Assert.False(BillingSourceContract.IsAllowedEffectType(
                BillingSourceContract.LaboratorySourceContext,
                "LaboratorySettlement"));
        }

        [Fact]
        public void ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun()
        {
            Type[] modelLaboratorium =
            {
                typeof(LabOrder),
                typeof(LabSpecimen),
                typeof(LabTransitionHistory),
                typeof(MstLabRejectionReason)
            };

            foreach (var model in modelLaboratorium)
            {
                var namaProperti = model
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
        public void ServiceLaboratorium_TidakMemilikiMethodKewenanganFinansial()
        {
            Type[] serviceLaboratorium =
            {
                typeof(LabOrderService),
                typeof(LabSpecimenService)
            };

            foreach (var service in serviceLaboratorium)
            {
                var namaMethod = service
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
        }

        /// <summary>
        /// RJ-BIL-GATE-DEC-003: pengambilan, penerimaan, dan penetapan kelayakan memakai
        /// kewenangan berbeda. Satu permission tunggal untuk seluruh langkah akan membuat
        /// petugas yang hanya berhak mengambil sampel ikut dapat menyatakan sampel layak
        /// periksa, dan penetapan layak itulah yang membentuk tagihan.
        /// </summary>
        [Theory]
        [InlineData(nameof(LabSpecimenController.Plan), "LabSpecimen", "Plan")]
        [InlineData(nameof(LabSpecimenController.Collect), "LabSpecimen", "Collect")]
        [InlineData(nameof(LabSpecimenController.Receive), "LabSpecimen", "Receive")]
        [InlineData(nameof(LabSpecimenController.Accept), "LabSpecimen", "Accept")]
        [InlineData(nameof(LabSpecimenController.Reject), "LabSpecimen", "Accept")]
        [InlineData(nameof(LabSpecimenController.RequestRecollection), "LabSpecimen", "Accept")]
        [InlineData(nameof(LabSpecimenController.Cancel), "LabSpecimen", "Cancel")]
        public void EndpointSampel_MemakaiPermissionYangDitetapkan(
            string namaMethod,
            string controllerName,
            string actionName)
        {
            var method = typeof(LabSpecimenController)
                .GetMethod(namaMethod, BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

            Assert.NotNull(permission);
            Assert.Equal(new object[] { controllerName, actionName }, permission!.Arguments);
        }

        [Fact]
        public void PermissionPengambilanDanPenetapanLayak_TidakBolehSama()
        {
            var collect = PermissionDari(nameof(LabSpecimenController.Collect));
            var receive = PermissionDari(nameof(LabSpecimenController.Receive));
            var accept = PermissionDari(nameof(LabSpecimenController.Accept));

            Assert.NotEqual(collect, accept);
            Assert.NotEqual(receive, accept);
            Assert.NotEqual(collect, receive);
        }

        /// <summary>
        /// Barcode sampel tidak boleh memuat informasi pasien. Pembuktian paling kuat yang
        /// tersedia tanpa database adalah bahwa pembangkitnya tidak menerima masukan apa pun,
        /// sehingga secara struktural tidak mungkin menyisipkan identitas pasien ke dalamnya.
        /// </summary>
        [Fact]
        public void PembangkitBarcode_TidakMenerimaMasukanApaPun()
        {
            var generator = typeof(LabSpecimenService)
                .GetMethod("GenerateSpecimenBarcode", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(generator);
            Assert.Empty(generator!.GetParameters());
        }

        [Fact]
        public void BarcodeSampel_BerbentukLspDiikutiTigaPuluhDuaHeksadesimal()
        {
            var generator = typeof(LabSpecimenService)
                .GetMethod("GenerateSpecimenBarcode", BindingFlags.NonPublic | BindingFlags.Static);

            var dihasilkan = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < 200; i++)
            {
                var barcode = (string)generator!.Invoke(null, null)!;

                Assert.Matches("^LSP-[0-9A-F]{32}$", barcode);
                Assert.True(dihasilkan.Add(barcode), "Barcode yang dihasilkan tidak unik.");
            }
        }

        /// <summary>
        /// Siklus hidup yang dikunci RJ-BIL-GATE-DEC-003 harus tersedia seluruhnya, tidak
        /// kurang dan tidak ditambah status karangan.
        /// </summary>
        [Fact]
        public void SiklusHidupPesanan_SamaPersisDenganKeputusanTerkunci()
        {
            var diharapkan = new[]
            {
                "Draft", "Requested", "Accepted", "InProcess", "Completed",
                "OnHold", "CancelRequested", "Cancelled"
            };

            Assert.Equal(diharapkan.OrderBy(x => x), Enum.GetNames<LabOrderStatus>().OrderBy(x => x));
        }

        [Fact]
        public void SiklusHidupSampel_SamaPersisDenganKeputusanTerkunci()
        {
            var diharapkan = new[]
            {
                "Planned", "Collected", "Received", "Accepted",
                "Rejected", "RecollectionRequired", "Cancelled", "OnHold"
            };

            Assert.Equal(diharapkan.OrderBy(x => x), Enum.GetNames<LabSpecimenStatus>().OrderBy(x => x));
        }

        /// <summary>
        /// Enum tidak boleh memiliki anggota bernilai 0. Kolom status berjenis integer dengan
        /// nilai bawaan 0 akan menghasilkan baris yang tidak dapat dibaca kembali sebagai
        /// status mana pun — persis cacat yang ditemukan pada migration BE-003.
        /// </summary>
        [Fact]
        public void StatusLaboratorium_TidakMemakaiNilaiNol()
        {
            Assert.DoesNotContain(0, Enum.GetValues<LabOrderStatus>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<LabSpecimenStatus>().Select(x => (int)x));
            Assert.DoesNotContain(0, Enum.GetValues<LabRecollectionCause>().Select(x => (int)x));
        }

        private static object[] PermissionDari(string namaMethod)
        {
            var method = typeof(LabSpecimenController)
                .GetMethod(namaMethod, BindingFlags.Public | BindingFlags.Instance);

            return method!.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments;
        }
    }
}
