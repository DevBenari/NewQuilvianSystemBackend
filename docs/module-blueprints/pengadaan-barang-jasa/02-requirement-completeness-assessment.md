# Pengadaan Barang & Jasa — Penilaian Kelengkapan Kebutuhan

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 1, `decision_revision 3` (`PBJ-DEC-001`..`026`) |
| Revisi penilaian | `4` — revisi 4 (17 November 2026) menutup blocker `manajemen-vendor`, bagian 9; revisi 3 menambah sub-modul `manajemen-vendor`, bagian 8; revisi 2 = blocker revisi 1 ditutup `PBJ-DEC-027`..`030` |
| Tanggal | 12 November 2026 |
| Scope | Sub-modul `perencanaan-permintaan` (K-01..K-04) dan fondasi tingkat modul yang dibutuhkannya |
| Bukti yang dipakai | (1) Jawaban pemilik pada `00-interview-decisions.md`, wewenang tertinggi; (2) BRD & PRD v1.0 DRAFT, berstatus **PROPOSAL** karena belum disahkan; (3) `01-existing-capability-map.md` revisi 1 @`100c011d`/`763d665b` untuk kemampuan yang sudah ada. SOP rumah sakit **belum diterima** (`PBJ-OQ-019`). Baseline `indonesia-hospital-domain-reference` tidak dipakai |
| Verdict keseluruhan | **`READY_FOR_DOMAIN_DESIGN`** (revisi 2); revisi 1 = `PARTIALLY_READY` |

> **Cara membaca.** Setiap butir diberi status bukti:
> - `CONFIRMED`: dikonfirmasi pemilik.
> - `PROPOSED`: usulan BRD atau usulan agent, belum dikonfirmasi.
> - `MISSING`: belum ada informasinya.
> - `CONFLICT`: sumber saling bertentangan.
>
> Setiap gap diberi dampak:
> - `BLOCKING`: harus diputuskan dulu.
> - `NON_BLOCKING_STANDARD`: usulan standar yang aman, tetap terlihat.
> - `CONFIGURABLE_DEFAULT`: dijadikan pengaturan dengan nilai awal.

---

## 1. Slice yang dinilai

| Slice | Isi | Keputusan pendukung |
| --- | --- | --- |
| `S-F1` | Perluasan mesin approval `Wfl` untuk jenis dokumen Pengadaan | `PBJ-DEC-005`, `007`, `016`, `019`, `026` |
| `S-F2` | Katalog barang/jasa dan jenis pengadaan | `PBJ-DEC-022`, `024` |
| `S-F3` | Lampiran berkas Pengadaan | `PBJ-DEC-023` |
| `S-F4` | Penomoran dokumen | `PBJ-DEC-025` |
| `S-F5` | Pembatasan data per unit | `PBJ-DEC-020` |
| `S-01` | Rencana kebutuhan tahunan, konsolidasi, revisi (K-01 tanpa rencana paket) | `PBJ-DEC-003`, `014` |
| `S-01P` | Rencana Paket Pengadaan (submenu 2.4) | — |
| `S-02` | Pagu anggaran dan sisa pagu (K-02) | `PBJ-DEC-009` |
| `S-03` | Permintaan Pengadaan: pengajuan, verifikasi, mendesak, riwayat (K-03) | `PBJ-DEC-007`, `014`, `016` |
| `S-04` | Cek stok manual (K-04) | `PBJ-DEC-008` |

## 2. Temuan per dimensi

Kolom berisi status bukti. `—` berarti dimensi tidak berlaku secara material, dengan alasan di bagian 2.1.

