# Perencanaan & Permintaan — Laporan Kesiapan

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2, bentuk `COMPOSITE`; sub-modul `perencanaan-permintaan` (`PBJ-PP`), status desain `approved` (`PBJ-DEC-034`) |
| Keputusan yang berlaku | `PBJ-DEC-001`..`040` (terakhir: `PBJ-DEC-039` bundel izin, `PBJ-DEC-040` tab Persetujuan) |
| Kontrak yang berlaku | `PBJ-PP-API` 1.2, `-PERM` 1.3, `-INT` 1.1, `-STATE` 1.0, `-VAL` 1.0, `-TEST` 1.0, `-MVP` 1.0 |
| Source yang diaudit | Backend `9f396063` (branch `sukmagp`), frontend `608e46ba4` (branch `sukmagpV2`); keduanya bersih, tanpa perubahan belum di-commit |
| Lingkungan uji | `QuilvianNewDevSukma` (query baca-saja `SELECT` saat audit) |
| Tanggal audit | 17 November 2026 |
| Skill | `verify-module-readiness` (read-only terhadap source) |
| Verdict sub-modul | **`READY_WITH_CONDITIONS`** |
| Verdict modul | **`NOT_READY`** — diturunkan dari sub-modul terlemah; tiga sub-modul lain belum dimulai |

---

## 1. Ringkasan untuk pembaca umum

Kode `perencanaan-permintaan` sudah lengkap dan sudah diuji. Pengusul bisa membuat permintaan pengadaan (PR),
Tim Pengadaan memverifikasi dan mengecek stok, penyetuju menyetujui lewat mesin persetujuan, dan Perencana
menyusun rencana kebutuhan, pagu, serta rencana paket.

Namun sub-modul ini **belum bisa dipakai pengguna nyata hari ini**, karena konfigurasinya belum diisi:

- **Penyetuju belum ditentukan.** Kelima alur persetujuan `PBJ-*` masih tidak aktif dan tidak punya satu pun
  baris matriks penyetuju yang aktif.
  **Contoh:** Tim Pengadaan menyelesaikan verifikasi PR Rp 45 juta. Sistem menjawab "Alur persetujuan pengadaan
  belum dikonfigurasi. Hubungi administrator." (`PBJ-VAL-091`), dan PR tetap berstatus "Diajukan". Ini memang
  disengaja (`PBJ-DEC-038`): sistem lebih baik menolak daripada mengarang penyetuju.
- **Hak akses peran belum diterapkan** di layar Akses Role untuk lingkungan nyata.
- **Pengguna bisnis belum menjalankan UAT**, dan penanggung jawab keamanan belum ditunjuk.

Begitu tiga hal itu beres, sub-modul ini siap dipakai.

---

## 2. Skor kesiapan per dimensi

Kemajuan scaffold (berkas, build) dipisahkan dari kesiapan ujung-ke-ujung. Setiap angka memakai denominator yang
disebut.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi (registry, migration, skema, seeder, perluasan `Wfl`) | 20% | 6 / 7 | Registry `Prc` `ACTIVE` (`AT-PP-60`, `BE-PBJ-002`); 8/8 migration Pengadaan tercatat di `__EFMigrationsHistory`; 14 tabel `Prc*`/`MstProcurement*` (`AT-PP-62`); seeder idempoten (`AT-PP-64`); 5/5 definisi `PBJ-*` beserta langkahnya (PR 4 langkah, 2 bersyarat) | Konfigurasi persetujuan: 0/5 definisi aktif, 0 baris matriks aktif |
| Backend | 25% | 11 / 11 task; 88 / 88 endpoint berizin | Laporan `BE-PBJ-001`..`011`; 88 endpoint, semuanya punya `[AccessAction]` dan `[AccessPermission]`; 0 `AllowAnonymous`; 0 role hardcode (`IsInRole`, nama peran); QBE Strict `PASS`; tidak ada perubahan model tertunda | 2 delta kontrak terbuka (bagian 5, G-05 dan G-06) |
| Frontend | 20% | 8 / 8 task; 24 / 24 rute terjangkau | Laporan `FE-PBJ-001`..`008`; `npm run lint:errors` exit 0; `npm run build` exit 0 dengan 24 rute `/corporate/procurement/**`; 15 butir menu + 9 rute anak (buat baru/detail) dibuka dari tombol layar induk | 3 keterbatasan layar tercatat (bagian 5, G-09) |
| Integrasi dan runtime | 15% | 4 / 6 | `Wfl`: buat, ajukan atas nama pemohon, setujui, tolak, kembalikan, tarik, sinkron ulang (`AT-PP-20`..`36`); lampiran dan cakupan unit (`AT-PP-08`, `53`..`56`); penomoran (`AT-PP-01`, `50`); regresi HR dengan dokumen pengganti (`AT-PP-35`) | Regresi cuti/lembur belum (`UAT-16`); sambungan ke `pemilihan-penyedia` belum ada (sengaja, di luar roadmap) |
| Verifikasi penerimaan | 20% | 43 / 45 `AT-PP` penuh; 0 / 16 `UAT` | Bagian 3 | `AT-PP-35` dan `AT-PP-63` sebagian; UAT pengguna belum dijalankan |

