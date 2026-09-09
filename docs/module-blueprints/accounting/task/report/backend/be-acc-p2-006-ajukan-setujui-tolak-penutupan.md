# Laporan Perubahan Backend — `BE-ACC-P2-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-006` |
| Judul | Ajukan, setujui, dan tolak penutupan |
| Slice | Gelombang `P2-4` tutup bulan |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-006` |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`, `ACC-DEC-016`, `ACC-DEC-027`; `FR-P2-026` sampai `FR-P2-029` |
| Contract version | `ACC-API-0.8`, `ACC-STATE-0.2` bagian 2, `ACC-PERMISSION-0.4`, `ACC-VALIDATION-0.6` bagian 4 |
| Dependency | `BE-ACC-P2-005` 🟡 — endpoint daftar periksanya berdiri dan dipakai ulang |
| Klasifikasi | `MEDIUM` — skor 7: repository 0 (frontend hanya dibaca), berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 1, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `1812446` |
| Tanggal | 9 September 2026 |
| Status | **🟡 `SEBAGIAN`** — 5 dari 5 acceptance terbukti dan **jalan pintas empat mata sudah ditutup**; yang tersisa hanya verifikasi PostgreSQL yang tidak dapat dijalankan di lingkungan ini. Lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sampai `BE-ACC-P2-005`, sistem sudah dapat memberi tahu apa yang menahan penutupan bulan — tetapi
belum ada cara menutupnya lewat persetujuan dua orang.

Yang dijaga di sini adalah **prinsip empat mata**: pernyataan bahwa angka satu bulan sudah final
tidak boleh datang dari satu orang saja. Manajer Akuntansi mengajukan, `Accounting Director`
menyetujui, dan keduanya wajib orang yang berbeda (`ACC-DEC-052`, meneruskan `ACC-DEC-016`).

Tanpa pemisahan itu, satu orang dapat menyatakan angka bulanan final tanpa pernah dilihat siapa
pun — dan tidak ada error apa pun yang menandainya. Yang tersisa hanya jejak audit yang menunjuk
satu nama, dan itu baru dipersoalkan saat auditor datang.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi | Status periode |
| ---: | --- | --- | --- |
| 1 | Manajer Akuntansi | Membuka daftar periksa, memastikan nol penghalang | `Open` |
| 2 | Manajer Akuntansi | Mengajukan penutupan | `Open` → **`PendingClosingApproval`** |
| 3 | Sistem | Mencatat riwayat nomor 1 `Submitted`, menyimpan pengaju | — |
| 4 | Accounting Director | Menyetujui | `PendingClosingApproval` → **`SoftClosed`** |
| 5 | Sistem | Mencatat riwayat nomor 2 `Approved` | — |

Periode berakhir di **`SoftClosed`**, bukan `Closed`. Tutup permanen tetap langkah terpisah dan
hanya sah dari `SoftClosed` — jurnal penyesuaian dan pembalikan masih boleh masuk selama masa
tenggang itu.

### 2.2 Jalur penolakan

| Langkah | Pelaku | Yang terjadi | Status periode |
| ---: | --- | --- | --- |
| 4b | Accounting Director | Menolak, **wajib menuliskan alasan** | `PendingClosingApproval` → **`Open`** |
| 5b | Sistem | Mencatat riwayat `Rejected` beserta alasannya, **mengosongkan pengaju** | — |
| 6b | Manajer Akuntansi | Memperbaiki, lalu mengajukan ulang | `Open` → `PendingClosingApproval` |

**Pengaju sengaja dikosongkan saat penolakan.** Kalau tidak, pengaju lama terus terhitung sebagai
pengaju pada putaran berikutnya — sehingga bila kelak orang lain yang mengajukan ulang, penilaian
"penyetuju bukan pengaju" dilakukan terhadap nama yang keliru.

### 2.3 Jalur tidak normal

| Keadaan | Kode | Pesan |
| --- | :---: | --- |
| Masih ada jurnal belum disahkan | `409` | "Masih ada N jurnal yang belum disahkan." |
| Periode bukan `Open` saat diajukan | `409` | "Periode ... tidak dalam keadaan terbuka." |
| **Penyetuju sama dengan pengaju** | **`403`** | "Penutupan tidak dapat disetujui oleh orang yang mengajukannya." |
| Menyetujui/menolak periode yang tidak menunggu | `409` | "Periode ... tidak sedang menunggu persetujuan penutupan." |
| Penolakan tanpa alasan | `400` | "Alasan penolakan wajib diisi." |
| Periode tidak ada | `404` | "Periode akuntansi tidak ditemukan." |

