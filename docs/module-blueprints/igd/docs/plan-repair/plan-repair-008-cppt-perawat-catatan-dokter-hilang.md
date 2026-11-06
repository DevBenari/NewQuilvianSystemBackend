# PLAN-REPAIR-008 — Satu Tampilan CPPT IGD untuk Dokter dan Perawat

```yaml
plan_id: PLAN-REPAIR-IGD-008
issue: ../issue/issue-008-cppt-perawat-catatan-dokter-hilang.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-11-05"
diputuskan_oleh: "Rizki Gunawan — arah perbaikan dari laporan: \"tampilan cppt nyaa ini samakan dengan yang ada di dokter aja\""
tanggal_keputusan: "2026-11-05"
basis_source_backend: "30f1396d (rizkiG)"
basis_source_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004 s.d. 007"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-008-01` | `ISS-IGD-008-01` (riwayat tab SOAP perawat) | `isDoctorNote` mengenali "Doctor" dan "Dokter" | FE (milik IGD) | 1 | — | 🟡 SEBAGIAN — source diubah, ESLint bersih; riwayat tab SOAP perawat belum diuji di browser; laporan task tracked belum ada | — |
| `FIX-IGD-008-02` | `ISS-IGD-008-01`, `-02`, `-T1`; `ISSUE-IGD-006-T5` | Komponen CPPT IGD bersama (`EmergencyCpptPanel`) dirakit sesuai Rawat Inap; dipakai Ruang Kerja Dokter dan tab Catatan Terintegrasi perawat | FE (milik IGD) | 1 | — | 🟡 SEBAGIAN — AC 1–6 terverifikasi di browser (8/8 skenario); laporan task tracked belum ada | [laporan uji](../../perawat-igd/testing/test-by-agy/laporan-live-testing-cppt-terintegrasi.md) |

---

## 2. Solusi terpilih

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ekstrak isi tab CPPT dokter IGD menjadi `EmergencyCpptPanel` (props: `encounterId`, `isEpisodeClosed`, `canVerify`, `patientName`, `medicalRecordNumber`). Tab dokter dan tab perawat memakainya | Satu sumber tampilan; perbaikan props sekali untuk dua layar; tanpa menyentuh komponen Rawat Inap | — |
| B | Perbaiki `isDoctorNote` saja | Paling kecil | Tidak memenuhi permintaan "samakan dengan dokter" |
| C | Salin tab CPPT dokter ke folder perawat | Cepat | Dua salinan akan berbeda lagi seiring waktu |

**Solusi terpilih: Opsi A**, ditambah `FIX-01` untuk tab SOAP perawat yang tetap memakai komponen lama
(mode `soapOnly`).

Perakitan mengikuti `integrated-progress-note-tab.jsx` (Rawat Inap): `DoctorCpptHeader` dengan
`patientName`/`medicalRecordNumber`/`totalRows`/`loading`/`onRefresh`; `DoctorCpptFilterBar` dengan
`value`/`onChange`/`disabled` sebagai `headerAction` timeline; pesan aksi lewat `ClinicalSafetyAlert`;
tanpa `ClinicalActionGuard`.

---

## 3. Skema tampilan sebelum → sesudah

**SEBELUM (perawat):**

```text
Catatan Perkembangan Pasien Terintegrasi
Rabu, 4 November 2026
 Catatan Dokter (0)              │ Catatan Perawat (2)
 Tidak ada catatan dokter        │ dr. Jenah Ali 14.36  ← catatan dokter salah kolom
                                 │ Dimas Kurniawan 11.55
```

**SESUDAH (perawat dan dokter sama):**

```text
CPPT Terintegrasi                [Pasien: Tn. Pseudonim E] [No. RM 00-00-00-01] [2 catatan] [Refresh]
                         ◀ November 2026 ▶          [Semua | Dokter | Perawat | Profesi lain]
Rabu, 4 November 2026
 Catatan Dokter (1)              │ Catatan Perawat (1)
 ┌ dr. Contoh A · 14.36 ───────┐ │ ┌ Ns. Contoh B · 11.55 ─────┐
 │ S/O/A/P ringkas    [Rincian]│ │ │ S/O/A/P ringkas  [Rincian]│
 └─────────────────────────────┘ │ └───────────────────────────┘
```

