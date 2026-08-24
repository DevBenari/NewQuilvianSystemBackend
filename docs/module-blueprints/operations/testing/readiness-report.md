# Laporan Kesiapan Backend — Modul Operasi

| Field | Nilai |
| --- | --- |
| Blueprint | `operations`, revision 2, `approved` |
| Cakupan laporan | `BE-OPR-001` sampai `BE-OPR-011` (backend saja) |
| Backend commit yang diaudit | `767470f742bc6f2eebadbd653a873f69d6f93121`, cabang `Ikbal` |
| Frontend commit yang diaudit | `400104f2a0f3239c14c40f5905b419977a538450` |
| Kontrak terkunci | `opr-api-v1`, `opr-state-v1`, `opr-integration-v1`, `opr-validation-v1`, `opr-permission-v1` |
| Verdict | `NOT_READY` untuk dipakai pengguna; `READY_FOR_REVIEW` untuk tinjauan kode dan otorisasi migration |

> **Peringatan bukti.** Seluruh source modul Operasi masih berupa perubahan kerja yang
> **belum di-commit** pada cabang `Ikbal`. Commit SHA di atas adalah keadaan repository
> **sebelum** modul ini ditulis. Selama belum di-commit, setiap klaim di laporan ini hanya
> dapat diverifikasi pada direktori kerja, bukan pada riwayat git. Ini blocker nomor 1.

---

## 1. Ringkasan untuk pemilik proses

Modul Operasi menangani perjalanan satu kasus operasi dari permintaan dokter sampai pasien
diserahterimakan ke unit tujuan. Sebelas task backend yang direncanakan sudah ditulis dan
lulus pengujian otomatis. Artinya: aturan bisnisnya sudah berjalan di dalam kode.

Yang **belum** terjadi, dan karena itu modul belum boleh dipakai perawat atau dokter:

1. Tabel-tabelnya belum pernah dibuat di database mana pun. Migration sudah ditulis tetapi
   sengaja belum dijalankan, karena menjalankan migration memerlukan izin terpisah.
2. Belum ada satu pun layar frontend yang memakai modul ini.
3. Tiga sambungan ke modul lain — Farmasi/Inventory, Billing, dan unit tujuan pasien —
   belum punya pemilik yang ditunjuk, sehingga pengirimannya berhenti di antrean.

| Aspek | Kesiapan | Dasar perhitungan |
| --- | --- | --- |
| Fondasi (entity, configuration, migration) | 13 dari 13 entity terbukti | Seluruh `Opr*` punya model, configuration, dan migration |
| Backend | 11 dari 11 task ditulis | `BE-OPR-001` sampai `BE-OPR-011` |
| Frontend | 0 dari 9 task | `FE-OPR-001` sampai `FE-OPR-009` belum dimulai |
| Integrasi dan runtime | 0 dari 6 terbukti | Belum ada eksekusi database dan belum ada consumer nyata |
| Cakupan pengujian | 126 test, 0 gagal | 114 test modul Operasi, 12 test modul Farmasi |

---

## 2. Proses bisnis yang sudah berjalan di kode

### 2.1 Tujuan

Memastikan satu kasus operasi hanya berpindah status ketika syarat keselamatan pasien benar-benar
terpenuhi, dan setiap perpindahan itu tercatat siapa pelakunya.

### 2.2 Pelaku

| Pelaku | Kewenangan yang ditegakkan sistem |
| --- | --- |
| Dokter pemohon | Membuat dan memperbaiki permintaan operasi |
| Koordinator kamar operasi | Menetapkan jadwal, ruang, tim, menunda, dan menjadwalkan ulang |
| Dokter bedah utama | Sign-off kesiapan, memulai operasi, mengisi dan memfinalisasi catatan operasi |
| Dokter anestesi | Sign-off kesiapan, catatan anestesi, keputusan keluar recovery |
| Perawat kamar operasi | Sign-off kesiapan, checklist keselamatan, pencatatan material |
| Unit tujuan pasien | Menerima serah terima pasien |

### 2.3 Langkah utama

1. Dokter mengirim permintaan operasi. Status menjadi `Requested`.
2. Koordinator menetapkan ruang, waktu, dan tim minimum. Status menjadi `Scheduled`.
3. Tim melengkapi consent, checklist keselamatan, dan tiga sign-off. Sistem sendiri yang
   menaikkan status menjadi `Ready`.
