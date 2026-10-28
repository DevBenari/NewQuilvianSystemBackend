# Rekam Medis — PRD ke MVP (Amandemen: Berkas Lintas Modul dan Kelengkapan)

## 1. Identitas dokumen

| Field | Value |
|---|---|
| Produk | Quilvian V2 |
| Modul | Rekam Medis (`rekam-medis`, kode `RM`) |
| Blueprint | `RM-BP-001` revision `4` |
| Status | **`approved`** |
| Owner | Yoga Aji Pratama — product/domain, clinical governance, security/privacy, API, frontend (`RM-DEC-027`, `028`) |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 28 Oktober 2026 (`RM-DEC-062`) |
| Repository target | `NewQuilvianSystemBackend` (branch `yoga`), `QuilvianSystemFrontendDev` (branch `YogaV2`) |
| Commit baseline | Backend `363de34f`; frontend `f0a3b07b8` |
| Input | Decision log revision `9`; capability map revision `3`; `RM-RCG-001` revision 2; `RM-DA-001` revision 1 (`DOMAIN_ARCHITECTURE_PARTIAL`); `02-backend-architecture.md` dan `03-frontend-architecture.md` bagian amandemen revision 4; kontrak `0.2.0` |
| Ringkasan cakupan | Berkas rekam medis membaca dokumen final seluruh modul klinis yang sudah ada, dan unit rekam medis mendapat daftar kerja kelengkapan berkas per kunjungan. Hasil Patologi Anatomi dikecualikan |

Rilis pertama modul (penelusuran CPPT dan 12 jenis lain, keutuhan, jejak akses) sudah selesai dan
**bukan** bagian dokumen ini. "MVP" di sini berarti **rilis amandemen**.

## 2. Ringkasan eksekutif

Hari ini dokter yang membuka berkas pasien hanya melihat catatan poliklinik. Hasil lab, laporan
radiologi, catatan IGD, ringkasan pulang, laporan operasi, resep, dan hasil MCU — semuanya sudah
dicatat di sistem — tidak terlihat di berkas. Lebih buruk: hasil lab dan sesi hemodialisa tampil
**kosong tanpa peringatan**, sehingga dokter dapat menyimpulkan pasien tidak pernah diperiksa.

Amandemen ini menjadikan berkas benar-benar satu kesatuan, dan memberi unit rekam medis satu
daftar kunjungan yang berkasnya belum lengkap beserta kekurangannya — tanpa menahan pelayanan,
pemulangan, maupun tagihan.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
|---|---|
| Linimasa hanya membaca 13 jenis dokumen `ClinicalManagement` | `RM-FACT-014`, `RM-CAP-035` |
| Jenis 14 (sesi HD) dan 15 (hasil lab) dijawab kosong tanpa galat | `MedicalRecordTimelineService.cs` cabang `_ =>`; `RM-CAP-035` |
| Dua belas area klinis menyimpan dokumen yang tidak terbaca berkas | `RM-FACT-013`, capability map kelompok F dan G |
| Penanda `Susulan` disahkan sejak 26 Agustus tetapi nol implementasi | `RM-FACT-011`, `RM-FACT-018` |
| Tidak ada penilaian kelengkapan berkas | `RM-CAP-040` |
| Waktu tutup klinis hanya tersimpan pada dokumen konsep | `RM-CAP-036`, `AGG-RM-02` |
| Pemberitahuan cakupan layar keliru ("hanya CPPT") | `RM-CAP-041` |
| Disposisi IGD ke rawat inap tidak mengunci catatan IGD | `GAP-ARCH-RM-01` |

Yang **sudah ada** dan dipakai ulang: mekanisme gangguan per sumber (`RM-CAP-034`), jalur baca
laporan radiologi per pasien (`RM-CAP-058`), mesin keutuhan dan addendum, jejak akses.

## 4. Visi produk

1. Seorang pasien mempunyai satu berkas.
2. Berkas itu memuat setiap dokumen klinis final dari modul mana pun, dibaca langsung dari pemiliknya.
3. Setiap dokumen membawa keterangan apakah ia susulan, pernah diralat, atau masih dapat berubah.
4. Setiap kunjungan yang selesai dilayani dinilai kelengkapannya menurut aturan unit rekam medis.
5. Kekurangan tampil di daftar kerja sampai dituntaskan oleh penulisnya atau oleh hasil yang menyusul.

