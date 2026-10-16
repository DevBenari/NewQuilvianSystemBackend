# Laporan Perubahan Frontend — `FE-RWI-236`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-236` |
| Judul | Bed Usage History menampilkan hunian awal, snapshot dan semua versi koreksi |
| Slice | Consumer Bed Management sesuai kartu FE-RWI-236; delivery BM-DEL-20261011-01 revision1 |
| Roadmap | [frontend-roadmap-bed-management.md](../../../roadmap/frontend-roadmap-bed-management.md) — kartu FE-RWI-236 |
| Trace | FR-BM-011, FR-BM-012; RWI-DEC-286, RWI-DEC-287, RWI-DEC-288, RWI-DEC-292; RWI-AC-402, RWI-AC-415, RWI-AC-416, RWI-AC-417, RWI-AC-418; NFR-702, NFR-703, NFR-705 |
| Contract version | 0.12.0 Bed Management; decision46, child manifest11, input24/24 SHA256 cocok. Pengguna mengotorisasi seluruh tujuh task FE dalam sesi ini. Delivery lock existing berstatus APPROVED_BACKEND_IMPLEMENTATION_PARTIAL; approval FE sesi ini dicatat di laporan/roadmap, tanpa mengubah lock/manifest/kontrak. |
| Wewenang UI | DEV_DISCRETION RWI-DEC-292 untuk detail nonkritis, memakai token/base components; tidak menetapkan kebijakan bisnis, grant, SOP atau official class order |
| Dependency | `FE-RWI-231`, `BE-RWI-220`; source backend pasangan 🟡, runtime/migration belum terbukti; fondasi FE-RWI-231 🟡 |
| Klasifikasi | HEAVY — reader history dengan privasi, snapshot historis, seluruh versi serta adapter correction/Billing existing. Skor numerik baku tidak tersedia pada governance yang dibaca. |
| Task mode | FRONTEND + CROSS-REPO terbatas laporan/status/tautan bukti |
| Target tulis | QuilvianSystemFrontendDev/src dan tests terkait; backend hanya task/report/frontend, status frontend-roadmap dan dua requirement-traceability modul |
| Model | Pengerjaan awal: Codex berbasis GPT-6. Pengerjaan ulang 2026-10-14: Claude Opus 5.5 (Claude Code) |
| Commit frontend saat dikerjakan | Awal: 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2. Pengerjaan ulang: 9d890cf1c6b298e57cf79de8181012405972755e (working tree belum di-commit); branch HamzahV2 → origin/HamzahV2 |
| Commit backend yang dijadikan rujukan | Awal: db85bede83c0ddbc9aaa412c576f2c3fd7447492. Pengerjaan ulang: a1569f4787d98476b73ba5f8694e1eb73bc03588; branch MHamzah; source Bed Management dibaca apa adanya, tidak diubah |
| Tanggal | 2026-10-11 (awal); 2026-10-14 pengerjaan ulang, Asia/Jakarta |
| Status | 🟡 SEBAGIAN — pengerjaan ulang 2026-10-14: lint:errors PASS, build PASS, unit scope bed 46/46 PASS, suite penuh 2821/2853 dengan 32 kegagalan di luar scope. Acceptance penuh tetap menunggu bukti browser/API dengan akun nyata. Rincian pada bagian "Pengerjaan ulang 2026-10-14". |

## Pengerjaan ulang 2026-10-14 — riwayat tampil tanpa memilih periode

Bagian ini mencatat pengerjaan ulang atas permintaan pengguna: riwayat penggunaan harus langsung tampil tanpa memilih periode, dan tampilannya dirapikan. Desain diajukan sebagai mockup lalu disetujui pengguna sebelum implementasi. Bagian 1–8 di bawahnya adalah catatan pengerjaan awal 2026-10-11 dan dipertahankan sebagai jejak.

**Wewenang dan governance.** Task mode FRONTEND: pengguna menyetujui implementasi setelah analisis. Runtime skill `quilvian-engineering-skills:build-module-frontend` tidak terdaftar (`Unknown skill`), lalu pengguna memberi izin melanjutkan. Aturan `rules/frontend/` (architecture, design-tokens, base-component catalog dan decision gate, UI checklist, test-policy) dibaca dan diterapkan manual. Tidak ada git add/commit/push. Working tree juga berisi perubahan lain milik pekerjaan paralel (transfer, admisi, monitoring) yang tidak disentuh.

### A. Keadaan yang ditemukan

