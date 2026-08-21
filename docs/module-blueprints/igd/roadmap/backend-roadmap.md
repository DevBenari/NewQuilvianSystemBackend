# Roadmap Delivery Backend — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 1
status: DRAFT
owners:
  - "Product/Domain Owner sementara (IGD-DEC-046) — nama belum diisi"
approved_by: []
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 4
  01-existing-capability-map.md: 2
artifact_hashes:
  contracts/api-contract.md: "f64dea9e9c98a269091b18a5b72d817dc1bf263cdc7692e8a957055dfdb77719"
  contracts/state-transition-matrix.md: "208ddc38ff2367210d8783c29b8d9b2e0b09fa7691a51317006fa848145add5f"
  contracts/validation-matrix.md: "b4bc0a86b8122e9ff20749f9c25497fabea78e49bb0ecf1fbd6a83eac26169ee"
  contracts/permission-audit-matrix.md: "18c36104ca7917136f5cb7d6672ec60d20cd6493579c63b9fc25f014453db83f"
  contracts/integration-contract.md: "79e9d928a2a810d2b8c4fe4987cacd17468bc32c8f4b37fc0b7cbf92f72150ca"
contract_versions:
  - "API 0.2.0"
  - "State 0.2.0"
  - "Validation 0.2.0"
  - "Integration 0.2.0"
  - "Permission/Audit 0.2.0"
source_commits:
  backend: "e5331a015fa416a89454b435de0014455f0326d8"
  frontend: "08c84d371ed90640189ce1758019184b0a955e13"
```

Roadmap ini berstatus `DRAFT` karena satu alasan saja: urutan prioritas adalah wewenang
Product/Domain Owner, dan dokumen ini baru dibuat sehingga belum pernah dibaca pemiliknya.
Isinya sendiri sudah bertumpu pada blueprint revision 4 yang **sudah** disetujui pada
14 Agustus 2026.

---

## 1. Cara membaca roadmap ini

Pekerjaan dipecah menjadi **slice**, bukan menjadi lapisan teknis. Satu slice adalah satu hasil
yang bisa dirasakan petugas IGD dan bisa diperiksa benar atau salahnya. Contohnya "perawat
dapat menilai ulang pasien", bukan "buat semua model" lalu "buat semua endpoint".

Setiap task memakai ID tetap `BE-IGD-nnn`. ID tidak pernah dipakai ulang walaupun task
dibatalkan, supaya rujukan pada laporan lama tidak berubah arti.

Tiga istilah yang dipakai berulang:

| Istilah | Arti |
| --- | --- |
| *Closure gate* | Syarat yang harus terpenuhi sebelum kunjungan boleh dinyatakan selesai |
| *Breach* | Keadaan ketika batas waktu respons terlampaui dan pasien belum ditangani |
| *Hosted service* | Proses latar belakang yang berjalan sendiri secara berkala di dalam aplikasi |

---

## 2. Keadaan awal yang menentukan urutan

Impact scan 14 Agustus 2026 membuktikan tidak ada satu pun berkas `.cs` yang berubah sejak
blueprint diaudit. Tiga fakta berikut karena itu masih berlaku dan langsung membentuk urutan
pengerjaan.

| Fakta | Bukti | Akibat pada urutan |
| --- | --- | --- |
| Delapan service IGD tidak pernah didaftarkan ke dependency injection | `Program.cs` baris 259–281 tidak memuat satu pun `Emergency*Service`; pencarian `EmergencyVisitService` hanya menemukan berkas service dan controller-nya | Seluruh 52 endpoint IGD gagal dipanggil sekarang juga. Ini menjadi task nomor satu |
| Enam tabel master IGD kosong | `IGD-FACT-005`; tidak ditemukan seeder level triase | Penilaian triage tidak dapat dibuat sama sekali sebelum master terisi |
| Target waktu tunggu bertipe angka biasa, bukan angka yang boleh kosong | `MstEmergencyTriageLevel.MaxWaitingMinutes` bertipe `int` baris 35; `EmergencyTriageController` baris 220 menghitung `ResponseDueAt = StartedAt.AddMinutes(MaxWaitingMinutes)` | "Belum dikonfigurasi" tidak dapat dibedakan dari "0 menit". Harus diperbaiki sebelum pemantau breach dinyalakan |

Fakta ketiga adalah yang paling mudah terlewat, jadi contohnya ditulis di sini:

> **Contoh:** SOP MMC belum menetapkan target untuk level 3. Petugas mengisi master dengan
> mengosongkan kolom target, tetapi karena kolomnya angka biasa, yang tersimpan adalah 0.
> Pasien level 3 yang dinilai pukul 08.00 langsung memperoleh `ResponseDueAt` pukul 08.00
> juga. Satu menit kemudian pemantau menandainya melampaui batas. Layar perawat penuh
> peringatan palsu, dan peringatan yang benar untuk pasien Merah ikut tenggelam.

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task |
| --- | --- | --- |
| **S0 — Modul benar-benar hidup** | Endpoint IGD dapat dipanggil, master terisi, dan "target belum diatur" dapat dibedakan dari nol menit | `BE-IGD-001`, `BE-IGD-002`, `BE-IGD-003` |
| **S1 — Perawat dapat menilai ulang pasien** | Retriage membuat penilaian baru dan penilaian lama menjadi historis | `BE-IGD-004` |
| **S2 — Pasien menunggu terlalu lama tertandai** | Sistem menandai sendiri pasien yang melewati batas dan menyediakan daftarnya | `BE-IGD-005`, `BE-IGD-006`, `BE-IGD-007` |
| **S3 — Kunjungan dapat diselesaikan secara klinis** | Status `Completed` terpisah dari `Disposed`, dengan gate yang benar | `BE-IGD-008`, `BE-IGD-009` |
| **S4 — Hak akses sesuai kewenangan** | Pemeriksaan akses mengenal unit dan sumber daya, akses darurat tercatat | `BE-IGD-010`, `BE-IGD-011`, `BE-IGD-012` |
| **S5 — Utang teknis dan kesiapan** | Penyimpangan struktur folder dirapikan, bukti penerimaan lengkap | `BE-IGD-013`, `BE-IGD-014` |

### Urutan dependency

```text
BE-IGD-001 (DI)
   ├── BE-IGD-002 (target boleh kosong) ── BE-IGD-003 (data master) ──┐
   │                                                                   │
   ├── BE-IGD-004 (retriage) ─────────────────────────────────────────┤
   │                                                                   │
   ├── BE-IGD-005 (kolom breach) ── BE-IGD-006 (pemantau) ── BE-IGD-007 (daftar breach)
   │                                                                   │
   └── BE-IGD-008 (enum Completed) ── BE-IGD-009 (endpoint complete)   │
                                                                       │
