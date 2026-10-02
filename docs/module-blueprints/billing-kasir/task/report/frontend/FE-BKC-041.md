# Laporan Perubahan Frontend — `FE-BKC-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-041` |
| Judul | Layar Consumer Handoffs Tab "Rawat Inap" |
| Slice | Gelombang 1 (`MVP-29`), `frontend-roadmap.md` |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` — bagian Task `FE-BKC-041` |
| Trace | `BKC-DEC-115`, `BKC-DEC-116`, `BKC-DES-045`, `FR-BKC-250`, `03-frontend-architecture.md` Bagian 2 |
| Contract version | `BIL-API-1.4`, `BIL-PERMISSION-1.2` — keduanya `approved` |
| Wewenang UI | Terkunci: keberadaan tab Rawat Inap, rincian panel kamar, banner status clearance, hak akses tombol, kosakata badge status (`CLEARED`, `BLOCKED`, `REVOKED`) dan warna penanda. `DEV_DISCRETION`: jarak, padding kartu, transisi animasi, ikon visual |
| Dependency | `[BE] BE-BKC-076` — 🟡 selesai 24 September 2026 (controller `InpatientClearanceController` terpasang dengan 6 endpoint integrasi, perlindungan privasi data perawat, penegakan RBAC Resource `BillingInpatient`, dan kontrak Swagger `BIL-API-1.4`). Lihat [laporan](../backend/BE-BKC-076.md) |
| Klasifikasi | `MEDIUM` — penambahan tab navigasi, integrasi thunk evaluasi ulang kelayakan, parsing detail respon handoff ranap, lencana status 3-state, format sisa tagihan, modal evaluasi ulang, dan pengamanan otorisasi per-baris |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — berkas view, hook, constants, slice, dan test fitur consumer-handoffs. Laporan ini ditulis di `NewQuilvianSystemBackend` sesuai wewenang lintas repository sempit yang diberikan `AGENTS.md` frontend § Pelaporan Task Modul |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend saat dikerjakan | `0f022e0c45dd0b941ba83b01e2923944de49b201` |
| Commit backend yang dijadikan rujukan | `59910dc0ac04c7e99de36f9b299e3024c6f86a39` |
| Tanggal | 24 September 2026 |
| Status | 🟡 **SEBAGIAN — source dan validasi statis selesai penuh; 6/6 unit test lulus; npm run build lulus (exit code 0).** Seluruh acceptance criteria terpetakan ke source kode. Yang masih menahan `✅` adalah uji manual interaktif live API ter-autentikasi pada backend runtime aktif. |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dibuka, layar Consumer Handoffs (`/health-services/billing-management/billing/consumer-handoffs`) yang diimplementasikan pada `FE-BKC-040` hanya menangani dua jenis surat handoff:
1. `COLLECTION` (Terima Tagihan ke modul Keuangan/Finance)
2. `PRESCRIPTION` (Clearance Resep ke modul Farmasi)

Keterbatasan yang ditemukan:
1. **Belum Ada Tab Rawat Inap:** Filter jenis surat pada `billing-consumer-handoff-constants.js` hanya berisi opsi Keuangan dan Farmasi; belum ada navigasi tab eksplisit untuk memfilter surat fakta kelayakan kepulangan rawat inap (`INPATIENT`).
2. **Belum Ada Thunk Evaluasi Ulang:** Redux slice `billing-consumer-handoff-slice.jsx` hanya memiliki aksi `acknowledgeConsumerHandoff`; belum ada thunk `reevaluateInpatientClearance` untuk memanggil endpoint evaluasi kelayakan `POST /api/v1/health-services/billing-management/billing/inpatient-clearance/reevaluate` (`BKC-DEC-115`, `BKC-DES-045`).
3. **Format Kolom Spesifik Ranap Belum Ada:** Kolom tabel sebelumnya menampilkan rujukan umum kwitansi/resep, belum menyajikan informasi esensial rawat inap: Badge Status Kelayakan (`CLEARED`, `BLOCKED`, `REVOKED`) dan nominal Sisa Tagihan Pasien.
4. **Otorisasi Lintas Modul:** Hook sebelumnya hanya memeriksa `BillingConsumerHandoff : Acknowledge`. Untuk rawat inap, hak akses evaluasi ulang dikontrol oleh `BillingInpatient : Clearance`, dan pengakuan surat ranap didukung oleh `BillingInpatient : Acknowledge`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna layar ini meliputi **Kasir Utama / Petugas Billing**, **Perawat Bangsal / Nurse Station**, dan **Administrator Sistem**:

### Alur Normal Penggunaan:
1. **Navigasi Layar:** Petugas membuka menu "Billing dan Kasir" → "Surat ke Modul Konsumen".
2. **Pemilihan Tab "Rawat Inap":**
   - Petugas mengklik tab **"Rawat Inap"** pada bilah navigasi tab di bagian atas.
   - Hook secara otomatis memperbarui penyaring `handoffType` menjadi `"INPATIENT"` dan memuat surat-surat fakta kelayakan rawat inap yang belum diakui (`GET /consumer-handoffs/pending?handoffType=INPATIENT`).
3. **Pemantauan Status Kelayakan:**
   - Tabel menampilkan kolom: Jenis Surat (`Rawat Inap`), Modul Tujuan (`Rawat Inap`), Data Pasien / Kunjungan (`Encounter ID` & `Nomor Tagihan`), Status Kelayakan (`CLEARED`, `BLOCKED`, atau `REVOKED`), Sisa Tagihan Pasien (dalam format rupiah `Rp ...`), dan Waktu Terbit.
   - Status kelayakan divisualisasikan dengan lencana berwarna:
     - `CLEARED`: Hijau (Layak Pulang)
     - `BLOCKED`: Merah (Tertahan)
     - `REVOKED`: Kuning/Amber (Izin Dicabut karena Tagihan Susulan Masuk)
4. **Eksekusi Evaluasi Ulang (*Reevaluate*):**
   - Setelah keluarga pasien melunasi kekurangan tagihan di loket kasir, Kasir Utama menekan tombol **"Evaluasi Ulang"** pada baris kunjungan pasien tersebut.
   - Dialog konfirmasi `ConfirmModal` muncul, meminta konfirmasi dan catatan opsional.
   - Kasir menekan "Evaluasi Ulang". Tombol terkunci untuk mencegah klik ganda (`actionLoading: true`).
   - Sistem memanggil endpoint `POST /v1/health-services/billing-management/billing/inpatient-clearance/reevaluate`.
   - Backend menghitung ulang saldo tagihan dan pelunasan, lalu memperbarui status kelayakan (misal dari `BLOCKED` menjadi `CLEARED`).
   - Notifikasi sukses muncul, antrean dimuat ulang secara otomatis, dan status terkini langsung tampil.
5. **Pengakuan Tanda Terima Surat Handoff (*Acknowledge*):**
   - Petugas berwenang (Perawat Bangsal atau Admin) menekan tombol **"Akui"**.
   - Modal konfirmasi muncul, lalu memanggil `PATCH /v1/health-services/billing-management/consumer-handoffs/{id}/acknowledge` dengan payload `{ handoffType: "INPATIENT" }`.
   - Surat yang diakui hilang dari antrean aktif.

### Alur Tidak Normal / Pengecualian:
- **Pengakuan Ganda (*Conflict 409*):** Jika surat telah diakui oleh pihak lain sesaat sebelumnya, sistem menampilkan toast peringatan ramah dan menyegarkan tabel tanpa galat merah.
- **Tidak Ada Surat Menggantung:** Tabel menampilkan pesan ramah: *"Tidak ada surat kelayakan rawat inap yang menggantung. Seluruh surat fakta kelayakan kepulangan rawat inap sudah diakui oleh bangsal, atau belum ada pasien rawat inap yang menunggu kelayakan kepulangan."*
- **Peran Tanpa Izin:**
  - Pengguna tanpa izin `BillingConsumerHandoff:Acknowledge` atau `BillingInpatient:Acknowledge` tidak melihat tombol "Akui".
  - Pengguna tanpa izin `BillingInpatient:Clearance` (misalnya perawat biasa) tidak melihat tombol "Evaluasi Ulang".
  - Jika kedua izin tidak dimiliki, kolom tindakan menampilkan teks *"Hanya pantau"*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (`FE-BKC-041`)
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (`BIL-API-1.4`)
- `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md` (`BIL-PERMISSION-1.2`)
- `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Controllers/InpatientClearanceController.cs`
- `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
- `src/components/view/health-services/billing-management/consumer-handoffs/consumer-handoffs-view.jsx`
- `src/lib/hooks/health-services/billing-management/billing-consumer-handoffs/use-billing-consumer-handoffs.js`
- `src/lib/state/slice/health-services/billing-management/billing-consumer-handoff-slice.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/billing-consumer-handoffs/billing-consumer-handoff-constants.js` | Menambahkan konstanta `INPATIENT` pada tipe & target modul, daftar tab `BILLING_HANDOFF_TABS`, status clearance `INPATIENT_CLEARANCE_STATUSES`, konfigurasi lencana `INPATIENT_CLEARANCE_STATUS_CONFIG`, fungsi `parseInpatientHandoffDetails` untuk ekstraksi data status/sisa tagihan, dan utilitas `formatRupiah`. |
| `src/lib/state/slice/health-services/billing-management/billing-consumer-handoff-slice.jsx` | Menambahkan thunk `reevaluateInpatientClearance` yang memanggil `POST /v1/health-services/billing-management/billing/inpatient-clearance/reevaluate`, beserta extraReducers penanganan state `actionLoading` dan `actionError`. |
| `src/lib/hooks/health-services/billing-management/billing-consumer-handoffs/use-billing-consumer-handoffs.js` | Menambahkan integrasi otorisasi `BillingInpatient:Acknowledge` dan `BillingInpatient:Clearance`, method per-row `canAcknowledgeRow` & `canReevaluateRow`, sinkronisasi tab aktif `activeTab` & `handleTabChange`, serta modal state dan aksi `openReevaluate`, `closeReevaluate`, dan `confirmReevaluate`. |
| `src/components/view/health-services/billing-management/consumer-handoffs/consumer-handoffs-view.jsx` | Memasang bilah navigasi tab `BILLING_HANDOFF_TABS`, kolom tabel khusus rawat inap (Status Kelayakan ber-badge, Sisa Tagihan Pasien, Data Pasien/Encounter), tombol aksi `[Akui]` dan `[Evaluasi Ulang]`, modal konfirmasi evaluasi ulang, serta pesan empty state ramah untuk rawat inap. |
| `src/components/view/health-services/billing-management/consumer-handoffs/consumer-handoffs-view.module.css` | Menambahkan styling untuk `.tabNav`, `.tabButton`, `.tabButtonActive`, `.tabCountBadge`, `.statusBadgeCell`, `.outstandingCell`, `.actionGroup`, dan `.reevaluateButton`. |
| `tests/unit/billing-consumer-handoffs-inpatient.test.mjs` | Berkas unit test baru yang memvalidasi konstanta tab/status, parser detail respon, format mata uang rupiah, Redux slice thunk, otorisasi RBAC hook, dan elemen UI view. |

