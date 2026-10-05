# Laporan Perubahan Frontend — `FE-FIN-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-030` |
| Judul | Petugas dapat mencatat pembayaran langsung beserta metode, sumber dana, nomor rujukan, dan bukti — dan tahu lebih dulu berkas seperti apa yang diterima |
| Slice | `REV-14D` — `EPIC FIN-23` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "REV-14D dan REV-14E (frontend)" |
| Trace | `FR-FIN-160`..`162`, `FR-FIN-174`..`177`; `FIN-DEC-126`, `FIN-DEC-134`, `FIN-DEC-139`; `03-frontend-architecture.md` §20.1 |
| Contract version | `FIN-API-1.6` §F.3/F.8 (approved 2 Oktober 2026, Yasmin); `FIN-VAL-1.8` §G.1 (`FIN-VAL-214`..`223`); `FIN-PERM-1.8` §H.1/H.3 |
| Wewenang UI | `FIN-DES-092` dan `03-frontend-architecture.md` §20.1 adalah **invariant**, bukan pilihan rupa — nol tombol "Ganti Bukti", jenis/batas berkas disebut sebelum pemilihan, `503` dibedakan dari `400`/`413`. Tata letak, warna, ikon tetap `DEV_DISCRETION`. Penempatan menu tidak tersentuh (`FIN-OQ-079`, tidak relevan — nol layar baru pada task ini) |
| Dependency | `BE-FIN-077`, `BE-FIN-078` — **belum diimplementasikan** (status roadmap backend: `Direncanakan`). Task ini dikerjakan **lebih dulu**, sesuai urutan rilis terkunci (`FE-FIN-030` sebelum atau bersamaan `BE-FIN-077`/`078`) yang dicatat `02-frontend-roadmap.md` baris "Urutan rilis" |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 1 berkas baru + 2 diubah (skor 1); nol hook baru, 1 komponen presentasional baru dipakai 2 layar (skor 1); 1 endpoint baru dikonsumsi (`POST /transaction-proofs`, belum tersedia backend) + 2 endpoint existing diperluas payload-nya (skor 1); database — tidak relevan frontend (skor 0); keamanan/auth — nol resource/action baru, dua resource existing (`FinanceTransactionProof`, `MstDirectPaymentThreshold`) dipakai sesuai dokumen (skor 0); UI/workflow — dua layar transaksi existing berubah bentuk formnya (field wajib baru, unggah berkas, konfirmasi ambang) (skor 1). Total 4 → `MEDIUM` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk memverifikasi route/DTO as-is, dan ditulis hanya untuk laporan ini plus dua koreksi dokumentasi kontrak (lihat §1) |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/finance/receivable/payment/finance-payment-ar-view.jsx`, `src/components/view/finance/payable/payment/finance-payment-ap-view.jsx`, `src/components/view/finance/shared/transaction-proof-upload-field.jsx` **(baru)**. `NewQuilvianSystemBackend` — `docs/module-blueprints/finance-management/contracts/{api-contract,validation-matrix}.md` (koreksi path, lihat §1), roadmap, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `0b54fdce6` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `0e2567655` — branch `Yasmina` (read-only untuk source; ditulis untuk dokumentasi blueprint) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak target `FIN-API-1.6`, `npm run lint:errors` **PASS**. `npm run build` **NOT RUN** (menunggu konfirmasi eksplisit pengguna). Verifikasi jalur `400`/`409`/`413`/`503`/`404`/`422` **NOT FEASIBLE** — `BE-FIN-075`/`077`/`078` belum diimplementasikan, endpoint `POST /transaction-proofs` belum ada untuk dipanggil |

---

## 1. Keadaan yang ditemukan di awal — dan dua koreksi dokumentasi kontrak

Task ini dikerjakan **sebelum** `BE-FIN-074`..`078` selesai, sesuai wewenang eksplisit skill:
membangun terhadap kontrak target `FIN-API-1.6` yang sudah `approved`, bukan terhadap source
backend saat ini yang masih membuang field metode/sumber dana/rujukan.

**Verifikasi rute yang sebenarnya (sebelum menulis kode):**

- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` dibaca
  persis: route-nya `[Route("api/finance/receivable")]` + `[HttpPost("payment")]` =
  `POST api/finance/receivable/payment`, dengan `ReceivableId` sebagai **field body**
  (`RecordReceivablePaymentRequest`), bukan path parameter.
