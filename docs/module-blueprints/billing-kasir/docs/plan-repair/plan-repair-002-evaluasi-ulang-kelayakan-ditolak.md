# PLAN-REPAIR-002 — Pulihkan tombol evaluasi ulang kelayakan kasir dan rapikan ID layar blueprint

```yaml
plan_id: PLAN-REPAIR-BKC-002
issue: ../issue/issue-002-evaluasi-ulang-kelayakan-ditolak.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "Pemilik modul Billing — perintah langsung 'kerjakan FIX-BKC-002-01 dan 02'"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "d166cf4f (Yasmina)"
basis_source_frontend: "6e5edd6df (yasmina)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-BKC-002-01` | `ISS-BKC-002-01` | Tambahkan `REEVALUATED_BY_CASHIER` ke constraint kode alasan surat kelayakan, beserta berkas konfigurasinya | BE | 1 | `BE-BKC-FIX-022` | 🟡 SEBAGIAN — 3 dari 6 acceptance criteria terpenuhi (nomor 1, 5, 6). Nomor 2, 3, 4 menunggu uji tombol di layar | [`BE-BKC-FIX-022.md`](../../task/report/backend/BE-BKC-FIX-022.md) |
| `FIX-BKC-002-02` | `ISS-BKC-002-T2` | Ganti ID layar karangan pada amendment revisi `1.9`–`2.1` dengan pola `BIL-SCR-NN` | Dokumen blueprint | 1 | — (perbaikan dokumen, bukan task builder) | ✅ SELESAI | Tabel registri di `blueprint-manifest.md` |

**Ringkasan: 1 dari 2 perbaikan selesai; 1 sebagian (menunggu uji tombol di layar).**

`ISS-BKC-002-T1` sengaja tidak mendapat perbaikan; alasannya pada issue bagian 6.

## 2. Solusi terpilih per temuan

### ISS-BKC-002-01 — Kode alasan evaluasi ulang kasir tidak terdaftar pada constraint

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambahkan `REEVALUATED_BY_CASHIER` ke daftar `CHECK` pada berkas konfigurasi, lalu buat satu migration yang hanya mengubah constraint itu | Radius paling kecil; nol perubahan logika; mengikuti pola berkas konfigurasi lain yang menulis `CHECK` secara literal | Daftar tetap ditulis tangan, sehingga pola penyimpangannya masih mungkin terulang |
| B | Ubah endpoint agar memakai kode yang sudah diizinkan, misalnya `CORRECTION_APPLIED` | Tanpa migration | Menghapus informasi bahwa evaluasi ulang dipicu kasir; surat kelayakan jadi berbohong tentang sebabnya; kode `REEVALUATED_BY_CASHIER` menjadi konstanta yatim |
| C | Susun string `CHECK` dari kelas konstanta, lalu buat migration | Penyimpangan serupa mustahil terulang | Menyimpang dari pola seluruh berkas konfigurasi lain di repository; terlalu besar untuk perbaikan yang mendesak |

**Solusi terpilih: Opsi A.**

1. Pemilik meminta perbaikan yang **segera dan terpisah**; Opsi A adalah perubahan terkecil yang
   menutup cacat.
2. Kode `REEVALUATED_BY_CASHIER` memang dirancang ada (konstantanya sudah ada, endpoint sudah
   memakainya, dan matriks peran revisi `1.5` menyebut tindakan Evaluasi Ulang). Yang salah adalah
   constraint-nya, bukan kodenya.
3. Opsi A mengikuti pola literal yang dipakai seluruh `CHECK` lain pada
   `BilInpatientClearanceHandoffConfiguration.cs`.

**Kenapa bukan opsi lain.** Opsi B merusak kejujuran jejak audit surat kelayakan demi menghindari
satu migration. Opsi C adalah pencegahan yang baik, tetapi itu keputusan pola untuk seluruh
repository dan dicatat terpisah pada issue bagian 8.

### ISS-BKC-002-T2 — ID layar karangan bertabrakan dengan ruang nomor task

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ganti ke pola `BIL-SCR-NN` yang berlaku: `FE-BKC-21` menjadi `BIL-SCR-41`; layar baru mendapat nomor `BIL-SCR` berikutnya; Menu Pembayaran dicarikan ID resminya atau didaftarkan | Selaras dengan satu-satunya pola ID layar yang pernah dipakai blueprint ini; tabrakan dengan task hilang | Menyentuh sekitar 40 kemunculan di beberapa dokumen |
| B | Biarkan ID karangan, tambahkan catatan bahwa itu ID layar | Paling sedikit suntingan | `FE-BKC-44` dan task `FE-BKC-044` tetap nyaris identik; catatan mudah terlewat |

