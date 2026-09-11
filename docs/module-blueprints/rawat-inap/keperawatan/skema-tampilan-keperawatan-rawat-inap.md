# Skema Tampilan Frontend — Keperawatan Rawat Inap

## 1. Tujuan

Dokumen ini menjadi rancangan tampilan frontend untuk submodul **Keperawatan Rawat Inap**.

Prinsip utama:

- layout mengikuti pola halaman Pengkajian IGD existing;
- struktur layout diseragamkan melalui shared clinical workspace;
- Rawat Inap Keperawatan menjadi reference implementation yang lebih rapi;
- setelah stabil, IGD dapat direfactor mengikuti shared base yang sama;
- kebutuhan bisnis IGD dan Rawat Inap tetap berbeda walaupun layout dasarnya sama.

---

# 2. Arsitektur Layout

Pola utama:

```text
Patient Information Header
        +
Left Section Navigation
        +
Main Clinical Content
        +
Right Quick Summary / Context Panel
```

Skema:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ INFORMASI PASIEN                                                            │
├──────────────────┬───────────────────────────────────────┬──────────────────┤
│                  │                                       │                  │
│ Section Menu     │ Main Clinical Content                 │ Quick Summary    │
│                  │                                       │                  │
│                  │                                       ├──────────────────┤
│                  │                                       │ Context / Alert  │
│                  │                                       │                  │
└──────────────────┴───────────────────────────────────────┴──────────────────┘
```

---

# 3. Strategi Shared Base Component

Jangan fixing seluruh IGD terlebih dahulu.

Gunakan urutan:

```text
IGD EXISTING
   ↓
ambil pola layout
   ↓
SHARED CLINICAL WORKSPACE BASE
   ↓
RAWAT INAP KEPERAWATAN
   ↓
reference implementation
   ↓
REFACTOR IGD
```

Direkomendasikan membuat:

```text
src/components/ui/
└── clinical-workspace/
    ├── ClinicalWorkspaceShell.jsx
    ├── PatientContextHeader.jsx
    ├── ClinicalSectionNav.jsx
    ├── ClinicalContentPanel.jsx
    ├── ClinicalQuickSummary.jsx
    ├── ClinicalInfoPanel.jsx
    ├── ClinicalStateBoundary.jsx
    ├── ClinicalSafetyAlert.jsx
    ├── ClinicalStatusBadge.jsx
    └── index.js
```

Komponen lanjutan:

```text
ClinicalTimeline
ClinicalTimelineItem
ClinicalDocumentMeta
ClinicalDeadlineBadge
ClinicalCompletionBar
ClinicalAddendumList
ClinicalAddendumItem
ClinicalRevisionHistory
ClinicalValidationSummary
ClinicalActionGuard
```

---

# 4. Informasi Pasien

Informasi pasien harus lebih rapi dari layout IGD existing.

Target:

```text
┌────────────────────────────────────────────────────────────────────────────┐
│ INFORMASI PASIEN                                                           │
│                                                                            │
│ Tn. Budi Santoso                                   Laki-laki • 54 tahun    │
│ RM 00123456                                        EP-2026-0912            │
│                                                                            │
│ MELATI • Kamar 302 • Bed B                         Hari Rawat ke-3         │
│ DPJP : dr. Andi Saputra                            Perawat PJ : Ns. Sari   │
│                                                                            │
│ ⚠ Alergi : Amoxicillin                             ● Sedang Dirawat        │
└────────────────────────────────────────────────────────────────────────────┘
```

Prioritas visual:

```text
1. Nama pasien
2. No. RM
3. Episode
4. Lokasi / kamar / bed
5. Hari rawat
6. DPJP
7. Perawat penanggung jawab
8. Alergi
9. Status episode
```

---

# 5. Ruang Kerja Keperawatan — FE-RWI-051

Skema utama:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ ← Kembali                              KEPERAWATAN RAWAT INAP              │
├─────────────────────────────────────────────────────────────────────────────┤
│ INFORMASI PASIEN                                                            │
│                                                                             │
│ Tn. Budi Santoso     RM 00123456       EP-2026-0912       Hari Rawat 3     │
│ Melati / 302 / B     DPJP dr. Andi     Perawat PJ Ns. Sari                 │
│ ⚠ Alergi Amoxicillin                                      ● Dirawat        │
├──────────────────┬───────────────────────────────────────┬──────────────────┤
│                  │                                       │ RINGKASAN CEPAT  │
│ Pengkajian       │ PENGKAJIAN KEPERAWATAN               │                  │
│                  │                                       │ Pengkajian   3   │
│ Rencana Asuhan   │                                       │ Masalah Aktif 4  │
│                  │                                       │ Tindakan      8  │
│ Tindakan         │                                       │ Koreksi       1  │
│ Keperawatan      │                                       │                  │
│                  │                                       ├──────────────────┤
│ Lini Masa        │                                       │ STATUS           │
│                  │                                       │ Episode Aktif    │
└──────────────────┴───────────────────────────────────────┴──────────────────┘
```