## 5. Batas MVP

**Titik mulai:**

1. Dokter atau petugas berizin membuka berkas pasien.
2. Kunjungan pasien ditutup klinis lewat salah satu jalur penutupan yang ada.

**Titik akhir:**

1. Berkas menampilkan dokumen final seluruh modul `RM-DEC-039` (kecuali PA), dengan penanda yang benar dan gangguan yang jujur.
2. Kunjungan berkeadaan `Complete` di daftar kerja, atau kekurangannya terlihat lengkap bagi petugas.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
|---|---|
| Dokter dan perawat | Membaca berkas lintas modul; membuat addendum pengesahan atas catatannya sendiri |
| Kepala unit / DPJP | Membuat addendum pengesahan atas nama penulis yang berhalangan |
| Petugas rekam medis | Membaca daftar kerja; menghubungi penulis; menilai ulang kunjungan |
| Pengelola master rekam medis | Mengubah profil dokumen wajib |
| Pengelola akses | Memberi izin daftar kerja dan master per departemen-posisi |
| Modul pemilik dokumen | Menyediakan pembaca sumber dan pembaca uji kelengkapan |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Perbaikan kekosongan diam-diam jenis 14/15 dan pemberitahuan cakupan | `RM-CAP-035`, `041` | Wajib — kerusakan keselamatan yang terjadi hari ini |
| Pembaca sumber Lab (PK/Mikro) dan Radiologi | `RM-CAP-043`, `045`, `058`, `069` | Wajib — penunjang paling sering dicari dokter |
| Pembaca sumber pelayanan, Farmasi, MCU, dan `ClinicalManagement` baru | `RM-CAP-046`–`057`, `059`–`067` | Wajib — tanpa ini berkas tetap tidak utuh (`RM-DEC-039`) |
| Ringkasan pencatatan berkala | `RM-CAP-066` | Wajib — tanpanya linimasa rawat inap tidak terbaca |
| Batas waktu per sumber | `GAP-ARCH-RM-05` | Wajib — tanpa ini satu sumber lambat menahan seluruh berkas |
| Penutupan klinis kunjungan | `RM-CAP-036` | Wajib — dasar `Susulan` dan daftar kerja |
| Penanda `Susulan` | `RM-CAP-037` | Wajib — utang `RM-DEC-006` |
| Profil dokumen wajib dan isi awalnya | `RM-CAP-038` | Wajib — tanpa master, syarat 3 tidak dapat dinilai |
| Penilaian kelengkapan dan daftar kerja | `RM-CAP-040` | Wajib — inti cakupan nomor 4 |
| Addendum pengesahan | `RM-CAP-039` | Wajib — tanpanya catatan `LockedUnsigned` membuat berkas mustahil lengkap |
| Penguncian pada disposisi IGD ke rawat inap | `GAP-ARCH-RM-01` | Wajib — tanpanya kunjungan IGD itu tidak pernah masuk daftar kerja |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Hasil Patologi Anatomi di berkas | `RM-CAP-044` | Laporan PA dapat dibuka kembali sesudah rilis (`RM-FACT-020`); open question 18 | Keterangan "belum tersedia di berkas"; hasil PA dibaca di layar Laboratorium |
| Penanda pernah diralat untuk hasil lab | `RM-CAP-043` | `S6` belum dibangun, sehingga hasil terrilis memang tidak dapat diubah (`RM-FACT-019`) | Penanda bernilai `false` dengan benar; disambung bersama `S6` |
| Koding ICD, resume medis rekam medis, pelepasan informasi, retensi | — | Butuh SOP dan dasar regulasi yang belum diserahkan (`RM-DEC-037`) | Proses manual unit rekam medis yang berjalan sekarang |
| Indikator mutu persentase kelengkapan | — | Belum diminta pemilik; dasar regulasi angka tenggat belum diverifikasi | `FirstCompletedAt` sudah disimpan sehingga laporan dapat dibuat kelak tanpa mengubah struktur |
| Pemberitahuan otomatis ke penulis | — | `RM-DEC-046`: tindak lanjut di luar sistem | Petugas menghubungi penulis yang tercantum di rincian kekurangan |

