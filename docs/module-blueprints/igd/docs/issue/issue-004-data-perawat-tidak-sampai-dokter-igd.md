# ISSUE-004 — Data Perawat IGD Tidak Sampai ke Ruang Kerja Dokter, dan SOAP Dokter Kehilangan Isinya Saat Disimpan Ulang

```yaml
issue_id: ISSUE-IGD-004
module_id: igd
submodule: igd
layar: "Ruang Kerja Dokter IGD (SCR-IGD-D01) — tab Pengkajian Medis dan tab Catatan Dokter (SOAP)"
sumber_laporan: "Laporan pelapor 04-11-2026: 3 butir, tanpa lampiran"
tanggal_issue: "2026-11-04"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2)"
rencana_perbaikan: ../plan-repair/plan-repair-004-data-perawat-tidak-sampai-dokter-igd.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Pelapor menyampaikan tiga keluhan pada **Ruang Kerja Dokter IGD**. Pertama, keluhan utama, tanda
vital, pengkajian nyeri, dan diagnosis ICD-10 yang sudah diisi perawat tidak tampil di **Pengkajian
Medis** dokter. Kedua, kolom **Subjective** pada SOAP dokter tidak terisi pengkajian nyeri dari
perawat. Ketiga, kolom **Objective** tidak terisi tanda vital dan isinya tidak tersimpan.

Penelusuran menemukan enam masalah berbeda di balik tiga keluhan itu. Yang **paling berbahaya**:
setiap kali dokter menyimpan ulang draf SOAP atau menyelesaikan draf yang sudah diubah, seluruh isi
Subjective, Objective, Assessment, dan Plan **terhapus** di server. Penyebabnya, layar mengirim
isian dalam bentuk yang salah. Setelah isinya terhapus, server menolak penyelesaian dengan pesan
"Objective atau pemeriksaan fisik wajib diisi", padahal dokter sudah mengisinya.

Tiga masalah lain adalah cacat program: SOAP dokter tidak pernah mengambil data nyeri dan tanda
vital pasien, dan tanda vital di Pengkajian Medis tidak pernah tersimpan. Dua masalah sisanya
(keluhan dan nyeri tidak terisi otomatis di Pengkajian Medis, serta ICD-10 dari perawat) memang
**sesuai rancangan sekarang**. Mengubahnya butuh keputusan pemilik dan keputusan klinis.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "keluhan utama vitalsign pain assessment diagnosisi icd ten pasien di igd yang sudah di input di perawat tidak tampil di pengkajian awal dokter" | — |
| 2 | "lalu di soap dokter field subjective tidak terisi painAssesment pasien yang sudah diinput" | — |
| 3 | "obejctve nyaa juga tidak tersimpan vitalSign pasien nyaa" | — |

"Pengkajian awal dokter" ditafsirkan sebagai tab **Pengkajian Medis** (kajian medis awal), dan "SOAP
dokter" sebagai tab **Catatan Dokter**. Keduanya adalah nama tab pada
`emergency-physician-constant.jsx:24-31`. Laporan tidak berlampiran, sehingga keadaan layar yang
dilihat pelapor ditanyakan pada bagian 7.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-004-06` | 3 | Simpan ulang / Selesaikan draf SOAP dokter mengosongkan S, O, A, dan P | `BUG` | Frontend | **High** | SUDAH-VERIFIKASI (source FE + BE) | `FIX-IGD-004-01` |
| `ISS-IGD-004-04` | 2 | Subjective tidak pernah terisi pengkajian nyeri: pengambil datanya tidak ada | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-004-02` |
| `ISS-IGD-004-05` | 3 | Objective tidak pernah terisi tanda vital: akar yang sama dengan -04 | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-004-02` |
| `ISS-IGD-004-01` | 1 | Tanda vital di Pengkajian Medis dokter tidak tersimpan dan tidak tampil lagi | `BUG` | Frontend (komponen bersama Rawat Inap) | High | SUDAH-VERIFIKASI | `FIX-IGD-004-03` |
| `ISS-IGD-004-02` | 1 | Keluhan utama dan nyeri perawat tidak terisi otomatis di formulir Pengkajian Medis | `DESIGN_CHANGE` + `PERLU_KONFIRMASI` | Frontend | Medium | SUDAH-VERIFIKASI (perilaku); DUGAAN (yang dilihat pelapor) | `FIX-IGD-004-04` |
| `ISS-IGD-004-03` | 1 | ICD-10 pilihan perawat hanya menjadi teks pesanan Lab, bukan diagnosis pasien | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-IGD-004-05` |
| `ISS-IGD-004-T1` | — | Tanda vital ukuran dokter IGD tidak masuk deret tanda vital pasien | `BUG` | Backend | Low | SUDAH-VERIFIKASI | — (dicatat) |
| `ISS-IGD-004-T2` | — | SOAP dokter tidak punya tombol Selesaikan (IGD dan Rawat Inap) | `BUG` | Frontend (komponen bersama Rawat Inap) | High | SUDAH-VERIFIKASI | — (usul issue baru) |
| `ISS-IGD-004-T3` | — | SOAP tidak dapat dibuat untuk pasien IGD yang encounter-nya belum punya dokter | `CONTRACT_MISMATCH` | Frontend + Backend | Medium | SUDAH-VERIFIKASI (source + uji browser) | — (usul issue baru) |

Urutan tabel mengikuti keparahan dan urutan pengerjaan, bukan nomor ID.

---

## 4. Rincian per temuan

### ISS-IGD-004-06 — Simpan ulang / Selesaikan draf SOAP dokter mengosongkan S, O, A, dan P

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High — data klinis terhapus tanpa disadari |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend dan backend; kejadian di data pelapor DUGAAN (cara membuktikan pada bagian 7) |
| **Perbaikan** | `FIX-IGD-004-01` |

**Apa yang terjadi.** Dokter membuat catatan SOAP baru dan menekan *Simpan Draf*. Penyimpanan
pertama ini benar. Bila dokter lalu menambah isi dan menekan *Simpan* lagi, atau menekan
*Selesaikan* setelah mengubah draf, server menerima Subjective, Objective, Assessment, dan Plan
dalam keadaan **kosong** dan menghapus isi yang sudah tersimpan. Saat diselesaikan, server menolak
dengan pesan "Subjective atau keluhan utama wajib diisi" dan "Objective atau pemeriksaan fisik
wajib diisi". Dokter melihat Objective-nya hilang.

**Kenapa terjadi.**

```text
Objective (dan S/A/P) kosong setelah simpan ulang
  ← PATCH /doctor-consultations/{id}/soap mengirim "subjective": "", "objective": "", …
    ← buildProgressNoteSoapPayload dipanggil dengan bentuk argumen yang salah
      ← hook IGD menyalin pola pembuat payload "buat baru" (satu objek) untuk payload "ubah"
        (dua argumen) sejak halaman dokter IGD dibuat, commit 905aec3bd, 8 Okt 2026
