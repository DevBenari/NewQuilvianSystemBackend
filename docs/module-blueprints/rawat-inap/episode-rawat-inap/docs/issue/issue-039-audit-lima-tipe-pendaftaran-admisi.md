# ISSUE-039 — Audit lima tipe pendaftaran Admisi Rawat Inap: muat ulang membuat Draft ganda, rujukan Kamar Pulih hilang, data rujukan tidak tersimpan, dan lima temuan lain

```yaml
issue_id: ISSUE-EPS-039
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Pilih Tipe Pendaftaran dan seluruh langkah lima jalur: Pasien Baru, Pasien Lama, Bayi Baru Lahir, Kamar Pulih, Transfer IGD/Poli (route /health-services/inpatient-management/admissions/**)"
sumber_laporan: "Permintaan pemilik 23-10-2026: audit kode lima tipe pendaftaran agar bug sejenis ISSUE-EPS-038 tidak terulang; 1 lampiran (screenshot layar Pilih Tipe Pendaftaran)"
tanggal_issue: "2026-10-23"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "b6c638ee (MHamzah)"
source_sha_frontend: "229f071bb (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-039-audit-lima-tipe-pendaftaran-admisi.md
```

---

## 1. Ringkasan

Audit ini memeriksa kelima kartu pada layar **Pilih Tipe Pendaftaran** dengan memakai pola bug
yang ditemukan pada ISSUE-EPS-038 sebagai daftar periksa. Ditemukan **9 masalah**: 3 berkeparahan
**High**, 3 **Medium**, dan 3 **Low**. Admisi Bayi Baru Lahir sudah ditangani ISSUE-EPS-038
(menunggu uji layar); yang tersisa untuk bayi hanya isi pengganti "Tunai" (temuan 06).

Tiga temuan paling berbahaya:

1. **Muat ulang di langkah Pilih Bed membuat kunjungan dan episode `Draft` kedua** pada Pasien
   Baru, Pasien Lama, dan Kamar Pulih. Episode pertama sudah dibuat otomatis saat langkah Pilih
   Bed dibuka, tetapi nomornya tidak dicatat di alamat halaman, dan pilihan DPJP ikut hilang.
2. **Rujukan Kamar Pulih hilang setelah muat ulang.** Langkah berikutnya lalu menolak pasien
   dengan pesan "buka admisi dari permintaan itu", padahal petugas memang sedang di jalur Kamar
   Pulih.
3. **Data rujukan faskes luar yang wajib diisi tidak pernah dikirim ke server.** Nomor surat,
   faskes, dokter, dan diagnosa rujukan hilang begitu admisi selesai.

Tidak ada temuan yang mengharuskan backend diubah kecuali temuan 03, yang menunggu keputusan
pemilik.

---

## 2. Laporan asli

| No. | Permintaan pelapor (diringkas) | Lampiran |
| :---: | --- | :---: |
| 1 | "Cek code dan audit dari kesalahan-kesalahan bug tersebut agar tidak terjadi error, khusus admisi rawat inap, cek 5 tipe pendaftaran agar terhindar dari bug dan error." | L-1 |

**Lampiran L-1** — screenshot layar "Pilih Tipe Pendaftaran" dengan lima kartu: Pendaftaran
Pasien Baru, Pendaftaran Pasien Lama, Admisi Bayi Baru Lahir, Admisi dari Kamar Pulih, dan Admisi
Transfer IGD & Poliklinik. Tidak ada galat yang terlihat; lampiran dipakai sebagai daftar cakupan.

### Cakupan dan daftar periksa

Daftar periksa diturunkan dari akar masalah ISSUE-EPS-038, ditambah daftar periksa skill:

| Pola | Asal |
| --- | --- |
| Episode yang sudah dibuat tidak tercatat di URL sehingga muat ulang membuat yang baru | ISS-EPS-038-T2 |
| Pilihan penting hanya hidup di state layar, hilang saat muat ulang | ISS-EPS-038-01 |
| Langkah disembunyikan lalu dipasang ulang saat memuat | ISS-EPS-038-01 |
| Kunci `sessionStorage` bersama yang membawa data ke admisi berikutnya | ISS-EPS-038-T1 |
| Data hanya halaman pertama | ISS-EPS-038-02, -T5 |
| Isi pengganti yang tampak seperti data | ISS-EPS-038-T6 |
| Isian yang diwajibkan layar tetapi tidak sampai ke server | Daftar periksa skill: "aturan server tidak dicerminkan" |

| Jalur | Langkah | Temuan |
| --- | --- | --- |
| Pasien Baru | Jenis Kunjungan → Tipe Pasien → Pendaftaran → Pembayaran → Deposit → Dokter → Pilih Bed → Booking → Konfirmasi → Persetujuan → Cetak → Kartu | 01, 03, 04, 06, 08 |
| Pasien Lama | Pasien Lama → Jenis Kunjungan & Kategori → Pembayaran → Deposit → Dokter → Pilih Bed → Booking → Konfirmasi → Persetujuan → Cetak | 01, 03, 06, 08 |
| Bayi Baru Lahir | Delapan langkah bayi | ISSUE-EPS-038 (menunggu uji layar); 06 |
| Kamar Pulih | Permintaan Kamar Pulih → Informasi Pasien → Tipe Pasien → Pembayaran → Deposit → Dokter → Pilih Bed → Booking → … | 01, 02, 06, 07, 08 |
| Transfer IGD/Poli | Antrean → Tinjau Pasien → Pembayaran → Deposit → Dokter → Pilih Bed → Booking → … | 05, 06, 09 |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-039-01` | 1 | Muat ulang di Pilih Bed membuat kunjungan dan episode `Draft` kedua; DPJP ikut hilang | `BUG` | Frontend | High | Kode SUDAH-VERIFIKASI; data ganda DUGAAN | `FIX-EPS-039-01` |
| `ISS-EPS-039-02` | 1 | Rujukan Kamar Pulih hilang saat muat ulang, lalu admisi ditolak `INP-ADM-REF-001` | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-EPS-039-02` |
| `ISS-EPS-039-03` | 1 | Data rujukan faskes luar yang wajib diisi tidak pernah dikirim ke server | `CONTRACT_MISMATCH`, `PERLU_KONFIRMASI` | Frontend + Backend | High | SUDAH-VERIFIKASI | `FIX-EPS-039-03` |
| `ISS-EPS-039-04` | 1 | Kunci sesi `new:active` membawa jenis kunjungan, rujukan, dan kategori ke Pasien Baru berikutnya | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-039-04` |
| `ISS-EPS-039-05` | 1 | Transfer memuat ulang sumber di setiap langkah, langkah berkedip, dan episode sendiri dianggap "melanjutkan admisi" | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-039-05` |
| `ISS-EPS-039-06` | 1 | Penjamin yang belum terbaca ditampilkan sebagai "Umum" / "Tunai / Mandiri" | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-039-06` |
| `ISS-EPS-039-07` | 1 | Daftar Kamar Pulih hanya 50 baris; pencarian dan angka lencana terpotong | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-039-07` |
| `ISS-EPS-039-08` | 1 | Kunci idempotensi deposit bukan GUID sehingga diabaikan server | `CONTRACT_MISMATCH` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-039-08` |
| `ISS-EPS-039-09` | 1 | Cabang pilihan bed transfer yang tak terpakai membaca nama field sebagai ID bed | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-039-09` |

---

## 4. Rincian per temuan

### ISS-EPS-039-01 — Muat ulang di Pilih Bed membuat kunjungan dan episode `Draft` kedua; DPJP ikut hilang

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | Kode SUDAH-VERIFIKASI; terbentuknya data ganda DUGAAN |
| **Perbaikan** | `FIX-EPS-039-01` |
| **Jalur** | Pasien Baru, Pasien Lama, Kamar Pulih |

