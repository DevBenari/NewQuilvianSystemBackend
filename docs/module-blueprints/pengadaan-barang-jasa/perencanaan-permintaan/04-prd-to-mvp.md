# Perencanaan & Permintaan — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk / modul | Quilvian V2 · Pengadaan Barang & Jasa · sub-modul `perencanaan-permintaan` |
| Dokumen | `PBJ-PP-MVP` · versi `1.0` · status **`approved`** — pengguna sesi, 12 November 2026 (`PBJ-DEC-034`) |
| Repository target | `NewQuilvianSystemBackend` @`100c011d` (branch `sukmagp`), `V2QuilvianSystemFrontendDev` @`763d665b` (branch `sukmagpV2`) |
| Masukan | `02-backend-architecture.md`, `03-frontend-architecture.md`, `contracts/*` 1.0, `data/data-dictionary.md`, `flowcharts/*`; `PBJ-DEC-001`..`030` |
| Cakupan | Dari katalog dan pagu tersedia, unit mengajukan PR, Tim Pengadaan memverifikasi dan mencatat cek stok, sampai PR disetujui berjenjang dan siap dipaketkan; ditambah rencana kebutuhan, revisi, konsolidasi, dan rencana paket |

## 2. Ringkasan eksekutif

Rumah sakit butuh satu jalur resmi bagi setiap unit untuk meminta barang atau jasa. Jalur itu harus bisa
dilacak, memeriksa stok sebelum membeli, menandai permintaan di luar rencana atau melebihi pagu, dan disetujui
orang yang berwenang. Pengusul tidak boleh menyetujui permintaannya sendiri. MVP ini menghasilkan PR yang
sudah disetujui atas **nilai yang benar-benar akan dibeli**, lengkap dengan jejaknya, sebagai bahan bagi
sub-modul Pemilihan Penyedia.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
| --- | --- |
| Belum ada modul, menu, maupun halaman Pengadaan | `01-existing-capability-map.md` `PBJ-CAP-018` |
| Belum ada master barang umum; hanya obat dan satuan | `PBJ-CAP-012` |
| Mesin approval bersama ada, tetapi baru meneruskan hasil ke dokumen HR | `PBJ-CAP-001` |
| Penomoran, hak akses, dan penempatan pengguna per unit sudah tersedia | `PBJ-CAP-003`, `006`, `007` |
| PO dan GR sudah dikelola Finance/BLM, sehingga tidak boleh dibuat ulang | `PBJ-FACT-001`, `002` |

## 4. Visi produk

1. Tim Pengadaan memelihara katalog barang/jasa.
2. Keuangan menyusun pagu per unit, kategori, dan sumber dana.
3. Unit menyusun rencana kebutuhan tahunan yang disetujui.
4. Unit mengajukan PR dari katalog. Sistem membagi kuantitas menjadi dalam dan di luar rencana.
5. Tim Pengadaan mencatat cek stok. Sistem menghitung kebutuhan beli dan membandingkannya dengan sisa pagu.
6. Penyetuju menyetujui sesuai matriks atas nilai beli, dengan jenjang tambahan bila melebihi pagu.
7. PR yang disetujui siap diambil Pemilihan Penyedia, dan rencana paket menjadi sumber paket.

## 5. Batas MVP

**Titik mulai:**
1. Registry prefix dan Area sudah disetujui (`PBJ-OQ-002`).
2. Katalog berisi barang yang akan diminta, dan definisi approval `PBJ-*` beserta matriksnya sudah di-seed.
3. Pengusul punya penempatan aktif di unitnya.

