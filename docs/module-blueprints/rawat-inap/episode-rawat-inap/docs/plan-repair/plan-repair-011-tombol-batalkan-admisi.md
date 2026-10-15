# PLAN-REPAIR-011 — Penambahan Tombol Batalkan Admisi dan Mekanisme Reset Formulir pada Alur Admisi Rawat Inap

```yaml
plan_id: PLAN-REPAIR-EPS-011
issue: ../issue/issue-011-tombol-batalkan-admisi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "8fce8c96 (MHamzah)"
basis_source_frontend: "1b0ad5dd4 (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-011-01` | `ISS-EPS-011-01`, `ISS-EPS-011-T1` | Tambahkan tombol "Batal Pendaftaran" (reset form) di Langkah 1 s/d 6 dan tombol "Batalkan Admisi" di Langkah 7 s/d 8 | Frontend | 1 | — | ✅ **SELESAI** | Tombol `Batal Pendaftaran` terpasang di langkah 1-6 dan `Batalkan Admisi` di langkah 7-8 & resume; ESLint 0 error |
| `FIX-EPS-011-02` | `ISS-EPS-011-02` | Hubungkan modal konfirmasi pembatalan beralasan `FE-INP-17` ke API `PATCH /episodes/{id}/cancel` untuk melepas reservasi bed | Frontend | 1 | — | ✅ **SELESAI** | Terhubung ke `inpatientEpisodeService.patch` dengan validasi alasan & pelepasan bed seketika |
| `FIX-EPS-011-03` | `ISS-EPS-011-03` | Bangun fungsi utilitas pembersihan menyeluruh (*clear session & form reset*) kembali ke tampilan awal admisi | Frontend | 2 | — | ✅ **SELESAI** | `clearInpatientAdmissionSession` di `inpatient-admission-session-utils.js` membersihkan `sessionStorage` & reset form |

**Ringkasan: 3 dari 3 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-011-01 — Ketiadaan tombol batalkan admisi pada antarmuka tahapan admisi rawat inap

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Bedakan tombol menurut status transaksi: "Batal Pendaftaran" pada tahap form lokal (Langkah 1–6), dan "Batalkan Admisi" pada tahap transaksi server aktif (Langkah 7–8 & pelanjutan). | Tepat secara arsitektur, tidak memanggil API yang belum ada datanya, UX sangat jelas bagi petugas. | Memerlukan dua varian aksi di bilah navigasi bawah. |
| B | Pasang satu tombol "Batalkan Admisi" seragam di semua langkah dari langkah 1 sampai 10. | Tampilan konsisten di setiap langkah. | Di langkah awal membingungkan petugas karena belum ada nomor episode di riwayat. |
| C | Tidak menambah tombol di alur; paksa petugas keluar ke Daftar Kerja Episode untuk membatalkan. | Tidak ada perubahan kode alur admisi. | UX buruk, membiarkan kamar terkunci 60 menit, keluhan pengguna tidak terjawab. |

**Solusi terpilih: Opsi A.**

1. Sesuai dengan siklus hidup data: pada Langkah 1 s/d 6 (sebelum tombol Lanjut pada Pilih Bed ditekan), belum ada baris episode di database, sehingga pembatalan cukup berupa reset formulir lokal.
2. Pada Langkah 7 (Booking Bed) dan Langkah 8 (Konfirmasi), episode berstatus `Draft` sudah berjangkar di server dan tempat tidur sedang dipesan. Tombol resmi **Batalkan Admisi** wajib mengeksekusi pembatalan server agar tempat tidur seketika bebas kembali (*Available*).

*Kenapa bukan opsi lain:* Opsi B akan menjanjikan nomor episode yang dibatalkan padahal di langkah 1 data belum pernah dikirim ke database. Opsi C menolak permintaan pengguna dan mempertahankan masalah operasional rumah sakit.

---

### ISS-EPS-011-02 — Tombol keluar yang ada (*Tinggalkan Admisi*) membiarkan episode `Draft` dan reservasi bed menggantung

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Gantikan tombol "Tinggalkan Admisi" dengan tombol tegas "Batalkan Admisi" yang membuka dialog `FE-INP-17` dengan isian alasan wajib. | Kamar tidur langsung lepas ke status *Available*, data tercatat resmi sebagai *Cancelled*, audit RS rapi. | Petugas wajib mengetikkan alasan pembatalan singkat (minimal 1 kata). |
| B | Pertahankan tombol "Tinggalkan Admisi" apa adanya, tambahkan tombol "Batalkan Admisi" di sebelahnya. | Memberi pilihan apakah ingin menunda admisi atau membatalkannya. | Bilah aksi bawah menjadi terlalu ramai dengan tiga tombol sekunder. |

