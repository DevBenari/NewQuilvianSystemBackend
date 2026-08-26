# Farmasi — Roadmap Delivery Routing Depo

| Field | Value |
| --- | --- |
| Roadmap revision | `PHA-RM-001-r1` |
| Status | `DRAFT` — menunggu approval task |
| Blueprint | `PHA-BP-001` revision `3`, `approved` |
| Scope | Routing Depo saja |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| Input revision/hash | `PHA-DA-001-r1`; artifact hashes pada manifest revision `3` |
| Kontrak terkunci | `PHA-DEPOT-ROUTING-v1`, `PHA-VAL-ROUTING-v1`, `PHA-INT-ROUTING-v1`, `PHA-STATE-ROUTING-v1`, `PHA-PERM-ROUTING-v1` |
| Branch saat perencanaan | `Ikbal`; pelaksanaan wajib memverifikasi dan memperoleh otorisasi branch ini |

## Urutan phase

| Phase ID | Outcome | Dependency | Backend task | Frontend task | Status | Blocker/next action |
| --- | --- | --- | --- | --- | --- | --- |
| `PHA-PH-008` | Resolver menentukan tepat satu Depo tanpa mutation | `PHA-DEP-004` | `PHA-BE-001` | Tidak ada | `READY` setelah task disetujui | Approval task dan `TASK MODE: BACKEND` |
| `PHA-PH-009` | Perilaku resolver terbukti otomatis | `PHA-BE-001`; test infrastructure | `PHA-BE-002` | Tidak ada | `READY` setelah task disetujui | Repository belum memiliki test project; task terpisah mencegah perluasan diam-diam |
| `PHA-PH-010` | Workflow Farmasi memakai routing sebelum reservasi | `PHA-BE-001`, `PHA-BE-002`, Billing dan ledger | `PHA-BE-003` | `PHA-FE-001` bila UI existing belum menampilkan pesan | `BLOCKED` | `PHA-OQ-014`, `PHA-OQ-015`, dan trigger “Farmasi mulai memproses” belum terkunci |

Phase terblokir tidak menghalangi `PHA-PH-008` dan `PHA-PH-009`.

## Task backend

### `PHA-BE-001` — Implementasi resolver routing Depo

| Field | Isi |
| --- | --- |
| Outcome | Backend dapat menentukan tepat satu lokasi Depo atau menghasilkan rejection terstruktur tanpa mengubah data |
| Requirement/decision | `PHA-DEC-040`, `PHA-DEC-041`, `PHA-DEP-004` |
| Contract | `PHA-DEPOT-ROUTING-v1`, `PHA-VAL-ROUTING-v1`, `PHA-INT-ROUTING-v1` |
| Reuse | `TrxPatientEncounter`, `MstDrugStorageLocation`, `ApplicationDbContext`, DI existing |
| Cakupan | Tambah `PharmacyDepotRoutingService`, internal result DTO, dan registrasi `AddScoped`; tidak ada endpoint, migration, atau mutation workflow |
| Dependency | Blueprint revision `3`; backend SHA; registry owner `HealthServices/PharmacyManagement`, prefix `Phm` |
| Risiko/pemilik | Data lokasi ganda/tidak lengkap — Master Data owner; perubahan branch/source — backend owner |
| QBE handoff | Builder wajib melakukan preflight `AGENTS.md`, `BACKEND_ENGINEERING_CONTRACT.md`, registry, branch, dan working tree saat eksekusi |

Acceptance criteria:

1. Rawat Jalan memilih satu kandidat berdasarkan `ClinicId`, lalu `ServiceUnitId` bila tidak ada Clinic match.
2. IGD memilih lokasi dengan `ServiceUnitId` sama dan `StorageLocationType = Emergency`.
3. Rawat Inap memilih lokasi dengan `ServiceUnitId` sama dan `StorageLocationType = Pharmacy`.
4. Kandidat nonaktif, dihapus, Gudang Utama, karantina, non-Farmasi, atau tidak boleh dispensing dikeluarkan.
5. Nol kandidat menghasilkan `PHA_ROUTE_NOT_FOUND`.
6. Lebih dari satu kandidat pada prioritas sama menghasilkan `PHA_ROUTE_AMBIGUOUS`; tidak memilih baris pertama.
7. Jenis encounter selain tiga layanan menghasilkan `PHA_ROUTE_SERVICE_UNSUPPORTED`.
8. Resolver memakai `AsNoTracking`, cancellation token, dan tidak melakukan `SaveChanges`.
9. Build backend berhasil tanpa migration baru.

Bukti verifikasi:

- diff hanya menyentuh DTO/service/DI yang disetujui;
- `dotnet build QuilvianSystemBackend.csproj --no-restore` berhasil;
- pemeriksaan query membuktikan tidak ada mutation;
- `git status --short` membedakan perubahan task dan perubahan `Program.cs` milik user yang sudah ada.

Definition of Done:

- seluruh acceptance criteria implementasi terpenuhi;
- kontrak error code tidak berubah;
- tidak ada endpoint, tabel, migration, atau frontend baru;
- review QBE dan laporan handoff selesai;
- bukti perilaku otomatis tetap menjadi DoD `PHA-BE-002`, bukan diklaim selesai oleh task ini.

### `PHA-BE-002` — Tambahkan pengujian otomatis resolver

| Field | Isi |
| --- | --- |
| Outcome | Jalur sukses dan gagal resolver terbukti melalui test repeatable |
| Requirement | `PHA-TEST-ROUTING-v1` |
| Contract | Seluruh kontrak routing v1 |
| Cakupan | Buat/adopsi test project backend sesuai governance; uji resolver tanpa database bersama/produksi |
| Dependency | `PHA-BE-001` selesai; approval package/test infrastructure bila dependency baru diperlukan |
| Risiko/pemilik | Repository belum memiliki test project; backend engineering owner menentukan baseline package |
| Status | `READY` secara kontrak, tetapi eksekusi terpisah dari `PHA-BE-001` |

Acceptance criteria:

1. Seluruh skenario pada `testing/acceptance-test-matrix.md` yang hanya menyangkut resolver memiliki test.
2. Test tidak memakai database shared/production dan tidak membutuhkan secret.
3. Test nol/ganda membuktikan tidak ada fallback acak.
4. Test cancellation membuktikan operasi dapat dihentikan.
5. Perintah test berhasil dan hasil aktual dicatat.

Definition of Done: test project sesuai governance, seluruh test routing lulus, build tetap lulus, tidak ada migration/database execution.

### `PHA-BE-003` — Integrasikan routing ke workflow Farmasi

Status: `BLOCKED`.

Task ini belum boleh dieksekusi sampai owner menutup:

- `PHA-OQ-014`: pembayaran berhasil tetapi reservasi gagal;
- `PHA-OQ-015`: sumber authoritative payment dan idempotency;
- definisi tindakan/status yang tepat untuk “Farmasi mulai memproses”.

Setelah terbuka, task wajib memastikan routing divalidasi ulang tepat sebelum reservasi dan kegagalan tidak mengubah stok maupun payment.

## Roadmap frontend

Tidak ada task frontend untuk `PHA-PH-008` dan `PHA-PH-009` karena resolver tidak memiliki endpoint atau layar. `PHA-FE-001` belum dibuat sebagai task siap; kebutuhan UI baru dinilai setelah kontrak integrasi workflow `PHA-BE-003` selesai. Posisi alert dan komponen tetap `DEV_DISCRETION`, tetapi backend tetap authoritative atas hasil routing.

## Traceability

| Requirement/decision | Domain/design | Contract | Backend task | Frontend task | Test/bukti | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `PHA-DEC-040` tepat satu Depo | `PHA-DA-001`; backend architecture | Routing/validation v1 | `PHA-BE-001` | — | `PHA-BE-002` skenario layanan, nol, ganda | Covered |
| `PHA-DEC-041` routing tidak mengambil stok | `PHA-DA-001` | Integration v1 | `PHA-BE-001` | — | Review no-mutation; integration test | Covered untuk resolver |
| `PHA-DEP-004` reuse encounter/lokasi | Ownership table/ERD | Integration v1 | `PHA-BE-001` | — | Build dan query test | Covered |
| Validasi ulang sebelum reservasi | State/validation v1 | State v1 | `PHA-BE-003` | Mungkin `PHA-FE-001` | Belum dapat dibuat | `BLOCKED` |
| Pesan kegagalan UI | Frontend architecture | Validation v1 | `PHA-BE-003` | Belum ditetapkan | E2E belum tersedia | Coverage gap, downstream |

## Coverage gap

- Tidak ada bukti E2E sampai resolver diintegrasikan ke workflow.
- Tidak ada test project existing; `PHA-BE-002` menutup gap ini secara terpisah.
- Reservasi, Billing, dispense, dan penyerahan bukan bagian roadmap siap ini.

## Approval yang diperlukan

Roadmap ini belum mengizinkan implementasi. Untuk menjalankan task pertama, product/domain owner perlu menyetujui `PHA-RM-001-r1` dan `PHA-BE-001`, menetapkan `TASK MODE: BACKEND`, mengizinkan write backend lokal, serta mengonfirmasi branch `Ikbal` sebagai target kerja. Commit, push, migration, database, dan deployment tetap tidak termasuk.