**Titik akhir:**
1. PR berstatus `APPROVED` dengan nilai beli, tanda (mendesak, luar rencana, lewat pagu), dan jejak persetujuan; atau berstatus `FULFILLED_FROM_STOCK`.
2. Rencana kebutuhan dan pagu berlaku per unit dan tahun, dengan riwayat revisi.
3. Rencana paket `APPROVED` dengan rincian unit asal.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
| --- | --- |
| Unit Peminta | Menyusun rencana kebutuhan, membuat dan mengajukan PR, memperbaiki PR yang dikembalikan, menarik PR |
| Kepala Unit | Penyetuju jenjang awal sesuai matriks |
| Tim Pengadaan | Katalog, verifikasi, cek stok, konsolidasi, rencana paket, pembatalan PR disetujui |
| Manajer Pengadaan / Direktur | Penyetuju jenjang berikutnya dan jenjang lewat pagu |
| Keuangan / Anggaran | Pagu dan revisinya |
| Auditor | Membaca semua dokumen dan riwayat |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Perluasan approval `Wfl` (penangan dokumen non-HR + kode kondisi langkah) | `PBJ-CAP-001`, `002` | Wajib; tanpa ini PR tidak pernah berubah status setelah disetujui |
| Penomoran dokumen | `PBJ-CAP-003` | Wajib; dipakai apa adanya |
| Hak akses dan cakupan unit | `PBJ-CAP-006`, `007`, `008` | Wajib; UAT-03 BRD |
| Katalog barang/jasa | `PBJ-CAP-012` | Wajib; PR wajib memilih dari katalog (`PBJ-DEC-022`) |
| Lampiran | `PBJ-CAP-004` | Wajib; alasan dan bukti kebutuhan |
| Riwayat dokumen | `PBJ-CAP-005` | Wajib; submenu 3.5 dan audit |
| PR, verifikasi, cek stok manual, mendesak | `PBJ-CAP-018` (layar), K-03, K-04 | Wajib; inti sub-modul |
| Rencana kebutuhan dan revisi | K-01 | Wajib; dasar pembagian dalam/luar rencana |
| Pagu dan uji lewat pagu | K-02 | Wajib; `PBJ-DEC-030` |
| Konsolidasi dan rencana paket | K-01 (2.2, 2.4) | Wajib; sumber paket `pemilihan-penyedia` |

## 8. Kemampuan yang ditunda

| Kemampuan | ID asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Integrasi data stok | `PBJ-CAP-013`, `PBJ-OQ-008` | Sumber stok umum belum ada dan pemiliknya belum diketahui | Cek stok dicatat manual per baris (`PBJ-DEC-008`) |
| Dashboard 1.1, 1.3, 1.4, 1.5 | K-13 | Butuh data paket, kontrak, dan monitoring dari sub-modul yang belum dirancang | Daftar PR (`FE-PBJ-PP-09`, `-16`) dan daftar pagu dengan sisa (`-04`) |
| Laporan 11.4–11.6 | K-13 | Idem; definisi realisasi bergantung pada rantai paket → PO → faktur | Riwayat dokumen per PR dan saringan daftar |
| Notifikasi khusus Pengadaan | `PBJ-CAP-017` | Notifikasi platform belum dirancang | Kotak masuk `Wfl` menampilkan tugas; gerbang notifikasi `Wfl` existing dipakai apa adanya bila sudah aktif |
| Penulisan komitmen pagu | `PBJ-DEC-009` | Komitmen lahir dari penetapan vendor (`pemilihan-penyedia`) | Sisa pagu = pagu (komitmen 0) |
| `CONVERTED_TO_PACKAGE` | `PBJ-DEC-028` | Dipicu `pemilihan-penyedia` | PR berhenti di `APPROVED` |

## 9. Alur bisnis target

**`FLOW-PBJ-MVP-001`:**
1. Unit membuat PR.
2. Sistem membagi kuantitas dalam/luar rencana saat diajukan dan memberi nomor.
3. Tim Pengadaan memverifikasi dan mencatat cek stok.
4. Sistem menghitung nilai beli dan menguji sisa pagu.
5. Sistem mengirim PR ke `Wfl` dengan definisi normal atau mendesak serta kode kondisi.
6. Penyetuju menyetujui per jenjang.
7. `Wfl` memanggil penangan Pengadaan, dan PR menjadi `APPROVED`.

