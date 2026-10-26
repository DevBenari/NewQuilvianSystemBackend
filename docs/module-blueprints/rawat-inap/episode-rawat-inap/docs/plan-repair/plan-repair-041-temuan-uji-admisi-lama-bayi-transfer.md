# PLAN-REPAIR-041 — Perbaikan temuan live testing Admisi Pasien Lama, Bayi Baru Lahir, dan Transfer IGD & Poliklinik

```yaml
plan_id: PLAN-REPAIR-EPS-041
issue: ../issue/issue-041-temuan-uji-admisi-lama-bayi-transfer.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-26"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "e095f656 (MHamzah)"
basis_source_frontend: "2cd1c0610 (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-041-01` | `ISS-EPS-041-01`, `ISS-EPS-041-05` | Lengkapi respons antrean transfer: kebutuhan IGD, DPJP usulan, diagnosis, kelas diminta, identitas | BE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-02` | `ISS-EPS-041-01`, `ISS-EPS-041-05` | Verifikasi transfer menampilkan kebutuhan dan diagnosis; isi otomatis isolasi dan DPJP usulan | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-03` | `ISS-EPS-041-01` | Denah transfer mengirim konteks pasien ke `available-beds` | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-04` | `ISS-EPS-041-01`, `ISS-EPS-041-06` | `admit` meneruskan kode galat reservasi asli | BE | 2 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-05` | `ISS-EPS-041-01` | Tetapkan bed isolasi di master data | Data | 1 | — | ⛔ TERBLOKIR — menunggu K-01 | — |
| `FIX-EPS-041-06` | `ISS-EPS-041-02` | Pemeriksaan kelayakan pasien di langkah Verifikasi transfer | FE + dokumen | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-07` | `ISS-EPS-041-03` | Teks Konfirmasi dan dialog pembatalan jalur transfer sesuai kontrak | FE | 2 | — | ⛔ TERBLOKIR — menunggu K-02 | — |
| `FIX-EPS-041-08` | `ISS-EPS-041-04` | Simpan konsulen dan tingkat ketergantungan setelah `admit` transfer | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-09` | `ISS-EPS-041-06` | Rapikan label status, judul galat, tanda baca, dan kode status respons transfer | FE + BE | 3 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-10` | `ISS-EPS-041-07` | Lanjutkan admisi bayi yang identitasnya sudah ada; gerbang kembar hanya menghitung bayi berbeda | FE + BE + dokumen | 1 | — | ⛔ TERBLOKIR — menunggu K-03 | — |
| `FIX-EPS-041-11` | `ISS-EPS-041-07` | Penanganan RM bayi tanpa episode yang sudah terbentuk | Data | 2 | — | ⛔ TERBLOKIR — menunggu K-03 dan P-02 | — |
| `FIX-EPS-041-12` | `ISS-EPS-041-08` | Batas tanggal lahir, teks kelas, format waktu lahir, lantai denah bayi | FE | 3 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-13` | `ISS-EPS-041-08` | Pengaturan kode RS pada label dan unit layanan kamar NICU | Data | 3 | — | ⛔ TERBLOKIR — menunggu K-04 dan P-01 | — |
| `FIX-EPS-041-14` | `ISS-EPS-041-09` | Format waktu peringatan admisi Draft lain memakai WIB dan bahasa Indonesia | BE | 3 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-15` | `ISS-EPS-041-10` | Penjaga nominal deposit jalur biasa setara jalur transfer | FE + BE | 2 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-16` | `ISS-EPS-041-11` | Perbaiki label kamar "ODC 321" ganda | Data | 3 | — | ⛔ TERBLOKIR — menunggu P-01 | — |
| `FIX-EPS-041-17` | `ISS-EPS-041-12` | Tombol Selesai, keadaan memuat denah, waktu pesan setelah F5, `PlannedDepositAmount`, istilah "dikunci" | FE + BE | 3 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-041-18` | `ISS-EPS-041-12` | Konfirmasi menampilkan order deposit dan tingkat ketergantungan | FE + dokumen | 3 | — | ⛔ TERBLOKIR — menunggu K-06 | — |

**Ringkasan: 0 dari 18 perbaikan selesai. 11 dapat dikerjakan sekarang; 7 menunggu keputusan pemilik atau jawaban kueri.**

Perbaikan admisi Draft ganda (`ISS-EPS-041-09`, bagian perilaku) tidak didaftarkan di sini; ia tetap pada `FIX-EPS-022-01` (⛔ menunggu K-01/K-02 ISSUE-022). `ISS-EPS-041-T1` (footer) diteruskan ke pemilik layout global dan tidak masuk register.

---

## 2. Solusi terpilih per temuan

### ISS-EPS-041-01 — Pasien butuh isolasi ditawari bangsal biasa

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Kirim `patientId` + `requiresIsolation` dari jalur transfer ke `available-beds`, bawa kebutuhan IGD ke respons antrean, isi otomatis sakelar isolasi, teruskan kode galat, tetapkan bed isolasi | Memakai mekanisme server yang sudah ada (jalur bayi); layar dan server memakai aturan yang sama | Menyentuh hook Pilih Bed bersama |
| B | Buat episode Draft lebih dulu di jalur transfer, lalu pakai denah ber-`episodeId` seperti jalur biasa | Satu cara untuk semua jalur | Membongkar kontrak EPS-006/K-01 (satu transaksi `admit`) yang sudah disahkan |
| C | Saring bed isolasi di layar saja | Perubahan kecil | Aturan ganda di layar; dilarang pola "layar menyaring ulang dengan aturannya sendiri" (`use-inpatient-admission-bed.jsx:86-90`) |

**Solusi terpilih: Opsi A** (`FIX-EPS-041-01` s.d. `-05`).

1. Server sudah menerima `PatientId` dan `RequiresIsolation` tanpa episode (`InpBedOccupancyService.cs:135-146`), dan pembentuk kueri layar sudah mengirimnya bila `patientContext` berisi `patientId` (`inpatient-bed-utils.jsx:384-398`).
2. Tidak mengubah kontrak transaksi transfer yang sudah disahkan.

**Kenapa bukan opsi lain.** B membongkar kontrak yang disahkan 12-10-2026. C menduplikasi aturan server di layar.

### ISS-EPS-041-02 — Verifikasi transfer tanpa pemeriksaan kelayakan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pakai `useInpatientAdmissionEligibility` di langkah Verifikasi transfer, dengan alert dan tombol tindak lanjut yang sama seperti Pasien Lama | Reuse penuh; perilaku seragam | Perlu revisi cakupan Amandemen 010 |
| B | Server menandai permintaan "pasien sedang dirawat" di respons antrean | Terlihat sejak antrean | Menambah kueri per baris; tetap butuh pemeriksaan saat proses |

**Solusi terpilih: Opsi A** (`FIX-EPS-041-06`). Penanda di antrean (opsi B) dapat menyusul bila pemilik menginginkannya.

**Kenapa bukan opsi lain.** B saja tidak menahan petugas yang membuka permintaan lewat URL langsung.

### ISS-EPS-041-03 — Pembatalan admisi transfer

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pertahankan kontrak EPS-006/K-02; ubah teks Konfirmasi dan dialog pembatalan khusus jalur transfer | Tidak mengubah kontrak; perubahan kecil | IGD/Poli harus menerbitkan permintaan baru |
| B | Ubah kontrak: pembatalan mengembalikan permintaan ke antrean | Petugas bisa memproses ulang | Mengubah kontrak, menyentuh modul IGD dan Klinik; risiko surat dipakai ulang |

**Solusi terpilih: Opsi A** (`FIX-EPS-041-07`), menunggu konfirmasi K-02.

### ISS-EPS-041-04 — Konsulen dan ketergantungan hilang

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Setelah `admit` berhasil, layar menjalankan penyimpanan dokter pendukung dan ketergantungan yang sama dengan jalur biasa | Reuse endpoint yang ada; tidak mengubah kontrak `admit` | Dua langkah tulis; perlu penanganan bila langkah kedua gagal |
| B | Tambahkan dokter pendukung dan ketergantungan ke payload `admit` | Satu transaksi | Mengubah kontrak dan orchestrator backend |

**Solusi terpilih: Opsi A** (`FIX-EPS-041-08`). Jalur biasa sudah memakai pola yang sama, termasuk peringatan bila penyimpanan ketergantungan gagal (`use-inpatient-admission-doctor.jsx:863-880`).

### ISS-EPS-041-05 — Data rujukan tidak terbawa

Hanya satu cara yang wajar: lengkapi respons antrean dari data sumber yang sudah ada, lalu tampilkan dan pakai di layar (`FIX-EPS-041-01`, `-02`). Tidak ada tabel baru atau keputusan bisnis baru.

### ISS-EPS-041-06 — Teks, status, dan kode galat transfer

Satu cara wajar per butir (`FIX-EPS-041-04`, `-09`): pakai helper label status yang ada (`getBedReservationStatusLabel`), pisahkan state galat muat dan galat simpan, perbaiki pembersih teks, teruskan kode galat, dan samakan `statusCode` respons. Butir 6 dibuktikan dulu sebelum diperbaiki.

### ISS-EPS-041-07 — RM bayi tanpa episode

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Bayi tanpa episode tampil "Admisi belum selesai" dengan tombol "Lanjutkan admisi bayi ini" yang memakai pasien bayi yang sudah ada; gerbang kembar hanya menghitung bayi yang sudah punya episode atau dikonfirmasi berbeda | Mencegah RM ganda; urutan kelahiran tetap benar | Menambah satu cabang di langkah 1 |
| B | Pindahkan pembuatan pasien bayi ke langkah Aktifkan | Tidak ada RM yatim baru | Membongkar desain RWI-BAYI-022 (RM terbit di langkah 2, dipakai encounter dan reservasi) |
| C | Batalkan otomatis pasien bayi yang tidak diadmisi dalam X jam | Otomatis | Menghapus data rekam medis tanpa persetujuan Rekam Medis |

**Solusi terpilih: Opsi A** (`FIX-EPS-041-10`), menunggu K-03. Data lama ditangani terpisah (`FIX-EPS-041-11`).

### ISS-EPS-041-08 — Catatan kecil jalur bayi

Satu cara wajar per butir (`FIX-EPS-041-12`, `-13`). Batas tanggal memakai `field.min`/`field.max` yang sudah didukung `BaseDateField`; nilai batas mengikuti keputusan K-03 ISSUE-040 yang sudah diterapkan di validasi.

### ISS-EPS-041-09 — Format waktu peringatan

Satu cara wajar: `InpAdmissionText.FormatDateTime` (`FIX-EPS-041-14`).

### ISS-EPS-041-10 — Deposit tanpa batas

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Samakan dengan jalur transfer: `Number.isSafeInteger` di layar dan batas maksimum di server, ditambah pesan di bawah isian | Reuse aturan yang sudah ada | Batas kebijakan masih menunggu K-05 |
| B | Batas kebijakan saja (misalnya Rp 1 miliar) | Paling ketat | Butuh keputusan bisnis lebih dulu |

**Solusi terpilih: Opsi A** sekarang; batas kebijakan ditambahkan setelah K-05 (`FIX-EPS-041-15`).

### ISS-EPS-041-11 dan ISS-EPS-041-12

`FIX-EPS-041-16` (data) dan `FIX-EPS-041-17`/`-18` mengikuti rekomendasi pada issue; tidak ada opsi lain yang bermakna.

---

## 3. Skema tampilan sebelum → sesudah

Tanda rangka mengikuti `05-skema-tampilan.md` bagian 1. Data contoh memakai nama pseudonim.

### 3.1 Verifikasi Pasien jalur transfer — `FIX-EPS-041-02`, `FIX-EPS-041-06`

**SEBELUM**

```text
TRANSFER PASIEN IGD   No. Kunjungan: ENC-EMG-…   CITO   Waktu Tunggu: 113 mnt
┌ IDENTITAS PASIEN ─────────────── TERDAFTAR ┐  ┌ RUJUKAN RAWAT INAP ───────────────────┐
│ Nama, No. RM, Tempat/Tgl lahir, …          │  │ DPJP DITUJU / PEMERIKSA: dr. Jaga IGD │
└────────────────────────────────────────────┘  │ REKOMENDASI RUANG: Rawat Inap         │
                                                │ DIAGNOSA / INDIKASI: <alasan rujukan> │
                                                │ TTV: GCS · TD · Nadi · RR · SpO2      │
                                                │ Catatan Disposisi: .                  │
                                                │ INSTRUKSI TINDAK LANJUT               │
                                                └───────────────────────────────────────┘
                                         [ Ganti Pasien Transfer ] [ Lanjut ke Pembayaran ]
