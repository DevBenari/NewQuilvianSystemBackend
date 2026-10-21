# PLAN-REPAIR-025 — Perbaikan Penumpukan Dialog Konfirmasi Mutasi Master dan Indikator Kolom Wajib pada Modal Denah

```yaml
plan_id: PLAN-REPAIR-EPS-025
issue: ../issue/issue-025-konfirmasi-mutasi-tertutup-modal-denah.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-17"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "c2bc563e (MHamzah)"
basis_source_frontend: "15ae73984 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Solusi | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-EPS-025-01` | `ISS-EPS-025-01` | Koreksi hierarki *stacking context* dan sembunyikan modal denah sementara saat dialog konfirmasi mutasi aktif | Frontend | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-025-02` | `ISS-EPS-025-02` | Berikan indikator visual tanda bintang merah (`*`) untuk kolom wajib dan validasi proaktif pada modal denah | Frontend | 2 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 2 perbaikan selesai (rencana siap diajukan untuk persetujuan pemilik).**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-025-01 — Dialog konfirmasi mutasi master tertutup di balik modal pop-up denah

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Sembunyikan pop-up denah secara kondisional saat `state.mutation.show === true`, DAN sematkan kelas layer konfirmasi mutasi denah pada `ConfirmModal` dengan `z-index: 3000` | Menghilangkan konflik visual sepenuhnya; fokus pengguna 100% pada dialog persetujuan; jika batal, form kembali utuh tanpa kehilangan data | Membutuhkan pengaturan visibilitas popup berbasis state mutasi |
| **B** | Mengubah z-index global `.region-modal` di `modal-confirm.css` menjadi `3000` | Sangat singkat, satu baris CSS | Berisiko memengaruhi seluruh modal Bootstrap lain di modul farmasi, kasir, dan pendaftaran |
| **C** | Menggabungkan langkah konfirmasi mutasi langsung di dalam modal denah tanpa memakai `BedMasterMutationConfirmation` terpisah | Tidak perlu ada 2 modal bertumpuk | Melanggar arsitektur standar Quilvian di mana seluruh mutasi master bed wajib melewati komponen gerbang `BedMasterMutationConfirmation` |

**Solusi terpilih: Opsi A.**

1. Menjaga kepatuhan pada invariant tata kelola rumah sakit: `BedMasterMutationConfirmation` tetap dipakai sebagai gerbang persetujuan operasional sebelum data dikirim ke API.
2. Ketika tombol "Periksa Perubahan" ditekan dan valid, pop-up denah di latar belakang disembunyikan sementara (`mutation.show === true`), lalu `ConfirmModal` tampil tegas dan jelas di tengah layar.
3. Bila pengguna menekan "Batal", pop-up denah otomatis muncul kembali dengan nilai isian form (nama kamar, unit, wing, alasan) yang tetap tersimpan tanpa ter-reset.

*Kenapa bukan opsi lain:* Opsi B memiliki radius dampak global yang terlalu luas pada komponen CSS bersama, sedangkan Opsi C melanggar konsistensi arsitektur konfirmasi mutasi master bed.

---

### ISS-EPS-025-02 — Ketiadaan penanda visual kolom wajib dan validasi proaktif

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Tambahkan prop `required` pada komponen `Field` di modal denah, render tanda bintang merah (`*`), dan tampilkan pesan ringkas jika kolom mandatory masih kosong saat submit | Jelas bagi pengguna, konsisten dengan standar form Quilvian, tidak mengandalkan tooltip native browser yang sering tersembunyi | Perlu menambahkan prop `required` pada pemanggilan `Field` |
| **B** | Nonaktifkan tombol `Periksa Perubahan` (`disabled`) sampai semua kolom terisi | Tombol tidak bisa diklik jika form belum valid | Melanggar prinsip UI Quilvian: tombol mati membingungkan pengguna karena tidak memberitahu alasan kenapa tombol mati |
| **C** | Hanya mengandalkan atribut HTML5 `required` bawaan browser | Tidak ada perubahan kode baru | Terbukti gagal di lapangan karena peramban memblokir submit form secara pasif tanpa pesan yang disadari pengguna |

**Solusi terpilih: Opsi A.**

1. Memenuhi standar konsistensi form Quilvian: setiap isian wajib diberi tanda bintang merah (`*`).
2. Label yang jelas (misal: `Unit rawat inap (nama · kode) *` dan `Alasan perubahan operasional *`) mencegah pengguna melewatkan isian penting.
3. Memberikan umpan balik langsung jika pengguna menekan tombol saat masih ada isian kosong, sehingga tidak terjadi kebingungan *"kenapa tombol tidak bisa diklik"*.

*Kenapa bukan opsi lain:* Opsi B melanggar aturan keramahan pengguna Quilvian, sedangkan Opsi C adalah kondisi eksisting yang menjadi sumber keluhan pelapor.

---

## 3. Skema Tampilan: Sebelum → Sesudah

### Tampilan Sebelum (Dialog Konfirmasi Tertutup di Belakang Modal Denah)

```text
Layer Depan (z-index: 2000) — Tampak di Layar Pengguna:
┌──────────────────────────────────────────────────────────────┐
│                      EDITOR MASTER BED                    [X]│
│                           VIP 303                            │
│                                                              │
│  Nomor kamar                                                 │
│  [ 303                                                     ] │
│  Kelas pasien (nama · kode)                                  │
│  [ VIP · CLS-10                                            ▾]│
│  Unit rawat inap (nama · kode)                               │
│  [ Rawat Inap · SU-IPD-001                                 ▾]│
│  Arah wing                                                   │
│  [ Barat Daya                                              ▾]│
│  Jumlah bed: [ 1 ]                 Jumlah boks bayi: [ 0 ]   │
│  Alasan perubahan operasional                                │
│  [ bismillah                                               ] │
│                                                              │
│  [ Batal ]                                [ Periksa Perubahan]
└──────────────────────────────────────────────────────────────┘
                         ▲
                         │ (TERTUTUP TOTAL DI BELAKANG)
                         ▼
