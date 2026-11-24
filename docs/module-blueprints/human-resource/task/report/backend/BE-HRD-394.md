# Laporan Perubahan Backend — `BE-HRD-394`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-394` |
| Judul | Lifecycle pendidikan, berkas ijazah, dan satu pendidikan tertinggi |
| Slice | `S-FIN-M04` / Pendidikan (alias `FIN-HRD-04-BE-008`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-394` |
| Trace | Instruksi Menu 04 `FIN-HRD-04-BE-008`; `UAT-04-18` s.d. `23` |
| Contract version | Kontrak pendidikan Workforce Core existing |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpEducationController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-TXN-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Penanda "pendidikan tertinggi" harus tepat satu per pegawai, karena dipakai untuk persyaratan jabatan dan kredensial. Pelepasan penanda lain (`UnsetOtherHighestAsync`), validasi berkas (≤ 10 MB, PDF/JPG/JPEG/PNG), dan larangan verifikasi oleh pembuat sudah ada. Celah:

1. **Dua permintaan serentak** "jadikan pendidikan tertinggi" (mis. S1 dan Profesi Ners pada saat yang sama) sama-sama melepas penanda yang lain lalu menandai miliknya — hasilnya dua pendidikan tertinggi.
2. Create memanggil pelepasan dua kali berturut-turut (tidak berbahaya, tetapi membingungkan pembaca kode).
3. Berkas ijazah terbuka lewat URL statis tanpa login (diperbaiki bersama `BE-HRD-397`).

## 2. Proses bisnis

1. Petugas menambah/mengubah pendidikan atau menekan "jadikan tertinggi".
2. Di dalam transaksi, server mengambil kunci per pegawai, melepas penanda tertinggi pendidikan lain, lalu menandai pendidikan ini. Permintaan kedua menunggu dan melihat hasil yang pertama.
3. Berkas divalidasi sebelum disimpan; mengganti berkas mencabut status terverifikasi.
4. Verifikasi oleh petugas selain pembuat (`PATCH /verify`; pembuat `403 SELF_VERIFICATION_PROHIBITED`).
5. Berkas dibuka lewat `GET /{id}/file` (hak `WorkforceEducation : Read`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpEducationController.cs` (create, update, highest, verify, file, delete), `Program.cs` (penyajian `/uploads`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpEducationController.cs` | Kunci per pegawai di `UnsetOtherHighestAsync`; pemanggilan ganda dihapus; `FileUrl` menunjuk endpoint unduh terlindungi; cek profil tidak mensyaratkan profil aktif (`BE-HRD-401`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; `FileUrl` kini `/api/v1/.../educations/{id}/file` (relatif, diterjemahkan frontend ke origin API) |
| Database | Tanpa migration |
| Keamanan/Auth | Berkas ijazah tidak lagi terbuka tanpa login |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Education

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/educations/{id}/highest` | Tetapkan pendidikan tertinggi (tepat satu) | `WorkforceEducation : Update` |
| `PATCH` | `…/educations/{id}/verify` | Verifikasi oleh selain pembuat | `WorkforceEducation : Update` |
| `GET` | `…/educations/{id}/file` | Unduh berkas ijazah | `WorkforceEducation : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: berkas 12 MB | `400` "maksimal 10 MB" | `PASS` (penelusuran) | `ValidateFile` |
| Penelusuran: berkas `.docx` | `400` format | `PASS` (penelusuran) | `ValidateFile` |
| Penelusuran: dua "jadikan tertinggi" serentak | Diantrekan kunci per pegawai; satu tertinggi | `PASS` (penelusuran) | `UnsetOtherHighestAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `UnsetOtherHighestAsync` dipanggil saat create/update | Terpenuhi | Create, update, dan `PATCH /highest`, kini dengan kunci per pegawai |
| (2) Verifikasi oleh pembuat ditolak | Terpenuhi | `VerifyEducation` |
| (3) Berkas di luar batas ditolak | Terpenuhi | `ValidateFile` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-397` |