4. Dokter bedah utama memulai operasi. Status menjadi `In Progress`.
5. Tim mengisi catatan operasi, catatan anestesi, dan pemakaian material.
6. Catatan operasi difinalisasi, pasien keluar recovery, dan serah terima diterima unit
   tujuan. Sistem sendiri yang menaikkan status menjadi `Completed`.

### 2.4 Perubahan status yang ditegakkan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Kirim permintaan | `Requested` | Dokter pemohon | Satu tindakan utama dan data minimum lengkap |
| `Requested` | Tetapkan jadwal | `Scheduled` | Koordinator | Ruang, waktu, dan tim minimum tanpa benturan |
| `Requested` / `Scheduled` | Tunda | `Postponed` | Koordinator | Alasan wajib dan dikonfirmasi dokter aktif |
| `Postponed` | Jadwalkan ulang | `Scheduled` | Koordinator | Jadwal baru valid dan beralasan |
| `Scheduled` | Lengkapi kesiapan | `Ready` | Sistem | Tiga sign-off, serta consent dan checklist sah atau jalur darurat sah |
| `Ready` | Mulai operasi | `In Progress` | Dokter bedah utama | Identitas pasien dan tindakan dikonfirmasi |
| `In Progress` | Tutup kasus | `Completed` | Sistem | Catatan operasi final, pasien keluar recovery, serah terima diterima |
| `Requested` / `Scheduled` / `Ready` | Batalkan | `Cancelled` | Dokter bedah atau dokter anestesi | Alasan klinis wajib |

Pembatalan setelah operasi dimulai **ditolak**. Penghentian di tengah operasi dicatat sebagai
hasil `StoppedEarly`, dan kasus tetap berjalan menuju `Completed`.

### 2.5 Contoh aturan berangka

**Contoh A — benturan jadwal karena buffer.**

> Kasus pertama dijadwalkan di OK 1 pukul 08.00–09.00. Buffer pembersihan yang dikonfigurasi
> adalah 30 menit, sehingga OK 1 baru bebas pukul 09.30. Ketika koordinator mencoba menjadwalkan
> kasus kedua di OK 1 pukul 09.20, sistem menolak dengan kode `OPR003` dan pesan
> "Ruang atau anggota tim sudah memiliki jadwal pada waktu tersebut." Bila kasus kedua digeser
> ke pukul 12.00, penjadwalan berhasil.

**Contoh B — gerbang kesiapan.**

> Checklist sebelum anestesi sudah selesai dan consent operasi serta anestesi sah. Dokter bedah
> memberi sign-off, lalu dokter anestesi. Status masih `Scheduled` karena sign-off perawat belum
> ada. Begitu perawat memberi sign-off ketiga, sistem langsung menaikkan status ke `Ready` dan
> menulis **satu** baris riwayat perpindahan status, bukan tiga.

**Contoh C — jalur darurat.**

> Pasien dengan perdarahan aktif dijadwalkan sebagai kasus darurat. Consent tertulis belum ada.
> Penanggung jawab mencatat jalur darurat beserta alasannya. Syarat consent dan checklist
> digugurkan, tetapi tiga sign-off **tetap wajib**. Setelah pasien stabil, checklist dilengkapi
> dan sistem mencatat waktu pelengkapannya.

### 2.6 Jalur tidak normal yang sudah ditangani

| Kejadian | Yang terjadi | Yang dilihat pengguna |
| --- | --- | --- |
| Tombol Simpan ditekan dua kali | Hanya satu data tersimpan | Hasil yang sama dikembalikan, tanpa data ganda |
| Kunci pengulangan dipakai dengan isi berbeda | Permintaan ditolak | "Permintaan tidak dapat diverifikasi sebagai permintaan yang sama." (`OPR013`) |
| Dua petugas mengubah kasus yang sama | Perubahan kedua ditolak | "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali." (`OPR012`) |
| Tim minimum belum lengkap | Penjadwalan ditolak | "Lengkapi dokter bedah, dokter anestesi, perawat instrumen, dan perawat sirkuler." (`OPR004`) |
| Kewenangan klinis diblokir | Penjadwalan ditolak | "Anggota tim tidak aktif atau tidak memiliki kewenangan yang sesuai." (`OPR005`) |
| Catatan operasi sudah final lalu diubah | Perubahan ditolak | "Catatan final hanya dapat diperbaiki melalui addendum." (`OPR010`) |
| Serah terima belum diterima unit tujuan | Kasus tidak ditutup | "Serah terima pasien belum diterima unit tujuan." (`OPR011`) |
| Pengiriman ke Billing/Inventory gagal | Operasi tetap tersimpan | Pengiriman tercatat `Pending`/`Failed` dan dapat diulang tanpa menggandakan |

