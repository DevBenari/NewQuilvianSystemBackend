# ISSUE-003 — Riwayat Assesmen Awal IGD Tetap Kosong Padahal Pengkajian Berhasil Disimpan

```yaml
issue_id: ISSUE-IGD-003
module_id: igd
submodule: igd
layar: "Ruang Kerja Pemeriksaan IGD — Asuhan Keperawatan › Assesmen Awal IGD › segmen Riwayat Dokumen Pengkajian (FE-IGD-031/032)"
sumber_laporan: "Laporan pelapor 04-11-2026: 1 butir, 1 lampiran (screenshot segmen Riwayat kosong)"
tanggal_issue: "2026-11-04"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "30f1396d (rizkiG)"
source_sha_frontend: "739e05f68 (RizkiV2)"
rencana_perbaikan: ../plan-repair/plan-repair-003-riwayat-assesmen-awal-kosong-setelah-simpan.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Pelapor mengisi formulir **Assesmen Awal IGD** di Ruang Kerja Pemeriksaan IGD dan menekan
**Simpan Pengkajian**. Penyimpanan berhasil — layar berpindah sendiri ke segmen **Riwayat Dokumen
Pengkajian**, dan perpindahan itu hanya terjadi bila server menerima data. Namun segmen Riwayat
menampilkan **"Belum ada assesmen awal untuk pasien ini."**

Datanya **tidak hilang**. Pengkajian tersimpan di server dengan benar. Yang salah adalah layarnya:
sejak 16 Oktober 2026, sesudah menyimpan, layar **tidak lagi meminta ulang daftar riwayat** ke
server, sehingga yang ditampilkan masih daftar lama yang diambil sebelum pengkajian dibuat.

Ini **menyimpang** dari desain yang disetujui (`IGD-DEC-133`: "sesudah penyimpanan berhasil … daftarnya
dimuat ulang"), jadi jenisnya `BUG`. Bahaya terbesarnya: perawat mengira penyimpanan gagal lalu
mengisi ulang, sehingga pasien yang sama punya dua dokumen pengkajian awal.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "saat saya mengisi assesment awal di ruang kerja perawat dan berhasil disimpan kenapa seperti ini mana data nyaa ?" | Lampiran 1: tangkapan layar tab **Assesmen Awal IGD**, segmen **Riwayat Dokumen Pengkajian** aktif, kotak kosong "Belum ada assesmen awal untuk pasien ini.", badge jumlah pada segmen Riwayat tidak tampil |

Pelapor juga meminta: "analisa dulu sebelum memperbaiki". Karena itu dokumen ini hanya berisi
analisis; perbaikan dikerjakan setelah rencana disetujui.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-003-01` | 1 | Daftar riwayat tidak dimuat ulang sesudah pengkajian berhasil disimpan | `BUG` (regresi) | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-003-01` |
| `ISS-IGD-003-T1` | — | Keterangan segmen Riwayat memakai istilah teknis "backend" dan menjanjikan hal yang tidak terjadi | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-IGD-003-02` |

---

## 4. Rincian per temuan

### ISS-IGD-003-01 — Daftar riwayat tidak dimuat ulang sesudah pengkajian berhasil disimpan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` (regresi — dulu berfungsi, rusak sejak commit `ae239b5e6`, 16 Oktober 2026) |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend dan backend; gejala DARI-CAPTURE lampiran 1 |
| **Perbaikan** | `FIX-IGD-003-01` |

**Apa yang terjadi.** Saat tab Assesmen Awal IGD dibuka, layar mengambil daftar pengkajian pasien
dari server satu kali. Pada pasien yang belum pernah dikaji, daftar itu kosong. Perawat lalu
mengisi formulir dan menyimpan. Server menyimpan pengkajiannya, layar pindah ke segmen Riwayat —
tetapi layar tidak bertanya lagi ke server, sehingga tetap menampilkan daftar kosong yang diambil
di awal. Pengkajiannya baru terlihat bila perawat pindah tab lalu kembali, atau memuat ulang
halaman (F5).