**Solusi terpilih: Opsi A (dengan opsi transisi Opsi B bila alur pelanjutan tetap dibutuhkan).**

1. Petugas yang berada di Langkah Booking Bed atau Konfirmasi hampir selalu ingin membatalkan jika proses tidak diteruskan; membiarkan kamar terkunci mengganggu pelayanan IGD/Poliklinik.
2. Memakai ulang (*reuse*) komponen modal konfirmasi `ConfirmModal` dengan prop `requireReason` yang sudah terbukti andal pada `FE-RWI-031`.

---

### ISS-EPS-011-03 — Mekanisme reset/clear formulir terpadu kembali ke awal

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Buat fungsi `clearInpatientAdmissionSession()` yang membersihkan `sessionStorage` penjamin, storage deposit, state form, lalu `router.replace("/health-services/inpatient-management/admissions")`. | Form benar-benar bersih tanpa jejak, mencegah kontaminasi data pasien berikutnya. | Sedikit penyesuaian di level view controller. |
| B | Hanya reload halaman dengan `window.location.reload()`. | Sangat cepat dibuat. | Pengalaman pengguna kasar (layar berkedip putih), URL query param berisiko tetap menempel. |

**Solusi terpilih: Opsi A.**

1. Menjaga arsitektur Single Page Application (SPA) Next.js yang mulus.
2. Menjamin seluruh *session storage* terisolasi bersih sebelum pendaftaran berikutnya dimulai.

---

## 3. Skema tampilan sebelum → sesudah

### A. Pada Langkah 1 s/d 6 (Contoh: Langkah 2 Jenis Kunjungan & Kategori — Sesuai Screenshot Pelapor)

#### SEBELUM
```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ Admisi Rawat Inap                                                                      │
│ LANGKAH 2 DARI 10  Jenis Kunjungan & Kategori                                          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 1. Pilih Jenis Kunjungan                                                               │
│    [ (U) Kunjungan Umum ]   [ Kunjungan Rujukan ]                                      │
│                                                                                        │
│ 2. Pilih Kategori Pasien                                                               │
│    [ (U) Pasien Umum ]      [ Bayi Baru Lahir ]      [ Pegawai RS ]                    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                        [ Kembali ke Pencarian ] [ Lanjut ke Pembayaran ]│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### SESUDAH
```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ Admisi Rawat Inap                                                                      │
│ LANGKAH 2 DARI 10  Jenis Kunjungan & Kategori                                          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 1. Pilih Jenis Kunjungan                                                               │
│    [ (U) Kunjungan Umum ]   [ Kunjungan Rujukan ]                                      │
│                                                                                        │
│ 2. Pilih Kategori Pasien                                                               │
│    [ (U) Pasien Umum ]      [ Bayi Baru Lahir ]      [ Pegawai RS ]                    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [ Batal Pendaftaran ]                  [ Kembali ke Pencarian ] [ Lanjut ke Pembayaran ]│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### B. Pada Langkah 7 Booking Bed & Langkah 8 Konfirmasi (Pasca Titik Tulis Server)

#### SEBELUM (Langkah Konfirmasi)
```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ RINGKASAN CEPAT: Tn. Budi (RM 00-12-34) · RI-2026-00045 · Bed 02 (Dipesan: 58 mnt)    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [Panel Identitas Pasien & Kamar Perawatan]                                            │
│ [Isian Catatan Admisi]                                                                 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [ Kembali ] [ Tinggalkan Admisi ]                  [ Kunci Admisi & Cetak Persetujuan ]│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### SESUDAH (Langkah Konfirmasi & Booking Bed)
```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ RINGKASAN CEPAT: Tn. Budi (RM 00-12-34) · RI-2026-00045 · Bed 02 (Dipesan: 58 mnt)    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [Panel Identitas Pasien & Kamar Perawatan]                                            │
│ [Isian Catatan Admisi]                                                                 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [ Batalkan Admisi ⚠ ]   [ Kembali ]                [ Kunci Admisi & Cetak Persetujuan ]│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Dialog Modal Konfirmasi Pembatalan (`FE-INP-17` Reuse)
```text
┌─ Batalkan Admisi Rawat Inap? ──────────────────────────────────────────┐
│ Pasien: Tn. Budi Santoso (RM 00-12-34-56)                               │
│ Episode: RI-2026-00045 (Sedang Disiapkan)                               │
│                                                                         │
│ ⚠ PERHATIAN:                                                            │
│ Tempat tidur BED 02 - Ruang Melati yang sedang dipesan akan dilepas     │
│ seketika menjadi TERSEDIA kembali untuk pasien lain.                    │
│ Transaksi akan dicatat sebagai DIBATALKAN pada Riwayat Episode.         │
│                                                                         │
│ Alasan Pembatalan *                                                    │
│ [ Keluarga pasien memilih rawat jalan / menolak rawat inap.......... ] │
│                                                                         │
│                                          [ Kembali ] [ Batalkan Admisi ]│
└─────────────────────────────────────────────────────────────────────────┘
```

