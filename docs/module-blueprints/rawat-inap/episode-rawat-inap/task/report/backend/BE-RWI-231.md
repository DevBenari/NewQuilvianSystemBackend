# Laporan Perubahan Backend - BE-RWI-231

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | BE-RWI-231 |
| Judul | Konteks dan kelayakan admisi bayi |
| Slice | PLAN-REPAIR-EPS-016; pasien baru dan pasien lama |
| Roadmap | [backend-roadmap-bayi-016.md](../../../roadmap/backend-roadmap-bayi-016.md) |
| Trace / contract version | RWI-BAYI-016-1.0; FIX-EPS-016-01/03/04 dan dukungan 06/09; disetujui pengguna 14 Oktober 2026 |
| Dependency | Kontrak disetujui; tidak ada task backend lain yang diimplementasikan pada slice ini |
| Klasifikasi | HEAVY, 11: repository 0, dibaca 2, diubah 2, bisnis 2, API 2, database 2, keamanan 1, UI 0 |
| Task mode | BACKEND; Area HealthServices, Module InPatientManagement, prefix Inp ACTIVE; PatientManagement hanya perluasan query profil existing |
| Target tulis | Source admisi bayi, DTO/EF/migration dan dokumen slice pada NewQuilvianSystemBackend; branch MHamzah / origin/MHamzah |
| Applicability | NEW CODE DTO/helper dalam module terdaftar; TOUCHED LEGACY service/model/query/controller; tidak membuat entity atau prefix baru |
| QBE | QBE-MOD-001/002, QBE-SVC-001, QBE-API-001, QBE-VAL-001, QBE-CFG-001, QBE-PERM-001, QBE-TXN-001, QBE-LOG-001; batas domain, validasi, izin dan operasi existing dipertahankan |
| Model | GPT-6 / Codex sesuai sesi; tidak ada agent tambahan |
| Commit backend saat dikerjakan | 9f8d82666e8d67dde203d64ffab640305efc90ab |
| Tanggal | 2026-10-14 |
| Status | Implementasi source selesai; penerapan migration dan pembuktian PostgreSQL/runtime belum dilakukan |

## 1. Masalah yang diperbaiki

Kategori bayi di layar belum menjadi konteks perawatan tersimpan. Tempat dewasa masih berpotensi dipakai bayi, hubungan episode ibu belum dicocokkan dengan master ibu, dan sumber kebutuhan intensif tidak tersimpan. Contoh: bayi Siti tidak boleh ditautkan ke episode ibu pasien lain atau ditempatkan pada bed dewasa walaupun request dikirim langsung.

## 2. Proses bisnis

1. Pendaftaran menyimpan atau menggunakan master bayi beserta MotherPatientId. Bayi mempunyai RM, kunjungan dan episode sendiri.
2. Admisi mengirim sumber HospitalBirth/External/Readmission, keputusan RoomingIn/Neonatal, sumber keputusan klinis, DPJP, preferensi penagihan dan rencana deposit.
3. Sebelum episode tersedia, pencarian bed memakai subject pasien yang dibaca server. Setelah Draft tersedia, metadata episode menjadi sumber keputusan server.
4. Rawat gabung memerlukan episode ibu yang cocok dengan master ibu dan kamar penempatan ibu yang masih aktif. Bayi dari luar tidak diwajibkan mempunyai episode ibu lokal.
5. Reservasi dan penempatan memanggil aturan bersama. Bayi wajib bed IsForNewborn; pasien dewasa ditolak pada boks bayi. Rencana intensif pada Draft memerlukan kemampuan intensif bed/kamar; isolasi tetap memakai aturan existing.
6. Penempatan pertama bayi memerlukan GeneralConsent Completed dan tanda tangan orang tua/wali. Operasi bed, versi, transaksi, status history dan outbox existing tetap dipakai.
7. Hubungan historis yang tidak berubah dipertahankan setelah ibu Closed; tautan baru rawat gabung tetap memerlukan ibu hadir. Setelah bayi Admitted, rencana awal admisi tidak menggantikan keputusan transfer klinis berikutnya; kategori boks bayi tetap ditegakkan.
8. Gelang dan label bayi kembar menambahkan urutan lahir dari master, misalnya BY. NY. SITI / BAYI 2. RM tetap pembeda utama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