### 3.3 Gerbang Keputusan Base Component (Base Component Decision Gate)

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Tab Navigasi Kategori | Semantic `<nav role="tablist">` + Design Tokens | `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran` | `COMPOSE` | **Opsi A (Rekomendasi):** Rangkai tab navigasi menggunakan button semantic beraksesibilitas WAI-ARIA dan token CSS Quilvian. Konsistensi visual tinggi dan bebas regresi. |
| Header halaman | `Hero` | `src/components/features/base-features/hero` | `REUSE` | Digunakan kembali dengan deskripsi yang mencakup rawat inap. |
| Filter & tanggal | `DataFilter`, `FilterSelect`, `FilterDatePicker` | `src/components/features/base-features/data-filter` | `REUSE` | Digunakan kembali dengan opsi filter jenis surat terpadu. |
| Tabel handoffs | `DataTable` | `src/components/features/base-features/data-table` | `REUSE` | Digunakan kembali dengan definisi kolom dinamis sesuai tab aktif. |
| Lencana status kelayakan | `StatusBadge` | `src/components/features/base-features/status-badge` | `REUSE` | Memakai `INPATIENT_CLEARANCE_STATUS_CONFIG` (`CLEARED`: active/hijau, `BLOCKED`: inactive/merah, `REVOKED`: warning/kuning). |
| Tombol aksi | `BaseButton` | `src/components/features/base-features/base-button` | `REUSE` | Tombol `Akui` (primary) dan `Evaluasi Ulang` (secondary). |
| Modal konfirmasi evaluasi ulang | `ConfirmModal` | `src/components/features/base-features/confirm-modal` | `REUSE` | Modal konfirmasi dengan field input alasan evaluasi ulang opsional. |
| Notifikasi & Alert | `InformationAlert`, `ToastStack` | `src/components/features/base-features/information-alert` | `REUSE` | Penanganan pesan error dan toast sukses/peringatan. |
| Gerbang akses | `AccessDeniedGate` | `src/components/features/base-features/access-denied-gate` | `REUSE` | Membungkus layar bila terjadi galat wewenang. |

