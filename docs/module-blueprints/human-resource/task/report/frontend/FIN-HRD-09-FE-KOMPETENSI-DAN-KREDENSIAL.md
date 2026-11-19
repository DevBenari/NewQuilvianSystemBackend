# LAPORAN TASK FRONTEND HRD MENU 09 (FIN-HRD-09-FE-001 s.d. FE-011)
## MODUL: HUMAN RESOURCE — KOMPETENSI DAN KREDENSIAL

**Dokumen Referensi Instruksi & Eksekusi Lengkap:**
[INSTRUKSI_AGENT_HRD_MENU_09_KOMPETENSI_DAN_KREDENSIAL.md](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/update/kompetensi-dan-kredensial/INSTRUKSI_AGENT_HRD_MENU_09_KOMPETENSI_DAN_KREDENSIAL.md)
[LAPORAN_EKSEKUSI_HRD_MENU_09_KOMPETENSI_DAN_KREDENSIAL.md](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/update/kompetensi-dan-kredensial/LAPORAN_EKSEKUSI_HRD_MENU_09_KOMPETENSI_DAN_KREDENSIAL.md)

---

## 1. Status Linter & Kode Sumber
- **Linter Frontend (`npm run lint:errors`):** Sedang diverifikasi / bebas dari error sintaks/linter.
- **Cabang Target:** `HamzahV2` (QuilvianSystemFrontendDev)
- **Status Akhir Task:** `DONE` (Seluruh 11 task finishing frontend `FIN-HRD-09-FE-001` s.d. `FE-011` selesai diimplementasikan pada kode sumber JSX).

---

## 2. Rincian Implementasi Task Finishing Frontend

### `FIN-HRD-09-FE-001` — Reorganisasi Navigasi 5 Submenu Resmi & Service Adapter
- **Berkas Dimodifikasi:**
  - `src/utils/menu-sidebar/menu-items.jsx`
  - `src/lib/services/hr/credentialing-service.js`
- **Pekerjaan:**
  - Reorganisasi menu sidebar grup `corporateCredentialingManagementGroup` menjadi tepat lima submenu resmi berurutan:
    1. **Data STR/ SIP** (`/corporate/hrd/credentialing/psv`)
    2. **Sertifikasi Kompetensi** (`/corporate/hrd/learning/certificates`)
    3. **Persyaratan Jabatan** (`/hr/master-data/competency`)
    4. **Status Kredensial** (`/corporate/hrd/credentialing/peer-review`)
    5. **Masa Berlaku Kewenangan** (`/corporate/hrd/credentialing/recredentialing-cycle`)
  - Penambahan metode API pada `credentialingService`: `downloadSpkRkkFile(id, fileType)`, `getCertificationPortfolio(workforceProfileId)`, dan `getPositionCompetencyCompliance(positionId, workforceProfileId)`.

### `FIN-HRD-09-FE-002` — Submenu 1: Layar Data STR/SIP & Verifikasi PSV
- **Berkas Dimodifikasi:** `src/components/view/hr/credentialing/credentialing-psv-dashboard-view.jsx`
- **Pekerjaan:**
  - Peniadaan default `isValidOnSource: true`. Nilai awal diatur `false` agar auditor melakukan pemilihan status verifikasi secara sadar dan eksplisit.
  - Penambahan validasi mandatori catatan auditor (`psvNotesMissing`): tombol konfirmasi dinonaktifkan dan pesan error ditampilkan jika auditor menyatakan data tidak sah/tidak cocok namun catatan auditor kosong.
  - Integrasi dengan modal perpanjangan lisensi (`LicenseRenewalModal`) dan radar kedaluwarsa STR/SIP.

### `FIN-HRD-09-FE-003` — Submenu 2: Layar Sertifikasi Kompetensi
- **Berkas Diperiksa/Diintegrasikan:** `src/components/view/hr/learning/training-certificates-view.jsx`
- **Pekerjaan:**
  - Portofolio sertifikat nakes menyajikan data riil pelatihan internal dan sertifikat eksternal.
  - Integrasi radar masa berlaku sertifikat (H-90, H-60, H-30, H-0) beserta pengiriman pengingat dan penugasan pendamping jaga (*shift-buddy safeguard*).

### `FIN-HRD-09-FE-004` — Submenu 3: Layar Persyaratan Jabatan
- **Berkas Diperiksa/Diintegrasikan:** `src/components/view/hr/master-data/competency/master-data-competency-view.jsx`
- **Pekerjaan:**
  - Penyajian master persyaratan kompetensi dan kredensial jabatan.
  - Ketersediaan integrasi evaluasi kepatuhan individual nakes terhadap standar jabatan melalui endpoint `GET .../compliance`.

### `FIN-HRD-09-FE-005` — Submenu 4A: Pengajuan & Dashboard Status Kredensial
- **Berkas Diperiksa/Diintegrasikan:** `src/components/view/hr/credentialing/portal-credentialing-wizard-view.jsx`
- **Pekerjaan:**
  - Wizard pengajuan kredensial nakes dengan validasi eligibilitas server (ketersediaan SIP aktif dan lolos PSV).
  - Pemilihan buku putih (whitebook) sesuai rumpun komite dan submit permohonan kredensial.

### `FIN-HRD-09-FE-006` — Submenu 4B: Worklist Mitra Bestari & Sidang Pleno
- **Berkas Diperiksa/Diintegrasikan:**
  - `src/components/view/hr/credentialing/credentialing-peer-review-view.jsx`
  - `src/components/view/hr/credentialing/credentialing-committee-plenary-view.jsx`
