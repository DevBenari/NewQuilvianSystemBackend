# Laporan Perubahan Frontend — `PAY-FE-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-13` |
| Judul | Penanda status bayar gaji dan peringatan pada layar potongan piutang, riwayat pembayaran, dan detail run; pemakaian `confirmationSource` pada konfirmasi manual |
| Slice | Payroll mandiri — potongan piutang dan pembayaran |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 8.1 butir 2 dan 7 (keputusan pemilik 11 November 2026); permintaan pemilik 11 November 2026 ("boleh kerjakan PAY-BE-11 dan PAY-FE-13") |
| Contract version | Field tambahan dari `PAY-BE-11` (laporan `task/report/backend/PAY-BE-11.md`): `salaryPaymentStatus`, `salaryPaymentNumber`, `confirmedPaymentPendingCount`, `confirmedPaymentFailedCount`, `confirmedDeductionPaymentPendingCount`, `confirmedDeductionPaymentFailedCount`, `confirmationSource`, dan pemeriksaan `AR_DEDUCTION_PAYMENT`. Dibaca dari source backend, bukan ditebak |
| Dependency | `PAY-BE-11` 🟡 — source ditulis dan lolos pemeriksaan statis, **`dotnet build` dan runtime menunggu pemilik**. Frontend dibuat toleran terhadap backend lama yang belum membawa field baru |
| Klasifikasi | `HEAVY` menurut jumlah berkas (9 diubah, 2 test), `LOW RISK` menurut isi: data, kolom, dan teks pada komponen yang sudah ada |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; di repository backend hanya laporan ini dan baris register |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `94de48311` (pemilik sudah meng-commit pekerjaan `PAY-FE-11` dan `PAY-FE-12`; perubahan task ini belum di-commit) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Selesai secara statis; menunggu pemilik menjalankan build backend dan frontend lalu melihat layar (bagian 6).** 16 unit test baru PASS, suite penuh, resolver impor, dan ESLint sama dengan baseline, kolom dan peringatan dirender uji. `npm run build`/`dev` oleh agen tidak dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Pemilik memutuskan potongan cicilan piutang **tetap diakui saat run disahkan**, dengan satu tambahan: petugas harus bisa melihat apakah gaji penerimanya benar-benar sudah dibayar. Sebelumnya tidak ada tanda apa pun: daftar potongan hanya menampilkan status potongan, dan bila run disahkan lalu pembayaran gagal total, potongan sudah tercatat di Finance tetapi layar tidak menunjukkannya. Selain itu, detail run masih menulis "Berhasil Dibayar hanya berasal dari konfirmasi hasil bank oleh Finance", padahal konfirmasi manual (`PAY-FE-12`) sudah ada.

## 2. Proses bisnis dari sisi pengguna

- **Cicilan Piutang Karyawan.** Pada riwayat potongan dan pada hasil rekonsiliasi periode ada kolom baru **Status Bayar Gaji**: Lunas, Menunggu Bayar, Belum Dikirim, Gagal, Dibalik, atau Tanpa Pembayaran, dengan nomor instruksi sebagai keterangan kecil bila ada. Di atas kartu ringkasan muncul dua kartu baru ("Terkonfirmasi, Gaji Belum Dibayar" dan "Terkonfirmasi, Gaji Gagal/Dibalik") dan peringatan bila jumlahnya lebih dari nol.
- **Riwayat Pembayaran dan detail Payroll Run.** Bila ada potongan yang sudah dikonfirmasi tetapi gaji penerimanya belum lunas atau gagal, muncul peringatan dengan jumlahnya. Teks yang sama di kedua layar dan di Cicilan Piutang (satu pembangun).
- **Rekonsiliasi Keuangan.** Pemeriksaan baru dari backend ("Potongan cicilan terkonfirmasi hanya untuk penerima yang gajinya sudah dibayar") tampil otomatis dengan badge Sesuai, Informasi, atau Ada Selisih.
- **Konfirmasi manual** kini mengirim `confirmationSource: "MANUAL"` sehingga backend mencatat sumbernya.

