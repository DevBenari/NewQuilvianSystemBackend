using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    /// <summary>
    /// Konteks identitas dan batasan akses data pengguna yang memanggil layanan Medical Fee.
    /// Mengimplementasikan prinsip Pembatasan Data Lapis Service (MDF-PERM-1.0 §5, MF-DEC-010, BE-MDF-017).
    /// </summary>
    public class MedicalFeeActorContext
    {
        /// <summary>User ID pengguna yang sedang masuk.</summary>
        public Guid UserId { get; set; }

        /// <summary>Nama pengguna / display name.</summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Apakah pengguna memiliki hak akses penuh tanpa saringan data (Staf, Supervisor, Manajer, Admin Master, SuperAdmin).
        /// Jika true, pengguna berhak melihat seluruh penerima jasa (T-84).
        /// </summary>
        public bool IsUnrestricted { get; set; }

        /// <summary>
        /// Apakah pengguna adalah tenaga medis (Dokter / Staf Pelaksana) yang dibatasi hanya boleh melihat datanya sendiri.
        /// </summary>
        public bool IsRestrictedToSelf => !IsUnrestricted && RestrictedPayeeReferenceId.HasValue;

        /// <summary>
        /// ID penerima (MstDoctor.Id atau MstWorkforceProfile.Id) milik pengguna jika pengguna adalah tenaga medis.
        /// </summary>
        public Guid? RestrictedPayeeReferenceId { get; set; }

        /// <summary>
        /// Jenis penerima tenaga medis: 'Doctor' atau 'Workforce' (MdfPayeeTypes).
        /// </summary>
        public string? RestrictedPayeeType { get; set; }

        public static MedicalFeeActorContext CreateUnrestricted(Guid userId, string? userName = null) =>
            new MedicalFeeActorContext
            {
                UserId = userId,
                UserName = userName,
                IsUnrestricted = true
            };

        public static MedicalFeeActorContext CreateRestricted(Guid userId, Guid payeeReferenceId, string payeeType, string? userName = null) =>
            new MedicalFeeActorContext
            {
                UserId = userId,
                UserName = userName,
                IsUnrestricted = false,
                RestrictedPayeeReferenceId = payeeReferenceId,
                RestrictedPayeeType = payeeType
            };
    }
}

