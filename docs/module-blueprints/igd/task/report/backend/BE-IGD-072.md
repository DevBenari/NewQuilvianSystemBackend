# Laporan Perubahan Backend — `BE-IGD-072`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-072` |
| Judul | Form Permintaan Rawat Inap dari draf sampai *Menunggu TTD* |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 gelombang 1 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-109`, `110`, `111`, `112` (minta TTD, *Perlu TTD*, kembalikan), `115` (batal sebelum kirim); `AT-IGD-221`…`224`, `227` |
| Keputusan | `IGD-DEC-268`, `271`, `273`, `274`, `278`, `282`…`294`, `297`, `299`; **`IGD-DEC-300`** (metode aksi `POST`, diputuskan saat build) |
| Contract version | API **`0.17.1`** §12.2–12.3 (kecuali `sign` dan `print-data`) — `0.17.0` `approved` `IGD-DEC-294`, dikoreksi `IGD-DEC-300`; validation **`0.16.0`** §14 aturan 1–14, 17, 19 (sebelum kirim), 26; state **`0.11.0`** §11.1; permission/audit **`0.10.0`** §11; kamus data §10; `02-backend-architecture.md` §17 |
| Dependency | — (gelombang 1) |
| Klasifikasi | `HEAVY` — 9 berkas baru, 8 berkas lama disentuh, 9 endpoint baru, 2 tabel baru + 1 kolom (migration 1), transaksi dengan kunci per kunjungan |
| Task mode | `BACKEND` — izin pemilik *"lanjutkan saja"* (12 Oktober 2026) sesudah rencana `IGD-DEC-297` dan prioritas `IGD-DEC-298` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): berkas pada bagian 3.2; dokumen blueprint IGD untuk `IGD-DEC-300`, laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6a1c6cf9` (`rizkiG`) + working tree |
| Tanggal | 12 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — source lengkap, QBE `PASS`; migration dan build oleh Rizki 15.01–15.02; putaran A dinilai dari bukti mentah: 12 dari 14 terbukti, A-01 sebagian, A-11 tidak terbukti ([penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026)); jumlah warning build dan penjaga `Down()` belum** |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` — dua entity, dua enum, dua configuration, DTO, service `partial`, controller. `TOUCHED LEGACY` — `EmergencyDispositionService`, `EmergencyDispositionController`, `EmergencyDispositionDtos`, `EmgDispositionType` beserta DTO, controller, dan seeder master-nya, `ApplicationDbContext` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| `Program.cs` | Nol baris — service baru tinggal di berkas `partial` `EmergencyDispositionService` yang sudah terdaftar; dua dependency baru (`PatientAllergyQueryService`, `EncounterInsuranceService`) sudah terdaftar `Scoped` (`Program.cs:476`, `:697`) |

---

## 1. Masalah yang diselesaikan

Sebelum task ini, perawat IGD yang pasiennya diputuskan **rawat inap** tidak punya tempat untuk mengisi Form Permintaan
Rawat Inap. Keputusan dokter berhenti pada tindak lanjut `RANAP`; unit tujuan, kelas, DPJP rawat inap usulan, lama
rawat, dan indikasi klinis dikirim lewat telepon atau kertas, dan DPJP IGD tidak punya daftar form yang menunggu tanda
tangannya.

Contoh. Tn. Budi (nyeri dada, troponin naik) diputuskan rawat inap oleh dr. Andi pukul 10.05. Perawat Dimas harus
menelepon admisi, menyebut "Bangsal Melati, kelas 2, dr. Sinta", dan dr. Andi menandatangani kertas di meja jaga.
Tidak ada jejak siapa mengisi apa dan kapan.

Task ini membuat sisi IGD dari form itu: perawat membuat draf dari tindak lanjut, mengisinya, meminta tanda tangan;
DPJP IGD aktif melihat daftar *Perlu TTD* dan dapat mengembalikan form dengan catatan; perawat dapat membatalkan
selama form belum ditandatangani. Penandatanganan, pengiriman ke admisi, dan cetak adalah `BE-IGD-074`.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi | Keadaan form |
| ---: | --- | --- | --- |
| 1 | dr. Andi | Mengonfirmasi tindak lanjut berjenis rawat inap (`RequiresAdmissionRequest = true`) | — |
| 2 | Perawat Dimas | *Buat Form* → draf lahir. Unit diisi dari unit tujuan tindak lanjut, indikasi dari alasan tindak lanjut, rencana perawatan dari instruksi tindak lanjut; lama rawat bawaan 3 hari | *Draf* |
| 3 | Perawat Dimas | Melengkapi unit, kelas, DPJP usulan (pilihan disaring jadwal unit), lama rawat, CITO, kebutuhan khusus, indikasi, rencana; *Simpan Draf* berkali-kali | *Draf* |
| 4 | Perawat Dimas | *Minta TTD* — sistem memeriksa isian wajib dan DPJP IGD aktif yang punya akun | *Menunggu TTD* |
| 5 | dr. Andi | Membuka daftar *Perlu TTD*: hanya form yang DPJP IGD aktifnya dr. Andi, CITO paling atas lalu terlama menunggu | *Menunggu TTD* |
| 6a | dr. Andi | *Kembalikan* dengan catatan, misalnya "Kelas tidak sesuai hak penjamin" | *Draf* |
| 6b | dr. Andi | *Tandatangani* — **`BE-IGD-074`** | *Ditandatangani* |
| — | Perawat Dimas | *Batalkan Form* beralasan selama *Draf* atau *Menunggu TTD* | *Dibatalkan* |

Setiap perpindahan keadaan dicatat di `EmgAdmissionRequestEvent` (tambah-saja): pelaku, waktu server, keadaan
sebelum dan sesudah, catatan atau alasan.

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Tindak lanjut masih *Draf* atau jenisnya bukan rawat inap | `409` aturan 1 |
| Kunjungan belum punya pasien atau encounter | `409` aturan 2 |
| Dua perawat menekan *Buat Form* hampir bersamaan | Kunci per kunjungan (`pg_advisory_xact_lock`) + index unik bersaring; yang kedua `409` aturan 3 |
| Pasien sudah punya permintaan admisi `Pending` dari sumber lain (mis. kamar pulih) | `409` aturan 4 |
| Form diubah orang lain sejak dibuka | `409` aturan 26 |
| DPJP IGD dialihkan dari dr. Andi ke dr. Rina sesudah *Minta TTD* | Form pindah ke daftar *Perlu TTD* dr. Rina; dr. Andi menekan *Kembalikan* → `403` aturan 13 |
| DPJP IGD belum punya akun | *Minta TTD* `409` aturan 12 dengan nama dokter |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/igd/contracts/api-contract.md` §12, `validation-matrix.md` §14, `state-transition-matrix.md`
§11, `permission-audit-matrix.md` §11, `erd/data-dictionary.md` §10, `02-backend-architecture.md` §17,
`roadmap/backend-roadmap.md` R3.18; source pola: `EmergencyDoctorAssignmentController.cs` (controller tipis + hasil
service), `EmergencyTriageController.cs:131` (pengenal bentrok unik), `EmergencyEpisodeRule.cs:228` (kunci *advisory*),
`PatientAllergyQueryService.cs`, `EncounterInsuranceService.cs:25`, `EmergencyDispositionService.cs:181`,
`EmgDoctorAssignment`, `MstDoctorSchedule`, `MstPatientClass`, `InpAdmissionReferral`, `ApplicationUser`,
`Migrations/20261011052215_AddEmergencyDoctorRosterOverride.cs` (pola `Down()` berpenjaga).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyAdmissionRequestStatus.cs` | **Baru** — `EmergencyAdmissionRequestStatus` (Draft 1, AwaitingSignature 2, Signed 3, Cancelled 4), `EmergencyAdmissionRequestDisplayStatus` (Draft 1, AwaitingSignature 2, Sent 3, Completed 4, Cancelled 5, CancelledByAdmission 6), `EmergencyAdmissionAllergyState` (NotRecorded 1, Recorded 2) |
| `…/Enums/EmergencyAdmissionRequestEventType.cs` | **Baru** — Opened 1, SignatureRequested 2, Returned 3, Signed 4, Cancelled 5, CancelledAfterSend 6, AutoCancelled 7 |
| `…/Models/EmgAdmissionRequest.cs` | **Baru** — kamus data §10.1; `RowVersion` (`Guid`) |
| `…/Models/EmgAdmissionRequestEvent.cs` | **Baru** — kamus data §10.2 |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgAdmissionRequestConfiguration.cs` | **Baru** — `CK_EmgAdmissionRequest_State`, `CK_EmgAdmissionRequest_Los` (1–90), index unik bersaring `UX_EmgAdmissionRequest_Visit_Open` (`"Status" IN (1, 2) AND NOT "IsDelete"`), index `DispositionId` dan `Status`, seluruh FK `Restrict`, `SnapshotJson` `jsonb` |
| `…/EmgAdmissionRequestEventConfiguration.cs` | **Baru** — index `IX_EmgAdmissionRequestEvent_Request_OccurredAt` |
| `…/DTOs/EmergencyAdmissionRequestDtos.cs` | **Baru** — request, response, ringkasan, opsi DPJP, salinan beku |
| `…/Services/EmergencyDispositionService.AdmissionRequest.cs` | **Baru** — berkas `partial`: buat, simpan draf, minta TTD, *Perlu TTD*, kembalikan, batal sebelum kirim, daftar per kunjungan, rincian, pilihan DPJP, `BacaRingkasanPermintaanRawatInapAsync` (static, untuk Rawat Inap di `BE-IGD-073`) |
| `…/Controllers/EmergencyAdmissionRequestController.cs` | **Baru** — sembilan endpoint; nol akses `DbContext` |
| `…/Services/EmergencyDispositionService.cs` | `partial`; dua dependency konstruktor; aturan "diagnosis yang dihitung" dipindah ke `KueriDiagnosisTerhitung` dan dipakai bersama `ValidateDiagnosisBeforeConfirmAsync` (perilaku tidak berubah — `Include` pada `AnyAsync` tidak berpengaruh) |
| `…/Controllers/EmergencyDispositionController.cs` | `ToResponse` mengisi `RequiresAdmissionRequest` |
| `…/DTOs/EmergencyDispositionDtos.cs` | `EmergencyDispositionResponse.RequiresAdmissionRequest` |
| `…/MasterData/Models/EmgDispositionType.cs` | Kolom `RequiresAdmissionRequest` (`bool`, bawaan `false`) |
| `…/MasterData/DTOs/EmergencyDispositionTypeDtos.cs` | Ruas yang sama pada response dan create (update mewarisi create) |
| `…/MasterData/Controllers/EmergencyDispositionTypeController.cs` | Dipetakan pada create, update, dan response |
| `…/MasterData/Seeders/EmergencyMasterDataSeeder.cs` | Definisi `RANAP` diberi `true` untuk lingkungan baru; baris yang sudah ada tidak disentuh seeder |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` (BOM berkas lama dipertahankan) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sembilan endpoint baru (aditif). `RequiresAdmissionRequest` bertambah pada respons jenis tindak lanjut dan tindak lanjut (aditif). Delta terhadap API §12 di bagian 3.4 |
| Database | **Migration 1 `AddEmergencyAdmissionRequest` belum dibuat** — wewenang Rizki. Selama belum diterapkan, model EF lebih maju daripada skema: endpoint form dan seluruh pembacaan `EmgDispositionType` / tindak lanjut akan `500` `42703 column "RequiresAdmissionRequest" does not exist` |
| Keamanan/Auth | Butir hak akses baru `EmergencyAdmissionRequest`: `Read`, `Create`, `Update`, `Sign`, `Cancel` (permission §11.1). Pemberian ke peran dilakukan pengurus Akses Role (`IGD-DEC-291`), bukan agent. Kewenangan data: *Kembalikan* dan isi *Perlu TTD* dibandingkan `ApplicationUser.DoctorId` dengan DPJP IGD aktif — bukan nama peran. Log kustom hanya `EntityId`, `EmergencyVisitId`, status, controller, action; nol isian klinis |

### 3.4 Delta terhadap kontrak yang disetujui

| No | Kontrak | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | `PATCH /{id}/request-signature`, `/return`, `/cancel` | `POST` | **`IGD-DEC-300`** — standar endpoint transaksi: perpindahan status `POST /{id}/<aksi>`. API dikoreksi ke `0.17.1`; `sign` dan `admission-referrals/{id}/cancel` ikut `POST` |
| 2 | Contoh §12.3 menulis `"status": "AwaitingSignature"` | Nilai enum dikirim sebagai **angka** (`"status": 2`) | Konvensi seluruh backend: tidak ada `JsonStringEnumConverter`; kontrak IGD lain menulis angka (`"visitStatus": 2`, API §1). Tabel nilai di bawah |
| 3 | `specialNeeds` daftar kode | `specialNeeds` (`IntensiveCare`, `Isolation`, `OxygenSupport`, `SpecialMonitoring`) **dan** empat boolean `needs*` | Aditif — empat boolean mencerminkan request supaya form dapat diisi ulang tanpa menerjemahkan |
| 4 | `admissionReferral` "setelah *Signed*" | Belum ada | Lahir di `BE-IGD-074`; form *Signed* belum mungkin ada sebelum itu |
| 5 | Aturan 12: "DPJP IGD (dr. {nama}) belum punya akun…" | "DPJP IGD ({nama}) belum punya akun…" dengan nama lengkap dokter apa adanya | Nama di master dokter sudah bergelar (contoh data: "dr. Andi Prasetya, Sp.B"); awalan tetap akan menghasilkan "dr. dr." — bunyi akhirnya sama dengan maksud kontrak |
| 6 | Aturan 10 hanya untuk dokter yang tidak berjadwal | Tambahan `400` *"Dokter usulan tidak ditemukan atau tidak aktif."* bila ID bukan dokter aktif | Permukaan teknis; tanpa ini ID asal-asalan tersimpan |
| 7 | Aturan 3 menghitung juga form terkirim dengan permintaan `Pending` | `BE-IGD-072` menghitung *Draf* dan *Menunggu TTD* | Form terkirim baru ada di `BE-IGD-074`; bagian itu ditambahkan di sana |
| 8 | Aturan 19: batal sesudah kirim selama `Pending` | Form *Signed* → `409` *"Form ini sudah ditandatangani. Pembatalan form yang sudah terkirim belum tersedia."* | Sementara sampai `BE-IGD-074` |
| 9 | `displayStatus` `Sent`/`Completed`/`CancelledByAdmission` dibaca dari permintaan admisi | *Signed* → `Sent` | Sementara sampai `BE-IGD-074` membaca `InpAdmissionReferral` |
| 10 | Nama DTO `RowVersionRequest` | `EmergencyAdmissionRequestRowVersionRequest` | Menghindari nama generik di namespace bersama; bentuk body sama: `{ "rowVersion": "…" }` |
| 11 | Ruas tambahan | `emergencyVisitNumber`, `encounterId`, `encounterNumber`, `signatureRequestedAt`, `cancelledAt`, `cancellationReason`, `createDateTime`; `patient.patientId`, `birthDate`; `payer.errorMessage`; `events[].fromStatus`, `toStatus`, `actorUserId` | Aditif — kebutuhan layar riwayat dan pesan penjamin tidak valid |

**Tabel nilai enum pada respons:**

| Ruas | Nilai |
| --- | --- |
| `status` | 1 Draf · 2 Menunggu TTD · 3 Ditandatangani · 4 Dibatalkan |
| `displayStatus` | 1 Draf · 2 Menunggu TTD · 3 Terkirim · 4 Selesai · 5 Dibatalkan · 6 Dibatalkan admisi |
| `allergies.state` | 1 Belum ada data alergi · 2 Tercatat |
| `events[].eventType` | 1 Dibuat · 2 Minta TTD · 3 Dikembalikan · 4 Ditandatangani · 5 Dibatalkan · 6 Dibatalkan sesudah kirim · 7 Batal otomatis |
| `diagnoses[].type`, `allergies.items[].category`, `severity` | Enum Clinical Management yang sudah ada, angka |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Admission Request

Base URL: `/api/v1/health-services/emergency-installation-management/emergency-admission-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?emergencyVisitId={id}` | Seluruh form satu kunjungan, terbaru dulu | `EmergencyAdmissionRequest : Read` |
| `GET` | `/{id}` | Rincian; *Draf*/*Menunggu TTD* membaca data pasien, alergi, penjamin, dan diagnosis terkini | `EmergencyAdmissionRequest : Read` |
| `POST` | `/` | Membuat draf dari tindak lanjut rawat inap terkonfirmasi → `201` | `EmergencyAdmissionRequest : Create` |
| `PUT` | `/{id}` | Menyimpan isian draf | `EmergencyAdmissionRequest : Update` |
| `POST` | `/{id}/request-signature` | *Draf* → *Menunggu TTD* | `EmergencyAdmissionRequest : Update` |
| `GET` | `/pending-signature` | Daftar *Perlu TTD* akun dokter yang masuk | `EmergencyAdmissionRequest : Sign` |
| `POST` | `/{id}/return` | *Menunggu TTD* → *Draf* dengan catatan | `EmergencyAdmissionRequest : Sign` |
| `POST` | `/{id}/cancel` | Membatalkan beralasan sebelum ditandatangani | `EmergencyAdmissionRequest : Cancel` |
| `GET` | `/inpatient-doctor-options?serviceUnitId=&search=` | Pilihan DPJP rawat inap usulan, maksimal 50 | `EmergencyAdmissionRequest : Read` |

`[AccessAction]` dan `[AccessPermission]` pada setiap method sama persis (`ControllerName = "EmergencyAdmissionRequest"`);
`Sign` dan `Cancel` memakai `AccessType = Update`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `-Mode Strict` atas 17 berkas (9 baru, 8 lama) | `VIOLATION 0`, `REVIEW 0`, `INFO 0`, *Final result: PASS* | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path … -Mode Strict`, 12 Oktober 2026 |
| Akhiran baris (hitungan byte Node) | 9 berkas baru CRLF tanpa BOM, nol LF tunggal; 8 berkas lama tetap CRLF; `ApplicationDbContext.cs` tetap ber-BOM seperti sebelumnya | `PASS` | Skrip hitung byte; nol berkas campur |
| Komentar baru | Nol `//`, `///`, `/* */` pada berkas baru dan pada baris tambahan `git diff -U0`; komentar lama tidak disunting | `PASS` | `grep` atas berkas baru dan diff |
| `Program.cs` | Tidak berubah | `PASS` | `git status --short` |
| Diagnostik VS Code untuk berkas service baru | Daftar kosong | `NOT RUN` | Server bahasa C# belum menganalisis berkas yang tidak terbuka — **bukan** bukti bebas galat |
| Build | Dilakukan Rizki; `bin/Debug/net9.0/QuilvianSystemBackend.dll` 12 Oktober 2026 15.02.45; jumlah warning tidak dilaporkan | `PASS` (pernyataan pemilik) | Pernyataan Rizki 12 Oktober 2026 + cap waktu DLL |
| Migration `20261012080102_AddEmergencyAdmissionRequest` | Dibuat dan diterapkan Rizki. Isi skema sesuai harapan: 1 `AddColumn` (`RequiresAdmissionRequest`, bawaan `false`), 2 `CreateTable`, 2 check constraint, index unik bersaring, nol operasi modul lain. **`migrationBuilder.Sql` = 0, seharusnya 2**: `UPDATE` `RANAP` tidak ada di `Up()`, penjaga tidak ada di `Down()` | `NEW ERROR` | Pembacaan berkas migration (tanpa kueri basis data) |
| Putaran A (A-01…A-14) lewat layar `FE-IGD-059` dan API, 16.12–16.43 | 12 terbukti, 1 sebagian (A-01), 1 tidak terbukti (A-11 — `PUT` tab 2 diputus skrip, jalur `409` tidak tercapai); nol cacat backend ditemukan | `PASS` sebagian | [penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026) |

