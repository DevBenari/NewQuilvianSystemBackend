# ISSUE-006 — Tombol "Buka" Riwayat SOAP Dokter IGD Galat, dan SOAP Dokter Tidak Pernah Masuk CPPT

```yaml
issue_id: ISSUE-IGD-006
module_id: igd
submodule: igd
layar: "Ruang Kerja Dokter IGD (SCR-IGD-D01) — tab Catatan Dokter: segmen Riwayat SOAP dan CPPT Pasien"
sumber_laporan: "Laporan pelapor 04-11-2026: 2 butir, 3 lampiran"
tanggal_issue: "2026-11-04"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004-01/02 dan FIX-IGD-005-01/02 pada use-emergency-progress-note.jsx"
rencana_perbaikan: ../plan-repair/plan-repair-006-buka-soap-galat-cppt-dokter-kosong.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Dokter IGD sudah dapat menyimpan SOAP lengkap dengan diagnosis. Tetapi dua hal lain tidak berjalan.

1. Menekan **Buka** pada kartu riwayat SOAP memunculkan galat "diagnosisApi.resetDraftDiagnoses is
   not a function", dan catatan tidak terbuka. Ada **dua** cacat bertumpuk: layar mengirim jenis data
   yang salah ke fungsi pembuka catatan, dan fungsi itu memanggil perintah yang tidak ada. Akar yang
   sama juga membuat tombol **Reset** dan **Catatan baru** galat. Akibatnya draf yang sudah tersimpan
   tidak dapat dibuka dan dilanjutkan sama sekali (**Blocker**).
2. Sub-tab **CPPT Pasien** menampilkan "Catatan Dokter (0)". SOAP dokter IGD memang **tidak pernah
   disalin ke CPPT**. Rawat Jalan menyalinnya saat catatan diselesaikan, sedangkan IGD tidak punya
   langkah itu, dan juga tidak punya tombol Selesaikan (`ISSUE-IGD-004-T2`).

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "sudah bisa di inputkan tetapi ketika saya klik buka button di bawah itu ada error" | Lampiran 1: kartu riwayat "SOAP #1", status *Sedang ditulis*, diagnosis A02.0 (Utama) dan A17.0, isi S/O/A/P lengkap, tombol **Buka**. Lampiran 2: galat Next.js "Runtime TypeError — diagnosisApi.resetDraftDiagnoses is not a function", `use-emergency-progress-note.jsx (547:22) @ selectNote` ← `emergency-doctor-progress-tab.jsx (205:15) onSelectNote` ← `soap-history-panel.jsx (273:36) onClick` |
| 2 | "di cppt dokter juga tidak tampil kenapa ?" | Lampiran 3: CPPT Pasien — "Catatan Dokter (0) — Belum ada catatan dokter"; "Catatan Perawat (1)" tampil |

Lampiran 1 juga menunjukkan bahwa perbaikan `ISSUE-IGD-004` dan `-005` bekerja pada data pelapor.
Subjective berisi keluhan dan nyeri, Objective berisi tanda vital, dan Assessment berisi blok
diagnosis.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-006-01` | 1 | "Buka", "Reset", dan "Catatan baru" galat: id dikirim sebagai objek dan pemanggilan fungsi yang tidak ada | `BUG` | Frontend | **Blocker** | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-006-01` |
| `ISS-IGD-006-02` | 2 | SOAP dokter IGD tidak pernah disalin ke CPPT | `BUG` + keputusan pemilik | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-006-02`, `-03` |

---

## 4. Rincian per temuan

### ISS-IGD-006-01 — "Buka", "Reset", dan "Catatan baru" galat

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `emergency-doctor-progress-tab.jsx` dan `use-emergency-progress-note.jsx` (keduanya milik IGD) |
| **Keparahan** | Blocker — draf SOAP yang tersimpan tidak dapat dibuka dan dilanjutkan |
| **Status bukti** | SUDAH-VERIFIKASI di source; gejala DARI-CAPTURE lampiran 2 |
| **Perbaikan** | `FIX-IGD-006-01` |

**Apa yang terjadi.** Dokter menekan **Buka** pada kartu "SOAP #1" untuk melanjutkan draf. Layar
menampilkan galat merah dan catatan tidak terbuka.

**Kenapa terjadi.** Dua cacat bertumpuk:

```text
TypeError: diagnosisApi.resetDraftDiagnoses is not a function
  ← selectNote(undefined) masuk cabang "draf baru" (!id) lalu memanggil resetDraftDiagnoses()
    ← cacat B: hook diagnosis tidak punya resetDraftDiagnoses; namanya discardPending
  ← selectNote menerima undefined
    ← cacat A: SoapHistoryPanel mengirim id (teks), tab IGD membacanya sebagai objek (note.id)
      ← keduanya ditulis saat halaman dokter IGD dibuat (commit 905aec3bd, 8 Okt 2026)
```