BE-IGD-010 (scope unit)  ── BE-IGD-011 (break-glass) ── BE-IGD-012 (SuperAdmin, TERTAHAN)
BE-IGD-013 (rapikan folder, kapan saja setelah S3)                     │
                                                                  BE-IGD-014 (bukti penerimaan)
```

`BE-IGD-002`, `BE-IGD-004`, `BE-IGD-005`, dan `BE-IGD-010` tidak saling bergantung, sehingga
boleh dikerjakan paralel oleh orang berbeda setelah `BE-IGD-001` selesai.

---

## 4. Task

### `BE-IGD-001` — Seluruh service IGD didaftarkan sehingga endpoint benar-benar dapat dipanggil

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas IGD yang membuka layar mana pun tidak lagi menerima kegagalan sistem. Saat ini setiap permintaan ke endpoint IGD berhenti sebelum kode modul dijalankan, karena aplikasi tidak tahu cara membuat objek service yang diminta controller |
| **Trace** | `CAP-16` (status `Repair`) pada capability map; `IGD-DEC-046`; kontrak `0.2.0` tidak berubah |
| **Reuse** | Pola pendaftaran yang sudah dipakai 30-an service lain pada `Program.cs` baris 259–281, yaitu `builder.Services.AddScoped<TService>()`. Tidak ada interface dan tidak ada mekanisme baru |
| **Scope** | `Program.cs` saja. Delapan kelas: `EmergencyVisitService`, `EmergencyTriageService`, `EmergencyResuscitationService`, `EmergencyObservationService`, `EmergencyDispositionService`, `EmergencyTransferService`, `EmergencyDocumentNumberService`, dan `EmergencySettingService` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Aplikasi menyala tanpa galat. 2. Memanggil `GET /emergency-visits` tanpa token mengembalikan 401, bukan 500. 3. Memanggil dengan token berhak mengembalikan 200. 4. Kesembilan controller IGD dan `EmergencySettingController` berhasil dibuat saat diminta |
| **Verification** | Test aktivasi controller yang meminta setiap controller IGD dari container; satu integration test per resource yang membuktikan balasan bukan 500 |
| **Risk/blocker** | Bila ternyata ada mekanisme pendaftaran lain yang belum terlihat, pendaftaran ganda dapat terjadi. Periksa dulu, jangan menambah tanpa memeriksa. Owner: Backend/API |
| **DoD** | Delapan service terdaftar; test aktivasi lulus; build lulus; laporan perubahan mencatat bahwa ini memperbaiki blocker runtime, bukan menambah fitur |

> **Mengapa ini nomor satu:** tanpa task ini, seluruh task lain tidak dapat dibuktikan
> berjalan. Menulis endpoint retriage di atas modul yang tidak bisa dipanggil sama saja
> dengan menulis kode yang tidak pernah dieksekusi.

---

### `BE-IGD-002` — "Target waktu belum diatur" dapat dibedakan dari "0 menit"

| Field | Isi |
| --- | --- |
| **Outcome** | Level triase yang target waktunya belum ditetapkan SOP tidak lagi diperlakukan seolah-olah harus dilayani seketika. Perawat tidak dibanjiri peringatan palsu, dan peringatan pasien Merah tetap menonjol |
| **Trace** | `IGD-DEC-027`, `IGD-DEC-035` (Kuning/Hijau berstatus `TargetUnconfigured`); validation matrix bagian 2 "Contoh target yang belum dikonfigurasi"; `AT-IGD-012` |
| **Reuse** | Kolom, index, dan alur perhitungan yang sudah ada. Tidak ada tabel atau service baru |
| **Scope** | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` (`MaxWaitingMinutes` menjadi `int?`); `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` (`MaxWaitingMinutesSnapshot` menjadi `int?`); `EmergencyTriageController` baris 218–220 (hanya menghitung `ResponseDueAt` bila target terisi); DTO master dan triage; migration baru `MakeTriageMaxWaitingMinutesNullable` |
| **Dependency** | `BE-IGD-001` |
| **Acceptance criteria** | 1. Level dengan target kosong menghasilkan `ResponseDueAt` kosong dan `MaxWaitingMinutesSnapshot` kosong. 2. Level 1 dengan target 0 menit tetap menghasilkan `ResponseDueAt` sama dengan `StartedAt`. 3. Penilaian lama yang sudah punya snapshot angka tidak berubah nilainya. 4. Baris master lama terisi apa adanya oleh migration, tanpa menebak |
| **Verification** | Unit test tiga kasus: target kosong, target 0, target 30. Integration test `AT-IGD-011` dan `AT-IGD-012`. Uji migration maju dan mundur pada basis data lokal |
| **Risk/blocker** | **Penyimpangan dari blueprint.** Rencana migration pada `02-backend-architecture.md` bagian 6 hanya memuat satu migration, yaitu `AddTriageSlaBreachMarker`. Task ini menambah satu migration lagi yang belum tercatat di sana. Asumsi yang dipakai: aturan `TargetUnconfigured` pada validation matrix mengikat, sehingga tipe datanya harus mampu menyatakan "kosong". Owner: Product/Domain + Backend/API. Bila owner menolak, satu-satunya alternatif adalah menebak angka target, dan itu dilarang keputusan yang sudah ada |
| **DoD** | Model, DTO, perhitungan, dan migration selesai; tiga unit test lulus; arsitektur backend bagian 6 diperbarui agar rencana migration cocok dengan kenyataan; laporan perubahan menyebut penyimpangan ini secara eksplisit |

---

