# Laporan Perubahan Frontend — `PAY-FE-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-12` |
| Judul | Layar Konfirmasi Pembayaran Manual (Payroll), selama belum ada koneksi bank |
| Slice | Payroll mandiri — pembayaran gaji dan honor |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 8.1 butir 5 dan 7 (jawaban pemilik 11 November 2026: "karena sistem ini belum dikoneksikan ke bank, maka tetap harus ada konfirmasi manual untuk menerbitkan slip gaji ... ketika testing saya ingin juga menerbitkan slip gaji, even itu belum terkoneksi dengan API bank"); permintaan lanjut pemilik 11 November 2026 ("bolehhh") |
| Contract version | Controller dan service Backend Payroll saat ini, dibaca dari source (`PayrollPaymentsController`, `PayrollPaymentService.AcknowledgeAsync`, `PayrollPaymentDtos.cs`). **Tidak ada perubahan kontrak dan backend tidak disentuh** |
| Wewenang UI | Pola layar Riwayat Pembayaran, aksi "Coba Ulang", dan modal klaim jasa medis yang sudah ada; komponen dasar yang sudah ada; teks Bahasa Indonesia |
| Dependency | Tidak ada yang menghalangi. `PAY-BE-11` (penanda sumber `MANUAL`) sudah ditulis (🟡, build menunggu pemilik) dan `PAY-FE-13` membuat layar ini mengirim `confirmationSource: "MANUAL"`; kunci berawalan `MANUAL-` tetap dikirim sebagai cadangan untuk backend lama (bagian 3.4) |
| Klasifikasi | `HEAVY` — hook dan util baru, satu modal baru, tindakan keuangan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; di repository backend hanya laporan ini dan baris register |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `03a57481f` ditambah perubahan `PAY-FE-11` yang belum di-commit (tidak disentuh task ini) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Selesai secara statis; menunggu pemilik menjalankan `npm run build` dan menguji alurnya (bagian 6).** 28 unit test baru PASS, suite penuh dan ESLint sama dengan baseline, modal dirender uji pada delapan keadaan. `npm run build`/`dev` oleh agen tidak dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Pembayaran gaji hanya bisa menjadi `Berhasil Dibayar` lewat endpoint ACK (`POST .../payroll-runs/{id}/payments/acknowledge`) yang dirancang untuk dipanggil Finance dengan hasil bank. Tidak ada layar frontend untuk endpoint itu (asersi test lama bahkan melarangnya), sedangkan belum ada koneksi bank. Akibatnya instruksi berstatus `Dijadwalkan` tidak pernah bergerak, dan karena kebijakan slip adalah "terbit setelah pembayaran terkonfirmasi", tombol Terbitkan Slip menahan semua penerima, termasuk saat pengujian.

## 2. Proses bisnis dari sisi pengguna

Petugas yang diberi hak `PayrollPayment : Acknowledge` pada Akses Role membuka **Riwayat Pembayaran**, memilih payroll run yang sudah dikirim ke Finance, lalu:

- **Satuan.** Pada baris berstatus Dijadwalkan atau Diproses, menekan **Konfirmasi Pembayaran**. Di dialog ia memilih hasil:
  - *Berhasil dibayar*: mengisi nomor referensi atau bukti transfer (wajib), dan tanggal dibayar (bawaan hari ini, tidak boleh masa depan). Nominal tampil terkunci sama dengan instruksi dan tidak bisa diubah.
  - *Gagal*: mengisi alasan (wajib); referensi opsional.
- **Massal.** Pada kepala halaman, menekan **Konfirmasi Pembayaran Massal**. Dialog menampilkan jumlah dan total nominal seluruh instruksi Dijadwalkan atau Diproses pada run itu; petugas mengisi satu nomor referensi bersama dan satu tanggal (pola transfer bank massal), hanya hasil Berhasil dibayar.

Setelah dikirim, dialog menampilkan hasil per instruksi dari Backend (Diterapkan, Sudah dikonfirmasi sebelumnya, atau Ditolak beserta alasannya), status payroll run, dan total dibayar. Daftar dimuat ulang. Sesudah itu tombol Terbitkan Slip pada layar Slip Gaji tidak lagi menahan penerima yang sudah Berhasil Dibayar.

