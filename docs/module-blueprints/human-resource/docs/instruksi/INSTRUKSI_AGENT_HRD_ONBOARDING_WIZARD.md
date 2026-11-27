# MASTER INSTRUKSI AGENT - MULTI-STEP ONBOARDING PEGAWAI BARU QUILVIAN V2

**Tanggal:** 25 November 2026 | **Status requirement:** planning tujuh tahapan disetujui pengguna dalam percakapan.  
**Target:** Sumber Daya Manusia > Siklus Kepegawaian > Penerimaan & Onboarding > Kasus Onboarding > Detail Kasus.  
**Slice canonical:** `S-C4-ONB` (Onboarding); integrasi `S-D2` (Rekrutmen dan Hiring).  
**Jenis:** satu master instruksi implementasi langsung, bukan laporan pekerjaan atau klaim UAT.  
**Urutan:** backend terlebih dahulu, frontend setelah kontrak stabil, integrasi, UAT live browser, perbaikan, retest.  
**Status awal:** `PARTIAL / REPAIR / MISSING` untuk wizard; layanan existing `IMPLEMENTED_NOT_VERIFIED` pada snapshot audit.  
**Lokasi dokumen yang diusulkan:** `docs/module-blueprints/human-resource/docs/update/penerimaan-dan-onboarding/INSTRUKSI_AGENT_HRD_ONBOARDING_WIZARD.md` (hanya jika izin tulis dokumen tersedia).

## 1. Role

Anda adalah Senior Fullstack Engineer Quilvian V2 (ASP.NET Core/EF Core, Next.js/React), analis proses bisnis HRD rumah sakit, UI/UX Engineer, dan QA/UAT Engineer. Implementasikan fitur nyata berdasarkan keputusan pengguna; jangan hanya mengaudit, membuat mockup, atau menghasilkan planning ulang. Semua komunikasi dan laporan dalam bahasa Indonesia.

| Repository | Branch | SHA sumber yang diperiksa | Peran |
|---|---|---|---|
| `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `ba6671d01ecc87e826ef01065cc7d8a9e36141ce` | Backend, kontrak, blueprint |
| `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `362eaa2185e1511ef3723bba481cdea77d841ed5` | Frontend |
| `DevBenari/QuilvianEngineeringSkills` | `main` | `62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5` | Governance, read-only |

Tulis hanya di workspace lokal dan mode yang sah. Jangan mengubah branch secara otomatis. Jangan commit, push, pull, merge, rebase, deploy, membuat atau menjalankan migration, atau menjalankan perintah langsung terhadap DB tanpa izin tersendiri. Pengujian mutasi melalui API/browser hanya pada environment dev/test yang diizinkan dengan data sintetis. Jangan sentuh produksi. Jangan memodifikasi repository Engineering Skills. Ikuti `AGENTS.md`, `CLAUDE.md`, dan canonical governance. Prompt ini secara eksplisit meminta skill runtime `quilvian-engineering-skills:build-module-backend` dan `quilvian-engineering-skills:build-module-frontend` sesuai fase; jika skill yang diminta tidak tersedia, laporkan blocker skill, bukan invokasi palsu. Jika canonical governance tidak tersedia, `BLOCKED - canonical governance unavailable`.

## 2. Task

### 2.1 Hasil yang harus diwujudkan

Setelah Recruitment Journey berhasil menyerahkan kandidat, HRD membuka kasus onboarding yang sudah terbentuk. Satu halaman memperlihatkan progres, data sumber, pekerjaan tersisa, aktor yang berwenang, dan aksi atau tautan kontekstual. Data yang disimpan pada satu tahap terlihat kembali setelah reload dan oleh petugas lain yang berwenang. Wizard bukan form sekali submit, melainkan ruang kerja berkelanjutan.

```text
Sumber Daya Manusia
  Siklus Kepegawaian
    Penerimaan & Onboarding
      Perlu Tindakan
      Onboarding
      Kasus Onboarding
        Detail Kasus [Onboarding Wizard - 7 tahap]
      Template Onboarding
      Pendampingan Klinis
      Permintaan Akses IT
      Pengaturan Lifecycle
```

Pertahankan tujuh submenu existing dan labelnya. Registry lama Menu 03 memuat Onboarding; struktur sidebar terbaru telah mengelompokkan fungsi onboarding ke Penerimaan & Onboarding. Jangan mengembalikan struktur lama. Scope instruksi ini adalah fitur Wizard pada Kasus Onboarding dan koneksi tujuh submenu di atas, bukan perombakan semua submenu atau seluruh HRD. Jangan menambah sidebar, mengganti tampilan global, menduplikasi master data/kontrak, atau mengubah fitur Payroll/Administrator/Master Data yang tidak relevan. Recruitment Journey dan Multi-Step Pendaftaran tetap berfungsi; jangan menimpa pekerjaan Agent sebelumnya.

### 2.2 Keputusan bisnis yang terkunci

| ID / sumber | Ketentuan |
|---|---|
| Persetujuan pengguna 25 Nov 2026 | Tujuh tahap: Konfirmasi Penerimaan; Kelengkapan Data Pegawai; Perjanjian Kerja; Penempatan Pegawai; Orientasi & Pendampingan; Permintaan Akses IT; Verifikasi Siap Kerja. Layout stepper, konten tengah, side section; reuse data existing; backend dahulu. |
| `HRD-DEC-083` | Kasus terbentuk otomatis dari handoff, data di-prefill; koordinator mengikuti petugas; Batal Bergabung hanya sebelum kehadiran hari pertama, dengan konsekuensi canonical. |
| `HRD-DEC-084` / `095` | Draft kontrak pertama dari offer yang disetujui, tanpa approval kedua; scan bertanda tangan wajib. Perubahan substansi kontrak mengikuti mekanisme offer, bukan edit diam-diam. |
| `HRD-DEC-086` | Template umum + klinis jika berlaku + unit jika tersedia; snapshot kasus tidak berubah retroaktif. Butir HRD dan Kepala Unit diselesaikan pihak yang berwenang; manual, bukti, otomatis dan butir pemilihan orang sesuai kontrak. Tidak membuat approval baru per butir. |
| `HRD-DEC-087` | Preceptor ditetapkan sebelum siap kerja untuk tenaga klinis. Pendampingan berlangsung setelah siap kerja. Siap Mandiri adalah keputusan Kepala Unit terpisah; kredensial mengikuti kontrak dan keputusan terkini, bukan izin klinis baru. |
| `HRD-DEC-088` | Siap kerja oleh HRD setelah seluruh butir wajib terpenuhi. Butir opsional boleh diselesaikan kemudian. Account creation saat kasus dibuat, ESS setelah hadir, akses kerja setelah siap kerja, status didampingi untuk klinis, gaji sejak mulai kontrak. |
| `HRD-DEC-096` | Atasan langsung dipilih Kepala Unit sebagai butir sistem; penempatan awal dan gaji berasal dari offer; Perlu Tindakan sebagai worklist; sertifikat otomatis dari sumber existing; riwayat kontrak bukan sumber tulis kedua. |
| `VR-ONB-03/04/06/10/11/12/16/17` | Pembagian otoritas, butir otomatis tidak dicentang manual, tanggal hadir tidak mendahului rencana, checklist wajib selesai, pemisahan aktor, batal bergabung sebelum hadir, pembatalan hadir hanya sebelum kontrak pertama sah. |

Jaga keputusan tersebut. Revisi decision log terbaru bila ada superseding decision yang benar-benar berlaku. Persetujuan wizard di chat tidak berarti kontrak canonical telah disinkronkan. Jangan mengarang nomor keputusan resmi baru. Klasifikasi data tambahan di tahap 2 sebagai informasi kelengkapan kecuali persyaratan wajibnya berasal dari template atau keputusan approved; jangan membuat hambatan siap kerja baru hanya karena sebuah field kosong.

**Model progres:** tahapan adalah pengelompokan pekerjaan, bukan tujuh transaksi approval berurutan. Orientasi, profil, dan kontrak dapat berjalan paralel selama prasyarat domain dipenuhi. Pengguna boleh meninjau tahapan manapun yang berhak dibaca. Klik stepper tidak memutasi status, tidak menyelesaikan butir, dan tidak mengaktifkan akun. Tahap akses IT dapat berisi pekerjaan pasca-siap kerja; tidak menjadi circular dependency. Hanya backend menentukan status, izin, blocker, dan kesiapan. Perubahan navigasi tetap pada halaman kasus existing.

