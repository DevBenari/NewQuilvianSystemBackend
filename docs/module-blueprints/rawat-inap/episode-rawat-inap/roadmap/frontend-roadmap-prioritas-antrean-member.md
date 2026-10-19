# Roadmap Frontend Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` (bentuk `COMPOSITE`) |
| planning_id / revision / tanggal | `TRF-DEL-20261019-01` / 1 / 2026-10-19 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; disetujui untuk implementasi demo Selasa 20 Oktober 2026. Eksekusi memakai `build-module-frontend`, tepat satu task per pemanggilan |
| kontrak terkunci | `0.17.0`; [Frontend 20](../03-frontend-architecture.md), [PRD 31](../04-prd-to-mvp.md), [Testing 25](../testing/acceptance-test-matrix.md) |
| source snapshot | Frontend `HamzahV2@5ad45a6944b3c320038cc48bb288471cd8f2910a`; backend `MHamzah@7a4d443fcdf0c9d33cc1a9f4be81522b4fadaefb` |
| hash lock | [prioritas-antrean-member-delivery-lock.json](prioritas-antrean-member-delivery-lock.json) |
| scope / ID | 2 task: `FE-RWI-268` dan `FE-RWI-269`; berikutnya `FE-RWI-270` |
| roadmap pasangan | [Backend](backend-roadmap-prioritas-antrean-member.md) · [Traceability](requirement-traceability-prioritas-antrean-member.md) |
| target demo | Demo Operasional Selasa, 20 Oktober 2026 (`IGD-REQ-004`) |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai dan seluruh acceptance criteria serta Definition of Done terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` (Belum dikerjakan) |

## Grafik Urutan Dependency

```text
BE-RWI-253 [BE] ──> FE-RWI-268 ──> FE-RWI-269
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-prioritas-antrean-member.md](backend-roadmap-prioritas-antrean-member.md). Garis selalu prasyarat → task.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
|---:|---|---|---|
| 1 | `BE-RWI-253` [BE] | `FE-RWI-268` | Visualisasi lencana CITO dan Tier Membership pada baris pasien di Tabel Antrean Transfer (`InpatientAdmissionTransferList.jsx`) |
| 2 | `FE-RWI-268` | `FE-RWI-269` | Visualisasi lencana CITO dan Tier Membership pada Header Langkah Admisi PPRI (`InpatientAdmissionTransferReviewStep.jsx`) dan verifikasi isolasi privasi Denah Bed Management |

Setiap task muncul tepat satu kali dan pasangan dependency grafik sama persis dengan kolom Dependency pada register.

## Skema Tampilan Antarmuka (UI Wireframe & Layout)

Amandemen ini mengubah dua layar utama dan menegaskan invariant privasi pada satu layar denah bangsal:
1. **Layar 1 (`InpatientAdmissionTransferList.jsx`)**: Tabel Antrean Transfer Pasien Masuk IGD & Rawat Jalan.
2. **Layar 2 (`InpatientAdmissionTransferReviewStep.jsx`)**: Header Langkah Admisi PPRI Transfer (Wizard Admisi Rawat Inap).
3. **Layar 3 (`bed-management-view.jsx`)**: Denah Bed Management Bangsal Umum (Penegasan Isolasi Privasi Pasien).

---

### 1. Skema Tampilan 1: Tabel Antrean Transfer Pasien (`InpatientAdmissionTransferList.jsx`)

#### A. Perbandingan Visual Kolom Pasien (Sebelum vs Sesudah)

