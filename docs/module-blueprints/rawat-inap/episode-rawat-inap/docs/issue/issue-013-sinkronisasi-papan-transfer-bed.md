# ISSUE-013 — Papan Denah Tempat Tidur Tidak Berubah Otomatis Pasca Sukses Perpindahan (Transfer Bed)

```yaml
issue_id: ISSUE-EPS-013
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Bed Management — Tab Perpindahan Tempat Tidur (FE-BM-02) & Detail Episode (FE-INP-04)"
sumber_laporan: "Laporan pemilik sistem 13-10-2026: ketika perpindahan tempat tidur berhasil, denah/peta tempat tidur tidak berubah otomatis dan harus diklik refresh manual"
tanggal_issue: "2026-10-13"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "a1569f4787d98476b73ba5f8694e1eb73bc03588 (MHamzah)"
source_sha_frontend: "9d890cf1c6b298e57cf79de8181012405972755e (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-013-sinkronisasi-papan-transfer-bed.md
```

## 1. Ringkasan

Pada antarmuka Bed Management tab **Perpindahan Tempat Tidur (Transfer Bed)** maupun pada section transfer di Detail Episode, petugas rawat inap dapat memindahkan pasien dari satu tempat tidur ke tempat tidur lain. Setelah petugas memilih tempat tidur tujuan, mengisi kategori serta alasan transfer, lalu mengonfirmasi dialog pop-up *"Pindahkan Pasien"*, transaksi pemindahan tempat tidur berhasil dicatat di server (*HTTP 200/201*). 