### 2.4 Contoh berangka

September 2026, seluruh jurnalnya sudah disahkan.

1. Manajer (`aaaa…0001`) mengajukan → riwayat **1** `Submitted`, periode `PendingClosingApproval`.
2. Manajer yang sama mencoba menyetujui → **`403`**. Periode **tetap** `PendingClosingApproval`.
3. Direktur (`bbbb…0002`) menolak dengan alasan *"Penyusutan September belum dijurnal."* →
   riwayat **2** `Rejected`, periode kembali `Open`, pengaju dikosongkan.
4. Manajer mengajukan ulang → riwayat **3** `Submitted`.
5. Direktur menyetujui → riwayat **4** `Approved`, periode `SoftClosed`.

Riwayatnya bertambah, **tidak pernah menimpa** — nomor 1 sampai 4 semuanya tersimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `006`; `contracts/state-transition-matrix.md` bagian 2; `contracts/api-contract.md`
grup Accounting Period; `contracts/validation-matrix.md` bagian 4; `ACC-DEC-052`, `ACC-DEC-055`,
`ACC-DEC-016`, `ACC-DEC-027`; `AccPeriodClosingApproval.cs` beserta configuration-nya;
`AccAccountingPeriod.cs`; `PeriodClosingAction.cs`; `AccAccountingPeriodService.CloseAsync`;
`AccountingPeriodController.cs`. **Frontend dibaca read-only:**
`accounting-period-slice.jsx`, `use-accounting-period.jsx`, `accounting-period-view.jsx`.

### 3.2 Berkas yang berubah

Empat berkas — satu baru, tiga diperbarui.

| Berkas | Perubahan |
| --- | --- |
| `Tests/.../AccountingManagement/AccPeriodClosingApprovalTests.cs` | **Baru.** 16 uji acceptance |
| `.../AccountingPeriod/Services/AccPeriodClosingService.cs` | `SubmitClosingAsync`, `ApproveClosingAsync`, `RejectClosingAsync`, `GetClosingHistoryAsync`, beserta pembantunya |
| `.../AccountingPeriod/DTOs/PeriodClosingDtos.cs` | `PeriodClosingApprovalResponse` dan tiga request |
| `.../AccountingPeriod/Controllers/AccountingPeriodController.cs` | **Empat endpoint**; hak akses `Approve` baru |

**Nol perubahan pada `Program.cs`** — service-nya sudah terdaftar sejak `005`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Empat endpoint baru.** Tiga sesuai `ACC-API-0.8`; yang keempat (`closing-history`) delta kontrak — lihat bagian 4 |
| Database | **Nol migration.** Menulis ke `AccPeriodClosingApproval` dan dua kolom `AccAccountingPeriod` yang sudah diterapkan `BE-ACC-P2-004` ✅ |
| Keamanan/Auth | **Hak akses `Approve` baru** pada controller Accounting Period, terpisah dari `Close`. Nol pelonggaran; pemisahan ini justru yang membuat empat mata dapat ditegakkan dari layar Akses Role |

### 3.4 Keputusan yang perlu dijelaskan

**Empat mata diperiksa terhadap data tersimpan, bukan terhadap isian permintaan.** Pemeriksaan
`periode.ClosingSubmittedBy == actorUserId` membaca kolom yang ditulis saat pengajuan. Kalau
pengaju dibaca dari badan permintaan, siapa pun dapat mengaku bukan pengaju.

**Hak akses `Approve` dipisah dari `Close`.** Ini yang membuat prinsip empat mata dapat ditegakkan
**dua lapis**: lapis pertama lewat matriks hak akses — peran yang boleh mengajukan tidak perlu
diberi hak menyetujui — dan lapis kedua di backend, yang tetap menolak walau kedua hak diberikan
ke orang yang sama. Lapis kedua tidak diserahkan ke pengaturan peran, sesuai aturan bahwa
kewenangan yang melekat pada data tetap diperiksa backend.

**Nomor urut riwayat dihitung dari baris yang sudah ada, lalu dijaga database.** Unique index
`(AccountingPeriodId, ActionSequence)` dari `BE-ACC-P2-001` adalah penjaganya: dua pengajuan
bersamaan menghasilkan nomor yang sama, dan yang kedua **ditolak database**, bukan saling menimpa.

