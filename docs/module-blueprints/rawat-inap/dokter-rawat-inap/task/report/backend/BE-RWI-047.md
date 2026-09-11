# Laporan Perubahan Backend — `BE-RWI-047`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-047` |
| Judul | Catatan lama tetap dapat dibetulkan, termasuk setelah pasien pulang |
| Slice | `DOK-MVP-3` — catatan harian |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-047` |
| Trace | `FR-DOK-015`, `FR-DOK-016`, `FR-DOK-047`, `FR-DOK-048`; `RWI-DEC-088`; `RWI-AC-161`, `RWI-AC-163` s.d. `RWI-AC-167`; `permission-audit-matrix.md` §3; `VAL-DOK-03`, `VAL-DOK-34`, `VAL-DOK-35` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-038` **selesai** ([laporan](BE-RWI-038.md)); `BE-RWI-046` **selesai** ([laporan](BE-RWI-046.md)) |
| Klasifikasi | `MEDIUM`, skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 0, database 0, keamanan/auth 3, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, satu controller `MedicalRecordManagement`, `Program.cs`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Keenam acceptance criteria terbukti. Nol perubahan **model** pada `MedicalRecordManagement`; satu selisih DoD dilaporkan pada bagian 7 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`, menyentuh satu controller `MedicalRecordManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY`; `MedicalRecordManagement / Mrc` — `ACTIVE` |
| Applicability | `NEW CODE` untuk service penjaga kewenangan; `TOUCHED LEGACY` untuk controller koreksi |
| QBE berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-API-001` |
| Entity operasional baru | `NONE`. Nol model persisted, nol kolom, nol tabel |
| Archetype | Transaksi. Nol endpoint baru; yang ditambahkan adalah penjaga aturan bisnis pada jalur koreksi yang sudah ada |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

Dua masalah berbeda, dan keduanya berpangkal pada hal yang sama: **koreksi paling sering
dibutuhkan justru setelah pasien pulang.**

### 1.1 Perawatan tertutup menutup terlalu banyak

Perawatan yang sudah ditutup memang harus menolak catatan **baru** — pasien sudah pulang, dan
catatan baru pada perawatan yang selesai akan menggeser riwayat serta lama rawat. Tetapi
penolakan yang sama tidak boleh berlaku bagi **koreksi** atas catatan yang sudah ada. Kesalahan
tulis biasanya ditemukan saat berkas dibaca ulang, dan itu terjadi setelah pasien pulang.

### 1.2 Penetapan berhalangan membuka pintu terlalu lebar

Ini celah yang paling mudah terlewat, dan `permission-audit-matrix.md` bagian 3 menuliskannya
dengan sadar sebelum implementasi dimulai.

Penetapan berhalangan milik `MedicalRecordManagement` menyatakan **"dokter ini berhalangan"** —
dan berhenti di situ. Ia **tidak menyebut siapa penggantinya**. Begitu satu penetapan berlaku,
setiap pemegang butir hak akses `ClinicalNoteAddendum : CreateAsSubstitute` dapat mengoreksi
catatan dokter itu, termasuk untuk pasien yang sama sekali bukan tanggung jawabnya.

Contoh nyatanya:

> dr. Andi, DPJP bangsal Melati, jatuh sakit. Kepala unit menerbitkan penetapan berhalangan atas
> namanya supaya catatannya tetap dapat dikoreksi. Sejak saat itu — tanpa penjaga tambahan —
> dr. Budi dari bangsal Anggrek, yang memegang butir pengganti untuk kebutuhannya sendiri, dapat
> mengoreksi catatan seluruh pasien dr. Andi. Mesin hak akses tidak dapat menutupnya, karena ia
> mengenal peran dan tidak mengenal pasien.

Uji yang hanya menguji hak akses **tidak akan menangkap ini**: seluruh pemeriksaan hak akses
memang lolos.

---

## 2. Proses bisnis

### 2.1 Tiga tingkat kewenangan koreksi

