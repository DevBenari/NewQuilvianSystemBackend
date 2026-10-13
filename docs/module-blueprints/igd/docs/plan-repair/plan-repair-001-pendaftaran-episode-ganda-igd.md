# PLAN-REPAIR-001 — Peringatan Dini Episode Berjalan dan Kontrol Kolom Episode Ganda pada Langkah 2 Pendaftaran IGD

```yaml
plan_id: PLAN-REPAIR-IGD-001
issue: ../issue/issue-001-pendaftaran-episode-ganda-igd.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-12"
diputuskan_oleh: "Pemilik modul — persetujuan 2026-10-12"
tanggal_keputusan: "2026-10-12"
basis_source_backend: "6a1c6cf9 (rizkiG)"
basis_source_frontend: "701ddfb5d (RizkiV2)"
perubahan_backend: "Tidak ada — seluruh perbaikan berada di frontend"
ditulis_dengan: "skill diagnose-module-issue"
```

Rencana ini menutup keluhan pelapor dan dua temuan pendukung pada [ISSUE-001](../issue/issue-001-pendaftaran-episode-ganda-igd.md). Seluruh perbaikan source berada di **frontend** (`QuilvianSystemFrontendDev`), karena backend sudah memiliki endpoint pra-cek baca-saja `GET /v1/health-services/emergency-installation-management/emergency-visits/active-episode?patientId=` yang siap pakai (`BE-IGD-050`).

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-001-01` | `ISS-IGD-001-01` | Pindahkan pra-cek ke pemilihan pasien; tampilkan alert peringatan dan aktifkan kolom episode ganda hanya saat ada episode aktif di Langkah 2 | FE | 1 | — | ✅ SELESAI 2026-10-12 | `emergency-visit-step.jsx`, `use-emergency-registration.js` |
| `FIX-IGD-001-02` | `ISS-IGD-001-T1` | Nonaktifkan (*disabled*) dan bersihkan otomatis (*auto-clear*) isian alasan episode ganda bila pasien tidak memiliki episode aktif | FE | 1 | — | ✅ SELESAI 2026-10-12 | `emergency-visit-step.jsx`, `use-emergency-registration.js` |
| `FIX-IGD-001-03` | `ISS-IGD-001-T2` | Pertahankan pra-cek verifikasi akhir sebagai jaring pengaman sekunder (*fail-safe*) | FE | 2 | — | ✅ SELESAI 2026-10-12 | `use-emergency-registration.js` |

**Ringkasan: 3 dari 3 perbaikan selesai (12 Oktober 2026). ESLint 0 error; unit test 20/20 PASS.**

---

## 2. Solusi terpilih per temuan

### 2.1 ISS-IGD-001-01 — Pemeriksaan Kunjungan Berjalan di Langkah 2

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Jalankan `lookupActiveEmergencyEpisode` saat pasien lama dipilih di Langkah 1; simpan hasil di state controller `useEmergencyRegistration`; teruskan ke Langkah 2 untuk menampilkan `EmergencyInlineAlert` dan mengontrol properti `disabled` pada `duplicateEpisodeOverrideReason` | Paling mulus: data episode aktif sudah siap saat Langkah 2 terbuka (tanpa *loading delay* di Langkah 2); memanfaatkan fungsi utilitas yang sudah ada | State controller bertambah satu properti status episode aktif |
| **B** | Panggil `lookupActiveEmergencyEpisode` di dalam `useEffect` komponen `EmergencyVisitStep` | Terisolasi di dalam file Langkah 2 saja | Muncul jeda *loading* visual sesaat setelah memasuki Langkah 2; jika pengguna bergerak cepat, peringatan baru muncul belakangan |
| **C** | Tampilkan dialog konfirmasi modal popup yang memblokir saat menekan "Lanjut ke Langkah 2" | Memaksa petugas membaca peringatan | Mengganggu kelancaran input petugas di IGD yang menuntut kecepatan (*modal fatigue*) |

**Solusi terpilih: Opsi A.**
- Menggunakan endpoint yang sudah tersedia di backend (`GET emergency-visits/active-episode?patientId=`).
- Hasil pengecekan langsung mengontrol tampilan Langkah 2:
  1. Bila `hasActiveEpisode: true`: Tampilkan `EmergencyInlineAlert tone="warning"` berisi nomor kunjungan/encounter berjalan, dan kolom isian alasan pendaftaran episode ganda menjadi **AKTIF (enabled)** serta wajib diisi sebelum lanjut ke langkah berikutnya.
  2. Bila `hasActiveEpisode: false`: Tidak menampilkan alert peringatan, dan kolom isian alasan pendaftaran episode ganda menjadi **NONAKTIF (disabled)**.

---

### 2.2 ISS-IGD-001-T1 — Pencegahan Galat 400 Bad Request Isian Alasan Tanpa Episode Aktif

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Berikan prop `disabled={!hasActiveEpisode}` pada `EmergencyTextField` dan lakukan `setValue("duplicateEpisodeOverrideReason", "")` saat `hasActiveEpisode` bernilai `false` | Menjamin form state bersih; mencegah pengguna mengetik teks yang pasti ditolak backend; tata letak visual tetap konsisten | Kolom tetap terlihat abu-abu (disabled) |
| **B** | Sembunyikan sama sekali (*conditional render*) kolom `duplicateEpisodeOverrideReason` jika `!hasActiveEpisode` | Form terlihat lebih bersih | Tata letak form berubah-ubah (tinggi form melompat); petugas tidak tahu bahwa fitur pendaftaran ganda tersedia |

**Solusi terpilih: Opsi A.**
Dengan mempertahankan kolom dalam keadaan `disabled` saat kunjungan aktif tidak ada, petugas mendapatkan kepastian visual bahwa pendaftaran ini adalah pendaftaran kunjungan normal tanpa perlu mengisi alasan ganda.

---

### 2.3 ISS-IGD-001-T2 — Penanganan Langkah Verifikasi

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Tetapkan pra-cek pada `handleSubmitRegistration` sebagai lapis kedua (*fail-safe*); jika lolos validasi Langkah 2, verifikasi akhir akan langsung sukses | Aman dari pendaftaran ganda yang terjadi serentak di dua loket berbeda; tidak mengubah kontrak backend | Tidak ada |
| **B** | Hapus pra-cek dari `handleSubmitRegistration` sepenuhnya | Mengurangi 1 pemanggilan API | Berisiko membuat encounter yatim bila dua loket mendaftarkan pasien yang sama dalam hitungan detik |

**Solusi terpilih: Opsi A.**

---

## 3. Skema tampilan sebelum → sesudah

### Skema Langkah 2: Data Emergency Visit

#### Sebelum Perbaikan (Kondisi Saat Ini — Semua Pasien Mendapat Tampilan yang Sama)

```text
+-------------------------------------------------------------------------------+
| LANGKAH 2                                                           < Kembali |
| Data Emergency Visit                                                          |
| Isi kategori kunjungan dan keluhan utama...                                   |
+-------------------------------------------------------------------------------+
| Jenis Kunjungan Pasien                                                        |
| +-----------------------------------+ +-------------------------------------+ |
| | [v] Umum                          | | [ ] Rujukan / Tindak Lanjut         | |
| +-----------------------------------+ +-------------------------------------+ |
|                                                                               |
| Keluhan Utama                                                                 |
| +---------------------------------------------------------------------------+ |
| | Masukkan keluhan utama saat pasien datang ke IGD                          | |
| +---------------------------------------------------------------------------+ |
|                                                                               |
| Alasan Pendaftaran Episode Ganda                                              |
| +---------------------------------------------------------------------------+ |
| | Kosongkan untuk pendaftaran normal. Isi hanya bila pasien benar-benar      | |
| | datang kembali sementara episode lama belum ditutup.                      | |
| +---------------------------------------------------------------------------+ |
| (Kolom selalu TERBUKA & EDITABLE, tidak ada alert kunjungan berjalan)         |
|                                                                               |
| [ Kembali ]                                            [ Lanjut ke Pembayaran ]
+-------------------------------------------------------------------------------+
```

---

#### Sesudah Perbaikan — Keadaan A: Pasien Memiliki Kunjungan/Encounter Berjalan (`hasActiveEpisode: true`)

```text
+-------------------------------------------------------------------------------+
| LANGKAH 2                                                           < Kembali |
| Data Emergency Visit                                                          |
| Isi kategori kunjungan dan keluhan utama...                                   |
+-------------------------------------------------------------------------------+
| Jenis Kunjungan Pasien                                                        |
| +-----------------------------------+ +-------------------------------------+ |
| | [v] Umum                          | | [ ] Rujukan / Tindak Lanjut         | |
| +-----------------------------------+ +-------------------------------------+ |
|                                                                               |
| Keluhan Utama                                                                 |
| +---------------------------------------------------------------------------+ |
| | Sesak napas memberat sejak 30 menit yang lalu                             | |
| +---------------------------------------------------------------------------+ |
|                                                                               |
| [!] PERINGATAN: Pasien Memiliki Kunjungan IGD yang Masih Berjalan             |
|     Pasien ini masih memiliki kunjungan IGD aktif (No: IGD-20261012-0045,      |
|     Status: Menunggu Triage). Bila kedatangan ini merupakan peristiwa baru,   |
|     wajib isi alasan pendaftaran episode ganda di bawah ini.                  |
|                                                                               |
| Alasan Pendaftaran Episode Ganda *                                            |
| +---------------------------------------------------------------------------+ |
| | Pasien datang kembali karena sesak napas bertambah parah                   | |
| +---------------------------------------------------------------------------+ |
| (Kolom AKTIF dan WAJIB DIISI agar dapat melanjutkan)                          |
|                                                                               |
| [ Kembali ]                                            [ Lanjut ke Pembayaran ]
+-------------------------------------------------------------------------------+
```

---

#### Sesudah Perbaikan — Keadaan B: Pasien Normal / Pasien Baru / Tanpa Kunjungan Berjalan (`hasActiveEpisode: false`)

```text
+-------------------------------------------------------------------------------+
| LANGKAH 2                                                           < Kembali |
| Data Emergency Visit                                                          |
| Isi kategori kunjungan dan keluhan utama...                                   |
+-------------------------------------------------------------------------------+
| Jenis Kunjungan Pasien                                                        |
| +-----------------------------------+ +-------------------------------------+ |
| | [v] Umum                          | | [ ] Rujukan / Tindak Lanjut         | |
| +-----------------------------------+ +-------------------------------------+ |
|                                                                               |
| Keluhan Utama                                                                 |
| +---------------------------------------------------------------------------+ |
| | Demam tinggi dan nyeri perut                                              | |
| +---------------------------------------------------------------------------+ |
|                                                                               |
| Alasan Pendaftaran Episode Ganda                                              |
| +---------------------------------------------------------------------------+ |
| | Tidak berlaku (Pasien tidak memiliki kunjungan IGD yang sedang berjalan)  | |
| +---------------------------------------------------------------------------+ |
| (Kolom DINONAKTIFKAN / DISABLED, nilai otomatis kosong, tidak ada alert)      |
|                                                                               |
| [ Kembali ]                                            [ Lanjut ke Pembayaran ]
+-------------------------------------------------------------------------------+
```

---

### Tabel Wilayah, Tombol, dan Keadaan

| Wilayah | Elemen | Perilaku saat `hasActiveEpisode: true` | Perilaku saat `hasActiveEpisode: false` |
| --- | --- | --- | --- |
| Peringatan | `EmergencyInlineAlert` | Tampil (tone="warning"): Menampilkan nomor kunjungan/encounter aktif dan instruksi | Disembunyikan (tidak dirender) |
| Form Input | `duplicateEpisodeOverrideReason` | **Aktif (`disabled={false}`)**, wajib diisi (`required: true`) | **Nonaktif (`disabled={true}`)**, placeholder/helper text penjelas, nilai direset ke `""` |
| Tombol Navigasi | `Lanjut ke Pembayaran` / `Lanjut ke Data Rujukan` | Hanya dapat lanjut bila keluhan utama terisi DAN alasan episode ganda terisi | Lanjut normal (hanya memeriksa keluhan utama) |

---

## 4. Rincian perbaikan

### FIX-IGD-001-01 & FIX-IGD-001-02 — Integrasi Pra-Cek ke Hook dan Langkah 2

#### Berkas yang diubah:
1. `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js`
2. `src/components/view/health-services/registration-management/emergency-registration/emergency-visit-step.jsx`
3. `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-page.jsx`

#### Perubahan spesifik:
1. **Di `use-emergency-registration.js`:**
   - Tambahkan state `activeEpisodeCheck: { loading: false, hasActiveEpisode: false, visit: null, encounter: null }`.
   - Di dalam fungsi `handleChooseExistingPatient` dan `handleSelectExistingPatient`, segera jalankan `lookupActiveEmergencyEpisode(targetPatientId)` dan simpan hasilnya ke state `activeEpisodeCheck`.
   - Untuk pasien baru (`patientMode === PATIENT_ENTRY_MODE.NEW`), set `activeEpisodeCheck` ke `{ loading: false, hasActiveEpisode: false, visit: null, encounter: null }`.
   - Di dalam `handleEmergencyVisitStepNext`:
     - Jika `activeEpisodeCheck.hasActiveEpisode === true` dan nilai `duplicateEpisodeOverrideReason` kosong, gagalkan navigasi ke langkah berikutnya dengan pesan: *"Alasan pendaftaran episode ganda wajib diisi karena pasien masih memiliki kunjungan IGD yang berjalan."*
     - Jika `activeEpisodeCheck.hasActiveEpisode === false`, pastikan `setValue("duplicateEpisodeOverrideReason", "")` agar tidak ada nilai tertinggal.
   - Ekspos `activeEpisodeCheck` ke return object hook.
2. **Di `emergency-registration-page.jsx`:**
   - Teruskan prop `activeEpisodeCheck` dari `registrationController` ke `<EmergencyVisitStep activeEpisodeCheck={activeEpisodeCheck} ... />`.
3. **Di `emergency-visit-step.jsx`:**
   - Terima prop `activeEpisodeCheck`.
   - Jika `activeEpisodeCheck?.hasActiveEpisode === true`:
     - Render `EmergencyInlineAlert tone="warning"` tepat di atas kolom Alasan Pendaftaran Episode Ganda.
     - Berikan prop `disabled={false}` dan `rules={{ required: "Alasan pendaftaran episode ganda wajib diisi" }}` pada `EmergencyTextField`.
   - Jika `activeEpisodeCheck?.hasActiveEpisode === false`:
     - Jangan render alert.
     - Berikan prop `disabled={true}` pada `EmergencyTextField`, dengan placeholder *"Tidak perlu diisi untuk pendaftaran normal (tidak ada kunjungan aktif)"*.

#### Acceptance Criteria (Kriteria Keberterimaan):
- **AC-01:** Saat memilih Pasien Lama yang memiliki kunjungan IGD berjalan, Langkah 2 langsung menampilkan kotak peringatan kuning berisi informasi nomor kunjungan/encounter berjalan.
- **AC-02:** Pada kondisi AC-01, kolom *Alasan Pendaftaran Episode Ganda* aktif dan wajib diisi sebelum petugas dapat mengklik tombol *Lanjut ke Pembayaran* / *Lanjut ke Data Rujukan*.
- **AC-03:** Saat memilih Pasien Lama yang tidak memiliki kunjungan aktif, atau saat mendaftarkan Pasien Baru, tidak ada kotak peringatan yang muncul, dan kolom *Alasan Pendaftaran Episode Ganda* berstatus `disabled`.
- **AC-04:** Nilai kolom *Alasan Pendaftaran Episode Ganda* dipastikan kosong (`""`) saat pasien tidak memiliki kunjungan aktif, sehingga tidak ada galat *400 Bad Request* dari backend.
- **AC-05:** Pada Langkah Verifikasi, pendaftaran dapat diselesaikan langsung tanpa terhenti oleh validasi alasan episode ganda yang terlambat.

---

### FIX-IGD-001-03 — Jaring Pengaman Sekunder di Verifikasi Akhir

#### Berkas yang diubah:
- `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js`

#### Perubahan spesifik:
- Logika pengecekan di `handleSubmitRegistration` tetap dipertahankan apa adanya sebagai pengaman lapis kedua untuk menangani *race condition* (misalnya ada petugas lain yang mendaftarkan pasien yang sama beberapa detik sebelumnya).

---

## 5. Urutan pengerjaan

```text
[FIX-IGD-001-01] Kaitkan pra-cek di use-emergency-registration & ekspos activeEpisodeCheck
       │
       ▼
