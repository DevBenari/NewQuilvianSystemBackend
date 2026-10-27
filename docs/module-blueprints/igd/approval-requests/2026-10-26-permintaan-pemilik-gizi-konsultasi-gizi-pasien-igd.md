# Permintaan ke Pemilik Gizi — Apakah Tim Gizi Melayani Order Konsultasi Gizi dari Pasien IGD

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-008` |
| `tanggal` | 2026-10-26 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Pemilik Gizi** (belum dipetakan namanya) |
| `rujukan` | `IGD-OQ-119` (dibuka 6 Oktober 2026), `IGD-FACT-082` |
| `status` | **`menunggu jawaban`** |
| `sifat` | **Pertanyaan operasional, bukan permintaan kode.** Jawabannya bisa satu kata |
| `menahan` | `FE-IGD-053` ⛔ |

---

## 1. Satu paragraf untuk yang tidak punya waktu

`IGD-OQ-119` dibuka **6 Oktober 2026** dan belum pernah ada dokumen permintaannya — dua puluh hari
itu bukan menunggu jawaban, melainkan belum ada yang ditanya. Pertanyaannya satu, dan jawabannya
bisa sesingkat **"ya"** atau **"tidak"**:

> **Apakah tim Gizi melayani order konsultasi gizi untuk pasien yang masih berada di IGD?**

## 2. Kenapa ini ditanyakan

Modul Gizi ditulis untuk **asuhan gizi rawat inap**, tetapi validasinya **menerima encounter jenis
lain** — termasuk IGD (`IGD-FACT-082`). Artinya hari ini layar IGD **secara teknis bisa** mengirim
order gizi dan order itu akan diterima sistem, tanpa ada yang pernah memutuskan apakah tim Gizi
memang akan mengerjakannya.

Itu keadaan yang buruk ke dua arah:

| Bila sebenarnya **tidak** dilayani | Bila sebenarnya **dilayani** |
|---|---|
| Perawat IGD mengirim order yang tidak pernah dikerjakan siapa pun, dan tidak ada pesan yang memberi tahu | Kami tidak membangun layarnya, padahal kemampuannya ada — pasien IGD yang perlu asuhan gizi terlewat |

## 3. Yang kami butuhkan

| # | Pertanyaan | Bentuk jawaban |
|---:|---|---|
| 1 | Tim Gizi melayani order dari pasien IGD? | **Ya** / **Tidak** |
| 2 | Bila **ya**: adakah syaratnya — misalnya hanya untuk pasien yang sudah diputuskan rawat inap, atau yang lama observasinya melewati batas tertentu? | Satu atau dua kalimat |
| 3 | Bila **tidak**: apakah modul Gizi sebaiknya **menolak** encounter IGD secara tegas, alih-alih menerimanya diam-diam seperti sekarang? | **Ya** / **Tidak** — ini perbaikan di sisi Gizi, bukan IGD |

## 4. Akibat tiap jawaban

| Jawaban | Yang kami kerjakan |
|---|---|
| **Tidak dilayani** | `FE-IGD-053` ditutup sebagai **dibatalkan**, jenis pesanan gizi dicabut dari tab Penunjang IGD, dan kami catat alasannya. Nol pekerjaan sia-sia |
| **Dilayani** | `FE-IGD-053` dikerjakan mengikuti syarat yang Anda sebutkan |
| **Dilayani bersyarat** | Syaratnya kami jadikan penjaga di layar, bukan sekadar catatan |

Jawaban **"tidak"** sama berharganya dengan "ya" — ia menutup satu task dan menghapus satu
ketidakpastian. Yang tidak berguna hanya diamnya.

## 5. Lingkup yang jelas tidak tersentuh

Permintaan ini **hanya** tentang jenis pesanan gizi pada tab Penunjang Medis IGD. Asuhan gizi rawat
inap, alur gizi yang sudah berjalan, dan pesanan gizi dari poliklinik **tidak diubah sama sekali**.
