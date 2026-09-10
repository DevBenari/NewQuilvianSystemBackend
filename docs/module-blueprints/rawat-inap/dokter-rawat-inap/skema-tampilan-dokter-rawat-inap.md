# Skema Tampilan Frontend — Dokter Rawat Inap

## 1. Identitas Dokumen

| Field | Nilai |
|---|---|
| Modul | Rawat Inap |
| Submodul | Dokter Rawat Inap |
| Backend Blueprint | `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap` |
| Sumber utama | `roadmap/frontend-roadmap.md` dan `03-frontend-architecture.md` |
| Frontend target | `QuilvianSystemFrontendDev` branch `HamzahV2` |
| Scope | Rancangan tampilan frontend |
| Catatan | Dokumen ini fokus pada skema UI/UX dan composition component. Detail menu sidebar mengikuti implementasi menu existing dan keputusan operasional tim. |

---

# 2. Prinsip Rancangan

Ruang kerja Dokter Rawat Inap harus berbasis **episode rawat inap dan satu pasien yang konteksnya sudah pasti**, bukan berbasis antrean dokter rawat jalan.

Rancangan utama:

```text
Daftar Pasien / Entry Point
        ↓
Pilih Pasien
        ↓
Episode Rawat Inap
        ↓
Ruang Kerja Dokter
        │
        ├── Kajian Medis
        ├── Catatan Perkembangan
        ├── Catatan Terpadu
        ├── Visite
        ├── Resep & Tindakan
        └── Penunjang
```

Prinsip wajib:

1. Satu workspace hanya menangani **satu pasien dan satu episode aktif**.
2. Identitas pasien dan episode selalu terlihat sebelum dokter melakukan aksi klinis.
3. Kegagalan memuat konteks pasien harus menonaktifkan seluruh aksi tulis.
4. Kegagalan memuat alergi harus terlihat jelas.
5. Tidak ada lagi konsep aksi antrean seperti:
   - Panggil.
   - Lewati.
   - Tidak Hadir.
6. Catatan final tidak diedit langsung.
7. Koreksi dokumen final dilakukan melalui mekanisme koreksi/addendum.
8. Visite merupakan event tersendiri, bukan diturunkan dari SOAP/CPPT.
9. Status Farmasi, Laboratorium, Radiologi, dan Billing ditampilkan sebagai informasi dari modul pemiliknya.
10. Workspace tidak menyediakan aksi yang mengambil alih ownership modul lain.

---

# 3. Skema Tampilan Utama

## 3.1 Physician Workspace

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ ← Kembali                              DOKTER RAWAT INAP                    │
│                                        Dokumentasi klinis pasien            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                          PATIENT SAFETY CONTEXT                             │
│                                                                             │
│  Tn. Budi Santoso                   No. RM : 00123456                       │
│  EP-2026-0912                       Hari Rawat : 3                          │
│  Melati / Kamar 302 / Bed B         DPJP : dr. Andi Saputra                │
│                                                                             │
│  Diagnosis Kerja : Pneumonia                                                │
│                                                                             │
│  ⚠ ALERGI : AMOXICILLIN                                                     │
│                                                                             │
│  ✓ Anda berwenang melakukan dokumentasi pada episode ini                   │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│ Kajian Medis │ Catatan │ Catatan Terpadu │ Visite │ Resep & Tindakan │    │
│ Penunjang                                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                         KONTEN TAB AKTIF                                     │
│                                                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 4. Komponen Frontend

## 4.1 Base Component Existing yang Direuse

Base component existing pada:

```text
src/components/ui/doctor-clinical-base/
```

Komponen yang dapat dipakai:

| Component | Keputusan | Kegunaan |
|---|---|---|
| `ClinicalPageHeader` | REUSE | Header halaman |
| `ClinicalSummaryBar` | REUSE SELECTIVE | Monitoring / summary bila dibutuhkan |
| `ClinicalStatusBadge` | REUSE | Status Draft, Final, Pending, Verified, Cancelled |
| `ClinicalContextBar` | REUSE + ADAPTER | Dasar informasi dokter/pasien/episode |
| `ClinicalTabNav` | REUSE | Navigasi tab utama |
| `ClinicalSectionPanel` | REUSE | Wrapper section |
| `ClinicalDataTable` | REUSE | Resep, tindakan, lab, radiologi |
| `ClinicalEmptyState` | REUSE | Empty/loading/error state |
| `ClinicalPatientCard` | TIDAK DIREUSE AS-IS | Masih membawa konsep `queueCode` / antrean |

