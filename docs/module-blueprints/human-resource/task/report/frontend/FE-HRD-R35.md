# Laporan Perubahan Frontend — `FE-HRD-R35`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R35` |
| Judul | Lembur bagian 2: verifikasi, Direktur Medik, CTO, rekonsiliasi, pengaturan |
| Slice | `S-B3` (Lembur), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R35`; task asal `FE-HRD-121`–`128` |
| Trace | Minimal 30 menit diakui, risiko lelah 60 jam/minggu, istirahat wajib 11 jam, CTO mendesak 14 hari |
| Contract version | Source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component, penyambungan API nyata, perbaikan body permintaan |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 5 layar, 2 komponen, 1 modal baru, perbaikan service |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — verifikasi, inbox Direktur Medik, CTO, dan rekonsiliasi nyata; pengaturan baca-saja karena backend hanya menyimpan di memori |

---

## 1. Keadaan yang ditemukan di awal

- Fungsi service `closePeriod`, `reopenPeriod`, dan `executePayrollHandoff` dipanggil tanpa body, padahal controller mewajibkan `[FromBody]` — penutupan periode selalu gagal.
- Drawer anomali lama memakai elemen mentah; layar verifikasi, CTO, dan rekonsiliasi memakai `overtime.module.css` dan util data contoh.

## 2. Proses bisnis dari sisi pengguna

1. **Pusat verifikasi**: petugas melihat realisasi lembur yang menunggu verifikasi biometrik (filter unit/tanggal), memverifikasi per baris; anomali dibuka di **modal anomali** untuk diselesaikan (akui penuh, sesuaikan, tolak) dengan nomor referensi logbook.
2. **Inbox Direktur Medik**: lembur berisiko (melampaui 60 jam/minggu atau melanggar istirahat 11 jam) disetujui atau ditolak Direktur Medik.
3. **CTO**: kartu saldo cuti pengganti per pegawai menampilkan tiket yang akan kedaluwarsa (mendesak ≤14 hari); admin dapat menjalankan proses kedaluwarsa.
4. **Rekonsiliasi**: admin melihat periode berjalan, menutup periode, membuka kembali dengan alasan minimal, melihat pratinjau paket payroll, lalu mengeksekusi serah terima ke payroll.
5. **Pengaturan**: parameter lembur ditampilkan baca-saja.
6. Jalur tidak normal: tanpa hak, menu dan tombol tidak tampil; galat server tampil; verifikasi massal diproses per baris dengan hasil per baris.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Enam controller lembur modern, dua controller lama (realisasi, cuti pengganti), `OvertimeSettingService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/overtime/overtime-constants.js` | Rute, permission, status verifikasi/CTO/periode, opsi penyelesaian anomali, ambang |
| `src/lib/services/hr/overtime-service.js` | `getRealizationsForVerification`, `getCompensatoryLedger`, `signal`; PERBAIKAN body close/reopen/execute |
| `features/hr/overtime/OvertimeDiscrepancyModal.jsx` | BARU (pengganti drawer; DEC-FER-01 BaseDrawer belum diputuskan) |
| `CompensatoryTimeOffCard.jsx`, `OvertimeReconciliationTable.jsx` | Ditulis ulang (WRAP `HrPanel`/`DataTable`) |
| `overtime-verifications`, `overtime-director-inbox`, `overtime-compensatory-leave`, `overtime-reconciliations`, `overtime-settings` | Ditulis ulang |
| `OvertimeDiscrepancyDrawer.jsx`, `overtime.module.css`, `overtime-display-utils.js` | Dihapus |
| `menu-items.jsx` | `requiredPermission` inbox Direktur Medik, rekonsiliasi, pengaturan |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; tanpa pola baru.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `DataTable` | `REUSE` | — |
| Kartu CTO | `HrPanel` | `WRAP` | — |
| Tabel rekonsiliasi | `DataTable` | `WRAP` | — |
| Penyelesaian anomali | `ConfirmModal` + `BaseFormControl` | `COMPOSE` | Pengganti drawer |

`UI GATE: PASS — 4 elemen REUSE, 1 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <8 berkas JSX R35>
OK — 8 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas R35>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/kartu |
| Kosong | "Tidak ada realisasi menunggu verifikasi." / "Tidak ada antrean Direktur Medik." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | Peringatan hak; tombol aksi disembunyikan |

