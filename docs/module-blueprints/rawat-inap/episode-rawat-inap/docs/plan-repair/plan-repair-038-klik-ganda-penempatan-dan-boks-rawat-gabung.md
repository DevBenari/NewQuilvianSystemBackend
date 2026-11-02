# PLAN-REPAIR-038 — Langkah 4 admisi bayi sekali klik, boks rawat gabung lintas lantai, dan pemesanan tanpa perintah muat ulang

```yaml
plan_id: PLAN-REPAIR-EPS-038
issue: ../issue/issue-038-klik-ganda-penempatan-dan-boks-rawat-gabung.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-23"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-23"
basis_source_backend: "d79e3ed13d5ec73af63a3d22cd386aa702477dc1 (MHamzah)"
basis_source_frontend: "2b16ec441982e8917be151b6c58bc5673cf67d11 (HamzahV2)"
```

Seluruh perbaikan berada di **frontend**. Backend tidak perlu diubah: endpoint dan aturan
kelayakannya sudah benar, dan data yang dibutuhkan layar sudah tersedia.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-038-01` | `ISS-EPS-038-01`, `-T2`, `-T4` | Episode yang dibuat layar sendiri tidak memicu gerbang pelanjutan; episode bayi langsung dicatat di URL | FE | 1 | `FIX-EPS-038` | 🟡 SEBAGIAN — 6/6 AC ada di source; 0/6 terbukti uji layar (S1, S2, S7, S9 belum dijalankan) | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-02` | `ISS-EPS-038-01` | Sub-tampilan Pilih Bed ↔ Pesan dipegang pengendali bed, ditentukan fungsi murni bertes | FE | 1 | `FIX-EPS-038` | 🟡 SEBAGIAN — 4/5 AC terbukti tes unit dan render; AC5 menunggu uji layar | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-03` | `ISS-EPS-038-02`, `-T3` | Boks rawat gabung dibaca dari papan kamar ibu lintas lantai, hanya bed bayi, tiga keadaan dibedakan | FE | 1 | `FIX-EPS-038` | 🟡 SEBAGIAN — 5/6 AC terbukti tes unit dan render; AC6 (tanpa `floor-plan` di Network) menunggu uji layar | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-04` | `ISS-EPS-038-03` | Pemesanan boks dua tahap dengan pengendali terbaru; pemesanan yang sudah tercatat server diadopsi, bukan dilaporkan gagal | FE | 1 | `FIX-EPS-038` | 🟡 SEBAGIAN — 4/4 AC ada di source; 0/4 terbukti uji layar (S3, S6) | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-05` | `ISS-EPS-038-T1` | Draf sesi bayi tidak memakai kunci `active`; kunci lama dibersihkan saat berpindah | FE | 2 | `FIX-EPS-038` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti uji layar (S8) | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-06` | `ISS-EPS-038-T5` | Kandidat ibu terpilih dicari ulang bila tidak ada di halaman pertama | FE | 3 | `FIX-EPS-038` | 🟡 SEBAGIAN — 1/1 AC ada di source; belum terbukti uji layar | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |
| `FIX-EPS-038-07` | `ISS-EPS-038-T6` | Hapus isi pengganti yang tampak seperti data pada konfirmasi rawat gabung | FE | 3 | `FIX-EPS-038` | ✅ SELESAI | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) — render R5 |
| `FIX-EPS-038-08` | `ISS-EPS-038-T6` | Teks kartu "Rawat Gabung" tidak menyebut kamar/lantai tetap | FE + dokumen | 3 | `FIX-EPS-038` | 🟡 SEBAGIAN — teks kartu selesai (K-03, tes unit); revisi teks di `plan-repair-031` belum (bagian 7) | [`FIX-EPS-038.md`](../../task/report/frontend/FIX-EPS-038.md) |

**Ringkasan: 1 dari 8 perbaikan selesai; 7 sebagian — source, lint, unit test (22/22), dan build
selesai 23-10-2026, uji layar S1–S10 belum dijalankan.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-038-01 — Klik ganda "Lanjut ke Pemesanan"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Gerbang pelanjutan tidak menyembunyikan langkah bila episode di URL adalah episode milik layar ini, **ditambah** sub-tampilan dipindah ke pengendali bed | Menutup akar di dua lapis; juga mencegah jalur lain (transfer) ikut berkedip | Menyentuh view yang dipakai lima jalur admisi |
| B | Hanya memindahkan `inBookingView` ke pengendali bed | Perubahan kecil | Langkah tetap berkedip kosong; adopsi pelanjutan tetap berjalan di tengah langkah |
| C | Tidak menulis `episodeId` ke URL | Paling kecil | Muat ulang kehilangan episode → episode ganda (T2); bertentangan dengan `FE-RWI-032` |