Layer Belakang (z-index: 1200) — Tersembunyi:
┌─────────────────────────────────────────┐
│     Konfirmasi Perubahan Master         │
│  bismillah                              │
│  Bed baru akan mengikuti kesiapan server│
│  [ Batal ]        [ Konfirmasi Simpan ] │
└─────────────────────────────────────────┘
```

---

### Tampilan Sesudah (Pop-Up Bergantian Secara Rapi & Berindikator Kolom Wajib)

#### Fase 1: Form Pengisian Modal Denah (Dengan Indikator Bintang Merah)
```text
┌──────────────────────────────────────────────────────────────┐
│                      EDITOR MASTER BED                    [X]│
│                           VIP 303                            │
│                                                              │
│  Nomor kamar *                                               │
│  [ 303                                                     ] │
│  Kelas pasien (nama · kode) *                                │
│  [ VIP · CLS-10                                            ▾]│
│  Unit rawat inap (nama · kode) *                             │
│  [ Rawat Inap · SU-IPD-001                                 ▾]│
│  Arah wing *                                                 │
│  [ Barat Daya                                              ▾]│
│  Jumlah bed *: [ 1 ]               Jumlah boks bayi *: [ 0 ] │
│  Alasan perubahan operasional *                              │
│  [ Aktivasi kamar VIP 303 di denah                         ] │
│                                                              │
│  [ Batal ]                                [ Periksa Perubahan]
└──────────────────────────────────────────────────────────────┘
```

#### Fase 2: Setelah "Periksa Perubahan" Ditekan (Dialog Konfirmasi Tampil di Depan)
```text
┌──────────────────────────────────────────────────────────────┐
│                 Konfirmasi Perubahan Master                  │
│                                                              │
│  Alasan Operasional:                                         │
│  "Aktivasi kamar VIP 303 di denah"                           │
│                                                              │
│  Detail Perubahan:                                           │
│  • Kamar baru: VIP 303 (Unit: Rawat Inap · SU-IPD-001)       │
│  • Bed baru akan mengikuti pemeriksaan kesiapan server.      │
│                                                              │
│  [i] Bed terisi atau dipesan tetap dilindungi server.        │
│                                                              │
│  [ Batal ]                              [ Konfirmasi Simpan ]│
└──────────────────────────────────────────────────────────────┘
*(Catatan: Modal denah di belakang disembunyikan sementara, sehingga dialog konfirmasi terlihat 100% tanpa halangan)*
```

---

### Tabel Wilayah, Tombol, dan Keadaan

#### Tabel Wilayah
| Wilayah | Isi | Sumber Data | Komponen |
| :--- | :--- | :--- | :--- |
| **Form Editor Denah** | Kolom nomor, kelas, unit, wing, kapasitas bed/boks, alasan dengan tanda bintang `*` | `room`, `state.patientClassOptions`, `state.serviceUnitOptions` | `ActionForm` (`Field` ber-prop `required`) |
| **Dialog Konfirmasi Mutasi** | Ringkasan alasan operasional, status bed, dan peringatan kesiapan | `state.mutation` | `BedMasterMutationConfirmation` (`ConfirmModal`) |

#### Tabel Tombol
| Tombol | Jenis | Kapan Aktif | Yang Terjadi |
| :--- | :--- | :--- | :--- |
| **Periksa Perubahan** | Primary | Selalu dapat ditekan; memvalidasi kelengkapan form | Membuka dialog `ConfirmModal` di lapisan depan dan menyembunyikan sementara form denah |
| **Batal (pada form denah)** | Secondary | Selalu aktif | Menutup form aksi dan kembali ke ringkasan kamar |
| **Konfirmasi Simpan** | Primary (Warning) | Selalu aktif di dialog konfirmasi | Mengeksekusi mutasi API backend (`activateRoom`), memperbarui denah, dan menutup seluruh modal |
| **Batal (pada konfirmasi)** | Secondary | Selalu aktif di dialog konfirmasi | Menutup dialog konfirmasi dan menampilkan kembali formulir denah dengan isian utuh |

---

## 4. Rincian Perbaikan

### FIX-EPS-025-01 — Koreksi Stacking Context & Visibilitas Modal Konfirmasi Mutasi Denah

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-025-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`floor-plan-master-room-popup.jsx`, `floor-plan.module.css`, `bed-master-mutation-confirmation.jsx`) |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-master-room-popup.jsx`<br>`QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/bed-master-mutation-confirmation.jsx` |
| **Radius dampak** | Layar Editor Denah Master Bed (`/health-services/master-data/bed`) |
| **Bergantung pada** | Persetujuan pemilik sistem |