## 9. Alur bisnis target

**`FLOW-RM-MVP-001` — Kunjungan rawat inap dengan operasi dan hasil menyusul** (data samaran)

1. Ibu S dirawat inap, dioperasi, dan sampel kultur dikirim ke laboratorium.
2. Dokter membuka berkasnya: hasil lab PK, laporan radiologi, laporan operasi, resep, ringkasan
   gula darah per hari, dan CPPT tampil dalam satu linimasa.
3. Episode rawat inap ditutup Jumat. Catatan konsep dikunci `LockedUnsigned`, dan **penutupan
   klinis** tercatat sekali.
4. Rekam medis menilai kunjungan: catatan anestesi belum ada, satu CPPT perawat tidak bertanda
   tangan, kultur belum keluar → **`Menunggu hasil`** dengan tiga rincian.
5. Petugas rekam medis melihatnya di daftar kerja, lalu menghubungi dokter anestesi dan perawat.
6. Perawat membuat **addendum pengesahan**; dokter anestesi memfinalkan catatannya di modul Kamar
   Operasi.
7. Pemeriksaan berkala menilai ulang: tinggal kultur → tetap `Menunggu hasil`, satu rincian.
8. Dua minggu kemudian kultur dirilis. Di berkas ia bertanda **`Susulan`**; penilaian berikutnya
   menghasilkan **`Lengkap`** dan mencatat waktu pertama kali lengkap.

## 10. Epic dan functional requirement

### `EPIC RM-01` — Berkas jujur: perbaikan jenis kosong dan pemberitahuan cakupan

Disposisi: **`EXTEND`**

> **FR-RM-001 — Tidak ada jenis yang kosong diam-diam.** Daftar jenis yang dibaca diambil dari
> pembaca yang terdaftar; jenis tanpa pembaca dilaporkan di `unavailableKinds`.
> **Contoh:** pasien dengan hasil PA yang dirilis membuka berkas; bagian PA berbunyi "Hasil
> Patologi Anatomi belum tersedia di berkas", bukan daftar kosong. (`AT-RM-51`)

> **FR-RM-002 — Batas waktu per sumber.** Sumber yang melewati 3 detik dicatat sebagai gangguan;
> berkas tetap terbuka. (`AT-RM-50`)

> **FR-RM-003 — Pemberitahuan cakupan sesuai keadaan.** Layar menyebut jenis yang benar-benar
> tunduk aturan keutuhan. (`RM-CAP-041`)

### `EPIC RM-02` — Sumber penunjang: Laboratorium dan Radiologi

Disposisi: Lab **`MISSING / NEW`** (pembaca baru), Radiologi **`EXISTING / REUSE`** lewat adapter

> **FR-RM-010 — Hasil lab PK dan Mikrobiologi.** Hasil dengan `ReleasedAt` terisi tampil; yang
> belum dirilis tidak. **Contoh:** hasil gula darah dirilis 09.00 tampil; hasil kultur yang belum
> dirilis tidak tampil. (`AT-RM-53`)

> **FR-RM-011 — Laporan radiologi.** Laporan yang sudah dirilis tampil; versi ke-2 ke atas
> bertanda pernah diralat. (`AT-RM-48`)

### `EPIC RM-03` — Sumber pelayanan

Disposisi: **`MISSING / NEW`** (pembaca di setiap modul pemilik)

> **FR-RM-020** — Dokumen final IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, dan Bank Darah
> tampil menurut kolom final pada `02-backend-architecture.md` A4.10. (`AT-RM-44`)

> **FR-RM-021** — Konsep tidak tampil. **Contoh:** ringkasan pulang tanpa tanda tangan tidak
> tampil. (`AT-RM-45`)

> **FR-RM-022** — Catatan asuhan gizi berlabel "dapat berubah". (`AT-RM-54`)

### `EPIC RM-04` — Farmasi, `ClinicalManagement` baru, dan ringkasan berkala

Disposisi: Farmasi **`MISSING / NEW`**; `ClinicalManagement` **`EXTEND`**

> **FR-RM-030** — Resep yang diajukan, pemberian obat, dan rekonsiliasi tampil. (`AT-RM-44`)