| Dimensi | S-F1 | S-F2 | S-F3 | S-F4 | S-F5 | S-01 | S-01P | S-02 | S-03 | S-04 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 01 Tujuan | C | C | C | C | C | C | P | C | C | C |
| 02 Aktor | C | P (G-07) | C | C | P (G-08) | P (G-05) | M | P (G-05) | C | **M (G-01)** |
| 03 Pemicu/prasyarat | C | C | C | C | C | P | M | P | C | **M (G-01)** |
| 04 Alur utama | C | P | C | C | C | P | M | P | **M (G-01)** | **M (G-01)** |
| 05 Alur alternatif | C | P | C | C | P | P (G-05) | M | P | P (G-04) | C |
| 06 Data minimum | P | C | P | C | C | P | M | P | P | C |
| 07 Aturan/validasi | C | C | P (G-09) | C | C | P | M | **M (G-03)** | **M (G-02, G-03)** | C |
| 08 Status | C | P | P | C | — | P (G-05) | M | P | **M (G-01)** | C |
| 09 Peran/otorisasi | C | P (G-07) | C | — | P (G-08) | P | M | P | C | **M (G-01)** |
| 10 Dependency | C | C | C | C | C | C | **CONFLICT (G-06)** | C | C | C |
| 11 Integrasi | C | C | — | C | C | — | — | P (G-10) | — | C |
| 12 Hasil akhir | C | C | C | C | C | P | M | C | **M (G-01)** | C |
| 13 Batal/koreksi | C | P | C | C | — | P (G-05) | M | P | P (G-04) | P |
| 14 Audit/histori | C | P | C | C | C | P (G-05) | M | P | C | C |
| 15 Notifikasi | P (G-11) | — | — | — | — | — | — | — | P (G-11) | — |
| 16 Billing | — | — | — | — | — | — | — | — | — | — |
| 17 Keselamatan klinis | — | — | — | — | — | — | — | — | P | — |
| 18 Pelaporan | C | P | — | — | — | C | M | C | C | C |

C = `CONFIRMED`, P = `PROPOSED`, M = `MISSING`.

### 2.1 Dimensi yang tidak berlaku

- **16 Billing:** pengadaan tidak membuat tagihan pasien. Dampak keuangannya (komitmen anggaran) dicatat di dimensi 07/12 `S-02`. Utang tetap di Finance (`PBJ-DEC-012`).
- **17 Keselamatan klinis:** tidak ada data klinis pasien. Satu-satunya titik sentuh adalah PR mendesak untuk alat medis kritis (`S-03`). Jalurnya sudah dikonfirmasi (`PBJ-DEC-016`), dan tidak ada persetujuan setelah kejadian.
- **11 Integrasi** tidak berlaku untuk `S-F3`, `S-01`, `S-03`: semuanya internal Quilvian.

## 3. Gap dan dampaknya

