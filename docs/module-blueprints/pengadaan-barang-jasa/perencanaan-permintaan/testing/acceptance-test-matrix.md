# Perencanaan & Permintaan — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-TEST` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-034`) |
| Masukan | `PBJ-PP-API`, `-STATE`, `-VAL`, `-PERM` 1.0 |
| Kebijakan bukti | Sesuai kebijakan verifikasi repository: **tidak membuat proyek test** dan tidak mensyaratkan `dotnet test`. Bukti berupa build, konformitas QBE, skema EF/basis data, uji API manual (Swagger/HTTP), bukti query basis data, dan uji browser untuk FE |

Jenis bukti: **API** = permintaan HTTP beserta respons tercatat; **DB** = query `SELECT` yang menunjukkan
keadaan baris; **FE** = uji browser (Playwright atau manual) dengan tangkapan layar; **QBE** = laporan pemeriksa
konformitas.

## 1. Permintaan Pengadaan

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-01` | `PBJ-DEC-025`, FR alokasi nomor | Buat PR draft lalu ajukan | API, DB | Draft tanpa nomor; setelah ajukan `RequestNumber` terisi, status `SUBMITTED`, satu baris riwayat `SUBMIT` |
| `AT-PP-02` | `PBJ-VAL-001`, `002`, `006` | Ajukan PR tanpa baris, dengan jumlah 0, dan tanpa alasan | API | Tiga kali 422 dengan kode masing-masing; status tetap `DRAFT`; nomor tidak dialokasikan |
| `AT-PP-03` | `PBJ-VAL-008`, `PBJ-DEC-016` | PR mendesak tanpa alasan | API | 422 `PBJ-VAL-008` |
| `AT-PP-04` | `PBJ-DEC-029` | Rencana 10 unit; PR-1 6 unit diajukan; PR-2 6 unit diajukan dengan alasan | API, DB | PR-2 baris: dalam rencana 4, luar rencana 2, `IsOutOfPlan = true` |
| `AT-PP-05` | `PBJ-VAL-009` | PR-2 tanpa alasan luar rencana | API | 422 `PBJ-VAL-009` |
| `AT-PP-06` | `PBJ-DEC-029` (kembali) | PR-1 dibatalkan, lalu cek sisa | API | `plan-line-options` menunjukkan sisa naik 6 |
| `AT-PP-07` | Kunci bersamaan | Dua PR memakai sisa rencana 4 diajukan hampir bersamaan, masing-masing 4 | API (dua klien), DB | Total dalam rencana kedua PR = 4, bukan 8 |
| `AT-PP-08` | `PBJ-DEC-020`, UAT-03 BRD | Pengguna IGD membuka PR Radiologi | API, FE | 404; daftar IGD tidak memuat PR Radiologi |
| `AT-PP-09` | `PBJ-VAL-090` | Dua pengguna mengubah draft yang sama; yang kedua memakai versi lama | API | Yang kedua 409 `PBJ-VAL-090` |
| `AT-PP-10` | Idempotensi | `submit` dikirim dua kali dengan `Idempotency-Key` sama | API, DB | Satu nomor, satu baris riwayat `SUBMIT` |

## 2. Verifikasi dan cek stok

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-20` | `PBJ-DEC-027`, `PBJ-AC-004` | 20 kursi roda @Rp 3.000.000; stok dipenuhi 5; selesaikan | API, DB | `PurchaseQuantity = 15`, `PurchaseTotal = 45.000.000`; instance `Wfl` ber-`Amount = 45.000.000`; status `PENDING_APPROVAL` |
| `AT-PP-21` | `PBJ-DEC-027` (stok penuh) | Semua baris dipenuhi stok | API, DB | Status `FULFILLED_FROM_STOCK`; tidak ada instance `Wfl` |
| `AT-PP-22` | `PBJ-VAL-020` | Dipenuhi 8, tersedia 5 | API | 422 `PBJ-VAL-020` |
| `AT-PP-23` | `PBJ-VAL-021` | Selesaikan dengan satu baris belum dicek | API | 422 `PBJ-VAL-021` |
| `AT-PP-24` | `PBJ-VAL-091` | Definisi `PBJ-PR` dinonaktifkan, lalu selesaikan | API, DB | 409; PR tetap `SUBMITTED`; tidak ada instance setengah jadi |
| `AT-PP-25` | `PBJ-DEC-030` | Sisa pagu Rp 30 juta; nilai beli Rp 45 juta | API, DB | `IsOverBudget = true`, `OverBudgetAmount = 15.000.000`; `RequestContext` `Wfl` memuat `PBJ_OVER_BUDGET`; langkah Direktur Keuangan dibuat |
| `AT-PP-26` | Kondisi tidak aktif | PR tidak lewat pagu dan dalam rencana | DB | Langkah ber-kode kondisi tidak dibuat |
| `AT-PP-27` | Verifikator ≠ pengusul | Pengusul yang juga Tim Pengadaan mencoba memverifikasi PR-nya | API, FE | Ditolak (403/409); tombol tersembunyi |
| `AT-PP-28` | Kembalikan | Kembalikan tanpa alasan, lalu dengan alasan | API | 422, lalu `RETURNED` dengan alasan di riwayat |

