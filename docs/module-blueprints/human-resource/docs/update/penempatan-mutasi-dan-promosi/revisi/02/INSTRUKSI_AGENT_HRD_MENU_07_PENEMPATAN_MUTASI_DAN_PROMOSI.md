# MASTER INSTRUKSI AGENT — HRD MENU 07: PENEMPATAN, MUTASI & PROMOSI

**Revisi:** 20 November 2026 (WIB) — penanganan temuan UAT Batch Penempatan Awal dan fitur **Smart Validation & Guided Data Correction**  
**Jenis:** Instruksi eksekusi source code nyata untuk Agent Quilvian V2, **BUKAN audit-only** dan **BUKAN laporan implementasi sukses**.  
**Modul/Menu:** Kepegawaian → Menu 07 — **Penempatan, Mutasi & Promosi**.  
**Lima Submenu resmi:** (1) Penempatan Awal, (2) Perpindahan Unit, (3) Rotasi Promosi, (4) Uraian Tugas, (5) Atasan Langsung.  
**Prioritas pekerjaan baru:** Submenu 1 / Penempatan Awal, melibatkan dependensi Master Organisasi/Jabatan, Data Induk Pegawai, penempatan organisasi/jabatan, dan workflow SK. Empat Submenu lain wajib dijaga kompatibilitas dan diregresi.  
**Urutan implementasi wajib:** preflight governance → penyesuaian keputusan/kontrak canonical **hanya jika berizin** → **BACKEND terlebih dahulu** → integrasi kontrak → **FRONTEND** → tes integrasi/UAT → laporan berbukti.  
**Dokumen canonical Menu 07 yang harus DIPERBARUI, bukan dibuat duplikat:** `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/INSTRUKSI_AGENT_HRD_MENU_07_PENEMPATAN_MUTASI_DAN_PROMOSI.md` di BE branch `MHamzah`. **Merge instruksi baru ini ke dokumen existing; pertahankan seluruh persyaratan 18 temuan audit, 16 tugas FIN, keputusan HRD dan test lama yang tidak bertentangan. JANGAN mengganti isi master lama secara destruktif.**  
**Penomoran FIN yang wajib dipertahankan:** `FIN-HRD-07-BE-001..009` dan `FIN-HRD-07-FE-001..007`. **Jangan buat seri FIN baru.** Temuan baru menggunakan ID UAT (bukan ID task baru) dan dipetakan ke task FIN existing.  
**Status saat penyusunan:** Temuan UI direproduksi oleh tester melalui screenshot; akar penyebab sebagian terkonfirmasi di source; solusi **belum diimplementasikan**, UAT pascaperbaikan **belum dilakukan**.

---

## 1. Role

Anda adalah **Agent Fullstack Senior Quilvian V2** dengan kemampuan ASP.NET Core/EF Core, Next.js App Router, arsitektur HR rumah sakit, kontrol akses, desain UI/UX, dan QA. Mandat Anda adalah **LANGSUNG MENGIMPLEMENTASIKAN** perbaikan yang telah disetujui untuk Menu 07 pada workspace dan mode tulis yang sah; bukan membuat proposal lagi, bukan mengulang wawancara yang telah dijawab, dan bukan menulis UI dummy.

### 1.1 Target repository, branch, dan mode resmi

| Fase | Mode | Target tulis terbatas | Referensi read-only |
|---|---|---|---|
| Sinkronisasi keputusan bila diperlukan dan diizinkan | `MODULE BLUEPRINT MODE` | BE: `docs/module-blueprints/human-resource/` terkait Menu 07 saja | Source FE/BE |
| Implementasi prioritas pertama | `BACKEND MODE` | `DevBenari/NewQuilvianSystemBackend` branch `MHamzah`, khusus service/controller/DTO/model/workflow HRD terkait dan test scoped | FE `HamzahV2` |
| Implementasi setelah kontrak BE stabil | `FRONTEND MODE` | `DevBenari/QuilvianSystemFrontendDev` branch `HamzahV2`, khusus route/view/service/hook/utility/style/test scoped HRD | BE `MHamzah` |
| Hanya jika izin lintas repo eksplisit ada | `CROSS-REPO MODE` | Kedua repo, **hanya** path yang ditetapkan dalam dua fase tadi | Governance canonical |
| QA dan report | Mode yang diizinkan sesuai jenis file | Test/laporan scoped; tidak memberikan izin deployment atau mutasi DB | — |

**Wajib preflight:** periksa `git status`, branch, upstream, HEAD, ownership, area tulis dan source terbaru; jangan mengubah branch untuk 'menyesuaikan' sendiri. Nama branch/repo di atas adalah target pengguna; izin menulis tetap harus selaras dengan governance workspace dan owner.

**Registered runtime Skills wajib, bukan hanya dibaca:**
- Implementasi BE: invoke `quilvian-engineering-skills:build-module-backend`.
- Implementasi FE: invoke `quilvian-engineering-skills:build-module-frontend`.
- Baca `AGENTS.md` dan `CLAUDE.md` kedua repo; Engineering Skills `main` → `agents/rules/GLOBAL_RULES.md`, `agents/rules/README.md`, aturan `backend/`, `frontend/`, `testing/`, `rule-output/` relevan; serta BE `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
- Jika runtime skill yang relevan gagal dipanggil: **`BLOCKED — Quilvian runtime skill unavailable`**; jangan bypass dengan implementasi manual.
- **Tidak ada izin otomatis** `git commit/push/pull/merge/rebase/checkout`, deploy, membuat/menjalankan migration, maupun mengubah database staging/production. Operasi tersebut membutuhkan otorisasi terpisah sesuai governance.

---

## 2. Task

### 2.1 Temuan UAT yang harus diperbaiki

**Sumber runtime:** tester pada website UAT `https://dev.quilvian-mmchospital.com/` tanggal 20 November 2026, pada Menu 07 → Penempatan Awal → tombol `+ Buat Batch Baru`.

| ID temuan | Tingkat risiko | Bukti/actual | Expected setelah perbaikan |
|---|---|---|---|
| `UAT-HRD07-INITIAL-001` | P1 — alur kerja utama terhambat | Template berisi **41 kandidat**; setelah `Validasi Ulang Integritas`, **37 valid dan 4 tidak valid**. Tombol `Simpan & Buat Batch (37 Pegawai)` tidak bisa ditekan | HRD bisa memilih sekumpulan kandidat **yang memenuhi syarat** untuk disimpan; empat kandidat bermasalah tidak boleh otomatis tersimpan; batch terpilih harus valid 100% |
| `UAT-HRD07-INITIAL-002` | P1 — pencampuran kandidat | Sekurangnya **dua** dari empat baris invalid memiliki pesan `Sudah memiliki jabatan aktif. Tidak dapat diproses melalui batch inisialisasi awal.` | Pegawai dengan penempatan resmi yang benar masuk kategori `Dilewati — Sudah Ditempatkan`; jangan memaksa hapus jabatan supaya lolos |
| `UAT-HRD07-INITIAL-003` | P1 — diagnosis tidak terbimbing | Pesan validasi hanya ditampilkan pada tabel; **belum ada action** untuk pergi ke data sumber yang dapat diperbaiki dan kembali ke batch | Ada `Perbaiki Data` / `Tinjau Penempatan` yang menuju tujuan aman dan tepat, mempertahankan konteks kerja, kembali, lalu validasi ulang dari DB |
| `UAT-HRD07-INITIAL-004` | P2 — validasi tidak konsisten/kurang jelas | Jumlah awal sempat **38 valid / 3 invalid**, setelah validasi ulang menjadi **37 valid / 4 invalid** | Validasi initial preview menggunakan aturan server yang selaras dengan validasi ulang; bila data berubah, beri pesan perubahan data/version, bukan tampak inkonsisten tanpa alasan |