**Penghalang dihitung ulang saat pengajuan.** Tidak dipercaya dari daftar periksa yang dilihat
pengguna beberapa menit lalu, dan memakai `HitungJurnalBelumDisahkanAsync` yang **sama persis**
dengan yang dipakai `005` — supaya daftar periksa dan pengajuan tidak mungkin berselisih.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Accounting Period

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/periods/{id}/submit-closing` | Manajer Akuntansi mengajukan penutupan | `AccountingPeriod : Close` |
| `POST` | `/api/v1/corporate/accounting/periods/{id}/approve-closing` | Penyetuju menyetujui; periode menjadi tutup sementara | `AccountingPeriod : Approve` |
| `POST` | `/api/v1/corporate/accounting/periods/{id}/reject-closing` | Penyetuju menolak; alasan tertulis wajib | `AccountingPeriod : Approve` |
| `GET` | `/api/v1/corporate/accounting/periods/{id}/closing-history` | Riwayat penutupan periode | `AccountingPeriod : Read` |

### Delta kontrak yang dicatat

| Delta | Alasan |
| --- | --- |
| **`GET /{id}/closing-history` — endpoint di luar kartu** | Riwayat persetujuan adalah **bukti audit** yang ditampilkan ke pengguna (`ACC-DEC-052`), dan tanpa endpoint ini ia tersimpan tetapi tidak pernah dapat dilihat. `FE-ACC-P2-002` menampilkan "siapa mengajukan, siapa menyetujui" dan membutuhkannya. Memakai hak akses `Read` yang sudah ada; nol permukaan baru |
| Hak akses `("AccountingPeriod", ...)`, bukan `("Period", ...)` | Sama seperti `005`: argumen pertama wajib sama persis dengan `ControllerName` |
| DTO berakhiran `Response`, bukan `Dto` | Konvensi source modul |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -c Release --no-incremental` | `0 Error(s)`, `145 Warning(s)` | `PASS` | Angka sama persis — nol warning baru |
| 16 uji `BE-ACC-P2-006` | `Failed: 0, Passed: 16` | `PASS` | `AccPeriodClosingApprovalTests` |
| Seluruh project `UnitTests.Sqlite`, 485 uji | `Failed: 3, Passed: 482` | `EXISTING / ENVIRONMENT ISSUE` | **Nol regresi**; 466 → 482 tepat `+16` |
| QBE checker atas 4 berkas | `VIOLATION: 0`, `PASS` | `PASS` | Keluaran checker |
| **Test integrasi PostgreSQL** | **Tidak dapat dijalankan** | **`NOT RUN`** | `QUILVIAN_BILLING_TEST_DB` tidak diset; fixture `fail-closed`. Sama seperti `005` |

### Rincian uji, terutama acceptance (2) dan (4)

| Uji | Yang dibuktikan |
| --- | --- |
| `MasihAdaJurnalBelumSah_PengajuanDitolak409` | **Acceptance 1** — `409`, dan periode **tidak** bergeser statusnya |
| `PeriodeBersih_PengajuanBerhasil_DanStatusMenungguPersetujuan` | Pengaju tersimpan pada `ClosingSubmittedBy` |
| **`PenyetujuSamaDenganPengaju_Ditolak403`** | **Acceptance 2** — `403`, dan periode **tetap** menunggu, tidak diam-diam tertutup |
| `PenyetujuOrangLain_Diterima_DanPeriodeMenjadiTutupSementara` | Menjadi `SoftClosed`, **bukan** `Closed` |
| `PenolakanTanpaAlasan_Ditolak400` (3 kasus: `null`, kosong, spasi) | **Acceptance 3** — `400`, nol baris riwayat bertambah |
| `PenolakanBeralasan_PeriodeKembaliTerbuka` | **Acceptance 3** — kembali `Open`, pengaju dikosongkan |
| `SesudahDitolak_DapatDiajukanUlang_RiwayatBertambah` | Riwayat 1-2-3 bertambah, tidak menimpa |
| **`PeriodeSoftClosedSebelumPhase2_TetapSahTanpaRiwayat`** | **Acceptance 4** — riwayat kosong bukan error; kedua kolom Phase 2 `null`; daftar periksanya tetap dapat dihitung |
| `PeriodeSudahTertutup_PengajuanDitolak409` | Periode tertutup tidak dapat diajukan |
| `MenyetujuiPeriodeYangTidakMenunggu_Ditolak409` | Tidak diam-diam diterima |
| `KetigaEndpoint_MembawaHakAksesYangBenar` (3 kasus) | **Acceptance 5** |
| `HakAksesMenyetujui_TerpisahDariMengajukan` | `Approve` ≠ `Close` |