**Apa yang terjadi.** Petugas membuka langkah Pilih Bed untuk Tn. Budi. Tanpa disadari, server
sudah membuat kunjungan dan episode `Draft` RI-A. Peramban macet, petugas menekan F5. Layar
meminta kembali ke langkah Dokter karena DPJP kosong. Petugas memilih DPJP lagi dan masuk ke Pilih
Bed, lalu server membuat kunjungan dan episode RI-B. Tn. Budi kini punya dua admisi "Sedang
Disiapkan".

**Kenapa terjadi.**

```text
Dua kunjungan dan dua episode Draft untuk satu pasien
  ← muat ulang tidak tahu episode RI-A, lalu langkah Pilih Bed membuat yang baru
    ← efek penulisan URL sengaja melewati langkah Pilih Bed untuk jalur non-bayi
       (inpatient-admission-view.jsx:691-703, kondisi baris 697)
    ← episode dibuat otomatis begitu langkah Pilih Bed terbuka
       (use-inpatient-admission-bed.jsx:998-1063; prepareFloorPlanDraft dipanggil di :1054)
    ← pilihan DPJP hanya disimpan di memori, kecuali jalur Transfer
       (use-inpatient-admission-doctor.jsx:165 dan :448-514 hanya untuk transferDraftKey;
        canAdmit di :570-573 memerlukan isian valid)
      ← server sengaja hanya memberi peringatan untuk Draft ganda
         (InpEpisodeService.cs:365 "admisi Draft ganda adalah peringatan, bukan penolakan";
          InpEpisodeService.AdmissionEligibility.cs:116)
```

Perbandingan dengan jalur Bayi: `FIX-EPS-038-01` sudah mencatat episode bayi ke URL di langkah
Pilih Bed dan mencegah langkah dipasang ulang (`isOwnEpisode`, `inpatient-admission-view.jsx:683`).
Jalur non-bayi belum ikut.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `FE-RWI-032` merancang URL sebagai penyimpan
episode "supaya memuat ulang halaman di tengah pelanjutan tidak memulai admisi baru untuk pasien
yang sama" (komentar `use-inpatient-admission-flow.jsx`, `resumeEpisodeId`).

**Dampak nyata.** Draft ganda tampil di Daftar Pasien Rawat Inap dan memegang kunjungan sendiri.
Bila RI-A sempat memesan bed, bed itu tertahan 15 menit atas nama episode yang tidak dipakai.

**Cara membuktikan (DUGAAN).** Query baca untuk pelapor:

```sql
SELECT "PatientId", COUNT(*) AS jumlah_draft
FROM public."InpEpisode"
WHERE "EpisodeStatus" = 0 AND NOT "IsDelete" AND "DeliveryCaseId" IS NULL
GROUP BY "PatientId"
HAVING COUNT(*) > 1;
```

**Rekomendasi.** Catat `episodeId` ke URL segera setelah Draft dibuat, untuk semua jalur, dan
simpan pilihan DPJP per pasien di `sessionStorage` seperti pilihan Pembayaran.

---

### ISS-EPS-039-02 — Rujukan Kamar Pulih hilang saat muat ulang, lalu admisi ditolak `INP-ADM-REF-001`

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-02` |
| **Jalur** | Kamar Pulih |

**Apa yang terjadi.** Petugas memilih permintaan Kamar Pulih untuk Ny. Rina lalu lanjut ke
Informasi Pasien. Setelah halaman dimuat ulang, layar menandai Ny. Rina tidak boleh diadmisi:
"Pasien punya permintaan admisi dari kamar pulih; buka admisi dari permintaan itu". Bila petugas
memaksa lanjut, server menolak di langkah Pilih Bed dengan pesan yang sama. Satu-satunya jalan
keluar adalah kembali ke langkah 1 dan memilih ulang permintaannya.

**Kenapa terjadi.**

```text
Admisi Kamar Pulih ditolak "buka admisi dari permintaan itu"
  ← kelayakan dan pembuatan episode dikirim tanpa admissionReferralId
     (inpatient-admission-view.jsx:599, :660)
    ← permintaan terpilih hanya disimpan di state view
       (inpatient-admission-view.jsx:539; diisi di :930)
      ← URL hanya memuat langkah dan patientId; permintaan tidak tercatat di URL maupun sessionStorage
        ← server mewajibkan admisi pasien berpermintaan Pending dibuat dari permintaan itu
           (InpEpisodeService.cs:218-232; InpEpisodeService.AdmissionEligibility.cs:80-92;
            InpAdmissionReferralService.cs:223)
