# Laporan Perubahan Frontend & Database — `FIX-EPS-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Penerapan 3 Kartu Pilihan Perawatan Bayi (Rawat Gabung, Perinatologi, NICU), Konfigurasi Boks Kamar VIP Lantai 3, Pembersihan Data Uji Coba, dan Pendaftaran Unit NICU |
| Rencana Perbaikan | `PLAN-REPAIR-031` |
| Issue Terkait | `ISSUE-031` (Penerapan 3 Kartu Pilihan Perawatan Bayi, Konfigurasi Boks Kamar VIP Lantai 3, dan Perencanaan Unit NICU) |
| Tanggal | 22 Oktober 2026 |
| Status | **Selesai Source, Migrasi DB & Lulus Uji (13/13)** |
| Basis Git Frontend | `HamzahV2` (`381ec25fa`) |
| Basis Git Backend | `MHamzah` (`6406fc76`) |
| Target Database | `QuilvianNewDevHamzah` (PostgreSQL 15.15) |

---

## Ringkasan Perubahan

Perbaikan ini mengimplementasikan kesepakatan desain alur pendaftaran admisi bayi baru lahir (`entry=newborn`) yang disetujui Pemilik Sistem pada 21-22 Oktober 2026:
1. Mengganti model pilihan 2 radio lama ("Rawat gabung dengan ibu" dan "Neonatal terpisah") serta checkbox intensif tersembunyi menjadi **3 kartu pilihan langsung yang jelas dan mandiri** pada Langkah 3 ("Perawatan dan DPJP Bayi"):
   - **Kartu 1: Rawat Gabung dengan Ibu** (`careMode: "RoomingIn"`, non-intensif)
   - **Kartu 2: Ruang Perinatologi** (`careMode: "Neonatal"`, non-intensif)
   - **Kartu 3: Ruang NICU** (`careMode: "Neonatal"`, intensif kritis)
2. **Pembersihan Data Penempatan Uji Coba (*Safe State Reset*)**: Menutup seluruh penempatan uji coba yang aktif, membatalkan reservasi sementara sisa pengujian, menyelesaikan siklus kebersihan, dan mengembalikan status fisik seluruh 8 tempat tidur uji di Lantai 2 dan Lantai 3 menjadi `Available` (1).
3. **Konfigurasi Boks Bayi Kamar VIP 301 s.d. 305 Lantai 3**: Mendaftarkan 1 boks bayi resmi (`BedNumber = 'BOKS'`, `IsForNewborn = true`) pada masing-masing kamar privat `VIP 301` sampai `VIP 305` di Lantai 3 (`BD-RSMMC-BOKS-301` s.d. `305`) dan menaikkan kapasitas kamar menjadi 2 (1 ranjang ibu + 1 boks bayi).
4. **Pendaftaran Kamar & Inkubator Ruang NICU Lantai 2**: Mendaftarkan 1 kamar fisik percontohan `Ruang NICU` (`RM-RSMMC-NICU01`, nomor `NICU-1`, kelas `NICU` `CLS-149`, `IsIntensiveCare = true`) dengan 2 inkubator intensif (`BD-RSMMC-NICU-01`, `BD-RSMMC-NICU-02`) berstatus `IsForNewborn = true` dan `IsIntensiveCareBed = true`.

---

## Rincian Perubahan per Komponen

### 1. Frontend (`QuilvianSystemFrontendDev`)

1. **`src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js`**:
   - Menambahkan konstanta `NEWBORN_CARE_CARD_OPTIONS` yang memuat definisi 3 kartu pilihan langsung:
     - `RoomingIn`: judul "Rawat Gabung dengan Ibu", badge "Kamar Ibu (VIP)", `careMode: "RoomingIn"`, `requiresIntensiveCare: false`.
     - `Perinatology`: judul "Ruang Perinatologi", badge "Kamar Bayi (Lt. 2)", `careMode: "Neonatal"`, `requiresIntensiveCare: false`.
     - `NICU`: judul "Ruang NICU", badge "Unit Intensif (Lt. 2)", `careMode: "Neonatal"`, `requiresIntensiveCare: true`.
   - Tetap mempertahankan `NEWBORN_CARE_OPTIONS` untuk kompatibilitas ke belakang.

