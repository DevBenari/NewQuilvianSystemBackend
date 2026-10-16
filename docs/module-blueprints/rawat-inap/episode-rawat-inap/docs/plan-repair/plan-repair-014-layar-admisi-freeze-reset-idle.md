# PLAN-REPAIR-014 — Perbaikan Penanganan Kedaluwarsa Reservasi Tempat Tidur dan Persistensi Alur Admisi Rawat Inap

```yaml
plan_id: PLAN-REPAIR-EPS-014
issue: ../issue/issue-014-layar-admisi-freeze-reset-idle.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "Pemilik Sistem (Pengguna)"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-014-01` | `ISS-EPS-014-01` | Implementasikan Modal Dialog Kedaluwarsa Reservasi Tempat Tidur ramah, peringatan dini (<= 2 menit), tombol perpanjang waktu, dan hapus pemindahan diam-diam (*silent rollback*) agar tombol tidak terkunci tanpa penjelasan | FE | 1 | — | SELESAI | `use-inpatient-admission-bed.jsx:104,188,235,631`, `inpatient-admission-bed-step.jsx:205,259`, `inpatient-admission-view.jsx:1029,1286` |
| `FIX-EPS-014-02` | `ISS-EPS-014-02` | Tambahkan mekanisme persistensi sesi alur admisi (`patientType`, `visitType`, `referralData`) ke `sessionStorage` agar data tidak hilang (*fresh/blank*) saat tab idle/hibernasi | FE | 2 | — | SELESAI | `use-inpatient-admission-flow.jsx:20,126,306,462` |
| `FIX-EPS-014-03` | `ISS-EPS-014-03` | Standarisasi siklus hidup modal Bootstrap pada `InpatientAdmissionView` dan isolasi portal React secara deterministik | FE | 3 | — | SELESAI | `inpatient-admission-view.jsx:1255-1278` |

**Ringkasan: 3 dari 3 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-014-01 — Kedaluwarsa Reservasi Bed Memicu Pemindahan Balik Otomatis yang Mengosongkan Bed

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Pertahankan pemindahan otomatis tanpa dialog)** | Tetap menggunakan `useEffect` yang memanggil `onBack()` seketika token bertambah tanpa dialog apa pun. | Tidak memerlukan komponen UI tambahan. | Pengguna tetap bingung, merasa form ter-reset sendiri (*fresh*), tombol *"Lanjut"* mati (*freeze*), dan menimbulkan komplain berulang. |
| **Opsi 2 (Modal Dialog Peringatan Kedaluwarsa Interaktif + Peringatan Dini + Perpanjang Waktu - Terpilih)** | Saat timer hitung mundur aktif: (1) Menampilkan peringatan visual (warna oranye/merah) ketika waktu tersisa <= 2 menit dilengkapi tombol *"Perpanjang Waktu (+5 Menit)"*, (2) Jika waktu benar-benar habis, server tetap melepaskan reservasi bed demi keselamatan data RS, (3) UI TIDAK memanggil `onBack()` diam-diam, melainkan memunculkan dialog ramah: *"Batas Waktu Reservasi Sementara Habis. Alokasi tempat tidur telah dilepaskan kembali demi ketersediaan rawat inap. Silakan pilih kembali tempat tidur yang Anda inginkan"*, (4) Menyediakan tombol aksi *"Pilih Ulang Tempat Tidur"* yang mengarahkan kembali ke langkah pemilihan bed secara sadar dengan banner panduan yang jelas. | Sangat transparan, sesuai invariant `03-frontend-architecture.md` bagian 5.5 ("layar wajib menyatakannya"), mengedukasi petugas loket mengenai operasional bed RS, mencegah salah paham sistem macet (*freeze*), dan memberi fleksibilitas perpanjangan waktu. | Memerlukan penambahan state dialog kedaluwarsa dan tombol perpanjangan waktu pada hook/komponen bed. |

**SOLUSI TERPILIH: Opsi 2 (Disetujui Pemilik Sistem pada 14 Oktober 2026).** Menggantikan pemindahan diam-diam dengan dialog interaktif transparan, peringatan dini sisa waktu, dan tombol perpanjang waktu.

---

