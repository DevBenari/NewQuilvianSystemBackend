using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Penjaga kontrak hak akses bagi kedua controller yang disentuh slice keperawatan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa uji ini ada.</b> <c>BE-RWI-034</c> menemukan sembilan endpoint yang nama pada
    /// <c>[AccessAction]</c> dan <c>[AccessPermission]</c>-nya berbeda. Akibatnya baris untuk
    /// dicentang tidak pernah lahir di layar Akses Role, dan setiap orang selain SuperAdmin
    /// ditolak <c>403</c> selamanya — kesalahan yang <b>tidak terlihat</b> saat pengujian memakai
    /// akun SuperAdmin, karena SuperAdmin melewati seluruh pemeriksaan. Kesalahan yang sama
    /// menahan tujuh task frontend selama hampir seminggu.
    /// </para>
    /// <para>
    /// Uji ini menyandingkan ketiga nilainya huruf demi huruf, sehingga kesalahan itu tertangkap
    /// sebelum sampai ke layar — <c>rules/backend/role-access-rules.md</c> bagian 3 dan 7.
    /// </para>
    /// </remarks>
    public class NursingAssessmentAccessContractTests
    {
        public static TheoryData<Type> ControllerYangDisentuh() =>
        [
            typeof(PatientAssessmentController),
            typeof(ClinicalAssessmentPolicyController)
        ];

        /// <summary>
        /// Setiap action punya <c>[AccessAction]</c> dan <c>[AccessPermission]</c>, dan ketiga
        /// nilainya cocok huruf demi huruf.
        /// </summary>
        [Theory]
        [MemberData(nameof(ControllerYangDisentuh))]
        public void SetiapAction_PunyaPasanganAtributYangNamanyaSama(Type controllerType)
        {
            var controllerAttr = controllerType.GetCustomAttribute<AccessControllerAttribute>();

            Assert.True(controllerAttr != null,
                $"{controllerType.Name} tidak punya [AccessController].");
            Assert.True(controllerType.GetCustomAttribute<AuthorizeAttribute>() != null,
                $"{controllerType.Name} tidak punya [Authorize].");

            var controllerName = controllerAttr!.ControllerName;

            Assert.False(string.IsNullOrWhiteSpace(controllerName),
                $"{controllerType.Name} tidak menyebut ControllerName.");

            foreach (var method in ActionMethods(controllerType))
            {
                var action = method.GetCustomAttribute<AccessActionAttribute>();
                var permission = method.GetCustomAttribute<AccessPermissionAttribute>();

                Assert.True(action != null,
                    $"{controllerType.Name}.{method.Name} tidak punya [AccessAction].");
                Assert.True(permission != null,
                    $"{controllerType.Name}.{method.Name} tidak punya [AccessPermission].");

                var argumen = permission!.Arguments!;

                Assert.Equal(controllerName, (string)argumen[0]!);
                Assert.Equal(action!.ActionName, (string)argumen[1]!);

                Assert.Contains(action.AccessType, AccessTypes.AllowedForRoleAccess);
                Assert.True(action.VisibleInRoleAccess,
                    $"{controllerType.Name}.{method.Name} disembunyikan dari layar Akses Role.");
                Assert.False(action.IsSystemOnly,
                    $"{controllerType.Name}.{method.Name} disetel hanya untuk SuperAdmin.");
            }
        }

        /// <summary>
        /// Tidak ada satu pun endpoint yang dibiarkan terbuka tanpa pemeriksaan hak akses.
        /// </summary>
        [Theory]
        [MemberData(nameof(ControllerYangDisentuh))]
        public void TidakAdaEndpointAnonim(Type controllerType)
        {
            foreach (var method in ActionMethods(controllerType))
            {
                Assert.True(method.GetCustomAttribute<AllowAnonymousAttribute>() == null,
                    $"{controllerType.Name}.{method.Name} bersifat anonim tanpa alasan tertulis.");
            }
        }

        /// <summary>
        /// Kewenangan tidak pernah diturunkan dari nama peran, nama departemen, nama posisi,
        /// maupun <c>UserType</c>.
        /// </summary>
        /// <remarks>
        /// <c>rules/backend/role-access-rules.md</c> bagian 5. Yang diperiksa adalah source dua
        /// berkas baru milik slice ini; hardcode pada kode lama dicatat sebagai temuan pada
        /// laporan task, bukan diperbaiki tanpa wewenang.
        /// </remarks>
        [Theory]
        [InlineData("Areas/HealthServices/MasterData/Services/ClinicalAssessmentPolicyService.cs")]
        [InlineData("Areas/HealthServices/MasterData/Controllers/ClinicalAssessmentPolicyController.cs")]
        [InlineData("Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentMonitoringService.cs")]
        public void SourceBaru_TidakMemakaiHardcodeRole(string jalurRelatif)
        {
            var akar = CariAkarRepository();
            var berkas = Path.Combine(akar, jalurRelatif.Replace('/', Path.DirectorySeparatorChar));

            Assert.True(File.Exists(berkas), $"Berkas {jalurRelatif} tidak ditemukan.");

            var isi = File.ReadAllText(berkas);

            foreach (var antiPola in new[] { "IsInRole", "UserType ==", "SuperAdmin" })
            {
                Assert.False(isi.Contains(antiPola, StringComparison.Ordinal),
                    $"{jalurRelatif} memakai anti-pola hak akses \"{antiPola}\".");
            }
        }

        private static IEnumerable<MethodInfo> ActionMethods(Type controllerType) =>
            controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsSpecialName)
                .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any());

        /// <summary>
        /// Menemukan akar repository dari lokasi berkas uji, tanpa menanam jalur mutlak.
        /// </summary>
        private static string CariAkarRepository()
        {
            var direktori = new DirectoryInfo(AppContext.BaseDirectory);

            while (direktori != null &&
                   !File.Exists(Path.Combine(direktori.FullName, "QuilvianSystemBackend.sln")))
            {
                direktori = direktori.Parent;
            }

            Assert.True(direktori != null, "Akar repository tidak ditemukan dari lokasi uji.");

            return direktori!.FullName;
        }
    }
}
