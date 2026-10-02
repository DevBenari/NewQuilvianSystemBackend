# Laporan Perubahan Backend — `BE-RWI-038`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-038` |
| Judul | Catatan yang sudah diselesaikan dapat dikoreksi |
| Slice | `DOK-MVP-0b` — pendaftaran dokumen ke mesin keutuhan |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-038` |
| Trace | `RWI-DEC-086`, `RWI-DEC-087`, `RWI-RULE-038`, `RWI-FACT-014`; `FR-DOK-044`, `FR-DOK-045`, `FR-DOK-046`; `RWI-AC-157` s.d. `RWI-AC-162`; `02-backend-architecture.md` §4.9.2; `VAL-DOK-32` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | — (task tanpa dependency; salah satu dari dua akar roadmap) |
| Klasifikasi | `MEDIUM`, skor 8: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `MedicalRecordManagement`, `ClinicalManagement`, `PharmacyManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Keenam acceptance criteria terbukti. Nol migration, nol kolom baru, nol tabel baru, nol nilai jenis dokumen baru |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalRecordManagement`, `ClinicalManagement`, `PharmacyManagement` |
| Pemilik / prefix registry | `MedicalRecordManagement / Mrc` — `ACTIVE`; `ClinicalManagement / Cli` — `ACTIVE / LEGACY`; `PharmacyManagement / Phm` — `ACTIVE / LEGACY` |
| Applicability | `TOUCHED LEGACY`. Seluruh berkas yang disentuh adalah kode lama |
| QBE berlaku | `QBE-TXN-001`, `QBE-VAL-001`, `QBE-SVC-001`, `QBE-LOG-001` |
| Entity operasional baru | `NONE`. Nol model persisted dibuat, nol kolom ditambahkan |
| Archetype | Transaksi. Task ini tidak menambah satu endpoint pun; ia menyisipkan pendaftaran keutuhan pada tiga jalur finalisasi yang sudah ada |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Catatan dokter yang sudah diselesaikan berada di keadaan buntu: tidak dapat disunting, dan
tidak dapat dikoreksi.**

Mesin keutuhan rekam medis sudah lengkap sejak modul `MedicalRecordManagement` dibangun. Ia
menyimpan tanda tangan, penguncian, addendum bernomor urut beserta alasannya, dan penetapan
penulis pengganti. Masalahnya, ia hanya menegakkan aturan itu untuk **satu** jenis dokumen —
catatan terpadu — sesuai cakupan rilis pertamanya. Temuan `RWI-FACT-014` mencatatnya apa adanya.

Akibatnya bagi dokter rawat inap:

1. Dokter menulis catatan pemeriksaan, lalu menekan Selesai.
2. Catatan berstatus `Completed`, dan jalur penyuntingannya tertutup — memang seharusnya begitu.
3. Sore harinya dokter menyadari ia salah mengetik frekuensi napas: tertulis 20, seharusnya 24.
4. Ia membuka jalur koreksi. Mesin koreksi menjawab bahwa catatan itu **tidak dikenal**, karena
   catatan dokter memang tidak pernah didaftarkan.
5. Satu-satunya jalan yang tersisa adalah menulis **catatan baru** yang membantah catatan lama.

Rekam medis yang memuat dua catatan saling membantah lebih berbahaya daripada rekam medis yang
memuat satu salah ketik: pembaca berikutnya tidak punya cara mengetahui mana yang benar, dan
tidak ada alasan koreksi yang tersimpan di mana pun.

Keadaan yang sama berlaku pada kajian medis dan pada tindakan yang sudah ditandai dikerjakan.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Dokumen klinis yang sudah final dapat dikoreksi lewat koreksi beralasan, tanpa isi aslinya berubah |
| Pelaku | Dokter penulis dokumen; kelak juga DPJP pengganti lewat `BE-RWI-047` |
| Pemicu | Dokter menekan Selesai pada catatan dokter, kajian medis, atau tindakan |
| Hasil akhir | Dokumen tercatat pada daftar keutuhan berstatus **tertanda tangan**, dan sejak itu menerima koreksi |

### 2.2 Langkah berurutan

1. Dokter menyelesaikan dokumennya.
2. Pada **transaksi yang sama**, dokumen didaftarkan ke daftar keutuhan dan langsung ditandai
   tertanda tangan, dengan **penulis dokumen** sebagai penanda tangannya.
3. Waktu tanda tangan, perangkat, dan alamat jaringan tersimpan sebagai bukti. Ketiganya diambil
   dari permintaan HTTP, bukan dari kiriman layar — nilai yang dikirim layar dapat dipalsukan.
4. Dokumen berpindah ke keadaan terkunci. Penyuntingan langsung tertutup; koreksi terbuka.
5. Ketika dokter menemukan kesalahan, ia menambahkan koreksi beserta alasannya. Koreksi menempel
   di bawah dokumen aslinya dan diberi nomor urut. **Isi aslinya tidak berubah satu huruf pun.**

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Pendaftaran keutuhan gagal saat finalisasi | **Finalisasi ikut dibatalkan.** Dokumen tetap belum selesai | `400`, atau galat basis data yang membatalkan transaksi |
| Koreksi diminta pada dokumen yang **belum** final | Ditolak beserta arahan menyunting langsung pada catatannya | `400` |
| Koreksi diminta oleh orang yang bukan penulis dan tidak punya kewenangan pengganti | Ditolak | `403` |
| Dokumen sudah terkunci lalu difinalkan ulang | Tanda tangan **tidak** ditimpa; bukti tanda tangan pertama tetap utuh | `200` |

### 2.4 Kenapa satu transaksi, bukan dua langkah

Ini butir yang paling menentukan pada task ini.

> Bila pendaftaran dipisah dari finalisasi dan pendaftarannya gagal, yang lahir adalah dokumen
> **final tanpa baris keutuhan** — dokumen yang tidak dapat disunting karena statusnya sudah
> selesai, sekaligus tidak dapat dikoreksi karena mesin koreksi tidak mengenalnya. Itu persis
> keadaan buntu yang sedang ditutup task ini.

Karena itu kegagalan pendaftaran **membatalkan** finalisasi. Dokumen yang gagal difinalkan masih
dapat diperbaiki dan difinalkan ulang; dokumen buntu tidak dapat diperbaiki selamanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `AGENTS.md`, `rules/backend/**`, `rules/backend/engineering/**` | Governance, preflight, dan batas wewenang |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD `BE-RWI-038` |
| `contracts/api-contract.md` §9, `contracts/state-transition-matrix.md` §6, `contracts/validation-matrix.md` §8 | Kontrak koreksi dokumen dan kalimat penolakannya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Mesin keutuhan yang dipakai ulang apa adanya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | Aturan kewenangan koreksi |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Titik finalisasi catatan dokter beserta transaksinya |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Titik penyelesaian kajian medis |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Titik penandaan tindakan dikerjakan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Daftar jenis yang ditegakkan bertambah `Consultation`, `Assessment`, `Procedure`. Ditambahkan `RegisterSignedAsync` yang mendaftarkan sekaligus menandatangani, **tanpa menyimpan** — pemanggil wajib menjalankannya di dalam transaksinya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | Untuk jenis yang ditegakkan, ketiadaan baris keutuhan kini dijawab "Catatan ini belum final. Perbaiki langsung pada catatannya." — `VAL-DOK-32` — dan dipetakan ke `400` |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Pendaftaran tertanda tangan disisipkan di dalam transaksi finalisasi; kegagalannya membatalkan finalisasi dan dijawab sebagai penolakan permintaan, bukan galat sistem |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Meneruskan perangkat dan alamat jaringan penanda tangan dari permintaan HTTP |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Pendaftaran kajian medis dinaikkan dari **konsep** menjadi **tertanda tangan** |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Penandaan tindakan dikerjakan sekaligus mendaftarkannya tertanda tangan pada `SaveChanges` yang sama |
| `Tests/.../ClinicalManagement/ClinicalDocumentFinalizationIntegrityTests.cs` | **Baru.** Tujuh uji, satu per acceptance criteria beserta regresi catatan terpadu |
| `Tests/.../Infrastructure/TindakanTestData.cs` | **Baru.** Penyiapan tindakan master, catatan induk, dan tindakan pasien |
| `Tests/.../MedicalRecordManagement/ClinicalDocumentIntegrityServiceTests.cs` | Contoh jenis yang belum ditegakkan berpindah ke tanda vital; ditambah uji bahwa tiga jenis baru benar-benar masuk daftar |
| `Tests/.../MedicalRecordManagement/MedicalRecordFileEndpointTests.cs` | Hitungan jenis yang ditegakkan berubah dari satu menjadi empat |
| Enam berkas uji lain | Menyuntikkan service keutuhan pada konstruksi `ConsultationFinalizationService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru, tidak ada endpoint yang berubah bentuknya. Yang berubah adalah **kalimat penolakan** koreksi atas dokumen yang belum final, sesuai `VAL-DOK-32`. Respons penyelesaian kajian medis tetap memuat `IsRegisteredToIntegrity` seperti sebelumnya |
| Database | Nol perubahan schema, nol migration, nol eksekusi database. Yang bertambah hanyalah **baris data** pada `MrcClinicalDocumentIntegrity` untuk tiga jenis dokumen yang sebelumnya tidak didaftarkan |
| Keamanan/Auth | Penanda tangan diambil dari **penulis dokumen**, bukan dari aktor yang menekan tombol. Perangkat dan alamat jaringan diambil dari permintaan HTTP, bukan dari kiriman klien — `RM-DEC-021`. Nol perubahan pada butir hak akses |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah maupun mengubah bentuk endpoint. Tiga endpoint di bawah **perilakunya**
berubah, dan dicantumkan supaya perubahannya terbaca.

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan catatan dokter; **kini sekaligus mendaftarkannya sebagai dokumen tertanda tangan** | `DoctorConsultation : Update` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian; pendaftaran keutuhannya **naik dari konsep menjadi tertanda tangan** | `PatientAssessment : Update` |

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/execute` | Menandai tindakan dikerjakan; **kini sekaligus mendaftarkannya sebagai dokumen tertanda tangan** | `PatientProcedure : Update` |

#### Health Services / Medical Record Management / Clinical Note Addendum

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-document/{documentKind}/{documentId}` | Menambahkan koreksi; **kini menerima `Consultation`, `Assessment`, dan `Procedure`** | `ClinicalNoteAddendum : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Finalisasi catatan dokter mendaftarkan dokumen tertanda tangan atas nama **penulis**, bukan aktor | Penanda tangan sama dengan penulis; berbeda dari supervisor yang menekan tombol | `PASS` | `FinalisasiCatatanDokter_MendaftarkanDokumenTertandaTanganAtasNamaPenulis` |
| Pendaftaran gagal → finalisasi ikut batal | Catatan tetap `InProgress`, tanpa waktu selesai, tanpa baris keutuhan berlaku | `PASS` | `PendaftaranKeutuhanGagal_FinalisasiCatatanDokterIkutDibatalkan` |
| Penyelesaian kajian medis mendaftarkan tertanda tangan | Status `Signed`, pemicu kunci `AuthorSigned` | `PASS` | `PenyelesaianKajianMedis_MendaftarkanDokumenTertandaTangan` |
| Penandaan tindakan dikerjakan mendaftarkan tertanda tangan | Status `Signed`, penanda tangan pelaksana | `PASS` | `PenandaanTindakanDikerjakan_MendaftarkanDokumenTertandaTangan` |
| Koreksi pada dokumen final diterima, isi asli tidak berubah | `201`; `Objective` tetap "Napas 20 kali per menit"; `AddendumCount` menjadi 1 | `PASS` | `KoreksiPadaCatatanDokterYangSudahFinal_Diterima` |
| Koreksi pada dokumen belum final ditolak `400` beserta arahan | Pesan memuat "belum final" dan "Perbaiki langsung pada catatannya" | `PASS` | `KoreksiPadaCatatanDokterYangBelumFinal_Ditolak400BesertaArahanMenyunting` |
| Catatan terpadu tidak berubah perilakunya | Tetap terdaftar **konsep** saat dibuat dan tetap boleh disunting | `PASS` | `CatatanTerpadu_TetapTerdaftarSebagaiKonsepDanMasihBolehDisunting` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalannya `BillingManagement.BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, membandingkan status folio `FINAL` terhadap `CLOSED`. Berkas itu **tidak disentuh** task ini — lihat `git status --short` pada bagian 7 |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Seluruh kegagalan berasal dari satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` — `QUILVIAN_BILLING_TEST_DB` belum diisi, sehingga tidak ada database uji yang boleh dipakai |

Uji manual: `NOT FEASIBLE`. Tidak ada lingkungan runtime yang diberi wewenang pada task ini.

**Tidak dijalankan:** eksekusi migration dan perintah basis data apa pun. Task ini tidak
menghasilkan migration dan tidak memerlukannya.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Memfinalkan catatan dokter mendaftarkannya sebagai dokumen tertanda tangan, dengan penulis dokumen sebagai penanda tangan | Terpenuhi | `FinalisasiCatatanDokter_MendaftarkanDokumenTertandaTanganAtasNamaPenulis` — penanda tangan adalah penulis, dan sengaja **berbeda** dari aktor yang menekan tombol |
| 2. Bila pendaftaran gagal, finalisasi ikut batal | Terpenuhi | `PendaftaranKeutuhanGagal_FinalisasiCatatanDokterIkutDibatalkan` — catatan tetap `InProgress` dan tidak punya baris keutuhan berlaku |
| 3. Menyelesaikan kajian medis dan menandai tindakan dikerjakan berperilaku sama | Terpenuhi | `PenyelesaianKajianMedis_...` dan `PenandaanTindakanDikerjakan_...` |
| 4. Koreksi pada dokumen yang sudah final diterima | Terpenuhi | `KoreksiPadaCatatanDokterYangSudahFinal_Diterima` |
| 5. Koreksi pada dokumen yang belum final ditolak `400` beserta arahan menyunting langsung | Terpenuhi | `KoreksiPadaCatatanDokterYangBelumFinal_Ditolak400BesertaArahanMenyunting` |
| 6. Catatan terpadu tidak berubah perilakunya | Terpenuhi | `CatatanTerpadu_TetapTerdaftarSebagaiKonsepDanMasihBolehDisunting` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | ✅ |
| Tiga jenis dokumen terdaftar | ✅ `Consultation`, `Assessment`, `Procedure` |
| Test transaksi hijau | ✅ `PendaftaranKeutuhanGagal_FinalisasiCatatanDokterIkutDibatalkan` |
| Laporan menyebut nol perubahan bentuk data | ✅ Nol tabel, nol kolom, nol nilai enum baru, nol migration |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `185 Warning(s)` pada build, seluruhnya peringatan dokumentasi XML yang sudah ada sebelum task ini. Nol peringatan baru |
| Masalah yang diketahui | Sembilan jenis dokumen lain pada `ClinicalDocumentKind` masih belum ditegakkan aturan keutuhannya. Keadaan itu **dinyatakan terbuka** pada layar lewat penanda `IsIntegrityEnforced`, bukan disembunyikan |
| Risiko tersisa | Kajian medis dan tindakan yang **sudah** selesai sebelum task ini tidak punya baris keutuhan, sehingga belum dapat dikoreksi. Pengisian data lama untuk kedua jenis itu belum ada; yang tersedia hari ini hanya untuk catatan terpadu. Dicatat sebagai pekerjaan tersendiri milik `MedicalRecordManagement` |
| Selisih yang dilaporkan | Kalimat penolakan koreksi atas dokumen yang belum terdaftar diubah dari "Catatan tidak ditemukan pada daftar keutuhan." menjadi "Catatan ini belum final. Perbaiki langsung pada catatannya." untuk jenis yang ditegakkan. Kalimat baru mengikuti `VAL-DOK-32`; kalimat lama benar secara teknis tetapi menyesatkan bagi pengguna, karena dokumennya jelas ada — yang belum ada adalah finalisasinya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; sesudahnya 19 berkas source dan 13 berkas uji berubah atau baru, seluruhnya dalam cakupan delapan task yang dikerjakan berurutan pada sesi ini. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | `BE-RWI-047` memakai hasil task ini untuk membuka koreksi setelah pasien pulang beserta penjaga kewenangan per pasien |
