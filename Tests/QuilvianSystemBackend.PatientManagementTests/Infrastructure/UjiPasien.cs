using System.Reflection;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>
    /// Membandingkan seluruh kolom skalar <see cref="MstPatient"/>, supaya uji dapat membuktikan
    /// kolom mana saja yang berubah — bukan hanya kolom yang kebetulan diperiksa.
    /// </summary>
    public static class UjiPasien
    {
        /// <summary>Empat kolom yang boleh berubah oleh rekonsiliasi (`AC-08`).</summary>
        public static readonly string[] KolomBolehBerubah =
        [
            nameof(MstPatient.MedicalRecordNumber),
            nameof(MstPatient.QrCodePath),
            nameof(MstPatient.UpdateDateTime),
            nameof(MstPatient.UpdateBy)
        ];

        public static void SamaPersis(MstPatient sebelum, MstPatient sesudah) =>
            Assert.Empty(KolomBerubah(sebelum, sesudah));

        public static List<string> KolomBerubah(MstPatient sebelum, MstPatient sesudah)
        {
            return KolomSkalar()
                .Where(p => !Equals(p.GetValue(sebelum), p.GetValue(sesudah)))
                .Select(p => p.Name)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
        }

        private static IEnumerable<PropertyInfo> KolomSkalar() =>
            typeof(MstPatient)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSkalar(p.PropertyType));

        private static bool IsSkalar(Type type)
        {
            var dasar = Nullable.GetUnderlyingType(type) ?? type;

            return dasar.IsPrimitive ||
                dasar.IsEnum ||
                dasar == typeof(string) ||
                dasar == typeof(Guid) ||
                dasar == typeof(DateTime) ||
                dasar == typeof(decimal) ||
                dasar == typeof(TimeSpan);
        }
    }
}