---

## 4.2 Base Component Baru yang Direkomendasikan

Tambahkan base component baru yang tetap generik dan tidak mengandung domain rawat inap secara langsung.

```text
src/components/ui/doctor-clinical-base/
```

### 1. `ClinicalSafetyAlert`

Digunakan untuk:

- Alergi.
- Warning patient context.
- Warning data gagal.
- Warning hasil belum final.
- Warning visite berdekatan.

Contoh:

```text
⚠ ALERGI: Amoxicillin
```

atau:

```text
⚠ Riwayat alergi tidak dapat dimuat.
```

---

### 2. `ClinicalStateBoundary`

Standard wrapper untuk:

- Loading.
- Error.
- Retry.
- Read-only.
- Permission denied.

Contoh:

```text
┌────────────────────────────────────────────┐
│ ⚠ Data pasien tidak dapat dimuat          │
│                                            │
│ Identitas episode belum dapat diverifikasi │
│                                            │
│ [ Coba Lagi ]                              │
└────────────────────────────────────────────┘
```

---

### 3. `ClinicalTimeline`

Digunakan bersama:

```text
ClinicalTimelineItem
```

Untuk:

- SOAP.
- CPPT.
- Visite.
- Koreksi/addendum.

---

### 4. `ClinicalTimelineItem`

Menampilkan:

- Waktu klinis.
- Penulis.
- Profesi.
- Status.
- Isi ringkas.
- Verifikator.
- Koreksi.
- Cancellation.

---

### 5. `ClinicalDocumentMeta`

Menampilkan metadata dokumen:

```text
Penulis      : dr. Andi
Profesi      : Dokter
Waktu Klinis : 07 Sep 2026 07:40
Dicatat      : 07 Sep 2026 11:03
Status       : Final
```

---

### 6. `ClinicalValidationSummary`

Digunakan untuk menampilkan field yang belum lengkap.

Contoh:

```text
Dokumen belum dapat diselesaikan.

Bagian yang belum lengkap:
- Pemeriksaan fisik
- Diagnosis kerja
- Rencana terapi
```

---

### 7. `ClinicalCompletionBar`

Digunakan pada dokumen yang mempunyai Draft → Completed.

```text
┌───────────────────────────────────────────────────────────────────────┐
│ ⚠ Setelah diselesaikan, dokumen akan dikunci dan tidak dapat diedit. │
│                                        [Simpan Draft] [Selesaikan]   │
└───────────────────────────────────────────────────────────────────────┘
```

---

### 8. `ClinicalAuditBadge`

Contoh:

```text
DIKOREKSI
DIVERIFIKASI
DIBATALKAN
TERLAMBAT
```

---

### 9. `ClinicalSegmentedNav`

Digunakan untuk navigasi internal:

```text
[ RESEP ] [ TINDAKAN ]
```

atau:

```text
[ LABORATORIUM ] [ RADIOLOGI ]
```

---

### 10. `ClinicalActionGuard`

Wrapper visual untuk mengatur:

- Hide action.
- Disable action.
- Permission explanation.
- Authority check.

Contoh:

```text
Pengguna tidak berwenang melakukan verifikasi.
DPJP aktif: dr. Andi Saputra
```

---

# 5. Struktur Komponen Domain

Direkomendasikan struktur:

```text
src/components/view/
└── health-services/
    └── inpatient-management/
        └── physician-workspace/
            │
            ├── physician-workspace-client.jsx
            ├── physician-workspace-view.jsx
            │
            ├── components/
            │   ├── inpatient-episode-header.jsx
            │   ├── physician-authority-indicator.jsx
            │   ├── inpatient-allergy-alert.jsx
            │   └── physician-workspace-tabs.jsx
            │
            ├── tabs/
            │   ├── assessment/
            │   │   └── medical-assessment-tab.jsx
            │   │
            │   ├── progress-note/
            │   │   └── physician-progress-tab.jsx
            │   │
            │   ├── integrated-note/
            │   │   └── integrated-progress-note-tab.jsx
            │   │
            │   ├── physician-visit/
            │   │   └── physician-visit-tab.jsx
            │   │
            │   ├── medication-procedure/
            │   │   └── prescription-procedure-tab.jsx
            │   │
            │   └── supporting-service/
            │       └── supporting-service-tab.jsx
            │
            └── modals/
                ├── complete-document-modal.jsx
                ├── correction-modal.jsx
                ├── record-visit-modal.jsx
                └── cancel-visit-modal.jsx
```

---

# 6. FE-RWI-043 — Ruang Kerja Dokter

## Tujuan

Membentuk shell utama dokter rawat inap berbasis episode pasien.

## Layout

```text
┌──────────────────────────────────────────────────────────────────────┐
│ ← Kembali                           DOKTER RAWAT INAP                 │
│                                     Episode Aktif                    │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ Tn. Budi Santoso                                                     │
│ No RM 00123456      EP-2026-0912      Hari Rawat : 3                │
│ Melati 302 / B      DPJP dr. Andi                                   │
│                                                                      │
│ Diagnosis : Pneumonia                                                │
│                                                                      │
│ ⚠ ALERGI AMOXICILLIN                                                 │
│                                                                      │
│ ✓ Anda berwenang melakukan dokumentasi                              │
│                                                                      │
├──────────────────────────────────────────────────────────────────────┤
│ Kajian │ Catatan │ Terpadu │ Visite │ Resep & Tindakan │ Penunjang │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│                           ACTIVE TAB                                 │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

## Component Composition

```text
ClinicalPageHeader
        ↓
InpatientEpisodeHeader
        ↓
ClinicalContextBar
        ↓
ClinicalSafetyAlert
        ↓
PhysicianAuthorityIndicator
        ↓
ClinicalTabNav
        ↓
TabContent
```

---

## Context Failure

```text
┌──────────────────────────────────────────────────────────────────────┐
│ ⚠ DATA PASIEN TIDAK DAPAT DIMUAT                                   │
│                                                                      │
│ Identitas pasien dan episode belum dapat diverifikasi.              │
│                                                                      │
│ [ Coba Lagi ]                                                        │
└──────────────────────────────────────────────────────────────────────┘
```

Dalam state ini:

```text
Kajian Medis       DISABLED
Catatan            DISABLED
Visite             DISABLED
Resep              DISABLED
Tindakan           DISABLED
Penunjang          DISABLED
```

---

## Allergy Failure

Tidak boleh dianggap sama dengan tidak ada alergi.

```text
⚠ RIWAYAT ALERGI TIDAK DAPAT DIMUAT