### ISS-EPS-014-02 — Ketiadaan Persistensi State Alur Admisi

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya mengandalkan React in-memory state)** | Biarkan state `useState("")` seperti saat ini. | Kode sederhana. | Sangat rapuh terhadap tab discard / memory saver peramban; form kembali kosong dan tombol lanjut terkunci jika ditinggal lama. |
| **Opsi 2 (Persistensi Sesi Terisolasi via `sessionStorage` - Terpilih)** | Pada hook `useInpatientAdmissionFlow`, buat mekanisme sinkronisasi dua arah ke `sessionStorage` dengan kunci terisolasi per pasien: `quilvian_admission_draft_${patientId || "new"}`. Saat komponen di-mount, lakukan rehidrasi nilai `patientType`, `visitType`, dan data rujukan jika ada di storage. Bersihkan storage hanya setelah admisi berhasil dibuat atau tombol *"Batal Admisi"* dikonfirmasi. | Data isian aman meskipun tab mengalami hibernasi memori atau pengguna tidak sengaja menekan refresh; validasi form tetap terpenuhi sehingga tombol tidak mendadak terkunci. | Memerlukan utilitas helper serializer/deserializer aman di hook flow. |

**SOLUSI TERPILIH: Opsi 2.** Menerapkan persistensi berbasis sesi per pasien agar alur multi-langkah kebal terhadap siklus inaktivitas dan hibernasi tab.

---

### ISS-EPS-014-03 — Kerentanan Elemen Penutup Modal (Ghost Backdrop Trap)

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Biarkan perilaku default modal)** | Tidak mengubah pengelolaan backdrop modal Bootstrap. | Tanpa beban kerja. | Risiko overlay transparan tetap ada jika tab blur saat transisi dialog keluar/masuk. |
| **Opsi 2 (Sanitasi DOM dan Strict Cleanup Lifecycle - Terpilih)** | Tambahkan efek pembersihan defensif pada komponen `InpatientAdmissionView` saat dialog ditutup: pastikan kelas `modal-open` dihapus dari `document.body` dan sisa elemen `.modal-backdrop` di-remove secara tuntas dari DOM jika tidak ada modal yang berstatus `open`. | Memastikan container `#content-page` selalu bebas dari lapisan penutup tak terlihat (*ghost backdrop*); interaksi klik pengguna selalu sampai ke form admisi. | Memerlukan sedikit kode pembersihan defensif di `useEffect`. |

**SOLUSI TERPILIH: Opsi 2.** Menerapkan pembersihan DOM yang ketat dan defensif untuk modal konfirmasi admisi.

---

## 3. Skema tampilan sebelum → sesudah

### Alur Sebelum Perbaikan (Masalah Terjadi)
```text
PETUGAS MEMILIH TEMPAT TIDUR DI LANGKAH 7 (BOOKING BED)
  ┌──────────────────────────────────────────────────────────────┐
  │ Langkah 7: Pemesanan Tempat Tidur                           │
  │ Tempat Tidur: BED-001 (Ruang Mawar Kelas II)                 │
  │ [ Sisa Waktu Reservasi: 09:45 ... 00:00 ]                    │
  └──────────────────────────────────────────────────────────────┘
                               │
                Petugas meninggalkan layar (idle 10 menit)
                untuk memverifikasi berkas fisik pasien...
                               │
                               ▼
  [ SISTEM MENDETEKSI WAKTU HABIS (TIMEOUT) ]
  1. useInpatientAdmissionBed memanggil releaseExpiredReservation()
  2. clearSelection() -> selectedBedId menjadi null
  3. returnToSelectionToken bertambah -> memicu onBack() secara silent!
                               │
                               ▼
  KONDISI LAYAR SAAT PETUGAS KEMBALI KE MEJA (BINGUNG / PANIK):
  ┌──────────────────────────────────────────────────────────────┐
  │ Langkah 6: Pemilihan Tempat Tidur (Tiba-tiba mundur sendiri)│
  │ Denah Bed: [ BED-001 ] [ BED-002 ]                           │
  │ (Tidak ada bed yang terpilih karena baru saja di-clear)      │
  │                                                              │
  │ [ < Kembali ]                [ Lanjut ke Pemesanan (MATI) ]  │
  └──────────────────────────────────────────────────────────────┘
  HASIL:
  - Tombol "Lanjut ke Pemesanan" berwarna ABU-ABU REDUP (DISABLED)!
  - Tidak ada pesan kesalahan atau pemberitahuan apa pun.
  - Petugas mengira sistem rusak/freeze total ("tidak bisa diklik"),
    sementara menu sidebar tetap bisa diklik!
```

---