```

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `RWI-DEC-201` dan `VAL-RWF-87/88` mengharuskan
admisi Kamar Pulih tertaut ke permintaannya. Layar menawarkan langkah yang pasti ditolak server.

**Dampak nyata.** Petugas pada pergantian shift yang membuka ulang tab di tengah alur tidak dapat
menyelesaikan admisi pasien pasca operasi tanpa mengulang dari awal.

**Rekomendasi.** Catat ID permintaan di URL (`referralId`), lalu baca ulang permintaannya lewat
`GET /admission-referrals/{id}` saat halaman dimuat. Endpoint ini sudah ada.

---

### ISS-EPS-039-03 — Data rujukan faskes luar yang wajib diisi tidak pernah dikirim ke server

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `CONTRACT_MISMATCH` (utama), `PERLU_KONFIRMASI` (tempat penyimpanan) |
| **Area** | Frontend + Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-03` (⛔ menunggu K-01) |
| **Jalur** | Pasien Baru (langkah 1), Pasien Lama (langkah 2) |

**Apa yang terjadi.** Pada Jenis Kunjungan "Rujukan", petugas wajib mengisi Nomor Surat Rujukan,
Tanggal & Jam, Faskes Perujuk, Dokter Perujuk, dan Diagnosa. Layar menolak lanjut bila ada yang
kosong. Setelah admisi selesai, kelima isian itu tidak ada di mana pun: tidak di kunjungan, tidak
di episode, dan tidak di dokumen cetak.

**Kenapa terjadi.**

```text
Data rujukan hilang
  ← isian hanya disimpan ke state alur dan sessionStorage
     (inpatient-admission-visit-type-step.jsx:80, :83; inpatient-admission-visit-category-step.jsx:79, :82)
    ← payload kunjungan dan episode tidak memuat satu pun field rujukan
       (inpatient-admission-encounter-utils.jsx:233-298)
      ← desain frontend mencatatnya sebagai "state alur" tanpa endpoint
         (03-frontend-architecture.md:404), padahal PRD mewajibkan "merekam data rujukan"
         (04-prd-to-mvp.md FR-RI-180, MUST)
```

Endpoint yang dapat dipakai ulang sudah ada di modul Pendaftaran:
`PUT /api/v1/health-services/registration-management/patient-encounters/{encounterId}/referral`
(`PatientEncounterController.cs`, `UpsertEncounterReferralRequest`: `ReferralNumber`,
`ReferralDateTime`, `ReferralInstitutionId`, `ReferralDoctorName`, `DiagnosisNote`,
`ReferralReason`). Bedanya, faskes perujuk di sana berupa **ID master institusi**, sedangkan layar
admisi meminta **teks bebas** sesuai `RWI-DEC-269`. Apakah endpoint itu menerima kunjungan rawat
inap belum dibuktikan (DUGAAN); cara membuktikannya: panggil endpoint dengan kunjungan rawat inap
di lingkungan dev.

**Apakah ini menyimpang dari desain?** Ya — PRD `FR-RI-180` (MUST) dan dokumen frontend saling
membantah. Karena penyelesaiannya menyentuh kontrak lintas modul, tempat penyimpanannya
diputuskan pemilik (K-01).

**Dampak nyata.** Klaim penjamin yang mensyaratkan surat rujukan tidak dapat ditelusuri dari
sistem; petugas harus mencari berkas fisik.

**Rekomendasi.** Pakai ulang endpoint rujukan kunjungan dan ganti isian faskes teks dengan pilihan
master institusi, setelah K-01 diputuskan.

