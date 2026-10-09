# ISSUE-011 — Duplikasi Bilah Tab Navigasi dan Inkonsistensi Visual pada Layar Pemakaian Alat Medis

```yaml
issue_id: ISSUE-KEP-011
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan Rawat Inap — Seksi Pemakaian Alat (FE-RWI-090 / FE-RWI-188 / FE-KEP-28)"
sumber_laporan: "Laporan pengguna 06-10-2026: 'perbaiki tampilan pemakaian alat medis' disertai tangkapan layar antarmuka"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-011-tampilan-pemakaian-alat-medis.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Pengguna melaporkan keluhan terhadap tampilan layar **Pemakaian Alat Medis** di Ruang Kerja Keperawatan Rawat Inap dengan menyertakan tangkapan layar antarmuka. Berdasarkan audit kode sumber dan analisis tangkapan layar, ditemukan masalah utama berupa **duplikasi bilah tab navigasi (double tab bar)**. Di atas kartu judul seksi telah tersedia bilah navigasi horizontal resmi `NursingSecondaryTabBar` yang memuat tab *Order Alat Kesehatan* dan *History Alat Kesehatan*, namun di dalam komponen `NursingEquipmentSection` terdapat bilah tab kedua berbasis Bootstrap (`nav nav-tabs`) dengan nama yang sama persis. Selain itu, state tab lokal di dalam komponen terkunci pada inisialisasi awal (`useState(initialTab)`) sehingga ketika pengguna mengklik tab resmi di bilah atas, tampilan di bawahnya tidak berganti.

Masalah kedua adalah ketidakseimbangan tata letak dan kekasaran visual (*unpolished UI*): kartu pemantauan *Alat Medis Sedang Berjalan* hanya berupa bidang kosong tanpa ilustrasi status (*empty state*), elemen formulir pemakaian alat masih menggunakan elemen kontrol HTML native tanpa komponen dasar Quilvian (*Base Components*), dan modal interaksi (Selesai, Batal, Koreksi Waktu) masih dirender secara manual via div overlay tanpa integrasi pustaka modal terstandar.

Isu ini berstatus keparahan **High** karena memicu kebingungan navigasi bagi perawat dan menghambat pergantian tab dari bilah navigasi utama, serta diselesaikan tuntas dengan menyelaraskan arsitektur navigasi satu pintu, mengadopsi *Base Components*, dan menyempurnakan ergonomi pemantauan alat medis rawat inap.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "perbaiki tampilan pemakaian alat medis" | Tangkapan layar antarmuka Pemakaian Alat Medis menampilkan dua bilah tab bertumpuk (*Order Alat Kesehatan* & *History Alat Kesehatan*), formulir mulai pemakaian di sebelah kiri, dan kartu alat berjalan di sebelah kanan. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-KEP-011-01` | 1 | Duplikasi bilah navigasi tab (*double tab bar*) dan desinkronisasi pergantian tab | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-KEP-011-01` |
| `ISS-KEP-011-02` | 1 | Ketidakseimbangan tata letak dan ketiadaan *empty state* pada panel alat berjalan | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-011-02` |
| `ISS-KEP-011-03` | 1 | Formulir pemakaian alat masih memakai elemen kontrol native dan styling kasar | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-011-03` |
| `ISS-KEP-011-T1` | — | Dialog modal penyelesaian, pembatalan, dan koreksi waktu memakai div overlay manual | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-011-04` |
| `ISS-KEP-011-T2` | — | Peringatan linter `react-hooks/set-state-in-effect` pada pemuatan data awal | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-KEP-011-05` |

---

## 4. Rincian per Temuan

### ISS-KEP-011-01 — Duplikasi Bilah Navigasi Tab dan Desinkronisasi Pergantian Tab

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-equipment-section.jsx:47–50, 312–334` |
| **Perbaikan** | `FIX-KEP-011-01` |

**Apa yang terjadi.**
Pada layar Pemakaian Alat Medis, perawat melihat dua baris tab dengan tulisan yang sama persis (*Order Alat Kesehatan* dan *History Alat Kesehatan*). Baris pertama berada di menu horizontal atas, sedangkan baris kedua berada tepat di bawah judul kartu pemakaian alat. Ketika perawat mengklik tab di baris atas, tampilan layar tidak berubah karena sistem mengabaikan klik tersebut dan hanya merespons klik pada tab baris bawah.

