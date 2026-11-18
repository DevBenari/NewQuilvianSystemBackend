# Dataset dan prosedur uji persetujuan permintaan barang

| Field | Nilai |
| --- | --- |
| Blueprint | `LGS-BP-001` r5 |
| Keputusan | `LGS-DEC-003`, `LGS-DEC-004`, `LGS-DEC-005`, `LGS-DEC-006` |
| Status | **BLOCKED untuk uji dengan data nyata.** Penulisan data membutuhkan izin tersendiri dan keputusan HRD; belum ada basis data sandbox yang disetujui |
| Disusun | 2026-11-17 |

## 1. Hasil audit ulang (hanya baca, 2026-11-17)

| Ukuran | `QuilvianNewDevMaster` (tim) | `QuilvianNewDevIkbalFr` (lokal) |
| --- | --- | --- |
| Unit layanan aktif | 20 | 5 |
| Unit layanan yang ditautkan ke unit organisasi | **0** | **0** |
| Unit organisasi aktif | 5 | **0** |
| Penempatan HR aktif | 61 | 1 |
| Penempatan HR yang menyebut unit organisasi | 2 (keduanya non-manajerial) | 0 |
| Penempatan manajerial aktif | **0** | **0** |
| Akun aktif dengan profil tenaga kerja | 59 | 4 |
| Izin `StockRequest:Approve` / `AuthorizeEmergency` yang diberikan | belum terdaftar (aplikasi versi baru belum dijalankan di sana) | terdaftar, 0 jabatan |

Kesimpulannya, data organisasi yang dibutuhkan belum ada. Tidak satu pun pengguna di kedua basis
data dapat menyetujui permintaan.

## 2. Kebutuhan dataset dan ketersediaannya

| Kebutuhan | Tersedia di basis data tim? | Keterangan |
| --- | --- | --- |
| Satu unit layanan | Ya | Lihat kandidat bagian 3 |
| Satu unit organisasi terkait | Unit organisasinya ada, **tautannya belum ada** | Tautan diputuskan HRD (`LGS-DEC-003` butir 5) |
| Satu pegawai dengan penempatan manajerial aktif | **Tidak** | 0 penempatan manajerial |
| Satu pemohon non-manajerial | Ya | Ada penempatan non-manajerial pada unit organisasi `ORG-RSMMC-00003`. Identitas tidak dicantumkan di dokumen ini |
| Satu jabatan penerima izin CITO | Kandidat ada, **penunjukan belum ada** | Jabatan bernama "Kepala Instalasi" atau "Penanggung Jawab Gudang" tidak ditemukan |
| Satu pemohon dari unit berbeda | **Tidak** | Butuh unit layanan kedua yang tertaut |

## 3. Kandidat identifier dari data nyata

Kandidat di bawah dipilih karena **namanya mirip**, bukan karena hubungannya sudah benar.
Tautan baru sah setelah dikonfirmasi HRD.

### Unit organisasi aktif (basis data tim)

| Id | Kode | Nama | Jenis |
| --- | --- | --- | --- |
| `28c51aef-c15b-44e0-8626-170a408dc904` | ORG-RSMMC-00001 | Direktorat Pelayanan Medis | Directorate |
| `8414497b-27b3-46a5-b3d1-66855db30f7c` | ORG-RSMMC-00002 | Departemen Keperawatan | Department |
| `42242c4c-5e4c-4f83-87db-e7da37e5551b` | ORG-RSMMC-00005 | Instalasi Gawat Darurat | Installation |
| `c797d03f-8882-4fbc-b582-aeb51a198d51` | ORG-RSMMC-00003 | Instalasi Rawat Inap | Installation |
| `0c8a3e0a-3226-421b-9e21-f40e1de86be4` | ORG-RSMMC-00004 | Unit Perawatan Intensif (ICU) | Unit |

### Pasangan nama yang mirip (perlu konfirmasi HRD)

| Unit layanan | Unit organisasi yang namanya mirip |
| --- | --- |
| `fddbe4ae-832b-484c-aba7-d6280e07c311` SU-IPD-001 Rawat Inap | `c797d03f-…` ORG-RSMMC-00003 Instalasi Rawat Inap |
| `4dd4827b-6b69-4cfa-9c5c-ded8ed07660e` SU-ER-001 Instalasi Gawat Darurat | `42242c4c-…` ORG-RSMMC-00005 Instalasi Gawat Darurat |
| `09bc36e7-9a7d-49ac-83cf-d8206253cb43` SU-ICU-001 ICU | `0c8a3e0a-…` ORG-RSMMC-00004 Unit Perawatan Intensif (ICU) |

### Jabatan yang namanya terkait (kandidat, bukan penunjukan)

