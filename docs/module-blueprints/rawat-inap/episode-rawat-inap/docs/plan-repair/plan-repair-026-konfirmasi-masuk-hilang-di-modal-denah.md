# PLAN-REPAIR-026 — Pemulihan Konfirmasi Masuk dan Pembatalan Reservasi pada Modal Denah

```yaml
plan_id: PLAN-REPAIR-EPS-026
issue: ../issue/issue-026-konfirmasi-masuk-hilang-di-modal-denah.md
status_rencana: SELESAI_DILAKSANAKAN
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem (user sesi)"
tanggal_keputusan: "2026-10-17"
basis_source_backend: "c2bc563ed9332f08fa95b9fe35786fea7a647279 (MHamzah)"
basis_source_frontend: "15ae73984ad26a408e3038566d7e1d1533fe10c3 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan solusi | Area | Prioritas | Task ID | Status | Bukti target |
| --- | --- | --- | --- | ---: | --- | --- | --- |
| `FIX-EPS-026-01` | `ISS-EPS-026-01`, `ISS-EPS-026-T1`, `ISS-EPS-026-T2`, `ISS-EPS-026-T3` | Sambungkan kembali orkestrasi placement/cancel existing ke pop-up denah, tambahkan adapter refresh denah, pulihkan dialog/recovery/toast, dan kunci dengan test regresi | FE | 1 | `FE-RWI-263` | `SELESAI` | [Laporan task](../../task/report/frontend/FE-RWI-263.md) |

**Batas perubahan:** frontend saja. Tidak ada perubahan database, migrasi, endpoint, DTO, atau aturan bisnis backend.

---

## 2. Solusi Terpilih per Temuan

### `ISS-EPS-026-01` dan `ISS-EPS-026-T1` — Pulihkan aksi existing pada mode Lihat

| Opsi | Cara kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| 1. Kembalikan kartu/tabel lama | Monitoring denah dibatalkan dan UI lama dirender lagi | Kedua tombol langsung tersedia | Membatalkan keputusan denah, menduplikasi dua UI monitoring, dan berisiko membuat sumber data bercabang |
| **2. Integrasikan hook aksi existing ke modal denah (terpilih)** | `FloorPlanMonitoring` memakai `useInpatientBedBoardActions`; `FloorPlanRoomPopup` hanya merender tombol dan meneruskan event | Menjaga denah sebagai UI tunggal, reuse kontrak transaksi yang sudah teruji, perubahan kecil dan terlokalisasi | Membutuhkan adapter bentuk refresh serta koordinasi focus trap ketika dialog anak terbuka |
| 3. Tulis request placement langsung di popup | Popup memanggil service sendiri | Tampak cepat | Menduplikasi permission, refresh, version guard, failure parsing, recovery, toast, dan auto-close; rawan regresi issue 012 |

**Solusi terpilih: Opsi 2.** Hook `useInpatientBedBoardActions` tetap menjadi pemilik proses transaksi. Pop-up kamar hanya menjadi presentational trigger.

### `ISS-EPS-026-T2` — Adapter refresh yang eksplisit

`FloorPlanMonitoring` membuat callback refresh untuk aksi yang:

1. menunggu `state.refresh()` selesai;
2. mengambil hasil terbaru, bukan state render lama;
3. meratakan `fresh.rooms[].beds` menjadi `{ beds: [...] }`;
4. mengembalikan `null` bila refresh gagal agar hook menolak dialog dengan pesan aman;
5. tidak mengubah struktur canonical `normalizeFloorPlanResponse` untuk consumer denah lain.

Dengan demikian `prepare()` tetap mencari `bedId`, `episodeId`, `reservationId`, dan `operationalVersion` terbaru sebelum membuka dialog.

### `ISS-EPS-026-T3` — Test parity capability

Test fokus monitoring denah ditambah untuk menjaga empat hal berikut:

- respons `Reserved` mempertahankan `holdingContext.episodeId`, `reservationId`, dan `operationalVersion`;
- monitoring memasang `useInpatientBedBoardActions` melalui adapter `rooms[].beds`;
- pop-up mode Lihat menyediakan label `Konfirmasi Masuk` dan `Batalkan Pesanan`;
- dialog placement memakai `PlacementFailureList`, pembatalan mewajibkan alasan, dan operation recovery/toast tetap dirender.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Sebelum

```text
┌──────────────────────────────────────────────────────────────┐
│ KELAS II 329                                             [X] │
│ KELAS II  ·  3 tempat tidur · 1 terisi                      │
│                                                              │
│ 329-A  ● Dipesan                                             │
│ Pasien/episode sesuai hak · Sisa reservasi 01:58:26          │
│                                             [ Riwayat ]       │
│                                                              │
│ 329-B  ● Tersedia                  [ Riwayat ] [ Tutup bed ]  │
└──────────────────────────────────────────────────────────────┘

