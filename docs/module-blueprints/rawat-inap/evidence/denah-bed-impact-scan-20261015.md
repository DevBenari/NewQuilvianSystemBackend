# BM-IMP-20261015-01 — Impact scan Integrasi Denah ↔ Bed Management

Tanggal: 15 Oktober 2026. Revision `1`, status **draft, source-audited**. Blueprint `RWI-BP-001`,
sub-modul `episode-rawat-inap`. Kontrak target Bed Management `0.12.0` draft (manifest anak,
`approved_by: null`). Masukan keputusan: `00-interview-decisions.md` revision `48`, yaitu
`RWI-DEC-298`–`316`, `RWI-FACT-071`–`072`, `RWI-OQ-141`–`144`, dan `RWI-AC-436`–`454`. Dokumen ini
tidak mengubah approval mana pun dan tidak merancang arsitektur target.

> **Cara membaca.** Dokumen ini menjawab satu pertanyaan: **apa yang sudah ada di sistem hari ini
> untuk menjalankan integrasi denah, dan sejauh mana itu dapat dipakai ulang.** Setiap baris punya
> bukti berupa repository, berkas, baris, dan commit. Keputusan bisnis yang masih dibutuhkan
> dikumpulkan di bagian 8.

## 1. Identitas, masukan, dan batas audit

| Field | Nilai |
| --- | --- |
| Pemicu | Pass wawancara Integrasi Denah tuntas (revision `48`). Peta kemampuan dan bukti `BM-IMP-20261011-01` dibuat pada commit lama, sehingga perlu impact scan sebelum desain |
| Masukan bisnis | [Decision Log](../00-interview-decisions.md) revision `48`; [rencana integrasi denah](../episode-rawat-inap/docs/denah-map-ruangan-rs-mmc/rencana-integrasi-denah-bed.md); [rencana kelas kamar](../episode-rawat-inap/docs/denah-map-ruangan-rs-mmc/rencana-kelas-kamar.md). Dua berkas pertama belum di-commit; hash byte dan teks LF-nya ada di [snapshot JSON](./denah-bed-source-snapshot-20261015.json) |
| Backend | `NewQuilvianSystemBackend` branch `MHamzah`, commit **`ca09cae909597442385ce4513be9c803549be10e`** (`BE@ca09cae`). Berjarak 192 commit dari snapshot `BM-IMP-20261011-01` (`c5d3b5bf`). Dari basis rencana `5f0c1412` hanya satu path non-dokumen yang berubah, yaitu daftar `tools/authorization-verifier/approved-compatibility-fallback.txt` (baris `PatientMembership`), yang tidak menyentuh bed. **Selama audit, pekerjaan lain berjalan paralel di working tree:** `InpBedOccupancyService.cs` (+10 baris penyaring unit registrasi), `PatientEncounterController.cs`, berkas baru `ServiceUnitRegistrationRules.cs`, serta dokumen issue dan plan-repair 022 alur admisi bayi. Pekerjaan itu bukan bagian audit ini dan tidak disentuh. Seluruh bukti backend dibaca dari `HEAD`, dan nomor baris mengacu `HEAD`; satu dampaknya dicatat sebagai pengamatan working tree pada `DNH-CAP-08` |
| Frontend | `QuilvianSystemFrontendDev` branch `HamzahV2`, commit **`4d9bf608dbbaddc96a2b4acfc8723753b5dd5028`** (`FE@4d9bf60`). Berjarak 140 commit dari snapshot `5aa2c7c7`. **Selama audit, pemilik sedang mengubah 8 berkas alur admisi** (antara lain `inpatient-admission-view.jsx`, `inpatient-newborn-admission-steps.jsx`, `use-inpatient-admission-bed.jsx`). Tiga di antaranya dikutip di sini; baris impor papan bed, langkah Pilih Bed, dan endpoint reservasi pada ketiganya tidak berubah. Seluruh bukti frontend dibaca dari `HEAD` |
| Klaster yang diaudit | Identity/Master (kelas, kamar, bed, unit layanan); Episode (tingkat ketergantungan, isolasi); Location/Resource (lantai, wing, kunci denah, kode); Workflow/Status (enam status bed, status kamar, kesiapan); Authorization/Audit (cakupan unit, penyamaran identitas, jalur mutasi master); Documentation (kategori General Consent); Financial (tarif kamar, deposit, cakupan penjamin); pembersihan data (relasi episode); konsumen frontend |
| Di luar audit | Isi database (kelas, unit, kamar, tarif, dan jumlah baris yang tersimpan); aplikasi berjalan dan UAT; role dan permission di database; SOP RS. Nilai konfigurasi tidak dicetak, hanya nama key dan jenis nilainya |
| Cara audit | Pembacaan statis dengan `rg` lalu membaca implementasi: model, enum, konfigurasi Entity Framework (model snapshot), controller beserta `[Tags]` dan `[AccessPermission]`, service, registrasi DI di `Program.cs`, opsi konfigurasi, service/hook/komponen frontend, dan inventaris test. Relasi antar-tabel dihitung dari `Migrations/ApplicationDbContextModelSnapshot.cs`. **Tidak ada build, test, migration, maupun eksekusi aplikasi atau database** |
| Batas tulis | Dokumen ini, snapshot JSON-nya, catatan `STALE` pada `BM-IMP-20261011-01`, dan baris penunjuk di manifest modul, decision log, serta rencana integrasi. Tidak ada satu baris source aplikasi yang diubah |

**Status `BM-IMP-20261011-01`.** Bukti itu terikat `c5d3b5bf` dan **tidak** diaudit ulang seluruhnya.
Lima klaimnya terbukti sudah berubah, sehingga butir di bawah ini dinyatakan `STALE`:

| Klaim 11 Oktober | Keadaan pada `BE@ca09cae` / `FE@4d9bf60` |
| --- | --- |
| `BM-CAP-07` Missing: data kesiapan bed belum ada | Ada: `InpBedReadiness` dengan lima keadaan (`Areas/HealthServices/InPatientManagement/Enums/InpBedReadinessState.cs`), service `InpBedReadinessService` terdaftar di `Program.cs:716` |
| `BM-CAP-09` Missing: Housekeeping mulai/selesai dan verifikasi perawat | Ada endpoint `cleaning-attempts` dan `readiness-verifications` (`InpatientBedManagementController.cs:79-108`), dipakai `FE use-inpatient-bed-cleaning.js:40, 110-135` |
| `BM-CAP-08` Repair: bed bekas pasien langsung Available | Pelepasan hunian memanggil `InvalidateAsync` dengan penempatan yang dilepas (`InpBedOccupancyService.cs:1944`), sehingga kesiapan menjadi `WaitingCleaning` (`InpBedMutationCoordinator.cs:212`) |
| `BM-CAP-14` Repair: alasan pembatalan tidak disimpan | Disimpan di `InpBedReservation.cs:14 #CancellationReason`; request `CancelReservationRequest.Reason` maksimum 500 karakter (`InpatientBedOccupancyDtos.cs:279-283`) |
| `S11`: menu "Papan Tempat Tidur", belum tiga tab | Menu "Manajemen Tempat Tidur" (`FE menu-items.jsx:1797-1801`) dengan tiga tab Pemantauan, Transfer, dan Riwayat Penggunaan (`bed-management-constants.js:4-6`) |

Butir `BM-CAP` lain tidak diperiksa ulang di sini. Status runtime Bed Management tetap seperti roadmap:
`BE-RWI-208`–`221` 🟡 (source dan migration tersedia, *database update* lulus 12 Oktober, runtime
`NOT_RUN`), `FE-RWI-231`–`237` ✅.