### `BE-IGD-003` — Enam data master IGD terisi sehingga modul dapat dipakai

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat dapat memilih level triase, petugas dapat memilih cara kedatangan dan jenis kasus, dan dokter dapat memilih jenis tindak lanjut. Tanpa ini, layar IGD hanya menampilkan daftar pilihan kosong |
| **Trace** | `02-backend-architecture.md` bagian 7; `IGD-DEC-047`, `IGD-DEC-048`; `AT-IGD-070` |
| **Reuse** | Pola seeder yang sudah ada, contohnya `Areas/HealthServices/PharmacyManagement/Seeders/PrescriptionReviewCriterionSeeder.cs` dan `Seeders/Icd10DiagnosisSeeder.cs`. Keenam controller master IGD juga sudah tersedia di `Areas/HealthServices/MasterData/Controllers/` |
| **Scope** | Seeder baru pada `Areas/HealthServices/MasterData/Seeders/`; pendaftarannya mengikuti cara seeder lain dipanggil |
| **Dependency** | `BE-IGD-002` — target level 2 sampai 5 harus dapat dikosongkan sebelum data diisi |
| **Acceptance criteria** | 1. `MstEmergencyTriageLevel` berisi level 1 sampai 5 beserta kelompok warna Merah, Kuning, dan Hijau, ditambah satu baris Hitam di luar skala antrean. 2. Hanya level 1 yang memiliki target waktu, yaitu 0 menit. Level 2 sampai 5 dikosongkan. 3. Lima master lain terisi sesuai daftar isi minimum pada arsitektur bagian 7.2. 4. Menjalankan seeder dua kali tidak menghasilkan data ganda. 5. `MstEmergencySetting` memiliki tepat satu baris default |
| **Verification** | Integration test: jalankan seeder dua kali lalu hitung barisnya; `AT-IGD-070` membuktikan pesan yang mengarahkan pengisian master saat master kosong; `AT-IGD-073` membuktikan setting default kedua ditolak |
| **Risk/blocker** | Isi `MstEmergencyTriageIndicator` berasal dari SOP triase MMC yang belum tersedia. Isi indikator ABCDE secara umum dan tandai bahwa daftar finalnya menunggu SOP. **Jangan** mengisi target waktu level 2 sampai 5. Owner: Product/Domain, dengan clinical governance owner sebagai pengesah akhir |
| **DoD** | Seeder selesai dan idempotent; enam master terisi; kolom target level 2 sampai 5 tetap kosong; laporan perubahan mencantumkan isi yang di-seed apa adanya |

---

### `BE-IGD-004` — Perawat dapat menilai ulang pasien tanpa merusak riwayat

| Field | Isi |
| --- | --- |
| **Outcome** | Kondisi pasien memburuk atau membaik, perawat menilai ulang, dan penilaian lama tetap tersimpan utuh sebagai riwayat. Auditor kelak dapat melihat urutan penilaian dari awal sampai akhir |
| **Trace** | `IGD-DEC-004`, `IGD-DEC-048`; api contract `POST /emergency-triages/{id}/retriage`; state matrix bagian 2; validation matrix bagian 2; `AT-IGD-013`, `AT-IGD-014`, `AT-IGD-015` |
| **Reuse** | Kolom `Sequence`, `IsRetriage`, dan `PreviousTriageId` sudah ada pada `TrxEmergencyTriage` baris 23, 25, dan 27. Index unik `(EmergencyVisitId, Sequence)` sudah ada dan mencegah nomor urut ganda |
| **Scope** | `EmergencyTriageService` (logika retriage), `EmergencyTriageController` (aksi baru), `EmergencyTriageDtos.cs` (`RetriageEmergencyTriageRequest`) |
| **Dependency** | `BE-IGD-001`; `BE-IGD-003` agar ada level triase yang dapat dipilih |
| **Acceptance criteria** | 1. Retriage atas penilaian `Completed` membuat baris baru dengan `Sequence` berikutnya, `IsRetriage` benar, dan `PreviousTriageId` menunjuk baris lama. 2. Baris lama berubah menjadi `Superseded` dan isinya tidak berubah sedikit pun. 3. Retriage atas penilaian `Cancelled` ditolak 409 dengan pesan "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang." 4. Retriage atas penilaian yang belum `Completed` ditolak 409. 5. Tanpa hak `EmergencyTriage : Update`, ditolak 403. 6. Menekan tombol dua kali hanya menghasilkan satu baris baru |
| **Verification** | Integration test untuk keenam kriteria; satu test membandingkan isi baris lama sebelum dan sesudah retriage kolom demi kolom |
| **Risk/blocker** | Endpoint `PUT /emergency-triages/{id}` yang sudah ada masih dapat menimpa penilaian mana pun, termasuk yang sudah `Superseded`. Sifat append-only karena itu belum benar-benar terlindungi. Ini tercatat sebagai gap `CG-07` di bagian 7 dan **tidak** diselesaikan oleh task ini |
| **DoD** | Endpoint sesuai kontrak `0.2.0`; keenam test lulus; api contract diperbarui dari "Rencana (belum tersedia)" menjadi tersedia; traceability diperbarui |

---

### `BE-IGD-005` — Kunjungan menyimpan penanda pelampauan batas waktu

| Field | Isi |
| --- | --- |
| **Outcome** | Sistem punya tempat untuk mencatat bahwa seorang pasien terlambat ditangani, beserta waktunya. Ini fondasi bagi dua task berikutnya, dan tidak mengubah perilaku apa pun jika berdiri sendiri |
| **Trace** | `IGD-GAP-007`; data dictionary bagian triage kolom `IsSlaBreached` dan `SlaBreachedAt`; `02-backend-architecture.md` bagian 5 dan 6 |
| **Reuse** | Preseden penanda serupa pada `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxHrServiceRequest.cs` kolom `IsSlaBreached` baris 66. Bentuk kolom mengikuti preseden itu |
| **Scope** | `TrxEmergencyTriage.cs` (dua kolom baru); `Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyTriageConfiguration.cs` (index gabungan); migration `AddTriageSlaBreachMarker` |
| **Dependency** | `BE-IGD-001` |
| **Acceptance criteria** | 1. Kolom `IsSlaBreached` bertipe boolean wajib dengan nilai bawaan salah. 2. Kolom `SlaBreachedAt` boleh kosong. 3. Index `(EmergencyVisitId, ResponseDueAt, IsSlaBreached)` terbentuk. 4. Seluruh baris lama terisi salah tanpa perhitungan ulang riwayat. 5. Migration dapat dijalankan dan dimundurkan tanpa mematikan layanan |
| **Verification** | Uji migration maju dan mundur pada basis data lokal; periksa bentuk kolom dan index sesuai DDL pada data dictionary bagian 6.2 |
| **Risk/blocker** | Migration tidak boleh diterapkan ke basis data mana pun selain lokal tanpa izin eksplisit. Owner: Backend/API |
| **DoD** | Dua kolom, satu index, dan satu migration selesai; uji maju-mundur lulus; laporan perubahan menyatakan migration belum diterapkan di luar lokal |

