# ISSUE-004 — Pemesanan Ruangan Bedah: Tab ganda bertumpuk, kontrol input mentah, tabrakan footer, dan form aktif tanpa order dokter

```yaml
issue_id: ISSUE-EPS-004
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Ruang Kerja Keperawatan — Menu Pemesanan Ruangan Bedah (FE-RWI-193 / FE-INP-25 & FE-RWI-194 / FE-INP-26)"
sumber_laporan: "Laporan pelapor 06-10-2026: permintaan perbaikan tampilan menu pemesanan ruangan bedah (bedah operasi dan obgyn), disertai 1 lampiran screenshot tangkapan layar antarmuka"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-004-pemesanan-ruangan-bedah.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Laporan ini menyoroti permasalahan tampilan dan interaksi pengguna pada menu **Pemesanan Ruangan Bedah** (sub-tab Bedah Operasi dan Bedah Obgyn) di Ruang Kerja Keperawatan Rawat Inap (`FE-KEP-07`). Analisis mendalam terhadap tangkapan layar pelapor dan kode sumber frontend (`surgery-booking-form.jsx`, `surgery-booking-section.jsx`, `nursing-workspace-sections.jsx`) mengidentifikasi lima isu utama dan dua temuan penyerta.

Masalah paling mencolok dan membingungkan pengguna adalah keberadaan **tab navigasi ganda yang saling bertumpuk**: tab sekunder bawaan ruang kerja berada tepat di atas kartu formulir, sementara di dalam kartu formulir terdapat sepasang tombol tab lagi dengan visual kotak garis mentah yang tidak tersinkronisasi dengan baik. Selain itu, **bilah kaki aplikasi (footer) menabrak dan menutupi formulir bagian bawah**, sehingga nama penginput, kolom catatan, dan tombol simpan pemesanan terhalang oleh teks hak cipta aplikasi.

Dari segi tata letak klinis, formulir ini memakai **kontrol masukan bawaan peramban (native HTML inputs)** seperti pemilih tanggal-waktu bawaan browser, tombol pilihan bulat (radio) yang sangat rapat tanpa jarak aman, serta kotak dropdown standar yang menyimpang dari katalog komponen antarmuka standar Quilvian. Terakhir, ketika pasien **belum memiliki order tindakan operasi dari dokter**, formulir tetap menyajikan seluruh isian klinis secara terbuka dengan tombol simpan terkunci tanpa kejelasan alur kerja bagi staf perawat.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata asli) | Lampiran yang merujuk |
| :---: | --- | --- |
| 1 | perbaiki tampilan pada menu pemesanan ruangan bedah, bedah operasi dan obygn | Lampiran 1 (`media_1791282368895.png`): Tangkapan layar formulir Pemesanan Ruangan Bedah pasien Budi Santoso |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-004-01` | 1 | Tab ganda bertumpuk dan desinkronisasi tab antara ruang kerja dan form | `BUG` + `DESIGN_CHANGE` | Frontend | High | SUDAH-VERIFIKASI | `FIX-EPS-004-01` |
| `ISS-EPS-004-02` | 1 | Kontrol formulir memakai elemen HTML native yang berdesakan dan menyimpang dari katalog komponen | `RULE_VIOLATION` + `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-004-02` |
| `ISS-EPS-004-03` | 1 | Formulir tetap aktif terbuka meski tindakan operasi belum dipesan dokter | `BUG` + `DESIGN_CHANGE` | Frontend | High | SUDAH-VERIFIKASI | `FIX-EPS-004-03` |
| `ISS-EPS-004-04` | 1 | Tombol "Pesan Ruang Bedah" terkunci mati tanpa indikasi isian yang kurang | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-004-04` |
| `ISS-EPS-004-05` | 1 | Bilah footer aplikasi menabrak dan menutupi bagian bawah formulir pemesanan | `BUG` | Frontend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-004-05` |
| `ISS-EPS-004-T1` | — | Label dokter operator berdempetan tanda minus tanpa spasi pemisah | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-004-06` |
| `ISS-EPS-004-T2` | — | Tabel riwayat kasus operasi pasien terdorong jauh ke bawah dan belum memakai komponen aksi standar | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-004-06` |