```

**SESUDAH**

```text
TRANSFER PASIEN IGD   No. Kunjungan: ENC-EMG-…   CITO   Waktu Tunggu: 113 mnt
⚠ Pasien tidak dapat diadmisi — Ny. S sudah dirawat pada episode RI-… di VIP 302.   ← hanya bila diblokir
  [ Pindah Tempat Tidur ] [ Lihat Detail Episode ]
┌ IDENTITAS PASIEN ─── <lencana hasil kelayakan> ┐  ┌ RUJUKAN RAWAT INAP ────────────────────────┐
│ Nama, No. RM, Tempat/Tgl lahir, …              │  │ DPJP USULAN RAWAT INAP: dr. Umum Ranap     │
└────────────────────────────────────────────────┘  │ DIPUTUSKAN OLEH: dr. Jaga IGD              │
                                                    │ KEBUTUHAN: [Isolasi] [O2] [Monitoring]     │
                                                    │ KELAS DIMINTA: Kelas I          (Poli)     │
                                                    │ DIAGNOSA: Demam Berdarah Dengue  (Poli)    │
                                                    │ ALASAN RAWAT INAP: <alasan rujukan>        │
                                                    │ TTV: GCS · TD · Nadi · RR · SpO2           │
                                                    │ Catatan Disposisi: ronkhi basah basal      │
                                                    │ INSTRUKSI TINDAK LANJUT                    │
                                                    └────────────────────────────────────────────┘
                                         [ Ganti Pasien Transfer ] [ Lanjut ke Pembayaran ]