## 2. Ringkasan untuk pembaca umum

Bayangkan petugas admisi Sari akan menempatkan Tn. Budi Santoso (samaran) di kamar Kelas 2 329,
Lantai 3. Begini keadaan sistem hari ini bila denah dijadikan satu-satunya tampilan bed:

1. **Mesin bed-nya sudah ada.** Enam status bed (Tersedia, Terisi, Dipesan, Menunggu Pembersihan,
   Dalam Pembersihan, Tidak Tersedia), pesanan 120 menit, pembersihan oleh Housekeeping, pengesahan
   perawat, transfer satu langkah, dan pemeriksaan kelayakan per pasien semuanya sudah ada di source.
   Denah tinggal membaca data yang sama, bukan membangun ulang.
2. **Yang belum ada untuk denah:**
   - kolom lantai terstruktur, wing delapan arah, dan kunci yang menghubungkan kamar dengan gambar;
   - status kamar beserta badge "kapasitas/terisi" yang dihitung dari bed;
   - tingkat ketergantungan High/Low Dependency beserta riwayatnya;
   - importer susunan kamar dan bed dari denah.
3. **Tiga perilaku source berbenturan dengan rencana:**
   - **Kode bed.** Setiap bed baru otomatis diberi kode `BD-RSMMC-00042`, bukan `329-A`.
   - **Kapasitas.** Mengubah kapasitas kamar dianggap perubahan berbahaya, sehingga semua bed di kamar
     itu harus kosong. Menyelaraskan kapasitas saat menambah bed di kamar berpenghuni akan ditolak.
   - **Bed baru.** Bed yang baru dibuat berstatus "belum diverifikasi" dan tampil Tidak Tersedia sampai
     perawat mengesahkannya. Bila 168 bed diimpor, semuanya tampil Tidak Tersedia.
4. **Mengganti daftar kelas menyentuh banyak modul.** Kelas pasien dipakai 10 modul, termasuk tarif
   kamar, kebijakan deposit, dan aturan cakupan penjamin. Perbandingan Down/Up Grade juga berhenti
   (kode `CLASS_ORDER_UNVERIFIED`) setiap kali Id atau urutan kelas aktif berubah, sampai konfigurasi
   urutan kelas diperbarui. Saat ini belum ada jalur "kelas khusus bebas memilih kategori".
5. **Unit layanan per lantai menyentuh tarif dan hak akses.** Tarif kamar dicocokkan per unit layanan
   **dan** kelas. Unit baru per lantai belum punya tarif, dan setiap petugas perlu penugasan unit di
   konfigurasi.
6. **Jenis kelamin tersebar di banyak tempat.** Penghapusannya menyentuh kolom master, DTO, filter,
   ringkasan, validasi "minimal satu jenis kelamin", dua kode kelayakan, isian master di frontend, dan
   tujuh berkas test.
7. **Pembersihan data rawat inap tidak sederhana.** Ada 123 tabel di 10 modul yang bergantung pada
   episode rawat inap lewat relasi database. Data yang terikat lewat kunjungan dan Billing butuh
   pembacaan katalog database.

Dari 41 kemampuan yang diaudit: **4** `Ready to reuse`, **17** `Reuse with adapter`, **3** `Extend`,
**5** `Repair`, **7** `Missing`, **3** `Conflict`, dan **2** `Unknown`.

## 3. Proses as-is yang ditelusuri

### 3.1 Memilih dan memakai bed hari ini

| Unsur | Isi |
| --- | --- |
| Tujuan | Pasien mendapat bed yang siap, sesuai kelas, dan lolos aturan kelayakan |
| Pelaku | Petugas admisi memesan; petugas berwenang menempatkan dan memindahkan; Housekeeping membersihkan; perawat ruangan yang ditugaskan mengesahkan kesiapan; Admin MasterData mengubah master |
| Pemicu | Langkah Pilih Bed pada admisi (termasuk admisi bayi), transfer, atau perubahan master |
| Prasyarat | Kamar punya kelas aktif dan unit aktif; bed punya data kesiapan berstatus Ready; pengguna punya hak dan penugasan unit |

Langkah utama hari ini:

1. Layar memanggil `GET bed-occupancies/available-beds` dengan `episodeId` atau data pasien. Bila
   `includeIneligible=true`, server juga mengembalikan bed yang ditolak beserta alasannya.
2. Petugas memilih bed pada papan kartu (`inpatient-bed-board.jsx`).
3. Petugas memesan bed (`POST reservations`). Pesanan berlaku 120 menit.
4. Pasien tiba, lalu petugas menempatkannya (`POST placements`).
5. Saat pasien pulang atau pindah, bed lama otomatis menjadi Menunggu Pembersihan.
6. Housekeeping memulai dan menyelesaikan pembersihan, lalu perawat mengesahkan bed siap.

**Aturan kelayakan yang berjalan** (`InpBedOccupancyService.cs:1595` `#EvaluatePlacementEligibilityAsync`):

| Kode | Arti bagi petugas | Contoh |
| --- | --- | --- |
| `ROOM_CLASS_MISSING` | Kamar belum punya kelas | Ruang Bayi tanpa kelas tidak bisa dipesan |
| `BED_NOT_READY` | Bed belum siap atau masih dipegang | Bed baru yang belum diverifikasi |
| `BED_GENDER_MISMATCH`, `PATIENT_GENDER_UNKNOWN` | Penanda bed tidak menerima jenis kelamin pasien | Akan dihapus (`RWI-DEC-315`) |
| `ISOLATION_REQUIRED`, `ISOLATION_BED_RESERVED` | Isolasi dua arah | Pasien isolasi hanya ke Isolasi 319 |
| `NEWBORN_BED_REQUIRED`, `NEWBORN_BED_ONLY`, `NEWBORN_ROOMING_IN_MISMATCH`, `NEWBORN_INTENSIVE_REQUIRED` | Aturan boks bayi | Pasien dewasa ditolak di boks: "Tempat ini khusus pasien bayi" |

### 3.2 Perubahan status bed yang sudah ada di source

| Dari | Tindakan | Ke | Siapa | Bukti |
| --- | --- | --- | --- | --- |
| (bed baru) | Admin membuat bed | Tidak Tersedia (kesiapan `Unverified`) | Admin MasterData | `InpBedMutationCoordinator.cs:149-156`; `InpBedReadiness.cs:11` |
| Tidak Tersedia (`Unverified`) | Sahkan siap | Tersedia | Perawat yang ditugaskan sebagai verifikator | `InpBedManagementQueryService.cs:179-185`; `InpBedAccessService.cs:152-158` |
| Tersedia | Pesan bed | Dipesan | Petugas admisi | `POST reservations` |
| Dipesan | Tempatkan pasien | Terisi | Petugas berwenang | `POST placements` |
| Terisi | Pasien pulang/pindah | Menunggu Pembersihan | Sistem | `InpBedOccupancyService.cs:1944` |
| Menunggu Pembersihan | Mulai | Dalam Pembersihan | Housekeeping | `POST cleaning-attempts` |
| Dalam Pembersihan | Selesai, lalu disahkan | Tersedia | Housekeeping, lalu perawat | `PATCH …/complete`, `POST readiness-verifications` |
| Tersedia | **Perubahan master berbahaya** (kelas, unit, kapasitas, penanda, aktif) | Tidak Tersedia (`Unverified`) atau Menunggu Pembersihan | Admin MasterData, dengan alasan | `BedMutationService.cs:63-69, 92-99`; `InpBedMutationCoordinator.cs:207-215` |