**Batas bukti:** screenshot hanya memperlihatkan alasan invalid untuk dua pegawai. Jangan menyimpulkan bahwa **keempatnya** pasti memiliki alasan sama. Jangan menyalin identitas pegawai ke fixture/report/prompt; gunakan ID anonim pada test.

**Penyebab teknis yang TERKONFIRMASI dari source:**
1. BE `HrdInitialPlacementBatchService.GetTemplateRowsAsync` mengambil seluruh penugasan organisasi primer aktif, termasuk yang sudah punya `WfpPositionAssignment` primer aktif; mereka tetap ditambahkan sebagai baris `IsValid=false`.
2. BE `ValidateRowsAsync` melakukan pemeriksaan yang lebih luas daripada penandaan sederhana di template awal (duplikat, profil, posisi, departemen, unit organisasi, atasan, tanggal, penempatan aktif). Initial preview dan hasil validasi ulang dapat berbeda.
3. FE `initial-placement-batch-view.jsx` menetapkan `disabled` pada tombol Simpan apabila `templateInvalidCount > 0`; proses penyimpanan membentuk payload dari **seluruh** `templateRows`, tanpa pemilihan subset yang aman.
4. FE belum menyediakan workflow perbaikan terarah beserta pengembalian konteks/refresh setelah pengguna membenahi data pada halaman lain.

### 2.2 Requirement hasil akhir — Smart Validation & Guided Data Correction

**Kunci desain yang disetujui pada pembahasan UAT:**
1. Pisahkan baris menjadi **`SIAP_DIPROSES`**, **`DILEWATI_SUDAH_DITEMPATKAN`**, **`PERLU_PERBAIKAN`**, dan **`PERLU_REKONSILIASI`** bila sumber sejarah perlu keputusan HRD. Ini adalah kategori UI/diagnostik; jangan diam-diam mengganti `BatchStatus` canonical.
2. **Batch hanya berisi pegawai yang dipilih dan telah lolos validasi server**. Untuk contoh 41 kandidat dengan 37 valid, pengguna dapat membuat batch dari 37 kandidat jika semuanya tetap layak saat disimpan; 4 lainnya tidak ikut payload. **Tidak berarti eksekusi parsial**: sekali batch berisi N pegawai disetujui, eksekusi N pegawai tetap **atomik/all-or-nothing**.
3. Baris yang sudah ditempatkan dengan sah **dilewati**, bukan diubah dari Tidak Valid ke Valid. Jika data aktif ternyata salah atau merupakan legacy yang perlu dikoreksi berdasarkan SK, tampilkan **Perlu Rekonsiliasi** dan arahkan ke review/approval resmi; jangan otomatis menghapus, menonaktifkan, atau menimpa assignment.
4. Baris yang dapat diperbaiki mendapat penjelasan spesifik dan **CTA `Perbaiki Data` yang membuka halaman pemilik data** serta entitas yang tepat jika identitas/izin mendukung. Jika entitas spesifik belum bisa di-resolve dengan aman, buka halaman daftar dengan filter pencarian yang jelas dan tampilkan pemberitahuan, **bukan menebak ID**.
5. Setelah simpan koreksi, pengguna dapat **`Kembali ke Batch`**, daftar/template diambil ulang dari API, dan validasi backend dijalankan lagi; pilihan yang masih eligible boleh dipulihkan secara aman; pilihan yang kini tidak layak harus dicabut dan diberi alasan. Tidak boleh muncul status hijau hanya karena FE mengubah state lokal.
6. Aksi memperbaiki data membutuhkan permission dan workflow milik halaman tujuan. Jika butuh SK, maker-checker, atau izin khusus, ikuti workflow tersebut; klik CTA **tidak pernah mengubah database otomatis**.
7. Tanggal efektif penempatan mengacu SK terakhir yang sebenarnya. Jangan mengubah tanggal sejarah menjadi tanggal hari ini hanya untuk meloloskan validasi. Penyesuaian gaji tetap dalam domain Payroll, terpisah dari batch penempatan.
8. Pesan keberhasilan harus berasal dari hasil API dan tetap benar sesudah refresh/re-login. Error server, unauthorized, konflik versi, dan validation errors tampil jujur.

**Keputusan canonical yang TIDAK BOLEH DIUBAH:** `HRD-DEC-054` (initial batch sekali per pegawai, semua baris satu batch valid sebelum diajukan, approval satu kali oleh aktor berbeda, audit, salary batch terpisah), `HRD-DEC-031` (maker-checker), `HRD-DEC-049` (satu primary yang tepat dan tanggal efektif), `HRD-DEC-048` (perubahan setelah data awal melalui SK/event), `HRD-DEC-036` (alur assignment tertentu terpisah), serta guard Closed attendance/kompetensi klinis pada Menu 07. **Keputusan baru ialah kemampuan memilih kandidat yang eligible sebelum batch dibentuk, bukan membolehkan batch invalid diajukan.**

### 2.3 Model kategori diagnostik yang diusulkan (belum ada dalam API existing)

| Reason code usulan, bukan enum existing | Kategori | Penanganan |
|---|---|---|
| `ELIGIBLE_INITIAL_PLACEMENT` | SIAP_DIPROSES | Bisa dipilih bila master, status, tanggal, dan konflik batch lolos |
| `ALREADY_HAS_ACTIVE_PRIMARY_POSITION` | DILEWATI / PERLU_REKONSILIASI | Lihat penempatan aktif beserta provenance SK/batch. Jika sah dan final → dilewati. Jika legacy/duplikat/riwayat salah → review HRD, jangan langsung edit |
| `POSITION_NOT_ASSIGNED` | PERLU_PERBAIKAN | Pilih jabatan yang **sudah ada** melalui editor kandidat bila diizinkan; jika master jabatan belum tersedia, arahkan ke master Posisi |
| `POSITION_DEPARTMENT_MISMATCH` | PERLU_PERBAIKAN | Tinjau master Posisi–Departemen/penugasan, pertahankan aturan SK/perubahan yang berlaku |
| `ORGANIZATION_UNIT_DEPARTMENT_MISMATCH` | PERLU_PERBAIKAN | Tinjau master Unit Organisasi dan sumber Penempatan Organisasi |
| `EMPLOYEE_INACTIVE_OR_MISSING` | PERLU_PERBAIKAN / DILEWATI | Buka Profil Pegawai; hanya koreksi jika ada bukti/status resmi yang benar |
| `MANAGER_INVALID_OR_SELF` | PERLU_PERBAIKAN | Tinjau relasi Atasan Langsung; jangan izinkan manager=self atau siklus hierarki |
| `EFFECTIVE_DATE_UNVERIFIED` | PERLU_REKONSILIASI | Verifikasi SK/histori dan tanggal efektif, bukan isi tanggal default tanpa bukti |
| `ALREADY_IN_APPROVED_INITIAL_BATCH` | DILEWATI (TIDAK ELIGIBLE) | Tidak boleh masuk batch awal kedua; buka riwayat batch, bukan override |
| `DUPLICATE_IN_SELECTION_OR_CONCURRENT_CHANGE` | PERLU_PERBAIKAN | Cegah create/approve/execute sampai daftar valid setelah re-query server |