```

1. Fungsi pembentuk payload menerima **dua** argumen, `(form, options)`:

   `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx:1020-1031`

   ```jsx
   export const buildProgressNoteSoapPayload = (form = {}, options = {}) => ({
     subjective: toText(form.subjective).trim(),
     objective: toText(form.objective).trim(),
     assessment: toText(form.assessment).trim(),
     plan: toText(form.plan).trim(),
     // …
   });
   ```

2. Hook SOAP dokter IGD memanggilnya dengan **satu** objek pembungkus, di jalur Simpan (`:706`) dan
   Selesaikan (`:771`):

   `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-progress-note.jsx:704-711`

   ```jsx
   await patchDoctorConsultationSoap(
     selectedId,
     buildProgressNoteSoapPayload({
       form: currentForm,
       vitalSign: { mode: VITAL_SIGN_MODE.MANUAL },
       planFields: null,
     }),
   );
   ```

   Pembungkus itu diperlakukan sebagai `form`, sehingga `form.objective` tidak ada dan semua kolom
   menjadi `""`. Pada Rawat Inap fungsi yang sama dipanggil dengan benar:
   `use-inpatient-progress-note.jsx:852` dan `:933` → `buildProgressNoteSoapPayload(currentForm, {…})`.

3. Backend memperlakukan teks kosong sebagai "kosongkan", bukan "biarkan":

   `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs:2302-2308`

   ```csharp
   if (request.Subjective != null)
       entity.Subjective = NormalizeNullableText(request.Subjective);
   if (request.Objective != null)
       entity.Objective = NormalizeNullableText(request.Objective);
   ```

4. Penyelesaian ditolak karena S/O/A/P sudah kosong: `ConsultationValidationService.cs:75-81`.

**Apakah ini menyimpang dari desain?** Ya. `03-frontend-architecture.md` §15 menetapkan tab Catatan
Dokter sebagai *Reuse with adapter* dari catatan dokter Rawat Inap. Perilaku Rawat Inap menyimpan
isi SOAP dengan benar. Jenis: `BUG`.

**Dampak nyata.** Contoh: dr. Contoh A menulis SOAP untuk "Tn. Pseudonim C" pukul 10.00 lalu
menyimpan draf (tersimpan). Pukul 10.20 ia menambah hasil laboratorium ke Objective dan menekan
*Selesaikan*. Server menghapus S/O/A/P, lalu menolak penyelesaian. Satu-satunya cara catatan dapat
diselesaikan adalah membuat catatan baru dan langsung menekan *Selesaikan* tanpa menyimpan draf lebih
dulu.

**Rekomendasi.** Perbaiki dua pemanggilan menjadi `buildProgressNoteSoapPayload(currentForm, { … })`.
Rinciannya ada di `FIX-IGD-004-01`.

---

### ISS-IGD-004-04 — Subjective tidak pernah terisi pengkajian nyeri: pengambil datanya tidak ada

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` (implementasi belum selesai) |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-004-02` |

**Apa yang terjadi.** Hook SOAP dokter IGD sudah memiliki seluruh logika untuk mengisi Subjective
dari pengkajian nyeri perawat. Contohnya: "Keluhan utama: …; Nyeri skala 6, lokasi dada kiri, …".
Hook itu juga sudah menyiapkan tombol "Sisipkan dari catatan perawat: Nyeri 6/10". Tetapi data
nyerinya **tidak pernah diambil** dari server, sehingga semua logika itu tidak pernah berjalan.

**Kenapa terjadi.**

```text
Subjective kosong, tombol "Sisipkan dari catatan perawat" tidak muncul
  ← latestPainAssessment selalu null
    ← tidak ada satu baris pun yang memanggil setLatestPainAssessment / setLatestVitalSign
      ← commit 5bb240c65 (2 Nov 2026) menambahkan state, pengisi otomatis, dan tombolnya,
        tetapi tidak menambahkan kode pengambil data
