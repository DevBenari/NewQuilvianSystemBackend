# Rekam Medis — Backend Delivery Roadmap

```yaml
module_id: RM-BP-001
roadmap_revision: 1
status: DRAFT
owners:
  product_domain: Yoga Aji Pratama
  clinical_governance: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  api_authority: Yoga Aji Pratama
approved_by: [Yoga Aji Pratama]
approved_at: 2026-08-26
input_revisions:
  interview_decisions: 4
  capability_map: 2
  backend_architecture: 1
artifact_hashes:
  interview_decisions: sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c
  capability_map: sha256:9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae
  backend_architecture: sha256:32ab3711e9203bedf2838cdadbbeb1ab6400c20d49b1b1497eaed9efaa5243a1
contract_versions:
  api: 0.1.1 (approved 2026-08-27; penyegaran status, bentuk kontrak tidak berubah dari 0.1.0)
  state_transition: 0.1.0 (draft)
  validation: 0.1.0 (draft)
  integration: 0.1.0 (draft)
  permission_audit: 0.1.0 (draft)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

> **GERBANG PERENCANAAN TERPENUHI.** Sejak 26 Agustus 2026 seluruh keputusan berstatus
> `approved` atas nama Yoga Aji Pratama (`RM-DEC-027`), sehingga roadmap ini **sudah menjadi
> izin mulai bekerja** untuk backend.
>
> Dua pengecualian yang tetap berlaku: ~~`BE-08` menunggu bukti pada salinan data nyata~~ (**dijalankan di
> devYoga 2 November 2026**), dan `BE-09` menunggu SOP rekam medis. Keduanya bukan soal pengesahan.
>
> **Pembaruan 27 Agustus 2026 (`RM-DEC-028`).** Yoga Aji Pratama ditetapkan pula sebagai pemilik
> frontend dan pemilik API, sehingga kontrak API naik menjadi `approved` dan **gerbang paralel
> frontend terbuka**. Tidak ada lagi gerbang pengesahan yang tertutup pada modul ini.

---

## 1. Cara membaca roadmap ini

### Status keterlaksanaan

| Status | Arti |
|---|---|
| `SIAP` | Tidak bergantung pada keputusan atau kontrak berstatus draft. Dapat dimulai hari ini |
| `TERTAHAN APPROVAL` | Rancangannya lengkap, tetapi bergantung pada keputusan yang belum disahkan owner |
| `TERTAHAN BLOCKER` | Tertahan hal tertentu yang disebut namanya, di luar soal approval. **Tidak ada lagi task berstatus ini** |
| `SELESAI` | Sudah dikerjakan dan seluruh acceptance criteria terbukti |

Perbedaan dua status tertahan penting. `TERTAHAN APPROVAL` hilang begitu owner ditunjuk dan
menyetujui. `TERTAHAN BLOCKER` menuntut pekerjaan nyata lebih dulu, misalnya menetapkan angka
masa simpan atau menelusuri data. Per 24 Agustus 2026 kedua blocker itu sudah tertutup —
`RM-DEC-024` dijawab 25 tahun, dan penelusuran `RM-CAP-007` selesai.

### Aturan urutan yang mengikat

Tiga aturan berikut berasal dari keputusan, bukan dari selera penyusunan:

| Aturan | Sumber |
|---|---|
| Penutupan tiga celah CPPT mendahului layar penelusuran | `RM-DEC-019` |
| Pengisian data lama mendahului pemanggilan pendaftaran keutuhan | Integration contract bagian 3 |
| Angka masa simpan ditetapkan sebelum migration tabel jejak | `RM-DEC-023`. **Terpenuhi** — 25 tahun, `RM-DEC-024` |

### Satu ketergantungan yang tidak boleh dilanggar

**Penguncian tanpa addendum akan melumpuhkan pekerjaan klinis.** Bila `BE-04` (menandatangani
dan mengunci) dirilis tanpa `BE-06` (addendum), tenaga klinis tidak punya cara apa pun
membetulkan catatan yang keliru. Keduanya **wajib** dirilis bersamaan sebagai satu potongan
kerja, bukan berurutan lintas rilis.

Ini bukan penyempurnaan melainkan syarat keselamatan: catatan klinis yang salah dan tidak dapat
dikoreksi lebih berbahaya daripada catatan yang dapat diubah bebas.

---

## 2. Ringkasan status seluruh task

Status per **14 Oktober 2026**. Semula per 26 Agustus 2026, setelah pengesahan `RM-DEC-027`.
Penyegaran 14 Oktober mencocokkan tabel dengan source dan dokumen pendamping; yang berubah hanya
keterangan `BE-08` dan `BE-09`, angka denominatornya tetap. **Penyegaran 2 November 2026:** `BE-08`
menjadi `SELESAI` setelah pengisian data lama dijalankan di devYoga.

| Milestone | Task | Status | Keterangan |
|---|---|---|---|
| B0 | `BE-00` | **`SELESAI`** 24 Agustus 2026 | Project uji backend, 4 uji lulus |
| B0 | `BE-01` | **`SELESAI`** 26 Agustus 2026 | 3 tabel keutuhan, 6 enum, migration, 7 uji lulus |
| B0 | `BE-02` | **`SELESAI`** 26 Agustus 2026 | Service keutuhan, 4 metode, 17 uji lulus |
| B1 | `BE-03` | **`SELESAI`** 26 Agustus 2026 | Tiga celah CPPT ditutup, 8 uji lulus |
| B1 | `BE-04`, `BE-05`, `BE-06` | **`SELESAI`** 26 Agustus 2026 | Tanda tangan, penetapan berhalangan, addendum — 3 controller, 17 uji lulus. Dirilis bersamaan sesuai aturan urutan |
| B1 | `BE-07` | **`SELESAI`** 26 Agustus 2026 | Penguncian saat kunjungan selesai, dipasang di **tiga** jalur, 4 uji lulus |
| B1 | `BE-08` | **`SELESAI`** 2 November 2026 | Alat dan panduannya selesai 26 Agustus 2026, 11 uji lulus. **Dijalankan di devYoga 2 November 2026** lewat kode service aslinya: penelaahan → percobaan → satu potongan sungguhan → penelaahan ulang. 10 CPPT lama kini `LockedUnsigned` (`BackfillEncounterClosed`), nol tersisa. Pemberitahuan unit rekam medis dan penelaahan ulang produksi dipindah menjadi **syarat pemasangan produksi** (keputusan pemilik modul). Lihat `BE-08-panduan-pengisian-data-lama.md` bagian 0b |
| B2 | `BE-09` | **`SELESAI` sebagian** 26 Agustus 2026 | Model, configuration, dan migration selesai. Butir (1) — DTO, controller, dan keenam endpoint api-contract bagian 7 — **ditutup `BE-20` 31 Agustus 2026**. Sisa satu butir **non-kode**: isi awal master keperluan akses menunggu SOP rekam medis; tidak ada seeder di source, isinya diisi lewat layar `FE-06` |
| B2 | `BE-10`, `BE-11` | **`SELESAI`** 26 Agustus 2026 | Tabel jejak akses, service kewenangan pasien, 12 uji lulus |
| B2 | `BE-12` | **`SELESAI`** 26 Agustus 2026 | Tinjauan akses, 4 endpoint, 6 uji lulus |
| B3 | `BE-13` | **`SELESAI`** 26 Agustus 2026 | Service penggabungan riwayat 13 sumber, 10 uji lulus |
| B3 | `BE-14` | **`SELESAI`** 27 Agustus 2026 | Endpoint berkas rekam medis, 4 endpoint, 11 uji lulus |
| B3 | `BE-15` | **`SELESAI` sebagian** 27 Agustus 2026 | Endpoint catatan pribadi, izin terpisah, 9 uji lulus. **Pemberitahuan ke penulis CPPT belum dijalankan** — lihat `BE-15-pemberitahuan-penulis-cppt.md` |
| B3 | `BE-16` | **`SELESAI`** 27 Agustus 2026 | Pengaman pasien bernomor ganda, 4 pintu masuk dijaga, 7 uji lulus |
| B4 | `BE-17` | **`SELESAI`** 27 Agustus 2026 | 14 jalur gagal seluruhnya punya uji dan lulus |
| B4 | `BE-18` | **`SELESAI`** 27 Agustus 2026 | Swagger dan catatan rilis, 3 uji lulus. Catatan rilis disetujui pemilik API pada hari yang sama (`RM-DEC-028`) |
| B5 | `BE-19` | **`SELESAI`** 31 Agustus 2026 | Penanda kunjungan aktif pada endpoint pencarian pasien, 12 uji lulus |
| B5 | `BE-20` | **`SELESAI`** 31 Agustus 2026 | `MedicalRecordAccessPurposeController`, 6 endpoint, 9 uji lulus |

**Denominator: 21 task. Sembilan belas `SELESAI`, dua `SELESAI` sebagian, nol `SIAP DIJALANKAN`,
nol `SIAP`, nol `TERTAHAN APPROVAL`** (per 2 November 2026; sebelumnya delapan belas `SELESAI` dan satu
`SIAP DIJALANKAN`, yaitu `BE-08`). Kedua `SELESAI` sebagian (`BE-09`, `BE-15`) kodenya sudah
lengkap; sisanya isi master dari SOP dan pemberitahuan penulis CPPT.

**SELURUH PEKERJAAN KODE BACKEND SELESAI.** Tidak ada lagi task yang menunggu ditulis. Yang
tersisa seluruhnya bukan pekerjaan kode — lihat tabel di bawah.

### Milestone B5 — dua task yang ditambahkan setelah frontend berjalan

Keduanya lahir dari temuan saat frontend dibangun, bukan dari perencanaan awal. Itu sebabnya
nomornya menyusul `BE-18` yang semula penutup roadmap.

#### `BE-19` — Penanda kunjungan aktif pada pencarian pasien

| Field | Isi |
|---|---|
| **Task ID** | `BE-19` |
| **Status** | **`SELESAI`** 31 Agustus 2026 |
| **Outcome** | Petugas tahu sebelum menekan tombol bahwa membuka berkas pasien tertentu akan meminta pernyataan keperluan akses |
| **Asal** | Temuan `FE-01`: kolom `Kunjungan` yang diwajibkan arsitektur frontend 11.1 tidak punya sumber data. `PatientResponse` tidak membawanya, dan `isActive` pada endpoint kunjungan berarti flag aktif baris, bukan "kunjungan sedang berjalan" |
| **Scope** | `PatientResponse.HasActiveEncounter` bertipe `bool?`; method batch pada `MedicalRecordAccessAuditService`; pengisian pada `GET /patients` |
| **Acceptance criteria** | 1) Nilainya selalu sepadan dengan `access.hasActiveEncounter` pada balasan ringkasan rekam medis. 2) Satu query untuk seluruh halaman, bukan satu per baris. 3) Tiga keadaan dipertahankan: `true`, `false`, dan `null` untuk tidak dihitung |
| **Verification** | 12 uji pada `ActiveEncounterFlagTests.cs`, membandingkan penilaian sekelompok terhadap penilaian satu per satu pada enam status kunjungan plus `CompletedAt`, `IsCancel`, dan pasien tanpa kunjungan |
| **Keputusan yang melekat** | Aturan "kunjungan masih berjalan" diangkat menjadi satu expression yang dipakai bersama kedua jalur — satu aturan, satu tempat. Penanda **tidak** dipasang pada `/patients/options` yang dibagi dengan kiosk |
| **DoD** | Terpenuhi |

#### `BE-20` — Controller master keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-20` |
| **Status** | **`SELESAI`** 31 Agustus 2026 |
| **Outcome** | Unit rekam medis dapat mengisi master keperluan akses tanpa meminta perubahan kode — dan dengan itu berkas pasien di luar rawatan dapat dibuka sama sekali |
| **Asal** | `BE-09` menyelesaikan tabel `MstMedicalRecordAccessPurpose` dan konfigurasi EF-nya, **bukan** controller-nya. Enam endpoint api-contract bagian 7 tidak pernah ditulis, dan `FE-06` tertahan karenanya |
| **Scope** | `MedicalRecordAccessPurposeDtos.cs`, `MedicalRecordAccessPurposeService.cs`, `MedicalRecordAccessPurposeController.cs`, registrasi DI |
| **Acceptance criteria** | 1) Keenam endpoint kontrak bagian 7 tersedia. 2) Kode keperluan kembar ditolak `409`. 3) Penonaktifan tidak menyentuh jejak akses yang sudah memakainya |
| **Verification** | 9 uji pada `MedicalRecordAccessPurposeMasterTests.cs` |
| **Keputusan yang melekat** | Controller tidak menyentuh `ApplicationDbContext` (QBE-SVC-001), berbeda dari 28 controller master data tetangganya. Tidak ada endpoint hapus. `/options` memakai `MedicalRecordAccessPurposeOptionResponse` yang sudah ada, bukan tipe `OptionResponse` yang disebut kontrak tetapi tidak ada di codebase |
| **Migration** | Tidak diperlukan — entity, configuration, dan index unik `PurposeCode` sudah ada sejak `BE-09` |
| **DoD** | Terpenuhi |

Milestone B0, B1, dan B2 tuntas — kecuali dua hal yang bukan pekerjaan kode: isi awal master
keperluan akses (`BE-09`, menunggu SOP) dan ~~penjalanan pengisian data lama (`BE-08`)~~ — dijalankan di
devYoga 2 November 2026.

**Milestone B3 tuntas.** Seluruh kode `BE-13` sampai `BE-16` selesai dan terbukti uji; yang
tersisa pada `BE-15` bukan pekerjaan kode, melainkan pemberitahuan kepada penulis CPPT yang
diwajibkan `RM-DEC-022`.

**Tiga butir yang bukan pekerjaan kode dan menahan penyelesaian penuh:**

| Butir | Task | Menunggu | Akibat bila dilewati |
|---|---|---|---|
| Isi awal master keperluan akses | `BE-09` | SOP rekam medis rumah sakit | **Seluruh** pembukaan rekam medis pasien di luar rawatan ditolak. **Sudah terjadi sekarang** — tabelnya ada sejak 27 Agustus 2026, isinya nol |
| ~~Penjalanan pengisian data lama~~ | `BE-08` | **Selesai di devYoga** 2 November 2026; di produksi menjadi syarat pemasangan (penelaahan ulang + pemberitahuan unit rekam medis) | Catatan lama tidak punya baris keutuhan |
| Pemberitahuan penulis CPPT | `BE-15` | Pemilik modul menjalankan penyampaian | Penulis CPPT tidak tahu catatan pribadinya dapat dibuka lewat jalur sah |

Butir keempat — persetujuan catatan rilis — **tertutup 27 Agustus 2026** lewat `RM-DEC-028`.

Satu pemeriksaan data juga disarankan sebelum modul dipakai: jumlah pasien yang
`MergedToPatientId`-nya sudah terisi. Bila ada, berkas mereka tidak dapat dibuka lewat nomor
lama. Lihat laporan `BE-16`.

**Bukti kumulatif:** `dotnet test` → `Failed: 0, Passed: 132`. `dotnet build` → 0 error.

### Keadaan database — diterapkan 27 Agustus 2026

Bagian ini dicatat terpisah karena **uji hijau tidak membuktikan database sungguhan sudah siap**.
Uji berjalan di atas SQLite dalam memori yang tabelnya dibentuk langsung dari konfigurasi EF
Core, tanpa melewati migration sama sekali.

| Field | Isi |
|---|---|
| Basis data | `QuilvianNewDevTim01` pada `160.22.250.77` — **basis data dev bersama**, bukan lokal |
| Dijalankan | `dotnet ef database update`, tanpa `--force`, tanpa reset |
| Otorisasi | Diberikan pemilik modul pada 27 Agustus 2026 |

Dua migration yang diterapkan:

| Migration | Tabel yang dibuat |
|---|---|
| `20260826034557_AddMedicalRecordIntegrityTables` | `TrxClinicalDocumentIntegrity`, `TrxClinicalNoteAddendum`, `TrxClinicalNoteAuthorDelegation` |
| `20260826081755_AddMedicalRecordAccessAuditTables` | `TrxMedicalRecordAccessLog`, `MstMedicalRecordAccessPurpose` |

**Lima tabel baru, nol perubahan kolom** pada tabel yang sedang dipakai — sesuai `RM-DEC-013`.
Alur IGD, antrean dokter, dan farmasi tidak tersentuh.

Bukti sebelum dan sesudah, dari `dotnet ef migrations list`:

| Keadaan | Migration tertunda |
|---|---:|
| Sebelum | 2 — keduanya milik modul rekam medis |
| Sesudah | **0** |

Total 107 migration; 105 sisanya sudah diterapkan sebelumnya, termasuk seluruh migration dari
lima branch yang di-merge pada hari yang sama. **Tidak ada migration modul lain yang ikut
terbawa**, dan kedua migration rekam medis berada paling akhir dalam urutan sehingga tidak
menyisipkan apa pun di tengah.

**Kenapa baru dijalankan sekarang.** Aturan kerja modul ini memisahkan empat wewenang: menulis
source, membuat migration, menjalankan ke database, dan deploy. Ketiga wewenang pertama dipakai
sepanjang `BE-01` sampai `BE-18`; wewenang menjalankan ke database baru diberikan 27 Agustus
2026, setelah endpoint `medical-record-access-logs` menjawab `500` dengan sebab
`42P01: relation "public.TrxMedicalRecordAccessLog" does not exist`.

**Yang masih kosong isinya.** `MstMedicalRecordAccessPurpose` terbuat tetapi **nol baris**,
karena isi awalnya menunggu SOP rekam medis (`BE-09`). Akibat nyatanya sekarang:

| Keadaan | Perilaku |
|---|---|
| Membuka berkas pasien yang punya kunjungan berjalan | Berfungsi |
| Membuka berkas pasien di luar rawatan | Ditolak `400` — tidak ada keperluan akses yang dapat dipilih |
| `/filters/metadata` | Berfungsi, mengembalikan `isAccessPurposeMasterEmpty: true` beserta peringatannya |

Baris kedua **bukan kerusakan**, melainkan penerapan `RM-DEC-005` yang menutup rapat.

### Hasil `BE-18`

| Berkas | Status |
|---|---|
| `QuilvianSystemBackend.csproj` | Diperbarui — `GenerateDocumentationFile` menyala |
| `Program.cs` | Diperbarui — Swagger membaca berkas dokumentasi XML |
| `Areas/.../ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs` | Diperbarui — keterangan pada dua kolom yang diabaikan |
| `Areas/.../ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Diperbarui — keterangan pada endpoint ubah CPPT |
| `catatan-rilis.md` | Baru — catatan rilis modul |
| `tests/.../SwaggerDocumentationTests.cs` | Baru — 3 uji |

**Yang mengubah keadaan: XML documentation diaktifkan.** Sebelum task ini, Swagger **tidak**
membaca komentar keterangan sama sekali — `GenerateDocumentationFile` tidak menyala dan
`IncludeXmlComments` tidak dipasang. Artinya seluruh komentar yang ditulis sepanjang `BE-01`
sampai `BE-17` tidak pernah tampil di halaman Swagger.

Menuliskan keterangan tanpa menyalakannya berarti acceptance criteria nomor 1 tidak akan pernah
terpenuhi, sebanyak apa pun komentar ditulis. Karena itu penyalaannya menjadi bagian task ini.

Peringatan `CS1591` disenyapkan dengan sadar. Peringatan itu menuntut komentar XML pada **setiap**
anggota publik di seluruh aplikasi; menyalakannya menghasilkan ribuan peringatan yang
menenggelamkan peringatan sungguhan. Terbukti pada hasil build: **0 peringatan `CS1591`**.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Swagger menyebut `ProviderUserId` dan `IsReadOnlyGenerated` diabaikan | `Swagger_MenyebutDuaKolomYangDiabaikanPadaPermintaanUbah` |
| 3) Keterangan bahwa baru CPPT yang tunduk aturan keutuhan | `Swagger_MenyatakanBaruCpptYangTundukAturanKeutuhan` |
| Kode `400` baru pada catatan terkunci beserta jalan keluarnya | `Swagger_MenyebutPenolakanPadaCatatanTerkunciBesertaJalanKeluarnya` |

**Kenapa diuji otomatis padahal verifikasinya manual.** Roadmap menyebut verifikasi `BE-18`
berupa pemeriksaan manual halaman Swagger. Masalahnya, pemeriksaan manual hanya berlaku pada hari
ia dilakukan — keterangannya dapat terhapus pada perubahan berikutnya tanpa ada yang menyadari.
Uji memeriksa berkas dokumentasi XML hasil build, yaitu sumber yang dipakai Swagger. Bila
keterangannya hilang atau berkas dokumentasinya berhenti dihasilkan, uji gagal.

Yang tetap perlu dilihat dengan mata sekali: tampilan halaman Swagger-nya sendiri.

**Acceptance criteria 2 — catatan rilis** dipenuhi `catatan-rilis.md`, memuat keempat perubahan
perilaku pada manifest, cakupan aturan keutuhan, endpoint baru beserta kode statusnya, hak akses
baru, dan daftar hal yang wajib disiapkan sebelum modul dipakai.

**DoD lengkap sejak 27 Agustus 2026.** Definition of Done menuntut catatan rilis disetujui
pemilik API. Pemilik API ditetapkan pada hari yang sama — Yoga Aji Pratama, `RM-DEC-028` — dan
catatan rilis beserta kontrak `0.1.0` disahkan bersamanya. Dua hal yang ikut disahkan: perubahan
bentuk balasan `/timeline`, dan field `access` pada seluruh balasan endpoint berkas.

### Hasil `BE-17`

| Berkas | Status |
|---|---|
| `tests/.../MedicalRecordFailurePathTests.cs` | Baru — 6 uji |
| `tests/.../Infrastructure/HubContextKosong.cs` | Baru — konteks SignalR tiruan |

Tidak ada perubahan pada source aplikasi. Task ini murni pembuktian.

#### Peta lengkap empat belas jalur gagal

Acceptance test matrix bagian 3 mendaftar empat belas jalur gagal. Berikut uji yang menutup
masing-masing, seluruhnya lulus:

| No | Jalur gagal | ID | Uji | Ditutup task |
|---:|---|---|---|---|
| 1 | Mengubah dokumen terkunci | `AT-RM-01` | `MengubahCpptYangSudahDitandatangani_DitolakDanIsinyaTidakBerubah` | `BE-03` |
| 2 | Menandatangani catatan orang lain | `AT-RM-02` | `BukanPenulis_TidakDapatMenandatangani` | `BE-02` |
| 3 | Menandatangani dokumen yang sudah terkunci | `AT-RM-11` | `DokumenYangSudahTerkunci_TidakDapatDitandatanganiUlang` | `BE-02` |
| 4 | Addendum oleh yang tidak berwenang | `AT-RM-05` | `BukanPenulis_TanpaKewenanganPengganti_Ditolak` | `BE-06` |
| 5 | Addendum memakai penetapan kedaluwarsa | `AT-RM-27` | `PenetapanYangSudahLewatBatasWaktu_TidakLagiMembukaJalur` | `BE-06` |
| 6 | Penetapan tanpa batas waktu | `AT-RM-26` | `PenetapanTanpaBatasWaktu_DitolakDanTidakTersimpan` | **`BE-17`** |
| 7 | Akses tanpa alasan pada pasien tanpa kunjungan aktif | `AT-RM-07` | `PasienTanpaKunjunganAktif_TanpaKeperluan_DitolakDanIsinyaTidakDikembalikan` | `BE-11` |
| 8 | `SuperAdmin` tanpa alasan | `AT-RM-13` | `SuperAdmin_TetapDimintaAlasanDanTetapDitandaiPerluDitinjau` | **`BE-17`** |
| 9 | Penilaian kunjungan gagal | `AT-RM-25` | `PenilaianKunjunganGagal_DiperlakukanSebagaiAksesBeralasan` | `BE-11` |
| 10 | Pencatatan jejak gagal | `AT-RM-30` | `PencatatanJejakGagal_Dijawab503DanIsiTidakDikembalikan` | **`BE-17`** |
| 11 | Pendaftaran keutuhan gagal | `AT-RM-35` | `PendaftaranKeutuhanGagal_PembuatanCpptIkutDibatalkan` | **`BE-17`** |
| 12 | Penguncian saat penutupan gagal | `AT-RM-36` | `PenguncianGagalSaatKunjunganDitutup_KunjunganTetapTerbuka` | **`BE-17`** |
| 13 | Pasien hasil penggabungan | `AT-RM-22` | `PasienHasilPenggabungan_DitolakPadaSeluruhPintuMasukBerkas` | `BE-16` |
| 14 | Data lama tanpa penulis | `AT-RM-33` | `CatatanTanpaPenulis_TetapDibuatDenganPenandaPenulisTidakDiketahui` | `BE-08` |

**Sepuluh sudah terbukti pada task pendahulunya, empat ditutup di sini** — ditambah satu yang
semula terlihat tertutup padahal belum, lihat di bawah.

#### Cara menirukan kegagalan

Tabel yang bersangkutan **dihapus** dari basis data uji, sehingga query ke tabel itu benar-benar
gagal. Bukan disimulasikan lewat penanda maupun tiruan objek — yang diuji adalah perilaku sistem
ketika basis datanya sungguh-sungguh bermasalah.

| Uji | Tabel yang dihapus |
|---|---|
| `AT-RM-30` | `TrxMedicalRecordAccessLog` |
| `AT-RM-35` | `TrxClinicalDocumentIntegrity` |
| `AT-RM-36` | `TrxClinicalDocumentIntegrity` |

#### Temuan: `AT-RM-26` semula belum benar-benar tertutup

`BE-05` sudah punya `PenetapanDenganBatasWaktuYangSudahLewat_Ditolak`, yang menguji batas waktu
**diisi tetapi sudah lewat**. Yang diminta `AT-RM-26` berbeda: batas waktunya **tidak diisi sama
sekali**.

Bedanya bukan main-main. Atribut `[Required]` pada kolom tanggal yang tidak boleh kosong **tidak**
menangkap keadaan itu — nilai bawaan tanggal bukan nilai kosong, sehingga lolos pemeriksaan
atribut. Yang benar-benar menahannya adalah aturan "batas waktu harus setelah hari ini" di dalam
service, yang kebetulan juga menolak nilai bawaan karena nilai itu berada di masa lampau.

Perilakunya sudah benar, tetapi **belum pernah dibuktikan**. Sekarang sudah, dan uji itu menjaga
agar aturannya tidak dilonggarkan seseorang yang mengira `[Required]` sudah cukup.

#### Alat bantu baru: konteks SignalR tiruan

`AT-RM-36` menuntut pengujian lewat `PatientEncounterController`, yang konstruktornya menerima
layanan antrean realtime walaupun endpoint yang diuji tidak memakainya. Tanpa tiruan, controller
itu tidak dapat dibentuk sama sekali dari uji.

Tiruannya sengaja tidak mencatat apa pun. Bila kelak ada uji yang perlu membuktikan pesan
realtime terkirim, tiruan itu perlu diganti yang mencatat panggilannya — bukan dipakai apa adanya
lalu dianggap membuktikan sesuatu.

#### Satu uji pembanding

`PenguncianSehat_PenutupanKunjunganBerhasil` memastikan uji `AT-RM-36` benar-benar membuktikan
**penguncian yang gagal** membatalkan penutupan — bukan sekadar membuktikan bahwa penutupan
memang tidak pernah bekerja.

### Hasil `BE-16`

| Berkas | Status |
|---|---|
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordAccessAuditService.cs` | Diperbarui — penelusuran rantai penggabungan |
| `tests/.../MergedPatientGuardTests.cs` | Baru — 7 uji |