| Masalah | Dampak bagi pengguna |
| --- | --- |
| Periode default 30 hari; opsi "Semua (1 Tahun)" sebenarnya hanya 1 tahun | Bed yang terakhir dipakai lebih dari 30 hari lalu tampil kosong, sehingga pengguna tetap harus memilih periode |
| Tombol reset mengosongkan bed bila tab dibuka langsung | Muncul pesan "Silakan pilih tempat tidur", bukan data |
| Timeline tidak punya paginasi | Hanya 25 catatan pertama yang bisa dilihat |
| Batas periode memakai 00.00 UTC (07.00 WIB) dan tanggal akhir tidak dihitung | Hunian pada tanggal akhir setelah 07.00 WIB tidak ikut tampil |
| Kartu "Status Saat Ini" hanya dibaca dari riwayat | Bed yang "Menunggu Pembersihan" tampil "Tersedia / Siap ditempati" |
| Kartu "Pasien Aktif / Terakhir" berisi "Tidak ada (Kosong)" walaupun pasien terakhir ada | Informasi pasien terakhir hilang |
| Total dan rata-rata durasi ikut menghitung versi yang digantikan koreksi | Angka ringkasan menggelembung |
| Pemisah lokasi tampil sebagai `?` (byte `?` literal sejak HEAD, sisa encoding rusak) | Teks lokasi tampil "BD-001 ? BED 001 ? …" |
| Badge timeline memakai status `completed` yang tidak dikenal StatusBadge | Versi yang digantikan koreksi tampil "Selesai" berwarna merah |
| Label teknis (`StoredSnapshot`, "Efektif Billing", "Tidak tercatat") tampil ke pengguna; alasan keluar (EndReason) tidak ditampilkan | Riwayat sulit dibaca orang awam |
| Search buatan sendiri, kartu KPI custom, ikon emoji, inline style, warna literal di CSS | Melanggar ui-consistency-checklist C dan D |

### B. Proses bisnis dari sisi pengguna (setelah perubahan)

1. Tab Bed Usage History langsung menampilkan **seluruh riwayat** bed terpilih, dengan yang terbaru di atas. Bed terpilih otomatis: bed yang dibuka dari tombol Riwayat, atau bed pertama dalam cakupan akses.
2. Empat kartu ringkasan tampil:
   - **Status Bed Saat Ini**, dari Monitoring, mis. "Menunggu Pembersihan · Kosong sejak 10 Okt 2026, 15.30".
   - **Pasien Saat Ini/Terakhir**, dari hunian efektif terbaru, mis. "Keluar 10 Okt 2026, 15.30 · pasien pulang".
   - **Jumlah Hunian**, tidak menghitung versi yang digantikan koreksi.
   - **Rata-rata Lama Hunian**.
3. Periode adalah penyaring opsional lewat tab DataFilter: Semua Riwayat (default), 7 Hari, 30 Hari, 3 Bulan, Bulan Ini, Rentang Tanggal. Tanggal dipilih dalam WIB dan tanggal akhir ikut dihitung. Contoh: rentang 1–13 Okt dikirim sebagai `FromUtc=2026-09-30T17:00Z`, `ToUtc=2026-10-13T17:00Z`. Saat "Rentang Tanggal" dipilih, rentang langsung terisi 30 hari terakhir.
4. Filter kamar tetap ada sesuai RWI-DEC-286, dan sekarang bertingkat. Memilih kamar mempersempit daftar bed. Bila bed aktif tidak berada di kamar itu, bed pertama di kamar tersebut dipilih otomatis agar hasil tidak kosong. Pilihan bed bisa dicari berdasarkan kode, nama, kamar, atau unit.
5. Timeline (default) memuat:
   - penanda **Sekarang** (mis. "Bed kosong 3 hari 7 jam · sejak 10 Okt 2026, 15.30");
   - pemisah per bulan WIB (mis. "September 2026 · 3 hunian · 1 versi dikoreksi");
   - kartu hunian berisi waktu masuk/keluar, lama hunian, cara masuk (Penempatan awal / Transfer · UpGrade / Koreksi salah catat), cara keluar (Pasien pulang / Pindah bed / Episode rawat ditutup / Admisi dibatalkan / Digantikan koreksi), dan lokasi saat itu;
   - **jeda bed kosong** di antara dua hunian efektif. Jeda 7 hari ke atas ditandai "jeda panjang".
   Versi yang digantikan koreksi tampil redup dengan badge "Digantikan koreksi". Paginasi server tersedia di timeline dan tabel.
