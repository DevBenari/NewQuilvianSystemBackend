# Modul Gizi — Keputusan Wawancara

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Revision | `1` |
| Status | `draft` |
| Product/domain owner | Pemilik kebutuhan |
| Backend SHA | `f2c5090` |
| Frontend SHA | `847be1fc0` |
| Registry sistem | `SEGAR`, dipindai 2026-08-27 |
| Pass wawancara | Scope Pass |

Wawancara ini dibuka dengan Kartu Konteks Pra-Wawancara berdasarkan registry sistem, sesuai
aturan pra-scan. Alur versi pertama yang ditulis pemilik kebutuhan menjadi bahan masuk.

## Scope dan Outcome

Modul Gizi menangani asuhan gizi pasien rawat inap. Alur dimulai ketika dokter penanggung
jawab membuat order konsultasi gizi, dan berakhir ketika pasien keluar dari rawat inap.

### Termasuk dalam scope versi pertama

1. Order konsultasi gizi untuk pasien rawat inap.
2. Daftar pemesanan konsultasi gizi sebagai daftar kerja ahli gizi.
3. Kunjungan ahli gizi yang berulang selama pasien dirawat.
4. Asuhan gizi per kunjungan: asesmen, diagnosis gizi, intervensi, serta monitoring dan
   evaluasi.
5. Konseling gizi dan recall asupan sebagai bagian kunjungan.
6. Penentuan diet dan kebutuhan nutrisi.
7. Penutupan asuhan gizi.

### Di luar scope versi pertama

- Pasien rawat jalan dan IGD. Dicatat sebagai kemungkinan versi berikutnya, bukan ditolak.
- Skrining gizi awal oleh perawat. Modul Gizi **membaca** hasilnya, tetapi pengisiannya milik
  modul keperawatan atau rawat inap.
- Pemesanan makanan, menu, siklus menu, porsi, distribusi ke bangsal, dan pencatatan sisa
  makanan. Seluruhnya milik layanan makanan atau dapur yang belum ada di sistem.
- Perhitungan tarif dan penagihan konsultasi gizi. Tetap milik Billing.

### Di luar scope — untuk modul lain

Modul dapur atau layanan makanan belum ada sama sekali di sistem. Penyerahan hasil penentuan
diet ke dapur akan menjadi kontrak integrasi yang menunggu modul itu dibuat.

## Aktor dan Tanggung Jawab

| Aktor | Tanggung jawab utama |
|---|---|
| Perawat rawat inap | Mengisi skrining gizi awal dan mengusulkan rujukan bila hasilnya berisiko |
| Dokter penanggung jawab pasien | Membuat order konsultasi gizi |
| Ahli gizi atau konsulen gizi | Melakukan kunjungan, asesmen, diagnosis gizi, intervensi, konseling, recall, dan penentuan diet |

## Keputusan

