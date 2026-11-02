# PLAN-REPAIR-030 — Rencana Perbaikan Denah Tempat Tidur Admisi Bayi, Selektivitas Kamar Neonatal Terpisah, dan Validasi Penempatan Kamar Ibu Tertutup

```yaml
plan_id: PLAN-REPAIR-EPS-030
issue: ../issue/issue-030-denah-bed-admisi-bayi-neonatal-rawat-gabung.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-21"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-21"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
```

Dokumen ini memuat rencana perbaikan untuk mengatasi masalah hilangnya tampilan denah tempat tidur pada alur pendaftaran admisi bayi baru lahir, menegakkan aturan bahwa hanya kamar bayi yang dapat dipilih pada mode neonatal terpisah sementara kamar lain tetap tampil di denah, serta memasang penjagaan agar kamar penempatan ibu yang sudah berstatus *closed* tidak dapat dipilih untuk rawat gabung.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- | :---: |
| `FIX-EPS-030-01` | `ISS-EPS-030-01` | Hapus pra-syarat episode draf buatan dan aktifkan pemuatan denah master data mandiri pada admisi bayi | FE | 1 | `FIX-EPS-030` | ✅ SELESAI | [`FIX-EPS-030.md`](../../task/report/frontend/FIX-EPS-030.md) |
| `FIX-EPS-030-02` | `ISS-EPS-030-02` | Terapkan adaptor selektivitas denah mode Neonatal Terpisah (seluruh kamar tampil, hanya kamar bayi yang dapat diklik) | FE | 1 | `FIX-EPS-030` | ✅ SELESAI | [`FIX-EPS-030.md`](../../task/report/frontend/FIX-EPS-030.md) |
| `FIX-EPS-030-03` | `ISS-EPS-030-03` | Pasang validasi guard status kamar ibu closed pada pemilihan mode Rawat Gabung di Langkah 3 & Langkah 4 | FE + BE | 1 | `FIX-EPS-030` | ✅ SELESAI | [`FIX-EPS-030.md`](../../task/report/frontend/FIX-EPS-030.md) |
| `FIX-EPS-030-04` | `ISS-EPS-030-T1` | Sinkronisasi kontrak pencarian bed dan denah layout tanpa ketergantungan episode | FE | 2 | `FIX-EPS-030` | ✅ SELESAI | [`FIX-EPS-030.md`](../../task/report/frontend/FIX-EPS-030.md) |

**Ringkasan: 4 dari 4 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-030-01 — Denah Tempat Tidur Admisi Bayi Terblokir Galat Draf Prematur

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Pola Denah Mandiri Tanpa Draf Prematur**: Nonaktifkan `prepareFloorPlanDraft` untuk alur bayi, muat denah visual langsung via master data (`purpose: ""`), dan buat draf episode hanya saat bed telah dipilih petugas. | Menghilangkan pesan galat merah secara tuntas, denah langsung tampil tanpa menunggu episode sementara, konsisten dengan pola transfer `FIX-EPS-024`. | Menuntut hook mengelola penundaan pembuatan draf (*deferred episode creation*) sampai bed terpilih. |
| **B** | Memaksa pembuatan draf episode kosong di Langkah 3 sebelum petugas masuk ke Langkah 4. | `useFloorPlan` dapat langsung memakai `purpose: "Admission"`. | Menyalahi aturan bisnis: episode draft tersimpan tanpa tempat tidur, mencemari database jika petugas membatalkan di Langkah 4. |
| **C** | Mengubah backend agar mengizinkan `GET /bed-occupancies/floor-plan` tanpa `EpisodeId`. | Mempertahankan `purpose: "Admission"` di frontend. | Mengubah kontrak backend bersama yang dipakai alur admisi umum dan monitoring rawat inap; radius risiko tinggi. |

**Solusi Terpilih: Opsi A.**
1. Pola ini terbukti sukses dan stabil pada alur admisi transfer (`FIX-EPS-024`), di mana denah fisik dapat dieksplorasi secara bebas tanpa menuntut keberadaan rekaman episode di database.
2. Mencegah terciptanya rekaman draf "zombie" di database bila petugas menutup peramban di tengah langkah pemilihan bed.

*Kenapa bukan opsi lain:* Opsi B mencemari database dengan transaksi kotor, sedangkan Opsi C memiliki radius dampak terlalu luas terhadap kontrak backend bersama.

---