> **FR-RM-031 — Ringkasan berkala.** **Contoh:** enam pengukuran gula darah 28 Oktober, satu
> dibatalkan → satu baris "Gula darah — 5 pengukuran" yang rinciannya memuat lima. (`AT-RM-52`)

### `EPIC RM-05` — MCU

Disposisi: **`MISSING / NEW`**

> **FR-RM-040** — Resume MCU terverifikasi dan hasilnya tampil dengan izin berkas biasa; peserta
> tanpa pasien tidak muncul. (`AT-RM-44`)

### `EPIC RM-06` — Penutupan klinis dan penanda `Susulan`

Disposisi: **`MISSING / NEW`** (penutupan), **`EXTEND`** (linimasa)

> **FR-RM-050 — Satu penutupan per kunjungan.** **Contoh:** kunjungan IGD ditutup lewat visit
> lalu lewat finalisasi tagihan; tersimpan tepat satu waktu tutup — yang pertama. (`AT-RM-56`)

> **FR-RM-051 — Tidak berubah oleh rekonsiliasi.** (`AT-RM-57`)

> **FR-RM-052 — Susulan.** Dokumen final 1 detik sesudah tutup klinis bertanda `Susulan`; 1 detik
> sebelumnya tidak. (`AT-RM-47`, `AT-RM-58`)

> **FR-RM-053 — Disposisi IGD ke rawat inap mengunci.** (`AT-RM-59`)

### `EPIC RM-07` — Profil dokumen wajib

Disposisi: **`MISSING / NEW`**

> **FR-RM-060** — Isi awal sebelas profil A4.8 tersedia setelah migration. (`AT-RM-66`, `AT-RM-71`)

> **FR-RM-061** — Perubahan profil berlaku tanpa rilis program dan memicu penilaian ulang. (`AT-RM-69`)

> **FR-RM-062** — Profil tidak sah ditolak. (`AT-RM-70`)

### `EPIC RM-08` — Penilaian kelengkapan dan daftar kerja

Disposisi: **`MISSING / NEW`**

> **FR-RM-070 — Tiga syarat dan urutan keadaan.** **Contoh:** ringkasan pulang tidak ada dan
> kultur belum keluar → `Menunggu hasil` dengan dua rincian. (`AT-RM-60`, `AT-RM-62`)

> **FR-RM-071 — Gangguan tidak pernah `Lengkap`.** (`AT-RM-63`)

> **FR-RM-072 — Kunjungan penunjang langsung.** RJ di unit Laboratorium lengkap begitu hasil
> dirilis, tanpa dokumen dokter. (`AT-RM-66`)

> **FR-RM-073 — Tidak menahan.** (`AT-RM-68`)

> **FR-RM-074 — Cakupan daftar kerja.** Kunjungan berjalan, batal, dan sebelum rilis tidak masuk.
> (`AT-RM-64`, `AT-RM-65`)

> **FR-RM-075 — Pemeriksaan berkala.** Kunjungan belum lengkap dinilai ulang tiap 60 menit
> (≤ 30 hari) atau 24 jam (lebih lama). (`02-backend-architecture.md` A4.13)

### `EPIC RM-09` — Addendum pengesahan

Disposisi: **`EXTEND`**

> **FR-RM-080** — Penulis mengesahkan catatan `LockedUnsigned`; status tetap; kekurangan tuntas. (`AT-RM-72`)

> **FR-RM-081** — Pengesahan tidak sah ditolak. (`AT-RM-73`, `AT-RM-74`)

## 11. Model status yang diusulkan

| Objek | Status | Invariant utama |
|---|---|---|
| Penutupan klinis | Tercatat (terminal) | Satu per kunjungan; tidak berubah |
| Penilaian kelengkapan | `Undetermined`, `AwaitingResult`, `Incomplete`, `Complete` | Prioritas tetap; `Complete` hanya tanpa kekurangan dan tanpa gangguan |
| Profil dokumen wajib | Aktif, Nonaktif | Tidak dihapus; kunci unik |
| Addendum | `Correction`, `Attestation` | Pengesahan hanya atas `LockedUnsigned`, satu per catatan |

