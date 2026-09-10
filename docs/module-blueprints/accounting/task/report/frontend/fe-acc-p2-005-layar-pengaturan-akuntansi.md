# Laporan Perubahan Frontend — `FE-ACC-P2-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-005` |
| Judul | Layar Pengaturan Akuntansi |
| Slice | `P2-0a` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-005` |
| Trace | `ACC-DEC-054`; `FR-P2-032` |
| Contract version | `ACC-API-0.9` — `approved`, Rizki 10 September 2026 |
| Wewenang UI | Satu butir menu tingkat 3 di bawah Master Data, satu layar kecil, satu slice, satu hook, satu constants, satu CSS Module |
| Dependency | `BE-ACC-P2-009` — **`DONE`**; route dan hak akses diverifikasi langsung ke `AccountingConfigurationController` |
| Klasifikasi | `LIGHT` — satu isian dan satu tombol, tanpa daftar, tanpa komponen base baru |
| Task mode | `CROSS-REPO` — source di frontend, laporan di backend |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, dan laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a383e7622` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `3e2fb76` (branch `rizkiG`) |
| Tanggal | 10 September 2026 |
| Status | Selesai. UAT peramban diserahkan kepada Rizki atas permintaannya |

---

## 1. Keadaan yang ditemukan di awal

Layar ini belum ada sama sekali. Akibatnya akun laba ditahan tidak dapat ditetapkan lewat
aplikasi, dan **seluruh tutup tahun terkunci**: `GET /year-end-closing/preview` menjawab `422`
dengan kalimat "Akun laba ditahan belum ditetapkan pada pengaturan akuntansi."

Itulah alasan task ini dikerjakan lebih dahulu daripada `FE-ACC-P2-006`, walaupun nomornya lebih
kecil dan gelombangnya (`P2-0a`) memang paling awal: tanpa layar ini, layar Tutup Tahun tidak
dapat diuji sama sekali.

Backend-nya sudah lengkap dan diperiksa langsung, bukan dibaca dari dokumen:

| Yang diperiksa | Hasil |
| --- | --- |
| Route | `api/v1/corporate/accounting/configuration` — cocok dengan kontrak |
| Hak akses | `AccountingConfiguration : Read` dan `: Update` — cocok |
| Keadaan belum diisi | `GET` menjawab **`200` ber-`isConfigured: false`**, bukan `404` (`ACC-DEC-069` butir 3) |
| Penolakan | **Empat** sebab berbeda, seluruhnya `422` beserta kalimatnya masing-masing |

Empat sebab penolakan itu penting bagi layar: akun tidak ditemukan, akun milik badan hukum lain,
akun bukan berjenis Ekuitas, akun induk yang tidak menerima transaksi, dan akun nonaktif. Masing-
masing punya kalimat sendiri, dan layar menampilkannya apa adanya.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya Manajer Akuntansi** atau Administrator. Layar dibuka lewat **Akuntansi › Master
Data › Pengaturan Akuntansi**.

1. Petugas memilih **badan hukum**. Pengaturan disimpan terpisah untuk setiap badan hukum
   (`ACC-DEC-037`), jadi tanpa memilih badan hukum tidak ada yang bisa ditampilkan.
2. Bila akunnya belum pernah ditetapkan, muncul spanduk kuning yang menyebut **akibatnya**:
   *"Selama akun ini kosong, jurnal penutup tahun tidak dapat disusun untuk badan hukum ini."*
3. Petugas memilih akun dari daftar. Daftarnya **hanya berisi akun Ekuitas yang menerima
   transaksi** — akun jenis lain tidak muncul sama sekali.
4. Tombol **Simpan Pengaturan** menyala hanya bila pilihannya berubah. Sesudah tersimpan, muncul
   pemberitahuan hijau dan kartu ringkasan berisi akun tersimpan, jenisnya, dan kapan terakhir
   diubah.

### Kenapa kalimat keadaan kosongnya berbunyi begitu

Acceptance (2) menuntut keadaan kosong menjelaskan bahwa **tutup tahun belum dapat dijalankan** —
bukan sekadar "belum ada data".

Bedanya nyata. "Belum diatur" tidak memberi tahu siapa pun apa yang akan rusak, dan pengaturan
yang tampak sepele mudah ditunda berbulan-bulan. "Tutup tahun tidak dapat disusun" memberi tahu,
dan kalimatnya sengaja dibuat mirip dengan yang kelak ditolakkan backend di layar Tutup Tahun —
supaya petugas yang bertemu penolakan itu mengenali kalimat yang sama dan tahu ke mana harus
pergi.

### Jalur tidak normal

- **Belum memilih badan hukum.** Spanduk biru menjelaskan bahwa pengaturan disimpan per badan
  hukum. Isian dimatikan.