```

- `use-emergency-progress-note.jsx:196-205`: state `latestVitalSign`, `latestPainAssessment`,
  `vitalLoading`, dan `vitalAndPainToken` dideklarasikan. Pengubahnya tidak pernah dipanggil di
  berkas mana pun (diperiksa dengan `grep`).
- `:363-381`, `:390-399`: pengisi otomatis S/O pada draf baru hanya berjalan **bila** data itu ada.
- `:606-622`: tombol salin langsung berhenti (`return`) karena datanya `null`.
- `:5`: `InstanceAxios` diimpor tetapi tidak dipakai. Ini jejak pemanggil yang belum ditulis.
- Sebelum commit itu, tanda vital pada hook ini hanya berupa stub kosong
  `{ items: [], loading: false }`. Jadi fungsi ini belum pernah berjalan di IGD.

**Apakah ini menyimpang dari desain?** Ya. Tab Catatan Dokter ditetapkan *Reuse with adapter*
(`03-frontend-architecture.md` §15). Versi Rawat Inap mengambil tanda vital dan nyeri lewat
`use-progress-note-vital-signs.jsx`, tetapi hanya berdasarkan **episode** rawat inap. Adapter untuk
IGD, yang berdasarkan **encounter**, belum dibuat.

**Dampak nyata.** Dokter harus membuka layar perawat atau bertanya langsung untuk mengetahui skala
dan lokasi nyeri, lalu mengetik ulang.

**Rekomendasi.** Tambahkan pengambil data berbasis encounter. Sumbernya pengkajian keperawatan
terbaru (rincian nyeri dibaca dari **detail** dokumen, karena respons daftar hanya membawa `hasPain`
dan `painScale`; lihat `PatientAssessmentController.cs:3209-3210` lawan `:3308-3314`) dan tanda
vital terbaru per encounter. `FIX-IGD-004-02`.

---

### ISS-IGD-004-05 — Objective tidak pernah terisi tanda vital: akar yang sama dengan -04

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-004-02` |

