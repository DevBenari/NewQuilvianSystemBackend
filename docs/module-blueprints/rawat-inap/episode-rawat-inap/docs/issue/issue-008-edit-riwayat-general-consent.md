# ISSUE-008 — Formulir General Consent di Workspace PPRI belum bisa dikoreksi dan perubahan persetujuan tidak meninggalkan riwayat

```yaml
issue_id: ISSUE-EPS-008
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Workspace PPRI — menu General Consent, tab Formulir General Consent (FE-INP-36); sumber datanya langkah Formulir Persetujuan & TTD Digital admisi (FE-INP-03)"
sumber_laporan: "Permintaan pemilik sistem 13-10-2026, 1 butir, 1 screenshot tab Formulir General Consent"
tanggal_issue: "2026-10-13"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "4ec02370dfa2160a9b749f507f8ebd45b0a395db (MHamzah); folder InPatientManagement bersih di working tree"
source_sha_frontend: "f4495aa14c067c15e282203533e9c163363528ca (HamzahV2); inpatient-admission-consent-form-step.jsx dan inpatient-consent-form.jsx sedang diubah pekerjaan lain di working tree"
rencana_perbaikan: ../plan-repair/plan-repair-008-edit-riwayat-general-consent.md
```

## 1. Ringkasan

Persetujuan umum rawat inap (General Consent) diisi dan ditandatangani digital oleh pasien atau
walinya pada langkah admisi. Hasilnya tampil di Workspace PPRI, menu General Consent, tetapi hanya
untuk dibaca dan dicetak. Pemilik sistem meminta tab **Formulir General Consent** menjadi **form
edit**. Bila ada data yang salah, petugas dapat mengoreksinya, dan setiap koreksi wajib
meninggalkan riwayat: siapa yang mengubah, kapan, dan apa yang berubah.

Permintaan ini adalah perubahan desain, karena desain yang berlaku menyebut menu tersebut
*read-only / print-ready*. Penelusuran juga menemukan masalah yang lebih berbahaya. Satu-satunya
jalur ubah yang ada saat ini, yaitu menyimpan ulang dari langkah admisi, **menimpa** persetujuan
lama tanpa jejak. Isi lama hilang dan hanya nama pengubah terakhir yang tersisa.

Ada tiga temuan tambahan:

- pilihan "Sudah menerima panduan rawat inap" tidak pernah tersimpan;
- cetakan formulir yang sudah bertanda tangan digital tidak mencentang hubungan penanda tangan;
- tombol simpan langkah admisi dimatikan sampai isian lengkap.

## 2. Laporan asli

| No. laporan | Keluhan pelapor (diringkas) | Lampiran |
| ---: | --- | --- |
| 1 | "Form General Consent sebenarnya nge-link dari admisi, ada surat persetujuan umum. Jadi di situ menjadi form edit saja: ketika di admisi rawat inap sudah diinput, form General Consent bisa diedit jika ada yang salah, tetapi ketahuan riwayat siapa yang melakukan edit." | Screenshot tab Formulir General Consent berstatus "Bertanda Tangan Digital" |

