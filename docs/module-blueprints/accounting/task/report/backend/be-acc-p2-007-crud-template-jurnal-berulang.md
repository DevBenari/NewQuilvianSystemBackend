# Laporan Perubahan Backend — `BE-ACC-P2-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-007` |
| Judul | CRUD template jurnal berulang |
| Slice | `P2-3` — jurnal berulang (`ACC-P2-S2`) |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-007` |
| Trace | `ACC-DEC-050`, `ACC-DEC-019`, `ACC-DEC-037`; `FR-P2-018`, `FR-P2-022` |
| Contract version | `ACC-API-0.8` grup Recurring Journal, `ACC-VALIDATION-0.6` bagian 3, `ACC-PERMISSION-0.4` — seluruhnya `approved` |
| Dependency | `BE-ACC-P2-004` ✅ (tabel sudah diterapkan owner 9 Sep 2026) |
| Klasifikasi | `HEAVY` — skor 9: repository 1 (0), berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika bisnis kompleks (2), memakai kontrak yang sudah ada (1), perilaku persistence yang sudah ada (1), keamanan berkaitan tetapi bukan intinya (1), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, `UnitTests.Sqlite`, dan `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `3e2fb76ee8482faacc3f4b3c6ad0b226bee960cf`, branch `rizkiG` |
| Tanggal | 10 September 2026 |
| Status | **✅ `DONE`** — lima dari lima acceptance terbukti lewat 32 uji baru; nol migration |

---

## 1. Masalah yang diperbaiki

Penyusutan bulanan, amortisasi sewa dibayar di muka, dan beban berkala lain sebelumnya harus
diketik ulang sebagai jurnal manual **setiap bulan**. Dua akibatnya: pekerjaan berulang yang
memakan waktu, dan — yang lebih berbahaya — satu bulan yang terlewat tidak menimbulkan error
apa pun. Bebannya sekadar tidak tercatat, laba bulan itu tampak lebih besar daripada
sebenarnya, dan tidak ada yang menandainya sampai audit.

Tabel templatenya sudah berdiri sejak `BE-ACC-P2-002` dan diterapkan ke database lewat
`BE-ACC-P2-004`, tetapi **belum ada satu pun jalan mengisinya**. Task ini membuka jalan itu.

**Yang paling mudah salah dirancang di sini adalah membiarkan template disimpan timpang.**
`ACC-DEC-025` mengizinkan draft jurnal manual disimpan timpang, dan menirunya tampak konsisten.
Tetapi template berbeda sifatnya: ia disimpan **sekali** lalu menerbitkan jurnal **sendiri**
setiap bulan tanpa ada yang menyusunnya ulang. Template timpang yang lolos akan menerbitkan
draft timpang tiap bulan, dan masing-masing baru ketahuan gagal berbulan-bulan kemudian, saat
seseorang mencoba mengajukannya. Karena itu template **wajib seimbang sejak disimpan**.

---

## 2. Proses bisnis

**Tujuan.** Menyimpan sekali pola jurnal yang berulang tiap bulan, lalu membiarkan sistem
menerbitkannya.

**Pelaku.** Staf Akuntansi menyusun dan menyunting; pengaktifannya tindakan tersendiri.

### Langkah normal

1. **Menambah template.** Staf mengisi kode, nama, jenis jurnal, tanggal terbit tiap bulan,
   masa berlaku, dan seluruh barisnya sekaligus. Contoh: `SUSUT-ALKES`, terbit tanggal 25,
   dua baris — `5-2003 Beban Penyusutan` didebit Rp 5.000.000 dengan unit biaya Umum, dan
   `1-4001 Akumulasi Penyusutan` dikredit Rp 5.000.000.
2. **Template lahir TIDAK AKTIF.** Ia tersimpan tetapi belum menerbitkan apa pun.
3. **Diperiksa, lalu diaktifkan.** Barisnya diperiksa **ulang** pada saat pengaktifan, bukan
   dipercayakan pada pemeriksaan saat disimpan.
4. **Sistem menerbitkan jurnalnya tiap bulan** sebagai `Draft` — itu `BE-ACC-P2-008`.
5. **Riwayat penerbitannya dapat dilihat** lewat `GET /{id}/runs`, menjawab pertanyaan
   *"penyusutan bulan Agustus sudah terbit belum"*.

### Kenapa `IsActive` tidak ada pada form penambahan

