# Pengadaan Barang & Jasa — Penilaian Kelengkapan Kebutuhan

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 1, `decision_revision 3` (`PBJ-DEC-001`..`026`) |
| Revisi penilaian | `2` — blocker revisi 1 ditutup `PBJ-DEC-027`..`030` pada hari yang sama |
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