---

### Tabel Spesifikasi Antarmuka

#### 1. Wilayah
| Wilayah | Isi | Sumber | Komponen |
| --- | --- | --- | --- |
| Bilah Aksi Kiri (Langkah 1–6) | Tombol "Batal Pendaftaran" | Form state lokal | `BaseButton variant="ghost"` warna teks netral/merah halus |
| Bilah Aksi Kiri (Langkah 7–8) | Tombol "Batalkan Admisi" | Episode ID & Reservation | `BaseButton variant="outline-danger"` atau `variant="danger"` |
| Dialog Pembatalan Server | Identitas pasien, nomor episode, info bed dilepas, input alasan wajib | Detail episode & reservasi bed | `ConfirmModal` dengan `requireReason` |

#### 2. Tombol
| Label Tombol | Jenis | Kapan Aktif | Yang Terjadi Saat Diklik |
| --- | --- | --- | --- |
| **Batal Pendaftaran** | `ghost` / `secondary` | Langkah 1 s/d 6 | Muncul konfirmasi sederhana "Batalkan pendaftaran pasien ini?". Jika ya, form di-reset dan kembali ke menu awal. |
| **Batalkan Admisi** | `danger` | Langkah 7, 8, dan alur pelanjutan | Membuka modal pembatalan `FE-INP-17`. Memvalidasi alasan, memanggil `PATCH /episodes/{id}/cancel`, melepas bed, reset form, dan kembali ke menu awal. |

#### 3. Keadaan Layar
| Keadaan | Tampilan |
| --- | --- |
| **Sedang membatalkan (Loading)** | Tombol di dalam modal menampilkan spinner (*loading state*), tombol navigasi langkah terkunci. |
| **Pembatalan Berhasil** | Notifikasi Toast hijau muncul: *"Admisi RI-2026-XXXX berhasil dibatalkan. Riwayat tercatat pada Daftar Kerja Episode."*, layar langsung kembali ke menu Pilih Tipe Pendaftaran. |
| **Pembatalan Gagal (Server Error)** | Alert merah di dalam modal menampilkan pesan galat dari server (misal: "Alasan pembatalan wajib diisi"). Alasan yang sudah diketik petugas tidak hilang. |

---

## 4. Rincian perbaikan

### FIX-EPS-011-01 — Tambahkan tombol pembatalan pada bilah aksi langkah admisi rawat inap

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-011-01`, `ISS-EPS-011-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`<br>`src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx`<br>`src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` |
| **Radius dampak** | Layar Admisi Rawat Inap (`/health-services/inpatient-management/admissions`) |
| **Bergantung pada** | `FIX-EPS-011-02`, `FIX-EPS-011-03` |

**Langkah:**
1. Tambahkan tombol "Batal Pendaftaran" di bilah aksi kiri pada langkah 1 s/d 6.
2. Pasang tombol "Batalkan Admisi" di bilah aksi kiri pada langkah Booking Bed (Langkah 7) dan Konfirmasi (Langkah 8).
3. Sambungkan tombol tersebut ke modal konfirmasi pembatalan.

**Acceptance criteria:**
1. Pada Langkah 2 (Jenis Kunjungan & Kategori), terdapat tombol "Batal Pendaftaran" di bilah kiri bawah.
2. Pada Langkah 7 (Booking Bed) dan Langkah 8 (Konfirmasi), terdapat tombol tegas "Batalkan Admisi".
3. Pada alur pelanjutan admisi (*resume episode*), tombol "Batalkan Admisi" selalu tersedia di seluruh langkah.

**Verifikasi:**
- `npm run lint` nol error.
- Uji navigasi visual pada browser localhost.

---

### FIX-EPS-011-02 — Eksekusi pembatalan resmi via `PATCH /episodes/{id}/cancel` dan pelepasan bed

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-011-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (hook & service handler) |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`<br>`src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` |
| **Radius dampak** | Alur admisi rawat inap dan status ketersediaan papan tempat tidur |
| **Bergantung pada** | Tidak ada (backend API sudah tersedia dan terverifikasi) |