---

### `BE-IGD-006` — Sistem menandai sendiri pasien yang terlambat ditangani

| Field | Isi |
| --- | --- |
| **Outcome** | Tidak ada yang perlu menghitung manual siapa yang sudah menunggu terlalu lama. Proses latar belakang memeriksa berkala dan menandai sendiri, sehingga keterlambatan terlihat walaupun tidak ada yang membuka layar |
| **Trace** | `IGD-DEC-027`, `IGD-GAP-007`; integration contract bagian "Proses terjadwal di dalam aplikasi"; `AT-IGD-020`, `AT-IGD-021`, `AT-IGD-022`, `AT-IGD-023` |
| **Reuse** | Lima hosted service yang sudah matang pada modul Human Resource, yaitu Attendance, Leave Accrual, Leave Carry Forward, Leave Execution, dan Overtime, terdaftar pada `Program.cs` baris 319, 331, 337, 351, dan 384. Ikuti polanya; jangan membuat mekanisme penjadwalan baru |
| **Scope** | `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTriageSlaMonitorHostedService.cs` (baru); satu baris `AddHostedService` pada `Program.cs` |
| **Dependency** | `BE-IGD-002` (target kosong tidak boleh dihitung), `BE-IGD-005` (tempat menyimpan penanda) |
| **Acceptance criteria** | 1. Penilaian yang `ResponseDueAt`-nya sudah lewat dan pasiennya belum ditangani ditandai `IsSlaBreached` benar beserta `SlaBreachedAt`. 2. Penilaian yang `ResponseDueAt`-nya kosong **tidak pernah** ditandai. 3. Penilaian satu menit sebelum batas tidak ditandai. 4. Menjalankan pemindaian dua kali tidak mengubah `SlaBreachedAt` yang sudah terisi. 5. Kegagalan pemindaian tidak menghalangi triage, penanganan, maupun penyelesaian kunjungan. 6. Pemindaian tidak mengubah kolom klinis apa pun |
| **Verification** | Integration test `AT-IGD-020` sampai `AT-IGD-023`; satu test menjalankan pemindaian dua kali lalu membandingkan `SlaBreachedAt`; satu test memaksa galat pada pemindaian lalu membuktikan endpoint triage tetap melayani |
| **Risk/blocker** | Frekuensi pemindaian belum ditetapkan siapa pun. Buat dapat dikonfigurasi dan beri nilai bawaan yang wajar; jangan menanamkan angka di kode. Owner: Product/Domain |
| **DoD** | Hosted service selesai dan terdaftar; enam kriteria terbukti; frekuensi dapat dikonfigurasi; integration contract diperbarui dari "Baru" menjadi tersedia |

---

### `BE-IGD-007` — Perawat dapat mengambil daftar pasien yang melampaui batas

| Field | Isi |
| --- | --- |
| **Outcome** | Kepala jaga dapat melihat dalam satu daftar siapa saja yang terlambat ditangani, tanpa membuka satu per satu kunjungan |
| **Trace** | api contract `GET /emergency-triages/sla-breaches`; permission matrix baris `EmergencyTriage : Read`; `AT-IGD-024` |
| **Reuse** | Index gabungan dari `BE-IGD-005`; pola penyaringan dan halaman yang sudah dipakai `GET /emergency-triages` |
| **Scope** | `EmergencyTriageController` (aksi baru), `EmergencyTriageDtos.cs` (`BreachListResponse`), `EmergencyTriageService` |
| **Dependency** | `BE-IGD-006` |
| **Acceptance criteria** | 1. Daftar hanya memuat pasien yang benar-benar melampaui batas dan belum ditangani. 2. Pasien yang sudah ditangani tidak muncul walaupun penanda breach-nya tetap tersimpan sebagai riwayat. 3. Penyaringan menurut unit dan rentang waktu berfungsi. 4. Tanpa hak `EmergencyTriage : Read`, ditolak 403. 5. Balasan memuat nama pasien, bukan hanya identifier |
| **Verification** | Integration test `AT-IGD-024` beserta kasus tanpa hak akses; satu test menyiapkan tiga pasien dengan keadaan berbeda lalu memeriksa hanya satu yang muncul |
| **Risk/blocker** | Daftar ini menampilkan data pasien. Kolom bertanda sensitif pada data dictionary tidak boleh ikut. Owner: Security/privacy |
| **DoD** | Endpoint sesuai kontrak; lima kriteria terbukti; api contract diperbarui; tidak ada kolom sensitif dalam balasan |

---

### `BE-IGD-008` — Status `Completed` dipisahkan dari `Disposed`

