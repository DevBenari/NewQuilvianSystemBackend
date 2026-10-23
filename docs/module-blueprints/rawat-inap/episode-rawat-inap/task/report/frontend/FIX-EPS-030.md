# Laporan Perubahan Frontend — `FIX-EPS-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Penyiapan Denah Mandiri Admisi Bayi, Selektivitas Kamar Neonatal Terpisah, dan Validasi Kamar Ibu Tertutup |
| Rencana Perbaikan | `PLAN-REPAIR-030` |
| Issue Terkait | `ISSUE-030` (Denah Bed Admisi Bayi Belum Muncul dan Validasi Kamar Ibu Closed) |
| Tanggal | 21 Oktober 2026 |
| Status | **Selesai Source & Lulus Uji** |
| Basis Git Frontend | `HamzahV2` (`381ec25fa`) |
| Basis Git Backend | `MHamzah` (`6406fc76`) |

---

## Ringkasan Perubahan

Perbaikan ini menyelesaikan masalah kegagalan tampilan denah tempat tidur pada alur pendaftaran admisi bayi baru lahir (`entry=newborn`), di mana pada Langkah 4 ("Penempatan dan Pemesanan") layar sebelumnya terblokir oleh galat merah *"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif"*, serta menegakkan aturan bisnis selektivitas kamar bayi untuk mode Neonatal Terpisah dan validasi penempatan kamar ibu tertutup (*closed*) untuk mode Rawat Gabung.

Sesuai persetujuan Pemilik Sistem atas rencana perbaikan `PLAN-REPAIR-030`, telah diimplementasikan 4 solusi:

1. **Pemuatan Denah Mandiri Tanpa Draf Prematur (`FIX-EPS-030-01`)**:
   - Menghapus ketergantungan draf buatan pada `use-inpatient-admission-bed.jsx`.
   - Menetapkan konstanta `isNewbornAdmission = Boolean(newbornContext && !episodeId)`.
   - Mengarahkan `useFloorPlan` untuk menggunakan `purpose: ""` (master data layout umum tanpa kewajiban episode) saat `isNewbornAdmission` bernilai benar.
   - Melewati (*bypass*) fungsi `prepareFloorPlanDraft` yang sebelumnya memicu galat merah ketika belum ada tempat tidur yang dipilih.
   - Menunda komitmen pembuatan episode draf (`admission.prepare(selectedBed)`) hingga petugas benar-benar memilih tempat tidur pada denah dan menekan tombol **"Lanjut ke Pemesanan"**.

2. **Adaptor Selektivitas Denah Mode Neonatal Terpisah (`FIX-EPS-030-02`)**:
   - Memperluas `adaptedFloorPlanData` pada `use-inpatient-admission-bed.jsx` untuk menangani `isNewbornAdmission`.
   - Pada mode **Neonatal Terpisah** (`newbornCareMode === "Neonatal"`):
     - Seluruh ruangan dan kamar di lantai rumah sakit **tetap dirender utuh** pada peta SVG layout gedung (*full floor plan preserved*).
     - **Hanya kamar bayi/perinatologi dan tempat tidur khusus bayi (`isForNewborn === true`) yang ditandai `isSelectable = true`**.
     - Memeriksa syarat perawatan intensif neonatal (`requiresIntensiveCare`): bed wajib memiliki kapabilitas intensif (`isIntensiveCareBed` atau `room.isIntensiveCare`). Jika opsi intensif aktif namun rumah sakit hanya memiliki kamar Perinatologi reguler (seperti `BAYI-1` dan `BAYI-2`), antarmuka menyajikan `InformationAlert` penjelas beserta tombol aksi instan `[ Ubah ke Perawatan Perinatologi / Non-Intensif ]` agar petugas dapat langsung memilih bed tanpa terjebak pemblokiran.
     - Kamar non-bayi (kamar rawat inap dewasa umum) ditandai `isSelectable = false` dan diberi penanda kelayakan: *"Tempat tidur ini bukan kamar/boks khusus bayi."*. Kamar tersebut tetap terlihat secara visual dengan warna netral namun tidak dapat diklik.