Uji manual: `PASS` sebagian — A-11 dan keadaan awal A-01 diuji ulang (laporan uji 7.4).

**Tidak dijalankan:** build dan migration (wewenang Rizki); kueri basis data apa pun (batasan tetap); unit test (keputusan
tim).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Form hanya lahir dari tindak lanjut berjenis `RequiresAdmissionRequest` yang *Confirmed*, pada kunjungan berpasien dan berencounter; satu form aktif per kunjungan; ditolak bila pasien punya permintaan `Pending` lain (aturan 1–4) | **Sebagian** | Aturan 3 terbukti (`A-03.json` `409`); pembuatan dari tindak lanjut terkonfirmasi terbukti (log `201` 16.12.01 dan 16.41.09). Aturan 1, 2, 4 tidak diuji — tidak ada data tindak lanjut *Draf*, kunjungan tanpa encounter, atau permintaan `Pending` lain |
| 2 | Draf terisi unit, indikasi, dan instruksi dari tindak lanjut; simpan draf menjalankan aturan 5–10 dengan pesan persis | **Sebagian** | Isian awal terbukti (`A-13.png`); aturan 6 terbukti (`A-05.png`, log `PUT 400`). Aturan 5, 7–10 tidak diuji lewat API — layar hanya menawarkan pilihan yang sah |
| 3 | Pilihan DPJP rawat inap: hanya dokter berjadwal di unit bila unit berjadwal; semua dokter aktif bila tidak, dengan `isScheduledInUnit` | Terpenuhi | `A-04-1.png`, `A-04-2.png`. Jadwal dokter prasyarat dibuat lewat `superadmin` 15.54 (laporan uji 7.2 nomor 3) |
| 4 | *Minta TTD* menjalankan aturan 11–12; *Perlu TTD* dihitung dari DPJP aktif saat ini; *Kembalikan* hanya oleh DPJP aktif dengan catatan (aturan 13, 14, 17) | Terpenuhi | Aturan 11 `A-06.png`; aturan 12 dari sisi layar `A-13.png`; *Perlu TTD* `A-08.json`; aturan 13 `A-09.json` `403`; *Kembalikan* `A-10.json`. Aturan 14 dan 17 tidak diuji |
| 5 | Rincian menampilkan identitas, alergi (`NotRecorded`/`Recorded`), penjamin dengan teks hak kelas, diagnosis utama paling atas, `activeIgdDoctor`, `canSign`, `events` | Terpenuhi | `A-10.json` (seluruh ruas); hak kelas tidak teruji karena pasien tunai |
| 6 | Batal sebelum kirim beralasan; setiap perpindahan keadaan tercatat di `EmgAdmissionRequestEvent` | Terpenuhi | `A-12.png`: empat kejadian (dibuat, minta TTD, dikembalikan, dibatalkan) beserta catatan dan alasan |
| 7 | `RequiresAdmissionRequest` terbaca dan dapat diubah lewat API master; ikut di respons tindak lanjut; migration mengisi `RANAP` | **Sebagian** | Diubah lewat `PUT` master 15.39.57 (oleh `superadmin` — pelaku menunggu konfirmasi pemilik) dan ikut di respons (`A-14.json`). Migration **tidak** mengisi: `UPDATE` tidak tertempel, dan kode jenis rawat inap di lingkungan ini `RI`, bukan `RANAP` (`IGD-UNK-24`) |
| 8 | Migration 1 diterapkan pemilik; `Down()` berpenjaga diuji atau dikecualikan pemilik | **Sebagian** | Migration diterapkan (pernyataan Rizki). Penjaga `Down()` ditempel pemilik sendiri — belum ada di berkas saat diperiksa |
| 9 | Diff hanya berkas Cakupan; nol `Program.cs`; nol komentar baru; akhiran baris benar | Terpenuhi | Bagian 5 |
| 10 | Build 0 error; jumlah warning dilaporkan | Sebagian | Build berhasil (pernyataan Rizki, DLL 15.02.45) |

