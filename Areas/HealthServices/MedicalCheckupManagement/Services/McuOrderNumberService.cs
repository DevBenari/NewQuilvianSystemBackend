using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Nomor order MCU (<c>MCU-RSMMC-000001</c>) dan nomor impor booking perusahaan
    /// (<c>MCU-IMP-000001</c>). Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bentuknya mengikuti nomor pesanan Lab dan Radiologi</b> (<c>LAB-RSMMC-</c>,
    /// <c>RAD-RSMMC-</c>): tiga deret itu disebut petugas yang sama di meja admisi, dan bentuk yang
    /// seragam membuat asal nomor terbaca seketika (<c>02-backend-architecture.md</c> bagian 9).
    /// </para>
    /// <para>
    /// <b>Berbeda dari Lab/Rad, nomor tidak dihitung dari data.</b> Lab dan Rad masih membaca
    /// <c>MAX + 1</c> di bawah kunci; MCU kode baru, sehingga wajib memakai
    /// <see cref="NumberSeriesAllocator"/> (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>). Akibatnya
    /// nomor yang terbit untuk booking yang lalu gagal disimpan <b>hangus</b> dan tidak dipakai
    /// ulang — lubang deret dibiarkan (<c>INV-PLT-002</c>). Index unik
    /// <c>IX_McuOrder_OrderNumber</c> dan <c>IX_McuBookingImport_ImportNumber</c> tetap menjadi
    /// penjaga terakhir (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// <b>Enam digit adalah lebar minimum, bukan batas.</b> Sama dengan Lab/Rad, order ke-1.000.000
    /// menjadi <c>MCU-RSMMC-1000000</c>, bukan galat; kolomnya 30 karakter.
    /// </para>
    /// <para>
    /// Penolakan alokator (<see cref="NumberSeriesAllocationException"/>) diteruskan apa adanya.
    /// Service booking dan impor yang menerjemahkannya menjadi pesan petugas.
    /// </para>
    /// </remarks>
    public class McuOrderNumberService
    {
        public const string OrderSequenceKey = "MCU-ORDER";
        public const string OrderNumberPrefix = "MCU-RSMMC";

        public const string ImportSequenceKey = "MCU-IMPORT";
        public const string ImportNumberPrefix = "MCU-IMP";

        public const int SequenceDigits = 6;

        private readonly NumberSeriesAllocator _allocator;

        public McuOrderNumberService(NumberSeriesAllocator allocator)
        {
            _allocator = allocator;
        }

        /// <summary>Menerbitkan satu nomor order MCU, contoh <c>MCU-RSMMC-000042</c>.</summary>
        public Task<string> AllocateOrderNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(OrderSequenceKey, OrderNumberPrefix, actorUserId, cancellationToken);

        /// <summary>Menerbitkan satu nomor impor booking perusahaan, contoh <c>MCU-IMP-000007</c>.</summary>
        public Task<string> AllocateImportNumberAsync(Guid actorUserId, CancellationToken cancellationToken = default) =>
            AllocateAsync(ImportSequenceKey, ImportNumberPrefix, actorUserId, cancellationToken);

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