---

### ISS-EPS-039-04 — Kunci sesi `new:active` membawa jenis kunjungan, rujukan, dan kategori ke Pasien Baru berikutnya

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-04` |
| **Jalur** | Pasien Baru (Pasien Lama dan Kamar Pulih ikut terlindungi) |

**Apa yang terjadi.** Petugas mengisi rujukan Ny. Sari (nomor 445/RS-X, diagnosa "G2P1 aterm") di
langkah 1, memilih kategori "Pegawai", lalu pindah menu lewat sidebar. Saat membuka Pendaftaran
Pasien Baru untuk Tn. Andi, langkah 1 sudah terisi rujukan Ny. Sari dan langkah 2 terpilih
"Pegawai".

**Kenapa terjadi.** Sebelum pasien baru terdaftar, kunci draf sesi adalah
`quilvian:inpatient-admission:flow:new:active` (`use-inpatient-admission-flow.jsx:147-156`).
`FIX-EPS-038-05` hanya menonaktifkan kunci `active` untuk jalur bayi (`:153`). Efek rehidrasi
memulihkan `patientType`, `visitType`, dan `referralData` dari kunci itu (`:216-228`). Kunci hanya
dihapus lewat "Batal Pendaftaran" atau "Pilih Tipe" kembali, tidak saat petugas pindah menu.

**Apakah ini menyimpang dari desain?** Ya, `BUG` — `FIX-EPS-029` menetapkan draf sesi tidak boleh
mencemari admisi lain.

**Dampak nyata.** Kategori "Pegawai" yang terbawa dapat memicu aturan penjamin karyawan untuk
pasien umum. Rujukan pasien lain tampil seolah milik pasien baru.

**Rekomendasi.** Perluas `FIX-EPS-038-05` ke semua jalur: jangan simpan atau pulihkan draf pada
kunci `active`.

---

### ISS-EPS-039-05 — Transfer memuat ulang sumber di setiap langkah, langkah berkedip, dan episode sendiri dianggap "melanjutkan admisi"

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-05` |
| **Jalur** | Transfer IGD/Poli |

**Apa yang terjadi.** Setiap kali petugas pindah langkah (Pembayaran → Deposit → Dokter → Pilih
Bed …), area langkah sesaat berganti menjadi "Memuat dan memeriksa permintaan rawat inap...". Saat
"Pesan Tempat Tidur" berhasil, langkah Booking hilang sebentar, lalu muncul banner "Melanjutkan
admisi RI-…" untuk admisi yang baru saja dibuat petugas sendiri.

**Kenapa terjadi.**

```text
Langkah berkedip dan dipasang ulang
  ← view mengganti AdmissionStep dengan kotak memuat selama transfer.loading
     (inpatient-admission-view.jsx:1209)
    ← efek pemuat sumber transfer bergantung pada stepSlug dan episodeId, lalu mengosongkan
       source sebelum memuat (use-inpatient-admission-transfer.js:28-46, dependensi :46)
Banner "Melanjutkan admisi" untuk episode sendiri
  ← adoptTransferEpisode tidak mengisi createdEpisodeId (use-inpatient-admission-doctor.jsx:898-901)
    ← sehingga isOwnEpisode (inpatient-admission-view.jsx:683) bernilai salah dan gerbang
       pelanjutan menyembunyikan langkah sampai episode dibaca ulang
```

**Apakah ini menyimpang dari desain?** Ya, `BUG` — pola yang sama dengan ISS-EPS-038-01.

**Dampak nyata.** Belum ada kehilangan data karena isian disimpan di hook level view, tetapi setiap
langkah memicu satu permintaan `GET` tambahan, dan state lokal langkah (misalnya dialog konfirmasi
Booking) direset di tengah aksi.

**Rekomendasi.** Muat sumber transfer hanya saat `transferId`/`patientId`/`reload` berubah,
pertahankan data lama selama memuat ulang, dan tandai episode hasil transfer sebagai episode milik
layar.