**Sebagian besar perilakunya sudah berjalan sejak `BE-11`.** Pemeriksaan `MergedToPatientId`
diletakkan di service jejak akses, yang dipanggil **seluruh** endpoint berkas rekam medis
sebelum isi diambil. Karena itu `BE-16` sebagian besar berupa pembuktian, bukan penulisan
perilaku baru.

**Yang benar-benar ditambahkan: penelusuran rantai penggabungan.** Sebelumnya nomor pengganti
diambil satu langkah saja. Bila pasien A digabung ke B dan B kemudian digabung ke C, pengguna
disuruh membuka nomor B — yang juga akan ditolak. Petunjuk menyesatkan seperti itu membuat
sistem terlihat rusak. Sekarang yang disebut adalah nomor di ujung rantai.

Rantai ditelusuri paling banyak sepuluh langkah, dan setiap pasien yang dilewati dicatat.
Keduanya mencegah rantai melingkar — A ke B lalu B kembali ke A — membuat permintaan berjalan
tanpa akhir. Keadaan itu seharusnya tidak pernah terjadi, tetapi tidak ada satu pun aturan di
sistem yang mencegahnya: pemeriksaan saat menggabungkan hanya memastikan pasien tujuan ada dan
aktif, tanpa memeriksa apakah tujuan itu kelak ikut digabungkan.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Dijawab `409` disertai nomor pengganti, pada **seluruh** pintu masuk (`AT-RM-22`) | `PasienHasilPenggabungan_DitolakPadaSeluruhPintuMasukBerkas` |
| 2) Riwayat sebagian tidak ditampilkan walaupun datanya ada | `RiwayatSebagian_TidakDitampilkanWalaupunDatanyaAda` |
| Penolakan `409` tidak menghasilkan jejak akses | `Penolakan409_TidakMenghasilkanJejakAkses` |
| Rantai penggabungan menyebut nomor ujung rantai | `PenggabunganBerantai_MenyebutNomorUjungRantai` |
| Rantai melingkar tetap dijawab, tidak menggantung | `RantaiPenggabunganMelingkar_TetapDijawabTanpaMenggantung` |
| Nomor pengganti selalu menunjuk pasien yang ada | `NomorPengganti_SelaluMenunjukPasienYangAda` |
| Pasien tujuan penggabungan tetap dapat dibuka | `PasienTujuanPenggabungan_TetapDapatDibuka` |

**Keempat pintu masuk diperiksa, bukan hanya riwayat.** Ringkasan, riwayat, detail dokumen, dan
catatan pribadi diuji satu per satu. Satu pintu yang lupa dijaga sudah cukup untuk menampilkan
riwayat terpecah, dan pengaman yang berlubang di satu tempat bukan pengaman.

**Pasien tujuan penggabungan tetap dapat dibuka.** Diuji tersendiri karena kekeliruan ke arah
sebaliknya berakibat fatal: bila nomor penggantinya ikut tertutup, pasiennya justru kehilangan
seluruh berkas — kebalikan dari maksud `RM-DEC-026`.

**Hasil penelusuran yang menjadi dasar task ini** sudah tercatat sejak 24 Agustus 2026 pada
`01-existing-capability-map.md` bagian "Penelusuran lanjutan `RM-CAP-007`". Ringkasnya:
penggabungan pasien di sistem ini hanya penandaan, tidak memindahkan data klinis apa pun, dan
tidak ada query di modul mana pun yang mengikuti `MergedToPatientId`.

**Satu pertanyaan yang masih terbuka, dan akibatnya perlu diketahui.** Closure question nomor 10
belum terjawab: berapa banyak pasien yang `MergedToPatientId`-nya sudah terisi pada data nyata.
Pertanyaan itu tidak dapat dijawab dari source. Akibatnya: **bila ternyata ada pasien seperti
itu, berkas mereka menjadi tidak dapat dibuka lewat nomor lama sejak modul ini dipakai** —
pengguna wajib membuka nomor penggantinya. Itu memang perilaku yang dikehendaki `RM-DEC-026`,
tetapi jumlahnya perlu diketahui sebelum modul dipakai agar unit rekam medis tidak terkejut.

Satu hal yang meringankan: `RM-FACT-008` mencatat fitur penggabungan **tidak dapat dipakai dari
antarmuka** karena `mergeReason` tidak pernah dikirim. Selama celah itu terbuka, tidak ada pasien
bernomor ganda baru yang tercipta.

### Hasil `BE-15`

| Berkas | Status |
|---|---|
| `Areas/.../MedicalRecordManagement/Controllers/MedicalRecordController.cs` | Diperbarui — 1 endpoint |
| `Areas/.../MedicalRecordManagement/DTOs/MedicalRecordDtos.cs` | Diperbarui — `MedicalRecordPrivateNoteResponse` |
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Diperbarui — `GetPrivateNoteAsync`, `MendukungCatatanPribadi` |
| `tests/.../MedicalRecordPrivateNoteTests.cs` | Baru — 9 uji |
| `roadmap/BE-15-pemberitahuan-penulis-cppt.md` | Baru — bahan komunikasi untuk DoD |

#### Endpoint

| Method | Path | Hak akses | Query |
|---|---|---|---|
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | `MedicalRecord : ReadPrivateNote` | `accessPurposeId` **wajib**, `accessReason` |

**Sebagian besar aturannya sudah ada sejak `BE-11`.** Service jejak akses sudah menegakkan
"catatan pribadi selalu menuntut keperluan" beserta pesannya yang khusus. `BE-15` tinggal
memanggilnya dengan cakupan `PrivateNote` dan memasang izin terpisah. Ini hasil langsung dari
keputusan `BE-11` meletakkan aturan itu di satu service, bukan di controller.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Alasan diminta walaupun pasien punya kunjungan aktif (`AT-RM-16`) | `CatatanPribadi_TetapMenuntutAlasanWalaupunPasienSedangDirawat` |
| 1) Dengan keperluan sah, catatan benar-benar terbuka | `CatatanPribadi_TerbukaBilaKeperluanDiisi` |
| 2) Jejak tercatat dengan `AccessScope = PrivateNote` | `CatatanPribadi_JejakTercatatDenganCakupanPrivateNote` |
| 2) Terhitung terpisah pada rekap tinjauan `BE-12` | `PembukaanCatatanPribadi_TerhitungTerpisahPadaRekapTinjauan` |
| 3) Memakai izin terpisah, bukan izin baca biasa | `EndpointCatatanPribadi_MemakaiIzinTerpisah` |
| Jenis tanpa catatan pribadi ditolak jelas, tanpa jejak | `JenisDokumenTanpaCatatanPribadi_Dijawab404TanpaJejak` |
| "Memang kosong" dibedakan dari "disembunyikan" | `DokumenTanpaIsiCatatanPribadi_DitandaiKosongBukanDisembunyikan` |
| Catatan pribadi pasien lain ditolak `404` | `CatatanPribadiPasienLain_Dijawab404` |
| Pasien tanpa kunjungan berjalan tetap ditolak tanpa keperluan | `PasienTanpaKunjunganBerjalan_TetapDitolakTanpaKeperluan` |

**Uji pembanding pada acceptance criteria nomor 1.** Pada pasien yang sama dan pengguna yang
sama, detail dokumen biasa dijawab `200` tanpa keperluan akses, sedangkan catatan pribadinya
dijawab `400`. Inilah bukti bahwa aturan `RM-DEC-022` benar-benar berbeda dari aturan isi rekam
medis lainnya, bukan sekadar dinyatakan.

**Cara membuktikan izin terpisah.** Uji memanggil controller langsung tanpa melewati lapisan hak
akses, sehingga izinnya diperiksa pada atributnya: endpoint catatan pribadi membawa
`ReadPrivateNote`, sedangkan endpoint detail dokumen membawa `Read`. Ini menutup pelajaran
`BE-06` — satu endpoint hanya boleh punya satu `[AccessAction]`, jadi bila izin ini digabungkan
ke endpoint detail, hak `ReadPrivateNote` tidak akan pernah terdaftar dan tidak dapat diberikan
kepada siapa pun.

**Yang belum selesai, dan bukan pekerjaan kode.** DoD `BE-15` menuntut dua hal: endpoint berjalan
**dan** penulis CPPT sudah diberi tahu bahwa kolom itu ternyata dapat dibuka lewat jalur sah.
Butir kedua belum dijalankan. Bahan siap pakainya ada pada
`BE-15-pemberitahuan-penulis-cppt.md`, tetapi penyampaiannya adalah pekerjaan pemilik modul.

### Hasil `BE-14`

| Berkas | Status |
|---|---|
| `Areas/.../MedicalRecordManagement/Controllers/MedicalRecordController.cs` | Baru — 4 endpoint |
| `Areas/.../MedicalRecordManagement/DTOs/MedicalRecordDtos.cs` | Baru — bentuk balasan seluruh endpoint |
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Diperbarui — 3 metode baca baru |
| `tests/.../MedicalRecordFileEndpointTests.cs` | Baru — 11 uji |
| `contracts/api-contract.md` | Diperbarui — status endpoint dan catatan delta |

**Tidak ada tabel baru, tidak ada migration, tidak ada `AddScoped` baru.** Controller ASP.NET
tidak perlu didaftarkan, dan kedua service yang dipakainya sudah terdaftar sejak `BE-11` dan
`BE-13`.

#### Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Method | Path | Kegunaan | Hak akses | Query |
|---|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas | `MedicalRecord : Read` | `accessPurposeId`, `accessReason` |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan lintas kunjungan | `MedicalRecord : Read` | `documentKinds`, `encounterId`, `startDate`, `endDate`, `includeCancelled`, `newestFirst`, `page`, `pageSize`, `accessPurposeId`, `accessReason` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail dokumen beserta addendum | `MedicalRecord : Read` | `accessPurposeId`, `accessReason` |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` | — |

Endpoint `private-note` **belum** dibuat — itu scope `BE-15`.

**Urutan yang mengikat pada ketiga endpoint pembuka berkas.** Kewenangan dinilai dan jejak
ditulis lebih dulu; baru setelah itu isi diambil. Bila penilaian menolak atau pencatatan jejak
gagal, permintaan berhenti tanpa menyentuh isi rekam medis sama sekali. `/filters/metadata`
**tidak** mencatat jejak karena tidak menyentuh data pasien mana pun.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Setiap permintaan melewati pencatatan jejak lebih dulu | `SetiapPembukaan_MencatatJejakLebihDulu` |
| 1) Akses ditolak, isi tidak dikembalikan | `AksesDitolak_IsiRekamMedisTidakDikembalikan` |
| 1) Akses beralasan dilayani dan dinyatakan akan ditelaah | `AksesBeralasan_DilayaniDanPenggunaDiberiTahuAkanDitelaah` |
| 2) Status keutuhan ikut dikembalikan | `StatusKeutuhan_IkutDikembalikanUntukJenisYangSudahTunduk` |
| 3) Jenis yang belum tunduk ditandai jelas (`AT-RM-32`) | `JenisYangBelumTundukAturanKeutuhan_DitandaiJelas` |
| 4) `PrivateNote` tidak ada di respons mana pun (`AT-RM-37`) | `CatatanPribadi_TidakAdaPadaResponsManaPun` |
| Dokumen pasien lain ditolak `404` | `DokumenMilikPasienLain_Dijawab404` |
| Pasien hasil penggabungan ditolak `409` | `PasienHasilPenggabungan_Dijawab409TanpaRiwayatSebagian` |
| Ringkasan berkas (`AT-RM-09`) | `RingkasanBerkas_MemuatIdentitasAlergiAktifDanJumlahDokumen` |
| Peringatan master keperluan kosong | `DaftarPilihan_MemperingatkanBilaMasterKeperluanKosong` |
| Daftar pilihan tidak menghasilkan jejak | `DaftarPilihan_TidakMenghasilkanJejakAkses` |

**Cara membuktikan `PrivateNote` tidak bocor.** Seluruh balasan diubah menjadi teks JSON lalu
dicari isinya. Bila kolom itu bocor lewat jalur mana pun — bagian isi dokumen, judul, keterangan
pendek, atau kolom yang tidak sengaja ikut — uji gagal. Yang tetap dikembalikan hanya penanda
`hasPrivateNote`, karena tanpanya tidak ada yang tahu ada sesuatu yang dapat dibuka lewat jalur
sah pada `BE-15`.

**Perubahan kontrak: bentuk balasan `/timeline`.** Dari
`ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>` menjadi
`ApiResponse<MedicalRecordTimelineResponse>`. Bentuk semula tidak punya tempat untuk menyatakan
sumber yang gagal dibaca, padahal acceptance criteria `BE-13` nomor 4 mewajibkannya. Selubung
barunya memuat halaman yang sama pada field `page`, ditambah `access`, `requestedKinds`,
`failedSources`, `isTruncated`, dan `isComplete`. Rinciannya pada `contracts/api-contract.md`
bagian 2. **Belum disahkan pemilik API** — `api_authority` masih `OPEN`.

**Keputusan letak kode.** Ringkasan berkas dan detail dokumen diletakkan di
`MedicalRecordTimelineService`, bukan di controller, karena arsitektur bagian 5.9 mewajibkan
controller rekam medis memakai service dan tidak menyentuh `ApplicationDbContext` langsung.
Alasan kedua: pengetahuan tentang tiga belas tabel klinis hanya boleh tinggal di satu tempat.

**Yang belum diverifikasi.** Uji memanggil controller langsung, bukan lewat HTTP, sehingga
lapisan `[AccessPermission]`, `[Authorize]`, dan model binding **tidak** ikut terbukti. Tampilan
Swagger juga belum dibuka, walaupun atributnya sudah dipasang.

### Hasil `BE-13`

| Berkas | Status |
|---|---|
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Baru — penggabungan 13 sumber |
| `Areas/.../MedicalRecordManagement/DTOs/MedicalRecordTimelineDtos.cs` | Baru — permintaan, baris riwayat, sumber gagal, hasil |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordTimelineTests.cs` | Baru — 10 uji |

**Tidak ada tabel baru, tidak ada migration, tidak ada endpoint.** Task ini murni lapisan
pembaca di atas tiga belas tabel klinis yang sudah ada, persis seperti status audit
`Reuse with adapter` pada `RM-CAP-004`. Endpoint-nya menyusul pada `BE-14`.

**Bisnis prosesnya, urut.** Petugas membuka berkas rekam medis seorang pasien. Sebelum service
ini ada, layar harus memanggil sampai tiga belas endpoint terpisah — CPPT, konsultasi, asesmen,
diagnosis, tindakan, tanda vital, alergi, riwayat penyakit, riwayat keluarga, dokumen klinis,
lampiran, surat keterangan, dan persetujuan — masing-masing dengan penomoran halaman sendiri,
lalu mengurutkan sendiri hasilnya. Sekarang alurnya menjadi:

1. Pemanggil menyebut pasien, jenis dokumen yang ingin dilihat, rentang tanggal, dan halaman.
2. Service menanyakan **hanya** jenis yang diminta ke tabel masing-masing, dengan batas baris.
3. Seluruh hasil digabung, diurutkan menurut waktu kejadian, lalu dipotong sesuai halaman.
4. Baris yang benar-benar ditampilkan dilengkapi status keutuhan dokumennya.
5. Bila ada sumber yang gagal dibaca, namanya ikut dikembalikan bersama hasilnya.

**Contoh.** Pasien dengan tiga kunjungan: CPPT 90 hari lalu, alergi 30 hari lalu, dan
persetujuan tindakan 2 hari lalu. Satu permintaan mengembalikan ketiganya sebagai satu daftar
berurut waktu, lengkap dengan nomor kunjungan masing-masing. Petugas tidak perlu membuka
kunjungan satu per satu.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu (`AT-RM-09`) | `RiwayatTigaKunjungan_TampilSebagaiSatuDaftarBerurutWaktu` |
| 2) Jumlah baris dibatasi dan penyaringan tanggal berfungsi (`AT-RM-31`) | `PasienDenganBanyakDokumen_JumlahBarisDibatasiDanTanggalTersaring` |
| 3) Hanya jenis dokumen yang diminta yang diambil | `HanyaJenisYangDiminta_YangDiambil` |
| 4) Satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai | `SatuSumberGagal_SumberLainTetapTampilDanYangGagalDitandai` |
| 5) Memakai `AsNoTracking` | `SeluruhPembacaan_TidakMeninggalkanEntityTerlacak` |
| Penyaring kunjungan mempersempit ke satu kunjungan | `PenyaringKunjungan_HanyaMengambilDokumenKunjunganItu` |
| Status keutuhan menempel; jenis yang belum tunduk ditandai terbuka | `StatusKeutuhan_DitempelkanUntukJenisYangSudahDitegakkan` |
| Dokumen dibatalkan tidak tampil kecuali diminta | `DokumenDibatalkan_TidakTampilKecualiDiminta` |
| Riwayat pasien lain tidak ikut terbawa | `RiwayatPasienLain_TidakIkutTerbawa` |
| Permintaan tanpa id pasien ditolak, bukan dijawab daftar kosong | `TanpaIdPasien_PermintaanDitolak` |

**Cara membuktikan sumber gagal, bukan sekadar disimulasikan.** Uji nomor 4 menghapus satu
tabel dari basis data uji, sehingga pembacaannya benar-benar gagal. Hasilnya: dua sumber lain
tetap terbaca, dan jenis `Persetujuan Tindakan` muncul pada daftar sumber gagal. Ini penting
karena penandanya bukan hiasan — layar wajib menyatakan bahwa daftar yang tampil belum lengkap.

**Dua query per sumber, bukan satu.** Satu untuk menghitung jumlah dokumen, satu untuk mengambil
barisnya. Penghitungan dipilih dengan sadar: tanpa itu, jumlah total pada layar hanya sebesar
isi halaman dan tombol "halaman berikutnya" kehilangan artinya. Query penghitungan tidak
mengambil baris apa pun dan dilayani index `(PatientId, tanggal, IsDelete)` yang sudah ada di
tiga belas tabel klinis. Konsekuensinya harus dipahami pemakai kontrak: meminta seluruh jenis
berarti dua puluh enam query, sedangkan meminta satu jenis berarti dua. **Menyebut jenis
dokumen adalah cara termurah menekan biaya permintaan.**

**Batas yang berlaku.** Ukuran halaman bawaan 25 dan paling besar 100; permintaan yang lebih
besar dipotong ke batas itu, bukan ditolak. Setiap sumber paling banyak menyerahkan 500 baris
dalam satu permintaan. Bila batas itu tersentuh, hasilnya ditandai terpotong — jumlah totalnya
tetap benar, tetapi isi halaman yang jauh belum tentu lengkap.

**Daftar riwayat sengaja tidak membawa isi catatan klinis.** Setiap baris hanya memuat nomor
dokumen, judul pendek, dan keterangan singkat dari kolom yang panjangnya sudah terbatas.
Isi lengkap dokumen — termasuk `PrivateNote` — tidak pernah ikut, dan itu memang harus dibuka
lewat endpoint detail yang jalur aksesnya tercatat tersendiri (`BE-14`, `BE-15`).

**Delta kontrak yang perlu diputuskan pada `BE-14`.** Api-contract bagian 2 menyebut balasan
`/timeline` berbentuk `ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>`. Bentuk itu
tidak punya tempat untuk daftar sumber gagal, padahal acceptance criteria nomor 4 mewajibkan
kekurangan dinyatakan. Karena `BE-13` hanya lapisan service, keputusannya belum diambil di
sini: service mengembalikan `MedicalRecordTimelineResult` yang memuat halaman **beserta**
daftar sumber gagal dan penanda terpotong. `BE-14` yang menentukan bagaimana keterangan itu
tampil pada balasan endpoint.

