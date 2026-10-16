# ISSUE-014 — Layar Admisi Rawat Inap Ter-reset dan Mengunci (Freeze) Setelah Ditinggal Idle Beberapa Menit

```yaml
issue_id: ISSUE-EPS-014
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap (FE-INP-03) — /health-services/inpatient-management/admissions"
sumber_laporan: "Laporan pengguna: Ketika layar admisi ditinggal lama (sekitar beberapa menit), tiba-tiba layar seperti fresh tidak bisa diklik lanjut step atau kembali dan lain-lain, kecuali menu items"
tanggal_issue: "2026-10-14"
status: TERSELESAIKAN
keparahan_tertinggi: High
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-014-layar-admisi-freeze-reset-idle.md
```

## 1. Ringkasan

Pada modul Rawat Inap menu **Admisi Rawat Inap (`/health-services/inpatient-management/admissions`)**, petugas admisi menjalankan alur pendaftaran rawat inap multi-tahap (pemilihan pasien, penetapan jenis pasien/kunjungan, penjaminan, dokter penanggung jawab, hingga alokasi dan reservasi tempat tidur).

Berdasarkan laporan pengguna, ketika layar admisi sedang terbuka lalu ditinggal diam (*idle*) selama beberapa menit (misalnya petugas sedang memverifikasi berkas fisik pasien, menerima telepon konfirmasi keluarga, atau mendatangi dokter IGD), tampilan admisi mendadak **"seperti fresh" dan terkunci (*freeze*)**. Pengguna tidak dapat mengeklik tombol *"Lanjut Step"*, tombol *"Kembali"*, maupun tombol aksi form lainnya. Namun anehnya, menu navigasi pada sidebar sistem tetap dapat diklik secara normal.

Melalui penelusuran mendalam terhadap kode sumber frontend, ditemukan 3 (tiga) akar penyebab teknis:
1. **Pemicu Utama (Bed Reservation Timeout & Silent Back-Redirect):** Pada Langkah 6 (Pilih Bed) dan Langkah 7 (Pemesanan Bed), hook `useInpatientAdmissionBed` memiliki *timer countdown* reservasi sementara (10 menit). Ketika waktu habis saat layar ditinggal, fungsi `releaseExpiredReservation()` otomatis membatalkan reservasi di server, mengosongkan tempat tidur terpilih (`clearSelection()`), lalu menaikkan token pemicu `returnToSelectionToken`. Komponen `InpatientAdmissionBedStep` merespons token tersebut dengan memanggil `onBack()` secara diam-diam. Akibatnya, form terpental mundur ke langkah pemilihan tempat tidur dengan kondisi pilihan tempat tidur kosong, sehingga tombol *"Lanjut ke Pemesanan"* berstatus `disabled` (terkunci mati). Bagi petugas yang baru kembali ke meja, layar tampak seperti ter-*reset* (*fresh*) dan macet (*freeze*).
2. **Ketiadaan Persistensi State Flow Utama:** State krusial pada hook `useInpatientAdmissionFlow` (`patientType`, `visitType`, `referralData`) disimpan semata-mata di dalam `useState` memori React tanpa disinkronkan ke URL maupun `sessionStorage`. Jika peramban melakukan *tab hibernation / memory discarding* saat ditinggal lama atau terjadi re-render siklus sesi, data jenis pasien hilang kembali ke string kosong `""`. Hal ini menyebabkan validasi form terkunci (`canContinuePatientType = false`) dan tombol *"Lanjut"* mati tanpa ada pesan kesalahan.
3. **Potensi Perangkap Tumpukan Modal Backdrop (Ghost Backdrop):** Komponen `InpatientAdmissionView` menampung tiga modal konfirmasi berbasis Bootstrap (`ConfirmModal`). Pada kondisi pelepasan fokus atau event DOM terputus saat *idle*, elemen penutup latar belakang (`.modal-backdrop` dengan z-index 1050) dapat tertinggal di atas `#content-page`, memblokir interaksi klik form sementara bilah navigasi utama (`.iq-sidebar`) berada pada konteks tumpukan terpisah sehingga tetap bisa diklik.