**Kemajuan scaffold:** 19 / 19 task ✅, 100% laporan tracked ada, grafik dependency dan tanda status cocok.
**Kesiapan ujung-ke-ujung di lingkungan nyata:** belum, karena syarat konfigurasi K-1 sampai K-3 di bagian 6
belum terpenuhi.

---

## 3. Uji penerimaan (`AT-PP-*`)

Denominator: 45 skenario di `testing/acceptance-test-matrix.md`.

| Kelompok | Jumlah | Terbukti penuh | Sebagian | Bukti utama |
| --- | ---: | ---: | ---: | --- |
| 1. Permintaan Pengadaan (`01`..`10`) | 10 | 10 | 0 | `BE-PBJ-006`, `BE-PBJ-007`, `FE-PBJ-003` |
| 2. Verifikasi dan cek stok (`20`..`28`) | 9 | 9 | 0 | `BE-PBJ-008` |
| 3. Persetujuan `Wfl` (`30`..`36`) | 7 | 6 | 1 | `BE-PBJ-008`, `BE-PBJ-007`, `BE-PBJ-001` |
| 4. Rencana, rencana paket, pagu (`40`..`46`) | 7 | 7 | 0 | `BE-PBJ-009`, `BE-PBJ-010`, `BE-PBJ-011` |
| 5. Katalog dan lampiran (`50`..`56`) | 7 | 7 | 0 | `BE-PBJ-003`, `BE-PBJ-004`, `BE-PBJ-006` |
| 6. Fondasi dan konformitas (`60`..`64`) | 5 | 4 | 1 | `BE-PBJ-002`, `BE-PBJ-003`, `BE-PBJ-005`, log build |
| **Total** | **45** | **43** | **2** | |

| Skenario sebagian | Yang sudah terbukti | Yang belum |
| --- | --- | --- |
| `AT-PP-35` regresi HR | Dokumen HR standar ketenagaan (`WorkforceStaffingStandard`) dibuat, diajukan, disetujui sampai `Completed`; langkah tanpa kode kondisi tetap dibuat; penangan HR dipanggil lebih dulu | Cuti dan lembur belum diuji: master kebijakan cuti (`MstLeavePolicy`) dan kebijakan lembur di dev kosong. Ditutup oleh `UAT-16` |
| `AT-PP-63` menu | Kode: 15 butir menu menunjuk 15 rute, 9 rute anak dibuka dari tombol layar induk (24/24). Runtime: skrip uji layar fase a–j membuka ke-24 rute itu | Bukti yang diminta adalah tangkapan layar **setiap** butir menu; yang tersimpan baru sebagian (antara lain Dasbor Menunggu Persetujuan, Cek Stok, dan Validasi Persetujuan belum punya tangkapan layar). `AT-PP-61` dan `AT-PP-63` juga belum dicatat dengan ID-nya di laporan task mana pun |

Catatan untuk `AT-PP-51`: perilakunya benar (barang terpakai tidak terhapus, data tetap ada), tetapi kode HTTP
penghapusan barang terpakai adalah **400**, sedangkan matriks uji dan `validation-matrix.md` menulis **409**.
Lihat G-05.

**UAT pengguna (`04-prd-to-mvp.md` bagian 18):** 0 dari 16 dijalankan oleh pengguna bisnis. Skenario agen di atas
mencakup isi `UAT-01`..`15`, tetapi UAT adalah penerimaan oleh pemilik proses, bukan oleh pengembang.

---

## 4. Definition of Done MVP (`04-prd-to-mvp.md` bagian 19)

