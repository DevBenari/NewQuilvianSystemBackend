using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services
{
    /// <summary>
    /// Jurnal: penyimpanan draft, penomoran, dan daur hidup persetujuan.
    ///
    /// Cakupan `BE-ACC-010` (simpan, ubah, hapus, nomor) dan `BE-ACC-011` (`submit`, `approve`,
    /// `reject`, `post`). Pembalikan dan penyesuaian adalah `BE-ACC-013`.
    /// </summary>
    /// <remarks>
    /// Seluruh aturan `ACC-VALIDATION-0.2` bagian 3 ditegakkan di service ini, bukan di
    /// controller, supaya jalur tulis mana pun melewatinya. Keseimbangan debit-kredit
    /// <b>tidak</b> termasuk: `ACC-DEC-025` mengizinkan draft timpang disimpan, dan
    /// keseimbangan baru menggigit saat pengajuan pada `BE-ACC-011`.
    /// </remarks>
    public class AccJournalService
    {
        private readonly ApplicationDbContext _db;

        public AccJournalService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Awalan kunci deret nomor jurnal pada <see cref="AccNumberSeries"/>.</summary>
        private const string AwalanKunciDeret = "ACC_JOURNAL_";

        /// <summary>
        /// Nomor jurnal direset tiap bulan akuntansi, sesuai bentuk
        /// <c>{prefix}/{yyyy}/{MM}/{00001}</c> yang memuat bulan di dalam nomornya.
        /// </summary>
        private const string KebijakanReset = "MONTHLY";

        /// <summary>Lebar urutan pada nomor jurnal — lima angka, contoh <c>00001</c>.</summary>
        private const int LebarUrutan = 5;

        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<JournalListResponse>>> GetPagedAsync(
            JournalPagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<JournalListResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccJournal> q = _db.Set<AccJournal>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.JournalTypeId.HasValue)
                q = q.Where(x => x.JournalTypeId == query.JournalTypeId.Value);

            if (query.JournalStatus.HasValue)
                q = q.Where(x => x.JournalStatus == query.JournalStatus.Value);

            if (query.DateFrom.HasValue)
            {
                var dari = query.DateFrom.Value.Date;
                q = q.Where(x => x.AccountingDate >= dari);
            }

            if (query.DateTo.HasValue)
            {
                var sampai = query.DateTo.Value.Date;
                q = q.Where(x => x.AccountingDate <= sampai);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.JournalNumber.ToLower().Contains(cari)
                              || x.Description.ToLower().Contains(cari));
            }

            var menurun = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "journalnumber" => menurun ? q.OrderByDescending(x => x.JournalNumber) : q.OrderBy(x => x.JournalNumber),
                "createdatetime" => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => menurun
                    ? q.OrderByDescending(x => x.AccountingDate).ThenByDescending(x => x.JournalNumber)
                    : q.OrderBy(x => x.AccountingDate).ThenBy(x => x.JournalNumber)
            };

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new JournalListResponse
                {
                    Id = x.Id,
                    JournalNumber = x.JournalNumber,
                    AccountingDate = x.AccountingDate,
                    JournalTypeId = x.JournalTypeId,
                    JournalTypeName = x.JournalType != null ? x.JournalType.JournalTypeName : string.Empty,
                    Description = x.Description,
                    JournalStatus = x.JournalStatus,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit
                })
                .ToListAsync(ct);

            var hasil = new PagedResult<JournalListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };

            return AccountingServiceResult<PagedResult<JournalListResponse>>.Ok(
                hasil, "Daftar jurnal berhasil diambil.");
        }

        public async Task<AccountingServiceResult<JournalDetailResponse>> GetByIdAsync(
            Guid id,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: false, ct);

            return jurnal is null
                ? TidakDitemukan<JournalDetailResponse>()
                : AccountingServiceResult<JournalDetailResponse>.Ok(
                    await PetakanRincianAsync(jurnal, actorUserId, izin, ct),
                    "Rincian jurnal berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        /// <remarks>
        /// Acceptance (1), (2), (5), (6), (7), dan (8) `BE-ACC-010` bertemu di method ini.
        /// Urutannya disengaja: seluruh pemeriksaan selesai <b>sebelum</b> transaction dibuka,
        /// supaya isian yang salah tidak pernah memegang advisory lock nomor jurnal.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> CreateAsync(
            CreateJournalRequest request,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (request.LegalEntityId == Guid.Empty)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Badan hukum wajib dipilih.");
            }

            var siap = await SiapkanAsync<JournalDetailResponse>(
                request.LegalEntityId,
                request.JournalTypeId,
                request.AccountingDate,
                request.Description,
                request.Lines,
                ct);

            if (siap.Gagal is not null) return siap.Gagal;

            var jenis = siap.JenisJurnal!;
            var tanggal = request.AccountingDate.Date;

            // Satu transaction memuat alokasi nomor sekaligus penyimpanan jurnal dan barisnya —
            // acceptance (8). Bila pemanggil sudah membuka transaction sendiri, transaction itu
            // yang dipakai dan tidak ada yang bersarang.
            var transaksiSendiri = _db.Database.CurrentTransaction is null;
            var transaksi = transaksiSendiri
                ? await _db.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                var nomor = await AlokasikanNomorJurnalAsync(
                    _db, jenis.NumberPrefix, request.LegalEntityId, tanggal, actorUserId, ct);

                var jurnal = new AccJournal
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = request.LegalEntityId,
                    JournalNumber = nomor,
                    JournalTypeId = jenis.Id,
                    AccountingPeriodId = siap.Periode!.Id,
                    DocumentNumber = request.DocumentNumber?.Trim(),
                    DocumentDate = request.DocumentDate?.Date,
                    AccountingDate = tanggal,
                    Description = request.Description.Trim(),
                    JournalStatus = JournalStatus.Draft,
                    TotalDebit = siap.Baris.Sum(x => x.DebitAmount),
                    TotalCredit = siap.Baris.Sum(x => x.CreditAmount),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };

                foreach (var baris in siap.Baris)
                {
                    baris.JournalId = jurnal.Id;
                    baris.CreateDateTime = jurnal.CreateDateTime;
                    baris.CreateBy = actorUserId;
                    jurnal.Lines.Add(baris);
                }

                _db.Set<AccJournal>().Add(jurnal);

                await _db.SaveChangesAsync(ct);

                if (transaksi is not null) await transaksi.CommitAsync(ct);

                var tersimpan = await MuatLengkapAsync(jurnal.Id, lacak: false, ct);

                return AccountingServiceResult<JournalDetailResponse>.Ok(
                    await PetakanRincianAsync(tersimpan!, actorUserId, izin, ct),
                    "Jurnal berhasil disimpan sebagai draft.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                if (transaksi is not null) await transaksi.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (transaksi is not null) await transaksi.DisposeAsync();
            }
        }

        /// <remarks>
        /// Baris dikirim <b>utuh</b> dan menggantikan seluruh baris sebelumnya. Nomor jurnal
        /// tidak pernah dialokasikan ulang, walaupun bulan akuntansinya berubah: nomor adalah
        /// identitas yang mungkin sudah dicetak dan dirujuk dokumen lain, dan nomor terlewat
        /// memang diizinkan (`ACC-DEC-014`) sedangkan nomor berpindah tidak diizinkan siapa pun.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> UpdateAsync(
            Guid id,
            UpdateJournalRequest request,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<JournalDetailResponse>();

            var status = PeriksaDapatDisunting<JournalDetailResponse>(jurnal, untukPenghapusan: false);
            if (status is not null) return status;

            // Rejected -> Draft. Perpindahan ini bagian dari penyuntingan itu sendiri
            // (`ACC-STATE-0.1` bagian 1.1), bukan endpoint tersendiri: memperbaiki jurnal yang
            // ditolak berarti ia kembali menjadi draft yang siap diajukan ulang.
            var kembaliKeDraft = jurnal.JournalStatus == JournalStatus.Rejected;

            var siap = await SiapkanAsync<JournalDetailResponse>(
                jurnal.LegalEntityId,
                request.JournalTypeId,
                request.AccountingDate,
                request.Description,
                request.Lines,
                ct);

            if (siap.Gagal is not null) return siap.Gagal;

            var sekarang = DateTime.UtcNow;

            var transaksiSendiri = _db.Database.CurrentTransaction is null;
            var transaksi = transaksiSendiri
                ? await _db.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                // Baris lama dibuang seluruhnya, bukan dicocokkan satu per satu. Kontrak menyebut
                // baris dikirim utuh, sehingga tidak ada baris "yang tidak disebut" untuk
                // dipertahankan. Draft belum pernah masuk buku besar, jadi penghapusan kerasnya
                // tidak menghilangkan jejak transaksi apa pun.
                //
                // DUA KALI SaveChanges, DI DALAM SATU TRANSACTION, dan itu disengaja:
                //
                // 1. Unique index (JournalId, LineNumber) menyaring "IsDelete" = false. Bila
                //    penghapusan baris lama dan penyisipan baris baru dikirim dalam satu batch,
                //    EF bebas menyusun urutannya, dan penyisipan baris nomor 1 yang mendahului
                //    penghapusan baris nomor 1 yang lama akan menabrak index itu.
                // 2. Baris pengganti sengaja TIDAK ditambahkan lewat navigasi `jurnal.Lines`.
                //    Menyunting koleksi yang sedang dilacak sambil menghapus isinya membuat EF
                //    memperlakukan entity baru sebagai `Modified`, sehingga ia mengirim UPDATE
                //    atas baris yang belum pernah ada dan gagal dengan
                //    DbUpdateConcurrencyException.
                //
                // Transaction-lah yang menjaga keduanya tetap satu satuan: bila penyisipan gagal,
                // penghapusan ikut dibatalkan dan jurnal tidak pernah kehilangan barisnya.
                var barisLama = await _db.Set<AccJournalLine>()
                    .Where(x => x.JournalId == jurnal.Id)
                    .ToListAsync(ct);

                _db.Set<AccJournalLine>().RemoveRange(barisLama);
                await _db.SaveChangesAsync(ct);

                foreach (var baris in siap.Baris)
                {
                    baris.JournalId = jurnal.Id;
                    baris.CreateDateTime = sekarang;
                    baris.CreateBy = actorUserId;
                }

                _db.Set<AccJournalLine>().AddRange(siap.Baris);

                jurnal.JournalTypeId = siap.JenisJurnal!.Id;
                jurnal.AccountingPeriodId = siap.Periode!.Id;
                jurnal.DocumentNumber = request.DocumentNumber?.Trim();
                jurnal.DocumentDate = request.DocumentDate?.Date;
                jurnal.AccountingDate = request.AccountingDate.Date;
                jurnal.Description = request.Description.Trim();
                jurnal.TotalDebit = siap.Baris.Sum(x => x.DebitAmount);
                jurnal.TotalCredit = siap.Baris.Sum(x => x.CreditAmount);
                jurnal.UpdateDateTime = sekarang;
                jurnal.UpdateBy = actorUserId;

                if (kembaliKeDraft)
                {
                    jurnal.JournalStatus = JournalStatus.Draft;
                    jurnal.RejectionReason = null;
                    jurnal.SubmittedBy = null;
                    jurnal.SubmittedAt = null;
                }

                await _db.SaveChangesAsync(ct);

                if (transaksi is not null) await transaksi.CommitAsync(ct);
            }
            catch
            {
                if (transaksi is not null) await transaksi.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (transaksi is not null) await transaksi.DisposeAsync();
            }

            var tersimpan = await MuatLengkapAsync(jurnal.Id, lacak: false, ct);

            return AccountingServiceResult<JournalDetailResponse>.Ok(
                await PetakanRincianAsync(tersimpan!, actorUserId, izin, ct),
                kembaliKeDraft
                    ? "Jurnal berhasil diperbarui dan kembali menjadi draft."
                    : "Jurnal berhasil diperbarui.");
        }

        /// <remarks>
        /// Penghapusan lunak. Hanya draft yang dapat dihapus; jurnal yang sudah disahkan
        /// permanen (`ACC-DEC-006`, `ACC-DEC-015`) dan penolakannya diperkuat lagi pada
        /// `BE-ACC-011` acceptance (4).
        /// </remarks>
        public async Task<AccountingServiceResult<bool>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<bool>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<bool>();

            var status = PeriksaDapatDisunting<bool>(jurnal, untukPenghapusan: true);
            if (status is not null) return status;

            var sekarang = DateTime.UtcNow;

            jurnal.IsDelete = true;
            jurnal.DeleteDateTime = sekarang;
            jurnal.DeleteBy = actorUserId;

            // Baris ikut ditandai supaya penghitungan saldo dan pemeriksaan "akun sudah
            // bertransaksi" tidak perlu ikut menelusuri status jurnal induknya di setiap query.
            foreach (var baris in jurnal.Lines)
            {
                baris.IsDelete = true;
                baris.DeleteDateTime = sekarang;
                baris.DeleteBy = actorUserId;
            }

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<bool>.Ok(true, "Jurnal draft berhasil dihapus.");
        }

        // ------------------------------------------------------------------
        // Daur hidup — BE-ACC-011
        // ------------------------------------------------------------------

        /// <summary>
        /// <c>Draft</c> atau <c>Rejected</c> → <c>PendingApproval</c>.
        /// </summary>
        /// <remarks>
        /// Kesembilan syarat `ACC-STATE-0.1` bagian 1.3 diperiksa di sini, dan **diperiksa ulang**
        /// pada <see cref="PostAsync"/>. Pemeriksaan kedua bukan duplikasi yang dapat dihemat:
        /// periode dapat ditutup, akun dapat dinonaktifkan, dan Cost Center dapat dipindahkan
        /// di antara pengajuan dan pengesahan.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> SubmitAsync(
            Guid id,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<JournalDetailResponse>();

            if (jurnal.JournalStatus is not (JournalStatus.Draft or JournalStatus.Rejected))
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jurnal {jurnal.JournalNumber} sudah diajukan dan tidak dapat diajukan lagi.");
            }

            var syarat = await PeriksaSembilanSyaratAsync<JournalDetailResponse>(jurnal, ct);
            if (syarat is not null) return syarat;

            var sekarang = DateTime.UtcNow;

            jurnal.JournalStatus = JournalStatus.PendingApproval;
            jurnal.SubmittedBy = actorUserId;
            jurnal.SubmittedAt = sekarang;
            jurnal.RejectionReason = null;
            jurnal.UpdateDateTime = sekarang;
            jurnal.UpdateBy = actorUserId;

            CatatRiwayat(jurnal, JournalApprovalAction.Submitted, actorUserId, null, sekarang);

            await _db.SaveChangesAsync(ct);

            return await MuatDanPetakanAsync(jurnal.Id, "Jurnal berhasil diajukan.", actorUserId, izin, ct);
        }

        /// <summary>
        /// <c>PendingApproval</c> → <c>Approved</c>.
        /// </summary>
        /// <remarks>
        /// Aturan pembuat-bukan-penyetuju (<c>ACC-DEC-016</c>) ditegakkan di sini, **tanpa
        /// pengecualian** — termasuk bagi pengguna yang berhak penuh, dan termasuk bila pembuatnya
        /// kebetulan satu-satunya penyetuju yang tersedia. Itu sebabnya ia `403` dan bukan `409`:
        /// yang ditolak adalah orangnya, bukan keadaan jurnalnya.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> ApproveAsync(
            Guid id,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<JournalDetailResponse>();

            if (jurnal.JournalStatus == JournalStatus.Rejected)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Jurnal yang ditolak harus diperbaiki dan diajukan kembali.");
            }

            if (jurnal.JournalStatus != JournalStatus.PendingApproval)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jurnal {jurnal.JournalNumber} tidak sedang menunggu persetujuan.");
            }

            var sendiri = PeriksaBukanJurnalSendiri<JournalDetailResponse>(jurnal, actorUserId);
            if (sendiri is not null) return sendiri;

            var sekarang = DateTime.UtcNow;

            jurnal.JournalStatus = JournalStatus.Approved;
            jurnal.ApprovedBy = actorUserId;
            jurnal.ApprovedAt = sekarang;
            jurnal.UpdateDateTime = sekarang;
            jurnal.UpdateBy = actorUserId;

            CatatRiwayat(jurnal, JournalApprovalAction.Approved, actorUserId, null, sekarang);

            await _db.SaveChangesAsync(ct);

            return await MuatDanPetakanAsync(jurnal.Id, "Jurnal berhasil disetujui.", actorUserId, izin, ct);
        }

        /// <summary>
        /// <c>PendingApproval</c> atau <c>Approved</c> → <c>Rejected</c>. Alasan wajib.
        /// </summary>
        /// <remarks>
        /// `ACC-STATE-0.1` bagian 1.1 mengizinkan penolakan dari dua status: penyetuju menolak
        /// jurnal yang menunggu, dan Manajer menolak jurnal yang sudah disetujui tetapi ternyata
        /// keliru saat hendak disahkan.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> RejectAsync(
            Guid id,
            RejectJournalRequest request,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<JournalDetailResponse>();

            if (jurnal.JournalStatus is not (JournalStatus.PendingApproval or JournalStatus.Approved))
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jurnal {jurnal.JournalNumber} tidak dalam keadaan yang dapat ditolak.");
            }

            var alasan = request.Reason?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(alasan) || alasan.Length > 500)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penolakan wajib diisi.");
            }

            var sekarang = DateTime.UtcNow;

            jurnal.JournalStatus = JournalStatus.Rejected;
            jurnal.RejectionReason = alasan;
            jurnal.UpdateDateTime = sekarang;
            jurnal.UpdateBy = actorUserId;

            CatatRiwayat(jurnal, JournalApprovalAction.Rejected, actorUserId, alasan, sekarang);

            await _db.SaveChangesAsync(ct);

            return await MuatDanPetakanAsync(jurnal.Id, "Jurnal ditolak.", actorUserId, izin, ct);
        }

        /// <summary>
        /// <c>Approved</c> → <c>Posted</c>. Titik tidak dapat kembali.
        /// </summary>
        /// <remarks>
        /// Sesudah ini jurnal menjadi riwayat permanen: tidak dapat diubah, tidak dapat dihapus,
        /// dan koreksinya hanya lewat pembalikan (<c>ACC-DEC-006</c>, <c>ACC-DEC-015</c>). Karena
        /// itu kesembilan syarat diperiksa **ulang** di sini, bukan dipercayakan pada pemeriksaan
        /// saat pengajuan.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> PostAsync(
            Guid id,
            Guid actorUserId,
            JournalActorPermissions? izin = null,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jurnal = await MuatLengkapAsync(id, lacak: true, ct);
            if (jurnal is null) return TidakDitemukan<JournalDetailResponse>();

            var status = jurnal.JournalStatus switch
            {
                JournalStatus.Draft => "Jurnal harus diajukan dan disetujui lebih dahulu.",
                JournalStatus.PendingApproval => "Jurnal belum disetujui.",
                JournalStatus.Rejected => "Jurnal yang ditolak harus diperbaiki dan diajukan kembali.",
                JournalStatus.Posted => $"Jurnal {jurnal.JournalNumber} sudah disahkan.",
                _ => null
            };

            if (status is not null)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status409Conflict, status);
            }

            // Diperiksa ulang — syarat 9 khususnya. Periode dapat ditutup sesudah persetujuan.
            var syarat = await PeriksaSembilanSyaratAsync<JournalDetailResponse>(jurnal, ct);
            if (syarat is not null) return syarat;

            var sekarang = DateTime.UtcNow;

            jurnal.JournalStatus = JournalStatus.Posted;
            jurnal.PostedBy = actorUserId;
            jurnal.PostedAt = sekarang;
            jurnal.UpdateDateTime = sekarang;
            jurnal.UpdateBy = actorUserId;

            CatatRiwayat(jurnal, JournalApprovalAction.Posted, actorUserId, null, sekarang);

            await _db.SaveChangesAsync(ct);

            return await MuatDanPetakanAsync(jurnal.Id, "Jurnal berhasil disahkan.", actorUserId, izin, ct);
        }

        /// <summary>
        /// Kesembilan syarat pengajuan `ACC-STATE-0.1` bagian 1.3, dinilai dari keadaan
        /// <b>tersimpan</b>, bukan dari isian permintaan.
        /// </summary>
        /// <remarks>
        /// Sengaja membaca ulang dari database dan tidak memercayai <c>TotalDebit</c> maupun
        /// <c>TotalCredit</c> pada kepala jurnal: keduanya salinan untuk mempercepat tampilan
        /// daftar, dan `AccJournal` sendiri menyebutnya bukan sumber kebenaran.
        /// </remarks>
        private async Task<AccountingServiceResult<T>?> PeriksaSembilanSyaratAsync<T>(
            AccJournal jurnal,
            CancellationToken ct)
        {
            var baris = jurnal.Lines.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber).ToList();

            // Syarat 2 — sekurang-kurangnya dua baris.
            if (baris.Count < 2)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Jurnal harus memiliki sekurang-kurangnya dua baris.");
            }

            var idAkun = baris.Select(x => x.AccountId).Distinct().ToList();

            var akunTersedia = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => idAkun.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var idUnitBiaya = baris
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var unitBiayaTersedia = await _db.Set<MstCostCenter>()
                .AsNoTracking()
                .Where(x => idUnitBiaya.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            foreach (var b in baris)
            {
                var n = b.LineNumber;

                // Syarat 3 — tepat satu sisi terisi dan lebih besar dari nol.
                if ((b.DebitAmount > 0m) == (b.CreditAmount > 0m)
                    || b.DebitAmount < 0m
                    || b.CreditAmount < 0m)
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: isi salah satu saja, debit atau kredit, dan nilainya "
                        + "harus lebih dari nol.");
                }

                if (!akunTersedia.TryGetValue(b.AccountId, out var akun))
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif.");
                }

                // Syarat 4 — akun aktif. Dapat berubah sesudah draft tersimpan.
                if (!akun.IsActive)
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun {akun.AccountCode} sudah tidak aktif.");
                }

                // Syarat 5 — akun menerima transaksi.
                if (!akun.IsPostable)
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} adalah akun induk dan tidak "
                        + "dapat menerima transaksi.");
                }

                // Syarat 6 — akun milik badan hukum jurnalnya.
                if (akun.LegalEntityId != jurnal.LegalEntityId)
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} bukan milik badan hukum jurnal ini.");
                }

                // Syarat 7 — akun beban wajib menyebutkan Cost Center.
                if (akun.AccountType == AccountType.Expense && !b.CostCenterId.HasValue)
                {
                    return AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun beban {akun.AccountCode} wajib menyebutkan unit biaya.");
                }

                // Syarat 8 — Cost Center aktif dan milik badan hukum yang sama.
                if (b.CostCenterId.HasValue)
                {
                    if (!unitBiayaTersedia.TryGetValue(b.CostCenterId.Value, out var unitBiaya)
                        || !unitBiaya.IsActive
                        || unitBiaya.LegalEntityId != jurnal.LegalEntityId)
                    {
                        return AccountingServiceResult<T>.Fail(
                            StatusCodes.Status409Conflict,
                            $"Baris ke-{n}: unit biaya tidak aktif atau bukan milik badan hukum "
                            + "jurnal ini.");
                    }
                }
            }

            // Syarat 1 — keseimbangan, dihitung ulang dari baris.
            var debit = baris.Sum(x => x.DebitAmount);
            var kredit = baris.Sum(x => x.CreditAmount);

            if (debit != kredit)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Jurnal belum seimbang. Total debit Rp {debit:N0}, total kredit "
                    + $"Rp {kredit:N0}, selisih Rp {Math.Abs(debit - kredit):N0}.");
            }

            // Syarat 9 — periode menerima jenis jurnal ini. Diperiksa dua kali secara sengaja.
            var kodeJenis = jurnal.JournalType?.JournalTypeCode
                ?? await _db.Set<AccJournalType>()
                    .AsNoTracking()
                    .Where(x => x.Id == jurnal.JournalTypeId)
                    .Select(x => x.JournalTypeCode)
                    .FirstOrDefaultAsync(ct)
                ?? string.Empty;

            var alasanPeriode = await AccAccountingPeriodService.AlasanPenolakanJenisJurnalAsync(
                _db, jurnal.AccountingPeriodId, kodeJenis, ct);

            if (alasanPeriode is not null)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status422UnprocessableEntity, alasanPeriode);
            }

            return null;
        }

        /// <remarks>
        /// <c>ACC-DEC-016</c> tanpa pengecualian. Dipisah menjadi method tersendiri supaya
        /// `BE-ACC-013` memakai aturan yang sama persis, bukan menyalinnya.
        /// </remarks>
        private static AccountingServiceResult<T>? PeriksaBukanJurnalSendiri<T>(
            AccJournal jurnal,
            Guid actorUserId)
        {
            return jurnal.CreateBy == actorUserId
                ? AccountingServiceResult<T>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri.")
                : null;
        }

        /// <summary>
        /// Menambahkan satu baris riwayat. Baris ini <b>tidak pernah</b> diubah maupun dihapus.
        /// </summary>
        private void CatatRiwayat(
            AccJournal jurnal,
            JournalApprovalAction aksi,
            Guid actorUserId,
            string? alasan,
            DateTime sekarang)
        {
            _db.Set<AccJournalApproval>().Add(new AccJournalApproval
            {
                Id = Guid.NewGuid(),
                JournalId = jurnal.Id,
                ApprovalAction = aksi,
                ActionBy = actorUserId,
                ActionAt = sekarang,
                Reason = alasan,
                CreateDateTime = sekarang,
                CreateBy = actorUserId
            });
        }

        private async Task<AccountingServiceResult<JournalDetailResponse>> MuatDanPetakanAsync(
            Guid id,
            string pesan,
            Guid actorUserId,
            JournalActorPermissions? izin,
            CancellationToken ct)
        {
            var tersimpan = await MuatLengkapAsync(id, lacak: false, ct);

            return AccountingServiceResult<JournalDetailResponse>.Ok(
                await PetakanRincianAsync(tersimpan!, actorUserId, izin, ct), pesan);
        }

        // ------------------------------------------------------------------
        // Penomoran
        // ------------------------------------------------------------------

        /// <summary>
        /// Mengalokasikan satu nomor jurnal berbentuk <c>{prefix}/{yyyy}/{MM}/{00001}</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Mekanisme yang terkunci roadmap.</b> Alokasi memakai
        /// <c>pg_advisory_xact_lock(hashtext(kunci))</c> yang diambil di dalam transaction, lalu
        /// menambah <see cref="AccNumberSeries.CurrentValue"/> pada baris ber-
        /// <c>(SequenceKey, ScopeKey)</c>. Ini pola repository yang sudah terbukti — lihat
        /// <c>BillingNumberSeriesService</c> — dan bukan <c>Count+1</c>, <c>Max+1</c>, counter
        /// statis, maupun lock tingkat aplikasi, yang ketiganya dilarang <c>QBE-CODE-003</c>.
        /// </para>
        /// <para>
        /// Lock ini dipegang <b>database</b> dan ber-scope pada kunci nomor, sehingga ia tetap
        /// benar walau aplikasi berjalan lebih dari satu instance. Ia lepas sendiri saat
        /// transaction berakhir, jadi tidak ada lock yang tertinggal bila permintaan gagal.
        /// </para>
        /// <para>
        /// <b>Pembagian kunci.</b> <c>SequenceKey</c> memuat awalan jenis jurnal dan
        /// <c>ScopeKey</c> memuat badan hukum beserta bulan akuntansinya. Dengan begitu
        /// <c>JU/2026/09/00001</c> dan <c>JP/2026/09/00001</c> dapat hidup berdampingan — keduanya
        /// nomor yang berbeda — sementara dua jurnal <c>JU</c> pada badan hukum dan bulan yang
        /// sama tidak akan pernah memperoleh urutan yang sama.
        /// </para>
        /// <para>
        /// <b>Bulannya diambil dari <see cref="AccJournal.AccountingDate"/></b>, bukan dari waktu
        /// permintaan. Nomor jurnal memuat bulan di dalam dirinya, sehingga jurnal bertanggal
        /// akuntansi September yang disusun pada Oktober tetap bernomor September.
        /// </para>
        /// <para>
        /// Nomor terlewat diizinkan (<c>ACC-DEC-014</c>): bila transaction gagal setelah nomor
        /// dialokasikan, penambahan <c>CurrentValue</c> ikut dibatalkan pada rollback, tetapi
        /// urutan tetap boleh berlubang tanpa dianggap cacat. Nomor kembar tidak pernah
        /// diizinkan, dan unique index <c>(LegalEntityId, JournalNumber)</c> menjadi jaring
        /// terakhirnya.
        /// </para>
        /// <para>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya
        /// pembalikan jurnal pada `BE-ACC-013` memakai alokator yang sama tanpa registrasi DI
        /// baru, sesuai <c>02-backend-architecture.md</c> bagian 6.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Bila dipanggil di luar transaction pada penyedia relasional. Alokasi tanpa transaction
        /// tidak dapat dijamin atomik, dan gagal keras lebih baik daripada diam-diam menghasilkan
        /// nomor kembar.
        /// </exception>
        public static async Task<string> AlokasikanNomorJurnalAsync(
            ApplicationDbContext db,
            string numberPrefix,
            Guid legalEntityId,
            DateTime accountingDate,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var awalan = numberPrefix.Trim().ToUpperInvariant();
            var tanggal = accountingDate.Date;

            var sequenceKey = AwalanKunciDeret + awalan;
            var scopeKey = $"{legalEntityId:N}_{tanggal.ToString("yyyyMM", CultureInfo.InvariantCulture)}";

            if (db.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException(
                    "Alokasi nomor jurnal wajib berada di dalam transaction.");
            }

            // pg_advisory_xact_lock hanya ada di PostgreSQL. Pada penyedia lain — SQLite yang
            // dipakai sebagian test — alokasi tetap berjalan di dalam transaction, tetapi
            // kebenarannya saat berbarengan hanya dapat dibuktikan pada PostgreSQL sungguhan.
            // Itulah sebabnya acceptance (3) menuntut test integrasi terhadap database nyata.
            if (db.Database.IsNpgsql())
            {
                var kunci = $"ACC_NUMBER_{sequenceKey}_{scopeKey}";
                await db.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));", [kunci], ct);
            }

            var deret = await db.Set<AccNumberSeries>()
                .FirstOrDefaultAsync(x => x.SequenceKey == sequenceKey && x.ScopeKey == scopeKey, ct);

            if (deret is null)
            {
                deret = new AccNumberSeries
                {
                    Id = Guid.NewGuid(),
                    SequenceKey = sequenceKey,
                    ScopeKey = scopeKey,
                    ResetPolicy = KebijakanReset,
                    CurrentValue = 1,
                    LastAllocatedAt = DateTimeOffset.UtcNow,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };

                db.Set<AccNumberSeries>().Add(deret);
            }
            else
            {
                checked { deret.CurrentValue++; }
                deret.LastAllocatedAt = DateTimeOffset.UtcNow;
                deret.UpdateDateTime = DateTime.UtcNow;
                deret.UpdateBy = actorUserId;
            }

            var urutan = deret.CurrentValue.ToString($"D{LebarUrutan}", CultureInfo.InvariantCulture);

            return $"{awalan}/{tanggal.ToString("yyyy", CultureInfo.InvariantCulture)}"
                 + $"/{tanggal.ToString("MM", CultureInfo.InvariantCulture)}/{urutan}";
        }

        // ------------------------------------------------------------------
        // Pemeriksaan isian — ACC-VALIDATION-0.2 bagian 3
        // ------------------------------------------------------------------

        private sealed class HasilPersiapan<T>
        {
            public AccountingServiceResult<T>? Gagal { get; init; }

            public AccJournalType? JenisJurnal { get; init; }

            public AccAccountingPeriod? Periode { get; init; }

            public List<AccJournalLine> Baris { get; init; } = new();
        }

        /// <summary>
        /// Memeriksa seluruh isian dan menyusun baris jurnal yang siap disimpan. Dipakai bersama
        /// oleh <see cref="CreateAsync"/> dan <see cref="UpdateAsync"/> supaya aturan yang sama
        /// tidak ditulis dua kali dengan pesan yang berbeda-beda.
        /// </summary>
        private async Task<HasilPersiapan<T>> SiapkanAsync<T>(
            Guid legalEntityId,
            Guid journalTypeId,
            DateTime accountingDate,
            string? description,
            List<CreateJournalLineRequest> lines,
            CancellationToken ct)
        {
            var jenis = journalTypeId == Guid.Empty
                ? null
                : await _db.Set<AccJournalType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == journalTypeId && !x.IsDelete && x.IsActive, ct);

            if (jenis is null) return Tolak<T>(StatusCodes.Status400BadRequest, "Jenis jurnal wajib dipilih.");

            if (accountingDate == default)
                return Tolak<T>(StatusCodes.Status400BadRequest, "Tanggal akuntansi wajib diisi.");

            var keterangan = description?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keterangan) || keterangan.Length > 500)
            {
                return Tolak<T>(
                    StatusCodes.Status400BadRequest,
                    "Keterangan jurnal wajib diisi dan maksimal 500 karakter.");
            }

            var tanggal = accountingDate.Date;

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete
                         && x.LegalEntityId == legalEntityId
                         && x.StartDate <= tanggal
                         && x.EndDate >= tanggal, ct);

            if (periode is null)
            {
                return Tolak<T>(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Belum ada periode akuntansi untuk {NamaBulan[tanggal.Month - 1]} "
                    + $"{tanggal.Year.ToString(CultureInfo.InvariantCulture)}. "
                    + "Minta administrator membangkitkan periode tahun buku ini.");
            }

            // Aturan status periode dipakai ulang dari `BE-ACC-009` — bentuk tunggalnya ada di
            // AccAccountingPeriodService, sehingga Accounting tidak punya dua tafsir yang
            // berbeda atas status periode. Lihat catatan ACC-TD-014: menolak sejak penyimpanan
            // draft adalah pemeriksaan yang LEBIH AWAL daripada yang didaftar
            // `ACC-VALIDATION-0.2` bagian 3, dan menunggu ratifikasi owner. `BE-ACC-011` tetap
            // wajib memeriksanya ulang saat pengajuan dan pengesahan, karena periode dapat
            // berubah status setelah draft tersimpan.
            var alasanPeriode = await AccAccountingPeriodService.AlasanPenolakanJenisJurnalAsync(
                _db, periode.Id, jenis.JournalTypeCode, ct);

            if (alasanPeriode is not null)
                return Tolak<T>(StatusCodes.Status422UnprocessableEntity, alasanPeriode);

            var barisGagal = await SusunBarisAsync<T>(legalEntityId, lines, ct);
            if (barisGagal.Gagal is not null) return barisGagal;

            return new HasilPersiapan<T>
            {
                JenisJurnal = jenis,
                Periode = periode,
                Baris = barisGagal.Baris
            };
        }

        /// <remarks>
        /// Pesan penolakan selalu menyebut <b>nomor baris</b>, sesuai ketentuan kontrak, supaya
        /// petugas tidak perlu menebak baris mana yang salah.
        /// </remarks>
        private async Task<HasilPersiapan<T>> SusunBarisAsync<T>(
            Guid legalEntityId,
            List<CreateJournalLineRequest> lines,
            CancellationToken ct)
        {
            var permintaan = lines ?? new List<CreateJournalLineRequest>();

            if (permintaan.Select(x => x.LineNumber).Distinct().Count() != permintaan.Count)
                return Tolak<T>(StatusCodes.Status400BadRequest, "Nomor baris tidak boleh kembar.");

            var idAkun = permintaan.Select(x => x.AccountId).Distinct().ToList();

            var akunTersedia = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => idAkun.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var idUnitBiaya = permintaan
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var unitBiayaTersedia = await _db.Set<MstCostCenter>()
                .AsNoTracking()
                .Where(x => idUnitBiaya.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var hasil = new List<AccJournalLine>();

            foreach (var baris in permintaan.OrderBy(x => x.LineNumber))
            {
                var n = baris.LineNumber;

                // Nilai negatif diperiksa lebih dahulu daripada aturan satu sisi. Keduanya akan
                // sama-sama menolak baris bernilai negatif, tetapi hanya pesan inilah yang
                // memberi tahu petugas cara memperbaikinya.
                if (baris.DebitAmount < 0m || baris.CreditAmount < 0m)
                {
                    return Tolak<T>(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: nilai tidak boleh negatif. Untuk membalik arah, "
                        + "pindahkan ke sisi sebaliknya.");
                }

                var debitTerisi = baris.DebitAmount > 0m;
                var kreditTerisi = baris.CreditAmount > 0m;

                if (debitTerisi == kreditTerisi)
                {
                    return Tolak<T>(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: isi salah satu saja, debit atau kredit, dan nilainya "
                        + "harus lebih dari nol.");
                }

                if (!akunTersedia.TryGetValue(baris.AccountId, out var akun) || !akun.IsActive)
                {
                    return Tolak<T>(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif.");
                }

                if (!akun.IsPostable)
                {
                    return Tolak<T>(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} adalah akun induk dan tidak "
                        + "dapat menerima transaksi.");
                }

                if (akun.LegalEntityId != legalEntityId)
                {
                    return Tolak<T>(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} bukan milik badan hukum jurnal ini.");
                }

                if (akun.AccountType == AccountType.Expense && !baris.CostCenterId.HasValue)
                {
                    return Tolak<T>(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun beban {akun.AccountCode} wajib menyebutkan unit biaya.");
                }

                if (baris.CostCenterId.HasValue)
                {
                    if (!unitBiayaTersedia.TryGetValue(baris.CostCenterId.Value, out var unitBiaya)
                        || !unitBiaya.IsActive
                        || unitBiaya.LegalEntityId != legalEntityId)
                    {
                        return Tolak<T>(
                            StatusCodes.Status409Conflict,
                            $"Baris ke-{n}: unit biaya tidak aktif atau bukan milik badan hukum "
                            + "jurnal ini.");
                    }
                }

                hasil.Add(new AccJournalLine
                {
                    Id = Guid.NewGuid(),
                    LineNumber = n,
                    AccountId = baris.AccountId,
                    CostCenterId = baris.CostCenterId,
                    Description = baris.Description?.Trim(),
                    DebitAmount = baris.DebitAmount,
                    CreditAmount = baris.CreditAmount
                });
            }

            return new HasilPersiapan<T> { Baris = hasil };
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private static HasilPersiapan<T> Tolak<T>(int statusCode, string pesan)
            => new() { Gagal = AccountingServiceResult<T>.Fail(statusCode, pesan) };

        private static AccountingServiceResult<T> TidakDitemukan<T>()
            => AccountingServiceResult<T>.Fail(StatusCodes.Status404NotFound, "Jurnal tidak ditemukan.");

        /// <summary>
        /// Menolak penyuntingan menurut status, dengan pesan yang tepat per status.
        /// </summary>
        /// <remarks>
        /// Pesannya diambil kata demi kata dari `ACC-STATE-0.1` bagian 1.2. Pesan yang berbeda
        /// per status bukan kemewahan: "tidak dapat diubah" saja membuat petugas menebak apakah
        /// jurnalnya perlu ditolak dulu, sudah disahkan, atau sedang dinilai orang lain.
        ///
        /// <c>Rejected</c> sengaja **boleh** disunting: `ACC-STATE-0.1` bagian 1.1 menyediakan
        /// perpindahan <c>Rejected</c> → <c>Draft</c> lewat penyuntingan oleh pembuatnya.
        /// </remarks>
        private static AccountingServiceResult<T>? PeriksaDapatDisunting<T>(
            AccJournal jurnal,
            bool untukPenghapusan)
        {
            if (jurnal.JournalStatus is JournalStatus.Draft or JournalStatus.Rejected)
            {
                // Jurnal yang ditolak boleh disunting, tetapi tidak boleh dihapus — riwayat
                // penolakannya adalah jejak audit yang sudah terbentuk.
                if (!untukPenghapusan || jurnal.JournalStatus == JournalStatus.Draft) return null;

                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status409Conflict,
                    "Jurnal yang sudah pernah ditolak tidak dapat dihapus. Perbaiki lalu ajukan kembali.");
            }

            var pesan = (jurnal.JournalStatus, untukPenghapusan) switch
            {
                (JournalStatus.Posted, false) =>
                    "Jurnal yang sudah disahkan tidak dapat diubah. Gunakan pembalikan atau "
                    + "jurnal penyesuaian.",
                (JournalStatus.Posted, true) =>
                    "Jurnal yang sudah disahkan tidak dapat dihapus.",
                (JournalStatus.PendingApproval, _) =>
                    "Jurnal sedang menunggu persetujuan dan tidak dapat diubah.",
                (JournalStatus.Approved, _) =>
                    "Jurnal sudah disetujui dan tidak dapat diubah. Minta penolakan lebih dahulu "
                    + "bila perlu diperbaiki.",
                _ => $"Jurnal {jurnal.JournalNumber} tidak dapat diubah pada status ini."
            };

            return AccountingServiceResult<T>.Fail(StatusCodes.Status409Conflict, pesan);
        }

        private Task<AccJournal?> MuatLengkapAsync(Guid id, bool lacak, CancellationToken ct)
        {
            IQueryable<AccJournal> q = _db.Set<AccJournal>()
                .Include(x => x.JournalType)
                .Include(x => x.AccountingPeriod)
                .Include(x => x.ReversalOfJournal)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.Account)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.CostCenter);

            if (!lacak) q = q.AsNoTracking();

            return q.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        }

        private async Task<JournalDetailResponse> PetakanRincianAsync(
            AccJournal jurnal,
            Guid actorUserId,
            JournalActorPermissions? izin,
            CancellationToken ct)
        {
            // Jurnal yang sudah pernah dibalik tidak boleh dibalik lagi (`ACC-DEC-006`), jadi
            // `reverse` pun tidak boleh muncul sebagai tindakan yang tersedia.
            var sudahDibalik = await _db.Set<AccJournal>()
                .AsNoTracking()
                .AnyAsync(x => x.ReversalOfJournalId == jurnal.Id && !x.IsDelete, ct);

            var riwayat = await _db.Set<AccJournalApproval>()
                .AsNoTracking()
                .Where(x => x.JournalId == jurnal.Id && !x.IsDelete)
                .OrderBy(x => x.ActionAt)
                .Select(x => new JournalApprovalResponse
                {
                    ApprovalAction = x.ApprovalAction,
                    ActionBy = x.ActionBy,
                    ActionAt = x.ActionAt,
                    Reason = x.Reason
                })
                .ToListAsync(ct);

            return new JournalDetailResponse
            {
                Id = jurnal.Id,
                LegalEntityId = jurnal.LegalEntityId,
                JournalNumber = jurnal.JournalNumber,
                JournalTypeId = jurnal.JournalTypeId,
                JournalTypeCode = jurnal.JournalType?.JournalTypeCode ?? string.Empty,
                JournalTypeName = jurnal.JournalType?.JournalTypeName ?? string.Empty,
                AccountingPeriodId = jurnal.AccountingPeriodId,
                PeriodCode = jurnal.AccountingPeriod?.PeriodCode ?? string.Empty,
                DocumentNumber = jurnal.DocumentNumber,
                DocumentDate = jurnal.DocumentDate,
                AccountingDate = jurnal.AccountingDate,
                Description = jurnal.Description,
                JournalStatus = jurnal.JournalStatus,
                TotalDebit = jurnal.TotalDebit,
                TotalCredit = jurnal.TotalCredit,
                IsBalanced = jurnal.TotalDebit == jurnal.TotalCredit && jurnal.Lines.Count > 0,
                SubmittedBy = jurnal.SubmittedBy,
                SubmittedAt = jurnal.SubmittedAt,
                ApprovedBy = jurnal.ApprovedBy,
                ApprovedAt = jurnal.ApprovedAt,
                PostedBy = jurnal.PostedBy,
                PostedAt = jurnal.PostedAt,
                RejectionReason = jurnal.RejectionReason,
                ReversalOfJournalId = jurnal.ReversalOfJournalId,
                ReversalOfJournalNumber = jurnal.ReversalOfJournal?.JournalNumber,
                CorrectionType = jurnal.CorrectionType,
                CreateDateTime = jurnal.CreateDateTime,
                CreateBy = jurnal.CreateBy,
                Lines = jurnal.Lines
                    .OrderBy(x => x.LineNumber)
                    .Select(x => new JournalLineResponse
                    {
                        Id = x.Id,
                        LineNumber = x.LineNumber,
                        AccountId = x.AccountId,
                        AccountCode = x.Account?.AccountCode ?? string.Empty,
                        AccountName = x.Account?.AccountName ?? string.Empty,
                        CostCenterId = x.CostCenterId,
                        CostCenterName = x.CostCenter?.CostCenterName,
                        Description = x.Description,
                        DebitAmount = x.DebitAmount,
                        CreditAmount = x.CreditAmount
                    })
                    .ToList(),
                Approvals = riwayat,
                AvailableActions = TindakanTersedia(
                    jurnal, actorUserId, izin ?? JournalActorPermissions.Kosong, sudahDibalik)
            };
        }

        /// <summary>
        /// Acceptance (6) `BE-ACC-011` — status, hak akses, dan aturan pembuat-bukan-penyetuju
        /// digabung menjadi satu daftar.
        /// </summary>
        /// <remarks>
        /// Ketiganya harus terpenuhi sekaligus. Sebuah tindakan hanya muncul bila status jurnal
        /// mengizinkannya (`ACC-STATE-0.1` bagian 1.1), pengguna memegang hak aksesnya, dan —
        /// khusus <c>approve</c> — pengguna bukan pembuat jurnalnya (<c>ACC-DEC-016</c>).
        ///
        /// <c>reject</c> sengaja **tidak** ikut dibatasi aturan pembuat: menolak jurnal sendiri
        /// tidak berbahaya, dan itu jalan keluar yang wajar ketika pembuatnya sendiri menyadari
        /// jurnalnya keliru sesudah diajukan.
        /// </remarks>
        private static List<string> TindakanTersedia(
            AccJournal jurnal,
            Guid actorUserId,
            JournalActorPermissions izin,
            bool sudahDibalik)
        {
            var tindakan = new List<string>();
            var pembuatnyaSendiri = jurnal.CreateBy == actorUserId;

            switch (jurnal.JournalStatus)
            {
                case JournalStatus.Draft:
                    if (izin.CanUpdate) tindakan.Add("update");
                    if (izin.CanDelete) tindakan.Add("delete");
                    if (izin.CanSubmit) tindakan.Add("submit");
                    break;

                case JournalStatus.Rejected:
                    // Diperbaiki lalu diajukan kembali. Tidak dapat dihapus.
                    if (izin.CanUpdate) tindakan.Add("update");
                    if (izin.CanSubmit) tindakan.Add("submit");
                    break;

                case JournalStatus.PendingApproval:
                    if (izin.CanApprove && !pembuatnyaSendiri) tindakan.Add("approve");
                    if (izin.CanApprove) tindakan.Add("reject");
                    break;

                case JournalStatus.Approved:
                    if (izin.CanPost) tindakan.Add("post");
                    if (izin.CanApprove) tindakan.Add("reject");
                    break;

                case JournalStatus.Posted:
                    if (izin.CanReverse && !sudahDibalik) tindakan.Add("reverse");
                    break;
            }

            return tindakan;
        }
    }
}