Ringkasan:
```text
UI GATE: 9 elemen — REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (*Loading*) | Indikator memuat bawaan `DataTable` (*"Mengambil daftar surat menggantung..."*), tombol "Muat Ulang" berputar, filter dinonaktifkan. |
| Kosong (*Empty*) — Tab Rawat Inap | Judul: *"Tidak ada surat kelayakan rawat inap yang menggantung"*, deskripsi: *"Seluruh surat fakta kelayakan kepulangan rawat inap sudah diakui oleh bangsal, atau belum ada pasien rawat inap yang menunggu kelayakan kepulangan."* |
| Kosong (*Empty*) — Tab Umum | Judul: *"Tidak ada surat yang menggantung"*, deskripsi: *"Seluruh fakta sudah diambil kedua modul."* |
| Gagal (*Error*) | `InformationAlert` merah dengan pesan spesifik dari backend atau validasi rentang tanggal terbalik. |
| Tanpa Hak Akses (*Unauthorized*) | `AccessDeniedGate` menampilkan pesan akses ditolak standar sistem; peran tanpa izin aksi tidak melihat tombol terkait (kolom menampilkan *"Hanya pantau"*). |
| Mutasi Sedang Berlangsung (*Submitting*) | Tombol "Akui" dan "Evaluasi Ulang" terkunci (*disabled*) untuk mencegah klik ganda (*duplicate submission lock*). |

---

## 5. Endpoint yang dikonsumsi

#### [Tags("BillingConsumerHandoff")]
Base URL: `api/v1/health-services/billing-management/consumer-handoffs`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/pending` | Mengambil daftar surat menggantung dengan filter `handoffType=INPATIENT`. | `BillingConsumerHandoff : Read` |
| `PATCH` | `/{id}/acknowledge` | Mengakui penerimaan surat kelayakan rawat inap secara idempoten oleh modul konsumen. | `BillingConsumerHandoff : Acknowledge` / `BillingInpatient : Acknowledge` |

