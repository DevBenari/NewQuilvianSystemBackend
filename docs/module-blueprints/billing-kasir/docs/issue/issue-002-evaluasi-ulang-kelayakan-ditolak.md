# ISSUE-BKC-002 — Tombol evaluasi ulang kelayakan pulang milik kasir ditolak basis data

```yaml
issue_id: ISSUE-BKC-002
module_id: billing-kasir
submodule: null  # blueprint berbentuk SINGLE; <blueprint-root> = folder modul
layar: "Surat ke Modul Konsumen — tombol Evaluasi Ulang kelayakan pemulangan rawat inap (BIL-SCR-41)"
sumber_laporan: "Temuan agent pada /design-business-module revisi 2.1, 14-10-2026; dikonfirmasi pemilik modul Billing sebagai perbaikan terpisah yang didahulukan. 1 butir, 0 lampiran"
tanggal_issue: "2026-10-14"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "d166cf4f (Yasmina)"
source_sha_frontend: "6e5edd6df (yasmina)"
rencana_perbaikan: ../plan-repair/plan-repair-002-evaluasi-ulang-kelayakan-ditolak.md
```

## 1. Ringkasan

Layar **Surat ke Modul Konsumen** punya tombol **Evaluasi Ulang** untuk memperbarui status
kelayakan pulang pasien rawat inap secara manual. Berdasarkan penelusuran source, tombol itu
**hampir pasti gagal setiap kali ditekan**: sistem menyimpan surat kelayakan baru dengan kode
alasan yang tidak diizinkan oleh aturan basis data, sehingga basis data menolaknya. Kasir hanya
melihat pesan umum "Terjadi kesalahan pada server". Cacat ini sudah ada sejak fitur dirilis pada
24 September 2026. Penerbitan kelayakan otomatis saat pembayaran lunas **tidak** terdampak. Satu
hal penting: temuan ini belum dibuktikan dengan menjalankan aplikasi, sehingga langkah verifikasi
pada bagian 7 perlu dijalankan lebih dulu. Ada juga dua temuan tambahan, salah satunya kesalahan
penamaan layar pada dokumen blueprint yang ditulis agent sendiri.

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | Tombol evaluasi ulang kelayakan milik kasir kemungkinan besar rusak: kode alasan `REEVALUATED_BY_CASHIER` ditolak constraint basis data. Pemilik memutuskan perbaikannya dikerjakan sebagai task terpisah yang didahulukan, setelah diverifikasi di lingkungan nyata | Tidak ada. Temuan berasal dari pembacaan source, bukan dari screenshot atau capture basis data |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-BKC-002-01` | 1 | Kode alasan evaluasi ulang kasir tidak terdaftar pada constraint basis data | `BUG` | Backend | Blocker | `SUDAH-VERIFIKASI` di source **dan** di basis data (P-01, 14-10-2026) | `FIX-BKC-002-01` |
| `ISS-BKC-002-T1` | — | Kegagalan basis data muncul ke kasir sebagai pesan umum tanpa petunjuk | `BUG` | Backend | Low | `SUDAH-VERIFIKASI` | Tidak direncanakan — lihat bagian 6 |
| `ISS-BKC-002-T2` | — | ID layar karangan pada dokumen blueprint bertabrakan dengan ruang nomor task | `CONTRACT_MISMATCH` | Dokumen blueprint | Medium | `SUDAH-VERIFIKASI` | `FIX-BKC-002-02` |

## 4. Rincian per temuan

### ISS-BKC-002-01 — Kode alasan evaluasi ulang kasir tidak terdaftar pada constraint basis data

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker — untuk alur evaluasi ulang manual saja |
| **Status bukti** | Rantai source `SUDAH-VERIFIKASI` pada `d166cf4f`. Constraint di basis data `SUDAH-VERIFIKASI` lewat P-01 pada 14-10-2026 — lihat bagian 7. Yang belum diuji hanya bunyi persis pesan di layar, karena mengujinya menuntut penulisan ke basis data bersama |
| **Perbaikan** | `FIX-BKC-002-01` |

**Apa yang terjadi.** Kasir membuka layar Surat ke Modul Konsumen, memilih surat kelayakan pulang
seorang pasien rawat inap, menekan Evaluasi Ulang, lalu mengonfirmasi. Alih-alih status kelayakan
yang terbaru, kasir menerima pemberitahuan merah "Evaluasi Gagal" dengan pesan "Terjadi kesalahan
pada server". Mengulang tidak menolong, karena setiap percobaan gagal dengan sebab yang sama.

**Kenapa terjadi.**

```text
Kasir menerima "Evaluasi Gagal"
  ← server mengembalikan 500 dari penangan galat global
    ← basis data menolak baris surat baru karena melanggar CHECK constraint
      ← surat ditulis dengan ReasonCode = 'REEVALUATED_BY_CASHIER'
        ← AKAR: daftar nilai CHECK ditulis tangan terpisah dari kelas konstanta,
                dan satu nilai terakhir tertinggal sejak hari pertama