Rincian dan jalur gagal ada di `flowcharts/pengajuan-dan-verifikasi-pr.md`.

## 10. Epic dan functional requirement

### `EPIC PBJ-01` — Perluasan approval `Wfl` · disposisi `EXTEND`

- **FR-PBJ-001:** `Wfl` memanggil penangan terdaftar untuk `ReferenceType` yang tidak dikenali penangan HR. **Contoh:** instance `PBJ_PR` disetujui → `ProcurementWorkflowLifecycleHandler` dipanggil; instance cuti tetap ditangani penangan cuti.
- **FR-PBJ-002:** langkah ber-`ActivationConditionCode` hanya dibuat bila kode ada di `RequestContext.conditionCodes`. **Contoh:** `PBJ_OVER_BUDGET` ada → langkah Direktur Keuangan dibuat; tidak ada → dilewati.
- **FR-PBJ-003:** definisi dan langkah HR yang ada berperilaku sama (`AT-PP-35`).

### `EPIC PBJ-02` — Katalog dan master · disposisi `MISSING / NEW`

- **FR-PBJ-010:** Tim Pengadaan menambah barang, dan kode `KTL-000045` dibuat server.
- **FR-PBJ-011:** barang serupa ditolak (`PBJ-VAL-070`); barang terpakai hanya dapat dinonaktifkan (`PBJ-VAL-071`).
- **FR-PBJ-012:** empat jenis pengadaan dan sumber dana tersedia lewat data awal.

### `EPIC PBJ-03` — Lampiran dan riwayat · disposisi `MISSING / NEW`

- **FR-PBJ-020:** berkas ditolak bila ekstensi atau isinya tidak cocok. **Contoh:** `penawaran.pdf` berisi HTML → 422.
- **FR-PBJ-021:** berkas pengganti membuat versi baru, dan versi lama tetap dapat dibuka.
- **FR-PBJ-022:** setiap aksi status menulis satu baris riwayat dengan pelaku, waktu, status sebelum–sesudah, dan alasan.

### `EPIC PBJ-04` — Pengajuan PR · disposisi `MISSING / NEW`

- **FR-PBJ-030:** PR draft tanpa nomor; nomor dialokasikan saat pertama diajukan.
- **FR-PBJ-031:** pembagian dalam/luar rencana dihitung server saat diajukan. **Contoh:** sisa 4, diminta 6 → 4 dalam, 2 luar rencana, dan alasan wajib.
- **FR-PBJ-032:** dua PR yang diajukan bersamaan tidak memakai sisa rencana yang sama (`AT-PP-07`).
- **FR-PBJ-033:** pengguna hanya melihat PR unit dalam cakupannya; di luar cakupan → 404.
- **FR-PBJ-034:** PR mendesak wajib beralasan dan berbatas waktu, dan memakai definisi `PBJ-PR-MENDESAK`.
- **FR-PBJ-035:** pengusul dapat membatalkan atau menarik PR sebelum disetujui **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]**.

### `EPIC PBJ-05` — Verifikasi dan cek stok · disposisi `MISSING / NEW`

- **FR-PBJ-040:** Tim Pengadaan mencatat stok tersedia dan jumlah dipenuhi stok per baris; dipenuhi tidak boleh melebihi tersedia maupun diminta.
- **FR-PBJ-041:** kebutuhan beli dan nilai beli dihitung server. **Contoh:** 20 − 5 = 15 × Rp 3.000.000 = Rp 45.000.000.
- **FR-PBJ-042:** bila kebutuhan beli 0, PR menjadi `FULFILLED_FROM_STOCK` tanpa approval.
- **FR-PBJ-043:** bila melebihi sisa pagu, PR ditandai dengan jumlah kelebihan, dan kode `PBJ_OVER_BUDGET` dikirim ke `Wfl`.
- **FR-PBJ-044:** pengusul tidak dapat memverifikasi PR-nya sendiri.

