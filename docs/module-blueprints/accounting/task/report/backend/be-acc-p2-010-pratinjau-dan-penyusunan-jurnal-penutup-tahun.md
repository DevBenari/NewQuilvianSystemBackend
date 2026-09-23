# Laporan Perubahan Backend — `BE-ACC-P2-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-010` |
| Judul | Pratinjau dan penyusunan jurnal penutup tahun |
| Slice | `P2-5` — tutup tahun (`ACC-P2-S4`) |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-010` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`, `ACC-DEC-029`, `ACC-DEC-019`, `ACC-DEC-013`; `FR-P2-030` sampai `FR-P2-034` |
| Contract version | `ACC-API-0.8` grup Year End Closing, `ACC-VALIDATION-0.6` bagian 5, `ACC-STATE-0.3` bagian 2.2 dan 4, `ACC-PERMISSION-0.4` — seluruhnya berstatus `approved`. `ACC-STATE` naik `0.2` → `0.3` lewat `ACC-DEC-067`, 10 September 2026 |
| Dependency | `BE-ACC-P2-006` 🟡 (jalur penutupan periode berjalan), `BE-ACC-P2-009` ✅ (akun laba ditahan) |
| Klasifikasi | `HEAVY` — skor 10: repository 1 (0), berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika bisnis kompleks (2), memakai kontrak yang sudah ada (1), hanya perilaku query/persistence yang sudah ada (1), keamanan berkaitan tetapi bukan intinya (1), workflow terbatas (1), ditambah kenaikan satu tingkat karena dua faktor jatuh di tingkat berikutnya |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project uji `UnitTests.Sqlite`, dan `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `3e2fb76ee8482faacc3f4b3c6ad0b226bee960cf`, branch `rizkiG` |
| Tanggal | 10 September 2026 |
| Status | **🟡 `SEBAGIAN`** — enam dari enam acceptance criteria terbukti lewat 20 uji baru pada `UnitTests.Sqlite`; **test integrasi PostgreSQL `NOT RUN`** karena menjalankannya menuntut penerapan migration ke basis data dev pemilik, dan itu wewenang terpisah yang sengaja dihentikan pada task ini |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, tidak ada satu pun jalan untuk menutup tahun buku. Akun pendapatan dan
beban terus menumpuk sepanjang tahun, dan pada 1 Januari saldo mereka tetap membawa angka tahun
lalu — sehingga laporan laba rugi tahun berjalan menampilkan gabungan dua tahun, dan laba tahun
lalu tidak pernah sampai ke akun laba ditahan.

**Bahaya yang sesungguhnya bukan ketiadaan fiturnya, melainkan cara fitur ini gagal bila salah
dibuat.** Jurnal penutup yang angkanya keliru **tetap seimbang**: total debit sama dengan total
kredit, jurnalnya lolos pengajuan, lolos persetujuan, dan lolos pengesahan. Tidak ada error di
mana pun. Angkanya baru ketahuan salah bertahun-tahun kemudian, lewat saldo awal laba ditahan yang
tidak dapat dijelaskan siapa pun.

Contoh nyata cara kesalahan itu terjadi, dan yang justru paling lazim: tutup tahun 2026 hampir
selalu dikerjakan pada Februari atau Maret 2027, ketika Januari 2027 sudah berisi jurnal yang
disahkan. Bila saldo dihitung **seumur hidup akun** — cara paling sederhana, dan cara yang
dipakai `AccChartOfAccountService.HitungSaldoAsync` — pendapatan Januari 2027 akan ikut tersapu
ke laba ditahan tahun 2026, dan akun pendapatan 2027 menjadi negatif. Jurnalnya tetap seimbang.
Tidak ada yang menandainya. Karena itu perhitungan pada task ini dibatasi periode tahun buku yang
ditutup, dan keadaan tersebut diuji tersendiri.

---

## 2. Proses bisnis

**Tujuan.** Menolkan saldo seluruh akun pendapatan dan beban satu tahun buku, lalu memindahkan
selisihnya — laba atau rugi tahun itu — ke akun laba ditahan.

**Pelaku.** Accounting Manager menyusun; penyetuju yang berbeda menyetujui; pengesahan memakai
jalur jurnal yang sudah ada.

**Pemicu.** Seluruh periode tahun buku itu sudah ditutup, dan pembukuan tahun itu dianggap final.

### Langkah normal

1. **Membuka pratinjau tutup tahun.** Accounting Manager mengisi badan hukum dan tahun buku.
   Sistem menampilkan saldo tiap akun pendapatan dan beban, totalnya, laba atau ruginya, dan
   seluruh baris jurnal yang akan dibuat. **Tidak ada apa pun yang dibuat pada langkah ini.**
2. **Sistem memeriksa seluruh periode tahun itu sudah tertutup.** Yang menahan hanya periode
   berstatus `Open` dan `PendingClosingApproval`. `SoftClosed` sudah cukup.
3. **Sistem menghitung saldo pendapatan dan beban**, hanya dari baris jurnal yang jurnalnya sudah
   disahkan, dan hanya dari periode tahun buku itu.
4. **Accounting Manager memeriksa angkanya.** Bila janggal, jurnalnya diperiksa dulu, dan tidak
   ada yang perlu dibatalkan karena belum ada yang dibuat.
5. **Menekan Susun Jurnal Penutup.** Sistem menghitung **ulang** — angka dari pratinjau tidak
   dipercaya, karena jurnal Desember dapat saja disahkan di antara pratinjau dibuka dan tombol
   ditekan — lalu membuat satu jurnal `Draft` berjenis `JT` bertanggal akhir tahun buku.
6. **Jurnal penutup diajukan, disetujui orang kedua, lalu disahkan** lewat endpoint jurnal yang
   sudah ada. Sesudah disahkan, saldo seluruh akun pendapatan dan beban tahun itu menjadi nol,
   dan labanya berada di akun laba ditahan.

### Contoh berangka, mengikuti [`flowcharts/04-tutup-tahun.md`](../../../flowcharts/04-tutup-tahun.md)

Badan hukum `LE-MMC-001`, tahun buku 2026:

| Kelompok | Akun | Saldo |
| --- | --- | ---: |
| Pendapatan | `4-1001 Pendapatan Rawat Jalan` | Rp 800.000.000 |
| Pendapatan | `4-1002 Pendapatan Rawat Inap` | Rp 500.000.000 |
| Beban | `5-1001 Beban Obat` | Rp 300.000.000 |
| Beban | `5-2001 Beban Gaji` | Rp 600.000.000 |

Laba 2026 = Rp 1.300.000.000 − Rp 900.000.000 = **Rp 400.000.000**.

Jurnal penutup yang disusun sistem:

| Baris | Akun | Debit | Kredit |
| ---: | --- | ---: | ---: |
| 1 | `4-1001 Pendapatan Rawat Jalan` | Rp 800.000.000 | |
| 2 | `4-1002 Pendapatan Rawat Inap` | Rp 500.000.000 | |
| 3 | `5-1001 Beban Obat` | | Rp 300.000.000 |
| 4 | `5-2001 Beban Gaji` | | Rp 600.000.000 |
| 5 | `3-3001 Laba Ditahan` | | Rp 400.000.000 |
| | **Total** | **Rp 1.300.000.000** | **Rp 1.300.000.000** |

Sesudah jurnal ini disahkan, keempat akun pendapatan dan beban bersaldo **nol**, dan `3-3001`
bersaldo kredit Rp 400.000.000. Kas tidak tersentuh sama sekali — tutup tahun memindahkan laba,
bukan uang.

### Rumusnya, tanpa memandang jenis akun

Saldo satu baris dihitung sebagai debit dikurangi kredit, sehingga positif berarti condong debit.
Untuk menolkannya, sistem mencatat lawannya:

- saldo **positif** (lazimnya beban) ⇒ baris **kredit** sebesar saldo itu;
- saldo **negatif** (lazimnya pendapatan) ⇒ baris **debit** sebesar nilai mutlaknya.

Rumus ini sengaja tidak memandang jenis akun maupun saldo normalnya. Akun pendapatan yang
kebetulan bersaldo debit — misalnya karena retur melebihi pendapatan — tetap menjadi nol dengan
rumus yang sama.

Baris laba ditahan menutup selisihnya: laba masuk **kredit**, rugi masuk **debit**. Bila
pendapatan dan beban kebetulan sama persis, baris laba ditahan **tidak dibuat sama sekali** —
jurnalnya sudah seimbang tanpa baris itu, dan baris bernilai nol pada kedua sisi akan ditolak
validasi baris jurnal.

### Kenapa baris beban dipecah per unit biaya

`ACC-DEC-019` mewajibkan setiap baris jurnal berakun `Expense` menyebutkan unit biaya, dan syarat
ke-7 `ACC-STATE-0.1` bagian 1.3 memeriksanya **ulang** saat pengajuan dan pengesahan. Satu baris
gabungan per akun beban karena itu akan tertahan di sana. Menebak satu unit biaya untuk mengisinya
justru lebih buruk: jurnalnya tetap seimbang, akunnya tetap nol, tetapi laporan beban per unit
biaya menjadi salah tanpa satu pun tanda.

Karena itu saldo dihitung per pasangan **akun dan unit biaya**. Contohnya, `5-1001 Beban Obat`
yang dipakai dua unit biaya — Rawat Jalan Rp 120.000.000 dan Rawat Inap Rp 80.000.000 — menjadi
**dua** baris jurnal penutup, masing-masing membawa unit biayanya sendiri. Jumlah keduanya tetap
Rp 200.000.000, sehingga saldo akunnya tetap menjadi nol.

### Jalur tidak normal

| Keadaan | Jawaban | Yang perlu dilakukan pengguna |
| --- | --- | --- |
| Ada periode tahun itu masih `Open` atau `PendingClosingApproval` | `409` beserta **daftar nama periodenya** | Tutup periode-periode itu lebih dahulu |
| Tahun buku itu belum punya periode sama sekali | `422` | Minta administrator membangkitkan periode tahun buku itu |
| Periode terakhir tahun itu sudah **tutup permanen** | `422` beserta nama periodenya | Periode `Closed` tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri |
| Akun laba ditahan belum ditetapkan | `422` | Tetapkan akunnya lewat Pengaturan Akuntansi |
| Akun laba ditahan berubah menjadi tidak layak sesudah ditetapkan | `422` beserta sebabnya | Perbaiki pengaturan akuntansi |
| Seluruh akun pendapatan dan beban bersaldo nol | `422` | Tidak ada yang perlu ditutup |
| Jurnal penutup tahun itu sudah pernah disusun | `409` beserta **nomor jurnalnya** | Hapus draftnya, atau balik jurnal yang sudah sah |
| Badan hukum utama tidak tepat satu | `409` | Penjaga `ACC-DEC-041`, sama seperti seluruh service Accounting |

### Bila ada jurnal Desember yang terlewat

Tidak ada mekanisme "buka kembali tahun buku". Jurnal penutup adalah jurnal biasa, sehingga
koreksinya memakai pembalikan jurnal yang sudah ada (`ACC-DEC-029`): balik jurnal penutupnya,
buka kembali periode Desember beserta alasan tertulis, masukkan jurnal yang terlewat, tutup lagi,
lalu susun ulang jurnal penutupnya. **Diratifikasi `ACC-DEC-068`, 10 September 2026**, menutup
`DEC-ACC-P2-006`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak dan blueprint:

- `roadmap/backend-roadmap-phase2.md` kartu `BE-ACC-P2-010`, `006`, `009`
- `contracts/api-contract.md` grup Year End Closing dan Configuration
- `contracts/validation-matrix.md` Phase 2 bagian 5
- `contracts/state-transition-matrix.md` bagian 2 dan 4
- `contracts/permission-audit-matrix.md` baris tutup tahun
- `flowcharts/04-tutup-tahun.md`
- `02-backend-architecture.md` bagian 15.3, 16, 17
- `UTANG-TEKNIS.md`

Source:

- `AccAccountingConfigurationService.cs`, `AccountingConfigurationController.cs`, `AccountingConfigurationDtos.cs`, `AccAccountingConfiguration.cs`
- `AccChartOfAccountService.cs` (`HitungSaldoAsync`), `AccChartOfAccount.cs`
- `AccJournalService.cs` seluruhnya — `CreateAsync`, `SubmitAsync`, `ApproveAsync`, `PostAsync`, `ReverseAsync`, `SiapkanAsync`, `SusunBarisAsync`, `PeriksaSembilanSyaratAsync`, `AlokasikanNomorJurnalAsync`
- `AccJournal.cs`, `AccJournalLine.cs`, `AccJournalType.cs`, `JournalDtos.cs`, `JournalController.cs`
- `AccAccountingPeriodService.cs`, `AccAccountingPeriod.cs`, `AccountingPeriodStatus.cs`, `AccountingPeriodController.cs`
- `AccountingLegalEntityGuard.cs`, `AccountingServiceResult.cs`, `MstCostCenter.cs`
- `AccountingMasterDataSeeder.cs`, `Program.cs`
- `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/TestDatabase.cs` dan uji Accounting yang sudah ada

Tata kelola:

- `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`, `role-access-rules`, `transaction-endpoint-standard`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccYearEndClosingService.cs` | **Baru.** Perhitungan saldo tahun buku, penyusunan rencana jurnal penutup, dan pembuatan jurnalnya. Memuat `HitungSaldoTahunAsync` sebagai `public static` |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/DTOs/YearEndClosingDtos.cs` | **Baru.** `YearEndClosingPreviewResponse`, `YearEndClosingPreviewLineResponse`, `GenerateYearEndClosingRequest`, `YearEndClosingExistingJournal` |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Controllers/YearEndClosingController.cs` | **Baru.** Dua endpoint beserta metadata hak aksesnya |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccAccountingPeriodService.cs` | **Diubah.** `AlasanPenolakanJenisJurnal` menerima `JT` pada periode `SoftClosed`; cabang `PendingClosingApproval` ditambahkan; `JenisJurnalYangDiterima` diselaraskan; `NamaPeriode` dibuka menjadi `public static` |
| `Program.cs` | **Diubah.** Satu baris `AddScoped<AccYearEndClosingService>()`, bersebelahan dengan delapan registrasi Accounting yang sudah ada |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/AccYearEndClosingTests.cs` | **Baru.** 20 uji, seluruh acceptance criteria |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru** pada grup yang sepenuhnya baru, `api/v1/corporate/accounting/year-end-closing`. Nol endpoint yang sudah ada berubah bentuk, nama, atau payload-nya. Satu perubahan **perilaku** pada jalur jurnal yang sudah ada: jurnal berjenis `JT` kini diterima periode `SoftClosed` — lihat bagian 7 |
| Database | **Nol migration, nol perubahan entity, nol perubahan `ApplicationDbContext`, nol `DbSet` baru, snapshot tidak disentuh.** Task ini murni membaca tabel yang sudah ada dan menulis lewat `AccJournalService` yang sudah ada. **Tidak ada perintah database yang dijalankan** terhadap basis data mana pun |
| Keamanan/Auth | Dua hak akses baru, `YearEndClosing : Read` dan `YearEndClosing : Generate`, sesuai `ACC-PERMISSION-0.4`. Keduanya ditegakkan `[AccessPermission]` dan tampil di layar Akses Role lewat `[AccessAction]`. **Nol pemeriksaan peran yang dihardcode.** Nilai laba tahun berjalan sengaja **tidak** masuk payload maupun pesan log (`ACC-PERMISSION-0.3` bagian 4) |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Year End Closing