### Hasil `BE-09`, `BE-10`, dan `BE-11`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs` | Baru |
| `Areas/.../MedicalRecordManagement/Models/TrxMedicalRecordAccessLog.cs` | Baru |
| `Repositories/Configurations/HealthServices/MedicalRecordManagement/` | 2 configuration baru |
| `Services/MedicalRecordAccessAuditService.cs` | Baru — penilaian kewenangan dan pencatatan jejak |
| `Migrations/20260826081755_AddMedicalRecordAccessAuditTables.cs` | Baru |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordAccessAuditTests.cs` | Baru — 12 uji |

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Pasien berkunjungan aktif dibuka tanpa alasan, tetap tercatat | `PasienDenganKunjunganAktif_DibukaTanpaAlasanDanTetapTercatat` |
| Tanpa kunjungan aktif dan tanpa keperluan ditolak, **isi tidak dikembalikan** | `PasienTanpaKunjunganAktif_TanpaKeperluan_DitolakDanIsinyaTidakDikembalikan` |
| Dengan keperluan sah, diizinkan dan ditandai untuk ditinjau | `PasienTanpaKunjunganAktif_DenganKeperluan_DiizinkanDanDitandaiUntukDitinjau` |
| Catatan pribadi **selalu** menuntut keperluan | `CatatanPribadi_SelaluMenuntutKeperluanWalauPasienSedangDirawat` |
| Sepuluh pembukaan menghasilkan tepat sepuluh jejak | `SepuluhPembukaan_MenghasilkanTepatSepuluhBarisJejak` |
| Pasien hasil penggabungan ditolak `409` disertai nomor pengganti | `PasienHasilPenggabungan_DitolakDisertaiNomorPengganti` |
| Kegagalan penilaian **tidak** melonggarkan kewenangan | `PenilaianKunjunganGagal_DiperlakukanSebagaiAksesBeralasan` |

**Kunci utama gabungan.** `TrxMedicalRecordAccessLog` memakai kunci utama `(Id, AccessedAt)`,
bukan `Id` saja. Ini disiapkan untuk pembagian tabel per tahun: PostgreSQL mensyaratkan kolom
pembagi ikut menjadi bagian kunci utama, dan **mengubah kunci utama pada tabel berisi jutaan
baris adalah bagian yang menuntut waktu henti layanan**. Menyiapkannya sejak awal menghilangkan
biaya itu.

**Yang belum dikerjakan pada pembagian tabel.** Perintah `PARTITION BY RANGE` belum diterapkan;
tabelnya masih tabel biasa. Alasannya: pada volume sekarang, pembagian adalah pekerjaan
sia-sia. Yang mahal untuk ditunda — bentuk kunci utama — sudah dikerjakan, sehingga penerapan
pembagian kelak hanya memerlukan pemindahan data, bukan perubahan skema kunci.

### Yang tersisa pada `BE-09`

Struktur tabelnya selesai dan siap dipakai. **Isi awalnya belum**, karena membutuhkan SOP rekam
medis rumah sakit yang belum tersedia. Isi minimum yang direncanakan ada pada arsitektur
bagian 9: lima keperluan akses beserta penanda mana yang menuntut penjelasan tambahan.

Sampai master ini terisi, seluruh pembukaan rekam medis pasien di luar rawatan akan ditolak,
karena tidak ada keperluan yang dapat dipilih. Itu perilaku yang menutup rapat, tetapi berarti
**master ini wajib terisi sebelum modul dipakai**.

### Hasil gabungan `BE-04`, `BE-05`, dan `BE-06`

Ketiganya dirilis bersamaan karena mengunci tanpa menyediakan addendum akan melumpuhkan
koreksi klinis.

| Berkas | Status |
|---|---|
| `DTOs/ClinicalDocumentIntegrityDtos.cs` | Baru — permintaan tanda tangan, balasan keutuhan, addendum |
| `DTOs/ClinicalNoteAuthorDelegationDtos.cs` | Baru — penetapan dan jawaban kewenangan |
| `Services/ClinicalNoteAddendumService.cs` | Baru — kewenangan bertingkat dan pembuatan addendum |
| `Services/ClinicalNoteAuthorDelegationService.cs` | Baru — penetapan beserta 6 aturan validasi |
| `Controllers/ClinicalDocumentIntegrityController.cs` | Baru — 4 endpoint |
| `Controllers/ClinicalNoteAddendumController.cs` | Baru — 4 endpoint |
| `Controllers/ClinicalNoteAuthorDelegationController.cs` | Baru — 3 endpoint |
| `Program.cs` | Diperbarui — 2 `AddScoped` tambahan |
| `tests/.../AuthorDelegationAndAddendumTests.cs` | Baru — 17 uji |

**Perubahan kontrak.** Grup addendum bertambah satu endpoint, dari 3 menjadi 4. Rancangan
semula menyatukan addendum biasa dan addendum pengganti dalam satu endpoint, dengan kewenangan
diperiksa di dalamnya. Itu tidak dapat diterapkan: atribut `[AccessAction]` hanya boleh satu
per endpoint, sehingga hak akses `CreateAsSubstitute` tidak akan pernah terdaftar dan tidak
dapat diberikan kepada siapa pun. Rinciannya pada `contracts/api-contract.md` bagian 4.

**Keputusan desain.** Kewenangan pengganti diterima service sebagai masukan, bukan dibaca
sendiri. `AccessPermissionService` memerlukan `UserManager`, yang tidak dapat dihidupkan pada
uji tanpa menyalakan seluruh Identity. Dengan memisahkannya, aturan bisnisnya tetap dapat
diuji dan sumber kewenangannya tetap satu tempat yang jelas di controller.

---

## 3. Milestone B0 — Fondasi

### `BE-00` — Membuat project test backend

| Field | Isi |
|---|---|
| **Task ID** | `BE-00` |
| **Status** | **`SELESAI`** — dikerjakan 24 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Tim punya cara membuktikan bahwa perubahan pada catatan klinis tidak merusak alur IGD dan antrean dokter. Sebelum ini, satu-satunya cara adalah mencoba manual dan berharap tidak ada yang terlewat |
| **Trace** | `RM-CAP-032`; open question nomor 11 pada decision log |
| **Reuse** | Tidak ada. Backend belum memiliki project test apa pun |
| **Scope** | Project test baru pada solution; penyiapan basis data uji; contoh uji yang menyentuh basis data |
| **Dependency** | **Tidak ada.** Tidak bergantung pada satu pun keputusan modul rekam medis |
| **Acceptance criteria** | 1) Perintah uji dapat dijalankan dan melaporkan hasil. 2) Sekurang-kurangnya satu uji integrasi menyentuh basis data dan lulus. 3) Uji dapat dijalankan berulang tanpa saling mengganggu |
| **Verification** | `dotnet test` dijalankan tiga kali berturut-turut: `Failed: 0, Passed: 4` pada ketiganya. `dotnet build QuilvianSystemBackend.sln` lolos dengan 0 error |
| **Risk/blocker** | **Tertutup.** Kerangka uji ditetapkan xUnit; basis data uji memakai SQLite di dalam memori. Batasan yang diterima tercatat pada `tests/README.md` bagian "Batasan yang diketahui" |
| **DoD** | **Terpenuhi.** Project test ada di solution, cara menjalankannya terdokumentasi di `tests/README.md`, dan empat uji lulus |

### Hasil `BE-00`

| Berkas | Status | Isi |
|---|---|---|
| `tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | Baru | Project test xUnit, menunjuk project utama |
| `tests/QuilvianSystemBackend.Tests/Infrastructure/TestDatabase.cs` | Baru | Penyedia basis data uji SQLite di dalam memori |
| `tests/QuilvianSystemBackend.Tests/Infrastructure/TestDatabaseTests.cs` | Baru | Empat uji yang membuktikan fondasi bekerja |
| `tests/README.md` | Baru | Cara menjalankan, cara menulis uji baru, dan batasan yang diketahui |
| `QuilvianSystemBackend.csproj` | Diperbarui | Pengecualian `tests\**` agar project test tidak ikut terkompilasi ke aplikasi |
| `QuilvianSystemBackend.sln` | Diperbarui | Project test ditambahkan ke solution |

Empat uji yang dihasilkan:

| Uji | Yang dibuktikan |
|---|---|
| `BasisDataUji_DapatDibentukLengkapDenganTabelnya` | Seluruh pemetaan EF Core aplikasi dapat dibentuk menjadi tabel sungguhan |
| `DataYangDisimpan_DapatDibacaKembaliLewatKonteksBaru` | Uji integrasi benar-benar menyentuh basis data, bukan hanya memori konteks |
| `IndexUnik_MenolakKodeYangKembar` | Aturan keunikan ditegakkan basis data — **prasyarat agar `BE-01` dapat dibuktikan** |
| `SetiapBasisDataUji_BerdiriSendiriDanTidakSalingMelihat` | Uji tidak saling mengganggu, sehingga hasilnya tidak bergantung urutan jalannya |

Uji ketiga yang paling menentukan. `BE-01` menuntut pembuktian bahwa index unik
`(DocumentKind, DocumentId)` menolak baris kembar. Bila basis data uji tidak menegakkan
keunikan, uji itu akan lulus padahal seharusnya gagal — dan itu lebih berbahaya daripada tidak
punya uji sama sekali.

### Yang sengaja tidak dikerjakan pada `BE-00`

| Yang tidak dikerjakan | Alasan |
|---|---|
| Menambahkan langkah `dotnet test` ke CI | Mengubah perilaku CI, perlu persetujuan pemilik CI. Cara menambahkannya sudah dituliskan di `tests/README.md` |
| Uji lewat HTTP | Belum ada endpoint modul ini. Ditambahkan saat endpoint pertama dibuat |
| Uji migration | `EnsureCreated` membentuk tabel langsung dari model, tidak menjalankan migration. Diperlukan saat `BE-08` yang memindahkan data |
| Uji untuk controller yang sudah ada | Di luar acceptance criteria `BE-00`. Uji regresi alur antrean dokter menjadi bagian `BE-03` |

Catatan mengapa task ini didahulukan meski bukan bagian modul rekam medis. Tiga perbaikan pada
`BE-03` menyentuh `PatientIntegratedProgressNoteController`, berkas sepanjang 1.407 baris yang
dipakai alur antrean dokter dan IGD. Mengubahnya tanpa jaring pengaman otomatis adalah risiko
yang dapat dihindari dengan pekerjaan yang tidak menunggu approval siapa pun.

### `BE-01` — Model, enum, dan configuration keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-01` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Sistem punya tempat menyimpan keterangan keutuhan dokumen, terpisah dari isi klinisnya |
| **Trace** | `RM-DEC-013`; ERD `keutuhan-dokumen.md`; kamus data bagian 1 sampai 3 dan 6 |
| **Reuse** | `IdentityModel`; pola configuration `Repositories/Configurations/HealthServices/`; `ApplyConfigurationsFromAssembly` pada `ApplicationDbContext.cs:612` |
| **Scope** | `Areas/HealthServices/MedicalRecordManagement/Models/` tiga model; `Enums/` enam enum; `Repositories/Configurations/HealthServices/MedicalRecordManagement/` tiga configuration; migration `AddMedicalRecordIntegrityTables` |
| **Dependency** | `BE-00` disarankan lebih dulu |
| **Acceptance criteria** | 1) Migration berjalan dan mundur tanpa galat. 2) Index unik `(DocumentKind, DocumentId)` menolak baris kembar. 3) Index unik `(IntegrityId, Sequence)` menolak addendum berurutan kembar. 4) Seluruh relasi memakai `DeleteBehavior.Restrict` |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 11`. Tujuh uji baru pada `ClinicalDocumentIntegritySchemaTests`. `dotnet build` lolos 0 error |
| **Risk/blocker** | **Tertutup untuk lingkup task ini.** Risiko rujukan polimorfik diterima sadar dan ditutup di service pada `BE-02` |
| **DoD** | **Terpenuhi.** Migration `20260826034557_AddMedicalRecordIntegrityTables` terbentuk dengan 3 tabel, 2 index unik, dan `Down()` yang menghapus ketiganya |

### Hasil `BE-01`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Enums/` — 6 enum | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalDocumentIntegrity.cs` | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAddendum.cs` | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAuthorDelegation.cs` | Baru |
| `Repositories/Configurations/HealthServices/MedicalRecordManagement/` — 3 configuration | Baru |
| `Repositories/ApplicationDbContext.cs` | Diperbarui — 3 `DbSet` dan satu `using` |
| `Migrations/20260826034557_AddMedicalRecordIntegrityTables.cs` | Baru |
| `tests/.../Infrastructure/RekamMedisTestData.cs` | Baru — penyiapan pengguna, unit, pasien, kunjungan |
| `tests/.../MedicalRecordManagement/ClinicalDocumentIntegritySchemaTests.cs` | Baru — 7 uji |

Aturan yang terbukti ditegakkan basis data:

| Aturan | Uji |
|---|---|
| Satu dokumen tepat satu baris keutuhan | `DokumenYangSama_TidakDapatPunyaDuaBarisKeutuhan` |
| Keunikan berlaku pada pasangan jenis + Id, bukan Id saja | `JenisDokumenBerbeda_BolehMemakaiIdYangSama` |
| Urutan addendum tidak dapat kembar pada satu dokumen | `Addendum_TidakDapatPunyaUrutanKembarPadaDokumenYangSama` |
| Urutan boleh sama antar dokumen berbeda | `Addendum_BerurutanDiterimaDanUrutanBolehSamaAntarDokumen` |
| Dokumen yang punya addendum tidak dapat dihapus | `KeutuhanYangMasihPunyaAddendum_TidakDapatDihapus` |
| Enum tersimpan utuh sebagai angka | `StatusKeutuhanDanJenisDokumen_TersimpanUtuh` |

**Catatan temuan.** Penolakan penghapusan terjadi di lapisan EF Core saat baris ditandai hapus,
bukan di basis data saat disimpan. Hasilnya sama — addendum tidak pernah ikut terhapus diam-diam
— tetapi jenis galatnya `InvalidOperationException`, bukan `DbUpdateException`. Ini dicatat pada
komentar uji agar implementer berikutnya tidak salah menduga.

**Tidak dijalankan.** Migration **belum diterapkan** ke basis data mana pun. Berkasnya sudah
terbentuk dan terbukti maju-mundur secara bentuk, tetapi penerapannya ke basis data berjalan
memerlukan otorisasi terpisah.

### `BE-02` — Service keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-02` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Ada satu tempat yang memutuskan apakah sebuah dokumen masih boleh diubah. Tidak tersebar di banyak controller |
| **Trace** | `RM-DEC-003`; arsitektur bagian 5.6; state transition matrix bagian 2 dan 3 |
| **Reuse** | Pola service tanpa interface, didaftarkan `AddScoped`, mencontoh `DoctorConsultationLifecycleService` |
| **Scope** | `Services/ClinicalDocumentIntegrityService.cs`; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-01` |
| **Acceptance criteria** | 1) `RegisterAsync` menolak pendaftaran kedua untuk dokumen yang sama. 2) `SignAsync` menolak bila pemanggil bukan `AuthorUserId`. 3) `SignAsync` menolak bila status bukan `Draft`. 4) `EnsureMutableAsync` menolak dokumen `Signed`, `LockedUnsigned`, dan `Cancelled`. 5) `AuthorUserId` tidak dapat diubah lewat jalur mana pun |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 28`. Tujuh belas uji baru pada `ClinicalDocumentIntegrityServiceTests`, mencakup `AT-RM-02`, `AT-RM-10`, `AT-RM-11` |
| **Risk/blocker** | **Ditutup dengan cakupan sempit.** Aturan hanya ditegakkan untuk `ProgressNote`, dinyatakan lewat `ClinicalDocumentIntegrityService.DitegakkanUntuk` yang dapat dibaca dan diuji |
| **DoD** | **Terpenuhi.** Service terdaftar `AddScoped` pada `Program.cs`; seluruh acceptance criteria terbukti uji |

### Hasil `BE-02`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Baru |
| `Program.cs` | Diperbarui — satu `AddScoped` dan satu `using` |
| `tests/.../ClinicalDocumentIntegrityServiceTests.cs` | Baru — 17 uji |

Empat metode yang tersedia:

| Metode | Kegunaan | Menyimpan sendiri? |
|---|---|:---:|
| `RegisterAsync` | Mendaftarkan dokumen baru berstatus draf. Aman dipanggil berulang | **Tidak** — pemanggil wajib satu transaksi dengan pembuatan dokumen |
| `EnsureMutableAsync` | Memeriksa apakah dokumen masih boleh diubah | Tidak menulis |
| `SignAsync` | Menandatangani sekaligus mengunci | **Ya** — hanya menyentuh satu baris keutuhan |
| `LockOpenDocumentsForEncounterAsync` | Mengunci seluruh dokumen draf saat kunjungan ditutup, bertahap per potongan | **Tidak** — pemanggil wajib satu transaksi dengan penutupan kunjungan |

Perbedaan "menyimpan sendiri" itu mengikat. Keliru di sini berakibat data setengah tersimpan —
misalnya dokumen terbuat tetapi baris keutuhannya tidak, sehingga dokumen itu luput dari
seluruh aturan penguncian.

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Penulis dapat menandatangani, tanpa kata sandi maupun sidik jari | `Penulis_DapatMenandatanganiCatatannyaSendiri` |
| Bukan penulis ditolak `403`, bukan `400` | `BukanPenulis_TidakDapatMenandatangani` |
| Dokumen terkunci tidak dapat ditandatangani ulang | `DokumenYangSudahTerkunci_TidakDapatDitandatanganiUlang` |
| Dokumen terkunci menolak perubahan dan mengarahkan ke addendum | `DokumenYangDitandatangani_MenolakPerubahanDanMengarahkanKeAddendum` |
| Jenis dokumen yang belum ditegakkan dibiarkan lewat | `JenisDokumenYangBelumDitegakkan_DibiarkanLewat` |
| Penutupan kunjungan mengunci seluruh dokumen draf | `PenutupanKunjungan_MenguncSeluruhDokumenDrafPadaKunjunganItu` |
| Dokumen yang sudah ditandatangani tidak ikut diubah | `PenutupanKunjungan_TidakMengubahDokumenYangSudahDitandatangani` |
| Aman dipanggil berulang | `PenguncianKunjungan_AmanDipanggilBerulang` |
| Kunjungan lain tidak ikut terkunci | `PenutupanKunjungan_TidakMenyentuhDokumenKunjunganLain` |
| Penguncian bertahap menyelesaikan seluruhnya | `PenguncianBertahap_MenguncSeluruhnyaWalauPotongannyaKecil` |
| Penulis tidak berubah setelah ditandatangani | `PenulisDokumen_TidakBerubahSetelahDitandatangani` |

**Catatan desain.** Penolakan dikembalikan sebagai `IntegrityGuardResult` berisi kode status
dan pesan, bukan dilempar sebagai exception. Alasannya: setiap penolakan sudah punya pesan dan
kode yang ditetapkan validation matrix, sehingga controller cukup meneruskannya tanpa
menerjemahkan ulang. Pola ini mengikuti `ValidateMergedPatientReferenceAsync` yang sudah ada
pada `PatientController`.

**Catatan temuan.** Kolom `UserCode` pada tabel pengguna memiliki keunikan yang ditegakkan
basis data. Penyiapan data uji wajib mengisinya dengan nilai berbeda tiap pengguna; bila
dilewatkan, uji kedua yang membuat pengguna akan gagal.

---

## 4. Milestone B1 — Slice minimum: CPPT terkunci dan dapat dikoreksi

Ini vertical slice pertama yang menghasilkan sesuatu yang dapat diverifikasi pemilik proses:
**catatan CPPT yang sudah ditandatangani tidak dapat diubah diam-diam, dan koreksinya
meninggalkan jejak.**

### `BE-03` — Menutup tiga celah pada CPPT

| Field | Isi |
|---|---|
| **Task ID** | `BE-03` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Catatan CPPT tidak lagi dapat diubah setelah ditandatangani, penulisnya tidak dapat dipindahkan ke orang lain, dan penanda read-only tidak dapat dilepas dari luar |
| **Trace** | `RM-DEC-019`; `RM-CAP-011`, `RM-CAP-012`, `RM-CAP-013`; api-contract bagian 8 |
| **Reuse** | `PatientIntegratedProgressNoteController` yang sudah ada; hanya perilakunya berubah |
| **Scope** | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`. Tiga perubahan: memanggil `EnsureMutableAsync` sebelum mengubah; **menghapus** penetapan `entity.ProviderUserId` dari permintaan pada baris 533; **menghapus** penetapan `entity.IsReadOnlyGenerated` dari permintaan pada baris 550. Tambahan: memanggil `RegisterAsync` saat CPPT dibuat |
| **Dependency** | `BE-00`, `BE-02` |
| **Acceptance criteria** | 1) Mengubah CPPT terkunci ditolak `400` dengan pesan mengarahkan ke addendum. 2) Mengirim `ProviderUserId` orang lain tidak mengubah apa pun. 3) Mengirim `IsReadOnlyGenerated` tidak mengubah apa pun. 4) Membuat CPPT menghasilkan baris keutuhan `Draft`. 5) Bila pendaftaran keutuhan gagal, pembuatan CPPT ikut dibatalkan |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 36`. Delapan uji baru pada `ProgressNoteIntegrityRepairTests`, mencakup `AT-RM-01`, `AT-RM-19`, `AT-RM-20`, `AT-RM-24`. `dotnet build` lolos 0 error |
| **Risk/blocker** | **Risiko tertinggi di seluruh roadmap, dan terbukti tertutup.** `BE-00` menjadi jaring pengamannya: 36 uji lulus setelah perubahan, termasuk uji yang membuktikan CPPT draf tetap dapat diubah seperti biasa |
| **DoD** | **Terpenuhi.** Ketiga celah tertutup dan terbukti uji. Pencatatan perubahan perilaku pada Swagger dan catatan rilis menjadi bagian `BE-18` |

### Hasil `BE-03`

Seluruh perubahan berada pada **perilaku**, bukan skema. Tidak ada satu kolom pun berubah.

| Berkas | Perubahan |
|---|---|
| `PatientIntegratedProgressNoteController.cs` | Diperbarui — 4 perubahan, lihat di bawah |
| `tests/.../ProgressNoteIntegrityRepairTests.cs` | Baru — 8 uji |
| `tests/.../Infrastructure/ControllerTestHarness.cs` | Baru — alat bantu memanggil controller dari uji |

Empat perubahan pada controller:

| No | Perubahan | Menutup |
|---:|---|---|
| 1 | `UpdateProgressNote` memanggil `EnsureMutableAsync` sebelum mengubah | `RM-CAP-011` |
| 2 | Penetapan `entity.ProviderUserId` dari isi permintaan **dihapus** | `RM-CAP-012` |
| 3 | Penetapan `entity.IsReadOnlyGenerated` dari isi permintaan **dihapus** | `RM-CAP-013` |
| 4 | `CreateProgressNote` dan `CreateFromConsultation` memanggil `RegisterIntegrityAsync` | `RM-DEC-013` |

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| CPPT yang ditandatangani menolak perubahan, isinya tidak tersentuh | `MengubahCpptYangSudahDitandatangani_DitolakDanIsinyaTidakBerubah` |
| CPPT yang terkunci karena kunjungan ditutup juga menolak | `MengubahCpptYangTerkunciKarenaKunjunganDitutup_Ditolak` |
| **CPPT draf tetap dapat diubah seperti biasa** | `MengubahCpptYangMasihDraf_TetapBerhasil` |
| Mengirim penulis orang lain tidak memindahkan penulis | `MengirimPenulisOrangLain_TidakMengubahPenulisCatatan` |
| Mengirim penanda hanya-baca tidak mengubah penanda | `MengirimPenandaHanyaBaca_TidakMengubahPenandaPadaCatatan` |
| Membuat CPPT menghasilkan baris keutuhan draf | `MembuatCppt_MenghasilkanBarisKeutuhanBerstatusDraf` |
| CPPT baru langsung tunduk aturan setelah ditandatangani | `CpptBaru_LangsungTundukAturanKeutuhanSetelahDitandatangani` |
| CPPT tanpa kunjungan tidak membuat baris keutuhan menggantung | `CpptTanpaKunjungan_TetapDibuatTetapiTidakTerdaftarKeutuhan` |

