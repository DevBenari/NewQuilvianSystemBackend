# Rekam Medis — Backend Delivery Roadmap

```yaml
module_id: RM-BP-001
roadmap_revision: 1
status: DRAFT
owners:
  product_domain: OPEN
  clinical_governance: OPEN
  security_privacy: OPEN
  api_authority: OPEN
approved_by: []
input_revisions:
  interview_decisions: 4
  capability_map: 2
  backend_architecture: 1
artifact_hashes:
  interview_decisions: sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c
  capability_map: sha256:9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae
  backend_architecture: sha256:32ab3711e9203bedf2838cdadbbeb1ab6400c20d49b1b1497eaed9efaa5243a1
contract_versions:
  api: 0.1.0 (draft)
  state_transition: 0.1.0 (draft)
  validation: 0.1.0 (draft)
  integration: 0.1.0 (draft)
  permission_audit: 0.1.0 (draft)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

> **PERINGATAN GERBANG PERENCANAAN.** Skill perencanaan mensyaratkan blueprint dan kontrak
> berstatus `APPROVED`. Seluruh keputusan dan kontrak modul ini masih `draft`, atas pilihan
> sadar yang tercatat pada `RM-DEC-025`. Roadmap ini karena itu **bukan izin mulai bekerja**.
> Setiap task diberi status keterlaksanaan pada bagian 2, dan hanya satu task yang benar-benar
> dapat dimulai hari ini.

---

## 1. Cara membaca roadmap ini

### Status keterlaksanaan

| Status | Arti |
|---|---|
| `SIAP` | Tidak bergantung pada keputusan atau kontrak berstatus draft. Dapat dimulai hari ini |
| `TERTAHAN APPROVAL` | Rancangannya lengkap, tetapi bergantung pada keputusan yang belum disahkan owner |
| `TERTAHAN BLOCKER` | Tertahan hal tertentu yang disebut namanya, di luar soal approval |

Perbedaan dua status terakhir penting. `TERTAHAN APPROVAL` hilang begitu owner ditunjuk dan
menyetujui. `TERTAHAN BLOCKER` menuntut pekerjaan nyata lebih dulu, misalnya menetapkan angka
masa simpan atau menelusuri data.

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

| Milestone | Task | Status | Tertahan oleh |
|---|---|---|---|
| B0 | `BE-00` | **`SIAP`** | — |
| B0 | `BE-01`, `BE-02` | `TERTAHAN APPROVAL` | `RM-DEC-013` |
| B1 | `BE-03` … `BE-08` | `TERTAHAN APPROVAL` | `RM-DEC-003`, `004`, `014`, `019`, `020`, `021` |
| B2 | `BE-09`, `BE-11`, `BE-12` | `TERTAHAN APPROVAL` | `RM-DEC-005`, `015`, `016`, `017` |
| B2 | `BE-10` | `TERTAHAN APPROVAL` | `RM-DEC-024` **tertutup** 24 Agustus 2026 (25 tahun). Kini hanya menunggu approval owner |
| B3 | `BE-13` … `BE-15` | `TERTAHAN APPROVAL` | `RM-DEC-002`, `022` |
| B3 | `BE-16` | `TERTAHAN APPROVAL` — **prioritas rendah** | Ditetapkan sebagai pengaman pada `RM-DEC-026`. Dikerjakan paling akhir di milestone B3 |
| B4 | `BE-17`, `BE-18` | `TERTAHAN APPROVAL` | Seluruh task pendahulunya |

**Denominator: 19 task. Satu berstatus `SIAP`, tujuh belas `TERTAHAN APPROVAL`, satu
`TERTAHAN BLOCKER`.**

Angka ini adalah jawaban paling berguna dari roadmap ini: hampir seluruh pekerjaan menunggu
penunjukan tiga owner. Satu-satunya yang dapat berjalan sekarang, `BE-00`, kebetulan juga
prasyarat bagi tiga perbaikan paling berisiko.

---

## 3. Milestone B0 — Fondasi

### `BE-00` — Membuat project test backend

| Field | Isi |
|---|---|
| **Task ID** | `BE-00` |
| **Status** | **`SIAP`** — dapat dimulai hari ini |
| **Outcome** | Tim punya cara membuktikan bahwa perubahan pada catatan klinis tidak merusak alur IGD dan antrean dokter. Sebelum ini, satu-satunya cara adalah mencoba manual dan berharap tidak ada yang terlewat |
| **Trace** | `RM-CAP-032`; open question nomor 11 pada decision log |
| **Reuse** | Tidak ada. Backend belum memiliki project test apa pun |
| **Scope** | Project test baru pada solution; penyiapan basis data uji; contoh uji untuk satu controller yang sudah ada |
| **Dependency** | **Tidak ada.** Tidak bergantung pada satu pun keputusan modul rekam medis |
| **Acceptance criteria** | 1) Perintah uji dapat dijalankan dan melaporkan hasil. 2) Sekurang-kurangnya satu uji integrasi menyentuh basis data dan lulus. 3) Uji dapat dijalankan berulang tanpa saling mengganggu |
| **Verification** | Keluaran perintah uji yang menunjukkan jumlah uji lulus |
| **Risk/blocker** | Risiko: pilihan kerangka uji dan cara menyiapkan basis data uji belum pernah ditetapkan project ini. Owner: pemilik arsitektur backend, **`OPEN`** |
| **DoD** | Project test ada di solution, terdokumentasi cara menjalankannya, dan satu uji contoh lulus |

Catatan mengapa task ini didahulukan meski bukan bagian modul rekam medis. Tiga perbaikan pada
`BE-03` menyentuh `PatientIntegratedProgressNoteController`, berkas sepanjang 1.407 baris yang
dipakai alur antrean dokter dan IGD. Mengubahnya tanpa jaring pengaman otomatis adalah risiko
yang dapat dihindari dengan pekerjaan yang tidak menunggu approval siapa pun.

### `BE-01` — Model, enum, dan configuration keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-01` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-013` |
| **Outcome** | Sistem punya tempat menyimpan keterangan keutuhan dokumen, terpisah dari isi klinisnya |
| **Trace** | `RM-DEC-013`; ERD `keutuhan-dokumen.md`; kamus data bagian 1 sampai 3 dan 6 |
| **Reuse** | `IdentityModel`; pola configuration `Repositories/Configurations/HealthService/`; `ApplyConfigurationsFromAssembly` pada `ApplicationDbContext.cs:612` |
| **Scope** | `Areas/HealthServices/MedicalRecordManagement/Models/` tiga model; `Enums/` enam enum; `Repositories/Configurations/HealthService/MedicalRecordManagement/` tiga configuration; migration `AddMedicalRecordIntegrityTables` |
| **Dependency** | `BE-00` disarankan lebih dulu |
| **Acceptance criteria** | 1) Migration berjalan dan mundur tanpa galat. 2) Index unik `(DocumentKind, DocumentId)` menolak baris kembar. 3) Index unik `(IntegrityId, Sequence)` menolak addendum berurutan kembar. 4) Seluruh relasi memakai `DeleteBehavior.Restrict` |
| **Verification** | Uji integrasi yang mencoba menyisipkan baris kembar dan memastikannya ditolak |
| **Risk/blocker** | Risiko: rujukan polimorfik tidak dapat dijamin foreign key. Diterima sadar, ditutup di service pada `BE-02` |
| **DoD** | Migration teruji maju dan mundur; ketiga tabel terbentuk sesuai kamus data; uji constraint lulus |

### `BE-02` — Service keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-02` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-003`, `RM-DEC-013` |
| **Outcome** | Ada satu tempat yang memutuskan apakah sebuah dokumen masih boleh diubah. Tidak tersebar di banyak controller |
| **Trace** | `RM-DEC-003`; arsitektur bagian 5.6; state transition matrix bagian 2 dan 3 |
| **Reuse** | Pola service tanpa interface, didaftarkan `AddScoped`, mencontoh `DoctorConsultationLifecycleService` |
| **Scope** | `Services/ClinicalDocumentIntegrityService.cs`; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-01` |
| **Acceptance criteria** | 1) `RegisterAsync` menolak pendaftaran kedua untuk dokumen yang sama. 2) `SignAsync` menolak bila pemanggil bukan `AuthorUserId`. 3) `SignAsync` menolak bila status bukan `Draft`. 4) `EnsureMutableAsync` menolak dokumen `Signed`, `LockedUnsigned`, dan `Cancelled`. 5) `AuthorUserId` tidak dapat diubah lewat jalur mana pun |
| **Verification** | `AT-RM-02`, `AT-RM-10`, `AT-RM-11` |
| **Risk/blocker** | Risiko: aturan hanya berlaku bila service dipanggil. Ditutup dengan membatasi cakupan ke satu jenis dokumen, arsitektur bagian 7 |
| **DoD** | Seluruh acceptance criteria terbukti uji; service terdaftar di `Program.cs` |