6. Tampilan Tabel tetap tersedia dengan kolom No, Pasien, Masuk, Keluar, Lama Hunian, Cara Masuk, Cara Keluar, Lokasi Saat Itu, dan Status.
7. Klik kartu atau baris membuka modal **Rincian Hunian** berisi tiga kelompok: Ringkasan hunian, Lokasi saat itu (snapshot), dan Jejak data (versi, sumber data, dihitung untuk tarif kamar, koreksi). Catatan khusus tampil untuk versi koreksi, versi yang digantikan, data lama, dan hunian aktif. Tombol **Koreksi Salah Catat** hanya muncul bila hak koreksi ada dan versinya masih berlaku; form koreksi existing tidak diubah.
8. Pencarian lokal (nama pasien/lokasi) hanya menyaring data yang sudah dikirim server pada halaman aktif. Identitas yang disamarkan server tetap tampil "Identitas dibatasi".
9. Keadaan kosong memberi tindakan yang jelas: "Tampilkan seluruh riwayat" bila filter mempersempit, "Hapus pencarian" bila pencarian tidak cocok. Tombol reset tidak mengosongkan bed. Tombol Muat Ulang di hero ikut memuat ulang riwayat, sehingga tombol Muat Ulang ganda dihapus.

### C. Berkas yang berubah (frontend)

| Berkas | Perubahan | Status Git |
| --- | --- | --- |
| `src/lib/constants/health-services/inpatient-management/bed-management-constants.js` | Preset periode, mode tampilan, label EndReason/ContextSource/status, batas jeda panjang, tanggal awal "Semua Riwayat" | M |
| `src/utils/health-services/inpatient-management/bed-management-utils.js` | Query periode WIB inklusif, preset WIB, model tampilan hunian, timeline (bulan + jeda), ringkasan, rincian, catatan; perbaikan pemisah `·` dan format durasi berjalan | M |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-usage-history.js` | Default Semua Riwayat; bacaan hunian terbaru terpisah (pageSize 5, semua waktu); filter kamar bertingkat; reset tanpa mengosongkan bed; reloadSignal; keadaan kosong/pending | M |
| `src/components/features/health-services/inpatient-management/inpatient-bed-usage-history.jsx` | Komposisi SummaryGrid → DataFilter (tabs periode, search, bed, kamar, tanggal) → InformationAlert → Timeline/DataTable → modal rincian; form koreksi existing dipertahankan | M |
| `src/components/features/health-services/inpatient-management/inpatient-bed-usage-timeline.jsx` | Timeline memakai ClinicalTimeline/ClinicalTimelineItem, penanda Sekarang, pemisah bulan, jeda kosong, paginasi | ?? |
| `src/components/features/health-services/inpatient-management/bed-history-table-columns.jsx` | Kolom berbasis model tampilan; format tanggal sama dengan timeline | M |
| `src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx` | Muat Ulang hero mengirim `reloadSignal` ke tab riwayat (3 baris; perubahan paralel di berkas ini tidak disentuh) | M |
| `src/style/health-services/inpatient-management/bed-management.module.css` | Blok sementara sesi sebelumnya (KPI emoji, preset, search, timeline custom, warna literal) diganti blok `usage*` berbasis token | M |
| `tests/unit/inpatient-bed-usage-history.test.mjs` | 8 test logika murni baru | ?? |
| `tests/unit/inpatient-bed-management.test.mjs` | Kontrak periode diperbarui ke WIB inklusif; pemisah `?` menjadi `·` | M |

### D. Keputusan komponen (decision gate)

| Elemen | Keputusan | Bukti / alasan |
| --- | --- | --- |
| Kartu ringkasan | REUSE `SummaryGrid` | Menggantikan kartu KPI custom |
| Periode, pencarian, reset | REUSE `DataFilter` (`tabs`, `onSearchChange`, `onReset`) | Menggantikan preset chip dan search buatan sendiri |
| Pilihan bed/kamar/tanggal | REUSE `FilterSelect` (searchable), `FilterDatePicker` | Props existing |
| Timeline | COMPOSE `ClinicalTimeline` + `ClinicalTimelineItem` (doctor-clinical-base) + `StatusBadge` | Pola yang sama dipakai riwayat status mesin HD dan catatan terpadu; `muted` untuk versi yang digantikan, `onSelect` aksesibel |
| Pemisah bulan, jeda kosong, penanda Sekarang | COMPOSE: elemen `<li>` fitur di dalam slot `ClinicalTimeline` | CSS fitur berbasis token |
| Tabel, paginasi | REUSE `DataTable`, `Pagination` | Kolom di `bed-history-table-columns.jsx` |
| Rincian | REUSE react-bootstrap `Modal` + class modal detail bed existing + `BaseDetailCard` (`groupTitle`) + `InformationAlert` | Pola modal detail bed di tab Monitoring; z-index modal detail aman di atas modal papan bed |
| Pengalih Timeline/Tabel | REUSE pola `viewToggleGroup` milik halaman yang sama | Grep anti-regresi #3 menemukan `<button>` di toggle ini. Dipertahankan agar identik dengan pengalih Kartu/Tabel di tab Monitoring (`bed-management-view.jsx`) |

Tidak ada komponen NEW dan tidak ada perubahan default base component. Grep anti-regresi pada blok CSS baru: 0 warna literal, 0 `!important`, typography hanya pada selector milik fitur. Pada JSX yang berubah: 0 inline style, 0 emoji, 0 `<table>` mentah.

### E. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-management/usage-history` | Riwayat per bed/periode/kamar (paginasi server); bacaan terpisah pageSize 5 sejak 2000-01-01 untuk status sekarang/pasien terakhir | `InpatientBedManagement : ReadUsageHistory` |
| `GET` | `/v1/health-services/inpatient-management/bed-management/monitoring` | Pilihan bed/kamar dan status operasional bed | `InpatientBedManagement : Read` |