| Id | Kode | Nama | Departemen | Pengguna aktif |
| --- | --- | --- | --- | --- |
| `fdda783c-74c1-5e8f-9041-9d81510d7d0d` | POS-KEP-002 | Kepala Ruangan | Keperawatan | 1 |
| `e8ecb2bc-8f96-f6bb-07e2-69d7a442d87c` | POS-KEP-001 | Kepala Keperawatan | Keperawatan | 1 |
| `1159bb7e-b529-4639-9233-eeb8b850c44a` | POS-FAR-001 | Apoteker | Farmasi | 1 |
| `22d37f81-79d7-54ca-3463-d6a4e3fb3196` | POS-FAR-003 | Petugas Gudang Farmasi | Farmasi | 0 |
| `db93fae2-c6d1-a50a-31ce-e3053ded769d` | POS-LOG-002 | Petugas Gudang | Logistik dan Gudang | 0 |

`LGS-DEC-004` menyebut Kepala Instalasi/Unit atau Penanggung Jawab Gudang sebagai kandidat, dan
daftar definitifnya ditunjuk manajemen. Izin CITO **tidak** diberikan otomatis kepada jabatan
mana pun di atas.

## 4. Prosedur penyiapan data uji terisolasi (belum dijalankan)

Prosedur ini hanya untuk **basis data sandbox yang sudah disetujui**, yaitu salinan yang bukan
basis data tim maupun produksi. Setiap langkah tulis membutuhkan izin tersendiri.

1. **Pastikan target.** Jalankan `select current_database(), inet_server_addr();` dan pastikan
   hasilnya basis data sandbox yang disetujui.
2. **Tautkan satu unit layanan** ke unit organisasi yang dikonfirmasi HRD. Ini mengubah master unit
   layanan, sehingga lebih aman dilakukan lewat layar master unit layanan bila tersedia.
   Templatnya sengaja tidak dapat langsung dijalankan:

   ```sql
   -- TEMPLAT. Ganti <...> dengan nilai yang dikonfirmasi HRD. Jalankan hanya di sandbox.
   -- update "MstServiceUnit" set "OrganizationUnitId" = '<org_unit_id>'
   -- where "Id" = '<service_unit_id>';
   ```

3. **Tandai satu penempatan manajerial** lewat modul HR (penempatan organisasi pegawai yang
   ditunjuk sebagai atasan unit). Jangan menyisipkan pegawai baru.
4. **Beri izin** lewat layar Akses Role:
   - `StockRequest:Approve` ke departemen × jabatan atasan unit tersebut;
   - `StockRequest:AuthorizeEmergency` ke satu jabatan yang ditunjuk manajemen.
5. **Ulangi untuk unit layanan kedua**, untuk uji negatif lintas unit.
6. **Verifikasi hanya baca:**

   ```sql
   select su."ServiceUnitCode", o."UnitCode", count(w.*) filter (where w."IsManagerialAssignment") as atasan
   from "MstServiceUnit" su
   join "MstOrganizationUnit" o on o."Id" = su."OrganizationUnitId"
   left join "WfpOrganizationAssignment" w on w."OrganizationUnitId" = o."Id"
        and w."IsActive" and not w."IsDelete" and not w."IsCancel"
   where su."OrganizationUnitId" is not null
   group by 1, 2;
   ```

7. **Jalankan skenario** pada bagian 5 dengan pemeriksaan izin aktif
   (`Security__Authorization__Enabled=true`). Jangan memakai `appsettings.Development.json` yang
   mematikan izin sebagai bukti keamanan.
8. **Kembalikan data sandbox** atau buang sandbox setelah uji.

## 5. Skenario uji runtime

| No | Skenario | Pengguna | Hasil yang diharapkan |
| --- | --- | --- | --- |
| 1 | Pemohon menyetujui permintaannya sendiri | Pemohon (walau berizin) | `403` |
| 2 | Atasan unit A menyetujui permintaan unit B | Atasan unit A | `403` |
| 3 | Pengguna tanpa izin `Approve` | Atasan unit tanpa izin | `403` |
| 4 | SuperAdmin tanpa penempatan sah | SuperAdmin | `403` untuk setuju, tolak, CITO, dan verifikasi |
| 5 | CITO tanpa izin eksplisit | Atasan unit atau SuperAdmin | `403` |
| 6 | CITO dengan izin eksplisit | Jabatan CITO yang ditunjuk | `200`, status Disetujui, verifikasi susulan Menunggu |
| 7 | Tolak tanpa alasan | Atasan unit | `400` (validasi model) atau `422` `PHM015` |
| 8 | CITO tanpa alasan, saat pengajuan maupun saat otorisasi | Pemohon / jabatan CITO | `422` `PHM014` / `PHM015` |
| 9 | Persetujuan kedua | Atasan unit kedua | `409` `PHM018` |
| 10 | Dua persetujuan hampir bersamaan | Dua atasan | Satu `200`, satu `409` `PHM012` |
| 11 | Permintaan sebelum batas | Gudang | Boleh diserahkan langsung |
| 12 | Permintaan sesudah batas | Gudang | `409` `PHM016` sampai disetujui |
| 13 | Verifikasi susulan | Atasan unit | Saldo dan mutasi stok tidak berubah |
| 14 | Verifikasi oleh pemohon atau pengotorisasi CITO | — | `403` |
| 15 | Bentuk respons | — | Angka status 1–6 dan `emergencyReviewState` 0–3 sesuai konstanta frontend |
