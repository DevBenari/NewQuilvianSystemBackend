# PLAN-REPAIR-012 — Perbaikan Visual State Loading dan Penutupan Otomatis Modal Detail Bed Pasca Konfirmasi Masuk

```yaml
plan_id: PLAN-REPAIR-EPS-012
issue: ../issue/issue-012-konfirmasi-masuk-bed-management.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "a1569f4787d98476b73ba5f8694e1eb73bc03588 (MHamzah)"
basis_source_frontend: "9d890cf1c6b298e57cf79de8181012405972755e (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-012-01` | `ISS-EPS-012-01`, `ISS-EPS-012-T2` | Tambahkan visualisasi status `loading` pada tombol aksi "Konfirmasi Masuk" dan "Batalkan Pesanan" di modal Detail Bed | FE | 2 | — | ✅ SELESAI | `bed-management-view.jsx:334,346` & `use-inpatient-bed-board-actions.jsx:157-158` |
| `FIX-EPS-012-02` | `ISS-EPS-012-02`, `ISS-EPS-012-T1`, `ISS-EPS-012-T2` | Integrasikan callback `onSuccess` pada `useInpatientBedBoardActions` untuk menutup modal induk Detail Bed secara otomatis | FE | 1 | — | ✅ SELESAI | `bed-management-view.jsx:36-39` & `use-inpatient-bed-board-actions.jsx:13,121` |

**Ringkasan: 2 dari 2 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-012-01 — Ketiadaan indikator visual pemuatan (*loading state*) pada tombol aksi modal Detail Bed

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya disable tombol)** | Mempertahankan implementasi saat ini di mana tombol hanya `disabled={busy}` tanpa animasi. | Tidak memerlukan penyesuaian kode. | Tidak memberikan umpan balik visual; pengguna mengira tombol macet. |
| **Opsi 2 (Spinner terpadu BaseButton - Terpilih)** | Mengirimkan prop `loading` dari state aksi (`actions.preparing`) ke `BaseButton` pada kedua tombol aksi ("Konfirmasi Masuk" dan "Batalkan Pesanan"). | Menggunakan kemampuan bawaan `BaseButton`, konsisten dengan design token Quilvian, memberi indikasi visual putaran spinner yang jelas. | Perlu sedikit membedakan tombol mana yang sedang aktif diproses jika ingin spinner hanya di tombol yang diklik. |

**SOLUSI TERPILIH: Opsi 2.** `BaseButton` sudah memiliki prop `loading` standar yang menampilkan spinner animasi dan menonaktifkan interaksi klik ganda secara aman.

---

### ISS-EPS-012-02 — Modal Detail Tempat Tidur tidak tertutup secara otomatis setelah transaksi sukses

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Biarkan pengguna menutup manual)** | Modal tetap terbuka, tetapi data di dalam modal diperbarui secara paksa (*force refresh* snapshot). | Pengguna tetap melihat detail bed yang sama setelah status berubah. | Tidak efisien; alur penempatan pasien di RS menuntut petugas segera kembali ke papan monitor utama untuk melayani pasien berikutnya. |
| **Opsi 2 (Callback penutupan otomatis onSuccess - Terpilih)** | Menambahkan kontrak callback `onSuccess` ke dalam `useInpatientBedBoardActions`. Komponen `BedManagementView` mengirim `onSuccess: () => setSelectedBed(null)`. Begitu request placement/cancel berhasil di backend, modal induk langsung ditutup otomatis. | Alur cepat, bersih, modal langsung tertutup bersamaan dengan toast sukses, mengeliminasi risiko konflik data usang (*stale state* / 409 Conflict). | Tidak ada kekurangan teknis; ini adalah alur standar modal transaksi. |

**SOLUSI TERPILIH: Opsi 2.** Menutup modal secara otomatis adalah konvensi baku UI/UX transaksi pelayanan rumah sakit ketika suatu konfirmasi penempatan telah berhasil disetujui server.

---

## 3. Skema tampilan sebelum → sesudah