**Apa yang terjadi.** Objective pada draf baru seharusnya berisi otomatis tanda vital terbaru
(misalnya "TD 130/85 mmHg, Nadi 92 x/menit, RR 20 x/menit, Suhu 37,8 °C, SpO₂ 97%"). Karena
`latestVitalSign` tidak pernah diambil, Objective selalu kosong.

**Kenapa terjadi.** Sama dengan `ISS-IGD-004-04`: `buildObjectiveFromVitalSign` dan pengisinya
(`:369-371`, `:394-396`, `:615-622`) tidak pernah menerima data.

**Apakah ini menyimpang dari desain?** Ya, alasannya sama dengan -04.

**Dampak nyata.** Bersama `ISS-IGD-004-06`, dokter mengalami dua hal. Objective tidak pernah
terisi otomatis, dan isi yang ia ketik sendiri terhapus saat disimpan ulang. Itulah yang dilaporkan
sebagai "objective tidak tersimpan vital sign".

**Rekomendasi.** Diselesaikan bersama -04 oleh `FIX-IGD-004-02`.

---

### ISS-IGD-004-01 — Tanda vital di Pengkajian Medis dokter tidak tersimpan dan tidak tampil lagi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — komponen **milik Rawat Inap** (`physician-workspace/tabs/assessment`), dipakai ulang IGD |
| **Keparahan** | High — dokter mengira tanda vital tersimpan padahal tidak |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-004-03` |

**Apa yang terjadi.** Bagian "2. Tanda Vital & Kondisi Umum" pada formulir Pengkajian Medis punya
isian TD, Nadi, Pernapasan, Suhu, dan Saturasi. Ada juga tombol **Sinkronkan Tanda Vital dari
Perawat**. Dokter dapat menekan tombol itu atau mengetik sendiri, dan angkanya tampil. Tetapi
begitu disimpan lalu dibuka lagi, isian tanda vital kembali kosong, dan tampilan baca-saja
menunjukkan "- mmHg".

**Kenapa terjadi.**

```text
Tanda vital kajian dokter tampil "-" setelah disimpan
  ← buildFormFromDetail tidak membaca tanda vital dari detail ke isian tandaVital*
  ← buildMedicalTextPayload / buildMedicalAssessmentCreatePayload tidak mengirim tandaVital*
    ← isian tandaVital* ditambahkan ke formulir (commit ff11419aa, 30 Sep 2026)
      tanpa disambungkan ke pembentuk payload maupun pembaca detail