Radius dampak: Perubahan hanya menyentuh komponen internal fitur pemesanan ruang bedah keperawatan pada folder `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/` dan tidak mengubah kontrak API backend maupun modul lain di luar rawat inap.

---

## 4. Rincian per temuan

### ISS-EPS-004-01 — Tab ganda bertumpuk dan desinkronisasi tab antara ruang kerja dan form

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` + `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source dan capture pelapor |
| **Perbaikan** | `FIX-EPS-004-01` |

**Apa yang terjadi.** Pada layar pemesanan ruangan bedah, perawat disajikan dua set tab yang saling bertumpuk persis di atas satu sama lain. Di baris teratas (luar kartu putih), terdapat tab sekunder standar ruang kerja bertuliskan **Bedah Operasi** dan **Bedah Obgyn**. Namun tepat di bawah judul kartu putih, muncul lagi sepasang tombol tab dengan bingkai kotak garis bertuliskan **Bedah Operasi** dan **Bedah Obgyn (SC/Caesar)**. Ketika perawat menekan tab di bilah atas, formulir di dalam kartu tidak berganti jenis bedah karena status tab di dalam formulir terisolasi dan tidak mendengarkan perubahan alamat tautan (URL). Sebaliknya, menekan tab di dalam kartu tidak mengubah indikator tab di bilah atas.

**Kenapa terjadi.** Rantai penyebab:
1. `inpatient-nursing-constants.js:124-127` mendefinisikan `tabs` untuk section `surgery-booking` berisi `general-surgery` dan `obgyn-surgery`, sehingga pembungkus `NursingWorkspaceView` secara otomatis merender `NursingSecondaryTabBar` di atas kartu.
2. Komponen `SurgeryBookingSection` (`surgery-booking-section.jsx:20`) menyimpan state lokal `const [currentTab, setCurrentTab] = useState(activeTab || "general-surgery");` hanya pada saat awal render, tanpa `useEffect` yang menyinkronkan ketika prop `activeTab` dari induk berubah.
3. Di dalam formulir `SurgeryBookingForm` (`surgery-booking-form.jsx:260-283`), dibuat lagi tombol navigasi tab manual dengan styling inline Tailwind yang kaku:
```jsx
// surgery-booking-form.jsx:260-283
<div className="inline-flex p-1 bg-slate-200/80 rounded-lg text-xs font-medium">
  <button
    type="button"
    onClick={() => onTabChange && onTabChange("general-surgery")}
    className={`px-3 py-1.5 rounded-md transition-all ${
      !isObgynTab ? "bg-white text-slate-800 shadow-sm font-semibold" : "text-slate-600 hover:text-slate-900"
    }`}
  >
    Bedah Operasi
  </button>
  <button
    type="button"
    onClick={() => onTabChange && onTabChange("obgyn-surgery")}
    className={`px-3 py-1.5 rounded-md transition-all ${
      isObgynTab ? "bg-white text-slate-800 shadow-sm font-semibold" : "text-slate-600 hover:text-slate-900"
    }`}
  >
    Bedah Obgyn (SC/Caesar)
  </button>
</div>
```

**Apakah ini menyimpang dari desain?** Menyimpang (`BUG` + `DESIGN_CHANGE`). Dokumen arsitektur frontend `03-frontend-architecture.md` bagian 13.4.1 (`FE-INP-25`) merancang satu pemilih tab untuk membedakan alur Bedah Operasi dan Bedah Obgyn. Duplikasi visual terjadi akibat penyusunan komponen section yang tidak menyelaraskan diri dengan tab bar global ruang kerja keperawatan.