Section utama:

```text
Pengkajian
Rencana Asuhan
Tindakan Keperawatan
Lini Masa
```

---

# 6. Clinical Quick Summary

Panel kanan dibuat configurable.

Contoh Rawat Inap:

```text
RINGKASAN CEPAT

Pengkajian                 3
Masalah Aktif              4
Tindakan Hari Ini          8
Koreksi                    1
Pengkajian Terlambat       0
```

Panel kanan bawah dapat digunakan untuk:

```text
STATUS PENGKAJIAN

Pengkajian Awal
⚠ 2 jam tersisa

Episode Aktif
```

atau:

```text
PERINGATAN

⚠ Risiko jatuh tinggi
⚠ Pengkajian ulang jatuh tempo
```

---

# 7. Context Failure

Jika identitas pasien / episode gagal dimuat:

```text
┌────────────────────────────────────────────────────────────────────┐
│ ⚠ DATA PASIEN TIDAK DAPAT DIMUAT                                 │
│                                                                    │
│ Identitas pasien dan episode belum dapat diverifikasi.            │
│ Jangan melakukan dokumentasi sebelum konteks pasien tersedia.     │
│                                                                    │
│ [ Coba Lagi ]                                                      │
└────────────────────────────────────────────────────────────────────┘
```

Semua aksi tulis:

```text
DISABLED
```

---

# 8. Allergy Failure

Jika alergi gagal dimuat:

```text
⚠ RIWAYAT ALERGI TIDAK DAPAT DIMUAT

Data alergi pasien belum dapat diverifikasi.
```

Tidak boleh dianggap:

```text
Tidak ada alergi
```

---

# 9. Pengkajian Keperawatan — FE-RWI-052

Gunakan internal section navigation.

```text
┌───────────────────────┬────────────────────────────────────────────────────┐
│ BAGIAN PENGKAJIAN     │ PENGKAJIAN KEPERAWATAN                            │
│                       │                                                    │
│ ● Kajian Umum         │ Jenis Pengkajian                                  │
│ ○ Risiko Jatuh        │ (●) Pengkajian Awal   ( ) Pengkajian Ulang        │
│ ○ Nyeri               │                                                    │
│ ○ Skrining Gizi       │ Status  : Draft                                    │
│ ○ Kemandirian         │ Tenggat : 07 Sep 2026 14:00                        │
│ ○ Edukasi             │                                                    │
│ ○ Rencana Pemulangan  │ ────────────────────────────────────────────────   │
│                       │                                                    │
│                       │ Keluhan Utama                                      │
│                       │ [............................................]     │
│                       │                                                    │
│                       │ Riwayat Penyakit Sekarang                          │
│                       │ [............................................]     │
│                       │                                                    │
│                       │ ...                                                │
├───────────────────────┴────────────────────────────────────────────────────┤
│ Progress 5/7 bagian selesai                  [Simpan] [Selesaikan]         │
└────────────────────────────────────────────────────────────────────────────┘
```

Tujuh kelompok:

```text
Kajian Umum
Risiko Jatuh
Nyeri
Skrining Gizi
Kemandirian
Edukasi
Rencana Pemulangan
```

---

# 10. Tenggat Pengkajian

Jika policy tersedia:

```text
Status
● Draft

Tenggat
07 Sep 2026 14:00

Sisa Waktu
2 jam 14 menit
```

Jika policy tidak tersedia:

```text
○ BATAS WAKTU BELUM DITETAPKAN

Pengkajian tetap dapat dilakukan.
```

Tidak boleh membuat angka tenggat default dari frontend.

---

# 11. Penyelesaian Pengkajian

```text
┌──────────────────────────────────────────────────────────────────────┐
│ ⚠ Pengkajian yang sudah diselesaikan akan dikunci.                 │
│                                                                      │
│                            [Simpan Draft] [Selesaikan Pengkajian]    │
└──────────────────────────────────────────────────────────────────────┘
```