Endpoint koreksi tidak berubah dari bagian 5.

### F. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm.cmd run lint:errors` | exit 0 | PASS | ESLint pada file yang berubah: 0 error, 5 warning `react-hooks/set-state-in-effect` di hook. Pola `setState` di dalam effect yang sama sudah ada di versi HEAD → EXISTING WARNING |
| `npm.cmd run build` | exit 0; postbuild standalone siap | PASS | Next.js 16.2.12 |
| AUTOMATED TEST: unit scope bed (`inpatient-bed-management`, `inpatient-bed-usage-history`, `inpatient-bed-transfer-refresh`, `inpatient-foundation`, `inpatient-bed-board`) | 46/46 | PASS | Termasuk 8 test baru: preset WIB, query inklusif, model hunian, timeline/jeda, ringkasan, pasien terbaru, rincian, format |
| AUTOMATED TEST: `npm.cmd run test:unit` | 2853 test; 2821 PASS; 32 FAIL | UNRELATED EXISTING ISSUE | Tidak ada file test yang gagal mengimpor atau membaca file yang diubah task ini (menu, hemodialisis, episode detail/worklist, pra-operasi, dan lain-lain) |
| Diff/scope | Hanya file pada tabel C; tidak ada stage/commit/push | PASS | `git status --short` |

**MANUAL TEST: NOT FEASIBLE** — dev server tidak berjalan dan akun/role nyata tidak tersedia di sesi ini. Yang perlu diverifikasi di browser:
- default Semua Riwayat dan bed terpilih otomatis;
- tab periode dan rentang tanggal;
- filter kamar bertingkat, pencarian, dan reset;
- paginasi timeline/tabel;
- modal rincian dan alur koreksi;
- modal riwayat dari papan bed (`readOnly`);
- tampilan responsif;
- identitas disamarkan untuk akun tanpa hak episode.

### G. Acceptance criteria setelah pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pasien tanpa transfer muncul; snapshot/versi lama tetap terbaca sesudah rename/koreksi; IsEffectiveForBilling dari server. | Source dipetakan; runtime NOT_RUN | "Penempatan awal" untuk segmen tanpa transfer; lokasi dibaca dari snapshot; status digantikan dari `SupersededByCorrectionId`/`IsEffectiveForBilling` server; unit PASS |
| Viewer tanpa hak episode tidak memperoleh identitas/link pasien; HK tab forbidden tidak fetch. | Source dipetakan; runtime NOT_RUN | Nama hanya dari `PatientIdentity` server ("Identitas dibatasi" bila null); pencarian hanya atas data yang sudah dikirim server; mounting tab tidak berubah |
| Billing CLOSED correction422 tidak diakali delete; IsSuperseded tidak dipakai membuang normal transfer. | Source dipetakan; runtime NOT_RUN | Form/hook koreksi existing tidak diubah; versi digantikan tetap tampil (redup), tidak dibuang |
| Tidak membawa batas31hari report ke history atau menyalin state pasien lokal. | Source dipetakan; runtime NOT_RUN | Default "Semua Riwayat" tanpa batas rentang; seluruh data dari reader server |

