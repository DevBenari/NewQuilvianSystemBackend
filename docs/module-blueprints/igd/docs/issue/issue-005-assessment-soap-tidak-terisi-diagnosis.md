# ISSUE-005 — Assessment SOAP Dokter IGD Tidak Terisi dari Diagnosis ICD-10 yang Dipilih

```yaml
issue_id: ISSUE-IGD-005
module_id: igd
submodule: igd
layar: "Ruang Kerja Dokter IGD (SCR-IGD-D01) — tab Catatan Dokter, bagian Diagnosis Terpilih dan A — Assessment"
sumber_laporan: "Laporan pelapor 04-11-2026: 1 butir, 1 lampiran"
tanggal_issue: "2026-11-04"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Medium
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004-01/02 pada berkas yang sama"
rencana_perbaikan: ../plan-repair/plan-repair-005-assessment-soap-tidak-terisi-diagnosis.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Dokter IGD memilih dua diagnosis ICD-10 pada SOAP: satu **Utama** dan satu **Tambahan**. Layar
menyebut "Daftar ICD-10 ini menjadi sumber otomatis untuk field Assessment", dan kolom Assessment
berbunyi "Terisi otomatis dari diagnosa ICD-10 terpilih". Kenyataannya kolom Assessment tetap kosong.

Penyebabnya ada di layar IGD, bukan di server. Mekanisme pengisian otomatisnya sudah ada (disalin
dari Rawat Inap), tetapi **tidak pernah diaktifkan** untuk draf yang terbuka saat tab Catatan Dokter
pertama kali dibuka. Tombol cadangan "Susun ulang blok diagnosa" yang ada di Rawat Inap juga tidak
pernah muncul di IGD, karena nilai pemicunya tidak disediakan.

Perilaku ini menyimpang dari rancangan (`BUG`). Rawat Inap, yang memakai editor yang sama, mengisi
Assessment dengan benar.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "assessement ini juga jika dia sudah milih atau sudah menambahan diagnnosa pada icd ten ini sekaligus input ke assessemnt atau masukan nilai tersebut ke field assessment, saya sudah menambahkan diagnosa dari icd ten ini tapi di field assessement nyaa kosong" | Lampiran 1: bagian *Diagnosis Terpilih* berisi A02.1 (Utama) dan A20.1 (Tambahan); kolom *A — Assessment* kosong dengan placeholder "…Terisi otomatis dari diagnosa ICD-10 terpilih."; tombol *Generate Plan* nonaktif |

Permintaan pelapor ("masukkan nilai tersebut ke field assessment") sama dengan perilaku yang
dijanjikan layar, jadi bukan permintaan perilaku baru.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-005-01` | 1 | Isi otomatis Assessment dari ICD-10 tidak aktif pada draf yang terbuka saat tab dibuka | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-005-01` |
| `ISS-IGD-005-02` | 1 | Tombol cadangan "Susun ulang blok diagnosa" tidak pernah muncul di IGD | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-005-02` |

---

## 4. Rincian per temuan

### ISS-IGD-005-01 — Isi otomatis Assessment dari ICD-10 tidak aktif pada draf yang terbuka saat tab dibuka

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `use-emergency-progress-note.jsx` (milik IGD) |
| **Keparahan** | Medium — dokter dapat mengetik sendiri, tetapi layar menjanjikan isi otomatis, dan penyelesaian catatan mewajibkan Assessment |
| **Status bukti** | SUDAH-VERIFIKASI di source; gejala DARI-CAPTURE lampiran 1 |
| **Perbaikan** | `FIX-IGD-005-01` |

**Apa yang terjadi.** Setiap kali daftar diagnosis berubah, layar seharusnya menulis blok berikut
di awal kolom Assessment:

```text
1. A02.1 - Salmonella septicaemia (Diagnosa Utama)
2. A20.1 - Cellulocutaneous plague (Diagnosa Tambahan)
```