```

- `medical-assessment-form.jsx:83-97`: bagian tanda vital memuat `tandaVitalTd`, `tandaVitalNadi`,
  `tandaVitalPernapasan`, `tandaVitalSuhu`, dan `tandaVitalSaturasi`.
- `medical-assessment-tab.jsx:192-203`: tombol sinkron mengisi kelima isian itu dari rujukan perawat.
- `inpatient-medical-assessment-utils.jsx`: kata `tandaVital` **tidak muncul sama sekali**, baik di
  `buildFormFromDetail` (`:378`), `buildMedicalTextPayload` (`:507`), maupun payload buat baru.
  Payload ubah (`:579`) bahkan mengirim ulang angka lama dari server (`detail.vitals`, `:282`),
  sehingga angka baru dari layar tidak pernah sampai.

**Apakah ini menyimpang dari desain?** Ya. Isian yang dapat diisi dan diberi tombol sinkron jelas
dimaksudkan untuk tersimpan. Komponen ini milik Rawat Inap (pemilik: Muhammad Hamzah,
`RWI-DEC-061`), jadi **cacat yang sama juga ada di Ruang Kerja Dokter Rawat Inap**.

**Dampak nyata.** dr. Contoh A menekan *Sinkronkan Tanda Vital dari Perawat* dan TD 130/85 tampil.
Ia menyimpan lalu menyelesaikan kajian. Dokumen kajian medis tersimpan **tanpa** tanda vital, dan
siapa pun yang membukanya melihat "- mmHg".

**Rekomendasi.** Petakan `tandaVital*` ke kolom `bloodPressureSystolic`, `bloodPressureDiastolic`,
`pulseRate`, `respiratoryRate`, `temperature`, dan `oxygenSaturation`, baik saat menyimpan maupun
saat membaca detail. Kolom-kolom ini sudah ada di `TrxPatientAssessment` dan dipakai perawat. Butuh
persetujuan pemilik Rawat Inap (K-01). `FIX-IGD-004-03`.

---

### ISS-IGD-004-02 — Keluhan utama dan nyeri perawat tidak terisi otomatis di formulir Pengkajian Medis

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` + `PERLU_KONFIRMASI` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | Perilaku SUDAH-VERIFIKASI; apa yang dilihat pelapor DUGAAN |
| **Perbaikan** | `FIX-IGD-004-04` (menunggu K-02 dan P-01/P-02) |

**Apa yang terjadi menurut source.** Data perawat pada Pengkajian Medis tampil dengan dua cara:

1. **Kotak rujukan keperawatan** (`nursing-assessment-reference.jsx`). Isinya baris ringkas TD,
   Nadi, Suhu, SpO₂, dan Nyeri, ditambah rincian keluhan yang **tertutup** secara bawaan
   (`isExpanded = false`, `:38`).
2. **Tombol salin manual**: *Sinkronkan Tanda Vital dari Perawat* dan *Salin Keluhan Utama Perawat*
   (`medical-assessment-form.jsx:406-426`, `:627-645`).

Formulir dokter **tidak** terisi otomatis. Formulir itu juga **tidak punya isian nyeri**, sehingga
nyeri hanya terbaca di kotak rujukan.

**Kenapa pelapor bisa melihat "tidak tampil".** Ada tiga kemungkinan (DUGAAN):

| Kemungkinan | Penjelasan | Cara membuktikan |
| --- | --- | --- |
| A | Pelapor mengharapkan isian formulir terisi sendiri, padahal rancangannya tombol salin | Screenshot tab Pengkajian Medis (P-01) |
| B | Data diisi perawat di **Triage** atau **Observasi**, bukan di Assesmen Awal IGD. Kotak rujukan hanya membaca dokumen **Assesmen Awal** (`normalizeNursingReference` menyaring tipe 0, `inpatient-medical-assessment-utils.jsx:601-608`) | Jawaban P-02 |
| C | Tanda vital sudah disinkronkan tetapi hilang setelah disimpan | Ini `ISS-IGD-004-01`, sudah terbukti |

**Apakah ini menyimpang dari desain?** Tidak. `03-frontend-architecture.md` §15 dan teks tab
(`emergency-physician-constant.jsx:61-63`, "Dilengkapi rujukan pengkajian keperawatan IGD")
menetapkan data perawat sebagai **rujukan**, bukan isian otomatis. Mengubahnya adalah `DESIGN_CHANGE`.