**Dampak nyata.** Perawat Budi Santoso di bangsal rawat inap ingin memesan jadwal operasi caesar darurat (Obgyn). Perawat mengklik tab "Bedah Obgyn" pada bilah navigasi atas, namun formulir tetap menampilkan mode Bedah Operasi umum dengan pilihan radio Elektif/Darurat. Perawat bingung apakah pesanan akan masuk sebagai kasus kebidanan atau bedah umum.

**Rekomendasi.** Satukan kendali tab:
- Gunakan `NursingSecondaryTabBar` sebagai pengendali utama (single source of truth) dengan URL query sinkron (`?section=surgery-booking&tab=general-surgery` atau `tab=obgyn-surgery`).
- Sinkronkan prop `activeTab` pada `SurgeryBookingSection` secara reaktif.
- Di dalam kartu formulir, hilangkan deret tombol ganda tersebut dan ganti dengan *Badge Konteks Layanan* yang elegan, misalnya badge penanda mode: `Layanan: Bedah Operasi Umum` atau `Layanan: Bedah Obstetri & Ginekologi (SC / Caesar)`.

---

### ISS-EPS-004-02 — Kontrol formulir memakai elemen HTML native yang berdesakan dan menyimpang dari katalog komponen

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `RULE_VIOLATION` + `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan capture pelapor |
| **Perbaikan** | `FIX-EPS-004-02` |

**Apa yang terjadi.** Elemen-elemen pengisian pada formulir pemesanan kamar operasi terlihat kasar, berdesakan, dan menggunakan kontrol bawaan browser:
1. Isian **Tanggal & Jam Operasi** menggunakan tag `<input type="datetime-local">`, menghasilkan kotak masukan bertulisan font peramban yang kaku dan ikon kalender bawaan Windows/Chrome yang tidak serasi dengan tema sistem.
2. Pilihan **Jenis Kasus** (Elektif / Darurat) dan **Sisi Tubuh (Laterality)** (Tidak berlaku / Kiri / Kanan / Bilateral) menggunakan radio button HTML bulat bawaan yang sangat kecil dan menempel langsung ke teks (`oElektif oDarurat` dan `oTidak berlaku oKiri oKanan oBilateral`).
3. Dropdown **Rencana Anestesi** dan **Prioritas** menggunakan `<select>` mentah dengan opsi teks biasa.
4. **Perkiraan Durasi Operasi** menggunakan kotak angka kecil 24 piksel dengan tulisan pembatas `menit (1-1440)` di sebelahnya yang tidak sejajar secara visual.
5. Kotak teks **Indikasi Operasi** dan **Keterangan** hanya disetel setinggi dua baris (`rows="2"`), sangat sempit untuk perawat menulis alasan medis operasi.

**Kenapa terjadi.** Di berkas `surgery-booking-form.jsx:400-566`, pengembang menuliskan elemen HTML primitif tanpa memanfaatkan base component Quilvian atau kelas token desain antarmuka.

Kutipan kode:
```jsx
// surgery-booking-form.jsx:407-413 (Native datetime-local)
<input
  type="datetime-local"
  value={preferredAt}
  onChange={(e) => setPreferredAt(e.target.value)}
  required
  className="w-full text-xs rounded-md border border-slate-300 px-3 py-1.5 bg-white text-slate-800 focus:outline-none focus:ring-2 focus:ring-indigo-500"
/>

// surgery-booking-form.jsx:460-484 (Cramped native radios)
<div className="flex items-center gap-3 py-1.5 text-xs text-slate-700">
  <label className="inline-flex items-center gap-1 cursor-pointer">
    <input type="radio" name="caseType" value="Elective" ... />
    <span>Elektif</span>
  </label>
  <label className="inline-flex items-center gap-1 cursor-pointer">
    <input type="radio" name="caseType" value="Emergency" ... />
    <span>Darurat</span>
  </label>