### ISS-EPS-030-02 — Selektivitas Denah Khusus Kamar Bayi pada Mode Neonatal Terpisah

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Adaptor Peta Denah Selektif**: Tetap render seluruh kamar di lantai denah (`floorPlan.data.rooms`), evaluasi atribut `isForNewborn` dan `available-beds`, tandai `isSelectable = true` hanya untuk kamar/bed bayi. Kamar lain berstatus `isSelectable = false`. | Memenuhi 100% permintaan pengguna: *"yang lain tetap bisa tampil juga, hanya kamar bayi saja yang bisa diklik"*, konteks navigasi denah tetap utuh. | Memerlukan mapping adaptasi properti `isSelectable` pada memori frontend. |
| **B** | Saring kamar di query awal sehingga denah hanya memuat kamar bayi saja. | Beban data lebih kecil. | **Ditolak tegas**: Denah visual gedung menjadi bolong-bolong dan melanggar instruksi pengguna: *"yang lain tetap bisa tampil juga"*. |
| **C** | Biarkan semua kamar dapat diklik, lalu tolak di form pemesanan jika memilih kamar dewasa. | Tidak perlu adaptor denah. | Membingungkan petugas; petugas baru tahu tempat salah setelah mencoba memilih bed. |

**Solusi Terpilih: Opsi A.**
1. Mengikuti prinsip UI Quilvian: *Validasi visual mencegah kesalahan sebelum tindakan dilakukan*. Petugas langsung melihat outline hijau interaktif pada kamar perinatologi/NICU, sementara kamar pasien dewasa tetap terlihat di posisinya namun tidak dapat diklik.
2. Memberikan label status yang jelas pada kamar non-bayi: `(×) Bukan Kamar Bayi`.

---

### ISS-EPS-030-03 — Penjagaan Status Kamar Penempatan Ibu Closed pada Rawat Gabung

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Validasi Guard Bertingkat (Langkah 3 & Langkah 4)**: Periksa `currentRoomId` dan status penempatan ibu di Langkah 3. Jika kamar ibu tertutup (*closed*), tampilkan peringatan dan cegah opsi rawat gabung. Di Langkah 4, pastikan kamar ibu ditandai tidak dapat dipilih bila statusnya tidak aktif. | Integritas data terjamin dua lapis: petugas dicegah sejak awal pemilihan perawatan, dan denah tetap terlindungi bila data berubah di tengah jalan. | Memerlukan penyelarasan data penempatan aktif ibu di Langkah 3 dan Langkah 4. |
| **B** | Hanya periksa saat penekanan tombol Simpan / Aktivasi Admisi. | Perubahan kode minimal. | Petugas sudah lelah mengisi 7 langkah pendaftaran, baru ditolak di langkah akhir; pengalaman pengguna sangat buruk. |
| **C** | Hapus opsi Rawat Gabung jika ibu tidak memiliki kamar aktif. | Petugas tidak bisa salah klik. | Petugas tidak tahu *mengapa* opsi rawat gabung hilang; lebih informatif bila opsi tetap ada namun diberi pesan alasan kenapa terkunci. |

**Solusi Terpilih: Opsi A.**
1. Menghormati keputusan klinis rumah sakit: rawat gabung hanya sah secara medis dan operasional jika ibu masih dirawat aktif di dalam kamar rawat inap.
2. Menyajikan pesan penjelasan yang edukatif dan solutif kepada petugas admisi bila kamar ibu sudah ditutup.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Keadaan Sebelum Perbaikan (Sesuai Screenshot Lampiran 2)

```text
+---------------------------------------------------------------------------------------------------------+
| LANGKAH 4 DARI 7   Penempatan dan Pemesanan                                               57% Selesai   |
| [✓] Identitas Bayi  [✓] Perawatan dan DPJP Bayi  [•] Penempatan dan Pemesanan  [ ] Persetujuan Wali     |
+---------------------------------------------------------------------------------------------------------+
| ADMISI BAYI                                                                                             |
| Penempatan dan Pemesanan Bayi                                                                           |
| Tempat harus sesuai kebutuhan bayi. Unit layanan dan kelas mengikuti kamar bed yang dipilih.             |
|                                                                                                         |
| LANGKAH PILIH BED                                                                                       |
| Pilih Tempat Tidur                                                                                      |
| Pilih satu tempat tidur yang dapat dipakai pasien ini, lalu lanjut ke pemesanan.                        |
|                                                                                                         |
| (i) Kelayakan penempatan ditentukan server                                                         [x]  |
|     Daftar tempat tidur datang apa adanya dari server. Layar ini tidak menyaring ulang dengan          |
|     aturannya sendiri, sehingga yang terlihat di sini sama dengan yang akan diterima saat pemesanan.    |
|                                                                                                         |
| [X] Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif.                      [x]  |
|     Muat ulang atau hubungi pengelola master bed.                                                       |
|                                                                                                         |
| [ Coba Siapkan Denah Lagi ]                                                                             |
|                                                                                                         |
|                                                                                                         |
|                                                                                                         |
|                                                                             [ Kembali ] [ (x) Lanjut ]  |
+---------------------------------------------------------------------------------------------------------+
```