**Rekomendasi.** Untuk IGD, saat dokter membuat kajian medis awal baru, isi otomatis isian yang
**masih kosong** dengan keluhan utama dan tanda vital perawat, beri penanda sumbernya, dan
tampilkan nyeri di bagian anamnesis. Perilaku Rawat Inap tetap seperti sekarang, diatur lewat prop
konteks. Menunggu K-02.

---

### ISS-IGD-004-03 — ICD-10 pilihan perawat hanya menjadi teks pesanan Lab, bukan diagnosis pasien

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-004-05` (menunggu K-03) |

**Apa yang terjadi.** Di layar perawat, ICD-10 dipilih pada panel **Laboratorium** lewat kotak
"Diagnosa Terpilih". Pilihan itu **ditempelkan sebagai teks** ke catatan diagnosis klinis pesanan
Lab, misalnya "• A09 — Diare dan gastroenteritis". Teks itu tidak pernah menjadi data diagnosis
pasien. Pengkajian Medis dan kartu pasien dokter membaca diagnosis dari data diagnosis pasien per
encounter, jadi hasilnya kosong.

**Kenapa terjadi.**

- `emergency-assessment-procedure-catalog-table.jsx:353-364`: `handleAddIcdToDiagnosis` hanya
  menyambung teks (`"• " + label`).
- `emergency-assessment-lab-panel.jsx:97-98`, `:199`, `:295-297`: teks itu dikirim sebagai
  `clinicalDiagnosis` pada pesanan Lab.
- `use-emergency-physician-workspace.jsx:156-196`: dokter membaca `GET /patient-diagnoses?encounterId=…`.
- Tidak ada layar perawat IGD yang membuat data diagnosis pasien (`grep` pada
  `emergency-installation-management`).

**Apakah ini menyimpang dari desain?** Tidak. Rancangan §15 menyebut diagnosis berasal dari
"diagnosis per encounter" yang ditulis dokter lewat Pengkajian Medis atau Catatan Dokter. Diagnosis
medis lazimnya kewenangan dokter. Menjadikan ICD-10 perawat sebagai diagnosis pasien, atau sekadar
menampilkannya ke dokter, adalah keputusan klinis (K-03).

**Rekomendasi.** Tampilkan teks diagnosis klinis dari pesanan penunjang perawat sebagai **rujukan
baca-saja** di bagian diagnosis Pengkajian Medis. Dokter tetap menetapkan diagnosis lewat pencarian
ICD-10 yang sudah ada. Jangan membuat diagnosis pasien otomatis dari pilihan perawat.

---

## 5. Tanya-jawab pelapor

Pelapor tidak mengajukan pertanyaan eksplisit. Pertanyaan tersirat "kenapa data perawat tidak
tampil dan objective tidak tersimpan" dijawab di bagian 1. Ringkasnya:

> **T:** Kenapa objective-nya tidak tersimpan?
>
> **J:** Karena setiap simpan ulang atau penyelesaian draf mengirim isi SOAP dalam keadaan kosong,
> lalu server mengosongkan kolomnya. Ini cacat program sejak halaman dokter IGD dibuat. Selain itu,
> Objective memang tidak pernah terisi otomatis karena pengambil data tanda vitalnya belum pernah
> ditulis.

---

## 6. Temuan tambahan

### ISS-IGD-004-T1 — Tanda vital ukuran dokter IGD tidak masuk deret tanda vital pasien

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `BUG` (kemungkinan keterbatasan rancangan) |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | — (dicatat; tidak dimasukkan rencana ini) |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri penyimpanan tanda vital pada SOAP dokter (butir 3) |

`DoctorConsultationVitalSignService.cs:169-172` langsung berhenti bila catatan dokter tidak punya
`InpEpisodeId`. Akibatnya, tanda vital yang **diukur dokter** pada catatan IGD tidak pernah masuk
deret tanda vital pasien yang dibaca perawat. Layar IGD saat ini belum menyediakan isian tanda vital
ukuran dokter (state `vitalForm` tidak dapat diubah dan `formToVitalRow` tidak dipakai,
`use-emergency-progress-note.jsx:73-82`, `:177`), jadi belum ada dampak nyata. Dicatat untuk
keputusan berikutnya bila isian itu ditambahkan.

### ISS-IGD-004-T2 — SOAP dokter tidak punya tombol Selesaikan

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `BUG` |
| **Area** | Frontend — `soap-editor.jsx` milik Rawat Inap, dipakai IGD |
| **Keparahan** | High — catatan dokter tidak dapat diselesaikan dan ditandatangani dari layar |
| **Status bukti** | SUDAH-VERIFIKASI (source dan uji browser 4 Nov 2026) |
| **Perbaikan** | — (usul issue baru bersama pemilik Rawat Inap) |
| **Kenapa dicantumkan** | Ditemukan saat menguji `FIX-IGD-004-01`. Jalur Selesaikan yang diperbaiki tidak dapat dijangkau |

**Apa yang terjadi.** Bilah aksi SOAP dokter hanya berisi *Reset* dan *Simpan Catatan CPPT*. Hook
IGD dan Rawat Inap sudah punya fungsi penyelesaian (`completeNote`) beserta modal konfirmasinya,
dan tab dokter mengirim `onComplete` ke editor. Tetapi `soap-editor.jsx:86` hanya menerima
`onComplete` dan tidak memasangnya ke tombol mana pun.

**Dampak nyata.** Catatan dokter IGD yang ditulis dari layar ini tetap berstatus draf. Catatan itu
tidak pernah terkunci sebagai dokumen bertanda tangan, sehingga isinya masih dapat berubah setelah
pasien ditangani.

### ISS-IGD-004-T3 — SOAP tidak dapat dibuat untuk pasien IGD yang encounter-nya belum punya dokter

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend + Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI (source dan uji browser: `400` pada pasien uji B9CDD2) |
| **Perbaikan** | — (usul issue baru) |
| **Kenapa dicantumkan** | Ditemukan saat menguji `FIX-IGD-004-01`; dapat dikira pelapor sebagai "SOAP tidak tersimpan" |

**Apa yang terjadi.** Pada pasien yang kartu dokternya masih "Belum ada DPJP", dokter menekan
*Simpan Catatan CPPT* dan mendapat pesan "Dokter pemeriksa wajib diisi untuk konsultasi tanpa
antrean." Catatannya tidak tersimpan.

**Kenapa terjadi.** Backend mengambil dokter dari `request.DoctorId ?? encounter.DoctorId`
(`DoctorConsultationController.cs:1478-1481`). Layar sengaja tidak mengirim `doctorId`. Komentar di
atas `buildProgressNoteCreatePayload` menyebut dokter "ditentukan backend dari pengguna yang masuk",
tetapi backend tidak melakukannya. Kedua sisi saling mengandalkan.

**Rekomendasi.** Putuskan bersama pemilik: backend memakai dokter dari sesi bila permintaan dan
encounter kosong, atau layar menahan penulisan SOAP dan mengarahkan dokter menetapkan DPJP lebih
dulu, disertai alasannya.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Kirim screenshot tab **Pengkajian Medis** dokter untuk pasien yang dikeluhkan. Apakah kotak "Referensi pengkajian keperawatan" tampil, dan apakah isinya kosong? | Membedakan kemungkinan A/B/C pada `ISS-IGD-004-02` | `FIX-IGD-004-04` belum dapat dirinci; perbaikan lain tetap jalan |
| P-02 | Di layar perawat mana keluhan, tanda vital, dan nyeri pasien itu diisi: **Assesmen Awal IGD**, **Triage**, atau **Observasi**? | Rujukan dokter hanya membaca Assesmen Awal | Sama dengan P-01 |
| P-03 | ICD-10 yang dimaksud diisi lewat kotak "Diagnosa Terpilih" di panel **Laboratorium** perawat? | Satu-satunya tempat perawat IGD memilih ICD-10 menurut source | `FIX-IGD-004-05` dapat salah sasaran |
| P-04 | Apakah dokter sempat menekan *Simpan* lebih dari sekali, atau *Selesaikan* setelah mengubah draf? | Memastikan `ISS-IGD-004-06` terjadi pada data pelapor | Tidak menahan perbaikan |

Untuk P-04, query baca-saja ini memperlihatkan catatan dokter yang isinya terhapus (ganti
`<encounter-id>`):

```sql
SELECT "Id", "ConsultationStatus", "UpdateDateTime",
       length(coalesce("Subjective",'')) AS panjang_s,
       length(coalesce("Objective",''))  AS panjang_o,
       length(coalesce("Assessment",'')) AS panjang_a,
       length(coalesce("Plan",''))       AS panjang_p
