# Laporan Penyelesaian Task: BE-HRD-215 REST API Controller SPK-RKK, Sidang Komite & Integrasi E-Dossier Sub-02

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-215`
* **Nama Task**: Pembangunan REST API Controller SPK-RKK & Sidang Komite, Template Verifikasi Publik QR Code, serta Integrasi Otomatis Arsip E-Dossier Sub-02 (`INT-HR-68`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Controllers/SpkRkkIssuanceController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Services/SpkRkkIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Models/WfpDocument.cs`
  - `NewQuilvianSystemBackend/Program.cs` (registrasi DI lengkap)

---

## 2. Latar Belakang & Kebutuhan Integrasi Sistem
1. **Pengarsipan Otomatis E-Dossier Pegawai (`INT-HR-68`)**: Surat Penugasan Klinis dan lampiran Rincian Kewenangan Klinis yang telah disahkan Direktur Utama wajib diarsipkan secara otomatis dan permanen ke dalam berkas digital pegawai (*Employee Digital Dossier* pada Sub-02 Workforce Master Data / tabel `WfpDocument`). Hal ini menjamin bahwa seluruh berkas kredensial staf medis dapat diaudit kapan saja oleh surveyor KARS, JCI, maupun Dinas Kesehatan tanpa proses pengunggahan manual.
2. **REST API Terstandar Swagger**: Controller dibangun dengan mematuhi standar API Quilvian v18 menggunakan grup tag `[Tags("HR Credentialing - Committees, Peer Review & SPK-RKK Issuance")]`.

---

## 3. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Credentialing - Committees, Peer Review & SPK-RKK Issuance")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :---: | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/credentialing/committees` | Mengambil daftar 3 komite mandiri rumah sakit (Medik, Keperawatan, Nakes Lain) beserta susunan pejabatnya. | `[Authorize]` Seluruh Pegawai | - | `200 OK` (`List<CredentialingCommitteeDto>`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/whitebook-catalog` | Mengambil katalog buku putih kewenangan klinis sesuai rumpun profesi pemohon. | `[Authorize]` Seluruh Pegawai | Query `?committeeCode=&professionId=` | `200 OK` (`List<ClinicalPrivilegeCatalogItemDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications` | Mengajukan berkas permohonan kredensial nakes (cek kelayakan PSV mandatori `VR-CRD-02`). | `[Authorize]` Nakes / Analis HR | `CreateCredentialingApplicationDto` | `201 Created` (`CredentialingApplicationDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id}` | Mengambil rincian berkas permohonan kredensial dan panel Mitra Bestari yang ditugaskan. | `[Authorize]` Nakes / Komite | - | `200 OK` (`CredentialingApplicationDto`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/assign-reviewers` | Penunjukan panel Mitra Bestari (min 2 reviewer & Anti-Self-Review `VR-CRD-15`). | `[Authorize]` Ketua Komite Kredensial | `AssignPeerReviewersDto` | `200 OK` (`CredentialingApplicationDto`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/assessments` | Pengisian formulir asesmen portofolio klinis per butir tindakan oleh reviewer Mitra Bestari. | `[Authorize]` Reviewer Mitra Bestari | `SubmitPeerReviewAssessmentDto` | `200 OK` (Status berhasil) |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/plenary-session` | Pelaksanaan Sidang Pleno Komite & pengesahan Berita Acara Pleno (BAP) resmi. | `[Authorize]` Ketua Komite Kredensial | `ConcludePlenarySessionDto` | `200 OK` (`PlenarySessionResultDto`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/issue-draft` | Menerbitkan draf SPK-RKK bersiklus 3 tahun berdasarkan rekomendasi BAP Sidang Pleno. | `[Authorize]` Sekretariat Komite / HR | `IssueSpkRkkRequestDto` | `201 Created` (`SpkRkkDetailDto`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id}/sign-dirut` | Otorisasi pengesahan tunggal digital e-Sign Direktur Utama RS & aktivasi hak SIMRS. | `[Authorize]` Direktur Utama RS | `SignSpkRkkDto` | `200 OK` (`SpkRkkDetailDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id}` | Mengambil detail lengkap SPK-RKK, tautan berkas PDF, hash verifikasi, dan status e-Dossier. | `[Authorize]` Nakes / Direksi | - | `200 OK` (`SpkRkkDetailDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/employee/{employeeId}` | Mengambil seluruh riwayat SPK-RKK milik seorang tenaga kesehatan. | `[Authorize]` Nakes / HR Manager | - | `200 OK` (`List<SpkRkkDetailDto>`) |

---

## 4. Contoh Konkret Respons Pengesahan SPK-RKK oleh Direktur Utama
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/spk-rkk/3a4b5c6d-7e8f-9012-3456-789012345678/sign-dirut`
* **Response Body (`200 OK`)**:
```json
{
  "id": "3a4b5c6d-7e8f-9012-3456-789012345678",
  "workforceProfileId": "b1a2c3d4-e5f6-7890-abcd-ef1234567890",
  "employeeName": "dr. Kevin Pratama, Sp.JP",
  "employeeCode": "DOC-2026-0012",
  "profession": "Dokter Spesialis Jantung & Pembuluh Darah",
  "committeeName": "Komite Medik",
  "spkNumber": "SPK/RSQ-DIRUT/XI/2026/0001",
  "rkkNumber": "RKK/RSQ-DIRUT/XI/2026/0001",
  "issuedAt": "2026-11-09T12:00:00Z",
  "validFrom": "2026-11-09T00:00:00Z",
  "validUntil": "2029-11-09T00:00:00Z",
  "clinicalPrivilegeType": "Regular",
  "directorGeneralName": "Prof. Dr. dr. Hendra Wijaya, Sp.B-KBD",
  "isSignedByDirectorGeneral": true,
  "signedAt": "2026-11-09T12:35:00Z",
  "verificationHash": "d8e8fca2dc6b34e5a9f012b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3",
  "spkPdfUrl": "/storage/spk-rkk/3a4b5c6d-7e8f-9012-3456-789012345678_SPK.pdf",
  "qrCodeUrl": "/storage/qr-codes/d8e8fca2dc6b34e5a9f012b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3.png",
  "eDossierDocumentId": "9f8e7d6c-5b4a-3210-fedc-ba9876543210",
  "issuanceStatus": "Active",
  "approvedPrivileges": [
    {
      "catalogId": "c1d2e3f4-a5b6-7890-1234-567890abcdef",
      "recommendedSupervisionLevel": 1,
      "reviewerNotes": "Mandiri penuh berdasarkan bukti 120 kasus PCI mandiri di logbook."
    },
    {
      "catalogId": "d2e3f4a5-b6c7-8901-2345-67890abcdef1",
      "recommendedSupervisionLevel": 2,
      "reviewerNotes": "Di bawah supervisi dokter subspesialis intervensi senior."
    }
  ]
}
```

---

## 5. Verifikasi Kompilasi & Kesiapan Produksi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Integrasi Dependency Injection**: Seluruh service (`ICredentialingCommitteeService`, `ICredentialingWorkflowService`, `ISpkRkkIssuanceService`) telah terdaftar resmi di `Program.cs` dan siap melayani request API secara penuh.
