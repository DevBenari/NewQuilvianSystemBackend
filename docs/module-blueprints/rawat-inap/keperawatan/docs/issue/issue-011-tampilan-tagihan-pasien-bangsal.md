# ISSUE-011 — Inkonsistensi Banner Privasi Kasir, Pemalsuan Subtotal "Rp 0" saat Kalkulasi Belum Ada, dan Evaluasi Hak Akses Nominal pada Tagihan Pasien Bangsal

```yaml
issue_id: ISSUE-KEP-011
module_id: rawat-inap
submodule: keperawatan
layar: "Ruang Kerja Keperawatan — Seksi Tagihan Pasien (FE-KEP-23 / FE-KEP-07)"
sumber_laporan: "Laporan pengguna 06-10-2026: 'perbaiki tampilan tagihan pasien', disertai tangkapan layar dengan sorotan cyan pada waktu penilaian kasir dan pesan privasi rupiah bangsal"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-011-tampilan-tagihan-pasien-bangsal.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Laporan pengguna menyoroti ketidaksesuaian dan kejanggalan pada seksi **Tagihan Pasien** di dalam ruang kerja keperawatan rawat inap (`nursing-workspace`). Berdasarkan tangkapan layar yang diserahkan pelapor, terdapat dua sorotan khusus berwarna biru muda (*cyan*): teks *"Waktu penilaian kasir belum tersedia"* pada kartu status kasir dan teks *"Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir."* Tepat di bawah peringatan tersebut, antarmuka justru menampilkan teks nominal Rupiah berupa *"Subtotal: Rp 0"* pada kelompok Tindakan, Penunjang Medis, dan Obat & Alkes, serta panel besar bertuliskan *"Total Tagihan Berjalan: Rp 0"*.

Penelusuran mendalam terhadap kode sumber backend dan frontend mengungkap bahwa kejanggalan ini dipicu oleh tiga faktor utama:
1. **Pemalsuan Nilai "Rp 0" Palsu:** Backend secara sah mengirimkan nilai `null` untuk subtotal dan total berjalan karena kasir/sistem billing belum menjalankan kalkulasi resmi (`BilCalculationVersion` belum terbentuk, `calculatedAt: null`). Utilitas formatter frontend `formatBillingIdr(null)` secara disiplin mengembalikan `null` untuk mencegah kepalsuan nol rupiah. Namun komponen antarmuka `nursing-billing-section.jsx` menimpanya dengan fallback ekspresi `|| "Rp 0"`, sehingga tindakan klinis yang sebenarnya berbiaya tampak seolah-olah gratis.
2. **Kontradiksi Pesan Privasi Kasir:** Komponen kartu kasir bersama (`BillingSummaryCard` / `FE-INT-06`) meng-hardcode pesan privasi bahwa layar bangsal tidak menampilkan rupiah. Ketika dibuka oleh pengguna yang memiliki hak akses `PatientBillingSummary:ViewAmount`, seksi tagihan di bawahnya merender subtotal dan total, sehingga dua kalimat yang saling bertentangan hadir dalam satu pandangan mata.
3. **Pengecekan Hak Akses Belum Ketat:** Hak akses pembacaan nominal `PatientBillingSummary:ViewAmount` dievaluasi tanpa menunggu status pemuatan selesai (`viewAmountLoaded`), sehingga saat state permission belum siap, hook `usePermission` secara permisif mengembalikan `true`.

Isu ini berstatus keparahan **High** karena menampilkan informasi tagihan yang salah ("Rp 0") kepada staf rumah sakit, menimbulkan risiko sengketa biaya dengan keluarga pasien, dan menciptakan kebingungan operasional mengenai kepatuhan privasi finansial di bangsal rawat inap.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "perbaiki tampilan tagihan pasien" | Tangkapan layar antarmuka `media_1791282426666.png` yang memperlihatkan seksi Tagihan Pasien Bangsal dengan dua sorotan cyan pada kartu status kasir: (1) `Waktu penilaian kasir belum tersedia` dan (2) `Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir.`, sementara bagian bawah menampilkan `Subtotal: Rp 0` pada 3 kelompok dan kartu `Total Tagihan Berjalan: Rp 0`. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-KEP-011-01` | 1 | Pemalsuan nilai "Rp 0" palsu pada subtotal kelompok dan total tagihan saat kalkulasi kasir belum tersedia | `BUG` | Frontend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-011-01` |
| `ISS-KEP-011-02` | 1 | Kontradiksi langsung antara banner privasi kasir "Layar bangsal tidak menampilkan rupiah" dengan tampilan nominal subtotal/total | `BUG` / `DESIGN_CHANGE` | Frontend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-011-02` |
| `ISS-KEP-011-03` | 1 | Ambiguasi keterangan "Waktu penilaian kasir belum tersedia" tanpa konteks operasional alur kepulangan | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-011-03` |
| `ISS-KEP-011-04` | 1 | Duplikasi tombol pemuatan ulang ([Perbarui Status] dan [Muat Ulang]) dengan siklus hidup terpisah | `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-011-04` |
| `ISS-KEP-011-05` | 1 | Evaluasi izin `PatientBillingSummary:ViewAmount` tidak memvalidasi status selesai muat (`viewAmountLoaded`) | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-KEP-011-05` |

