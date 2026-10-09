using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Service baca identitas pasien, relasi, dan kontak darurat milik <c>PatientManagement</c> —
    /// <c>BE-RWI-187</c>, kontrak <c>episode-rawat-inap</c> <c>0.11.0</c> backend 13.6 dan 13.8.4,
    /// integrasi <c>INT-RWA-01</c>, <c>02</c> (<c>RWI-DEC-264</c>, disetujui <c>RWI-DEC-266</c>).
    /// </summary>
    /// <remarks>
    /// Service ini <b>hanya membaca</b>. Ia tidak mengubah tabel maupun endpoint
    /// <c>PatientManagement</c> dan tidak membuka endpoint baru. Modul lain — Workspace PPRI
    /// Rawat Inap — memanggilnya dalam proses yang sama supaya tidak menulis query sendiri ke
    /// tabel master pasien (<c>INV-RWA-14</c>), dan supaya petugasnya tidak perlu diberi hak baca
    /// master pasien (<c>RWI-DEC-257</c>).
    ///
    /// <para>
    /// Contoh: Tn. Budi punya relasi <c>Spouse</c> bernama Rina Santoso dan kontak darurat "Dimas"
    /// berhubungan "adik". <see cref="GetPartyCandidatesAsync"/> mengembalikan keduanya: Rina dengan
    /// jenis terstruktur <c>Spouse</c>, Dimas dengan teks "adik" apa adanya — tanpa dicocokkan.
    /// </para>
    /// </remarks>
    public sealed class PatientProfileQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public PatientProfileQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Identitas pasien untuk header, gelang, label, IPD, dan salinan beku dokumen. Pasien yang
        /// tidak ada atau sudah terhapus dikembalikan kosong.
        /// </summary>
        public async Task<PatientIdentityProfile?> GetIdentityAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
            {
                return null;
            }

            var row = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.MedicalRecordNumber,
                    x.FullName,
                    x.NickName,
                    x.BirthDate,
                    x.Gender,
                    x.Religion,
                    x.MaritalStatus,
                    x.IdentityType,
                    x.IdentityNumber,
                    x.PhoneNumber,
                    x.Email,
                    x.Address,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    CityName = x.City != null ? x.City.CityName : null,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    x.IsNewborn,
                    x.MotherPatientId,
                    MotherName = x.MotherPatient != null ? x.MotherPatient.FullName : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
            {
                return null;
            }

            var hasMedicalRecordNumber = !string.IsNullOrWhiteSpace(row.MedicalRecordNumber);
            var rawNumber = PatientQrPayloadBuilder.NormalizeToRawDigits(row.MedicalRecordNumber);

            return new PatientIdentityProfile
            {
                PatientId = row.Id,
                MedicalRecordNumber = rawNumber != null
                    ? PatientQrPayloadBuilder.FormatMedicalRecordNumber(rawNumber)
                    : row.MedicalRecordNumber.Trim(),
                FullName = row.FullName,
                NickName = Clean(row.NickName),
                BirthDate = row.BirthDate,
                Gender = row.Gender,
                Religion = row.Religion,
                MaritalStatus = row.MaritalStatus,
                IdentityType = Clean(row.IdentityType),
                IdentityNumber = Clean(row.IdentityNumber),
                PhoneNumber = Clean(row.PhoneNumber),
                Email = Clean(row.Email),
                Address = Clean(row.Address),
                DistrictName = Clean(row.DistrictName),
                CityName = Clean(row.CityName),
                ProvinceName = Clean(row.ProvinceName),
                PostalCode = Clean(row.PostalCode),
                IsNewborn = row.IsNewborn,
                MotherPatientId = row.MotherPatientId,
                MotherName = Clean(row.MotherName),
                // Pasien tanpa No. RM tidak punya isi QR; tidak ada nomor pengganti yang dikarang.
                QrPayload = hasMedicalRecordNumber
                    ? PatientQrPayloadBuilder.Build(row.MedicalRecordNumber)
                    : null
            };
        }

        /// <summary>
        /// Calon penanda tangan dan penanggung jawab: relasi aktif pasien dengan jenis terstruktur,
        /// lalu kontak darurat aktif dengan teks hubungan apa adanya (<c>RWI-DEC-252</c>).
        /// </summary>
        /// <remarks>
        /// Kontak darurat tidak pernah dicocokkan lewat teks hubungannya; ia hanya ditawarkan sebagai
        /// pilihan. Kontak yang alamatnya ditandai sama dengan pasien dan dibiarkan kosong memakai
        /// alamat pasien.
        /// </remarks>
        public async Task<IReadOnlyList<PatientPartyCandidate>> GetPartyCandidatesAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
            {
                return Array.Empty<PatientPartyCandidate>();
            }

            var patientAddress = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId && !x.IsDelete)
                .Select(x => x.Address)
                .FirstOrDefaultAsync(cancellationToken);

            var relationships = await _dbContext.Set<MstPatientRelationship>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.RelationshipType,
                    Name = x.RelatedPersonName != null && x.RelatedPersonName != string.Empty
                        ? x.RelatedPersonName
                        : (x.RelatedPatient != null ? x.RelatedPatient.FullName : null),
                    Address = x.RelatedPersonAddress != null && x.RelatedPersonAddress != string.Empty
                        ? x.RelatedPersonAddress
                        : (x.RelatedPatient != null ? x.RelatedPatient.Address : null),
                    PhoneNumber = x.RelatedPersonPhoneNumber != null && x.RelatedPersonPhoneNumber != string.Empty
                        ? x.RelatedPersonPhoneNumber
                        : (x.RelatedPatient != null ? x.RelatedPatient.PhoneNumber : null),
                    x.RelatedPersonIdentityType,
                    x.RelatedPersonIdentityNumber,
                    x.IsPrimary,
                    x.IsResponsiblePerson,
                    x.IsLegalGuardian,
                    x.IsEmergencyContact
                })
                .ToListAsync(cancellationToken);

            var contacts = await _dbContext.Set<MstPatientEmergencyContact>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ContactName,
                    x.Relationship,
                    x.Address,
                    x.IsSameAddressAsPatient,
                    x.PhoneNumber,
                    x.IdentityType,
                    x.IdentityNumber,
                    x.IsPrimary,
                    x.IsResponsiblePerson
                })
                .ToListAsync(cancellationToken);

            var candidates = relationships
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Name)
                .Select(x => new PatientPartyCandidate
                {
                    Source = PatientPartyCandidateSource.PatientRelationship,
                    SourceRecordId = x.Id,
                    Name = x.Name!.Trim(),
                    RelationshipType = x.RelationshipType,
                    RelationshipText = null,
                    Address = Clean(x.Address),
                    PhoneNumber = Clean(x.PhoneNumber),
                    IdentityType = Clean(x.RelatedPersonIdentityType),
                    IdentityNumber = Clean(x.RelatedPersonIdentityNumber),
                    IsPrimary = x.IsPrimary,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsLegalGuardian = x.IsLegalGuardian,
                    IsEmergencyContact = x.IsEmergencyContact
                })
                .ToList();

            candidates.AddRange(contacts
                .Where(x => !string.IsNullOrWhiteSpace(x.ContactName))
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.ContactName)
                .Select(x => new PatientPartyCandidate
                {
                    Source = PatientPartyCandidateSource.EmergencyContact,
                    SourceRecordId = x.Id,
                    Name = x.ContactName.Trim(),
                    RelationshipType = null,
                    RelationshipText = Clean(x.Relationship),
                    Address = Clean(x.Address) ?? (x.IsSameAddressAsPatient ? Clean(patientAddress) : null),
                    PhoneNumber = Clean(x.PhoneNumber),
                    IdentityType = Clean(x.IdentityType),
                    IdentityNumber = Clean(x.IdentityNumber),
                    IsPrimary = x.IsPrimary,
                    IsResponsiblePerson = x.IsResponsiblePerson,
                    IsLegalGuardian = false,
                    IsEmergencyContact = true
                }));

            return candidates;
        }

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Identitas pasien yang dibaca modul lain (<c>BE-RWI-187</c>). Bukan kontrak API.</summary>
    public sealed class PatientIdentityProfile
    {
        public Guid PatientId { get; init; }

        /// <summary>No. RM terformat, misalnya <c>00-12-34-56</c>.</summary>
        public string MedicalRecordNumber { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string? NickName { get; init; }

        public DateTime? BirthDate { get; init; }

        public Gender? Gender { get; init; }

        public Religion Religion { get; init; }

        public MaritalStatus MaritalStatus { get; init; }

        /// <summary>Jenis tanda pengenal sebagaimana tersimpan, misalnya "KTP".</summary>
        public string? IdentityType { get; init; }

        public string? IdentityNumber { get; init; }

        public string? PhoneNumber { get; init; }

        public string? Email { get; init; }

        public string? Address { get; init; }

        public string? DistrictName { get; init; }

        public string? CityName { get; init; }

        public string? ProvinceName { get; init; }

        public string? PostalCode { get; init; }

        public bool IsNewborn { get; init; }

        public Guid? MotherPatientId { get; init; }

        public string? MotherName { get; init; }

        /// <summary>Isi QR = No. RM terformat (<c>RWI-DEC-259</c>); kosong bila pasien tanpa No. RM.</summary>
        public string? QrPayload { get; init; }
    }

    /// <summary>Asal calon penanda tangan (<c>BE-RWI-187</c>).</summary>
    public enum PatientPartyCandidateSource
    {
        PatientRelationship = 1,
        EmergencyContact = 2
    }

    /// <summary>Satu relasi atau kontak darurat pasien yang dapat dipilih sebagai penanda tangan.</summary>
    public sealed class PatientPartyCandidate
    {
        public PatientPartyCandidateSource Source { get; init; }

        public Guid SourceRecordId { get; init; }

        public string Name { get; init; } = string.Empty;

        /// <summary>Jenis hubungan terstruktur; hanya untuk relasi.</summary>
        public PatientRelationshipType? RelationshipType { get; init; }

        /// <summary>Teks hubungan apa adanya; hanya untuk kontak darurat.</summary>
        public string? RelationshipText { get; init; }

        public string? Address { get; init; }

        public string? PhoneNumber { get; init; }

        public string? IdentityType { get; init; }

        public string? IdentityNumber { get; init; }

        public bool IsPrimary { get; init; }

        public bool IsResponsiblePerson { get; init; }

        public bool IsLegalGuardian { get; init; }

        public bool IsEmergencyContact { get; init; }
    }
}
