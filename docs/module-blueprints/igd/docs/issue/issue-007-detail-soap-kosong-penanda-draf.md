# ISSUE-007 — Detail SOAP Dokter IGD yang Sudah Selesai Kosong, Tanpa Jalan Kembali, dan Draf Tidak Menonjol di Riwayat

```yaml
issue_id: ISSUE-IGD-007
module_id: igd
submodule: igd
layar: "Ruang Kerja Dokter IGD (SCR-IGD-D01) — tab Catatan Dokter: Form SOAP (catatan selesai) dan Riwayat SOAP"
sumber_laporan: "Laporan pelapor 04-11-2026: 2 butir (1 bug, 1 usulan + pertanyaan), 1 lampiran"
tanggal_issue: "2026-11-04"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2) + perubahan belum di-commit FIX-IGD-004/005/006"
rencana_perbaikan: ../plan-repair/plan-repair-007-detail-soap-kosong-penanda-draf.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Setelah dokter menekan **Buka** pada catatan SOAP yang **sudah selesai**, layar pindah ke Form SOAP.
Tetapi isi catatannya tidak tampil; yang muncul hanya "Riwayat Koreksi". Tidak ada pula tombol untuk
kembali atau membuat catatan baru, sehingga dokter harus memuat ulang halaman. Penyebabnya, tab IGD
memanggil tiga komponen Rawat Inap dengan **nama props yang salah**. Ini pola yang sama dengan
`ISSUE-IGD-006`.

Usulan pelapor agar draf dibedakan dengan warna latar **disetujui dengan penyesuaian**. Kartu riwayat
sebenarnya sudah punya label status ("Sedang ditulis" / "Selesai"), tetapi samar dan latarnya sama.
Penanda warna perlu disertai teks, dan menyentuh komponen milik Rawat Inap sehingga butuh izin
(K-01).

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "ketika saya klik buka pada riwayat soap ini langsung mengarah ke form soap, tapi tidak ada button untuk kembali memunculkan ini harus refresh dulu" | Lampiran 1: segmen *Form SOAP* aktif, kartu pasien "Diagnosis kerja: Salmonella enteritis (A02.0)", hanya panel **Riwayat Koreksi** "0 Koreksi — Belum ada koreksi pada catatan ini." |
| 2 | "jika itu menjadi draf tapi di riwayat soap nyaa tidak ada pemberitahuan mengenai bahwa data nyaa ini masih dalam bentuk draf, bisa di tandai dengan bg nyaa kan kalo sukses hijau, bagaimana menurut anda ?" | — |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-007-01` | 1 | Isi catatan selesai tidak tampil: `ProgressNoteDetail` menerima `note`, bukan `detail` | `BUG` | Frontend | High | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-007-01` |
| `ISS-IGD-007-02` | 1 | Tidak ada jalan keluar: bilah aksi tampilan catatan tidak dibuat; "Buat SOAP Baru" dan "Refresh" di Riwayat mati karena props salah | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-007-01` |
| `ISS-IGD-007-03` | 2 | Draf tidak menonjol di Riwayat SOAP; pelapor mengusulkan warna latar per status | `DESIGN_CHANGE` | Frontend (komponen bersama Rawat Inap) | Medium | SUDAH-VERIFIKASI | `FIX-IGD-007-02` |

---

## 4. Rincian per temuan