Data alergi belum dapat diverifikasi.
```

---

# 7. FE-RWI-044 — Kajian Medis Awal

## Layout

Direkomendasikan layout desktop **70% / 30%**.

```text
┌──────────────────────────────────────────────┬────────────────────────┐
│ KAJIAN MEDIS AWAL                           │ REFERENSI KEPERAWATAN  │
│                                              │ READ ONLY              │
│ Anamnesis                                    │                        │
│ ┌──────────────────────────────────────────┐ │ Tanda Vital            │
│ │                                          │ │ TD : 120/80            │
│ └──────────────────────────────────────────┘ │ Nadi : 82              │
│                                              │ SpO2 : 98%             │
│ Pemeriksaan Fisik                            │                        │
│ ┌──────────────────────────────────────────┐ │ Keluhan Utama          │
│ │                                          │ │ ...                    │
│ └──────────────────────────────────────────┘ │                        │
│                                              │ Risiko Jatuh           │
│ Assessment                                   │ ...                    │
│ ┌──────────────────────────────────────────┐ │                        │
│ │                                          │ │                        │
│ └──────────────────────────────────────────┘ │                        │
│                                              │                        │
│ Planning                                     │                        │
│ ┌──────────────────────────────────────────┐ │                        │
│ │                                          │ │                        │
│ └──────────────────────────────────────────┘ │                        │
│                                              │                        │
│ Diagnosis / Problem List                     │                        │
│ [+ Tambah Diagnosis]                         │                        │
├──────────────────────────────────────────────┴────────────────────────┤
│ ⚠ Setelah diselesaikan dokumen akan dikunci.                         │
│                                      [Simpan Draft] [Selesaikan]    │
└──────────────────────────────────────────────────────────────────────┘
```

## Rule Tampilan

Pengkajian keperawatan:

- Hanya baca.
- Bukan bagian dari Kajian Medis.
- Dibedakan secara visual.
- Tidak menjadi blocker bila belum tersedia.

Empty state:

```text
Pengkajian keperawatan belum tersedia.
Kajian medis tetap dapat dilanjutkan.
```

---

# 8. FE-RWI-045 — Catatan Perkembangan

## Layout

Timeline kiri + editor kanan.

```text
┌───────────────────────┬──────────────────────────────────────────────┐
│ RIWAYAT CATATAN       │ CATATAN PERKEMBANGAN                       │
│                       │                                              │
│ 07 Sep 07:40          │ Waktu Pemeriksaan                           │
│ dr. Andi              │ [07 Sep 2026 07:40]                         │
│ ● Final               │                                              │
│                       │ SUBJECTIVE                                   │
│ 06 Sep 17:20          │ ┌──────────────────────────────────────────┐ │
│ dr. Andi              │ │                                          │ │
│ ● Dikoreksi           │ └──────────────────────────────────────────┘ │
│                       │                                              │
│ 06 Sep 08:00          │ OBJECTIVE                                    │
│ dr. Sinta             │ ┌──────────────────────────────────────────┐ │
│ ● Final               │ │                                          │ │
│                       │ └──────────────────────────────────────────┘ │
│                       │                                              │
│                       │ ASSESSMENT                                   │
│                       │ ┌──────────────────────────────────────────┐ │
│                       │ │                                          │ │
│                       │ └──────────────────────────────────────────┘ │
│                       │                                              │
│                       │ PLAN                                         │
│                       │ ┌──────────────────────────────────────────┐ │
│                       │ │                                          │ │
│                       │ └──────────────────────────────────────────┘ │
├───────────────────────┴──────────────────────────────────────────────┤
│ ⚠ Selesaikan = tanda tangan dan dokumen dikunci                    │
│                                      [Simpan Draft] [Selesaikan]   │
└──────────────────────────────────────────────────────────────────────┘
```

## Setelah Final

Tidak ada:

```text
[Sunting]
```

Yang tersedia:

```text
[Koreksi]
```

jika user berwenang.

---

## Tampilan Koreksi

```text
CATATAN ASLI
Penulis : dr. Andi
Waktu   : 07 Sep 07:40

────────────────────────────────────────

KOREKSI #1
Dikoreksi oleh : dr. Budi
Sebagai         : Dokter Pengganti
Alasan          : Koreksi dosis terapi
Waktu           : 07 Sep 13:20
```

Penulis asli tetap menjadi penulis dokumen utama.

---

# 9. FE-RWI-046 — Catatan Terpadu / CPPT

## Layout

```text
┌──────────────────────────────────────────────────────────────────────┐
│ CATATAN TERPADU                                      [Filter Profesi]│
├──────────────────────────────────────────────────────────────────────┤
│ 07 Sep 10:24                                                        │
│ Ns. Sari • Perawat                                                  │
│                                                                      │
│ S : ...                                                              │
│ O : ...                                                              │
│ A : ...                                                              │
│ P : ...                                                              │
│                                                                      │
│ Status : MENUNGGU VERIFIKASI                       [Verifikasi]      │
├──────────────────────────────────────────────────────────────────────┤
│ 07 Sep 08:12                                                        │
│ dr. Andi • Dokter                                                   │
│                                                                      │
│ S : ...                                                              │
│ O : ...                                                              │
│ A : ...                                                              │
│ P : ...                                                              │
│                                                                      │
│ ✓ DIVERIFIKASI                                                      │
│ Verifikator : dr. Andi                                              │
│ Waktu       : 07 Sep 09:30                                          │
└──────────────────────────────────────────────────────────────────────┘
```

## Rule Tampilan

Wajib tampil terpisah:

```text
Penulis      : Ns. Sari
Verifikator  : dr. Andi
```

Bukan:

```text
Dokter : dr. Andi
```

yang menggantikan penulis asli.

---

## Policy Verification Tidak Aktif

```text
○ VERIFIKASI TIDAK DIWAJIBKAN