[FIX-IGD-001-02] Perbarui emergency-visit-step.jsx (render alert kondisional & disabled kolom)
       │
       ▼
[FIX-IGD-001-03] Verifikasi jaring pengaman sekunder & uji unit regression
```

---

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari dokumen issue:
1. **K-01:** Bentuk tampilan kolom "Alasan Pendaftaran Episode Ganda" saat pasien tidak memiliki episode aktif.
   - Rekomendasi: `disabled` dengan placeholder penjelas (menjaga konsistensi tata letak form).
2. **K-02:** Waktu eksekusi pra-cek episode berjalan.
   - Rekomendasi: Saat pasien dipilih di Langkah 1 (segera tersedia sebelum membuka Langkah 2).

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Alasan revisi |
| --- | --- | --- |
| `NewQuilvianSystemBackend/docs/module-blueprints/igd/03-frontend-architecture.md` | Bagian Alur Pendaftaran IGD (Langkah 2) | Menambahkan spesifikasi bahwa Langkah 2 memuat pra-cek episode aktif dan mengontrol status aktif kolom episode ganda |
| `NewQuilvianSystemBackend/docs/module-blueprints/igd/roadmap/frontend-roadmap.md` | Bagian R3.11 / catatan `FE-IGD-038` | Menambahkan referensi perbaikan `FIX-IGD-001-01` untuk menyempurnakan penempatan pra-cek dari verifikasi akhir ke Langkah 2 |

---

## 8. Verifikasi menyeluruh

| No | Skenario Uji | Tindakan Penguji | Hasil yang Diharapkan |
| :---: | --- | --- | --- |
| 1 | Pasien baru (belum pernah terdaftar) | Pilih "Pasien Baru", isi data identitas di Langkah 1, lanjut ke Langkah 2 | Tidak ada alert kuning. Kolom "Alasan Pendaftaran Episode Ganda" berstatus disabled. Petugas dapat lanjut ke pembayaran normal. |
| 2 | Pasien lama tanpa kunjungan aktif | Cari pasien yang tidak sedang dirawat di IGD, klik pilih, masuk ke Langkah 2 | Tidak ada alert kuning. Kolom alasan berstatus disabled. Nilai alasan kosong. Pendaftaran tersimpan sukses di akhir tanpa galat 400. |
| 3 | Pasien lama dengan kunjungan aktif (belum selesai) | Cari pasien yang saat ini berstatus "Menunggu Triage" atau sedang aktif di IGD, masuk ke Langkah 2 | Kotak alert kuning muncul menampilkan nomor kunjungan aktif. Kolom alasan berstatus aktif. Bila alasan dikosongkan dan tombol lanjut ditekan, sistem menolak dengan pesan wajib isi. |
| 4 | Pasien lama dengan kunjungan aktif + alasan diisi | Pada skenario 3, isi alasan "Pasien datang kembali dengan keluhan baru", lalu lanjut | Langkah berikutnya terbuka normal, data rujukan/pembayaran terisi, dan saat verifikasi akhir diklik "Selesaikan Pendaftaran", pendaftaran tersimpan sukses tanpa penolakan. |

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Rencana perbaikan dibuat menanggapi ISSUE-IGD-001 | `diagnose-module-issue` |
