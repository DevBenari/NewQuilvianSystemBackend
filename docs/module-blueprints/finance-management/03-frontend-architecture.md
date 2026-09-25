# Finance Management — Arsitektur Frontend

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Frontend SHA | `abed49b03` |
| Backend SHA | `09101d05` |
| Wewenang UI | **Belum ada UI brief yang disetujui.** Dokumen ini menetapkan kontrak fungsional, bukan tampilan |

Dokumen ini menetapkan **apa yang harus bisa dilakukan layar** dan **data apa yang dikonsumsi**.
Ia sengaja tidak menetapkan tata letak, warna, urutan menu, atau pilihan antara modal dan
halaman penuh — semua itu memerlukan brief yang sah atau didelegasikan sebagai
`DEV_DISCRETION`.

## 1. Urutan wewenang yang berlaku

```text
keamanan/privasi/invariant
  -> brief produk/UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Lapisan pertama tidak dapat ditawar oleh lapisan mana pun di bawahnya. Contoh nyata: layar
**MUST NOT** menampilkan tombol Setujui kepada pengguna yang mengajukan permohonan itu, walau
secara tata letak lebih rapi bila tombolnya selalu ada.

## 2. Keadaan frontend saat ini

Diverifikasi langsung pada `abed49b03`:

| Yang sudah ada | Keadaan |
|---|---|
| Menu sidebar "Keuangan" (`corporateFinance`) | Ada, berisi **dua** butir saja: Kategori Petty Cash dan Anggaran Petty Cash |
| `/finance/petty-cash-budget` | Halaman nyata, 687 baris, berfungsi penuh |
| `/finance/master-data/petty-cash-category` | Halaman nyata beserta create, detail, dan update |
| Redux slice Petty Cash | Masih di `src/lib/state/slice/health-services/billing-management/` |
| Hook Petty Cash | Masih di `src/lib/hooks/health-services/billing-management/petty-cash/` |
| Halaman voucher di bawah `/finance/` | **Belum ada** — hanya ada di rute `health-services/billing-management` |
| Layar AR, AP, pembayaran, setoran, kas harian | **Belum ada satu pun** |

Konsekuensinya: pekerjaan frontend modul ini adalah **membangun dari nol** untuk AR, AP, kas,
dan pemantauan integrasi, ditambah **merapikan** yang sudah ada untuk Petty Cash.

## 3. Layar yang dibutuhkan

Kolom "Kebutuhan minimum" adalah kontrak fungsional. Bentuk tampilannya `DEV_DISCRETION`.

### 3.1 Piutang

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar piutang | Cari dan saring menurut penjamin, status, dan rentang jatuh tempo; menampilkan nilai asli dan sisa | `GET /receivables`, `GET /receivables/filters/metadata` |
| Rincian piutang | Menampilkan item pasien, daftar berkas klaim, riwayat pelunasan, koreksi, dan penghapusan dalam satu halaman | `GET /receivables/{id}` |
| Umur piutang | Empat kelompok 0-30, 31-60, 61-90, di atas 90 hari; dapat disaring per penjamin | `GET /receivables/aging` |
| Berkas klaim | Menandai berkas sudah diterima; mengubah status kelengkapan | `POST`/`PATCH .../documents`, `PATCH .../claim-status` |
| Ajukan koreksi | Memilih arah, nominal, dan alasan wajib | `POST /receivables/{id}/adjustments` |
| Setujui atau tolak koreksi | Terpisah dari layar pengajuan; menampilkan siapa yang mengajukan | `POST .../approve`, `POST .../reject` |
| Ajukan penghapusan | Nominal dan alasan wajib | `POST /receivables/{id}/write-offs` |
| Setujui atau tolak penghapusan | Sama seperti koreksi | `POST .../approve`, `POST .../reject` |

### 3.2 Penerimaan

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar penerimaan | Saring menurut metode, tanggal, dan shift kasir | `GET /receipts` |
| Buku penerimaan kasir | Telusur dari penerimaan ke nomor tender, kwitansi, dan tagihan | `GET /receipts/register` |
| Rekonsiliasi shift | Membandingkan total penerimaan tunai Finance dengan `SystemCash` Billing; menampilkan selisih | `GET /receipts/shift-reconciliation` |
| Alokasi penerimaan | Memilih sendiri piutang mana yang dilunasi dan berapa; **tidak ada** pencocokan otomatis (`FIN-DEC-011`) | `POST /receipts/{id}/allocations` |
| Pembalikan | Membalik satu alokasi atau seluruh penerimaan, dengan alasan | `POST .../reverse` |

### 3.3 Utang dan pembayaran

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar utang supplier | Saring menurut supplier, status, dan jatuh tempo | `GET /supplier-payables` |
| Input faktur supplier | Memilih supplier dari master existing, mengisi nomor faktur, tanggal, termin, dan rincian baris | `POST /supplier-payables` |
| Umur utang | Kelompok jatuh tempo | `GET /supplier-payables/aging` |
| Daftar utang dokter | Saring menurut dokter dan periode | `GET /doctor-payables` |
| **Ringkasan fee dokter belum dibayar** | Per dokter: berapa yang belum dibayar dan sejak periode kapan | `GET /doctor-payables/unpaid-summary` |
| Susun pembayaran rekap | Memilih **banyak** utang dalam satu pembayaran; menampilkan selisih antara total dan jumlah alokasi secara langsung | `POST /payments` |
| Ajukan, setujui, tolak | Terpisah sesuai hak akses | `POST .../submit`, `/approve`, `/reject` |
| Tandai sudah dibayar | Nomor bukti transfer wajib | `POST .../mark-paid` |

Layar "Ringkasan fee dokter belum dibayar" adalah jawaban langsung atas keluhan yang disebut
owner pada `FIN-DEC-019`. Ia **MUST** ada sejak rilis pertama rumpun dokter; tanpa layar itu,
rekonsiliasi kembali jatuh ke Excel.

### 3.4 Kas

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Setoran bank | Menampilkan **saldo tersedia** sebelum petugas mengisi nominal | `GET /bank-deposits/available-balance`, `POST /bank-deposits` |
| Posting setoran | Menampilkan penolakan beserta saldo tersedia bila nominal melebihi | `POST /bank-deposits/{id}/post` |
| Kas harian | Posisi hari berjalan dan riwayat per tanggal | `GET /daily-cash/current`, `GET /daily-cash` |
| Rincian kas harian | Telusur dari angka ringkasan ke penerimaan dan setoran penyusunnya | `GET /daily-cash/{cashDate}/breakdown` |
| Tutup kas | Menolak bila masih ada setoran `DRAFT`, dengan pesan yang menyebut jumlahnya | `POST /daily-cash/{cashDate}/close` |

### 3.5 Data induk dan pemantauan

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Bank dan rekening | CRUD beserta aktif/nonaktif | Grup `master-data/banks`, `master-data/bank-accounts` |
| Mata uang dan kurs | CRUD beserta riwayat kurs | Grup `master-data/currencies` |
| Pemantauan fakta Billing | Melihat yang gagal diolah dan mengulangnya | `GET /billing-intake`, `POST .../retry` |
| Pemantauan kejadian Accounting | Melihat status kirim, termasuk yang tertahan; mengirim ulang yang gagal | `GET /accounting-events`, `POST .../retry` |

Layar pemantauan kejadian **MUST** membedakan `HELD` dari `FAILED`, karena tindakan penggunanya
berbeda: yang pertama menunggu Accounting melengkapi aturan posting, yang kedua menunggu Finance
sendiri memperbaiki datanya.

**Diperbarui 25 September 2026 (`FIN-DEC-030`).** Status `HELD_FOR_FINALIZATION` **tidak lagi
dihasilkan** — penerimaan sebelum tagihan final kini langsung siap kirim dengan jenis kejadian
yang berbeda, bukan ditahan. Layar pemantauan tetap **MUST** dapat menampilkan status itu untuk
baris warisan (bila ada), diberi keterangan bahwa ia peninggalan kebijakan lama yang perlu
dibetulkan, bukan keadaan normal yang menunggu Billing.

## 4. Aksi per peran

| Peran | Yang dapat dilakukan | Yang **MUST NOT** terlihat |
|---|---|---|
| Finance AR Staff | Melihat piutang, mengelola berkas klaim, mengalokasikan penerimaan, mengajukan koreksi dan penghapusan | Tombol Setujui pada permohonan mana pun |
| Finance AP Staff | Input faktur supplier, menyusun dan mengajukan pembayaran | Tombol Setujui pembayaran |
| Finance Supervisor | Menyetujui atau menolak koreksi, penghapusan, dan pembayaran | Tombol Setujui pada permohonan yang **dia sendiri** ajukan |
| Cashier/Treasury | Setoran bank, tutup kas harian, menandai pembayaran sudah dibayar | Pengajuan atau persetujuan koreksi piutang |
| Auditor/Manager | Seluruh layar dalam mode baca | Seluruh tombol yang mengubah data |
| Accounting Staff | Layar pemantauan kejadian | Seluruh layar subledger Finance |

**Aturan yang paling mudah dilanggar frontend.** Menyembunyikan tombol Setujui dari pengaju
adalah bantuan tampilan, **bukan** pengaman. Backend tetap menolak lewat `FIN-VAL-020`. Frontend
**MUST NOT** pernah dianggap sebagai lapisan keamanan — menampilkan tombolnya pun tidak akan
membuat permohonannya lolos.

## 5. Penanganan keadaan layar

| Keadaan | Ketentuan |
|---|---|
| Sedang memuat | Menampilkan penanda muat; **MUST NOT** menampilkan angka nol seolah-olah itu nilai sebenarnya |
| Kosong | Membedakan "belum ada data" dari "tidak ada hasil untuk penyaring ini" |
| Gagal | Menampilkan pesan dari backend apa adanya — pesan itu sudah ditulis untuk pengguna (`validation-matrix.md`) |
| Coba lagi | Tersedia untuk kegagalan jaringan; **MUST NOT** otomatis mengulang perintah yang memindahkan uang |
| Data basi | Setelah `409` "Data telah berubah", layar **MUST** memuat ulang sebelum mengizinkan simpan berikutnya |
| Kirim ganda | Tombol simpan dinonaktifkan selama permintaan berjalan, **dan** setiap perintah uang membawa `Idempotency-Key` yang sama selama percobaan itu |

Butir terakhir penting: menonaktifkan tombol saja tidak cukup. Kalau jaringan putus lalu
pengguna menekan ulang, `Idempotency-Key` yang sama-lah yang mencegah catatan kedua
(`FIN-VAL-085`).

## 6. Angka yang MUST NOT dihitung frontend

| Angka | Diambil dari |
|---|---|
| Sisa piutang dan sisa utang | Response backend, bukan hasil pengurangan di layar |
| Kelompok umur piutang | `GET /receivables/aging` |
| Saldo kas tersedia untuk disetor | `GET /bank-deposits/available-balance`, dihitung ulang backend saat posting |
| Posisi kas harian | `GET /daily-cash/current` |
| Selisih antara total pembayaran dan jumlah alokasi | Boleh ditampilkan langsung di layar sebagai bantuan, tetapi keputusan diterima atau ditolak tetap milik backend |
| Total penerimaan per shift | `GET /receipts/shift-reconciliation` |

Alasannya sama untuk semuanya: dua tempat yang menghitung angka uang pasti berbeda setelah
revisi pertama.

## 7. Privasi di layar

| Data | Ketentuan |
|---|---|
| `PatientId`, `EncounterId` | Boleh ditampilkan di rincian piutang Finance; **MUST NOT** ikut ke layar pemantauan kejadian Accounting |
| `BenefitOwnerId`, hubungan keluarga | Hanya pada layar piutang manfaat karyawan, hanya untuk peran yang berhak |
| Nomor rekening bank | Ditampilkan sebagian pada daftar; lengkap hanya pada layar master untuk peran yang berhak |
| `DoctorId` | Boleh pada layar utang dokter; **MUST NOT** pada layar kejadian Accounting |

## 8. Rute

**Belum ada brief yang menetapkan rute final.** Yang mengikat hanyalah dua hal:

1. Rute baru mengikuti pola yang sudah berjalan, yaitu di bawah `/finance/...` — bukan di bawah
   `/health-services/billing-management/...`. Ini konsisten dengan dua rute Finance yang sudah
   ada.
2. Butir menu baru masuk ke group `corporateFinance` yang **sudah ada** di
   `src/utils/menu-sidebar/menu-items.jsx`. Tidak perlu membuat group baru.

Struktur rute selebihnya, termasuk apakah koreksi dan penghapusan memakai halaman terpisah atau
modal, adalah `DEV_DISCRETION` sampai ada brief.

## 9. Pekerjaan merapikan Petty Cash

Bukan membangun ulang — halaman `/finance/petty-cash-budget` sudah berfungsi penuh.

| Pekerjaan | Sifat |
|---|---|
| Memindahkan hook dan Redux slice Petty Cash ke path Finance | Rapi-rapi, bukan fitur baru |
| Menambah halaman voucher di bawah `/finance/` | Memakai ulang modal yang sudah ada (`create-voucher-modal`, `voucher-detail-modal`, `reverse-voucher-modal`) |
| Menambah butir menu voucher pada group Keuangan | Satu baris di `menu-items.jsx` |

Aturan bisnis Petty Cash **MUST NOT** diubah dalam pekerjaan ini. Keputusannya milik blueprint
`billing-kasir` (`FIN-DEC-009`).

## 10. Matriks `DEV_DISCRETION`

| Keputusan | Wewenang | Catatan |
|---|---|---|
| Tata letak halaman, urutan kolom tabel | `DEV_DISCRETION` | Mengikuti pola halaman Finance yang sudah ada |
| Modal atau halaman penuh untuk koreksi dan penghapusan | `DEV_DISCRETION` | — |
| Urutan butir menu di dalam group Keuangan | `DEV_DISCRETION` | Group-nya sendiri sudah ditetapkan |
| Warna, tipografi, spacing | Design system project | Bukan wewenang modul ini |
| Apakah tombol Setujui disembunyikan atau dinonaktifkan bagi pengaju | `DEV_DISCRETION` | Keduanya sah; yang **MUST** adalah backend tetap menolak |
| Apakah rekonsiliasi shift satu halaman atau tab | `DEV_DISCRETION` | — |
| Bentuk penyajian umur piutang | `DEV_DISCRETION` | Kelompoknya sudah ditetapkan `FIN-DEC-010` dan **MUST NOT** diubah layar |
| Kapan angka dimuat ulang otomatis | `DEV_DISCRETION` | Kecuali setelah `409`, yang **MUST** memuat ulang |

## 11. Ketergantungan pada backend

Seluruh layar pada bagian 3 menunggu endpoint yang saat ini berlabel
**Rencana (belum tersedia)** di `contracts/api-contract.md`. Frontend **MUST NOT** dimulai
dengan data tiruan yang bentuknya ditebak — kontrak API sudah tertulis, dan bentuk itulah yang
dipakai.

Dua layar menunggu pihak ketiga, bukan menunggu Finance:

| Layar | Menunggu |
|---|---|
| Piutang manfaat karyawan | Konfirmasi Billing + HR (`FIN-CQ-03`) |
| Status kirim nyata ke Accounting | Endpoint Accounting dibangun (`FIN-CAP-018`) |