| ID | Jenis | Keputusan | Owner | Status | approved_by / approved_at | Bukti |
|---|---|---|---|---|---|---|
| `GIZ-DEC-001` | Decision | Order konsultasi gizi memakai entity milik modul Gizi sendiri, menunjuk `PatientId` dan `EncounterId`, bukan menumpang `TrxDoctorConsultation` | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-002` | Decision | Versi pertama hanya melayani pasien rawat inap | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-003` | Decision | Skrining gizi dikerjakan perawat dalam 24 jam pertama, sebelum order. Pasien berisiko baru dirujuk ke ahli gizi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-004` | Decision | ~~Modul Gizi berhenti di penentuan diet dan tidak mengurus pemesanan makanan ke dapur~~ **DIGANTI `GIZ-DEC-013`** | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-005` | Decision | Asuhan gizi dicatat berulang per kunjungan ahli gizi, setiap kunjungan memuat asesmen, diagnosis gizi, intervensi, serta monitoring dan evaluasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-006` | Decision | Diagnosis gizi dipilih dari master berkode, bukan isian bebas | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-007` | Decision | Order konsultasi gizi hanya boleh dibuat dokter penanggung jawab pasien | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-008` | Decision | Asuhan gizi ditutup ketika pasien keluar rawat inap, disertai catatan penutup ahli gizi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Wawancara Scope Pass |
| `GIZ-DEC-009` | Decision | ~~Diagnosis gizi menumpang `MstDiagnosis` yang sudah ada dengan `DiagnosisType` bernilai `NUTRITION`, bukan master tersendiri~~ **DIGANTI `GIZ-DEC-011` V1, revisi disetujui pemilik proses 2026-09-25** | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Pemeriksaan master setelah audit kemampuan |
| `GIZ-DEC-010` | Decision | Kunjungan ahli gizi ditulis sebagai baris CPPT `TrxPatientIntegratedProgressNote` dengan `ProfessionType` `Nutritionist` dan `SourceModule` `Nutrition`. Data terstruktur gizi disimpan entity milik Gizi yang menunjuk balik ke baris CPPT tersebut | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Pemeriksaan kemampuan existing |
| `GIZ-DEC-011` | Decision V1 | **Mengganti `GIZ-DEC-009`.** Diagnosis gizi memakai master berkode milik Gizi sendiri dengan baseline standar IDNT. Satu baris memuat `Code`, `Name`, dan `Domain`; domain V1 adalah `NI`, `NC`, `NB`. Tidak ada isian bebas | Pemilik proses gizi | approved | Pemilik proses gizi, 2026-09-25 | Keputusan V1 atas `GIZ-OQ-002` |
| `GIZ-DEC-012` | Decision V1 | **Mengganti arahan sementara sebelumnya.** Kebutuhan nutrisi V1 memuat energi (kkal/hari), protein, lemak, karbohidrat (gram/hari), dan cairan (ml/hari). Sistem menghitung nilai awal, ahli gizi boleh mengoreksi; yang disimpan adalah nilai kalkulasi, nilai final, alasan perubahan, pelaku, dan waktunya. Diet dipilih dari master diet rumah sakit, bukan teks bebas. Perubahan kebutuhan memiliki histori | Pemilik proses gizi | approved | Pemilik proses gizi, 2026-09-25 | Keputusan V1 atas `GIZ-OQ-004` |
| `GIZ-DEC-013` | Decision | **Mengganti `GIZ-DEC-004`.** Scope Gizi diperluas mencakup diet pasien, produksi makanan, dan distribusi makanan, beserta master pendukungnya | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-09-01 | Struktur menu yang diminta pemilik kebutuhan |
| `GIZ-DEC-014` | Decision V1 | Pemilik proses bisnis modul Gizi yang berwenang menyetujui adalah **Kepala Instalasi Gizi / Kepala Unit Gizi**. Menutup `GIZ-OQ-006` dan zona konflik `KF-001` untuk modul ini | Pemilik proses gizi | approved | Pemilik proses gizi, 2026-09-25 | Keputusan V1 atas `GIZ-OQ-006` |

### GIZ-DEC-001 — Order konsultasi gizi memakai entity sendiri

**Masalah yang diputuskan.** Registry menunjukkan `TrxDoctorConsultation` sudah ada dengan
tingkat `L4 Terpakai`, dan strukturnya mirip dengan order konsultasi gizi: ia sudah membawa
`EncounterId`, `PatientId`, `DoctorId`, `ServiceUnitId`, dan status konsultasi. Ini tercatat
sebagai zona konflik `KF-003` pada registry.

**Alasan memilih entity sendiri.** Order gizi membawa hal yang tidak ada pada konsultasi
dokter: ahli gizi sebagai penerima, anjuran diet awal, dan lifecycle asuhan gizi yang berjalan
berhari-hari. Sebaliknya, konsultasi dokter membawa tanda vital dan `ClinicId` yang tidak
relevan bagi gizi. Memaksakan keduanya ke satu tabel berisiko melanggar aturan konsultasi
dokter tanpa disadari.

**Konsekuensi yang diterima.** Laporan gabungan konsultasi lintas profesi perlu menggabungkan
dua tabel. Ini diterima karena laporan seperti itu belum menjadi kebutuhan versi pertama.

**Yang tetap dipakai ulang.** `MstPatient`, `TrxPatientEncounter`, `MstDoctor`,
`MstWorkforceProfile`, `MstDiagnosis`, dan `MstServiceUnit`. Modul Gizi menyimpan penunjuknya
saja, tidak menyalin isinya.

### GIZ-DEC-003 — Skrining gizi berada sebelum order

**Perubahan dari alur versi pertama.** Alur yang ditulis pemilik kebutuhan menempatkan skrining
gizi sebagai salah satu bagian pengkajian ahli gizi. Keputusan ini memindahkannya ke depan.

**Alasan.** Order konsultasi gizi membutuhkan pemicu yang jelas. Bila skrining berada di dalam
pengkajian, pasien berisiko baru terdeteksi setelah order dibuat, padahal order itu sendiri
seharusnya lahir dari hasil skrining.

> **Contoh:** Pasien masuk bangsal pukul 14.00. Perawat mengisi skrining gizi pukul 16.00 dan
> hasilnya menunjukkan risiko. Perawat mengusulkan rujukan, dokter penanggung jawab membuat
> order konsultasi gizi pukul 17.00. Ahli gizi membuka daftar kerja dan melakukan kunjungan
> pertama keesokan paginya. Bila skrining berada di dalam pengkajian ahli gizi, pasien ini
> tidak akan pernah muncul di daftar kerja karena tidak ada yang memicu order.

**Konsekuensi.** Modul Gizi perlu membaca hasil skrining sebagai prasyarat order. Audit
kemampuan menemukan skrining itu sudah ada di `TrxPatientAssessment` milik Clinical
Management, sehingga modul Gizi cukup membacanya. Rinciannya pada
`01-existing-capability-map.md`.

### GIZ-DEC-005 — Asuhan dicatat berulang per kunjungan

**Bentuk yang disepakati.**

```text
Order Konsultasi Gizi (satu per episode rawat inap)
  |
  +-- Kunjungan 1  -> asesmen, diagnosis gizi, intervensi, rencana monitoring
  +-- Kunjungan 2  -> asesmen ulang, evaluasi capaian, intervensi disesuaikan
  +-- Kunjungan 3  -> dan seterusnya