```

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Alert kelayakan | Kalimat blokir/peringatan server dan tombol tindak lanjut | `GET /episodes/admission-eligibility?patientId=` | `InformationAlert` + `BaseButton` (sama dengan Pasien Lama) |
| Lencana identitas | "Siap Melanjutkan", "Sedang Dirawat", "Kunjungan Aktif", "Ada Admisi Disiapkan", "Belum Terperiksa" | Hasil kelayakan | `BasePatientVerificationCard` |
| DPJP usulan / diputuskan oleh | Dua baris terpisah | `proposedInpatientDoctorName`, `decidedByDoctorName` | Kartu rujukan existing |
| Kebutuhan | Chip Isolasi, Intensif, O2, Monitoring; hanya yang bernilai benar | `needsIsolation`, `needsIntensiveCare`, `needsOxygenSupport`, `needsSpecialMonitoring` | `StatusBadge` |
| Kelas diminta, Diagnosa | Hanya untuk sumber Poli; baris disembunyikan bila kosong | `requestedRoomClass`, `diagnosis` | Kartu rujukan existing |
| Alasan rawat inap | Label diganti dari "Diagnosa / Indikasi" | `dispositionReason` | Kartu rujukan existing |

**Tombol**

| Label | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Pembayaran | Primer | Selalu (tidak dimatikan karena hasil kelayakan, sesuai Amandemen 010) | Memeriksa kelayakan; boleh → langkah 3; ditolak → tetap di langkah 2, alert digulir ke pandangan |
| Pindah Tempat Tidur / Lihat Detail Episode | Sekunder | Bila pasien sedang dirawat | Membuka layar perpindahan / detail episode |
| Ganti Pasien Transfer | Sekunder | Selalu | Kembali ke antrean |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Memuat kelayakan | Lencana "Belum Terperiksa"; tombol Lanjut menampilkan "Memeriksa kelayakan…" |
| Gagal diperiksa | Alert dengan tombol "Periksa Ulang" |
| Pasien sedang dirawat | Alert merah dan tombol tindak lanjut (contoh di rangka) |
| Ada admisi Draft lain | Alert peringatan "Lanjutkan Admisi {nomor}"; Lanjut tetap dapat dipakai |
| Sumber tanpa kebutuhan khusus | Baris Kebutuhan disembunyikan |

### 3.2 Pilih Ibu Persalinan — bayi yang admisinya belum selesai — `FIX-EPS-041-10`

**SEBELUM**

```text
Riwayat Bayi yang Sudah Didaftarkan
  Bayi ke-1   By. Ny. M · RM … · Terdaftar
⚠ Bayi Sudah Terdaftar di Ruangan Rawat Inap — … beri tanda centang konfirmasi … (Bayi ke-2)
  [ ] Konfirmasi: Pasien ini adalah pendaftaran bayi kembar berikutnya (Bayi ke-2)
      "…hanya bila … bayi sebelumnya telah selesai diadmisi."
                                                         [ Lanjut ke Identitas Bayi ]
```

**SESUDAH**

```text
Riwayat Bayi pada Kasus Persalinan Ini
  Bayi ke-1   By. Ny. M · RM …   Admisi belum selesai      [ Lanjutkan admisi bayi ini ]
ⓘ Bayi ke-1 sudah memiliki No. RM, tetapi admisinya belum selesai. Lanjutkan admisinya
  supaya tidak terbentuk No. RM ganda.
  [ ] Ini bayi yang berbeda (kelahiran kembar, Bayi ke-2)
                                                         [ Lanjut ke Identitas Bayi ]
```

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Baris bayi | Nama, No. RM, status: "Sedang dirawat — {kamar}", "Admisi belum selesai", atau "Pulang/Batal" | `registeredBabies[]` (`episodeId`, `episodeStatus`, `isCurrentlyInRoom`) | Daftar existing |
| Tombol per baris | "Lanjutkan admisi bayi ini" hanya untuk bayi tanpa episode aktif | — | `BaseButton` |
| Info | Penjelasan risiko RM ganda | Teks tetap | `InformationAlert` (info) |
| Centang kembar | Wajib hanya untuk membuat bayi **baru** saat sudah ada bayi lain | — | Checkbox existing |

**Tombol**

| Label | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjutkan admisi bayi ini | Sekunder | Bayi tanpa episode aktif | Memakai pasien bayi yang ada; melewati langkah Identitas (baca-saja) ke langkah Perawatan dan DPJP |
| Lanjut ke Identitas Bayi | Primer | Selalu | Bila ada bayi lain dan centang kosong: pesan "Pilih 'Lanjutkan admisi bayi ini' atau centang 'Ini bayi yang berbeda'" |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Belum ada bayi | Tidak ada daftar dan tidak ada centang |
| Semua bayi sudah diadmisi | Tidak ada tombol Lanjutkan; centang kembar seperti sekarang |
| Bayi tanpa episode | Baris "Admisi belum selesai" dengan tombol Lanjutkan |

### 3.3 Konfirmasi dan dialog pembatalan jalur transfer — `FIX-EPS-041-07`, `FIX-EPS-041-08`

**SEBELUM**

```text
PERAWATAN   DPJP Utama: dr. Jantung      (konsulen tidak tampil)
TEMPAT TIDUR   Status Tempat Tidur: Active
🔒 DPJP dan penjamin sudah melekat pada kunjungan. Jika keliru, admisi harus dibatalkan dan dibuka ulang.

Dialog "Batalkan Admisi Rawat Inap?"
  … Pemesanan tempat tidur akan dilepas …   Alasan Pembatalan *   [ Kembali ] [ Batalkan Admisi ]
```

**SESUDAH**

```text
PERAWATAN   DPJP Utama: dr. Jantung
            Dokter Pendukung: dr. Bedah (Konsulen) — "Konsul bedah toraks bila efusi pleura"
            Tingkat Ketergantungan: High Dependency
TEMPAT TIDUR   Status Tempat Tidur: Dipesan
🔒 DPJP dan penjamin sudah melekat pada kunjungan. Pada admisi transfer, pembatalan menutup
   permintaan IGD/Poli ini; bila perlu dirawat ulang, IGD/Poli menerbitkan permintaan baru.