## 5. Endpoint yang dikonsumsi

#### HR Overtime - Biometric Verification & Discrepancies

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/overtime/verifications/{realizationId}/verify-biometric` | Verifikasi | `OvertimeVerificationModern : Verify` |
| `GET` | `/v1/corporate/human-resource/overtime/verifications/discrepancies` | Antrean anomali | `OvertimeVerificationModern : View` |
| `POST` | `/v1/corporate/human-resource/overtime/verifications/discrepancies/{verificationId}/resolve` | Selesaikan anomali | `OvertimeVerificationModern : Verify` |

#### Corporate / Human Resource / Overtime Management / Overtime Realization

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime-management/realizations` | Menunggu verifikasi (filter unit/tanggal) | `OvertimeRealization : Read` |

#### HR Overtime - Approvals & Escalations

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime/approvals/medical-director-inbox` | Antrean | `OvertimeApproval : MedicalDirectorApprove` |
| `POST` | `/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-approve` | Setujui | `OvertimeApproval : MedicalDirectorApprove` |
| `POST` | `/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-reject` | Tolak | `OvertimeApproval : MedicalDirectorApprove` |

#### HR Overtime - Compensatory Time Off (CTO)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime/compensatory-time-off/staff-tickets/{workforceProfileId}` | Tiket CTO pegawai | `CompensatoryTimeOffModern : View` |
| `POST` | `/v1/corporate/human-resource/overtime/compensatory-time-off/run-expiry` | Proses kedaluwarsa | `CompensatoryTimeOffModern : Execute` |

#### Corporate / Human Resource / Overtime Management / Compensatory Leave

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime-management/compensatory-leaves` | Buku besar cuti pengganti | `OvertimeCompensatoryLeave : Read` |

#### HR Overtime - Period & Retrospective Reconciliation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime/periods` / `periods/current` | Daftar dan periode berjalan | `OvertimePeriodModern : View` |
| `POST` | `/v1/corporate/human-resource/overtime/periods/{id}/close` | Tutup periode | `OvertimePeriodModern : Close` |
| `POST` | `/v1/corporate/human-resource/overtime/periods/{id}/reopen` | Buka kembali | `OvertimePeriodModern : Reopen` |

#### HR Overtime - Payroll Handoff & Export

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime/payroll-handoff/preview/{periodId}` | Pratinjau paket | `OvertimePayrollHandoffModern : View` |
| `POST` | `/v1/corporate/human-resource/overtime/payroll-handoff/execute/{periodId}` | Serah terima | `OvertimePayrollHandoffModern : Execute` |

#### HR Overtime - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/overtime-settings` | Parameter (baca-saja) | `OvertimeSetting : View` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 10 berkas R35 | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 8 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-1 / BUILD-2 | 821/821 dan 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| `FE-HRD-121`–`126` verifikasi, anomali, Direktur Medik, CTO, rekonsiliasi | Terpenuhi sebagian | Tab anomali tanpa filter (server hanya paginasi); verifikasi massal per baris; logbook berupa nomor referensi |
| `FE-HRD-127` penutupan periode | Terpenuhi sebagian | Paket handoff tanpa estimasi beban finansial |
| `FE-HRD-128.1` parameter dapat disesuaikan, `128.3` log audit | Belum terpenuhi | `OvertimeSettingService` menyimpan di memori statis — hilang saat restart, tanpa audit |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Parameter `urgency` pada inbox Direktur Medik tidak dipakai server, sehingga filter urgensi tidak ditawarkan |
| Masalah yang diketahui | Formulir cuti ESS belum menerima prefill jenis cuti via URL (tombol CTO hanya membuka formulir) |
| Dependency backend | Penyimpanan pengaturan ke DB + audit, filter antrean anomali, verifikasi batch, unggah logbook, estimasi beban finansial handoff |
| Perubahan sampingan | PERBAIKAN bug body permintaan tutup/buka/eksekusi; drawer, CSS, dan util lama dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Backend menyimpan pengaturan lembur di DB; uji browser |