**Contoh.** Admin mengubah kamar 505 dari Kelas 3 menjadi Kelas 2 pada pukul 10:00. Keenam bednya
kosong, sehingga perubahan diterima dengan alasan "Renovasi selesai". Sejak saat itu keenam bed tampil
Tidak Tersedia sampai perawat mengesahkan kesiapannya satu per satu. Bila satu bed masih dihuni,
perubahan ditolak dengan kode 409 `BED_HOLD_CONFLICT` "Bed masih ditempati atau dipesan."

## 4. Capability evidence map

Format bukti: `repo@sha path:baris #simbol`. `BE@ca09cae` adalah backend commit `ca09cae9…`, dan
`FE@4d9bf60` adalah frontend commit `4d9bf608…`. Path backend relatif terhadap
`NewQuilvianSystemBackend`, path frontend relatif terhadap `QuilvianSystemFrontendDev`. Setiap baris
memakai tepat satu status dari kontrak bukti kemampuan.

**Aturan status yang dipakai.** `Ready to reuse` hanya untuk kemampuan yang wiring, hak akses,
konsumen, dan datanya ada, **dan** tidak disentuh keputusan baru. Kemampuan Bed Management yang lahir
dari `BE-RWI-208`–`221` diberi `Reuse with adapter` karena bukti runtime dan acceptance-nya
(`BM-G04`) belum ada, walaupun source-nya lengkap.

### A. Master lokasi: kelas, kamar, bed, unit

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-01` | Lantai terstruktur dan kunci denah pada kamar (`RWI-DEC-306`, rencana 3.2) | MasterData | `BE@ca09cae Areas/HealthServices/MasterData/Models/MstRoom.cs:34-35 #FloorName` (teks bebas, maks 50); tidak ada kolom kode lantai maupun kunci denah. `MstServiceUnit.cs:30-31 #FloorName` juga teks bebas | **Missing** | Kolom baru pada `MstRoom`, kunci denah unik | `MstRoom` juga dirujuk jadwal kamar operasi, kunjungan registrasi, stasiun hemodialisis, jadwal dokter, dan gudang obat (`DNH-CAP-28`), jadi kolom baru harus *nullable* |
| `DNH-CAP-02` | Wing delapan arah mata angin (`RWI-DEC-313`, `314`) | MasterData | `MstRoom.cs:31-32 #LocationName` teks bebas maks 100; tidak ada kolom wing | **Missing** | Kolom wing bernilai salah satu dari delapan arah; `LocationName` boleh memuat label "Lantai 3 · Barat Laut" | — |
| `DNH-CAP-03` | Kode dan nama kamar "Kelas 2 329" | MasterData | `MstRoom.cs:18-29` (`RoomCode` wajib, `RoomName`, `RoomNumber`); indeks unik `RoomCode` di `Migrations/ApplicationDbContextModelSnapshot.cs:91655-91656` | **Reuse with adapter** | Importer membentuk kode kamar unik. Nomor ganda "K. 321" butuh dua kode berbeda | `RoomCode` unik di seluruh database, termasuk kamar non-rawat-inap |
| `DNH-CAP-04` | Kode bed `329-A` (rencana 3.2) | MasterData | `BE@ca09cae Areas/HealthServices/MasterData/Services/BedMutationService.cs:123-132 #AllocateBedCodeAsync` selalu memberi kode `BD-RSMMC-#####` lewat seri nomor `MST_BED`; indeks unik `BedCode` di snapshot `:86580-86581`; `BedName` (`MstBed.cs:22`) dan `BedNumber` (`:25`) bebas, dengan indeks unik (kamar, nomor bed) `:86585-86586` | **Conflict** | Rencana menulis `329-A` di `BedCode`; source mengalokasikan kodenya sendiri. Jalan tengah: `BedCode` tetap otomatis, `329-A` tampil dari `BedName`/`BedNumber` | Mengubah alokasi kode menyentuh semua pembuat bed dan seri nomor |
| `DNH-CAP-05` | 12 kelas resmi + Perinatologi beserta `ClassLevel` (`RWI-DEC-307`, `310`, `311`) | MasterData | `MstPatientClass.cs:15-43` (`PatientClassCode`, `PatientClassName`, `PatientClassType`, `ClassLevel`, penanda rawat inap/ICU/bayi/tarif); enum `Enums/PatientClassType.cs:5-23` sudah punya `Class3`, `Class2`, `Class1`, `VIP`, `ICU`, `Isolation`, `Odc`, `Perinatology`, `Suite`, `Luxury`; `PatientClassMasterWriteService.cs:235` (`ClassLevel` ≥ 0); `FE@4d9bf60 src/utils/health-services/master-data/patient-class-utils.jsx:491-526` (tipe dan level di form) | **Reuse with adapter** | Data diisi lewat layar master yang sudah ada. `PatientClassType` dapat menandai kelas khusus, sehingga tidak perlu mencocokkan nama | Lihat `DNH-CAP-17`, `29`, `30` |
| `DNH-CAP-06` | Penanda ICU, isolasi, boks bayi, ODC tetap dipakai | MasterData | `MstRoom.cs:43-49`; `MstBed.cs:33-39` | **Ready to reuse** | Importer mengisinya dari kelas | Penanda salah membuat kategori General Consent salah (`DNH-CAP-32`) |
| `DNH-CAP-07` | Hapus penanda laki-laki/perempuan (`RWI-DEC-315`) | MasterData | Kolom `MstRoom.cs:39-41`, `MstBed.cs:29-31`; DTO `DTOs/RoomDtos.cs:42-43, 79-80, 132-133, 212-213` dan `DTOs/BedDtos.cs:45-46, 84-85, 138-139, 208-209`; filter `Controllers/BedController.cs:75-76`; ringkasan `MaleBed`/`FemaleBed` `:127-128`; validasi "minimal laki-laki, perempuan, atau bayi" `Services/RoomMasterWriteService.cs:337` dan `Services/BedMasterWriteService.cs:370`; daftar field berbahaya `BedMutationService.cs:171-172`; respons `AvailableBedResponse` `InpBedOccupancyService.cs:272-273` | **Repair** | Perubahan kontrak yang memutus (*breaking*): kolom, DTO, filter, ringkasan, validasi, dan daftar field berbahaya. Validasi "minimal satu" diganti "boks bayi atau umum" | Konsumen frontend dan test mengunci field ini (`DNH-CAP-39`, `40`) |
| `DNH-CAP-08` | Unit layanan satu per lantai (`RWI-DEC-309`, asumsi) | MasterData | `MstServiceUnit.cs:10-72`; 46 tabel bergantung langsung pada `MstServiceUnit` (snapshot). `Areas/Administrator/MasterData/Models/MstNurseStationCluster.cs:9-56` diperiksa, tetapi itu klaster antrean rawat jalan, bukan bangsal | **Reuse with adapter** | Unit baru dibuat lewat master yang sudah ada | Tarif kamar (`DNH-CAP-29`) dan penugasan akses (`DNH-CAP-20`) terikat unit. **Pengamatan working tree (belum di-commit):** penyaring baru `ServiceUnitRegistrationRules.InpatientRegistrationEligible` hanya meloloskan unit aktif, `IsAvailableForRegistration`, dan bertipe `ServiceUnitType.Inpatient`. Enum itu tidak punya tipe ICU atau Perinatologi, sehingga unit ICU dan Perinatologi Lantai 2 harus bertipe `Inpatient` agar bednya muncul |

