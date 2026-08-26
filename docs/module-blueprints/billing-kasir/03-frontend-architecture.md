# Billing dan Kasir — Arsitektur Frontend

> Revision `0.4`, status **approved**; semua layar tetap berstatus **Rencana (belum tersedia)** sampai source diimplementasikan. Input keputusan `0.2`; owner fungsional Product/Billing/Cashier, authority UI Frontend. Root `AGENTS.md` frontend belum ditemukan sehingga aturan visual rinci menjadi dependency build, bukan blocker desain fungsional.

## Prinsip pengalaman pengguna

UI harus memperlihatkan perbedaan antara tagihan berjalan, dana deposit, pembayaran yang sudah berhasil, saldo pasien, saldo penjamin, dan status finalisasi. Tombol tidak boleh menyiratkan “lunas” ketika saldo diselesaikan lewat write-off. Data klinis minimum saja ditampilkan; nomor kartu, token, dan payload provider tidak masuk browser log atau analytics.

## Layar dan workspace

| Workspace | Aktor | Data/status | Aksi utama | Exception yang terlihat |
| --- | --- | --- | --- | --- |
| Daftar Billing | Billing, Kasir | Encounter, jenis layanan, invoice state, outstanding | Cari/filter/buka | Stale version, invoice belum ada |
| Detail Invoice | Billing, Kasir | Item sumber, qty, tarif snapshot, coverage, diskon, tax, patient/guarantor portion | Recalculate, void eligible item, finalisasi | Order belum complete, duplicate source, harga berubah |
| Deposit Rawat Inap | Kasir | Saldo tersedia, top-up, allocation, ledger | Top-up, alokasikan progress, release sisa | Dana kurang, concurrency conflict |
| Pembayaran | Kasir | Outstanding, tender split, status provider | Tambah tender, submit, retry status | QRIS gagal/pending; tender tunai tetap sukses |
| Refund/Write-off/Adjustment | Billing/Finance | Case, reason, maker/approver, histori | Ajukan, approve/reject, reverse | Self-approval ditolak, post-final rule |
| Finalisasi | Billing | Checklist order, calculation version, patient paid, debtor AR, doctor AP basis | Preview, confirm final | Missing order/coverage/debtor |
| Shift Kasir | Kasir/Kepala Kasir | Opening, receipts, system cash, physical cash, variance | Open, handover, close, review/reopen | Selisih belum direview |
| Master Policy | Finance/IT/Dokter | Effective dates, nominal/rate, approval | View/configure sesuai hak | Overlap effective period |

Contoh pembayaran split: kasir memasukkan Tunai Rp300.000 dan QRIS Rp700.000. Tunai sukses tetapi QRIS gagal. UI mempertahankan receipt tunai, menampilkan outstanding Rp700.000, lalu hanya meminta metode pengganti untuk saldo tersebut.

## Alur utama

### Rawat inap dan progress payment

Tujuan: menerima dana tanpa menutup billing berjalan. Prasyarat: encounter ranap aktif, invoice OPEN, shift kasir aktif. Kasir membuka Deposit, melakukan top-up, lalu memilih jumlah allocation. Sistem menampilkan saldo deposit sebelum/sesudah dan versi invoice. Setelah berhasil, state deposit dan outstanding dimuat ulang. Tindakan baru tetap dapat masuk. Jika versi berubah saat submit, UI tidak menebak; tampilkan konflik dan muat ulang.

### OTC

Tujuan: memastikan pembayaran lunas sebelum layanan. Kasir menyelesaikan seluruh tender split. Hanya status settled yang mengaktifkan bukti clearance. Jika petugas lab membatalkan sebelum pemeriksaan, UI menunjukkan request refund; pelaksanaan dana tetap oleh Finance sesuai metode asal.

### Final billing

Tujuan: mengunci kalkulasi dan membuat basis AR/AP. Billing melihat checklist: semua order complete, calculation terbaru, tanggungan pasien settled atau departure exception sah, debtor penjamin valid. Konfirmasi memperlihatkan patient, primary, excess, AR per debtor, dan AP dokter “belum siap dibayar”. Sesudah sukses layar menjadi read-only; koreksi diarahkan ke Adjustment.

