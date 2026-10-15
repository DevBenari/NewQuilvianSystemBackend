# PLAN-REPAIR-013 — Perbaikan Sinkronisasi Otomatis Papan Denah Bed Pasca Perpindahan Tempat Tidur (Transfer Bed)

```yaml
plan_id: PLAN-REPAIR-EPS-013
issue: ../issue/issue-013-sinkronisasi-papan-transfer-bed.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-13"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "a1569f4787d98476b73ba5f8694e1eb73bc03588 (MHamzah)"
basis_source_frontend: "9d890cf1c6b298e57cf79de8181012405972755e (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-013-01` | `ISS-EPS-013-01` | Hubungkan callback pasca-transfer ke `boardState.refresh()` dan `boardRefreshToken` pada `InpatientBedTransfer` agar denah tempat tidur otomatis terbarui | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-013-02` | `ISS-EPS-013-T1` | Satukan fungsi tombol "Muat Ulang Data" agar turut menyegarkan denah tempat tidur bersamaan dengan data konteks transfer | FE | 2 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 2 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-013-01 — Ketiadaan sinkronisasi pembaruan otomatis pada denah bed pasca-transfer

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Biarkan pengguna refresh manual)** | Tidak mengubah kode; pengguna diarahkan menekan refresh browser. | Tanpa beban perubahan kode. | Sangat membingungkan pengguna, melanggar standar responsivitas data RS, memicu risiko salah pilih bed dan galat 409 Conflict. |
| **Opsi 2 (Integrasi boardRefreshToken & coordinated callback - Terpilih)** | Di dalam `InpatientBedTransfer`, kelola state `boardRefreshToken`. Pada saat `state.submit()` memanggil `onTransferred`, jalankan penyegaran `boardState.refresh()` sekaligus naikkan nilai `boardRefreshToken`, lalu teruskan ke parent `onTransferred`. | Otomatis, seketika (*instant*), memanfaatkan kontrak hook bawaan `useInpatientBedBoard({ refreshToken })`, dan denah bed langsung ter-update tanpa reload halaman. | Memerlukan sedikit penyesuaian handler pada `inpatient-bed-transfer.jsx`. |

**SOLUSI TERPILIH: Opsi 2.** Mengintegrasikan koordinasi lifecycle antar-hook sehingga mutasi transfer langsung merefleksikan perubahan status pada denah bed.

---

### ISS-EPS-013-T1 — Tombol "Muat Ulang Data" tidak menyegarkan denah tempat tidur

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya reload formulir)** | Tombol tetap hanya memanggil `state.reload()`. | Tidak perlu perubahan. | Data kartu asal dan papan denah di bawahnya berpotensi tidak sinkron. |
| **Opsi 2 (Reload menyeluruh Asal + Denah - Terpilih)** | Ubah handler tombol "Muat Ulang Data" agar memanggil `Promise.allSettled([state.reload(), boardState.refresh()])`. | Konsisten dan terpadu; satu klik menyegarkan seluruh elemen pada tampilan transfer. | Tidak ada kekurangan. |

**SOLUSI TERPILIH: Opsi 2.** Tombol reload pada satu layar wajib menyegarkan seluruh data yang ditampilkan di layar tersebut.

---

## 3. Skema tampilan sebelum → sesudah

### Skema Sebelum Perbaikan
```text
[Langkah 1: Pasien Terpilih - Budi Pratama (No. Rawat: RI-2026-001)]
[Tempat Tidur Asal: Ruang Kelas II - BED 001 (Terisi)]
------------------------------------------------------------------------
[Langkah 2: Papan Ketersediaan Tempat Tidur (InpatientBedBoard)]
  Kamar Kelas II:
  [ BED 001 - Terisi ]  [ BED 002 - Tersedia ] <-- Petugas memilih BED 002
------------------------------------------------------------------------
[Langkah 3: Form Transfer] -> Klik [ Pindahkan Pasien ] -> Transaksi SUKSES!
                                       │
                                       ▼