| Gap | Slice | Status | Uraian | Dampak | Keputusan |
| --- | --- | --- | --- | --- | --- |
| `G-01` | S-03, S-04 | `MISSING` | **Urutan langkah PR belum diputuskan:** kapan verifikasi kebutuhan oleh Tim Pengadaan, kapan cek stok, kapan approval matriks, dan siapa yang mencatat cek stok. BRD hanya mengusulkan "PR → cek stok → verifikasi & anggaran → approval". Urutan ini menentukan status PR dan siapa yang boleh bertindak di tiap status. **Contoh beda hasil:** bila cek stok dilakukan setelah approval, PR 20 kursi roda disetujui senilai 20 unit padahal 5 unit dipenuhi dari stok, sehingga nominal yang menentukan jenjang approval menjadi salah | `BLOCKING` (status, otorisasi, nominal approval) | `PBJ-OQ-026` |
| `G-02` | S-03 | `MISSING` | **Hubungan PR dengan baris rencana:** apakah PR "mengurangi" sisa kuantitas rencana, dan apakah PR yang melebihi sisa rencana dianggap "Di luar rencana"? **Contoh:** rencana IGD 2027 = 10 monitor. PR pertama 6 unit, PR kedua 6 unit. Apakah PR kedua dianggap 4 unit dalam rencana dan 2 di luar rencana? | `BLOCKING` (relasi data, aturan `PBJ-DEC-014`) | `PBJ-OQ-028` |
| `G-03` | S-02, S-03 | `MISSING` | **Perilaku PR bila estimasi melebihi sisa pagu**, dan **kapan komitmen dicatat.** `PBJ-DEC-009` menyebut komitmen = nilai penetapan vendor, sehingga PR belum mengikat pagu. Namun belum diputuskan apakah PR tetap boleh diajukan bila estimasinya lebih besar dari sisa pagu. **Contoh:** sisa pagu Radiologi Rp 30 juta, PR baru Rp 45 juta: ditolak, diberi peringatan, atau perlu jenjang approval tambahan? | `CONFIGURABLE_DEFAULT`, dengan nilai awal yang harus dikonfirmasi | `PBJ-OQ-029` |
| `G-04` | S-03 | `PROPOSED` | Pembatalan dan koreksi PR. Usulan: pengusul boleh membatalkan selama belum disetujui; setelah disetujui dan sebelum masuk paket, hanya Tim Pengadaan dengan alasan; PR yang dikembalikan (`RETURNED`) kembali ke draft dan riwayatnya disimpan; PR yang ditolak tidak bisa dihidupkan lagi | `NON_BLOCKING_STANDARD` | Dikonfirmasi saat desain |
| `G-05` | S-01, S-02 | `PROPOSED` | Siklus rencana dan pagu. Usulan BRD: unit menyusun rencana, Tim Pengadaan mengonsolidasi, matriks menyetujui; rencana yang disetujui tidak boleh ditimpa, sehingga perubahan dibuat sebagai versi revisi yang juga disetujui; pagu disusun Tim Pengadaan/Keuangan per unit × kategori × tahun dan disetujui lewat matriks | `NON_BLOCKING_STANDARD` | Dikonfirmasi saat desain |
| `G-06` | S-01P | `CONFLICT` | **"Rencana Paket Pengadaan" (submenu 2.4) dan "Paket" pada sub-modul `pemilihan-penyedia`.** BRD memakai paket untuk dua hal: rencana paket di menu 2, dan paket yang dieksekusi di menu 4/6 (status `PLANNING → PREPARATION → ...`). Pada bentuk `COMPOSITE`, dua sub-modul **tidak boleh** memiliki tabel yang sama (`bentuk-blueprint.md` 4.2). Perlu diputuskan apakah rencana paket adalah entity perencanaan sendiri yang kelak "diwujudkan" menjadi paket, atau paket yang sama milik `pemilihan-penyedia`. Ini juga menentukan batas serah PR → paket (`CONVERTED_TO_PACKAGE`) | `BLOCKING` (kepemilikan data lintas sub-modul) | `PBJ-OQ-027` |
| `G-07` | S-F2 | `PROPOSED` | Pengelola katalog: Tim Pengadaan menambah dan menonaktifkan; kode unik dibuat server (`KTL-00045`); barang yang sudah dipakai tidak dihapus, hanya dinonaktifkan; daftar kategori dikonfigurasi | `NON_BLOCKING_STANDARD` | — |
| `G-08` | S-F5 | `PROPOSED` | Peran lintas unit (Tim Pengadaan, Auditor, Keuangan) diberikan lewat izin khusus "lihat semua unit". Pengguna dengan beberapa penempatan memilih unit saat membuat PR, dari unit penempatannya saja | `NON_BLOCKING_STANDARD` | — |
| `G-09` | S-F3 | `PROPOSED` | Nilai awal tipe dan ukuran berkas. Usulan: PDF, JPG, PNG, DOCX, XLSX; maksimal 10 MB per berkas | `CONFIGURABLE_DEFAULT` | — |
| `G-10` | S-02 | `PROPOSED` | Sumber dana: belum ada master. Usulan: daftar sumber dana dikonfigurasi di Pengadaan (mis. Operasional, Investasi) | `CONFIGURABLE_DEFAULT` | — |
| `G-11` | S-F1, S-03 | `PROPOSED` | Notifikasi: penyetuju diberi tahu saat tugas masuk, pengusul saat keputusan keluar, memakai gerbang notifikasi `Wfl` yang ada (`UnifiedNotificationGatewayService`). Isi notifikasi tanpa nominal | `NON_BLOCKING_STANDARD` | — |
| `G-12` | Semua | `MISSING` | Prefix entity dan Area di registry (`PBJ-OQ-002`). Desain boleh memakai usulan `Prc` / `Corporate/ProcurementManagement` dengan tanda "menunggu registry" | Memblokir **implementasi**, bukan desain | `PBJ-OQ-002` |
| `G-13` | S-F1 | `PROPOSED` | Nilai awal matriks (`PBJ-OQ-019`): jenjang dan ambang dari SOP | `CONFIGURABLE_DEFAULT` | `PBJ-OQ-019` |

## 4. Log keputusan pemblokir