### H. Catatan penutup pengerjaan ulang

| Hal | Isi |
| --- | --- |
| Deviasi dari mockup yang disetujui | (1) Filter kamar dipertahankan karena RWI-DEC-286 mewajibkannya; dibuat bertingkat. (2) Timeline memakai ClinicalTimeline (kartu dengan rail kiri) sesuai keputusan COMPOSE, bukan titik-garis custom di mockup |
| Risiko | Bacaan hunian terbaru menambah satu request kecil (pageSize 5) per pergantian bed. `HistoryAsync` backend masih N+1 per baris (cek identitas + nama pasien); pageSize dibiarkan 25 |
| Perubahan paralel | Working tree berisi perubahan pekerjaan lain (transfer, monitoring, admisi, `yarn.lock`) yang tidak disentuh task ini |
| Langkah berikutnya | Jalankan aplikasi dengan akun berhak dan tanpa hak episode, lalu verifikasi skenario MANUAL TEST di atas dan perbarui laporan ini dengan buktinya |

Skill build-module-frontend diterapkan berurutan pada ketujuh ID sesuai instruksi eksplisit pengguna “kerjakan seluruh task frontend”; satu laporan tetap dibuat per ID. Instruksi pengguna agar tidak menjalankan build/lint mengesampingkan butir validasi itu pada skill. Dependency backend yang baru selesai pada level source tidak diubah menjadi ✅ dan tidak dianggap bukti runtime.

## 1. Keadaan yang ditemukan di awal

Riwayat transfer bukan seluruh penggunaan bed; episode awal tanpa transfer dan seluruh versi correction memerlukan consumer usage-history sendiri. Payload koreksi lama belum mengirim affectedBedVersions dan key.

Source as-is backend telah tersedia dalam working tree hasil task BE-RWI-208–221, semuanya 🟡. Kontrak target tetap0.12.0. Delta kontrak/as-is diperlihatkan di bagian8; frontend tidak menulis source backend. Dua lockfile frontend sudah berubah sebelum task dan hash keduanya tetap sama setelah pengerjaan.

## 2. Proses bisnis dari sisi pengguna

1. Pengguna yang memperoleh tab History memilih bed, batas awal/akhir UTC dan kamar opsional. Tanpa bed/periode lengkap request history tidak dibuat. Rentang harus awal < akhir; batas akhir eksklusif dijelaskan.
2. Tabel berhalaman menampilkan segmen initial, transfer, current dan versi correction dari server. Segmen ongoing tidak diberi waktu selesai buatan; snapshot historis dibaca dari nama/kode, bukan master sekarang atau UUID.
3. Detail menampilkan ContextSource, kategori nullable dan IsEffectiveForBilling dari server. Identitas/link pasien tidak direkonstruksi dari state lokal; tab forbidden tidak dimount.
4. Koreksi memakai form existing, fresh placement version dan versi distinct semua bed terdampak dari proyeksi monitoring. Billing CLOSED422 ditampilkan; tidak ada delete atau pembatalan transfer committed. Konflik409 meminta data baru.

Tanpa hak akses, consumer baru menahan pemasangan/pengambilan data yang dilarang; penolakan403 server tetap ditampilkan. Data kosong dan gagal memuat dibedakan dari keberhasilan mutasi. Akun, role dan fixture rumah sakit nyata belum tersedia untuk pembuktian manual.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

AGENTS.md frontend/backend; canonical GLOBAL_RULES dan frontend-architecture; skill build-module-frontend beserta profile, token, katalog/gate komponen, composition, UI checklist, master-data-feature-standard dan test-policy; roadmap, delivery lock, keputusan46, kontrak/desain target dan laporan dependency backend. Source controller/DTO/query as-is Bed Management, Bed Occupancy, Discharge, AdmissionTransfer dan empat master dibaca untuk memastikan method, field, permission serta delta respons. Referensi visual: census/list rawat inap dan komponen region existing; master mengacu pada standar hr/master-data/job-level dengan batas adapter terhadap bentuk legacy existing.

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Status Git |
| --- | --- | --- |
| [src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-usage-history.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-usage-history.js) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | ?? |
| [src/components/features/health-services/inpatient-management/inpatient-bed-usage-history.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/inpatient-bed-usage-history.jsx) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | ?? |
| [src/components/features/health-services/inpatient-management/bed-history-table-columns.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/bed-history-table-columns.jsx) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | ?? |
| [src/lib/hooks/health-services/inpatient-management/use-inpatient-placement-correction.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-placement-correction.jsx) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | M |
| [src/lib/services/health-services/inpatient-management/bed-master-mutation.service.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/bed-master-mutation.service.js) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | ?? |
| [src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx) | Tab usage history dengan filter bed/periode UTC/kamar dan pagination server; snapshot aman dan semua versi; correction existing memakai affected-bed versions/key. | M |