Yang **tidak** ada: tombol atau saran pembalikan otomatis, dan pengubahan titik pengakuan potongan. Peringatan hanya informasi; tindak lanjutnya (coba ulang instruksi atau konfirmasi manual) memakai alur yang sudah ada.

**Contoh.** Run `PR-2026-11` disahkan: potongan Rp 500.000 untuk Ani dan Budi tercatat di Finance. Sebelum dikirim, kolom keduanya menampilkan *Belum Dikirim*. Setelah dikirim: *Menunggu Bayar*. Ani dikonfirmasi manual: *Lunas*. Instruksi Budi gagal: *Gagal*, peringatan merah "1 potongan ... gagal atau dibalik" muncul di Riwayat Pembayaran dan detail run, dan Rekonsiliasi Keuangan menandai pemeriksaannya *Ada Selisih*. Setelah Budi dikirim ulang dan dikonfirmasi, semuanya *Lunas* dan peringatan hilang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PAY-BE-11.md` dan DTO backend (`PayrollEmployeeDeductionDtos.cs`, `PayrollRunDtos.cs`, `PayrollPaymentDtos.cs`, `PayrollSalaryPaymentStatuses.cs`, `PayrollConfirmationSources.cs`), `payroll-deduction-table-columns.jsx`, `payroll-employee-deduction-view.jsx`, `payroll-payment-history-view.jsx`, `payroll-run-detail-view.jsx`, `payroll-report-table-columns.jsx` dan `payroll-reconciliation-view.jsx` (verifikasi), `status-badge.jsx`, normalizer dan konstanta Payroll.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-constants.js` | `SALARY_PAYMENT_STATUSES`, `PAYROLL_SALARY_PAYMENT_BADGE_CONFIG` (enam kode), `SALARY_PAYMENT_NOTICE_TEXT`, `MANUAL_CONFIRMATION_SOURCE` |
| `src/utils/payroll/payroll-payment-utils.js` | `isSalaryPaymentPending`, `isSalaryPaymentFailed`, `buildConfirmedDeductionPaymentNotices`; hitungan pada ringkasan daftar pembayaran; `buildAcknowledgeRequest` membawa sumber; hasil ACK membawa `confirmationSource` |
| `src/utils/payroll/payroll-deduction-utils.js` | Status dan nomor instruksi pada item; dua hitungan dan status per item pada rekonsiliasi |
| `src/utils/payroll/payroll-run-utils.js` | Dua hitungan pada ringkasan pembayaran detail run |
| `src/lib/hooks/payroll/use-payroll-payment-history.js` | Nilai awal dua hitungan; konfirmasi manual mengirim `MANUAL_CONFIRMATION_SOURCE` |
| `src/components/view/payroll/deduction/payroll-deduction-table-columns.jsx` | Kolom "Status Bayar Gaji" pada riwayat dan pada rekonsiliasi |
| `src/components/view/payroll/deduction/payroll-employee-deduction-view.jsx` | Dua kartu ringkasan dan peringatan pada ringkasan dan pada hasil rekonsiliasi |
| `src/components/view/payroll/payment/payroll-payment-history-view.jsx` | Peringatan di bawah pesan "belum berarti dibayar" |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | Peringatan pada bagian "Pembayaran ke Finance"; kalimat catatan yang keliru diganti `PAYROLL_MESSAGES.paymentNotPaid` |
| `tests/unit/payroll-manual-payment-confirmation.test.mjs` | Test bentuk request diperluas (sumber hanya bila diisi); satu asersi sumber untuk hook |
| `tests/unit/payroll-salary-payment-status.test.mjs` | **Baru** — 16 test |

Tidak ada berkas CSS yang dibuat atau diubah, dan tidak ada komponen baru.

### 3.3 Aturan tampilan

