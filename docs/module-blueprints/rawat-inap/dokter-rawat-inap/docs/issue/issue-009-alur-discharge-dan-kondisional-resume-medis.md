# ISSUE-009 — Restrukturisasi Alur Discharge Dokter Rawat Inap: Kondisional Otomatis Resume ODC/Ranap, Integrasi Lengkap Keputusan Pulang, dan Penataan Ulang Status Episode Full-Width (col-md-12)

```yaml
issue_id: ISSUE-DOK-009
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Ruang Kerja Dokter Rawat Inap — Tab Resume Medis & Layar Keputusan Pulang Episode"
sumber_laporan: "Laporan dan arahan pengguna 22-10-2026: 3 butir keluhan dan arahan restrukturisasi UI resume medis, disertai 3 screenshot pembanding"
tanggal_issue: "2026-10-22"
status: TERBUKA
keparahan_tertinggi: High
basis_source_backend: "41c6451e (MHamzah)"
basis_source_frontend: "dc92a799a (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-009-alur-discharge-dan-kondisional-resume-medis.md
ditulis_dengan: diagnose-module-issue
```

---

## 1. Ringkasan

Berdasarkan laporan pengguna dan evaluasi langsung terhadap alur kerja pemulangan pasien rawat inap, ditemukan bahwa pemisahan antara **Keputusan Pulang (*Decide Discharge*)** di ruang kerja admisi dengan **Resume Medis Pulang** di ruang kerja dokter menimbulkan hambatan kerja yang signifikan bagi dokter spesialis (DPJP):

1. **Segmentasi Manual ODC vs Rawat Inap**: Di tab *Resume Medis* dokter, sistem menyediakan tombol sakelar manual antara "Resume Rawat Inap" dan "Resume ODC". Hal ini membingungkan karena sistem sudah mencatat tipe perawatan pasien sejak admisi; seharusnya sistem **otomatis mendeteksi tipe rawat** dan langsung menyajikan formulir yang sesuai tanpa seleksi manual.
2. **Keterpisahan Menu Keputusan Pulang**: Sesuai aturan medikolegal rumah sakit (`GUARD-INP-03`), form resume medis terkunci selama pasien masih aktif dirawat (`Admitted`). Namun di ruang kerja dokter, tidak disediakan formulir penetapan Keputusan Pulang. Dokter terpaksa harus keluar dari modul dokter dan membuka ruang kerja episode admisi hanya untuk mengklik cara pulang. Padahal dokter dan staf admisi memiliki batasan wewenang yang berbeda.
3. **Keterbatasan Ruang Formulir akibat Layout Dua Kolom**: Card *Status Episode / Ringkasan Operasional* diletakkan sebagai sidebar sempit di sebelah kanan, mempersempit area formulir resume di sebelah kiri. Pengguna menginstruksikan agar kartu ringkasan operasional dan alur discharge diubah menjadi format selebar layar penuh (**`col-md-12`**).

---

## 2. Laporan Asli Pelapor

| No | Kalimat Pelapor | Lampiran Rujukan |
| :-: | :--- | :--- |
| **1** | *"satu tab resume medis odc dan rawat inap itu harusnya tampil dengena menggunakan kondisi: jika dia odc maka tampil resume medis odc , jika dia rawat inap maka tampil resume medis rawat inap"* | Lampiran 3 (Switch Segment Resume) |
| **2** | *"karena aturan resume medis harus diisi setelah keputusan pulang maka , menu resume medis pada tampilan ini memgikuti tampilan yang ada di episode tetapi yang di ambil mulai dari card Alur Discharge sampai dengan riwayat ... jadi seharus ya dokter tidak perlu membuka menu ruang kerja episode dan admisi juga tidak bisa melihat ruang kerja dokter maka dari itu tetap tampilan ya ada tapi hanya bisa di lihat oleh admisi"* | Lampiran 1 & 2 (Alur Discharge & Status Kasir di Episode) |
| **3** | *"Perbaikan pada tampilan status episode seharus pada workspace episode ,dijadikan col md 12 ... lalu ringkasan operasional di buatkan col md 12 saja"* | Lampiran 2 (SidePanel Ringkasan Operasional kanan) |

---

## 3. Ringkasan Temuan

