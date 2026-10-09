# ISSUE-005 — Temuan Pengujian Runtime Workspace PPRI: Skema Penjamin Out-of-Sync, Konflik Multi-Site, Gagal Simpan Tanda Tangan EF Core, dan Ketiadaan Kebijakan Hak Akses

```yaml
issue_id: ISSUE-EPS-005
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Workspace PPRI (Penerimaan Pasien Rawat Inap) — API & Dokumen Admisi (BE-RWI-185 s.d. BE-RWI-202)"
sumber_laporan: "Hasil eksekusi verifikasi runtime backend pada 8 Oktober 2026 (laporan: task/report/backend/verifikasi-runtime-workspace-ppri-2026-10-08.md)"
tanggal_issue: "2026-10-09"
status: SEDANG_DIVERIFIKASI
keparahan_tertinggi: High
source_sha_backend: "fdf85a07"
source_sha_frontend: null
rencana_perbaikan: ../plan-repair/plan-repair-005-perbaikan-runtime-workspace-ppri.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Laporan ini merangkum empat temuan teknis dan operasional yang teridentifikasi selama pelaksanaan pengujian runtime menyeluruh terhadap 28 endpoint Workspace PPRI (Penerimaan Pasien Rawat Inap) pada tanggal 8 Oktober 2026. Pengujian dilakukan pada lingkungan *Development* (`https://localhost:7184`, basis data PostgreSQL `QuilvianNewDevHamzah`) dengan migrasi acuan `20261008055604_AddWorkspacePpriAdmissionDocuments`.

Meskipun secara konseptual seluruh logika bisnis dan matriks validasi (seperti pembatasan 5 butir kepercayaan, aturan shortfall deposit, isolasi dokumen pemulangan vs admisi, dan keamanan nominal rupiah) berjalan sesuai spesifikasi, terdapat kendala runtime pada lapisan basis data, data master, pelacakan entitas EF Core, dan konfigurasi wewenang yang menghambat alur kerja end-to-end:
1. **Kegagalan Pembacaan Penjamin Asuransi**: Kolom `CardImagePath` pada tabel `MstPatientCompanyGuarantor` belum tersedia di basis data, menyebabkan `EncounterInsuranceService` melempar `PostgresException: 42703`. Dampaknya, status dokumen selisih biaya menjadi *Uncountable* dan kelengkapan dokumen admisi dilaporkan tidak dapat dihitung.
2. **Kegagalan Penguncian Dokumen Admisi**: Master data memiliki 3 situs rumah sakit utama aktif (`IsMainSite = true`), sehingga sistem menolak menebak profil RS dan membatalkan penguncian dokumen dengan status `422 INP-ADM-DOC-027`.
3. **Kegagalan Penyimpanan Tanda Tangan**: Penambahan objek tanda tangan baru pada dokumen yang sudah dilacak EF Core memicu `DbUpdateConcurrencyException` karena EF Core memperlakukannya sebagai entitas ubahan (`Modified`) alih-alih entitas baru (`Added`). Hal ini menyebabkan penandatanganan dokumen admisi selalu ditolak dengan status keliru `409 INP-ADM-DOC-004`, menghambat transisi dokumen ke status `Completed`.
4. **Ketiadaan Kebijakan Hak Akses Staf**: Tabel `SysAccessPolicy` tidak memuat pemetaan wewenang untuk controller `InpatientAdmissionDocument`, sehingga seluruh staf non-superadmin selalu ditolak dengan status `403 Forbidden`.

---

## 2. Laporan Asli & Sumber Bukti

Sumber bukti berasal dari dokumen resmi laporan verifikasi runtime:
- **Dokumen Laporan**: `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/verifikasi-runtime-workspace-ppri-2026-10-08.md`
- **Hasil Pengujian**: 42 skenario diuji (35 PASS, 3 FAIL, 4 NOT RUN).

---

## 3. Ringkasan Temuan