### Alur Sesudah Perbaikan (Solusi Opsi 2 Terpilih)
```text
PETUGAS MEMILIH TEMPAT TIDUR DI LANGKAH 7 (BOOKING BED)
  ┌──────────────────────────────────────────────────────────────┐
  │ Langkah 7: Pemesanan Tempat Tidur                           │
  │ Tempat Tidur: BED-001 (Ruang Mawar Kelas II)                 │
  │ [ Sisa Waktu Reservasi: 01:00 ] (Peringatan warna oranye)    │
  └──────────────────────────────────────────────────────────────┘
                               │
                Petugas meninggalkan layar (idle 10 menit)...
                               │
                               ▼
  [ SISTEM MENDETEKSI WAKTU HABIS (TIMEOUT) ]
  1. Reservasi sementara dibatalkan di server (bed tidak terkunci)
  2. TIDAK ADA PENGALIHAN SILENT!
  3. DIALOG PERINGATAN RESMI MUNCUL DI TENGAH LAYAR:
  ┌──────────────────────────────────────────────────────────────┐
  │ ⚠️  Waktu Reservasi Sementara Habis                          │
  │                                                              │
  │ Batas waktu penahanan tempat tidur sementara (10 menit)      │
  │ telah berakhir demi menjaga ketersediaan tempat tidur RS.    │
  │ Alokasi tempat tidur BED-001 telah dilepaskan kembali.       │
  │                                                              │
  │ Silakan periksa kembali ketersediaan dan pilih ulang         │
  │ tempat tidur untuk melanjutkan proses admisi pasien ini.     │
  │                                                              │
  │                   [ 🔁 Pilih Ulang Tempat Tidur ]            │
  └──────────────────────────────────────────────────────────────┘
                               │
                Petugas kembali & mengeklik tombol
                               │
                               ▼
  PENGALIHAN TERATUR KE LANGKAH PEMILIHAN BED:
  ┌──────────────────────────────────────────────────────────────┐
  │ [ℹ️ Informasi: Alokasi tempat tidur sebelumnya kedaluwarsa.  │
  │ Silakan klik salah satu tempat tidur di bawah untuk memilih] │
  │                                                              │
  │ Langkah 6: Pemilihan Tempat Tidur                            │
  │ [ BED-001 (Tersedia) ]  [ BED-002 (Tersedia) ]              │
  │                                                              │
  │ [ < Kembali ]                      [ Lanjut ke Pemesanan ]   │
  └──────────────────────────────────────────────────────────────┘
  HASIL:
  - Sangat jelas dan edukatif, petugas memahami alasan operasionalnya.
  - Tidak ada lagi kesan "sistem freeze atau tiba-tiba hilang".
```

---

## 4. Rincian perbaikan

### FIX-EPS-014-01 — Dialog Ramah Kedaluwarsa Reservasi Tempat Tidur, Peringatan Dini, dan Perpanjangan Waktu

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`
  - `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx`
- **Rincian Perubahan:**
  1. Pada `use-inpatient-admission-bed.jsx`:
     - Tambahkan state boolean `isExpiredDialogOpen` dan fungsi pembuka/penutupnya `dismissExpiredDialog()`.
     - Tambahkan fungsi `extendReservation()`: jika waktu tersisa <= 2 menit dan petugas masih membutuhkan waktu untuk verifikasi berkas keluarga, tombol *"Perpanjang Waktu (+5 Menit)"* dapat diklik untuk memperbarui `expiresAt` (menambahkan 5 menit) atau melakukan pembaruan reservasi ke server.
     - Deteksi peringatan dini: sediakan properti turunan `isWarningCountdown` (bernilai `true` saat `remainingMs <= 2 * 60 * 1000` dan `> 0`).
     - Saat hitung mundur mencapai `<= 0`:
       ```javascript
       if (getReservationRemainingMs(expiresAt, now) <= 0) {
         void releaseExpiredReservation();
         setIsExpiredDialogOpen(true); // Membuka dialog resmi
       }
       ```
     - Jangan langsung mereset `selectedBedId` menjadi kosong seketika saat timer habis jika dialog belum ditutup, atau simpan catatan nama bed yang tadi dilepaskan untuk ditampilkan pada isi teks dialog.
  2. Pada `inpatient-admission-bed-step.jsx`:
     - Hapus pemanggilan otomatis `onBack?.()` pada `useEffect` yang memantau token secara membabi buta tanpa konfirmasi pengguna.
     - Tambahkan indikator visual peringatan dini di samping jam hitung mundur: jika `isWarningCountdown` aktif, tampilkan teks sisa waktu berlatar oranye/merah dengan tombol interaktif:
       ```jsx
       {isWarningCountdown && (
         <BaseButton
           size="sm"
           variant="outline-warning"
           onClick={bed.extendReservation}
         >
           Perpanjang Waktu (+5 Menit)
         </BaseButton>
       )}
       ```
     - Tambahkan komponen modal konfirmasi ramah:
       ```jsx
       <ConfirmModal
         open={bed.isExpiredDialogOpen}
         title="Waktu Reservasi Tempat Tidur Habis"
         message="Batas waktu penahanan tempat tidur sementara telah berakhir demi menjaga ketersediaan tempat tidur rawat inap rumah sakit. Alokasi tempat tidur telah dilepaskan kembali. Silakan pilih kembali tempat tidur yang Anda inginkan."
         confirmText="Pilih Ulang Tempat Tidur"
         cancelText={null} // Dialog harus diakui pengguna
         onConfirm={() => {
           bed.dismissExpiredDialog();
           onBack?.(); // Berpindah ke pemilihan tempat tidur secara sadar
         }}
       />
       ```
     - Tambahkan banner informasi berwarna biru/kuning di atas tabel bed jika pengguna baru saja dialihkan karena kedaluwarsa.

---

### FIX-EPS-014-02 — Persistensi Sesi Alur Admisi di `sessionStorage`

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`
- **Rincian Perubahan:**
  1. Buat helper persistensi berbasis `sessionStorage`:
     ```javascript
     const getStorageKey = (patientId) => `quilvian_admission_draft_${patientId || "new"}`;
     ```
  2. Pada inisialisasi state `patientType`, `visitType`, dan `referralData`:
     - Baca dari `sessionStorage` terlebih dahulu jika tersedia sebelum menggunakan fallback string kosong default.
  3. Pada `useEffect` pemantau perubahan state:
     - Simpan data draft ke `sessionStorage` setiap kali `patientType`, `visitType`, atau `referralData` mengalami perubahan.
  4. Tambahkan fungsi `clearAdmissionDraft()` yang dipanggil saat:
     - Admisi berhasil disimpan (*final submit success*).
     - Pengguna secara sadar mengeklik tombol *"Batal Admisi"* atau *"Reset Formulir"*.