</div>
```

**Apakah ini menyimpang dari desain?** Melanggar aturan (`RULE_VIOLATION`). Aturan rekayasa `rules/frontend/base-component-catalog.md` dan `rules/frontend/ui-consistency-checklist.md` melarang penggunaan elemen form primitif tak bertema bila katalog komponen bersama tersedia, serta mewajibkan kartu pilihan/radio berjarak aman demi pencegahan salah klik klinis.

**Dampak nyata.** Perawat yang menggunakan komputer bangsal dengan layar sentuh atau resolusi padat kesulitan menekan lingkaran radio Sisi Tubuh (Laterality) karena ukuran tombol yang terlalu kecil, meningkatkan risiko salah memilih laterality bedah (misal tertukar antara Kanan dan Kiri), yang merupakan isu keselamatan pasien (*patient safety incident*).

**Rekomendasi.** 
1. Tata ulang layout formulir menjadi kartu-kartu segmen logis:
   - Segmen 1: Detail Order & Dokter Operator
   - Segmen 2: Waktu & Klasifikasi Prosedur (Tanggal/Jam, Rencana Anestesi, Prioritas)
   - Segmen 3: Karakteristik Klinis Bedah (Jenis Kasus & Sisi Tubuh laterality dibuat berbentuk Radio Pill / Segmented Buttons yang lega dan nyaman diklik)
   - Segmen 4: Durasi, Indikasi Klinis, dan Catatan Persiapan
2. Perbesar tinggi textarea Indikasi dan Catatan menjadi minimal 3-4 baris lengkap dengan penghitung karakter dan label wajib yang jelas.

---

### ISS-EPS-004-03 — Formulir tetap aktif terbuka meski tindakan operasi belum dipesan dokter

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` + `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source dan capture pelapor |
| **Perbaikan** | `FIX-EPS-004-03` |

**Apa yang terjadi.** Pada screenshot pelapor terlihat kotak informasi biru:
> *(i) Tindakan operasi belum dipesan dokter. Minta dokter memesan tindakan lebih dulu melalui CPPT / formulir order tindakan klinis.*

Meskipun kotak pemberitahuan ini muncul, seluruh isian formulir di bawahnya (Dokter Operator, Tanggal/Jam, Anestesi, Prioritas, Kasus, Sisi Tubuh, Durasi, Indikasi, Catatan) tetap tampil terbuka secara utuh. Pengguna dapat mengetik tanggal, memilih anestesi, dan mengetik indikasi, namun saat selesai mereka baru menyadari bahwa tombol simpan di bagian paling bawah terkunci abu-abu dan tidak dapat dikirim.

**Kenapa terjadi.** Di `surgery-booking-form.jsx:320-330`, pengecekan ketiadaan order (`procedures.length === 0`) hanya menggantikan dropdown pemilihan tindakan dengan alert biru. Namun elemen formulir selebihnya di baris 354-566 tetap dirender apa adanya tanpa kondisi penjaga (*guard condition*).

```jsx
// surgery-booking-form.jsx:320-330
procedures.length === 0 ? (
  <div className="p-3 bg-blue-50 border border-blue-200 rounded-md text-xs text-blue-800 flex items-start gap-2">
    <Info className="w-4 h-4 shrink-0 text-blue-600 mt-0.5" />
    <div>
      <p className="font-medium">Tindakan operasi belum dipesan dokter.</p>
      <p className="text-[11px] text-blue-700 mt-0.5">
        Minta dokter memesan tindakan lebih dulu melalui CPPT / formulir order tindakan klinis.
      </p>
    </div>
  </div>
) : ( ... )
```

**Apakah ini menyimpang dari desain?** Menyimpang (`BUG` + `DESIGN_CHANGE`). Berdasarkan invariant domain rawat inap `INV-RWF-25` dan `RWI-DEC-176`, pemesanan ruangan bedah wajib mengikat tepat satu order tindakan operasi aktif. Mengizinkan perawat mengisi seluruh formulir ketika tidak ada order yang bisa dipesan adalah pemborosan waktu kerja perawat dan menyebabkan kebingungan operasional.

**Dampak nyata.** Perawat jaga malam mengisi lengkap rencana operasi, menulis indikasi klinis panjang untuk pasien darurat, lalu mendapati formulir tidak bisa disimpan. Perawat mengira sistem mengalami galat/hang, padahal akar masalahnya adalah dokter DPJP belum memasukkan order tindakan bedah pada rekam medis elektronik.

**Rekomendasi.**
- Jika `procedures.length === 0`, tampilkan **Panel Status Prasyarat Klinis** yang tegas dan informatif.
- Tampilkan tombol aksi panduan: `[ Segarkan Daftar Order ]` agar perawat bisa langsung mengecek ulang setelah dokter selesai membuat order tindakan di CPPT.
- Kunci atau sembunyikan bidang pengisian formulir dengan pesan penjelas bahwa form baru dapat diisi setelah ada order tindakan aktif.

---

### ISS-EPS-004-04 — Tombol "Pesan Ruang Bedah" terkunci mati tanpa indikasi isian yang kurang

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-004-04` |