Masalah ini diklasifikasikan sebagai **High** karena memotong alur registrasi rawat inap darurat, menghapus data yang telah diinput petugas, dan menimbulkan kebingungan operasional di loket admisi rumah sakit.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "pada admisi rawat inap. Ketika layar admisi di tinggal lama , sekitar beberapa menit , tiba tiba layar seperti fresh tidak bisa di klik klik lanjut step atau kembali dan lain lain , kecuali menu items" | — (Laporan operasional langsung dari pengguna) |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-014-01` | 1 | Kedaluwarsa Reservasi Bed (Countdown Timeout) Memicu Pemindahan Balik Otomatis yang Mengosongkan Pilihan Bed Tanpa Dialog/Indikasi Jelas sehingga Tombol Terkunci (*Freeze/Disabled*) | `BUG` | Frontend | High | SUDAH-VERIFIKASI di `use-inpatient-admission-bed.jsx:203-220` dan `inpatient-admission-bed-step.jsx:215-222` | `FIX-EPS-014-01` |
| `ISS-EPS-014-02` | 1 | Ketiadaan Persistensi State Alur Admisi (`patientType`, `visitType`, `referralData`) Menyebabkan Nilai Isian Reset Kosong (*Fresh*) Saat Tab Idle/Hibernasi Peramban | `BUG` | Frontend | High | SUDAH-VERIFIKASI di `use-inpatient-admission-flow.jsx:25-34` dan `inpatient-admission-patient-type-step.jsx:29-37` | `FIX-EPS-014-02` |
| `ISS-EPS-014-03` | 1 | Kerentanan Elemen Penutup Modal (*Ghost Backdrop Trap*) Menutupi Area Konten Admisi Tanpa Memblokir Menu Navigasi Sidebar | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI di `inpatient-admission-view.jsx:1146-1232` dan `confirm-modal.jsx:45-70` | `FIX-EPS-014-03` |

---

## 4. Rincian per temuan

### ISS-EPS-014-01 — Kedaluwarsa Reservasi Bed (Countdown Timeout) Memicu Pemindahan Balik Otomatis yang Mengosongkan Pilihan Bed Tanpa Dialog/Indikasi Jelas sehingga Tombol Terkunci (*Freeze/Disabled*)

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source `use-inpatient-admission-bed.jsx:203-220` & `inpatient-admission-bed-step.jsx:215-222` |
| **Perbaikan** | `FIX-EPS-014-01` |

**Apa yang terjadi.** Petugas admisi telah memilih tempat tidur di Langkah 6 (Pilih Bed) atau sedang berada di Langkah 7 (Pemesanan/Reservasi Bed). Ketika petugas meninggalkan layar selama beberapa menit (melewati batas waktu timer reservasi), layar admisi mendadak berpindah sendiri ke langkah pemilihan tempat tidur dengan seluruh pilihan tempat tidur dibersihkan. Tombol *"Lanjut ke Pemesanan"* dan tombol *"Lanjut"* menjadi berwarna abu-abu redup (*disabled*) dan tidak merespons klik. Tidak ada jendela pemberitahuan (*modal dialog*) maupun pesan peringatan mencolok yang menjelaskan bahwa batas waktu reservasi telah habis dan pengguna harus memilih ulang tempat tidur.

**Kenapa terjadi.**
1. Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` baris 203–220:
   ```javascript
   useEffect(() => {
     if (!expiresAt) return undefined;
     const timer = setInterval(() => {
       const now = Date.now();
       setTickNow(now);
       if (getReservationRemainingMs(expiresAt, now) <= 0) {
         void releaseExpiredReservation();
       }
     }, COUNTDOWN_INTERVAL_MS);
     return () => clearInterval(timer);
   }, [expiresAt, releaseExpiredReservation]);
   ```