**Aturan klasifikasi:** Master yang salah tidak boleh dipaksakan “Valid”; record yang sudah sah harus dilewati; entri historis yang belum jelas harus direkonsiliasi. Kategori di atas adalah **desain target**; Agent boleh memakai nama kode yang konsisten dengan style dan kontrak existing setelah dicek, tetapi perilaku dan tesnya wajib sama.

### 2.4 Instruksi per Submenu resmi Menu 07

### Submenu 1 - Penempatan Awal

**Tujuan dan aktor:** HR Admin membuat batch baseline dari pegawai existing yang layak; HR Manager/`CorporateHr` berwenang menyetujui dengan pemisahan aktor; petugas berwenang mengeksekusi; audit mencatat pembuat/pengaju/penyetuju/tanggal/perubahan. Target awal adalah penetapan unit, departemen, posisi, atasan, dan tanggal efektif yang konsisten, bukan input ulang identitas atau nominal gaji.

**Data masuk/keluar:** `WorkforceProfileId`, `OrganizationUnitId`, `DepartmentId`, `PositionId`, `ManagerWorkforceProfileId` bila valid, `EffectiveStartDate`, nomor batch, status, alasan diagnostik per baris, riwayat validasi. Jangan mengekspos gaji melalui respons pembaca batch biasa.

**Happy path baru:** buka template kandidat riil → BE klasifikasikan semuanya → FE tampilkan ringkasan dan tab kandidat → HR pilih yang eligible → validasi subset terpilih ke backend → buat batch dari subset tersebut → `Draft/Validated → PendingApproval → Approved → Executed` mengikuti transisi resmi → setiap pemanggilan backend memverifikasi ulang kelayakan/izin, menjaga batch valid dan audit.

**Alur pengecualian:** klik `Perbaiki Data` → resolve target resmi dan izin → buka data spesifik → koreksi oleh user melalui endpoint pemilik → kembali menggunakan konteks tersimpan → refetch+revalidate → status berubah hanya bila server mengonfirmasi. Jika pengguna tidak punya izin, tampilkan `Tidak memiliki akses`/permintaan tindak lanjut; jangan lewatkan access control.

**Perbaikan BE langsung — task `FIN-HRD-07-BE-006`, `BE-008`, `BE-009`:**
- Refactor seleksi template dan validasi agar menggunakan satu mesin aturan yang sama untuk preview dan `POST /validate`; hindari N+1 untuk pengecekan assignment aktif jika aman dioptimalkan; tidak menandai `Valid` dari tes subset yang lebih lemah.
- Rancang metadata diagnostik **additive/backward-compatible** pada `InitialBatchRowResponse` atau endpoint diagnostik tambahan bila envelope existing perlu dipertahankan. Tetapkan field typed seperti `eligibilityStatus`, `reasonCode`, `reasonMessage`, `targetResourceKey`, `targetRecordId` yang aman/opsional; jangan mengembalikan URL eksternal arbitrer dari server.
- Klasifikasi penempatan aktif menggunakan sumber historis/effective-date dan provenance batch/SK. `ALREADY_HAS_ACTIVE_PRIMARY_POSITION` **bukan** alasan untuk menghapus assignment yang sah. Bedakan approved/executed earlier batch, legitimate legacy, dan anomali yang perlu rekonsiliasi; simpan jejak audit.
- Kontrak create mempertahankan request `CreateInitialBatchRequest`/`Rows`. Validasi server **hanya** terhadap rows yang benar-benar dipilih, tanpa mempercayai `isValid`, `eligibilityStatus`, alasan, maupun ID yang dimanipulasi client. Hindari memecahkan perilaku draft lama tanpa mengaudit konsumen; UI baru harus mengirim subset valid, backend harus menolak izin/kandidat tak layak.
- Pertahankan atau perketat pembatasan satu pegawai satu initial batch, satu primary efektif, siklus approval, status validasi yang boleh dimutasi, revalidation sebelum approve dan execute, transaksi all-or-nothing, idempotensi dan concurrency. Jangan menempatkan pegawai saat baru klik Simpan Batch.
- Untuk old unit baseline bertanggal salah, ikuti `HRD-DEC-054`: koreksi berdasar SK terakhir hanya dengan jalur rekonsiliasi sah, bukan otomatis skip seluruh pegawai dengan assignment historis atau membuat SK baru tanpa keputusan.

**Perbaikan FE langsung — task `FIN-HRD-07-FE-002`, `FE-007`:**
- Tampilkan 4 kategori, summary count `Total Terbaca | Siap | Dilewati | Perlu Perbaikan | Perlu Rekonsiliasi` dan status `dipilih N`; tombol `Simpan & Buat Batch (N Pegawai)` aktif hanya jika **N > 0** dan **semua N terpilih** lolos validasi backend terbaru; baris tidak dipilih tidak menghalangi batch. Jangan memberi label '37 dapat diproses' jika server saat itu belum memastikan.
- Aksi row `Lihat Alasan`, `Perbaiki Data`, `Tinjau Penempatan`, dan filter status; pilih semua **yang eligible saja**; tidak boleh mencentang baris `Dilewati`, `Perlu Perbaikan`, `Perlu Rekonsiliasi`.
- Buat modal konfirmasi berisi N terpilih, jumlah dilewati, jumlah error, unit dan tanggal efektif ringkas; peringatan `Simpan Batch ≠ Eksekusi Penempatan`.
- Simpan konteks sesi terbatas sebelum navigasi, kembali ke batch dan refetch/revalidate, tangani 401/403/409/422 serta perubahan data concurrent dengan pencabutan seleksi tidak sah; aksesibilitas/responsive/base components wajib.

**Acceptance:** pada sampel 41/37/4, jika 37 masih eligible, pengguna berizin bisa membentuk batch valid dari 37, sementara 4 tetap tidak tersimpan dan 2 yang sudah sah tetap dilindungi. Keluaran batch mencatat tepat 37 baris, tidak 41 atau 4 error; refresh tetap 37; approve oleh creator ditolak; execute ganda tidak menggandakan assignment. Angka contoh hanya snapshot UAT, test harus juga berlaku untuk N berapa pun.

### Submenu 2 - Perpindahan Unit

**Fungsi:** mengajukan perpindahan antarunit/departemen melalui SK kepegawaian, dengan alasan, unit asal/tujuan, tanggal efektif, pemeriksaan jadwal/kompetensi klinis dan persetujuan sesuai decision canonical. Reuse `UnitTransfersView` + `PersonnelActionOrderListView`/service SK dan backend `HrdPersonnelActionOrder*`, bukan membuat form mutasi baru.