2. **`src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`**:
   - Di `NewbornCareStep`:
     - Mengimpor `NEWBORN_CARE_CARD_OPTIONS`.
     - Mengganti komponen `<Choices>` radio dan kotak centang `<BaseCheckboxField name="requiresIntensiveCare">` dengan visual 3 kartu `BaseCheckboxCard` di dalam kontainer `.patientTypeGrid`.
     - Mengaitkan pemilihan kartu agar secara otomatis memetakan `careMode` dan `requiresIntensiveCare` tanpa langkah tersembunyi.
     - Menghitung kartu aktif melalui `activeCareKey`.
   - Di `NewbornPlacementStep`:
     - Menyesuaikan banner peringatan jika mode NICU aktif menjadi banner informatif bahwa denah akan memfasilitasi inkubator intensif di Ruang NICU Lantai 2.

3. **`tests/unit/inpatient-newborn-admission.test.mjs`**:
   - Menambahkan 2 pengujian unit otomatis untuk `FIX-EPS-031`:
     - Memverifikasi keberadaan 3 opsi kartu dan integritas atribut mapping (`careMode` dan `requiresIntensiveCare`).
     - Memverifikasi bahwa komponen `NewbornCareStep` merender 3 kartu dan meniadakan checkbox tersembunyi.
   - Seluruh 13 pengujian unit lulus (13/13).

---

### 2. Database (`QuilvianNewDevHamzah`)

1. **Eksekusi Pembersihan Data Uji Coba (`FIX-EPS-031-02`)**:
   - `InpBedPlacement`: 8 baris penempatan aktif ditutup (`EndDateTime = NOW()`).
   - `InpBedReservation`: 7 baris reservasi dibatalkan (`ReservationStatus = 3`).
   - `InpBedCleaningAttempt`: 2 baris siklus pembersihan diselesaikan (`State = 3`).
   - `InpBedReadiness`: 9 baris siklus kesiapan tempat tidur direset ke Ready (`State = 4`).
   - `MstBed`: 8 tempat tidur master dikembalikan ke status `Available` (1): `326-A`, `321-D`, `325-A`, `509-A`, `327-A`, `304-A`, `ICU-A`, `319-A`.

2. **Eksekusi Penambahan Boks Bayi Kamar VIP Lantai 3 (`FIX-EPS-031-03`)**:
   - `MstRoom`: Kapasitas 5 kamar VIP (`VIP 301` s.d. `VIP 305`) dinaikkan dari 1 menjadi 2.
   - `MstBed`: 5 boks bayi resmi didaftarkan:
     - `BD-RSMMC-BOKS-301` (`301-BOKS`, Room VIP 301, BedStatus: 1, IsForNewborn: true)
     - `BD-RSMMC-BOKS-302` (`302-BOKS`, Room VIP 302, BedStatus: 1, IsForNewborn: true)
     - `BD-RSMMC-BOKS-303` (`303-BOKS`, Room VIP 303, BedStatus: 1, IsForNewborn: true)
     - `BD-RSMMC-BOKS-304` (`304-BOKS`, Room VIP 304, BedStatus: 1, IsForNewborn: true)
     - `BD-RSMMC-BOKS-305` (`305-BOKS`, Room VIP 305, BedStatus: 1, IsForNewborn: true)

3. **Eksekusi Pendaftaran Kamar Fisik & Inkubator NICU Lantai 2 (`FIX-EPS-031-04`)**:
   - `MstRoom`: Kamar fisik `Ruang NICU` (`RM-RSMMC-NICU01`, nomor `NICU-1`, lantai 2, kapasitas 2, `IsForNewborn = true`, `IsIntensiveCare = true`, kelas `NICU` `CLS-149`) terdaftar.
   - `MstBed`: 2 tempat tidur inkubator intensif didaftarkan:
     - `BD-RSMMC-NICU-01` (`NICU-1-A`, BedStatus: 1, IsForNewborn: true, IsIntensiveCareBed: true)
     - `BD-RSMMC-NICU-02` (`NICU-1-B`, BedStatus: 1, IsForNewborn: true, IsIntensiveCareBed: true)

---

## Bukti Verifikasi