### 2.3 Tujuh tahap yang harus diimplementasikan

#### Step 01 - Konfirmasi Penerimaan

- **Tujuan/aktor:** HRD memastikan hasil rekrutmen valid dan mencatat hadir hari pertama; Kepala Unit melihat pegawai masuk pada scope unitnya.
- **Input:** `caseId`, `HiringHandoffId`, `WorkforceProfileId`, data kandidat, offer, unit, posisi, site, tanggal rencana mulai, koordinator, clearance ringkas sesuai izin. Jangan meminta isi ulang NIK atau membuat pegawai kedua. Pastikan rehire mengacu profil yang benar.
- **Reuse:** `HrdOnboardingCaseService.CreateFromHandoffAsync`, `GetByIdAsync`, `RecordFirstDayAttendanceAsync`, `CancelFirstDayAttendanceAsync`, `CancelJoinAsync`; `LifecycleService.getCaseById`, `recordFirstDayAttendance`, `cancelJoin`. Pada Recruitment Journey hanya tautkan kasus yang benar-benar ada; jika gagal dibuat setelah handoff, tampilkan blocker dan mekanisme recovery existing atau perbaikan idempotent yang sah. Jangan menampilkan onboarding sukses dari status Hired saja.
- **FE:** ringkasan data penerimaan baca-saja, status handoff, rencana vs tanggal hadir, koordinator; aksi kontekstual Catat Hadir, pembatalan pencatatan bila kontrak masih Draft dan berwenang, Batal Bergabung bila belum hadir, ganti koordinator jika tersedia dan sah. Dialog tanggal dan alasan memakai base component. Tidak boleh mencatat kehadiran hanya karena klik Selanjutnya.
- **BE:** revalidasi actor dan scope, tanggal rencana, status, row version/concurrency; jangan izinkan hadir ganda atau pembatalan yang menghindari proses keluar resmi; pastikan aksi batal menutup kasus/kontrak draft, status dan kursi WFP sesuai kontrak, audit dan konsistensi efek. Pekerjaan lintas domain di luar scope yang belum berizin dilaporkan, bukan diubah sembarang.
- **Selesai:** case/profil/handoff terhubung; catat hadir persisten, status dan tindakan aktual setelah refresh; pembatalan sebelum hadir mengikuti efek existing. UAT-ONB-01..05, 23..25.

#### Step 02 - Kelengkapan Data Pegawai

- **Tujuan/aktor:** HRD melengkapi data personal dan administratif pada profil existing, bukan registrasi pegawai baru.
- **Kelompok:** identitas/kontak dari rekrutmen; keluarga; pendidikan; riwayat pekerjaan; dokumen personal; data profesi; kontak darurat; rekening/profil penggajian hanya bila relevan dan diizinkan. Data yang belum diketahui jangan dianggap lengkap. Mask NIK/rekening; jangan menampilkan data pribadi sensitif kepada petugas yang tidak berwenang.
- **Reuse:** `WorkforceProfileId` sebagai identitas sumber; verifikasi relasi ke master pegawai/dokter, halaman profil existing dan API pemilik domain pada source aktual. Jangan mengasumsikan endpoint/route edit atau field yang belum ditemukan. Gunakan deep link edit berdasarkan ID yang benar dan akses yang berlaku, lalu kembali ke `caseId`. Jika hanya ada halaman baca atau tidak ada izin, tampilkan informasi/tautan ke petugas pemilik yang sah. Jangan mengubah layout Master Data existing.
- **FE:** satu panel kelengkapan, baris per kelompok dengan status lengkap/perlu dilengkapi/tidak berlaku/belum dapat diverifikasi/akses terbatas, tautan yang tepat. Gunakan form existing; jika reuse komponen form aman dan diizinkan, boleh inline terbatas tanpa menyalin logika. Jangan membuat field dummy atau menyimpan PII ke localStorage.
- **BE:** agregasi ringkas dengan pemeriksaan izin per domain; jangan menyalin profil ke tabel baru, jangan membuat CRUD kedua; perbaiki relasi hanya bila dibutuhkan untuk kasus ini, kontrak dan hak tulis sah. Kelengkapan harus berasal dari field wajib existing atau template; unknown bukan gagal/selesai otomatis. Re-fetch setelah perubahan profil.
- **Selesai:** perubahan tersimpan pada sumber yang tepat, dibaca kembali setelah navigasi/refresh; data gaji dan rekening tidak bocor; tidak membuat profil ganda. UAT-ONB-06..08, 30.

#### Step 03 - Perjanjian Kerja

- **Tujuan/aktor:** HRD memproses kontrak pertama yang berasal dari offer, dengan persetujuan sebelumnya tetap dihormati.
- **Input:** ID kontrak pertama terkait handoff/offer/case; jenis kontrak, tanggal mulai/akhir bila berlaku, durasi sesuai offer, status, scan tanda tangan. Rehire mungkin memiliki kontrak historis; jangan mengambil kontrak aktif lama tanpa verifikasi kaitan. Nominal gaji hanya untuk `WorkforceEmploymentContract:ViewAmount` sesuai masking canonical.
- **Reuse:** `HrdEmploymentContractController`, `LifecycleService.getContractById` atau nama method existing yang diverifikasi, `/print-data`, `/record-signed`, `/download-scan`, `CheckAutomaticItemsAsync`. Gunakan draft dari `CreateDraftFromOfferAsync`; jangan create draft kedua atau approval baru. Jika draft tidak ada/gagal, perbaiki pemulihan idempotent dengan kontrak resmi, bukan bikin kontrak random.
- **FE:** ringkasan nomor, jenis, periode dan status kontrak; tombol Buka Perjanjian Kerja, Cetak sesuai izin, unggah/record signed melalui form existing. Setelah berhasil refresh data kontrak dan butir otomatis. Jangan tampilkan print payload, nominal atau unduhan scan tanpa hak yang diperlukan; pastikan return ke wizard.
- **BE:** pastikan kontrak terkait memang bertanda tangan, dokumen bukti tersimpan dan sah menurut contract service; `Active` atau checklist Complete saja tidak cukup bila bukti tidak ada. Pemeriksaan siap kerja mengacu kontrak pertama yang benar dan validitas sekarang, bukan kontrak lain. Cek hasil pembuatan draft; kegagalan harus ditangani/terlihat, bukan false success. Jangan mengubah isi offer/gaji/aturan PKWT/PKWTT.
- **Selesai:** kontrak draft belum siap kerja; signed berhasil persisten; checklist sistem merefleksikan status nyata; scan yang salah/hilang ditolak. UAT-ONB-09..12, 26.

#### Step 04 - Penempatan Pegawai

- **Tujuan/aktor:** periksa penempatan awal dari offer dan pilih atasan langsung melalui Kepala Unit tujuan.
- **Input:** site, organization unit, position, lokasi, uraian tugas existing bila tersedia, atasan langsung, gaji awal (restricted). Verifikasi relasi/master dan tanggal efektif sesuai canonical. Jangan membuat penempatan/mutasi kedua untuk pegawai yang sudah memiliki penempatan dari handoff.
- **Reuse:** item `EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned` di `CompleteItemAsync`; `SelectedWorkforceProfileId`; `WfpManagerAssignment` yang dibentuk pada declare-ready. Gunakan selector pegawai yang sah dan pola resource select existing, bukan input UUID. Tautkan uraian tugas/penempatan existing sesuai akses.
- **FE:** tampilkan ringkasan dari sumber, status penetapan atasan dan penanggung jawab; aksi Pilih Atasan hanya untuk actor yang diizinkan; hasil pemilihan dan bukti audit dari backend. Tidak memberi kemampuan edit offer lewat wizard.
- **BE:** validasi profil atasan benar-benar ada, eligible/aktif sesuai kebijakan, tidak self-assignment, unit/kewenangan sesuai kontrak. Validasi actor unit tujuan dan larangan siap kerja oleh actor yang sama dengan pemilih atasan. Jika relasi manager existing berbeda, deteksi konflik dan gunakan resolusi resmi, jangan diam-diam menyatakan penempatan sukses atau overwrite. Cegah duplikasi assignment pada retry/concurrency.
- **Selesai:** atasan dipilih pihak sah; siap kerja membuat/mengakui relasi yang benar tepat satu kali; petugas berbeda untuk deklarasi. UAT-ONB-13..16, 27.