**Langkah:**
1. Pada `FloorPlanMasterRoomPopup.jsx`, tambahkan kondisi: bila `state.mutation.show` bernilai `true`, sembunyikan rendering div `.dialogBackdrop` (atau berikan kelas utilitas `display: none`), sehingga `ConfirmModal` dapat tampil langsung di hadapan pengguna tanpa terhalang.
2. Pada `BedMasterMutationConfirmation.jsx`, tambahkan prop kelas modal `modalClassName={styles.bedMasterConfirmModal}` dengan z-index cadangan `z-index: 3000 !important` untuk memastikan modal konfirmasi selalu berada di atas layer mana pun.
3. Pastikan penekanan tombol "Batal" pada `BedMasterMutationConfirmation` memanggil `mutation.cancel()` yang memulihkan kembali tampilan modal denah dengan data yang belum hilang.

**Acceptance Criteria:**
1. Pengguna membuka modal aktivasi kamar denah (misal: VIP 303).
2. Pengguna mengisi seluruh field dan alasan operasional `"Aktivasi kamar VIP 303 di denah"`.
3. Pengguna mengklik tombol **"Periksa Perubahan"**.
4. Dialog **"Konfirmasi Perubahan Master"** langsung muncul di tengah layar secara jelas dan dapat diklik.
5. Bila pengguna mengklik **"Batal"** pada dialog konfirmasi, formulir aktivasi kamar kembali muncul dengan seluruh isian tetap lengkap.
6. Bila pengguna mengklik **"Konfirmasi Simpan"**, request POST/PUT terkirim ke server, data kamar tersimpan, dan modal tertutup otomatis.

**Verifikasi:**
1. `npm run lint` pada frontend lolos tanpa galat.
2. Uji langsung di browser: aktivasi kamar belum dipetakan berhasil menampilkan dialog konfirmasi dan menyimpan kamar baru ke database.

**Risiko:**
State form ter-reset jika komponen ter-unmount secara tidak sengaja saat menyembunyikan modal. Pencegahan: sembunyikan visual modal melalui CSS (`className` kondisional atau atribut `hidden`) tanpa me-reset state `ActionForm`.

---