```text
SEBELUM:
┌──────────────────────────────────────────────────────────────┐
│ Pasien                                                       │
├──────────────────────────────────────────────────────────────┤
│ Tn. Budi Santoso                                             │
│ [No. RM: 00-12-34]  [L]                                      │
└──────────────────────────────────────────────────────────────┘

SESUDAH (Kontrak 0.17.0 - RWI-DEC-338 & RWI-DEC-341):
┌──────────────────────────────────────────────────────────────┐
│ Pasien                                                       │
├──────────────────────────────────────────────────────────────┤
│ Tn. Hendra Gunawan                  [ CITO ]                 │  <-- Pasien CITO (Merah)
│ [No. RM: 00-12-34]  [L]                                      │
├──────────────────────────────────────────────────────────────┤
│ Ny. Sri Wahyuni                     [ PLATINUM ]             │  <-- Member Prioritas (Emas)
│ [No. RM: 00-56-78]  [P]                                      │
├──────────────────────────────────────────────────────────────┤
│ Tn. Ahmad Fauzi                     [ GOLD ]                 │  <-- Member Gold
│ [No. RM: 00-78-90]  [L]                                      │
├──────────────────────────────────────────────────────────────┤
│ Tn. Bambang Sutrisno                                         │  <-- Pasien Reguler / Expired
│ [No. RM: 00-99-11]  [L]                                      │
└──────────────────────────────────────────────────────────────┘
```

#### B. Wireframe Lengkap Antrean Transfer Pasien (Tabel Terurut Multi-Kriteria)

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  [⇄] Antrean Pasien Transfer IGD & Rawat Jalan       [ 12 Pasien ]  [ 2 Menunggu > 60 Mnt ]                       [ ↻ Segarkan ] │
│  Pasien IGD/Poliklinik yang telah dinyatakan rawat inap oleh dokter dan sedang menunggu alokasi kamar bangsal.                    │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  [ Cari No. RM atau Nama Pasien... ]  [ Semua Asal ▾ ]  [ [✓] Hanya Menunggu > 60 Mnt ]                      [ Reset Filter ]    │
├──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│  Pasien / Prioritas           │ Asal Kunjungan │ Waktu Disposisi │ DPJP & Indikasi Ranap │ Unit Rekomendasi │ Tindakan Admisi    │
├───────────────────────────────┼────────────────┼─────────────────┼───────────────────────┼──────────────────┼────────────────────┤
│  Tn. Hendra Gunawan           │ [IGD]          │ 09:15 WIB       │ dr. Tirta, Sp.JP      │ Bangsal Jantung  │ [ Proses Admisi ]  │
│  [No. RM: 00-88-21] [L]       │ EMG-261019-004 │ (15 mnt lalu)   │ ACS STEMI Anteroseptal│ Kelas 1 / ICU    │                    │
│  [ CITO ]                     │                │                 │                       │                  │                    │
├───────────────────────────────┼────────────────┼─────────────────┼───────────────────────┼──────────────────┼────────────────────┤
│  Ny. Sri Wahyuni              │ [IGD]          │ 08:30 WIB       │ dr. Hendro, Sp.PD     │ Bangsal Penyakit │ [ Proses Admisi ]  │
│  [No. RM: 00-56-78] [P]       │ EMG-261019-002 │ (60 mnt lalu)   │ DHF Grade II Dehidrasi│ Dalam VIP        │                    │
│  [ PLATINUM ]                 │                │                 │                       │                  │                    │
├───────────────────────────────┼────────────────┼─────────────────┼───────────────────────┼──────────────────┼────────────────────┤
│  Tn. Ahmad Fauzi              │ [Poliklinik]   │ 08:45 WIB       │ dr. Budi, Sp.B        │ Bangsal Bedah    │ [ Proses Admisi ]  │
│  [No. RM: 00-78-90] [L]       │ POL-261019-011 │ (45 mnt lalu)   │ Hernia Inguinalis     │ Kelas 2          │                    │
│  [ GOLD ]                     │                │                 │                       │                  │                    │
├───────────────────────────────┼────────────────┼─────────────────┼───────────────────────┼──────────────────┼────────────────────┤
│  Nn. Jessica Salim            │ [Poliklinik]   │ 09:00 WIB       │ dr. Maria, Sp.M       │ Bangsal Umum     │ [ Proses Admisi ]  │
│  [No. RM: 00-44-32] [P]       │ POL-261019-015 │ (30 mnt lalu)   │ Post Operasi Katarak  │ Kelas 2          │                    │
│  [ SILVER ]                   │                │                 │                       │                  │                    │
├───────────────────────────────┼────────────────┼─────────────────┼───────────────────────┼──────────────────┼────────────────────┤
│  Tn. Bambang Sutrisno         │ [IGD]          │ 07:45 WIB       │ dr. Hendro, Sp.PD     │ Bangsal Paru     │ [ Proses Admisi ]  │
│  [No. RM: 00-99-11] [L]       │ EMG-261019-001 │ (105 mnt lalu)  │ GEA Dehidrasi Ringan  │ Kelas 3          │                    │
│  (Reguler / Non-Member)       │                │ ! Overdue       │                       │                  │                    │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 2. Skema Tampilan 2: Header Langkah Admisi PPRI Transfer (`InpatientAdmissionTransferReviewStep.jsx`)

