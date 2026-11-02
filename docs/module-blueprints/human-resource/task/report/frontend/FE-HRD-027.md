# Laporan Perubahan Frontend — `FE-HRD-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-027` |
| Judul | Formulir Permohonan Perubahan Data Kuasa HR (On-Behalf Profile Change) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-052`, `HRD-DEC-031`, `BE-HRD-028` |
| Contract version | Employee Profile Change API v1 (`EmployeeProfileChangeController.cs`) |
| Dependency | `BE-HRD-028` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/employee-profile-changes/create-on-behalf/page.jsx`, `src/app/hr/workforce-core/employee-profile-changes/on-behalf/page.jsx`, `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx`, `src/lib/services/hr/workforce-core-cross-employee-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Banyak staf rumah sakit (seperti tenaga penunjang, petugas kebersihan, atau staf yang sedang sakit/rawat jalan) tidak memiliki akses langsung ke aplikasi portal mandiri (*Employee Self-Service*) saat perlu memperbarui nomor telepon darurat, kontak keluarga, atau alamat domisili.

Task ini menyediakan **Formulir Pengajuan Kuasa HR (*On-Behalf*)**:
1. Staf HR dapat mengajukan permohonan pembaruan data kontak/alamat/identitas atas nama pegawai yang diwakilkan.
2. Form mewajibkan pengisian alasan dasar kuasa dan pencatatan nama berkas bukti pendukung fisik (surat kuasa/formulir bertandatangan).
3. **Audit & Anti-Self Verification (`HRD-DEC-031` / `HRD-DEC-052`)**: Sistem mencatat identitas staf HR pembuat kuasa. Demi tata kelola independen (*Segregation of Duties*), staf HR yang mengajukan dilarang memverifikasi permohonannya sendiri pada antarmuka verifikasi.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-027: Pengajuan Kuasa HR (On-Behalf) terintegrasi ke service dan form (0.409ms)
```
Status: **PASS (100%)**