## State management dan integrasi FE

| Concern | Rencana |
| --- | --- |
| Route | App Router di area Health Services/Billing Management; nama final mengikuti navigasi existing saat task FE |
| API | Axios service per resource; correlation/idempotency header dibuat sekali per command dan dipertahankan saat retry |
| Server state | Hook query dengan invalidate terarah setelah command; jangan menyimpan invoice finansial sebagai cache permanen |
| Client state | Redux hanya untuk lintas-step payment draft/shift context bila pola repo membenarkan; form lokal untuk filter/modal |
| Concurrency | Kirim version/ETag; `409` menampilkan “Data berubah, muat ulang sebelum melanjutkan.” |
| Pending provider | Poll/status refresh terukur; jangan resubmit tender baru otomatis |
| Error | Pesan Indonesia dari validation contract; correlation ID boleh ditampilkan, payload sensitif tidak |
| Money/time | Decimal diterima sebagai nilai kontrak; format `id-ID`; timestamp ditampilkan Asia/Jakarta dengan sumber UTC/offset |

Lokasi target mengikuti konvensi existing setelah discovery task: route di `src/app`, API di `src/lib/services`, hook di `src/lib/hooks`, Redux registration di `src/lib/state/store.jsx`, dan komponen domain di folder Billing Management. Semua berstatus Baru/Rencana; exact path adalah `DEV_DISCRETION` selama tidak mengubah route/API contract.

## Aksi per peran dan kewenangan UI

| Aksi | Kasir | Billing | Dokter | Finance | Kepala Kasir |
| --- | :---: | :---: | :---: | :---: | :---: |
| Lihat invoice/payment | Ya | Ya | Terbatas miliknya | Ya | Ya |
| Tambah tender/top-up | Ya | Tidak | Tidak | Lihat | Lihat |
| Void item eligible | Tidak | Ya sesuai source authority | Order miliknya melalui domain sumber | Tidak | Tidak |
| Input diskon master | Ya | Ya | Tidak | Ya | Tidak |
| Approve diskon dokter | Tidak | Tidak | Ya, miliknya | Exception saja | Tidak |
| Ajukan adjustment/write-off | Tidak | Ya | Tidak | Ya | Tidak |
| Approve Finance exception | Tidak | Tidak | Tidak | Ya, bukan maker | Tidak |
| Close shift | Ya, shift sendiri | Tidak | Tidak | Lihat | Review |
| Reopen/review variance | Tidak | Tidak | Tidak | Sesuai policy | Ya |
| Finalisasi | Tidak | Ya | Tidak | Lihat/exception | Tidak |

UI menyembunyikan aksi yang tidak berhak, tetapi backend tetap sumber otorisasi. Status `403` harus dijelaskan sebagai hak tidak tersedia, bukan error umum.

## Accessibility dan privacy

Status tidak boleh mengandalkan warna saja; sertakan label dan ikon/teks. Dialog approval memiliki fokus terkelola, keyboard navigation, label nominal yang dibacakan, dan konfirmasi eksplisit. Tabel menyediakan heading, pagination, empty/loading/error states. Cetak receipt hanya memuat data minimum. Mask nomor identitas/provider; jangan render clinical narrative yang tidak diperlukan.

## DEV_DISCRETION

Frontend boleh menentukan grid, urutan panel, komponen drawer/modal, breakpoints, ikon, debounce pencarian, skeleton, dan pembagian hook/component. Frontend tidak boleh mengubah arti status, rumus nominal, kapan OTC clear, siapa approver, idempotency, ataupun menyimpulkan settlement dari tampilan. Perubahan kontrak bisnis kembali ke Product/Domain.

## Acceptance dan dependency

Minimal dibuktikan oleh `BIL-AT-005` split tender parsial, `BIL-AT-007` progress rawat inap, `BIL-AT-012` doctor discount approval, `BIL-AT-016` shift variance, `BIL-AT-020` conflict, dan `BIL-AT-024` privacy/accessibility. Build menunggu approval task roadmap per slice serta pemulihan/penetapan kontrak governance frontend.