Masalah: reservasi terlihat, tetapi tidak ada tindakan untuk
menempatkan pasien atau membatalkan reservasi.
```

### Sesudah

```text
┌──────────────────────────────────────────────────────────────┐
│ KELAS II 329                                             [X] │
│ KELAS II  ·  3 tempat tidur · 1 terisi                      │
│                                                              │
│ 329-A  ● Dipesan                                             │
│ Pasien/episode sesuai hak · Sisa reservasi 01:58:26          │
│ [ Konfirmasi Masuk ] [ Batalkan Pesanan ] [ Riwayat ]       │
│                                                              │
│ 329-B  ● Tersedia                  [ Riwayat ] [ Tutup bed ]  │
└──────────────────────────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│ Konfirmasi Masuk Pasien                                 [X] │
│ Pasien uji · 329-A                                          │
│ Server menolak → semua alasan kelayakan tetap ditampilkan   │
│                              [ Batal ] [ Konfirmasi Masuk ]  │
└──────────────────────────────────────────────────────────────┘

Sukses: dialog dan pop-up kamar tertutup, toast tampil, denah
dibaca ulang, lalu bed berstatus Terisi.
```

### Wilayah

| Wilayah | Isi sesudah perbaikan | Sumber/guard |
| --- | --- | --- |
| Baris bed `Reserved` | Status, identitas yang diizinkan, countdown, aksi placement/cancel/history | Respons denah; tidak ada cache rehydrate identitas |
| Dialog konfirmasi masuk | Nama pasien yang sudah diizinkan, nama bed, daftar kegagalan server | Target hasil refresh terbaru + `PlacementFailureList` |
| Dialog pembatalan | Nama bed dan alasan wajib | Reservation ID terbaru + `ConfirmModal requireReason` |
| Notifikasi/recovery | Toast sukses/gagal dan pemulihan outcome ambigu | `ToastStack` + `BedOperationRecovery` existing |

### Tombol

| Tombol | Kapan tampil/aktif | Perilaku |
| --- | --- | --- |
| **Konfirmasi Masuk** | mode Lihat; `statusCode === "Reserved"`; `holdingContext.episodeId` dan `reservationId` tersedia; permission `InpatientBedOccupancy:Create` diizinkan | Loading saat prepare; refresh; dialog; `POST placements` dengan versi terbaru |
| **Batalkan Pesanan** | mode Lihat; reservation aktif dan metadata tersedia; permission `InpatientBedOccupancy:Update` diizinkan | Loading saat prepare; alasan wajib; `PATCH reservations/{id}/cancel` |
| **Riwayat** | Metadata tab riwayat mengizinkan | Tetap seperti sekarang |
| **Pembersihan / Tutup / Buka bed** | Guard existing masing-masing | Tidak berubah |

### Keadaan

| Keadaan | Tampilan/perilaku |
| --- | --- |
| Menyiapkan aksi | Tombol yang dipilih menampilkan loading; aksi transaksi lain dinonaktifkan |
| Permission ditolak/unknown | Tombol tidak ditampilkan; frontend tidak menginvent permission |
| `HoldingContext` disamarkan | Identitas dan kedua tombol reservasi tidak tampil |
| Reservasi berubah saat refresh | Dialog tidak dibuka; toast meminta muat ulang |
| HTTP 409 | Target dialog dibersihkan, denah dimuat ulang, pesan server tampil |
| HTTP 422 | Seluruh alasan kelayakan server ditampilkan melalui `PlacementFailureList` |
| Outcome ambigu | Operation recovery existing menyediakan pemeriksaan/ulangan key yang sama |
| Sukses | Pop-up kamar ditutup, toast sukses tampil, denah dibaca ulang, fokus kembali ke denah |

---

## 4. Rincian Perbaikan

### `FIX-EPS-026-01` — Integrasi aksi placement/cancel ke modal denah

#### A. `floor-plan-monitoring.jsx`

1. Import dan inisialisasi `useInpatientBedBoardActions`.
2. Tambahkan callback adapter, misalnya `refreshActions`, yang mengubah hasil `state.refresh()` menjadi `{ beds: fresh.rooms.flatMap(room => room.beds ?? []) }`.
3. Berikan `refresh: refreshActions` kepada hook dan `onSuccess: closeRoom` agar perbaikan auto-close issue 012 tetap berlaku.
4. Hitung `busy` dari loading denah, `preparing`, `operation.pending`, dan `operation.unknown`.
5. Teruskan permission, loading, busy, dan handler request ke `FloorPlanRoomPopup`.
6. Render ulang komponen existing di lapisan monitoring:
   - `BedOperationRecovery`;
   - `ConfirmModal` konfirmasi masuk;
   - `ConfirmModal requireReason` pembatalan;
   - `PlacementFailureList`;
   - `ToastStack`.
7. Setelah sukses, tutup pop-up kamar dan tunggu refresh. Pada konflik, pertahankan pesan server dan gunakan data denah terbaru.

#### B. `floor-plan-room-popup.jsx`

1. Tambahkan props aksi yang eksplisit; jangan memindahkan request/service ke komponen presentasi.
2. Pada `BedRow`, hitung reservasi aktif hanya dari status dan metadata server:
   - `bed.statusCode === "Reserved"`;
   - `bed.holdingContext?.episodeId` tersedia;
   - `bed.holdingContext?.reservationId` tersedia.
3. Render `BaseButton` **Konfirmasi Masuk** hanya jika reservasi aktif dan `canPlace` bernilai benar.
4. Render `BaseButton variant="danger"` **Batalkan Pesanan** hanya jika reservasi aktif dan `canCancel` bernilai benar.
5. Gunakan `loading={preparingConfirm}` / `loading={preparingCancel}` dan `disabled={busy}` agar perilaku issue 012 tidak kembali rusak.
6. Kedua tombol hanya berlaku pada mode Lihat; mode Pilih dan Master tidak berubah.
7. Saat dialog transaksi anak terbuka, jeda focus trap/Escape milik pop-up kamar agar dialog teratas menjadi satu-satunya pemilik keyboard. Setelah dialog ditutup tanpa sukses, fokus kembali ke trigger yang sesuai.

#### C. Reuse / create-new

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| `useInpatientBedBoardActions` | `REUSE_WITH_ADAPTER` | Seluruh orkestrasi transaksi sudah ada; hanya bentuk hasil refresh yang berbeda |
| `ConfirmModal` | `REUSE` | Mendukung loading, disable, dan reason wajib |
| `PlacementFailureList` | `REUSE` | Sudah merender alasan penolakan placement server |
| `BedOperationRecovery` | `REUSE` | Menjaga idempotensi/outcome ambigu |
| `ToastStack` | `REUSE` | Hook existing sudah menghasilkan toast |
| `BaseButton` | `REUSE` | Loading dan variant sudah sesuai katalog Quilvian |
| Komponen dasar baru | `TIDAK DIBUAT` | Tidak diperlukan |
| Adapter flatten respons denah | `CREATE_SMALL_ADAPTER` | Menghubungkan `rooms[].beds` ke kontrak hook tanpa mengubah response canonical |

#### D. Berkas target

- `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-monitoring.jsx`
- `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room-popup.jsx`
- `QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` hanya bila tata letak tiga tombol membutuhkan aturan responsif; gunakan token existing.
- `QuilvianSystemFrontendDev/tests/unit/floor-plan-monitoring.test.mjs`

Tidak ada target backend source.

#### E. Acceptance criteria

- [ ] Pengguna dengan `InpatientBedOccupancy:Create` melihat **Konfirmasi Masuk** pada bed `Reserved` yang memiliki metadata episode dan reservation.
- [ ] Pengguna dengan `InpatientBedOccupancy:Update` melihat **Batalkan Pesanan** pada reservasi aktif.
- [ ] Pengguna tanpa masing-masing hak tidak melihat tombol terkait; status permission `unknown` tidak dianggap allowed.
- [ ] Respons tanpa `HoldingContext` tidak menampilkan identitas maupun kedua tombol, termasuk untuk Housekeeping.
- [ ] Klik tombol selalu menunggu refresh denah terbaru sebelum dialog terbuka.
- [ ] Bila reservasi/status/version sudah berubah, dialog tidak dibuka dan pengguna menerima pesan aman.
- [ ] Konfirmasi sukses mengirim `episodeId`, `bedId`, serta `expectedBedVersion` terbaru; pop-up tertutup; toast tampil; denah berubah ke `Occupied/Terisi` setelah respons server.
- [ ] Pembatalan mewajibkan alasan, memakai `reservationId` dan versi terbaru, lalu mengubah bed sesuai proyeksi server.
- [ ] 409 memicu refresh; 422 menampilkan semua kegagalan; outcome ambigu menggunakan recovery existing.
- [ ] Aksi pembersihan, riwayat, buka/tutup bed, mode Pilih, mode Master, dan privacy redaction tidak regresi.
- [ ] Test fokus, lint error, lint terarah, build, serta live browser untuk peran berhak/tidak berhak lulus atau dicatat dengan bukti objektif.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-026-01A  Adapter refresh denah → kontrak hook aksi
        │
        ▼
FIX-EPS-026-01B  Orkestrasi action hook + dialog/recovery/toast
        │
        ▼
FIX-EPS-026-01C  Tombol guarded pada BedRow + koordinasi fokus
        │
        ▼
FIX-EPS-026-01D  Test regresi + lint + build + live browser
```