Teks yang diketik dokter di bawah blok itu tetap dipertahankan. Di IGD, blok ini tidak pernah
ditulis pada draf yang langsung terbuka ketika dokter membuka tab Catatan Dokter.

**Kenapa terjadi.**

```text
Assessment kosong walau dua diagnosis sudah dipilih
  ← efek "diagnosis berubah → tulis blok" berhenti karena penanda blok bernilai null
    ← penanda blok hanya diubah menjadi "" (aktif) oleh selectNote(draf)
      ← di IGD draf sudah terpilih sejak awal (useState(PROGRESS_NOTE_DRAFT_SELECTION)),
        sehingga startNewNote → selectNote(draf) tidak pernah dipanggil
        ← penanda awal null dan di-reset ke null saat encounter berubah —
          berbeda dari Rawat Inap yang memakai "" (commit 905aec3bd, 8 Okt 2026)
```

1. Efek penulis blok berhenti bila penandanya `null`:

   `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-progress-note.jsx:692-710`

   ```jsx
   useEffect(() => {
     if (!editable) return;
     const block = buildAssessmentBlock(diagnosisApi.diagnoses);
     // …
     const previous = assessmentBlockRef.current;
     if (previous === null) return;
     // … tulis blok ke form.assessment
   }, [commitForm, diagnosisApi.diagnoses, diagnosisApi.revision, editable]);
   ```

2. Penanda awal dan reset-nya bernilai `null` (`:270-276`):

   ```jsx
   const assessmentBlockRef = useRef(null);

   useEffect(() => {
     objectiveBlockRef.current = null;
     assessmentBlockRef.current = null;
   }, [encounterId]);
   ```

3. Penanda baru menjadi `""` di dalam `selectNote(PROGRESS_NOTE_DRAFT_SELECTION)` (`:529-543`).
   Fungsi itu dipanggil lewat `startNewNote`, tetapi tab hanya memanggil `startNewNote` bila **belum
   ada** yang terpilih (`emergency-doctor-progress-tab.jsx:149-153`, syarat `!selectedId`). Di IGD,
   `selectedId` sudah berisi draf sejak awal (`use-emergency-progress-note.jsx:226`).

4. Pembanding — Rawat Inap (`use-inpatient-progress-note.jsx`):
   - `:121` → `useState("")`, sehingga `startNewNote` dipanggil;
   - `:135` → `useRef("")`;
   - `:229-232` → reset ke `""` saat episode berubah.

**Kapan gejala tidak muncul.** Bila dokter berpindah pasien di daftar kiri, `selectedId` dikosongkan
dan `startNewNote` terpanggil, sehingga isi otomatis aktif. Jadi gejala muncul pada **pasien pertama
yang dibuka** setelah halaman dimuat. Pasien itulah yang paling sering dikerjakan dokter (DUGAAN —
cara membuktikan: buka pasien A, pindah ke pasien B, tambahkan ICD-10; Assessment B seharusnya terisi).

**Apakah ini menyimpang dari desain?** Ya. Tab Catatan Dokter ditetapkan *Reuse with adapter* dari
Rawat Inap (`03-frontend-architecture.md` §15). Layar sendiri menjanjikan isi otomatis
(`DoctorDiagnosisTable.jsx:37`, placeholder `soap-editor.jsx:404`). Jenis: `BUG`.

**Dampak nyata.** dr. Contoh A membuka pasien "Tn. Pseudonim D", memilih A02.1 dan A20.1, lalu
melihat Assessment kosong. Ia harus menyalin kode dan nama diagnosis secara manual. Bila lupa,
penyelesaian catatan ditolak dengan "Assessment wajib diisi" (`ConsultationValidationService.cs:79-80`).

**Rekomendasi.** Samakan penanda dengan Rawat Inap: nilai awal `""` dan reset `""`. `FIX-IGD-005-01`.

---