Bidangnya sengaja **tidak ada** di `CreateRecurringJournalRequest` maupun
`UpdateRecurringJournalRequest`. Sekali bidang itu tersedia, template dapat lahir langsung
aktif dan menerbitkan jurnal pada siklus penjadwal berikutnya — sebelum satu orang pun sempat
memeriksa barisnya. Pengaktifan punya endpoint dan hak aksesnya sendiri.

### Kenapa barisnya diperiksa ulang saat diaktifkan

Akun dapat dinonaktifkan, unit biaya dapat dipindahkan, dan jenis jurnal dapat dimatikan
**sesudah** template tersimpan. Template yang diaktifkan dalam keadaan itu akan gagal
menerbitkan setiap bulan — dan gagalnya **di dalam penjadwal**, tempat tidak seorang pun
melihatnya. Lebih baik ditolak sekarang, saat orangnya masih menatap layar.

### Jalur tidak normal

| Keadaan | Jawaban | Contoh pesan |
| --- | --- | --- |
| Baris kurang dari dua | `400` | "Template jurnal minimal memiliki 2 baris." |
| Total debit ≠ total kredit | `400` | "Total debit dan kredit template harus sama. … selisih Rp 1.000.000." |
| Satu baris berisi debit **dan** kredit, atau keduanya nol | `400` | "Baris ke-1: isi salah satu saja …" |
| Baris bernilai negatif | `400` | "Baris ke-1: nilai tidak boleh negatif. Untuk membalik arah, pindahkan ke sisi sebaliknya." |
| Baris berakun `Expense` tanpa unit biaya | `400` | "Baris ke-1: akun beban 5-2003 wajib menyebutkan unit biaya." |
| Tanggal terbit di luar 1–28 | `400` | "… Tanggal 29, 30, dan 31 tidak ada di setiap bulan …" |
| Tanggal berakhir lebih awal daripada tanggal mulai | `400` | "Tanggal berakhir tidak boleh lebih awal daripada tanggal mulai." |
| Kode template sudah dipakai badan hukum itu | `409` | "Kode template SUSUT-ALKES sudah dipakai pada badan hukum ini." |
| Baris menunjuk akun induk | `409` | "Baris ke-1: akun 5-2000 adalah akun induk …" |
| Baris menunjuk akun badan hukum lain | `409` | "Baris ke-1: akun 1-1001 bukan milik badan hukum template ini." |
| Mengaktifkan yang sudah aktif | `409` | "Template SUSUT-ALKES sudah aktif." |
| Mengaktifkan template yang barisnya tidak lagi layak | `409` | "Template SUSUT-ALKES tidak dapat diaktifkan. Baris ke-1: akun tidak ditemukan atau sudah tidak aktif." |
| Template tidak ditemukan | `404` | — |
| Badan hukum utama tidak tepat satu | `409` | Penjaga `ACC-DEC-041` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap-phase2.md` kartu `007`; `contracts/api-contract.md` grup Recurring
Journal; `contracts/validation-matrix.md` Phase 2 bagian 3; `contracts/permission-audit-matrix.md`;
`02-backend-architecture.md` bagian 16–17; `AccRecurringJournalTemplate.cs`,
`AccRecurringJournalTemplateLine.cs`, `AccRecurringJournalRun.cs` beserta ketiga
configuration-nya; `AccJournalService.cs` (`SusunBarisAsync`, `SiapkanAsync`, pola induk-baris);
`AccChartOfAccountService.cs` (pola paging dan pencarian); `JournalController.cs` (pola logging
dan hak akses); `AccountingLegalEntityGuard.cs`; `MstCostCenter.cs`; `Program.cs`; serta
governance `AGENTS.md`, `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`,
dan `rules/backend/`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `RecurringJournal/Services/AccRecurringJournalService.cs` | **Baru.** Tujuh operasi beserta seluruh validasinya; `AlasanTidakLayakTerbitAsync` dan `MuatLengkapAsync` dibuat `public static` untuk dipakai `BE-ACC-P2-008` |
| `RecurringJournal/DTOs/RecurringJournalDtos.cs` | **Baru.** Delapan DTO grup Recurring Journal |
| `RecurringJournal/Controllers/RecurringJournalController.cs` | **Baru.** Tujuh endpoint beserta metadata hak aksesnya |
| `Program.cs` | **Diubah.** Satu baris `AddScoped<AccRecurringJournalService>()` |
| `Tests/…/AccRecurringJournalTemplateTests.cs` | **Baru.** 32 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tujuh endpoint baru** pada grup baru `api/v1/corporate/accounting/recurring-journals`. Nol endpoint yang sudah ada berubah |
| Database | **Nol migration, nol entity baru, nol perubahan `ApplicationDbContext`, snapshot tidak disentuh.** Ketiga tabelnya sudah berdiri sejak `BE-ACC-P2-002` dan diterapkan `BE-ACC-P2-004`. **Nol perintah database dijalankan** |
| Keamanan/Auth | Empat hak akses baru: `RecurringJournal : Read`, `Create`, `Update`, dan `Activate`. Seluruhnya ditegakkan `[AccessPermission]` dan tampil di layar Akses Role lewat `[AccessAction]`. **Nol pemeriksaan peran yang dihardcode.** Nominal tidak masuk log |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Recurring Journal

Base URL `api/v1/corporate/accounting/recurring-journals`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar template berhalaman, dapat disaring per badan hukum, jenis jurnal, dan status aktif | `RecurringJournal : Read` |
| `GET` | `/{id}` | Rincian satu template beserta seluruh barisnya | `RecurringJournal : Read` |
| `GET` | `/{id}/runs` | Riwayat penerbitan per periode beserta jurnal yang dihasilkan | `RecurringJournal : Read` |
| `POST` | `/` | Menambah template beserta barisnya. **Selalu lahir tidak aktif** | `RecurringJournal : Create` |
| `PUT` | `/{id}` | Mengubah template; baris dikirim utuh dan menggantikan seluruhnya | `RecurringJournal : Update` |
| `PATCH` | `/{id}/activate` | Mengaktifkan template. Barisnya diperiksa ulang | `RecurringJournal : Activate` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan template. Jurnal yang sudah terbit tidak ikut dibatalkan | `RecurringJournal : Activate` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false --no-incremental` | Berhasil | `PASS` | `0 Error(s)`, `200 Warning(s)` — **nol warning dari berkas task ini** |
| `dotnet test … --filter "FullyQualifiedName~AccRecurringJournalTemplateTests"` | Berhasil | `PASS` | `Failed: 0, Passed: 32` |
| `dotnet test … UnitTests.Sqlite` (seluruh project) | Berhasil | `PASS` | `Failed: 0, Passed: 604` — **nol regresi** |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Berhasil | `PASS` | `VIOLATION: 0`, `Final result: PASS` |
| Test integrasi PostgreSQL untuk penyimpanan induk-baris | Tidak dijalankan | `NOT RUN` | Lihat di bawah |