1. **Cacat A — bentuk data.** Panel riwayat mengirim **id**:

   `src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/soap-history-panel.jsx:273`

   ```jsx
   onClick={() => onSelectNote?.(item.id)}
   ```

   Tab IGD memperlakukan nilai itu sebagai **objek catatan**:

   `src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-progress-tab.jsx:204-207`

   ```jsx
   onSelectNote={(note) => {
     selectNote(note.id);   // note berisi teks id, sehingga note.id = undefined
     setActiveSubTab("form");
   }}
   ```

   Rawat Inap meneruskan fungsinya langsung: `physician-progress-tab.jsx:306`, `:320` →
   `onSelectNote={selectNote}`.

2. **Cacat B — fungsi tidak ada.** `selectNote` dengan id kosong dianggap "buat draf baru" dan
   memanggil:

   `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-progress-note.jsx:547`

   ```jsx
   diagnosisApi.resetDraftDiagnoses();
   ```

   Hook diagnosis (`use-progress-note-diagnoses.jsx:695-732`) hanya menyediakan
   `discardPending` ("Membuang diagnosa yang belum tersimpan — dipakai Reset pada catatan baru",
   `:687-693`). Rawat Inap memanggil `diagnosisApi.discardPending()`
   (`use-inpatient-progress-note.jsx:612`).

**Jalur lain yang terkena cacat B** (SUDAH-VERIFIKASI dari source): setiap pemanggilan
`selectNote(PROGRESS_NOTE_DRAFT_SELECTION)`, yaitu:

- tombol **Reset** pada editor (`resetForm: startNewNote`);
- tombol **Catatan baru** pada riwayat (`onNewNote={startNewNote}`);
- dokter berpindah pasien **bila** tab tidak dipasang ulang. Ini DUGAAN: layar dokter menampilkan
  keadaan memuat saat kunjungan berganti, yang kemungkinan memasang ulang tab. Cara membuktikan: buka
  pasien A di tab Catatan Dokter, pilih pasien B dari daftar kiri, lalu amati apakah galat yang sama
  muncul.

**Koreksi atas `ISSUE-IGD-005`.** Issue itu menyebut isi otomatis Assessment "aktif setelah dokter
berpindah pasien karena `startNewNote` terpanggil". Bila jalur itu memang terpanggil, ia akan galat
oleh cacat B ini, bukan aktif. Koreksi dicatat pada Riwayat `ISSUE-IGD-005`.

**Apakah ini menyimpang dari desain?** Ya. Tab Catatan Dokter ditetapkan *Reuse with adapter*
(`03-frontend-architecture.md` §15). Daftar riwayat catatan dokter yang dapat dibuka adalah bagian
dari rancangannya. Jenis: `BUG`.

**Dampak nyata.** dr. Contoh A menyimpan draf SOAP pukul 14.14 lalu berpindah ke sub-tab lain.
Untuk melanjutkan, ia menekan **Buka** dan mendapat galat. Draf itu tidak dapat dilanjutkan maupun
diselesaikan. Satu-satunya jalan adalah menulis catatan baru dari awal, sehingga draf lama tertinggal
sebagai draf yatim.

**Rekomendasi.** Teruskan id apa adanya ke `selectNote`, dan ganti `resetDraftDiagnoses` dengan
`discardPending`. `FIX-IGD-006-01`.

---

### ISS-IGD-006-02 — SOAP dokter IGD tidak pernah disalin ke CPPT

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` (jalur tulis CPPT pada rancangan tidak dibangun) + keputusan pemilik (kapan disalin) |
| **Area** | Frontend |
| **Keparahan** | High — CPPT pasien IGD tidak memuat catatan dokter, padahal CPPT adalah catatan terintegrasi lintas profesi |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-006-02` (tombol Selesaikan), `FIX-IGD-006-03` (salin ke CPPT) |

**Apa yang terjadi.** CPPT Pasien membagi catatan menjadi Catatan Dokter dan Catatan Perawat.
Catatan perawat muncul karena perawat menulis langsung ke CPPT. SOAP dokter disimpan di tempat lain,
yaitu catatan konsultasi dokter, dan tidak pernah disalin ke CPPT.

**Kenapa terjadi.**