---

## 4. Rincian per Temuan

### ISS-KEP-011-01 — Pemalsuan Nilai "Rp 0" Palsu pada Subtotal Kelompok dan Total Tagihan saat Kalkulasi Kasir Belum Tersedia

| | |
| --- | --- |
| **No. laporan** | 1 (Area Subtotal dan Total Tagihan Berjalan) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source (`nursing-billing-section.jsx:233`, `:304`); DARI-CAPTURE |
| **Perbaikan** | `FIX-KEP-011-01` |

**Apa yang terjadi.**
Pada layar Tagihan Pasien, pasien telah menerima sejumlah tindakan klinis nyata: pemasangan infus 2 unit, monitor EKG 1 unit, pemeriksaan darah tepi 1 unit, dan pemberian tablet paracetamol 10 unit. Namun, subtotal untuk masing-masing kelompok Tindakan, Penunjang Medis, dan Obat & Alkes tertulis *"Subtotal: Rp 0"*, dan kartu total bawah tertulis *"Total Tagihan Berjalan: Rp 0"*. Layar memberikan ilusi seolah-olah seluruh pelayanan tersebut tidak dipungut biaya atau bernilai nol rupiah.

**Kenapa terjadi.**
Rantai sebab dari gejala ke akar masalah:
1. Pasien baru dirawat dan item pelayanan telah tercatat pada faktur tagihan (`BilInvoiceItems`), namun petugas kasir belum pernah melakukan eksekusi kalkulasi tarif resmi (`BilCalculationVersions` belum terbentuk, sehingga `CurrentCalculationVersion` null/0).
2. Di backend `PatientBillingSummaryService.Breakdown.cs:71`, pemanggilan `ReadCalculationAsync(invoice, ct)` memulangkan `null`.
3. Di baris 115, flag `unavailable` bernilai `true`. Akibatnya, baris 143 mengembalikan total `null`, dan metode `GetBreakdownAmountsAsync` di baris 48–52 mengembalikan `RunningTotalAmount: null` serta `SubtotalAmount: null` untuk setiap kelompok layanan. Hal ini sah dan sesuai kontrak backend: server tidak mengarang nominal nol jika belum ada kalkulasi.
4. Di utilitas frontend `patient-billing-summary-utils.js:18-21`, fungsi `formatBillingIdr(null)` secara disiplin mengembalikan `null` agar pemanggil tidak menampilkan data palsu.
5. Namun di komponen antarmuka `src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx`, pengembang menulis kode berikut:
   ```jsx
   // Baris 231-233
   {canViewAmount && subtotalAmount !== undefined && (
     <div className="fw-bold text-dark font-monospace" data-testid={`subtotal-${group.groupCode}`}>
       Subtotal: {formatBillingIdr(subtotalAmount) || "Rp 0"}
     ...
   // Baris 297-305
   {canViewAmount && amountsData?.runningTotalAmount !== undefined && (
     ...
     <div className="fs-4 fw-bold font-monospace">
       {formatBillingIdr(amountsData.runningTotalAmount) || "Rp 0"}
     </div>
   ```
   Di JavaScript, `null !== undefined` bernilai `true`. Dan karena `formatBillingIdr(null)` menghasilkan `null`, operator logika `|| "Rp 0"` memaksa nilai yang belum dihitung tersebut menjadi string teks `"Rp 0"`.