```

Konseling gizi dan recall asupan menempel pada kunjungan, bukan pada order. Dengan begitu
langkah "Evaluasi dan tindak lanjut" pada alur versi pertama punya pembanding antar waktu.

**Wadah kunjungan ditentukan `GIZ-DEC-010`.** Kunjungan tidak memerlukan entity tersendiri
milik Gizi; ia memakai CPPT yang sudah ada.

> **Contoh:** Kunjungan hari pertama mencatat asupan oral 40 persen dari kebutuhan. Kunjungan
> hari ketiga mencatat 75 persen. Perbandingan dua angka itulah yang menjadi bukti evaluasi.
> Bila hanya ada satu catatan yang diperbarui terus, angka hari pertama hilang dan evaluasi
> tidak dapat dibuktikan.

### GIZ-DEC-006 dan GIZ-DEC-009 — Diagnosis gizi memakai master berkode yang sudah ada

`GIZ-DEC-006` menetapkan diagnosis gizi dipilih dari master berkode, bukan isian bebas.
`GIZ-DEC-009` kemudian menetapkan master mana yang dipakai, setelah pemeriksaan master
dilakukan.

**Yang ditemukan saat pemeriksaan.** `MstDiagnosis` bukan master khusus ICD-10. Ia membawa
kolom `DiagnosisType` dengan nilai bawaan `ICD10`, dan penyaring per jenis itu **sudah dipakai
kode existing**:

> `Areas/HealthServices/ClinicalManagement/Controllers/DiagnosisRecommendationResolverController.cs`
> baris 79 @ `f2c5090` menyaring `x.DiagnosisType == "ICD10"`.

Nilai jenis lain yang sudah beredar di kode: `PNPK` dan `ICD9`. Artinya master ini memang
dirancang menampung lebih dari satu sistem klasifikasi.

**Keputusan.** Diagnosis gizi masuk ke `MstDiagnosis` dengan `DiagnosisType = "NUTRITION"`.

> **Contoh isi:**
>
> | `DiagnosisCode` | `DiagnosisName` | `DiagnosisType` |
> |---|---|---|
> | `E44` | Malnutrisi protein energi | `ICD10` |
> | `NI-2.1` | Asupan oral tidak adekuat | `NUTRITION` |
> | `NC-1.1` | Kesulitan menelan | `NUTRITION` |
>
> Kode dan uraian pada baris `NUTRITION` di atas hanya ilustrasi bentuk. Isi sebenarnya
> ditetapkan pemilik proses gizi lewat `GIZ-OQ-002`.

**Konsekuensi yang diterima.** `MstDiagnosis` adalah data bersama milik Master Data
HealthServices, bukan milik Gizi. Penambahan jenis baru perlu sepengetahuan pemiliknya. Sebagai
gantinya, sistem tidak bertambah satu master lagi, dan pengguna mencari diagnosis di satu
tempat.

**Yang belum selesai.** Isi masternya tetap menunggu `GIZ-OQ-002`. Sebelum baris `NUTRITION`
diisi dan disahkan, fitur diagnosis gizi belum dapat dipakai.

## Pertanyaan Terbuka

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| ~~`GIZ-OQ-001`~~ | ~~Siapa pemilik data skrining gizi awal?~~ **Tertutup oleh audit.** Skrining gizi sudah ada di `TrxPatientAssessment` milik Clinical Management, tingkat `L4`, memuat `NutritionRiskStatus`, `NutritionRiskScore`, dan `NutritionNote` | Clinical Management | Tidak lagi |
| ~~`GIZ-OQ-002`~~ | ~~Apa isi master diagnosis gizi yang disahkan rumah sakit?~~ **TERTUTUP 2026-09-25 oleh `GIZ-DEC-011` V1**: master berkode milik Gizi, baseline IDNT, berkolom `Code`, `Name`, `Domain` | Pemilik proses gizi | Tidak lagi |
| `GIZ-OQ-003` | Apakah `MstProfession` sudah berisi baris untuk ahli gizi? Entity-nya tersedia di `L4`, tetapi isinya belum diperiksa karena audit bersifat read-only terhadap source | Human Resource | Tidak |
| ~~`GIZ-OQ-004`~~ | ~~Komponen kebutuhan nutrisi apa yang dihitung dan disimpan?~~ **TERTUTUP 2026-09-25 oleh `GIZ-DEC-012` V1**: energi, protein, lemak, karbohidrat, cairan; sistem menghitung, ahli gizi mengoreksi; berhistori | Pemilik proses gizi | Tidak lagi |
| `GIZ-OQ-005` | Berapa lama pasien tidak berisiko harus diskrining ulang, dan apakah pengulangan itu tanggung jawab modul Gizi? | Pemilik proses gizi | Tidak untuk versi pertama |
| ~~`GIZ-OQ-006`~~ | ~~Siapa pemilik proses bisnis modul Gizi yang berwenang menyetujui keputusan ini?~~ **TERTUTUP 2026-09-25 oleh `GIZ-DEC-014`**: Kepala Instalasi Gizi / Kepala Unit Gizi | Pemilik proses gizi | Tidak lagi |
| `GIZ-OQ-007` | **DEFERRED 2026-09-25 atas keputusan pemilik proses.** Rumus kalkulasi kebutuhan nutrisi tidak dibuat pada V1. Nilai kebutuhan diinput dan difinalisasi ahli gizi; struktur registry rumus tetap disiapkan agar rumus dapat ditambahkan kemudian tanpa membongkar tabel | Pemilik proses gizi | Tidak memblokir. Ditunda, bukan menunggu |

## Zona Konflik Registry yang Menyentuh Modul Ini

| ID | Temuan | Penanganan pada wawancara ini |
|---|---|---|
| `KF-003` | Konsep konsultasi sudah ada sebagai `TrxDoctorConsultation` | Ditutup lewat `GIZ-DEC-001`. Perlu sepengetahuan pemilik Clinical Management |
| `KF-001` | Tidak ada modul yang tercatat pemilik proses bisnisnya | **Tertutup untuk modul Gizi** lewat `GIZ-DEC-014`: Kepala Instalasi Gizi / Kepala Unit Gizi. Modul lain tetap terbuka |

## Catatan

Seluruh keputusan di atas berstatus `approved`. `GIZ-DEC-011`, `GIZ-DEC-012`, dan `GIZ-DEC-014`
disetujui pemilik proses gizi pada 2026-09-25; sisanya oleh pemilik kebutuhan yang mengikuti wawancara
ini. Status blueprint dinaikkan menjadi `READY` pada 2026-09-25: seluruh pertanyaan yang
memblokir desain sudah tertutup. Yang tersisa `GIZ-OQ-003`, `GIZ-OQ-005`, dan `GIZ-OQ-007`,
ketiganya tidak memblokir desain.

Keputusan ini adalah persetujuan pemilik kebutuhan terhadap rekomendasi, bukan klaim regulasi
atau SOP rumah sakit. Praktik skrining gizi dalam 24 jam pada `GIZ-DEC-003` perlu diverifikasi
terhadap kebijakan mutu rumah sakit yang berlaku.

### GIZ-DEC-010 — Kunjungan ahli gizi memakai CPPT yang sudah ada

**Yang ditemukan saat pemeriksaan kemampuan.** `TrxPatientIntegratedProgressNote`, Catatan
Perkembangan Pasien Terintegrasi, sudah ada pada tingkat `L4 Terpakai` dan endpoint-nya sudah
dipanggil frontend. Yang menentukan: **tempat untuk gizi sudah disiapkan di dalamnya**, bukan
hasil penafsiran.

> `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`
> @ `f2c5090`:
>
> - baris 1268 mendaftarkan `new() { Value = "Nutrition", Label = "Gizi" }` sebagai pilihan
>   `SourceModule`;
> - baris 1294 memetakan `"gizi"`, `"nutrition"`, dan `"nutritionist"` menjadi
>   `ProfessionType = "Nutritionist"`;
> - baris 1309 memetakan `"Nutritionist"` menjadi nama profesi `"Gizi"`.

Perancang CPPT sudah menyiapkan tempat bagi catatan ahli gizi. Yang belum ada hanyalah modul
yang mengisinya.

**Yang cocok dan yang berbeda.**

| Kebutuhan Gizi | Tersedia di CPPT | Putusan |
|---|---|---|
| Catatan kunjungan naratif | `SubjectiveSummary`, `ObjectiveSummary`, `AssessmentSummary`, `PlanSummary` | Pakai yang ada |
| Evaluasi tindak lanjut | `Evaluation` | Pakai yang ada |
| Instruksi untuk tenaga lain | `Instruction` | Pakai yang ada |
| Penanda profesi | `ProfessionType`, `ProfessionName` | Pakai yang ada |
| Penunjuk balik ke modul asal | `SourceModule`, `SourceReferenceId`, `SourceReferenceNumber` | Pakai yang ada |
| Diagnosis gizi berkode | Tidak ada tautan diagnosis | Buat baru |
| Target intervensi terukur | Tidak ada | Buat baru |
| Recall asupan | Tidak ada | Buat baru |
| Diet dan kebutuhan nutrisi | Tidak ada | Buat baru |

**Keputusan.** Setiap kunjungan ahli gizi menulis satu baris CPPT dengan
`ProfessionType = "Nutritionist"` dan `SourceModule = "Nutrition"`. Data terstruktur gizi
disimpan pada entity milik Gizi, dan `SourceReferenceId` pada CPPT menunjuk balik ke entity
tersebut.

```text
TrxPatientIntegratedProgressNote  (CPPT, milik Clinical Management)
  ProfessionType    = "Nutritionist"
  SourceModule      = "Nutrition"
  SourceReferenceId --------------------+
  Subjective / Objective / Assessment / Plan / Instruction / Evaluation
                                        |
                                        v
