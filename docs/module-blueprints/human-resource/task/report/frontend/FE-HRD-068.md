# Laporan Perubahan Frontend — `FE-HRD-068`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-068` |
| Judul | Layar Pengajuan Permintaan Akun & Akses Sistem Pegawai Baru (FE-HRD-136) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-15` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-089, VR-ONB-70 s.d. VR-ONB-74, BE-HRD-077` |
| Contract version | API Contract v9 bagian 15.6 (onboarding-access-requests) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-077 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/onboarding-access-requests-view.jsx, src/app/hr/lifecycle/onboarding-access-requests/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penyelarasan fasilitas digital pegawai baru: pengajuan akun SIMRS, email resmi RS, hak akses Electronic Health Record (EHR), dan kartu akses fisik ke departemen IT/Sistem Informasi.

Task ini mengimplementasikan:
1. **Layar Manajemen Permintaan Akses (/hr/lifecycle/onboarding-access-requests)**:
   - Menampilkan antrean permintaan hak akses per pegawai baru yang terhubung ke kasus onboarding.
   - Pilihan checklist modul sistem yang diajukan (SIMRS Rawat Inap, Rawat Jalan, Farmasi, Laboratorium, Radiologi, Keuangan, dsb.).
   - Pelacakan status provisioning IT (REQUESTED, IN_PROGRESS, PROVISIONED, REJECTED).
   - Form pemenuhan akses: Petugas IT mencatat username yang dibuatkan, nomor kartu akses fisik, dan tanggal aktif akun.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Daftar permintaan akses menampilkan status provisioning IT secara jelas | ✅ PASS | OnboardingAccessRequestsView menyajikan tabel filter status dan detail akun. |
| 2 | Aksi pemenuhan akses memperbarui status provisioning ke backend | ✅ PASS | Terhubung ke LifecycleService.fulfillAccessRequest. |
| 3 | Validasi form pengajuan akses melarang duplikasi permintaan | ✅ PASS | Pengecekan ID onboarding case mencegah double submission. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/onboarding-access-requests-view.jsx (Tabel permintaan akses & modal pemenuhan IT)`
- `src/app/hr/lifecycle/onboarding-access-requests/page.jsx (Rute App Router permintaan akses)`
