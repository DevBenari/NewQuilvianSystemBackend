# Laporan Live Testing — Bagian Disiplin Employee 360

| Field | Nilai |
| --- | --- |
| Menu | Administrasi Kepegawaian → **Employee 360** (`/hr/employee-360/{id}`), tab Pribadi & Disiplin (Rahasia) |
| Pemilik menu | HR (Administrasi Kepegawaian) |
| Task | `FE-HRD-409`, layar `FE-HRD-401` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev `QuilvianNewDevHamzah` |
| Peramban | Chromium headed, `slowMo` 450 ms; screenshot bagian disiplin diambil per elemen sehingga NIK tidak ikut terekam |
| Akun uji | `uat.pkb.relations@rsmmc.local` (HR + `ViewSanctions`), `uat.pkb.manajer@rsmmc.local` (HR tanpa `ViewSanctions`), `uat.pkb.atasan@rsmmc.local` (atasan langsung) |
| Hasil | **5 dari 5 skenario lulus**; 0 galat server |

## 1. Ringkasan

Hak melihat data disiplin dibedakan dengan benar oleh backend dan layar:

| Siapa | Apa yang terlihat di bagian disiplin |
| --- | --- |
| HR yang memegang `ViewSanctions` | Tabel berisi 2 sanksi alur resmi dan 1 catatan lama berlabel "Catatan Lama" tanpa tombol |
| HR tanpa `ViewSanctions` (Manajer SDM) | "Data disiplin hanya untuk HR." di tempat tabel |
| Atasan langsung | Mode "Atasan Langsung — data terbatas" dan "Data disiplin hanya untuk HR."; nomor SP tidak tampil |
| Atasan yang bukan atasan pegawai tersebut | "Akses ditolak. Profil 360 hanya dapat diakses oleh HR Administrator atau Atasan Langsung aktif." |

## 2. Alur proses bisnis yang diuji

Contoh: Perawat Tono baru menerima SP 1. Pak Budi adalah atasan langsungnya.

1. Bu Rina (HR Relations) membuka Employee 360 Tono, tab **Pribadi & Disiplin (Rahasia)**. Ia melihat SP 1 (Berlaku), Teguran Lisan (Berlaku), dan satu catatan pembinaan lama dari profil sebelum alur resmi, berstatus Arsip, tanpa tombol ubah atau terbitkan.
2. Bu Maya (Manajer SDM) membuka profil yang sama dari daftar nama Dashboard SDM. Ia berada pada mode HR tetapi tidak memegang hak melihat sanksi, sehingga bagian disiplin menampilkan "Data disiplin hanya untuk HR."
3. Pak Budi (atasan langsung Tono) membuka profil Tono. Data rahasia disembunyikan dan bagian disiplin menampilkan pesan yang sama.
4. Pak Budi mencoba membuka profil pegawai lain yang bukan bawahannya dan ditolak dengan pesan yang jelas.

## 3. Data uji (dibuat lewat API resmi)

| Data | Langkah |
| --- | --- |
| Pegawai "UAT PKB Tono Bawahan" (tanpa akun login) | `POST master-data/employees` |
| Atasan langsung Tono = "UAT PKB Budi Atasan" | `POST workforce-profiles/{id}/manager-assignments` |
| SP 1 dan Teguran Lisan | Lihat laporan Tindak Disiplin |
| Catatan disiplin lama "Catatan pembinaan sebelum alur resmi (data uji)" | `POST workforce-profiles/{id}/disciplinary-actions` |

## 4. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| L08 | P0 | HRD-129 | HR + `ViewSanctions` | 2 baris `OFFICIAL` + 1 "Catatan lama" tanpa tombol (`AT-HRD-PKB-13`, `15`) | 2 + 1; 0 tombol | ✅ PASS | `fe-hrd-401/relations-04-employee360-disiplin.png` |
| M07 | P1 | HRD-129 | HR tanpa `ViewSanctions` | "Data disiplin hanya untuk HR." (`VR-PKB-09`) | Sesuai | ✅ PASS | `fe-hrd-401/manajer-03-employee360-disiplin-tersembunyi.png` |
| T01 | P1 | HRD-130 | Menu atasan | Tanpa butir Paket B | Sesuai | ✅ PASS | — |
| T02 | P1 | HRD-129 | Atasan langsung | Pesan disembunyikan; nomor SP tidak tampil (`AT-HRD-PKB-14`) | Sesuai | ✅ PASS | `fe-hrd-401/atasan-01-employee360-atasan.png` |
| T03 | P1 | HRD-129 | Bukan atasan | Pesan akses ditolak | Sesuai | ✅ PASS | `fe-hrd-401/atasan-02-employee360-bukan-bawahan.png` |

## 5. Endpoint yang diuji

### `[Tags("Corporate / Human Resource / Workforce Core / Employee 360")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | Profil terpadu | `HrdEmployee360 : Read`; mode HR lewat `HrdPersonnelActionOrder : Read`; isi disiplin lewat `EmployeeDisciplinary : ViewSanctions` | `asOfDate` opsional | `200` (HR dan atasan), `403` (bukan atasan) |

## 6. Temuan

Tidak ada temuan baru. Catatan untuk diketahui: mode Atasan Langsung menampilkan dua peringatan bertumpuk (akses terbatas bagian pribadi, lalu pesan disiplin). Keduanya benar, tetapi dapat disederhanakan pada task lain.