**Solusi terpilih: Opsi A** (`FIX-EPS-038-01` + `FIX-EPS-038-02`).

1. Gerbang `resume.isResuming && !resume.isReady` dirancang untuk episode yang dibuka dari daftar
   kerja (`FE-RWI-032`), bukan episode yang baru saja dibuat layar ini — membedakannya memulihkan
   maksud desain aslinya.
2. Sub-tahap langkah yang dipegang pengendali level view tidak bergantung pada komponen langkah
   tetap terpasang, sehingga regresi serupa `FIX-EPS-033` tidak terulang.

**Kenapa bukan opsi lain.** B meninggalkan layar kosong sesaat dan efek adopsi yang tidak perlu.
C mengorbankan ketahanan muat ulang dan membuka jalan episode ganda.

### ISS-EPS-038-02 dan T3 — Boks rawat gabung "belum tersedia"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Baca boks dari papan `bed-board`/`available-beds` kamar ibu yang **sudah diambil** (`roomId` + `isForNewborn`) | Lintas lantai; kelayakan dinilai server; tanpa endpoint baru | Papan lama harus tetap hidup setelah episode dibuat |
| B | Muat denah pada lantai kamar ibu | Tetap memakai denah | Kandidat ibu tidak memuat kode lantai → perlu perubahan backend |
| C | Endpoint backend baru "boks rawat gabung untuk episode" | Paling eksplisit | Pekerjaan backend dan kontrak baru untuk data yang sudah ada |

**Solusi terpilih: Opsi A.**

1. Data sudah diambil dengan filter yang tepat (`inpatient-bed-utils.jsx:366-382`), hanya belum dibaca.
2. Server tetap pemutus kelayakan (`InpNewbornAdmissionRules.ValidateBed`), sesuai prinsip
   "kelayakan penempatan ditentukan server" yang tertulis di langkah Pilih Bed.
3. Tidak ada perubahan backend atau kontrak.

**Kenapa bukan opsi lain.** B butuh field baru di backend dan tetap memuat satu lantai penuh. C
membuat jalur kedua untuk pertanyaan yang sudah terjawab endpoint yang ada.

### ISS-EPS-038-03 — "Sesi berubah. Muat ulang"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Dua tahap: tombol membuat episode, lalu pemesanan dijalankan oleh pengendali render terbaru; ditambah adopsi pemesanan dari papan setelah galat tidak pasti | Tidak ada salinan pengendali lama; tahan jaringan putus | Alur tombol lebih panjang |
| B | Membuat scope `useBedOperation` stabil per pasien | Satu baris | Pemuat denah lama tetap membatalkan permintaan baru; `use-bed-operation.js` dipakai modul Bed Management lain |
| C | Menghapus pemeriksaan `current.current !== intent` | Pesan hilang | Melemahkan pengaman sesi yang sengaja dipasang untuk pergantian pengguna |

**Solusi terpilih: Opsi A.**

1. Menghilangkan akar (pemanggilan sesudah `await` yang mengubah identitas pengendali), bukan
   menutupi gejalanya.
2. Tidak menyentuh `use-bed-operation.js` milik Bed Management, sehingga radius dampak tetap di
   admisi.
3. Adopsi pemesanan dari papan sudah menjadi pola yang ada (`recoveredReservation`,
   `use-inpatient-admission-bed.jsx:453-467`).

**Kenapa bukan opsi lain.** B tidak menutup pembatalan denah dan menyentuh hook bersama. C
melemahkan pengaman.

### ISS-EPS-038-T1 — Draf sesi `newborn:active`

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Langkah 1–2 bayi tidak disimpan ke `sessionStorage`; kunci `newborn:active` dihapus saat halaman dibuka; kunci lama dihapus saat kunci berpindah | Tidak ada kebocoran antar admisi | Muat ulang di langkah 1 mengosongkan pilihan ibu (cukup pilih ulang) |
| B | Hapus semua draf saat komponen dilepas | Sederhana | Juga menghapus draf yang sah saat petugas pindah tab dalam alur |

**Solusi terpilih: Opsi A**, dibatasi pada mode `newborn` agar jalur Pasien Baru/Lama tidak
berubah.

### ISS-EPS-038-T5 dan T6

- T5: cari ulang kandidat memakai No. RM ibu yang sudah dimuat — tidak perlu endpoint baru.
- T6: isi pengganti diganti "Belum terbaca" dan tombol pesan dikunci dengan alasan
  (`FIX-EPS-038-07`); teks kartu menunggu K-03 (`FIX-EPS-038-08`).

---

## 3. Skema tampilan sebelum → sesudah

