using System.Linq.Expressions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Satu sumber aturan unit layanan yang boleh dipakai untuk registrasi rawat inap.
    /// Predicate ini dipakai baik saat menampilkan bed maupun saat menyimpan encounter,
    /// sehingga pilihan yang terlihat tidak ditolak oleh validasi pada langkah berikutnya.
    /// </summary>
    public static class ServiceUnitRegistrationRules
    {
        public static Expression<Func<MstServiceUnit, bool>> InpatientRegistrationEligible =>
            unit => unit.IsActive &&
                    !unit.IsDelete &&
                    unit.IsAvailableForRegistration &&
                    unit.ServiceUnitType == ServiceUnitType.Inpatient;
    }
}
