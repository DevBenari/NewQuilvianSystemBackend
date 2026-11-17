# Laporan Perubahan Frontend — `FE-BKC-FIX-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-015` (ad-hoc, di luar roadmap; nomor berikutnya setelah `FE-BKC-FIX-014`). Pasangan backend: [`BE-BKC-FIX-025`](../backend/BE-BKC-FIX-025.md) |
| Judul | Menu Pembayaran tunai: uang tunai diterima, kembalian otomatis, konfirmasi penyerahan, dan kwitansi yang memuat ketiga nilai |
| Slice | Lanjutan Menu Pembayaran / split payment / Dokumen Kasir, tanpa task roadmap |
| Roadmap | `NOT APPLICABLE` — task ad-hoc atas permintaan langsung pemilik modul (master prompt 17 November 2026). Tidak ada kartu roadmap yang dapat ditandai; `roadmap/**` tidak diubah |
| Trace | `NOT APPLICABLE` |
| Contract version | Mengonsumsi perubahan aditif `BE-BKC-FIX-025` (`CreateTenderRequest.cashTenderedAmount`/`cashChangeGivenConfirmed`; `TenderResponse`, `PaymentHistoryTenderResponse`, `EncounterPaymentHistoryEntryResponse` membawa `cashTenderedAmount`/`cashChangeAmount`). Kontrak itu **belum diterapkan di database mana pun** |
| Wewenang UI | Master prompt B.1–B.12: field tunai kondisional di modal dan kartu baris, rincian di kwitansi; gunakan base component dan pola styling existing, tanpa komponen global baru |
| Dependency | `BE-BKC-FIX-025` — source selesai, **skema database belum disiapkan dan build belum dijalankan** (`🟡`). Tanpa skema dan backend baru, frontend ini tidak mencatat kembalian (lihat risiko) |
| Klasifikasi | `MEDIUM` — skor 7 (repository 0, diperiksa 1, diubah 2, logika 1, kontrak 1, database 0, auth 0, UI 2) |
| Task mode | `CROSS-REPO` (kedua repository diberi wewenang tulis eksplisit). Backend dikerjakan lebih dulu |
| Target tulis | `QuilvianSystemFrontendDev` (source) + berkas laporan ini di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `baad7dd6246f7530d8cdefbe1e6e174502f7ba71` (branch `yasmina`) + working tree; belum di-commit |
| Commit backend yang dijadikan rujukan | `dbac9acc3d8640efb6e6dac892f7024b8ebabd7b` (branch `Yasmina`) + working tree `BE-BKC-FIX-025` |
| Tanggal | 17 November 2026 |
| Status | 🟡 **Source selesai (termasuk addendum keputusan final 17 November 2026, bagian 9); logika murni terbukti lewat 39 tes unit; tampilan belum pernah dibuka di peramban.** `npm run build` dan `npm run dev` **tidak dijalankan** (larangan eksplisit master prompt) sehingga `VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE`. Pemeriksa `check-css-usage.mjs` melaporkan 8 temuan lama (bukan dari baris yang saya tambahkan) sehingga targetnya "0 temuan" belum tercapai. Tidak dapat `✅` |

---

## 1. Keadaan yang ditemukan di awal

- Kartu pembayaran draft (`PaymentAllocationRow`) hanya punya satu isian uang: "Nominal" (neto). Tidak ada tempat mengisi uang fisik atau melihat kembalian.
- `submitOneRow` mengirim `amount` saja ke `addBillingTender`; `buildTenderSignature` (identitas percobaan) tidak mengenal uang fisik.
- `evaluatePaymentSubmission` menolak total baris yang melebihi sisa tagihan di luar toleransi pembulatan tunai, sehingga uang fisik lebih besar tidak mungkin dimasukkan sebagai nominal.
- `KwitansiDocument` menampilkan satu baris "Jumlah Diterima / Amount Received" dari `tenderAmount`. Tender yang dimuat ulang oleh Dokumen Kasir (`settlement.settlements` dari backend, di-spread `{...t}`) sudah membawa field apa pun yang dikirim backend, jadi data kwitansi tidak bergantung pada state layar.
- Bukti: pembacaan `payment-allocation-row.jsx`, `add-tender-modal.jsx`, `billing-split-payment-rows.js`, `use-billing-settlement.js`, `billing-settlement-slice.jsx`, `kwitansi-document.jsx`, `use-dokumen-kasir-page.js`.

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** kasir dengan shift aktif. **Pemicu:** menekan Proses Pembayaran pada Menu Pembayaran pasien yang membayar tunai.

**Langkah**

1. Kasir memilih metode **Tunai** pada baris pembayaran (lewat popup Pilih Metode atau kartu baris). Isian lama tidak berubah: "Nominal Pembayaran Tagihan" tetap nominal neto.
2. Muncul blok khusus tunai: **Uang Tunai Diterima** (WAJIB diisi; tombol **Uang Pas** mengisinya sama dengan nominal), **Kembalian** (otomatis, read-only), dan — bila kembalian lebih dari nol — kotak centang **"Kembalian sudah diserahkan kepada pasien"**.
3. Kasir mengisi uang yang diterima. Kembalian dihitung langsung dari satu fungsi yang sama (`computeCashChange`) di popup, kartu baris, ringkasan, dan payload.
4. Tombol Proses Pembayaran baru aktif bila uang tidak kurang dari nominal dan kembalian positif sudah dicentang. Alasan tombol terkunci tampil di bawah tombol dan pada barisnya.
5. Saat diproses, frontend mengirim `amount` (neto), `cashTenderedAmount`, dan `cashChangeGivenConfirmed`. Kembalian **tidak** dikirim; backend yang menghitungnya.
6. Setelah berhasil, notifikasi menyebut uang diterima dan kembalian dari **nilai tersimpan di backend**. Kwitansi dan Riwayat Pembayaran membacanya dari backend juga.

**Contoh berangka**