---

### Keadaan Sesudah Perbaikan (Kondisi 1: Neonatal Terpisah)

```text
+---------------------------------------------------------------------------------------------------------+
| LANGKAH 4 DARI 7   Penempatan dan Pemesanan                                               57% Selesai   |
| [✓] Identitas Bayi  [✓] Perawatan dan DPJP Bayi  [•] Penempatan dan Pemesanan  [ ] Persetujuan Wali     |
+---------------------------------------------------------------------------------------------------------+
| ADMISI BAYI                                                                                             |
| Penempatan dan Pemesanan Bayi                                                                           |
| Mode: Neonatal Terpisah (Membutuhkan Perawatan Intensif Sesuai Keputusan Klinis)                        |
|                                                                                                         |
| (i) Denah Pemilihan Bed Bayi                                                                            |
|     Seluruh denah ruangan lantai ditampilkan. Hanya kamar dan boks khusus bayi yang dapat diklik        |
|     dan dipilih sesuai kebutuhan klinis neonatal.                                                       |
|                                                                                                         |
| Pilih Lantai: [ Lantai 2 ]  (•) Lantai 3 (Perinatologi/NICU)  [ Lantai 4 ]  [ Lantai 5 ]               |
|                                                                                                         |
| +-----------------------------------------------------------------------------------------------------+ |
| | PETA DENAH LANTAI 3                                                                                 | |
| |                                                                                                     | |
| | +-----------------------+  +-----------------------+  +-------------------------------------------+ | |
| | | KAMAR MAWAR 301       |  | KAMAR MAWAR 302       |  | RUANG PERINATOLOGI & NICU (BAYI)          | | |
| | | Rawat Inap Dewasa     |  | Rawat Inap Dewasa     |  | Kamar Khusus Neonatal / Bayi              | | |
| | | (x) Bukan Kamar Bayi  |  | (x) Bukan Kamar Bayi  |  | [✓] DAPAT DIPILIH (3 Boks Tersedia)       | | |
| | | [ Tidak dapat diklik ]|  | [ Tidak dapat diklik ]|  | [>>> KLIK RUANGAN UNTUK PILIH BOKS <<<]   | | |
| | +-----------------------+  +-----------------------+  +-------------------------------------------+ | |
| |                                                                                                     | |
| | +-----------------------+  +-----------------------+  +-------------------------------------------+ | |
| | | RUANG ISOLASI DEWASA  |  | RUANG TINDAKAN MEDIS  |  | RUANG PERAWAT (NURSE STATION)             | | |
| | | (x) Bukan Kamar Bayi  |  | (x) Bukan Ruang Rawat |  | [ Ruang Operasional ]                     | | |
| | +-----------------------+  +-----------------------+  +-------------------------------------------+ | |
| +-----------------------------------------------------------------------------------------------------+ |
|                                                                                                         |
| Bed Terpilih: Ruang Perinatologi · Boks Neonatal B-02 (Kelas Perinatologi)                              |
| [✓] Sesuai Kebutuhan Intensif Neonatal                                                                  |
|                                                                                                         |
|                                                                     [ Kembali ] [ Lanjut ke Pemesanan ] |
+---------------------------------------------------------------------------------------------------------+
```

---

### Keadaan Sesudah Perbaikan (Kondisi 2: Rawat Gabung — Sub-kasus Kamar Ibu Closed)