### 3.1 Perinatologi/NICU — klik "Lanjut ke Pemesanan"

**SEBELUM** (sesuai L-1):

```text
┌ LANGKAH PILIH BED ─────────────────────────────────────────────┐
│ ✓ Bed terpilih: NICU · bed NICU-1                               │
│ [ Denah lantai 2 — NICU-1 tersorot ]                            │
│ [ Batal Pendaftaran ]           [ Kembali ] [ Lanjut ke Pemesanan ] │
└────────────────────────────────────────────────────────────────┘
        │ klik 1 → episode dibuat, layar kosong sesaat
        ▼
┌ LANGKAH PILIH BED  (sama persis, URL sudah ada episodeId) ──────┐
│ ✓ Bed terpilih: NICU · bed NICU-1                               │
│ [ Batal Pendaftaran ]           [ Kembali ] [ Lanjut ke Pemesanan ] │
└────────────────────────────────────────────────────────────────┘
        │ klik 2
        ▼
      Kartu "Pesan Tempat Tidur"
```

**SESUDAH:**

```text
┌ LANGKAH PILIH BED ─────────────────────────────────────────────┐
│ ✓ Bed terpilih: NICU · bed NICU-1                               │
│ [ Denah lantai 2 — NICU-1 tersorot ]                            │
│ [ Batal Pendaftaran ]           [ Kembali ] [ Lanjut ke Pemesanan ] │
└────────────────────────────────────────────────────────────────┘
        │ klik 1 → tombol "Menyimpan admisi..." → episode dibuat
        ▼
┌ LANGKAH BOOKING BED — Pesan Tempat Tidur ───────────────────────┐
│ Tempat Tidur Dipilih: NICU · bed NICU-1 · Kelas NICU            │
│ ⓘ Pemesanan menahan tempat tidur selama 15 menit.               │
│ [ Batalkan Admisi ]                  [ Kembali ] [ Pesan Tempat Tidur ] │
└────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Pilih Bed | Denah satu lantai, bed terpilih | `useFloorPlan` (`purpose=Admission` setelah episode ada) | `FloorPlanSelection` existing |
| Booking | Kartu bed terpilih, info masa tahan | Pengendali bed | `InpatientAdmissionBedBookingStep` existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Pemesanan | utama | Bed terpilih, tidak sedang memproses | Membuat episode bila belum ada → `episodeId` ditulis ke URL → sub-tampilan Booking terbuka **tanpa** layar kosong |
| Kembali (Booking) | sekunder | Belum ada pemesanan | Kembali ke Pilih Bed pada langkah yang sama |
| Pesan Tempat Tidur | utama | Bed terpilih, episode ada | Konfirmasi lalu `POST /reservations` |

| Keadaan | Tampilan |
| --- | --- |
| Memuat (membuat episode) | Tombol "Menyimpan admisi..."; langkah tetap terlihat |
| Gagal membuat episode | Kotak merah berisi pesan server; tetap di Pilih Bed |
| Muat ulang setelah episode dibuat | Banner "Memuat admisi…" sebentar, lalu Pilih Bed dengan bed perlu dipilih ulang (pilihan bed hanya di memori) — tidak membuat episode kedua |
| Tidak berhak | Tidak berubah dari perilaku sekarang |

### 3.2 Rawat gabung — kartu konfirmasi boks

**SEBELUM** (ibu di VIP 301 lantai 3, lantai terakhir di peramban = 2):

```text
┌ Konfirmasi Penempatan Boks Bayi di Kamar Ibu ───────────────────┐
│ Kamar Perawatan Ibu   VIP 301 · Kelas VIP                       │
│ Boks Bayi Disediakan  Sedang mencari fasilitas boks bayi...     │
│ Status Kesiapan Boks  [ Boks Bayi Belum Tersedia ]              │
└────────────────────────────────────────────────────────────────┘
⚠ Boks Bayi Belum Tersedia di Kamar Ibu … atau alihkan ke Perinatologi.
[ Ubah ke Perawatan Perinatologi ]
[ Batal Pendaftaran ]       [ Kembali ] [ Pesan Boks Bayi & Lanjut (×) ]
```

**SESUDAH:**

```text
┌ Konfirmasi Penempatan Boks Bayi di Kamar Ibu ───────────────────┐
│ Pasien Ibu            Ny. Sari (No. RM 00-12-34)                │
│ Kamar Perawatan Ibu   VIP 301 · Kelas VIP · Lantai 3            │
│ Boks Bayi             301-BOKS                                  │
│   (bila > 1 boks:  Boks Bayi * [ Pilih boks ▾ ]  — K-01)        │
│ Status Kesiapan Boks  [ Tersedia & Siap Dipesan ]               │
└────────────────────────────────────────────────────────────────┘
[ Batal Pendaftaran ]       [ Kembali ] [ Pesan Boks Bayi & Lanjut ]
```

Keadaan khusus pengganti kalimat tunggal "belum tersedia":

```text
(a) Kamar ibu tanpa boks
⚠ Kamar ibu (Kelas 1 · 204) tidak memiliki boks bayi. Rawat gabung hanya dapat
  dilakukan di kamar yang memiliki boks. Pindahkan ibu ke kamar berboks atau pilih
  Ruang Perinatologi.