Screenshot memuat nama pasien uji. Dokumen ini memakai nama samaran.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-008-01` | 1 | Tab Formulir General Consent hanya baca; data persetujuan yang salah tidak dapat dikoreksi dari Workspace PPRI | `DESIGN_CHANGE` | FE + BE + dokumen | High | SUDAH-VERIFIKASI | `FIX-EPS-008-01`, `FIX-EPS-008-02`, `FIX-EPS-008-04` |
| `ISS-EPS-008-02` | 1 | Menyimpan ulang persetujuan menimpa isi lama tanpa riwayat | `BUG` | BE + FE | High | SUDAH-VERIFIKASI | `FIX-EPS-008-01`, `FIX-EPS-008-03` |
| `ISS-EPS-008-T1` | — | "Sudah menerima panduan rawat inap" hanya tersimpan di layar | `BUG` | FE + BE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-008-01`, `FIX-EPS-008-02` |
| `ISS-EPS-008-T2` | — | Cetakan formulir bertanda tangan digital tidak mencentang hubungan penanda tangan | `BUG` | FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-008-02` |
| `ISS-EPS-008-T3` | — | Tombol "Simpan & Lanjut ke Cetak" pada langkah admisi mati sampai isian lengkap | `RULE_VIOLATION` | FE | Low | SUDAH-VERIFIKASI | Di luar rencana ini (lihat bagian 6) |

## 4. Rincian per temuan

### ISS-EPS-008-01 — Tab Formulir General Consent hanya baca; data yang salah tidak dapat dikoreksi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend, backend, dan dokumen blueprint |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source dan dokumen desain; tampilan dari screenshot pelapor |
| **Perbaikan** | `FIX-EPS-008-01`, `FIX-EPS-008-02`, `FIX-EPS-008-04` |

**Apa yang terjadi.** Contoh: pada admisi, Ny. Rina Santoso menandatangani persetujuan sebagai istri
Tn. Budi. Petugas salah mengetik NIK-nya (satu angka tertukar). Di Workspace PPRI, tab Formulir
General Consent hanya menampilkan data itu sebagai teks beserta lencana "Tanda Tangan Digital
Tersimpan (Admisi)". Tidak ada tombol untuk memperbaikinya. Satu-satunya jalan adalah kembali ke
langkah admisi, yang tidak lagi tersedia sesudah admisi selesai.

**Kenapa terjadi.**

```text
tab Formulir GC hanya menampilkan teks
  ← general-consent-section.jsx:181-207: bila isConsentSigned, panel <dl> baca-saja; tidak ada aksi
    ← general-consent-section.jsx:68-74: "cetak saja … Tidak ada tombol Simpan dan tidak ada satu
      pun permintaan tulis"
      ← desain: 05-skema-tampilan.md:705 "Menu PPRI berstatus read-only / print-ready";
        03-frontend-architecture.md:1187-1203 "FE-INP-36 … cetak saja"
```

Backend juga tidak menyediakan jalur ubah. `InpatientAdmissionConsentController.cs:42-93` hanya
punya `POST` (simpan dari langkah admisi) dan `GET`.

**Apakah ini menyimpang dari desain?** Tidak. Layar sudah sesuai `RWI-DEC-272` dan
`05-skema-tampilan.md:705`, yang sengaja menjadikan menu PPRI *read-only*. Karena itu jenisnya
`DESIGN_CHANGE`. Dokumen desain ikut direvisi agar tidak membantah source baru (`FIX-EPS-008-04`).

**Dampak nyata.** Persetujuan umum adalah dokumen hukum yang dicetak dan masuk berkas rekam medis.
Kesalahan nama, NIK, nomor telepon, atau hubungan wali tercetak apa adanya dan tidak dapat
dibetulkan lewat sistem.

**Rekomendasi.** Tab Formulir General Consent mendapat tombol **Edit Persetujuan**. Setiap simpan
membuat **versi baru** dengan alasan wajib. Versi lama tetap tersimpan sebagai riwayat. Pola ini
sama dengan Versi Koreksi dokumen admisi lain (`RWI-DEC-240`).

### ISS-EPS-008-02 — Menyimpan ulang persetujuan menimpa isi lama tanpa riwayat

| | |
| --- | --- |
| **No. laporan** | 1 ("ketahuan riwayat siapa yang melakukan edit") |
| **Jenis** | `BUG` |
| **Area** | Backend (sebab), frontend langkah admisi (pemicu) |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-008-01`, `FIX-EPS-008-03` |

**Apa yang terjadi.** Bila petugas membuka lagi langkah Formulir Persetujuan pada admisi yang sama
lalu menekan Simpan, data lama diganti data baru. Contoh: pukul 09.40 tersimpan "penanda tangan:
Rina Santoso (istri)". Pukul 10.15 petugas lain menyimpan ulang dengan "Andi Santoso (anak)".
Sesudahnya sistem hanya mengenal Andi. Tidak ada catatan bahwa Rina pernah menandatangani, dan
tanda tangan Rina hilang.

**Kenapa terjadi.**

`NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpatientAdmissionConsentService.cs:170-181`

```csharp
else
{
    // Update dokumen yang sudah ada
    existingDoc.Status = InpAdmissionDocumentStatus.Completed;
    ...
    existingDoc.SnapshotJson = snapshotJson;   // isi + citra TTD lama tertimpa
    existingDoc.UpdateBy = actorUserId ?? Guid.Empty;
```

