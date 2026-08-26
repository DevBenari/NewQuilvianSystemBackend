using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bagian penanggung jawab, kebutuhan isolasi, dan penjagaan satu pasien satu episode.
    /// Diisi task <c>BE-RWI-012</c>, <c>BE-RWI-014</c>, <c>BE-RWI-017</c>, dan
    /// <c>BE-RWI-018</c>.
    /// </summary>
    /// <remarks>
    /// <b>Penjaga kewenangan per pasien ada di sini, bukan di mesin hak akses.</b> Mesin hak
    /// akses repository ini hanya mengenal "peran ini boleh memanggil endpoint ini"; ia tidak
    /// mengenal "dokter ini boleh bertindak terhadap pasien ini". Bukti keterbatasan itu
    /// tercatat sebagai <c>RWI-TF-014</c>, dan akibatnya <c>GUARD-INP-01</c> sampai
    /// <c>GUARD-INP-04</c> wajib dipanggil dari dalam service.
    ///
    /// <para>
    /// Konsekuensinya, penjaga itu <b>hanya bekerja bila benar-benar dipanggil</b>. Endpoint
    /// baru yang lupa memanggilnya akan lolos tanpa kesalahan kompilasi maupun peringatan
    /// runtime. Risiko ini tercatat sebagai <c>RWI-RISK-004</c>, dan penurunnya adalah test
    /// yang diwajibkan <c>RWI-DEC-051</c>.
    /// </para>
    /// </remarks>
    public partial class InpEpisodeService
    {
        /// <summary>Nilai <c>ActionType</c> untuk pengalihan DPJP.</summary>
        public const string ActionHandoverDoctor = "HandoverDoctor";

        /// <summary>Nilai <c>ActionType</c> untuk penugasan perawat penanggung jawab.</summary>
        public const string ActionAssignNurse = "AssignNurse";

        // =====================================================================
        // BE-RWI-012 — INV-INP-10, satu pasien satu episode yang hadir
        // =====================================================================

        /// <summary>
        /// Mencari episode milik pasien yang <b>benar-benar hadir</b> di ruangan, yaitu
        /// berstatus <c>Admitted</c>, atau <c>DischargePending</c> yang kepergiannya belum
        /// dicatat. Mengembalikan <c>null</c> bila tidak ada.
        /// </summary>
        /// <remarks>
        /// <b>Kenapa batasnya kepergian fisik, bukan penutupan episode.</b> Pasien yang sudah
        /// pulang pukul 10:15 tetapi episodenya baru ditutup pukul 13:10 sesungguhnya sudah
        /// tidak dirawat. Bila ia kembali dengan keluhan baru pukul 12:00, admisi barunya
        /// tidak boleh tertahan hanya karena urusan administrasi episode lama belum beres.
        /// Ini <c>RWI-DEC-054</c>, dan bentuk yang sama dipakai unique index parsial
        /// <c>IX_InpEpisode_PatientId_Present</c>.
        ///
        /// <para>
        /// Pemeriksaan ini adalah lapis pertama; ia ada supaya petugas menerima kalimat yang
        /// dapat dibacanya. Lapis terakhirnya tetap unique index parsial tersebut, yang bekerja
        /// walaupun dua permintaan sama-sama lolos pemeriksaan ini pada saat hampir bersamaan.
        /// </para>
        /// </remarks>
        public async Task<InpPresentEpisodeInfo?> FindPresentEpisodeAsync(
            Guid patientId,
            Guid excludeEpisodeId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
            {
                return null;
            }

            var present = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.Id != excludeEpisodeId &&
                    !x.IsDelete &&
                    (x.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     (x.EpisodeStatus == InpEpisodeStatus.DischargePending &&
                      x.PhysicallyLeftAt == null)))
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    BedName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .OrderByDescending(p => p.SequenceNumber)
                        .Select(p => p.Bed != null ? p.Bed.BedName : null)
                        .FirstOrDefault(),
                    RoomName = x.BedPlacements
                        .Where(p => p.EndDateTime == null && !p.IsDelete)
                        .OrderByDescending(p => p.SequenceNumber)
                        .Select(p => p.Room != null ? p.Room.RoomName : null)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (present == null)
            {
                return null;
            }

            return new InpPresentEpisodeInfo(
                present.Id,
                present.EpisodeNumber,
                present.PatientName,
                present.BedName,
                present.RoomName);
        }

        // =====================================================================
        // BE-RWI-014 — Kebutuhan isolasi beserta GUARD-INP-04
        // =====================================================================

        /// <summary>
        /// Menetapkan atau mengubah kebutuhan isolasi episode.
        /// </summary>
        /// <param name="actorDoctorId">
        /// Identitas dokter milik pengguna yang sedang masuk, dibaca dari klaim
        /// <c>doctor_id</c>. Kosong bila penggunanya bukan dokter — itulah jalur petugas
        /// admisi.
        /// </param>
        /// <remarks>
        /// <b><c>GUARD-INP-04</c>.</b> Mesin hak akses menjawab <c>SetIsolation</c> dengan
        /// "boleh" untuk petugas admisi <b>dan</b> untuk dokter mana pun. Yang membedakan
        /// keduanya adalah status episode dan siapa DPJP aktifnya:
        ///
        /// <list type="bullet">
        /// <item><description>
        /// Selagi <c>Draft</c>, petugas admisi boleh merekam keterangan dokter pengirim.
        /// Hasilnya ditandai <c>AdmissionRecord</c> dan tidak menyamar sebagai keputusan
        /// klinis.
        /// </description></item>
        /// <item><description>
        /// Setelah episode berjalan, hanya <b>DPJP aktif episode itu</b> yang boleh
        /// mengubahnya. Hasilnya ditandai <c>ClinicalDecision</c>.
        /// </description></item>
        /// </list>
        ///
        /// Tanpa penjaga ini, dokter jaga mana pun dapat mengubah keputusan pengendalian
        /// infeksi milik DPJP lain, dan tidak ada yang dapat membedakannya dari keputusan yang
        /// sah.
        ///
        /// <para>
        /// <b>Perubahan tidak pernah ditahan penempatan.</b> Pasien yang sedang berada di
        /// tempat tidur biasa tetap boleh dinyatakan membutuhkan isolasi. Fakta klinis dicatat
        /// lebih dulu; penempatannya kemudian muncul pada daftar pantau penempatan tidak
        /// sesuai. Urutan sebaliknya — menahan pencatatan demi menjaga aturan penempatan —
        /// ditolak tegas oleh <c>RWI-RULE-012</c> bagian A aturan 7.
        /// </para>
        /// </remarks>
        public async Task<InpEpisodeOperationResult> SetIsolationRequirementAsync(
            Guid episodeId,
            SetIsolationRequirementRequest request,
            Guid actorUserId,
            Guid? actorDoctorId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.Invalid("Isian kebutuhan isolasi belum dikirim.");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu.",
                    episode);
            }

            switch (episode.EpisodeStatus)
            {
                case InpEpisodeStatus.Closed:
                    return InpEpisodeOperationResult.Conflict("Episode sudah ditutup.", episode);

                case InpEpisodeStatus.Cancelled:
                    return InpEpisodeOperationResult.Conflict(
                        "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                        episode);
            }

            // GUARD-INP-04.
            var isDoctor = actorDoctorId.HasValue && actorDoctorId.Value != Guid.Empty;
            var isActiveDoctor = isDoctor &&
                await IsActiveDoctorAsync(episode.Id, actorDoctorId, cancellationToken);

            if (episode.EpisodeStatus == InpEpisodeStatus.Draft)
            {
                if (isDoctor && !isActiveDoctor)
                {
                    return InpEpisodeOperationResult.Forbidden(
                        "Anda tidak punya hak akses untuk tindakan ini.");
                }
            }
            else if (!isActiveDoctor)
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Setelah pasien dirawat, hanya DPJP episode ini yang dapat mengubah " +
                    "kebutuhan isolasi.");
            }

            var note = NormalizeText(request.IsolationNote);

            if (request.RequiresIsolation && string.IsNullOrWhiteSpace(note))
            {
                return InpEpisodeOperationResult.Invalid(
                    "Tuliskan alasan atau keterangan kebutuhan isolasi.");
            }

            var now = DateTime.UtcNow;

            episode.RequiresIsolation = request.RequiresIsolation;
            episode.IsolationNote = note;
            episode.IsolationSetAt = now;
            episode.IsolationSetByUserId = actorUserId;

            // Sumber ditetapkan sistem, tidak pernah dikirim pemanggil. Petugas admisi
            // menghasilkan catatan awal; DPJP aktif menghasilkan keputusan klinis.
            if (isActiveDoctor)
            {
                episode.IsolationSource = InpIsolationSource.ClinicalDecision;
                episode.IsolationSetByDoctorId = actorDoctorId;
            }
            else
            {
                episode.IsolationSource = InpIsolationSource.AdmissionRecord;
                episode.IsolationSetByDoctorId = null;
            }

            episode.UpdateDateTime = now;
            episode.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpEpisodeOperationResult.Success(
                episode,
                request.RequiresIsolation
                    ? "Kebutuhan isolasi berhasil disimpan."
                    : "Kebutuhan isolasi berhasil dicabut.");
        }

        // =====================================================================
        // BE-RWI-017 — Penugasan dan pengalihan DPJP
        // =====================================================================

        /// <summary>
        /// Mengalihkan DPJP: menutup penugasan yang sedang berlaku dan membuka penugasan baru
        /// pada tindakan yang sama.
        /// </summary>
        /// <param name="actorIsWardHeadOrSupervisor">
        /// Benar bila pelakunya kepala ruangan atau supervisor. Hanya mereka yang boleh
        /// mengalihkan DPJP, sesuai <c>RWI-RULE-016</c>. Penjaga ini berada di service karena
        /// pengalihan DPJP memakai butir hak akses yang sama dengan seluruh perubahan episode
        /// lain, yaitu <c>InpatientEpisode : Update</c>.
        /// </param>
        /// <remarks>
        /// <b>Riwayat berperiode, bukan satu kolom yang ditimpa.</b> Ketika auditor bertanya
        /// siapa yang berwenang pada 22 September, sistem masih dapat menjawabnya pada
        /// 25 September. Menyimpan <c>CurrentDoctorId</c> sebagai kolom pada episode akan
        /// membuat query lebih murah dan menghapus jawaban itu selamanya; bentuk berperiode
        /// dikunci `blueprint-manifest.md` bagian 8 butir 4.
        /// </remarks>
        public async Task<InpEpisodeOperationResult> HandoverDoctorAsync(
            Guid episodeId,
            HandoverDoctorRequest request,
            Guid actorUserId,
            bool actorIsWardHeadOrSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.DoctorId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Dokter penanggung jawab belum dipilih.");
            }

            if (!HasMeaningfulReason(request.HandoverReason))
            {
                return InpEpisodeOperationResult.Invalid(
                    "Alasan pengalihan DPJP wajib diisi dengan kalimat yang dapat dibaca.");
            }

            if (!actorIsWardHeadOrSupervisor)
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Pengalihan DPJP hanya dapat dilakukan kepala ruangan atau supervisor.");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu.",
                    episode);
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Closed)
            {
                return InpEpisodeOperationResult.Conflict("Episode sudah ditutup.", episode);
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                    episode);
            }

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.DoctorId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!doctorExists)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Dokter penanggung jawab yang dipilih tidak ditemukan atau tidak aktif.");
            }

            var activeAssignments = await _dbContext.Set<InpDoctorAssignment>()
                .Where(x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .ToListAsync(cancellationToken);

            if (activeAssignments.Count > 1)
            {
                // INV-INP-03 sudah dilanggar sebelum permintaan ini datang. Menambah
                // penugasan baru hanya akan memperdalam kerusakannya.
                return InpEpisodeOperationResult.Conflict(
                    "Episode ini punya lebih dari satu DPJP aktif. Hubungi supervisor untuk " +
                    "membetulkan riwayat penugasannya lebih dulu.",
                    episode);
            }

            var current = activeAssignments.FirstOrDefault();

            if (current != null && current.DoctorId == request.DoctorId)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Dokter yang dipilih sudah menjadi DPJP episode ini.",
                    episode);
            }

            var now = DateTime.UtcNow;
            var reason = request.HandoverReason.Trim();

            var lastSequence = await _dbContext.Set<InpDoctorAssignment>()
                .Where(x => x.EpisodeId == episode.Id)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                if (current != null)
                {
                    current.EndDateTime = now;
                    current.IsActive = false;
                    current.HandoverReason = Truncate(reason, 500);
                    current.UpdateDateTime = now;
                    current.UpdateBy = actorUserId;
                }

                var assignment = new InpDoctorAssignment
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    DoctorId = request.DoctorId,
                    SequenceNumber = lastSequence + 1,
                    StartDateTime = now,
                    EndDateTime = null,
                    AssignedByUserId = actorUserId,
                    HandoverReason = Truncate(reason, 500),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<InpDoctorAssignment>().Add(assignment);

                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(episode, "DPJP berhasil dialihkan.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Dua pengalihan pada saat hampir bersamaan. Unique index parsial
                // IX_InpDoctorAssignment_EpisodeId_Active menolak yang kalah, sehingga
                // INV-INP-03 tetap tidak pernah dilanggar.
                return InpEpisodeOperationResult.Conflict(
                    "Pengalihan DPJP lain sedang tersimpan untuk episode ini. Muat ulang " +
                    "layar lalu coba lagi.",
                    episode);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>Membaca riwayat penugasan DPJP satu episode, urut nomor urut.</summary>
        public async Task<List<InpatientDoctorAssignmentResponse>> GetDoctorAssignmentsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new InpatientDoctorAssignmentResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    SequenceNumber = x.SequenceNumber,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    IsCurrent = x.EndDateTime == null,
                    AssignedByUserId = x.AssignedByUserId,
                    HandoverReason = x.HandoverReason
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Menjawab siapa DPJP yang berwenang pada satu titik waktu.
        /// </summary>
        /// <remarks>
        /// Inilah yang membuat riwayat berperiode punya guna: pertanyaan "siapa yang
        /// berwenang pada 22 September" tetap dapat dijawab pada 25 September, walaupun DPJP
        /// sudah berganti dua kali sejak itu.
        /// </remarks>
        public async Task<InpatientDoctorAssignmentResponse?> GetDoctorAssignmentAtAsync(
            Guid episodeId,
            DateTime pointInTime,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.EpisodeId == episodeId &&
                    !x.IsDelete &&
                    x.StartDateTime <= pointInTime &&
                    (x.EndDateTime == null || x.EndDateTime > pointInTime))
                .OrderByDescending(x => x.SequenceNumber)
                .Select(x => new InpatientDoctorAssignmentResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    SequenceNumber = x.SequenceNumber,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    IsCurrent = x.EndDateTime == null,
                    AssignedByUserId = x.AssignedByUserId,
                    HandoverReason = x.HandoverReason
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>Identitas DPJP yang sedang berlaku, atau <c>null</c> bila tidak ada.</summary>
        public Task<Guid?> GetActiveDoctorIdAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<InpDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete)
                .OrderByDescending(x => x.SequenceNumber)
                .Select(x => (Guid?)x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Benar bila <paramref name="doctorId"/> adalah DPJP yang sedang berlaku pada episode
        /// tersebut. Inilah isi <c>GUARD-INP-01</c>, <c>GUARD-INP-02</c>, <c>GUARD-INP-03</c>,
        /// dan <c>GUARD-INP-04</c>.
        /// </summary>
        public async Task<bool> IsActiveDoctorAsync(
            Guid episodeId,
            Guid? doctorId,
            CancellationToken cancellationToken = default)
        {
            if (!doctorId.HasValue || doctorId.Value == Guid.Empty)
            {
                return false;
            }

            var activeDoctorId = await GetActiveDoctorIdAsync(episodeId, cancellationToken);

            return activeDoctorId.HasValue && activeDoctorId.Value == doctorId.Value;
        }

        // =====================================================================
        // BE-RWI-018 — Penugasan perawat penanggung jawab
        // =====================================================================

        /// <summary>
        /// Menugaskan perawat penanggung jawab. Penugasan sebelumnya ditutup dan penugasan
        /// baru dibuka pada tindakan yang sama.
        /// </summary>
        /// <remarks>
        /// <b>Ketiadaan perawat tidak menahan apa pun.</b> Episode boleh berjalan tanpa
        /// perawat penanggung jawab: penempatan, perpindahan, dan keputusan pulang semuanya
        /// tetap berhasil. Penugasan perawat sering menyusul beberapa menit setelah pasien
        /// tiba, dan menahan pekerjaan sampai ia terisi hanya memindahkan antrean ke tempat
        /// lain — <c>RWI-DEC-032</c>. Yang muncul untuk episode tanpa perawat adalah daftar
        /// pantau kepala ruangan, bukan penolakan.
        /// </remarks>
        public async Task<InpEpisodeOperationResult> AssignNurseAsync(
            Guid episodeId,
            AssignNurseRequest request,
            Guid actorUserId,
            bool actorIsWardHeadOrSupervisor,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.EmployeeId == Guid.Empty)
            {
                return InpEpisodeOperationResult.Invalid("Perawat penanggung jawab belum dipilih.");
            }

            if (!actorIsWardHeadOrSupervisor)
            {
                return InpEpisodeOperationResult.Forbidden(
                    "Penugasan perawat penanggung jawab hanya dapat dilakukan kepala ruangan " +
                    "atau supervisor.");
            }

            var episode = await LoadEpisodeForWriteAsync(episodeId, cancellationToken);

            if (episode == null)
            {
                return InpEpisodeOperationResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (await ExpireDraftIfDueAsync(episode, cancellationToken))
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah gugur karena ditinggalkan melewati batas waktu.",
                    episode);
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Closed)
            {
                return InpEpisodeOperationResult.Conflict("Episode sudah ditutup.", episode);
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return InpEpisodeOperationResult.Conflict(
                    "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
                    episode);
            }

            var employeeExists = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmployeeId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (!employeeExists)
            {
                return InpEpisodeOperationResult.Invalid(
                    "Perawat yang dipilih tidak ditemukan atau tidak aktif.");
            }

            var activeAssignments = await _dbContext.Set<InpNurseAssignment>()
                .Where(x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var current = activeAssignments
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefault();

            if (current != null && current.EmployeeId == request.EmployeeId)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Perawat yang dipilih sudah menjadi penanggung jawab episode ini.",
                    episode);
            }

            var now = DateTime.UtcNow;

            var lastSequence = await _dbContext.Set<InpNurseAssignment>()
                .Where(x => x.EpisodeId == episode.Id)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var assignment in activeAssignments)
                {
                    assignment.EndDateTime = now;
                    assignment.IsActive = false;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                }

                var newAssignment = new InpNurseAssignment
                {
                    Id = Guid.NewGuid(),
                    EpisodeId = episode.Id,
                    EmployeeId = request.EmployeeId,
                    SequenceNumber = lastSequence + 1,
                    StartDateTime = now,
                    EndDateTime = null,
                    AssignedByUserId = actorUserId,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<InpNurseAssignment>().Add(newAssignment);

                episode.UpdateDateTime = now;
                episode.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return InpEpisodeOperationResult.Success(
                    episode,
                    "Perawat penanggung jawab berhasil ditugaskan.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                return InpEpisodeOperationResult.Conflict(
                    "Penugasan perawat lain sedang tersimpan untuk episode ini. Muat ulang " +
                    "layar lalu coba lagi.",
                    episode);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>Membaca riwayat penugasan perawat satu episode, urut nomor urut.</summary>
        public async Task<List<InpatientNurseAssignmentResponse>> GetNurseAssignmentsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<InpNurseAssignment>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .Select(x => new InpatientNurseAssignmentResponse
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.FullName : null,
                    SequenceNumber = x.SequenceNumber,
                    StartDateTime = x.StartDateTime,
                    EndDateTime = x.EndDateTime,
                    IsCurrent = x.EndDateTime == null,
                    AssignedByUserId = x.AssignedByUserId
                })
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Keterangan episode pasien yang sedang hadir di ruangan, dipakai menyusun kalimat
    /// penolakan <c>INV-INP-10</c>.
    /// </summary>
    /// <remarks>
    /// Kalimatnya menyebut nomor episode dan lokasi supaya petugas langsung tahu bahwa yang
    /// dibutuhkan adalah perpindahan, bukan admisi baru. Penolakan tanpa keduanya memaksa
    /// petugas mencari sendiri, dan pencarian itulah yang biasanya berakhir dengan admisi
    /// kedua.
    /// </remarks>
    public sealed record InpPresentEpisodeInfo(
        Guid EpisodeId,
        string EpisodeNumber,
        string? PatientName,
        string? BedName,
        string? RoomName)
    {
        /// <summary>Lokasi terbaca, misalnya <c>Melati 3 3B</c>.</summary>
        public string LocationText =>
            string.IsNullOrWhiteSpace(RoomName) && string.IsNullOrWhiteSpace(BedName)
                ? "tempat tidur yang belum tercatat"
                : string.Join(" ", new[] { RoomName, BedName }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

        /// <summary>Kalimat penolakan sebagaimana ditetapkan validation matrix bagian 3.</summary>
        public string RejectionMessage =>
            $"{(string.IsNullOrWhiteSpace(PatientName) ? "Pasien ini" : PatientName)} sudah " +
            $"dirawat pada episode {EpisodeNumber} di {LocationText}. Bila memang pindah kamar, " +
            "pakai perpindahan, bukan admisi baru.";
    }
}