**Apa yang terjadi.** Tombol "Pesan Ruang Bedah" di bagian bawah dalam kondisi nonaktif (berwarna abu-abu, kursor dilarang) jika salah satu dari sekian banyak kondisi belum terpenuhi, tanpa adanya indikasi isian mana yang kurang atau salah.

**Kenapa terjadi.** Logika `canSubmit` di `surgery-booking-form.jsx:144-167` menggabungkan 9 kondisi boolean:
```jsx
// surgery-booking-form.jsx:144-167
const canSubmit = useMemo(() => {
  if (!canCreateCase) return false;
  if (isDischargePending) return false;
  if (!selectedProcedureId) return false;
  if (!preferredAt) return false;
  if (!plannedAnesthesiaType) return false;
  if (!priority) return false;
  if (!caseType) return false;
  if (!indication.trim()) return false;
  if (estimatedMinutes < 1 || estimatedMinutes > 1440) return false;
  if (submitting) return false;
  return true;
}, [...]);
```
Lalu tombol dirender dengan `disabled={!canSubmit}`:
```jsx
// surgery-booking-form.jsx:586-587
<button type="submit" disabled={!canSubmit} className="... disabled:opacity-50 disabled:cursor-not-allowed">
```

**Apakah ini menyimpang dari desain?** Melanggar aturan (`RULE_VIOLATION`). Prinsip panduan antarmuka Quilvian butir 7.3 secara tegas menyatakan: *"Jangan mematikan tombol tanpa memberi tahu sebabnya. Biarkan tombol dapat ditekan, lalu tunjukkan isian yang kurang."*

**Dampak nyata.** Perawat yang terburu-buru memesan jadwal operasi darurat mengira tombol rusak karena tombol berwarna abu-abu mati, padahal hanya kolom durasi atau indikasi yang belum terisi spasi valid.

**Rekomendasi.**
- Biarkan tombol tetap aktif (hanya dinonaktifkan saat sedang memproses jaringan / `submitting`).
- Ketika tombol diklik sementara ada isian wajib yang belum lengkap, tampilkan pesan validasi inline berwarna merah pada ruas yang bersangkutan dan gulirkan layar secara otomatis ke ruas pertama yang kosong.

---

### ISS-EPS-004-05 — Bilah footer aplikasi menabrak dan menutupi bagian bawah formulir pemesanan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di capture pelapor |
| **Perbaikan** | `FIX-EPS-004-05` |

**Apa yang terjadi.** Pada screenshot pelapor, bilah footer aplikasi (berwarna toska muda dengan tautan *Terms of Use*, *Quilvian System Version 2.2.0*, dan *All Rights Reserved 2026*) melayang tepat di atas formulir bagian bawah. Akibatnya, baris "Penginput SuperAdmin", textarea catatan khusus, dan tombol aksi "Pesan Ruang Bedah" terpotong dan tertutup secara visual.