**Solusi terpilih: Opsi A.**

1. `BIL-SCR-41` sudah ada di `03-frontend-architecture.md:1172` sebagai pola ID layar.
2. `/plan-module-delivery` akan segera mengalokasikan task `FE-BKC-NNN` baru untuk `MVP-36` dan
   `MVP-37`; tabrakan nama harus hilang **sebelum** itu.

**Kenapa bukan opsi lain.** Opsi B menitipkan ketelitian kepada setiap pembaca berikutnya untuk
selamanya.

## 3. Skema tampilan sebelum → sesudah

**Tidak ada perubahan susunan layar.** Kedua perbaikan tidak menyentuh source frontend. Yang berubah
hanya hasil yang diterima kasir setelah menekan Evaluasi Ulang:

```text
SEBELUM — Surat ke Modul Konsumen (BIL-SCR-41)
+---------------------------------------------------------------+
| Jenis Surat | Tujuan     | Pasien                | Status     |
| Kelayakan   | Rawat Inap | Ny. Sari Mandasari    | BLOCKED    |
|             |            |                [ Evaluasi Ulang ] |
+---------------------------------------------------------------+
  ⚠ Evaluasi Gagal
    Terjadi kesalahan pada server.                        [ x ]

SESUDAH
+---------------------------------------------------------------+
| Jenis Surat | Tujuan     | Pasien                | Status     |
| Kelayakan   | Rawat Inap | Ny. Sari Mandasari    | CLEARED    |
|             |            |                [ Evaluasi Ulang ] |
+---------------------------------------------------------------+
  Evaluasi Berhasil
    Evaluasi ulang kelayakan kepulangan rawat inap selesai.
    Status clearance: CLEARED.
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Baris surat kelayakan | Jenis, tujuan, pasien, status | Response daftar surat yang sudah ada | `consumer-handoffs-view.jsx` — tidak berubah |
| Pemberitahuan hasil | Teks berhasil atau gagal | Response `POST /inpatient-clearance/reevaluate` | `use-billing-consumer-handoffs.js:268-318` — tidak berubah |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Evaluasi Ulang | Aksi baris | Bagi pemegang `BillingInpatient : Clearance` | Membuka konfirmasi, lalu menilai ulang kelayakan dan menerbitkan surat versi baru |

| Keadaan | Perilaku |
| --- | --- |
| Berhasil | Pemberitahuan hijau menyebut status terbaru; daftar dimuat ulang |
| Gagal karena sebab lain yang sah | Pemberitahuan merah dengan pesan dari server — perilaku yang sudah ada |
| Tidak berhak | Tombol tidak tampil — perilaku yang sudah ada |

## 4. Rincian perbaikan

### FIX-BKC-002-01 — Tambahkan `REEVALUATED_BY_CASHIER` ke constraint kode alasan

| | |
| --- | --- |
| **Menutup** | `ISS-BKC-002-01` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (berkas konfigurasi EF Core) ditambah satu migration |
| **Berkas yang diubah** | `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInpatientClearanceHandoffConfiguration.cs:22-24` — tambah satu nilai pada daftar `CHECK`. Satu berkas migration baru beserta `Designer`, dan pembaruan `Migrations/ApplicationDbContextModelSnapshot.cs` |
| **Radius dampak** | Satu tabel, `public."BilInpatientClearanceHandoff"`. Penulis tabel itu: endpoint evaluasi ulang kasir, penyelesaian invoice, pembayaran dan pembaliknya, serta tagihan susulan. Empat jalur terakhir tidak terpengaruh karena aturannya hanya **dilonggarkan** |
| **Bergantung pada** | P-01 (verifikasi runtime) dan K-02 (persetujuan rencana) |

**Langkah.**

1. Jalankan langkah verifikasi P-01 dan lampirkan hasilnya pada laporan task.
2. Pastikan working tree backend bersih dari perubahan model yang **belum** punya migration. Pada
   `d166cf4f` working tree memuat migration payroll dan employee benefit milik pemilik yang belum
   di-commit, beserta `ApplicationDbContextModelSnapshot.cs` yang sudah berubah.
3. Tambahkan `'REEVALUATED_BY_CASHIER'` ke daftar `CK_BilInpatientClearanceHandoff_ReasonCode` pada
   berkas konfigurasi.
4. Buat migration `FixClearanceReasonCodeReevaluatedByCashier`.
5. Baca isi `Up()` dan `Down()` migration yang dihasilkan. Isinya **MUST** hanya menghapus dan
   memasang ulang satu constraint itu. Bila ada perubahan lain, migration itu membawa perubahan
   model yang bukan miliknya — buang, rapikan working tree, lalu buat ulang.
6. Serahkan penerapan migration kepada pemilik modul. Builder tidak menjalankan
   `dotnet ef database update`.

**Acceptance criteria.**

1. Pada basis data yang sudah menjalankan migration ini, query nomor 2 pada P-01 mengembalikan
   tujuh nilai, termasuk `REEVALUATED_BY_CASHIER`.
2. Kasir menekan Evaluasi Ulang untuk surat kelayakan Ny. Sari Mandasari (nama samaran) yang
   berstatus `BLOCKED` dengan sisa tagihan nol. Hasilnya: pemberitahuan "Evaluasi Berhasil", surat
   versi baru tersimpan dengan `ReasonCode = 'REEVALUATED_BY_CASHIER'`, dan `FinancialVersion`
   naik tepat satu dari versi sebelumnya.
3. Evaluasi ulang untuk pasien yang sisa tagihannya masih Rp 1.500.000 menghasilkan surat
   berstatus `BLOCKED`, bukan galat.
4. Pelunasan invoice rawat inap lewat pembayaran biasa tetap menerbitkan surat dengan
   `ReasonCode = 'INVOICE_SETTLED'` seperti sebelumnya.
5. `Up()` migration berisi tepat satu perubahan: penggantian
   `CK_BilInpatientClearanceHandoff_ReasonCode`.
6. `Down()` mengembalikan enam nilai semula.

**Verifikasi.** Sesuai aturan backend, `dotnet build` dijalankan builder bila task memintanya;
skill ini **tidak** menjalankannya (`NOT RUN`). Tidak ada project test Billing, sehingga verifikasi
fungsional dilakukan manual mengikuti acceptance criteria 2–4. Bila pemilik ingin bukti otomatis,
test regresi `BIL-AT-187` pada desain revisi `2.1` dapat ditulis lebih dulu dalam keadaan gagal.

**Risiko.**

| Risiko | Cara mendeteksi |
| --- | --- |
| Migration ikut membawa perubahan model milik pekerjaan lain yang belum di-commit | Langkah 5: baca `Up()`; hanya satu constraint yang boleh berubah |
| Migration `MVP-37` kelak menulis ulang constraint ini tanpa `REEVALUATED_BY_CASHIER` sehingga cacatnya hidup lagi | Desain revisi `2.1` bagian 6 sudah memuat daftar sebelas nilai yang wajib; reviewer `MVP-37` memeriksa daftar itu |
| `Down()` dijalankan setelah ada baris ber-`REEVALUATED_BY_CASHIER` | `Down()` akan gagal karena baris lama melanggar constraint lama. Mundur hanya aman sebelum ada evaluasi ulang yang berhasil |

### FIX-BKC-002-02 — Rapikan ID layar karangan pada dokumen blueprint

| | |
| --- | --- |
| **Menutup** | `ISS-BKC-002-T2` |
| **Area** | Dokumen blueprint |
| **Jenis perubahan** | Revisi dokumen desain lewat `/design-business-module` — **bukan** pekerjaan builder |
| **Berkas yang diubah** | `03-frontend-architecture.md`, `02-backend-architecture.md`, `04-prd-to-mvp.md`, `contracts/*.md`, `testing/acceptance-test-matrix.md`, dan `blueprint-manifest.md` — seluruh kemunculan `FE-BKC-09`, `FE-BKC-21`, `FE-BKC-43`, `FE-BKC-44` sebagai ID layar |
| **Radius dampak** | Dokumen saja; nol source aplikasi. Task `FE-BKC-043` dan `FE-BKC-044` pada roadmap **tidak** disentuh — merekalah pemilik sah nomor itu |
| **Bergantung pada** | K-02 |

**Langkah.**

1. Ganti `FE-BKC-21` menjadi `BIL-SCR-41`.
2. Cari ID resmi Menu Pembayaran pada dokumen frontend dan peta menu. Bila tidak ada, daftarkan
   satu nomor `BIL-SCR` baru untuknya.
3. Beri layar Pelunasan Gabungan dan Rekonsiliasi Rangkaian Kunjungan nomor `BIL-SCR` berikutnya
   yang belum dipakai.
4. Catat pemetaan ID lama ke ID baru pada manifest, supaya rujukan pada riwayat percakapan dan
   laporan lama tetap dapat dibaca.

**Acceptance criteria.**

1. Pencarian `FE-BKC-21`, `FE-BKC-43`, `FE-BKC-44`, dan `FE-BKC-09` sebagai ID **layar** pada
   seluruh dokumen desain menghasilkan nol temuan, kecuali pada tabel pemetaan di manifest.
2. Task `FE-BKC-043` dan `FE-BKC-044` pada `roadmap/frontend-roadmap.md` tetap utuh.
3. Setiap layar pada peta butir menu revisi `1.9` dan `2.0` punya ID berpola `BIL-SCR-NN`.

**Verifikasi.** Pencarian teks pada folder blueprint. Nol perintah build.

**Risiko.** Menyunting terlalu luas lalu ikut mengganti task `FE-BKC-0NN` yang sah. Cara mencegahnya:
penggantian dilakukan per kemunculan yang konteksnya jelas layar, bukan cari-dan-ganti massal.

## 5. Urutan pengerjaan

```text
P-01 (verifikasi runtime) — terjawab 14-10-2026
└── K-02 (persetujuan rencana) — diputuskan 14-10-2026
    ├── FIX-BKC-002-01   BE-BKC-FIX-022, didahulukan sebelum MVP-36
    └── FIX-BKC-002-02   wajib selesai sebelum /plan-module-delivery untuk MVP-36 dan MVP-37
```

Kedua perbaikan tidak saling bergantung dan boleh dikerjakan bersamaan.

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Kapan cacat diperbaiki | **Sudah diputuskan 14-10-2026:** task terpisah, didahulukan, setelah verifikasi runtime | — | — |
| K-02 | Persetujuan rencana ini | **Sudah diputuskan 14-10-2026:** keduanya disetujui | — | — |

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `02-backend-architecture.md` | Amendment revisi `2.1`, bagian 6 rencana migration, baris nomor 1 | Migration `FixClearanceReasonCodeReevaluatedByCashier` berstatus rencana | Ditandai sudah diterapkan, merujuk laporan task `FIX-BKC-002-01`, setelah perbaikan selesai |
| `03-frontend-architecture.md` dan dokumen desain lain | Seluruh kemunculan ID layar karangan | `FE-BKC-09`, `FE-BKC-21`, `FE-BKC-43`, `FE-BKC-44` | `BIL-SCR-NN` sesuai `FIX-BKC-002-02` |

Revisi dokumen desain itu tidak dikerjakan oleh `diagnose-module-issue`.

## 8. Verifikasi menyeluruh

Setelah kedua perbaikan selesai:

1. Kasir menekan Evaluasi Ulang pada tiga surat kelayakan dengan keadaan berbeda — sisa tagihan nol,
   sisa tagihan masih ada, dan pasien yang baru mendapat tagihan susulan. Ketiganya menghasilkan
   surat versi baru tanpa galat, dengan status `CLEARED`, `BLOCKED`, dan `BLOCKED`.
2. Query nomor 3 pada P-01 kini memuat baris `REEVALUATED_BY_CASHIER`.
3. Log server tidak lagi memuat pelanggaran `CK_BilInpatientClearanceHandoff_ReasonCode`.
4. Pencarian ID layar karangan pada dokumen desain menghasilkan nol temuan di luar tabel pemetaan.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-14 | Rencana dibuat: 2 perbaikan, keduanya menunggu persetujuan; `FIX-BKC-002-01` juga menunggu verifikasi runtime | `diagnose-module-issue` |
| 2026-10-14 | Pemilik mengerjakan kedua perbaikan. Diverifikasi agent: migration sudah di-commit (`d268a41f`) dan diterapkan ke dev; constraint memuat `REEVALUATED_BY_CASHIER`; ID layar karangan nol di dokumen desain. Register dirapikan: `FIX-BKC-002-01` 🟡 karena tabel surat kelayakan dev masih kosong sehingga tombol belum teruji; `FIX-BKC-002-02` ✅. `status_rencana` tetap `DISETUJUI` | `diagnose-module-issue` |
| 2026-10-14 | P-01 terjawab (constraint terbukti enam nilai). K-02: pemilik menyetujui kedua perbaikan. `FIX-BKC-002-01` didaftarkan sebagai `BE-BKC-FIX-022`. `status_rencana` menjadi `DISETUJUI` | Agent atas perintah pemilik |
