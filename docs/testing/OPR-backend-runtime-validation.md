# Validasi Backend Operasi (`BE-OPR-001`..`BE-OPR-011`)

Tanggal: 28 September 2026.

Dua lapis bukti dipakai dan keduanya dirawat berdampingan. Runtime/API membuktikan modul bekerja
di atas PostgreSQL sungguhan beserta seluruh middleware-nya. Proyek uji membuktikan aturan yang
sama tetap berlaku setiap kali kode berubah, tanpa menunggu seseorang menjalankan aplikasi.

## Lingkungan

| Hal | Nilai |
|---|---|
| Basis data runtime | `localhost` / `QuilvianNewDevIkbalFr` (development) |
| Aplikasi | dibangun dari repositori, dijalankan pada `:5000` |
| Aturan klinis | `OperatingRoom:RelaxClinicalRules=false` (aturan penuh) |
| Akun | `opr.bedah`, `opr.anestesi`, `opr.perawat` — dibuat `OperatingRoomDemoSeeder` |
| Proyek uji | `Tests/QuilvianSystemBackend.OperatingRoomTests`, SQLite dalam memori |
| Migration | tidak ada migration baru; tidak ada `database update` |

Perintah uji:

```
dotnet test Tests\QuilvianSystemBackend.OperatingRoomTests\QuilvianSystemBackend.OperatingRoomTests.csproj -p:SkipMigrationMetadata=true
```

`-p:SkipMigrationMetadata=true` wajib disertakan. Tanpa itu satu build ikut mengompilasi seluruh
berkas Designer dan snapshot migration, dan waktunya berubah dari sekitar 40 detik menjadi
sekitar satu jam.

**Hasil:** 70 uji, seluruhnya lulus, sekitar 37 detik.

## Perubahan kode pada tahap ini

Tiga perubahan, seluruhnya di dalam modul Operasi, dan tidak satu pun berupa migration baru.

1. **`OperatingRoomMaterialService`** menolak nomor serial implant yang sudah tercatat `Used`
   pada kasus yang sama, dengan kode baru `OPR014`.
2. **`OperatingRoomDemoSeeder`** membuat dokter bedah demo beserta akun `opr.bedah`. Sebelumnya
   seeder menyatakan tujuannya "supaya sign-off tiga peran dapat benar-benar diuji" tetapi hanya
   membuat dua akun, sehingga peran dokter bedah tidak pernah terisi. Akun sasaran seeder — di
   pemakaian biasa `superadmin` — tidak dapat dipinjam karena `AspNetUsers.DoctorId` dan
   `AspNetUsers.WorkforceProfileId` keduanya berindeks unik.
3. **`OperatingRoomDemoSeeder`** memberi satuan stok pada item demo. Tanpa
   `MstDrug.StockUnitMeasurementId`, setiap pencatatan material ditolak `PHM091` dan seluruh
   alur material tidak dapat dicoba sama sekali.

## Aturan keunikan serial implant (`OPR014`)

Aturan V1: satu nomor serial implant hanya boleh tercatat sebagai `Used` satu kali dalam satu
kasus operasi.

Yang tidak termasuk, beserta alasannya:

- **Kasus operasi lain** tidak dibatasi. Implant yang gagal dan diganti pada operasi revisi harus
  tetap dapat dicatat.
- **`Returned` dan `Wasted`** tidak menutup nomor serinya, karena keduanya justru menyatakan
  implant tidak jadi terpasang.
- **Baris yang sudah digantikan koreksi** dilepas, supaya salah catat tidak mengunci sebuah nomor
  seri selamanya.

Penjagaannya dipasang di service, bukan sebagai unique index basis data. Alasannya bukan
kemudahan: aturan ini memuat syarat "belum digantikan koreksi", yang tidak dapat dinyatakan
sebagai indeks parsial atas satu tabel. Indeks yang menyederhanakannya akan ikut menolak
pencatatan ulang yang sah sesudah koreksi. Karena itu penjagaan tingkat basis data ditahan
sampai pemilik proses memutuskan bagaimana koreksi berinteraksi dengan keunikan, dan bagaimana
cakupannya di luar satu kasus. Akibat yang harus diketahui: dua permintaan yang benar-benar
bersamaan masih dapat lolos keduanya, karena tidak ada penjagaan di tingkat basis data.

## Hasil runtime di bawah aturan klinis penuh

