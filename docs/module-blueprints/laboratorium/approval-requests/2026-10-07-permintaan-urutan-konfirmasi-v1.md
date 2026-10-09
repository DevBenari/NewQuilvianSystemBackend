# Permintaan Persetujuan — Urutan Terima Sampling dan Konfirmasi mengikuti v1

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-016` |
| `tanggal` | 2026-10-07 |
| `pengaju` | `design-business-module`, menurunkan amendment pass putaran 23 |
| `rujukan` | `LAB-DEC-193`..`LAB-DEC-197` (BR-138), `LAB-DEC-199`; decision log revision 88; `02-backend-architecture.md` bagian 24; `AC-283`..`AC-288` |
| `status` | ✅ **`disetujui`** 2026-10-07 — Yoga Aji Pratama, pemilik modul: *"saya setujui kelima butir di atas, lanjutkan"*; **kelima butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen empat kontrak. Keputusan bisnisnya **sudah** diambil 2026-10-07; yang diminta di sini adalah persetujuan **bentuk kontraknya** — lima butir di bagian 3 |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Anda memutuskan urutan v1: sampel diterima dulu, baru dikonfirmasi, lalu diproses — dan pesanan tidak boleh
diproses sebelum dikonfirmasi. Supaya backend menegakkannya, empat kontrak perlu diubah: **Konfirmasi**
kini juga diterima pada pesanan yang wadahnya sudah layak (statusnya tetap *Diterima*), **Proses
Pemeriksaan** ditolak bila pesanan belum dikonfirmasi, dan **hak Konfirmasi** dipisah menjadi izin
tersendiri yang diberikan kepada Analis. Nol tabel, nol kolom, nol migration, nol endpoint baru. Satu task
backend, satu task frontend, dan satu langkah rilis pemberian izin.

---

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r40` | [35](../contracts/api-contract.md) | `confirm` → `LabOrder : Confirm` dan sah pada `Accepted`; `start-process` → `409` untuk status salah dan untuk belum dikonfirmasi |
| `LAB-STATE-v1` | `r8` | [10](../contracts/state-transition-matrix.md) | `Accepted` → Konfirmasi → `Accepted` (tetap); `Accepted` belum dikonfirmasi → Proses ditolak; `LAB-OPEN-027` ditutup |
| `LAB-VAL-v1` | `r17` | [19](../contracts/validation-matrix.md) | `VAL-71` dipersempit; `VAL-151` baru |
| `LAB-PERM-v1` | revision 13 | [15](../contracts/permission-audit-matrix.md) | Aksi `LabOrder : Confirm`; diberikan kepada Analis saat rilis |

---

## 3. Lima butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---:|---|---|---|
| 1 | Nama aksi izin Konfirmasi | **`LabOrder : Confirm`** | Sebutkan nama lain; tidak mengubah perilaku |
| 2 | Kode penolakan Proses sebelum konfirmasi | **`409`** `VAL-151` — keadaan pesanan, bukan isian salah; sama dengan penolakan status lain | `422` akan membuat layar membedakan dua jenis penolakan yang artinya sama bagi petugas |
| 3 | Penyelarasan `start-process` status salah ke `409` (`BE-LAB-89`) | **Dilebur** ke task backend `MVP-12a` — keduanya menyunting `StartProcessAsync` | Dua task menyunting method yang sama berurutan |
| 4 | Pemegang `LabOrder : Confirm` saat rilis | **Analis Laboratorium saja.** Akibatnya *Dokter Umum* — satu-satunya pemegang `LabOrder : Update` hari ini — **kehilangan** Konfirmasi; Batalkan dan konteks klinis PA tetap miliknya | Sebutkan jabatan lain yang juga perlu `Confirm` |
| 5 | Jejak konfirmasi terlambat | **Satu baris riwayat `Order.Confirm`** dengan `from` = `to` = `Accepted` | Tanpa baris riwayat — konfirmasi hanya terbaca dari kolomnya, tidak dari riwayat |

**Contoh untuk butir 4:** pada hari rilis, kedua akun analis dapat mengonfirmasi dan memproses
pesanan, tetapi tidak dapat membatalkannya. Seorang dokter umum yang membuka daftar pasien lab melihat
Konfirmasi redup.

---

## 4. Usulan task

| Task | Gelombang | Isi |
|---|---|---|
| Backend — melebur `BE-LAB-89` | `MVP-12a` | `ConfirmAsync` menerima `Accepted` tanpa mengubah status; `StartProcessAsync` penjaga status `409` lalu `VAL-151`; atribut `[AccessAction("Confirm", …)]` + `[AccessPermission("LabOrder", "Confirm")]` |
| Frontend | `MVP-12b` | Konfirmasi aktif pada `Accepted` belum dikonfirmasi; Proses redup sebelum konfirmasi; izin `LabOrder : Confirm` dibaca layar |
| Langkah rilis | `MVP-12c` | Deploy backend → **segera** beri `LabOrder : Confirm` kepada Analis tanpa menimpa set jabatan → deploy frontend |

Rinciannya disusun `/plan-module-delivery` sesudah persetujuan ini.

---

## 5. Yang tidak diminta

| Hal | Alasan |
|---|---|
| Opsi *langsung proses* di pop-up Konfirmasi | Tetap ditunda `LAB-DEC-188` |
| Hak Batalkan bagi analis | `LAB-OPEN-052` |
| Status baru | `LAB-DEC-195` |
| Kunci Lunas | `LAB-COORD-010` |

---

## 6. Cara menyetujui

Balas dengan persetujuan atas kelima butir bagian 3 — atau sebutkan nomor butir yang ingin diubah. Sesudah
disetujui, status keempat amandemen kontrak berubah menjadi `approved` beserta tanggal dan kutipan
persetujuan Anda, lalu roadmap disusun.

---

## 7. Sesudah disetujui — ✅ diterapkan 2026-10-07

| Artefak | Perubahan |
|---|---|
| `LAB-API-v1` | `r40` `approved` — bagian 35 |
| `LAB-STATE-v1` | `r8` `approved` — bagian 10 |
| `LAB-VAL-v1` | `r17` `approved` — bagian 19 |
| `LAB-PERM-v1` | revision 13 `approved` — bagian 15 |
| `04-prd-to-mvp.md` | 25.7: `LAB-REQ-016` ditutup |
| Roadmap | Disusun `/plan-module-delivery` — gelombang `MVP-12` |
