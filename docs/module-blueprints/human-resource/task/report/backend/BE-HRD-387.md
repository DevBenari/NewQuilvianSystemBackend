# Laporan Perubahan Backend — `BE-HRD-387`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-387` |
| Judul | Resolver identitas resmi lintas Pegawai, Dokter, dan Pengguna Eksternal |
| Slice | `S-FIN-M04` / Profil (alias `FIN-HRD-04-BE-001`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-387` |
| Trace | `HRD-DEC-051`; instruksi Menu 04 `FIN-HRD-04-BE-001`; `UAT-04-02` s.d. `07`, `38` |
| Contract version | Kontrak Data Induk existing (`BE-HRD-003`, `BE-HRD-026`); tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `MEDIUM` — service bersama + tiga controller master, tanpa skema baru |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: "kerjakan seluruh task … ikuti semua rekomendasi") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/HumanResource/MasterData/Workforce/**`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), perubahan belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API DB dev `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` |
| Pemilik / prefix registry | HR, prefix `Mst`/`Wfp` (terdaftar) |
| Keberlakuan | `TOUCHED LEGACY` (resolver dan tiga controller master) |
| QBE yang berlaku | QBE-CODE-001, QBE-CODE-003, QBE-CODE-004, QBE-CODE-006, QBE-VAL-001, QBE-TXN-001, QBE-SVC-001 (catatan legacy: controller master masih memakai `DbContext` langsung, tidak di-refactor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Satu orang di rumah sakit bisa punya beberapa peran: dr. A adalah dokter spesialis sekaligus pegawai struktural. Aturan `HRD-DEC-051` meminta orang itu tetap **satu profil** (`MstWorkforceProfile`). Sebelum perubahan ada tiga celah:

1. **Kode profil dari hitung jumlah data + 1.** Dua petugas yang menyimpan data bersamaan bisa mendapat nomor `WFP-RSMMC-000123` yang sama; penyimpanan kedua gagal dengan galat 500.
2. **Tidak ada penjaga balapan identitas.** Bagian SDM membuat data Pegawai dr. A dan Komite Medik membuat data Dokter dr. A pada detik yang sama. Keduanya belum melihat data satu sama lain, sehingga lahir dua profil untuk satu NIK.
3. **Paspor WNA tidak dapat dipakai Pegawai dan nomornya dipotong.** Validasi Pegawai memaksa 16 digit untuk semua jenis identitas, dan nomor disimpan dengan membuang huruf (`A1234567` menjadi `1234567`). Pencocokan lintas peran pun gagal karena resolver membandingkan `A1234567`.

## 2. Proses bisnis

1. Petugas membuat Pegawai, Dokter, atau Pengguna Eksternal dengan jenis dan nomor identitas.
2. Nomor dinormalisasi: NIK hanya digit (spasi/titik/tanda hubung pemisah boleh diketik), paspor huruf besar tanpa spasi/tanda hubung.
3. Format divalidasi: NIK tepat 16 digit; paspor 6–16 huruf/angka (sama dengan aturan frontend `input-normalizer-utils`). Salah format → `400` tanpa data tersimpan.
4. Di dalam transaksi pembuatan peran, resolver mengambil kunci per nomor identitas (`pg_advisory_xact_lock`). Permintaan lain untuk nomor yang sama menunggu.
5. Resolver mencari peran aktif dengan nomor itu di tiga tabel:
   - ditemukan dengan peran berbeda → peran baru **ditautkan** ke profil yang sama (jumlah profil bertambah 0);
   - ditemukan dengan peran sama → `409 DUPLICATE_IDENTITY_NUMBER`;
   - tidak ditemukan → profil baru dengan kode dari alokator nomor bersama.
6. Kunci dilepas saat transaksi selesai.

**Contoh berangka balapan.** Pukul 09.00:00 SDM menyimpan Pegawai dr. A (NIK `3201…0001`), Komite Medik menyimpan Dokter dr. A pada saat yang sama. Permintaan SDM mendapat kunci dan membuat profil `WFP-RSMMC-000124`. Permintaan Komite Medik menunggu ±50 ms, lalu menemukan Pegawai dr. A yang baru di-commit dan menautkan Dokter ke `WFP-RSMMC-000124`. Hasil: satu profil, dua peran.

**Jalur tidak normal.** Dokter atau Pengguna Eksternal boleh tanpa nomor identitas (aturan lama dipertahankan): profil baru tetap dibuat tetapi tidak dapat dicocokkan; sebelumnya kasus ini justru galat 500 (resolver melempar `ArgumentException`), dan Pengguna Eksternal memakai GUID acak sebagai "nomor identitas". Data ganda historis tidak digabung otomatis (`HRD-Q-67`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkforceIdentityResolverService.cs`, `IWorkforceIdentityResolverService.cs`, `EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs`, `MstWorkforceProfile(Configuration).cs`, konfigurasi `MstEmployee`/`MstDoctor`/`MstExternalUser` (index unik `IdentityNumber`, `WorkforceProfileId`), `NumberSeriesAllocator.cs`, `AllowancePolicyCodeService.cs` (pola batas bawah deret lama), `HiringHandoffService.cs`, frontend `utils/shared/input-normalizer-utils.jsx` (aturan paspor).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Workforce/Services/WorkforceIdentityResolverService.cs` | Validasi NIK/paspor diperketat; kunci per identitas di dalam transaksi pemanggil; wajib transaksi (`IDENTITY_RESOLUTION_REQUIRES_TRANSACTION`); identitas kosong membuat profil baru tanpa galat; kode profil dari `NumberSeriesAllocator` (`MST_WORKFORCE_PROFILE`, `WFP-RSMMC`, 6 digit, batas bawah = kode lama tertinggi) |
| `MasterData/Workforce/Services/IWorkforceIdentityResolverService.cs` | Tambah `IsPassportType` |
| `EmployeeController.cs` | Nomor identitas disimpan dalam bentuk normal resolver; validasi 16 digit untuk semua jenis dihapus (diganti `IsValidIdentity`); uji keunikan memakai normalisasi resolver; metode mati `GenerateWorkforceProfileCodeAsync` (`Count+1`) dihapus |
| `DoctorController.cs`, `ExternalUserController.cs` | Sama; Pengguna Eksternal tidak lagi memakai GUID acak sebagai nomor identitas; pengisian identitas pertama kali (data lama tanpa NIK) wajib valid dan tidak boleh milik profil lain (`409 IDENTITY_BELONGS_TO_OTHER_PROFILE`); metode mati pembangkit kode dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route dan DTO tetap. Perilaku baru: Pegawai berpaspor diterima; kode galat baru `IDENTITY_BELONGS_TO_OTHER_PROFILE` (409) pada update Dokter/Eksternal yang mengisi identitas pertama kali |
| Database | Tanpa migration. Deret `MST_WORKFORCE_PROFILE` tercipta otomatis di `NumNumberSeries` saat alokasi pertama. Index unik `ProfileCode` sudah ada |
| Keamanan/Auth | Hak akses tidak berubah (`Employee/Doctor/ExternalUser : Create`). NIK tidak ditulis ke log |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru; perilaku berubah pada:

#### Corporate / Human Resource / Master Data / Employee

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/master-data/employees` | Membuat Pegawai dan menautkan profil yang sudah ada | `Employee : Create` |

(Padanan untuk Dokter `Doctor : Create` dan Pengguna Eksternal `ExternalUser : Create`.)

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratch>` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` atas berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran source: NIK `3201-0123-4567-8901` (dengan tanda hubung) | Dinormalisasi `3201012345678901`, lolos 16 digit | `PASS` | `NormalizeIdentityNumber`, `IsValidIdentity` |
| Penelusuran source: NIK `32010123456789AB` | Ditolak `400` (huruf tidak dibuang diam-diam) | `PASS` | `IsValidIdentity` (pola digit) |
| Penelusuran source: paspor `a 1234567` jenis `Paspor` | Disimpan `A1234567`, lolos 6–16 | `PASS` | resolver + `EmployeeController.CreateEmployee` |
| Penelusuran source: dua create serentak identitas sama | Permintaan kedua menunggu kunci, lalu menaut/409; satu profil | `PASS` (penelusuran) | `AcquireIdentityLockAsync` di transaksi controller |
| Uji API serentak di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan; backend lokal pengguna sedang berjalan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — butuh wewenang tulis DB dev.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) NIK/paspor dinormalisasi dan divalidasi; format salah ditolak `400` | Terpenuhi | `IsValidIdentity` dipanggil sebelum transaksi di tiga controller |
| (2) Orang yang sudah ada ditautkan, bukan dibuatkan profil baru | Terpenuhi | Cabang `match.Found` → `IsExistingLinked`; nomor tersimpan kini sama bentuknya dengan nomor yang dicari |
| (3) Dua permintaan serentak dengan identitas sama tidak menghasilkan dua profil atau kode kembar | Terpenuhi (source) | Kunci per identitas dalam transaksi + alokator atomik + index unik `ProfileCode`; bukti runtime `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kunci identitas hanya aktif di PostgreSQL (sama seperti alokator nomor) |
| Masalah yang diketahui | (a) Data lama Dokter/Eksternal yang tersimpan dengan spasi tidak cocok sampai dinormalisasi; (b) rehire sesudah data Pegawai dihapus (soft delete) membuat profil baru karena index unik `MstEmployee.WorkforceProfileId` tidak difilter — menunggu prosedur `HRD-Q-67`; (c) temuan di luar Menu 04: `HiringHandoffService` (`S-D2`) masih membuat kode profil `WFP-yyyyMMdd-NNNN` dan nomor `HND-` dari `Count+1` |
| Risiko tersisa | Bukti balapan baru penelusuran source |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-HRD-402` (status gabungan slice) |
| Langkah berikutnya | Uji API serentak di DB dev bila wewenang diberikan |