| Butir | Status | Bukti |
| --- | --- | --- |
| Registry prefix dan Area disetujui sebelum model pertama | Terbukti | `AT-PP-60`; `PBJ-DEC-035` |
| Satu PR dapat berjalan dari draft sampai `APPROVED` | Terbukti di dev dengan matriks uji | `AT-PP-31`; `UAT-01` belum |
| Pengusul tidak dapat menyetujui atau memverifikasi PR sendiri | Terbukti | `AT-PP-27`, `AT-PP-30`; langkah `PBJ-*` di DB ber-`AllowSelfApproval = false` |
| Nominal approval = nilai beli setelah stok | Terbukti | `AT-PP-20` (20 kursi roda, stok 5, nominal Rp 45.000.000) |
| Sisa rencana tidak bisa terpakai ganda | Terbukti | `AT-PP-07` |
| Data unit lain tidak bocor | Terbukti | `AT-PP-08`, `AT-PP-56`; endpoint baru `GET /{id}/approval` juga 404 untuk unit lain (`phase_i`, laporan `BE-PBJ-008`) |
| Alur HR tidak berubah | Sebagian | `AT-PP-35` dengan dokumen pengganti; `UAT-16` menunggu master cuti |
| Seluruh master MVP terisi dan seeder idempoten | Terbukti untuk seeder | Jenis pengadaan 4/4, sumber dana 2/2, kategori `UMUM` 1; `AT-PP-64`. Katalog barang aktif **0** — diisi Tim Pengadaan sebelum dipakai (bukan seeder) |
| Setiap layar `MVP-*` terjangkau dari menu atau layar induk | Sebagian (bukti) | `AT-PP-63` di bagian 3 |
| Build backend dan frontend sukses; skema sesuai kamus data | Terbukti | `AT-PP-61`, `AT-PP-62` |

---

## 5. Gap, dikelompokkan

| ID | Kelompok | Gap | Dampak nyata | Arahkan ke |
| --- | --- | --- | --- | --- |
| G-01 | Data/konfigurasi | Definisi `PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU` tidak aktif; matriks penyetuju aktif 0 baris (`PBJ-OQ-019`) | Verifikasi PR, pengajuan rencana, pagu, dan rencana paket semuanya dijawab `PBJ-VAL-091`. PR yang seluruhnya dipenuhi stok tetap bisa selesai (`FULFILLED_FROM_STOCK`) | Admin `Wfl` + Product Owner (isi dari SOP, lalu aktifkan) |
| G-02 | Keamanan | Bundel izin `PBJ-DEC-039` belum diterapkan di Akses Role lingkungan nyata | Tanpa izin baca sumber dana, pengusul tidak bisa memilih sumber dana; tanpa izin baca satuan, Tim Pengadaan tidak bisa menambah barang | Admin Akses Role |
| G-03 | Keamanan | Pemilik keamanan sub-modul "belum ditetapkan" (manifest sub-modul, `owners.security`) | Penjaga lampiran (tanda tangan berkas) dan cakupan unit sudah lulus uji, tetapi belum ditinjau orang yang berwenang | Product Owner menunjuk peninjau |
| G-04 | Test | UAT pengguna 0/16; `UAT-16` (cuti) terhalang master cuti dev | Belum ada bukti pemilik proses menerima alurnya | Product Owner; pemilik HR untuk `UAT-16` |
| G-05 | Backend/kontrak | Hapus master terpakai (barang, kategori, sumber dana) menjawab **400** tanpa `errors.code`. `api-contract.md` 1.1 dan standar endpoint master data menulis 400, tetapi `validation-matrix.md` dan `AT-PP-51` masih menulis **409** (sisa versi 1.0) | Kontrak saling bertentangan; klien tidak bisa mengenali sebab penolakan dari kode | Diputuskan `PBJ-DEC-041`: tetap 400 + `errors.code` `PBJ-VAL-071` |
| G-06 | Backend/kontrak | `GET /budgets/remaining` memakai `ProcurementBudget : Read` (delta tercatat di `BE-PBJ-010`), tetapi `api-contract.md` dan `03-frontend-architecture.md` masih menulis `ProcurementRequest : Read` | Pengusul tanpa izin baca pagu tidak melihat ringkasan "Sisa pagu" di layar PR; pengajuan tetap jalan | Diputuskan `PBJ-DEC-042`: endpoint `GET purchase-requests/budget-remaining` ber-izin PR |
| G-07 | Desain/dokumentasi | Manifest basi: SHA masih `100c011d`/`763d665b`; versi kontrak masih API 1.1/PERM 1.2; `decision_revision` 9 (`..038`); 7 dari 19 hash masukan/artefak tidak cocok; `last_verified_at` kosong | Pembaca manifest mendapat versi kontrak yang salah | `manage-module-blueprint` |
| G-08 | Desain/dokumentasi | `02-backend-architecture.md` bagian 10, `04-prd-to-mvp.md` tabel pertanyaan terbuka, dan `requirement-traceability.md` masih menulis "approval memakai baris fallback", padahal bagian 8 dan `PBJ-DEC-038` menetapkan seed **tanpa** baris matriks (fail-closed) | Admin bisa mengira persetujuan sudah jalan dengan baris cadangan | `manage-module-blueprint` |
| G-09 | Frontend | (a) Tolak tidak tersedia di layar Validasi Persetujuan; ada tombol ke Kotak Masuk Terpadu. (b) Penanggung jawab paket (`picUserId`) belum bisa dipilih karena belum ada feed pengguna. (c) "Lihat versi" pagu menyaring di halaman yang sudah terbaca, karena backend belum punya saringan `lineageKey` | Ketiganya punya jalan lain dan tidak menghentikan proses | Backlog; tidak memblokir |
| G-10 | Integrasi | Penulis komitmen pagu, `MarkConvertedAsync` (`CONVERTED_TO_PACKAGE`), dan `GetApprovedAsync` dibangun oleh `pemilihan-penyedia` | Selama MVP, PR berakhir di `APPROVED`; pembelian dilanjutkan di luar sistem (PO di BLM, `PBJ-DEC-001`) | Sub-modul `pemilihan-penyedia` |
| G-11 | Operasional | Dokumen uji di dev sudah dihapus lunak, sehingga katalog aktif 0 dan kategori aktif hanya `UMUM` | Pengguna pertama tidak menemukan barang untuk dipilih | Tim Pengadaan mengisi katalog dan kategori |

