# ISSUE-002 — Tampilan Modal Formulir Permintaan Hemodialisa Sempit, Berantakan, dan Terpotong Vertikal Akibat Salah Penggunaan ConfirmModal

```yaml
issue_id: ISSUE-HMD-002
module_id: hemodialisa
layar: "Ruang Kerja Rawat Inap — Penunjang Medis → Modal Formulir Permintaan Hemodialisa (FE-HMD-07 / FE-HMD-12)"
sumber_laporan: "Laporan pengguna 06-10-2026: 'perbaiki tampilan ya pada modal ini', disertai tangkapan layar modal formulir permintaan hemodialisa yang sempit, berdesakan, dan terpotong"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-002-tampilan-modal-permintaan-hemodialisa.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Laporan pengguna menyoroti ketidaknyamanan visual dan tata letak yang berantakan pada **Modal Formulir Permintaan Hemodialisa** di ruang kerja rawat inap (baik di layar perawat maupun dokter). Berdasarkan tangkapan layar dan penelusuran kode sumber, ditemukan bahwa formulir input klinis yang kompleks ini dibungkus menggunakan komponen `ConfirmModal` (`src/components/features/base-features/confirm-modal.jsx`), yang secara desain diperuntukkan khusus bagi kotak dialog konfirmasi hapus/peringatan sederhana dengan batas lebar paten (*hardcoded*) hanya **430 piksel** (`modal-confirm.css:39`).

Akibat keterbatasan lebar 430px tersebut, seluruh elemen formulir—meliputi ringkasan pasien terkunci, sakelar Cito, tanggal tindakan, tipe akses vaskular, 6 tombol preset indikasi klinis, textarea alasan klinis, dan instruksi bangsal—terpaksa berdesakan secara ekstrem dalam satu kolom sempit. Tombol preset indikasi klinis bertumpuk vertikal menjadi 6 baris penuh yang mendorong textarea dan tombol pengiriman ke bawah hingga terpotong dan memicu scrollbar vertikal ganda di dalam modal. Di samping itu, judul modal dan teks pesan terpusat ke tengah (*centered*) dengan ikon bulat `(i)` mengambang di atasnya, menciptakan kesan dialog peringatan alih-alih formulir pemesanan tindakan medis yang profesional.

Isu ini berstatus keparahan **Medium** karena fungsi penyimpanan data masih dapat berjalan jika pengguna men-scroll ke bawah, namun pengalaman interaksi klinis (*usability & ergonomics*) dokter dan perawat menjadi sangat buruk, rawan salah klik, dan tidak mencerminkan estetika premium sistem rumah sakit Quilvian.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "perbaiki tampilan ya pada modal ini" | Tangkapan layar ruang kerja rawat inap menu *Penunjang Medis* → sub-tab *Hemodialisa*, memperlihatkan modal *Formulir Permintaan Hemodialisa* yang sangat sempit (lebar 430px), berdesakan vertikal, tombol pilihan cepat indikasi menumpuk 6 baris ke bawah, dan footer modal terpotong di bagian bawah peramban. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-HMD-002-01` | 1 | Salah penggunaan `ConfirmModal` yang membatasi lebar modal secara paksa pada 430px | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-HMD-002-01` |
| `ISS-HMD-002-02` | 1 | Tata letak isian dalam modal sempit, sesak (*cramped*), dan terpotong vertikal | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-HMD-002-01` |
| `ISS-HMD-002-03` | 1 | Pilihan cepat indikasi klinis bertumpuk vertikal 6 baris penuh memakan ruang layar | `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-HMD-002-02` |
| `ISS-HMD-002-T1` | — | Judul dan ikon modal terpusat di tengah menyerupai dialog konfirmasi hapus data | `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-HMD-002-01` |

---

## 4. Rincian per Temuan

### ISS-HMD-002-01 — Salah Penggunaan ConfirmModal yang Membatasi Lebar Modal pada 430px

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-HMD-002-01` |

**Apa yang terjadi.**
Modal formulir permintaan hemodialisa muncul di tengah layar dengan ukuran yang sangat ramping/sempit (lebar ~430 piksel). Meskipun komponen memanggil prop `size="lg"`, ukuran fisiknya tidak berubah karena styling internal memaksanya tetap kecil. Bagian atas modal memunculkan ikon lingkaran `(i)` mengambang di atas judul yang berada di tengah.

**Kenapa terjadi.**
Pada `hemodialysis-order-modal.jsx` baris 127–142, pengembang menggunakan komponen `ConfirmModal`:

```jsx
// src/.../supporting-service/hemodialysis-order-modal.jsx:127
<ConfirmModal
  show={show}
  variant={isCito ? "danger" : "primary"}
  title="Formulir Permintaan Hemodialisa"
  message="Kirim permintaan tindakan cuci darah ke unit Hemodialisa dengan konteks rekam medis rawat inap terkunci."
  size="lg"