Dialog "Batalkan Admisi Rawat Inap?"
  … Pemesanan tempat tidur akan dilepas …
  ⚠ Permintaan rawat inap dari IGD (EMG-…) tidak kembali ke antrean setelah dibatalkan.
  Alasan Pembatalan *   [ Kembali ] [ Batalkan Admisi ]
```

**Wilayah, Tombol, Keadaan.** Wilayah PERAWATAN menambah dua baris dari episode (`supportingDoctors`, `careDependencyLevel`). Tidak ada tombol baru. Teks transfer hanya tampil bila `flow.entryMode === transfer`; jalur lain tidak berubah. Bila konsulen gagal tersimpan, Konfirmasi menampilkan peringatan "Dokter pendukung belum tersimpan" dengan tombol "Simpan Ulang".

### 3.4 Perbaikan tanpa perubahan susunan tampilan

`FIX-EPS-041-01`, `-03`, `-04`, `-05`, `-09`, `-11` s.d. `-17` hanya mengubah isi, teks, atau data; susunan wilayah tetap. `FIX-EPS-041-18` menambah dua baris baca-saja pada Konfirmasi bila K-06 disetujui.

---

## 4. Rincian perbaikan

### FIX-EPS-041-01 — Lengkapi respons antrean transfer

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-01`, `ISS-EPS-041-05` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (DTO dan query); kontrak respons diperluas tanpa mengubah kolom lama |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`; `Services/InpAdmissionTransferService.Queries.cs` |
| **Radius dampak** | `GET /admission-transfers` dan `GET /admission-transfers/{id}`; konsumen hanya layar Admisi Transfer. Pemilik data sumber: IGD (`EmgAdmissionRequest`) dan Klinik (`CliDoctorCertificate`) — dibaca saja |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tambahkan ke `InpatientAdmissionTransferItemResponse`: `NeedsIsolation`, `NeedsIntensiveCare`, `NeedsOxygenSupport`, `NeedsSpecialMonitoring`, `EstimatedLengthOfStayDays`, `ProposedInpatientDoctorId`, `ProposedInpatientDoctorName`, `Diagnosis`, `RequestedRoomClass`.
2. Sumber IGD: isi dari `EmgAdmissionRequest` berstatus `Signed` milik disposisi itu.
3. Sumber Poli: isi `Diagnosis` dari `ReferralDiagnosis` (atau `Diagnosis` bila kosong) dan `RequestedRoomClass`; kebutuhan IGD bernilai `false`.
4. Isi `Gender`, `NationalId`, dan `PhoneNumber` yang selama ini ada di DTO tetapi selalu `null`.

**Acceptance criteria.**

1. Permintaan IGD Tn. R (suspek TB, `NeedsIsolation = true`) mengembalikan `needsIsolation: true`, `needsOxygenSupport: false`.
2. Permintaan IGD Tn. A (CITO) mengembalikan `proposedInpatientDoctorName` = DPJP usulan IGD, berbeda dari `decidedByDoctorName` = dokter jaga.
3. Surat pengantar Poli Nn. D mengembalikan `diagnosis: "Demam Berdarah Dengue"` dan `requestedRoomClass: "Kelas I"`.
4. Kolom lama (`isCito`, `dispositionReason`, `decidedByDoctorId`, dst.) tidak berubah nilai maupun nama.

**Verifikasi.** `dotnet build`; panggil kedua endpoint pada DB pengembang yang diizinkan dan bandingkan dengan data sumber.

**Risiko.** Subkueri tambahan memperlambat antrean. Deteksi: waktu respons `GET /admission-transfers?pageSize=50` sebelum dan sesudah.

### FIX-EPS-041-02 — Verifikasi transfer menampilkan kebutuhan dan diagnosis; isi otomatis isolasi dan DPJP usulan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-01`, `ISS-EPS-041-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-review-step.jsx`; `lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` (isian awal transfer, baris 205-212) |
| **Radius dampak** | Hanya jalur transfer (isian awal dibaca dari `transferSource`) |
| **Bergantung pada** | `FIX-EPS-041-01` |

**Langkah.**

1. Ubah kartu rujukan sesuai skema 3.1: baris DPJP usulan dan "Diputuskan oleh", chip Kebutuhan, Kelas diminta, Diagnosa, dan label "Alasan Rawat Inap".
2. Isian awal langkah Dokter: `doctorId` dari `proposedInpatientDoctorId`, lalu `decidedByDoctorId` bila kosong; `requiresIsolation` dari `needsIsolation`; `isolationNote` diisi awal "Permintaan IGD menyebut kebutuhan isolasi" agar validasi keterangan terpenuhi dan dapat diubah petugas.
3. Isian yang sudah diubah petugas (draf tersimpan) tetap menang atas isian awal.

**Acceptance criteria.**

1. Membuka permintaan Tn. R menampilkan chip "Isolasi", dan sakelar isolasi di langkah Dokter sudah tercentang dengan keterangan terisi.
2. Membuka permintaan Tn. A menampilkan "DPJP Usulan Rawat Inap: <dokter usulan>", dan kartu DPJP itu yang terpilih di langkah Dokter.
3. Surat Poli Nn. D menampilkan "Kelas Diminta: Kelas I" dan "Diagnosa: Demam Berdarah Dengue"; baris itu tidak tampil untuk sumber IGD yang kosong.
4. "Catatan Disposisi" tidak lagi menampilkan "." saja (lihat juga `FIX-EPS-041-09`).

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser dengan tiga permintaan di atas.

**Risiko.** Isian awal menimpa pilihan petugas setelah muat ulang. Deteksi: pilih DPJP lain, tekan F5, pastikan pilihan petugas bertahan.

