# Manajemen Vendor — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk / modul | Quilvian V2 · Pengadaan Barang & Jasa · sub-modul `manajemen-vendor` |
| Dokumen | `PBJ-VD-MVP` · versi `1.0` · status **`approved`** — pengguna sesi, 17 November 2026 (`PBJ-DEC-062`) |
| Repository target | `NewQuilvianSystemBackend` @`c6b86a6e` (branch `sukmagp`), `V2QuilvianSystemFrontendDev` @`059b6c612` (branch `sukmagpV2`) |
| Masukan | `02-backend-architecture.md`, `03-frontend-architecture.md`, `contracts/*` 1.0, `data/data-dictionary.md`, `flowcharts/*`; `PBJ-DEC-010`, `015`, `021`, `043`..`054`, `059`..`061`; `PBJ-ASM-001`..`004` |
| Cakupan | K-06 data pendamping vendor dan K-11 penilaian berkala manual (`S-V1`..`S-V6`) |

## 2. Ringkasan eksekutif

Pengadaan saat ini hanya punya identitas vendor di Master Data. Rilis ini menambahkan apa yang dibutuhkan Tim Pengadaan
sebelum memilih vendor: kategori yang dilayani, dokumen legalitas yang terverifikasi dan dipantau masa berlakunya, status
Dibatasi/Diblokir yang sama di seluruh sistem, serta penilaian kinerja berkala. Hasil akhirnya adalah satu jawaban yang
dapat dipakai sub-modul `pemilihan-penyedia`: "vendor ini layak dipilih untuk kategori ini atau tidak, dan mengapa".

## 3. Masalah produk

| Masalah | Dampak hari ini |
| --- | --- |
| Legalitas vendor tidak tercatat terpusat | Pembelian obat/alkes dari vendor dengan izin mati tidak tercegah |
| Blokir vendor hanya centang di Master Data | Tidak ada alasan, penyetuju, atau riwayat; Pengadaan tidak bisa menindak dengan bukti |
| Tidak ada penilaian kinerja | Keputusan menahan vendor bergantung ingatan |

## 4. Visi produk

Satu profil vendor Pengadaan yang menempel pada identitas Master Data, sehingga setiap pemilihan vendor didukung
legalitas terverifikasi dan riwayat kinerja yang dapat ditelusuri.

## 5. Batas MVP

| Titik | Isi |
| --- | --- |
| Mulai | Vendor sudah ada di Master Data Supplier |
| Akhir | Vendor punya kategori, dokumen terverifikasi, status yang sah, dan penilaian final; kelayakannya dapat ditanyakan per kategori |

## 6. Pelaku sasaran

| Pelaku | Kebutuhan |
| --- | --- |
| Tim Pengadaan | Mencatat dan memverifikasi dokumen, mengatur kategori, menilai vendor |
| Manajer Pengadaan | Membatasi vendor, mengusulkan blokir, memfinalkan penilaian |
| Direktur / penyetuju | Memutuskan usulan blokir dari kotak masuk persetujuan |
| Admin Pengadaan | Mengatur jenis dokumen, dokumen wajib, dan kriteria penilaian |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID asal | Prioritas |
| --- | --- | --- |
| Identitas vendor dipakai ulang | `PBJ-CAP-009` | `MUST HAVE` (`EXISTING / REUSE`) |
| Status blokir satu sumber | `PBJ-CAP-010` | `MUST HAVE` (`EXTEND`) |
| Rekening tersamarkan | `PBJ-CAP-011` | `MUST HAVE` (adapter) |
| Kategori, dokumen legalitas, Dibatasi, riwayat | K-06 | `MUST HAVE` |
| Penilaian berkala manual | K-11 (`PBJ-DEC-044`) | `MUST HAVE` |

## 8. Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti selama MVP |
| --- | --- | --- |
| Evaluasi per kontrak, rekap keterlambatan dan NCR (`S-V7`) | Datanya lahir di `kontrak-pelaksanaan` yang belum dirancang (`PBJ-DEC-044`) | Penilaian berkala manual; catatan penilai merangkum kejadian |
| Notifikasi aktif kedaluwarsa | Kebijakan notifikasi `PBJ-OQ-015` belum ditetapkan | Tanda "Segera kedaluwarsa" di daftar dan ringkasan |
| Submenu menu 10 selain daftar penilaian | Nama SOURCE belum tercatat (`PBJ-OQ-033`) dan bergantung `S-V7` | Daftar penilaian berkala |

## 9. Alur bisnis target

Lihat `flowcharts/00-alur-utama.md`: vendor dari Master Data → kategori → dokumen dan verifikasi → layak dipilih →
penilaian berkala → tindakan Dibatasi atau usulan blokir yang disetujui.

## 10. Epic dan functional requirement

### `EPIC VD-01` — Konfigurasi vendor · disposisi `MISSING / NEW`