Utility shared dari FE-RWI-231 digunakan ulang oleh task berikutnya; hubungan itu dicatat sebagai dependency, bukan implementasi HTTP/state paralel. File laporan ini dan tautan status roadmap/traceability juga ditulis; file backend lainnya tidak diubah oleh pekerjaan frontend.

### 3.3 Kepatuhan arsitektur frontend

Route App Router tetap entry point. View/feature merangkai base components dan memanggil hook; hook memegang state/validasi/request, service memakai InstanceAxios existing, pure normalizer/payload berada pada utils. Redux existing tetap dipakai untuk auth dan master data. Tidak ada state library, HTTP client, framework test atau primitive shared/base baru. Definisi kolom baru dipisahkan sebagai *-table-columns.jsx.

| Elemen | Keputusan | Bukti / alasan |
| --- | --- | --- |
| Hero, ringkasan, alert, toast dan access gate | REUSE | Hero, SummaryGrid, InformationAlert, ToastStack, AccessDeniedGate dari base-features; pola list/census existing |
| Filter/pilihan/input/tombol | REUSE | DataFilter, FilterSelect/ResourceFilterSelect, BaseFormControl dan BaseButton; API props existing tanpa perubahan default |
| Tabel, status, halaman dan dialog | REUSE | DataTable, StatusBadge, PaginationComponent, ConfirmModal; existing correction/master wrappers |
| Filter/detail history dan integrasi dialog correction existing | COMPOSE | Komposisi fitur domain memakai primitive di atas; pilihan1 di bawah berlaku untuk tiap komposisi, tidak menambah shared primitive |

Pilihan untuk setiap elemen COMPOSE:

1. **Rekomendasi: rangkai base components pada feature domain.** Konsistensi visual mengikuti token/katalog; risiko regresi terbatas pada consumer; biaya perubahan lokal.
2. Perluas base components dengan aturan cycle/transfer/affected-set. Konsistensi dapat terjaga, tetapi risiko pada seluruh pemakai serta biaya review shared lebih besar. Tidak dipilih.

**UI GATE: PASS pada keputusan reuse/komposisi source.** Tidak ada NEW atau perubahan default base; validasi visual browser tetap NOT FEASIBLE. CSS baru tidak memuat warna literal, override typography, !important atau tema gelap. Sembilan JSX baru tidak mempunyai button/table mentah, Bootstrap typography atau inline style statis.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat / refresh | Loading base table/select/button; snapshot belum dapat dikonfirmasi sampai pembacaan fresh selesai |
| Kosong | Pesan daftar kosong atau permintaan melengkapi bed/periode/pilihan; tidak ditampilkan sebagai mutasi berhasil |
| Gagal memuat | InformationAlert atau error existing serta tindakan Muat Ulang |
| Tanpa hak akses /403 | AccessDeniedGate/InformationAlert dan consumer forbidden tidak dipasang; server tetap otoritatif |
| Konflik409 | Snapshot lama ditinggalkan; data dibaca kembali dan pengguna diminta memeriksa/konfirmasi ulang |
| Validasi422 | Alasan/pilihan tetap tersedia untuk diperbaiki; tidak ada perubahan lokasi atau delete sebagai jalan pintas |
| Pending | Utility menolak klik ganda; tombol dan isian terkait dinonaktifkan |
| Unknown/timeout/offline | Tidak menyatakan sukses; body/key tetap; retry membaca own outcome lalu replay key sama. Outcome403 tidak membuang key. |
| Sukses | State dibaca ulang dari server; callback UI gagal tidak membalik hasil mutasi yang telah committed |

Untuk helper FE-RWI-237, state layar adalah tanggung jawab caller dan belum mempunyai caller UI; helper hanya menolak payload tidak lengkap atau melempar error operationUnknown yang bisa ditangani caller.

## 5. Endpoint yang dikonsumsi