Rincian: `contracts/state-transition-matrix.md` bagian amandemen.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| Mekanisme `FailedSources`; `RadReportService.GetByPatientAsync`; mesin keutuhan; jejak akses; addendum | `MedicalRecordTimelineService`; `ClinicalDocumentIntegrityService`; `ClinicalNoteAddendumService`; `ClinicalDocumentKind` | `MrcEncounterClinicalClosure`; `MrcEncounterCompletenessAssessment`; dua master profil; dua kontrak sumber dan sepuluh pembacanya; layanan kelengkapan dan pemeriksaan berkala |

## 13. Sasaran kemampuan API

Subset `contracts/api-contract.md` versi `0.2.0`.

### Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/{patientId}/timeline` | Linimasa lintas modul | `MedicalRecord : Read` | `MedicalRecordTimelineQuery` | `ApiResponse<MedicalRecordTimelineResponse>` | `EPIC RM-01`–`06` | Tersedia; perluasan **Rencana (belum tersedia)** |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Rincian dokumen | `MedicalRecord : Read` | — | `ApiResponse<MedicalRecordDocumentResponse>` | `EPIC RM-02`–`05` | Tersedia; jenis baru **Rencana (belum tersedia)** |
| `GET` | `/{patientId}/periodic-observations` | Rincian ringkasan berkala | `MedicalRecord : Read` | `kind`, `date` | `ApiResponse<List<PeriodicObservationEntryResponse>>` | `EPIC RM-04` | **Rencana (belum tersedia)** |

### Health Services / Medical Record Management / Medical Record Completeness

Base URL: `api/v1/health-services/medical-record-management/medical-record-completeness`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kerja | `MedicalRecordCompleteness : Read` | `MedicalRecordCompletenessPagedQuery` | `ApiResponse<PagedResult<…ListItemResponse>>` | `EPIC RM-08` | **Rencana (belum tersedia)** |
| `GET` | `/encounters/{encounterId}` | Rincian penilaian | `MedicalRecordCompleteness : Read` | — | `ApiResponse<…DetailResponse>` | `EPIC RM-08` | **Rencana (belum tersedia)** |
| `POST` | `/encounters/{encounterId}/evaluate` | Nilai ulang | `MedicalRecordCompleteness : Evaluate` | — | `ApiResponse<…DetailResponse>` | `EPIC RM-08` | **Rencana (belum tersedia)** |

### Health Services / Master Data / Medical Record Completeness Profile

Base URL: `api/v1/health-services/master-data/medical-record-completeness-profiles`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar profil | `MedicalRecordCompletenessProfile : Read` | `PagedQuery` | `ApiResponse<PagedResult<…>>` | `EPIC RM-07` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat profil | `MedicalRecordCompletenessProfile : Create` | `Create…Request` | `ApiResponse<…DetailResponse>` | `EPIC RM-07` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah profil | `MedicalRecordCompletenessProfile : Update` | `Update…Request` | `ApiResponse<…DetailResponse>` | `EPIC RM-07` | **Rencana (belum tersedia)** |

### Health Services / Medical Record Management / Clinical Note Addendum

