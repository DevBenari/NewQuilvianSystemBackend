using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdFixedTermPolicyService : IHrdFixedTermPolicyService
    {
        // Tanggal mulai berlaku UU No. 27 Tahun 2026 tentang Pelindungan Ketenagakerjaan
        public static readonly DateOnly Uu27Year2026EffectiveDate = new(2026, 10, 28);

        public const int Uu27InitialMaxMonths = 24;      // Pasal 82: Jangka awal paling lama 2 tahun
        public const int Uu27MaxRenewalCount = 2;        // Pasal 82: Maksimal 2 kali perpanjangan
        public const int Uu27MaxMonthsPerRenewal = 12;   // Pasal 82: Masing-masing perpanjangan paling lama 1 tahun
        public const int Uu27NoticeLeadDays = 7;         // Pasal 82 ayat (4): Pemberitahuan tertulis paling lambat 7 hari sebelum kontrak berakhir

        public const int LegacyMaxCumulativeMonths = 60; // PP 35/2021 batas 5 tahun kumulatif

        public FixedTermCheckResponse EvaluateCompliance(WfpContractHistory contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            var isFixedTerm = contract.ContractType?.RequiresEndDate ?? contract.EndDate.HasValue;
            var startDate = DateOnly.FromDateTime(contract.StartDate);
            var endDate = contract.EndDate.HasValue ? DateOnly.FromDateTime(contract.EndDate.Value) : (DateOnly?)null;

            if (!isFixedTerm || !endDate.HasValue)
            {
                return new FixedTermCheckResponse
                {
                    ContractHistoryId = contract.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    FixedTermSeriesStartDate = startDate,
                    TotalUsedMonths = 0,
                    RemainingMonths = 0,
                    CanExtend = false,
                    MaxExtensionMonths = 0,
                    RuleVersion = "N/A",
                    LegalBasis = "Bukan merupakan kontrak PKWT berjangka waktu (Pegawai Tetap/PKWTT).",
                    RenewalSequence = contract.RenewalSequence,
                    MaxRenewalCount = 0,
                    MaxMonthsPerRenewal = 0,
                    RequiredNoticeDate = null,
                    LegalTransitionStatus = null,
                    Message = "Bukan merupakan kontrak PKWT berjangka waktu (Pegawai Tetap/PKWTT)."
                };
            }

            var seriesStart = contract.FixedTermSeriesStartDate.HasValue
                ? DateOnly.FromDateTime(contract.FixedTermSeriesStartDate.Value)
                : startDate;

            var totalUsedMonths = CalculateMonthsBetween(seriesStart, endDate.Value);
            var requiredNoticeDate = endDate.Value.AddDays(-Uu27NoticeLeadDays);

            // Menentukan apakah kontrak tunduk pada UU 27/2026 atau rezim legacy PP 35/2021
            var isGovernedByUu27 = seriesStart >= Uu27Year2026EffectiveDate || contract.StartDate >= new DateTime(2026, 10, 28);

            if (isGovernedByUu27)
            {
                // Rezim UU No. 27 Tahun 2026
                var renewalSeq = contract.RenewalSequence;
                var validationErrors = new List<string>();

                // Periksa durasi kontrak awal jika ini kontrak awal
                if (renewalSeq == 0 && totalUsedMonths > Uu27InitialMaxMonths)
                {
                    validationErrors.Add($"Durasi kontrak PKWT awal ({totalUsedMonths} bulan) melebihi batas maksimal 24 bulan menurut UU No. 27/2026 Pasal 82.");
                }

                // Cek kelayakan perpanjangan lanjutan
                bool canExtend;
                int maxExtensionMonths;
                string message;

                if (renewalSeq >= Uu27MaxRenewalCount)
                {
                    canExtend = false;
                    maxExtensionMonths = 0;
                    message = $"Kontrak ini telah mencapai batas maksimal {Uu27MaxRenewalCount} kali perpanjangan menurut UU No. 27/2026 Pasal 82. Kontrak harus diangkat menjadi pegawai tetap (PKWTT) atau tidak diperpanjang.";
                    validationErrors.Add("Perpanjangan ke-3 dilarang oleh UU No. 27/2026 Pasal 82 (maksimal 2 kali perpanjangan).");
                }
                else
                {
                    canExtend = true;
                    maxExtensionMonths = Uu27MaxMonthsPerRenewal;
                    var nextSeq = renewalSeq + 1;
                    message = $"Kontrak memenuhi syarat untuk perpanjangan ke-{nextSeq} (maksimal 12 bulan per perpanjangan, maksimal {Uu27MaxRenewalCount} kali). Pemberitahuan tertulis wajib disampaikan paling lambat {requiredNoticeDate:dd-MM-yyyy} (H-7).";
                }

                return new FixedTermCheckResponse
                {
                    ContractHistoryId = contract.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    FixedTermSeriesStartDate = seriesStart,
                    TotalUsedMonths = totalUsedMonths,
                    RemainingMonths = canExtend ? maxExtensionMonths : 0,
                    CanExtend = canExtend,
                    MaxExtensionMonths = maxExtensionMonths,
                    RuleVersion = "UU-27-2026",
                    LegalBasis = "Undang-Undang Nomor 27 Tahun 2026 tentang Pelindungan Ketenagakerjaan (Pasal 82-87)",
                    RenewalSequence = renewalSeq,
                    MaxRenewalCount = Uu27MaxRenewalCount,
                    MaxMonthsPerRenewal = Uu27MaxMonthsPerRenewal,
                    RequiredNoticeDate = requiredNoticeDate,
                    LegalTransitionStatus = "ACTIVE_LAW",
                    ValidationErrors = validationErrors,
                    Message = message
                };
            }
            else
            {
                // Rezim Historis / Transisi (PP No. 35/2021 batas 5 tahun kumulatif, berstatus NEEDS_SYNC)
                var remainingMonths = Math.Max(0, LegacyMaxCumulativeMonths - totalUsedMonths);
                var canExtend = remainingMonths > 0 && contract.RenewalSequence < Uu27MaxRenewalCount;

                var message = canExtend
                    ? $"Kontrak historis (dimulai sebelum 28 Okt 2026). Sisa kumulatif batas 5 tahun adalah {remainingMonths} bulan. Perpanjangan baru yang berlaku sesudah 28 Okt 2026 tunduk pada batas maksimal 12 bulan per perpanjangan (UU 27/2026)."
                    : "Masa PKWT kumulatif telah mencapai batas maksimal. Hubungan kerja harus dikonversi ke tetap (PKWTT) atau diakhiri.";

                return new FixedTermCheckResponse
                {
                    ContractHistoryId = contract.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    FixedTermSeriesStartDate = seriesStart,
                    TotalUsedMonths = totalUsedMonths,
                    RemainingMonths = remainingMonths,
                    CanExtend = canExtend,
                    MaxExtensionMonths = Math.Min(remainingMonths, Uu27MaxMonthsPerRenewal),
                    RuleVersion = "PP-35-2021",
                    LegalBasis = "PP No. 35/2021 & UU No. 27/2026 (Transisi Legal)",
                    RenewalSequence = contract.RenewalSequence,
                    MaxRenewalCount = Uu27MaxRenewalCount,
                    MaxMonthsPerRenewal = Uu27MaxMonthsPerRenewal,
                    RequiredNoticeDate = requiredNoticeDate,
                    LegalTransitionStatus = "NEEDS_SYNC",
                    ValidationErrors = new List<string>(),
                    Message = message
                };
            }
        }

        public (bool IsValid, string? ErrorMessage) ValidateRenewalProposal(
            WfpContractHistory contract,
            DateOnly proposedStartDate,
            DateOnly? proposedEndDate,
            int? extensionMonths)
        {
            if (contract == null) return (false, "Kontrak tidak ditemukan.");

            if (!contract.EndDate.HasValue)
            {
                return (false, "Kontrak asal tidak memiliki tanggal berakhir (bukan PKWT berjangka waktu).");
            }

            var contractEndDate = DateOnly.FromDateTime(contract.EndDate.Value);

            // Validasi kesinambungan tanggal: usulan mulai tidak boleh sebelum tanggal akhir kontrak asal atau lompat mundur
            if (proposedStartDate < contractEndDate)
            {
                return (false, $"Tanggal mulai perpanjangan ({proposedStartDate:dd-MM-yyyy}) tidak boleh sebelum tanggal akhir kontrak berjalan ({contractEndDate:dd-MM-yyyy}).");
            }

            if (!proposedEndDate.HasValue)
            {
                return (false, "Tanggal berakhir perpanjangan (ProposedEndDate) wajib diisi untuk perpanjangan PKWT.");
            }

            if (proposedEndDate.Value <= proposedStartDate)
            {
                return (false, "Tanggal berakhir perpanjangan harus lebih besar daripada tanggal mulai perpanjangan.");
            }

            var months = CalculateMonthsBetween(proposedStartDate, proposedEndDate.Value);

            // Validasi batasan UU No. 27 Tahun 2026
            var seriesStart = contract.FixedTermSeriesStartDate.HasValue
                ? DateOnly.FromDateTime(contract.FixedTermSeriesStartDate.Value)
                : DateOnly.FromDateTime(contract.StartDate);

            var isGovernedByUu27 = seriesStart >= Uu27Year2026EffectiveDate || proposedStartDate >= Uu27Year2026EffectiveDate;

            if (isGovernedByUu27)
            {
                // 1. Batas maksimal perpanjangan: 2 kali (Pasal 82)
                if (contract.RenewalSequence >= Uu27MaxRenewalCount)
                {
                    return (false, $"VR-ONB-36: Kontrak telah diperpanjang sebanyak {contract.RenewalSequence} kali. UU No. 27/2026 Pasal 82 membatasi perpanjangan PKWT maksimal 2 kali. Usulan perpanjangan ke-3 ditolak.");
                }

                // 2. Batas durasi per perpanjangan: maksimal 1 tahun / 12 bulan (Pasal 82)
                if (months > Uu27MaxMonthsPerRenewal)
                {
                    return (false, $"VR-ONB-36: Usulan durasi perpanjangan ({months} bulan) melebihi batas maksimal 1 tahun (12 bulan) per perpanjangan sesuai UU No. 27/2026 Pasal 82.");
                }

                // 3. Konsistensi ExtensionMonths jika diberikan
                if (extensionMonths.HasValue && extensionMonths.Value > Uu27MaxMonthsPerRenewal)
                {
                    return (false, $"VR-ONB-36: Nilai ExtensionMonths ({extensionMonths.Value} bulan) melebihi batas maksimal 12 bulan.");
                }
            }
            else
            {
                // Fallback transisi: tidak boleh melebihi batas 5 tahun kumulatif dan perpanjangan baru tetap dibatasi 12 bulan
                var totalMonths = CalculateMonthsBetween(seriesStart, proposedEndDate.Value);
                if (totalMonths > LegacyMaxCumulativeMonths)
                {
                    return (false, $"VR-ONB-36: Total durasi PKWT kumulatif ({totalMonths} bulan) melebihi batas maksimal 5 tahun (60 bulan).");
                }

                if (months > Uu27MaxMonthsPerRenewal)
                {
                    return (false, $"VR-ONB-36: Perpanjangan kontrak yang berlaku pada periode ini dibatasi maksimal 12 bulan per perpanjangan sesuai UU No. 27/2026.");
                }
            }

            return (true, null);
        }

        private static int CalculateMonthsBetween(DateOnly start, DateOnly end)
        {
            var months = ((end.Year - start.Year) * 12) + end.Month - start.Month;
            if (end.Day < start.Day)
            {
                // Bulan parsial belum penuh
                months = Math.Max(0, months - 1);
            }
            // Jika rentang dari tanggal 1 sampai akhir bulan atau hari yang sama, hitung penuh
            if (months < 0) months = 0;
            return months;
        }
    }
}