**Dampak perbaikan batch:** tautan pada kesalahan `ORGANIZATION_UNIT_DEPARTMENT_MISMATCH` boleh mengarahkan ke **halaman tinjau Penempatan Organisasi** atau Master Unit Organisasi; **tidak boleh** otomatis menjalankan mutasi sebagai perbaikan data awal. Bila perubahan nyata terhadap penempatan resmi diperlukan, arahkan ke alur SK Mutasi sesuai `HRD-DEC-048`; jika attendance Closed atau kompetensi belum sah, backend menolak.

**BE/FE dan kontrol:** Pertahankan `FIN-HRD-07-BE-002/004/005` serta `FIN-HRD-07-FE-003`; SK dan scheduled apply menjaga effective-date, cancel approval, role, tidak menimpa assignment tanpa dasar. Saat menambah CTA lintas halaman, hindari bypass ke direct CRUD.

**Acceptance & regresi:** data mutasi lama tetap terlihat, kategori `Mutation` terisi benar, pengajuan/approval/penolakan/cancel melalui API nyata, perubahan efektif baru terjadi pada waktunya, pemilik batch yang kembali tidak kehilangan state, dan tidak ada mutasi otomatis hanya karena CTA `Perbaiki Data`.

### Submenu 3 - Rotasi Promosi

**Fungsi:** peristiwa SK rotasi jabatan setara atau promosi jenjang, mempertahankan histori jabatan dan tanggal efektif. Reuse `RotationPromotionsView`, tiga tab `Rotation/Promotion/Semua`, dan `PersonnelActionOrder` yang sama; perubahan gaji tidak diproses di form ini.

**Dampak perbaikan batch:** jika candidate memiliki jabatan yang sudah sah, ia tidak boleh diproses ulang lewat Penempatan Awal. Untuk perubahan jabatan ke depan gunakan SK `Rotation` atau `Promotion` yang benar. CTA pada status `ALREADY_HAS_ACTIVE_PRIMARY_POSITION` **hanya membuka tinjauan**, tidak otomatis membuat SK.

**BE/FE dan kontrol:** Pertahankan `FIN-HRD-07-BE-001/002/005`, `FIN-HRD-07-FE-004`. Verifikasi param kategori URL di editor, maker-checker, tanggal efektif, status Scheduled/Applied, scan signed SK, dan jalur reversal canonical; jangan membuat gaji dari promosi secara diam-diam.

**Acceptance & regresi:** form selalu menampilkan kategori yang berasal dari tab sah, data tersimpan pada kategori benar, tidak terjadi perubahan salary assignment, CTA perbaikan batch tidak salah me-redirect menjadi Promosi atau Mutasi.

### Submenu 4 - Uraian Tugas

**Fungsi:** menampilkan uraian tugas yang efektif sesuai jabatan dan tanggal acuan; master uraian tugas tetap milik Menu 05. Reuse `EmployeeJobDescriptionsView` dan `WorkforceCoreCrossEmployee*`/master JobDescription; jangan menggandakan tabel/endpoint master baru untuk guided correction.

**Dampak perbaikan batch:** setelah batch disetujui dan efektif, uraian tugas harus mengikuti posisi efektif dari backend; sebelum efektif tetap menampilkan posisi lama. Jika jabatan kandidat belum tersedia, tautan menuju master Posisi, **bukan** mengarang uraian tugas agar kandidat dianggap valid.

**BE/FE dan kontrol:** Pertahankan `FIN-HRD-07-BE-007/008` dan `FIN-HRD-07-FE-005`; response nested DTO (`jobDescription`, `items`, `versionHistory`) harus dipetakan benar dan as-of date harus mengutamakan riwayat effective assignment, bukan mirror terkini.

**Acceptance & regresi:** uraian tugas historis benar per tanggal, tidak ada fallback data dummy, response 403 dibedakan dengan empty state, dan alur perbaikan master jabatan tidak merusak baca uraian tugas.

### Submenu 5 - Atasan Langsung

**Fungsi:** melihat dan menindaklanjuti hubungan atasan pegawai serta rantai pelaporan. Penetapan/perubahan atasan harus mengikuti approval/permission dan tidak boleh menciptakan siklus manajerial.

**Dampak perbaikan batch:** jika alasan invalid `MANAGER_INVALID_OR_SELF`, CTA mengarahkan ke tinjauan `EmployeeManagersView`/assignment terkait; jika perubahan resmi dibutuhkan, gunakan workflow yang sesuai dengan keputusan canonical, **bukan** redirect ke SK default kategori `Mutation` tanpa verifikasi. Jika manager belum ada, formulir kandidat boleh memilih atasan **hanya** dari master yang sah dan dengan pemeriksaan backend.

**BE/FE dan kontrol:** Pertahankan `FIN-HRD-07-BE-004/008` dan `FIN-HRD-07-FE-006`; validasi tidak menerima self-manager, manager nonaktif, rantai siklus, konflik tanggal efektif, atau direct CRUD bypass.

**Acceptance & regresi:** struktur atasan benar di daftar/detail, perubahan berizin terekam dan disetujui, 403 jelas, dan CTA guided correction tidak melewati workflow atau menambahkan assignment ganda.

### 2.5 Skema tampilan Frontend WAJIB (bukan hanya menyebut 'buat UI bagus')

**Halaman:** `/hr/workforce-core/initial-placements` — pertahankan layout existing, `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `InformationAlert`, `StatusBadge`, `BaseButton`, `ConfirmModal`, `ToastStack`, `BaseInputField`/base-form-control dan token dari Engineering Skills. **Jangan mengganti logo, warna tema, atau sidebar.**

```text
[Hero] Human Resource > Penempatan, Mutasi & Promosi
       Batch Data Awal Penempatan                         [+ Buat Batch Baru]
[Info] Inisialisasi satu kali; mutasi/promosi berikutnya harus melalui SK

MODAL: Buat Batch Baru Penempatan Awal
+-----------------------------------------------------------------------+
| Nama Batch [________________________________________]                  |
| Ringkasan: Total | Siap | Dilewati | Perlu Perbaikan | Rekonsiliasi     |
| [Siap Diproses] [Dilewati] [Perlu Perbaikan] [Rekonsiliasi]             |
| Cari pegawai / unit / alasan                       [Validasi Ulang]     |
|-----------------------------------------------------------------------|
| [x] Pegawai | Unit/Dept | Jabatan | Tgl Efektif | Status | Aksi        |
| [x] P-001   | IGD       | Perawat | ...         | Siap   | Lihat       |
| [ ] P-002   | ...       | ...     | ...         | Lewati | Tinjau      |
| [ ] P-003   | ...       | -       | ...         | Perbaiki| Perbaiki Data|
|-----------------------------------------------------------------------|
| [Info] Dipilih: N | Dilewati: K | Perlu perbaikan: M                  |
| [Batal] [Simpan & Buat Batch (N Pegawai)]                            |
+-----------------------------------------------------------------------+

DETAIL MASALAH (inline panel/drawer kecil, bukan deretan card memanjang):
  Pegawai: <nama yang memang diizinkan tampil>
  Masalah: Jabatan tidak sesuai dengan departemen aktif
  Sumber data: Master Posisi / Penugasan Organisasi
  Solusi: Tinjau relasi; simpan lewat izin/approval yang benar
  [Buka Halaman Perbaikan] [Tutup]

