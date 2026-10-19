# Patient Management — Penilaian Kelengkapan Requirement `BE-PAT-MIG-003` dan `BE-PAT-MIG-002`

| Field | Nilai |
| --- | --- |
| Assessment ID | `PAT-RCG-001` |
| Revision | `1` |
| Blueprint ID | `PAT-BP-001` revision `3` |
| Status | `final` — hasil gate, 15 Oktober 2026 (sesi ketiga) |
| **Hasil** | **`PASS` untuk kedua slice.** `S3` (`BE-PAT-MIG-003`) dan `S2` (`BE-PAT-MIG-002`, Canary-A) sama-sama **`READY_FOR_DOMAIN_DESIGN`**. Tidak ada butir `BLOCKING`, `MISSING` yang memblokir, atau `CONFLICT` |
| Amandemen | 16 Oktober 2026 — `PAT-DEC-067` mengubah dua usulan standar (`PAT-GAP-002`, `PAT-GAP-008`) atas instruksi pemilik sebelum build. Hasil gate tidak berubah: kedua slice tetap `READY_FOR_DOMAIN_DESIGN`. `PAT-DEC-068` (16 Oktober 2026, kedua) mengganti usulan `PAT-GAP-004` dengan kunci dua bagian tetap |
| Urutan penilaian | Mengikuti dependency: `S3` lebih dulu, lalu `S2` — gerbang `NORMAL_ALLOCATION_GUARD` milik `S2` memakai pemeriksa milik `S3` |
| Masukan | `00-interview-decisions.md` revision `3` (`PAT-DEC-017`–`066`, `PAT-FACT-030`–`089`, `PAT-SRC-001`–`029`, `PAT-OQ-008`–`037`); `contracts/api-contract.md` Bagian B usulan `1.2.0`; kartu `BE-PAT-MIG-002` dan `BE-PAT-MIG-003` pada `roadmap/backend-roadmap.md`. Hash masukan dicatat pada `blueprint-manifest.md` |
| SHA backend | `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` (`QuilvianIntegrationBackend`) — dibaca read-only |
| SHA frontend | `2831ea635d83c0eca3f2cfbb0ae3be383e3ce204` (`QuilvianSystemFrontendDev`) — hanya untuk konsumen QR |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan pada bagian 7 |
| `indonesia-hospital-domain-reference` | Tidak dipakai. Seluruh bukti berasal dari keputusan pemilik, data read-only yang diserahkan pemilik, dan source. Tidak ada gap yang butuh baseline rujukan |
| Task mode | `MODULE BLUEPRINT MODE` — read-only terhadap source, database, endpoint, dan filesystem aplikasi |

> **Cara membaca dokumen ini.** Dokumen ini hanya menjawab satu pertanyaan: *apakah requirement
> kedua task sudah cukup lengkap untuk dikunci dan dibangun?* Dokumen ini tidak merancang tabel,
> endpoint, atau layar baru, dan tidak menjawab keputusan bisnis atas nama pemilik.
>
> | Status bukti | Artinya |
> | --- | --- |
> | `CONFIRMED` | Didukung keputusan pemilik, data read-only dari pemilik, atau source untuk keadaan sekarang |
> | `PROPOSED` | Usulan standar agent yang **belum** diputuskan pemilik secara tersendiri; terlihat di sini dan ikut dikunci lewat `PAT-DEC-033` |
> | `MISSING` | Tidak ada dalam bukti |
> | `CONFLICT` | Dua sumber bertentangan tanpa penyelesaian |
>
> | Dampak gap | Artinya |
> | --- | --- |
> | `BLOCKING` | Dapat mengubah model data, wewenang, integritas data, atau alur yang sulit dibatalkan — desain harus berhenti |
> | `NON_BLOCKING_STANDARD` | Usulan standar yang aman dinyatakan eksplisit tanpa mengubah makna inti |
> | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar environment dan aman dijadikan konfigurasi |

---

## 1. Scope penilaian