Base URL: `api/v1/health-services/medical-record-management/clinical-note-addendums`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/by-document/{documentKind}/{documentId}` | Addendum, kini dengan `purpose` | `ClinicalNoteAddendum : Create` | `{ …, purpose }` | `ApiResponse<…>` | `EPIC RM-09` | Tersedia; `purpose` **Rencana (belum tersedia)** |

## 14. Matriks kewenangan

| Tindakan | String permission |
|---|---|
| Membuka berkas lintas modul | `[AccessPermission("MedicalRecord", "Read")]` |
| Melihat daftar kerja | `[AccessPermission("MedicalRecordCompleteness", "Read")]` |
| Menilai ulang | `[AccessPermission("MedicalRecordCompleteness", "Evaluate")]` |
| Melihat / membuat / mengubah profil | `[AccessPermission("MedicalRecordCompletenessProfile", "Read")]`, `"Create"`, `"Update"` |
| Mengesahkan catatan | `[AccessPermission("ClinicalNoteAddendum", "Create")]`, `"CreateAsSubstitute"` |

## 15. Batas integrasi dan billing

| Yang tidak boleh dibuat rekam medis | Pemiliknya |
|---|---|
| Salinan isi dokumen modul lain | Modul pemilik (`RM-DEC-038`) |
| Aturan final dan ralat dokumen modul lain | Modul pemilik (`RM-DEC-041`, `042`) |
| Penahanan tagihan atau pemulangan | — ditolak (`RM-DEC-046`) |
| Jalur HTTP berlingkup pasien di modul lain | — ditolak; pembaca dalam proses |

Billing: **nol dampak charge.**

## 16. Guardrail regulasi

| Kewajiban | Penanganan |
|---|---|
| Keutuhan rekam medis: isi lama tidak hilang | Addendum pengesahan tidak mengubah isi maupun status catatan |
| Kerahasiaan | Satu izin berkas + pemeriksaan keperluan akses + satu jejak akses; daftar kerja berizin tersendiri dan tidak menampilkan isi |
| Masa simpan | Jejak akses tetap 25 tahun (`RM-DEC-024`). Masa simpan resume MCU: `VERIFY_CURRENT_REGULATION`, tidak menahan |
| Tinjauan komite medik dan perlindungan data | Belum dilakukan (`RM-DEC-027`); serologi HIV/hepatitis dan hasil MCU kini terbaca pemegang izin berkas (`RM-DEC-040`) |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-001` | Penutupan klinis ditulis di dalam transaksi penutupan kunjungan; gagal = penutupan gagal |
| `NFR-002` | Penutupan klinis idempoten: tabrakan index unik diperlakukan sebagai sudah tercatat |
| `NFR-003` | Penilaian disimpan dengan concurrency token `Version`; dua penilaian serentak tidak saling menimpa diam-diam |
| `NFR-004` | Batas waktu 3 detik per sumber, 20 detik per berkas; pembatalan oleh batas waktu dibedakan dari pembatalan pengguna |
| `NFR-005` | Seluruh waktu disimpan UTC; tanggal ringkasan berkala dan saringan daftar kerja memakai tanggal WIB |
| `NFR-006` | Payload logger tidak memuat `Deficiencies`, nama pasien, nama penulis, atau isi addendum |
| `NFR-007` | Pemeriksaan berkala memproses paling banyak 200 kunjungan per putaran |

## 18. Skenario UAT