```

Langkah demi langkah:

1. Endpoint evaluasi ulang mengirim kode alasan khusus kasir.
   `Areas/HealthServices/BillingManagement/Billing/Controllers/InpatientClearanceController.cs:116-118`

   ```csharp
   var handoff = await _clearanceService.EvaluateClearanceAsync(
       request.EncounterId,
       InpatientClearanceReasonCodes.ReevaluatedByCashier,
   ```

2. Service menulis kode itu apa adanya ke kolom `ReasonCode`, lalu menyimpan **tanpa syarat**.
   `Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs:174` dan
   `:189-190`

   ```csharp
   ReasonCode = reasonCode,
   // ...
   _dbContext.BilInpatientClearanceHandoffs.Add(handoff);
   await _dbContext.SaveChangesAsync(cancellationToken);
   ```

3. Kelas konstanta mendefinisikan **tujuh** kode alasan, termasuk `REEVALUATED_BY_CASHIER`.
   `Areas/HealthServices/BillingManagement/Billing/Models/BilInpatientClearanceHandoff.cs:93-99`

4. Constraint basis data hanya mengizinkan **enam**, tanpa `REEVALUATED_BY_CASHIER`.
   `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInpatientClearanceHandoffConfiguration.cs:22-24`

   ```csharp
   table.HasCheckConstraint(
       "CK_BilInpatientClearanceHandoff_ReasonCode",
       "\"ReasonCode\" IN ('INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED',
                         'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED')");
   ```

   Constraint yang sama tercatat pada migration
   `Migrations/20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.cs:80`, sehingga
   setiap basis data yang sudah menjalankan migration itu ikut memasangnya.

5. Controller hanya menangkap `KeyNotFoundException` dan `InvalidOperationException`
   (`InpatientClearanceController.cs:130-139`). Penolakan basis data datang sebagai
   `DbUpdateException`, sehingga lolos ke `Middlewares/GlobalExceptionMiddleware.cs:52-66`, yang
   membalas `500` dengan pesan "Terjadi kesalahan pada server."

6. Frontend menampilkan pesan itu pada pemberitahuan "Evaluasi Gagal".
   `QuilvianSystemFrontendDev/src/lib/hooks/health-services/billing-management/billing-consumer-handoffs/use-billing-consumer-handoffs.js:307-315`

**Kapan cacat ini lahir.** `git log -S` menunjukkan konstanta `REEVALUATED_BY_CASHIER`,
constraint `CK_BilInpatientClearanceHandoff_ReasonCode`, dan pemakaiannya di endpoint **masuk
bersamaan** pada commit `59910dc0` tanggal 24 September 2026. Ini bukan regresi yang datang
belakangan; tombol itu kemungkinan besar belum pernah berhasil sejak dirilis.

**Radius dampak — mengapa hanya satu jalur.** Ada empat jalur lain yang menulis surat kelayakan,
dan seluruhnya memakai kode yang **diterima** constraint:

| Jalur | Kode yang ditulis | Diterima? | Bukti |
| --- | --- | :---: | --- |
| Pelunasan invoice lewat finalisasi | `INVOICE_SETTLED` | Ya | `BillingFinalizationService.cs:665-667` |
| Pembayaran berhasil atau dibalik | `INVOICE_SETTLED` atau `PAYMENT_REVERSED` | Ya | `BillingSettlementService.cs:634-638` |
| Pembalikan alokasi | `PAYMENT_REVERSED` | Ya | `BillingSettlementService.cs:671-673` |
| Tagihan susulan setelah izin pulang | `LATE_CHARGE_POSTED` | Ya | `InpatientClearanceService.cs:279-281` |
| **Evaluasi ulang oleh kasir** | `REEVALUATED_BY_CASHIER` | **Tidak** | `InpatientClearanceController.cs:118` |

Ketiga jalur pertama sengaja memetakan kode resep ke kode rawat inap, sehingga kode resep yang
tidak dikenal constraint rawat inap — misalnya `INVOICE_WRITTEN_OFF` dan
`PAYER_COVERAGE_REVERSED` — tidak pernah sampai ke tabel ini.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Dokumen
`03-frontend-architecture.md` amendment revisi `1.5` (baris sekitar 1320) mencantumkan tindakan
**"Menjalankan Evaluasi Ulang"** pada matriks peran layar ini. Desain menghendaki tombol itu bekerja.

**Dampak nyata.** Ny. **Sari Mandasari** (nama samaran) melunasi sisa tagihan rawat inapnya lewat
transfer bank yang baru terkonfirmasi sore hari. Kasir ingin memperbarui status kelayakan pulangnya
supaya bangsal dapat memulangkan pasien. Tombol Evaluasi Ulang gagal, sehingga kasir harus mencari
jalan lain, misalnya menunggu proses otomatis atau meminta bantuan tim teknis, sementara pasien dan
keluarganya menunggu di bangsal.

**Rekomendasi.** Tambahkan `REEVALUATED_BY_CASHIER` ke constraint lewat satu migration, sekaligus
perbarui berkas konfigurasinya. Rincian dan opsinya ada di plan repair.

## 5. Tanya-jawab pelapor

Tidak ada pertanyaan eksplisit dari pelapor.

## 6. Temuan tambahan

### ISS-BKC-002-T1 — Kegagalan basis data muncul ke kasir sebagai pesan umum

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | Tidak direncanakan |
| **Kenapa dicantumkan** | Inilah sebab cacat `ISS-BKC-002-01` tidak terlihat jelas oleh kasir maupun pelapor selama tiga minggu |

**Apa yang terjadi.** Saat basis data menolak penyimpanan, kasir hanya membaca "Terjadi kesalahan
pada server". Tidak ada petunjuk bahwa ini kesalahan sistem yang harus dilaporkan, bukan kesalahan
isian kasir.

**Kenapa terjadi.** `InpatientClearanceController.cs:130-139` tidak menangkap `DbUpdateException`,
sehingga penanganannya jatuh ke penangan galat global yang memang sengaja seragam. Detail lengkap
pelanggaran tetap tercatat di log server.

**Rekomendasi.** **Tidak diubah** pada perbaikan ini. Penangan galat global adalah konvensi seluruh
project, dan mengubahnya hanya untuk satu controller berarti menyimpang dari pola yang berlaku.
Setelah `FIX-BKC-002-01`, sebab kegagalan ini hilang.

### ISS-BKC-002-T2 — ID layar karangan pada dokumen blueprint bertabrakan dengan ruang nomor task

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Dokumen blueprint |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` |
| **Perbaikan** | `FIX-BKC-002-02` |
| **Kenapa dicantumkan** | Ditemukan saat menetapkan kode layar pemilik tombol ini, dan menyesatkan `/plan-module-delivery` bila tidak dirapikan |

**Apa yang terjadi.** Amendment revisi `1.9`, `2.0`, dan `2.1` pada blueprint ini — yang ditulis
**agent** dalam sesi `/design-business-module` — memberi ID layar `FE-BKC-09`, `FE-BKC-21`,
`FE-BKC-43`, dan `FE-BKC-44`. ID itu tidak pernah ada sebelumnya:

| ID karangan | Dipakai untuk | ID resmi | Bukti |
| --- | --- | --- | --- |
| `FE-BKC-21` | Layar Surat ke Modul Konsumen | **`BIL-SCR-41`** | `03-frontend-architecture.md:1172` |
| `FE-BKC-09` | Menu Pembayaran | Belum ditemukan ID resminya | Tidak ada kode `BIL-SCR` untuk Menu Pembayaran pada dokumen sebelum revisi `1.9` |
| `FE-BKC-43` | Layar Pelunasan Gabungan (baru, revisi `1.9`) | Belum dialokasikan | — |
| `FE-BKC-44` | Layar Rekonsiliasi Rangkaian Kunjungan (baru, revisi `2.0`) | Belum dialokasikan | — |

**Kenapa ini berbahaya.** Pola `FE-BKC-NNN` di blueprint ini adalah **ruang nomor task**, bukan
layar. Roadmap sudah memakai task `FE-BKC-043` "Badge Status, Saringan & Pesan Blokir" dan
`FE-BKC-044` "Review Dua Hasil & Modal Penyelesaian" — keduanya task Shift Kasir yang **sudah
selesai** (`roadmap/frontend-roadmap.md:1937-1938`). Pembaca, builder, dan `/plan-module-delivery`
dapat mengira `FE-BKC-44` dan `FE-BKC-044` adalah benda yang sama. Seluruhnya ada sekitar 40
kemunculan ID karangan pada dokumen desain.

**Rekomendasi.** Ganti ke pola ID layar yang berlaku (`BIL-SCR-NN`) lewat `/design-business-module`
**sebelum** `/plan-module-delivery` dijalankan untuk `MVP-36` dan `MVP-37`.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Mohon jalankan empat langkah verifikasi di bawah pada lingkungan yang sudah menjalankan migration `20260924054559`, lalu serahkan hasilnya | Pemilik sendiri memutuskan perbaikan dikerjakan **setelah** diverifikasi di lingkungan nyata | **TERJAWAB 14-10-2026** — lihat hasil di bawah |

Langkah verifikasi untuk P-01 — seluruhnya hanya membaca, tidak mengubah data:

```sql
-- 1. Pastikan migration pemasang constraint sudah diterapkan.
SELECT "MigrationId" FROM "__EFMigrationsHistory"
WHERE "MigrationId" LIKE '20260924054559%';

-- 2. Lihat isi constraint yang benar-benar terpasang di basis data.
SELECT conname, pg_get_constraintdef(oid)
FROM pg_constraint
WHERE conname = 'CK_BilInpatientClearanceHandoff_ReasonCode';

-- 3. Hitung kode alasan yang pernah tersimpan. Bila dugaan benar,
--    REEVALUATED_BY_CASHIER tidak akan pernah muncul.
SELECT "ReasonCode", COUNT(*)
FROM public."BilInpatientClearanceHandoff"
GROUP BY "ReasonCode"
ORDER BY "ReasonCode";
```

4. Pada layar Surat ke Modul Konsumen, tekan Evaluasi Ulang untuk satu surat kelayakan rawat inap,
   lalu periksa log server. Dugaan terbukti bila log memuat `DbUpdateException` dengan
   `PostgresException` berkode `23514` (pelanggaran `CHECK`) yang menyebut
   `CK_BilInpatientClearanceHandoff_ReasonCode`.

**Hasil P-01, 14-10-2026.** Atas perintah pemilik ("verifikasi"), agent menjalankan query 1–3
dalam **sesi basis data read-only** terhadap basis data pengembangan yang dirujuk
`appsettings.Development.json` (host remote; connection string tidak dicetak). Sesi ditutup dengan
rollback; nol perubahan data.

| Query | Hasil | Arti |
| --- | --- | --- |
| 1 | `20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff` ada | Migration pemasang constraint sudah diterapkan |
| 2 | `CHECK ("ReasonCode" = ANY (ARRAY['INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED', 'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED']))` | **Terbukti:** enam nilai, tanpa `REEVALUATED_BY_CASHIER`. Karena constraint PostgreSQL bersifat pasti, setiap penyimpanan surat dengan kode itu **pasti** ditolak |
| 3 | Tabel `BilInpatientClearanceHandoff` **kosong** | Belum pernah ada satu pun surat kelayakan tersimpan di lingkungan ini — fitur belum pernah dipakai di sini, sehingga belum ada pengguna yang benar-benar terdampak |

Langkah 4 **sengaja tidak dijalankan**. Basis data ini remote dan kemungkinan dipakai bersama;
bila tombol ternyata berhasil, ia akan menulis surat kelayakan baru yang ikut terbaca bangsal.
Bukti query 2 bersama rantai source di bagian 4 sudah cukup memastikan kegagalannya.

Konteks tambahan dari query yang sama: migration terakhir yang diterapkan adalah
`20261014021257_AddEmployeeBenefitGuarantorAndDeferredBillingConsent`, sehingga migration perbaikan
dibuat di atas keadaan itu.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Kapan cacat ini diperbaiki | **Sudah diputuskan 14-10-2026:** task terpisah, didahulukan sebelum `MVP-36`, setelah verifikasi runtime | — | — |
| K-02 | Persetujuan rencana perbaikan ini | **Sudah diputuskan 14-10-2026:** keduanya disetujui — perintah pemilik "kerjakan FIX-BKC-002-01 dan 02" | — | — |

## 8. Catatan pola

**Pola: daftar nilai `CHECK` yang ditulis tangan menyimpang dari kelas konstantanya.** Cacat ini
lahir karena daftar nilai yang sama ditulis di dua tempat — kelas konstanta C# dan string SQL
constraint — dan keduanya tidak saling diperiksa. Pola yang sama sudah **nyaris** terjadi sekali
lagi di blueprint ini: draf pertama desain revisi `1.9` melewatkan satu dari dua constraint
`Purpose` pada `BilSettlement`, dan baru tertangkap pada impact scan berikutnya.

Risiko berikutnya sudah terlihat: migration `MVP-37` (desain revisi `2.1`) akan menulis ulang
`CK_BilInpatientClearanceHandoff_ReasonCode` secara utuh. Bila daftarnya disusun ulang dari enam
nilai lama ditambah empat nilai baru, `REEVALUATED_BY_CASHIER` **terhapus kembali** dan cacat ini
hidup lagi. Desain revisi `2.1` sudah mencatat jebakan itu beserta daftar sebelas nilai yang benar.

**Usulan pencegahan, untuk dipertimbangkan terpisah:**

| Usulan | Kelebihan | Kekurangan |
| --- | --- | --- |
| Susun string `CHECK` dari konstanta yang sama di berkas konfigurasi, bukan ditulis tangan | Mustahil menyimpang; migration berikutnya otomatis mendeteksi perubahan | Berbeda dari pola konfigurasi lain di repository yang menulis `CHECK` secara literal |
| Test yang membandingkan kelas konstanta dengan isi constraint | Menangkap penyimpangan sebelum rilis | Billing belum punya project test; menambahkannya adalah pekerjaan tersendiri |

Kedua usulan **tidak** dimasukkan ke rencana perbaikan ini supaya perbaikan yang mendesak tetap
kecil.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-14 | Issue dibuat dari temuan `/design-business-module` revisi `2.1`: 1 butir, 2 temuan tambahan. Runtime belum diverifikasi | `diagnose-module-issue` |
| 2026-10-14 | P-01 terjawab: constraint di basis data terbukti enam nilai tanpa `REEVALUATED_BY_CASHIER`; tabel surat kelayakan kosong. Status bukti `ISS-BKC-002-01` naik menjadi `SUDAH-VERIFIKASI` di basis data. K-02 diputuskan pemilik: kedua perbaikan disetujui. Status issue menjadi `DALAM_PERBAIKAN` | Agent atas perintah pemilik |
