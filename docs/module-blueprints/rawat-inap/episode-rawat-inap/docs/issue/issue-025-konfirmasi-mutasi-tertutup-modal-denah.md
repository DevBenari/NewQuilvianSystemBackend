# ISSUE-025 — Dialog Konfirmasi Mutasi Master Tertutup oleh Modal Denah dan Ketiadaan Indikator Kolom Wajib

```yaml
issue_id: ISSUE-EPS-025
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Editor Denah Master Bed (FE-RWI-248 / FE-RWI-250 / FloorPlanMasterRoomPopup / BedMasterMutationConfirmation)"
sumber_laporan: "Laporan pengguna 17-10-2026: 2 butir, 2 lampiran (media_1792216445369_7cc75270.png dan media_1792216585352_8b422047.png)"
tanggal_issue: "2026-10-17"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "c2bc563e (MHamzah)"
source_sha_frontend: "15ae73984 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-025-konfirmasi-mutasi-tertutup-modal-denah.md
```

---

## 1. Ringkasan

Laporan pengujian pengguna pada layar Editor Denah Master Bed (`/health-services/master-data/bed`) menemukan dua masalah berurutan saat mencoba mengaktifkan kamar belum dipetakan (VIP 304 dan VIP 303). Pada laporan pertama, pengguna menekan tombol "Periksa Perubahan" namun tidak ada respons karena kolom unit dan alasan operasional masih kosong tanpa adanya penanda visual kolom wajib (*required indicator*). Pada laporan kedua, setelah seluruh kolom termasuk alasan operasional telah diisi lengkap, penekanan tombol "Periksa Perubahan" tetap tidak menghasilkan perubahan tampilan apa pun di layar dan tab Network DevTools tetap mencatat 0 *request*. Penelusuran kode membuktikan bahwa dialog konfirmasi mutasi (`BedMasterMutationConfirmation` yang membungkus `ConfirmModal`) sebenarnya berhasil dipicu (`mutation.show = true`), tetapi dialog tersebut muncul pada lapisan `z-index: 1200`, sedangkan modal denah (`FloorPlanMasterRoomPopup`) memiliki lapisan `z-index: 2000`. Akibatnya, dialog konfirmasi terkubur secara tak terlihat di balik modal denah sehingga alur aktivasi kamar terblokir total (*Blocker*).

---

## 2. Laporan Asli

| No. | Kalimat Pelapor | Lampiran |
| ---: | :--- | :--- |
| 1 | "ini kenapa saya klik simpan tapi masih gak bisa ?" | `media_1792216445369_7cc75270.png` (Modal "Editor Master Bed — VIP 304", kolom Unit rawat inap masih "Pilih unit" dan Alasan perubahan operasional masih kosong) |
| 2 | "sama aja tidak ada aksi ini kenapa ya" | `media_1792216585352_8b422047.png` (Modal "Editor Master Bed — VIP 303", seluruh isian sudah terisi lengkap termasuk Alasan "bismillah", tombol "Periksa Perubahan" diklik namun tidak tampak aksi apa pun di layar, tab Network DevTools mencatat 0 request) |

---

## 3. Ringkasan Temuan

| ID | No. Lap | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :---: | :---: | :---: | :---: |
| `ISS-EPS-025-01` | 2 | Dialog konfirmasi mutasi master tertutup di balik modal pop-up denah akibat konflik `z-index` | `BUG` | Frontend | `Blocker` | `SUDAH-VERIFIKASI` | `FIX-EPS-025-01` |
| `ISS-EPS-025-02` | 1 | Ketiadaan penanda visual kolom wajib dan validasi proaktif pada formulir aktivasi denah | `RULE_VIOLATION` | Frontend | `Medium` | `SUDAH-VERIFIKASI` | `FIX-EPS-025-02` |

---

## 4. Rincian per Temuan

### ISS-EPS-025-01 — Dialog konfirmasi mutasi master tertutup di balik modal pop-up denah akibat konflik `z-index`

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | `Blocker` |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend; capture pelapor `media_1792216585352_8b422047.png` |
| **Perbaikan** | `FIX-EPS-025-01` |

**Apa yang terjadi.**  
Petugas rumah sakit yang sedang mengaktifkan kamar baru (VIP 303) di denah telah melengkapi seluruh isian, termasuk nomor kamar, kelas pasien, unit rawat inap, arah wing, kapasitas tempat tidur, dan alasan operasional. Namun ketika menekan tombol **"Periksa Perubahan"**, layar tampak diam tidak bereaksi sama sekali, tidak ada pesan berhasil atau gagal, dan tidak ada jendela konfirmasi lanjutan yang terlihat.