2. Saat sisa waktu habis (`<= 0`), fungsi `releaseExpiredReservation()` dipanggil:
   ```javascript
   // use-inpatient-admission-bed.jsx:188-197
   const releaseExpiredReservation = useCallback(async () => {
     if (isExpiredRef.current || !activeReservation?.bookingCode) return;
     isExpiredRef.current = true;
     clearSelection(); // Mengosongkan selectedBedId dan data kamar
     setReturnToSelectionToken((prev) => prev + 1); // Memperbarui token balik
     ...
   }, [activeReservation, clearSelection, ...]);
   ```
3. Komponen `inpatient-admission-bed-step.jsx` (baris 215–222) memantau token tersebut:
   ```javascript
   useEffect(() => {
     if (!bed.returnToSelectionToken) return;
     onBack?.(); // Memaksa step form kembali ke langkah sebelumnya secara silent!
   }, [bed.returnToSelectionToken, onBack]);
   ```
4. Di saat yang sama, karena `clearSelection()` telah dijalankan, variabel `canContinueToBooking` menjadi `false`:
   ```javascript
   // inpatient-admission-bed-step.jsx:119-122
   const canContinueToBooking = Boolean(
     selectedBedId && selectedRoomId && ...
   );
   ```
5. Tombol *"Lanjut ke Pemesanan"* dirender dengan kondisi:
   ```javascript
   // inpatient-admission-bed-step.jsx:597-603
   <BaseButton
     variant="primary"
     disabled={!canContinueToBooking || bed.loading}
     onClick={handleContinueToBooking}
   >
     Lanjut ke Pemesanan
   </BaseButton>
   ```
6. Akibatnya, saat pengguna kembali ke layar komputer setelah beberapa menit:
   - Layar tampak berpindah sendiri (*auto-jump/fresh*).
   - Tempat tidur yang tadi dipilih telah hilang (tidak ada bed yang terpilih).
   - Tombol *"Lanjut"* terkunci mati (*disabled*) dan tidak bisa diklik.
   - Pengguna mengira aplikasi rusak/macet total (*freeze*), padahal penyebabnya adalah mekanisme pembersihan diam-diam (*silent rollback*).

**Apakah ini menyimpang dari desain?** Ya, ini adalah `BUG` fatal pada pengalaman pengguna (*UX antipattern*). Reservasi tempat tidur memang memiliki masa kedaluwarsa demi mencegah pemblokiran bed di rumah sakit, namun sistem TIDAK BOLEH memulangkan layar ke langkah sebelumnya secara diam-diam dan mematikan tombol tanpa menampilkan dialog penjelasan yang ramah (misal: *"Waktu penahanan tempat tidur telah habis. Silakan pilih kembali tempat tidur yang tersedia"*).

**Dampak nyata.** Petugas rawat inap di loket bingung, merasa sistem *hang*, dan terpaksa memuat ulang seluruh halaman dari awal, yang berpotensi membatalkan seluruh data pasien yang sudah diisikan pada tahap awal.

**Rekomendasi.**
1. Hapus efek pemindahan otomatis yang diam-diam (`onBack()` tanpa dialog).
2. Tampilkan modal dialog kedaluwarsa (*Reservation Expired Dialog*) yang jelas ketika waktu habis: menginformasikan bahwa alokasi sementara tempat tidur telah dilepaskan kembali ke publik dan memberikan tombol *"Pilih Ulang Tempat Tidur"*.
3. Jika modal dikonfirmasi, barulah pindahkan pengguna ke langkah pemilihan tempat tidur secara elegan dengan status pesan informatif di atas tabel denah bed.

---

### ISS-EPS-014-02 — Ketiadaan Persistensi State Alur Admisi (`patientType`, `visitType`, `referralData`) Menyebabkan Nilai Isian Reset Kosong (*Fresh*) Saat Tab Idle/Hibernasi Peramban

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source `use-inpatient-admission-flow.jsx:25-34` & `inpatient-admission-patient-type-step.jsx:29-37` |
| **Perbaikan** | `FIX-EPS-014-02` |