### FIX-EPS-041-03 — Denah transfer mengirim konteks pasien ke `available-beds`

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` (baris 132-141); `lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` (baris 87-101) bila parameter konteks perlu digeneralisasi |
| **Radius dampak** | `use-inpatient-bed-board` juga dipakai Papan Tempat Tidur (`FE-INP-02`) dan jalur bayi. Perubahan hanya aktif bila pemanggil mengirim konteks; pemakai lain tidak berubah |
| **Bergantung pada** | Tidak ada (manfaat penuh untuk pasien isolasi setelah `FIX-EPS-041-05`) |

**Langkah.**

1. Pada jalur transfer tanpa episode, kirim konteks `{ patientId, requiresIsolation, isOdcBed }` dari form Dokter ke `useInpatientBedBoard`, memakai jalur `patientContext` yang sama dengan bayi.
2. Baca ulang `available-beds` ketika sakelar isolasi berubah di langkah Dokter.
3. Bed yang tidak lolos menampilkan alasan server (`includeIneligible`) di dialog kamar, sama seperti jalur bayi.

**Acceptance criteria.**

1. Untuk Tn. R dengan isolasi menyala, kamar Kelas II 329 dan VIP 307 nonaktif dengan alasan "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi."
2. Untuk Tn. A tanpa isolasi, kamar ODC nonaktif ("tidak ada bed yang layak untuk pasien ini"), sama seperti jalur Pasien Lama.
3. Papan Tempat Tidur dan admisi bayi menampilkan hasil yang sama seperti sebelum perubahan.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser jalur transfer, bayi, dan Papan Tempat Tidur.

**Risiko.** Kueri berganda saat sakelar dinyalakan-dimatikan berulang. Deteksi: tab jaringan; batalkan permintaan lama (pola `AbortController` existing).

### FIX-EPS-041-04 — `admit` meneruskan kode galat reservasi asli

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-01`, `ISS-EPS-041-06` butir 3 |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Services/InpAdmissionTransferService.cs` (baris 152-154) |
| **Radius dampak** | Respons galat `POST /admission-transfers/admit` saja |
| **Bergantung pada** | Tidak ada |

**Langkah.** Teruskan kode kegagalan pertama dari hasil reservasi (misalnya `ISOLATION_REQUIRED` atau `NEWBORN_BED_ONLY`) sebagai `errors.code`; `BED_NOT_READY` hanya dipakai bila bed memang belum siap.

**Acceptance criteria.**

1. Pesan isolasi Tn. R menghasilkan 422 dengan `errors.code: "ISOLATION_REQUIRED"`.
2. Bed yang sedang dibersihkan tetap menghasilkan `BED_NOT_READY`.

**Verifikasi.** `dotnet build`; uji dua kasus di atas.

**Risiko.** Layar yang memeriksa `BED_NOT_READY` secara harfiah. Deteksi: `rg "BED_NOT_READY"` di frontend sebelum mengubah.

### FIX-EPS-041-05 — Tetapkan bed isolasi di master data

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-01` |
| **Area** | Data master (Manajemen Tempat Tidur) |
| **Jenis perubahan** | Koreksi data lewat layar Master Bed yang berwenang — bukan skrip |
| **Radius dampak** | Seluruh penempatan: bed isolasi menolak pasien non-isolasi (Aturan 8) |
| **Bergantung pada** | K-01 |

**Langkah.** Pemilik menetapkan kamar/bed isolasi (K-01), lalu petugas master bed menandai `IsIsolationBed`.

**Acceptance criteria.**

1. Kueri `SELECT COUNT(*) FROM "MstBed" WHERE "IsIsolationBed" AND NOT "IsDelete"` (dijalankan pemilik DB) bernilai lebih dari 0.
2. Tn. R dapat dipesankan bed isolasi; pasien non-isolasi ditolak di bed itu.

**Verifikasi.** Uji di browser setelah `FIX-EPS-041-03`.

**Risiko.** Kapasitas bangsal biasa berkurang karena bed isolasi tidak dapat dipakai pasien lain (Aturan 8 dua arah) — konsekuensi yang harus diketahui pemilik saat memutuskan K-01.

### FIX-EPS-041-06 — Pemeriksaan kelayakan di langkah Verifikasi transfer

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-02` |
| **Area** | Frontend + dokumen |
| **Jenis perubahan** | Source frontend; revisi cakupan Amandemen 010 di `05-skema-tampilan.md` |
| **Berkas yang diubah** | `components/view/health-services/inpatient-management/inpatient-admission-view.jsx` (baris 635-651); `admission-transfers/inpatient-admission-transfer-review-step.jsx` (lencana baris 436-443, wilayah alert) |
| **Radius dampak** | Hook kelayakan dipakai juga Pasien Lama dan Kamar Pulih; perubahan hanya menambah slug `transfer-patient-review` |
| **Bergantung pada** | Revisi dokumen (bagian 7) |

**Langkah.**

1. Tambahkan `transfer-patient-review` ke `isEligibilityStep`, dengan `patientId` dari sumber transfer.
2. Tampilkan alert dan tombol tindak lanjut seperti skema 3.1; lencana mengikuti hasil.
3. "Lanjut ke Pembayaran" memeriksa kelayakan lebih dulu; ditolak → tetap di langkah 2.
4. Perbaiki komentar "jalur transfer punya pemeriksaan sendiri".

**Acceptance criteria.**

1. Membuka permintaan Poli Ny. S (sedang dirawat di VIP 302) menampilkan alert "Pasien tidak dapat diadmisi … sudah dirawat pada episode …" dan lencana "Sedang Dirawat"; menekan Lanjut tidak berpindah langkah; 0 POST.
2. Permintaan Tn. A berlencana "Siap Melanjutkan" dan dapat lanjut seperti sebelumnya.
3. Pasien dengan admisi Draft lain mendapat peringatan, dan Lanjut tetap dapat dipakai (sesuai Amandemen 010).

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser memakai permintaan `SPRI/2026/10/AGY04` yang masih ada di antrean.

**Risiko.** Pemeriksaan ulang memblokir alur pelanjutan transfer. Deteksi: buka URL transfer yang sudah punya episode; pemeriksaan tidak boleh berjalan (`!flow.resumeEpisodeId`).

### FIX-EPS-041-07 — Teks Konfirmasi dan dialog pembatalan jalur transfer

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (teks) |
| **Berkas yang diubah** | `inpatient-admission-confirmation-step.jsx` (baris 605); komponen dialog pembatalan admisi (FE-INP-17 di wizard) |
| **Radius dampak** | Teks hanya berubah untuk `entryMode === transfer` |
| **Bergantung pada** | K-02 (pilihan a) |

**Langkah.** Ganti kalimat pada jalur transfer dan tambahkan peringatan pada dialog sesuai skema 3.3.

**Acceptance criteria.**

1. Konfirmasi jalur transfer tidak lagi memuat "dibuka ulang"; jalur Pasien Lama tetap memuat kalimat lama.
2. Dialog pembatalan jalur transfer menyebut nomor permintaan sumber dan bahwa permintaan tidak kembali ke antrean.

**Verifikasi.** `npm run lint:errors`, `npm run build`; uji di browser.

**Risiko.** Bila pemilik memilih K-02 (b), perbaikan ini diganti revisi kontrak dan perubahan backend lintas modul.

### FIX-EPS-041-08 — Simpan konsulen dan tingkat ketergantungan setelah `admit` transfer

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` (ekstrak langkah 2b dan penyimpanan ketergantungan, baris 811-880, menjadi fungsi yang juga dipanggil setelah `adoptTransferEpisode`, baris 955-960); `components/view/health-services/inpatient-management/inpatient-admission-view.jsx` (pemanggil `transfer.submit`); `inpatient-admission-confirmation-step.jsx` (baris dokter pendukung dan ketergantungan) |
| **Radius dampak** | Jalur biasa memakai fungsi hasil ekstraksi yang sama; perilakunya tidak boleh berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Ekstrak penyimpanan dokter pendukung dan ketergantungan menjadi fungsi bersama.
2. Panggil fungsi itu sesudah `admit` berhasil, memakai `episodeId` hasil transfer.
3. Bila salah satu penyimpanan gagal, tampilkan peringatan dan tombol "Simpan Ulang" di Booking/Konfirmasi tanpa membatalkan admisi.
4. Tampilkan dokter pendukung dan ketergantungan di Konfirmasi.