---

## 4. Milestone B1 — Slice minimum: CPPT terkunci dan dapat dikoreksi

Ini vertical slice pertama yang menghasilkan sesuatu yang dapat diverifikasi pemilik proses:
**catatan CPPT yang sudah ditandatangani tidak dapat diubah diam-diam, dan koreksinya
meninggalkan jejak.**

### `BE-03` — Menutup tiga celah pada CPPT

| Field | Isi |
|---|---|
| **Task ID** | `BE-03` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-019` |
| **Outcome** | Catatan CPPT tidak lagi dapat diubah setelah ditandatangani, penulisnya tidak dapat dipindahkan ke orang lain, dan penanda read-only tidak dapat dilepas dari luar |
| **Trace** | `RM-DEC-019`; `RM-CAP-011`, `RM-CAP-012`, `RM-CAP-013`; api-contract bagian 8 |
| **Reuse** | `PatientIntegratedProgressNoteController` yang sudah ada; hanya perilakunya berubah |
| **Scope** | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`. Tiga perubahan: memanggil `EnsureMutableAsync` sebelum mengubah; **menghapus** penetapan `entity.ProviderUserId` dari permintaan pada baris 533; **menghapus** penetapan `entity.IsReadOnlyGenerated` dari permintaan pada baris 550. Tambahan: memanggil `RegisterAsync` saat CPPT dibuat |
| **Dependency** | `BE-00`, `BE-02` |
| **Acceptance criteria** | 1) Mengubah CPPT terkunci ditolak `400` dengan pesan mengarahkan ke addendum. 2) Mengirim `ProviderUserId` orang lain tidak mengubah apa pun. 3) Mengirim `IsReadOnlyGenerated` tidak mengubah apa pun. 4) Membuat CPPT menghasilkan baris keutuhan `Draft`. 5) Bila pendaftaran keutuhan gagal, pembuatan CPPT ikut dibatalkan |
| **Verification** | `AT-RM-01`, `AT-RM-19`, `AT-RM-20`, `AT-RM-24`, `AT-RM-35`; ditambah `AT-RM-34` sebagai uji regresi alur antrean dokter |
| **Risk/blocker** | **Risiko tertinggi di seluruh roadmap.** Menyentuh berkas 1.407 baris yang dipakai IGD dan antrean dokter. Ditutup oleh `BE-00`. Owner: `OPEN` |
| **DoD** | Ketiga celah tertutup dan terbukti uji; alur antrean dokter berjalan penuh tanpa regresi; perubahan perilaku tercatat pada catatan rilis |

