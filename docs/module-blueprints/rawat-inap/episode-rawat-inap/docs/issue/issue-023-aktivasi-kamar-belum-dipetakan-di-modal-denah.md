# ISSUE-023 — Alur Buntu dan Ketiadaan Fitur Aktivasi Kamar Belum Dipetakan pada Modal Denah Master Bed

```yaml
issue_id: ISSUE-EPS-023
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Editor Denah Master Bed (FE-RWI-248 / FE-RWI-250 / FloorPlanMasterRoomPopup)"
sumber_laporan: "Keluhan pemilik & pengguna 17-10-2026: UI/UX membingungkan, kamar belum dipetakan buntu dan harus keluar ke halaman impor"
tanggal_issue: "2026-10-17"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "c2bc563e (MHamzah)"
source_sha_frontend: "15ae73984 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-023-aktivasi-kamar-belum-dipetakan-di-modal-denah.md
```

---

## 1. Ringkasan

Laporan pengguna menyoroti inkonsistensi dan kebuntuan antarmuka pada layar Editor Denah Master Bed (`/health-services/master-data/bed`). Saat pengguna mengklik ruangan fisik berstatus **"Ruang cadangan"**, modal pop-up menyediakan tombol langsung `+ Aktifkan Ruang Cadangan` sehingga pengguna dapat menetapkan nomor, kelas, unit, dan bed tanpa meninggalkan denah. Sebaliknya, saat pengguna mengklik ruangan berstatus **"Belum dipetakan"** (seperti kamar 301–318 di Lantai 3), modal hanya menyajikan pesan peringatan pasif dan tautan keluar ke Master Kamar tanpa menyediakan tombol aktivasi atau pemetaan mandiri. Pengguna terpaksa berpindah ke halaman Impor Denah yang rumit dan rentan galat validasi. Masalah ini diklasifikasikan sebagai `DESIGN_CHANGE` (permintaan perbaikan interaksi langsung di modal) dan `UX_IMPROVEMENT`, dengan keparahan `Medium` karena menghambat efisiensi konfigurasi denah oleh staf operasional rumah sakit.

---

## 2. Laporan Asli

| No. | Kalimat Pelapor | Lampiran |
| ---: | :--- | :--- |
| 1 | "agak pusing ui/ux ya. kenapa tidak di satu denah saja? lalu pada modal disitu untuk mengaktifkan untuk melakukan pemetakan? tidak perlu pergi ke halaman lain." | `media_1792213876375_22db6ed9.png` (Modal "Editor Master Bed — Belum dipetakan") |
| 2 | "oke untuk menu import denah biarkan tetap ada. saya ingin mengatasi UI yang belum di petakan terlebih dahulu. saya ingin di klik pada kamar yang belum di petakan maka muncul pop up modal, lalu dari modal tersebut bisa di klik di aktifkan. berikan planning dan skema tampilannya ya." | `media_1792211713606_f092d4c4.png` (Modal Peringatan Belum Dipetakan) |

---

## 3. Ringkasan Temuan

| ID | No. Lap | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :---: | :---: | :---: | :---: |
| `ISS-EPS-023-01` | 1, 2 | Modal kamar belum dipetakan buntu tanpa aksi aktivasi langsung | `DESIGN_CHANGE` | FE | `Medium` | `SUDAH-VERIFIKASI` | `FIX-EPS-023-01` |
| `ISS-EPS-023-02` | 1 | Ketiadaan opsi penautan ke kamar database existing langsung dari modal | `DESIGN_CHANGE` | FE + BE | `Medium` | `SUDAH-VERIFIKASI` | `FIX-EPS-023-02` |

---

## 4. Rincian per Temuan

### `ISS-EPS-023-01` — Modal kamar belum dipetakan buntu tanpa aksi aktivasi langsung

- **Lokasi Kode:**  
  `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-master-room-popup.jsx:361-374`
- **Gejala:**  
  Saat ruangan bernomor fisik di denah yang belum terhubung ke database diklik, modal hanya menampilkan `InformationAlert` berwarna kuning: *"Posisi ini belum terhubung ke kamar master. Gunakan Impor susunan denah untuk memetakannya."* dan tautan ke halaman Master Kamar konvensional.