| Slice | Kemampuan | Task | Keputusan inti |
| --- | --- | --- | --- |
| `S3` | Guard reservasi nomor pasien normal: pendaftaran pasien normal di Staging tidak pernah memakai `PatientCode` atau MRN yang dicadangkan migrasi RSMMC; ditambah pemeriksa baca-saja untuk worker | `BE-PAT-MIG-003` | `PAT-DEC-042`, `054`, `055`, `057`, `064`, `066` |
| `S2` | Worker migrasi massal pasien RSMMC, terbatas pada manifest Canary-A `RSMMC-CANARY-A` v1 | `BE-PAT-MIG-002` | `PAT-DEC-017`–`032`, `034`–`053`, `056`, `058`–`063`, `065` |

Di luar penilaian: Canary-B (`PAT-GATE-003`) dan gelombang besar (`PAT-GATE-004`). Keduanya
dirancang lewat revisi blueprint terpisah sesudah Canary-A lulus runtime.

---

## 2. Bukti yang dipakai

| Bukti | Wewenang | Isi yang dipakai |
| --- | --- | --- |
| Keputusan tertulis pemilik, 14 dan 15 Oktober 2026 (tiga sesi) | Requirement eksplisit pemilik — tertinggi | `PAT-DEC-017`–`066`; status `PAT-OQ-008`–`037` |
| Data database read-only yang diserahkan pemilik | Pernyataan pemilik, dinyatakan otoritatif; agent tidak mengakses database | `PAT-FACT-030`–`089`, termasuk skema fisik (`079`–`083`), bukti MRN (`084`), rentang `legacy_pid` (`086`), keutuhan Canary-A (`087`), dan riwayat MRN `00-00-00-27` (`089`) |
| Source backend `a52b3c98` | Keadaan sekarang | `PAT-SRC-001`–`029` |
| Source frontend `2831ea63` | Keadaan sekarang, konsumen QR saja | `PAT-SRC-019`, `020` |

Tidak ada pertentangan urutan waktu. Bukti sesi ketiga melengkapi — tidak membalik — keputusan
sesi sebelumnya; satu-satunya teks yang dicabut adalah status `BLOCKING` keempat pertanyaan.

**Status pertanyaan per sesi ketiga.** 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, **0 `BLOCKING`**,
0 `PROPOSED` dari 30 pertanyaan (`PAT-OQ-008`–`037`). Isu P0: `PAT-P0-001` diputuskan
(`PAT-DEC-052`), `PAT-P0-002` diputuskan (`PAT-DEC-054`, `066`), `PAT-P0-003` diputuskan
(`PAT-DEC-064`).

---

## 3. Temuan `S3` — `BE-PAT-MIG-003`, guard reservasi nomor pasien normal