### `BE-04` — Menandatangani dan mengunci dokumen

| Field | Isi |
|---|---|
| **Task ID** | `BE-04` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-003`, `RM-DEC-021` |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-020` |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-004` |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-003` lapis kedua |
| **Outcome** | Tidak ada catatan yang tertinggal terbuka setelah kunjungan pasien selesai |
| **Trace** | `RM-DEC-003`; integration contract bagian 2.2 |
| **Reuse** | `PatientEncounterController` yang sudah ada |
| **Scope** | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`, endpoint `PATCH /{id}/status`; penambahan `LockOpenDocumentsForEncounterAsync` pada `BE-02` |
| **Dependency** | `BE-02`, `BE-03` |
| **Acceptance criteria** | 1) Perpindahan menuju `Completed` mengunci seluruh dokumen `Draft` pada kunjungan itu. 2) Penguncian dan perubahan status berada dalam satu transaksi. 3) Bila penguncian gagal, penutupan kunjungan ikut dibatalkan. 4) Perpindahan menuju `Cancelled` **tidak** mengunci apa pun. 5) Aman dipanggil berulang |
| **Verification** | `AT-RM-03`, `AT-RM-36` |
| **Risk/blocker** | Risiko: kunjungan dengan sangat banyak dokumen membuat transaksi panjang. Ditutup dengan penguncian per potongan. Catatan: endpoint ini tidak memvalidasi perpindahan status (`RM-CAP-019`), dan itu **tidak** diperbaiki task ini |
| **DoD** | Acceptance criteria terbukti uji; alur penutupan kunjungan berjalan tanpa regresi |

### `BE-08` — Pengisian data lama

| Field | Isi |
|---|---|
| **Task ID** | `BE-08` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-014`, memerlukan persetujuan clinical governance owner |
| **Outcome** | Catatan CPPT yang sudah tersimpan sebelum modul ini ada ikut memiliki status keutuhan, sehingga tidak ada bagian rekam medis yang luput dari aturan |
| **Trace** | `RM-DEC-014`; arsitektur bagian 8.2 migration ketiga |
| **Reuse** | — |
| **Scope** | Migration `BackfillProgressNoteIntegrity` |
| **Dependency** | `BE-01`. **Wajib selesai sebelum `BE-03` diaktifkan di produksi** — lihat integration contract bagian 3 |
| **Acceptance criteria** | 1) CPPT pada kunjungan `Completed` atau `Cancelled` bernilai `LockedUnsigned` dengan `LockTrigger = BackfillEncounterClosed`. 2) CPPT pada kunjungan berjalan bernilai `Draft`. 3) CPPT yang sudah dibatalkan bernilai `Cancelled`. 4) CPPT tanpa `ProviderUserId` tetap dibuat barisnya dengan `IsAuthorKnown = false`, **tidak dilewati diam-diam**. 5) Dijalankan bertahap per potongan. 6) Dapat dimundurkan dengan menghapus baris yang dibuatnya |
| **Verification** | `AT-RM-21`, `AT-RM-33`; ditambah percobaan pada salinan data nyata |
| **Risk/blocker** | **Satu-satunya task yang menyentuh data klinis nyata.** Jumlah barisnya belum diketahui karena data produksi tidak diaudit. Owner: clinical governance, **`OPEN`**. Prasyarat: unit rekam medis diberi tahu lebih dulu bahwa laporan kelengkapan akan menampilkan banyak catatan tidak ditandatangani |
| **DoD** | Migration teruji pada salinan data nyata; jumlah baris terdampak dilaporkan; unit rekam medis sudah diberi penjelasan; cara mundur terbukti |