- `Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceApController.cs` dibaca persis:
  `POST api/finance/payable/payment`, `SupplierPayableId` juga field body.
- **Temuan:** `contracts/api-contract.md` §F.8 (ditulis sendiri pada pass desain revisi 15
  sebelumnya di sesi ini) mendokumentasikan path gaya REST ber-parameter
  (`POST /receivables/{id:guid}/payment`, `POST /supplier-payables/{id:guid}/direct-payment`)
  yang **tidak cocok** dengan controller yang benar-benar berjalan, dan juga tidak cocok dengan
  frontend existing yang sudah memanggil path literal di atas. `FIN-DES-085` sendiri hanya
  membahas penambahan field **body** pada endpoint yang sudah ada — ia tidak pernah menuntut
  path baru. Membangun frontend memakai path idealis itu akan memanggil route yang **tidak ada**
  (`404` murni), merusak satu-satunya jalur pembayaran langsung yang sedang berjalan — bertentangan
  langsung dengan alasan urutan rilis yang dikunci untuk task ini.
- **Koreksi yang dilakukan:** `contracts/api-contract.md` §F.8 dan `contracts/validation-matrix.md`
  baris `FIN-VAL-222`/`223` diperbarui memakai path literal yang benar
  (`POST api/finance/receivable/payment`, `POST api/finance/payable/payment`), beserta catatan
  penjelas supaya tidak terulang. Ini koreksi dokumentasi atas kesalahan penulisan sendiri pada
  pass sebelumnya, **bukan** perubahan keputusan desain — `FIN-DES-085` tidak disentuh.

**Verifikasi field dari kamus data (`erd/data-dictionary.md`), dibaca persis sebelum menulis
form:** `PaymentMethodCode` (`TRANSFER`|`CASH`), `FundingSourceType` (`BANK_ACCOUNT`|`CASH`,
**wajib** bila `PaymentMethodCode` terisi), `FundingSourceId` (kosong bila sumber dananya kas),
`ReferenceNumber`, `ProofId` (unique parsial, `FK Restrict` ke `FinTransactionProof`).

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR/AP Finance yang berwenang mencatat pembayaran langsung.

**Langkah normal (kedua layar — piutang dan utang):**

1. Petugas membuka modal pembayaran dari daftar tagihan/utang (tidak berubah).
2. Mengisi nominal (tidak berubah), memilih **Metode Pembayaran** (kini hanya Transfer/Tunai
   untuk jalur langsung), memilih **Sumber Dana** (Rekening Bank atau Kas — rekening bank hanya
   muncul bila sumber dananya rekening), mengisi **Nomor Referensi** (kini wajib), dan **mengunggah
   bukti pembayaran** (wajib, satu berkas, jenis dan batas ukuran disebut sebelum memilih berkas).
3. Berkas terunggah segera (panggilan `POST /transaction-proofs` tersendiri, bukan ikut payload
   pembayaran) — begitu berhasil, `ProofId` tersimpan dan keterangan berkas (nama, ukuran)
   ditampilkan. Bukti yang gagal terpakai (pembayarannya gagal sesudahnya) tetap sah dipakai
   ulang tanpa unggah ulang — sesuai `FIN-DES-092` ("bukti menggantung dibiarkan").
4. Klik "Simpan Pembayaran"/"Konfirmasi Pembayaran Langsung" memunculkan **konfirmasi ambang**
   (tanpa menyebut angkanya — `FIN-OQ-080` belum dijawab) sebelum benar-benar mengirim.
5. Backend (ketika `BE-FIN-077`/`078` sudah ada) menolak `404` (ambang belum ditetapkan), `422`
   (melewati ambang), `409`/`404` (bukti tidak valid) — pesannya ditampilkan apa adanya dari
   backend.

**Jalur tidak normal yang MUST ditangani (lihat §6 untuk status verifikasinya):**
`400`/`413`/`503` pada unggah bukti — tiga pesan berbeda, `503` menonaktifkan tombol unggah dan
mengarahkan ke administrator, bukan menyarankan ganti berkas.

**Yang sengaja TIDAK ditambahkan di layar:**

- **Tombol "Ganti Bukti"** — koreksi = batalkan/balik pembayaran lalu catat ulang beserta bukti
  baru, sesuai `FIN-DEC-139`.