FROM public."TrxDoctorConsultation"
WHERE "EncounterId" = '<encounter-id>'
ORDER BY "UpdateDateTime" DESC;
```

Catatan berstatus draf dengan keempat panjang `0` padahal dokter pernah mengisinya adalah korban
`ISS-IGD-004-06`.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Izin mengubah komponen bersama Pengkajian Medis milik Rawat Inap (pemilik Muhammad Hamzah) | Setujui / tolak (IGD membuat adapter sendiri) | **Setujui**. Perbaikannya memulihkan penyimpanan tanda vital di **kedua** modul, tanpa mengubah perilaku lain | `FIX-IGD-004-03`, `FIX-IGD-004-04` |
| K-02 | Untuk IGD: apakah keluhan utama, tanda vital, dan nyeri perawat **terisi otomatis** di formulir Pengkajian Medis dokter? | Otomatis untuk isian kosong / tetap tombol salin / rujukan saja | **Otomatis untuk isian yang masih kosong** saat kajian baru dibuat, dengan penanda "dari pengkajian perawat"; dokter tetap dapat mengubah | `FIX-IGD-004-04` |
| K-03 | ICD-10 yang dipilih perawat: bagaimana sampai ke dokter? | Menjadi diagnosis pasien otomatis / rujukan baca-saja yang dikonfirmasi dokter / tetap hanya di pesanan | **Rujukan baca-saja**; dokter menetapkan diagnosis sendiri lewat pencarian ICD-10. Diagnosis medis adalah kewenangan dokter | `FIX-IGD-004-05` |

---

## 8. Catatan pola

- **Pemanggilan fungsi bersama dengan bentuk argumen berbeda.** `buildProgressNoteCreatePayload`
  menerima satu objek, sedangkan `buildProgressNoteSoapPayload` menerima dua argumen. Hook IGD
  menyamaratakan keduanya. Pencegahan: samakan bentuk kedua fungsi, atau beri pemeriksaan bentuk
  argumen di pembentuk payload.
- **State "disiapkan" tanpa pengisi.** Ini pola yang sama dengan `ISSUE-IGD-003` (`reload()`
  hilang): fungsi dianggap bekerja karena konsumennya ada, padahal sumber datanya tidak pernah
  dipanggil. Pencegahan: uji browser untuk setiap jalur "terisi otomatis".
- **Isian layar tanpa pemetaan payload** (`tandaVital*`). Ini terjadi pada komponen bersama, jadi
  cacatnya ikut ke modul lain. Pencegahan: daftar periksa build frontend — setiap isian baru wajib
  punya pasangan di payload simpan **dan** pembaca detail.
- Issue Rawat Inap yang berdekatan: `rawat-inap/dokter-rawat-inap/docs/issue/issue-007` (penyelesaian
  SOAP mengunci transaksi). Akarnya berbeda.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Issue dibuat dari laporan 3 butir tanpa lampiran; 6 temuan pelapor, 1 temuan tambahan | `diagnose-module-issue` |
| 2026-11-04 | Rizki Gunawan menyetujui; status `DALAM_PERBAIKAN`. `FIX-IGD-004-01` dan `-02` dikerjakan dan diuji di browser (lihat plan repair Riwayat). Ditambahkan `ISS-IGD-004-T2` dan `-T3` dari hasil uji. K-01, K-02, dan K-03 belum diputuskan | Claude Code |
