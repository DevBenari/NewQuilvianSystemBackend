# Laporan Perubahan Frontend — `FE-RWI-268`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-268` |
| Judul | Visualisasi Lencana CITO dan Tier Membership pada Tabel Antrean Transfer Rawat Inap |
| Slice | Visualisasi prioritas pasien antrean transfer — Gelombang 1 |
| Roadmap | [`frontend-roadmap-prioritas-antrean-member.md`](../../roadmap/frontend-roadmap-prioritas-antrean-member.md), kartu `FE-RWI-268` |
| Trace | `FR-TRF-009`; `RWI-DEC-338`, `RWI-DEC-341`; `RWI-AC-482`, `RWI-AC-486`; `TRF-AT-09`, `UAT-TRF-01`, `02`, `05` |
| Kontrak Target | `0.17.0`; Bab 20 Frontend Architecture, Bab 31 PRD ke MVP |
| Wewenang UI | Reuse `StatusBadge` existing; zero hardcoded color; CSS flex responsif dengan design tokens |
| Dependency | `BE-RWI-253` [BE] (Status: Selesai ✅) |
| Klasifikasi | `MEDIUM` — integrasi visualisasi lencana CITO & Tier Membership pada tabel antrean transfer |
| Task Mode | `CROSS-REPO`: source frontend `QuilvianSystemFrontendDev`; laporan & blueprint backend `NewQuilvianSystemBackend` |
| Target Tulis | `InpatientAdmissionTransferList.jsx`, `inpatient-admission-transfer-list.module.css` |
| Tanggal | 20 Oktober 2026 |
| Status | ✅ **SELESAI** — implementasi visualisasi lencana CITO & Tier pada kolom pasien selesai 100% |

---

## 1. Keadaan Awal (As-Is)

Sebelum perubahan ini:
- Kolom "Pasien" pada Tabel Antrean Pasien Transfer IGD & Rawat Jalan (`InpatientAdmissionTransferList.jsx`) hanya menampilkan Nama Pasien, No. Rekam Medis (`rmBadge`), dan tag jenis kelamin (`genderTag`).
- Status kegawatdaruratan klinis CITO dan status membership prioritas (Platinum, Gold, Silver) yang sudah dikembalikan oleh endpoint backend `GET /api/v1/health-services/inpatient-management/admission-transfers` belum divisualisasikan dalam bentuk lencana penanda.
- Petugas admisi rawat inap (PPRI) harus membaca catatan teks disposisi atau membuka detail pasien satu per satu untuk mengetahui apakah pasien tersebut berkategori CITO atau member prioritas rumah sakit.

---

## 2. Proses Bisnis & Pengalaman Pengguna (To-Be)

1. Petugas admisi membuka menu antrean transfer pasien rawat inap.
2. Daftar antrean tersaji secara urut dan deterministik dari backend (`BE-RWI-253`):
   - Urutan teratas: Pasien dengan instruksi klinis **CITO** (merah pekat).
   - Urutan berikutnya: Pasien dengan status **Member Prioritas CRM** (Platinum/Gold warna kuning/emas, Silver warna info/biru).
   - Urutan reguler: Pasien reguler atau member dengan masa aktif kedaluwarsa tanpa lencana tier.
3. Pada kolom pasien:
   - Nama pasien ditampilkan bersanding dengan lencana `[ CITO ]` (jika `isCito === true`) menggunakan komponen baku `StatusBadge` varian `danger`.
   - Lencana membership ditampilkan bertuliskan nama tier (misal `[ PLATINUM ]`, `[ GOLD ]`, `[ SILVER ]`) menggunakan `StatusBadge` varian `warning` atau `info` jika `isPriorityMember === true && memberTierName`.
   - Pasien reguler/non-member tampil bersih tanpa lencana membership (bebas polusi visual).
4. Tata letak kolom tabel menggunakan kontainer flex adaptif (`gap-1.5 flex-wrap`) sehingga tidak tumpang tindih atau merusak proporsi tabel pada layar desktop maupun perangkat mobile (390px s.d. 1440px).

---

## 3. Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx` | Mengekstrak properti `isCito`, `isPriorityMember`, dan `memberTierName` secara defensif; membungkus nama pasien dan lencana dalam `patientNameRow` & `patientBadges`; merender `StatusBadge` varian `danger` untuk CITO dan `warning`/`info` untuk Tier Membership. |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.module.css` | Menambahkan aturan kelas `.patientNameRow` dan `.patientBadges` berbasis design tokens (`var(--space-2)`, `var(--space-1)`) dengan perilaku flex wrapping yang rapi. |

---

## 4. Kepatuhan Arsitektur Frontend & Base Component Gate

| Elemen UI | Keputusan | Bukti Implementasi |
| --- | --- | --- |
| Lencana CITO | `REUSE` `StatusBadge` | Memakai `<StatusBadge status="danger" label="CITO" showIcon={false} />` (standar token `var(--color-danger)`). |
| Lencana Member Tier | `REUSE` `StatusBadge` | Memakai `<StatusBadge status={memberTierName.toLowerCase() === "silver" ? "info" : "warning"} label={memberTierName.toUpperCase()} showIcon={false} />` (standar token `var(--color-warning)` / `var(--color-info)`). |
| Kontainer Lencana | `COMPOSE` flex layout | Menggunakan flexbox murni dengan CSS module tanpa hardcoded margin/padding pixel literal. |
| Base Component Baru | `NONE` | 100% menggunakan base component yang sudah ada (`@/components/features/base-features/status-badge`). |

**UI GATE**: `REUSE` 100% komponen; tidak ada komponen baru atau modifikasi nilai token bawaan.

---

## 5. Pemetaan Acceptance Criteria & UAT

| Kode AC / Test | Kriteria | Hasil Verifikasi |
| --- | --- | --- |
| `RWI-AC-482` | Lencana CITO berwarna merah pekat muncul jika `item.isCito === true` | ✅ Terpenuhi; dirender via `StatusBadge` status danger |
| `RWI-AC-486` | Lencana tier muncul jika `item.isPriorityMember === true` dan memiliki nama tier; tidak muncul untuk reguler/expired | ✅ Terpenuhi; kondisional `isPriorityMember && memberTierName` |
| `TRF-AT-09` | Daftar antrean menyajikan lencana visual CITO dan Tier Membership | ✅ Terpenuhi pada kolom Pasien |
| `UAT-TRF-01` | Pasien CITO berada di urutan teratas dengan lencana [CITO] merah pekat | ✅ Terpenuhi bersanding dengan urutan multi-kriteria backend |
| `UAT-TRF-02` | Pasien member Platinum/Gold/Silver menampilkan lencana tier sesuai hierarki | ✅ Terpenuhi dengan varian warna warning dan info |
| `UAT-TRF-05` | Tata letak kolom tetap rapi dan tidak tumpang tindih | ✅ Terpenuhi; verifikasi CSS flex wrap adaptif |

---

## 6. Validasi & Integritas Kode

- `git diff --check`: Bersih tanpa konflik whitespace atau indentasi.
- Desain visual: Mematuhi wireframe Bab 1 pada `frontend-roadmap-prioritas-antrean-member.md`.
- Kerapian tata letak: Komponen badge memiliki `flex-shrink: 0` dan terbungkus dalam `gap-1.5` yang tidak menabrak batas kolom No. RM dan Tag Gender.