```text
CPPT: "Catatan Dokter (0)"
  ← CPPT hanya membaca tabel catatan terintegrasi (GET /patient-integrated-progress-notes?encounterId=)
    ← SOAP dokter tersimpan di TrxDoctorConsultation; backend tidak menyalinnya otomatis
      ← penyalinan dilakukan frontend lewat POST /patient-integrated-progress-notes/from-consultation/{id}
        ← hanya Rawat Jalan yang memanggilnya, saat catatan diselesaikan; IGD tidak pernah
          ← IGD juga tidak punya tombol Selesaikan (ISSUE-IGD-004-T2), dan SOAP pelapor masih draf
```

- `use-emergency-integrated-note.jsx:85-92`: CPPT dokter IGD membaca `getProgressNotesByEncounter`,
  yaitu tabel catatan terintegrasi saja.
- `PatientIntegratedProgressNoteController.cs:629` (`[HttpPost("from-consultation/{consultationId:guid}")]`):
  endpoint penyalin SOAP → CPPT. Backend tidak memanggilnya saat finalisasi
  (`ConsultationFinalizationService` tidak menyentuh CPPT), dan `DoctorConsultationController` tidak
  merujuk tabel CPPT.
- `useDoctorConsultationWorkspace.js:260-273`, `:326`: Rawat Jalan memanggil
  `createPatientIntegratedProgressNoteFromConsultation(..., { useCurrentUserAsProvider: true,
  isReadOnlyGenerated: false }, { ignoreDuplicate: true })` **setelah** konsultasi diselesaikan.
- `grep`: fungsi itu tidak dipakai di modul IGD maupun Rawat Inap.
- SOAP pelapor berstatus *Sedang ditulis* (lampiran 1). Dengan pola Rawat Jalan pun, draf belum masuk
  CPPT. Draf baru masuk setelah diselesaikan.

**Apakah ini menyimpang dari desain?** Ya, sebagian. `03-frontend-architecture.md` §15 baris tab
Catatan Dokter menyebut "CPPT (**dibaca dan ditulis**)", dan `IGD-DEC-221` membawa "Catatan Dokter
(SOAP) beserta CPPT". Jalur tulis CPPT dari layar dokter IGD tidak dibangun sama sekali. **Kapan**
SOAP masuk CPPT (saat diselesaikan atau saat disimpan) belum tertulis untuk IGD, jadi itu butuh
keputusan (K-01).

**Dampak nyata.** Perawat dan dokter shift berikutnya membuka CPPT pasien "Tn. Pseudonim D" dan hanya
melihat catatan perawat. Penilaian dokter (diagnosis A02.0, rencana terapi) tidak tampil di catatan
terintegrasi.

**Rekomendasi.** Sediakan tombol **Selesaikan** di tab Catatan Dokter IGD (`FIX-IGD-006-02`, berkas
milik IGD). Setelah penyelesaian berhasil, salin SOAP ke CPPT dengan pola Rawat Jalan
(`FIX-IGD-006-03`), lalu muat ulang CPPT.

---

## 5. Tanya-jawab pelapor

> **T:** Kenapa ketika klik Buka ada error?
>
> **J:** Karena tombol Buka mengirim nomor catatan, tetapi layar IGD membacanya seolah-olah isinya
> seluruh catatan, sehingga nomornya hilang. Tanpa nomor, layar mengira dokter ingin membuat catatan
> baru, lalu memanggil perintah pembersih diagnosis yang namanya salah. Dua kesalahan kecil itu
> membuat galat. Tombol Reset dan Catatan baru terkena hal yang sama.

> **T:** Kenapa di CPPT dokter tidak tampil?
>
> **J:** Karena SOAP dokter disimpan di catatan konsultasi dokter, sedangkan CPPT membaca tabel
> catatan terintegrasi. Di Rawat Jalan, SOAP disalin ke CPPT saat dokter menyelesaikan catatan. Di
> IGD langkah salin itu belum ada, dan tombol Selesaikan juga belum ada. SOAP Anda juga masih berstatus
> draf ("Sedang ditulis").

---

## 6. Temuan tambahan

`ISSUE-IGD-004-T2` (tidak ada tombol Selesaikan) menjadi bagian dari `FIX-IGD-006-02` untuk sisi
IGD. Sisi Rawat Inap tetap terbuka. Temuan berikut muncul saat mengerjakan dan menguji perbaikan
(4 Nov 2026). Semuanya SUDAH-VERIFIKASI di source.