| Wilayah | Isi | Sumber | Perbedaan dokter/perawat |
| --- | --- | --- | --- |
| Kepala | Nama, No. RM, jumlah catatan, Refresh | Kunjungan + jumlah catatan | — |
| Penyaring | Semua / Dokter / Perawat / Profesi lain | `noteKind`/`professionType` | — |
| Timeline | Per bulan dan hari; kolom Dokter dan Perawat | `GET /patient-integrated-progress-notes?encounterId=` | Tombol verifikasi hanya untuk dokter (DPJP) |

---

## 4. Rincian perbaikan

### FIX-IGD-008-01 — `isDoctorNote` mengenali "Doctor"

**Berkas:** `emergency-assessment-integrated-note-tab.jsx`. **Radius:** riwayat tab SOAP perawat.

**AC.** Pada riwayat tab SOAP perawat, catatan ber-`professionType` "Doctor" masuk kolom Dokter.

### FIX-IGD-008-02 — `EmergencyCpptPanel` bersama

**Berkas:** `doctor-emergency/tabs/emergency-doctor-integrated-note-tab.jsx` (komponen bersama +
pembungkus dokter); `emergency-assessment-view/emergency-assessment-detail-view.jsx` (tab Catatan
Terintegrasi perawat memakai panel); `use-emergency-assessment-detail.jsx` (muatan Redux CPPT lama
untuk tab itu tidak lagi diperlukan).

**Acceptance criteria.**

1. Layar perawat, tab Catatan Terintegrasi, pasien dengan SOAP dokter selesai → catatan dokter tampil di
   kolom **Catatan Dokter**, catatan perawat di kolom **Catatan Perawat**.
2. Kepala menampilkan nama pasien, No. RM, dan jumlah catatan yang benar (bukan "0 catatan"/"No. RM -").
3. Penyaring Dokter/Perawat tampil dan menyaring.
4. Tidak ada kotak "Aksi tidak tersedia".
5. Akun perawat tidak melihat tombol verifikasi; akun dokter tetap seperti sebelumnya.
6. CPPT di Ruang Kerja Dokter tampil sama dengan layar perawat.

**Verifikasi.** `npx eslint`; uji browser akun Perawat IGD dan Dokter IGD pada pasien uji K1-C (punya
catatan dokter dan perawat).

---

## 5. Urutan pengerjaan

```text
FIX-IGD-008-01   tanpa dependency
FIX-IGD-008-02   tanpa dependency
```

---

## 6. Keputusan pemilik yang dibutuhkan

Tidak ada.

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Perubahan |
| --- | --- | --- |
| `03-frontend-architecture.md` | Bagian layar perawat (tab Catatan Terintegrasi) | "Memakai komponen CPPT IGD yang sama dengan Ruang Kerja Dokter; verifikasi DPJP hanya untuk dokter" |

---

## 8. Verifikasi menyeluruh

1. Perawat membuka tab Catatan Terintegrasi → catatan dokter di kolom Dokter.
2. Dokter membuka CPPT Pasien → tampilan sama, kepala benar.
3. Penyaring bekerja di kedua layar.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-05 | Rencana dibuat, 2 perbaikan; arah disetujui lewat laporan pelapor | `diagnose-module-issue` |
| 2026-11-05 | Dikerjakan: `emergency-doctor-integrated-note-tab.jsx` (komponen `EmergencyCpptPanel` + pembungkus dokter), `emergency-assessment-detail-view.jsx`, `emergency-assessment-integrated-note-tab.jsx`, `use-emergency-assessment-detail.jsx`. ESLint 4 berkas: 0 masalah. Uji browser baca-saja, akun Perawat IGD dan Dokter IGD: 8/8 lulus, 0 galat, 0 penulisan data | Claude Code |
