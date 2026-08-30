# Human Resource — Flowchart Alur Proses

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Folder | `flowcharts/` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`) |
| `input_revision` | `00-interview-decisions.md` revision `10`; `contracts/state-transition-matrix.md` `v1` |
| `input_hash` — decision log | `91d62d4ea81aa11fd5bf4c1c922b6c8dbe1ad273a1609e4897bae0ecafa590c0` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |

---

## 1. Apa yang dijawab folder ini

Folder ini menjawab satu pertanyaan yang **tidak dipegang dokumen lain**: **urutan langkah apa
yang dikerjakan orang**, dan apa yang ia lakukan ketika langkahnya gagal.

Pembacanya adalah petugas dan analis, bukan implementer. Karena itu diagram di sini
**MUST NOT** memuat nama tabel, nama kolom, endpoint, maupun nama class.

## 2. Pembagian tanggung jawab dengan dokumen lain

| Pertanyaan | Dijawab oleh |
| --- | --- |
| Urutan langkah dan pelakunya | **Folder ini** |
| Titik keputusan dan syarat percabangannya | **Folder ini** |
| Apa yang petugas lakukan setelah ditolak | **Folder ini** |
| Daftar lengkap perpindahan status yang sah **dan yang tidak sah** | [`../contracts/state-transition-matrix.md`](../contracts/state-transition-matrix.md) |
| Kondisi penolakan beserta kode dan kalimat pesannya | [`../contracts/validation-matrix.md`](../contracts/validation-matrix.md) |
| Arah baca dan tulis antar modul | [`../contracts/integration-contract.md`](../contracts/integration-contract.md) |
| Bentuk tabel dan kolom | [`../data/data-dictionary.md`](../data/data-dictionary.md) |
| Relasi antar entity | [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 4 |

Folder ini **MUST NOT** menyalin isi keempat dokumen pertama. Node penolakan cukup menyebut
**sebabnya** dengan singkat; kalimat pesan yang sebenarnya tetap satu-satunya di validation
matrix.

## 3. Hubungan dengan folder `../flows/`

Kedua folder ini **berbeda isi dan berbeda tujuan**. Keduanya bukan salinan satu sama lain.

| Folder | Isi | Pembaca |
| --- | --- | --- |
| `flowcharts/` — folder ini | Langkah yang dikerjakan orang, digambar sebagai flowchart beserta tabel langkah | Petugas, analis, penguji UAT |
| [`../flows/`](../flows/) | **Bukti dan penalaran** di balik aturan bisnis: purpose, actor, trigger, precondition, aturan yang terbukti dari source, dan penanda provenance `[EXISTING]` / `[DECISION]` / `[OPEN]` / `[BLOCKED]` per aturan | Perancang dan implementer yang perlu tahu **dari mana** sebuah aturan berasal |

`../flows/` adalah keluaran pass `PHASE 2A` sampai `PHASE 2C` yang tercatat pada decision log
revision `6` sampai `10`. Ia **tetap dipertahankan** karena memuat jejak asal-usul yang tidak
ada di tempat lain — termasuk aturan mana yang masih `[OPEN]` dan tidak boleh dijadikan dasar
implementasi.

**Bila keduanya bertentangan**, folder ini yang berlaku untuk **urutan langkah**, dan
`../flows/` yang berlaku untuk **asal-usul aturan**. Pertentangan yang sebenarnya — bukan sekadar
beda kata — **MUST** dicatat sebagai Open Question baru pada decision log, bukan diselesaikan
dengan menyunting salah satunya diam-diam.

## 4. Aturan yang mengikat setiap berkas di folder ini

| Aturan | Alasan |
| --- | --- |
| Satu diagram **MUST** muat dibaca dalam satu layar | Diagram yang perlu digulir tidak dibaca sampai selesai |
| Jalur pengecualian **MUST** digambar, bukan hanya jalur berhasil | Jalur gagal yang paling sering ditemui petugas, dan paling sering lupa dibuatkan layarnya |
| Setiap cabang keputusan **MUST** punya jalur keluar | Cabang buntu menyembunyikan pekerjaan yang belum dirancang |
| Nama status pada node keadaan **MUST** sama persis dengan `../contracts/state-transition-matrix.md` | Agar dua dokumen dapat diperiksa silang tanpa menerjemahkan |
| Diagram **MUST NOT** memuat nama tabel, kolom, endpoint, atau nama class | Pembacanya petugas dan analis |
| Setiap flowchart **MUST** didampingi tabel langkah | Diagram menunjukkan bentuk alur; tabel menjawab pertanyaan implementer |

### 4.1 Bentuk node yang dipakai

| Bentuk | Dipakai untuk |
| --- | --- |
| `([teks])` | Titik mulai dan titik selesai |
| `[teks]` | Langkah yang dikerjakan pelaku |
| `{teks}` | Titik keputusan; label cabang menyebut jawabannya |
| `[/teks/]` | Penolakan yang dilihat petugas di layar |
| `[(teks)]` | Keadaan setelah langkah itu — namanya sama persis dengan state-transition matrix |

## 5. Daftar berkas

| Berkas | Proses | Slice | Kesiapan |
| --- | --- | --- | --- |
| [`00-alur-utama.md`](./00-alur-utama.md) | Alur pokok modul ujung ke ujung, jalur normal saja | seluruh `READY` | `READY FOR DESIGN` |
| [`administrasi-kepegawaian.md`](./administrasi-kepegawaian.md) | Perubahan data pegawai | `S-A1` | `READY FOR DESIGN` |
| [`kehadiran-harian.md`](./kehadiran-harian.md) | Rekaman mentah sampai kehadiran harian siap payroll | `S-A5`, `S-B1` | `READY FOR DESIGN` |
| [`koreksi-kehadiran.md`](./koreksi-kehadiran.md) | Permohonan koreksi kehadiran | `S-B1` | `READY FOR DESIGN` |
| [`cuti.md`](./cuti.md) | Permohonan cuti sampai saldo diselesaikan | `S-A2`, `S-B2` | `READY FOR DESIGN` |
| [`lembur.md`](./lembur.md) | Permohonan lembur sampai realisasi diverifikasi | `S-A3`, `S-B3` | `READY FOR DESIGN` |
| [`penjadwalan-kerja.md`](./penjadwalan-kerja.md) | Roster disusun sampai jadwal terbit | `S-B4` | `READY FOR DESIGN` |
| [`ubah-jadwal-dan-tukar-shift.md`](./ubah-jadwal-dan-tukar-shift.md) | Ubah jadwal dan tukar shift antar pegawai | `S-A4`, `S-B4` | `READY FOR DESIGN` |
| [`izin-pulang-cepat.md`](./izin-pulang-cepat.md) | Izin pulang cepat | `S-A6` | `READY FOR DESIGN` |
| [`kotak-masuk-persetujuan.md`](./kotak-masuk-persetujuan.md) | Satu kotak masuk untuk seluruh jenis pengajuan | `S-A7` | `READY FOR DESIGN` |
| [`payroll-sisi-hr.md`](./payroll-sisi-hr.md) | Payroll sampai batas tanggung jawab HR | `S-B5` | **`PARTIAL`** — serah terima ke Finance `BLOCKED` |
| [`kompetensi-dan-pelatihan.md`](./kompetensi-dan-pelatihan.md) | Pelatihan wajib dan sertifikat | `S-C2` | `READY FOR DESIGN` |
| [`manajemen-kinerja.md`](./manajemen-kinerja.md) | Siklus penilaian kinerja | `S-C3` | `READY FOR DESIGN` |
| [`offboarding.md`](./offboarding.md) | Pengunduran diri sampai serah terima selesai | `S-C4` | `READY FOR DESIGN` |
| [`hubungan-karyawan-dan-disiplin.md`](./hubungan-karyawan-dan-disiplin.md) | Laporan insiden sampai tindakan disiplin berlaku | `S-C5` | `READY FOR DESIGN` |

### 5.1 Proses yang sengaja TIDAK dibuatkan flowchart

| Proses | Slice | Alasan |
| --- | --- | --- |
| Kredensial, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` | `BLOCKED`. Menggambar alurnya berarti mengarang batas kewenangan praktik yang belum ditetapkan Komite Medik — `HRD-Q-08` |
| Kesehatan dan keselamatan kerja staf | `S-C6` | `BLOCKED`. Aturan akses rekam kesehatan kerja belum disahkan K3RS — `HRD-DEC-010` masih `draft` |
| Perencanaan tenaga kerja, rekrutmen, benefit, layanan HR | `S-D1` s.d. `S-D4` | `BLOCKED` oleh `HRD-Q-05`. Isi tabelnya belum diketahui |
| Perjalanan dinas dan reimbursement | `S-D5` | `DEFERRED`, dan tetap terikat `HRD-Q-05` |

Ketiadaan berkas untuk keempat kelompok di atas **bukan** kelalaian. Ia adalah batas yang
disengaja dan tercatat.