**Acceptance criteria.**

1. Transfer Tn. R dengan konsulen dr. Bedah dan Low Dependency menghasilkan dua baris `InpDoctorAssignment` (DPJP dan Konsulen) serta `CareDependencyLevel = 2` dengan satu riwayat ketergantungan.
2. Transfer Tn. A dengan High Dependency tanpa konsulen menghasilkan `CareDependencyLevel = 1`.
3. Admisi Pasien Lama Tn. B tetap menyimpan High Dependency seperti sebelum perubahan.
4. Konfirmasi transfer menampilkan "Dokter Pendukung: dr. Bedah (Konsulen)".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser dan baca-ulang episode lewat `GET /episodes/{id}`.

**Risiko.** Penyimpanan ganda saat petugas menekan "Simpan Ulang" berkali-kali. Deteksi: endpoint penugasan menolak duplikat; uji klik ganda.

### FIX-EPS-041-09 — Rapikan label status, judul galat, tanda baca, dan kode status respons

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-06` butir 1, 2, 4, 5, 6 |
| **Area** | Frontend dan Backend |
| **Jenis perubahan** | Source |
| **Berkas yang diubah** | FE: `inpatient-admission-confirmation-step.jsx` (baris 389, 432-433) memakai `getBedReservationStatusLabel`; `use-inpatient-admission-transfer.js` (pisahkan `loadError` dan `submitError`, baris 51 dan 92); `inpatient-admission-view.jsx` (baris 1286-1288, judul per jenis galat); `inpatient-admission-transfer-review-step.jsx` (baris 99-102). BE: `InpatientAdmissionTransferAdmitController.cs` (baris 29) |
| **Radius dampak** | Konfirmasi dipakai semua jalur; label baru hanya menerjemahkan status yang sama |
| **Bergantung pada** | Butir 6 dibuktikan dulu (lihat issue) |

**Acceptance criteria.**

1. Konfirmasi transfer menampilkan "Status Tempat Tidur: Dipesan", bukan "Active".
2. Penolakan simpan menampilkan judul "Admisi Belum Tersimpan"; kegagalan memuat sumber tetap "Gagal Memuat Permintaan Rawat Inap".
3. TTV "…SpO2 95%." menghasilkan Catatan Disposisi kosong (baris disembunyikan), bukan ".".
4. Respons sukses `admit` bernilai HTTP 201 dengan `statusCode: 201` di badan.
5. Butir 6: bila terbukti, membuka ulang URL transfer yang persetujuannya sudah tersimpan langsung ke langkah Cetak.

**Verifikasi.** FE `npm run lint:errors`, `npm run test:unit`, `npm run build`; BE `dotnet build`; uji di browser.

**Risiko.** Konsumen yang membaca `statusCode` 200 secara harfiah. Deteksi: `rg "statusCode === 200"` pada service transfer.

### FIX-EPS-041-10 — Lanjutkan admisi bayi yang identitasnya sudah ada

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-07` |
| **Area** | Frontend + Backend + dokumen |
| **Jenis perubahan** | Source; revisi `contracts/newborn-admission-repair-022.md` dan amandemen bayi di `05-skema-tampilan.md` |
| **Berkas yang diubah** | FE: `components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` (baris 66-130); `lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` (baris 255-265). BE: `Services/InpEpisodeService.NewbornCandidates.cs` (baris 218-276: hitungan dan pratinjau urutan) |
| **Radius dampak** | Hanya Admisi Bayi; sekuens `PatNewbornBirthSequence` tidak diubah, jadi bayi baru tetap mendapat nomor berikutnya |
| **Bergantung pada** | K-03 |

**Langkah.**

1. BE: tambahkan `AdmittedBabyCount` dan `PendingBabyCount` (bayi tanpa episode aktif); `RegisteredBabyCount` tetap untuk kompatibilitas.
2. FE: tampilkan baris bayi sesuai skema 3.2 dan tombol "Lanjutkan admisi bayi ini" yang memilih pasien bayi tersebut dan melewati langkah Identitas.
3. FE: gerbang kembar hanya wajib bila petugas membuat bayi baru sementara sudah ada bayi lain; teks centang menjadi "Ini bayi yang berbeda (kelahiran kembar, Bayi ke-n)".

**Acceptance criteria.**

1. Ibu dengan satu bayi tanpa episode menampilkan "Admisi belum selesai" dan tombol "Lanjutkan admisi bayi ini".
2. Menekan tombol itu menyelesaikan admisi bayi tersebut tanpa membuat No. RM baru; jumlah pasien bayi milik ibu tetap 1.
3. Membuat bayi baru tanpa centang "Ini bayi yang berbeda" ditolak dengan pesan yang menyebut kedua pilihan.
4. Ibu tanpa bayi tidak melihat daftar maupun centang.

**Verifikasi.** BE `dotnet build`; FE `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser memakai ibu yang memiliki bayi tanpa episode.

**Risiko.** Bayi yang sebenarnya sudah pulang dianggap "belum selesai". Deteksi: status hanya "belum selesai" bila bayi tidak pernah punya episode, bukan bila episodenya *Closed*.

### FIX-EPS-041-11 — Penanganan RM bayi tanpa episode yang sudah terbentuk

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-07` (data) |
| **Area** | Data, oleh Rekam Medis |
| **Jenis perubahan** | Koreksi data lewat proses Rekam Medis yang berwenang |
| **Radius dampak** | Master pasien; modul lain yang mungkin sudah merujuk RM itu (diperiksa dulu) |
| **Bergantung pada** | K-03, P-02 |

**Acceptance criteria.**

1. Setiap RM pada hasil kueri P-02 diberi keputusan tertulis: dilanjutkan admisinya, digabung, atau dinonaktifkan.
2. Setelah koreksi, kueri P-02 hanya mengembalikan bayi yang memang masih menunggu admisi.

**Risiko.** Menonaktifkan RM yang sudah dipakai modul lain (laboratorium, farmasi). Deteksi: periksa keterkaitan sebelum menonaktifkan.

### FIX-EPS-041-12 — Catatan kecil jalur bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-08` butir 1-4 |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx` (baris 178, 196-197, 400, 444, 710, 754); `lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` (lantai awal mode neonatal) |
| **Radius dampak** | `use-floor-plan.js` dipakai empat pemakai denah; lantai awal hanya diubah untuk admisi bayi mode neonatal, tanpa mengubah penyimpanan lantai terakhir |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Picker tanggal lahir menonaktifkan tanggal setelah hari ini dan sebelum (tanggal kasus diminta − 1 hari); untuk kasus diminta 23-10-2026 pada 26-10-2026, hanya 22–26 Oktober yang aktif.
2. Konfirmasi rawat gabung menulis "KELAS I · KELAS I 330" (atau "Kelas I 330"), tanpa "Kelas KELAS".
3. Waktu lahir tampil "26 Okt 2026, 09.30 WIB" di Tinjau dan Dokumen Bayi.
4. Memilih Perinatologi atau NICU membuka denah di lantai ruang bayi.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji di browser.