### `EPIC PBJ-06` — Rencana kebutuhan dan revisi · disposisi `MISSING / NEW`

- **FR-PBJ-050:** satu rencana berlaku per unit dan tahun.
- **FR-PBJ-051:** revisi membuat versi baru dengan kunci baris yang sama; versi lama menjadi `SUPERSEDED` saat revisi disetujui.
- **FR-PBJ-052:** revisi tidak boleh menurunkan kuantitas di bawah pemakaian PR atau alokasi paket.
- **FR-PBJ-053:** layar revisi menampilkan perbandingan sebelum–sesudah per baris.

### `EPIC PBJ-07` — Pagu · disposisi `MISSING / NEW`

- **FR-PBJ-060:** pagu unik per tahun × unit × kategori × sumber dana.
- **FR-PBJ-061:** sisa pagu = pagu berlaku − komitmen aktif, dan komitmen menempel lewat garis keturunan pagu.
- **FR-PBJ-062:** revisi pagu tidak boleh di bawah komitmen.

### `EPIC PBJ-08` — Konsolidasi dan rencana paket · disposisi `MISSING / NEW`

- **FR-PBJ-070:** gabungan kebutuhan per barang beserta unit asal dan sisa belum dialokasikan.
- **FR-PBJ-071:** alokasi ke rencana paket tidak melebihi baris rencana.
- **FR-PBJ-072:** satu rencana paket berisi satu jenis pengadaan.

## 11. Model status yang diusulkan

Lengkapnya di `contracts/state-transition-matrix.md`.
- **PR:** `DRAFT` → `SUBMITTED` → `PENDING_APPROVAL` → `APPROVED`, dengan cabang `RETURNED`, `REJECTED`, `CANCELLED`, `FULFILLED_FROM_STOCK`, dan `CONVERTED_TO_PACKAGE`.
- **Rencana dan pagu:** `DRAFT` → `PENDING_APPROVAL` → `APPROVED`, lalu `SUPERSEDED` saat digantikan revisi.

**Invariant:** status `APPROVED` hanya lahir dari `Wfl`, dan dokumen final tidak dihidupkan kembali.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| `NumberSeriesAllocator`, `[AccessPermission]` + seeder, `AspNetUserOrganization`, `MstDepartment`, `MstMeasurement`, `MstDrug`, kotak masuk `Wfl` | `WorkflowReferenceLifecycleService` (penangan terdaftar), `WorkflowService` (saring langkah), `MstWorkflowStep` (+1 kolom) | Area `Corporate/ProcurementManagement`: 4 master, 10 tabel `Prc*`, 14 service, 9 controller |

## 13. Sasaran kemampuan API

Seluruhnya **Rencana (belum tersedia)**. Rinciannya ada di `contracts/api-contract.md`.

| Grup Swagger | Base URL | Epic |
| --- | --- | --- |
| `Corporate / Procurement Management / Request / Purchase Request` | `api/v1/corporate/procurement-management/purchase-requests` | `EPIC PBJ-04` |
| `Corporate / Procurement Management / Request / Verification` | `.../purchase-request-verifications` | `EPIC PBJ-05` |
| `Corporate / Procurement Management / Planning / Need Plan` | `.../need-plans` | `EPIC PBJ-06` |
| `Corporate / Procurement Management / Planning / Budget` | `.../budgets` | `EPIC PBJ-07` |
| `Corporate / Procurement Management / Planning / Consolidation` | `.../plan-consolidations` | `EPIC PBJ-08` |
| `Corporate / Procurement Management / Planning / Package Plan` | `.../package-plans` | `EPIC PBJ-08` |
| `Corporate / Procurement Management / Master Data / Item` | `.../master-data/items` | `EPIC PBJ-02` |
| `Corporate / Procurement Management / Master Data / Reference` | `.../master-data` | `EPIC PBJ-02` |
| `Corporate / Procurement Management / Common / Attachment` | `.../attachments` | `EPIC PBJ-03` |