3. **Penjagaan Kamar Ibu Berstatus Closed pada Rawat Gabung (`FIX-EPS-030-03`)**:
   - Di Langkah 3 (`NewbornCareStep` dan `use-inpatient-newborn-admission.js`):
     - Memperbarui fungsi `validateCare`: bila petugas memilih "Rawat gabung dengan ibu" dan ibu tidak memiliki penempatan kamar aktif (`!selectedCandidate?.currentRoomId`), sistem menampilkan peringatan edukatif dan menolak validasi dengan pesan: *"Kamar penempatan ibu sudah berstatus closed / tidak aktif. Rawat gabung tidak dapat dipilih."*.
     - Menambahkan banner peringatan `InformationAlert variant="warning"` pada antarmuka Langkah 3 jika kamar penempatan ibu terdeteksi closed.
   - Di Langkah 4 (`NewbornPlacementStep` dan `adaptedFloorPlanData`):
     - Memeriksa kesesuaian kamar ibu (`motherRoomId`). Jika status kamar ibu *closed* atau tidak aktif, seluruh bed ditandai `isSelectable = false` dengan kode `MOTHER_ROOM_CLOSED`.
     - Menyediakan tombol bantuan langsung pada antarmuka penempatan: `[ Ubah ke Mode Neonatal Terpisah ]` yang secara instan mengalihkan rencana perawatan ke Neonatal Terpisah dan memuat kamar perinatologi.

4. **Sinkronisasi Parameter Pencarian Bed dan Denah (`FIX-EPS-030-04`)**:
   - Menyinkronkan mapping DTO kelayakan tempat tidur bayi antara `legacyBoard.availableBeds`, `legacyBoard.ineligibleBeds`, dan `effectiveFloorPlan`.
   - Memastikan `canContinueToBooking` menjadi aktif segera setelah petugas memilih satu boks bayi yang memenuhi syarat.

---

## Berkas yang Diubah

1. `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`:
   - Menambahkan deteksi `isNewbornAdmission`.
   - Menghubungkan `legacyBoard` dan `useFloorPlan` agar aktif secara mandiri tanpa menunggu episode draf.
   - Memperluas `adaptedFloorPlanData` dengan logika selektivitas `isNeonatalSeparate` dan `isRoomingIn`.
   - Mengalihkan `effectiveFloorPlan` dan `board` untuk menggunakan denah teradaptasi pada mode admisi bayi.
   - Mencegah eksekusi `prepareFloorPlanDraft` yang menyebabkan galat merah.
2. `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`:
   - Meneruskan prop `newborn={newborn}` ke `NewbornPlacementStep`.
   - Memperbarui objek `newbornContext` dengan properti `roomId` dan `isMotherRoomClosed`.
3. `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`:
   - Memperbarui `validateCare` agar memeriksa ketersediaan `selectedCandidate.currentRoomId` pada opsi rawat gabung.
4. `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`:
   - Menambahkan peringatan kamar ibu closed di Langkah 3 (`NewbornCareStep`).
   - Menambahkan peringatan dan tombol alih `[ Ubah ke Mode Neonatal Terpisah ]` di Langkah 4 (`NewbornPlacementStep`).
5. `tests/unit/inpatient-newborn-admission.test.mjs`:
   - Menambahkan unit test otomasi untuk validasi deteksi kamar ibu closed dan selektivitas kamar bayi pada hook bed.

---

## Bukti dan Verifikasi

### 1. Automated Unit Tests
Perintah:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-newborn-admission.test.mjs
```
Hasil:
```text
✔ FE-RWI-257 / FIX-EPS-028: opsi kartu Admisi Bayi Baru Lahir aktif pada menu utama (0.8733ms)
✔ FE-RWI-257: kandidat ibu memakai endpoint kasus persalinan (0.1018ms)
✔ FE-RWI-257 & FIX-EPS-029: alur bayi tepat tujuh langkah tanpa penjamin dan deposit pada mode newborn (0.7669ms)
✔ FE-RWI-258: identitas bayi mewajibkan kasus, tanggal, jam, dan jenis kelamin (16.7219ms)
✔ FE-RWI-258: payload bayi tidak mengirim nama atau urutan manual (0.9351ms)
✔ FE-RWI-259: konteks episode tidak membawa keputusan invoice/deposit (0.1681ms)
✔ FE-RWI-259: rawat gabung wajib episode ibu dan semua perawatan wajib DPJP (0.1701ms)
✔ FE-RWI-259: query bed mempertahankan konteks bayi sebelum episode dibuat (0.2058ms)
✔ FE-RWI-259: tampilan bayi tidak menyediakan input invoice/deposit (1.0745ms)
✔ FIX-EPS-030: validasi rawat gabung mendeteksi kamar penempatan ibu closed (0.899ms)
✔ FIX-EPS-030: hook bed mendukung pemuatan denah mandiri dan selektivitas kamar bayi (0.654ms)
ℹ tests 11
ℹ suites 0
ℹ pass 11
ℹ fail 0
```
Status: **LULUS (11/11 PASS 100%)**.

### 2. Linter CI/CD
Perintah:
```bash
npx eslint src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js --quiet
```
Hasil: **LULUS BERSIH (0 errors, 0 warnings, Exit Code 0)**.
