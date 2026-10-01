# Laporan Perubahan Frontend — `FE-RJE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RJE-003` |
| Judul | Layar Antrean Rekonsiliasi Tagihan Klinis |
| Slice | `MVP-4` — `EPIC RJE-06` |
| Roadmap | [roadmap/e2e-frontend-roadmap.md](../../../roadmap/e2e-frontend-roadmap.md), kartu `FE-RJE-003` |
| Trace | `FR-RJE-053`; `RJ-E2E-DEC-009`; `03-frontend-architecture.md` V2.4 (`FE-RJE-03`), V2.6 (pesan `409`); `UAT-15`, `UAT-16` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.0` — `approved`; bentuk aktual dari `BE-RJE-012` (lihat bagian 6 untuk delta) |
| Wewenang UI | Tata letak, nama, dan urutan butir menu `DEV_DISCRETION` di dalam skema V2.4 (Sukma Giri, 2026-09-30) |
| Dependency | `BE-RJE-012` ✅ (di-commit pemilik) |
| Klasifikasi | `STANDARD` — route, view, hook, service, utilitas, konstanta, butir menu baru |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` (`sukmagpV2`); laporan di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `83b8b7274` (`sukmagpV2`) + perubahan `FE-RJE-001`/`002` yang belum di-commit |
| Commit backend yang dijadikan rujukan | HEAD `sukmagp` (build `out-rje012b`) |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — kedelapan acceptance criteria terbukti di Chromium terhadap backend sungguhan |

---

## 1. Keadaan yang ditemukan di awal

`BE-RJE-012` sudah menyediakan `GET /billing/charge-reconciliations`, `POST /{itemType}/{id}/retry`,
dan `POST /{itemType}/{id}/resolve` dengan hak akses `BillingChargeReconciliation : Read/Update`.
Belum ada layar, service, maupun butir menu di frontend. Petugas Billing tidak punya jalan untuk
melihat pelayanan klinis yang gagal tercatat di tagihan kecuali lewat database.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Billing membuka **Billing dan Kasir → Rekonsiliasi Tagihan Klinis**.
2. Layar menampilkan item yang **belum selesai** (gagal dan perlu penanganan), 20 per halaman.
   Setiap baris memuat nomor kunjungan, nomor invoice, jenis pelayanan, versi, sebab dalam bahasa
   biasa (mis. "Tarif belum ada") beserta pesan backend, jumlah percobaan, waktu, dan status.
   Tidak ada nama pasien.
3. Petugas menyaring berdasarkan status, jenis pelayanan, sebab, rentang tanggal, dan mencari nomor
   invoice atau nomor kunjungan.
4. **Kirim Ulang.** Setelah penyebabnya diperbaiki (mis. tarif dilengkapi di master tarif), petugas
   menekan **Kirim Ulang**. Bila berhasil, muncul "Item sudah dikirim ulang dan keluar dari
   antrean." dan item hilang. Bila belum berhasil, muncul "Kirim ulang belum berhasil — Sebab: …"
   dan item tetap di antrean.
5. **Selesaikan.** Untuk item yang tidak akan dikirim ulang, petugas menekan **Selesaikan**, memilih
   jenis penyelesaian (Sudah ditagih manual / Tidak perlu ditagih / Duplikat), dan menulis alasan
   10–500 karakter tanpa isi klinis. Item keluar dari antrean dan dapat dilihat lagi lewat saringan
   **Sudah diselesaikan**.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Alasan kosong / < 10 karakter / jenis belum dipilih | Pesan merah di bawah isian; tidak ada request |
| Item sedang dipegang pemroses lain (`409`) | "Item sedang diproses sistem. Muat ulang beberapa saat lagi."; daftar dimuat ulang otomatis, **tanpa** kirim ulang otomatis |
| Item sudah tercatat (`422 RJE-VAL-020`) / tidak ditemukan (`404`) | Pesan backend / "Item tidak ditemukan"; daftar dimuat ulang |
| Daftar gagal dimuat | Peringatan merah + keadaan "Daftar belum dapat ditampilkan"; pulih lewat **Muat Ulang** |
| Rentang tanggal terbalik | Peringatan kuning; request tidak dikirim |
| Tanpa `Update` | Daftar tampil tanpa kolom **Tindakan** |
| Tanpa `Read` | Butir menu tersembunyi; halaman menampilkan **Ups! Akses Ditolak** |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend: `BillingChargeReconciliationController`, `BillingChargeReconciliationService` (saringan,
status item tunggal `SYNCED`/`DISPATCHED`/`PENDING`, kode `RJE-VAL-020..025`),
`ChargeReconciliationDtos.cs`, `PagedResult`, `BillingSourceTariffResolver` (tarif konsultasi).
Frontend: `consumer-handoffs-view.jsx` + hook (rujukan visual terdekat), `use-doctor-billing-summary.js`,
`access-denied-gate.jsx`/`access-denied-utils.jsx`, `data-filter.jsx`, `data-table.jsx`,
`filter-select.jsx`, `filter-date-picker.jsx`, `confirm-modal.jsx`, `status-badge.jsx`,
`base-form-control.jsx`, `toast-stack.jsx`, `use-permission.jsx`, `menu-items.jsx`, `globals.css`.

