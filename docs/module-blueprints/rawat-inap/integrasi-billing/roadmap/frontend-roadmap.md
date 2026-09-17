# Roadmap Delivery Frontend — Integrasi Rawat Inap ↔ Kasir / Billing

> ## Berkas Baru — Sub-modul `integrasi-billing` (Slice `INP-S22`)
>
> Berkas ini adalah register kerja pengiriman (*delivery roadmap*) frontend untuk antarmuka **Integrasi Rawat Inap ↔ Kasir / Billing**, mencakup komponen status operasional kasir tanpa rupiah di bangsal, gerbang pemulangan pasien (*Discharge Gate*), *Auto-Reblock*, dan modal otorisasi *Supervisor Override*.
>
> | Hal | Keterangan |
> |---|---|
> | Letak berkas | `docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/frontend-roadmap.md` |
> | Hubungan dengan roadmap lain | **Terpisah dan independen.** Tidak mengganggu roadmap frontend lama. |
> | Rentang Task ID | **`FE-RWI-095` s.d. `FE-RWI-100`** (6 task vertical slice terfokus) |
> | ID Bebas Berikutnya | **`FE-RWI-101`** |
> | Target Framework | Next.js 14+ App Router, React 18, Redux Toolkit, Tailwind CSS, Quilvian Design Tokens. |

---

## Metadata

```yaml
module_id: rawat-inap
submodule: integrasi-billing
slice_id: INP-S22
blueprint_id: RWI-BP-001-INT-BIL
blueprint_revision: 1.0.0
roadmap_file: docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/frontend-roadmap.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
approval_gate: APPROVED
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-17"
approval_decision: RWI-DEC-162
primary_source: "docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md"
contract_version: 1.0.0
decision_source: "docs/module-blueprints/rawat-inap/00-interview-decisions.md revision 26 (RWI-DEC-156 s.d. 162)"
frontend_source_sha: 2c00758832f834cff0288bef4f0d2fcf1161fb52
task_id_range: FE-RWI-095..FE-RWI-100
task_id_next_free: FE-RWI-101
waves: [INT-FE-1, INT-FE-2, INT-FE-3]
write_authority: "TIDAK diberikan di sini. Wewenang tulis komponen UI dan route dinyatakan terpisah per task"
```

## Legenda Status Task

| Tanda | Arti Status |
| :---: | --- |
| ✅ | Kriteria penerimaan UI dan interaksi terbukti penuh, verifikasi visual lolos |
| 🟡 | Komponen UI sudah dibuat namun integrasi endpoint/validasi belum tuntas |
| ⛔ | Prasyarat endpoint backend belum siap (tertahan) |
| tanpa tanda | Belum dikerjakan (Ready for execution setelah approval) |

---

## Grafik Urutan Dependency Frontend

```text
BE-RWI-132 (Backend API Billing Status)
    │
    ▼
FE-RWI-095 (Komponen Lencana BillingStatusBadge)
    │
    ├──► FE-RWI-096 (Integrasi Sensus & Detail Pasien)
    │        │
    │        ├──► FE-RWI-097 (Drawer Rincian Finansial Berizin)
    │        │
    │        └──► FE-RWI-098 (Gerbang Pemulangan & Auto-Reblock di Modal)
    │                 │   ▲
    │                 │   └── BE-RWI-133 (Webhook Backend)
    │                 │
    │                 ├──► FE-RWI-099 (Modal Supervisor Override Kedaruratan)
    │                 │        ▲
    │                 │        └── BE-RWI-134 (Backend Override Endpoint)
    │                 │
    │                 └──► FE-RWI-100 (Aksi Konfirmasi Pasien Pulang Fisik)
    │                          ▲
    │                          └── BE-RWI-134 (Backend Confirm Physical Discharge)
```

### Tabel Gelombang Eksekusi