**Kenapa terjadi.** Rantai sebabnya:

```text
Segmen Riwayat menampilkan "Belum ada assesmen awal untuk pasien ini."
  ← sebab langsung: section.items untuk "assessments" masih daftar lama (kosong);
    setelah simpan berhasil tidak ada yang men-dispatch fetchPatientAssessments
    ← slice hanya menandai assessments.loaded = false, dan tidak ada kode yang
      bereaksi terhadap tanda itu untuk tab ini
      ← akar: commit ae239b5e6 (16-10-2026, implementasi IGD-DEC-311) mengganti
        reload() dengan onVisitUpdated(). onVisitUpdated hanya memuat ulang data
        kunjungan (kartu pasien), bukan daftar pengkajian.
```

1. Fungsi `submit` pada tab ini, setelah server menerima pengkajian, mengosongkan formulir,
   memicu transisi status, memanggil `onVisitUpdated`, lalu memindahkan layar ke Riwayat —
   **tanpa** memuat ulang daftar (SUDAH-VERIFIKASI).

   `QuilvianSystemFrontendDev/src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-initial-tab.jsx:681-712`

   ```jsx
   if (createPatientAssessment.fulfilled.match(result)) {
     setScreeningForm({ ...EMPTY_SCREENING_FORM, /* … */ });
     // IGD-DEC-311 … startEmergencyImmediateCare(…)
     onVisitUpdated?.();
     // Hanya penyimpanan yang berhasil memindahkan layar ke segmen Riwayat.
     setSavedSignal((current) => current + 1);
   }
   ```

   Fungsi `reload` sebenarnya masih ada di berkas yang sama (`:660-663`), tetapi sekarang hanya
   dipakai tombol "Coba lagi" saat daftar gagal dimuat (`:1089`).

2. Commit `ae239b5e6` adalah titik regresinya (SUDAH-VERIFIKASI lewat `git show`):

   ```diff
   -      reload();
   +      // IGD-DEC-311: … startEmergencyImmediateCare …
   +      onVisitUpdated?.();
   ```

   `reload()` sebelumnya ditambahkan pada commit `ffde72514` dan berfungsi.

3. `onVisitUpdated` bukan pengganti `reload`. Pada halaman induk ia diisi `refreshVisit`
   (`emergency-assessment-detail-view.jsx:239`), yang hanya mengambil ulang data kunjungan
   (`use-emergency-assessment-detail.jsx:249-253`). Daftar pengkajian baru diambil ulang bila
   `context`, tab, atau navigasi berubah (`use-emergency-assessment-detail.jsx:130-176`). `context`
   dibentuk dari nilai yang tidak berubah oleh penyimpanan pengkajian — id kunjungan, id pasien,
   id encounter, unit, waktu tiba, keluhan (`:118-128`) — sehingga muatan ulang itu tidak terpicu.

4. Slice Redux menandai `assessments.loaded = false` setelah simpan berhasil
   (`emergency-assessment-slice.jsx:1192-1195`), tetapi tanda itu **tidak dibaca** oleh tab
   Assesmen Awal maupun hook halaman. Pembaca tanda `loaded` hanya ada di tab Tindakan, panel
   Radiologi, dan panel Bank Darah.

5. Tab saudara memakai pola yang benar. Tab SOAP memanggil `onVisitUpdated` **dan** `reload`:

   `emergency-assessment-soap-tab.jsx:274-278`

   ```jsx
   onVisitUpdated?.();
   setNoteDateTime(getCurrentLocalDateTime());
   reload();
   setSavedSignal((current) => current + 1);
   ```

   Tab Observasi (`:542`, `:675`, `:739`), Nosokomial (`:354`), Tindak Lanjut (`:80`), dan Resep
   (`:279`) juga memuat ulang daftarnya sendiri. Hanya tab Assesmen Awal yang tidak.

**Backend tidak bermasalah.** Data yang tersimpan dapat dibaca kembali oleh permintaan daftar yang
sama (SUDAH-VERIFIKASI):