### 3.2 Berkas baru

| Berkas | Isi |
| --- | --- |
| `src/app/health-services/billing-management/billing/charge-reconciliations/page.jsx` | Route tipis + metadata |
| `src/components/view/health-services/billing-management/charge-reconciliations/charge-reconciliations-view.jsx` | Hero, DataFilter (cari + 4 FilterSelect + 2 FilterDatePicker + Muat Ulang), DataTable, ConfirmModal berisi form Selesaikan, ToastStack, AccessDeniedGate |
| `src/lib/hooks/health-services/billing-management/charge-reconciliation/use-charge-reconciliations.js` | Saringan, pembatalan request lama, satu aksi pada satu waktu (`pendingRef`), `409` → pesan + muat ulang, validasi form, pemetaan `422` ke isian |
| `src/lib/services/health-services/billing-management/charge-reconciliation.service.js` | `getChargeReconciliations`, `retryChargeReconciliation`, `resolveChargeReconciliation` lewat `InstanceAxios` |
| `src/lib/constants/health-services/billing-management/charge-reconciliation.constants.js` | Endpoint, izin, batas, opsi saringan, label sebab/status/penyelesaian, pesan `409` |
| `src/utils/health-services/billing-management/charge-reconciliation-utils.js` | Normalisasi whitelist, parameter query (pencarian ≤ 100, batas hari lokal → ISO), validasi form, pembaca galat |
| `src/style/health-services/billing-management/charge-reconciliations-view.module.css` | Tata letak sel dan modal; seluruh nilai dari token |
| `tests/unit/charge-reconciliation.test.mjs` | 11 uji utilitas + pemeriksaan pemakaian base component dan izin menu |
| `tests/e2e/charge-reconciliation-screen.spec.mjs` | 8 skenario Playwright pola Bank Darah (R1–R8) |

### 3.3 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Rekonsiliasi Tagihan Klinis** di grup *Billing dan Kasir*, sesudah *Surat ke Modul Konsumen*; `requiredPermission: { resource: "BillingChargeReconciliation", action: "Read" }` |

### 3.4 Gerbang keputusan base component

| Elemen | Status | Base component |
| --- | --- | --- |
| Judul halaman | `REUSE` | `Hero` |
| Saringan + pencarian + reset | `REUSE` | `DataFilter`, `FilterSelect`, `FilterDatePicker` |
| Tabel + paginasi + keadaan kosong/memuat | `REUSE` | `DataTable` |
| Status | `REUSE` | `StatusBadge` |
| Tombol aksi | `REUSE` | `BaseButton` (`loading`, `loadingLabel`, `disabled`) |
| Form Selesaikan | `COMPOSE` | `ConfirmModal` (children) + `BaseNativeSelectField` + `BaseTextAreaField` |
| Galat daftar / tanggal terbalik | `REUSE` | `InformationAlert` |
| Gerbang hak akses | `REUSE` | `AccessDeniedGate` + `usePermission` |
| Umpan balik aksi | `REUSE` | `ToastStack` |

`UI GATE: PASS` — tidak ada `NEW` atau `EXTEND`; satu `COMPOSE` memakai props yang sudah ada.

### 3.5 Keputusan teknis

- **State lokal di hook**, bukan Redux slice baru — sama seperti hook `FE-RJE-001`; alurnya tetap
  view → hook → service → `InstanceAxios`.
- **Tombol Selesaikan di modal tidak dinonaktifkan saat isian kosong**, supaya pesan validasi muncul
  di bawah isian (`UAT-16`). `ConfirmModal` sendiri menonaktifkannya selama request berjalan.
- **Seluruh tombol aksi nonaktif selama satu aksi berjalan**; `pendingRef` juga menolak klik kedua
  sebelum React merender ulang.