[ Ubah ke Perawatan Perinatologi ]

(b) Boks ada tetapi sedang terpakai / dibersihkan
⚠ Boks 302-BOKS sedang Dipakai (atau: Menunggu Pembersihan). Hubungi petugas bangsal
  untuk menyiapkan boks, lalu tekan Periksa Ulang.
[ Periksa Ulang ]   [ Ubah ke Perawatan Perinatologi ]

(c) Kamar ibu belum terbaca
⚠ Data kamar ibu belum terbaca. Tekan Periksa Ulang.
[ Periksa Ulang ]
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Kartu konfirmasi | Ibu, kamar, boks, status | Kandidat ibu + papan kamar ibu (`bed-board`, `available-beds` dengan `roomId` dan `isForNewborn`) | `BaseDetailCard`, `StatusBadge` existing |
| Pilihan boks (bila > 1) | Daftar boks tersedia di kamar ibu | `available-beds` | `BaseSelectField` existing |
| Peringatan keadaan | Satu dari (a)/(b)/(c) | Fungsi murni `pickRoomingInCrib` | `InformationAlert` existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Pesan Boks Bayi & Lanjut | utama | Ada boks berstatus tersedia dan terpilih | Tahap 1 membuat episode (bila belum) → tahap 2 memesan boks → maju ke Persetujuan Wali |
| Periksa Ulang | sekunder | Keadaan (b) atau (c) | Membaca ulang papan kamar ibu |
| Ubah ke Perawatan Perinatologi | sekunder | Keadaan (a) atau (b) | Mengubah rencana perawatan ke Neonatal, kembali ke tampilan denah |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | "Mencari boks bayi di kamar ibu…" |
| Gagal memuat papan | Kotak merah + Periksa Ulang |
| Pemesanan tercatat server tetapi jawaban hilang | Notifikasi "Pemesanan boks sudah tercatat" dan layar maju; **tidak** ada perintah muat ulang |
| Tidak berhak memesan | Pesan server apa adanya (perilaku sekarang) |

---

## 4. Rincian perbaikan

### FIX-EPS-038-01 — Episode milik layar sendiri tidak memicu gerbang pelanjutan; episode bayi langsung dicatat di URL

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-01`, `ISS-EPS-038-T2`, `ISS-EPS-038-T4` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` |
| **Radius dampak** | View ini dipakai lima route admisi: Pasien Baru, Pasien Lama, Bayi, Transfer, Kamar Pulih |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Turunkan `isOwnEpisode = Boolean(doctor.episodeId) && doctor.episodeId === flow.resumeEpisodeId`.
2. Gerbang render `:1174` menjadi `resume.isResuming && !resume.isReady && !isOwnEpisode ? null : …`.
3. Banner memuat `:1128-1133` dan banner "Melanjutkan admisi …" `:1162-1172` hanya tampil bila
   `!isOwnEpisode`. Banner "Admisi bayi … sudah aktif" tidak diubah.
4. Efek penulisan URL `:677-688`: untuk `newborn.enabled`, jangan lewati langkah `bed-selection` —
   `episodeId` ditulis (`replace: true`) segera setelah `doctor.episodeId` ada. Jalur non-bayi
   tidak diubah.

**Acceptance criteria.**

1. NICU: pilih `NICU-1`, klik "Lanjut ke Pemesanan" **sekali** → kartu "Pesan Tempat Tidur" tampil.
   Tab Network berisi tepat satu `POST …/episodes`.
2. Selama klik itu, area langkah tidak pernah kosong dan banner "Melanjutkan admisi" tidak muncul.
3. Rawat gabung: bila pemesanan gagal karena sebab apa pun, URL sudah memuat `episodeId` dari
   episode yang dibuat.
4. Setelah episode dibuat, muat ulang halaman → tidak ada `POST …/episodes` kedua; query T2 pada
   issue tidak menambah baris untuk bayi tersebut.
5. Membuka admisi `Draft` dari Daftar Pasien Rawat Inap ("Lanjutkan") tetap menampilkan banner
   memuat dan banner "Melanjutkan admisi" seperti sekarang.