### ISS-IGD-005-02 — Tombol cadangan "Susun ulang blok diagnosa" tidak pernah muncul di IGD

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `use-emergency-progress-note.jsx` |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-005-02` |

**Apa yang terjadi.** Editor SOAP menampilkan petunjuk "Blok diagnosa di Assessment belum sesuai
daftar ICD-10 terbaru" beserta tombol **Susun ulang blok diagnosa**, bila nilai `assessmentOutOfSync`
benar (`soap-editor.jsx:412-424`). Tombol itu adalah jalan keluar ketika isi otomatis tidak dapat
berjalan, misalnya pada draf lama yang dibuka dari riwayat atau setelah dokter mengubah blok sendiri.

**Kenapa terjadi.** Hook Rawat Inap menghitung dan mengembalikan `assessmentOutOfSync`
(`use-inpatient-progress-note.jsx:1046-1049`, `:1078`). Hook IGD tidak menghitungnya sama sekali
(`grep`: hanya ada di hook Rawat Inap dan editor). Akibatnya nilainya selalu `undefined`, dan tombol
`rebuildAssessmentBlock` yang sudah tersedia di hook IGD (`:712-724`) tidak pernah dapat ditekan.

**Apakah ini menyimpang dari desain?** Ya. Alasannya sama dengan -01.

**Rekomendasi.** Hitung `assessmentOutOfSync` di hook IGD dengan aturan yang sama dengan Rawat Inap,
lalu kembalikan bersama nilai lain. `FIX-IGD-005-02`.

---

## 5. Tanya-jawab pelapor

> **T (tersirat):** Kenapa diagnosis yang sudah saya tambahkan tidak masuk ke Assessment?
>
> **J:** Karena fitur isi otomatisnya tidak aktif pada draf yang langsung terbuka saat tab Catatan
> Dokter dibuka. Fiturnya sudah ada dan bekerja di Rawat Inap, tetapi di IGD penanda yang
> menyalakannya diberi nilai awal yang salah. Sesudah diperbaiki, setiap diagnosis yang dipilih
> langsung tertulis di awal Assessment, dan teks yang Anda ketik di bawahnya tetap aman.

---

## 6. Temuan tambahan

Tidak ada temuan baru di luar `ISSUE-IGD-004` (T2: tombol Selesaikan tidak ada; T3: SOAP tidak dapat
dibuat bila encounter tanpa dokter). Keduanya tetap berlaku di layar ini.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada.

**Untuk pemilik:** Tidak ada. Perbaikan mengembalikan perilaku yang sudah dijanjikan layar dan sudah
berjalan di Rawat Inap, tanpa menyentuh komponen milik Rawat Inap.

---

## 8. Catatan pola

Ini temuan ketiga dalam satu hari pada hook yang sama (`ISSUE-IGD-004-04/-05/-06`, `ISSUE-IGD-005`)
dengan akar serupa: hook IGD disalin dari Rawat Inap lalu diubah sebagian, dan bagian yang berubah
tidak dicocokkan ulang dengan aslinya (bentuk argumen payload, pengambil data, nilai awal penanda,
nilai kembalian). Usul pencegahan: sebelum task berikutnya pada `use-emergency-progress-note.jsx`,
bandingkan daftar nilai kembalian dan nilai awal state/ref dengan `use-inpatient-progress-note.jsx`
baris demi baris, dan catat perbedaan yang **disengaja**.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Issue dibuat dari laporan 1 butir + 1 lampiran; 2 temuan | `diagnose-module-issue` |
| 2026-11-04 | Disetujui dan diperbaiki; status `DALAM_PERBAIKAN`. Uji browser 7/7 lulus (lihat plan repair Riwayat) | Claude Code |
| 2026-11-04 | **Koreksi** §4 "Kapan gejala tidak muncul": klaim bahwa isi otomatis aktif setelah berpindah pasien karena `startNewNote` terpanggil **keliru**. Jalur `selectNote(draf)` galat oleh `diagnosisApi.resetDraftDiagnoses is not a function` (`ISSUE-IGD-006-01`). Klaim itu berstatus DUGAAN dan tidak pernah diuji | `diagnose-module-issue` |
