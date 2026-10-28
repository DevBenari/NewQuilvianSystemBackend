using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy
{
    /// <summary>
    /// Pengujian unit untuk kebijakan pembatasan data tenaga medis di lapis service (BE-MDF-017).
    /// Menguji skenario kepatuhan T-80, T-81, T-82, dan T-84 sesuai acceptance-test-matrix.md dan MDF-PERM-1.0 §5.
    /// </summary>
    public class MedicalFeeDataRestrictionPolicyTests
    {
        private readonly Guid _doctorAId = Guid.NewGuid();
        private readonly Guid _doctorBId = Guid.NewGuid();
        private readonly Guid _userAId = Guid.NewGuid();
        private readonly Guid _staffUserId = Guid.NewGuid();

        [Fact]
        public void T80_DoctorA_RequestingDoctorBFees_ReturnsEmptyResult()
        {
            // Arrange: Dokter A meminta hasil jasa Dokter B secara eksplisit
            var actor = MedicalFeeActorContext.CreateRestricted(_userAId, _doctorAId, "Doctor", "dr. Ahmad");
            Guid? requestedPayeeId = _doctorBId;

            // Act
            var applied = MedicalFeeDataRestrictionPolicy.ApplyListFilter(actor, ref requestedPayeeId, out var shouldReturnEmpty);

            // Assert: Sistem menolak menghasilkan baris dan mengembalikan kosong
            Assert.True(applied);
            Assert.True(shouldReturnEmpty);
        }

        [Fact]
        public void T81_DoctorA_RequestingWithoutFilter_ForcesFilterToDoctorAOnly()
        {
            // Arrange: Dokter A meminta daftar tanpa filter (GET /service-fees)
            var actor = MedicalFeeActorContext.CreateRestricted(_userAId, _doctorAId, "Doctor", "dr. Ahmad");
            Guid? requestedPayeeId = null;

            // Act
            var applied = MedicalFeeDataRestrictionPolicy.ApplyListFilter(actor, ref requestedPayeeId, out var shouldReturnEmpty);

            // Assert: Query otomatis dipaksa menyaring hanya baris milik Dokter A
            Assert.True(applied);
            Assert.False(shouldReturnEmpty);
            Assert.Equal(_doctorAId, requestedPayeeId);
        }

        [Fact]
        public void T82_DoctorA_AccessingDoctorBFeeDirectly_ThrowsKeyNotFoundException_Yielding404()
        {
            // Arrange: Dokter A membuka rincian hasil jasa milik Dokter B lewat ID langsung
            var actor = MedicalFeeActorContext.CreateRestricted(_userAId, _doctorAId, "Doctor", "dr. Ahmad");
            var feeId = Guid.NewGuid();

            // Act & Assert: Wajib melempar KeyNotFoundException (menghasilkan 404, bukan 403 yang membocorkan keberadaan data)
            var ex = Assert.Throws<KeyNotFoundException>(() =>
                MedicalFeeDataRestrictionPolicy.AssertCanAccessServiceFee(actor, _doctorBId, feeId));

            Assert.Contains(feeId.ToString(), ex.Message);
        }

        [Fact]
        public void T82_DoctorA_AccessingOwnFeeDirectly_PassesValidation()
        {
            // Arrange: Dokter A membuka rincian hasil jasanya sendiri
            var actor = MedicalFeeActorContext.CreateRestricted(_userAId, _doctorAId, "Doctor", "dr. Ahmad");
            var feeId = Guid.NewGuid();

            // Act & Assert: Tidak ada exception dilempar
            MedicalFeeDataRestrictionPolicy.AssertCanAccessServiceFee(actor, _doctorAId, feeId);
        }

        [Fact]
        public void T84_StaffMedicalFee_OpeningAllPayees_HasUnrestrictedAccess()
        {
            // Arrange: Staf / Admin Medical Fee meminta daftar tanpa filter
            var actor = MedicalFeeActorContext.CreateUnrestricted(_staffUserId, "Staf Admin MDF");
            Guid? requestedPayeeId = null;

            // Act
            var applied = MedicalFeeDataRestrictionPolicy.ApplyListFilter(actor, ref requestedPayeeId, out var shouldReturnEmpty);

            // Assert: Tidak ada pembatasan data paksa; seluruh 218 dokter terlihat
            Assert.False(applied);
            Assert.False(shouldReturnEmpty);
            Assert.Null(requestedPayeeId);
        }

        [Fact]
        public void T84_StaffMedicalFee_AccessingAnyPayeeFee_PassesValidation()
        {
            // Arrange: Staf / Admin Medical Fee membuka hasil jasa milik Dokter B
            var actor = MedicalFeeActorContext.CreateUnrestricted(_staffUserId, "Staf Admin MDF");
            var feeId = Guid.NewGuid();

            // Act & Assert: Lolos validasi tanpa exception
            MedicalFeeDataRestrictionPolicy.AssertCanAccessServiceFee(actor, _doctorBId, feeId);
        }

        [Fact]
        public void SharingAgreement_DoctorA_AccessingDoctorBAgreement_ThrowsKeyNotFoundException()
        {
            // Arrange: Dokter A membuka kesepakatan tarif milik Dokter B
            var actor = MedicalFeeActorContext.CreateRestricted(_userAId, _doctorAId, "Doctor", "dr. Ahmad");
            var agreementId = Guid.NewGuid();

            // Act & Assert: Menghasilkan KeyNotFoundException (404)
            var ex = Assert.Throws<KeyNotFoundException>(() =>
                MedicalFeeDataRestrictionPolicy.AssertCanAccessSharingAgreement(actor, _doctorBId, agreementId));

            Assert.Contains(agreementId.ToString(), ex.Message);
        }
    }
}