**Apakah ini menyimpang dari desain?**
Menyimpang keras (`BUG`). Pada blueprint `03-frontend-architecture.md` baris 722 dan laporan task `FE-RWI-185.md` AC-3, disebutkan secara eksplisit: *"Tagihan belum terbentuk tampil sebagai pesan resmi, BUKAN Rp 0... Tidak pernah 'Rp 0' dan tidak ada tempat kosong yang menyiratkan nol"*.

**Dampak nyata.**
Jika perawat atau staf admisi membacakan nilai ini kepada pasien (misalnya Tn. Rian yang dirawat di bangsal), keluarga pasien akan mengira biaya rawat inap mereka Rp 0. Saat pasien hendak pulang dan loket kasir menyodorkan tagihan asli jutaan rupiah, keluarga pasien akan protes keras dan menuduh rumah sakit melakukan kecurangan.

**Rekomendasi.**
Hapus fallback `|| "Rp 0"`. Bila `subtotalAmount === null` atau `runningTotalAmount === null` namun izin `ViewAmount` aktif, render lencana status netral berbunyi *"Menunggu kalkulasi kasir"* atau *"Belum dihitung"*.

---

### ISS-KEP-011-02 — Kontradiksi Langsung Antara Banner Privasi Kasir "Layar bangsal tidak menampilkan rupiah" dengan Tampilan Nominal Subtotal/Total