| Tingkat | Keadaan | Siapa yang boleh | Perlu penetapan? |
| ---: | --- | --- | --- |
| 1 | Penulis masih aktif | Penulis asli | Tidak |
| 2 | Akun penulis sudah nonaktif | DPJP perawatan itu | Tidak — disimpulkan sistem |
| 3 | Penulis berhalangan sementara | DPJP perawatan itu | Ya — penetapan kepala unit, **wajib berbatas waktu** |

Tingkat 2 dan 3 kini dijaga dua lapis: mesin koreksi memutuskan **apakah jalur pengganti
terbuka**, dan penjaga Rawat Inap memutuskan **siapa yang boleh melewatinya**.

### 2.2 Langkah berurutan koreksi atas nama dokter lain

1. Kepala unit menerbitkan penetapan berhalangan atas nama penulis, disertai alasan dan batas
   waktu. Penetapan tanpa batas waktu **ditolak** — penetapan permanen sama saja dengan pintu
   belakang tetap.
2. DPJP membuka catatan yang perlu dikoreksi dan mengirim koreksi lewat jalur pengganti.
3. Penjaga Rawat Inap menemukan perawatan yang menaungi dokumen itu lewat baris keutuhannya,
   lalu memeriksa apakah pengirim memang dokter penanggung jawab perawatan tersebut.
4. Bila ya, mesin koreksi menilai kewenangan pengganti seperti biasa, lalu menyimpan koreksi.
5. **Penulis catatan aslinya tidak berpindah.** Yang tersimpan adalah dua nama pada dua tempat
   berbeda: penulis tetap dokter yang berhalangan, penulis koreksi adalah DPJP penggantinya.

### 2.3 Kewenangan setelah pasien pulang

Ketika perawatan ditutup, penugasan DPJP biasanya ikut diakhiri. Bila kewenangan hanya dinilai
dari penugasan yang berlaku **hari ini**, koreksi setelah pasien pulang menjadi mustahil bagi
siapa pun — persis kebalikan dari yang diminta `FR-DOK-047`.

Karena itu aturannya bercabang:

| Keadaan perawatan | Yang dinilai |
| --- | --- |
| Masih berjalan | Penugasan yang periodenya memuat saat ini |
| Sudah ditutup atau dibatalkan | Penugasan **terakhir** yang pernah berlaku pada perawatan itu |

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Catatan **baru** pada perawatan tertutup | Ditolak beserta arahan bahwa koreksi tetap bisa | `422` |
| Koreksi oleh dokter yang bukan DPJP perawatan itu | Ditolak, walaupun hak akses dan penetapannya sah | `403` |
| Koreksi oleh pengguna yang tidak terhubung ke dokter mana pun | Ditolak | `403` |
| Penetapan tanpa masa berlaku | Ditolak | `400` |
| Dokumen di luar perawatan rawat inap | **Dilewatkan** — aturan ini memang tidak berlaku baginya | — |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD `BE-RWI-047` |
| `contracts/permission-audit-matrix.md` §3 | Batas yang tidak dapat dijaga mesin hak akses |
| `contracts/state-transition-matrix.md` §6.1 | Tiga tingkat kewenangan koreksi |
| `contracts/validation-matrix.md` §8 | `VAL-DOK-34`, `VAL-DOK-35` beserta kalimat penolakannya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | Aturan kewenangan yang dipakai ulang apa adanya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAuthorDelegationService.cs` | Penerbitan penetapan beserta penolakan batas waktu |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Pemeriksaan penugasan dokter berperiode |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`, `InpDoctorAssignment.cs` | Sumber kebenaran kewenangan per pasien |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientDocumentCorrectionAuthorityService.cs` | **Baru.** Penjaga `VAL-DOK-35`: menemukan perawatan yang menaungi dokumen, memetakan pengguna ke baris dokter lewat data, lalu memeriksa penugasannya. Dokumen di luar rawat inap dilewatkan |
| `Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs` | Memanggil penjaga itu pada jalur koreksi atas nama penulis lain. Keputusannya tetap milik service Rawat Inap; di sini hanya tempat memanggilnya |
| `Program.cs` | Pendaftaran service baru pada dependency injection |
| `Tests/.../ClinicalManagement/InpatientDocumentCorrectionTests.cs` | **Baru.** Tujuh uji, mencakup keenam acceptance criteria beserta regresi dokumen non-rawat-inap |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint baru, nol perubahan bentuk permintaan maupun balasan. Yang bertambah adalah satu **penolakan `403` baru** pada `POST /by-document/{documentKind}/{documentId}/as-substitute` bagi dokter yang bukan DPJP perawatan itu |
| Database | Nol perubahan schema, nol migration, nol eksekusi database. Nol perubahan **model** pada `MedicalRecordManagement` |
| Keamanan/Auth | **Menguat.** Satu celah kewenangan per pasien ditutup. Nol butir hak akses baru; nol pemeriksaan nama peran, nama jabatan, maupun `UserType` — kewenangan diturunkan dari penugasan dokter berperiode |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah endpoint. Dua endpoint di bawah **perilakunya** berubah.

#### Health Services / Medical Record Management / Clinical Note Addendum

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-document/{documentKind}/{documentId}/as-substitute` | Mengoreksi atas nama dokter yang berhalangan; **kini dibatasi DPJP perawatan pasien itu** | `ClinicalNoteAddendum : CreateAsSubstitute` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter; perawatan tertutup menolaknya `422` beserta arahan bahwa koreksi tetap bisa | `DoctorConsultation : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Perawatan tertutup menolak catatan baru | `422`, pesan memuat "sudah ditutup" dan "koreksi"; nol catatan tersimpan | `PASS` | `PerawatanTertutup_MenolakCatatanBaru422` |
| Perawatan tertutup menerima koreksi tanpa bergeser | `201`; status tetap `Closed`; `AdmittedAt`, `PhysicallyLeftAt`, `ClosedAt` identik; tempat tidur dan waktu mulainya identik; `EndDateTime` tetap kosong | `PASS` | `PerawatanTertutup_MenerimaKoreksiTanpaMenggeserKeadaanPerawatan` |
| DPJP mengoreksi atas nama penulis berhalangan | `201`; penulis catatan tetap dokter berhalangan; penulis koreksi DPJP; `IsSubstituteAuthor` benar; penetapan tercatat | `PASS` | `PenetapanBerlaku_DpjpAktifMengoreksiTanpaMemindahkanPenulisAsli` |
| **Dokter bukan DPJP ditolak walau hak akses dan penetapan lolos** | `403`, pesan memuat "DPJP yang sedang bertanggung jawab"; nol koreksi tersimpan | `PASS` | `DokterBukanDpjpPerawatanItu_Ditolak403MeskipunHakAksesDanPenetapanLolos` |
| Koreksi setelah pasien pulang oleh DPJP terakhir | `201` walaupun penugasannya sudah diakhiri bersamaan penutupan | `PASS` | `SetelahPasienPulang_DpjpTerakhirMasihDapatMengoreksiAtasNamaPenulis` |
| Penetapan tanpa masa berlaku ditolak | `400`, pesan memuat "Batas waktu penetapan"; nol penetapan tersimpan | `PASS` | `PenetapanTanpaMasaBerlaku_Ditolak400` |
| Dokumen di luar rawat inap tidak ikut terkunci | Penjaga mengembalikan `NotInpatientDocument` dan meloloskannya | `PASS` | `DokumenDiLuarRawatInap_TidakTundukPenjagaDpjp` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun; task ini tidak menghasilkan
migration.

### 5.1 Catatan tentang uji kriteria 5

Kriteria 5 menuntut bukti bahwa **seluruh pemeriksaan hak akses lolos** dan penolakannya datang
dari aturan bisnis. Uji `DokterBukanDpjpPerawatanItu_Ditolak403MeskipunHakAksesDanPenetapanLolos`
memenuhinya dengan cara berikut:

1. Endpoint dipanggil **langsung**, melewati filter hak akses seluruhnya — artinya pemanggil
   diperlakukan sebagai pemegang butir `CreateAsSubstitute`.
2. Jalur yang dipanggil adalah jalur pengganti, sehingga `actorHasSubstituteAuthority` bernilai
   benar.
3. Penetapan berhalangannya sah, masih berlaku, dan diterbitkan lewat endpoint sungguhan.
4. Dokter penyusupnya adalah **dokter sungguhan** dengan akun tertaut, hanya tanpa penugasan
   pada perawatan itu.

Dengan keempat hal itu, satu-satunya yang tersisa adalah aturan bisnis — dan dari sanalah `403`
datang.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Perawatan tertutup menolak catatan **baru** `422` | Terpenuhi | `PerawatanTertutup_MenolakCatatanBaru422` |
| 2. Perawatan tertutup menerima koreksi; status tetap tertutup, tempat tidur tidak berubah, lama dirawat tidak bergeser | Terpenuhi | `PerawatanTertutup_MenerimaKoreksiTanpaMenggeserKeadaanPerawatan` |
| 3. Setelah penetapan berlaku, DPJP aktif dapat mengoreksi catatan dokter yang berhalangan | Terpenuhi | `PenetapanBerlaku_DpjpAktifMengoreksiTanpaMemindahkanPenulisAsli` |
| 4. Koreksi atas nama dokter lain tidak mengubah penulis catatan aslinya | Terpenuhi | Uji yang sama — `AuthorUserId` dan `SignedByUserId` tetap dokter berhalangan |
| 5. Dokter yang bukan DPJP ditolak `403` walaupun butir hak akses dan penetapannya ada | Terpenuhi | `DokterBukanDpjpPerawatanItu_Ditolak403MeskipunHakAksesDanPenetapanLolos`, lihat bagian 5.1 |
| 6. Penetapan tanpa masa berlaku ditolak `400` | Terpenuhi | `PenetapanTanpaMasaBerlaku_Ditolak400` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | ✅ |
| Test nomor 5 hijau beserta catatan bahwa hak aksesnya lolos | ✅ Catatannya pada bagian 5.1 |
| Laporan menyebut nol perubahan pada `MedicalRecordManagement` | 🟡 **Tidak terpenuhi apa adanya.** Nol perubahan **model**, tetapi satu controller berubah. Rinciannya pada bagian 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Selisih DoD yang dilaporkan** | DoD menuntut "nol perubahan pada `MedicalRecordManagement`". Yang tercapai adalah **nol perubahan model, entity, dan migration** di sana — sesuai kolom Reuse pada roadmap. Namun `ClinicalNoteAddendumController.cs` **berubah**: satu parameter konstruktor dan satu pemanggilan penjaga. Alasannya tidak dapat dihindari — jalur tulis koreksi atas nama dokter lain hanya ada di endpoint itu, sehingga kriteria 5 mustahil dipenuhi tanpa menyentuhnya. Aturan bisnisnya sendiri **tidak** ditulis di sana: ia berada penuh pada service milik `ClinicalManagement`, sehingga kepemilikan aturan tetap di Rawat Inap |
| **Utang terbuka** | `MedicalRecordManagement` **tidak** termasuk tiga modul yang persetujuan lintas modulnya sudah ada lewat `RWI-DEC-062`. Perubahan dua baris pada controllernya karena itu perlu diketahui pemilik modul tersebut. Dicatat sebagai utang terbuka, sejenis dengan utang registry `Rad` pada [laporan `BE-RWI-042`](BE-RWI-042.md) |
| Masalah yang diketahui | Kebijakan siapa yang boleh menerbitkan penetapan berhalangan ditegakkan lewat butir hak akses `ClinicalNoteAuthorDelegation : Create`, bukan lewat pemeriksaan jabatan kepala unit. `RWI-DEC-088` menyebut kepala unit rawat inap; penetapannya kepada peran mana adalah pekerjaan admin di layar Akses Role, bukan kode |
| Risiko tersisa | Perawatan yang **tidak pernah** memiliki penugasan dokter menolak seluruh koreksi atas nama penulis lain, karena tidak ada DPJP yang dapat ditunjuk. Keadaan itu tidak seharusnya ada pada perawatan sungguhan; bila ditemukan, ia menandakan data penugasan yang belum lengkap, bukan kesalahan penjaga ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Pemilik `MedicalRecordManagement` diminta mengetahui perubahan dua baris pada controller koreksi |
