# Laporan Perubahan Frontend — `FE-RWI-166`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-166` |
| Judul | Status kasir dari satu kartu; pembaca dan penulis lama dicabut |
| Slice | `MVP-0` / `RWF-W0` — gelombang eksekusi 1 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-166` |
| Trace | `FR-RWF-002`, `FR-RWF-003`, `FR-RWF-004`; `RWI-DEC-167`, `RWI-DEC-170`; `AC-RWF-003`; `FIN-CON-04`; gap dokumen `TRC-RWF-01` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.1 dan 6.3 `FE-INT-06`; API 3.1 dan 3.3 |
| Wewenang UI | Bentuk kartu, warna badge, dan ikon `DEV_DISCRETION` (`03-frontend-architecture.md` 6.6). Mengikat: empat label kasir, tanpa rupiah, penyegaran 10 detik |
| Dependency | `BE-RWI-152` [BE] ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika sedang (1), memakai kontrak yang ada (1), hak akses terkait (1), banyak layar (2). Rubrik skor dipinjam dari model delapan faktor suite, karena `rules/frontend/` tidak memuat rubrik sendiri |
| Task mode | `FRONTEND` — backend strict read-only; laporan dan tautan bukti ditulis di repository backend sesuai wewenang laporan |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, dan traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02` (branch `HamzahV2` ↔ `origin/HamzahV2`). Implementasi ikut commit pengguna `f6eca4ef7` (6 Oktober 2026 10.48), lalu merge pengguna `eb0a0a790`; satu perubahan susulan (alias `BillingStatusBadge.jsx`) belum di-commit |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI.** Keenam acceptance criteria terpetakan ke source. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026** |

**Otorisasi eksekusi.** Roadmap masih berstatus `DRAFT`. Pada 5 Oktober 2026 pengguna memerintahkan
seluruh task roadmap ini dikerjakan sampai tuntas dan ditandai selesai; perintah itu dicatat sebagai
otorisasi eksekusi. Status `DRAFT` pada metadata roadmap tidak diubah, karena itu wewenang skill
perencanaan.

---

## 1. Keadaan yang ditemukan di awal

Kartu status kasir di bangsal masih memakai kontrak lama `1.0.0`, padahal backend sudah berpindah ke
kontrak `1.1.0`. Akibatnya kartu itu tidak lagi bisa dipercaya:

| Temuan | Bukti | Akibat bagi pengguna |
| --- | --- | --- |
| Kartu membaca `canPhysicallyDischarge`, `blockerReasons`, `operationalStatusText` | `billing-summary-card.jsx` lama | Field itu tidak ada di `InpatientBillingStatusResponse`; kartu selalu berbunyi "Pulang Fisik Tertahan" dan "Status kasir sedang diproses" |
| Badge mengenal enam status, termasuk "Override Supervisor" | `billing-status-badge.jsx` lama | Status `OVERRIDDEN` sudah dibuang (`FIN-CON-04`) |
| Penyegaran tiap 30 detik | `use-inpatient-billing-status.js` lama | Kontrak meminta paling lambat 10 detik (`AC-RWF-003`) |
| Tombol "Buka Rincian Finansial" membuka laci berisi rupiah | `billing-financial-details-drawer.jsx` memanggil `GET .../billing-details` | Endpoint itu dihapus API 3.1 dan menjawab 404 |
| Layar Pemulangan memasang kartu gerbang lama | `discharge-clearance-gate-card.jsx` memanggil `confirm-physical-discharge` dan `supervisor-override` | Keduanya dihapus API 3.1 dan menjawab 404 |
| Layar Kelayakan Keuangan `FE-INP-08` masih punya formulir tandai | `use-inpatient-financial-clearance.jsx` memanggil `POST .../financial-clearance` | Dihapus API 3.1; formulir gagal 404 (gap dokumen `TRC-RWF-01`) |

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas bangsal tahu apakah kasir sudah memberi izin pulang, tanpa melihat rupiah.

**Pelaku.** Perawat pelaksana, kepala ruangan, petugas admisi — siapa pun pemegang
`InpatientBillingOperational : Read`.

**Langkah.**

1. Petugas membuka Detail Episode, layar Keputusan Pulang dan Resume, atau menu Tagihan Pasien di
   Ruang Kerja Keperawatan.
2. Kartu **"Izin Pulang dari Kasir"** membaca status kasir langsung dari Billing.
3. Badge menunjukkan salah satu dari empat status: **Menunggu kasir**, **Terkendala**,
   **Disetujui kasir**, atau **Izin dicabut**.