DoD: laporan ini ✅; acceptance 3–6 dan 9 terpenuhi; 1, 2, 7, 8, 10 sebagian. Penilaian putaran A: [penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026). Koreksi 12 Oktober 2026: status *🟢 SELESAI* yang ditulis agen uji dikembalikan ke 🟡 — tidak didukung bukti.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Selama migration belum diterapkan, **jangan menjalankan backend hasil build ini terhadap basis data dev**: kolom `EmgDispositionType.RequiresAdmissionRequest` dibaca oleh layar tindak lanjut IGD yang sudah dipakai, sehingga layar itu ikut `500` |
| Masalah yang diketahui | Delta 4, 7, 8, 9 pada bagian 3.4 bersifat sementara dan ditutup `BE-IGD-074` |
| Risiko tersisa | Data pasien terkini dibaca dari empat modul (alergi, penjamin, diagnosis, pasien) pada setiap `GET /{id}`; kegagalan modul lain tampil sebagai `500` (aturan 16 `503` baru berlaku saat TTD di `BE-IGD-074`). `IGD-OQ-127` (aturan tipe unit) tetap ditahan — unit cukup aktif |
| Perubahan sampingan | `NONE` |
| Interupsi | Pemadatan konteks percakapan di tengah pengerjaan; dilanjutkan dari keadaan `git status` dan berkas yang sudah ada, tanpa penyuntingan ganda |
| Status Git | Source: 8 `M` (bagian 3.2) dan 9 `??` berkas baru; dokumen blueprint IGD `M` dari amandemen 12 Oktober. Nol stage, nol commit |
| Langkah berikutnya | Rizki: migration dan build (di bawah). Agent: `FE-IGD-059` sesudah build hijau; `BE-IGD-073` gelombang 2 |