**Pemeriksaan tingkat modul (`COMPOSITE`):**

| Pemeriksaan | Hasil |
| --- | --- |
| Kemampuan yatim | 0 dari 13 (`K-01`..`K-13`): semuanya punya sub-modul pemilik di `02-module-map.md` bagian 4 |
| Tabel bertuan ganda | 0: tiap kelompok data di `02-module-map.md` bagian 2 punya satu pemilik; tabel `Prc*` hanya milik `perencanaan-permintaan` |
| Sub-modul lain | `pemilihan-penyedia`, `manajemen-vendor`, `kontrak-pelaksanaan`: `NOT_STARTED`, belum punya desain maupun roadmap |

---

## 6. Syarat (`READY_WITH_CONDITIONS`)

Sub-modul boleh dibuka ke pengguna setelah syarat K-1 sampai K-3 terpenuhi. K-4 sampai K-6 boleh menyusul,
tetapi wajib selesai sebelum sign-off akhir.

| No | Syarat | Pemilik | Mitigasi selama belum terpenuhi | Risiko bila diabaikan |
| ---: | --- | --- | --- | --- |
| K-1 | Isi matriks persetujuan `PBJ-*` dari SOP lalu aktifkan definisinya (G-01) | Product Owner + admin `Wfl` | Fail-closed: tidak ada persetujuan yang dikarang; mesin sudah terbukti jalan dengan matriks uji (`AT-PP-20`..`36`) | Proses berhenti di verifikasi |
| K-2 | Terapkan bundel izin peran `PBJ-DEC-039` di Akses Role (G-02) | Admin Akses Role | Layar menyembunyikan tombol tanpa izin dan menampilkan "Akses Ditolak" | Pengguna tidak bisa memilih sumber dana atau satuan |
| K-3 | Tunjuk pemilik keamanan dan tinjau penjaga lampiran serta cakupan unit (G-03) | Product Owner | `AT-PP-08`, `53`, `54`, `56` lulus; endpoint `GET /{id}/approval` dibatasi unit (404) | Celah yang luput dari uji fungsional tidak tertangkap |
| K-4 | Jalankan UAT-01..15 bersama pengguna; `UAT-16` setelah master cuti tersedia (G-04) | Product Owner; pemilik HR | Skenario agen sudah mencakup isi UAT | Alur tidak sesuai kebiasaan kerja lapangan |
| K-5 | Putuskan G-05 dan G-06, lalu samakan kode atau kontraknya | Product Owner; satu task `build-module-backend` bila kode diubah | Layar sudah menangani keduanya | Klien lain yang membaca kontrak salah tafsir |
| K-6 | Perbarui manifest dan buang teks "baris fallback" (G-07, G-08) | `manage-module-blueprint` | — | Admin salah paham soal kesiapan persetujuan |