| Skenario | Nominal pembayaran tagihan | Uang diterima | Kembalian | Yang dihitung sebagai alokasi |
| --- | ---: | ---: | ---: | ---: |
| Tunai pas (kolom uang diterima kosong) | 75.000 | — (dianggap 75.000) | 0 | 75.000 |
| Tagihan Rp75.000, tunai Rp100.000 | 75.000 | 100.000 | 25.000 | 75.000 |
| Tagihan Rp100.000: QRIS 20.000 + tunai neto 80.000 dari 100.000 | 20.000 dan 80.000 | — dan 100.000 | — dan 20.000 | 100.000 |
| Tagihan Rp100.000, tunai neto 50.000 dari 100.000 | 50.000 | 100.000 | 50.000 | 50.000 (sisa 50.000) |

**Jalur tidak normal**

| Kejadian | Yang dilihat kasir |
| --- | --- |
| Uang diterima kurang dari nominal | Pesan pada baris dan kolom: "Uang tunai diterima kurang dari nominal pembayaran tagihan."; tombol terkunci |
| Uang diterima belum diisi | "Uang tunai diterima wajib diisi. Tekan Uang Pas bila pasien membayar pas."; tombol terkunci |
| Uang diterima diisi 0 atau bukan angka | "Uang tunai diterima harus lebih besar dari nol." |
| Kembalian positif belum dicentang | Tombol terkunci dengan alasan "centang konfirmasi kembalian … sudah diserahkan kepada pasien" |
| Nominal atau uang diterima diubah setelah dicentang | Centang gugur otomatis (konfirmasi hanya berlaku untuk nominal kembalian yang dikonfirmasi) |
| Metode diganti dari Tunai ke QRIS/Transfer | Uang diterima dan konfirmasi dihapus; nominal alokasi tidak berubah; field tunai hilang |
| Metode diganti dari non-tunai ke Tunai | Isian uang diterima kosong (wajib diisi atau Uang Pas); nominal alokasi tidak berubah |
| Backend menolak (422/409) | Pesan backend tampil pada baris yang gagal; baris dapat diperbaiki lalu dikirim ulang |
| Percobaan ulang dengan uang fisik berbeda | Dianggap percobaan baru (kunci idempotensi baru); retry yang sama persis tetap replay |

**Keputusan UX (SUDAH DIPUTUSKAN pemilik, 17 November 2026):** kolom "Uang Tunai Diterima" **wajib diisi** pada setiap pembayaran tunai, termasuk uang pas, dengan tombol "Uang Pas" sebagai jalan pintas. Pilihan sementara "kosong = pas" yang tertulis pada versi pertama laporan ini sudah dicabut.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; `payment-allocation-row.jsx`, `add-tender-modal.jsx`, `menu-pembayaran-view.jsx`, `kwitansi-document.jsx`, `dokumen-kasir-view.jsx`; `billing-split-payment-rows.js`, `use-billing-settlement.js`, `use-menu-pembayaran.js`, `use-dokumen-kasir-page.js`, `billing-payment-outcome.js`, `billing-deposit-draft.js`, `billing-invoice-constants.js`, `billing-settlement-slice.jsx`; `menu-pembayaran.module.css`; `billing-split-payment-rows.test.mjs`, `billing-settlement-active-selection.test.mjs`, dan `billing-single-payment-action.test.mjs` (dibaca isinya); 17 berkas tes billing lain hanya dijalankan; seluruh `rules/frontend/*` pada suite Skill.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-split-payment-rows.js` | Baris baru membawa `cashTenderedAmount` dan `cashChangeConfirmedAmount`. Fungsi murni baru: `isCashPaymentMethod` (hanya flag `isCash`, sama dengan backend), `computeCashChange` (satu-satunya perhitungan kembalian), `isCashChangeConfirmed`, `buildCashTenderPayload`, `summarizeCashDrafts`, `applyPaymentMethodChange`. `getRowProblem` menambah dua masalah baris (`CASH_TENDERED`, `CASH_CHANGE_CONFIRMATION`). `buildTenderSignature` menambah segmen uang diterima hanya bila diisi |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-settlement.js` | Ganti metode memakai `applyPaymentMethodChange` (reset data tunai); `submitOneRow` menyertakan payload tunai; ringkasan pengiriman mengumpulkan uang diterima/kembalian **tersimpan** dari respons backend; `cashDraftSummary` untuk ringkasan |
| `src/lib/state/slice/health-services/billing-management/billing-settlement-slice.jsx` | `addBillingTender` mengirim `cashTenderedAmount` dan `cashChangeGivenConfirmed` hanya bila berupa angka (tunai); non-tunai tidak membawa field itu |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-payment-outcome.js` | `describePaymentOutcome` menambahkan kalimat uang diterima dan kembalian (nilai tersimpan) bila ada; teks default tidak berubah |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-deposit-draft.js` | `combineDepositWithTenderSubmission` meneruskan rincian tunai hanya bila ada kembalian (bentuk hasil deposit murni tidak berubah) |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-menu-pembayaran.js` | Meneruskan rincian tunai ke `describePaymentOutcome` |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/payment-allocation-row.jsx` | Blok tunai pada kartu draft (uang diterima, kembalian read-only, konfirmasi); label "Nominal Pembayaran Tagihan" untuk tunai; mode tersimpan menampilkan uang diterima/kembalian dari backend |
| `src/components/view/health-services/billing-management/billing-invoices/detail/add-tender-modal.jsx` | Field kondisional tunai: Uang Tunai Diterima, rincian Nominal/Diterima/Kembalian, centang konfirmasi; label Nominal menyesuaikan |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | Meneruskan props ke baris; validasi simpan popup untuk tunai; baris ringkasan "Uang Tunai Diterima (draft)" dan "Kembalian (draft)"; sel Nominal Riwayat Pembayaran menampilkan rincian tersimpan |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/kwitansi-document.jsx` | Untuk tunai dengan rincian tersimpan: Jumlah Tagihan Dibayar, Uang Tunai Diterima, Kembalian, Pembayaran Bersih. Terbilang tetap dari nominal neto. Non-tunai, tunai lama, dan Rp0 tidak berubah |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir-page.js` | `kwitansiDocumentProps` meneruskan `cashTenderedAmount`/`cashChangeAmount` dari tender yang dimuat ulang dari backend |
| `src/style/health-services/billing-management/menu-pembayaran.module.css` | Enam kelas baru di CSS Module yang sudah ada, seluruhnya `var(--…)`: `.paymentCashBlock`, `.paymentRowHint`, `.cashChangeRow`, `.cashChangeValue`, `.historyAmountCell`, `.historyCashDetail` |
| `tests/unit/billing-cash-change.test.mjs` (baru) | 39 tes unit logika murni: kembalian, uang diterima wajib, Uang Pas, konfirmasi, payload, reset ganti metode, alokasi neto, identitas percobaan, pesan hasil, tender tunai tertahan, muatan iuran membership |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi dijaga: logika uang berada di helper murni (`billing-split-payment-rows.js`) yang dipakai hook dan view; view tidak memanggil Axios; request tetap lewat `InstanceAxios` di slice. Tidak ada komponen global, hook generik, instance Axios, atau state manager baru. Perubahan memakai alias `@/` dan penamaan yang sudah ada.