Namun, **denah / papan visual ketersediaan tempat tidur (InpatientBedBoard) di bagian bawah tidak berubah secara otomatis**. Tempat tidur tujuan yang baru saja ditempati pasien tetap tampak berstatus *"Tersedia"*, dan tempat tidur asal belum berstatus *"Menunggu Pembersihan"*. Petugas terpaksa harus melakukan muat ulang (*refresh*) halaman atau tombol reload secara manual agar perubahan tata letak dan status tempat tidur terlihat. Masalah ini diklasifikasikan sebagai **High** karena menyajikan data visual hunian usang (*stale visual map*) yang dapat menyesatkan petugas lain untuk memilih tempat tidur yang sebenarnya sudah terisi, sehingga memicu risiko penolakan transaksi (*HTTP 409 Conflict*).

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "ketika pada saat melakukan perpindahan tempat tidur atau transfer bed , ketika di klik pindahkan pasien berhasil tetapi map perpindahan tempat tidur tidak secara otomatis berubah harus di klik refresh dulu coba anda analisis terlebih dahulu" | — (Deskripsi alur operasional transfer bed) |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-013-01` | 1 | Ketiadaan sinkronisasi pembaruan otomatis pada hook `useInpatientBedBoard` pasca-eksekusi transfer tempat tidur | `BUG` | Frontend | High | SUDAH-VERIFIKASI di `inpatient-bed-transfer.jsx:49-65` dan `use-inpatient-bed-transfer.js:190-213` | `FIX-EPS-013-01` |
| `ISS-EPS-013-T1` | — | Tombol *"Muat Ulang Data"* pada formulir transfer hanya memuat ulang data konteks internal tanpa memperbarui papan denah bed | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI di `inpatient-bed-transfer.jsx:468-474` | `FIX-EPS-013-02` |

---

## 4. Rincian per temuan

### ISS-EPS-013-01 — Ketiadaan sinkronisasi pembaruan otomatis pada hook `useInpatientBedBoard` pasca-eksekusi transfer tempat tidur

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source `inpatient-bed-transfer.jsx:49-65` & `use-inpatient-bed-transfer.js:190-213` |
| **Perbaikan** | `FIX-EPS-013-01` |

**Apa yang terjadi.** Petugas rawat inap melakukan pemindahan tempat tidur pasien (misal dari BED 001 Ruang Kelas II ke BED 002 Ruang Kelas II). Setelah menekan tombol "Pindahkan Pasien" pada dialog konfirmasi, muncul pemberitahuan bahwa transfer berhasil dicatat. Namun, tampilan papan kartu tempat tidur (*InpatientBedBoard*) yang ada di Langkah 2 tidak mengalami perubahan status secara langsung: BED 002 masih berlabel "Tersedia" dan BED 001 masih berlabel "Terisi". Petugas harus menekan tombol refresh browser agar denah tempat tidur menampilkan kondisi terkini.

**Kenapa terjadi.** Terjadi keterputusan komunikasi data antara dua hook independen di dalam komponen `InpatientBedTransfer`:
1. Komponen `InpatientBedTransfer` (`inpatient-bed-transfer.jsx`) menggunakan hook `useInpatientBedTransfer` untuk mengelola proses transfer dan hook `useInpatientBedBoard` untuk menampilkan papan denah tempat tidur:
   ```javascript
   // inpatient-bed-transfer.jsx:49-65
   const state = useInpatientBedTransfer({
     episodeId: fixedEpisodeId,
     disabled,
     onTransferred, // Meneruskan callback props dari parent
   });

   const boardState = useInpatientBedBoard({
     episodeId: currentEpisodeId,
     enabled: Boolean(currentEpisodeId && state.canTransfer),
   });
   ```
2. Saat pemindahan dieksekusi dengan sukses pada `use-inpatient-bed-transfer.js:190-212`:
   ```javascript
   await operation.run(body, (config) =>
     bedOccupancyService.post("placements/transfer", body, config),
   );
   ...
   await reload({ target: "" }); // Hanya me-reload transfer-context dan available-beds lokal hook ini
   await onTransferred?.();      // Memanggil state.refresh milik parent BedManagementView
   ```
3. Perhatikan bahwa `reload()` di dalam `useInpatientBedTransfer` **hanya** memperbarui state `context` dan `beds` internal hook tersebut (yang dipakai untuk kartu Tempat Tidur Asal).
4. Di sisi lain, papan visual tempat tidur dikendalikan sepenuhnya oleh `boardState` dari hook `useInpatientBedBoard`. Hook ini memiliki state internal sendiri (`board`, `serviceUnits`, `availableBeds`) yang bersumber dari pemanggilan endpoint `GET /bed-occupancies/bed-board` dan `GET /bed-occupancies/available-beds`.
5. Karena `episodeId` pasien tidak berubah dan `InpatientBedTransfer` **tidak pernah memanggil `boardState.refresh()`** atau memicu `refreshToken`, hook `useInpatientBedBoard` tidak pernah membaca ulang data ke server.
6. Akibatnya, visualisasi denah tempat tidur tetap menampilkan data lama (*stale board cache*).

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG`. Sesuai blueprint arsitektur frontend (`03-frontend-architecture.md` bagian 5.2), papan tempat tidur berisiko tinggi terhadap konkurensi hunian; setiap transaksi penempatan atau perpindahan wajib segera merefleksikan status terbaru pada visualisasi denah tanpa menuntut refresh manual dari pengguna.

**Dampak nyata.**
1. Petugas loket atau perawat mengira perpindahan tempat tidur belum terjadi karena tempat tidur tujuan masih berlabel "Tersedia".
2. Petugas lain yang melihat layar dapat salah mengira tempat tidur tersebut masih kosong dan mencoba menempatkan pasien lain, yang akan berujung pada galat penolakan server (*HTTP 409 Conflict*).

**Rekomendasi.**
Pada `inpatient-bed-transfer.jsx`, bungkus callback `onTransferred` dengan handler lokal yang:
1. Menambah nilai `boardRefreshToken` (atau langsung memanggil `boardState.refresh()`).
2. Meneruskan pemanggilan `onTransferred?.()` ke parent (`BedManagementView` / `EpisodeDetailView`).
3. Mengirimkan `refreshToken={boardRefreshToken}` ke hook `useInpatientBedBoard`.

---

### ISS-EPS-013-T1 — Tombol "Muat Ulang Data" pada formulir transfer tidak memuat ulang papan denah bed

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source `inpatient-bed-transfer.jsx:468-474` |
| **Perbaikan** | `FIX-EPS-013-02` |