**Risiko.** Batas tanggal salah zona waktu. Deteksi: uji pukul 00.30 WIB dengan jam perangkat diubah atau dengan unit test parameter `now`.

### FIX-EPS-041-13 — Pengaturan kode RS pada label dan unit layanan NICU

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-08` butir 5-6 |
| **Area** | Data/pengaturan |
| **Jenis perubahan** | Pengaturan Rawat Inap (`PatientLabelHospitalCode`) dan master kamar NICU lewat layar berwenang |
| **Bergantung pada** | K-04, P-01 |

**Acceptance criteria.** Label bayi memuat kode RS yang diputuskan (misalnya "… / BAYI 1 / RSMMC"); bila kamar NICU dipindah, ringkasan admisi NICU menulis "Unit Layanan: NICU".

**Risiko.** Pemindahan unit layanan NICU memengaruhi tarif dan laporan Billing — konfirmasi dengan pemilik Billing sebelum mengubah.

### FIX-EPS-041-14 — Format waktu peringatan admisi Draft lain

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-09` (format) |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Services/InpEpisodeService.AdmissionEligibility.cs` (baris 117-121, 136-139) |
| **Radius dampak** | Teks peringatan kelayakan di Pasien Lama, Kamar Pulih, dan (setelah `FIX-EPS-041-06`) Transfer |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.** Admisi Draft yang dibuat 26-10-2026 03:44 UTC menghasilkan peringatan "…sejak 26 Oktober 2026 10.44 WIB…" (format mengikuti `InpAdmissionText.FormatDateTime`).

**Verifikasi.** `dotnet build`; panggil `GET /episodes/admission-eligibility` untuk pasien dengan admisi Draft.

**Risiko.** Rendah.

### FIX-EPS-041-15 — Penjaga nominal deposit jalur biasa

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-10` |
| **Area** | Frontend dan Backend |
| **Jenis perubahan** | Source |
| **Berkas yang diubah** | FE: `lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx` (baris 96-110) dan langkah Deposit (pesan di bawah isian). BE: `DTOs/InpatientEpisodeDtos.cs` (baris 85, tambah `[Range]`) |
| **Radius dampak** | Langkah Deposit dipakai Pasien Baru, Pasien Lama, Kamar Pulih, dan Transfer; jalur transfer sudah punya penjaga yang sama |
| **Bergantung pada** | Batas teknis: tidak ada. Batas kebijakan: K-05 |

**Acceptance criteria.**

1. Mengetik dua puluh angka 9 menampilkan pesan "Nominal melebihi batas yang didukung" dan tombol Lanjut menahan petugas di langkah Deposit.
2. Mengetik "12,5" menampilkan petunjuk "Ketik tanpa koma, contoh 12500000 untuk Rp 12,5 juta".
3. Server menolak `requestedDepositAmount` di atas batas dengan 400 dan pesan berbahasa Indonesia.
4. Rp 5.000.000 tetap menghasilkan order deposit seperti sekarang.

**Verifikasi.** FE `npm run lint:errors`, `npm run test:unit`, `npm run build`; BE `dotnet build`; uji di browser.

**Risiko.** Deposit sah bernilai besar (misalnya paket VIP Suite) tertahan bila batas kebijakan terlalu rendah — alasan batas kebijakan menunggu K-05.

### FIX-EPS-041-16 — Perbaiki label kamar "ODC 321" ganda

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-11` |
| **Area** | Data master kamar / pemetaan denah |
| **Bergantung pada** | P-01 |

**Acceptance criteria.** Denah Lantai 3 menampilkan nama kamar ODC yang unik (misalnya 322, 321, 323, 320 sesuai denah fisik di `docs/map-bed/denah-map-ruangan-rs-mmc/lantai-3.png`).

**Risiko.** Mengubah nama kamar yang sudah tercetak di dokumen lama — nama lama tetap tercatat di snapshot.

### FIX-EPS-041-17 — Kenyamanan jalur Pasien Lama

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-12` butir 1-5 |
| **Area** | Frontend dan Backend |
| **Jenis perubahan** | Source |
| **Berkas yang diubah** | FE: `inpatient-admission-view.jsx` (baris 532, tombol Selesai pada langkah terakhir keluar langsung ke Daftar Kerja Episode bila persetujuan tersimpan); `components/features/health-services/inpatient-management/floor-plan/floor-plan-summary.jsx` (baris 14-19, keadaan memuat); teks lembar cetak "Admisi sudah dikunci…". BE: `DTOs/InpatientBedOccupancyDtos.cs` (baris 276, tambah `ReservedAt` pada item papan) dan pengisinya; `Services/InpEpisodeService.cs` (sekitar baris 545, isi `PlannedDepositAmount`) |
| **Radius dampak** | `floor-plan-summary` dipakai keempat pemakai denah (perubahan hanya menambah keadaan memuat); field papan baru bersifat tambahan |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Pada langkah Cetak Persetujuan jalur Pasien Lama, setelah persetujuan tersimpan, "Selesai" langsung membuka Daftar Kerja Episode tanpa dialog "Tinggalkan Admisi?".
2. Selama denah memuat, ringkasan menulis "Memuat denah…", bukan "Belum ada kamar pada lantai ini."
3. Setelah F5 di Booking Bed, "Dipesan Pada" tetap "26 Okt 2026, 10.45".
4. Admisi Pasien Lama dengan deposit Rp 5.000.000 menyimpan `PlannedDepositAmount = 5000000`.
5. Lembar cetak menulis "Data admisi sudah diperiksa pada langkah Konfirmasi" atau kalimat lain yang tidak menjanjikan penguncian server.

**Verifikasi.** FE `npm run lint:errors`, `npm run test:unit`, `npm run build`; BE `dotnet build`; uji di browser.

**Risiko.** "Selesai" tanpa dialog padahal persetujuan belum tersimpan. Deteksi: tombol hanya keluar langsung bila `completionAllowed` dan persetujuan tersimpan; selain itu tetap memakai penjaga keluar.

### FIX-EPS-041-18 — Konfirmasi menampilkan order deposit dan tingkat ketergantungan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-041-12` butir 6 |
| **Area** | Frontend + dokumen |
| **Jenis perubahan** | Source frontend; revisi `05-skema-tampilan.md` 3.11 |
| **Bergantung pada** | K-06 |

**Acceptance criteria.** Konfirmasi Tn. B menampilkan "Order Deposit: Rp 5.000.000 (belum dibayar)" dan "Tingkat Ketergantungan: High Dependency", baca-saja.