| Langkah | Bukti |
| --- | --- |
| Saat menyimpan, `PatientId` diambil dari encounter, bukan dari isian layar | `PatientAssessmentController.cs:729` |
| Status `Completed` bila "Selesaikan pengkajian saat disimpan" dicentang, `InProgress` bila tidak — keduanya tetap masuk daftar | `PatientAssessmentController.cs:738-740` |
| Daftar disaring `encounterId` dan `patientId` — sama dengan yang dikirim layar | `PatientAssessmentController.cs:355-363`; `emergency-assessment-slice.jsx:373-381` |
| Satu-satunya saringan bawaan adalah "belum dihapus" (`!IsDelete`) | `PatientAssessmentController.cs:3128-3141` |

**Apakah ini menyimpang dari desain?** Ya. `00-interview-decisions.md:4286` (`IGD-DEC-133`,
disetujui Rizki Gunawan 16-09-2026): *"Sesudah penyimpanan berhasil, layar berpindah sendiri ke
segmen Riwayat **dan daftarnya dimuat ulang**."* Layar sekarang hanya melakukan separuh pertama.
`IGD-DEC-311` (`03-frontend-architecture.md:985`) hanya menambahkan pemicu status dan
`onVisitUpdated` untuk menyegarkan kartu pasien; ia **tidak** meminta muatan ulang daftar
dihapus. Jenis: `BUG`.

**Dampak nyata.** Contoh: Ns. Contoh A mengkaji pasien "Tn. Pseudonim B" pukul 09.10. Riwayat
kosong, jadi ia mengira simpan gagal, mengisi ulang, dan menyimpan lagi pukul 09.14. Pasien kini
punya dua dokumen pengkajian awal dengan nomor berbeda. Selain itu:

- badge jumlah pada segmen Riwayat tetap tidak muncul (terlihat pada lampiran 1);
- baris **"Terakhir dikaji"** di atas formulir — yang dibentuk dari daftar yang sama
  (`emergency-assessment-initial-tab.jsx:725-728`) — tidak muncul, sehingga rekan sejawat yang
  membuka formulir tidak tahu pasien baru saja dikaji. Ini bertentangan dengan tujuan
  `IGD-DEC-133`.

**Rekomendasi.** Kembalikan pemanggilan `reload()` setelah simpan berhasil, mengikuti pola tab
SOAP — perubahan satu berkas, tanpa endpoint baru. Rinciannya di plan repair `FIX-IGD-003-01`.

---

## 5. Tanya-jawab pelapor

> **T:** Kenapa seperti ini, mana datanya?
>
> **J:** Datanya sudah tersimpan di server; yang tidak terjadi adalah penyegaran daftar di layar.
> Sejak perubahan 16 Oktober 2026, layar lupa meminta daftar terbaru ke server setelah menyimpan,
> sehingga yang tampil masih daftar lama yang kosong. Untuk membuktikan datanya ada sebelum
> diperbaiki: pindah ke tab **Observasi**, lalu kembali ke **Assesmen Awal IGD** — atau tekan F5.
> Pengkajian yang tadi disimpan akan muncul di Riwayat.

---

## 6. Temuan tambahan

### ISS-IGD-003-T1 — Keterangan segmen Riwayat memakai istilah teknis dan menjanjikan hal yang tidak terjadi

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source; terlihat pada lampiran 1 |
| **Perbaikan** | `FIX-IGD-003-02` |
| **Kenapa dicantumkan** | Terlihat pada lampiran 1 tetapi tidak dikeluhkan; kalimatnya justru menjanjikan perilaku yang rusak pada `ISS-IGD-003-01` |

**Apa yang terjadi.** Di bawah judul "Riwayat Assesmen Awal" tertulis *"Dimuat ulang dari backend
setiap kali penyimpanan berhasil."* Perawat tidak perlu tahu apa itu "backend", dan saat ini
kalimat itu pun tidak benar.