Uji ketiga yang paling menenangkan: **CPPT draf tetap dapat diubah seperti biasa.** Perbaikan
ini menutup celah tanpa memblokir alur yang wajar.

### Keterbatasan yang dinyatakan terbuka

| Keterbatasan | Penjelasan |
|---|---|
| CPPT tanpa kunjungan tidak terdaftar keutuhan | Baris keutuhan mensyaratkan kunjungan sebagai pengelompokannya. CPPT semacam itu tetap dapat dibuat, tetapi tidak tunduk aturan penguncian. Dilewati tanpa menggagalkan pembuatan, karena menolak akan memblokir alur yang berjalan |
| CPPT lama belum terdaftar | Baru terdaftar setelah `BE-08` dijalankan. Sampai saat itu, `EnsureMutableAsync` memperlakukannya sebagai masih boleh diubah |
| Hak akses dan bentuk balasan HTTP belum diuji | Uji memanggil controller langsung, melewati lapisan pemeriksaan hak akses. Perlu uji lewat HTTP bila kelak diperlukan |

### Perubahan perilaku yang wajib diberitahukan

Dicatat di sini dan akan dituangkan ke Swagger serta catatan rilis pada `BE-18`.

| Endpoint | Perubahan | Dampak bagi klien |
|---|---|---|
| `PUT .../patient-integrated-progress-notes/{id}` | Menolak bila dokumen terkunci | Menerima `400`. Sebelumnya berhasil |
| Endpoint yang sama | `ProviderUserId` diabaikan | **Tidak** menerima galat, tetapi nilainya tidak berpengaruh |
| Endpoint yang sama | `IsReadOnlyGenerated` diabaikan | Sama seperti di atas |

### `BE-04` — Menandatangani dan mengunci dokumen

| Field | Isi |
|---|---|
| **Task ID** | `BE-04` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Dokter dan perawat dapat menyatakan catatannya final, dan setelah itu isinya terjamin tidak berubah |
| **Trace** | `RM-DEC-003`, `RM-DEC-021`; api-contract bagian 3 |
| **Reuse** | `BE-02` |
| **Scope** | `Controllers/ClinicalDocumentIntegrityController.cs`; `DTOs/ClinicalDocumentIntegrityDtos.cs`. Empat endpoint: status per dokumen, menandatangani, catatan saya yang belum ditandatangani, keutuhan per kunjungan |
| **Dependency** | `BE-02` |
| **Acceptance criteria** | 1) Menandatangani mengisi `SignedAt`, `SignedByUserId`, `SignatureDeviceInfo`, dan `SignatureIpAddress`. 2) Perangkat dan IP diambil server dari permintaan, **tidak** dari kiriman klien. 3) Tidak ada permintaan kata sandi maupun sidik jari. 4) `/my-unsigned` hanya memuat dokumen milik pengguna |
| **Verification** | `AT-RM-02`, `AT-RM-18` |
| **Risk/blocker** | Risiko: bila layar `/my-unsigned` tidak ada, catatan yang lupa ditandatangani tidak dapat ditemukan. Karena itu endpoint ini bagian dari task yang sama, bukan tambahan |
| **DoD** | Empat endpoint berjalan; acceptance criteria terbukti uji; terdaftar di Swagger |

### `BE-05` — Penetapan penulis berhalangan

| Field | Isi |
|---|---|
| **Task ID** | `BE-05` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Kepala unit dapat membuka jalur koreksi ketika penulis catatan berhalangan, dan penetapan itu tercatat beserta alasan dan batas waktunya |
| **Trace** | `RM-DEC-020`; api-contract bagian 5; validation matrix bagian 3 |
| **Reuse** | `BE-01` |
| **Scope** | `Services/ClinicalNoteAddendumService.cs` bagian penentu kewenangan; `Controllers/ClinicalNoteAuthorDelegationController.cs`; `DTOs/` |
| **Dependency** | `BE-01` |
| **Acceptance criteria** | 1) Penetapan tanpa `ValidUntil` ditolak. 2) Penetapan dengan batas waktu yang sudah lewat ditolak. 3) Menetapkan diri sendiri ditolak. 4) Penetapan ganda untuk penulis yang sama ditolak. 5) Penetapan untuk akun yang sudah nonaktif ditolak disertai penjelasan bahwa jalurnya sudah terbuka otomatis |
| **Verification** | `AT-RM-26`, `AT-RM-27` |
| **Risk/blocker** | Risiko: penetapan manual dapat disalahgunakan. Ditutup dengan kewajiban batas waktu dan pencatatan alasan |
| **DoD** | Seluruh aturan validasi terbukti uji; penetapan tanpa batas waktu tidak dapat tersimpan lewat jalur mana pun |

### `BE-06` — Addendum

| Field | Isi |
|---|---|
| **Task ID** | `BE-06` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Kesalahan pada catatan yang sudah terkunci dapat dibetulkan tanpa menghapus isi aslinya. Pembaca melihat keduanya dan tahu urutan kejadiannya |
| **Trace** | `RM-DEC-004`, `RM-DEC-020`; api-contract bagian 4; state transition matrix bagian 4 |
| **Reuse** | `BE-02`, `BE-05` |
| **Scope** | `Services/ClinicalNoteAddendumService.cs`; `Controllers/ClinicalNoteAddendumController.cs`; `DTOs/` |
| **Dependency** | `BE-02`, `BE-05`. **Wajib dirilis bersamaan `BE-04`** — lihat bagian 1 |
| **Acceptance criteria** | 1) Addendum pada dokumen `Draft` ditolak. 2) Addendum oleh bukan penulis tanpa penetapan ditolak `403`. 3) Kepala unit dapat menambah addendum bila akun penulis nonaktif, dan `AuthorUserId` berisi kepala unit. 4) Isi dokumen induk tidak berubah. 5) Status dokumen tetap sama setelah addendum. 6) `AuthorUserId`, `IsSubstituteAuthor`, dan `DelegationId` ditentukan server, **tidak** diterima dari klien. 7) Tidak ada endpoint mengubah atau menghapus addendum |
| **Verification** | `AT-RM-04`, `AT-RM-05`, `AT-RM-14`, `AT-RM-17`, `AT-RM-28` |
| **Risk/blocker** | Risiko: addendum tidak dapat dihapus, sehingga kiriman ganda menempel selamanya. Pencegahannya di sisi frontend, `FE-04` |
| **DoD** | Seluruh acceptance criteria terbukti uji; endpoint pemeriksa kewenangan tersedia untuk dipakai frontend |

### `BE-07` — Penguncian saat kunjungan ditutup

| Field | Isi |
|---|---|
| **Task ID** | `BE-07` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Tidak ada catatan yang tertinggal terbuka setelah kunjungan pasien selesai |
| **Trace** | `RM-DEC-003`; integration contract bagian 2.2 |
| **Reuse** | `PatientEncounterController` yang sudah ada |
| **Scope** | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`, endpoint `PATCH /{id}/status`; penambahan `LockOpenDocumentsForEncounterAsync` pada `BE-02` |
| **Dependency** | `BE-02`, `BE-03` |
| **Acceptance criteria** | 1) Perpindahan menuju `Completed` mengunci seluruh dokumen `Draft` pada kunjungan itu. 2) Penguncian dan perubahan status berada dalam satu transaksi. 3) Bila penguncian gagal, penutupan kunjungan ikut dibatalkan. 4) Perpindahan menuju `Cancelled` **tidak** mengunci apa pun. 5) Aman dipanggil berulang |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 57`. Empat uji baru pada `EncounterClosureLockTests`, mencakup `AT-RM-03`. `dotnet build` lolos 0 error |
| **Risk/blocker** | Risiko transaksi panjang ditutup dengan penguncian per potongan. Catatan: endpoint status kunjungan tidak memvalidasi perpindahan (`RM-CAP-019`), dan itu **tidak** diperbaiki task ini |
| **DoD** | **Terpenuhi.** Pemicu terpasang pada seluruh jalur penyelesaian kunjungan; acceptance criteria terbukti uji |

### Hasil `BE-07` — dan satu temuan yang mengubah scope

**Desain menyebut satu jalur. Kenyataannya ada tiga.**

Penelusuran menemukan kunjungan dapat berpindah ke `Completed` lewat tiga tempat berbeda,
bukan hanya lewat `PatientEncounterController` seperti tertulis pada arsitektur bagian 5.11:

| Jalur | Berkas | Kapan dipakai |
|---|---|---|
| Perubahan status umum | `PatientEncounterController.UpdateEncounterStatus` | Penyesuaian manual oleh petugas |
| **Konsultasi dokter selesai** | `DoctorQueueController` baris ~461 | **Jalur paling sering dipakai** pada rawat jalan |
| Screening perawat selesai | `NurseStationQueueController` baris ~330 | Pasien yang tidak memerlukan dokter |

Bila pemicu hanya dipasang di jalur pertama seperti tertulis di desain, **kunjungan yang
diselesaikan lewat antrean dokter tidak akan pernah mengunci catatannya** — dan itu justru
jalur yang paling sering dipakai. Aturan `RM-DEC-003` lapis kedua akan tampak berjalan padahal
hampir tidak pernah aktif.

Pemicu karena itu dipasang pada **ketiganya**.

| Berkas | Perubahan |
|---|---|
| `PatientEncounterController.cs` | Diperbarui — pemicu pada perpindahan menuju `Completed` |
| `DoctorQueueController.cs` | Diperbarui — pemicu saat konsultasi dokter selesai |
| `NurseStationQueueController.cs` | Diperbarui — pemicu saat screening selesai tanpa dokter |
| `tests/.../EncounterClosureLockTests.cs` | Baru — 4 uji |

Pada ketiganya, penguncian **tidak menyimpan sendiri** — ia ikut `SaveChanges` yang sudah ada,
sehingga bila penguncian gagal, penyelesaian kunjungan ikut dibatalkan.

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Kunjungan selesai mengunci seluruh catatan draf | `KunjunganSelesai_MenguncSeluruhCatatanDrafDanTidakMenyentuhYangDitandatangani` |
| Catatan yang sudah ditandatangani tidak tersentuh | uji yang sama |
| Setelah selesai, seluruh catatan menolak perubahan | `SetelahKunjunganSelesai_SeluruhCatatannyaMenolakPerubahan` |
| Pembatalan kunjungan **tidak** mengunci | `KunjunganDibatalkan_TidakMenguncCatatan` |
| Catatan yang dibuat setelah kunjungan selesai tetap draf | `CatatanYangDibuatSetelahKunjunganSelesai_TetapBerstatusDraf` |

### Keterbatasan yang dinyatakan terbuka

| Keterbatasan | Penjelasan |
|---|---|
| Catatan susulan tetap berstatus draf | Penguncian hanya berlaku pada saat kunjungan berpindah ke selesai. Catatan yang dibuat setelahnya perlu ditandatangani penulisnya sendiri, atau akan menggantung terbuka |
| Perpindahan status tidak divalidasi | `RM-CAP-019` tetap terbuka. Penguncian dipicu tujuan perpindahan, bukan urutannya, sehingga tetap bekerja walaupun status melompat |

### `BE-08` — Pengisian data lama

| Field | Isi |
|---|---|
| **Task ID** | `BE-08` |
| **Status** | **`SELESAI`** — 2 November 2026. Alat, uji, dan panduan selesai 26 Agustus 2026; dijalankan di devYoga 2 November 2026 (10 CPPT → `LockedUnsigned`, nol tersisa). Penjalanan di produksi mengikuti panduan yang sama saat pemasangan |
| **Outcome** | Catatan CPPT yang sudah tersimpan sebelum modul ini ada ikut memiliki status keutuhan, sehingga tidak ada bagian rekam medis yang luput dari aturan |
| **Trace** | `RM-DEC-014`; arsitektur bagian 8.2 migration ketiga |
| **Reuse** | — |
| **Scope** | Migration `BackfillProgressNoteIntegrity` |
| **Dependency** | `BE-01`. **Wajib selesai sebelum `BE-03` diaktifkan di produksi** — lihat integration contract bagian 3 |
| **Acceptance criteria** | 1) CPPT pada kunjungan `Completed` atau `Cancelled` bernilai `LockedUnsigned` dengan `LockTrigger = BackfillEncounterClosed`. 2) CPPT pada kunjungan berjalan bernilai `Draft`. 3) CPPT yang sudah dibatalkan bernilai `Cancelled`. 4) CPPT tanpa `ProviderUserId` tetap dibuat barisnya dengan `IsAuthorKnown = false`, **tidak dilewati diam-diam**. 5) Dijalankan bertahap per potongan. 6) Dapat dimundurkan dengan menghapus baris yang dibuatnya |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 68`. Sebelas uji baru pada `MedicalRecordBackfillTests`, mencakup `AT-RM-21` dan `AT-RM-33` |
| **Risk/blocker** | **Satu-satunya task yang menyentuh data klinis nyata.** Risikonya diturunkan lewat tiga hal: penelaahan yang hanya membaca, mode percobaan yang tidak menyimpan, dan penjalanan bertahap yang dapat dilanjutkan. Jumlah baris pada data nyata **masih belum diketahui** |
| **DoD** | **Terpenuhi** untuk devYoga. Alat, uji, panduan, penelaahan, percobaan, penjalanan, dan penelaahan ulang selesai. Pemberitahuan unit rekam medis tidak diperlukan untuk data pengembangan (keputusan pemilik modul, 2 November 2026); keduanya kembali wajib saat pemasangan produksi |

### Hasil `BE-08` — dan satu perubahan cara

**Rancangan semula memakai migration. Diganti menjadi service yang dijalankan terkendali.**

| Alasan | Penjelasan |
|---|---|
| Jumlah barisnya tidak diketahui | Migration langsung berjalan tanpa dapat ditelaah lebih dulu |
| Waktunya tidak dapat dipilih | Migration berjalan otomatis saat aplikasi naik. Pengisian ini sebaiknya dijalankan ketika unit rekam medis sudah siap |
| Sulit dilanjutkan bila terhenti | Service dapat dijalankan bertahap dan melanjutkan dari sisa |

Aturan penentuan statusnya **tidak berubah** — persis `RM-DEC-014`.

| Berkas | Status |
|---|---|
| `Services/MedicalRecordBackfillService.cs` | Baru — penelaahan dan penjalanan bertahap |
| `DTOs/MedicalRecordBackfillDtos.cs` | Baru |
| `Controllers/MedicalRecordBackfillController.cs` | Baru — 2 endpoint |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordBackfillTests.cs` | Baru — 11 uji |
| `roadmap/BE-08-panduan-pengisian-data-lama.md` | Baru — panduan menjalankan beserta bahan pemberitahuan |

Tiga sifat yang menurunkan risikonya:

| Sifat | Penjelasan |
|---|---|
| **Penelaahan hanya membaca** | Menjawab berapa banyak dan akan menjadi apa, tanpa mengubah apa pun. Aman dijalankan kapan saja |
| **Percobaan tidak menyimpan** | Menghitung seluruhnya lalu melaporkan hasilnya, tanpa menyentuh data. Terbukti melaporkan angka yang sama dengan penjalanan sungguhan |
| **Bertahap dan dapat dilanjutkan** | Catatan yang sudah terdaftar tidak diproses ulang, sehingga aman bila terhenti di tengah |

**Cara mundur.** Pengisian ini **hanya menambah baris** pada tabel keutuhan dan tidak mengubah
satu pun catatan klinis. Pembatalannya berupa penghapusan baris yang dibuatnya, tanpa menyentuh
data pasien. Rinciannya pada panduan bagian 4.

### Hasil penelaahan — dijalankan 26 Agustus 2026

| Yang ditelaah | Hasil | Dampaknya |
|---|---:|---|
| Total CPPT | **10** | Selesai satu potongan, penjalanan bertahap tidak diperlukan |
| Penulis tidak tercatat | **0** | Kekhawatiran penulis tidak diketahui tidak berlaku |
| Kunjungan masih berjalan | **0** | Nol risiko mengganggu pasien yang sedang dirawat |
| Tanpa kunjungan | **0** | Seluruh catatan dapat didaftarkan |
| Akan terkunci tanpa tanda tangan | **10** | Angka yang muncul di laporan kelengkapan |

Profil risikonya turun jauh dari perkiraan semula. Rinciannya pada
`BE-08-panduan-pengisian-data-lama.md` bagian 0.

**Peringatan yang tetap berlaku.** Sepuluh CPPT adalah jumlah lingkungan pengembangan, bukan
jumlah pelayanan nyata. Koneksi `Development` project ini menunjuk server bersama, bukan
produksi. **Sebelum modul dipasang di produksi, penelaahan wajib dijalankan ulang di sana**,
dan seluruh pertimbangan pada panduan kembali berlaku penuh.

### Yang tersisa sebelum `BE-08` dapat dinyatakan selesai

| No | Yang tersisa | Siapa | Keadaan |
|---:|---|---|---|
| 1 | ~~Penelaahan pada basis data~~ | — | **Selesai** 26 Agustus 2026 |
| 2 | ~~Menjalankan percobaan, memeriksa pembagian antara terkunci dan dibatalkan~~ | Tim pengembang | **Selesai** 2 November 2026 — 10 terkunci, nol dibatalkan, nol draf, nol dilewati; koneksi dipaksa baca-saja |
| 3 | ~~Memberi tahu unit rekam medis~~ | Penanggung jawab modul | **Tidak diperlukan untuk data pengembangan** — keputusan pemilik modul 2 November 2026; wajib saat produksi |
| 4 | ~~Menjalankan sungguhan, satu potongan cukup~~ | Tim pengembang | **Selesai** 2 November 2026 di devYoga — 10 baris, pelaksana `superadmin`, nol tersisa |
| 5 | Mengulang penelaahan di produksi sebelum pemasangan di sana | Tim pengembang | **Syarat pemasangan produksi**, bukan sisa task. Tetap **wajib** |

---

## 5. Milestone B2 — Jejak dan kewenangan akses

### `BE-09` — Master keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-09` |
| **Status** | **`SELESAI` sebagian** — dikoreksi 27 Agustus 2026 terhadap source. **Dua bagian scope belum dikerjakan, bukan satu.** Rinciannya pada baris Risk/blocker |
| **Outcome** | Petugas punya daftar keperluan akses yang dapat dipilih, bukan kotak teks kosong yang jawabannya tidak dapat dibandingkan |
| **Trace** | Arsitektur bagian 9; api-contract bagian 7; arsitektur frontend bagian 11.5 |
| **Reuse** | Pola master data yang sudah ada, misalnya `MstBillingItemCategory` |
| **Scope** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs`; DTO; controller; configuration; migration `AddMedicalRecordAccessAuditTables` bagian master; data awal |
| **Dependency** | Tidak ada task pendahulu |
| **Acceptance criteria** | 1) Lima keperluan minimum terisi. 2) Baris `Lainnya` memiliki `IsFreeTextRequired` bernilai benar. 3) `PurposeCode` dijamin unik. 4) Endpoint `/options` mengembalikan hanya yang aktif |
| **Verification** | Uji integrasi endpoint master; pemeriksaan data awal setelah migration |
| **Risk/blocker** | **Dua hal terbuka.** (1) **Kode belum lengkap** — model, configuration, dan migration ada; **DTO, controller, dan keenam endpoint api-contract bagian 7 belum dibuat**. Tidak ada `MedicalRecordAccessPurposeController` di seluruh source. Akibatnya `FE-06` tertahan dan master hanya dapat diisi lewat seeder atau SQL langsung. (2) **Isi awal** harus berasal dari SOP rekam medis rumah sakit yang belum tersedia; owner product/domain |
| **DoD** | Controller dan enam endpoint berjalan; master terisi; daftar keperluan disetujui unit rekam medis. **Butir pertama belum terpenuhi**, dan ia tidak menunggu SOP — ia pekerjaan kode yang terlewat |

### `BE-10` — Tabel jejak akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-10` |
| **Status** | **`SELESAI`** 26 Agustus 2026 bersama `BE-11` — tabel jejak akses `MrcAccessLog` (masa simpan 25 tahun, terbagi per tahun, `RM-DEC-024`); lihat ringkasan bagian 2. *Semula `TERTAHAN APPROVAL`; diselaraskan 2 November 2026 karena bagian detail ini tertinggal dari ringkasan* |
| **Outcome** | Sistem punya tempat menyimpan catatan siapa membuka rekam medis siapa |
| **Trace** | `RM-DEC-015`, `RM-DEC-023`; ERD `jejak-akses.md`; kamus data bagian 4 |
| **Reuse** | `IdentityModel`, dengan pengecualian: penandaan hapus tidak dipakai |
| **Scope** | `Models/TrxMedicalRecordAccessLog.cs`; configuration; migration `AddMedicalRecordAccessAuditTables` bagian jejak, **termasuk rancangan pembagian tabel per periode** |
| **Dependency** | `BE-09` |
| **Acceptance criteria** | 1) Tabel terbentuk dengan empat index gabungan sesuai kamus data. 2) Pembagian tabel per periode terpasang sejak migration pertama. 3) Tidak ada endpoint yang dapat mengubah atau menghapus baris |
| **Verification** | Uji integrasi index; pemeriksaan rancangan pembagian tabel |
| **Risk/blocker** | **Blocker keras.** Memasang pembagian tabel setelah berisi puluhan juta baris menuntut waktu henti layanan. Menunda keputusan ini berarti memilih pekerjaan yang jauh lebih mahal di kemudian hari. Owner: security/privacy, **`OPEN`** |
| **DoD** | Migration teruji; tabel terbagi per tahun terpasang; penjadwalan pembuatan bagian tahun berikutnya sudah otomatis dan terpantau |

### `BE-11` — Service jejak dan kewenangan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-11` |
| **Status** | **`SELESAI`** 26 Agustus 2026 — `MedicalRecordAccessAuditService`, penilaian kewenangan pasien (`RM-DEC-005`, `016`, `017` disahkan `RM-DEC-027`); lihat ringkasan bagian 2. *Semula `TERTAHAN APPROVAL`; diselaraskan 2 November 2026* |
| **Outcome** | Setiap pembukaan rekam medis tercatat, dan pembukaan di luar pasien rawatan menuntut alasan lebih dulu |
| **Trace** | `RM-DEC-005`, `RM-DEC-015`, `RM-DEC-016`, `RM-DEC-017`; permission-audit-matrix |
| **Reuse** | `TrxPatientEncounter` untuk menilai kunjungan aktif |
| **Scope** | `Services/MedicalRecordAccessAuditService.cs`; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-09`, `BE-10` |
| **Acceptance criteria** | 1) Pasien dengan kunjungan aktif diperlakukan `RoutineCare` tanpa diminta alasan. 2) Pasien tanpa kunjungan aktif menuntut keperluan; bila kosong, isi **tidak dikembalikan sama sekali**. 3) Jejak ditulis dan transaksinya selesai **sebelum** isi dikembalikan. 4) Bila penulisan jejak gagal, permintaan dijawab `503` dan isi tidak dikembalikan. 5) Bila penilaian kunjungan gagal, akses diperlakukan sebagai beralasan, bukan sebagai rawatan. 6) `SuperAdmin` tunduk aturan yang sama. 7) `AccessPermissionService.cs` **tidak disentuh** |
| **Verification** | `AT-RM-06`, `AT-RM-07`, `AT-RM-12`, `AT-RM-13`, `AT-RM-25`, `AT-RM-30` |
| **Risk/blocker** | Risiko: gangguan tabel jejak akan menghambat pembacaan rekam medis. Diterima sadar. `RM-DEC-017` menyentuh wilayah di luar modul dan paling mungkin ditolak owner |
| **DoD** | Tujuh acceptance criteria terbukti uji, terutama nomor 2, 4, dan 5 yang merupakan jalur gagal |

### `BE-12` — Tinjauan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-12` |
| **Status** | **`SELESAI`** 26 Agustus 2026 — tinjauan akses (`MedicalRecordAccessReviewService`, `MedicalRecordAccessLogController`), 6 uji lulus; lihat ringkasan bagian 2. *Semula `TERTAHAN APPROVAL`; diselaraskan 2 November 2026* |
| **Outcome** | Unit rekam medis dapat memeriksa akses yang ditandai perlu ditinjau, sehingga jejak akses berguna alih-alih hanya menumpuk |
| **Trace** | `RM-DEC-005`; api-contract bagian 6 |
| **Reuse** | `BE-10` |
| **Scope** | `Services/MedicalRecordAccessReviewService.cs`; `Controllers/MedicalRecordAccessLogController.cs`; DTO |
| **Dependency** | `BE-10`, `BE-11` |
| **Acceptance criteria** | 1) Antrean tinjauan hanya memuat baris bertanda perlu ditinjau. 2) Menandai baris yang tidak perlu ditinjau ditolak. 3) Menandai ulang baris yang sudah ditinjau ditolak. 4) Tidak ada endpoint mengubah atau menghapus jejak |
| **Verification** | `AT-RM-08`, `AT-RM-29` |
| **Risk/blocker** | Risiko privasi: layar ini memuat `AccessReason` yang bertanda sensitif. Hak aksesnya harus lebih sempit daripada hak baca rekam medis |
| **DoD** | Endpoint berjalan; acceptance criteria terbukti uji; batasan hak akses tercatat pada permission matrix |

