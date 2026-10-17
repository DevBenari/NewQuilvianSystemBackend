# Laporan Perubahan Frontend — `FE-BKC-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-045` |
| Judul | Admin poli digantikan di Menu Pembayaran dan ringkasan rawat inap |
| Slice | Gelombang Revisi 2.3 (`BIL-CASH-001` rev 2.3), `frontend-roadmap.md` |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` — bagian Task `FE-BKC-045` |
| Trace | `FR-BKC-285`; `BKC-DEC-119`, `BKC-DEC-164`; `BKC-DES-084`–`086` (`BKC-DES-092` dicabut) |
| Contract version | `BIL-API-2.0` Bagian A (`isSupersededByInpatientAdmission`, `supersededByInpatientInvoiceId`, `supersededReasonCode`, `referredOutpatientAdminCreditedAmount`, `referredOutpatientAdminUnappliedCreditAmount`, `referredOutpatientAdminOrigins`), `BIL-TEST-2.1` |
| Wewenang UI | Terkunci: baris biaya administrasi poli tetap tampil dengan nominal Rp 0 dan label "Digantikan rawat inap" bila digantikan rawat inap; nomor tagihan rawat inap pengganti dimuat; penghapusan baris lama "Biaya Administrasi Rajal Dibatalkan"; penyajian kredit admin terpakai, rincian daftar tagihan poli asal, dan peringatan sisa kredit refund saat rawat inap difinalisasi. `DEV_DISCRETION`: tata letak, margin, padding, warna lencana status info |
| Dependency | `[BE] BE-BKC-084` — ✅ selesai (evaluasi penggantian dan pengkreditan biaya administrasi poli berbasis tautan aktif) |
| Klasifikasi | `LOW-MEDIUM` — penyelarasan presentasi biaya administrasi terhubung rawat jalan dan rawat inap |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — berkas komponen Menu Pembayaran (`detail-invoice-billing-view.jsx`), panel ringkasan rawat inap (`billing-inpatient-summary-panel.jsx`), konstanta badge (`billing-invoice-constants.js`), ekspor endpoint slice (`billing-invoice-slice.jsx`), dan test unit (`billing-superseded-admin-fee.test.mjs`). Laporan ini ditulis di `NewQuilvianSystemBackend` sesuai wewenang repositori tata kelola modul |
| Model | Antigravity |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ **SELESAI** |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task `FE-BKC-045` dikerjakan:
1. **Admin Poli Hilang Sepenuhnya Tanpa Jejak pada Tagihan Poli:**
   Pada layar Menu Pembayaran (`detail-invoice-billing-view.jsx:825`), baris biaya administrasi hanya dirender apabila `adminFeeApplied > 0`. Ketika pasien rawat jalan dirujuk masuk rawat inap dan biaya administrasinya digantikan oleh admisi ranap (`isSupersededByInpatientAdmission = true`), nilai `adminFeeApplied` menjadi `0`. Akibatnya, baris biaya administrasi poli tidak ditampilkan sama sekali, sehingga pasien dan kasir merasa heran mengapa invoice rawat jalan tersebut tampak seperti tidak pernah dikenakan biaya pendaftaran/administrasi.
2. **Ketiadaan Tautan Tagihan Pengganti:**
   Tidak ada informasi visual yang menghubungkan tagihan rawat jalan tersebut dengan tagihan rawat inap penggantinya (`supersededByInpatientInvoiceId`), sehingga kasir kesulitan menjelaskan alasan administrasi bernilai Rp 0.
3. **Pesan Usang "Biaya Administrasi Rajal Dibatalkan":**
   Pada panel ringkasan rawat inap (`billing-inpatient-summary-panel.jsx:78-80, 329-333`), antarmuka masih membaca properti usang `referredOutpatientAdminVoided` dan merender teks ambigu *"Biaya Administrasi Rajal Dibatalkan (Digantikan Biaya Admin Ranap)"*. Di sisi backend (`BIL-API-2.0`), field boolean ini telah dihapus karena penggantian dan pengkreditan kini diatur secara bertingkat dan berbasis bukti tautan riil.
4. **Ketiadaan Rincian Tagihan Asal dan Sisa Kredit:**
   Panel ringkasan rawat inap belum menampilkan daftar invoice poli asal (`referredOutpatientAdminOrigins`) beserta perlakuannya masing-masing (apakah dihapus di tagihan poli, dikreditkan, atau tanpa admin), serta belum menampilkan keterangan jika terdapat sisa kredit biaya administrasi (`referredOutpatientAdminUnappliedCreditAmount`) yang belum terserap oleh tagihan ranap dan akan menjadi hak *refund* pasien saat finalisasi tagihan.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna utama dari antarmuka ini adalah **Petugas Kasir Rumah Sakit**, **Petugas Penagihan & Piutang (Billing Staff)**, dan **Keluarga Pasien**:

### Skenario Nyata Rumah Sakit:
> **Contoh Kasus 1 (Poli Belum Bayar Dirujuk Ranap):**
> Pasien Anisa berobat ke Poliklinik Penyakit Dalam (Invoice `INV-RJ-001001`). Pada invoice poli tertera biaya administrasi rawat jalan Rp 25.000 (belum dibayar). Karena dokter menemukan indikasi darurat, Anisa langsung dirujuk masuk rawat inap (Admisi Ranap, Tagihan `INV-RANAP-002050`).
> Sesuai aturan bisnis rumah sakit (`BKC-DEC-119`), biaya administrasi rawat jalan yang belum dibayar tidak boleh ditagih lagi ke pasien karena sudah disatukan ke dalam formula administrasi rawat inap. 
> Saat kasir membuka tagihan poli Anisa di Menu Pembayaran, kasir melihat baris:
> `Biaya Administrasi` | `Rp 0` | Badge `[Digantikan rawat inap]`
> Catatan kecil di bawahnya: `Tagihan rawat inap: INV-RANAP-002050`.
> Kasir dapat dengan tenang menjelaskan kepada keluarga: *"Biaya administrasi poli dihapus menjadi Rp 0 karena dialihkan ke tagihan rawat inap Ibu Anisa nomor INV-RANAP-002050."*

> **Contoh Kasus 2 (Poli Sudah Bayar Dirujuk Ranap & Masuk Panel Ranap):**
> Pasien Budi berobat ke Poli Jantung (Invoice `INV-RJ-000880`) dan sudah membayar tunai administrasi poli Rp 30.000. Sore harinya kondisi memburuk dan Budi diopname di Bangsal Rawat Inap (Invoice `INV-RANAP-003000`).
> Pada invoice ranap Budi, formula administrasi 7% menghasilkan biaya Rp 150.000. Uang Rp 30.000 yang sudah dibayar Budi di poli dikreditkan sebagai pengurang tagihan ranap.
> Saat kasir membuka panel ringkasan rawat inap Budi:
> - Tertera baris: `Kredit Biaya Administrasi Rajal (Pengurang): - Rp 30.000`
> - Pada bagian tautan rawat jalan asal: `INV-RJ-000880 (CLOSED) — Dikreditkan Rp 30.000`
> Pasien tidak dirugikan karena uang administrasi yang telah dibayar di poli memotong total tagihan rawat inapnya.

> **Contoh Kasus 3 (Sisa Kredit Melebihi Admin Ranap):**
> Bila pasien memiliki kredit admin poli Rp 50.000, sedangkan tagihan ranap hanya menghasilkan biaya admin Rp 35.000, maka sisa kredit Rp 15.000 tidak hangus. Panel ranap menampilkan catatan informatif:
> *"Sisa kredit Rp 15.000 akan menjadi kredit refund saat tagihan difinalisasi."*

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang disentuh

| Berkas | Lokasi | Perubahan |
| --- | --- | --- |
| `billing-invoice-constants.js` | `QuilvianSystemFrontendDev/src/lib/hooks/...` | Menambahkan lencana `digantikan_rawat_inap: { label: "Digantikan rawat inap", className: "region-status-info" }` pada `BILLING_ITEM_COVERAGE_BADGE_CONFIG`. |
| `billing-invoice-slice.jsx` | `QuilvianSystemFrontendDev/src/lib/state/slice/...` | Mengekspor konstanta endpoint `BILLING_INVOICE_ENDPOINT` untuk penggunaan konsisten lintas komponen. |
| `detail-invoice-billing-view.jsx` | `QuilvianSystemFrontendDev/src/components/view/.../detail-billing/` | Menampilkan baris biaya administrasi jika `adminFeeApplied > 0 || isSupersededByInpatientAdmission`. Memuat nomor invoice rawat inap pengganti via `InstanceAxios`. Menampilkan label `"Digantikan rawat inap"`, nominal Rp 0, dan catatan nomor tagihan pengganti. Mengamankan kondisi tabel kosong agar tidak muncul ganda jika invoice hanya berisi admin yang digantikan. |
| `billing-inpatient-summary-panel.jsx` | `QuilvianSystemFrontendDev/src/components/view/.../detail/` | Menghapus pembacaan `referredAdminVoided`. Menambahkan pembacaan `referredAdminUnappliedCredit` dan `referredAdminOrigins`. Merender daftar tagihan rajal asal dengan status dan label perlakuan (`Admin dihapus di tagihan poli` / `Dikreditkan Rp ...` / `Tanpa admin`). Merender catatan informatif sisa kredit refund bila `referredAdminUnappliedCredit > 0`. |
| `billing-superseded-admin-fee.test.mjs` | `QuilvianSystemFrontendDev/tests/unit/` | Uji otomatis Acceptance Criteria `BIL-AT-225` dan `BIL-AT-226` berbasis Node Test Runner. |

---

## 4. Keputusan UI & Komponen

1. **Badge Status `digantikan_rawat_inap`:**
   Memakai `StatusBadge` standar dengan class `region-status-info` (warna cyan/teal khas Quilvian). Menghindari penggunaan warna peringatan merah/oranye karena penggantian admisi ranap merupakan proses normal operasional medis, bukan galat atau anomali.
2. **Keterangan Tagihan Pengganti (Resilient Loading):**
   Nomor tagihan rawat inap diambil secara asynchronous dari API `GET /v1/health-services/billing-management/billing/invoices/{id}`. Bila nomor tagihan belum selesai dimuat atau terjadi gangguan jaringan, antarmuka tetap menampilkan fallback aman `"Tagihan rawat inap"` tanpa memblokir interaksi kasir.
3. **Penyajian Daftar Asal pada Panel Ranap:**
   Daftar asal tagihan poli disajikan rapi dalam batas garis putus-putus (*dashed line*) di bawah rincian biaya admin dengan font sekunder `11px`, menyajikan nomor tagihan, status invoice (mis. `OPEN` atau `CLOSED`), serta status perlakuannya secara transparan.

---

## 5. Matriks Pengujian & Verifikasi

| ID Uji | Skenario Pengujian | Target Komponen / Endpoint | Hasil yang Diharapkan | Status |
| --- | --- | --- | --- | --- |
| `BIL-AT-225` | Tampilan admin poli yang digantikan rawat inap | `DetailInvoiceBillingView` | Baris admin fee tampil Rp 0 dengan badge "Digantikan rawat inap" dan catatan "Tagihan rawat inap: [nomor]"; jika nomor belum ada tampil fallback tanpa nomor. | ✅ PASS |
| `BIL-AT-226` | Tampilan kredit, daftar asal, dan sisa kredit ranap | `BillingInpatientSummaryPanel` | Menampilkan kredit terpakai, rincian tiap tagihan poli asal beserta perlakuannya, keterangan sisa kredit refund jika ada; tidak ada lagi teks "Biaya Administrasi Rajal Dibatalkan". | ✅ PASS |
| `MANUAL TEST` | Verifikasi regresi tagihan reguler tanpa penggantian | `DetailInvoiceBillingView` & `BillingInpatientSummaryPanel` | Tagihan poli biasa tetap menampilkan admin reguler; invoice ranap tanpa rujukan tetap tidak menampilkan kredit rajal. | ✅ PASS |
| `AUTOMATED TEST` | Pengujian unit otomatis (Node Test Runner) | `tests/unit/billing-superseded-admin-fee.test.mjs` | Seluruh 3 test case lulus 100% tanpa kegagalan (durasi ~54ms). | ✅ PASS |
| `AUTOMATED TEST` | Pengujian unit integrasi panel ranap | `tests/unit/billing-inpatient-summary-panel.test.mjs` | Seluruh 6 test case lulus 100% (durasi ~65ms). | ✅ PASS |
| `LINT TEST` | Pemeriksaan statis ESLint | `npx eslint` pada seluruh 4 berkas fitur | 0 error, 0 warning. | ✅ PASS |

---

## 6. Catatan Penutup

- Perubahan mematuhi seluruh konstitusi Quilvian dan panduan rekayasa frontend Next.js App Router.
- Tidak ada panggilan otomatis ke `npm run build` maupun migrasi database sesuai batasan perintah pengguna.
- Task `FE-BKC-045` dinyatakan **SELESAI** dan siap dilanjutkan ke task berikutnya.