| Kasus | Tampilan |
| --- | --- |
| Status bayar gaji ada | Badge berlabel (`Lunas`, `Menunggu Bayar`, `Belum Dikirim`, `Gagal`, `Dibalik`, `Tanpa Pembayaran`) memakai kelas status dasar yang sudah ada (aktif, peringatan, tertunda, bahaya, bahaya, tidak aktif), dan "Instruksi PAY-..." bila ada nomor |
| Status kosong (backend lama, atau angsuran belum masuk run) | Tanda hubung, tanpa badge |
| Kode status yang belum dikenal frontend | Badge menampilkan kodenya dalam huruf kecil (perilaku `StatusBadge`); tidak galat |
| Hitungan ringkasan rekonsiliasi `null` (backend lama) | Kartu tidak ditampilkan |
| Hitungan ringkasan pembayaran absen (backend lama) | Dianggap nol; peringatan tidak ditampilkan |
| Hitungan lebih dari nol | Peringatan "gagal atau dibalik" (merah, lebih dulu) dan "belum dibayar" (kuning); jumlah dari backend apa adanya, tidak dihitung ulang |
| Detail run sebelum instruksi dikirim | `paymentSummary` kosong dari backend, jadi bagian dan peringatan tidak ada |

### 3.4 Kepatuhan arsitektur frontend

Normalizer dan pembangun peringatan berupa fungsi murni di `src/utils/payroll/`, teks dan badge di `src/lib/constants/payroll/`, view hanya memanggil dan merender. Tidak ada Axios baru, hook baru, atau slice baru.

**UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kolom tabel "Status Bayar Gaji" | `DataTable` (definisi kolom `render`) | Kolom lain pada berkas yang sama memakai pola yang sama | REUSE | — |
| Penanda status | `StatusBadge` dengan config data baru | Dipakai kolom status lain pada berkas yang sama; config berisi data, bukan komponen | REUSE | — |
| Dua kartu ringkasan | `SummaryCards` (`summary-grid`) | Sudah dipakai pada layar yang sama; hanya item ditambah | REUSE | — |
| Peringatan | `InformationAlert` | Dipakai pada ketiga layar | REUSE | — |
| Teks catatan "belum berarti dibayar" pada detail run | Elemen `<p className={styles.footnote}>` yang sudah ada | Hanya isi teks yang diganti | REUSE | — |

Tidak ada status selain `REUSE`, jadi tidak ada keputusan yang menunggu pemilik.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Backend lama (belum dibuild ulang) | Kolom tampil "-", kartu dan peringatan tidak tampil, tidak ada galat |
| Backend baru, semua lunas | Badge `Lunas`; tidak ada peringatan; kartu menunjukkan nol |
| Menunggu | Badge `Menunggu Bayar` atau `Belum Dikirim`; peringatan kuning |
| Gagal atau dibalik | Badge `Gagal` atau `Dibalik`; peringatan bernada bahaya lebih dulu; Rekonsiliasi Keuangan `Ada Selisih` |
| Memuat, kosong, gagal muat, tanpa hak akses | Tidak berubah (ditangani komponen yang sudah ada) |

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Field tambahan dibaca dari endpoint yang sudah dipakai layar:

| Method | Path | Tambahan |
| --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/employee-receivable-deductions` | `salaryPaymentStatus`, `salaryPaymentNumber` per item |
| `GET` / `POST` | `/employee-receivable-deductions/summary`, `/reconcile` | `confirmedPaymentPendingCount`, `confirmedPaymentFailedCount`, `salaryPaymentStatus` per item |
| `GET` | `/payroll-runs/{id}/payments`, `/payroll-runs/{id}` | `summary.` dan `paymentSummary.confirmedDeductionPayment{Pending,Failed}Count` |
| `GET` | `/reports/reconciliation` | Pemeriksaan `AR_DEDUCTION_PAYMENT` (dirender generik) |
| `POST` | `/payroll-runs/{id}/payments/acknowledge` | Permintaan membawa `confirmationSource: "MANUAL"`; respons membawa `confirmationSource` |

---

## 6. Verifikasi

Baseline diambil sebelum mengubah: suite 4756 test, 4709 lulus, 47 gagal (identik dengan hasil `PAY-FE-12`); resolver 61 impor tak terselesaikan.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --test` pada `payroll-salary-payment-status.test.mjs` dan `payroll-manual-payment-confirmation.test.mjs` | 44 test, 44 lulus (16 baru) | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | Sebelum: 4756 test, 4709 lulus, 47 gagal. Sesudah: 4772 test, 4725 lulus, 47 gagal. Daftar nama yang gagal **identik** (`diff` kosong) | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| Resolver impor statis (6852 berkas) | 61 tak terselesaikan sebelum dan sesudah, himpunan identik | `PASS` | `diff` hasil sebelum dan sesudah |
| `npx eslint` pada 12 berkas yang diubah (termasuk dua layar verifikasi) | Sebelum dan sesudah sama: 0 galat, 2 peringatan lama (`react-hooks/set-state-in-effect` pada hook); pesan identik | `PASS` (0 galat baru) | Keluaran ESLint format JSON |
| `npx eslint` pada test baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| Render sisi server (`react-dom/server`, pemuat sekali pakai di scratchpad; **bukan** bagian repo): kolom riwayat dan rekonsiliasi pada 8 nilai status (enam kode, kosong, kode tak dikenal), dan peringatan pada 4 kombinasi hitungan | Semua dirender tanpa galat; label, nomor instruksi, tanda hubung untuk kosong, urutan peringatan (gagal dulu), dan teks sesuai rancangan | `PASS` | Keluaran render |
| Penjagaan kontrak dengan DTO backend (dalam test baru) | Enam kode, tujuh nama field, `ConfirmationSource`, dan dua sumber sama dengan source backend | `PASS` (dilewati otomatis bila repository backend tidak ada) | Test "nama kode dan field sama dengan Backend" |
| Grep anti-regresi pada baris tambahan (tombol mentah, `.btn`, tipografi Bootstrap, warna literal, `!important`, `style={{}}`, `<table`) | Kosong; tidak ada berkas CSS berubah | `PASS` | Keluaran `grep`, `git status` |
| Stage | 0 berkas ter-stage | `PASS` | `git diff --cached` |
| Verifikasi layar Rekonsiliasi Keuangan | Kolom Detail sudah merender `details` (baris per penerima) dan badge `Sesuai`/`Informasi`/`Ada Selisih` sudah ada pada config; tidak perlu perubahan | `PASS` (telaah source) | `payroll-report-table-columns.jsx` baris 98 dan seterusnya; `PAYROLL_RECONCILIATION_BADGE_CONFIG` |
| `npm run build` / `npm run dev` | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | — |

MANUAL TEST: `NOT FEASIBLE` oleh agen — tidak ada server atau peramban yang boleh dijalankan, dan backend `PAY-BE-11` belum di-build.

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 4772 test, hasil identik dengan baseline untuk yang lama; 16 baru lulus.

**Langkah uji untuk pemilik** (setelah `dotnet build` backend dan `npm run build` atau `npm run dev` frontend):