| Field | Isi |
| --- | --- |
| **Outcome** | "Dokter sudah memutuskan tindak lanjut" tidak lagi berarti sama dengan "urusan pasien di IGD sudah tuntas". Dua keadaan yang berbeda itu akhirnya punya status berbeda, sehingga laporan lama tinggal pasien di IGD menjadi benar |
| **Trace** | `IGD-DEC-049`, `IGD-GAP-001` (terkonfirmasi di source); `02-backend-architecture.md` bagian 5; state matrix bagian 1; `AT-IGD-035` |
| **Reuse** | Enum `EmergencyVisitStatus` yang sudah ada. Nilai baru `Completed = 9` dipilih agar delapan nilai lama tidak bergeser dan data lama tetap terbaca benar |
| **Scope** | `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyVisitStatus.cs`; `EmergencyVisitController` bagian `UpdateStatus`, yang saat ini mengisi `VisitCompletedAt` ketika status menjadi `Disposed` atau `Cancelled` |
| **Dependency** | `BE-IGD-001`; **`FE-IGD-005` harus lebih dulu rilis** karena ini perubahan yang berpotensi memutus tampilan |
| **Acceptance criteria** | 1. Nilai `Completed = 9` tersedia. 2. `VisitCompletedAt` tidak lagi terisi saat status menjadi `Disposed`. 3. `VisitCompletedAt` tidak lagi terisi saat status menjadi `Cancelled`. 4. Data lama dengan `VisitCompletedAt` terisi pada status `Disposed` tidak diubah, karena mengubahnya berarti memalsukan riwayat. 5. Transisi dari `Completed` ke status mana pun ditolak 409 |
| **Verification** | Unit test nilai enum; integration test `AT-IGD-035`; satu test membuktikan `VisitCompletedAt` tetap kosong saat pindah ke `Disposed` |
| **Risk/blocker** | Perubahan ini membuat data lama dan data baru punya arti berbeda untuk kolom yang sama. Laporan yang menghitung kunjungan selesai berdasarkan `VisitCompletedAt` akan mencampur keduanya. Catat batas waktunya, jangan diam-diam. Owner: Product/Domain |
| **DoD** | Enum dan perilaku pengisian selesai; lima kriteria terbukti; catatan kompatibilitas ditulis di laporan perubahan; `FE-IGD-005` sudah rilis lebih dulu |

---

### `BE-IGD-009` — Dokter dapat menyelesaikan kunjungan setelah seluruh kewajiban tuntas

| Field | Isi |
| --- | --- |
| **Outcome** | Dokter menutup kunjungan hanya bila memang sudah selesai. Sistem yang memeriksa syaratnya, bukan ingatan petugas. Pasien yang masih diobservasi atau masih dalam proses perpindahan tidak bisa ditutup |
| **Trace** | `IGD-DEC-049`, `IGD-DEC-021`; api contract `PATCH /emergency-visits/{id}/complete`; validation matrix bagian 3; `AT-IGD-030` sampai `AT-IGD-034` |
| **Reuse** | Status transfer dan observasi yang sudah ada; kolom `VisitCompletedAt` yang sudah ada pada `TrxEmergencyVisit` |
| **Scope** | Endpoint pada `EmergencyVisitController` karena route dan hak aksesnya milik resource `EmergencyVisit`; logika gate pada `EmergencyDispositionService` sesuai arsitektur bagian 3.3; DTO `CompleteVisitRequest` |
| **Dependency** | `BE-IGD-008` |
| **Acceptance criteria** | 1. Dari status `Disposed` berhasil, status menjadi `Completed` dan `VisitCompletedAt` terisi waktu server. 2. Dari status selain `Disposed` ditolak 409 dengan pesan "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan." 3. Masih ada observasi `Active` ditolak 409. 4. Masih ada transfer yang belum `Completed` atau `Rejected` ditolak 409. 5. Billing `Pending` atau `Outstanding` **tidak** menghalangi, sesuai `IGD-DEC-021`. 6. Tanpa hak `EmergencyVisit : Update` ditolak 403 |
| **Verification** | Integration test `AT-IGD-030` sampai `AT-IGD-034`; `AT-IGD-031` khusus membuktikan billing tidak menghalangi |
| **Risk/blocker** | **Ketidakcocokan dokumen.** Arsitektur bagian 3.4 menempatkan aksi ini pada `EmergencyDispositionController`, sedangkan api contract dan permission matrix menempatkan route-nya di bawah `emergency-visits` dengan hak akses `EmergencyVisit : Update`. Asumsi yang dipakai: dua dokumen kontrak menang atas satu dokumen arsitektur, jadi endpoint berada di `EmergencyVisitController`. Minta penegasan owner, lalu rapikan dokumen yang kalah. Owner: Backend/API + Product/Domain |
| **DoD** | Endpoint sesuai kontrak; enam kriteria terbukti; api contract diperbarui; ketidakcocokan dokumen di atas sudah ditutup, bukan dibiarkan |

---

### `BE-IGD-010` — Pemeriksaan akses mengenal unit pelayanan dan sumber daya

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas unit tujuan hanya dapat menerima perpindahan ke unitnya sendiri, bukan ke unit mana pun di rumah sakit. Sekarang pemeriksaan akses sama sekali tidak tahu unit apa yang sedang disentuh |
| **Trace** | `IGD-DEC-026`; `IGD-GAP-006` (status `Missing` untuk scope resource/unit); permission matrix bagian 3 kebutuhan nomor 2 |
| **Reuse** | `SysAccessPolicy` beserta join Department dan Position pada `AccessPermissionService` baris 85–112, termasuk masa berlaku penugasan baris 101–104. Yang ditambah hanya parameter konteks, bukan sistem izin kedua |
| **Scope** | `Services/Security/AccessPermissionService.cs` (`HasAccessAsync` menerima konteks sumber daya); `Filters/AccessPermissionFilter.cs`; `Attributes/AccessPermissionAttribute.cs` bila perlu |
| **Dependency** | — (boleh paralel dengan seluruh slice IGD) |
| **Acceptance criteria** | 1. Pemeriksaan akses dapat menerima unit pelayanan sebagai konteks. 2. Endpoint yang belum mengirim konteks berperilaku persis seperti sekarang, sehingga tidak ada modul lain yang rusak. 3. Petugas unit A ditolak 403 saat menerima perpindahan ke unit B. 4. `AT-IGD-041` lulus, yaitu pengaju transfer tidak dapat menerima transfernya sendiri |
| **Verification** | Unit test pada `AccessPermissionService`; integration test `AT-IGD-041`; regression test yang membuktikan endpoint modul lain tidak berubah perilakunya |
| **Risk/blocker** | Perubahan ini menyentuh jalur otorisasi seluruh aplikasi, bukan hanya IGD. Sifatnya harus **menambah**, tidak mencabut akses siapa pun. Owner: Security/privacy owner sebagai pengesah akhir; secara teknis Backend/API |
| **DoD** | Parameter konteks tersedia dan dipakai endpoint transfer IGD; empat kriteria terbukti; regression modul lain lulus; laporan perubahan menegaskan tidak ada akses yang dicabut |

---

### `BE-IGD-011` — Akses darurat klinis punya jalur resmi yang tercatat