```text
+---------------------------------------------------------------------------------------------------------+
| LANGKAH 4 DARI 7   Penempatan dan Pemesanan                                               57% Selesai   |
| [✓] Identitas Bayi  [✓] Perawatan dan DPJP Bayi  [•] Penempatan dan Pemesanan  [ ] Persetujuan Wali     |
+---------------------------------------------------------------------------------------------------------+
| ADMISI BAYI                                                                                             |
| Penempatan dan Pemesanan Bayi                                                                           |
| Mode: Rawat Gabung dengan Ibu (Ny. Rahmawati · Kasus Persalinan OPR-2026-088)                           |
|                                                                                                         |
| [!] Kamar Penempatan Ibu Sudah Closed / Tidak Aktif                                                [x]  |
|     Ibu tercatat telah selesai menempati kamar (Kamar Mawar 201 berstatus CLOSED / pasien pulang).     |
|     Bayi tidak dapat ditempatkan rawat gabung di kamar ini.                                             |
|                                                                                                         |
| Solusi Operasional:                                                                                     |
| 1. Jika bayi dirawat mandiri, kembali ke Langkah 3 dan ubah mode ke "Neonatal terpisah".                |
| 2. Jika ibu masih dirawat, periksa dan perbarui penempatan kamar ibu di Bed Management.                 |
|                                                                                                         |
| +-----------------------------------------------------------------------------------------------------+ |
| | PETA DENAH LANTAI 2                                                                                 | |
| |                                                                                                     | |
| | +--------------------------------------------+  +-------------------------------------------------+ | |
| | | KAMAR MAWAR 201 (KAMAR IBU)                |  | KAMAR MAWAR 202                                 | | |
| | | Status Penempatan Ibu: CLOSED / TIDAK AKTIF|  | Rawat Inap Dewasa                               | | |
| | | (x) TIDAK DAPAT DIPILIH                    |  | (x) Bukan Kamar Ibu                             | | |
| | +--------------------------------------------+  +-------------------------------------------------+ | |
| +-----------------------------------------------------------------------------------------------------+ |
|                                                                                                         |
| [ Ubah Mode ke Neonatal Terpisah ]                                  [ Kembali ] [ (x) Lanjut ]          |
+---------------------------------------------------------------------------------------------------------+
```

---

### Tabel Wilayah Layar Sesudah Perbaikan

| Wilayah | Isi dan Fungsi | Sumber Data | Komponen Pengisi |
| :--- | :--- | :--- | :--- |
| **Header Alur & Mode** | Judul langkah, indikator progres, dan badge mode perawatan bayi (Neonatal Terpisah atau Rawat Gabung). | State alur `flow` & draf `newborn.draft` | `inpatient-newborn-admission-steps.jsx` |
| **Banner Validasi / Guard** | Pesan penjelas kelayakan atau peringatan status kamar ibu closed. | Evaluasi guard frontend & ketersediaan penempatan ibu | `InformationAlert` (`variant="info"` / `"warning"` / `"danger"`) |
| **Navigasi Lantai** | Tab pemilih lantai denah (Lantai 2, 3, 4, dst.). | `floorPlan.data.floors` | Segmented tab `FloorPlanSelection` |
| **Peta Denah Visual** | Peta SVG tata letak seluruh kamar dan ruangan di lantai aktif. Menampilkan outline hijau interaktif khusus kamar bayi yang memenuhi syarat. | `effectiveFloorPlan.data` via `adaptedFloorPlanData` | `FloorPlanDrawing` |
| **Modal Rincian Bed Kamar** | Muncul saat kamar yang dapat dipilih diklik. Menampilkan daftar tempat tidur/boks bayi di kamar tersebut beserta statusnya. | `room.beds` | Modal pop-up `FloorPlanSelection` |
| **Panel Bed Terpilih & Aksi** | Menampilkan rincian boks bayi terpilih dan tombol navigasi alur. | State `selectedBed` hook bed | `BaseDetailCard`, `BaseButton` |

---

### Tabel Tombol Layar Sesudah Perbaikan

| Label Tombol | Jenis | Kapan Aktif | Yang Terjadi Saat Diklik |
| :--- | :--- | :--- | :--- |
| **Lanjut ke Pemesanan** | `primary` | Aktif hanya jika bed/boks bayi yang valid telah dipilih (`selectedBed !== null`). | Membuat draf episode bayi secara atomik untuk bed terpilih dan melangkah ke Langkah 5 (Persetujuan Wali). |
| **Kembali** | `secondary` | Selalu aktif (kecuali saat pemesanan sedang in-flight). | Kembali ke Langkah 3 (Perawatan dan DPJP Bayi). |
| **Ubah Mode ke Neonatal Terpisah** | `secondary` | Muncul saat kamar ibu berstatus *closed* pada mode Rawat Gabung. | Memperbarui draf `careMode` menjadi `Neonatal` dan memuat ulang denah ke kamar perinatologi. |
| **Klik Ruang Denah** | `action` | Aktif hanya pada kamar yang memiliki `isSelectable === true`. | Membuka modal daftar boks bayi untuk memilih tempat tidur. |