**Kenapa terjadi.**
Komponen induk `nursing-workspace-view.jsx` sudah menyediakan bilah navigasi horizontal tunggal `NursingSecondaryTabBar` yang membaca konfigurasi tab dari `INPATIENT_NURSING_WORKSPACE_SECTIONS[4].tabs`. Namun, di dalam `nursing-equipment-section.jsx`, pengembang menambahkan bilah tab internal kedua:
```jsx
// nursing-equipment-section.jsx:47-49
export default function NursingEquipmentSection({ activeTab: initialTab = "order" }) {
  const [activeTab, setActiveTab] = useState(initialTab);
```
Nilai `initialTab` hanya ditangkap satu kali saat komponen dipasang (*mounted*). Ketika perawat mengklik bilah tab atas, prop `activeTab` berubah namun state lokal `activeTab` tidak disinkronkan, sehingga tab bawah tidak berpindah. Di sisi tampilan, baris 312–334 merender `<div className="nav nav-tabs mb-4">` yang menduplikasi menu atas secara visual.

**Apakah ini menyimpang dari desain?**
Menyimpang (`BUG`). Pada seksi keperawatan lain seperti Tindakan (`nursing-procedure-section.jsx`) dan Penunjang Medis (`nursing-ancillary-section.jsx`), seluruh pergantian sub-tab dikontrol dari bilah atas terpusat tanpa bilah tab internal lokal ganda.

**Dampak nyata.**
Perawat yang terbiasa berpindah tab dari menu atas mengira fitur Riwayat Alat Medis sedang macet atau tidak dapat dibuka, padahal harus mengklik tombol tab kedua di bawahnya.

**Rekomendasi.**
Hapus bilah tab Bootstrap internal lokal. Sinkronkan prop `activeTab` secara langsung ke alur render (`const currentTab = activeTab || "order"`), konsisten dengan seksi Tindakan dan Penunjang Medis.

---

### ISS-KEP-011-02 — Ketidakseimbangan Tata Letak dan Ketiadaan Empty State pada Panel Alat Berjalan

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-equipment-section.jsx:431–510` |
| **Perbaikan** | `FIX-KEP-011-02` |

**Apa yang terjadi.**
Panel "Alat Medis Sedang Berjalan" di sebelah kanan terlihat kosong melompong dengan teks abu-abu sederhana tanpa ikon atau panduan klinis saat tidak ada alat yang dipasang pada pasien. Saat ada alat yang berjalan, tombol aksi (Koreksi Waktu, Batal, Selesai) tampil kecil dan menumpuk di bagian bawah tanpa penegasan visual yang rapi.

**Kenapa terjadi.**
Panel kanan hanya mengandalkan kelas bawaan Bootstrap `.card` dan `.list-group-item` standar tanpa integrasi pedoman antarmuka klinis Quilvian (*Quilvian Design System*).

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Desain awal hanya mencakup fungsi minimal, namun untuk operasional bangsal yang padat, kejelasan status alat aktif sangat menentukan keselamatan pasien (*patient safety*) dan keakuratan pencatatan waktu.

**Dampak nyata.**
Perawat baru sulit memastikan apakah sistem sedang memuat data alat atau memang belum ada alat yang dipasang.

**Rekomendasi.**
Rancang ulang panel alat berjalan dengan:
1. Header kartu yang memuat lencana jumlah alat aktif (*live counter badge*).
2. Tampilan *empty state* profesional dengan ikon klinis, judul informatif, dan teks penjelasan.
3. Kartu alat aktif individual yang menampilkan nama alat, dokter penanggung jawab, perawat pelaksana, waktu mulai, durasi berjalan (*elapsed time*), dan tombol aksi terstruktur (`BaseButton`).

---

### ISS-KEP-011-03 — Formulir Pemakaian Alat Masih Memakai Elemen Kontrol Native dan Styling Kasar

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-equipment-section.jsx:340–428` |
| **Perbaikan** | `FIX-KEP-011-03` |

**Apa yang terjadi.**
Formulir "Mulai Pemakaian Alat Medis" menggunakan elemen dropdown `<select>`, kotak teks `<textarea>`, dan tombol `<button className="btn btn-primary">` standar peramban. Pesan error dan sukses ditampilkan melalui `<div className="alert alert-danger">` biasa, berbeda dari format peringatan keselamatan klinis Quilvian.