| | |
| --- | --- |
| **No. laporan** | 1 (Highlight 2 pelapor) |
| **Jenis** | `BUG` / `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source (`billing-summary-card.jsx:156`, `nursing-billing-section.jsx:209`); DARI-CAPTURE |
| **Perbaikan** | `FIX-KEP-011-02` |

**Apa yang terjadi.**
Pada bagian atas layar, di dalam kartu izin kasir, terdapat teks catatan privasi berwarna abu-abu: *"Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir."* Tepat di bawahnya, ada kotak biru info pedoman etika: *"Perawat dilarang memperdebatkan atau merinci harga layanan kepada pasien atau keluarga."*
Namun tepat di bawah kedua peringatan larangan tersebut, antarmuka justru menampilkan rincian nominal Rupiah (Subtotal kelompok dan kartu Total Tagihan Berjalan).

**Kenapa terjadi.**
Rantai sebab dari gejala ke akar masalah:
1. Komponen `BillingSummaryCard` (`FE-INT-06`) dirancang sebagai kartu bersama untuk Detail Episode Rawat Inap (`FE-INP-04`) dan alur kepulangan. Di berkas `inpatient-billing-status-constants.js:59`, konstanta `CASHIER_STATUS_MESSAGES.PRIVACY` diisi teks statis: *"Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir."*
2. Kartu ini diikutsertakan di bagian paling atas `NursingBillingSection` (baris 154) untuk memantau izin pulang kasir secara *real-time*.
3. Pada saat yang sama, arsitektur `FE-RWI-185` (dan `FE-KEP-23`) mengizinkan pemegang wewenang khusus `PatientBillingSummary:ViewAmount` (seperti kepala ruangan rawat inap atau supervisor administrasi) untuk memantau subtotal kelompok dan total berjalan.
4. Karena `BillingSummaryCard` tidak menerima atau memeriksa status hak akses pengguna, teks larangan rupiah tetap muncul secara statis di atas, bertolak belakang dengan angka rupiah yang muncul di bawahnya.

**Apakah ini menyimpang dari desain?**
Menyimpang (`BUG` / `DESIGN_CHANGE`). Desain menghendaki privasi ketat untuk perawat pelaksana biasa (tidak boleh melihat rupiah sama sekali), namun mengizinkan angka agregat bagi petugas berwenang. Hadirnya banner yang menyatakan layar tidak menampilkan rupiah sementara layar menampilkan rupiah adalah cacat logika penyajian antarmuka.

**Dampak nyata.**
Petugas bangsal dan staf IT internal bingung apakah antarmuka ini melanggar regulasi privasi rumah sakit ataukah ada bug kebocoran data nominal.

**Rekomendasi.**
Jadikan catatan privasi pada `BillingSummaryCard` bersifat kontekstual atau operasikan prop pengendali:
- Bila pengguna adalah perawat pelaksana biasa (`!canViewAmount`): tampilkan kalimat penegasan privasi tersebut, dan pastikan tidak ada subtotal/total rupiah yang muncul di bawah.
- Bila pengguna memiliki izin `ViewAmount`: ganti catatan tersebut menjadi keterangan resmi: *"Subtotal dan total tagihan berjalan ditampilkan untuk pemantauan administratif pemegang wewenang. Rincian tarif per item tetap steril di kasir."*

---

### ISS-KEP-011-03 — Ambiguasi Keterangan "Waktu penilaian kasir belum tersedia" Tanpa Konteks Operasional Alur Kepulangan

| | |
| --- | --- |
| **No. laporan** | 1 (Highlight 1 pelapor) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source (`billing-summary-card.jsx:136-138`); DARI-CAPTURE |
| **Perbaikan** | `FIX-KEP-011-03` |

**Apa yang terjadi.**
Pelapor menyoroti teks *"Waktu penilaian kasir belum tersedia"* yang terletak di samping tombol `[Perbarui Status]`. Teks ini disajikan secara pasif dengan gaya tulisan kecil abu-abu di footer kartu, tanpa memberi kejelasan kepada perawat mengapa waktu tersebut belum ada dan apakah ada tindakan yang perlu dilakukan.

**Kenapa terjadi.**
Di `billing-summary-card.jsx` baris 136–138:
```jsx
{billingStatus?.evaluatedAt
  ? `Dinilai kasir ${formatDateTime(billingStatus.evaluatedAt)}`
  : "Waktu penilaian kasir belum tersedia"}