---

### Tabel Keadaan Layar (*States*)

| Keadaan | Ciri di Layar | Perilaku Komponen |
| :--- | :--- | :--- |
| **Memuat Denah (*Loading*)** | Skeleton / spinner pemuatan denah aktif. | Seluruh tombol aksi dinonaktifkan sementara. |
| **Denah Berhasil Dimuat (Neonatal)** | Seluruh kamar di lantai muncul di denah. Kamar bayi memiliki garis batas hijau interaktif; kamar lain abu-abu netral. | Petugas dapat mengklik kamar bayi untuk memilih boks. |
| **Denah Berhasil Dimuat (Rawat Gabung Aktif)** | Kamar ibu otomatis disorot dan ditandai dapat dipilih boks bayinya. | Petugas dapat memilih boks bayi di kamar ibu. |
| **Kamar Ibu Closed (Rawat Gabung)** | Muncul peringatan jelas: *"Kamar penempatan ibu sudah berstatus closed/tidak aktif"*. Kamar ibu tidak dapat diklik. | Tombol Lanjut dinonaktifkan; muncul tombol bantuan beralih ke mode Neonatal. |
| **Tidak Ada Bed Bayi Tersedia** | Kamar bayi muncul di denah, tetapi bertanda *"Penuh / Tidak Tersedia"*. | Muncul peringatan bahwa seluruh boks bayi di lantai ini penuh, silakan pilih lantai lain. |

---

## 4. Rincian Perbaikan

### FIX-EPS-030-01 — Hapus Pra-syarat Episode Draf Buatan dan Aktifkan Pemuatan Denah Master Data Mandiri pada Admisi Bayi

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-030-01` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source frontend (`use-inpatient-admission-bed.jsx`, `inpatient-admission-view.jsx`) |
| **Berkas yang Diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` |
| **Radius Dampak** | Khusus penanganan alur admisi bayi (`newbornContext`) pada Langkah 4 |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Di `use-inpatient-admission-bed.jsx`, kenali kondisi admisi bayi:
   ```javascript
   const isNewbornAdmission = Boolean(newbornContext && !episodeId);
   ```
2. Selaraskan konfigurasi pemanggilan hook `useFloorPlan`:
   ```javascript
   const floorPlan = useFloorPlan({
     enabled: enabled && usesAdmissionFloorPlan && (Boolean(episodeId) || isTransferAdmission || isNewbornAdmission),
     purpose: (isTransferAdmission || isNewbornAdmission) ? "" : "Admission",
     episodeId: (isTransferAdmission || isNewbornAdmission) ? "" : episodeId,
   });
   ```
3. Cegah eksekusi `prepareFloorPlanDraft` ketika berada dalam mode admisi bayi (`isNewbornAdmission`):
   ```javascript
   if (!usesAdmissionFloorPlan || episodeId || !active || !admission?.enabled || isTransferAdmission || isNewbornAdmission) return;
   ```
4. Pindahkan komitmen pembuatan episode draf (`admission.prepare(selectedBed)`) ke saat petugas menekan tombol **"Lanjut ke Pemesanan"** (`prepareBooking`), sama persis dengan alur admisi transfer.

**Acceptance Criteria:**
1. Saat petugas masuk ke Langkah 4 Admisi Bayi, pesan galat merah *"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif"* **tidak muncul lagi**.
2. Peta visual denah lantai rumah sakit langsung dimuat dan tampil utuh di layar dalam waktu < 2 detik.
3. Tombol *"Coba Siapkan Denah Lagi"* tidak ditampilkan karena denah berhasil dimuat secara mandiri.

**Verifikasi:**
- Jalankan pemeriksaan linter: `npm run lint:errors` di `QuilvianSystemFrontendDev`.
- Buka alur admisi bayi di peramban, navigasi dari Langkah 3 ke Langkah 4: denah lantai tampil bersih tanpa pesan galat.

**Risiko & Mitigasi:**
- *Risiko:* Episode draf belum tercipta saat masuk ke langkah booking.
- *Mitigasi:* Fungsi `prepareBooking` sudah memiliki mekanisme memanggil `runAdmissionPreparation(effectiveSelectedBed)` sesaat sebelum berpindah ke langkah berikutnya.