**Kenapa terjadi.** `emergency-assessment-initial-tab.jsx:1069` mengisi prop `description` dengan
kalimat teknis. Daftar periksa UI (`rules/frontend/ui-consistency-checklist.md` bagian E)
mewajibkan label dan pesan dalam Bahasa Indonesia, dan daftar periksa diagnosis mencatat "pesan
teknis mentah" pada layar petugas sebagai pelanggaran.

**Rekomendasi.** Ganti menjadi kalimat petugas, misalnya *"Daftar diperbarui otomatis setiap kali
pengkajian berhasil disimpan."* Dikerjakan bersama `FIX-IGD-003-01`, karena kalimatnya baru benar
setelah perbaikan itu.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Setelah pindah ke tab Observasi lalu kembali ke Assesmen Awal IGD (atau F5), apakah pengkajian yang tadi disimpan muncul? | Konfirmasi lapangan bahwa datanya ada di server dan masalahnya hanya penyegaran layar | Tidak menahan perbaikan; analisis source sudah membuktikannya |
| P-02 | Apakah Anda sempat menyimpan lebih dari sekali karena mengira gagal? | Bila ya, pasien itu punya dokumen pengkajian ganda | Dokumen ganda tetap tercatat dan ikut terbaca pada SOAP dan rekam medis |

Untuk memeriksa P-02 tanpa menebak, pelapor dapat menjalankan query baca-saja berikut
(ganti `<encounter-id>` dengan encounter pasien yang diuji):

```sql
SELECT "AssessmentNumber", "AssessmentStatus", "AssessmentDateTime", "IsDelete"
FROM public."TrxPatientAssessment"
WHERE "EncounterId" = '<encounter-id>'
ORDER BY "AssessmentDateTime" DESC;
```

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bila P-02 terbukti ada dokumen ganda, bagaimana menanganinya? | Batalkan dokumen yang berlebih lewat aksi Batal pengkajian di aplikasi (`PATCH /patient-assessments/{id}/cancel`) / biarkan keduanya | Batalkan lewat aplikasi dengan alasan "terisi ganda karena riwayat tidak tampil" — jejaknya tetap tercatat | Tidak ada; `FIX-IGD-003-01` dan `-02` tetap dapat dikerjakan |

---

## 8. Catatan pola

- **Pola regresi:** satu baris perilaku (`reload()`) dihapus saat menambahkan fitur lain
  (`IGD-DEC-311`) karena `onVisitUpdated` dianggap sudah "menyegarkan semuanya". Pada tab SOAP,
  fitur yang sama dikerjakan dengan benar — `reload()` dipertahankan di samping `onVisitUpdated`.
- **Tanda `loaded = false` yang tidak dibaca siapa pun:** `attachCreate` pada slice menandai
  daftar basi setelah simpan, tetapi sebagian besar tab tidak memakai tanda itu dan memuat ulang
  secara manual. Akibatnya, setiap tab baru harus "ingat" memanggil `reload()`. Pencegahan yang
  diusulkan: tambahkan satu kalimat pada `03-frontend-architecture.md` §17.13.1 butir 1 bahwa
  `onVisitUpdated` hanya menyegarkan data kunjungan dan **tidak menggantikan** muatan ulang daftar
  tab (lihat plan repair bagian 7).
- Issue IGD terdahulu (`issue-001`, `issue-002`) tidak memiliki akar yang sama.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Issue dibuat dari laporan 1 butir + 1 lampiran; 1 temuan pelapor, 1 temuan tambahan | `diagnose-module-issue` |
| 2026-11-04 | Rencana disetujui Rizki Gunawan; status `DALAM_PERBAIKAN`. `FIX-IGD-003-01` dan `-02` sudah diubah di source, menunggu uji browser | Claude Code |
| 2026-11-04 | Uji browser lulus 7/7 (lihat plan repair Riwayat). P-02 terjawab dari database: pengkajian pelapor `ASM-20261104-00001` tunggal, tidak ada dokumen ganda, sehingga K-01 tidak diperlukan | Claude Code |