HALAMAN TUJUAN (pemilik master data):
  Breadcrumb: Perbaikan Data Penempatan Awal > Master Posisi
  Data terkait dipilih / difilter jika resolusi ID aman
  [Simpan perubahan]  [Kembali ke Batch]

SETELAH KEMBALI:
  [Memuat ulang dari server] [Validasi ulang]
  [Status kandidat terbaru] [Perubahan seleksi jika stale]
```

**Aturan visual/UX yang konkret:** tabel responsif, kolom Catatan dapat dibaca (wrap atau detail), tombol Perbaiki Data berada di baris/panel kesalahan bukan di Header global, footer modal tetap terlihat, tidak memotong tombol di tablet, pemilihan fokus/keyboard/tab/Enter/ARIA status pesan jelas, disabled tombol disertai alasan, loading/empty/403/409 state berbeda. **Jangan membuat empat puluh satu card memanjang**; pertahankan tabel ringkas dengan tab/filter, detail issue kontekstual.

### 2.6 Destination/route mapping yang harus didukung

| Masalah | Destination yang TERBUKTI punya route FE | Kedalaman navigasi yang harus dikerjakan Agent |
|---|---|---|
| Master jabatan/posisi tidak tersedia atau relasi posisi–departemen keliru | `/hr/master-data/organization?type=positions` | Buka tab Posisi; jika bisa resolve `positionId`, gunakan helper route-token existing untuk detail/update **setelah permission**; jangan hardcode raw URL `[slug]` karena aplikasi memakai private route token |
| Master departemen bermasalah | `/hr/master-data/organization?type=departments` | Tunjuk departemen terkait dengan filter/konteks aman; gunakan halaman edit official |
| Unit organisasi vs departemen tidak selaras | `/hr/master-data/organization-unit` | Gunakan halaman detail/update resmi untuk `organizationUnitId` valid; bila data adalah assignment, tinjau juga penempatan organisasi |
| Sumber penempatan organisasi perlu ditinjau | `/hr/workforce-core/organization-assignments` | **List/review**, bukan izin direct edit otomatis; perubahan material lanjut SK yang sah |
| Profil pegawai/status aktif bermasalah | `/hr/data-induk-pegawai/profil` | Halaman existing menerima `?employeeId=...`, tetapi **pastikan mapping `WorkforceProfileId` → employee ID nyata**, jangan kirim WFP ID seolah ID master pegawai |
| Sudah punya jabatan aktif | `/hr/workforce-core/position-assignments` | `Tinjau Penempatan`; jika sah → `Dilewati`; jika anomali → proses resmi/reconciliation |
| Atasan langsung bermasalah | `/hr/workforce-core/employee-managers` | Tinjau relasi manager; perubahan mengikuti workflow yang benar, bukan mutasi default |
| Kategori SK baru yang benar-benar diperlukan setelah approval | `/hr/workforce-core/personnel-actions/create` | Hanya jika keputusan kategori/role/asal peristiwa sah; jangan otomatis create mutation/promotion dari tombol perbaikan |

**Implementasi Deep Link Aman:**
- BE mengirim *reasonCode* dan entitas domain yang aman, **bukan** URL bebas. FE punya allowlist `reasonCode → routeKey/path`, menggunakan `router.push` internal dan permission gate; tolak nilai dari user/URL yang bukan route allowlisted (**anti open redirect**).
- Untuk master organisasi, `use-master-data-organization.jsx` memakai `registerPrivateRouteToken` sebelum masuk detail. Reuse mekanisme itu melalui fungsi util/resolver aman; jangan menyuntik langsung `id` ke `[slug]` jika route mengharuskan token sesi.
- Jika entitas tidak dapat dipetakan, tampilkan penjelasan dan arahkan ke daftar/filter resmi dengan pencarian yang aman; tidak boleh deep-link ke pegawai salah.
- Tombol `Perbaiki Data` **hanya navigasi**, tidak pernah melakukan POST/PUT/PATCH/DELETE atau approval secara otomatis.
- Simpan sementara `batchDraftName`, `selectedWorkforceProfileIds` (tanpa PII/nominal/credential), tab/filter, dan `returnTo` internal yang telah divalidasi, menggunakan pola existing. Tandai sesi kedaluwarsa/berubah; jangan simpan pilihan sebagai sumber kebenaran. Jika browser refresh, data live wajib dipakai lagi.
- Setelah kembali, bandingkan hasil server terhadap pilihan lama: baris yang sudah tidak eligible otomatis dicabut dan disertai alasan; jangan membalik `isValid=true` di FE tanpa panggilan server.

### 2.7 Matriks tugas FIN — tetap gunakan nomor lama

| FIN existing | Fokus perbaikan dalam revisi UAT | Hasil wajib |
|---|---|---|
| `FIN-HRD-07-BE-001` | Jaga maker-checker dan signed SK | Tidak ada approval diri sendiri/scan SK bypass pada jalur koreksi |
| `FIN-HRD-07-BE-002` | Jaga state machine pembatalan SK | CTA koreksi tidak mengabaikan Scheduled/Cancelled/Applied |
| `FIN-HRD-07-BE-003` | Hindari leak salary di diagnostic/list; pertahankan nomor SK aman | Response pembaca batch tidak expose gaji |
| `FIN-HRD-07-BE-004` | Guard perubahan direct organisasi/jabatan/atasan | Navigasi koreksi tidak berubah jadi bypass CRUD |
| `FIN-HRD-07-BE-005` | Effective-date worker/atomicity | Penempatan masa depan tidak langsung mengubah mirror |
| `FIN-HRD-07-BE-006` | **UTAMA:** template eligibility + konsistensi validasi + subset + approval/concurrency | Batch dipilih saja, seluruh row valid, tanpa duplikat |
| `FIN-HRD-07-BE-007` | Regression uraian tugas as-of date | Data historis tetap benar |
| `FIN-HRD-07-BE-008` | Reason code/DTO+izin dan navigasi contextual | FE menerima diagnosis typed yang aman |
| `FIN-HRD-07-BE-009` | Unit/integration/security/concurrency tests | Bukti HTTP+DB test sesuai aturan |
| `FIN-HRD-07-FE-001` | Route, sidebar, permission, return navigasi | Link menuju route existing yang benar |
| `FIN-HRD-07-FE-002` | **UTAMA:** tab, pilihan, CTA koreksi, persist konteks, refetch | FE mengirim hanya selected eligible; error jelas |
| `FIN-HRD-07-FE-003` | Regression Mutasi | Tidak ada mutasi otomatis saat guided correction |
| `FIN-HRD-07-FE-004` | Regression Rotasi/Promosi dan kategori SK | Tidak mengirim kategori yang salah |
| `FIN-HRD-07-FE-005` | Regression Uraian Tugas | Effective position/DTO tetap benar |
| `FIN-HRD-07-FE-006` | Regression Atasan Langsung | Tidak ada manager=self/cycle/bypass |
| `FIN-HRD-07-FE-007` | Base components, responsive, keyboard, E2E | Modal/tabel/route/validasi bekerja dan diuji |

---

## 3. Context

### 3.1 Source snapshot GitHub read-only, **bukan** hasil edit

| Peran | Repository @ branch | HEAD diperiksa saat penulisan |
|---|---|---|
| FE | `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2` | `0fc88be5c64e68d105c7c75adbca2e7104d8de4a` |
| BE | `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | `b9a9ae08709fe103eeb0cbaff29682e8ecc2a246` |
| Governance | `DevBenari/QuilvianEngineeringSkills` @ `main` | `201af64552b27873fd9af83c6722cf11b441bb1e` |

