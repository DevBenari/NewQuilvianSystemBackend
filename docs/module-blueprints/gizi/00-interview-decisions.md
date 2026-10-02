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
| `GIZ-DEC-009` | Decision | Diagnosis gizi menumpang `MstDiagnosis` yang sudah ada dengan `DiagnosisType` bernilai `NUTRITION`, bukan master tersendiri | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Pemeriksaan master setelah audit kemampuan |
| `GIZ-DEC-010` | Decision | Kunjungan ahli gizi ditulis sebagai baris CPPT `TrxPatientIntegratedProgressNote` dengan `ProfessionType` `Nutritionist` dan `SourceModule` `Nutrition`. Data terstruktur gizi disimpan entity milik Gizi yang menunjuk balik ke baris CPPT tersebut | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-27 | Pemeriksaan kemampuan existing |
| `GIZ-DEC-011` | Arahan sementara | **BUKAN keputusan final; `GIZ-OQ-002` tetap BLOCKED.** Isi master diagnosis gizi diadopsi dari Quilvian v1. Kemiripan yang tidak persis dapat diterima; daftarnya diambil lewat ekspor dari v1, bukan disusun ulang maupun dikarang | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-09-01 | Jawaban `GIZ-OQ-002` |
| `GIZ-DEC-012` | Arahan sementara | **BUKAN keputusan final; `GIZ-OQ-004` tetap BLOCKED.** Kebutuhan nutrisi diketik ahli gizi, bukan dihitung sistem. Yang disimpan hanya kebutuhan energi dalam kkal | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-09-01 | Jawaban `GIZ-OQ-004` |
| `GIZ-DEC-013` | Decision | **Mengganti `GIZ-DEC-004`.** Scope Gizi diperluas mencakup diet pasien, produksi makanan, dan distribusi makanan, beserta master pendukungnya | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-09-01 | Struktur menu yang diminta pemilik kebutuhan |

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
| `GIZ-OQ-002` | Apa isi master diagnosis gizi yang disahkan rumah sakit? **BLOCKED_BY_BUSINESS_DECISION.** Arahan sementara pada `GIZ-DEC-011` bukan keputusan final | Pemilik proses gizi | Ya, memblokir master diagnosis |
| `GIZ-OQ-003` | Apakah `MstProfession` sudah berisi baris untuk ahli gizi? Entity-nya tersedia di `L4`, tetapi isinya belum diperiksa karena audit bersifat read-only terhadap source | Human Resource | Tidak |
| `GIZ-OQ-004` | Komponen kebutuhan nutrisi apa yang dihitung dan disimpan? **BLOCKED_BY_BUSINESS_DECISION.** Arahan sementara pada `GIZ-DEC-012` bukan keputusan final | Pemilik proses gizi | Ya, memblokir perhitungan nutrisi |
| `GIZ-OQ-005` | Berapa lama pasien tidak berisiko harus diskrining ulang, dan apakah pengulangan itu tanggung jawab modul Gizi? | Pemilik proses gizi | Tidak untuk versi pertama |
| `GIZ-OQ-006` | Siapa pemilik proses bisnis modul Gizi yang berwenang menyetujui keputusan ini? | Belum ditentukan | Ya, terkait `KF-001` pada registry |

## Zona Konflik Registry yang Menyentuh Modul Ini

| ID | Temuan | Penanganan pada wawancara ini |
|---|---|---|
| `KF-003` | Konsep konsultasi sudah ada sebagai `TrxDoctorConsultation` | Ditutup lewat `GIZ-DEC-001`. Perlu sepengetahuan pemilik Clinical Management |
| `KF-001` | Tidak ada modul yang tercatat pemilik proses bisnisnya | Belum tertutup. Menjadi `GIZ-OQ-006` |

## Catatan

Seluruh keputusan di atas berstatus `approved` oleh pemilik kebutuhan yang mengikuti wawancara
ini. Status blueprint tetap `draft` karena enam pertanyaan terbuka belum tertutup, dan empat di
antaranya memblokir desain.

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

### GIZ-DEC-011 — Master diagnosis gizi diadopsi dari Quilvian v1

**Yang diputuskan.** Isi master diagnosis gizi tidak disusun ulang dan tidak dikarang. Ia
diambil dari Quilvian v1 lewat ekspor. Pemilik kebutuhan menyatakan kemiripan yang tidak
persis dapat diterima.

