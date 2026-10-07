# ISSUE-012 — Status Integrasi Catatan Pra-Operasi serta Pemesanan Laboratorium dan Radiologi pada Ruang Kerja Keperawatan

```yaml
issue_id: ISSUE-KEP-012
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan Rawat Inap — Catatan Keperawatan (Sub-tab Pra-Operasi) & Penunjang Medis (Sub-tab Lab & Radiologi) (FE-KEP-24, FE-KEP-15 / FE-RWI-180, FE-RWI-089)"
sumber_laporan: "Laporan pengguna 07-10-2026: '1. Catatan Pra Operasi 2. radiologi dan laboratorium pada yang saya sebutkan kenapa pada keperawatan belum terintegrasi ya ? coba anda lakukan analisis terlebih dahulu lalu jelaskan'"
tanggal_issue: "2026-10-07"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "671191eb (MHamzah)"
source_sha_frontend: "1f889d67c (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-012-integrasi-pra-operasi-lab-radiologi-keperawatan.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Laporan pengguna menanyakan mengapa **Catatan Pra-Operasi** serta pemeriksaan **Radiologi dan Laboratorium** belum terintegrasi pada modul Keperawatan Rawat Inap.

Berdasarkan penelusuran menyeluruh pada kode sumber frontend dan backend serta dokumen blueprint rekayasa Quilvian, ditemukan bahwa kedua fitur tersebut berada pada status integrasi yang berbeda dengan akar masalah yang nyata:

1. **Catatan Pra-Operasi (`ISS-KEP-012-01`)**: Sub-tab *Catatan Pra-Operasi* pada Menu 3 (*Catatan Keperawatan*) masih menampilkan komponen penanda sementara (*stub*) `<NursingUnavailableSection>` bertuliskan *"Integrasi belum tersedia"*. Padahal, komponen fungsional laci Catatan Pra-Operasi bangsal ([`WardPreOpDrawer.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/features/health-services/operating-room-management/ward-pre-op/ward-pre-op-drawer.jsx)) dan backend pendukungnya ([`OprWardPreOpNote.cs`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprWardPreOpNote.cs)) **sudah selesai diimplementasikan penuh**, namun ditempatkan di **Menu 7 (Ruangan Bedah)** pada tabel kasus operasi pasien. Terjadi fragmentasi navigasi: perawat yang membuka sub-tab Catatan Pra-Operasi di Menu 3 tidak disajikan data kasus bedah atau akses ke laci pra-operasi, melainkan pesan bahwa fitur belum tersedia.
2. **Laboratorium dan Radiologi (`ISS-KEP-012-02`)**: Pada Menu 4 (*Penunjang Medis*), pembacaan riwayat dan hasil akhir laboratorium dan radiologi **sudah terintegrasi 100% (Read-Only)**. Namun, tombol aksi **`+ Pesan Laboratorium`** dan **`+ Pesan Radiologi`** dalam keadaan mati (**`disabled={true}`**) dengan keterangan *"Pemesanan dari sisi perawat sedang disiapkan"*. Hal ini terjadi karena tata kelola klinis (*clinical governance*) membatasi pemesanan penunjang oleh perawat hanya boleh atas dasar **instruksi DPJP**. Di sisi backend, dukungan delegasi instruksi dokter telah selesai (`BE-RWI-104`), namun formulir pop-up/modal pemesanan khusus perawat yang mewajibkan input Dokter Pemberi Instruksi belum dibuat di frontend (berbeda dengan Konsultasi Gizi, Bank Darah, dan Hemodialisa yang sudah memiliki modal pemesanan perawat).

Isu ini berstatus keparahan **High** karena alur klinis persiapan operasi dan penindakan instruksi penunjang dari DPJP terhambat, serta menimbulkan persepsi bagi perawat bangsal bahwa sistem belum mendukung fitur-fitur tersebut.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "1. Catatan Pra Operasi ... pada yang saya sebutkan kenapa pada keperawatan belum terintegrasi ya ?" | Layar Ruang Kerja Keperawatan rawat inap, sub-tab Catatan Pra-Operasi pada menu Catatan Keperawatan yang memuat badge "Integrasi belum tersedia". |
| 2 | "2. radiologi dan laboratorium ... pada yang saya sebutkan kenapa pada keperawatan belum terintegrasi ya ?" | Layar Ruang Kerja Keperawatan rawat inap, menu Penunjang Medis sub-tab Laboratorium & Radiologi dengan tombol pemesanan berstatus disabled. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | :---: | :---: | :---: | :---: | --- |
| `ISS-KEP-012-01` | 1 | Sub-tab Catatan Pra-Operasi pada Catatan Keperawatan menampilkan stub "Integrasi belum tersedia" padahal laci pra-operasi bangsal sudah aktif di Ruangan Bedah | `DESIGN_CHANGE` / `BUG` | Frontend | High | `SUDAH-VERIFIKASI` di source | `FIX-KEP-012-01` |
| `ISS-KEP-012-02` | 2 | Tombol pemesanan baru Laboratorium dan Radiologi dinonaktifkan (`disabled`) karena modal pemesanan berdelegasi instruksi DPJP (`BE-RWI-104`) belum dibuat di frontend | `DESIGN_CHANGE` / `MISSING` | Frontend | High | `SUDAH-VERIFIKASI` di source | `FIX-KEP-012-02`, `FIX-KEP-012-03` |