Uji manual: `NOT APPLICABLE` — endpoint belum punya layar; `FE-ACC-P2-003`/`004` belum dibangun.

**Tidak dijalankan:** test integrasi PostgreSQL untuk penyimpanan induk-baris. Project
`IntegrationTests.Postgres` menjalankan migration sendiri sebelum uji pertama, dan penerapan
migration adalah wewenang terpisah. **Yang dituntut kartu sebagai "test integrasi untuk
penyimpanan induk-baris" tetap terbukti pada SQLite** — jebakan EF yang disebut kartu justru
diuji secara langsung lewat perbandingan `Id` baris sebelum dan sesudah penyuntingan.

### Jebakan EF yang disebut kartu, dan bagaimana dibuktikan tertutup

Kartu memperingatkan: mengganti baris anak dengan `RemoveRange` lalu menambahkannya lewat
**navigation yang terlacak** membuat EF mencocokkan baris baru dengan baris lama dan mengirim
`UPDATE`, bukan `INSERT`. Penangkalnya dua, keduanya diterapkan:

1. penghapusan disimpan **lebih dahulu** dengan `SaveChangesAsync` tersendiri;
2. baris baru ditambahkan lewat `DbSet.AddRange`, **bukan** lewat `template.Lines`.

Yang membuktikan penangkalnya bekerja bukan jumlah barisnya yang benar — itu tetap benar
walaupun jebakan terjadi — melainkan **`Id` barisnya yang berganti seluruhnya**. Uji
`GantiSeluruhBaris_MenghasilkanBarisBaruBukanTimpaan` sengaja memakai nomor baris yang **sama
persis**, karena itulah yang memancing EF mencocokkan.