#### Step 05 - Orientasi & Pendampingan

- **Tujuan/aktor:** HRD dan Kepala Unit menyelesaikan pekerjaan sesuai penanggung jawab; tenaga klinis disiapkan untuk mulai bekerja didampingi.
- **Input:** snapshot item kasus dari template General, Clinical, Unit; `itemId`, `ItemKind`, `SystemItemCode`, `IsRequired`, status, role, due date, bukti, selected person, audit. Jangan mengubah snapshot lama saat template baru diedit. Butir yang tidak cocok dengan kategori khusus tetap terlihat di orientasi (jangan hilang). Hitung checklist dari ID unik; item yang direferensikan pada tahap lain tidak dihitung ganda.
- **Reuse:** `HrdOnboardingCaseService.CompleteItemAsync`, `UncompleteItemAsync`, `CheckAutomaticItemsAsync`, `DownloadItemEvidenceAsync`, `HrdClinicalPreceptorshipController`, `LifecycleService.completeChecklistItem`, `uncheckChecklistItem`, `checkAutomaticItems`, layanan preceptor existing. Kelompokkan berdasarkan kode sistem, sumber dan template layer, tidak berdasarkan string judul semata.
- **FE:** filter Lapis/Semua, Wajib/Opsional, Belum/Selesai; tabel kecil dengan nama, penanggung jawab, batas, status, bukti, aksi sesuai izin. Butir biasa pakai konfirmasi; butir Evidence wajib file; butir person menggunakan resource selector; otomatis baca-saja dengan tombol Periksa Butir Otomatis. Tampilkan alasan gagal dan tombol ulang yang aman. Preceptor hanya untuk klinis. Daftar tugas opsional tetap tersedia pada kasus Completed.
- **BE:** validasi kind dan evidence server-side (bukan hanya UI); validasi kepemilikan item pada case; validasi role/unit pada complete/uncomplete/evidence. Perkuat batas status: kasus Cancelled tidak boleh dimutasi; Completed dapat menyelesaikan butir opsional tersisa sesuai keputusan; required dan butir sistem yang menjadi dasar siap kerja tidak boleh di-uncomplete sembarangan sesudah siap kerja. Koreksi formal mengikuti kontrak; jangan melakukan deactivation otomatis. Periksa file type/size/signature, storage key, authorization, audit, penyimpanan dan failure cleanup. Pastikan butir otomatis berasal dari data nyata, tidak dapat dipalsukan lewat request, stale result tidak dipakai untuk declare-ready. Pastikan template umum yang hilang atau butir wajib rusak dilaporkan sebagai blocker; jangan silently retroaktif menerapkan template baru ke kasus lama. Gunakan recovery resmi yang disetujui bila diperlukan.
- **Klinis:** penetapan preceptor oleh Kepala Unit yang berwenang, linkage aktual ke kasus. Setelah siap kerja, status Accompanied/Didampingi; tombol Siap Mandiri tetap di proses Pendampingan Klinis, bukan aksi HRD wizard. Jangan mewajibkan seluruh periode pendampingan selesai sebelum siap kerja. Persyaratan kredensial dan pembatasan kerja mengikuti owner/canonical terkini.
- **Selesai:** setiap jenis item diuji, bukti tersimpan dan terbaca sah, preceptor valid, opsional dapat diselesaikan pasca-ready, klinis tidak otomatis mandiri. UAT-ONB-17..22, 28..29.

#### Step 06 - Permintaan Akses IT

- **Tujuan/aktor:** HRD memantau readiness akun; Administrator/IT mengeksekusi sesuai permission. HRD tidak memperoleh hak Administrator karena wizard.
- **Input:** access request existing berdasarkan case dan profil, jenis `AccountCreation`/`WorkAccess`, status, waktu, alasan gagal yang aman, aktor, hasil yang telah dikonfirmasi. Jangan expose password, token, raw exception, diagnosis, role assignment detail tanpa izin.
- **Reuse:** `HrdOnboardingAccessRequestController`, `LifecycleService` list/complete/retry existing, `OnboardingCaseDetailResponse.AccessRequests`. Tautkan ke halaman Permintaan Akses IT dengan filter kasus jika didukung. Aksi retry/complete hanya dengan hak dan validasi layanan pemilik.
- **BE:** case creation meminta akun, hadir pertama membuka ESS sesuai kontrak, setelah declare-ready menerbitkan permintaan akses kerja; jangan mengaktifkan klinis sebelum waktunya. Verifikasi integrasi nyata, request unik, idempotent retry, sanitasi pesan gagal, otorisasi dan audit. Jangan membentuk akun/role kedua dari wizard. Jika integrasi belum dapat diverifikasi, tampilkan status faktual Requested/Failed/dll, bukan Completed palsu.
- **FE:** timeline/tabel status per jenis akses; bedakan account exists, ESS active, WorkAccess requested vs completed. Jika belum waktunya, tampilkan alasan; tautkan ke petugas IT. Tidak ada tombol paksa aktifkan akun atau edit role.
- **Penting:** penyelesaian WorkAccess tidak memblokir siap kerja karena permintaannya dibuat setelah siap kerja. Tahap ini dapat memiliki follow-up setelah kasus Completed.
- **Selesai:** akses bertahap, status gagal dan retry aman, pegawai sebelum siap kerja tidak memperoleh akses kerja; UAT-ONB-31..34.

#### Step 07 - Verifikasi Siap Kerja

- **Tujuan/aktor:** HRD memeriksa kesiapan dan menyatakan siap kerja dengan otoritas terpisah, bukan otomatis ketika bar terakhir hijau.
- **Input:** hasil readiness backend, daftar butir wajib dan opsional, kehadiran, signed contract dari handoff, clearance FIT/FIT_WITH_RESTRICTIONS dan tindak lanjut pembatasan sesuai kontrak, atasan, preceptor jika klinis, status kasus dan actor.
- **BE:** bangun satu evaluator readiness yang dipakai baik untuk response agregasi maupun sebelum mutasi `DeclareReadyForWorkAsync`. Recompute fakta canonical dari database dalam batas transaksi dan concurrency; jangan hanya memakai boolean checklist tersimpan. Verifikasi `InProgress`, hadir, signed contract terkait, fitness yang diterima dan aturan pembatasan, item wajib dan template/system items yang benar-benar tersedia, atasan valid, preceptor jika klinis, actor HRD dan tidak sama dengan pemilih atasan. Jika belum memenuhi, kembalikan alasan yang aman; jangan mengaktifkan profil sebagian. Cegah all-required vacuous truth (`All()` pada set kosong), item sistem opsional yang salah, dan stale status otomatis.
- **BE efek sukses:** case Completed, `ReadyForWorkAt/By` audit; profil aktif sesuai canonical, penempatan/manager terhubung; klinis Accompanied bukan Independent; WorkAccess request tepat satu dan status sesuai transaksi. Pastikan consistency apabila perubahan preceptor atau akses gagal, hindari save parsial; gunakan transactional pattern existing, external side effect melalui mekanisme retry/reconciliation yang sah. Jangan klaim WorkAccess sudah Completed hanya karena request dibuat. Repeated/stale request harus ditolak atau mengembalikan existing result sesuai kontrak, tidak menggandakan efek. Verifikasi downstream schedule eligibility dan payroll eligibility melalui kontrak existing; jangan ubah rumus Payroll.
- **FE:** daftar persyaratan real read-only, pisahkan wajib/opsional dan blocker actor; aksi Nyatakan Siap Kerja berada pada side section dengan dialog final dan catatan sesuai DTO. Jangan gunakan checkbox UI untuk mengubah status readiness. Setelah request sukses, muat ulang case, status profil, dan akses. Jika masih gagal, pertahankan layar dan tampilkan informasi yang dapat ditindaklanjuti. Pada Completed tampilkan hasil, item opsional tersisa, preceptor/akses lanjutan; tidak boleh declare-ready lagi.
- **Selesai:** semua gate diuji langsung API dan browser; efek state persisten tepat satu, no clinical independence, no unauthorized work access. UAT-ONB-26..29, 34..40.

### 2.4 Pemetaan tujuh submenu existing (batas integrasi)