## 14. Matriks kewenangan

String izin ada di `contracts/permission-audit-matrix.md` bagian 2 dan 3. Contoh: Unit Peminta memegang
`ProcurementRequest : Create, Submit`; Tim Pengadaan memegang `ProcurementRequestVerification : Verify`;
penyetuju memegang `ApprovalInbox : Approve`.

## 15. Batas integrasi dan billing

Sub-modul ini **tidak** membuat PO, GR, faktur, utang, pembayaran, jurnal, maupun tagihan pasien; semuanya
milik Finance/BLM/Accounting. Ia juga tidak menulis stok dan tidak membuat paket eksekusi.

## 16. Guardrail regulasi

RS swasta dengan SOP internal (`PBJ-DEC-006`). Tidak ada data rekam medis atau pasien. Kewajiban yang
mengikat: jejak audit untuk setiap keputusan, pemisahan tugas antara pengusul, verifikator, dan penyetuju,
serta penyamaran nilai di notifikasi. Masa simpan dokumen belum ditetapkan (`PBJ-OQ-015`).

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | Aksi status atomik: status, riwayat, nomor, dan instance `Wfl` dalam satu transaksi |
| `NFR-002` | Konkurensi: `RowVersion` pada dokumen; *advisory lock* per baris rencana saat diajukan |
| `NFR-003` | Idempotensi: `Idempotency-Key` pada aksi status; `IdempotencyKey` `Wfl` per pengajuan |
| `NFR-004` | Otorisasi di backend; cakupan unit pada setiap query; 404 untuk data di luar cakupan |
| `NFR-005` | Nominal `numeric(18,2)`, kuantitas `numeric(18,4)`, tanpa bilangan pecahan biasa |
| `NFR-006` | Waktu disimpan UTC (`DateTimeOffset`) dan ditampilkan zona waktu lokal |
| `NFR-007` | Daftar memakai pagination server, `AsNoTracking`, dan saringan dari daftar putih |

## 18. Skenario UAT