**Kenapa terjadi.**  
Rantai sebab dari kode sumber:
1. Tombol `Periksa Perubahan` memicu `onSubmit` form yang memanggil `state.prepareAction(form)` (`QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-master-room-popup.jsx:91-94`).
2. Fungsi `prepareAction` memvalidasi alasan operasional lalu mengeksekusi `await mutation.prepare(payload, reason)` (`QuilvianSystemFrontendDev/src/lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js:477-506`).
3. Pada mutasi aktivasi kamar baru, resource adalah `"room"` dengan `id: ""` sehingga `prepareBedMasterMutation` langsung mengembalikan payload dan menyalakan state `setShow(true)` (`QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-bed-master-confirmation.js:30`).
4. State `mutation.show = true` menyebabkan komponen `BedMasterMutationConfirmation` merender `ConfirmModal` (`QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/bed-master-mutation-confirmation.jsx:7-20`).
5. Namun di dalam CSS global modal konfirmasi, z-index ditentukan sebesar:
   ```css
   /* QuilvianSystemFrontendDev/src/style/components/features/base-features/modal-confirm.css:19-26 */
   .modal-backdrop {
     z-index: 1150 !important;
   }
   .modal,
   .region-modal.modal {
     z-index: 1200 !important;
   }
   ```
6. Sementara itu, modal pop-up denah `FloorPlanMasterRoomPopup` memiliki z-index sebesar **2000**:
   ```css
   /* QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css:1437-1447 */
   .dialogBackdrop {
     position: fixed;
     inset: 0;
     z-index: 2000;
     display: grid;
     place-items: center;
     background: rgba(15, 23, 42, 0.42);
   }
   ```
7. Karena modal denah tetap di-render di layer `z-index: 2000` dan tidak disembunyikan saat `mutation.show === true`, `ConfirmModal` (`z-index: 1200`) ter-render tepat **di belakang** modal denah. Pengguna tidak dapat melihat dialog konfirmasi, dan klik pada tombol "Periksa Perubahan" berikutnya tertahan karena state mutasi sedang aktif. Permintaan HTTP mutasi baru akan dikirim setelah pengguna menekan "Konfirmasi Simpan" pada dialog tersebut, sehingga di tab Network DevTools terlihat 0 request.

**Apakah ini menyimpang dari desain?**  
Ya, ini adalah `BUG` murni pada susunan *stacking context* antarmuka. Desain bertingkat Quilvian mengharuskan dialog konfirmasi mutasi master kritis selalu tampil di lapisan paling depan di atas form pengisi data.

**Dampak nyata.**  
Petugas administrasi kamar rawat inap tidak bisa menyelesaikan aktivasi kamar fisik mana pun di denah. Petugas mengira sistem rusak atau tombol "mati" karena tidak ada respon visual.

**Rekomendasi.**  
1. Pada `FloorPlanMasterRoomPopup.jsx`, sembunyikan tampilan pop-up denah saat `state.mutation.show` bernilai `true` (atau sembunyikan backdrop pop-up), sehingga dialog `ConfirmModal` tampil langsung di hadapan pengguna. Jika pengguna membatalkan konfirmasi, pop-up denah otomatis muncul kembali dengan data form tetap terjaga.
2. Sediakan kelas prioritas tinggi untuk konfirmasi mutasi denah dengan `z-index` di atas 2000 (misalnya `3000`) agar dialog konfirmasi tidak pernah tertutup oleh modal apa pun.

---

### ISS-EPS-025-02 — Ketiadaan penanda visual kolom wajib dan validasi proaktif pada formulir aktivasi denah

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `RULE_VIOLATION` / `UX_IMPROVEMENT` |
| **Area** | Frontend |
| **Keparahan** | `Medium` |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend; capture pelapor `media_1792216445369_7cc75270.png` |
| **Perbaikan** | `FIX-EPS-025-02` |

**Apa yang terjadi.**  
Pada formulir aktivasi kamar baru di pop-up denah, pengguna tidak menyadari bahwa kolom "Unit rawat inap" dan "Alasan perubahan operasional" wajib diisi. Tidak ada tanda bintang merah (`*`) pada label kolom. Ketika pengguna langsung menekan tombol "Periksa Perubahan", peramban menolak submit formulir secara diam-diam tanpa umpan balik UI yang jelas, membuat pengguna bertanya-tanya mengapa data tidak bisa disimpan.