...
```

Namun, di dalam `src/style/components/features/base-features/modal-confirm.css` baris 38–40:

```css
.region-modal .modal-dialog {
  width: min(100% - 28px, 430px);
}
```

Kelas `.region-modal` secara keras membatasi lebar modal maksimal 430px untuk keperluan popup konfirmasi hapus data (`Hapus data?`). Menggunakan komponen ini untuk formulir klinis berisikan 6+ kontrol input adalah kekeliruan arsitektur pemilihan komponen (*component misuse*).

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Blueprint `03-frontend-architecture.md` dan task `FE-HMD-07` menuntut modal formulir permintaan hemodialisa yang terpadu dan nyaman digunakan perawat/dokter bangsal.

**Dampak nyata.**
Dokter atau perawat yang mengisi permintaan di bangsal merasa form berdesakan, sulit membaca teks indikasi klinis, dan harus berhati-hati agar tidak salah memilih opsi.

**Rekomendasi.**
Ganti pembungkus `ConfirmModal` dengan komponen modal formulir standar (`react-bootstrap/Modal` atau modal form standar Quilvian) dengan ukuran dialog `size="lg"` (lebar 750px–800px), header modal rata kiri yang elegan dengan tombol close `[x]`, serta footer terpadu.

---

### ISS-HMD-002-02 — Tata Letak Isian Dalam Modal Sempit dan Terpotong Vertikal

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-HMD-002-01` |

**Apa yang terjadi.**
Di dalam modal yang sempit, isian Tanggal Permintaan dan Tipe Akses Vaskular dipaksa berdampingan lewat kelas `row g-3 col-md-6`, sehingga label dan dropdown menjadi terjepit. Textarea catatan bangsal dan tombol submit di bawah terdorong ke luar batas pandang layar (*viewport cutoff*).

**Kenapa terjadi.**
Ketika lebar modal dinaikkan ke ukuran standar `size="lg"` (~750–800px), grid 2 kolom (`col-md-6`) untuk Tanggal Permintaan dan Tipe Akses Vaskular akan memiliki ruang horizontal yang sangat lapang (~350px per kolom), sehingga teks dan dropdown tidak lagi terpotong.

**Rekomendasi.**
Dengan memperluas modal ke `size="lg"`, tata letak form disusun rapi menjadi 4 bagian berjenjang:
1. Kartu Info Pasien & DPJP (1 baris ringkas, bukan kotak tebal)
2. Sakelar Status Urgensi Cito / Rutin (kompak)
3. Grid 2 Kolom: Tanggal Tindakan & Akses Vaskular
4. Indikasi Klinis (Pilihan Preset Grid + Textarea) & Catatan Tambahan.

---

### ISS-HMD-002-03 — Pilihan Cepat Indikasi Klinis Bertumpuk Vertikal 6 Baris Penuh

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-HMD-002-02` |

**Apa yang terjadi.**
Enam tombol pilihan cepat indikasi klinis (`+ AKI Stadium 3`, `+ CKD Stage 5 on HD Reguler`, `+ Hiperkalemia Refrakter`, `+ Edema Paru Akut`, `+ Asidosis Metabolik Berat`, `+ Sindrom Uremikum`) menumpuk vertikal ke bawah, masing-masing memakan 1 baris penuh, sehingga menyita tinggi sekitar 180 piksel hanya untuk tombol pilihan cepat.

**Kenapa terjadi.**
Pada `hemodialysis-order-modal.jsx` baris 240–254:
```jsx
<div className="d-flex flex-wrap gap-1 mb-2">
  {HMD_CLINICAL_INDICATION_PRESETS.map((preset) => (
    <BaseButton ... className="py-0 px-2" style={{ fontSize: "11px" }}>
      + {preset.label}
    </BaseButton>
  ))}