| ID | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-01` | 04, 05 | Rencana IGD 20 kursi roda; pagu cukup | Ajukan PR 20; verifikasi stok 5; setujui semua jenjang | PR `APPROVED`, nilai beli Rp 45.000.000 |
| `UAT-02` | 04 | — | Ajukan PR tanpa baris atau dengan jumlah 0 | Ditolak dengan pesan yang bisa diperbaiki |
| `UAT-03` | 04 | PR milik Radiologi | Pengguna IGD membuka tautannya | "Tidak ditemukan atau bukan milik unit Anda." |
| `UAT-04` | 01, 05 | — | Pengusul mencoba menyetujui PR sendiri | Ditolak |
| `UAT-05` | 05 | Stok mencukupi semua | Verifikasi | `FULFILLED_FROM_STOCK`, tanpa approval |
| `UAT-06` | 05, 07 | Sisa pagu Rp 30 juta | PR bernilai beli Rp 45 juta | Tanda lewat pagu Rp 15 juta; ada jenjang Direktur Keuangan |
| `UAT-07` | 04, 06 | Sisa rencana 4 | PR 6 tanpa alasan, lalu dengan alasan | Ditolak, lalu diterima dengan 2 di luar rencana |
| `UAT-08` | 04 | Sisa rencana 4 | Dua PR × 4 diajukan bersamaan | Hanya 4 yang tercatat dalam rencana |
| `UAT-09` | 06 | Rencana 10, terpakai 6 | Revisi ke 5 | Ditolak |
| `UAT-10` | 06 | Rencana v1 disetujui | Revisi ke 12 dan disetujui | v2 berlaku; banding versi menampilkan 10 → 12 |
| `UAT-11` | 08 | Tiga unit merencanakan barang sama | Konsolidasi dan buat rencana paket | Satu baris gabungan; unit asal tersimpan |
| `UAT-12` | 08 | Baris rencana 10 | Paket A 6 + paket B 5 | Paket B ditolak |
| `UAT-13` | 07 | Pagu sudah ada | Buat pagu kombinasi sama | Ditolak; diarahkan ke revisi |
| `UAT-14` | 03 | — | Unggah HTML / EXE berganti nama | Ditolak tanpa berkas tersimpan |
| `UAT-15` | 02 | Barang dipakai PR | Hapus barang | Ditolak; hanya dapat dinonaktifkan |
| `UAT-16` | 01 | Cuti pegawai berjalan | Setujui cuti setelah perubahan `Wfl` | Perilaku cuti tidak berubah |

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Registry prefix dan Area disetujui sebelum model pertama | `AT-PP-60` |
| Satu PR dapat berjalan dari draft sampai `APPROVED` | `UAT-01`, `AT-PP-31` |
| Pengusul tidak dapat menyetujui atau memverifikasi PR sendiri | `UAT-04`, `AT-PP-27`, `AT-PP-30` |
| Nominal approval = nilai beli setelah stok | `AT-PP-20` |
| Sisa rencana tidak bisa terpakai ganda | `UAT-08`, `AT-PP-07` |
| Data unit lain tidak bocor | `UAT-03`, `AT-PP-08`, `AT-PP-56` |
| Alur HR tidak berubah | `UAT-16`, `AT-PP-35` |
| Seluruh master MVP terisi dan seeder idempoten | `AT-PP-64`; bagian 10 arsitektur |
| Setiap layar `MVP-*` terjangkau dari menu atau layar induk | `AT-PP-63` |
| Build backend dan frontend sukses; skema sesuai kamus data | `AT-PP-61`, `AT-PP-62` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Epic | Syarat mulai |
| --- | --- | --- | --- |
| `MVP-0` | Registry; perluasan `Wfl`; master dan katalog; lampiran; riwayat; cakupan unit; seed definisi `PBJ-*` | 01, 02, 03 | Blueprint disetujui (`PBJ-DEC-034`); `PBJ-OQ-002` |
| `MVP-1` | PR, verifikasi, cek stok, approval, daftar 3.1–3.5, 1.2, 11.1, 11.2 | 04, 05 | `MVP-0` |
| `MVP-2` | Rencana kebutuhan dan revisi | 06 | `MVP-0`; dapat paralel dengan `MVP-1` |
| `MVP-3` | Pagu dan uji lewat pagu | 07 | `MVP-1` (uji lewat pagu ada di verifikasi) |
| `MVP-4` | Konsolidasi dan rencana paket | 08 | `MVP-2` |
| `POST-MVP` | Semua butir bagian 8 | — | Di luar rilis pertama |

Sebelum `MVP-2` selesai, semua PR akan ditandai di luar rencana karena belum ada rencana. Ini sah, tetapi
pengusul wajib mengisi alasan. Bila tidak diinginkan, kerjakan `MVP-2` sebelum `MVP-1` dibuka ke pengguna.

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| ~~`PBJ-OQ-002`~~ — ditutup `PBJ-DEC-035` (registry `Prc` `ACTIVE`) | — | — | — |
| ~~`PBJ-OQ-031`~~ — ditutup `PBJ-DEC-031` (ikut Platform, `PR-000031`) | — | — | — |
| ~~`PBJ-OQ-032`~~ — ditutup `PBJ-DEC-032` (grup "Pengaturan Pengadaan") | — | — | — |
| ~~Konfirmasi `G-04`, `G-05`, `G-07`, `G-08`, `G-11`~~ — ditutup `PBJ-DEC-033` | — | — | — |
| `PBJ-OQ-019` — nilai awal matriks dari SOP | Pengguna sesi | Approval memakai baris fallback | Tidak |
| `PBJ-OQ-015` — SLA dan masa simpan | Pengguna sesi | Tidak ada penghapusan otomatis; SLA langkah memakai nilai bawaan `Wfl` | Tidak |