**Risiko.** Rendah.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-041-01   tanpa dependency — boleh dikerjakan sekarang
└── FIX-EPS-041-02
FIX-EPS-041-03   tanpa dependency — boleh dikerjakan sekarang
FIX-EPS-041-04   tanpa dependency — boleh dikerjakan sekarang
FIX-EPS-041-06   revisi Amandemen 010 lebih dulu (bagian 7), lalu source
FIX-EPS-041-08   tanpa dependency — boleh dikerjakan sekarang
FIX-EPS-041-09   tanpa dependency; butir 6 setelah dibuktikan
FIX-EPS-041-12   tanpa dependency — boleh dikerjakan sekarang
FIX-EPS-041-14   tanpa dependency — boleh dikerjakan sekarang
FIX-EPS-041-15   tanpa dependency (batas teknis); batas kebijakan setelah K-05
FIX-EPS-041-17   tanpa dependency — boleh dikerjakan sekarang
K-01 (keputusan pemilik)
└── ⛔ FIX-EPS-041-05
K-02 (keputusan pemilik)
└── ⛔ FIX-EPS-041-07
K-03 (keputusan pemilik)
├── ⛔ FIX-EPS-041-10
└── ⛔ FIX-EPS-041-11   juga menunggu P-02
K-04 + P-01
└── ⛔ FIX-EPS-041-13
P-01
└── ⛔ FIX-EPS-041-16
K-06 (keputusan pemilik)
└── ⛔ FIX-EPS-041-18
```

Saran urutan builder: `-01` → `-02` dan `-03` → `-06` → `-08` (kelompok jalur transfer, satu sesi uji ulang), lalu `-15`, `-14`, `-04`, `-09`, `-12`, `-17`.

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Bed isolasi di master data | Tetapkan bed isolasi; bila belum ada ruang tekanan negatif, tetapkan kamar tunggal sebagai isolasi sementara | `FIX-EPS-041-05` |
| K-02 | Admisi transfer yang dibatalkan | Pertahankan kontrak EPS-006/K-02 (permintaan ditutup permanen); perbaiki teks | `FIX-EPS-041-07` |
| K-03 | Bayi terdaftar tanpa episode | Lanjutkan admisi memakai pasien bayi yang ada; RM yatim lama diputuskan bersama Rekam Medis | `FIX-EPS-041-10`, `-11` |
| K-04 | Kode RS di label dan unit layanan NICU | Isi `PatientLabelHospitalCode` dengan kode singkat; NICU ke unit "NICU" bila disetujui Billing | `FIX-EPS-041-13` |
| K-05 | Batas atas nominal deposit | Batas kebijakan dari Billing; minimal batas teknis | `FIX-EPS-041-15` (bagian batas kebijakan) |
| K-06 | Konfirmasi menampilkan deposit dan ketergantungan | Tampilkan, baca-saja | `FIX-EPS-041-18` |

Pertanyaan untuk pelapor/pemilik DB: P-01 dan P-02 (kueri baca-saja) pada issue bagian 7.

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Untuk |
| --- | --- | --- | --- | --- |
| `05-skema-tampilan.md` | Amandemen PLAN-REPAIR-EPS-010 (baris 1852-1866) | "Aturan yang sama berlaku pada langkah Informasi Pasien Lama jalur Kamar Pulih" | Ditambah: "dan pada langkah Verifikasi Pasien jalur Transfer IGD & Poliklinik (tombol 'Lanjut ke Pembayaran')" | `FIX-EPS-041-06` |
| `contracts/admission-transfer-repair-006.md` | Respons antrean | Kolom respons tanpa kebutuhan IGD, DPJP usulan, diagnosis, kelas diminta | Ditambah kolom `FIX-EPS-041-01` langkah 1 | `FIX-EPS-041-01` |
| `contracts/api-contract.md` | `POST /admission-transfers/admit` | Kode galat reservasi `BED_NOT_READY` | Kode galat asli diteruskan; `statusCode` badan = 201 | `FIX-EPS-041-04`, `-09` |
| `docs/plan-repair/plan-repair-006-admisi-transfer-igd-rawat-jalan.md` | Bagian 3, skema Verifikasi | Kartu rujukan tanpa kebutuhan/diagnosis | Mengacu skema 3.1 rencana ini | `FIX-EPS-041-02` |
| `contracts/newborn-admission-repair-022.md` dan amandemen bayi di `05-skema-tampilan.md` | Langkah 1 Pilih Ibu Persalinan | Konfirmasi kembar bila sudah ada bayi terdaftar | Bayi tanpa episode dapat dilanjutkan; konfirmasi "bayi berbeda" hanya untuk membuat bayi baru | `FIX-EPS-041-10` |
| `05-skema-tampilan.md` | 3.11 Konfirmasi | Tanpa deposit dan ketergantungan | Ditambah dua baris baca-saja | `FIX-EPS-041-18` (bila K-06 disetujui) |

Revisi dokumen dikerjakan lewat jalur blueprint yang berwenang, bukan oleh `diagnose-module-issue`.

---

## 8. Verifikasi menyeluruh

Jalankan setelah seluruh perbaikan yang disetujui selesai, memakai live browser testing (Chromium headed) pada DB pengembang:

| No | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Transfer IGD isolasi baru (data uji dibuat ulang dengan `admisi-1026-seed-transfer.py`) | Chip "Isolasi" di Verifikasi; sakelar tercentang; denah hanya mengaktifkan bed isolasi; pemesanan berhasil di bed isolasi |
| 2 | Permintaan Poli untuk pasien yang sedang dirawat (`SPRI/2026/10/AGY04`) | Diblokir di langkah 2 dengan alert dan tombol Pindah Tempat Tidur; 0 POST |
| 3 | Transfer IGD CITO dengan konsulen dan High Dependency | Konsulen dan ketergantungan tersimpan dan tampil di Konfirmasi; DPJP terisi dari DPJP usulan |
| 4 | Batalkan admisi transfer | Dialog menyebut permintaan tidak kembali ke antrean (K-02 a) |
| 5 | Ibu dengan bayi tanpa episode | "Lanjutkan admisi bayi ini" menyelesaikan admisi tanpa RM baru |
| 6 | Deposit dua puluh angka 9 di Pasien Lama | Ditahan di langkah Deposit dengan pesan |
| 7 | Regresi jalur Pasien Lama, Bayi (Perinatologi/NICU), dan Papan Tempat Tidur | Hasil sama seperti uji 26-10-2026 |
| 8 | Jaringan | 0 respons 5xx, 0 error JavaScript |

Hasilnya ditulis sebagai laporan live testing baru per menu di `testing/live-testing/`, lalu register bagian 1 diperbarui oleh builder.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-26 | Rencana dibuat: 18 perbaikan (11 siap dikerjakan, 7 menunggu K-01 s.d. K-06 atau P-01/P-02); menunggu persetujuan pemilik | `diagnose-module-issue` |