**Risiko yang disengaja (sesuai keputusan, bukan gap):** pagu belum terikat saat PR disetujui
(`PBJ-DEC-030`); komitmen baru tercatat saat penetapan vendor (`PBJ-DEC-009`). Karena `pemilihan-penyedia`
belum ada, **sisa pagu selama MVP selalu sama dengan pagu penuh**.
**Contoh:** pagu Radiologi Rp 100 juta. Tiga PR masing-masing Rp 40 juta disetujui. Ketiganya lolos tanpa
tanda "Melebihi pagu", karena masing-masing dibandingkan dengan Rp 100 juta. Product Owner perlu sadar akan hal
ini sebelum pagu dipakai sebagai kontrol.

**Temuan lintas modul (pemilik HR, bukan Pengadaan):** empat bug HR lama tercatat di laporan `BE-PBJ-001`. Salah
satunya penting: penerap keputusan HR menunggu status `Approved`, padahal mesin menutup instance dengan
`Completed`, sehingga dokumen perencanaan tenaga kerja tidak berpindah status setelah disetujui.

---

## 7. Langkah berikutnya

1. **Product Owner + admin `Wfl`:** isi matriks `PBJ-*` dari SOP, lalu aktifkan definisinya (K-1).
2. **Admin Akses Role:** terapkan bundel `PBJ-DEC-039` (K-2).
3. **Product Owner:** tunjuk pemilik keamanan (K-3) dan putuskan G-05 serta G-06 (K-5).
4. **`manage-module-blueprint`:** perbarui manifest sub-modul dan modul, lalu rapikan teks "baris fallback" (K-6).
5. **UAT bersama pengguna** (K-4), lalu jalankan `verify-module-readiness` ulang untuk sign-off.
6. **Sub-modul berikutnya:** `manajemen-vendor` lewat `grill-me` (`PBJ-DEC-018`: boleh paralel; datanya dipakai
   `pemilihan-penyedia`).

---

## 8. Tindak lanjut sesudah audit

| Tanggal | Syarat | Yang dikerjakan | Oleh |
| --- | --- | --- | --- |
| 17 November 2026 | K-6 (G-07, G-08) | Manifest modul dan sub-modul diselaraskan ke backend `9f396063` dan frontend `608e46ba4`, versi kontrak (API 1.2, PERM 1.3, MVP 1.1), `decision_revision` 10 (`PBJ-DEC-001`..`040`), hash masukan dan artefak, serta `last_verified_at`. Teks "approval memakai baris fallback" diganti di `02-backend-architecture.md` bagian 10, `04-prd-to-mvp.md` (naik ke versi 1.1), dan `requirement-traceability.md` | `manage-module-blueprint` |
| 17 November 2026 | K-5 (G-05, G-06) | Keputusan `PBJ-DEC-041` (hapus master terpakai tetap 400 + `errors.code` `PBJ-VAL-071`) dan `PBJ-DEC-042` (sisa pagu layar PR lewat `GET purchase-requests/budget-remaining`). Kontrak naik: API 1.3, VAL 1.1, TEST 1.1 (`AT-PP-11` baru). Implementasi lewat `build-module-backend` dan `build-module-frontend` | Pengguna sesi; `manage-module-blueprint` |
| 17 November 2026 | K-5 selesai | `PBJ-DEC-041`: hapus master terpakai 400 + `errors.code` `PBJ-VAL-071` (runtime 5/5 `PASS`). `PBJ-DEC-042`: `GET purchase-requests/budget-remaining` (runtime 9/9, layar 4/4 `PASS`). Build backend 0 error, QBE Strict `PASS`; lint dan build frontend exit 0 | `build-module-backend`, `build-module-frontend` |

Catatan koreksi: versi pertama laporan ini menulis "kontrak menulis 409" untuk G-05. Yang benar: `api-contract.md` 1.1 menulis 400,
hanya `validation-matrix.md` dan `AT-PP-51` yang menulis 409. Pengguna memutuskan ulang setelah koreksi itu.