**Contoh.** Run `PR-2026-11` berisi 120 instruksi Dijadwalkan. Keuangan mentransfer semuanya dalam satu berkas bank dengan referensi `BATCH-1111`. Petugas menekan Konfirmasi Pembayaran Massal, mengisi `BATCH-1111` dan tanggal transfer, lalu mengonfirmasi. Bila dua instruksi ditolak Backend (misalnya sudah berstatus Paid dengan referensi lain), dialog menampilkan kedua alasannya, 118 lainnya berstatus Berhasil Dibayar, dan peringatan "sebagian ditolak" muncul.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollPaymentsController.cs`, `PayrollPaymentService.cs` (`AcknowledgeAsync`, `GetPaymentsAsync`), `PayrollPaymentDtos.cs`, `PayrollRunStatuses.cs` (`IsPaymentStage`); `payroll-payment-history-view.jsx`, `payroll-payment-table-columns.jsx`, `payroll-dispatch-modal.jsx`, `payroll-medical-fee-claim-modal.jsx` (preseden dialog berformulir), `use-payroll-payment-history.js`, `payroll-payment-service.js`, `payroll-payment-utils.js`, `payroll-common-utils.js`, `payroll-constants.js`, `base-form-control.jsx`, `payroll-module-contract.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-constants.js` | Ditambah izin `payment.acknowledge`, konstanta konfirmasi manual (hasil, status yang boleh, batas panjang dan jumlah, awalan kunci, teks), kode hasil per item ACK. Teks `paymentNotPaid` diperbarui (bagian 3.4) |
| `src/lib/services/payroll/payroll-payment-service.js` | Ditambah `acknowledge(runId, body)`; komentar kepala yang keliru diperbarui |
| `src/utils/payroll/payroll-payment-utils.js` | Fungsi murni baru: kelayakan baris, validasi isian, tanggal ke ISO, pembentuk item dan request, pemecah 500 item, kunci per kiriman, normalisasi dan penggabungan hasil, klasifikasi hasil, pemilih item untuk dialog, pembaca semua halaman, penghitung total |
| `src/utils/payroll/payroll-common-utils.js` | `newIdempotencyKey(prefix)` |
| `src/lib/hooks/payroll/use-payroll-payment-history.js` | Grup `manualConfirmation` (izin, target satuan atau massal, pemuatan, pengiriman, hasil, galat) beserta penguncian kirim ganda dan satu kunci per niat |
| `src/components/view/payroll/payment/payroll-manual-confirmation-modal.jsx` | **Baru** — dialog konfirmasi (pembungkus tipis `ConfirmModal` dan kontrol form dasar, mengikuti `PayrollDispatchModal`) |
| `src/components/view/payroll/payment/payroll-payment-table-columns.jsx` | Tombol "Konfirmasi Pembayaran" pada kolom Aksi; tombol "Coba Ulang" tidak berubah; lebar minimum kolom 150 menjadi 190 |
| `src/components/view/payroll/payment/payroll-payment-history-view.jsx` | Tombol massal pada kepala halaman, pemasangan dialog, komentar kepala |
| `tests/unit/payroll-module-contract.test.mjs` | **Asersi lama diganti** (bagian 6) |
| `tests/unit/payroll-manual-payment-confirmation.test.mjs` | **Baru** — 28 test |

Tidak ada berkas CSS yang dibuat atau diubah; dialog memakai `styles.modalStack` yang sudah ada.

### 3.3 Kontrak Backend yang dibaca (tidak diubah)

| Hal | Isi |
| --- | --- |
| Endpoint | `POST /api/v1/corporate/payroll-management/payroll-runs/{runId}/payments/acknowledge`, izin `PayrollPayment : Acknowledge` |
| Body | `{ idempotencyKey?, financePaymentBatchId?, items: [1..500] { paymentNumber, status: "Paid" or "Failed", paidAmount?, paymentReferenceNumber? (<=100), financePaymentId?, paidAt?, failureReason? (<=500) } }` |
| Aturan | Run harus tahap pembayaran (`SentToFinance`, `PartiallyPaid`, `Paid`). `Paid` wajib referensi dan nominal sama dengan instruksi. Hanya `Scheduled` atau `Processing` yang bisa menjadi `Paid` atau `Failed`. `Failed` wajib alasan. Nomor ganda dalam satu kiriman ditolak |
| Respons | HTTP 200 dengan `runStatus`, `totalPaidAmount`, hitungan, dan `items[] { paymentNumber, outcome: Diterapkan, Idempoten, atau Ditolak, paymentStatus, message }`. **Penolakan item tidak mengubah status HTTP**, jadi layar membaca hasil per item |
| Batas daftar | `GetPaymentsAsync` membatasi `pageSize` 1 sampai 100 (dibaca di source) |

### 3.4 Keputusan dan tafsiran agen (perlu diketahui pemilik)

| Hal | Isi |
| --- | --- |
| Tafsiran "satu atau beberapa instruksi" | Satuan per baris, dan **massal per payroll run** (semua Dijadwalkan atau Diproses, satu referensi). Pemilihan baris dengan kotak centang **tidak** dibuat (tidak diminta, dan dukungan `DataTable` belum dikaji). Massal hanya hasil Berhasil dibayar; Gagal hanya per baris |
| Pengumpulan massal | Memanggil daftar pembayaran per status (Dijadwalkan lalu Diproses, karena filter Backend hanya menerima satu status), seluruh halaman dengan `pageSize` 100, lalu mengirim per paling banyak 500 item. Lebih dari 100 halaman ditolak dengan pesan |
| Penanda sumber sementara | `idempotencyKey` dikirim `MANUAL-<UUID>` (`MANUAL-<UUID>#2of3` bila lebih dari satu kiriman). Backend mencatatnya pada audit dan catatan persetujuan run `PaymentAck`, sehingga konfirmasi manual dapat dibedakan dari ACK Finance **tanpa mengubah backend**. Ini pengganti sementara untuk penanda `MANUAL`/`FINANCE_ACK` yang dijanjikan `PAY-BE-11` **Pembaruan `PAY-FE-13` (11 November 2026):** layar ini kini juga mengirim `confirmationSource: "MANUAL"` (dibaca backend `PAY-BE-11`); kunci berawalan `MANUAL-` tetap dikirim sebagai cadangan untuk backend lama. |
| Satu kunci per niat | Kunci dibuat sekali ketika petugas mengirim, dipakai ulang bila kiriman gagal di jaringan lalu diulang, dan dibuang setelah hasil sukses penuh atau dialog ditutup. Idempotensi sebenarnya ada di Backend per item (instruksi yang sudah Paid dengan referensi sama dibaca "Idempoten") |
| Hasil di luar sukses penuh | Item Ditolak, hasil kosong, dan hasil di luar tiga nilai yang dikenal **tidak pernah dibaca sebagai sukses**; dialog hasil memakai nada peringatan atau galat dan daftar dimuat ulang bila sebagian sudah diterapkan |
| Tanggal dibayar | Pilihan tanggal "hari ini" memakai waktu sekarang; tanggal lampau dikirim pukul 12.00 waktu lokal agar tidak bergeser hari saat dikonversi ke UTC |
| Teks `paymentNotPaid` | Sebelumnya: "hanya berasal dari konfirmasi hasil bank oleh Finance". Kini: "berasal dari konfirmasi hasil bank oleh Finance atau, selama belum ada koneksi bank, dari konfirmasi manual oleh petugas yang berwenang". Teks ini juga tampil di dialog pengiriman ke Finance |

