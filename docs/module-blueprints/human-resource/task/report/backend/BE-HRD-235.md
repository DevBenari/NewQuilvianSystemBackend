# Laporan Penyelesaian Task: BE-HRD-235 Penetapan 4 Putusan Sidang Rekredensialing & Dispatcher Integrasi SIMRS EMR

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-235`
* **Nama Task**: Pembangunan Layanan Penetapan 4 Putusan Sah Sidang Rekredensialing, Penandatanganan Digital SHA-256, dan Dispatcher Sinkronisasi Wewenang Klinis ke Modul SIMRS EMR
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IRecredentialingCompositeProvider.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/ISimrsClinicalPrivilegeDispatcher.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/RecredentialingQualityController.cs`

---

## 2. Tata Laksana Putusan Sidang Rekredensialing & Integrasi SIMRS EMR
Sesuai amanat Permenkes Kredensial Rumah Sakit, penetapan hasil evaluasi berkala 3 tahun dokter wajib diputuskan melalui sidang pleno Komite Medik dan disinkronkan secara *fail-closed* ke modul pelayanan rumah sakit (SIMRS EMR) melalui adapter `INT-HR-76`:

### 1. Empat Putusan Baku Sidang Rekredensialing (`VR-OPP-20`)
1. **`RenewAsIs` (Perpanjang Penuh)**: Kewenangan klinis mandiri (Level 4) diperpanjang untuk siklus 3 tahun ke depan tanpa pembatasan.
2. **`Upgrade` (Peningkatan Kewenangan)**: Dokter memperoleh persetujuan penambahan rincian kewenangan tindakan mandiri baru pasca-evaluasi proctoring.
3. **`Restricted` (Pembatasan Wewenang Level 3 Supervisi) (`VR-OPP-21`)**:
   - Akibat penurunan performa atau riwayat insiden berulang, status kewenangan klinis diturunkan ke Level 3 supervisi.
   - Sinyal ditransmisikan ke SIMRS EMR sehingga setiap instruksi medis dokter target **tertahan** dan mewajibkan tanda tangan pendamping (*co-signature*) oleh dokter supervisor senior dalam kurun waktu 24 jam.
4. **`Revoked` (Pencabutan Kewenangan Mutlak & Hard-Stop SIMRS) (`VR-OPP-22`, `AT-HRD-OPP-22`)**:
   - Sanksi fatal atas pelanggaran berat keselamatan pasien atau etika profesi.
   - Dispatcher mengirim instruksi seketika (*real-time kill switch*) ke modul EMR: Seluruh menu peresepan farmasi, pemesanan laboratorium/radiologi, dan penjadwalan kamar operasi (OK) dokter terkunci total secara mutlak (*Hard-Stop*).

### 2. Validasi Struktur Kontrak Transmisi Data SIMRS (`VR-OPP-25`)
Payload integrasi `INT-HR-76` divalidasi ketat: wajib memuat `verdictId`, `doctorCode`, level klinis (1-4), stempel waktu UTC, dan penanda `isHardStopBlocked`. Transmisi yang tidak lengkap ditolak dengan pesan galat standard.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/verdicts` | Menerbitkan berita acara putusan sah sidang pleno rekredensialing Komite Medik. | `[Authorize]` Ketua Komite Medik | `CreateRecredVerdictRequestDto` | `201 Created` (`ApiResponse<RecredVerdictResponseDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/verdicts/{id}/dispatch-simrs` | Mengirim instruksi pembaruan wewenang Level 1-4 ke SIMRS EMR (`INT-HR-76`). | `[Authorize]` Administrator Komite | `DispatchSimrsPrivilegeRequestDto` | `200 OK` (`ApiResponse<SimrsPrivilegeSyncAuditLogDto>`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/recredentialing/simrs-sync/audit-logs` | Menampilkan log riwayat transmisi sinkronisasi wewenang ke SIMRS EMR. | `[Authorize]` Komite Medik / Audit | - | `200 OK` (`ApiResponse<List<SimrsPrivilegeSyncAuditLogDto>>`) |

---

## 4. Contoh Konkret Skenario Hard-Stop SIMRS

### Skenario: Transmisi Putusan Pencabutan Kewenangan (Revoked)
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/recredentialing/verdicts/a5b6c7d8-e9f0-1234-5678-90abcdef1234/dispatch-simrs`
* **Request Body**:
```json
{
  "verdictId": "a5b6c7d8-e9f0-1234-5678-90abcdef1234",
  "doctorCode": "DOC-ORT-012",
  "tindakanId": null,
  "level": 1,
  "notes": "Eksekusi putusan pleno Revoked atas insiden sentinel keselamatan pasien. Blokir seketika hak order resep dan OK di SIMRS."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Instruksi pembaruan wewenang klinis berhasil dikirimkan ke modul SIMRS EMR.",
  "data": {
    "id": "7b8a9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
    "verdictId": "a5b6c7d8-e9f0-1234-5678-90abcdef1234",
    "doctorCode": "DOC-ORT-012",
    "targetLevel": 1,
    "isHardStopBlocked": true,
    "responseStatusCode": 200,
    "latencyMs": 38,
    "dispatchedAt": "2026-11-09T14:57:10Z",
    "status": "SUCCESS",
    "message": "Akses peresepan obat keras dan penjadwalan tindakan operasi di modul SIMRS EMR diblokir total secara seketika (Hard-Stop Mutlak VR-OPP-22)."
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Batasan 4 Putusan Sah Sidang (`VR-OPP-20`)**: ✅ Lulus. Putusan di luar 4 pilihan baku ditolak; berita acara ditandatangani stempel digital SHA-256.
2. **Pembatasan Wewenang Level 3 Supervisi (`VR-OPP-21`)**: ✅ Lulus. Transmisi ke SIMRS mengaktifkan kewajiban co-signature digital 24 jam.
3. **Hard-Stop Mutlak SIMRS EMR (`VR-OPP-22`, `VR-OPP-25`, `AT-HRD-OPP-22`)**: ✅ Lulus. Putusan Revoked mengirimkan sinyal pemblokiran total hak order dan reservasi OK secara seketika (*fail-closed*).