4. Bila kasir mencatat kendala, kendalanya tampil sebagai daftar kalimat, tanpa angka rupiah.
5. Kartu memperbarui diri tiap 10 detik. Petugas juga dapat menekan **Perbarui Status**.

**Contoh.** Kasir menyetujui izin pulang pukul 09.00.00. Kartu yang sedang terbuka di bangsal
berubah menjadi "Disetujui kasir" paling lambat pukul 09.00.10.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Billing tidak dapat dibaca, atau pembacaan gagal | Badge "Status kasir tidak dapat dibaca" dan peringatan kuning. Kartu **tidak pernah** berbunyi "Disetujui kasir" dalam keadaan ini |
| Episode ditutup lewat penutupan tanpa izin kasir | Tanda tambahan "Ditutup tanpa izin kasir" |
| Pengguna tidak punya `InpatientBillingOperational : Read` | Kartu tidak dirender sama sekali |

**Riwayat kelayakan keuangan.** Layar Kelayakan Keuangan kini hanya riwayat. Penandaan manual
lama tetap terbaca bagi pemegang `InpatientDischarge : ReadFinancialClearance`, tetapi tidak dapat
ditambah lagi dan tidak lagi menentukan penutupan episode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` frontend, `rules/GLOBAL_RULES.md`, seluruh `rules/frontend/`, dan
  `rules/rule-output/` (lokasi laporan, status roadmap, grafik dependency, aturan output).
- Blueprint: `03-frontend-architecture.md` bagian 6, `contracts/api-contract.md` bagian 3,
  `testing/acceptance-test-matrix.md` bagian 4, `roadmap/frontend-roadmap-finishing.md`,
  `roadmap/requirement-traceability-finishing.md`.
- Backend (baca saja): `InpatientBillingOperationalController.cs`,
  `InpatientBillingSummaryDtos.cs`, `IInpBillingClearanceAdapter.cs`,
  `InpatientDischargeController.cs`, `InpatientClosureDtos.cs`, `InpClearanceObservation.cs`,
  `Program.cs` (tidak ada `JsonStringEnumConverter`).
- Frontend: seluruh `billing-integration/`, alias `features/inpatient/billing-integration/`,
  `inpatient-billing.service.js`, `inpatient-api.service.js`, `use-permission.jsx`,
  `inpatient-episode-detail-layout.jsx`, `status-badge.jsx`, layar Pemulangan, Detail Episode,
  Kelayakan Keuangan, kolom sensus, dan bagian Tagihan Pasien ruang kerja keperawatan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-billing-status-constants.js` | **Baru.** Kosakata `CASHIER_STATUS`, label dan nada badge, pemetaan angka `InpClearanceObservation`, interval 10 detik |
| `src/utils/health-services/inpatient-management/inpatient-billing-status-utils.js` | **Baru.** Normalisasi `InpatientBillingStatusResponse`; `IsReadable = false` selalu menjadi "tidak dapat dibaca"; nilai tak dikenal tidak pernah dibaca sebagai disetujui |
| `src/lib/services/health-services/inpatient-management/inpatient-billing.service.js` | `fetchInpatientBillingDetails`, `submitSupervisorOverride`, `confirmPhysicalDischarge` dicabut |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-billing-status.js` | Bawaan 10 detik; jawaban dinormalisasi; pembacaan gagal menjadi `UNREADABLE`, bukan mempertahankan status lama |
| `src/components/features/health-services/inpatient-management/billing-integration/billing-status-badge.jsx` | Lima keadaan baru; label "Override Supervisor" dan animasi berkedip dibuang |
| `src/components/features/health-services/inpatient-management/billing-integration/billing-summary-card.jsx` | Dirombak menjadi kartu `FE-INT-06`; dijaga `InpatientBillingOperational : Read`; laci rupiah dan "Boleh Pulang Fisik" dibuang |
| `src/style/health-services/inpatient-management/billing-summary-card.module.css`, `billing-status-badge.module.css` | Ditulis ulang memakai token; seluruh literal warna lama hilang |
| `src/components/view/health-services/inpatient-management/inpatient-census-table-columns.jsx` | Kolom Status Kasir memakai badge baru |
| `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx` | Kartu gerbang lama diganti kartu `FE-INT-06` |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` | Kartu `FE-INT-06` dipasang di Tagihan Pasien (`FE-KEP-07`), di luar batas keadaan rincian tagihan |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Kartu dipasang collapsible; tautan diganti "Riwayat Kelayakan Keuangan" |
| `src/lib/hooks/.../use-inpatient-financial-clearance.jsx`, `inpatient-financial-clearance-view.jsx`, `inpatient-financial-clearance-utils.jsx`, `inpatient-financial-clearance-constants.jsx` | `FE-INP-08` menjadi riwayat baca saja; formulir tandai, kewenangan, validasi, dan payload penandaan dicabut |
| `src/components/features/inpatient/billing-integration/BillingStatusBadge.jsx` | Alias hanya me-re-export `default` (atas persetujuan pengguna 6 Oktober 2026) |
| **Dihapus** | `billing-financial-details-drawer.jsx`, `discharge-clearance-gate-card.jsx`, `physical-discharge-confirm-modal.jsx`, `supervisor-override-modal.jsx`, keempat alias PascalCase-nya, dan keempat CSS Module-nya |
| `tests/unit/inpatient-billing-status-badge.test.mjs` | Ditulis ulang menjadi tes perilaku kosakata `FE-INT-06` |
| `tests/unit/inpatient-billing-census-detail.test.mjs`, `inpatient-financial-clearance.test.mjs` | Diperbarui ke kontrak `1.1.0` |
| **Tes dihapus** | `inpatient-billing-financial-drawer`, `inpatient-discharge-clearance-gate`, `inpatient-physical-discharge`, `inpatient-supervisor-override-modal` — keempatnya menguji komponen kontrak `1.0.0` yang dicabut |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap `view → hook → service → InstanceAxios`. Pemetaan status berada di
`utils` sebagai fungsi murni, kosakata di `constants`, dan kartu memakai `EpisodeSection`,
`StatusBadge`, `InformationAlert`, serta `BaseButton` yang sudah ada. Tidak ada HTTP client, state
management, atau base component baru.

