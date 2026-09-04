# Laporan Perubahan Backend — `RJ-DOC-BE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-BE-001` |
| Judul | Satukan jalur penyelesaian ke canonical finalization |
| Slice | Doctor / Rawat Jalan Clinical — penyelesaian konsultasi |
| Roadmap | [roadmap/doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `4.1` |
| Trace | `RJ-DOC-DEC-001`, `RJ-DOC-DEC-005`, `RJ-DOC-DEC-006`; `RJ-DOC-CAP-015`, `CAP-016`, `CAP-017`, `CAP-030` |
| Contract version | `RJ-DOC-COMPLETION-001@1.0.0` — `FROZEN` |
| Dependency | `RJ-DOC-INT-001` `FROZEN`; `RJ-DOC-INT-002` `FROZEN` |
| Klasifikasi | `MEDIUM` — skor `7`: repository `0`, berkas diperiksa `1`, berkas diubah `1`, logika bisnis `1`, kontrak API `1`, database `1`, keamanan/auth `1`, UI/workflow `1` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/**`, `Tests/QuilvianSystemBackend.Tests/**`, `docs/module-blueprints/rawat-jalan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `801a4f52459e1251ec9bb03c1abfe5e17dd3639c` cabang `sukmagp` — sama persis dengan SHA contract freeze, tanpa drift |
| Tanggal | `2026-08-31` |
| Status | `COMPLETE` |

---

## 1. Masalah yang diperbaiki

Ketika dokter menekan tombol **Selesai Konsultasi**, sistem menutup antrean pasien dan menandai
kunjungannya selesai — tetapi **tidak pernah menutup konsultasinya sendiri**.

Akibatnya, untuk setiap pasien Rawat Jalan yang konsultasinya "selesai":

1. Catatan konsultasi tetap berstatus *sedang berjalan* selamanya. Ia tidak pernah tercatat
   selesai, tidak pernah punya waktu penyelesaian, dan tidak pernah punya nama dokter yang
   menyelesaikannya.
2. Karena catatan itu tidak pernah berstatus selesai, **seluruh penguncian yang bergantung
   padanya tidak pernah aktif**. SOAP, diagnosis, resep, dan tindakan masih dapat diubah setelah
   dokter menyatakan konsultasi selesai.
3. Resep tetap berstatus draf. Ia tidak pernah difinalkan, sehingga tidak pernah sampai ke
   farmasi melalui jalur resmi.
4. **Tidak satu pun fakta klinis pernah diserahkan ke Billing.** Aturan yang berlaku menyatakan
   resep menjadi dasar tagihan ketika difinalkan bersama konsultasi dokter — dan momen itu tidak
   pernah terjadi.
5. Kunjungan langsung ditandai **Selesai**, melewati keadaan *Konsultasi Selesai* dan *Proses
   Billing*. Padahal Rekam Medis memperlakukan kunjungan berstatus Selesai sebagai kunjungan yang
   sudah tidak berjalan, lalu **mengunci catatan klinisnya**. Catatan terkunci sementara farmasi,
   laboratorium, radiologi, dan Billing masih bekerja.

Contoh konkret. Pasien A berkonsultasi, dokter menuliskan resep dan menekan Selesai Konsultasi.
Yang terjadi sebelum perbaikan: antrean pasien A hilang dari layar dokter, kunjungannya tertulis
Selesai, catatan medisnya terkunci — tetapi resepnya masih draf, konsultasinya masih berjalan,
dan bagian Billing tidak pernah menerima kabar apa pun tentang resep itu.

---

## 2. Proses bisnis

**Tujuan.** Menyelesaikan konsultasi dokter Rawat Jalan secara sah dan lengkap, satu kali, melalui
satu jalur.

**Pelaku.** Dokter pemeriksa.

**Pemicu.** Dokter menekan tombol *Selesai Konsultasi*.

**Langkah berurutan sesudah perbaikan.**

1. Sistem memastikan antrean itu memang milik dokter yang sedang login.
2. Sistem memastikan antrean benar-benar sedang dalam konsultasi.
3. Sistem **mencari sendiri** catatan konsultasi milik antrean tersebut. Nomor konsultasi tidak
   pernah dikirim dari layar, sehingga tidak ada identitas yang perlu dipercaya dari luar.
4. Catatan tambahan antrean disimpan, dan catatan klinis yang belum ditandatangani dikunci.
5. Sistem menjalankan **pemeriksaan kelayakan klinis**: SOAP, diagnosis utama, kelengkapan resep,
   dan kelengkapan tindakan.
6. Bila lolos, konsultasi ditandai **Selesai** beserta waktu dan nama dokternya; resep draf
   difinalkan; fakta klinis diserahkan ke Billing.
7. Antrean ditutup, kunjungan berpindah ke **Konsultasi Selesai**, dan layar antrean lain
   diberitahu.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Antrean bukan milik dokter yang login | Ditolak `404`, seperti sebelumnya |
| Antrean belum dalam konsultasi | Ditolak `400` "Konsultasi dokter belum dimulai" |
| Catatan konsultasi tidak ditemukan | Ditolak `404` dengan penjelasan |
| Dokumentasi klinis belum lengkap | Ditolak `400` beserta rincian per tab yang perlu diperbaiki. **Antrean dan kunjungan tidak ikut ditutup** |
| Ada peringatan yang belum dikonfirmasi | Ditolak `400`; peringatan **tidak** dianggap disetujui secara otomatis |
| Data konsultasi berubah dari perangkat lain | Ditolak `409` |
| Identitas dokter tidak dapat dibaca | Ditolak; penyelesaian tanpa aktor tidak disimpan |
| Konsultasi sudah selesai sebelumnya | Ditolak; waktu dan aktor penyelesaian pertama tidak tertimpa |

**Hasil akhir.** Konsultasi `Completed`, antrean `Completed`, kunjungan `ConsultationCompleted`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `REPORT_TEMPLATE`,
`BACKEND_ENGINEERING_CONTRACT`, `MODULE_OWNERSHIP_PREFIX_REGISTRY`);
`roadmap/doctor-consultation-roadmap.md`; `contracts/doctor-consultation-contracts.md`;
`contracts/integration-contract.md`; `MODULE-STATUS.md`; `00-interview-decisions.md`;
`DoctorQueueController.cs`; `DoctorConsultationController.cs`;
`ConsultationFinalizationService.cs`; `ConsultationValidationService.cs`;
`DoctorConsultationLifecycleService.cs`; `ClinicalDocumentIntegrityService.cs`;
`ClinicalMilestoneFactProducer.cs`; `EncounterStatus.cs`;
`MedicalRecordAccessAuditService.cs`; `MedicalRecordBackfillService.cs`;
`TrxDoctorConsultationConfiguration.cs`; `MstDoctorConfiguration.cs`;
`Tests/QuilvianSystemBackend.Tests/Infrastructure/**`; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | `POST /{id}/finish-consultation` berhenti memiliki logika penyelesaian klinis sendiri dan menjadi lapisan orkestrasi: ia meresolusi konsultasi milik antrean, lalu mendelegasikan finalisasi ke `ConsultationFinalizationService`. Efek antrean yang sah dipertahankan — catatan antrean, penguncian catatan klinis, dan pemberitahuan realtime. Penetapan `EncounterStatus = Completed` dan `Encounter.CompletedAt` **dihapus** karena melanggar kontrak bagian 1.8. Ditambahkan audit log penyelesaian yang sebelumnya tidak ada sama sekali |
| `Areas/HealthServices/ClinicalManagement/Services/DoctorConsultationLifecycleService.cs` | Ditambahkan `ResolveFinalizableForQueueAsync` — resolusi konsultasi dari antrean di lapisan service, memeriksa kecocokan antrean **dan** kunjungan sekaligus, serta menolak konsultasi yang sudah selesai atau dibatalkan |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Penyelesaian ditolak bila aktor tidak dapat ditentukan, sesuai kontrak bagian 1.2. Sebelumnya `Guid.Empty` dapat tersimpan sebagai penyelesai konsultasi |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | `POST /doctor-consultations` menolak `CompleteImmediately=true` untuk pembuatan **berantrean**, sesuai `RJ-DOC-DEC-005`. Jalur tanpa antrean tidak disentuh |
| `Tests/QuilvianSystemBackend.Tests/ClinicalManagement/DoctorConsultationCompletionTests.cs` | Berkas baru — `9` uji acceptance |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada route, nama parameter, atau bentuk payload yang berubah. Yang berubah adalah **perilaku** `POST /doctor-queues/{id}/finish-consultation`: ia kini dapat membalas `400` validasi klinis dan `409` konflik, yang sebelumnya tidak pernah terjadi karena tidak ada validasi sama sekali. `POST /doctor-consultations` menolak satu kombinasi yang sebelumnya diterima. Keduanya adalah pengetatan yang disengaja dan diminta kontrak, bukan perubahan bentuk kontrak |
| Database | `NOT APPLICABLE`. Tidak ada perubahan schema, tidak ada entity baru, tidak ada migration dibuat, tidak ada migration diterapkan |
| Keamanan/Auth | Model authorization **tidak diubah**. `[Authorize]` dan `[AccessPermission("DoctorQueue","Update")]` tetap berlaku; pemeriksaan kepemilikan antrean oleh dokter yang login tetap berjalan. Diperketat: identitas konsultasi diresolusi server sehingga tidak ada ID relasi dari client yang dipercaya, dan penyelesaian tanpa aktor yang dapat ditentukan ditolak |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/finish-consultation` | Menyelesaikan konsultasi dokter dari layar antrean. Kini memfinalisasi konsultasi melalui jalur canonical, bukan sekadar menutup antrean | `DoctorQueue : Update` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Jalur canonical penyelesaian konsultasi. Tidak berubah pada task ini selain penolakan aktor kosong | `DoctorConsultation : Update` |
| `POST` | `/` | Membuat konsultasi dokter. `completeImmediately=true` kini ditolak bila permintaan membawa antrean | `DoctorConsultation : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)`; tidak ada peringatan baru pada keempat berkas yang diubah | `PASS` | Keluaran perintah |
| `dotnet test QuilvianSystemBackend.Tests` — seluruh project | `141` lulus, `0` gagal, `0` dilewati | `PASS` | Keluaran perintah |
| `A` Konsultasi `InProgress` menjadi `Completed` beserta antrean dan kunjungan | Status, `CompletedAt`, `CompletedByUserId`, `QueueStatus`, dan `EncounterStatus = ConsultationCompleted` terbukti | `PASS` | `Canonical_MenyelesaikanKonsultasiBesertaAntreanDanKunjungan` |
| `A2` Kunjungan tidak dinaikkan ke `Billing` maupun `Completed` | Terbukti | `PASS` | `Canonical_TidakMenaikkanKunjunganKeBillingAtauCompleted` |
| `B` Resolusi konsultasi dari antrean | Ditemukan sebelum selesai, tidak ditemukan lagi sesudah selesai | `PASS` | `ResolusiDariAntrean_MenemukanKonsultasiLaluBerhentiSetelahSelesai` |
| `B2` Resolusi tidak mengambil konsultasi milik antrean lain | Terbukti, termasuk pasangan antrean/kunjungan yang disilangkan | `PASS` | `ResolusiDariAntrean_TidakMengambilKonsultasiMilikAntreanLain` |
| `C` Validasi klinis gagal tidak meninggalkan state separuh jadi | Konsultasi tetap `InProgress`, antrean tetap `InConsultation`, kunjungan tidak berpindah | `PASS` | `ValidasiGagal_TidakMenutupKonsultasiAntreanMaupunKunjungan` |
| `D` Perilaku antrean yang sah tidak regresi | `141` uji lulus termasuk `EncounterClosureLockTests` yang menguji penguncian catatan | `PASS` | Keluaran perintah |
| `E` `CompleteImmediately` tidak dapat dipakai sebagai jalur alternatif Rawat Jalan | Ditolak `400` dengan pesan pembatasan, dan tidak ada konsultasi selesai yang lahir | `PASS` | `CompleteImmediately_DitolakUntukPembuatanKonsultasiBerantrean` |
| `F` Pembuatan konsultasi normal tetap berjalan | Konsultasi tercipta `InProgress` tanpa waktu penyelesaian | `PASS` | `PembuatanKonsultasiNormal_TetapBerjalanTanpaCompleteImmediately` |
| Aktor kosong ditolak | Terbukti; penyelesaian tidak tersimpan | `PASS` | `AktorKosong_DitolakDanTidakMenyimpanPenyelesaian` |
| Penyelesaian kedua ditolak tanpa menimpa jejak pertama | Terbukti | `PASS` | `PenyelesaianKedua_DitolakDanTidakMenimpaJejakPenyelesaianPertama` |

Uji manual: `NOT FEASIBLE` — memerlukan lingkungan berjalan beserta frontend; jalur yang sama
sudah dibuktikan lewat uji otomatis pada lapisan service dan controller.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `dotnet test QuilvianSystemBackend.BillingTests` | Fixture-nya menerapkan `Database.Migrate()` ke basis data pengembangan bersama. `AGENTS.md` dan batasan task melarangnya. Project-nya tetap dibuktikan **ikut ter-compile** lewat build solution |
| Migration | Task ini tidak mengubah schema |

Basis data uji yang dipakai adalah SQLite di dalam memori (`TestDatabase`), yang menurut
dokumentasinya tidak pernah menyentuh basis data mana pun yang tercatat di `appsettings`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setelah `Selesai Konsultasi` berhasil, `ConsultationStatus = Completed`, `CompletedAt` dan `CompletedByUserId` terisi | Terpenuhi | `Canonical_MenyelesaikanKonsultasiBesertaAntreanDanKunjungan` |
| 2. Efek antrean existing dipertahankan | Terpenuhi | Uji yang sama beserta `141` uji regresi |
| 3. `EncounterStatus` menjadi `ConsultationCompleted` dari setiap permukaan | Terpenuhi | `Canonical_...` dan `Canonical_TidakMenaikkanKunjunganKeBillingAtauCompleted` |
| 4. Penguncian `ProgressNote` existing tetap berjalan | Terpenuhi | Pemanggilan dipertahankan; `EncounterClosureLockTests` lulus |
| 5. Tidak tersisa dua implementasi finalisasi yang dapat menghasilkan state berbeda | Terpenuhi | `ConsultationFinalizationService` menjadi satu-satunya implementasi; jalur antrean mendelegasikan; jalur `CompleteImmediately` ditutup untuk Rawat Jalan |
| DoD — satu implementasi canonical, terbukti test, tanpa regresi antrean maupun IGD | Terpenuhi | Jalur tanpa antrean tidak disentuh; `141` uji lulus |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution memunculkan peringatan pada berkas yang **tidak** disentuh task ini. Keempat berkas yang diubah tidak memunculkan peringatan baru |
| Masalah yang diketahui | **Perubahan perilaku yang disengaja.** Jalur antrean kini dapat menolak penyelesaian yang sebelumnya selalu berhasil, yaitu ketika dokumentasi klinis belum lengkap atau ada peringatan yang belum dikonfirmasi. Ini konsekuensi langsung `RJ-DOC-BE-002` yang belum dikerjakan dan `RJ-DOC-FE-002` yang akan menyediakan layar konfirmasinya. Penyelesaian tanpa validasi **tidak** dipertahankan sebagai jalan pintas karena melanggar kontrak dan mengembalikan cacat yang justru sedang diperbaiki |
| Risiko tersisa | `1` Jalur antrean mengirim `AcknowledgedWarningKeys` kosong, sehingga konsultasi yang memiliki peringatan akan tertahan sampai `RJ-DOC-FE-002` selesai. `2` `ExpectedUpdatedAt` masih opsional — pengetatannya adalah `RJ-DOC-BE-003`. `3` TOCTOU pada penjaga status belum ditutup; itu juga `RJ-DOC-BE-003`. `4` Fakta klinis yang gagal diserahkan belum dapat ditemukan kembali; itu `RJ-DOC-BE-005` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `4` berkas source berubah, `1` berkas uji baru, ditambah dokumentasi blueprint. Berkas staged yang tidak berkaitan — `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratoryAuthorityTests.cs` dan `agents/rules/**` — **tidak disentuh** |
| Langkah berikutnya | `RJ-DOC-BE-002` — menjadikan validasi finalisasi mengikat pada seluruh permukaan. Wewenang implementasinya belum diberikan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` (`Cli`), `RegistrationManagement` (`Reg`), `PharmacyManagement` (`Phm`) |
| Submodule | `NOT APPLICABLE` |
| Status registry | Ketiganya `ACTIVE / LEGACY` dan sudah terdaftar |
| Keberlakuan | `TOUCHED LEGACY` — tidak ada entity operasional baru, tidak ada tabel baru, tidak ada rename |
| QBE yang berlaku | `QBE-SVC-001` orkestrasi domain berada di service, controller tidak lagi memuat logika finalisasi; `QBE-API-001` boundary dan status response existing dipakai apa adanya; `QBE-PERM-001` metadata Access tidak diubah; `QBE-VAL-001` validasi canonical dipakai, bukan dilewati; `QBE-TXN-001` konsistensi lintas record dijaga satu transaksi; `QBE-DTO-001` tidak ada entity EF yang menjadi kontrak API; `QBE-LOG-001` audit penyelesaian ditambahkan; `QBE-AUD-001` audit database tetap terpisah dari logging aplikasi |
| QBE yang **tidak** berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-MOD-002/003`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada entity, prefix, kode bisnis, maupun pekerjaan database pada task ini |
| Sisa `agents/rules/` pada repository | Ditemukan sebagai berkas terhapus yang sudah di-stage sebelum sesi ini. Sesuai governance suite, folder itu memang sudah dicabut dari repository target dan **tidak dipakai**; governance yang dipakai adalah `rules/backend/` milik suite skill |