</div>
```
Karena lebar wadah modal hanya 430px dan teks masing-masing preset cukup panjang (misal: "Hiperkalemia Refrakter (>6.5 mEq/L)"), tombol tidak muat bersanding sehingga wrap ke baris baru satu per satu.

**Rekomendasi.**
Pada modal lebar `size="lg"`, tombol preset dapat mengalir secara alami dalam 2–3 baris horizontal rapi. Tombol diberi gaya pill outline yang bersih (`variant="outline"`), mudah diklik, dan tidak mendominasi ruang vertikal form.

---

### ISS-HMD-002-T1 — Judul dan Ikon Modal Terpusat di Tengah Menyerupai Dialog Peringatan

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-HMD-002-01` |
| **Kenapa dicantumkan** | Terlihat pada tangkapan layar pelapor bahwa bagian atas modal tampak canggung dengan lingkaran `(i)` di atas judul. |

**Apa yang terjadi.**
Modal formulir klinis memiliki ikon lingkaran `(i)` besar di atas judul dan teks judul berada di tengah (*text-align: center*). Ini adalah gaya visual dialog hapus/konfirmasi, bukan gaya formulir entri data medis rumah sakit.

**Rekomendasi.**
Ubah struktur header modal menjadi header standar:
- Ikon tematik hemodialisa (`RiDropLine` merah/teal) berdampingan dengan judul di sisi kiri: `🩸 Formulir Permintaan Hemodialisa`.
- Subtitle penjelas di bawah judul: `Pemesanan jadwal tindakan cuci darah ke Unit Hemodialisa dengan konteks rawat inap.`
- Tombol silang `[×]` di pojok kanan atas untuk menutup modal dengan cepat.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa tampilan modal permintaan hemodialisa saat ini sangat sempit dan tombol-tombolnya menumpuk vertikal ke bawah?
>
> **J:** Karena modal tersebut saat ini memakai komponen `ConfirmModal` yang dirancang untuk popup konfirmasi penghapusan data dengan lebar paten hanya 430 piksel. Akibatnya, formulir klinis yang panjang ini terperangkap di dalam kotak sempit. Solusinya adalah menggantinya dengan `Modal` standar ukuran besar (`size="lg"` / 750px–800px) sehingga seluruh elemen dapat ditata lega dalam grid 2 kolom yang rapi.

---

## 6. Temuan Tambahan

Seluruh temuan tambahan (`ISS-HMD-002-T1`) telah diuraikan pada bagian 4 dan diselesaikan bersama pada paket perbaikan `FIX-HMD-002-01`.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik Modul (Product Owner / Domain Lead)

| No | Keputusan yang dibutuhkan | Rekomendasi | Opsi lain | Dampak bila ditolak |
| :---: | --- | --- | --- | --- |
| K-01 | Pengesahan migrasi modal permintaan hemodialisa dari `ConfirmModal` ke modal dialog formulir klinis standar berukuran `lg` | **Setujui Opsi A**: Gunakan `Modal` (size `lg`) dengan header kiri, tata letak 2 kolom lapang, dan footer standar. | Opsi B: Pertahankan `ConfirmModal` tetapi override CSS width-nya menjadi 750px. | Struktur ikon mengambang di tengah dan perataan teks `ConfirmModal` tetap merusak ergonomi form klinis. |

---

## 8. Catatan Pola

1. **Pola "ConfirmModal Misuse":** Seringkali pengembang menggunakan `ConfirmModal` hanya karena komponen itu sudah memiliki prop `onConfirm` dan `onCancel`, tanpa menyadari bahwa stylesheet `modal-confirm.css` membatasi lebar maksimal 430px dan memaksa perataan tengah.
2. **Pencegahan:** Formulir input berjenjang wajib memakai modal formulir standar (`react-bootstrap/Modal` dengan `Modal.Header`, `Modal.Body`, `Modal.Footer`), bukan dialog konfirmasi aksi.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Dokumen issue dibuat berdasarkan tangkapan layar pengguna; 3 temuan utama dan 1 temuan tambahan dirinci dengan status TERBUKA. | `diagnose-module-issue` (Antigravity) |
| 2026-10-06 | Perbaikan diselesaikan via PLAN-REPAIR-HMD-002 (migrasi ke Modal lg dan chip preset horizontal). Status ditutup: SELESAI. | Antigravity Builder |