### B. Status bed, status kamar, dan data peta

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-09` | Enam status bed (`RWI-DEC-282`) | InPatientManagement | `BE@ca09cae Areas/HealthServices/InPatientManagement/Services/InpBedManagementQueryService.cs:15-19 #StatusLabels`, aturan `:117-131`, alasan blokir `:141-143` (`MASTER_INVALID`, `BED_UNAVAILABLE`, `BED_NOT_READY`); DI `Program.cs:714` | **Reuse with adapter** | Denah memakai proyeksi yang sama; bukti runtime `BM-G04` belum ada | Kamar tanpa kelas aktif membuat semua bednya `MASTER_INVALID` |
| `DNH-CAP-10` | Status kamar turunan dan badge "kapasitas/terisi" (`RWI-DEC-303`) | InPatientManagement | Monitoring hanya mengelompokkan bed per unit lalu per kamar (`InpBedManagementQueryService.cs:213-217`); papan bed mengirim `Capacity` dari `MstRoom.Capacity` yang diisi manual (`InpBedOccupancyService.BedManagement.cs:103-148`) | **Missing** | Hitung status kamar dan badge dari baris bed: kapasitas = bed yang dapat dipakai, terisi = Terisi + Dipesan | Kapasitas manual bisa berbeda dari jumlah bed sebenarnya |
| `DNH-CAP-11` | Data satu lantai dalam satu panggilan | InPatientManagement | Penyaring monitoring: bayi, unit, kamar, kelas, kata kunci, status (`InpBedManagementQueryService.cs:187-200`); tidak ada penyaring lantai | **Reuse with adapter** | Dengan satu unit per lantai, penyaring `serviceUnitId` setara lantai (Lantai 2 punya dua unit). Tanpa asumsi itu perlu penyaring lantai baru | Bergantung pada `RWI-DEC-309` |
| `DNH-CAP-12` | Penghuni (nama, No. RM) dan HD/LD di pop up kamar | InPatientManagement | `HoldingContext` hanya berisi nomor episode dan nama pasien (`InpBedManagementQueryService.cs:145-151`); penyamaran `InpBedAccessService.cs:130-136` | **Extend** | Tambah No. RM dan HD/LD pada proyeksi; penyamaran tetap berlaku | Identitas bocor bila penyamaran dilewati |

### C. Pemilihan bed, kelayakan, dan transfer

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-13` | Kelayakan per pasien untuk outline hijau (`RWI-DEC-316`) | InPatientManagement | `InpBedOccupancyService.cs:96 #SearchAvailableBedsAsync` (menerima `episodeId` atau data pasien sebelum episode; `includeIneligible` `:194-195`; daftar ditolak `:237-245`; penyaring kamar dan unit) | **Reuse with adapter** | Panggil per unit/lantai dengan `includeIneligible=true`, lalu ringkas per kamar | Hasil berhalaman; ukuran halaman harus memuat satu lantai |
| `DNH-CAP-14` | Aturan isolasi dan boks bayi tetap | InPatientManagement | Isolasi `InpBedOccupancyService.cs:1739-1758`; boks bayi `InpNewbornAdmissionRules.cs:57-74`, dipanggil `InpBedOccupancyService.cs:1693-1694` | **Ready to reuse** | — | — |
| `DNH-CAP-15` | Hapus aturan jenis kelamin (`RWI-DEC-315`) | InPatientManagement | `InpBedOccupancyService.cs:1703-1738` (`BED_GENDER_MISMATCH`, `PATIENT_GENDER_UNKNOWN`), komentar riwayat `:1577`; `FE@4d9bf60 src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx:15-16` | **Repair** | Hapus dua aturan beserta pengecualian boks bayinya; nomor aturan lain dibiarkan tetap | Frontend yang mencocokkan kode harus ikut berubah pada gelombang yang sama |
| `DNH-CAP-16` | Kelas khusus bebas memilih kategori transfer (`RWI-DEC-312`) | InPatientManagement | `InpBedClassComparisonService.cs:17-47` mengembalikan `Unknown` bila level ≤ 0 atau *digest* tidak cocok; transfer menolak `Unknown` dengan 422 `CLASS_ORDER_UNVERIFIED` (`InpBedOccupancyService.BedManagement.cs:54-56`); papan memberi kategori kosong (`InpBedOccupancyService.cs:315-316`) | **Extend** | Jalur "kelas khusus = kategori dipilih petugas" belum ada. `PatientClassType` dapat menjadi penanda kelas khusus | Hari ini transfer ke/dari ICU ditolak bila perbandingan tidak terverifikasi |
| `DNH-CAP-17` | Urutan kelas tetap terverifikasi setelah daftar kelas berubah | InPatientManagement + konfigurasi | `Options/BedManagementOptions.cs:10-17`; *digest* SHA-256 dari Id dan `ClassLevel` **semua** kelas aktif beserta pasangan setara (`InpBedClassComparisonService.cs:32-35`); `appsettings*.json` bagian `BedManagement.ClassOrder` berisi arah, rujukan bukti, dan *digest* (terisi), pasangan setara 0 | **Reuse with adapter** | Setiap perubahan Id atau `ClassLevel` kelas aktif, termasuk kelas rawat jalan, wajib diikuti pembaruan *digest* di konfigurasi | Transfer berhenti total (422) sampai *digest* baru dipasang; gate `BM-G01` |
| `DNH-CAP-18` | Pesan, batal, tempatkan, dan transfer dari pop up | InPatientManagement | Controller Bed Occupancy (bagian 5); alasan batal tersimpan `InpBedReservation.cs:14` | **Reuse with adapter** | Denah memanggil endpoint yang sama; bukti runtime `BM-G04` belum ada | — |

### D. Tingkat ketergantungan dan isolasi

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-19` | High/Low Dependency per episode beserta riwayat (`RWI-DEC-302`) | InPatientManagement | Pencarian `Dependency`, `Acuity`, `Ketergantungan` di model rawat inap: nol. Pola isolasi: kolom `InpEpisode.cs:77-88`, endpoint `PATCH episodes/{id}/isolation-requirement` dengan `InpatientEpisode : SetIsolation` (`InpatientEpisodeController.cs:379-386`). Pembaruan isolasi **menimpa** kolom tanpa riwayat (`InpEpisodeService.Assignments.cs:217-238`). Data pendukung: `TrxPatientAssessment.cs:454 #FunctionalStatus` dari indeks Barthel (`ClinicalInstrumentDraftSeeder.cs:484-519`) | **Missing** | Kolom, tabel riwayat, endpoint, dan izin baru. Pola isolasi dipakai untuk kolom, sumber, dan izin; riwayatnya harus baru | Barthel mengukur kemandirian, bukan HD/LD. Menurunkannya otomatis adalah keputusan klinis (`RWI-OQ-141`) |

### E. Hak akses dan privasi

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-20` | Cakupan unit untuk baca, pesan, transfer, dan ubah master | InPatientManagement | `InpBedAccessService.cs:58-97 #UnitsAsync`: Bed Management "Read" melihat semua unit; operasi lain memakai `UnitAssignments` di konfigurasi per pengguna; SuperAdmin semua unit aktif. `appsettings*.json` berisi 4 penugasan | **Reuse with adapter** | Dengan unit per lantai, setiap petugas admisi dan admin master perlu penugasan untuk setiap unit lantai | Penugasan berada di konfigurasi per akun, bukan di database (`BM-G03`) |
| `DNH-CAP-21` | Penyamaran identitas penghuni (`RWI-DEC-287`) | InPatientManagement | `InpBedAccessService.cs:130-136 #CanReadLoadedIdentityAsync`; dipakai `InpBedManagementQueryService.cs:145-151` | **Reuse with adapter** | Pop up memakai aturan yang sama; bukti runtime belum ada | — |
| `DNH-CAP-22` | Aksi pembersihan per bed di pop up | InPatientManagement | `AvailableActions` `InpBedManagementQueryService.cs:153-170`; `InpBedAccessService.cs:152-158 #WorkflowAssignment` (wajib `EnableWorkflow` dan rujukan SOP) | **Reuse with adapter** | Pop up menampilkan aksi dari server | Penugasan SOP nyata belum terbukti (`BM-G02`) |