Layar ini menyajikan verifikasi pasien transfer sebelum petugas memilih kamar rawat inap:

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│  [ TRANSFER PASIEN IGD ]  [ No. Kunjungan: EMG-261019-004 ]  [ CITO ]  [ MEMBER PLATINUM ]  [ ⏱ Waktu Tunggu: 15 Menit ]         │
│  Verifikasi Identitas & Rujukan Pasien IGD                                                                                       │
│  Periksa kecocokan data rekam medis dan instruksi rujukan sebelum penetapan kamar dan penjamin rawat inap.                       │
├─────────────────────────────────────────────────────────────────┬────────────────────────────────────────────────────────────────┤
│  KOLOM KIRI: Identitas Terverifikasi (BasePatientVerification)  │  KOLOM KANAN: Instruksi Klinis & Disposisi Transfer IGD        │
├─────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────┤
│  [ Foto / KTP Pasien ]                                          │  Unit Perujuk      : IGD Utama                                 │
│                                                                 │  DPJP Pengirim     : dr. Tirta, Sp.JP                          │
│  Nama Pasien  : Tn. Hendra Gunawan                              │  Waktu Disposisi   : 19 Okt 2026, 09:15 WIB                    │
│  No. RM       : 00-88-21                                        │  Alasan Rawat Inap : ACS STEMI Anteroseptal (CITO)             │
│  NIK          : 3201234567890001                                │  Rekomendasi Unit  : Bangsal Jantung / ICU                     │
│  Tanggal Lahir: 14 Mei 1978 (48 Thn)                            │  Rekomendasi Kelas : VIP / VVIP                                │
│  Jenis Kelamin: Laki-laki                                       │                                                                │
│  Kontak / WA  : 081234567890                                    │  Catatan TTV:                                                  │
│                                                                 │  - TD: 150/95 mmHg · HR: 110 x/mnt · RR: 24 x/mnt · SpO2: 96%  │
│  Status Member: [ PLATINUM ACTIVE ]                             │  Instruksi Lanjutan:                                           │
│  Berlaku S.d. : 31 Des 2026 (Priority Queue Active)             │  Siapkan oksigen nasal kanul 3 lpm dan monitor EKG kontinu.    │
├─────────────────────────────────────────────────────────────────┴────────────────────────────────────────────────────────────────┤
│  [ < Kembali ke Antrean ]                                                   [ Tolak / Minta Klarifikasi ]  [ Pilih Kamar & Bed > ]│
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 3. Skema Tampilan 3: Denah Bed Management Bangsal Umum (`bed-management-view.jsx`) — Penegasan Isolasi Privasi

Sesuai `RWI-DEC-341` dan `RWI-AC-488`, denah bangsal umum **DILARANG** menampilkan lencana tier membership pasien demi menjaga privasi:

```text
┌──────────────────────────────────────────────────────────────┐
│                           Kelas 2 329                     ✕  │
│                      Lantai 3 · Barat Laut                   │
│     ● Kelas 2 | Tersedia | High Dependency (1 HD · 1 LD)     │
├──────────────────────────────────────────────────────────────┤
│ 329-A ● Terisi    Tn. Hendra G. · RM 00-88-21 · High Dep.    │  <-- BERSIH DARI LENCANA TIER (Privasi Terjaga)
│ 329-B ● Terisi    Ny. Sari W.   · RM 00-56-78 · Low Dep.     │  <-- BERSIH DARI LENCANA TIER
│ 329-C ● Tersedia                     [ Pilih bed ini ]       │
└──────────────────────────────────────────────────────────────┘
```