---

## 6. Milestone B3 — Penelusuran berkas

### `BE-13` — Service penggabungan riwayat

| Field | Isi |
|---|---|
| **Task ID** | `BE-13` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti uji. Lihat bagian 2 "Hasil `BE-13`" |
| **Outcome** | Riwayat klinis seorang pasien dapat diambil dari tiga belas sumber sekaligus dalam satu daftar berurut waktu |
| **Trace** | `RM-DEC-002`; `RM-CAP-004`; arsitektur bagian 5.8 |
| **Reuse** | Index `(PatientId, <tanggal>, IsDelete)` yang sudah ada pada seluruh tabel klinis; pola penggabungan `PrescriptionWorkspaceService` |
| **Scope** | `Services/MedicalRecordTimelineService.cs`. **Bertambah saat pengerjaan:** `DTOs/MedicalRecordTimelineDtos.cs`, karena service memerlukan bentuk permintaan dan balasannya sendiri. Diletakkan pada berkas terpisah supaya `DTOs/MedicalRecordDtos.cs` tetap menjadi milik `BE-14` |
| **Dependency** | `BE-02` untuk status keutuhan |
| **Acceptance criteria** | 1) Dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu. 2) Jumlah baris dibatasi dan penyaringan tanggal berfungsi. 3) Hanya jenis dokumen yang diminta yang diambil. 4) Bila satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai. 5) Memakai `AsNoTracking` |
| **Verification** | `AT-RM-09`, `AT-RM-31` |
| **Risk/blocker** | Risiko: penggabungan tiga belas sumber dapat menghasilkan banyak query. Ditutup pembatasan wajib pada acceptance criteria nomor 2 dan 3 |
| **DoD** | Acceptance criteria terbukti uji; waktu tanggap diukur pada data yang cukup banyak |

### `BE-14` — Endpoint berkas rekam medis

| Field | Isi |
|---|---|
| **Task ID** | `BE-14` |
| **Status** | **`SELESAI`** — dikerjakan 27 Agustus 2026, seluruh acceptance criteria terbukti uji. Lihat bagian 2 "Hasil `BE-14`" |
| **Outcome** | Frontend dapat menampilkan berkas rekam medis pasien lengkap dengan ringkasan dan riwayatnya |
| **Trace** | `RM-DEC-002`; api-contract bagian 2 |
| **Reuse** | `BE-11`, `BE-13` |
| **Scope** | `Controllers/MedicalRecordController.cs`; `DTOs/MedicalRecordDtos.cs`. Endpoint ringkasan, riwayat, detail dokumen, dan metadata penyaring. **Bertambah saat pengerjaan:** tiga metode baca pada `Services/MedicalRecordTimelineService.cs` — `GetSummaryAsync`, `GetDocumentCountsAsync`, `GetDocumentDetailAsync` — karena arsitektur bagian 5.9 melarang controller menyentuh `ApplicationDbContext` langsung |
| **Dependency** | `BE-11`, `BE-13`. **Tidak boleh dimulai sebelum `BE-03` selesai** sesuai `RM-DEC-019` |
| **Acceptance criteria** | 1) Setiap permintaan melewati pencatatan jejak lebih dulu. 2) Status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan. 3) Jenis dokumen yang belum tunduk ditandai jelas. 4) `PrivateNote` **tidak ada** pada respons mana pun di endpoint ini |
| **Verification** | `AT-RM-09`, `AT-RM-32`, `AT-RM-37` |
| **Risk/blocker** | Risiko: menampilkan catatan sebagai berkas resmi padahal baru CPPT yang terlindungi. Ditutup acceptance criteria nomor 3 dan `RM-FE-009` |
| **DoD** | Endpoint berjalan; jejak akses tercatat pada setiap permintaan; Swagger terisi |

### `BE-15` — Endpoint `PrivateNote`

| Field | Isi |
|---|---|
| **Task ID** | `BE-15` |
| **Status** | **`SELESAI` sebagian** — kode selesai 27 Agustus 2026, ketiga acceptance criteria terbukti uji. **DoD belum penuh:** pemberitahuan ke penulis CPPT belum dijalankan. Lihat bagian 2 "Hasil `BE-15`" dan `BE-15-pemberitahuan-penulis-cppt.md` |
| **Outcome** | Catatan pribadi klinisi tidak terlihat pada pemakaian sehari-hari, tetapi tetap dapat dibuka secara sah bila benar diperlukan |
| **Trace** | `RM-DEC-022`; api-contract bagian 2; validation matrix bagian 4 |
| **Reuse** | `BE-11` |
| **Scope** | Satu endpoint pada `MedicalRecordController`; izin terpisah `MedicalRecord : ReadPrivateNote` |
| **Dependency** | `BE-11`, `BE-14` |
| **Acceptance criteria** | 1) Alasan diminta **walaupun** pasien punya kunjungan aktif. 2) Jejak tercatat dengan `AccessScope = PrivateNote`. 3) Memakai izin terpisah, bukan izin baca biasa |
| **Verification** | `AT-RM-16`, `AT-RM-37` |
| **Risk/blocker** | Risiko: penulis CPPT selama ini menganggap kolom itu sepenuhnya pribadi. `RM-DEC-022` mewajibkan mereka diberi tahu bahwa tidak demikian. Ini pekerjaan komunikasi, bukan kode |
| **DoD** | Endpoint berjalan; penulis CPPT sudah diberi tahu perubahan sifat kolom ini |

### `BE-16` — Penanganan pasien hasil penggabungan

| Field | Isi |
|---|---|
| **Task ID** | `BE-16` |
| **Status** | **`SELESAI`** — dikerjakan 27 Agustus 2026, kedua acceptance criteria terbukti uji pada seluruh pintu masuk berkas. Lihat bagian 2 "Hasil `BE-16`" |
| **Outcome** | Pasien yang punya dua nomor rekam medis tidak ditampilkan riwayatnya secara terpotong tanpa peringatan |
| **Trace** | `RM-CAP-007`; validation matrix bagian 4; api-contract bagian 2 kode `409` |
| **Reuse** | `MstPatient.MergedToPatientId` yang sudah ada |
| **Scope** | Pemeriksaan pada `MedicalRecordController` sebelum riwayat diambil. **Letak sebenarnya:** pemeriksaan berada di `MedicalRecordAccessAuditService`, yang dipanggil setiap endpoint controller sebelum isi diambil. Diletakkan di sana agar aturannya ditulis satu kali, bukan empat kali di empat endpoint |
| **Dependency** | `BE-14`; keputusan closure question nomor 8 pada capability map revision 2 |
| **Acceptance criteria** | 1) Pasien dengan `MergedToPatientId` terisi dijawab `409` disertai nomor rekam medis pengganti. 2) Riwayat sebagian **tidak** ditampilkan |
| **Verification** | `AT-RM-22` |
| **Risk/blocker** | **Blocker:** kolom `MergedToPatientId` ada tetapi alur penggabungannya tidak ditemukan di controller mana pun. Perlu dipastikan lebih dulu apakah di lapangan benar ada pasien seperti ini. Bila ternyata tidak ada, task ini tetap dikerjakan sebagai pengaman, tetapi prioritasnya turun |
| **DoD** | Hasil penelusuran tercatat; perilaku `409` terbukti uji |

---

## 7. Milestone B4 — Pengerasan dan kesiapan

### `BE-17` — Uji jalur gagal lengkap

| Field | Isi |
|---|---|
| **Task ID** | `BE-17` |
| **Status** | **`SELESAI`** — dikerjakan 27 Agustus 2026. Keempat belas jalur gagal punya uji dan lulus; tidak ada yang ditandai dilewati. Lihat bagian 2 "Hasil `BE-17`" |
| **Outcome** | Empat belas jalur gagal terbukti berperilaku sebagaimana dirancang, bukan hanya jalur berhasil |
| **Trace** | Acceptance test matrix bagian 3 |
| **Reuse** | `BE-00` |
| **Scope** | Project test |
| **Dependency** | `BE-03` sampai `BE-16` |
| **Acceptance criteria** | Empat belas jalur gagal pada acceptance test matrix bagian 3 seluruhnya punya uji dan lulus |
| **Verification** | Keluaran perintah uji |
| **Risk/blocker** | Risiko: jalur gagal sering dianggap pelengkap lalu dilewati saat waktu menipis. Karena itu dijadikan task tersendiri, bukan diselipkan |
| **DoD** | Empat belas uji ada dan lulus; tidak ada yang ditandai dilewati |

### `BE-18` — Swagger dan catatan rilis

| Field | Isi |
|---|---|
| **Task ID** | `BE-18` |
| **Status** | **`SELESAI`** — dikerjakan 27 Agustus 2026, ketiga acceptance criteria terpenuhi dan tiga di antaranya terbukti uji. DoD lengkap: catatan rilis disetujui pemilik API pada hari yang sama (`RM-DEC-028`). Lihat bagian 2 "Hasil `BE-18`" |
| **Outcome** | Pemakai API mengetahui perubahan perilaku yang tidak terlihat dari bentuk permintaan maupun responsnya |
| **Trace** | api-contract bagian 8; manifest bagian dampak kompatibilitas |
| **Reuse** | Pengaturan Swagger yang sudah ada |
| **Scope** | Keterangan pada endpoint CPPT; catatan rilis |
| **Dependency** | `BE-03` |
| **Acceptance criteria** | 1) Swagger menyebut bahwa `ProviderUserId` dan `IsReadOnlyGenerated` diabaikan pada permintaan ubah. 2) Catatan rilis memuat empat perubahan perilaku pada manifest. 3) Keterangan bahwa baru CPPT yang tunduk aturan keutuhan |
| **Verification** | Pemeriksaan manual halaman Swagger dan catatan rilis |
| **Risk/blocker** | Risiko: mengabaikan kiriman klien tanpa pemberitahuan adalah praktik buruk. Task ini yang menutupnya |
| **DoD** | Swagger terbaca jelas; catatan rilis disetujui pemilik API |

---

## 8. Urutan pelaksanaan yang disarankan

```text
BE-00  (SIAP — dapat dimulai hari ini, tidak menunggu siapa pun)
   |
   +-- setelah tiga owner ditunjuk --------------------------------+
                                                                   |
BE-01 -> BE-02 -> BE-03 -> BE-08 (data lama)                       |
                     |                                             |
                     +-> BE-04 ------+                             |
                     |               +-- WAJIB dirilis bersama     |
                     +-> BE-05 -> BE-06 ------+                    |
                     |                                             |
                     +-> BE-07 (penguncian saat kunjungan ditutup) |
                                                                   |
BE-09 -> BE-10 -> BE-11 -> BE-12                                    |
                                                                   |
BE-13 -> BE-14 -> BE-15                                            |
            |                                                      |
            +-> BE-16 (butuh keputusan closure question no. 8)      |
                                                                   |
BE-17, BE-18 -------------------------------------------------------+
```

Tiga hal yang mengikat urutan di atas:

1. `BE-08` **wajib** selesai sebelum `BE-03` diaktifkan di produksi. Bila tidak, CPPT lama tidak
   punya baris keutuhan sementara CPPT baru punya, dan layar penelusuran akan menampilkan
   sebagian dokumen tanpa status tanpa penjelasan.
2. `BE-04` dan `BE-06` **wajib** dirilis bersamaan. Mengunci tanpa menyediakan addendum berarti
   tenaga klinis tidak dapat membetulkan catatan yang keliru sama sekali.
3. `BE-14` **tidak boleh** dimulai sebelum `BE-03` selesai, sesuai `RM-DEC-019`.

---

## 9. Risiko yang berdiri di atas seluruh roadmap

| Risiko | Dampak | Cara menutup | Owner |
|---|---|---|---|
| Tiga owner belum ditunjuk | 16 dari 19 task tertahan | Penunjukan owner | Manajemen rumah sakit |
| Tidak ada project test backend | Tiga perbaikan menyentuh kode berjalan tanpa jaring pengaman | `BE-00`, sudah `SIAP` | Arsitektur backend, `OPEN` |
| Jumlah data lama tidak diketahui | Lama dan dampak `BE-08` tidak dapat diperkirakan | **Dev tertutup** 2 November 2026 (10 CPPT, selesai dalam milidetik). Produksi: penelaahan ulang wajib saat pemasangan | Clinical governance |
| Bagian tahun baru lupa dibuat pada tabel jejak | Pembacaan rekam medis berhenti pada 1 Januari, karena gagal mencatat jejak berarti gagal membaca | Penjadwalan otomatis dan pemantauan, bukan pengingat manusia | Security/privacy, `OPEN` |
| `RM-DEC-017` menyentuh luar modul | Bila ditolak, `BE-11` berubah | Penerapan sudah dibatasi di dalam service modul | Security/privacy, `OPEN` |
| SOP rekam medis belum ada | Isi awal master keperluan akses tidak dapat ditetapkan | Permintaan SOP ke unit rekam medis | Product/domain, `OPEN` |

---

## 10. Yang sengaja tidak masuk roadmap ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Kelengkapan berkas, verifikasi koding, resume medis, peminjaman, retensi | Cakupan 4 sampai 8, rilis berikutnya menurut `RM-DEC-002` |
| Perbaikan validasi perpindahan status kunjungan | `RM-CAP-019`, di luar tiga celah yang ditetapkan `RM-DEC-019` |
| Penegakan tingkat kerahasiaan dokumen | Ditolak `RM-DEC-018` untuk rilis pertama |
| Perubahan pada `AccessPermissionService` | Ditolak arsitektur bagian 11 |
| Perapian nama domain `HealthService` menjadi `HealthServices` | Utang teknis yang harus jadi task tersendiri dengan approval pemilik arsitektur |
| Keutuhan untuk dua belas jenis dokumen selain CPPT | Rilis berikutnya, sesuai arsitektur bagian 7 |


---

# Roadmap revision 2 — Amandemen: berkas lintas modul dan kelengkapan

```yaml
module_id: RM-BP-001
blueprint_revision: 4
roadmap_revision: 2
status: APPROVED_INPUTS   # seluruh masukan disahkan; task berstatus sendiri-sendiri
owners:
  product_domain: Yoga Aji Pratama
  clinical_governance: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  api_authority: Yoga Aji Pratama
approved_by: [Yoga Aji Pratama]
approved_at: 2026-10-28        # RM-DEC-062
input_revisions:
  interview_decisions: 10
  capability_map: 3
  requirement_completeness: RM-RCG-001 r2
  domain_architecture: RM-DA-001 r1 (DOMAIN_ARCHITECTURE_PARTIAL)
  backend_architecture: 4 (bagian "Amandemen revision 4")
  prd_to_mvp: 1
artifact_hashes:
  interview_decisions: sha256:d7ccef575e6fae8f4e6f6b99015e4264ec080b8e6ac533175a67cf69d53a6924
  # hash artefak revision 4 lainnya: blueprint-manifest.md bagian "Revision 4 — approved"
contract_versions:
  api: 0.2.0 (approved 2026-10-28)
  state_transition: 0.2.0 (approved)
  validation: 0.2.0 (approved)
  integration: 0.2.0 (approved)
  permission_audit: 0.2.0 (approved)
source_commits:
  backend: 363de34f
  frontend: f0a3b07b8
module_owner_approvals:   # untuk berkas pembaca di folder modul lain
  laboratorium: approved (RM-DEC-063)
  mcu: approved (RM-DEC-063)
  radiologi: approved (RM-DEC-064)
  igd: approved (RM-DEC-067) — Rizki Gunawan, 2026-10-29
  rawat_inap: approved (RM-DEC-069) — Muhammad Hamzah, 2026-10-29
  kamar_operasi: approved (RM-DEC-071) — Muhammad Hamzah dan Ikbal Yuliyanto, 2026-10-29
  hemodialisa: approved (RM-DEC-072) — Muhammad Hamzah, 2026-10-29
  bank_darah: approved (RM-DEC-074) — Sukma, 2026-10-29
  farmasi: approved (RM-DEC-075) — Ikbal Yuliyanto, 2026-10-29
  gizi: approved (RM-DEC-073) — Ikbal Yuliyanto, 2026-10-29
```

## R2.1 Cara membaca bagian ini

| Status task | Arti |
|---|---|
| `SIAP` | Seluruh masukan disahkan; boleh diserahkan ke `build-module-backend` dengan approval task |
| `TERTAHAN PERSETUJUAN MODUL` | Task menulis di folder modul lain yang pemiliknya belum menyetujui. Bukan keputusan bisnis yang terbuka — desain dan kontraknya sudah disahkan |
| `TERTAHAN DEPENDENCY` | Menunggu task lain selesai |

**Aturan yang mengikat setiap task backend:**

1. QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md`
   backend dan dokumen engineering canonical — tidak disalin di sini.
2. **Pembuatan maupun eksekusi migration memerlukan instruksi dan konfirmasi terpisah** dari user
   (`CLAUDE.md` backend). Task yang memuat migration menyiapkan perubahan model dan configuration;
   pembuatan berkas migration dijalankan hanya setelah instruksi itu ada.
3. Build backend tanpa analyzer (`-p:RunAnalyzers=False`); server lokal dimatikan sebelum build.
4. Controller baru mendeklarasikan `filters/metadata` dan `summary` tepat sesudah konstruktor dan
   tidak memakai `<summary>` XML pada action.
5. Pembaca di folder modul lain **hanya** menambah satu berkas service dan pendaftaran DI; tidak
   mengubah perilaku modul itu, kecuali `BE-27` yang memang mengubah satu jalur IGD.

## R2.2 Ringkasan task

| Task | Gelombang | Epic | Ringkas | Status |
|---|---|---|---|---|
| `BE-21` | `MVP-0` | `RM-01` | Registry jenis, kontrak sumber, batas waktu per sumber, `unavailableKinds` | **`SELESAI`** 28 Oktober 2026 — 11 uji lulus, laporan `task/report/backend/BE-21.md` |
| `BE-22` | `MVP-0` | `RM-04` | Model `ClinicalManagement` baru + ringkasan berkala | **`SELESAI`** 28 Oktober 2026 — laporan `task/report/backend/BE-22.md` |
| `BE-23` | `MVP-1` | `RM-02` | Pembaca Laboratorium (PK/Mikro) + pemeriksaan tertunda termasuk PA | **`SELESAI`** 28 Oktober 2026 — laporan `task/report/backend/BE-23.md` |
| `BE-24` | `MVP-1` | `RM-02` | Pembaca Radiologi + pemeriksaan tertunda | **`SELESAI`** 28 Oktober 2026 — laporan `task/report/backend/BE-24.md` |
| `BE-25` | `MVP-2` | `RM-06` | Penutupan klinis kunjungan | **`SELESAI`** 28 Oktober 2026 — 13 uji lulus, laporan `task/report/backend/BE-25.md` |
| `BE-26` | `MVP-2` | `RM-06` | Penanda `Susulan` di linimasa | **`SELESAI`** 29 Oktober 2026 — 23 uji lulus (seluruh project `85`), laporan `task/report/backend/BE-26.md` |
| `BE-27` | `MVP-2` | `RM-06` | Disposisi IGD ke rawat inap mengunci dan mencatat penutupan | **`SELESAI`** 29 Oktober 2026 — 7 uji lulus (seluruh project `151`), laporan `task/report/backend/BE-27.md` |
| `BE-28` | `MVP-2` | `RM-06` | Pemanggil penutupan meneruskan `ClinicalClosureSource` | **`SELESAI`** 29 Oktober 2026 — seluruh jalur, 17 uji lulus (seluruh project `168`), laporan `task/report/backend/BE-28.md` |
| `BE-29` | `MVP-3` | `RM-03` | Pembaca IGD | **`SELESAI`** 29 Oktober 2026 — 9 uji lulus (seluruh project `184`), laporan `task/report/backend/BE-29.md` |
| `BE-30` | `MVP-3` | `RM-03` | Pembaca Rawat Inap | **`SELESAI`** 29 Oktober 2026 — 7 uji lulus (seluruh project `175`), laporan `task/report/backend/BE-30.md` |
| `BE-31` | `MVP-3` | `RM-03` | Pembaca Kamar Operasi | **`SELESAI`** 2 November 2026 — 10 uji lulus (seluruh project `194`), laporan `task/report/backend/BE-31.md`; kriteria per kasus selesai dan checklist tiga fase (`RM-DEC-076`) |
| `BE-32` | `MVP-3` | `RM-03` | Pembaca Hemodialisa | **`SELESAI`** 2 November 2026 — 7 uji lulus (seluruh project `201`), laporan `task/report/backend/BE-32.md` |
| `BE-33` | `MVP-3` | `RM-03` | Pembaca Gizi | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `207`), laporan `task/report/backend/BE-33.md` |
| `BE-34` | `MVP-3` | `RM-03` | Pembaca Bank Darah | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `213`), laporan `task/report/backend/BE-34.md` |
| `BE-35` | `MVP-3` | `RM-04` | Pembaca Farmasi | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `219`), laporan `task/report/backend/BE-35.md` |
| `BE-36` | `MVP-4` | `RM-05` | Pembaca MCU | **`SELESAI`** 28 Oktober 2026 — laporan `task/report/backend/BE-36.md`; acceptance dikoreksi `RM-DEC-065`/`066` |
| `BE-37` | `MVP-5` | `RM-07` | Profil dokumen wajib + isi awal + endpoint master | **`SELESAI`** 29 Oktober 2026 — 17 uji lulus (seluruh project `102`), laporan `task/report/backend/BE-37.md` |
| `BE-38` | `MVP-5` | `RM-09` | Addendum pengesahan | **`SELESAI`** 29 Oktober 2026 — 12 uji lulus (seluruh project `114`), laporan `task/report/backend/BE-38.md` |
| `BE-39` | `MVP-6` | `RM-08` | Penilaian kelengkapan + endpoint daftar kerja | **`SELESAI`** 29 Oktober 2026 — 21 uji lulus (seluruh project `135`), laporan `task/report/backend/BE-39.md` |
| `BE-40` | `MVP-6` | `RM-08` | Pemeriksaan berkala dan pemicu penilaian ulang | **`SELESAI`** 29 Oktober 2026 — 9 uji lulus (seluruh project `144`), laporan `task/report/backend/BE-40.md` |
| `BE-41` | `MVP-6` | — | Swagger, catatan rilis, pemberitahuan perubahan perilaku amandemen | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `225`), laporan `task/report/backend/BE-41.md`; catatan rilis menunggu pengesahan |

**Keadaan 2 November 2026: seluruh 21 task backend amandemen (`BE-21`–`BE-41`) `SELESAI`.** Seluruh
pembaca sumber tersambung; tidak ada lagi kriteria maupun kondisi yang dianggap tidak ada karena
pembacanya belum tersedia. Yang tersisa: pengesahan catatan rilis amandemen (`catatan-rilis.md`
bagian A9), layar `FE-10`–`FE-15`, dan penerapan migration di luar devYoga. Paragraf di bawah
mencatat keadaan 28 Oktober 2026 dan dipertahankan sebagai riwayat.

**Skema amandemen sudah diterapkan ke devYoga — 28 Oktober 2026, atas instruksi user.** Lapisan persistensi
`BE-25`, `BE-37`, `BE-38`, dan `BE-39` (model, enum, configuration, DbSet) dibuat lebih dulu, lalu satu migration
`20261028070658_AddMedicalRecordCompletenessFoundation` diterapkan bersama enam migration modul lain yang tertunda
(riwayat 362 → 369, nol tertunda; uji kering naik → `ROLLBACK` lebih dulu). **Layanan, endpoint, dan seeder keempat task
belum dikerjakan** (keadaan 28 Oktober; seluruhnya selesai 29 Oktober) — statusnya tetap seperti tabel di atas; bagian migration pada masing-masing task sudah tuntas.
Dua selisih terhadap A4.6–A4.7: (1) **satu** migration gabungan, bukan empat, karena keempatnya dirilis bersama; (2) kolom
`SortOrder` pada `MstMedicalRecordCompletenessProfile` dan butirnya **tidak** dibuat — QBE melarang `SortOrder` presentasi
generik yang dipersistensi pada kode baru; daftar profil diurutkan menurut kode, butir menurut nilai kriteria.

**Yang dapat berjalan sekarang tanpa menunggu siapa pun** (keadaan 28 Oktober 2026): `BE-21`, `BE-25`, `BE-37`, `BE-38`,
lalu `BE-22`, `BE-23`, `BE-24`, `BE-36`, `BE-26`, dan `BE-39`/`BE-40`. Delapan pembaca yang
tertahan **tidak** menahan `BE-39`: kriteria yang dijawab modul tanpa pembaca menghasilkan
kekurangan `SourceUnavailable` dan keadaan `Undetermined` — jujur, sesuai `RM-DEC-044`. Kunjungan
Rawat Jalan poliklinik, Telemedicine, serta Rawat Jalan unit Laboratorium dan Radiologi sudah dapat
dinilai penuh begitu `BE-39` selesai.

## R2.3 Milestone B6 — Fondasi amandemen (`MVP-0`)

### `BE-21` — Registry jenis, kontrak sumber, dan batas waktu per sumber

| Field | Isi |
|---|---|
| **Task ID** | `BE-21` |
| **Status** | **`SELESAI`** — 28 Oktober 2026. Laporan `task/report/backend/BE-21.md`. Tiga selisih aditif terhadap rencana dicatat di laporan bagian 7: `UnavailableKinds` juga pada ringkasan berkas; enum `CompletenessCriterion`/`CompletenessCondition` dibuat di sini (bukan `BE-37`); `IsComplete` tidak dipengaruhi jenis belum tersedia |
| **Outcome** | Linimasa tidak pernah lagi menjawab kosong diam-diam; satu sumber lambat tidak membatalkan berkas |
| **Trace** | `RM-DEC-030`, `044`, `058`; `RM-CAP-034`, `035`; `FR-RM-001`, `FR-RM-002`; `GAP-ARCH-RM-05` |
| **Kontrak** | API `0.2.0` A1 (`unavailableKinds`, `failedSources`); integration `0.2.0` `INT-RM-10` |
| **Reuse** | `MedicalRecordTimelineService`, `FailedSources`, `MedicalRecordTimelineSourceFailure` |
| **Scope** | `Enums/ClinicalDocumentKind.cs` (nilai 16–44); `Services/Sources/IMedicalRecordDocumentSource.cs`, `IMedicalRecordCompletenessProbe.cs`; `DTOs/MedicalRecordSourceDtos.cs`; `MedicalRecordTimelineService.cs`: daftar jenis dari registry, `unavailableKinds`, batas waktu per sumber dan per berkas, pembedaan pembatalan pengguna dari batas waktu (A4.9 no. 1–3); konfigurasi `MedicalRecord:SourceTimeoutSeconds`, `FileTimeoutSeconds`. Jenis 14 dan 15 dilaporkan `unavailableKinds` sampai `BE-23`/`BE-32` mengisinya |
| **Dependency** | — |
| **Acceptance criteria** | 1) Jenis tanpa pembaca muncul di `unavailableKinds`, tidak di linimasa kosong. 2) Sumber dipalsukan 10 detik → masuk `failedSources`, respons `200`. 3) Pembatalan oleh pengguna tetap membatalkan permintaan. 4) Klien `0.1.1` tetap dapat membaca respons |
| **Verification** | `AT-RM-49`, `AT-RM-50`, `AT-RM-51`; uji kompatibilitas bentuk respons. **Hasil:** `dotnet test Tests/QuilvianSystemBackend.MedicalRecordTests` → `Passed: 11`, `Failed: 0` |
| **Risk/owner** | Mengubah perilaku layanan yang dipakai layar berjalan. Pemilik: rekam medis |
| **DoD** | Kriteria 1–4 terbukti uji; build lolos |

### `BE-22` — Model `ClinicalManagement` baru dan ringkasan berkala

| Field | Isi |
|---|---|
| **Task ID** | `BE-22` |
| **Status** | **`SELESAI`** — 28 Oktober 2026, laporan `task/report/backend/BE-22.md`. `dotnet test` → `Passed: 49`, `Failed: 0` |
| **Outcome** | Rencana asuhan, visite dokter, intervensi, monitoring transfusi, surat dokter, evaluasi manajemen kasus, infeksi nosokomial, dan serah terima tampil; pencatatan berkala tampil satu baris per jenis per hari |
| **Trace** | `RM-DEC-039` kelompok C, `RM-DEC-052`; `RM-CAP-059`–`067`; `FR-RM-031` |
| **Kontrak** | API `0.2.0` A1 (`periodicSummary`, `GET /{patientId}/periodic-observations`); validation `VAL-RM-A11` |
| **Reuse** | Penarik generik `AmbilAsync` (`RM-CAP-042`) |
| **Scope** | `MedicalRecordTimelineService.cs` (cabang jenis 35–43); `PeriodicObservationSummaryService.cs`; `Enums/PeriodicObservationKind.cs`; `MedicalRecordController.cs` (endpoint baru). **Nol** berkas di `ClinicalManagement` |
| **Dependency** | `BE-21` |
| **Acceptance criteria** | 1) Tiap jenis 35–42 tampil bila final menurut A4.10. 2) Enam pengukuran satu hari, satu batal → satu baris berjumlah 5. 3) Tanggal ringkasan memakai tanggal WIB (`NFR-005`). 4) `kind`/`date` tidak sah → `400` |
| **Verification** | `AT-RM-52`; uji per jenis |
| **Risk/owner** | Banyak jenis sekaligus menambah waktu baca berurutan; diukur terhadap batas `BE-21`. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji |

## R2.4 Milestone B7 — Sumber penunjang (`MVP-1`)

### `BE-23` — Pembaca Laboratorium

| Field | Isi |
|---|---|
| **Task ID** | `BE-23` |
| **Status** | **`SELESAI`** — 28 Oktober 2026, laporan `task/report/backend/BE-23.md`. `dotnet test` → `Passed: 19`, `Failed: 0`. Satu butir DoD terbuka: kewajiban `S6` belum dicatat di blueprint Laboratorium |
| **Outcome** | Hasil PK dan Mikrobiologi yang dirilis tampil; pemeriksaan tertunda (termasuk PA) tersedia untuk kelengkapan |
| **Trace** | `RM-DEC-030`–`035`, `057`, `058`; `RM-CAP-043`, `069`; `FR-RM-010`; blocker 7 |
| **Kontrak** | Integration `0.2.0` `INT-RM-10a`; API A1 |
| **Reuse** | Rantai `LabExamination` → `LabOrder.EncounterId` → `RegPatientEncounters` (pola `RadReportService.GetByPatientAsync`) |
| **Scope** | **Baru** `Areas/HealthServices/LaboratoryManagement/Services/LabMedicalRecordDocumentSource.cs` (pembaca + uji kelengkapan pemeriksaan tertunda); pendaftaran DI di `Program.cs`. `isCorrected = false` sampai `S6`. PA dikecualikan dari bacaan, **tidak** dari pemeriksaan tertunda |
| **Dependency** | `BE-21` |
| **Acceptance criteria** | 1) Hasil `ReleasedAt` terisi tampil; belum dirilis tidak. 2) Hasil PA tidak pernah tampil. 3) Pemeriksaan PA belum dirilis terhitung tertunda. 4) Pengguna tanpa izin Lab tetap melihat hasil. 5) Nol kolom baru di tabel Lab |
| **Verification** | `AT-RM-46`, `AT-RM-51`, `AT-RM-53` |
| **Risk/owner** | Kewajiban `S6` (`RM-DEC-057`) wajib dicatat di roadmap Laboratorium. Pemilik: rekam medis + Laboratorium |
| **DoD** | Kriteria terbukti uji; catatan kewajiban `S6` masuk blueprint Laboratorium |

### `BE-24` — Pembaca Radiologi

| Field | Isi |
|---|---|
| **Task ID** | `BE-24` |
| **Status** | **`SELESAI`** — 28 Oktober 2026, laporan `task/report/backend/BE-24.md`. `dotnet test` → `Passed: 25`, `Failed: 0` |
| **Outcome** | Laporan radiologi dirilis tampil, versi amandemen bertanda pernah diralat |
| **Trace** | `RM-DEC-038`, `041`, `042`; `RM-CAP-045`, `058`; `FR-RM-011` |
| **Kontrak** | `INT-RM-10b` |
| **Reuse** | `RadReportService.GetByPatientAsync` apa adanya, dipanggil dalam proses |
| **Scope** | **Baru** `RadiologyManagement/Services/RadMedicalRecordDocumentSource.cs` + uji pemeriksaan tertunda; DI |
| **Dependency** | `BE-21` |
| **Acceptance criteria** | 1) Hanya laporan dirilis. 2) `CurrentVersionNumber > 1` → `isCorrected`. 3) Tidak memeriksa `RadReport : Read`. 4) Order tanpa laporan dirilis terhitung tertunda |
| **Verification** | `AT-RM-45`, `AT-RM-48` |
| **Risk/owner** | Rendah. Pemilik: rekam medis + Radiologi |
| **DoD** | Kriteria terbukti uji |

## R2.5 Milestone B8 — Penutupan klinis dan Susulan (`MVP-2`)

### `BE-25` — Penutupan klinis kunjungan

| Field | Isi |
|---|---|
| **Task ID** | `BE-25` |
| **Status** | **`SELESAI`** — 28 Oktober 2026, laporan `task/report/backend/BE-25.md`. `dotnet test` → `Passed: 13` (seluruh project `62`), `Failed: 0`. Selisih terhadap scope: model, enum, configuration, DbSet, dan migration sudah dibuat lebih dulu (R2.2); task ini hanya mengubah `ClinicalDocumentIntegrityService.cs`. Kriteria 2 "nol galat" dijamin pada empat jalur bertransaksi; dua jalur tanpa transaksi (PATCH status, antrean dokter) gagal utuh lalu dapat diulang — penyempurnaannya calon `BE-28` |
| **Outcome** | Setiap kunjungan yang ditutup sejak rilis punya tepat satu waktu tutup klinis yang tidak berubah |
| **Trace** | `RM-DEC-051`; `AGG-RM-02`; `RM-CAP-036`; `FR-RM-050`, `FR-RM-051`; `NFR-001`, `NFR-002` |
| **Kontrak** | State `0.2.0` S-A; integration `INT-RM-12` |
| **Reuse** | `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync` dan enam pemanggilnya |
| **Scope** | **Baru** `Models/MrcEncounterClinicalClosure.cs`, `Enums/ClinicalClosureSource.cs`, `Repositories/Configurations/HealthServices/MedicalRecordManagement/MrcEncounterClinicalClosureConfiguration.cs`, DbSet; **Diperbarui** `ClinicalDocumentIntegrityService.cs` (parameter opsional `ClinicalClosureSource`, pencatatan idempoten). Migration `AddMrcEncounterClinicalClosure` — **menunggu instruksi terpisah**. Nol berkas di modul lain |
| **Dependency** | — |
| **Acceptance criteria** | 1) Dua penutupan → satu baris, waktu pertama. 2) Penutupan serentak → satu baris, nol galat. 3) Kunjungan tanpa catatan konsep tetap tercatat. 4) Rekonsiliasi IGD tidak mengubah baris. 5) Gagal mencatat → penutupan kunjungan gagal |
| **Verification** | `AT-RM-56`, `AT-RM-57` |
| **Risk/owner** | Masuk transaksi enam jalur penutupan; diuji per jalur. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; migration disiapkan menunggu instruksi |

### `BE-26` — Penanda Susulan

| Field | Isi |
|---|---|
| **Task ID** | `BE-26` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-26.md`. `dotnet test` → `Passed: 23` (seluruh project `85`), `Failed: 0`. Waktu final 13 jenis lama tidak ditetapkan desain; dipakai waktu tanda tangan keutuhan, lalu waktu masuk baris (`CreateDateTime`) — selisih dicatat di laporan bagian 7 |
| **Outcome** | Dokumen jenis apa pun yang final sesudah tutup klinis bertanda `Susulan` |
| **Trace** | `RM-DEC-006`, `043`, `051`; `RM-CAP-037`; `RM-FACT-011`, `018`; `FR-RM-052` |
| **Kontrak** | API A1 (`isLateEntry`, `clinicalClosedAt`, `isClinicalClosureDerived`) |
| **Reuse** | `BE-25`; urutan sumber data lama A4.12 |
| **Scope** | `MedicalRecordTimelineService.cs`; DTO linimasa |
| **Dependency** | `BE-21`, `BE-25` |
| **Acceptance criteria** | 1) Final 1 detik sesudah tutup → `Susulan`; 1 detik sebelum → tidak. 2) Berlaku juga untuk 13 jenis lama. 3) Kunjungan lama memakai waktu turunan dan bertanda `isClinicalClosureDerived` |
| **Verification** | `AT-RM-47`, `AT-RM-58` |
| **Risk/owner** | Rendah. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; `RM-FACT-011` ditutup |

### `BE-27` — Disposisi IGD ke rawat inap mengunci catatan

| Field | Isi |
|---|---|
| **Task ID** | `BE-27` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-27.md`. `dotnet test` → `Passed: 7` (seluruh project `151`), `Failed: 0`. Jalur utama ternyata sudah mengunci lewat `EmergencyVisitService.TryCloseAfterDispositionAsync` sebelum audit `GAP-ARCH-RM-01`; yang ditutup task ini adalah sumber penutupan `EmergencyAdmissionDisposition` dan jalur cadangan baris 1585–1598 yang memang tanpa penguncian |
| **Outcome** | Catatan IGD pasien yang lanjut rawat inap terkunci dan kunjungannya tercatat tutup klinis |
| **Trace** | `RM-DEC-003`; `GAP-ARCH-RM-01`; `FR-RM-053` |
| **Kontrak** | `INT-RM-12` |
| **Reuse** | `LockOpenDocumentsForEncounterAsync` |
| **Scope** | `EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs` sekitar baris 1585–1598: memanggil penguncian di dalam transaksi yang sama, sumber `EmergencyAdmissionDisposition` |
| **Dependency** | `BE-25` |
| **Acceptance criteria** | 1) Catatan konsep IGD menjadi `LockedUnsigned`. 2) Penutupan klinis tercatat. 3) Penguncian gagal → disposisi gagal |
| **Verification** | `AT-RM-59` |
| **Risk/owner** | Mengubah jalur IGD yang sedang dipakai. Pemilik: IGD |
| **DoD** | Persetujuan pemilik IGD tercatat; kriteria terbukti uji |

### `BE-28` — Sumber penutupan diteruskan pemanggil (opsional)

| Field | Isi |
|---|---|
| **Task ID** | `BE-28` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-28.md`. `dotnet test` → `Passed: 17` (seluruh project `168`), `Failed: 0`. Seluruh pemanggil penguncian meneruskan sumbernya; jalur IGD membedakan disposisi rawat inap dari penutupan IGD lain. Transaksi pada dua jalur tanpa transaksi (catatan `BE-25`) **tidak** ditambahkan — di luar batas "satu argumen per pemanggil" |
| **Outcome** | `ClosureSource` mencerminkan jalur sebenarnya, bukan bawaan `EncounterStatusPatch` |
| **Trace** | `INT-RM-12` |
| **Scope** | Enam pemanggil pada tabel `INT-RM-12`; satu argumen per pemanggil |
| **Dependency** | `BE-25` |
| **Acceptance criteria** | Setiap jalur menulis `ClosureSource` yang sesuai |
| **Verification** | Uji per jalur |
| **Risk/owner** | **Tidak memblokir apa pun** — tanpa task ini sumber tercatat bawaan. Pemilik: modul masing-masing |
| **DoD** | Kriteria terbukti uji |

## R2.6 Milestone B9 — Sumber pelayanan dan Farmasi (`MVP-3`)

Ketujuh task berbentuk sama: **satu berkas baru** `<Prefix>MedicalRecordDocumentSource.cs` di
`Services/` modul pemilik, pendaftaran DI, dan uji. Kolom final, waktu final, jalan ke pasien,
`isCorrected`, dan `isProvisional` mengikuti `02-backend-architecture.md` A4.10; kriteria dan kondisi
mengikuti A4.11. Dependency semua: `BE-21`. Reuse: model modul itu sendiri, tanpa perubahan.

| Task | Modul / berkas | Jenis | Kriteria dan kondisi yang dijawab | Acceptance criteria khusus | Verifikasi | Status |
|---|---|---|---|---|---|---|
| `BE-29` | IGD / `EmgMedicalRecordDocumentSource.cs` | 17–20 | `EmergencyTriage`, `EmergencyDisposition` | Triase `Superseded` tampil sebagai triase yang digantikan; disposisi `Draft` tidak tampil dan tidak memenuhi | `AT-RM-44`, AC-50 | **`SELESAI`** 29 Oktober 2026 — 9 uji lulus (seluruh project `184`), laporan `task/report/backend/BE-29.md`; disposisi final = `Confirmed`/`ConfirmedAt` |
| `BE-30` | Rawat Inap / `InpMedicalRecordDocumentSource.cs` | 21, 22 | `InpatientGeneralConsent`, `DischargeSummary` | Ringkasan tanpa `SignedAt` tidak tampil; persetujuan registrasi tidak memenuhi `InpatientGeneralConsent` | `AT-RM-45`, `AT-RM-60` | **`SELESAI`** 29 Oktober 2026 — 7 uji lulus (seluruh project `175`), laporan `task/report/backend/BE-30.md` |
| `BE-31` | Kamar Operasi / `OprMedicalRecordDocumentSource.cs` | 23–26 | `SurgeryReport`, `AnesthesiaRecord`, `SurgicalSafetyChecklist`; kondisi `Surgery` | Kasus `Cancelled`/`Rejected`/`Postponed` tidak memicu kondisi | `AT-RM-61` | **`SELESAI`** 2 November 2026 — 10 uji lulus (seluruh project `194`), laporan `task/report/backend/BE-31.md`; catatan kelahiran final saat tersimpan; kriteria per kasus `Completed`, checklist Sign In/Time Out/Sign Out (`RM-DEC-076`) |
| `BE-32` | Hemodialisa / `HmdMedicalRecordDocumentSource.cs` | 14 | `HemodialysisSessionSigned`; kondisi `HemodialysisSession` | Jenis 14 keluar dari `unavailableKinds` | `AT-RM-66` | **`SELESAI`** 2 November 2026 — 7 uji lulus (seluruh project `201`), laporan `task/report/backend/BE-32.md`; sesi `Stopped` ikut memicu kondisi dan wajib disahkan |
| `BE-33` | Gizi / `GziMedicalRecordDocumentSource.cs` | 27 | — | `isProvisional = true` | `AT-RM-54` | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `207`), laporan `task/report/backend/BE-33.md`; final = `CreateDateTime`, kejadian = `VisitAt` |
| `BE-34` | Bank Darah / `BbkMedicalRecordDocumentSource.cs` | 28, 29 | — | Golongan darah tanpa kunjungan tetap tampil | `AT-RM-44` | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `213`), laporan `task/report/backend/BE-34.md`; golongan darah berketerangan berlaku/ditahan/tidak berlaku |
| `BE-35` | Farmasi / `PhmMedicalRecordDocumentSource.cs` | 30–32 | — | Resep `Draft` tidak tampil | `AT-RM-44` | **`SELESAI`** 2 November 2026 — 6 uji lulus (seluruh project `219`), laporan `task/report/backend/BE-35.md`; rekonsiliasi memakai keputusan terakhir |

Outcome bersama: dokumen final modul tersebut tampil di berkas tanpa izin modul asal. Risiko:
rendah per modul — hanya menambah berkas baca. DoD bersama: persetujuan pemilik modul tercatat
pada decision log; kriteria terbukti uji; `AT-RM-44` lulus untuk modul itu.

## R2.7 Milestone B10 — MCU (`MVP-4`)

### `BE-36` — Pembaca MCU

| Field | Isi |
|---|---|
| **Task ID** | `BE-36` |
| **Status** | **`SELESAI`** — 28 Oktober 2026, laporan `task/report/backend/BE-36.md`. `dotnet test` → `Passed: 34`, `Failed: 0`. Desain semula keliru membaca `Corrected` sebagai final (`RM-FACT-022`); dikoreksi `RM-DEC-065`, `RM-DEC-066` |
| **Outcome** | Resume dan hasil MCU terverifikasi tampil |
| **Trace** | `RM-DEC-040`; `RM-CAP-057`; `FR-RM-040`; `MCU-GAP-04` |
| **Kontrak** | `INT-RM-10j` |
| **Scope** | **Baru** `MedicalCheckupManagement/Services/McuMedicalRecordDocumentSource.cs` (jenis 33, 34; kriteria `McuResume`); DI |
| **Dependency** | `BE-21` |
| **Acceptance criteria** | ~~1) Resume `Verified`/`Corrected` tampil; `Draft`/`Approved` tidak. 2) Resume `Corrected` → `isCorrected`.~~ **Diganti `RM-DEC-065`/`066` (28 Oktober 2026):** 1) Versi terakhir yang pernah diverifikasi tampil; order tanpa versi terverifikasi tidak; selama koreksi berketerangan "sedang dikoreksi" tanpa isi konsep. 2) Versi terverifikasi > 1 → `isCorrected`. 3) Peserta tanpa `PatientId` tidak muncul dan tidak menimbulkan galat. 4) Hasil pemeriksaan memakai nilai versi terverifikasi |
| **Verification** | `AT-RM-44`; UAT-09, UAT-10 |
| **Risk/owner** | Blueprint MCU masih menyebut `MCU-GAP-04` ditunda — diselaraskan di blueprint MCU. Pemilik: rekam medis + MCU |
| **DoD** | Kriteria terbukti uji |

## R2.8 Milestone B11 — Master dan addendum (`MVP-5`)

### `BE-37` — Profil dokumen wajib

