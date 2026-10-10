# Laporan Perubahan Backend — `BE-RWI-204`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-204` |
| Judul | Validasi Nomor Telepon Kontak Darurat Maksimal 13 Digit dan Pengikatan Default Service Unit Rawat Inap |
| Slice | Slice 1 (Fondasi Kontrak & Validasi); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/backend-roadmap-admisi-pendaftaran.md`](../../../roadmap/backend-roadmap-admisi-pendaftaran.md) — kartu `BE-RWI-204` |
| Trace | `FR-RI-179`, `FR-RI-183`; `RWI-DEC-267`, `RWI-DEC-268`; `RWI-AC-388`, `RWI-AC-389`; Validation `VAL-ADM-01`, `VAL-ADM-02`; API Contract `0.11.0` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | — (Task fondasi mandiri) |
| Klasifikasi | `MEDIUM` — Validasi DTO kontak darurat, penegakan numeric digit, integrasi service unit ranap |
| Task mode | `BACKEND` |
| Target tulis | `PatientEmergencyContactDtos.cs`, `PatientEmergencyContactController.cs`, `PatientEncounterController.cs` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-388` dan `RWI-AC-389` terimplementasi bersih dan tervalidasi. |

---

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` (MasterData Kontak Darurat) & `RegistrationManagement` (PatientEncounter) |
| Prefix registry | `Mst` & `Reg` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-VAL-001`, `QBE-SVC-001`, `QBE-API-001` |
| Wewenang | Source: ya. Tidak ada perubahan struktur skema tabel database (hanya aturan validasi dan DTO). |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada alur pendaftaran admisi rawat inap:
1. **Nomor HP Kontak Darurat (`RWI-DEC-267`, `RWI-AC-388`):** Nomor telepon darurat keluarga/penanggung jawab pasien wajib dibatasi maksimal 13 karakter numerik dan hanya boleh memuat angka (standar format nomor seluler Indonesia). Format > 13 digit atau karakter non-angka ditolak untuk menjaga konsistensi integrasi gateway notifikasi (SMS/WhatsApp).
2. **Penyembunyian & Pengikatan Default Service Unit Rawat Inap (`RWI-DEC-268`, `RWI-AC-389`):** Dropdown pilihan unit layanan rawat inap di antarmuka langkah Dokter disembunyikan agar petugas tidak bingung memilih. Backend memverifikasi bahwa kunjungan bertipe rawat inap mengikat unit layanan rawat inap yang sah (`ServiceUnitType.Inpatient`), serta mampu menyelesaikan default unit rawat inap secara otomatis bila tidak dikirimkan.

---

## 2. Rincian Perubahan Source Code

### 2.1 `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientEmergencyContactDtos.cs`
- Memperketat validasi atribut pada property `PhoneNumber` di dalam `CreatePatientEmergencyContactRequest`:
  - `[MaxLength(13, ErrorMessage = "Nomor telepon kontak darurat maksimal 13 karakter numerik.")]`
  - `[RegularExpression(@"^[0-9]+$", ErrorMessage = "Nomor telepon kontak darurat hanya boleh berisi angka.")]`
- Karena `UpdatePatientEmergencyContactRequest` mewarisi `CreatePatientEmergencyContactRequest`, perubahan ini otomatis berlaku untuk pembuatan maupun pengubahan kontak darurat.

### 2.2 `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientEmergencyContactController.cs`
- Pada method `ValidateRequestAsync(Guid? excludeId, CreatePatientEmergencyContactRequest request)`:
  - Menambahkan pengecekan: jika `PhoneNumber` tidak kosong, periksa bahwa seluruh karakter adalah digit (`char.IsDigit`). Jika ada karakter non-digit, kembalikan pesan gagal `"Nomor telepon kontak darurat hanya boleh berisi angka."`
  - Memeriksa panjang string angka `cleanPhone.Length > 13`. Jika melebihi 13 digit, kembalikan pesan gagal `"Nomor telepon kontak darurat maksimal 13 karakter numerik."`

### 2.3 `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`
- Pada method `ValidateCreateRequestAsync`:
  - Jika `request.ServiceUnitId == Guid.Empty` dan `request.EncounterType == EncounterType.Inpatient`, backend otomatis menyelesaikan `ServiceUnitId` ke unit rawat inap default yang aktif dan tersedia untuk registrasi (`ServiceUnitType.Inpatient`).
  - Menambahkan validasi penegakan tipe unit: jika `request.EncounterType == EncounterType.Inpatient`, backend memverifikasi bahwa `ServiceUnitId` yang terikat benar-benar bertipe rawat inap (`ServiceUnitType.Inpatient`). Jika bukan unit rawat inap, transaksi ditolak dengan pesan `"Unit tujuan untuk kunjungan rawat inap wajib merupakan unit layanan bertipe rawat inap."`

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-388` | Input No. HP Kontak Darurat membatasi panjang maksimal 13 karakter numerik dan menolak karakter alfabet/simbol | ✅ LULUS | 1. DTO dilengkapi `[MaxLength(13)]` dan `[RegularExpression(@"^[0-9]+$")]`.<br/>2. `ValidateRequestAsync` menolak input seperti `08123456789012` (14 digit) dengan pesan validasi baku.<br/>3. Input `0812-3456-ABCD` ditolak karena memuat karakter non-digit.<br/>4. Input `081234567890` (12 digit angka) dan `0812345678901` (13 digit angka) diterima valid. |
| `RWI-AC-389` | Payload encounter pendaftaran rawat inap otomatis mengikat ServiceUnit rawat inap yang valid | ✅ LULUS | 1. `ValidateCreateRequestAsync` otomatis mencari dan mengikat `MstServiceUnit` bertipe `ServiceUnitType.Inpatient` bila `ServiceUnitId` kosong pada encounter rawat inap.<br/>2. Penegakan tipe unit memastikan tidak ada unit poliklinik atau IGD yang terikat salah sasaran pada admisi ranap. |

---

## 4. Status Database dan Migration
- **Perubahan Skema:** Tidak ada penambahan tabel atau kolom baru; perubahan murni pada lapisan validasi DTO, sanitasi string, dan logika verifikasi tipe unit.
- **Migration:** Tidak memerlukan migration database baru.

---

## 5. Handoff ke Task Berikutnya
- Task backend `BE-RWI-204` selesai dan siap membuka:
  - Lapisan frontend: **`FE-RWI-222`** (Input masking 13 digit & penyembunyian dropdown unit tujuan).
  - Lapisan backend: **`BE-RWI-205`** (Endpoint & Persistence formulir persetujuan rawat inap & TTD digital).