| ID | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | Perbaikan |
| --- | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-005-01` | Skema database out-of-sync: Kolom `CardImagePath` belum ada di tabel `MstPatientCompanyGuarantor` | `SCHEMA_OUT_OF_SYNC` | Database | High | SUDAH-VERIFIKASI | `FIX-EPS-005-01` |
| `ISS-EPS-005-02` | Konflik master data: Lebih dari satu situs utama aktif menggagalkan penguncian dokumen admisi | `DATA_ANOMALY` | Master Data | High | SUDAH-VERIFIKASI | `FIX-EPS-005-02` |
| `ISS-EPS-005-03` | Bug penanganan EF Core: `DbUpdateConcurrencyException` saat menyimpan tanda tangan dokumen admisi | `BUG` | Backend Service | High | SUDAH-VERIFIKASI | `FIX-EPS-005-03` |
| `ISS-EPS-005-04` | Gap konfigurasi keamanan: Tabel `SysAccessPolicy` belum memetakan hak akses controller admisi | `CONFIG_GAP` | Auth & Seeder | Medium | SUDAH-VERIFIKASI | `FIX-EPS-005-04` |

---

## 4. Rincian per Temuan

### ISS-EPS-005-01 — Skema database out-of-sync pada `MstPatientCompanyGuarantor`

- **Gejala**: Pemanggilan `GET .../admission-workspace/summary` pada Episode A (pasien asuransi BPJS) mengembalikan status 200 OK namun dengan status penjamin `Failed`, badge `CostDifferenceStatement: Uncountable`, dan pesan peringatan *"Kelengkapan dokumen admisi tidak dapat dihitung"*.
- **Log Backend**:
  ```text
  Npgsql.PostgresException (0x80004005): 42703: column m3.CardImagePath does not exist
  at EncounterInsuranceService.GetContextAsync(...)
  ```
- **Akar Masalah**: Berkas migrasi `20261006092040_AddCardImagePathToPatientCompanyGuarantor.cs` sudah ada pada folder migrasi backend, tetapi DDL penambahan kolom belum dieksekusi pada database PostgreSQL target.

---

### ISS-EPS-005-02 — Konflik multi-site pada `MstHospitalSite` menggagalkan penguncian dokumen admisi

- **Gejala**: Penguncian dokumen Permintaan Privasi atau dokumen admisi lainnya (`PATCH .../documents/{id}/lock`) ditolak dengan HTTP `422 Unprocessable Entity` dan kode error `INP-ADM-DOC-027` (*"Data pasien atau profil rumah sakit tidak dapat dimuat"*).
- **Akar Masalah**: 
  Tabel master `MstHospitalSite` memuat tiga baris dengan `IsMainSite = true` (`SITE-MMC-001`, `SITE-MDC-001`, `SITE-MHS-001`). Sesuai aturan AC BE-RWI-188 butir 2, sistem menolak menebak situs jika terdapat lebih dari satu situs utama aktif (`InpAdmissionSourceReader.GetHospitalProfileAsync` mengembalikan status `Failed`). Akibatnya, `CanFreeze` bernilai `false` di `InpAdmissionSnapshotBuilder.cs:41`, dan `InpAdmissionDocumentService.LockAsync` membatalkan penguncian dokumen.

---

### ISS-EPS-005-03 — `DbUpdateConcurrencyException` saat menyimpan tanda tangan dokumen admisi

- **Gejala**: Pemanggilan endpoint pencatatan tanda tangan kertas (`POST .../signatures/patient-or-family`) maupun atestasi elektronik petugas (`POST .../signatures/head-nurse`, dll.) selalu menghasilkan penolakan keliru HTTP `409 Conflict` dengan kode `INP-ADM-DOC-004` (*"Dokumen sudah diubah petugas lain"*).
- **Akar Masalah**:
  Model `InpAdmissionDocumentSignature` menginisialisasi properti primary key secara inline: `public Guid Id { get; set; } = Guid.NewGuid();`. Di `InpAdmissionSignatureService.cs:285`, objek tanda tangan baru dimasukkan ke navigasi relasi: `document.Signatures.Add(signature)`. Karena induk `document` berstatus ter-track (`EntityState.Unchanged`) dan objek anak `signature` sudah memiliki nilai GUID non-empty, change tracker EF Core mengidentifikasi tanda tangan sebagai entitas lama yang dimodifikasi (`EntityState.Modified`). Saat `SaveChangesAsync`, EF Core mengeksekusi perintah SQL `UPDATE` ke tabel `InpAdmissionDocumentSignature`. Karena baris tersebut belum pernah ada di database, update menghasilkan 0 baris, memicu `DbUpdateConcurrencyException`, yang ditangkap dan salah diterjemahkan menjadi kode stale concurrency `409 INP-ADM-DOC-004`.

---

### ISS-EPS-005-04 — Ketiadaan konfigurasi kebijakan wewenang `SysAccessPolicy`

- **Gejala**: Pengujian menggunakan akun staf non-superadmin (misalnya Petugas Admisi atau Perawat Ruangan) selalu ditolak dengan HTTP `403 Forbidden` (*"Anda tidak memiliki akses ke menu atau fitur ini"*).
- **Akar Masalah**:
  Meskipun controller `InpatientAdmissionDocument` dan 10 aksi terkait sudah terdaftar di `SysControllerAccess` dan `SysActionAccess`, tidak ada satu pun baris pemetaan di tabel `SysAccessPolicy` yang mengaitkan departemen atau jabatan staf rumah sakit ke aksi-aksi tersebut. Akibatnya, sistem wewenang menolak seluruh pengguna selain SuperAdmin.
