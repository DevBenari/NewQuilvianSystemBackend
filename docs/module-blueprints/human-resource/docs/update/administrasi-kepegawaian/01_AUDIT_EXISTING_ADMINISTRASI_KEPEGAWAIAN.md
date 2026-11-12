# Audit Kemampuan Existing Administrasi Kepegawaian SIMRS Quilvian V2

**Dokumen**: `01_AUDIT_EXISTING_ADMINISTRASI_KEPEGAWAIAN.md`  
**Modul**: Corporate / Human Resource Management (HRD)  
**Menu Induk**: Administrasi Kepegawaian (`corporateHumanResourceWorkforceCore`)  
**Tanggal Audit**: 11 November 2026  
**Status Audit**: Selesai (Static Analysis & Code Tracing Terverifikasi)  

---

## 1. Identifikasi Repository, Branch, dan Komitmen Git Aktual

Sesuai aturan keselamatan rekayasa perangkat lunak Quilvian, seluruh audit dilakukan secara read-only terhadap workspace lokal yang aktif tanpa melakukan mutasi remote GitHub.

| Komponen | Target Dokumen | Cabang (Branch) Terpasang | Commit SHA Head Aktual | Status Sinkronisasi |
|---|---|---|---|---|
| **Frontend** (`QuilvianSystemFrontendDev`) | `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `d09dc74da510f9bb6f59aa6062c77cff44a5c518` | Sesuai persis dengan target referensi instrumen |
| **Backend** (`NewQuilvianSystemBackend`) | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `26b31514810c8ad9109c956ae6d134361407a16a` | Cabang sesuai. HEAD lokal maju 11 commit dari referensi awal (`f751f8cac4e477f216d3ccb444a1fae8a052b3aa`) |

> [!NOTE]
> Kemajuan 11 commit pada backend mencakup pembaruan pada DTO `EmployeeProfileChangeDtos.cs`, servis `EmployeeProfileChangeService.cs`, pengontrol mandiri `EmployeeProfileChangeSelfServiceController.cs`, serta pelaporan task frontend terdahulu.

---

## 2. Matriks 5 Cakupan Bisnis Administrasi Kepegawaian

Berdasarkan inventarisasi berkas frontend dan backend, berikut klasifikasi kemampuan sistem terhadap 5 cakupan yang diamanatkan:

| No | Cakupan Bisnis | Status Penilaian | Entitas & Controller Backend | Halaman & Komponen Frontend | Keterangan & Batasan |
|---|---|---|---|---|---|
| 1 | **Surat Keputusan (SK)** | `REUSE_EXISTING` | `HrdPersonnelActionOrderController.cs`<br>`HrdPersonnelActionOrderService.cs`<br>`TrxPersonnelActionOrder` | `/hr/workforce-core/personnel-actions`<br>`personnel-action-order-list-view.jsx`<br>`personnel-action-order-create-view.jsx` | Telah mengimplementasikan draft, pengajuan, persetujuan, penolakan, komparasi penempatan, dan unggah pindaian fisik dokumen SK. |
| 2 | **Surat Tugas** | `EXTEND_EXISTING` | `OfficialDutyRequestController.cs`<br>`OfficialDutyRequestService.cs`<br>`HrdOfficialDutyRequest` | `/hr/attendance/official-duties` (parsial presensi)<br>`official-duty-service.js` | Eksisting hanya mencakup penugasan 1 orang untuk presensi luar (ambulans/kurir lab). Belum memiliki verifikasi HRD berjenjang, penugasan multi-pegawai, template cetak kop resmi Surat Tugas, maupun integrasi ke Administrasi Kepegawaian. |
| 3 | **Surat Keterangan** | `REUSE_EXISTING` (dengan penyesuaian) | `EmployeeDocumentIssuanceController.cs`<br>`EmployeeDocumentIssuanceService.cs`<br>`MstEmployeeDocumentType`<br>`TrxEmployeeDocumentIssuance` | `/corporate/hrd/services/tickets`<br>`/portal/services/requests`<br>`hr-service.js` | Telah mendukung pengajuan, persetujuan C&B, otorisasi final, penomoran otomatis, watermarking BAP, dan QR code verifikasi. Memerlukan penyesuaian pemisahan ID pengaju vs pegawai tujuan pada alur pengajuan atasan. |
| 4 | **Dokumen Administrasi** | `EXTEND_EXISTING` | `EmployeeDocumentIssuanceController.cs`<br>`WfpDocument`<br>`TrxEmployeeDocumentIssuance` | `/portal/services/my-documents`<br>`PortalMyDocumentsView.jsx`<br>`Employee360View` (tab Dokumen) | Belum ada layar registri terpadu satu pintu untuk seluruh jenis arsip (SK, Surat Tugas, Keterangan, Ijazah/STR/SIP) pada menu Administrasi Kepegawaian untuk staf HRD. |
| 5 | **Pengajuan Perubahan Data** | `REUSE_EXISTING` | `EmployeeProfileChangeController.cs`<br>`EmployeeProfileChangeService.cs`<br>`TrxEmployeeProfileChangeRequest` | `/hr/workforce-core/employee-profile-changes`<br>`employee-profile-changes-view.jsx`<br>`create-on-behalf/page.jsx` | Telah lengkap: draft, on-behalf kuasa HR, verifikasi HRD, anti-self-approval guard, komparasi nilai lama vs baru, unduh bukti, dan integrasi workflow engine. |

---

## 3. Penelusuran Sumber Bukti (Source Evidence) Mendalam

### 3.1. Cakupan 1: Surat Keputusan (SK Kepegawaian)
- **Menu Navigasi**: `menu-items.jsx:613` -> Label `"Dokumen SK Kepegawaian"`, path `/hr/workforce-core/personnel-actions`.
- **Backend Controller**: `HrdPersonnelActionOrderController.cs` (Route: `api/v1/corporate/human-resource/personnel-action-orders`).
  - `GET filters/metadata`: Mendapatkan metadata filter (kategori tindakan, unit, status).
  - `GET summary`: Mendapatkan metrik ringkasan SK (Total, Menunggu Persetujuan, Diterapkan, Ditolak).
  - `GET`: Mengambil daftar paged SK dengan filter multi-parameter.
  - `GET {id}`: Rincian SK dan komparasi penempatan sebelum/sesudah.
  - `POST`: Membuat draft SK baru dengan validasi integritas penempatan.
  - `PUT {id}`: Memperbarui draft SK sebelum diajukan.
  - `POST {id}/attachments`: Mengunggah berkas pindaian SK bertanda tangan basah (PDF/PNG, max 10MB).
  - `GET {id}/attachments/{attachmentId}`: Mengunduh pindaian berkas SK.
  - `POST {id}/submit`: Mengajukan draft ke tahapan persetujuan direksi/pejabat.
  - `POST {id}/approve`: Persetujuan resmi yang secara atomik menerapkan mutasi/promosi/demosi ke master penempatan saat tanggal efektif tercapai.
  - `POST {id}/reject`: Menolak permohonan SK dengan alasan wajib.
  - `POST {id}/cancel`: Membatalkan SK yang telah disahkan dengan menyertakan SK pembatalan kontra.
- **Frontend View**:
  - `personnel-action-order-list-view.jsx`: Menggunakan `SummaryGrid`, `DataFilter`, `DataTable`, dan `RegionPagination`. Terhubung langsung ke API melalui `PersonnelActionOrderService`.
  - `personnel-action-order-create-view.jsx`: Form bertahap terstruktur (`BaseGroupedEditorView`) dengan panel `CurrentEmployeeInfoPanel` untuk membandingkan penempatan lama dan penempatan baru secara dinamis.

### 3.2. Cakupan 2: Surat Tugas (Official Duty)
- **Backend Controller**: `OfficialDutyRequestController.cs` (Route: `api/v1/corporate/human-resource/attendance-management/official-duties`).
  - Model data: `HrdOfficialDutyRequest.cs`.
  - Mengelola `DutyCategory` (AmbulanceReferral, LabCourier, TrainingSeminar, HomeCare, Other).
  - **Keterbatasan Kritis Terverifikasi**:
    1. Hanya menyimpan tepat 1 `WorkforceProfileId` per baris penugasan. Tidak memiliki tabel relasi anak untuk penugasan tim (multi-pegawai dalam 1 surat tugas).
    2. Alur persetujuan hanya 1 tahap (`Approve` / `Reject`), belum membedakan verifikasi kelengkapan HRD dengan pengesahan pejabat yang berwenang (misal: Direktur Medis untuk tugas klinis).
    3. Belum menghasilkan naskah dinas resmi bernomor formal, kop surat rumah sakit, atau QR code validasi keaslian.

### 3.3. Cakupan 3: Surat Keterangan Resmi
- **Backend Controller**: `EmployeeDocumentIssuanceController.cs` (Route: `api/v1/corporate/hrd/services/documents`).
  - `POST request`: Pengajuan surat keterangan (mendukung penarikan data gaji otomatis).
  - `POST {id}/cb-approve`: Persetujuan Tier 1 oleh Supervisor Kompensasi & Benefit untuk permohonan finansial/KPR.
  - `POST {id}/authorize-issue`: Otorisasi final oleh Manajer SDM / Pejabat Direksi dengan penguncian nomor surat resmi dan stempel digital.
  - `GET queue`: Antrean operasional penerbitan surat bagi staf HRD.
  - `GET {id}/download`: Pengunduhan naskah dinas PDF dengan pengamanan hak unduh.
  - `POST {id}/revoke`: Pencabutan surat resmi yang salah/batal dengan wajib mencantumkan Berita Acara Pencabutan (BAP).
  - `GET public-verify/{hash}`: Verifikasi keaslian publik via tautan QR Code.
  - `POST unlock-financial`: Pembukaan rincian nominal gaji pada halaman publik dengan 4-digit PIN terenkripsi.
- **Frontend Integrasi**:
  - `hr-service.js`: Memuat pemanggil API Axios lengkap untuk seluruh alur penerbitan dokumen resmi.
  - `PortalMyDocumentsView.jsx`: Antarmuka ESS untuk membuka arsip surat terbit milik pegawai dan membuka PIN privat.
  - **Temuan Frontend**: `PortalMyDocumentsView` dan `PortalServiceRequestsView` masih menyertakan fallback data tiruan (mock) lokal jika props awal tidak diberikan oleh halaman induk.

### 3.4. Cakupan 4: Dokumen Administrasi & Registri
- **Model Backend**:
  - `WfpDocument.cs`: Menyimpan metadata berkas fisik (Ijazah, STR, SIP, Sertifikat Pelatihan, KTP, NPWP) per profil pegawai.
  - `TrxEmployeeDocumentIssuance.cs`: Menyimpan riwayat seluruh naskah dinas resmi yang diterbitkan SIMRS Quilvian, termasuk relasi ke arsip `EDossierDocumentId`.
- **Kondisi Frontend**:
  - Pengelolaan berkas arsip tersebar di dalam tab profil karyawan (`Employee360View` -> tab Dokumen) dan portal ESS (`/portal/services/my-documents`).
  - **Gap**: Belum tersedia layar "Registri Dokumen Administrasi" terpadu di menu `Administrasi Kepegawaian` bagi staf HRD untuk mencari, memantau masa berlaku (STR/SIP/Surat), mengunduh, atau mencabut dokumen lintas pegawai.

### 3.5. Cakupan 5: Pengajuan Perubahan Data Karyawan
- **Menu Navigasi**: `menu-items.jsx:582` -> Label `"Perubahan Data Karyawan"`, path `/hr/workforce-core/employee-profile-changes`.
- **Backend Controller**: `EmployeeProfileChangeController.cs` (Route: `api/v1/corporate/human-resource/employee-profile-changes`).
  - `GET filters/metadata` & `GET summary`: Metrik status permohonan.
  - `GET`: Daftar permohonan perubahan profil lintas pegawai.
  - `GET {id}`: Detail permohonan beserta daftar field yang diubah dan bukti pendukung.
  - `POST on-behalf`: Pengajuan perubahan data oleh HRD atas nama pegawai lain (menggunakan `multipart/form-data`).
  - `POST {id}/start-verification`: Memulai verifikasi bukti dan data oleh HRD (dengan perlindungan anti-self-approval).
  - `POST {id}/apply`: Penerapan perubahan nilai baru secara atomik ke entitas master (`MstEmployee`, kontak, alamat) setelah disetujui.
  - `GET {id}/evidence/download`: Pengunduhan berkas bukti pendukung (KTP, KK, Buku Tabungan).
- **Frontend View**:
  - `employee-profile-changes-view.jsx`: Antarmuka rekapitulasi data dengan filter status/kategori, modal rincian verifikasi, komparasi tabel field sebelum vs sesudah, peringatan anti-self-approval visual, dan tautan unduh bukti.
  - `create-on-behalf/page.jsx`: Antarmuka formulir pengajuan atas nama pegawai lain.

---

## 4. Evaluasi Temuan Potensial & Analisis Risiko

Berikut adalah hasil verifikasi terhadap 5 temuan potensial yang dicantumkan dalam instrumen:

| No | Temuan Potensial | Hasil Verifikasi Kode Aktual | Dampak Risiko & Solusi Rekayasa |
|---|---|---|---|
| 1 | FE `startVerification` diduga memanggil `/{id}/verify/start` sedangkan BE memakai `/{id}/start-verification`. | **TERVERIFIKASI SUDAH SESUAI**: Pada berkas `workforce-core-cross-employee-service.js` baris 144, endpoint telah tertulis `${PROFILE_CHANGES_URL}/${id}/start-verification`. | Tidak ada risiko inkonsistensi URL pada fungsi ini; integrasi API sinkron. |
| 2 | BE `EmployeeDocumentIssuanceService.RequestDocumentAsync` mengisi field penerima dari profil pengaju walaupun ada `RequestedForWorkforceProfileId`. | **TERVERIFIKASI CACAT LOGIKA (BUG)**: Baris 45 memuat profil pengaju (`workforceProfileId`). Baris 114 mengisi `RequestedForWorkforceProfileId`, namun baris 115 dan baris 138-139 tetap menggunakan `profile.Employee?.Id` dan `workforceProfileId` milik pengaju. Penarikan slip gaji baris 82 juga memeriksa slip milik pengaju. | **Risiko Tinggi**: Jika Kepala Unit mengajukan surat untuk stafnya, data slip gaji dan pemilik surat yang terbit adalah milik Kepala Unit, bukan staf target. Wajib diperbaiki dengan memuat entitas target terpisah bila `RequestedForWorkforceProfileId` terisi. |
| 3 | `hr-service-tickets-view.jsx` dan `hr-service-catalog-view.jsx` menggunakan data awal statis/mock lokal jika tanpa props. | **TERVERIFIKASI BENAR**: Halaman pembungkus `tickets/page.jsx` memanggil `<HrServiceTicketsView />` tanpa props `initialTickets`, sehingga komponen selalu merender 4 tiket simulasi lokal. Perubahan status di layar hanya mengubah state memori React. | **Risiko Sedang**: Tampilan operasional tidak menampilkan data real database. Harus ditautkan dengan pemanggilan API `hrService.getTicketQueue()` pada siklus hidup `useEffect`. |
| 4 | Template default penerbitan dokumen hardcoded sebagai Surat Keterangan. | **TERVERIFIKASI BENAR**: Baris 868 pada `EmployeeDocumentIssuanceService.cs` memuat judul hardcoded `SURAT KETERANGAN RESMI KEPEGAWAIAN`. Format isi belum menyediakan variabel penugasan dinas luar (lokasi, tanggal mulai-selesai, dasar penugasan, daftar anggota). | **Risiko Sedang**: Penerbitan Surat Tugas melalui modul dokumen akan menghasilkan judul dan format Surat Keterangan yang keliru secara administratif. |
| 5 | Hak akses approval `EmployeeProfileChangeController` memblokir aksi via route legacy (`409 Conflict`). | **TERVERIFIKASI BENAR**: Baris 361-395 pada `EmployeeProfileChangeController.cs` secara sengaja melempar respons `409 Conflict` pada endpoint `approve` dan `reject`, menginstruksikan bahwa persetujuan harus melalui Approval Inbox generic workflow engine. | **Risiko Desain**: Frontend tidak boleh memanggil `approve`/`reject` langsung ke endpoint controller ini, melainkan harus menggunakan `unified-approval-service.js` atau tombol `Apply` setelah workflow berstatus disetujui. |

---

## 5. Ringkasan Kesiapan & Rekomendasi Arsitektur

1. **Surat Keputusan (SK)**: Siap digunakan 100% menggunakan fitur existing `Dokumen SK Kepegawaian` (`/hr/workforce-core/personnel-actions`).
2. **Pengajuan Perubahan Data**: Siap digunakan 100% menggunakan fitur existing `Perubahan Data Karyawan` (`/hr/workforce-core/employee-profile-changes`).
3. **Surat Keterangan**: Memerlukan perbaikan perutean profil penerima pada `EmployeeDocumentIssuanceService` dan penyambungan pemanggilan API aktif pada komponen tampilan.
4. **Surat Tugas**: Memerlukan perancangan sub-entitas multi-pegawai dan penyediaan antrean verifikasi HRD serta pengesahan pejabat struktural.
5. **Dokumen Administrasi**: Memerlukan antarmuka registri pencarian terpadu yang merangkum SK, Surat Tugas, Surat Keterangan, dan arsip kepegawaian dalam satu jendela kerja.
