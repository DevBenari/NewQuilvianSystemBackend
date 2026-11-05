# PLAN-REPAIR-005 — Aktifkan Isi Otomatis Assessment dari ICD-10 pada SOAP Dokter IGD

```yaml
plan_id: PLAN-REPAIR-IGD-005
issue: ../issue/issue-005-assessment-soap-tidak-terisi-diagnosis.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-11-04"
diputuskan_oleh: "Rizki Gunawan (persetujuan lisan di sesi kerja: \"perbaiki sekarang\")"
tanggal_keputusan: "2026-11-04"
basis_source_backend: "30f1396d (rizkiG)"
basis_source_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004-01/02"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-005-01` | `ISS-IGD-005-01` | Penanda blok Assessment bernilai awal `""` dan di-reset `""`, sama dengan Rawat Inap | FE | 1 | — | 🟡 SEBAGIAN — AC 5 dari 5 terverifikasi di browser; laporan task tracked belum ada | [laporan uji §8](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |
| `FIX-IGD-005-02` | `ISS-IGD-005-02` | Hitung dan kembalikan `assessmentOutOfSync` supaya tombol "Susun ulang blok diagnosa" muncul | FE | 1 | — | 🟡 SEBAGIAN — AC 2 dari 3 terverifikasi (AC 3 "catatan selesai" tidak dapat diuji: tidak ada tombol Selesaikan, `ISSUE-IGD-004-T2`); laporan task tracked belum ada | [laporan uji §8](../../testing/test-by-agy/laporan-live-testing-soap-dokter-igd.md) |

**Ringkasan: 0 dari 2 perbaikan selesai; keduanya sebagian (lolos uji browser, menunggu laporan task tracked dan commit).**

---

## 2. Solusi terpilih per temuan

### ISS-IGD-005-01 — Isi otomatis tidak aktif

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `assessmentBlockRef` (dan `objectiveBlockRef`) bernilai awal `""` dan di-reset `""` saat encounter berubah | Dua baris; sama persis dengan Rawat Inap; draf awal langsung aktif | — |
| B | Ubah nilai awal `selectedId` menjadi `""` supaya `startNewNote` terpanggil | Juga mengaktifkan penanda | Mengubah alur pemilihan draf yang dipakai isi otomatis S/O (`FIX-IGD-004-02`) dan auto-start tab; radius lebih besar |

**Solusi terpilih: Opsi A.** Perubahan terkecil, meniru `use-inpatient-progress-note.jsx:135` dan
`:229-232`, dan tidak menyentuh alur pemilihan draf.

### ISS-IGD-005-02 — Tombol cadangan tidak muncul

Hanya ada satu cara yang wajar: hitung `assessmentOutOfSync` dengan aturan Rawat Inap
(`use-inpatient-progress-note.jsx:1046-1049`). Nilainya benar bila catatan dapat disunting, daftar
diagnosis tidak kosong, dan Assessment tidak diawali blok diagnosis terbaru.

---

## 3. Skema tampilan sebelum → sesudah

**SEBELUM** (lampiran 1):

```text
Diagnosis Terpilih
  Utama     A02.1  Salmonella septicaemia      [ Jadikan Utama ] [ Hapus ]
  Tambahan  A20.1  Cellulocutaneous plague     [ Jadikan Utama ] [ Hapus ]

A — Assessment
[ Diagnosis kerja/penilaian klinis dokter. Terisi otomatis dari … ]   ← kosong
```

**SESUDAH:**

```text
Diagnosis Terpilih
  Utama     A02.1  Salmonella septicaemia      [ Jadikan Utama ] [ Hapus ]
  Tambahan  A20.1  Cellulocutaneous plague     [ Jadikan Utama ] [ Hapus ]

A — Assessment
[ 1. A02.1 - Salmonella septicaemia (Diagnosa Utama)                  ]
[ 2. A20.1 - Cellulocutaneous plague (Diagnosa Tambahan)              ]
[ <teks penilaian klinis yang diketik dokter tetap di bawah blok>     ]
```

Bila blok tidak dapat diperbarui otomatis (misalnya dokter mengubah baris blok sendiri, atau draf lama
dibuka dari riwayat):

```text
A — Assessment
[ …teks dokter…                                                       ]
⚠ Blok diagnosa di Assessment belum sesuai daftar ICD-10 terbaru.  [ Susun ulang blok diagnosa ]
```

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Diagnosis Terpilih | Daftar ICD-10, Utama dulu | `useProgressNoteDiagnoses` | `SoapIcdSection` / `DoctorDiagnosisTable` (tidak berubah) |
| A — Assessment | Blok diagnosis bernomor di awal + teks dokter | `buildAssessmentBlock(diagnoses)` | `SoapEditor` (tidak berubah) |
| Petunjuk tidak sinkron | Teks + tombol susun ulang | `assessmentOutOfSync` (baru di hook IGD) | `SoapEditor:412-424` (tidak berubah) |

**Tombol**

| Label | Jenis | Kapan tampil | Yang terjadi |
| --- | --- | --- | --- |
| Susun ulang blok diagnosa | Ghost | Catatan dapat disunting, ada diagnosis, Assessment tidak diawali blok terbaru | Blok lama diganti blok terbaru; teks dokter di bawahnya dipertahankan |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Belum ada diagnosis | Assessment tidak diubah; petunjuk tidak tampil |
| Diagnosis ditambah/dihapus/diganti Utama | Blok di awal Assessment ikut berubah seketika |
| Catatan selesai/terkunci | Tidak ada perubahan otomatis; petunjuk tidak tampil |

---

## 4. Rincian perbaikan

### FIX-IGD-005-01 — Aktifkan penanda blok Assessment sejak draf awal

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-005-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `use-emergency-progress-note.jsx` — `useRef(null)` → `useRef("")` untuk kedua penanda; reset saat encounter berubah → `""` |
| **Radius dampak** | Tab Catatan Dokter IGD saja |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Dokter membuka pasien pertama setelah halaman dimuat → tab Catatan Dokter → menambah A02.1 sebagai
   Utama: Assessment langsung berisi "1. A02.1 - Salmonella septicaemia (Diagnosa Utama)".
2. Menambah A20.1: baris kedua "2. A20.1 - Cellulocutaneous plague (Diagnosa Tambahan)" muncul.
3. Dokter mengetik "Klinis sepsis, pantau hemodinamik" di bawah blok, lalu menghapus A20.1: blok
   tinggal satu baris dan teks dokter tetap ada.
4. Menjadikan A20.1 Utama: urutan blok berubah, Utama di nomor 1.
5. Isi otomatis S/O dari perawat (`FIX-IGD-004-02`) tetap berjalan.

### FIX-IGD-005-02 — Sediakan `assessmentOutOfSync`

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-005-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `use-emergency-progress-note.jsx` — nilai turunan baru dan kembalian hook |
| **Radius dampak** | Tab Catatan Dokter IGD saja |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Dokter menghapus satu baris blok di Assessment secara manual, lalu menambah diagnosis lain:
   petunjuk "Blok diagnosa di Assessment belum sesuai…" dan tombol **Susun ulang blok diagnosa** tampil.
2. Menekan tombol: Assessment diawali blok terbaru, teks dokter lain tetap ada, dan petunjuk hilang.
3. Tanpa diagnosis, atau pada catatan selesai, petunjuk tidak tampil.

**Verifikasi kedua perbaikan.** `npx eslint` berkas; uji browser akun Dokter IGD pada pasien uji
yang encounter-nya punya dokter (lihat `ISSUE-IGD-004-T3`).

**Risiko.** Isi otomatis kini juga berjalan pada draf awal yang sudah diisi S/O otomatis. Itu memang
diharapkan, karena blok hanya menyentuh kolom Assessment. Deteksi: kriteria 5.

---

## 5. Urutan pengerjaan

```text
FIX-IGD-005-01   tanpa dependency
FIX-IGD-005-02   tanpa dependency
```

Keduanya di berkas yang sama dan muat dalam satu task builder.

---

## 6. Keputusan pemilik yang dibutuhkan

Tidak ada, selain persetujuan rencana ini.

---

## 7. Dokumen hulu yang ikut direvisi

Tidak ada. Perbaikan mengembalikan perilaku yang sudah dijanjikan layar dan dirancang di §15.

---

## 8. Verifikasi menyeluruh

1. Muat ulang halaman dokter IGD, buka pasien uji, buka Catatan Dokter.
2. Tambah dua diagnosis → Assessment berisi blok dua baris.
3. Ketik teks klinis → ubah Utama → blok berubah urutan, teks tetap.
4. Simpan Catatan CPPT → buka ulang → Assessment tersimpan utuh (bergantung `FIX-IGD-004-01`).

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Rencana dibuat, 2 perbaikan, menunggu persetujuan | `diagnose-module-issue` |
| 2026-11-04 | Disetujui Rizki Gunawan ("perbaiki sekarang"). Kedua perbaikan dikerjakan pada `use-emergency-progress-note.jsx`; ESLint 0 masalah. Uji browser Chromium akun Dokter IGD: 7/7 lulus, tanpa penulisan data | Claude Code |
| 2026-11-04 | Perluasan `FIX-IGD-005-01` (ditemukan saat uji `ISSUE-IGD-006`): draf yang **dibuka ulang** kini juga menerima blok ICD-10 bila catatan belum berdiagnosis, sama dengan Rawat Inap. Uji: Assessment menjadi "1. A02.0 - Salmonella enteritis (Diagnosa Utama)" di atas teks dokter, dan tersimpan di database | Claude Code |
