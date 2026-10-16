# Laporan Perubahan Frontend — `FE-FIN-047`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-047` |
| Judul | Billing: pembagian dua porsi dan Berita Acara Koreksi |
| Slice | Slice B — Finalisasi Tagihan & Berita Acara Koreksi (`REV-19`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-220`..`222`, `231`..`234`, `246`, `248`, `251`, `FIN-DEC-183`, `185`, `200`, `208`, `218` |
| Kontrak | `FIN-API-1.10` (approved sisi Finance dan HR); `FIN-INTEGRATION-1.8` §`P.1`, §`P.6` |
| Wewenang UI | `03-frontend-architecture.md` §24.1–§24.4 (`FE-FIN-047-A` panel pembagian dua porsi pada finalisasi tagihan, `FE-FIN-047-B` form Berita Acara Koreksi salah orang setelah tagihan final) |
| Dependency | `BE-FIN-107` ✅, `BE-FIN-109` ✅, `BE-FIN-119` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (constants, slice, hook, split section component & style, correction modal & style, finalize modal, finalization panel, finance integration panel, detail billing view) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-047` mengimplementasikan dua sub-layar pada modul Billing Management terkait penjamin internal benefit karyawan:

### 1.1 `FE-FIN-047-A` — Panel Pembagian Dua Porsi Tagihan & Pilihan Pegawai
1. **Penyajian Authoritative Dua Porsi Tagihan**: Menampilkan rincian porsi tagihan persis dari backend (`preview.employeeBenefitSplit` / `FinalizationPreviewResponse`) tanpa perhitungan matematika porsi di sisi klien (`FIN-DEC-185`):
   - Ditanggung aturan penjamin ($C$)
   - Sisa plafon benefit karyawan aktif dari HR ($S$)
   - Porsi RS Benefit ($P = \min(C, S)$)
   - Kelebihan di atas plafon yang menjadi utang pegawai ($E = C - P$)
   - Sisa tagihan tanggung jawab pasien / co-pay ($K$)
2. **Pilihan Pegawai atas Sisa Tagihan Pasien ($D$)**:
   - Pilihan radio:
     - `Bayar lunas di kasir` ($D = 0$): sisa tagihan pasien dibayar tunai.
     - `Jadikan utang seluruhnya` ($D = K$): seluruh sisa tagihan dialihkan ke utang pegawai (potong gaji).
     - `Sebagian` ($0 < D < K$): pegawai mengalihkan nominal tertentu ke utang pegawai.
   - Ringkasan hasil pilihan pegawai menampilkan:
     - Total Porsi RS Benefit: $P$
     - Total Porsi Utang Pegawai: $E + D$
     - Kewajiban Tunai di Kasir: $K - D$
3. **Validasi Batasan Pilihan $D$**: Klien memvalidasi $0 \le D \le K$. Jika nominal melebihi sisa tagihan pasien ($D > K$), sistem menampilkan pesan penolakan dan menonaktifkan pengajuan finalisasi (`FIN-VAL-262`, AC 5).
4. **Isian Nama Pemberi Persetujuan (`FIN-DEC-218`)**: Muncul secara kondisional bila pasien bukan pegawai sendiri (`relationshipCode !== "SELF"`) dan $D > 0$. Wajib diisi sebelum finalisasi dapat diajukan (`FIN-VAL-289`).
5. **Penahanan Finalisasi (`BlockingMessage`)**: Jika backend mengindikasikan benefit tidak dapat dipastikan (`isUsable === false` atau ada `blockingMessage`), banner peringatan bahaya/penahanan ditampilkan dan tombol finalisasi invoice dinonaktifkan (`FIN-VAL-264`).

### 1.2 `FE-FIN-047-B` — Formulir Berita Acara Koreksi Salah Orang
1. **Akses Bersyarat**: Tombol **"Ajukan Berita Acara Koreksi"** hanya muncul pada tagihan yang sudah final (`isFinal === true`) dan memiliki penjamin internal benefit (`hasInternalBenefit === true`). Tombol ini **tidak pernah muncul** untuk invoice non-internal (`FIN-DEC-200`, AC 4).
2. **Modal Berita Acara Koreksi**:
   - Field Nomor Berita Acara Resmi (`officialReportNumber`): wajib diisi.
   - Field Alasan Koreksi (`reason`): wajib diisi (`FIN-DEC-200`, AC 3).
   - Pencarian Pegawai Pengganti yang Benar (`correctEmployeeId`): input pencarian debounced minimal 3 karakter memanfaatkan `EmployeeBenefitLookupService.searchEmployees` dengan NIK tersamar.
   - Pilihan Anggota Keluarga Pengganti (`correctFamilyMemberId`): dropdown keluarga dari `EmployeeBenefitLookupService.getFamilyMembers`.
   - Nama Pemberi Persetujuan (`consentGivenBy`): teks opsional.
   - Catatan Tambahan (`notes`): teks opsional.
3. **Eksekusi Koreksi Transaksional**: Memanggil endpoint backend `POST /api/v1/health-services/billing-management/billing/finalizations/invoices/{invoiceId}/correct-benefit-debtor`. Setelah sukses, status handoff langsung dimuat ulang (`refreshHandoffs`) dan mutasi koreksi pembalik/pengganti tampil pada tabel koreksi dan AR handoffs.

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 6 elemen — REUSE 3, EXTEND 0, COMPOSE 3, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Ringkasan pembagian 2 porsi ($C, S, P, E, K$) | `<dl>` / grid kartu ringkasan + `StatusBadge` + `InformationAlert` | `components/features/base-features/information-alert`, `status-badge`, `billing-finalization-panel.jsx` | `COMPOSE` | Susun card ringkasan terstruktur dengan CSS Module menggunakan design token |
| Kontrol pilihan pegawai atas sisa tagihan pasien ($D = 0$, $D = K$, $0 < D < K$) | Radio group + numeric input + ringkasan kalkulasi backend | Input radio standar + input text/number terkelola | `COMPOSE` | Rangkai kontrol radio interaktif 3 mode dengan input nominal terkelola dan validasi batas $0 \le D \le K$ |
| Input nama pemberi persetujuan (`RequiresConsent`) | Form input text terkelola | `FinalizeInvoiceModal.jsx` | `REUSE` | Tampilkan input teks secara kondisional saat `requiresConsent` / bukan diri sendiri dan $D > 0$ |
| Alert penahanan finalisasi / benefit unconfirmed | `InformationAlert` | `components/features/base-features/information-alert` | `REUSE` | Gunakan `InformationAlert` varian `warning`/`danger` dengan pesan dari `blockingMessage` |
| Tombol aksi "Ajukan Berita Acara Koreksi Salah Orang" | `BaseButton` | `components/features/base-features/base-button` | `REUSE` | Tombol `variant="secondary"`, hanya tampil bila invoice berpenjamin internal dan berstatus final |
| Modal Formulir Berita Acara Koreksi | React Bootstrap `Modal`, `BaseButton`, `InformationAlert`, form controls | `components/features/base-features/base-button`, `finalize-invoice-modal.jsx`, `create-adjustment-modal.jsx` | `COMPOSE` | Rangkai modal dialog berstandar billing Quilvian dengan pencarian debounced pegawai via `EmployeeBenefitLookupService` |

### Pilihan Bernomor untuk Elemen COMPOSE:

> **Keputusan 1: Komposisi Panel Pembagian Dua Porsi (`FE-FIN-047-A`)**
> - **A. COMPOSE kartu ringkasan terstruktur dengan CSS Module menggunakan design token (`--color-surface-soft`, `--color-border`, `--color-primary`, dll.) — Rekomendasi.** Menghadirkan hierarki visual yang jelas antara porsi RS Benefit dan utang pegawai, konsisten 100% dengan design system Quilvian tanpa ketergantungan library luar.
> - **B. Tabel HTML datar tanpa kartu ringkasan.** Kontras visual antara porsi tanggungan RS vs utang pegawai menjadi kabur dan kurang cepat dipahami kasir/petugas billing.
>
> **Dipilih: Opsi A (Rekomendasi).**

> **Keputusan 2: Komposisi Kontrol Pilihan Pegawai atas Sisa Tagihan Pasien ($D$)**
> - **A. COMPOSE kontrol radio interaktif 3 mode (Bayar Kasir, Jadikan Utang Seluruhnya, Sebagian) — Rekomendasi.** Memberikan pengalaman kasir yang intuitif tanpa harus menghitung manual nominal sisa pasien, didukung validasi client-side batas $0 \le D \le K$.
> - **B. Hanya satu field input teks nominal bebas.** Menyulitkan kasir karena harus menghafal dan mengetik ulang nominal sisa tagihan pasien.
>
> **Dipilih: Opsi A (Rekomendasi).**

> **Keputusan 3: Modal Formulir Berita Acara Koreksi (`FE-FIN-047-B`)**
> - **A. COMPOSE modal dialog terisolasi (`CorrectBenefitDebtorModal`) menggunakan React Bootstrap `Modal`, `BaseButton`, `InformationAlert`, dan pencarian pegawai ter-debounce — Rekomendasi.** Memastikan form koreksi tidak mengganggu alur tampilan detail invoice utama dan menyajikan formulir yang aman dan fokus.
> - **B. Form inline di halaman detail invoice.** Mengacaukan tata letak detail invoice yang sudah padat dan meningkatkan risiko submit koreksi tidak sengaja.
>
> **Dipilih: Opsi A (Rekomendasi).**

---

## 3. Berkas yang Dibuat dan Diubah

### Berkas Baru
1. `src/components/view/health-services/billing-management/billing-invoices/detail/billing-employee-benefit-split-section.module.css` — Stylesheet CSS Module berbasis token desain untuk card pembagian dua porsi dan kontrol pilihan pegawai.
2. `src/components/view/health-services/billing-management/billing-invoices/detail/billing-employee-benefit-split-section.jsx` — Komponen tampilan pembagian dua porsi tagihan ($C, S, P, E, K$), pilihan pegawai atas sisa pasien ($D$), isian persetujuan non-self, dan pesan penahanan.
3. `src/components/view/health-services/billing-management/billing-invoices/detail/correct-benefit-debtor-modal.module.css` — Stylesheet CSS Module untuk dropdown autocomplete pencarian pegawai dan rincian pegawai terpilih.
4. `src/components/view/health-services/billing-management/billing-invoices/detail/correct-benefit-debtor-modal.jsx` — Komponen modal dialog Berita Acara Koreksi salah orang pasca-finalisasi.

### Berkas yang Diubah
1. `src/lib/hooks/health-services/billing-management/billing-invoices/billing-finalization-constants.js` — Menambahkan label debitur `EMPLOYEE_BENEFIT` ("Benefit Karyawan (Utang Pegawai)") dan konstanta `EMPLOYEE_RECEIVABLE_CHOICE_MODES`.
2. `src/lib/state/slice/health-services/billing-management/billing-finalization-slice.jsx` — Memperluas `finalizeBillingInvoice` untuk mengirim `employeeReceivableChoiceAmount` dan `employeeReceivableConsentGivenBy`; menambahkan async thunk `correctBenefitDebtor` beserta state dan selektornya.
3. `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-finalization.js` — Menambahkan form state pilihan pegawai, validasi batas nominal $0 \le D \le K$, validasi persetujuan non-self, serta pengelolaan state dan aksi `correctBenefitDebtor`.
4. `src/components/view/health-services/billing-management/billing-invoices/detail/finalize-invoice-modal.jsx` — Mengintegrasikan `BillingEmployeeBenefitSplitSection` interaktif di dalam modal finalisasi dan menonaktifkan tombol submit bila benefit terblokir/tertahan.
5. `src/components/view/health-services/billing-management/billing-invoices/detail/billing-finalization-panel.jsx` — Menampilkan `BillingEmployeeBenefitSplitSection` pada tab checklist kesiapan finalisasi invoice.
6. `src/components/view/health-services/billing-management/billing-invoices/detail/billing-finance-integration-panel.jsx` — Menampilkan `BillingEmployeeBenefitSplitSection` pada tab integrasi finance, label debitur utang pegawai, dan tombol "Ajukan Berita Acara Koreksi" khusus invoice penjamin internal final.
7. `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx` — Menghubungkan props pratinjau, finalisasi, dan merender `CorrectBenefitDebtorModal`.

---

## 4. Pemenuhan Acceptance Criteria

| # | Acceptance Criteria | Bukti Implementasi |
|:--:|---|---|
| 1 | Tagihan Rp 5.000.000 tampil sebagai Rp 3.000.000 (RS Benefit) dan Rp 2.000.000 (pegawai) sesuai backend | `BillingEmployeeBenefitSplitSection` menampilkan angka $P$ (`rsBenefitAmount`) dan $E$ (`excessOverAllowanceAmount`) langsung dari respons DTO backend `EmployeeBenefitSplitResponse` tanpa modifikasi angka. |
| 2 | Tidak ada perhitungan porsi di browser | Seluruh formula pembagian porsi ($P = \min(C, S)$, $E = C - P$) dan saldo plafon dihitung secara authoritative di backend (`BillingEmployeeBenefitSplitService.cs`); frontend hanya menyajikan data hasil evaluasi backend (`FIN-DEC-185`). |
| 3 | Berita Acara tanpa alasan ditolak | Pada `use-billing-finalization.js` (`confirmCorrectDebtor`), field `reason` dan `officialReportNumber` divalidasi tidak boleh kosong. Jika kosong, submit dicegah dan pesan error ditampilkan. |
| 4 | Tombol koreksi tidak muncul untuk invoice non-internal | Pada `billing-finance-integration-panel.jsx`, tombol "Ajukan Berita Acara Koreksi" dipagari oleh `hasInternalBenefit && isFinal`. Untuk invoice pasien umum atau asuransi komersial biasa, tombol ini tidak pernah dirender (`FIN-DEC-200`). |
| 5 | Pegawai menjadikan co-pay utang: ringkasan menampilkan utang pegawai dan tunai persis dari backend; nominal melebihi sisa tagihan ditolak dengan pesan sistem | Kontrol pilihan pegawai mendukung mode `DEBT` dan `PARTIAL`. Jika $D > K$, sistem menampilkan pesan error dan memblokir submit (`FIN-VAL-262`). Saat difinalisasi, backend mengeksekusi serah terima dengan rincian $E + D$ dan $K - D$. |

---

## 5. Hasil Validasi

- **ESLint**: `npx eslint` pada seluruh 9 berkas target: **PASS (exit code 0, 0 error)**.
- **Grep Anti-Regresi**:
  - Warna literal / `!important` pada CSS baru: **PASS (0 temuan)**.
  - Raw button / raw table pada view baru: **PASS (0 temuan)**.
- **Runtime / Dev Server**: **NOT RUN** (mematuhi instruksi eksplisit pengguna: *"Jangan jalankan npm run dev. Biarkan saya yang running dan test"*).