```
Ketika episode rawat inap masih aktif berjalan dan dokter belum menerbitkan rencana pemulangan (*discharge planning*), kasir memang belum melakukan evaluasi kelayakan finansial kepulangan (*financial clearance*). Status ini wajar secara bisnis rumah sakit, namun disajikan seolah-olah merupakan data yang hilang atau tidak lengkap.

**Apakah ini menyimpang dari desain?**
Tidak menyimpang dari kode awal, namun merupakan kelemahan penyajian informasi operasional (`DESIGN_CHANGE`).

**Dampak nyata.**
Perawat pemula mengira ada kegagalan sinkronisasi data dengan sistem kasir, sehingga berkali-kali menekan tombol `[Perbarui Status]` tanpa hasil.

**Rekomendasi.**
Perjelas teks status saat `evaluatedAt` belum ada: *"Belum dievaluasi kasir (evaluasi dilakukan saat alur kepulangan dimulai)"* atau sertakan lencana status *"Menunggu proses pulang"*.

---

### ISS-KEP-011-04 — Duplikasi Tombol Pemuatan Ulang ([Perbarui Status] dan [Muat Ulang]) dengan Siklus Hidup Terpisah

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source (`billing-summary-card.jsx:142-152`, `nursing-billing-section.jsx:190-202`); DARI-CAPTURE |
| **Perbaikan** | `FIX-KEP-011-04` |

**Apa yang terjadi.**
Di bagian atas layar terdapat tombol `[Perbarui Status]` (milik kartu kasir) dan tepat beberapa sentimeter di bawahnya terdapat tombol `[Muat Ulang]` (milik header seksi Tagihan Pasien).

**Kenapa terjadi.**
`BillingSummaryCard` adalah komponen mandiri yang memiliki fungsi `refresh` sendiri yang terikat pada hook `useInpatientBillingStatus`. Sementara itu, `NursingBillingSection` memiliki tombol `[Muat Ulang]` sendiri yang memanggil `fetchBillingBreakdown`.

**Apakah ini menyimpang dari desain?**
Kelemahan ergonomi antarmuka pengguna (`DESIGN_CHANGE`). Pengguna rumah sakit mengharapkan satu tombol penyegaran untuk satu layar kerja.

**Dampak nyata.**
Jika perawat mengklik `[Perbarui Status]`, rincian tindakan klinis di tabel bawah tidak disegarkan. Sebaliknya jika mengklik `[Muat Ulang]`, status kasir di kartu atas tidak disegarkan.

**Rekomendasi.**
Satukan aksi penyegaran: tombol `[Muat Ulang]` pada header utama memicu penyegaran rincian tagihan sekaligus memicu `refresh()` pada kartu status kasir melalui callback ref atau event terkoordinasi.

---

### ISS-KEP-011-05 — Evaluasi Izin `PatientBillingSummary:ViewAmount` Tidak Memvalidasi Status Selesai Muat (`viewAmountLoaded`)

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source (`nursing-billing-section.jsx:56-59`, `use-permission.jsx:23-28`, `permission-slice.jsx:311-318`) |
| **Perbaikan** | `FIX-KEP-011-05` |

**Apa yang terjadi.**
Pada `nursing-billing-section.jsx`, hook perizinan dipanggil sebagai berikut:
```javascript
const {
  allowed: canViewAmount,
  loaded: viewAmountLoaded,
} = usePermission("PatientBillingSummary", "ViewAmount");
```
Namun dalam fungsi `fetchBillingBreakdown` (baris 84), kode langsung mengevaluasi `if (canViewAmount)`. Mengacu pada arsitektur `permission-slice.jsx` baris 23–26 dan 311–318, `selectHasPermission` secara default mengembalikan `true` selama data perizinan **belum termuat** (`!loaded`) sebagai jaring pengaman agar tombol tidak hilang tiba-tiba jika fetch tertunda.

**Kenapa terjadi.**
Pengembang tidak menggabungkan `viewAmountLoaded && canViewAmount`. Akibatnya, pada beberapa milidetik awal saat perawat biasa membuka layar dan daftar permission sedang diambil dari server, sistem sempat menganggap perawat tersebut berhak melihat nominal dan mengirim permintaan HTTP ke endpoint `/amounts`.

**Apakah ini menyimpang dari desain?**
Menyimpang (`BUG`). Pada `FE-RWI-185.md` kriteria 1 & 2 dinyatakan tegas: *"Perawat tidak boleh melihat angka rupiah sama sekali dan tidak ada request ke /amounts"*.

**Dampak nyata.**
Perawat pelaksana sempat memicu request `/amounts` yang kemudian ditolak 403 oleh backend atau merender komponen berupiah sesaat (*flicker*) sebelum izin selesai dimuat.

**Rekomendasi.**
Terapkan *strict permission check*:
```javascript
const isAuthorizedViewAmount = Boolean(viewAmountLoaded && canViewAmount);
```
dan gunakan variabel ini untuk seluruh kondisional pemanggilan data dan rendering nominal.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa pada tangkapan layar tertulis "Layar bangsal tidak menampilkan rupiah", tetapi di bawahnya justru muncul "Subtotal: Rp 0" dan "Total Tagihan Berjalan: Rp 0"?
>
> **J:** Hal ini disebabkan oleh dua hal: (1) Akun yang sedang masuk memiliki hak akses `PatientBillingSummary:ViewAmount` (atau status superadmin) sehingga sistem menampilkan komponen nominal, namun banner privasi pada kartu kasir di atasnya bersifat statis dan belum disesuaikan dengan wewenang akun tersebut; (2) Angka "Rp 0" tersebut adalah **angka nol palsu** yang dihasilkan oleh kode frontend `|| "Rp 0"`, padahal sebenarnya kasir belum melakukan kalkulasi tarif (`null`).

> **T:** Apa arti keterangan "Waktu penilaian kasir belum tersedia"?
>
> **J:** Keterangan tersebut berarti pasien masih berada dalam masa perawatan aktif di bangsal dan belum memasuki tahapan kepulangan (*discharge*), sehingga bagian kasir/keuangan rumah sakit belum melakukan verifikasi akhir izin pulang (*financial clearance*).

---

## 6. Temuan Tambahan

Tidak ada temuan tambahan di luar seksi Tagihan Pasien dan kartu status kasir.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor:

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| :---: | --- | --- | --- |
| P-01 | Apakah akun pengguna pada tangkapan layar tersebut adalah akun Perawat Bangsal atau akun Supervisor/Admisi? | Untuk memastikan apakah komponen subtotal/total rupiah seharusnya muncul atau sepenuhnya hilang bagi peran akun tersebut. | Jika akun tersebut adalah perawat biasa, perbaikan akan memastikan seluruh elemen rupiah tersembunyi 100% tanpa flicker. |

### Untuk Pemilik Sistem:

| No | Keputusan | Pilihan | Rekomendasi | Status Keputusan |
| :---: | --- | --- | --- | --- |
| K-01 | Format tampilan subtotal saat kasir belum melakukan kalkulasi (`subtotalAmount === null`) bagi pemegang izin ViewAmount | (A) Tampilkan lencana teks `"Belum dikalkulasi"`<br/>(B) Tampilkan tanda strip `"—"`<br/>(C) Sembunyikan baris subtotal sampai kalkulasi ada | **Pilihan A** (Lencana `"Belum dikalkulasi"` agar informatif dan transparan) | ✅ DISETUJUI oleh Pemilik Sistem (2026-10-07) |
| K-02 | Penyesuaian pesan privasi kasir saat dibuka oleh pemegang izin `ViewAmount` | (A) Sembunyikan pesan privasi kasir<br/>(B) Ubah pesan menjadi penegasan pengawasan administratif | **Pilihan B** (Menegaskan bahwa nominal hanya untuk pemantauan plafon/biaya) | ✅ DISETUJUI oleh Pemilik Sistem (2026-10-07) |

---

## 8. Catatan Pola

Masalah "Rp 0 Palsu" (*fake zero currency*) adalah pola kesalahan berulang yang pernah terjadi pada beberapa modul lain di Quilvian System (misalnya pada `detail-invoice-billing-view.jsx:135` dan `cashier-billing-overview.test.mjs:358`). Pola ini selalu bersumber dari penggunaan operator fallback JavaScript `|| "Rp 0"` terhadap nilai `null` atau `undefined` yang seharusnya merepresentasikan kondisi *belum ada data* atau *belum dihitung*. Pencegahan menyeluruh dapat dilakukan dengan mewajibkan penggunaan helper resmi tanpa fallback liar.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | :---: |
| 2026-10-06 | Dokumen issue dibuat dari laporan pengguna dan tangkapan layar seksi Tagihan Pasien Bangsal | `diagnose-module-issue` |
| 2026-10-07 | Rekomendasi disetujui pengguna; implementasi 5 perbaikan diselesaikan dan diverifikasi passing | `diagnose-module-issue` |