### 3.5 Keputusan komponen dasar

**UI GATE: 7 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 1, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol per baris dan tombol massal pada kepala halaman | `BaseButton` | Sudah dipakai pada kolom Aksi dan `actions` Hero di berkas yang sama; varian `primary` dipakai layar Payroll lain | REUSE | — |
| Pilihan hasil | `BaseSelectField` | Dipakai `payroll-medical-fee-claim-modal.jsx` | REUSE | — |
| Nomor referensi | `BaseTextField` | Dukungan `maxLength`, `required`, `error` dibaca di `base-form-control.jsx` | REUSE | — |
| Tanggal dibayar | `BaseDateField` | Dukungan batas `max` (opt-in) dibaca di `base-form-control.jsx`; dipakai `purchase-order-detail-view.jsx` | REUSE | — |
| Alasan gagal | `BaseTextAreaField` | Dipakai `payroll-dispatch-modal.jsx` | REUSE | — |
| Pesan dan hasil per item | `InformationAlert` | Dipakai kedua dialog Payroll yang ada | REUSE | — |
| Dialog berformulir | `ConfirmModal` | Preseden: `PayrollDispatchModal` dan `PayrollMedicalFeeClaimModal` (pembungkus domain tipis di atas `ConfirmModal`) | **WRAP** | A |