**Agent wajib verifikasi HEAD ulang pada runtime; snapshot dapat berubah.** Source yang dibaca dalam penyusunan:

- BE: `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs` → `GetTemplateRowsAsync`, `ValidateRowsAsync`, `CreateBatchDraftAsync`, `ExecuteBatchAsync`.
- BE: `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdInitialPlacementBatchController.cs` → `GET /template`, `POST /validate`, `POST /`, `POST /{id}/submit|approve|execute`.
- BE: `Areas/Corporate/HumanResource/WorkforceCore/DTOs/` → kontrak initial batch, ditemukan dari controller/service, tentukan file DTO persis sebelum ubah.
- FE: `src/app/hr/workforce-core/initial-placements/page.jsx` → route submenu; `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` → modal, summary, disabled/button, payload; `src/lib/services/hr/initial-placement-batch-service.js` → Axios API wrapper.
- FE: `src/lib/hooks/hr/master-data/organization/use-master-data-organization.jsx` → list/master tab query `?type=positions|departments`, route token untuk detail.
- FE: `src/components/view/hr/data-induk-pegawai/data-induk-pegawai-view.jsx` → menerima `?employeeId=...`, cek pemetaan ID lintas domain.
- FE route tujuan: `src/app/hr/master-data/organization`, `organization-unit`, `src/app/hr/workforce-core/{organization-assignments,position-assignments,employee-managers}`, dan `src/app/hr/data-induk-pegawai/profil`.
- FE related: `unit-transfers-view.jsx`, `rotation-promotions-view.jsx`, `employee-job-descriptions-view.jsx`, `employee-managers-view.jsx`, service SK/WorkforceCore existing.

**Kontrak API existing (jangan tebak URL baru):**

```text
GET    /api/v1/corporate/human-resource/workforce-core/initial-batches/template
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches/validate
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches
GET    /api/v1/corporate/human-resource/workforce-core/initial-batches
GET    /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/validate
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/submit
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/approve
POST   /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute
```

Kontrak `GET template` **saat ini** list of `InitialBatchRowResponse`, bukan response `groups` atau `eligibilityStatus` lengkap; field baru di 2.3 adalah **PROPOSED**, harus implementasikan BE lalu FE secara backward-compatible. FE sudah memakai `unwrap(response?.data?.data ?? response?.data)` pada service; jangan tambah Axios instance.

### 3.2 Canonical decision dan dokumen existing

- `docs/module-blueprints/human-resource/00-interview-decisions.md` → `HRD-DEC-054`, `HRD-DEC-031`, `HRD-DEC-049`, `HRD-DEC-048`, `HRD-DEC-036`, keputusan SK dan hak akses terkait.
- Master Menu 07 existing berisi 569 baris pada snapshot analisis: `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/INSTRUKSI_AGENT_HRD_MENU_07_PENEMPATAN_MUTASI_DAN_PROMOSI.md`.
- Laporan Agent existing yang **bukan** bukti runtime PASS: `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md`.
- Roadmap `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md` dan task report terkait; jangan mengganti status historis menjadi DONE tanpa bukti.
- **Potensi konflik yang wajib ditangani secara konservatif:** `HRD-DEC-054` mengizinkan batch baseline mengoreksi data penempatan unit lama dengan tanggal yang keliru berdasarkan SK sebenarnya. Karena itu **tidak boleh** men-skip semua pegawai berpenempatan aktif secara buta; bedakan penempatan sah, pernah diproses batch sebelumnya, dan legacy yang butuh rekonsiliasi. Jika status provenance tidak dapat dibuktikan, tandai `PERLU_REKONSILIASI`, bukan `VALID` atau `DILEWATI` seolah sudah pasti.

### 3.3 Hak akses, keamanan, dan invariant database

- Backend adalah pemutus kelayakan; client tidak boleh memilih identifier yang tidak dia miliki, mengabaikan invalid dengan memalsukan `IsValid`, atau mem-bypass workflow dengan direct HTTP.
- Permission `Read`, `Create`, `Approve`, dan operasi write validasi harus sesuai controller AccessController/AccessAction/AccessPermission. Uji 401/403 dan larangan self-approval.
- Tidak ada penghapusan/mutasi data jabatan/atasan yang benar hanya untuk menghilangkan label invalid.
- Satu penempatan primer yang sah pada suatu waktu; tidak boleh overlapping, tidak boleh memutasi mirror sebelum effective date; eksekusi batch atomik; idempotency/exactly-once selama retry; audit DB actor/time/source.
- Master pegawai, jabatan, departemen, unit organisasi, relasi atasan, kredensial klinis dan SK tetap menjadi **pemilik data masing-masing**, jangan membuat table duplikat untuk guided correction.
- Jangan log/menampilkan data sensitif (gaji, identitas penuh, NIK, credential) dalam diagnosa error, query string, atau laporan QA. Screenshot UAT berisi nama pada bukti tester; laporan publik/anonymized hanya menggunakan kategori dan angka.

---

## 4. Reasoning / Execution Workflow

> Bagian ini adalah prosedur kerja **terukur**, bukan permintaan agar Agent mengungkap private chain-of-thought. Setelah preflight sah, langsung laksanakan perubahan kode.

### 4.1 Preflight dan penguncian kontrak (singkat, **bukan audit-only**)

1. Pastikan branch exact `HamzahV2`/`MHamzah`, HEAD baru, working tree, upstream, owner, target tulis dan governance yang terpasang. Jika ada perubahan sejak snapshot, hitung impact hanya pada area berkaitan.
2. Baca master Menu 07 existing dan keputusan `HRD-DEC-054`; **merge** cakupan revisi ini ke master lama tanpa menghapus 18 temuan dan 16 FIN task. Bila perubahan dokumen canonical memerlukan `MODULE BLUEPRINT MODE`, lakukan hanya jika punya izin; jika tidak, tandai `DOC UPDATE BLOCKED BY MODE` dan kerjakan source yang independen setelah kontrak valid.
3. Kunci kontrak baris diagnostik additive, mapping ID pegawai, source assignment/provenance, aturan selected subset, dan return-to-batch. Gunakan API existing lebih dahulu. Jangan membuat endpoint baru tanpa alasan kompatibilitas dan kontrak yang jelas.

### 4.2 Implementasi **BACKEND dahulu** — `FIN-HRD-07-BE-006/008/009`