Berikut seluruh submenu grup Penerimaan & Onboarding yang telah dipakai pada sidebar terbaru. Langkah di atas bukan submenu baru. Lakukan regresi sesuai keterkaitan, bukan rewrite masing-masing halaman.

### Submenu 1 - Perlu Tindakan
- Route `/hr/lifecycle/worklist`. Tujuan: petugas melihat pekerjaan yang membutuhkan tindakan; prioritas/status berdasarkan sumber backend. Reuse worklist existing, tautkan kasus yang sah bila action mengacu onboarding case. Pastikan pekerjaan wajib/opsional dan akses yang gagal tidak hilang karena wizard. Jangan menambah engine notifikasi baru. Acceptance: klik row berizin membuka case benar, kembali ke worklist, status terbaru sesuai sumber (UAT-ONB-35).

### Submenu 2 - Onboarding
- Route `/hr/recruitment/handoffs`. Tujuan: HR melakukan serah terima rekrutmen. Reuse `HiringHandoffService` dan endpoint generate/confirm-transfer existing, tidak melakukan handoff baru di wizard. Tautan ke case aktual; jika case hilang atau integrasi gagal, tampilkan error dan recovery yang berizin/idempotent. UAT-ONB-01/02/36. Tidak memodifikasi aturan rekrutmen yang sedang dikerjakan Agent lain tanpa kebutuhan terverifikasi.

### Submenu 3 - Kasus Onboarding
- Route `/hr/lifecycle/onboarding-cases` dan `/{id}`. Target utama implementasi tujuh tahap. List existing tetap; detail kasus menjadi wizard, state server-based, akses sesuai scope, loading/empty/error, aksi di side section, navigasi kontekstual. BE agregasi dan penguatan guard pada case service. Acceptance semua UAT terkait wizard dan regresi list.

### Submenu 4 - Template Onboarding
- Route `/hr/lifecycle/onboarding-templates`. Tujuan: HRD mengatur template umum, klinis, unit sesuai canonical. Reuse template existing; jangan menulis ulang admin template. Wizard membaca snapshot, not live template. Jika konfigurasi wajib tidak ada, berikan tautan berizin dan blocker; jangan menyelesaikan butir palsu atau menambahkan item ke semua kasus historis. UAT-ONB-19/20.

### Submenu 5 - Pendampingan Klinis
- Route `/hr/lifecycle/clinical-preceptorships`. Tujuan: Kepala Unit menetapkan pendamping dan menyatakan siap mandiri melalui workflow terpisah. Reuse `HrdClinicalPreceptorshipController`; case menampilkan preceptor, status, tautan sesuai scope. Clinical-only; jangan buat izin tindakan klinis baru. UAT-ONB-21/22/28/29.

- BE wajib memeriksa kepemilikan `caseId`, status pendampingan dan scope unit sebelum perubahan; FE tidak membuka aksi deklarasi mandiri jika proses masih Pending Onboarding atau actor bukan pemilik kewenangan.

### Submenu 6 - Permintaan Akses IT
- Route `/hr/lifecycle/onboarding-access-requests`. Tujuan: pemilik akses menyelesaikan/retry permintaan. Reuse `HrdOnboardingAccessRequestController` dan UI existing. HRD hanya melihat ringkasan atau bertindak jika hak secara eksplisit mengizinkan; tidak edit role. Acceptance urutan account/ESS/work access dan failure/retry. UAT-ONB-31..34.

- Validasi backend atas status request dan actor pemilik tetap berlaku pada direct API. FE membedakan akun sudah dibuat dari akses kerja sudah diberikan; jangan menampilkan pesan sukses sebelum konfirmasi layanan sumber.

### Submenu 7 - Pengaturan Lifecycle
- Route `/hr/lifecycle/settings`. Tujuan: pengelolaan kebijakan lifecycle existing. Baca setting yang relevan jika memang digunakan service; tidak menciptakan konfigurasi baru atau mengubah kebijakan global untuk wizard. Jika kontrak setting menjadi blocker, jelaskan kepada petugas berwenang. Acceptance nilai existing tidak berubah oleh wizard; akses menu existing tetap bekerja (UAT-ONB-37).

## 3. Context

### 3.1 Bukti source pada snapshot yang dipin

- [FE halaman detail kasus](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/362eaa2185e1511ef3723bba481cdea77d841ed5/src/app/hr/lifecycle/onboarding-cases/%5Bid%5D/page.jsx) merender `OnboardingCaseDetailView`.
- [FE view existing](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/362eaa2185e1511ef3723bba481cdea77d841ed5/src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx): `getCaseById`, `availableActions`, `canRead/canComplete/canDeclare/canCancel`; tabel checklist, data kasus, akses; tombol aksi pada Hero; aksi item dibatasi `caseStatus === 1`; `hasGeneralTemplate` hanya warning; modal evidence dan person existing.
- [FE service existing](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/362eaa2185e1511ef3723bba481cdea77d841ed5/src/lib/services/hr/lifecycle-service.js): endpoint onboarding, kontrak, preceptor, access request. Reuse `LIFECYCLE_ROUTES`, `LIFECYCLE_PERMISSIONS`, constants/maps yang sudah ada. Baca file asli untuk nama method dan props aktual.
- [BE case service](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/ba6671d01ecc87e826ef01065cc7d8a9e36141ce/Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdOnboardingCaseService.cs): `CreateFromHandoffAsync` (74 dst; template merge dan fallback system items, draft kontrak, account request); `GetByIdAsync` (476 dst); `CompleteItemAsync` (654 dst); `UncompleteItemAsync` (775 dst); `CheckAutomaticItemsAsync` (826 dst); `DeclareReadyForWorkAsync` (948 dst); `CancelJoinAsync` (1072 dst); `MapToDetailResponse` (1228 dst).
- [BE controller kasus](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/ba6671d01ecc87e826ef01065cc7d8a9e36141ce/Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdOnboardingCaseController.cs): endpoint dan permission pada tabel 3.3.
- [BE keputusan canonical rev 44](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/ba6671d01ecc87e826ef01065cc7d8a9e36141ce/docs/module-blueprints/human-resource/00-interview-decisions.md): DEC-083, 084, 086, 087, 088, 095, 096; bagian 5155 dst, 5335 dst, 5452 dst, 5795 dst. Baca decision superseding/kontrak terbaru sebelum implementasi.
- [BE handoff](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/ba6671d01ecc87e826ef01065cc7d8a9e36141ce/Areas/Corporate/HumanResource/RecruitmentManagement/Services/HiringHandoffService.cs): penciptaan kasus setelah handoff; perhatikan hasil gagal case creation.
- Root BE relevan: `Areas/Corporate/HumanResource/LifecycleManagement/` dengan `Controllers/HrdEmploymentContractController.cs`, `Controllers/HrdClinicalPreceptorshipController.cs`, `Controllers/HrdOnboardingAccessRequestController.cs`, DTO/model/service yang terkait. Entitas terlihat: `HrdOnboardingCase`, `HrdOnboardingCaseItem`, `HrdOnboardingAccessRequest`, `MstOnboardingTemplate`, `MstOnboardingTemplateTask`, `WfpContractHistory` melalui DbSet `WfpContractHistories`, `WfpManagerAssignment`. Cocokkan nama file/model di workspace; jangan buat duplikat.
- Governance: FE/BE `AGENTS.md` dan `CLAUDE.md`, Engineering Skills `agents/rules/GLOBAL_RULES.md`, rule base component, frontend architecture, API, DB, review, test dan output; snapshot branch sama dengan audit sebelumnya. Referensi format dua dokumen Menu 06 dan 12 dalam folder `docs/update/`, bukan sumber keputusan wizard.

### 3.2 Temuan, prioritas, dan tindakan