---

### 4. Spesifikasi Komponen & Design Token

| Elemen UI | Komponen / Tag | Varian / Props | Label Teks | Penjelasan & Rationale |
|---|---|---|---|---|
| Lencana CITO | `StatusBadge` | `variant="danger"` | `"CITO"` | Warna merah pekat tegas untuk pasien gawat darurat klinis. Ditampilkan jika `isCito === true`. |
| Lencana Member Platinum | `StatusBadge` | `variant="warning"` | `"Platinum"` / `"MEMBER PLATINUM"` | Warna emas/kuning elegan penanda tier prioritas tertinggi. |
| Lencana Member Gold | `StatusBadge` | `variant="warning"` | `"Gold"` / `"MEMBER GOLD"` | Warna kuning/emas penanda tier prioritas menengah. |
| Lencana Member Silver | `StatusBadge` | `variant="primary"` | `"Silver"` / `"MEMBER SILVER"` | Warna biru/perak penanda tier prioritas dasar. |
| Non-Member / Expired | — | — | — | Tidak menampilkan lencana tier apa pun (bebas clutter). |
| Container Lencana | `div` flex | `gap-1.5 items-center flex-wrap` | — | Memastikan lencana CITO dan Tier bersanding rapi tanpa merusak tata letak tabel pada mobile. |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Keputusan | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `FE-RWI-268` | Visualisasi lencana CITO (Merah) dan Nama Tier Membership (Kuning/Emas) pada baris pasien di Tabel Antrean Transfer Rawat Inap | FR-TRF-009; RWI-DEC-338, 341 | 0.17.0; Bab 20 Frontend, Bab 31 PRD | `InpatientAdmissionTransferList.jsx`, `StatusBadge` | Di samping nama/RM pasien, tambahkan container badge flex merender `StatusBadge` CITO dan `StatusBadge` Tier; verifikasi rendering urutan deterministik dari backend | `BE-RWI-253` [BE] | RWI-AC-482, RWI-AC-486; TRF-AT-09, UAT-TRF-01, 02, 05 | `V-FE`; `npm run lint`, `npm run build`, uji responsif mobile/desktop | Frontend HealthServices; kerapian tata letak kolom tabel | `D-FE`; [laporan](../task/report/frontend/FE-RWI-268.md) |
| `FE-RWI-269` | Visualisasi lencana CITO dan Tier Membership pada Header Langkah Admisi PPRI dan penegakan isolasi privasi pada Denah Bed Management | FR-TRF-010, FR-TRF-011; RWI-DEC-338, 341 | 0.17.0; Bab 20 Frontend, Bab 31 PRD | `InpatientAdmissionTransferReviewStep.jsx`, `bed-management-view.jsx` | Tampilkan lencana CITO & Tier pada header ringkasan identitas pasien wizard admisi transfer; pastikan tombol pemilihan kamar membuka Denah Bed Management manual; verifikasi denah bangsal umum bebas lencana tier | `FE-RWI-268` | RWI-AC-487, RWI-AC-488, RWI-AC-489; TRF-AT-10, 11, 12, UAT-TRF-05, 06 | `V-FE`; `npm run lint`, `npm run build`, uji visual header dan denah bangsal | Frontend HealthServices; kepatuhan privasi pasien bangsal | `D-FE`; [laporan](../task/report/frontend/FE-RWI-269.md) |

## Batas Handoff dan Definition of Done

### V-FE — Bukti Wajib Setiap Task Frontend

Task frontend dikerjakan menggunakan konvensi Next.js App Router, JavaScript/JSX, Redux, Axios `InstanceAxios`, serta design token dan base component Quilvian (`@/components/features/base-features/`).

1. `npm run lint` lulus tanpa error atau warning baru.
2. `npm run build` lulus secara bersih tanpa kegagalan bundle.
3. Review kesesuaian visual: lencana CITO berwarna merah pekat, lencana tier berwarna kontras dan mudah dibaca, serta tata letak tabel tetap rapi pada layar 390px s.d. desktop.
4. Laporan tracked disimpan di `task/report/frontend/<TASK-ID>.md`.

