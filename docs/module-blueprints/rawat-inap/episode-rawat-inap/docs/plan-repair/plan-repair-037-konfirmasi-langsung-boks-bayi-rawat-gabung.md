# PLAN-REPAIR-037 — Konfirmasi Langsung Pemesanan Boks Bayi di Kamar Ibu pada Mode Rawat Gabung

```yaml
plan_id: PLAN-REPAIR-EPS-037
issue: ../issue/issue-037-konfirmasi-langsung-boks-bayi-rawat-gabung.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "41c6451e (MHamzah)"
basis_source_frontend: "431ac8175 (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan untuk mengalihkan alur Langkah 4 (*Penempatan dan Pemesanan*) pada mode **Rawat Gabung dengan Ibu (Rooming-In)**: meniadakan tampilan denah lantai 3 yang terblokir dan menggantikannya dengan antarmuka **Konfirmasi Pemesanan Langsung Boks Bayi di Kamar Ibu (*Rooming-In Direct Placement Confirmation*)**.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIX-EPS-037-01` | `ISS-EPS-037-01` | Mendeteksi mode `RoomingIn` pada `NewbornPlacementStep` dan menyajikan antarmuka konfirmasi langsung boks bayi kamar ibu alih-alih denah lantai ruangan | Frontend | P1 (Tinggi) | `FIX-EPS-037` | ✅ SELESAI | `inpatient-newborn-admission-steps.jsx` merender `RoomingInPlacementConfirmation` saat `isRoomingIn` aktif |
| `FIX-EPS-037-02` | `ISS-EPS-037-02` | Mengotomatisasi seleksi boks bayi kamar ibu (`301-BOKS`) dari ketersediaan data kamar ibu dan menampilkan kartu ringkasan penempatan kamar & boks bayi | Frontend | P1 (Tinggi) | `FIX-EPS-037` | ✅ SELESAI | Hook effect mendeteksi boks bayi kamar ibu (`301-BOKS`) dan menampilkan detail kartu penempatan |
| `FIX-EPS-037-03` | `ISS-EPS-037-02` | Menggabungkan aksi penyiapan draft dan pemesanan tempat tidur dalam satu tombol aksi *"Pesan Boks Bayi & Lanjut"*, lengkap dengan tombol Batal Pendaftaran lokal | Frontend | P1 (Tinggi) | `FIX-EPS-037` | ✅ SELESAI | Tombol `Pesan Boks Bayi & Lanjut` secara atomik mengeksekusi `prepareBooking` dan `reserve(episodeId)` |
| `FIX-EPS-037-04` | Seluruhnya | Memperbarui suite unit test otomatis `inpatient-newborn-admission.test.mjs` untuk memvalidasi alur konfirmasi langsung boks rawat gabung | Test | P1 (Tinggi) | `FIX-EPS-037` | ✅ SELESAI | `inpatient-newborn-admission.test.mjs` 18/18 test lulus 100% |

**Ringkasan: 4 dari 4 perbaikan selesai (100%).**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-037-01 & ISS-EPS-037-02 — Pengalihan Denah Terblokir Menjadi Konfirmasi Langsung Boks Bayi

| Opsi | Cara Kerja | Kelebihan | Kekurangan | Status |
| :--- | :--- | :--- | :--- | :--- |
| **Opsi A (Filter Denah Saja)** | Mempertahankan kanvas denah 2D tetapi hanya menampilkan kamar ibu yang di-zoom otomatis, menyembunyikan kamar lain. | Tetap memakai komponen `FloorPlanSelection`. | Petugas masih harus mengklik kamar ibu dan mengklik ikon boks bayi di kanvas grafis, proses tetap berbelit. | DITOLAK |
| **Opsi B (Konfirmasi Langsung — Pilihan Pengguna)** | Menghilangkan kanvas denah untuk mode `RoomingIn`, otomatis memilih boks bayi di kamar ibu, dan menampilkan kartu ringkasan pemesanan langsung (*Direct Confirmation Card*). | Sangat cepat, intuitif, petugas langsung melihat detail kamar ibu dan boks bayi, 1 kali klik langsung pesan & melangkah maju. | Memerlukan sub-tampilan khusus di Langkah 4 saat `careMode === "RoomingIn"`. | **SOLUSI TERPILIH** |

**Alasan Pemilihan Opsi B:**
Sesuai masukan tepat dari pengguna dan kaidah rekayasa rumah sakit: pada Rawat Gabung (*Rooming-In*), lokasi kamar sudah 100% terkunci mengikuti kamar ibu. Tidak ada keputusan memilih kamar lain. Menampilkan kartu konfirmasi langsung menghemat waktu petugas admisi dan menghilangkan kebingungan atas denah yang terblokir.

---

## 3. Skema Tampilan Sebelum → Sesudah

