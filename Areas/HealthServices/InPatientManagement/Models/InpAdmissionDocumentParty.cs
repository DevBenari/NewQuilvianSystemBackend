using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Penanda tangan atau deklarer yang dinyatakan dokumen — kamus data 20.4 (<c>BE-RWI-192</c>).
    /// Bukan salinan master relasi: ia isi pernyataan. <see cref="SourceRecordId"/> hanya jejak asal
    /// tanpa FK.
    /// </summary>
    /// <remarks>
    /// Pemakaian per jenis: Privasi = nama penanda tangan; Nilai Kepercayaan = nama, tanggal lahir,
    /// jenis kelamin, hubungan, alamat; Selisih Biaya = nama, alamat, pekerjaan, jenis dan nomor
    /// identitas, telepon; Pelunasan Deposit = nama, alamat, telepon, sumber data. Seluruh isian
    /// pribadi SENSITIF dan tidak pernah masuk logger.
    /// </remarks>
    public class InpAdmissionDocumentParty : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public InpAdmissionPartySource SourceType { get; set; } = InpAdmissionPartySource.Manual;

        /// <summary>Id relasi atau kontak darurat asal. Jejak saja, tanpa FK.</summary>
        public Guid? SourceRecordId { get; set; }

        /// <summary>Nama penanda tangan atau deklarer. SENSITIF.</summary>
        public string FullName { get; set; } = string.Empty;

        public InpAdmissionPartyRelationship? Relationship { get; set; }

        /// <summary>Teks hubungan bila <c>Other</c> atau berasal dari kontak darurat.</summary>
        public string? RelationshipText { get; set; }

        /// <summary>SENSITIF.</summary>
        public string? Address { get; set; }

        /// <summary>Nilai Kepercayaan; umur dihitung, tidak disimpan. SENSITIF.</summary>
        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        /// <summary>Selisih Biaya. SENSITIF.</summary>
        public string? Occupation { get; set; }

        public InpAdmissionPartyIdentityType? IdentityType { get; set; }

        /// <summary>Selisih Biaya; disamarkan di daftar. SENSITIF.</summary>
        public string? IdentityNumber { get; set; }

        /// <summary>Angka saja, maksimal 13 digit (<c>FR-RWA-082</c>). SENSITIF.</summary>
        public string? MobilePhone { get; set; }

        /// <summary>Selisih Biaya. SENSITIF.</summary>
        public string? OfficePhone { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