- FR-VD-001 Admin mengelola jenis dokumen legalitas beserta wajib tidaknya tanggal berlaku (`PBJ-ASM-001`).
- FR-VD-002 Admin mengaktifkan dokumen wajib per kategori secara bertahap; daftar dimulai kosong (`PBJ-DEC-061`).
- FR-VD-003 Admin mengelola kriteria dan bobot per kategori; layar menampilkan total bobot (`PBJ-DEC-050`).

### `EPIC VD-02` — Profil, kategori, rekening · disposisi `EXISTING / REUSE` + `MISSING / NEW`

- FR-VD-010 Daftar dan detail vendor membaca identitas dari Master Data dan menampilkan status hitung.
- FR-VD-011 Tim Pengadaan mengatur kategori vendor; profil lahir saat pertama dipakai (`PBJ-DEC-052`).
- FR-VD-012 Rekening hanya tampil tersamarkan, tidak dapat diubah (`PBJ-DEC-015`).

### `EPIC VD-03` — Dokumen legalitas dan kelayakan · disposisi `MISSING / NEW`

- FR-VD-020 Catat dokumen dan unggah berkas lewat lampiran Pengadaan.
- FR-VD-021 Verifikasi/tolak oleh petugas lain (`PBJ-DEC-046`).
- FR-VD-022 Keadaan berlaku dihitung; tanda 30 hari (`PBJ-ASM-002`).
- FR-VD-023 Kelayakan per kategori untuk pemilihan vendor (`PBJ-DEC-045`, `052`, `054`).

### `EPIC VD-04` — Status vendor · disposisi `EXTEND`

- FR-VD-030 Manajer menetapkan dan mencabut Dibatasi (`PBJ-DEC-047`).
- FR-VD-031 Usulan blokir/buka blokir lewat approval `Wfl`; hasilnya menulis Master Data (`PBJ-DEC-021`).
- FR-VD-032 Master Data Supplier berhenti mengubah blokir langsung (`PBJ-DEC-060`).

### `EPIC VD-05` — Penilaian berkala · disposisi `MISSING / NEW`

- FR-VD-040 Penilaian per vendor × kategori × periode dengan salinan kriteria (`PBJ-DEC-059`, `050`).
- FR-VD-041 Draf → final oleh orang lain; revisi menggantikan (`PBJ-DEC-049`).
- FR-VD-042 Rekomendasi hanya saran (`PBJ-DEC-048`).

### `EPIC VD-06` — Riwayat vendor · disposisi `EXISTING / REUSE`

- FR-VD-050 Riwayat dirakit dari kejadian di `PrcDocumentHistory` (`PBJ-ASM-004`), termasuk status blokir awal.

## 11. Model status yang diusulkan

Lihat `contracts/state-transition-matrix.md`: status vendor dihitung (Aktif, Dibatasi, Diblokir, Nonaktif di Master
Data); dokumen `PENDING → VERIFIED/REJECTED/CANCELLED`; usulan `DRAFT → PENDING_APPROVAL → APPROVED/REJECTED/RETURNED/
CANCELLED`; penilaian `DRAFT → FINAL → SUPERSEDED`, `DRAFT → CANCELLED`.

## 12. Sasaran arsitektur

Tiga master `Mst*` di folder `MasterData`, enam tabel `PrcVendor*` di folder baru `Vendor` (butuh registry `PBJ-OQ-037`),
satu service baru di modul Administrator, tanpa perubahan skema `MstSupplier`. Rincian di `02-backend-architecture.md`.

## 13. Sasaran kemampuan API

Empat grup vendor (Vendor, Vendor Document, Vendor Status Request, Vendor Evaluation) dan tiga grup master, seluruhnya
`Rencana (belum tersedia)`; satu perubahan perilaku `Supplier` create/update. Rincian di `contracts/api-contract.md`.

## 14. Matriks kewenangan

Lihat `contracts/permission-audit-matrix.md` bagian 3.

## 15. Batas integrasi dan billing

Tidak ada sistem luar dan tidak ada tagihan pasien. Pembayaran vendor tetap di Finance; Finance membaca status blokir dari
Master Data seperti sebelumnya.

## 16. Guardrail regulasi

Izin PBF dan izin distribusi alat kesehatan adalah syarat legal penyalur; daftar dokumen dan kewajibannya diisi dari SOP
(`PBJ-OQ-019`). Sistem tidak menafsirkan regulasi; ia menegakkan daftar yang diaktifkan admin.

## 17. Kebutuhan non-fungsional

| Butir | Sasaran |
| --- | --- |
| Kecepatan daftar vendor | Paginasi server; saringan tanggal/periode `PBJ-DEC-055` |
| Privasi | Rekening tersamarkan di server |
| Konsistensi | Status blokir satu sumber; penerapan keputusan dalam satu transaksi |
| Ketertelusuran | Setiap perubahan tercatat di riwayat vendor |