- **Tidak ada akun Ekuitas sama sekali.** Spanduk kuning: *"Tidak ada akun Ekuitas yang menerima
  transaksi."* disertai petunjuk membuat akunnya lebih dahulu di layar COA, dan mengingatkan
  bahwa akun induk tidak akan muncul.
- **Akun ditolak backend.** Pesan aslinya ditampilkan apa adanya — ia sudah menyebut sebab yang
  tepat dari empat kemungkinan, dan menggantinya dengan kalimat umum justru menghapus keterangan
  yang paling berguna.
- **Tanpa hak akses.** Tombol Simpan **dimatikan, bukan disembunyikan**, disertai keterangan
  "Anda tidak memiliki hak mengubah pengaturan akuntansi."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/.../Configuration/Controllers/AccountingConfigurationController.cs` | Route dan hak akses sebenarnya |
| `NewQuilvianSystemBackend/Areas/.../Configuration/DTOs/AccountingConfigurationDtos.cs` | Bentuk respons dan permintaan |
| `NewQuilvianSystemBackend/Areas/.../Configuration/Services/*.cs` | Empat sebab penolakan `422` beserta kalimatnya |
| `NewQuilvianSystemBackend/Areas/.../ChartOfAccount/Services/AccChartOfAccountService.cs` | Penyaring `GetOptionsAsync`: `IsActive && IsPostable`, **tanpa** penyaring jenis akun |
| `NewQuilvianSystemBackend/.../Enums/AccountType.cs` | `Equity = 3` |
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | Pola pemetaan pilihan akun |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | Pemakaian `BaseSelectField` bercari |
| `src/utils/menu-sidebar/menu-items.jsx` | Letak butir menu dan ikon yang sudah diimpor |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/corporate/accounting/accounting-configuration-slice.jsx` | **Baru.** Dua thunk, normalisasi dua ejaan, `isConfigured` dibaca dari backend |
| `src/lib/constants/corporate/accounting/accounting-configuration-constants.jsx` | **Baru.** Nilai `AccountType`, penyaring kandidat Ekuitas, salinan teks layar |
| `src/lib/hooks/corporate/accounting/configuration/use-accounting-configuration.jsx` | **Baru.** Pengambilan, penyaringan pilihan, penyimpanan, toast |
| `src/components/view/corporate/accounting/configuration/accounting-configuration-view.jsx` | **Baru.** Layarnya |
| `src/style/corporate/accounting/accounting-configuration-view.module.css` | **Baru.** Hanya tata letak |
| `src/app/corporate/accounting/configuration/page.jsx` beserta client-nya | **Baru.** Entry point dan metadata saja |
| `tests/unit/accounting-configuration-and-year-end.test.mjs` | **Baru.** Dibagi bersama `FE-ACC-P2-006` |
| `src/lib/state/store.jsx` | Mendaftarkan slice baru |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir menu tingkat 3 di bawah Master Data |

### 3.3 Kepatuhan arsitektur frontend

`UI GATE: 8 elemen — REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx` | `REUSE` |
| Gerbang tanpa hak akses | `AccessDeniedGate` | `base-features/access-denied-gate.jsx` | `REUSE` |
| Pemilih badan hukum | `AccountingLegalEntitySelect` | `view/corporate/accounting/shared/` | `REUSE` |
| Pemilih akun bercari | `BaseSelectField` | `base-form-control.jsx:396`; dipakai Form Jurnal untuk keperluan yang sama | `REUSE` |
| Lencana sudah/belum ditetapkan | `StatusBadge` | `base-features/status-badge.jsx` | `REUSE` |
| Spanduk keadaan kosong dan galat | `InformationAlert` | `base-features/information-alert.jsx` | `REUSE` |
| Tombol Simpan | `BaseButton` | `base-features/base-button.jsx` | `REUSE` |
| Notifikasi hasil | `ToastStack` | `base-features/toast-stack.jsx` | `REUSE` |

Tidak ada elemen berstatus `EXTEND`, `WRAP`, maupun `NEW`, sehingga gerbang ini tidak menuntut
keputusan user.

**Satu keputusan pelaksanaan yang perlu dicatat: penyaringan Ekuitas dilakukan di layar.**

`GET /chart-of-accounts/options` tidak menyediakan penyaring jenis akun — ia hanya menyaring
`IsActive` dan `IsPostable`. Tetapi responsnya **membawa `accountType` pada setiap pilihan**, jadi
layar menyaring memakai data yang backend kirim sendiri.

Ini bukan memindahkan aturan bisnis ke tempat yang salah, dan bedanya penting: backend **tetap**
menolak `422` bila akun yang dikirim bukan Ekuitas. Penyaringan di layar semata-mata supaya
petugas tahu **sebelum** memilih, bukan sesudah ditolak — prinsip yang sama dipakai
`FE-ACC-P2-007` acceptance (3) untuk akun control pada Form Jurnal.

Sisi `IsPostable` dan `IsActive` **tidak** disaring ulang di layar, dan ada uji yang memagarinya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pemilih akun menampilkan keadaan memuat bawaan `BaseSelectField`; tombol Simpan mati |
| Kosong — belum pilih badan hukum | Spanduk biru "Pilih badan hukum lebih dahulu." |
| Kosong — belum ditetapkan | Spanduk kuning beserta akibatnya bagi tutup tahun |
| Kosong — tidak ada akun Ekuitas | Spanduk kuning beserta petunjuk membuat akunnya di layar COA |
| Gagal memuat | Kotak galat halaman; `403` ditangani `AccessDeniedGate` |
| Gagal menyimpan | Toast merah beserta pesan asli backend, ditambah spanduk merah di kartu |
| Tanpa hak akses | Tombol Simpan mati beserta keterangan; isian tetap dapat dilihat |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - Accounting - Master Data - Configuration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/configuration/{legalEntityId}` | Menampilkan akun laba ditahan yang tersimpan beserta `isConfigured` | `AccountingConfiguration : Read` |
| `PUT` | `/v1/corporate/accounting/configuration/{legalEntityId}` | Menetapkan akun laba ditahan | `AccountingConfiguration : Update` |

#### Corporate - Accounting - Master Data - Chart of Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts/options` | Mengisi pilihan akun; disaring ke Ekuitas di layar | `ChartOfAccount : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada berkas yang disentuh | Tanpa keluaran, exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | 0 error, 673 warning | `PASS` | Jumlah warning **tidak bertambah** dari sebelum task ini — berkas baru nol warning |
| `npm run build` | `✓ Compiled successfully in 34.5s` | `PASS` | Route `○ /corporate/accounting/configuration` terdaftar |
| `node --test tests/unit/` | 627 lulus, 0 gagal | `PASS` | Naik dari 615; 12 uji baru dibagi dengan `FE-ACC-P2-006` |
| Grep anti-regresi 1, 3, 4, 5, 6, 7 | Kosong seluruhnya | `PASS` | Tanpa warna literal, tombol non-base, tabel mentah, utility typography, `!important`, maupun inline style |
| Grep anti-regresi 2 — typography | Empat temuan, dipertahankan | `PASS` | Seluruhnya menyasar `<h2>`, `<p>`, dan `<span>` milik layar ini sendiri; tidak satu pun menyasar komponen shared |
| UAT peramban | Tidak dijalankan | `NOT RUN` | **Diserahkan kepada Rizki atas permintaannya**, 10 September 2026 |

Uji manual: `NOT RUN` — pemilik meminta pengujian layar dilakukan sendiri.

**Tidak dijalankan:** verifikasi terhadap API sungguhan. Backend lokal sudah dimatikan saat task
ini dikerjakan, sehingga bentuk respons diambil dari **DTO pada source**, yang tetap otoritatif.
Berbeda dari `FE-ACC-P2-001` yang sempat diuji terhadap backend berjalan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Pemilih akun hanya menampilkan akun Ekuitas yang menerima transaksi | Terpenuhi | `isRetainedEarningsCandidate` menyaring `AccountType == 3`; sisi `IsPostable`/`IsActive` sudah disaring backend. Uji "hanya akun Ekuitas yang lolos sebagai kandidat laba ditahan" dan "layar tidak menyaring ulang isPostable maupun isActive" |
| (2) Keadaan kosong menjelaskan bahwa tutup tahun belum dapat dijalankan | Terpenuhi | `ACCOUNTING_CONFIGURATION_CONFIG.emptyMessage`, dirender saat `isConfigured` bernilai `false` |
| (3) Hanya yang berhak dapat menyimpan | Terpenuhi | `usePermission("AccountingConfiguration", "Update")` mematikan tombol Simpan beserta keterangannya; backend tetap menegakkan `403` |

**Definition of Done:** lint hijau, build hijau, dan laporan tracked ini ada — seluruhnya
terpenuhi. Kartu ini tidak menuntut UAT peramban.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan `react-hooks/set-state-in-effect` sempat muncul saat isian disinkronkan lewat `useEffect`. Diselesaikan bukan dengan mematikan aturannya, melainkan dengan menyesuaikan state **saat render** memakai pembanding kunci — pola yang dianjurkan React. Efek sampingnya menguntungkan: petugas tidak lagi melihat pilihan kosong sekejap sebelum nilai tersimpannya muncul |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` — `BE-ACC-P2-009` sudah `DONE` |
| Perubahan sampingan | `NONE`. Butir menu dan pendaftaran slice adalah bagian sah dari task ini |
| Interupsi | `NONE` |
| Status Git | Tujuh berkas baru ditambah dua berkas diubah (`store.jsx`, `menu-items.jsx`). Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Jalankan layar ini lebih dahulu untuk menetapkan akun laba ditahan, baru uji `FE-ACC-P2-006` — tanpa itu Tutup Tahun akan menolak `422` |