---

### FIX-EPS-030-02 — Terapkan Adaptor Selektivitas Denah Mode Neonatal Terpisah

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-030-02` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source frontend (`use-inpatient-admission-bed.jsx`) |
| **Berkas yang Diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` |
| **Radius Dampak** | Peta denah admisi bayi (`FE-INP-026`) |
| **Bergantung pada** | `FIX-EPS-030-01` |

**Langkah Pengerjaan:**
1. Perluas `adaptedFloorPlanData` di `use-inpatient-admission-bed.jsx` agar menangani `isNewbornAdmission`:
   ```javascript
   const isNewborn = Boolean(newbornContext);
   const isNeonatalSeparate = isNewborn && newbornContext?.newbornCareMode === "Neonatal";
   ```
2. Pada iterasi pemetaan kamar dan tempat tidur:
   - Jika `isNeonatalSeparate`:
     - Periksa setiap bed dalam kamar:
       `const isBabyBed = bed.isForNewborn === true;`
     - Jika membutuhkan perawatan intensif (`newbornContext?.requiresIntensiveCare`):
       `const meetsIntensive = bed.isIntensiveCareBed || room.isIntensiveCare;`
     - Bed ditandai `isSelectable = isBabyBed && (!newbornContext.requiresIntensiveCare || meetsIntensive) && bed.statusCode === "Available";`
     - Kamar ditandai `room.isSelectable = room.beds.some(b => b.isSelectable);`
   - Kamar yang tidak memiliki bed bayi yang cocok ditandai `room.isSelectable = false`, dan setiap bed di dalamnya diberi keterangan kegagalan kelayakan:
     `{ code: "NEWBORN_BED_REQUIRED", message: "Bukan tempat tidur / kamar khusus bayi." }`
3. Seluruh kamar tetap dimasukkan ke dalam daftar `rooms` dan `roomDataByKey` sehingga SVG layout menggambar seluruh gedung secara utuh tanpa ada kamar yang hilang.

**Acceptance Criteria:**
1. Pada mode Neonatal Terpisah, seluruh kamar di lantai (misal: Lantai 3 memuat 8 kamar dewasa dan 2 kamar perinatologi) **tetap tampak pada gambar denah**.
2. Garis batas hijau interaktif (*clickable indicator*) **hanya aktif pada kamar perinatologi / NICU**.
3. Kamar dewasa/umum berstatus non-interaktif dan tidak dapat dipilih.
4. Ketika kamar bayi diklik, modal terbuka dan petugas dapat memilih salah satu boks bayi yang tersedia.

**Verifikasi:**
- Uji klik pada kamar dewasa di denah: modal tidak terbuka atau menampilkan pesan larangan.
- Uji klik pada kamar bayi: modal terbuka dan bed bayi dapat dipilih.

**Risiko & Mitigasi:**
- *Risiko:* Ada kamar bayi yang belum diset `IsForNewborn = true` di master data.
- *Mitigasi:* Saring juga berdasarkan `patientClassType === "Perinatology"` atau nama kamar yang memuat kata "Bayi" / "Perinatologi" / "NICU" sebagai fallback toleran.

---

### FIX-EPS-030-03 — Pasang Validasi Guard Status Kamar Ibu Closed pada Mode Rawat Gabung

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-030-03` |
| **Area** | Frontend & Backend |
| **Jenis Perubahan** | Source frontend (`use-inpatient-newborn-admission.js`, `inpatient-newborn-admission-steps.jsx`, `use-inpatient-admission-bed.jsx`) |
| **Berkas yang Diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`, `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` |
| **Radius Dampak** | Alur pendaftaran admisi bayi mode rawat gabung |
| **Bergantung pada** | `FIX-EPS-030-01` |

**Langkah Pengerjaan:**
1. Di `use-inpatient-newborn-admission.js` pada fungsi `validateCare`:
   ```javascript
   if (draft.careMode === "RoomingIn") {
     if (!selectedCandidate?.motherEpisodeId) {
       errors.motherEpisodeId = "Rawat gabung memerlukan episode ibu yang masih aktif.";
     } else if (!selectedCandidate?.currentRoomId || selectedCandidate?.currentRoomStatus === "Closed") {
       errors.motherEpisodeId = "Kamar penempatan ibu sudah berstatus closed / tidak aktif. Rawat gabung tidak dapat dipilih.";
     }
   }
   ```