Path di bawah memakai prefix /v1; InstanceAxios menambahkan /api. Mutasi berpartisipasi pada coordinator mengirim Idempotency-Key dari utility bersama. Reader/options existing tidak diubah menjadi writer.

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-management/usage-history` | Seluruh segmen/version dalam periode UTC per bed | `InpatientBedManagement : ReadUsageHistory` |
| `GET` | `/v1/health-services/inpatient-management/bed-management/monitoring` | Versi distinct seluruh bed yang terdampak correction | `InpatientBedManagement : Read` |
| `GET` | `/v1/health-services/inpatient-management/bed-management/operations/{key}` | Pemulihan hasil correction ambigu | `InpatientBedManagement : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/placements/by-episode/{episodeId}` | Fresh placement version sebelum form correction | `InpatientBedOccupancy : Read` |
| `POST` | `/v1/health-services/inpatient-management/bed-occupancies/placements/{placementId}/corrections` | Correction versioned dengan reason dan affectedBedVersions/key | `InpatientBedOccupancy : Correct` |

#### Health Services / Master Data / Bed

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/beds/options` | Pilihan bed/kamar melalui useSelectResource existing | `Bed : Read` |

#### Health Services / Master Data / Room

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/rooms/options` | Pilihan bed/kamar melalui useSelectResource existing | `Room : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| AUTOMATED TEST: enam file unit scope melalui node --import ./tests/helpers/register.mjs --test | 58 tests;58 PASS;0 FAIL | PASS | inpatient-bed-management, inpatient-bed-admission-helper, inpatient-bed-status, inpatient-episode-detail, inpatient-placement, inpatient-foundation |
| AUTOMATED TEST: npm.cmd run test:unit | 2720 tests;2694 PASS;26 FAIL | FAIL — EXISTING / ENVIRONMENT ISSUE | Nama26 tes gagal juga ada pada pembanding virtual source HEAD; lihat rincian [FE-RWI-237](FE-RWI-237.md) |
| Unit pure logic baru | 15/15 PASS | PASS | Tests privacy/raw status, counts, positive versions, manual categories, dua versi transfer, UTC >31hari, snapshot tanpa UUID, key/body/replay409/timeout403/404/double click |
| Unit helper admisi transfer baru | 3/3 PASS | PASS | HTTP-double: missing version/intent tanpa POST,201+OperationMeta, timeout→receipt→same-key replay dengan episode sama |
| Parse Babel dan resolusi import/export lokal | 63 JS/JSX/MJS;0 syntax error,0 missing import/export,0 unbound identifier | PASS | Parse dan traverse seluruh source/test yang berubah; pemeriksaan ini bukan build/lint |
| Anti-regresi UI | 9 JSX baru/0 temuan; CSS baru/0 warna literal,0 override typography,0 !important | PASS | Pemeriksaan pola button/table/Bootstrap/inline style dan stylesheet |
| Hash input lock + lockfile pengguna | 24/24 masukan cocok;2/2 lockfile pengguna tetap | PASS | SHA256 raw bytes; package-lock/yarn tidak ditulis |
| Diff/scope | git diff --check bersih; HEAD/branch tetap; tidak ada stage/commit/push | PASS | Pemeriksaan read-only Git |
| Browser localhost | GET http://localhost:3000/health-services/inpatient-management/bed-board gagal ECONNREFUSED | EXISTING / ENVIRONMENT ISSUE | Server frontend tidak berjalan; akun/fixture terotorisasi juga belum tersedia |
| Lint:errors / lint | NOT_RUN | NOT RUN | Dikecualikan eksplisit pengguna; pengguna menjalankan manual |
| npm run build | NOT_RUN | NOT RUN | Dikecualikan eksplisit pengguna; pengguna menjalankan manual |
| BM-AT scoped, API/network role nyata, race database, UAT | NOT_RUN | NOT RUN | Unit source/HTTP double tidak menggantikan skenario end-to-end |

**MANUAL TEST: NOT FEASIBLE** — localhost:3000 menolak koneksi. Belum dapat membuktikan daftar/label select, pencarian, kombinasi/reset filter, pagination, dialog, keyboard/responsif,403/409/422 dan privacy role nyata di browser. Tidak menjalankan test:uat karena membawa build. Prettier API hanya dipakai merapikan source terkait, bukan lint.