**Kenapa terjadi.** Bilah footer aplikasi memiliki sifat penataan tetap/melayang di bagian bawah peramban (`iq-footer app-footer`), sementara kontainer kartu formulir `surgery-booking-form.jsx` tidak memiliki ruang bantalan bawah (*safe padding*) yang memadai untuk mencegah elemen terbawah masuk ke balik footer ketika digulir.

**Apakah ini menyimpang dari desain?** Menyimpang (`BUG`). Seluruh elemen fungsional wajib dapat diakses dengan leluasa tanpa tertutup elemen tata letak statis peramban.

**Dampak nyata.** Petugas rawat inap tidak dapat melihat atau mengklik tombol submit dan nama penginput dengan nyaman karena terhalang oleh footer sistem.

**Rekomendasi.**
- Tambahkan padding bawah yang memadai pada kontainer `SurgeryBookingSection` dan pembungkus formulir (misalnya `pb-28` atau margin bawah aman).
- Pastikan area scroll dokumen memperhitungkan tinggi footer aplikasi secara responsif.

---

### ISS-EPS-004-T1 — Label dokter operator berdempetan tanda minus tanpa spasi pemisah

| | |
| --- | --- |
| **Kenapa dicantumkan** | Terlihat jelas pada screenshot pelapor di baris kedua formulir |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source dan capture |
| **Perbaikan** | `FIX-EPS-004-06` |

**Apa yang terjadi.** Pada kartu dokter operator, jika nama dokter belum terpilih, teks yang muncul adalah `-(dari order, tidak dapat diubah)` tanpa ada spasi pemisah antara tanda `-` dan kurung buka `(`.

**Kenapa terjadi.** Di `surgery-booking-form.jsx:362-364`:
```jsx
<span>{operatorDoctorName}</span>
<span className="text-[10px] text-slate-400 font-normal italic">
  (dari order, tidak dapat diubah)
</span>
```
Karena nilai default `operatorDoctorName` adalah `"-"`, kedua tag `span` bersebelahan tanpa spasi horizontal sehingga terender sebagai satu kata bersambung.

**Rekomendasi.** Berikan spasi pemisah atau tampilkan sebagai pill status informatif: `- (dari order, tidak dapat diubah)`.

---