## 18. Skenario UAT

| ID | Epic | Prasyarat | Langkah | Hasil diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-VD-01` | VD-01 | — | Admin menambah jenis "Izin PBF" wajib tanggal berlaku | Tersimpan |
| `UAT-VD-02` | VD-01 | Bobot Obat 40+40 | Petugas membuat penilaian vendor Obat | Ditolak "Total bobot … harus 100%" |
| `UAT-VD-03` | VD-02 | Supplier tanpa profil | Atur kategori Alat Kesehatan | Profil dan kategori tersimpan; riwayat mencatat status awal |
| `UAT-VD-04` | VD-02 | Rekening 1234567890 | Buka tab Rekening | Tampil `****7890`, tidak bisa diubah |
| `UAT-VD-05` | VD-03 | — | Andi mencatat izin PBF lalu mencoba memverifikasi sendiri | Ditolak; Sari memverifikasi → Terverifikasi |
| `UAT-VD-06` | VD-03 | Kewajiban Izin PBF aktif untuk Obat | Izin kedaluwarsa | Vendor tidak layak untuk Obat |
| `UAT-VD-07` | VD-04 | — | Manajer membatasi tanpa alasan | Ditolak; dengan alasan → Dibatasi |
| `UAT-VD-08` | VD-04 | Approval vendor dikonfigurasi | Usulan blokir disetujui Direktur | Vendor Diblokir di Master Data dan Pengadaan |
| `UAT-VD-09` | VD-04 | Vendor diblokir | Admin mencoba membuka centang Blacklist di Master Data | Tidak bisa diubah |
| `UAT-VD-10` | VD-05 | Kriteria 100% | Penilai memfinalkan sendiri | Ditolak; Manajer memfinalkan → Final |
| `UAT-VD-11` | VD-05 | Penilaian final ada | Buat penilaian kedua periode sama | Ditolak; diarahkan ke revisi |
| `UAT-VD-12` | VD-06 | Beberapa kejadian | Buka Riwayat vendor | Kejadian urut waktu dengan pelaku dan alasan |

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Registry submodule `Vendor` disetujui sebelum model pertama | `AT-VD-60`; `PBJ-OQ-037` |
| Identitas vendor tidak diduplikasi | `AT-VD-01` |
| Rekening tidak pernah keluar utuh | `AT-VD-03` |
| Pengunggah tidak memverifikasi sendiri; penilai tidak memfinalkan sendiri | `AT-VD-12`, `AT-VD-33` |
| Dokumen wajib kedaluwarsa membuat vendor tidak layak; masa awal tidak mengunci | `AT-VD-14`, `AT-VD-15` |
| Blokir hanya lewat approval; Master Data terkunci | `AT-VD-24`..`26` |
| Penilaian unik per vendor × kategori × periode dan bobot tersalin | `AT-VD-31`, `AT-VD-32` |
| Layar daftar mengikuti `PBJ-FE-004` | `AT-VD-63` |
| Seeder idempoten, kewajiban dokumen kosong | `AT-VD-64` |
| Build dan skema sesuai kamus data | `AT-VD-61`, `AT-VD-62` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Epic | Syarat mulai |
| --- | --- | --- | --- |
| `MVP-V0` | Registry, migration, master konfigurasi, profil dan kategori, rekening | VD-01, VD-02, VD-06 | Blueprint disetujui; `PBJ-OQ-037` |
| `MVP-V1` | Dokumen legalitas, verifikasi, kelayakan | VD-03 | `MVP-V0` |
| `MVP-V2` | Dibatasi, usulan blokir, kunci Master Data | VD-04 | `MVP-V0`; dirilis bersama perubahan `SupplierController` |
| `MVP-V3` | Penilaian berkala | VD-05 | `MVP-V0` |
| `POST-MVP` | `S-V7`, notifikasi aktif, submenu menu 10 lain | — | `kontrak-pelaksanaan`, `PBJ-OQ-015`, `PBJ-OQ-033` |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| ~~`PBJ-OQ-037`~~ — ditutup `PBJ-DEC-063` (registry `Vendor` `ACTIVE`) | — | — | — |
| `PBJ-OQ-033` — nama submenu menu 10 | Pengguna sesi (BRD B10) | Label menu penilaian belum final | Tidak (backend); label layar saja |
| `PBJ-OQ-034` — nilai awal kriteria dan bobot | Pengguna sesi (SOP) | Seeder memakai contoh `PBJ-DEC-050` | Tidak |
| `PBJ-OQ-019` — penyetuju blokir, jenis dan kewajiban dokumen | Pengguna sesi (SOP) | Usulan blokir dijawab `PBJ-VAL-091` sampai dikonfigurasi | Tidak (fail-closed) |
| `PBJ-OQ-015` — notifikasi | Pengguna sesi | Hanya tanda di layar | Tidak |