**Kenapa terjadi.**  
Komponen pembungkus kolom `Field` di `floor-plan-master-room-popup.jsx:29-37` dirancang sangat sederhana:
```jsx
function Field({ label, children, hint }) {
  return (
    <label className={styles.masterField}>
      <span>{label}</span>
      {children}
      {hint ? <small>{hint}</small> : null}
    </label>
  );
}
```
Komponen ini tidak menerima parameter `required` dan tidak menampilkan penanda visual wajib (seperti `<span className="text-danger">*</span>`). Meskipun elemen input/select/textarea memiliki atribut native `required`, peramban Chromium sering kali tidak menampilkan bubble validasi secara mencolok jika elemen select berada dalam modal custom atau tertutup scroll.

**Apakah ini menyimpang dari desain?**  
Ya, melanggar prinsip dasar Quilvian UI/UX: *"Jangan mematikan atau menolak tombol aksi tanpa memberi tahu pengguna isian mana yang kurang, dan tandai setiap isian wajib secara visual."*

**Dampak nyata.**  
Pengguna baru atau staf medis bingung dan frustrasi karena mengira tombol simpan tidak berfungsi, padahal hanya ada satu dropdown yang belum dipilih.

**Rekomendasi.**  
1. Tambahkan prop `required` pada komponen `Field` yang menampilkan tanda bintang merah `*`.
2. Berikan pesan validasi proaktif atau sorotan warna pada input yang masih kosong jika pengguna menekan tombol saat kolom belum lengkap.

---

## 5. Tanya-Jawab Pelapor

| Pertanyaan Pelapor | Jawaban Berbasis Bukti Sistem |
| :--- | :--- |
| *"ini kenapa saya klik simpan tapi masih gak bisa ?"* (Laporan 1) | Form tertahan oleh validasi peramban karena ada 2 kolom wajib yang belum diisi: **Unit rawat inap** masih bernilai default *"Pilih unit"* dan **Alasan perubahan operasional** masih kosong. Ketiadaan tanda bintang merah membuat kolom ini tidak terlihat sebagai kolom wajib. |
| *"sama aja tidak ada aksi ini kenapa ya"* (Laporan 2) | Seluruh kolom sebenarnya sudah valid, dan sistem sudah memicu dialog **"Konfirmasi Perubahan Master"**. Namun dialog konfirmasi tersebut memiliki lapisan `z-index: 1200`, sedangkan modal denah di depannya memiliki `z-index: 2000`. Akibatnya dialog konfirmasi tertutup rapat di belakang modal sehingga pengguna tidak bisa melihat atau menekannya. |

---

## 6. Temuan Tambahan

1. **State Blocking pada useBedMasterConfirmation:** Ketika `mutation.show = true` aktif di belakang layar, penekanan ulang tombol "Periksa Perubahan" di modal denah akan diblokir oleh pengecekan `if (!action || mutation.loading) return;` dan `busy.current`, sehingga tidak ada log maupun pesan galat baru yang muncul di console.
2. **AutoFocus pada Dialog Konfirmasi:** Saat dialog konfirmasi berhasil ditampilkan di depan, tombol aksi utama ("Konfirmasi Simpan") sebaiknya menerima fokus keyboard secara otomatis agar pengguna dapat langsung menekan `Enter` untuk konfirmasi cepat.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

| No. | Pihak | Pertanyaan / Pilihan | Rekomendasi |
| :---: | :--- | :--- | :--- |
| K-01 | Pemilik Sistem | Saat dialog konfirmasi mutasi muncul, apakah pop-up modal denah di belakangnya sebaiknya disembunyikan sementara atau tetap tampak remang-remang di latar belakang? | **Sembunyikan sementara (atau buat dialog konfirmasi berada di `z-index: 3000` di atas backdrop denah)** agar fokus petugas tertuju penuh pada verifikasi kamar/bed sebelum disimpan permanen. |

---

## 8. Catatan Pola

Masalah penumpukan modal (*stacked modal z-index collision*) kerap terjadi saat komponen berbasis React Portal kustom (seperti denah SVG interaktif) berinteraksi dengan komponen modal global berbasis React-Bootstrap (`ConfirmModal`). Standar antarmuka Quilvian harus menetapkan hierarki z-index yang konsisten:
- Layer Denah Dasar: `z-index: 10 - 50`
- Tooltip & Badge: `z-index: 100 - 200`
- Modal Editor Primer (Denah / Form): `z-index: 1050 - 2000`
- Modal Konfirmasi Kritis / Alert Gate: `z-index: 3000` (wajib selalu di atas modal editor)

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| :--- | :--- | :--- |
| 2026-10-17 | Antigravity (`diagnose-module-issue`) | Dokumen issue dibuat berdasarkan 2 laporan dan tangkapan layar pengguna mengenai kegagalan simpan aktivasi kamar denah; status TERBUKA (menunggu persetujuan). |