### F. Master lewat denah dan impor

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-23` | Ubah kelas hanya bila kamar kosong (`RWI-DEC-304`) | MasterData | `PatientClassId` termasuk field berbahaya (`BedMutationService.cs:172`), sehingga alasan wajib dan `RequireEmptyAsync` dijalankan (`:63-69`; `InpBedMutationCoordinator.cs:173-178`, 409 `BED_HOLD_CONFLICT`). Sesudah berubah, bed di-*invalidate* (`BedMutationService.cs:92-99`) | **Reuse with adapter** | Pesan penolakan berbeda dari contoh rencana ("Kelas 2 329 masih dihuni 2 pasien") | Sesudah ubah kelas, semua bed kamar itu Tidak Tersedia sampai disahkan ulang. Konsekuensi ini belum tertulis di rencana |
| `DNH-CAP-24` | Tambah bed dan boks bayi dari denah | MasterData | `BedController.cs:326-330` memanggil `BedMutationService.CreateBedAsync`; data kesiapan baru `Unverified` (`InpBedMutationCoordinator.cs:149-156`; `InpBedReadiness.cs:11`) | **Reuse with adapter** | Boks bayi lewat `IsForNewborn`; bed baru Tidak Tersedia sampai diverifikasi | — |
| `DNH-CAP-25` | Kapasitas kamar = jumlah bed aktif, diselaraskan otomatis | MasterData | `MstRoom.cs:37 #Capacity` diisi manual; `Capacity` termasuk field berbahaya (`BedMutationService.cs:172`), sehingga perubahan menuntut semua bed kosong | **Conflict** | Menyelaraskan kapasitas pada kamar berpenghuni akan ditolak 409. Pilihan desain: hitung kapasitas dari bed tanpa disimpan, atau keluarkan `Capacity` dari field berbahaya | Menambah bed di kamar yang sedang terisi gagal bila kapasitas ikut disimpan |
| `DNH-CAP-26` | Kurangi bed: hapus bila belum pernah dipakai, nonaktifkan bila punya riwayat | MasterData | Hapus bed = *soft delete* dengan syarat status master Available (`BedMasterWriteService.cs:309`); riwayat hunian tidak diperiksa | **Extend** | Tambah pemeriksaan riwayat, atau cukup memakai nonaktif | *Soft delete* tetap menjaga riwayat, jadi risikonya rendah |
| `DNH-CAP-27` | Importer susunan kamar dan bed dari denah (`RWI-DEC-306`) | MasterData | Seeder sengaja tidak membuat kamar/bed (`Seeders/InpatientMasterDataSeeder.cs:16-23`); importer tidak ada | **Missing** | Importer baru melewati jalur mutasi master (cakupan unit, versi, *receipt*) dan membuat data kesiapan | 168 bed lahir `Unverified` (Tidak Tersedia). Cara bed impor menjadi Tersedia perlu keputusan (`DNH-UNK-05`) |
| `DNH-CAP-28` | Hapus master kamar uji lama (`RWI-DEC-305`) | MasterData | `RoomMasterWriteService.cs:206-268 #DeleteRoomAsync` menolak bila dipakai bed, jadwal dokter, atau kunjungan, lalu *soft delete*. `MstRoom` dirujuk langsung 8 tabel: `HmdStation`, `InpBedPlacement`, `MstBed`, `MstDoctorSchedule`, `MstDrugStorageLocation`, `OprSchedule`, `OprStockSource`, `RegPatientEncounter` | **Reuse with adapter** | Lewat API hasilnya *soft delete*. Penghapusan fisik lewat SQL wajib memeriksa kedelapan rujukan | Kamar operasi, hemodialisis, dan gudang obat ikut terhapus bila saringannya salah |

### G. Lintas modul

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-29` | Tarif kamar untuk kelas baru dan unit per lantai | BillingManagement, MasterData | `BE@ca09cae Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs:806-815 #ResolveRoomTariff`: unit layanan **wajib sama**, kelas sama atau kosong; `MstTariff.cs:22-24` | **Conflict** | Unit per lantai dan kelas baru belum punya tarif, sehingga biaya kamar keluar tanpa kebijakan (*missing policy*). `RWI-OQ-143` baru menyebut kelas, belum unit | Pemilik Billing (Yasmina) dan MasterData |
| `DNH-CAP-30` | Kebijakan deposit dan cakupan penjamin per kelas | BillingManagement, MasterData | `BillingDepositService.cs:72-80` (kebijakan per penjamin dan kelas); kolom `PatientClassId` pada `MstInsuranceTariff`, `MstInsuranceCoverageRule`, `MstCompanyGuarantorCoverageRule`, `MstDoctorServiceRule`. Kelas pasien dipakai 10 modul (MasterData 26 berkas, InPatientManagement 24, Billing 13, Clinical 7, Registration 4, IGD 3, Bank Darah 3, Radiologi 2, Laboratorium 2, Finance 2) | **Unknown** | Bergantung pada apakah kelas lama cukup diganti nama (Id tetap) atau diganti baris baru. Isi database belum dibaca | Mengganti Id kelas memutus aturan dan tarif yang sudah ada |
| `DNH-CAP-31` | Kelas layanan mengikuti kelas kamar yang ditempati | InPatientManagement | `InpServiceClassResolver.cs:10-31` (`BE-RWI-224` ✅); DI `Program.cs:526` | **Ready to reuse** | — | — |
| `DNH-CAP-32` | Kategori General Consent "Khusus (ICU/Isolasi)" | InPatientManagement | `InpAdmissionWorkspaceQueryService.cs:720-732` membaca kebutuhan isolasi episode, `IsIntensiveCareBed`, dan `IsIsolationBed` | **Ready to reuse** | Importer wajib mengisi penanda ICU dan isolasi dari kelas | Penanda salah membuat kategori consent salah |