---

### ISS-EPS-039-06 — Penjamin yang belum terbaca ditampilkan sebagai "Umum" / "Tunai / Mandiri"

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-06` |
| **Jalur** | Semua (Konfirmasi), Bayi (Pilih Ibu dan Tinjau) |

**Apa yang terjadi.** Pada alur pelanjutan, bila data kunjungan gagal dibaca, hook pelanjutan
sengaja tetap jalan tanpa ringkasan penjamin (`use-inpatient-admission-resume.jsx:89-97`). Kartu
Penjamin di langkah Konfirmasi lalu menulis "Umum" dan "Tunai / Mandiri"
(`inpatient-admission-confirmation-step.jsx:195-196`). Pasien BPJS terlihat seperti pasien tunai
tepat sebelum admisi diaktifkan. Jalur bayi memakai fallback "Tunai" yang sama
(`inpatient-newborn-admission-steps.jsx:79`, `:608`).

**Apakah ini menyimpang dari desain?** Ya, `BUG` — isi pengganti yang tampak seperti data, pola
ISS-EPS-038-T6.

**Rekomendasi.** Tampilkan "Belum terbaca" beserta tombol "Muat Ulang Ringkasan" bila penjamin
tidak terbaca.

---

### ISS-EPS-039-07 — Daftar Kamar Pulih hanya 50 baris; pencarian dan angka lencana terpotong

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-07` |
| **Jalur** | Kamar Pulih |

Layanan memuat permintaan `Pending` dengan `pageSize: 50` tanpa halaman berikutnya
(`inpatient-admission-referral.service.js:37`). Pencarian hanya menyaring 50 baris yang sudah
dimuat (`inpatient-admission-recovery-referral-step.jsx:118-148`). Lencana kartu menghitung
`items.length` (`inpatient-admission-view.jsx:851`), sehingga paling banyak "50", sedangkan kartu
Transfer memakai `totalData`. Bila permintaan ke-51 milik pasien yang dicari, petugas tidak
menemukannya.

---

### ISS-EPS-039-08 — Kunci idempotensi deposit bukan GUID sehingga diabaikan server

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-08` |
| **Jalur** | Pasien Baru, Pasien Lama, Kamar Pulih (deposit Tunai) |

Layar mengirim `depositIdempotencyKey: "ADM-DEP-<patientId>"` (`inpatient-admission-view.jsx:749`).
Server hanya menerima GUID dan membuat GUID baru bila gagal dibaca
(`InpEpisodeService.cs:505`). Akibatnya kunci tidak pernah melindungi pengiriman ulang. Saat ini
order deposit ganda tertahan oleh indeks unik kunjungan–episode, jadi dampaknya masih kecil.

---

### ISS-EPS-039-09 — Cabang pilihan bed transfer yang tak terpakai membaca nama field sebagai ID bed

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-EPS-039-09` |
| **Jalur** | Transfer (cabang mati) |

`BaseSelectField` memanggil `onChange(fieldName, value)`, sedangkan `selectTransferBed(bedId)`
menerima nama field sebagai `bedId` (`inpatient-admission-bed-step.jsx:59`, `:167`). Cabang ini
hanya dirender bila `selectionMode !== "floor-plan"`, yang tidak pernah terjadi pada admisi
(`usesAdmissionFloorPlan` selalu benar). Masalahnya akan muncul seketika bila cabang ini dihidupkan
kembali.

---

## 5. Tanya-jawab pelapor

> **T:** Apakah kelima tipe pendaftaran aman dari bug seperti admisi bayi kemarin?
>
> **J:** Belum seluruhnya. Pola "muat ulang membuat episode kedua" dan "pilihan hilang saat muat
> ulang" juga ada pada Pasien Baru, Pasien Lama, dan Kamar Pulih (temuan 01, 02). Pola "langkah
> berkedip lalu dipasang ulang" ada pada Transfer (temuan 05). Admisi bayi sudah diperbaiki lewat
> ISSUE-EPS-038 dan menunggu uji layar.

