# Laporan Perubahan Backend — `BE-ACC-P2-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-005` |
| Judul | Daftar periksa penutupan |
| Slice | Gelombang `P2-4` tutup bulan |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-005` |
| Trace | `ACC-DEC-051`, `ACC-DEC-065`, `ACC-DEC-046`; `FR-P2-023`, `FR-P2-024`, `FR-P2-025`, `FR-P2-038` |
| Contract version | `ACC-API-0.8` grup Accounting Period; `ACC-VALIDATION-0.6` bagian 4; roadmap revisi 2 |
| Dependency | `BE-ACC-P2-004` ✅ — migration diterapkan owner 9 September 2026 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2, berkas diubah 1 (6 berkas), logika bisnis 1, kontrak API 1, database 1, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `cc59164` |
| Tanggal | 9 September 2026 |
| Status | **🟡 `SEBAGIAN`** — 5 dari 5 acceptance terbukti, tetapi **verifikasi yang diminta roadmap (test integrasi PostgreSQL) tidak dapat dijalankan**. Lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Menutup bulan adalah pernyataan bahwa angka bulan itu final. Sampai sekarang tidak ada cara
mengetahui apakah bulan itu benar-benar siap dinyatakan final.

Yang terjadi tanpa daftar periksa: Manajer Akuntansi menutup periode, lalu seminggu kemudian
ketahuan masih ada tiga jurnal berstatus draft yang belum disahkan. Angkanya sudah terlanjur
dipakai laporan, dan membukanya kembali menuntut alasan tertulis serta meninggalkan jejak audit
yang harus dijelaskan ke auditor.

Endpoint ini menjawab satu pertanyaan: **apa yang masih menahan penutupan bulan ini, dan apa yang
sebaiknya diperhatikan walau tidak menahan.**

### Kenapa hasilnya tidak boleh disimpan

Ini inti `ACC-DEC-051` dan acceptance nomor 1. Bila hasil hitungan disimpan, urutan berikut dapat
terjadi:

1. Pukul 09.00 daftar periksa dihitung: 3 jurnal belum disahkan, penutupan ditahan.
2. Pukul 09.30 rekan sebelah mengesahkan ketiganya.
3. Pukul 10.00 Manajer Akuntansi mengajukan penutupan — **ditolak**, karena angka tersimpan masih
   mengatakan 3.

Dan yang lebih berbahaya adalah kebalikannya: daftar periksa tersimpan mengatakan bersih,
sementara satu jurnal draft baru saja dibuat, dan periode ditutup dengan jurnal itu tertinggal.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Manajer Akuntansi | Membuka layar penutupan periode September 2026 |
| 2 | Sistem | Menghitung ulang seluruh butir daftar periksa **saat itu juga** |
| 3 | Sistem | Menampilkan 3 penghalang dan 6 peringatan beserta angkanya |
| 4 | Manajer Akuntansi | Menyelesaikan penghalang — mengesahkan jurnal yang tertinggal |
| 5 | Manajer Akuntansi | Memuat ulang; angkanya turun |
| 6 | Sistem | `CanSubmitClosing` menjadi benar; tombol Ajukan terbuka |

Langkah 6 berhenti di situ untuk sekarang — **pengajuannya sendiri adalah `BE-ACC-P2-006`**.

### 2.2 Tiga penghalang dan enam peringatan

**Penghalang** menahan penutupan (`ACC-DEC-051`, diperluas `ACC-DEC-065`):

| Kode | Isi | Keadaan sekarang |
| --- | --- | --- |
| `UNPOSTED_JOURNALS` | Jurnal `Draft`, `PendingApproval`, atau `Approved` | **Dihitung** |
| `FAILED_EVENTS` | Kejadian keuangan berstatus Gagal | Belum dapat diperiksa |
| `OPEN_CASH_SHIFTS` | Shift kasir belum ditutup (`ACC-DEC-065`) | Belum dapat diperiksa |

**Peringatan** boleh dilewati:

| Kode | Isi | Keadaan sekarang |
| --- | --- | --- |
| `UNBALANCED_JOURNALS` | Jurnal draft yang debit dan kreditnya belum sama | **Dihitung** |
| `HELD_EVENTS` | Kejadian tertahan (`ACC-DEC-046`) | Belum dapat diperiksa |
| `INTEGRATION_MISMATCH` | Integrasi belum cocok | Belum dapat diperiksa |
| `DEPRECIATION_NOT_RUN` | Penyusutan belum dijalankan | Belum dapat diperiksa |
| `SUSPENSE_ACCOUNT_BALANCE` | Saldo tertinggal di akun sementara | Belum dapat diperiksa |
| `OPENING_CLOSING_MISMATCH` | Selisih saldo awal dan saldo akhir | Belum dapat diperiksa |

### 2.3 Pasangan yang paling mudah tertukar

**Kejadian `Gagal` menahan. Kejadian `Tertahan` hanya memperingatkan.** Keduanya tampil di layar
yang sama dan namanya mirip, tetapi akibatnya berlawanan. `ACC-DEC-046` menegaskan kejadian
tertahan wajib muncul — supaya orang tahu angka laporan bisa kurang — tanpa menahan penutupan.

### 2.4 Contoh berangka

September 2026 berisi lima jurnal: satu `Draft`, satu `PendingApproval`, satu `Approved`, satu
`Posted`, dan satu `Rejected`.

Penghalang `UNPOSTED_JOURNALS` bernilai **3**, bukan 4 dan bukan 5. `Posted` sudah selesai, dan
`Rejected` juga sudah selesai — ia ditolak, bukan menunggu.

Sesudah satu jurnal disahkan, panggilan berikutnya mengembalikan **2**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `005` dan `006`; `contracts/api-contract.md` grup Accounting Period;
`contracts/validation-matrix.md` bagian 4; `ACC-DEC-051`, `ACC-DEC-065`, `ACC-DEC-046`;
`AccountingPeriodController.cs`; `AccAccountingPeriodService.cs`; `AccountingPeriodDtos.cs`;
`AccountingPeriodStatus.cs`; `AccJournal.cs`; `JournalStatus`; `AccountingLegalEntityGuard.cs`;
`AccountingServiceResult.cs`; `Program.cs` blok DI Accounting; `BillingTestDatabaseFixture.cs`;
`AccessPermissionAttribute.cs`; `AccessControllerAttribute.cs`.

### 3.2 Berkas yang berubah

Enam berkas — empat baru, dua diperbarui.

| Berkas | Perubahan |
| --- | --- |
| `.../AccountingPeriod/Services/AccPeriodClosingService.cs` | **Baru.** Perhitungan daftar periksa |
| `.../AccountingPeriod/DTOs/PeriodClosingDtos.cs` | **Baru.** `PeriodClosingChecklistResponse`, `PeriodClosingBlockerResponse` |
| `.../AccountingPeriod/Enums/PeriodChecklistItemState.cs` | **Baru.** `Evaluated` / `NotYetAvailable` |
| `Tests/.../AccountingManagement/AccPeriodClosingChecklistTests.cs` | **Baru.** 8 uji acceptance |
| `.../AccountingPeriod/Controllers/AccountingPeriodController.cs` | Satu endpoint, satu service pada constructor |
| `Program.cs` | **Satu baris** `AddScoped<AccPeriodClosingService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru**, sesuai `ACC-API-0.8`. Nol endpoint yang sudah ada berubah |
| Database | **Nol.** Hanya membaca. Nol entity, nol configuration, nol migration |
| Keamanan/Auth | Satu `[AccessAction]` + `[AccessPermission]` baru pada action `Read` yang sudah ada. Nol pelonggaran |