Semua langkah merupakan satu vertical slice dan satu task builder; task tidak boleh ditandai selesai jika hanya tombol tampil tetapi request, refresh, privacy, error state, atau test belum terbukti.

---

## 6. Keputusan Pemilik yang Dibutuhkan

| ID | Topik | Keputusan yang diminta | Rekomendasi |
| --- | --- | --- | --- |
| `K-026-01` | Persetujuan perbaikan regresi | Setujui integrasi ulang **Konfirmasi Masuk** dan **Batalkan Pesanan** pada baris bed `Dipesan` di modal denah | **Setujui.** Ini memulihkan capability yang sudah disetujui, bukan menambah aturan bisnis baru. |
| `K-026-02` | Posisi aksi | Letakkan kedua tombol langsung pada baris bed `Dipesan`, berdampingan dengan `Riwayat`, dengan wrap responsif | **Setujui.** Konteks bed tetap jelas dan sesuai pola pop-up sekarang. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak diperlukan perubahan keputusan bisnis, kontrak API, atau blueprint utama: `05-skema-tampilan.md:1031-1036` dan kontrak API sudah menetapkan placement/cancel existing. Saat implementasi disetujui, dokumen delivery berikut perlu ditambah/diperbarui:

- task frontend baru hasil `plan-module-delivery`;
- laporan task frontend;
- roadmap/traceability yang menautkan repair issue 026;
- catatan regresi `FE-RWI-247` bahwa capability parity placement/cancel dipulihkan.