| ID | Status sumber | Risiko / bukti | Tindakan |
|---|---|---|---|
| ONB-G01 | MISSING | Detail existing satu halaman checklist tanpa 7 step | Tambahkan stage layout pada detail existing. |
| ONB-G02 | REPAIR | `DeclareReadyForWorkAsync` memeriksa daftar required dan hadir; `MapToDetailResponse` menggunakan `.All()`; tidak terlihat pemeriksaan langsung signed contract/template/fitness pada gate | Evaluator shared, check sumber aktual, tolak missing/misconfigured system items dan status stale. |
| ONB-G03 | REPAIR | Template umum bisa tidak ada; fallback system items memang ada, tetapi checklist orientasi bisa tidak lengkap; FE hanya warning | Blokir false-ready bila snapshot wajib tidak sah. Tidak retroaktif mengubah template kasus lama; recovery yang berizin. |
| ONB-G04 | REPAIR | `CompleteItemAsync` memberi bypass HR/Admin pada butir UnitHead; `UncompleteItemAsync` perlu cek role/status setara | Rekonsiliasi DEC-086, scope dan pemisahan actor di BE; admin exception hanya bila canonical eksplisit dan audit. |
| ONB-G05 | REPAIR | FE `caseStatus !== 1` menyembunyikan seluruh aksi item pasca-ready | Dukung optional pending pada Completed sesuai DEC-088; jangan buka required, Cancelled, atau aksi sistem secara bebas. |
| ONB-G06 | REPAIR | Pada source `CompleteItemAsync`, file diproses bila ada; kebutuhan evidence wajib harus dipastikan server-side | Uji request tanpa lampiran dan salah jenis; tegakkan validasi pada service. |
| ONB-G07 | PARTIAL | `CreateFromHandoffAsync` memanggil draft contract dan account; handoff dapat tersimpan sebelum pembentukan case sukses | Tangani hasil dependency dan recovery idempotent, pastikan UI tidak sukses palsu. |
| ONB-G08 | NOT VERIFIED | Link profil, kontrak, penempatan, dan preceptor serta izin field berbeda domain belum diaudit end-to-end | Verifikasi source pemilik, gunakan ID benar dan permission; jika tidak tersedia tampilkan kondisi nyata. |
| ONB-G09 | NOT VERIFIED | Aktivasi ESS dan akses kerja, transaksi lintas service serta eligibility scheduling belum diuji runtime | Uji efek dan keamanan sesuai izin, jangan mengklaim berhasil dari response status saja. |

Prioritas: keamanan dan siap kerja (P0/P1) sebelum kosmetik UI. Bukti source bukan bukti live UAT. Jangan merombak domain lain untuk mengejar status PASS.

### 3.3 Kontrak API existing dan rancangan agregasi

Prefix yang terbukti: `/api/v1/corporate/human-resource`. Pada frontend ikuti instance Axios dan prefix existing (hindari double `/api`). Nama action/permission case dari controller, bukan asumsi role UI.

| Method dan path relatif | Permission existing | Fungsi |
|---|---|---|
| GET `/onboarding-cases`, `/onboarding-cases/summary`, `/onboarding-cases/{id}` | `WorkforceOnboardingCase:Read` | List, summary, detail |
| POST `/onboarding-cases/{id}/first-day-attendance` | `WorkforceOnboardingCase:CompleteItem` | Catat hadir |
| DELETE `/onboarding-cases/{id}/first-day-attendance` | `WorkforceOnboardingCase:CompleteItem` | Pembatalan hadir sesuai prasyarat |
| POST `/onboarding-cases/{id}/items/{itemId}/complete` | `WorkforceOnboardingCase:CompleteItem` | Complete dengan DTO/multipart existing |
| POST `/onboarding-cases/{id}/items/{itemId}/uncomplete` | `WorkforceOnboardingCase:CompleteItem` | Koreksi sesuai status dan role |
| POST `/onboarding-cases/{id}/check-automatic-items` | `WorkforceOnboardingCase:CompleteItem` | Periksa butir otomatis |
| PUT `/onboarding-cases/{id}/coordinator` | `WorkforceOnboardingCase:Reassign` | Ganti koordinator |
| POST `/onboarding-cases/{id}/declare-ready` | `WorkforceOnboardingCase:DeclareReady` | Pernyataan siap kerja |
| POST `/onboarding-cases/{id}/cancel-join` | `WorkforceOnboardingCase:CancelJoin` | Batal bergabung |
| GET `/onboarding-cases/{id}/items/{itemId}/evidence` | `WorkforceOnboardingCase:Read` + cek scope | Bukti item |
| GET `/employment-contracts/{id}`, `/print-data`, `/download-scan` (suffix pada ID) | `WorkforceEmploymentContract:Read` + masking/izin isi | Baca, cetak, scan kontrak |
| GET `/employment-contracts/{id}/salary-detail` | `WorkforceEmploymentContract:ViewAmount` | Nominal berwenang |
| POST `/employment-contracts/{id}/record-signed` | `WorkforceEmploymentContract:MarkSigned` | Catat signed dengan bukti |
| GET `/clinical-preceptorships`, `/clinical-preceptorships/{id}` | `WorkforcePreceptorship:Read` | Pendampingan |
| PUT `/clinical-preceptorships/{id}/preceptor`; POST `/clinical-preceptorships/{id}/declare-independent` | `WorkforcePreceptorship:Manage` | Aksi owner, bukan HRD override |
| GET `/onboarding-access-requests`, `/onboarding-access-requests/{id}` | `WorkforceOnboardingAccessRequest:Read` | Status akses |
| POST `/onboarding-access-requests/{id}/complete` | `WorkforceOnboardingAccessRequest:Complete` | Selesaikan oleh pemilik |
| POST `/onboarding-access-requests/{id}/retry` | `WorkforceOnboardingAccessRequest:Retry` | Retry yang diizinkan |

Kontrak agregasi **usulan implementasi**: prioritaskan memperluas `OnboardingCaseDetailResponse` secara backward-compatible dengan ringkasan wizard. Jika terlalu berat atau kontrak lama tidak sesuai, pilih satu endpoint GET read-only `/onboarding-cases/{id}/journey` pada controller existing; jangan membangun keduanya tanpa kebutuhan. Catat keputusan teknis pada report dan sinkronisasi kontrak. Tidak membuat endpoint `save-step`, `complete-all`, atau `force-ready` yang mem-bypass domain. GET tidak boleh melakukan state mutation, mengaktifkan akun, memperbarui checklist atau menjalankan recovery.

Contoh rancangan shape (nama field ini usulan, bukan klaim DTO saat ini; cocokan standar serialization/wrapper existing):

```json
{
  "caseId": "<existing-case-id>",
  "workforceProfileId": "<existing-profile-id>",
  "hiringHandoffId": "<existing-handoff-id>",
  "caseStatus": "InProgress",
  "rowVersion": "<existing-row-version>",
  "progress": {"requiredTotal": 5, "requiredCompleted": 3, "optionalTotal": 2, "optionalCompleted": 0},
  "stages": [
    {"key": "acceptance", "status": "Completed", "allowedActions": [], "references": []},
    {"key": "employeeData", "status": "InProgress", "allowedActions": [], "references": []},
    {"key": "contract", "status": "Blocked", "allowedActions": [], "references": []},
    {"key": "placement", "status": "InProgress", "allowedActions": [], "references": []},
    {"key": "orientation", "status": "InProgress", "allowedActions": [], "references": []},
    {"key": "access", "status": "InProgress", "allowedActions": [], "references": []},
    {"key": "readiness", "status": "Blocked", "allowedActions": [], "references": []}
  ],
  "readiness": {"eligible": false, "canDeclare": false, "blockers": [{"code": "CONTRACT_NOT_SIGNED", "stageKey": "contract", "message": "Kontrak belum ditandatangani."}]}
}
```

- Stage key stabil; status visual `NotStarted`, `InProgress`, `Completed`, `Blocked`, `NotApplicable`, `Unknown`, `AccessRestricted` sesuai kebutuhan. Tidak menambah enum database baru; turunkan dari fakta case dan kontrak. `Cancelled` sebagai status kasus terminal, tampilkan stage historis tanpa aksi mutasi.
- `eligible` kesiapan bisnis berbeda dari `canDeclare` actor saat ini; jangan menyembunyikan blocker tanpa penjelasan. Response action dan link berasal dari permission dan kondisi server; validasi lagi saat mutation. Tidak memberikan detail atau resource ID sensitif kepada actor yang tidak boleh membaca. Return `403/404/409/422` mengikuti konvensi controller, bukan respons 200 palsu.
- Agregasi berdasarkan exact case ID dan relasi handoff/profil/kontrak; bukan nama, NIK atau pencarian halaman pertama. Query scoped, projection `AsNoTracking` untuk baca, hindari N+1, unbounded list dan leak lintas unit. Pertahankan response lama dan list existing.
- Progres required = jumlah butir wajib selesai / jumlah butir wajib aktual; jika belum tersedia/invalid, tampilkan belum terverifikasi dan blocker, bukan 100%. Optional dihitung terpisah. Stage completion tidak menggantikan gate siap kerja.