AGENTS.md kedua repo, aturan backend/global, Backend Engineering Contract, Module Ownership & Prefix Registry, plan/issue 016, kontrak episode/bed/consent, InpEpisodeService dan partial-nya, InpBedOccupancyService dan coordinator existing, konfigurasi EF, snapshot/migration existing, PatientController/DTO/master, PatientProfileQueryService, serta query gelang dan frontend konsumen.

### 3.2 Berkas yang berubah pada task ini

Path pada tabel relatif terhadap Areas/HealthServices/InPatientManagement kecuali disebutkan berbeda.

| Berkas | Perubahan |
| --- | --- |
| DTOs/InpatientNewbornDtos.cs | Kontrak konteks bayi, validasi ukuran dan nominal |
| Services/InpNewbornAdmissionRules.cs | Konsistensi konteks, kamar ibu dan kelayakan bed bayi |
| Models/InpEpisode.cs | Enam field domain konteks bayi; bukan state stepper |
| DTOs/InpatientEpisodeDtos.cs | Konteks pada create/update/detail; filter pasien ibu dan RoomId pilihan ibu |
| Services/InpEpisodeService.cs | Validasi/persistence/projection konteks dan selected bed |
| Services/InpEpisodeService.Corrections.cs | Hubungan master ibu dengan episode; pengecualian referensi historis yang sama |
| Services/InpEpisodeService.Reads.cs | Pencarian ibu per MotherPatientId; ibu DischargePending yang belum berangkat masih dapat dibaca |
| DTOs/InpatientBedOccupancyDtos.cs | Subject dan kebutuhan pencarian sebelum episode |
| Services/InpBedOccupancyService.cs | Evaluasi sebelum episode; aturan bayi dua arah; persetujuan wali sebelum penempatan |
| DTOs/InpatientBedManagementDtos.cs | Filter IsForNewborn untuk monitoring/board |
| Services/InpBedManagementQueryService.cs | Filter boks pada query server |
| Services/InpBedOccupancyService.BedManagement.cs; Controllers/InpatientBedOccupancyController.cs | Teruskan filter board tanpa mengubah izin |
| Services/InpAdmissionWorkspaceQueryService.cs; Services/InpWristbandRules.cs | Urutan kelahiran pada gelang/label kembar |
| Services/InpatientAdmissionConsentService.cs | Alias Orang Tua dikenali sebagai Parent |
| Areas/HealthServices/PatientManagement/MasterData/Services/PatientProfileQueryService.cs | BirthOrder pada kontrak query profil yang direuse |
| Repositories/Configurations/HealthServices/InPatientManagement/InpEpisodeConfiguration.cs | Panjang teks, boolean default false, nominal numeric(18,2) |
| Migrations/20261014091502_AddNewbornAdmissionContext.cs; .Designer.cs; ApplicationDbContextModelSnapshot.cs | Enam kolom pada public.InpEpisode; Up/Down tidak membawa perubahan membership/HR lain |

### 3.3 Dampak kontrak, database dan keamanan

newbornAdmission opsional untuk kompatibilitas konsumen lama. Jika dikirim, master bayi/ibu, source, careMode, careSource, familyBillingPreference dan nominal divalidasi. Metadata tidak membuat settlement atau invoice baru. Semua endpoint tetap menggunakan AccessPermission existing; tidak menambah endpoint bypass. Validasi bayi ditegakkan di service transaksi, bukan hanya filter UI.

Migration disiapkan, **belum diterapkan**. AGENTS.md baris 165?167 memisahkan pembuatan file dari database update dan menyatakan pemilik menjalankan update sendiri. Designer/snapshot diselaraskan hanya untuk perubahan slice ini; perubahan model lain yang belum bermigrasi tidak ikut dimasukkan. SkipMigrationMetadata pada build final mengikuti instruksi pengguna, sehingga build final memang tidak mengompilasi Designer/snapshot; build normal sebelumnya sempat lulus, dan artefak migration final diperiksa statis.

## 4. Dokumentasi endpoint

### Health Services / Inpatient Management / Inpatient Episode