1. Dalam `BACKEND MODE`, **invoke** `quilvian-engineering-skills:build-module-backend`, lalu perbaiki `GetTemplateRowsAsync` agar tidak menganggap seluruh penugasan organisasi aktif otomatis eligible. Resolusi validitas harus mengamati status nyata, primary/effective date, riwayat batch, master posisi/departemen, unit organisasi dan manager; bedakan `SKIPPED` vs `NEEDS_REPAIR` vs `NEEDS_RECONCILIATION`.
2. Satukan kriteria preview dan validasi server; desain agar front tidak mendapat label `Valid` palsu sebelum pemeriksaan lengkap. Jika expensive, berikan loading state/progressive load yang jujur dan hasil final yang konsisten; gunakan query set-based untuk mengurangi N+1 jika memungkinkan.
3. Berikan metadata diagnostik typed, reason code stabil, dan target entity key yang diperiksa. Tidak mengembalikan string URL eksternal. Pastikan nested errors rapi dan aman untuk user.
4. Pastikan `ValidateRowsAsync` menerima selected subset, reject duplikasi, ineligible dan data stale, serta memverifikasi referensi master/assignment efektif saat request berlangsung. `CreateBatchDraftAsync` harus memastikan hasil untuk rows terpilih konsisten, tanpa merusak backward compatibility Draft existing; untuk alur UI valid-only, jangan menyimpan 4 invalid diam-diam.
5. Guard concurrency: 2 request create/approve/execute bersamaan tidak boleh memasukkan satu pegawai ke dua initial batch yang sah atau menggandakan primary. Pertahankan transisi dan permission `POST /{id}/validate` write-only seperti mandat master lama.
6. Cek cross-module integration SK, Menu 08 onboarding, salary batch terpisah; perbaiki hanya area yang terdampak dengan izin, tidak refactor massal.
7. Tulis unit/integration tests: eligibility, legacy reconciliation, invalid selected, profile inactive, multiple active positions, manager cycle, date/time, self-approval, stale state, API 403/409, concurrent submit/approve/execute, all-or-nothing.
8. Kunci dan dokumentasikan kontrak FE/BE aktual beserta respons validasi dan guard; frontend baru mulai setelah backend benar dan dapat diuji. Jika perlu migration, **stop hanya bagian itu** dan minta izin, jangan jalankan migration otomatis.

**Contoh skema respons TARGET (proposal, bukan endpoint/payload existing):**

```json
{
  "workforceProfileId": "<opaque-id>",
  "employeeName": "<display-name-authorized>",
  "eligibilityStatus": "NEEDS_REPAIR",
  "reasonCode": "POSITION_DEPARTMENT_MISMATCH",
  "reasonMessage": "Jabatan tidak selaras dengan departemen pegawai.",
  "targetResourceKey": "MASTER_POSITION",
  "targetRecordId": "<opaque-id-if-authorized>",
  "isValid": false
}
```

**Kontrak di atas bersifat rancangan.** Jangan mengisi nilai dummy ke response runtime; gunakan ID/data hasil repository yang berwenang. Hindari public detail dengan ID sensitif; ubah DTO hanya secara additive atau adapt endpoint version bila perlu, dengan test caller lama.

### 4.3 Implementasi **FRONTEND setelah BE** — `FIN-HRD-07-FE-001/002/007`

1. Pindah ke `FRONTEND MODE` dengan izin sah, invoke `quilvian-engineering-skills:build-module-frontend` terlebih dahulu. Pastikan definisi DTO/response backend final terbaca read-only.
2. Implementasikan view dan modal target pada 2.5 dengan **base components existing**, CSS Modules/design tokens, shell/layout/menu existing. Jangan membuat base component baru bila belum melewati decision gate.
3. Tambahkan pemilihan kandidat by stable `WorkforceProfileId`, centang semua eligible, summary kategori, validasi ulang, keterangan alasan, CTA `Perbaiki Data` dan `Tinjau Penempatan`. Hilangkan ketergantungan `templateInvalidCount > 0` global sebagai guard seluruh template; ganti guard berdasarkan **selectedRows** yang tervalidasi terbaru, dengan 0 terpilih dan loading tetap disabled.
4. Kirim payload hanya daftar selected valid rows. Setiap perubahan/pemilihan ulang membatalkan hasil validasi lama atau memerlukan revalidasi; tangani respons 409/422 jika live backend menemukan perubahan data sebelum create.
5. Implementasikan safe route resolution (2.6), `returnTo` internal allowlisted, preserve context sementara, refetch/revalidate otomatis saat kembali, mismatch ID mapping → fallback aman, error state/permission, dan tetap memberi pilihan `Kembali ke Batch` secara manual.
6. Jangan menavigasi otomatis ketika validasi gagal. CTA harus dipicu klik pengguna; perubahan data tetap dilakukan pengguna pada halaman tujuan. Tanpa permission, tampilkan penolakan yang informatif.
7. Tambahkan automated tests untuk seleksi valid-only, klik tautan per kategori, aman dari open redirect, missing record/mapping, restore context, stale candidates, transform response, keyboard/navigation, modal/table mobile.

### 4.4 QA/UAT browser dan regression — jalankan bila akses dan izin tersedia

| TC ID | Risiko | Skenario | Expected |
|---|---|---|---|
| `HRD07-FIN-UAT-01` | P0 | 41 kandidat, 37 valid, 4 invalid; pilih 37 | Batch tersimpan hanya 37, tidak ada side effect pada 4 |
| `HRD07-FIN-UAT-02` | P0 | 2 pegawai sudah punya primary active yang sah | Dilewati, penempatan lama utuh, tidak perlu menghapus jabatan |
| `HRD07-FIN-UAT-03` | P0 | Pegawai punya legacy assignment bertanggal SK keliru | Masuk Perlu Rekonsiliasi, tidak auto-skip/auto-overwrite; jalan approval resmi |
| `HRD07-FIN-UAT-04` | P0 | Direct POST dengan ID ineligible/tampered isValid | BE menolak, tidak menyimpan transaksi tidak sah |
| `HRD07-FIN-UAT-05` | P0 | Valid saat preview, kemudian posisi berubah/nonaktif di tab lain sebelum create | Error konflik/validasi spesifik; tidak memaksa sukses |
| `HRD07-FIN-UAT-06` | P0 | 2 batch berbarengan untuk pegawai sama | Hanya satu hasil yang sah, audit benar |
| `HRD07-FIN-UAT-07` | P0 | 1 dari N baris invalid tepat sebelum execute | 0 dari N penempatan diterapkan, batch tetap konsisten |
| `HRD07-FIN-UAT-08` | P0 | Creator mencoba approve sendiri | Ditolak oleh backend dengan status/penjelasan semestinya |
| `HRD07-FIN-UAT-09` | P1 | Jabatan tidak ada; klik Perbaiki Data | Master Posisi terbuka pada tab benar; dapat kembali |
| `HRD07-FIN-UAT-10` | P1 | Posisi–departemen tidak konsisten | Target dibuka dengan konteks aman; edit via permission sah; revalidasi memperbarui status |
| `HRD07-FIN-UAT-11` | P1 | Unit organisasi tidak cocok | Master Unit Organisasi atau penempatan organisasi relevan, bukan mutasi otomatis |
| `HRD07-FIN-UAT-12` | P1 | Profil pegawai inactive / ID WFP bukan employee ID | Tidak membuka pegawai salah; fallback/403 jelas |
| `HRD07-FIN-UAT-13` | P1 | Manager=self/cycle; klik Perbaiki Data | Tinjau atasan; backend tetap menolak assignment tidak sah |
| `HRD07-FIN-UAT-14` | P1 | Kembali dari halaman perbaikan | Nama batch dan pilihan yang masih eligible dipulihkan; data direfetch dan direvalidasi |
| `HRD07-FIN-UAT-15` | P1 | Role Read-only membuka deep link dan coba edit/approve | Akses ditolak; tidak ada bypass dari CTA |
| `HRD07-FIN-UAT-16` | P1 | Refresh/re-login setelah create/approve/execute | Data dan status tetap sama sesuai DB, bukan Redux saja |
| `HRD07-FIN-UAT-17` | P1 | Initial preview dibanding validate ulang tanpa data berubah | Hitungan dan alasan identik, atau perbedaan mendapat versi/penjelasan |
| `HRD07-FIN-UAT-18` | P1 | URL returnTo berisi host eksternal / ID tak valid | Ditolak; tidak open redirect, tidak bocor PII |
| `HRD07-FIN-UAT-19` | P1 | Regression Mutasi/Rotasi/Promosi/Atasan | SK category/approval/manager-cycle/efektif tetap sesuai canonical |
| `HRD07-FIN-UAT-20` | P2 | Modal + tabel pada desktop/tablet/mobile, keyboard | Footer tidak terpotong, pesan validasi/CTA dapat digunakan |