### 3.4 Keputusan yang perlu dijelaskan

**Butir yang belum dapat diperiksa dinyatakan terang-terangan, bukan dilaporkan sebagai nol.**

Ini keputusan terpenting pada task ini. Delapan dari sembilan butir bergantung pada data yang
belum berdiri. Menampilkan semuanya sebagai `0` akan membuat layar berkata *"periode bersih, siap
ditutup"* — padahal delapan pemeriksaannya tidak pernah berjalan.

Karena itu setiap butir membawa `State`: `Evaluated` atau `NotYetAvailable`, beserta
`UnavailableReason`. Respons juga membawa `IsComplete`, yang bernilai `false` selama masih ada
butir yang belum dapat diperiksa. `CanSubmitClosing` dan `IsComplete` sengaja **dua bidang
terpisah** — yang pertama menjawab "apakah ada yang menahan", yang kedua menjawab "apakah
pemeriksaannya sudah lengkap".

**Butir yang belum dapat diperiksa tidak menahan penutupan.** Menahan atas dasar sesuatu yang
tidak dapat diperiksa berarti mengunci tutup bulan selamanya sampai `P2-1` berdiri. Yang benar
adalah menyatakannya, bukan menebaknya. Ini mengikuti Catatan `ACC-DEC-065` pada roadmap: *"selama
`P2-1` belum ada, nol kejadian kas mengalir, sehingga tidak ada shift yang dapat menahan
penutupan."*

