using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services
{
    /// <summary>
    /// Template jurnal berulang: penyimpanan, penyuntingan, dan pengaktifannya
    /// (<c>ACC-DEC-050</c>). Cakupan <c>BE-ACC-P2-007</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seluruh aturan <c>ACC-VALIDATION-0.6</c> bagian 3 yang berlaku pada penyimpanan ditegakkan
    /// di sini, bukan di controller, supaya jalur tulis mana pun melewatinya.
    /// </para>
    /// <para>
    /// <b>Berbeda dari jurnal manual, template TIDAK boleh disimpan timpang.</b>
    /// <c>ACC-DEC-025</c> mengizinkan draft jurnal timpang disimpan karena petugas memang
    /// menyusunnya bertahap dan akan menyelesaikannya sendiri. Template tidak begitu: ia disimpan
    /// sekali lalu menerbitkan jurnal <b>sendiri</b> setiap bulan tanpa ada yang menyusunnya
    /// ulang. Template timpang yang lolos akan menerbitkan draft timpang tiap bulan, dan
    /// masing-masing baru gagal jauh kemudian saat seseorang mencoba mengajukannya.
    /// </para>
    /// <para>
    /// <b>Penerbitan jurnalnya sendiri adalah <c>BE-ACC-P2-008</c></b>, bukan berkas ini.
    /// </para>
    /// </remarks>
    public class AccRecurringJournalService
    {
        private readonly ApplicationDbContext _db;
        private readonly AccJournalService _journalService;

        public AccRecurringJournalService(ApplicationDbContext db, AccJournalService journalService)
        {
            _db = db;
            _journalService = journalService;
        }

        /// <summary>Baris minimum satu template — sebuah jurnal selalu punya dua sisi.</summary>
        private const int BarisMinimum = 2;

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<RecurringJournalListResponse>>> GetPagedAsync(
            RecurringJournalPagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<RecurringJournalListResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccRecurringJournalTemplate> q = _db.Set<AccRecurringJournalTemplate>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.JournalTypeId.HasValue)
                q = q.Where(x => x.JournalTypeId == query.JournalTypeId.Value);

            if (query.IsActive.HasValue)
                q = q.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.TemplateCode.ToLower().Contains(cari)
                              || x.TemplateName.ToLower().Contains(cari));
            }

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "templatename" => menurun ? q.OrderByDescending(x => x.TemplateName) : q.OrderBy(x => x.TemplateName),
                "dayofmonth" => menurun ? q.OrderByDescending(x => x.DayOfMonth) : q.OrderBy(x => x.DayOfMonth),
                "isactive" => menurun ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive),
                "createdatetime" => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => menurun ? q.OrderByDescending(x => x.TemplateCode) : q.OrderBy(x => x.TemplateCode)
            };

            var total = await q.CountAsync(ct);

            // Ringkasan baris dan penerbitan diproyeksikan di dalam satu query, bukan dimuat
            // sebagai graf navigation lalu dijumlahkan di memori — daftar template dibuka jauh
            // lebih sering daripada rinciannya.
            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RecurringJournalListResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    TemplateCode = x.TemplateCode,
                    TemplateName = x.TemplateName,
                    JournalTypeId = x.JournalTypeId,
                    JournalTypeCode = x.JournalType != null ? x.JournalType.JournalTypeCode : string.Empty,
                    JournalTypeName = x.JournalType != null ? x.JournalType.JournalTypeName : string.Empty,
                    Frequency = x.Frequency,
                    DayOfMonth = x.DayOfMonth,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive,
                    LineCount = x.Lines.Count(l => !l.IsDelete),
                    TotalAmount = x.Lines.Where(l => !l.IsDelete).Sum(l => l.DebitAmount),
                    RunCount = x.Runs.Count(r => !r.IsDelete)
                })
                .ToListAsync(ct);

            var hasil = new PagedResult<RecurringJournalListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };

            return AccountingServiceResult<PagedResult<RecurringJournalListResponse>>.Ok(
                hasil, "Daftar template jurnal berulang berhasil diambil.");
        }

        public async Task<AccountingServiceResult<RecurringJournalDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var template = await MuatLengkapAsync(id, lacak: false, ct);

            return template is null
                ? TidakDitemukan<RecurringJournalDetailResponse>()
                : AccountingServiceResult<RecurringJournalDetailResponse>.Ok(
                    Petakan(template), "Rincian template berhasil diambil.");
        }

        /// <summary>
        /// Riwayat penerbitan template per periode beserta jurnal yang dihasilkannya.
        /// </summary>
        /// <remarks>
        /// Inilah layar yang menjawab <i>"penyusutan bulan Agustus sudah terbit belum"</i>. Tanpa
        /// endpoint ini barisnya tersimpan tetapi tidak pernah terlihat, dan satu-satunya cara
        /// mengetahuinya adalah membaca tabel langsung.
        /// </remarks>
        public async Task<AccountingServiceResult<List<RecurringJournalRunResponse>>> GetRunsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<List<RecurringJournalRunResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var ada = await _db.Set<AccRecurringJournalTemplate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete, ct);

            if (!ada) return TidakDitemukan<List<RecurringJournalRunResponse>>();

            var runs = await _db.Set<AccRecurringJournalRun>()
                .AsNoTracking()
                .Where(x => x.TemplateId == id && !x.IsDelete)
                .OrderByDescending(x => x.GeneratedAt)
                .Select(x => new RecurringJournalRunResponse
                {
                    Id = x.Id,
                    TemplateId = x.TemplateId,
                    AccountingPeriodId = x.AccountingPeriodId,
                    PeriodCode = x.AccountingPeriod != null ? x.AccountingPeriod.PeriodCode : string.Empty,
                    JournalId = x.JournalId,
                    JournalNumber = x.Journal != null ? x.Journal.JournalNumber : string.Empty,
                    JournalStatus = x.Journal != null
                        ? x.Journal.JournalStatus
                        : JournalManagement.Enums.JournalStatus.Draft,
                    GeneratedAt = x.GeneratedAt
                })
                .ToListAsync(ct);

            return AccountingServiceResult<List<RecurringJournalRunResponse>>.Ok(
                runs,
                runs.Count == 0
                    ? "Template ini belum pernah menerbitkan jurnal."
                    : $"Riwayat penerbitan berhasil diambil, {runs.Count} periode.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        /// <remarks>
        /// Acceptance (1), (2), (3), dan (4) bertemu di method ini. Template yang lahir
        /// <b>selalu tidak aktif</b>, dan itu tidak dapat dilangkahi permintaan: bidangnya memang
        /// tidak ada pada <see cref="CreateRecurringJournalRequest"/>.
        /// </remarks>
        public async Task<AccountingServiceResult<RecurringJournalDetailResponse>> CreateAsync(
            CreateRecurringJournalRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (request.LegalEntityId == Guid.Empty)
            {
                return Gagal<RecurringJournalDetailResponse>(
                    StatusCodes.Status400BadRequest, "Badan hukum wajib dipilih.");
            }

            var siap = await SiapkanAsync(
                request.LegalEntityId,
                templateId: null,
                request.TemplateCode,
                request.TemplateName,
                request.JournalTypeId,
                request.DayOfMonth,
                request.StartDate,
                request.EndDate,
                request.Lines,
                ct);

            if (siap.Gagal is not null) return siap.Gagal;

            var sekarang = DateTime.UtcNow;

            var template = new AccRecurringJournalTemplate
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                TemplateCode = siap.KodeTemplate,
                TemplateName = request.TemplateName.Trim(),
                JournalTypeId = request.JournalTypeId,
                Frequency = request.Frequency,
                DayOfMonth = request.DayOfMonth,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate?.Date,

                // Acceptance (4). Ditulis eksplisit walaupun model sudah berbawaan false,
                // supaya niatnya terbaca di tempat template benar-benar dibuat.
                IsActive = false,

                CreateDateTime = sekarang,
                CreateBy = actorUserId
            };

            foreach (var baris in siap.Baris)
            {
                baris.TemplateId = template.Id;
                baris.CreateDateTime = sekarang;
                baris.CreateBy = actorUserId;
            }

            _db.Set<AccRecurringJournalTemplate>().Add(template);
            _db.Set<AccRecurringJournalTemplateLine>().AddRange(siap.Baris);

            await _db.SaveChangesAsync(ct);

            var tersimpan = await MuatLengkapAsync(template.Id, lacak: false, ct);

            return AccountingServiceResult<RecurringJournalDetailResponse>.Ok(
                Petakan(tersimpan!),
                $"Template {template.TemplateCode} berhasil disimpan. Template baru berstatus "
                + "tidak aktif; aktifkan setelah barisnya diperiksa.",
                StatusCodes.Status201Created);
        }

        /// <remarks>
        /// <para>
        /// Baris dikirim <b>utuh</b> dan menggantikan seluruh baris sebelumnya.
        /// </para>
        /// <para>
        /// <b>Jebakan EF yang sudah memakan waktu sekali dan sengaja dicatat di sini.</b>
        /// Mengganti baris anak dengan <c>RemoveRange</c> lalu menambah baris baru lewat
        /// <b>navigation yang terlacak</b> pada permintaan yang sama membuat EF mencocokkan baris
        /// baru dengan baris lama yang belum benar-benar terhapus, lalu mengirim <c>UPDATE</c>
        /// alih-alih <c>INSERT</c>. Akibatnya baris lama tertimpa dan <c>Id</c>-nya bertahan.
        /// Penangkalnya dua: hapus dan <c>SaveChanges</c> <b>lebih dahulu</b>, lalu tambahkan
        /// lewat <c>DbSet.AddRange</c> — bukan lewat navigation. Keduanya berada dalam satu
        /// transaction supaya template tidak pernah tertinggal tanpa baris.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<RecurringJournalDetailResponse>> UpdateAsync(
            Guid id,
            UpdateRecurringJournalRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var template = await MuatLengkapAsync(id, lacak: true, ct);
            if (template is null) return TidakDitemukan<RecurringJournalDetailResponse>();

            var siap = await SiapkanAsync(
                template.LegalEntityId,
                templateId: template.Id,
                request.TemplateCode,
                request.TemplateName,
                request.JournalTypeId,
                request.DayOfMonth,
                request.StartDate,
                request.EndDate,
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
                var barisLama = template.Lines.ToList();

                _db.Set<AccRecurringJournalTemplateLine>().RemoveRange(barisLama);
                template.Lines.Clear();

                // Penghapusan disimpan LEBIH DAHULU. Tanpa langkah ini, baris baru bernomor sama
                // akan dicocokkan EF dengan baris lama yang masih terlacak.
                await _db.SaveChangesAsync(ct);

                template.TemplateCode = siap.KodeTemplate;
                template.TemplateName = request.TemplateName.Trim();
                template.JournalTypeId = request.JournalTypeId;
                template.Frequency = request.Frequency;
                template.DayOfMonth = request.DayOfMonth;
                template.StartDate = request.StartDate.Date;
                template.EndDate = request.EndDate?.Date;
                template.UpdateDateTime = sekarang;
                template.UpdateBy = actorUserId;

                foreach (var baris in siap.Baris)
                {
                    baris.TemplateId = template.Id;
                    baris.CreateDateTime = sekarang;
                    baris.CreateBy = actorUserId;
                }

                // Lewat DbSet, bukan lewat template.Lines yang terlacak.
                _db.Set<AccRecurringJournalTemplateLine>().AddRange(siap.Baris);

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

            var tersimpan = await MuatLengkapAsync(template.Id, lacak: false, ct);

            return AccountingServiceResult<RecurringJournalDetailResponse>.Ok(
                Petakan(tersimpan!), $"Template {template.TemplateCode} berhasil diperbarui.");
        }

        /// <summary>
        /// Mengaktifkan template sehingga penjadwal mulai menerbitkannya.
        /// </summary>
        /// <remarks>
        /// <b>Barisnya diperiksa ULANG di sini, bukan dipercayakan pada pemeriksaan saat
        /// disimpan.</b> Akun dapat dinonaktifkan, unit biaya dapat dipindahkan, dan jenis
        /// jurnal dapat dimatikan <b>sesudah</b> template tersimpan. Template yang diaktifkan
        /// dalam keadaan itu akan gagal menerbitkan setiap bulan — dan gagalnya di dalam
        /// penjadwal, tempat tidak seorang pun melihatnya. Lebih baik ditolak sekarang, saat
        /// orangnya masih menatap layar.
        /// </remarks>
        public async Task<AccountingServiceResult<RecurringJournalDetailResponse>> ActivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var template = await MuatLengkapAsync(id, lacak: true, ct);
            if (template is null) return TidakDitemukan<RecurringJournalDetailResponse>();

            if (template.IsActive)
            {
                return Gagal<RecurringJournalDetailResponse>(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} sudah aktif.");
            }

            var alasan = await AlasanTidakLayakTerbitAsync(template, ct);

            if (alasan is not null)
            {
                return Gagal<RecurringJournalDetailResponse>(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} tidak dapat diaktifkan. {alasan}");
            }

            // ACC-DEC-073 butir (2): menangkap template yang disimpan SEBELUM akunnya ditandai
            // control. Sengaja tidak dimasukkan ke AlasanTidakLayakTerbitAsync — method itu juga
            // dipakai penerbitan, sedangkan butir (3) melarang pemeriksaan ulang saat terbit.
            // Kodenya 422, bukan 409 seperti baris tidak sah lainnya: satu aturan control account
            // dijawab satu kode di setiap jalurnya (`ACC-API-0.11`).
            var alasanControl = await AccJournalService.AlasanControlAccountAsync(
                _db,
                template.Lines.Where(x => !x.IsDelete).Select(x => x.AccountId),
                AccJournalService.JalurTemplateBerulang,
                ct);

            if (alasanControl is not null)
            {
                return Gagal<RecurringJournalDetailResponse>(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Template {template.TemplateCode} tidak dapat diaktifkan. {alasanControl}");
            }

            template.IsActive = true;
            template.UpdateDateTime = DateTime.UtcNow;
            template.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            var tersimpan = await MuatLengkapAsync(template.Id, lacak: false, ct);

            return AccountingServiceResult<RecurringJournalDetailResponse>.Ok(
                Petakan(tersimpan!),
                $"Template {template.TemplateCode} berhasil diaktifkan. Mulai sekarang jurnalnya "
                + $"terbit setiap tanggal {template.DayOfMonth} sebagai draft.");
        }

        public async Task<AccountingServiceResult<RecurringJournalDetailResponse>> DeactivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var template = await MuatLengkapAsync(id, lacak: true, ct);
            if (template is null) return TidakDitemukan<RecurringJournalDetailResponse>();

            if (!template.IsActive)
            {
                return Gagal<RecurringJournalDetailResponse>(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} sudah tidak aktif.");
            }

            template.IsActive = false;
            template.UpdateDateTime = DateTime.UtcNow;
            template.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            var tersimpan = await MuatLengkapAsync(template.Id, lacak: false, ct);

            // Jurnal yang sudah terbit TIDAK ikut ditarik. Ia sudah menjadi draft milik petugas,
            // dan menghapusnya diam-diam akan menghilangkan pekerjaan yang mungkin sudah
            // disunting.
            return AccountingServiceResult<RecurringJournalDetailResponse>.Ok(
                Petakan(tersimpan!),
                $"Template {template.TemplateCode} berhasil dinonaktifkan. Jurnal yang sudah "
                + "terbit tidak ikut dibatalkan.");
        }

        // ------------------------------------------------------------------
        // Penerbitan — BE-ACC-P2-008
        // ------------------------------------------------------------------

        /// <summary>
        /// Menerbitkan jurnal <c>Draft</c> dari sebuah template untuk satu periode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Penjaga terbit ganda ada di DATABASE, bukan di kode ini.</b> Pemeriksaan
        /// <c>CariPenerbitanAsync</c> hanyalah lapisan pertama yang memberi pesan enak dibaca; ia
        /// <b>tidak</b> mencegah apa pun ketika dua proses berjalan bersamaan, karena keduanya
        /// dapat sama-sama lolos memeriksa sebelum salah satunya menyimpan. Yang benar-benar
        /// mencegah adalah unique index <c>(TemplateId, AccountingPeriodId)</c> pada
        /// <c>AccRecurringJournalRun</c>: dua proses dapat sama-sama lolos pemeriksaan, tetapi
        /// tidak dapat sama-sama lolos dari database.
        /// </para>
        /// <para>
        /// Karena itu <see cref="DbUpdateException"/> di sini <b>tidak dilemparkan ulang</b>
        /// melainkan diterjemahkan menjadi <c>409</c>: ia bukan kegagalan sistem, melainkan
        /// penjaga yang bekerja persis sebagaimana mestinya. Inilah acceptance (1) — penerbitan
        /// kedua ditolak <b>oleh database</b>, bukan hanya oleh kode.
        /// </para>
        /// <para>
        /// Seluruh langkah berada dalam satu transaction, sehingga tidak mungkin ada jurnal yang
        /// terbit tanpa baris penerbitan yang menjaganya — keadaan yang justru akan membuat
        /// periode itu dapat diterbitkan lagi dan lagi.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<RecurringJournalRunResponse>> GenerateAsync(
            Guid templateId,
            GenerateRecurringJournalRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<RecurringJournalRunResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var template = await MuatLengkapAsync(templateId, lacak: false, ct);
            if (template is null) return TidakDitemukan<RecurringJournalRunResponse>();

            var siap = await SiapkanPenerbitanAsync(template, request.AccountingDate, ct);

            if (siap.Gagal is not null)
            {
                return Gagal<RecurringJournalRunResponse>(siap.Gagal.StatusCode, siap.Gagal.Pesan);
            }

            return await TerbitkanAsync(template, siap.Periode!, siap.Tanggal, actorUserId, ct);
        }

        /// <summary>
        /// Menerbitkan seluruh template aktif yang sudah jatuh tempo pada tanggal tertentu.
        /// Dipanggil penjadwal.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Template yang tidak dapat diterbitkan DILEWATI, bukan menggagalkan seluruh
        /// siklus</b> — acceptance (2). Satu template yang periodenya sudah ditutup tidak boleh
        /// menghentikan penerbitan sembilan template lain yang baik-baik saja. Setiap yang
        /// dilewati beserta alasannya dikembalikan supaya penjadwal dapat mencatatnya.
        /// </para>
        /// <para>
        /// Template yang <b>sudah pernah terbit</b> untuk periode itu juga dilewati, dan ditandai
        /// tersendiri. Penjadwal berjalan berkali-kali sehari; memperlakukan "sudah terbit"
        /// sebagai kegagalan akan memenuhi log dengan peringatan yang justru menandakan sistemnya
        /// bekerja benar.
        /// </para>
        /// </remarks>
        public async Task<RecurringJournalCycleResult> TerbitkanYangJatuhTempoAsync(
            DateTime tanggalLokal,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var hasil = new RecurringJournalCycleResult { EvaluatedAt = DateTime.UtcNow };

            var tanggal = tanggalLokal.Date;
            var hari = tanggal.Day;

            var kandidat = await _db.Set<AccRecurringJournalTemplate>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.IsActive
                            && x.DayOfMonth <= hari
                            && x.StartDate <= tanggal
                            && (x.EndDate == null || x.EndDate >= tanggal))
                .OrderBy(x => x.TemplateCode)
                .Select(x => x.Id)
                .ToListAsync(ct);

            hasil.Considered = kandidat.Count;

            foreach (var id in kandidat)
            {
                if (ct.IsCancellationRequested) break;

                var template = await MuatLengkapAsync(id, lacak: false, ct);
                if (template is null) continue;

                // Tanggal akuntansinya adalah DayOfMonth template pada bulan berjalan, bukan
                // tanggal penjadwal kebetulan berjalan. Template bertanggal 25 yang baru sempat
                // diproses tanggal 27 tetap menghasilkan jurnal bertanggal 25.
                var tanggalAkuntansi = new DateTime(tanggal.Year, tanggal.Month, template.DayOfMonth);

                var siap = await SiapkanPenerbitanAsync(template, tanggalAkuntansi, ct);

                if (siap.Gagal is not null)
                {
                    hasil.Skipped.Add(new RecurringJournalCycleSkip
                    {
                        TemplateId = template.Id,
                        TemplateCode = template.TemplateCode,
                        Reason = siap.Gagal.Pesan,
                        AlreadyPublished = siap.Gagal.SudahTerbit
                    });
                    continue;
                }

                var terbit = await TerbitkanAsync(template, siap.Periode!, siap.Tanggal, actorUserId, ct);

                if (terbit.Success)
                {
                    hasil.Published.Add(terbit.Data!);
                }
                else
                {
                    hasil.Skipped.Add(new RecurringJournalCycleSkip
                    {
                        TemplateId = template.Id,
                        TemplateCode = template.TemplateCode,
                        Reason = terbit.Message,
                        AlreadyPublished = terbit.StatusCode == StatusCodes.Status409Conflict
                    });
                }
            }

            return hasil;
        }

        /// <summary>
        /// Menyimpan jurnal beserta baris penerbitannya dalam satu transaction.
        /// </summary>
        private async Task<AccountingServiceResult<RecurringJournalRunResponse>> TerbitkanAsync(
            AccRecurringJournalTemplate template,
            AccAccountingPeriod periode,
            DateTime tanggalAkuntansi,
            Guid actorUserId,
            CancellationToken ct)
        {
            var keterangan = $"{template.TemplateName} — terbitan {template.TemplateCode}";

            var permintaan = new CreateJournalRequest
            {
                LegalEntityId = template.LegalEntityId,
                JournalTypeId = template.JournalTypeId,
                AccountingDate = tanggalAkuntansi,
                Description = keterangan.Length > 500 ? keterangan[..500] : keterangan,
                Lines = template.Lines
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.LineNumber)
                    .Select(x => new CreateJournalLineRequest
                    {
                        LineNumber = x.LineNumber,
                        AccountId = x.AccountId,
                        CostCenterId = x.CostCenterId,
                        Description = x.Description,
                        DebitAmount = x.DebitAmount,
                        CreditAmount = x.CreditAmount
                    })
                    .ToList()
            };

            var transaksiSendiri = _db.Database.CurrentTransaction is null;
            var transaksi = transaksiSendiri
                ? await _db.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                var jurnal = await _journalService.CreateAsync(permintaan, actorUserId, null, ct);

                if (!jurnal.Success)
                {
                    if (transaksi is not null) await transaksi.RollbackAsync(ct);
                    return Gagal<RecurringJournalRunResponse>(jurnal.StatusCode, jurnal.Message);
                }

                var sekarang = DateTime.UtcNow;

                var run = new AccRecurringJournalRun
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    AccountingPeriodId = periode.Id,
                    JournalId = jurnal.Data!.Id,
                    GeneratedAt = sekarang,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                };

                _db.Set<AccRecurringJournalRun>().Add(run);

                await _db.SaveChangesAsync(ct);

                if (transaksi is not null) await transaksi.CommitAsync(ct);

                return AccountingServiceResult<RecurringJournalRunResponse>.Ok(
                    new RecurringJournalRunResponse
                    {
                        Id = run.Id,
                        TemplateId = run.TemplateId,
                        AccountingPeriodId = run.AccountingPeriodId,
                        PeriodCode = periode.PeriodCode,
                        JournalId = run.JournalId,
                        JournalNumber = jurnal.Data.JournalNumber,
                        JournalStatus = jurnal.Data.JournalStatus,
                        GeneratedAt = run.GeneratedAt
                    },
                    $"Template {template.TemplateCode} berhasil diterbitkan untuk periode "
                    + $"{periode.PeriodCode} sebagai jurnal {jurnal.Data.JournalNumber} "
                    + "berstatus draft.",
                    StatusCodes.Status201Created);
            }
            catch (DbUpdateException)
            {
                // Unique index (TemplateId, AccountingPeriodId) menolak penerbitan kedua. Ini
                // BUKAN kegagalan sistem — ini penjaga yang bekerja. Transaction dibatalkan
                // seluruhnya, sehingga jurnal yang sempat dibuat pada percobaan kedua ikut
                // hilang dan buku besar tetap berisi satu.
                if (transaksi is not null) await transaksi.RollbackAsync(ct);

                return Gagal<RecurringJournalRunResponse>(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} sudah diterbitkan untuk periode "
                    + $"{periode.PeriodCode}. Penerbitan kedua ditolak database.");
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

        private sealed class PenolakanTerbit
        {
            public int StatusCode { get; init; }

            public string Pesan { get; init; } = string.Empty;

            /// <summary>
            /// Benar bila sebabnya template memang sudah pernah terbit untuk periode itu —
            /// keadaan normal bagi penjadwal, bukan keadaan yang perlu diperingatkan.
            /// </summary>
            public bool SudahTerbit { get; init; }
        }

        private sealed class HasilPersiapanTerbit
        {
            public PenolakanTerbit? Gagal { get; init; }

            public AccAccountingPeriod? Periode { get; init; }

            public DateTime Tanggal { get; init; }
        }

        /// <summary>
        /// Seluruh pemeriksaan sebelum penerbitan, dipakai bersama jalur manual dan penjadwal
        /// supaya keduanya tidak punya dua tafsir yang berbeda atas kata "layak terbit".
        /// </summary>
        private async Task<HasilPersiapanTerbit> SiapkanPenerbitanAsync(
            AccRecurringJournalTemplate template,
            DateTime? accountingDate,
            CancellationToken ct)
        {
            // Template nonaktif tidak terbit (`ACC-VALIDATION-0.6` bagian 3).
            if (!template.IsActive)
            {
                return TolakTerbit(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} sedang tidak aktif.");
            }

            var tanggal = (accountingDate ?? TanggalTerbitBulanIni(template)).Date;

            if (tanggal < template.StartDate.Date)
            {
                return TolakTerbit(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} baru berlaku mulai "
                    + $"{template.StartDate:yyyy-MM-dd}.");
            }

            if (template.EndDate.HasValue && tanggal > template.EndDate.Value.Date)
            {
                return TolakTerbit(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} sudah berakhir pada "
                    + $"{template.EndDate.Value:yyyy-MM-dd}.");
            }

            var alasanBaris = await AlasanTidakLayakTerbitAsync(template, ct);

            if (alasanBaris is not null)
            {
                return TolakTerbit(
                    StatusCodes.Status409Conflict,
                    $"Template {template.TemplateCode} tidak dapat diterbitkan. {alasanBaris}");
            }

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete
                         && x.LegalEntityId == template.LegalEntityId
                         && x.StartDate <= tanggal
                         && x.EndDate >= tanggal, ct);

            if (periode is null)
            {
                return TolakTerbit(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Belum ada periode akuntansi untuk tanggal {tanggal:yyyy-MM-dd}. Minta "
                    + "administrator membangkitkan periode tahun buku ini.");
            }

            // Aturan status periode dipakai ulang dari AccAccountingPeriodService, bukan
            // ditafsirkan sendiri di sini.
            var kodeJenis = await _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => x.Id == template.JournalTypeId)
                .Select(x => x.JournalTypeCode)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            var alasanPeriode = AccAccountingPeriodService.AlasanPenolakanJenisJurnal(
                periode.PeriodStatus, periode.PeriodCode, kodeJenis);

            if (alasanPeriode is not null)
            {
                return TolakTerbit(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Periode tujuan tidak menerima pencatatan baru. {alasanPeriode}");
            }

            // Lapisan pertama penjaga terbit ganda. Lapisan yang sesungguhnya ada di database.
            var sudah = await CariPenerbitanAsync(template.Id, periode.Id, ct);

            if (sudah is not null)
            {
                return new HasilPersiapanTerbit
                {
                    Gagal = new PenolakanTerbit
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Pesan = $"Template {template.TemplateCode} sudah diterbitkan untuk "
                              + $"periode {periode.PeriodCode} dengan jurnal {sudah}.",
                        SudahTerbit = true
                    }
                };
            }

            return new HasilPersiapanTerbit { Periode = periode, Tanggal = tanggal };
        }

        private static HasilPersiapanTerbit TolakTerbit(int statusCode, string pesan)
            => new() { Gagal = new PenolakanTerbit { StatusCode = statusCode, Pesan = pesan } };

        /// <summary>Tanggal terbit template pada bulan berjalan.</summary>
        private static DateTime TanggalTerbitBulanIni(AccRecurringJournalTemplate template)
        {
            var sekarang = DateTime.UtcNow.Date;
            return new DateTime(sekarang.Year, sekarang.Month, template.DayOfMonth);
        }

        private Task<string?> CariPenerbitanAsync(
            Guid templateId, Guid periodeId, CancellationToken ct)
        {
            return _db.Set<AccRecurringJournalRun>()
                .AsNoTracking()
                .Where(x => x.TemplateId == templateId
                            && x.AccountingPeriodId == periodeId
                            && !x.IsDelete)
                .Select(x => x.Journal != null ? x.Journal.JournalNumber : "(tanpa nomor)")
                .FirstOrDefaultAsync(ct);
        }

        // ------------------------------------------------------------------
        // Dipakai bersama BE-ACC-P2-008
        // ------------------------------------------------------------------

        /// <summary>
        /// Alasan sebuah template tidak layak menerbitkan jurnal, atau <c>null</c> bila layak.
        /// </summary>
        /// <remarks>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya
        /// penjadwal <c>BE-ACC-P2-008</c> memakai penilaian yang sama persis dengan endpoint
        /// pengaktifan, tanpa registrasi DI baru — sesuai <c>02-backend-architecture.md</c>
        /// bagian 6. Bila keduanya menilai sendiri-sendiri, sebuah template dapat lolos
        /// diaktifkan lalu gagal terbit tiap bulan tanpa ada yang tahu sebabnya.
        /// </remarks>
        public static async Task<string?> AlasanTidakLayakTerbitAsync(
            ApplicationDbContext db,
            AccRecurringJournalTemplate template,
            CancellationToken ct = default)
        {
            var baris = template.Lines.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber).ToList();

            if (baris.Count < BarisMinimum)
            {
                return $"Template hanya memiliki {baris.Count} baris; minimal {BarisMinimum}.";
            }

            var debit = baris.Sum(x => x.DebitAmount);
            var kredit = baris.Sum(x => x.CreditAmount);

            if (debit != kredit)
            {
                return $"Baris template belum seimbang. Selisih Rp {Math.Abs(debit - kredit):N0}.";
            }

            var jenis = await db.Set<AccJournalType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == template.JournalTypeId && !x.IsDelete, ct);

            if (jenis is null || !jenis.IsActive)
            {
                return "Jenis jurnal template sudah tidak aktif.";
            }

            var idAkun = baris.Select(x => x.AccountId).Distinct().ToList();

            var akunTersedia = await db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => idAkun.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var idUnitBiaya = baris
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var unitBiayaTersedia = await db.Set<MstCostCenter>()
                .AsNoTracking()
                .Where(x => idUnitBiaya.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            foreach (var b in baris)
            {
                var n = b.LineNumber;

                if (!akunTersedia.TryGetValue(b.AccountId, out var akun) || !akun.IsActive)
                {
                    return $"Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif.";
                }

                if (!akun.IsPostable)
                {
                    return $"Baris ke-{n}: akun {akun.AccountCode} adalah akun induk dan tidak "
                         + "dapat menerima transaksi.";
                }

                if (akun.LegalEntityId != template.LegalEntityId)
                {
                    return $"Baris ke-{n}: akun {akun.AccountCode} bukan milik badan hukum "
                         + "template ini.";
                }

                if (akun.AccountType == AccountType.Expense && !b.CostCenterId.HasValue)
                {
                    return $"Baris ke-{n}: akun beban {akun.AccountCode} wajib menyebutkan unit "
                         + "biaya.";
                }

                if (b.CostCenterId.HasValue)
                {
                    if (!unitBiayaTersedia.TryGetValue(b.CostCenterId.Value, out var unitBiaya)
                        || !unitBiaya.IsActive
                        || unitBiaya.LegalEntityId != template.LegalEntityId)
                    {
                        return $"Baris ke-{n}: unit biaya tidak aktif atau bukan milik badan "
                             + "hukum template ini.";
                    }
                }
            }

            return null;
        }

        private Task<string?> AlasanTidakLayakTerbitAsync(
            AccRecurringJournalTemplate template, CancellationToken ct)
            => AlasanTidakLayakTerbitAsync(_db, template, ct);

        /// <summary>
        /// Memuat template beserta barisnya. <c>public static</c> dengan alasan yang sama.
        /// </summary>
        public static Task<AccRecurringJournalTemplate?> MuatLengkapAsync(
            ApplicationDbContext db,
            Guid id,
            bool lacak,
            CancellationToken ct = default)
        {
            // Kedua Include memakai filter yang SAMA PERSIS. EF hanya mengizinkan satu filter
            // per navigation; menambahkan OrderBy pada salah satunya membuat keduanya dianggap
            // filter berbeda dan seluruh query gagal saat dijalankan. Urutan barisnya diatur
            // Petakan di memori, bukan di sini.
            var q = db.Set<AccRecurringJournalTemplate>()
                .Include(x => x.JournalType)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.Account)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.CostCenter)
                .AsQueryable();

            if (!lacak) q = q.AsNoTracking();

            return q.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        }

        // ------------------------------------------------------------------
        // Pemeriksaan isian — ACC-VALIDATION-0.6 bagian 3
        // ------------------------------------------------------------------

        private sealed class HasilPersiapan
        {
            public AccountingServiceResult<RecurringJournalDetailResponse>? Gagal { get; init; }

            public string KodeTemplate { get; init; } = string.Empty;

            public List<AccRecurringJournalTemplateLine> Baris { get; init; } = new();
        }

        private static HasilPersiapan Tolak(int statusCode, string pesan)
            => new() { Gagal = Gagal<RecurringJournalDetailResponse>(statusCode, pesan) };

        /// <summary>
        /// Memeriksa seluruh isian dan menyusun baris template yang siap disimpan. Dipakai
        /// bersama oleh <see cref="CreateAsync"/> dan <see cref="UpdateAsync"/> supaya aturan
        /// yang sama tidak ditulis dua kali dengan pesan yang berbeda-beda.
        /// </summary>
        private async Task<HasilPersiapan> SiapkanAsync(
            Guid legalEntityId,
            Guid? templateId,
            string? templateCode,
            string? templateName,
            Guid journalTypeId,
            int dayOfMonth,
            DateTime startDate,
            DateTime? endDate,
            List<CreateRecurringJournalLineRequest>? lines,
            CancellationToken ct)
        {
            var kode = templateCode?.Trim().ToUpperInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(kode) || kode.Length > 50)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Kode template wajib diisi dan maksimal 50 karakter.");
            }

            var nama = templateName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nama) || nama.Length > 200)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Nama template wajib diisi dan maksimal 200 karakter.");
            }

            // Batas 1-28 juga dijaga check constraint pada tabel; diperiksa di sini supaya
            // pesannya menjelaskan sebabnya, bukan melemparkan galat database.
            if (dayOfMonth is < 1 or > 28)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Tanggal terbit harus antara 1 dan 28. Tanggal 29, 30, dan 31 tidak ada di "
                    + "setiap bulan, sehingga template bertanggal itu akan terlewat pada Februari.");
            }

            if (startDate == default)
            {
                return Tolak(StatusCodes.Status400BadRequest, "Tanggal mulai wajib diisi.");
            }

            if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Tanggal berakhir tidak boleh lebih awal daripada tanggal mulai.");
            }

            var jenis = journalTypeId == Guid.Empty
                ? null
                : await _db.Set<AccJournalType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == journalTypeId && !x.IsDelete && x.IsActive, ct);

            if (jenis is null)
            {
                return Tolak(StatusCodes.Status400BadRequest, "Jenis jurnal wajib dipilih.");
            }

            // Kode template unik dalam satu badan hukum. Penjaga terakhirnya unique index
            // (LegalEntityId, TemplateCode) tersaring IsDelete = false.
            var kembar = await _db.Set<AccRecurringJournalTemplate>()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                               && x.LegalEntityId == legalEntityId
                               && x.TemplateCode.ToLower() == kode.ToLower()
                               && (templateId == null || x.Id != templateId.Value), ct);

            if (kembar)
            {
                return Tolak(
                    StatusCodes.Status409Conflict,
                    $"Kode template {kode} sudah dipakai pada badan hukum ini.");
            }

            var barisSiap = await SusunBarisAsync(legalEntityId, lines, ct);
            if (barisSiap.Gagal is not null) return barisSiap;

            // ACC-DEC-073 butir (1). Diperiksa di hulu, saat masih ada manusia yang membaca
            // pesannya — bukan saat terbit, yang terjadi di penjadwal tanpa penonton.
            var alasanControl = await AccJournalService.AlasanControlAccountAsync(
                _db,
                barisSiap.Baris.Select(x => x.AccountId),
                AccJournalService.JalurTemplateBerulang,
                ct);

            if (alasanControl is not null)
                return Tolak(StatusCodes.Status422UnprocessableEntity, alasanControl);

            return new HasilPersiapan { KodeTemplate = kode, Baris = barisSiap.Baris };
        }

        /// <remarks>
        /// Pesan penolakan selalu menyebut <b>nomor baris</b>, mengikuti ketentuan yang sama
        /// dengan baris jurnal, supaya petugas tidak perlu menebak baris mana yang salah.
        /// </remarks>
        private async Task<HasilPersiapan> SusunBarisAsync(
            Guid legalEntityId,
            List<CreateRecurringJournalLineRequest>? lines,
            CancellationToken ct)
        {
            var permintaan = lines ?? new List<CreateRecurringJournalLineRequest>();

            // Acceptance (1) bagian pertama — minimal dua baris.
            if (permintaan.Count < BarisMinimum)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    $"Template jurnal minimal memiliki {BarisMinimum} baris.");
            }

            if (permintaan.Select(x => x.LineNumber).Distinct().Count() != permintaan.Count)
            {
                return Tolak(StatusCodes.Status400BadRequest, "Nomor baris tidak boleh kembar.");
            }

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

            var hasil = new List<AccRecurringJournalTemplateLine>();

            foreach (var baris in permintaan.OrderBy(x => x.LineNumber))
            {
                var n = baris.LineNumber;

                if (n < 1)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Nomor baris harus dimulai dari 1; ditemukan {n}.");
                }

                if (baris.DebitAmount < 0m || baris.CreditAmount < 0m)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: nilai tidak boleh negatif. Untuk membalik arah, "
                        + "pindahkan ke sisi sebaliknya.");
                }

                // Acceptance (3) — satu baris hanya satu sisi, dan sisinya harus lebih dari nol.
                if ((baris.DebitAmount > 0m) == (baris.CreditAmount > 0m))
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: isi salah satu saja, debit atau kredit, dan nilainya "
                        + "harus lebih dari nol.");
                }

                if (!akunTersedia.TryGetValue(baris.AccountId, out var akun) || !akun.IsActive)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif.");
                }

                if (!akun.IsPostable)
                {
                    return Tolak(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} adalah akun induk dan tidak "
                        + "dapat menerima transaksi.");
                }

                if (akun.LegalEntityId != legalEntityId)
                {
                    return Tolak(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: akun {akun.AccountCode} bukan milik badan hukum "
                        + "template ini.");
                }

                // Acceptance (2) — ACC-DEC-019.
                if (akun.AccountType == AccountType.Expense && !baris.CostCenterId.HasValue)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun beban {akun.AccountCode} wajib menyebutkan unit biaya.");
                }

                if (baris.CostCenterId.HasValue)
                {
                    if (!unitBiayaTersedia.TryGetValue(baris.CostCenterId.Value, out var unitBiaya)
                        || !unitBiaya.IsActive
                        || unitBiaya.LegalEntityId != legalEntityId)
                    {
                        return Tolak(
                            StatusCodes.Status409Conflict,
                            $"Baris ke-{n}: unit biaya tidak aktif atau bukan milik badan hukum "
                            + "template ini.");
                    }
                }

                hasil.Add(new AccRecurringJournalTemplateLine
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

            // Acceptance (1) bagian kedua — keseimbangan. Diperiksa PALING AKHIR supaya kesalahan
            // yang lebih spesifik pada satu baris disebut lebih dahulu; pesan "belum seimbang"
            // pada template yang barisnya memang salah hanya akan menyesatkan.
            var debit = hasil.Sum(x => x.DebitAmount);
            var kredit = hasil.Sum(x => x.CreditAmount);

            if (debit != kredit)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    $"Total debit dan kredit template harus sama. Total debit Rp {debit:N0}, "
                    + $"total kredit Rp {kredit:N0}, selisih Rp {Math.Abs(debit - kredit):N0}.");
            }

            return new HasilPersiapan { Baris = hasil };
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<AccRecurringJournalTemplate?> MuatLengkapAsync(
            Guid id, bool lacak, CancellationToken ct)
            => MuatLengkapAsync(_db, id, lacak, ct);

        private static AccountingServiceResult<T> Gagal<T>(int statusCode, string pesan)
            => AccountingServiceResult<T>.Fail(statusCode, pesan);

        private static AccountingServiceResult<T> TidakDitemukan<T>()
            => AccountingServiceResult<T>.Fail(
                StatusCodes.Status404NotFound, "Template jurnal berulang tidak ditemukan.");

        private static RecurringJournalDetailResponse Petakan(AccRecurringJournalTemplate x)
        {
            var baris = x.Lines
                .Where(l => !l.IsDelete)
                .OrderBy(l => l.LineNumber)
                .Select(l => new RecurringJournalLineResponse
                {
                    Id = l.Id,
                    LineNumber = l.LineNumber,
                    AccountId = l.AccountId,
                    AccountCode = l.Account?.AccountCode ?? string.Empty,
                    AccountName = l.Account?.AccountName ?? string.Empty,
                    CostCenterId = l.CostCenterId,
                    CostCenterName = l.CostCenter?.CostCenterName,
                    Description = l.Description,
                    DebitAmount = l.DebitAmount,
                    CreditAmount = l.CreditAmount
                })
                .ToList();

            var debit = baris.Sum(l => l.DebitAmount);
            var kredit = baris.Sum(l => l.CreditAmount);

            return new RecurringJournalDetailResponse
            {
                Id = x.Id,
                LegalEntityId = x.LegalEntityId,
                TemplateCode = x.TemplateCode,
                TemplateName = x.TemplateName,
                JournalTypeId = x.JournalTypeId,
                JournalTypeCode = x.JournalType?.JournalTypeCode ?? string.Empty,
                JournalTypeName = x.JournalType?.JournalTypeName ?? string.Empty,
                Frequency = x.Frequency,
                DayOfMonth = x.DayOfMonth,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                TotalDebit = debit,
                TotalCredit = kredit,
                IsBalanced = debit == kredit,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                Lines = baris
            };
        }
    }
}