### ISS-EPS-004-T2 — Tabel riwayat kasus operasi pasien terdorong jauh ke bawah dan belum memakai komponen aksi standar

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan selama audit tata letak berkas `patient-surgery-cases-table.jsx` |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-004-06` |

**Apa yang terjadi.** Di bawah formulir pemesanan terdapat tabel `PatientSurgeryCasesTable` (`FE-INP-26`). Namun tabel ini memakai tombol HTML biasa dan jarak vertikal yang kaku, serta pada tangkapan layar pelapor terdorong jauh ke luar area pandang akibat panjangnya formulir kosong di atasnya.

**Kenapa terjadi.** Berkas `patient-surgery-cases-table.jsx:71-79` menggunakan tombol muat ulang `<button>` biasa tanpa `BaseButton`, serta belum terintegrasi secara modular dengan state formulir di atasnya.

**Rekomendasi.** Perbarui tombol aksi menggunakan `BaseButton` dan rapikan jarak antara formulir pemesanan dan tabel riwayat agar proporsional.

---

## 5. Tanya-jawab pelapor

> **T:** Mengapa pada menu pemesanan ruangan bedah ada dua tab Bedah Operasi dan Bedah Obgyn yang muncul ganda?
>
> **J:** Hal itu terjadi karena bilah navigasi tab sekunder ruang kerja keperawatan (`NursingSecondaryTabBar`) sudah otomatis menyediakan tab "Bedah Operasi" dan "Bedah Obgyn" di bagian atas halaman, namun di dalam formulir kartu pemesanan dipasang lagi sepasang tombol tab manual. Karena keduanya tidak saling sinkron, tampilannya terlihat bertumpuk dan tombol tab di dalam kartu justru membingungkan perawat. Kami merekomendasikan untuk menghapus tombol ganda di dalam kartu dan menyelaraskan kendali tab sepenuhnya ke bilah navigasi atas.

> **T:** Mengapa footer aplikasi (Terms of Use / Quilvian System) menutupi tombol simpan di bagian bawah?
>
> **J:** Kontainer formulir pemesanan saat ini belum memiliki ruang pembatas bawah (*bottom safe padding*) yang cukup untuk memperhitungkan keberadaan bilah footer yang melayang (*floating/fixed*). Akibatnya ketika formulir terisi atau saat layar digulir ke bawah, area tombol simpan dan keterangan penginput tertutup oleh footer. Kami akan menambahkan margin dan padding pengaman di bawah formulir agar seluruh tombol aksi selalu bebas dari tumpang-tindih footer.

---

## 6. Temuan tambahan

Semua temuan tambahan telah dicatat secara transparan pada subbagian temuan `ISS-EPS-004-T1` dan `ISS-EPS-004-T2` di bagian 4.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor:
*Tidak ada pertanyaan yang menahan perbaikan (seluruh bukti visual dan teknis sudah terverifikasi secara lengkap).*

### Untuk pemilik:

| No | Keputusan | Pilihan | Rekomendasi | Status Keputusan |
| ---: | --- | --- | --- | --- |
| K-01 | Pengendalian Tab: Apakah tab "Bedah Operasi" dan "Bedah Obgyn" cukup dikendalikan oleh Secondary Tab Bar ruang kerja, atau tetap butuh penanda di dalam kartu? | (A) Kendali tunggal di Tab Bar atas + badge penjelas mode di dalam kartu / (B) Pertahankan tombol tab di dalam kartu dan sembunyikan tab bar atas | **Pilihan A**: Sesuai konsistensi navigasi ruang kerja keperawatan Quilvian | ✅ **DISETUJUI (Pilihan A)** oleh pemilik pada 07-10-2026 |
| K-02 | Perilaku Saat Order Operasi Belum Dipesan Dokter: Apakah formulir disembunyikan sepenuhnya atau ditampilkan dalam keadaan terkunci (read-only)? | (A) Tampilkan panel panduan alur klinis dan sembunyikan formulir isian / (B) Tetap tampilkan isian tetapi seluruhnya dinonaktifkan (disabled) | **Pilihan A**: Lebih bersih, mencegah perawat salah paham, dan langsung mengarahkan perawat untuk mengingatkan dokter DPJP | ✅ **DISETUJUI (Pilihan A)** oleh pemilik pada 07-10-2026 |

---

## 8. Catatan pola

1. **Pola Duplikasi Tab Navigasi**: Sering terjadi saat komponen fitur dirancang secara mandiri dengan tab internal, kemudian dipasang ke dalam wadah shell yang sudah memiliki tab bar otomatis. Pencegahan: Tetapkan aturan arsitektur bahwa komponen section anak tidak boleh membuat tombol tab mandiri bila tab tersebut sudah terdaftar pada konstanta navigasi induk.
2. **Pola Native Controls**: Pengembang sering kali menggunakan elemen HTML dasar (`<input type="datetime-local">`, `<input type="radio">`) untuk mempercepat penyelesaian task, yang kemudian menghasilkan tampilan yang tidak seragam dan melanggar token desain. Pencegahan: Gunakan checklist konsistensi UI sebelum menyatakan task selesai.
3. **Pola Tombol Dimatikan Secara Diam-diam**: Menghambat operasional pengguna di rumah sakit. Tombol harus selalu memberikan umpan balik ketika diklik bila validasi belum terpenuhi.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Dokumen issue dibuat dari laporan masalah tampilan pemesanan ruangan bedah beserta tangkapan layar antarmuka | `diagnose-module-issue` |
| 2026-10-07 | Pemilik menyetujui rekomendasi K-01 (Pilihan A) dan K-02 (Pilihan A); status issue berubah menjadi `DALAM_PERBAIKAN` | Pemilik sistem / User |