### 1. Bukti Pengujian Unit Otomatis
```text
✔ FE-RWI-257 / FIX-EPS-028: opsi kartu Admisi Bayi Baru Lahir aktif pada menu utama (0.852ms)
✔ FE-RWI-257: kandidat ibu memakai endpoint kasus persalinan (0.0982ms)
✔ FE-RWI-257 & FIX-EPS-029: alur bayi tepat tujuh langkah tanpa penjamin dan deposit pada mode newborn (0.7658ms)
✔ FE-RWI-258: identitas bayi mewajibkan kasus, tanggal, jam, dan jenis kelamin (17.1081ms)
✔ FE-RWI-258: payload bayi tidak mengirim nama atau urutan manual (0.3016ms)
✔ FE-RWI-259: konteks episode tidak membawa keputusan invoice/deposit (0.1697ms)
✔ FE-RWI-259: rawat gabung wajib episode ibu dan semua perawatan wajib DPJP (0.1769ms)
✔ FE-RWI-259: query bed mempertahankan konteks bayi sebelum episode dibuat (0.2344ms)
✔ FE-RWI-259: tampilan bayi tidak menyediakan input invoice/deposit (1.0758ms)
✔ FIX-EPS-030: validasi rawat gabung mendeteksi kamar penempatan ibu closed (0.9327ms)
✔ FIX-EPS-030: hook bed mendukung pemuatan denah mandiri dan selektivitas kamar bayi (0.285ms)
✔ FIX-EPS-031: kartu pilihan jenis perawatan bayi menyediakan 3 opsi eksplisit (Rawat Gabung, Perinatologi, NICU) (0.1402ms)
✔ FIX-EPS-031: komponen NewbornCareStep merender 3 kartu dan meniadakan centang intensif tersembunyi (0.6879ms)
ℹ tests 13
ℹ suites 0
ℹ pass 13
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 288.4918
```

### 2. Bukti Integritas Database `QuilvianNewDevHamzah`
```text
=== VERIFIKASI BOKS KAMAR VIP 301 - 305 ===
Room: VIP 301 (2) | Bed: 301-A | BedCode: BD-RSMMC-00062 | BedNumber: A | BedStatus: 1 | IsNewborn: false
Room: VIP 301 (2) | Bed: 301-BOKS | BedCode: BD-RSMMC-BOKS-301 | BedNumber: BOKS | BedStatus: 1 | IsNewborn: true
Room: VIP 302 (2) | Bed: 302-A | BedCode: BD-RSMMC-00061 | BedNumber: A | BedStatus: 1 | IsNewborn: false
Room: VIP 302 (2) | Bed: 302-BOKS | BedCode: BD-RSMMC-BOKS-302 | BedNumber: BOKS | BedStatus: 1 | IsNewborn: true
Room: VIP 303 (2) | Bed: 303-A | BedCode: BD-RSMMC-00060 | BedNumber: A | BedStatus: 1 | IsNewborn: false
Room: VIP 303 (2) | Bed: 303-BOKS | BedCode: BD-RSMMC-BOKS-303 | BedNumber: BOKS | BedStatus: 1 | IsNewborn: true
Room: VIP 304 (2) | Bed: 304-A | BedCode: BD-RSMMC-00059 | BedNumber: A | BedStatus: 1 | IsNewborn: false
Room: VIP 304 (2) | Bed: 304-BOKS | BedCode: BD-RSMMC-BOKS-304 | BedNumber: BOKS | BedStatus: 1 | IsNewborn: true
Room: VIP 305 (2) | Bed: 305-A | BedCode: BD-RSMMC-00058 | BedNumber: A | BedStatus: 1 | IsNewborn: false
Room: VIP 305 (2) | Bed: 305-BOKS | BedCode: BD-RSMMC-BOKS-305 | BedNumber: BOKS | BedStatus: 1 | IsNewborn: true

=== VERIFIKASI RUANG NICU LANTAI 2 ===
Room: Ruang NICU (NICU-1) | Floor: 2 | Cap: 2 | Bed: NICU-1-A (BD-RSMMC-NICU-01) | BedStatus: 1 | IsNewborn: true | IsIntensive: true
Room: Ruang NICU (NICU-1) | Floor: 2 | Cap: 2 | Bed: NICU-1-B (BD-RSMMC-NICU-02) | BedStatus: 1 | IsNewborn: true | IsIntensive: true

=== VERIFIKASI SISA BED NON-AVAILABLE ===
Total bed dengan status selain Available (1): 0
Total penempatan aktif: 0
```