### 3.4 Gerbang tampilan

**Modul referensi visual:** kartu pembayaran dan popup yang sudah ada pada `menu-pembayaran-view.jsx` / `add-tender-modal.jsx` (tidak ada layar baru).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Field Uang Tunai Diterima di popup | `BaseTextField` (`normalizeValue`/`formatDisplayValue`, sama dengan field Nominal) | `REUSE` | Props yang ada sudah cukup |
| Konfirmasi kembalian di popup | `Form.Check` react-bootstrap, pola yang sama dengan centang verifikasi kasir non-tunai | `REUSE` | Pola existing pada berkas yang sama |
| Rincian Nominal/Diterima/Kembalian di popup | Kelas `depositSummary*` dari `payment-method-picker.module.css` | `COMPOSE` | Dirangkai dari kelas modul tetangga, tanpa gaya baru |
| Blok tunai pada kartu baris | `PaymentAllocationRow` (komponen view lokal, bukan base) + `currencyInputWrap` | `EXTEND` | Enam props opsional baru; default tidak berubah; satu-satunya pemakai `menu-pembayaran-view.jsx` |
| Baris ringkasan draft tunai | `summaryRow` | `REUSE` | — |
| Sel Nominal pada Riwayat Pembayaran | `DataTable` (`render` kolom) + dua kelas modul | `REUSE` | — |
| Dokumen kwitansi | Gaya inline dokumen cetak yang sudah ada (diperlukan agar PDF/print konsisten) | `REUSE` | Pola dokumen existing |

**Keputusan non-`REUSE` (pilihan bernomor)**

- **Blok tunai pada kartu baris.** (A) **Rekomendasi — perluas `PaymentAllocationRow` dengan props opsional.** Satu kartu tetap menjadi satu-satunya tempat mengedit baris; tidak ada duplikasi; risiko regresi hanya pada satu pemakai. Biaya terkecil. (B) Komponen baru `CashPaymentFields`. Lebih terpisah tetapi menambah komponen untuk satu pemakai dan menuntut keputusan `NEW`. Dipilih **A**.
- **Rincian di popup.** (A) **Rekomendasi — rangkai kelas `depositSummary*` yang ada.** Konsisten dengan kartu deposit di popup yang sama. (B) Tambah kelas modul baru. Konsisten sama, tetapi menduplikasi gaya. Dipilih **A**.

`UI GATE: PASS — 7 elemen: REUSE 5, COMPOSE 1, EXTEND 1 (komponen view lokal), WRAP 0, NEW 0`

Catatan jujur: `payment-allocation-row.jsx` memakai dua `<input>` mentah (kolom uang diterima dan kotak centang) karena mengikuti pola yang sudah ada pada kartu yang sama (kolom Nominal dan Copy Nominal memakai `<input>` mentah dengan `currencyInputWrap`); `BaseTextField` tidak mendukung awalan "Rp" gaya kartu ini. Ini disebut agar grep #9 checklist tidak dianggap lolos tanpa alasan.

Hasil pemeriksa dan grep, diringkas dari keluaran perintah yang sebenarnya (daftar temuan lengkap 8 baris; jalur berkas dipersingkat):

```text
$ node <rules>/frontend/tools/check-css-usage.mjs payment-allocation-row.jsx kwitansi-document.jsx menu-pembayaran-view.jsx add-tender-modal.jsx
payment-allocation-row.jsx:257  PROP-TIDAK-SAH  BaseButton variant="outline-warning" tidak dikenal ...
menu-pembayaran-view.jsx:956    CLASS-TIDAK-ADA "region-status-pill" ...
menu-pembayaran-view.jsx:1128   CLASS-TIDAK-ADA "paid" / "partial" ...
menu-pembayaran-view.jsx:1149   CLASS-TIDAK-ADA "tagihan" ; :1160 "riwayat" ; :1174 "deposit"
menu-pembayaran-view.jsx:1557   PROP-TIDAK-SAH  BaseButton variant="outline-danger" ...
8 temuan pada 4 berkas.
```

Delapan temuan itu **semuanya sudah ada di `HEAD`** dan tidak berasal dari baris yang saya tambahkan: `outline-warning` (HEAD `payment-allocation-row.jsx:191`), `region-status-pill` (HEAD `menu-pembayaran-view.jsx:933`), `outline-danger` (HEAD `:1520`), kelas tab `"tagihan"`/`"riwayat"`/`"deposit"` (HEAD `:1110-1135`). Dibuktikan dengan `git show HEAD:` dan dengan menyaring diff yang saya tambahkan (tidak memuat pola itu). Karena targetnya "0 temuan", status laporan ini tidak `✅`; temuan lama tidak diperbaiki (di luar scope).