**Status jurnal ditulis sebagai daftar tegas, bukan negasi `!= Posted`.** Jurnal `Rejected` juga
bukan `Posted`. Menulisnya sebagai negasi akan membuat setiap periode yang pernah punya satu
jurnal ditolak **tidak akan pernah bisa ditutup**, tanpa penjelasan apa pun bagi penggunanya. Ini
diuji tersendiri.

**Perhitungan jurnal belum disahkan dibuat `public static`.** `BE-ACC-P2-006` akan memakainya pada
jalur pengajuan. Kalau kedua jalur menghitung sendiri-sendiri, cepat atau lambat keduanya
berselisih, dan pengguna melihat daftar periksa bersih tetapi pengajuannya ditolak `409`. Pola
`public static` menerima `ApplicationDbContext` mengikuti catatan mengikat roadmap.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Accounting Period

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/periods/{id}/closing-checklist` | Menghitung penghalang dan peringatan penutupan periode. **Dihitung saat diminta, tidak disimpan** | `AccountingPeriod : Read` |

`404` bila periodenya tidak ada. Penjaga badan hukum `AccountingLegalEntityGuard` berlaku seperti
seluruh endpoint Accounting lain.

### Delta kontrak yang dicatat

| Yang tertulis di roadmap/kontrak | Yang diimplementasikan | Alasan |
| --- | --- | --- |
| `[AccessPermission("Period","Read")]` | **`[AccessPermission("AccountingPeriod","Read")]`** | `ControllerName` pada `[AccessController]` bernilai `AccountingPeriod`. Argumen pertama `[AccessPermission]` **wajib sama persis** dengannya; bila menyimpang hasilnya `403` permanen yang tidak dapat diperbaiki dari layar Akses Role. `Period` pada kontrak adalah sebutan grup, bukan nama controller |
| `PeriodClosingChecklistDto`, `PeriodClosingBlockerDto` | `PeriodClosingChecklistResponse`, `PeriodClosingBlockerResponse` | Konvensi source modul ini memakai akhiran `Response`; kontrak memakai `Dto` sebagai sebutan umum, sama seperti `AccountingPeriodDetailDto` yang di source bernama `AccountingPeriodResponse` |
| "dua penghalang dan lima peringatan" | **tiga penghalang dan enam peringatan** | Penghalang ketiga dari `ACC-DEC-065`, yang memang diamanatkan kartu ini pada acceptance (5). Peringatan keenam adalah kejadian tertahan `ACC-DEC-046`, yang acceptance (3) mewajibkan tampil |

### Tambahan di luar tulisan kartu

`State`, `UnavailableReason`, `IsComplete`, dan `NotYetAvailableCount` **tidak disebut** kartu
roadmap. Keempatnya ditambahkan karena tanpa mereka respons ini menyesatkan — lihat bagian 3.4.
Dicatat sebagai delta kontrak untuk ratifikasi owner.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -c Release --no-incremental` | `0 Error(s)`, `145 Warning(s)` | `PASS` | Angka sama persis dengan sebelum task ini — nol warning baru |
| 8 uji `BE-ACC-P2-005` di SQLite | `Failed: 0, Passed: 8` | `PASS` | `AccPeriodClosingChecklistTests` |
| Seluruh project `UnitTests.Sqlite`, 469 uji | `Failed: 3, Passed: 466` | `EXISTING / ENVIRONMENT ISSUE` | **Nol regresi**; 458 → 466 tepat `+8` |
| QBE checker atas 6 berkas | `VIOLATION: 0`, `PASS` | `PASS` | Keluaran checker |
| **Test integrasi PostgreSQL** | **Tidak dapat dijalankan** | **`NOT RUN`** | Lihat di bawah |