---

## 5. Milestone B2 — Jejak dan kewenangan akses

### `BE-09` — Master keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-09` |
| **Status** | `TERTAHAN APPROVAL` — memerlukan SOP rekam medis untuk isi awalnya |
| **Outcome** | Petugas punya daftar keperluan akses yang dapat dipilih, bukan kotak teks kosong yang jawabannya tidak dapat dibandingkan |
| **Trace** | Arsitektur bagian 9; api-contract bagian 7 |
| **Reuse** | Pola master data yang sudah ada, misalnya `MstBillingItemCategory` |
| **Scope** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs`; DTO; controller; configuration; migration `AddMedicalRecordAccessAuditTables` bagian master; data awal |
| **Dependency** | Tidak ada task pendahulu |
| **Acceptance criteria** | 1) Lima keperluan minimum terisi. 2) Baris `Lainnya` memiliki `IsFreeTextRequired` bernilai benar. 3) `PurposeCode` dijamin unik. 4) Endpoint `/options` mengembalikan hanya yang aktif |
| **Verification** | Uji integrasi endpoint master; pemeriksaan data awal setelah migration |
| **Risk/blocker** | **Blocker:** isi awal harus berasal dari SOP rekam medis rumah sakit yang belum tersedia. Owner: product/domain, **`OPEN`** |
| **DoD** | Master terisi; daftar keperluan disetujui unit rekam medis; endpoint berjalan |

### `BE-10` — Tabel jejak akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-10` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-024` **sudah terjawab**: masa simpan 25 tahun, terbagi per tahun |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-005`, `RM-DEC-016`, `RM-DEC-017` |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-005` |
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
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-002` |
| **Outcome** | Riwayat klinis seorang pasien dapat diambil dari tiga belas sumber sekaligus dalam satu daftar berurut waktu |
| **Trace** | `RM-DEC-002`; `RM-CAP-004`; arsitektur bagian 5.8 |
| **Reuse** | Index `(PatientId, <tanggal>, IsDelete)` yang sudah ada pada seluruh tabel klinis; pola penggabungan `PrescriptionWorkspaceService` |
| **Scope** | `Services/MedicalRecordTimelineService.cs` |
| **Dependency** | `BE-02` untuk status keutuhan |
| **Acceptance criteria** | 1) Dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu. 2) Jumlah baris dibatasi dan penyaringan tanggal berfungsi. 3) Hanya jenis dokumen yang diminta yang diambil. 4) Bila satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai. 5) Memakai `AsNoTracking` |
| **Verification** | `AT-RM-09`, `AT-RM-31` |
| **Risk/blocker** | Risiko: penggabungan tiga belas sumber dapat menghasilkan banyak query. Ditutup pembatasan wajib pada acceptance criteria nomor 2 dan 3 |
| **DoD** | Acceptance criteria terbukti uji; waktu tanggap diukur pada data yang cukup banyak |