| ID | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :-: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-DOK-009-01` | 1 | Opsi manual Resume ODC vs Rawat Inap membebani dokter | `DESIGN_CHANGE` | Frontend | Medium | `SUDAH-VERIFIKASI` | `FIX-DOK-009-01` |
| `ISS-DOK-009-02` | 2 | Tab resume dokter tidak memiliki akses Keputusan Pulang (terisolasi di workspace admisi) | `DESIGN_CHANGE` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-009-02` |
| `ISS-DOK-009-03` | 3 | Layout sidebar Ringkasan Operasional mempersempit form dan tidak `col-md-12` | `LAYOUT_REFINE` | Frontend | Medium | `SUDAH-VERIFIKASI` | `FIX-DOK-009-03` |
| `ISS-DOK-009-T1` | — | Temuan Tambahan: `ClinicalActionGuard` menahan resume tanpa tombol shortcut Keputusan Pulang | `UI_DEFECT` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-009-02` |

---

## 4. Rincian per Temuan

### ISS-DOK-009-01 — Opsi Manual Resume ODC vs Rawat Inap Membebani Dokter

| Atribut | Keterangan |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend (`inpatient-resume-tab.jsx`) |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code ([inpatient-resume-tab.jsx](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/resume/inpatient-resume-tab.jsx#L82-L92)) |
| **Perbaikan** | `FIX-DOK-009-01` |

**Apa yang terjadi.**  
Pada tab *Resume Medis* ruang kerja dokter, dokter disajikan navigasi tersegmentasi 3 tombol: *Resume Rawat Inap*, *Resume ODC*, dan *Riwayat Revisi*. Dokter harus mengeklik tombol tab yang sesuai secara manual.

**Kenapa terjadi.**  
Komponen `inpatient-resume-tab.jsx` mengimpor `INPATIENT_RESUME_SEGMENTS` secara statis tanpa melakukan evaluasi terhadap jenis unit/kamar perawatan pasien:
```javascript
// inpatient-resume-tab.jsx:82
const segments = INPATIENT_RESUME_SEGMENTS.map((segment) => { ... });
```

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE`. Desain awal PRD Bagian 15 menetapkan pemisahan tab ODC dan Rawat Inap. Namun dalam operasional rumah sakit nyata, pasien yang dibuka oleh dokter sudah memiliki metadata pasti apakah pasien tersebut sedang menempati ruang ODC (One Day Care) atau bangsal rawat inap biasa.

**Dampak nyata.**  
Dokter yang merawat pasien ODC (seperti pasien Farhan Alamsyah di ruang ODC 321A) secara tidak sengaja membuka tab *Resume Rawat Inap*, atau sebaliknya dokter bangsal reguler bingung memilih tab yang benar.

**Rekomendasi.**  
Deteksi otomatis berbasis properti `patientClassName`, `serviceUnitName`, atau `roomName`. Jika teridentifikasi ODC, sistem langsung menampilkan form Resume ODC. Jika ranap biasa, langsung tampilkan form Resume Rawat Inap. Sub-navigasi cukup membedakan antara **Formulir Resume** dan **Riwayat Revisi**.

---

### ISS-DOK-009-02 — Tab Resume Dokter Tidak Memiliki Akses Keputusan Pulang (Terisolasi di Admisi)

| Atribut | Keterangan |
| :--- | :--- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` / `WORKFLOW_INTEGRATION` |
| **Area** | Frontend (`inpatient-resume-tab.jsx`, `use-inpatient-resume-tab.jsx`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source ([use-inpatient-resume-tab.jsx](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-resume-tab.jsx#L90-L100) & [inpatient-discharge-utils.jsx](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-discharge-utils.jsx#L241-L247)) |
| **Perbaikan** | `FIX-DOK-009-02` |

**Apa yang terjadi.**  
Dokter yang membuka tab Resume Medis pada pasien aktif mendapati peringatan *"Pengisian Resume Ditahan"* dengan semua isian dinonaktifkan. Tidak ada tombol atau formulir bagi dokter untuk menetapkan status boleh pulang di layar tersebut, sehingga dokter terpaksa membuka modul admisi episode.

**Kenapa terjadi.**  
Logika penjaga medikolegal `GUARD-INP-03` menolak penyusunan resume selama episode berstatus `Admitted` (`episodeStatus !== DISCHARGE_PENDING`). Namun kontrol penetapan Keputusan Pulang (`DecideDischarge`) hanya diletakkan di halaman admisi episode ([inpatient-discharge-view.jsx](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx)), bukan di tab dokter rawat inap.

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE`. Desain sebelumnya memisahkan layar dokter (khusus EMR) dan layar episode (khusus administrasi pemulangan). Namun secara regulasi klinis, keputusan pasien boleh pulang merupakan wewenang mutlak Dokter DPJP, sehingga harus dapat dilakukan langsung di ruang kerja dokter.

**Dampak nyata.**  
Alur kerja dokter terputus. Dokter harus berpindah-pindah menu sistem, padahal staf admisi dilarang masuk ke ruang kerja dokter demi privasi medis.

**Rekomendasi.**  
Integrasikan komponen pemulangan lengkap ke dalam tab Resume Medis Dokter:
1. Stepper **Alur Discharge** (Progres Keputusan $\rightarrow$ Resume $\rightarrow$ E-Sign $\rightarrow$ Final),
2. **Tahap 1: Keputusan Pulang** (Pilihan cara pulang & tombol simpan langsung),
3. **Tahap 2: Resume Pulang** (Form 8 bagian klinis dengan tombol prefill "Isi dari Data Klinis"),
4. **Status Izin Kasir** & **Riwayat Versi Resume**.

---

### ISS-DOK-009-03 — Layout Sidebar Ringkasan Operasional Mempersempit Form dan Tidak `col-md-12`