| Decision ID | Pertanyaan | Slice terdampak | Bukti saat ini | Usulan | Dampak | Pemilik | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PBJ-OQ-026` | Urutan verifikasi, cek stok, dan approval PR; siapa yang mencatat cek stok | S-03, S-04 | BRD A6 (PROPOSAL) | Pengajuan → verifikasi Tim Pengadaan (kelengkapan + cek stok + hitung kebutuhan beli) → approval matriks atas **nilai kebutuhan beli** → selesai | Status PR, otorisasi, nominal penentu jenjang | Pengguna sesi | `OPEN` |
| `PBJ-OQ-027` | Rencana Paket (2.4) entity perencanaan sendiri, atau paket yang sama milik `pemilihan-penyedia`? | S-01P, batas PR → paket | BRD B3 2.4, B4.2 | Rencana paket milik `perencanaan-permintaan`; paket eksekusi milik `pemilihan-penyedia` dan dibuat **dari** rencana paket atau PR yang disetujui | Kepemilikan tabel lintas sub-modul | Pengguna sesi | `OPEN` |
| `PBJ-OQ-028` | Apakah PR mengurangi sisa rencana, dan bagaimana kelebihannya diperlakukan? | S-03 | `PBJ-DEC-014` | PR mengurangi sisa kuantitas baris rencana; bagian yang melebihi sisa ditandai "Di luar rencana" per baris | Relasi data, laporan penyimpangan | Pengguna sesi | `OPEN` |
| `PBJ-OQ-029` | PR melebihi sisa pagu: tolak, peringatan, atau jenjang tambahan? | S-02, S-03 | `PBJ-DEC-009` | Boleh diajukan dengan peringatan, dan matriks menambah jenjang "melebihi pagu" (dapat dikonfigurasi) | Aturan approval | Pengguna sesi | `OPEN` |

## 5. Kesiapan per slice

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| `S-F1` Perluasan `Wfl` | `READY_FOR_DOMAIN_DESIGN` | Semua keputusan ada; nilai awal matriks bisa dikonfigurasi |
| `S-F2` Katalog | `READY_FOR_DOMAIN_DESIGN` | `G-07` standar |
| `S-F3` Lampiran | `READY_FOR_DOMAIN_DESIGN` | `G-09` dapat dikonfigurasi |
| `S-F4` Penomoran | `READY_FOR_DOMAIN_DESIGN` | — |
| `S-F5` Pembatasan unit | `READY_FOR_DOMAIN_DESIGN` | `G-08` standar |
| `S-01` Rencana kebutuhan, konsolidasi, revisi | `READY_FOR_DOMAIN_DESIGN` | `G-05` standar dan tetap terlihat |
| `S-01P` Rencana Paket | `BUSINESS_DECISION_REQUIRED` | `PBJ-OQ-027` |
| `S-02` Pagu | `READY_FOR_DOMAIN_DESIGN` untuk struktur pagu; aturan PR melebihi pagu ikut `S-03` | `PBJ-OQ-029` hanya memengaruhi validasi PR |
| `S-03` Permintaan Pengadaan | `BUSINESS_DECISION_REQUIRED` | `PBJ-OQ-026`, `PBJ-OQ-028`, `PBJ-OQ-029` |
| `S-04` Cek stok manual | `BUSINESS_DECISION_REQUIRED` | `PBJ-OQ-026` |

**Verdict sub-modul `perencanaan-permintaan`: `PARTIALLY_READY`.**

- **Boleh lanjut ke desain:** `S-F1`..`S-F5`, `S-01`, dan struktur `S-02`.
- **Harus berhenti:** `S-01P`, `S-03`, `S-04`.
- **Ketergantungan:** `S-03` bergantung pada `S-F1`..`S-F5`, `S-01`, dan `S-02`. `S-04` adalah langkah di dalam `S-03`. `S-01P` bergantung pada keputusan batas sub-modul `pemilihan-penyedia`.

Karena PR (`S-03`) adalah inti sub-modul ini, sebaiknya keempat keputusan di bagian 4 ditutup lebih dulu
lewat `grill-me`, lalu seluruh sub-modul dirancang sekaligus. Ini menghindari rancangan fondasi yang
harus diubah lagi.

## 6. Handoff

| Field | Nilai |
| --- | --- |
| Skill berikutnya | `grill-me` (amendment pass) untuk `PBJ-OQ-026`..`029`, lalu `design-business-module` |
| `hospital-domain-architect` | Tidak dipakai: `DOMAIN_ARCHITECTURE_NOT_RUN`. Alasan: kemampuan ini administratif, tanpa data klinis maupun billing pasien. Lintas konteksnya (Wfl, Master Data, Finance) sudah diputuskan eksplisit di `PBJ-DEC-019`..`021` |
| `requirement_readiness` | `PARTIALLY_READY` |
| Blocker terbuka | `PBJ-OQ-026`, `027`, `028`, `029` (desain); `PBJ-OQ-002` (implementasi) |

---

## 7. Revisi 2 — setelah blocker ditutup (12 November 2026)

| Decision ID | Ditutup oleh | Gap |
| --- | --- | --- |
| `PBJ-OQ-026` | `PBJ-DEC-027` | `G-01` → `CONFIRMED` |
| `PBJ-OQ-027` | `PBJ-DEC-028` | `G-06` → `CONFIRMED` |
| `PBJ-OQ-028` | `PBJ-DEC-029` | `G-02` → `CONFIRMED` |
| `PBJ-OQ-029` | `PBJ-DEC-030` | `G-03` → `CONFIRMED` |

| Slice | Kesiapan revisi 2 |
| --- | --- |
| `S-F1`..`S-F5`, `S-01`, `S-02` | `READY_FOR_DOMAIN_DESIGN` (tidak berubah) |
| `S-01P` Rencana Paket | `READY_FOR_DOMAIN_DESIGN` |
| `S-03` Permintaan Pengadaan | `READY_FOR_DOMAIN_DESIGN` |
| `S-04` Cek stok manual | `READY_FOR_DOMAIN_DESIGN` |

**Verdict sub-modul `perencanaan-permintaan`: `READY_FOR_DOMAIN_DESIGN`.**

Butir yang **tetap `PROPOSED`** dan wajib tampil sebagai usulan (bukan kebijakan terkonfirmasi) di desain:
`G-04` (pembatalan/koreksi PR), `G-05` (siklus rencana dan pagu), `G-07` (pengelola katalog), `G-08`
(peran lintas unit), `G-11` (notifikasi). Nilai yang dapat dikonfigurasi: `G-09` (tipe/ukuran berkas),
`G-10` (sumber dana), `G-13` (nilai awal matriks dari SOP). Pemblokir implementasi yang tersisa: `PBJ-OQ-002`.

**Handoff:** `design-business-module`, dengan `requirement_readiness: READY_FOR_DOMAIN_DESIGN` dan
`domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN` (alasan di bagian 6).

---

## 8. Revisi 3 — sub-modul `manajemen-vendor` (17 November 2026)

| Field | Nilai |
| --- | --- |
| Scope | Sub-modul `manajemen-vendor`: K-06 data pendamping vendor dan K-11 penilaian vendor (`PBJ-DEC-043`) |
| Bukti yang dipakai | (1) Jawaban pemilik di `00-interview-decisions.md`: `PBJ-DEC-010`, `015`, `021`, `026`, `043`..`054`, `PBJ-ASM-001`..`004` (disetujui 17 November 2026) — wewenang tertinggi; (2) BRD & PRD v1.0 DRAFT menu 5 dan 10 — **PROPOSAL**; (3) `01-existing-capability-map.md` `PBJ-CAP-009`..`011`, dicek ulang terhadap backend `c6b86a6e` (tidak ada commit pada `MstSupplier`/`SupplierController`/`SupplierDtos` sejak `100c011d`); (4) source `SupplierController.cs` baris 469 dan 575 (lihat `G-V08`) |
| `indonesia-hospital-domain-reference` | Tidak dipakai: aturan legalitas vendor diputuskan langsung oleh pemilik; tidak ada baseline yang menggantikannya |
| Verdict | **`PARTIALLY_READY`** |

### 8.1 Slice yang dinilai

| Slice | Isi | Keputusan pendukung |
| --- | --- | --- |
| `S-V1` | Profil pendamping vendor dan kategori vendor | `PBJ-DEC-010`, `051`, `052`, `053` |
| `S-V2` | Dokumen legalitas: unggah, verifikasi, masa berlaku, kedaluwarsa | `PBJ-DEC-045`, `046`; `PBJ-ASM-001`, `002` |
| `S-V3` | Status pengadaan: Dibatasi, usulan blokir/buka blokir lewat approval | `PBJ-DEC-021`, `047`, `054`; `PBJ-ASM-003` |
| `S-V4` | Tampilan rekening tersamarkan | `PBJ-DEC-015`; `PBJ-AC-009` |
| `S-V5` | Riwayat vendor | `PBJ-ASM-004` |
| `S-V6` | Penilaian berkala manual dan rekomendasi | `PBJ-DEC-044`, `048`, `049`, `050` |
| `S-V7` | Evaluasi per kontrak, rekap keterlambatan dan NCR otomatis | `PBJ-DEC-044` (sengaja ditunda) |

### 8.2 Temuan per dimensi

| Dimensi | S-V1 | S-V2 | S-V3 | S-V4 | S-V5 | S-V6 | S-V7 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 01 Tujuan | C | C | C | C | C | C | C |
| 02 Aktor | C | C | C | C | C | C | M |
| 03 Pemicu/prasyarat | C | C | C | C | C | C | M |
| 04 Alur utama | C | C | C | C | P | C | M |
| 05 Alur alternatif | P (G-V02) | C | **CONFLICT (G-V08)** | C | P | P (G-V05) | M |
| 06 Data minimum | P (G-V01) | P (G-V03) | C | C | P | **M (G-V06)** | M |
| 07 Aturan/validasi | C | C | C | C | — | **CONFLICT (G-V06)** | M |
| 08 Status | — | C | C | — | — | C | M |
| 09 Peran/otorisasi | C | C | **CONFLICT (G-V08)** | C | C | C | M |
| 10 Dependency | C | C | C | C | P | C | **M (G-V09)** |
| 11 Integrasi | C | — | **CONFLICT (G-V08)** | C | — | — | M |
| 12 Hasil akhir | C | C | C | C | C | C | M |
| 13 Batal/koreksi | P (G-V02) | C | P (G-V07) | — | — | C | M |
| 14 Audit/histori | C | C | P (G-V08) | — | P | C | M |
| 15 Notifikasi | — | P (G-V04) | P (G-V04) | — | — | — | — |
| 16 Billing | — | — | — | — | — | — | — |
| 17 Keselamatan klinis | — | P | — | — | — | — | — |
| 18 Pelaporan | P (G-V10) | P (G-V10) | P (G-V10) | — | C | P (G-V10) | M |

C = `CONFIRMED`, P = `PROPOSED`, M = `MISSING`.

**Dimensi yang tidak berlaku:** 16 Billing — vendor tidak membuat tagihan pasien; pembayaran vendor tetap di Finance
(`PBJ-DEC-012`). 17 Keselamatan klinis — tidak ada data pasien; titik sentuhnya hanya bahwa vendor alkes/obat dengan izin
mati tidak dapat dipilih, dan itu sudah diputuskan (`PBJ-DEC-045`).

### 8.3 Gap dan dampaknya

| Gap | Status | Dampak | Uraian |
| --- | --- | --- | --- |
| `G-V01` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Isi profil pendamping di luar kategori, dokumen, dan status (mis. kontak khusus pengadaan) belum disebut. Usulan: identitas dan kontak tetap dibaca dari `MstSupplier`; profil Pengadaan hanya kategori, dokumen, status, riwayat |
| `G-V02` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Melepas kategori dari vendor. Usulan: boleh, riwayat mencatatnya; dokumen wajib kategori yang dilepas tidak lagi memengaruhi kelayakan |
| `G-V03` | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Daftar jenis dokumen dan dokumen wajib per kategori diisi dari SOP (`PBJ-OQ-019`). **Akibat penting `PBJ-DEC-045`:** bila daftar wajib sudah diisi saat aplikasi mulai dipakai, seluruh vendor lama tanpa dokumen langsung tidak dapat dipilih. Usulan: daftar wajib dimulai kosong per kategori dan diaktifkan bertahap setelah dokumen vendor diunggah |
| `G-V04` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pengingat kedaluwarsa 30 hari dan pemberitahuan hasil approval blokir: tanda di daftar dulu (`PBJ-ASM-002`), notifikasi aktif mengikuti `PBJ-OQ-015` |
| `G-V05` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Draf penilaian boleh dihapus pembuatnya sebelum final; panjang periode penilaian dapat diatur (bawaan semester) |
| `G-V06` | `CONFLICT` | **`BLOCKING`** | `PBJ-DEC-049` menetapkan **satu** penilaian berlaku per vendor per periode, sedangkan `PBJ-DEC-050` menetapkan kriteria **per kategori vendor**, dan satu vendor boleh punya banyak kategori (`PBJ-DEC-052`). **Contoh:** PT Sehat Medika berkategori Alat Kesehatan (kriteria A) dan Umum (kriteria B); set kriteria mana yang dipakai penilaian Semester I 2027? Jawabannya menentukan kunci unik penilaian dan bentuk datanya |
| `G-V07` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Hanya satu usulan blokir/buka blokir terbuka per vendor; usulan dapat ditarik pengusul sebelum diputuskan |
| `G-V08` | `CONFLICT` | **`BLOCKING`** | `PBJ-DEC-021` menetapkan status blokir bersumber satu dan berubah **lewat approval Pengadaan**, dengan alasan dan penyetuju tersimpan. Namun layar Master Data Supplier yang ada masih menulis `IsBlacklisted` dan `BlacklistReason` langsung dari isian (`SupplierController.cs` baris 469 dan 575). Bila keduanya hidup, vendor bisa diblokir atau dibuka tanpa approval dan tanpa riwayat Pengadaan |
| `G-V09` | `MISSING` | `NON_BLOCKING_STANDARD` (untuk slice lain) | Sumber data keterlambatan dan NCR baru ada di `kontrak-pelaksanaan` yang belum dirancang. Sudah diputuskan ditunda (`PBJ-DEC-044`); tidak menahan `S-V1`..`S-V6` |
| `G-V10` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Laporan vendor (vendor kedaluwarsa, rekap penilaian) mengikuti nama submenu menu 10 yang belum tercatat (`PBJ-OQ-033`); hanya memengaruhi desain layar |

### 8.4 Log keputusan pemblokir

| Decision ID | Pertanyaan | Slice terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PBJ-OQ-035` | Untuk vendor berkategori lebih dari satu, penilaian berkala dibuat per vendor (dengan set kriteria apa) atau per vendor × kategori? | `S-V6` | `PBJ-DEC-049` vs `PBJ-DEC-050`, `052` | Penilaian per vendor × kategori × periode, karena kriteria dan dokumen wajib juga per kategori | Kunci unik, bentuk data penilaian, laporan | Pengguna sesi (Product Owner Pengadaan) | `OPEN` |
| `PBJ-OQ-036` | Setelah Pengadaan aktif, apakah layar Master Data Supplier masih boleh mengubah `IsBlacklisted` langsung? | `S-V3` | `PBJ-DEC-021` vs `SupplierController.cs` 469/575 | Kunci kolom blokir di Master Data: hanya dibaca di sana, perubahan lewat usulan Pengadaan; atau tetap boleh tetapi tercatat ke riwayat vendor sebagai "diubah di Master Data" | Kepemilikan kolom lintas modul, otorisasi, integritas riwayat, perubahan modul Administrator | Pengguna sesi (pemilik Master Data Supplier, `PBJ-DEC-026`) | `OPEN` |

