using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.ClinicalIntegration
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-002: clinical endpoint tidak menetapkan status finansial.
    ///
    /// Test di sini bekerja pada permukaan API, bukan pada database, karena yang dijaga adalah
    /// keberadaan jalurnya. Selama route dan method-nya masih ada, siapa pun yang memegang
    /// izin klinis `Prescription : Update` tetap dapat menyatakan resep lunas — walaupun tidak
    /// ada satu pun layar yang memanggilnya.
    ///
    /// Test ini juga berfungsi sebagai pengaman regresi: bila suatu saat seseorang
    /// mengembalikan endpoint tersebut, test ini gagal lebih dulu sebelum sampai ke produksi.
    /// </summary>
    public sealed class ClinicalFinancialAuthorityTests
    {
        private static readonly string[] RouteFinansialYangDihapus =
        {
            "billing-generated",
            "payment-paid",
            "insurance-approved",
            "payment-waived"
        };

        private static readonly string[] MethodFinansialYangDihapus =
        {
            "MarkBillingGeneratedAsync",
            "MarkPaidAsync",
            "MarkInsuranceApprovedAsync",
            "MarkPaymentWaivedAsync"
        };

        [Fact]
        public void PrescriptionController_TidakLagiMemilikiRouteFinansial()
        {
            var templates = typeof(PrescriptionController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetCustomAttributes<HttpMethodAttribute>())
                .Select(attribute => attribute.Template ?? string.Empty)
                .ToList();

            foreach (var route in RouteFinansialYangDihapus)
            {
                Assert.DoesNotContain(
                    templates,
                    template => template.Contains(route, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void PrescriptionWorkflowService_TidakLagiMemilikiKewenanganFinansial()
        {
            var methodNames = typeof(PrescriptionWorkflowService)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .ToList();

            foreach (var methodName in MethodFinansialYangDihapus)
                Assert.DoesNotContain(methodName, methodNames);

            Assert.DoesNotContain("CompletePaymentAsync", methodNames);
        }

        [Fact]
        public void PembatalanKlinis_TidakLagiMenulisPaymentStatus()
        {
            // CancelAsync tetap ada karena pembatalan resep adalah alur dokter yang aktif
            // dipakai. Yang dihapus hanyalah kewenangannya atas status pembayaran.
            var cancelAsync = typeof(PrescriptionWorkflowService)
                .GetMethod("CancelAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(cancelAsync);

            // Nilai enum pembatalan pembayaran dibiarkan ada demi kompatibilitas data lama,
            // tetapi tidak boleh lagi ditulis modul klinis. Penulisnya kini hanya Billing.
            Assert.True(Enum.IsDefined(typeof(PrescriptionPaymentStatus), PrescriptionPaymentStatus.Cancelled));
        }
    }
}