### Skema Sebelum Perbaikan
```text
+-------------------------------------------------------------+
| Modal: Detail Tempat Tidur: BED 001                     [X] |
+-------------------------------------------------------------+
| Nama Bed: BED 001               Kode Bed: BD-RSMMC-00041    |
| Status: Dipesan                 Kamar: Ruang Rawat Inap...  |
|                                                             |
| [ Konfirmasi Masuk ] [ Batalkan Pesanan ]                   |  <-- Klik tombol: tombol disabled abu-abu tanpa spinner
|                                                             |
| +---------------------------------------------------------+ |
| | Dialog: Konfirmasi Masuk Pasien                         | |  <-- Muncul konfirmasi kedua
| | Pasien: Sdr. Budi - BED 001                             | |
| | [ Batal ] [ Konfirmasi Masuk (Loading...) ]             | |  <-- Klik Konfirmasi: request sukses!
| +---------------------------------------------------------+ |
|                                                             |
| (Dialog konfirmasi tertutup, Toast "Sukses" muncul)         |
| MASALAH: Modal "Detail Tempat Tidur" TETAP TERBUKA!         |
| Status masih tertulis "Dipesan", tombol masih aktif.        |
|                                                             |
|                                                  [ Tutup ]  |  <-- Petugas harus klik Tutup manual
+-------------------------------------------------------------+
```

### Skema Sesudah Perbaikan
```text
+-------------------------------------------------------------+
| Modal: Detail Tempat Tidur: BED 001                     [X] |
+-------------------------------------------------------------+
| Nama Bed: BED 001               Kode Bed: BD-RSMMC-00041    |
| Status: Dipesan                 Kamar: Ruang Rawat Inap...  |
|                                                             |
| [ 🔄 Menyiapkan... ] [ Batalkan Pesanan (Disabled) ]       |  <-- ADA SPINNER saat verifikasi data ke server
|                                                             |
| +---------------------------------------------------------+ |
| | Dialog: Konfirmasi Masuk Pasien                         | |
| | Pasien: Sdr. Budi - BED 001                             | |
| | [ Batal ] [ Konfirmasi Masuk ]                          | |  <-- Klik Konfirmasi
| +---------------------------------------------------------+ |
+-------------------------------------------------------------+
                           │
                           │ Transaksi Berhasil Diproses (HTTP 200/201)
                           ▼
+-------------------------------------------------------------+
| Papan Bed Management (Halaman Utama)                       |
|                                                             |
| [✓] Notifikasi: Pasien dikonfirmasi masuk. Papan diperbarui |
|                                                             |
| +-------------------+  +-------------------+               |
| | BED 001           |  | BED 002           |               |
| | Status: Terisi 👤 |  | Status: Tersedia  |  <-- Modal Detail Bed OTOMATIS TERTUTUP!
| | Budi Pratama      |  |                   |      Papan langsung menampilkan status terkini.
| | [Detail Bed]      |  | [Detail Bed]      |               |
| +-------------------+  +-------------------+               |
+-------------------------------------------------------------+
```

---

## 4. Rincian perbaikan

### FIX-EPS-012-01 — Tambahkan visualisasi status `loading` pada tombol aksi modal Detail Bed

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx`
  - `src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx`
- **Rincian Perubahan:**
  1. Pada hook `useInpatientBedBoardActions`, lacak tipe aksi yang sedang disiapkan (`preparingAction: "confirm" | "cancel" | null`).
  2. Ekspor `preparingConfirm: preparing && preparingAction === "confirm"` dan `preparingCancel: preparing && preparingAction === "cancel"`.
  3. Pada `bed-management-view.jsx`:
     - Tombol "Konfirmasi Masuk":
       ```jsx
       <BaseButton
         disabled={busy}
         loading={actions.preparingConfirm}
         onClick={() => actions.requestConfirmAdmission(selectedBed)}
       >
         Konfirmasi Masuk
       </BaseButton>
       ```
     - Tombol "Batalkan Pesanan":
       ```jsx
       <BaseButton
         variant="danger"
         disabled={busy}
         loading={actions.preparingCancel}
         onClick={() => actions.requestCancelReservation(selectedBed)}
       >
         Batalkan Pesanan
       </BaseButton>
       ```
- **Kriteria Keberterimaan (*Acceptance Criteria*):**
  - [ ] Saat tombol "Konfirmasi Masuk" diklik, tombol menampilkan animasi spinner pemuatan selama fungsi `prepare` berjalan membaca data ke backend.
  - [ ] Tombol "Batalkan Pesanan" dinonaktifkan secara aman selama proses penyiapan konfirmasi berlangsung.
  - [ ] Begitu pula sebaliknya, jika tombol "Batalkan Pesanan" diklik, tombol tersebut menampilkan animasi spinner pemuatan.

---

### FIX-EPS-012-02 — Integrasikan callback `onSuccess` untuk menutup modal induk Detail Bed secara otomatis

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx`
  - `src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx`
