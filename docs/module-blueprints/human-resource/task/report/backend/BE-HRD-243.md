# Laporan Penyelesaian Task: BE-HRD-243 Penjadwalan Sesi Kelas Diklat, Workshop Lab Simulasi & Webinar

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-243`
* **Nama Task**: Penjadwalan Sesi Kelas Diklat, Workshop Laboratorium Simulasi & Webinar (`TrainingSessionService` & Controller)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingSessionDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingSessionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingSessionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingSessionsController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Tata Kelola Penjadwalan Batch Pelatihan Rumah Sakit
Dalam penyelenggaraan diklat rumah sakit (baik workshop bantuan hidup dasar, pencegahan infeksi, maupun simulasi ruang intensif ICU/OK), penjadwalan batch kelas wajib dikendalikan dengan batasan kapasitas simulator dan pencegahan bentrok fasilitas (`VR-TRN-07`, `VR-TRN-08`, `AT-HRD-TRN-09`).

### 1. Invarian Kuota & Kapasitas Peserta (`VR-TRN-07`, `AT-HRD-TRN-09`)
1. Setiap sesi memiliki batas kapasitas maksimum (`Capacity`) yang disesuaikan dengan ketersediaan manekin resusitasi, ruang lab basah, atau rasio instruktur-peserta (maksimal 1:10 untuk simulasi klinis kritis).
2. Sistem secara otomatis menghitung sisa kuota yang tersedia:
   $$\text{RemainingQuota} = \max(0, \text{Capacity} - \text{RegisteredParticipantCount})$$
3. Ketika kuota telah habis, sistem menolak pendaftaran tambahan dengan status penolakan `409 Conflict`.

### 2. Pencegahan Bentrok Jadwal Ruang & Instruktur Klinis (`VR-TRN-08`)
Sistem secara proaktif memvalidasi irisan jadwal waktu:
- **Konflik Ruang / Venue**: Jika ruang/aula/lab yang sama digunakan pada interval waktu:
  $$\text{StartDateTime} < \text{Existing.EndDateTime} \land \text{EndDateTime} > \text{Existing.StartDateTime}$$
  Maka sistem menolak jadwal baru dengan respons `409 Conflict` ("Jadwal sesi pelatihan bentrok dengan sesi lain di lokasi/ruang tersebut").
- **Konflik Instruktur Klinis**: Jika dokter spesialis / nakes penguji yang sama ditugaskan mengajar pada rentang waktu yang beririsan, pendaftaran sesi ditolak untuk mencegah pengabaian tugas mengajar atau jadwal klinis.

### 3. Penomoran Sesi Terstandarisasi
Setiap batch sesi kelas memiliki kode unik baku:
$$\text{SES}-\{\text{PlanYear}\}-\text{B}\{\text{BatchSequence}\} \quad (\text{Contoh: } \text{SES-2026-B1})$$

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Clinical Competency & OSCE Assessment")]`
**Base Route**: `/api/v1/corporate/hrd/learning/sessions`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/sessions` | Mengambil seluruh jadwal sesi pelatihan kelas, workshop lab, dan webinar dengan filter tanggal dan rencana kerja. | `[Authorize]` Pegawai / Diklat | Filter: `trainingPlanId`, `status`, `fromDate`, `toDate` | `200 OK` (`ApiResponse<List<TrainingSessionDto>>`) |
| `GET` | `/api/v1/corporate/hrd/learning/sessions/{id}` | Mengambil detail spesifikasi sesi pelatihan berdasarkan ID. | `[Authorize]` Pegawai / Diklat | - | `200 OK` (`ApiResponse<TrainingSessionDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/sessions` | Menjadwalkan batch sesi pelatihan baru lengkap dengan instruktur, venue, kuota (`VR-TRN-07`), dan anti-bentrok (`VR-TRN-08`). | `[Authorize]` Diklat / SuperAdmin | `CreateTrainingSessionRequestDto` | `201 Created` (`ApiResponse<TrainingSessionDto>`) |
| `PUT` | `/api/v1/corporate/hrd/learning/sessions/{id}` | Memperbarui jadwal sesi pelatihan, venue, atau instruktur. | `[Authorize]` Diklat / SuperAdmin | `UpdateTrainingSessionRequestDto` | `200 OK` (`ApiResponse<TrainingSessionDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/sessions/{id}/open-registration` | Membuka masa pendaftaran mandiri staf via ESS untuk sesi pelatihan ini. | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<TrainingSessionDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/sessions/{id}/close-registration` | Menutup masa pendaftaran pendaftaran sesi pelatihan menjelang pelaksanaan. | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<TrainingSessionDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/sessions/{id}/complete` | Menandai pelaksanaan sesi pelatihan telah selesai dilaksanakan. | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<TrainingSessionDto>`) |
| `DELETE` | `/api/v1/corporate/hrd/learning/sessions/{id}` | Menghapus sesi pelatihan dari sistem. | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<bool>`) |

---

## 4. Contoh Konkret Request & Response API

### Skenario 1: Penjadwalan Workshop Simulasi BHD (`201 Created`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/sessions`
* **Request Body**:
```json
{
  "trainingPlanId": "f1f2f3f4-1111-2222-3333-444455556666",
  "sessionName": "Workshop Simulasi Resusitasi BHD Gelombang 1",
  "startDateTime": "2026-04-15T08:00:00Z",
  "endDateTime": "2026-04-15T16:00:00Z",
  "deliveryMode": "Classroom",
  "venue": "Laboratorium Simulasi Lantai 3 Gedung Diklat",
  "instructorName": "dr. Robert Sp.An, KIC",
  "capacity": 30,
  "minimumParticipant": 10,
  "requiresAttendance": true,
  "requiresPostTest": true,
  "generatesCertificate": true,
  "minimumPassingScore": 80.0
}
```
* **Response Body (`201 Created`)**:
```json
{
  "success": true,
  "message": "Jadwal sesi kelas pelatihan berhasil dibuat dan pendaftaran dibuka.",
  "data": {
    "id": "e1e2e3e4-aaaa-bbbb-cccc-111122223333",
    "trainingPlanId": "f1f2f3f4-1111-2222-3333-444455556666",
    "sessionCode": "SES-2026-B1",
    "sessionName": "Workshop Simulasi Resusitasi BHD Gelombang 1",
    "startDateTime": "2026-04-15T08:00:00Z",
    "endDateTime": "2026-04-15T16:00:00Z",
    "deliveryMode": "Classroom",
    "venue": "Laboratorium Simulasi Lantai 3 Gedung Diklat",
    "instructorName": "dr. Robert Sp.An, KIC",
    "capacity": 30,
    "registeredParticipantCount": 0,
    "remainingQuota": 30,
    "sessionStatus": "RegistrationOpen",
    "minimumPassingScore": 80.0,
    "isActive": true
  }
}
```

### Skenario 2: Penolakan Bentrok Jadwal Ruang Lab (`409 Conflict`, `VR-TRN-08`)
* **Request Body**: Menjadwalkan sesi lain di ruang "Laboratorium Simulasi Lantai 3 Gedung Diklat" pada tanggal 15 April 2026 jam 10:00 - 14:00.
* **Response Body (`409 Conflict`)**:
```json
{
  "success": false,
  "message": "Jadwal sesi pelatihan bentrok dengan sesi lain di lokasi/ruang 'Laboratorium Simulasi Lantai 3 Gedung Diklat' pada rentang waktu yang sama (VR-TRN-08)."
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Pengendalian Kuota Peserta (`VR-TRN-07`, `AT-HRD-TRN-09`)**: ✅ Lulus. Kapasitas minimal 1 orang ditegakkan, dan kuota sisa terhitung secara real-time.
2. **Pencegahan Bentrok Fasilitas & Instruktur (`VR-TRN-08`)**: ✅ Lulus. Irisan waktu pada venue atau instruktur penilai yang sama secara deterministik diblokir dengan status HTTP 409.
3. **Penomoran Kode Sesi Otomatis**: ✅ Lulus. Format `SES-{PlanYear}-B{Batch}` terbit secara konsisten.