**Apa yang terjadi.** Di samping tombol konfirmasi transfer, terdapat tombol sekunder berlabel *"Muat Ulang Data"* (`FaRedo`). Jika petugas mengeklik tombol ini, sistem hanya memanggil `state.reload()`, sedangkan papan denah tempat tidur di bawahnya (`InpatientBedBoard`) tidak ikut dimuat ulang.

**Kenapa terjadi.** `onClick={() => state.reload()}` hanya memanggil fungsi reload internal `useInpatientBedTransfer`, tanpa menyertakan pemanggilan `boardState.refresh()`.

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG` inkonsistensi UX. Tombol muat ulang pada satu kesatuan panel transfer semestinya menyegarkan seluruh data yang tampil di panel tersebut (baik data asal maupun papan tujuan).

**Dampak nyata.** Petugas yang mencoba menekan tombol "Muat Ulang Data" tetap tidak melihat pembaruan denah tempat tidur.

**Rekomendasi.** Perbarui fungsi tombol "Muat Ulang Data" agar memicu `state.reload()` sekaligus memicu pembaruan `boardState.refresh()`.

---

## 5. Tanya-jawab pelapor

| Pertanyaan pelapor | Jawaban berbasis analisis teknis |
| --- | --- |
| *"Ketika diklik pindahkan pasien berhasil, tetapi map perpindahan tempat tidur tidak secara otomatis berubah, harus diklik refresh dulu"* | **Penyebabnya adalah hook pengendali papan denah (`useInpatientBedBoard`) tidak menerima sinyal pembaruan saat transaksi transfer selesai.** Komponen perpindahan tempat tidur (`InpatientBedTransfer`) merender denah menggunakan hook `useInpatientBedBoard`, namun saat fungsi `submit()` selesai dijalankan, sistem hanya menyegarkan data internal formulir dan tidak pernah memanggil fungsi `boardState.refresh()`. Akibatnya komponen denah tempat tidur tetap memegang data sebelum transfer sampai halaman di-refresh penuh oleh pengguna. |

---

## 6. Temuan tambahan

1. Mekanisme `refreshToken` sudah tersedia di dalam kontrak hook `useInpatientBedBoard({ refreshToken, ... })`, namun komponen `InpatientBedTransfer` belum memanfaatkannya.
2. Tombol manual *"Muat Ulang Data"* pada formulir transfer juga tidak menyentuh hook denah tempat tidur, sehingga petugas yang mencoba mengklik tombol reload tersebut tetap mendapati denah yang tidak berubah.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk Pelapor / Pengguna
- Tidak ada pertanyaan tambahan; alur dan penyebab masalah telah teridentifikasi secara presisi pada source code.

### Untuk Pemilik Sistem
- **K-01**: Konfirmasi bahwa penyegaran otomatis denah tempat tidur (`InpatientBedBoard`) pasca-transfer disetujui untuk diintegrasikan secara langsung pada callback keberhasilan transfer, didukung oleh sinkronisasi tombol "Muat Ulang Data". *(Rekomendasi: **Setuju**)*.

---

## 8. Catatan pola

Pola masalah ini merupakan **Unsynchronized Sibling Hooks dalam Satu Feature View**:
- Ketika dua hook (`useInpatientBedTransfer` dan `useInpatientBedBoard`) dipanggil berdampingan dalam satu komponen untuk menampilkan data yang saling bergantung (aksi mutasi di satu sisi, dan visualisasi papan di sisi lain), penyelesaian mutasi pada hook pertama wajib memicu siklus refresh pada hook kedua.
- **Pencegahan ke depan:** Komponen wrapper wajib menyatukan *refresh lifecycle* antar-hook melalui state `refreshToken` terpadu atau *coordinated callback handler*.

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-13 | AI Engineering Assistant (Antigravity) | Penyusunan awal dokumen analisis issue ISSUE-EPS-013 terkait ketiadaan sinkronisasi visual denah tempat tidur pasca transfer bed. |