> **T:** Bagian mana yang paling berisiko menimbulkan error?
>
> **J:** Kamar Pulih setelah muat ulang (temuan 02): server pasti menolak admisinya. Berikutnya
> temuan 01, yang membuat admisi ganda tanpa galat apa pun.

---

## 6. Temuan tambahan

Tidak ada. Seluruh temuan di atas adalah hasil langsung permintaan audit (No. laporan 1).

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Bila memungkinkan, jalankan query pada temuan 01 dan kirim jumlah barisnya | Membuktikan Draft ganda sudah terjadi | Temuan 01 tetap dikerjakan |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Di mana data rujukan faskes luar disimpan? | A: endpoint rujukan kunjungan modul Pendaftaran (faskes dipilih dari master institusi) / B: field baru pada episode rawat inap (backend Rawat Inap, teks bebas) / C: dicatat ke catatan admisi saja | Semula A; **direvisi dan diputuskan B (23-10-2026)** karena endpoint Pendaftaran tidak mendukung kunjungan rawat inap | `FIX-EPS-039-03` |
| K-02 | Draft ganda yang sudah terbentuk | Gugur otomatis (`RWI-DEC-030`) + batal manual / skrip data | Sama dengan K-02 ISSUE-038: gugur otomatis + batal manual, tanpa skrip | Tidak |

---

## 8. Catatan pola

1. **Ketahanan muat ulang tidak seragam antar jalur.** Pembayaran dan deposit disimpan per pasien,
   DPJP hanya untuk Transfer, rujukan Kamar Pulih tidak disimpan sama sekali, dan episode baru
   dicatat ke URL hanya pada sebagian langkah. Usulan: satu aturan — **setiap data yang dibutuhkan
   langkah berikutnya tinggal di URL (ID) atau `sessionStorage` per pasien (isian), tidak hanya di
   state view.**
2. **Gerbang "sedang memuat" yang mengganti seluruh langkah** muncul dua kali (pelanjutan dan
   Transfer). Usulan: data lama dipertahankan selama memuat ulang; gerbang pengganti langkah hanya
   untuk muatan pertama.
3. **Isi pengganti yang tampak seperti data** ("Tunai", "Umum", "VIP") muncul di beberapa jalur.
   Usulan aturan: nilai yang belum terbaca selalu ditulis "Belum terbaca".
4. Tes unit admisi sebagian besar mencocokkan teks source (lihat ISSUE-038 bagian 8), sehingga
   tidak menangkap pola di atas. Perbaikan issue ini sebaiknya menambah fungsi murni bertes.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-23 | Issue dibuat dari permintaan audit lima tipe pendaftaran; 9 temuan (3 High, 3 Medium, 3 Low) | `diagnose-module-issue` |
| 2026-10-23 | Pemilik menyetujui rencana, K-01 opsi A, K-02. Status → `DALAM_PERBAIKAN`; 8 perbaikan dikerjakan di task `FIX-EPS-039` ([laporan](../../task/report/frontend/FIX-EPS-039.md)). Koreksi atas ISS-EPS-039-03: endpoint rujukan kunjungan ternyata tidak mendukung kunjungan rawat inap (tujuan tercatat `Laboratory`, faskes wajib mitra `Pm17`), sehingga opsi A memerlukan task backend modul Pendaftaran | `build-module-frontend` |
| 2026-10-23 | Pemilik menyetujui revisi K-01 ke opsi B: data rujukan disimpan sebagai field teks pada episode rawat inap | Pemilik Sistem |
| 2026-10-23 | ISS-EPS-039-03 diperbaiki di source oleh `FIX-EPS-039-03-BE` ([laporan](../../task/report/backend/FIX-EPS-039-03-BE.md)) dan `FIX-EPS-039-03-FE` ([laporan](../../task/report/frontend/FIX-EPS-039-03-FE.md)); menunggu migration diterapkan dan uji layar | `build-module-backend`, `build-module-frontend` |