```text
Grep anti-regresi pada BARIS YANG DITAMBAHKAN (git diff -U0):
(3) tombol non-base:            (kosong)
(7) class Tailwind:             (kosong)
(8) dialog buatan:              (kosong)
(9) <input|select|textarea>:    2 temuan — payment-allocation-row.jsx (dibenarkan di atas)
(10) prop base tidak sah:       (kosong)
CSS baru: warna literal / !important / dark mode: (kosong)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/health-services/billing-management/billing-invoices/…/menu-pembayaran` | — | `VISUAL CHECK: NOT FEASIBLE` — `npm run dev` dan `npm run build` dilarang pada task ini, dan skema/backend belum siap untuk pengujian berdata nyata. Tampilan belum pernah dibuka |
| Dokumen Kasir → Kwitansi (preview, PDF, share) | — | `VISUAL CHECK: NOT FEASIBLE` — alasan yang sama |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah (loading halaman/metode pembayaran/settlement yang sudah ada); tombol memakai label "Memproses Pembayaran..." |
| Kosong | Tidak berubah. Blok tunai tidak dirender sampai metode Tunai dipilih |
| Gagal | Pesan backend tampil pada baris yang gagal (`errorMessage`) dan lewat toast; konflik `409` memuat ulang settlement seperti sebelumnya |
| Uang kurang / konfirmasi belum dicentang | Pesan inline pada baris dan alasan di bawah tombol; tombol Proses Pembayaran terkunci |
| Tanpa hak akses | Tidak berubah (`AccessDeniedGate` halaman) |
| Data lama | Kwitansi dan riwayat tender tunai lama (rincian `null`) tampil seperti sebelumnya, tanpa kembalian |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Billing / Patient Funds

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/billing-management/billing/patient-funds/settlements/{id}/tenders` | Mengirim satu baris pembayaran; tunai membawa `cashTenderedAmount` dan `cashChangeGivenConfirmed` | `BillingPayment : Create` |
| `GET` | `/v1/health-services/billing-management/billing/patient-funds/invoices/{invoiceId}/settlements` | Memuat tender tersimpan untuk Dokumen Kasir/kwitansi (kini membawa `cashTenderedAmount`/`cashChangeAmount`) | `BillingPayment : Read` |
| `GET` | `/v1/health-services/billing-management/billing/patient-funds/settlements/{id}` | Menyegarkan settlement antar baris | `BillingPayment : Read` |

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/billing-management/billing/invoices/encounters/{encounterId}/payment-history` | Tabel Riwayat Pembayaran Kunjungan; sel Nominal menampilkan rincian tunai bila ada | `BillingInvoice : Read` |