### D-FE — Selesai Berarti

- Outcome dan acceptance criteria terpenuhi 100%.
- Laporan task tracked ditulis lengkap dengan screenshot / bukti verifikasi komponen.
- Status task pada roadmap diberi tanda ✅ hanya setelah bukti nyata ada di laporan.

---

## Kartu Task

### FE-RWI-268 — Visualisasi Lencana CITO dan Tier Membership pada Tabel Antrean Transfer

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Tabel Antrean Transfer Rawat Inap (`InpatientAdmissionTransferList.jsx`) menampilkan lencana visual CITO (Merah) dan Lencana Tier Membership (Kuning/Emas) pada kolom pasien secara rapi dan adaptif |
| File Terdampak | `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx` |
| Jejak Requirement | `FR-TRF-009`; `RWI-DEC-338`, `341`; `RWI-AC-482`, `486` |
| Kontrak Target | `0.17.0`; Bab 20 Frontend Architecture, Bab 31 PRD ke MVP |
| Reuse | `InpatientAdmissionTransferList`, komponen `StatusBadge` atau `Badge` base-features |
| Dependency | `BE-RWI-253` [BE] |
| Verifikasi yang Wajib | `V-FE` (`npm run lint`, `npm run build`) + pengujian render lencana pada data campuran |
| Definition of Done | `D-FE`; laporan `task/report/frontend/FE-RWI-268.md` |

**Kriteria Penerimaan**:
- Pada kolom pasien di tabel antrean transfer, di samping nomor RM dan nama pasien disajikan:
  - Lencana CITO bertuliskan `"CITO"` dengan varian `danger` jika `item.isCito === true`.
  - Lencana Tier bertuliskan nama tier (misal `"Platinum"`, `"Gold"`) dengan varian `warning`/`primary` jika `item.isPriorityMember === true && item.memberTierName`.
- Pasien non-member atau member expired tidak memunculkan lencana membership.
- Data antrean tersaji secara urut: Pasien CITO berada di posisi teratas, disusul Member Prioritas sesuai level, lalu Reguler.
- Tampilan tabel tetap rapi dan tidak tumpang tindih pada layar mobile maupun desktop.

---

### FE-RWI-269 — Visualisasi Lencana pada Header Langkah Admisi PPRI & Isolasi Privasi Denah Bangsal

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Header ringkasan pasien pada langkah admisi transfer (`InpatientAdmissionTransferReviewStep.jsx`) menampilkan lencana CITO dan Tier Membership, proses penempatan bed tetap manual melalui denah, dan denah bangsal umum terbukti bebas dari lencana tier demi privasi |
| File Terdampak | `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-review-step.jsx` |
| Jejak Requirement | `FR-TRF-010`, `FR-TRF-011`; `RWI-DEC-338`, `339`, `341`; `RWI-AC-487`, `488`, `489` |
| Kontrak Target | `0.17.0`; Bab 20 Frontend Architecture, Bab 31 PRD ke MVP |
| Reuse | `InpatientAdmissionTransferReviewStep`, `StatusBadge` |
| Dependency | `FE-RWI-268` |
| Verifikasi yang Wajib | `V-FE` (`npm run lint`, `npm run build`) + uji visual header wizard admisi transfer & audit privasi denah bangsal |
| Definition of Done | `D-FE`; laporan `task/report/frontend/FE-RWI-269.md` |

**Kriteria Penerimaan**:
- Panel ringkasan pasien di bagian atas `InpatientAdmissionTransferReviewStep.jsx` menampilkan lencana CITO dan lencana Tier Membership berdampingan dengan Nama, No RM, dan Asal Unit (IGD).
- Tombol aksi penempatan kamar dan tempat tidur tetap membuka modal Denah Bed Management existing secara sadar dan manual (tanpa auto-booking sistem).
- Denah Bed Management bangsal umum (`bed-management-view.jsx`, `floor-plan-room-popup.jsx`, `floor-plan-bed-row.jsx`) tidak menampilkan lencana tier membership pasien yang sedang dirawat (`RWI-AC-488`).
