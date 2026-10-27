# Permintaan ke Pemilik Gizi — Apakah Tim Gizi Melayani Order Konsultasi Gizi dari Pasien IGD

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-008` |
| `tanggal` | 2026-10-26 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Pemilik Gizi** (belum dipetakan namanya) |
| `rujukan` | `IGD-OQ-119` (dibuka 6 Oktober 2026), `IGD-FACT-082` |
| `status` | **`dijawab: tidak`** — Iqbal, pemilik Gizi, 27 Oktober 2026; disahkan pemilik IGD sebagai keputusan Gizi (`IGD-DEC-361`). *Sebelumnya:* **`menunggu jawaban`** |
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

## 6. Jawaban — Iqbal (pemilik Gizi), 27 Oktober 2026

Jawaban yang diterima pengaju berbentuk analisis dan rekomendasi, dan menyatakan sendiri bahwa bagian *"tidak selama
masih IGD"* adalah rekomendasi berdasarkan implementasi yang ada, belum keputusan operasional tercatat. Pengaju
**mengesahkannya sebagai keputusan pemilik Gizi** pada 27 Oktober 2026 (`IGD-DEC-361`).

Jawaban singkat yang disahkan:

> Tidak selama pasien masih berada di IGD. Konsultasi Gizi baru dilayani setelah pasien diputuskan dan resmi masuk
> Rawat Inap. Encounter IGD sebaiknya ditolak oleh modul Gizi agar tidak ada order yang masuk tetapi tidak terlayani.

Alur yang berlaku: pasien masih IGD → tidak dapat memesan konsultasi gizi; dokter memutuskan rawat inap → admisi
Rawat Inap → encounter/episode Rawat Inap aktif → konsultasi gizi dapat dipesan → masuk worklist Tim Gizi. Dasar yang
disebut: frontend Gizi menyaring `jenisKunjungan: "IP"` (`list-pasien-gizi.jsx`), sedangkan backend Nutrition menerima
order berbasis `EncounterId` — tanpa penolakan, order IGD dapat masuk tetapi tidak pernah tampil di worklist.

Pertanyaan 3 dijawab **ya**: modul Gizi sebaiknya menolak encounter IGD secara eksplisit, misalnya *"Konsultasi Gizi hanya
dapat dipesan setelah pasien masuk Rawat Inap."* Penolakan itu pekerjaan modul Gizi, bukan task IGD.

**Akibat untuk IGD:** `IGD-OQ-119` dijawab *tidak*; `FE-IGD-053` dibatalkan (`IGD-DEC-361`), sesuai bagian 4 dokumen ini.