KONDISI LAYAR SETELAH SUKSES (MASALAH):
- Kartu Asal ter-reload: BED 002
- TETAPI PAPAN BED DI BAWAHNYA TIDAK BERUBAH:
  [ BED 001 - Terisi ]  [ BED 002 - Tersedia ]  <-- MASIH DATA LAMA!
  (Petugas bingung, harus klik tombol refresh browser manual)
```

### Skema Sesudah Perbaikan
```text
[Langkah 1: Pasien Terpilih - Budi Pratama (No. Rawat: RI-2026-001)]
[Tempat Tidur Asal: Ruang Kelas II - BED 001]
------------------------------------------------------------------------
[Langkah 2: Papan Ketersediaan Tempat Tidur (InpatientBedBoard)]
  Kamar Kelas II:
  [ BED 001 - Terisi ]  [ BED 002 - Tersedia ] <-- Petugas memilih BED 002
------------------------------------------------------------------------
[Langkah 3: Form Transfer] -> Klik [ Pindahkan Pasien ] -> Transaksi SUKSES!
                                       │
                                       ▼
KONDISI LAYAR SETELAH SUKSES (OTOMATIS TERSINKRONISASI):
[✓] Notifikasi: Transfer berhasil dicatat.
- Kartu Asal otomatis berubah: Tempat Tidur Asal: BED 002
- PAPAN BED DI BAWAHNYA OTOMATIS BERUBAH:
  [ BED 001 - Menunggu Pembersihan 🧽 ]  [ BED 002 - Terisi 👤 ]
  (Denah langsung mencerminkan kondisi riil tanpa perlu refresh manual!)
```

---

## 4. Rincian perbaikan

### FIX-EPS-013-01 — Koordinasi refresh otomatis pada `InpatientBedTransfer`

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/components/features/health-services/inpatient-management/inpatient-bed-transfer.jsx`
- **Rincian Perubahan:**
  1. Tambahkan state `boardRefreshToken`:
     ```javascript
     const [boardRefreshToken, setBoardRefreshToken] = useState(0);
     ```
  2. Simpan referensi `boardState` menggunakan `useRef`:
     ```javascript
     const boardStateRef = useRef(null);
     ```
  3. Susun fungsi koordinasi `handleTransferred`:
     ```javascript
     const handleTransferred = useCallback(async () => {
       setBoardRefreshToken((count) => count + 1);
       try {
         await boardStateRef.current?.refresh?.();
       } catch {
         // Abaikan galat sekunder jika dibatalkan oleh token baru
       }
       await onTransferred?.();
     }, [onTransferred]);
     ```
  4. Sambungkan `handleTransferred` ke opsi `onTransferred` pada hook `useInpatientBedTransfer`:
     ```javascript
     const state = useInpatientBedTransfer({
       episodeId: fixedEpisodeId,
       disabled,
       onTransferred: handleTransferred,
     });
     ```
  5. Kirim `refreshToken: boardRefreshToken` ke hook `useInpatientBedBoard`:
     ```javascript
     const boardState = useInpatientBedBoard({
       episodeId: currentEpisodeId,
       refreshToken: boardRefreshToken,
       enabled: Boolean(currentEpisodeId && state.canTransfer),
     });
     boardStateRef.current = boardState;
     ```
- **Kriteria Keberterimaan (*Acceptance Criteria*):**
  - [ ] Begitu konfirmasi transfer sukses, `boardState.refresh()` otomatis dieksekusi.
  - [ ] Denah kartu tempat tidur (`InpatientBedBoard`) langsung menampilkan tempat tidur tujuan sebagai "Terisi" dan tempat tidur asal sebagai "Menunggu Pembersihan".
  - [ ] Tidak memerlukan tindakan refresh manual dari pengguna.
  - [ ] Pemanggilan callback induk `onTransferred?.()` tetap berjalan normal untuk menyegarkan komponen luar (`BedManagementView` / `EpisodeDetailView`).

