# Manajemen Vendor — Laporan Kesiapan

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2, bentuk `COMPOSITE`; sub-modul `manajemen-vendor` (`PBJ-VD`), status desain `approved` (`PBJ-DEC-062`); registry submodule `Vendor` `ACTIVE` (`PBJ-DEC-063`) |
| Keputusan yang berlaku | `PBJ-DEC-001`..`063` dan `PBJ-FE-004` (`decision_revision` 16) |
| Kontrak yang berlaku | `PBJ-VD-API`, `-STATE`, `-VAL`, `-INT`, `-PERM`, `-TEST`, `-MVP` semuanya 1.0 (`approved`). Usulan `PBJ-VD-API` 1.1 dan `PBJ-VD-INT` 1.1 tercatat di laporan task, belum disahkan |
| Masukan desain | Hash 4 masukan dan 14 artefak di manifest **cocok** dengan berkas saat audit (SHA256 16 karakter) |
| Source yang diaudit | Backend `a6df5619` (branch `sukmagp`) + 42 berkas berubah/baru belum di-commit; frontend `b1b360efa` (branch `sukmagpV2`) + 34 berkas berubah/baru belum di-commit. Manifest masih mencatat `c6b86a6e`/`059b6c612` |
| Lingkungan uji | `QuilvianNewDevSukma` (query baca-saja `SELECT` saat audit; data uji task sudah dibersihkan) |
| Tanggal audit | 19 November 2026 |
| Skill | `verify-module-readiness` (read-only terhadap source) |
| Verdict sub-modul | **`READY_WITH_CONDITIONS`** |
| Verdict modul | **`NOT_READY`** — diturunkan dari sub-modul terlemah; `pemilihan-penyedia` dan `kontrak-pelaksanaan` belum dimulai |

---

## 1. Ringkasan untuk pembaca umum

Seluruh 13 task Manajemen Vendor sudah dibangun dan diuji. Tim Pengadaan bisa:

- mengatur kategori vendor;
- mencatat dokumen legalitas dan meminta petugas lain memverifikasinya;
- melihat kelayakan vendor per kategori;
- membatasi vendor;
- mengusulkan blokir lewat persetujuan Direktur;
- menilai vendor setiap semester.

Master Data Supplier tidak bisa lagi mengubah status blokir.

Namun sub-modul ini **belum bisa dipakai pengguna nyata hari ini**:

- **Alur persetujuan blokir belum dikonfigurasi.** Definisi `PBJ-VND-BLOKIR` dan `PBJ-VND-BUKA` masih Draft tanpa penyetuju (`PBJ-OQ-019`). Karena Master Data sudah dikunci, saat ini **tidak ada jalan untuk memblokir vendor sama sekali**.
  **Contoh:** Manajer mengajukan usulan blokir "Alkes Maju" karena tiga NCR kritis. Sistem menjawab "Alur persetujuan status vendor belum dikonfigurasi. Hubungi administrator." (`PBJ-VAL-091`), dan vendor tetap Aktif. Selama itu, satu-satunya rem adalah **Batasi**: vendor tetap bisa dipilih, tetapi dengan catatan.
- **Hak akses peran belum diterapkan.** Di Akses Role belum ada satu pun jabatan nyata yang diberi izin vendor Pengadaan. Bundel awal `PBJ-DEC-051` juga kurang satu izin: Direktur belum diberi izin membaca lampiran usulan.
- **Kode belum di-commit**, kontrak 1.1 belum disahkan, dan pengguna bisnis belum menjalankan UAT.

Begitu persetujuan blokir dikonfigurasi, izin peran diterapkan, dan kodenya di-commit, sub-modul ini siap dipakai.

---

## 2. Skor kesiapan per dimensi