- **Pencarian dipotong 100 karakter di hook.** `DataFilter` tetap mengizinkan 200 karakter di isian
  (batas base component), tetapi yang dikirim ke backend paling banyak 100 (terbukti di R2).
- **Tanggal dikirim sebagai awal/akhir hari lokal dalam ISO UTC**, karena backend membandingkan
  dengan `ToUniversalTime()`.

---

## 4. Pemetaan acceptance criteria

| AC | Bukti source | Bukti runtime |
| --- | --- | --- |
| 1. Butir menu membuka layar | `menu-items.jsx`; `page.jsx` | R1 — dari *Surat ke Modul Konsumen*, butir diklik → URL layar, judul, baris tampil |
| 2. Saringan + pencarian ≤ 100 | `buildChargeReconciliationParams`, `updateFilter` | R2 — `status`, `sourceDomain`, `errorCode`, `dateFrom`/`dateTo` terkirim dan isi baris sesuai; gabungan bertahan; 150 karakter → `search` ≤ 100; reset membersihkan semua |
| 3. `UAT-15` | `retry`, toast berdasar status hasil | R5 — Kirim Ulang tanpa tarif → "belum berhasil — Tarif belum ada", item tetap; tarif diisi → Kirim Ulang identitas sama → item hilang; backend tidak lagi mengembalikan item |
| 4. `UAT-16` | `validateResolveForm`, `BaseTextAreaField error` | R3 — pesan di bawah kedua isian, `aria-invalid`, 0 request; alasan sah → 1 `POST resolve` 200, item keluar, tampil di saringan *Sudah diselesaikan* tanpa tombol |
| 5. `409` | `resolveChargeReconciliationActionError` | R4 — baris dikunci di DB → backend `409`; pesan tetap tampil; daftar dimuat ulang; tetap 1 `POST` setelah 3 detik |
| 6. Tombol nonaktif | `actionPending`, `pendingRef`, `BaseButton loading` | R4 — selama request: *Mengirim…*, semua *Selesaikan*, dan *Muat Ulang* nonaktif; klik paksa kedua tidak mengirim request |
| 7. Gerbang hak akses | `buildColumns(canUpdate)`, `accessError`, `requiredPermission` | R6 — hanya `Read`: tanpa kolom/tombol aksi, butir menu ada. R7 — tanpa `Read`: *Ups! Akses Ditolak*, butir menu tidak ada |
| 8. Empat keadaan | `DataTable loading/empty`, `errorMessage` | Memuat & gagal & pulih: R8. Kosong: R2 ("Tidak ada item yang cocok"). Berisi: R1 |

---

## 5. Validasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint` (berkas baru + `menu-items.jsx`) | `PASS` — 0 galat, 0 peringatan |
| `npm run lint:errors` | `PASS` |
| `node --test tests/unit/charge-reconciliation.test.mjs` | `PASS` 11/11 |
| `npm run test:unit` | 2070/2077 — 7 kegagalan lama (`accounting-reconciliation`, `hemodialysis` ×4, `menu-permission-filter`, `petty-cash`), sama dengan baseline `FE-RJE-001`/`002`. **UNRELATED EXISTING ISSUE** |
| `npm run build` | `PASS` — route `/health-services/billing-management/billing/charge-reconciliations` terdaftar |
| Playwright Chromium, standalone `127.0.0.1:3710`, backend sungguhan `localhost:5219` (`QuilvianNewDevSukma`) | **8/8 skenario passed**, dijalankan per kelompok (lihat di bawah) |

**Riwayat run Playwright (apa adanya).**

| Run | Skenario | Hasil | Catatan |
| --- | --- | --- | --- |
| 1 | R1, R2, R6, R7, R8 | 1 failed, 4 not run | R1: sidebar headless 1280×720 tidak dapat digulir ke butir; diganti klik DOM pada tautan yang sama |
| 2 | R1, R2, R6, R7, R8 | **5 passed** | — |
| 3 | R4 | failed | Kunjungan punya dua baris Radiologi; `.first()` mengenai baris lain. Saringan spec ditambah sebab |
| 4 | R4 | **passed** | Item kunci tidak berubah |
| 5 | R3, R5 | R3 **passed**, R5 failed | Item UAT-15 pertama (ENC-RSMMC-00068) invoicenya `CLOSED`; setelah tarif diisi backend dengan benar mengganti sebab menjadi `ADJUSTMENT_REJECTED`. Diganti item dengan invoice `OPEN` |
| 6 | R5 | failed | Pembantu data: kode tarif uji bentrok constraint unik; kode dibuat unik |
| 7 | R5 | **passed** | — |