### Kenapa test integrasi PostgreSQL tidak dijalankan — dan ini yang membuat status 🟡

Kolom Verifikasi kartu `005` berbunyi: *"Test integrasi PostgreSQL: siapkan periode berisi 3
jurnal belum sah, panggil endpoint, sahkan satu, panggil lagi, pastikan angkanya turun."*

**Skenarionya dijalankan** — persis seperti tertulis — tetapi **di atas SQLite**, pada uji
`TigaJurnalBelumSah_SahkanSatu_AngkanyaTurun`. Yang tidak terpenuhi adalah **PostgreSQL**-nya.

Sebabnya konkret: `BillingTestDatabaseFixture` membaca connection string **hanya** dari
environment variable `QUILVIAN_BILLING_TEST_DB`, dan variabel itu **tidak diset** di lingkungan
ini. Fixture-nya sengaja `fail-closed` dan menolak berjalan tanpanya.

**Mengarahkannya ke database dev tidak dilakukan, dan tidak boleh.** Fixture itu menjalankan
`Database.Migrate()` dan menulis baris nyata; ia bahkan menolak nama database yang tidak memuat
penanda `test`. Riwayat `RJ-BIL-BE-002` mencatat fallback semacam itu pernah menerapkan migration
ke database dev bersama tanpa ada yang memerintahkannya.

**Yang tidak terbukti karenanya:** perilaku khusus PostgreSQL. Untuk endpoint ini risikonya
kecil — seluruh perhitungannya `COUNT` hanya-baca, tanpa tipe khusus provider, tanpa
`EF.Functions`, tanpa check constraint — tetapi "kecil" bukan "nol", dan kartunya menuntut
PostgreSQL secara eksplisit. Karena itu statusnya **🟡 `SEBAGIAN`**, bukan `✅`.

**Cara menutupnya:** sediakan database test PostgreSQL, set `QUILVIAN_BILLING_TEST_DB`, lalu
pindahkan kedelapan uji ini ke project `IntegrationTests.Postgres`. Ini juga menutup sebagian
`ACC-TD-016`.

**Tidak dijalankan:**

- Migration apa pun — task ini nol dampak schema.
- Endpoint terhadap database sungguhan — belum ada pemanggilan runtime.

Uji manual: `NOT FEASIBLE` — belum ada layar; `FE-ACC-P2-001` yang akan memakainya.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | **Dihitung saat diminta, bukan disimpan** — dua panggilan berturut-turut setelah satu jurnal disahkan menghasilkan angka berbeda | **Terpenuhi** | `TigaJurnalBelumSah_SahkanSatu_AngkanyaTurun`: 3 → 2. Nol penyimpanan hasil di source; `EvaluatedAt` diisi setiap panggilan |
| 2 | Jurnal `Draft`, `PendingApproval`, dan `Approved` semuanya terhitung sebagai penghalang; `Posted` tidak | **Terpenuhi** | `HanyaTigaStatusBelumSah_YangTerhitung`: 5 jurnal, hanya 3 menahan. `Rejected` ikut diuji |
| 3 | Kejadian **Tertahan** muncul sebagai **peringatan**, bukan penghalang | **Terpenuhi** | `KejadianTertahan_AdaSebagaiPeringatan_BukanPenghalang` — ada di `Warnings`, tidak ada di `Blockers`; `FAILED_EVENTS` sebaliknya ada di `Blockers` |
| 4 | `[AccessPermission("Period","Read")]` terpasang | **Terpenuhi dengan delta** | `Endpoint_MembawaHakAksesYangCocokDenganAccessController` — terpasang sebagai `("AccountingPeriod","Read")`; alasannya di bagian 4 |
| 5 | Penghalang ketiga **disediakan tempatnya pada respons**, bernilai kosong sampai `P2-1` | **Terpenuhi** | `PenghalangKetiga_AdaTempatnya_TetapiBelumDapatDiperiksa` — `Blockers` selalu berisi 3, `OPEN_CASH_SHIFTS` ber-`State = NotYetAvailable` |