| ID | Temuan | Keparahan | Status |
| --- | --- | --- | --- |
| `ISS-IGD-006-T1` | Modal **Selesaikan** di tab IGD memakai props yang salah (`isOpen`, `onClose`, `submitting`, `documentTitle`), padahal `CompleteDocumentModal` membaca `show`, `onCancel`, `loading`, `documentLabel`. Modal tidak pernah terbuka | High | **Diperbaiki** dalam `FIX-IGD-006-02` (prasyaratnya) |
| `ISS-IGD-006-T2` | Simpan/Selesaikan draf yang **dibuka ulang** tidak memanggil `diagnosisApi.persistPending`, berbeda dari Rawat Inap (`use-inpatient-progress-note.jsx:859-865`, `:940-946`). Diagnosis baru pada draf lama bisa hilang | High | **Diperbaiki** dalam `FIX-IGD-006-02` |
| `ISS-IGD-006-T3` | Draf yang dibuka ulang tidak menerima blok ICD-10 di Assessment: IGD selalu memberi penanda `null`, Rawat Inap memberi `""` bila catatan belum berdiagnosis (`use-inpatient-progress-note.jsx`, cabang `selectNote` catatan ada) | Medium | **Diperbaiki** — perluasan `FIX-IGD-005-01` |
| `ISS-IGD-006-T4` | Modal **Koreksi (addendum)** dan modal **Salin dari catatan sebelumnya** di tab IGD juga memakai props yang salah (`isOpen`/`onClose`/`submitting`, `confirmText`/`cancelText`), sehingga tidak dapat terbuka. Selain itu `handleCorrectionSubmit` membaca `payload.notes`/`payload.text`, padahal modal mengirim `{ addendumText, correctionReason }`. Hook IGD juga mengirim `{ notes }` ke API addendum, padahal Rawat Inap mengirim `{ addendumText, correctionReason }` | High | **Belum** — di luar laporan; usul issue baru |
| `ISS-IGD-006-T6` | Server hanya mengizinkan **satu** catatan dokter terbuka per kunjungan IGD (`DoctorConsultationController.cs:1570`, "Masih ada draf catatan dokter yang belum diselesaikan…"). Tetapi tab Catatan Dokter selalu membuka **draf baru kosong**, bukan draf terbuka yang sudah ada. Dokter bisa menulis catatan lengkap lalu baru ditolak saat menyimpan. Contoh: pasien yang masih punya SOAP #1 "Sedang ditulis" | Medium | **Belum** — usul: buka otomatis draf terbuka milik dokter, atau beri tahu sebelum menulis |
| `ISS-IGD-006-T5` | Kepala CPPT dokter IGD (`DoctorCpptHeader`) menerima `title`/`subtitle`/`filterControl`/`actionControl`, padahal komponen membaca `totalRows`, `patientName`, `medicalRecordNumber`, `onRefresh`. Akibatnya tampil "0 catatan" dan "No. RM -" walau CPPT berisi, dan penyaring Dokter/Perawat tidak tampil di kepala | Low | **Belum** — usul issue baru |

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Kapan SOAP dokter IGD masuk CPPT? | Saat **diselesaikan** (seperti Rawat Jalan) / saat setiap kali disimpan | **Saat diselesaikan.** CPPT adalah catatan sah lintas profesi; draf yang masih berubah tidak boleh tampil sebagai catatan. Satu SOAP = satu entri CPPT | `FIX-IGD-006-03` |

---

## 8. Catatan pola

Ini temuan keempat hari ini dengan akar sama: adaptasi Rawat Inap → IGD pada commit `905aec3bd`
mengubah antarmuka tanpa mencocokkan pemanggilnya (bentuk argumen `buildProgressNoteSoapPayload`, nilai
awal penanda, `onSelectNote(note)` lawan `onSelectNote(id)`, nama fungsi `discardPending`). Usul
pencegahan pada `ISSUE-IGD-005` §8 makin mendesak: periksa seluruh hook IGD `use-emergency-*`
terhadap padanan Rawat Inapnya, dan uji setiap tombol pada tab dokter IGD sekali jalan.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Issue dibuat dari laporan 2 butir + 3 lampiran; 2 temuan; koreksi atas klaim `ISSUE-IGD-005` dicatat | `diagnose-module-issue` |
| 2026-11-04 | Rizki Gunawan menyetujui rencana dan K-01 (SOAP masuk CPPT saat diselesaikan); status `DALAM_PERBAIKAN`. Saat dikerjakan ditemukan dan diperbaiki T1–T3; T4–T5 dicatat, belum diperbaiki | Claude Code |
| 2026-11-04 | Keputusan pemilik: IGD memakai **satu tombol** simpan yang langsung menyelesaikan dan memasukkan catatan ke CPPT (`FIX-IGD-006-04`). Ditambahkan `ISS-IGD-006-T6` (satu draf terbuka per kunjungan, tetapi layar membuka draf baru) | Claude Code |