Tidak ada kebijakan aktif yang mewajibkan verifikasi DPJP.
```

---

# 10. FE-RWI-047 — Riwayat Visite

## Layout

Gunakan event timeline.

```text
┌──────────────────────────────────────────────────────────────────────┐
│ RIWAYAT VISITE                                      [+ Catat Visite]│
├──────────────────────────────────────────────────────────────────────┤
│ ● 07 Sep 07:40                                                     │
│   dr. Andi — DPJP                                                   │
│   Visite pagi                                                       │
│   Tertaut : SOAP 07:50                                              │
│                                                      [Batalkan]      │
│                                                                      │
│ ● 06 Sep 16:10                                                     │
│   dr. Andi — DPJP                                                   │
│   Tidak ditautkan                                                   │
│                                                      [Batalkan]      │
│                                                                      │
│ × 06 Sep 08:05   DIBATALKAN                                        │
│   dr. Andi                                                          │
│   Alasan : Salah input jam                                          │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

Tidak ada:

```text
[Edit]
```

---

## Modal Catat Visite

```text
┌─────────────────────────────────────────────┐
│ CATAT VISITE                                │
├─────────────────────────────────────────────┤
│                                             │
│ Waktu Visite                               │
│ [ 07 Sep 2026 07:40 ]                      │
│                                             │
│ Peran                                      │
│ [ DPJP ▼ ]                                 │
│                                             │
│ Catatan                                    │
│ [.......................................]  │
│                                             │
│ Tautkan Dokumen                            │
│ [ Opsional ▼ ]                             │
│                                             │
│ ⚠ Visite lain tercatat pukul 07:20.        │
│   Anda tetap dapat melanjutkan.            │
│                                             │
│                     [Batal] [Catat Visite] │
└─────────────────────────────────────────────┘
```

Visite berdekatan:

- Warning.
- Bukan blocker.

---

## Modal Batalkan Visite

```text
┌─────────────────────────────────────────────┐
│ BATALKAN VISITE                             │
├─────────────────────────────────────────────┤
│                                             │
│ Alasan pembatalan *                        │
│ [.......................................]  │
│                                             │
│                [Kembali] [Batalkan Visite] │
└─────────────────────────────────────────────┘
```

Tombol submit disabled bila alasan kosong.

---

# 11. FE-RWI-048 — Resep dan Tindakan

Gunakan satu tab utama:

```text
RESEP & TINDAKAN
```

dengan segmented navigation:

```text
[ RESEP ] [ TINDAKAN ]
```

---

## 11.1 Resep

```text
┌──────────────────────────────────────────────────────────────────────┐
│ RESEP                                             [+ Buat Resep]     │
├────────────┬───────────┬──────────────┬────────────┬─────────────────┤
│ Tanggal    │ Jenis     │ Dokter       │ Status     │ Farmasi         │
├────────────┼───────────┼──────────────┼────────────┼─────────────────┤
│ 07 Sep     │ Harian    │ dr. Andi     │ Aktif      │ Diproses        │
│ 06 Sep     │ Rutin     │ dr. Andi     │ Selesai    │ Diserahkan      │
│ 05 Sep     │ Pulang    │ dr. Andi     │ Aktif      │ Menunggu        │
└────────────┴───────────┴──────────────┴────────────┴─────────────────┘
```

Status Farmasi:

```text
READ ONLY
```

Tidak ada:

```text
[Tandai Diserahkan]
```

---

## 11.2 Tindakan

```text
┌──────────────────────────────────────────────────────────────────────┐
│ TINDAKAN                                      [+ Tambah Tindakan]    │
├──────────────┬────────────┬─────────────┬────────────────────────────┤
│ Tindakan     │ Status     │ Dokter      │ Billing                    │
├──────────────┼────────────┼─────────────┼────────────────────────────┤
│ Nebulisasi   │ Completed  │ dr. Andi    │ ✓ Terkirim                │
│ Debridement  │ Completed  │ dr. Andi    │ ⚠ Gagal dikirim           │
└──────────────┴────────────┴─────────────┴────────────────────────────┘
```