Base api/v1/health-services/inpatient-management/episodes.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| POST | / | Draft bayi dengan newbornAdmission dan DPJP | InpatientEpisode : Create |
| PUT | /{id} | Koreksi Draft; metadata opsional tidak menghapus konteks tersimpan | InpatientEpisode : Update |
| GET | /{id} | Detail dan pemulihan konteks | InpatientEpisode : Read |
| GET | /active-mothers | Ibu yang masih hadir, dapat difilter MotherPatientId | InpatientEpisode : Read |

### Health Services / Inpatient Management / Bed Occupancy

Base api/v1/health-services/inpatient-management/bed-occupancies.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | /available-beds | Query subject/kebutuhan sebelum episode atau metadata episode sesudahnya | InpatientBedOccupancy : Read |
| GET | /bed-board | Board terfilter IsForNewborn | InpatientBedOccupancy : Read |
| POST | /reservations | Reservasi dengan validasi bayi dan versi existing | InpatientBedOccupancy : Create |
| POST | /placements | Penempatan setelah consent orang tua/wali | InpatientBedOccupancy : Create |

### Health Services / Inpatient Management / Inpatient Admission Workspace

GET /episodes/{id}/admission-workspace/identity-labels mengikuti route existing, hak InpatientAdmissionDocument : Print; bentuk response existing dengan nama yang sekarang memuat urutan kembar. Tidak menambah endpoint dokumen.

## 5. Verifikasi

| Perintah/skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| dotnet build -p:SkipMigrationMetadata=true -p:RunAnalyzers=false -p:UseAppHost=false -p:OutputPath=<temp> --no-restore --verbosity quiet | 0 error, 279 warning, 1 menit 3,91 detik pada build final | PASS | Keluaran build 14 Oktober 2026; output terisolasi, backend berjalan tidak dihentikan |
| Build normal sebelum instruksi build terbaru | Pernah lulus 0 error; percobaan awal default output terbentur apphost terkunci | PASS / EXISTING / ENVIRONMENT ISSUE | Build output temp menyelesaikan benturan tanpa mematikan aplikasi pengguna |
| Review migration | AddColumn enam field InpEpisode dan Down enam DropColumn; snapshot delta 26 penambahan/1 perubahan newline | PASS | Artefak migration dan diff snapshot |
| Review jalur transaksi | Aturan dipanggil oleh shared evaluator; consent hanya pada placement pertama; query setelah episode memakai metadata server | PASS pada source | Services/InpBedOccupancyService.cs, InpEpisodeService.cs, InpNewbornAdmissionRules.cs |
| API tulis/PostgreSQL/DB update | Tidak dijalankan | NOT RUN | Tidak ada wewenang penerapan DB/runtime baru; backend pengguna tetap berjalan dengan binary lamanya |
| Test backend otomatis baru | Tidak dibuat | NOT REQUIRED | Mengikuti larangan test class/project baru pada kebijakan repository |

MANUAL TEST: NOT FEASIBLE untuk backend terintegrasi karena migration belum diterapkan pada lingkungan yang ditetapkan. Pengujian browser FE memakai API tiruan; bukan bukti PostgreSQL.

## 6. Acceptance criteria dan Definition of Done

| Kriteria roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Metadata tersimpan/terbaca dan tervalidasi | Terpenuhi pada source, belum dibuktikan pada database | Model/config/DTO, ValidateContext, ApplyContext dan proyeksi detail |
| 2. Relasi ibu/episode dan referensi historis konsisten | Terpenuhi pada source | ValidateMotherEpisodeAsync, FindMotherRoomAsync |
| 3. Kelayakan bayi/dewasa, rawat gabung, intensif/isolasi | Terpenuhi pada source | Shared evaluator pencarian/reservasi/placement/transfer; konteks rencana hanya Draft |
| 4. Pencarian sebelum/sesudah episode | Terpenuhi pada source | Pseudo context tidak dipersist; metadata episode tidak dioverride query browser |
| 5. Migration/Designer/snapshot dan build tanpa DB update | Terpenuhi | File migration tersedia; build final PASS; database tidak diterapkan |