2. Di `use-inpatient-admission-bed.jsx` pada adaptasi denah rawat gabung (`newbornContext?.newbornCareMode === "RoomingIn"`):
   - Periksa apakah kamar penempatan ibu (`motherRoomId`) ada dan berstatus aktif.
   - Bila `motherRoomId` tidak ditemukan atau berstatus *closed*:
     - Tetapkan pesan peringatan pada denah: *"Kamar penempatan ibu sudah berstatus tertutup (closed/selesai dirawat). Bayi tidak dapat ditempatkan rawat gabung."*
     - Kunci pemilihan seluruh tempat tidur di kamar tersebut (`isSelectable = false`).
     - Tampilkan tombol bantuan langsung: `[ Ubah ke Mode Neonatal Terpisah ]` yang mengalihkan `careMode` menjadi `Neonatal`.

**Acceptance Criteria:**
1. Jika seorang ibu persalinan status kamarnya sudah *closed* (penempatan selesai/pulang):
   - Pada Langkah 3, memilih "Rawat gabung dengan ibu" menampilkan pesan galat penjelas dan mencegah lanjut ke penempatan.
   - Pada Langkah 4 (bila berhasil diakses), denah menandai kamar ibu dengan badge `(×) Kamar Ibu Closed` dan tidak dapat diklik.
2. Jika ibu masih menempati kamar secara aktif (status kamar aktif/open):
   - Kamar ibu otomatis disorot pada denah, boks bayi di kamar ibu berstatus `isSelectable = true` dan dapat dipilih.

**Verifikasi:**
- Uji skenario data kandidat ibu dengan `currentRoomId = null` atau placement selesai: sistem menolak rawat gabung dan mengarahkan ke neonatal terpisah.
- Uji skenario data ibu dengan placement aktif: sistem mengizinkan pemilihan boks di kamar ibu.

**Risiko & Mitigasi:**
- *Risiko:* Perubahan status kamar ibu terjadi saat petugas sedang mengisi Langkah 4.
- *Mitigasi:* Backend `InpNewbornAdmissionRules.cs:70-81` sudah memiliki penjagaan `FindMotherRoomAsync` yang mengecek status placement ibu secara real-time saat penyimpanan akhir.

---

### FIX-EPS-030-04 — Sinkronisasi Kontrak Pencarian Bed dan Denah Layout Tanpa Ketergantungan Episode

| Field | Nilai |
| :--- | :--- |
| **Menutup** | `ISS-EPS-030-T1` |
| **Area** | Frontend / API Adapter |
| **Jenis Perubahan** | Source frontend (`inpatient-bed-utils.jsx`) |
| **Berkas yang Diubah** | `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` |
| **Radius Dampak** | Query ketersediaan bed admisi bayi |
| **Bergantung pada** | `FIX-EPS-030-01` |

**Langkah Pengerjaan:**
1. Di `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` pada `buildBedBoardQuery`:
   - Pastikan ketika `patientContext?.isForNewborn` dikirim, jangan mengunci query papan ketersediaan hanya pada kamar bayi saja jika yang diminta adalah denah lantai penuh.
   - Gunakan parameter `includeIneligible = true` pada query `available-beds` agar server mengembalikan seluruh bed beserta daftar alasan ketidaklayakannya (*failures array*), sehingga frontend dapat menentukan warna dan status interaksi masing-masing kamar pada denah visual.