Jika belum lengkap:

```text
Pengkajian belum dapat diselesaikan.

Bagian yang belum lengkap:
• Risiko Jatuh
• Skrining Gizi
• Rencana Pemulangan
```

---

# 12. Koreksi Pengkajian

Setelah status selesai:

```text
[Sunting]        TIDAK ADA
[Tambah Koreksi] ADA
```

Contoh:

```text
┌─────────────────────────────────────────────────────────────────────┐
│ PENGKAJIAN AWAL                                      ✓ SELESAI     │
├─────────────────────────────────────────────────────────────────────┤
│ Nyeri                                                               │
│ Skala Nyeri : 7                                                     │
│                                                                     │
│ Penulis : Ns. Sari                                                  │
│ Waktu   : 07 Sep 2026 08:00                                         │
├─────────────────────────────────────────────────────────────────────┤
│ KOREKSI #1                                              DIKOREKSI   │
│                                                                     │
│ Skala nyeri yang benar adalah 4.                                   │
│                                                                     │
│ Alasan     : Salah input skor                                      │
│ Dikoreksi  : Ns. Sari                                              │
│ Waktu      : 07 Sep 2026 09:20                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

# 13. Lini Masa Pengkajian — FE-RWI-053

Direkomendasikan menggunakan timeline card.

```text
┌──────────────────────────────────────────────────────────────────────┐
│ LINI MASA PENGKAJIAN                                                │
│                                                                      │
│ [Semua] [Nyeri] [Risiko Jatuh] [Gizi]                              │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ ● 07 Sep 14:00                                                     │
│   Nyeri                                                             │
│   Skor : 3/10                                                       │
│   Ns. Sari                                                          │
│   ↓ Membaik dari 5                                                  │
│                                                                      │
│ ● 07 Sep 08:00                                                     │
│   Nyeri                                                             │
│   Skor : 5/10                                                       │
│   Ns. Sari                                                          │
│   [Koreksi #1]                                                      │
│                                                                      │
│ ● 06 Sep 20:00                                                     │
│   Nyeri                                                             │
│   Skor : 7/10                                                       │
│   Ns. Rina                                                          │
└──────────────────────────────────────────────────────────────────────┘
```

Nilai lama tidak boleh hilang.

---

# 14. Rencana Asuhan Keperawatan — FE-RWI-054

Gunakan master-detail.

```text
┌───────────────────────────────┬─────────────────────────────────────────────┐
│ MASALAH KEPERAWATAN           │ DETAIL RENCANA ASUHAN                      │
│                               │                                             │
│ ● Nyeri Akut                  │ Masalah                                     │
│   ACTIVE                      │ Nyeri akut                                  │
│                               │                                             │
│ ● Risiko Jatuh               │ Tujuan                                      │
│   ACTIVE                      │ Skala nyeri ≤ 3 dalam 24 jam                │
│                               │                                             │
│ ✓ Gangguan Tidur             │ Rencana                                     │
│   RESOLVED                    │ • Monitor skala nyeri                       │
│                               │ • Posisi nyaman                             │
│ × Kecemasan                  │ • Edukasi relaksasi                         │
│   DISCONTINUED                │                                             │
│                               │ Evaluasi                                    │
│ [+ Tambah Masalah]            │ Pasien mengatakan nyeri berkurang           │
│                               │                                             │
│                               │ [Ubah] [Tambah Evaluasi]                    │
│                               │ [Nyatakan Tercapai]                         │
└───────────────────────────────┴─────────────────────────────────────────────┘
```

Status:

```text
ACTIVE
RESOLVED
DISCONTINUED
```

---

# 15. Riwayat Rencana Asuhan

Rencana Asuhan menggunakan version history, bukan addendum.

```text
RIWAYAT PERUBAHAN

Versi 3 — CURRENT
07 Sep 14:10 — Ns. Sari
Target nyeri diubah menjadi ≤ 3

Versi 2
07 Sep 10:00 — Ns. Sari
Ditambahkan edukasi relaksasi

Versi 1
07 Sep 08:10 — Ns. Rina
Rencana dibuat
```

---

# 16. Episode Closed

Jika episode sudah ditutup:

```text
EPISODE TELAH DITUTUP

Rencana asuhan hanya dapat dibaca.
```

Action:

```text
Tambah Masalah       HIDDEN
Ubah                 HIDDEN
Tambah Evaluasi      HIDDEN
Nyatakan Tercapai    HIDDEN
```

Riwayat tetap dapat dibaca.

---

# 17. Catatan Tindakan Keperawatan — FE-RWI-055

Gunakan chronological timeline.

```text
┌──────────────────────────────────────────────────────────────────────┐
│ CATATAN TINDAKAN KEPERAWATAN                    [+ Catat Tindakan]  │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ ● 14:20                                                             │
│   Pemasangan infus                                                  │
│   Ns. Sari                                                          │
│                                                                      │
│   Hasil : Infus terpasang baik                                      │
│   Rencana : Nyeri Akut                                              │
│                                                                      │
│   ✓ FINAL                                                           │
│   ⚠ TAGIHAN BELUM TERKIRIM                                         │
│                                                    [Tambah Koreksi] │
│                                                                      │
│ ● 11:00                                                             │
│   Perawatan luka                                                    │
│   Ns. Rina                                                          │
│                                                                      │
│   Hasil : Luka bersih                                               │
│   Rencana : Tidak ditautkan                                         │
│                                                                      │
│   ✓ FINAL                                                           │
└──────────────────────────────────────────────────────────────────────┘
```

---

# 18. Form Catat Tindakan

```text
┌──────────────────────────────────────────────┐
│ CATAT TINDAKAN KEPERAWATAN                   │
├──────────────────────────────────────────────┤
│                                              │
│ Tindakan *                                   │
│ [........................................]   │
│                                              │
│ Waktu tindakan *                             │
│ [07 Sep 2026] [14:20]                       │
│                                              │
│ Hasil                                        │
│ [........................................]   │
│                                              │
│ Rencana Asuhan terkait                       │
│ [Opsional ▼]                                 │
│                                              │
│                    [Batal] [Simpan Tindakan]│
└──────────────────────────────────────────────┘
```

Rencana Asuhan bersifat optional.

---

# 19. Billing Failure

Jika catatan klinis berhasil tersimpan tetapi Billing gagal:

```text
Pemasangan Infus
✓ Catatan tersimpan

⚠ TAGIHAN BELUM TERKIRIM
```

Jangan menampilkan full-page error seolah-olah pencatatan klinis gagal.

---

# 20. Koreksi Catatan Tindakan

```text
[Sunting]         TIDAK ADA
[Tambah Koreksi]  ADA jika berwenang
```

Contoh:

```text
TINDAKAN ASLI
Pemasangan Infus
Waktu: 14:20
Hasil: Infus terpasang baik

────────────────────────────────────

KOREKSI #1
Hasil yang benar:
Infus terpasang pada tangan kiri.

Alasan : Salah lokasi
Ns. Sari — 15:03
```

---

# 21. Daftar Pantau Kepatuhan — FE-RWI-056

Tidak membuat layar baru.

Tambahkan section pada Daftar Pantau existing.

```text
┌────────────────────────────────────────────────────────────────────────────┐
│ KEPATUHAN PENGKAJIAN                                5 perlu perhatian     │
│                                                                            │
│ Ruangan [Semua ▼]            Status [Belum / Terlambat ▼]                 │
├──────────────┬──────────────┬─────────────┬────────────┬───────────────────┤
│ Pasien       │ Kamar / Bed  │ Pengkajian │ Tenggat    │ Status            │
├──────────────┼──────────────┼─────────────┼────────────┼───────────────────┤
│ Tn. Budi     │ 302 / B      │ Belum ada   │ 14:00      │ ⚠ 2 jam lagi     │
│ Ny. Ani      │ 304 / A      │ Belum ada   │ 09:00      │ Terlambat 3 jam  │
│ Tn. Rudi     │ 305 / C      │ Draft       │ 12:00      │ ⚠ 30 menit       │
└──────────────┴──────────────┴─────────────┴────────────┴───────────────────┘
```

Tidak menampilkan isi klinis.

---

# 22. State Daftar Pantau

Semua tepat waktu:

```text
✓ Seluruh pengkajian sudah tepat waktu.
```

Policy belum ada:

```text
○ BATAS WAKTU PENGKAJIAN BELUM DITETAPKAN

Keterlambatan belum dapat dihitung.
```

Error:

```text
⚠ Data kepatuhan pengkajian tidak dapat dimuat.

[ Coba Lagi ]
```

---

# 23. Reuse Existing Component

Existing form base:

```text
src/components/ui/form-pemeriksaan-ui/
```

Direkomendasikan reuse:

```text
BaseModal
BaseSelectField
BaseTextField
BaseTextareaField
BaseCheckboxCard
BaseSimpleCheckbox
```

Komponen generik dari `doctor-clinical-base` dapat direfactor bertahap ke:

```text
src/components/ui/clinical-workspace/
```

Nursing tidak sebaiknya bergantung permanen pada folder bernama `doctor-clinical-base`.

---

# 24. Struktur Folder Domain

```text
src/components/view/
└── health-services/
    └── inpatient-management/
        └── nursing-workspace/
            │
            ├── nursing-workspace-client.jsx
            ├── nursing-workspace-view.jsx
            │
            ├── components/
            │   ├── nursing-episode-header.jsx
            │   ├── responsible-nurse-indicator.jsx
            │   └── nursing-workspace-sections.jsx
            │
            ├── sections/
            │   ├── assessment/
            │   ├── timeline/
            │   ├── care-plan/
            │   └── intervention/
            │
            └── modals/
                ├── complete-assessment-modal.jsx
                ├── add-correction-modal.jsx
                ├── care-plan-item-modal.jsx
                ├── care-plan-evaluation-modal.jsx
                ├── close-care-plan-item-modal.jsx
                └── nursing-intervention-modal.jsx
```

---

# 25. Mapping Task ke Tampilan

| Task | Tampilan |
|---|---|
| `FE-RWI-051` | Nursing Workspace + Patient Context |
| `FE-RWI-052` | Pengkajian Awal/Ulang + Completion + Koreksi |
| `FE-RWI-053` | Lini Masa Nyeri/Risiko Jatuh/Gizi |
| `FE-RWI-054` | Rencana Asuhan + Evaluasi + Version History |
| `FE-RWI-055` | Catatan Tindakan + Billing State + Koreksi |
| `FE-RWI-056` | Daftar Pantau Kepatuhan |

---

# 26. Responsive Behaviour

## Desktop

```text
Patient Header
────────────────────────────────────────
Left Nav | Main Content | Right Summary
```

## Tablet

```text
Patient Header
────────────────────────
Left Nav | Main Content
           ↓
      Right Summary
```

## Mobile

```text
Patient Header
↓
Section Selector
↓
Main Content
↓
Quick Summary
↓
Context Panel
```

---

# 27. Acceptance Visual Minimum

Implementasi dianggap sesuai rancangan bila:

1. Menggunakan pola Patient Header + Left Nav + Main Content + Right Summary.
2. Informasi pasien lebih rapi daripada grid IGD existing.
3. Patient context selalu terlihat.
4. Alergi tampil menonjol.
5. Context failure menonaktifkan seluruh aksi tulis.
6. Menu section Nursing dapat berbeda dari IGD tanpa mengubah base layout.
7. Quick Summary configurable.
8. Right context panel configurable.
9. Pengkajian memiliki tujuh kelompok yang jelas.
10. Pengkajian selesai tidak dapat diedit langsung.
11. Koreksi tampil sebagai addendum bernomor.
12. Nilai lama tetap terlihat.
13. Lini masa menunjukkan perkembangan dari waktu ke waktu.
14. Rencana Asuhan memakai version history.
15. Tindakan dapat dibuat tanpa Care Plan.
16. Billing failure tidak dianggap sebagai kegagalan menyimpan klinis.
17. Catatan tindakan final tidak mempunyai tombol Sunting.
18. Monitoring tidak menampilkan isi klinis.
19. Shared base tidak mengandung business field khusus IGD atau Rawat Inap.
20. Rawat Inap dapat menjadi reference implementation untuk refactor IGD.

---

# 28. Kesimpulan

Skema akhir:

```text
PASIEN RAWAT INAP
        ↓
NURSING WORKSPACE
        │
        ├── Pengkajian
        ├── Rencana Asuhan
        ├── Tindakan Keperawatan
        └── Lini Masa
```

Di luar workspace pasien:

```text
DAFTAR PANTAU RAWAT INAP
        │
        └── Kepatuhan Pengkajian
```

Arah arsitektur:

```text
IGD EXISTING
   ↓
ambil pola struktur
   ↓
SHARED CLINICAL WORKSPACE BASE
   ↓
RAWAT INAP KEPERAWATAN
   ↓
reference implementation
   ↓
IGD REFACTOR
```