R3 dan R5 mengonsumsi datanya (item diselesaikan / tersinkron), sehingga suite penuh tidak dapat
diulang dalam satu run tanpa menyiapkan item baru. `MANUAL TEST`: diwakili run Playwright di atas
terhadap backend sungguhan; tidak ada jawaban API yang dikarang. Yang dipasang di proxy hanya
daftar izin (R6, R7), jeda (R4, R8), dan pemutusan jaringan (R8). `409` dibuat dengan mengunci baris
di database.

**Keamanan artefak.** Cookie backend dibaca dari berkas scratchpad lewat environment dan tidak pernah
dicetak; berkasnya dihapus setelah run. `test-results` dikembalikan (`git restore` + `git clean`);
tidak ada token di artefak.

---

## 6. Delta kontrak dan temuan

| Butir | Keterangan |
| --- | --- |
| Parameter halaman | Backend memakai `page`, bukan `pageNumber`; respons tetap `PagedResult.pageNumber` |
| Status di respons aksi | Selain `FAILED`/`RECONCILIATION_REQUIRED`/`RESOLVED`, respons aksi dapat berisi `SYNCED`, `DISPATCHED`, `PENDING`; layar memetakan ketiganya |
| Jenis penyelesaian di daftar | Respons tidak memuat kolom jenis penyelesaian terpisah; jenisnya hanya terbaca sebagai awalan `ResolutionNote` (mis. `NOT_BILLABLE: …`) |
| Sebab berganti setelah Kirim Ulang | Item `TARIFF_NOT_FOUND` pada kunjungan ber-invoice `CLOSED` berubah menjadi `ADJUSTMENT_REJECTED` setelah tarif diisi. Perilaku backend benar; petugas melihatnya sebagai "Kirim ulang belum berhasil — Sebab: Penyesuaian ditolak" |

---

## 7. Data uji dan pemulihan (`QuilvianNewDevSukma`)

| Data | Keadaan akhir |
| --- | --- |
| Tarif sementara `TEST-RJE-FE003`, `TEST-RJE-FE003-c329f1` (klinik `f0685864…`) | Dinonaktifkan (`IsActive=false`, `IsDelete=true`) |
| Efek `c608aa13…` (item kunci R4) | Tidak berubah — `TARIFF_NOT_FOUND`, percobaan 0 |
| Efek `d7deba79…` (item UAT-15 pertama) | Sebab dikembalikan ke `TARIFF_NOT_FOUND` beserta teks aslinya |
| Efek `0515508b…` (terkena run 3) | Tetap `ADJUSTMENT_REJECTED`, percobaan 0; hanya `UpdateDateTime` berubah |
| Efek `c5bceef2…` (ENC-RSMMC-00133, UAT-15) | **Sengaja dibiarkan** `Synced`: baris konsultasi Rp125.000 `ACTIVE` di `BIL-20260930-00000001`, merujuk tarif uji yang sudah dinonaktifkan |
| Fakta sintetis `7968105f…` (`TEST-RJE-FE002`) | **Sengaja dibiarkan** diselesaikan: `NOT_BILLABLE: TEST-RJE-FE003 fakta sintetis uji, tidak perlu ditagih` |
| Berkas cookie scratchpad | Dihapus |

Tidak ada baris tagihan lain yang sempat memakai tarif sementara (diperiksa: 1 baris, yaitu hasil
UAT-15).

---

## 8. Risiko tersisa

| Risiko | Pemilik |
| --- | --- |
| Baris Rp125.000 di `BIL-20260930-00000001` merujuk tarif uji nonaktif; hapus/batalkan bila invoice itu dipakai uji kasir | Pemilik data uji |
| Pencarian > 100 karakter tetap tampil utuh di isian (batas `DataFilter` 200), walau yang dikirim hanya 100 | Frontend — base component |
| `usePermission` bernilai "boleh" sebelum izin termuat; satu request daftar dapat terkirim sebelum gerbang menutup (backend tetap menolak `403` untuk pengguna tanpa hak) | Frontend — desain hook dasar |
| Endpoint daftar tidak membatasi per unit/klinik; siapa pun dengan `Read` melihat seluruh antrean | Backend / keputusan akses |

---

## 9. Langkah berikutnya

Ketiga task frontend roadmap `e2e-frontend` sudah selesai. Langkah yang disarankan:
`verify-module-readiness` untuk E2E lintas layar (dokter → kasir → farmasi). Tidak ada stage,
commit, atau push yang dilakukan.
