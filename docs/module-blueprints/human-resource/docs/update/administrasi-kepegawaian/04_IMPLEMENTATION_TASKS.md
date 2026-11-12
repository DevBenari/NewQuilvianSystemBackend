# Rencana Tugas Implementasi (Implementation Tasks) Administrasi Kepegawaian SIMRS Quilvian V2

**Dokumen**: `04_IMPLEMENTATION_TASKS.md`  
**Modul**: Corporate / Human Resource Management (HRD)  
**Menu Induk**: Administrasi Kepegawaian  
**Tanggal**: 11 November 2026  
**Status**: Pelacakan Tugas Aktif (Audit & Rencana Implementasi)  

---

## 1. Ringkasan Status Progres per Cakupan Bisnis

| Kode Task | Cakupan Bisnis | Status Realistis | Persentase Selesai | Catatan Kunci Kesiapan |
|---|---|---|---|---|
| **AUD-01** | Audit Menyeluruh Administrasi Kepegawaian | `SELESAI (DONE)` | 100% | Pemetaan kode, rute, model, DTO, dan temuan risiko telah tuntas didokumentasikan. |
| **SK-IMP** | Surat Keputusan (SK Kepegawaian) | `SIAP UAT` | 95% | Fitur existing `Dokumen SK Kepegawaian` telah lengkap; menunggu pengujian live server. |
| **ST-IMP** | Surat Tugas Resmi (Official Duty) | `DALAM PENGERJAAN` | 50% | Memerlukan perluasan dukungan multi-pegawai dan antrean verifikasi HRD terpadu. |
| **SKT-IMP** | Surat Keterangan Resmi & Anti-Manual Guard | `SIAP UAT (TERBATAS)`| 85% | Backend persuratan lengkap; memerlukan penyambungan API aktif pada antarmuka helpdesk. |
| **DA-IMP** | Registri Dokumen Administrasi Terpadu | `DALAM PENGERJAAN` | 60% | Fondasi E-Dossier & Dokumen Saya ada; perlu layar registri induk terpadu di menu HRD. |
| **PD-IMP** | Pengajuan Perubahan Data Karyawan | `SIAP UAT` | 95% | Fitur existing `Perubahan Data Karyawan` lengkap dengan anti-self-approval dan komparasi field. |
| **REG-IMP**| Integrasi Lintas Cakupan & Audit Kepatuhan | `SIAP OPERASIONAL (STATIS)`| 80% | Seluruh aturan RBAC dan audit log siap; pengujian end-to-end terblokir ketersediaan runtime live. |

---

## 2. Rincian Task dan Kontrak Spesifikasi API per Cakupan

---

### Task 1: AUD-01 Audit Administrasi Kepegawaian
- **Tujuan**: Memetakan seluruh kode program, basis data, dan rute navigasi untuk mencegah duplikasi menu atau entitas.
- **Hasil**: Berkas `01_AUDIT_EXISTING_ADMINISTRASI_KEPEGAWAIAN.md` dan `02_ALUR_BISNIS_DAN_KEPUTUSAN.md`.
- **Status**: `DONE (Selesai)`.

---

### Task 2: SK-IMP Implementasi & Validasi Surat Keputusan (SK Kepegawaian)
- **Tujuan**: Memastikan alur formal keputusan kepegawaian (mutasi, promosi, rotasi, demosi) berjalan aman, membandingkan data penempatan sebelum/sesudah, dan menerapkan mutasi secara idempoten saat tanggal efektif tiba.
- **Berkas Terkait**:
  - Backend: `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdPersonnelActionOrderController.cs`, `HrdPersonnelActionOrderService.cs`.
  - Frontend: `src/app/hr/workforce-core/personnel-actions/page.jsx`, `personnel-action-order-list-view.jsx`, `personnel-action-order-create-view.jsx`.
- **Status**: `SIAP UAT (95%)`.