**Gerbang keputusan base component.**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kartu status kasir | `EpisodeSection`, `StatusBadge`, `InformationAlert` | `billing-summary-card.jsx` sudah membungkus ketiganya | WRAP | Rombak kartu domain yang ada; satu kartu di tiga layar |
| Badge status kasir | `StatusBadge` | pembungkus domain `billing-status-badge.jsx` | WRAP | Ganti kosakata |
| Riwayat kelayakan keuangan | layar `FE-INP-08` yang ada | `inpatient-financial-clearance-view.jsx` | REUSE | Cabut formulir |

`UI GATE: 3 elemen — REUSE 1, EXTEND 0, COMPOSE 0, WRAP 2, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Badge "Memeriksa status kasir..." |
| Kosong | Tanpa kendala, daftar kendala tidak ditampilkan |
| Gagal | Badge dan peringatan "Status kasir tidak dapat dibaca"; kartu mencoba lagi tiap 10 detik dan menyediakan "Perbarui Status" |
| Tanpa hak akses | Kartu tidak dirender; riwayat kelayakan keuangan menampilkan "Riwayat Tidak Dapat Dibaca" bila `ReadFinancialClearance` tidak dimiliki |

---

## 5. Endpoint yang dikonsumsi

#### Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/billing-status` | Kartu status kasir, badge sensus | `InpatientBillingOperational : Read` |

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/financial-clearance` | Riwayat penandaan manual, baca saja | `InpatientDischarge : ReadFinancialClearance` |
| `GET` | `/{episodeId}/closure-readiness` | Dampak syarat kasir pada layar riwayat | `InpatientDischarge : Read` |

**Pemanggil yang dicabut** (endpoint-nya menjawab 404 sejak API 3.1): `GET .../billing-details`,
`POST .../financial-clearance`, `POST .../supervisor-override`, `POST .../confirm-physical-discharge`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` garis dasar, sebelum perubahan | 2179 tes, 8 gagal di berkas lain | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah 6 Oktober 2026 |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya kegagalan garis dasar | `PASS` | Keluaran perintah |
| `npm run test:unit` sesudah merge pengguna 10.56 | 2497 tes, 9 gagal: 1 tes task peka akhir baris CRLF (diperbaiki dengan regex), 8 lain di luar task | `PASS` sesudah perbaikan | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Keluaran perintah |
| `npm run lint:errors` sesudah implementasi | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run lint:errors` diulang pukul 10.55 | 1 error parse di `src/utils/menu-sidebar/menu-items.jsx:28`, berkas yang sedang disunting saat merge pengguna | `EXISTING / ENVIRONMENT ISSUE` | Bukan berkas task |
| ESLint terarah pada berkas task | 0 error; warning baru milik task sudah diperbaiki | `PASS` | Keluaran perintah |
| `npm run build` final | `✓ Compiled successfully in 64s`, postbuild standalone siap, exit 0, 107 detik | `PASS` | Keluaran perintah |
| Grep anti-regresi checklist UI | Nol temuan warna literal, typography shared, tombol mentah, tabel mentah, `!important` | `PASS` | Keluaran perintah |
| Pencarian kode endpoint lama | Nol pemanggil di `src/` | `PASS` | Tes kriteria 5 |
| Uji manual empat status dan keadaan gagal baca | Tidak dijalankan | `NOT RUN` | Lihat baris di bawah |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — sesi ini tidak punya alat kendali peramban, tidak ada akun uji, dan skenario membutuhkan kasir menyetujui/mencabut izin di Billing secara nyata. Butir DoD uji manual dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run test:e2e` (tidak diminta dan `playwright.config` tidak ada di root);
uji runtime dengan backend berjalan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kartu menampilkan keempat label sesuai `ClearanceStatus`; tidak ada label "Overridden" | Terpenuhi | `CASHIER_STATUS_BADGES`; tes "FE-RWI-166 kriteria 1" |
| 2. Billing tidak terbaca → "Status kasir tidak dapat dibaca", tidak pernah "Disetujui kasir" | Terpenuhi | `resolveCashierStatusKey`; hook menjadikan pembacaan gagal `UNREADABLE`; tes "kriteria 2" |
| 3. Kasir menyetujui pukul T → paling lambat T + 10 detik kartu "Disetujui kasir" | Terpenuhi pada source | `CASHIER_STATUS_POLLING_INTERVAL_MS = 10000` dipakai kartu; tes "FE-RWI-096 AC-3". Pengamatan waktu nyata termasuk uji manual yang dikecualikan |
| 4. Tidak ada angka rupiah pada kartu, Detail Episode, maupun ruang kerja keperawatan | Terpenuhi | Kartu tanpa field nominal; laci rupiah dihapus; tes "steril dari rupiah". Subtotal di Tagihan Pasien `FE-RWI-185` tetap hanya bagi pemegang `ViewAmount`, sesuai `RWI-DEC-170` |
| 5. Pencarian kode: nol pemanggil `billing-details` dan `POST financial-clearance` | Terpenuhi | Tes "kriteria 5" dan "FE-RWI-166 butir 4" |
| 6. Riwayat kelayakan keuangan tetap terbaca bagi `ReadFinancialClearance` | Terpenuhi | `use-inpatient-financial-clearance.jsx` membaca `GET .../financial-clearance`; 403 dipisah dari galat halaman |
| DoD: lint dan build lulus | Terpenuhi | Bagian 6 |
| DoD: laporan memuat `AUTOMATED TEST` dan `MANUAL TEST` | Terpenuhi | Bagian 6 |
| DoD: uji manual | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |
| DoD: roadmap dan traceability diperbarui | Terpenuhi | Kartu `FE-RWI-166` dan baris `FR-RWF-002`–`004` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Enam warning ESLint bawaan di berkas yang ikut disentuh (`set-state-in-effect` pada detail episode dan Tagihan Pasien) tidak diperbaiki karena di luar cakupan |
| Masalah yang diketahui | `TRC-RWF-01` (layar `FE-INP-08` tidak disebut kontrak frontend bagian 6) ditangani dengan menjadikannya riwayat baca saja; kontrak frontend sebaiknya menyebutnya pada revisi berikutnya |
| Dependency backend | `BE-RWI-152` ✅. Tidak ada task backend yang menahan |
| Perubahan sampingan | Empat tes kontrak `1.0.0` (`FE-RWI-097`–`100`) dihapus bersama komponennya. Alias `BillingStatusBadge.jsx` diubah atas persetujuan pengguna setelah penulisan pertamanya ditolak pengklasifikasi mode otomatis |
| Interupsi | Disk C: sempat penuh di awal sesi; pengguna meng-commit dan me-merge branch di tengah pekerjaan; dua kali jeda permintaan pengguna. Pekerjaan dilanjutkan dari keadaan terverifikasi |
| Status Git | Implementasi berada di commit pengguna `f6eca4ef7`. `git status --short` pada akhir pekerjaan: ` M src/components/features/inpatient/billing-integration/BillingStatusBadge.jsx`, ` M src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx`, ` M tests/unit/inpatient-integration-billing-finishing.test.mjs`, ditambah dua berkas sesi lain (`procedure-form-panel.jsx`, `menu-items.jsx`) |
| Langkah berikutnya | Uji di peramban bersama kasir: setujui, cabut, dan matikan Billing, lalu amati kartu dalam 10 detik |