### 8.5 Kesiapan per slice

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| `S-V1` Profil dan kategori | `READY_FOR_DOMAIN_DESIGN` | `G-V01`, `G-V02` standar |
| `S-V2` Dokumen legalitas | `READY_FOR_DOMAIN_DESIGN` | `G-V03` dapat dikonfigurasi; `G-V04` standar |
| `S-V3` Status Dibatasi/Diblokir | `BUSINESS_DECISION_REQUIRED` | `PBJ-OQ-036` |
| `S-V4` Rekening tersamarkan | `READY_FOR_DOMAIN_DESIGN` | — |
| `S-V5` Riwayat vendor | `READY_FOR_DOMAIN_DESIGN` untuk kejadian Pengadaan; asal perubahan blokir menunggu `PBJ-OQ-036` | `PBJ-ASM-004` |
| `S-V6` Penilaian berkala | `BUSINESS_DECISION_REQUIRED` | `PBJ-OQ-035` |
| `S-V7` Evaluasi per kontrak | `LATER SLICE` — di luar gelombang ini | Ditunda `PBJ-DEC-044`; menunggu desain `kontrak-pelaksanaan` |

**Verdict sub-modul `manajemen-vendor`: `PARTIALLY_READY`.**

- **Boleh lanjut ke desain:** `S-V1`, `S-V2`, `S-V4`, `S-V5` (kejadian Pengadaan).
- **Harus berhenti:** `S-V3` (`PBJ-OQ-036`), `S-V6` (`PBJ-OQ-035`).
- **Ketergantungan:** kelayakan vendor untuk dipilih menggabungkan `S-V2` (legalitas) dan `S-V3` (status), sehingga
  kontrak kelayakan untuk `pemilihan-penyedia` baru lengkap setelah `PBJ-OQ-036`. `S-V5` mencatat kejadian `S-V2`, `S-V3`,
  dan `S-V6`.