Kegagalan Billing:

```text
⚠ Gagal dikirim
[Coba Lagi]
```

ditampilkan pada row.

Tidak menjadi full-page error.

---

# 12. FE-RWI-049 — Pemeriksaan Penunjang

Gunakan dua section terpisah:

```text
LABORATORIUM
RADIOLOGI
```

Tidak dicampur dalam satu list.

---

## 12.1 Laboratorium

```text
┌──────────────────────────────────────────────────────────────────────┐
│ LABORATORIUM                                       [+ Pesan Lab]     │
├──────────────────────────────────────────────────────────────────────┤
│ Darah Lengkap        07 Sep      FINAL                              │
│ Gula Darah           07 Sep      ⚠ BELUM FINAL                     │
│ Elektrolit           06 Sep      FINAL                              │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 12.2 Radiologi

```text
┌──────────────────────────────────────────────────────────────────────┐
│ RADIOLOGI                                      [+ Pesan Radiologi]   │
├──────────────────────────────────────────────────────────────────────┤
│ Thorax PA       X-Ray       07 Sep 10:00      FINAL                 │
│ CT Thorax       CT          08 Sep 09:00      TERJADWAL             │
│ USG Abdomen     USG         06 Sep 11:00      ⚠ BELUM FINAL        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Result State

### Final

```text
✓ HASIL FINAL
```

### Belum Final

```text
⚠ HASIL BELUM FINAL
```

Keduanya harus dibedakan secara visual.

Workspace tidak menyediakan:

```text
[Input Hasil]
```

---

# 13. FE-RWI-050 — Daftar Pantau Verifikasi

Tampilan ditambahkan ke layar monitoring/daftar pantau existing.

```text
┌──────────────────────────────────────────────────────────────────────┐
│ VERIFIKASI CATATAN TERPADU                              4 tertunda  │
├─────────────┬─────────────┬───────────┬───────────┬──────────────────┤
│ Pasien      │ Penulis     │ Profesi   │ Terlambat │ Aksi             │
├─────────────┼─────────────┼───────────┼───────────┼──────────────────┤
│ Tn. Budi    │ Ns. Sari    │ Perawat   │ 3 jam     │ [Buka Catatan]  │
│ Ny. Ani     │ dr. Rudi    │ Dokter    │ 1 jam     │ [Buka Catatan]  │
└─────────────┴─────────────┴───────────┴───────────┴──────────────────┘
```

Jangan tampilkan isi klinis.

Yang boleh tampil:

- Nama pasien.
- Penulis.
- Profesi.
- Keterlambatan.
- Status.
- Aksi buka catatan.

---

## Monitoring State

### Semua Selesai

```text
✓ Semua catatan sudah terverifikasi.
```

### Policy Tidak Aktif

```text
○ Verifikasi DPJP tidak diwajibkan.
Tidak ada catatan yang perlu dipantau.
```

### Error

```text
⚠ Data verifikasi tidak dapat dimuat.

[ Coba Lagi ]
```

Ketiga state tersebut harus dibedakan.

---

# 14. Tab Workspace Final

Tab utama:

```text
[Kajian Medis]
[Catatan Perkembangan]
[Catatan Terpadu]
[Visite]
[Resep & Tindakan]
[Penunjang]
```

Mapping dari source existing:

| Existing | Target |
|---|---|
| Screening | Kajian Medis |
| SOAP | Catatan Perkembangan |
| CPPT | Catatan Terpadu |
| Prescription | Resep & Tindakan |
| Procedure | Resep & Tindakan |
| Certificate | Keluar dari scope workspace MVP ini |
| — | Visite |
| — | Penunjang |

---

# 15. Komponen yang Harus Dihilangkan dari Workspace Existing

Workspace existing masih berbasis antrean dokter.

Hal yang harus dilepas:

```text
useDoctorQueue
useDoctorQueueBoard
useInfiniteQueueScroll
useDoctorConsultationWorkspace yang bergantung antrean
queueCode
call action
skip action
no-show action
queue timer
queue call lock
```