---

## 3. Dokumentasi API

Seluruh endpoint memerlukan pengguna yang sudah masuk. Kode status mengikuti `opr-api-v1`:
`200` berhasil, `400` isian kurang atau salah format, `401` belum masuk, `403` tidak berwenang,
`404` kasus tidak ditemukan, `409` benturan atau perpindahan status ilegal, `422` aturan klinis
belum terpenuhi.

### Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kasus dengan penyaringan dan halaman | `OperatingRoomCase : Read` | `BE-OPR-003` |
| `GET` | `/{id}` | Detail satu kasus operasi | `OperatingRoomCase : Read` | `BE-OPR-003` |
| `POST` | `/` | Membuat permintaan operasi | `OperatingRoomCase : Create` | `BE-OPR-003` |
| `PUT` | `/{id}` | Memperbaiki permintaan sebelum dijadwalkan | `OperatingRoomCase : Update` | `BE-OPR-003` |
| `GET` | `/{id}/schedule` | Melihat jadwal dan tim yang berlaku | `OperatingRoomCase : Read` | `BE-OPR-004` |
| `PATCH` | `/{id}/schedule` | Menetapkan atau merevisi jadwal dan tim | `OperatingRoomSchedule : Update` | `BE-OPR-004` |
| `PATCH` | `/{id}/postpone` | Menunda kasus | `OperatingRoomSchedule : Update` | `BE-OPR-004` |
| `PATCH` | `/{id}/start` | Memulai operasi | `OperatingRoomExecution : Update` | `BE-OPR-006` |
| `PATCH` | `/{id}/cancel` | Membatalkan sebelum operasi dimulai | `OperatingRoomCase : Cancel` | `BE-OPR-006` |

### Health Services / Operating Room Management / Preparation

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat consent, checklist, sign-off, dan prasyarat tersisa | `OperatingRoomPreparation : Read` | `BE-OPR-005` |
| `PUT` | `/checklists/{phase}` | Menyimpan checklist keselamatan per fase | `OperatingRoomPreparation : Update` | `BE-OPR-005` |
| `POST` | `/sign-offs` | Memberikan sign-off kesiapan | `OperatingRoomPreparation : Update` | `BE-OPR-005` |
| `POST` | `/emergency-bypass` | Mencatat jalur darurat | `OperatingRoomPreparation : Update` | `BE-OPR-005` |

### Health Services / Operating Room Management / Execution

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/operation-record` | Membaca catatan operasi beserta addendum | `OperatingRoomCase : Read` | `BE-OPR-006` |
| `PUT` | `/operation-record` | Menyimpan atau memfinalisasi catatan operasi | `OperatingRoomExecution : Update` | `BE-OPR-006` |
| `POST` | `/operation-record/addenda` | Menambah koreksi pada catatan yang sudah final | `OperatingRoomExecution : Update` | `BE-OPR-006` |
| `GET` | `/anesthesia-record` | Membaca catatan anestesi | `OperatingRoomAnesthesia : Read` **(di luar kontrak)** | `BE-OPR-007` |
| `PUT` | `/anesthesia-record` | Menyimpan atau memfinalisasi catatan anestesi | `OperatingRoomAnesthesia : Update` | `BE-OPR-007` |
| `GET` | `/recovery` | Membaca pemantauan recovery | `OperatingRoomAnesthesia : Read` **(di luar kontrak)** | `BE-OPR-007` |
| `PUT` | `/recovery` | Menyimpan pemantauan dan keputusan recovery | `OperatingRoomAnesthesia : Update` | `BE-OPR-007` |
| `GET` | `/handovers` | Membaca daftar serah terima | `OperatingRoomHandover : Read` **(di luar kontrak)** | `BE-OPR-007` |
| `POST` | `/handovers` | Mengirim serah terima ke unit tujuan | `OperatingRoomHandover : Update` | `BE-OPR-007` |
| `PATCH` | `/handovers/{handoverId}/accept` | Unit tujuan menerima serah terima | `OperatingRoomHandover : Update` | `BE-OPR-007` |
| `GET` | `/materials` | Membaca pemakaian material dan implant | `OperatingRoomMaterial : Read` **(di luar kontrak)** | `BE-OPR-008` |
| `POST` | `/materials` | Mencatat pemakaian, retur, waste, atau koreksi | `OperatingRoomMaterial : Update` | `BE-OPR-008` |

### Health Services / Operating Room Management / Integration

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/integration`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/reconciliation` | Melihat status pengiriman ke modul lain | `OperatingRoomIntegration : Read` **(di luar kontrak)** | `BE-OPR-009` |
| `PATCH` | `/deliveries/{deliveryId}/attempts` | Mencatat hasil percobaan pengiriman | `OperatingRoomIntegration : Update` | `BE-OPR-009` |
| `PATCH` | `/deliveries/{deliveryId}/retry` | Mengulang pengiriman yang gagal | `OperatingRoomIntegration : Update` | `BE-OPR-009` |

### Health Services / Operating Room Management / Reports

Base URL: `api/v1/health-services/operating-room-management/reports`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/operations` | Laporan kasus, tindakan, durasi, dan status | `OperatingRoomCase : Read` | `BE-OPR-010` |
| `GET` | `/utilization` | Pemakaian ruang dan penundaan | `OperatingRoomCase : Read` | `BE-OPR-010` |
| `GET` | `/materials` | Penelusuran material dan implant | `OperatingRoomMaterial : Read` **(di luar kontrak)** | `BE-OPR-010` |