| No | Dimensi | Temuan | Status |
| ---: | --- | --- | --- |
| 01 | Tujuan | Reservasi `PatientCode` 742–705.249 dan seluruh MRN akhir `FINALIZED` (numerik 27–890.187) tidak dapat diambil pendaftaran pasien normal, sehingga pasien RSMMC pemiliknya tetap dapat dibuat dan folder QR historis Pilot tidak ditimpa | `CONFIRMED` — `PAT-DEC-042`, `054`, `057`, `064` |
| 02 | Aktor | Petugas pendaftaran, kiosk, dan admin — pelaku yang sama dengan sekarang. Pemilik mengatur konfigurasi `RsmmcMigrationReservationGuard.Enabled` saat deployment Staging | `CONFIRMED` — `PAT-SRC-023`, `027`; `PAT-DEC-066` |
| 03 | Pemicu / prasyarat | `POST /`, `POST /kiosk`, `POST /admin`. Guard berlaku hanya bila `EnvironmentName == "Staging"` persis **dan** `Enabled == true` | `CONFIRMED` — `PAT-DEC-066` |
| 04 | Alur utama | `CreatePatient` membuka transaksi (baris 510), mengambil kunci database, memilih suffix `PatientCode` pertama yang belum dipakai ≥ 705.250 dan MRN numerik pertama yang belum dipakai ≥ 890.188, menyimpan, lalu *commit* (baris 639) | `CONFIRMED` — `PAT-DEC-054`, `064`; `PAT-SRC-028` |
| 05 | Alur alternatif / exception | (a) Di Staging persis, konfigurasi hilang atau tidak sah → **gagal tertutup**: pendaftaran ditolak, tidak jatuh ke penomoran lama. (b) Di luar Staging persis → penomoran berjalan seperti sekarang. *Diamandemen 16 Oktober 2026 (`PAT-DEC-067`): `Enabled = false` eksplisit di Staging persis kini ikut gagal tertutup; teks lama: "atau `Enabled = false` eksplisit".* (c) Dua pendaftaran bersamaan → yang kedua menunggu kunci. (d) MRN melewati 99.999.999 → ditolak seperti sekarang. (e) Indeks unik tetap jaring terakhir | (a)(b) `CONFIRMED` — `PAT-DEC-066`; kode galat (a) `PROPOSED` (`PAT-GAP-002`); (c) `CONFIRMED` — `PAT-DEC-054`; (d)(e) `CONFIRMED` — source |
| 06 | Data minimum | Tidak ada data persisten baru. Satu kunci konfigurasi `RsmmcMigrationReservationGuard:Enabled`. Lantai 705.250 dan 890.188 | Kunci konfigurasi dan nilai lantai `CONFIRMED` — `PAT-DEC-066`; letak lantai sebagai konstanta `PROPOSED` (`PAT-GAP-003`) |
| 07 | Aturan bisnis / validasi | Pertama yang belum dipakai ≥ lantai; tidak pernah di bawah lantai selama kampanye aktif; serialisasi database; indeks unik dipertahankan | `CONFIRMED` — `PAT-DEC-054`, `064` |
| 08 | Status / perubahan status | Tidak ada lifecycle baru. Aktif-tidaknya guard dihitung dari environment dan konfigurasi, tidak disimpan | `CONFIRMED` |
| 09 | Peran / authorization | Tidak berubah: `POST /` dan `/kiosk` policy `KioskRead`; `POST /admin` `Patient : Create`. Pemeriksa baca-saja adalah service internal, bukan endpoint HTTP | Akses `CONFIRMED` — `PAT-SRC-027`; pemeriksa sebagai service internal `PROPOSED` (`PAT-GAP-008`) |
| 10 | Dependency antarmodul | Modul lain hanya menerima `PatientCode`/MRN dengan format yang sama; bentuk request/response tidak berubah. `BE-PAT-MIG-002` memakai pemeriksanya | `CONFIRMED` — kontrak B14.1; `PAT-DEC-057` |
| 11 | Integrasi | Tidak ada integrasi eksternal. Kunci transaksi PostgreSQL; konfigurasi dari deployment | `CONFIRMED` |
| 12 | Hasil akhir | Di Staging, `PatientCode` normal berikutnya ≥ `PAT-RSMMC-705250` dan MRN normal berikutnya ≥ `00-89-01-88`; `PAT-GATE-005` `PASS`; sesudah itu pemilik boleh mencabut pembekuan pendaftaran | `CONFIRMED` — `PAT-DEC-055`, `064`, `066` |
| 13 | Pembatalan / koreksi | Guard di Staging persis tidak dapat dimatikan lewat konfigurasi; `Enabled` selain `true` gagal tertutup (`PAT-DEC-067`, 16 Oktober 2026). *Teks lama: "Guard dimatikan pemilik lewat `Enabled = false` atau deployment lain."* Nomor yang sudah diberikan tidak ditarik atau diberi ulang | Mekanisme mati `CONFIRMED` — bunyi `PAT-DEC-066`; nomor tidak ditarik `PROPOSED` (`PAT-GAP-007`) |
| 14 | Audit / histori | `CreateBy`/`CreateDateTime` pada `MstPatient` tetap. Penolakan karena konfigurasi dicatat di log tanpa data pribadi. Bukti `PAT-GATE-005` dicatat pemilik | Kolom audit `CONFIRMED`; log penolakan `PROPOSED` (`PAT-GAP-002`) |
| 15 | Notifikasi | **Tidak berlaku secara material.** Tidak ada kejadian bisnis baru yang perlu diberitahukan; perubahan nomor terlihat langsung pada respons pendaftaran | — |
| 16 | Billing / charge | **Tidak berlaku secara material.** Tidak ada charge dibuat atau diubah; billing merujuk pasien lewat `Id`, dan format `PatientCode`/MRN tetap | — |
| 17 | Keselamatan klinis | Relevan: identifikasi pasien. Tanpa guard, pasien normal dapat memegang MRN yang dicadangkan untuk pasien RSMMC lain, sehingga satu nomor dapat menunjuk dua orang di rekam medis lama dan baru. Guard menutup risiko itu; indeks unik tetap jaring terakhir | `CONFIRMED` — `PAT-FACT-072`, `073`, `089` |
| 18 | Pelaporan / traceability | `PAT-MIG3-AC-01`–`14`, traceability bagian 6, bukti `PAT-GATE-005` | `CONFIRMED` |