**Acceptance Criteria:**
1. Endpoint `GET /bed-occupancies/available-beds` mengembalikan daftar bed lengkap beserta objek `ineligibleBeds`.
2. Frontend dapat membedakan mana bed yang `isEligible = true` dan mana yang memiliki `failures: [{ code: "NEWBORN_BED_REQUIRED" }]`.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-030-01 (Denah mandiri tanpa draf prematur)
├── FIX-EPS-030-04 (Sinkronisasi parameter available-beds & ineligible)
└── FIX-EPS-030-02 (Adaptor selektivitas kamar bayi mode Neonatal Terpisah)
FIX-EPS-030-03 (Validasi guard status kamar ibu closed pada Rawat Gabung)
```

- `FIX-EPS-030-01` adalah fondasi kritis yang wajib diselesaikan pertama kali agar layar denah dapat terbuka tanpa pesan galat.
- `FIX-EPS-030-02` dan `FIX-EPS-030-04` dikerjakan setelah denah terbuka untuk mengatur agar hanya kamar bayi yang dapat diklik.
- `FIX-EPS-030-03` dapat dikerjakan secara paralel untuk memastikan keamanan klinis opsi rawat gabung.

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan yang Dibutuhkan | Pilihan yang Tersedia | Rekomendasi Terpilih | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Tindakan antarmuka bila kamar penempatan ibu terdeteksi closed saat memilih Rawat Gabung | **1**: Cegah di Langkah 3 dengan pesan galat.<br>**2**: Izinkan ke Langkah 4 lalu beri tombol otomatis alih ke Neonatal. | **Kombinasi 1 & 2**: Cegah di Langkah 3, dan jika terlanjur sampai di Langkah 4 sediakan tombol `[ Ubah ke Neonatal Terpisah ]`. | `FIX-EPS-030-03` |
| **K-02** | Gaya interaksi kamar dewasa/umum pada denah mode Neonatal Terpisah | **1**: Kamar abu-abu netral, kursor not-allowed, klik tidak membuka modal.<br>**2**: Kamar bisa diklik, modal terbuka tapi tombol pilih bed mati. | **Pilihan 1**: Lebih ringkas, ramah pengguna, dan mencegah beban klik berulang. | `FIX-EPS-030-02` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian / Paragraf | Kondisi Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `05-skema-tampilan.md` | Bagian Alur Admisi Bayi (Langkah 4 Penempatan) | Denah admisi diasumsikan memuat via `purpose: "Admission"` dengan draf episode aktif. | Denah penempatan bayi memuat denah fisik master data secara mandiri; selektivitas kamar bayi (Neonatal) dan kamar ibu (Rawat Gabung) dievaluasi di memori antarmuka. |
| `03-frontend-architecture.md` | Bagian Alur Penempatan Kamar Bayi Baru Lahir | Mengharuskan draf episode dibuat oleh hook sebelum denah dirender. | Draf episode dibuat secara atomik saat tombol "Lanjut ke Pemesanan" ditekan setelah bed dipilih petugas. |

---

## 8. Verifikasi Menyeluruh

Skenario pengujian ujung-ke-ujung (*end-to-end*) setelah seluruh perbaikan diterapkan:

1. **Skenario 1 — Mode Neonatal Terpisah (Normal Flow):**
   - Masuk ke pendaftaran bayi baru lahir, pilih ibu persalinan Ny. Siti.
   - Di Langkah 3, pilih *"Neonatal terpisah"*, isi sumber keputusan, centang perawatan intensif, pilih DPJP dr. Spesialis Anak. Klik Lanjut.
   - **Hasil:** Langkah 4 terbuka mulus tanpa galat merah. Peta denah lantai 3 tampil lengkap. Kamar rawat dewasa tampil abu-abu (tidak bisa diklik). Kamar Perinatologi/NICU bergaris hijau interaktif.
   - Klik Kamar Perinatologi, pilih Boks B-01, klik *"Lanjut ke Pemesanan"*. Draf episode tercipta dengan sukses dan alur melangkah ke Langkah 5.

2. **Skenario 2 — Mode Rawat Gabung (Kamar Ibu Masih Aktif):**
   - Di Langkah 3, pilih *"Rawat gabung dengan ibu"*. Ibu sedang dirawat aktif di Kamar Mawar 201. Klik Lanjut.
   - **Hasil:** Langkah 4 terbuka. Denah otomatis menyorot Kamar Mawar 201. Boks bayi di kamar ibu dapat dipilih.

3. **Skenario 3 — Mode Rawat Gabung (Kamar Ibu Sudah Closed / Pulang):**
   - Di Langkah 3, pilih *"Rawat gabung dengan ibu"* untuk ibu yang sudah di-discharge / penempatan kamarnya selesai.
   - **Hasil:** Sistem menampilkan validasi *"Kamar penempatan ibu sudah berstatus closed/tidak aktif. Bayi tidak dapat didaftarkan rawat gabung"*. Petugas diarahkan memilih opsi Neonatal Terpisah.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-21 | Rencana perbaikan dibuat dengan 4 butir perbaikan terstruktur, skema tampilan sebelum-sesudah ASCII, acceptance criteria, dan verifikasi menyeluruh. Menunggu persetujuan pemilik. | `diagnose-module-issue` |
| 2026-10-21 | Rencana perbaikan disetujui oleh Pemilik Sistem. Implementasi source frontend FIX-EPS-030 selesai dan seluruh unit test lulus (11/11). Status perbaikan ditandai SELESAI. | Tim Rekayasa Frontend |