```carousel
### SEBELUM: Tampilan Denah Lantai Terblokir (Membingungkan)
```text
+-------------------------------------------------------------------------------+
| LANGKAH 4 — Penempatan dan Pemesanan Bayi                                     |
| Peringatan: Denah Lantai 3 (Seluruh Kamar Dewasa Diblokir)                    |
| +---------------------------------------------------------------------------+ |
| | [TERBLOKIR] Kamar 302 (Kelas I)  | [TERBLOKIR] Kamar 303 (Kelas II)       | |
| | [TERBLOKIR] Kamar 304 (Isolasi)  | [TERBLOKIR] Kamar 305 (ODC)            | |
| | ... Puluhan kamar abu-abu/terblokir di kanvas denah ...                   | |
| | [Kamar VIP 301] -> Bed 301-A (Occupied) | Bed 301-BOKS (Available)        | |
| +---------------------------------------------------------------------------+ |
| (Petugas harus mencari kamar ibu di denah, klik kamar, lalu klik boks bayi)   |
| [Batal Pendaftaran]                                 [Kembali]  [Lanjut ->]    |
+-------------------------------------------------------------------------------+
```
<!-- slide -->
### SESUDAH (Belum Dipesan): Konfirmasi Penempatan Langsung Boks Bayi di Kamar Ibu
```text
+-------------------------------------------------------------------------------+
| LANGKAH 4 — Penempatan dan Pemesanan Bayi (Rawat Gabung dengan Ibu)           |
| Bayi ditempatkan pada boks bayi di dalam kamar perawatan ibu.                |
|                                                                               |
| [INFORMASI RAWAT GABUNG]                                                      |
| Pasien Ibu           : Ny. RATNA SARI DEWI (No. RM: 00-00-00-49)              |
| Lokasi Kamar Ibu     : Kamar VIP 301 — Sayap Barat Lantai 3                   |
| Kelas & Unit Layanan : Kelas VIP · Rawat Inap Terpadu                         |
| Tempat Tidur Ibu     : 301-A (Terisi Pasien Ibu)                              |
| Boks Bayi Disediakan : 301-BOKS (BD-RSMMC-BOKS-301)                           |
| Status Kesiapan      : [V] Boks Bayi Tersedia & Siap Dipesan                  |
|                                                                               |
| +---------------------------------------------------------------------------+ |
| | [INFO] Tempat tidur boks bayi di kamar ibu telah otomatis teridentifikasi. |
| | Klik tombol di bawah untuk memesan boks dan melanjutkan admisi.           | |
| +---------------------------------------------------------------------------+ |
|                                                                               |
| [Batal Pendaftaran] (Ghost)          [Kembali]  [Pesan Boks Bayi & Lanjut ->] |
+-------------------------------------------------------------------------------+
```
<!-- slide -->
### SESUDAH (Sudah Dipesan / Sedang Berjalan): Rincian Reservasi Aktif
```text
+-------------------------------------------------------------------------------+
| LANGKAH 4 — Penempatan dan Pemesanan Bayi (Reservasi Aktif)                   |
| [RINCIAN PEMESANAN TEMPAT TIDUR BAYI]                                         |
| Boks Bayi Dipesan    : 301-BOKS (Kamar VIP 301 - Kelas VIP)                   |
| Pasien Ibu Terkait   : Ny. RATNA SARI DEWI (RM 00-00-00-49)                   |
| Batas Waktu Reservasi: 14:55 Menit (Hitung Mundur Berjalan)                   |
| Status Pemesanan     : Dipesan Sementara (Draft Admisi Disimpan)              |
|                                                                               |
| [Batalkan Admisi] (Danger)                       [Lanjut ke Persetujuan Wali] |
+-------------------------------------------------------------------------------+
```
````

---

## 4. Rincian Perbaikan

### FIX-EPS-037-01 — Pendeteksian Mode Rawat Gabung pada Langkah 4
- **Komponen Target**: `inpatient-newborn-admission-steps.jsx` pada `NewbornPlacementStep`.
- **Logika**:
  Jika `newborn?.draft?.careMode === "RoomingIn"`, render tampilan `RoomingInPlacementConfirmation` alih-alih merender `InpatientAdmissionBedSelectionStep`.
  Untuk mode `Neonatal` (Perinatologi & NICU), alur denah/daftar bed tetap dipertahankan.

### FIX-EPS-037-02 — Seleksi Otomatis Boks Bayi Kamar Ibu
- Menemukan boks bayi di kamar ibu secara reaktif dari `floorPlan?.data?.rooms` atau `bed.board?.availableBeds` / `legacyBoard.availableBeds`:
  ```javascript
  const motherRoom = (floorPlan?.data?.rooms || []).find(
    (room) => String(room.roomId || "") === String(motherRoomId || "")
  );
  const roomingInBed = motherRoom?.beds?.find(
    (b) => b.isForNewborn || b.bedCode?.includes("BOKS") || b.isSelectable
  );
  ```
- Jika boks bayi ditemukan dan belum ada bed terpilih, otomatis panggil `bed.selectBed(roomingInBed)`.
- Jika di kamar ibu tidak ditemukan boks bayi yang tersedia, tampilkan peringatan:
  *"Boks bayi di Kamar VIP 301 sedang tidak tersedia atau belum terdaftar. Silakan hubungi perawat bangsal atau alihkan perawatan ke Ruang Perinatologi."* beserta tombol pengalihan cepat.

### FIX-EPS-037-03 — Tombol Aksi Atomik "Pesan Boks Bayi & Lanjut"
- Tombol di bilah bawah:
  1. Tombol `Batal Pendaftaran` (`variant="ghost"`) di sebelah kiri untuk mereset pendaftaran lokal tanpa alasan (mengikuti pola `FIX-EPS-036`).
  2. Tombol `Kembali` (`variant="secondary"`) untuk kembali ke Langkah 3 DPJP.
  3. Tombol `Pesan Boks Bayi & Lanjut` (`variant="primary"`):
     - Memanggil `await bed.prepareBooking()`.
     - Memanggil `await bed.reserve(episodeId)`.
     - Mengarahkan `flow.goNext()` langsung ke Langkah 5 (`consent-form`).

### FIX-EPS-037-04 — Unit Test Otomatis
- Menambahkan skenario uji di `tests/unit/inpatient-newborn-admission.test.mjs`:
  - Memastikan pada `careMode === "RoomingIn"`, `NewbornPlacementStep` merender `RoomingInPlacementConfirmation` dan tidak merender kanvas denah `InpatientAdmissionBedSelectionStep`.
  - Memastikan tersedianya tombol aksi *"Pesan Boks Bayi & Lanjut"* dan tombol *"Batal Pendaftaran"*.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-037-01 (Deteksi mode RoomingIn di Langkah 4) ✅
       │
       ▼
FIX-EPS-037-02 (Seleksi otomatis boks bayi kamar ibu & tampilan kartu konfirmasi) ✅
       │
       ▼
FIX-EPS-037-03 (Tombol aksi atomik Pesan Boks Bayi & Lanjut + Batal Pendaftaran) ✅
       │
       ▼
FIX-EPS-037-04 (Update unit test otomatis inpatient-newborn-admission.test.mjs) ✅
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

- **Keputusan Pemilik K-01**: Menyetujui perombakan Langkah 4 khusus Rawat Gabung (*Rooming-In*) tidak lagi menampilkan denah lantai yang terblokir, melainkan langsung menampilkan Kartu Konfirmasi Pemesanan Boks Bayi di Kamar Ibu dengan tombol pesan satu-klik. **(DISETUJUI & SELESAI 22-10-2026)**.

---

## 7. Dokumen Hulu yang Ikut Direvisi

- Dokumen blueprint layar `05-skema-tampilan.md` pada bagian alur Admisi Bayi Baru Lahir diperbarui untuk mencerminkan sub-langkah konfirmasi boks langsung khusus mode Rawat Gabung.

---

## 8. Verifikasi Menyeluruh

1. Jalankan unit test `npm test` (`inpatient-newborn-admission.test.mjs`) -> 18/18 test lolos (100%).
2. Uji langsung di antarmuka web (localhost):
   - Buka menu **Admisi Rawat Inap** -> **Admisi Bayi Baru Lahir**.
   - Pilih Ibu `Ny. RATNA SARI DEWI` (Kamar VIP 301).
   - Isi identitas bayi (Langkah 2).
   - Pilih tipe perawatan: **Rawat Gabung dengan Ibu (Rooming-In)** dan pilih DPJP (Langkah 3).
   - Masuk ke Langkah 4:
     - **TIDAK MUNCUL** denah lantai 3 yang terblokir abu-abu.
     - **MUNCUL KARTU KONFIRMASI LANGSUNG**:
       - Lokasi: Kamar VIP 301
       - Boks Bayi: 301-BOKS (Tersedia & Siap Dipesan)
     - Klik **"Pesan Boks Bayi & Lanjut"**.
     - Reservasi boks terbentuk dan layar langsung melangkah ke Langkah 5 (Persetujuan Wali).

---

## 9. Riwayat Dokumen

| Tanggal | Status | Catatan |
| :--- | :---: | :--- |
| 2026-10-22 | `MENUNGGU_PERSETUJUAN` | Dokumen rencana perbaikan diterbitkan menunggu persetujuan pemilik sistem |
| 2026-10-22 | `SELESAI` | Seluruh perbaikan FIX-EPS-037-01 s.d. FIX-EPS-037-04 selesai diimplementasikan dan diverifikasi |