| Gelombang | Prasyarat Backend | Task ID | Lingkup Pekerjaan |
|---:|---|---|---|
| **INT-FE-1** | `BE-RWI-132` | `FE-RWI-095`, `FE-RWI-096` | Komponen visual lencana status kasir warna-warni dan integrasi ke halaman sensus bangsal & detail episode. |
| **INT-FE-2** | `BE-RWI-132`, `BE-RWI-133` | `FE-RWI-097`, `FE-RWI-098` | Drawer rincian finansial berizin (`InpatientBilling:View`) dan kartu gerbang clearance & auto-reblock pada modal pemulangan. |
| **INT-FE-3** | `BE-RWI-134` | `FE-RWI-099`, `FE-RWI-100` | Modal otorisasi kedaruratan *Supervisor Override* dan tombol konfirmasi pelepasan fisik pasien pulang. |

---

## Tabel Rincian Task Frontend

| Task ID | Outcome | Requirement & Keputusan | Kontrak | Kemampuan Existing | Cakupan Pekerjaan | Dependency | Acceptance Criteria | Verifikasi | Risiko & Mitigasi |
|---|---|---|---|---|---|---|---|---|---|
| `FE-RWI-095` | Komponen visual lencana status operasional kasir warna-warni tanpa rupiah | `FR-INT-011`, `FR-INT-012`; `RWI-DEC-160`, `RWI-AC-240` | `1.0.0` — Skema §1, Frontend §4 | Design Token Badge | Pembuatan komponen `BillingStatusBadge.jsx` dengan dukungan 6 varian status (`OPEN`, `PENDING_CLEARANCE`, `CLEARED`, `REVOKED`, `OVERRIDDEN`, `CLOSED`). Steril dari angka rupiah. | `BE-RWI-132` | AC-1: Lencana menampilkan label Bahasa Indonesia yang tepat.<br>AC-2: Varian warna sesuai state (biru, kuning, hijau, merah berkedip).<br>AC-3: Tidak ada teks angka rupiah yang dirender. | Storybook / Jest component render test | Lencana membingungkan perawat; teks label disesuaikan dengan istilah medis RS. |
| `FE-RWI-096` | Halaman Sensus dan Detail Episode menampilkan status kasir real-time | `FR-INT-011`, `FR-INT-012`; `RWI-DEC-160`, `RWI-AC-240` | `1.0.0` — Skema §1 & §2 | `FE-INP-01`, `FE-INP-04` | Integrasi `BillingStatusBadge` pada kolom tabel sensus (`FE-INP-01`) dan kartu `BillingSummaryCard.jsx` pada tab administrasi detail episode (`FE-INP-04`). Implementasi hook `useInpatientBillingStatus` dengan polling berkala 30 detik. | `FE-RWI-095` | AC-1: Kolom status kasir tampil di sensus bangsal.<br>AC-2: Kartu status kasir menampilkan status dan daftar kendala blocker.<br>AC-3: Polling berjalan di background tanpa flicker UI. | Browser verification di Chrome/Firefox | Polling memicu re-render berlebih; dicegah via memoization React. |
| `FE-RWI-097` | Panel geser rincian finansial kasir hanya terbuka bagi staf berizin | `FR-INT-013`; `RWI-DEC-160`, `RWI-AC-240` | `1.0.0` — Skema §5 | Drawer Component | Pembuatan komponen `BillingFinancialDetailsDrawer.jsx` yang menampilkan rincian biaya rupiah, deposit, dan sisa kurang bayar. Tombol pemicu hanya muncul jika user memiliki klaim `InpatientBilling:View`. | `FE-RWI-096` | AC-1: Tombol pembuka terkunci/tersembunyi bagi perawat biasa.<br>AC-2: Pemegang izin dapat membuka drawer rincian finansial.<br>AC-3: Format mata uang rupiah terformat rapi (IDR). | Role-based authorization test | Pelanggaran privasi keuangan; tombol tidak dirender di DOM bagi non-keuangan. |
| `FE-RWI-098` | Gerbang pemulangan fisik terkunci saat belum lunas & auto-reblock aktif | `FR-INT-014`, `FR-INT-015`; `RWI-DEC-158`, `RWI-AC-238` | `1.0.0` — Skema §3 | `FE-INP-06` | Pembuatan kartu `DischargeClearanceGateCard.jsx` pada modal pemulangan. Tombol pulang fisik disabled saat status pending/revoked, aktif saat cleared. Banner peringatan merah berkedip saat status revoked (*Auto-Reblock*). Polling cepat 10 detik saat modal terbuka. | `FE-RWI-096`, `BE-RWI-133` | AC-1: Tombol konfirmasi pulang fisik disabled saat pending.<br>AC-2: Tombol aktif saat clearance kasir disetujui.<br>AC-3: Auto-reblock seketika mengunci tombol saat clearance dicabut kasir. | Modal state transition test | Keterlambatan deteksi pencabutan kasir; dimitigasi via polling 10 detik pada modal aktif. |
| `FE-RWI-099` | Modal otorisasi supervisor override untuk evakuasi darurat klinis | `FR-INT-016`; `RWI-DEC-158`, `RWI-AC-238` | `1.0.0` — Skema §4 | Modal Component | Pembuatan `SupervisorOverrideModal.jsx`. Muncul saat status clearance revoked/pending dan ditekan oleh pengguna berizin `InpatientSupervisor:Override`. Memvalidasi teks alasan darurat (>= 20 karakter) dan input PIN otorisasi. | `FE-RWI-098`, `BE-RWI-134` | AC-1: Tombol override hanya tampil bagi supervisor.<br>AC-2: Validasi alasan minimal 20 karakter bekerja di klien.<br>AC-3: Pengiriman form memanggil endpoint override backend dan menyegarkan modal pemulangan. | Form validation & submission test | Salah pencet darurat; modal dilengkapi peringatan resiko hukum dan konfirmasi PIN. |
| `FE-RWI-100` | Konfirmasi kepulangan fisik pasien mengunci jam dan melepas bed | `FR-INT-005`, `FR-INT-006`; `RWI-DEC-159`, `RWI-AC-239` | `1.0.0` — Skema §3.1 | `FE-INP-06` | Penanganan klik tombol `Konfirmasi Pasien Pulang Fisik` yang memanggil `POST .../confirm-physical-discharge` dengan stempel waktu saat ini (`PhysicallyLeftAt`), menampilkan notifikasi sukses, menutup modal, dan memperbarui status bed di sensus menjadi perlu pembersihan. | `FE-RWI-098`, `BE-RWI-134` | AC-1: Tombol memicu dialog konfirmasi akhir.<br>AC-2: Jam keluar fisik terkirim presisi ke backend.<br>AC-3: Status tempat tidur berubah seketika di layar sensus. | End-to-end user flow test | Duplikasi submit; tombol di-disable seketika dengan loading spinner saat diklik. |

