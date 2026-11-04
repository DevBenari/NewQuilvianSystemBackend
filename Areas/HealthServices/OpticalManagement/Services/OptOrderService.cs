using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Pesanan Optik tahap awal (<c>BE-OPT-032</c>, <c>OPT-API-v1</c> grup Opt Order, <c>OPT-STATE-v1</c> bagian 3,
    /// <c>OPT-VAL-v1</c> bagian 4): buat dan ubah <c>Draft</c>, simpan &amp; tagihkan, batal sebelum pengerjaan, status bayar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Cakupan.</b> Pesanan berpengerjaan dan penjualan langsung (<c>BE-OPT-042</c>, <c>OPT-DEC-020</c>, <c>046</c>). Pesanan
    /// remake dibuat dari keluhan garansi (<c>BE-OPT-050</c>), bukan dari layar ini. Penjualan langsung: tanpa uang muka dan
    /// tanpa pengerjaan; ditagihkan tetap <c>Draf</c>, lalu diserahkan sesudah lunas dengan potong stok saat serah. Lensa kontak
    /// wajib resep lensa kontak yang berlaku; barang lain bebas resep. Kirim ke pengerjaan dan tandai siap ditambahkan
    /// <c>BE-OPT-034</c>; serah dan batal sesudah pengerjaan milik <c>BE-OPT-040</c>/<c>041</c>.
    /// </para>
    /// <para>
    /// <b>Kirim ke pengerjaan</b> (<c>BE-OPT-034</c>, <c>OPT-DEC-007</c>, <c>013</c>, <c>019</c>). Tujuan dan perkiraan jadi
    /// dicek (<c>OPT-VAL-048</c>), lalu uang muka dan aturan sementara dari <see cref="OptOrderPaymentGate"/>
    /// (<c>OPT-VAL-046</c>, <c>047</c>, <i>fail-closed</i>). Potong stok frame/lensa stok, kunci resep, dan perubahan status
    /// berada dalam <b>satu transaksi</b>: stok kurang membatalkan semuanya (<c>OPT-VAL-049</c>). Sesudah commit, fakta
    /// tagihan barang direvisi menjadi <c>IN_WORK</c> agar Billing menahan finalisasi (<c>OPT-DEC-043</c>). Tombol yang
    /// ditekan dua kali dengan isian sama dijawab sebagai pengulangan tanpa memotong stok lagi (<c>OPT-AC-008</c>).
    /// </para>
    /// <para>
    /// <b>Tagihkan.</b> Tarif setiap baris diperiksa lebih dulu dengan resolver Billing, lalu fakta <c>OpticalItemCharge</c>
    /// berstatus <c>ORDERED</c> diterbitkan per baris di luar transaksi. Pesanan baru pindah ke <c>AwaitingDownPayment</c>
    /// sesudah seluruh baris terbukti masuk tagihan; bila Billing tidak menjawab, pesanan tetap <c>Draft</c> (<c>503</c>) dan
    /// percobaan berikutnya mengulang fakta yang sama (<c>Replayed</c>), sehingga tidak ada baris ganda (<c>OPT-AC-018</c>).
    /// Waktu fakta = waktu perubahan terakhir pesanan, jadi stabil selama isi pesanan tidak berubah.
    /// </para>
    /// <para>
    /// <b>Batal.</b> Status dan baris berubah dalam satu penyimpanan; pembatalan baris tagihan diterbitkan sesudahnya
    /// (<c>INT-OPT-05</c>). Billing melakukan void karena baris masih <c>ORDERED</c>.
    /// </para>
    /// <para>
    /// <b>Minimum uang muka</b> dibekukan dari pengaturan unit saat pesanan dibuat (<c>OPT-DEC-009</c>, <c>OPT-AC-006</c>).
    /// </para>
    /// </remarks>
    public class OptOrderService
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        public const string NotFoundMessage = "Pesanan tidak ditemukan.";
        public const string StaleMessage = "Pesanan sudah diubah petugas lain. Muat ulang halaman.";
        public const string NoItemMessage = "Tambahkan minimal satu barang.";
        public const string NotOpticalItemMessage = "Barang ini belum terdaftar sebagai barang Optik.";
        public const string PrescriptionRequiredMessage = "Pilih resep untuk pesanan kacamata.";
        public const string ContactLensPrescriptionMessage = "Lensa kontak wajib memakai resep lensa kontak yang masih berlaku.";
        public const string LensSupplyMessage = "Tentukan lensa stok atau custom.";
        public const string QuantityMessage = "Jumlah harus lebih dari 0.";
        public const string CancelReasonMessage = "Tuliskan alasan pembatalan.";
        public const string CancelAfterWorkMessage = "Pembatalan sesudah pengerjaan perlu persetujuan Kepala Optik.";
        public const string MixedPrescriptionMessage = "Lensa kontak dan lensa kacamata memakai resep berbeda. Buat pesanan terpisah.";
        public const string OutdatedVersionMessage = "Resep ini sudah direvisi. Pilih versi terbaru.";
        public const string DirectSaleSpectacleLensMessage = "Lensa kacamata dijual lewat pesanan berpengerjaan, bukan penjualan langsung.";
        public const string RemakeMessage = "Pesanan remake dibuat dari keluhan garansi yang disetujui, bukan dari layar pesanan.";
        public const string RemakeBillMessage = "Pesanan remake tidak ditagihkan.";
        public const string CancelBillingPendingMessage =
            "Pesanan dibatalkan, tetapi pembatalan baris tagihan belum diterima Billing. Minta kasir memeriksa tagihan kunjungan ini.";

        private static readonly OptOrderStatus[] StatusSesudahPengerjaan = [OptOrderStatus.InFabrication, OptOrderStatus.ReadyForPickup];

        private readonly ApplicationDbContext _dbContext;
        private readonly OptNumberService _numberService;
        private readonly OptVisitService _visitService;
        private readonly OptPrescriptionService _prescriptionService;
        private readonly OptBillingGateway _billing;
        private readonly OptOrderPaymentGate _paymentGate;
        private readonly OptStockGateway _stockGateway;
        private readonly TimeProvider _time;

        public OptOrderService(
            ApplicationDbContext dbContext,
            OptNumberService numberService,
            OptVisitService visitService,
            OptPrescriptionService prescriptionService,
            OptBillingGateway billing,
            OptOrderPaymentGate paymentGate,
            OptStockGateway stockGateway,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _numberService = numberService;
            _visitService = visitService;
            _prescriptionService = prescriptionService;
            _billing = billing;
            _paymentGate = paymentGate;
            _stockGateway = stockGateway;
            _time = timeProvider ?? TimeProvider.System;
        }

        // ---------------------------------------------------------------- baca

        public async Task<PagedResult<OptOrderListItemResponse>> GetListAsync(OptOrderListQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var orders = _dbContext.OptOrders.AsNoTracking().Where(x => !x.IsDelete);
            if (query.Status is { } status) orders = orders.Where(x => x.Status == status);
            if (query.EncounterId is { } encounterId) orders = orders.Where(x => x.EncounterId == encounterId);
            if (query.PatientId is { } patientId) orders = orders.Where(x => x.PatientId == patientId);

            if (query.Overdue == true)
            {
                var hariIni = DateOnly.FromDateTime(Today());
                orders = orders.Where(x => x.Status == OptOrderStatus.InFabrication && x.EstimatedReadyDate != null && x.EstimatedReadyDate < hariIni);
            }

            if (query.NeedsContact == true)
            {
                var batas = Now().AddDays(-await PickupReminderDaysAsync(cancellationToken));
                orders = orders.Where(x => x.Status == OptOrderStatus.ReadyForPickup && x.ReadyAt != null && x.ReadyAt <= batas);
            }

            if (query.HasPendingCancellation == true)
                orders = orders.Where(x => _dbContext.OptOrderCancellations.Any(c =>
                    c.OrderId == x.Id && !c.IsDelete && c.Status == OptCancellationStatus.Requested));

            if (query.PastNotCollectedLimit == true)
            {
                // Pesanan Siap Diambil sedikit; dinilai di memori dengan fungsi yang sama dengan Tutup Tidak Diambil,
                // supaya penyaring, tanda baris, dan tombol tutup tidak pernah berbeda pendapat.
                var batasTutup = await NotCollectedCloseDaysByUnitAsync(cancellationToken);
                var hariIni = DateOnly.FromDateTime(Today());
                var calon = await (
                    from o in orders.Where(x => x.Status == OptOrderStatus.ReadyForPickup)
                    join e in _dbContext.Set<RegPatientEncounter>().AsNoTracking() on o.EncounterId equals e.Id
                    select new { o.Id, SiapAt = o.ReadyAt ?? o.UpdateDateTime ?? o.CreateDateTime, e.ServiceUnitId })
                    .ToListAsync(cancellationToken);
                var lewat = calon
                    .Where(x => IsPastNotCollectedLimit(x.SiapAt, NotCollectedCloseDays(batasTutup, x.ServiceUnitId), hariIni))
                    .Select(x => x.Id).ToList();
                orders = orders.Where(x => lewat.Contains(x.Id));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var kata = query.Search.Trim().ToLower();
                orders = orders.Where(x => x.OrderNumber.ToLower().Contains(kata)
                    || _dbContext.Set<MstPatient>().Any(p => p.Id == x.PatientId
                        && (p.MedicalRecordNumber.ToLower().Contains(kata) || p.FullName.ToLower().Contains(kata))));
            }

            var total = await orders.CountAsync(cancellationToken);
            var page = await ProjectListAsync(
                orders.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.OrderNumber)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize),
                cancellationToken);

            return new PagedResult<OptOrderListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page,
            };
        }

        public async Task<OptResult<OptOrderResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.OptOrders.AsNoTracking()
                .Include(x => x.Items).Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            return order is null
                ? OptResult<OptOrderResponse>.NotFound(NotFoundMessage)
                : OptResult<OptOrderResponse>.Ok(await ProjectAsync(order, cancellationToken));
        }

        public async Task<OptResult<OptOrderPaymentStatusResponse>> GetPaymentStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.OptOrders.AsNoTracking()
                .Include(x => x.Items.Where(i => i.Status == OptOrderItemStatus.Active && !i.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            return order is null
                ? OptResult<OptOrderPaymentStatusResponse>.NotFound(NotFoundMessage)
                : await _paymentGate.GetStatusAsync(order, cancellationToken);
        }

        public const string PricePreviewBilledMessage = "Harga pesanan yang sudah ditagihkan dibaca dari tagihan.";

        /// <summary>
        /// Perkiraan harga pesanan <c>Draft</c> yang belum ditagihkan (<c>BE-OPT-054</c>, <c>OPT-DEC-075</c>, <c>076</c>). Waktu tarif
        /// sama dengan yang dipakai Tagihkan (waktu terakhir draf diubah), sehingga angkanya sama dengan baris tagihan bila draf
        /// ditagihkan tanpa diubah lagi. Hanya membaca: tidak menerbitkan fakta tagihan dan tidak mengubah <c>Version</c>.
        /// </summary>
        public async Task<OptResult<OptOrderPricePreviewResponse>> GetPricePreviewAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.OptOrders.AsNoTracking()
                .Include(x => x.Items.Where(i => i.Status == OptOrderItemStatus.Active && !i.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (order is null)
                return OptResult<OptOrderPricePreviewResponse>.NotFound(NotFoundMessage);
            if (order.IsRemake)
                return OptResult<OptOrderPricePreviewResponse>.Conflict(OptErrorCodes.Val057, RemakeBillMessage);
            if (order.Status != OptOrderStatus.Draft || order.BilledAt is not null)
                return OptResult<OptOrderPricePreviewResponse>.Conflict(OptErrorCodes.Val057, PricePreviewBilledMessage);

            var aktif = order.Items.OrderBy(x => x.LineNo).ToList();
            var perkiraan = await _billing.EstimateItemPricesAsync(order, aktif, order.UpdateDateTime ?? order.CreateDateTime, cancellationToken);
            if (!perkiraan.IsSuccess)
                return OptResult<OptOrderPricePreviewResponse>.Unavailable(perkiraan.Code ?? OptErrorCodes.BillingUnavailable, perkiraan.Message ?? OptBillingGateway.PriceEstimateUnavailableMessage);

            var drugIds = aktif.Select(x => x.DrugId).Distinct().ToList();
            var barang = await _dbContext.Set<MstDrug>().AsNoTracking()
                .Where(x => drugIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new { x.DrugCode, x.DrugName }, cancellationToken);
            var perBaris = perkiraan.Value!.ToDictionary(x => x.ItemId);
            var lines = aktif.Select(x =>
            {
                var p = perBaris[x.Id];
                return new OptOrderPricePreviewLine
                {
                    ItemId = x.Id,
                    LineNo = x.LineNo,
                    DrugCode = barang.GetValueOrDefault(x.DrugId)?.DrugCode ?? string.Empty,
                    DrugName = barang.GetValueOrDefault(x.DrugId)?.DrugName ?? string.Empty,
                    Quantity = x.Quantity,
                    UnitPrice = p.UnitPrice,
                    LineAmount = p.LineAmount,
                    TariffAvailable = p.UnitPrice is not null,
                    Message = p.Message,
                };
            }).ToList();
            var semuaAda = lines.All(x => x.TariffAvailable);
            return OptResult<OptOrderPricePreviewResponse>.Ok(new OptOrderPricePreviewResponse
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Lines = lines,
                AllTariffsAvailable = semuaAda,
                EstimatedTotal = semuaAda ? lines.Sum(x => x.LineAmount ?? 0m) : null,
            });
        }

        // ---------------------------------------------------------------- buat dan ubah

        public async Task<OptResult<OptOrderResponse>> CreateAsync(OptOrderCreateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var tolakJenis = RejectKind(request.OrderKind);
            if (tolakJenis is not null)
                return tolakJenis;
            if (request.EncounterId is not { } encounterId || encounterId == Guid.Empty)
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih kunjungan Optik.");

            var encounter = await _visitService.FindOpticalEncounterAsync(encounterId, cancellationToken);
            if (encounter is null)
                return OptResult<OptOrderResponse>.NotFound("Kunjungan Optik tidak ditemukan.");
            if (OptVisitService.IsClosed(encounter.EncounterStatus))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val030, "Kunjungan sudah ditutup. Daftarkan kunjungan baru.");

            var isi = await ValidateContentAsync(request, encounter.PatientId, cancellationToken);
            if (isi.Error is not null)
                return isi.Error;

            var setting = await _dbContext.OptSettings.AsNoTracking()
                .Where(x => x.ServiceUnitId == encounter.ServiceUnitId && !x.IsDelete).FirstOrDefaultAsync(cancellationToken);
            if (setting is null)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.SettingMissing, "Pengaturan Optik belum tersedia. Hubungi Admin Optik.");

            var now = Now();
            var order = new OptOrder
            {
                OrderNumber = await _numberService.AllocateOrderNumberAsync(actorUserId, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                OrderKind = request.OrderKind,
                PrescriptionId = isi.PrescriptionId,
                PrescriptionVersionId = isi.PrescriptionVersionId,
                Status = OptOrderStatus.Draft,
                // Penjualan langsung tidak mengenal uang muka (OPT-DEC-020); minimum 0%.
                DownPaymentMinPercentSnapshot = request.OrderKind == OptOrderKind.DirectSale ? 0m : setting.DownPaymentMinPercent,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };
            var lineNo = 0;
            foreach (var baris in isi.Items)
            {
                order.Items.Add(new OptOrderItem
                {
                    OrderId = order.Id, LineNo = ++lineNo, DrugId = baris.DrugId, ItemCategory = baris.Category,
                    LensSupply = baris.LensSupply, Quantity = baris.Quantity, CreateDateTime = now, CreateBy = actorUserId,
                });
            }
            order.StatusHistories.Add(History(order.Id, null, OptOrderStatus.Draft, actorUserId, now, null));
            _dbContext.OptOrders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return OptResult<OptOrderResponse>.Ok(await ProjectAsync(order, cancellationToken));
        }

        /// <summary>
        /// Ubah pesanan <c>Draft</c>. Baris diperbarui sesuai urutan: baris yang sudah ada diganti isinya, baris lebih
        /// ditandai batal, baris tambahan diberi nomor baris lanjutan (nomor lama tidak dipakai ulang).
        /// </summary>
        public async Task<OptResult<OptOrderResponse>> UpdateAsync(Guid id, OptOrderUpdateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);

            var tolak = RejectUnlessUnbilledDraft(order) ?? RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;
            if (request.OrderKind != order.OrderKind)
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, "Jenis pesanan tidak dapat diubah.");

            var isi = await ValidateContentAsync(request, order.PatientId, cancellationToken);
            if (isi.Error is not null)
                return isi.Error;

            var now = Now();
            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            var nomorBerikut = order.Items.Count == 0 ? 0 : order.Items.Max(x => x.LineNo);
            var dibuang = new List<OptOrderItem>();

            for (var i = 0; i < Math.Max(aktif.Count, isi.Items.Count); i++)
            {
                if (i < aktif.Count && i < isi.Items.Count)
                {
                    var baris = aktif[i];
                    baris.DrugId = isi.Items[i].DrugId;
                    baris.ItemCategory = isi.Items[i].Category;
                    baris.LensSupply = isi.Items[i].LensSupply;
                    baris.Quantity = isi.Items[i].Quantity;
                    baris.UpdateDateTime = now;
                    baris.UpdateBy = actorUserId;
                }
                else if (i < aktif.Count)
                {
                    aktif[i].Status = OptOrderItemStatus.Cancelled;
                    aktif[i].CancelledAt = now;
                    aktif[i].UpdateDateTime = now;
                    aktif[i].UpdateBy = actorUserId;
                    dibuang.Add(aktif[i]);
                }
                else
                {
                    var baru = new OptOrderItem
                    {
                        OrderId = order.Id, LineNo = ++nomorBerikut, DrugId = isi.Items[i].DrugId, ItemCategory = isi.Items[i].Category,
                        LensSupply = isi.Items[i].LensSupply, Quantity = isi.Items[i].Quantity, CreateDateTime = now, CreateBy = actorUserId,
                    };
                    // Ditambahkan lewat DbSet: Id Guid sudah terisi, sehingga lewat koleksi saja EF menganggapnya baris lama.
                    _dbContext.OptOrderItems.Add(baru);
                }
            }

            order.PrescriptionId = isi.PrescriptionId;
            order.PrescriptionVersionId = isi.PrescriptionVersionId;
            Touch(order, actorUserId, now);

            var simpan = await SaveAsync(order, cancellationToken);
            if (!simpan.IsSuccess || dibuang.Count == 0)
                return simpan;

            // Percobaan tagih sebelumnya yang gagal (503) mungkin sudah menerbitkan fakta untuk baris yang kini dibuang.
            var gagal = await _billing.EmitItemCancellationsAsync(order, dibuang, now, actorUserId, cancellationToken);
            if (gagal.Count > 0)
                simpan.Value!.BillingMessage = CancelBillingPendingMessage;
            return simpan;
        }

        // ---------------------------------------------------------------- tagihkan dan batal

        public async Task<OptResult<OptOrderResponse>> BillAsync(Guid id, OptOrderBillRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            if (order.IsRemake)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val057, RemakeBillMessage);

            var tolak = RejectUnlessUnbilledDraft(order) ?? RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;

            var encounter = await _visitService.FindOpticalEncounterAsync(order.EncounterId, cancellationToken);
            if (encounter is null || OptVisitService.IsClosed(encounter.EncounterStatus))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val030, "Kunjungan sudah ditutup. Daftarkan kunjungan baru.");

            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            if (aktif.Count == 0)
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, NoItemMessage);

            var waktuPesan = order.UpdateDateTime ?? order.CreateDateTime;
            var tarif = await _billing.CheckItemTariffsAsync(order, aktif, waktuPesan, cancellationToken);
            if (!tarif.IsSuccess)
                return Relay(tarif);

            var fakta = await _billing.EmitItemChargesAsync(order, aktif, BillingSourceContract.OpticalItemMilestones[0], waktuPesan, actorUserId, cancellationToken);
            if (!fakta.IsSuccess)
                return Relay(fakta);

            // Producer dapat membersihkan change tracker konteks bersama bila penyerahannya terputus; muat ulang bila terlepas.
            if (_dbContext.Entry(order).State == EntityState.Detached)
            {
                order = await LoadTrackedAsync(id, cancellationToken);
                if (order is null || order.Status != OptOrderStatus.Draft || order.BilledAt is not null)
                    return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
                aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).ToList();
            }

            var now = Now();
            // Penjualan langsung tetap Draf sesudah ditagih (OPT-STATE-v1 bagian 3); berpengerjaan menunggu uang muka.
            var ke = order.OrderKind == OptOrderKind.DirectSale ? OptOrderStatus.Draft : OptOrderStatus.AwaitingDownPayment;
            order.Status = ke;
            order.BilledAt = now;
            foreach (var baris in aktif)
                baris.BillingFactVersion = Math.Max(baris.BillingFactVersion, 1);
            Touch(order, actorUserId, now);
            _dbContext.OptOrderStatusHistories.Add(History(order.Id, OptOrderStatus.Draft, ke, actorUserId, now, ke == OptOrderStatus.Draft ? "Ditagihkan" : null));

            return await SaveAsync(order, cancellationToken);
        }

        public async Task<OptResult<OptOrderResponse>> CancelAsync(Guid id, OptOrderCancelRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);

            if (StatusSesudahPengerjaan.Contains(order.Status))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val057, CancelAfterWorkMessage);
            if (order.Status is not (OptOrderStatus.Draft or OptOrderStatus.AwaitingDownPayment))
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;
            if (string.IsNullOrWhiteSpace(request.Reason))
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val053, CancelReasonMessage);

            var now = Now();
            var dari = order.Status;
            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).ToList();
            foreach (var baris in aktif)
            {
                baris.Status = OptOrderItemStatus.Cancelled;
                baris.CancelledAt = now;
                baris.UpdateDateTime = now;
                baris.UpdateBy = actorUserId;
            }
            order.Status = OptOrderStatus.Cancelled;
            order.CancelledAt = now;
            Touch(order, actorUserId, now);
            _dbContext.OptOrderStatusHistories.Add(History(order.Id, dari, OptOrderStatus.Cancelled, actorUserId, now, request.Reason.Trim()));

            var simpan = await SaveAsync(order, cancellationToken);
            if (!simpan.IsSuccess)
                return simpan;

            var gagal = await _billing.EmitItemCancellationsAsync(order, aktif, now, actorUserId, cancellationToken);
            if (gagal.Count > 0)
                simpan.Value!.BillingMessage = CancelBillingPendingMessage;
            return simpan;
        }

        // ---------------------------------------------------------------- pengerjaan (BE-OPT-034)

        public const string WorkshopIncompleteMessage = "Lengkapi tujuan pengerjaan dan perkiraan jadi.";

        public async Task<OptResult<OptOrderResponse>> SendToWorkAsync(Guid id, OptOrderSendToWorkRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);

            // Tombol ditekan dua kali: permintaan kedua membawa versi sebelum kirim dan isian yang sama.
            if (IsSendToWorkReplay(order, request))
                return OptResult<OptOrderResponse>.Ok(await ProjectAsync(await ReloadAsync(order.Id, cancellationToken), cancellationToken));

            if (order.Status != OptOrderStatus.AwaitingDownPayment)
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion) ?? await ValidateWorkshopAsync(request, cancellationToken);
            if (tolak is not null)
                return tolak;

            var encounter = await _visitService.FindOpticalEncounterAsync(order.EncounterId, cancellationToken);
            if (encounter is null || OptVisitService.IsClosed(encounter.EncounterStatus))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val030, "Kunjungan sudah ditutup. Daftarkan kunjungan baru.");

            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            var bayar = await _paymentGate.GetStatusAsync(order, cancellationToken);
            if (!bayar.IsSuccess)
                return Relay(bayar);
            if (bayar.Value!.BlockingOrderNumber is { } penahan)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val047, $"Lunasi pesanan {penahan} dulu atau daftarkan kunjungan baru.");
            if (!bayar.Value.IsDownPaymentMet)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val046, $"Uang muka belum memenuhi minimum {Rupiah(bayar.Value.DownPaymentMinAmount)}.");

            var now = Now();
            await using (var trx = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                foreach (var baris in aktif.Where(OptStockGateway.RequiresStock))
                {
                    var stok = await _stockGateway.IssueAsync(baris, cancellationToken);
                    if (!stok.IsSuccess)
                    {
                        await trx.RollbackAsync(cancellationToken);
                        return Relay(stok);
                    }
                    baris.StockIssuedAt ??= now;
                }

                if (order.PrescriptionId is { } prescriptionId)
                {
                    var resep = await _dbContext.OptPrescriptions.FirstOrDefaultAsync(x => x.Id == prescriptionId, cancellationToken);
                    if (resep is not null && resep.LockedAt is null)
                    {
                        // OPT-DEC-007: sesudah lensa dipesan, resep tidak boleh direvisi (OPT-VAL-022).
                        resep.LockedAt = now;
                        resep.LockedByOrderId = order.Id;
                        resep.Version += 1;
                        resep.UpdateDateTime = now;
                        resep.UpdateBy = actorUserId;
                    }
                }

                order.Status = OptOrderStatus.InFabrication;
                order.SentToWorkAt = now;
                order.WorkshopType = request.WorkshopType;
                order.ExternalLabSupplierId = request.WorkshopType == OptWorkshopType.ExternalLab ? request.ExternalLabSupplierId : null;
                order.EstimatedReadyDate = request.EstimatedReadyDate;
                Touch(order, actorUserId, now);
                _dbContext.OptOrderStatusHistories.Add(History(order.Id, OptOrderStatus.AwaitingDownPayment, OptOrderStatus.InFabrication, actorUserId, now, null));

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await trx.CommitAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await trx.RollbackAsync(cancellationToken);
                    return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
                }
            }

            return await AfterProgressAsync(order, aktif, "IN_WORK", now, actorUserId, cancellationToken);
        }

        public async Task<OptResult<OptOrderResponse>> MarkReadyAsync(Guid id, OptOrderMarkReadyRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            if (order.Status != OptOrderStatus.InFabrication)
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;

            var now = Now();
            order.Status = OptOrderStatus.ReadyForPickup;
            order.ReadyAt = now;
            Touch(order, actorUserId, now);
            _dbContext.OptOrderStatusHistories.Add(History(order.Id, OptOrderStatus.InFabrication, OptOrderStatus.ReadyForPickup, actorUserId, now, null));
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
            }

            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            return await AfterProgressAsync(order, aktif, "READY", now, actorUserId, cancellationToken);
        }

        /// <summary>
        /// Sesudah commit: revisi fakta tagihan barang ke tahap baru. Pesanan remake tidak punya tagihan. Gagal menyerahkan
        /// tidak membatalkan perubahan status; jawaban membawa <c>BillingMessage</c> dan fakta yang tercatat dikirim ulang producer.
        /// </summary>
        private async Task<OptResult<OptOrderResponse>> AfterProgressAsync(
            OptOrder order, List<OptOrderItem> aktif, string milestone, DateTime now, Guid actorUserId, CancellationToken cancellationToken)
        {
            string? pesanTagihan = null;
            if (!order.IsRemake && aktif.Count > 0)
            {
                var fakta = await _billing.EmitItemChargesAsync(order, aktif, milestone, now, actorUserId, cancellationToken);
                if (!fakta.IsSuccess)
                    pesanTagihan = "Status pesanan tersimpan, tetapi tagihan belum mengikuti tahap ini. Sistem akan mengirim ulang; bila tetap tertinggal, minta kasir memeriksa tagihan.";
            }

            var hasil = await ProjectAsync(await ReloadAsync(order.Id, cancellationToken), cancellationToken);
            hasil.BillingMessage = pesanTagihan;
            return OptResult<OptOrderResponse>.Ok(hasil);
        }

        private static bool IsSendToWorkReplay(OptOrder order, OptOrderSendToWorkRequest request) =>
            order.Status == OptOrderStatus.InFabrication
            && order.SentToWorkAt is not null
            && request.ExpectedVersion is { } v && v == order.Version - 1
            && request.WorkshopType == order.WorkshopType
            && request.EstimatedReadyDate == order.EstimatedReadyDate
            && (request.WorkshopType == OptWorkshopType.ExternalLab ? request.ExternalLabSupplierId : null) == order.ExternalLabSupplierId;

        private async Task<OptResult<OptOrderResponse>?> ValidateWorkshopAsync(OptOrderSendToWorkRequest request, CancellationToken cancellationToken)
        {
            var tidakLengkap = OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val048, WorkshopIncompleteMessage);
            if (request.WorkshopType is not { } jenis || !Enum.IsDefined(jenis))
                return tidakLengkap;
            if (request.EstimatedReadyDate is not { } perkiraan || perkiraan < DateOnly.FromDateTime(Today()))
                return tidakLengkap;
            if (jenis == OptWorkshopType.ExternalLab)
            {
                if (request.ExternalLabSupplierId is not { } supplierId || supplierId == Guid.Empty)
                    return tidakLengkap;
                var ada = await _dbContext.Set<MstSupplier>().AsNoTracking()
                    .AnyAsync(x => x.Id == supplierId && x.IsActive && !x.IsDelete, cancellationToken);
                if (!ada)
                    return tidakLengkap;
            }
            return null;
        }

        private static string Rupiah(decimal amount) => "Rp " + amount.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));

        private Task<OptOrder> ReloadAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.OptOrders.AsNoTracking().Include(x => x.Items).Include(x => x.StatusHistories)
                .FirstAsync(x => x.Id == id, cancellationToken);

        // ---------------------------------------------------------------- penyerahan (BE-OPT-040)

        public const string IdentityMessage = "Cocokkan identitas pasien sebelum menyerahkan.";
        public const string RepresentativeMessage = "Isi nama dan hubungan pengambil.";

        /// <summary>
        /// Serahkan pesanan <c>Siap Diambil</c> (<c>OPT-DEC-041</c>, <c>043</c>, <c>048</c>). Syarat: identitas dicocokkan, wakil
        /// lengkap, dan <b>tagihan kunjungan lunas</b> (<c>OPT-AC-009</c>; dibaca per tagihan selama aturan sementara
        /// <c>OPT-DEC-035</c> berlaku). Masa garansi dibekukan dari pengaturan. Sesudah commit, tagihan barang direvisi ke
        /// <c>HANDED_OVER</c>, lalu Billing diminta memfinalisasi bila ini pesanan terakhir yang berjalan di kunjungan.
        /// Penyerahan penjualan langsung (<c>BE-OPT-042</c>) dan serah susulan (<c>BE-OPT-041</c>) tidak lewat sini.
        /// </summary>
        public async Task<OptResult<OptOrderResponse>> HandOverAsync(Guid id, OptOrderHandOverRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            // Serah susulan (BE-OPT-041, OPT-DEC-049): pesanan Tidak Diambil boleh diserahkan sesudah lunas.
            // Penjualan langsung (BE-OPT-042) diserahkan dari Draf yang sudah ditagih.
            var penjualanLangsung = order.OrderKind == OptOrderKind.DirectSale && order.Status == OptOrderStatus.Draft && order.BilledAt is not null;
            if (!penjualanLangsung && order.Status is not (OptOrderStatus.ReadyForPickup or OptOrderStatus.NotCollected))
                return InvalidStatus(order);
            var dariStatus = order.Status;
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;
            if (!request.HandoverIdentityVerified)
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val051, IdentityMessage);

            var nama = string.IsNullOrWhiteSpace(request.ReceiverName) ? null : request.ReceiverName.Trim();
            var hubungan = string.IsNullOrWhiteSpace(request.ReceiverRelationship) ? null : request.ReceiverRelationship.Trim();
            if ((nama is null) != (hubungan is null))
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val052, RepresentativeMessage);

            if (!order.IsRemake)
            {
                var bayar = await _paymentGate.GetStatusAsync(order, cancellationToken);
                if (!bayar.IsSuccess)
                    return Relay(bayar);
                if (!bayar.Value!.IsInvoiceSettled)
                    return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val050, $"Tagihan belum lunas. Sisa {Rupiah(bayar.Value.InvoiceOutstandingAmount)}.");
            }

            var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == order.EncounterId).Select(x => new { x.ServiceUnitId }).FirstAsync(cancellationToken);
            var setting = await _dbContext.OptSettings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ServiceUnitId == encounter.ServiceUnitId && !x.IsDelete, cancellationToken);
            if (setting is null)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.SettingMissing, "Pengaturan Optik belum tersedia. Hubungi Admin Optik.");

            var now = Now();
            var aktifSerah = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            await using var trx = penjualanLangsung ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
            if (penjualanLangsung)
            {
                // OPT-DEC-020: stok penjualan langsung dipotong saat diserahkan, satu transaksi dengan perubahan status.
                foreach (var baris in aktifSerah.Where(OptStockGateway.RequiresStock))
                {
                    var stok = await _stockGateway.IssueAsync(baris, cancellationToken);
                    if (!stok.IsSuccess)
                    {
                        await trx!.RollbackAsync(cancellationToken);
                        return Relay(stok);
                    }
                    baris.StockIssuedAt ??= now;
                }
            }

            order.Status = OptOrderStatus.HandedOver;
            order.HandedOverAt = now;
            order.HandedOverBy = actorUserId;
            order.HandoverIdentityVerified = true;
            order.HandoverReceiverName = nama;
            order.HandoverReceiverRelationship = hubungan;
            order.FittingNote = string.IsNullOrWhiteSpace(request.FittingNote) ? null : request.FittingNote.Trim();
            order.WarrantyFitDaysSnapshot = setting.WarrantyFitDays;
            order.WarrantyLensDefectMonthsSnapshot = setting.WarrantyLensDefectMonths;
            order.WarrantyFrameDefectMonthsSnapshot = setting.WarrantyFrameDefectMonths;
            Touch(order, actorUserId, now);
            var catatan = nama is null ? null : $"Diambil oleh {nama} ({hubungan})";
            if (dariStatus == OptOrderStatus.NotCollected)
                catatan = "Penyerahan susulan" + (catatan is null ? string.Empty : $"; {catatan}");
            _dbContext.OptOrderStatusHistories.Add(History(order.Id, dariStatus, OptOrderStatus.HandedOver, actorUserId, now, catatan));
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (trx is not null)
                    await trx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (trx is not null)
                    await trx.RollbackAsync(cancellationToken);
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
            }
            if (trx is not null)
                await trx.DisposeAsync();

            var hasil = await AfterProgressAsync(order, aktifSerah, "HANDED_OVER", now, actorUserId, cancellationToken);
            var (final, _) = await _billing.TryCompleteVisitBillingAsync(order.EncounterId, order.Id, actorUserId, cancellationToken);
            hasil.Value!.InvoiceFinalized = final;
            return hasil;
        }

        // ---------------------------------------------------------------- batal sesudah pengerjaan dan tidak diambil (BE-OPT-041)

        public const string LensCutMessage = "Tandai apakah lensa stok sudah dipotong.";
        public const string PendingCancellationMessage = "Masih ada pengajuan batal yang menunggu keputusan.";
        public const string RejectNoteMessage = "Tuliskan alasan penolakan.";

        /// <summary>
        /// Ajukan batal pesanan <c>Dalam Pengerjaan</c>/<c>Siap Diambil</c> (<c>OPT-DEC-010</c>, <c>038</c>). Setiap lensa stok
        /// wajib ditandai sudah/belum dipotong. Status pesanan tidak berubah sampai Kepala Optik memutuskan.
        /// </summary>
        public async Task<OptResult<OptOrderResponse>> RequestCancellationAsync(Guid id, OptOrderCancellationCreateRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            if (!StatusSesudahPengerjaan.Contains(order.Status))
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;
            if (string.IsNullOrWhiteSpace(request.Reason))
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val053, CancelReasonMessage);
            if (await _dbContext.OptOrderCancellations.AnyAsync(x => x.OrderId == order.Id && x.Status == OptCancellationStatus.Requested && !x.IsDelete, cancellationToken))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val055, PendingCancellationMessage);

            var tanda = (request.Items ?? new()).GroupBy(x => x.ItemId).ToDictionary(g => g.Key, g => g.Last().IsLensCut);
            var lensaStok = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete
                && x.ItemCategory == OptItemCategory.SpectacleLens && x.LensSupply == OptLensSupply.Stock).ToList();
            if (lensaStok.Any(x => !tanda.TryGetValue(x.Id, out var potong) || potong is null))
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val054, LensCutMessage);

            var now = Now();
            foreach (var lensa in lensaStok)
                lensa.IsLensCut = tanda[lensa.Id];
            _dbContext.OptOrderCancellations.Add(new Models.OptOrderCancellation
            {
                OrderId = order.Id, Status = OptCancellationStatus.Requested, Reason = request.Reason.Trim(),
                RequestedBy = actorUserId, RequestedAt = now, CreateDateTime = now, CreateBy = actorUserId,
            });
            Touch(order, actorUserId, now);
            try
            {
                return await SaveAsync(order, cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Unique index pengajuan Requested: pengajuan lain menang lebih dulu.
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val055, PendingCancellationMessage);
            }
        }

        /// <summary>
        /// Keputusan Kepala Optik. <b>Setuju</b>: dalam satu transaksi frame dan lensa stok yang belum dipotong kembali ke stok
        /// (Stok Masuk batch yang sama, <c>OPT-DEC-060</c>) dan barisnya batal, sedangkan lensa custom dan lensa stok yang sudah
        /// dipotong tetap ditagih (<c>OPT-DEC-010</c>, <c>038</c>); pesanan <c>Batal</c>. Sesudah commit seluruh baris tagihannya
        /// ditutup ke <c>NOT_COLLECTED</c> agar tidak menahan finalisasi, lalu baris yang batal diterbitkan pembatalannya;
        /// Billing menetapkan bentuknya (adjustment kredit karena sudah dikerjakan). <b>Tolak</b>: catatan wajib, status tetap.
        /// </summary>
        public async Task<OptResult<OptOrderResponse>> DecideCancellationAsync(Guid id, Guid requestId, OptOrderCancelDecisionRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            var pengajuan = await _dbContext.OptOrderCancellations
                .FirstOrDefaultAsync(x => x.Id == requestId && x.OrderId == order.Id && !x.IsDelete, cancellationToken);
            if (pengajuan is null)
                return OptResult<OptOrderResponse>.NotFound("Pengajuan batal tidak ditemukan.");
            if (pengajuan.Status != OptCancellationStatus.Requested)
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val057, "Pengajuan batal ini sudah diputuskan.");
            if (!StatusSesudahPengerjaan.Contains(order.Status))
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;

            var now = Now();
            var catatan = string.IsNullOrWhiteSpace(request.DecisionNote) ? null : request.DecisionNote.Trim();
            pengajuan.DecidedBy = actorUserId;
            pengajuan.DecidedAt = now;
            pengajuan.DecisionNote = catatan;
            pengajuan.UpdateDateTime = now;
            pengajuan.UpdateBy = actorUserId;

            if (!request.Approve)
            {
                if (catatan is null)
                    return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, RejectNoteMessage);
                pengajuan.Status = OptCancellationStatus.Rejected;
                Touch(order, actorUserId, now);
                return await SaveAsync(order, cancellationToken);
            }

            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            var batal = aktif.Where(x => !TetapDitagih(x)).ToList();
            var dari = order.Status;

            await using (var trx = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                foreach (var baris in batal)
                {
                    if (baris.StockIssuedAt is not null && baris.StockReturnedAt is null)
                    {
                        var kembali = await _stockGateway.ReturnAsync(baris, cancellationToken);
                        if (!kembali.IsSuccess)
                        {
                            await trx.RollbackAsync(cancellationToken);
                            return Relay(kembali);
                        }
                        baris.StockReturnedAt = now;
                    }
                    baris.Status = OptOrderItemStatus.Cancelled;
                    baris.CancelledAt = now;
                    baris.UpdateDateTime = now;
                    baris.UpdateBy = actorUserId;
                }

                pengajuan.Status = OptCancellationStatus.Approved;
                order.Status = OptOrderStatus.Cancelled;
                order.CancelledAt = now;
                Touch(order, actorUserId, now);
                _dbContext.OptOrderStatusHistories.Add(History(order.Id, dari, OptOrderStatus.Cancelled, actorUserId, now,
                    pengajuan.Reason + (catatan is null ? string.Empty : $" — disetujui: {catatan}")));
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await trx.CommitAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await trx.RollbackAsync(cancellationToken);
                    return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
                }
            }

            return await CloseBillingAsync(order, aktif, batal, now, actorUserId, cancellationToken);
        }

        /// <summary>
        /// Tutup pesanan <c>Siap Diambil</c> sebagai <c>Tidak Diambil</c> sesudah <c>NotCollectedCloseDays</c> (<c>OPT-DEC-018</c>,
        /// <c>049</c>). Seluruh barang tetap ditagih dan frame tidak dibongkar ke stok; sisa kurang bayar menjadi piutang pasien
        /// milik Billing. Pasien yang datang kemudian dan melunasi menerima kacamatanya lewat serah susulan.
        /// </summary>
        public async Task<OptResult<OptOrderResponse>> CloseNotCollectedAsync(Guid id, OptOrderNotCollectedRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var order = await LoadTrackedAsync(id, cancellationToken);
            if (order is null)
                return OptResult<OptOrderResponse>.NotFound(NotFoundMessage);
            if (order.Status != OptOrderStatus.ReadyForPickup)
                return InvalidStatus(order);
            var tolak = RejectStale(order, request.ExpectedVersion);
            if (tolak is not null)
                return tolak;
            if (string.IsNullOrWhiteSpace(request.Reason))
                return OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val053, "Tuliskan alasan penutupan.");

            var unitId = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == order.EncounterId).Select(x => x.ServiceUnitId).FirstAsync(cancellationToken);
            var hari = NotCollectedCloseDays(await NotCollectedCloseDaysByUnitAsync(cancellationToken), unitId);
            if (!IsPastNotCollectedLimit(order.ReadyAt ?? order.UpdateDateTime ?? order.CreateDateTime, hari, DateOnly.FromDateTime(Today())))
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val056, $"Pesanan baru bisa ditutup sesudah {hari} hari sejak siap diambil.");

            var now = Now();
            order.Status = OptOrderStatus.NotCollected;
            order.NotCollectedAt = now;
            Touch(order, actorUserId, now);
            _dbContext.OptOrderStatusHistories.Add(History(order.Id, OptOrderStatus.ReadyForPickup, OptOrderStatus.NotCollected, actorUserId, now, request.Reason.Trim()));
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
            }

            var aktif = order.Items.Where(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete).OrderBy(x => x.LineNo).ToList();
            return await CloseBillingAsync(order, aktif, new List<OptOrderItem>(), now, actorUserId, cancellationToken);
        }

        /// <summary>Lensa custom dan lensa stok yang sudah dipotong tetap ditagih; selain itu batal (<c>OPT-DEC-010</c>, <c>038</c>).</summary>
        private static bool TetapDitagih(OptOrderItem item) =>
            item.ItemCategory == OptItemCategory.SpectacleLens
            && (item.LensSupply == OptLensSupply.Custom || item.IsLensCut == true);

        /// <summary>
        /// Sesudah commit pesanan yang berakhir tanpa diserahkan: seluruh baris tagihannya dibawa ke <c>NOT_COLLECTED</c> (terminal),
        /// karena Billing hanya menilai status baris aktif dan adjustment kredit tidak mengubahnya; lalu baris yang batal diterbitkan
        /// pembatalannya, dan Billing diminta memfinalisasi bila tidak ada pesanan berjalan dan tagihan lunas.
        /// </summary>
        private async Task<OptResult<OptOrderResponse>> CloseBillingAsync(
            OptOrder order, List<OptOrderItem> semua, List<OptOrderItem> batal, DateTime now, Guid actorUserId, CancellationToken cancellationToken)
        {
            string? pesan = null;
            if (!order.IsRemake && semua.Count > 0)
            {
                var tutup = await _billing.EmitItemChargesAsync(order, semua, "NOT_COLLECTED", now, actorUserId, cancellationToken);
                var gagal = batal.Count == 0 ? (IReadOnlyList<OptOrderItem>)Array.Empty<OptOrderItem>()
                    : await _billing.EmitItemCancellationsAsync(order, batal, now, actorUserId, cancellationToken);
                if (!tutup.IsSuccess || gagal.Count > 0)
                    pesan = "Status pesanan tersimpan, tetapi tagihan belum mengikuti perubahan ini. Sistem akan mengirim ulang; bila tetap tertinggal, minta kasir memeriksa tagihan.";
            }

            var (final, _) = await _billing.TryCompleteVisitBillingAsync(order.EncounterId, order.Id, actorUserId, cancellationToken);
            var hasil = await ProjectAsync(await ReloadAsync(order.Id, cancellationToken), cancellationToken);
            hasil.BillingMessage = pesan;
            hasil.InvoiceFinalized = final;
            return OptResult<OptOrderResponse>.Ok(hasil);
        }

        // ---------------------------------------------------------------- validasi isi

        private sealed record BarisValid(Guid DrugId, OptItemCategory Category, OptLensSupply? LensSupply, decimal Quantity);

        private sealed record IsiValid(List<BarisValid> Items, Guid? PrescriptionId, Guid? PrescriptionVersionId, OptResult<OptOrderResponse>? Error)
        {
            public static IsiValid Fail(OptResult<OptOrderResponse> error) => new([], null, null, error);
        }

        private async Task<IsiValid> ValidateContentAsync(OptOrderCreateRequest request, Guid patientId, CancellationToken cancellationToken)
        {
            var items = request.Items ?? new List<OptOrderItemInput>();
            if (items.Count == 0)
                return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, NoItemMessage));

            var drugIds = items.Where(x => x.DrugId.HasValue).Select(x => x.DrugId!.Value).Distinct().ToList();
            var profil = await _dbContext.OptItemProfiles.AsNoTracking()
                .Where(x => drugIds.Contains(x.DrugId) && x.IsActive && !x.IsDelete)
                .ToDictionaryAsync(x => x.DrugId, x => x.ItemCategory, cancellationToken);

            var valid = new List<BarisValid>();
            foreach (var item in items)
            {
                if (item.DrugId is not { } drugId || !profil.TryGetValue(drugId, out var kategori))
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val040, NotOpticalItemMessage));
                if (item.Quantity <= 0)
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val044, QuantityMessage));
                if (kategori == OptItemCategory.SpectacleLens && item.LensSupply is null)
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val043, LensSupplyMessage));
                if (item.LensSupply is { } suplai && !Enum.IsDefined(suplai))
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val043, LensSupplyMessage));
                // Suplai hanya bermakna untuk lensa; barang lain selalu lewat stok.
                var lensSupply = kategori is OptItemCategory.SpectacleLens or OptItemCategory.ContactLens ? item.LensSupply : null;
                valid.Add(new BarisValid(drugId, kategori, lensSupply, item.Quantity));
            }

            var adaLensaKontak = valid.Any(x => x.Category == OptItemCategory.ContactLens);
            var adaLensaKacamata = valid.Any(x => x.Category == OptItemCategory.SpectacleLens);
            if (request.OrderKind == OptOrderKind.DirectSale && adaLensaKacamata)
                return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, DirectSaleSpectacleLensMessage));
            if (adaLensaKontak && adaLensaKacamata)
                return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, MixedPrescriptionMessage));

            if (request.PrescriptionVersionId is not { } versionId || versionId == Guid.Empty)
            {
                if (adaLensaKontak)
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val042, ContactLensPrescriptionMessage));
                // OPT-DEC-020: barang bebas resep pada penjualan langsung tidak butuh resep.
                if (request.OrderKind == OptOrderKind.DirectSale)
                    return new IsiValid(valid, null, null, null);
                return IsiValid.Fail(OptResult<OptOrderResponse>.Invalid(OptErrorCodes.Val041, PrescriptionRequiredMessage));
            }

            var versi = await _dbContext.OptPrescriptionVersions.AsNoTracking()
                .Where(x => x.Id == versionId && !x.IsDelete)
                .Select(x => new { x.PrescriptionId, x.VersionNo, x.Prescription!.CurrentVersionNo })
                .FirstOrDefaultAsync(cancellationToken);
            if (versi is null)
                return IsiValid.Fail(OptResult<OptOrderResponse>.NotFound("Resep tidak ditemukan."));
            if (versi.VersionNo != versi.CurrentVersionNo)
                return IsiValid.Fail(OptResult<OptOrderResponse>.Conflict(OptErrorCodes.InvalidState, OutdatedVersionMessage));

            var jenis = adaLensaKontak ? OptPrescriptionType.ContactLens : OptPrescriptionType.Spectacle;
            var layak = await _prescriptionService.CheckUsableForOrderAsync(versi.PrescriptionId, patientId, jenis, cancellationToken);
            if (!layak.IsSuccess)
            {
                // OPT-VAL-042: lensa kontak dengan resep yang bukan resep lensa kontak, atau sudah kedaluwarsa.
                if (adaLensaKontak && layak.Kind != OptResultKind.NotFound)
                    return IsiValid.Fail(OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val042, ContactLensPrescriptionMessage));
                return IsiValid.Fail(Relay(layak));
            }

            return new IsiValid(valid, versi.PrescriptionId, versionId, null);
        }

        private static OptResult<OptOrderResponse>? RejectKind(OptOrderKind kind) => kind switch
        {
            OptOrderKind.Fabrication => null,
            OptOrderKind.DirectSale => null,
            OptOrderKind.Remake => OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, RemakeMessage),
            _ => OptResult<OptOrderResponse>.Invalid(OptErrorCodes.InvalidRequest, "Jenis pesanan tidak dikenal."),
        };

        private static OptResult<OptOrderResponse>? RejectUnlessUnbilledDraft(OptOrder order) =>
            order.Status == OptOrderStatus.Draft && order.BilledAt is null ? null : InvalidStatus(order);

        private static OptResult<OptOrderResponse> InvalidStatus(OptOrder order) =>
            OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val057,
                $"Tindakan ini tidak berlaku untuk pesanan berstatus {OptLabels.OrderStatus(order.Status)}.");

        private static OptResult<OptOrderResponse>? RejectStale(OptOrder order, int? expectedVersion) =>
            expectedVersion is { } v && v == order.Version
                ? null
                : OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);

        private static OptResult<OptOrderResponse> Relay<T>(OptResult<T> source) => source.Kind switch
        {
            OptResultKind.NotFound => OptResult<OptOrderResponse>.NotFound(source.Message ?? NotFoundMessage),
            OptResultKind.Forbidden => OptResult<OptOrderResponse>.Forbidden(source.Code!, source.Message!),
            OptResultKind.Conflict => OptResult<OptOrderResponse>.Conflict(source.Code!, source.Message!),
            OptResultKind.Unavailable => OptResult<OptOrderResponse>.Unavailable(source.Code!, source.Message!),
            _ => OptResult<OptOrderResponse>.Invalid(source.Code ?? OptErrorCodes.InvalidRequest, source.Message ?? "Permintaan tidak valid."),
        };

        // ---------------------------------------------------------------- penolong

        private Task<OptOrder?> LoadTrackedAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.OptOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private static void Touch(OptOrder order, Guid actorUserId, DateTime now)
        {
            order.Version += 1;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;
        }

        private static OptOrderStatusHistory History(Guid orderId, OptOrderStatus? from, OptOrderStatus to, Guid actorUserId, DateTime now, string? reason) => new()
        {
            OrderId = orderId, FromStatus = from, ToStatus = to, ActorUserId = actorUserId, OccurredAt = now, Reason = reason,
            CreateDateTime = now, CreateBy = actorUserId,
        };

        private async Task<OptResult<OptOrderResponse>> SaveAsync(OptOrder order, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptOrderResponse>.Conflict(OptErrorCodes.Val058, StaleMessage);
            }

            var segar = await _dbContext.OptOrders.AsNoTracking()
                .Include(x => x.Items).Include(x => x.StatusHistories)
                .FirstAsync(x => x.Id == order.Id, cancellationToken);
            return OptResult<OptOrderResponse>.Ok(await ProjectAsync(segar, cancellationToken));
        }

        private async Task<List<OptOrderListItemResponse>> ProjectListAsync(IQueryable<OptOrder> source, CancellationToken cancellationToken)
        {
            var rows = await (
                from o in source
                join e in _dbContext.Set<RegPatientEncounter>().AsNoTracking() on o.EncounterId equals e.Id
                select new
                {
                    Row = new OptOrderListItemResponse
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        EncounterId = o.EncounterId,
                        EncounterNumber = e.EncounterNumber,
                        PatientId = o.PatientId,
                        MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == o.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault() ?? string.Empty,
                        FullName = _dbContext.Set<MstPatient>().Where(p => p.Id == o.PatientId).Select(p => p.FullName).FirstOrDefault() ?? string.Empty,
                        OrderKind = o.OrderKind,
                        Status = o.Status,
                        ItemCount = o.Items.Count(i => i.Status == OptOrderItemStatus.Active && !i.IsDelete),
                        EstimatedReadyDate = o.EstimatedReadyDate,
                        CreatedAt = o.CreateDateTime,
                        Version = o.Version,
                    },
                    o.ReadyAt,
                    SiapAt = o.ReadyAt ?? o.UpdateDateTime ?? o.CreateDateTime,
                    e.ServiceUnitId,
                }).ToListAsync(cancellationToken);

            foreach (var x in rows)
            {
                x.Row.OrderKindLabel = OptLabels.OrderKind(x.Row.OrderKind);
                x.Row.StatusLabel = OptLabels.OrderStatus(x.Row.Status);
            }
            await MarkFlagsAsync(rows.Select(x => new FlagSource(x.Row, x.ReadyAt, x.SiapAt, x.ServiceUnitId)).ToList(), cancellationToken);
            return rows.Select(x => x.Row).ToList();
        }

        // ---------------------------------------------------------------- tanda baris (BE-OPT-054)

        private sealed record FlagSource(OptOrderListItemResponse Row, DateTime? ReadyAt, DateTime SiapAt, Guid ServiceUnitId);

        /// <summary>
        /// Mengisi empat tanda baris dengan aturan yang sama dengan penyaringnya: <c>Overdue</c>, <c>NeedsContact</c>,
        /// <c>HasPendingCancellation</c>, dan <c>PastNotCollectedLimit</c> (= syarat Tutup Tidak Diambil).
        /// </summary>
        private async Task MarkFlagsAsync(IReadOnlyList<FlagSource> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
                return;
            var hariIni = DateOnly.FromDateTime(Today());
            var batasPengingat = Now().AddDays(-await PickupReminderDaysAsync(cancellationToken));
            var batasTutup = await NotCollectedCloseDaysByUnitAsync(cancellationToken);
            var ids = rows.Select(x => x.Row.Id).ToList();
            var menungguBatal = await _dbContext.OptOrderCancellations.AsNoTracking()
                .Where(c => ids.Contains(c.OrderId) && !c.IsDelete && c.Status == OptCancellationStatus.Requested)
                .Select(c => c.OrderId).Distinct().ToListAsync(cancellationToken);

            foreach (var x in rows)
            {
                var siap = x.Row.Status == OptOrderStatus.ReadyForPickup;
                x.Row.IsOverdue = x.Row.Status == OptOrderStatus.InFabrication && x.Row.EstimatedReadyDate is { } jadi && jadi < hariIni;
                x.Row.NeedsContact = siap && x.ReadyAt is { } siapAt && siapAt <= batasPengingat;
                x.Row.HasPendingCancellation = menungguBatal.Contains(x.Row.Id);
                x.Row.IsPastNotCollectedLimit = siap && IsPastNotCollectedLimit(x.SiapAt, NotCollectedCloseDays(batasTutup, x.ServiceUnitId), hariIni);
            }
        }

        private async Task<int> PickupReminderDaysAsync(CancellationToken cancellationToken) =>
            await _dbContext.OptSettings.AsNoTracking().Where(x => !x.IsDelete)
                .OrderBy(x => x.CreateDateTime).Select(x => (int?)x.PickupReminderDays).FirstOrDefaultAsync(cancellationToken) ?? 30;

        private async Task<Dictionary<Guid, int>> NotCollectedCloseDaysByUnitAsync(CancellationToken cancellationToken) =>
            await _dbContext.OptSettings.AsNoTracking().Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.ServiceUnitId, x => x.NotCollectedCloseDays, cancellationToken);

        public const int DefaultNotCollectedCloseDays = 90;

        private static int NotCollectedCloseDays(IReadOnlyDictionary<Guid, int> byUnit, Guid serviceUnitId) =>
            byUnit.TryGetValue(serviceUnitId, out var hari) ? hari : DefaultNotCollectedCloseDays;

        /// <summary>
        /// Satu-satunya aturan "sudah boleh ditutup Tidak Diambil" (<c>OPT-DEC-018</c>): tanggal siap (WIB) ditambah batas hari
        /// sudah sampai hari ini (WIB). Dipakai Tutup Tidak Diambil, penyaring <c>PastNotCollectedLimit</c>, dan tanda barisnya.
        /// </summary>
        internal static bool IsPastNotCollectedLimit(DateTime readyAtUtc, int closeDays, DateOnly todayWib)
        {
            var siapWib = DateOnly.FromDateTime(OptPractitionerLicenseReader.ToOperationalDate(
                new DateTimeOffset(DateTime.SpecifyKind(readyAtUtc, DateTimeKind.Utc))));
            return siapWib.AddDays(closeDays) <= todayWib;
        }

        private async Task<OptOrderResponse> ProjectAsync(OptOrder order, CancellationToken cancellationToken)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == order.EncounterId).Select(x => x.EncounterNumber).FirstOrDefaultAsync(cancellationToken);
            var pasien = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => x.Id == order.PatientId).Select(x => new { x.MedicalRecordNumber, x.FullName }).FirstOrDefaultAsync(cancellationToken);
            var drugIds = order.Items.Select(x => x.DrugId).Distinct().ToList();
            var barang = await _dbContext.Set<MstDrug>().AsNoTracking()
                .Where(x => drugIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new { x.DrugCode, x.DrugName }, cancellationToken);
            var pengajuanBatal = await _dbContext.OptOrderCancellations.AsNoTracking()
                .Where(x => x.OrderId == order.Id && !x.IsDelete).OrderBy(x => x.RequestedAt)
                .Select(x => new OptOrderCancellationResponse
                {
                    Id = x.Id, Status = x.Status, Reason = x.Reason, RequestedAt = x.RequestedAt, DecidedAt = x.DecidedAt, DecisionNote = x.DecisionNote,
                })
                .ToListAsync(cancellationToken);
            foreach (var p in pengajuanBatal)
                p.StatusLabel = p.Status switch
                {
                    OptCancellationStatus.Requested => "Menunggu Keputusan",
                    OptCancellationStatus.Approved => "Disetujui",
                    OptCancellationStatus.Rejected => "Ditolak",
                    _ => "Tidak diketahui",
                };
            var pemasok = order.ExternalLabSupplierId is { } supplierId
                ? await _dbContext.Set<MstSupplier>().AsNoTracking().Where(x => x.Id == supplierId).Select(x => x.SupplierName).FirstOrDefaultAsync(cancellationToken)
                : null;
            var resep = order.PrescriptionVersionId is { } versionId
                ? await _dbContext.OptPrescriptionVersions.AsNoTracking().Where(x => x.Id == versionId)
                    .Select(x => new { x.VersionNo, x.Prescription!.PrescriptionNumber }).FirstOrDefaultAsync(cancellationToken)
                : null;

            var unitKunjungan = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                .Where(x => x.Id == order.EncounterId).Select(x => x.ServiceUnitId).FirstOrDefaultAsync(cancellationToken);

            var response = new OptOrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                EncounterId = order.EncounterId,
                EncounterNumber = encounter ?? string.Empty,
                PatientId = order.PatientId,
                MedicalRecordNumber = pasien?.MedicalRecordNumber ?? string.Empty,
                FullName = pasien?.FullName ?? string.Empty,
                OrderKind = order.OrderKind,
                OrderKindLabel = OptLabels.OrderKind(order.OrderKind),
                Status = order.Status,
                StatusLabel = OptLabels.OrderStatus(order.Status),
                ItemCount = order.Items.Count(x => x.Status == OptOrderItemStatus.Active && !x.IsDelete),
                EstimatedReadyDate = order.EstimatedReadyDate,
                CreatedAt = order.CreateDateTime,
                Version = order.Version,
                PrescriptionId = order.PrescriptionId,
                PrescriptionVersionId = order.PrescriptionVersionId,
                PrescriptionNumber = resep?.PrescriptionNumber,
                PrescriptionVersionNo = resep?.VersionNo,
                DownPaymentMinPercentSnapshot = order.DownPaymentMinPercentSnapshot,
                BilledAt = order.BilledAt,
                WorkshopType = order.WorkshopType,
                WorkshopTypeLabel = order.WorkshopType is { } jenis ? OptLabels.WorkshopType(jenis) : null,
                ExternalLabSupplierId = order.ExternalLabSupplierId,
                ExternalLabSupplierName = pemasok,
                SentToWorkAt = order.SentToWorkAt,
                ReadyAt = order.ReadyAt,
                NotCollectedAt = order.NotCollectedAt,
                HandedOverAt = order.HandedOverAt,
                HandoverIdentityVerified = order.HandoverIdentityVerified,
                HandoverReceiverName = order.HandoverReceiverName,
                HandoverReceiverRelationship = order.HandoverReceiverRelationship,
                FittingNote = order.FittingNote,
                WarrantyFitDaysSnapshot = order.WarrantyFitDaysSnapshot,
                WarrantyLensDefectMonthsSnapshot = order.WarrantyLensDefectMonthsSnapshot,
                WarrantyFrameDefectMonthsSnapshot = order.WarrantyFrameDefectMonthsSnapshot,
                CancelledAt = order.CancelledAt,
                IsRemake = order.IsRemake,
                Items = order.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNo).Select(x => new OptOrderItemResponse
                {
                    Id = x.Id,
                    LineNo = x.LineNo,
                    DrugId = x.DrugId,
                    DrugCode = barang.GetValueOrDefault(x.DrugId)?.DrugCode ?? string.Empty,
                    DrugName = barang.GetValueOrDefault(x.DrugId)?.DrugName ?? string.Empty,
                    ItemCategory = x.ItemCategory,
                    ItemCategoryLabel = OptLabels.ItemCategory(x.ItemCategory),
                    LensSupply = x.LensSupply,
                    Quantity = x.Quantity,
                    StockIssuedAt = x.StockIssuedAt,
                    StockReturnedAt = x.StockReturnedAt,
                    IsLensCut = x.IsLensCut,
                    Status = x.Status,
                    StatusLabel = OptLabels.OrderItemStatus(x.Status),
                }).ToList(),
                Cancellations = pengajuanBatal,
                StatusHistory = order.StatusHistories.OrderBy(x => x.OccurredAt).Select(x => new OptOrderStatusHistoryResponse
                {
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ToStatusLabel = OptLabels.OrderStatus(x.ToStatus),
                    OccurredAt = x.OccurredAt,
                    Reason = x.Reason,
                }).ToList(),
            };
            await MarkFlagsAsync(new[] { new FlagSource(response, order.ReadyAt, order.ReadyAt ?? order.UpdateDateTime ?? order.CreateDateTime, unitKunjungan) }, cancellationToken);
            return response;
        }

        private DateTime Now() => _time.GetUtcNow().UtcDateTime;

        private DateTime Today() => OptPractitionerLicenseReader.ToOperationalDate(_time.GetUtcNow());
    }
}