**Wadahnya sudah ditetapkan `GIZ-DEC-009`:** `MstDiagnosis` dengan `DiagnosisType` bernilai
`NUTRITION`. Jadi yang tersisa hanyalah mengisi, bukan merancang.

**Yang ini buka dan yang ini tidak.** Keputusan ini menutup pertanyaan rancangan: struktur
masternya sudah pasti, dan desain domain boleh berjalan. Yang belum selesai adalah pekerjaan
data, yaitu mengekspor daftarnya dari v1 dan memasukkannya ke `MstDiagnosis`.

**Sampai ekspor itu ada, masternya kosong.** Layar diagnosis gizi akan tampil tanpa pilihan.
Ini perlu disadari saat menetapkan tanggal rilis, dan bukan alasan menunda desain.

**Yang tetap tidak boleh.** Mengisi master dengan daftar karangan, sekadar agar layarnya
tidak kosong. Daftar semacam itu akan terlihat resmi padahal tidak pernah disahkan siapa pun,
dan diagnosis yang salah menempel pada rekam medis pasien.

### GIZ-DEC-012 — Kebutuhan nutrisi diketik, bukan dihitung sistem

**Yang diputuskan.** Ahli gizi menghitung kebutuhan pasien di luar sistem lalu mengetik
hasilnya. Sistem tidak memuat rumus apa pun. Yang disimpan **hanya kebutuhan energi dalam
kkal**.

```text
Catatan asuhan gizi
  kebutuhan_energi_kkal : angka, diisi ahli gizi
```

**Alasan memilih ini.** Rumus kebutuhan gizi berbeda antar rumah sakit dan antar kondisi
pasien. Menanamkannya di sistem berarti satu rumus yang keliru berdampak pada seluruh pasien
sekaligus, dan kekeliruan itu sulit terlihat karena hasilnya tetap tampak masuk akal.
Membiarkan ahli gizi yang menghitung menjaga tanggung jawab klinis tetap pada orangnya.

**Konsekuensi yang diterima.** Sistem tidak dapat memeriksa kewajaran angka yang diketik,
dan tidak dapat menghitung ulang bila berat badan pasien berubah. Keduanya diterima sebagai
ganti dari kesederhanaan dan keamanan di atas.

**Batas versi pertama.** Protein, lemak, karbohidrat, dan cairan **tidak** disimpan. Bila
kelak dibutuhkan, penambahannya berupa kolom baru pada entity yang sama — bukan perombakan,
karena bentuknya sudah angka bebas, bukan hasil rumus.

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

## Status blocker Phase 1

Diperbarui 2026-09-01 atas perintah pemilik kebutuhan.

| ID | Status | Dampak pada Phase 1 |
|---|---|---|
| `GIZ-OQ-002` isi master diagnosis gizi | `BLOCKED_BY_BUSINESS_DECISION` | Tidak menghambat. Phase 1 tidak memakai diagnosis gizi |
| `GIZ-OQ-004` komponen kebutuhan nutrisi | `BLOCKED_BY_BUSINESS_DECISION` | Tidak menghambat. Phase 1 hanya menyimpan satu angka energi yang diketik, tanpa rumus dan tanpa komponen nutrisi lain |

`GIZ-DEC-011` dan `GIZ-DEC-012` diturunkan statusnya menjadi **arahan sementara**. Keduanya
cukup untuk menjalankan Phase 1, tetapi tidak boleh diperlakukan sebagai keputusan klinis
final. Keputusan final menunggu pemilik proses gizi.

**Yang tetap tidak boleh dikerjakan sampai kedua blocker itu ditutup:**

| Dilarang | Alasan |
|---|---|
| Mengisi master diagnosis gizi dengan daftar apa pun | Diagnosis yang salah menempel pada rekam medis pasien |
| Membuat enum diagnosis gizi di kode | Menjadikan asumsi tampak sebagai ketetapan |
| Menanamkan rumus energi, protein, lemak, karbohidrat, atau cairan | Satu rumus keliru berdampak pada seluruh pasien sekaligus |
| Menambah komponen nutrisi wajib selain energi | Komponennya belum ditetapkan siapa pun |