---

## Kartu Task Frontend Detil

### Kartu `FE-RWI-095`: Komponen Lencana BillingStatusBadge
- **Outcome:** Komponen `BillingStatusBadge.jsx` siap pakai dengan 6 status operasional kasir.
- **Kriteria Penerimaan (AC):**
  1. Komponen menerima prop `status` dan me-render label Bahasa Indonesia yang sesuai: `Tagihan Berjalan` (biru), `Menunggu Kasir` (kuning), `Clearance Disetujui` (hijau), `Clearance Dicabut` (merah berkedip), `Override Supervisor` (ungu), `Tagihan Selesai` (abu-abu).
  2. Komponen steril dari angka rupiah dan saldo piutang.
  3. Desain responsif dan konsisten dengan token desain Quilvian (Tailwind CSS).
- **Definition of Done (DoD):** Komponen unit test render lulus, file tersimpan di `src/components/features/inpatient/billing-integration/BillingStatusBadge.jsx`.

### Kartu `FE-RWI-096`: Integrasi Sensus & Detail Pasien
- **Outcome:** Kolom status kasir terpasang di sensus bangsal (`FE-INP-01`) dan kartu ringkasan di detail episode (`FE-INP-04`).
- **Kriteria Penerimaan (AC):**
  1. Tabel sensus bangsal menampilkan kolom status kasir dengan komponen `BillingStatusBadge`.
  2. Tab administrasi pada detail episode menampilkan kartu `BillingSummaryCard.jsx` yang memuat status kasir dan daftar kendala blocker dalam bullet points.
  3. Hook `useInpatientBillingStatus` melakukan polling setiap 30 detik tanpa menyebabkan layar berkedip (*no UI flicker*).