- **Rantai Sebab:**  
  1. Pengembang awal mengasumsikan pemetaan kamar denah hanya dilakukan secara massal (*bulk*) melalui layar `/floor-plan-import`.
  2. Logika `room.isUnmapped` di percabangan pop-up tidak diberikan form aksi `startAction(...)`, berbeda dengan `room.isReserve` yang memiliki tombol `+ Aktifkan Ruang Cadangan`.
- **Dampak:**  
  Pengguna merasa alur kerja terputus (*dead-end*), membingungkan, dan menuntut navigasi bolak-balik antar-layar hanya untuk mengaktifkan satu kamar.

### `ISS-EPS-023-02` — Ketiadaan opsi penautan ke kamar database existing langsung dari modal

- **Lokasi Kode:**  
  `QuilvianSystemFrontendDev/src/lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js`
- **Gejala:**  
  Bila rumah sakit sudah memiliki data kamar di database (misal Ruang ICU di Lantai 2) yang belum memiliki kunci denah (`floorPlanRoomKey`), pengguna tidak dapat mengaitkan kotak denah tersebut ke kamar existing secara langsung melalui pop-up denah.
- **Rantai Sebab:**  
  Mutasi `BedMasterMutation` saat ini baru mendukung aktivasi kamar baru dari ruang cadangan (`activate`), ubah kelas/wing (`editRoom`), tambah bed (`addBed`), ubah label (`renameBed`), dan ketersediaan (`availability`), namun belum memiliki aksi khusus penautan kunci denah ke kamar existing (`attachExistingRoom`).

---

## 5. Tanya-Jawab Pelapor

| Pertanyaan Pelapor | Jawaban Berbasis Bukti Sistem |
| :--- | :--- |
| *"Kenapa tidak di satu denah saja?"* | Awalnya pembuat sistem merancang halaman Impor terpisah untuk mengimpor 106 kamar dan 168 bed secara borongan saat setup awal rumah sakit. Namun untuk operasional berkala, mengaktifkan kamar langsung di satu denah adalah alur yang jauh lebih intuitif dan alami bagi pengguna. |
| *"Lalu pada modal disitu untuk mengaktifkan untuk melakukan pemetaan, tidak perlu pergi ke halaman lain?"* | Ya, ini sangat memungkinkan dan logis. Modal denah dapat diperluas agar memiliki tombol **"Aktifkan Kamar"** (membuat kamar dan bed baru) atau **"Tautkan Kamar Existing"** (menghubungkan ke data kamar yang sudah ada di database) langsung tanpa pindah halaman. |

---

## 6. Temuan Tambahan

1. **Konsistensi Penamaan Kamar Otomatis:** Saat mengaktifkan kamar baru dari denah, nomor kamar sudah diketahui dari geometri denah (misal: `301`), sehingga nomor kamar dapat diisi otomatis sebagai default di formulir aktivasi tanpa mengharuskan pengguna mengetik ulang.
2. **Saran Kelas Pasien Otomatis:** Denah sudah memiliki metadata kelas bawaan (`expectedPatientClassName`). Di formulir modal, saran kelas ini dapat langsung terpilih otomatis (*pre-selected*).

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

1. **Untuk Pemilik/User:** Apakah tombol aktivasi pada modal kamar belum dipetakan cukup mendukung pembuatan kamar baru, atau wajib juga mendukung penautan ke kamar existing yang sudah ada di database?  
   *Rekomendasi:* Sediakan kedua opsi (Tab 1: Buat Kamar Baru, Tab 2: Tautkan Kamar Existing) agar tidak terjadi bentrok nama tempat tidur seperti pada kasus Lantai 2.

---

## 8. Catatan Pola

Pola ini serupa dengan `ISS-EPS-009` (kebuntuan alur pemilihan bed). Antarmuka visual (denah) harus menjadi pusat interaksi terpadu (*single source of truth and interaction*), bukan hanya penampil pasif yang melempar pengguna ke form tabel konvensional.

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| :--- | :--- | :--- |
| 2026-10-17 | Antigravity AI | Penerbitan dokumen analisis issue ISSUE-EPS-023 berdasarkan laporan pengguna. |