### 3.4 Skema tampilan yang wajib diwujudkan

Gunakan layout/detail pattern existing, warna/token Quilvian yang sama, tanpa perubahan global. Header/Hero hanya judul, breadcrumb, deskripsi dan Kembali sesuai pattern; tombol mutasi pindah ke side section. Fokus pada satu konten tahap, bukan tujuh card menjulur ke bawah. Desktop dua kolom; pada layar sempit panel aksi turun secara natural tanpa horizontal overflow.

```text
+------------------------------------------------------------------------+
| SDM / Penerimaan & Onboarding / Kasus Onboarding / Detail     [Kembali] |
| Onboarding Pegawai Baru - Nama | Nomor kasus | Unit | Jabatan | Status |
+------------------------------------------------------------------------+
| Wajib selesai 3/5 | Opsional 0/2 | Koordinator | Rencana mulai | status |
| (1) Penerimaan (2) Data (3) Kontrak (4) Penempatan (5) Orientasi        |
| (6) Akses IT (7) Siap Kerja                                            |
+----------------------------------------------+-------------------------+
| KONTEN TAHAP DIPILIH (~ 2/3)                 | SIDE SECTION (~ 1/3)    |
| Judul + status + ringkasan / tabel / field    | Tindakan berwenang      |
| Hanya konten tahap yang relevan              | Penanggung jawab        |
| Link ke sumber yang relevan                 | Alasan hambatan         |
| Empty/loading/error sesuai data              | Langkah berikutnya      |
+----------------------------------------------+-------------------------+
| [Tahap Sebelumnya]                      [Lihat Tahap Berikutnya]       |
+------------------------------------------------------------------------+
```

| Tahap | Konten tengah | Side section | Kondisi khusus |
|---|---|---|---|
| 1 Penerimaan | Identitas read-only, sumber handoff, rencana vs aktual hadir, koordinator, hasil clearance ringkas | Catat Hadir; Batal Bergabung; koreksi hadir/ganti koordinator bila sah | Kasus gagal/hilang tidak membuat profil baru; cancelled read-only |
| 2 Data Pegawai | Daftar 9 kelompok kelengkapan; status dan link edit pemilik | Lengkapi Data Pegawai; arahan field yang belum lengkap | Data sensitif masked; unknown bukan 100% |
| 3 Kontrak | Nomor, jenis, periode, status, dokumen signed | Buka/Cetak/Record Signed sesuai izin | Draft belum signed; rehire tidak mengambil kontrak historis |
| 4 Penempatan | Unit, site, posisi, uraian tugas, atasan, status relasi | Pilih Atasan oleh Kepala Unit; lihat referensi | Tidak ada transaksi mutasi/gaji baru |
| 5 Orientasi | Checklist per layer, wajib/opsional, bukti, penanggung jawab, status preceptor | Complete/unggah/pilih orang sesuai item; Periksa Otomatis | Clinical only; opsional follow-up setelah Completed |
| 6 Akses IT | AccountCreation/ESS/WorkAccess, status waktu, failure aman | Lihat Permintaan Akses; Retry/Complete hanya pemegang izin | WorkAccess belum diminta sebelum ready bukan blocker siap kerja |
| 7 Siap Kerja | Daftar gate dari backend; required dan optional; audit hasil | Nyatakan Siap Kerja + konfirmasi; link perbaikan ke tahap | Tidak otomatis; pemisahan aktor; setelah selesai akses/preceptor follow-up |

Stepper memuat 7 label lengkap dan ringkas sesuai breakpoint. Tahap tidak berlaku terlihat jelas. Pilihan stage dapat dipertahankan pada query parameter non-sensitif yang divalidasi (mis. `?stage=contract`) jika sesuai pattern existing; gunakan `caseId` route yang ada. Default masuk ke pekerjaan relevan berdasarkan data dan izin, bukan indeks local yang salah. Navigasi kembali/refresh mengambil sumber terbaru. Fokus keyboard, aria-current pada step aktif, label aksesibel, status tidak hanya mengandalkan warna, error dekat field, loading disable tombol aksi dan konfirmasi. Tidak ada progress bar hardcode atau success dummy. Pada small screen gunakan stepper scroll internal atau pola responsive canonical dengan semua tahap dapat diakses.

**Base component gate:** inventaris elemen dan baca props nyata di `src/components/features/base-features/`; reuse Hero, BaseButton, DataTable, StatusBadge/HrStatusBadge, InformationAlert, ConfirmModal, BaseFormControl fields, resource picker, attachment/file field dan ToastStack existing. Reuse HrPanel/HrInfoGrid dan hooks lifecycle existing jika compliant. `WorkflowStepperCard` untuk approval bukan otomatis cocok untuk wizard; evaluasi wizard payroll/registration existing sebagai referensi komposisi, bukan copy business logic. Jangan membuat base component global baru tanpa decision gate dan otorisasi; komposisi feature-specific dari base components cukup bila tidak ada generic stepper. Catat tabel elemen/reuse/alasan, design token dan CSS Module; tidak hardcode warna prototype. Jangan gunakan Tailwind bila tidak tersedia, custom dialog/field/table yang menggantikan base component atau refactor massal.

## 4. Reasoning / Execution Workflow

### 4.1 Preflight dan fase sinkronisasi kontrak

1. Verifikasi `git status`, branch, HEAD/upstream read-only. Bandingkan dengan tiga SHA di dokumen. Jika berubah, baca diff/file terbaru di scope, pertahankan pekerjaan existing; jangan pull atau checkout otomatis.
2. Baca governance canonical dan rule relevan; cek report dan roadmap/traceability S-C4-ONB serta dependency recruitment. `00-interview-decisions.md` merupakan sumber keputusan, `contracts/`, `data/`, `flowcharts/`, `roadmap/` untuk kontrak dan task resmi. Nomor FIN di bawah adalah label eksekusi usulan yang sudah dipakai dalam planning; rekonsiliasi ke task ID resmi yang tersedia, jangan membuat status roadmap fiktif.
3. Bila kontrak wizard/guard baru belum canonical, `MODULE BLUEPRINT MODE` hanya pada `docs/module-blueprints/human-resource/` jika mode dan target tulis diizinkan. Dokumentasikan perubahan response read-only, gate, stage, scope, acceptance dan traceability. Jangan memperluas kebijakan yang belum disetujui. Jika akses dokumen belum ada, laporkan `NEEDS_SYNC` dan lanjutkan perubahan independen yang kontraknya sudah sah; tahan bagian yang mensyaratkan approval kontrak. Jangan mengulang interview atas tujuh tahap yang telah disetujui.
4. `BACKEND MODE` target tulis terbatas pada domain LifecycleManagement dan integrasi handoff yang dibutuhkan serta tes terkait. `FRONTEND MODE` hanya pada route/view/hook/service/constants/style HR lifecycle dan tautan yang terkait. Repository lain read-only per fase. Validasi lintas repo dalam izin yang sah; tidak melakukan mutasi silang tanpa mode/izin yang sesuai. Laporan hanya di lokasi governance yang diotorisasi.

### 4.2 Task finishing (kerjakan sesuai dependensi)

| FIN ID | Mode | Implementasi yang diminta | Gate selesai |
|---|---|---|---|
| `FIN-HRD-ONB-WIZ-01` | BACKEND | Implementasikan agregasi tujuh stage, link context, progress, actor-specific action, blocker; query scoped dan field masking. Reuse existing detail atau satu GET baru. | Contract dan authorization tests, no write saat GET. |
| `FIN-HRD-ONB-WIZ-02` | BACKEND | Shared ready evaluator; signed/fitness/system checklist; role/unit/four-eyes; evidence; optional after ready; cancelled/required guard; transaction/idempotency; perbaiki handoff/draft/access recovery yang sah. | P0/P1 negative tests, tidak ada bypass lewat direct API, state konsisten. |
| `FIN-HRD-ONB-WIZ-03` | FRONTEND | Detail kasus menjadi seven-step workflow, layout tengah + side section, navigasi status, progress, loading/error, responsif dan aksesibilitas, base component gate. | Route existing dan sidebar utuh; status real; tidak ada aksi header. |
| `FIN-HRD-ONB-WIZ-04` | FRONTEND | Integrasi penerimaan, data pegawai, kontrak, penempatan dengan ID benar, return path dan izin. | Step 1-4 tersambung real, persist/reload, tidak duplicate profile/contract. |
| `FIN-HRD-ONB-WIZ-05` | FRONTEND | Integrasi orientasi, evidence/person/automatic items, optional follow-up, akses IT dan final readiness, blocker/action sesuai actor. | Step 5-7 terhubung; klinis/nonklinis berbeda; akses bertahap. |
| `FIN-HRD-ONB-WIZ-06` | QA/integrasi | Live browser role-based, regresi semua tujuh submenu dan upstream recruitment, perbaiki bug dalam scope dan retest. | Bukti aktual untuk semua kriteria; blocker dilabel tepat. |