6. Regresi: Pasien Baru, Pasien Lama, Transfer, dan Kamar Pulih sampai langkah Konfirmasi tanpa
   perubahan perilaku.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji manual skenario
S1, S2, S7 pada bagian 8.

**Risiko.** Efek adopsi episode pelanjutan di hook dokter (`use-inpatient-admission-doctor.jsx:191-245`)
tetap berjalan di latar belakang untuk episode sendiri dan mengisi ulang form dari server. Deteksi:
isian DPJP/catatan pada langkah 3 tetap sama setelah kembali dari langkah 4.

### FIX-EPS-038-02 — Sub-tampilan Pilih Bed ↔ Pesan dipegang pengendali bed

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend + tes unit |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`; `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`; `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`; `tests/unit/inpatient-newborn-admission.test.mjs` |
| **Radius dampak** | Hook bed dipakai semua jalur admisi; state baru hanya dibaca langkah bayi |
| **Bergantung pada** | `FIX-EPS-038-01` (urutan, bukan teknis) |

**Langkah.**

1. Di hook bed tambahkan `bookingViewOpen`, `openBookingView()`, `closeBookingView()`. Tutup
   otomatis ketika pilihan dilepas (`clearSelection`), pemesanan dibatalkan, masa tahan habis, atau
   `admission.sourceKey` berganti.
2. Tambahkan fungsi murni `resolveNewbornPlacementView({ isRoomingIn, hasActiveReservation, bookingViewOpen, hasSelectedBed })`
   yang mengembalikan `"rooming-in"`, `"booking"`, atau `"selection"`.
3. `NewbornPlacementStep`: hapus `useState` `inBookingView`; pakai fungsi di atas;
   `handleSelectionNext` → `bed.openBookingView()`; `handleBookingBack` → `bed.closeBookingView()`.
4. Ganti tes FIX-EPS-033 yang mencocokkan teks source (`:210-220`) dengan tes perilaku yang
   memanggil `resolveNewbornPlacementView`.

**Acceptance criteria.**

1. `resolveNewbornPlacementView({ isRoomingIn: false, hasActiveReservation: false, bookingViewOpen: true, hasSelectedBed: true })` → `"booking"`.
2. `… bookingViewOpen: true, hasSelectedBed: false` → `"selection"` (mencegah ISSUE-033 kembali).
3. `… hasActiveReservation: true` → `"booking"` apa pun nilai lainnya.
4. `… isRoomingIn: true` → `"rooming-in"`.
5. Kembali dari Booking tanpa pemesanan → Pilih Bed pada langkah 4, bukan langkah 3 (perilaku
   `FIX-EPS-033` tetap).

**Verifikasi.** `npm run test:unit` (tes baru lulus), lint, build; uji manual S1.

**Risiko.** Lupa menutup `bookingViewOpen` pada salah satu jalur pembatalan membuat Booking tampil
tanpa bed. Deteksi: acceptance 2 dan skenario S6.

### FIX-EPS-038-03 — Boks rawat gabung dari papan kamar ibu lintas lantai

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-02`, `ISS-EPS-038-T3` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend + tes unit |
| **Berkas yang diubah** | `use-inpatient-admission-bed.jsx`; `inpatient-newborn-admission-steps.jsx`; `inpatient-newborn-admission-utils.js`; `tests/unit/inpatient-newborn-admission.test.mjs` |
| **Radius dampak** | Hanya mode `RoomingIn`; jalur lain dijaga kondisi `isRoomingInNewborn` |
| **Bergantung pada** | Tidak ada; K-01 memakai rekomendasi sampai diputuskan lain |

**Langkah.**

1. Di hook bed, turunkan `isRoomingInNewborn` dari `newbornContext.newbornCareMode === "RoomingIn"`.
   Untuk mode ini:
   - papan lama (`useInpatientBedBoard`) **tetap aktif** setelah episode dibuat (`:127`); kueri
     sudah membawa `roomId` dan `isForNewborn`, dan sesudah episode ada juga `episodeId`;
   - `board` = papan lama (bukan denah), sehingga `reserve` mencari bed sasaran dan versinya dari
     `available-beds` lintas lantai;
   - denah (`useFloorPlan`) tidak dimuat, karena tidak ditampilkan.
2. Tambahkan fungsi murni `pickRoomingInCrib({ boardBeds, availableBeds, motherRoomId })` yang
   **hanya** menerima bed `isForNewborn === true` di kamar ibu dan mengembalikan
   `{ status: "available" | "occupied" | "no-crib" | "room-unknown", cribs }`.