- **Tautan unduh bukti pada riwayat mutasi** — dicari lebih dulu (`grep -rln "movements"` pada
  `src/app/finance` dan `src/components/view/finance`), **tidak ditemukan satu pun layar riwayat
  mutasi piutang/utang** di frontend saat ini. Brief task ini sendiri menulisnya bersyarat
  ("jika riwayat itu sudah ada layarnya") — karena layarnya belum ada, butir ini **tidak**
  dikerjakan pada task ini. `GET /receivables/{id}/movements` dan
  `GET /supplier-payables/{id}/movements` sudah tersedia backend (`BE-FIN-063`), sehingga ini
  murni menunggu task layar riwayat mutasi yang belum bernomor, bukan gap pada task ini.

---

## 3. Gerbang keputusan base component (dijalankan sebelum menulis JSX)

| Elemen | Bukti yang diperiksa | Status | Alasan |
| --- | --- | --- | --- |
| Pilihan Metode Pembayaran, Sumber Dana | `base-features/base-form-control.jsx` — `BaseNativeSelectField`, didesain tepat untuk "daftar pendek dan tetap" | `REUSE` | Cocok persis, dipakai apa adanya |
| Konfirmasi ambang sebelum kirim | `base-features/confirm-modal.jsx` — `ConfirmModal`, `variant="warning"`, `message`/`children` custom | `REUSE` | Cocok persis, dipakai apa adanya |
| Label/hint/error pada field unggah bukti | `base-features/base-form-control.jsx` — `BaseFormControl` (shell generik label+children+hint+error) | `REUSE` | Dipakai sebagai pembungkus input berkas, bukan ditulis ulang |
| Input `type="file"` beserta logika unggah, preview, dan state 503-lockout | `base-features/base-grouped-editor-field.jsx` (cabang `FILE_TYPES`, terikat penuh pada dispatcher config-driven satu pemakai: `doctor-schedules-form-view.jsx`) dan `features/UplodFoto/UploadPhotoField.jsx` (terikat `react-hook-form` + webcam, hardcode jenis gambar) | `NEW` (dibungkus `WRAP` atas `BaseFormControl`) | Kedua kandidat existing **tidak cocok**: yang pertama menuntut mengadopsi seluruh sistem form config-driven hanya untuk satu input berkas; yang kedua terikat `react-hook-form`+webcam dan jenis berkasnya salah (gambar saja, bukan PDF). Dibuat sebagai `transaction-proof-upload-field.jsx` di `src/components/view/finance/shared/` — dipakai **ulang oleh kedua layar** (AR dan AP) karena governed oleh **satu** kontrak bukti yang sama (`FIN-VAL-214`..`221`), bukan komponen generik lintas-domain |

Karena hanya satu elemen berstatus `NEW`/non-`REUSE`, dan ia bersifat teknis (tidak ada pilihan
rupa produk yang didelegasikan), tidak ada keputusan UI yang menunggu user untuk melanjutkan
bagian lain task — seluruh pekerjaan diselesaikan.

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `src/components/view/finance/shared/transaction-proof-upload-field.jsx` | Pemilih+pengunggah bukti bersama AR/AP: `POST /v1/corporate/finance-management/transaction-proofs` (multipart, field `file`), tampilkan jenis diterima sebelum pemilihan, status `idle`/`uploading`/`uploaded`/`blocked`, pesan berbeda untuk `413`/`503`/lainnya, tombol "Pilih berkas lain" (pra-kirim, bukan "Ganti Bukti" pasca-mutasi) |