**Lima dari lima terpenuhi**, satu dengan delta penamaan hak akses yang dijelaskan.

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Endpoint berjalan | **Ya** — build lulus, terdaftar, hak akses terpasang |
| Test hijau | **Sebagian** — 8 uji SQLite hijau; **test integrasi PostgreSQL yang diminta kartu belum dijalankan** |
| Laporan task tertulis | **Ya** — berkas ini |
| Penghalang ketiga **tercatat sebagai pekerjaan menyusul**, bukan didiamkan | **Ya** — `State = NotYetAvailable` beserta alasannya pada respons, diuji, dan dicatat di sini |
| Roadmap ditandai | **Ya** — `🟡` |

**Butir DoD yang belum terpenuhi: test integrasi PostgreSQL.** Disebut apa adanya, tidak
didiamkan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 145 warning solution, seluruhnya pre-existing. Nol dari task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **Delapan dari sembilan butir belum dapat diperiksa.** Daftar periksa saat ini praktis hanya memeriksa jurnal belum disahkan. Layar wajib menampilkan `IsComplete = false` supaya pengguna tidak salah menyimpulkan | Rizki / `FE-ACC-P2-001` |
| 2 | Empat bidang tambahan (`State`, `UnavailableReason`, `IsComplete`, `NotYetAvailableCount`) belum ada di kontrak `ACC-API-0.8` | Rizki — ratifikasi |
| 3 | Test integrasi PostgreSQL belum tersedia; `QUILVIAN_BILLING_TEST_DB` belum diset | Owner Backend |
| 4 | Satu baris ditambahkan ke `Program.cs`. Blok DI Accounting di sana memang menyatakan *"satu baris per service modul"*, jadi ini pola yang sudah berlaku — tetapi dicatat karena preferensi umum adalah tidak menyentuh `Program.cs` | Rizki |
| 5 | `SwaggerDocumentationTests` tidak dapat lulus pada Release | Owner Backend |
| 6 | ~~Jenis jurnal `JT` belum terisi~~ — **SELESAI** 9 Sep 2026, master jenis jurnal kini 5 baris | Selesai |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Daftar periksa yang tampak bersih** | `CanSubmitClosing` dapat bernilai benar sementara delapan pemeriksaan belum berjalan. Mitigasinya `IsComplete`, tetapi mitigasi itu hanya bekerja bila layar benar-benar menampilkannya |
| **`006` bergantung pada perhitungan yang sama** | Pengajuan penutupan wajib memakai `HitungJurnalBelumDisahkanAsync` yang sama, bukan menghitung ulang sendiri |
| Perilaku PostgreSQL belum terbukti | Lihat bagian 5 |

### Status Git

```text
 M Areas/Corporate/AccountingManagement/AccountingPeriod/Controllers/AccountingPeriodController.cs
 M Program.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/DTOs/PeriodClosingDtos.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/PeriodChecklistItemState.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccPeriodClosingService.cs
?? Tests/.../AccountingManagement/AccPeriodClosingChecklistTests.cs
```

Ditambah berkas `003`, `004`, dan `011` yang juga belum di-commit.
**Nol commit, push, stage, merge, atau rebase dilakukan.**

### Langkah berikutnya

**`BE-ACC-P2-006`** — ajukan, setujui, dan tolak penutupan. Ia melanjutkan
`AccPeriodClosingService` yang sudah berdiri, dan tutup bulan menjadi berjalan penuh.

Satu hal yang wajib disiapkan lebih dulu: acceptance (2) `006` menuntut **penyetuju bukan
pengaju**, dan pengujiannya memerlukan peran `Accounting Director` benar-benar terisi lewat layar
Administrator. Roadmap sudah memverifikasi bahwa peran adalah data, bukan kode — nol perubahan
platform — tetapi pengisiannya tetap harus dilakukan sebelum acceptance itu diuji.