3. `NewbornPlacementStep` dan `RoomingInPlacementConfirmation` memakai fungsi itu; hapus pencarian
   lama `:387-408` beserta cadangan bed dewasa dan boks tidak tersedia.
4. K-01 (rekomendasi): satu boks tersedia → terpilih otomatis; lebih dari satu → `BaseSelectField`
   wajib dipilih.
5. Kalimat keadaan mengikuti skema 3.2 (a)/(b)/(c).
6. Builder memastikan bed dari `bed-board` memuat `operationalVersion` untuk aktivasi
   (`use-inpatient-newborn-admission.js:232-238`); bila tidak, aktivasi memakai `available-beds`
   dengan `episodeId`.

**Acceptance criteria.**

1. Ibu di VIP 301 (lantai 3), `localStorage["quilvian.inpatient.floor-plan.last-floor"] = "2"` →
   `301-BOKS` tampil "Tersedia & Siap Dipesan".
2. Ibu di Kelas 1 · 204 (tanpa boks) → keadaan (a); tidak ada nama bed yang ditampilkan sebagai boks.
3. `302-BOKS` berstatus Dipakai → keadaan (b) menyebut status sebenarnya.
4. Bed kedua dewasa `VIP 301-B` yang kosong tidak pernah tampil sebagai "Boks Bayi".
5. `pickRoomingInCrib` diuji dengan empat keadaan di atas.
6. Tab Network pada rawat gabung tidak memuat `floor-plan`.

**Verifikasi.** `npm run test:unit`, lint, build; uji manual S3–S5.

**Risiko.** Papan lama yang tetap aktif menambah dua permintaan per pembacaan ulang. Deteksi: tidak
ada permintaan berulang tanpa aksi petugas selain penyegaran saat jendela kembali fokus.

### FIX-EPS-038-04 — Pemesanan boks dua tahap dan adopsi pemesanan yang sudah tercatat

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx`; `use-inpatient-admission-bed.jsx` |
| **Radius dampak** | Tombol rawat gabung; langkah 3 di bawah mengubah jalur galat `reserve` untuk semua admisi (hanya galat tidak pasti) |
| **Bergantung pada** | `FIX-EPS-038-01` (URL ditulis), `FIX-EPS-038-03` (boks dan papan) |

**Langkah.**

1. Hook bed mengembalikan `episodeId` dan menyediakan `requestReservation(bedId)`, yang menyimpan
   permintaan pemesanan tertunda.
2. Efek di hook bed menjalankan `reserve({ bedId })` milik render saat itu ketika permintaan
   tertunda ada, `episodeId` ada, belum ada pemesanan, dan tidak sedang sibuk; lalu menghapus
   permintaan tertunda.
3. `reserve`: bila galatnya tidak pasti (`operationUnknown`, "Sesi berubah", atau `409`), baca ulang
   papan dan cari pemesanan milik episode ini (`findBoardReservationForEpisode` pada papan aktif
   dan papan lama, seperti `:453-467`). Bila ada → `applyReservation`, notifikasi "Pemesanan boks
   sudah tercatat", kembalikan sukses.
4. Tombol "Pesan Boks Bayi & Lanjut": episode belum ada → `prepareBooking(boks)` lalu
   `requestReservation`; episode ada → `requestReservation`. Tidak ada pemanggilan `bed.reserve`
   sesudah `await`. Hapus baris mati `bed.episodeId` (`:424`).
5. Ketika `hasActiveReservation` menjadi benar karena permintaan dari tombol ini, panggil
   `flow.goNextWithEpisode(episodeId)` satu kali.

**Acceptance criteria.**

1. Rawat gabung, episode belum ada: satu klik → satu `POST …/episodes`, satu
   `POST …/bed-occupancies/reservations`, tanpa pesan "Sesi berubah", layar maju ke Persetujuan Wali
   dengan hitung mundur 15:00.
2. Klik dua kali cepat → tetap satu episode dan satu pemesanan.
3. Bila jawaban `POST reservations` hilang (jaringan diputus setelah terkirim, uji DevTools offline),
   setelah tersambung layar menampilkan pemesanan yang sudah tercatat, bukan galat.
4. Tidak ada permintaan `floor-plan` berstatus (canceled) pada rawat gabung.

**Verifikasi.** Lint, unit test, build; uji manual S3 dan S6.

**Risiko.** Efek pemesanan tertunda dapat berjalan dua kali bila penanda tidak dihapus sebelum
`await`. Deteksi: acceptance 2. Cegah memakai `actionInFlight` yang sudah ada.

### FIX-EPS-038-05 — Draf sesi bayi tanpa kunci `active`

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` |
| **Radius dampak** | Hook alur dipakai semua jalur admisi; perubahan dijaga `entryMode === "newborn"` |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Mode bayi: jangan menyimpan maupun memulihkan draf bila kunci berakhiran `:active`.
2. Saat halaman bayi dibuka, hapus `quilvian:inpatient-admission:flow:newborn:active`.
3. Mode bayi: ketika `storageKey` berpindah (pasien → episode), tulis ke kunci baru lalu hapus kunci
   lama.