---

## 4. Penyimpangan dari kontrak yang disetujui

Bagian ini sengaja dibuat menonjol. Ketiganya perlu keputusan pemilik sebelum modul disahkan.

| No | Penyimpangan | Alasan implementasi | Yang dibutuhkan |
| ---: | --- | --- | --- |
| 1 | Tiga sign-off kesiapan disimpan sebagai baris `OprStatusHistory` dengan `Action = "ReadinessSignOff"`, bukan tabel tersendiri | `opr-api-v1` mensyaratkan `POST /sign-offs`, tetapi ERD yang disetujui tidak memuat tabel sign-off. `OprSafetyChecklist` hanya menyediakan satu penanda tangan per fase, sehingga tiga sign-off tidak muat | Keputusan pemilik sudah diambil pada 2026-08-24 memilih opsi ini. Perlu dicatat resmi di decision log dan data dictionary |
| 2 | Enam endpoint `GET` memakai permission baca yang belum ada di `opr-permission-v1`: `OperatingRoomAnesthesia : Read`, `OperatingRoomMaterial : Read`, `OperatingRoomHandover : Read`, `OperatingRoomIntegration : Read` | Frontend perlu memuat ulang satu bagian tanpa menarik seluruh workspace kasus. Permission dibuat lebih ketat daripada `OperatingRoomCase : Read`, bukan lebih longgar | Revisi `opr-permission-v1` dan `opr-api-v1`, lalu persetujuan security owner |
| 3 | Item wajib checklist divalidasi dari isi permintaan, bukan dari master template | Blueprint melarang hardcode master checklist dan master-nya memang belum ditetapkan | Master template checklist perlu ditetapkan; setelah itu validasi harus pindah ke sisi server |

Penyimpangan nomor 2 dijaga oleh pengujian otomatis: permission di luar kontrak hanya boleh
dipakai endpoint `GET`, dan daftarnya harus benar-benar terpakai. Bila ada yang mencoba
memakainya pada endpoint yang mengubah data, pengujian gagal.

---

## 5. Blocker, diurutkan dari yang paling berdampak

| No | Blocker | Dampak nyata bila modul dipakai sekarang |
| ---: | --- | --- |
| 1 | Seluruh source modul belum di-commit | Pekerjaan bisa hilang, tidak bisa direview, dan tidak bisa ditelusuri siapa mengubah apa |
| 2 | Migration `AddOperatingRoomFoundation` belum dijalankan di lingkungan mana pun | Setiap endpoint modul ini akan gagal begitu dipanggil di luar komputer developer, karena tabelnya belum ada |
| 3 | Belum ada layar frontend | Dokter, koordinator, dan perawat tidak punya cara memakai modul ini |
| 4 | Owner API Billing dan Inventory belum ditetapkan | Pemakaian material dan tagihan tindakan berhenti di antrean pengiriman dan tidak pernah sampai ke modul keuangan maupun stok |
| 5 | Resolver item Inventory belum tersedia | Item yang dicatat tim operasi tidak dapat dipastikan benar-benar ada di master barang |
| 6 | Consumer serah terima Rawat Inap/ICU belum tersedia | Kasus tidak akan pernah mencapai `Completed` di lingkungan nyata, karena tidak ada unit yang menekan tombol terima |
| 7 | Belum ada pengujian ujung ke ujung terhadap database sungguhan | Perilaku unique index, filtered index, dan concurrency token baru terbukti pada database dalam memori |