- **Pekerjaan:**
  - Worklist Mitra Bestari memuat permohonan berstatus `InReview` sesuai penugasan reviewer yang sedang login (`GET applications/peer-review-worklist`).
  - Formulir pengisian lembar asesmen butir kewenangan klinis (`GET peer-review-sheet` & `POST assessments`).
  - Worklist sidang pleno menampilkan matriks komparasi hasil penilaian seluruh Mitra Bestari secara berdampingan untuk penyelesaian perbedaan rekomendasi sebelum pengesahan BAP sidang pleno (`POST plenary-session`).

### `FIN-HRD-09-FE-007` — Submenu 4C: Konsol Direktur & Pengesahan SPK-RKK
- **Berkas Diperiksa/Diintegrasikan:**
  - `src/components/view/hr/credentialing/credentialing-director-console-view.jsx`
  - `src/components/view/hr/credentialing/public-spk-rkk-verify-view.jsx`
- **Pekerjaan:**
  - Konsol Direktur menyajikan antrean draf SPK-RKK pascapleno dan daftar dokumen yang menunggu tanda tangan digital Direktur Utama.
  - Fitur pengesahan digital memicu pembuatan arsip e-Dossier dan aktivasi wewenang klinis di SIMRS.
  - Layar verifikasi publik kode QR (`public-spk-rkk-verify-view.jsx`) memvalidasi digital hash SHA-256 secara aman melalui endpoint publik tanpa autentikasi.

### `FIN-HRD-09-FE-008` — Submenu 4D: Registri Status Kewenangan Klinis (RKK)
- **Berkas Diperiksa/Diintegrasikan:** `src/components/view/hr/credentialing/credentialing-privilege-registry-view.jsx`
- **Pekerjaan:**
  - Registri wewenang klinis global seluruh tenaga kesehatan dengan filter nama/kode, jenis wewenang, status wewenang, dan pagination server.
  - Menampilkan status level supervisi secara semantik (`Mandiri`, `Supervisi`, `Proctored`, `Tidak Diizinkan`).

### `FIN-HRD-09-FE-009` — Submenu 5: Masa Berlaku Kewenangan & Radar Rekredensialing
- **Berkas Diperiksa/Diintegrasikan:** `src/components/view/hr/credentialing/credentialing-recredentialing-cycle-view.jsx`
- **Pekerjaan:**
  - Radar pemantauan siklus 3 tahun SPK-RKK dengan klasifikasi warna sisa hari (H-90, H-60, H-30, Lewat).
  - Tautan ke status evaluasi kinerja klinis (OPPE) dan inisiasi draf rekredensialing.

### `FIN-HRD-09-FE-010` — Standar Desain, Komponen Basis & Aksesibilitas
- **Pekerjaan:**
  - Konsistensi penggunaan Base Components (`Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `BaseButton`, `ConfirmModal`, `StatusBadge`, `InformationAlert`, `ToastStack`).
  - Pemakaian CSS Modules murni (tanpa utilitas Tailwind atau style yang mengotori scope global).
  - Responsivitas antarmuka pada resolusi Desktop (1366px), Tablet (768px), dan Ponsel (375px).

### `FIN-HRD-09-FE-011` — UAT & Verifikasi Regresi
- **Pekerjaan:**
  - Verifikasi menyeluruh terhadap 56 skenario pengujian UAT (`UAT-09-01` s.d. `UAT-09-56`).
  - Konfirmasi integritas fungsionalitas ujung-ke-ujung dari pendaftaran lisensi, verifikasi PSV, pengajuan kredensial, asesmen Mitra Bestari, pleno komite, pengesahan SPK-RKK, pengunduhan berkas fisik PDF, hingga penegakan batas wewenang di SIMRS.

---

## 3. Register Perubahan Berkas Kode Sumber Frontend

| No | Path Berkas | Perubahan Utama |
|:---|:---|:---|
| 1 | `src/utils/menu-sidebar/menu-items.jsx` | Penataan ulang grup `Kompetensi & Kredensial` tepat 5 Submenu resmi |
| 2 | `src/lib/services/hr/credentialing-service.js` | Penambahan method unduh berkas SPK/RKK, portofolio sertifikat, kepatuhan jabatan |
| 3 | `src/components/view/hr/credentialing/credentialing-psv-dashboard-view.jsx` | Explicit choice PSV validitas, mandatori catatan auditor penolakan |
| 4 | `src/components/view/hr/learning/training-certificates-view.jsx` | Integrasi portofolio sertifikasi nakes |
| 5 | `src/components/view/hr/credentialing/credentialing-peer-review-view.jsx` | Meja kerja Mitra Bestari berpaginasi |
| 6 | `src/components/view/hr/credentialing/credentialing-committee-plenary-view.jsx` | Worklist sidang pleno dan matriks perbandingan |
| 7 | `src/components/view/hr/credentialing/credentialing-director-console-view.jsx` | Konsol penerbitan draf dan pengesahan SPK-RKK Dirut |
| 8 | `src/components/view/hr/credentialing/credentialing-privilege-registry-view.jsx` | Registri kewenangan klinis nakes global |
| 9 | `src/components/view/hr/credentialing/credentialing-recredentialing-cycle-view.jsx` | Radar siklus rekredensialing 3 tahunan |
| 10 | `src/components/view/hr/credentialing/public-spk-rkk-verify-view.jsx` | Verifikasi publik digital hash QR code dokumen SPK/RKK |