Kedua keputusan itu kecil dan dipegang pemilik yang sama. Sebaiknya keduanya ditutup lewat `grill-me` lebih dulu, lalu
seluruh sub-modul dirancang sekaligus, supaya kontrak kelayakan vendor tidak dirancang dua kali.

### 8.6 Handoff

| Field | Nilai |
| --- | --- |
| Skill berikutnya | `grill-me` (amendment pass) untuk `PBJ-OQ-035`, `PBJ-OQ-036`, lalu `design-business-module` untuk `manajemen-vendor` |
| `hospital-domain-architect` | Tidak dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`): kemampuan administratif tanpa data klinis maupun billing pasien; lintas konteks (Master Data Supplier, Finance, `Wfl`) sudah diputuskan eksplisit (`PBJ-DEC-010`, `015`, `021`) |
| `requirement_readiness` | `PARTIALLY_READY` |
| Butir tetap `PROPOSED` | `G-V01`, `G-V02`, `G-V04`, `G-V05`, `G-V07`, `G-V10` |
| Nilai dapat dikonfigurasi | `G-V03` (jenis dan dokumen wajib per kategori), `PBJ-OQ-034` (kriteria dan bobot), `PBJ-OQ-019` (penyetuju blokir) |
| Standar layar | `PBJ-FE-004` berlaku untuk semua daftar sub-modul ini |

---

## 9. Revisi 4 — blocker `manajemen-vendor` ditutup (17 November 2026)

| Decision ID | Ditutup oleh | Gap |
| --- | --- | --- |
| `PBJ-OQ-035` | `PBJ-DEC-059` | `G-V06` → `CONFIRMED` (penilaian per vendor × kategori × periode) |
| `PBJ-OQ-036` | `PBJ-DEC-060` | `G-V08` → `CONFIRMED` (kolom blokir Master Data dikunci) |
| — | `PBJ-DEC-061` | `G-V03` → `CONFIRMED` (dokumen wajib dimulai kosong, diaktifkan bertahap) |

| Slice | Kesiapan revisi 4 |
| --- | --- |
| `S-V1`, `S-V2`, `S-V4` | `READY_FOR_DOMAIN_DESIGN` (tidak berubah) |
| `S-V3` Status Dibatasi/Diblokir | `READY_FOR_DOMAIN_DESIGN` — termasuk perubahan kecil modul Administrator (`PBJ-DEC-060`) |
| `S-V5` Riwayat vendor | `READY_FOR_DOMAIN_DESIGN` |
| `S-V6` Penilaian berkala | `READY_FOR_DOMAIN_DESIGN` |
| `S-V7` Evaluasi per kontrak | `LATER SLICE` (tidak berubah, `PBJ-DEC-044`) |

**Verdict sub-modul `manajemen-vendor`: `READY_FOR_DOMAIN_DESIGN`** untuk `S-V1`..`S-V6`.

Butir yang tetap `PROPOSED` dan wajib tampil sebagai usulan di desain: `G-V01`, `G-V02`, `G-V04`, `G-V05`, `G-V07`, `G-V10`.
Nilai yang dapat dikonfigurasi: jenis dan dokumen wajib per kategori (`PBJ-DEC-061`), kriteria dan bobot (`PBJ-OQ-034`),
penyetuju blokir (`PBJ-OQ-019`). Pertanyaan terbuka yang hanya memengaruhi layar: `PBJ-OQ-033` (nama submenu menu 10).

**Handoff:** `design-business-module` untuk sub-modul `manajemen-vendor`, dengan `requirement_readiness:
READY_FOR_DOMAIN_DESIGN` dan `domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN` (alasan di bagian 8.6).