**Tidak dijalankan:**

- Test integrasi PostgreSQL — lihat `005`; penyebab dan cara menutupnya sama.
- **Pengujian terhadap peran `Accounting Director` sungguhan.** Roadmap mencatat peran adalah
  data, bukan kode, dan pengisiannya lewat layar Administrator. Uji di sini membuktikan
  pemisahan hak akses **di kode**; bahwa perannya benar-benar terisi dan tercentang adalah
  langkah pengisian data yang belum dilakukan.
- Migration apa pun — nol dampak schema.

Uji manual: `NOT FEASIBLE` — layarnya `FE-ACC-P2-002`, belum dibuat.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pengajuan ditolak `409` selama masih ada penghalang | **Terpenuhi** | `MasihAdaJurnalBelumSah_PengajuanDitolak409` |
| 2 | **Penyetuju sama dengan pengaju ditolak `403`** | **Terpenuhi** | `PenyetujuSamaDenganPengaju_Ditolak403`; diperiksa terhadap `ClosingSubmittedBy` tersimpan |
| 3 | Penolakan tanpa alasan ditolak `400` dan mengembalikan periode ke `Open` | **Terpenuhi** | `PenolakanTanpaAlasan_Ditolak400` (3 kasus) + `PenolakanBeralasan_PeriodeKembaliTerbuka` |
| 4 | Periode yang sudah `SoftClosed` sebelum Phase 2 **tetap sah tanpa riwayat persetujuan** | **Terpenuhi** | `PeriodeSoftClosedSebelumPhase2_TetapSahTanpaRiwayat` |
| 5 | Ketiga endpoint membawa `[AccessPermission]` yang benar | **Terpenuhi** | `KetigaEndpoint_MembawaHakAksesYangBenar` + `HakAksesMenyetujui_TerpisahDariMengajukan` |

**Lima dari lima terpenuhi.**

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Tiga endpoint berjalan | **Ya** — ditambah `closing-history` |
| Test hijau | **Sebagian** — 16 uji SQLite hijau; **test integrasi PostgreSQL belum dijalankan** |
| Laporan task tertulis | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `🟡` |

**Butir DoD yang belum terpenuhi: test integrasi PostgreSQL.**

---

## 7. Catatan penutup

### ✅ Temuan utama — jalan pintas empat mata, **SUDAH DITUTUP**

