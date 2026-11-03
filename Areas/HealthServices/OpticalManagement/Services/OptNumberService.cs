using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Nomor dokumen Optik (<c>BE-OPT-004</c>, <c>02-backend-architecture.md</c> bagian 4.2):
    /// pemeriksaan <c>OPT-EXM-RSMMC-000001</c>, resep <c>OPT-RX-RSMMC-000001</c>, pesanan
    /// <c>OPT-ORD-RSMMC-000001</c>, dan keluhan garansi <c>OPT-CLM-RSMMC-000001</c>.
    /// Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nomor tidak dihitung dari data.</b> Seluruh deret lewat <see cref="NumberSeriesAllocator"/>
    /// (<c>QBE-CODE-001</c>, <c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>), sama dengan MCU. Nomor yang terbit untuk
    /// dokumen yang lalu gagal disimpan hangus dan tidak dipakai ulang; lubang deret dibiarkan
    /// (<c>INV-PLT-002</c>). Index unik nomor pada keempat tabel tetap menjadi penjaga terakhir
    /// (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// <b>Enam digit adalah lebar minimum, bukan batas</b> — dokumen ke-1.000.000 menjadi
    /// <c>OPT-ORD-RSMMC-1000000</c>; kolomnya 30 karakter. Seluruh deret <c>NEVER</c> reset.
    /// </para>
    /// <para>
    /// Penolakan alokator (<see cref="NumberSeriesAllocationException"/>) diteruskan apa adanya; service
    /// pemanggil yang menerjemahkannya menjadi pesan petugas.
    /// </para>
    /// </remarks>
    public class OptNumberService
    {
        public const string ExamSequenceKey = "OPT-EXM";
        public const string ExamNumberPrefix = "OPT-EXM-RSMMC";

        public const string PrescriptionSequenceKey = "OPT-RX";
        public const string PrescriptionNumberPrefix = "OPT-RX-RSMMC";

        public const string OrderSequenceKey = "OPT-ORD";
        public const string OrderNumberPrefix = "OPT-ORD-RSMMC";

        public const string ClaimSequenceKey = "OPT-CLM";
        public const string ClaimNumberPrefix = "OPT-CLM-RSMMC";

        public const int SequenceDigits = 6;

        private readonly NumberSeriesAllocator _allocator;

        public OptNumberService(NumberSeriesAllocator allocator)
        {
            _allocator = allocator;
        }

        /// <summary>Nomor pemeriksaan refraksi, contoh <c>OPT-EXM-RSMMC-000042</c>.</summary>
        public Task<string> AllocateExamNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(ExamSequenceKey, ExamNumberPrefix, actorUserId, cancellationToken);

        /// <summary>Nomor resep, contoh <c>OPT-RX-RSMMC-000042</c>.</summary>
        public Task<string> AllocatePrescriptionNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(PrescriptionSequenceKey, PrescriptionNumberPrefix, actorUserId, cancellationToken);

        /// <summary>Nomor pesanan, contoh <c>OPT-ORD-RSMMC-000042</c>.</summary>
        public Task<string> AllocateOrderNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(OrderSequenceKey, OrderNumberPrefix, actorUserId, cancellationToken);

        /// <summary>Nomor keluhan garansi, contoh <c>OPT-CLM-RSMMC-000042</c>.</summary>
        public Task<string> AllocateClaimNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(ClaimSequenceKey, ClaimNumberPrefix, actorUserId, cancellationToken);

        private Task<string> AllocateAsync(
            string sequenceKey,
            string prefix,
            Guid actorUserId,
            CancellationToken cancellationToken) =>
            _allocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: sequenceKey,
                    Prefix: prefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    AllowSequenceOverflow: true),
                cancellationToken);
    }
}