- **Definition of Done (DoD):** Verifikasi tampilan di layar sensus dan detail episode di browser berhasil.

### Kartu `FE-RWI-097`: Drawer Rincian Finansial Berizin
- **Outcome:** Panel rincian finansial kasir dapat dibuka hanya oleh pemegang permission `InpatientBilling:View`.
- **Kriteria Penerimaan (AC):**
  1. Tombol "Buka Rincian Biaya Finansial" hanya muncul jika user memiliki permission `InpatientBilling:View`.
  2. Saat tombol diklik, panel drawer meluncur dari sisi kanan menampilkan rincian total biaya, penjamin, excess, deposit, dan sisa kurang bayar berformat rupiah (IDR).
  3. User tanpa hak akses tidak dapat melihat tombol ini atau mengakses datanya.
- **Definition of Done (DoD):** Komponen drawer terpasang, pengetesan izin peran kasir vs perawat lulus.

### Kartu `FE-RWI-098`: Gerbang Pemulangan Fisik & Auto-Reblock
- **Outcome:** Modal pemulangan pasien (`FE-INP-06`) mengontrol tombol pemulangan fisik berdasarkan status clearance kasir.
- **Kriteria Penerimaan (AC):**
  1. Komponen `DischargeClearanceGateCard.jsx` memeriksa kelayakan: jika clearance belum `Cleared`, tombol `Konfirmasi Pasien Pulang Fisik` terkunci (*disabled*).
  2. Begitu status berubah `Cleared`, tombol menjadi aktif dan berwarna hijau.
  3. Bila sinyal `ClearanceRevoked` diterima, sistem seketika mengeksekusi *Auto-Reblock*: tombol kembali disabled dan muncul banner peringatan merah berkedip dengan alasan pencabutan kasir.
  4. Polling beroperasi cepat setiap 10 detik saat modal pemulangan sedang aktif dibuka.
- **Definition of Done (DoD):** Verifikasi interaksi modal saat status pending, cleared, dan revoked lulus 100%.

### Kartu `FE-RWI-099`: Modal Supervisor Override Kedaruratan
- **Outcome:** Modal otorisasi kedaruratan klinis memungkinkan supervisor memulangkan pasien darurat saat clearance terblokir.
- **Kriteria Penerimaan (AC):**
  1. Tombol "Supervisor Override" hanya tampil jika status clearance adalah `Revoked` atau `Pending` DAN pengguna memiliki peran Supervisor Bangsal.
  2. Modal `SupervisorOverrideModal.jsx` menampilkan peringatan tanggung jawab hukum dan kolom input alasan klinis wajib.
  3. Validasi form menolak submit jika alasan klinis kurang dari 20 karakter.
  4. Form meminta verifikasi PIN supervisor sebelum tombol submit aktif.
  5. Pengiriman sukses memicu toast notifikasi dan menyegarkan status modal pemulangan menjadi `Overridden`.
- **Definition of Done (DoD):** Form validasi dan submit ke endpoint supervisor override berhasil diuji.

### Kartu `FE-RWI-100`: Konfirmasi Kepulangan Fisik & Pelepasan Bed
- **Outcome:** Klik tombol konfirmasi pemulangan fisik mencatat `PhysicallyLeftAt` presisi dan memperbarui status kamar.
- **Kriteria Penerimaan (AC):**
  1. Mengklik tombol `Konfirmasi Pasien Pulang Fisik` memunculkan dialog konfirmasi kepulangan nyata.
  2. Konfirmasi mengirimkan request `POST .../confirm-physical-discharge` dengan stempel waktu saat ini.
  3. Setelah respons sukses diterima, modal tertutup, muncul toast sukses, dan baris pasien di sensus bangsal diperbarui (tempat tidur menjadi kosong / perlu dibersihkan).
  4. Tombol dilengkapi pencegahan klik ganda (*double-click prevention*) via state loading spinner.
- **Definition of Done (DoD):** Alur pemulangan fisik end-to-end berhasil diverifikasi di lingkungan frontend.