## 3. Persetujuan (`Wfl`)

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-30` | `PBJ-AC-002` | Pengusul mencoba menyetujui PR sendiri dari kotak masuk | API, FE | Ditolak `Wfl`; PR tetap `PENDING_APPROVAL` |
| `AT-PP-31` | Sinkron disetujui | Semua jenjang menyetujui | API, DB | PR `APPROVED`; riwayat `WF_APPROVED` sekali |
| `AT-PP-32` | Sinkron ditolak / dikembalikan | Penyetuju menolak; pada PR lain mengembalikan | DB | `REJECTED` / `RETURNED` |
| `AT-PP-33` | `PBJ-AC-003` | Matriks diubah setelah PR disetujui | DB | Jejak approval tetap menyimpan jenjang dan nominal lama |
| `AT-PP-34` | Sinkron gagal lalu diulang | Penangan dibuat gagal sementara | DB, log | Keputusan `Wfl` sah; PR `PENDING_APPROVAL`; setelah sinkron ulang `APPROVED` tanpa riwayat ganda |
| `AT-PP-35` | Regresi HR | Ajukan dan setujui cuti dan lembur setelah perubahan `Wfl` | API | Perilaku HR sama seperti sebelumnya (kolom kondisi `null`) |
| `AT-PP-36` | Tarik | Pengusul menarik PR `PENDING_APPROVAL` | API, DB | PR `CANCELLED`; instance `Wfl` `Withdrawn` |

## 4. Rencana, rencana paket, pagu

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-40` | `PBJ-VAL-040` | Buat rencana kedua untuk IGD 2027 | API | 409 |
| `AT-PP-41` | Revisi berversi | Revisi rencana disetujui | DB | v1 `SUPERSEDED`, v2 `IsCurrentVersion`; `LineKey` sama |
| `AT-PP-42` | `PBJ-VAL-042` | Revisi turun di bawah pemakaian PR | API | 422 |
| `AT-PP-43` | `PBJ-VAL-050` | Alokasi dua rencana paket melebihi baris | API | 422 |
| `AT-PP-44` | Konsolidasi | Tiga unit, satu barang | API | Satu baris gabungan, total benar, tiga unit asal |
| `AT-PP-45` | `PBJ-VAL-060`, `062` | Pagu ganda; revisi pagu di bawah komitmen (komitmen diisi lewat DB uji) | API | 409; 422 |
| `AT-PP-46` | Lineage komitmen | Pagu direvisi naik | DB | Sisa = pagu baru − komitmen lama |

## 5. Katalog dan lampiran

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-50` | `PBJ-DEC-022` | Tambah barang | API, DB | Kode `KTL-…` dari NumberSeries |
| `AT-PP-51` | `PBJ-VAL-070`, `071` | Barang serupa; hapus barang terpakai | API | 409; 409 |
| `AT-PP-52` | `PBJ-VAL-005` | PR memakai barang nonaktif | API | 422 |
| `AT-PP-53` | `PBJ-VAL-080`, `081`, UAT-15 BRD | Unggah `.html`; `.pdf` berisi HTML; `.exe` berganti nama | API | 422 semuanya; tidak ada berkas tersimpan |
| `AT-PP-54` | `PBJ-VAL-082` | Berkas 11 MB | API | 422 |
| `AT-PP-55` | Versi lampiran | Unggah pengganti | DB | Versi 2 `IsCurrent`, versi 1 tetap ada |
| `AT-PP-56` | Unduh terlindungi | Pengguna unit lain mengunduh lampiran PR IGD | API | 404 |

## 6. Fondasi dan konformitas

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-PP-60` | `QBE-MOD-002/003` | Registry memuat prefix dan Area sebelum model pertama | QBE | Laporan konformitas lulus |
| `AT-PP-61` | Build | Backend dan frontend terbangun | Build | Build sukses (backend dijalankan di latar belakang sesuai durasi build) |
| `AT-PP-62` | Skema | Migration menghasilkan tabel dan index sesuai kamus data | DB | `information_schema` dan `pg_indexes` cocok, termasuk index unik parsial |
| `AT-PP-63` | Menu | Setiap layar terjangkau dari menu atau layar induk | FE | Tangkapan layar setiap butir menu `MVP-*` |
| `AT-PP-64` | Data master | Seeder idempoten dijalankan dua kali | DB | Jumlah baris master sama |
