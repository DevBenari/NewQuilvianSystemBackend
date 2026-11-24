# Laporan Perubahan Backend — `BE-HRD-393`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-393` |
| Judul | Tanggungan pajak, benefit, dan asuransi hanya berubah lewat apply sah |
| Slice | `S-FIN-M04` / Keluarga (alias `FIN-HRD-04-BE-007`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-393` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-007`; `UAT-04-13` s.d. `17` |
| Contract version | Kontrak tanggungan Workforce Core existing |
| Dependency | `BE-HRD-392` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpDependentController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Flag `IsTaxDependent`, `IsBenefitEligible`, `IsInsuranceEligible` sudah ditolak `409` bila diubah lewat `PUT`, dan `FamilyMemberId` sudah divalidasi milik profil yang sama. Celah tersisa: penjaga relasi hanya berjalan bila request **mengisi** `FamilyMemberId`. Mengirim `FamilyMemberId` kosong memutus tautan tanggungan ke anggota keluarganya tanpa Menu 06 — misalnya anak yang terdaftar sebagai tanggungan asuransi menjadi "tanggungan tanpa orang".

## 2. Proses bisnis

1. Tanggungan dibuat lewat Data Induk dengan `FamilyMemberId` milik profil yang sama (anggota keluarga aktif) atau `BenefitPlanId`.
2. `PUT` yang mengubah salah satu flag kelayakan atau mengubah/mengosongkan `FamilyMemberId` → `409 SENSITIVE_DEPENDENT_LOCKED`, arahan ke Menu 06.
3. Perubahan flag lewat Menu 06 diterapkan sesudah verifikasi aktor berbeda (`BE-HRD-390`), dengan target yang dipastikan milik profil yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpDependentController.cs` (create, `PUT`, `PATCH status`, `DELETE`, `ValidateRequestAsync`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpDependentController.cs` | Perubahan relasi termasuk pengosongan `FamilyMemberId` dikunci; cek profil tidak mensyaratkan profil aktif (`BE-HRD-401`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `PUT` dengan `FamilyMemberId` kosong pada tanggungan bertaut kini `409` |
| Database | Tanpa migration |
| Keamanan/Auth | Tidak berubah |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Dependent

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/dependents/{id}` | Ubah data non-sensitif tanggungan | `WorkforceDependent : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: `IsTaxDependent` false → true via `PUT` | `409` | `PASS` (penelusuran) | guard lama |
| Penelusuran: `FamilyMemberId` dikosongkan via `PUT` | `409` (dulu lolos) | `PASS` (penelusuran) | guard baru |
| Penelusuran: `FamilyMemberId` milik profil lain saat create | `400` | `PASS` (penelusuran) | `ValidateRequestAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) PUT flag kelayakan ditolak `409` | Terpenuhi | `isSensitiveChanged` |
| (2) `FamilyMemberId` divalidasi | Terpenuhi | Milik profil yang sama, aktif, tidak terhapus; perubahan/pengosongan dikunci |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Pembuatan tanggungan baru dengan flag pajak, penghentian lewat `PATCH status`, dan penghapusan tetap langsung (siklus hidup, di luar kriteria kartu); perlu keputusan pemilik bila ingin lewat Menu 06 |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-401` |
