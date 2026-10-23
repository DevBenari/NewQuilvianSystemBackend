using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Responses;

using MstTariff = QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstTariff;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Booking Radiologi v1 (<c>FormBookingRadiologi</c>, dibaca dari v1 tayang 2026-10-16):
    /// cari pasien dengan No. RM, isi Form Order Pemeriksaan, tambahkan beberapa pemeriksaan
    /// (Cito, Dokter Pemeriksa, Harga), lalu Simpan.
    /// </summary>
    /// <remarks>
    /// Satu pesanan V2 = satu pemeriksaan, sehingga satu booking menjadi beberapa pesanan untuk satu
    /// kunjungan dengan tanggal pemeriksaan yang sama — persis satu baris Daftar Pasien. Seluruhnya
    /// dibuat dalam satu transaksi: sebagian pemeriksaan yang tersimpan tidak pernah terjadi.
    /// </remarks>
    public partial class RadOrderService
    {
        /// <summary>Batas pemeriksaan per booking — mencegah permintaan tak wajar.</summary>
        internal const int MaksimumPemeriksaanBooking = 20;

        /// <summary>
        /// Kunjungan pasien yang dapat dibooking dari No. RM. Pencarian mengabaikan tanda hubung
        /// ("25-31-10-72" = "25311072"), seperti v1. Kunjungan batal dan tidak hadir tidak ikut;
        /// yang terbaru lebih dulu.
        /// </summary>
        public async Task<List<RadBookingEncounterResponse>> GetBookingEncountersAsync(
            string? medicalRecordNumber,
            CancellationToken cancellationToken = default)
        {
            var digit = new string((medicalRecordNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digit.Length == 0) return new List<RadBookingEncounterResponse>();

            var kunjungan = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(e => !e.IsDelete &&
                            e.EncounterStatus != EncounterStatus.Cancelled &&
                            e.EncounterStatus != EncounterStatus.NoShow &&
                            e.Patient != null &&
                            e.Patient.MedicalRecordNumber.Replace("-", "").Replace(".", "").Replace(" ", "") == digit)
                .OrderByDescending(e => e.EncounterDate)
                .Take(5)
                .Select(e => new
                {
                    e.Id,
                    e.EncounterDate,
                    e.EncounterNumber,
                    e.EncounterType,
                    e.VisitType,
                    e.PaymentType,
                    e.PatientId,
                    e.AgeTextAtEncounter,
                    e.ServiceUnitId,
                    ServiceUnitName = e.ServiceUnit != null ? e.ServiceUnit.ServiceUnitName : null,
                    e.RoomId,
                    RoomName = e.Room != null ? e.Room.RoomName : null,
                    e.PatientClassId,
                    PatientClassName = e.PatientClass != null ? e.PatientClass.PatientClassName : null,
                    GuarantorName = e.PaymentSource != null ? e.PaymentSource.PaymentSourceNameSnapshot : null,
                    PatientName = e.Patient!.FullName,
                    e.Patient.MedicalRecordNumber,
                    e.Patient.Gender,
                    e.Patient.BirthDate,
                    Telepon = e.Patient.WhatsAppNumber ?? e.Patient.PhoneNumber,
                    e.DoctorId,
                    DoctorName = e.Doctor != null ? e.Doctor.FullName : null,
                })
                .ToListAsync(cancellationToken);

            if (kunjungan.Count == 0) return new List<RadBookingEncounterResponse>();

            var encounterIds = kunjungan.Select(x => x.Id).ToList();
            var diagnosa = await _dbContext.TrxPatientDiagnoses
                .AsNoTracking()
                .Where(d => encounterIds.Contains(d.EncounterId) && !d.IsDelete)
                .OrderByDescending(d => d.IsPrimary)
                .ThenBy(d => d.DiagnosisDateTime)
                .Select(d => new { d.EncounterId, d.DiagnosisCode, d.DiagnosisName })
                .ToListAsync(cancellationToken);

            var diagnosaPerKunjungan = diagnosa
                .GroupBy(d => d.EncounterId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g
                        .Select(d => string.IsNullOrWhiteSpace(d.DiagnosisCode)
                            ? d.DiagnosisName
                            : $"{d.DiagnosisCode} - {d.DiagnosisName}")
                        .Distinct()));

            return kunjungan.Select(x => new RadBookingEncounterResponse
            {
                EncounterId = x.Id,
                EncounterDate = x.EncounterDate,
                Patient = new RadOrderPatientContextResponse
                {
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    PatientName = x.PatientName,
                    Gender = x.Gender?.ToString(),
                    BirthDate = x.BirthDate,
                    AgeText = x.AgeTextAtEncounter,
                    EncounterNumber = x.EncounterNumber,
                    EncounterType = x.EncounterType.ToString(),
                    VisitType = x.VisitType.ToString(),
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnitName,
                    RoomId = x.RoomId,
                    RoomName = x.RoomName,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClassName,
                    PaymentType = x.PaymentType.ToString(),
                    GuarantorName = x.GuarantorName,
                },
                PhoneNumber = string.IsNullOrWhiteSpace(x.Telepon) ? null : x.Telepon.Trim(),
                DoctorId = x.DoctorId,
                DoctorName = x.DoctorName,
                DiagnosisText = diagnosaPerKunjungan.GetValueOrDefault(x.Id),
            }).ToList();
        }

        /// <summary>
        /// Harga tampilan satu pemeriksaan dan alat yang diturunkan dari pemeriksaannya. Tarif dipilih dengan aturan
        /// yang sama dengan Billing (<see cref="MstTariffSelector"/>): kelas pasien lebih dulu, lalu tarif umum;
        /// dua tarif sama spesifik dengan harga berbeda dilaporkan <c>AMBIGUOUS</c> tanpa harga.
        /// </summary>
        /// <param name="patientClassId">
        /// Kelas pratinjau (opsional). Kosong = kelas kunjungan. Hanya mengubah harga yang ditampilkan; kelas
        /// kunjungan tidak diubah dan Billing tetap menagih menurut kelas kunjungan.
        /// </param>
        public async Task<RadOperationResult<RadBookingPriceResponse>> GetBookingPriceAsync(
            Guid encounterId,
            Guid procedureId,
            CancellationToken cancellationToken = default,
            Guid? patientClassId = null)
        {
            var tindakan = await _dbContext.MstProcedures.AsNoTracking()
                .Where(p => p.Id == procedureId && !p.IsDelete)
                .Select(p => new { p.Id, p.ProcedureName, p.ProcedureCategoryName, p.IsRadiology, p.IsActive })
                .FirstOrDefaultAsync(cancellationToken);
            if (tindakan == null || !tindakan.IsRadiology || !tindakan.IsActive)
            {
                return RadOperationResult<RadBookingPriceResponse>.NotFound(
                    RadErrorCodes.ProcedureNotFound, "Pemeriksaan radiologi tidak ditemukan atau tidak aktif.");
            }

            var konteks = await LoadTariffContextAsync(encounterId, cancellationToken);
            var kelasId = patientClassId is { } dipilih && dipilih != Guid.Empty ? dipilih : konteks.EncounterClassId;
            var alat = await TurunkanAlatAsync(tindakan.ProcedureName, tindakan.ProcedureCategoryName, cancellationToken);

            var respons = new RadBookingPriceResponse
            {
                ProcedureId = tindakan.Id,
                ModalityId = alat?.Id,
                ModalityCode = alat?.ModalityCode,
                ModalityName = alat?.ModalityName,
                EncounterPatientClassId = konteks.EncounterClassId,
                EncounterPatientClassName = konteks.EncounterClassName,
                PriceBasisPatientClassId = kelasId,
                IsPreviewClass = kelasId != konteks.EncounterClassId,
            };

            if (!konteks.EncounterFound)
            {
                respons.TariffStatus = RadTariffStatuses.EncounterRequired;
                respons.TariffMessage = DescribeTariffProblem(respons.TariffStatus, null);
                return RadOperationResult<RadBookingPriceResponse>.Success(respons);
            }

            var sekarang = DateTime.UtcNow;
            var pilihan = (await SelectTariffsAsync(new[] { procedureId }, kelasId, konteks.ClinicId, sekarang, cancellationToken))[procedureId];
            var namaKelas = await LoadPatientClassNamesAsync(
                new Guid?[] { kelasId, pilihan.Chosen?.PatientClassId }.Concat(pilihan.Tied.Select(t => t.PatientClassId)),
                cancellationToken);
            var namaDipakai = kelasId.HasValue ? namaKelas.GetValueOrDefault(kelasId.Value) : null;

            respons.PriceBasisPatientClassName = namaDipakai;
            respons.TariffStatus = StatusFrom(pilihan.Status);
            respons.TariffMessage = DescribeTariffProblem(respons.TariffStatus, namaDipakai, pilihan.Tied);

            if (pilihan.Chosen is { } tarif)
            {
                respons.TariffAmount = tarif.NormalPrice;
                respons.TariffId = tarif.Id;
                respons.TariffCode = tarif.TariffCode;
                respons.TariffName = tarif.TariffName;
                respons.TariffPatientClassId = tarif.PatientClassId;
                respons.EffectiveStartDate = tarif.EffectiveStartDate;
                respons.EffectiveEndDate = tarif.EffectiveEndDate;
            }

            return RadOperationResult<RadBookingPriceResponse>.Success(respons);
        }

        /// <summary>
        /// Pilihan "Pilih Diagnosa ICD-10" Booking v1 dari master diagnosis — syarat sama dengan
        /// <c>diagnoses/clinical-options</c> (ICD-10 klinis, aktif, dapat dipilih). Disajikan Radiologi
        /// sendiri karena pembooking tidak memegang <c>Diagnosis : Read</c> master data — pola
        /// <see cref="GetProcedureOptionsAsync"/>. Kode yang diawali kata kunci tampil lebih dulu.
        /// </summary>
        public async Task<PagedResult<RadBookingDiagnosisOptionResponse>> GetBookingDiagnosisOptionsAsync(
            RadAssigneeOptionQuery? query,
            CancellationToken cancellationToken = default)
        {
            var (halaman, ukuran, cari) = RapikanPilihan(query);

            var source = _dbContext.MstDiagnoses
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsSelectableForClinicalUse &&
                            x.DiagnosisType == "ICD10" && x.IcdVersion == "ICD-10");

            if (cari != null)
            {
                source = source.Where(x =>
                    x.DiagnosisCode.ToLower().Contains(cari) ||
                    x.DiagnosisName.ToLower().Contains(cari));
            }

            var total = await source.CountAsync(cancellationToken);
            var items = await source
                .OrderBy(x => cari != null && x.DiagnosisCode.ToLower().StartsWith(cari) ? 0 : 1)
                .ThenBy(x => x.DiagnosisCode)
                .Skip((halaman - 1) * ukuran).Take(ukuran)
                .Select(x => new RadBookingDiagnosisOptionResponse
                {
                    Id = x.Id,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RadBookingDiagnosisOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = items,
            };
        }

        /// <summary>Menyimpan Booking Radiologi v1: satu pesanan per pemeriksaan, satu transaksi.</summary>
        public async Task<RadOperationResult<RadBookingResponse>> CreateBookingAsync(
            RadBookingRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var butir = request.Items ?? new List<RadBookingItemRequest>();

            if (butir.Count == 0)
            {
                return RadOperationResult<RadBookingResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Tambahkan pemeriksaan radiologi.");
            }

            if (butir.Count > MaksimumPemeriksaanBooking)
            {
                return RadOperationResult<RadBookingResponse>.Validation(
                    RadErrorCodes.ValidationFailed, $"Satu booking paling banyak {MaksimumPemeriksaanBooking} pemeriksaan.");
            }

            if (butir.Select(x => x.ProcedureId).Distinct().Count() != butir.Count)
            {
                return RadOperationResult<RadBookingResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Pemeriksaan tidak boleh duplikat.");
            }

            var kunjungan = await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(e => e.Id == request.EncounterId && !e.IsDelete)
                .Select(e => new { e.Id, e.EncounterStatus })
                .FirstOrDefaultAsync(cancellationToken);
            if (kunjungan == null || kunjungan.EncounterStatus is EncounterStatus.Cancelled or EncounterStatus.NoShow)
            {
                return RadOperationResult<RadBookingResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Kunjungan pasien tidak ditemukan atau sudah dibatalkan.");
            }

            var procedureIds = butir.Select(x => x.ProcedureId).ToList();
            var tindakan = await _dbContext.MstProcedures.AsNoTracking()
                .Where(p => procedureIds.Contains(p.Id) && !p.IsDelete)
                .Select(p => new { p.Id, p.ProcedureName, p.ProcedureCategoryName, p.IsRadiology, p.IsActive })
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Alat tiap baris: dipilih, atau diturunkan dari pemeriksaannya seperti v1 (v1 tidak
            // memilih alat). Baris yang alatnya tidak dapat ditentukan ditolak dengan namanya.
            var alatPerBaris = new List<Guid>(butir.Count);
            foreach (var item in butir)
            {
                if (!tindakan.TryGetValue(item.ProcedureId, out var t) || !t.IsRadiology || !t.IsActive)
                {
                    return RadOperationResult<RadBookingResponse>.Validation(
                        RadErrorCodes.ProcedureNotFound, "Ada pemeriksaan radiologi yang tidak ditemukan atau nonaktif.");
                }

                Guid? alatId = item.ModalityId;
                if (alatId.HasValue)
                {
                    var aktif = await _dbContext.MstRadModalities
                        .AnyAsync(m => m.Id == alatId.Value && !m.IsDelete && m.IsActive, cancellationToken);
                    if (!aktif) alatId = null;
                }

                alatId ??= (await TurunkanAlatAsync(t.ProcedureName, t.ProcedureCategoryName, cancellationToken))?.Id;
                if (!alatId.HasValue)
                {
                    return RadOperationResult<RadBookingResponse>.Validation(
                        RadErrorCodes.ModalityNotFound, $"Alat pencitraan untuk {t.ProcedureName} tidak dapat ditentukan.");
                }

                alatPerBaris.Add(alatId.Value);
            }

            // Tarif sah wajib ada untuk KELAS KUNJUNGAN — kelas yang dipakai Billing saat menagih. Pemeriksaan tanpa
            // tarif sah, atau dengan tarif ambigu, tidak dibooking. Harga tidak pernah datang dari permintaan ini:
            // Billing menetapkannya dari master tarif ketika study lolos mutu.
            var konteksTarif = await LoadTariffContextAsync(kunjungan.Id, cancellationToken);
            var pilihanTarif = await SelectTariffsAsync(
                procedureIds, konteksTarif.EncounterClassId, konteksTarif.ClinicId, DateTime.UtcNow, cancellationToken);
            foreach (var item in butir)
            {
                var pilihan = pilihanTarif[item.ProcedureId];
                if (pilihan.Status == MstTariffSelectionStatus.Resolved) continue;

                var alasan = DescribeTariffProblem(StatusFrom(pilihan.Status), konteksTarif.EncounterClassName, pilihan.Tied);
                return RadOperationResult<RadBookingResponse>.Validation(
                    pilihan.Status == MstTariffSelectionStatus.Ambiguous
                        ? RadErrorCodes.TariffAmbiguous
                        : RadErrorCodes.TariffNotAvailable,
                    $"{tindakan[item.ProcedureId].ProcedureName}: {alasan}");
            }

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var waktuOrder = request.OrderedAt?.ToUniversalTime() ?? now;
            var waktuPeriksa = request.ExaminationAt?.ToUniversalTime();
            // Tanggal pemeriksaan di hari lain menjadi jadwal (Pasien Perjanjian v1).
            DateTime? jadwal = waktuPeriksa.HasValue && TanggalWib(waktuPeriksa.Value) != TanggalWib(waktuOrder)
                ? waktuPeriksa
                : null;
            var indikasi = string.IsNullOrWhiteSpace(request.ClinicalIndication) ? null : request.ClinicalIndication.Trim();

            await using var transaksi = _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var dibuat = new List<RadOrder>(butir.Count);
            for (var i = 0; i < butir.Count; i++)
            {
                var item = butir[i];
                var entity = new RadOrder
                {
                    OrderNumber = await _orderNumberService.AllocateOneAsync(cancellationToken),
                    EncounterId = kunjungan.Id,
                    ProcedureId = item.ProcedureId,
                    ModalityId = alatPerBaris[i],
                    ClinicalIndication = indikasi,
                    OrderStatus = RadOrderStatus.Requested,
                    InstructionVerificationStatus = RadOrderInstructionVerificationStatus.NotRequired,
                    RequestedAt = waktuOrder,
                    ScheduledAt = jadwal,
                    RequestedByUserId = actorUserId,
                    IsUrgent = item.IsUrgent,
                    UrgentMarkedByUserId = item.IsUrgent ? actorUserId : null,
                    UrgentMarkedAt = item.IsUrgent ? now : null,
                    RequiresPreparation = request.RequiresPreparation == true,
                    ExaminingDoctorId = item.ExaminingDoctorId,
                    CreateBy = actorUserId,
                    CreateDateTime = now,
                };

                _dbContext.RadOrders.Add(entity);
                AddHistory(entity, "Order.Create", null, entity.OrderStatus.ToString(),
                    null, "Booking Radiologi", actorUserId, now);

                // Setiap nomor dialokasikan dari MAX + 1 yang terbaca SQL; pesanan yang belum
                // tersimpan tidak terlihat olehnya, sehingga disimpan satu per satu di dalam
                // transaksi yang sama supaya nomor berikutnya tidak kembar.
                await _dbContext.SaveChangesAsync(cancellationToken);
                dibuat.Add(entity);
            }

            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

            return RadOperationResult<RadBookingResponse>.Success(new RadBookingResponse
            {
                OrderIds = dibuat.Select(x => x.Id).ToList(),
                OrderNumbers = dibuat.Select(x => x.OrderNumber).ToList(),
            });
        }

        /// <summary>
        /// Alat pencitraan yang diturunkan dari pemeriksaan — Booking v1 hanya memilih pemeriksaan.
        /// Nama pemeriksaan didahulukan (kategori impor v1 terlalu kasar: mamografi, densitometri,
        /// dan barium tercatat "X-Ray"), lalu kategori. Hanya alat aktif yang dipakai.
        /// </summary>
        private async Task<MstRadModalityRingkas?> TurunkanAlatAsync(
            string? namaPemeriksaan,
            string? kategori,
            CancellationToken cancellationToken)
        {
            var kode = KodeAlatDariPemeriksaan(namaPemeriksaan, kategori);
            if (kode.Count == 0) return null;

            var alat = await _dbContext.MstRadModalities.AsNoTracking()
                .Where(m => !m.IsDelete && m.IsActive && kode.Contains(m.ModalityCode))
                .Select(m => new MstRadModalityRingkas(m.Id, m.ModalityCode, m.ModalityName))
                .ToListAsync(cancellationToken);

            return kode.Select(k => alat.FirstOrDefault(a => string.Equals(a.ModalityCode, k, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(a => a != null);
        }

        internal sealed record MstRadModalityRingkas(Guid Id, string ModalityCode, string ModalityName);

        /// <summary>Kode alat kandidat, urut prioritas, dari nama lalu kategori pemeriksaan.</summary>
        public static List<string> KodeAlatDariPemeriksaan(string? namaPemeriksaan, string? kategori)
        {
            var nama = (namaPemeriksaan ?? string.Empty).ToUpperInvariant();
            var kat = (kategori ?? string.Empty).ToUpperInvariant();
            bool Ada(string teks, params string[] kata) => kata.Any(teks.Contains);

            if (Ada(nama, "MAMMO", "MAMOGRA")) return ["MG"];
            if (Ada(nama, "DENSITO", "BMD", "DEXA")) return ["BMD", "DXA", "DX", "CR"];
            if (Ada(nama, "ANGIO", "DSA ", "EMBOLI")) return ["XA"];
            if (Ada(nama, "BARIUM", "FLUORO", "HSG", "APPENDICOGRAM", "COLON IN LOOP", "OMD", "ESOFAGOGRAM", "OESOPHAGOGRAM", "URETHROGRAM", "URETROGRAM"))
                return ["RF", "CR", "DX"];
            if (Ada(nama, "MRI", "MRCP", "MRA ")) return ["MR"];
            if (Ada(nama, "MSCT", "CT SCAN", "CT-SCAN")) return ["CT"];
            if (Ada(nama, "USG", "ULTRASONO", "DOPPLER")) return ["US"];
            if (Ada(nama, "PET", "SPECT", "BONE SCAN", "SCINTI", "SKINTI")) return ["NM", "PT"];

            if (Ada(kat, "MRI")) return ["MR"];
            if (Ada(kat, "CT")) return ["CT"];
            if (Ada(kat, "USG", "ULTRASONO")) return ["US"];
            if (Ada(kat, "X-RAY", "XRAY", "KONVENSIONAL", "RONTGEN")) return ["CR", "DX"];

            return [];
        }
    }
}