**Contoh alur.** Di Staging dengan `Enabled = true`, belum ada `PAT-RSMMC-` ≥ 705.250 dan belum ada
MRN ≥ 890.188. Petugas kiosk mendaftarkan "Budi Contoh": ia mendapat `PAT-RSMMC-705250` dan
`00-89-01-88`. Pendaftaran berikutnya mendapat `PAT-RSMMC-705251` dan `00-89-01-89`. Di Produksi,
penomoran tetap memilih nomor kosong terkecil seperti sekarang.

---

## 4. Temuan `S2` — `BE-PAT-MIG-002`, worker migrasi Canary-A

| No | Dimensi | Temuan | Status |
| ---: | --- | --- | --- |
| 01 | Tujuan | Pasien RSMMC kanonik yang belum ada di Quilvian dibuat persis dari target preview yang sudah diaudit, tanpa nomor baru, tanpa pasien ganda, dan dapat dilanjutkan | `CONFIRMED` — `PAT-DEC-017`, `020`–`025` |
| 02 | Aktor | Admin migrasi dengan `Patient : Update`; pemilik/leader memberi otorisasi `PAT-GATE-002` (a) dan (b) | `CONFIRMED` — `PAT-DEC-028`, `029` |
| 03 | Pemicu / prasyarat | Panggilan admin di Staging persis; `PAT-GATE-001` `CLOSED/PASS`; `PAT-GATE-005` `PASS`; seluruh gerbang B4.2 lulus | `CONFIRMED` — `PAT-DEC-029`, `055`, `057` |
| 04 | Alur utama | 13 langkah B4.3: transaksi per pasien, kunci dua bagian, baca ulang, insert pasien, tulis `IsMember = false`, insert crosswalk, verifikasi baca ulang, QR `CreateNew` di `rsmmc-bulk`, *commit* | `CONFIRMED` — `PAT-DEC-023`, `047`, `048`, `052`, `053` |
| 05 | Alur alternatif / exception | Kasus A–J; berhenti pada `CONFLICT`/`FAILED` tak terduga pertama; simulasi tidak pernah berhenti; `CONCURRENT_RUN`; `LOCK_KEY_OUT_OF_RANGE` | `CONFIRMED` — `PAT-DEC-036`, `048`; kontrak B4.5 |
| 06 | Data minimum | Kolom preview, perencana, dan crosswalk dengan tipe fisik `PAT-FACT-079`–`083`. Tipe `full_name` dan `birth_date` tidak tercantum pada ringkasan skema | Tipe yang dilaporkan `CONFIRMED` — `PAT-DEC-063`; tipe `full_name`/`birth_date` `MISSING` (`PAT-GAP-009`) |
| 07 | Aturan bisnis / validasi | `PatientCode` preview apa adanya; MRN akhir perencana = MRN usulan preview; tanpa pencocokan NIK; tanpa nama pengganti; nilai enum dipetakan ketat | `CONFIRMED` — `PAT-DEC-020`–`022`, `030`; perlakuan NULL/nilai tidak dikenal `PROPOSED` (`PAT-GAP-010`) |
| 08 | Status / perubahan status | Crosswalk baru `TARGET_CREATED` / `MIGRATED_BULK_WORKER` — muat `varchar(30)`, tanpa trigger; preview dan crosswalk historis tidak berubah | `CONFIRMED` — `PAT-DEC-034`, `035`, `051`; `PAT-FACT-080`, `081` |
| 09 | Peran / authorization | `[AccessAction("Update")]` + `[AccessPermission("Patient", "Update")]`; aktor wajib dikenali | `CONFIRMED` — `PAT-DEC-028`, `044` |
| 10 | Dependency antarmodul | Pemeriksa baca-saja `BE-PAT-MIG-003` untuk `NORMAL_ALLOCATION_GUARD`; komponen `BE-PAT-MIG-001` dipakai ulang | `CONFIRMED` — `PAT-DEC-055`, `057` |
| 11 | Integrasi | Tabel `migration.rsmmc_*` lewat SQL berparameter **bertipe**, tanpa fallback `::text`; filesystem QR | `CONFIRMED` — `PAT-DEC-063`, `052` |
| 12 | Hasil akhir | Per pasien: satu `MstPatient`, satu baris crosswalk, satu QR di `rsmmc-bulk`. Eksekusi ulang → `ALREADY_MIGRATED` | `CONFIRMED` — `PAT-MIG2-AC-35`, `36` |
| 13 | Pembatalan / koreksi | Sebelum *commit*: rollback per pasien, QR hanya dihapus bila kepemilikan terbukti. Sesudah *commit*: tidak ada pembalikan otomatis | Sebelum *commit* `CONFIRMED` — `PAT-DEC-024`; sesudah *commit* `PROPOSED` (`PAT-GAP-016`) |
| 14 | Audit / histori | `CreateBy`/`CreateDateTime`; `created_at` crosswalk; log tanpa data pribadi; Pilot dan cadangan immutable | `CONFIRMED` — `PAT-DEC-025`, `039`, `049` |
| 15 | Notifikasi | **Tidak berlaku secara material.** Aksi admin sekali jalan di Staging; hasil lengkap dikembalikan pada respons | — |
| 16 | Billing / charge | **Tidak berlaku secara material.** Tidak ada kunjungan, order, atau charge yang dibuat | — |
| 17 | Keselamatan klinis | Relevan: identifikasi pasien. Pasien tidak pernah digabung lewat NIK; tabrakan identitas/telepon → `CONFLICT`; MRN akhir unik 705.223 (`PAT-FACT-084`); QR berisi MRN akhir | `CONFIRMED` — `PAT-DEC-022`, `050`; `PAT-FACT-084` |
| 18 | Pelaporan / traceability | `PAT-MIG2-AC-01`–`51`; respons memuat identifier, versi, dan sidik jari manifest | `CONFIRMED` |