| Field | Isi |
|---|---|
| **Task ID** | `BE-37` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-37.md`. `dotnet test` → `Passed: 17` (seluruh project `102`), `Failed: 0`. Model, enum, configuration, dan migration sudah ada sejak R2.2; task ini menambah DTO, service, controller, seeder, dan pendaftarannya. Selisih kontrak A3: `items[].sortOrder` tidak diterima (kolomnya tidak dibuat, R2.2) |
| **Outcome** | Sebelas profil awal tersedia dan dapat dikelola tanpa rilis program |
| **Trace** | `RM-DEC-045`, `047`, `053`–`056`, `060`, `061`; `RM-CAP-038`; `FR-RM-060`–`062` |
| **Kontrak** | API `0.2.0` A3; validation `VAL-RM-A04`–`A07`, `A10`; permission `MedicalRecordCompletenessProfile : Read/Create/Update` |
| **Reuse** | Pola `MedicalRecordAccessPurposeController`/`Service` |
| **Scope** | **Baru** `MasterData/Models/MstMedicalRecordCompletenessProfile.cs`, `…ProfileItem.cs`; configuration di `Repositories/Configurations/HealthServices/MedicalRecordManagement/`; `Enums/CompletenessCriterion.cs`, `CompletenessCondition.cs`; `MasterData/Services/MedicalRecordCompletenessProfileService.cs`; `MasterData/Controllers/MedicalRecordCompletenessProfileController.cs`; `MasterData/DTOs/MedicalRecordCompletenessProfileDtos.cs`; `MasterData/Seeders/MedicalRecordCompletenessProfileSeeder.cs` (A4.8, idempoten). Migration `AddMedicalRecordCompletenessProfile` — **menunggu instruksi terpisah** |
| **Dependency** | — |
| **Acceptance criteria** | 1) Seeder mengisi sebelas profil; dijalankan dua kali tetap sebelas. 2) `VAL-RM-A04`–`A07`, `A10` terbukti. 3) Perubahan profil tercatat logger dengan sebelum-sesudah di teks pesan. 4) Profil hanya dinonaktifkan, tidak dihapus |
| **Verification** | `AT-RM-70`; uji seeder |
| **Risk/owner** | Isi awal bukan SOP rumah sakit (`RM-DEC-047`). Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; migration disiapkan menunggu instruksi |

### `BE-38` — Addendum pengesahan

| Field | Isi |
|---|---|
| **Task ID** | `BE-38` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-38.md`. `dotnet test` → `Passed: 12` (seluruh project `114`), `Failed: 0`. Kolom, enum, index unik terfilter, dan migration sudah ada sejak R2.2; task ini mengubah service, DTO, dan controller addendum. Penandaan kunjungan untuk dinilai ulang (A4.12) belum dikerjakan — milik pemicu penilaian ulang `BE-40` |
| **Outcome** | Catatan `LockedUnsigned` dapat disahkan penulisnya tanpa mengubah statusnya |
| **Trace** | `RM-DEC-049`; `RM-CAP-039`; `FR-RM-080`, `FR-RM-081` |
| **Kontrak** | API A4; state S-C; validation `VAL-RM-A01`–`A03` |
| **Reuse** | `ClinicalNoteAddendumService`, jalur `as-substitute`, endpoint `/authority` |
| **Scope** | `Models/MrcClinicalNoteAddendum.cs` (`AddendumPurpose`); `Enums/ClinicalNoteAddendumPurpose.cs`; configuration (index unik terfilter); `ClinicalNoteAddendumService.cs`; DTO addendum. Migration `AddMrcClinicalNoteAddendumPurpose` — **menunggu instruksi terpisah** |
| **Dependency** | — |
| **Acceptance criteria** | 1) Pengesahan atas `LockedUnsigned` tersimpan; status tetap. 2) Atas `Signed` → `409`. 3) Pengesahan kedua → `409`. 4) Bukan penulis tanpa jalur pengganti → `403`. 5) Klien lama tanpa `purpose` tetap membuat addendum koreksi |
| **Verification** | `AT-RM-72`, `AT-RM-73`, `AT-RM-74` |
| **Risk/owner** | Rendah. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji |

## R2.9 Milestone B12 — Kelengkapan (`MVP-6`)

### `BE-39` — Penilaian kelengkapan dan daftar kerja

| Field | Isi |
|---|---|
| **Task ID** | `BE-39` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-39.md`. `dotnet test` → `Passed: 21` (seluruh project `135`), `Failed: 0`. Model, enum, configuration, dan migration sudah ada sejak R2.2. **Pemicu penilaian pertama** diputuskan pemilik modul 29 Oktober 2026: penutupan klinis hanya menaruh id kunjungan di antrean memori, dan worker latar menilainya di luar transaksi penutupan. Isi antrean hilang saat restart; disusul `BE-40` |
| **Outcome** | Petugas rekam medis melihat kunjungan yang belum lengkap beserta seluruh kekurangannya |
| **Trace** | `RM-DEC-045`, `046`, `050`, `051`, `053`–`056`, `060`; `AGG-RM-03`; `RM-CAP-040`; `FR-RM-070`–`074`; `NFR-003`, `NFR-006` |
| **Kontrak** | API A2; state S-B; validation `VAL-RM-A08`, `A09`; permission `MedicalRecordCompleteness : Read/Evaluate` |
| **Reuse** | `MrcClinicalDocumentIntegrity` untuk syarat 1; profil `BE-37`; pembaca uji yang sudah ada |
| **Scope** | **Baru** `Models/MrcEncounterCompletenessAssessment.cs` + configuration; `Enums/CompletenessState.cs`, `CompletenessDeficiencyType.cs`; `Services/MedicalRecordCompletenessService.cs` (penyusunan profil A4.11, kriteria generik `ClinicalManagement`, panggilan seluruh `IMedicalRecordCompletenessProbe`); `Controllers/MedicalRecordCompletenessController.cs`; `DTOs/MedicalRecordCompletenessDtos.cs`; penilaian pertama dipicu penutupan klinis. Migration `AddMrcEncounterCompletenessAssessment` — **menunggu instruksi terpisah** |
| **Dependency** | `BE-23`, `BE-25`, `BE-37`, `BE-38` |
| **Acceptance criteria** | 1) Urutan keadaan dan rincian lengkap. 2) Pembaca gagal atau belum ada → `Undetermined`, tidak pernah `Complete`. 3) RJ unit Laboratorium `Complete` setelah dirilis. 4) Kunjungan berjalan/batal → `409`. 5) Kunjungan sebelum rilis dinilai tanpa disimpan. 6) Penutupan, pemulangan, tagihan tidak tertahan. 7) Logger tanpa `Deficiencies` |
| **Verification** | `AT-RM-60`–`AT-RM-68`, `AT-RM-71` |
| **Risk/owner** | Kunjungan IGD/Rawat Inap tetap `Undetermined` sampai `BE-29`–`BE-31` — **sesuai desain**, wajib diberitahukan pada catatan rilis. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; migration disiapkan menunggu instruksi |

### `BE-40` — Pemeriksaan berkala dan pemicu penilaian ulang

| Field | Isi |
|---|---|
| **Task ID** | `BE-40` |
| **Status** | **`SELESAI`** — 29 Oktober 2026, laporan `task/report/backend/BE-40.md`. `dotnet test` → `Passed: 9` (seluruh project `144`), `Failed: 0`. Kedua kewajiban tambahan dari `BE-38`/`BE-39` tertutup: addendum pengesahan dan tanda tangan memicu penilaian ulang lewat antrean memori, dan setiap putaran menyusul penutupan klinis yang belum punya penilaian |
| **Outcome** | Dokumen yang final belakangan tertangkap tanpa petugas menekan apa pun |
| **Trace** | `GAP-ARCH-RM-04`; `FR-RM-075`; `NFR-007` |
| **Reuse** | Pola hosted service yang sudah ada (`AccAccountingEventSchedulerHostedService`) |
| **Scope** | **Baru** `Services/MedicalRecordCompletenessRecheckHostedService.cs`; konfigurasi A4.13; pemicu dari addendum pengesahan, tanda tangan, dan perubahan profil |
| **Dependency** | `BE-39` |
| **Acceptance criteria** | 1) Kunjungan ≤ 30 hari belum lengkap dinilai ulang sesuai `NextCheckAt`. 2) Maksimal 200 per putaran. 3) Perubahan profil menilai ulang kunjungan belum `Complete`. 4) Log hanya ringkasan jumlah |
| **Verification** | `AT-RM-69`; uji layanan berkala dengan jam semu di tingkat unit |
| **Risk/owner** | Beban basis data; dipantau setelah rilis. Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji |

### `BE-41` — Swagger dan catatan rilis amandemen

| Field | Isi |
|---|---|
| **Task ID** | `BE-41` |
| **Status** | **`SELESAI`** — 2 November 2026, laporan `task/report/backend/BE-41.md`. Catatan rilis amandemen terbit di `catatan-rilis.md` bagian "Rilis amandemen `0.2.0`", **menunggu pengesahan** pemilik API (bagian A9). Keterangan Swagger dipasang pada schema DTO, karena keterangan tingkat controller tidak dimuat Swagger dan action tidak memakai `<summary>`. Temuan: `unavailableKinds` tidak pernah sampai ke selubung `/timeline` sejak `BE-21` — diperbaiki dan dikunci uji |
| **Outcome** | Konsumen tahu perubahan perilaku linimasa dan keterbatasan sementara |
| **Scope** | `catatan-rilis.md`; deskripsi Swagger pada grup — **tanpa** `<summary>` XML pada action |
| **Acceptance criteria** | Catatan rilis menyebut: jenis 14/15 tidak lagi kosong; `unavailableKinds`; keadaan `Undetermined` untuk kunjungan yang sumbernya belum tersambung; PA belum tersedia |
| **Verification** | `ReleaseContractTests` (6 uji): selubung `/timeline` memuat setiap field hasil service; keterangan schema amandemen terbit di XML dokumentasi Swagger. Seluruh project `225` lulus |
| **DoD** | Catatan terbit bersama rilis |

## R2.10 Risiko amandemen

| Risiko | Dampak | Penanganan | Pemilik |
|---|---|---|---|
| ~~Delapan pemilik modul belum menyetujui~~ | **Tertutup** 29 Oktober 2026 (`RM-DEC-067`–`075`); pembacanya selesai `BE-29`–`BE-35` | — | Pemilik modul masing-masing |
| Pembacaan berurutan lambat | Berkas lambat | Batas waktu `BE-21`; pilihan context per sumber dicatat sebagai lanjutan | Rekam medis |
| Migration butuh instruksi terpisah | Task selesai kode tetapi belum berjalan di basis data | Dicatat per task | User |
| PostgreSQL < 15 | Index `NULLS NOT DISTINCT` gagal | Index ekspresi `COALESCE` | Rekam medis |


---

# Roadmap revision 3 — Amandemen: Optik dan Rehab Medik

```yaml
module_id: RM-BP-001
blueprint_revision: 5
roadmap_revision: 3
status: APPROVED_INPUTS   # seluruh masukan disahkan; task berstatus sendiri-sendiri
owners:
  product_domain: Yoga Aji Pratama
  clinical_governance: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  api_authority: Yoga Aji Pratama
approved_by: [Yoga Aji Pratama]
approved_at: 2026-11-10        # RM-DEC-088
input_revisions:
  interview_decisions: 16        # RM-DEC-078–088
  capability_map: 4              # RM-CAP-071–082
  backend_architecture: 5 (bagian "Amandemen revision 5")
  prd_to_mvp: bagian 21–26
artifact_hashes:
  interview_decisions: sha256:83ea0cc895849f21786a517e0e56172b02f377782f806caf248d2411dbf25c58
  capability_map: sha256:73a74962185a991e87a5dadf1984e505d74f3347f612d666a084887035dbf995
  # hash artefak revision 5 lainnya: blueprint-manifest.md bagian "Revision 5 — approved"
contract_versions:
  api: 0.3.0 (approved 2026-11-10)
  state_transition: 0.3.0 (approved)
  validation: 0.3.0 (approved)
  integration: 0.3.0 (approved)
  permission_audit: 0.3.0 (approved)
source_commits:
  backend: c7686f07 (branch yoga)
  frontend: 3397211dd (branch YogaV2)
```

Bagian di atas garis ini (revision 1 dan 2) tetap berlaku dan **tidak** diubah. Bagian ini
hanya menambah lima task backend untuk gelombang `MVP-7` dan `MVP-8`
(`04-prd-to-mvp.md` bagian 26).

**Handoff implementasi.** Setiap task di bawah dijalankan lewat `/build-module-backend` dengan
task ID-nya. QBE preflight (Area, Module, prefix registry, applicability, rule ID) dan kesesuaian
dengan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` diselesaikan **pada waktu eksekusi** dari
`AGENTS.md` backend, bukan di roadmap ini. Seluruh task menyentuh prefix yang sudah `ACTIVE`
(`Mst`/`Mrc`, `Opt`, `Rhb`); tidak ada model persisted baru. Build memakai
`-p:RunAnalyzers=False`; migration hanya dibuat, **tidak** dijalankan tanpa instruksi terpisah.

## R3.1 Ringkasan task

| Task | Gelombang | Epic | Ringkas | Status |
|---|---|---|---|---|
| `BE-42` | `MVP-7` | `RM-11` | Kunci jenis poli pada profil, pemilihan profil, migration, seeder `RJ-OPT` | **`SELESAI`** 10 November 2026 — 12 uji baru, seluruh project `237` lulus; migration dibuat, belum diterapkan; laporan `task/report/backend/BE-42.md` |
| `BE-43` | `MVP-7` | `RM-10` | Addendum koreksi dokumen Optik ditolak; `isCorrectionAllowed` | **`SELESAI`** 10 November 2026 — 13 uji baru, seluruh project `250` lulus; laporan `task/report/backend/BE-43.md` |
| `BE-44` | `MVP-7` | `RM-10` | Penanda "pernah diralat" Optik menghitung addendum koreksi | **`SELESAI`** 10 November 2026 — 4 uji baru, proyek Optik `297` lulus; laporan `task/report/backend/BE-44.md` |
| `BE-45` | `MVP-8` | `RM-11` | Butir `RehabClinicalDocument`, probe dan penanda Rehab, seeder `RJ-REHAB`, uji pembaca Rehab | **`SELESAI`** 10 November 2026 — 14 uji baru, seluruh project `264` lulus; laporan `task/report/backend/BE-45.md` |
| `BE-46` | `MVP-7`/`8` | — | Catatan rilis `0.3.0` dan keterangan Swagger | **`SELESAI`** 10 November 2026 — catatan rilis `0.3.0` (menunggu pengesahan), `ReleaseContractTests` 10 lulus, seluruh project `268`; laporan `task/report/backend/BE-46.md` |

Urutan: `BE-42` dan `BE-43` paralel → `BE-44` begitu disetujui → `BE-45` sesudah `BE-42` dan
persetujuan → `BE-46` menutup.

## R3.2 Milestone B13 — Profil berpoli dan Optik (`MVP-7`)

### `BE-42` — Kunci jenis poli pada profil dokumen wajib

| Field | Isi |
|---|---|
| **Task ID** | `BE-42` |
| **Status** | **`SELESAI`** — 10 November 2026, laporan `task/report/backend/BE-42.md`. `dotnet test` → `Passed: 237`, `Failed: 0` (12 uji baru). Migration `20261110043756_AddClinicTypeToMedicalRecordCompletenessProfile` dibuat, **belum diterapkan** |
| **Outcome** | Kunjungan unit Optik dinilai dengan profil `RJ-OPT` tanpa butir; profil dapat dikunci per jenis poli, siap dipakai `RJ-REHAB` |
| **Trace** | `RM-DEC-080`, `085`; `RM-CAP-071`, `073`, `078`; `FR-RM-082`, `FR-RM-086` |
| **Kontrak** | API `0.3.0` B2; validation `VAL-RM-B02`–`B04`; state S-D tambahan; A5.6–A5.9.1 `02-backend-architecture.md` |
| **Reuse** | `MedicalRecordCompletenessProfileService`, `MedicalRecordCompletenessService.PilihProfilDasar`, seeder profil, `TandaiPenilaianUlangAsync` |
| **Scope** | `MasterData/Models/MstMedicalRecordCompletenessProfile.cs` (kolom `ClinicType`); configuration (index empat kolom, CHECK `ClinicTypeTanpaKondisi`); migration `AddClinicTypeToMedicalRecordCompletenessProfile`; DTO profil (`clinicType`, `clinicTypeName`, `clinicTypeOptions`); `MedicalRecordCompletenessProfileService` (validasi, saringan, opsi, label, kunci kembar); `MedicalRecordCompletenessProfileController` (`clinicType` pada `GET /`); `MedicalRecordCompletenessService` (proyeksi `ClinicType` kunjungan, urutan A5.9.1, `Profil`, `SidikProfil`, proyeksi daftar kerja); seeder (kunci empat kolom, `RJ-OPT`). **Tidak** menambah `RJ-REHAB` |
| **Dependency** | — |
| **Acceptance criteria** | 1) Seeder menambah `RJ-OPT`; dijalankan dua kali tetap satu. 2) Empat urutan A5.9.1 terbukti. 3) Kunjungan poli umum tetap memakai `RJ`, hasil identik dengan sebelum perubahan. 4) `VAL-RM-B02`, `B03` → `400`; `B04` → `409`. 5) Membuat profil lewat API menandai penilaian belum lengkap jatuh tempo (perilaku yang ada tetap). 6) Migration disiapkan dengan langkah mundur A5.7 |
| **Verification** | `AT-RM-75`, `76`, `79`, `80`, `81`, `82`, `89`, `90`. Uji bentrok **wajib** `TegakkanKunciUnikAsync` (harness SQLite melepas index unik terfilter). Regresi seluruh `MedicalRecordTests` |
| **Risk/owner** | Index lama harus dihapus sebelum index baru; nama index lama dibaca dari snapshot model. PostgreSQL ≥ 15 untuk `NULLS NOT DISTINCT` (devYoga 15.15). Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; laporan `task/report/backend/BE-42.md`; migration dibuat, menunggu instruksi penerapan |

### `BE-43` — Addendum koreksi dokumen Optik ditolak

| Field | Isi |
|---|---|
| **Task ID** | `BE-43` |
| **Status** | **`SELESAI`** — 10 November 2026, laporan `task/report/backend/BE-43.md`. `dotnet test` → `Passed: 250`, `Failed: 0` (13 uji baru) |
| **Outcome** | Dokumen Optik hanya dikoreksi lewat versi baru di layar Optik; dokumen Optik `LockedUnsigned` tetap dapat disahkan |
| **Trace** | `RM-DEC-079`, `084`; `RM-CAP-075`, `076`; `RM-CF-R4-01`; `FR-RM-083` |
| **Kontrak** | API `0.3.0` B3; validation `VAL-RM-B01`; state S-E; A5.9.2 |
| **Reuse** | `ClinicalNoteAddendumService.CreateAsync`/`ResolveAuthorityAsync`; pola penolakan `IntegrityGuardResult.Denied` |
| **Scope** | `MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` (himpunan `KoreksiLewatRevisi`, penolakan sebelum pemeriksaan isi, `IsCorrectionAllowed`); `MedicalRecordManagement/DTOs/ClinicalNoteAuthorDelegationDtos.cs` (`AddendumAuthorityResponse.IsCorrectionAllowed`, bawaan `true`). Controller tidak berubah |
| **Dependency** | — |
| **Acceptance criteria** | 1) `Correction` (termasuk `purpose` kosong) pada jenis 45/46 berstatus `Signed` dan `LockedUnsigned` → `400` dengan pesan kontrak, nol baris tertulis. 2) `Attestation` pada jenis 45/46 `LockedUnsigned` oleh penulis → `201`. 3) `/authority` jenis 45/46 → `isCorrectionAllowed = false`; jenis lain → `true`. 4) Addendum koreksi CPPT dan dokumen Rehab tidak berubah perilakunya |
| **Verification** | `AT-RM-91`, `92`; regresi `AT-RM-72`–`74` |
| **Risk/owner** | Perubahan perilaku pada endpoint berjalan; wajib disebut catatan rilis (`BE-46`). Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; laporan `task/report/backend/BE-43.md` |

### `BE-44` — Penanda "pernah diralat" dokumen Optik

| Field | Isi |
|---|---|
| **Task ID** | `BE-44` |
| **Status** | **`SELESAI`** — 10 November 2026, laporan `task/report/backend/BE-44.md`. Proyek uji Optik → `Passed: 297`, `Failed: 0` (4 uji baru). Persetujuan `RM-DEC-089` |
| **Outcome** | Addendum koreksi lama pada dokumen Optik tidak tersembunyi dari pembaca berkas |
| **Trace** | `RM-DEC-084`; `RM-CAP-076`; `FR-RM-084` |
| **Kontrak** | API `0.3.0` B1 (`isCorrected`); integration `INT-RM-10k`; A5.9.4 |
| **Reuse** | Pembaca Optik yang sudah ada; tabel `MrcClinicalNoteAddendum` |
| **Scope** | `OpticalManagement/Services/OptMedicalRecordDocumentSource.cs` — `IsCorrected = versi > 1` **atau** ada addendum `Correction` pada dokumen keutuhan versi mana pun milik dokumen itu (daftar dan rincian) |
| **Dependency** | Persetujuan pemilik Optik. Tidak bergantung `BE-43` |
| **Acceptance criteria** | 1) Versi 1 + satu addendum `Correction` → `isCorrected = true`. 2) Versi 1 + addendum `Attestation` saja → `false`. 3) Versi 2 tanpa addendum → `true` (perilaku yang ada). 4) Tanpa N+1: hitungan addendum satu kueri per halaman |
| **Verification** | `AT-RM-77`, `78`; uji yang ada `Tests/QuilvianSystemBackend.OpticalTests/Be013MedicalRecordTests.cs` tetap lulus |
| **Risk/owner** | Rendah. Pemilik: Optik (folder), rekam medis (aturan) |
| **DoD** | Kriteria terbukti uji; laporan `task/report/backend/BE-44.md` |

## R3.3 Milestone B14 — Poli Rehabilitasi Medik (`MVP-8`)

### `BE-45` — Butir dokumen rehab dan probe Rehab

| Field | Isi |
|---|---|
| **Task ID** | `BE-45` |
| **Status** | **`SELESAI`** — 10 November 2026, laporan `task/report/backend/BE-45.md`. `dotnet test` → `Passed: 264`, `Failed: 0` (14 uji baru). Persetujuan `RM-DEC-090` |
| **Outcome** | Kunjungan poli rehab dinilai dengan profil `RJ-REHAB`: lengkap bila ada asesmen, sesi, reasesmen, atau ringkasan akhir final; tidak pernah `Menunggu hasil` |
| **Trace** | `RM-DEC-081`, `082`, `086`, `087`; `DS-R5-04`; `RM-CAP-074`, `079`, `081`; `FR-RM-087`–`089` |
| **Kontrak** | API `0.3.0` B2 (opsi kriteria 17); integration `INT-RM-10l`, `INT-RM-11c`; A5.9.3, A5.9.4 |
| **Reuse** | Pola probe `HmdMedicalRecordDocumentSource`; pendaftaran instance ganda `Program.cs:735-736`; harness `MedicalRecordTests` |
| **Scope** | `MedicalRecordManagement/Enums/CompletenessCriterion.cs` (`RehabClinicalDocument = 17`) beserta label/keterangan di `MedicalRecordCompletenessProfileService`; `RehabilitationManagement/Services/RhbMedicalRecordDocumentSource.cs` (probe; penanda menghitung addendum `Correction` saja); `Program.cs` (daftarkan probe); seeder (`RJ-REHAB`); **uji baru** pembaca dan probe Rehab |
| **Dependency** | `BE-42`; persetujuan pemilik Rehab Medik |
| **Acceptance criteria** | 1) Asesmen, sesi, reasesmen, atau ringkasan akhir final pada kunjungan → butir terpenuhi. 2) Program saja, edukasi saja, draf, batal, atau dokumen kunjungan lain → tidak terpenuhi. 3) `PendingResults` selalu kosong. 4) Probe galat/lewat batas waktu → `Undetermined`. 5) Dokumen rehab yang hanya disahkan → `isCorrected = false`; dengan addendum koreksi → `true`. 6) `RJ-REHAB` terpasang lewat seeder bersama probe, idempoten. 7) Kunjungan poli rehab tanpa poli `Rehabilitation` di master tetap memakai `RJ` |
| **Verification** | `AT-RM-83`–`88`; regresi `MedicalRecordTests`. Berkas uji baru di `Tests/` (diabaikan git) perlu `git add -f` |
| **Risk/owner** | **Jangan** memasang `RJ-REHAB` tanpa probe: seluruh kunjungan poli rehab menjadi belum lengkap (`RM-CAP-074`). Pemilik: Rehab Medik (folder), rekam medis (aturan) |
| **DoD** | Kriteria terbukti uji; laporan `task/report/backend/BE-45.md` |