**Acceptance criteria.**

1. Pilih ibu Ny. Sari di langkah 1, pindah ke Dashboard lewat sidebar, buka lagi Admisi Bayi →
   langkah 1 kosong.
2. Setelah episode dibuat, `sessionStorage` hanya memuat `…:newborn:<episodeId>` untuk bayi itu.
3. Muat ulang di langkah 3 (URL memuat `patientId`) → rencana perawatan tetap terisi (perilaku
   `FIX-EPS-029-04` dipertahankan).

**Verifikasi.** Lint, unit test, build; DevTools → Application → Session Storage.

**Risiko.** Muat ulang di langkah 1 mengosongkan pilihan ibu. Ini disengaja dan cukup dipilih ulang.

### FIX-EPS-038-06 — Kandidat ibu terpilih dicari ulang

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-T5` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` |
| **Radius dampak** | Hanya admisi bayi |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Bila `draft.deliveryCaseId` terisi, halaman 1 sudah dimuat, dan kandidatnya tidak ada, panggil
   `GET newborn-mother-candidates?search=<No. RM ibu>` sekali (No. RM dari detail ibu yang sudah
   dimuat), lalu gabungkan hasilnya.

**Acceptance criteria.**

1. Bayi dari kasus persalinan ke-60 terbaru, muat ulang di langkah 4 rawat gabung → kamar ibu
   terbaca, bukan "Kamar Penempatan Ibu Closed".

**Verifikasi.** Lint, unit test, build; uji manual.

**Risiko.** Pencarian No. RM yang sama mengembalikan beberapa kasus ibu; pemilihan tetap memakai
`deliveryCaseId`, jadi tidak tertukar.

### FIX-EPS-038-07 — Hapus isi pengganti yang tampak seperti data

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-T6` (bagian `BUG`) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx` (`:226-231`, `:411`) |
| **Radius dampak** | Hanya kartu konfirmasi rawat gabung |
| **Bergantung pada** | `FIX-EPS-038-03` (kartu yang sama) |

**Langkah.**

1. Ganti "VIP", "Kamar Penempatan Ibu", "Bed Ibu", "Kelas Perawatan Ibu" dengan "Belum terbaca".
2. Bila kamar ibu belum terbaca, pakai keadaan (c) skema 3.2 dan kunci tombol pesan beserta
   alasannya.

**Acceptance criteria.**

1. Ibu tanpa data kelas → baris kelas berbunyi "Belum terbaca", bukan "VIP".

**Verifikasi.** Lint, build; uji manual.

**Risiko.** Kecil.