| Atribut | Keterangan |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `LAYOUT_REFINE` |
| **Area** | Frontend (`inpatient-discharge-view.jsx`, `inpatient-discharge-view.module.css`) |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source CSS ([inpatient-discharge-view.module.css](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/inpatient-discharge-view.module.css#L375-L380)) |
| **Perbaikan** | `FIX-DOK-009-03` |

**Apa yang terjadi.**  
Card *STATUS EPISODE - Ringkasan Operasional* ditampilkan di kolom kanan sempit (`1.12fr`), membuat kolom formulir kiri (`3fr`) berdesakan dan membatasi kenyamanan dokter saat mengetik resume klinis.

**Kenapa terjadi.**  
CSS `.workspaceGrid` menggunakan CSS Grid 2-kolom:
```css
.workspaceGrid {
  display: grid;
  grid-template-columns: minmax(0, 3fr) minmax(calc(var(--space-8) * 9), 1.12fr);
  gap: var(--space-4);
}
```

**Apakah ini menyimpang dari desain?**  
`DESIGN_CHANGE` / `LAYOUT_REFINE`. Layout desktop 2-kolom memakan ruang horizontal yang berharga bagi form rekam medis.

**Dampak nyata.**  
Tampilan pada layar laptop standar (1366x768 atau 1920x1080) terasa sempit, tabel diagnosis dan textarea catatan dokter terpotong atau membutuhkan scroll horizontal/vertikal berlebih.

**Rekomendasi.**  
Ubah struktur menjadi single-column full width (`col-md-12` / `col-12`). Card *Ringkasan Operasional* didesain ulang menjadi kartu horizontal ringkas yang elegan di bawah stepper alur pemulangan.

---

## 5. Tanya-Jawab Pelapor

> **T:** *Mengapa saat membuka resume Farhan Alamsyah muncul 'Pengisian Resume Ditahan' padahal saya sudah mengisi SOAP, Resep, dan Tindakan?*  
> **J:** Karena status pasien masih `Admitted` (Aktif dirawat). Sesuai aturan keselamatan sistem Quilvian, dokumen Resume Medis Kepulangan hanya boleh disusun jika dokter telah menetapkan bahwa pasien boleh pulang (*Discharge Pending*). Seluruh data SOAP, resep, dan tindakan yang telah Anda isi tersimpan aman di database dan nantinya dapat diimpor otomatis ke formulir resume dengan tombol **"Isi dari Data Klinis"**.

> **T:** *Apakah halaman episode di admisi akan dihapus jika alurnya dipindah ke ruang kerja dokter?*  
> **J:** Tidak dihapus. Halaman episode di admisi (`/episodes/[id]/discharge`) tetap ada khusus untuk staf Admisi dan Rekam Medis (dalam mode pantau status berkas dan billing kasir), karena staf admisi dilarang mengakses Ruang Kerja Klinis Dokter. Namun dokter kini dapat menyelesaikan seluruh alur pemulangan langsung dari Ruang Kerja Dokter tanpa perlu membuka menu admisi lagi.

---

## 6. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik Bisnis / Tim Pengembang:
| No | Pertanyaan / Pilihan | Kenapa Ditanyakan | Rekomendasi Solusi |
| :-: | :--- | :--- | :--- |
| **K-01** | Apakah form Resume ODC memerlukan field khusus atau memakai form 8 bagian rawat inap dengan penyesuaian label? | Panel ODC saat ini berstatus *"Integrasi Belum Tersedia"*. | **Rekomendasi:** Tampilkan form resume terpadu yang disesuaikan untuk ODC (ringkasan tindakan dan anjuran 24 jam) saat tipe rawat adalah ODC. |
| **K-02** | Posisi Card Ringkasan Operasional `col-md-12` diletakkan di atas Tahap 1 atau di bawah Tahap 2? | Penataan visual alur kerja. | **Rekomendasi:** Diletakkan tepat di bawah Card Alur Discharge sebagai banner status horizontal `col-md-12`. |

---

## 7. Catatan Pola & Pencegahan Regresi

1. **Pola Konsistensi Ruang Kerja Klinis**: Setiap dokumen klinis yang membutuhkan prasyarat status administratif harus menyediakan kontrol perubahan status tersebut secara terpadu di ruang kerja dokter atau memberikan instruksi kontekstual yang jelas.
2. **Pola Layout Responsif**: Komponen formulir klinis berbobot tinggi (seperti Resume Medis, Pengkajian Medis Awal, CPPT) harus menggunakan lebar penuh (`col-md-12`) agar memberikan ruang baca yang ergonomis bagi tenaga medis.

---

## 8. Riwayat Dokumen

| Tanggal | Versi | Pengubah | Keterangan |
| :--- | :---: | :--- | :--- |
| **2026-10-22** | `1.0.0` | Antigravity AI (`diagnose-module-issue`) | Penulisan dokumen issue awal berdasarkan laporan pengguna dan tangkapan layar 22-10-2026 |