**Apa yang terjadi.** Petugas admisi membuka halaman admisi, memilih jenis pasien (misal Pasien Rujukan / Non-Rujukan), lalu meninggalkan tab peramban selama beberapa menit untuk mengambil berkas fisik. Ketika peramban modern (seperti Google Chrome atau Microsoft Edge dengan fitur *Memory Saver / Tab Discarding*) memulihkan tab yang lama tidak aktif, atau terjadi pembaharuan token sesi di latar belakang, nilai formulir `patientType` mendadak hilang kembali ke status awal (string kosong `""`). Di layar tampak form kosong seperti baru dibuka pertama kali (*fresh*), dan tombol *"Lanjut"* terkunci mati tanpa pesan kesalahan apa pun.

**Kenapa terjadi.**
1. Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` baris 25–34:
   ```javascript
   const [patientType, setPatientType] = useState(initialPatientType);
   const [visitType, setVisitType] = useState(initialVisitType);
   const [referralData, setReferralData] = useState({ ... });
   ```
2. State alur admisi ini sepenuhnya disimpan dalam memori lokal komponen (*React in-memory state*) dan sama sekali **tidak disinkronkan ke URL query parameter maupun `sessionStorage`**.
3. Di sisi lain, URL hanya menyimpan parameter minimal:
   `?entry=new&step=patient-type&patientId=...&episodeId=...`
4. Ketika tab ditinggal beberapa menit dan peramban melakukan *soft refresh / reload from memory cache*, komponen di-mount ulang dengan nilai awal default (`""`).
5. Pada `inpatient-admission-patient-type-step.jsx` baris 29–37:
   ```javascript
   const canContinue = Boolean(
     flow.patientType && (!flow.isNewborn || flow.motherEpisodeId)
   );
   ...
   <BaseButton
     variant="primary"
     disabled={!canContinue}
     onClick={handleContinue}
   >
     Lanjut
   </BaseButton>
   ```
6. Karena `patientType` telah kembali menjadi `""`, `canContinue` bernilai `false`. Tombol *"Lanjut"* menjadi non-aktif (*disabled*), tombol radio pilihan jenis pasien tampak tidak terpilih, dan pengguna merasa layar admisi *freeze* serta kehilangan isian sebelumnya.

**Apakah ini menyimpang dari desain?** Ya, alur pendaftaran pasien rawat inap merupakan proses multi-langkah (*wizard*) yang berdurasi panjang. Sesuai prinsip arsitektur Quilvian, alur multi-langkah wajib memiliki mekanisme pemulihan status (*state recovery/hydration*) yang tangguh terhadap penyegaran tab peramban maupun inaktivitas sementara.

**Dampak nyata.** Petugas loket kehilangan data isian jenis kunjungan dan rujukan, merasa bingung mengapa tombol lanjut tidak bisa diklik, serta harus mengulang pemilihan jenis pasien dari awal.

**Rekomendasi.**
1. Tambahkan persistensi ringan berbasis `sessionStorage` atau query parameter pada hook `useInpatientAdmissionFlow` dengan kunci terisolasi per sesi pasien (misal: `quilvian_adm_flow_${patientId}`).
2. Ketika komponen di-mount ulang, lakukan *hydration* otomatis dari `sessionStorage` sebelum kembali ke nilai default kosong.
3. Bersihkan cache `sessionStorage` tersebut hanya ketika proses admisi berhasil diselesaikan (*final submit*) atau tombol *"Batal Admisi"* ditekan secara sengaja.

---

### ISS-EPS-014-03 — Kerentanan Elemen Penutup Modal (*Ghost Backdrop Trap*) Menutupi Area Konten Admisi Tanpa Memblokir Menu Navigasi Sidebar

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source `inpatient-admission-view.jsx:1146-1232` & `confirm-modal.jsx:45-70` |
| **Perbaikan** | `FIX-EPS-014-03` |

**Apa yang terjadi.** Pengguna mengeluhkan bahwa: *"tidak bisa diklik lanjut step atau kembali dan lain-lain, kecuali menu items"*. Ini adalah gejala khas adanya lapisan tak kasat mata (*invisible pointer-events trap / ghost backdrop*) yang menutupi container konten utama (`#content-page`), sementara menu navigasi bilah samping (*sidebar* `.iq-sidebar`) tetap berada di luar cakupan lapisan tersebut sehingga tetap merespons klik tetikus.