- **Rincian Perubahan:**
  1. Pada `useInpatientBedBoardActions({ refresh, onSuccess })`:
     - Tambahkan penerimaan argumen opsi `onSuccess`.
     - Pada fungsi `submit`: setelah `await operation.run(...)` berhasil dijalankan, panggil callback `onSuccess?.({ action: cancel ? "cancel" : "confirm", target })`.
  2. Pada `bed-management-view.jsx`:
     - Sambungkan parameter inisialisasi hook:
       ```javascript
       const actions = useInpatientBedBoardActions({
         refresh: state.refresh,
         onSuccess: () => setSelectedBed(null),
       });
       ```
- **Kriteria Keberterimaan (*Acceptance Criteria*):**
  - [ ] Ketika transaksi konfirmasi masuk berhasil di backend, dialog `ConfirmModal` tertutup DAN modal induk `Detail Tempat Tidur` tertutup secara otomatis tanpa memerlukan intervensi klik tombol Tutup dari petugas.
  - [ ] Notifikasi sukses (Toast hijau) tetap muncul dengan informasi yang jelas bahwa pasien telah dikonfirmasi masuk.
  - [ ] Tampilan papan bed utama langsung menampilkan status tempat tidur yang telah diperbarui menjadi "Terisi" (*Occupied*).
  - [ ] Hal yang sama berlaku ketika transaksi pembatalan pesanan berhasil: modal induk otomatis tertutup dan status tempat tidur kembali menjadi "Tersedia" (*Available*).

---

## 5. Urutan pengerjaan

```text
FIX-EPS-012-02 (Kontrak callback onSuccess & auto-close modal detail)
      │
      ▼
FIX-EPS-012-01 (Penyematan prop loading pada BaseButton di modal detail)
```

---

## 6. Keputusan pemilik yang dibutuhkan

| ID | Topik | Pertanyaan untuk Pemilik | Rekomendasi Solusi |
| --- | --- | --- | --- |
| **K-01** | Penutupan Otomatis Modal Detail Bed | Apakah disetujui bahwa modal Detail Tempat Tidur otomatis tertutup segera setelah penempatan pasien atau pembatalan pesanan berhasil diproses server? | **Setujui (Rekomendasi).** Menutup modal secara otomatis mempercepat alur kerja loket dan mencegah tampilan data usang (*stale state*). |

---

## 7. Dokumen hulu yang ikut direvisi

Tidak ada perubahan skema database atau kontrak API backend (`contracts/`). Kedua perbaikan berada di ranah presentasi dan orkestrasi hook frontend (`QuilvianSystemFrontendDev`).

---

## 8. Verifikasi menyeluruh

| No | Skenario Pengujian | Hasil yang Diharapkan |
| ---: | --- | --- |
| 1 | Buka rincian tempat tidur berstatus *Dipesan* (BED 001) di layar Bed Management. Klik tombol **Konfirmasi Masuk**. | Tombol menampilkan animasi pemuatan (*spinner*) dan tombol Batalkan Pesanan menjadi nonaktif sementara sistem memverifikasi data ke backend. |
| 2 | Dialog penegas "Konfirmasi Masuk Pasien" muncul. Klik tombol **Konfirmasi Masuk** pada dialog. | Spinner pemuatan aktif pada tombol dialog konfirmasi hingga request `POST /placements` selesai. |
| 3 | Server merespons sukses (HTTP 200/201). | Dialog konfirmasi penegas tertutup, modal induk "Detail Tempat Tidur: BED 001" **otomatis tertutup**, notifikasi toast sukses muncul di pojok kanan atas, dan kartu BED 001 pada grid monitoring berubah warna dan berlabel "Terisi". |
| 4 | Buka rincian tempat tidur lain berstatus *Dipesan*. Klik tombol **Batalkan Pesanan** dan masukkan alasan pembatalan. | Spinner aktif saat penyiapan dan saat submit, modal otomatis tertutup setelah pembatalan berhasil, dan kartu bed kembali berstatus "Tersedia". |

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-13 | AI Engineering Assistant (Antigravity) | Penyusunan awal dokumen rencana perbaikan PLAN-REPAIR-EPS-012 mencakup penambahan state loading dan mekanisme penutupan otomatis modal detail bed. |
| 2026-10-13 | Pemilik Sistem (User) & AI Assistant | Keputusan K-01 disetujui pemilik sistem. Implementasi `FIX-EPS-012-01` dan `FIX-EPS-012-02` selesai dan tervalidasi di repository frontend. |