| Field | Isi |
| --- | --- |
| **Outcome** | Ketika keadaan darurat menuntut akses di luar kewenangan biasa, ada jalur yang sah, berbatas waktu, dan tercatat, bukan mengandalkan seseorang yang kebetulan punya akses penuh |
| **Trace** | `IGD-DEC-050`; `IGD-FACT-010` (tidak ada mekanisme break-glass di kode mana pun); permission matrix bagian 3 rincian kebutuhan 3 |
| **Reuse** | Pola masa berlaku efektif yang sudah dipakai `TrxApprovalDelegation` (`EffectiveStartAt` dan `EffectiveEndAt` baris 41–43) dan penugasan organisasi. Pola audit memakai `LoggerService` yang sudah ada |
| **Scope** | Kandidat `Services/Security/`; tempat pastinya menjadi bagian desain task ini |
| **Dependency** | `BE-IGD-010` |
| **Acceptance criteria** | 1. Akses darurat wajib mengisi alasan. 2. Akses berakhir sendiri setelah batas waktu, tanpa perlu dicabut manual. 3. Setiap penggunaan tercatat beserta pelaku, waktu, alasan, dan sumber daya yang diakses. 4. Isi rekam medis yang dibaca **tidak** ikut tercatat. 5. Dapat ditinjau setelah kejadian melalui daftar tersendiri |
| **Verification** | Integration test kelima kriteria; satu test membuktikan akses benar-benar berhenti setelah batas waktu lewat |
| **Risk/blocker** | Ini kebutuhan desain baru, bukan penyesuaian kecil. Persetujuan security/privacy owner adalah syarat go-live, bukan syarat mulai membangun. Owner: Security/privacy |
| **DoD** | Mekanisme selesai dan teruji; lima kriteria terbukti; permission matrix diperbarui; status aktivasi produksi ditandai menunggu security/privacy owner |

---

### `BE-IGD-012` — Kewenangan SuperAdmin dipisahkan menurut jenis endpoint

| Field | Isi |
| --- | --- |
| **Outcome** | Administrator teknis tetap dapat mengurus sistem, tetapi tidak lagi otomatis dapat membaca dan mengubah data klinis pasien tanpa kebijakan akses |
| **Trace** | `IGD-DEC-050`, `IGD-CONFLICT-003`; `IGD-FACT-009`; permission matrix bagian 3 kebutuhan 1; `AT-IGD-052`, `AT-IGD-053` |
| **Reuse** | Penanda `IsSystemOnly` yang sudah ada pada `Attributes/AccessControllerAttribute.cs` baris 30 dan sudah dikecualikan dari pencarian policy pada `AccessPermissionService` baris 65 dan 70 |
| **Scope** | `Services/Security/AccessPermissionService.cs`, yaitu `IsSuperAdminUser` baris 54–57 dan 117–151 |
| **Dependency** | `BE-IGD-011` — akses darurat harus **sudah tersedia** sebelum jalur bypass klinis ditutup |
| **Acceptance criteria** | 1. SuperAdmin tetap diterima pada endpoint bertanda `IsSystemOnly`, misalnya `HumanResourceContextController`. 2. SuperAdmin ditolak 403 pada endpoint klinis IGD bila tidak punya kebijakan akses. 3. Pengguna teknis yang saat ini bergantung pada bypass sudah diinventarisasi lebih dulu, sehingga tidak ada pekerjaan sah yang mendadak berhenti. 4. Perilaku baru berada di balik saklar yang dapat dimatikan tanpa deploy ulang |
| **Verification** | Integration test `AT-IGD-052` dan `AT-IGD-053`; inventarisasi pemakaian bypass sebagai bukti tertulis sebelum perubahan dinyalakan |
| **Risk/blocker** | **TERTAHAN untuk aktivasi produksi.** Boleh dibangun, diuji, dan digabung, tetapi tidak boleh dinyalakan sebelum security/privacy owner ditunjuk dan menyetujuinya, dan sebelum `BE-IGD-011` benar-benar berjalan. Menutup bypass tanpa jalur darurat berarti memindahkan risiko dari keamanan ke keselamatan pasien. Owner: Security/privacy |
| **DoD** | Perubahan selesai, teruji, dan **mati secara bawaan**; empat kriteria terbukti; inventarisasi pemakaian bypass terlampir; status aktivasi tercatat sebagai menunggu owner |

---

### `BE-IGD-013` — Penyimpangan struktur folder IGD dirapikan

| Field | Isi |
| --- | --- |
| **Outcome** | Modul IGD mengikuti pola yang sama dengan 25 modul lain, sehingga pengembang berikutnya tidak menebak-nebak dan tidak menyalin penyimpangan ini ke modul baru |
| **Trace** | `02-backend-architecture.md` bagian 4 "Utang teknis"; `DEC-RSK-003` yang mewajibkan perapian menjadi task tersendiri |
| **Reuse** | Pola penamaan 25 folder modul lain |
| **Scope** | `Areas/HealthServices/EmergencyInstallationManagement/Controller/` menjadi `Controllers/`; `Repositories/Configurations/HealthService/` menjadi `HealthServices/`; namespace master IGD diselaraskan dengan foldernya |
| **Dependency** | Seluruh task slice S1 sampai S3 sudah selesai, agar perubahan nama berkas tidak bertabrakan dengan pekerjaan yang sedang berjalan |
| **Acceptance criteria** | 1. Nama folder dan namespace mengikuti pola standar. 2. **Tidak ada satu pun perubahan perilaku**, hanya perpindahan dan penyesuaian namespace. 3. Seluruh test yang sudah ada tetap lulus tanpa diubah isinya |
| **Verification** | Build lulus; seluruh test regression lulus; pemeriksaan diff membuktikan tidak ada perubahan logika |
| **Risk/blocker** | `DEC-RSK-003` melarang perapian ini diselipkan diam-diam ke tengah task lain. Jika muncul di dalam task lain, tolak dan kembalikan ke sini. Owner: Backend/API |
| **DoD** | Nama folder dan namespace standar; nol perubahan perilaku; arsitektur backend bagian 4 diperbarui karena utang teknis sudah lunas |

---

### `BE-IGD-014` — Bukti penerimaan lintas-slice dan kesiapan modul