DoD source dan build terpenuhi. Sign-off integrasi produksi belum diberikan; memerlukan penerapan migration terotorisasi dan UAT data nyata. Tidak menyatakan API runtime sudah memakai field baru.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning compiler/dependency existing termasuk ImageSharp; tidak melakukan upgrade paket |
| Masalah yang diketahui | Identitas ibu tidak dikenal belum mempunyai kontrak sementara; validasi menolak ibu fiktif. Metadata bukan mesin pembayaran lintas invoice |
| Risiko tersisa | Backend binary baru tidak boleh digunakan sebelum kolom migration tersedia. Kemampuan bed/kamar harus ditandai benar pada master, bukan mengandalkan nama ruang |
| Perubahan sampingan | Perubahan membership/HR yang sempat tergenerasi oleh EF dikeluarkan dari migration task; perubahan working tree pengguna pada file lain dipertahankan |
| Interupsi | Instruksi build pengguna diterapkan; sejumlah perubahan source lain muncul bersamaan dan tidak direvert |
| Langkah berikutnya | Pemilik menerapkan migration pada DB yang ditetapkan, menjalankan backend baru, lalu UAT relasi, bed, consent dan retry dengan data terkendali |

Status Git akhir pemeriksaan (termasuk perubahan eksternal yang dipertahankan):

~~~text
 M Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientBedManagementDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs
 M Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs
 M Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs
 M Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs
 M Areas/HealthServices/InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpBedManagementQueryService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.BedManagement.cs
 M Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Corrections.cs
 M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs
 M Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs
 M Areas/HealthServices/InPatientManagement/Services/InpWristbandRules.cs
 M Areas/HealthServices/InPatientManagement/Services/InpatientAdmissionConsentService.cs
 M Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs
 M Areas/HealthServices/PatientManagement/MasterData/Services/PatientProfileQueryService.cs
 M Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/HealthServices/InPatientManagement/InpEpisodeConfiguration.cs
 M Seeders/AppVersionSeeder.cs
?? Areas/HealthServices/InPatientManagement/DTOs/InpatientNewbornDtos.cs
?? Areas/HealthServices/InPatientManagement/Services/InpNewbornAdmissionRules.cs
?? Migrations/20261014091502_AddNewbornAdmissionContext.Designer.cs
?? Migrations/20261014091502_AddNewbornAdmissionContext.cs
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/contracts/newborn-admission-repair-016.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-014-layar-admisi-freeze-reset-idle.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-014-sinkronisasi-penanda-tangan-general-consent.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-015-scan-ktp-wilayah-tidak-terdeteksi.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-016-alur-admisi-episode-bayi.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-017-admisi-transfer-galat-server.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/issue/issue-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-014-layar-admisi-freeze-reset-idle.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-014-sinkronisasi-penanda-tangan-general-consent.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-015-scan-ktp-wilayah-tidak-terdeteksi.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-016-alur-admisi-episode-bayi.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-017-admisi-transfer-galat-server.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/docs/plan-repair/plan-repair-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap-bayi-016.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-bayi-016.md
?? docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/frontend/FIX-EPS-018.md
~~~

## Handoff source kepada pemilik - 15 Oktober 2026

Pengguna menginstruksikan implementasi kode saja; dotnet build dan penerapan migration dikerjakan sendiri oleh pengguna. Tidak ada perintah build, migration atau pengujian baru yang dijalankan setelah instruksi tersebut. Helper preview/test milik task sudah tidak berjalan. Pemeriksaan source/read-only memastikan implementasi pasien baru/lama serta tiga artefak migration tetap tersedia pada commit terbaru.

Scope source admisi selesai. Validasi build dan penerapan database berikutnya menjadi tanggung jawab pemilik sesuai instruksi terbaru; catatan kendala packaging tanggal 14 tetap dipertahankan sebagai riwayat dan bukan pekerjaan yang diteruskan agent. Settlement dua invoice tetap di luar slice source admisi ini; pilihan Combined adalah preferensi/handoff Billing.

Migration yang diserahkan: Migrations/20261014091502_AddNewbornAdmissionContext.cs, .Designer.cs, dan ApplicationDbContextModelSnapshot.cs. Enam kolom hanya pada public.InpEpisode; agent tidak menerapkan migration. Build sebelum instruksi pengalihan terakhir telah selesai 0 error/279 warning menggunakan flag pengguna dan output/intermediate terpisah; ini adalah bukti historis, bukan build baru sesudah instruksi.