Pilihan untuk elemen dialog (bukan `REUSE`):

- **A. WRAP: `PayrollManualConfirmationModal` di `components/view/payroll/payment`, mengikuti dua dialog Payroll yang sudah ada. — Rekomendasi.** Konsisten secara visual dengan dialog lain, tanpa mengubah komponen dasar, biaya terkecil, risiko regresi ke modul lain nol.
- B. EXTEND `ConfirmModal` dengan slot formulir dan validasi. Menyeragamkan semua dialog berformulir, tetapi mengubah komponen yang dipakai banyak modul sehingga risiko regresi lintas modul dan butuh persetujuan.
- C. Komponen dialog baru. Paling bebas, tetapi menggandakan kontrak modal dan berisiko menyimpang dari token.

Tidak ada jawaban dari pemilik atas pilihan ini; dijalankan **A** sesuai ketentuan "tanpa jawaban". Tidak ada elemen berstatus `NEW`.

### 3.6 Kepatuhan arsitektur frontend

Alur `view → hook → service → InstanceAxios`; tidak ada Axios di view atau util. Konstanta di `src/lib/constants/payroll/`, fungsi murni di `src/utils/payroll/`, tanpa slice Redux baru (mengikuti hook payroll yang sudah ada). Hak akses diambil dari `usePermission("PayrollPayment", "Acknowledge")`, tanpa peran, departemen, atau `UserType` yang ditulis di kode.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Tanpa hak `Acknowledge` | Tombol satuan dan massal tidak muncul |
| Tidak ada instruksi menunggu | Tombol massal tidak muncul (`scheduledCount` nol); baris yang sudah Paid, Failed, dan lainnya tidak punya tombol konfirmasi |
| Memuat daftar massal | Dialog menampilkan "Memuat daftar instruksi yang menunggu konfirmasi..."; tombol konfirmasi nonaktif |
| Daftar massal kosong atau gagal dimuat | Pesan di dalam dialog; tombol konfirmasi nonaktif |
| Isian belum valid | Pesan per bidang setelah percobaan kirim pertama (referensi wajib dan maks. 100, tanggal wajib dan tidak masa depan, alasan wajib dan maks. 500); tidak ada permintaan dikirim |
| Mengirim | Tombol berstatus memuat; tutup dan kirim ganda dikunci |
| Gagal di jaringan atau server | Pesan galat di dalam dialog; isian dipertahankan; bila sebagian kiriman sudah diproses, pesan menyebut jumlahnya |
| Sukses penuh | Toast sukses, dialog hasil (nada sukses), daftar dimuat ulang |
| Sebagian atau seluruhnya ditolak | Dialog hasil bernada peringatan atau galat dengan alasan per instruksi (yang ditolak lebih dulu; hasil massal tidak menampilkan ratusan baris "Diterapkan"); daftar dimuat ulang bila ada yang diterapkan |
| Respons kosong | Pesan "Server tidak mengembalikan hasil per instruksi..." dan dialog tetap terbuka; tidak dianggap sukses |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Payment**.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{runId}/payments` | Daftar instruksi (sudah ada); juga dibaca per status untuk konfirmasi massal | `PayrollPayment : Read` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{runId}/payments/acknowledge` | Konfirmasi pembayaran manual (Berhasil dibayar atau Gagal) | `PayrollPayment : Acknowledge` |

---

## 6. Verifikasi