| Field | Isi |
| --- | --- |
| **Outcome** | Pemilik proses memperoleh bukti bahwa modul benar-benar dapat dipakai perawat dan dokter dari awal sampai akhir, bukan sekadar kumpulan endpoint yang lulus sendiri-sendiri |
| **Trace** | Seluruh `AT-IGD-*` pada acceptance test matrix, khususnya yang membutuhkan lebih dari satu task, yaitu `AT-IGD-003`, `AT-IGD-023`, `AT-IGD-035`, dan `AT-IGD-070` |
| **Reuse** | Test yang sudah dibuat pada masing-masing task; task ini tidak menulis ulang, hanya menutup yang belum tercakup |
| **Scope** | Proyek test; tidak menyentuh kode aplikasi |
| **Dependency** | `BE-IGD-003`, `BE-IGD-004`, `BE-IGD-007`, `BE-IGD-009` |
| **Acceptance criteria** | 1. Seluruh `AT-IGD-*` yang dapat diuji sudah punya test dan hasilnya tercatat. 2. Tiga area yang belum dapat diuji, yaitu target level 2 sampai 5, break-glass, dan scope sumber daya, dinyatakan **belum diuji** secara eksplisit dan tidak dihitung lulus. 3. Satu alur menyeluruh terbukti: pasien tiba, dinilai, dinilai ulang, diobservasi, diputuskan tindak lanjutnya, lalu kunjungan diselesaikan |
| **Verification** | Rangkaian integration test; laporan hasil per `AT-IGD-*` |
| **Risk/blocker** | Menyatakan lulus untuk hal yang tidak diuji adalah pelanggaran paling berbahaya di sini. Acceptance matrix bagian penutup sudah melarangnya. Owner: Product/Domain |
| **DoD** | Laporan per `AT-IGD-*` lengkap; daftar yang belum dapat diuji tertulis apa adanya; siap dilanjutkan `/verify-module-readiness` |

---

## 5. Endpoint yang dihasilkan roadmap ini

Judul mengikuti nilai `[Tags(...)]` pada controller supaya dapat dicocokkan langsung dengan
halaman Swagger.

### Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`

