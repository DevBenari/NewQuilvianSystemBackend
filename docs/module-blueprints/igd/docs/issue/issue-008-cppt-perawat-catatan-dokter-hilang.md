# ISSUE-008 — CPPT di Ruang Kerja Perawat IGD Menaruh Catatan Dokter di Kolom Perawat

```yaml
issue_id: ISSUE-IGD-008
module_id: igd
submodule: igd
layar: "Ruang Kerja Pemeriksaan IGD (layar perawat, SCR-IGD-P01) — tab Catatan Terintegrasi; Ruang Kerja Dokter IGD — CPPT Pasien"
sumber_laporan: "Laporan pelapor 05-11-2026: 2 butir (1 bug, 1 permintaan tampilan), 1 lampiran"
tanggal_issue: "2026-11-05"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004 s.d. 007"
rencana_perbaikan: ../plan-repair/plan-repair-008-cppt-perawat-catatan-dokter-hilang.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Setelah SOAP dokter berhasil masuk CPPT (`ISSUE-IGD-006`), CPPT di layar **perawat** menampilkan
"Catatan Dokter (0)". Catatan dokter itu (penulis dr. Jenah Ali, diagnosis A02.0) muncul di kolom
**Catatan Perawat**. Penyebabnya, layar perawat mengenali catatan dokter dari kata **"Dokter"**,
padahal server mengirim profesi dalam bentuk baku **"Doctor"**. Akibatnya **semua** catatan dokter
selalu masuk kolom perawat.

Pelapor juga meminta tampilan CPPT perawat disamakan dengan CPPT di Ruang Kerja Dokter. Permintaan
itu sekaligus menyelesaikan bug di atas, karena komponen CPPT dokter mengenali "Doctor" maupun
"Dokter". Tetapi CPPT dokter IGD sendiri punya cacat tampilan: "0 catatan", "No. RM -", penyaring
tidak tampil, dan kotak "Aksi tidak tersedia". Cacat itu harus dibetulkan dulu supaya tidak ikut
tersalin ke layar perawat.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "di ruang kerja perawat kenapa catatan dokter tidak muncul, atau catatan soap dokter tidak muncul" | Lampiran 1: tab Catatan Terintegrasi, Rabu 4 Nov 2026 — "Catatan Dokter (0) — Tidak ada catatan dokter"; "Catatan Perawat (2)" memuat entri "dr. Jenah Ali 14.36" berisi keluhan, nyeri, tanda vital, Assessment A02.0/A17.0, Rencana |
| 2 | "tampilan cppt nyaa ini samakan dengan yang ada di dokter aja" | — |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-008-01` | 1 | Catatan dokter dikelompokkan sebagai catatan perawat: layar mencari "Dokter", server mengirim "Doctor" | `CONTRACT_MISMATCH` | Frontend | High | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-008-01`, `-02` |
| `ISS-IGD-008-02` | 2 | Tampilan CPPT perawat berbeda dari dokter | `DESIGN_CHANGE` (permintaan pemilik) | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-008-02` |
| `ISS-IGD-008-T1` | — | CPPT dokter IGD salah merakit komponen Rawat Inap: kepala ("0 catatan", "No. RM -"), penyaring tidak tampil, kotak "Aksi tidak tersedia" (`ISSUE-IGD-006-T5`) | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-IGD-008-02` |

---

## 4. Rincian per temuan

### ISS-IGD-008-01 — Catatan dokter masuk kolom perawat

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend — `emergency-assessment-integrated-note-tab.jsx` (milik IGD) |
| **Keparahan** | High — perawat tidak melihat instruksi dan penilaian dokter di tempat yang seharusnya |
| **Status bukti** | SUDAH-VERIFIKASI; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-IGD-008-01`, `FIX-IGD-008-02` |

**Kenapa terjadi.**

```text
Catatan dokter tampil di kolom "Catatan Perawat"
  ← isDoctorNote(item) bernilai false untuk professionType "Doctor"
    ← isDoctorNote mencari teks "dokter" (INTEGRATED_NOTE_PROFESSION.DOCTOR = "Dokter")
      ← server menyimpan dan mengirim profesi baku berbahasa Inggris:
        NormalizeProfessionType("dokter" | "doctor") → "Doctor"
```

`src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-integrated-note-tab.jsx:35-38`

```jsx
const isDoctorNote = (item) =>
  String(item?.professionType || "")
    .toLowerCase()
    .includes(INTEGRATED_NOTE_PROFESSION.DOCTOR.toLowerCase());   // "dokter"
```

`emergency-assessment-constant.jsx:233-236` → `DOCTOR: "Dokter"`.

`NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`,
`NormalizeProfessionType`: `"dokter" or "doctor" => "Doctor"`, `"perawat" or "nurse" => "Nurse"`.

Karena "doctor" tidak mengandung "dokter", setiap catatan dokter, termasuk salinan SOAP dari
`ISSUE-IGD-006`, masuk kolom perawat. Pembanding: CPPT dokter memakai `resolveProfessionKey`
(`doctor-cppt-utils.js`), yang menerima `doctor`, `dokter`, maupun kode angka.

Fungsi ini sudah ada sejak layar perawat dibuat (commit `ab441baac`, 21 Agu 2026). Komponen yang sama
juga dipakai riwayat pada tab **SOAP perawat** (`soapOnly`).

**Apakah ini menyimpang dari desain?** Ya. Komponen itu sendiri menyatakan CPPT "dipisahkan antara
catatan dokter dan catatan perawat" untuk penelusuran siapa mencatat apa. Jenis: `CONTRACT_MISMATCH`.

---

### ISS-IGD-008-02 — Samakan tampilan CPPT perawat dengan dokter

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` — diminta pemilik |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Perbaikan** | `FIX-IGD-008-02` |

**Keadaan sekarang.** Layar perawat memakai komponen sendiri: daftar teks per hari, dua kolom, tanpa
penyaring dan tanpa rincian. Layar dokter memakai CPPT Terintegrasi Rawat Inap: navigasi bulan, kolom
Dokter/Perawat, kartu catatan, penyaring jenis catatan, dan rincian per catatan.

**Rekomendasi.** Jadikan CPPT dokter IGD sebagai **satu komponen IGD yang dipakai kedua layar**.
Datanya diambil per kunjungan, dan verifikasi DPJP hanya menyala bagi akun dokter. Perawat melihat
tampilan yang sama tanpa tombol verifikasi.

---

### ISS-IGD-008-T1 — CPPT dokter IGD salah merakit komponen

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `BUG` |
| **Keparahan** | Low |
| **Kenapa dicantumkan** | Bila tampilan perawat disamakan, cacat ini ikut tersalin |

`emergency-doctor-integrated-note-tab.jsx` dibandingkan dengan aslinya,
`integrated-progress-note-tab.jsx` (Rawat Inap):

| Bagian | IGD mengirim | Komponen membaca | Akibat |
| --- | --- | --- | --- |
| `DoctorCpptHeader` | `title`, `subtitle`, `filterControl`, `actionControl` | `patientName`, `medicalRecordNumber`, `totalRows`, `loading`, `onRefresh` | "0 catatan", "No. RM -" |
| `DoctorCpptFilterBar` | `activeValue`, `notes` | `value`, `disabled` | Pilihan aktif tidak tersorot |
| Penempatan penyaring | di kepala (diabaikan) | `headerAction` pada `IntegratedNoteTimeline` | Penyaring tidak tampil |
| Pesan aksi | `ClinicalActionGuard error/notice` | `allowed`/`reason` | Kotak "Aksi tidak tersedia" selalu tampil |

---

## 5. Tanya-jawab pelapor

> **T:** Kenapa catatan dokter tidak muncul di ruang kerja perawat?
>
> **J:** Catatannya muncul, tetapi di kolom yang salah. Layar perawat mengenali catatan dokter dari
> kata "Dokter", sedangkan server menulisnya "Doctor", sehingga catatan itu dianggap catatan perawat.
> Setelah layar perawat memakai tampilan CPPT yang sama dengan dokter, catatan dokter masuk kolom
> Dokter.

---

## 6. Temuan tambahan

`ISS-IGD-008-T1` di atas.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada. **Untuk pemilik:** Tidak ada. Arah perbaikan sudah diminta pelapor
("samakan dengan yang ada di dokter").

---

## 8. Catatan pola

- Nilai baku server berbahasa Inggris ("Doctor"/"Nurse") dibandingkan dengan label berbahasa
  Indonesia di layar. Usul: setiap pembanding profesi di frontend memakai satu fungsi bersama
  (`resolveProfessionKey`), bukan konstanta label.
- Untuk keenam kalinya di modul ini, komponen Rawat Inap dirakit dengan props yang salah
  (`ISSUE-IGD-004` s.d. `-007`).

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-05 | Issue dibuat dari laporan 2 butir + 1 lampiran; 2 temuan + 1 temuan tambahan | `diagnose-module-issue` |
| 2026-11-05 | Diperbaiki dan diuji (lihat plan repair Riwayat); status `DALAM_PERBAIKAN` | Claude Code |