Baris `:183-208` juga menimpa `Party` (nama, NIK, alamat) dan `Signature` di tempat. Akar
masalahnya: service ini (`BE-RWI-205`) ditulis terpisah dari siklus dokumen admisi dan tidak
mengikuti invariant tabelnya sendiri. `Models/InpAdmissionDocument.cs:11-15` menyatakan
"Setiap versi koreksi adalah baris sendiri … tanda tangan, salinan beku, dan isi versi lama tidak
pernah ditimpa". Kolom yang dibutuhkan sudah ada: `VersionNo`, `PreviousVersionId`,
`CorrectionReason`, `SupersededAt`, dan status `Superseded`
(`Enums/InpAdmissionDocumentStatus.cs`).

Pemicunya ada di frontend. `inpatient-admission-consent-form-step.jsx:85-109` (working tree)
memuat persetujuan yang sudah tersimpan ke form, lalu `:342` mengirim `POST` lagi.

Ada akibat ikutan bila versi mulai dipakai. `GetConsentAsync` (`:224-230`) dan pencarian dokumen
pada `SaveConsentAsync` (`:77-82`) memakai `FirstOrDefaultAsync` tanpa saringan status dan tanpa
urutan. Begitu ada versi `Superseded`, keduanya bisa mengembalikan versi lama.