| Method | Path | Kegunaan | Hak akses | Request | Response | Task |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/retriage` | Menilai ulang pasien; penilaian lama menjadi historis | `EmergencyTriage : Update` | `RetriageEmergencyTriageRequest` | Penilaian baru | `BE-IGD-004` |
| `GET` | `/sla-breaches` | Daftar pasien yang melewati batas waktu dan belum ditangani | `EmergencyTriage : Read` | Query unit dan rentang waktu | Daftar ringkas berhalaman | `BE-IGD-007` |

### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Task |
| --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan kunjungan secara klinis dan mengisi waktu selesai | `EmergencyVisit : Update` | `CompleteVisitRequest` | Pesan berhasil | `BE-IGD-009` |

Lima puluh dua endpoint lain sudah ada di kode dan tidak diubah kontraknya oleh roadmap ini.
Yang berubah bagi endpoint lama hanyalah kenyataan bahwa setelah `BE-IGD-001` mereka benar-benar
dapat dipanggil.

### Kode status dan artinya

| Kode | Arti teknis | Arti bagi pengguna |
| --- | --- | --- |
| `200` | Berhasil | Permintaan diproses dan datanya tersedia |
| `400` | Permintaan tidak valid | Isian tidak lengkap atau melanggar aturan bisnis |
| `401` | Belum masuk | Sesi habis; pengguna perlu masuk ulang |
| `403` | Tidak berwenang | Sudah masuk tetapi tidak punya hak untuk tindakan ini |
| `404` | Tidak ditemukan | Data sudah ditandai terhapus atau tidak pernah ada |
| `409` | Bentrok | Transisi status tidak sah, atau data sedang diubah pihak lain |

---

## 6. Gate yang menahan aktivasi, bukan pembangunan

Perbedaan ini penting supaya pekerjaan tidak berhenti tanpa alasan.

| Gate | Menahan apa | Tidak menahan apa |
| --- | --- | --- |
| SOP triase MMC belum ada | Pengisian target waktu level 2 sampai 5 | `BE-IGD-002`, `BE-IGD-006`, dan seluruh kode SLA tetap boleh dibangun |
| Security/privacy owner belum ditunjuk | Menyalakan `BE-IGD-012` di produksi | `BE-IGD-010`, `BE-IGD-011`, dan `BE-IGD-012` tetap boleh dibangun dan diuji |
| Clinical governance owner belum ditunjuk | Menjadikan skema triase sebagai aturan klinis yang disahkan | Pemakaian baseline Permenkes 47/2018 pada `BE-IGD-003` |
| `GovernanceAssignment` bernama belum ada | Klaim approval formal untuk go-live | Seluruh pekerjaan delivery |

Satu aturan yang tidak boleh dilanggar: **gate tidak pernah memblokir pelayanan klinis
darurat.** Bila sebuah gate mulai menghambat penanganan pasien, gate itu salah tempat dan harus
dibawa kembali ke owner.

---

## 6b. Tambahan setelah roadmap revisi 1 — `BE-IGD-015`

Task berikut tidak ada pada roadmap revisi 1. Ia lahir dari kebutuhan layar pengkajian pasien
IGD, dan dicatat di sini supaya tidak menjadi pekerjaan tanpa jejak.

### `BE-IGD-015` — Kejadian infeksi nosokomial dapat dicatat dan disurveilans

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat dapat mencatat kejadian infeksi yang diduga didapat selama pelayanan, dan tim pengendali infeksi dapat menelaah serta menetapkan statusnya. Sebelum ini tidak ada satu pun tempat menyimpannya — pencarian `nosokomial` di seluruh repository menghasilkan nol berkas |
| **Trace** | Kebutuhan layar pengkajian IGD; tidak berasal dari decision log revisi 1, sehingga **belum melewati `/grill-me`** |
| **Reuse** | Pola entity klinis milik pasien yang sudah dipakai `TrxPatientAllergy` dan `TrxPatientMedicalHistory`, termasuk cara menautkan pasien, encounter, dan unit pelayanan |
| **Scope** | `Areas/HealthServices/ClinicalManagement/` — enum, model `TrxNosocomialInfection`, DTO, dan `NosocomialInfectionController`; konfigurasi EF; migration `AddNosocomialInfection` |
| **Dependency** | `BE-IGD-001` |
| **Acceptance criteria** | 1. Kejadian dapat dicatat beserta jenis, waktu munculnya gejala, kriteria, dan kaitannya dengan pemakaian alat. 2. Catatan baru selalu berstatus `Suspected`; konfirmasi adalah tindakan tersendiri. 3. Menyatakan bukan infeksi terkait pelayanan wajib mengisi alasan. 4. Kejadian hanya dapat dinyatakan teratasi setelah dikonfirmasi. 5. Catatan yang sudah ditutup tidak dapat diubah isinya. 6. Selisih waktu terhadap waktu mulai dirawat tersimpan sebagai salinan, bukan dihitung ulang setiap laporan dibuat |
| **Verification** | Build lulus; pemeriksaan langsung ke basis data setelah migration diterapkan; `AT-IGD-*` **belum ada** karena solution masih tanpa proyek test |
| **Risk/blocker** | **Belum melewati wawancara owner.** Daftar jenis infeksi memakai istilah surveilans yang lazim dipakai rumah sakit Indonesia, tetapi daftar final serta kriteria penetapannya adalah wewenang tim PPI dan clinical governance. Owner: Clinical governance + tim PPI |
| **DoD** | Entity, controller, dan migration selesai; enam kriteria terbukti; daftar jenis infeksi disahkan tim PPI; laporan perubahan menyebut bahwa task ini di luar revisi 1 |

> **Mengapa rumahnya di Clinical Management, bukan di modul IGD:** surveilans infeksi berlaku
> untuk seluruh unit pelayanan — rawat inap, kamar operasi, dan ICU sama-sama melaporkannya.
> Menaruhnya di modul IGD berarti unit lain kelak membuat tabel keduanya untuk fakta yang sama,
> dan angka mutu rumah sakit dihitung dari dua sumber yang tidak pernah cocok.

---

## 7. Requirement tanpa task — coverage gap

Requirement berikut ada pada decision log tetapi **tidak** memiliki desain pada blueprint
revision 4, sehingga tidak dapat dijadikan task tanpa mengarang. Daftar ini adalah keluaran
yang diminta guardrail, bukan kelalaian.

| ID | Requirement yang belum tertutup | Bukti kekosongan | Langkah yang tepat |
| --- | --- | --- | --- |
| `CG-01` | Encounter provisional untuk pasien gawat dan pasien tidak dikenal (`IGD-DEC-002`, `IGD-DEC-016`) | `CAP-02` berstatus `Conflict`: `TrxPatientEncounter` mewajibkan `PatientId`, sedangkan alur provisional mensyaratkan sebaliknya. Arsitektur revision 4 tidak merancang jalan keluarnya | `/design-business-module` bersama owner Registration Management |
| `CG-02` | Mode korban massal atau bencana yang sudah masuk scope rilis awal (`IGD-DEC-009`) | `CAP-19` berstatus `Missing`; tidak ada satu pun rancangan pada revision 4 | `/grill-me` untuk menegaskan apakah tetap di rilis awal, lalu `/design-business-module` |
| `CG-03` | Lifecycle temporary patient dan reconciliation (`IGD-DEC-017` sampai `IGD-DEC-019`, `IGD-DEC-031`) | `CAP-03` berstatus `Repair`; tidak dirancang pada revision 4 | `/design-business-module` bersama owner Patient Management |
| `CG-04` | Serah terima billing saat kunjungan selesai dengan tagihan belum final (`IGD-DEC-021` butir 51) | `CAP-09` berstatus `Missing`; `BE-IGD-009` memang tidak boleh diblokir billing, tetapi catatan serah terimanya belum ada perancangnya | `/design-business-module` bersama owner Finance/Billing |
| `CG-05` | Tindak lanjut hasil penunjang yang datang terlambat (`IGD-DEC-024`, `IGD-DEC-032`) | `CAP-11` berstatus `Missing`; sistem pemilik order dan hasil bahkan belum bernama | `/grill-me` untuk menetapkan sistem dan owner-nya lebih dulu |
| `CG-06` | Keandalan lintas modul berupa outbox, inbox, dan rekonsiliasi (`IGD-DEC-025`, `IGD-DEC-033`) | `CAP-14` berstatus `Missing`. Integration contract menyatakan revisi ini tidak memanggil sistem luar, sehingga penundaan masih beralasan | Tinjau ulang saat kebutuhan integrasi pertama muncul |
| `CG-07` | Sifat append-only riwayat klinis benar-benar terlindungi (`IGD-DEC-029`) | Endpoint `PUT` dan `DELETE` umum pada triage, disposition, dan transfer masih dapat menimpa atau menghapus riwayat, termasuk baris `Superseded`. Retriage pada `BE-IGD-004` tidak menutup celah ini | `/design-business-module` untuk merancang pembatasan koreksi, lalu jadikan task tersendiri |

`CG-07` adalah yang paling mendesak dari daftar ini, karena ia melemahkan hasil `BE-IGD-004`
pada hari yang sama task itu selesai.

---

## 8. Risiko yang perlu diputuskan pemilik

| No | Risiko | Dampak bila diabaikan | Yang diminta dari owner |
| ---: | --- | --- | --- |
| 1 | Tipe data target waktu belum mampu menyatakan "belum diatur" | Seluruh pasien Kuning dan Hijau tampak terlambat sejak menit pertama; peringatan yang benar ikut tenggelam | Setujui migration tambahan pada `BE-IGD-002` |
| 2 | Dua dokumen menempatkan endpoint penyelesaian kunjungan di controller berbeda | Implementer menebak, lalu dokumen dan kode berbeda selamanya | Tegaskan `EmergencyVisitController`, lalu perbaiki arsitektur bagian 3.4 |
| 3 | Menutup bypass SuperAdmin sebelum break-glass ada | Saat darurat, tidak ada jalur akses sah sama sekali | Kunci urutan `BE-IGD-011` sebelum `BE-IGD-012` |
| 4 | Arti kolom `VisitCompletedAt` berubah di tengah jalan | Laporan lama tinggal pasien mencampur dua arti dan hasilnya salah | Setujui pencatatan batas waktu perubahan arti |
| 5 | Frekuensi pemindaian breach belum ditetapkan | Terlalu sering membebani basis data, terlalu jarang membuat penanda terlambat | Tetapkan nilai bawaan yang wajar dan dapat dikonfigurasi |