Jalankan runtime skill backend untuk task 01-02, frontend untuk 03-05 sesuai governance dan wewenang. Baca `BACKEND_ENGINEERING_CONTRACT.md`, registry ownership prefix, aturan QBE/DB/API, test policy; FE cek base component decision gate, design tokens, architecture, test policy dan UI checker. Gunakan struktur dan dependency injection existing; jangan menambah generic workflow engine, tabel draft, enum DB, migrasi atau permission wildcard. Gunakan test backend dan FE yang relevan; build/lint/test hanya sesuai aturan repo dan izin pengguna. Jangan membuat migration sekadar untuk mengejar test. Jika constraint schema perlu perubahan, status `BLOCKED_SCHEMA` dengan bukti dan kebutuhan keputusan; lanjutkan scope independen.

### 4.3 Matriks UAT live browser wajib (40 kasus)

Persiapan: lingkungan dev/test yang diizinkan; akun HRD (minimal dua actor berbeda bila tersedia), Kepala Unit tujuan, Kepala Unit lain, IT/Administrator sesuai hak, read-only/tanpa hak. Gunakan data sintetis: satu kandidat klinis (mis. perawat), satu nonklinis, satu rehire/kontrak historis; satu kasus no-show, kasus missing template atau fault injection test fixture terisolasi. Jangan membuat data pasien/pegawai asli atau mengubah konfigurasi produksi. Jika tidak ada fixture/akses, tandai NOT RUN/BLOCKED untuk skenario itu; jangan memalsukan data atau mengubah DB langsung. Uji P0/P1 terlebih dahulu. Setiap baris wajib memiliki hasil aktual, bukti, dan retest. API negative test melengkapi browser; tandai mana yang hanya bisa diuji API/automated test secara jujur.

| ID | Prioritas | Langkah pengujian | Expected result |
|---|---|---|---|
| UAT-ONB-01 | P1 | Dari handoff sukses klik tautan onboarding | Case, profil, unit, jabatan dan offer sesuai; tidak ada registrasi ulang. |
| UAT-ONB-02 | P0 | Simulasikan case belum terbentuk setelah handoff dalam fixture sah; buka tautan/retry | Tidak ada sukses palsu; blocker nyata; recovery tidak membuat profil/case ganda. |
| UAT-ONB-03 | P1 | HRD catat hadir pertama pada tanggal sah, reload | Tanggal dan audit persisten, available actions diperbarui. |
| UAT-ONB-04 | P1 | Catat hadir sebelum planned start (UI dan direct API) | Ditolak server; tidak ada efek akun/status. |
| UAT-ONB-05 | P1 | Koreksi hadir sebelum signed, lalu coba pada kontrak sah | Jalur awal sesuai kontrak; koreksi sesudah signed ditolak. |
| UAT-ONB-06 | P1 | Buka kelompok data pegawai dan edit satu field pada pemilik existing | Perubahan muncul pada profil yang sama setelah kembali/reload. |
| UAT-ONB-07 | P0 | Login role tanpa akses profil/nominal, inspeksi response dan link | Field/identifier sensitif tidak bocor; akses ditolak sesuai kontrak. |
| UAT-ONB-08 | P2 | Data keluarga/pendidikan belum lengkap atau API gagal | Status perlu dilengkapi/unknown; tidak false Complete dan tidak otomatis hard-block jika bukan wajib. |
| UAT-ONB-09 | P1 | Buka kontrak draft hasil offer | ID/jenis/periode benar, hanya satu draft relevan, nominal mengikuti izin. |
| UAT-ONB-10 | P0 | Coba record signed tanpa scan, file salah atau bukan kontrak kasus | Ditolak, status tidak berubah. |
| UAT-ONB-11 | P1 | Record signed dengan scan sah, refresh wizard | Status nyata; butir otomatis terverifikasi; tidak approval kedua. |
| UAT-ONB-12 | P0 | Rehire punya kontrak historis signed tetapi kontrak baru Draft | Gate memakai kontrak baru; tidak ready palsu. |
| UAT-ONB-13 | P1 | Kepala Unit tujuan menetapkan atasan yang eligible | Item dan ID tersimpan, penempatan tidak diduplikasi. |
| UAT-ONB-14 | P0 | Kepala Unit lain/role tidak sah mencoba item unit | UI action tidak tersedia dan API menolak. |
| UAT-ONB-15 | P1 | Pilih atasan tidak valid/inactive/self atau relasi tidak sah | Ditolak sesuai kebijakan; audit dan data tidak berubah. |
| UAT-ONB-16 | P0 | Actor yang menetapkan atasan mencoba declare-ready | Server menolak four-eyes; actor HRD berbeda tetap dapat bertindak bila semua gate terpenuhi. |
| UAT-ONB-17 | P1 | Selesaikan manual item oleh pemilik, coba uncomplete sah sebelum ready | State konsisten, role benar, reload persist. |
| UAT-ONB-18 | P0 | Complete item evidence tanpa file melalui API; unggah file sah dan akses unduhan lintas unit | Tanpa file ditolak; file sah tersimpan; akses ilegal ditolak. |
| UAT-ONB-19 | P0 | Coba complete/uncomplete manual item otomatis; fixture general template hilang atau system item invalid | Butir otomatis tidak bisa dipalsukan; readiness menolak snapshot tidak sah, bukan auto-ready. |
| UAT-ONB-20 | P1 | Periksa kasus lama ketika template berubah pada fixture sah | Snapshot lama tidak ditimpa; missing template diberi solusi/recovery yang sah. |
| UAT-ONB-21 | P1 | Klinis pilih preceptor valid melalui Kepala Unit | Link preceptor/selected person benar; actor lain ditolak. |
| UAT-ONB-22 | P1 | Kasus nonklinis membuka orientasi | Tidak ada kewajiban preceptor/pendampingan yang tidak berlaku. |
| UAT-ONB-23 | P1 | HRD batal bergabung sebelum hadir dengan alasan | Case/profil/kontrak draft/kursi mengikuti efek canonical; state persisten. |
| UAT-ONB-24 | P0 | Coba batal bergabung setelah hadir | Ditolak; diarahkan ke proses pemisahan resmi, tidak ada perubahan state. |
| UAT-ONB-25 | P0 | Direct API complete/catat hadir/ready pada kasus Cancelled | Ditolak; kasus tidak hidup kembali. |
| UAT-ONB-26 | P0 | Panggil declare-ready sebelum hadir/signed atau dengan clearance tidak layak, termasuk checklist stale | Ditolak dengan blocker; profil tetap tidak aktif. |
| UAT-ONB-27 | P0 | Semua UI tampak selesai tetapi salah satu required item belum selesai atau atasan hilang | Readiness dan mutation server tetap menolak. |
| UAT-ONB-28 | P0 | Klinis semua wajib terpenuhi kecuali preceptor; kemudian tetapkan dan siap kerja | Awalnya tertahan, kemudian status Didampingi; bukan Siap Mandiri. |
| UAT-ONB-29 | P1 | Setelah Completed, selesaikan optional pending; coba uncomplete required; Kepala Unit lakukan Siap Mandiri melalui proses terpisah jika memenuhi syarat | Optional dapat selesai; required tidak dibatalkan bebas; pendampingan terpisah dan otorisasi sah. |
| UAT-ONB-30 | P0 | Manipulasi caseId/profileId/itemId pada URL/API dengan pengguna beda unit | Tidak boleh baca/mutasi case atau item pihak lain. |
| UAT-ONB-31 | P1 | Case baru, lihat permintaan account creation | Satu request/akun existing; status real, tanpa credential. |
| UAT-ONB-32 | P0 | Sebelum hadir dan setelah hadir sebelum ready, uji ESS serta akses kerja/klinis | ESS mengikuti hari pertama; akses kerja tidak diberikan prematur. |
| UAT-ONB-33 | P1 | IT role sah retry request gagal; HR tanpa izin mencoba sama | Retry aman/idempotent; unauthorized ditolak; pesan failure aman. |
| UAT-ONB-34 | P0 | Declare-ready berhasil saat WorkAccess belum dibuat; cek request pasca-ready | Ready tidak terhalang circular gate; WorkAccess dibuat satu kali dan status bukan completed palsu. |
| UAT-ONB-35 | P2 | Buka Perlu Tindakan, klik case/pekerjaan dan kembali | Navigasi benar, worklist mengikuti data terbaru; tidak kehilangan optional follow-up. |
| UAT-ONB-36 | P1 | Dari Recruitment Journey existing klik Onboarding; kembali ke daftar/handoff | Case tepat, tidak menjalankan ulang handoff, registrasi dan rekrutmen tidak regress. |
| UAT-ONB-37 | P2 | Buka Template/Settings dan seluruh 7 submenu sebagai actor berizin | Route dan tampilan existing terjaga; wizard tidak mengubah konfigurasi. |
| UAT-ONB-38 | P1 | Dua HRD/tab mencoba declare-ready bersamaan, ulang setelah response timeout | Satu efek sukses konsisten; tidak ada duplicate manager/access/profile; error/refresh jelas. |
| UAT-ONB-39 | P2 | Navigasi 7 stage, back/refresh, API timeout/403, layar desktop/mobile, keyboard | Tahap benar; no stale overwrite, loading/error, tidak overflow; aksi jelas dan aksesibel. |
| UAT-ONB-40 | P0 | Happy path nonklinis/klinis sesuai syarat; cek profil, manager, WorkAccess, scheduling eligibility, tanggal awal kontrak untuk payroll | Completed dan aktif tepat satu; klinis Didampingi, restriction aman, status downstream sesuai kontrak; data payroll tidak dihitung ulang dari ready date. |

