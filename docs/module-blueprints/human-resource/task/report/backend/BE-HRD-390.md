# Laporan Perubahan Backend — `BE-HRD-390`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-390` |
| Judul | Workflow Menu 06 untuk keluarga dan tanggungan dengan dual actor |
| Slice | `S-FIN-M04` / Profil/Keluarga (alias `FIN-HRD-04-BE-004`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-390` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-004`; `UAT-04-09` s.d. `11`, `14` |
| Contract version | Kontrak Pengajuan Perubahan Data (`BE-HRD-028`); tidak ada endpoint baru |
| Dependency | `BE-HRD-389` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/EmployeeProfileChangeService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-TXN-001, QBE-LOG-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Field keluarga (`WfpFamilyMember`: hubungan, nama, nomor identitas, status perkawinan) dan tanggungan (`WfpDependent`: tanggungan pajak, kelayakan benefit dan asuransi) sudah terdaftar di kamus field Menu 06, dan larangan pemohon menjadi verifikator sudah ada. Audit menemukan dua celah:

1. **Record target tidak diperiksa pemiliknya.** Saat pengajuan disusun, divalidasi, dan diterapkan, record anak profil dicari hanya berdasarkan Id. Pengajuan atas nama pegawai A dapat menunjuk Id rekening atau tanggungan milik pegawai B, dan saat diterapkan yang berubah adalah data B.
2. **Apply belum tepat sekali.** Status `Approved` dibaca sebelum transaksi; dua klik "Terapkan" yang bersamaan sama-sama lolos dan menerapkan dua kali.

## 2. Proses bisnis

1. Pegawai atau HR (atas nama) menyusun pengajuan dengan bukti → `Submitted`.
2. Petugas **selain** pemohon/pembuat memulai verifikasi dan memutuskan per detail (dua penjaga lama: `StartVerificationAsync`, `DecideVerificationAsync`; juga `ApproveAsync`).
3. Setiap detail yang menunjuk record keluarga, tanggungan, rekening, alamat, atau kontak darurat diperiksa: record harus milik profil pegawai pada pengajuan itu dan belum dihapus; bila tidak → `400`/`409`.
4. Apply: di dalam transaksi diambil kunci per pengajuan, status dibaca ulang; bila sudah bukan `Approved` → `409`. Semua nilai diterapkan dan status menjadi `Applied` dalam satu commit.

**Contoh.** HR menyusun pengajuan untuk perawat A dengan `TargetEntityId` = Id tanggungan milik perawat B → ditolak `400` "Data tanggungan yang dirujuk bukan milik pegawai pada permintaan ini." Dua petugas menekan "Terapkan" bersamaan → satu `200`, satu `409`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EmployeeProfileChangeService.cs` (kamus field, `ResolveTarget`, tiga pemanggil, `StartVerificationAsync`, `DecideVerificationAsync`, `ApproveAsync`, `ApplyAsync`), `EmployeeProfileChangeController.cs`, model `WfpFamilyMember`, `WfpDependent`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Services/EmployeeProfileChangeService.cs` | `GuardTargetOwnership` pada lima jenis record anak di `ResolveTarget`; kunci `pg_advisory_xact_lock` per pengajuan dan baca ulang status di `ApplyAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; respons baru `400`/`409` untuk target milik profil lain atau terhapus, dan `409` untuk apply ganda |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup IDOR lintas pegawai pada jalur perubahan data |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Employee Profile Change Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/employee-profile-changes/{id}/apply` | Menerapkan pengajuan yang disetujui, tepat sekali | `EmployeeProfileChange : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: target milik profil lain | `400` sebelum detail tersimpan maupun saat apply | `PASS` (penelusuran) | `GuardTargetOwnership` di tiga pemanggil `ResolveTarget` |
| Penelusuran: pemohon memverifikasi sendiri | `403` | `PASS` (penelusuran) | penjaga lama di tiga aksi |
| Penelusuran: apply ganda serentak | Kedua menunggu kunci, baca ulang `Applied`, `409` | `PASS` (penelusuran) | `ApplyAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Definisi field keluarga dan tanggungan tersedia | Terpenuhi | `AllowedFields` (`WfpFamilyMember` 4 field, `WfpDependent` 3 field) |
| (2) Creator ≠ verifier ditegakkan | Terpenuhi | Penjaga `RequestedByUserId`/`CreateBy` pada mulai verifikasi, putusan, dan persetujuan |
| (3) Apply sekali dan atomik | Terpenuhi | Satu transaksi + kunci per pengajuan + baca ulang status |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar scope | `Apply` memakai `[AccessAction("Apply")]` tetapi `[AccessPermission("EmployeeProfileChange", "Update")]`: centang "Apply" di Akses Role tidak berpengaruh, yang ditegakkan hak `Update`. Tidak diubah karena mengubahnya mencabut akses peran yang kini mengandalkan `Update`; perlu keputusan pemilik |
| Masalah yang diketahui | Tanggungan baru tidak dapat dibuat lewat Menu 06 karena `FamilyMemberId` tidak ada di kamus field; pembuatan tetap lewat Data Induk lalu flag diubah lewat Menu 06 |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-391`, `BE-HRD-392` |