Catatan Asuhan Gizi  (entity baru, milik Gizi)
  order konsultasi, diagnosis gizi berkode, target dan capaian intervensi,
  recall asupan, diet dan kebutuhan nutrisi
```

**Alasan.** CPPT ada supaya seluruh profesi menulis di satu tempat sehingga dokter melihat
perkembangan gizi pasien saat visite. Membuat kunjungan gizi terpisah akan mengalahkan tujuan
itu.

**Konsekuensi yang diterima.** Modul Gizi menulis ke data milik Clinical Management. Ini
memerlukan sepengetahuan pemiliknya, dan aturan CPPT tetap milik Clinical Management, bukan
Gizi.

**Batas yang tetap dijaga.** CPPT menyimpan narasi, bukan angka. Kolomnya teks bebas sehingga
tidak dapat dipakai menghitung laporan mutu gizi. Karena itu diagnosis berkode, target
intervensi, recall, dan diet tetap disimpan terstruktur di entity Gizi.

### Revisi `GIZ-DEC-009` — diagnosis gizi keluar dari `MstDiagnosis`

**Status: revisi disetujui pemilik proses, 25 September 2026.** Keputusan lama menumpangkan
diagnosis gizi pada `MstDiagnosis` dengan `DiagnosisType` bernilai `NUTRITION`. Keputusan V1
menggantinya dengan master khusus milik modul Gizi.

**Alasan pertama: `MstDiagnosis` adalah diagnosis klinis/ICD.** Bentuknya memang ICD, bukan
kebetulan penamaan. Diperiksa pada `Areas/HealthServices/MasterData/Models/MstDiagnosis.cs`,
kolomnya memuat `IcdVersion` dengan nilai bawaan `ICD-10`, `DiagnosisChapterId` yang menunjuk
bab ICD, serta `IsPrimaryDiagnosisAllowed` dan `IsSecondaryDiagnosisAllowed` yang mengatur
posisi diagnosis pada klaim. Tak satu pun berlaku bagi diagnosis gizi.

**Alasan kedua: diagnosis gizi memakai domain dan terminologi yang berbeda.** Diagnosis gizi
berdiri di atas terminologi IDNT, dengan pengelompokan `NI`, `NC`, dan `NB` yang tidak punya
padanan pada struktur bab ICD. `MstDiagnosis` tidak menyediakan tempat bagi domain itu, dan
menambahkan kolom `Domain` di sana berarti mengubah master milik modul MasterData demi
kebutuhan satu modul lain — melanggar batas kepemilikan tabel.

**Akibat praktis bila dipaksakan.** Baris IDNT yang dicampur ke master ICD akan ikut muncul
pada layar diagnosis modul lain yang membaca `MstDiagnosis` tanpa menyaring tipe. Dokter yang
mencari diagnosis penyakit akan melihat diagnosis gizi di daftarnya, dan sebaliknya.

**Biaya perpindahan nol.** Diperiksa pada database uji: `MstDiagnosis` berisi 0 baris untuk
seluruh tipe, dan belum ada satu pun catatan asuhan gizi. Tidak ada data yang perlu
dipindahkan, dan **tidak ada data migration** yang dibutuhkan. Biaya ini akan naik begitu data
sungguhan masuk — itulah sebabnya revisinya diputuskan sekarang.

### GIZ-DEC-011 — Master diagnosis gizi berkode milik Gizi, baseline IDNT

**Mengganti `GIZ-DEC-009`.** Keputusan lama menumpangkan diagnosis gizi pada `MstDiagnosis`
dengan `DiagnosisType` bernilai `NUTRITION`. Keputusan V1 menggantinya dengan master milik
modul Gizi sendiri.

**Yang diputuskan.** Diagnosis gizi dipilih dari master berkode, tanpa isian bebas. Baseline
yang dipakai adalah standar IDNT. Satu baris diagnosis memuat tiga hal:

| Bagian | Guna |
|---|---|
| `Code` | Kode diagnosis, misalnya bentuk `NI-5.2` pada IDNT |
| `Name` | Nama diagnosis yang dibaca ahli gizi |
| `Domain` | Kelompok besarnya |

Domain V1 ada tiga: `NI` (Nutrition Intake), `NC` (Nutrition Clinical), dan `NB` (Nutrition
Behavioral-Environmental).

**Kenapa pindah dari `MstDiagnosis`.** Alasannya bukan selera, melainkan tiga hal yang dapat
diperiksa pada source:

1. `MstDiagnosis` berbentuk ICD — ia memuat `IcdVersion`, `DiagnosisChapterId`,
   `IsPrimaryDiagnosisAllowed`, dan `IsSecondaryDiagnosisAllowed`. Tidak satu pun berlaku
   bagi diagnosis gizi, dan tidak ada tempat bagi `Domain`.
2. Menaruh `Domain` di sana berarti menambah kolom pada master milik modul MasterData demi
   kebutuhan satu modul lain. Itu melanggar batas kepemilikan tabel.
3. Baris IDNT yang dicampur ke master ICD akan ikut muncul pada layar diagnosis modul lain
   yang membaca `MstDiagnosis` tanpa menyaring tipe.

**Biaya perpindahannya nol.** Diperiksa pada database uji: `MstDiagnosis` berisi **0 baris**
untuk seluruh tipe, dan belum ada satu pun catatan asuhan gizi. Tidak ada data yang perlu
dipindahkan. Biaya ini akan naik begitu data sungguhan masuk, dan itulah alasan memutuskannya
sekarang.

**Masternya tetap dibuat kosong.** Baseline IDNT menyebut standarnya, bukan isinya. Daftar
barisnya diimpor admin gizi, tidak dikarang sistem dan tidak diketik dari ingatan.

### GIZ-DEC-012 — Kebutuhan nutrisi lima parameter, diinput ahli gizi; rumus ditunda

**Mengganti arahan sementara sebelumnya**, yang hanya menyimpan satu angka energi yang diketik.

**Parameter V1.**

| Parameter | Satuan |
|---|---|
| Energi | kkal/hari |
| Protein | gram/hari |
| Lemak | gram/hari |
| Karbohidrat | gram/hari |
| Cairan | ml/hari |

**Cara mengisinya.** Sistem menghitung nilai awal, lalu ahli gizi boleh mengoreksinya. Yang
disimpan bukan hanya hasil akhirnya, melainkan lima hal sekaligus untuk setiap parameter:
nilai kalkulasi sistem, nilai final, alasan perubahan, siapa yang mengubah, dan kapan.

Menyimpan kelimanya membuat angka yang dipakai merawat pasien dapat dipertanggungjawabkan.
Tanpa nilai kalkulasi, tidak ada yang tahu apakah ahli gizi mengoreksi banyak atau sedikit;
tanpa alasan, koreksi besar tidak dapat ditelaah ketika hasilnya dipersoalkan.

**Diet dari master.** Diet ditetapkan dengan memilih baris master diet rumah sakit, bukan
mengetik teks bebas. Kolom teks bebas `DietPrescription` pada catatan asuhan dicabut.

**Kebutuhan nutrisi berhistori.** Perubahan tidak menimpa nilai sebelumnya; tiap perubahan
melahirkan revisi baru, dan revisi lama tetap dapat dibaca.

**Rumus ditunda, bukan ditunggu (`GIZ-OQ-007` DEFERRED, 25 September 2026).** Pemilik proses
memutuskan rumus kalkulasi **tidak dibuat pada V1**. Nilai kebutuhan nutrisi diinput dan
difinalisasi ahli gizi, persis seperti arahan lama — perbedaannya, kini di atas struktur yang
sudah siap menerima rumus.

Yang tetap dibangun sekarang adalah **registry rumus beserta antarmukanya**, dalam keadaan
kosong. Membangunnya sekarang berarti penambahan rumus kelak tidak menuntut pembongkaran
tabel: cukup satu baris master dan satu kelas perhitungan. Yang tidak dibangun adalah isinya —
rumus tidak diambil dari internet dan tidak dikarang.

Konsekuensi yang diterima: selama registry kosong, kolom nilai kalkulasi selalu kosong dan
sistem tidak dapat memeriksa kewajaran angka yang diketik. Tanggung jawab klinisnya tetap pada
ahli gizi.

**Kenapa parameter disimpan sebagai master, bukan lima kolom tetap.** Bila kelak serat, natrium,
atau kalium ikut dihitung, penambahannya berupa satu baris master — bukan kolom baru, bukan
migration, dan bukan perombakan data historis. Bentuk ini yang diminta pemilik proses: fleksibel
lewat master, bukan lewat pembongkaran tabel.

### GIZ-DEC-013 — Scope diperluas sampai produksi dan distribusi makanan

**Mengganti `GIZ-DEC-004`.** Keputusan sebelumnya menghentikan modul Gizi di penentuan diet
dan menyerahkan urusan makanan kepada modul dapur yang belum ada. Pemilik kebutuhan
menetapkan bahwa urusan itu masuk ke dalam modul Gizi.

**Bentuk yang disepakati.**

```text
MODUL GIZI
  Daftar Pasien Gizi
  Pelayanan Gizi              asesmen, diagnosis, intervensi, konseling, evaluasi
  Diet dan Makanan Pasien
    Diet Pasien               diet aktif per pasien, beserta riwayat perubahannya
    Produksi Makanan          rekap kebutuhan makanan per jadwal makan
    Distribusi Makanan        penyerahan makanan per pasien per jadwal makan
  Master Gizi                 jenis diet, bentuk makanan, jadwal makan
  Laporan