Kemajuan scaffold (berkas, build) dipisahkan dari kesiapan ujung-ke-ujung. Setiap angka memakai denominator yang disebut.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi (registry, migration, skema, seeder, definisi `Wfl`) | 20% | 5 / 6 | Registry `Vendor` `ACTIVE` (`AT-VD-60`); migration `20261117074530_AddProcurementVendorManagement` tercatat di `__EFMigrationsHistory`; skema sesuai kamus data (`AT-VD-62`); `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration." (19 November 2026); seeder idempoten (`AT-VD-64`): 5 jenis dokumen aktif, 3 kriteria contoh kategori Umum (100%), kewajiban dokumen 0 (memang kosong, `PBJ-DEC-061`); 2/2 definisi `PBJ-VND-*` beserta langkahnya, `AllowSelfApproval = false` | Konfigurasi persetujuan: 0/2 definisi aktif, 0 baris matriks aktif (G-01) |
| Backend | 25% | 8 / 8 task; 66 / 66 endpoint berizin | Laporan `BE-PBJ-VD-001`..`008`; 7 controller (`Vendors`, `VendorDocuments`, `VendorStatusRequests`, `VendorEvaluations`, tiga master) dengan 66 aksi HTTP, masing-masing punya `[AccessAction]` dan `[AccessPermission]`; 0 `AllowAnonymous`, 0 `IsInRole`/`UserType`; QBE Strict `PASS` di setiap task; build Release 0 error | Delta kontrak belum masuk dokumen kontrak (G-04) |
| Frontend | 20% | 5 / 5 task; 15 / 15 rute terjangkau | Laporan `FE-PBJ-VD-001`..`005`; `npm run lint:errors` exit 0 dan `npm run build` `Compiled successfully` (39 rute Pengadaan) pada task terakhir; 10 butir menu (6 di menu 5, 1 di menu 10, 3 di Pengaturan) + 5 rute anak (detail vendor, usulan baru/detail, penilaian baru/detail) dibuka dari layar induk; Master Data Supplier: centang blokir baca saja | Label menu 10 masih sementara (`PBJ-OQ-033`) |
| Integrasi dan runtime | 15% | 4 / 6 | `Wfl`: ajukan, setujui dari Antrean Persetujuan Pengadaan, tarik (`Withdrawn`), sinkron ulang (`AT-VD-22`..`29`, uji layar `FE-PBJ-VD-004`); penerap keputusan menulis `MstSupplier.IsBlacklisted` dalam satu transaksi; lampiran `PBJ_VENDOR_DOCUMENT`/`STATUS`/`EVALUATION`; kunci Master Data (`AT-VD-26`) | Konfigurasi `Wfl` nyata belum ada (G-01); konsumen kelayakan (`pemilihan-penyedia`) belum dibangun (G-09) |
| Verifikasi penerimaan | 20% | 34 / 34 `AT-VD` penuh; 0 / 12 `UAT-VD` | Bagian 3 | UAT pengguna belum dijalankan (G-06) |

**Kemajuan scaffold:** 13 / 13 task ✅, 13 / 13 laporan tracked ada, 17 / 17 FR punya task BE dan FE ✅, grafik dependency dan tanda status di kedua roadmap cocok dengan kartu task.

**Kesiapan ujung-ke-ujung di lingkungan nyata:** belum, karena syarat K-1 sampai K-3 di bagian 6 belum terpenuhi.

---

## 3. Uji penerimaan (`AT-VD-*`)

Denominator: 34 skenario di `testing/acceptance-test-matrix.md`.

| Kelompok | Jumlah | Terbukti penuh | Bukti API/DB | Bukti layar |
| --- | ---: | ---: | --- | --- |
| 1. Profil, kategori, rekening (`01`..`04`) | 4 | 4 | `BE-PBJ-VD-003` | `FE-PBJ-VD-002` |
| 2. Dokumen legalitas dan kelayakan (`10`..`17`) | 8 | 8 | `BE-PBJ-VD-004`, `BE-PBJ-VD-005` | `FE-PBJ-VD-003` |
| 3. Status vendor dan usulan blokir (`20`..`29`) | 10 | 10 | `BE-PBJ-VD-005`..`007` | `FE-PBJ-VD-004` |
| 4. Penilaian berkala (`30`..`36`) | 7 | 7 | `BE-PBJ-VD-008` | `FE-PBJ-VD-005` |
| 5. Fondasi dan konformitas (`60`..`64`) | 5 | 5 | `BE-PBJ-VD-001`; cek EF audit ini | `AT-VD-63` di kelima task FE |
| **Total** | **34** | **34** | | |

Catatan bukti:

- **`AT-VD-12`.** Di `BE-PBJ-VD-004` aktor pengunggah tertolak oleh Akses Role sebelum aturan `PBJ-VAL-103`, karena tidak diberi `Verify`. Aturannya kemudian terbukti di `FE-PBJ-VD-003` (`API-01`/`02`): pengunggah yang punya `Verify` tetap ditolak 403 `PBJ-VAL-103`.
- **`AT-VD-25`.** Terbukti dua lapis:
  - langkah `PBJ-VND-*` di DB ber-`AllowSelfApproval = false`;
  - pengusul tidak mendapat tugas persetujuan, sehingga `approve` atas tugas penyetuju dijawab 404 (`BE-PBJ-VD-007`, `FE-PBJ-VD-004` `UI-09` dan `API-01`).

  Kasus "pengusul juga tercantum sebagai penyetuju di matriks" belum diuji karena matriks uji hanya berisi Direktur.
- **`AT-VD-33`.** `PBJ-VAL-111` terbukti dengan pemfinal yang juga penilai revisi (`FE-PBJ-VD-005` `API-01`).

**UAT pengguna (`04-prd-to-mvp.md` bagian 18):** 0 dari 12 dijalankan oleh pengguna bisnis. Skenario agen di atas mencakup isi `UAT-VD-01`..`12`, tetapi UAT adalah penerimaan oleh pemilik proses. `UAT-VD-08` butuh G-01 lebih dulu.

---

## 4. Definition of Done MVP (`04-prd-to-mvp.md` bagian 19)

| Butir | Status | Bukti |
| --- | --- | --- |
| Registry submodule `Vendor` disetujui sebelum model pertama | Terbukti | `AT-VD-60`; `PBJ-DEC-063` |
| Identitas vendor tidak diduplikasi | Terbukti | `AT-VD-01`: profil lahir satu baris, `MstSupplier` identik sebelum/sesudah |
| Rekening tidak pernah keluar utuh | Terbukti | `AT-VD-03`; daftar vendor juga tersamarkan (`BE-PBJ-VD-003` `R-04`) |
| Pengunggah tidak memverifikasi sendiri; penilai tidak memfinalkan sendiri | Terbukti | `AT-VD-12`, `AT-VD-33` (lihat catatan bagian 3) |
| Dokumen wajib kedaluwarsa membuat vendor tidak layak; masa awal tidak mengunci | Terbukti | `AT-VD-14`, `AT-VD-15`; kewajiban dokumen kosong di DB |
| Blokir hanya lewat approval; Master Data terkunci | Terbukti di dev dengan matriks uji | `AT-VD-24`..`26`; di lingkungan nyata menunggu G-01 |
| Penilaian unik per vendor × kategori × periode dan bobot tersalin | Terbukti | `AT-VD-31`, `AT-VD-32` (4,20 tetap setelah bobot master menjadi 40/50/10) |
| Layar daftar mengikuti `PBJ-FE-004` | Terbukti | `AT-VD-63` di `-01`, `-03`..`-06`, `-08`, `-09`, `-11`..`-13` |
| Seeder idempoten, kewajiban dokumen kosong | Terbukti | `AT-VD-64`; audit DB: kewajiban aktif 0 |
| Build dan skema sesuai kamus data | Terbukti | `AT-VD-61`, `AT-VD-62`; tanpa perubahan model tertunda |

---

## 5. Gap, dikelompokkan

| ID | Kelompok | Gap | Dampak nyata | Arahkan ke |
| --- | --- | --- | --- | --- |
| G-01 | Data/konfigurasi | `PBJ-VND-BLOKIR` dan `PBJ-VND-BUKA` masih `Draft`, tidak aktif, 0 baris matriks (`PBJ-OQ-019`). Disengaja fail-closed | Usulan blokir/buka blokir dijawab `PBJ-VAL-091`. Karena Master Data sudah dikunci (`PBJ-DEC-060`), **tidak ada jalan memblokir vendor** sampai ini diisi | Product Owner (SOP) + admin `Wfl` |
| G-02 | Keamanan | Bundel izin `PBJ-DEC-051` belum diterapkan: 0 jabatan nyata punya izin vendor Pengadaan di Akses Role | Menu 5 dan 10 tidak tampil bagi siapa pun selain SuperAdmin | Admin Akses Role |
| G-03 | Keamanan/kontrak | Bundel Direktur di `permission-audit-matrix.md` bagian 3 tidak memuat `ProcurementAttachment : Read` (`FE-PBJ-VD-004` F-6). Penyetuju tidak bisa membuka lampiran bukti usulan. Pemilik keamanan sub-modul juga "belum ditetapkan" (manifest `owners.security`) | Direktur memutus usulan tanpa melihat bukti lampiran; penjaga lampiran dan cakupan baca belum ditinjau orang berwenang | `grill-me` amendment (`PBJ-VD-PERM` 1.1); Product Owner menunjuk peninjau |
| G-04 | Desain/kontrak | Delta kontrak dari laporan task belum masuk dokumen kontrak, antara lain: daftar vendor membawa rekening tersamarkan dan saringan bank (D-9); `vendorStatus=NON_ACTIVE` (D-10); `warnings`, kode, dan nama vendor di kelayakan (INT); `POST /{id}/workflow/synchronize` usulan; body opsional `finalize`; ringkasan penilaian berparameter; jenis periode `SEMESTER`/`QUARTER`/`YEAR` | Klien lain yang membaca kontrak 1.0 tidak tahu permukaan baru; dokumen dan kode berbeda | `manage-module-blueprint` + persetujuan pemilik (`PBJ-VD-API` 1.1, `PBJ-VD-INT` 1.1) |
| G-05 | Operasional | Kode belum di-commit (backend 42 berkas, frontend 34 berkas, termasuk pekerjaan modul lain dalam sesi). Manifest masih `c6b86a6e`/`059b6c612`, roadmap tertulis "draft", `last_verified_at` kosong | Bukti audit tidak terikat ke SHA yang bisa diulang; risiko perubahan hilang | Pengguna: commit atas izin sendiri; `manage-module-blueprint` memperbarui manifest |
| G-06 | Test | UAT pengguna 0/12 | Belum ada bukti pemilik proses menerima alurnya | Product Owner |
| G-07 | Data/konfigurasi | Master awal minim: 1 kategori aktif (Umum), kriteria hanya untuk Umum (nilai contoh `PBJ-DEC-050`, `PBJ-OQ-034`), 2 supplier aktif tanpa profil Pengadaan | Penilaian kategori lain ditolak `PBJ-VAL-108` sampai kriterianya diisi; vendor perlu diberi kategori sebelum dinilai atau diperiksa kelayakannya | Tim Pengadaan mengisi kategori dan kriteria; `PBJ-OQ-034` |
| G-08 | Frontend/dokumentasi | Label submenu menu 10 masih sementara "Penilaian Vendor" (`PBJ-OQ-033`) | Nama menu bisa berubah setelah keputusan | Pengguna sesi (BRD B10) |
| G-09 | Integrasi | Konsumen kelayakan vendor (`pemilihan-penyedia`) belum dibangun | Kelayakan baru tampil di layar Pengadaan; belum mencegah pemilihan vendor di RFQ | Sub-modul `pemilihan-penyedia` |
| G-10 | Lintas modul | **Di luar Pengadaan:** form Master Data Supplier selalu mengirim `taxPercent`, sedangkan backend menolaknya untuk supplier non-pajak ("Tax percent hanya boleh diisi jika supplier taxable."), sehingga supplier non-pajak tidak bisa disimpan dari layar itu (`FE-PBJ-VD-004` T-2) | Admin Master Data tidak bisa memperbarui supplier non-pajak, termasuk sesudah blokir menjadi baca saja | Pemilik Master Data Supplier; satu task frontend terpisah |

**Pemeriksaan tingkat modul (`COMPOSITE`):**

| Pemeriksaan | Hasil |
| --- | --- |
| Kemampuan yatim | 0 dari 13 (`K-01`..`K-13`): semuanya punya sub-modul pemilik di `02-module-map.md` bagian 4 |
| Tabel bertuan ganda | 0: data pendamping vendor (`PrcVendor*`) dan konfigurasinya hanya milik `manajemen-vendor`; `MstSupplier` milik Administrator Master Data dan hanya dibaca (kecuali status blokir lewat usulan, `PBJ-DEC-060`) |
| Sub-modul lain | `perencanaan-permintaan` `READY_WITH_CONDITIONS` (17 November 2026); `pemilihan-penyedia` dan `kontrak-pelaksanaan` `NOT_STARTED` |

---

## 6. Syarat (`READY_WITH_CONDITIONS`)

Sub-modul boleh dibuka ke pengguna setelah K-1 sampai K-3 terpenuhi. K-4 sampai K-7 boleh menyusul, tetapi wajib selesai sebelum sign-off akhir.

| No | Syarat | Pemilik | Mitigasi selama belum terpenuhi | Risiko bila diabaikan |
| ---: | --- | --- | --- | --- |
| K-1 | Isi matriks penyetuju `PBJ-VND-BLOKIR`/`BUKA` dari SOP, lalu aktifkan definisinya (G-01). **Wajib sebelum kunci Master Data (`BE-PBJ-VD-007`) dirilis ke produksi** | Product Owner + admin `Wfl` | Fail-closed; Batasi tetap tersedia sebagai rem sementara; mesin sudah terbukti jalan dengan matriks uji (`AT-VD-22`..`29`) | Vendor bermasalah tidak bisa diblokir sama sekali |
| K-2 | Terapkan bundel izin `PBJ-DEC-051` di Akses Role, ditambah `ProcurementAttachment : Read` untuk Direktur (G-02, G-03) | Admin Akses Role; Product Owner untuk tambahan Direktur | Layar menyembunyikan tombol tanpa izin; server tetap menolak | Menu tidak tampil; Direktur memutus tanpa melihat bukti |
| K-3 | Commit kode backend dan frontend, lalu perbarui manifest (SHA, status roadmap, `last_verified_at`) (G-05) | Pengguna (commit); `manage-module-blueprint` | Laporan task mencatat berkas yang berubah | Perubahan hilang atau tidak bisa ditelusuri ke SHA |
| K-4 | Sahkan `PBJ-VD-API` 1.1, `PBJ-VD-INT` 1.1, dan `PBJ-VD-PERM` 1.1 (G-03, G-04) | Product Owner; `manage-module-blueprint` / `grill-me` | Delta tercatat lengkap di laporan `BE-PBJ-VD-003`..`008` | Klien lain salah tafsir permukaan API |
| K-5 | Tunjuk pemilik keamanan dan tinjau penjaga lampiran, rekening tersamarkan, dan kunci Master Data (G-03) | Product Owner | `AT-VD-03`, `04`, `12`, `26`, `33` lulus | Celah yang luput dari uji fungsional tidak tertangkap |
| K-6 | Jalankan `UAT-VD-01`..`12` bersama pengguna; `UAT-VD-08` setelah K-1 (G-06) | Product Owner | Skenario agen sudah mencakup isi UAT | Alur tidak sesuai kebiasaan kerja lapangan |
| K-7 | Isi kategori dan kriteria penilaian nyata (`PBJ-OQ-034`); putuskan label menu 10 (`PBJ-OQ-033`) (G-07, G-08) | Tim Pengadaan; Pengguna sesi | Kriteria contoh kategori Umum 100% | Penilaian kategori lain ditolak `PBJ-VAL-108` |

**Risiko yang disengaja (sesuai keputusan, bukan gap):**

- Kewajiban dokumen dimulai kosong (`PBJ-DEC-061`), jadi semua vendor tampil Layak sampai Tim Pengadaan mengaktifkan kewajiban per kategori.
  **Contoh:** Izin PBF milik "Sehat Farma" kedaluwarsa kemarin, tetapi vendor tetap Layak untuk kategori Obat karena kewajiban Izin PBF belum diaktifkan.
- Rekomendasi penilaian hanya saran (`PBJ-DEC-048`), jadi rekomendasi Blokir tidak memblokir vendor.

**Temuan lintas modul:** G-10 (Master Data Supplier, pajak). Bug ini sudah ada sebelum Manajemen Vendor, tetapi terlihat di layar yang sama dengan kunci blokir.

---

## 7. Langkah berikutnya

1. **Product Owner + admin `Wfl`:** isi matriks `PBJ-VND-*` dari SOP lalu aktifkan (K-1), sebelum kunci Master Data dirilis.
2. **Admin Akses Role:** terapkan bundel `PBJ-DEC-051` + `ProcurementAttachment : Read` untuk Direktur (K-2).
3. **Pengguna:** commit perubahan backend dan frontend; lalu `manage-module-blueprint` memperbarui manifest (K-3).
4. **Product Owner:** sahkan kontrak 1.1 (K-4) dan tunjuk pemilik keamanan (K-5).
5. **UAT bersama pengguna** (K-6) dan isi master nyata (K-7), lalu jalankan `verify-module-readiness` ulang untuk sign-off.
6. **Pemilik Master Data:** satu task frontend untuk G-10 bila disetujui.
7. **Sub-modul berikutnya:** `pemilihan-penyedia` lewat `grill-me` (butuh `PBJ-OQ-005` metode pengadaan); di sana kelayakan vendor dipakai.

---

## 8. Tindak lanjut sesudah audit

| Tanggal | Syarat | Yang dikerjakan | Oleh |
| --- | --- | --- | --- |
| 19 November 2026 | K-3 (sebagian) | Manifest sub-modul diselaraskan: SHA `a6df5619`/`b1b360efa` dengan catatan perubahan belum di-commit, `last_verified_at`, `readiness`, status roadmap 13/13, usulan kontrak 1.1, hash laporan ini. Manifest modul dan `MODULE-STATUS.md` mencatat verdict dan syarat VD K-1..K-7. **Commit tetap menunggu perintah pengguna** | `manage-module-blueprint` |
| 19 November 2026 | K-4 (diputuskan) | `PBJ-DEC-064`: seluruh delta laporan `BE-PBJ-VD-001`..`008` disahkan menjadi `PBJ-VD-API`/`INT`/`STATE`/`VAL` 1.1, termasuk empat aturan bisnis (blokir mencabut status supplier pilihan, ubah pembatasan lewat cabut lalu batasi ulang, draf penilaian bisa disunting pemegang `Update`, kategori nonaktif membuat vendor tidak layak). `PBJ-DEC-065`: bundel Direktur ditambah `ProcurementAttachment : Read` dan `ApprovalInbox : RequestRevision` (`PBJ-VD-PERM` 1.1). Dokumen kontrak 1.1 belum ditulis | `grill-me` amendment pass |
| 19 November 2026 | K-4 selesai | Dokumen kontrak ditulis: `PBJ-VD-API` 1.1 (seluruh endpoint kini tercatat tersedia, delta per laporan task ditandai "(1.1, VD-00x D-y)", tabel perubahan 1.0 → 1.1), `PBJ-VD-INT` 1.1 (`supplierCode`, `supplierName`, `warnings[]`), `PBJ-VD-STATE` 1.1 (Digantikan, aturan cabut-lalu-batasi, pengusul saja, revisi menyalin snapshot), `PBJ-VD-VAL` 1.1 (bagian penolakan tanpa kode), `PBJ-VD-PERM` 1.1 (bundel Direktur, penjaga di service, riwayat usulan). Manifest diperbarui | `manage-module-blueprint` |

K-1, K-2, K-5..K-7 dan bagian commit K-3 masih terbuka; K-4 selesai. Verdict tetap `READY_WITH_CONDITIONS` sampai audit ulang.