### 4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `finance-payment-ar-view.jsx` | Tambah state `fundingSourceType`/`fundingSourceId`/`bankAccounts`/`proofId`/`showThresholdConfirm`; tambah `fetchBankAccounts`; metode pembayaran dipersempit ke Transfer/Tunai (GIRO/EDC dicabut — layar ini **tunggal** alurnya, nol flow lain yang bergantung padanya); field Sumber Dana + Rekening (kondisional); Nomor Referensi kini wajib; `TransactionProofUploadField` wajib; submit dipecah jadi validasi lokal → `ConfirmModal` ambang → `performSubmit` (payload menambah `fundingSourceType`/`fundingSourceId`/`proofId`, `referenceNumber` wajib); `ToastStack` ditambah prop `onClose` (prop aslinya; `onCloseToast` yang dipakai sebelumnya adalah no-op, ditemukan `FE-FIN-018`/`019` — diperbaiki sekalian karena file ini sudah disentuh penuh, bukan perubahan tersendiri) |
| `finance-payment-ap-view.jsx` | Tambah state `fundingSourceType`/`proofId`/`showThresholdConfirm` (bank account sudah ada sebelumnya, dipakai ulang sebagai `fundingSourceId`). **Alur kanonikal `FinPayment` (deposit retur, `FE-FIN-010`) TIDAK disentuh** — tetap submit langsung tanpa konfirmasi, tanpa field baru, karena kontrak target `FIN-API-1.6` §F.8 hanya mengubah endpoint direct-payment legacy, bukan endpoint `/submit` kanonikal. Alur **direct payment legacy** (`else`, tanpa draf) dipisah: validasi lokal (termasuk menolak `GIRO` dengan pesan eksplisit — lihat §5) → `ConfirmModal` ambang → `performDirectSubmit`. Field Sumber Dana + unggah bukti dirender **hanya** saat `!paymentDraft`; "Rekening Bank Sumber" kini kondisional (disembunyikan saat sumber dananya Kas, kecuali draf kanonikal aktif — perilaku draf tidak berubah) |

Nol berkas backend aplikasi disentuh (hanya dokumentasi blueprint, lihat §1).

---

## 5. Keputusan desain yang diambil sendiri (didokumentasikan, bukan ditanya ke user)

| Keputusan | Kenapa diputuskan sendiri, bukan diserahkan ke `AskUserQuestion` |
| --- | --- |
| AR: metode pembayaran dipersempit ke Transfer/Tunai, GIRO dan EDC dicabut | Layar AR **tidak punya** alur lain yang bergantung pada kedua opsi itu — mencabutnya murni menyamakan dengan kontrak target `TRANSFER`\|`CASH`, nol regresi fitur lain |
| AP: opsi GIRO **dipertahankan** di dropdown (bukan dicabut), tetapi submit langsung menolaknya dengan pesan eksplisit | Opsi GIRO dipakai **juga** oleh alur kanonikal `FinPayment` (deposit retur, `FE-FIN-010`, di luar cakupan task ini) saat membuat draf. Mencabut opsi ini dari dropdown akan meregresi fitur deposit retur yang sudah berjalan dan tidak diminta task ini. Menolaknya hanya pada jalur submit langsung (di mana `paymentDraft` masih kosong) mengisolasi dampaknya tepat pada kontrak yang berubah |
| Bukti diunggah segera saat dipilih (panggilan tersendiri), bukan dibundel ke payload pembayaran akhir | `03-frontend-architecture.md` §20.1 eksplisit menyebut "Keterangan bukti terunggah" sebagai wilayah layar tersendiri dengan sumber data `POST /transaction-proofs` response — menyiratkan unggah adalah langkah terpisah yang hasilnya ditampilkan sebelum pembayaran dikirim. Ini juga yang membuat bukti "menggantung" (gagal dipakai karena pembayaran ditolak sebab lain) tetap sah dipakai ulang tanpa unggah ulang, sesuai `FIN-DES-092` |
| Peringatan ambang ditampilkan **setiap kali** submit (bukan sekali per sesi) | `FIN-DEC-134` mencatat pemecahan pembayaran agar tetap di bawah ambang **tidak** tertangkap otomatis — peringatan berulang adalah satu-satunya kontrol manusia yang disebutkan desainnya. Tidak menampilkannya berulang melemahkan mitigasi yang justru dirancang `FIN-DEC-134` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`eslint . --quiet`, exit code 0) | `PASS` | Keluaran perintah |
| `npm run build` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna (standing instruction sesi ini) |
| Review diff/scope | 1 berkas baru + 2 diubah, sesuai cakupan task | `PASS` | `git status --short` |
| Grep anti-regresi UI (hex/rgb literal, `!important`) | Nol temuan baru pada ketiga file (satu `rgba()` yang ditemukan adalah backdrop modal pra-eksisting, tidak disentuh) | `PASS` | Keluaran grep |
| Review path endpoint terhadap controller sebenarnya | `POST api/finance/receivable/payment`, `POST api/finance/payable/payment` dikonfirmasi dari source, bukan dari dokumen idealis | `PASS` | §1 |
| Jalur `400`/`409`/`413`/`503` (unggah bukti) | — | `NOT FEASIBLE` | `POST /transaction-proofs` belum ada backend-nya (`BE-FIN-075` belum dikerjakan) |
| Jalur `404`/`422` (ambang) pada submit pembayaran | — | `NOT FEASIBLE` | `BE-FIN-077`/`078` belum ada; endpoint saat ini masih mengabaikan field baru |
| Verifikasi manual UI (buka modal, pilih opsi, lihat pesan GIRO) | — | `NOT FEASIBLE` | Server dev tidak dijalankan task ini |