| Task | Uji | Diharapkan | Aktual | Status |
|---|---|---|---|---|
| `BE-OPR-001` | Struktur 14 tabel `Opr*` | FK Restrict, unique index, kolom concurrency | 23 FK seluruhnya `NO ACTION`/`RESTRICT`, 0 CASCADE; 29 unique index; 4 kolom `Version` | LULUS |
| `BE-OPR-003` | Buat kasus, akun tertaut dokter | 201 | 201 | LULUS |
| `BE-OPR-003` | Indikasi kosong / tanpa tindakan / pasien tak dikenal | 400 | 400 | LULUS |
| `BE-OPR-003` | Kunci sama + isi sama | kasus yang sama | `caseNumber` sama, tanpa kasus baru | LULUS |
| `BE-OPR-003` | Kunci sama + isi beda | 409 `OPR013` | 409 `OPR013` | LULUS |
| `BE-OPR-003` | 5 permintaan paralel, satu kunci | tepat 1 kasus | 5×201, tepat 1 kasus | LULUS |
| `BE-OPR-003` | Tindakan sudah dipakai kasus lain | ditolak | 409 `OPR002` | LULUS |
| `BE-OPR-004` | Buat kasus di aturan penuh (`opr.bedah`) | 201 | 201 | LULUS |
| `BE-OPR-004` | Penjadwalan tim empat peran | 200, `Scheduled` | 200, status 2 | LULUS |
| `BE-OPR-004` | Jadwal bertabrakan | 409 `OPR003` | 409 `OPR003` | LULUS |
| `BE-OPR-004` | 5 penjadwalan paralel | tepat 1 berhasil | 1×200, 1×409 `OPR003`, 3×409 konkurensi | LULUS |
| `BE-OPR-004` | Penjadwalan ulang | jadwal lama tersimpan | jadwal lama `isCurrent:false` | LULUS |
| `BE-OPR-005` | Checklist SignIn diselesaikan | 200 | 200 | LULUS |
| `BE-OPR-005` | Sign-off dokter bedah (`opr.bedah`) | 200 | 200, kasus tetap `Scheduled` | LULUS |
| `BE-OPR-005` | Sign-off dokter anestesi (`opr.anestesi`) | 200 | 200, kasus tetap `Scheduled` | LULUS |
| `BE-OPR-005` | Sign-off perawat (`opr.perawat`) | 200, kasus naik `Ready` | 200, status 3, kekurangan kosong | LULUS |
| `BE-OPR-005` | Sign-off oleh peran yang salah | 403 | 403 dengan peran yang berwenang disebut | LULUS |
| `BE-OPR-005` | Peran yang sama dua kali | 409 `OPR006` | 409 `OPR006` | LULUS |
| `BE-OPR-005` | `ExpectedVersion` basi | 409 | 409 `OPR012` | LULUS |
| `BE-OPR-005` | Consent dibuat tidak sah | muncul sebagai kekurangan | muncul, hilang setelah dipulihkan | LULUS |
| `BE-OPR-005` | Finalisasi checklist butir wajib kosong | ditolak | 422 `OPR006` | LULUS |
| `BE-OPR-006` | Mulai tanpa konfirmasi | ditolak | 422 `StartNotConfirmed` | LULUS |
| `BE-OPR-006` | Mulai oleh bukan dokter bedah tim | 403 | 403 | LULUS |
| `BE-OPR-006` | Mulai oleh dokter bedah tim | 200, `InProgress` | 200, status 4 | LULUS |
| `BE-OPR-006` | Mulai ulang kunci sama | idempotent | tetap 1 `OprExecutionRecord` | LULUS |
| `BE-OPR-006` | Batalkan sesudah mulai | ditolak | 409 `InvalidStateTransition` | LULUS |
| `BE-OPR-006` | Finalisasi tanpa outcome | ditolak | 422 `OutcomeRequired` | LULUS |
| `BE-OPR-006` | Ubah catatan final | ditolak | 422 `OPR010` | LULUS |
| `BE-OPR-006` | Addendum append-only + retry | tidak menggandakan | 2 addendum, retry tidak menambah | LULUS |
| `BE-OPR-007` | Catatan anestesi oleh bukan anestesi tim | 403 | 403 | LULUS |
| `BE-OPR-007` | Recovery `Released` tanpa tujuan | ditolak | 422 `RecoveryDecisionRequired` | LULUS |
| `BE-OPR-007` | Serah terima sebelum keluar recovery | ditolak | 422 `RecoveryNotReleased` | LULUS |
| `BE-OPR-007` | Tolak serah terima tanpa alasan | ditolak | 422 `RejectionReasonRequired` | LULUS |
| `BE-OPR-007` | Serah terima diterima | kasus `Completed` | status 5 | LULUS |
| `BE-OPR-008` | Kuantitas 0 | ditolak | 422 `OPR008` | LULUS |
| `BE-OPR-008` | Implant tanpa batch/serial | ditolak | 422 `OPR009` | LULUS |
| `BE-OPR-008` | Pemakaian implant serial baru | 200 | 200 | LULUS |
| `BE-OPR-008` | Serial implant yang sama pada kasus yang sama | ditolak | 409 `OPR014` | LULUS |
| `BE-OPR-008` | Ulang kunci+isi sama sesudah aturan serial | 200 idempotent | 200, bukan `OPR014` | LULUS |
| `BE-OPR-008` | Serial berbeda | 200 | 200 | LULUS |
| `BE-OPR-008` | Retur dengan serial yang sama | 200 | 200 | LULUS |
| `BE-OPR-008` | Koreksi tanpa alasan | ditolak | 422 `CorrectionReasonRequired` | LULUS |
| `BE-OPR-008` | Pemakaian pada kasus `Completed` | hanya koreksi | 409 | LULUS |
| `BE-OPR-010` | Filter status | menyempit sesuai status | 3 → 1 → 0 sesuai filter | LULUS |
| `BE-OPR-010` | Rentang tanggal di luar data | 0 baris | 0 | LULUS |
| `BE-OPR-010` | Paging dan batas `pageSize` | metadata benar, 400 di luar batas | benar, 400 | LULUS |
| `BE-OPR-010` | Utilization tanpa rentang / dengan rentang | 400 / angka terisi | 400; `scheduledCases` 3 | LULUS |
| `BE-OPR-010` | Laporan material | serial tampil | tampil | LULUS |
| `BE-OPR-009` | Pemakaian material menghasilkan pesan outbox | 1 pesan, `eventVersion` 1.0 | 1 pesan, `operating-room.material-usage.recorded` v1.0 | LULUS |
| `BE-OPR-009` | Tiga permintaan, dua kejadian berbeda | 2 pesan, 2 `eventId` | 2 pesan, 2 `eventId` unik | LULUS |
| `BE-OPR-009` | Amplop memuat bidang wajib | lengkap | `eventId`/`eventType`/`eventVersion`/`occurredAt`/`caseId`/`caseNumber`/`patientId`/`encounterId`/`serviceRequestId` terisi; 1 procedure; 4 performer; blok `material` lengkap | LULUS |
| `BE-OPR-009` | Baca amplop lewat endpoint | 200 | 200, isi sesuai yang tersimpan | LULUS |
| `BE-OPR-009` | Consumer menolak | pengiriman `Failed`, data klinis utuh | status pengiriman 4, `CONSUMER_TIMEOUT`; status kasus tetap 4 dan jumlah pemakaian tetap 5 | LULUS |
| `BE-OPR-009` | Antrekan ulang | `Pending`, tanpa pesan baru | status 1, `retryCount` 1, jumlah pesan tetap 2, `eventId` tidak berubah | LULUS |