Blocker 4, 5, dan 6 adalah dependency eksternal yang memang sudah ditandai `BLOCKED` pada
roadmap. Ketiganya bukan kegagalan implementasi, tetapi tetap menghalangi pemakaian nyata.

---

## 6. Bukti verifikasi

| Klaim | Bukti |
| --- | --- |
| Build backend bersih | `dotnet build QuilvianSystemBackend.sln` — 0 error, 0 warning |
| Seluruh pengujian lulus | `dotnet test` — 126 lulus, 0 gagal, 0 dilewati |
| Perpindahan status hanya satu kali per status | `QuilvianSystemBackend.Tests/HealthServices/OperatingRoomManagement/OperatingRoomHardeningTests.cs` + `FullLifecycle_FromScheduledToCompleted_ProducesOneTransitionPerStatus` |
| Kasus `Completed` menolak perintah lanjutan | file yang sama + `CompletedCase_RejectsFurtherLifecycleCommands` |
| Setiap endpoint punya permission dari matrix | file yang sama + `EveryEndpoint_DeclaresPermissionFromApprovedMatrix` |
| Permission di luar kontrak hanya pada endpoint baca | file yang sama + `PendingPermissions_AreOnlyUsedByAdditiveReadEndpoints` |
| Setiap controller wajib login dan mendeklarasikan modul | file yang sama + `EveryController_RequiresAuthenticationAndDeclaresModule` |
| Benturan ruang dan tim termasuk buffer | `OperatingRoomSchedulingServiceTests.cs` + `ScheduleAsync_RoomAlreadyBookedWithinBuffer_RejectsOpr003`, `ScheduleAsync_TeamMemberBookedInOtherRoom_RejectsOpr003` |
| Gerbang kesiapan dan jalur darurat | `OperatingRoomPreparationServiceTests.cs` + `ThreeSignOffsAfterChecklist_MovesCaseToReadyExactlyOnce`, `EmergencyBypass_WaivesConsentAndChecklistButNotSignOffs` |
| Catatan final hanya dapat dikoreksi lewat addendum | `OperatingRoomExecutionServiceTests.cs` |
| Serah terima wajib diterima sebelum `Completed` | `OperatingRoomRecoveryServiceTests.cs` |
| Pengulangan pengiriman tidak menggandakan pemakaian | `OperatingRoomMaterialServiceTests.cs`, `OperatingRoomIntegrationServiceTests.cs` |
| Configuration FK, unique index, dan concurrency token | `OperatingRoomModelConfigurationTests.cs` |

Seluruh path di atas relatif terhadap repository `NewQuilvianSystemBackend` pada direktori
kerja cabang `Ikbal`. Commit SHA belum dapat dicantumkan karena berkasnya belum di-commit.

---

## 7. Yang sengaja tidak dikerjakan

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Menjalankan migration ke database | Roadmap `BE-OPR-002` menyatakan eksekusi migration memerlukan otorisasi terpisah |
| Commit dan push | Belum diminta pemilik pekerjaan |
| Adapter nyata ke Billing dan Inventory | Owner API kedua modul belum ditetapkan; yang dibuat baru catatan pengiriman lokal yang idempotent |
| Master template checklist dan master skor recovery | Blueprint melarang menetapkan master yang belum disahkan rumah sakit |
| Seluruh task frontend | Di luar cakupan sebelas task backend ini |

---

## 8. Langkah berikutnya yang disarankan

1. Commit seluruh perubahan modul Operasi agar dapat direview dan ditelusuri.
2. Tutup tiga penyimpangan pada bagian 4 melalui revisi kontrak dan persetujuan owner.
3. Beri otorisasi eksekusi migration di lingkungan uji, lalu ulangi verifikasi terhadap
   database sungguhan.
4. Tetapkan owner API Billing, Inventory, dan unit tujuan serah terima.
5. Mulai `FE-OPR-001` setelah endpoint kasus terbukti berjalan di lingkungan uji.
6. Jalankan `/qv-verify` ulang setelah langkah 1 sampai 3 selesai.