K-1 sampai K-4 masih terbuka. Verdict tetap `READY_WITH_CONDITIONS` sampai audit ulang.

---

## 9. Pemeriksaan ulang 26 November 2026

| Field | Nilai |
| --- | --- |
| Source yang diperiksa | Backend `c97642bb` (branch `sukmagp`), frontend `3e5386679` (branch `sukmagpV2`); keduanya bersih. Kode Pengadaan terakhir berubah di backend `5e777833` dan frontend `af8a430ac` (19 November 2026, sub-modul `manajemen-vendor`) |
| Cara | Read-only: riwayat Git, hash dokumen, dan query `SELECT` di `QuilvianNewDevSukma` dalam transaksi `READ ONLY`. Build dan uji runtime **tidak** dijalankan ulang, karena kode sub-modul ini tidak berubah perilakunya (lihat tabel dampak) |
| Verdict sub-modul | **Tetap `READY_WITH_CONDITIONS`** |
| Verdict modul | **Tetap `NOT_READY`** — `pemilihan-penyedia` dan `kontrak-pelaksanaan` masih `NOT_STARTED` |

### 9.1 Dampak perubahan source sejak `9f396063` / `608e46ba4`

| Perubahan | Isi | Dampak pada sub-modul ini |
| --- | --- | --- |
| `a6df5619` / `b1b360efa` | `BE-PBJ-012` dan `FE-PBJ-009` milik roadmap revisi 2 sub-modul ini; laporannya ada | Tidak basi |
| `5e777833` (manajemen vendor) | 8 berkas di `Common`: konstanta baru (jenis dokumen `PBJ_VENDOR_*`, status, izin, kode alur), dua definisi alur `PBJ-VND-*` di seeder, 3 baris di layanan linimasa persetujuan. 126 baris ditambah, 3 baris dihapus: `Item` menjadi `Item,` dan dua baris komentar | Hanya penambahan. Perilaku PR, rencana, pagu, dan rencana paket tidak berubah |
| `af8a430ac` (manajemen vendor) | `ProcurementStatusBadge`, `procurement-document-tabs`, dan `procurement-history-panel` diperluas | Jenis status lama tidak disentuh (laporan `FE-PBJ-VD-002`) |
| Merge modul lain | `menu-items.jsx` diformat ulang; snapshot model berubah karena migration modul lain | 25 rute Pengadaan dan himpunan izin menu identik dengan `af8a430ac`; 9 migration Pengadaan tercatat di `__EFMigrationsHistory` |

Hash 4 masukan dan 15 artefak desain cocok dengan manifest sub-modul, sehingga desain tidak basi.

### 9.2 Keadaan syarat

| No | Status | Bukti 26 November 2026 |
| ---: | --- | --- |
| K-1 | Terbuka | 5/5 definisi (`PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU`) masih `Draft` dan tidak aktif. Dari 22 baris matriks, 0 yang aktif. Baris yang ada adalah sisa matriks uji yang sudah dinonaktifkan |
| K-2 | Terbuka | 17 akses controller `Procurement*` terdaftar di `SysControllerAccess`, tetapi 0 baris `SysAccessPolicy` memberi izin atas controller itu. Sebagai pembanding, 28 jabatan sudah punya izin modul lain |
| K-3 | Terbuka | `owners.security` di manifest sub-modul masih "belum ditetapkan" |
| K-4 | Terbuka | Tidak ada catatan UAT baru; tetap 0/16 |
| K-5, K-6 | Selesai | 17 November 2026 (bagian 8) |

**Contoh dampak K-2 hari ini:** Kepala Instalasi Radiologi masuk dengan jabatan nyatanya. Menu "Pengadaan Barang & Jasa"
tidak tampil, karena jabatannya belum diberi `ProcurementRequest : Read`. Hanya SuperAdmin yang melihat menu itu.

### 9.3 Langkah berikutnya (tidak berubah)

1. Product Owner + admin `Wfl`: isi matriks `PBJ-*` dari SOP lalu aktifkan definisinya (K-1, `PBJ-OQ-019`).
2. Admin Akses Role: terapkan bundel `PBJ-DEC-039` (K-2).
3. Product Owner: tunjuk pemilik keamanan (K-3).
4. UAT bersama pengguna (K-4), lalu `verify-module-readiness` ulang untuk sign-off.
