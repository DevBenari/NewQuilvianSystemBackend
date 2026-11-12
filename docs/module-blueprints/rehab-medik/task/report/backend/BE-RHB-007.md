# Laporan Perubahan Backend — `BE-RHB-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-007` |
| Judul | Permintaan dan disposisi |
| Slice | S-B — Permintaan dan episode (`MVP-1`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-007` |
| Trace | FR-RHB-001..003; FEAT-001..003; RHB-INV-02, 07; RHB-DEC-008, 010 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Request*; `state-transition-matrix.md` §1; V-REQ-01..09, V-GEN-01/02 |
| Dependency | `BE-RHB-002` ✅, `BE-RHB-006` ✅ |
| Klasifikasi | `HIGH` — aggregate ber-lifecycle, pembukaan episode, konkurensi, hak akses berlapis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Constants}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-05). Build `PASS`, QBE Strict `PASS`, uji API manual 63/63 sesuai harapan (§5), cek DB `PASS`, review log `PASS`. Uji 409 `RHB-VAL-072` yang diteruskan dari `BE-RHB-005` juga lolos. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru (tabel dari `BE-RHB-002`). |
| Jenis capability | **Transaksi — aggregate ber-lifecycle.** Perpindahan status lewat `POST /{id}/<aksi>`, batal lewat `cancel`; tidak ada `DELETE`, `options`, maupun `PATCH status`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001 (controller tanpa `ApplicationDbContext`), QBE-CODE-002/003/006 (nomor hanya dari `NumberSeriesAllocator`, tanpa Count/Max+1), QBE-API-001, standar hak akses (konstanta `RehabilitationPermissions.Request`) |
| Lintas modul | Baca saja: `MstPatient`, `RegPatientEncounter`, `InpEpisode`, `MstDoctor`, `CliDoctorCertificate`. Tulis hanya ke tabel `Rhb*` dan deret nomor `RHB_REQUEST`/`RHB_EPISODE` milik rehab. |

## 1. Masalah yang diperbaiki

Rehab belum punya pintu masuk. Tanpa permintaan, tidak ada episode, dan seluruh alur berikutnya
(asesmen, program, sesi) tidak dapat dimulai. Permintaan juga harus bisa dipilah: petugas admisi
mengembalikan berkas yang kurang, dokter Sp.KFR menerima, mengalihkan, atau menolak secara klinis.
Kedua kewenangan ini **tidak boleh** tertukar (RHB-INV-02).

**Contoh.** Admisi membuat permintaan fisioterapi `RHB-REQ-2026-000001` dari kunjungan Poli Saraf,
lalu mengajukannya. dr. Sp.KFR menerimanya, dan dalam simpanan yang sama terbuka episode
`RHB-EPS-2026-000001` berstatus *Menunggu asesmen medis*.

## 2. Proses bisnis

1. **Draf** (`POST /`): pasien, sumber, kunjungan, dokter peminta, indikasi, layanan. Isian yang
   sudah diisi harus sah. Nomor terbit dari deret `RHB_REQUEST`.
2. **Ubah** (`PUT /{id}`) selama `Draft`/`NeedsClarification`, dengan `expectedVersion`.
3. **Ajukan** (`submit`): kelengkapan V-REQ-01..06 ditagih. Bila pasien punya permintaan atau
   episode berjalan dengan layanan beririsan, jawabannya 409 `RHB-VAL-007` beserta daftar kandidat,
   lalu boleh dilanjutkan dengan alasan (V-REQ-07, UAT-02).
4. **Telaah:**
   - **Kembalikan** (`return`): butir `Return`, administratif, ke `NeedsClarification`.
   - **Minta klarifikasi** (`request-clarification`): butir `Disposition`, klinis, ke `NeedsClarification`.
   - **Terima** (`accept`): butir `Disposition`, ke `Accepted`, sekaligus membuka episode.
   - **Alihkan** (`redirect`) / **Tolak** (`reject`): butir `Disposition`, alasan wajib.
5. **Batal** (`cancel`) dari `Draft`/`Submitted`/`NeedsClarification`, alasan wajib.
6. Setiap perpindahan status dicatat di `RhbStatusHistory`: dari, ke, aksi, alasan, pelaku, waktu.

Jalur tidak normal: kunjungan pasien lain dijawab sama seperti kunjungan yang tidak ada (anti-IDOR).
Data yang sudah diubah orang lain → 409 `RHB-VAL-010`. Aksi dari status yang salah → 409 `RHB-VAL-020`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `HemodialysisManagement/Services/HmdOrderService.cs` — pola nomor, `Version++`, tangkapan konkurensi
- `Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs` — **mengalokasikan di transaksinya sendiri**; nomor yang batal dipakai hangus (`INV-PLT-002`)
- `RegistrationManagement/Services/EncounterReferralService.cs`, model `CliDoctorCertificate`
- Konfigurasi EF `RhbRequest` (`CK_RhbRequest_Source`, `Version` concurrency token), `RhbEpisode` (unik `RequestId`)
- `Services/Security/AccessPermissionService.cs` — akses non-SuperAdmin = kebijakan pasangan departemen + jabatan

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Services/RhbRequestService.cs` | Baru (1010 baris) | Metadata, ringkasan, worklist, rincian, riwayat, cek duplikasi, draf, ubah, dan 7 aksi status |
| `Services/RhbEpisodeService.cs` | Baru (69 baris) | `OpenForAcceptedRequest`: episode `AwaitingMedicalAssessment` + riwayat. **Tidak menyimpan**, karena ikut `SaveChanges` permintaan. Disiplin episode sengaja tidak disalin dari permintaan (RHB-DEC-005). |
| `Services/RhbStatusHistoryWriter.cs` | Baru (74 baris) | `Record` (ikut `SaveChanges` pemanggil) dan `ReadAsync` |
| `DTOs/RehabRequestDtos.cs` | Baru (206 baris) | Query, request, respons daftar/rincian/ringkasan/metadata, kandidat duplikasi, riwayat |
| `Controllers/RehabRequestController.cs` | Baru (243 baris) | 15 endpoint (§4) |
| `Services/RhbServiceSupport.cs` | Diperbarui | `ResolveRange` dipindahkan ke sini dari `RhbConfigurationService` (dipakai bersama); `BusinessTimeZone`, `ToUtc` |
| `Services/RhbConfigurationService.cs` | Diperbarui | Memanggil `RhbServiceSupport.ResolveRange`; tidak ada perubahan perilaku |
| `Constants/RhbErrorCodes.cs` | Diperbarui | **Delta:** `RHB-VAL-078` dokter peminta, `079` isian terlalu panjang, `080` nomor gagal terbit |
| `Program.cs` | Diperbarui | `AddScoped<RhbEpisodeService>()`, `AddScoped<RhbRequestService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru *Rehab Request*, 15 endpoint sesuai kontrak v1 |
| Database | Tidak ada perubahan schema maupun migration. Menulis `RhbRequest`, `RhbRequestDiscipline`, `RhbEpisode`, `RhbStatusHistory`, serta deret nomor `RHB_REQUEST`/`RHB_EPISODE`. |
| Keamanan/Auth | Resource `RehabRequest`, aksi `Read`, `Create`, `Update`, `Return`, `Disposition`, `Cancel`. Keenamnya terdaftar di `SysActionAccess` saat startup, jadi dapat dicentang di Akses Role. Tidak ada role yang di-hardcode. |
| Privasi | Daftar dan log tidak memuat kolom bertanda S (indikasi, nama fasilitas/perujuk, nomor rujukan eksternal, alasan). Rincian memuatnya untuk tampilan klinis. |

## 4. Dokumentasi endpoint

**[Tags("Health Services / Rehabilitation Management / Rehab Request")]** — base URL
`/api/v1/health-services/rehabilitation-management/rehab-requests`

| Method | Path | Hak akses | Kode status |
| --- | --- | --- | --- |
| GET | `/filters/metadata` | `RehabRequest : Read` | 200 |
| GET | `/summary` | `RehabRequest : Read` | 200, 400 (`076`) |
| GET | `/` | `RehabRequest : Read` | 200, 400 (`076`) |
| GET | `/duplicate-check?patientId=&disciplineIds=&excludeRequestId=` | `RehabRequest : Read` | 200, 400 (`001`) |
| GET | `/{id}` | `RehabRequest : Read` | 200, 404 |
| GET | `/{id}/status-history` | `RehabRequest : Read` | 200, 404 |
| POST | `/` | `RehabRequest : Create` | 201, 400 (`079`), 422 (`001`, `002`, `005`, `006`, `078`, `080`) |
| PUT | `/{id}` | `RehabRequest : Update` | 200, 404, 409 (`010`, `020`), 422 |
| POST | `/{id}/submit` | `RehabRequest : Update` | 200, 404, 409 (`007` + kandidat di `errors.details`, `010`, `020`), 422 (`001`–`006`, `078`) |
| POST | `/{id}/return` | `RehabRequest : Return` | 200, 404, 409, 422 (`008`) |
| POST | `/{id}/request-clarification` | `RehabRequest : Disposition` | 200, 404, 409, 422 (`008`) |
| POST | `/{id}/accept` | `RehabRequest : Disposition` | 200 (+ `episodeId`, `episodeNumber`), 404, 409 (`010`, `020`), 422 (`009`, `080`) |
| POST | `/{id}/redirect` | `RehabRequest : Disposition` | 200, 404, 409, 422 (`008`) |
| POST | `/{id}/reject` | `RehabRequest : Disposition` | 200, 404, 409, 422 (`008`) |
| POST | `/{id}/cancel` | `RehabRequest : Cancel` | 200, 404, 409, 422 (`008`) |

Contoh penolakan duplikasi (UAT-02):

```json
{
  "statusCode": 409,
  "message": "Pasien sudah memiliki permintaan/episode rehab yang masih berjalan. Periksa daftar di bawah, atau isi alasan untuk tetap melanjutkan.",
  "errors": {
    "code": "RHB-VAL-007",
    "fields": ["duplicateOverrideReason"],
    "details": [
      { "candidateType": "Episode", "number": "RHB-EPS-2026-000001", "status": 1,
        "statusName": "Menunggu asesmen medis", "disciplineNames": ["Uji BE-RHB-007"] }
    ]
  }
}
```

`availableActions` pada rincian mengikuti status: `Draft`/`NeedsClarification` →
`update, submit, cancel`; `Submitted` → `return, request-clarification, accept, redirect, reject,
cancel`; status akhir → kosong. Hak akses tetap diperiksa di setiap endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE conformance `-Mode Strict` (8 berkas) | 0 violation untuk setiap berkas | `PASS` | Dijalankan agent 2026-11-05 |
| `dotnet build` Debug, perintah owner | `0 Error(s)`, 426 warning, 0 warning Rehab, `EXIT=0`, 4 menit 49 detik (15:31:16–15:36:06) | `PASS` | Log `build-BE-RHB-007.log` |
| Pendaftaran hak akses | `SysActionAccess` `RehabRequest`: Read (Read), Create (Create), Update/Return/Disposition/Cancel (Update) | `PASS` | Query DB sesudah startup |
| Uji API bagian 1 (SuperAdmin) | 42/50. 8 `FAIL` disebabkan cara uji, bukan kode: (a) uji balapan memakai dua login akun yang sama, sehingga login kedua mematikan sesi pertama (401); (b) akun uji ditolak geofence login (403) karena koordinat belum dikirim. Keduanya diulang di bagian 2. | — | Tabel di bawah |
| Uji API bagian 2 | 13/13 | `PASS` | Tabel di bawah |
| **Total sesuai harapan** | **63/63** — 42 bagian 1 + 13 bagian 2 + 8 yang diulang (1 balapan + 7 hak akses) | `PASS` | |
| Cek DB (read-only) | Sesuai | `PASS` | Rincian di bawah |
| Review log (konsol + `Logs/quilvian-backend-20261105.json`) | 28 baris `RehabRequest.*`; 0 kemunculan penanda indikasi uji, "BPJS", "Kontraindikasi", "hasil MRI", "KTP buram", maupun kata "nyeri" | `PASS` | Kriteria (7) |

Hasil uji API (2026-11-05 15:47–15:55):

| Pemeriksaan | Hasil |
| --- | --- |
| Draf: pasien tidak terdaftar | 422 `RHB-VAL-001` / `["patientId"]` |
| Draf: sumber internal tanpa kunjungan | 422 `RHB-VAL-002` / `["sourceEncounterId"]` |
| Draf: kunjungan milik pasien B untuk pasien A | 422 `RHB-VAL-002` (anti-IDOR) |
| Draf: sumber rawat inap + kunjungan rawat jalan | 422 `RHB-VAL-002` / `["sourceType","sourceEncounterId"]` |
| Draf: waktu permintaan besok | 422 `RHB-VAL-006` |
| Draf: layanan tidak aktif (fisioterapi seed) | 422 `RHB-VAL-005` |
| Draf: dokter peminta tidak ada | 422 `RHB-VAL-078` |
| Draf: indikasi 2001 karakter | 400 `RHB-VAL-079` |
| **UAT-01** draf → `RHB-REQ-2026-000001` (Draft, v0) → `PUT` (v1) → `submit` | 201 → 200 → 200 `Submitted` |
| (6) `PUT` dengan `expectedVersion` 99 | 409 `RHB-VAL-010` |
| `submit` ulang dari `Submitted` | 409 `RHB-VAL-020` |
| `submit` dengan indikasi "abc" | 422 `RHB-VAL-004` |
| (4) `accept` dari `Draft` | 409 `RHB-VAL-020` |
| Rujukan eksternal: draf (kunjungan dikosongkan) → `submit` tanpa fasilitas/perujuk | 201 → 422 `RHB-VAL-003` |
| (6) `accept` versi basi | 409 `RHB-VAL-010` |
| `accept` tanpa dokter penanggung jawab | 422 `RHB-VAL-009` |
| `accept` | 200 `Accepted`, episode `RHB-EPS-2026-000001` |
| `cancel` dari `Accepted` | 409 `RHB-VAL-020` |
| `duplicate-check` pasien A + layanan sama | Kandidat: episode `RHB-EPS-2026-000001` |
| `duplicate-check` tanpa pasien | 400 |
| **UAT-02** `submit` tanpa alasan → dengan alasan | 409 `RHB-VAL-007` + 1 kandidat episode → 200 `Submitted` |
| `return` tanpa alasan → dengan alasan → `submit` ulang → `request-clarification` | 422 `RHB-VAL-008` → 200 `NeedsClarification` → 200 `Submitted` → 200 `NeedsClarification` |
| `redirect` tanpa tujuan | 422 `RHB-VAL-008` / `["redirectTargetText"]` |
| `reject` → lalu `accept` | 200 `Rejected` → 409 `RHB-VAL-020` |
| `redirect` dengan tujuan; `cancel` draf | 200 `Redirected`; 200 `Cancelled` |
| `status-history` R1 | `create, submit, accept` |
| Rincian R1; rincian id acak | 200 (episode, 1 layanan, `availableActions` kosong); 404 |
| Daftar cari nomor | 1 baris, **tanpa** `indicationText` atau kolom S lain |
| Daftar filter status + layanan; ringkasan; metadata | 200; ringkasan `total 6, draft 1, accepted 2, redirected 1, rejected 1, cancelled 1`; 7 opsi status |
| **BE-RHB-005 (3)** `DELETE` layanan yang dipakai permintaan | 409 `RHB-VAL-072` |
| **(5)** dua `accept` bersamaan pada permintaan yang sama (satu sesi, dimulai serentak) | **200 + 409** |
| **UAT-03** akun uji dengan `RehabRequest : Read` + `Return` saja: `GET` rincian, `GET` daftar | 200, 200 |
| Akun uji: `reject`, `accept`, `request-clarification`, `redirect` (butir `Disposition`) | **403 ×4** ("Anda tidak memiliki akses ke menu atau fitur ini.") |
| Akun uji: `cancel` (tanpa `Cancel`), `POST /` (tanpa `Create`), `rehab-lookups` (tanpa `RehabLookup`) | 403 ×3 |
| Akun uji: `return` | 200 `NeedsClarification`; riwayat mencatat `return` dengan pelaku akun uji |
| Akun uji sesudah dinonaktifkan | 403 |

Cek DB (read-only):

| Pemeriksaan | Hasil |
| --- | --- |
| Episode per permintaan `Accepted` | Tepat 1 untuk ketiga permintaan `Accepted` (`RHB-REQ-2026-000001`, `000004`, `000008`) |
| Episode yang permintaannya bukan `Accepted` | **0** |
| Permintaan `Accepted` tanpa episode | **0** |
| Nomor episode yang terbit | `RHB-EPS-2026-000001`, `000002`, `000004`. **`000003` hangus**: pihak yang kalah dalam uji balapan sudah lolos pemeriksaan status dan menerbitkan nomor 3, lalu simpanannya ditolak indeks unik `RequestId` (dijawab 409 `RHB-VAL-020`) dan dibatalkan utuh. Inilah bukti kriteria (5): penerimaan yang gagal di tengah tidak meninggalkan episode, dan nomor yang hangus memang disengaja (`INV-PLT-002`). |
| Riwayat | Jumlah baris riwayat = jumlah perpindahan status per permintaan (contoh `000005`: 7 baris untuk create → submit → return → submit → clarification → submit → reject); setiap episode punya 1 baris `open` |
| Disiplin episode | 0, karena diisi resep asesmen medis (`BE-RHB-009`), bukan disalin dari permintaan |
| `DuplicateOverrideReason` | Hanya terisi pada permintaan yang memang punya kandidat duplikasi |
| Permintaan `Cancelled` | `IsCancel=true` beserta `CancelBy`/`CancelDateTime` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-01 | Terpenuhi (runtime) | Draf → ajukan → `Submitted` bernomor `RHB-REQ-2026-000001` |
| (2) UAT-02 (409 + kandidat, lanjut dengan alasan) | Terpenuhi (runtime) | 409 `RHB-VAL-007` dengan kandidat episode; dengan alasan → `Submitted` |
| (3) UAT-03 (`Return` ≠ `Disposition`, 403) | Terpenuhi (runtime) | Akun uji `Read`+`Return`: 4 aksi disposisi → 403, `return` → 200 |
| (4) `accept` dari status selain `Submitted` → 409 | Terpenuhi (runtime) | Dari `Draft` dan `Rejected` → 409 `RHB-VAL-020` |
| (5) Penerimaan gagal di tengah tidak meninggalkan episode yatim | Terpenuhi (runtime + DB) | Uji balapan: 200 + 409, nomor 3 hangus, 0 episode yatim; status, episode, dan riwayat disimpan dalam satu `SaveChanges` |
| (6) `expectedVersion` basi → 409 | Terpenuhi (runtime) | `PUT` dan `accept` → 409 `RHB-VAL-010` |
| (7) Log tanpa indikasi | Terpenuhi | Review log §5 |
| Verifikasi kartu: API + DB manual; review log | Terpenuhi | §5 |
| DoD: build owner, laporan | Terpenuhi | §5, berkas ini |
| Diteruskan dari `BE-RHB-005`: 409 `RHB-VAL-072` | Terpenuhi (runtime) | §5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak | (a) Kode teknis `RHB-VAL-078` (dokter peminta tidak ada/tidak aktif, atau kosong untuk sumber internal saat `submit`), `079` (isian melebihi batas kolom), `080` (nomor gagal terbit). (b) Sumber internal wajib berkunjungan **sejak draf**, bukan baru saat `submit`, karena `CK_RhbRequest_Source` menolak baris tanpa kunjungan. (c) Jenis kunjungan harus sesuai sumber: rawat jalan ↔ RJ/MCU/telemedisin, rawat inap ↔ RWI, gawat darurat ↔ IGD. (d) Untuk sumber rawat inap, `inpEpisodeId` diturunkan server dari kunjungan; kiriman klien yang berbeda ditolak. (e) Untuk rujukan eksternal, kunjungan/perawatan/surat internal dari klien diabaikan. (f) Toleransi jam klien 2 menit pada V-REQ-06. (g) `note` saat `accept` disimpan di `DispositionReason`. (h) `ResolveRange` dipindah ke `RhbServiceSupport` (refactor internal). |
| Kode bentrok balapan | Pihak yang kalah dijawab `RHB-VAL-020` ("sudah diproses pengguna lain"), bukan `010`, karena yang tertabrak lebih dulu adalah indeks unik episode. Keduanya 409, dan pesannya meminta muat ulang. |
| Peringatan | Payload `Data` logger aplikasi (nomor permintaan, status) **tidak** muncul di konsol maupun berkas log; yang tercatat hanya aksi, path, dan pelaku. Ini perilaku `LoggerService` yang sudah ada, bukan perubahan task ini, dan tidak melanggar kriteria (7). |
| Data uji di `QuilvianNewDevSukma` | 9 permintaan `RHB-REQ-2026-000001`..`009`, 3 episode, riwayatnya, dan layanan `RHB_UJI_10722EA2` (dinonaktifkan; tidak dapat dihapus karena dipakai). **Dua akun uji** `rhb-uji-5f999cd4@example.test` dan `rhb-uji-2ebcc0e9@example.test` beserta data pengguna eksternalnya **sudah dinonaktifkan**; password awalnya tidak pernah dicetak atau disimpan. Kebijakan Akses Role *Pendaftaran / Admission Officer* yang dipakai selama uji **dikembalikan ke kosong**, seperti sebelum uji (0 kebijakan aktif). |
| Perubahan sampingan | `NONE` pada source. Backend sudah dimatikan; port 7184 kosong. |
| Interupsi | Satu kali sesi dimulai ulang sebelum implementasi. Dilanjutkan dari kondisi Git, karena belum ada berkas task ini. |
| Status Git | `M Program.cs`, `M Constants/RhbErrorCodes.cs`, `M Services/RhbServiceSupport.cs`, `M Services/RhbConfigurationService.cs`, `??` 5 berkas baru di `Areas/HealthServices/RehabilitationManagement/` |
| Langkah berikutnya | `BE-RHB-008` (worklist dan ruang kerja episode) kini 🟡. |