### FIX-EPS-038-08 — Teks kartu "Rawat Gabung" tidak menyebut kamar/lantai tetap

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-038-T6` (bagian `DESIGN_CHANGE`) |
| **Area** | Frontend + dokumen |
| **Jenis perubahan** | Source frontend dan revisi dokumen desain |
| **Berkas yang diubah** | `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js` (`:21`, `:23`) |
| **Radius dampak** | Kartu pilihan perawatan langkah 3 |
| **Bergantung pada** | K-03 |

**Langkah.** Sesuai keputusan K-03; usulan: badge "Kamar Ibu", butir "Di kamar ibu yang memiliki
boks bayi".

**Acceptance criteria.** Teks kartu sama dengan keputusan K-03 dan tidak menyebut nomor lantai.

**Verifikasi.** Lint, build.

**Risiko.** Kecil.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-038-01 🟡   tanpa dependency — kerjakan lebih dulu
├── FIX-EPS-038-02 🟡   sub-tampilan di pengendali bed
└── FIX-EPS-038-04 🟡   pemesanan dua tahap (perlu URL episode dari FIX-01)
FIX-EPS-038-03 🟡   tanpa dependency — boleh paralel dengan FIX-01
├── FIX-EPS-038-04 🟡   (juga perlu papan kamar ibu dan pickRoomingInCrib)
└── FIX-EPS-038-07 ✅   kartu konfirmasi yang sama
FIX-EPS-038-05 🟡   tanpa dependency
FIX-EPS-038-06 🟡   tanpa dependency
K-03 (keputusan pemilik) ✅ diputuskan 23-10-2026
└── FIX-EPS-038-08 🟡
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan | Keputusan pemilik |
| ---: | --- | --- | --- | --- | --- |
| K-01 | Bila kamar ibu punya lebih dari satu boks kosong, siapa yang memilih? | Otomatis boks pertama / petugas memilih | Otomatis bila tepat satu; daftar pilihan bila lebih | Tidak — `FIX-EPS-038-03` memakai rekomendasi | Rekomendasi disetujui, 23-10-2026 |
| K-02 | Episode `Draft` ganda/yatim yang sudah terbentuk akibat bug | Gugur otomatis (`RWI-DEC-030`) / batal manual / skrip data | Gugur otomatis + batal manual bila mengganggu; tanpa skrip | Tidak | Rekomendasi disetujui, 23-10-2026 |
| K-03 | Teks kartu "Rawat Gabung" pada langkah 3 | Tetap "Kamar Ibu (VIP) · Lantai 3" / umum "Kamar ibu yang memiliki boks bayi" | Umum, agar tidak salah ketika master boks bertambah | `FIX-EPS-038-08` | Rekomendasi disetujui, 23-10-2026 |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `05-skema-tampilan.md` | Amandemen admisi bayi `RWI-BAYI-022-2.0.0` — langkah 4 | Tidak merinci perilaku sub-tampilan dan keadaan rawat gabung | Tambah "Amandemen PLAN-REPAIR-EPS-038": langkah 4 maju ke Booking dengan satu klik; rawat gabung membaca boks kamar ibu lintas lantai dan membedakan keadaan (a) tanpa boks, (b) boks terpakai, (c) kamar ibu belum terbaca |
| `docs/plan-repair/plan-repair-031-…md` | Teks kartu Rawat Gabung (`FIX-EPS-031-01`) | "Kamar Ibu (VIP)", "Kamar privat ibu di Lantai 3" | Mengikuti K-03 — hanya bila K-03 memilih teks umum |

Revisi dokumen dikerjakan lewat jalur blueprint yang berwenang, bukan oleh skill ini.

---

## 8. Verifikasi menyeluruh

Jalankan setelah semua perbaikan yang disetujui selesai, di peramban dengan DevTools terbuka:

| No | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| S1 | Bayi NICU: pilih `NICU-1` → Lanjut ke Pemesanan → Pesan Tempat Tidur → Persetujuan Wali | Satu klik per tombol; satu `POST episodes`; satu `POST reservations` |
| S2 | Ulangi S1, muat ulang setelah kartu Booking tampil | Kembali ke Pilih Bed pada episode yang sama; tidak ada `POST episodes` kedua |
| S3 | Rawat gabung, ibu di VIP 301; set lantai terakhir di localStorage ke "2" | `301-BOKS` tersedia; satu klik maju ke Persetujuan Wali; tanpa "Sesi berubah"; tanpa `floor-plan` |
| S4 | Rawat gabung, ibu di kamar tanpa boks | Keadaan (a) dengan nama kamar ibu |
| S5 | Rawat gabung, boks kamar ibu sedang dipakai | Keadaan (b) dengan status sebenarnya |
| S6 | S3 dengan jaringan diputus tepat setelah `POST reservations` terkirim | Setelah tersambung: pemesanan diadopsi; tidak ada perintah muat ulang |
| S7 | Buka "Lanjutkan" admisi `Draft` dari Daftar Pasien Rawat Inap | Banner memuat dan "Melanjutkan admisi" tetap tampil |
| S8 | Pilih ibu di langkah 1, pindah menu lewat sidebar, buka lagi Admisi Bayi | Langkah 1 kosong |
| S9 | Regresi: Pasien Baru, Pasien Lama, Transfer, Kamar Pulih sampai Konfirmasi | Tidak berubah |
| S10 | Jalankan query T2 pada issue | Tidak ada bayi baru dengan lebih dari satu `Draft` setelah S1–S6 |

Perintah wajib frontend: `npm run lint:errors`, `npm run test:unit`, `npm run build`.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-23 | Rencana dibuat: 8 perbaikan (7 siap dikerjakan, 1 menunggu K-03), menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-23 | Pemilik menyetujui seluruh rencana beserta rekomendasi K-01, K-02, K-03; `status_rencana` → `DISETUJUI`; `FIX-EPS-038-08` tidak lagi terblokir | Pemilik Sistem |
| 2026-10-23 | Seluruh perbaikan dikerjakan dalam task `FIX-EPS-038`. Source di-commit pemilik sebagai frontend `9de9f17cc` dan `229f071bb`; lint PASS, tes admisi bayi 22/22, build PASS, render SSR 11/11. `FIX-EPS-038-07` ✅; tujuh lainnya 🟡 menunggu uji layar S1–S10 (agent tidak dapat login) dan revisi dokumen hulu bagian 7 | `build-module-frontend` |