Perintah scope: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-bed-management.test.mjs tests/unit/inpatient-bed-admission-helper.test.mjs tests/unit/inpatient-bed-status.test.mjs tests/unit/inpatient-episode-detail.test.mjs tests/unit/inpatient-placement.test.mjs tests/unit/inpatient-foundation.test.mjs`.

## 7. Acceptance criteria dan Definition of Done

Setiap teks kriteria berikut disalin persis dari kartu roadmap. Pemetaan source bukan pernyataan skenario API/browser telah lulus; acceptance penuh tetap0/4 sampai bukti runtime tersedia.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pasien tanpa transfer muncul; snapshot/versi lama tetap terbaca sesudah rename/koreksi; IsEffectiveForBilling dari server. | Source dipetakan; runtime NOT_RUN | Consumer usage-history tidak menyaring !IsSuperseded; snapshot whitelist mempertahankan nama/kode lama, flag billing dibaca langsung. Unit snapshot tanpa UUID dan periode >31 hari PASS; fixture episode awal/rename/koreksi runtime NOT_RUN. |
| Viewer tanpa hak episode tidak memperoleh identitas/link pasien; HK tab forbidden tidak fetch. | Source dipetakan; runtime NOT_RUN | Tab forbidden tidak memasang history; nama pasien hanya dari respons server bila disediakan, tidak dari cache. Pengujian role HK/viewer dan payload PHI nyata NOT_RUN. |
| Billing CLOSED correction422 tidak diakali delete; IsSuperseded tidak dipakai membuang normal transfer. | Source dipetakan; runtime NOT_RUN | Correction tetap POST placements/{id}/corrections dan error422 tidak memicu delete. SupersededByCorrectionId, bukan IsSuperseded umum, menentukan label penggantian koreksi. |
| Tidak membawa batas31hari report ke history atau menyalin state pasien lokal. | Source dipetakan; runtime NOT_RUN | buildBedHistoryQuery UTC dengan akhir eksklusif tidak membatasi31hari; unit rentang panjang PASS; seluruh state berasal dari reader server. |

D-FE: source, laporan, keputusan UI, status/tautan traceability dan validasi statis/unit tersedia. **Belum terpenuhi:** runtime/API/browser, dependency backend selesai penuh, pembuktian privacy/role/gate aktual serta kriteria yang memerlukan transaksi nyata. **Dikecualikan pengguna:** build dan lint. Suite penuh masih FAIL baseline; status task 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | npm.ps1 diblokir ExecutionPolicy; tes memakai npm.cmd. Git memberi pemberitahuan konversi LF/CRLF; diff-check tidak menunjukkan whitespace error. |
| Masalah yang diketahui | Privacy role nyata BM-G03, Billing OPEN/CLOSED, initial segment/rename/history seluruh versi dan koreksi concurrent belum diuji API/browser. ReadAffectedBedVersions membutuhkan akses Monitoring operasional pada scope bed terdampak; permission/grant nyata perlu diverifikasi. |
| Batas pemulihan | Stable intent disimpan dalam memori per pemilik/scope selama sesi aplikasi, bukan penyimpanan permanen lintas full reload browser. Pemulihan setelah reload penuh/keluar sesi belum dibuktikan; jangan menganggap unknown selesai atau membuat key baru untuk maksud operasi yang sama tanpa rekonsiliasi. |
| Dependency backend | `FE-RWI-231`, `BE-RWI-220`; BE source 🟡 dan migration/runtime NOT_RUN. BM-G01 kelas resmi, BM-G02 SOP, BM-G03 scope/privacy akun aktual dan BM-G04 cutover tetap mengikuti cakupan masing-masing, belum ditutup. |
| Perubahan sampingan | NONE oleh task ini. package-lock.json dan yarn.lock dirty sejak awal, SHA256 tetap. Working tree backend user/source/migration yang sudah ada tidak disentuh. |
| Interupsi | Sesi dilanjutkan melalui instruksi continue; status/diff diperiksa sebelum melanjutkan. Tidak ada rollback atau penggandaan perubahan. |
| Status Git | Source/test tabel3.2: M atau ?? sesuai Git; laporan ini baru ??, frontend-roadmap-bed-management dan requirement-traceability-bed-management ?? sejak planning; requirement-traceability.md M. Seluruh perubahan belum di-stage/commit. |
| Langkah berikutnya | Pengguna menjalankan build/lint manual; pemilik backend menyelaraskan metadata transfer/evidence historis bila diwajibkan kontrak; jalankan aplikasi/fixture/role dan BM-AT scoped, lalu perbarui laporan ini dengan bukti. |