UI yang harus dihilangkan:

```text
[Panggil]
[Lewati]
[Tidak Hadir]
```

Konsep:

```text
Daftar antrean pasien kiri
+
Workspace konsultasi kanan
```

diganti menjadi:

```text
Satu Episode
+
Satu Pasien
+
Satu Physician Workspace
```

---

# 16. Global Finalize Consultation

Source existing mempunyai konsep:

```text
Simpan Konsultasi / Visite

[Simpan Konsultasi]
```

Rancangan baru tidak menggunakan satu tombol global untuk menyelesaikan seluruh proses.

Lifecycle mengikuti tiap domain:

```text
Kajian Medis
Draft
  ↓
Completed


Catatan Perkembangan
Draft
  ↓
Completed
  ↓
Correction / Addendum


Catatan Terpadu
Written
  ↓
Pending Verification
  ↓
Verified


Visite
Recorded
  ↓
Cancelled


Resep
Per prescription


Tindakan
Per procedure
```

Karena itu:

```text
Global "Simpan Konsultasi"
```

direkomendasikan dihapus.

---

# 17. State Handling Standard

Semua layar menggunakan standard state berikut.

## Loading

Gunakan skeleton / row placeholder.

```text
Memuat...
████████████████
████████████
██████████████████
```

---

## Empty

Harus menyatakan arti business yang sebenarnya.

Contoh:

```text
Belum ada visite tercatat.
```

bukan:

```text
Tidak ada data.
```

---

## Error

```text
Data tidak dapat dimuat.

[ Coba Lagi ]
```

---

## Permission

Action yang benar-benar tidak boleh digunakan:

```text
HIDDEN
```

bukan selalu disabled.

Contoh:

Dokter jaga tidak boleh verify CPPT:

```text
[Verifikasi]
```

tidak ditampilkan.

---

## Episode Closed

Workspace berubah menjadi read-only.

```text
EPISODE TELAH DITUTUP

Dokumentasi baru tidak dapat dibuat.
Dokumen final yang memiliki kewenangan koreksi tetap dapat dikoreksi.
```

---

## Duplicate Submission

Tombol menjadi disabled selama request.

Contoh:

```text
[ Mencatat Visite... ]
```

untuk mencegah double submit.

---

# 18. Responsive Behaviour

## Desktop

```text
≥ 1200 px
```

- Full patient context.
- Horizontal tab navigation.
- Assessment dapat memakai layout 70/30.
- SOAP dapat memakai timeline + editor dua kolom.

---

## Tablet

```text
768 – 1199 px
```

- Context berubah menjadi 2 kolom.
- Tab dapat horizontal-scroll.
- Timeline + editor dapat menjadi 35/65.

---

## Mobile

```text
< 768 px
```

Gunakan single column.

Contoh Kajian Medis:

```text
Kajian Medis
↓
Form
↓
Referensi Keperawatan
↓
Completion Bar
```

Timeline:

```text
Timeline
↓
Pilih Catatan
↓
Detail / Editor
```

Patient safety context tetap berada di bagian atas.

---

# 19. Visual Hierarchy

Urutan prioritas visual:

```text
1. Identitas pasien
2. Episode rawat inap
3. Alergi / safety alert
4. Authority pengguna
5. Status dokumen
6. Konten klinis
7. Secondary metadata
```

Jangan membuat diagnosis, metadata administratif, atau decorative icon lebih dominan daripada identitas dan alergi pasien.

---

# 20. Component Composition Final