---

### FIX-EPS-014-03 — Sanitasi dan Cleanup Modal Backdrop Bootstrap

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- **Rincian Perubahan:**
  1. Pastikan setiap penutupan modal dialog (`resetConfirmOpen`, `cancellationModalOpen`, `exitGuard`) memicu pembersihan status backdrop:
     ```javascript
     useEffect(() => {
       return () => {
         // Cleanup saat komponen unmount atau dialog ditutup
         if (typeof document !== "undefined") {
           const backdrops = document.querySelectorAll(".modal-backdrop");
           backdrops.forEach((el) => el.remove());
           document.body.classList.remove("modal-open");
           document.body.style.removeProperty("padding-right");
           document.body.style.removeProperty("overflow");
         }
       };
     }, []);
     ```

---

## 5. Urutan pengerjaan & dependency

```text
[FIX-EPS-014-01: Dialog Kedaluwarsa Bed] (Prioritas 1)
          │
          ▼
[FIX-EPS-014-02: Persistensi Sesi Flow di sessionStorage] (Prioritas 2)
          │
          ▼
[FIX-EPS-014-03: Sanitasi Modal Backdrop Cleanup] (Prioritas 3)
```

Perbaikan dapat dikerjakan secara berurutan pada sisi Frontend tanpa memerlukan perubahan skema basis data backend (karena endpoint pembatalan dan perpanjangan reservasi tempat tidur telah tersedia di backend).

---

## 6. Strategi verifikasi & acceptance criteria (DoD)

### Kriteria Selesai (Definition of Done)
1. **Verifikasi Kasus 1 (Reservasi Habis Saat Idle):**
   - Petugas memilih tempat tidur hingga Langkah 7.
   - Layar dibiarkan idle selama batas waktu countdown timer (10 menit).
   - Layar TIDAK memindahkan langkah secara diam-diam dan tombol TIDAK mendadak terkunci tanpa penjelasan.
   - Muncul modal dialog jelas yang mengabarkan waktu habis dengan tombol *"Pilih Ulang Tempat Tidur"*.
   - Saat tombol diklik, form kembali ke Langkah 6 secara mulus dengan pesan panduan yang ramah.
2. **Verifikasi Kasus 2 (Tab Hibernasi / Reload):**
   - Petugas mengisi jenis pasien (misal Pasien Rujukan) di Langkah 2.
   - Lakukan refresh halaman atau tab didiamkan hingga memori ter-flush.
   - Nilai jenis pasien berhasil direhidrasi dari `sessionStorage`; tombol *"Lanjut"* tetap aktif dan dapat diklik.
3. **Verifikasi Kasus 3 (Interaksi Navigasi Bebas dari Ghost Backdrop):**
   - Setelah membuka dan menutup modal konfirmasi pembatalan atau reset, seluruh tombol pada `#content-page` tetap dapat menerima klik secara normal tanpa terhalang lapisan penutup transparan.
4. **Verifikasi Build:**
   - Menjalankan linting dan build frontend Next.js tanpa memunculkan galat sintaks.

---

## 7. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-14 | AI Engineering Assistant (Antigravity) | Penyusunan awal rencana perbaikan PLAN-REPAIR-EPS-014 untuk menyelesaikan issue layar admisi rawat inap ter-reset/mengunci pasca-idle. |