**UAT runtime sesudah perbaikan bukan lanjutan sesi tester saat ini:** pengguna telah menghentikan sesi testing manual. Agent hanya melakukan browser headed/live bila lingkungan/akun/izin tersedia, dan wajib mencatat `NOT RUN` jika tidak dilakukan. Jangan klaim hasil PASS dari screenshot sebelum perbaikan atau source yang sekadar terlihat benar.

### 4.5 Definition of Done (DoD) wajib

- [ ] Semua 5 Submenu tetap muncul dan berfungsi setidaknya tanpa regresi dari baseline yang terbukti.
- [ ] BE mengembalikan klasifikasi kategori dan alasan stabil dari data nyata, dengan UAT sampel 41/37/4 ditangani tanpa menyimpan invalid.
- [ ] FE hanya mengirim kandidat selected eligible, tidak diblokir oleh kandidat non-selected yang invalid.
- [ ] Pegawai sudah ditempatkan sah tidak duplikat dan tidak diubah otomatis; legacy anomali ditangani via review resmi.
- [ ] Setiap kategori masalah memiliki CTA route existing yang aman atau fallback yang jelas; pengguna dapat kembali dan memicu revalidasi tanpa kehilangan konteks yang aman.
- [ ] Branch+HEAD, API/DTO, auth, approval, history, audit, date, transaction dan concurrency diverifikasi sesuai task FIN.
- [ ] Test unit/integration, lint/CSS, build bila diizinkan, browser/UAT bila tersedia mempunyai output/hasil aktual; status `NOT RUN` jujur bila tidak ada akses.
- [ ] Report implementation existing diperbarui secara sah; 16 FIN dan 18 temuan audit lama tetap dilacak; jangan mengubah presentase menjadi 100% tanpa bukti.

---

## 5. Stop conditions

**JANGAN berhenti** hanya setelah membuat analisis, tabel temuan, mock UI, atau report. Setelah gate sah, langsung buat perubahan source nyata dan perbaiki failure yang masih dalam scope.

**STOP hanya pada area terdampak**, sambil lanjutkan pekerjaan independen jika aman, bila:

1. Branch, owner/upstream, working tree, target tulis/mode tidak sesuai instruksi; jangan checkout/menimpa perubahan orang lain.
2. `AGENTS.md`, `CLAUDE.md`, canonical rules, engineering contract tidak tersedia → `BLOCKED — canonical governance unavailable`.
3. Registered runtime skill implementasi tidak tersedia → `BLOCKED — Quilvian runtime skill unavailable`.
4. Keputusan material/semantik data lama tidak dapat ditetapkan dari SK/audit → `PERLU_REKONSILIASI` dan pertahankan existing tanpa destruksi; jangan mengarang izin override.
5. Perubahan memerlukan migration, SQL write, deployment, git mutation, akses database/role di luar izin → tahan operasi tersebut; jangan otomatis menjalankannya.
6. UAT live/browser/credential tidak tersedia → tes terkait `NOT RUN`; jangan mengklaim fitur siap produksi tanpa bukti end-to-end.

Larangan eksplisit: **no dummy success, no hardcode data pegawai, no delete/disable jabatan aktif untuk memaksa valid, no bypass workflow melalui CTA, no unverified open redirect, no silent partial batch execution, no duplicate master data**, dan no status `DONE` karena UI semata.

---

## 6. Output

Saat pekerjaan **selesai dikerjakan oleh Agent** (bukan oleh pembuat file ini), keluarkan laporan ringkas tetapi berbukti:

1. Repository, branch, HEAD awal/akhir, mode aktif, registered Skills yang benar-benar invoked, file dan simbol yang benar-benar diubah, `git status --short`.
2. Diff/summary **BE dahulu lalu FE**, termasuk rule eligibility, alasan diagnostik, DTO, endpoint, selected subset, mapping deep link, context restoration, dan transaction guards.
3. Matriks **16 FIN task existing** `BE-001..009` dan `FE-001..007`, alasan status per task; serta semua temuan audit Menu 07 existing yang terimbas.
4. Matriks **5 Submenu resmi** dan status `IMPLEMENTED / PARTIAL / REPAIR / BLOCKED / NOT VERIFIED` berdasarkan source + tes.
5. Bukti test nyata: command, output, unit/integration, API negative, database persistence di test environment berizin, browser screenshot/network jika memang dijalankan. Pisahkan **PASS**, **FAIL**, **NOT RUN**, dan **BLOCKED**.
6. Bukti revisi master existing (jika mode dokumen berizin) dan update laporan `LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md`; jangan menghilangkan history atau mengganti klaim lama tanpa evidensi.
7. Jika sinkronisasi `docs/module-blueprints/human-resource/memory.md` belum dilakukan/diizinkan, laporkan **`MEMORY_PENDING_SYNC`**, bukan mengklaim sudah tersimpan.
8. Ringkasan blocker tersisa dan satu next action yang spesifik. **Tidak ada auto commit/push/deploy/migration.**

### PERINTAH FINAL AGENT

**MULAI LANGSUNG IMPLEMENTASI PERBAIKAN SOURCE MENU 07 pada workspace yang sah.** Fokus utama: `FIN-HRD-07-BE-006/008/009` lebih dahulu, lanjut `FIN-HRD-07-FE-001/002/007` setelah kontrak backend stabil. Gunakan task FIN existing; pertahankan seluruh pekerjaan dan 5 Submenu dari master Menu 07 lama. Bangun Smart Validation & Guided Data Correction lengkap dengan selected-valid-only batch, redirect perbaikan yang aman, kembali ke batch dan validasi ulang nyata, pengamanan approval/penempatan/audit, regression testing, dan bukti hasil. **Jangan mengakhiri sesi pada tahap audit atau planning tanpa perubahan source.**