| No | Langkah | Yang harus terlihat |
| ---: | --- | --- |
| a | Buka **Cicilan Piutang Karyawan**, pilih periode yang runnya sudah disahkan | Kolom "Status Bayar Gaji" pada tab riwayat; kartu "Terkonfirmasi, Gaji ..." pada ringkasan |
| b | Run disahkan tetapi belum dikirim ke Finance | Badge `Belum Dikirim`; peringatan kuning di Cicilan Piutang |
| c | Kirim ke Finance | `Menunggu Bayar` dengan nomor instruksi; peringatan kuning di Riwayat Pembayaran dan detail run |
| d | Konfirmasi manual satu instruksi (`PAY-FE-12`) | `Lunas`; jumlah peringatan berkurang; tab Network: kiriman membawa `confirmationSource: "MANUAL"` |
| e | Tandai satu instruksi Gagal | `Gagal`; peringatan **merah** di Riwayat Pembayaran dan detail run |
| f | Buka **Rekonsiliasi Keuangan** untuk run itu | Pemeriksaan "Potongan cicilan terkonfirmasi hanya untuk penerima yang gajinya sudah dibayar": `Informasi` saat menunggu, `Ada Selisih` saat gagal, `Sesuai` saat semua lunas, dengan baris per penerima |
| g | Pada tab Rekonsiliasi Periode, jalankan **Rekonsiliasi Periode** | Hasil membawa kartu status bayar dan kolom "Status Bayar Gaji"; "Selisih Ditemukan" **tidak** berubah karena status bayar |
| h | Dengan backend lama (sebelum `dotnet build` ulang) buka ketiga layar | Tidak ada galat; kolom "-"; tidak ada kartu atau peringatan baru |
| i | Lihat kolom Status pada Riwayat Pembayaran untuk instruksi yang dikonfirmasi manual | Keterangan "Dikonfirmasi manual (bukan hasil bank)" |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom Status Bayar Gaji pada riwayat potongan dan rekonsiliasi periode, kosong tampil "-" | Terpenuhi (statis, render) | Bagian 3.3, 6 |
| Dua kartu ringkasan dan peringatan pada Cicilan Piutang, tanpa menyatakannya selisih | Terpenuhi (statis) | Bagian 3.2; `DiscrepancyCount` tidak berubah |
| Peringatan pada Riwayat Pembayaran dan detail run dari satu pembangun yang sama | Terpenuhi | Test penjagaan sumber; render |
| Kalimat yang keliru pada detail run diganti | Terpenuhi | Test "kalimat yang keliru sudah diganti" |
| Pemeriksaan `AR_DEDUCTION_PAYMENT` tampil benar pada Rekonsiliasi Keuangan | Terpenuhi menurut telaah source; belum diamati di layar | Bagian 6 |
| Konfirmasi manual mengirim `confirmationSource: "MANUAL"` | Terpenuhi (statis) | Test request; test sumber hook |
| Toleran terhadap backend lama | Terpenuhi (test dan render) | Test "Backend lama tanpa field baru" |
| Tidak ada tombol atau saran pembalikan otomatis; titik pengakuan tidak berubah | Terpenuhi | Test peringatan gagal; diff |
| Tidak ada komponen baru, CSS baru, atau regresi test, ESLint, dan impor | Terpenuhi | Tabel 3.4 dan bagian 6 |
| Terbukti pada build dan layar | **Belum** | Menunggu pemilik (langkah a sampai i) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bukti statis tidak menggantikan build. `PAY-BE-11` belum dikompilasi; bila nama field di respons nyata berbeda dari DTO yang dibaca, kolom akan kosong ("-") dan peringatan tidak tampil tanpa galat |
| Keputusan implementasi | (1) Hitungan ringkasan rekonsiliasi `null` bila absen supaya kartu tidak menampilkan "0" palsu pada backend lama; hitungan ringkasan pembayaran memakai nol karena peringatan memang tersembunyi pada nol. (2) Teks peringatan sengaja bernada informasi: potongan sah tercatat saat run disahkan; yang perlu ditindaklanjuti adalah gaji penerimanya. (3) Tiga layar memanggil pembangun yang sama alih-alih membuat komponen pembungkus, sesuai ketentuan "tanpa komponen baru" |
| Masalah yang diketahui | Peringatan pada detail run tidak muncul sebelum instruksi pembayaran dikirim karena `paymentSummary` kosong pada saat itu (perilaku backend). Sebelum dikirim, status `Belum Dikirim` tetap terlihat pada kolom Cicilan Piutang |
| Dependency backend | `PAY-BE-11` 🟡 (build dan runtime menunggu pemilik) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: `M` 10 berkas (9 source dan satu test), `??` 1 berkas test baru. Tidak ada stage atau commit. Pekerjaan `PAY-FE-11` dan `PAY-FE-12` sudah di-commit pemilik |
| Langkah berikutnya | Pemilik: `dotnet build` backend, `npm run build` frontend, dan langkah uji a sampai i; setelah itu laporan ini dan `PAY-BE-11` dapat naik ke ✅ |