Catatan arsitektur `FE-DNH-02` tentang `AvailableActions ∩ permission` tidak boleh ditafsirkan dengan mengarang aksi di browser. Untuk placement/cancel existing, visibility mengikuti kontrak eksplisit `Reserved + metadata + permission`, lalu refresh dan validasi commit server tetap menjadi otoritas akhir.

---

## 8. Verifikasi Menyeluruh

| No. | Prioritas | Skenario | Hasil yang diharapkan |
| ---: | --- | --- | --- |
| 1 | P0 | Login sebagai peran berhak Create/Update, buka kamar dengan bed `Dipesan` | Kedua tombol tampil pada baris bed yang tepat |
| 2 | P0 | Klik **Konfirmasi Masuk**, lalu setujui dialog | Ada refresh sebelum dialog; `POST /placements` memakai versi terbaru; sukses menutup pop-up; bed menjadi `Terisi`; episode menjadi `Admitted` |
| 3 | P0 | Ubah reservasi/version dari sesi lain sebelum konfirmasi | Tidak ada optimistic success; 409/changed state ditampilkan dan denah dibaca ulang |
| 4 | P1 | Klik **Batalkan Pesanan** tanpa alasan lalu dengan alasan valid | Tanpa alasan ditolak di UI; dengan alasan request cancel sukses dan status proyeksi diperbarui |
| 5 | P1 | Login dengan permission Create saja, Update saja, dan tanpa keduanya | Hanya tombol yang benar tampil; tidak ada grant yang diinvent frontend |
| 6 | P1 | Login sebagai Housekeeping/identity-redacted | Nama/RM/metadata sensitif dan tombol placement/cancel tidak muncul |
| 7 | P2 | Server mengembalikan 422 placement failures | Seluruh alasan tampil pada `PlacementFailureList`; input/konteks tidak berubah menjadi sukses |
| 8 | P2 | Simulasikan outcome ambigu/offline | Recovery existing tampil dan retry mempertahankan operation key/payload |
| 9 | P3 | Uji mode Pilih, cleaning, history, buka/tutup bed, pergantian lantai | Semua capability existing tetap berfungsi tanpa tombol placement pada mode yang salah |
| 10 | P4/P5 | Uji viewport desktop dan sempit, keyboard Tab/Shift+Tab/Escape | Tombol wrap rapi; hanya dialog teratas memegang fokus; fokus kembali dengan benar |
| 11 | Otomatis | Jalankan test fokus `floor-plan-monitoring.test.mjs` | Fixture Reserved, adapter, action wiring, privacy, dan regression assertions lulus |
| 12 | Quality gate | Jalankan lint terarah, `npm run lint:errors`, `npm run build`, dan `git diff --check` | Tidak ada error baru; build berhasil; diff bersih |

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-17 | Codex | Rencana awal PLAN-REPAIR-EPS-026 disusun dari diagnosis source dan kontrak; menunggu persetujuan pemilik. |
| 2026-10-17 | Pemilik Sistem / Codex | `K-026-01` dan `K-026-02` disetujui melalui instruksi "oke setujui tolong anda lakukan perbaikan"; task delivery frontend dikunci sebagai `FE-RWI-263`. |
| 2026-10-17 | Codex / Pemilik Sistem | `FE-RWI-263` selesai; pemilik mengonfirmasi runtime sudah berfungsi dan menetapkan testing/build berikutnya dilakukan mandiri. |