Baseline diambil sebelum mengubah, pada working tree yang sudah memuat `PAY-FE-11`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --test` pada `payroll-manual-payment-confirmation.test.mjs` dan `payroll-module-contract.test.mjs` | 40 test, 40 lulus (28 baru) | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | Sebelum: 4728 test, 4681 lulus, 47 gagal. Sesudah: 4756 test, 4709 lulus, 47 gagal. Daftar nama yang gagal **identik** (`diff` kosong); tidak ada yang terkait Payroll | `EXISTING / ENVIRONMENT ISSUE` (0 baru) | Keluaran perintah; `diff` daftar nama |
| `npx eslint` pada 8 berkas lama yang diubah | Sebelum dan sesudah sama: 0 galat, 2 peringatan (`react-hooks/set-state-in-effect` pada dua efek lama di hook); pesan identik | `PASS` (0 galat baru) | Keluaran ESLint format JSON |
| `npx eslint` pada 2 berkas baru (dialog dan test) | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| Resolver impor statis (6851 berkas `src` dan `tests`) | 61 impor tak terselesaikan sebelum dan sesudah, himpunan identik; seluruhnya impor lama yang tak terkait | `PASS` | `diff` hasil sebelum dan sesudah |
| Render sisi server dialog (`react-dom/server`, pemuat sekali pakai di scratchpad dengan `ConfirmModal` diganti pengganti sederhana; **bukan** bagian repo) pada 8 keadaan: satuan, massal memuat, massal terisi, massal kosong, galat, hasil sebagian, hasil sukses, tertutup | Semua dirender tanpa galat; teks, nominal terkunci, label tombol (`Ya, Konfirmasi 2 Instruksi`), dan alasan penolakan tampil seperti dirancang | `PASS` | Keluaran render |
| Grep anti-regresi pada JSX baru dan baris tambahan (tombol mentah, `.btn`, tipografi Bootstrap, warna literal, `!important`, `style={{}}`, `<table`) | Kosong; tidak ada berkas CSS berubah | `PASS` | Keluaran `grep`, `git status` |
| Stage | 0 berkas ter-stage; perubahan `PAY-FE-11` tidak disentuh | `PASS` | `git diff --cached` |
| `npm run build` / `npm run dev` | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | — |

**Penggantian asersi test lama (eksplisit).** `payroll-module-contract.test.mjs` memuat `assert.ok(!/acknowledge/i.test(payments), "ACK hasil bank dipanggil Finance, bukan layar Payroll")`. Asersi itu mengunci keputusan lama yang dibalik pemilik pada 11 November 2026. Diganti dengan asersi yang mengunci perilaku baru: service memanggil `/payments/acknowledge`; nama izin `Acknowledge` ada; hook memakai `PERMISSION.acknowledge` dan tidak memuat penentu kewenangan yang ditulis di kode (`IsInRole`, `roleName`, `userType`); tombol baris hanya untuk `canConfirm && isPaymentConfirmable(item)`; dialog menampilkan hasil per item. Asersi lain pada berkas itu tidak diubah.

MANUAL TEST: `NOT FEASIBLE` oleh agen — tidak ada server atau peramban yang boleh dijalankan.

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 4756 test, hasil identik dengan baseline untuk yang lama, 28 baru lulus.

**Langkah uji untuk pemilik** (setelah `npm run build` atau `npm run dev`; hentikan server lama dulu):

| No | Langkah | Yang harus terlihat |
| ---: | --- | --- |
| a | Pada Akses Role, beri hak `PayrollPayment : Acknowledge` ke akun uji. Buka Riwayat Pembayaran dengan akun **tanpa** hak itu lebih dulu | Tidak ada tombol Konfirmasi Pembayaran maupun Massal |
| b | Dengan akun berhak, pilih run berstatus Dikirim ke Finance, klik **Konfirmasi Pembayaran** pada satu baris, hasil Berhasil dibayar, isi referensi | Status baris menjadi Berhasil Dibayar; kartu ringkasan "Berhasil Dibayar" dan "Belum Dibayar" bergerak; dialog hasil menampilkan "Diterapkan" |
| c | Buka dialog lagi pada baris lain: coba kosongkan referensi lalu konfirmasi; ubah tanggal ke masa depan | Pesan wajib per bidang; tidak ada permintaan terkirim. Nominal hanya teks (tidak bisa diubah) |
| d | Pilih hasil Gagal dengan alasan | Status baris Gagal dan tombol Coba Ulang muncul |
| e | Klik **Konfirmasi Pembayaran Massal**, isi satu referensi | Dialog menampilkan jumlah dan total nominal; setelah dikirim seluruh instruksi Berhasil Dibayar; status run menjadi Dibayar Sebagian atau Sudah Dibayar sesuai Backend |
| f | Ulangi konfirmasi pada instruksi yang sudah Berhasil Dibayar dengan referensi yang **sama** lewat jalur lain, dan dengan referensi **berbeda** | Referensi sama: "Sudah dikonfirmasi sebelumnya" (tidak ganda). Referensi berbeda: Ditolak dengan alasan dari Backend |
| g | Buka Slip Gaji run itu, klik Terbitkan Slip | Penerima yang sudah Berhasil Dibayar tidak lagi ditahan |
| h | Periksa log audit atau catatan persetujuan run `PaymentAck` | Kunci berawalan `MANUAL-` tercatat |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Petugas berhak dapat mengonfirmasi satu instruksi Berhasil dibayar atau Gagal; nominal terkunci | Terpenuhi (statis) | Bagian 3.2, 6; test pembentuk item |
| Petugas berhak dapat mengonfirmasi seluruh instruksi menunggu pada satu run dengan satu referensi | Terpenuhi (statis) | Test pengumpul halaman dan pemecah 500; render |
| Hasil per item dibaca; item ditolak tidak dibaca sebagai sukses | Terpenuhi | Test klasifikasi dan penggabungan |
| Tombol hanya muncul bagi yang berhak; tanpa peran di kode | Terpenuhi | Asersi kontrak; `usePermission` |
| Kirim ganda dikunci; satu kunci per niat; penanda `MANUAL-` | Terpenuhi | Test sumber hook; test kunci |
| Tidak ada komponen baru selain pembungkus dialog, tidak ada CSS, tombol hanya `BaseButton` | Terpenuhi | Tabel 3.5; grep |
| Tidak ada regresi test, ESLint, dan impor | Terpenuhi | Bagian 6 |
| Terbukti pada build dan layar (slip terbit setelah konfirmasi) | **Belum** | Menunggu pemilik (langkah a sampai h) |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Peringatan pemisahan tugas** | Orang yang berhak `FinanceHandoff` (mengirim instruksi) dan `Acknowledge` (menandai dibayar) sebaiknya **tidak sama** pada produksi. Pemberian hak ada pada admin lewat Akses Role, bukan kode. Konfirmasi manual tidak memeriksa apakah uang benar-benar masuk; ia bergantung pada kejujuran dan kontrol petugas |
| Status tidak bisa dibatalkan dari layar | Setelah `Paid`, Backend menolak menandainya Failed atau mengganti referensi, dan layar ini tidak menyediakan koreksi. Dialog menuliskannya. Cara memperbaiki konfirmasi yang salah belum dirancang |
| Komentar backend yang kini usang | Komentar kelas `PayrollPaymentsController` menyatakan "tidak ada aksi di layar Payroll yang menandai pembayaran lunas". Backend tidak disentuh task ini; komentar itu layak diperbarui pada task backend berikutnya (`PAY-BE-11`) |
| Pengumpulan massal | Run dengan ratusan instruksi memicu beberapa permintaan daftar berurutan sebelum dialog siap. Tidak ada kemajuan per halaman yang ditampilkan |
| Kewenangan massal | Konfirmasi massal menandai semua instruksi menunggu pada run, termasuk yang tidak tampil pada halaman saat ini. Dialog menyebutkannya dan menampilkan jumlah dan total sebelum dikirim |
| Masalah yang diketahui | Dua peringatan ESLint lama pada hook (`react-hooks/set-state-in-effect`) tidak diperbaiki karena di luar cakupan |
| Dependency backend | Tidak ada yang menghalangi. `PAY-BE-11` (penanda sumber dan status bayar pada potongan) sudah ditulis dan dipakai oleh `PAY-FE-13` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | FE: `M` 8 berkas (`payroll-constants.js`, `payroll-payment-service.js`, `payroll-payment-utils.js`, `payroll-common-utils.js`, `use-payroll-payment-history.js`, tabel kolom, view, dan `payroll-module-contract.test.mjs`), `??` 2 berkas baru (dialog dan test), di atas perubahan `PAY-FE-11` yang belum di-commit. Tidak ada stage atau commit |
| Langkah berikutnya | Pemilik: `npm run build` dan langkah uji a sampai h; setelah itu laporan ini naik ke ✅. `PAY-BE-11` dan `PAY-FE-13` (penanda status bayar dan peringatan pada potongan piutang dan rekonsiliasi) sudah dikerjakan secara statis; lihat laporannya |
