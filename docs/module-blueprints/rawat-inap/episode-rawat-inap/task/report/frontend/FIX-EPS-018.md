# FIX-EPS-018 — Penyelarasan Tampilan dan Aturan Deposit Transfer Pasien terhadap Standar Pasien Lama

Tanggal: 2026-10-14. Status: SELESAI.

Masukan: [rencana disetujui](../../../docs/plan-repair/plan-repair-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md), [dokumen issue](../../../docs/issue/issue-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md). Wewenang: persetujuan eksplisit pemilik sistem pada 14 Oktober 2026. Frontend branch `HamzahV2`, backend branch `MHamzah`.

---

## 1. Perubahan dan Source

1. **`FIX-EPS-018-01`**: Spanduk kebijakan deposit resmi (`MstDepositPolicy`) dari billing (`Ketentuan Minimum Deposit: Rp 10.000.000 • Batas Jatuh Tempo Pelunasan: 3 Hari Kerja Pasca Admisi`) ditampilkan seragam pada alur transfer pasien, dengan catatan penerbitan kwitansi admisi transfer yang terintegrasi.
2. **`FIX-EPS-018-02`**: Indikator status pemenuhan pembayaran deposit aktif secara universal:
   - Jika `amount == 0`: Status *"Belum Membayar Deposit"* dengan pencatatan kekurangan penuh Rp 10.000.000 untuk ditindaklanjuti pada Workspace PPRI.
   - Jika `0 < amount < minimumAmount`: Status *"Terbayar Sebagian"* dengan perhitungan kekurangan selisih (misal pembayaran Rp 1.000.000 menyisakan kekurangan Rp 9.000.000).
   - Jika `amount >= minimumAmount`: Status *"Lunas Memenuhi Kebijakan"*.
3. **`FIX-EPS-018-03`**: Form pemilihan metode penerimaan deposit transfer ditata secara elegan menggunakan kontainer `.depositMethodCard`, label resmi `Metode Penerimaan Deposit *`, tombol muat ulang metode kompak dengan ikon `RotateCcw` di samping label, dan petunjuk penggunaan yang jelas.
4. **`FIX-EPS-018-04`**: Teks bantuan di bawah input nominal disatukan dan dibersihkan dari pengulangan kata.
5. Invariant `RWI-DEC-095` tetap terjaga: tombol **Lanjut ke Dokter** selalu aktif (tidak pernah `disabled`), deposit tidak pernah menahan jalannya proses admisi pasien.

**Berkas yang dimodifikasi:**
- `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx`
- `QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/inpatient-admission.module.css`

---

## 2. Acceptance Criteria dan Bukti

- **AC-01 (Spanduk Kebijakan):** Langkah Deposit pada admisi transfer menampilkan spanduk kebijakan billing minimum Rp 10.000.000 dan jatuh tempo 3 hari. (Terpenuhi di `inpatient-admission-deposit-step.jsx:52-85`)
- **AC-02 (Indikator Kekurangan Bayar):** Pengisian nominal Rp 1.000.000 memunculkan peringatan kuning *"Status Pembayaran: Terbayar Sebagian"* dan mencatat kekurangan Rp 9.000.000. (Terpenuhi di `inpatient-admission-deposit-step.jsx:110-128`)
- **AC-03 (Metode Pembayaran Rapi):** Bagian metode pembayaran hanya muncul jika nominal > 0, memiliki label form, tombol reload kompak, dan terbungkus kartu visual yang rapi. (Terpenuhi di `inpatient-admission-deposit-step.jsx:144-180` dan `inpatient-admission.module.css:2490-2525`)
- **AC-04 (Pembersihan Teks Duplikat):** Kalimat di bawah input nominal rapi dan tidak mengulang kalimat yang sama. (Terpenuhi di `inpatient-admission-deposit-step.jsx:105-108, 137-142`)
- **AC-05 (Kelancaran Alur Admisi):** Petugas dapat menekan tombol *Lanjut ke Dokter* dengan nominal kosong, sebagian, maupun lunas tanpa tertahan. (Terpenuhi di `inpatient-admission-deposit-step.jsx:182-192`)

---

## 3. Governance dan UI Gate

- **UI Decision Gate:**
  - `REUSE`: `BaseButton`, `InformationAlert`, `FilterSelect`.
  - `COMPOSE`: Form group `.depositMethodCard` dan `.depositMethodHeader` menggunakan design tokens Quilvian (`var(--color-border)`, `var(--radius-lg)`, `var(--color-surface-soft)`, `var(--space-*)`).
  - `NEW`: 0 (tidak ada komponen baru).
- **Aturan Dokumentasi:** Seluruh laporan, issue, dan rencana perbaikan ditulis dalam Bahasa Indonesia baku, terstruktur runtut, dan mudah dipahami orang umum.