## Hasil proyek uji

| Berkas | Yang dibuktikan | Jumlah |
|---|---|---|
| `PermissionMatrixTests` | Setiap controller Operasi menuntut login dan terdaftar pada modul izin Operasi; setiap endpoint menuntut izin atas sumber daya Operasi, bukan modul lain; endpoint yang mengubah data tidak pernah cukup dengan izin baca | 25 |
| `StateTransitionTests` | Setiap status dicapai lewat service sungguhan; transisi terlarang ditolak; `Completed` menunggu ketiga gerbangnya | 9 |
| `ConcurrencyAndIdempotencyTests` | Versi basi ditolak; pengulangan tidak menggandakan; kunci sama isi beda ditolak; tabrakan jadwal; riwayat jadwal | 8 |
| `MaterialSerialTests` | Aturan `OPR014` beserta batasnya: kasus lain boleh, retur boleh, bahan habis pakai tidak terkena, idempotensi tetap utuh | 8 |
| `AuditPrivacyTests` | Setiap perubahan status meninggalkan riwayat beserta pelakunya; jejak audit tidak memuat nama pasien, nomor rekam medis, maupun indikasi | 3 |
| `IntegrationOutboxTests` | Satu kejadian satu pesan; amplop memuat seluruh bidang wajib; pengulangan tidak menggandakan; `EventId` tetap sama walau permintaannya diulang; kejadian berbeda menghasilkan pesan berbeda; kegagalan pengiriman tidak merusak data operasi; pesan gagal dapat diantrekan ulang tanpa pesan baru; pesan yang sudah diterima tidak dapat dikirim ulang; antrean menunggu dapat dibaca | 10 |
| `SeedSmokeTests` | Basis data uji dan seeder demo benar-benar terbentuk, tiga peran dipegang tiga tenaga berbeda | 4 |

