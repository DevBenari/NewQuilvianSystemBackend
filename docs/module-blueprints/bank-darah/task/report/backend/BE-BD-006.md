# Laporan Perubahan Backend — `BE-BD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-006` |
| Judul | Kantong dialokasikan satu aktif, alokasi keliru dibatalkan |
| Slice | Alokasi kantong — jalur kritis `BE-BD-006` → `BE-BD-007` |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian 5, revisi **11** |
| Trace | `DEC-BD-003`, `DEC-BD-007`, `DEC-BD-014`, `DEC-BD-029`, `DEC-BD-036`, `DEC-BD-037`; `BD-AGG-03`; `INV-BD-019`, `INV-BD-025`, `INV-BD-028`, `INV-BD-035`; `ARCH-BD-POS-03`, `ARCH-BD-POS-06`; api-contract `v4` `blood-units`; state-transition §3; validation-matrix §3 dan §4b |
| Contract version | `v4` — **`approved`** |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-015` ✅ — seluruhnya tertutup |
| Klasifikasi | `MEDIUM` — satu entity baru, satu enum baru, satu konfigurasi EF baru, dua endpoint baru, nol modul lain tersentuh |
| Task mode | `BACKEND` |
| Target tulis | `DevBenari/NewQuilvianSystemBackend` cabang `sukmagp` — `Areas/HealthServices/BloodBankManagement/**`, `Repositories/**`, `docs/module-blueprints/bank-darah/**` |
| Model | `claude-opus-5` |
| Commit backend saat dikerjakan | `5e54b1d5473d5995356221ea0ce2e9cc1c580dd8` cabang `sukmagp` (HEAD saat task dimulai) |
| Tanggal | 12 September 2026 · **tinjauan statik 12 September 2026 sesudah mesin pemilik restart** |
| Status | 🟡 **SELESAI SEBAGIAN — SOURCE LENGKAP, VALIDASI BELUM DIJALANKAN.** Seluruh source di dalam scope selesai ditulis. **`dotnet build` TIDAK dijalankan atas instruksi eksplisit pemilik** ("jangan lakukan build automatis, untuk build biarkan saya jalankan manual"), sehingga migration belum dibuat dan database belum disentuh. Kesembilan acceptance criteria berstatus **`NOT EXECUTED`**, bukan `PASS` |

---

## 0. Backend Governance Preflight

| Field | Hasil |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` / Blood Bank |
| Submodule | — (blueprint berbentuk `SINGLE`) |
| Pemilik & prefix pada registry | `BloodBankManagement / Blood Bank` · prefix **`Bbk`** · `BUSINESS DOMAIN / MODULE` |
| Status registry | **`ACTIVE`** — terverifikasi pada **kedua** salinan: `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30 (salinan yang dibaca `tooling/qbe/Invoke-QbeConformanceCheck.ps1`) dan registry canonical suite skill baris 30. Aktivasi tercatat 2026-09-03 atas approval owner Bank Darah dan blueprint `BD-BP-001` kontrak `v4` |
| Keberlakuan | **`NEW CODE`** untuk `BbkBloodUnitAllocation`, `BbkAllocationStatus`, `BbkBloodUnitAllocationConfiguration`, dan `BloodUnitAllocationDtos`. **`TOUCHED`** untuk empat berkas milik modul ini sendiri yang sudah ada. **Nol `LEGACY MIGRATION`** — tidak ada rename entity, tidak ada tabel lama disentuh |
| QBE ID yang berlaku | `QBE-MOD-002` kepemilikan modul dan prefix entity operasional — **lolos**, `Bbk` `ACTIVE`. `QBE-NAM-*` penamaan entity berprefix pemilik — **lolos**, `BbkBloodUnitAllocation`. `QBE-ENT-003` fakta tidak disalin dua kali — **lolos**, pasien dan komponen dibaca lewat relasi, tidak disalin ke tabel alokasi. `QBE-CODE-002`/`003` pembangkitan nomor `Count`/`Max`+1 — **tidak berlaku**, entity ini tidak punya nomor bisnis. `QBE-CODE-004` `SortOrder` generik yang dipersistensi — **tidak berlaku**. `QBE-DB-001`/`QBE-DB-002` migration dan eksekusi database — **belum dinilai**, karena migration belum dibuat |
| Peninggalan `agents/rules/` di repository target | **Tidak ada** — sudah dicabut sebagaimana mestinya |
| Selisih dua salinan governance | `BACKEND_ENGINEERING_CONTRACT.md` **identik**. `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` **berselisih** pada baris modul lain — salinan backend memuat `Platform / NumberSeriesManagement / Num` `ACTIVE` yang tidak ada di salinan suite, dan riwayat keduanya berbeda urutan. **Baris `Bbk` identik pada keduanya**, sehingga wewenang task ini tidak terpengaruh. Selisih ini adalah `ACC-DEP-007` yang sudah terbuka dan tetap milik lead — dicatat, tidak diperbaiki pada task ini |

---

## 1. Masalah yang diperbaiki

Sampai task ini, kantong darah di sistem hanya bisa sampai di pintu kulkas. `BE-BD-004`
melahirkannya dari penerimaan PMI, `BE-BD-015` menyimpannya sehingga berstatus **Tersedia** — dan di
situ perjalanannya berhenti. **Tidak ada satu pun jalan untuk menyatakan "kantong ini disiapkan
untuk pasien Ibu Ani".**

Akibatnya nyata dan berlapis:

1. **Petugas tidak dapat menyiapkan darah.** Order sudah masuk, darah sudah ada di kulkas, tetapi
   tidak ada cara mencatat kantong mana untuk pasien mana. Dalam praktik pencatatan seperti itu
   pindah ke buku tulis atau papan tulis, dan sejak saat itu sistem tidak lagi tahu keadaan stok
   yang sebenarnya.
2. **Satu kantong berisiko dijanjikan ke dua pasien.** Tanpa pengikatan yang dijaga, dua petugas di
   dua ruangan dapat sama-sama merasa kantong PRC nomor tertentu adalah milik pasiennya.
3. **Kekeliruan tidak punya jalan keluar yang tercatat.** Salah pilih kantong adalah kejadian
   sehari-hari. Tanpa jalur pembatalan yang menuntut alasan terkendali, koreksinya dilakukan dengan
   menghapus atau menimpa data — dan jejak kekeliruannya hilang bersama perbaikannya.
4. **Empat task sesudahnya tertahan.** `BE-BD-007` (bukti kecocokan dan pemberian) sampai
   `BE-BD-010` (koreksi) seluruhnya berangkat dari kantong yang sudah dialokasikan.

Task ini menutup celah itu dengan dua tindakan: **mengalokasikan** dan **membatalkan alokasi**.

**Yang sengaja tidak dikerjakan.** `reallocate`, pemberian, bukti kecocokan, jalur darurat, koreksi,
pengembalian ke PMI, penetapan tidak layak, Billing, dan frontend. `AC-BD-071` beserta endpoint
`POST /{id}/reallocate` adalah milik `BE-BD-009` sejak roadmap revisi 11, dan **tidak** dikerjakan di
sini.

---

## 2. Proses bisnis

### 2.1 Mengalokasikan kantong

Petugas Bank Darah membuka daftar kantong, memilih kantong **Tersedia**, lalu menunjuk satu baris
kebutuhan pada order darah yang berjalan. Sesudah berhasil, kantong berstatus **Dialokasikan** dan
tidak lagi muncul sebagai stok bebas bagi siapa pun.

Urutan yang dijalankan backend, dan alasan tiap langkah ada:

| No | Langkah | Kenapa ada |
| ---: | --- | --- |
| 1 | Pelaku dikenali dari akun yang login | Alokasi wajib punya pelaku manusia; `Guid.Empty` ditolak `400` |
| 2 | Isian diperiksa — baris kebutuhan wajib ditunjuk | Tanpa tujuan, alokasi tidak punya makna |
| 3 | Kantong dibaca **dengan tracking**, beserta token `Version` | Token inilah yang kelak mencegah dua penulisan bersamaan |
| 4 | Token dari layar dibandingkan bila dikirim | Layar yang memegang data lama ditolak `409` sebelum apa pun ditulis |
| 5 | **Gerbang penyimpanan `BE-BD-015` dipanggil** | Kantong belum disimpan → `422 VAL-BD-063`; lokasinya nonaktif → `422 VAL-BD-064` |
| 6 | Kantong berlebih atau menunggu keputusan ditolak | `422 VAL-BD-033` — jalurnya penyelesaian `PendingReview`, bukan alokasi langsung |
| 7 | Kantong yang sudah terikat ditolak | `409 VAL-BD-018c` |
| 8 | Status wajib **Tersedia** | Matriks §3 hanya mengenal `Available` → `Allocated` |
| 9 | Baris kebutuhan, order, dan kunjungan pasien diperiksa | Order yang sudah batal, kedaluwarsa, terpenuhi penuh, atau pasiennya sudah pulang tidak lagi menerima alokasi |
| 10 | Alokasi aktif lain dibaca sekali lagi | Penyaring kekeliruan biasa; penjaga sebenarnya ada di langkah 13 |
| 11 | Baris `BbkBloodUnitAllocation` dibuat berstatus `Active` | Pelaku dan waktunya tersimpan di baris itu sendiri |
| 12 | Kantong berpindah ke `Allocated`, `Version` naik, satu baris riwayat ditambahkan | Setiap perpindahan status meninggalkan jejak (`BD-DOM-15`) |
| 13 | Disimpan sekali, dalam satu transaksi | Pelanggaran index unik terfilter atau token diterjemahkan menjadi `409 VAL-BD-018c` |

**Kenapa urutan langkah 5 dan 6 tidak boleh ditukar — dengan contoh.** Kantong berlebih lahir
berstatus **Diterima** juga. Kalau langkah 6 dijalankan lebih dulu, kantong berlebih yang belum
pernah masuk kulkas akan ditolak dengan alasan "menunggu keputusan" (`VAL-BD-033`), padahal sebab
sebenarnya ia belum disimpan (`VAL-BD-063`) — dan petugas akan mencari tombol penyelesaian yang
belum relevan. `INV-BD-025` menetapkan tonggak penyimpanan sebagai syarat pertama, sehingga gerbang
dinilai lebih dulu. Sesudah kantong berlebih itu disimpan, statusnya menjadi **Menunggu keputusan**
dengan lokasi aktif; gerbang terbuka, dan penolakan yang benar barulah `VAL-BD-033` — yaitu
`AC-BD-033` persis seperti bunyinya.

### 2.2 Membatalkan alokasi

Petugas menyadari kantong yang dipilih keliru — misalnya salah komponen, atau salah pasien. Ia
menekan pembatalan dan **wajib memilih alasan dari daftar**, tidak boleh mengetik bebas.

**Ke mana kantong kembali bukan pilihan petugas, melainkan jawaban atas keadaan order asalnya:**

| Keadaan order asal | Kantong menjadi | Kriteria |
| --- | --- | --- |
| Masih berjalan (`Active` atau `PartiallyFulfilled`) **dan** kunjungan pasien belum berakhir | **Tersedia** — langsung dapat dipakai pasien lain | `AC-BD-043` |
| Sudah `Cancelled`, `Expired`, `FullyFulfilled`, **atau** kunjungan pasiennya sudah berakhir | **Menunggu keputusan** — bukan stok bebas | `AC-BD-044` |

**Contoh berangka untuk `AC-BD-044`.** Kantong PRC dialokasikan ke baris order Pasien B pada Senin
pukul 09.00. Senin pukul 14.00 Pasien B pulang dan kunjungannya ditutup. Selasa pukul 08.00 petugas
membatalkan alokasi itu. Kantong **tidak** kembali menjadi stok bebas: pasien yang menjadi alasan
kantong itu diminta sudah tidak ada di tempat, dan keputusan mau diapakan kantongnya — dialihkan,
dikembalikan ke PMI, atau dinyatakan tidak layak — adalah keputusan manusia yang jalurnya
`BE-BD-009`. Karena itu kantong masuk **Menunggu keputusan**.

**Dua hal yang tidak terjadi pada pembatalan:**

1. **Baris alokasinya tidak dihapus.** Ia berpindah ke `Cancelled` lalu menyimpan pelaku, waktu,
   kode alasan, dan **salinan teks** alasannya. Tiga bulan kemudian pertanyaan "kantong ini pernah
   disiapkan untuk siapa saja" masih terjawab lengkap — termasuk percobaan yang dibatalkan.
2. **Kantong tidak pernah kembali ke `Tersimpan` maupun `Diterima`.** Tonggak penempatan hanya
   dilewati sekali; kantongnya memang masih di kulkas yang sama, dan pembatalan alokasi tidak
   memindahkan apa pun secara fisik.

**Kantong yang sudah diberikan tidak dapat dibatalkan** — `422 VAL-BD-023`, `AC-BD-046`. Pemberian
bersifat terminal, dan perpindahan `Issued` → `Available` tidak ada di jalur mana pun. Jalur
perbaikannya catatan koreksi `BE-BD-010`.

### 2.3 Dua petugas, satu kantong, satu detik yang sama

Inilah `VAL-BD-018c`, dan inilah alasan validasi aplikasi saja tidak cukup.

Petugas A dan Petugas B sama-sama membuka kantong PRC nomor `PMI-0001` pada pukul 10.00.00. Keduanya
menekan alokasi pada milidetik yang hampir sama, ke baris order pasien yang berbeda:

| Waktu | Petugas A | Petugas B |
| --- | --- | --- |
| 10.00.00.100 | Membaca: belum ada alokasi aktif ✔ | — |
| 10.00.00.101 | — | Membaca: belum ada alokasi aktif ✔ |
| 10.00.00.150 | `INSERT` alokasi + `UPDATE` kantong → **berhasil** | — |
| 10.00.00.151 | — | `INSERT` alokasi → **ditolak database** |
| Hasil | Kantong terikat pada pasiennya | `409` — "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain." |

Keduanya lolos pembacaan pada langkah 10, karena keduanya membaca sebelum ada yang menulis. Yang
menolak B adalah **index unik terfilter** `IX_BbkBloodUnitAllocation_ActiveUnit` di PostgreSQL,
ditambah token `Version` pada kantong. Penolakannya ditangkap di service dan diterjemahkan menjadi
`409 VAL-BD-018c`; **nol** `DbUpdateException`, `PostgresException`, nama constraint, SQL, atau
stack trace sampai ke pengguna.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`Models/BbkBloodUnit.cs` · `Models/BbkBloodUnitPlacement.cs` · `Models/BbkBloodOrder.cs` ·
`Models/BbkBloodOrderLine.cs` · `Models/BbkTransitionHistory.cs` · `Enums/BbkBloodUnitStatus.cs` ·
`Enums/BbkBloodOrderStatus.cs` · `Services/BbkBloodUnitService.cs` · `Services/BbkBloodOrderService.cs`
(pola alasan terkendali) · `Services/BbkProviderRequestService.cs` · `Services/BbkEncounterStatusReader.cs` ·
`Controllers/BbkBloodUnitController.cs` · `Repositories/Configurations/.../BbkBloodUnitPlacementConfiguration.cs`
(pola index unik terfilter) · `Repositories/Configurations/.../BbkBloodUnitConfiguration.cs` ·
`Areas/HealthServices/MasterData/Models/MstBloodBankReason.cs` ·
`Areas/HealthServices/MasterData/Seeders/BloodBankReasonSeeder.cs` · `Repositories/ApplicationDbContext.cs` ·
`Program.cs` · `Models/IdentityModel.cs` · `QuilvianSystemBackend.csproj`

### 3.2 Berkas yang berubah

**Baru — empat berkas:**

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkAllocationStatus.cs` | Enum dua nilai `Active = 0` / `Cancelled = 1`, sesuai kamus data dan `02-backend-architecture.md` §546. Catatan pada dokumentasinya mengikat `Active` tetap bernilai `0`, karena filter index menyebut angka itu secara harfiah |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnitAllocation.cs` | Entity alokasi, seluruh kolom persis kamus data: `BloodUnitId`, `BloodOrderLineId`, `AllocationStatus`, `AllocatedByUserId`, `AllocatedAt`, `CancelReasonCode`, `CancelReasonNote`, `CancelledByUserId`, `CancelledAt`, ditambah jejak audit `IdentityModel`. Pasien dan komponen **tidak** disalin ke sini |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitAllocationConfiguration.cs` | Pemetaan tabel, empat index — `BloodUnitId`, `BloodOrderLineId`, `AllocationStatus`, dan **index unik terfilter** `IX_BbkBloodUnitAllocation_ActiveUnit` atas `(BloodUnitId) WHERE "AllocationStatus" = 0` — serta dua FK `Restrict` ke kantong dan baris order |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitAllocationDtos.cs` | `AllocateUnitRequest` (hanya `BloodOrderLineId` + `Version`), `CancelAllocationRequest` (hanya `ReasonCode` + `Version`), dan `BloodUnitAllocationDto` untuk membaca riwayat alokasi |

**Berubah — empat berkas:**

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | `AllocateAsync` dan `CancelAllocationAsync`; penolong `CheckOrderLineAsync`, `IsAllocationOriginActiveAsync`, `ReadAllocationsAsync`; konstanta pesan kanonis `VAL-BD-016/018c/023/033`; daftar `AllocatableOrderStatuses`; `BbkEncounterStatusReader` masuk constructor; `AppendTransition` bertambah parameter opsional `reasonCode`; `IsCurrentPlacementViolation` menjadi `IsSingleRowGuardViolation` yang kini mengenali **dua** index unik terfilter; `AvailableActionsFor` bertambah parameter `isExcess` dan menawarkan `Allocate`/`CancelAllocation`; `GetDetailAsync` mengembalikan riwayat alokasi; `BloodUnitOutcome` bertambah `AllocationConflict` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | Dua endpoint `POST /{id}/allocate` dan `POST /{id}/cancel-allocation`, keduanya `[AccessAction("Allocate", …)]` + `[AccessPermission("BloodUnit", "Allocate")]`; penolong `LogAllocationAsync`; `MapFailure` memetakan `AllocationConflict` → `409` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | `BloodUnitDetailDto` bertambah `Allocations` dan `CurrentAllocation`; dokumentasi `AvailableActions` disegarkan |
| `Repositories/ApplicationDbContext.cs` | Satu baris `DbSet<BbkBloodUnitAllocation>` pada region `BLOOD BANK MANAGEMENT`. Konfigurasi EF terpasang sendiri lewat `ApplyConfigurationsFromAssembly` |

**Nol berkas modul lain tersentuh.** `Program.cs` **tidak** berubah — `BbkEncounterStatusReader` sudah
terdaftar `Scoped` sejak `BE-BD-003`, sehingga dependency baru pada `BbkBloodUnitService` teresolusi
tanpa registrasi tambahan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint kontrak `v4` yang sebelumnya berstatus "Rencana" kini ada.** Kode galat dan HTTP-nya mengikuti `api-contract` baris `allocate` dan `cancel-allocation` persis: `409 VAL-BD-018c`, `422 VAL-BD-033/063/064` untuk alokasi; `422 VAL-BD-023` dan `400 VAL-BD-016` untuk pembatalan. Respons keduanya `ApiResponse<BloodUnitDetailDto>` sesuai kontrak. `GET /{id}` kini benar-benar memuat "riwayat alokasi" seperti yang sudah dijanjikan kontrak |
| Database | **Satu tabel baru `BbkBloodUnitAllocation`** beserta empat index dan dua FK `Restrict`. **Migration BELUM dibuat** dan **database BELUM disentuh** — lihat bagian 5 dan 8 |
| Keamanan/Auth | `[Authorize]` tingkat controller berlaku. Kedua endpoint dijaga `[AccessPermission("BloodUnit", "Allocate")]` yang cocok persis dengan `ControllerName = "BloodUnit"` pada `[AccessController]` dan dengan argumen pertama `[AccessAction("Allocate", …)]` pada method yang sama, sehingga kemampuannya muncul dan dapat dicentang di layar Pengaturan → Manajemen Role → Akses Role. **Nol nama peran, jabatan, departemen, `UserType`, atau `IsInRole` dipakai sebagai penentu kewenangan.** Pelaku diambil dari klaim `NameIdentifier`, tidak pernah dari body. Status, waktu, dan teks alasan ditentukan server. Log tidak memuat nomor kantong PMI maupun nama pasien; yang ditulis hanya id, kode alasan terkendali, dan status |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Unit

Base path: `/api/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/allocate` | Mengikat kantong pada satu baris kebutuhan order; kantong menjadi **Dialokasikan** | `BloodUnit : Allocate` |
| `POST` | `/{id}/cancel-allocation` | Membatalkan alokasi keliru sebelum kantong diberikan; kantong kembali **Tersedia** atau masuk **Menunggu keputusan** | `BloodUnit : Allocate` |

**`POST /{id}/allocate`**

Request `AllocateUnitRequest`:

```json
{
  "bloodOrderLineId": "00000000-0000-0000-0000-000000000000",
  "version": 3
}
```

| Kode | Kondisi | Isi pesan |
| --- | --- | --- |
| `200` | Berhasil | "Kantong berhasil dialokasikan ke baris kebutuhan order." + `BloodUnitDetailDto` |
| `400` | Pelaku tidak dikenali · baris kebutuhan kosong atau tidak ditemukan | Pesan bisnis |
| `403` | Tidak memegang `BloodUnit : Allocate` | Ditegakkan `[AccessPermission]` |
| `404` | Kantong tidak ada atau sudah dihapus | "Kantong darah tidak ditemukan atau sudah dihapus." |
| `409` | **`VAL-BD-018c`** — sudah ada alokasi aktif, atau token kantong sudah berubah | "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain." |
| `422` | **`VAL-BD-063`** belum disimpan · **`VAL-BD-064`** lokasi nonaktif · **`VAL-BD-033`** berlebih/menunggu keputusan · order asal tidak lagi menerima alokasi · kunjungan pasien sudah berakhir | Pesan kanonis matriks validasi |

**`POST /{id}/cancel-allocation`**

Request `CancelAllocationRequest`:

```json
{
  "reasonCode": "SEED-BATAL-ALOKASI",
  "version": 4
}
```

| Kode | Kondisi | Isi pesan |
| --- | --- | --- |
| `200` | Berhasil | "Alokasi kantong berhasil dibatalkan. Kantong kembali tersedia." atau "… Order asal sudah berakhir, sehingga kantong masuk daftar menunggu keputusan." |
| `400` | **`VAL-BD-016`** — alasan kosong, tidak ada di daftar, atau sudah nonaktif | "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas." |
| `403` | Tidak memegang `BloodUnit : Allocate` | Ditegakkan `[AccessPermission]` |
| `404` | Kantong tidak ada | "Kantong darah tidak ditemukan atau sudah dihapus." |
| `409` | Token kantong sudah berubah | "Kantong ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini." |
| `422` | **`VAL-BD-023`** kantong sudah diberikan · kantong tidak sedang dialokasikan · alasan berkategori salah | Pesan kanonis matriks validasi |

---

## 5. Verifikasi

**`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(rules/backend/TEST_POLICY.md).`** Kebijakan verifikasi roadmap revisi 11 bagian 0.1 berlaku dan
dipatuhi: **nol** folder `Tests/` dibuat, **nol** berkas `*Tests.cs` dibuat, **nol** project test
dibuat, **nol** dependency test ditambahkan, dan `dotnet test` **tidak** dijalankan.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Guard cabang — `git branch --show-current` | `sukmagp` | `PASS` | Keluaran perintah |
| Guard HEAD — `git rev-parse HEAD` | `5e54b1d5473d5995356221ea0ce2e9cc1c580dd8` | `PASS` | Keluaran perintah |
| Guard remote | `https://github.com/DevBenari/NewQuilvianSystemBackend.git` | `PASS` | `git remote -v` |
| Guard working tree — nol berkas `.cs` kotor sebelum implementasi | Lima berkas kotor, **seluruhnya `docs/module-blueprints/bank-darah/**`** milik koreksi tata kelola revisi 11 yang mendahului task ini; **nol** `.cs` | `PASS` dengan catatan | `git status --short`, disaring `.cs` → 0 |
| Guard roadmap — `roadmap_revision` | `11` | `PASS` | `roadmap/backend-roadmap.md` baris 11 |
| Guard roadmap — status `BE-BD-006` | 🟡 `PENDING — SIAP DIJADWALKAN (READY)` | `PASS` | Kartu `BE-BD-006` baris `Status` |
| Guard roadmap — `AC-BD-071` **bukan** milik `BE-BD-006` | Benar — tidak ada pada baris `Acceptance` kartu `BE-BD-006` | `PASS` | Kartu `BE-BD-006` baris `Acceptance (final, roadmap revisi 11)` |
| Guard roadmap — `AC-BD-071` milik `BE-BD-009` | Benar | `PASS` | `requirement-traceability.md` baris `AC-BD-071` → ⛔ `BE-BD-009` |
| Guard kebijakan §0.1 aktif | Ada | `PASS` | `roadmap/backend-roadmap.md` bagian `0.1` |
| Preflight `.ef-probe` tidak ada | Tidak ada | `PASS` | Pemindaian folder root |
| Preflight `Tests/` tidak ada | Tidak ada | `PASS` | Pemindaian folder root; `QuilvianSystemBackend.sln` memuat satu project produksi |
| Registry `Bbk` `ACTIVE` pada salinan backend | Ada, baris 30 | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Kebersihan whitespace — `git diff --check` | Bersih, nol temuan | `PASS` | Keluaran perintah |
| Inspeksi source — hak akses kedua endpoint cocok `[AccessController]`/`[AccessAction]` | `BloodUnit` + `Allocate` cocok persis di ketiga tempat | `PASS` | Inspeksi controller |
| Inspeksi source — nol hardcode role | Nol `IsInRole`, nol daftar nama peran, nol `UserType` | `PASS` | Inspeksi controller dan service |
| Inspeksi source — gerbang alokasi dipakai, tidak diduplikasi | `AllocateAsync` memanggil `EvaluateAllocationGateAsync` milik `BE-BD-015`; nol logika keaktifan lokasi ditulis ulang | `PASS` | `BbkBloodUnitService.AllocateAsync` langkah 5 |
| Inspeksi source — galat database tidak bocor | `IsSingleRowGuardViolation` menangkap dua constraint bernama, lalu `MapFailure` hanya meneruskan pesan bisnis | `PASS` | Inspeksi service dan controller |
| **`dotnet build`** | **`NOT RUN`** | **`NOT RUN`** | **Instruksi eksplisit pemilik pada task ini: "jangan lakukan build automatis, untuk build biarkan saya jalankan manual"** |
| `dotnet ef migrations has-pending-model-changes` (baseline sebelum perubahan model) | `NOT RUN` | `NOT RUN` | Menuntut assembly yang baru dikompilasi; build belum dijalankan |
| Migration `BE-BD-006` | **BELUM DIBUAT** | `NOT RUN` | Pembuatan migration menuntut baseline terkompilasi lebih dulu |
| Penerapan database | **BELUM** | `NOT RUN` | Lihat bagian 8 |
| Verifikasi manual API | `NOT EXECUTED` | `NOT RUN` | Aplikasi belum dapat dijalankan tanpa build |
| Verifikasi read-only DB | `NOT EXECUTED` | `NOT RUN` | Tabelnya belum ada; `psql` juga tidak tersedia pada PATH mesin ini |
| QBE Strict | **`DEFERRED — REQUIRES USER COMMIT`** | `NOT RUN` | QBE berbasis GitRange; source `BE-BD-006` masih belum ter-commit, dan commit adalah wewenang pemilik |

### 5.1 Pemeriksaan statis yang benar-benar dijalankan sebagai ganti build

Build tidak dijalankan, sehingga pemeriksaan berikut dikerjakan dengan membaca source. Ia
**bukan** pengganti compiler dan tidak dilaporkan sebagai bukti kompilasi:

| Yang diperiksa | Hasil |
| --- | --- |
| Keseimbangan kurung kurawal dan kurung biasa `BbkBloodUnitService.cs` | Seimbang — 66/66 dan 417/417 |
| Urutan argumen `AppendTransition` pada empat pemanggil | **Satu cacat ditemukan dan diperbaiki.** Dua pemanggil baru semula menulis `reasonCode:` pada posisi ke-5 lalu diikuti argumen posisional `actorUserId` — bentuk itu ditolak compiler C#. Diperbaiki: `reasonNote:` dipakai pada posisinya yang benar, dan `reasonCode:` dipindah ke akhir |
| Pemanggil `AvailableActionsFor` | Satu pemanggil, sudah disesuaikan ke tiga parameter |
| Nama lama `IsCurrentPlacementViolation` | Nol rujukan tertinggal di seluruh repository |
| Keterjemahan LINQ `ReadAllocationsAsync` | `MstPatient.FullName` dan `MstBloodComponent.ComponentCode`/`ComponentName` adalah properti terpetakan biasa, bukan properti hitungan — sudah dipakai pada `Where` EF di berkas yang sama |
| Kolom audit | `IdentityModel` menyediakan `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `IsDelete`, `IsCancel` bertipe seperti yang dipakai |
| Registrasi DI | `BbkEncounterStatusReader` sudah `Scoped` pada `Program.cs` baris 474; `Program.cs` tidak perlu diubah |
| Nama database efektif | `appsettings.Development.json` → `QuilvianNewDevSukma` (connection string tidak dicetak) |

### 5.2 Tinjauan statik 12 September 2026 — sesudah mesin pemilik restart

Mesin pemilik restart ketika `dotnet build` hemat sumber daya dijalankan manual, sehingga build tetap
belum pernah selesai. Sebagai gantinya seluruh perubahan `BE-BD-006` ditinjau ulang **tanpa
mengompilasi apa pun**: nol `dotnet build`, nol `dotnet restore`, nol `dotnet test`, nol
`dotnet ef`, nol analyzer, nol perintah yang memicu MSBuild. Yang dipakai hanya `git status`,
`git diff`, `git diff --check`, `git grep`, dan pembacaan berkas.

**Hasil: `FIXED`** — empat temuan diperbaiki, nol temuan dibiarkan terbuka, nol pekerjaan dibuang.

| No | Temuan | Tingkat | Tindakan |
| ---: | --- | --- | --- |
| 1 | Nama DTO pembatalan menyimpang dari kontrak `v4` | **Kontrak** | **DIPERBAIKI** — `CancelAllocationRequest` → `CancelWithReasonRequest` pada DTO, signature service, dan parameter controller. Nol rujukan lama tertinggal |
| 2 | Saringan aplikasi atas "alokasi aktif" lebih sempit daripada predikat index database | **Konsistensi invariant** | **DIPERBAIKI** — kedua query pembawa invariant kini menyaring `AllocationStatus == Active` saja, mencerminkan `WHERE "AllocationStatus" = 0` persis. Semula keduanya menambahkan `!IsDelete`, yang tidak ada pada predikat index; baris yang menempati slot index akan terbaca "tidak ada", dan pada jalur pembatalan itu dapat membuat kantong `Allocated` tanpa alokasi yang dapat dibatalkan — tanpa jalan keluar lewat API. Tidak terjangkau hari ini karena nol jalur bisnis menghapus baris alokasi, tetapi dua penjaga yang berselisih tidak dibiarkan |
| 4 | Kode alasan dinormalkan menjadi `string?`, berbeda dari pola yang sudah berjalan | **Konsistensi** | **DIPERBAIKI** — penolong `NormalizeReasonCode` ditambahkan dengan bentuk sama persis seperti `NormalizeCode` pada `BbkBloodOrderService` (`BE-BD-003`): kosong menjadi `string.Empty`, bukan `null`. Local non-nullable itu kemudian dipakai di dalam lambda EF, sehingga nol pertanyaan aliran nullable tersisa pada jalur alasan terkendali, dan kedua jalur alasan Bank Darah memperlakukan masukan klien dengan cara yang sama |
| 3 | Komentar urutan pemeriksaan pada pembatalan tidak tepat | **Dokumentasi** | **DIPERBAIKI** — komentar semula berbunyi `VAL-BD-023` diperiksa "sebelum alasan dibaca", padahal pemeriksaan kode alasan kosong (`VAL-BD-016`) memang berjalan lebih dulu. Teksnya diperjelas: `VAL-BD-023` mendahului **pencarian alasan di master**, sementara body tanpa kode alasan sama sekali tetap ditolak `VAL-BD-016` lebih dulu karena body seperti itu tidak sah bagi tindakan apa pun |

**Yang ditinjau dan terbukti benar tanpa perubahan:**

| Yang diperiksa | Hasil |
| --- | --- |
| `BbkAllocationStatus` — nilai numerik | `Active = 0` dan `Cancelled = 1` ditulis **eksplisit**, tidak bergantung urutan implisit. Dokumentasi enum menyatakan ikatannya terhadap predikat index, dan konfigurasi EF memetakannya `HasConversion<int>()`. Cocok dengan SQL kamus data `-- enum: 0 Active, 1 Cancelled` |
| `BbkBloodUnitAllocation` — kolom, tipe, nullability | Sembilan kolom kamus data cocok satu per satu, termasuk empat kolom pembatalan yang seluruhnya nullable dan kosong selama alokasi aktif. Dua navigasi, nol salinan pasien maupun komponen, audit dari `IdentityModel` |
| Konfigurasi EF | FK `BloodUnit` `Restrict` · FK `BloodOrderLine` `Restrict` · index unik terfilter `(BloodUnitId) WHERE "AllocationStatus" = 0` · tiga index biasa `BloodUnitId`, `BloodOrderLineId`, `AllocationStatus` · panjang `30`/`500` · nol konfigurasi entity lain disentuh |
| `AllocateUnitRequest` | Hanya `BloodOrderLineId` wajib + `Version` opsional. Nol field status, pelaku, waktu, maupun pasien diterima dari klien |
| Legalitas argumen bernama | Keempat pemanggil `AppendTransition` sah: argumen bernama hanya dipakai pada posisinya sendiri atau di akhir. Cacat yang ditemukan pada pass sebelumnya tetap tertutup |
| Batas transaksi | Alokasi dan pembatalan masing-masing **satu** `SaveChangesAsync`, sehingga baris alokasi, perpindahan status, kenaikan token, dan baris riwayat tersimpan dalam satu transaksi implisit EF. `BeginTransaction` eksplisit **tidak** dipakai dan memang tidak perlu — berbeda dari `MoveStorageLocationAsync` yang menyimpan dua kali. Pada kegagalan, `ChangeTracker` dibersihkan dan nol baris tersimpan |
| Penelanan galat database | `IsSingleRowGuardViolation` hanya menangkap `UniqueViolation` dengan nama constraint salah satu dari **dua** index terfilter Bank Darah. Galat unik lain — misalnya nomor kantong PMI ganda — tetap naik sebagaimana mestinya |
| Kebocoran galat ke pengguna | Nol `DbUpdateException`, `PostgresException`, nama constraint, SQL, connection string, atau stack trace melewati service. `MapFailure` hanya meneruskan pesan bisnis |
| N+1 dan ledakan query | `ReadAllocationsAsync` satu query dengan navigasi diproyeksikan menjadi `LEFT JOIN`, bukan satu query per baris. `GetDetailAsync` naik dari dua menjadi **tiga** query — tetap dan tidak bergantung jumlah data |
| Rute | `POST {id:guid}/allocate` dan `POST {id:guid}/cancel-allocation` di bawah `api/v1/health-services/blood-bank-management/blood-units` — cocok kontrak persis |
| Hak akses | `[AccessAction("Allocate", …)]` dan `[AccessPermission("BloodUnit", "Allocate")]` pada kedua method; argumen kedua `[AccessPermission]` sama persis dengan argumen pertama `[AccessAction]`, dan argumen pertamanya sama persis dengan `ControllerName = "BloodUnit"`. Dua method berbagi satu butir aksi — pola yang sama dengan `Store` pada dua endpoint penyimpanan |
| Hardcode role | Nol `IsInRole`, nol daftar nama peran, jabatan, departemen, maupun `UserType` |
| Pelaku | Dari klaim `NameIdentifier`, `Guid.Empty` ditolak `400`. Nol pelaku dari body |
| SQL mentah | Nol pada berkas `BE-BD-006`. Dua `ExecuteSqlRawAsync` yang ada di modul ini milik `BE-BD-003`/`BE-BD-004`, **terparameter** dengan `{0}`, bukan disambung teks, dan tidak disentuh |
| Kredensial | Nol. Pemindaian kata kunci hanya memunculkan `CancellationToken cancellationToken = default` |
| `ApplicationDbContext` | Tepat **satu** baris `DbSet` ditambahkan, nol duplikat, nol perubahan lain |
| `TrxPatientEncounter` | Nol rujukan pada berkas `BE-BD-006`. Kunjungan dibaca lewat `BbkEncounterStatusReader` yang memakai `RegPatientEncounter` |
| `reallocate` | Nol endpoint, nol method, nol rute. Namanya hanya muncul pada komentar yang justru menyatakan ia milik `BE-BD-009` |
| `Tests/` dan `*Tests.cs` | Nol dibuat |

**Dua hal sengaja dibiarkan apa adanya, beserta alasannya:**

| Hal | Kenapa tidak diubah |
| --- | --- |
| **Kode `VAL-BD-*` tidak terbaca mesin pada respons** | `ApiResponse.Fail` menyediakan parameter `errors` yang belum dipakai, sehingga kode aturan **dapat** dibawa ke payload. Tindakan itu tidak diambil pada task ini karena empat endpoint bersaudara di controller yang sama — milik `BE-BD-015` yang sudah ✅ — hanya mengembalikan pesan kanonis, dan `BloodUnitResult` yang dipakai bersama keduanya harus ikut berubah. Hari ini penolakan dikenali dari **status HTTP ditambah pesan kanonis matriks validasi**, sama seperti `BE-BD-015`. Akibat sampingannya: `EvaluateAllocationGateAsync` sudah menghitung `RuleCode` (`VAL-BD-063`/`VAL-BD-064`) tetapi nilainya dibuang — hanya pesannya yang diteruskan. **Perlu keputusan pemilik:** bawa kode aturan ke `errors` untuk seluruh endpoint kantong sekaligus, atau tetap mengandalkan pesan |
| **Pesan `409` pada balapan yang bukan alokasi** | Pada alokasi, kegagalan `TrySaveAsync` selalu dijawab `VAL-BD-018c`. Bila yang sebenarnya terjadi adalah petugas lain **memindahkan lokasi** kantong pada saat yang sama, token `Version` ikut naik dan permintaan alokasi gagal dengan pesan "baru saja dialokasikan petugas lain" — padahal sebabnya perpindahan. Status HTTP-nya tetap benar (`409`) dan tindakan yang diminta kepada pengguna juga sama (muat ulang lalu ulangi), sehingga dampaknya pada ketepatan pesan saja. Memisahkan keduanya menuntut `TrySaveAsync` memulangkan sebab, bukan `bool` — dan method itu dipakai bersama jalur `BE-BD-015` |

---

## 6. Acceptance criteria dan Definition of Done

**Matriks acceptance — sembilan butir milik `BE-BD-006` pada roadmap revisi 11.** Tidak satu pun
berstatus `PASS`, karena tidak satu pun benar-benar dijalankan. Source-nya ada dan dapat ditunjuk;
buktinya belum.

| Kriteria | Status bukti | Source yang menjawabnya | Yang masih dibutuhkan |
| --- | --- | --- | --- |
| `AC-BD-033` — kantong berlebih dialokasikan langsung → ditolak `VAL-BD-033` | **`NOT EXECUTED`** | `AllocateAsync` langkah 6 | Build, migration, lalu satu panggilan API atas kantong `IsExcess` yang sudah disimpan |
| `AC-BD-043` — batalkan alokasi, order asal aktif → kantong `Available` + riwayat | **`NOT EXECUTED`** | `CancelAllocationAsync` + `IsAllocationOriginActiveAsync` | Build, migration, lalu satu alokasi dan satu pembatalan |
| `AC-BD-044` — batalkan alokasi, order asal berakhir → kantong `PendingReview` | **`NOT EXECUTED`** | `IsAllocationOriginActiveAsync` lewat `BbkEncounterStatusReader` | Build, migration, lalu kunjungan yang ditutup |
| `AC-BD-045` — batalkan alokasi tanpa alasan terkendali → ditolak `VAL-BD-016` | **`NOT EXECUTED`** | `CancelAllocationAsync` pemeriksaan alasan, `400` | Build, lalu panggilan tanpa `reasonCode` dan dengan kode palsu |
| `AC-BD-046` — batalkan alokasi kantong `Issued` → ditolak `VAL-BD-023` | **`NOT EXECUTED`** | `CancelAllocationAsync` pemeriksaan status | Build; kantong `Issued` sendiri baru dapat dibuat `BE-BD-007`, sehingga pembuktian penuhnya menuntut data yang disiapkan langsung di database |
| `AC-BD-060` — kantong `Received` dialokasikan → ditolak `VAL-BD-063` | **`NOT EXECUTED`** | `AllocateAsync` langkah 5, gerbang `BE-BD-015` | Build, migration, satu panggilan API |
| `AC-BD-068` — kantong di lokasi nonaktif dialokasikan → ditolak `VAL-BD-064` | **`NOT EXECUTED`** | `AllocateAsync` langkah 5, gerbang `BE-BD-015` | Build, migration, satu lokasi dinonaktifkan |
| `AC-BD-070` — sesudah dipindah ke lokasi aktif, alokasi berhasil | **`NOT EXECUTED`** | `AllocateAsync` jalur berhasil | Build, migration, lanjutan skenario `AC-BD-068` |
| `VAL-BD-018c` — dua permintaan serentak → tepat satu berhasil, satu `409` | **`NOT EXECUTED`** | Index unik terfilter + token `Version` + `IsSingleRowGuardViolation` | Build, migration, lalu dua permintaan sungguhan yang dikirim bersamaan |

**`AC-BD-071` tidak ada di tabel ini, dan itu benar.** Ia milik `BE-BD-009` sejak roadmap revisi 11
karena menguji endpoint `reallocate`. Endpoint itu **tidak** dibuat pada task ini.

### Definition of Done

| Butir | Keadaan |
| --- | --- |
| Source vertical slice lengkap | **Terpenuhi** |
| Kontrak API diikuti tanpa penyimpangan kode galat | **Terpenuhi** |
| Hak akses terdaftar dan ditegakkan | **Terpenuhi** |
| Invariant satu alokasi aktif dijaga database | **Terpenuhi pada source**; belum ada di database |
| Gerbang `EvaluateAllocationGateAsync` dipakai, bukan diduplikasi | **Terpenuhi** |
| Gerbang tetap dapat dipakai ulang `BE-BD-009` | **Terpenuhi** — gerbangnya tidak disentuh sama sekali |
| `dotnet build` `0 Error(s)` | **BELUM** — tidak dijalankan atas instruksi pemilik |
| Migration dibuat dan diperiksa isinya | **BELUM** |
| Seluruh acceptance criteria terbukti | **BELUM** — sembilan dari sembilan `NOT EXECUTED` |
| Laporan tracked ada | **Terpenuhi** — dokumen ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak 1 — **penolakan "order tidak aktif" belum punya kode** | Matriks perpindahan status §3 menetapkan syarat "order aktif" pada alokasi, tetapi tidak memberi kode `VAL-BD-*`, dan `api-contract` hanya mencantumkan `409 VAL-BD-018c` serta `422 VAL-BD-033/063/064`. Penolakannya karena itu memakai pesan bisnis yang jelas **tanpa mengarang kode baru**. **Butuh keputusan pemilik kontrak:** beri kode tersendiri, atau biarkan tanpa kode |
| Delta kontrak 2 — **nama DTO pembatalan, SUDAH DIPERBAIKI** | Semula DTO dinamai `CancelAllocationRequest` mengikuti konvensi source. **Pada tinjauan statik nama itu diganti menjadi `CancelWithReasonRequest`, mengikuti kontrak `v4`.** Dasarnya bukan selera: `api-contract.md` menyebut nama yang sama pada **tiga** endpoint — baris 37 pembatalan order, baris 88 pembatalan permintaan PMI, baris 106 pembatalan alokasi — dan bentuk ketiganya memang identik, yaitu satu kode alasan terkendali ditambah token konkurensi. Kontrak jelas memaksudkan satu bentuk bersama, dan kontrak yang sudah disetujui mengalahkan konvensi source. **Sisa penyimpangan bukan milik task ini:** `CancelBloodOrderRequest` (`BE-BD-003`) dan `CancelProviderRequestRequest` (`BE-BD-004`) masih bernama per-aggregate. Keduanya milik task yang sudah ✅ dan **tidak** disentuh; penyeragamannya menunggu keputusan pemilik kontrak |
| Delta kontrak 3 — **`VAL-BD-018` lawan `VAL-BD-018c`** | Matriks perpindahan status §3 baris alokasi menyebut "`VAL-BD-018` alokasi ganda", sedangkan matriks validasi mendefinisikan `VAL-BD-018` sebagai gerbang bukti pada **pemberian** dan `VAL-BD-018c` sebagai alokasi aktif ganda. `api-contract` baris `allocate` menyebut **`VAL-BD-018c`**. Implementasi mengikuti matriks validasi dan `api-contract`; rujukan `VAL-BD-018` pada matriks perpindahan status perlu dirapikan pemilik kontrak |
| **Risiko tersisa 1 — kecocokan komponen tidak diperiksa** | Kantong PRC saat ini **dapat** dialokasikan ke baris kebutuhan trombosit. Kontrak `v4` **tidak** memuat aturan maupun kode galat untuk ketidakcocokan komponen pada alokasi — sudah dicari pada matriks validasi, matriks perpindahan status, dan arsitektur domain. Aturan seperti itu **tidak diarang** di sini. **Ini risiko klinis yang perlu diputuskan pemilik proses BDRS**, dan bila diputuskan, tempatnya `AllocateAsync` langkah 9 ditambah satu kode `VAL-BD-*` baru |
| Risiko tersisa 2 — `AC-BD-046` menuntut data dari task lain | Kantong berstatus `Issued` hanya dapat lahir dari `BE-BD-007`. Pembuktian `AC-BD-046` karena itu menuntut penyiapan data langsung di database dev pemilik, atau ditunda sampai `BE-BD-007` selesai |
| Risiko tersisa 3 — **kemungkinan migration modul lain yang belum diterapkan** | Folder `Migrations/` kini memuat **179** migration, sedangkan catatan penerapan terakhir pada `BE-BD-015` (11 September 2026) menyebut `141/141`. Selisihnya belum dapat diperiksa karena `dotnet ef migrations list` menuntut build dan `psql` tidak tersedia. **Bila EF hendak menerapkan migration milik modul lain lebih dulu, penerapan wajib dihentikan** dan dilaporkan `DATABASE APPLICATION = BLOCKED — UNRELATED PENDING MIGRATIONS`; jangan menyunting riwayat migration, jangan memalsukan baris `__EFMigrationsHistory`, dan jangan melewati migration tim lain |
| Risiko tersisa 4 — source belum pernah dikompilasi | Pemeriksaan statis menemukan dan memperbaiki satu cacat urutan argumen. Pemeriksaan seperti itu tidak menggantikan compiler; cacat lain masih mungkin ada dan hanya build yang dapat menjawabnya |
| Yang **tidak** dilakukan | Nol `Tests/` dibuat · nol `*Tests.cs` dibuat · nol project atau dependency test ditambahkan · `dotnet test` tidak dijalankan · frontend tidak disentuh · `reallocate` tidak diimplementasikan · `BE-BD-007` sampai `BE-BD-010` dan `BE-BD-013` tidak dimulai · nol migration dibuat · nol database disentuh · nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, atau PR · nol laporan task historis disunting · nol kepemilikan acceptance criteria dipindah |
| Task berikutnya | **Pemilik menjalankan build manual, lalu pekerjaan ini dilanjutkan dari keadaan terverifikasi** — perintahnya ada di bagian 8. Sesudah build hijau: baseline `has-pending-model-changes`, satu migration `BE-BD-006`, inspeksi isinya, penerapan ke `QuilvianNewDevSukma` bila tidak ada migration modul lain yang ikut, lalu sembilan skenario acceptance dijalankan dan laporan ini diperbarui |

---

## 8. Yang perlu dijalankan pemilik — urutan pasti

Build dan seluruh langkah sesudahnya menunggu pemilik, sesuai instruksi pada task ini.

**Langkah 1 — build hemat sumber daya.** Jalankan dari root repository:

```powershell
dotnet build .\QuilvianSystemBackend.csproj `
  --configuration Debug `
  --no-restore `
  -m:1 `
  -p:BuildInParallel=false `
  -p:UseSharedCompilation=false `
  -p:RunAnalyzers=false
```

Yang diharapkan: **`0 Error(s)`**. Jumlah warning dibandingkan baseline `210 Warning(s)` yang
tercatat pada `BE-BD-004`. Bila ada error, laporkan keluarannya — perbaikannya masuk lanjutan task
ini, bukan task baru.

**Langkah 2 — baseline EF, sebelum migration dibuat.** `ASPNETCORE_ENVIRONMENT` pada shell ini
**tidak ter-set**, sehingga EF berisiko tidak membaca `appsettings.Development.json`. Set lebih dulu:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

Lalu jalankan `has-pending-model-changes`. Perhatikan: karena model `BE-BD-006` sudah masuk source,
perintah ini **akan** melaporkan ada perubahan tertunda — dan itu memang benar. Yang wajib
dipastikan adalah **isinya hanya milik `BE-BD-006`**, yang diperiksa pada langkah 3.

**Langkah 3 — buat satu migration ber-scope.** Isinya wajib **hanya**: tabel
`BbkBloodUnitAllocation`, empat index-nya termasuk `IX_BbkBloodUnitAllocation_ActiveUnit` yang
terfilter `WHERE "AllocationStatus" = 0`, dan dua FK `Restrict`-nya. **Bila migration memuat operasi
milik `PettyCash`, `Accounting`, `Pharmacy`, `Radiology`, `Inpatient`, `Laboratory`, `Registration`,
atau modul lain — berhenti, jangan diterapkan, dan laporkan `BLOCKED`.**

**Langkah 4 — periksa dulu, terapkan kemudian.** `dotnet ef migrations list` lebih dulu. Database
yang boleh disentuh **hanya `QuilvianNewDevSukma`**. Bila EF akan menerapkan migration modul lain
lebih dulu, jangan jalankan `database update`.

**Langkah 5 — sembilan skenario acceptance** pada bagian 6, dijalankan lewat panggilan API
sungguhan, termasuk dua permintaan bersamaan untuk `VAL-BD-018c`. Alasan pembatalan yang siap pakai:
`SEED-BATAL-ALOKASI` berkategori `AllocationCancellation`.

**Langkah 6 — laporan ini dan roadmap diperbarui** dengan hasil sebenarnya, lalu status task
dinaikkan dari 🟡 menjadi ✅ hanya bila kesembilan kriteria benar-benar terbukti.