Untuk UAT-ONB-26/27/28/38/40 sertakan bukti sebelum dan sesudah dari API serta refresh browser, bukan screenshot badge saja. Bila downstream scheduling/payroll/IT tidak dapat diuji karena environment/izin, laporkan subkriteria NOT RUN dan jangan menyimpulkan siap operasional. Untuk 403/404 masking, simpan bukti yang tidak mengandung data pribadi, token, cookie atau file asli. Gunakan screenshot headed/browser nyata dan network status/payload tersanitasi. Rekam expected vs actual, tanggal/jam WIB, actor role, case ID sintetis, endpoint, hasil, bug/retest. Jangan menulis log diagnosis atau data rekening.

### 4.4 Definition of Done

- Tujuh tahap terjangkau pada detail kasus existing; seluruh menu tetap dan semua pekerjaan masuk kategori tanpa hilang/duplikat.
- Source BE real; gate siap kerja menggunakan kondisi aktual dan actor, operasi terotorisasi, perubahan persisten; API/DTO/enum tidak mismatch; tidak ada mock, fallback data contoh, false success atau local-only mutation.
- Signed contract, clearance, template dan system items, atasan, preceptor jika klinis, required/optional, pembatasan medis, pembagian actor, account/ESS/work access diuji; siap kerja tidak sama dengan siap mandiri.
- Base components dan desain existing terbukti lewat import/path, decision gate dan pemeriksaan UI. Aksi mutasi pada side section. Responsif dan error recovery nyata.
- Tidak ada regresi handoff, registration wizard, recruitment journey, daftar kasus dan tujuh submenu terkait. Tidak mengubah Master Data, Payroll, Administrator di luar kontrak integrasi yang sah.
- Build/lint/unit/integration/QBE/FE checker/UAT sesuai izin dan governance; P0/P1 FAIL diperbaiki dan retest. Bukti DB hanya dari response/persistence sah atau query yang diizinkan; tidak mengklaim database telah diuji bila tidak dilakukan.
- Laporan resmi, status task, traceability dan checkpoint hanya sesuai mode/izin. Jangan beri status VERIFIED WORKING/100% jika acceptance atau live test belum terbukti.

## 5. Stop conditions

Jangan berhenti hanya karena audit selesai, route ada, compile pass, atau menemukan bug yang bisa diperbaiki dalam scope. Kerjakan source yang sudah sah. Hentikan hanya area terdampak jika:

- branch/target tulis salah atau ada konflik perubahan pengguna; canonical governance tidak tersedia; runtime skill yang diminta tidak tersedia; izin kerja belum sah;
- keputusan canonical dan approval chat berbeda secara material (mis. aturan clinical clearance, role exception, recovery template historis, perubahan offer) sehingga tidak aman menebak; laporkan konflik dan lanjutkan pekerjaan independen;
- butuh tabel/kolom/index/migration/operasi DB yang belum berizin (`BLOCKED_SCHEMA`); jangan menyamarkan dengan data dummy;
- butuh akses Administrator/Payroll/clinical domain atau environment test yang tidak tersedia; jangan melanggar batas data, perizinan, atau mencoba produksi;
- build/test/browser membutuhkan izin atau tidak bisa dijalankan; tandai NOT RUN/BLOCKED; jangan klaim lulus.

Larangan tetap: tidak ada auto commit/push/pull/merge/rebase/deploy, migration, DB command, penghapusan data atau perubahan akses massal. Jangan membocorkan data medis/personal/payroll. Jangan membuat prosedur klinis/ketenagakerjaan baru.

## 6. Output

Agent wajib mengembalikan satu ringkasan implementasi yang dapat ditinjau dan report sesuai Engineering Skills:

1. Branch, HEAD awal/akhir, scope/mode, rule dan skill yang dipanggil, status working tree; file path dan symbol aktual yang diubah per FIN task. Jelaskan keputusan reuse vs add endpoint serta alasan.
2. Matriks 7 tahap dan 7 submenu integrasi: `VERIFIED WORKING`, `IMPLEMENTED_NOT_VERIFIED`, `PARTIAL`, `REPAIR`, `BLOCKED` atau `NOT VERIFIED`, dengan evidence dan sisa pekerjaan. Jangan gunakan persentase palsu.
3. Kontrak API aktual, data source per stage, permission/action dan blocker, mapping readiness, efek mutasi dan persistence; bukti guard/negative test, no duplicate, scope privacy.
4. Bukti skema UI dan base component gate, screenshot live sebelum/sesudah yang disanitasi, route dan regresi; daftar masalah dan retest.
5. Hasil build/lint/test/UAT 40 kasus: jumlah PASS/FAIL/NOT RUN/BLOCKED, numerator/denominator jelas; command/URL lingkungan aman, timestamp, link bukti, actual, bukan simulasi. Satu laporan UAT grup sesuai aturan resmi.
6. Laporan task tracked pada `docs/module-blueprints/human-resource/task/report/backend/` dan `task/report/frontend/` dengan ID resmi yang direkonsiliasi, hanya jika mode/izin memungkinkan; update roadmap/traceability canonical bila diizinkan. Jangan membuat bukti fiktif, jangan commit/push. Jika perubahan kontrak masih menunggu, catat `NEEDS_SYNC`.
7. Checkpoint memory: `docs/module-blueprints/human-resource/memory.md` saat dibaca pada snapshot BE mengembalikan 404 (`MEMORY_NOT_INITIALIZED`). Simpan checkpoint hanya setelah izin mode yang sah; jika belum, sertakan ringkasan di report (`MEMORY_PENDING_SYNC`). Catat persetujuan 7 tahap, FIN-01..06, snapshot, sumber keputusan, hasil aktual, blocker dan next action. Jangan membuat dokumen instruksi kedua. Snapshot saat penyusunan prompt: source existing belum diuji live; wizard baru masih direncanakan.

**PERINTAH AKHIR:** Requirement tujuh tahap sudah disetujui. Setelah preflight dan gate governance yang sah, langsung implementasikan backend, frontend, integrasi dan pengujian sesuai instruksi ini. Jangan berhenti pada audit atau planning. Laporkan blocker secara spesifik dan selesaikan bagian independen. Pertahankan sidebar, Master Data, Administrator, Payroll serta fitur rekrutmen existing di luar scope perubahan yang memang diperlukan.