### Migration untuk Rizki — `AddEmergencyAdmissionRequest`

```bash
dotnet ef migrations add AddEmergencyAdmissionRequest
dotnet build -p:RunAnalyzers=false
```

**Isi yang diharapkan** — bila hasil `migrations add` memuat operasi lain (misalnya `CreateTable` tabel Billing atau
Operating Room karena snapshot), hentikan dan periksa snapshot lebih dulu:

| Operasi | Isi |
| --- | --- |
| `AddColumn` | `EmgDispositionType.RequiresAdmissionRequest` `boolean NOT NULL DEFAULT false` |
| `CreateTable` | `EmgAdmissionRequest` — 41 kolom (31 milik tabel + 10 `IdentityModel`), PK `Id`, check `CK_EmgAdmissionRequest_State` dan `CK_EmgAdmissionRequest_Los` |
| `CreateTable` | `EmgAdmissionRequestEvent` — 18 kolom (8 milik tabel + 10 `IdentityModel`), PK `Id` |
| FK `Restrict` | `EmgAdmissionRequest` → `EmgVisit`, `EmgDisposition`, `RegPatientEncounter`, `MstPatient`, `MstServiceUnit`, `MstPatientClass`, `MstDoctor` (dua kali: usulan dan penanda tangan); `EmgAdmissionRequestEvent` → `EmgAdmissionRequest` |
| Index | `UX_EmgAdmissionRequest_Visit_Open` (unik, saring `"Status" IN (1, 2) AND NOT "IsDelete"`), `IX_EmgAdmissionRequest_DispositionId`, `IX_EmgAdmissionRequest_Status`, index FK lain yang dibangkitkan EF, `IX_EmgAdmissionRequestEvent_Request_OccurredAt` |

**Tempel manual pada `Up()`**, sesudah `AddColumn` (EF hanya membangkitkan skema; rencana migration §17 nomor 1):

```csharp
migrationBuilder.Sql("""
    UPDATE public."EmgDispositionType"
    SET "RequiresAdmissionRequest" = true
    WHERE "Code" = 'RANAP';
    """);
```

**Tempel manual di awal `Down()`** (pola `AddEmergencyDoctorRosterOverride`):

```csharp
migrationBuilder.Sql("""
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."EmgAdmissionRequest") THEN
            RAISE EXCEPTION 'Down BE-IGD-072 dihentikan: EmgAdmissionRequest sudah berisi Form Permintaan Rawat Inap. Menghapus tabel ini menghilangkan dokumen klinis pasien.';
        END IF;
    END $$;
    """);
```

Sebelum `database update`, hitung `migrationBuilder.Sql` pada berkas hasil: harus **2**. Lingkungan yang kode jenis
tindak lanjut rawat inapnya bukan `RANAP` diatur admin lewat layar master (`IGD-UNK-24`).
