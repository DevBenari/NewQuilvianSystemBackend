# Laporan Perubahan Backend — `BE-PBJ-VD-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-007` |
| Judul | Usulan blokir dan kunci Master Data |
| Slice | `MVP-V2` — status vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-007` |
| Trace | `PBJ-DEC-021`, `054`, `060`; `PBJ-ASM-003`; FR-VD-031, 032; `PBJ-VAL-090`, `091`, `093`, `104`, `106`, `107`; `AT-VD-22`..`26`, `28`, `29` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Status Request + §Supplier, `PBJ-VD-STATE` 1.0 bagian 3, `PBJ-VD-INT` 1.0 §1, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-003` ✅ |
| Klasifikasi | `HIGH` (logika 3, kontrak API 2, integrasi `Wfl` + modul Administrator 3, keamanan 3 — satu penulis status blokir) |
| Task mode | `BACKEND` (pemilik, 18 November 2026: "saya berikan izin dan lanjutkan" atas dua izin terpisah: ubah `SupplierController` dan aktifkan sementara approval uji `PBJ-VND-*` di `QuilvianNewDevSukma`) |
| Target tulis | `Vendor/**` (controller, service, DTO), `Administrator/MasterData/Services/SupplierProcurementStatusService.cs` (baru), `SupplierController.cs`, `Program.cs`, `ProcurementApprovalTimelineService.cs`, konstanta status/riwayat, extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 18 November 2026 |
| Status | ✅ **SELESAI.** Sebelas endpoint usulan, penerap status `Wfl` `PBJ_VENDOR_STATUS`, satu penulis status blokir, Master Data terkunci; uji runtime 28/28 `PASS` dengan approval uji yang dicabut sesudahnya; tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor`; `Administrator` / `MasterData` (penulis status blokir, `SupplierController`) |
| Registry | `ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`); `Master / Reference / MasterData · Mst · ACTIVE` |
| Keberlakuan | `NEW CODE` untuk berkas `Vendor/**` dan `SupplierProcurementStatusService`; `TOUCHED LEGACY` untuk `SupplierController.cs` (create/update/validasi), `Program.cs` (DI), `ProcurementApprovalTimelineService.cs`, konstanta, extension DI |
| Arketipe endpoint | Aggregate ber-lifecycle dengan approval (`DRAFT → PENDING_APPROVAL → APPROVED/REJECTED/RETURNED/CANCELLED`); aksi `POST /{id}/<aksi>`; tanpa `DELETE` |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-CODE-002..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (10 berkas, 0 temuan) |
| Temuan kode lama | `SupplierController.cs` baris 1214 (`GetActorNameMapAsync`) memberi warning `CS8619` yang sudah ada sebelum task ini; tidak disentuh |

## 1. Masalah yang diselesaikan

Sebelumnya status blokir vendor bisa diubah langsung dari Master Data Supplier dengan satu centang, tanpa persetujuan
dan tanpa jejak di Pengadaan. Setelah task ini:
- blokir dan buka blokir hanya terjadi lewat **usulan Pengadaan** yang disetujui di mesin approval bersama (`Wfl`);
- Master Data Supplier tidak lagi bisa mengubah status blokir (`PBJ-DEC-060`);
- Finance tetap membaca status yang sama dari `MstSupplier.IsBlacklisted` (`PBJ-DEC-021`).

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Membuat usulan blokir | Manajer Pengadaan | `POST /vendor-status-requests` | "Tiga NCR kritis dalam enam bulan terakhir." + bukti penilaian (opsional) → `DRAFT` |
| Mengajukan | Pengusul | `POST …/{id}/submit` | Nomor `UBV-000002`, instance `Wfl` `PBJ-VND-BLOKIR` atas nama pengusul → `PENDING_APPROVAL` |
| Memutus | Direktur (penyetuju `Wfl`) | Kotak masuk `Wfl` (`approve` / `reject` / `request-revision`) | Disetujui → vendor diblokir; ditolak → tidak berubah; dikembalikan → pengusul memperbaiki |
| Memperbaiki dan mengajukan ulang | Pengusul | `PUT …/{id}`, `POST …/{id}/submit` | Instance baru, nomor tetap, `submissionCount` 2 |
| Menarik | Pengusul | `POST …/{id}/cancel` saat `PENDING_APPROVAL` | Instance `Wfl` → `Withdrawn`, usulan `CANCELLED` |
| Membatalkan draf | Pemegang `Cancel` | `POST …/{id}/cancel` saat `DRAFT`/`RETURNED` | `CANCELLED` |
| Membuka blokir | Manajer → Direktur | Usulan `UNBLOCK` | Disetujui → `IsBlacklisted = false`, alasan dikosongkan |

**Penerapan keputusan.** Saat `Wfl` menyetujui, penerap status `PBJ_VENDOR_STATUS` memanggil
`SupplierProcurementStatusService.ApplyAsync`. Status usulan, status blokir supplier, dan riwayat vendor tersimpan dalam
**satu transaksi**. Vendor yang diblokir juga otomatis bukan lagi supplier pilihan, sama dengan aturan Master Data
sebelumnya. Rekomendasi penilaian tidak memicu usulan apa pun (`PBJ-DEC-048`).

**Jalur gagal:**

| Kejadian | Respons |
| --- | --- |
| `BLOCK` untuk vendor sudah diblokir / `UNBLOCK` untuk vendor tidak diblokir | 409 `PBJ-VAL-106` |
| Masih ada usulan terbuka untuk vendor itu | 409 `PBJ-VAL-107` "Masih ada usulan … yang belum selesai untuk vendor ini." |
| Definisi `PBJ-VND-*` belum aktif (SOP belum mengisi penyetuju) | 409 `PBJ-VAL-091`; usulan tetap `DRAFT` |
| Pengusul mencoba menyetujui sendiri | Ditolak `Wfl`; usulan tetap `PENDING_APPROVAL` |
| Ubah usulan yang sedang diproses | 409 `PBJ-VAL-093` |
| Tarik/batal dengan alasan < 10 karakter | 422 `PBJ-VAL-104` |
| Master Data mengirim `isBlacklisted` | Diabaikan; nilai tersimpan tetap |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Controllers/VendorStatusRequestsController.cs` | Baru | 11 endpoint, resource `ProcurementVendorStatusRequest` (`Read`, `Create`, `Cancel`) |
| `.../Vendor/Services/ProcurementVendorStatusService.cs` | Baru | Buat, ubah, ajukan, batal/tarik, sinkron, daftar, ringkasan, detail, riwayat; `ApplyWorkflowStatusAsync`; `VendorStatusRequestAccessProvider`; `VendorStatusRequestWorkflowStatusApplier` |
| `.../Vendor/DTOs/VendorStatusDtos.cs` | Baru | Query, permintaan, respons, ringkasan, metadata |
| `Areas/Administrator/MasterData/Services/SupplierProcurementStatusService.cs` | Baru | Satu-satunya penulis `IsBlacklisted`/`BlacklistReason`; ikut transaksi pemanggil |
| `Areas/Administrator/MasterData/Controllers/SupplierController.cs` | Diperbarui | Create: selalu tidak diblokir. Update: blokir tidak diubah, `IsPreferredSupplier` dipaksa `false` bila sedang diblokir. Dua validasi blokir dihapus karena field-nya diabaikan |
| `Program.cs` | Diperbarui | `AddScoped<SupplierProcurementStatusService>` |
| `.../Common/Services/ProcurementApprovalTimelineService.cs` | Diperbarui | Linimasa untuk `PBJ_VENDOR_STATUS` |
| `.../Common/Constants/ProcurementStatuses.cs` | Diperbarui | `VendorStatusRequest` (6 status + himpunan `Open`) |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `STATUS_REQUEST_SUBMIT`, `STATUS_REQUEST_REJECT`, `VENDOR_BLOCK`, `VENDOR_UNBLOCK` |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Service usulan, penyedia akses, penerap status `Wfl` |

### 3.2 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Penulisan ke:
- `PrcVendorStatusRequest` dan `PrcDocumentHistory`;
- `PrcVendorProfile`, bila profil belum ada;
- `MstSupplier`, hanya lewat `SupplierProcurementStatusService`;
- `NumNumberSeries`, karena deret `UBV` dibuat allocator pada pengajuan pertama;
- tabel `Wfl`, lewat gateway.

**Keamanan.**
- Hanya keputusan approval yang mengubah status blokir.
- Penangan lifecycle `Wfl` yang sudah ada memeriksa ulang bahwa pengusul tidak menyetujui sendiri.
- Ubah, ajukan, dan tarik hanya oleh pengusul (kepemilikan data, bukan peran).

**Perilaku endpoint lama yang berubah** (sesuai kontrak §Supplier):
- `POST`/`PUT api/v1/administrator/master-data/suppliers` mengabaikan `isBlacklisted`/`blacklistReason`;
- respons tetap memuatnya sebagai data baca saja.

Layar Master Data Supplier masih menampilkan centang Blacklist sampai `FE-PBJ-VD-004` menjadikannya baca saja. Roadmap
mewajibkan keduanya dirilis bersamaan.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | API tidak memuat sinkron ulang; INT §1 menyebut "sinkron ulang lewat endpoint `synchronize`" | `POST /{id}/workflow/synchronize` (izin `Create`), pola `PurchaseRequestVerificationsController` | Jalur pemulihan bila penerapan keputusan sempat gagal |
| D-2 | `PUT`/`submit` dengan izin `Create` | Hanya **pengusul** yang boleh mengubah, mengajukan, dan menarik; batal draf boleh pemegang `Cancel` mana pun | Pola PR. `Wfl` hanya mengizinkan tarik oleh pemohon instance |
| D-3 | — | Nomor `UBV` dialokasikan sebelum gateway, seperti PR dan rencana kebutuhan. Bila pengajuan ditolak (`PBJ-VAL-091`), nomor itu hangus (`DEC-PLT-008`) | Pada uji, `UBV-000001` hangus di `AT-VD-23`, sehingga usulan pertama yang lolos bernomor `UBV-000002` |
| D-4 | — | Diblokir → `IsPreferredSupplier = false`. Buka blokir tidak mengembalikannya | Aturan Master Data lama: supplier blacklist tidak boleh preferred. Menjadikannya preferred lagi tetap keputusan Master Data |
| D-5 | — | Validasi lama "alasan blacklist wajib" dan "blacklist tidak boleh preferred" dihapus dari `SupplierController` | Kedua field kini diabaikan; validasi itu akan menolak permintaan tanpa efek |
| D-6 | — | Penyedia akses `PBJ_VENDOR_STATUS`: lampiran boleh ditambah pengusul selama `DRAFT`/`RETURNED`; linimasa butuh izin `Read` | Dipakai `GET /{id}/approval` dan lampiran pendukung usulan |
| D-7 | — | Riwayat vendor mencatat pengajuan (`STATUS_REQUEST_SUBMIT`) dan keputusan (`VENDOR_BLOCK`/`VENDOR_UNBLOCK`/`STATUS_REQUEST_REJECT`); dikembalikan/ditarik hanya di riwayat usulan | PERM bagian 5: "Usulan diajukan/diputuskan" |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor Status Request

Base URL: `api/v1/corporate/procurement-management/vendor-status-requests`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, jenis usulan, status | `ProcurementVendorStatusRequest : Read` | — | `VendorStatusRequestFilterMetadataResponse` |
| `GET` | `/summary` | Jumlah per status | `… : Read` | — | `VendorStatusRequestSummaryResponse` |
| `GET` | `/` | Daftar | `… : Read` | `startDate`, `endDate`, `customPeriod`, `search`, `supplierId`, `requestType`, `status`, `sortBy` (`createDateTime`/`requestNumber`/`supplier`/`status`), `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<VendorStatusRequestListItemResponse>` |
| `GET` | `/{id}` | Detail | `… : Read` | — | `VendorStatusRequestResponse` |
| `GET` | `/{id}/history` | Riwayat usulan | `… : Read` | — | `List<DocumentHistoryResponse>` |
| `GET` | `/{id}/approval` | Langkah dan keputusan `Wfl` | `… : Read` | — | `ProcurementApprovalTimelineResponse` |
| `POST` | `/` | Buat usulan | `… : Create` | `{ supplierId, requestType, reason, evidenceEvaluationId? }` | `VendorStatusRequestResponse` (**201**) |
| `PUT` | `/{id}` | Ubah alasan/bukti (`DRAFT`/`RETURNED`, pengusul) | `… : Create` | `{ expectedRowVersion, reason, evidenceEvaluationId? }` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/submit` | Ajukan (pengusul) | `… : Create` | `{ expectedRowVersion? }` + `Idempotency-Key` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/cancel` | Batalkan draf atau tarik | `… : Cancel` | `{ reason, expectedRowVersion? }` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/workflow/synchronize` | Sinkron ulang (delta D-1) | `… : Create` | — | `VendorStatusRequestResponse` |

#### Administrator / Master Data / Supplier (sudah ada — perilaku berubah)

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `api/v1/administrator/master-data/suppliers` | `isBlacklisted`/`blacklistReason` diabaikan; supplier baru selalu tidak diblokir |
| `PUT` | `api/v1/administrator/master-data/suppliers/{id}` | `isBlacklisted`/`blacklistReason` diabaikan; nilai tersimpan tetap |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning baru (satu `CS8619` lama di `SupplierController.cs:1214`, tidak disentuh) | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 10 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | 28 skenario, 28 `PASS` | `PASS` | `vd007.results.json` scratchpad sesi + query DB |

**Konfigurasi approval uji (izin pemilik 18 November 2026).** Skrip `vnd_wfl.py` menyiapkan:
- satu baris `MstApprovalMatrix` fallback `SpecificUser` (Direktur uji) untuk langkah `PBJ-VND-BLOKIR-01` dan `PBJ-VND-BUKA-01`;
- satu `MstRejectionReason` `PBJ_VENDOR_STATUS`;
- aktivasi kedua definisi.

Semuanya bertanda `DATA UJI PBJ VD007 (bukan nilai SOP)`. Di akhir uji (blok `finally`), baris uji dihapus lunak dan
kedua definisi kembali `Draft`/tidak aktif. Diperiksa ulang: ketujuh definisi `PBJ-*` tidak aktif.

**Fixture uji (`TEST-PBJ VD007 20261118095241`, lewat API SuperAdmin).** Tiga akun `Employee` **tanpa role**:
- `mgr`: usulan `Read, Create, Cancel`, vendor `Read`, kotak masuk `Wfl`.
- `dir`: usulan `Read`, vendor `Read`, kotak masuk `Wfl`; penyetuju di matriks uji.
- `staff`: usulan `Read`, vendor `Read`.

Ada juga satu supplier uji yang dibuat dengan `isBlacklisted = true` lewat Master Data.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role | `PASS` | `Cancel`, `Create`, `Read` |
| `AT-VD-26a` | `POST` supplier dengan `isBlacklisted = true` | `PASS` | Tersimpan tidak diblokir, alasan kosong |
| `S-00` | Tanpa izin `Create` membuat usulan | `PASS` | 403 |
| `AT-VD-28` | `UNBLOCK` untuk vendor tidak diblokir | `PASS` | 409 `PBJ-VAL-106` |
| `S-01` | Usulan tanpa alasan | `PASS` | 422 |
| `S-02` | Usulan `BLOCK` | `PASS` | 201 `DRAFT` tanpa nomor; profil vendor lahir |
| `AT-VD-22` | Usulan kedua untuk vendor yang sama | `PASS` | 409 `PBJ-VAL-107` |
| `AT-VD-23` | Ajukan saat definisi tidak aktif | `PASS` | 409 `PBJ-VAL-091`; tetap `DRAFT` |
| `WFL-UJI` | Approval uji diaktifkan | `PASS` | 2 definisi aktif, masing-masing 1 matriks uji |
| `S-03` | Ajukan; ulang dengan kunci sama | `PASS` | `PENDING_APPROVAL`, `UBV-000002`, instance `Wfl`; `SUBMIT` sekali |
| `S-04` | Ubah saat diproses | `PASS` | 409 `PBJ-VAL-093` |
| `AT-VD-25` | Pengusul menyetujui usulannya sendiri | `PASS` | Ditolak `Wfl` (404 "Approval assignment tidak ditemukan pada inbox user login." — penugasan milik penyetuju lain); usulan tetap `PENDING_APPROVAL`; supplier belum diblokir |
| `AT-VD-24` | Direktur menyetujui | `PASS` | `APPROVED`; `IsBlacklisted = true`, `BlacklistReason` = alasan, `IsPreferredSupplier = false`; riwayat vendor `VENDOR_BLOCK` tepat 1; status vendor Diblokir; `Wfl` `Completed` |
| `S-05` | Sinkron ulang | `PASS` | 200; `VENDOR_BLOCK` tetap 1 |
| `S-06` | Linimasa persetujuan | `PASS` | 200, satu langkah |
| `AT-VD-26` | `PUT` Master Data dengan `isBlacklisted = false` | `PASS` | 200; DB tetap diblokir, alasan tetap, preferred tetap `false` |
| `S-07` | `BLOCK` untuk vendor sudah diblokir | `PASS` | 409 `PBJ-VAL-106` |
| `S-08` | Direktur mengembalikan usulan `UNBLOCK` | `PASS` | `RETURNED` |
| `S-09` | Perbaiki dan ajukan ulang | `PASS` | Instance baru, nomor tetap, `submissionCount = 2` |
| `S-10` | Direktur menolak | `PASS` | `REJECTED`; vendor tetap diblokir; riwayat vendor `STATUS_REQUEST_REJECT` |
| `S-11` | Tarik dengan alasan pendek | `PASS` | 422 `PBJ-VAL-104` |
| `AT-VD-29` | Pengusul menarik usulan `PENDING_APPROVAL` | `PASS` | `CANCELLED`; `Wfl` `Withdrawn` |
| `S-12` | Batalkan draf; ulang | `PASS` | `CANCELLED`; 409 `PBJ-VAL-093` |
| `S-13` | Buka blokir disetujui | `PASS` | `IsBlacklisted = false`, alasan kosong; riwayat `VENDOR_UNBLOCK`; status vendor Aktif |
| `S-14` | Daftar usulan vendor uji | `PASS` | Disetujui, ditolak, dibatalkan, dibatalkan, disetujui |
| `S-15` | Jenis usulan tidak dikenal | `PASS` | 422 |
| `S-16` | Ringkasan | `PASS` | Disetujui 2, ditolak 1, dibatalkan 2 |
| `WFL-REVERT` | Approval uji dicabut | `PASS` | Kedua definisi `Draft`/tidak aktif, matriks hidup 0 |

28 skenario, 28 `PASS`.

**Catatan `AT-VD-25`.** Uji ini membuktikan pengusul tidak dapat menyetujui usulannya sendiri: penugasan dimiliki
penyetuju lain dan `Wfl` menolak. Cabang `AllowSelfApproval = false` saat pengusul **juga** penyetuju yang ditugaskan
tidak diuji terpisah. Lapisan keduanya tetap ada di penangan lifecycle Pengadaan (PBJ-AC-002).

**Sesudah uji.** Supplier uji dihapus lunak lewat API. Simulasi pembersihan sisa fixture
(`cleanup_vendor_fixture.py dry vd007.fixture.json`) mendapati:
- 5 usulan dan 1 profil;
- 18 policy, 3 Employee, 3 jabatan, 1 unit, dan 3 akun.

Apply dijalankan atas izin pemilik (18 November 2026, "lanjutkan" atas tawaran pembersihan) dalam satu transaksi:
- dihapus lunak: 5 usulan, 1 profil, 18 policy, 3 Employee, 3 jabatan, dan 1 unit (kode diberi akhiran `-DEL-20261118095241`);
- dinonaktifkan dan dikunci: 3 akun.

Sesudahnya profil dan usulan aktif = 0. Instance `Wfl` uji dibiarkan sebagai riwayat, seperti uji approval sebelumnya.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-22` usulan kedua → 409 `PBJ-VAL-107` | Terpenuhi | `AT-VD-22` |
| `AT-VD-23` definisi tidak aktif → 409 `PBJ-VAL-091`, tetap `DRAFT` | Terpenuhi | `AT-VD-23` |
| `AT-VD-24` disetujui → `APPROVED`, `IsBlacklisted = true`, alasan, satu riwayat vendor | Terpenuhi | `AT-VD-24`, `S-05` |
| `AT-VD-25` pengusul menyetujui sendiri → ditolak `Wfl`, tetap `PENDING_APPROVAL` | Terpenuhi (lihat catatan) | `AT-VD-25` |
| `AT-VD-26` `PUT` Master Data → 200, blokir tetap | Terpenuhi | `AT-VD-26`, `AT-VD-26a` |
| `AT-VD-28` `UNBLOCK` vendor tidak diblokir → 409 `PBJ-VAL-106` | Terpenuhi | `AT-VD-28` |
| `AT-VD-29` tarik → `CANCELLED`, `Wfl` `Withdrawn` | Terpenuhi | `AT-VD-29` |
| Matriks uji dibersihkan sesudahnya | Terpenuhi | `WFL-REVERT` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Definisi `PBJ-VND-*` tetap tidak aktif sampai penyetuju diisi dari SOP (`PBJ-OQ-019`). Selama itu pengajuan usulan dijawab `PBJ-VAL-091` |
| Masalah yang diketahui | Sampai `FE-PBJ-VD-004`, layar Master Data Supplier masih menampilkan centang Blacklist yang kini tidak berpengaruh. Rilis backend ini harus bersamaan dengan `FE-PBJ-VD-004` (roadmap) |
| Risiko tersisa | Delta D-4: supplier yang dibuka blokirnya tidak otomatis kembali menjadi supplier pilihan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: `VendorStatusRequestsController.cs`, `ProcurementVendorStatusService.cs`, `VendorStatusDtos.cs`, `SupplierProcurementStatusService.cs`, laporan ini. Diubah: `SupplierController.cs`, `Program.cs`, `ProcurementApprovalTimelineService.cs`, `ProcurementStatuses.cs`, `ProcurementDocumentHistoryActions.cs`, extension DI, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | Seluruh backend `manajemen-vendor` selesai (8/8). Berikutnya roadmap frontend `FE-PBJ-VD-001`..`005`; `FE-PBJ-VD-004` dirilis bersama task ini |
