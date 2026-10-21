# PLAN-REPAIR-023 — Penyediaan Formulir Aktivasi dan Pemetaan Kamar Langsung pada Modal Denah Master Bed

```yaml
plan_id: PLAN-REPAIR-EPS-023
issue: ../issue/issue-023-aktivasi-kamar-belum-dipetakan-di-modal-denah.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-17"
basis_source_backend: "c2bc563e (MHamzah)"
basis_source_frontend: "15ae73984 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Solusi | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| `FIX-EPS-023-01` | `ISS-EPS-023-01` | Sediakan tombol dan formulir aktivasi kamar baru langsung di dalam pop-up modal `FloorPlanMasterRoomPopup` | FE | 1 | FE-RWI-254 | ✅ SELESAI | `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-master-room-popup.jsx` |
| `FIX-EPS-023-02` | `ISS-EPS-023-02` | Sediakan opsi penautan ke kamar database existing langsung dari modal untuk mencegah duplikasi bed | FE + BE | 1 | FE-RWI-255 | ✅ SELESAI | `QuilvianSystemFrontendDev/src/lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js` |

---

## 2. Solusi Terpilih per Temuan

### A. Untuk `ISS-EPS-023-01` (Aktivasi Kamar Baru Langsung di Modal)
* **Solusi Terpilih:**  
  Saat kamar berstatus `isUnmapped` diklik, modal menampilkan tombol aksi utama: **`+ Aktifkan Kamar Ini`**.  
  Ketika tombol diklik, modal membuka formulir aktivasi (menggunakan pola `ActionForm` `actionType="activate"` yang sudah ada di sistem), dengan keunggulan:
  - Nomor kamar otomatis terisi dari identitas geometri denah (misal: `301`), dapat diedit jika perlu.
  - Kelas pasien otomatis terpilih sesuai kelas saran denah (`expectedPatientClassName`).
  - Unit rawat inap dapat dipilih dari daftar Nurse Station aktif.
  - Arah wing otomatis terisi dari arah denah (misal: `Barat Daya`).
  - Kapasitas bed default otomatis terisi sesuai kapasitas bawaan denah (misal 1 bed, 0 boks).
  - Alasan operasional wajib diisi sebagai audit trail.
  - Menekan **`Periksa Perubahan`** akan menampilkan dialog konfirmasi mutasi standar Quilvian dan langsung menyimpan kamar ke database dengan mengunci kunci denah (`floorPlanRoomKey`).

### B. Untuk `ISS-EPS-023-02` (Penautan Kamar Existing)
* **Solusi Terpilih:**  
  Pada modal, sediakan opsi alternatif: **`Tautkan ke Kamar Existing`**.
  - Menampilkan dropdown daftar kamar di database yang belum memiliki tautan denah (`roomsWithoutFloorPlanKey`).
  - Cocok untuk kasus di mana data kamar dan bed sudah dibuat sebelumnya (seperti Ruang ICU Lantai 2), sehingga pengguna tidak terkena penolakan duplikasi nama bed.

---

## 3. Skema Tampilan: Sebelum → Sesudah

### Tampilan Sebelum (Buntu / Dead-End)
```text
┌──────────────────────────────────────────────────────────────┐
│                      EDITOR MASTER BED                    [X]│
│                       Belum dipetakan                        │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ [!] Posisi ini belum terhubung ke kamar master.        │  │
│  │     Gunakan Impor susunan denah untuk memetakannya.   │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  Buka Master Kamar ↗                                         │
└──────────────────────────────────────────────────────────────┘
```

### Tampilan Sesudah (Interaktif & Lengkap di Satu Modal)
```text
┌──────────────────────────────────────────────────────────────┐
│                      EDITOR MASTER BED                    [X]│
│                     Kamar 301 (Lantai 3)                     │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ [i] Posisi fisik ini belum terhubung ke database.      │  │
│  │     Pilih salah satu cara aktivasi di bawah ini:       │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  [ + Aktifkan Kamar Baru ]      [ Tautkan Kamar Existing ]   │
│                                                              │
│  - ATAU -                                                    │
│                                                              │
│  Bila memilih "+ Aktifkan Kamar Baru", formulir muncul:      │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Nomor Kamar         : [ 301                          ] │  │
│  │ Kelas Pasien        : [ VIP · CLS-004              ▼ ] │  │
│  │ Unit Rawat Inap     : [ Rawat Inap Lt. 3           ▼ ] │  │
│  │ Arah Sayap (Wing)   : [ Barat Daya                 ▼ ] │  │
│  │ Kapasitas Bed/Boks  : Bed: [ 1 ]   Boks Bayi: [ 0 ]    │  │
│  │ Alasan Operasional  : [ Aktivasi kamar 301 dari denah] │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  [ Batal ]                                [ Periksa Perubahan ]
└──────────────────────────────────────────────────────────────┘
```

---

## 4. Rincian Perbaikan

### `FIX-EPS-023-01` — Penambahan Aksi Aktivasi pada `FloorPlanMasterRoomPopup.jsx`
1. **Frontend:**
   - Pada `FloorPlanMasterRoomPopup.jsx`, hapus blok peringatan buntu pada `room.isUnmapped`.
   - Tambahkan tombol aksi:
     - `BaseButton variant="primary"`: **`+ Aktifkan Kamar Baru`**.
     - `BaseButton variant="secondary"`: **`Tautkan Kamar Existing`**.
   - Ketika tombol aktivasi diklik, panggil `startAction("activate")` dengan nilai default form yang dipasok dari properti kamar denah (`room.roomNumber`, `room.expectedPatientClassId`, `room.wingDirection`, `room.bedCount`).
2. **Koneksi Mutasi:**
   - Gunakan `state.prepareAction(form)` yang sudah terhubung dengan aman ke `requestBedMasterMutation`.

### `FIX-EPS-023-02` — Dukungan Penautan Kamar Existing
1. **Frontend & Hook:**
   - Tambahkan sub-aksi `attachExisting` pada modal.
   - Mengambil data pilihan dari `state.data?.roomsWithoutFloorPlanKey`.
   - Mengirimkan mutasi `Room:Update` untuk mengisi field `FloorPlanRoomKey` pada kamar yang dipilih.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-023-01 (Tombol & Form Aktivasi Baru di Modal)
       │
       ▼
FIX-EPS-023-02 (Opsi Tautkan Kamar Existing)
       │
       ▼
Verifikasi Ujung-ke-Ujung (Klik kamar di denah → Aktif tanpa keluar layar)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

1. **Konfirmasi Desain Tampilan:** Apakah skema tampilan di atas (dua tombol: *Aktifkan Kamar Baru* dan *Tautkan Kamar Existing*) sudah sesuai dengan ekspektasi operasional rumah sakit?
2. **Halaman Impor Massal:** Seperti yang telah disetujui, tombol dan halaman **`Impor Susunan Denah`** tetap dipertahankan untuk kebutuhan setup borongan.

---

## 7. Dokumen Hulu yang Ikut Direvisi

- Dokumen blueprint arsitektur frontend `03-frontend-architecture.md` dan `05-skema-tampilan.md` pada submodul `episode-rawat-inap` akan diperbarui untuk mencatat bahwa modal kamar denah kini mandiri dan tidak lagi buntu.

---

## 8. Verifikasi Menyeluruh

| Langkah | Aksi Pengguna | Hasil yang Diharapkan |
| :---: | :--- | :--- |
| 1 | Buka Editor Denah Master Bed (`/health-services/master-data/bed`). | Denah tampil normal dengan kamar terpetakan dan belum dipetakan. |
| 2 | Klik salah satu kamar bergaris putus-putus (misal `301`). | Pop-up modal muncul, menampilkan nomor kamar 301 beserta opsi aktivasi. |
| 3 | Klik tombol `+ Aktifkan Kamar Baru`. | Formulir aktivasi terbuka dengan nomor kamar `301` dan saran kelas langsung terisi. |
| 4 | Lengkapi unit dan alasan, lalu klik `Periksa Perubahan` → Konfirmasi. | Pop-up sukses; kamar 301 di denah seketika berubah menjadi garis solid dengan warna kelasnya. |
| 5 | Periksa menu Monitoring Bed & Admisi Pasien. | Kamar 301 langsung muncul dan siap digunakan. |

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| :--- | :--- | :--- |
| 2026-10-17 | Antigravity AI | Penerbitan rencana perbaikan awal PLAN-REPAIR-EPS-023 untuk aktivasi kamar langsung di modal denah. |
