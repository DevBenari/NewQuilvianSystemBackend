# Laporan Perubahan Frontend — `FE-HRD-R40`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R40` |
| Judul | Migrasi Restitusi dan pasang layar yang tertinggal |
| Slice | `S-B6` (Potongan Gaji & Restitusi), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R40`; task asal `FE-HRD-146`–`156` |
| Trace | API Contract v15 bagian 21; HRD-DEC-130, HRD-DEC-133; PP 36/2021 (batas potongan 50%); VR-DED-08, VR-DED-22 |
| Contract version | API Contract v15 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component, rute baru, pemasangan dua modal |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 10 berkas, 42 pelanggaran, 1 rute baru |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — antrean persetujuan punya rute dan menu, pratinjau nota dan voucher QR dapat dibuka, semua data contoh dihapus; unggah nota dan beberapa aturan server tertahan backend |

---

## 1. Keadaan yang ditemukan di awal

- Kelima layar Restitusi memakai data contoh dan mengembalikan data contoh di blok `catch`.
- Antrean persetujuan klaim tidak punya rute; ketiga halaman `/corporate/hrd/payroll/deductions/*` juga tidak ada di menu.
- Antrean memakai simulasi peran dan akun ("Ns. Siti Rahmawati"), `prompt()`/`alert()`, anti-persetujuan-diri di peramban, variant tombol "outline" yang tidak sah, dan alasan tolak minimal 5 karakter (server 15).
- Modal voucher menampilkan token "CITO-882194", nominal, dan nama rekaan dengan QR palsu.
- ESS mengirim `employeeId` "emp-ded-001" (bukan GUID) dan URL nota "/uploads/receipts/<nama>" rekaan, sehingga pengajuan selalu gagal.
- Simulasi batas 50% dihitung di peramban dengan gaji rekaan 8.950.000; koreksi salah potong mengirim field yang tidak dikenal server.
- Tombol sinkron Finance mengirim objek ke endpoint yang meminta daftar piutang (`List<FinanceReceivableSyncItemDto>`), sehingga selalu gagal.
- Pengaturan menampilkan parameter yang tidak ada di server (ambang peringatan, batas hari klaim, masa QR, cut-off sinkron, minimal karakter).

## 2. Proses bisnis dari sisi pengguna

1. **Pegawai (ESS)** melihat sisa plafon kacamata kerja dan faskes luar, lalu mengajukan klaim: kategori, tanggal nota (tidak boleh masa depan), nominal, tautan berkas nota, opsional resume medis, dan penanda darurat cito.
2. **Atasan** (tab Menunggu Atasan) menyetujui atau menolak (alasan minimal 15 karakter).
3. **HR C&B** (tab Verifikasi HR) memeriksa nota lewat **pratinjau nota** (perbesar, putar, buka di tab baru) dan dapat mengisi nominal terverifikasi.
4. **Persetujuan final** menetapkan nominal disetujui. Klaim reguler masuk antrean payroll; klaim cito yang disetujui dapat diterbitkan **voucher kasir 24/7** — modal voucher menampilkan nomor, token keamanan (sekali tampil), QR, nominal, dan masa berlaku. Voucher yang pernah terbit dilihat lagi dari tab **Voucher Kasir**.
5. **Dasbor** menampilkan klaim yang menunggu, klaim cito, jumlah jadwal angsuran aktif, potongan per bulan, dan sisa pokok piutang.
6. **Jadwal angsuran** menampilkan cicilan per pegawai/prioritas/status, **simulasi batas 50%** dari server, dan **koreksi salah potong** periode lampau (dikembalikan sebagai restitusi non-pajak).
7. **Pengaturan** mengelola plafon per grade dan kategori, minimal masa kerja, batas potongan, izin kasir cito, dan memuat standar bawaan.
8. Jalur tidak normal: akun tanpa data karyawan diberi peringatan dan tombol ajukan nonaktif; tanpa hak tampil peringatan; galat server tampil.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`RestitutionClaimController`, `PayrollDeductionScheduleController`, `CashierVoucherController`, `RestitutionSettingController`, DTO `RestitutionClaimDtos`, `CashierVoucherDtos`, `DeductionScheduleDtos`, `PayrollDeductionEngineDtos`, `RestitutionSettingDtos`, layanan `RestitutionClaimService`, `CashierVoucherDisbursementService`, `RestitutionSettingService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/restitution/restitution-constants.js` | BARU: rute, status klaim/voucher/jadwal, tahap persetujuan, label kategori, resource siklus payroll |
| `src/style/hr/restitution/restitution-preview.module.css` | BARU: bingkai pratinjau dan kartu voucher berbasis token |
| `restitution-approvals-view.jsx` | Ditulis ulang: antrean 3 tingkat + tab Voucher Kasir |
| `deductions-dashboard-view.jsx`, `deduction-schedules-view.jsx`, `ess-restitution-claims-view.jsx`, `restitution-settings-view.jsx` | Ditulis ulang |
| `ModalClaimDocumentPreview.jsx`, `ModalVoucherQrViewer.jsx` (QR `qrcode.react`), `DeductionCapSimulationModal.jsx`, `EssClaimFormModal.jsx` | Ditulis ulang |
| `PastPeriodCorrectionModal.jsx` | BARU (pengganti drawer) |
| `src/app/corporate/hrd/payroll/deductions/approvals/page.jsx` | RUTE BARU |
| `restitution-service.js` | `seedDefaultSettings` |
| `menu-items.jsx` | Empat menu baru dengan permission |
| `PastPeriodCorrectionDrawer.jsx`, `restitution.module.css` (56 literal warna), `restitution-display-utils.js` | Dihapus |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`. Identitas pegawai ESS dari `state.auth.userInfo.employeeId` (ID `MstEmployees`, sesuai FK klaim).

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, tab, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `ResourceFilterSelect`, `DataTable` | `REUSE` | — |
| Pratinjau nota | `ConfirmModal` + `restitution-preview.module.css` | `NEW` (CSS Module) | Bingkai zoom/rotasi belum dicakup base component; skala via variabel CSS |
| Kartu voucher + QR | `ConfirmModal` + `QRCodeSVG` + CSS Module | `NEW` (CSS Module) | Pola QR sudah dipakai `base-patient-card` |
| Dialog persetujuan/formulir | `ConfirmModal` + `BaseFormControl` + `HrResourceField` | `COMPOSE` | — |

`UI GATE: PASS — 4 elemen REUSE, 1 COMPOSE, 2 NEW (CSS Module berbasis token)`

```text
$ node .../check-css-usage.mjs <10 berkas JSX restitusi>
OK — 1 berkas diperiksa, 0 temuan. (×10)
$ grep Tailwind <berkas R40>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/ringkasan |
| Kosong | "Tidak ada klaim pada tab ini." / "Belum ada voucher kasir." / "Belum ada kebijakan. Gunakan Muat Standar Bawaan…" |
| Gagal | Pesan server di `InformationAlert`, tanpa data contoh |
| Tanpa hak akses | "Anda belum punya hak untuk melihat …" |

## 5. Endpoint yang dikonsumsi

#### HR Deductions - Restitution Claims

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/hr-deductions/claims/quota` | Sisa plafon ESS | `RestitutionClaim : ViewOwn` |
| `POST` | `/hr-deductions/claims` | Ajukan klaim | `RestitutionClaim : Submit` |
| `GET` | `/hr-deductions/claims` | Antrean dan riwayat | `RestitutionClaim : Read` |
| `POST` | `/hr-deductions/claims/{id}/approve-supervisor` | Setuju atasan | `RestitutionClaim : ApproveSupervisor` |
| `POST` | `/hr-deductions/claims/{id}/review-hr` | Verifikasi HR | `RestitutionClaim : ReviewHr` |
| `POST` | `/hr-deductions/claims/{id}/approve-final` | Setuju final | `RestitutionClaim : ApproveFinal` |
| `POST` | `/hr-deductions/claims/{id}/reject` | Tolak | `RestitutionClaim : Reject` |

#### HR Deductions - Restitutions

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/hr-deductions/restitutions/salary-correction` | Koreksi salah potong | `RestitutionClaim : Correct` |

#### HR Deductions - Cashier Vouchers & Schedules

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/hr-deductions/vouchers` | Terbitkan voucher cito | `CashierVoucher : Issue` |
| `GET` | `/hr-deductions/vouchers` | Daftar voucher | `CashierVoucher : Read` |
| `GET` | `/hr-deductions/schedules` | Jadwal angsuran | `PayrollDeductionSchedule : Read` |

#### HR Deductions - Engine

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/hr-deductions/engine/simulate` | Simulasi batas 50% | `PayrollDeductionSchedule : Read` |

#### HR Deductions - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/hr-deductions/settings` | Daftar kebijakan | `RestitutionSetting : Read` |
| `POST` | `/hr-deductions/settings` | Simpan (buat/perbarui per grade+kategori) | `RestitutionSetting : Manage` |
| `POST` | `/hr-deductions/settings/seed-defaults` | Muat standar bawaan | `RestitutionSetting : Manage` |

Pendukung: `GET /v1/corporate/payroll-management/payroll-runs` (pilihan siklus), `master-data/employees/options`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 10 JSX + rute + konstanta + service + menu | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 10 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-2 | 832/832 halaman (termasuk rute `deductions/approvals`), `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| Antrean persetujuan klaim bisa dibuka | Terpenuhi | Rute baru + menu "Persetujuan Klaim Restitusi" |
| Ketiga komponen dapat dibuka dari menu | Terpenuhi (source) | Menu → antrean → Lihat Nota (pratinjau) → Terbitkan Voucher / tab Voucher Kasir (voucher QR) |
| Unggah nota klaim | Belum terpenuhi | Server mewajibkan `ReceiptAttachmentUrl` tanpa endpoint unggah; ESS mengisi tautan |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Anti-persetujuan-diri di server membandingkan `claim.EmployeeId` (`MstEmployees`) dengan `userId`; kemungkinan hanya pencocokan `CreateBy` yang efektif |
| Masalah yang diketahui | `GET claims` butuh `RestitutionClaim : Read`, sehingga pegawai yang hanya punya `ViewOwn` tidak dapat melihat riwayat klaimnya; tidak ada `PUT` pengaturan dan log audit pengaturan; sinkron piutang Finance tidak dapat dipicu dari layar |
| Dependency backend | Unggah berkas nota, perbaikan anti-self-approval, endpoint riwayat klaim milik sendiri, log audit pengaturan |
| Perubahan sampingan | Drawer, CSS 56 literal warna, dan util lama dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Perbaikan backend butir di atas; uji browser alur klaim end-to-end |