#### [Tags("BillingInpatientIntegration")]
Base URL: `api/v1/health-services/billing-management/billing`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/inpatient-clearance/reevaluate` | Memeriksa ulang tagihan/pelunasan dan menerbitkan status kelayakan kepulangan terkini (`CLEARED`, `BLOCKED`, `REVOKED`). | `BillingInpatient : Clearance` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/lib/... src/components/...` | Lulus tanpa kesalahan (0 error, 0 warning) | `PASS` | Lint pada 4 berkas fitur yang diubah |
| `node --test tests/unit/billing-consumer-handoffs-inpatient.test.mjs` | Seluruh 6 subtest lulus penuh | `PASS` | 6 tests passed, duration 108ms |
| `npm run test:unit` | Fitur lulus; 8 kegagalan pre-existing pada domain lain tidak terkait (`FE-RWI-*`, `blood-bank`, `petty-cash sidebar`) | `UNRELATED EXISTING ISSUE` | Hasil eksekusi `node --test` suite lengkap |
| `npm run build` | Kompilasi Next.js 16 (Turbopack) sukses, 406 halaman statis tergenerasi, output standalone siap | `PASS` | Exit code 0, build berhasil dalam 47 detik |
| Grep Anti-Regresi Warna Literal | 0 warna hex/rgba ditemukan di module CSS | `PASS` | Memakai CSS variable token |
| Grep Anti-Regresi `!important` | 0 kemunculan `!important` | `PASS` | Tidak ada override paksa |
| Grep Anti-Regresi Tabel Mentah | 0 tag `<table>` mentah | `PASS` | Memakai base component `DataTable` |
| Uji Manual Browser Interaktif | Pengujian login peran kasir/perawat dan eksekusi jaringan live | `NOT FEASIBLE` | Membutuhkan backend API server dan database runtime aktif |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria / Syarat DoD | Status | Bukti |
| --- | --- | --- |
| Tab Rawat Inap menampilkan daftar surat kelayakan yang belum diakui | Terpenuhi | Tab "Rawat Inap" pada `consumer-handoffs-view.jsx` memfilter `handoffType: "INPATIENT"`, terbukti pada AC-1 & AC-6. |
| Badge status berwarna sesuai state (`CLEARED`: hijau, `BLOCKED`: merah, `REVOKED`: kuning/merah) | Terpenuhi | Terpasang `StatusBadge` dengan `INPATIENT_CLEARANCE_STATUS_CONFIG` (`region-status-active`, `region-status-inactive`, `region-status-warning`), terbukti pada AC-1 & AC-2. |
| Tombol Akui memanggil endpoint patch pengakuan | Terpenuhi | Terhubung ke `acknowledgeConsumerHandoff` (`PATCH /consumer-handoffs/{id}/acknowledge`), terbukti pada AC-5 & AC-6. |
| Tombol Evaluasi Ulang memicu re-evaluasi sisa tagihan | Terpenuhi | Terhubung ke `reevaluateInpatientClearance` (`POST /inpatient-clearance/reevaluate`), terbukti pada AC-4 & AC-5. |
| Peran tidak berwenang tidak melihat tombol akui / evaluasi ulang | Terpenuhi | Penjagaan RBAC ketat per baris pada `canAcknowledgeRow` dan `canReevaluateRow`, terbukti pada AC-5. |
| Keadaan kosong berbunyi ramah | Terpenuhi | Teks kosong rawat inap berbunyi ramah dan informatif, terbukti pada AC-6. |
| Pengakuan ganda dicegah dengan lock tombol saat submit | Terpenuhi | Properti `disabled={actionLoading}` terpasang pada tombol dan modal konfirmasi, mencegah klik berulang. |
| Tab Rawat Inap aktif, terhubung API, loading/empty/error state sesuai standar, `npm run build` lulus | Terpenuhi | Seluruh state ditangani; build lulus dengan exit code 0. |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru pada berkas yang disentuh. |
| Masalah yang diketahui | 8 kegagalan unit test pre-existing pada modul lain (`FE-RWI-*`, `blood-bank`, `petty-cash`) tidak disentuh karena di luar cakupan task ini. |
| Dependency backend | `BE-BKC-076` telah terpasang di backend. Verifikasi runtime live menunggu deployment dan migrasi database terpadu. |
| Perubahan sampingan | `NONE` — tidak ada refactor oportunistik maupun penghapusan file di luar scope. |
| Interupsi | `NONE`. |
| Status Git | `M src/components/view/health-services/billing-management/consumer-handoffs/consumer-handoffs-view.jsx`, `M consumer-handoffs-view.module.css`, `M billing-consumer-handoff-constants.js`, `M use-billing-consumer-handoffs.js`, `M billing-consumer-handoff-slice.jsx`, `?? tests/unit/billing-consumer-handoffs-inpatient.test.mjs`. |
| Langkah berikutnya | Melanjutkan pengerjaan task frontend pasangannya: `FE-BKC-042` (Panel Ringkasan Rawat Inap & Clearance pada Menu Pembayaran Kasir). |