**Kenapa terjadi.**
Komponen ditulis dengan fokus fungsionalitas murni tanpa menerapkan pustaka *Base Components* yang diwajibkan oleh aturan tata kelola frontend Quilvian (`base-component-catalog.md`).

**Apakah ini menyimpang dari desain?**
`RULE_VIOLATION`. Melanggar standar konsistensi komponen frontend Quilvian yang mewajibkan penggunaan `BaseButton`, `ClinicalSafetyAlert`, dan `InformationAlert`.

**Dampak nyata.**
Sensasi penggunaan antarmuka terasa tidak terpadu dan canggung bila dibandingkan dengan formulir konsultasi gizi, bank darah, atau catatan asuhan keperawatan.

**Rekomendasi.**
Ganti tombol submit dengan `BaseButton`, ganti kotak alert dengan `ClinicalSafetyAlert`, tambahkan kartu informasi edukasi penagihan otomatis rawat inap (*InformationAlert*), dan beri styling elegan pada elemen pilihan alat serta dokter penanggung jawab.

---

### ISS-KEP-011-T1 — Dialog Modal Penyelesaian, Pembatalan, dan Koreksi Waktu Memakai Div Overlay Manual

| | |
| :--- | :--- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `nursing-equipment-section.jsx:600–742` |
| **Perbaikan** | `FIX-KEP-011-04` |

**Apa yang terjadi.**
Ketiga jendela pop-up interaksi (Selesaikan Pemakaian Alat, Batalkan Pemakaian Alat, dan Koreksi Waktu) dirender menggunakan manipulasi CSS manual `<div className="modal d-block bg-black bg-opacity-50">` alih-alih memanfaatkan pustaka komponen modal resmi.

**Kenapa terjadi.**
Solusi cepat saat pembangunan awal yang tidak memakai komponen `react-bootstrap/Modal`.

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`.

**Dampak nyata.**
Aksesibilitas terhambat (tidak dapat ditutup dengan tombol Escape keyboard secara alami, tidak ada penanganan scroll locking body), dan tata letak footer modal tidak seragam dengan modal pemesanan penunjang lainnya.

**Rekomendasi.**
Migrasikan ketiga jendela pop-up ke `Modal` React Bootstrap standar dengan header ikonik, ringkasan data alat yang sedang diproses, peringatan keselamatan klinis untuk pembatalan (`CLI-EQP-002`), serta tombol aksi `BaseButton`.

---

### ISS-KEP-011-T2 — Peringatan Linter react-hooks/set-state-in-effect pada Pemuatan Data Awal

| | |
| :--- | :--- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI via `npx eslint` baris 123 |
| **Perbaikan** | `FIX-KEP-011-05` |

**Apa yang terjadi.**
ESLint memberikan peringatan: `Calling setState synchronously within an effect can trigger cascading renders` pada `useEffect` yang memanggil `fetchUsages()` dan `loadOptions()`.

**Kenapa terjadi.**
Fungsi `fetchUsages` memanggil `setLoading(true)` dan `setActionError("")` secara langsung di awal fungsi yang dipicu oleh effect.

**Rekomendasi.**
Rapikan siklus hidup pengambilan data dengan pengaman *isMounted* atau pengkondisian state yang aman untuk mencegah *cascading render*.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa di layar pemakaian alat muncul dua bilah tab yang isinya sama persis?
>
> **J:** Hal tersebut terjadi karena sistem ruang kerja keperawatan induk sudah menyediakan bilah tab horizontal terpadu di bagian atas, namun komponen pemakaian alat di dalamnya secara keliru merender bilah tab kedua miliknya sendiri. Melalui perbaikan ini, bilah tab kedua dihapus sehingga perawat hanya menggunakan satu bilah tab resmi yang bersih di bagian atas.
>
> **T:** Apakah pembatalan pemakaian alat medis tetap aman dan tidak merusak tagihan pasien?
>
> **J:** Sangat aman. Sistem tetap mengunci aturan keselamatan finansial (`CLI-EQP-002`). Jika tagihan rawat inap pasien sudah difinalkan oleh kasir, pemakaian alat tidak dapat dibatalkan secara sepihak oleh perawat bangsal dan sistem akan menampilkan pesan penolakan resmi.

---

## 6. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Dokumen issue dibuat berdasarkan telaah tangkapan layar pengguna; 3 temuan utama dan 2 temuan tambahan dirumuskan beserta solusinya. Status: SELESAI. | `diagnose-module-issue` (Antigravity) |
