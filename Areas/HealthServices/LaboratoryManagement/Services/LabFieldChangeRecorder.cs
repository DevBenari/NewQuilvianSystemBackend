using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Mencatat perubahan nilai ruas ke <see cref="LabFieldChangeLog"/> (<c>LAB-DEC-112</c>,
    /// <c>BE-LAB-57</c>).
    ///
    /// <b>Dipisahkan menjadi kelasnya sendiri justru supaya dapat dipakai ulang.</b>
    /// <c>LAB-DEC-112</c> memilih bentuk tabel yang umum — <c>EntityName</c> beserta
    /// <c>EntityId</c> — dan pemisahan ini menjaga bentuk umum itu tetap berguna ketika ruas
    /// lain kelak perlu dijejaki.
    /// </summary>
    public class LabFieldChangeRecorder
    {
        private readonly ApplicationDbContext _dbContext;

        public LabFieldChangeRecorder(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Membandingkan nilai lama terhadap nilai baru, lalu <b>hanya mencatat yang
        /// benar-benar berubah</b>.
        ///
        /// Mencatat ruas yang nilainya sama berarti memenuhi jejak dengan baris yang nol
        /// menjelaskan apa pun — dan jejak yang penuh kebisingan sama tidak bergunanya dengan
        /// jejak yang kosong.
        /// </summary>
        /// <returns>Jumlah ruas yang benar-benar berubah dan tercatat.</returns>
        public int Record(
            string entityName,
            Guid entityId,
            IEnumerable<(string FieldName, string? OldValue, string? NewValue)> changes,
            Guid? actorUserId,
            DateTime changedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
            ArgumentNullException.ThrowIfNull(changes);

            var rows = new List<LabFieldChangeLog>();

            foreach (var (fieldName, oldValue, newValue) in changes)
            {
                var lama = Normalize(oldValue);
                var baru = Normalize(newValue);

                if (string.Equals(lama, baru, StringComparison.Ordinal))
                    continue;

                rows.Add(new LabFieldChangeLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    FieldName = fieldName,
                    OldValue = Truncate(lama),
                    NewValue = Truncate(baru),

                    // Guid.Empty adalah pelaku yang tidak pernah ada, dan kolom ini nol
                    // ber-foreign key sehingga database TIDAK akan menolaknya.
                    ChangedByUserId = actorUserId == Guid.Empty ? null : actorUserId,

                    ChangedAt = changedAt,
                    CreateDateTime = changedAt,
                    CreateBy = actorUserId ?? Guid.Empty
                });
            }

            if (rows.Count > 0)
                _dbContext.LabFieldChangeLogs.AddRange(rows);

            return rows.Count;
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? Truncate(string? value)
            => value is { Length: > 500 } ? value[..500] : value;
    }
}