**Kenapa terjadi.**
1. Pada `InpatientAdmissionView.jsx` (baris 1146–1232), terdapat setidaknya 3 instance `ConfirmModal` yang di-mount:
   - Dialog Reset Formulir (`resetConfirmOpen`)
   - Dialog Pembatalan Admisi (`cancellationModalOpen`)
   - Pelindung Navigasi Keluar / Unsaved Changes (`exitGuard.open`)
2. Komponen `ConfirmModal` (`src/components/features/base-features/confirm-modal.jsx` baris 45–70) menggunakan struktur Bootstrap Modal (`modal show fade` dan `.modal-backdrop`).
3. Pada arsitektur tata letak tema Quilvian:
   - Sidebar navigasi utama dirender dalam elemen `.iq-sidebar` dengan `z-index` tersendiri (berkisar antara 1000–1060).
   - Area formulir admisi berada di dalam kontainer `#content-page`.
4. Jika terjadi peristiwa peralihan fokus, dialog konfirmasi batal terbuka karena tab blur, atau proses pembersihan DOM (*cleanup*) backdrop terputus ketika tab ditinggal idle, elemen `.modal-backdrop` atau kontainer modal transparan dapat tertinggal di atas `#content-page`.
5. Lapisan ini menyerap seluruh klik mouse yang mengarah ke tombol form admisi (Lanjut, Kembali, dsb.), namun tautan menu pada sidebar tetap dapat menerima klik karena hierarki tumpukan CSS yang berbeda.

**Apakah ini menyimpang dari desain?** Ya, modal dialog tidak boleh meninggalkan elemen backdrop yatim piatu (*orphaned backdrop*) di DOM yang memblokir akses pengguna ke halaman utama.

**Dampak nyata.** Layar tampak normal di mata petugas (seperti tidak ada dialog apa pun), tetapi seluruh tombol di halaman admisi tidak dapat diklik sama sekali, sedangkan mengeklik menu lain di sidebar berhasil berpindah halaman.

**Rekomendasi.**
1. Pastikan seluruh instance `ConfirmModal` di `InpatientAdmissionView` memiliki jaminan *unmount cleanup* yang ketat (menghapus kelas `modal-open` pada elemen `document.body` dan memastikan tidak ada sisa elemen `.modal-backdrop`).
2. Pastikan prop `backdrop="static"` atau penanganan event ESC/luar dikelola secara deterministik tanpa meninggalkan *lingering overlay*.

---

## 5. Tanya-jawab pelapor

| Pertanyaan pelapor | Jawaban berbasis analisis teknis |
| --- | --- |
| *"Kenapa ketika ditinggal lama beberapa menit, tiba-tiba layar seperti fresh?"* | **Karena reservasi sementara tempat tidur kedaluwarsa dan/atau state form ter-reset.** Pada langkah pemilihan tempat tidur, sistem memiliki timer otomatis (10 menit). Jika ditinggal melewati batas waktu tersebut, sistem secara otomatis melepaskan tempat tidur dan memundurkan form ke langkah pemilihan tempat tidur tanpa ada dialog konfirmasi. Selain itu, jika tab peramban mengalami hibernasi memori saat ditinggal, state jenis pasien yang belum tersimpan di `sessionStorage` kembali kosong (`""`). Akibatnya layar tampak seperti baru dimuat ulang (*fresh* / data hilang). |
| *"Kenapa tombol lanjut step atau kembali tidak bisa diklik sama sekali (seperti freeze)?"* | **Karena tombol-tombol tersebut berstatus 'disabled' (terkunci) akibat data prasyaratnya kosong.** Tombol *"Lanjut ke Pemesanan"* hanya aktif jika ada tempat tidur yang sedang dipilih (`selectedBedId`). Ketika reservasi kedaluwarsa, sistem otomatis membersihkan pilihan tersebut (`clearSelection()`), sehingga tombol langsung mati (*disabled* berwarna abu-abu). Pengguna yang tidak menyadari bahwa pilihan bed-nya telah dihapus merasa tombol tersebut macet (*freeze*). Pada kondisi lain, lapisan penutup modal yang tertinggal (*ghost backdrop*) dapat memblokir klik pada area formulir. |
| *"Kenapa menu items di sidebar tetap bisa diklik normal?"* | **Karena menu sidebar berada di luar alur validasi form dan memiliki struktur lapisan CSS terpisah.** Navigasi sidebar adalah tautan menu Next.js independen yang tidak bergantung pada kondisi validasi form admisi. Bahkan jika ada lapisan backdrop modal yang memblokir kontainer konten `#content-page`, hierarki CSS `.iq-sidebar` memungkinkan menu samping tetap menerima klik pengguna. |