```text
PhysicianWorkspacePage
│
├── ClinicalPageHeader
│
├── InpatientEpisodeHeader
│   ├── ClinicalContextBar
│   ├── ClinicalSafetyAlert
│   └── PhysicianAuthorityIndicator
│
├── ClinicalTabNav
│
└── TabContent
    │
    ├── MedicalAssessmentTab
    │   ├── ClinicalSectionPanel
    │   ├── ClinicalValidationSummary
    │   └── ClinicalCompletionBar
    │
    ├── PhysicianProgressTab
    │   ├── ClinicalTimeline
    │   ├── ClinicalTimelineItem
    │   ├── ClinicalDocumentMeta
    │   ├── ClinicalAuditBadge
    │   └── ClinicalCompletionBar
    │
    ├── IntegratedProgressNoteTab
    │   ├── ClinicalTimeline
    │   ├── ClinicalTimelineItem
    │   └── ClinicalAuditBadge
    │
    ├── PhysicianVisitTab
    │   ├── ClinicalTimeline
    │   ├── ClinicalTimelineItem
    │   ├── RecordVisitModal
    │   └── CancelVisitModal
    │
    ├── PrescriptionProcedureTab
    │   ├── ClinicalSegmentedNav
    │   ├── ClinicalDataTable
    │   └── ClinicalStatusBadge
    │
    └── SupportingServiceTab
        ├── ClinicalSegmentedNav
        ├── ClinicalSectionPanel
        ├── ClinicalDataTable
        └── ClinicalStatusBadge
```

---

# 21. Matriks Task dan Skema Tampilan

| Task | Tampilan |
|---|---|
| `FE-RWI-042` | Entry point / navigasi menuju konteks pasien rawat inap |
| `FE-RWI-043` | Physician Workspace + Patient Safety Context |
| `FE-RWI-044` | Kajian Medis Awal |
| `FE-RWI-045` | Catatan Perkembangan / SOAP Timeline |
| `FE-RWI-046` | Catatan Terpadu / CPPT + Verifikasi |
| `FE-RWI-047` | Riwayat Visite |
| `FE-RWI-048` | Resep & Tindakan |
| `FE-RWI-049` | Laboratorium & Radiologi |
| `FE-RWI-050` | Monitoring Verifikasi |

---

# 22. Acceptance Visual Minimum

Implementasi dianggap memenuhi rancangan tampilan bila:

1. Workspace tidak lagi menampilkan aksi antrean.
2. Satu workspace hanya menampilkan satu episode pasien.
3. Patient safety context selalu berada di atas tab.
4. Alergi terlihat jelas.
5. Context failure menonaktifkan semua write action.
6. Kajian Medis dan Pengkajian Keperawatan terpisah secara visual.
7. SOAP menampilkan waktu klinis.
8. Catatan final tidak mempunyai tombol Edit.
9. Koreksi menampilkan penulis asli dan penulis koreksi secara terpisah.
10. CPPT menampilkan penulis dan verifikator secara terpisah.
11. Cancelled visite tetap terlihat.
12. Visite tidak mempunyai tombol Edit.
13. Status Farmasi bersifat read-only.
14. Kegagalan Billing tampil pada row, bukan full-page error.
15. Lab dan Radiologi terpisah jelas.
16. Hasil non-final tidak terlihat seperti hasil final.
17. Monitoring tidak menampilkan isi klinis.
18. Empty, policy-disabled, dan error state dapat dibedakan.
19. Tidak ada global finalize consultation yang mencampur lifecycle seluruh dokumen.
20. Layout tetap usable pada desktop, tablet, dan mobile.

---

# 23. Kesimpulan

Target frontend Dokter Rawat Inap bukan membuat ulang seluruh komponen klinis, tetapi:

```text
REUSE
doctor-clinical-base
        +
ADAPTER
episode-based context
        +
NEW BASE COMPONENT
timeline / safety / audit / completion
        +
NEW DOMAIN COMPONENT
kajian / visite / penunjang
```

Skema akhir:

```text
PASIEN RAWAT INAP
        ↓
EPISODE AKTIF
        ↓
PATIENT SAFETY CONTEXT
        ↓
PHYSICIAN WORKSPACE
        │
        ├── KAJIAN MEDIS
        ├── CATATAN PERKEMBANGAN
        ├── CATATAN TERPADU
        ├── VISITE
        ├── RESEP & TINDAKAN
        └── PENUNJANG
```

Dengan rancangan ini developer tidak perlu menebak:

- bentuk workspace;
- base component yang digunakan;
- base component yang perlu dibuat;
- komponen domain;
- state loading/error/empty;
- behaviour dokumen final;
- behaviour visite;
- struktur resep/tindakan;
- struktur Lab/Radiologi;
- serta hubungan antara tiap task frontend.