Jalur dicocokkan dengan `[Route]` dan `[Tags]` pada controller backend (`BillingSettlementsController`, `BillingInvoicesController`), bukan dengan service frontend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/billing-cash-change.test.mjs` | 39 tes, 39 lulus, 0 gagal (hasil terkini, sesudah addendum) | `PASS` | Keluaran perintah (TC-01…07 sisi layar, TC-03, TC-04, TC-05, TC-06, TC-12, TC-15, serta tes keputusan final) |
| 19 berkas tes billing existing + `membership-fee-charge-utils` + berkas baru (21 berkas; hasil terkini sesudah addendum) | 382 tes: 378 lulus, 4 gagal | `EXISTING / ENVIRONMENT ISSUE` untuk 4 kegagalan (lihat di bawah); 378 `PASS`. (Versi pertama laporan: 317 tes, 313 lulus, 4 gagal lama yang sama) | Keluaran perintah |
| `eslint` pada 11 berkas source + 1 berkas tes yang diubah | 0 error, 2 warning | `PASS` (0 error); 2 warning `EXISTING` | `menu-pembayaran-view.jsx:464` (memoization) dan `use-dokumen-kasir-page.js:220` (dependency effect): bukan baris yang saya ubah |
| `check-css-usage.mjs` | 8 temuan | `EXISTING / ENVIRONMENT ISSUE` (0 dari baris tambahan) | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Larangan eksplisit master prompt dan preferensi pemilik |
| `npm run dev`, Playwright, uji peramban | Tidak dijalankan | `NOT RUN` | Larangan eksplisit master prompt |
| TC-08 refresh/cetak ulang menampilkan kembalian dari backend | Data kwitansi berasal dari `settlement.settlements` (backend) lewat `use-dokumen-kasir-page.js`; bukan state draft | `PASS` (penelusuran source); belum terbukti runtime | `use-dokumen-kasir-page.js` (`kwitansiDocumentProps`) |

**Empat kegagalan tes lama (bukan dari perubahan ini)**

1. `billing-settlement-active-selection.test.mjs` — tiga tes `Cannot read properties of null (reading 'id')`: reducer `getBillingSettlementsByInvoice.fulfilled` hanya mengisi state bila `meta.arg === state.activeInvoiceId`, sedangkan tes tidak menyetelnya. Diff saya pada slice hanya menyentuh `addBillingTender`.
2. `billing-single-payment-action.test.mjs` — "Backend: finalisasi otomatis…" membaca source backend dan mencari string `if (invoice is null || invoice.Status != BillingInvoiceStatuses.Open) return;` yang **tidak ada pada `HEAD` backend** (dibandingkan dengan `git show HEAD:`).

`AUTOMATED TEST: node --test tests/unit/billing-cash-change.test.mjs — PASS (39/39)`; suite existing terkait — 378 lulus, 4 gagal lama (di atas).

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, uji peramban/Playwright, dan seluruh uji terhadap backend hidup — alasan di tabel.

---

## 7. Acceptance criteria dan Definition of Done

Task ad-hoc tanpa kartu roadmap; kriteria dari master prompt bagian B dan "Stop conditions".

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tunai menampilkan Nominal Pembayaran Tagihan, Uang Tunai Diterima, Kembalian read-only, konfirmasi | Terpenuhi di source | `payment-allocation-row.jsx`, `add-tender-modal.jsx` |
| Non-tunai tidak menampilkan atau mengirim field tunai | Terpenuhi di source | `buildCashTenderPayload` mengembalikan `{}`; slice tidak menyertakan field; tes TC-05 |
| Kembalian dihitung ulang saat metode/nominal/uang berubah, tidak negatif | Terpenuhi | `computeCashChange`; tes TC-02, TC-06 |
| Ganti Tunai ke non-tunai mereset data tunai | Terpenuhi | `applyPaymentMethodChange`; tes TC-06 |
| Total alokasi dari nilai neto; uang diterima bukan alokasi | Terpenuhi | `sumPendingAmounts` tidak berubah; tes TC-03, TC-04 |
| Perubahan uang diterima masuk identitas percobaan | Terpenuhi | `buildTenderSignature`; tes TC-15 |
| `addBillingTender` mengirim `CashTenderedAmount` hanya untuk tunai | Terpenuhi | `billing-settlement-slice.jsx` |
| Ringkasan dan informasi tender berhasil menampilkan tagihan dibayar, uang diserahkan, kembalian | Terpenuhi di source | Baris ringkasan draft, kalimat notifikasi hasil, sel Riwayat Pembayaran |
| Kwitansi menampilkan nilai dari backend; non-tunai menyembunyikan kembalian; Rp0 tidak berubah | Terpenuhi di source | `kwitansi-document.jsx`, `use-dokumen-kasir-page.js` |
| Preview, riwayat, refresh, PDF, share memakai data yang sama | Terpenuhi pada penelusuran source (satu komponen, satu ref cetak) | `dokumen-kasir-view.jsx:395` |
| Memakai base component dan pola existing, tanpa komponen global baru | Terpenuhi | Bagian 3.4 |
| Tampilan terbukti di peramban (screenshot desktop dan ponsel) | **Belum terpenuhi** | `VISUAL CHECK: NOT FEASIBLE` |
| `check-css-usage.mjs` bernilai 0 temuan | **Belum terpenuhi** | 8 temuan lama; 0 dari baris tambahan |
| Lint, build | Lint `0 error`; build **belum dijalankan** | Bagian 6 |
| Perilaku runtime (TC-08, TC-09, TC-11 dengan backend hidup) | **Belum terpenuhi** | Menunggu skema, build backend, dan shift nyata |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Frontend ini harus dideploy **sesudah** skema database dan backend `BE-BKC-FIX-025`. Frontend baru dengan backend lama mengabaikan field tunai tanpa galat: pembayaran tercatat sebagai Amount saja dan kembalian **tidak tercatat**, sementara layar tampak normal |
| Masalah yang diketahui | Baris tunai dengan nominal pecahan sen (mis. Rp75.000,60 dari Copy Nominal) mensyaratkan uang diterima minimal sama dengan nominal itu — sesuai validasi backend; perilaku bersama Cash Rounding belum diuji di runtime (TC-12) |
| Dependency backend | `BE-BKC-FIX-025`: skema belum diterapkan, `dotnet build` belum dijalankan; riwayat tender yang ditahan `CREATED` dengan uang fisik berbeda tidak digabung (risiko R-2 di laporan backend) |
| Temuan di luar scope (tidak diubah) | Delapan temuan `check-css-usage` lama; dua warning eslint lama; empat tes unit lama yang gagal (bagian 6); `PaymentAllocationRow` masih memakai gaya inline dan warna literal pada lencana "Terverifikasi Kasir" |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend (terkini): 14 berkas source/CSS termodifikasi, 3 berkas tes lama dengan fixture diperbarui, dan 1 berkas tes baru (`git status --short`). Backend: lihat laporan `BE-BKC-FIX-025`. Tidak ada stage, commit, atau push |
| Langkah berikutnya | (1) Terapkan skema `BE-BKC-FIX-025` dan jalankan `dotnet build`; (2) jalankan `npm run dev` sendiri dan buka Menu Pembayaran serta Dokumen Kasir dengan shift aktif — uji TC-01…TC-15 termasuk screenshot desktop dan ponsel; (3) putuskan apakah kolom uang diterima boleh kosong |

---

## 9. Addendum 17 November 2026 — Keputusan Final pemilik modul Billing

Dua keputusan pemilik mengubah perilaku layar. Bagian 1–8 sudah disesuaikan; bagian ini merangkumnya. Kontrak backend yang dipakai: addendum bagian 8 pada [`BE-BKC-FIX-025`](../backend/BE-BKC-FIX-025.md).

### 9.1 Uang tunai diterima wajib, tombol "Uang Pas"

| Aspek | Perilaku |
| --- | --- |
| Isian | "Uang Tunai Diterima" **wajib** pada setiap baris tunai, termasuk pembayaran pas. Kosong **bukan** uang pas |
| Tombol **Uang Pas** | Ada di kartu baris dan popup Pilih Metode. Mengisi uang diterima = nominal pembayaran saat ini dan membatalkan konfirmasi kembalian lama. Nonaktif selama nominal belum diisi |
| Bila nominal berubah sesudah Uang Pas | Uang diterima **tidak ikut berubah diam-diam**; kembalian dihitung ulang dan harus dikonfirmasi ulang (atau tekan Uang Pas lagi) |
| Gerbang | Baris tunai tanpa uang diterima = belum lengkap: Proses Pembayaran terkunci dengan alasan "…wajib diisi. Tekan Uang Pas bila pasien membayar pas." Popup menolak simpan dengan pesan yang sama |
| Request | Tidak ada yang dikirim bila belum diisi; backend menolak `422` (pertahanan kedua) |
| Non-tunai | Tidak berubah: tanpa uang diterima, kembalian, atau tombol Uang Pas |
| Ringkasan, kwitansi, riwayat | Tetap dari nilai tersimpan backend (tidak berubah dari bagian 3) |

**Penyimpangan yang disengaja pada alur lain.** Pembayaran **iuran membership** juga memanggil `POST …/tenders` dan tunai di sana akan ditolak backend. Layar iuran selalu membayar penuh dan belum punya isian uang diterima/kembalian, jadi `buildMembershipFeeTenderPayload` mengirim tunai sebagai uang pas (uang diterima = nominal, tanpa kembalian); non-tunai tidak berubah. Bila iuran membership perlu kembalian, itu task terpisah.

### 9.2 Tender tunai tertahan `CREATED`: Lanjutkan Pembayaran atau Batalkan Percobaan

**Proses dari sisi kasir.** Baris "Pembayaran Menunggu Konfirmasi" yang berstatus `CREATED` **dan** bermetode tunai kini menampilkan penjelasan singkat dan dua tombol; baris non-tunai tetap memakai Konfirmasi Manual, dan tender `PENDING` tidak pernah ditawari pembatalan.

| Tombol | Dialog | Yang terjadi |
| --- | --- | --- |
| **Lanjutkan Pembayaran** | Ringkasan metode, nominal neto, uang diterima, kembalian; kotak centang konfirmasi kembalian bila kembalian > 0 | Frontend mengirim ulang pembayaran yang **sama persis** (`buildContinueCashTenderInput`) lewat `POST …/tenders`; backend memulihkan tender yang sama menjadi berhasil. Notifikasi menyebut kembalian dari nilai tersimpan |
| **Batalkan Percobaan** | Ringkasan yang sama, **alasan wajib**, dan pernyataan "Saya memastikan uang pasien belum diterima atau seluruhnya telah dikembalikan" | `POST …/tenders/{tenderId}/cancel`. Berhasil → tender `CANCELLED`, saldo tertahan dilepas, kasir dapat membuat pembayaran baru. Riwayat tetap menampilkan baris itu dengan badge **Dibatalkan** |

**Contoh.** Rekonsiliasi tender tunai Rp75.000 (uang diterima Rp100.000) gagal sehingga tersisa baris menunggu. Pasien membatalkan: kasir menekan **Batalkan Percobaan**, menulis alasan, mencentang pernyataan, menyimpan. Baris itu hilang dari area input, "Menunggu Konfirmasi" kembali ke Rp0, kas shift tidak berubah, dan Riwayat Pembayaran menampilkannya "Dibatalkan" tanpa tombol Cetak Kwitansi.

**Jalur tidak normal.** Status tender sudah berubah saat tombol ditekan (mis. sudah berhasil atau sudah dibatalkan dari layar lain) → tidak ada yang dikirim, data dimuat ulang. `409`/`422` backend tampil pada dialog (pesan backend), dialog tetap terbuka. Setelah pembatalan, `attempt` (kunci idempotensi) baris draft dibuang agar kiriman berikutnya memakai kunci baru.

**Status `CANCELLED` pada layar.** Badge **Dibatalkan** pada tabel riwayat (`TENDER_STATUS_BADGE_CONFIG`) dan badge **DIBATALKAN / CANCELLED** pada kwitansi. Tender batal **tidak** dihitung sebagai kwitansi di Dokumen Kasir (bukan penerimaan uang), sehingga tidak menutup jalur kwitansi Rp0; tombol Cetak Kwitansi pada riwayat dinonaktifkan untuknya.

### 9.3 Berkas tambahan pada addendum

| Berkas | Perubahan |
| --- | --- |
| `billing-split-payment-rows.js` | `computeCashChange` (`isMissing`, `tendered = 0` bila kosong), `applyExactCash`, `isStuckCashTender`, `buildContinueCashTenderInput`, gerbang `CASH_TENDERED_REQUIRED`; `buildCashTenderPayload` tidak mengirim bila belum diisi |
| `use-billing-settlement.js` | `cancelCashTender`, `continueCashTender`, `applyExactCash` per baris |
| `billing-settlement-slice.jsx` | Thunk dan reducer `cancelBillingTender` |
| `payment-allocation-row.jsx` | Tombol Uang Pas; dua tombol pemulihan pada baris tunai `CREATED` |
| `add-tender-modal.jsx` | Uang diterima wajib + tombol Uang Pas |
| `menu-pembayaran-view.jsx` | Dialog pemulihan (satu `Modal`, dua mode), penyambungan props, validasi simpan popup, tombol kwitansi riwayat untuk `CANCELLED` |
| `menu-pembayaran.module.css` | Tiga kelas baru (`cashRecoveryBox`, `cashRecoveryText`, `cashRecoveryActions`), seluruhnya `var(--…)` |
| `billing-invoice-constants.js`, `kwitansi-document.jsx`, `use-dokumen-kasir-page.js` | Badge `Dibatalkan`/`DIBATALKAN`; tender batal tidak menjadi kwitansi |
| `membership-fee-charge-utils.js` | Tunai iuran dikirim sebagai uang pas |
| `tests/unit/billing-split-payment-rows.test.mjs`, `billing-deposit-draft.test.mjs`, `billing-employee-ar-draft.test.mjs` | Fixture baris tunai diberi uang diterima = nominal (asersi tidak diubah), karena baris tunai tanpa uang diterima kini memang belum lengkap |

Satu tes lama berbasis teks sumber (`billing-manual-cashier-single-step`, "409 pada konfirmasi tender memuat ulang settlement") memotong isi `manualConfirmTender` sampai `return {` berikutnya. Dua callback baru saya awalnya berada di antaranya sehingga tes itu gagal; saya memindahkan callback baru ke **sebelum** `manualConfirmTender` dan tes itu tidak diubah.

### 9.4 Gerbang tampilan tambahan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Tombol Uang Pas, Lanjutkan Pembayaran, Batalkan Percobaan | `BaseButton` (`secondary`, `primary`, `danger`) | `REUSE` | Varian yang sah |
| Dialog pemulihan | `Modal` react-bootstrap + `Form.Check` + `InformationAlert` + `BaseButton`, pola dialog Konfirmasi Manual pada berkas yang sama | `REUSE` | Pola existing untuk pemulihan tender pada layar ini |
| Blok pesan dan tombol pada baris tertahan | `PaymentAllocationRow` (props opsional `onContinue`, `onCancelAttempt`) + tiga kelas pada CSS Module existing | `EXTEND` | Default tidak berubah; satu-satunya pemakai `menu-pembayaran-view.jsx` |

`UI GATE: PASS — 3 elemen tambahan: REUSE 2, EXTEND 1 (komponen view lokal), COMPOSE 0, WRAP 0, NEW 0`

Hasil terkini: `check-css-usage.mjs` — 8 temuan, **identik** dengan sebelum addendum dan seluruhnya sudah ada di `HEAD` (0 dari baris yang ditambahkan). Grep anti-regresi pada baris tambahan: tombol non-base, Tailwind, dialog buatan, prop tidak sah, warna literal/`!important` pada CSS — semuanya kosong; `<input>` mentah hanya dua (kolom uang diterima dan kotak centang pada `PaymentAllocationRow`, mengikuti pola kartu yang sama). `eslint`: 0 error, 2 warning lama pada baris yang tidak diubah.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Menu Pembayaran: kartu tunai, popup, dialog pemulihan | — | `VISUAL CHECK: NOT FEASIBLE` — `npm run dev`/`build` dilarang dan database/backend belum diuji |
| Kwitansi dan Riwayat Pembayaran (badge Dibatalkan) | — | `VISUAL CHECK: NOT FEASIBLE` — alasan sama |

### 9.5 Verifikasi tambahan dan risiko

| Hal | Isi |
| --- | --- |
| Yang terbukti | Logika murni: uang diterima wajib, Uang Pas, konfirmasi gugur, payload hanya tunai, input Lanjutkan Pembayaran (termasuk data lama = uang pas), status `CANCELLED`, muatan iuran. Seluruhnya lewat tes unit yang dijalankan |
| Yang **tidak** terbukti | Alur klik di peramban: tampilan dialog, pemanggilan nyata `…/cancel`, dan perilaku `Lanjutkan Pembayaran` terhadap backend hidup (bergantung pada pemulihan tender `CREATED` oleh backend, yang hanya ditelusuri di source). `MANUAL TEST: NOT FEASIBLE` |
| Risiko deploy | Backend yang mewajibkan uang diterima menolak tunai dari frontend lama; frontend baru memanggil endpoint yang belum ada di backend lama. Deploy keduanya bersamaan setelah DDL terpasang |
| Risiko "Lanjutkan Pembayaran" | Bila lebih dari satu tender `CREATED` cocok (metode, nominal, uang diterima sama), backend menolak `409` "lebih dari satu pembayaran yang masih menunggu"; kasir membatalkan salah satunya lebih dulu |
| Izin | Tombol Batalkan Percobaan tidak disembunyikan berdasarkan izin di frontend; backend yang menegakkan (`BillingPayment : Update`). Lihat temuan hak akses pada laporan backend 8.6 |

---

## 10. Addendum 17 November 2026 — Penyelarasan dengan master prompt "Finishing Pembayaran Tunai, Kembalian, dan Pembatalan Tender"

Master prompt lanjutan (FE-CASH-001..008) dibandingkan dengan source frontend setelah addendum 9. Kontrak backend yang dipakai: bagian 9 pada [`BE-BKC-FIX-025`](../backend/BE-BKC-FIX-025.md). Tanpa `npm run build`/`dev`; tampilan tidak dibuka di peramban (`VISUAL CHECK: NOT FEASIBLE`, tidak ada komponen global atau kelas CSS baru pada putaran ini).

### 10.1 Pemetaan master prompt ke source

| Butir | Keadaan sesudah penyelarasan | Berkas |
| --- | --- | --- |
| FE-CASH-001 form tunai: nominal tagihan, uang diterima, **Uang Pas**, kembalian otomatis read-only, konfirmasi penyerahan, reset saat metode berganti | Sudah ada sejak addendum 9. **Diubah:** teks galat disamakan dengan master prompt: "Uang tunai diterima wajib diisi.", "Jumlah uang diterima kurang dari nominal pembayaran.", "Kembalian sebesar Rp… harus diserahkan kepada pasien." | `billing-split-payment-rows.js`, `add-tender-modal.jsx`, `menu-pembayaran-view.jsx` |
| FE-CASH-002 state split payment (`cashTenderedAmount`, `cashChangeAmount` preview) | `cashTenderedAmount` disimpan pada baris. **Kembalian pratinjau tidak disimpan sebagai field** melainkan selalu dihitung dari `computeCashChange(row).change` (satu fungsi untuk modal, kartu, ringkasan, payload). Alasan: nilai turunan yang disimpan bisa basi saat nominal atau uang diterima berubah. Uang diterima tidak masuk total alokasi; identitas percobaan (`buildTenderSignature`) memuat `cash:<n>` | `billing-split-payment-rows.js` |
| FE-CASH-003 integrasi API | **Diubah:** nama field mengikuti DTO backend: `cashChangeGivenConfirmed` (sebelumnya `isCashChangeHandedOver`); thunk batal mengirim `cancellationReason` dan `cashCustodyConfirmed` (sebelumnya `reason`, `isCashHandlingConfirmed`). `cashChangeAmount` tidak pernah dikirim. Setelah sukses settlement dimuat ulang dari backend; `409` memuat ulang daftar settlement tanpa mengubah status lokal | `billing-settlement-slice.jsx`, `use-billing-settlement.js`, `membership-fee-charge-utils.js` |
| FE-CASH-004 kartu pembayaran menampilkan uang diterima dan kembalian | Sudah ada (mode draft dan mode tersimpan dari data backend) | `payment-allocation-row.jsx` |
| FE-CASH-005 Lanjutkan Pembayaran / Batalkan Percobaan | Dialog sudah ada. **Diubah:** menampilkan "Status Tender" (badge), judul "Batalkan Percobaan Pembayaran?", pernyataan "Saya memastikan uang pasien belum diterima atau seluruhnya telah dikembalikan." Tombol kirim nonaktif bila status tender bukan `CREATED`. **Ditambah:** Idempotency-Key dan CorrelationId **dipertahankan** untuk percobaan yang sama (respons hilang/jaringan putus/502-504) sehingga retry tidak membuat pembatalan dengan kunci berbeda; kunci dilepas hanya setelah ada jawaban pasti backend | `menu-pembayaran-view.jsx`, `use-billing-settlement.js` |
| FE-CASH-005 penanganan `409` | **Diubah:** bila status tender sudah berubah, tidak ada yang dikirim; dialog menampilkan pesan yang sesuai ("Tender sudah berhasil diproses dan tidak dapat dibatalkan melalui pembatalan percobaan.", "Percobaan pembayaran ini sudah dibatalkan.", atau "Data pembayaran berubah. Silakan muat ulang sebelum melanjutkan.") dan data dimuat ulang dari backend. Pesan sukses: "Tender berhasil dibatalkan. Saldo tagihan tersedia kembali." | `menu-pembayaran-view.jsx`, `use-billing-settlement.js` |
| FE-CASH-006 riwayat | `CANCELLED` bertanda "Dibatalkan", tanpa aksi lanjutan, tidak dihitung berhasil, tidak masuk "menunggu konfirmasi"; `SUCCEEDED` tanpa tombol batal; `PENDING` non-tunai tetap lewat konfirmasi manual | `billing-invoice-constants.js`, `payment-allocation-row.jsx`, `menu-pembayaran-view.jsx` |
| FE-CASH-007 kwitansi | **Diubah:** label mengikuti master prompt: Jumlah Tagihan Dibayar, **Uang Tunai Diterima**, Kembalian, **Pembayaran Bersih** (sebelumnya "Uang Tunai Diserahkan" dan "Pembayaran Diterapkan"). Terbilang tetap dari nominal neto; kembalian bukan pembayaran tambahan. **Ditambah:** dokumen untuk tender `CANCELLED` (bila terbuka) tidak mencetak "Sudah terima dari", terbilang, maupun "Jumlah Diterima"; memuat catatan "bukan bukti penerimaan pembayaran". Kwitansi Rp0 tidak berubah | `kwitansi-document.jsx` |
| FE-CASH-008 UX dan galat | Pesan bisnis dari backend ditampilkan apa adanya (backend sudah mengirim kalimat bisnis Indonesia); galat penting tidak disembunyikan | seluruh berkas di atas |

### 10.2 Yang BELUM sesuai master prompt

| Butir | Keadaan | Alasan |
| --- | --- | --- |
| Kwitansi: "Petugas Kasir : [Nama Kasir]" | Tetap blok tanda tangan "Petugas Kasir / Cashier Officer" tanpa nama | Kontrak tender tidak membawa nama petugas; menambahkannya butuh perubahan backend dan keputusan privasi |
| Dokumen kasir untuk tender `CANCELLED` | Tender batal tetap **tidak** masuk daftar kwitansi (`use-dokumen-kasir-page.js` menyaringnya) dan tombol cetak riwayat dinonaktifkan. Master prompt hanya meminta label "DIBATALKAN" "jika dokumen historis dapat dibuka", jadi komponen sudah aman bila kelak dibuka | Mempertahankan keputusan addendum 9 agar percobaan batal tidak menjadi bukti bayar |
| Kembalian pratinjau sebagai field state `cashChangeAmount` | Tidak disimpan (lihat FE-CASH-002) | Menghindari nilai basi; perilaku layar identik |
| Cek visual dan uji klik | Tidak dilakukan | Dilarang menjalankan dev server; `VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` |

### 10.3 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/billing-cash-change.test.mjs` | 43 tes, 43 lulus (4 tes baru: redaksi pesan; nama field thunk; kecocokan nama field dan route dengan DTO/controller backend, tidak dilewati; label kwitansi) | `AUTOMATED TEST: PASS` |
| Gabungan `billing-cash-change`, `billing-split-payment-rows`, `billing-deposit-draft`, `billing-employee-ar-draft` | 146 tes, 146 lulus | `AUTOMATED TEST: PASS` |
| 46 berkas tes `*billing*`/`*membership*` | 613 tes, 606 lulus, **7 gagal**, semuanya pada berkas yang tidak diubah task ini: `billing-settlement-active-selection` (3, diketahui), `billing-single-payment-action` (1, diketahui), `billing-inpatient-summary-panel` (1; view tidak mengimpor `BillingInpatientSummaryPanel` baik di HEAD maupun working tree, sudah dibuktikan), `billing-data-employee-search` (1) dan `inpatient-billing-status-badge` (1) di modul Finance dan Rawat Inap yang tidak saya sentuh (tidak saya telusuri sampai ke HEAD) | `AUTOMATED TEST: FAIL` (lama), bukan regresi yang saya buktikan |
| `npx eslint` pada tujuh berkas yang diubah | 0 error, 1 warning lama (`react-hooks/preserve-manual-memoization` pada `menu-pembayaran-view.jsx:565`) | `PASS` dengan peringatan |
| `check-css-usage.mjs` | Tidak dijalankan ulang: putaran ini tidak menambah kelas CSS | `NOT RUN` |
| `npm run build`, `npm run dev`, tampilan, uji klik | Tidak dijalankan | `NOT RUN` / `NOT FEASIBLE` |

### 10.4 Risiko

| Hal | Isi |
| --- | --- |
| Perubahan nama field | `cashChangeGivenConfirmed`, `cancellationReason`, `cashCustodyConfirmed` harus sejalan dengan backend yang sama. Frontend ini dengan backend sebelum penyelarasan akan ditolak (`cashChangeGivenConfirmed` dibaca sebagai field tak dikenal dan diabaikan, sehingga kembalian positif ditolak `422`). Deploy backend dan frontend bersamaan |
| Retry pembatalan | Kunci dipertahankan hanya selama frontend tidak dimuat ulang (disimpan di memori hook). Setelah refresh browser, retry memakai kunci baru; backend tetap aman karena memanggil ulang pada tender `CANCELLED` dijawab idempoten |