**Apakah ini menyimpang dari desain?** Ya, jenisnya `BUG`. Perilaku ini menyimpang dari invariant
model di atas, dari `RWI-DEC-240` ("Buat Versi Koreksi … versi lama menjadi `Superseded` dan
tampil di tab Riwayat"), dan dari `RWI-DEC-236` butir 3 ("koreksi membuat versi baru").

**Dampak nyata.** Jejak audit dokumen hukum hilang. Rumah sakit tidak dapat menunjukkan siapa yang
semula menandatangani bila terjadi sengketa.

**Rekomendasi.** Pakai rantai versi pada tabel yang sama:

- Edit dari Workspace PPRI dan simpan ulang dari langkah admisi sama-sama membuat versi baru.
- Simpan ulang tanpa perubahan apa pun tidak membuat versi.
- Endpoint riwayat menampilkan setiap versi beserta pengubah, waktu, alasan, dan daftar perubahan.

### ISS-EPS-008-T3 — Tombol simpan langkah admisi dimatikan sampai lengkap

Rinciannya di bagian 6.

## 5. Tanya-jawab pelapor

> **T:** Bukankah form General Consent itu terhubung dari surat persetujuan umum di admisi?
>
> **J:** Ya. Kedua tab di menu General Consent membaca rekaman yang sama, yang disimpan pada langkah
> Formulir Persetujuan & TTD Digital admisi (`GET …/consent`, `use-admission-general-consent.js:67-75`).
> Karena itu, koreksi yang disimpan dari tab Formulir General Consent langsung berlaku juga pada tab
> Surat Persetujuan dan lembar cetaknya.

## 6. Temuan tambahan

### ISS-EPS-008-T1 — "Sudah menerima panduan rawat inap" hanya tersimpan di layar

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend dan backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI; terlihat pada screenshot |
| **Kenapa dicantumkan** | Terlihat pada lampiran: panel "Penerimaan Panduan Rawat Inap" tetap dapat diklik walau dokumen dinyatakan *read-only* |
| **Perbaikan** | `FIX-EPS-008-01`, `FIX-EPS-008-02` |

Pilihan Ya/Tidak dan keterangannya hanya ada di state halaman (`use-admission-general-consent.js:21-28`)
dan hilang begitu halaman ditutup. Pilihan itu tetap aktif meski dokumen sudah bertanda tangan
(`general-consent-section.jsx:307-324`).

Akibatnya, centang pada cetakan berasal dari klik petugas *sesudah* pasien menandatangani. Centang
itu juga bisa membantah butir 12 surat persetujuan ("telah membaca … Brosur Panduan Rawat Inap").

Panduan ikut disimpan sebagai bagian isi persetujuan. Pada versi baru, isinya dapat diubah lewat
Edit Persetujuan.

### ISS-EPS-008-T2 — Cetakan formulir bertanda tangan tidak mencentang hubungan

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri sumber data cetak formulir V1 |
| **Perbaikan** | `FIX-EPS-008-02` |

`general-consent-form-print.jsx:97-104` mencentang hubungan dari `choices.relationship`. Pada
dokumen bertanda tangan, pilihan hubungan disembunyikan (`general-consent-section.jsx:181-207`),
sehingga nilainya kosong. Hasilnya, cetakan untuk Ny. Rina (istri) tidak mencentang "Istri" maupun
pilihan lain.

Perbaikannya: hubungan pada cetakan diturunkan dari data tersimpan. Contoh pemetaan:

- `Self` → Diri Sendiri;
- `Parent` → Orang Tua;
- `Spouse` → Suami atau Istri, menurut teks hubungan;
- `Family` → Anak bila teksnya "anak", selain itu Lainnya;
- `Guardian` → Lainnya.

### ISS-EPS-008-T3 — Tombol "Simpan & Lanjut ke Cetak" mati sampai isian lengkap

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` (preferensi pemilik 06-10-2026: tombol simpan jangan dimatikan) |
| **Area** | Frontend langkah admisi |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di working tree |
| **Kenapa dicantumkan** | Berkas yang sama dengan pemicu `ISS-EPS-008-02` |
| **Perbaikan** | Tidak dimasukkan ke rencana ini |

`inpatient-admission-consent-form-step.jsx:784` memakai `disabled={!canSubmit || saving}`. Berkas
yang sama juga memakai `<input>` bawaan peramban (`:463-532`), padahal base component tersedia.

Berkas ini sedang diubah pekerjaan lain (belum di-commit). Merapikannya sekarang berisiko menimpa
pekerjaan itu, jadi dicatat untuk pemilik berkas.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bila persetujuan dikoreksi, apakah tanda tangan lama tetap dipakai? | (a) Selalu wajib tanda tangan ulang; (b) tanda tangan lama dipakai untuk koreksi data, wajib tanda tangan baru bila jenis penanda tangan berganti; (c) selalu memakai tanda tangan lama | **(b)**: koreksi salah ketik tidak mengubah siapa yang menandatangani; berganti jenis penanda tangan berarti orangnya berbeda | Tidak. Rekomendasi (b) dipakai sebagai bawaan sampai pemilik memutuskan lain; aturannya ada di satu validasi backend |

Keputusan yang **sudah diambil** dan tidak ditanyakan ulang:

| Hal | Keputusan | Sumber |
| --- | --- | --- |
| Persetujuan dapat diedit dengan riwayat pengubah | Ya | Pemilik sistem, laporan 13-10-2026 |
| Koreksi membuat versi baru, versi lama tetap terbaca | Ya | `RWI-DEC-240`, `RWI-DEC-236` butir 3 |
| Alasan koreksi wajib, 10–500 karakter | Ya | Pola `ReviseAsync` (`InpAdmissionDocumentService.cs:762-773`, `RWI-AC-355`) |
| Episode yang sudah ditutup tidak dapat diubah | Ya | `RWI-AC-358` (`InpAdmissionWriteGuard.EnsureWritableAsync`) |
| Hak ubah di Workspace PPRI | `InpatientAdmissionDocument : Update` | Sama dengan Versi Koreksi dokumen admisi lain (`InpatientAdmissionDocumentController.cs:322-328`) |

## 8. Catatan pola

- **Fitur baru yang menulis ke tabel bersama tanpa memakai siklus tabel itu.** `BE-RWI-205`
  menulis langsung ke `InpAdmissionDocument` dan melewati `InpAdmissionDocumentService`.
  Akibatnya invariant versi, `RowVersion`, dan penjaga episode tertutup tidak berlaku. Pencegahan:
  setiap writer baru ke tabel dokumen admisi wajib memakai pembantu siklus yang ada (`Touch`,
  `EnsureWritableAsync`, rantai `PreviousVersionId`).
- **"Muat lalu simpan ulang" pada langkah wizard** juga muncul di langkah admisi lain. Simpan ulang
  dari wizard sebaiknya selalu idempoten: isi yang sama tidak menghasilkan perubahan.
- **Status layar yang tidak pernah disimpan** (`T1`) adalah sisa desain *fail-closed*
  (`RWI-DEC-233`). Desain itu sudah dilampaui `RWI-DEC-272`, tetapi layarnya belum diselaraskan.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Issue dibuat dari laporan 1 butir; 2 temuan utama dan 3 temuan tambahan | `diagnose-module-issue` |
| 2026-10-13 | Status `DALAM_PERBAIKAN`: pemilik meminta perbaikan langsung dikerjakan ("tolong di perbaiki ya") | `diagnose-module-issue` |