---

## 6. Temuan tambahan

1. **Timer Reservasi Tanpa Peringatan Dini:** Saat ini tidak ada indikator visual peringatan dini (seperti hitung mundur berwarna merah atau dialog pop-up konfirmasi perpanjangan waktu saat sisa waktu tinggal 1 menit) sebelum reservasi tempat tidur kedaluwarsa.
2. **Ketiadaan Validasi Auto-Recovery Saat Step URL Berubah:** Ketika URL berada pada `?step=bed-selection`, namun data `patientType` di memori bernilai kosong, sistem tidak mengarahkan pengguna kembali ke langkah awal secara terstruktur, melainkan membiarkan layar berada pada langkah tersebut dengan status form yang tidak valid.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk Pelapor / Pengguna
- Apakah saat layar ditinggal beberapa menit, petugas sedang berada di langkah pemilihan/pemesanan tempat tidur (Langkah 6/7) atau di langkah awal (Langkah 1/2)? *(Analisis teknis membuktikan bahwa fenomena ini dapat terjadi di Langkah 6/7 akibat timer reservasi bed, dan di Langkah 1/2 akibat hibernasi memori tab)*.

### Untuk Pemilik Sistem
- **K-01 (Dialog Kedaluwarsa Bed & Opsi Rekomendasi):** Konfirmasi bahwa pemindahan otomatis secara diam-diam (`onBack()` silent) digantikan dengan dialog peringatan kedaluwarsa ramah (*Reservation Timeout Modal*), didukung indikator peringatan dini saat sisa waktu <= 2 menit dan tombol *"Perpanjang Waktu (+5 Menit)"*.
  - **Status Keputusan:** **DISETUJUI** oleh Pemilik Sistem pada 14 Oktober 2026.
- **K-02 (Persistensi Sesi Alur Admisi):** Konfirmasi bahwa state alur admisi (`patientType`, `visitType`, `referralData`) dipersistensikan sementara di `sessionStorage` agar aman dari penyegaran tab atau hibernasi memori peramban.
  - **Status Keputusan:** **DISETUJUI** oleh Pemilik Sistem pada 14 Oktober 2026.

---

## 8. Catatan pola

Pola masalah ini merupakan **Silent Timeout Rollback with In-Memory Ephemeral State**:
- **Pelepasan Resource Berwaktu Tanpa Komunikasi UI:** Ketika sebuah resource (seperti tempat tidur rumah sakit) memiliki batas waktu penahanan (*time-to-live*), jangan pernah melakukan pembatalan dan pemindahan navigasi secara diam-diam di latar belakang (*silent rollback*). Pengguna manusia yang kembali setelah beberapa menit tidak mengetahui apa yang terjadi dan mengasumsikan sistem mengalami kerusakan (*freeze/bug*).
- **Penyimpanan State Form Panjang Hanya di Memori:** Formulir multi-langkah (*wizard*) di rumah sakit yang memerlukan waktu pengerjaan lama wajib memiliki persistensi sesi sekunder (`sessionStorage` atau URL query) untuk mencegah hilangnya data akibat manajemen memori peramban modern.

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-10-14 | AI Engineering Assistant (Antigravity) | Penyusunan awal dokumen analisis akar masalah ISSUE-EPS-014 terkait layar admisi ter-reset dan mengunci pasca-idle beberapa menit. |