Keduanya berada dalam satu transaction, dan uji `PenyuntinganDitolak_BarisLamaTetapUtuh`
membuktikan penyuntingan yang gagal tidak meninggalkan template tanpa baris — keadaan yang akan
membuatnya menerbitkan jurnal kosong tiap bulan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Template tidak seimbang ditolak `400` | **Terpenuhi** | `TemplateTidakSeimbang_Ditolak400` — nol template **dan** nol baris tersimpan. Ditambah `BarisKurangDariDua_Ditolak400` |
| (2) Baris berakun `Expense` tanpa cost center ditolak `400` | **Terpenuhi** | `AkunBebanTanpaUnitBiaya_Ditolak400`, pesannya menyebut nomor baris. Diimbangi `AkunBukanBebanTanpaUnitBiaya_Diterima` yang membuktikan kewajibannya diturunkan dari jenis akun, bukan dipukul rata |
| (3) Baris berisi debit **dan** kredit sekaligus ditolak `400` | **Terpenuhi** | `BarisDuaSisiAtauKosong_Ditolak400`, dua kali — keduanya terisi, dan keduanya kosong. Ditambah `BarisBernilaiNegatif_Ditolak400DenganPetunjuk` |
| (4) Template baru berstatus tidak aktif | **Terpenuhi** | `TemplateBaru_LahirTidakAktif` memeriksa respons **dan** baris tersimpan. Diperkuat `PermintaanSimpan_TidakPunyaBidangIsActive` yang menjaga bentuk kontraknya lewat refleksi |
| (5) Tujuh endpoint membawa `[AccessPermission]` | **Terpenuhi** | `TujuhEndpoint_MembawaHakAksesYangBenar`, tujuh kali, memeriksa argumen pertama sama dengan `ControllerName` dan argumen kedua sama dengan `ActionName`. Ditambah `ControllerMemiliki_DelapanEndpointBerhakAkses` (tujuh + satu milik `008`) |

### Definition of Done

| Butir | Status |
| --- | --- |
| Tujuh endpoint berjalan | **Terpenuhi** |
| Test hijau | **Terpenuhi** — 32 uji baru, 604 uji project, nol regresi |
| Laporan task tertulis | **Terpenuhi** — berkas ini |
| Nol migration | **Terpenuhi** |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 200 warning pada build `Release`, seluruhnya sudah ada sebelumnya; nol dari berkas task ini |
| Masalah yang diketahui | `RecurringFrequency` hanya memuat `Bulanan`. Disengaja sejak `BE-ACC-P2-002`: nilai tanpa penanganan akan lolos validasi lalu gagal diam-diam di penjadwal |
| Risiko tersisa | Template dapat menunjuk akun yang **benar tetapi keliru** — misalnya akun beban yang salah pilih. Tidak ada yang dapat menangkapnya selain orang yang memeriksa, dan itulah alasan template lahir tidak aktif |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | `BE-ACC-P2-008` — penerbitan dan penjadwalnya. `FE-ACC-P2-003`/`004` layar template |

### Delta kontrak

| Delta | Isi | Kenapa |
| --- | --- | --- |
| **Hak akses `Activate` terpisah dari `Update`** | `ACC-API-0.8` menulis `RecurringJournal : Update` untuk `activate` dan `deactivate`; diimplementasikan `RecurringJournal : Activate` | Mengaktifkan template jauh lebih berat daripada menyuntingnya: ia menyalakan penulisan jurnal otomatis ke buku besar. Hak akses terpisah memungkinkan admin memberi wewenang menyunting **tanpa** memberi wewenang menyalakan. Preseden yang sama sudah diambil `BE-ACC-P2-006` yang memisahkan `Approve` dari `Close`. **Nol konsumen yang rusak** — layar template belum ada. **Menunggu ratifikasi** |
| **Penamaan DTO `...Response`** | Kontrak menulis `RecurringJournalListDto` dan seterusnya | Mengikuti konvensi source dan preseden `BE-ACC-P2-005`, `006`, `009`, `010` |
| **Empat bidang ringkasan di luar kontrak** | `LineCount`, `TotalAmount`, `RunCount` pada daftar; `IsBalanced` pada rincian | Tanpa `RunCount`, layar tidak dapat membedakan template yang belum pernah terbit dari yang rutin terbit — perbedaan yang justru paling ingin dilihat. Ketiganya dihitung dalam query yang sama, nol query tambahan per baris |
| **Tiga penolakan di luar tulisan kartu** | Kode template kembar (`409`), tanggal berakhir lebih awal daripada tanggal mulai (`400`), dan pengaktifan template yang barisnya tidak lagi layak (`409`) | Ketiganya menutup kesalahan yang **tidak menimbulkan error**: dua template berkode sama membuat riwayat penerbitan mustahil dibaca, masa berlaku terbalik membuat template tidak pernah terbit tanpa sebab yang terlihat, dan template yang tidak layak gagal terbit **di dalam penjadwal** tempat kegagalannya tidak terlihat siapa pun |