**Langkah:**
1. Gunakan fungsi pembatalan admisi `inpatientEpisodeService.patch(`${episodeId}/cancel`, { reason })`.
2. Validasi alasan pembatalan tidak boleh kosong atau hanya berisi spasi (`hasMeaningfulReason`).
3. Pastikan reservasi tempat tidur ikut dilepaskan secara otomatis oleh server.

**Acceptance criteria:**
1. Dialog modal meminta alasan pembatalan wajib diisi sebelum tombol konfirmasi aktif.
2. Setelah disubmit, API `PATCH /episodes/{id}/cancel` berhasil terpanggil dengan HTTP 200.
3. Tempat tidur yang tadinya berstatus `Reserved` langsung kembali menjadi `Available` di papan tempat tidur.
4. Di menu **Daftar Kerja Episode (`/health-services/inpatient-management/episodes`)**, episode tersebut berstatus **`Batal`** lengkap dengan alasan pembatalannya.

---

### FIX-EPS-011-03 — Bangun utilitas pembersihan menyeluruh (*clear session & form reset*)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-011-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (utility & flow state) |
| **Berkas yang diubah** | `src/utils/health-services/inpatient-management/inpatient-admission-session-utils.js` (baru)<br>`src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` |
| **Radius dampak** | Manajemen penyimpanan lokal alur admisi |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Buat utilitas `clearInpatientAdmissionSession()` yang menghapus kunci storage penjamin, deposit, dan memo dokter.
2. Sambungkan pemanggilan utilitas ini saat proses pembatalan sukses maupun saat petugas mengonfirmasi "Batal Pendaftaran".
3. Arahkan peramban kembali ke URL bersih `/health-services/inpatient-management/admissions` tanpa query params.

**Acceptance criteria:**
1. Setelah pembatalan berhasil, halaman kembali bersih menampilkan "Pilih Tipe Pendaftaran".
2. Saat petugas memulai pendaftaran pasien baru berikutnya, tidak ada sisa data (penjamin, deposit, atau dokter) dari pasien sebelumnya.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-011-03 (Utilitas clear session & reset form)
       │
       ▼
FIX-EPS-011-02 (Integrasi dialog FE-INP-17 & API PATCH cancel)
       │
       ▼
FIX-EPS-011-01 (Penyematan tombol pada bilah aksi langkah wizard)
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Status |
| ---: | --- | --- | --- |
| K-01 | Peletakan tombol pada Langkah 1–6 (tahap form lokal) | Opsi A: Sediakan tombol "Batal Pendaftaran" (reset form langsung). | **Disetujui Pemilik** pada chat 13-10-2026 |
| K-02 | Retensi riwayat episode saat dibatalkan | Tetap tercatat di Riwayat Episode dengan status Batal (*Cancelled*). | **Disetujui Pemilik** pada chat 13-10-2026 |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Kalimat Lama | Menjadi |
| --- | --- | --- | --- |
| `05-skema-tampilan.md` | Bagian 21, `FE-INP-17` | "Lokasi: dialog aksi dari FE-INP-16 dan FE-INP-04; bukan menu tersendiri." | "Lokasi: dialog aksi dari FE-INP-16, FE-INP-04, dan bilah aksi langkah FE-INP-09 (Booking Bed) & FE-INP-10 (Konfirmasi) pada alur admisi." |

---

## 8. Verifikasi menyeluruh

1. **Uji Batal di Langkah Awal (Langkah 2 Jenis Kunjungan & Kategori):**
   - Buka admisi pasien lama -> klik Batal Pendaftaran -> form bersih kembali ke Pilih Tipe Pendaftaran.
2. **Uji Batal di Langkah Pasca Simpan (Langkah 7 Booking Bed & Langkah 8 Konfirmasi):**
   - Lakukan admisi sampai tahap pilih bed -> lanjut ke booking bed (episode `Draft` lahir, bed `Reserved`).
   - Klik tombol **Batalkan Admisi** -> masukkan alasan: *"Pasien menolak rawat inap"* -> submit.
   - Periksa:
     - Notifikasi sukses muncul.
     - Layar admisi bersih kembali ke menu awal.
     - Buka menu **Daftar Kerja Episode**: baris episode pasien tersebut muncul berstatus **Batal (Cancelled)** dengan alasan tercatat.
     - Buka menu **Papan Tempat Tidur**: tempat tidur yang tadi dipesan sudah kembali **Tersedia (Available)**.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Rencana perbaikan dibuat mencakup 3 butir FIX dan skema tampilan sebelum-sesudah | `diagnose-module-issue` |
| 2026-10-13 | Implementasi FIX-EPS-011-01 s/d 03 selesai: tombol Batal Pendaftaran & Batalkan Admisi terpasang, modal ConfirmModal beralasan terhubung ke API cancel, dan clear session terpadu | `build-module-frontend` |
