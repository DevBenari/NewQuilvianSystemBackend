using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    public sealed class BarisPerencana
    {
        public Guid BatchId { get; init; }

        public string LegacyPid { get; init; } = string.Empty;

        public string PlanStatus { get; set; } = "FINALIZED";

        public string? FinalMedicalRecordNumber { get; set; }
    }

    public sealed class BarisCrosswalk
    {
        public string LegacyPid { get; init; } = string.Empty;

        public string? QuilvianPatientId { get; set; }

        /// <summary>Bukti historis Pilot V1. Sengaja dibuat berbeda dari MRN kanonik pada uji.</summary>
        public string? NormalizedMrn { get; init; }
    }

    public sealed class BarisCadangan
    {
        public Guid BatchId { get; init; }

        public string LegacyPid { get; init; } = string.Empty;

        public Guid PatientId { get; init; }

        public string OldMedicalRecordNumber { get; init; } = string.Empty;

        public string PlannedMedicalRecordNumber { get; init; } = string.Empty;

        public string? OldQrCodePath { get; init; }
    }

    /// <summary>
    /// Tiruan tabel <c>migration.rsmmc_*</c> di memori, meniru semantik kueri
    /// <see cref="RsmmcPilotMigrationSource"/>.
    /// </summary>
    /// <remarks>
    /// Antarmuka yang ditiru hanya punya operasi baca. <see cref="Snapshot"/> dipakai uji untuk
    /// membuktikan isi ketiga tabel tidak berubah sesudah rekonsiliasi.
    /// </remarks>
    public sealed class TiruanSumberMigrasiRsmmc : IRsmmcPilotMigrationSource
    {
        public List<BarisPerencana> Perencana { get; } = [];

        public List<BarisCrosswalk> Crosswalk { get; } = [];

        public List<BarisCadangan> Cadangan { get; } = [];

        /// <summary>Baris FINALIZED tambahan yang bukan Pilot, tanpa perlu dimaterialisasi.</summary>
        public int BarisPerencanaNonPilotTambahan { get; set; }

        public int JumlahPembacaan { get; private set; }

        /// <summary>Dipanggil sebelum pembacaan ulang per pasien, untuk mensimulasikan perubahan data.</summary>
        public Action<string>? SebelumPembacaanUlang { get; set; }

        public Task<IReadOnlyList<RsmmcPilotPlanRow>> GetMappedFinalizedRowsAsync(
            Guid batchId,
            CancellationToken cancellationToken)
        {
            JumlahPembacaan++;

            var hasil = Perencana
                .Where(p => p.BatchId == batchId && p.PlanStatus == "FINALIZED")
                .Join(Crosswalk, p => p.LegacyPid, c => c.LegacyPid, (p, c) => (p, c))
                .Where(x => x.c.QuilvianPatientId != null)
                .Select(x => Baris(x.p, x.c))
                // Sengaja dibalik, supaya uji membuktikan service mengurutkan sendiri.
                .Reverse()
                .ToList();

            return Task.FromResult<IReadOnlyList<RsmmcPilotPlanRow>>(hasil);
        }

        public Task<IReadOnlyList<RsmmcPilotPlanRow>> GetRowsForRevalidationAsync(
            Guid batchId,
            string legacyPid,
            CancellationToken cancellationToken)
        {
            JumlahPembacaan++;
            SebelumPembacaanUlang?.Invoke(legacyPid);

            var hasil = Perencana
                .Where(p => p.BatchId == batchId && p.LegacyPid == legacyPid)
                .SelectMany(
                    p => Crosswalk.Where(c => c.LegacyPid == p.LegacyPid).DefaultIfEmpty(),
                    (p, c) => Baris(p, c))
                .ToList();

            return Task.FromResult<IReadOnlyList<RsmmcPilotPlanRow>>(hasil);
        }

        public Task<int> CountFinalizedPlanRowsAsync(Guid batchId, CancellationToken cancellationToken)
        {
            JumlahPembacaan++;

            var jumlah = Perencana.Count(p => p.BatchId == batchId && p.PlanStatus == "FINALIZED") +
                BarisPerencanaNonPilotTambahan;

            return Task.FromResult(jumlah);
        }

        public Task<int> CountBackupRowsAsync(Guid batchId, CancellationToken cancellationToken)
        {
            JumlahPembacaan++;
            return Task.FromResult(Cadangan.Count(b => b.BatchId == batchId));
        }

        /// <summary>Isi ketiga tabel sebagai teks, untuk dibandingkan sebelum dan sesudah.</summary>
        public string Snapshot()
        {
            var teks = new StringBuilder();

            foreach (var p in Perencana)
            {
                teks.AppendLine($"P|{p.BatchId}|{p.LegacyPid}|{p.PlanStatus}|{p.FinalMedicalRecordNumber}");
            }

            foreach (var c in Crosswalk)
            {
                teks.AppendLine($"C|{c.LegacyPid}|{c.QuilvianPatientId}|{c.NormalizedMrn}");
            }

            foreach (var b in Cadangan)
            {
                teks.AppendLine($"B|{b.BatchId}|{b.LegacyPid}|{b.PatientId}|{b.OldMedicalRecordNumber}|{b.PlannedMedicalRecordNumber}|{b.OldQrCodePath}");
            }

            return teks.ToString();
        }

        private static RsmmcPilotPlanRow Baris(BarisPerencana p, BarisCrosswalk? c) => new()
        {
            LegacyPid = p.LegacyPid,
            PlanStatus = p.PlanStatus,
            FinalMedicalRecordNumber = p.FinalMedicalRecordNumber,
            QuilvianPatientId = c?.QuilvianPatientId
        };
    }
}
