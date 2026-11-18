# Laporan Perubahan Frontend — `FE-HRD-R37`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R37` |
| Judul | Migrasi base component Cuti |
| Slice | `S-B2` (Cuti & Izin), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R37`; task asal `FE-HRD-103`–`114` |
| Trace | API Contract v12 bagian 18; HRD-DEC-110 s.d. HRD-DEC-115 |
| Contract version | API Contract v12 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component; fungsi cuti tidak berubah |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 14 berkas Lampiran B, 67 pelanggaran |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — 14 berkas bermigrasi dan alur tersambung benar; metadata surat sakit, verifikasi surat dokter, dan daftar penyesuaian retrospektif tertahan backend |

---

## 1. Keadaan yang ditemukan di awal

Kartu menyebut "data sudah tersambung", tetapi audit source menemukan banyak salah kontrak:

- Kartu saldo dan riwayat membaca field yang tidak ada, sehingga selalu menampilkan bawaan 12 hari.
- Jenis cuti ditulis di kode; payload formulir pengajuan tidak cocok dengan server. `POST` korporat langsung buat+ajukan, sehingga gagal bila lampiran wajib (lampiran baru bisa diunggah setelah permohonan ada) dan meninggalkan draf yatim.
- Banner kecukupan staf memakai angka palsu; payload setuju/tolak/penyesuaian salah nama field.
- Daftar staf recall dan tiket recall berupa data contoh.
- Rekonsiliasi mengirim `year/month/cutOffDate` (server: `periodYear/periodMonth/notes`), tabel retrospektif berupa data contoh, dan blok `catch` mengembalikan data contoh.
- Layar penyesuaian (`/hr/leave/adjustments`) membaca `totalCount/totalPages` (server: `totalData/totalPage`), mengirim `workforceProfileId` kosong, `adjustedDays`, dan `documentReferenceNumber` sehingga selalu gagal; rute ini juga tidak ada di menu.

## 2. Proses bisnis dari sisi pengguna

1. **Saldo Saya**: pegawai melihat hak, terpakai, diproses, bawaan, dan peringatan kedaluwarsa; riwayat buku besar dibuka di drawer; permohonan dapat dibatalkan bila server mengizinkan (`availableActions` berisi `Cancel`).
2. **Ajukan Cuti** (alur bertahap): pilih jenis dari saldo yang tersedia → hitung hari (termasuk setengah hari pagi/siang) → simpan draf → unggah surat sakit/dokumen pendukung bila wajib → ajukan.
3. **Persetujuan atasan**: setujui (mitigasi wajib bila ada peringatan kecukupan staf), minta revisi, atau tolak dengan alasan.
4. **Manajemen saldo staf**: HR melihat saldo berhalaman, menyesuaikan saldo dengan nomor dokumen audit, dan menjalankan proses carry-forward akhir tahun/tengah tahun.
5. **Penyesuaian saldo berdokumen**: HR membuat draf (pilih saldo pegawai, jenis, arah, alasan dari master, hari, tanggal efektif, dasar SK), lalu mengajukan atau membatalkan.
6. **Recall darurat**: kepala unit memilih staf yang sedang cuti hari ini dari kalender, mengusulkan recall; HR Manager menyetujui atau menolak.
7. **Rekonsiliasi**: HR memilih bulan/tahun, melihat cuti yang belum direkonsiliasi (penanda "Menunggu rekonsiliasi"), lalu menutup dan mengunci periode; hasil penutupan ditampilkan.
8. Jalur tidak normal: tanpa hak tampil peringatan; galat server tampil; formulir menolak tanggal di luar rentang cuti atau hari ≤ 0.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh controller Cuti di bagian 5, DTO `LeaveAdjustmentDtos.cs`, `LeaveEntitlementBalanceQueryDtos.cs`, Lampiran B baris S-B2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/leave/leave-constants.js` | BARU: rute, peta status permohonan/recall/penyesuaian, opsi kategori darurat, jenis/arah penyesuaian, resource saldo |
| `LeaveBalanceMetricCards`, `my-leave-balance-view`, `LeaveDateRangePicker`, `leave-request-view`, `SafeStaffingCoverageWarningBanner` (WRAP `InformationAlert`), `leave-approval-inbox-view`, `LeaveBalanceAdjustmentModal`, `leave-balances-admin-view`, `LeaveRecallInitiationModal`, `leave-recalls-view`, `leave-reconciliations-view`, `leave-admin-view` | Ditulis ulang dengan base component dan kontrak benar |
| `components/features/hr/leave/LeaveAdjustmentDraftModal.jsx` | BARU: draf penyesuaian berdokumen |
| `employee-self-service.js` | Opsi saldo, hitung, ajukan, unggah lampiran (self-service) |
| `leave-service.js` | `getLeaveCalendar`; komentar payload dikoreksi |
| `leave-admin-service.js` | Ringkasan, opsi alasan, ajukan, batalkan penyesuaian |
| `leave-display-utils.js` | Normalizer saldo/permohonan/buku besar; lima konstanta/fungsi yatim dihapus |
| `menu-items.jsx`, `app/hr/leave/adjustments/page.jsx` | Menu "Penyesuaian Saldo Cuti" (`LeaveAdjustment : Read`), metadata halaman |
| `MedicalCertificateUploadModal.jsx` | Dihapus (tidak dipakai lagi) |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; reset modal memakai `key`. `LeaveLedgerDrawer.jsx` dan `leave.module.css` dipertahankan (0 temuan, di luar daftar Lampiran B).

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `DataTable` | `REUSE` | — |
| Kartu saldo dengan bilah progres | `SummaryGrid` + `exit-clearance.module.css` | `REUSE` | — |
| Banner kecukupan staf | `InformationAlert` | `WRAP` | Data dari `hasStaffingCoverageWarning` |
| Dialog | `ConfirmModal` + `BaseFormControl` + `HrResourceField` | `COMPOSE` | Saldo pegawai dipilih dari daftar |