#### Spesifikasi Kontrak API (Swagger-Style)
`[Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi & Hak Akses | Format Permintaan / Respons |
|---|---|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/personnel-action-orders/summary` | Mengambil ringkasan metrik SK (Total, Menunggu Persetujuan, Diterapkan, Ditolak). | `Bearer Token` (`HrdPersonnelActionOrder:Read`) | **Response**: `ApiResponse<PersonnelActionOrderSummaryResponse>` |
| `GET` | `/api/v1/corporate/human-resource/personnel-action-orders` | Mengambil daftar SK dengan filter nomor, kategori, status, dan unit. | `Bearer Token` (`HrdPersonnelActionOrder:Read`) | **Query**: `pageNumber`, `pageSize`, `actionCategory`, `orderStatus`<br>**Response**: `ApiResponse<PagedResult<PersonnelActionOrderListResponse>>` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders` | Membuat draf SK kepegawaian baru beserta data penempatan target. | `Bearer Token` (`HrdPersonnelActionOrder:Create`) | **Body**: `CreatePersonnelActionOrderRequest` (Pegawai, Kategori, Tgl Efektif, Unit/Jabatan Baru, Gaji Pokok, Alasan)<br>**Response**: `201 Created` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/attachments` | Mengunggah berkas fisik pindaian SK bertanda tangan basah. | `Bearer Token` (`HrdPersonnelActionOrder:Update`) | **Body**: `multipart/form-data` (`file` PDF/PNG maks 10MB)<br>**Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/submit` | Mengajukan draf SK ke tahap persetujuan berjenjang. | `Bearer Token` (`HrdPersonnelActionOrder:Update`) | **Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/approve` | Menyetujui SK dan secara atomik menerapkan penempatan baru saat tanggal efektif tiba. | `Bearer Token` (`HrdPersonnelActionOrder:Update`) | **Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/reject` | Menolak permohonan SK dengan alasan wajib. | `Bearer Token` (`HrdPersonnelActionOrder:Update`) | **Body**: `RejectOrderRequest` (`Reason`)<br>**Response**: `200 OK` |

- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. SK berstatus Draf atau Ditolak tidak mengubah unit atau jabatan pegawai pada master.
  2. Saat SK disetujui dan tanggal efektif telah tiba, data penempatan master diperbarui tepat satu kali.
  3. Dokumen pindaian fisik dapat diunggah dan diunduh kembali dengan aman.

---

### Task 3: ST-IMP Implementasi Surat Tugas (Official Duty Assignment)
- **Tujuan**: Mengimplementasikan alur Surat Tugas dinas luar rumah sakit dengan verifikasi berjenjang: Pengajuan oleh Kepala Unit -> Verifikasi kelengkapan & jadwal oleh HRD -> Pengesahan oleh Pejabat Struktural -> Penerbitan Surat Resmi ber-KOP & QR Code.
- **Berkas Terkait**:
  - Backend: `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/OfficialDutyRequestController.cs`, `OfficialDutyRequestService.cs`.
  - Frontend: `src/lib/services/hr/official-duty-service.js`, antarmuka pengajuan dan antrean verifikasi Surat Tugas.
- **Status**: `DALAM PENGERJAAN (50%)`.

#### Spesifikasi Kontrak API (Swagger-Style)
`[Tags("HR Attendance & Official Duty Administration")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi & Hak Akses | Format Permintaan / Respons |
|---|---|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/attendance-management/official-duties` | Mengambil daftar penugasan dinas dengan filter status dan rentang tanggal. | `Bearer Token` (`OfficialDutyRequest:Read`) | **Query**: `pageNumber`, `dutyCategory`, `status`, `startDate`, `endDate`<br>**Response**: `ApiResponse<OfficialDutyPagedResponse>` |
| `GET` | `/api/v1/corporate/human-resource/attendance-management/official-duties/{id}` | Mengambil rincian penugasan dinas beserta daftar peserta. | `Bearer Token` (`OfficialDutyRequest:Read`) | **Response**: `ApiResponse<OfficialDutyResponse>` |
| `POST` | `/api/v1/corporate/human-resource/attendance-management/official-duties` | Mengajukan penugasan dinas resmi oleh Kepala Unit. | `Bearer Token` (`OfficialDutyRequest:Create`) | **Body**: `CreateOfficialDutyRequest` (Kategori, Nama Tugas, Tujuan, Periode Mulai/Selesai, Daftar Peserta, Dokumen Dasar)<br>**Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/attendance-management/official-duties/{id}/approve` | Pengesahan tugas dinas oleh Pejabat Berwenang. | `Bearer Token` (`OfficialDutyRequest:Approve`) | **Body**: `ApproveOfficialDutyRequest` (`ApproverWorkforceProfileId`, `ApprovalNote`)<br>**Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/attendance-management/official-duties/{id}/reject` | Menolak pengajuan tugas dinas dengan alasan wajib. | `Bearer Token` (`OfficialDutyRequest:Reject`) | **Body**: `RejectOfficialDutyRequest` (`RejectionReason`)<br>**Response**: `200 OK` |

- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. Satu permohonan dapat memuat lebih dari satu pegawai yang ditugaskan.
  2. Kepala Unit hanya dapat memilih staf bawahan di unitnya yang berstatus aktif.
  3. Sistem memunculkan peringatan jika ada jadwal shift kerja atau dinas lain yang tumpang tindih.
  4. Naskah terbit menggunakan kop surat resmi rumah sakit dan mencantumkan nomor formal.

---

### Task 4: SKT-IMP Implementasi Surat Keterangan Resmi
- **Tujuan**: Menerbitkan surat keterangan fakta kepegawaian (keterangan aktif bekerja, pengalaman kerja, penghasilan KPR) dengan penarikan otomatis dari Payroll Sub-10 dan verifikasi publik via QR Code.
- **Berkas Terkait**:
  - Backend: `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs`, `EmployeeDocumentIssuanceService.cs`.
  - Frontend: `src/lib/services/hr/hr-service.js`, `src/components/view/hr/services/portal-my-documents-view.jsx`.
- **Status**: `SIAP UAT TERBATAS (85%)` (Perbaikan logika pemisahan pemohon vs penerima teridentifikasi).

#### Spesifikasi Kontrak API (Swagger-Style)
`[Tags("HR Services - Document Issuance & Verification")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi & Hak Akses | Format Permintaan / Respons |
|---|---|---|---|---|
| `POST` | `/api/v1/corporate/hrd/services/documents/request` | Mengajukan permohonan naskah dinas resmi. Divalidasi Anti-Manual Payroll Guard (VR-HRS-06). | `Bearer Token` (`EmployeeDocumentIssuance:Request`) | **Query**: `workforceProfileId`<br>**Body**: `SubmitDocumentRequestDto`<br>**Response**: `201 Created` |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/cb-approve` | Persetujuan Tier 1 oleh Supervisor Kompensasi & Benefit untuk dokumen finansial/KPR. | `Bearer Token` (Role `HRAdmin,HRManager,HRStaff`) | **Body**: `CbSupervisorApprovalDto`<br>**Response**: `200 OK` |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/authorize-issue` | Otorisasi final dan penerbitan naskah dinas resmi bernomor formal dan stempel digital. | `Bearer Token` (Role `SuperAdmin,HRAdmin,HRManager`) | **Body**: `FinalAuthorizeAndIssueDto`<br>**Response**: `200 OK` |
| `GET` | `/api/v1/corporate/hrd/services/documents/{id}/download` | Mengunduh salinan resmi berkas PDF naskah dinas. | `Bearer Token` (`EmployeeDocumentIssuance:Download`) | **Response**: `application/pdf` |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/revoke` | Mencabut naskah dinas resmi dengan nomor BAP dan alasan pencabutan. | `Bearer Token` (Role `SuperAdmin,HRAdmin,HRManager`) | **Body**: `RevokeDocumentRequestDto`<br>**Response**: `200 OK` |
| `GET` | `/api/v1/corporate/hrd/services/documents/public-verify/{hash}` | Verifikasi keaslian naskah dinas secara publik via pemindaian QR Code. | `AllowAnonymous` (Publik) | **Response**: `PublicDocumentVerificationResultDto` (Data finansial tersamar) |
| `POST` | `/api/v1/corporate/hrd/services/documents/unlock-financial` | Membuka kunci nominal gaji pada verifikasi publik menggunakan 4-digit PIN rahasia. | `AllowAnonymous` (Publik) | **Body**: `UnlockFinancialDetailsRequestDto`<br>**Response**: `200 OK` |

- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. Surat Keterangan Penghasilan dilarang input manual; jika slip gaji final belum terbit di Payroll, sistem menolak pengajuan.
  2. Dokumen yang dicabut tampil jelas sebagai *TIDAK BERLAKU* pada verifikasi publik dan berkas cetak.
  3. Pegawai hanya dapat mengunduh dokumen miliknya sendiri.

---

### Task 5: DA-IMP Integrasi Dokumen Administrasi & Registri
- **Tujuan**: Menyediakan satu pintu registri pencarian arsip dokumen (SK, Surat Tugas, Surat Keterangan, STR/SIP/Ijazah) bagi staf HRD dan audit akreditasi rumah sakit.
- **Berkas Terkait**:
  - Backend: `WfpDocument.cs`, `TrxEmployeeDocumentIssuance.cs`, `EmployeeDocumentIssuanceController.cs`.
  - Frontend: Registri Dokumen Administrasi terpadu pada menu Administrasi Kepegawaian.
- **Status**: `DALAM PENGERJAAN (60%)`.

- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. Seluruh dokumen SK, Surat Tugas, dan Surat Keterangan yang terbit otomatis terindeks pada registri.
  2. Pencarian cepat berdasarkan nomor surat, nama pegawai, NIP, jenis dokumen, dan rentang tanggal.
  3. Pratinjau dokumen aman tanpa membuka lokasi penyimpanan fisik di server.

---

### Task 6: PD-IMP Penyempurnaan Pengajuan Perubahan Data Karyawan
- **Tujuan**: Memastikan permohonan perubahan profil pegawai berjalan melalui verifikasi berkas bukti pendukung, komparasi nilai lama vs baru, pencegahan verifikasi mandiri (*anti-self-approval*), dan penerapan atomik ke master pegawai.
- **Berkas Terkait**:
  - Backend: `Areas/Corporate/HumanResource/WorkforceCore/Controllers/EmployeeProfileChangeController.cs`, `EmployeeProfileChangeService.cs`.
  - Frontend: `src/app/hr/workforce-core/employee-profile-changes/page.jsx`, `employee-profile-changes-view.jsx`, `create-on-behalf/page.jsx`.
- **Status**: `SIAP UAT (95%)`.

#### Spesifikasi Kontrak API (Swagger-Style)
`[Tags("Corporate / Human Resource / Workforce Core / Employee Profile Change Administration")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi & Hak Akses | Format Permintaan / Respons |
|---|---|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/employee-profile-changes/summary` | Mengambil ringkasan metrik status perubahan data. | `Bearer Token` (`EmployeeProfileChange:Read`) | **Response**: `ApiResponse<EmployeeProfileChangeSummaryResponse>` |
| `GET` | `/api/v1/corporate/human-resource/employee-profile-changes` | Mengambil daftar permohonan perubahan profil lintas pegawai. | `Bearer Token` (`EmployeeProfileChange:Read`) | **Query**: `pageNumber`, `pageSize`, `requestStatus`, `requestCategory`<br>**Response**: `ApiResponse<PagedResult<EmployeeProfileChangeListResponse>>` |
| `GET` | `/api/v1/corporate/human-resource/employee-profile-changes/{id}` | Mengambil rincian permohonan, komparasi field lama vs baru, dan dokumen bukti. | `Bearer Token` (`EmployeeProfileChange:Read`) | **Response**: `ApiResponse<EmployeeProfileChangeResponse>` |
| `POST` | `/api/v1/corporate/human-resource/employee-profile-changes/on-behalf` | Mengajukan permohonan perubahan profil atas nama pegawai lain oleh HRD. | `Bearer Token` (`EmployeeProfileChange:Create`) | **Body**: `multipart/form-data` (`CreateOnBehalfProfileChangeRequest`)<br>**Response**: `201 Created` |
| `POST` | `/api/v1/corporate/human-resource/employee-profile-changes/{id}/start-verification` | Memulai verifikasi permohonan oleh HRD (dengan perlindungan Anti-Self-Approval). | `Bearer Token` (`EmployeeProfileChange:Update`) | **Response**: `200 OK` |
| `POST` | `/api/v1/corporate/human-resource/employee-profile-changes/{id}/apply` | Menerapkan nilai perubahan baru secara atomik ke master `MstEmployee` setelah disetujui. | `Bearer Token` (`EmployeeProfileChange:Update`) | **Body**: `ApplyEmployeeProfileChangeRequest`<br>**Response**: `200 OK` |
| `GET` | `/api/v1/corporate/human-resource/employee-profile-changes/{id}/evidence/download` | Mengunduh berkas fisik bukti pendukung permohonan (KTP, KK, Buku Tabungan). | `Bearer Token` (`EmployeeProfileChange:Read`) | **Response**: Berkas gambar/PDF pendukung |

- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. Master data tidak berubah saat permohonan berstatus Draf, Diajukan, Sedang Diverifikasi, atau Ditolak.
  2. Pegawai atau staf HRD tidak dapat memverifikasi permohonan yang diajukannya sendiri.
  3. Tombol Apply hanya berfungsi saat permohonan telah berstatus Disetujui (*Approved*).
  4. Komparasi nilai sebelum dan sesudah tersimpan permanen untuk kebutuhan audit rumah sakit.

---

### Task 7: REG-IMP Integrasi dan Regresi Lintas Submenu
- **Tujuan**: Memverifikasi kepatuhan RBAC, integritas tautan antar-dokumen, dan memastikan tidak ada tabrakan rute atau duplikasi entitas.
- **Status**: `SIAP OPERASIONAL SECARA STATIS (80%)`.
- **Kriteria Keberterimaan (Acceptance Criteria)**:
  1. Hak akses masing-masing peran (Kepala Unit, HRD, Direksi, Pegawai) terlindungi di lapisan backend controller.
  2. Seluruh dokumen naskah dinas resmi terhubung dengan profil pegawai pemiliknya.
  3. Tidak ada kebocoran informasi penghasilan/gaji kepada pengguna tanpa hak akses.
