# Laporan Perubahan Frontend — `FE-RWI-228-IGD-REQ-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-228-IGD-REQ-003` (Penyelesaian Butir 2 penahan `BE-IGD-077` dari sisi Frontend) |
| Judul | Penambahan Tombol dan Modal Penolakan Rujukan Transfer di Layar Antrean Transfer Rawat Inap |
| Slice | Antrean Transfer Admisi Rawat Inap (`inpatient-admission-transfer-list`) |
| Requirement | `FR-IGD-119`, `FR-IGD-120`; `AT-IGD-238`…`240`; `IGD-REQ-003` Butir 2 |
| Keputusan | `IGD-DEC-309`, `RWI-DEC-338`…`341` |
| Rujukan Surat | [`2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md`](../../../igd/approval-requests/2026-10-26-tindak-lanjut-igd-req-003-rawat-inap.md) |
| Target tulis | `QuilvianSystemFrontendDev` (branch `HamzahV2`): 4 berkas |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **SELESAI / DONE — Linting Lulus (0 Error), Unit Test Lulus (5/5 Pass).** |

---

## 1. Masalah yang Diselesaikan

Pada layar Antrean Pasien Transfer IGD & Rawat Jalan (`inpatient-admission-transfer-list.jsx`), kolom aksi sebelumnya hanya menyediakan tombol *"Proses Admisi"*. Petugas admisi rawat inap tidak memiliki antarmuka untuk menolak permintaan rawat inap yang diajukan oleh IGD (misal bila bangsal penuh atau indikasi tidak sesuai).

---

## 2. Rincian Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js` | Menambahkan method `rejectAdmissionTransfer(id, payload, config)` yang mengirimkan `POST /admission-transfers/{id}/reject`. |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx` | 1. Menambahkan tombol aksi *Tolak* (variant `danger`, icon `XCircle`) khusus untuk pasien asal IGD (`sourceType === "Emergency"`).<br>2. Menyediakan komponen modal dialog penolakan lengkap dengan kartu ringkasan pasien, alert informasi, textarea alasan (wajib 3–500 karakter), indikator karakter real-time, penanganan error (termasuk conflict 409 bila pasien sudah di-admit di tab lain), serta banner notifikasi sukses pasca-tolak. |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.module.css` | Menambahkan styling modal, backdrop blur, kartu ringkasan rujukan, textarea border error/focus, dan animasi notifikasi toast menggunakan 100% token desain Quilvian. |
| `tests/unit/inpatient-admission-transfer.test.mjs` | Menambahkan assertion uji unit untuk memastikan `rejectAdmissionTransfer` tersedia sebagai function pada service. |

---

## 3. Bukti Verifikasi

### 3.1 Unit Testing
```text
▶ FE-RWI-227 s.d. FE-RWI-230: Admisi Transfer Pasien IGD & Poliklinik
  ✔ FE-RWI-227: INPATIENT_ADMISSION_ENTRY_MODE memiliki mode TRANSFER (1.0225ms)
  ✔ FE-RWI-227: Opsi kartu ke-4 pada landing admisi ranap terdaftar dengan benar (0.2357ms)
  ✔ FE-RWI-227 & FE-RWI-229: Stepper alur transfer memuat tahapan rujukan dan verifikasi pasien (0.2029ms)
  ✔ FE-RWI-227: resolveInpatientAdmissionSteps menyembunyikan Deposit bila non-tunai pada alur transfer (0.2799ms)
  ✔ FE-RWI-228 & FE-RWI-230: Service frontend transfer menyediakan fungsi query antrean dan eksekusi admisi (0.2178ms)
✔ FE-RWI-227 s.d. FE-RWI-230: Admisi Transfer Pasien IGD & Poliklinik (3.9128ms)
ℹ tests 5
ℹ suites 1
ℹ pass 5
ℹ fail 0
```

### 3.2 Linting
```text
✖ 1 problem (0 errors, 1 warning)
```
0 error linting pada seluruh berkas yang disentuh.