**Pemeriksaan silang bukti sesi ketiga terhadap desain `S2`.**

| Bukti | Desain yang bergantung padanya | Hasil |
| --- | --- | --- |
| `PAT-FACT-086` — seluruh `legacy_pid` kanonik di rentang Int32 | Kunci dua bagian `PAT-DEC-048` | Sah untuk seluruh batch kanonik; pemeriksaan rentang tetap ada sebagai pengaman |
| `PAT-FACT-087` — Canary-A 25/25/25/25/25 | Manifest `RSMMC-CANARY-A` v1 | Utuh; urutan tetap berwenang |
| `PAT-FACT-084` — MRN akhir unik 705.223, format tidak sah 0 | `PLANNER_MISMATCH`, `MRN_ALREADY_USED` | Perencana konsisten |
| `PAT-FACT-089` — `00-00-00-27` sementara Pilot 797015, kanonik -667995 | Path `rsmmc-bulk` (`PAT-DEC-052`); kandidat nomor 5 | Kandidat 5 menulis ke `rsmmc-bulk/00-00-00-27/`; QR historis 797015 di path bawaan tidak disentuh |
| `PAT-FACT-079`, `080` — crosswalk `varchar(30)`, FK `last_batch_id` | Langkah `INSERT_CROSSWALK` | Muat tanpa perubahan skema; baris batch dijaga `BATCH_ROW_EXISTS` |
| `PAT-SRC-029` — default database kolom enum `MstPatient` | Langkah `INSERT_PATIENT` | Hanya `IsMember` yang terkena cacat default EF; kolom enum aman bila nilai tidak dikenal ditolak |

**Contoh.** Kandidat nomor 5 (`legacy_pid` -667995) dibuat dengan `PAT-RSMMC-00742` dan
`00-00-00-27`. QR barunya ditulis ke `patient-qrcodes/rsmmc-bulk/00-00-00-27/qrcode.png`, sementara
`patient-qrcodes/00-00-00-27/qrcode.png` milik riwayat Pilot 797015 tetap byte demi byte.

---

## 5. Daftar gap dan dampaknya

Seluruh gap di bawah **tidak** mengubah model data persisten, wewenang, makna klinis, atau alur yang
sulit dibatalkan. Setiap usulan tetap terlihat, tidak disajikan sebagai keputusan pemilik, dan ikut
dikunci lewat `PAT-DEC-033`.