### `BE-14` — Endpoint berkas rekam medis

| Field | Isi |
|---|---|
| **Task ID** | `BE-14` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-002` |
| **Outcome** | Frontend dapat menampilkan berkas rekam medis pasien lengkap dengan ringkasan dan riwayatnya |
| **Trace** | `RM-DEC-002`; api-contract bagian 2 |
| **Reuse** | `BE-11`, `BE-13` |
| **Scope** | `Controllers/MedicalRecordController.cs`; `DTOs/MedicalRecordDtos.cs`. Endpoint ringkasan, riwayat, detail dokumen, dan metadata penyaring |
| **Dependency** | `BE-11`, `BE-13`. **Tidak boleh dimulai sebelum `BE-03` selesai** sesuai `RM-DEC-019` |
| **Acceptance criteria** | 1) Setiap permintaan melewati pencatatan jejak lebih dulu. 2) Status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan. 3) Jenis dokumen yang belum tunduk ditandai jelas. 4) `PrivateNote` **tidak ada** pada respons mana pun di endpoint ini |
| **Verification** | `AT-RM-09`, `AT-RM-32`, `AT-RM-37` |
| **Risk/blocker** | Risiko: menampilkan catatan sebagai berkas resmi padahal baru CPPT yang terlindungi. Ditutup acceptance criteria nomor 3 dan `RM-FE-009` |
| **DoD** | Endpoint berjalan; jejak akses tercatat pada setiap permintaan; Swagger terisi |

### `BE-15` — Endpoint `PrivateNote`

| Field | Isi |
|---|---|
| **Task ID** | `BE-15` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-022` |
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
| **Status** | `TERTAHAN APPROVAL` — **prioritas rendah**. Ditetapkan sebagai pengaman pada `RM-DEC-026`; perilakunya sudah pasti (`409`), dikerjakan paling akhir di milestone B3 |
| **Outcome** | Pasien yang punya dua nomor rekam medis tidak ditampilkan riwayatnya secara terpotong tanpa peringatan |
| **Trace** | `RM-CAP-007`; validation matrix bagian 4; api-contract bagian 2 kode `409` |
| **Reuse** | `MstPatient.MergedToPatientId` yang sudah ada |
| **Scope** | Pemeriksaan pada `MedicalRecordController` sebelum riwayat diambil |
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
| **Status** | `TERTAHAN APPROVAL` — bergantung seluruh task pendahulu |
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
| **Status** | `TERTAHAN APPROVAL` |
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
| Jumlah data lama tidak diketahui | Lama dan dampak `BE-08` tidak dapat diperkirakan | Percobaan pada salinan data nyata | Clinical governance, `OPEN` |
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