## R3.4 Penutup

### `BE-46` — Catatan rilis `0.3.0` dan keterangan Swagger

| Field | Isi |
|---|---|
| **Task ID** | `BE-46` |
| **Status** | **`SELESAI`** — 10 November 2026, laporan `task/report/backend/BE-46.md`. Catatan rilis `0.3.0` **disahkan** 11 November 2026 (`RM-DEC-110`) |
| **Outcome** | Konsumen API tahu jenis 45–52, kolom `clinicType`, butir 17, dan penolakan addendum koreksi Optik |
| **Trace** | `RM-DEC-088`; `RM-CAP-082` |
| **Kontrak** | API `0.3.0` seluruhnya |
| **Reuse** | Pola `BE-41` (`ReleaseContractTests`) |
| **Scope** | `docs/module-blueprints/rekam-medis/catatan-rilis.md` bagian rilis `0.3.0`; keterangan `<summary>` pada **properti DTO** baru (bukan pada action controller) |
| **Dependency** | `BE-42`, `BE-43`; `BE-44`, `BE-45` untuk bagian masing-masing |
| **Acceptance criteria** | 1) Catatan rilis menyebut perubahan perilaku `VAL-RM-B01` dan urutan penerapan migration sesudah migration amandemen revision 4. 2) Properti `clinicType`, `isCorrectionAllowed` berketerangan di Swagger. 3) Tidak ada `<summary>` baru pada action controller |
| **Verification** | `ReleaseContractTests` diperluas; tinjauan Swagger |
| **Risk/owner** | Rendah. Pemilik: API owner |
| **DoD** | Catatan rilis disahkan pemilik API (`RM-DEC-110`) |

## R3.5 Risiko amandemen revision 5

| Risiko | Dampak | Penanganan | Pemilik |
|---|---|---|---|
| Persetujuan pemilik Rehab Medik tidak turun | `MVP-8` tertahan; kunjungan poli rehab tetap dinilai dengan `RJ` (keadaan sekarang) | `MVP-7` tetap jalan; `RJ-REHAB` tidak dipasang | Sukma Giri Pratama |
| Poli rehab belum ada di master data (`RM-UN-R4-01`) | `RJ-REHAB` tidak pernah terpilih | Cek data sebelum `UAT-23` | Admin data |
| Migration revision 4 belum diterapkan di luar devYoga | Migration `BE-42` gagal di lingkungan itu | Terapkan berurutan; dicatat `BE-46` | User |
| Index lama bernama lain dari dugaan | Migration gagal | Nama dibaca dari snapshot model saat dibuat | Rekam medis |

---

# Roadmap revision 4 — Amandemen: indikator ringkasan pulang dan titik sentuh HR

```yaml
module_id: RM-BP-001
blueprint_revision: 6
roadmap_revision: 4
status: APPROVED_INPUTS   # seluruh masukan disahkan; task berstatus sendiri-sendiri
owners:
  product_domain: Yoga Aji Pratama
  clinical_governance: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  api_authority: Yoga Aji Pratama
approved_by: [Yoga Aji Pratama]
approved_at: 2026-11-11        # RM-DEC-108
input_revisions:
  interview_decisions: 21        # RM-DEC-097–108
  capability_map: 5              # RM-CAP-083–095
  backend_architecture: 6 (bagian "Amandemen revision 6", A6.1–A6.13)
  prd_to_mvp: bagian 27–32
artifact_hashes:
  interview_decisions: sha256:1f9aa1155d02b30c895c74fa86fca6254d406cdaae5c24a3f3cd3c119b9c37f6
  capability_map: sha256:c69ed2f603f8a32e4d5b78190c77b974b5ef67802ffd461c3fb8452fed7e5b26
  backend_architecture: sha256:8dbf6eca20363b40b92d9191bc85b08075df1f4547493216aa4482c53d9e7315
  # hash artefak revision 6 lainnya: blueprint-manifest.md bagian "Revision 6 — approved"
contract_versions:
  api: 0.4.0 (approved 2026-11-11)
  state_transition: 0.4.0 (approved)
  validation: 0.4.0 (approved)
  integration: 0.4.0 (approved)
  permission_audit: 0.4.0 (approved)
source_commits:
  backend: 46a57def (branch yoga)
  frontend: 75b97ecad (branch YogaV2)
```

Bagian di atas garis ini (revision 1–3) tetap berlaku dan **tidak** diubah. Bagian ini menambah lima
task backend untuk gelombang `MVP-9` dan `MVP-10` (`04-prd-to-mvp.md` bagian 32).

**Handoff implementasi.** Setiap task dijalankan lewat `/build-module-backend` dengan task ID-nya.
QBE preflight (Area, Module, prefix registry, applicability, rule ID) dan kesesuaian dengan
`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` diselesaikan **pada waktu eksekusi** dari
`AGENTS.md` backend, bukan di roadmap ini. **Tidak ada model persisted baru dan tidak ada
migration**; seluruh task menyentuh folder dengan prefix yang sudah `ACTIVE` (`Mrc`, `Inp`). Build
memakai `-p:RunAnalyzers=False`.

## R4.1 Ringkasan task

| Task | Gelombang | Epic | Ringkas | Folder | Status |
|---|---|---|---|---|---|
| `BE-47` | `MVP-9` | `RM-13` | Bagian "Rencana kontrol" pada detail ringkasan pulang | Rawat Inap | **`SELESAI`** 11 November 2026 — 1 uji baru, seluruh project `270` lulus; laporan `task/report/backend/BE-47.md` |
| `BE-48` | `MVP-9` | `RM-13` | Penolakan tanda tangan ulang ringkasan pulang (`409`) | Rawat Inap | **`SELESAI`** 11 November 2026 — 5 uji baru, seluruh project `275` lulus; laporan `task/report/backend/BE-48.md` |
| `BE-49` | `MVP-10` | `RM-12` | Kontrak pembaca, service pengelompokan, DTO, controller, izin, audit, uji arsitektur | Rekam medis | **`SELESAI`** 11 November 2026 — 16 uji baru, seluruh project `291` lulus; `403` HTTP `NOT RUN`; laporan `task/report/backend/BE-49.md` |
| `BE-50` | `MVP-10` | `RM-12` | Pembaca `InpDischargeSummaryTimelinessSource` dan pendaftarannya | Rawat Inap | **`SELESAI`** 11 November 2026 — 6 uji baru, seluruh project `297` lulus; laporan `task/report/backend/BE-50.md` |
| `BE-51` | `MVP-9`/`10` | — | Catatan rilis `0.4.0` dan keterangan Swagger | Rekam medis | **`SELESAI`** 11 November 2026 — 11 uji kontrak baru, seluruh project `308` lulus; catatan rilis disahkan (`RM-DEC-110`); laporan `task/report/backend/BE-51.md` |

Urutan: `BE-49` dapat dimulai sekarang → begitu persetujuan turun, `BE-48` lebih dulu (supaya
`resignTrackingNote` hanya berlaku untuk data lama), lalu `BE-47` dan `BE-50` paralel → `BE-51`
menutup.

**Pekerjaan yang tetap berjalan tanpa persetujuan Rawat Inap:** `BE-49` seluruhnya, termasuk uji
pengelompokan dengan pembaca tiruan, dan bagian `BE-51` untuk endpoint indikator.

## R4.2 Milestone B15 — Ringkasan pulang di Rawat Inap dan berkas (`MVP-9`)

### `BE-47` — Rencana kontrol di detail berkas

| Field | Isi |
|---|---|
| **Task ID** | `BE-47` |
| **Status** | **`SELESAI`** — 11 November 2026, laporan `task/report/backend/BE-47.md`. `dotnet test` → `Passed: 270`, `Failed: 0` (1 uji baru, `AT-RM-105`/`106`). Persetujuan pemilik Rawat Inap `RM-DEC-109` (disampaikan Yoga Aji Pratama) |
| **Outcome** | Pembaca berkas melihat tanggal, poli, dan dokter kontrol yang ditulis DPJP di ringkasan pulang |
| **Trace** | `RM-DEC-104`; `DS-R6-06`; `RM-CAP-090`; `RM-FACT-045`; `FR-RM-097` |
| **Kontrak** | API `0.4.0` C2; integration `INT-RM-10m`; A6.9.4 |
| **Reuse** | `InpMedicalRecordDocumentSource.ReadDetailAsync`, helper `Tambah`, format `TulisWaktu`; navigasi `FollowUpServiceUnit`, `FollowUpDoctor` yang sudah ada |
| **Scope** | `Areas/HealthServices/InPatientManagement/Services/InpMedicalRecordDocumentSource.cs` — proyeksi detail `DischargeSummary` menambah `FollowUpDate`, `FollowUpServiceUnit.ServiceUnitName`, `FollowUpDoctor.FullName`; bagian "Rencana kontrol" sesudah "Instruksi tindak lanjut". Daftar linimasa **tidak** berubah. Uji: perluas `Tests/QuilvianSystemBackend.MedicalRecordTests/InpMedicalRecordDocumentSourceTests.cs` |
| **Dependency** | Persetujuan Muhammad Hamzah — **terpenuhi** (`RM-DEC-109`) |
| **Acceptance criteria** | 1) Tanggal + poli + dokter → `"10-10-2026 — Poli Penyakit Dalam — dr. Maya"`. 2) Tanggal + poli → tanpa bagian dokter. 3) Tanpa tanggal → bagian tidak ada. 4) Status kelengkapan kunjungan tidak berubah. 5) Bagian lain dan urutannya tidak berubah |
| **Verification** | `AT-RM-105`, `106`; regresi `MedicalRecordTests` |
| **Risk/owner** | Rendah. Pemilik: Rawat Inap (folder), rekam medis (aturan tampilan) |
| **DoD** | Kriteria terbukti uji; persetujuan tercatat sebagai `RM-DEC` baru; laporan `task/report/backend/BE-47.md` |

### `BE-48` — Penolakan tanda tangan ulang ringkasan pulang

| Field | Isi |
|---|---|
| **Task ID** | `BE-48` |
| **Status** | **`SELESAI`** — 11 November 2026, laporan `task/report/backend/BE-48.md`. `dotnet test` → `Passed: 275`, `Failed: 0` (5 uji baru di `InpDischargeSignGuardTests.cs`). Persetujuan `RM-DEC-109` |
| **Outcome** | Waktu tanda tangan pertama ringkasan pulang tidak lagi dapat tertimpa tanpa jejak |
| **Trace** | `RM-DEC-105` bagian 1; `RM-CF-R5-01`; `RM-CAP-085`; `FR-RM-098` |
| **Kontrak** | API `0.4.0` C3; validation `VAL-INP-RS01`; state S-F; integration `INT-RM-19`; A6.9.3 |
| **Reuse** | `GetOpenCorrectionSessionAsync`, `InpDischargeSummaryOperationResult.Conflict`, `FromSummaryFailure` |
| **Scope** | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` — `SignSummaryAsync`: sesudah ringkasan ditemukan dan **sebelum** `GUARD-INP-03`, bila `SignedAt != null` dan tidak ada sesi koreksi terbuka → `Conflict` "Resume sudah ditandatangani; ubah lewat sesi koreksi.". Controller tidak berubah. Uji di `Tests/QuilvianSystemBackend.MedicalRecordTests/` (preseden `ClosureSourcePerPathTests.cs`) kecuali pemilik Rawat Inap menetapkan lokasi lain |
| **Dependency** | Persetujuan Muhammad Hamzah — **terpenuhi** (`RM-DEC-109`) |
| **Acceptance criteria** | 1) Sudah bertanda tangan, tanpa sesi koreksi → `409`, `SignedAt`/`SignedByDoctorId` tidak berubah. 2) Sudah bertanda tangan, sesi koreksi terbuka → diterima seperti sekarang. 3) Belum bertanda tangan → seperti sekarang. 4) Dokter bukan DPJP pada ringkasan yang sudah final menerima `409`, bukan `403`. 5) Penutupan episode dan jalur supervisor tidak berubah |
| **Verification** | `AT-RM-107`; regresi `ClosureSourcePerPathTests` dan seluruh `MedicalRecordTests` |
| **Risk/owner** | **Perubahan perilaku** pada endpoint Rawat Inap yang berjalan: layar Rawat Inap yang menekan tanda tangan dua kali akan menerima `409`. Wajib disebut catatan rilis (`BE-51`) dan diberitahukan ke pemilik frontend Rawat Inap. Pemilik: Rawat Inap |
| **DoD** | Kriteria terbukti uji; persetujuan tercatat; laporan `task/report/backend/BE-48.md` |

## R4.3 Milestone B16 — Indikator ketepatan ringkasan pulang (`MVP-10`)

### `BE-49` — Service, endpoint, izin, dan audit indikator

| Field | Isi |
|---|---|
| **Task ID** | `BE-49` |
| **Status** | **`SELESAI`** — 11 November 2026, laporan `task/report/backend/BE-49.md`. `dotnet test` → `Passed: 291`, `Failed: 0` (16 uji baru di `DischargeSummaryTimelinessTests.cs`). `403` lewat HTTP penuh terbukti di UAT `MVP-10` (`testing/uat-amandemen-0.4.0.md`). Endpoint menjawab `503` sampai `BE-50` (kini terpasang) |
| **Outcome** | Pemegang izin dapat menarik angka ketepatan ringkasan pulang per dokter per periode tanpa identitas pasien |
| **Trace** | `RM-DEC-101`, `102`, `105`–`107`; `DS-R6-01`–`05`, `07`; `RM-CAP-086`–`089`, `091`, `092`, `093`; `FR-RM-091`–`096` |
| **Kontrak** | API `0.4.0` C1; validation `VAL-RM-C01`–`C04`; permission/audit `0.4.0`; integration `INT-RM-20`, `INT-RM-N01`–`N04`, "Kegagalan"; A6.4, A6.9.2, A6.9.5 |
| **Reuse** | Pola `MedicalRecordCompletenessController` (`filters/metadata` sesudah konstruktor, `ValidateQuery`, `ApiResponse`, paging); `LoggerService.AuditAsync` (isi di teks pesan); `MstDoctor.WorkforceProfileId` |
| **Scope** | **Baru:** `MedicalRecordManagement/Services/IDischargeSummaryTimelinessSource.cs` (kontrak baris A6.9.1); `MedicalRecordManagement/Services/DischargeSummaryTimelinessService.cs`; `MedicalRecordManagement/DTOs/DischargeSummaryTimelinessDtos.cs`; `MedicalRecordManagement/Controllers/DischargeSummaryTimelinessController.cs` (`[AccessPermission("DischargeSummaryTimeliness", "Read")]`). **Diperbarui:** `Program.cs` (service). Service menerima `IEnumerable<IDischargeSummaryTimelinessSource>`; **bila belum ada pembaca terdaftar**, `GET /` mengembalikan `503` sesuai kontrak "Kegagalan", sehingga task ini aman dipasang sebelum `BE-50`. `waktuTarik` diambil lewat `TimeProvider` (atau penyuntikan setara) agar uji tidak bergantung jam nyata. **Uji baru:** pengelompokan dengan pembaca tiruan; controller; uji arsitektur A6.9.5 |
| **Dependency** | — |
| **Acceptance criteria** | 1) Seluruh baris A6.9.2 termasuk batas tepat 24:00:00 dan 48:00:00. 2) `notYetDueCount` tidak masuk `assessedCount`. 3) Item "Tanpa DPJP aktif" (`doctorId = null`) selalu terakhir. 4) JSON mentah tidak memuat `patientId`, `encounterId`, `episodeId`, nomor RM. 5) `VAL-RM-C01`–`C04` → `400` berpesan kontrak; `doctorId` tak dikenal → hasil kosong, bukan `400`/`404`. 6) Tanpa izin, atau hanya `MedicalRecordCompleteness : Read` → `403`. 7) Satu baris audit per tarikan berhasil (periode, saringan, jumlah item, `generatedAt` di teks pesan); `503` tanpa audit dan tanpa angka sebagian. 8) `dataNotes` lengkap. 9) `MedicalRecordManagement` tidak merujuk `IClinicalPrivilegeGateService`, `TrxClinicalPrivilegeEmergencyOverrideLog`, maupun tipe `OccupationalHealthManagement` |
| **Verification** | `AT-RM-93`, `94`, `97`, `98`, `102`, `103`, `104` (bagian validasi), `108`; uji arsitektur; regresi `MedicalRecordTests`. `AT-RM-95`, `96`, `99`–`101` dibuktikan bersama `BE-50` |
| **Risk/owner** | Izin baru terdaftar otomatis tetapi **tidak** boleh dipasangkan ke jabatan oleh seeder (`DS-R6-05`). Pemilik: rekam medis |
| **DoD** | Kriteria terbukti uji; laporan `task/report/backend/BE-49.md`; berkas uji baru di `Tests/` di-`git add -f` oleh user |

### `BE-50` — Pembaca indikator di Rawat Inap

| Field | Isi |
|---|---|
| **Task ID** | `BE-50` |
| **Status** | **`SELESAI`** — 11 November 2026, laporan `task/report/backend/BE-50.md`. `dotnet test` → `Passed: 297`, `Failed: 0` (6 uji baru di `InpDischargeSummaryTimelinessSourceTests.cs`). Endpoint indikator kini menjawab angka nyata. Persetujuan `RM-DEC-109` |
| **Outcome** | Indikator membaca data Rawat Inap nyata lewat pemiliknya, tanpa salinan dan tanpa identitas pasien |
| **Trace** | `RM-DEC-102`, `105`–`107`; `DS-R6-04`; `RM-CAP-083`–`086`; `FR-RM-093`–`095` |
| **Kontrak** | Integration `INT-RM-18`; state S-F; A6.9.1 |
| **Reuse** | Pola pembaca `InpMedicalRecordDocumentSource` (`AsNoTracking`, batas waktu A4.14); model `InpEpisode`, `InpDischargeSummary`, `InpDischargeSummaryRevision`, `InpDoctorAssignment` |
| **Scope** | **Baru:** `Areas/HealthServices/InPatientManagement/Services/InpDischargeSummaryTimelinessSource.cs`. **Diperbarui:** `Program.cs` (dua pendaftaran, pola baris 728–729). Satu kueri baca per permintaan; rentang WIB dikonversi ke UTC sebelum kueri |
| **Dependency** | `BE-49` (kontrak pembaca); persetujuan Muhammad Hamzah — **terpenuhi** (`RM-DEC-109`). Disarankan sesudah `BE-48` |
| **Acceptance criteria** | 1) Jam mulai `PhysicallyLeftAt ?? ClosedAt` dengan `StartSource` benar. 2) Episode `Cancelled`, dihapus, atau `DischargePending` tanpa kepergian tidak terbaca. 3) Tanda tangan pertama = nilai terkecil `PreviousSignedAt` versi (tidak dihapus) dan `SignedAt`; `Guid.Empty` pada versi dianggap kosong. 4) DPJP aktif diambil hanya untuk yang belum ditandatangani. 5) Batas periode: `PeriodFrom 00:00 WIB` inklusif, `PeriodTo + 1 hari 00:00 WIB` eksklusif. 6) Tidak ada kolom pasien di baris |
| **Verification** | `AT-RM-95`, `96`, `99`, `100`, `101`; uji ujung-ke-ujung controller dengan pembaca nyata pada harness. Harness SQLite menyimpan tanggal sebagai teks — rentang dibandingkan dalam UTC yang sudah dikonversi |
| **Risk/owner** | Kinerja kueri rentang pada data nyata belum diukur; bila lambat, index diajukan ke pemilik Rawat Inap lewat task terpisah (A6.6), bukan diam-diam. Pemilik: Rawat Inap (folder), rekam medis (aturan) |
| **DoD** | Kriteria terbukti uji; persetujuan tercatat; laporan `task/report/backend/BE-50.md` |

## R4.4 Penutup

### `BE-51` — Catatan rilis `0.4.0` dan keterangan Swagger

| Field | Isi |
|---|---|
| **Task ID** | `BE-51` |
| **Status** | **`SELESAI`** — 11 November 2026, laporan `task/report/backend/BE-51.md`. `ReleaseContractTests` 21 lulus; seluruh project `308` lulus. Catatan rilis `0.4.0` **disahkan** 11 November 2026 (`RM-DEC-110`). Selisih contoh paging kontrak C1 dicatat sebagai penyegaran `0.4.1` |
| **Outcome** | Pemakai API (termasuk HR) tahu endpoint indikator, izinnya, batas datanya, dan perubahan perilaku tanda tangan ulang |
| **Trace** | `RM-DEC-108`; `RM-DEC-095` (penyegaran `0.3.1` yang dilebur) |
| **Kontrak** | API `0.4.0` seluruhnya |
| **Reuse** | Pola `BE-41`/`BE-46` (`ReleaseContractTests`) |
| **Scope** | `docs/module-blueprints/rekam-medis/catatan-rilis.md` bagian rilis `0.4.0`; keterangan `<summary>` pada **properti DTO** baru (bukan pada action controller) |
| **Dependency** | `BE-49`; `BE-47`, `48`, `50` untuk bagiannya |
| **Acceptance criteria** | 1) Catatan rilis menyebut: endpoint baru dan izinnya tidak terpasang otomatis; `resignTrackingNote`; perubahan perilaku `409` (bila `BE-48` dipasang); penyegaran `clinicType = 0`. 2) Properti DTO indikator berketerangan di Swagger. 3) Tidak ada `<summary>` baru pada action controller |
| **Verification** | `ReleaseContractTests` diperluas; tinjauan Swagger |
| **Risk/owner** | Rendah. Pemilik: API owner |
| **DoD** | Catatan rilis disahkan pemilik API (`RM-DEC-110`); laporan `task/report/backend/BE-51.md` |

## R4.5 Risiko amandemen revision 6

| Risiko | Dampak | Penanganan | Pemilik |
|---|---|---|---|
| Persetujuan pemilik Rawat Inap tidak turun | `BE-47`, `48`, `50` tertahan; endpoint indikator menjawab `503` | `BE-49` tetap dipasang dan aman; catat di catatan rilis | Muhammad Hamzah |
| `BE-48` ditolak | Tanda tangan pertama tetap dapat tertimpa | Indikator berjalan dengan `resignTrackingNote` (`RM-DEC-105` bagian 3) | Muhammad Hamzah |
| HR membangun `IOppeSimrsMetricExtractorService` membaca tabel klinis langsung | Dua definisi "tepat waktu" | Open question 28; kirim kontrak `INT-RM-20` ke pemilik HR | Pemilik HR |
| Izin indikator terpasang ke jabatan yang salah | Kinerja dokter terlihat pihak yang tidak berwenang | `DS-R6-05`: tidak dipasang otomatis; pemasangan oleh admin sesudah HR menerima | Yoga Aji Pratama |
| Kueri rentang lambat pada data produksi | Tarikan lewat batas waktu → `503` | Ukur saat UAT; index lewat task terpisah | Rawat Inap |