| ID | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-01` | `RM-01` | Pasien punya hasil lab yang dirilis | Buka berkas | Hasil lab tampil — tidak lagi kosong |
| `UAT-02` | `RM-01` **(gagal)** | Modul Farmasi dibuat lambat di lingkungan uji | Buka berkas | Berkas terbuka; blok peringatan menyebut Farmasi |
| `UAT-03` | `RM-02` | Laporan radiologi dirilis lalu diamandemen | Buka berkas | Laporan tampil bertanda pernah diralat |
| `UAT-04` | `RM-02` **(gagal)** | Hasil kultur belum dirilis | Buka berkas | Kultur tidak tampil |
| `UAT-05` | `RM-03` | Pasien IGD dengan triase selesai dan disposisi dikonfirmasi | Buka berkas | Keduanya tampil |
| `UAT-06` | `RM-03` **(gagal)** | Ringkasan pulang belum ditandatangani | Buka berkas | Ringkasan tidak tampil |
| `UAT-07` | `RM-04` | Pasien rawat inap dengan banyak pengukuran gula darah | Buka berkas | Satu baris per hari; rincian dapat dibuka |
| `UAT-08` | `RM-04` **(gagal)** | Satu pengukuran dibatalkan | Buka rincian hari itu | Pengukuran batal tidak dihitung |
| `UAT-09` | `RM-05` | Peserta MCU terverifikasi | Buka berkas | Resume MCU tampil |
| `UAT-10` | `RM-05` **(gagal)** | Peserta MCU tanpa nomor rekam medis | Cari berkas | Tidak ada berkas; tidak ada galat |
| `UAT-11` | `RM-06` | Episode rawat inap ditutup, hasil dirilis esoknya | Buka berkas | Hasil bertanda Susulan |
| `UAT-12` | `RM-06` **(gagal)** | Rekonsiliasi IGD mengubah waktu selesai | Buka berkas | Penanda Susulan tidak berubah |
| `UAT-13` | `RM-07` | Petugas menambah kriteria pada profil Rawat Jalan | Simpan, buka daftar kerja | Kunjungan RJ belum lengkap menampilkan kekurangan baru |
| `UAT-14` | `RM-07` **(gagal)** | Profil Rawat Jalan sudah ada | Buat profil Rawat Jalan kedua | Ditolak "Profil untuk jenis kunjungan dan unit ini sudah ada" |
| `UAT-15` | `RM-08` | `FLOW-RM-MVP-001` langkah 1–8 | Jalankan | Keadaan berganti `Menunggu hasil` → `Menunggu hasil` → `Lengkap` |
| `UAT-16` | `RM-08` **(gagal)** | Kunjungan masih berjalan | Tekan nilai ulang | Ditolak "Kunjungan ini masih berjalan…" |
| `UAT-17` | `RM-08` | Pasien rujukan hanya cek lab | Hasil dirilis | Kunjungan `Lengkap` tanpa dokumen dokter |
| `UAT-18` | `RM-09` | Catatan perawat `LockedUnsigned` | Perawat mengesahkan | Kekurangan hilang; catatan tetap "terkunci tanpa tanda tangan" |
| `UAT-19` | `RM-09` **(gagal)** | Catatan sudah ditandatangani | Coba sahkan | Ditolak "Pengesahan hanya untuk catatan yang terkunci tanpa tanda tangan" |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Tidak ada jenis dokumen yang tampil kosong diam-diam | `AT-RM-51`, `UAT-01` |
| Setiap modul `RM-DEC-039` (kecuali PA) punya pembaca sumber yang lulus uji | `AT-RM-44` per modul |
| Konsep tidak pernah tampil | `AT-RM-45`, `UAT-06` |
| Satu sumber gagal tidak menahan berkas | `AT-RM-49`, `AT-RM-50`, `UAT-02` |
| Setiap kunjungan yang ditutup sejak rilis punya tepat satu penutupan klinis | `AT-RM-56`, `AT-RM-59` |
| Penanda Susulan benar dan tidak berubah oleh rekonsiliasi | `AT-RM-47`, `AT-RM-57` |
| Sebelas profil awal terisi | Rencana data master A4.8; `AT-RM-66` |
| Daftar kerja tidak pernah menyatakan `Lengkap` saat ada gangguan | `AT-RM-63` |
| Kunjungan penunjang langsung dapat `Lengkap` | `AT-RM-66`, `UAT-17` |
| Kelengkapan tidak menahan penutupan, pemulangan, tagihan | `AT-RM-68` |
| Addendum pengesahan menuntaskan kekurangan tanpa mengubah status | `AT-RM-72` |
| Pemilik setiap modul sumber menyetujui berkas pembaca di modulnya | Catatan persetujuan per `INT-RM-10a`–`10j` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | `EPIC RM-01`; enum, dua kontrak sumber, batas waktu per sumber | Blueprint revision 4 dan kontrak `0.2.0` disahkan |
| `MVP-1` | `EPIC RM-02` (Lab PK/Mikro, Radiologi) | `MVP-0` |
| `MVP-2` | `EPIC RM-06` (penutupan klinis, Susulan, disposisi IGD) | `MVP-0`; persetujuan pemilik IGD |
| `MVP-3` | `EPIC RM-03`, `EPIC RM-04` | `MVP-1`; persetujuan pemilik tiap modul |
| `MVP-4` | `EPIC RM-05` | `MVP-1` |
| `MVP-5` | `EPIC RM-07`, `EPIC RM-09` | `MVP-2` |
| `MVP-6` | `EPIC RM-08` | `MVP-2`, `MVP-3`, `MVP-5` |
| `POST-MVP` | Seluruh bagian 8 | Di luar rilis amandemen |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| Pengesahan blueprint revision 4 dan kontrak `0.2.0` | Yoga Aji Pratama | Seluruh gelombang tidak dapat dimulai | **Ya** |
| Persetujuan menulis pembaca di folder setiap modul sumber | Pemilik masing-masing modul | Gelombang yang memuat modul itu tertahan; modul lain jalan | Ya, per gelombang |
| Konfirmasi arti final disposisi IGD, asuhan gizi, catatan kelahiran (open question 17) | Pemilik IGD, Gizi, Kamar Operasi | Aturan sementara `RM-DEC-059` tetap berlaku | Tidak |
| Buka-ulang laporan PA sesudah rilis (open question 18) | Yoga Aji Pratama (pemilik Laboratorium) | PA tetap di `POST-MVP` | Tidak |