```

**Batas yang tetap dijaga: struktur dibuat, isi tidak dikarang.**

Master jenis diet, bentuk makanan, dan jadwal makan dibuat **kosong**. Isinya khas tiap rumah
sakit — jam makan, nama diet, dan bentuk makanan berbeda antar tempat — dan menebaknya
menghasilkan master yang terlihat resmi padahal tidak pernah disahkan siapa pun. Sama seperti
sikap terhadap master diagnosis gizi pada `GIZ-DEC-011`.

**Produksi makanan tidak menjadi entity tersendiri.** Kebutuhan produksi adalah **hasil
hitungan** atas diet yang sedang aktif pada satu tanggal dan jadwal makan, bukan data yang
diisi orang. Menyimpannya sebagai entity berarti menyimpan angka yang bisa berbeda dari diet
yang sebenarnya berlaku, dan dapur akan memasak berdasarkan angka basi.

**Yang masih di luar scope.** Menu, siklus menu, standar porsi, resep, dan stok bahan makanan.
Kelimanya milik pengelolaan dapur sebagai unit produksi, bukan asuhan gizi pasien, dan
memerlukan pemilik proses tersendiri. Bila kelak dibutuhkan, itu penambahan berikutnya.

### GIZ-DEC-014 — Pemilik proses modul Gizi

**Yang diputuskan.** Yang berwenang menyetujui aturan proses gizi adalah **Kepala Instalasi
Gizi / Kepala Unit Gizi**. Dengan ini `GIZ-OQ-006` tertutup, dan zona konflik registry
`KF-001` tertutup sepanjang menyangkut modul Gizi.

**Yang tidak ikut diputuskan, dan sengaja tidak ditebak.** Jabatan ini belum tentu sudah ada
sebagai baris pada master jabatan. Yang dapat dipastikan dari source hanyalah wadahnya:
`MstPosition`, `MstDepartment`, dan `MstOrganizationUnit` tersedia, dan pemetaan profesi
`Nutritionist` → `Ahli Gizi` sudah ada pada `EmployeeController`. Apakah baris "Kepala
Instalasi Gizi" sudah terisi **tidak dapat dijawab dari source**, karena isi master adalah
data, bukan kode.

Karena itu keputusan ini **tidak** diterjemahkan menjadi permission, enum, atau kolom baru.
Ia dicatat sebagai pemilik proses. Pemberian hak akses tetap lewat `SysAccessPolicy` per
Departemen dan Jabatan seperti modul lain, setelah baris jabatannya ada.

## Status blocker Phase 1

Diperbarui 2026-09-25 atas keputusan V1 pemilik proses gizi.

| ID | Status | Keterangan |
|---|---|---|
| `GIZ-OQ-002` isi master diagnosis gizi | **TERTUTUP** | `GIZ-DEC-011` V1: master berkode milik Gizi, baseline IDNT, berkolom `Code`, `Name`, `Domain` |
| `GIZ-OQ-004` komponen kebutuhan nutrisi | **TERTUTUP** | `GIZ-DEC-012` V1: lima parameter, dihitung lalu boleh dikoreksi, berhistori |
| `GIZ-OQ-006` pemilik proses | **TERTUTUP** | `GIZ-DEC-014`: Kepala Instalasi Gizi / Kepala Unit Gizi |
| `GIZ-OQ-007` rumus kalkulasi | **DEFERRED** | Tidak dibuat pada V1 atas keputusan pemilik proses. Nilai kebutuhan diinput ahli gizi; registry rumus tetap berdiri dan kosong |

Desain domain dengan ini dinyatakan **READY**, dan pembuatan entity beserta migration boleh
berjalan.

**Yang tetap tidak boleh dikerjakan.**

| Dilarang | Alasan |
|---|---|
| Mengisi master diagnosis gizi dengan daftar karangan | Diagnosis yang salah menempel pada rekam medis pasien. Baseline IDNT menyebut standarnya, bukan membebaskan siapa pun mengetik isinya dari ingatan |
| Membuat enum diagnosis gizi di kode | Menjadikan isi master sebagai ketetapan kode; penambahan diagnosis akan menuntut rilis ulang |
| Menanamkan rumus energi, protein, lemak, karbohidrat, atau cairan yang tidak diserahkan pemilik proses | Satu rumus keliru berdampak pada seluruh pasien sekaligus, dan kekeliruannya sulit terlihat karena hasilnya tetap tampak masuk akal. Lihat `GIZ-OQ-007` |
| Menetapkan batas wajar klinis parameter selain yang sudah ada | Batas energi 1–10000 kkal berasal dari aturan `GIZ006` yang sudah disepakati. Batas protein, lemak, karbohidrat, dan cairan belum ditetapkan siapa pun |
| Menerjemahkan `GIZ-DEC-014` menjadi permission atau jabatan baru | Isi master jabatan adalah data; menebaknya melanggar aturan pemetaan jabatan berbasis bukti |