**AUTOMATED TEST: NOT APPLICABLE** — repository ini tidak memakai Jest; menulis test baru
bersifat opsional, tidak diminta eksplisit pada task ini.

**MANUAL TEST: NOT FEASIBLE** — empat dari tujuh baris verifikasi bergantung pada backend
(`BE-FIN-075`/`077`/`078`) yang belum diimplementasikan; ini **sesuai ekspektasi** urutan rilis
yang dikunci task ini (frontend lebih dulu), bukan kegagalan verifikasi.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-030`) | Status | Bukti |
| --- | --- | --- |
| 2 layar pembayaran langsung diperbarui (piutang dan utang): metode, sumber dana, rujukan, bukti wajib, peringatan ambang | Terpenuhi | §4 |
| Tautan unduh bukti pada riwayat mutasi | **Tidak dikerjakan — bersyarat, layarnya belum ada** | §2 |
| Nol tombol "Ganti Bukti" | Terpenuhi | §4.2 |
| Jenis/batas berkas disebut sebelum pemilihan | Terpenuhi | `transaction-proof-upload-field.jsx` |
| `413` berbunyi ukuran melewati batas; `503` berbunyi konfigurasi belum lengkap dan menonaktifkan tombol | Terpenuhi (source); **belum terverifikasi runtime** | §6 — `NOT FEASIBLE` |
| `409` berbunyi "bukti sudah dipakai..."; tautan bukti orang lain tidak disembunyikan | Terpenuhi (source: pesan backend ditampilkan apa adanya pada submit; task ini tidak menambah tautan apa pun yang bisa menyembunyikan) | §4.2 |
| `npm run lint` dan `npm run build` PASS | **Sebagian** — lint PASS, build NOT RUN | §6 |
| Verifikasi manual keempat jalur tolak | **NOT FEASIBLE** — dependency backend belum ada | §6 |
| Urutan rilis terhadap `BE-FIN-077`/`078` tercatat | Terpenuhi | Metadata — Dependency |
| Laporan menyebut cara mencapai layar selama menu tertahan | Tidak relevan — task ini nol layar baru/route baru, kedua layar sudah terjangkau menu existing | — |

Task ini **belum** dapat ditandai ✅: `npm run build` belum dikonfirmasi, dan verifikasi jalur
tolak menunggu `BE-FIN-075`/`077`/`078`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sebagian besar verifikasi runtime **tidak dapat** dijalankan karena dependency backend memang belum ada — ini sesuai desain urutan rilis task ini, bukan kelalaian. Risiko residual: bila bentuk request `POST /transaction-proofs` atau respons `TransactionProofResponse` yang sebenarnya (saat `BE-FIN-075` dikerjakan) berbeda dari asumsi field `id`/`proofId`/`Id` pada `transaction-proof-upload-field.jsx`, penyesuaian kecil diperlukan saat integrasi |
| Masalah yang diketahui | Bug `onCloseToast` (no-op) pada `finance-payment-ar-view.jsx` diperbaiki sekalian (file sudah disentuh penuh). AP sudah benar sebelumnya. Permission legacy `/finance/receivable` dan isu serupa dari `FE-FIN-017`/`018` tidak tersentuh task ini |
| Dependency backend | `BE-FIN-075` (unggah bukti), `BE-FIN-077`/`078` (payload payment) — ketiganya `Direncanakan`, belum dikerjakan. Task ini sengaja mendahuluinya sesuai urutan rilis terkunci |
| Perubahan sampingan | Dua koreksi dokumentasi kontrak (`api-contract.md` §F.8, `validation-matrix.md` `FIN-VAL-222`/`223`) — koreksi kesalahan penulisan path pada pass desain sebelumnya, dijelaskan penuh §1 |
| Interupsi | `NONE` |
| Status Git (frontend) | `git status --short`: 2 berkas diubah, 1 berkas baru — seluruhnya sesuai §4 |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run build` dan mengonfirmasi; (2) `BE-FIN-074`..`078` (backend, prasyarat verifikasi runtime task ini); (3) `FE-FIN-031` (layar master ambang, tidak menunggu task ini) |