Ini temuan terpenting task ini. Dilaporkan lebih dahulu sebagai keputusan owner, lalu
**diperbaiki atas instruksi Rizki 9 September 2026** ("jika ada temuan yang kurang dan akan
menyebabkan kekeliruan maka perbaiki saja").

`ACC-STATE-0.2` bagian 2 menyatakan perpindahan `Open` → `SoftClosed` **DILARANG** pada Phase 2:

> *"Melompati persetujuan. Inilah yang diubah `ACC-DEC-052`: pada MVP perpindahan ini sah, pada
> Phase 2 tidak lagi."*

Tetapi endpoint lama **`POST /{id}/close` masih mengizinkannya**. Ia berasal dari `BE-ACC-009` dan
tidak disentuh task ini. Akibatnya:

> Siapa pun yang punya hak `AccountingPeriod : Close` dapat memanggil `POST /{id}/close` dan
> menutup periode **tanpa persetujuan siapa pun** — melewati seluruh mekanisme yang dibangun task
> ini.

**Bukti pemakaian frontend** (dibaca read-only, nol perubahan): layar
`accounting-period-view.jsx` punya tombol **"Tutup Sementara"** dan **"Tutup Permanen"** yang
memanggil endpoint itu lewat `use-accounting-period.jsx` dan `accounting-period-slice.jsx`.
Jadi jalan pintasnya bukan teoretis — ia ada sebagai tombol di layar yang sudah berjalan.

#### Perbaikan yang diterapkan

`AccAccountingPeriodService.PeriksaPerpindahanTutup` diperketat mengikuti `ACC-STATE-0.2`
bagian 2. Endpoint `POST /{id}/close` kini **hanya** menerima `SoftClosed` menjadi `Closed`:

| Dari | Ke | Sebelum | Sesudah |
| --- | --- | :---: | :---: |
| `Open` | `SoftClosed` | Diterima | **`409`** — "Ajukan penutupan lebih dahulu" |
| `Open` | `Closed` | Diterima | **`409`** — sama |
| `PendingClosingApproval` | mana pun | Diterima | **`409`** — "Selesaikan lewat Setujui atau Tolak" |
| `SoftClosed` | `Closed` | Diterima | **Tetap diterima** |
| `Closed` | mana pun | `409` | **Tetap `409`** |

Pesan penolakannya sengaja menunjukkan jalan yang benar, bukan sekadar menolak — pengguna yang
menekan tombol lama perlu tahu apa gantinya.

#### Dampak ke frontend — perlu tindak lanjut

Layar `accounting-period-view.jsx` punya tombol **"Tutup Sementara"** dan **"Tutup Permanen"**
yang memanggil endpoint ini. Untuk periode berstatus `Open`, **keduanya kini mengembalikan
`409`** beserta pesan yang mengarahkan ke pengajuan penutupan.

Ini **disengaja dan disetujui owner**: lebih baik tombol menolak dengan penjelasan daripada
menutup periode tanpa persetujuan siapa pun. Penggantinya adalah `FE-ACC-P2-002`, yang belum
dibuat. **Nol berkas frontend disentuh** — perbaikan ini murni backend.

Untuk periode berstatus `SoftClosed`, tombol "Tutup Permanen" **tetap berjalan seperti biasa**.

### Masalah lain yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | Peran `Accounting Director` **belum terisi** di layar Administrator. Tanpa itu, acceptance (2) hanya terbukti di kode | Rizki |
| 2 | Test integrasi PostgreSQL belum tersedia | Owner Backend |
| 3 | Endpoint `closing-history` belum ada di kontrak `ACC-API-0.8` | Rizki — ratifikasi |
| 4 | Penghalang kejadian gagal dan shift kasir belum dapat diperiksa saat pengajuan — sama seperti `005`. Pengajuan saat ini hanya ditahan jurnal belum disahkan | Rizki |
| 5 | `SwaggerDocumentationTests` tidak dapat lulus pada Release | Owner Backend |
| 6 | ~~Jenis jurnal `JT` belum terisi~~ — **SELESAI** 9 Sep 2026, master jenis jurnal kini 5 baris | Selesai |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| ~~Jalan pintas `POST /{id}/close`~~ | **DITUTUP** 9 September 2026 — lihat temuan utama. Yang tersisa: tombol "Tutup Sementara" pada layar lama kini `409` untuk periode `Open`, sampai `FE-ACC-P2-002` dibuat |
| **Peran belum terisi** | Bila `Accounting Director` tidak pernah dibuat, tidak ada yang dapat menyetujui, dan periode akan tertahan di `PendingClosingApproval` |
| Perilaku PostgreSQL belum terbukti | Terutama unique index `(AccountingPeriodId, ActionSequence)` di bawah dua pengajuan bersamaan — SQLite tidak membuktikan konkurensi |

### Status Git

```text
 M Areas/Corporate/AccountingManagement/AccountingPeriod/Controllers/AccountingPeriodController.cs
 M Areas/Corporate/AccountingManagement/AccountingPeriod/DTOs/PeriodClosingDtos.cs
 M Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccPeriodClosingService.cs
 M Program.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/PeriodChecklistItemState.cs
?? Tests/.../AccountingManagement/AccPeriodClosingApprovalTests.cs
?? Tests/.../AccountingManagement/AccPeriodClosingChecklistTests.cs
```

Berisi hasil `005` dan `006` yang belum di-commit. **Nol commit, push, stage, merge, atau rebase
dilakukan.** **Nol berkas frontend disentuh.**

### Langkah berikutnya

1. **Putuskan jalan pintas `POST /{id}/close`** — ini yang menentukan apakah `006` benar-benar
   berlaku atau hanya kosmetik.
2. **Isi peran `Accounting Director`** lewat layar Administrator, supaya acceptance (2) dapat
   diuji dengan pengguna sungguhan.
3. Task terbuka berikutnya: `007` CRUD template jurnal berulang, `009` endpoint pengaturan
   akuntansi, `012` penolakan jurnal manual ke control account, `013` saldo control account.
   Sesudah `009`, gelombang `P2-5` tutup tahun (`010`) ikut terbuka.