### ISS-IGD-007-01 — Isi catatan selesai tidak tampil

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `emergency-doctor-progress-tab.jsx` (milik IGD) |
| **Keparahan** | High — dokumen klinis yang sudah ditandatangani tidak dapat dibaca dari Form SOAP |
| **Status bukti** | SUDAH-VERIFIKASI; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-IGD-007-01` |

**Apa yang terjadi.** Catatan yang sudah selesai dibuka dalam mode baca-saja. Seharusnya tampil
"Dokumen asli" berisi nomor catatan, dokter, waktu, S/O/A/P, dan diagnosis. Yang tampil hanya panel
Riwayat Koreksi.

**Kenapa terjadi.**

`src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/progress-note-detail.jsx:27-28`

```jsx
export default function ProgressNoteDetail({ detail, isLockedUnsigned = false }) {
  if (!detail) return null;
```

`src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-progress-tab.jsx:245-253`

```jsx
<ProgressNoteDetail
  note={detail}                         // komponen membaca "detail", jadi detail = undefined
  …
  onBack={() => selectNote("")}         // tidak dikenal komponen
  onNewNote={…}                         // tidak dikenal komponen
  onAddCorrection={…}                   // tidak dikenal komponen
```

Karena `detail` tidak pernah sampai, komponen langsung `return null`. Rawat Inap memanggilnya dengan
benar: `physician-progress-tab.jsx:227` → `<ProgressNoteDetail detail={detail}
isLockedUnsigned={isLockedUnsigned} />`. Cacat ini ada sejak halaman dokter IGD dibuat
(commit `905aec3bd`, 8 Okt 2026).

**Apakah ini menyimpang dari desain?** Ya. Tab Catatan Dokter ditetapkan *Reuse with adapter*
(`03-frontend-architecture.md` §15), dan tampilan baca-saja catatan selesai adalah bagian dari
komponen yang dipakai ulang. Jenis: `BUG`.

---

### ISS-IGD-007-02 — Tidak ada jalan keluar dari tampilan catatan selesai

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `emergency-doctor-progress-tab.jsx` |
| **Keparahan** | High — dokter tidak dapat membuat catatan baru tanpa memuat ulang halaman |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-007-01` |

**Kenapa terjadi.** Dua sebab:

1. Di Rawat Inap, di bawah detail ada bilah aksi berisi **Koreksi** dan **Catatan Baru**
   (`physician-progress-tab.jsx:238-272`). IGD tidak membuat bilah itu. IGD mengandalkan props
   `onBack`/`onNewNote` pada `ProgressNoteDetail`, yang tidak dikenal komponen itu.
2. Panel Riwayat SOAP punya tombol **Buat SOAP Baru** dan **Refresh**, tetapi keduanya membaca
   `onStartNewNote` dan `onRefresh` (`soap-history-panel.jsx:80-81`, `:168-176`). IGD mengirim
   `onNewNote` dan tidak mengirim `onRefresh` (`emergency-doctor-progress-tab.jsx:206-214`), sehingga
   kedua tombol tidak berbuat apa-apa.

Selain itu, IGD membaca `addendumAuthority?.canAddAddendum` (`:254`), padahal hasil normalisasi
memakai `isAllowed` (`inpatient-progress-note-utils.jsx:1156`). Tombol Koreksi pun tidak pernah
ditawarkan. Ini bagian dari `ISSUE-IGD-006-T4`.

**Dampak nyata.** dr. Contoh A membuka catatan selesai untuk membaca ulang penilaiannya. Layar kosong.
Ia kembali ke Riwayat SOAP dan menekan "Buat SOAP Baru"; tidak terjadi apa-apa. Satu-satunya jalan
adalah memuat ulang halaman.

---

### ISS-IGD-007-03 — Draf tidak menonjol di Riwayat SOAP

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend — `soap-history-panel.jsx` **milik Rawat Inap** (pemilik Muhammad Hamzah) |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-IGD-007-02` (menunggu K-01) |

**Keadaan sekarang.** Setiap kartu riwayat sudah memuat label status lewat `ClinicalStatusBadge`
(`soap-history-panel.jsx:245-252`): draf/sedang ditulis bernada `waiting`, selesai `success`, dan
batal `danger` (`inpatient-progress-note-constants.jsx:25-31`). Tetapi label itu kecil, dan latar
kartu sama untuk semua status. Setelah IGD memakai satu tombol simpan (`FIX-IGD-006-04`), draf hanya
tersisa bila penyelesaian ditolak atau berasal dari sebelum perubahan. Justru catatan seperti itu
yang perlu paling terlihat, karena **belum masuk CPPT**.

**Apakah ini menyimpang dari desain?** Tidak. Penanda status lewat label adalah rancangan sekarang.
Menambah warna latar adalah `DESIGN_CHANGE`.

**Pendapat atas usulan pelapor.** Setuju, dengan tiga penyesuaian:

1. **Warna tidak boleh menjadi satu-satunya penanda.** Sebagian pengguna sulit membedakan warna, dan
   layar di IGD sering terkena silau. Warna dipasangkan dengan teks yang jelas.
2. **Warna lembut, bukan latar penuh yang mencolok.** Latar tipis ditambah garis tebal di sisi kiri
   kartu. Draf memakai kuning/oranye (tanda perlu tindakan), selesai hijau, dibatalkan abu-abu.
3. **Draf diberi kalimat tindakan**: "Draf — belum masuk CPPT. Buka lalu tekan Simpan Catatan CPPT."

---

## 5. Tanya-jawab pelapor

> **T:** Bagaimana menurut Anda soal menandai draf dengan warna latar?
>
> **J:** Setuju, karena draf sekarang adalah catatan yang belum masuk CPPT dan perlu ditindaklanjuti.
> Saran saya, warnanya lembut (kuning untuk draf, hijau untuk selesai) dan selalu disertai teks
> "Draf — belum masuk CPPT". Dengan begitu tetap terbaca walau warna sulit dibedakan. Karena kartu
> riwayat dipakai bersama Rawat Inap, perubahannya dibuat sebagai pilihan yang hanya dinyalakan IGD,
> kecuali pemilik Rawat Inap ingin ikut memakainya.

---

## 6. Temuan tambahan

`ISSUE-IGD-006-T4` (modal Koreksi dan Salin rusak; payload addendum salah) bersinggungan dengan
`ISS-IGD-007-02`, karena tombol Koreksi berada di tampilan catatan selesai. Saat mengerjakan
keputusan "tanpa draf" ditemukan:

| ID | Temuan | Keparahan | Status |
| --- | --- | --- | --- |
| `ISS-IGD-007-T1` | `completeNote` IGD memeriksa `issues.hasErrors`, padahal `findProgressNoteCompletionIssues` mengembalikan **array**. Pemeriksaan layar tidak pernah menahan apa pun: catatan tak lengkap tetap dibuat ke server, lalu ditolak, sehingga tertinggal sebagai draf. Inilah sumber draf yang membingungkan dokter. Rawat Inap memakai `issues.length > 0` (`use-inpatient-progress-note.jsx:910`) | High | **Diperbaiki** (`FIX-IGD-007-04`) |
| `ISS-IGD-007-T2` | Pesan penolakan server ditulis ke `completionIssuesNotice`, yang tidak dirender di layar mana pun. Dokter tidak melihat alasan penolakan | Medium | **Diperbaiki** — pesan memakai `saveError` (`FIX-IGD-007-04`) |

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Izin menambah pilihan penanda warna pada `soap-history-panel.jsx` (milik Rawat Inap) | Pilihan opt-in, hanya IGD yang menyalakan / berlaku juga di Rawat Inap / tidak diubah | **Opt-in**, tanpa mengubah Rawat Inap; cukup laporan lisan Rizki bahwa pemilik Rawat Inap setuju | `FIX-IGD-007-02` |
| K-02 | Sertakan perbaikan `ISSUE-IGD-006-T4` (Koreksi/addendum dan Salin) sekarang? | Ya / nanti | **Ya.** Tombol Koreksi berada di tampilan yang diperbaiki `FIX-IGD-007-01`, dan catatan selesai saat ini tidak dapat dikoreksi | `FIX-IGD-007-03` |

---

## 8. Catatan pola

Sudah lima issue hari ini (`ISSUE-IGD-004` s.d. `-007`) berakar sama: tab dokter IGD memanggil
komponen Rawat Inap dengan props yang tidak cocok (`isOpen`/`show`, `note`/`detail`,
`onNewNote`/`onStartNewNote`, `canAddAddendum`/`isAllowed`, `notes`/`addendumText`). Usul pencegahan
yang konkret: sekali jalan, cocokkan **setiap** pemakaian komponen Rawat Inap di
`doctor-emergency/tabs/*.jsx` dengan daftar props komponennya, lalu uji setiap tombol tab dokter IGD
satu per satu.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Issue dibuat dari laporan 2 butir + 1 lampiran; 3 temuan | `diagnose-module-issue` |
| 2026-11-04 | Pelapor memutuskan: IGD tanpa draf (catatan langsung utuh atau tidak tersimpan sama sekali), dan pindah segmen menyegarkan tampilan. `ISS-IGD-007-03` diselesaikan lewat keputusan itu, bukan lewat warna. Ditambahkan `ISS-IGD-007-T1` dan `-T2`, keduanya diperbaiki. Status `DALAM_PERBAIKAN` | Claude Code |