Uji izin sengaja memeriksa kontrak yang terpasang pada endpoint, bukan jawaban `403` runtime.
Jawaban `403` bergantung pada peran dan pemetaan izin milik lingkungan; menuliskannya di dalam
uji berarti mengarang keadaan lalu menyatakannya terbukti.

`IntegrationOutboxTests` berhenti tepat di batas modul. Bentuk pesan yang diterima Billing dan
Inventory tidak diuji, karena kontrak consumer keduanya belum disahkan pemiliknya.

## Yang masih terbuka

**1. Kolom pengguna pada baris audit terisi id kasus, bukan id pelaku.**
`LoggerService.WriteAsync` mengambil nilainya dari `UserId` lalu `Id` pada data yang dikirim,
sedangkan data audit Operasi memuat `Id` kasus. Nomor kasus dan `ActorUserId` yang dikirim
service tidak ikut tercetak pada baris lognya. Pertanggungjawaban pelaku karena itu hanya
terbaca dari `OprStatusHistory.CreateBy`, dan itulah yang diuji. `LoggerService` dipakai seluruh
modul sehingga perbaikannya bukan keputusan modul Operasi sendiri.

**2. Penolakan izin per peran belum pernah terlihat berjalan.**
Ketiga akun demo dibuat berperan `SuperAdmin` oleh seeder — disengaja, supaya alur klinis dapat
dicoba tanpa menyiapkan pemetaan izin lebih dulu — sehingga seluruhnya selalu lolos. Satu-satunya
akun tanpa peran di basis data pengembangan kata sandinya tidak diketahui. Membuktikan `403`
menuntut peran dan pemetaan izin yang disiapkan pemilik lingkungan.

**3. Keunikan serial implant belum dijaga basis data.**
Lihat alasannya di bagian `OPR014`. Perlu keputusan pemilik proses sebelum indeks dibuat.

**4. Consumer Billing belum ada yang mengambil pesannya.**
Kontrak kejadian outbox sudah lengkap dan pesannya terbentuk, tetapi tujuan Billing belum punya
penerima, sehingga pesannya menunggu di antrean dan rekonsiliasinya masih dilakukan orang. Itu
kesiapan penerima, bukan pekerjaan modul Operasi yang tertinggal.

## Data uji yang perlu dibersihkan

Semua berada di `localhost` / `QuilvianNewDevIkbalFr`.

| Objek | Penanda |
|---|---|
| `TrxPatientProcedure` | `dddd0008-…-0001` (`UJI-BE-OPR-008`) dan `dddd0011-…-0001` (`UJI-BE-OPR-011`) |
| `OprCase` | indikasi berawalan "Uji " |
| `OprMaterialUsage` | batch `UJI-BATCH-01`, serial `UJI-SN-0001`, `UJI-SN-9001`, `UJI-SN-9002` |
| Riwayat idempotensi | `CorrelationId` berawalan `UJI-` |
| `MstDrug` | `18793e83…` dan `7f9b6709…`: `StockUnitMeasurementId` pernah diisi manual `bbbb0000-…-0001` sebelum seeder menyediakan satuannya sendiri |

Baris buatan `OperatingRoomDemoSeeder` berawalan `DEMO-OPR` dan bukan data uji sekali pakai;
ia dipakai ulang setiap kali seeder dijalankan.

Consent pasien uji sempat diubah ke `Draft` untuk membuktikan gerbang consent, lalu dikembalikan
ke `Signed` pada langkah yang sama.