### H. Pembersihan data rawat inap

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-33` | Peta relasi untuk pembersihan DB Hamzah (`RWI-DEC-305`) | Lintas modul | Model snapshot EF: 41 tabel lain bergantung langsung pada `InpEpisode` (ditambah relasi ke dirinya sendiri lewat episode ibu) dan 82 tabel tidak langsung, di 10 modul: Klinis 37, Rawat Inap 24, Farmasi 23, Laboratorium 11, Hemodialisis 8, Radiologi 7, Gizi 7, IGD 4, Billing 1, Kamar Operasi 1. `MstBed` dirujuk `InpBedPlacement`, `InpBedReadiness`, `InpBedReservation` | **Unknown** | Data yang terikat lewat kunjungan (`RegPatientEncounter`) dan Billing (folio, invoice) tidak lewat `InpEpisode`. Perlu katalog database (`information_schema`) dan hitungan baris, sesuai langkah rencana bagian 9 | Tanpa katalog, penghapusan meninggalkan data yatim atau gagal karena rujukan |

### I. Frontend

| ID | Kebutuhan dan keputusan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `DNH-CAP-34` | Komponen denah sebagai tampilan bersama | `episode-rawat-inap` (FE) | `FE@4d9bf60 src/app/health-services/inpatient-management/floor-plan/page.jsx` (data dummy). Tidak ada tautan menu ke halaman ini; menu hanya "Manajemen Tempat Tidur" → `bed-board` (`src/utils/menu-sidebar/menu-items.jsx:1797-1801`). 106 kamar; **76 id kamar memuat nama kelas**, mis. `svip-318`, `vip-isolasi-k-319`, `premium-801` | **Reuse with adapter** | Jadikan komponen bersama; kunci denah dibuat netral kelas, mis. `lantai-3/318` | Kunci yang memuat kelas menyesatkan setelah reklasifikasi, misalnya `svip-318` yang kini VIP Superior |
| `DNH-CAP-35` | Label kelas resmi di denah | FE | `floor-plan/_data/room-classes.js:14` `#ROOM_CLASSES` masih memakai Kelas III, SVIP, VIP Isolasi, dan Ruang Bayi | **Repair** | Label diambil dari master lewat API (`RWI-DEC-308`, `310`, `311`) | — |
| `DNH-CAP-36` | Tab Pemantauan menampilkan denah | FE | `src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx:24-25, 44, 254-255` (pilihan tampilan kartu atau tabel, bawaan kartu); `bed-monitoring-cards.jsx` 415 baris; data dari `use-bed-management.js` (`monitoring`, `metadata`) | **Missing** | Ganti kartu dengan denah; hook data dapat dipakai | Tampilan **tabel** bukan kartu, tetapi `RWI-DEC-299` menetapkan denah sebagai satu-satunya tampilan bed, sehingga tabel ikut diganti (`DNH-UNK-06`) |
| `DNH-CAP-37` | Pilih bed di admisi, admisi bayi, transfer, dan Workspace Keperawatan memakai denah | FE | `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` (1.261 baris) dipakai `inpatient-bed-transfer.jsx`, `inpatient-admission-bed-step.jsx`, `nursing-transfer-form-panel.jsx`, `use-inpatient-bed-board.jsx`, `use-inpatient-admission-bed.jsx`. Transfer dipakai tab Transfer, `nursing-transfer-section.jsx`, dan `inpatient-episode-detail-view.jsx`; langkah Pilih Bed dipakai `inpatient-admission-view.jsx` dan `inpatient-newborn-admission-steps.jsx` | **Missing** | Ganti papan kartu pada kelima konsumen; hook reservasi dan transfer dapat dipakai | Admisi bayi dan rawat gabung harus tetap jalan |
| `DNH-CAP-38` | Konsumen `bed-board` di luar layar bed | FE | `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx` memanggil `bed-board` | **Reuse with adapter** | Endpoint `bed-board` tidak boleh dihapus bersama kartu | Daftar pasien rawat inap ikut rusak |
| `DNH-CAP-39` | Isian jenis kelamin pada master kamar dan bed | FE | `src/utils/health-services/master-data/room-utils.jsx:447-448, 489-490, 516-517`; `bed-utils.jsx:814-821, 890-891, 952-960`; `src/lib/constants/health-services/master-data/room-constants.jsx:51-52, 115-121`; `bed-constants.jsx:88-89, 134-140` | **Repair** | Hapus isian, nilai bawaan, dan label detail bersama backend | — |
| `DNH-CAP-40` | Test yang mengunci perilaku lama | FE | `tests/unit/inpatient-bed-board.test.mjs`, `inpatient-placement.test.mjs`, `inpatient-foundation.test.mjs`, `inpatient-discharge.test.mjs`, `inpatient-episode-detail.test.mjs`; `tests/e2e/bed-status-toggle.spec.mjs`, `inpatient-episode-detail.spec.mjs` | **Repair** | Sesuaikan atau hapus saat kartu dan jenis kelamin dihapus | — |
| `DNH-CAP-41` | Editor master lewat denah (rencana P-FE-07) | FE | Rute master `src/app/health-services/master-data/bed`, `room`, `patient-class`, `service-unit`; alur konfirmasi mutasi `use-bed-master-confirmation.js` dan `bed-master-mutation.service.js` (`FE-RWI-234` ✅: alasan, versi bed terdampak, hasil operasi) | **Reuse with adapter** | Editor denah memakai alur konfirmasi mutasi yang sudah ada | — |

## 5. Kontrak as-is yang ditelusuri

Semua endpoint berikut **tersedia di source** dan terdaftar di `Program.cs:710-717`, tetapi belum
dibuktikan saat aplikasi berjalan. Envelope memakai `ApiResponse<T>`; daftar memakai `PagedResult<T>`.
Pengguna diambil dari akun yang masuk, bukan dari isian request.

### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/metadata` | Daftar status, unit, dan aksi yang boleh | `InpatientBedManagement : Read` | Query `serviceUnitId` | Objek metadata |
| `GET` | `/monitoring` | Hitungan status dan bed per unit lalu per kamar | `InpatientBedManagement : Read` | `BedMonitoringQuery` | `BedMonitoringResponse` |
| `GET` | `/usage-history` | Riwayat pemakaian bed | `InpatientBedManagement : ReadUsageHistory` | `BedUsageHistoryQuery` | `PagedResult<BedUsageHistoryResponse>` |
| `GET` | `/operations/{key}` | Hasil operasi yang pernah dikirim | `InpatientBedManagement : Read` | `key` | `OperationOutcomeResponse` |
| `GET` | `/beds/{bedId}/cleaning-attempts` | Riwayat upaya pembersihan | `InpatientBedManagement : Read` | `pageNumber`, `pageSize` | `PagedResult<BedCleaningAttemptResponse>` |
| `POST` | `/beds/{bedId}/cleaning-attempts` | Mulai pembersihan | `InpatientBedManagement : StartCleaning` | `StartBedCleaningRequest` | `BedOperationResponse` |
| `PATCH` | `/cleaning-attempts/{attemptId}/complete` | Selesai pembersihan | `InpatientBedManagement : CompleteCleaning` | `CompleteBedCleaningRequest` | `BedOperationResponse` |
| `POST` | `/beds/{bedId}/readiness-verifications` | Sahkan siap atau belum siap | `InpatientBedManagement : VerifyReadiness` | `VerifyBedReadinessRequest` | `BedOperationResponse` |

### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/available-beds` | Cari bed beserta alasan bed yang ditolak | `InpatientBedOccupancy : Read` | `AvailableBedQuery` | `AvailableBedPagedResult` |
| `GET` | `/bed-board` | Papan bed per unit dan kamar | `InpatientBedOccupancy : Read` | `serviceUnitId`, `roomId`, `patientClassId`, `search`, `isForNewborn` | `BedBoardResponse` |
| `POST` | `/reservations` | Pesan bed | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `BedReservationResponse` |
| `PATCH` | `/reservations/{id}/cancel` | Batalkan pesanan | `InpatientBedOccupancy : Update` | `CancelReservationRequest` | `BedReservationResponse` |
| `POST` | `/placements` | Tempatkan pasien | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `BedPlacementResponse` |
| `POST` | `/placements/transfer` | Pindahkan pasien | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` | `BedPlacementResponse` |
| `POST` | `/placements/{placementId}/corrections` | Koreksi hunian | `InpatientBedOccupancy : Correct` | `CorrectPlacementRequest` | `BedPlacementResponse` |
| `GET` | `/placements/by-episode/{episodeId}` | Riwayat hunian satu episode | `InpatientBedOccupancy : Read` | `episodeId` | `List<BedPlacementResponse>` |
| `GET` | `/transfer-context` | Asal transfer dan perbandingan kelas | `InpatientBedOccupancy : Read` | `episodeId` | Objek konteks transfer |

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes` (hanya endpoint pola isolasi)

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/isolation-requirement` | Menetapkan kebutuhan isolasi episode | `InpatientEpisode : SetIsolation` | `SetIsolationRequirementRequest` | `InpatientEpisodeDetailResponse` |

### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/`, `/options`, `/summary`, `/filters/metadata`, `/{id}` | Daftar, pilihan, ringkasan, penyaring, detail | `Room : Read` | Query penyaring | `ResponseRoomPagedResult`, `RoomOptionPagedResponse`, `RoomSummaryResponse`, `RoomFilterMetadataResponse`, `RoomDetailResponse` |
| `POST` | `/` | Tambah kamar | `Room : Create` | `CreateRoomRequest` | `RoomCreateResponse` |
| `PUT` | `/{id}` | Ubah kamar (lewat jalur mutasi bed) | `Room : Update` | `UpdateRoomRequest` | `RoomUpdateResponse` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `Room : Update` | `UpdateRoomStatusRequest` | `RoomUpdateResponse` |
| `DELETE` | `/{id}` | Hapus (*soft delete*) | `Room : Delete` | `DeleteRoomRequest` | `RoomDeleteResponse` |

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/`, `/options`, `/summary`, `/filters/metadata`, `/{id}` | Daftar, pilihan, ringkasan, penyaring, detail | `Bed : Read` | Query penyaring | `ResponseBedPagedResult`, `BedOptionPagedResponse`, `BedSummaryResponse`, `BedFilterMetadataResponse`, `BedDetailResponse` |
| `POST` | `/` | Tambah bed; kode `BD-RSMMC-#####` dibuat otomatis | `Bed : Create` | `CreateBedRequest` | `BedCreateResponse` |
| `PUT` | `/{id}` | Ubah bed | `Bed : Update` | `UpdateBedRequest` | `BedUpdateResponse` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `Bed : Update` | `UpdateBedStatusRequest` | `BedUpdateResponse` |
| `PATCH` | `/{id}/availability` | Tutup atau buka bed | `Bed : Update` | `UpdateBedAvailabilityRequest` | `BedUpdateResponse` |
| `DELETE` | `/{id}` | Hapus (*soft delete*) bila status Available | `Bed : Delete` | `DeleteBedRequest` | `BedDeleteResponse` |

### Health Services / Master Data / Patient Class

Base URL: `api/v1/health-services/master-data/patient-classes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/`, `/options`, `/summary`, `/filters/metadata`, `/{id}` | Daftar, pilihan, ringkasan, penyaring, detail | `PatientClass : Read` | Query penyaring | `ResponsePatientClassPagedResult`, `PatientClassOptionPagedResponse`, `PatientClassSummaryResponse`, `PatientClassFilterMetadataResponse`, `PatientClassDetailResponse` |
| `POST` | `/` | Tambah kelas | `PatientClass : Create` | `CreatePatientClassRequest` | `PatientClassCreateResponse` |
| `PUT` | `/{id}` | Ubah kelas (lewat jalur mutasi bed) | `PatientClass : Update` | `UpdatePatientClassRequest` | `PatientClassUpdateResponse` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `PatientClass : Update` | `UpdatePatientClassStatusRequest` | `PatientClassUpdateResponse` |
| `DELETE` | `/{id}` | Hapus | `PatientClass : Delete` | `DeletePatientClassRequest` | `PatientClassDeleteResponse` |

**Arti kode status bagi pengguna:**

- **403** — pengguna tidak punya hak, atau unitnya di luar penugasan (`ACCESS_DENIED`).
- **409** — bed baru saja diambil atau diubah petugas lain (`STALE_BED_VERSION`), atau masih ditempati
  atau dipesan (`BED_HOLD_CONFLICT`). Layar memuat ulang data.
- **422** — aturan dilanggar: kelayakan (bagian 3.1), alasan wajib (`REASON_REQUIRED`), urutan kelas
  belum terverifikasi (`CLASS_ORDER_UNVERIFIED`), atau alur kerja belum diaktifkan
  (`WORKFLOW_NOT_ACTIVATED`).
- **503** — akun sistem untuk kedaluwarsa pesanan belum terverifikasi (`DEPENDENCY_UNAVAILABLE`).

## 6. Ketidakcocokan rencana ↔ source dan frontend ↔ backend

| ID | Temuan | Bukti | Dampak | Arah penyelesaian (rekomendasi) |
| --- | --- | --- | --- | --- |
| `DNH-MIS-01` | Rencana memakai `BedCode` `329-A`; source mengalokasikan `BD-RSMMC-#####` | `DNH-CAP-04` | Importer dan editor tidak dapat menulis `329-A` ke `BedCode` lewat API | `BedCode` tetap otomatis; `329-A` dari `BedName` dan `BedNumber` (unik per kamar) |
| `DNH-MIS-02` | Rencana menyelaraskan `Capacity` otomatis; source menjadikannya field berbahaya | `DNH-CAP-25` | Tambah bed di kamar terisi ditolak 409 | Kapasitas dihitung dari bed aktif saat dibaca, atau dikeluarkan dari field berbahaya |
| `DNH-MIS-03` | Rencana belum menyebut bahwa bed baru dan bed sesudah ubah kelas berstatus Tidak Tersedia sampai disahkan | `DNH-CAP-23`, `24`, `27` | 168 bed impor dan kamar yang diubah kelasnya tidak bisa dipesan | Putuskan jalur kesiapan awal impor (`DNH-UNK-05`) dan tulis konsekuensinya di desain |
| `DNH-MIS-04` | Kelas khusus bebas memilih kategori (`RWI-DEC-312`), tetapi source menolak semua perbandingan `Unknown` | `DNH-CAP-16`, `17` | Transfer ke ICU/Isolasi/Perinatologi ditolak 422 | Jalur kelas khusus berdasarkan `PatientClassType`; *digest* diperbarui bersamaan dengan data kelas |
| `DNH-MIS-05` | Tarif kamar dicocokkan per unit layanan; asumsi unit per lantai belum menyentuh Billing | `DNH-CAP-29` | Biaya kamar unit baru keluar tanpa kebijakan | Perluas `RWI-OQ-143` ke tarif per unit; koordinasi Billing sebelum unit dibuat |
| `DNH-MIS-06` | Contoh kunci denah `lantai-3/k-329` netral kelas, tetapi 76 id gambar memuat nama kelas | `DNH-CAP-34` | Kunci menyesatkan setelah reklasifikasi | Kunci dibentuk dari lantai dan nomor, bukan dari id gambar |
| `DNH-MIS-07` | Rencana menghapus bed yang belum pernah dipakai; source memeriksa status, bukan riwayat | `DNH-CAP-26` | Bed beriwayat dapat di-*soft delete* | Pakai nonaktif untuk bed beriwayat, atau tambah pemeriksaan riwayat |
| `DNH-MIS-08` | Jenis kelamin dipakai bersama oleh backend dan frontend (field, kode, test) | `DNH-CAP-07`, `15`, `39`, `40` | Bila hanya satu sisi berubah, form master dan pesan kelayakan rusak | Satu gelombang backend–frontend |
| `DNH-MIS-09` | `bed-board` juga dipakai daftar pasien rawat inap | `DNH-CAP-38` | Menghapus endpoint bersama kartu merusak daftar pasien | Pertahankan endpoint; hapus hanya komponen kartu |

## 7. Fakta, inferensi, dan rekomendasi

**Fakta** (dari source, tanpa runtime):

- `DNH-FACT-01` — Enam status bed, kesiapan, pembersihan, dan pengesahan sudah ada di source dan
  terdaftar di DI (`Program.cs:710-717`); konfigurasi `BedManagement.EnableWorkflow` bernilai `true`.