| ID | Slice | Gap | Status | Dampak | Usulan dan alasan |
| --- | --- | --- | --- | --- | --- |
| `PAT-GAP-002` | `S3` | Arti "konfigurasi hilang atau tidak sah" dan bentuk penolakannya | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak sah = bagian `RsmmcMigrationReservationGuard` tidak ada, `Enabled` tidak ada, atau nilainya bukan boolean. Pendaftaran ditolak `503` dengan kode `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` dan log peringatan tanpa data pribadi. Hanya berlaku di Staging persis. *Diamandemen 16 Oktober 2026 (`PAT-DEC-067`): `Enabled = false` di Staging persis juga gagal tertutup. Teks lama: "`Enabled = false` yang ditulis eksplisit bukan tidak sah: guard mati, sesuai bunyi `PAT-DEC-066`".* |
| `PAT-GAP-003` | `S3` | Letak nilai lantai | `PROPOSED` | `NON_BLOCKING_STANDARD` | 705.250 dan 890.188 menjadi konstanta di profil reservasi, bukan konfigurasi, supaya tidak dapat diturunkan lewat konfigurasi. Konfigurasi hanya menyalakan atau mematikan |
| `PAT-GAP-004` | `S3` | Bentuk kunci serialisasi allocator | `PROPOSED` | `NON_BLOCKING_STANDARD` | *Diamandemen 16 Oktober 2026 (`PAT-DEC-068`): `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE), bukan `hashtext()`. Teks lama:* `pg_advisory_xact_lock(hashtext('quilvian.patient.identifier-allocation'))` satu kunci — pola yang sudah ada (`PAT-SRC-026`) — di dalam transaksi `CreatePatient` yang sudah ada (`PAT-SRC-028`), menunggu, hanya saat guard aktif. Ruang kuncinya tidak bersinggungan dengan kunci dua bagian worker |
| `PAT-GAP-005` | `S3` | Perilaku saat guard tidak aktif | `PROPOSED` | `NON_BLOCKING_STANDARD` | Algoritma "nomor kosong terkecil" dipindah ke module service **tanpa perubahan hasil** dan tanpa kunci baru, sehingga penomoran Produksi tidak berubah sesuai `PAT-DEC-066` |
| `PAT-GAP-006` | `S3` | Isi uji PostgreSQL provider-real untuk allocator | `PROPOSED` | `NON_BLOCKING_STANDARD` | Urutan pemilik menempatkan uji provider-real tepat sesudah `BE-PAT-MIG-003`. Isi minimum: lantai, rentang cadangan, dua pendaftaran bersamaan memperoleh nomor berbeda, mode tanpa guard tidak berubah, gagal tertutup. Basis datanya PostgreSQL 15 sekali pakai yang sama dengan `PAT-DEC-065` |
| `PAT-GAP-007` | `S3` | Nomor yang sudah diberikan saat guard dimatikan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak ditarik dan tidak diberi ulang. Menarik nomor pasien yang sudah terdaftar adalah perubahan data pasien di luar cakupan |
| `PAT-GAP-008` | `S3` | Bentuk keluaran pemeriksa baca-saja | `PROPOSED` | `NON_BLOCKING_STANDARD` | *Diamandemen 16 Oktober 2026 (`PAT-DEC-067`): juga endpoint admin baca-saja khusus Staging `GET /admin/migration/rsmmc-reservation-guard`, `Patient : Update`, dengan nama field instruksi pemilik (kontrak `1.2.1` B14.2). Teks lama:* Service internal, bukan endpoint HTTP. Keluaran: nama environment, `guardActive`, `configurationValid`, `nextPatientCode`, `nextMedicalRecordNumber`, lantai kode, lantai MRN. Tidak mengalokasikan dan tidak menulis |
| `PAT-GAP-009` | `S2` | Tipe kolom preview `full_name` dan `birth_date` | `MISSING` | `NON_BLOCKING_STANDARD` | Kontrak memakai `text` dan `date`. Gerbang baru `PREVIEW_SCHEMA_SIGNATURE` membaca `information_schema` dan menolak seluruh panggilan — sebelum satu pasien pun diproses — bila kolom yang dipakai worker tidak ada atau tipenya berbeda. Gagal tertutup; bila gagal, pemilik memutuskan lewat revisi |
| `PAT-GAP-010` | `S2` | Nilai NULL atau tidak dikenal pada kolom enum/boolean preview | `PROPOSED` | `NON_BLOCKING_STANDARD` | `gender` NULL → `Gender` NULL, karena kolomnya *nullable*. Enum lain dan keempat boolean NULL → `PREVIEW_VALUE_INVALID`. Nilai yang bukan anggota enum — termasuk 0 untuk `PatientType`/`PatientStatus` — → `PREVIEW_VALUE_INVALID`. Tidak ada pemetaan otomatis ke `Unknown`; simulasi Canary-A menampilkannya lebih dulu |
| `PAT-GAP-011` | `S2` | Kelompok sumber per `legacy_pid` untuk 25 kandidat | `MISSING` | `NON_BLOCKING_STANDARD` | Tidak ditebak. Gerbang `MANIFEST_INTEGRITY` memeriksa sebaran 7/6/6/6 dari kolom `source_group` (`PAT-FACT-083`) saat runtime dan gagal tertutup |
| `PAT-GAP-012` | `S2` | Kolom perencana `target_collision` dan `target_collision_patient_id` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak dipakai sebagai syarat. Tabrakan diperiksa langsung ke `MstPatient` sesudah kunci (`PAT-MIG2-AC-07`, `08`), sehingga kolom perencana yang mungkin basi tidak dapat meloloskan pasien |
| `PAT-GAP-013` | `S2` | Letak status normalisasi `READY_FOR_TARGET_PREVIEW` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak dibutuhkan. `DEFERRED_SET` dihitung dari perencana, preview, dan Pilot |
| `PAT-GAP-014` | `S2` | Harness uji provider-real `PAT-DEC-065` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Connection string server PostgreSQL 15 sekali pakai dari environment variable; test membuat basis data acak, membentuk skema `MstPatient` dari model EF aplikasi — termasuk default `IsMember = true` — membuat tabel `migration.*` sesuai `PAT-FACT-079`–`083`, lalu menghapus basis data itu. Bila bukti diminta tetapi server tidak tersedia, test **gagal**, tidak dilewati. Tanpa dependency baru seperti Testcontainers. Server sekali pakai disediakan pemilik atau developer saat build |
| `PAT-GAP-015` | `S2` | Kriteria `NORMAL_ALLOCATION_GUARD` sesudah lantai MRN | `CONFIRMED` | — | Lulus bila pemeriksa melaporkan guard aktif, konfigurasi sah, `PatientCode` berikutnya ≥ `PAT-RSMMC-705250`, dan MRN berikutnya ≥ `00-89-01-88` (`PAT-DEC-064`, `066`) |
| `PAT-GAP-016` | `S2` | Pembalikan pasien hasil migrasi sesudah *commit* | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak ada pembalikan otomatis atau endpoint pembalikan. Membatalkan pasien yang sudah ter-*commit* adalah operasi database terpisah yang butuh desain dan otorisasi pemilik. Risikonya ditekan simulasi wajib, tinjauan pemilik, dan berhenti pada konflik pertama |

Tidak ada butir `CONFIGURABLE_DEFAULT`. Lantai sengaja **tidak** dijadikan konfigurasi
(`PAT-GAP-003`), karena menurunkannya akan merusak integritas reservasi.

---

## 6. Decision Log

Tidak ada ambiguitas pemblokir baru, sehingga tidak ada decision ID baru yang dibuka. Keputusan
yang menutup seluruh blocker sebelumnya:

| Decision ID | Pertanyaan yang ditutup | Slice | Status |
| --- | --- | --- | --- |
| `PAT-DEC-063` | `PAT-OQ-019` — skema fisik; SQL bertipe tanpa fallback `::text` | `S2` | `approved` — pemilik, sesi ketiga |
| `PAT-DEC-064` | `PAT-OQ-031` — lantai MRN 890.188 | `S3` (dan gerbang `S2`) | `approved` — pemilik, sesi ketiga |
| `PAT-DEC-065` | `PAT-OQ-036` — PostgreSQL 15 sekali pakai | `S2`; dipakai juga `S3` | `approved` — pemilik, sesi ketiga |
| `PAT-DEC-066` | `PAT-OQ-037` — Staging persis **dan** `Enabled == true` | `S3` | `approved` — pemilik, sesi ketiga |
| `PAT-DEC-033` | Persetujuan desain kedua task | `S2`, `S3` | Dicatat `approved` sesudah gate ini `PASS`, sesuai instruksi bersyarat pemilik sesi ketiga |

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Catatan |
| --- | --- | --- |
| `S3` — `BE-PAT-MIG-003` | **`READY_FOR_DOMAIN_DESIGN`** | Tidak menunggu task lain |
| `S2` — `BE-PAT-MIG-002` | **`READY_FOR_DOMAIN_DESIGN`** | **Build** menunggu `BE-PAT-MIG-003` ✅, karena gerbang `NORMAL_ALLOCATION_GUARD` memakai pemeriksanya. **Runtime** menunggu `PAT-GATE-005` `PASS`. Keduanya dependency penyerahan, bukan kekurangan requirement |

**Arsitektur domain.** `hospital-domain-architect` tidak dijalankan (`DOMAIN_ARCHITECTURE_NOT_RUN`).
Kedua slice bekerja di dalam aggregate `MstPatient` yang sudah ada di modul pemiliknya. Tidak ada
konsep domain, status, relasi, atau entity persisten baru. Dampak lintas modul hanya berupa
rentang nomor pasien normal baru di Staging dengan format yang sama, ditambah bertambahnya pasien
yang dapat dicari. Pemilik tetap boleh memintanya.

---

## 8. Yang boleh berjalan dan yang harus berhenti

**Boleh berjalan sekarang — pekerjaan dokumen saja.**
- `design-business-module`: mengunci kontrak `1.2.0` — Bagian B dan B14 — dengan butir
  `PAT-GAP-002`–`016`.
- `plan-module-delivery`: membuka kartu `BE-PAT-MIG-003` dan `BE-PAT-MIG-002`.

**Boleh berjalan sesudah itu, masing-masing dalam sesi `TASK MODE: BACKEND` tersendiri lewat
`build-module-backend`.**
- Build `BE-PAT-MIG-003` lebih dulu.
- Build `BE-PAT-MIG-002` sesudah `BE-PAT-MIG-003` ✅.

**Harus berhenti sampai ada otorisasi terpisah.**
- Commit, push, dan deployment.
- Eksekusi database, termasuk pembuatan basis data sekali pakai di luar test yang disetujui.
- Pemanggilan endpoint.
- Pembuatan QR.
- Simulasi maupun eksekusi Canary-A (`PAT-GATE-002`).
- Pencabutan pembekuan pendaftaran Staging (sesudah `PAT-GATE-005`).
- Canary-B dan gelombang besar.

---

## 9. Keputusan pemilik yang dibutuhkan

**Untuk desain: tidak ada.** Butir `PROPOSED` pada bagian 5 tidak memblokir. Pemilik boleh
menolak salah satunya lewat revisi blueprint sebelum build terkait dimulai.

**Untuk build, bukan keputusan desain:** server PostgreSQL 15 sekali pakai beserta connection
string-nya disediakan saat build (`PAT-GAP-014`).

**Untuk runtime:** `PAT-GATE-005`, inventaris filesystem (`PAT-DEC-062`), `PAT-GATE-002` (a) dan (b).

---

## 10. Handoff

| Field | Nilai |
| --- | --- |
| Skill berikutnya | `design-business-module` (finalisasi kontrak `1.2.0`), lalu `plan-module-delivery` (kartu dan traceability). `hospital-domain-architect` dilewati — bagian 7 |
| Slice yang diserahkan | `S3` `BE-PAT-MIG-003`, lalu `S2` `BE-PAT-MIG-002` |
| Decision ID | `PAT-DEC-017`–`066` |
| Gap terbuka yang dibawa | `PAT-GAP-002`–`016` — seluruhnya `NON_BLOCKING_STANDARD` atau `CONFIRMED` |
| Dependency | `BE-PAT-MIG-002` → `BE-PAT-MIG-001` ✅, `BE-PAT-MIG-003`. Runtime: `PAT-GATE-005` sebelum `PAT-GATE-002` |
| Keluaran hilir yang diharapkan | Kontrak `1.2.0` `approved`; kartu kedua task dibuka dengan acceptance criteria final; urutan build `BE-PAT-MIG-003` → `BE-PAT-MIG-002` |