### FIX-EPS-025-02 — Penambahan Indikator Visual Kolom Wajib dan Validasi Form Modal Denah

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-025-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`floor-plan-master-room-popup.jsx`, `floor-plan.module.css`) |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-master-room-popup.jsx`<br>`QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` |
| **Radius dampak** | Layar Editor Denah Master Bed (`/health-services/master-data/bed`) |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Perbarui komponen `Field` pada `floor-plan-master-room-popup.jsx`:
   ```jsx
   function Field({ label, children, hint, required }) {
     return (
       <label className={styles.masterField}>
         <span>
           {label}
           {required ? <span className={styles.requiredAsterisk}> *</span> : null}
         </span>
         {children}
         {hint ? <small>{hint}</small> : null}
       </label>
     );
   }
   ```
2. Berikan prop `required={true}` pada label: Nomor kamar, Kelas pasien, Unit rawat inap, Arah wing, Jumlah bed, dan Alasan perubahan operasional.
3. Berikan styling `.requiredAsterisk { color: var(--color-danger, #dc2626); font-weight: bold; }` di `floor-plan.module.css`.

**Acceptance Criteria:**
1. Semua kolom wajib di modal denah memiliki tanda bintang merah `*` yang kontras dan jelas di samping labelnya.
2. Pengguna dapat dengan mudah membedakan kolom wajib dan opsional sebelum menekan tombol simpan.

**Verifikasi:**
1. `npm run lint` pada frontend lolos 0 error.
2. Visual check pada browser memastikan tanda bintang merah tampak rapi.

**Risiko:**
Perubahan kosmetik berisiko sangat rendah dan tidak mempengaruhi logika bisnis.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-025-02 (Penanda visual kolom wajib) ──► boleh dikerjakan segera
K-01 (Persetujuan Pemilik Sistem)
└── FIX-EPS-025-01 (Koreksi penumpukan dialog konfirmasi mutasi)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No. | Keputusan yang Dibutuhkan | Dampak Bila Ditunda | Rekomendasi Solusi |
| :---: | :--- | :--- | :--- |
| **K-01** | Persetujuan mekanisme perbaikan penumpukan modal: menyembunyikan pop-up denah sementara saat konfirmasi aktif vs menaikkan z-index konfirmasi ke level 3000 | `FIX-EPS-025-01` tertahan; aktivasi kamar denah tetap tidak dapat diselesaikan pengguna | Setujui Opsi A (menyembunyikan sementara form denah di latar belakang dan menaikkan z-index konfirmasi ke 3000). |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Kalimat Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/05-skema-tampilan.md` | Bagian Modal Editor Denah Master Bed | Dialog konfirmasi mutasi dibuka di atas denah | Dialog konfirmasi mutasi dibuka sebagai modal prioritas utama di depan pengguna dengan menyembunyikan sementara pop-up denah latar belakang |

---

## 8. Verifikasi Menyeluruh

Setelah perbaikan diimplementasikan (sesudah persetujuan pemilik):
1. **Langkah 1 (Kolom Kosong):** Buka kamar belum dipetakan. Periksa apakah tanda `*` merah muncul di label Unit dan Alasan. Kosongkan salah satunya dan klik "Periksa Perubahan", pastikan peramban atau UI memberi peringatan yang jelas.
2. **Langkah 2 (Kolom Lengkap):** Isi seluruh kolom wajib termasuk alasan operasional. Klik "Periksa Perubahan".
3. **Langkah 3 (Dialog Konfirmasi Tampil):** Verifikasi bahwa dialog "Konfirmasi Perubahan Master" langsung muncul di tengah layar secara utuh dan jelas di lapisan terdepan.
4. **Langkah 4 (Uji Batal):** Klik "Batal" pada dialog konfirmasi, pastikan form denah kembali terlihat dengan isian yang tidak hilang.
5. **Langkah 5 (Uji Simpan):** Klik "Periksa Perubahan" lagi, lalu klik "Konfirmasi Simpan". Verifikasi request POST/PUT berhasil terkirim ke backend, kamar berhasil aktif di denah, dan modal tertutup otomatis.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-17 | Rencana perbaikan dibuat mencakup 2 butir perbaikan (FIX-EPS-025-01 dan FIX-EPS-025-02); status rencana: MENUNGGU_PERSETUJUAN (sesuai instruksi: dicatat sebagai issue terlebih dahulu tanpa eksekusi langsung). | Antigravity (`diagnose-module-issue`) |