`UI GATE: PASS — 6 elemen REUSE, 1 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <14 berkas JSX Cuti>
OK — 1 berkas diperiksa, 0 temuan. (×14)
$ grep Tailwind <berkas R37>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/kartu |
| Kosong | "Tidak ada cuti yang menunggu rekonsiliasi pada periode ini." / "Belum ada penyesuaian saldo cuti." |
| Gagal | Pesan server di `InformationAlert`, tanpa data contoh |
| Tanpa hak akses | "Anda belum punya hak untuk melihat …" |

## 5. Endpoint yang dikonsumsi

#### HR Leave - Balances & Ledgers

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/balances/my` | Saldo saya | `LeaveBalanceLedger : Read` |
| `GET` | `/v1/corporate/human-resource/leave/balances/ledger` | Buku besar | `LeaveBalanceLedger : Read` |
| `POST` | `/v1/corporate/human-resource/leave/balances/adjustment` | Penyesuaian langsung | `LeaveBalanceLedger : Update` |
| `POST` | `.../balances/carry-forward/run-year-end`, `run-mid-year-expiry` | Proses carry-forward | `LeaveBalanceLedger : Update` |

#### Corporate / Human Resource / Leave Management / Leave Balance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/balances` | Daftar saldo staf dan pilihan saldo | `LeaveBalance : Read` |

#### Self Services / Human Resource / Leave Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/leave/requests/balances/options` | Jenis cuti tersedia | `MyLeaveRequest : Read` |
| `POST` | `.../leave/requests/calculate` | Hitung hari | `MyLeaveRequest : Read` |
| `POST` | `.../leave/requests` | Simpan draf | `MyLeaveRequest : Create` |
| `POST` | `.../leave/requests/{id}/attachments` | Unggah surat/dokumen | `MyLeaveRequest : Update` |
| `POST` | `.../leave/requests/{id}/submit` | Ajukan | `MyLeaveRequest : Submit` |

#### HR Leave - Approvals & Coverage

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/approvals/inbox` | Kotak masuk | `LeaveApproval : View` |
| `POST` | `.../approvals/{id}/approve` | Setujui (`approvalNotes`, `mitigationNotes`) | `LeaveApproval : Approve` |
| `POST` | `.../approvals/{id}/revise` | Minta revisi | `LeaveApproval : Approve` |
| `POST` | `.../approvals/{id}/reject` | Tolak (`rejectionReason`) | `LeaveApproval : Approve` |

#### Corporate / Human Resource / Leave Management / Leave Calendar

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/calendar` | Staf sedang cuti (recall) | `LeaveCalendar : Read` |

#### HR Leave - Recall Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`/`POST` | `/v1/corporate/human-resource/leave/recalls` | Daftar / usulkan recall | `LeaveRecall : View` / `Create` |
| `POST` | `.../recalls/{id}/approve`, `.../reject` | Putusan HR Manager | `LeaveRecall : Approve` |

#### HR Leave - Reconciliations & Close

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/reconciliations/pending` | Cuti belum direkonsiliasi | `LeaveReconciliation : View` |
| `POST` | `/v1/corporate/human-resource/leave/reconciliations/close-period` | Tutup & kunci periode | `LeaveReconciliation : Close` |

#### Corporate / Human Resource / Leave Management / Leave Adjustment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/leave/adjustments` | Register penyesuaian | `LeaveAdjustment : Read` |
| `GET` | `.../adjustments/summary`, `.../reasons/options` | Ringkasan, pilihan alasan | `LeaveAdjustment : Read` |
| `POST` | `.../adjustments` | Draf | `LeaveAdjustment : Create` |
| `POST` | `.../adjustments/{id}/submit` | Ajukan | `LeaveAdjustment : Submit` |
| `POST` | `.../adjustments/{id}/cancel` | Batalkan | `LeaveAdjustment : Cancel` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 14 JSX + service/konstanta/util/menu Cuti | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 14 JSX | 0 temuan (1 positif palsu `"Debit"` di `className` diselesaikan) | `PASS` | 3.4 |
| `npm run build` BUILD-2 | 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| Fungsi cuti tidak berubah | Terpenuhi, dengan perbaikan kontrak | Alur sama; payload kini cocok dengan server |
| Metadata faskes/SIP surat sakit | Belum terpenuhi | Endpoint surat sakit korporat meminta `FilePath` server, respons lampiran tidak mengembalikan path |
| Verifikasi surat dokter dari kotak masuk | Belum terpenuhi | Endpoint butuh `attachmentId` yang tidak ada di DTO kotak masuk |
| Daftar penyesuaian retrospektif (HRD-DEC-114) | Belum terpenuhi | Belum ada endpoint |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bila tanggal diubah setelah draf terbentuk, draf tidak di-PUT ulang; pengguna memulai ulang |
| Masalah yang diketahui | Posting, reversal, dan lampiran alur penyesuaian berdokumen di luar layar ini |
| Dependency backend | Path lampiran untuk surat sakit, `attachmentId` di kotak masuk, endpoint penyesuaian retrospektif |
| Perubahan sampingan | `MedicalCertificateUploadModal.jsx` dan lima ekspor yatim di util dihapus; menu penyesuaian ditambah |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Backend: tiga gap di atas; uji browser |