Base URL `api/v1/corporate/accounting/year-end-closing`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/preview` | Menampilkan saldo tiap akun pendapatan dan beban satu tahun buku, laba atau ruginya, dan seluruh baris jurnal penutup yang akan dibuat. **Tidak membuat apa pun** | `YearEndClosing : Read` |
| `POST` | `/generate` | Menyusun jurnal penutup tahun berstatus `Draft` berjenis `JT` | `YearEndClosing : Generate` |

`GET /preview` menerima `legalEntityId` dan `fiscalYear` pada query. `POST /generate` menerima
`GenerateYearEndClosingRequest` berisi `legalEntityId`, `fiscalYear`, dan `description` yang tidak
wajib.

**Pengesahan dan pembalikannya memakai endpoint yang sudah ada**, yaitu
`POST /api/v1/corporate/accounting/journals/{id}/submit`, `/approve`, `/post`, dan `/reverse`.
Tidak ada jalur pengesahan khusus tutup tahun, karena jurnal penutup adalah jurnal biasa.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false --no-incremental` | Berhasil | `PASS` | `0 Error(s)`, `200 Warning(s)` — **nol warning berasal dari keenam berkas task ini**, diperiksa dengan menyaring keluaran build menurut nama berkasnya |
| `dotnet test ... UnitTests.Sqlite --filter "FullyQualifiedName~AccYearEndClosingTests"` | Berhasil | `PASS` | `Failed: 0, Passed: 20, Skipped: 0, Total: 20` |
| `dotnet test ... UnitTests.Sqlite` (seluruh project) | Berhasil | `PASS` | `Failed: 0, Passed: 558, Skipped: 0, Total: 558` — **nol regresi**; 538 uji yang sudah ada seluruhnya tetap hijau |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Berhasil | `PASS` | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS`, 6 berkas dievaluasi |
| Test integrasi PostgreSQL memakai contoh berangka flowchart | Tidak dijalankan | `NOT RUN` | Lihat "Tidak dijalankan" di bawah |

Uji manual: `NOT FEASIBLE` — menjalankan endpoint sungguhan menuntut aplikasi berjalan terhadap
basis data yang migration-nya sudah diterapkan, dan penerapan migration adalah wewenang terpisah
yang sengaja dihentikan pada task ini.

**Tidak dijalankan:**

- **Test integrasi PostgreSQL.** `Tests/QuilvianSystemBackend.IntegrationTests.Postgres`
  menjalankan **migration** sendiri sebelum uji pertama, terhadap basis data yang ditunjuk
  `appsettings.Development.json`. Task ini dihentikan sebelum migration atas instruksi pemilik,
  dan penerapan migration ke basis data mana pun adalah wewenang terpisah menurut
  `AGENTS.md` bagian *Keselamatan Database*. Ini pembatasan yang sama dengan yang tercatat pada
  `BE-ACC-P2-006` dan `BE-ACC-P2-013`.
- **Perintah database apa pun.** Nol `SELECT`, `INSERT`, `UPDATE`, `DELETE`, `dotnet ef`, dan DDL
  terhadap basis data mana pun.

### Apa yang sebenarnya dibuktikan uji SQLite

Yang membuat 20 uji ini berarti bukan jumlahnya, melainkan cara acceptance (4) dibuktikan. Uji
tidak berhenti pada "debit sama dengan kredit" — justru itu yang **selalu** benar, bahkan pada
jurnal penutup yang angkanya salah. Alih-alih, uji menempuh seluruh daur hidup: menyusun jurnal
penutup, mengajukannya, menyetujuinya dengan pelaku yang berbeda, **mengesahkannya**, lalu
memeriksa saldo tiap akun satu per satu memakai `AccChartOfAccountService.HitungSaldoAsync` —
penghitung saldo milik buku besar, bukan penghitung khusus uji. Bila tutup tahun memakai
perhitungan yang diam-diam berbeda dari buku besar, perbedaan itu muncul di sana.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) **Pratinjau tidak membuat apa pun** — dipanggil sepuluh kali, nol jurnal terbentuk | **Terpenuhi** | `Pratinjau_DipanggilSepuluhKali_NolJurnalTerbentuk`. Selain jumlah jurnal, uji memeriksa `AccNumberSeries` tetap **kosong** — pratinjau yang diam-diam mengalokasikan nomor tidak meninggalkan jurnal apa pun tetapi menghabiskan nomor, dan jurnal berikutnya melompat tanpa penjelasan |
| (2) Ada periode belum tertutup ⇒ `409` beserta daftar periodenya | **Terpenuhi** | `AdaPeriodeBelumTertutup_Ditolak409`, dua kali untuk `Open` dan `PendingClosingApproval`; pesan memuat "Februari 2026" dan "November 2026". Berlaku pada pratinjau maupun penyusunan |
| (3) Akun laba ditahan belum ditetapkan ⇒ `422` | **Terpenuhi** | `AkunLabaDitahanBelumDitetapkan_Ditolak422`, ditambah `AkunLabaDitahanTidakLagiLayak_Ditolak422` untuk tiga cara akunnya berubah sesudah ditetapkan |
| (4) Jurnal penutup **seimbang** dan **menolkan seluruh akun `Revenue` serta `Expense`** | **Terpenuhi** | `SesudahDisahkan_SaldoTiapAkunPendapatanDanBebanMenjadiNol` — keempat akun diperiksa satu per satu **sesudah jurnalnya disahkan**, memakai `HitungSaldoAsync`. Diperkuat `AkunBebanDuaUnitBiaya_TetapNolDanTerpisahPerUnitBiaya`, `JurnalTahunBerikutnya_TidakIkutDitutup`, dan `TahunRugi_LabaDitahanDidebit` |
| (5) Penyusunan kedua untuk tahun yang sama ⇒ `409` | **Terpenuhi** | `PenyusunanKedua_Ditolak409`; pesan memuat nomor jurnal yang sudah ada, dan jumlah jurnal penutup tetap satu. `TahunBukuLain_TidakIkutTertahan` membuktikan penjaganya tidak kebablasan |
| (6) Jurnal penutup dapat dibalik lewat jalur pembalikan yang sudah ada | **Terpenuhi** | `JurnalPenutup_DapatDibalikLewatJalurYangSudahAda` — `ReverseAsync` `FullReversal`, disetujui, disahkan, lalu saldo keempat akun kembali ke angka sebelum ditutup dan laba ditahan kembali nol |

### Definition of Done

| Butir | Status |
| --- | --- |
| Dua endpoint berjalan | **Terpenuhi** — `GET /preview` dan `POST /generate`, keduanya membawa `[AccessAction]` dan `[AccessPermission]` yang diuji lewat refleksi |
| Test hijau | **Terpenuhi untuk `UnitTests.Sqlite`** — 20 uji baru hijau, 558 uji project hijau, nol regresi |
| Test integrasi PostgreSQL | **Belum terpenuhi** — `NOT RUN`, alasannya pada bagian 5 |
| Laporan task tertulis | **Terpenuhi** — berkas ini |
| Nol migration | **Terpenuhi** — nol entity baru, nol perubahan snapshot, nol perintah database |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build `Release` seluruh solution menghasilkan 200 warning, **seluruhnya sudah ada sebelumnya** dan tidak satu pun berasal dari keenam berkas task ini |
| Masalah yang diketahui | **Menutup Desember secara permanen sebelum tutup tahun dijalankan akan mengunci tutup tahun selamanya** — periode `Closed` tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri. Urutan yang benar: tutup sementara seluruh bulan → jalankan tutup tahun → baru tutup permanen. Sudah dicatat pada flowchart dan state matrix. Selain itu, sistem tidak dapat membedakan `Laba Ditahan` dari `Modal Disetor` — keduanya akun ekuitas yang menerima transaksi. Batas ini diwarisi dari `BE-ACC-P2-009` dan memang diserahkan ke pemilik proses oleh `ACC-DEC-054` |
| Risiko tersisa | **Penjaga jurnal penutup ganda belum dijamin database.** Pemeriksaan dan pembuatan jurnal berada dalam satu transaction, sehingga dua permintaan berurutan pasti tertangkap. Dua permintaan **bersamaan** pada dua koneksi berbeda masih dapat lolos keduanya. Penutupnya adalah unique constraint database, dan itu menuntut migration — di luar wewenang task ini. Setara dengan yang dituntut `BE-ACC-P2-008` acceptance (1) bagi jurnal berulang |
| Perubahan sampingan | `NONE`. Empat berkas dokumentasi yang tampak pada `git status` — `UTANG-TEKNIS.md` dan tiga laporan task — sudah berubah **sebelum** task ini dimulai dan tidak disentuh sama sekali |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | `FE-ACC-P2-*` layar tutup tahun; test integrasi PostgreSQL ketika migration sudah diterapkan pemilik; unique constraint penjaga jurnal penutup ganda bila diputuskan perlu |

### Delta kontrak yang perlu diratifikasi

| Delta | Isi | Kenapa |
| --- | --- | --- |
| **`JT` diterima periode `SoftClosed`** | `AccAccountingPeriodService.AlasanPenolakanJenisJurnal` sebelumnya hanya menerima `JP` dan `JB` pada periode tutup sementara | **Tanpa ini seluruh task mustahil berjalan.** Tutup tahun baru boleh disusun setelah seluruh periode tahun itu tertutup, sementara tanggal akuntansi jurnal penutup wajib berada **di dalam** tahun yang ditutup — yaitu tepat pada periode yang barusan ditutup. Membiarkan `JT` ditolak membuat jurnal penutup lahir sebagai draft yang **tidak akan pernah dapat diajukan maupun disahkan**, karena syarat ke-9 memeriksa aturan yang sama saat pengajuan dan pengesahan. Ini kesimpulan yang dituntut kontrak yang sudah disetujui, bukan kebijakan baru. **Diratifikasi `ACC-DEC-067`, 10 September 2026**; `ACC-STATE` naik `0.2` → `0.3`, dan bagian 2.2 state matrix kini memuat tabel penerimaan `JT` per status periode |
| **Controller tersendiri, bukan menumpang `AccountingPeriodController`** | `02-backend-architecture.md` bagian 16 menyebut endpoint tutup tahun menumpang `AccountingPeriodController` | `ACC-PERMISSION-0.4` menetapkan hak aksesnya `YearEndClosing : Read` dan `YearEndClosing : Generate`, sementara argumen pertama `[AccessPermission]` **wajib** sama persis dengan `ControllerName` pada `[AccessController]` — bila menyimpang hasilnya `403` permanen yang tidak dapat diperbaiki dari layar Akses Role. Menumpang akan memaksa hak aksesnya bernama `AccountingPeriod`, menyatukan kewenangan menutup tahun dengan kewenangan mengelola periode. `contracts/api-contract.md` juga sudah menetapkan base URL dan `[Tags(...)]` tersendiri, jadi yang dipilih adalah bacaan kontrak API |
| **Penamaan DTO `...Response`, bukan `...Dto`** | `api-contract.md` menyebut `YearEndClosingPreviewDto` dan `YearEndClosingPreviewLineDto` | Mengikuti konvensi source yang berlaku dan preseden `BE-ACC-P2-009`, yang menamai `AccountingConfigurationResponse` walaupun kontraknya menulis `AccountingConfigurationDto` |
| **Penolakan ganda tidak berlaku pada pratinjau** | Pratinjau tetap menampilkan angka walaupun jurnal penutup tahun itu sudah tersusun | `flowcharts/04-tutup-tahun.md` menempatkan aturan ganda pada langkah 5 — langkah **menyusun** — bukan langkah memeriksa angka. Melihat kembali angka tahun yang sudah ditutup adalah kebutuhan wajar, dan menolaknya tidak melindungi apa pun |
| **Dua penolakan `422` di luar tulisan kartu** | Periode terakhir sudah tutup permanen; akun laba ditahan berubah menjadi tidak layak sesudah ditetapkan | Keduanya tetap akan ditolak tanpa tambahan ini, tetapi jauh di dalam validasi baris jurnal, dengan kalimat "Baris ke-5" yang tidak menyebut periode maupun pengaturan akuntansi sama sekali |

### Perbaikan di luar cakupan yang ikut dikerjakan

| Temuan | Perbaikan |
| --- | --- |
| `AlasanPenolakanJenisJurnal` menjawab **"Status periode … tidak dikenali"** untuk periode `PendingClosingApproval` | Statusnya dikenal betul — periodenya memang sedang menunggu persetujuan. Penolakannya sudah benar sejak semula; hanya kalimatnya yang menyesatkan. Kini menjawab kalimat yang menyebut keadaan sebenarnya |
| `JenisJurnalYangDiterima` tidak pernah menyebut `JT` | Daftar ini dibaca layar, sementara aturan penolakannya menerima kode apa pun pada periode `Open`. Sejak `BE-ACC-P2-003` menambahkan `JT`, layar menyembunyikan jenis yang backend sebenarnya terima. Kini kedua sumbernya selaras |

### Status Git

```text
 M Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccAccountingPeriodService.cs
 M Program.cs
 M docs/module-blueprints/accounting/UTANG-TEKNIS.md
 M docs/module-blueprints/accounting/task/report/backend/be-acc-p2-003-entity-pengaturan-akuntansi-dan-jenis-jurnal-jt.md
 M docs/module-blueprints/accounting/task/report/backend/be-acc-p2-009-endpoint-pengaturan-akuntansi.md
 M docs/module-blueprints/accounting/task/report/backend/be-acc-p2-013-saldo-control-account-dari-buku-besar.md
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Controllers/YearEndClosingController.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/DTOs/YearEndClosingDtos.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccYearEndClosingService.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/AccYearEndClosingTests.cs
```

Empat berkas dokumentasi bertanda `M` adalah pekerjaan pemilik yang sudah ada sebelum task ini
dimulai; nol di antaranya disentuh. Berkas laporan ini sendiri belum tampak di atas karena
`git status` di atas diambil sebelum laporan ditulis. Nol stage, nol commit, nol push.