---

## 4. Rincian per Temuan

### ISS-KEP-012-01 — Sub-tab Catatan Pra-Operasi pada Catatan Keperawatan Menampilkan Stub "Integrasi belum tersedia"

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` / `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend dan backend |
| **Perbaikan** | `FIX-KEP-012-01` |

**Apa yang terjadi.**  
Ketika perawat membuka menu **Catatan Keperawatan** dan memilih sub-tab **Catatan Pra-Operasi**, layar tidak menampilkan formulir atau daftar checklist pra-bedah, melainkan menampilkan panel kosong dengan badge peringatan: *"Integrasi belum tersedia — Catatan pra-operasi dijadwalkan pada rilis mendatang. Integrasi belum tersedia dan tidak ada permintaan jaringan."* Perawat menyimpulkan bahwa fitur ini belum dapat digunakan.

**Kenapa terjadi.**  
Rantai penyebab dari tampilan antarmuka hingga arsitektur sistem:

1. Di berkas [`nursing-narrative-tab.jsx:114-122`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx#L114-L122):
   ```jsx
   case "pre-op":
     return (
       // Catatan pra-operasi: FE-RWI-195
       <NursingUnavailableSection
         title="Catatan Pra-Operasi"
         description="Catatan pra-operasi dijadwalkan pada rilis mendatang. Integrasi belum tersedia dan tidak ada permintaan jaringan."
         badge="Integrasi belum tersedia"
       />
     );
   ```
2. Padahal, implementasi riil Catatan Pra-Operasi bangsal telah selesai dibangun:
   - Backend: [`OprWardPreOpNote.cs`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprWardPreOpNote.cs) (`BE-RWI-176`, `RWI-DEC-173`).
   - Frontend: [`WardPreOpDrawer.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/features/health-services/operating-room-management/ward-pre-op/ward-pre-op-drawer.jsx) (`FE-RWI-195`).
3. Namun komponen laci pra-operasi tersebut hanya dihubungkan pada **Menu 7: Ruangan Bedah** ([`surgery-booking-section.jsx:94-103`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx#L94-L103)) di dalam tabel kasus operasi pasien ([`patient-surgery-cases-table.jsx:209-214`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx#L209-L214)).
4. Karena Catatan Pra-Operasi bangsal membutuhkan parameter `caseId` (ID Kasus Jadwal Operasi aktif), formulir tersebut tidak bisa dibuka tanpa adanya pemesanan jadwal operasi terlebih dahulu. Ketika sub-tab `pre-op` di Menu 3 tidak dihubungkan ke alur kasus operasi pasien, sub-tab tersebut tertinggal sebagai *stub*.

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE`. Desain tata kelola Menu V2 (`FE-RWI-180` / `RWI-DEC-172`) mencantumkan sub-tab `pre-op` di bawah menu Catatan Keperawatan, tetapi tim implementasi modul Kamar Bedah meletakkannya di Menu 7. Solusinya adalah menyatukan jembatan navigasi kedua menu tersebut tanpa merusak keterikatan data kasus operasi.

**Dampak nyata.**  
Perawat yang bertugas menyiapkan pasien sebelum dijemput petugas Kamar Bedah (IBS) mengira sistem belum mendukung pengisian checklist pra-operasi digital, sehingga proses serah terima kembali dilakukan manual di atas kertas.

**Rekomendasi.**  
Ganti komponen stub pada sub-tab `pre-op` di `nursing-narrative-tab.jsx` menjadi **Konsol Status & Akses Pra-Operasi Bangsal**:
- Jika pasien memiliki kasus operasi aktif pada episode rawat inap ini: tampilkan kartu ringkasan jadwal operasi beserta tombol aksi langsung **`[ Buka Catatan Pra-Operasi ]`** yang memicu `WardPreOpDrawer`.
- Jika pasien belum memiliki jadwal operasi: tampilkan panduan alur klinis bahwa catatan pra-bedah memerlukan booking operasi terlebih dahulu, disertai tombol navigasi cepat **`[ Buka Menu Ruangan Bedah → ]`**.

---

### ISS-KEP-012-02 — Tombol Pemesanan Laboratorium dan Radiologi Dinonaktifkan (`disabled`)

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` / `MISSING` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source frontend dan backend |
| **Perbaikan** | `FIX-KEP-012-02`, `FIX-KEP-012-03` |

**Apa yang terjadi.**  
Pada menu **Penunjang Medis**, ketika perawat membuka sub-tab **Laboratorium** atau **Radiologi**, perawat dapat melihat riwayat pemeriksaan dan hasil ekspertise yang sudah selesai. Namun tombol pembuatan pesanan baru **`+ Pesan Laboratorium`** dan **`+ Pesan Radiologi`** terkunci berwarna abu-abu (*disabled*) dan tidak dapat ditekan. Terdapat teks petunjuk: *"Pemesanan dari sisi perawat sedang disiapkan. Hasil pemeriksaan tetap dapat dibaca di bawah."*

**Kenapa terjadi.**  
Rantai penyebab teknis dan tata kelola klinis:

1. Di berkas [`nursing-ancillary-section.jsx:254-272`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx#L254-L272):
   ```jsx
   <div className={styles.nursingAncillaryOrderGuard} data-testid="ancillary-order-guard">
     <BaseButton
       type="button"
       variant="secondary"
       className={styles.nursingAncillaryOrderButton}
       disabled={true}
       title={`Pemesanan ${sectionTitle} dari sisi perawat sedang disiapkan.`}
     >
       + Pesan {sectionTitle}
     </BaseButton>
     <small className={styles.nursingAncillaryOrderHint}>
       Pemesanan dari sisi perawat sedang disiapkan. Hasil pemeriksaan tetap dapat dibaca di bawah.
     </small>
   </div>
   ```
2. **Aspek Tata Kelola Klinis (*Clinical Governance*)**:  
   Pemeriksaan laboratorium (darah, urin, dsb.) dan radiologi (rontgen, USG, CT Scan) adalah **tindakan diagnostik medis delegatif**. Berdasarkan regulasi rumah sakit, perawat rawat inap dilarang meresepkan pemeriksaan penunjang secara mandiri tanpa instruksi DPJP.
3. **Kesiapan Backend vs Frontend**:
   - Backend telah menyelesaikan tata kelola instruksi dokter pada task [`BE-RWI-104.md`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/backend/BE-RWI-104.md) per 17 September 2026. Backend mewajibkan field `InstructingDoctorId` dan mengatur `InstructionVerificationStatus = Pending` bila pemesan adalah perawat rawat inap.
   - Frontend telah membuat modal pemesanan perawat untuk Konsultasi Gizi (`NutritionOrderModal`), Bank Darah (`BloodBankOrderModal`), dan Hemodialisa (`HemodialysisOrderModal`).
   - Namun, **komponen modal pemesanan khusus perawat untuk Laboratorium dan Radiologi (`NursingLabOrderModal` dan `NursingRadiologyOrderModal`) belum dibangun di frontend**. Sesuai acceptance criteria `AC-6`, tombol di-disable sampai modal dengan input wajib Dokter Pemberi Instruksi tersedia.

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE` / `MISSING`. Desain awal menahan tombol ini dengan sengaja demi keselamatan klinis (*clinical safety guard*) hingga backend `BE-RWI-104` stabil. Sekarang setelah backend siap, antarmuka formulir pemesanan perawat harus segera diwujudkan agar alur delegasi instruksi dapat berjalan.

**Dampak nyata.**  
Ketika DPJP melakukan visite pagi dan memberikan instruksi lisan *"Tolong cek Darah Lengkap dan Foto Thorax cito untuk pasien ini"*, perawat tidak bisa menginput pesanan tersebut ke sistem. Perawat terpaksa meminjam akun dokter atau menelepon unit laboratorium/radiologi secara manual di luar sistem.

**Rekomendasi.**  
Bangun dua komponen modal pemesanan khusus perawat:
1. `NursingLabOrderModal`: formulir pemesanan laboratorium dengan pemilihan tes/panel lab, prioritas (Rutin/Cito), instruksi klinis, serta **pemilihan wajib Dokter Pemberi Instruksi** dari daftar dokter yang sedang bertugas atas pasien.
2. `NursingRadiologyOrderModal`: formulir pemesanan radiologi dengan pemilihan modalitas, jenis pemeriksaan, indikasi klinis, prioritas, serta **pemilihan wajib Dokter Pemberi Instruksi**.
3. Aktifkan kedua tombol `+ Pesan Laboratorium` dan `+ Pesan Radiologi` di `nursing-ancillary-section.jsx` untuk memicu modal tersebut, dan tampilkan badge status verifikasi (*Pending / Verified*) pada tabel riwayat pesanan.

---

## 5. Tanya-Jawab Pelapor

> **T:** Kenapa Catatan Pra Operasi pada keperawatan belum terintegrasi?  
> **J:** Formulir dan alur Catatan Pra-Operasi bangsal sebenarnya **sudah selesai dibuat di sistem**, namun diletakkan di **Menu 7 (Ruangan Bedah)** karena formulir tersebut wajib terhubung ke nomor kasus pesanan kamar operasi (`OprCaseId`). Sementara itu, sub-tab *Catatan Pra-Operasi* yang berada di Menu 3 (*Catatan Keperawatan*) masih tertinggal dengan tampilan sementara (*stub*). Solusinya adalah menghubungkan sub-tab di Menu 3 langsung ke data kasus operasi pasien dan laci formulir pra-operasi terkait.

> **T:** Kenapa Radiologi dan Laboratorium pada keperawatan belum terintegrasi?  
> **J:** Integrasi pembacaan riwayat dan hasil akhir pemeriksaan **sudah berjalan penuh**. Yang saat ini belum dibuka adalah **pembuatan pesanan baru oleh perawat** (tombol dinonaktifkan). Tombol ini dikunci karena aturan rumah sakit mewajibkan pesanan penunjang dari perawat mencantumkan dokter yang memberi instruksi (*delegasi DPJP*). Skema ini sudah siap di backend (`BE-RWI-104`), dan yang perlu dilengkapi adalah pembuatan jendela formulir pop-up pemesanan khusus perawat di antarmuka web.

---

## 6. Temuan Tambahan

### ISS-KEP-012-T1 — Tabel Kasus Operasi Pasien Menghilang Bila Akun Belum Memiliki Permission Ruang Bedah

| | |
| --- | --- |
| **Area** | Frontend & Otorisasi RBAC |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di [`patient-surgery-cases-table.jsx:57-59`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx#L57-L59) |
| **Kenapa dicantumkan** | Berhubungan erat dengan keluhan nomor 1 (Catatan Pra-Operasi). Bila akun perawat belum memiliki hak `OperatingRoomCase:Read`, tabel kasus operasi tersembunyi (*return null*), sehingga tombol Pra-Operasi tidak dapat diakses sama sekali. |
| **Rekomendasi** | Pastikan seluruh role perawat rawat inap memiliki permission `OperatingRoomCase:Read` dan `OperatingRoomWardPreOp:Send`. Jika permission tidak ada, tampilkan pesan informatif hak akses, bukan menyembunyikan komponen tanpa jejak. |

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

**Untuk pelapor:**
*Tidak ada pertanyaan yang menahan analisis. Kedua butir keluhan telah terverifikasi secara tuntas pada kode sumber.*

**Untuk pemilik modul / produk:**

| No | Keputusan | Pilihan | Status Keputusan | Menahan perbaikan |
| ---: | --- | --- | :---: | :---: |
| K-01 | Alur pembukaan Catatan Pra-Operasi di Menu Catatan Keperawatan | A: Tampilkan kartu ringkasan kasus bedah aktif + tombol buka laci pra-operasi.<br/>B: Alihkan navigasi otomatis (*redirect*) ke Menu 7 (Ruangan Bedah). | **DISETUJUI (Opsi A)** oleh Pemilik Produk (2026-10-07) | `FIX-KEP-012-01` |
| K-02 | Verifikasi instruksi dokter untuk pesanan Lab/Rad perawat | A: Pesanan langsung dikirim ke instalasi Lab/Rad dengan status verifikasi `Pending` oleh DPJP.<br/>B: Pesanan tertahan di bangsal sampai DPJP memverifikasi sebelum dikirim ke Lab/Rad. | **DISETUJUI (Opsi A)** oleh Pemilik Produk (2026-10-07) | `FIX-KEP-012-02`, `FIX-KEP-012-03` |

---

## 8. Catatan Pola

- **Pola Tombol Aksi Disabled Tanpa Dialog Penjelas**: Sama dengan pola isu terdahulu pada modul rawat inap, mematikan tombol aksi tanpa menyediakan dialog/formulir yang memberi tahu syarat yang kurang membingungkan pengguna klinis.
- **Pola Komponen Stub Yang Terlupakan**: Ketika fitur direlokasi ke menu lain (seperti Pra-Operasi yang dipindah ke Ruangan Bedah), menu asal tidak dipasangi jembatan atau diarahkan ulang, sehingga menyisakan komponen *unavailable* yang kontradiktif.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-07 | Dokumen issue dibuat dari analisis laporan 2 butir (Catatan Pra-Operasi dan Lab/Radiologi) | `diagnose-module-issue` |
| 2026-10-07 | Rencana perbaikan disetujui penuh (K-01 Opsi A, K-02 Opsi A); status berubah menjadi DALAM_PERBAIKAN | Pemilik Produk |