---

### FIX-EPS-013-02 — Satukan fungsi tombol "Muat Ulang Data"

- **Area:** Frontend
- **Berkas yang dimodifikasi:**
  - `src/components/features/health-services/inpatient-management/inpatient-bed-transfer.jsx`
- **Rincian Perubahan:**
  1. Susun handler `handleReload`:
     ```javascript
     const handleReload = useCallback(async () => {
       setBoardRefreshToken((count) => count + 1);
       await Promise.allSettled([
         state.reload(),
         boardStateRef.current?.refresh?.(),
       ]);
     }, [state]);
     ```
  2. Ganti `onClick={() => state.reload()}` pada tombol "Muat Ulang Data" menjadi `onClick={handleReload}`.
- **Kriteria Keberterimaan (*Acceptance Criteria*):**
  - [ ] Tombol "Muat Ulang Data" pada formulir transfer secara bersamaan menyegarkan data konteks pasien dan visualisasi papan denah tempat tidur.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-013-01 (Koordinasi hook refresh pasca-transfer pada inpatient-bed-transfer.jsx)
      │
      ▼
FIX-EPS-013-02 (Sinkronisasi tombol manual "Muat Ulang Data")
```

---

## 6. Keputusan pemilik yang dibutuhkan

| ID | Topik | Pertanyaan untuk Pemilik | Rekomendasi Solusi |
| --- | --- | --- | --- |
| **K-01** | Sinkronisasi Otomatis Denah Bed Pasca-Transfer | Apakah Anda menyetujui bahwa denah/peta tempat tidur otomatis disegarkan seketika setelah tombol konfirmasi transfer berhasil, didukung oleh sinkronisasi tombol "Muat Ulang Data"? | **Setujui (Rekomendasi).** Menghilangkan keharusan refresh manual dan mencegah ketidaksesuaian data visual hunian bed. |

---

## 7. Dokumen hulu yang ikut direvisi

Tidak ada perubahan skema database atau kontrak API backend (`contracts/`). Masalah murni berada pada koordinasi siklus hidup hook di komponen presentasi frontend (`QuilvianSystemFrontendDev`).

---

## 8. Verifikasi menyeluruh

| No | Skenario Pengujian | Hasil yang Diharapkan |
| ---: | --- | --- |
| 1 | Buka Bed Management tab **Transfer**. Pilih pasien yang sedang menempati tempat tidur (misal BED 001). | Kartu Tempat Tidur Asal terisi data BED 001, dan papan denah tempat tidur menampilkan seluruh tempat tidur yang tersedia. |
| 2 | Klik salah satu tempat tidur tujuan yang tersedia (misal BED 002). Isi kategori dan alasan transfer minimal 10 karakter. | Form konfirmasi tujuan terisi dengan rincian BED 002 dan tombol "Periksa dan Konfirmasi Transfer" aktif. |
| 3 | Klik "Periksa dan Konfirmasi Transfer", lalu pada dialog konfirmasi klik "Pindahkan Pasien". | Server merespons sukses (HTTP 200). Notifikasi sukses muncul. |
| 4 | Amati tampilan layar tanpa menekan tombol refresh. | **Papan denah tempat tidur (InpatientBedBoard) otomatis terbarui:** BED 002 berubah menjadi "Terisi", BED 001 berubah menjadi "Menunggu Pembersihan", dan kartu tempat tidur asal pasien berubah ke tempat tidur yang baru. |
| 5 | Klik tombol "Muat Ulang Data" pada formulir. | Kedua sumber data (konteks pasien dan papan denah) termuat ulang secara sinkron tanpa galat. |

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-13 | AI Engineering Assistant (Antigravity) | Penyusunan awal dokumen rencana perbaikan PLAN-REPAIR-EPS-013 mencakup koordinasi refresh otomatis papan denah bed pasca transfer. |