- `DNH-FACT-02` — Bed baru dan bed yang terkena perubahan master berbahaya berstatus kesiapan
  `Unverified` atau `WaitingCleaning`, sehingga tampil Tidak Tersedia.
- `DNH-FACT-03` — `BedCode` dialokasikan otomatis `BD-RSMMC-#####`; `RoomCode` dan `BedCode` unik di
  seluruh database.
- `DNH-FACT-04` — Perbandingan kelas memakai *digest* dari semua kelas aktif dan menolak transfer bila
  tidak cocok.
- `DNH-FACT-05` — Tarif kamar dicocokkan per unit layanan dan kelas.
- `DNH-FACT-06` — Tidak ada kolom lantai, wing, kunci denah, maupun HD/LD di source.
- `DNH-FACT-07` — Pembaruan isolasi menimpa kolom episode tanpa riwayat.
- `DNH-FACT-08` — 123 tabel bergantung pada episode rawat inap lewat relasi EF.
- `DNH-FACT-09` — Halaman denah tidak terhubung ke menu; 76 dari 106 id kamar memuat nama kelas.

**Inferensi:**

- Dengan unit per lantai, penyaring `serviceUnitId` pada monitoring dan pencarian bed sudah cukup
  untuk "data satu lantai", kecuali Lantai 2 yang punya dua unit.
- Perubahan daftar kelas dan unit sebaiknya satu paket dengan pembaruan tarif dan *digest*. Bila
  tidak, transfer berhenti dan biaya kamar kosong pada hari yang sama.
- Data Barthel dapat membantu perawat menilai HD/LD, tetapi tidak setara dengan HD/LD.

**Rekomendasi** (bukan keputusan; untuk `design-business-module`):

1. Ganti nama kelas lama yang setara (mis. Kelas I → Kelas 1) **tanpa** mengganti Id, supaya tarif,
   deposit, dan aturan cakupan tetap tersambung. Buat baris baru hanya untuk kelas yang memang baru.
2. Tandai kelas khusus dengan `PatientClassType` yang sudah ada (`ICU`, `Isolation`, `Odc`,
   `Perinatology`), bukan dengan nama.
3. Hitung kapasitas dan status kamar dari bed saat dibaca, tidak disimpan.
4. Simpan riwayat HD/LD di tabel tersendiri; jangan meniru penimpaan kolom pada isolasi.
5. Jalankan importer lewat jalur mutasi master yang sudah ada, supaya cakupan unit, versi, dan
   *receipt* tetap berlaku.

## 8. Unknown dan pertanyaan penutup

| ID | Pertanyaan | Mengapa penting | Pemilik | Memblokir |
| --- | --- | --- | --- | --- |
| `DNH-UNK-01` | Kelas, unit, kamar, bed, dan tarif apa yang sekarang tersimpan di DB Hamzah? | Menentukan strategi ganti nama atau baris baru (`DNH-CAP-30`) | Pemilik aplikasi; butuh izin baca database | Desain data migrasi kelas |
| `DNH-UNK-02` | Tabel mana yang merujuk episode lewat kunjungan dan Billing, dan berapa barisnya? | Langkah 1–3 pembersihan (`RWI-DEC-305`) | Pemilik aplikasi; butuh izin baca database | Eksekusi pembersihan |
| `DNH-UNK-03` | Kelas lama diganti nama (Id tetap) atau diganti baris baru? | Tarif, deposit, cakupan, dan *digest* | MasterData dan Billing | Desain |
| `DNH-UNK-04` | Siapa menyiapkan tarif kamar untuk unit per lantai dan kelas baru, dan kapan? | Biaya kamar (`DNH-CAP-29`) | Billing (Yasmina) | Uji biaya, bukan denah |
| `DNH-UNK-05` | Bagaimana 168 bed hasil impor menjadi Tersedia: disahkan perawat satu per satu, atau importer boleh menetapkan Ready dengan rujukan pengesahan RS? | Tanpa jalur ini semua bed impor Tidak Tersedia | Pemilik produk dan keperawatan | Desain importer |
| `DNH-UNK-06` | Tampilan **tabel** di Pemantauan ikut dihapus bersama kartu, atau tetap sebagai tampilan kedua? | Pemilik meminta kartu dihapus; tabel bukan kartu | Pemilik produk | **Ditutup dari decision log:** `RWI-DEC-299` menetapkan denah sebagai satu-satunya tampilan bed, jadi tabel ikut diganti kecuali pemilik menyatakan lain |

`DNH-UNK-05` adalah keputusan manusia dan dibawa ke `grill-me` lanjutan. Pada closure pass revision 49,
`DNH-UNK-05` menjadi RWI-OQ-145, `DNH-UNK-03` menjadi RWI-OQ-146, dan `DNH-UNK-04` menjadi RWI-OQ-147
beserta perluasan RWI-OQ-143. Ketiganya dijawab pemilik pada revision 50 sebagai RWI-DEC-321, 322, dan
323. Konflik `DNH-MIS-01` dan `02` ditutup asumsi draft RWI-DEC-317 dan 318. `DNH-UNK-06` sudah dijawab
`RWI-DEC-299`. `DNH-UNK-01` dan `02` butuh izin membaca database pribadi `QuilvianNewDevHamzah`, hanya
baca. `DNH-UNK-03` dan `04` adalah titik sentuh modul lain.

## 9. Keterbatasan audit

- Tidak ada build, test, aplikasi berjalan, maupun pembacaan database. Semua status runtime tetap
  `NOT_RUN`.
- Relasi antar-tabel dihitung dari model snapshot EF. Kolom rujukan tanpa relasi EF tidak terlihat.
- Isi konfigurasi tidak dicetak; hanya nama key dan jenis nilai.
- Hak akses aktual per role di database tidak diperiksa.
- Kedua repository berubah di working tree selama audit karena pekerjaan paralel: delapan berkas
  frontend alur admisi dan tiga berkas source backend (issue 022). Bukti dibaca dari `HEAD`. Baris
  frontend yang dikutip dari tiga berkas yang ikut berubah sudah diperiksa tetap sama. Perubahan
  backend pada pencarian bed dicatat sebagai pengamatan, bukan fakta `HEAD`.

## 10. Handoff dan pemicu impact scan

**Langkah berikutnya:**

1. `grill-me` lanjutan untuk `DNH-UNK-05`, ditambah titik sentuh Billing (`DNH-UNK-03`, `04`).
2. Bila diizinkan, pembacaan katalog DB Hamzah (hanya baca) untuk `DNH-UNK-01` dan `02`.
3. `design-business-module` untuk amandemen blueprint dan kontrak Bed Management `0.13.0` (usulan),
   memakai bukti dan rekomendasi di atas.

**Pemicu impact scan ulang:**

- perubahan `HEAD` yang menyentuh `MstRoom`, `MstBed`, `MstPatientClass`, `BedMutationService`,
  `InpBedManagementQueryService`, `InpBedOccupancyService`, `InpBedClassComparisonService`,
  `InpBedAccessService`, atau penghitungan tarif kamar;
- commit pekerjaan issue 022 yang menambah `ServiceUnitRegistrationRules` pada pencarian bed;
- perubahan pada konsumen frontend di bagian 4 I;
- keputusan baru pada decision log;
- bukti database atau runtime yang bertentangan dengan dokumen ini.

Hash setiap berkas yang dikutip dan dokumen masukan ada di
[snapshot JSON](./denah-bed-source-snapshot-20261015.json).
