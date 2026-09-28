# Laporan Perubahan Frontend — `FE-BUI-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-005` |
| Judul | Filter Perbandingan Asuransi (Penyedia Asuransi Saja & Pengecualian Asuransi Aktif) |
| Slice | Gelombang `MVP-32` — Revisi UI Billing: Perbandingan Asuransi dan Payment Method (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-005`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-005` — Filter Perbandingan Asuransi |
| Trace | `BUI-DEC-004` (Perbandingan asuransi hanya tampilkan insurance provider), `BUI-DEC-005` (Asuransi aktif dikecualikan dari daftar pembanding), `FR-BUI-004`, `FR-BUI-005`, `BUI-DES-004`, `BIL-API-1.4`, `UAT-BUI-03` |
| Contract version | `BIL-API-1.4` — `GET /{id}/edit-context`, field `AvailablePayerOptions` (nol endpoint baru) |
| Wewenang UI | Filter jenis **terkunci** desain (`BUI-DEC-004`); tata letak kartu kandidat `DEV_DISCRETION`; selector kategori `BasePayerCategorySelector` dipertahankan apa adanya |
| Dependency | Tidak ada — task tunggal independen pada Gelombang `MVP-32` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 5 (skor 0); berkas diubah 1 (skor 0); logika penyaringan data presentasional (skor 0); kontrak API tidak berubah (skor 0); database `NOT APPLICABLE` (skor 0); keamanan/auth `NOT APPLICABLE` / tidak berubah (skor 0); UI/workflow filter kandidat perbandingan (skor 0). Total skor 0 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Filter daftar kandidat perbandingan penjamin pada mode Edit Asuransi (`use-edit-asuransi-panel.js` → `optionsForSelectedCategory`) kini secara ketat hanya menampilkan penyedia asuransi (`PayerType === "INSURANCE"`) saat kategori Asuransi dipilih, dan mengecualikan opsi Tunai (`CASH`) maupun Penjamin Perusahaan (`COMPANY_GUARANTOR`) dari daftar pembanding sesuai `BUI-DEC-004` & `UAT-BUI-03`. Pengecualian asuransi aktif pasien (`BUI-DEC-005`) diverifikasi bekerja berlapis (di backend lewat `BuildAvailablePayerOptionsAsync` dan di frontend lewat pencocokan `patientInsuranceId`/`insuranceProviderId`). Kategori Tunai dan Penjamin Perusahaan pada `BasePayerCategorySelector` tetap ada dan dapat dipilih apa adanya. Verifikasi kualitas: `npm run lint:errors` lulus (0 error, exit code 0); `npm run test:unit` lulus 1.685/1.693 (8 kegagalan pre-existing konsisten); `npm run build` berhasil (`✓ Compiled successfully`, 406 rute dan standalone runtime siap). |

---

## 1. Keadaan yang ditemukan di awal

Berdasarkan audit kemampuan awal (`01-existing-capability-map.md` `CAP-BUI-04`), wawancara keputusan (`00-interview-decisions.md` `BUI-DEC-004`, `BUI-DEC-005`), dan arsitektur frontend (`03-frontend-architecture.md` Bagian 3 `BUI-DES-004`):
1. **Opsi Penjamin di Backend Bersifat Tercampur**:
   - Endpoint backend `GET /{id}/edit-context` melalui fungsi `BillingPayerEditService.BuildAvailablePayerOptionsAsync` mengembalikan daftar `AvailablePayerOptions` yang menggabungkan seluruh penjamin terdaftar pasien: opsi `CASH`, kartu `INSURANCE`, dan kartu `COMPANY_GUARANTOR`.
   - Walaupun backend sudah menandai ruas `PayerType` pada masing-masing opsi, backend tidak memfilter opsi tersebut secara eksklusif untuk kebutuhan perbandingan asuransi karena endpoint ini juga melayani pemilihan penanggung umum.
2. **Kebutuhan Isolasi Kandidat Pembanding Asuransi**:
   - Berdasarkan keputusan `BUI-DEC-004`, kasir yang membuka langkah perbandingan asuransi menghendaki agar daftar kandidat pembanding hanya menampilkan penyedia asuransi (*Insurance provider*). Opsi Tunai (Pribadi) dan Penjamin Perusahaan tidak boleh membingungkan kasir pada daftar kartu pembanding.
3. **Pengecualian Asuransi yang Sedang Aktif**:
   - Berdasarkan `BUI-DEC-005`, asuransi yang sedang aktif dipakai pasien (misal pasien sudah terdaftar menggunakan Allianz) tidak boleh muncul kembali sebagai pilihan kandidat pembanding tagihan. Backend sudah mengecualikan provider aktif, namun frontend belum memiliki proteksi pertahanan berlapis (*defense-in-depth*) terhadap kemungkinan ketidaksesuaian data.
4. **Batasan Selector Kategori**:
   - Peringatan desain menegaskan bahwa penyaringan ini **hanya** berlaku pada daftar kartu kandidat pembanding saat kategori Asuransi dipilih, dan **tidak boleh** menghilangkan tombol kategori Tunai atau Penjamin Perusahaan dari `BasePayerCategorySelector`.

---

## 2. Proses bisnis dari sisi pengguna

1. **Kasir Membuka Tab Edit Asuransi**:
   - Kasir membuka halaman Edit Tagihan pasien (`/health-services/billing-management/billing-invoices/[slug]/edit-tagihan`) dan memilih mode **Edit Asuransi**.
   - Layar menampilkan penanggung yang sedang berlaku (misalnya: *Asuransi - Allianz* atau *Tunai*), rincian tagihan, dan bagian *"Ganti menjadi"*.
2. **Kasir Memilih Kategori "Asuransi"**:
   - Kasir mengklik tombol kategori **Asuransi** pada pemilih kategori (`BasePayerCategorySelector`).
   - Sistem menampilkan kartu-kartu penjamin asuransi yang tersimpan pada profil pasien.
3. **Hanya Penyedia Asuransi yang Tampil Sebagai Pembanding (`UAT-BUI-03`)**:
   - Daftar kartu yang disajikan kepada kasir **hanya** kartu penyedia asuransi kesehatan (misal: *Prudential*, *AXA Mandiri*, *Sinarmas*).
   - Opsi *Tunai / Pribadi* dan *Penjamin Perusahaan* tidak muncul di dalam grid kartu pembanding ini (`BUI-DEC-004`).
   - Asuransi yang sedang aktif digunakan pasien saat ini otomatis tersembunyi sehingga kasir tidak dapat membandingkan tagihan dengan asuransi yang sama (`BUI-DEC-005`).
4. **Kasir Memilih Kartu dan Membandingkan Tagihan**:
   - Kasir mengklik salah satu kartu asuransi kandidat (misal: *Prudential*), lalu menekan tombol **Bandingkan**.
   - Sistem meminta pratinjau perbandingan perhitungan ke server (`POST /{id}/payer-comparison-preview`).
   - Tabel tagihan berdampingan (*SEKARANG* vs *BILA DIGANTI*) muncul menampilkan simulasi porsi tanggungan asuransi dan porsi mandiri pasien secara akurat berdasarkan kontrak tarif server.
5. **Fleksibilitas Penggantian ke Tunai atau Penjamin Perusahaan Tetap Ada**:
   - Jika kasir ingin mengubah penanggung pasien menjadi Tunai atau Penjamin Perusahaan, kasir tetap dapat mengklik tombol kategori **Tunai** atau **Penjamin Perusahaan** pada `BasePayerCategorySelector` tanpa terhalang.

---

## 3. Rincian Perubahan Berkas

Berikut adalah rincian berkas yang diubah pada repositori `QuilvianSystemFrontendDev`:

### `src/lib/hooks/health-services/billing-management/billing-invoices/use-edit-asuransi-panel.js`
- **Perubahan**:
  - Memperbarui memoized selector `optionsForSelectedCategory` dengan logika filter normalisasi:
    1. Melakukan normalisasi string `toUpperCase()` pada `PayerType` opsi dan `selectedCategory`.
    2. Saat kategori terpilih adalah `INSURANCE`, menyaring secara tegas agar hanya kartu dengan `payerType === "INSURANCE"` yang lolos, secara otomatis mengecualikan opsi `CASH` maupun `COMPANY_GUARANTOR`.
    3. Menambahkan pengecualian berlapis (*defense-in-depth*) untuk asuransi aktif pasien: memeriksa jika `currentPayer` bertipe `INSURANCE`, maka kartu kandidat dengan `patientInsuranceId` atau `insuranceProviderId` yang identik dengan penjamin aktif akan disaring keluar dari daftar pembanding.
    4. Mempertahankan perilaku normal untuk kategori lain (`COMPANY_GUARANTOR` menyaring kartu perusahaan penjamin).
    5. Menambahkan `currentPayer` ke dalam *dependency array* `useMemo` agar daftar kandidat selalu reaktif terhadap perubahan konteks penanggung aktif.

---

## 4. Evaluasi Base Component Decision Gate

Sesuai aturan rekayasa antarmuka pengguna Quilvian (*Base Component Reuse & Decision Gate*):

| Elemen Antarmuka | Komponen / Library | Keputusan | Keterangan |
| --- | --- | :---: | --- |
| Pemilih Kategori Penjamin | `@/components/features/base-features/base-payer-workspace` (`BasePayerCategorySelector`) | `REUSE` | Dipakai ulang apa adanya tanpa modifikasi CSS/struktur |
| Kartu Penjamin Tersimpan | `@/components/features/base-features/base-payer-workspace` (`BaseSavedPayerCard`) | `REUSE` | Dipakai ulang apa adanya untuk merender kartu kandidat asuransi |
| Tombol Bandingkan | `@/components/features/base-features/base-button` | `REUSE` | Memakai BaseButton varian `secondary` ukuran `sm` |
| Alert Kosong / Informasi | `@/components/features/base-features/information-alert` | `REUSE` | Memakai InformationAlert varian `info` |

```text
UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

Seluruh elemen berstatus `REUSE` 100%. Tidak ada komponen dasar yang dimodifikasi perilakunya (`EXTEND = 0`) dan tidak ada komponen baru (`NEW = 0`).

---

## 5. Bukti Verifikasi Kualitas (*Evidence-Based Verification*)

1. **Pemeriksaan Linter (`npm run lint:errors`)**:
   - Perintah: `npm run lint:errors`
   - Hasil: **LULUS (0 errors, 0 warnings, exit code 0)**.
2. **Pengujian Unit (`npm run test:unit`)**:
   - Perintah: `npm run test:unit`
   - Hasil: **1.685 lulus, 8 gagal** (8 kegagalan terbukti identik dengan baseline repositori awal `b3f45db7b`, terisolasi pada modul lama di luar billing management).
   - Seluruh test suite modul *billing management* lulus 100%.
3. **Kompilasi Produksi (`npm run build`)**:
   - Perintah: `npm run build`
   - Hasil: **LULUS (Exit code 0)**.
   - Next.js 16.2.12 berhasil mengompilasi seluruh 406 rute aplikasi dan menyiapkan standalone runtime.
4. **Verifikasi Kriteria Penerimaan Skenario `UAT-BUI-03`**:
   - Saat modal/tab perbandingan asuransi dibuka dan kategori Asuransi aktif, hanya kartu penyedia asuransi yang muncul sebagai kandidat.
   - Opsi Tunai dan Penjamin Perusahaan tidak muncul di daftar kandidat perbandingan asuransi.
   - Asuransi yang sedang aktif digunakan pasien tidak muncul di daftar kandidat pembanding.

---

## 6. Pemenuhan Acceptance Criteria & Definition of Done

| Kriteria / Syarat | Status | Bukti |
| --- | :---: | --- |
| **`UAT-BUI-03`**: Modal perbandingan asuransi dibuka → Hanya penyedia asuransi yang tampil sebagai kandidat | ✅ Terpenuhi | `optionsForSelectedCategory` menyaring `payerType === "INSURANCE"`, mengeliminasi opsi Tunai dan Penjamin Perusahaan saat kategori Asuransi aktif |
| **`BUI-DEC-004`**: Opsi pribadi dan perusahaan disembunyikan dari daftar pembanding asuransi | ✅ Terpenuhi | Terverifikasi di `use-edit-asuransi-panel.js` baris 81-140 |
| **`BUI-DEC-005`**: Asuransi aktif pasien tidak muncul di daftar pembanding | ✅ Terpenuhi | Pengecualian backend `CAP-BUI-05` diperkuat proteksi berlapis di frontend |
| Kategori Tunai & Penjamin Perusahaan tidak hilang dari `BasePayerCategorySelector` | ✅ Terpenuhi | `CATEGORY_DEFS` dan `panel.categories` tetap memuat ketiga kategori |
| `BIL-API-1.4`: Nol endpoint baru | ✅ Terpenuhi | Mengonsumsi `GET /{id}/edit-context` existing |
| `npm run lint:errors` lulus | ✅ Terpenuhi | 0 error (exit code 0) |
| `npm run test:unit` lulus / konsisten dengan baseline | ✅ Terpenuhi | 1.685 lulus, 8 kegagalan konsisten dengan baseline commit awal |
| `npm run build` lulus | ✅ Terpenuhi | Exit code 0, 406 rute berhasil dikompilasi |
| Laporan task tracked dibuat | ✅ Terpenuhi | Dokumen ini (`docs/module-blueprints/billing-kasir/task/report/frontend/FE-BUI-005.md`) |
| Roadmap & Traceability diperbarui | ✅ Terpenuhi | Node mermaid, tabel gelombang, daftar task, kartu FE-BUI-005, dan requirement-traceability diperbarui |
