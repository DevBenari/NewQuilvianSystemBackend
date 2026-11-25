# Laporan Perubahan Backend — `BE-HRD-392`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-392` |
| Judul | Kunci field keluarga yang berdampak benefit dan pajak |
| Slice | `S-FIN-M04` / Keluarga (alias `FIN-HRD-04-BE-006`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-392` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-006`; `UAT-04-13` |
| Contract version | Kontrak anggota keluarga Workforce Core existing |
| Dependency | `BE-HRD-390` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpFamilyMemberController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-LOG-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Hubungan keluarga, nomor identitas, dan status perkawinan anggota keluarga memengaruhi pajak (PTKP), BPJS, dan benefit. Guard `409` untuk ketiganya sudah ada pada `PUT`. Audit menemukan satu celah: **jenis identitas** anggota keluarga tetap bisa diganti (mis. KTP menjadi KK dengan nomor sama), sehingga data identitas berubah tanpa Menu 06.

## 2. Proses bisnis

1. Petugas mengubah data anggota keluarga lewat `PUT`.
2. Hubungan, nomor identitas, jenis identitas (bila nomor sudah ada), atau status perkawinan berubah → `409 SENSITIVE_FAMILY_MEMBER_LOCKED`, arahan ke Menu 06.
3. Kontak, pekerjaan, penanda kontak darurat, dan keterangan tetap boleh diubah langsung; `UpdateBy`/`UpdateDateTime` dicatat.
4. Anggota keluarga yang masih menjadi tanggungan tidak dapat dihapus (`400`, penjaga lama).

**Jalur tidak normal.** Pengisian pertama (nilai lama kosong) diperbolehkan; mengosongkan nilai yang sudah ada dianggap perubahan → `409`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpFamilyMemberController.cs` (`PUT`, `PATCH status`, `PATCH emergency-contact`, `DELETE`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpFamilyMemberController.cs` | Jenis identitas ikut dikunci bila nomor identitas sudah ada; cek profil tidak mensyaratkan profil aktif (`BE-HRD-401`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; perubahan jenis identitas kini `409` |
| Database | Tanpa migration |
| Keamanan/Auth | Tidak berubah |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Family Member

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/family-members/{id}` | Ubah data non-sensitif; field sensitif `409` | `WfpFamilyMember : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: hubungan `Anak` → `Istri` | `409` | `PASS` (penelusuran) | guard lama |
| Penelusuran: jenis identitas `KTP` → `KK`, nomor sama | `409` (dulu lolos) | `PASS` (penelusuran) | `isIdentityTypeChanged` |
| Penelusuran: ubah nomor telepon | `200`, `UpdateBy` tercatat | `PASS` (penelusuran) | `Update` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Field relasi/identitas ditolak `409` | Terpenuhi | Guard hubungan, nomor identitas, jenis identitas, status perkawinan |
| (2) Perubahan kontak non-sensitif tercatat `UpdateBy` | Terpenuhi | `entity.UpdateBy = GetCurrentUserId()` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Nama lengkap anggota keluarga tidak dikunci (tidak termasuk daftar field sensitif kartu) |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-393` |
